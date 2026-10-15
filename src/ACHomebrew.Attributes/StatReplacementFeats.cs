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
                    LocalizedText.Text("WeaponInsight_Str.Name", "Titan's Momentum"),
                    "WeaponInsight_Str.Desc",
                    BuildWeaponInsightDescription(
                        LocalizedText.Text("Text.StatReplacementFeats.ConfigureAll.Text", "Strength"),
                        LocalizedText.Text("WeaponInsight_Str.Lore", "Rather than chase delicate openings, you commit your weight to a weapon's path and trust its momentum."))),
                CreateWeaponInsight(
                    StatType.Dexterity,
                    "DuelistsEye",
                    Guids.Replacement.WeaponInsight.Dex,
                    "WeaponInsight_Dex.Name",
                    LocalizedText.Text("WeaponInsight_Dex.Name", "Aldori Finesse"),
                    "WeaponInsight_Dex.Desc",
                    BuildWeaponInsightDescription(
                        LocalizedText.Text("Text.StatReplacementFeats.ConfigureAll.Text.2", "Dexterity"),
                        LocalizedText.Text("WeaponInsight_Dex.Lore", "Inspired by Aldori dueling traditions, you guide the blade with balance and exacting footwork."))),
                CreateWeaponInsight(
                    StatType.Constitution,
                    "IronStance",
                    Guids.Replacement.WeaponInsight.Con,
                    "WeaponInsight_Con.Name",
                    LocalizedText.Text("WeaponInsight_Con.Name", "Stout Grounding"),
                    "WeaponInsight_Con.Desc",
                    BuildWeaponInsightDescription(
                        LocalizedText.Text("Text.StatReplacementFeats.ConfigureAll.Text.3", "Constitution"),
                        LocalizedText.Text("WeaponInsight_Con.Lore", "A steady breath and a planted stance keep your weapon true even when fatigue begins to bite."))),
                CreateWeaponInsight(
                    StatType.Intelligence,
                    "TacticalStrike",
                    Guids.Replacement.WeaponInsight.Int,
                    "WeaponInsight_Int.Name",
                    LocalizedText.Text("WeaponInsight_Int.Name", "Geometer's Edge"),
                    "WeaponInsight_Int.Desc",
                    BuildWeaponInsightDescription(
                        LocalizedText.Text("Text.StatReplacementFeats.ConfigureAll.Text.4", "Intelligence"),
                        LocalizedText.Text("WeaponInsight_Int.Lore", "You see the duel as intersecting lines, choosing each stroke by angle rather than impulse."))),
                CreateWeaponInsight(
                    StatType.Wisdom,
                    "PredictiveCut",
                    Guids.Replacement.WeaponInsight.Wis,
                    "WeaponInsight_Wis.Name",
                    LocalizedText.Text("WeaponInsight_Wis.Name", "Karmic Interception"),
                    "WeaponInsight_Wis.Desc",
                    BuildWeaponInsightDescription(
                        LocalizedText.Text("Text.StatReplacementFeats.ConfigureAll.Text.5", "Wisdom"),
                        LocalizedText.Text("WeaponInsight_Wis.Lore", "You watch the intention behind a motion and meet your opponent where their next step will lead."))),
                CreateWeaponInsight(
                    StatType.Charisma,
                    "TheatricalCombat",
                    Guids.Replacement.WeaponInsight.Cha,
                    "WeaponInsight_Cha.Name",
                    LocalizedText.Text("WeaponInsight_Cha.Name", "Swashbuckler's Flourish"),
                    "WeaponInsight_Cha.Desc",
                    BuildWeaponInsightDescription(
                        LocalizedText.Text("Text.StatReplacementFeats.ConfigureAll.Text.6", "Charisma"),
                        LocalizedText.Text("WeaponInsight_Cha.Lore", "A flourish invites the eye to follow one story while your weapon pursues another."))),
            };


            CreateExtendedFeat(
                StatType.Wisdom,
                StatType.AC,
                "InnerSentinel",
                Guids.Replacement.Extended.InnerSentinel,
                "Extended_InnerSentinel.Name",
                LocalizedText.Text("Extended_InnerSentinel.Name", "Ascetic's Ward"),
                "Extended_InnerSentinel.Desc",
                BuildExtendedDescription(
                    LocalizedText.Text("Text.StatReplacementFeats.ConfigureAll.Text.5", "Wisdom"),
                    LocalizedText.Text("Extended_InnerSentinel.Lore", "Lightly burdened, you guard yourself through a traveler's quiet awareness and measured movement."),
                    LocalizedText.Text("Text.StatReplacementFeats.ConfigureAll.Text.7", "Adds your Wisdom modifier as an untyped bonus to AC while wearing no armor or light armor."),
                    LocalizedText.Text("Text.StatReplacementFeats.ConfigureAll.Text.8", "Applies only while wearing no armor or light armor. This feat can be combined with other Extended Replacement feats.")),
                CreateLightOrNoArmorRestriction());

            CreateExtendedFeat(
                StatType.Intelligence,
                StatType.AdditionalCMB,
                "CalculatedGrip",
                Guids.Replacement.Extended.CalculatedGrip,
                "Extended_CalculatedGrip.Name",
                LocalizedText.Text("Extended_CalculatedGrip.Name", "Anatomical Leverage"),
                "Extended_CalculatedGrip.Desc",
                BuildExtendedDescription(
                    LocalizedText.Text("Text.StatReplacementFeats.ConfigureAll.Text.4", "Intelligence"),
                    LocalizedText.Text("Extended_CalculatedGrip.Lore", "Anatomy turns a grapple into a study of joints, leverage, and the direction of resistance."),
                    LocalizedText.Text("Text.StatReplacementFeats.ConfigureAll.Text.9", "Adds your Intelligence modifier as an untyped bonus to CMB."),
                    LocalizedText.Text("Text.StatReplacementFeats.ConfigureAll.Text.10", "This feat can be combined with other Extended Replacement feats.")));

            CreateExtendedFeat(
                StatType.Charisma,
                StatType.AdditionalCMD,
                "UnyieldingWill",
                Guids.Replacement.Extended.UnyieldingWill,
                "Extended_UnyieldingWill.Name",
                LocalizedText.Text("Extended_UnyieldingWill.Name", "Monarch's Stature"),
                "Extended_UnyieldingWill.Desc",
                BuildExtendedDescription(
                    LocalizedText.Text("Text.StatReplacementFeats.ConfigureAll.Text.6", "Charisma"),
                    LocalizedText.Text("Extended_UnyieldingWill.Lore", "You hold your ground with the bearing of someone who expects to be obeyed."),
                    LocalizedText.Text("Text.StatReplacementFeats.ConfigureAll.Text.11", "Adds your Charisma modifier as an untyped bonus to CMD."),
                    LocalizedText.Text("Text.StatReplacementFeats.ConfigureAll.Text.10", "This feat can be combined with other Extended Replacement feats.")));

            CreateExtendedFeat(
                StatType.Strength,
                StatType.AdditionalCMD,
                "BrutalDefender",
                Guids.ExtendedReplacement2.BrutalDefender,
                "Extended_BrutalDefender.Name",
                LocalizedText.Text("Extended_BrutalDefender.Name", "Titan's Footing"),
                "Extended_BrutalDefender.Desc",
                BuildExtendedDescription(
                    LocalizedText.Text("Text.StatReplacementFeats.ConfigureAll.Text", "Strength"),
                    LocalizedText.Text("Extended_BrutalDefender.Lore", "Strong legs and a deliberate stance let you answer force without surrendering your footing."),
                    LocalizedText.Text("Text.StatReplacementFeats.ConfigureAll.Text.12", "Adds your Strength modifier as an untyped bonus to CMD."),
                    LocalizedText.Text("Text.StatReplacementFeats.ConfigureAll.Text.10", "This feat can be combined with other Extended Replacement feats.")));

            CreateExtendedFeat(
                StatType.Dexterity,
                StatType.AC,
                "LightfootDefense",
                Guids.ExtendedReplacement2.LightfootDefense,
                "Extended_LightfootDefense.Name",
                LocalizedText.Text("Extended_LightfootDefense.Name", "Zephyr's Grace"),
                "Extended_LightfootDefense.Desc",
                BuildExtendedDescription(
                    LocalizedText.Text("Text.StatReplacementFeats.ConfigureAll.Text.2", "Dexterity"),
                    LocalizedText.Text("Extended_LightfootDefense.Lore", "With little armor to hamper you, every small shift of weight becomes part of your guard."),
                    LocalizedText.Text("Text.StatReplacementFeats.ConfigureAll.Text.13", "Adds your Dexterity modifier as an untyped bonus to AC while wearing no armor or light armor."),
                    LocalizedText.Text("Text.StatReplacementFeats.ConfigureAll.Text.8", "Applies only while wearing no armor or light armor. This feat can be combined with other Extended Replacement feats.")),
                CreateLightOrNoArmorRestriction());

            CreateExtendedFeat(
                StatType.Constitution,
                StatType.HitPoints,
                "IronEndurance",
                Guids.ExtendedReplacement2.IronEndurance,
                "Extended_IronEndurance.Name",
                LocalizedText.Text("Extended_IronEndurance.Name", "Adamantine Mettle"),
                "Extended_IronEndurance.Desc",
                BuildExtendedDescription(
                    LocalizedText.Text("Text.StatReplacementFeats.ConfigureAll.Text.3", "Constitution"),
                    LocalizedText.Text("Extended_IronEndurance.Lore", "You have learned to carry hardship in your flesh as a forge carries the day's heat."),
                    LocalizedText.Text("Text.StatReplacementFeats.ConfigureAll.Text.14", "Adds your Constitution modifier as an untyped bonus to Hit Points."),
                    LocalizedText.Text("Text.StatReplacementFeats.ConfigureAll.Text.10", "This feat can be combined with other Extended Replacement feats.")));
        }

        private static BlueprintFeature CreateWeaponInsight(
            StatType baseStat,
            string internalName,
            string guid,
            string nameKey,
            LocalizedText displayName,
            string descKey,
            LocalizedText desc)
        {
            var cfg = FeatSelection.WeaponInsight.NewFeat(internalName, guid)
                .SetDisplayName(Common.L(nameKey, displayName))
                .SetDescription(Common.L(descKey, desc, tagEncyclopediaEntries: true))
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
            LocalizedText displayName,
            string descKey,
            LocalizedText desc,
            RestrictionCalculator restriction = null)
        {
            var cfg = FeatSelection.ExtendedReplacement.NewFeat(internalName, guid)
                .SetDisplayName(Common.L(nameKey, displayName))
                .SetDescription(Common.L(descKey, desc, tagEncyclopediaEntries: true))
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

        private static LocalizedText BuildWeaponInsightDescription(
            LocalizedText stat,
            LocalizedText lore)
        {
            return LocalizedText.Format("Text.StatReplacementFeats.BuildWeaponInsightDescription.Description", "<i>Weapon Insight · {0}</i>\n{1}\n\n<b>Effect:</b> Your weapon attack rolls use your {0} modifier instead of Strength or Dexterity whenever {0} would be better.\n\n<b>Restrictions:</b> While its exclusion group is on (configurable in mod settings), you can have only one Weapon Insight feat.", stat, lore);
        }

        private static LocalizedText BuildExtendedDescription(
            LocalizedText stat,
            LocalizedText lore,
            LocalizedText effect,
            LocalizedText restriction)
        {
            return LocalizedText.Format("Text.StatReplacementFeats.BuildExtendedDescription.Description", "<i>Extended Replacement · {0}</i>\n{1}\n\n<b>Effect:</b> {2} Attribute modifiers used for these bonuses and matching penalties have a minimum of 0.\n\n<b>Restrictions:</b> {3}", stat, lore, effect, restriction);
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
