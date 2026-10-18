"""Check a pushed version tag against its sources and write the draft GitHub Release notes."""
import argparse
import sys
from pathlib import Path

from prepare_nexus_release import changelog_section, version_from_tag, write_outputs
from release_delivery import check_sources


def release_notes(root, tag):
    version = version_from_tag(tag)
    source_version, _ = check_sources(root)
    if source_version != version:
        raise ValueError(f"Tag {tag} does not match the source version {source_version}.")
    sections = []
    for title, path in (("English", "CHANGELOG.md"), ("简体中文", "docs/player/CHANGELOG.zh-CN.md")):
        heading, _, body = changelog_section(root / path, version).partition("\n")
        sections.append(f"## {title}\n\n{body.strip()}")
    title = f"AC's Homebrew {version}"
    notes = "\n\n".join(sections) + (
        f"\n\nDownload `ACHomebrew-{version}.zip` and extract it into the game's `Mods/ACHomebrew` folder,"
        " or install it with Unity Mod Manager.\n")
    return {"version": version, "title": title, "notes": notes}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--tag", required=True)
    parser.add_argument("--source-root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--notes-file", type=Path, required=True)
    parser.add_argument("--github-output", type=Path)
    args = parser.parse_args()
    try:
        result = release_notes(args.source_root.resolve(), args.tag)
        args.notes_file.write_text(result["notes"], encoding="utf-8")
        if args.github_output:
            write_outputs(args.github_output, {"version": result["version"], "title": result["title"]})
        print(f"Release notes for {result['title']} written to {args.notes_file}")
        return 0
    except (AssertionError, ValueError, KeyError, OSError) as error:
        print(f"Release draft check failed: {error}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    sys.exit(main())
