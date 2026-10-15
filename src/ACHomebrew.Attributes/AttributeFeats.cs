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
                    Common.Text("MainAttr_Str.Name", "Titan's Apotheosis"),
                    Common.Text("MainAttr_Str.Name", "泰坦登阶", true),
                    "MainAttr_Str.Desc",
                    BuildDescription(
                        Common.Text("Text.MainAbilityToEverything_Feats.ConfigureAll.Text", "Strength", false),
                        Common.Text("Text.MainAbilityToEverything_Feats.ConfigureAll.Text", "力量", true),
                        Common.Text("MainAttr_Str.LoreTitle", "Primal Gravity."),
                        Common.Text("MainAttr_Str.Lore", "You train as though every task were a stone to be lifted, making strength the foundation of your whole bearing."),
                        Common.Text("MainAttr_Str.LoreTitle", "原初重力。", true),
                        Common.Text("MainAttr_Str.Lore", "你将每一次试炼都视作待举之石，以力量奠定身心的根基。", true))),
                CreateOne(
                    StatType.Dexterity,
                    "EmbodiedGrace",
                    Guids.dex_main_to_everything,
                    "MainAttr_Dex.Name",
                    Common.Text("MainAttr_Dex.Name", "Quicksilver Incarnate"),
                    Common.Text("MainAttr_Dex.Name", "水银具现", true),
                    "MainAttr_Dex.Desc",
                    BuildDescription(
                        Common.Text("Text.MainAbilityToEverything_Feats.ConfigureAll.Text.2", "Dexterity", false),
                        Common.Text("Text.MainAbilityToEverything_Feats.ConfigureAll.Text.2", "敏捷", true),
                        Common.Text("MainAttr_Dex.LoreTitle", "Unanchored Motion."),
                        Common.Text("MainAttr_Dex.Lore", "Balance, breath, and movement flow together like quicksilver, carrying your discipline beyond the blade."),
                        Common.Text("MainAttr_Dex.LoreTitle", "不羁灵动。", true),
                        Common.Text("MainAttr_Dex.Lore", "步法、呼吸与重心如水银般连贯，将敏捷的修习带到兵刃之外。", true))),
                CreateOne(
                    StatType.Constitution,
                    "LivingBulwark",
                    Guids.con_main_to_everything,
                    "MainAttr_Con.Name",
                    Common.Text("MainAttr_Con.Name", "Adamantine Vessel"),
                    Common.Text("MainAttr_Con.Name", "万劫金身", true),
                    "MainAttr_Con.Desc",
                    BuildDescription(
                        Common.Text("Text.MainAbilityToEverything_Feats.ConfigureAll.Text.3", "Constitution", false),
                        Common.Text("Text.MainAbilityToEverything_Feats.ConfigureAll.Text.3", "体质", true),
                        Common.Text("MainAttr_Con.LoreTitle", "Crucible of the Mountain."),
                        Common.Text("MainAttr_Con.Lore", "Years of hardship have taught your body to meet every ordeal with the patience of tempered metal."),
                        Common.Text("MainAttr_Con.LoreTitle", "崇山熔炉。", true),
                        Common.Text("MainAttr_Con.Lore", "多年的磨砺让你的身躯学会以百炼金属般的沉稳迎接试炼。", true))),
                CreateOne(
                    StatType.Intelligence,
                    "ArchitectOfSelf",
                    Guids.int_main_to_everything,
                    "MainAttr_Int.Name",
                    Common.Text("MainAttr_Int.Name", "Architect of Self"),
                    Common.Text("MainAttr_Int.Name", "灵枢架构师", true),
                    "MainAttr_Int.Desc",
                    BuildDescription(
                        Common.Text("Text.MainAbilityToEverything_Feats.ConfigureAll.Text.4", "Intelligence", false),
                        Common.Text("Text.MainAbilityToEverything_Feats.ConfigureAll.Text.4", "智力", true),
                        Common.Text("MainAttr_Int.LoreTitle", "Anatomy of the Theorem."),
                        Common.Text("MainAttr_Int.Lore", "You study your own habits like an arcane diagram, turning careful understanding into practiced control."),
                        Common.Text("MainAttr_Int.LoreTitle", "定理形构。", true),
                        Common.Text("MainAttr_Int.Lore", "你如研读奥术图谱般审视自身习惯，以理解换来娴熟的控制。", true))),
                CreateOne(
                    StatType.Wisdom,
                    "WellspringOfInsight",
                    Guids.wis_main_to_everything,
                    "MainAttr_Wis.Name",
                    Common.Text("MainAttr_Wis.Name", "Ocular of the Cosmos"),
                    Common.Text("MainAttr_Wis.Name", "寰宇天心", true),
                    "MainAttr_Wis.Desc",
                    BuildDescription(
                        Common.Text("Text.MainAbilityToEverything_Feats.ConfigureAll.Text.5", "Wisdom", false),
                        Common.Text("Text.MainAbilityToEverything_Feats.ConfigureAll.Text.5", "感知", true),
                        Common.Text("MainAttr_Wis.LoreTitle", "Still Waters of the Void."),
                        Common.Text("MainAttr_Wis.Lore", "Following the ideal of self-perfection associated with Irori, you make quiet attention the center of your practice."),
                        Common.Text("MainAttr_Wis.LoreTitle", "灵渊止水。", true),
                        Common.Text("MainAttr_Wis.Lore", "你借鉴伊洛里的自我完善之道，以宁静而专注的觉察统摄修习。", true))),
                CreateOne(
                    StatType.Charisma,
                    "CrownOfWill",
                    Guids.cha_main_to_everything,
                    "MainAttr_Cha.Name",
                    Common.Text("MainAttr_Cha.Name", "Sovereign of Wills"),
                    Common.Text("MainAttr_Cha.Name", "至高皇威", true),
                    "MainAttr_Cha.Desc",
                    BuildDescription(
                        Common.Text("Text.MainAbilityToEverything_Feats.ConfigureAll.Text.6", "Charisma", false),
                        Common.Text("Text.MainAbilityToEverything_Feats.ConfigureAll.Text.6", "魅力", true),
                        Common.Text("MainAttr_Cha.LoreTitle", "The Royal Decree."),
                        Common.Text("MainAttr_Cha.Lore", "Your confidence gathers scattered effort into a single purpose, as a sovereign gathers a wavering court."),
                        Common.Text("MainAttr_Cha.LoreTitle", "王庭御令。", true),
                        Common.Text("MainAttr_Cha.Lore", "你的自信将纷乱的行动凝成一个目标，宛如君主整肃摇摆的王庭。", true))),
            };

            foreach (var feat in feats)
            {
                FeatureConfigurator.For(feat).Configure();
            }

            ConfigureTargets();
        }

        private static readonly (StatType stat, string key, string en, string zh, string icon)[] Attributes =
        {
            (StatType.Strength, "Str", Common.Text("Text.MainAbilityToEverything_Feats.Labels.Text", "Strength", false), Common.Text("Text.MainAbilityToEverything_Feats.Labels.Text", "力量", true), "ApexPredator"),
            (StatType.Dexterity, "Dex", Common.Text("Text.MainAbilityToEverything_Feats.Labels.Text.2", "Dexterity", false), Common.Text("Text.MainAbilityToEverything_Feats.Labels.Text.2", "敏捷", true), "EmbodiedGrace"),
            (StatType.Constitution, "Con", Common.Text("Text.MainAbilityToEverything_Feats.Labels.Text.3", "Constitution", false), Common.Text("Text.MainAbilityToEverything_Feats.Labels.Text.3", "体质", true), "LivingBulwark"),
            (StatType.Intelligence, "Int", Common.Text("Text.MainAbilityToEverything_Feats.Labels.Text.4", "Intelligence", false), Common.Text("Text.MainAbilityToEverything_Feats.Labels.Text.4", "智力", true), "ArchitectOfSelf"),
            (StatType.Wisdom, "Wis", Common.Text("Text.MainAbilityToEverything_Feats.Labels.Text.5", "Wisdom", false), Common.Text("Text.MainAbilityToEverything_Feats.Labels.Text.5", "感知", true), "WellspringOfInsight"),
            (StatType.Charisma, "Cha", Common.Text("Text.MainAbilityToEverything_Feats.Labels.Text.6", "Charisma", false), Common.Text("Text.MainAbilityToEverything_Feats.Labels.Text.6", "魅力", true), "CrownOfWill"),
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
            (StatType stat, string key, string en, string zh, string icon) source,
            (StatType stat, string key, string en, string zh, string icon) target)
        {
            var name = $"MainAttribute_{source.key}_{target.key}";
            var guid = (string)typeof(Guids.MainAttribute).GetNestedType(source.key).GetField(target.key).GetValue(null);
            var en = Common.Format("Text.MainAbilityToEverything_Feats.CreateTarget.Description", "<i>Main Attribute Mastery · {0} to {1}</i>\n\n<b>Effect:</b> Add your {0} modifier (minimum 0) to your {1} score as an inherent bonus. Inherent bonuses do not stack with other inherent bonuses, such as those from tomes; only the highest applies. If the EnableBAB power option is on, it also adds half the modifier to Base Attack Bonus; with Power Mode on, half the modifier to attack, damage, attacks of opportunity, sneak attack, hit points and speed, plus 1 foot of reach.\n\n<b>Restrictions:</b> While the Main Attribute Mastery exclusion group is on, a character can have only one Main Attribute Mastery feat.", false, source.en, target.en);
            var zh = Common.Format("Text.MainAbilityToEverything_Feats.CreateTarget.Description", "<i>主属性专精 · {0}转{1}</i>\n\n<b>效果：</b>将你的{0}调整值（最低0）作为固有加值加到你的{1}属性值上。固有加值不与其他固有加值（如典籍）叠加，只取最高。若开启强力选项 EnableBAB，还会将半数调整值加到基础攻击加值；开启威力模式时，还会将半数调整值加到攻击、伤害、借机攻击次数、偷袭、生命值与速度，并使触及+1尺。\n\n<b>限制：</b>启用主属性专精互斥组时，每个角色只能拥有一个主属性专精专长。", true, source.zh, target.zh);
            var cfg = menu.NewFeat(name, guid)
                .SetDisplayName(Common.L($"{name}.Name", Common.Format("Text.MainAbilityToEverything_Feats.CreateTarget.Text", "{0} Mastery: {1}", false, source.en, target.en), Common.Format("Text.MainAbilityToEverything_Feats.CreateTarget.Text", "{0}专精·{1}", true, source.zh, target.zh)))
                .SetDescription(Common.L($"{name}.Desc", en, zh, tagEncyclopediaEntries: true))
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

        private static (string en, string zh) BuildDescription(
            string attrEn,
            string attrZh,
            string loreTitleEn,
            string loreBodyEn,
            string loreTitleZh,
            string loreBodyZh)
        {
            var en = Common.Format("Text.MainAbilityToEverything_Feats.BuildDescription.Description", "<i>Main Attribute Mastery (retired) · {0}</i>\n<i>{1}</i> {2}\n\n<b>Effect:</b> Adds half your {0} modifier (minimum 0, rounded down; the full modifier in Legacy_AllFull) to your other five ability scores as an inherent bonus, when attribute bonuses are enabled in mod settings. If self-stacking is enabled it also adds to {0}.\n\n<b>Retired:</b> This feat is no longer offered. Characters that already have it keep it. To switch to the new Main Attribute Mastery, which improves one chosen attribute by your full modifier, respec the character.", false, attrEn, loreTitleEn, loreBodyEn);
            var zh = Common.Format("Text.MainAbilityToEverything_Feats.BuildDescription.Description", "<i>主属性专精（已停用） · {0}</i>\n<i>{1}</i> {2}\n\n<b>效果：</b>启用属性加成时，将你的{0}调整值的一半（最低0，向下取整；Legacy_AllFull下为完整调整值）作为固有加值加到其他五项属性上。若启用了自我属性叠加，则同样附加至{0}。\n\n<b>已停用：</b>此专长不再提供选择，已拥有的角色继续保留。若想换成新的主属性专精（以完整调整值提升一项所选属性），请为角色洗点。", true, attrZh, loreTitleZh, loreBodyZh);
            return (en, zh);
        }

        private static BlueprintFeature CreateOne(
            StatType baseStat,
            string internalName,
            string guid,
            string nameKey,
            string nameEn,
            string nameZh,
            string descKey,
            (string en, string zh) desc)
        {
            var cfg = FeatSelection.MainLegacy.NewFeat(internalName, guid)
                .SetDisplayName(Common.L(nameKey, nameEn, nameZh))
                .SetDescription(Common.L(descKey, desc.en, desc.zh, tagEncyclopediaEntries: true))
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
