# Publish the same release ZIP to Nexus Mods

`.github/workflows/nexus-publish.yml` is the **Publish existing release to Nexus**
workflow. It downloads a published GitHub Release asset, checks it against that
release tag's source files, and uploads those exact bytes to Nexus Mods after
environment approval. It does not build the mod or upload game assemblies.

It starts in two ways:

- **Automatically when a GitHub Release is published** (see the
  [release flow](localization-and-release.md#release-flow)). Every published
  release, prereleases included, requests the upload and then waits for a
  `nexus-publish` reviewer; reject the deployment to skip Nexus for that release.
  Because a release run uses the workflow file from the
  tagged commit, that commit must already be on the default branch.
- **Manually** from the default branch, for a preview or to retry a release.

The uploader is the [official Nexus Mods action](https://github.com/Nexus-Mods/upload-action),
pinned to commit `98532f7670c27cbecc78fae03fa3e278147827e2`
(`v1.0.0-beta.11`). It uses the Nexus v3 API, not browser cookies. Review changes
to the pinned action before upgrading; this version is still a beta.

## One-time repository setup

Merge the workflow into the default branch before using it. In GitHub repository
settings, create an environment named **`nexus-publish`** and configure at least
one **required reviewer**. A workflow referring to an environment does not
automatically configure reviewer protection. This workflow checks the environment
API and refuses an upload request when required reviewers are absent.

Configure these values on that environment, not in source files:

| Kind | Name | Value |
|---|---|---|
| Secret | `NEXUSMODS_API_KEY` | Nexus API key belonging to an author allowed to update this mod |
| Variable | `NEXUSMODS_FILE_ID` | Existing Nexus file ID to which the action should add a version |
| Variable | `NEXUSMODS_MOD_ID` | Nexus mod ID used by the v3 changelog endpoint |

The Nexus mod page must already exist and have at least one uploaded file.
Find the identifiers using **Advanced** on the public Files tab or the edit
menu on Manage Files, as described in the official action's README. Do not
assume the number in a mod-page URL is the identifier expected by the v3 API.
The repository's player documentation links to
[the existing mod page](https://www.nexusmods.com/pathfinderwrathoftherighteous/mods/1217).

Do not paste API keys into issues, workflow inputs, logs or the repository.
Repository environment settings, credentials and a first live Nexus upload are
not created or tested by the code integration.

## Manual preview, then explicitly approve an upload

Build and validate locally using the
[release delivery procedure](localization-and-release.md). Publish the reviewed
ZIP once as the GitHub Release asset `ACHomebrew-X.Y.Z.zip`, attached to tag
`X.Y.Z` or `vX.Y.Z`. The tag must point to the matching source files, including
the bilingual player materials and release contracts. Published prereleases are
allowed; drafts are not.

In **Actions → Publish existing release to Nexus → Run workflow**:

1. Select the default branch and enter the published release tag.
2. Leave **Upload after validation and environment approval** unchecked to
   preview. The run downloads the exact asset ID, checks its GitHub-provided
   SHA-256 and size, and validates versions, required files, source bytes,
   language resources and icon coverage. Missing digests, duplicate matching
   assets, unsafe ZIP paths and contributor/dependency files fail the run.
3. Review the run summary's source link, ZIP name, SHA-256 and bilingual upload
   wording. The wording is taken from that version's English and Chinese
   changelog sections, not the latest default-branch text.
4. Start a new run for the same tag with upload checked. Review that run's
   preparation summary again before approving the **`nexus-publish`** deployment.

The preview needs no Nexus credentials. For an upload, preparation checks
required reviewer protection before requesting deployment. The upload job then
downloads only this run's prepared artifact and rechecks the ZIP against the
preparation job's checksum before using the Nexus key.

The action adds a **main** file version, uses the bilingual changelog as its
description and changelog, and updates the mod version. It **does not archive
the previous file** or change the default mod-manager download selection.
The workflow has read-only GitHub permissions and serializes publishing runs.
Prepared artifacts are kept for seven days; they are not a second release build.

## Failures and validation limits

Missing environment configuration, invalid release metadata or a failed package
check stops before any Nexus upload. There are no automatic upload retries.
The official action uploads the file version **before** adding the changelog:
a later error can mean the file already exists on Nexus. A reported version ID
is added to the run summary even when that later step fails. Check Nexus before
retrying; do not blindly rerun a failed upload and create duplicate versions.

After a live upload, download both copies and compare their SHA-256 and version.
A successful preparation or API response is not an independent check of the
Nexus download, nor proof of in-game behavior.

Offline failure controls require no game files or Nexus credentials:

```powershell
python scripts\test_nexus_release.py
```

They cover published-release metadata, exact asset selection, digest and size
failures, tagged-source validation, delivery/asset exclusions, changelog and
runner outputs, reviewer configuration, and checksum rechecking between jobs.
Public CI runs these without publishing. Workflow lint and fixture tests cannot
prove that a configured Nexus account has permission to upload; that requires a
separately reviewed live run.
