"""Prepare an existing GitHub release ZIP for a separately approved Nexus upload."""
import argparse
import hashlib
import json
import shutil
import sys
import uuid
import zipfile
from pathlib import Path

from release_delivery import REQUIRED, parse_json
from validate_assets import check_release_archive


def version_from_tag(tag):
    version = tag.removeprefix("v")
    parts = version.split(".")
    if len(parts) != 3 or not all(part.isascii() and part.isdigit() and str(int(part)) == part for part in parts):
        raise ValueError("Release tag must be X.Y.Z or vX.Y.Z with numeric version components.")
    return version


def release_url(repository, tag):
    parts = repository.split("/")
    if len(parts) != 2 or not all(part and part not in (".", "..") and
                                all(char.isascii() and (char.isalnum() or char in "-_.") for char in part)
                                for part in parts):
        raise ValueError("Repository must be owner/name.")
    return f"https://github.com/{repository}/releases/tag/{tag}"


def valid_digest(value):
    return isinstance(value, str) and len(value) == 64 and all(char in "0123456789abcdef" for char in value)


def file_digest(path):
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def inspect_release(metadata, tag, repository):
    version = version_from_tag(tag)
    url = release_url(repository, tag)
    if not isinstance(metadata, dict):
        raise ValueError("GitHub release metadata must be an object.")
    if metadata.get("tag_name") != tag or metadata.get("html_url") != url:
        raise ValueError("GitHub release tag or repository URL does not match the requested release.")
    published = metadata.get("published_at")
    if metadata.get("draft") is not False or not isinstance(published, str) or not published.strip():
        raise ValueError("GitHub release must be published, not a draft.")
    filename = f"ACHomebrew-{version}.zip"
    assets = metadata.get("assets")
    if not isinstance(assets, list) or not all(isinstance(asset, dict) for asset in assets):
        raise ValueError("GitHub release assets must be a list of objects.")
    matches = [asset for asset in assets if asset.get("name") == filename]
    if len(matches) != 1 or matches[0].get("state") != "uploaded":
        raise ValueError(f"Release must contain exactly one uploaded ZIP asset named {filename}.")
    asset = matches[0]
    digest = asset.get("digest")
    if not isinstance(digest, str) or not digest.startswith("sha256:") or not valid_digest(digest[7:]):
        raise ValueError("GitHub asset must provide a valid SHA-256 digest.")
    if any(type(asset.get(key)) is not int or asset[key] <= 0 for key in ("id", "size")):
        raise ValueError("GitHub asset ID and size must be positive integers.")
    return {"version": version, "filename": filename, "asset_id": asset["id"], "size": asset["size"],
            "sha256": digest[7:], "release_url": url}


def changelog_section(path, version):
    lines = path.read_text(encoding="utf-8-sig").splitlines()
    section = None
    for index, line in enumerate(lines):
        if not line.startswith("## "):
            continue
        if section is not None:
            break
        heading = line.split(maxsplit=2)
        if len(heading) >= 2 and heading[1] == version:
            section = index
    else:
        index = len(lines)
    if section is None or not any(line.strip() for line in lines[section + 1:index]):
        raise ValueError(f"No nonempty changelog section for {version} in {path.name}.")
    return "\n".join(lines[section:index]).strip()


def verify_package(package, manifest, expected_sha256):
    if package.name != manifest["filename"]:
        raise ValueError("Package filename does not match the release ZIP.")
    if not valid_digest(expected_sha256) or manifest["sha256"] != expected_sha256:
        raise ValueError("Prepared SHA-256 does not match the expected SHA-256.")
    if package.stat().st_size != manifest["size"]:
        raise ValueError("ZIP size does not match the GitHub release asset.")
    if file_digest(package) != expected_sha256:
        raise ValueError("ZIP SHA-256 does not match the GitHub release asset.")


def prepare_release(metadata, tag, repository, source_root, package, output_directory):
    manifest = inspect_release(metadata, tag, repository)
    verify_package(package, manifest, manifest["sha256"])
    check_release_archive(package, source_root)
    if parse_json((source_root / REQUIRED["Info.json"]).read_bytes())["Version"] != manifest["version"]:
        raise ValueError("Tagged source version does not match the GitHub release.")
    english = changelog_section(source_root / "CHANGELOG.md", manifest["version"])
    chinese = changelog_section(source_root / "docs/player/CHANGELOG.zh-CN.md", manifest["version"])
    manifest["changelog"] = f"English\n{english}\n\nSimplified Chinese\n{chinese}"
    if output_directory.exists() and any(output_directory.iterdir()):
        raise ValueError("Preparation output directory must be empty.")
    output_directory.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(package, output_directory / manifest["filename"])
    (output_directory / "nexus-upload.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    return manifest


def upload_inputs(directory, tag, repository, expected_sha256):
    manifest = parse_json((directory / "nexus-upload.json").read_bytes())
    version = version_from_tag(tag)
    required = {"version", "filename", "asset_id", "size", "sha256", "release_url", "changelog"}
    if not isinstance(manifest, dict) or manifest.keys() != required:
        raise ValueError("Prepared manifest must contain exactly the expected upload fields.")
    if manifest.get("filename") != f"ACHomebrew-{version}.zip":
        raise ValueError("Prepared filename does not match the requested release ZIP.")
    if manifest.get("version") != version or manifest.get("release_url") != release_url(repository, tag):
        raise ValueError("Prepared release version or URL does not match.")
    if not isinstance(manifest.get("changelog"), str) or not manifest["changelog"].strip():
        raise ValueError("Prepared changelog is missing.")
    package = directory / manifest["filename"]
    verify_package(package, manifest, expected_sha256)
    return {**manifest, "package_path": str(package.resolve())}


def require_reviewers(environment):
    rules = environment.get("protection_rules") if isinstance(environment, dict) else None
    if not isinstance(rules, list) or not all(isinstance(rule, dict) for rule in rules):
        raise ValueError("Environment metadata must list its reviewer protection rules.")
    if not any(rule.get("type") == "required_reviewers" and isinstance(rule.get("reviewers"), list)
               and rule["reviewers"] for rule in rules):
        raise ValueError("Configure required reviewers on the nexus-publish environment before uploading.")


def write_outputs(path, values):
    with path.open("a", encoding="utf-8") as output:
        for name, value in values.items():
            value = str(value)
            if "\n" not in value and "\r" not in value:
                output.write(f"{name}={value}\n")
                continue
            delimiter = uuid.uuid4().hex
            while delimiter in value.splitlines():
                delimiter = uuid.uuid4().hex
            output.write(f"{name}<<{delimiter}\n{value}\n{delimiter}\n")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    commands = parser.add_subparsers(dest="command", required=True)
    for command in ("metadata", "prepare", "upload-inputs"):
        options = commands.add_parser(command)
        options.add_argument("--tag", required=True)
        options.add_argument("--repository", required=True)
        options.add_argument("--github-output", type=Path)
        if command != "upload-inputs":
            options.add_argument("--metadata", type=Path, required=True)
        if command == "prepare":
            options.add_argument("--source-root", type=Path, required=True)
            options.add_argument("--package", type=Path, required=True)
            options.add_argument("--output-directory", type=Path, required=True)
            options.add_argument("--summary", type=Path)
        if command == "upload-inputs":
            options.add_argument("--directory", type=Path, required=True)
            options.add_argument("--sha256", required=True)
    commands.add_parser("check-environment")
    args = parser.parse_args()
    try:
        if args.command == "check-environment":
            require_reviewers(parse_json(sys.stdin.read()))
            print("Nexus environment has required reviewers.")
            return 0
        if args.command == "upload-inputs":
            result = upload_inputs(args.directory, args.tag, args.repository, args.sha256)
        else:
            metadata = parse_json(args.metadata.read_bytes())
            result = inspect_release(metadata, args.tag, args.repository) if args.command == "metadata" else prepare_release(
                metadata, args.tag, args.repository, args.source_root.resolve(), args.package, args.output_directory)
        if args.github_output:
            write_outputs(args.github_output, result)
        if args.command == "prepare" and args.summary:
            args.summary.write_text(
                f"## Prepared Nexus upload\n\nSource: {result['release_url']}\n\n"
                f"ZIP: `{result['filename']}`\n\nSHA-256: `{result['sha256']}`\n\n"
                f"Nothing has been uploaded by preparation. Review the release wording before approving.\n\n"
                f"### Upload wording\n\n{result['changelog']}\n", encoding="utf-8")
        label = "Selected" if args.command == "metadata" else "Verified"
        print(f"{label} {result['filename']}: SHA-256 {result['sha256']}")
        return 0
    except (AssertionError, ValueError, KeyError, TypeError, OSError, zipfile.BadZipFile) as error:
        print(f"Nexus preparation failed: {error}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    sys.exit(main())
