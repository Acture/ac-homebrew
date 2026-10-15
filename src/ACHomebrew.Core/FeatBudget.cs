using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;

namespace ACHomebrew.Feats
{
    /// <summary>
    /// Applies the per-character feat budget and exclusion groups to registered
    /// AttributeFeats leaves. Both read the unit's current features and live
    /// settings, so the level-up preview, cancelled choices, respec, loaded saves
    /// and setting changes need no separate bookkeeping.
    /// </summary>
    internal static class FeatBudget
    {
        private static readonly Dictionary<BlueprintGuid, int> Costs = new();
        private static readonly List<PrerequisiteFeatBudget> BudgetPrerequisites = new();
        private static readonly List<PrerequisiteFeatGroups> GroupPrerequisites = new();
        private static Dictionary<FeatGroup, HashSet<string>> GroupMembers = new();

        public static FeatBudgetLimits Limits
        {
            get
            {
                var s = Mod.Settings;
                return s == null
                    ? default
                    : new FeatBudgetLimits(s.EnableFeatCountLimit, s.MaxFeatCount, s.EnableFeatPointLimit, s.MaxFeatPoints);
            }
        }

        public static void Install()
        {
            var feats = FeatSelection.BudgetedFeats().ToList();
            GroupMembers = FeatGroupRules.Resolve(feats.Select(entry => (entry.feat.AssetGuid.ToString(), entry.family)));
            foreach (var (feat, _, cost) in feats)
            {
                if (Costs.ContainsKey(feat.AssetGuid)) continue;
                Costs[feat.AssetGuid] = cost;
                var budget = new PrerequisiteFeatBudget { Cost = cost };
                var configurator = FeatureConfigurator.For(feat).AddComponent(budget);
                BudgetPrerequisites.Add(budget);

                var guid = FeatGroupRules.Key(feat.AssetGuid.ToString());
                if (GroupMembers.Any(group => group.Value.Contains(guid)))
                {
                    var groups = new PrerequisiteFeatGroups { FeatGuid = guid };
                    configurator.AddComponent(groups);
                    GroupPrerequisites.Add(groups);
                }
                configurator.Configure();
            }
            SyncVisibility();
            Mod.Log?.Log($"AttributeFeats: feat budget covers {Costs.Count} feats; {GroupPrerequisites.Count} belong to exclusion groups.");
        }

        /// <summary>Hides tooltip lines for limits and groups that are currently off.</summary>
        public static void SyncVisibility()
        {
            var hideBudget = !Limits.AnyEnabled;
            foreach (var prerequisite in BudgetPrerequisites)
                prerequisite.HideInUI = hideBudget;
            foreach (var prerequisite in GroupPrerequisites)
                prerequisite.HideInUI = !GroupsOf(prerequisite.FeatGuid).Any(group => GroupLimit(group).Enabled);
        }

        public static bool TryGetCost(BlueprintFeature feature, out int cost)
        {
            cost = 0;
            return feature != null && Costs.TryGetValue(feature.AssetGuid, out cost);
        }

        /// <summary>Counts AttributeFeats leaves the unit owns; vanilla and other mods' feats are ignored.</summary>
        public static FeatBudgetUsage Usage(UnitDescriptor unit)
        {
            var owned = new List<(int cost, int rank)>();
            foreach (var fact in unit.Progression.Features)
            {
                if (Costs.TryGetValue(fact.Blueprint.AssetGuid, out var cost))
                    owned.Add((cost, fact.GetRank()));
            }
            return FeatBudgetRules.Tally(owned);
        }

        public static FeatBudgetVerdict Evaluate(UnitDescriptor unit, int cost)
            => FeatBudgetRules.Evaluate(Limits, Usage(unit), cost);

        public static FeatGroupLimit GroupLimit(FeatGroup group)
        {
            var s = Mod.Settings;
            if (s != null && !s.EnableMutex) return new FeatGroupLimit(false, group.DefaultMax);
            var setting = s?.FeatGroups?.FirstOrDefault(entry => entry.Id == group.SettingId);
            return setting == null
                ? new FeatGroupLimit(group.DefaultEnabled, group.DefaultMax)
                : new FeatGroupLimit(setting.Enabled, setting.Max);
        }

        /// <summary>Stores a changed group setting, keeping untouched groups on their defaults.</summary>
        public static void SetGroupLimit(FeatGroup group, bool enabled, int max)
        {
            var s = Mod.Settings;
            if (s == null) return;
            s.FeatGroups ??= new List<FeatGroupSetting>();
            var setting = s.FeatGroups.FirstOrDefault(entry => entry.Id == group.SettingId);
            if (setting == null) s.FeatGroups.Add(setting = new FeatGroupSetting { Id = group.SettingId });
            setting.Enabled = enabled;
            setting.Max = FeatBudgetRules.ClampLimit(max);
        }

        private static IEnumerable<FeatGroup> GroupsOf(string guid)
            => GroupMembers.Where(group => group.Value.Contains(guid)).Select(group => group.Key);

        private static HashSet<string> OwnedGuids(UnitDescriptor unit)
        {
            var owned = new HashSet<string>();
            foreach (var fact in unit.Progression.Features)
                owned.Add(FeatGroupRules.Key(fact.Blueprint.AssetGuid.ToString()));
            return owned;
        }

        public static List<FeatGroup> BlockingGroups(UnitDescriptor unit, string guid)
            => FeatGroupRules.Blocking(GroupMembers, GroupLimit, OwnedGuids(unit), guid);

        public static string GroupName(FeatGroup group)
            => Common.CurrentText(group.NameKey, group.NameEn);

        public static string DescribeGroups(UnitDescriptor unit, string guid)
        {
            var owned = OwnedGuids(unit);
            var parts = GroupsOf(guid)
                .Select(group => (group, limit: GroupLimit(group)))
                .Where(entry => entry.limit.Enabled)
                .Select(entry => string.Format("{0} {1}/{2}", GroupName(entry.group),
                    GroupMembers[entry.group].Count(member => member != guid && owned.Contains(member)), entry.limit.Max));
            return string.Format(Common.CurrentText("FeatGroups.Member", "Exclusion groups (others owned/limit): {0}"),
                string.Join(", ", parts));
        }

        public static string Describe(FeatBudgetVerdict verdict)
        {
            var text = new StringBuilder();
            if (verdict.Limits.CountEnabled)
                text.Append(string.Format(Common.CurrentText("FeatBudget.Count",
                    "Homebrew feats used: {0}/{1}"), verdict.Usage.Count, verdict.Limits.MaxCount));
            if (verdict.Limits.PointsEnabled)
            {
                if (text.Length > 0) text.Append("; ");
                text.Append(string.Format(Common.CurrentText("FeatBudget.Points",
                    "Homebrew feat points used: {0}/{1}, this feat costs {2}"),
                    verdict.Usage.Points, verdict.Limits.MaxPoints, verdict.Cost));
            }
            if (verdict.OverBudget)
                text.Append(". ").Append(Common.CurrentText("FeatBudget.OverBudget",
                    "Over budget: existing feats are kept, but no more can be chosen until the limit is raised or the character is respecced"));
            else if (verdict.Block == FeatBudgetBlock.CountFull)
                text.Append(". ").Append(Common.CurrentText("FeatBudget.CountFull",
                    "No Homebrew feat slots remain"));
            else if (verdict.Block == FeatBudgetBlock.PointsShort)
                text.Append(". ").Append(string.Format(Common.CurrentText("FeatBudget.PointsShort",
                    "Requires {0} more points"), verdict.PointsShortBy));
            return text.ToString();
        }

        // Ignore-prerequisite effects (such as the Trickster's) skip MeetsPrerequisites.
        // The budget is a character-wide cap rather than a prerequisite, so keep it.
        // Exclusion groups behave like the vanilla mutual exclusions they replace.
        [HarmonyPatch(typeof(BlueprintFeatureSelection), nameof(BlueprintFeatureSelection.CanSelect))]
        private static class BlueprintFeatureSelection_CanSelect_Patch
        {
            [HarmonyPostfix]
            private static void Postfix(UnitDescriptor unit, IFeatureSelectionItem item, ref bool __result)
            {
                if (!__result || unit == null || !Limits.AnyEnabled || !TryGetCost(item?.Feature, out var cost)) return;
                __result = Evaluate(unit, cost).Allowed;
            }
        }
    }

    [AllowMultipleComponents]
    [TypeId("6d0f2c8e4b7a4f1c9a3e5d7b2c1f8e40")]
    public class PrerequisiteFeatBudget : Prerequisite
    {
        public int Cost = 1;

        protected override bool CheckInternal(FeatureSelectionState selectionState, UnitDescriptor unit, LevelUpState state)
            => !FeatBudget.Limits.AnyEnabled || FeatBudget.Evaluate(unit, Cost).Allowed;

        protected override string GetUITextInternal(UnitDescriptor unit)
            => unit == null
                ? string.Empty
                : FeatBudget.Describe(FeatBudget.Evaluate(unit, Cost));
    }

    [AllowMultipleComponents]
    [TypeId("b3e81d5a9c0f4e27a6d42f17c8e95b03")]
    public class PrerequisiteFeatGroups : Prerequisite
    {
        public string FeatGuid;

        protected override bool CheckInternal(FeatureSelectionState selectionState, UnitDescriptor unit, LevelUpState state)
            => FeatBudget.BlockingGroups(unit, FeatGuid).Count == 0;

        protected override string GetUITextInternal(UnitDescriptor unit)
            => unit == null ? string.Empty : FeatBudget.DescribeGroups(unit, FeatGuid);
    }
}
