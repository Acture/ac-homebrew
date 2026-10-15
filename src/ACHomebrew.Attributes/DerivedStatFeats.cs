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
                displayName: LocalizedText.Text("Derived_ArcaneAegis.Name", "Arcane Aegis"),
                descKey: "Derived_ArcaneAegis.Desc",
                desc: BuildDescription(
                    LocalizedText.Text("Text.DerivedStatFeats.CreateArcaneAegis.Text", "Caster Level to AC"),
                    LocalizedText.Text("Derived_ArcaneAegis.Lore", "Years of spellwork teach you to turn practiced magical control toward personal defense."),
                    LocalizedText.Text("Text.DerivedStatFeats.CreateArcaneAegis.Text.2", "Adds half your caster level as an untyped bonus to AC.")))
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
                displayName: LocalizedText.Text("Derived_MartialInsight.Name", "War-Hardened Reflexes"),
                descKey: "Derived_MartialInsight.Desc",
                desc: BuildDescription(
                    LocalizedText.Text("Text.DerivedStatFeats.CreateMartialInsight.Text", "Base Attack Bonus to Saves"),
                    LocalizedText.Text("Derived_MartialInsight.Lore", "Battle has trained your responses until endurance, movement, and resolve share the same rhythm."),
                    LocalizedText.Text("Text.DerivedStatFeats.CreateMartialInsight.Text.2", "Adds half your base attack bonus as an untyped bonus to Fortitude, Reflex, and Will saving throws.")))
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
                displayName: LocalizedText.Text("Derived_SkilledDefender.Name", "Scholar's Positioning"),
                descKey: "Derived_SkilledDefender.Desc",
                desc: BuildDescription(
                    LocalizedText.Text("Text.DerivedStatFeats.CreateSkilledDefender.Text", "Total Skill Ranks to AC"),
                    LocalizedText.Text("Derived_SkilledDefender.Lore", "Lessons gathered from many crafts become practical answers to an enemy's approach."),
                    LocalizedText.Text("Text.DerivedStatFeats.CreateSkilledDefender.Text.2", "Adds one third of your total skill ranks as an untyped bonus to AC.")))
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
                displayName: LocalizedText.Text("Derived_MysticVitality.Name", "Ley-Infused Vitality"),
                descKey: "Derived_MysticVitality.Desc",
                desc: BuildDescription(
                    LocalizedText.Text("Text.DerivedStatFeats.CreateMysticVitality.Text", "Caster Level to Hit Points"),
                    LocalizedText.Text("Derived_MysticVitality.Lore", "You make the discipline of channeling magic part of the discipline of sustaining yourself."),
                    LocalizedText.Text("Text.DerivedStatFeats.CreateMysticVitality.Text.2", "Adds your caster level as an untyped bonus to Hit Points.")))
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
                displayName: LocalizedText.Text("Derived_SoulBulwark.Name", "Dawn of the Soul"),
                descKey: "Derived_SoulBulwark.Desc",
                desc: BuildDescription(
                    LocalizedText.Text("Text.DerivedStatFeats.CreateSoulBulwark.Text", "Caster Level to Temporary Hit Points"),
                    LocalizedText.Text("Derived_SoulBulwark.Lore", "When battle begins, your practiced spellwork gathers around you like a brief mantle of dawn."),
                    LocalizedText.Text("Text.DerivedStatFeats.CreateSoulBulwark.Text.2", "At the start of combat, gain temporary hit points equal to your caster level for up to 10 minutes or until combat ends, whichever comes first.")))
                .AddFacts(facts: new List<Blueprint<BlueprintUnitFactReference>> { Guids.Derived.SoulBulwarkBuff })
                .Configure();
        }

        private static void CreateSwordSaint()
        {
            NewFeature(
                internalName: "SwordSaint",
                guid: Guids.Derived.SwordSaint,
                nameKey: "Derived_SwordSaint.Name",
                displayName: LocalizedText.Text("Derived_SwordSaint.Name", "Blade of the Spell-Saint"),
                descKey: "Derived_SwordSaint.Desc",
                desc: BuildDescription(
                    LocalizedText.Text("Text.DerivedStatFeats.CreateSwordSaint.Text", "Base Attack Bonus to Spell DC"),
                    LocalizedText.Text("Derived_SwordSaint.Lore", "The precision learned with a weapon gives you another way to shape demanding magic."),
                    LocalizedText.Text("Text.DerivedStatFeats.CreateSwordSaint.Text.2", "Adds half your base attack bonus as an untyped bonus to the save DC of your spells. Class and racial abilities are not affected.")))
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
                .SetDisplayName(Common.L("Derived_SoulBulwark.Temp.Name", LocalizedText.Text("Derived_SoulBulwark.Name", "Dawn of the Soul")))
                .SetDescription(Common.L(
                    "Derived_SoulBulwark.Temp.Desc",
                    LocalizedText.Text("Derived_SoulBulwark.Temp.Desc", "<i>Derived · Caster Level to Temporary Hit Points</i>\nA reserve of radiant psychic force gathers around your soul for up to 10 minutes or until combat ends, whichever comes first, absorbing damage before your mortal flesh yields."),
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
                .SetDisplayName(Common.L("Derived_SoulBulwark.Buff.Name", LocalizedText.Text("Derived_SoulBulwark.Name", "Dawn of the Soul")))
                .SetDescription(Common.L(
                    "Derived_SoulBulwark.Buff.Desc",
                    LocalizedText.Text("Derived_SoulBulwark.Buff.Desc", "<i>Derived · Caster Level to Temporary Hit Points</i>\nThe soul keeps its own vigil, calling protective strength into place whenever a fight begins."),
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
            LocalizedText displayName,
            string descKey,
            LocalizedText desc)
            => FeatSelection.DerivedStat.NewFeat(internalName, guid)
                .SetDisplayName(Common.L(nameKey, displayName))
                .SetDescription(Common.L(descKey, desc, tagEncyclopediaEntries: true))
                .SetIconIfPresent(internalName);

        private static LocalizedText BuildDescription(
            LocalizedText subtitle,
            LocalizedText lore,
            LocalizedText effect)
        {
            return LocalizedText.Format("Text.DerivedStatFeats.BuildDescription.Description", "<i>Derived · {0}</i>\n{1}\n\n<b>Effect:</b> {2}\n\n<b>Restrictions:</b> None. This feat is independent and stacks normally with other feat families.", subtitle, lore, effect);
        }
    }
}
