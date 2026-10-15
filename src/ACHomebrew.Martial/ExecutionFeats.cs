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
    /// <summary>Execution: damage scaled by missing or maximum hit points, and lifesteal.</summary>
    internal static class ExecutionFeats
    {
        private static bool Initialized;

        public static void ConfigureAll()
        {
            if (Initialized) return;
            Initialized = true;

            ConfigureExecution();
        }

        private static void ConfigureExecution()
        {
            Feat(FeatSelection.Execution, "CorrosiveFinish", Guids.Dota.CorrosiveFinish, Common.Text("Text.ExecutionFeats.ConfigureExecution.Text", "Deepening the Cleft", false), Common.Text("Text.ExecutionFeats.ConfigureExecution.Text", "乘隙溃创", true),
                    Desc(Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.2", "Execution", false), Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.2", "处决", true),
                        Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.3", "For every 10% of hit points the target is missing (up to 90%), your weapon attacks deal +1 damage plus 1 per 10 character levels.", false),
                        Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.3", "目标每损失10%生命值（最多计90%），你的武器攻击对其伤害+1，每10角色等级再+1。", true)))
                .AddComponent<MissingHealthDamage>(c => c.UseTarget = true)
                .Configure();

            Feat(FeatSelection.Execution, "PhoenixFury", Guids.Dota.PhoenixFury, Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.4", "Pyre of Defiance", false), Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.4", "残躯燃焰", true),
                    Desc(Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.2", "Execution", false), Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.2", "处决", true),
                        Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.5", "At the start of each round in combat, enemies within 15 feet take fire damage equal to half your character level (minimum 1), increased by the percentage of hit points you are missing (for example, +50% when you are at half health).", false),
                        Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.5", "战斗中每轮开始时，15尺内的敌人受到等于你角色等级一半（最低1）的火焰伤害，并按你已损失生命值的百分比提高（例如半血时+50%）。", true)))
                .AddComponent<BurningAura>()
                .Configure();

            Feat(FeatSelection.Execution, "Feast", Guids.Dota.Feast, Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.6", "Colossus Reaver", false), Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.6", "猎巨诛命", true),
                    Desc(Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.2", "Execution", false), Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.2", "处决", true),
                        Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.7", "Your weapon hits deal extra damage equal to 2% of the target's maximum hit points (minimum 1, at most three times your character level).", false),
                        Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.7", "你的武器命中额外造成目标最大生命值2%的伤害（最低1，最多为角色等级的3倍）。", true)))
                .AddComponent<PercentHealthDamage>(c => c.FromCurrent = false)
                .Configure();

            Feat(FeatSelection.Execution, "FeastCurrent", Guids.Dota.FeastCurrent, Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.8", "Vanguard's Sunder", false), Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.8", "挫敌锐芒", true),
                    Desc(Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.2", "Execution", false), Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.2", "处决", true),
                        Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.9", "Your weapon hits deal extra damage equal to 3% of the target's current hit points (minimum 1, at most three times your character level).", false),
                        Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.9", "你的武器命中额外造成目标当前生命值3%的伤害（最低1，最多为角色等级的3倍）。", true)))
                .AddComponent<PercentHealthDamage>(c => c.FromCurrent = true)
                .Configure();

            Feat(FeatSelection.Execution, "FeastSpell", Guids.Dota.FeastSpell, Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.10", "Arcane Resection", false), Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.10", "崩元析命", true),
                    Desc(Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.2", "Execution", false), Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.2", "处决", true),
                        Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.11", "When your spell damages an enemy, it takes extra damage equal to 2% of its maximum hit points (minimum 1, at most three times your character level). Once per target for each spell you cast.", false),
                        Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.11", "你的法术伤害敌人时，其额外受到最大生命值2%的伤害（最低1，最多为角色等级的3倍）。你每施放一次法术，每个目标只触发一次。", true)))
                .AddComponent<SpellFeastDamage>()
                .Configure();

            Feat(FeatSelection.Execution, "FeastNatural", Guids.Dota.FeastNatural, Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.12", "Primal Gluttony", false), Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.12", "荒蛮撕嚼", true),
                    Desc(Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.2", "Execution", false), Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.2", "处决", true),
                        Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.13", "Your natural attacks deal extra damage equal to 2% of the target's maximum hit points (minimum 1, at most three times your character level), and you heal the same amount.", false),
                        Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.13", "你的天生武器攻击额外造成目标最大生命值2%的伤害（最低1，最多为角色等级的3倍），并回复等量生命。", true)))
                .AddComponent<PercentHealthDamage>(c =>
                {
                    c.FromCurrent = false;
                    c.NaturalOnly = true;
                    c.HealSelf = true;
                })
                .Configure();

            Feat(FeatSelection.Execution, "Lifesteal", Guids.Dota.Lifesteal, Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.14", "Sanguine Tithe", false), Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.14", "饮血淬芒", true),
                    Desc(Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.2", "Execution", false), Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.2", "处决", true),
                        Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.15", "You heal 15% of the damage your weapon attacks deal.", false),
                        Common.Text("Text.ExecutionFeats.ConfigureExecution.Text.15", "你回复武器攻击所造成伤害的15%。", true)))
                .AddComponent<WeaponLifesteal>()
                .Configure();
        }
    }

    [TypeId("a1b2c3d4e5f60718293a4b5c6d7e8f90")]
    public class MissingHealthDamage : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateWeaponStats>, IInitiatorRulebookSubscriber
    {
        public bool UseTarget;

        public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            var unit = UseTarget ? evt.AttackWithWeapon?.Target : Owner;
            if (unit == null || unit.MaxHP <= 0) return;
            var missingTenths = Math.Min(9, (int)(10 * (1 - (float)Math.Max(0, unit.HPLeft) / unit.MaxHP)));
            var bonus = missingTenths * (1 + Owner.Descriptor.Progression.CharacterLevel / 10);
            if (bonus > 0) evt.AddDamageModifier(bonus, Fact);
        }

        public void OnEventDidTrigger(RuleCalculateWeaponStats evt) { }
    }

    [TypeId("b2c3d4e5f60718293a4b5c6d7e8f90a1")]
    public class PercentHealthDamage : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleAttackWithWeapon>, IInitiatorRulebookSubscriber
    {
        public bool FromCurrent;
        public bool NaturalOnly;
        public bool HealSelf;

        public void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }

        public void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            if (!evt.AttackRoll.IsHit || evt.Target == null) return;
            if (NaturalOnly && evt.Weapon?.Blueprint.IsNatural != true) return;
            var amount = Amount(Owner, evt.Target, FromCurrent);
            CombatActions.DealDirectDamage(Owner, evt.Target, amount);
            if (HealSelf) CombatActions.Heal(Owner, Owner, amount);
        }

        internal static int Amount(UnitEntityData source, UnitEntityData target, bool fromCurrent)
        {
            var basis = fromCurrent ? Math.Max(0, target.HPLeft) * 3 / 100 : target.MaxHP * 2 / 100;
            return Math.Max(1, Math.Min(basis, 3 * source.Descriptor.Progression.CharacterLevel));
        }
    }

    [TypeId("2b7200e358a04b10833f725c9f490c39")]
    public class SpellFeastDamage : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleDealDamage>, IInitiatorRulebookSubscriber
    {
        [ThreadStatic] private static bool Feasting;
        private readonly HashSet<(object cast, UnitEntityData target)> Fed = new();

        public void OnEventAboutToTrigger(RuleDealDamage evt) { }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
            var context = evt.Reason?.Context;
            if (Feasting || evt.Result <= 0 || evt.Target == null || evt.Reason?.Ability?.Spellbook == null || context == null) return;
            if (Fed.Count > 256) Fed.Clear();
            if (!Fed.Add((context, evt.Target))) return;
            Feasting = true;
            try
            {
                CombatActions.DealDirectDamage(Owner, evt.Target, PercentHealthDamage.Amount(Owner, evt.Target, fromCurrent: false));
            }
            finally
            {
                Feasting = false;
            }
        }
    }

    [TypeId("ae9f467f31b949eab006ea1c3deea277")]
    public class WeaponLifesteal : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleDealDamage>, IInitiatorRulebookSubscriber
    {
        public void OnEventAboutToTrigger(RuleDealDamage evt) { }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
            if (evt.Result <= 0 || evt.DamageBundle.Weapon == null) return;
            CombatActions.Heal(Owner, Owner, evt.Result * 15 / 100);
        }
    }

    [TypeId("a9344ef73caf4372aaf027e4644d0ce1")]
    public class BurningAura : UnitFactComponentDelegate, Kingmaker.Controllers.Units.ITickEachRound
    {
        public void OnNewRound()
        {
            if (!Owner.IsInCombat || Owner.State.IsDead || Owner.MaxHP <= 0) return;
            var level = Owner.Descriptor.Progression.CharacterLevel;
            var missing = 1 - (float)Math.Max(0, Owner.HPLeft) / Owner.MaxHP;
            var damage = (int)(Math.Max(1, level / 2) * (1 + missing));
            foreach (var enemy in GameHelper.GetTargetsAround(Owner.Position, 15.Feet(), checkLOS: false)
                .Where(unit => unit.IsEnemy(Owner) && !unit.State.IsDead).ToList())
            {
                Rulebook.Trigger(new RuleDealDamage(Owner, enemy,
                    new EnergyDamage(new DiceFormula(0, DiceType.Zero), damage, Kingmaker.Enums.Damage.DamageEnergyType.Fire)));
            }
        }
    }
}
