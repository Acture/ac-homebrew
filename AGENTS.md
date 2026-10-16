# AC's Homebrew development (formerly AttributeFeats)

## Repository layout

- The Mod ships as one assembly built from several projects under `src/`:
  `ACHomebrew` (UMM entry, registration order, `Info.json`, icons, optional
  ModMenu page and packaging), `ACHomebrew.Logic` (game-independent logic
  and GUIDs, referenced by the tests), `ACHomebrew.Core` (menus, budget and
  exclusion enforcement, settings, localization and shared builders), and
  theme projects `ACHomebrew.Attributes`, `.Aptitudes`, `.Martial`,
  `.Defense`, `.Magic`, `.Summoning` and `.Styles`, one file per feat family.
  Group new families by play theme, not by inspiration source. The entry
  project merges every project and BlueprintCore into `ACHomebrew.dll`
  with ILRepack after build. Keep the UMM Id `AttributeFeats`, the install
  folder `Mods/AttributeFeats` and the settings XML root unchanged so
  existing installs, settings and saves keep working. Shared game references live in `src/WrathMod.props`.
  Open `ACHomebrew.slnx` from the repository root.
- Keep tests in `tests/`, CLI tools in `scripts/`, and workflows in `.github/`.
- Use `docs/` for mod documentation.
- Build outputs, intermediate files, packages and test reports go in ignored
  `artifacts/`. Keep the shared local `GamePath.props` at the root and untracked.
- Keep `Repository.json` at the root for existing release metadata consumers.
- Offline game tests use the public `WotR.Testing.Offline` NuGet package from
  wotr-testing (https://github.com/Acture/wotr-testing). Change the library there,
  publish it, then update the `PackageReference` in `tests/OfflineMechanics.Tests`.
  Keep local game-file snapshots in ignored `vendor/wotr/` (the package's
  `New-WotrSnapshot.ps1`); never commit or publish them.
- Build for verification with `dotnet build ACHomebrew.slnx -p:DeployMod=false`
  to avoid copying files into the installed game. Repository contracts run with
  `pwsh -NoProfile -File scripts/Test-RepositoryContracts.ps1`.
- Build a release ZIP without deploying via `dotnet build ACHomebrew.slnx
  -c Release -p:DeployToGame=false`. Follow [docs/validation.md](docs/validation.md)
  for calculation, initialization, localization, asset and package checks.

## Documentation

- Read [README.md](README.md) for the project, installation and build instructions.
- Put reviewed usage, configuration, integration and contributor documentation
  in [docs/](docs/README.md). Keep README as the entry point and CHANGELOG for releases.

## Validation

- Confirm repository checks and GitHub-managed CodeQL results on the latest PR
  commit before merging. Keep the CodeQL rule on the default branch enabled.
- Keep historical research observations distinct from current implementation and
  test evidence. Static repository checks do not prove feats work in the game.
- Use CLI and code-based checks; do not use computer use or launch a game as part
  of a documentation-only change.
