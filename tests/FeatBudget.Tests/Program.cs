using ACHomebrew.Feats;

// Pure rule checks. They model the level-up preview, which Wrath rebuilds by
// replaying pending choices in order and dropping any that no longer pass.
var failures = 0;
var cases = 0;
void Check(string name, Action test)
{
    cases++;
    try { test(); }
    catch (Exception error)
    {
        failures++;
        Console.Error.WriteLine($"FAIL {name}: {error.Message}");
    }
}

void Expect(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

FeatBudgetLimits Count(int max) => new(true, max, false, 0);
FeatBudgetLimits Points(int max) => new(false, 0, true, max);
FeatBudgetLimits Both(int count, int points) => new(true, count, true, points);
FeatBudgetUsage Owned(params string[] families)
    => FeatBudgetRules.Tally(families.Select(family => (FeatBudgetRules.FamilyCost(family), 1)));

Check("disabled limits never block", () =>
{
    var verdict = FeatBudgetRules.Evaluate(default, Owned("MainAttributeLegacy", "Stance", "Stance", "WeaponDamage"), 3);
    Expect(verdict.Allowed && !verdict.OverBudget, "A disabled budget blocked a feat");
});

Check("count limit can be used exactly", () =>
{
    Expect(FeatBudgetRules.Evaluate(Count(3), Owned("Defensive", "Stance"), 2).Allowed, "Third of three feats was refused");
    var full = FeatBudgetRules.Evaluate(Count(3), Owned("Defensive", "Stance", "Skilled"), 1);
    Expect(full.Block == FeatBudgetBlock.CountFull && full.RemainingCount == 0 && !full.OverBudget, "Fourth of three feats was not refused as full");
});

Check("point limit can be used exactly", () =>
{
    var exact = FeatBudgetRules.Evaluate(Points(6), Owned("MainAttributeLegacy", "Defensive"), 2);
    Expect(exact.Allowed, "A feat using the last two points was refused");
    var short1 = FeatBudgetRules.Evaluate(Points(6), Owned("MainAttributeLegacy", "Defensive"), 3);
    Expect(short1.Block == FeatBudgetBlock.PointsShort && short1.PointsShortBy == 1, "Missing point was not reported");
    Expect(FeatBudgetRules.Evaluate(Points(6), Owned("MainAttributeLegacy", "Defensive"), 1).Allowed, "A cheaper feat was refused");
});

Check("both limits apply together", () =>
{
    Expect(FeatBudgetRules.Evaluate(Both(2, 99), Owned("Defensive", "Skilled"), 1).Block == FeatBudgetBlock.CountFull, "Count limit ignored when points remain");
    Expect(FeatBudgetRules.Evaluate(Both(99, 4), Owned("MainAttributeLegacy"), 2).Block == FeatBudgetBlock.PointsShort, "Point limit ignored when slots remain");
    Expect(FeatBudgetRules.Evaluate(Both(3, 5), Owned("MainAttributeLegacy"), 2).Allowed, "Feat within both limits was refused");
});

Check("usage is pooled across families", () =>
{
    var usage = Owned("MainAttributeLegacy", "Defensive", "Stance", "SpellSchool", "Root");
    Expect(usage.Count == 5 && usage.Points == 3 + 1 + 2 + 2 + 1, $"Unexpected cross-family tally {usage.Count}/{usage.Points}");
    Expect(!FeatBudgetRules.Evaluate(Count(5), usage, 1).Allowed, "Different families escaped the shared count");
});

Check("parametrized choices and ranks are counted individually", () =>
{
    // Two Weapon Damage categories are two facts; a feat with rank 2 is two picks.
    var usage = FeatBudgetRules.Tally(new[] { (2, 1), (2, 1), (1, 2), (3, 0) });
    Expect(usage.Count == 4 && usage.Points == 2 + 2 + 2, $"Unexpected parametrized/rank tally {usage.Count}/{usage.Points}");
    Expect(!FeatBudgetRules.Evaluate(Points(7), usage, 2).Allowed, "A third weapon category ignored its cost");
});

Check("lowered limits report over budget without removing feats", () =>
{
    var owned = Owned("MainAttributeLegacy", "Stance", "Stance");
    var verdict = FeatBudgetRules.Evaluate(Both(2, 5), owned, 1);
    Expect(verdict.OverBudget && !verdict.Allowed, "Over-budget character could take another feat");
    Expect(verdict.Usage.Count == 3 && verdict.Usage.Points == 7, "Owned feats were not all retained in the tally");
    Expect(!FeatBudgetRules.Evaluate(Count(3), owned, 1).OverBudget, "An exactly full character was reported over budget");
});

Check("limits are clamped to the supported range", () =>
{
    Expect(new FeatBudgetLimits(true, -4, true, 500).MaxCount == 0, "Negative limit not clamped");
    Expect(new FeatBudgetLimits(true, -4, true, 500).MaxPoints == FeatBudgetRules.MaxLimit, "Large limit not clamped");
    Expect(!FeatBudgetRules.Evaluate(Count(0), default, 1).Allowed, "Zero limit allowed a feat");
});

Check("cost table covers every menu with tiers 1-3", () =>
{
    var menus = new[] { "MainAttribute_Str", "MainAttribute_Dex", "MainAttribute_Con", "MainAttribute_Int", "MainAttribute_Wis",
        "MainAttribute_Cha", "MainAttributeLegacy", "Defensive", "Maneuver", "ManeuverTactics", "Duel", "Skilled", "Arcane", "Stance", "Conditional",
        "WeaponInsight", "ExtendedReplacement", "GreaterSummoning", "SummonerSacrifice", "ReactiveArmor",
        "DerivedStat", "SpellSchool", "SpellDescriptor", "DistanceDamage", "WeaponDamage", "CastingStat", "ResourceStat", "Retaliation", "Momentum", "Stealth", "Solo", "Growth", "Execution", "Arcana", "Summoner", "Survival", "Meme", "Penetration", "Root" };
    Expect(FeatBudgetRules.Costs.Keys.OrderBy(k => k).SequenceEqual(menus.OrderBy(k => k)), "Cost table and menu list differ");
    Expect(FeatBudgetRules.Costs.Where(entry => entry.Key != "Meme").All(entry => entry.Value is >= 1 and <= 3), "Cost outside tiers 1-3");
    Expect(FeatBudgetRules.FamilyCost("Meme") == 0, "Meme feats should be free");
    Expect(FeatBudgetRules.FamilyCost("MainAttribute_Int") == 2 && FeatBudgetRules.FamilyCost("MainAttributeLegacy") == 3
        && FeatBudgetRules.FamilyCost("Defensive") == 1, "Tier assignment changed");
});

// Level-up preview replay: owned feats plus pending picks in selection order.
List<string> Replay(FeatBudgetLimits limits, string[] owned, List<string> pending)
{
    var unit = owned.ToList();
    var kept = new List<string>();
    foreach (var family in pending)
    {
        if (!FeatBudgetRules.Evaluate(limits, Owned(unit.ToArray()), FeatBudgetRules.FamilyCost(family)).Allowed) continue;
        unit.Add(family);
        kept.Add(family);
    }
    return kept;
}

Check("consecutive picks in one level-up share the budget", () =>
{
    var kept = Replay(Points(6), new[] { "Defensive" }, new List<string> { "Stance", "Skilled", "MainAttributeLegacy" });
    Expect(kept.SequenceEqual(new[] { "Stance", "Skilled" }), "Pending picks were not counted against later picks: " + string.Join(",", kept));
});

Check("cancelling a pending pick frees its budget", () =>
{
    var pending = new List<string> { "MainAttributeLegacy", "Stance" };
    Expect(Replay(Points(5), Array.Empty<string>(), pending).SequenceEqual(new[] { "MainAttributeLegacy", "Stance" }), "Exact two-pick budget refused");
    pending.Add("Defensive");
    Expect(Replay(Points(5), Array.Empty<string>(), pending).Count == 2, "Third pick exceeded the budget");
    pending.Remove("MainAttributeLegacy");
    Expect(Replay(Points(5), Array.Empty<string>(), pending).SequenceEqual(new[] { "Stance", "Defensive" }), "Cancelled pick still consumed budget");
});

Check("changing an earlier pick drops later picks that no longer fit", () =>
{
    var pending = new List<string> { "Defensive", "Stance" };
    Expect(Replay(Points(4), Array.Empty<string>(), pending).Count == 2, "Initial picks refused");
    pending[0] = "MainAttributeLegacy";
    Expect(Replay(Points(4), Array.Empty<string>(), pending).SequenceEqual(new[] { "MainAttributeLegacy" }), "Later pick survived after budget shrank");
});

// Exclusion groups.
var registered = new List<(string guid, string family)>
{
    ("def-str", "Defensive"), ("def-dex", "Defensive"), ("def-con", "Defensive"),
    ("cond-a", "Conditional"), ("cond-b", "Conditional"), ("cond-c", "Conditional"),
    ("wd-str", "WeaponDamage"), ("wd-dex", "WeaponDamage"),
    (Guids.Specialized.Defensive.Dex, "Defensive"), (Guids.Stance.Feature.Wis, "Stance"),
    (Guids.Conditional.EndlessResolve, "Conditional"),
};
var members = FeatGroupRules.Resolve(registered);
FeatGroup Group(string id) => FeatGroupRules.All.Single(group => group.Id == id);
FeatGroupLimit Defaults(FeatGroup group) => new(group.DefaultEnabled, group.DefaultMax);
List<string> Blocking(Func<FeatGroup, FeatGroupLimit> limit, string candidate, params string[] owned)
    => FeatGroupRules.Blocking(members, limit, new HashSet<string>(owned.Select(FeatGroupRules.Key)), FeatGroupRules.Key(candidate))
        .Select(group => group.Id).ToList();

Check("family group allows one member and blocks the rest", () =>
{
    Expect(Blocking(Defaults, "def-str").Count == 0, "First Defensive feat blocked");
    Expect(Blocking(Defaults, "def-dex", "def-str").SequenceEqual(new[] { "Defensive" }), "Second Defensive feat allowed");
});

Check("re-taking a parametrized member is not blocked by its own group", () =>
{
    Expect(Blocking(Defaults, "wd-str", "wd-str").Count == 0, "Second category of the same attribute blocked");
    Expect(Blocking(Defaults, "wd-dex", "wd-str").SequenceEqual(new[] { "WeaponDamage" }), "Second attribute allowed");
});

Check("group limits above one allow that many distinct members", () =>
{
    Expect(Blocking(Defaults, "cond-b", "cond-a").Count == 0, "Second Conditional feat blocked at limit 2");
    Expect(Blocking(Defaults, "cond-c", "cond-a", "cond-b").Contains("Conditional"), "Third Conditional feat allowed at limit 2");
});

Check("disabled groups and raised limits are honoured", () =>
{
    Expect(Blocking(group => new FeatGroupLimit(false, 1), "def-dex", "def-str").Count == 0, "Disabled group still blocks");
    Expect(Blocking(group => new FeatGroupLimit(true, 2), "def-dex", "def-str").Count == 0, "Raised limit still blocks");
    Expect(Blocking(group => new FeatGroupLimit(true, 0), "def-str").Count > 0, "Zero limit allows a member");
});

Check("cross-family groups resolve explicit GUIDs in either spelling", () =>
{
    var acGroup = Group("AttributeToAc");
    Expect(members[acGroup].Contains(FeatGroupRules.Key(Guids.Stance.Feature.Wis)), "Explicit member missing");
    var blocked = Blocking(Defaults, Guids.Conditional.EndlessResolve.ToUpperInvariant().Replace("-", ""),
        Guids.Specialized.Defensive.Dex, Guids.Stance.Feature.Wis);
    Expect(blocked.Contains("AttributeToAc"), "Third attribute-to-AC feat allowed at limit 2");
    Expect(!Blocking(Defaults, Guids.Stance.Feature.Wis, Guids.Specialized.Defensive.Dex).Contains("AttributeToAc"), "Second attribute-to-AC feat blocked");
});

Check("same-attribute groups share one setting and are off by default", () =>
{
    var shared = FeatGroupRules.All.Where(group => group.SettingId == "SameAttribute").ToList();
    Expect(shared.Count == 6 && shared.All(group => !group.DefaultEnabled), "Same-attribute groups misconfigured");
    Expect(FeatGroupRules.Settings.Count(group => group.SettingId == "SameAttribute") == 1, "Shared setting listed more than once");
    Expect(FeatGroupRules.Settings.Select(group => group.SettingId).Distinct().Count() == FeatGroupRules.Settings.Count(), "Duplicate setting ids");
});

Check("one Main per character covers new source menus and retired feats", () =>
{
    var main = FeatGroupRules.Resolve(new[] { ("new-int-str", "MainAttribute_Int"), ("old-str", "MainAttributeLegacy"), ("def", "Defensive") })
        [Group("MainAttribute")];
    Expect(main.SetEquals(new[] { "new-int-str", "old-str" }.Select(FeatGroupRules.Key)), "Main group members: " + string.Join(",", main));
    var sameStr = FeatGroupRules.All.Single(group => group.Id == "SameAttribute_Str").Members.Select(FeatGroupRules.Key).ToHashSet();
    Expect(sameStr.Contains(FeatGroupRules.Key(Guids.MainAttribute.Str.Dex)) && !sameStr.Contains(FeatGroupRules.Key(Guids.MainAttribute.Dex.Str)),
        "Same-attribute group should follow the Main source attribute");
});

Check("every group has bilingual names and a usable default", () =>
{
    foreach (var group in FeatGroupRules.All)
        Expect(!string.IsNullOrWhiteSpace(group.NameEn) && group.NameKey == "FeatGroup." + group.SettingId && group.DefaultMax >= 1, group.Id);
});

// Close-quarters scores and distance styles.
Check("weapon score favours shorter weapons", () =>
{
    Expect(CloseQuartersRules.WeaponScore(true, false, false, false, false) == 4, "Light weapon");
    Expect(CloseQuartersRules.WeaponScore(false, false, true, false, false) == 4, "Natural attack");
    Expect(CloseQuartersRules.WeaponScore(false, false, false, true, false) == 4, "Unarmed strike");
    Expect(CloseQuartersRules.WeaponScore(false, false, false, false, false) == 3, "One-handed weapon");
    Expect(CloseQuartersRules.WeaponScore(false, true, false, false, false) == 2, "Two-handed weapon");
    Expect(CloseQuartersRules.WeaponScore(false, true, false, false, true) == 0, "Reach weapon");
});

Check("reach score drops with total reach and converts Wrath's edge ranges", () =>
{
    Expect(CloseQuartersRules.ReachScore(5) == 4 && CloseQuartersRules.ReachScore(10) == 2, "5/10 ft");
    Expect(CloseQuartersRules.ReachScore(15) == 0 && CloseQuartersRules.ReachScore(40) == 0, "15+ ft");
    Expect(CloseQuartersRules.NominalWeaponReach(1) == 5 && CloseQuartersRules.NominalWeaponReach(5) == 5, "Ordinary weapon");
    Expect(CloseQuartersRules.NominalWeaponReach(6) == 10, "Reach weapon");
});

Check("distance score is highest when pressed against the target", () =>
{
    Expect(CloseQuartersRules.DistanceScore(0.6f) == 4, "Tiny attacker hugging");
    Expect(CloseQuartersRules.DistanceScore(1.7f) == 3, "Medium attacker hugging");
    Expect(CloseQuartersRules.DistanceScore(4.6f) == 1 && CloseQuartersRules.DistanceScore(6f) == 0, "Farther hits");
    Expect(CloseQuartersRules.DistanceScore(-1f) == 4, "Overlapping bodies");
});

Check("mid and long range scores", () =>
{
    Expect(CloseQuartersRules.MidDistanceScore(0.5f) == 0 && CloseQuartersRules.MidDistanceScore(6f) == 4, "Mid distance");
    Expect(CloseQuartersRules.ReachWeaponScore(true, true) == 4 && CloseQuartersRules.ReachWeaponScore(true, false) == 2
        && CloseQuartersRules.ReachWeaponScore(false, false) == 0, "Reach weapon");
    Expect(CloseQuartersRules.LongReachScore(5) == 0 && CloseQuartersRules.LongReachScore(10) == 2 && CloseQuartersRules.LongReachScore(20) == 4, "Long reach");
    Expect(CloseQuartersRules.LongDistanceScore(15f) == 0 && CloseQuartersRules.LongDistanceScore(30f) == 2 && CloseQuartersRules.LongDistanceScore(90f) == 4, "Long distance");
    Expect(CloseQuartersRules.RangedWeaponScore(60) == 4 && CloseQuartersRules.RangedWeaponScore(30) == 2 && CloseQuartersRules.RangedWeaponScore(10) == 0, "Ranged weapon");
    Expect(CloseQuartersRules.UnengagedScore(false) == 4 && CloseQuartersRules.UnengagedScore(true) == 0, "Unengaged");
    Expect(CloseQuartersRules.IsRanged(CloseQuartersMeasure.RangedWeapon)
        && !CloseQuartersRules.IsRanged(CloseQuartersMeasure.MidDistance), "Melee/ranged split");
});

Check("one distance style: close feats stack, styles exclude each other", () =>
{
    var distance = FeatGroupRules.Resolve(Array.Empty<(string, string)>());
    List<string> Style(string candidate, params string[] owned) => FeatGroupRules.Blocking(distance, Defaults,
        new HashSet<string>(owned.Select(FeatGroupRules.Key)), FeatGroupRules.Key(candidate)).Select(group => group.SettingId).Distinct().ToList();
    Expect(Style(Guids.DistanceDamage.ShortBlade, Guids.DistanceDamage.AggressorsEdge, Guids.DistanceDamage.CloseQuarters).Count == 0, "Close feats do not stack");
    Expect(Style(Guids.DistanceDamage.MarksmansFocus, Guids.DistanceDamage.ShortBlade).SequenceEqual(new[] { "DistanceStyle" }), "Long range mixed with close");
    Expect(Style(Guids.DistanceDamage.OptimalRange, Guids.DistanceDamage.MarksmansFocus).SequenceEqual(new[] { "DistanceStyle" }), "Mid range mixed with long");
    Expect(Style(Guids.DistanceDamage.LongHaft, Guids.DistanceDamage.OptimalRange, Guids.DistanceDamage.ReachControl).Count == 0, "Mid feats do not stack");
    Expect(Style(Guids.DistanceDamage.SteadySniper, Guids.DistanceDamage.StrongBow, Guids.DistanceDamage.MarksmansFocus).Count == 0, "Long feats do not stack");
});

Check("arcane specialization scheme C rounds half, full and quarter modifiers down", () =>
{
    void ExpectScale(int modifier, bool legacy, int casterLevel, int penetration, int dc)
    {
        Expect(ArcaneSpecializationRules.CasterLevelBonus(modifier, legacy) == casterLevel,
            $"CL {modifier} legacy {legacy}");
        Expect(ArcaneSpecializationRules.SpellPenetrationBonus(modifier, legacy) == penetration,
            $"penetration {modifier} legacy {legacy}");
        Expect(ArcaneSpecializationRules.SpellDcBonus(modifier, legacy) == dc,
            $"DC {modifier} legacy {legacy}");
    }

    Expect(ArcaneSpecializationRules.CasterLevelDivisor == 2
        && ArcaneSpecializationRules.SpellPenetrationDivisor == 1
        && ArcaneSpecializationRules.SpellDcDivisor == 4, "Scheme C divisors");
    ExpectScale(0, false, 0, 0, 0);
    ExpectScale(1, false, 0, 1, 0);
    ExpectScale(2, false, 1, 2, 0);
    ExpectScale(3, false, 1, 3, 0);
    ExpectScale(4, false, 2, 4, 1);
    ExpectScale(5, false, 2, 5, 1);
    ExpectScale(7, false, 3, 7, 1);
    ExpectScale(8, false, 4, 8, 2);
    ExpectScale(15, false, 7, 15, 3);
    ExpectScale(-1, false, 0, 0, 0);
    ExpectScale(-4, false, 0, 0, 0);
    ExpectScale(1, true, 1, 1, 1);
    ExpectScale(5, true, 5, 5, 5);
    ExpectScale(7, true, 7, 7, 7);
    ExpectScale(-3, true, 0, 0, 0);
});

Check("arcane aegis keeps half caster level and allows only light or no armor without a shield", () =>
{
    Expect(ArcaneAegisRules.ArmorClassBonus(0) == 0 && ArcaneAegisRules.ArmorClassBonus(1) == 0, "CL 0 and 1");
    Expect(ArcaneAegisRules.ArmorClassBonus(2) == 1 && ArcaneAegisRules.ArmorClassBonus(5) == 2, "CL 2 and 5");
    Expect(ArcaneAegisRules.ArmorClassBonus(9) == 4 && ArcaneAegisRules.ArmorClassBonus(20) == 10, "CL 9 and 20");
    Expect(ArcaneAegisRules.ArmorClassBonus(-4) == 0, "Negative caster level");

    bool Allows(string group, bool shield) => ArcaneAegisRules.Applies(ArcaneAegisRules.ClassifyGroup(group), shield);
    Expect(ArcaneAegisRules.Applies(ArcaneAegisArmor.Missing, false), "No armor slot");
    Expect(Allows(null, false) && Allows("None", false) && Allows("Light", false), "Unarmored and light armor");
    Expect(!Allows("Medium", false) && !Allows("Heavy", false), "Medium and heavy armor");
    Expect(!Allows("LightBarding", false) && !Allows("MediumBarding", false) && !Allows("HeavyBarding", false), "Barding");
    Expect(!Allows("Buckler", false) && !Allows("LightShield", false) && !Allows("HeavyShield", false) && !Allows("TowerShield", false), "Shield worn as armor");
    Expect(!Allows("Other", false), "Unknown armor");
    Expect(!ArcaneAegisRules.Applies(ArcaneAegisArmor.Missing, true), "Shield and no armor");
    Expect(!Allows("None", true) && !Allows("Light", true), "Shield with unarmored or light armor");
    Expect(!Allows("Medium", true) && !Allows("Heavy", true), "Shield with medium or heavy armor");
});

Check("arcane aegis counts toward attribute-to-AC, and stacking lifts that limit", () =>
{
    var attributeToAc = Group("AttributeToAc");
    Expect(members[attributeToAc].Contains(FeatGroupRules.Key(Guids.Derived.ArcaneAegis)), "Arcane Aegis missing from attribute-to-AC");
    Expect(members[Group("DerivedAc")].Contains(FeatGroupRules.Key(Guids.Derived.ArcaneAegis)), "Arcane Aegis missing from derived AC");

    var oneOther = new[] { Guids.Specialized.Defensive.Dex };
    var twoOthers = new[] { Guids.Specialized.Defensive.Dex, Guids.Stance.Feature.Dex };
    Expect(!Blocking(Defaults, Guids.Derived.ArcaneAegis).Contains("AttributeToAc"), "Arcane Aegis alone blocked");
    Expect(!Blocking(Defaults, Guids.Derived.ArcaneAegis, oneOther).Contains("AttributeToAc"), "Second attribute-to-AC feat blocked");
    Expect(Blocking(Defaults, Guids.Derived.ArcaneAegis, twoOthers).Contains("AttributeToAc"), "Third attribute-to-AC feat allowed");
    Expect(Blocking(Defaults, Guids.Derived.ArcaneAegis, Guids.Derived.SkilledDefender).Contains("DerivedAc"), "Derived AC pair allowed");

    FeatGroupLimit AllowAttributeStack(FeatGroup group) => group.SettingId == "AttributeToAc"
        ? new FeatGroupLimit(false, group.DefaultMax)
        : new FeatGroupLimit(group.DefaultEnabled, group.DefaultMax);
    Expect(!Blocking(AllowAttributeStack, Guids.Derived.ArcaneAegis, twoOthers).Contains("AttributeToAc"), "Disabled attribute-to-AC group still blocks");
    Expect(Blocking(AllowAttributeStack, Guids.Derived.ArcaneAegis, Guids.Derived.SkilledDefender).Contains("DerivedAc"), "Derived AC turned off with attribute stacking");

    Expect(Blocking(_ => new FeatGroupLimit(false, 1), Guids.Derived.ArcaneAegis, twoOthers.Append(Guids.Derived.SkilledDefender).ToArray()).Count == 0,
        "Master exclusion switch still blocks");
});

Check("arcane feats wire scheme C ranks and arcane aegis keeps its armor gate", () =>
{
    var root = Directory.GetCurrentDirectory();
    while (!File.Exists(Path.Combine(root, "ACHomebrew.slnx")))
        root = Directory.GetParent(root)?.FullName ?? throw new Exception("Cannot locate repository root.");
    var arcane = File.ReadAllText(Path.Combine(root, "src", "ACHomebrew.Aptitudes", "SpecializedFeats.cs"));
    Expect(arcane.Contains("Common.Rank(AbilityRankType.StatBonus)")
        && arcane.Contains("Common.Rank(AbilityRankType.Default)")
        && arcane.Contains("Common.Rank(AbilityRankType.SpeedBonus)"), "Arcane rank slots");
    Expect(arcane.Contains("AbilityRankType.StatBonus, Progression(casterLevel), step: casterLevel")
        && arcane.Contains("AbilityRankType.SpeedBonus, Progression(spellDc), step: spellDc"), "Arcane divisors");
    var aegis = File.ReadAllText(Path.Combine(root, "src", "ACHomebrew.Attributes", "DerivedStatFeats.cs"));
    Expect(aegis.Contains("ContextRankConfigs.CustomProperty(Guids.Derived.HighestSpellbookClassLevelProperty, min: 0).WithDiv2Progression()")
        && aegis.Contains("restrictions: CreateArcaneAegisRestriction()")
        && aegis.Contains("ArcaneAegisRules.Applies"), "Arcane Aegis gate");
});

Check("derived feats take the highest spellbook class level, never a sum, and reapply on level up", () =>
{
    Expect(DerivedStatRules.HighestSpellbookClassLevel(new[] { 8 }) == 8, "Single spellbook class");
    Expect(DerivedStatRules.HighestSpellbookClassLevel(new[] { 8, 4 }) == 8, "Two spellbook classes take the highest, not 12");
    Expect(DerivedStatRules.HighestSpellbookClassLevel(new[] { 4, 8 }) == 8, "Order does not matter");
    Expect(DerivedStatRules.HighestSpellbookClassLevel(Array.Empty<int>()) == 0, "No spellbook class");
    Expect(DerivedStatRules.HighestSpellbookClassLevel(new[] { 0 }) == 0, "Zero-level spellbook class");

    var root = Directory.GetCurrentDirectory();
    while (!File.Exists(Path.Combine(root, "ACHomebrew.slnx")))
        root = Directory.GetParent(root)?.FullName ?? throw new Exception("Cannot locate repository root.");
    var derived = File.ReadAllText(Path.Combine(root, "src", "ACHomebrew.Attributes", "DerivedStatFeats.cs"));
    Expect(derived.Contains("UnitPropertyConfigurator.New(\"HighestSpellbookClassLevelProperty\", Guids.Derived.HighestSpellbookClassLevelProperty)"),
        "The unit property blueprint is missing");
    Expect(!derived.Contains("ContextRankConfigs.CasterLevel("),
        "A derived feat still reads the empty-params caster level");
    const string property = "ContextRankConfigs.CustomProperty(Guids.Derived.HighestSpellbookClassLevelProperty, min: 0)";
    Expect(derived.Split(new[] { property }, StringSplitOptions.None).Length - 1 == 3,
        "Arcane Aegis, Mystic Vitality and the Soul Bulwark buff must all read the property");
    Expect(derived.Contains(property + ".WithDiv2Progression()"), "Arcane Aegis keeps half progression");
    Expect(derived.Split(new[] { ".SetReapplyOnLevelUp()" }, StringSplitOptions.None).Length - 1 == 2,
        "Arcane Aegis and Mystic Vitality reapply on level up");
});

Console.WriteLine($"{cases - failures}/{cases} feat budget, exclusion and arcane balance checks passed (pure rules, not an in-game test).");
return failures == 0 ? 0 : 1;
