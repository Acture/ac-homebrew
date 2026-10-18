# Invoked by VerifySettings.ps1 after loading real UMM and compiling the actual settings source.
$runDirectory = Join-Path $root ('artifacts/settings-tests/' + [Guid]::NewGuid().ToString('N'))
[void][IO.Directory]::CreateDirectory($runDirectory)
$results = New-Object 'System.Collections.Generic.List[object]'
$historyField = [UnityModManagerNet.UnityModManager+Logger].GetField('history', [Reflection.BindingFlags]'Static,NonPublic')
function Assert-Settings($Condition, [string]$Message) { if (-not $Condition) { throw $Message } }
function New-SettingsCase([string]$Name) {
    $mods = Join-Path (Join-Path $runDirectory $Name) 'Mods'
    $current = Join-Path $mods 'ACHomebrew'; $legacy = Join-Path $mods 'AttributeFeats'
    [void][IO.Directory]::CreateDirectory($current); [void][IO.Directory]::CreateDirectory($legacy)
    $info = New-Object UnityModManagerNet.UnityModManager+ModInfo
    $info.Id = 'ACHomebrew'; $info.Version = '0.2.1'; $info.ManagerVersion = '0.27.11'
    $entry = [UnityModManagerNet.UnityModManager+ModEntry]::new($info, $current + [IO.Path]::DirectorySeparatorChar)
    [pscustomobject]@{ Entry = $entry; Current = (Join-Path $current 'Settings.xml'); Legacy = (Join-Path $legacy 'Settings.xml') }
}
function Write-SettingsXml([string]$Path, [string]$RootName, [int]$Max) { [IO.File]::WriteAllText($Path, "<$RootName><MaxFeatCount>$Max</MaxFeatCount></$RootName>") }
function Assert-NewRoot([string]$Path) { Assert-Settings (([xml][IO.File]::ReadAllText($Path)).DocumentElement.Name -eq 'ACHomebrewSettings') 'Saved XML still uses the retired identity' }
function Invoke-SettingsCase([string]$Name, [scriptblock]$Body) {
    try { & $Body; $results.Add([pscustomobject]@{name=$Name;passed=$true;error=$null}); Write-Host "PASS: $Name" }
    catch { $results.Add([pscustomobject]@{name=$Name;passed=$false;error=$_.Exception.Message}); Write-Host "FAIL: $Name - $($_.Exception.Message)" }
}
Invoke-SettingsCase 'New XML root and type' {
    $writer = [IO.StringWriter]::new(); $serializer.Serialize($writer, (New-Object ACHomebrew.ModSettings))
    Assert-Settings (([xml]$writer.ToString()).DocumentElement.Name -eq 'ACHomebrewSettings') 'XML root still uses the retired identity'
    Assert-Settings ([ACHomebrew.ModSettings].GetCustomAttributes([Xml.Serialization.XmlTypeAttribute], $false)[0].TypeName -eq 'ACHomebrewSettings') 'XML type still uses the retired identity'
}
Invoke-SettingsCase 'No files load defaults and save to new directory' {
    $case = New-SettingsCase 'defaults'; $settings = [ACHomebrew.ModSettings]::LoadCompatible($case.Entry)
    Assert-Settings ($settings.MaxFeatCount -eq 6 -and -not $settings.EnableFeatCountLimit -and $settings.WeaponDamage -eq [ACHomebrew.WeaponDamageMode]::Replace) 'Defaults changed'
    Assert-Settings (-not [IO.File]::Exists($case.Current)) 'Loading wrote a file'
    $settings.Save($case.Entry); Assert-NewRoot $case.Current
    Assert-Settings (-not [IO.File]::Exists($case.Legacy)) 'Save wrote into the legacy directory'
}
Invoke-SettingsCase 'Existing new settings take precedence without writes' {
    $case = New-SettingsCase 'current-first'
    Write-SettingsXml $case.Current 'ACHomebrewSettings' 4; Write-SettingsXml $case.Legacy 'AttributeFeatsSettings' 19
    $before = [IO.File]::ReadAllText($case.Current); $legacyBefore = [IO.File]::ReadAllText($case.Legacy)
    $settings = [ACHomebrew.ModSettings]::LoadCompatible($case.Entry)
    Assert-Settings ($settings.MaxFeatCount -eq 4) 'Legacy settings took precedence'
    Assert-Settings ([IO.File]::ReadAllText($case.Current) -eq $before -and [IO.File]::ReadAllText($case.Legacy) -eq $legacyBefore) 'Load changed a file'
}
Invoke-SettingsCase 'Legacy directory imports values and preserves original' {
    $case = New-SettingsCase 'legacy-import'; Write-SettingsXml $case.Legacy 'AttributeFeatsSettings' 11
    $before = [IO.File]::ReadAllText($case.Legacy); $settings = [ACHomebrew.ModSettings]::LoadCompatible($case.Entry)
    Assert-Settings ($settings.MaxFeatCount -eq 11 -and $settings.ManeuverFreePerRound -eq 1 -and $settings.ManeuverBurstPerCombat -eq 3 -and $settings.ManeuverDurationMultiplier -eq 2) 'Legacy values or missing-field defaults lost'
    Assert-Settings (-not [IO.File]::Exists($case.Current)) 'Read-only import wrote before Save'
    $settings.Save($case.Entry); Assert-NewRoot $case.Current
    Assert-Settings ([IO.File]::ReadAllText($case.Legacy) -eq $before -and ([ACHomebrew.ModSettings]::LoadCompatible($case.Entry)).MaxFeatCount -eq 11) 'Migration changed the original or lost values'
}
Invoke-SettingsCase 'Copied legacy root in new directory upgrades on save' {
    $case = New-SettingsCase 'copied-legacy'; Write-SettingsXml $case.Current 'AttributeFeatsSettings' 8; Write-SettingsXml $case.Legacy 'AttributeFeatsSettings' 22
    $settings = [ACHomebrew.ModSettings]::LoadCompatible($case.Entry); Assert-Settings ($settings.MaxFeatCount -eq 8) 'Copied legacy root did not take precedence'
    $settings.Save($case.Entry); Assert-NewRoot $case.Current
    Assert-Settings (([ACHomebrew.ModSettings]::LoadCompatible($case.Entry)).MaxFeatCount -eq 8) 'Upgrading copied legacy settings lost values'
}
Invoke-SettingsCase 'Damaged new file blocks fallback and automatic save with a diagnostic' {
    $case = New-SettingsCase 'damaged-current'; [IO.File]::WriteAllText($case.Current, '<ACHomebrewSettings><MaxFeatCount>broken')
    Write-SettingsXml $case.Legacy 'AttributeFeatsSettings' 25; $before = [IO.File]::ReadAllText($case.Current); $start = $historyField.GetValue($null).Count
    $settings = [ACHomebrew.ModSettings]::LoadCompatible($case.Entry); Assert-Settings ($settings.MaxFeatCount -eq 6) 'Damaged settings silently fell back to legacy values'
    $settings.MaxFeatCount = 17; $settings.Save($case.Entry); Assert-Settings ([IO.File]::ReadAllText($case.Current) -eq $before) 'Automatic save overwrote damaged settings'
    $messages = @($historyField.GetValue($null) | Select-Object -Skip $start) -join "`n"
    Assert-Settings ($messages.Contains($case.Current) -and $messages -match 'Cannot read.*: .+' -and $messages -match 'saving is disabled') 'Failure did not report its path, reason and disabled saving'
}
Invoke-SettingsCase 'Damaged legacy source stays untouched without a defaults file' {
    $case = New-SettingsCase 'damaged-legacy'; [IO.File]::WriteAllText($case.Legacy, '<AttributeFeatsSettings>'); $before = [IO.File]::ReadAllText($case.Legacy)
    $settings = [ACHomebrew.ModSettings]::LoadCompatible($case.Entry); $settings.Save($case.Entry)
    Assert-Settings (-not [IO.File]::Exists($case.Current) -and [IO.File]::ReadAllText($case.Legacy) -eq $before) 'Failed migration changed settings files'
}
Invoke-SettingsCase 'Unreadable new file does not trigger fallback' {
    $case = New-SettingsCase 'locked-current'; Write-SettingsXml $case.Current 'ACHomebrewSettings' 13; Write-SettingsXml $case.Legacy 'AttributeFeatsSettings' 24
    $before = [IO.File]::ReadAllText($case.Current); $lock = [IO.File]::Open($case.Current, [IO.FileMode]::Open, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
    try { $settings = [ACHomebrew.ModSettings]::LoadCompatible($case.Entry) } finally { $lock.Dispose() }
    Assert-Settings ($settings.MaxFeatCount -eq 6) 'Unreadable settings fell back to legacy'
    $settings.Save($case.Entry); Assert-Settings ([IO.File]::ReadAllText($case.Current) -eq $before) 'Save overwrote unreadable settings'
}
Invoke-SettingsCase 'Migration does not overwrite a destination created after load' {
    $case = New-SettingsCase 'migration-race'; Write-SettingsXml $case.Legacy 'AttributeFeatsSettings' 18
    $settings = [ACHomebrew.ModSettings]::LoadCompatible($case.Entry); Write-SettingsXml $case.Current 'ACHomebrewSettings' 3; $before = [IO.File]::ReadAllText($case.Current)
    $settings.Save($case.Entry); Assert-Settings ([IO.File]::ReadAllText($case.Current) -eq $before) 'Migration overwrote a new destination file'
}
Invoke-SettingsCase 'Serialization failure preserves the last valid settings file' {
    $case = New-SettingsCase 'save-failure'; Write-SettingsXml $case.Current 'ACHomebrewSettings' 7; $before = [IO.File]::ReadAllText($case.Current)
    $settings = [ACHomebrew.ModSettings]::LoadCompatible($case.Entry)
    [ACHomebrew.ModSettings].GetField('WeaponDamage').SetValue($settings, [Enum]::ToObject([ACHomebrew.WeaponDamageMode], 999))
    $settings.Save($case.Entry); Assert-Settings ([IO.File]::ReadAllText($case.Current) -eq $before) 'Failed serialization damaged the original'
    Assert-Settings (@([IO.Directory]::GetFiles((Split-Path $case.Current -Parent), '*.tmp')).Count -eq 0) 'Failed serialization left a temporary file'
}
$reportPath = Join-Path $runDirectory 'results.json'
$results | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $reportPath -Encoding UTF8
$failed = @($results | Where-Object { -not $_.passed })
Write-Host "Settings migration results: $($results.Count - $failed.Count)/$($results.Count) passed; $reportPath"
if ($failed.Count -gt 0) { throw "$($failed.Count) settings migration checks failed." }
