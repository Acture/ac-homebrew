using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using BlueprintCore.Utils;
using HarmonyLib;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;

namespace ACHomebrew.Feats
{
    // Adjust the fresh application before immunity and stacking. Never multiply a buff's remaining time.
    public static class ManeuverDuration
    {
        [ThreadStatic] private static RuleCombatManeuver CurrentManeuver;
        [ThreadStatic] private static int ApplicationDepth;
        [ThreadStatic] private static EffectScope CurrentEffects;

        /// <summary>
        /// For registered opaque maneuvers: wrap only the immediate application of successful target effects.
        /// Dispose before returning to the game. A source ability alone does not prove a maneuver succeeded.
        /// </summary>
        public static IDisposable SuccessfulEffects(MechanicsContext context, UnitEntityData target)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (target == null) throw new ArgumentNullException(nameof(target));
            return new EffectScope(context, target, null, true);
        }

        private sealed class EffectScope : IDisposable
        {
            internal readonly MechanicsContext Context;
            internal readonly UnitEntityData Target;
            internal readonly CombatManeuver? Type;
            internal bool Successful;
            internal readonly int RuleDepth;
            private readonly EffectScope Previous;
            private bool Disposed;
            internal EffectScope(MechanicsContext context, UnitEntityData target, CombatManeuver? type, bool successful)
            {
                Context = context;
                RuleDepth = Rulebook.CurrentContext.StackSize;
                Target = target;
                Type = type;
                Successful = successful;
                Previous = CurrentEffects;
                CurrentEffects = this;
            }
            public void Dispose()
            {
                if (Disposed) return;
                CurrentEffects = Previous;
                Disposed = true;
            }
        }

        // The rule finishes before Custom.Success / OnSuccess applies its buffs. Keep the action's
        // context and actual outcome until RunAction returns, excluding Failure and later buff ticks.
        [HarmonyPatch]
        private static class CustomActionScope
        {
            private static IEnumerable<MethodBase> TargetMethods() => new[]
            {
                AccessTools.Method(typeof(ContextActionCombatManeuver), nameof(ContextActionCombatManeuver.RunAction)),
                AccessTools.Method(typeof(ContextActionCombatManeuverCustom), nameof(ContextActionCombatManeuverCustom.RunAction)),
            };
            private static void Prefix(object __instance, out EffectScope __state)
            {
                __state = CurrentEffects;
                var data = ContextData<MechanicsContext.Data>.Current;
                var type = __instance is ContextActionCombatManeuver native ? native.Type : ((ContextActionCombatManeuverCustom)__instance).Type;
                new EffectScope(data?.Context, data?.CurrentTarget?.Unit, type, false);
            }
            private static Exception Finalizer(Exception __exception, EffectScope __state)
            {
                CurrentEffects = __state;
                return __exception;
            }
        }

        [HarmonyPatch(typeof(RuleCombatManeuver), nameof(RuleCombatManeuver.OnTrigger))]
        private static class RecordOutcome
        {
            private static void Postfix(RuleCombatManeuver __instance)
            {
                var scope = CurrentEffects;
                if (scope?.Type == __instance.Type && scope.Context?.MaybeCaster == __instance.Initiator && scope.Target == __instance.Target
                    && ReferenceEquals(__instance.Reason.Context, scope.Context) && Rulebook.CurrentContext.StackSize == scope.RuleDepth + 1)
                    scope.Successful = __instance.Success;
            }
        }

        private static bool IsNativeEffect(RuleCombatManeuver rule, BlueprintBuff buff)
        {
            var mechanics = BlueprintRoot.Instance.SystemMechanics;
            return rule.Type switch
            {
                CombatManeuver.Disarm => buff == mechanics.DisarmMainHandBuff || buff == mechanics.DisarmOffHandBuff,
                CombatManeuver.SunderArmor => buff == mechanics.SunderArmorBuff,
                CombatManeuver.DirtyTrickBlind => buff == mechanics.DirtyTrickBlindnessBuff,
                CombatManeuver.DirtyTrickEntangle => buff == mechanics.DirtyTrickEntangledBuff,
                CombatManeuver.DirtyTrickSickened => buff == mechanics.DirtyTrickSickenedBuff,
                _ => false,
            };
        }

        private static TimeSpan? Extend(BuffCollection buffs, BlueprintBuff blueprint, MechanicsContext context, TimeSpan? duration)
        {
            if (!duration.HasValue || duration.Value <= TimeSpan.Zero || duration.Value == TimeSpan.MaxValue)
                return duration;
            if (blueprint.ComponentsArray.OfType<AddCondition>().Any(component => component.Condition == UnitCondition.Prone))
                return duration;
            UnitEntityData actor;
            if (CurrentManeuver != null)
            {
                if (CurrentManeuver.Target != buffs.Owner?.Unit || !IsNativeEffect(CurrentManeuver, blueprint)) return duration;
                actor = CurrentManeuver.Initiator;
            }
            else
            {
                var scope = CurrentEffects;
                if (scope?.Successful != true || !ReferenceEquals(context, scope.Context) || scope.Target != buffs.Owner?.Unit)
                    return duration;
                actor = context?.MaybeCaster;
                if (actor == buffs.Owner?.Unit || !ManeuverCompatibility.CanExtendEffect(context?.SourceAbility, blueprint)) return duration;
            }
            if (actor == null || !actor.Descriptor.HasFact(BlueprintTool.Get<BlueprintFeature>(Guids.ManeuverTactics.LingeringDisruption)))
                return duration;
            var multiplier = Mod.Settings?.ManeuverDurationMultiplier ?? 2f;
            if (float.IsNaN(multiplier) || float.IsInfinity(multiplier)) multiplier = 2f;
            multiplier = Math.Max(1, Math.Min(10, multiplier));
            var extra = Math.Max(0, Math.Min(99, Mod.Settings?.ManeuverMinimumExtraRounds ?? 2));
            var seconds = Math.Max(duration.Value.TotalSeconds * multiplier, duration.Value.TotalSeconds + extra * 6d);
            // Leave extreme durations alone rather than overflowing the game's end-time arithmetic.
            return seconds >= TimeSpan.MaxValue.TotalSeconds / 2 ? duration : TimeSpan.FromSeconds(seconds);
        }

        [HarmonyPatch(typeof(RuleCombatManeuver), "ApplyManeuver")]
        private static class NativeEffectScope
        {
            private static void Prefix(RuleCombatManeuver __instance, out RuleCombatManeuver __state)
            {
                __state = CurrentManeuver;
                CurrentManeuver = __instance;
            }
            private static Exception Finalizer(Exception __exception, RuleCombatManeuver __state)
            {
                CurrentManeuver = __state;
                return __exception;
            }
        }

        private static void Begin(BuffCollection buffs, BlueprintBuff blueprint, MechanicsContext context,
            ref TimeSpan? duration, out int state)
        {
            state = ApplicationDepth++;
            if (state == 0) duration = Extend(buffs, blueprint, context, duration);
        }

        [HarmonyPatch(typeof(BuffCollection), "AddBuff", new[] { typeof(BlueprintBuff), typeof(MechanicsContext), typeof(TimeSpan?) })]
        private static class ContextApplication
        {
            private static void Prefix(BuffCollection __instance, BlueprintBuff blueprint, MechanicsContext parentContext,
                ref TimeSpan? duration, out int __state) => Begin(__instance, blueprint, parentContext, ref duration, out __state);
            private static Exception Finalizer(Exception __exception, int __state)
            {
                ApplicationDepth = __state;
                return __exception;
            }
        }

        [HarmonyPatch(typeof(BuffCollection), "AddBuff", new[] { typeof(BlueprintBuff), typeof(UnitEntityData), typeof(TimeSpan?), typeof(AbilityParams) })]
        private static class CasterApplication
        {
            private static void Prefix(BuffCollection __instance, BlueprintBuff blueprint,
                ref TimeSpan? duration, out int __state) => Begin(__instance, blueprint, null, ref duration, out __state);
            private static Exception Finalizer(Exception __exception, int __state)
            {
                ApplicationDepth = __state;
                return __exception;
            }
        }
    }
}
