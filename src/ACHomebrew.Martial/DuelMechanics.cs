using System;
using System.Collections.Generic;
using System.Linq;
using BlueprintCore.Utils;
using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Newtonsoft.Json;

namespace ACHomebrew.Feats
{
    /// <summary>
    /// Duel rules. A Duel links two units for two rounds through <see cref="DuelLink"/> buffs. For an owner of
    /// Oath of the Duel, a Cavalier-style challenge also counts as a duel for victories and Single Combat.
    /// </summary>
    public static class DuelRules
    {
        public static readonly TimeSpan Duration = 2.Rounds().Seconds;

        // The challenge effect placed on the challenged creature by Cavalier's Challenge and Knight's Challenge.
        internal static readonly string[] VanillaChallengeBuffs = { "4f0218323ad379248b69de8a9501159f" };
        private static readonly HashSet<BlueprintGuid> ChallengeBuffs = new();

        [ThreadStatic] private static UnitEntityData PendingOpponent;

        // Cached: CanAttack runs for every cursor hover and AI decision.
        private static BlueprintBuff s_DuelBuff;
        private static BlueprintFeature s_Oath, s_MythicOath, s_Victories;
        internal static BlueprintBuff DuelBuff => s_DuelBuff ??= BlueprintTool.Get<BlueprintBuff>(Guids.Duel.Buff);
        internal static BlueprintFeature Oath => s_Oath ??= BlueprintTool.Get<BlueprintFeature>(Guids.Duel.Oath);
        internal static BlueprintFeature MythicOath => s_MythicOath ??= BlueprintTool.Get<BlueprintFeature>(Guids.Duel.MythicOath);
        internal static BlueprintFeature Victories => s_Victories ??= BlueprintTool.Get<BlueprintFeature>(Guids.Duel.Victories);

        /// <summary>
        /// Lets another mod's challenge count as a duel. Pass the effect its challenge places on the challenged
        /// creature, with the challenger as the effect's caster, as Cavalier's Challenge does.
        /// </summary>
        public static void RegisterChallenge(BlueprintBuff challengedEffect)
        {
            if (challengedEffect == null || !ChallengeBuffs.Add(challengedEffect.AssetGuid)) return;
            if (challengedEffect.ComponentsArray.OfType<ChallengeDuelTracker>().Any()) return;
            challengedEffect.ComponentsArray = challengedEffect.ComponentsArray
                .Concat(new BlueprintComponent[] { new ChallengeDuelTracker { name = "$ACHomebrewChallengeDuelTracker" } }).ToArray();
        }

        internal static void RegisterVanillaChallenges()
        {
            foreach (var guid in VanillaChallengeBuffs)
                RegisterChallenge(BlueprintTool.Get<BlueprintBuff>(guid));
        }

        public static bool IsChallengeEffect(BlueprintBuff buff) => buff != null && ChallengeBuffs.Contains(buff.AssetGuid);

        /// <summary>The creature this unit is dueling through the Duel ability, or null.</summary>
        public static UnitEntityData DuelOpponent(UnitEntityData unit)
        {
            var buff = unit?.Buffs.GetBuff(DuelBuff);
            UnitEntityData opponent = null;
            buff?.CallComponents<DuelLink>(link => opponent = link.Opponent);
            return opponent;
        }

        public static bool IsDueling(UnitEntityData unit) => DuelOpponent(unit) != null;

        /// <summary>Active challenges that count as duels: (challenge effect, challenger, challenged).</summary>
        internal static IEnumerable<(Buff effect, UnitEntityData challenger, UnitEntityData challenged)> ChallengeDuels(UnitEntityData unit)
        {
            if (unit == null) yield break;
            foreach (var buff in unit.Buffs.Enumerable.ToList())
            {
                var challenger = buff.Context.MaybeCaster;
                if (IsChallengeEffect(buff.Blueprint) && challenger != null && challenger != unit && challenger.Descriptor.HasFact(Oath))
                    yield return (buff, challenger, unit);
            }
            if (!unit.Descriptor.HasFact(Oath)) yield break;
            foreach (var buff in unit.Get<UnitPartDuel>()?.IssuedChallenges() ?? Enumerable.Empty<Buff>())
                yield return (buff, unit, buff.Owner);
        }

        /// <summary>Every creature this unit is currently dueling, by Duel or by a challenge counted as a duel.</summary>
        public static HashSet<UnitEntityData> Opponents(UnitEntityData unit)
        {
            var result = new HashSet<UnitEntityData>();
            var opponent = DuelOpponent(unit);
            if (opponent != null) result.Add(opponent);
            foreach (var (_, challenger, challenged) in ChallengeDuels(unit))
                result.Add(challenger == unit ? challenged : challenger);
            return result;
        }

        /// <summary>A dueling unit may attack and target only its opponent among creatures it could attack.</summary>
        public static bool ForbidsAttack(UnitEntityData attacker, UnitEntityData target)
        {
            if (attacker == null || target == null || target == attacker) return false;
            var opponent = DuelOpponent(attacker);
            return opponent != null && target != opponent;
        }

        /// <summary>The mythic oath keeps its owner under the player's (or its AI's) control during a duel.</summary>
        internal static bool KeepsControl(UnitEntityData unit) => unit.Descriptor.HasFact(MythicOath);

        public static bool UsedThisEncounter(UnitEntityData unit) => unit.Get<UnitPartDuel>()?.UsedThisEncounter == true;

        internal static bool CanTarget(UnitEntityData caster, UnitEntityData target)
            => caster != null && target != null && target != caster && caster.IsEnemy(target)
               && target.Descriptor.State.IsConscious && !IsDueling(caster) && !IsDueling(target);

        /// <summary>Starts a two-round duel between the caster and the target. Returns false if either cannot duel.</summary>
        public static bool Start(UnitEntityData caster, UnitEntityData target)
        {
            if (!CanTarget(caster, target)) return false;
            caster.Ensure<UnitPartDuel>().UsedThisEncounter = true;
            Link(caster, target, caster);
            Link(target, caster, caster);
            if (DuelOpponent(caster) == target && DuelOpponent(target) == caster) return true;
            End(caster);
            End(target);
            return false;
        }

        private static void Link(UnitEntityData unit, UnitEntityData opponent, UnitEntityData caster)
        {
            var previous = PendingOpponent;
            PendingOpponent = opponent;
            try { unit.AddBuff(DuelBuff, caster, Duration); }
            finally { PendingOpponent = previous; }
        }

        internal static UnitEntityData TakePendingOpponent() => PendingOpponent;

        /// <summary>Ends the Duel of this unit and of its opponent.</summary>
        public static void End(UnitEntityData unit)
        {
            var buff = unit?.Buffs.GetBuff(DuelBuff);
            if (buff == null) return;
            var opponent = DuelOpponent(unit);
            buff.Remove();
            var other = opponent?.Buffs.GetBuff(DuelBuff);
            if (other != null && DuelOpponent(opponent) == unit) other.Remove();
        }

        /// <summary>
        /// Settles every duel of a unit that has just fallen (unconscious or dead). Each surviving opponent
        /// wins once; a unit that falls together with its opponent wins nothing.
        /// </summary>
        internal static void Settle(UnitEntityData fallen)
        {
            var winners = new HashSet<UnitEntityData>();
            var opponent = DuelOpponent(fallen);
            if (opponent != null) winners.Add(opponent);
            foreach (var (effect, challenger, challenged) in ChallengeDuels(fallen).ToList())
            {
                var part = challenged.Ensure<UnitPartDuel>();
                if (part.IsSettled(effect)) continue;
                part.Settle(effect);
                winners.Add(challenger == fallen ? challenged : challenger);
            }
            End(fallen);
            foreach (var winner in winners)
                if (winner.Descriptor.State.IsConscious && winner.HPLeft > 0) AwardVictory(winner);
        }

        /// <summary>Adds one permanent duel victory: +1 to the winner's weapon damage.</summary>
        public static void AwardVictory(UnitEntityData winner)
        {
            var fact = winner.Progression.Features.GetFact(Victories) ?? winner.Progression.Features.AddFeature(Victories);
            fact?.CallComponents<DuelVictoryBonus>(bonus => bonus.AddVictory());
        }

        public static int VictoryCount(UnitEntityData unit)
        {
            var count = 0;
            unit?.Progression.Features.GetFact(Victories)?.CallComponents<DuelVictoryBonus>(bonus => count = bonus.Count);
            return count;
        }

        /// <summary>Sets every part of a calculated damage to <paramref name="final"/> of its final value.</summary>
        internal static void Rescale(List<DamageValue> values, Func<int, int> final)
        {
            for (var i = 0; i < values.Count; i++)
            {
                var value = values[i];
                var current = value.FinalValue;
                var scaled = Math.Max(0, final(current));
                if (scaled == current) continue;
                values[i] = new DamageValue(value.Source, value.Reduction + scaled, value.RollAndBonusValue, value.RollResult, value.TacticalCombatDRModifier);
            }
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class DuelLinkData
    {
        [JsonProperty] public EntityRef<UnitEntityData> Opponent;
        // Derived again whenever the effect turns on, like the game's AddCondition component.
        public bool Forced;
    }

    /// <summary>
    /// The two-round duel effect on each duelist. It records the opponent, compels a duelist without the mythic oath
    /// to attack only the opponent, and prevents damage the duelist deals to anyone else. The duel ends for both
    /// when either effect ends.
    /// </summary>
    [TypeId("9b86cbb57179413b90fe1802667a39cf")]
    public class DuelLink : UnitBuffComponentDelegate<DuelLinkData>, IInitiatorRulebookHandler<RuleCalculateDamage>, IInitiatorRulebookSubscriber
    {
        public UnitEntityData Opponent
        {
            get
            {
                var opponent = Data.Opponent.Entity;
                return opponent != null && !opponent.Destroyed ? opponent : null;
            }
        }

        protected override void OnActivate()
        {
            if (Data.Opponent.Entity == null) Data.Opponent = DuelRules.TakePendingOpponent();
        }

        protected override void OnTurnOn()
        {
            Data.Forced = !DuelRules.KeepsControl(Owner);
            if (Data.Forced) Owner.State.AddCondition(UnitCondition.AttackNearest, Buff);
        }

        protected override void OnTurnOff()
        {
            if (!Data.Forced) return;
            Data.Forced = false;
            Owner.State.RemoveCondition(UnitCondition.AttackNearest);
            // Hand control back now instead of on the unit's next confusion tick.
            if (!Owner.State.HasCondition(UnitCondition.AttackNearest) && !Owner.State.HasCondition(UnitCondition.Confusion))
                Owner.Remove<UnitPartConfusion>();
        }

        protected override void OnDeactivate()
        {
            var opponent = Data.Opponent.Entity;
            var other = opponent?.Buffs.GetBuff(DuelRules.DuelBuff);
            if (other != null && DuelRules.DuelOpponent(opponent) == Owner) other.Remove();
        }

        public void OnEventAboutToTrigger(RuleCalculateDamage evt) { }

        public void OnEventDidTrigger(RuleCalculateDamage evt)
        {
            if (evt.Target != null && evt.Target != Opponent) DuelRules.Rescale(evt.CalculatedDamage, _ => 0);
        }
    }

    /// <summary>Single Combat: while dueling, damage from creatures other than your opponents is halved.</summary>
    [TypeId("bcc1501a23ca4952a0a9659dc2492eb3")]
    public class SingleCombatGuard : UnitFactComponentDelegate, ITargetRulebookHandler<RuleCalculateDamage>, ITargetRulebookSubscriber
    {
        public void OnEventAboutToTrigger(RuleCalculateDamage evt) { }

        public void OnEventDidTrigger(RuleCalculateDamage evt)
        {
            var opponents = DuelRules.Opponents(Owner);
            if (opponents.Count == 0 || opponents.Contains(evt.Initiator)) return;
            DuelRules.Rescale(evt.CalculatedDamage, damage => damage / 2);
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class DuelVictoryData
    {
        [JsonProperty] public int Count;
    }

    /// <summary>Permanent duel victories: +1 weapon damage each. Respec removes this hidden feature.</summary>
    [TypeId("ef875308b9944fe882732a23fdfab64c")]
    public class DuelVictoryBonus : UnitFactComponentDelegate<DuelVictoryData>
    {
        public int Count => Data.Count;

        protected override void OnTurnOn() => Apply();

        protected override void OnTurnOff() => Owner.Stats.AdditionalDamage.RemoveModifiersFrom(Runtime);

        public void AddVictory()
        {
            Data.Count++;
            Owner.Stats.AdditionalDamage.RemoveModifiersFrom(Runtime);
            Apply();
        }

        private void Apply()
        {
            if (Data.Count > 0) Owner.Stats.AdditionalDamage.AddModifier(Data.Count, Runtime, ModifierDescriptor.UntypedStackable);
        }
    }

    /// <summary>Starts a duel between the ability's caster and its target.</summary>
    [TypeId("ba2bb98318c64a37affdbd89cf9e9a80")]
    public class ContextActionStartDuel : ContextAction
    {
        public override string GetCaption() => "Start a duel";

        public override void RunAction() => DuelRules.Start(Context?.MaybeCaster, Target?.Unit);
    }

    /// <summary>Duel can be used once per encounter, never while already dueling, against a conscious enemy that is not dueling.</summary>
    [TypeId("48d9335526244c3c9da90f6049646460")]
    public class DuelAbilityRestriction : BlueprintComponent, IAbilityCasterRestriction, IAbilityTargetRestriction
    {
        public bool IsCasterRestrictionPassed(UnitEntityData caster)
            => !DuelRules.IsDueling(caster) && !DuelRules.UsedThisEncounter(caster);

        public string GetAbilityCasterRestrictionUIText()
            => Common.CurrentText("Duel.Restriction.Caster", "Already dueling, or Duel was already used in this encounter");

        public bool IsTargetRestrictionPassed(UnitEntityData caster, TargetWrapper target)
            => DuelRules.CanTarget(caster, target?.Unit);

        public string GetAbilityTargetRestrictionUIText(UnitEntityData caster, TargetWrapper target)
            => Common.CurrentText("Duel.Restriction.Target", "Target a conscious enemy that is not dueling");
    }

    /// <summary>
    /// Added to each challenge effect counted as a duel: lets the challenger find the creatures it has challenged.
    /// The list is rebuilt from the active effects when a save loads.
    /// </summary>
    [TypeId("1d5daf5b860c40bca9da6c51d8703400")]
    public class ChallengeDuelTracker : UnitBuffComponentDelegate
    {
        protected override void OnTurnOn() => Context.MaybeCaster?.Ensure<UnitPartDuel>().Track(Buff);

        protected override void OnTurnOff() => Context.MaybeCaster?.Get<UnitPartDuel>()?.Untrack(Buff);
    }

    /// <summary>Per-unit duel bookkeeping kept in the save.</summary>
    [TypeId("987b22cd4d3a4c918213aedd52b073fd")]
    [JsonObject(MemberSerialization.OptIn)]
    public class UnitPartDuel : UnitPart
    {
        [JsonProperty] public bool UsedThisEncounter { get; internal set; }
        [JsonProperty] private List<string> SettledChallenges = new();
        private readonly List<Buff> Issued = new();

        internal void Track(Buff challenge)
        {
            if (!Issued.Contains(challenge)) Issued.Add(challenge);
        }

        internal void Untrack(Buff challenge) => Issued.Remove(challenge);

        internal IEnumerable<Buff> IssuedChallenges()
        {
            Issued.RemoveAll(buff => buff.Owner == null || buff.IsDisposed);
            return Issued.ToList();
        }

        internal bool IsSettled(Buff challenge) => SettledChallenges.Contains(challenge.UniqueId);

        internal void Settle(Buff challenge)
        {
            // Forget challenges that have since ended so the list stays small.
            SettledChallenges.RemoveAll(id => Owner.Buffs.Enumerable.All(buff => buff.UniqueId != id));
            SettledChallenges.Add(challenge.UniqueId);
        }
    }

    /// <summary>Global duel events: settlement when a duelist falls, and the per-encounter reset.</summary>
    internal sealed class DuelEvents : IUnitLifeStateChanged
    {
        private static DuelEvents Instance;

        internal static void Subscribe()
        {
            if (Instance != null) return;
            Instance = new DuelEvents();
            EventBus.Subscribe(Instance);
        }

        public void HandleUnitLifeStateChanged(UnitEntityData unit, UnitLifeState prevLifeState)
        {
            if (prevLifeState == UnitLifeState.Conscious && !unit.Descriptor.State.IsConscious) DuelRules.Settle(unit);
        }

        [HarmonyPatch(typeof(UnitCombatState), nameof(UnitCombatState.LeaveCombat))]
        private static class EndEncounter
        {
            private static void Prefix(UnitCombatState __instance, out bool __state) => __state = __instance.IsInCombat;

            private static void Postfix(UnitCombatState __instance, bool __state)
            {
                var part = __instance.Unit.Get<UnitPartDuel>();
                if (__state && !__instance.IsInCombat && part != null) part.UsedThisEncounter = false;
            }
        }

        // The game's attack-nearest compulsion drives a compelled duelist in real time and turn-based mode alike.
        // During a duel, aim it at the opponent instead of the nearest creature.
        [HarmonyPatch(typeof(UnitConfusionController), "AttackNearest")]
        private static class AttackOpponent
        {
            private static bool Prefix(UnitPartConfusion part, ref UnitCommand __result)
            {
                var unit = part.Owner.Unit;
                var opponent = DuelRules.DuelOpponent(unit);
                if (opponent == null || opponent.Descriptor.State.IsDead) return true;
                var charge = part.Owner.Abilities.GetAbility(BlueprintRoot.Instance.SystemMechanics.ChargeAbility);
                __result = charge != null && charge.Data.IsAvailable && charge.Data.CanTarget(opponent)
                    ? new UnitUseAbility(charge.Data, opponent)
                    : UnitAttack.CreateAttackCommand(unit, opponent);
                return false;
            }
        }

        // Attacks, hostile targeting and area selection all ask CanAttack.
        [HarmonyPatch(typeof(UnitEntityData), nameof(UnitEntityData.CanAttack), typeof(UnitEntityData))]
        private static class OnlyTheOpponent
        {
            private static void Postfix(UnitEntityData __instance, UnitEntityData unit, ref bool __result)
            {
                if (__result && DuelRules.ForbidsAttack(__instance, unit)) __result = false;
            }
        }
    }
}
