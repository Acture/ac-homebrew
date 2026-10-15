"""Check draft release notes and the tag/version guard without GitHub access."""
import shutil
import tempfile
from pathlib import Path

from draft_release import release_notes
from release_delivery import ROOT, check_sources


def rejected(root, tag):
    try:
        release_notes(root, tag)
    except (AssertionError, ValueError):
        return
    raise AssertionError(f"Release notes unexpectedly accepted {tag}")


def main():
    version, _ = check_sources()
    result = release_notes(ROOT, f"v{version}")
    assert result["title"] == f"AC's Homebrew {version}"
    notes = result["notes"]
    assert notes.startswith("## English\n\n### ") and "\n## 简体中文\n\n### " in notes, "both changelog sections"
    assert f"## {version}" not in notes, "version headings are replaced by language headings"
    assert f"ACHomebrew-{version}.zip" in notes
    major, minor, patch = map(int, version.split("."))
    for tag in (f"v{major}.{minor}.{patch + 1}", version + "-beta", "latest", f"v{major}.{minor}"):
        rejected(ROOT, tag)

    # A changelog without the tagged version must not produce a draft.
    with tempfile.TemporaryDirectory() as directory:
        copy = Path(directory)
        for path in ("CHANGELOG.md", "docs/player/CHANGELOG.zh-CN.md"):
            (copy / path).parent.mkdir(parents=True, exist_ok=True)
            text = (ROOT / path).read_text(encoding="utf-8")
            (copy / path).write_text(text.replace(f"## {version} ", "## 9.9.9 ", 1), encoding="utf-8")
        for path in ("src", "docs/player", "Repository.json", "LICENSE", "LICENSE-EXCEPTION.md"):
            source, target = ROOT / path, copy / path
            if source.is_dir():
                shutil.copytree(source, target, dirs_exist_ok=True,
                                ignore=lambda folder, names: [n for n in names if n.startswith("CHANGELOG")])
            else:
                shutil.copyfile(source, target)
        rejected(copy, f"v{version}")
    print(f"Draft release notes passed for v{version}: bilingual sections, download name and 5 rejected tags/changelogs.")


if __name__ == "__main__":
    main()
