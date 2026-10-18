using System.Collections;
using System.Reflection;
using ACHomebrew.Feats;

var failures = 0;
var cases = 0;
void Check(string name, Action test)
{
    cases++;
    World.Reset();
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

void ExpectRoot()
{
    Expect(World.Published.Count == 1, "Expected one root menu in outer feat lists");
    Expect(World.Published[0].AssetGuid.Value == Guids.ACHomebrewSelection, "Wrong root published");
}

Check("normal registration publishes a nested hierarchy", () =>
{
    FeatRegistry.ConfigureAll();
    ExpectRoot();
    Expect(World.RootContains(Guids.FeatSelections.MainAttribute), "Main family missing");
    Expect(World.RootContains(Guids.FeatSelections.WeaponDamage), "Weapon Damage family missing");
});

foreach (var family in World.RegistrationNames)
{
    Check($"{family} failure does not hide other families", () =>
    {
        World.FailedRegistrations.Add(family);
        FeatRegistry.ConfigureAll();
        ExpectRoot();
        Expect(World.Attempted.Count == World.RegistrationNames.Length, "A later registration was skipped");
        var survivingMenu = family == "MainAbilityToEverything_Feats"
            ? Guids.FeatSelections.WeaponDamage : Guids.FeatSelections.MainAttribute;
        Expect(World.RootContains(survivingMenu), "A healthy family is inaccessible");
        Expect(World.Logs.Any(log => log.Contains(family) && log.Contains("injected registration failure")), "Failure diagnostics lost the family or exception");
        Expect(!World.Logs.Any(log => log.Contains("98 total")), "Logged a full roster after partial failure");
    });
}

Check("feat budget covers leaves only, priced by family", () =>
{
    FeatRegistry.ConfigureAll();
    ExpectRoot();
    var budgeted = World.Budgeted.ToDictionary(entry => entry.feat.Name, entry => entry.cost);
    Expect(budgeted.Count == World.RegistrationNames.Length - 1, $"Expected one budgeted leaf per family, got {budgeted.Count}");
    Expect(!World.Budgeted.Any(entry => entry.feat is Kingmaker.Blueprints.Classes.Selection.BlueprintFeatureSelection), "A menu consumes budget");
    Expect(budgeted["MainAbilityToEverything_Feats"] == 2, "Main source-menu cost not applied");
    Expect(budgeted["SpecializedFeats"] == 1, "Defensive family cost not applied");
    Expect(budgeted["PolearmMasterFeats"] == 1, "Direct root feat cost not applied");
});

Check("a failed batch is not budgeted", () =>
{
    World.PartialFailure = "SpecializedFeats";
    FeatRegistry.ConfigureAll();
    Expect(!World.Budgeted.Any(entry => entry.feat.Name.StartsWith("SpecializedFeats") || entry.feat.Name.StartsWith("Partial")), "Rolled-back feat was budgeted");
    Expect(World.Budgeted.Any(entry => entry.feat.Name == "WeaponDamageFeats"), "Healthy feat lost its budget");
});

Check("Main source menus nest under Main Attribute Mastery", () =>
{
    FeatRegistry.ConfigureAll();
    ExpectRoot();
    var main = World.Published[0].Children.OfType<Kingmaker.Blueprints.Classes.Selection.BlueprintFeatureSelection>()
        .Single(child => child.AssetGuid.Value == Guids.FeatSelections.MainAttribute);
    Expect(main.Children.Any(child => child.AssetGuid.Value == Guids.MainAttribute.Source.Str), "Source menu not nested");
    Expect(!World.RootContains(Guids.MainAttribute.Source.Str), "Source menu also published at the root");
});

Check("a failed family menu does not stop later menus or root publication", () =>
{
    World.FailedMenu = "ACHomebrewSelection_Stance";
    FeatRegistry.ConfigureAll();
    ExpectRoot();
    Expect(!World.RootContains(Guids.FeatSelections.Stance), "Failed menu was published");
    Expect(World.RootContains(Guids.FeatSelections.WeaponDamage), "Later menu was skipped");
    Expect(World.Logs.Any(log => log.Contains(World.FailedMenu) && log.Contains("injected menu failure")), "Menu failure was not identified");
});

Check("partially registered leaves are not offered before their mutex is complete", () =>
{
    World.PartialFailure = "MainAbilityToEverything_Feats";
    FeatRegistry.ConfigureAll();
    ExpectRoot();
    Expect(!World.RootContains(Guids.FeatSelections.MainAttribute), "Incomplete Main family was published");
    Expect(World.RootContains(Guids.FeatSelections.WeaponDamage), "Healthy later family was lost");
});

Check("a failed batch removes its leaves across multiple families only", () =>
{
    World.PartialFailure = "SpecializedFeats";
    FeatRegistry.ConfigureAll();
    ExpectRoot();
    foreach (var guid in new[] { Guids.FeatSelections.Defensive, Guids.FeatSelections.Maneuver,
        Guids.FeatSelections.Skilled, Guids.FeatSelections.Arcane })
        Expect(!World.RootContains(guid), "Incomplete Specialized family was published");
    Expect(World.RootContains(Guids.FeatSelections.MainAttribute), "Earlier successful family was lost");
    Expect(World.RootContains(Guids.FeatSelections.WeaponDamage), "Later successful family was lost");
});

Check("a failed direct-root feat is removed without losing family menus", () =>
{
    World.PartialFailure = "PolearmMasterFeats";
    FeatRegistry.ConfigureAll();
    ExpectRoot();
    Expect(!World.RootContains("PolearmMasterFeatsFirst"), "Incomplete direct-root feat was published");
    Expect(World.RootContains(Guids.FeatSelections.MainAttribute), "Earlier successful family was lost");
});

Check("simultaneous registration and menu failures preserve healthy choices", () =>
{
    World.FailedRegistrations.Add("WeaponDamageFeats");
    World.FailedMenu = "ACHomebrewSelection_Stance";
    FeatRegistry.ConfigureAll();
    ExpectRoot();
    Expect(World.RootContains(Guids.FeatSelections.MainAttribute), "Main menu was lost");
    Expect(World.RootContains(Guids.FeatSelections.DistanceDamage), "Distance menu was lost");
});

Check("root publication failure is reported without claiming full initialization", () =>
{
    World.FailRootPublication = true;
    FeatRegistry.ConfigureAll();
    Expect(World.Published.Count == 0, "Root unexpectedly published");
    Expect(World.Logs.Any(log => log.Contains("ACHomebrewSelection_Root") && log.Contains("injected root publication failure")), "Root failure was not identified");
    Expect(!World.Logs.Any(log => log.Contains("registry initialized")), "Reported success after root failure");
});

Check("partial initialization is not replayed on a second call", () =>
{
    World.FailedRegistrations.Add("WeaponDamageFeats");
    FeatRegistry.ConfigureAll();
    ExpectRoot();
    var attempts = World.Attempted.Count;
    World.FailedRegistrations.Clear();
    FeatRegistry.ConfigureAll();
    Expect(World.Attempted.Count == attempts, "Replayed registrations after partial initialization");
    Expect(World.Published.Count == 1, "Published the root twice");
});

Console.WriteLine($"{cases - failures}/{cases} initialization checks passed.");
return failures == 0 ? 0 : 1;

internal static class World
{
    internal static readonly string[] RegistrationNames =
    {
        "MainAbilityToEverything_Feats", "SpecializedFeats", "ManeuverFeats", "StanceFeats", "ConditionalFeats",
        "StatReplacementFeats", "ReactiveArmorFeats", "DerivedStatFeats", "GreaterSummoningFeats",
        "SpellTagFeats", "SummonerSacrificeFeats", "PolearmMasterFeats", "DistanceDamageFeats",
        "WeaponDamageFeats", "CastingStatFeats", "ResourceStatFeats", "RetaliationFeats", "MomentumFeats", "ExecutionFeats", "ArcanaFeats", "SummonerFeats", "StealthFeats", "SoloFeats", "GrowthFeats", "SurvivalFeats", "MemeFeats", "PenetrationFeats", "WeaponPenetrationFeats", "MutexPass",
    };
    internal static readonly HashSet<string> FailedRegistrations = new();
    internal static readonly List<string> Attempted = new();
    internal static readonly List<string> Logs = new();
    internal static readonly List<(Kingmaker.Blueprints.Classes.BlueprintFeature feat, string family, int cost)> Budgeted = new();
    internal static readonly List<Kingmaker.Blueprints.Classes.Selection.BlueprintFeatureSelection> Published = new();
    internal static string FailedMenu;
    internal static string PartialFailure;
    internal static bool FailRootPublication;

    internal static void Register(string name, FeatSelection family = null)
    {
        Attempted.Add(name);
        if (FailedRegistrations.Contains(name)) throw new InvalidOperationException("injected registration failure");
        if (PartialFailure == name)
        {
            family?.NewFeat(name + "First", name + "First").Configure();
            family?.NewFeat(name + "Second", name + "Second").Configure();
            if (name == "SpecializedFeats")
            {
                FeatSelection.Maneuver.NewFeat("PartialManeuver", "PartialManeuver").Configure();
                FeatSelection.Skilled.NewFeat("PartialSkilled", "PartialSkilled").Configure();
                FeatSelection.Arcane.NewFeat("PartialArcane", "PartialArcane").Configure();
            }
            throw new InvalidOperationException("injected failure before mutex configuration");
        }
        family?.NewFeat(name, name).Configure();
    }

    internal static bool RootContains(string guid) => Published[0].Children.Any(child => child.AssetGuid.Value == guid);

    internal static void Reset()
    {
        FailedRegistrations.Clear();
        Attempted.Clear();
        Logs.Clear();
        Budgeted.Clear();
        Published.Clear();
        FailedMenu = null;
        PartialFailure = null;
        FailRootPublication = false;
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        foreach (var field in typeof(FeatSelection).GetFields(BindingFlags.Static | BindingFlags.Public))
        {
            var family = field.GetValue(null);
            ((IDictionary)typeof(FeatSelection).GetField("Feats", instanceFlags).GetValue(family)).Clear();
            typeof(FeatSelection).GetField("Configured", instanceFlags).SetValue(family, false);
        }
        foreach (var field in typeof(FeatRegistry).GetFields(BindingFlags.Static | BindingFlags.NonPublic))
            if (field.FieldType == typeof(bool)) field.SetValue(null, false);
    }
}
