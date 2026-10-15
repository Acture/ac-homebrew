using BlueprintCore.Actions.Builder.ContextEx;
using BlueprintCore.Actions.Builder;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Buffs;
using BlueprintCore.Conditions.Builder.ContextEx;
using BlueprintCore.Conditions.Builder;
using BlueprintCore.Utils.Types;
using HarmonyLib;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic;
using Kingmaker.Utility;
using Kingmaker;
using System.Collections.Generic;
using System.Linq;
using System;
using static ACHomebrew.Feats.FeatBuilders;

namespace ACHomebrew.Feats
{
    /// <summary>Momentum: bonuses that build up over consecutive kills, hits and attacks.</summary>
    internal static class MomentumFeats
    {
        private static bool Initialized;

        public static void ConfigureAll()
        {
            if (Initialized) return;
            Initialized = true;

            ConfigureKillsAndRhythm();
            ConfigureFervorAndCrits();
        }

        private static void ConfigureKillsAndRhythm()
        {
            var spree = Buff("KillingSpreeBuff", Guids.Playstyle.KillingSpreeBuff, LocalizedText.Text("Text.MomentumFeats.ConfigureKillsAndRhythm.Text", "Cascading Carnage"),
                    LocalizedText.Text("Text.MomentumFeats.ConfigureKillsAndRhythm.Text.2", "+2 on attack rolls and +10 feet speed per stack (up to 3 stacks)."))
                .SetStacking(StackingType.Rank)
                .SetRanks(3)
                .AddContextRankConfig(ContextRankConfigs.BuffRank(Guids.Playstyle.KillingSpreeBuff))
                .AddContextStatBonus(StatType.AdditionalAttackBonus, Common.Rank(), descriptor: ModifierDescriptor.UntypedStackable, multiplier: 2)
                .AddContextStatBonus(StatType.Speed, Common.Rank(), descriptor: ModifierDescriptor.UntypedStackable, multiplier: 10)
                .Configure();
            Feat(FeatSelection.Momentum, "KillingSpree", Guids.Playstyle.KillingSpree, LocalizedText.Text("Text.MomentumFeats.ConfigureKillsAndRhythm.Text", "Cascading Carnage"),
                    Desc(LocalizedText.Text("Text.MomentumFeats.ConfigureKillsAndRhythm.Text.3", "Momentum"),
                        LocalizedText.Text("Text.MomentumFeats.ConfigureKillsAndRhythm.Text.4", "When your weapon attack drops an enemy, gain a Cascading Carnage stack until the end of your next turn: +2 on attack rolls and +10 feet speed per stack, up to 3 stacks.")))
                .AddInitiatorAttackWithWeaponTrigger(action: ActionsBuilder.New().ApplyBuff(spree, Rounds(2)), actionsOnInitiator: true,
                    onlyHit: true, reduceHPToZero: true)
                .Configure();

            var rhythm = Buff("BattleRhythmBuff", Guids.Playstyle.BattleRhythmBuff, LocalizedText.Text("Text.MomentumFeats.ConfigureKillsAndRhythm.Text.5", "Unbroken Measure"),
                    LocalizedText.Text("Text.MomentumFeats.ConfigureKillsAndRhythm.Text.6", "+1 on attack rolls and +2 damage per stack (up to 3 stacks)."))
                .SetStacking(StackingType.Rank)
                .SetRanks(3)
                .AddContextRankConfig(ContextRankConfigs.BuffRank(Guids.Playstyle.BattleRhythmBuff))
                .AddContextStatBonus(StatType.AdditionalAttackBonus, Common.Rank(), descriptor: ModifierDescriptor.UntypedStackable)
                .AddContextStatBonus(StatType.AdditionalDamage, Common.Rank(), descriptor: ModifierDescriptor.UntypedStackable, multiplier: 2)
                .Configure();
            Feat(FeatSelection.Momentum, "BattleRhythm", Guids.Playstyle.BattleRhythm, LocalizedText.Text("Text.MomentumFeats.ConfigureKillsAndRhythm.Text.5", "Unbroken Measure"),
                    Desc(LocalizedText.Text("Text.MomentumFeats.ConfigureKillsAndRhythm.Text.3", "Momentum"),
                        LocalizedText.Text("Text.MomentumFeats.ConfigureKillsAndRhythm.Text.7", "Each weapon hit you land adds an Unbroken Measure stack for 3 rounds: +1 on attack rolls and +2 damage per stack, up to 3 stacks. Each weapon hit you take removes one stack.")))
                .AddInitiatorAttackWithWeaponTrigger(action: ActionsBuilder.New().ApplyBuff(rhythm, Rounds(3)), actionsOnInitiator: true, onlyHit: true)
                .AddTargetAttackWithWeaponTrigger(actionOnSelf: ActionsBuilder.New().RemoveBuffSingleStack(rhythm), onlyHit: true)
                .Configure();
        }

        private static void ConfigureFervorAndCrits()
        {
            var fervor = BuffConfigurator.New("FervorBuff", Guids.Dota.FervorBuff)
                .SetDisplayName(Common.L("FervorBuff.Name", LocalizedText.Text("FervorBuff.Name", "Quarry's Obsession")))
                .SetDescription(Common.L("FervorBuff.Desc", LocalizedText.Text("FervorBuff.Desc", "+1 dodge AC per stack; at 3 stacks, one extra attack in a full attack.")))
                .SetStacking(StackingType.Rank)
                .SetRanks(3)
                .AddContextRankConfig(ContextRankConfigs.BuffRank(Guids.Dota.FervorBuff))
                .AddContextStatBonus(StatType.AC, Common.Rank(), descriptor: ModifierDescriptor.Dodge)
                .Configure();
            var fervorAttack = BuffConfigurator.New("FervorAttackBuff", Guids.Dota.FervorAttackBuff)
                .SetDisplayName(Common.L("FervorAttackBuff.Name", LocalizedText.Text("FervorAttackBuff.Name", "Quarry's Obsession: extra attack")))
                .SetDescription(Common.L("FervorAttackBuff.Desc", LocalizedText.Text("FervorAttackBuff.Desc", "One extra attack in a full attack.")))
                .AddBuffExtraAttack(number: 1)
                .Configure();
            Feat(FeatSelection.Momentum, "Fervor", Guids.Dota.Fervor, LocalizedText.Text("Text.MomentumFeats.ConfigureFervorAndCrits.Text", "Quarry's Obsession"),
                    Desc(LocalizedText.Text("Text.MomentumFeats.ConfigureFervorAndCrits.Text.2", "Momentum"),
                        LocalizedText.Text("Text.MomentumFeats.ConfigureFervorAndCrits.Text.3", "Each consecutive weapon attack against the same target, hit or miss, adds an Obsession stack (up to 3): +1 dodge AC per stack, and at 3 stacks you make one extra attack in a full attack. Attacking a different target resets the stacks.")))
                .AddComponent<FervorTracker>(c =>
                {
                    c.Buff = fervor.ToReference<BlueprintBuffReference>();
                    c.AttackBuff = fervorAttack.ToReference<BlueprintBuffReference>();
                })
                .Configure();

            Feat(FeatSelection.Momentum, "CrushingRhythm", Guids.Dota.CrushingRhythm, LocalizedText.Text("Text.MomentumFeats.ConfigureFervorAndCrits.Text.4", "Fourth-Beat Ruin"),
                    Desc(LocalizedText.Text("Text.MomentumFeats.ConfigureFervorAndCrits.Text.2", "Momentum"),
                        LocalizedText.Text("Text.MomentumFeats.ConfigureFervorAndCrits.Text.5", "Every fourth weapon attack you make, hit or miss, is an automatic critical threat that is automatically confirmed if it hits.")))
                .AddComponent<EveryNthHitCritical>(c => c.Every = 4)
                .Configure();
        }
    }

    [TypeId("0718293a4b5c6d7e8f90a1b2c3d4e5f6")]
    public class FervorTracker : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleAttackWithWeapon>, IInitiatorRulebookSubscriber
    {
        public BlueprintBuffReference Buff;
        public BlueprintBuffReference AttackBuff;
        private UnitEntityData LastTarget;

        public void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }

        public void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            if (evt.Target == null) return;
            var buff = Buff.Get();
            if (evt.Target != LastTarget)
            {
                Owner.Buffs.RemoveFact(buff);
                Owner.Buffs.RemoveFact(AttackBuff.Get());
                LastTarget = evt.Target;
            }
            Owner.AddBuff(buff, Context, 2.Rounds().Seconds);
            if (Owner.Buffs.GetBuff(buff)?.Rank >= 3) Owner.AddBuff(AttackBuff.Get(), Context, 2.Rounds().Seconds);
        }
    }

    [TypeId("18293a4b5c6d7e8f90a1b2c3d4e5f607")]
    public class EveryNthHitCritical : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleAttackRoll>, IInitiatorRulebookSubscriber
    {
        public int Every = 4;
        private int Hits;

        public void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            if (evt.Weapon != null && Hits == Every - 1)
            {
                evt.AutoCriticalThreat = true;
                evt.AutoCriticalConfirmation = true;
            }
        }

        public void OnEventDidTrigger(RuleAttackRoll evt)
        {
            if (evt.Weapon != null) Hits = (Hits + 1) % Every;
        }
    }
}
