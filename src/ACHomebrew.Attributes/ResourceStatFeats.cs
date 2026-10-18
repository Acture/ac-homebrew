using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Class.LevelUp;

namespace ACHomebrew.Feats
{
    /// <summary>
    /// One feat per attribute: choose one of your attribute-based resources (channel energy,
    /// ki, lay on hands, performance, bloodline or domain powers, ...) and count its bonus
    /// uses with that attribute. Overrides are runtime-only and read by GetMaxAmount.
    /// </summary>
    internal static class ResourceStatFeats
    {
        private static readonly Dictionary<BlueprintGuid, StatType> Feats = new();
        private static readonly AccessTools.FieldRef<BlueprintParametrizedFeature, FeatureUIData[]> CachedItems =
            AccessTools.FieldRefAccess<BlueprintParametrizedFeature, FeatureUIData[]>("m_CachedItems");
        private static bool Initialized;

        public static void ConfigureAll()
        {
            if (Initialized) return;
            Initialized = true;

            Create(StatType.Strength, "Str", LocalizedText.Text("Text.ResourceStatFeats.ConfigureAll.Text", "Strength"), LocalizedText.Text("Text.ResourceStatFeats.ConfigureAll.Text.2", "Reservoir of Might"), Guids.ResourceStat.Str);
            Create(StatType.Dexterity, "Dex", LocalizedText.Text("Text.ResourceStatFeats.ConfigureAll.Text.3", "Dexterity"), LocalizedText.Text("Text.ResourceStatFeats.ConfigureAll.Text.4", "Springs of Agility"), Guids.ResourceStat.Dex);
            Create(StatType.Constitution, "Con", LocalizedText.Text("Text.ResourceStatFeats.ConfigureAll.Text.5", "Constitution"), LocalizedText.Text("Text.ResourceStatFeats.ConfigureAll.Text.6", "Font of Endurance"), Guids.ResourceStat.Con);
            Create(StatType.Intelligence, "Int", LocalizedText.Text("Text.ResourceStatFeats.ConfigureAll.Text.7", "Intelligence"), LocalizedText.Text("Text.ResourceStatFeats.ConfigureAll.Text.8", "Well of Thought"), Guids.ResourceStat.Int);
            Create(StatType.Wisdom, "Wis", LocalizedText.Text("Text.ResourceStatFeats.ConfigureAll.Text.9", "Wisdom"), LocalizedText.Text("Text.ResourceStatFeats.ConfigureAll.Text.10", "Meditative Spring"), Guids.ResourceStat.Wis);
            Create(StatType.Charisma, "Cha", LocalizedText.Text("Text.ResourceStatFeats.ConfigureAll.Text.11", "Charisma"), LocalizedText.Text("Text.ResourceStatFeats.ConfigureAll.Text.12", "Sovereign Font"), Guids.ResourceStat.Cha);
        }

        public static bool TryGetAttribute(BlueprintFeature feature, out StatType attribute)
        {
            attribute = default;
            return feature != null && Feats.TryGetValue(feature.AssetGuid, out attribute);
        }

        private static void Create(StatType attribute, string key, LocalizedText attributeName, LocalizedText title, string guid)
        {
            var name = $"ResourceStat_{key}";
            var desc = LocalizedText.Format("Text.ResourceStatFeats.Create.desc", "<i>Resource Attribute · {0}</i>\n\n<b>Effect:</b> Choose one of your resources whose uses grow with an ability modifier, such as channel energy, ki, lay on hands, performance rounds, or bloodline and domain powers. Its bonus uses come from your {0} modifier instead. The new maximum applies the next time the resource is restored, for example after resting.\n\n<b>Restrictions:</b> Each resource can have only one resource attribute feat. You can take the feat again for another resource.", attributeName);
            desc = FeatLore.Apply(name, desc);
            var feat = FeatSelection.ResourceStat.NewParametrizedFeat(name, guid)
                .SetDisplayName(Common.L($"{name}.Name", title))
                .SetDescription(Common.L($"{name}.Desc", desc, tagEncyclopediaEntries: true))
                .SetIconIfPresent(name)
                .SetParameterType(FeatureParameterType.Custom)
                .AddComponent<ResourceAttributeOverride>(c => c.Attribute = attribute)
                .Configure();
            // Variants grow as characters with new resources open the selection.
            feat.BlueprintParameterVariants = new AnyBlueprintReference[0];
            Feats[feat.AssetGuid] = attribute;
        }

        private static bool IncreasedByStat(BlueprintAbilityResource resource, out StatType stat)
        {
            var amount = Traverse.Create(resource).Field("m_MaxAmount");
            stat = amount.Field("ResourceBonusStat").GetValue<StatType>();
            return amount.Field("IncreasedByStat").GetValue<bool>();
        }

        /// <summary>Attribute-based resources the unit has that this attribute can take over.</summary>
        private static IEnumerable<BlueprintAbilityResource> Candidates(UnitDescriptor unit, StatType attribute)
        {
            foreach (var blueprint in unit.Resources)
            {
                if (blueprint is BlueprintAbilityResource resource && CanChoose(unit, resource, attribute))
                    yield return resource;
            }
        }

        public static bool CanChoose(UnitDescriptor unit, BlueprintAbilityResource resource, StatType attribute)
            => unit != null && resource != null && unit.Resources.ContainsResource(resource)
                && IncreasedByStat(resource, out var stat) && stat != attribute
                && !ResourceStat.HasOverride(unit, resource);

        private static string DisplayName(UnitDescriptor unit, BlueprintAbilityResource resource)
        {
            if (!string.IsNullOrWhiteSpace(resource.Name)) return resource.Name;
            foreach (var ability in unit.Abilities)
                if (ability.Blueprint.GetComponent<AbilityResourceLogic>()?.RequiredResource == resource)
                    return ability.Name;
            foreach (var ability in unit.ActivatableAbilities)
                if (ability.Blueprint.GetComponent<ActivatableAbilityResourceLogic>()?.RequiredResource == resource)
                    return ability.Name;
            return resource.name;
        }

        private static FeatureParam ParamFor(BlueprintParametrizedFeature feature, BlueprintAbilityResource resource)
        {
            var existing = feature.GetFullSelectionItems().FirstOrDefault(item => item.Param?.Blueprint == resource);
            if (existing != null) return existing.Param;
            feature.BlueprintParameterVariants = feature.BlueprintParameterVariants
                .Append(resource.ToReference<AnyBlueprintReference>()).ToArray();
            CachedItems(feature) = null;
            return feature.GetFullSelectionItems().First(item => item.Param?.Blueprint == resource).Param;
        }

        [HarmonyPatch(typeof(BlueprintParametrizedFeature), nameof(BlueprintParametrizedFeature.ExtractSelectionItems))]
        private static class ExtractSelectionItems_Patch
        {
            [HarmonyPostfix]
            private static void Postfix(BlueprintParametrizedFeature __instance, UnitDescriptor beforeLevelUpUnit,
                UnitDescriptor previewUnit, ref IEnumerable<IFeatureSelectionItem> __result)
            {
                if (!TryGetAttribute(__instance, out var attribute)) return;
                var unit = previewUnit ?? beforeLevelUpUnit;
                if (unit == null) return;
                __result = Candidates(unit, attribute)
                    .Select(resource => (IFeatureSelectionItem)new FeatureUIData(__instance, ParamFor(__instance, resource),
                        DisplayName(unit, resource), resource.Description, resource.Icon, resource.name))
                    .ToArray();
            }
        }

        [HarmonyPatch(typeof(BlueprintParametrizedFeature), nameof(BlueprintParametrizedFeature.CanSelect))]
        private static class CanSelect_Patch
        {
            [HarmonyPostfix]
            private static void Postfix(BlueprintParametrizedFeature __instance, UnitDescriptor unit, IFeatureSelectionItem item, ref bool __result)
            {
                if (__result && TryGetAttribute(__instance, out var attribute))
                    __result = CanChoose(unit, item?.Param?.Blueprint as BlueprintAbilityResource, attribute);
            }
        }
    }

    [TypeId("5989fdc62434409abf3dde19bc2e3fec")]
    public class ResourceAttributeOverride : UnitFactComponentDelegate
    {
        public StatType Attribute;

        protected override void OnTurnOn() => ResourceStat.Register(Owner, Fact, Attribute, Param.Blueprint as BlueprintAbilityResource);

        protected override void OnTurnOff() => ResourceStat.Unregister(Owner, Fact);
    }

    internal static class ResourceStat
    {
        private sealed class Entry
        {
            public EntityFact Fact;
            public StatType Attribute;
            public BlueprintAbilityResource Resource;
        }

        private static readonly ConditionalWeakTable<UnitDescriptor, List<Entry>> Overrides = new();

        public static void Register(UnitDescriptor unit, EntityFact fact, StatType attribute, BlueprintAbilityResource resource)
        {
            var entries = Overrides.GetOrCreateValue(unit);
            entries.RemoveAll(entry => entry.Fact == fact);
            entries.Add(new Entry { Fact = fact, Attribute = attribute, Resource = resource });
        }

        public static void Unregister(UnitDescriptor unit, EntityFact fact)
        {
            if (Overrides.TryGetValue(unit, out var entries)) entries.RemoveAll(entry => entry.Fact == fact);
        }

        public static bool HasOverride(UnitDescriptor unit, BlueprintAbilityResource resource)
            => Overrides.TryGetValue(unit, out var entries) && entries.Any(entry => entry.Resource == resource);

        public static StatType Resolve(StatType original, BlueprintAbilityResource resource, UnitDescriptor unit)
        {
            if (unit == null || !Overrides.TryGetValue(unit, out var entries)) return original;
            foreach (var entry in entries)
                if (entry.Resource == resource) return entry.Attribute;
            return original;
        }

        // GetMaxAmount reads m_MaxAmount.ResourceBonusStat; resolve it per unit afterwards.
        [HarmonyPatch(typeof(BlueprintAbilityResource), nameof(BlueprintAbilityResource.GetMaxAmount))]
        private static class GetMaxAmount_Patch
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var field = AccessTools.Field(AccessTools.Inner(typeof(BlueprintAbilityResource), "Amount"), "ResourceBonusStat");
                var resolve = AccessTools.Method(typeof(ResourceStat), nameof(Resolve));
                var replaced = 0;
                foreach (var instruction in instructions)
                {
                    yield return instruction;
                    if (!instruction.LoadsField(field)) continue;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, resolve);
                    replaced++;
                }
                if (replaced == 0) Mod.Log?.Log("ACHomebrew: resource attribute read not found in GetMaxAmount.");
            }
        }
    }
}
