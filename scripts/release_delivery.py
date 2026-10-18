"""Shared contracts for player materials and canonical language files."""
import json
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
# Game locales with a shipped language file; English first (the fallback for every other locale).
LOCALES = ("enGB", "zhCN", "deDE", "frFR", "esES", "itIT", "ptBR", "ruRU")
REQUIRED = {
    "README.en.md": "docs/player/README.en.md",
    "README.zh-CN.md": "docs/player/README.zh-CN.md",
    "CHANGELOG.md": "CHANGELOG.md",
    "CHANGELOG.zh-CN.md": "docs/player/CHANGELOG.zh-CN.md",
    "LICENSE": "LICENSE",
    "LICENSE-EXCEPTION.md": "LICENSE-EXCEPTION.md",
    **{f"Localization/{locale}.json": f"src/ACHomebrew.Core/Localization/{locale}.json" for locale in LOCALES},
    "Info.json": "src/ACHomebrew/Info.json",
}


def unique_object(pairs):
    result = {}
    for key, value in pairs:
        assert key not in result, f"duplicate JSON key: {key}"
        result[key] = value
    return result


def parse_json(data):
    return json.loads(data.decode("utf-8-sig") if isinstance(data, bytes) else data,
                      object_pairs_hook=unique_object)


def signature(text):
    """Validate .NET composite arguments, balanced rich text and game links."""
    tokens, stack = set(), []
    i = 0
    while i < len(text):
        c = text[i]
        if c == "{":
            if text[i:i + 2] == "{{":
                i += 2
                continue
            end = text.find("}", i + 1)
            assert end >= 0, "unclosed placeholder/link"
            value = text[i + 1:end]
            if value.startswith(("g|", "d|")):
                tokens.add("link:" + value)
                stack.append("{" + value[0])
            elif value in ("/g", "/d"):
                assert stack and stack.pop() == "{" + value[1], "unbalanced game link"
            else:
                field, _, fmt = value.partition(":")
                index, _, alignment = field.partition(",")
                assert index.isascii() and index.isdigit() and 0 <= int(index) < 64, "invalid argument"
                if alignment:
                    int(alignment)
                assert "{" not in fmt and "}" not in fmt, "invalid format specifier"
                tokens.add("argument:" + value)
            i = end + 1
        elif c == "}":
            assert text[i:i + 2] == "}}", "unmatched closing brace"
            i += 2
        elif c == "<":
            end = text.find(">", i + 1)
            # Less-than inequalities in prose are not rich-text tags.
            if end < 0:
                assert i + 1 >= len(text) or not text[i + 1].isalpha(), "unclosed rich-text tag"
                i += 1
                continue
            tag = text[i + 1:end]
            if not tag or not (tag[0].isalpha() or tag[0] == "/"):
                i += 1
                continue
            tokens.add("tag:" + tag)
            if tag.startswith("/"):
                assert stack and stack.pop() == tag[1:], "unbalanced rich-text tag"
            elif not tag.endswith("/"):
                stack.append(tag.partition("=")[0])
            i = end + 1
        else:
            i += 1
    assert not stack, "unclosed rich text/link"
    return tokens


def language_entries(packs):
    """Check {locale: language file} against English; return {key: {"Key", locale: text...}}."""
    assert all(isinstance(pack, dict) for pack in packs.values()), "language files must be JSON objects"
    english = packs["enGB"]
    assert english, "English language file is empty"
    for locale, pack in packs.items():
        assert pack.keys() == english.keys(), f"language key coverage differs: {locale}"
    for key in english:
        assert key.strip() and all(isinstance(pack[key], str) and pack[key].strip()
                                   for pack in packs.values()), f"blank/invalid translation: {key}"
        expected = signature(english[key])
        for locale, pack in packs.items():
            assert signature(pack[key]) == expected, f"placeholder/markup/link mismatch: {locale} {key}"
    return {key: {"Key": key, **{locale: pack[key] for locale, pack in packs.items()}} for key in english}


def check_changelog(text, version, name):
    headings = [line for line in text.splitlines() if line.startswith("## ")]
    if headings and headings[0] in ("## Unreleased", "## 尚未发布"):
        headings = headings[1:]
    assert headings and headings[0].startswith(f"## {version} "), f"outdated changelog: {name}"


def check_sources(root=ROOT):
    version = parse_json((root / REQUIRED["Info.json"]).read_bytes())["Version"]
    project_path = root / "src/ACHomebrew/ACHomebrew.csproj"
    project = ET.parse(project_path).getroot()
    for name in ("Version", "AssemblyVersion", "FileVersion"):
        assert project.findtext(f"./PropertyGroup/{name}") == version, f"{name} differs from Info.json"
    releases = parse_json((root / "Repository.json").read_bytes())["Releases"]
    assert any(r["Id"] == "ACHomebrew" and r["Version"] == version for r in releases), "repository version"
    outputs = {}
    for item in project.findall("./ItemGroup/None"):
        if item.get("CopyToOutputDirectory") not in ("Always", "PreserveNewest"):
            continue
        for include in (item.get("Include") or item.get("Update", "")).split(";"):
            pattern = project_path.parent / include
            paths = sorted(pattern.parent.glob(pattern.name)) if "*" in pattern.name else [pattern]
            for path in (path.resolve() for path in paths):
                link = item.get("Link", "").replace("%(Filename)%(Extension)", path.name)
                outputs[link or path.name] = path
    for destination, source in REQUIRED.items():
        assert outputs.get(destination) == (root / source).resolve(), f"not copied by packaging project: {destination}"
        assert (root / source).is_file(), f"missing player delivery source: {source}"
    for name in ("README.en.md", "README.zh-CN.md"):
        text = (root / REQUIRED[name]).read_text(encoding="utf-8")
        assert version in text.splitlines()[0], f"outdated player guide: {name}"
        assert f"ACHomebrew-{version}.zip" in text, f"outdated download name: {name}"
        assert "Validation scope" not in text and "tests/" not in text, f"developer report in player guide: {name}"
    for name in ("CHANGELOG.md", "CHANGELOG.zh-CN.md"):
        check_changelog((root / REQUIRED[name]).read_text(encoding="utf-8"), version, name)
    entries = language_entries({locale: parse_json((root / REQUIRED[f"Localization/{locale}.json"]).read_bytes())
                                for locale in LOCALES})
    return version, entries


def check_archive(archive, root=ROOT):
    version, _ = check_sources(root)
    names = archive.namelist()
    assert len(names) == len(set(names)), "duplicate ZIP paths"
    assert REQUIRED.keys() <= set(names), f"missing player delivery files: {REQUIRED.keys() - set(names)}"
    metadata = parse_json(archive.read("Info.json"))
    assert metadata["Version"] == version, "archive metadata version"
    assert archive.filename is None or Path(archive.filename).name == f"ACHomebrew-{version}.zip", "release ZIP version"
    language_entries({locale: parse_json(archive.read(f"Localization/{locale}.json")) for locale in LOCALES})
    for destination, source in REQUIRED.items():
        # REQUIRED contains text only. Git may check it out as CRLF on Windows and LF on Linux.
        # Compare that content without rewriting the ZIP; its full SHA-256 is verified separately.
        delivered = archive.read(destination).replace(b"\r\n", b"\n")
        expected = (root / source).read_bytes().replace(b"\r\n", b"\n")
        assert delivered == expected, f"archive differs from build inputs: {destination}"
