# Requires PowerShell 7 on Windows. Runs the AC's Homebrew offline mechanics tests with the runner from the
# WotR.Testing.Offline NuGet package (https://github.com/Acture/wotr-testing).
[CmdletBinding()]
param(
    # Game root: a local installation or a snapshot. Defaults to vendor/wotr, then GamePath.props.
    [string]$WotrInputRoot,
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Debug',
    [int]$TimeoutMinutes = 15,
    # Optional dotnet test filter, e.g. "FullyQualifiedName~MainAttribute".
    [string]$Filter,
    # The 0Harmony.dll the run loads: umm (default), game (the game's own 2.0.4), a file or a Lib.Harmony version.
    [string]$Harmony
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'tests/OfflineMechanics.Tests/OfflineMechanics.Tests.csproj'
# The runner is in the package's tools/ folder; restore first so the project reports it.
& dotnet restore $project -nologo -v quiet
if ($LASTEXITCODE -ne 0) { throw '[ENV_MISSING] Restoring the offline test project failed.' }
$tools = "$(& dotnet msbuild $project -getProperty:WotrToolsDirectory -nologo)".Trim()
$runner = Join-Path $tools 'Invoke-WotrOfflineTests.ps1'
if (-not $tools -or -not (Test-Path -LiteralPath $runner)) { throw "[ENV_MISSING] WotR.Testing.Offline runner not found: $runner" }
$arguments = @{
    Project = $project
    RepositoryRoot = $root
    ResultsDirectory = Join-Path $root 'artifacts/test-results/offline-mechanics'
    Configuration = $Configuration
    TimeoutMinutes = $TimeoutMinutes
}
if ($WotrInputRoot) { $arguments.WotrInputRoot = $WotrInputRoot }
if ($Filter) { $arguments.Filter = $Filter }
if ($Harmony) { $arguments.Harmony = $Harmony }
& $runner @arguments
exit $LASTEXITCODE
