# Offline mechanics tests

`tests/OfflineMechanics.Tests` runs AC's Homebrew (UMM Id `ACHomebrew`) against the real Pathfinder:
Wrath of the Righteous assemblies, vanilla blueprint pack and settings in a test
process on the game's own Unity Mono runtime. It does not start `Wrath.exe`, the
Unity player or the Unity Editor, and it does not touch saves or the installed
`Mods` folder.

The game-independent part is
[wotr-testing](https://github.com/Acture/wotr-testing), used as the public NuGet
package `WotR.Testing.Offline` (version: the `PackageReference` in the test project). Its
[documentation](https://github.com/Acture/wotr-testing/blob/main/docs/offline.md)
describes how the game runs without Unity, every environment adaptation, the
result classes and the general limits. This page covers the mod's tests and what they found.

## Setup

- Windows, PowerShell 7 and the .NET 10 SDK. The test library is restored from
  NuGet with the test project; the run script finds its runner there.
- One copy of the game files: `-WotrInputRoot`, the `WOTR_INPUT_ROOT` variable, a
  snapshot in `vendor/wotr/`, or `WrathInstallDir` from the local `GamePath.props`,
  in that order. To create a snapshot with the package's script:

  ```powershell
  dotnet restore tests/OfflineMechanics.Tests
  $tools = "$(dotnet msbuild tests/OfflineMechanics.Tests -getProperty:WotrToolsDirectory)".Trim()
  pwsh -NoProfile -File "${tools}New-WotrSnapshot.ps1" -WrathInstallDir "<game directory>" -Destination vendor/wotr
  ```

  `vendor/wotr/` is ignored by Git. These are licensed game files: never commit,
  package or upload them. The Mono runtime comes from the game's `MonoBleedingEdge`
  folder; recreate snapshots made without it.
- A checkout path of about 90 characters or less. The game's Mono does not support
  paths over 260 characters, and the run's folder adds about 170 to the checkout path.

## Running

```powershell
pwsh -NoProfile -File scripts/Invoke-OfflineMechanicsTests.ps1
pwsh -NoProfile -File scripts/Invoke-OfflineMechanicsTests.ps1 -WotrInputRoot vendor/wotr -Filter "FullyQualifiedName~MainAttribute"
pwsh -NoProfile -File scripts/Invoke-OfflineMechanicsTests.ps1 -Harmony game
pwsh -NoProfile -File scripts/Invoke-OfflineMechanicsTests.ps1 -Filter "FullyQualifiedName~Maneuver"
```

The mod is built from this checkout without deploying it, against the same game
inputs. Reports go to `artifacts/test-results/offline-mechanics/`: `wotr-offline.xunit.xml`,
`environment.json`, `summary.json`/`summary.md` and the captured logs in `logs/`. The run fails on any failed,
errored or unexplained skipped test, on zero tests, missing reports or a timeout.
Paths that need the Unity engine are reported as `unity-runtime-required`, never
as passed.

Besides the game startup stages, the test project fails initialization unless the
mod logs "ACHomebrew: registry initialized.".

Game-derived test components live in `tests/OfflineMechanics.TestSupport`.
The xUnit assembly refers to them inside test methods, after `RequireGame()`
installs the game assembly resolver. Keeping them out of xUnit's discovery
reflection avoids loading game dependencies before the fixture is ready.

### Harmony copies

A player's game has more than one `0Harmony.dll`: the game ships Harmony 2.0.4 in
`Wrath_Data/Managed`, and UnityModManager 0.32.4 ships 2.3.6. The mod uses whichever
copy loads first. Runs use UnityModManager's copy unless `-Harmony` names another
(`game`, a file or a `Lib.Harmony` version). `environment.json` (`harmony`) compares the
mod with the game's 2.0.4 and warns about members that copy lacks; warnings do not fail
the run. A `-Harmony game` run fails at `mod-load` when the mod calls such a member.
0.2.0 failed to load in some games this way (`CodeInstruction.LoadArgument`), so run
both before a release.

## Current coverage

| Test | Shows |
|---|---|
| Environment: inputs and build | Game, Unity and mod assemblies load from the prepared folder; the mod is this build |
| Environment: no game process | No `Wrath`/Unity process starts; `UnityPlayer.dll` and `Wrath.exe` are not loaded, and Unity's Mono runtime only from the game inputs |
| Environment: method bodies | Rewritten `Assembly-CSharp`, `Assembly-CSharp-firstpass` and `Owlcat.Runtime.Core` keep every method body |
| Environment: mod initialization | `Main.Load` and the `BlueprintsCache.Init` postfix register all families and menus |
| Main Attribute Mastery (Strength → Dexterity) | On a vanilla human pregen, the Strength modifier (minimum 0) appears as one inherent bonus on Dexterity and nowhere else; removal restores every stat |
| Retired Titan's Apotheosis | Half the Strength modifier (rounded down) appears as an inherent bonus on the other five attributes and nowhere else; removal restores every stat |
| Failure control | The same assertion fails when the mastery's stat component is removed |
| Summoning probe (Bloodline of Beasts) | Runs the game's `RuleSummonUnit` with a pre-made vanilla unit: the summoner keeps only the trigger buff; the summon gets the bonus buff, with the summoner as caster and the summoner's Strength modifier as the value |
| Seize the Opening | Real ability commands share per-round and per-encounter allowances; queries, cancellation and invalid execution do not spend them; resources and failed checks still count; exhausted allowances restore the original action cost |
| Action allowance persistence | The game's JSON serializer retains the unit part, including after combat-state reconstruction; feat removal and reapplication do not refill it |
| Lingering Disruption | Actual maneuver rules and buffs check fresh timed effects, failure, reapplication, nested rules and scope cleanup; unrelated, permanent and prone effects keep their duration |
| Third-party maneuver recognition | Synthetic physical abilities exercise native/custom maneuver actions, conditionals, variants and explicit registration; spells, weapon attacks and unknown overrides are excluded |
| Duel blueprints and uses | The Duel menu, prerequisites, the mythic feat in the vanilla mythic selection, the challenge tracker on the vanilla challenge effect; 1 + level/5 daily uses, +2 with Endless Challenge |
| Duel ability | A real ability command against a Dretch spends one use and links both duelists; allies, self and fallen creatures are invalid targets; the encounter limit lifts only on a real leave-combat transition |
| Duel compulsion and targets | Both duelists get the attack-nearest compulsion aimed at the opponent and lose direct control; neither can attack a bystander; ending one side ends both and returns control; Unbound Duelist keeps its owner uncompelled but restricted |
| Duel damage | Damage rules: duelists deal 0 to anyone but the opponent; Single Combat halves an interloper's 10 to 5, also during a challenge; without the duel everything is unchanged |
| Duel victories | A falling opponent gives the survivor one permanent +1 additional damage, the reverse when the owner falls, nothing for an expired or simultaneous duel or a repeated event; victories serialize and are excluded from respec carry-over |
| Challenges as duels | The vanilla Cavalier's Challenge effect counts only for an Oath owner, is settled once across a revival, spends no Duel uses and does not compel |
| Finger of the Reaper | The vanilla Finger of Death effect gets +CL; damage rules add CL + kills only to Finger of Death; only a lethal final hit followed by death counts, once per death, not after healing, another source or another caster; kills serialize with the feat and reset when it is removed |

The command tests invoke game command execution and action spending directly;
they do not run animation, view initialization or the full Unity command loop.
Serializer tests cover the allowance state, not opening a complete save file.
Third-party controls exercise blueprint patterns, not an installed mod collection.

These are evidence for the listed feats only. Duel and Finger of the Reaper are described in [duel-feats.md](duel-feats.md), including what still needs confirmation in the game. Other families, weapon damage,
the complete combat loop and the in-game UI are not covered here;
see [validation.md](validation.md) for the remaining checks.

## Observations

- **Runtime.** The tests run on the game's Unity Mono with rewritten Unity copies,
  not inside the Unity player. Confirm important results in the game.
- **Starting equipment.** The pregen's starting weapons and armor fail to equip
  offline. Tests that need a weapon grant its proficiency and insert it explicitly;
  this does not validate starting-equipment initialization or other weapon feats.
- **Undead and constructs.** The game gives them no Constitution, so the
  Constitution bonus cannot show on them. The tests check that their unit is a
  living creature.
- **BlueprintCore validation.** During registration, BlueprintCore warns about
  multiple components on several feats where only one is allowed (for example
  `AttackStatReplacementFixed`). The warnings are listed in `environment.json`
  (`logWarnings`) and are not evaluated by these tests.
- **Damage to non-party creatures.** The game's "green NPC" check compares the target with the main character, which an offline session lacks, and fails inside the damage rule. Duel and Finger of the Reaper tests resolve damage inside a cutscene-command context, where the game skips that check.
- **Deaths.** `UnitLifeController.OnUnitDeath` plays the death animation on the unit's view after setting the life state. View-less units throw there, so the tests raise the life-state event the controller raises next. Falling unconscious runs unchanged.
- **Charge.** A compelled unit charges when its charge ability can reach the target, which needs pathfinding in a scene. The compulsion tests remove the charge ability and check the direct attack command; they create the command but do not run it.
- **CI.** Public CI has no game files and does not run these tests.
