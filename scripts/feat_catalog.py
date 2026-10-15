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
    # Common.Text keeps the original literal as an emergency fallback.
    if expr.startswith("Common.Text("):
        return literal(split_args(expr[len("Common.Text("):-1])[1])
    return json.loads(expr)


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
    sources = sources or {p.name: p.read_text(encoding="utf-8-sig") for p in source_files()}
    records = []

    def add(file, family, internal, prefix, en, zh, lore_en, lore_zh):
        records.append(dict(file=file, family=family, internal=internal, key=prefix,
                            en=en, zh=zh, lore_en=lore_en, lore_zh=lore_zh))

    specs = [
        ("AttributeFeats.cs", "Main", "CreateOne", 1, 3, 4, 5, 7, 3),
        ("StatReplacementFeats.cs", "Weapon Insight", "CreateWeaponInsight", 1, 3, 4, 5, 7, 2),
        ("StatReplacementFeats.cs", "Extended", "CreateExtendedFeat", 2, 4, 5, 6, 8, 2),
    ]
    for file, family, method, internal_i, key_i, en_i, zh_i, desc_i, lore_i in specs:
        for body, _, _ in calls(sources[file], method):
            args = split_args(body)
            if not args[internal_i].startswith('"'):
                continue
            desc = args[desc_i]
            desc_args = split_args(desc[desc.index("(") + 1:-1])
            if family == "Main":
                lore_en = literal(desc_args[2]) + " " + literal(desc_args[3])
                lore_zh = literal(desc_args[4]) + " " + literal(desc_args[5])
            else:
                lore_en, lore_zh = map(literal, desc_args[lore_i:lore_i + 2])
            add(file, family, literal(args[internal_i]), literal(args[key_i]).removesuffix(".Name"),
                literal(args[en_i]), literal(args[zh_i]), lore_en, lore_zh)

    file = "SpecializedFeats.cs"
    attrs = {"Strength": "Str", "Dexterity": "Dex", "Constitution": "Con", "Intelligence": "Int", "Wisdom": "Wis", "Charisma": "Cha"}
    for body, _, _ in calls(sources[file], "CreateSpecialized"):
        args = split_args(body)
        if not args[4].startswith('"'):
            continue
        family, attr = args[0].split(".")[-1], attrs[args[1].split(".")[-1]]
        add(file, family, literal(args[4]), family + "_" + attr,
            *map(literal, args[5:9]))

    specs = [
        ("StanceFeats.cs", "Stance", "CreateStance", "displayNameEn", "displayNameZh", "description", 3),
        ("ConditionalFeats.cs", "Conditional", "NewFeature", "nameEn", "nameZh", "desc", 2),
        ("DerivedStatFeats.cs", "Derived", "NewFeature", "nameEn", "nameZh", "desc", 2),
        ("GreaterSummoningFeats.cs", "Summon", "CreateFeat", "nameEn", "nameZh", "desc", 2),
        ("SummonerSacrificeFeats.cs", "Sacrifice", "CreateFeat", "nameEn", "nameZh", "desc", 2),
        ("DistanceDamageFeats.cs", "Distance", "CreateFeat", "nameEn", "nameZh", "desc", 2),
    ]
    for file, family, method, en_field, zh_field, desc_field, lore_i in specs:
        for body, _, _ in calls(sources[file], method):
            fields = {k: v.strip() for k, v in named(split_args(body)).items()}
            if "internalName" not in fields:
                continue
            internal = literal(fields["internalName"])
            if family == "Stance":
                prefix = literal(fields["keyPrefix"])
            elif "nameKey" in fields:
                prefix = literal(fields["nameKey"]).removesuffix(".Name")
            else:
                prefix = {"Summon": "Summon_", "Sacrifice": "SummonerSacrifice_", "Distance": "DistanceDamage_"}[family] + internal
            desc = fields[desc_field]
            desc_args = split_args(desc[desc.index("(") + 1:-1])
            desc_fields = {k: v.strip() for k, v in named(desc_args).items()}
            if desc_fields:
                lore_en, lore_zh = literal(desc_fields["loreEn"]), literal(desc_fields["loreZh"])
            elif family == "Stance":
                lore_en = literal(desc_args[2]) + " " + literal(desc_args[3])
                lore_zh = literal(desc_args[4]) + " " + literal(desc_args[5])
            else:
                lore_en, lore_zh = map(literal, desc_args[lore_i:lore_i + 2])
            add(file, family, internal, prefix, literal(fields[en_field]), literal(fields[zh_field]), lore_en, lore_zh)

    file = "SpellTagFeats.cs"
    for method, family, key_part in [("SchoolFeatDefinition", "School", "School"), ("DescriptorFeatDefinition", "Descriptor", "Descriptor")]:
        for body, _, _ in calls(sources[file], method):
            fields = {k: v.strip() for k, v in named(split_args(body)).items()}
            if "internalName" not in fields:
                continue
            internal = literal(fields["internalName"])
            add(file, family, internal, f"SpellTag_{key_part}_{internal}",
                *[literal(fields[k]) for k in ["nameEn", "nameZh", "loreEn", "loreZh"]])

    for file, family, items in [
        ("ReactiveArmorFeats.cs", "Reactive Armor", [("SpikedDefense", "BuildSpikedDefenseDescription"), ("BulwarkOfSteel", "BuildBulwarkOfSteelDescription")]),
        ("PolearmMasterFeats.cs", "Reach", [("PolearmMaster", "BuildDescription")]),
    ]:
        source = sources[file]
        for internal, builder in items:
            prefix = "ReactiveArmor_" + internal if family == "Reactive Armor" else internal
            name_call = next(body for body, _, _ in calls(source, "Common.L") if body.startswith('"' + prefix + '.Name"'))
            args = split_args(name_call)
            lore = []
            resource_calls = [split_args(body) for body, _, _ in calls(source, "Common.Text")]
            for chinese in [False, True]:
                lore_args = next(values for values in resource_calls
                                 if literal(values[0]) == prefix + ".Lore"
                                 and (len(values) > 2 and values[2] == "true") == chinese)
                lore.append(literal(lore_args[1]))
            add(file, family, internal, prefix, literal(args[1]), literal(args[2]), *lore)

    assert len(records) == 92, f"Expected 92 feats, found {len(records)}"
    assert len({r["internal"] for r in records}) == 92
    return records


if __name__ == "__main__":
    print(json.dumps(catalog(), ensure_ascii=False, indent=2))
