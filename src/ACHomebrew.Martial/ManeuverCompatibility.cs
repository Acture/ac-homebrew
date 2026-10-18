using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace ACHomebrew.Feats
{
    /// <summary>Adapters for independently activated, non-spell combat maneuvers from other mods.</summary>
    public static class ManeuverCompatibility
    {
        private static readonly Dictionary<BlueprintAbility, HashSet<BlueprintBuff>> Registered = new();

        /// <summary>
        /// Register an independent maneuver whose custom action cannot be inspected. The adapter asserts that
        /// this is a maneuver activation, not an attack sequence. Effects are its successful target debuffs.
        /// Spells, spell-like abilities and item activations remain excluded even after registration.
        /// Call once blueprints have been created; registrations are additive.
        /// </summary>
        public static void Register(BlueprintAbility ability, params BlueprintBuff[] effects)
        {
            if (ability == null) throw new ArgumentNullException(nameof(ability));
            if (!Registered.TryGetValue(ability, out var registered))
                Registered.Add(ability, registered = new HashSet<BlueprintBuff>());
            foreach (var effect in effects ?? Array.Empty<BlueprintBuff>())
                if (effect != null) registered.Add(effect);
        }

        public static bool IsEligible(AbilityData ability)
            => ability != null && ability.SourceItem == null && ability.Spellbook == null && IsEligible(ability.Blueprint);

        private static bool IsEligible(BlueprintAbility ability)
        {
            if (ability == null || ability.IsSpell) return false;
            if (Registered.ContainsKey(ability)) return true;
            // Unknown delivery logic can turn a maneuver rider into a whole attack or spell sequence.
            // Such abilities require an explicit adapter; merely finding a nested maneuver is insufficient.
            if (ability.ComponentsArray.Any(c => c is AbilityDeliverEffect || c is AbilityCustomLogic)) return false;
            var effects = ability.ComponentsArray.OfType<AbilityApplyEffect>().ToArray();
            if (effects.Length != 1 || effects[0].GetType() != typeof(AbilityEffectRunAction)) return false;
            var run = (AbilityEffectRunAction)effects[0];
            return CountManeuvers(run.Actions, 0) == 1;
        }

        public static bool CanExtendEffect(BlueprintAbility ability, BlueprintBuff buff)
        {
            if (ability == null || buff == null || !IsEligible(ability)) return false;
            if (Registered.TryGetValue(ability, out var effects) && effects.Contains(buff)) return true;
            return ability.ComponentsArray.OfType<AbilityEffectRunAction>()
                .Where(run => run.GetType() == typeof(AbilityEffectRunAction))
                .Any(run => HasSuccessBuff(run.Actions, buff, false, 0));
        }

        // At most one maneuver can execute on any path. Conditional branches are mutually exclusive;
        // two successive maneuvers or any unrelated top-level action require an adapter.
        private static int CountManeuvers(ActionList list, int depth)
        {
            if (depth > 16) return -1;
            var count = 0;
            foreach (var action in list?.Actions ?? Array.Empty<GameAction>())
            {
                int found;
                switch (action)
                {
                    case ContextActionCombatManeuver maneuver when action.GetType() == typeof(ContextActionCombatManeuver) && SafeEffects(maneuver.OnSuccess, depth + 1):
                        found = 1;
                        break;
                    case ContextActionCombatManeuverCustom custom when action.GetType() == typeof(ContextActionCombatManeuverCustom) && SafeEffects(custom.Success, depth + 1) && SafeEffects(custom.Failure, depth + 1):
                        found = 1;
                        break;
                    case Conditional conditional when action.GetType() == typeof(Conditional):
                        var yes = CountManeuvers(conditional.IfTrue, depth + 1);
                        var no = CountManeuvers(conditional.IfFalse, depth + 1);
                        if (yes < 0 || no < 0) return -1;
                        found = Math.Max(yes, no);
                        break;
                    default:
                        return -1;
                }
                count += found;
                if (count > 1) return -1;
            }
            return count;
        }

        private static bool SafeEffects(ActionList list, int depth)
        {
            if (depth > 16) return false;
            foreach (var action in list?.Actions ?? Array.Empty<GameAction>())
            {
                if (action?.GetType() == typeof(ContextActionApplyBuff) || action?.GetType() == typeof(ContextActionRemoveBuff)) continue;
                if (action?.GetType() == typeof(Conditional) && action is Conditional conditional && SafeEffects(conditional.IfTrue, depth + 1) && SafeEffects(conditional.IfFalse, depth + 1)) continue;
                return false;
            }
            return true;
        }

        private static bool HasSuccessBuff(ActionList list, BlueprintBuff buff, bool success, int depth)
        {
            if (depth > 16) return false;
            foreach (var action in list?.Actions ?? Array.Empty<GameAction>())
            {
                if (success && action?.GetType() == typeof(ContextActionApplyBuff) && action is ContextActionApplyBuff apply && !apply.ToCaster && !apply.Permanent && apply.Buff == buff && buff.Harmful) return true;
                if (action?.GetType() == typeof(ContextActionCombatManeuver) && action is ContextActionCombatManeuver maneuver && HasSuccessBuff(maneuver.OnSuccess, buff, true, depth + 1)) return true;
                if (action?.GetType() == typeof(ContextActionCombatManeuverCustom) && action is ContextActionCombatManeuverCustom custom && HasSuccessBuff(custom.Success, buff, true, depth + 1)) return true;
                if (action?.GetType() == typeof(Conditional) && action is Conditional conditional && (HasSuccessBuff(conditional.IfTrue, buff, success, depth + 1) || HasSuccessBuff(conditional.IfFalse, buff, success, depth + 1))) return true;
            }
            return false;
        }
    }
}
