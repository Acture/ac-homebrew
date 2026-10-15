"""Read the literal feat definitions for the existing feats and their icon mappings."""
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
PROJECT = ROOT / "src/ACHomebrew"
STRING = re.compile(r'"(?:\\.|[^"\\])*"')


def split_args(text):
    result, start, depth, quoted, escaped = [], 0, 0, False, False
    for i, char in enumerate(text):
        if quoted:
            if escaped:
                escaped = False
            elif char == "\\":
                escaped = True
            elif char == '"':
                quoted = False
        elif char == '"':
            quoted = True
        elif char in "([{":
            depth += 1
        elif char in ")]}":
            depth -= 1
        elif char == "," and depth == 0:
            result.append(text[start:i].strip())
            start = i + 1
    result.append(text[start:].strip())
    return result


def calls(source, method):
    for match in re.finditer(r'\b' + re.escape(method) + r'\(', source):
        start, i, depth, quoted, escaped = match.end(), match.end(), 1, False, False
        while depth:
            char = source[i]
            if quoted:
                if escaped:
                    escaped = False
                elif char == "\\":
                    escaped = True
                elif char == '"':
                    quoted = False
            elif char == '"':
                quoted = True
            elif char == "(":
                depth += 1
            elif char == ")":
                depth -= 1
            i += 1
        yield source[start:i - 1], start, i - 1


def literal(expr):
    return json.loads(expr)


NAME = re.compile(r'^(?:\w+:\s*)?LocalizedText\.Text\(("[^"]+\.Name"), ("(?:\\.|[^"\\])*")\)$')


def named(args):
    return dict(arg.split(":", 1) for arg in args if re.match(r'^\w+:', arg))


def source_files():
    """C# sources of every mod project under src/, excluding build output."""
    return [p for p in (ROOT / "src").glob("*/*.cs") if "obj" not in p.parts and "bin" not in p.parts]


def source_file(name):
    matches = [p for p in source_files() if p.name == name]
    assert len(matches) == 1, f"expected one source named {name}, found {matches}"
    return matches[0]


def catalog(sources=None):
    """The 92 original feats: source file, family, blueprint name, text key and English/Chinese names.

    English comes from the compiled fallback beside the name key; Chinese from the canonical zhCN catalog.
    """
    sources = sources or {p.name: p.read_text(encoding="utf-8-sig") for p in source_files()}
    chinese = json.loads((ROOT / "src/ACHomebrew.Core/Localization/zhCN.json").read_text(encoding="utf-8-sig"))
    records = []

    def add(file, family, internal, name_key, english):
        key = name_key.removesuffix(".Name")
        records.append(dict(file=file, family=family, internal=internal, key=key,
                            en=english, zh=chinese[name_key]))

    specs = [
        ("AttributeFeats.cs", "Main", "CreateOne"),
        ("StatReplacementFeats.cs", "Weapon Insight", "CreateWeaponInsight"),
        ("StatReplacementFeats.cs", "Extended", "CreateExtendedFeat"),
        ("SpecializedFeats.cs", None, "CreateSpecialized"),
        ("StanceFeats.cs", "Stance", "CreateStance"),
        ("ConditionalFeats.cs", "Conditional", "NewFeature"),
        ("DerivedStatFeats.cs", "Derived", "NewFeature"),
        ("GreaterSummoningFeats.cs", "Summon", "CreateFeat"),
        ("SummonerSacrificeFeats.cs", "Sacrifice", "CreateFeat"),
        ("DistanceDamageFeats.cs", "Distance", "CreateFeat"),
        ("SpellTagFeats.cs", "School", "new SchoolFeatDefinition"),
        ("SpellTagFeats.cs", "Descriptor", "new DescriptorFeatDefinition"),
    ]
    for file, family, method in specs:
        for body, _, _ in calls(sources[file], method):
            args = split_args(body)
            fields = {k: v.strip() for k, v in named(args).items()}
            if "internalName" in fields:
                internal = fields["internalName"]
            else:
                internal = next((arg for arg in args if STRING.fullmatch(arg) and "." not in arg), None)
            names = [match for match in map(NAME.match, args) if match]
            if internal is None or not internal.startswith('"') or len(names) != 1:
                continue  # helper declarations and non-catalog calls
            group = family or args[0].split(".")[-1]
            add(file, group, literal(internal), literal(names[0].group(1)), literal(names[0].group(2)))

    for file, family, items in [
        ("ReactiveArmorFeats.cs", "Reactive Armor", [("SpikedDefense", "ReactiveArmor_SpikedDefense"), ("BulwarkOfSteel", "ReactiveArmor_BulwarkOfSteel")]),
        ("PolearmMasterFeats.cs", "Reach", [("PolearmMaster", "PolearmMaster")]),
    ]:
        for internal, prefix in items:
            body = next(body for body, _, _ in calls(sources[file], "Common.L") if body.startswith(f'"{prefix}.Name"'))
            match = NAME.match(split_args(body)[1])
            add(file, family, internal, literal(match.group(1)), literal(match.group(2)))

    assert len(records) == 92, f"Expected 92 feats, found {len(records)}"
    assert len({r["internal"] for r in records}) == 92
    return records


if __name__ == "__main__":
    print(json.dumps(catalog(), ensure_ascii=False, indent=2))
