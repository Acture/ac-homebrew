# Localization and release delivery

## One text source, embedded and editable

`src/ACHomebrew.Core/Localization/enGB.json` and `zhCN.json` are the canonical
UTF-8 resources. Each is a JSON object mapping stable text keys to strings.
The Core project embeds these exact files; the entry project copies them to
`Localization/` in the output and release ZIP. Do not maintain a second
hand-written export. English retains the historical embedded resource name
`AttributeFeats.Localization.FeatText.json`; this is a resource alias, not a
third source file.

The existing `Common.Text`, `Common.L` and locale-refresh path serve all feat
families. `Common.Format` resolves a shared template before formatting its
numbered arguments. Names, lore, effects, restrictions and description shells
are all in the catalog; the literal arguments in code are emergency fallbacks,
not a different editable catalog. Some shared keys intentionally serve several
feats. The optional ModMenu's static bilingual labels also use these resources;
the UMM settings panel remains English.

UMM's entry point calls `FeatTextCatalog.Initialize` with the **installed mod
directory**, not the working directory or game root, after setting up logging.
For each requested language the precedence is:

1. Valid external translation from the installed `Localization` file.
2. Embedded text for that language.
3. Embedded/loaded English if Chinese text is unavailable.
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
Non-Chinese game locales use English.

## Player delivery contract

The ZIP root must contain:

| Delivered path | Build input |
|---|---|
| `README.en.md`, `README.zh-CN.md` | `docs/player/` |
| `CHANGELOG.md` | Repository root |
| `CHANGELOG.zh-CN.md` | `docs/player/` |
| `LICENSE`, `LICENSE-EXCEPTION.md` | Repository root |
| `Localization/enGB.json`, `Localization/zhCN.json` | Core's canonical resources |
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
Roslyn source checks require literal text references to exist in both catalogs,
agree with emergency fallbacks and cover bilingual feat text. These use API
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

Build once without installing into the game:

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

The [manual Nexus publishing workflow](nexus-publishing.md) can download the
existing GitHub Release ZIP, recheck it against its tag and digest, and upload
those same bytes after environment approval. It defaults to preview only;
configure the documented environment secret, identifiers and required reviewers
before requesting an upload.
