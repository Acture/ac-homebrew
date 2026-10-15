"""Run positive and failure controls for delivery checks without game files."""
import io
import zipfile
from release_delivery import ROOT, REQUIRED, LOCALES, check_sources, check_archive, parse_json, language_entries


def archive(overrides=None, omit=None):
    buffer = io.BytesIO()
    with zipfile.ZipFile(buffer, "w") as output:
        for name, source in REQUIRED.items():
            if name != omit:
                output.writestr(name, (overrides or {}).get(name, (ROOT / source).read_bytes()))
    return zipfile.ZipFile(buffer)


def rejected(build):
    try:
        with build() as package:
            check_archive(package)
    except (AssertionError, ValueError):
        return
    raise AssertionError("Failure control unexpectedly accepted")


def main():
    version, entries = check_sources()
    with archive() as package:
        check_archive(package)
    for name in REQUIRED:
        rejected(lambda: archive(omit=name))
    for invalid in ('{', '[]', '{"key":"one","key":"two"}'):
        rejected(lambda: archive({"Localization/zhCN.json": invalid}))
    english = parse_json((ROOT / REQUIRED["Localization/enGB.json"]).read_bytes())
    chinese = parse_json((ROOT / REQUIRED["Localization/zhCN.json"]).read_bytes())
    for replacement in ("", "missing {9}", "<b>{0}</i>", "{g|Encyclopedia:Other}label{/g}"):
        broken = dict(chinese)
        broken["FeatBudget.PointsShort"] = replacement
        try:
            language_entries({"enGB": english, "zhCN": broken})
        except (AssertionError, ValueError):
            continue
        raise AssertionError("Invalid translation accepted")
    rejected(lambda: archive({"Info.json": '{"Version":"0.0.0"}'}))
    rejected(lambda: archive({"README.en.md": "outdated guide"}))
    print(f"Player delivery contracts passed: version {version}, {len(entries)} keys in {len(LOCALES)} languages; "
          f"{len(REQUIRED)} missing-file and 9 malformed/version/content failure controls.")


if __name__ == "__main__":
    main()
