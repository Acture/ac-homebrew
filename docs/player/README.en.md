# AC's Homebrew 0.2.1

A homebrew feat mod for **Pathfinder: Wrath of the Righteous**, installed with
Unity Mod Manager (UMM). Formerly AttributeFeats; the mod's installed folder is
still `Mods/AttributeFeats`.

## What you get

During character creation or level-up, open **AC's Homebrew**, open a feat
family, then choose an individual feat. One choice grants one feat; opening a
menu is not an extra feat. You can revisit families at later feat choices,
subject to their prerequisites, exclusion groups and any budget you enable.

- **Attributes:** improve one attribute from another, use an attribute for
  weapon attack or damage, convert derived statistics, or change a spellbook's
  casting attribute or a resource's bonus uses.
- **Aptitudes:** themed attribute bonuses to defense, maneuvers, skills or magic.
- **Martial:** activatable stances, conditional triggers, close/mid/long-range
  styles, reach, attack momentum and execution effects.
- **Defense:** reactive armor, retaliation and a temporary survival effect.
- **Magic:** school/descriptor specialists, spell-slot effects and immunity piercing.
- **Summoning:** stronger or more numerous summons, sacrificial bargains and
  shared healing.
- **Styles:** stealth, solo play and permanent growth from kills. Optional joke
  feats are off by default.

Names, lore and complete rule text—including shared description templates—are
readable in the `Localization` folder, one JSON file per game language. The same
files are used by the mod, not a separate preview. Text changes do not change effects.

## Install

1. Install [Unity Mod Manager](https://www.nexusmods.com/site/mods/21) for Wrath.
2. Download `ACHomebrew-0.2.1.zip` from
   [GitHub Releases](https://github.com/Acture/ac-homebrew/releases) or
   [Nexus Mods](https://www.nexusmods.com/pathfinderwrathoftherighteous/mods/1217).
3. Install that ZIP in UMM, then enable AC's Homebrew.
4. Start or restart the game. In-game, open UMM (normally **Ctrl+F10**) for settings.

BlueprintCore is already bundled. ModMenu and ModTagEx are optional; with
ModMenu installed, the game's options also show an AC's Homebrew settings page.

## Upgrade and existing characters

Exit the game first. Back up your saves and the existing `Mods/AttributeFeats`
folder, including settings and any edited language files. Install the new ZIP
through UMM, then reapply your translations to the new language files so new
keys are retained. Do not run the old and new mod as separate installations.

The UMM identity, settings identity and published feat IDs are unchanged.
**0.2.1 changes delivery and text loading, not feat effects or numbers.**
A leftover `AttributeFeats.dll` from older versions is no longer the entry
assembly; the current one is `ACHomebrew.dll`.

If upgrading from 0.1.x, note the **0.2.0 gameplay changes**: the six broad Main
feats are retired, retained on existing characters with narrowed effects.
Respec to choose the new source-to-target Main Attribute Mastery. Aptitudes,
some stances/triggers, summoner sacrifice and distance styles were also
adjusted; old characters receive the new effects. See the versioned
`CHANGELOG.md` (or `CHANGELOG.zh-CN.md`). Back up before upgrading from 0.0.x,
which predates the 0.1.0 redesign and settings reset.

## Settings and limits

The default **Balanced** mode is the intended baseline. `Legacy_AllFull` gives
full rank scaling; BAB and Power Mode options are off by default and are
deliberately stronger.

- **Weapon Damage:** Replace (default) uses the chosen attribute when better;
  Add adds its positive modifier once. The weapon must already add an attribute
  to damage. Both modes keep the same feat.
- **Feat budget:** count and point limits are both off by default. Enable either
  or both; they apply across all Homebrew families per character. Menus, vanilla
  feats and other mods' feats do not count. Repeated parametrized choices and
  ranks count separately. Lowering a limit never removes learned feats, but
  prevents further choices while over budget.
- **Exclusion groups:** enabled by default, with individual switches and limits.
  Disabling the master switch stops group enforcement. Casting/resource
  attributes still allow only one choice per spellbook/resource.
- **Casting:** scope is the selected spellbook by default; the attribute is
  always replaced by default. You may choose all spellbooks or only-when-higher.
- **Truly Solo:** pets do not reduce the absent-companion count by default.
  **Meme feats:** off by default.

Weapon damage, budget, exclusions, casting scope/mode and the Truly Solo pet
option apply immediately. Other switches, including Meme, require restarting
the game. Feature toggles can disable corresponding attribute/aptitude bonuses;
read each feat's rules before planning a build.

## Language files and translation

The game language selects the file: English (`enGB`), Simplified Chinese
(`zhCN`), German (`deDE`), French (`frFR`), Spanish (`esES`), Italian (`itIT`),
Brazilian Portuguese (`ptBR`) or Russian (`ruRU`). Any text missing from a file
falls back to English.

**German, French, Spanish, Italian, Portuguese and Russian are machine
translations** that no native speaker has reviewed yet. Names and terms may read
awkwardly or differ from the game's official wording. Corrections are welcome:
edit the file as described below and send the change through
[GitHub](https://github.com/Acture/ac-homebrew/issues) or the Nexus page. Edit the appropriate UTF-8 JSON file inside the installed
mod's `Localization` folder, save it, and **restart the game**. No DLL rebuild
is needed. Locale switching refreshes strings already registered in that session;
it does not reread edited files.

Keep keys unchanged. Shared templates use numbered arguments such as `{0}`
and `{1}`: you may reorder them but must retain each argument and any format
specifier. Preserve `\n`, rich-text tags such as `<i>`/`<b>` and any encyclopedia
links such as `{g|Encyclopedia:Armor}...{/g}`. Translate link labels, not targets.
JSON requires escaped quotes and backslashes; do not add trailing commas.

External text takes precedence. Missing files or keys, blank translations and
invalid placeholders/markup fall back to the embedded language text. A malformed
JSON file falls back as a whole. Diagnostics appear in the mod's UMM/game log;
unknown keys are reported and ignored. Missing Chinese embedded text falls back
to English, then to the code's emergency text.

## Known issues and feedback

This is a public beta: feat balance and behavior across all game situations
are not yet confirmed. Back up important saves and avoid relying on an
untried feat for an irreversible build.

- The spellbook screen header may still show the original casting attribute.
- The UMM settings panel is in English; the optional ModMenu page is bilingual.
- The six Weapon Damage feats do not have bespoke icons; Main Attribute
  Mastery reuses source-attribute artwork.

Report bugs on [GitHub Issues](https://github.com/Acture/ac-homebrew/issues) or
the [Nexus page](https://www.nexusmods.com/pathfinderwrathoftherighteous/mods/1217).
Include mod/game versions, feat name, relevant settings, expected and actual
behavior, reproduction steps and the relevant log excerpt. Do not post your
game files or personal information.

## License

GNU Affero General Public License v3.0; see `LICENSE`. The additional permission
in `LICENSE-EXCEPTION.md` covers linking with the proprietary game and Unity
assemblies. Those assemblies are not included in this ZIP.
