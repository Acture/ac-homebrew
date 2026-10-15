"""Validate final text, all feat icon mappings, blueprint identity, and release contents."""
import argparse
import collections
import hashlib
import json
import re
import struct
import zipfile
from pathlib import Path
from feat_catalog import ROOT, PROJECT, catalog, calls, split_args, source_file
from release_delivery import LOCALES, check_sources, check_archive


def read(path):
    return path.read_text(encoding="utf-8-sig")


def normalize(expr):
    return re.sub(r'\s+', '', expr)


def mechanics(source):
    # Compare actual configurator mutations, excluding UI text and artwork.
    results = []
    for match in re.finditer(r'\.(Add\w+|Set\w+)\(', source):
        method = match.group(1)
        if method in ('SetDisplayName', 'SetDescription', 'SetIconIfPresent'):
            continue
        body = next(calls(source[match.start():], method))[0]
        results.append((method, normalize(body)))
    # NewFeat passes the same name/GUID to FeatureConfigurator.New. Its menu
    # registration is covered separately by Initialization.Tests.
    source = re.sub(r'\bNewFeat\(', 'New(', source)
    for body, _, _ in calls(source, 'New'):
        args = split_args(body)
        if len(args) >= 2:
            results.append(('New', normalize(','.join(args[:2]))))
    return results


def check_release_archive(path, root=ROOT):
    manifest = json.loads(read(root / 'docs/icon-manifest.json'))
    with zipfile.ZipFile(path) as archive:
        check_archive(archive, root)
        names = archive.namelist()
        for name in names:
            parts = name.rstrip('/').split('/')
            assert '\\' not in name and all(part not in ('', '.', '..') and ':' not in part
                                           for part in parts), f'unsafe ZIP path: {name}'
        icons = {name for name in names if name.lower().endswith('.png')}
        assert icons == {'Icons/' + asset['filename'] for asset in manifest}, 'release icon coverage'
        assert {name for name in names if name.lower().endswith('.dll')} == {'ACHomebrew.dll'}, 'only the root mod DLL may ship'
        assert json.loads(archive.read('Info.json')) == json.loads(read(root / 'src/ACHomebrew/Info.json'))
        assert not any(name.split('/')[0] in ('notes', 'doc', 'docs', 'tests', 'scripts') for name in names), 'contributor files must not ship in the Mod ZIP'
        for asset in manifest:
            data = archive.read('Icons/' + asset['filename'])
            assert data == (root / 'src/ACHomebrew/Icons' / asset['filename']).read_bytes()
            assert hashlib.sha256(data).hexdigest() == asset['sha256'], asset['filename']


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--allow-pending-icons', action='store_true')
    parser.add_argument('--release', type=Path)
    args = parser.parse_args()
    rows = catalog()
    _, entries = check_sources()
    resources = list(entries.values())
    for entry in resources:
        assert all(entry[locale].strip() for locale in LOCALES), entry['Key']
    for row in rows:
        assert entries[row['key'] + '.Name']['enGB'] == row['en'], row['internal']
        assert entries[row['key'] + '.Name']['zhCN'] == row['zh'], row['internal']
        assert row['key'] + '.Lore' in entries
    for lang in ['en', 'zh']:
        names = [r[lang].casefold() for r in rows]
        assert len(set(names)) == 92, f'duplicate {lang} feat name'

    baseline = json.loads(read(ROOT / 'tests/baselines/published-mechanics.json'))
    assert set(baseline['files']) == {row['file'] for row in rows}, 'published family coverage'
    for filename, expected in baseline['files'].items():
        path = source_file(filename)
        actual = [list(call) for call in mechanics(read(path))]
        assert actual == expected['mechanics'], f'published mechanics changed: {path.name}'
        # Includes all inline GUIDs and component TypeId identities.
        pattern = r'"[0-9a-fA-F-]{32,36}"'
        assert re.findall(pattern, read(path)) == expected['inlineIds'], path.name
    # Named blueprint GUID compatibility is checked by Test-RepositoryContracts.ps1.

    manifest = json.loads(read(ROOT / 'docs/icon-manifest.json'))
    assert len(manifest) == len({r['internal'] for r in manifest}) == len({r['filename'] for r in manifest})
    # The 92 catalog feats come first; newer feats follow with curated names.
    assert {r['internal'] for r in manifest[:92]} == {r['internal'] for r in rows}
    aliases = dict(re.findall(r'\{ "([^"]+)", "([^"]+)" \}', read(source_file('IconLoader.cs'))))
    pending = []
    hashes = []
    for asset in manifest:
        assert set(asset) == {'internal', 'family', 'filename', 'nameEn', 'nameZh', 'sha256'}, 'public manifest fields'
        row = next((row for row in rows if row['internal'] == asset['internal']), None)
        if row is not None:
            assert (asset['nameEn'], asset['nameZh']) == (row['en'], row['zh']), asset['internal']
        path = PROJECT/'Icons'/asset['filename']
        if not path.exists():
            pending.append(asset['filename']); continue
        data = path.read_bytes()
        assert data[:8] == b'\x89PNG\r\n\x1a\n' and struct.unpack('>II',data[16:24]) == (128,128), path
        digest = hashlib.sha256(data).hexdigest()
        assert asset['sha256'] == digest, path
        hashes.append(digest)
        assert aliases.get(asset['internal'],asset['internal']) == path.stem, asset['internal']
    if not args.allow_pending_icons:
        assert not pending, f'missing icons: {pending}'
        assert len(set(hashes)) == len(manifest), 'duplicate icon images'
        assert {p.name for p in (PROJECT/'Icons').glob('*.png')} == {r['filename'] for r in manifest}, 'extra icon files'
    if args.release:
        assert not pending
        check_release_archive(args.release)
    print(f'{len(entries)} entries in {len(LOCALES)} languages, player delivery files, 92 unique existing feat names, published component calls/inline IDs and {len(manifest)-len(pending)}/{len(manifest)} icons verified (static checks only).')
    if args.release: print(f'Release ZIP verified: {args.release}')


if __name__ == '__main__':
    main()
