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
    /// <summary>Growth: permanent and lasting gains from kills.</summary>
    internal static class GrowthFeats
    {
        private static bool Initialized;

        public static void ConfigureAll()
        {
            if (Initialized) return;
            Initialized = true;

            ConfigureKillGrowth();
            ConfigureNecromastery();
        }

        private static void ConfigureKillGrowth()
        {
            Growth("EssenceShift", Guids.Playstyle.EssenceShift, Guids.Playstyle.EssenceShiftCounter, LocalizedText.Text("Text.GrowthFeats.ConfigureKillGrowth.Text", "Stolen Grace"),
                StatType.Dexterity, 100, casterKillsOnly: false, partyKills: false,
                LocalizedText.Text("Text.GrowthFeats.ConfigureKillGrowth.Text.2", "Every 100 enemies you kill permanently increase your Dexterity by 1. There is no limit."));
            Growth("FleshHeap", Guids.Playstyle.FleshHeap, Guids.Playstyle.FleshHeapCounter, LocalizedText.Text("Text.GrowthFeats.ConfigureKillGrowth.Text.3", "Corpse-Forged Bulk"),
                StatType.Strength, 100, casterKillsOnly: false, partyKills: true,
                LocalizedText.Text("Text.GrowthFeats.ConfigureKillGrowth.Text.4", "Every 100 enemies killed by your party (including pets) permanently increase your Strength by 1. There is no limit."));
            Growth("ArcaneSiphon", Guids.Playstyle.ArcaneSiphon, Guids.Playstyle.ArcaneSiphonCounter, LocalizedText.Text("Text.GrowthFeats.ConfigureKillGrowth.Text.5", "Stolen Epiphany"),
                StatType.Intelligence, 10, casterKillsOnly: true, partyKills: false,
                LocalizedText.Text("Text.GrowthFeats.ConfigureKillGrowth.Text.6", "Every 10 spellcasting enemies you kill permanently increase your Intelligence by 1. There is no limit."));
            Growth("DevouredVigor", Guids.Playstyle.DevouredVigor, Guids.Playstyle.DevouredVigorCounter, LocalizedText.Text("Text.GrowthFeats.ConfigureKillGrowth.Text.7", "Siphoned Vitality"),
                StatType.HitPoints, 10, casterKillsOnly: false, partyKills: false,
                LocalizedText.Text("Text.GrowthFeats.ConfigureKillGrowth.Text.8", "Every 10 enemies you kill permanently increase your maximum hit points by 1. There is no limit."));
        }

        private static void Growth(string name, string guid, string counterGuid, LocalizedText displayName, StatType stat, int killsPerPoint,
            bool casterKillsOnly, bool partyKills, LocalizedText effect)
        {
            // The counter's rank is the kill count; it is saved with the character like any feature.
            var counter = FeatureConfigurator.New($"{name}Counter", counterGuid)
                .SetDisplayName(Common.L($"{name}Counter.Name", LocalizedText.Format("Text.GrowthFeats.Growth.Text", "{0} (kills)", displayName)))
                .SetDescription(Common.L($"{name}Counter.Desc", effect))
                .SetHideInUI(true)
                .SetHideInCharacterSheetAndLevelUp(true)
                .SetRanks(1000000)
                .AddContextRankConfig(ContextRankConfigs.FeatureRank(counterGuid).WithDivStepProgression(killsPerPoint))
                .AddContextStatBonus(stat, Common.Rank(), descriptor: ModifierDescriptor.UntypedStackable)
                .Configure();
            Feat(FeatSelection.Growth, name, guid, displayName, Desc(LocalizedText.Text("Text.GrowthFeats.Growth.Text.2", "Growth"), effect))
                .AddComponent<KillGrowth>(c =>
                {
                    c.Counter = counter.ToReference<BlueprintFeatureReference>();
                    c.CasterKillsOnly = casterKillsOnly;
                    c.PartyKills = partyKills;
                })
                .Configure();
        }

        private static void ConfigureNecromastery()
        {
            var souls = BuffConfigurator.New("NecromasteryBuff", Guids.Dota.NecromasteryBuff)
                .SetDisplayName(Common.L("NecromasteryBuff.Name", LocalizedText.Text("NecromasteryBuff.Name", "Souls")))
                .SetDescription(Common.L("NecromasteryBuff.Desc", LocalizedText.Text("NecromasteryBuff.Desc", "+1 damage per 2 souls.")))
                .SetStacking(StackingType.Rank)
                .SetRanks(40)
                .AddContextRankConfig(ContextRankConfigs.BuffRank(Guids.Dota.NecromasteryBuff).WithDivStepProgression(2))
                .AddContextStatBonus(StatType.AdditionalDamage, Common.Rank(), descriptor: ModifierDescriptor.UntypedStackable)
                .Configure();
            Feat(FeatSelection.Growth, "Necromastery", Guids.Dota.Necromastery, LocalizedText.Text("Text.GrowthFeats.ConfigureNecromastery.Text", "Soul-Tether Harvest"),
                    Desc(LocalizedText.Text("Text.GrowthFeats.ConfigureNecromastery.Text.2", "Growth"),
                        LocalizedText.Text("Text.GrowthFeats.ConfigureNecromastery.Text.3", "Each enemy you kill grants a soul, up to twice your character level (and never more than 40). You deal +1 damage per 2 souls. Souls persist until you die.")))
                .AddComponent<NecromasterySouls>(c => c.Buff = souls.ToReference<BlueprintBuffReference>())
                .Configure();
        }
    }

    /// <summary>
    /// Counts kills by the owner (or, with PartyKills, by any party member or pet) in a hidden
    /// ranked feature that grants permanent bonuses. Listens globally to see party kills.
    /// </summary>
    [TypeId("5e3c1a9f7d5b4e3c1a9f7d5b3e1c9a7f")]
    public class KillGrowth : UnitFactComponentDelegate, IGlobalRulebookHandler<RuleDealDamage>, IGlobalRulebookSubscriber
    {
        public BlueprintFeatureReference Counter;
        public bool CasterKillsOnly;
        public bool PartyKills;

        public void OnEventAboutToTrigger(RuleDealDamage evt) { }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
            var target = evt.Target;
            var killer = evt.Initiator;
            if (target == null || killer == null || evt.Result <= 0) return;
            // Count the hit that took the target from positive to zero or below only once.
            if (target.HPLeft > 0 || target.HPLeft + evt.Result <= 0 || !target.IsEnemy(Owner)) return;
            if (PartyKills ? !Kingmaker.Game.Instance.Player.PartyAndPets.Contains(killer) : killer != Owner) return;
            if (CasterKillsOnly && !target.Descriptor.Spellbooks.Any()) return;

            var counter = Counter.Get();
            var fact = Owner.Progression.Features.GetFact(counter);
            if (fact == null) Owner.Progression.Features.AddFeature(counter);
            else fact.AddRank();
        }
    }

    [TypeId("4b5c6d7e8f90a1b2c3d4e5f60718293a")]
    public class NecromasterySouls : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleDealDamage>, IInitiatorRulebookSubscriber
    {
        public BlueprintBuffReference Buff;

        public void OnEventAboutToTrigger(RuleDealDamage evt) { }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
            var target = evt.Target;
            if (target == null || evt.Result <= 0 || !target.IsEnemy(Owner)) return;
            if (target.HPLeft > 0 || target.HPLeft + evt.Result <= 0) return;
            var buff = Buff.Get();
            var cap = Math.Min(40, 2 * Owner.Descriptor.Progression.CharacterLevel);
            if ((Owner.Buffs.GetBuff(buff)?.Rank ?? 0) < cap) Owner.AddBuff(buff, Context);
        }
    }
}
