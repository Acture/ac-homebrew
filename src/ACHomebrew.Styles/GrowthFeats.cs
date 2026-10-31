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
using System.Runtime.CompilerServices;
using System;
using Newtonsoft.Json;
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
            ConfigureFingerOfTheReaper();
        }

        private static void ConfigureFingerOfTheReaper()
        {
            Feat(FeatSelection.Growth, "FingerOfTheReaper", Guids.Reaper.FingerOfTheReaper, LocalizedText.Text("FingerOfTheReaper.Name", "Finger of the Reaper"),
                    LocalizedText.Text("FingerOfTheReaper.Desc", "<i>Every soul your word unmakes lends weight to the next.</i>\n\nYour Finger of Death deals +1 damage per caster level. Each creature it kills permanently adds +1 more, without limit. A creature counts when it dies and your Finger of Death dealt the final, lethal damage; each death counts once, so a creature raised and slain again counts again. Works with Finger of Death from any source, including scrolls and wands. Kills are kept in your save and reset if you lose this feat, for example by respec."))
                .AddComponent<ReaperFinger>()
                .Configure();
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

    [JsonObject(MemberSerialization.OptIn)]
    public class ReaperFingerData
    {
        [JsonProperty] public int Kills;
    }

    /// <summary>
    /// Finger of the Reaper: the owner's Finger of Death deals +1 damage per caster level plus +1 per creature
    /// it has killed. The kill count is the feat's own saved data, so losing the feat resets it.
    /// </summary>
    [TypeId("43ed4383695044de95570da4016fb74b")]
    public class ReaperFinger : UnitFactComponentDelegate<ReaperFingerData>, IInitiatorRulebookHandler<RuleDealDamage>, IInitiatorRulebookSubscriber,
        IGlobalRulebookHandler<RuleHealDamage>, IGlobalRulebookSubscriber, IUnitLifeStateChanged
    {
        public const string FingerOfDeath = "6f1dcf6cfa92d1948a740195707c0dbe";

        // Damage from the owner's Finger of Death that left its target at 0 HP or below.
        private static readonly ConditionalWeakTable<RuleDealDamage, object> LethalHits = new();
        private static readonly object Lethal = new();

        public int Kills => Data.Kills;

        private static readonly BlueprintGuid FingerOfDeathGuid = BlueprintGuid.Parse(FingerOfDeath);

        public static bool IsFingerOfDeath(RuleDealDamage evt)
        {
            var ability = evt.Reason.Ability?.Blueprint ?? evt.Reason.Context?.SourceAbility ?? evt.SourceAbility;
            return ability != null && (ability.AssetGuid == FingerOfDeathGuid || ability.Parent?.AssetGuid == FingerOfDeathGuid);
        }

        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {
            if (!IsFingerOfDeath(evt)) return;
            var first = evt.DamageBundle.FirstOrDefault();
            var casterLevel = evt.Reason.Context?.Params?.CasterLevel ?? 0;
            var bonus = Math.Max(0, casterLevel) + Data.Kills;
            if (first != null && bonus > 0) first.AddModifier(bonus, Fact);
        }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
            if (evt.Target != null && evt.Result > 0 && evt.Target.HPLeft <= 0 && IsFingerOfDeath(evt))
                LethalHits.GetValue(evt, _ => Lethal);
        }

        // Healing after the hit means the spell did not kill, even if the creature dies later without new damage.
        public void OnEventAboutToTrigger(RuleHealDamage evt) { }

        public void OnEventDidTrigger(RuleHealDamage evt)
        {
            var lastHit = evt.Target?.LastHandledDamage;
            if (lastHit != null && lastHit.Initiator == Owner) LethalHits.Remove(lastHit);
        }

        public void HandleUnitLifeStateChanged(UnitEntityData unit, UnitLifeState prevLifeState)
        {
            var lastHit = unit.LastHandledDamage;
            if (lastHit == null || lastHit.Initiator != Owner || !LethalHits.TryGetValue(lastHit, out _)) return;
            // The hit is settled by the next life-state change: a death counts once; anything else (unconscious,
            // revived) means it did not kill, so a later death that reuses LastHandledDamage is not credited.
            LethalHits.Remove(lastHit);
            if (prevLifeState != UnitLifeState.Dead && unit.Descriptor.State.LifeState == UnitLifeState.Dead) Data.Kills++;
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
