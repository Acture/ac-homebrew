using System;
using System.Collections.Generic;
using System.Linq;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;

namespace ACHomebrew.Feats
{
    internal static class MainAbilityToEverything_Feats
    {
        private static readonly ModifierDescriptor Desc = ModifierDescriptor.Inherent;

        private static readonly StatType[] AttributeStats =
        {
            StatType.Strength,
            StatType.Dexterity,
            StatType.Constitution,
            StatType.Intelligence,
            StatType.Wisdom,
            StatType.Charisma,
        };

        private static readonly StatType[] BABStats =
        {
            StatType.BaseAttackBonus,
        };

        private static readonly StatType[] PowerStats =
        {
            StatType.AdditionalAttackBonus,
            StatType.AdditionalDamage,
            StatType.AttackOfOpportunityCount,
            StatType.SneakAttack,
            StatType.HitPoints,
            StatType.Speed,
        };

        private static bool Initialized;

        public static void ConfigureAll()
        {
            if (Initialized) return;
            Initialized = true;

            var feats = new List<BlueprintFeature>
            {
                CreateOne(
                    StatType.Strength,
                    "ApexPredator",
                    Guids.str_main_to_everything,
                    "MainAttr_Str.Name",
                    LocalizedText.Text("MainAttr_Str.Name", "Titan's Apotheosis"),
                    "MainAttr_Str.Desc",
                    BuildDescription(
                        LocalizedText.Text("Text.MainAbilityToEverything_Feats.ConfigureAll.Text", "Strength"),
                        LocalizedText.Text("MainAttr_Str.LoreTitle", "Primal Gravity."),
                        LocalizedText.Text("MainAttr_Str.Lore", "You train as though every task were a stone to be lifted, making strength the foundation of your whole bearing."))),
                CreateOne(
                    StatType.Dexterity,
                    "EmbodiedGrace",
                    Guids.dex_main_to_everything,
                    "MainAttr_Dex.Name",
                    LocalizedText.Text("MainAttr_Dex.Name", "Quicksilver Incarnate"),
                    "MainAttr_Dex.Desc",
                    BuildDescription(
                        LocalizedText.Text("Text.MainAbilityToEverything_Feats.ConfigureAll.Text.2", "Dexterity"),
                        LocalizedText.Text("MainAttr_Dex.LoreTitle", "Unanchored Motion."),
                        LocalizedText.Text("MainAttr_Dex.Lore", "Balance, breath, and movement flow together like quicksilver, carrying your discipline beyond the blade."))),
                CreateOne(
                    StatType.Constitution,
                    "LivingBulwark",
                    Guids.con_main_to_everything,
                    "MainAttr_Con.Name",
                    LocalizedText.Text("MainAttr_Con.Name", "Adamantine Vessel"),
                    "MainAttr_Con.Desc",
                    BuildDescription(
                        LocalizedText.Text("Text.MainAbilityToEverything_Feats.ConfigureAll.Text.3", "Constitution"),
                        LocalizedText.Text("MainAttr_Con.LoreTitle", "Crucible of the Mountain."),
                        LocalizedText.Text("MainAttr_Con.Lore", "Years of hardship have taught your body to meet every ordeal with the patience of tempered metal."))),
                CreateOne(
                    StatType.Intelligence,
                    "ArchitectOfSelf",
                    Guids.int_main_to_everything,
                    "MainAttr_Int.Name",
                    LocalizedText.Text("MainAttr_Int.Name", "Architect of Self"),
                    "MainAttr_Int.Desc",
                    BuildDescription(
                        LocalizedText.Text("Text.MainAbilityToEverything_Feats.ConfigureAll.Text.4", "Intelligence"),
                        LocalizedText.Text("MainAttr_Int.LoreTitle", "Anatomy of the Theorem."),
                        LocalizedText.Text("MainAttr_Int.Lore", "You study your own habits like an arcane diagram, turning careful understanding into practiced control."))),
                CreateOne(
                    StatType.Wisdom,
                    "WellspringOfInsight",
                    Guids.wis_main_to_everything,
                    "MainAttr_Wis.Name",
                    LocalizedText.Text("MainAttr_Wis.Name", "Ocular of the Cosmos"),
                    "MainAttr_Wis.Desc",
                    BuildDescription(
                        LocalizedText.Text("Text.MainAbilityToEverything_Feats.ConfigureAll.Text.5", "Wisdom"),
                        LocalizedText.Text("MainAttr_Wis.LoreTitle", "Still Waters of the Void."),
                        LocalizedText.Text("MainAttr_Wis.Lore", "Following the ideal of self-perfection associated with Irori, you make quiet attention the center of your practice."))),
                CreateOne(
                    StatType.Charisma,
                    "CrownOfWill",
                    Guids.cha_main_to_everything,
                    "MainAttr_Cha.Name",
                    LocalizedText.Text("MainAttr_Cha.Name", "Sovereign of Wills"),
                    "MainAttr_Cha.Desc",
                    BuildDescription(
                        LocalizedText.Text("Text.MainAbilityToEverything_Feats.ConfigureAll.Text.6", "Charisma"),
                        LocalizedText.Text("MainAttr_Cha.LoreTitle", "The Royal Decree."),
                        LocalizedText.Text("MainAttr_Cha.Lore", "Your confidence gathers scattered effort into a single purpose, as a sovereign gathers a wavering court."))),
            };

            foreach (var feat in feats)
            {
                FeatureConfigurator.For(feat).Configure();
            }

            ConfigureTargets();
        }

        private static readonly (StatType stat, string key, LocalizedText name, string icon)[] Attributes =
        {
            (StatType.Strength, "Str", LocalizedText.Text("Text.MainAbilityToEverything_Feats.Labels.Text", "Strength"), "ApexPredator"),
            (StatType.Dexterity, "Dex", LocalizedText.Text("Text.MainAbilityToEverything_Feats.Labels.Text.2", "Dexterity"), "EmbodiedGrace"),
            (StatType.Constitution, "Con", LocalizedText.Text("Text.MainAbilityToEverything_Feats.Labels.Text.3", "Constitution"), "LivingBulwark"),
            (StatType.Intelligence, "Int", LocalizedText.Text("Text.MainAbilityToEverything_Feats.Labels.Text.4", "Intelligence"), "ArchitectOfSelf"),
            (StatType.Wisdom, "Wis", LocalizedText.Text("Text.MainAbilityToEverything_Feats.Labels.Text.5", "Wisdom"), "WellspringOfInsight"),
            (StatType.Charisma, "Cha", LocalizedText.Text("Text.MainAbilityToEverything_Feats.Labels.Text.6", "Charisma"), "CrownOfWill"),
        };

        /// <summary>Main Attribute Mastery: one source attribute improves one chosen target attribute.</summary>
        private static void ConfigureTargets()
        {
            var menus = new[]
            {
                FeatSelection.MainFromStr, FeatSelection.MainFromDex, FeatSelection.MainFromCon,
                FeatSelection.MainFromInt, FeatSelection.MainFromWis, FeatSelection.MainFromCha,
            };
            for (var i = 0; i < Attributes.Length; i++)
            {
                foreach (var target in Attributes)
                {
                    if (target.stat != Attributes[i].stat)
                        CreateTarget(menus[i], Attributes[i], target);
                }
            }
        }

        private static void CreateTarget(FeatSelection menu,
            (StatType stat, string key, LocalizedText name, string icon) source,
            (StatType stat, string key, LocalizedText name, string icon) target)
        {
            var name = $"MainAttribute_{source.key}_{target.key}";
            var guid = (string)typeof(Guids.MainAttribute).GetNestedType(source.key).GetField(target.key).GetValue(null);
            var desc = LocalizedText.Format("Text.MainAbilityToEverything_Feats.CreateTarget.Description", "<i>Main Attribute Mastery · {0} to {1}</i>\n\n<b>Effect:</b> Add your {0} modifier (minimum 0) to your {1} score as an inherent bonus. Inherent bonuses do not stack with other inherent bonuses, such as those from tomes; only the highest applies. If the EnableBAB power option is on, it also adds half the modifier to Base Attack Bonus; with Power Mode on, half the modifier to attack, damage, attacks of opportunity, sneak attack, hit points and speed, plus 1 foot of reach.\n\n<b>Restrictions:</b> While the Main Attribute Mastery exclusion group is on, a character can have only one Main Attribute Mastery feat.", source.name, target.name);
            var cfg = menu.NewFeat(name, guid)
                .SetDisplayName(Common.L($"{name}.Name", LocalizedText.Format("Text.MainAbilityToEverything_Feats.CreateTarget.Text", "{0} Mastery: {1}", source.name, target.name)))
                .SetDescription(Common.L($"{name}.Desc", desc, tagEncyclopediaEntries: true))
                .SetIconIfPresent(source.icon);

            var settings = Mod.Settings ?? new ModSettings();
            Common.AddRank(cfg, source.stat, AbilityRankType.Default, Common.ResolveProgression(settings.powerLevel, ScalingIntent.Full));
            Common.AddRank(cfg, source.stat, AbilityRankType.StatBonus, Common.ResolveProgression(settings.powerLevel, ScalingIntent.Half));
            cfg.AddComponent<AddContextStatBonus>(c =>
            {
                c.Stat = target.stat;
                c.Descriptor = Desc;
                c.Value = Common.Rank(AbilityRankType.Default);
            });
            AddOptInPowerBonuses(cfg, settings);
            cfg.AddRecalculateOnStatChange(stat: source.stat);
            cfg.Configure();
        }

        // Opt-in settings documented as power options; unchanged from 0.1.x.
        private static void AddOptInPowerBonuses(FeatureConfigurator cfg, ModSettings settings)
        {
            if (settings.EnableBAB)
            {
                cfg.AddComponent<AddContextStatBonus>(c =>
                {
                    c.Stat = StatType.BaseAttackBonus;
                    c.Descriptor = Desc;
                    c.Value = Common.Rank(AbilityRankType.StatBonus);
                });
            }

            if (!settings.EnablePowerMode) return;
            foreach (var stat in PowerStats)
            {
                cfg.AddComponent<AddContextStatBonus>(c =>
                {
                    c.Stat = stat;
                    c.Descriptor = Desc;
                    c.Value = Common.Rank(AbilityRankType.StatBonus);
                });
            }
            cfg.AddComponent<AddStatBonus>(c =>
            {
                c.Stat = StatType.Reach;
                c.Value = 1;
                c.Descriptor = Desc;
            });
        }

        private static LocalizedText BuildDescription(
            LocalizedText attr,
            LocalizedText loreTitle,
            LocalizedText loreBody)
        {
            return LocalizedText.Format("Text.MainAbilityToEverything_Feats.BuildDescription.Description", "<i>Main Attribute Mastery (retired) · {0}</i>\n<i>{1}</i> {2}\n\n<b>Effect:</b> Adds half your {0} modifier (minimum 0, rounded down; the full modifier in Legacy_AllFull) to your other five ability scores as an inherent bonus, when attribute bonuses are enabled in mod settings. If self-stacking is enabled it also adds to {0}.\n\n<b>Retired:</b> This feat is no longer offered. Characters that already have it keep it. To switch to the new Main Attribute Mastery, which improves one chosen attribute by your full modifier, respec the character.", attr, loreTitle, loreBody);
        }

        private static BlueprintFeature CreateOne(
            StatType baseStat,
            string internalName,
            string guid,
            string nameKey,
            LocalizedText displayName,
            string descKey,
            LocalizedText desc)
        {
            var cfg = FeatSelection.MainLegacy.NewFeat(internalName, guid)
                .SetDisplayName(Common.L(nameKey, displayName))
                .SetDescription(Common.L(descKey, desc, tagEncyclopediaEntries: true))
                .SetIconIfPresent(internalName);

            var settings = Mod.Settings ?? new ModSettings();
            Common.AddRank(cfg, baseStat, AbilityRankType.Default, Common.ResolveProgression(settings.powerLevel, ScalingIntent.Full));
            Common.AddRank(cfg, baseStat, AbilityRankType.StatBonus, Common.ResolveProgression(settings.powerLevel, ScalingIntent.Half));

            void AddContextBonuses(IEnumerable<StatType> stats, AbilityRankType rankType)
            {
                foreach (var stat in stats)
                {
                    cfg.AddComponent<AddContextStatBonus>(c =>
                    {
                        c.Stat = stat;
                        c.Descriptor = Desc;
                        c.Value = Common.Rank(rankType);
                    });
                }
            }

            if (settings.EnableAttributes)
            {
                var attributeStats = settings.IncludeSelfInAttributeStack
                    ? AttributeStats
                    : AttributeStats.Where(stat => stat != baseStat);
                AddContextBonuses(attributeStats, AbilityRankType.StatBonus);
            }

            AddOptInPowerBonuses(cfg, settings);

            cfg.AddRecalculateOnStatChange(stat: baseStat);
            return cfg.Configure();
        }
    }
}
