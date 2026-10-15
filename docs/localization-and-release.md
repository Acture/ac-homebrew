# Localization and release delivery

## One text source, embedded and editable

`src/ACHomebrew.Core/Localization/<locale>.json` are the canonical UTF-8
resources, one per supported game locale: `enGB`, `zhCN`, `deDE`, `frFR`, `esES`,
`itIT`, `ptBR` and `ruRU`. The six added in 0.2.1 (all but English and Chinese)
are unreviewed machine translations; say so wherever they are presented. Each is a JSON object mapping stable text keys to strings.
The Core project embeds these exact files; the entry project copies them to
`Localization/` in the output and release ZIP. Do not maintain a second
hand-written export. English retains the historical embedded resource name
`AttributeFeats.Localization.FeatText.json`; this is a resource alias, not a
third source file.

Feat builders compose `LocalizedText` values: `LocalizedText.Text(key, english)`
and `LocalizedText.Format(key, english, args...)` resolve the catalog once per
supported locale, with `LocalizedText` arguments resolved in the same locale.
`Common.L(key, text)` registers all versions, and the locale-refresh patch swaps
the stored text for the new locale. `Common.JoinList` joins names using the
`Text.List.*` keys. The only compiled text is English; no other language, and no
locale-specific code path, belongs in source (the Roslyn source checks enforce this). Names, lore, effects, restrictions and description shells
are all in the catalog; the literal arguments in code are emergency fallbacks,
not a different editable catalog. Some shared keys intentionally serve several
feats. The optional ModMenu's static labels also use these resources;
the UMM settings panel remains English.

UMM's entry point calls `FeatTextCatalog.Initialize` with the **installed mod
directory**, not the working directory or game root, after setting up logging.
For each requested language the precedence is:

1. Valid external translation from the installed `Localization` file.
2. Embedded text for that language.
3. Embedded/loaded English if the requested language lacks the key.
4. The caller's compiled emergency text if the key is unavailable.

Missing files and keys, blank translations, unknown keys, invalid JSON, invalid
value types and duplicate keys are logged. A broken document falls back as a
whole; a blank or structurally invalid translation falls back per key. Numbered
arguments (including format/alignment), balanced rich text and encyclopedia
link targets must match the original. Link labels and placeholder order may
change. Preserve JSON escaping and `\n` paragraph structure when translating.
Do not change GUIDs or numerical component configuration to translate rules.

External edits are read at startup. Restart after saving edits; changing the
game locale only refreshes registered strings from the already loaded catalog.
Game locales without a language file (none at present besides the voice-only
`Sound` locale) use English.

## Player delivery contract

The ZIP root must contain:

| Delivered path | Build input |
|---|---|
| `README.en.md`, `README.zh-CN.md` | `docs/player/` |
| `CHANGELOG.md` | Repository root |
| `CHANGELOG.zh-CN.md` | `docs/player/` |
| `LICENSE`, `LICENSE-EXCEPTION.md` | Repository root |
| `Localization/<locale>.json` for all eight locales | Core's canonical resources |
| `Info.json` | Entry project |
| `ACHomebrew.dll`, its configuration/symbols and `Icons/` | Build output |

Contributor documentation, tests, scripts, private notes and proprietary game
or dependency DLLs do not belong in the ZIP. Player guides explain features,
installation, upgrading, settings, known issues and feedback—not developer test
reports. Keep their version and download filename aligned with `Info.json`,
project version fields, `Repository.json` and the latest changelog headings.

## Validation and publishing

Run the [validation commands](validation.md). Localization tests compile the
production loader and registration/refresh code and edit real temporary
language files. They assert the edited name and rule template reach a
localization pack, as well as fallback diagnostics and preserved markup.
Roslyn source checks require literal text references to exist in every language file,
agree with English emergency fallbacks and keep translations out of source. These use API
doubles at the game boundary and do not establish Unity's locale hook or layout.

`scripts/release_delivery.py` is the shared source/ZIP contract.
`scripts/test_release_delivery.py` runs a valid delivery fixture and failure
controls for every required file, invalid resources, version and byte mismatch.
CI checks those without licensed game files; local final-package checks inspect
the actual compiled ZIP, not just the fixture.

With local game references, `tests/VerifyExternalLocalization.ps1` also invokes
the loader and formatter in the merged release DLL against edited temporary
copies of the delivered resources. It verifies external precedence and broken
JSON diagnostics without rebuilding. Unity-backed locale lookup, string
conversion and BlueprintCore registration are outside this offline DLL check;
the source tests exercise registration/refresh using API doubles instead.

### Release flow

CI cannot build the mod (it needs licensed game assemblies), so the ZIP is built
locally and the rest is automated:

1. Merge the version bump, player guides and both changelog sections to `master`.
2. Run `pwsh -NoProfile -File scripts/Publish-Release.ps1` on an up-to-date, clean
   `master`. It builds the Release ZIP, runs the package check, creates and pushes
   tag `vX.Y.Z`, waits for the draft release and uploads the ZIP to it.
3. Pushing the tag runs **Draft release from version tag**
   (`.github/workflows/release-draft.yml`): it requires the tagged commit to be on
   `master`, checks the tag against the source version and player materials, and
   creates a **draft** GitHub Release titled `AC's Homebrew X.Y.Z` with the English
   and Chinese changelog sections as notes. An existing release is left unchanged.
4. Review the draft (notes, prerelease flag, attached ZIP) and publish it.
5. Publishing runs the [Nexus workflow](nexus-publishing.md) on the same ZIP. A
   normal release requests the upload and waits for `nexus-publish` approval; a
   prerelease only previews.

To build and check without releasing:

```powershell
dotnet build ACHomebrew.slnx -c Release -p:DeployToGame=false
python scripts/validate_assets.py --release artifacts/packages/ACHomebrew-0.2.1.zip
Get-FileHash artifacts\packages\ACHomebrew-0.2.1.zip -Algorithm SHA256
```

After reviewing the release wording, upload **that same verified ZIP** to both
GitHub Release and Nexus; do not rebuild separately for each destination. Use
the matching version's changelog section for both release descriptions, with
the Chinese counterpart where appropriate. Confirm both downloads have the
same SHA-256 and version. A package build/check does not itself publish a release.

The [Nexus publishing workflow](nexus-publishing.md) downloads the published
GitHub Release ZIP, rechecks it against its tag and digest, and uploads those
same bytes after environment approval. It runs automatically when a release is
published and can also be started manually (preview by default); configure the
documented environment secret, identifiers and required reviewers first.
