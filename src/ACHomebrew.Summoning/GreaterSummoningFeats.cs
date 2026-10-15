using System.Collections.Generic;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Buffs;
using BlueprintCore.Utils.Types;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Mechanics.Components;

namespace ACHomebrew.Feats
{
    internal static class GreaterSummoningFeats
    {
        private static readonly ModifierDescriptor Desc = ModifierDescriptor.None;
        private static bool Initialized;

        public static void ConfigureAll()
        {
            if (Initialized) return;
            Initialized = true;

            var feats = new[]
            {
                CreateFeat(
                    internalName: "BloodlineOfBeasts",
                    featureGuid: Guids.Summon.Feature.BloodlineOfBeasts,
                    outerBuffGuid: Guids.Summon.OuterBuff.BloodlineOfBeasts,
                    innerBuffGuid: Guids.Summon.InnerBuff.BloodlineOfBeasts,
                    nameEn: Common.Text("Summon_BloodlineOfBeasts.Name", "Behemoth's Heritage"),
                    nameZh: Common.Text("Summon_BloodlineOfBeasts.Name", "比蒙遗脉", true),
                    desc: BuildDescription(
                        Common.Text("Text.GreaterSummoningFeats.ConfigureAll.Text", "Strength", false),
                        Common.Text("Text.GreaterSummoningFeats.ConfigureAll.Text", "力量", true),
                        Common.Text("Summon_BloodlineOfBeasts.Lore", "You lend your summoned companions the forceful bearing that guides your own movements."),
                        Common.Text("Summon_BloodlineOfBeasts.Lore", "你将支配自身行动的强劲力量借给召来的伙伴。", true),
                        Common.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.2", "Your summoned creatures gain an untyped bonus to Strength equal to your Strength modifier.", false),
                        Common.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.2", "你召唤的生物获得等同于你的力量调整值的无类型力量加值。", true)),
                    baseStat: StatType.Strength,
                    buffedStats: new[] { StatType.Strength }),
                CreateFeat(
                    internalName: "QuickenedPact",
                    featureGuid: Guids.Summon.Feature.QuickenedPact,
                    outerBuffGuid: Guids.Summon.OuterBuff.QuickenedPact,
                    innerBuffGuid: Guids.Summon.InnerBuff.QuickenedPact,
                    nameEn: Common.Text("Summon_QuickenedPact.Name", "Zephyr's Covenant"),
                    nameZh: Common.Text("Summon_QuickenedPact.Name", "风灵疾契", true),
                    desc: BuildDescription(
                        Common.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.3", "Dexterity", false),
                        Common.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.3", "敏捷", true),
                        Common.Text("Summon_QuickenedPact.Lore", "The rhythm of your agile steps becomes a pattern for the creatures answering your call."),
                        Common.Text("Summon_QuickenedPact.Lore", "你灵巧步伐的节奏，成为回应召唤的生物可以追随的范式。", true),
                        Common.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.4", "Your summoned creatures gain untyped bonuses to Dexterity and Speed equal to your Dexterity modifier.", false),
                        Common.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.4", "你召唤的生物获得等同于你的敏捷调整值的无类型敏捷与移动速度加值。", true)),
                    baseStat: StatType.Dexterity,
                    buffedStats: new[] { StatType.Dexterity, StatType.Speed }),
                CreateFeat(
                    internalName: "VitalPact",
                    featureGuid: Guids.Summon.Feature.VitalPact,
                    outerBuffGuid: Guids.Summon.OuterBuff.VitalPact,
                    innerBuffGuid: Guids.Summon.InnerBuff.VitalPact,
                    nameEn: Common.Text("Summon_VitalPact.Name", "Titan's Lifespring"),
                    nameZh: Common.Text("Summon_VitalPact.Name", "巨怪生机", true),
                    desc: BuildDescription(
                        Common.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.5", "Constitution", false),
                        Common.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.5", "体质", true),
                        Common.Text("Summon_VitalPact.Lore", "You draw on your own hardiness when preparing a body for a summoned companion."),
                        Common.Text("Summon_VitalPact.Lore", "为召来的伙伴塑成躯体时，你借鉴自身承受磨砺的耐力。", true),
                        Common.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.6", "Your summoned creatures gain an untyped bonus to Constitution equal to your Constitution modifier.", false),
                        Common.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.6", "你召唤的生物获得等同于你的体质调整值的无类型体质加值。", true)),
                    baseStat: StatType.Constitution,
                    buffedStats: new[] { StatType.Constitution }),
                CreateFeat(
                    internalName: "TacticalBinding",
                    featureGuid: Guids.Summon.Feature.TacticalBinding,
                    outerBuffGuid: Guids.Summon.OuterBuff.TacticalBinding,
                    innerBuffGuid: Guids.Summon.InnerBuff.TacticalBinding,
                    nameEn: Common.Text("Summon_TacticalBinding.Name", "Aegis of the Schema"),
                    nameZh: Common.Text("Summon_TacticalBinding.Name", "天元魔阵", true),
                    desc: BuildDescription(
                        Common.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.7", "Intelligence", false),
                        Common.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.7", "智力", true),
                        Common.Text("Summon_TacticalBinding.Lore", "You plan a summoned creature's defenses as carefully as an architect plans a wall."),
                        Common.Text("Summon_TacticalBinding.Lore", "你如建筑师设计城墙般，仔细安排召唤生物的防守。", true),
                        Common.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.8", "Your summoned creatures gain an untyped bonus to AC equal to your Intelligence modifier.", false),
                        Common.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.8", "你召唤的生物获得等同于你的智力调整值的无类型防御等级（AC）加值。", true)),
                    baseStat: StatType.Intelligence,
                    buffedStats: new[] { StatType.AC }),
                CreateFeat(
                    internalName: "InsightfulSummons",
                    featureGuid: Guids.Summon.Feature.InsightfulSummons,
                    outerBuffGuid: Guids.Summon.OuterBuff.InsightfulSummons,
                    innerBuffGuid: Guids.Summon.InnerBuff.InsightfulSummons,
                    nameEn: Common.Text("Summon_InsightfulSummons.Name", "Empathic Communion"),
                    nameZh: Common.Text("Summon_InsightfulSummons.Name", "神契灵犀", true),
                    desc: BuildDescription(
                        Common.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.9", "Wisdom", false),
                        Common.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.9", "感知", true),
                        Common.Text("Summon_InsightfulSummons.Lore", "Attentive guidance helps a summoned companion meet danger with steadier instincts."),
                        Common.Text("Summon_InsightfulSummons.Lore", "专注的引导帮助召来的伙伴，以更稳固的本能迎接危险。", true),
                        Common.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.10", "Your summoned creatures gain untyped bonuses to Fortitude, Reflex, and Will saves equal to your Wisdom modifier.", false),
                        Common.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.10", "你召唤的生物获得等同于你的感知调整值的强韧、反射与意志豁免检定无类型加值。", true)),
                    baseStat: StatType.Wisdom,
                    buffedStats: new[] { StatType.SaveFortitude, StatType.SaveReflex, StatType.SaveWill }),
                CreateFeat(
                    internalName: "MagneticCalling",
                    featureGuid: Guids.Summon.Feature.MagneticCalling,
                    outerBuffGuid: Guids.Summon.OuterBuff.MagneticCalling,
                    innerBuffGuid: Guids.Summon.InnerBuff.MagneticCalling,
                    nameEn: Common.Text("Summon_MagneticCalling.Name", "Dominator's Calling"),
                    nameZh: Common.Text("Summon_MagneticCalling.Name", "御统王令", true),
                    desc: BuildDescription(
                        Common.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.11", "Charisma", false),
                        Common.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.11", "魅力", true),
                        Common.Text("Summon_MagneticCalling.Lore", "Your confident command gives summoned companions a clear purpose when they enter the fray."),
                        Common.Text("Summon_MagneticCalling.Lore", "自信的指挥让召来的伙伴在投入战斗时拥有明确目标。", true),
                        Common.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.12", "Your summoned creatures gain an untyped bonus to attack rolls equal to your Charisma modifier.", false),
                        Common.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.12", "你召唤的生物获得等同于你的魅力调整值的无类型攻击检定加值。", true)),
                    baseStat: StatType.Charisma,
                    buffedStats: new[] { StatType.AdditionalAttackBonus }),
            };

        }

        private static BlueprintFeature CreateFeat(
            string internalName,
            string featureGuid,
            string outerBuffGuid,
            string innerBuffGuid,
            string nameEn,
            string nameZh,
            (string en, string zh) desc,
            StatType baseStat,
            IReadOnlyList<StatType> buffedStats)
        {
            var displayName = Common.L($"Summon_{internalName}.Name", nameEn, nameZh);
            var description = Common.L(
                $"Summon_{internalName}.Desc",
                desc.en,
                desc.zh,
                tagEncyclopediaEntries: true);

            var innerBuff = BuffConfigurator.New($"{internalName}InnerBuff", innerBuffGuid)
                .SetDisplayName(displayName)
                .SetDescription(description)
                .SetIconIfPresent(internalName);
            AddRank(innerBuff, baseStat);
            AddContextBonuses(innerBuff, buffedStats);
            var configuredInnerBuff = innerBuff.Configure();

            var outerBuff = BuffConfigurator.New($"{internalName}OuterBuff", outerBuffGuid)
                .SetDisplayName(displayName)
                .SetDescription(description)
                .SetIconIfPresent(internalName)
                .AddOnSpawnBuff(buff: configuredInnerBuff, isInfinity: true)
                .Configure();

            return FeatSelection.GreaterSummoning.NewFeat(internalName, featureGuid)
                .SetDisplayName(displayName)
                .SetDescription(description)
                .SetIconIfPresent(internalName)
                .AddFacts(new() { outerBuff })
                .Configure();
        }

        private static void AddRank(BuffConfigurator cfg, StatType baseStat)
        {
            cfg.AddContextRankConfig(ContextRankConfigs.StatBonus(baseStat, ModifierDescriptor.None, AbilityRankType.Default, min: 0));
        }

        private static void AddContextBonuses(BuffConfigurator cfg, IReadOnlyList<StatType> stats)
        {
            foreach (var stat in stats)
            {
                cfg.AddContextStatBonus(stat, Common.Rank(), Desc);
            }
        }


        private static (string en, string zh) BuildDescription(
            string attrEn,
            string attrZh,
            string loreEn,
            string loreZh,
            string effectEn,
            string effectZh)
        {
            var en = Common.Format("Text.GreaterSummoningFeats.BuildDescription.Description", "<i>Greater Summoning · {0}</i>\n{1}\n\n<b>Effect:</b> {2} Attribute modifiers used for these bonuses and matching penalties have a minimum of 0.\n\n<b>Restrictions:</b> While its exclusion group is on (configurable in mod settings), you can have only one Greater Summoning feat.", false, attrEn, loreEn, effectEn);
            var zh = Common.Format("Text.GreaterSummoningFeats.BuildDescription.Description", "<i>高等召唤 · {0}</i>\n{1}\n\n<b>效果：</b>{2}这些加值及对应减值均以属性调整值最低0计算。\n\n<b>限制：</b>启用该互斥组时（可在模组设置中调整），只能拥有一个“高等召唤”专长。", true, attrZh, loreZh, effectZh);
            return (en, zh);
        }
    }
}
