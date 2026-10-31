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

Console.WriteLine($"{cases - failures}/{cases} feat budget and exclusion group rule checks passed (pure rules, not an in-game test).");
return failures == 0 ? 0 : 1;
