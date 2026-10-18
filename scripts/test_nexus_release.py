"""Offline controls for preparing Nexus uploads; never contacts Nexus."""
import copy
import hashlib
import json
import shutil
import subprocess
import sys
import tempfile
import unittest
import zipfile
from pathlib import Path

from prepare_nexus_release import (
    inspect_release, prepare_release, upload_inputs, require_reviewers, write_outputs, changelog_section,
)
from release_delivery import ROOT, REQUIRED, check_sources


class NexusReleaseTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory(prefix="nexus-release-test-")
        self.addCleanup(self.temporary.cleanup)
        self.directory = Path(self.temporary.name)
        self.version, _ = check_sources()
        self.tag = "v" + self.version
        self.repository = "Acture/ac-homebrew"
        self.source = self.directory / "source"
        paths = set(REQUIRED.values()) | {
            "src/ACHomebrew/ACHomebrew.csproj", "Repository.json", "docs/icon-manifest.json",
        }
        for relative in paths:
            target = self.source / relative
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(ROOT / relative, target)
        shutil.copytree(ROOT / "src/ACHomebrew/Icons", self.source / "src/ACHomebrew/Icons")
        self.package = self.directory / f"ACHomebrew-{self.version}.zip"
        self.archive()
        self.output = self.directory / "ready"

    def archive(self, omit=None, overrides=None, extra=None):
        with zipfile.ZipFile(self.package, "w") as archive:
            for name, source in REQUIRED.items():
                if name != omit:
                    archive.writestr(name, (overrides or {}).get(name, (self.source / source).read_bytes()))
            for icon in (self.source / "src/ACHomebrew/Icons").glob("*.png"):
                archive.writestr("Icons/" + icon.name, icon.read_bytes())
            archive.writestr("ACHomebrew.dll", b"fixture, not a game or mod assembly")
            for name, value in (extra or {}).items():
                archive.writestr(name, value)
        self.digest = hashlib.sha256(self.package.read_bytes()).hexdigest()
        self.metadata = {
            "tag_name": self.tag,
            "draft": False,
            "prerelease": True,
            "published_at": "2026-10-13T00:00:00Z",
            "html_url": f"https://github.com/{self.repository}/releases/tag/{self.tag}",
            "assets": [{
                "id": 123,
                "name": self.package.name,
                "state": "uploaded",
                "size": self.package.stat().st_size,
                "digest": "sha256:" + self.digest,
            }],
        }

    def prepare(self):
        return prepare_release(self.metadata, self.tag, self.repository, self.source, self.package, self.output)

    def test_text_line_endings_may_differ_between_windows_zip_and_linux_checkout(self):
        for source_newline, archive_newline in ((b"\n", b"\r\n"), (b"\r\n", b"\n")):
            with self.subTest(source_newline=source_newline):
                self.output = self.directory / f"ready-eol-{len(source_newline)}"
                overrides = {}
                for name, relative in REQUIRED.items():
                    path = self.source / relative
                    canonical = path.read_bytes().replace(b"\r\n", b"\n")
                    path.write_bytes(canonical.replace(b"\n", source_newline))
                    overrides[name] = canonical.replace(b"\n", archive_newline)
                self.archive(overrides=overrides)
                result = self.prepare()
                self.assertEqual(result["sha256"], self.digest)
                self.assertEqual((self.output / self.package.name).read_bytes(), self.package.read_bytes())

    def test_text_comparison_still_rejects_content_and_whitespace_changes(self):
        original = (self.source / REQUIRED["README.en.md"]).read_bytes().replace(b"\r\n", b"\n")
        for changed in (original + b" ", original.replace(b"ACHomebrew", b"WrongMod")):
            with self.subTest(changed=changed[-30:]):
                self.archive(overrides={"README.en.md": changed})
                with self.assertRaisesRegex(AssertionError, "archive differs from build inputs"):
                    self.prepare()
                self.assertFalse(self.output.exists())

    def test_published_prerelease_metadata_selects_exact_asset(self):
        result = inspect_release(self.metadata, self.tag, self.repository)
        self.assertEqual(result["version"], self.version)
        self.assertEqual(result["filename"], self.package.name)
        self.assertEqual(result["asset_id"], 123)
        self.assertEqual(result["sha256"], self.digest)

    def test_draft_unpublished_or_wrong_release_is_rejected(self):
        for field, value in (
            ("draft", True), ("draft", "false"), ("published_at", None), ("published_at", True),
            ("tag_name", "v99.0.0"), ("html_url", "https://example.com/release"),
        ):
            with self.subTest(field=field, value=value):
                metadata = copy.deepcopy(self.metadata)
                metadata[field] = value
                with self.assertRaisesRegex(ValueError, "release|published|draft"):
                    inspect_release(metadata, self.tag, self.repository)

    def test_missing_duplicate_or_unfinished_zip_is_rejected(self):
        for assets in ([], self.metadata["assets"] * 2,
                       [{**self.metadata["assets"][0], "state": "new"}],
                       [{**self.metadata["assets"][0], "name": "wrong.zip"}]):
            with self.subTest(assets=assets):
                metadata = {**self.metadata, "assets": assets}
                with self.assertRaisesRegex(ValueError, "asset|ZIP"):
                    inspect_release(metadata, self.tag, self.repository)

    def test_digest_size_and_asset_id_must_be_unambiguous(self):
        for field, value in (
            ("digest", None), ("digest", "sha256:no"), ("digest", "md5:" + self.digest),
            ("size", 0), ("size", True), ("id", 0), ("id", "123"),
        ):
            with self.subTest(field=field, value=value):
                metadata = copy.deepcopy(self.metadata)
                metadata["assets"][0][field] = value
                with self.assertRaises(ValueError):
                    inspect_release(metadata, self.tag, self.repository)

    def test_tag_and_repository_cannot_inject_paths_or_runner_commands(self):
        for tag in ("../v0.2.1", "v0.2.1\nother=1", "v0.2.1-rc1", "--help", "v0.2"):
            with self.subTest(tag=tag), self.assertRaises(ValueError):
                inspect_release(self.metadata, tag, self.repository)
        for repository in ("../outside", "Acture/ac-homebrew\nother=1", "https://github.com/Acture/ac-homebrew"):
            with self.subTest(repository=repository), self.assertRaises(ValueError):
                inspect_release(self.metadata, self.tag, repository)

    def test_preparation_keeps_exact_zip_and_bilingual_version_changelog(self):
        result = self.prepare()
        self.assertEqual((self.output / self.package.name).read_bytes(), self.package.read_bytes())
        self.assertEqual(result["sha256"], self.digest)
        self.assertIn("English", result["changelog"])
        self.assertIn("Simplified Chinese", result["changelog"])
        self.assertIn(self.version, result["changelog"])
        manifest = json.loads((self.output / "nexus-upload.json").read_text(encoding="utf-8"))
        self.assertEqual(manifest, result)
        inputs = upload_inputs(self.output, self.tag, self.repository, self.digest)
        self.assertEqual(inputs["sha256"], self.digest)
        self.assertEqual(Path(inputs["package_path"]), (self.output / self.package.name).resolve())

    def test_corrupt_download_and_wrong_size_fail_before_output(self):
        self.package.write_bytes(self.package.read_bytes() + b"changed")
        with self.assertRaisesRegex(ValueError, "size|SHA-256"):
            self.prepare()
        self.assertFalse(self.output.exists())

    def test_shared_delivery_and_asset_failures_are_not_bypassed(self):
        for options in (
            {"omit": "LICENSE"}, {"overrides": {"README.en.md": b"wrong"}},
            {"extra": {"Game.dll": b"must not publish"}},
            {"extra": {"nested/ACHomebrew.dll": b"must not publish"}},
            {"extra": {"scripts/tool.py": b"must not publish"}},
            {"extra": {"../outside": b"must not publish"}},
        ):
            with self.subTest(options=options):
                self.archive(**options)
                with self.assertRaises((AssertionError, ValueError)):
                    self.prepare()
                self.assertFalse(self.output.exists())

    def test_matching_tag_source_is_used_not_workflow_worktree(self):
        path = self.source / REQUIRED["README.en.md"]
        path.write_bytes(path.read_bytes() + b"\nsource changed\n")
        with self.assertRaises((AssertionError, ValueError)):
            self.prepare()
        self.assertFalse(self.output.exists())

    def test_mismatched_release_and_source_version_is_rejected(self):
        self.metadata["tag_name"] = "v99.0.0"
        self.metadata["html_url"] = f"https://github.com/{self.repository}/releases/tag/v99.0.0"
        self.metadata["assets"][0]["name"] = "ACHomebrew-99.0.0.zip"
        with self.assertRaises((AssertionError, ValueError)):
            prepare_release(self.metadata, "v99.0.0", self.repository, self.source, self.package, self.output)
        self.assertFalse(self.output.exists())

    def test_changelog_extracts_only_the_exact_nonempty_version(self):
        path = self.directory / "changelog.md"
        path.write_text(
            f"# Changelog\n## {self.version}0 date\n\nwrong\n\n"
            f"## {self.version} date\n\nreviewed text\n\n## 0.0.0 date\n\nolder\n",
            encoding="utf-8")
        section = changelog_section(path, self.version)
        self.assertIn("reviewed text", section)
        self.assertNotIn("wrong", section)
        self.assertNotIn("older", section)
        path.write_text(f"## {self.version} date\n\n## 0.0.0 date\n\nolder\n", encoding="utf-8")
        with self.assertRaisesRegex(ValueError, "nonempty changelog"):
            changelog_section(path, self.version)

    def test_upload_stage_rechecks_bytes_and_prepared_checksum(self):
        self.prepare()
        with self.assertRaisesRegex(ValueError, "SHA-256"):
            upload_inputs(self.output, self.tag, self.repository, "0" * 64)
        package = self.output / self.package.name
        package.write_bytes(package.read_bytes() + b"changed")
        with self.assertRaisesRegex(ValueError, "size|SHA-256"):
            upload_inputs(self.output, self.tag, self.repository, self.digest)

    def test_upload_manifest_cannot_choose_arbitrary_file(self):
        self.prepare()
        path = self.output / "nexus-upload.json"
        manifest = json.loads(path.read_text(encoding="utf-8"))
        manifest["filename"] = "../outside.zip"
        path.write_text(json.dumps(manifest), encoding="utf-8")
        with self.assertRaisesRegex(ValueError, "filename|ZIP"):
            upload_inputs(self.output, self.tag, self.repository, self.digest)

    def test_missing_required_reviewer_protection_fails_closed(self):
        for environment in (
            [], {}, {"protection_rules": None}, {"protection_rules": [None]}, {"protection_rules": []},
            {"protection_rules": [{"type": "wait_timer"}]},
            {"protection_rules": [{"type": "required_reviewers", "reviewers": []}]},
        ):
            with self.subTest(environment=environment), self.assertRaisesRegex(ValueError, "reviewer"):
                require_reviewers(environment)
        require_reviewers({"protection_rules": [{
            "type": "required_reviewers", "reviewers": [{"type": "User", "reviewer": {"id": 1}}],
        }]})

    def test_multiline_runner_outputs_preserve_text_without_injection(self):
        path = self.directory / "outputs"
        value = "first\nname=not_an_output\nlast"
        write_outputs(path, {"changelog": value, "version": self.version})
        lines = path.read_text(encoding="utf-8").splitlines()
        name, delimiter = lines[0].split("<<", 1)
        self.assertEqual(name, "changelog")
        end = lines.index(delimiter, 1)
        self.assertEqual("\n".join(lines[1:end]), value)
        self.assertEqual(lines[end + 1], f"version={self.version}")

    def test_malformed_metadata_shapes_have_explicit_cli_errors(self):
        path = self.directory / "metadata.json"
        for data in ([], {**self.metadata, "assets": [None]}, {**self.metadata, "assets": None}):
            with self.subTest(data=data):
                path.write_text(json.dumps(data), encoding="utf-8")
                result = subprocess.run([
                    sys.executable, str(ROOT / "scripts/prepare_nexus_release.py"), "metadata",
                    "--metadata", str(path), "--tag", self.tag, "--repository", self.repository,
                ], capture_output=True, text=True)
                self.assertNotEqual(result.returncode, 0)
                self.assertEqual(result.stdout, "")
                self.assertIn("Nexus preparation failed:", result.stderr)
                self.assertNotIn("Traceback", result.stderr)

    def test_cli_preparation_writes_summary_and_safe_upload_outputs(self):
        path = self.directory / "metadata.json"
        path.write_text(json.dumps(self.metadata), encoding="utf-8")
        summary = self.directory / "summary.md"
        outputs = self.directory / "outputs"
        result = subprocess.run([
            sys.executable, str(ROOT / "scripts/prepare_nexus_release.py"), "prepare",
            "--metadata", str(path), "--tag", self.tag, "--repository", self.repository,
            "--source-root", str(self.source), "--package", str(self.package),
            "--output-directory", str(self.output), "--summary", str(summary), "--github-output", str(outputs),
        ], capture_output=True, text=True)
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn(self.digest, summary.read_text(encoding="utf-8"))
        self.assertIn("Upload wording", summary.read_text(encoding="utf-8"))
        self.assertIn("changelog<<", outputs.read_text(encoding="utf-8"))
        self.assertEqual((self.output / self.package.name).read_bytes(), self.package.read_bytes())


if __name__ == "__main__":
    unittest.main()
