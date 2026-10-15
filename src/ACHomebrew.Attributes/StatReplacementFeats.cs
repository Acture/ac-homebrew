using System;
using System.Collections.Generic;
using BlueprintCore.Blueprints.Components.Replacements;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items.Slots;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;

namespace ACHomebrew.Feats
{
    internal static class StatReplacementFeats
    {
        private static readonly WeaponSubCategory[] WeaponInsightCategories =
        {
            WeaponSubCategory.Melee,
            WeaponSubCategory.Ranged,
            WeaponSubCategory.Thrown,
            WeaponSubCategory.Natural,
        };

        private static bool Initialized;

        public static void ConfigureAll()
        {
            if (Initialized) return;
            Initialized = true;

            var weaponInsights = new List<BlueprintFeature>
            {
                CreateWeaponInsight(
                    StatType.Strength,
                    "CrushingForm",
                    Guids.Replacement.WeaponInsight.Str,
                    "WeaponInsight_Str.Name",
                    Common.Text("WeaponInsight_Str.Name", "Titan's Momentum"),
                    Common.Text("WeaponInsight_Str.Name", "巨灵摧破", true),
                    "WeaponInsight_Str.Desc",
                    BuildWeaponInsightDescription(
                        Common.Text("Text.StatReplacementFeats.ConfigureAll.Text", "Strength", false),
                        Common.Text("Text.StatReplacementFeats.ConfigureAll.Text", "力量", true),
                        Common.Text("WeaponInsight_Str.Lore", "Rather than chase delicate openings, you commit your weight to a weapon's path and trust its momentum."),
                        Common.Text("WeaponInsight_Str.Lore", "你不追逐细小的空隙，而将身量压入兵刃的轨迹，借其惯势出手。", true))),
                CreateWeaponInsight(
                    StatType.Dexterity,
                    "DuelistsEye",
                    Guids.Replacement.WeaponInsight.Dex,
                    "WeaponInsight_Dex.Name",
                    Common.Text("WeaponInsight_Dex.Name", "Aldori Finesse"),
                    Common.Text("WeaponInsight_Dex.Name", "阿尔多里绝诣", true),
                    "WeaponInsight_Dex.Desc",
                    BuildWeaponInsightDescription(
                        Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.2", "Dexterity", false),
                        Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.2", "敏捷", true),
                        Common.Text("WeaponInsight_Dex.Lore", "Inspired by Aldori dueling traditions, you guide the blade with balance and exacting footwork."),
                        Common.Text("WeaponInsight_Dex.Lore", "你借鉴阿尔多里的决斗传统，以重心与精确步法驾驭剑锋。", true))),
                CreateWeaponInsight(
                    StatType.Constitution,
                    "IronStance",
                    Guids.Replacement.WeaponInsight.Con,
                    "WeaponInsight_Con.Name",
                    Common.Text("WeaponInsight_Con.Name", "Stout Grounding"),
                    Common.Text("WeaponInsight_Con.Name", "磐固千钧", true),
                    "WeaponInsight_Con.Desc",
                    BuildWeaponInsightDescription(
                        Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.3", "Constitution", false),
                        Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.3", "体质", true),
                        Common.Text("WeaponInsight_Con.Lore", "A steady breath and a planted stance keep your weapon true even when fatigue begins to bite."),
                        Common.Text("WeaponInsight_Con.Lore", "即使疲惫袭来，沉稳的呼吸与扎实的站姿仍让兵刃沿着预定轨迹前行。", true))),
                CreateWeaponInsight(
                    StatType.Intelligence,
                    "TacticalStrike",
                    Guids.Replacement.WeaponInsight.Int,
                    "WeaponInsight_Int.Name",
                    Common.Text("WeaponInsight_Int.Name", "Geometer's Edge"),
                    Common.Text("WeaponInsight_Int.Name", "规矩之锋", true),
                    "WeaponInsight_Int.Desc",
                    BuildWeaponInsightDescription(
                        Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.4", "Intelligence", false),
                        Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.4", "智力", true),
                        Common.Text("WeaponInsight_Int.Lore", "You see the duel as intersecting lines, choosing each stroke by angle rather than impulse."),
                        Common.Text("WeaponInsight_Int.Lore", "你将交锋视作交错的线条，依角度而非冲动选择每一次出手。", true))),
                CreateWeaponInsight(
                    StatType.Wisdom,
                    "PredictiveCut",
                    Guids.Replacement.WeaponInsight.Wis,
                    "WeaponInsight_Wis.Name",
                    Common.Text("WeaponInsight_Wis.Name", "Karmic Interception"),
                    Common.Text("WeaponInsight_Wis.Name", "因果断隙", true),
                    "WeaponInsight_Wis.Desc",
                    BuildWeaponInsightDescription(
                        Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.5", "Wisdom", false),
                        Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.5", "感知", true),
                        Common.Text("WeaponInsight_Wis.Lore", "You watch the intention behind a motion and meet your opponent where their next step will lead."),
                        Common.Text("WeaponInsight_Wis.Lore", "你察看动作背后的意图，在对手下一步将至之处迎击。", true))),
                CreateWeaponInsight(
                    StatType.Charisma,
                    "TheatricalCombat",
                    Guids.Replacement.WeaponInsight.Cha,
                    "WeaponInsight_Cha.Name",
                    Common.Text("WeaponInsight_Cha.Name", "Swashbuckler's Flourish"),
                    Common.Text("WeaponInsight_Cha.Name", "游侠华彩", true),
                    "WeaponInsight_Cha.Desc",
                    BuildWeaponInsightDescription(
                        Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.6", "Charisma", false),
                        Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.6", "魅力", true),
                        Common.Text("WeaponInsight_Cha.Lore", "A flourish invites the eye to follow one story while your weapon pursues another."),
                        Common.Text("WeaponInsight_Cha.Lore", "华丽的虚招引导敌人的目光，兵刃则沿另一条轨迹逼近。", true))),
            };


            CreateExtendedFeat(
                StatType.Wisdom,
                StatType.AC,
                "InnerSentinel",
                Guids.Replacement.Extended.InnerSentinel,
                "Extended_InnerSentinel.Name",
                Common.Text("Extended_InnerSentinel.Name", "Ascetic's Ward"),
                Common.Text("Extended_InnerSentinel.Name", "云水自真", true),
                "Extended_InnerSentinel.Desc",
                BuildExtendedDescription(
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.5", "Wisdom", false),
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.5", "感知", true),
                    Common.Text("Extended_InnerSentinel.Lore", "Lightly burdened, you guard yourself through a traveler's quiet awareness and measured movement."),
                    Common.Text("Extended_InnerSentinel.Lore", "轻装行走时，你凭旅人的沉静觉察与适度挪移守护自身。", true),
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.7", "Adds your Wisdom modifier as an untyped bonus to AC while wearing no armor or light armor.", false),
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.7", "在未着甲或穿着轻甲时，将你的感知调整值作为无类型加值附加至防御等级（AC）。", true),
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.8", "Applies only while wearing no armor or light armor. This feat can be combined with other Extended Replacement feats.", false),
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.8", "仅在未着甲或穿着轻甲时生效。此专长可与其他“属性延展”专长正常叠加。", true)),
                CreateLightOrNoArmorRestriction());

            CreateExtendedFeat(
                StatType.Intelligence,
                StatType.AdditionalCMB,
                "CalculatedGrip",
                Guids.Replacement.Extended.CalculatedGrip,
                "Extended_CalculatedGrip.Name",
                Common.Text("Extended_CalculatedGrip.Name", "Anatomical Leverage"),
                Common.Text("Extended_CalculatedGrip.Name", "筋络推演", true),
                "Extended_CalculatedGrip.Desc",
                BuildExtendedDescription(
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.4", "Intelligence", false),
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.4", "智力", true),
                    Common.Text("Extended_CalculatedGrip.Lore", "Anatomy turns a grapple into a study of joints, leverage, and the direction of resistance."),
                    Common.Text("Extended_CalculatedGrip.Lore", "关节、杠杆与抵抗的方向，使每一次擒抱都成为对身体结构的实践。", true),
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.9", "Adds your Intelligence modifier as an untyped bonus to CMB.", false),
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.9", "将你的智力调整值作为无类型加值附加至战技加值（CMB）。", true),
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.10", "This feat can be combined with other Extended Replacement feats.", false),
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.10", "此专长可与其他“属性延展”专长正常叠加。", true)));

            CreateExtendedFeat(
                StatType.Charisma,
                StatType.AdditionalCMD,
                "UnyieldingWill",
                Guids.Replacement.Extended.UnyieldingWill,
                "Extended_UnyieldingWill.Name",
                Common.Text("Extended_UnyieldingWill.Name", "Monarch's Stature"),
                Common.Text("Extended_UnyieldingWill.Name", "帝胄岳立", true),
                "Extended_UnyieldingWill.Desc",
                BuildExtendedDescription(
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.6", "Charisma", false),
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.6", "魅力", true),
                    Common.Text("Extended_UnyieldingWill.Lore", "You hold your ground with the bearing of someone who expects to be obeyed."),
                    Common.Text("Extended_UnyieldingWill.Lore", "你以不容轻忽的威仪站稳阵地，令自己的坚持清晰可见。", true),
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.11", "Adds your Charisma modifier as an untyped bonus to CMD.", false),
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.11", "将你的魅力调整值作为无类型加值附加至战技防御（CMD）。", true),
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.10", "This feat can be combined with other Extended Replacement feats.", false),
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.10", "此专长可与其他“属性延展”专长正常叠加。", true)));

            CreateExtendedFeat(
                StatType.Strength,
                StatType.AdditionalCMD,
                "BrutalDefender",
                Guids.ExtendedReplacement2.BrutalDefender,
                "Extended_BrutalDefender.Name",
                Common.Text("Extended_BrutalDefender.Name", "Titan's Footing"),
                Common.Text("Extended_BrutalDefender.Name", "巨灵固步", true),
                "Extended_BrutalDefender.Desc",
                BuildExtendedDescription(
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text", "Strength", false),
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text", "力量", true),
                    Common.Text("Extended_BrutalDefender.Lore", "Strong legs and a deliberate stance let you answer force without surrendering your footing."),
                    Common.Text("Extended_BrutalDefender.Lore", "强健的双腿与审慎的站姿，让你在抵抗外力时守住立足之处。", true),
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.12", "Adds your Strength modifier as an untyped bonus to CMD.", false),
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.12", "将你的力量调整值作为无类型加值附加至战技防御（CMD）。", true),
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.10", "This feat can be combined with other Extended Replacement feats.", false),
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.10", "此专长可与其他“属性延展”专长正常叠加。", true)));

            CreateExtendedFeat(
                StatType.Dexterity,
                StatType.AC,
                "LightfootDefense",
                Guids.ExtendedReplacement2.LightfootDefense,
                "Extended_LightfootDefense.Name",
                Common.Text("Extended_LightfootDefense.Name", "Zephyr's Grace"),
                Common.Text("Extended_LightfootDefense.Name", "穿风灵步", true),
                "Extended_LightfootDefense.Desc",
                BuildExtendedDescription(
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.2", "Dexterity", false),
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.2", "敏捷", true),
                    Common.Text("Extended_LightfootDefense.Lore", "With little armor to hamper you, every small shift of weight becomes part of your guard."),
                    Common.Text("Extended_LightfootDefense.Lore", "少了重甲的牵制，每一次细微的重心转移都成为防守的一部分。", true),
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.13", "Adds your Dexterity modifier as an untyped bonus to AC while wearing no armor or light armor.", false),
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.13", "在未着甲或穿着轻甲时，将你的敏捷调整值作为无类型加值附加至防御等级（AC）。", true),
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.8", "Applies only while wearing no armor or light armor. This feat can be combined with other Extended Replacement feats.", false),
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.8", "仅在未着甲或穿着轻甲时生效。此专长可与其他“属性延展”专长正常叠加。", true)),
                CreateLightOrNoArmorRestriction());

            CreateExtendedFeat(
                StatType.Constitution,
                StatType.HitPoints,
                "IronEndurance",
                Guids.ExtendedReplacement2.IronEndurance,
                "Extended_IronEndurance.Name",
                Common.Text("Extended_IronEndurance.Name", "Adamantine Mettle"),
                Common.Text("Extended_IronEndurance.Name", "生机洪炉", true),
                "Extended_IronEndurance.Desc",
                BuildExtendedDescription(
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.3", "Constitution", false),
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.3", "体质", true),
                    Common.Text("Extended_IronEndurance.Lore", "You have learned to carry hardship in your flesh as a forge carries the day's heat."),
                    Common.Text("Extended_IronEndurance.Lore", "你学会如熔炉蓄热般承受艰辛，让磨砺沉淀于血肉之中。", true),
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.14", "Adds your Constitution modifier as an untyped bonus to Hit Points.", false),
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.14", "将你的体质调整值作为无类型加值附加至生命值上限（HP）。", true),
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.10", "This feat can be combined with other Extended Replacement feats.", false),
                    Common.Text("Text.StatReplacementFeats.ConfigureAll.Text.10", "此专长可与其他“属性延展”专长正常叠加。", true)));
        }

        private static BlueprintFeature CreateWeaponInsight(
            StatType baseStat,
            string internalName,
            string guid,
            string nameKey,
            string nameEn,
            string nameZh,
            string descKey,
            (string en, string zh) desc)
        {
            var cfg = FeatSelection.WeaponInsight.NewFeat(internalName, guid)
                .SetDisplayName(Common.L(nameKey, nameEn, nameZh))
                .SetDescription(Common.L(descKey, desc.en, desc.zh, tagEncyclopediaEntries: true))
                .SetIconIfPresent(internalName);

            AddWeaponInsightReplacements(cfg, baseStat);
            return cfg.Configure();
        }

        private static BlueprintFeature CreateExtendedFeat(
            StatType baseStat,
            StatType targetStat,
            string internalName,
            string guid,
            string nameKey,
            string nameEn,
            string nameZh,
            string descKey,
            (string en, string zh) desc,
            RestrictionCalculator restriction = null)
        {
            var cfg = FeatSelection.ExtendedReplacement.NewFeat(internalName, guid)
                .SetDisplayName(Common.L(nameKey, nameEn, nameZh))
                .SetDescription(Common.L(descKey, desc.en, desc.zh, tagEncyclopediaEntries: true))
                .SetIconIfPresent(internalName);

            Common.AddRank(cfg, baseStat, AbilityRankType.Default, ContextRankProgression.AsIs);
            cfg.AddContextStatBonus(
                stat: targetStat,
                value: Common.Rank(),
                descriptor: ModifierDescriptor.None,
                restrictions: restriction);
            return cfg.Configure();
        }

        private static void AddWeaponInsightReplacements(FeatureConfigurator cfg, StatType baseStat)
        {
            foreach (var subCategory in WeaponInsightCategories)
            {
                cfg.AddAttackStatReplacementFixed(new AttackStatReplacementFixed(baseStat, subCategory));
            }
        }

        private static RestrictionCalculator CreateLightOrNoArmorRestriction()
            => new()
            {
                Property = new PropertyCalculator
                {
                    Operation = PropertyCalculator.OperationType.Sum,
                    TargetType = PropertyTargetType.CurrentEntity,
                    Getters = new PropertyGetter[]
                    {
                        new LightOrNoArmorPropertyGetter(),
                    },
                },
            };

        private static (string en, string zh) BuildWeaponInsightDescription(
            string statEn,
            string statZh,
            string loreEn,
            string loreZh)
        {
            var en = Common.Format("Text.StatReplacementFeats.BuildWeaponInsightDescription.Description", "<i>Weapon Insight · {0}</i>\n{1}\n\n<b>Effect:</b> Your weapon attack rolls use your {0} modifier instead of Strength or Dexterity whenever {0} would be better.\n\n<b>Restrictions:</b> While its exclusion group is on (configurable in mod settings), you can have only one Weapon Insight feat.", false, statEn, loreEn);
            var zh = Common.Format("Text.StatReplacementFeats.BuildWeaponInsightDescription.Description", "<i>武器洞察 · {0}</i>\n{1}\n\n<b>效果：</b>当你的{0}调整值更高时，所有武器攻击检定均使用{0}调整值替代力量或敏捷调整值。\n\n<b>限制：</b>启用该互斥组时（可在模组设置中调整），只能拥有一个“武器洞察”专长。", true, statZh, loreZh);
            return (en, zh);
        }

        private static (string en, string zh) BuildExtendedDescription(
            string statEn,
            string statZh,
            string loreEn,
            string loreZh,
            string effectEn,
            string effectZh,
            string restrictionEn,
            string restrictionZh)
        {
            var en = Common.Format("Text.StatReplacementFeats.BuildExtendedDescription.Description", "<i>Extended Replacement · {0}</i>\n{1}\n\n<b>Effect:</b> {2} Attribute modifiers used for these bonuses and matching penalties have a minimum of 0.\n\n<b>Restrictions:</b> {3}", false, statEn, loreEn, effectEn, restrictionEn);
            var zh = Common.Format("Text.StatReplacementFeats.BuildExtendedDescription.Description", "<i>属性延展 · {0}</i>\n{1}\n\n<b>效果：</b>{2}这些加值及对应减值均以属性调整值最低0计算。\n\n<b>限制：</b>{3}", true, statZh, loreZh, effectZh, restrictionZh);
            return (en, zh);
        }
    }

    [Serializable]
    internal sealed class LightOrNoArmorPropertyGetter : UnitPropertyGetter
    {
        protected override int GetBaseValue()
        {
            var unit = CurrentEntity;
            if (unit?.Body == null)
            {
                return 1;
            }

            foreach (var slot in unit.Body.CurrentEquipmentSlots)
            {
                if (slot is ArmorSlot armorSlot)
                {
                    if (!armorSlot.HasArmor)
                    {
                        return 1;
                    }

                    var proficiencyGroup = armorSlot.Armor?.Blueprint?.ProficiencyGroup ?? ArmorProficiencyGroup.None;
                    return proficiencyGroup == ArmorProficiencyGroup.None || proficiencyGroup == ArmorProficiencyGroup.Light ? 1 : 0;
                }
            }

            return 1;
        }

        protected override string GetInnerCaption() => "No or light armor";
    }
}
