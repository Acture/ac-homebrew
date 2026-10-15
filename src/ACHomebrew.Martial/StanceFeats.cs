using System;
using System.Collections.Generic;
using BlueprintCore.Blueprints.Configurators.UnitLogic.ActivatableAbilities;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Abilities;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Buffs;
using BlueprintCore.Conditions.Builder;
using BlueprintCore.Conditions.Builder.ContextEx;
using BlueprintCore.Utils;
using BlueprintCore.Utils.Types;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;

namespace ACHomebrew.Feats
{
    internal static class StanceFeats
    {
        private const ModifierDescriptor Desc = ModifierDescriptor.None;
        private static bool Initialized;

        public static void ConfigureAll()
        {
            if (Initialized) return;
            Initialized = true;

            var feats = new List<BlueprintFeature>
            {
                CreateStance(
                    baseStat: StatType.Strength,
                    internalName: "BrutalStance",
                    featureGuid: Guids.Stance.Feature.Str,
                    buffGuid: Guids.Stance.Buff.Str,
                    activatableGuid: Guids.Stance.Activatable.Str,
                    keyPrefix: "Stance_Str",
                    displayName: LocalizedText.Text("Stance_Str.Name", "Berserker's Overrun"),
                    description: BuildDescription(
                        LocalizedText.Text("Text.StanceFeats.ConfigureAll.Text", "Strength"),
                        LocalizedText.Text("Stance_Str.LoreTitle", "Gorum's Reckless Abandon."),
                        LocalizedText.Text("Stance_Str.Lore", "You commit to the next blow with a battle zeal reminiscent of Gorum, leaving less attention for your guard."),
                        LocalizedText.Text("Text.StanceFeats.ConfigureAll.Text.2", "While active, adds your Strength modifier (minimum 0, untyped) to attack rolls and damage. You suffer a penalty equal to your Strength modifier (minimum 0) to AC.")),
                    configureBuff: buff => buff
                        .AddContextStatBonus(StatType.AdditionalAttackBonus, Common.Rank(), descriptor: Desc)
                        .AddContextStatBonus(StatType.AdditionalDamage, Common.Rank(), descriptor: Desc)
                        .AddContextStatBonus(StatType.AC, Common.Rank(), descriptor: Desc, multiplier: -1)),
                CreateStance(
                    baseStat: StatType.Dexterity,
                    internalName: "LiquidForm",
                    featureGuid: Guids.Stance.Feature.Dex,
                    buffGuid: Guids.Stance.Buff.Dex,
                    activatableGuid: Guids.Stance.Activatable.Dex,
                    keyPrefix: "Stance_Dex",
                    displayName: LocalizedText.Text("Stance_Dex.Name", "Willow in the Gale"),
                    description: BuildDescription(
                        LocalizedText.Text("Text.StanceFeats.ConfigureAll.Text.3", "Dexterity"),
                        LocalizedText.Text("Stance_Dex.LoreTitle", "Sinuous Evasion."),
                        LocalizedText.Text("Stance_Dex.Lore", "Like a willow in a gale, you favor yielding movement over the force of a committed strike."),
                        LocalizedText.Text("Text.StanceFeats.ConfigureAll.Text.4", "While active, adds your Dexterity modifier (minimum 0, untyped) to AC and Reflex saves. You suffer a penalty equal to your Dexterity modifier (minimum 0) to attack rolls.")),
                    configureBuff: buff => buff
                        .AddContextStatBonus(StatType.AC, Common.Rank(), descriptor: Desc)
                        .AddContextStatBonus(StatType.SaveReflex, Common.Rank(), descriptor: Desc)
                        .AddContextStatBonus(StatType.AdditionalAttackBonus, Common.Rank(), descriptor: Desc, multiplier: -1)),
                CreateStance(
                    baseStat: StatType.Constitution,
                    internalName: "EndlessVigor",
                    featureGuid: Guids.Stance.Feature.Con,
                    buffGuid: Guids.Stance.Buff.Con,
                    activatableGuid: Guids.Stance.Activatable.Con,
                    keyPrefix: "Stance_Con",
                    displayName: LocalizedText.Text("Stance_Con.Name", "Mountain's Deep Roots"),
                    description: BuildDescription(
                        LocalizedText.Text("Text.StanceFeats.ConfigureAll.Text.5", "Constitution"),
                        LocalizedText.Text("Stance_Con.LoreTitle", "Anchor of the Earth."),
                        LocalizedText.Text("Stance_Con.Lore", "You settle into a mountain's patient stillness, gathering yourself at the cost of offensive tempo."),
                        LocalizedText.Text("Text.StanceFeats.ConfigureAll.Text.6", "On activation, gain temporary hit points equal to your Constitution modifier (minimum 0); this stance does not refresh them each round. Your attack rolls and Attack of Opportunity count are reduced by your Constitution modifier (minimum 0).")),
                    configureBuff: buff =>
                    {
                        return buff
                            .AddTemporaryHitPointsFromAbilityValue(descriptor: Desc, removeWhenHitPointsEnd: false, value: Common.Rank())
                            .AddContextStatBonus(StatType.AdditionalAttackBonus, Common.Rank(), descriptor: Desc, multiplier: -1)
                            .AddContextStatBonus(StatType.AttackOfOpportunityCount, Common.Rank(), descriptor: Desc, multiplier: -1);
                    }),
                CreateStance(
                    baseStat: StatType.Intelligence,
                    internalName: "TacticalMind",
                    featureGuid: Guids.Stance.Feature.Int,
                    buffGuid: Guids.Stance.Buff.Int,
                    activatableGuid: Guids.Stance.Activatable.Int,
                    keyPrefix: "Stance_Int",
                    displayName: LocalizedText.Text("Stance_Int.Name", "Grandmaster's Gambit"),
                    description: BuildDescription(
                        LocalizedText.Text("Text.StanceFeats.ConfigureAll.Text.7", "Intelligence"),
                        LocalizedText.Text("Stance_Int.LoreTitle", "The Waiting Blade."),
                        LocalizedText.Text("Stance_Int.Lore", "You study the openings left by passing foes, keeping a careful account of their movement."),
                        LocalizedText.Text("Text.StanceFeats.ConfigureAll.Text.8", "While active, attacks of opportunity and their count gain an untyped bonus equal to your Intelligence modifier, minimum 0. Other attack rolls take an equal penalty.")),
                    includeNegativeRank: true,
                    configureBuff: buff => buff
                        .AddAttackOfOpportunityAttackBonus(Common.Rank(), descriptor: Desc, notAttackOfOpportunity: false)
                        .AddAttackOfOpportunityAttackBonus(Common.Rank(AbilityRankType.StatBonus), descriptor: Desc, notAttackOfOpportunity: true)
                        .AddContextStatBonus(StatType.AttackOfOpportunityCount, Common.Rank(), descriptor: Desc)),
                CreateStance(
                    baseStat: StatType.Wisdom,
                    internalName: "CenteredMind",
                    featureGuid: Guids.Stance.Feature.Wis,
                    buffGuid: Guids.Stance.Buff.Wis,
                    activatableGuid: Guids.Stance.Activatable.Wis,
                    keyPrefix: "Stance_Wis",
                    displayName: LocalizedText.Text("Stance_Wis.Name", "Mirror of Still Waters"),
                    description: BuildDescription(
                        LocalizedText.Text("Text.StanceFeats.ConfigureAll.Text.9", "Wisdom"),
                        LocalizedText.Text("Stance_Wis.LoreTitle", "Void of the Ascetic."),
                        LocalizedText.Text("Stance_Wis.Lore", "You quiet the impulse to strike, accepting a gentler attack in exchange for a steadier guard."),
                        LocalizedText.Text("Text.StanceFeats.ConfigureAll.Text.10", "While active, adds your Wisdom modifier (minimum 0, untyped) to AC and Will saves. Attack rolls and damage take an equal penalty; this stance does not reduce initiative.")),
                    configureBuff: buff => buff
                        .AddContextStatBonus(StatType.AC, Common.Rank(), descriptor: Desc)
                        .AddContextStatBonus(StatType.SaveWill, Common.Rank(), descriptor: Desc)
                        .AddContextStatBonus(StatType.AdditionalAttackBonus, Common.Rank(), descriptor: Desc, multiplier: -1)
                        .AddContextStatBonus(StatType.AdditionalDamage, Common.Rank(), descriptor: Desc, multiplier: -1)),
                CreateStance(
                    baseStat: StatType.Charisma,
                    internalName: "CommandingPresence",
                    featureGuid: Guids.Stance.Feature.Cha,
                    buffGuid: Guids.Stance.Buff.Cha,
                    activatableGuid: Guids.Stance.Activatable.Cha,
                    keyPrefix: "Stance_Cha",
                    displayName: LocalizedText.Text("Stance_Cha.Name", "Vanguard's Banner"),
                    description: BuildDescription(
                        LocalizedText.Text("Text.StanceFeats.ConfigureAll.Text.11", "Charisma"),
                        LocalizedText.Text("Stance_Cha.LoreTitle", "Warlord's Rallying Banner."),
                        LocalizedText.Text("Stance_Cha.Lore", "You place your companions' courage before your own safety and become the standard they rally around."),
                        LocalizedText.Text("Text.StanceFeats.ConfigureAll.Text.12", "While active, other allies within 30 feet gain an untyped bonus to attack rolls equal to your Charisma modifier (minimum 0). You suffer a penalty equal to your Charisma modifier (minimum 0) to AC.")),
                    configureBuff: buff =>
                    {
                        var allyBuff = BuffConfigurator.New("CommandingPresenceAllyBuff", Guids.Stance.AllyBuff.CommandingPresence)
                            .SetDisplayName(Common.L("Stance_Cha.AllyBuff.Name", LocalizedText.Text("Stance_Cha.Name", "Vanguard's Banner")))
                            .SetDescription(Common.L(
                                "Stance_Cha.AllyBuff.Desc",
                                LocalizedText.Text("Stance_Cha.AllyBuff.Desc", "<i>Stance · Charisma</i>\n<i>The Warlord's Standard.</i> Your ally fights under the inspiring canopy of your sovereign courage.\n\n<b>Effect:</b> Grants an untyped bonus to attack rolls equal to the user's Charisma modifier (minimum 0) while within 30 feet."),
                                tagEncyclopediaEntries: true))
                            .SetIconIfPresent("CommandingPresence");
                        AddRanks(allyBuff, StatType.Charisma, includeNegativeRank: false);
                        var configuredAllyBuff = allyBuff
                            .AddContextStatBonus(StatType.AdditionalAttackBonus, Common.Rank(), descriptor: Desc)
                            .AddRecalculateOnStatChange(stat: StatType.Charisma)
                            .Configure();

                        var areaEffect = AbilityAreaEffectConfigurator.New("CommandingPresenceArea", Guids.Stance.AreaEffect.CommandingPresence)
                            .SetAffectEnemies(false)
                            .SetAffectDead(false)
                            .SetShape(AreaEffectShape.Cylinder)
                            .SetSize(new Feet(30))
                            .AddAbilityAreaEffectBuff(
                                configuredAllyBuff,
                                checkConditionEveryRound: true,
                                condition: ConditionsBuilder.New().IsAlly().IsCaster(negate: true))
                            .Configure();

                        return buff
                            .AddAreaEffect(areaEffect)
                            .AddContextStatBonus(StatType.AC, Common.Rank(), descriptor: Desc, multiplier: -1);
                    }),
            };

            ApplyIntraFamilyMutex(feats);
        }

        private static LocalizedText BuildDescription(
            LocalizedText attr,
            LocalizedText loreTitle,
            LocalizedText loreBody,
            LocalizedText effect)
            => LocalizedText.Format("Text.StanceFeats.BuildDescription.Description", "<i>Stance · {0}</i>\n<i>{1}</i> {2}\n\n<b>Effect:</b> {3}\n\n<b>Activation:</b> Free action to toggle. The Stance exclusion group limits learning to one Stance feat. With it off, multiple learned stances can be toggled on together; there is no separate activation limit.\n\n<b>Restrictions:</b> While its exclusion group is on (configurable in mod settings), you can have only one Stance feat.", attr, loreTitle, loreBody, effect);

        private static void AddRanks(BuffConfigurator cfg, StatType baseStat, bool includeNegativeRank)
        {
            cfg.AddContextRankConfig(ContextRankConfigs.StatBonus(baseStat, Desc, AbilityRankType.Default, min: 0));
            if (includeNegativeRank)
            {
                // The game clamps after the progression, so the negated modifier is capped at 0 (max), not floored:
                // a positive modifier becomes an equal penalty, and a negative one never turns into a bonus.
                cfg.AddContextRankConfig(
                    ContextRankConfigs.StatBonus(baseStat, Desc, AbilityRankType.StatBonus, max: 0)
                        .WithMultiplyByModifierProgression(-1));
            }
        }

        private static void ApplyIntraFamilyMutex(IReadOnlyList<BlueprintFeature> feats)
        {
        }

        private static BlueprintFeature CreateStance(
            StatType baseStat,
            string internalName,
            string featureGuid,
            string buffGuid,
            string activatableGuid,
            string keyPrefix,
            LocalizedText displayName,
            LocalizedText description,
            Func<BuffConfigurator, BuffConfigurator> configureBuff,
            bool includeNegativeRank = false)
        {
            var buff = BuffConfigurator.New($"{internalName}Buff", buffGuid)
                .SetDisplayName(Common.L($"{keyPrefix}.Buff.Name", displayName))
                .SetDescription(Common.L($"{keyPrefix}.Buff.Desc", description, tagEncyclopediaEntries: true))
                .SetIconIfPresent(internalName);
            AddRanks(buff, baseStat, includeNegativeRank);
            var configuredBuff = configureBuff(buff)
                .AddRecalculateOnStatChange(stat: baseStat)
                .Configure();

            var activatable = ActivatableAbilityConfigurator.New($"{internalName}Activatable", activatableGuid)
                .SetDisplayName(Common.L($"{keyPrefix}.Activatable.Name", displayName))
                .SetDescription(Common.L($"{keyPrefix}.Activatable.Desc", description, tagEncyclopediaEntries: true))
                .SetIconIfPresent(internalName)
                .SetBuff(configuredBuff)
                .SetActivationType(AbilityActivationType.Immediately)
                .SetDeactivateIfCombatEnded(false)
                .SetDeactivateIfOwnerDisabled(true)
                .SetDeactivateIfOwnerUnconscious(true)
                .Configure();

            return FeatSelection.Stance.NewFeat(internalName, featureGuid)
                .SetDisplayName(Common.L($"{keyPrefix}.Feature.Name", displayName))
                .SetDescription(Common.L($"{keyPrefix}.Feature.Desc", description, tagEncyclopediaEntries: true))
                .SetIconIfPresent(internalName)
                .AddFacts(new List<Blueprint<BlueprintUnitFactReference>> { activatable })
                .Configure();
        }
    }
}
