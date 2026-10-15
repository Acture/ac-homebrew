using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using BlueprintCore.Blueprints.Configurators.Classes.Selection;
using HarmonyLib;
using Kingmaker.Assets.UnitLogic.Mechanics.Properties;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace ACHomebrew.Feats
{
    /// <summary>
    /// Strength, Dexterity or Constitution as a spellbook's casting attribute. The feat
    /// chooses a spellbook; the Casting Scope setting can extend it to all spellbooks.
    /// Overrides are runtime-only (no save data), applied where Wrath reads the attribute.
    /// </summary>
    internal static class CastingStatFeats
    {
        private static readonly HashSet<BlueprintGuid> FeatGuids = new();
        private static bool Initialized;

        public static void ConfigureAll()
        {
            if (Initialized) return;
            Initialized = true;

            var spellbooks = CollectSpellbooks();
            Create(StatType.Strength, "Str", Common.Text("Text.CastingStatFeats.ConfigureAll.Text", "Strength", false), Common.Text("Text.CastingStatFeats.ConfigureAll.Text", "力量", true), Common.Text("Text.CastingStatFeats.ConfigureAll.Text.2", "Somatic Force", false), Common.Text("Text.CastingStatFeats.ConfigureAll.Text.2", "力擎法脉", true), Guids.CastingStat.Str, spellbooks);
            Create(StatType.Dexterity, "Dex", Common.Text("Text.CastingStatFeats.ConfigureAll.Text.3", "Dexterity", false), Common.Text("Text.CastingStatFeats.ConfigureAll.Text.3", "敏捷", true), Common.Text("Text.CastingStatFeats.ConfigureAll.Text.4", "Sleight of Arcana", false), Common.Text("Text.CastingStatFeats.ConfigureAll.Text.4", "巧指弄玄", true), Guids.CastingStat.Dex, spellbooks);
            Create(StatType.Constitution, "Con", Common.Text("Text.CastingStatFeats.ConfigureAll.Text.5", "Constitution", false), Common.Text("Text.CastingStatFeats.ConfigureAll.Text.5", "体质", true), Common.Text("Text.CastingStatFeats.ConfigureAll.Text.6", "Sanguine Incantation", false), Common.Text("Text.CastingStatFeats.ConfigureAll.Text.6", "气血炼真", true), Guids.CastingStat.Con, spellbooks);
            Mod.Log?.Log($"AttributeFeats: casting attribute feats cover {spellbooks.Count} spellbooks.");
        }

        public static bool IsCastingFeat(BlueprintFeature feature) => feature != null && FeatGuids.Contains(feature.AssetGuid);

        // Class and archetype spellbooks, including mythic ones.
        private static List<BlueprintSpellbook> CollectSpellbooks()
        {
            var progression = BlueprintRoot.Instance.Progression;
            return progression.CharacterClasses.Concat(progression.CharacterMythics)
                .Where(characterClass => characterClass != null)
                .SelectMany(characterClass => new[] { characterClass.Spellbook }
                    .Concat(characterClass.Archetypes.Select(archetype => archetype?.ReplaceSpellbook)))
                .Where(spellbook => spellbook != null)
                .Distinct()
                .ToList();
        }

        private static void Create(StatType attribute, string key, string en, string zh, string titleEn, string titleZh, string guid, List<BlueprintSpellbook> spellbooks)
        {
            var name = $"CastingStat_{key}";
            var descEn = Common.Format("Text.CastingStatFeats.Create.desc", "<i>Casting Attribute · {0}</i>\n\n<b>Effect:</b> Choose one of your spellbooks. It uses {0} as its casting attribute: spell save DCs, bonus spell slots, the minimum score needed to cast each spell level, and concentration all use it.\n\n<b>Settings:</b> Casting Scope can make the feat apply to all your spellbooks instead; Casting Mode can make it apply only when {0} is higher than the spellbook's own attribute.\n\n<b>Restrictions:</b> Each spellbook can have only one casting attribute feat. You can take the feat again for another spellbook.", false, en);
            var descZh = Common.Format("Text.CastingStatFeats.Create.desc", "<i>施法属性 · {0}</i>\n\n<b>效果：</b>选择你的一本法术书，其施法属性改为{0}：法术豁免DC、额外法术位、各环法术所需的最低属性值与专注都改用该属性。\n\n<b>设置：</b>“施法范围”可改为作用于你的全部法术书；“施法模式”可改为仅在{0}高于法术书原属性时生效。\n\n<b>限制：</b>每本法术书只能有一个施法属性专长。可再次选取本专长用于另一本法术书。", true, zh);
            (descEn, descZh) = FeatLore.Apply(name, (descEn, descZh));
            var feat = FeatSelection.CastingStat.NewParametrizedFeat(name, guid)
                .SetDisplayName(Common.L($"{name}.Name", titleEn, titleZh))
                .SetDescription(Common.L($"{name}.Desc", descEn, descZh, tagEncyclopediaEntries: true))
                .SetIconIfPresent(name)
                .SetParameterType(FeatureParameterType.Custom)
                .AddComponent<CastingAttributeOverride>(c => c.Attribute = attribute)
                .Configure();
            // Parameter variants are every class spellbook; selection lists only the unit's own.
            feat.BlueprintParameterVariants = spellbooks.Select(book => book.ToReference<AnyBlueprintReference>()).ToArray();
            FeatGuids.Add(feat.AssetGuid);
        }
    }

    [TypeId("c6f02a876000468ca0bfbf0950a2861e")]
    public class CastingAttributeOverride : UnitFactComponentDelegate
    {
        public StatType Attribute;

        protected override void OnTurnOn() => CastingStat.Register(Owner, Fact, Attribute, Param.Blueprint as BlueprintSpellbook);

        protected override void OnTurnOff() => CastingStat.Unregister(Owner, Fact);
    }

    internal static class CastingStat
    {
        private sealed class Entry
        {
            public EntityFact Fact;
            public StatType Attribute;
            public BlueprintSpellbook Spellbook;
        }

        private static readonly ConditionalWeakTable<UnitDescriptor, List<Entry>> Overrides = new();

        public static void Register(UnitDescriptor unit, EntityFact fact, StatType attribute, BlueprintSpellbook spellbook)
        {
            var entries = Overrides.GetOrCreateValue(unit);
            entries.RemoveAll(entry => entry.Fact == fact);
            entries.Add(new Entry { Fact = fact, Attribute = attribute, Spellbook = spellbook });
        }

        public static void Unregister(UnitDescriptor unit, EntityFact fact)
        {
            if (Overrides.TryGetValue(unit, out var entries)) entries.RemoveAll(entry => entry.Fact == fact);
        }

        private static bool AllSpellbooks => Mod.Settings?.CastingScope == CastingAttributeScope.AllSpellbooks;

        // One casting feat per spellbook, whatever the scope; with AllSpellbooks the highest attribute wins.
        private static bool HasOverride(UnitDescriptor unit, BlueprintSpellbook spellbook)
            => Overrides.TryGetValue(unit, out var entries) && entries.Any(entry => entry.Spellbook == spellbook);

        /// <summary>The casting attribute <paramref name="unit"/> uses for <paramref name="spellbook"/>.</summary>
        public static StatType Resolve(BlueprintSpellbook spellbook, UnitDescriptor unit)
        {
            var original = spellbook.CastingAttribute;
            if (unit == null || !Overrides.TryGetValue(unit, out var entries) || entries.Count == 0) return original;

            var candidates = entries.Where(entry => AllSpellbooks || entry.Spellbook == spellbook)
                .Select(entry => entry.Attribute).ToList();
            if (candidates.Count == 0) return original;
            if (Mod.Settings?.CastingMode == CastingAttributeMode.IfHigher) candidates.Add(original);
            return candidates.OrderByDescending(stat => unit.Stats.GetStat(stat)?.ModifiedValue ?? 0).First();
        }

        /// <summary>Replacement for reading BlueprintSpellbook.CastingAttribute; ctx identifies the caster.</summary>
        public static StatType ResolveFor(BlueprintSpellbook spellbook, object ctx)
        {
            if (spellbook == null) throw new NullReferenceException();
            return Resolve(spellbook, UnitOf(ctx));
        }

        private static UnitDescriptor UnitOf(object ctx) => ctx switch
        {
            Spellbook book => book.Owner,
            RulebookEvent rule => rule.Initiator?.Descriptor,
            // Context is protected; this path is rare (casting-stat combat maneuvers).
            ContextAction action => Traverse.Create(action).Property("Context").GetValue<MechanicsContext>()?.MaybeCaster?.Descriptor,
            AbilityExecutionContext context => context.Caster?.Descriptor,
            MechanicsContext context => context.MaybeCaster?.Descriptor,
            UnitDescriptor unit => unit,
            UnitEntityData unit => unit.Descriptor,
            _ => null,
        };

        private static readonly Type[] ContextTypes =
        {
            typeof(Spellbook), typeof(RulebookEvent), typeof(ContextAction), typeof(AbilityExecutionContext),
            typeof(MechanicsContext), typeof(UnitDescriptor), typeof(UnitEntityData),
        };

        /// <summary>Argument index that identifies the caster: this, or the first suitable parameter.</summary>
        internal static int ContextArgument(MethodBase method)
        {
            bool Usable(Type type) => ContextTypes.Any(candidate => candidate.IsAssignableFrom(type));
            if (!method.IsStatic && Usable(method.DeclaringType)) return 0;
            var parameters = method.GetParameters();
            for (var i = 0; i < parameters.Length; i++)
                if (Usable(parameters[i].ParameterType)) return method.IsStatic ? i : i + 1;
            return -1;
        }

        /// <summary>Lists only the unit's spellbooks that do not already have a casting attribute feat.</summary>
        public static bool CanChoose(UnitDescriptor unit, BlueprintSpellbook spellbook)
        {
            if (unit == null || spellbook == null || unit.GetSpellbook(spellbook) == null) return false;
            return !HasOverride(unit, spellbook);
        }

        [HarmonyPatch]
        private static class CastingAttributeReaders
        {
            // Gameplay readers of BlueprintSpellbook.CastingAttribute (Wrath 2.x IL scan).
            // UI-only readers are left alone. Every overload of each name is patched.
            private static readonly (Type type, string[] names)[] Readers =
            {
                (typeof(Spellbook), new[] { "GetSpellsPerDay", "CantSpendReason", "SpendInternal", "GetConcentration", "HasEnoughCastingStat", "PostLoad" }),
                (typeof(RuleCalculateAbilityParams), new[] { nameof(RuleCalculateAbilityParams.OnTrigger) }),
                (typeof(ContextValue), new[] { nameof(ContextValue.GetAbilityParameter) }),
                (typeof(ContextActionCombatManeuver), new[] { nameof(ContextActionCombatManeuver.RunAction) }),
                (typeof(SpellSelectionData), new[] { nameof(SpellSelectionData.UpdateMaxLevelSpells) }),
                (typeof(CastingAttributeGetter), new[] { "GetBaseValue" }),
                (typeof(MaxCastingAttributeGetter), new[] { "GetBaseValue" }),
            };

            private static IEnumerable<MethodBase> TargetMethods()
                => Readers.SelectMany(reader => AccessTools.GetDeclaredMethods(reader.type)
                    .Where(method => reader.names.Contains(method.Name) && method.GetMethodBody() != null));

            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
            {
                var field = AccessTools.Field(typeof(BlueprintSpellbook), nameof(BlueprintSpellbook.CastingAttribute));
                var resolve = AccessTools.Method(typeof(CastingStat), nameof(ResolveFor));
                var argument = ContextArgument(original);
                var replaced = 0;
                foreach (var instruction in instructions)
                {
                    if (argument >= 0 && instruction.LoadsField(field))
                    {
                        // Stack: spellbook blueprint -> push caster context -> resolved attribute.
                        var load = CodeInstruction.LoadArgument(argument);
                        load.MoveLabelsFrom(instruction);
                        load.MoveBlocksFrom(instruction);
                        yield return load;
                        yield return new CodeInstruction(OpCodes.Call, resolve);
                        replaced++;
                        continue;
                    }
                    yield return instruction;
                }
                if (replaced == 0)
                    Mod.Log?.Log($"AttributeFeats: casting attribute read not found in {original.DeclaringType?.Name}.{original.Name}.");
            }
        }

        // Parameter lists show only the unit's own spellbooks without a casting attribute feat.
        [HarmonyPatch(typeof(BlueprintParametrizedFeature), nameof(BlueprintParametrizedFeature.ExtractSelectionItems))]
        private static class ExtractSelectionItems_Patch
        {
            [HarmonyPostfix]
            private static void Postfix(BlueprintParametrizedFeature __instance, UnitDescriptor beforeLevelUpUnit,
                UnitDescriptor previewUnit, ref IEnumerable<IFeatureSelectionItem> __result)
            {
                if (!CastingStatFeats.IsCastingFeat(__instance)) return;
                var unit = previewUnit ?? beforeLevelUpUnit;
                __result = __result.Where(item => CanChoose(unit, item.Param?.Blueprint as BlueprintSpellbook)).ToArray();
            }
        }

        [HarmonyPatch(typeof(BlueprintParametrizedFeature), nameof(BlueprintParametrizedFeature.CanSelect))]
        private static class CanSelect_Patch
        {
            [HarmonyPostfix]
            private static void Postfix(BlueprintParametrizedFeature __instance, UnitDescriptor unit, IFeatureSelectionItem item, ref bool __result)
            {
                if (__result && CastingStatFeats.IsCastingFeat(__instance))
                    __result = CanChoose(unit, item?.Param?.Blueprint as BlueprintSpellbook);
            }
        }
    }
}
