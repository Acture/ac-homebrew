# Changelog

## Unreleased

### Added
- **Duel** family. Oath of the Duel grants Duel: for 2 rounds you and an enemy are compelled to attack only each other, and damage either deals to anyone else is prevented. The survivor permanently gains +1 damage with weapon attacks per duel won. 1 use per day plus 1 per 5 levels, once per encounter. Cavalier's Challenge (including Knight's Challenge) also counts as a duel without spending uses. Endless Challenge adds 2 daily uses; Single Combat halves damage from anyone but your opponent while you duel. Each costs 1 budget point.
- **Unbound Duelist**, the mod's first mythic feat: you keep control during your duels while your opponent is still compelled.
- **Finger of the Reaper** (Growth): Finger of Death deals +1 damage per caster level, plus +1 for each creature it has killed, without limit.
- Other mods can register their challenge effects as duels with `DuelRules.RegisterChallenge`. See [Duel and Finger of the Reaper](docs/duel-feats.md).
- All new text in the eight supported languages.

## 0.2.1 — 2026-10-18

### Added
- **Maneuver Tactics:** Seize the Opening grants one free-action maneuver per round, plus three extra free uses per encounter with a limit of two per round. Lingering Disruption doubles timed maneuver effects with at least two extra rounds. Both feats can be selected together and cost 2 budget points each.
- Configurable maneuver allowances and duration, localized in all eight supported languages.
- Recognition of standalone maneuvers built with standard game actions, and an opt-in API for custom maneuvers and timed effects from other mods.
- The release ZIP now includes English and Simplified Chinese player readmes, versioned changelogs, `LICENSE` and `LICENSE-EXCEPTION.md`.
- Editable `Localization/enGB.json` and `Localization/zhCN.json` cover feat names, lore, full rules and shared description templates, plus the optional ModMenu's static setting labels. Restart after editing; no DLL rebuild is needed.
- German, French, Spanish, Italian, Brazilian Portuguese and Russian follow the game language. **These six are unreviewed machine translations**; corrections are welcome. Missing text falls back to English, and switching language in game switches all feat text.
- Exclusion group names in the budget tooltips and ModMenu follow the game language.

### Changed
- Unified the UMM Id, install directory and assembly as `ACHomebrew`. New settings use `ACHomebrewSettings`; the loader reads old settings without overwriting them and blocks simultaneous loading with the old installation. Published feat GUIDs stay unchanged. See the upgrade steps in the player readme.
- **Fixed the mod not loading in some games** (UnityModManager shows it as not loaded, and its log has `MissingMethodException` for `CodeInstruction.LoadArgument`). The mod no longer uses Harmony functions missing from Harmony 2.0.4, the copy the game ships, so it loads with that copy as well as with UnityModManager's newer one.
- External text takes precedence over embedded text in the same language. Missing files/keys, blank translations, malformed JSON and invalid placeholders, rich text or encyclopedia links produce diagnostics and fall back.
- Release checks require player materials, matching language keys/template arguments and consistent package/loader versions.
- **Fixed Grandmaster's Gambit** (Intelligence stance): other attack rolls now take the intended penalty equal to your Intelligence modifier (minimum 0). Previously the penalty never applied, and an Intelligence below 10 even turned it into a bonus. The description is corrected in every language. Characters using this stance attack slightly worse outside attacks of opportunity; no respec is required.
- Other existing feat mechanics and numbers are unchanged; the new Maneuver Tactics feats are optional.

## 0.2.0 — 2026-10-11 (Public beta)

### Changed
- Renamed the mod to **AC's Homebrew** (Chinese: AC 的房规手册). The UMM Id and install folder (`AttributeFeats`), settings file and feat IDs are unchanged, so existing installs, settings and saves keep working. The assembly is now `ACHomebrew.dll` and the release ZIP `ACHomebrew-<version>.zip`; after upgrading, a leftover `AttributeFeats.dll` in the mod folder is ignored and can be deleted.
- BlueprintCore is bundled inside `ACHomebrew.dll`, so the mod no longer depends on another mod providing it.
- With ModMenu installed, the game's options screen gets a AC's Homebrew page mirroring the UMM settings. ModMenu stays optional.
- Main Attribute Mastery now improves one chosen attribute from a chosen source attribute (30 feats, one per character). The six broad Main feats are retired: kept for existing characters, narrowed to half the modifier on the other attributes.
- Narrowed broad feats to two or three effects: aptitudes (formerly Specialized Adept), the Wisdom stance, Defiance at the Precipice, Blade of the Spell-Saint (spells only) and Summoner Sacrifice (physical attributes).
- Mutual exclusion is now a set of configurable exclusion groups checked at selection time; `EnableMutex` is the master switch.
- Distance Damage is rebuilt into close, mid and long styles of three stacking feats each.
- The source is split into theme projects (Logic, Core, Attributes, Aptitudes, Martial, Defense, Magic, Summoning, Styles) merged into one assembly.

### Added
- Optional per-character feat budget across all Homebrew feat families: a feat count limit, a point limit with family costs of 1–3, or both. Both are off by default and apply immediately when changed.
- Feat tooltips show used and maximum feats or points, the feat's cost and why it cannot be chosen, in English and Simplified Chinese. The mod settings list each party member's usage.
- New families: Casting Attribute (Str/Dex/Con spellcasting per spellbook), Resource Attribute, Retaliation, Momentum, Execution, Arcana, Summoner, Survival (Undying), Stealth, Solo (Lone Wolf, Truly Solo), Growth (kill-based permanent growth), Penetration (immunity piercing) and an optional Meme menu.
- ModTagEx shows AC's Homebrew as the source of its content.
- Themed English and Chinese names, a line of flavour text and individual icons for the new feats.
- `tests/VerifyHarmonyTargets.ps1` resolves every Harmony patch target against the installed game.

### Changed
- Only granted feats count. Menus, vanilla feats and other mods' feats do not; each Weapon Damage category and each rank count separately. Prerequisite-ignoring effects do not bypass the budget.
- Characters above a lowered limit or loaded from an older save keep every feat but cannot choose more until the limit is raised or they are respecced.

### Development
- Added offline mechanics tests (`scripts/Invoke-OfflineMechanicsTests.ps1`) that load the real game assemblies, blueprint pack and settings in a test process without starting the game. They apply Main Attribute Mastery and the retired Titan's Apotheosis to a vanilla unit and remove them, include a failure control, and probe Bloodline of Beasts through the game's summon rule. Game files stay local or in an ignored `vendor/wotr/` snapshot. The game-independent part lives in the public [wotr-testing](https://github.com/Acture/wotr-testing) repository, included as the `external/wotr-testing` submodule.
- Licensed the project under the GNU Affero General Public License v3.0, with an additional permission to link with the proprietary game and Unity assemblies.

## 0.1.2 — 2026-10-05

### Added
- Six standalone Weapon Damage feats for Strength, Dexterity, Constitution, Intelligence, Wisdom and Charisma. Choose a proficient weapon category, with no Main feat or Power Mode requirement.
- Live Weapon Damage setting: Replace uses the chosen attribute when it improves damage and preserves native damage multipliers; Add grants the positive modifier once. Weapons must already use an attribute for damage. Repeat selections can cover different weapon categories.
- English and Simplified Chinese names and descriptions for all 18 selection menus and the six new Weapon Damage feats.
- 92 individual icons for the existing feats, with matching buff and stance artwork.

### Changed
- Grouped 98 feats under Attribute Feats, with 17 family menus and Long-Reach Gambit directly in the root. Each choice grants one feat; menus can be revisited subject to normal prerequisites and optional family mutex.
- Refined the original 92 English/Chinese feat names and lore. Clarified scaling, settings gates, minimum modifiers, trigger timing, distance thresholds and spell-tag penalties in their descriptions.
- Preserved published blueprint IDs, existing mechanics and saved settings. Embedded 244 bilingual text entries with English and compiled-text fallbacks; language changes refresh registered strings.
- Moved the project to `src/AttributeFeats` and consolidated documentation. Outputs are under ignored `artifacts`.
- Added `-p:DeployToGame=false` for release packaging without copying files into the game.

### Fixed
- A failed feat registration or menu no longer prevents unrelated families or the root menu from being configured. Partial failed batches are not offered or replayed.
- Added Windows/Linux checks for identifiers, metadata, damage calculations, initialization, localization and packaged assets. Actual combat effects and in-game UI still require game validation.

## 0.1.1 — Mutex Relaxation + 17 New Feats

### Mutex Redesign
- **Cross-family same-attribute mutex removed.** Combinations like Apex Predator (Str Main) + Titan's Stance (Str Defensive) + Brutal Stance (Str Stance) now stack freely. The old release-time `MutexPass` cross-attribute pass is gone; only intra-family mutex remains.
- **New `EnableMutex` setting (default ON).** When OFF, every mutex prerequisite is skipped, including intra-family. Pick this for full collector / power-stack play; leave ON for the curated baseline.

### Added (17 new feats)
- **Conditional Trigger expansion (+2):** Berserker's Last Stand (Str, HP < 25%), Tactical Reading (Int, first attack of combat).
- **Extended Replacement expansion (+3):** Brutal Defender (Str → CMD), Lightfoot Defense (Dex → AC unarmored), Iron Endurance (Con → HP).
- **Spell Tag Descriptor expansion (+4):** Etheric Mind (Force / Int), Radiant Soul (Positive Energy / Cha), Hollow Heart (Negative Energy / Wis), Subtle Tyrant (Mind-Affecting / Cha). Each follows the same `+half mod` chosen-descriptor / `−half mod` other-descriptors tradeoff pattern.
- **Summoner Sacrifice (+3, Family 25):** Body of My Pact (1:1 transfer), Doubled Bond (1:2 transfer), Empowered Sacrifice (Cha → Str focused trade). Built on the same `SummonedUnitBuff` nested-buff pattern as Greater Summoning. Intra-family 3-way mutex.
- **Polearm Master (+1, Family 12):** Reach × 2 with a −4 weapon damage tradeoff. Classic spear/pike sweet spot, opt-in.
- **Distance Damage (+3, Family 24):** Aggressor's Edge (≤10 ft), Marksman's Focus (≥30 ft), Optimal Range (15–25 ft). Each grants +4 damage in its band via `AddContextStatBonus` + `AddInitiatorAttackWithWeaponTrigger` + `ContextConditionDistanceToTarget`.

### Fixed
- **Commanding Presence Stance** now applies its 30-ft Charisma-to-attack aura to allies. The 0.1.0 release shipped a self-only buff fallback because GUID exhaustion blocked the aura blueprint; the aura is now wired up via `AddAreaEffect` with a 30 ft cylinder filtered by `IsAlly()` and `IsCaster(negate: true)`.

### Changed
- README now lists `EnableMutex` in the settings table and updates the Stacking Rules section to reflect the cross-family mutex removal.
- Foundation GUIDs extended by 26 (nested classes for Conditional2, ExtendedReplacement2, SpellTagDescriptor2, PolearmMaster.Feature, DistanceDamage, SummonerSacrifice, plus the two Commanding Presence aura blueprints).

### Save Compatibility
- 0.1.0 saves load without change; 17 new feats appear in level-up lists.
- `EnableMutex` defaults to ON for upgrading installs (matches 0.1.0 mutex strictness minus the cross-family removal).

## 0.1.0 — Build Enabler Redesign

### Breaking Changes
- Mod settings reset on upgrade. Old `EnableBattles`, `EnableCaster`, and `progression` fields are gone; reconfigure once after installing 0.1.0.
- `Echo Spell` was removed. The duplicate-cast trigger relied on a non-functional spell-casting path and is no longer shipped.
- The old global progression cycler is replaced by `Power Level` (`Balanced` / `Legacy_AllFull`) plus granular feature toggles.
- Same-attribute mutex coverage is now enforced in the final release pass to prevent cross-family double-dipping.

### Added
- **76 feats total** across the 0.1.0 release families:
  - **Main Attribute Mastery (6):** Apex Predator, Embodied Grace, Living Bulwark, Architect of Self, Wellspring of Insight, Crown of Will.
  - **Specialized Adept (24):** Titan's Stance, Flowing Form, Iron Bulwark, Calculated Defense, Stoic Vigilance, Indomitable Presence, Crushing Grip, Deft Hand, Unyielding Hold, Tactical Bind, Predictive Lock, Domineering Throw, Practiced Hand, Effortless Skill, Tireless Practice, Polymath's Touch, Quiet Mastery, Inspired Versatility, Spell-Forged Will, Quickcast Reflex, Spell-Tempered Body, Scholar of the Weave, Oracle's Intuition, Sorcerous Presence.
  - **Stance (6):** Brutal Stance, Liquid Form, Endless Vigor, Tactical Mind, Centered Mind, Commanding Presence.
  - **Conditional Trigger (4):** Endless Resolve, First Blood, Vendetta, Patient Hunter.
  - **Replacement (9):** Crushing Form, Duelist's Eye, Iron Stance, Tactical Strike, Predictive Cut, Theatrical Combat, Inner Sentinel, Calculated Grip, Unyielding Will.
  - **Greater Summoning (6):** Bloodline of Beasts, Quickened Pact, Vital Pact, Tactical Binding, Insightful Summons, Magnetic Calling.
  - **Reactive Armor (2):** Spiked Defense, Bulwark of Steel.
  - **Derived Stat Conversion (6):** Arcane Aegis, Martial Insight, Skilled Defender, Mystic Vitality, Soul Bulwark, Sword Saint.
  - **Spell Tag Specialist (13):** Pure Warder, Master Caller, Seer's Edge, Heart's Tyrant, Spellforge, Veilweaver, Death Speaker, Shape-Shifter, Inner Flame, Frozen Heart, Storm Channel, Etching Mind, Resonant Voice.
- Granular redesign settings: `IncludeSelfInAttributeStack`, `EnableDefenses`, `EnableManeuvers`, `EnableChecks`, `EnableSkills`, `EnableCasterDC`, `EnableCasterLevel`, `EnableSpellPenetration`, `EnableBAB`, and `EnablePowerMode`.
- Release packaging now emits `AttributeFeats-0.1.0.zip` from the Deploy target during `dotnet build`.

### Changed
- Main feats now use flavor names while keeping their original GUIDs for save compatibility.
- Default behavior is retuned around the build-enabler philosophy: full scaling for attributes, defenses, maneuvers, skills, caster level, and spell penetration; reduced scaling for spell DC, BAB, and Power Mode bonuses while `Balanced` is selected.
- `Weapon Insight` remains a true attack-stat replacement path via `AddAttackStatReplacementFixed`; the unsafe untyped fallback path is gone.
- Self-stacking is now opt-in through `IncludeSelfInAttributeStack` instead of being assumed.

### Fixed
- Removed the duplicate `<Target Name="Deploy">` definition from `attribute feats.csproj`.
- Removed the old `BonusCasterLevel` double-scaling path from the Main / Specialized caster bonus flow.
- Added duplicate-rank registration protection in `Common.AddRank` to prevent repeated rank configs on the same feat.
- Locked down stable GUID handling with explicit save-compat comments and final duplicate GUID verification.
- Removed dead Echo Spell-era release baggage and aligned release packaging with the 0.1.0 artifact name.
