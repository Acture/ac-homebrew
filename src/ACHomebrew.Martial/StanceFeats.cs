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
                    displayNameEn: Common.Text("Stance_Str.Name", "Berserker's Overrun"),
                    displayNameZh: Common.Text("Stance_Str.Name", "破阵裂山势", true),
                    description: BuildDescription(
                        Common.Text("Text.StanceFeats.ConfigureAll.Text", "Strength", false),
                        Common.Text("Text.StanceFeats.ConfigureAll.Text", "力量", true),
                        Common.Text("Stance_Str.LoreTitle", "Gorum's Reckless Abandon."),
                        Common.Text("Stance_Str.Lore", "You commit to the next blow with a battle zeal reminiscent of Gorum, leaving less attention for your guard."),
                        Common.Text("Stance_Str.LoreTitle", "狂神破阵。", true),
                        Common.Text("Stance_Str.Lore", "你以令人想起戈鲁姆的战意全力挥击，也因此分出更少心力守护自身。", true),
                        Common.Text("Text.StanceFeats.ConfigureAll.Text.2", "While active, adds your Strength modifier (minimum 0, untyped) to attack rolls and damage. You suffer a penalty equal to your Strength modifier (minimum 0) to AC.", false),
                        Common.Text("Text.StanceFeats.ConfigureAll.Text.2", "激活时，将你的力量调整值（最低0，无类型加值）附加至攻击检定与伤害检定。你的防御等级（AC）承受等同于力量调整值（最低0）的减值。", true)),
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
                    displayNameEn: Common.Text("Stance_Dex.Name", "Willow in the Gale"),
                    displayNameZh: Common.Text("Stance_Dex.Name", "惊鸿穿林势", true),
                    description: BuildDescription(
                        Common.Text("Text.StanceFeats.ConfigureAll.Text.3", "Dexterity", false),
                        Common.Text("Text.StanceFeats.ConfigureAll.Text.3", "敏捷", true),
                        Common.Text("Stance_Dex.LoreTitle", "Sinuous Evasion."),
                        Common.Text("Stance_Dex.Lore", "Like a willow in a gale, you favor yielding movement over the force of a committed strike."),
                        Common.Text("Stance_Dex.LoreTitle", "惊鸿避刃。", true),
                        Common.Text("Stance_Dex.Lore", "你如狂风中的柳枝般顺势而动，将余力留给闪避而非重击。", true),
                        Common.Text("Text.StanceFeats.ConfigureAll.Text.4", "While active, adds your Dexterity modifier (minimum 0, untyped) to AC and Reflex saves. You suffer a penalty equal to your Dexterity modifier (minimum 0) to attack rolls.", false),
                        Common.Text("Text.StanceFeats.ConfigureAll.Text.4", "激活时，将你的敏捷调整值（最低0，无类型加值）附加至防御等级（AC）与反射豁免。你的攻击检定承受等同于敏捷调整值（最低0）的减值。", true)),
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
                    displayNameEn: Common.Text("Stance_Con.Name", "Mountain's Deep Roots"),
                    displayNameZh: Common.Text("Stance_Con.Name", "不动磐峰势", true),
                    description: BuildDescription(
                        Common.Text("Text.StanceFeats.ConfigureAll.Text.5", "Constitution", false),
                        Common.Text("Text.StanceFeats.ConfigureAll.Text.5", "体质", true),
                        Common.Text("Stance_Con.LoreTitle", "Anchor of the Earth."),
                        Common.Text("Stance_Con.Lore", "You settle into a mountain's patient stillness, gathering yourself at the cost of offensive tempo."),
                        Common.Text("Stance_Con.LoreTitle", "磐岳归根。", true),
                        Common.Text("Stance_Con.Lore", "你如山岳般沉稳地收束自身，并为此放慢进攻的节奏。", true),
                        Common.Text("Text.StanceFeats.ConfigureAll.Text.6", "On activation, gain temporary hit points equal to your Constitution modifier (minimum 0); this stance does not refresh them each round. Your attack rolls and Attack of Opportunity count are reduced by your Constitution modifier (minimum 0).", false),
                        Common.Text("Text.StanceFeats.ConfigureAll.Text.6", "启动时获得等同于体质调整值（最低0）的临时生命值；此姿态不会每轮刷新它们。你的攻击检定与借机攻击次数承受等同于体质调整值（最低0）的减值。", true)),
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
                    displayNameEn: Common.Text("Stance_Int.Name", "Grandmaster's Gambit"),
                    displayNameZh: Common.Text("Stance_Int.Name", "弈者静待势", true),
                    description: BuildDescription(
                        Common.Text("Text.StanceFeats.ConfigureAll.Text.7", "Intelligence", false),
                        Common.Text("Text.StanceFeats.ConfigureAll.Text.7", "智力", true),
                        Common.Text("Stance_Int.LoreTitle", "The Waiting Blade."),
                        Common.Text("Stance_Int.Lore", "You study the openings left by passing foes, keeping a careful account of their movement."),
                        Common.Text("Stance_Int.LoreTitle", "弈者断局。", true),
                        Common.Text("Stance_Int.Lore", "你研读敌人移动时留下的空隙，审慎把握他们的行动轨迹。", true),
                        Common.Text("Text.StanceFeats.ConfigureAll.Text.8", "While active, attacks of opportunity and their count gain an untyped bonus equal to your Intelligence modifier, minimum 0. Other attack rolls gain an untyped bonus equal to the negative of your Intelligence modifier, minimum 0.", false),
                        Common.Text("Text.StanceFeats.ConfigureAll.Text.8", "激活时，借机攻击的攻击检定与次数获得等同于智力调整值（最低0）的无类型加值。其他攻击检定获得等同于智力调整值相反数（最低0）的无类型加值。", true)),
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
                    displayNameEn: Common.Text("Stance_Wis.Name", "Mirror of Still Waters"),
                    displayNameZh: Common.Text("Stance_Wis.Name", "明镜止水势", true),
                    description: BuildDescription(
                        Common.Text("Text.StanceFeats.ConfigureAll.Text.9", "Wisdom", false),
                        Common.Text("Text.StanceFeats.ConfigureAll.Text.9", "感知", true),
                        Common.Text("Stance_Wis.LoreTitle", "Void of the Ascetic."),
                        Common.Text("Stance_Wis.Lore", "You quiet the impulse to strike, accepting a gentler attack in exchange for a steadier guard."),
                        Common.Text("Stance_Wis.LoreTitle", "澄澈自照。", true),
                        Common.Text("Stance_Wis.Lore", "你平息急于出手的冲动，以较轻的攻势换取更稳固的守势。", true),
                        Common.Text("Text.StanceFeats.ConfigureAll.Text.10", "While active, adds your Wisdom modifier (minimum 0, untyped) to AC and Will saves. Attack rolls and damage take an equal penalty; this stance does not reduce initiative.", false),
                        Common.Text("Text.StanceFeats.ConfigureAll.Text.10", "激活时，将你的感知调整值（最低0，无类型加值）附加至防御等级（AC）与意志豁免。你的攻击检定与伤害检定承受等同于感知调整值（最低0）的减值；此姿态不降低先攻。", true)),
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
                    displayNameEn: Common.Text("Stance_Cha.Name", "Vanguard's Banner"),
                    displayNameZh: Common.Text("Stance_Cha.Name", "金戈铁旌势", true),
                    description: BuildDescription(
                        Common.Text("Text.StanceFeats.ConfigureAll.Text.11", "Charisma", false),
                        Common.Text("Text.StanceFeats.ConfigureAll.Text.11", "魅力", true),
                        Common.Text("Stance_Cha.LoreTitle", "Warlord's Rallying Banner."),
                        Common.Text("Stance_Cha.Lore", "You place your companions' courage before your own safety and become the standard they rally around."),
                        Common.Text("Stance_Cha.LoreTitle", "铁旌号令。", true),
                        Common.Text("Stance_Cha.Lore", "你将同伴的勇气置于自身安危之前，成为众人聚拢的旗帜。", true),
                        Common.Text("Text.StanceFeats.ConfigureAll.Text.12", "While active, other allies within 30 feet gain an untyped bonus to attack rolls equal to your Charisma modifier (minimum 0). You suffer a penalty equal to your Charisma modifier (minimum 0) to AC.", false),
                        Common.Text("Text.StanceFeats.ConfigureAll.Text.12", "激活时，30英尺内的其他盟友在攻击检定上获得等同于你的魅力调整值（最低0）的无类型加值。你的防御等级（AC）承受等同于魅力调整值（最低0）的减值。", true)),
                    configureBuff: buff =>
                    {
                        var allyBuff = BuffConfigurator.New("CommandingPresenceAllyBuff", Guids.Stance.AllyBuff.CommandingPresence)
                            .SetDisplayName(Common.L("Stance_Cha.AllyBuff.Name", Common.Text("Stance_Cha.Name", "Vanguard's Banner"), Common.Text("Stance_Cha.Name", "金戈铁旌势", true)))
                            .SetDescription(Common.L(
                                "Stance_Cha.AllyBuff.Desc",
                                Common.Text("Stance_Cha.AllyBuff.Desc", "<i>Stance · Charisma</i>\n<i>The Warlord's Standard.</i> Your ally fights under the inspiring canopy of your sovereign courage.\n\n<b>Effect:</b> Grants an untyped bonus to attack rolls equal to the user's Charisma modifier (minimum 0) while within 30 feet.", false),
                                Common.Text("Stance_Cha.AllyBuff.Desc", "<i>姿态 · 魅力</i>\n<i>百战铁旌。</i>沐浴在战帅无畏勇烈之光辉下，盟军同仇敌忾。\n\n<b>效果：</b>在30英尺内，攻击检定获得等同于专长持有者魅力调整值（最低0）的无类型加值。", true),
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

        private static (string en, string zh) BuildDescription(
            string attrEn,
            string attrZh,
            string loreTitleEn,
            string loreBodyEn,
            string loreTitleZh,
            string loreBodyZh,
            string effectEn,
            string effectZh)
        {
            var en = Common.Format("Text.StanceFeats.BuildDescription.Description", "<i>Stance · {0}</i>\n<i>{1}</i> {2}\n\n<b>Effect:</b> {3}\n\n<b>Activation:</b> Free action to toggle. The Stance exclusion group limits learning to one Stance feat. With it off, multiple learned stances can be toggled on together; there is no separate activation limit.\n\n<b>Restrictions:</b> While its exclusion group is on (configurable in mod settings), you can have only one Stance feat.", false, attrEn, loreTitleEn, loreBodyEn, effectEn);
            var zh = Common.Format("Text.StanceFeats.BuildDescription.Description", "<i>姿态 · {0}</i>\n<i>{1}</i> {2}\n\n<b>效果：</b>{3}\n\n<b>启动：</b>切换姿态为自由动作。姿态互斥组启用时只能学习一种姿态专长；关闭后可学习并同时激活多个姿态，没有单独的启动互斥。\n\n<b>限制：</b>启用该互斥组时（可在模组设置中调整），只能拥有一个“姿态”专长。", true, attrZh, loreTitleZh, loreBodyZh, effectZh);
            return (en, zh);
        }

        private static void AddRanks(BuffConfigurator cfg, StatType baseStat, bool includeNegativeRank)
        {
            cfg.AddContextRankConfig(ContextRankConfigs.StatBonus(baseStat, Desc, AbilityRankType.Default, min: 0));
            if (includeNegativeRank)
            {
                cfg.AddContextRankConfig(
                    ContextRankConfigs.StatBonus(baseStat, Desc, AbilityRankType.StatBonus, min: 0)
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
            string displayNameEn,
            string displayNameZh,
            (string en, string zh) description,
            Func<BuffConfigurator, BuffConfigurator> configureBuff,
            bool includeNegativeRank = false)
        {
            var buff = BuffConfigurator.New($"{internalName}Buff", buffGuid)
                .SetDisplayName(Common.L($"{keyPrefix}.Buff.Name", displayNameEn, displayNameZh))
                .SetDescription(Common.L($"{keyPrefix}.Buff.Desc", description.en, description.zh, tagEncyclopediaEntries: true))
                .SetIconIfPresent(internalName);
            AddRanks(buff, baseStat, includeNegativeRank);
            var configuredBuff = configureBuff(buff)
                .AddRecalculateOnStatChange(stat: baseStat)
                .Configure();

            var activatable = ActivatableAbilityConfigurator.New($"{internalName}Activatable", activatableGuid)
                .SetDisplayName(Common.L($"{keyPrefix}.Activatable.Name", displayNameEn, displayNameZh))
                .SetDescription(Common.L($"{keyPrefix}.Activatable.Desc", description.en, description.zh, tagEncyclopediaEntries: true))
                .SetIconIfPresent(internalName)
                .SetBuff(configuredBuff)
                .SetActivationType(AbilityActivationType.Immediately)
                .SetDeactivateIfCombatEnded(false)
                .SetDeactivateIfOwnerDisabled(true)
                .SetDeactivateIfOwnerUnconscious(true)
                .Configure();

            return FeatSelection.Stance.NewFeat(internalName, featureGuid)
                .SetDisplayName(Common.L($"{keyPrefix}.Feature.Name", displayNameEn, displayNameZh))
                .SetDescription(Common.L($"{keyPrefix}.Feature.Desc", description.en, description.zh, tagEncyclopediaEntries: true))
                .SetIconIfPresent(internalName)
                .AddFacts(new List<Blueprint<BlueprintUnitFactReference>> { activatable })
                .Configure();
        }
    }
}
