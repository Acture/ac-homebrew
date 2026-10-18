param(
    [Parameter(Mandatory = $true)][string]$WrathInstallDir,
    [string]$ModAssembly
)

# Run with Windows PowerShell 5.1 against the compiled mod and the installed game.
# Resolves every Harmony patch target without applying patches or starting the game.
$ErrorActionPreference = 'Stop'
if (!$ModAssembly) { $ModAssembly = Join-Path (Split-Path $PSScriptRoot -Parent) 'artifacts/bin/ACHomebrew/Debug/ACHomebrew.dll' }
$managed = Join-Path $WrathInstallDir 'Wrath_Data/Managed'
foreach ($dependency in @('Assembly-CSharp.dll', 'Newtonsoft.Json.dll', 'UnityModManager/UnityModManager.dll', 'UnityModManager/0Harmony.dll')) {
    [void][System.Reflection.Assembly]::LoadFrom((Join-Path $managed $dependency))
}
$modMenu = Join-Path $WrathInstallDir 'Mods/ModMenu/ModMenu.dll'
if (Test-Path $modMenu) { [void][System.Reflection.Assembly]::LoadFrom($modMenu) }
$mod = [System.Reflection.Assembly]::LoadFrom((Resolve-Path $ModAssembly))

$flags = [System.Reflection.BindingFlags]'Public,NonPublic,Static,Instance,DeclaredOnly'
$types = try { $mod.GetTypes() } catch [System.Reflection.ReflectionTypeLoadException] { $_.Exception.Types | Where-Object { $_ } }
$checked = 0
$failures = @()
foreach ($type in $types) {
    if ($type.Namespace -notlike 'ACHomebrew*') { continue }
    $attributes = @($type.GetCustomAttributes($true) | Where-Object { $_.GetType().FullName -eq 'HarmonyLib.HarmonyPatch' })
    if ($attributes.Count -eq 0) { continue }

    $targetMethods = $type.GetMethod('TargetMethods', $flags)
    if ($targetMethods) {
        $methods = @($targetMethods.Invoke($null, $null))
        if ($methods.Count -eq 0) { $failures += "$($type.FullName): TargetMethods returned nothing" }
        $checked += $methods.Count
        continue
    }

    $targetMethod = $type.GetMethod('TargetMethod', $flags)
    if ($targetMethod) {
        $method = $targetMethod.Invoke($null, $null)
        if ($method -isnot [System.Reflection.MethodBase]) { $failures += "$($type.FullName): TargetMethod did not return a method or constructor" }
        $checked++
        continue
    }

    # Combine class-level patch attributes into one target, as Harmony does.
    $declaring = $null; $name = $null; $arguments = $null; $kind = 'Normal'
    # Method-level [HarmonyPatch(...)] attributes complete the class-level declaration.
    $methodAttributes = @($type.GetMethods($flags) | ForEach-Object { $_.GetCustomAttributes($true) } | Where-Object { $_.GetType().FullName -eq 'HarmonyLib.HarmonyPatch' })
    foreach ($attribute in $attributes + $methodAttributes) {
        $info = $attribute.info
        if ($info.declaringType) { $declaring = $info.declaringType }
        if ($info.methodName) { $name = $info.methodName }
        if ($null -ne $info.argumentTypes) { $arguments = $info.argumentTypes }
        if ($null -ne $info.methodType) { $kind = $info.methodType.ToString() }
    }
    if (!$declaring -or !$name) { $failures += "$($type.FullName): incomplete patch declaration"; continue }
    if ($kind -eq 'Getter') { $name = 'get_' + $name }
    elseif ($kind -eq 'Setter') { $name = 'set_' + $name }
    elseif ($kind -ne 'Normal') { $failures += "$($type.FullName): unsupported method kind $kind"; continue }
    $candidates = @($declaring.GetMethods($flags) | Where-Object { $_.Name -eq $name })
    if ($null -ne $arguments) {
        $candidates = @($candidates | Where-Object {
            $parameters = $_.GetParameters()
            $parameters.Count -eq $arguments.Count -and ($parameters.Count -eq 0 -or (0..($parameters.Count - 1) | Where-Object { $parameters[$_].ParameterType -ne $arguments[$_] }).Count -eq 0)
        })
    }
    if ($candidates.Count -ne 1) { $failures += "$($type.FullName): $($declaring.Name).$name matched $($candidates.Count) methods" }
    $checked++
}
if ($failures.Count) { throw ("Unresolved Harmony targets:`n" + ($failures -join "`n")) }
if ($checked -eq 0) { throw 'No Harmony patches found in the mod assembly' }
"PASS: $checked Harmony patch targets resolve against the installed game (static check; patches are not applied)."
