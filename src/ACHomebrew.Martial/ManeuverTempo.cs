using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using BlueprintCore.Utils;
using HarmonyLib;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Newtonsoft.Json;

namespace ACHomebrew.Feats
{
    [TypeId("b121ad36acc74048a176749ca8c0d857")]
    public class ManeuverTempo : UnitFactComponentDelegate
    {
        protected override void OnTurnOn()
        {
            var budget = Owner.Ensure<UnitPartManeuverTempo>();
            if (Owner.IsInCombat && !budget.InEncounter) budget.BeginEncounter();
        }
        // Deliberately retain the unit part on removal: respec/re-adding the feat must not refill it.
    }

    [TypeId("48b07fb83e554315ad45db8502262c62")]
    [JsonObject(MemberSerialization.OptIn)]
    public class UnitPartManeuverTempo : UnitPart
    {
        [JsonProperty] public bool InEncounter { get; private set; }
        [JsonProperty] public int FreeAttemptsThisRound { get; private set; }
        [JsonProperty] public int BurstUsesSpent { get; private set; }
        private readonly List<ManeuverTempoHooks.CommandTicket> Pending = new();

        private static int Cap => Math.Max(0, Math.Min(99, Mod.Settings?.ManeuverFreePerRoundCap ?? 2));
        private static int Free => Math.Max(0, Math.Min(Cap, Mod.Settings?.ManeuverFreePerRound ?? 1));
        private static int Burst => Math.Max(0, Math.Min(99, Mod.Settings?.ManeuverBurstPerCombat ?? 3));

        internal bool CanReserve
        {
            get
            {
                Pending.RemoveAll(ticket => ticket.Command.IsFinished || ticket.Committed);
                int used = FreeAttemptsThisRound + Pending.Count;
                int reservedBurst = Math.Max(0, used - Free) - Math.Max(0, FreeAttemptsThisRound - Free);
                return InEncounter && used < Cap && (used < Free || BurstUsesSpent + reservedBurst < Burst);
            }
        }

        internal void BeginEncounter()
        {
            InEncounter = true;
            FreeAttemptsThisRound = 0;
            BurstUsesSpent = 0;
        }

        internal void LeaveEncounter() => InEncounter = false;
        internal void NewRound() => FreeAttemptsThisRound = 0;
        internal void Reserve(ManeuverTempoHooks.CommandTicket ticket) => Pending.Add(ticket);
        internal void Release(ManeuverTempoHooks.CommandTicket ticket) => Pending.Remove(ticket);

        internal void Commit(ManeuverTempoHooks.CommandTicket ticket)
        {
            if (!Pending.Remove(ticket)) return;
            if (FreeAttemptsThisRound >= Free) ++BurstUsesSpent;
            ++FreeAttemptsThisRound;
            ticket.Committed = true;
        }
    }

    internal static class ManeuverTempoHooks
    {
        internal sealed class CommandTicket
        {
            internal UnitUseAbility Command;
            internal UnitCommand.CommandType NormalType;
            internal UnitCommand.CommandType SelectedType;
            internal bool Locked;
            internal bool Committed;
            internal UnitPartManeuverTempo Budget;
        }

        private static readonly ConditionalWeakTable<UnitUseAbility, CommandTicket> Tickets = new();
        [ThreadStatic] private static bool SuppressConversion;
        [ThreadStatic] private static UnitUseAbility Executing;
        [ThreadStatic] private static UnitUseAbility CheckingCost;

        private static bool OwnsFeat(UnitEntityData unit)
            => unit != null && unit.Descriptor.HasFact(BlueprintTool.Get<BlueprintFeature>(Guids.ManeuverTactics.SeizeOpening));

        private static UnitPartManeuverTempo BudgetFor(AbilityData ability)
        {
            var unit = ability?.Caster?.Unit;
            return unit?.IsInCombat == true && OwnsFeat(unit) && ManeuverCompatibility.IsEligible(ability)
                ? unit.Get<UnitPartManeuverTempo>() : null;
        }

        private static UnitCommand.CommandType NormalAction(AbilityData ability)
        {
            bool previous = SuppressConversion;
            SuppressConversion = true;
            try { return ability.RuntimeActionType; }
            finally { SuppressConversion = previous; }
        }

        private static void ConvertAction(AbilityData ability, ref UnitCommand.CommandType result)
        {
            if (SuppressConversion) return;
            var command = CheckingCost ?? Executing;
            if (command != null && ReferenceEquals(command.Ability, ability) && Tickets.TryGetValue(command, out var ticket) && ticket.Locked)
            {
                result = ticket.SelectedType;
                return;
            }
            if (result != UnitCommand.CommandType.Free && BudgetFor(ability)?.CanReserve == true)
                result = UnitCommand.CommandType.Free;
        }

        private static CommandTicket Ticket(UnitUseAbility command, UnitCommand.CommandType original)
            => Tickets.GetValue(command, _ => new CommandTicket
            {
                Command = command,
                NormalType = original == UnitCommand.CommandType.Free && command.Ability?.Caster?.Unit.Get<UnitPartManeuverTempo>() != null
                    ? NormalAction(command.Ability) : original,
            });

        private static void Lock(CommandTicket ticket, UnitCommand.CommandType selected)
        {
            if (ticket.Locked) return;
            ticket.SelectedType = selected;
            ticket.Locked = true;
            if (selected == UnitCommand.CommandType.Free && ticket.NormalType != UnitCommand.CommandType.Free)
            {
                ticket.Budget = BudgetFor(ticket.Command.Ability);
                ticket.Budget?.Reserve(ticket);
            }
        }

        private static bool CanCastForCommand(AbilityData ability, UnitCommand command)
        {
            var previous = CheckingCost;
            try
            {
                CheckingCost = command as UnitUseAbility;
                // Reading Type also locks a newly inserted active command before its first precheck.
                if (CheckingCost != null) _ = CheckingCost.Type;
                return ability.IsAvailableForCast;
            }
            finally { CheckingCost = previous; }
        }

        // Availability is checked before OnAction. Scope only that getter call, so command-controller
        // events and ordinary UI/new-command queries cannot borrow the active command's reservation.
        [HarmonyPatch(typeof(UnitCommandController), "TickCommandTurnBased")]
        private static class CheckReservedCommandAvailability
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var getter = AccessTools.PropertyGetter(typeof(AbilityData), nameof(AbilityData.IsAvailableForCast));
                var replacement = AccessTools.Method(typeof(ManeuverTempoHooks), nameof(CanCastForCommand));
                int replaced = 0;
                foreach (var instruction in instructions)
                {
                    if (instruction.operand is MethodInfo method && method == getter)
                    {
                        var argument = new CodeInstruction(OpCodes.Ldarg_1);
                        argument.labels.AddRange(instruction.labels);
                        argument.blocks.AddRange(instruction.blocks);
                        instruction.labels.Clear();
                        instruction.blocks.Clear();
                        yield return argument;
                        instruction.opcode = OpCodes.Call;
                        instruction.operand = replacement;
                        ++replaced;
                    }
                    yield return instruction;
                }
                if (replaced != 1) throw new InvalidOperationException("Maneuver command availability check was not found exactly once.");
            }
        }

        // The cooldown check and SpendAction both ask the concrete command whether it is full-round.
        // Its locked cost must win even when its reservation exhausted the remaining free allowance.
        [HarmonyPatch(typeof(UnitCommand), nameof(UnitCommand.IsFullRoundAbility))]
        private static class CheckReservedCommandFullRoundCost
        {
            private static void Prefix(UnitCommand __instance, out UnitUseAbility __state)
            {
                __state = CheckingCost;
                CheckingCost = __instance as UnitUseAbility;
                if (CheckingCost != null) _ = CheckingCost.Type;
            }

            private static void Finalizer(UnitUseAbility __state) => CheckingCost = __state;
        }

        // TTT Core postfixes both getters for CustomSpeedAbilityData. Run after it, and suppress only
        // our own conversion while recovering the underlying cost, preserving other mods' overrides.
        [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.ActionType), MethodType.Getter)]
        private static class ActionType
        {
            [HarmonyPriority(Priority.Last), HarmonyAfter("TabletopTweaks-Core")]
            private static void Postfix(AbilityData __instance, ref UnitCommand.CommandType __result) => ConvertAction(__instance, ref __result);
        }

        [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.RuntimeActionType), MethodType.Getter)]
        private static class RuntimeActionType
        {
            [HarmonyPriority(Priority.Last), HarmonyAfter("TabletopTweaks-Core")]
            private static void Postfix(AbilityData __instance, ref UnitCommand.CommandType __result) => ConvertAction(__instance, ref __result);
        }

        [HarmonyPatch]
        private static class RememberOriginalCommandCost
        {
            private static MethodBase TargetMethod() => AccessTools.Constructor(typeof(UnitUseAbility),
                new[] { typeof(UnitCommand.CommandType), typeof(AbilityData), typeof(TargetWrapper) });
            private static void Postfix(UnitUseAbility __instance, UnitCommand.CommandType commandType)
            {
                if (ManeuverCompatibility.IsEligible(__instance.Ability)) Ticket(__instance, commandType);
            }
        }

        [HarmonyPatch(typeof(UnitCommand), nameof(UnitCommand.Type), MethodType.Getter)]
        private static class RefreshQueuedCommandCost
        {
            private static void Postfix(UnitCommand __instance, ref UnitCommand.CommandType __result)
            {
                if (!(__instance is UnitUseAbility command)) return;
                if (!Tickets.TryGetValue(command, out var ticket))
                {
                    if (!ManeuverCompatibility.IsEligible(command.Ability)) return;
                    ticket = Ticket(command, __result);
                }
                if (ticket.Locked) { __result = ticket.SelectedType; return; }
                __result = ticket.NormalType;
                if (__result != UnitCommand.CommandType.Free && BudgetFor(command.Ability)?.CanReserve == true)
                    __result = UnitCommand.CommandType.Free;
                // UnitCommands indexes active slots by Type. Once inserted, reserve and freeze the type
                // until this command ends; queued/UI-only commands never reserve or spend anything.
                if (command.Executor?.Commands.Raw.Any(active => ReferenceEquals(active, command)) == true)
                    Lock(ticket, __result);
            }
        }

        [HarmonyPatch(typeof(UnitUseAbility), "OnAction")]
        private static class ExecutionScope
        {
            private static void Prefix(UnitUseAbility __instance, out UnitUseAbility __state)
            {
                __state = Executing;
                var selected = __instance.Type; // also restores tracking for externally reconstructed commands
                if (Tickets.TryGetValue(__instance, out var ticket) && !ticket.Locked)
                    Lock(ticket, selected);
                Executing = __instance;
            }

            private static void Finalizer(UnitUseAbility __instance, UnitUseAbility __state)
            {
                if (Tickets.TryGetValue(__instance, out var ticket) && !ticket.Committed) ticket.Budget?.Release(ticket);
                Executing = __state;
            }
        }

        // RuleCastSpell reaches AbilityData.Cast only after availability/target/cancellation checks.
        // Spend here, once per command, before success or failure of the actual maneuver is known.
        [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.Cast))]
        private static class SpendExecutedAttempt
        {
            private static void Prefix(AbilityData __instance)
            {
                if (Executing?.Ability == __instance && Tickets.TryGetValue(Executing, out var ticket) && !ticket.Committed)
                    ticket.Budget?.Commit(ticket);
            }
        }

        [HarmonyPatch(typeof(UnitUseAbility), "OnEnded")]
        private static class ReleaseCanceledAttempt
        {
            private static void Prefix(UnitUseAbility __instance)
            {
                if (Tickets.TryGetValue(__instance, out var ticket)) ticket.Budget?.Release(ticket);
            }
        }

        [HarmonyPatch(typeof(UnitCombatState), nameof(UnitCombatState.JoinCombat))]
        private static class NewEncounter
        {
            private static void Prefix(UnitCombatState __instance, out bool __state) => __state = __instance.IsInCombat;
            private static void Postfix(UnitCombatState __instance, bool __state)
            {
                var budget = __instance.Unit.Get<UnitPartManeuverTempo>();
                // OnPostLoad rebuilds UnitCombatState with IsInCombat=false. The persisted marker
                // distinguishes rejoining that saved encounter from starting a genuinely new fight.
                if (!__state && __instance.IsInCombat && budget != null && !budget.InEncounter) budget.BeginEncounter();
            }
        }

        [HarmonyPatch(typeof(UnitCombatState), nameof(UnitCombatState.LeaveCombat))]
        private static class EndEncounter
        {
            private static void Prefix(UnitCombatState __instance, out bool __state) => __state = __instance.IsInCombat;
            private static void Postfix(UnitCombatState __instance, bool __state)
            {
                if (__state && !__instance.IsInCombat) __instance.Unit.Get<UnitPartManeuverTempo>()?.LeaveEncounter();
            }
        }

        // Realtime: UnitTicksController's six-second unit timer. Turn based: TurnController.Prepare
        // calls this exactly once for the acting unit (and mount). It does not advance on UI queries.
        [HarmonyPatch(typeof(UnitTicksController), nameof(UnitTicksController.TickNextRound))]
        private static class NewRound
        {
            private static void Prefix(UnitEntityData unit)
            {
                if (unit.IsInCombat) unit.Get<UnitPartManeuverTempo>()?.NewRound();
            }
        }
    }
}
