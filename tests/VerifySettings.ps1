param([Parameter(Mandatory = $true)][string]$WrathInstallDir)

# Run with Windows PowerShell 5.1 (.NET Framework), matching the mod's runtime.
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$umm = Join-Path $WrathInstallDir 'Wrath_Data/Managed/UnityModManager/UnityModManager.dll'
[void][System.Reflection.Assembly]::LoadFrom((Join-Path $WrathInstallDir 'Wrath_Data/Managed/UnityEngine.CoreModule.dll'))
[void][System.Reflection.Assembly]::LoadFrom((Join-Path $WrathInstallDir 'Wrath_Data/Managed/UnityEngine.dll'))
[void][System.Reflection.Assembly]::LoadFrom($umm)
Add-Type -Path @((Join-Path $root 'src/ACHomebrew.Core/Settings.cs'), (Join-Path $root 'src/ACHomebrew.Logic/WeaponDamageRules.cs')) -ReferencedAssemblies @($umm, 'System.Xml.dll')

$serializer = New-Object System.Xml.Serialization.XmlSerializer([ACHomebrew.ModSettings])
$legacySerializer = [System.Xml.Serialization.XmlSerializer]::new([ACHomebrew.ModSettings], [System.Xml.Serialization.XmlRootAttribute]::new('AttributeFeatsSettings'))
$old = $legacySerializer.Deserialize([System.IO.StringReader]::new('<AttributeFeatsSettings><EnableMutex>false</EnableMutex><EnablePowerMode>true</EnablePowerMode></AttributeFeatsSettings>'))
if ($old.WeaponDamage -ne [ACHomebrew.WeaponDamageMode]::Replace -or $old.EnableMutex -ne $false -or $old.EnablePowerMode -ne $true) { throw 'Old settings compatibility failed' }
if ($old.EnableFeatCountLimit -or $old.EnableFeatPointLimit -or $old.MaxFeatCount -ne 6 -or $old.MaxFeatPoints -ne 10) { throw 'Old settings did not default to an unlimited feat budget' }
if ($old.FeatGroups -eq $null -or $old.FeatGroups.Count -ne 0) { throw 'Old settings did not default to built-in exclusion groups' }
if ($old.ManeuverFreePerRound -ne 1 -or $old.ManeuverBurstPerCombat -ne 3 -or $old.ManeuverFreePerRoundCap -ne 2 -or $old.ManeuverDurationMultiplier -ne 2 -or $old.ManeuverMinimumExtraRounds -ne 2) { throw 'Old settings did not receive maneuver defaults' }
$old.ManeuverFreePerRound = 2; $old.ManeuverBurstPerCombat = 5; $old.ManeuverFreePerRoundCap = 3
$old.ManeuverDurationMultiplier = 2.5; $old.ManeuverMinimumExtraRounds = 1
$old.EnableFeatCountLimit = $true; $old.MaxFeatCount = 4; $old.EnableFeatPointLimit = $true; $old.MaxFeatPoints = 7
$group = New-Object ACHomebrew.FeatGroupSetting; $group.Id = 'Conditional'; $group.Enabled = $false; $group.Max = 3; $old.FeatGroups.Add($group)
foreach ($mode in @([ACHomebrew.WeaponDamageMode]::Replace, [ACHomebrew.WeaponDamageMode]::Add)) {
    $old.WeaponDamage = $mode
    $writer = [System.IO.StringWriter]::new()
    $serializer.Serialize($writer, $old)
    $loaded = $serializer.Deserialize([System.IO.StringReader]::new($writer.ToString()))
    if ($loaded.ManeuverFreePerRound -ne 2 -or $loaded.ManeuverBurstPerCombat -ne 5 -or $loaded.ManeuverFreePerRoundCap -ne 3 -or $loaded.ManeuverDurationMultiplier -ne 2.5 -or $loaded.ManeuverMinimumExtraRounds -ne 1) { throw 'Maneuver settings roundtrip failed' }
    if ($loaded.WeaponDamage -ne $mode -or $loaded.EnableMutex -ne $false -or $loaded.EnablePowerMode -ne $true) { throw "Roundtrip failed for $mode" }
    if ($loaded.FeatGroups.Count -ne 1 -or $loaded.FeatGroups[0].Id -ne 'Conditional' -or $loaded.FeatGroups[0].Enabled -or $loaded.FeatGroups[0].Max -ne 3) { throw "Exclusion group roundtrip failed for $mode" }
    if (-not $loaded.EnableFeatCountLimit -or $loaded.MaxFeatCount -ne 4 -or -not $loaded.EnableFeatPointLimit -or $loaded.MaxFeatPoints -ne 7) { throw "Feat budget roundtrip failed for $mode" }
}
. (Join-Path $PSScriptRoot 'VerifySettingsMigration.ps1')
'PASS: settings identity, migration, file preservation, defaults, numeric options, damage modes, budget limits and exclusion groups.'
