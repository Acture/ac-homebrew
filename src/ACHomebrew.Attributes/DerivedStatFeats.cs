using System.Collections.Generic;
using BlueprintCore.Actions.Builder;
using BlueprintCore.Actions.Builder.ContextEx;
using BlueprintCore.Blueprints.Configurators.UnitLogic.Properties;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Buffs;
using BlueprintCore.Utils;
using BlueprintCore.Utils.Types;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Properties;

namespace ACHomebrew.Feats
{
    internal static class DerivedStatFeats
    {
        private const string SkilledDefenderSkillRanksPropertyGuid = "391e1155-2021-473f-a216-ab01f3f5e500";
        private const string SoulBulwarkTempBuffGuid = "dbbbf07b-3c5f-4d4c-84a9-952f531e242e";
        private static readonly ModifierDescriptor Desc = ModifierDescriptor.None;
        private static readonly StatType[] SaveStats =
        {
            StatType.SaveFortitude,
            StatType.SaveReflex,
            StatType.SaveWill,
        };

        private static readonly StatType[] SkillStats =
        {
            StatType.SkillAthletics,
            StatType.SkillKnowledgeArcana,
            StatType.SkillKnowledgeWorld,
            StatType.SkillLoreNature,
            StatType.SkillLoreReligion,
            StatType.SkillMobility,
            StatType.SkillPerception,
            StatType.SkillPersuasion,
            StatType.SkillStealth,
            StatType.SkillThievery,
            StatType.SkillUseMagicDevice,
        };

        private static bool Initialized;

        public static void ConfigureAll()
        {
            if (Initialized) return;
            Initialized = true;

            CreateSkilledDefenderSkillRanksProperty();
            CreateSoulBulwarkTempBuff();
            CreateSoulBulwarkTriggerBuff();
            CreateArcaneAegis();
            CreateMartialInsight();
            CreateSkilledDefender();
            CreateMysticVitality();
            CreateSoulBulwark();
            CreateSwordSaint();
        }

        private static void CreateArcaneAegis()
        {
            NewFeature(
                internalName: "ArcaneAegis",
                guid: Guids.Derived.ArcaneAegis,
                nameKey: "Derived_ArcaneAegis.Name",
                nameEn: Common.Text("Derived_ArcaneAegis.Name", "Arcane Aegis"),
                nameZh: Common.Text("Derived_ArcaneAegis.Name", "魔能天衣", true),
                descKey: "Derived_ArcaneAegis.Desc",
                desc: BuildDescription(
                    Common.Text("Text.DerivedStatFeats.CreateArcaneAegis.Text", "Caster Level to AC", false),
                    Common.Text("Text.DerivedStatFeats.CreateArcaneAegis.Text", "施法者等级转化AC", true),
                    Common.Text("Derived_ArcaneAegis.Lore", "Years of spellwork teach you to turn practiced magical control toward personal defense."),
                    Common.Text("Derived_ArcaneAegis.Lore", "多年的施法修习，让你学会将娴熟的魔力控制用于自身防守。", true),
                    Common.Text("Text.DerivedStatFeats.CreateArcaneAegis.Text.2", "Adds half your caster level as an untyped bonus to AC.", false),
                    Common.Text("Text.DerivedStatFeats.CreateArcaneAegis.Text.2", "将你的施法者等级的一半作为无类型加值附加至防御等级（AC）。", true)))
                .AddContextRankConfig(ContextRankConfigs.CasterLevel(min: 0).WithDiv2Progression())
                .AddContextStatBonus(StatType.AC, Common.Rank(), descriptor: Desc)
                .Configure();
        }

        private static void CreateMartialInsight()
        {
            var cfg = NewFeature(
                internalName: "MartialInsight",
                guid: Guids.Derived.MartialInsight,
                nameKey: "Derived_MartialInsight.Name",
                nameEn: Common.Text("Derived_MartialInsight.Name", "War-Hardened Reflexes"),
                nameZh: Common.Text("Derived_MartialInsight.Name", "百战身魄", true),
                descKey: "Derived_MartialInsight.Desc",
                desc: BuildDescription(
                    Common.Text("Text.DerivedStatFeats.CreateMartialInsight.Text", "Base Attack Bonus to Saves", false),
                    Common.Text("Text.DerivedStatFeats.CreateMartialInsight.Text", "基础攻击加值转化豁免", true),
                    Common.Text("Derived_MartialInsight.Lore", "Battle has trained your responses until endurance, movement, and resolve share the same rhythm."),
                    Common.Text("Derived_MartialInsight.Lore", "战斗锤炼你的反应，使耐力、行动与意志有了共同的节律。", true),
                    Common.Text("Text.DerivedStatFeats.CreateMartialInsight.Text.2", "Adds half your base attack bonus as an untyped bonus to Fortitude, Reflex, and Will saving throws.", false),
                    Common.Text("Text.DerivedStatFeats.CreateMartialInsight.Text.2", "将你的基础攻击加值（BAB）的一半作为无类型加值附加至强韧、反射与意志豁免检定。", true)))
                .AddContextRankConfig(ContextRankConfigs.BaseAttack(min: 0).WithDiv2Progression());

            foreach (var stat in SaveStats)
            {
                cfg.AddContextStatBonus(stat, Common.Rank(), descriptor: Desc);
            }

            cfg.Configure();
        }

        private static void CreateSkilledDefender()
        {
            NewFeature(
                internalName: "SkilledDefender",
                guid: Guids.Derived.SkilledDefender,
                nameKey: "Derived_SkilledDefender.Name",
                nameEn: Common.Text("Derived_SkilledDefender.Name", "Scholar's Positioning"),
                nameZh: Common.Text("Derived_SkilledDefender.Name", "通识御敌", true),
                descKey: "Derived_SkilledDefender.Desc",
                desc: BuildDescription(
                    Common.Text("Text.DerivedStatFeats.CreateSkilledDefender.Text", "Total Skill Ranks to AC", false),
                    Common.Text("Text.DerivedStatFeats.CreateSkilledDefender.Text", "技能总点数转化AC", true),
                    Common.Text("Derived_SkilledDefender.Lore", "Lessons gathered from many crafts become practical answers to an enemy's approach."),
                    Common.Text("Derived_SkilledDefender.Lore", "从各门技艺中积累的经验，成为你应对敌人来势的实际手段。", true),
                    Common.Text("Text.DerivedStatFeats.CreateSkilledDefender.Text.2", "Adds one third of your total skill ranks as an untyped bonus to AC.", false),
                    Common.Text("Text.DerivedStatFeats.CreateSkilledDefender.Text.2", "将你所有技能总点数的三分之一作为无类型加值附加至防御等级（AC）。", true)))
                .AddContextRankConfig(ContextRankConfigs.CustomProperty(SkilledDefenderSkillRanksPropertyGuid, min: 0).WithDivStepProgression(3))
                .AddContextStatBonus(StatType.AC, Common.Rank(), descriptor: Desc)
                .Configure();
        }

        private static void CreateMysticVitality()
        {
            NewFeature(
                internalName: "MysticVitality",
                guid: Guids.Derived.MysticVitality,
                nameKey: "Derived_MysticVitality.Name",
                nameEn: Common.Text("Derived_MysticVitality.Name", "Ley-Infused Vitality"),
                nameZh: Common.Text("Derived_MysticVitality.Name", "灵脉淬体", true),
                descKey: "Derived_MysticVitality.Desc",
                desc: BuildDescription(
                    Common.Text("Text.DerivedStatFeats.CreateMysticVitality.Text", "Caster Level to Hit Points", false),
                    Common.Text("Text.DerivedStatFeats.CreateMysticVitality.Text", "施法者等级转化生命", true),
                    Common.Text("Derived_MysticVitality.Lore", "You make the discipline of channeling magic part of the discipline of sustaining yourself."),
                    Common.Text("Derived_MysticVitality.Lore", "你将引导魔法的修习融入维持自身生机的修习。", true),
                    Common.Text("Text.DerivedStatFeats.CreateMysticVitality.Text.2", "Adds your caster level as an untyped bonus to Hit Points.", false),
                    Common.Text("Text.DerivedStatFeats.CreateMysticVitality.Text.2", "将你的施法者等级作为无类型加值附加至生命值上限（HP）。", true)))
                .AddContextRankConfig(ContextRankConfigs.CasterLevel(min: 0))
                .AddContextStatBonus(StatType.HitPoints, Common.Rank(), descriptor: Desc)
                .Configure();
        }

        private static void CreateSoulBulwark()
        {
            NewFeature(
                internalName: "SoulBulwark",
                guid: Guids.Derived.SoulBulwark,
                nameKey: "Derived_SoulBulwark.Name",
                nameEn: Common.Text("Derived_SoulBulwark.Name", "Dawn of the Soul"),
                nameZh: Common.Text("Derived_SoulBulwark.Name", "法相初明", true),
                descKey: "Derived_SoulBulwark.Desc",
                desc: BuildDescription(
                    Common.Text("Text.DerivedStatFeats.CreateSoulBulwark.Text", "Caster Level to Temporary Hit Points", false),
                    Common.Text("Text.DerivedStatFeats.CreateSoulBulwark.Text", "施法者等级转化临时生命", true),
                    Common.Text("Derived_SoulBulwark.Lore", "When battle begins, your practiced spellwork gathers around you like a brief mantle of dawn."),
                    Common.Text("Derived_SoulBulwark.Lore", "战斗开始时，娴熟的施法力量如短暂的晨辉般聚拢在你周围。", true),
                    Common.Text("Text.DerivedStatFeats.CreateSoulBulwark.Text.2", "At the start of combat, gain temporary hit points equal to your caster level for up to 10 minutes or until combat ends, whichever comes first.", false),
                    Common.Text("Text.DerivedStatFeats.CreateSoulBulwark.Text.2", "战斗开始时获得等同于施法者等级的临时生命值，持续至多10分钟或至战斗结束，以先到者为准。", true)))
                .AddFacts(facts: new List<Blueprint<BlueprintUnitFactReference>> { Guids.Derived.SoulBulwarkBuff })
                .Configure();
        }

        private static void CreateSwordSaint()
        {
            NewFeature(
                internalName: "SwordSaint",
                guid: Guids.Derived.SwordSaint,
                nameKey: "Derived_SwordSaint.Name",
                nameEn: Common.Text("Derived_SwordSaint.Name", "Blade of the Spell-Saint"),
                nameZh: Common.Text("Derived_SwordSaint.Name", "剑圣咒痕", true),
                descKey: "Derived_SwordSaint.Desc",
                desc: BuildDescription(
                    Common.Text("Text.DerivedStatFeats.CreateSwordSaint.Text", "Base Attack Bonus to Spell DC", false),
                    Common.Text("Text.DerivedStatFeats.CreateSwordSaint.Text", "基础攻击加值转化法术DC", true),
                    Common.Text("Derived_SwordSaint.Lore", "The precision learned with a weapon gives you another way to shape demanding magic."),
                    Common.Text("Derived_SwordSaint.Lore", "从兵刃上学来的精准，为你驾驭复杂魔法提供另一条途径。", true),
                    Common.Text("Text.DerivedStatFeats.CreateSwordSaint.Text.2", "Adds half your base attack bonus as an untyped bonus to the save DC of your spells. Class and racial abilities are not affected.", false),
                    Common.Text("Text.DerivedStatFeats.CreateSwordSaint.Text.2", "将你的基础攻击加值（BAB）的一半作为无类型加值附加至你的法术豁免难度等级（DC）。职业与种族能力不受影响。", true)))
                .AddContextRankConfig(ContextRankConfigs.BaseAttack(min: 0).WithDiv2Progression())
                .AddIncreaseAllSpellsDC(descriptor: Desc, spellsOnly: true, value: Common.Rank())
                .Configure();
        }

        private static void CreateSkilledDefenderSkillRanksProperty()
        {
            var cfg = UnitPropertyConfigurator.New("SkilledDefenderSkillRanksProperty", SkilledDefenderSkillRanksPropertyGuid)
                .SetBaseValue(0)
                .SetOperationOnComponents(BlueprintUnitProperty.MathOperation.Sum);

            foreach (var stat in SkillStats)
            {
                cfg.AddSkillRankGetter(new PropertySettings(), stat);
            }

            cfg.Configure();
        }

        private static void CreateSoulBulwarkTempBuff()
        {
            BuffConfigurator.New("SoulBulwarkTempBuff", SoulBulwarkTempBuffGuid)
                .SetDisplayName(Common.L("Derived_SoulBulwark.Temp.Name", Common.Text("Derived_SoulBulwark.Name", "Dawn of the Soul"), Common.Text("Derived_SoulBulwark.Name", "法相初明", true)))
                .SetDescription(Common.L(
                    "Derived_SoulBulwark.Temp.Desc",
                    Common.Text("Derived_SoulBulwark.Temp.Desc", "<i>Derived · Caster Level to Temporary Hit Points</i>\nA reserve of radiant psychic force gathers around your soul for up to 10 minutes or until combat ends, whichever comes first, absorbing damage before your mortal flesh yields.", false),
                    Common.Text("Derived_SoulBulwark.Temp.Desc", "<i>衍生属性 · 施法者等级转化临时生命</i>\n璀璨心能屏障护佑周身，持续至多10分钟或至战斗结束，以先到者为准，在血肉凡胎受创前优先抵御伤害。", true),
                    tagEncyclopediaEntries: true))
                .SetIconIfPresent("SoulBulwark")
                .SetStacking(StackingType.Replace)
                .AddContextRankConfig(ContextRankConfigs.CasterLevel(min: 0))
                .AddTemporaryHitPointsFromAbilityValue(descriptor: Desc, removeWhenHitPointsEnd: false, value: Common.Rank())
                .Configure();
        }

        private static void CreateSoulBulwarkTriggerBuff()
        {
            BuffConfigurator.New("SoulBulwarkTriggerBuff", Guids.Derived.SoulBulwarkBuff)
                .SetDisplayName(Common.L("Derived_SoulBulwark.Buff.Name", Common.Text("Derived_SoulBulwark.Name", "Dawn of the Soul"), Common.Text("Derived_SoulBulwark.Name", "法相初明", true)))
                .SetDescription(Common.L(
                    "Derived_SoulBulwark.Buff.Desc",
                    Common.Text("Derived_SoulBulwark.Buff.Desc", "<i>Derived · Caster Level to Temporary Hit Points</i>\nThe soul keeps its own vigil, calling protective strength into place whenever a fight begins.", false),
                    Common.Text("Derived_SoulBulwark.Buff.Desc", "<i>衍生属性 · 施法者等级转化临时生命</i>\n神识常驻清明，每当战端初起便即刻激发生命护壁。", true),
                    tagEncyclopediaEntries: true))
                .SetIconIfPresent("SoulBulwark")
                .AddCombatStateTrigger(
                    combatStartActions: ActionsBuilder.New().ApplyBuff(
                        buff: SoulBulwarkTempBuffGuid,
                        durationValue: ContextDuration.Fixed(10, DurationRate.Minutes),
                        asChild: true,
                        isNotDispelable: true,
                        toCaster: true),
                    combatEndActions: ActionsBuilder.New().RemoveBuff(
                        buff: SoulBulwarkTempBuffGuid,
                        onlyFromCaster: true,
                        toCaster: true))
                .Configure();
        }

        private static FeatureConfigurator NewFeature(
            string internalName,
            string guid,
            string nameKey,
            string nameEn,
            string nameZh,
            string descKey,
            (string en, string zh) desc)
            => FeatSelection.DerivedStat.NewFeat(internalName, guid)
                .SetDisplayName(Common.L(nameKey, nameEn, nameZh))
                .SetDescription(Common.L(descKey, desc.en, desc.zh, tagEncyclopediaEntries: true))
                .SetIconIfPresent(internalName);

        private static (string en, string zh) BuildDescription(
            string subtitleEn,
            string subtitleZh,
            string loreEn,
            string loreZh,
            string effectEn,
            string effectZh)
        {
            var en = Common.Format("Text.DerivedStatFeats.BuildDescription.Description", "<i>Derived · {0}</i>\n{1}\n\n<b>Effect:</b> {2}\n\n<b>Restrictions:</b> None. This feat is independent and stacks normally with other feat families.", false, subtitleEn, loreEn, effectEn);
            var zh = Common.Format("Text.DerivedStatFeats.BuildDescription.Description", "<i>衍生属性 · {0}</i>\n{1}\n\n<b>效果：</b>{2}\n\n<b>限制：</b>无。此专长独立生效，可与其他专长正常叠加。", true, subtitleZh, loreZh, effectZh);
            return (en, zh);
        }
    }
}
