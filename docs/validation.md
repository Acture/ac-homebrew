# Validation

Run these checks from the repository root. GitHub Actions runs the first ten
commands on Windows and Linux.

```powershell
pwsh -NoProfile -File scripts/Test-RepositoryContracts.ps1
dotnet run --project tests/WeaponDamage.Tests --configuration Release
dotnet run --project tests/FeatBudget.Tests --configuration Release
dotnet run --project tests/Initialization.Tests --configuration Release
dotnet run --project tests/FeatText --configuration Release
dotnet run --project tests/AssemblyInspector.Tests --configuration Release
python scripts/validate_assets.py
python scripts/test_release_delivery.py
python scripts/test_nexus_release.py
python scripts/test_draft_release.py
```

Use PowerShell 7, the .NET 10 SDK and Python 3.10 or newer. Localization tests
declare Newtonsoft.Json as a NuGet dependency; they need no game assemblies.
They compile the production text loader and registration/refresh methods with
an in-memory replacement for the external localization APIs. They check every
resource entry in all eight language files (same keys, compatible placeholders,
markup and links), every menu, weapon and budget key, a refresh round trip
through every locale, per-locale template composition, localized list joining,
runtime budget text, English fallback for unsupported locales, compiled fallback
and encyclopedia tagging calls. They edit external language files through the
production loader and assert that edited names and rule templates reach
localized strings without recompilation; missing, malformed, duplicate,
blank and structurally invalid translations have fallback diagnostics.
Roslyn source checks cover literal resource references in every language,
English compiled-fallback agreement, and reject translated (CJK) literals or
`Locale.zhCN` special cases in source: translations live only in the language files.

The [assembly inspector](type-inspection.md) checks type and member metadata
without executing target code. Its fixture controls cover hazardous initializers,
constructors, getters, methods and attribute constructors; dependency errors
must fail without partial output. These checks do not establish runtime behavior.

Feat budget checks run the production budget rules without game assemblies:
exactly full and exceeded count/point limits, both limits together, cross-family
pooling, parametrized choices and ranks, over-budget characters after a lowered
limit, consecutive picks in one level-up, and cancelled or changed picks under
Wrath's replay of pending choices. Initialization checks confirm that only leaf
feats are budgeted, menus are not, and rolled-back registrations are excluded.
These are rule tests. They do not prove the game's level-up and respec screens,
tooltips or saved characters behave the same way.

The repository contract checks preserve the 133 published blueprint identifiers
and allow new unique IDs. Asset checks compare the existing families' component
calls and inline IDs with a committed baseline extracted from v0.1.1, then check
every PNG mapping (152 at the time of writing), hash, dimension and catalog name. These are static checks of
identity and component configuration, not proof of combat behavior.

With a local Wrath installation configured in the ignored root `GamePath.props`:

```powershell
dotnet build ACHomebrew.slnx -p:DeployMod=false
powershell -NoProfile -File tests/VerifySettings.ps1 -WrathInstallDir "<game directory>"
powershell -NoProfile -File tests/VerifyLiveWeaponDamageMode.ps1 -WrathInstallDir "<game directory>"
powershell -NoProfile -File tests/VerifyHarmonyTargets.ps1 -WrathInstallDir "<game directory>"
dotnet build ACHomebrew.slnx -c Release -p:DeployToGame=false
powershell.exe -NoProfile -File tests\VerifyExternalLocalization.ps1 -WrathInstallDir "<game directory>"
python scripts/validate_assets.py --release artifacts/packages/ACHomebrew-0.2.1.zip
pwsh -NoProfile -File scripts/Invoke-OfflineMechanicsTests.ps1 -WotrInputRoot "<game directory>"
pwsh -NoProfile -File scripts/Invoke-OfflineMechanicsTests.ps1 -WotrInputRoot "<game directory>" -Harmony game
```

The offline mechanics tests (the `WotR.Testing.Offline` package) load the mod
into a real offline game session. Besides feat mechanics,
`LocalizationTests` checks every string the mod registers in all eight locales:
nonblank, no Chinese outside `zhCN`, and longer texts translated rather than
copied from English. The multi-locale refactor was checked against a dump of all
520 registered strings taken before it: English and Chinese were byte-identical.

The reflection checks run in Windows PowerShell 5.1 against installed
assemblies without starting the game. VerifyHarmonyTargets resolves every
Harmony patch target in the built mod against the game assembly. They check settings serialization, including feat budget defaults, and
whether an existing weapon-damage component reads changed mode settings.
VerifyExternalLocalization edits temporary copies of the delivered language files
and runs the **merged release DLL's** loader and formatter, checking external
precedence and malformed-file diagnostics without rebuilding or touching the
installed mod. It does not invoke Unity localization APIs; registration and
refresh are covered separately by the production-source tests with API doubles.
The package check verifies loader metadata and version, icon coverage and bytes,
both player guides and changelogs, both license files and all eight canonical
language files. It rejects missing delivery files, invalid language resources,
different package/source bytes, dependency/game DLLs and contributor directories.
CI runs positive and negative delivery fixtures without building against licensed
game files. See [localization and release delivery](localization-and-release.md)
for the single-artifact GitHub/Nexus publishing procedure.
The [manual Nexus workflow](nexus-publishing.md) has separate offline controls
for release metadata, digest/source matching, upload preparation and required
reviewers. It does not publish during CI. A reviewed live upload and independent
download comparison are still needed to verify account permissions and delivery.

## Offline mechanics with the real game files

```powershell
pwsh -NoProfile -File scripts/Invoke-OfflineMechanicsTests.ps1
```

This Windows-only check uses the public `WotR.Testing.Offline` NuGet package,
restored with the test project. It loads the installed
(or `vendor/wotr/` snapshot) game
assemblies, vanilla blueprints and settings in a test process on the game's own
Unity Mono runtime without starting the game. The default run uses
UnityModManager's Harmony 2.3.6 and warns about Harmony members missing from the
game's own 2.0.4; `-Harmony game` runs on 2.0.4 itself. It applies Main Attribute Mastery and the retired
Titan's Apotheosis to a vanilla unit and removes them, runs a failure control and probes Bloodline of Beasts through the
game's summon rule. Reports are written to `artifacts/test-results/offline-mechanics/`.
See [offline-mechanics-testing.md](offline-mechanics-testing.md) for the inputs,
every environment adaptation, result classes and limits. Public CI does not run
it because it needs licensed game files.

The checks above do not execute attacks in Wrath, prove all feats work, exercise
the real Unity locale-change hook, or verify the game's font/layout behavior.
Those remain game integration checks. Keep their results separate from CI.

Feat budget game checks require a real Wrath session; a missing game, zero
cases or failed initialization is not a pass. With each limit mode enabled,
confirm that level-up and respec screens agree with the character's final feats;
picks in one level-up, cancelling and changing picks, Weapon Damage categories
and Trickster prerequisite skipping respect the budget; the tooltip and settings
usage match; reloading a save keeps the same usage; and lowering a limit marks
the character over budget without removing feats. Also check that vanilla and
other mods' feats are not counted.

To refresh public asset documentation after an intentional name or icon change:

```powershell
python scripts/update_catalog_docs.py
python scripts/update_icon_manifest.py
powershell -NoProfile -File scripts/icon-contact-sheet.ps1
```

The contact-sheet command requires Windows/System.Drawing.
