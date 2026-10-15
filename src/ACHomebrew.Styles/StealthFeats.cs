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
    /// <summary>Stealth: slipping out of sight during combat.</summary>
    internal static class StealthFeats
    {
        private const string VanillaGreaterInvisibilityBuff = "e6b35473a237a6045969253beb09777c";
        private static bool Initialized;

        public static void ConfigureAll()
        {
            if (Initialized) return;
            Initialized = true;

            ConfigureStealth();
        }

        private static void ConfigureStealth()
        {
            var cooldown = Buff("ReStealthCooldownBuff", Guids.Playstyle.ReStealthCooldownBuff, Common.Text("Text.StealthFeats.ConfigureStealth.Text", "Vanishing Stroke (used)", false), Common.Text("Text.StealthFeats.ConfigureStealth.Text", "刃落无影（已触发）", true),
                    Common.Text("Text.StealthFeats.ConfigureStealth.Text.2", "Vanishing Stroke has triggered this round.", false), Common.Text("Text.StealthFeats.ConfigureStealth.Text.2", "刃落无影本轮已触发。", true))
                .Configure();
            var vanish = ActionsBuilder.New().Conditional(
                ConditionsBuilder.New().HasBuff(cooldown, negate: true),
                ifTrue: ActionsBuilder.New()
                    .ApplyBuff(VanillaGreaterInvisibilityBuff, Rounds(1))
                    .ApplyBuff(cooldown, Rounds(1)));
            Feat(FeatSelection.Stealth, "ReStealth", Guids.Playstyle.ReStealth, Common.Text("Text.StealthFeats.ConfigureStealth.Text.3", "Vanishing Stroke", false), Common.Text("Text.StealthFeats.ConfigureStealth.Text.3", "刃落无影", true),
                    Desc(Common.Text("Text.StealthFeats.ConfigureStealth.Text.4", "Stealth", false), Common.Text("Text.StealthFeats.ConfigureStealth.Text.4", "潜行", true),
                        Common.Text("Text.StealthFeats.ConfigureStealth.Text.5", "Once per round, when your weapon attack drops an enemy, scores a critical hit, or deals sneak attack damage, you become greatly invisible for 1 round.", false),
                        Common.Text("Text.StealthFeats.ConfigureStealth.Text.5", "每轮一次，你的武器攻击使敌人倒下、造成重击或造成偷袭伤害时，你获得1轮高等隐身。", true)))
                .AddInitiatorAttackWithWeaponTrigger(action: vanish, actionsOnInitiator: true, onlyHit: true, reduceHPToZero: true)
                .AddInitiatorAttackWithWeaponTrigger(action: vanish, actionsOnInitiator: true, onlyHit: true, criticalHit: true)
                .AddInitiatorAttackWithWeaponTrigger(action: vanish, actionsOnInitiator: true, onlyHit: true, onlySneakAttack: true)
                .Configure();
        }
    }
}
