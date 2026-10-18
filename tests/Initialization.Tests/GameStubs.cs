// Test boundary for Unity/game and BlueprintCore APIs. The real FeatRegistry and
// FeatSelection sources orchestrate these stand-ins; no game runtime is started.
namespace Kingmaker.Blueprints
{
    public record BlueprintGuid(string Value);
    public class BlueprintFeatureReference { }
}

namespace Kingmaker.Blueprints.Classes
{
    public enum FeatureGroup { Feat }
    public class BlueprintFeature
    {
        public Kingmaker.Blueprints.BlueprintGuid AssetGuid;
        public string Name;
    }
}

namespace Kingmaker.Blueprints.Classes.Selection
{
    public enum SelectionMode { Default, OnlyNew }
    public class BlueprintFeatureSelection : Kingmaker.Blueprints.Classes.BlueprintFeature
    {
        public Kingmaker.Blueprints.Classes.BlueprintFeature[] Children = Array.Empty<Kingmaker.Blueprints.Classes.BlueprintFeature>();
    }
}

namespace BlueprintCore.Utils
{
    public class Blueprint<T>
    {
        public Kingmaker.Blueprints.Classes.BlueprintFeature Feature;
        public static implicit operator Blueprint<T>(Kingmaker.Blueprints.Classes.BlueprintFeature feature) => new() { Feature = feature };
    }
}

namespace BlueprintCore.Blueprints.CustomConfigurators.Classes
{
    public class FeatureConfigurator
    {
        private readonly Kingmaker.Blueprints.Classes.BlueprintFeature feature;
        private Action<Kingmaker.Blueprints.Classes.BlueprintFeature> callback;
        protected FeatureConfigurator(string name, string guid) => feature = new() { Name = name, AssetGuid = new(guid) };
        public static FeatureConfigurator New(string name, string guid, Kingmaker.Blueprints.Classes.FeatureGroup group) => new(name, guid);
        public FeatureConfigurator SkipAddToSelections() => this;
        public FeatureConfigurator OnConfigure(Action<Kingmaker.Blueprints.Classes.BlueprintFeature> action) { callback = action; return this; }
        public void Configure() => callback?.Invoke(feature);
    }
}

namespace BlueprintCore.Blueprints.Configurators.Classes.Selection
{
    public class ParametrizedFeatureConfigurator : CustomConfigurators.Classes.FeatureConfigurator
    {
        private ParametrizedFeatureConfigurator(string name, string guid) : base(name, guid) { }
        public static ParametrizedFeatureConfigurator New(string name, string guid) => new(name, guid);
        public ParametrizedFeatureConfigurator SetGroups(Kingmaker.Blueprints.Classes.FeatureGroup group) => this;
        public new ParametrizedFeatureConfigurator OnConfigure(Action<Kingmaker.Blueprints.Classes.BlueprintFeature> action) { base.OnConfigure(action); return this; }
    }
}

namespace BlueprintCore.Blueprints.CustomConfigurators.Classes.Selection
{
    public class FeatureSelectionConfigurator
    {
        private Kingmaker.Blueprints.Classes.Selection.BlueprintFeatureSelection selection;
        private bool publish;
        public static FeatureSelectionConfigurator New(string name, string guid, Kingmaker.Blueprints.Classes.FeatureGroup group)
            => new() { selection = new() { Name = name, AssetGuid = new(guid) } };
        public static FeatureSelectionConfigurator For(Kingmaker.Blueprints.Classes.Selection.BlueprintFeatureSelection selection, bool updateSelections)
            => new() { selection = selection, publish = updateSelections };
        public FeatureSelectionConfigurator SkipAddToSelections() => this;
        public FeatureSelectionConfigurator SetDisplayName(string text) => this;
        public FeatureSelectionConfigurator SetDescription(string text) => this;
        public FeatureSelectionConfigurator SetGroup(Kingmaker.Blueprints.Classes.FeatureGroup group) => this;
        public FeatureSelectionConfigurator SetMode(Kingmaker.Blueprints.Classes.Selection.SelectionMode mode) => this;
        public FeatureSelectionConfigurator SetIgnorePrerequisites(bool value) => this;
        public FeatureSelectionConfigurator SetAllFeatures(BlueprintCore.Utils.Blueprint<Kingmaker.Blueprints.BlueprintFeatureReference>[] children)
        { selection.Children = children.Select(child => child.Feature).ToArray(); return this; }
        public Kingmaker.Blueprints.Classes.Selection.BlueprintFeatureSelection Configure()
        {
            if (selection.Name == World.FailedMenu) throw new InvalidOperationException("injected menu failure");
            if (publish && World.FailRootPublication) throw new InvalidOperationException("injected root publication failure");
            if (publish) World.Published.Add(selection);
            return selection;
        }
    }
}

namespace ACHomebrew
{
    internal static class Main { internal static readonly TestLogger Log = new(); }
    internal static class Mod { internal static readonly TestLogger Log = Main.Log; }
    internal class TestLogger { internal void Log(string message) => World.Logs.Add(message); }
}

namespace ACHomebrew.Feats
{
    internal static class Common { internal static string L(string key, string text) => text; }
    internal static class MainAbilityToEverything_Feats { internal static void ConfigureAll() => World.Register(nameof(MainAbilityToEverything_Feats), FeatSelection.MainFromStr); }
    internal static class SpecializedFeats { internal static void ConfigureAll() => World.Register(nameof(SpecializedFeats), FeatSelection.Defensive); }
    internal static class ManeuverFeats { internal static void ConfigureAll() => World.Register(nameof(ManeuverFeats), FeatSelection.ManeuverTactics); }
    internal static class StanceFeats { internal static void ConfigureAll() => World.Register(nameof(StanceFeats), FeatSelection.Stance); }
    internal static class ConditionalFeats { internal static void ConfigureAll() => World.Register(nameof(ConditionalFeats), FeatSelection.Conditional); }
    internal static class StatReplacementFeats { internal static void ConfigureAll() => World.Register(nameof(StatReplacementFeats), FeatSelection.WeaponInsight); }
    internal static class ReactiveArmorFeats { internal static void ConfigureAll() => World.Register(nameof(ReactiveArmorFeats), FeatSelection.ReactiveArmor); }
    internal static class DerivedStatFeats { internal static void ConfigureAll() => World.Register(nameof(DerivedStatFeats), FeatSelection.DerivedStat); }
    internal static class GreaterSummoningFeats { internal static void ConfigureAll() => World.Register(nameof(GreaterSummoningFeats), FeatSelection.GreaterSummoning); }
    internal static class SpellTagFeats { internal static void ConfigureAll() => World.Register(nameof(SpellTagFeats), FeatSelection.SpellSchool); }
    internal static class SummonerSacrificeFeats { internal static void ConfigureAll() => World.Register(nameof(SummonerSacrificeFeats), FeatSelection.SummonerSacrifice); }
    internal static class PolearmMasterFeats { internal static void ConfigureAll() => World.Register(nameof(PolearmMasterFeats), FeatSelection.Root); }
    internal static class DistanceDamageFeats { internal static void ConfigureAll() => World.Register(nameof(DistanceDamageFeats), FeatSelection.DistanceDamage); }
    internal static class WeaponDamageFeats { internal static void ConfigureAll() => World.Register(nameof(WeaponDamageFeats), FeatSelection.WeaponDamage); }
    internal static class CastingStatFeats { internal static void ConfigureAll() => World.Register(nameof(CastingStatFeats), FeatSelection.CastingStat); }
    internal static class ResourceStatFeats { internal static void ConfigureAll() => World.Register(nameof(ResourceStatFeats), FeatSelection.ResourceStat); }
    internal static class RetaliationFeats { internal static void ConfigureAll() => World.Register(nameof(RetaliationFeats), FeatSelection.Retaliation); }
    internal static class MomentumFeats { internal static void ConfigureAll() => World.Register(nameof(MomentumFeats), FeatSelection.Momentum); }
    internal static class ExecutionFeats { internal static void ConfigureAll() => World.Register(nameof(ExecutionFeats), FeatSelection.Execution); }
    internal static class ArcanaFeats { internal static void ConfigureAll() => World.Register(nameof(ArcanaFeats), FeatSelection.Arcana); }
    internal static class SummonerFeats { internal static void ConfigureAll() => World.Register(nameof(SummonerFeats), FeatSelection.Summoner); }
    internal static class StealthFeats { internal static void ConfigureAll() => World.Register(nameof(StealthFeats), FeatSelection.Stealth); }
    internal static class SoloFeats { internal static void ConfigureAll() => World.Register(nameof(SoloFeats), FeatSelection.Solo); }
    internal static class GrowthFeats { internal static void ConfigureAll() => World.Register(nameof(GrowthFeats), FeatSelection.Growth); }
    internal static class SurvivalFeats { internal static void ConfigureAll() => World.Register(nameof(SurvivalFeats), FeatSelection.Survival); }
    internal static class MemeFeats { internal static void ConfigureAll() => World.Register(nameof(MemeFeats), FeatSelection.Meme); }
    internal static class PenetrationFeats { internal static void ConfigureAll() => World.Register(nameof(PenetrationFeats), FeatSelection.Penetration); }
    internal static class WeaponPenetrationFeats { internal static void ConfigureAll() => World.Register(nameof(WeaponPenetrationFeats), FeatSelection.Penetration); }
    internal static class MutexPass { internal static void ApplyAll() => World.Register(nameof(MutexPass)); }
    internal static class FeatBudget { internal static void Install() => World.Budgeted.AddRange(FeatSelection.BudgetedFeats()); }
}
