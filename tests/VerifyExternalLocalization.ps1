param(
    [Parameter(Mandatory = $true)][string]$WrathInstallDir,
    [string]$ModAssembly
)

# Windows PowerShell 5.1; exercises the merged release DLL without starting Unity.
$ErrorActionPreference = 'Stop'
if (!$ModAssembly) { $ModAssembly = Join-Path (Split-Path $PSScriptRoot -Parent) 'artifacts\bin\ACHomebrew\Release\ACHomebrew.dll' }
$ModAssembly = (Resolve-Path -LiteralPath $ModAssembly).Path
$managed = Join-Path $WrathInstallDir 'Wrath_Data\Managed'
[void][Reflection.Assembly]::LoadFrom((Join-Path $managed 'Assembly-CSharp.dll'))
foreach ($dependency in @('Newtonsoft.Json.dll', 'Owlcat.Runtime.Core.dll', 'UnityEngine.CoreModule.dll',
    'UnityModManager\UnityModManager.dll', 'UnityModManager\0Harmony.dll')) {
    [void][Reflection.Assembly]::LoadFrom((Join-Path $managed $dependency))
}
$mod = [Reflection.Assembly]::LoadFrom($ModAssembly)
$flags = [Reflection.BindingFlags]'Public,NonPublic,Static,Instance'
$catalog = $mod.GetType('ACHomebrew.Feats.FeatTextCatalog', $true)
$common = $mod.GetType('ACHomebrew.Feats.Common', $true)
$messages = New-Object 'System.Collections.Generic.List[string]'
$logger = [Action[string]]{ param($message) $messages.Add($message) }
$temporary = Join-Path ([IO.Path]::GetTempPath()) ('ACHomebrew-release-localization-' + [Guid]::NewGuid().ToString('N'))

function Assert-Equal([string]$Expected, [string]$Actual) {
    if ($Expected -cne $Actual) { throw "Expected '$Expected', got '$Actual'." }
}

try {
    $languageDirectory = Join-Path $temporary 'Localization'
    [void][IO.Directory]::CreateDirectory($languageDirectory)
    # Edit one non-English language (German) and keep every other delivered file unchanged.
    $delivered = Join-Path (Split-Path $ModAssembly -Parent) 'Localization'
    Get-ChildItem -LiteralPath $delivered -Filter '*.json' | Copy-Item -Destination $languageDirectory
    $get = $catalog.GetMethod('Get', $flags)
    $german = [Enum]::Parse($get.GetParameters()[2].ParameterType, 'deDE')
    $translations = Get-Content -LiteralPath (Join-Path $delivered 'deDE.json') -Raw -Encoding UTF8 | ConvertFrom-Json
    $translations.'MainAttr_Str.Name' = 'External release translation'
    $translations.'Text.FeatBuilders.Desc.Text' = $translations.'Text.FeatBuilders.Desc.Text'.Replace('{1}', 'External rule: {1}')
    $translations | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $languageDirectory 'deDE.json') -Encoding UTF8

    [void]$catalog.GetMethod('Initialize', $flags).Invoke($null, [object[]]@([string]$temporary, $logger))
    if ($messages.Count) { throw ($messages -join "`n") }
    Assert-Equal 'External release translation' ($get.Invoke($null, [object[]]@('MainAttr_Str.Name', 'fallback', $german)))

    $arguments = New-Object object[] 4
    $arguments[0] = 'Text.FeatBuilders.Desc.Text'
    $arguments[1] = 'fallback'
    $arguments[2] = $german
    $arguments[3] = [object[]]@('Family', '+1 damage')
    $description = $common.GetMethod('Format', $flags).Invoke($null, $arguments)
    Assert-Equal ([string]::Format($translations.'Text.FeatBuilders.Desc.Text', 'Family', '+1 damage')) $description
    Set-Content -LiteralPath (Join-Path $languageDirectory 'deDE.json') -Value '{' -Encoding UTF8
    [void]$catalog.GetMethod('Initialize', $flags).Invoke($null, [object[]]@([string]$temporary, $logger))
    if (!($messages | Where-Object { $_ -like '*cannot load localization*' })) { throw 'Missing malformed-file diagnostic.' }
    $fallback = $get.Invoke($null, [object[]]@('MainAttr_Str.Name', 'fallback', $german))
    $embedded = Get-Content -LiteralPath (Join-Path $delivered 'deDE.json') -Raw -Encoding UTF8 | ConvertFrom-Json
    Assert-Equal $embedded.'MainAttr_Str.Name' $fallback
    'PASS: the merged release DLL reads edited external names and formats edited rule templates, and logs malformed-file fallback, without recompilation or starting Unity. Registration/refresh are covered by production-source tests with API doubles; this check does not execute Unity localization APIs.'
}
finally {
    if ([IO.Directory]::Exists($temporary)) { [IO.Directory]::Delete($temporary, $true) }
}
