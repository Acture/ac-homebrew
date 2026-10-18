# Requires PowerShell 7. Compiles the production GUID declarations only; no game DLLs.
[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Split-Path -Parent $PSScriptRoot),
    [string]$BaselinePath,
    [string]$ReportPath
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$RepositoryRoot = (Resolve-Path -LiteralPath $RepositoryRoot).Path
if (-not $BaselinePath) {
    $BaselinePath = Join-Path $RepositoryRoot 'tests/baselines/published-guids.json'
}
if (-not $ReportPath) {
    $ReportPath = Join-Path $RepositoryRoot 'artifacts/repository-contracts.json'
}

$checks = [System.Collections.Generic.List[object]]::new()
$identifiers = [System.Collections.Generic.Dictionary[string, string]]::new([StringComparer]::Ordinal)

function Invoke-Contract([string]$Name, [scriptblock]$Check) {
    try {
        $detail = (& $Check | Out-String).Trim()
        $checks.Add([pscustomobject]@{ name = $Name; passed = $true; detail = $detail })
        Write-Host "PASS: $Name - $detail"
    }
    catch {
        $detail = $_.Exception.Message
        $checks.Add([pscustomobject]@{ name = $Name; passed = $false; detail = $detail })
        Write-Host "FAIL: $Name - $detail"
    }
}

Invoke-Contract 'Production GUID declarations compile' {
    $types = @(Add-Type -Path (Join-Path $RepositoryRoot 'src/ACHomebrew.Logic/Guids.cs') -PassThru)
    $rootType = $types | Where-Object FullName -eq 'ACHomebrew.Feats.Guids'
    if (-not $rootType) { throw 'The production Guids type was not found.' }
    $pending = [System.Collections.Generic.Stack[Type]]::new()
    $pending.Push($rootType)
    while ($pending.Count -gt 0) {
        $type = $pending.Pop()
        foreach ($nested in $type.GetNestedTypes()) { $pending.Push($nested) }
        foreach ($field in $type.GetFields([Reflection.BindingFlags]'Public, Static, DeclaredOnly')) {
            if ($field.IsLiteral -and $field.FieldType -eq [string]) {
                $symbol = $type.FullName.Replace('ACHomebrew.Feats.', '').Replace('+', '.') + '.' + $field.Name
                $identifiers.Add($symbol, $field.GetRawConstantValue())
            }
        }
    }
    if ($identifiers.Count -eq 0) { throw 'No GUID declarations were discovered.' }
    "$($identifiers.Count) identifiers read from actual C# constants."
}

Invoke-Contract 'GUID values are valid, nonzero and unique' {
    if ($identifiers.Count -eq 0) { throw 'Cannot validate an empty GUID catalog.' }
    $owners = [System.Collections.Generic.Dictionary[Guid, string]]::new()
    foreach ($entry in $identifiers.GetEnumerator()) {
        $id = [Guid]::Empty
        if (-not [Guid]::TryParse($entry.Value, [ref]$id) -or $id -eq [Guid]::Empty) {
            throw "Invalid GUID at $($entry.Key): $($entry.Value)"
        }
        if ($owners.ContainsKey($id)) { throw "Duplicate GUID: $($owners[$id]) and $($entry.Key)" }
        $owners.Add($id, $entry.Key)
    }
    "$($owners.Count) distinct blueprint identifiers."
}

Invoke-Contract 'Published blueprint identities remain stable' {
    $baseline = Get-Content -LiteralPath $BaselinePath -Raw | ConvertFrom-Json -AsHashtable
    if (-not $baseline.ContainsKey('identifiers') -or $baseline.identifiers.Count -eq 0) {
        throw 'The reviewed compatibility baseline is missing or empty.'
    }
    foreach ($entry in $baseline.identifiers.GetEnumerator()) {
        if (-not $identifiers.ContainsKey($entry.Key)) { throw "Published symbol removed: $($entry.Key)" }
        if ([Guid]$entry.Value -eq [Guid]::Empty) { throw "Invalid baseline GUID: $($entry.Key)" }
        if ([Guid]$identifiers[$entry.Key] -ne [Guid]$entry.Value) {
            throw "Published GUID changed: $($entry.Key), expected $($entry.Value), got $($identifiers[$entry.Key])"
        }
    }
    "$($baseline.identifiers.Count) baseline identities preserved; new identifiers are allowed."
}

Invoke-Contract 'Loader metadata matches the project artifact' {
    $info = Get-Content -LiteralPath (Join-Path $RepositoryRoot 'src/ACHomebrew/Info.json') -Raw | ConvertFrom-Json
    [xml]$project = Get-Content -LiteralPath (Join-Path $RepositoryRoot 'src/ACHomebrew/ACHomebrew.csproj') -Raw
    $modId = $project.SelectSingleNode('/Project/PropertyGroup/ModId').InnerText
    if ($info.Id -cne 'ACHomebrew' -or $modId -cne 'ACHomebrew') { throw 'UMM Id and install folder must both be ACHomebrew.' }
    $assemblyName = $project.SelectSingleNode('/Project/PropertyGroup/AssemblyName').InnerText
    $version = $project.SelectSingleNode('/Project/PropertyGroup/Version').InnerText
    if ($info.AssemblyName -cne "$assemblyName.dll") {
        throw "UMM expects $($info.AssemblyName), but the project produces $assemblyName.dll."
    }
    if ($info.Version -cne $version) {
        throw "UMM version $($info.Version) differs from project version $version."
    }
    "$($info.AssemblyName), version $version."
}

$report = [ordered]@{
    scope = 'Repository contracts only; game registration and combat behavior are not tested.'
    identifierCount = $identifiers.Count
    checks = @($checks.ToArray())
    passed = @($checks | Where-Object { -not $_.passed }).Count -eq 0
}
$reportDirectory = Split-Path -Parent ([IO.Path]::GetFullPath($ReportPath))
[void][IO.Directory]::CreateDirectory($reportDirectory)
$report | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $ReportPath -Encoding utf8
if ($env:GITHUB_STEP_SUMMARY) {
    $summary = @('Repository contracts only. Game registration and combat behavior were not tested.', '')
    $summary += $checks | ForEach-Object { '- {0}: {1}' -f $(if ($_.passed) { 'PASS' } else { 'FAIL' }), $_.name }
    $summary | Add-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Encoding utf8
}
if (-not $report.passed) { exit 1 }
