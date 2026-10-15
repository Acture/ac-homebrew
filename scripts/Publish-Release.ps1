# Requires PowerShell 7, gh (authenticated) and local game references (GamePath.props).
# Builds and checks the release ZIP, pushes tag vX.Y.Z, waits for the draft GitHub Release that the
# tag workflow creates, and uploads the ZIP to it. Publishing the draft stays a manual, reviewed step;
# publishing then starts the Nexus workflow, which waits for environment approval.
[CmdletBinding()]
param(
    # Upload an already built ZIP instead of building it again.
    [switch]$SkipBuild,
    [int]$DraftTimeoutMinutes = 10
)

$ErrorActionPreference = 'Stop'
function Invoke-Checked {
    & $args[0] @($args | Select-Object -Skip 1)
    if ($LASTEXITCODE -ne 0) { throw "Failed ($LASTEXITCODE): $($args -join ' ')" }
}
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
    $version = (Get-Content -LiteralPath 'src/ACHomebrew/Info.json' -Raw | ConvertFrom-Json).Version
    $tag = "v$version"
    $zip = "artifacts/packages/ACHomebrew-$version.zip"

    $branch = git rev-parse --abbrev-ref HEAD
    $defaultBranch = (gh repo view --json defaultBranchRef --jq .defaultBranchRef.name)
    if ($branch -ne $defaultBranch) { throw "Release from $defaultBranch, not $branch." }
    if (git status --porcelain --untracked-files=no) { throw 'Commit or discard tracked changes first.' }
    Invoke-Checked git fetch --quiet origin $defaultBranch
    if ((git rev-parse HEAD) -ne (git rev-parse "origin/$defaultBranch")) { throw "Local $defaultBranch differs from origin." }

    if (-not $SkipBuild) {
        Invoke-Checked dotnet build ACHomebrew.slnx -c Release -p:DeployToGame=false
    }
    Invoke-Checked python scripts/validate_assets.py --release $zip
    $hash = (Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash.ToLowerInvariant()

    $tagCommit = git rev-parse -q --verify "refs/tags/$tag^{commit}" 2>$null
    if (-not $tagCommit) {
        Invoke-Checked git tag -a $tag -m "AC's Homebrew $version"
    } elseif ($tagCommit -ne (git rev-parse HEAD)) {
        throw "Tag $tag already exists on another commit."
    }
    Invoke-Checked git push origin "refs/tags/$tag"

    Write-Host "Waiting for the draft release $tag..."
    $deadline = (Get-Date).AddMinutes($DraftTimeoutMinutes)
    do {
        Start-Sleep -Seconds 10
        $release = gh release view $tag --json isDraft,assets 2>$null | ConvertFrom-Json
    } until ($release -or (Get-Date) -gt $deadline)
    if (-not $release) { throw "No release for $tag after $DraftTimeoutMinutes minutes; check the 'Draft release from version tag' run." }
    if (-not $release.isDraft) { throw "Release $tag is already published; not changing its assets." }
    if ($release.assets.name -contains (Split-Path $zip -Leaf)) { throw "The draft already has $(Split-Path $zip -Leaf)." }

    Invoke-Checked gh release upload $tag $zip
    $url = gh release view $tag --json url --jq .url
    Write-Host "Uploaded $zip (SHA-256 $hash) to the draft: $url"
    Write-Host 'Review the notes, then publish. Publishing starts the Nexus workflow (approval required).'
}
finally {
    Pop-Location
}
