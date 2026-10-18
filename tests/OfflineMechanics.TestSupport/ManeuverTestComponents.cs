using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;

namespace ACHomebrew.OfflineTests
{
    // Establish real turn-based state without starting a scene, movement, or UI controllers.
    public sealed class ManeuverTurnBasedTestScope : System.IDisposable
    {
        private readonly bool wasInCombat;
        private readonly bool wasMandatory;
        private readonly bool wasTurnBased;
        private readonly TurnBased.Controllers.TurnController previousTurn;
        private readonly CountableFlag preventDirectControl;

        public ManeuverTurnBasedTestScope(Kingmaker.EntitySystem.Entities.UnitEntityData unit)
        {
            var player = Kingmaker.Game.Instance.Player;
            wasInCombat = player.IsInCombat;
            wasMandatory = player.IsTurnBasedStateMandatory;
            wasTurnBased = player.MandatoryTurnBasedModeState;
            previousTurn = Kingmaker.Game.Instance.TurnBasedCombatController.CurrentTurn;
            HarmonyLib.AccessTools.Property(typeof(Kingmaker.Player), nameof(Kingmaker.Player.IsInCombat)).SetValue(player, true, null);
            player.IsTurnBasedStateMandatory = true;
            player.MandatoryTurnBasedModeState = true;
            var turn = new TurnBased.Controllers.TurnController(unit);
            EventBus.Unsubscribe(turn); // the test invokes the precheck, not UI turn notifications
            HarmonyLib.AccessTools.Property(typeof(TurnBased.Controllers.TurnController), "Status")
                .SetValue(turn, TurnBased.Controllers.TurnController.TurnStatus.Acting, null);
            HarmonyLib.AccessTools.Field(typeof(TurnBased.Controllers.CombatController), "m_CurrentTurn")
                .SetValue(Kingmaker.Game.Instance.TurnBasedCombatController, turn);
            preventDirectControl = unit.PreventDirectControl;
            preventDirectControl.Retain();
        }

        public void Dispose()
        {
            preventDirectControl.Release();
            var player = Kingmaker.Game.Instance.Player;
            HarmonyLib.AccessTools.Field(typeof(TurnBased.Controllers.CombatController), "m_CurrentTurn")
                .SetValue(Kingmaker.Game.Instance.TurnBasedCombatController, previousTurn);
            player.IsTurnBasedStateMandatory = wasMandatory;
            player.MandatoryTurnBasedModeState = wasTurnBased;
            HarmonyLib.AccessTools.Property(typeof(Kingmaker.Player), nameof(Kingmaker.Player.IsInCombat)).SetValue(player, wasInCombat, null);
        }
    }

    // Loaded by test method bodies after fixture.RequireGame installs the game assembly resolver.
    // Keeping game-derived types outside the xUnit assembly permits Mono's discovery reflection.
    public sealed class CustomManeuverAction : ContextActionCombatManeuverCustom { public override void RunAction() { } }
    public sealed class CustomConditional : Conditional { public override void RunAction() { } }
    public sealed class CustomApplyBuff : ContextActionApplyBuff { public override void RunAction() { } }
    public sealed class CustomRunAction : AbilityEffectRunAction
    {
        public override void Apply(AbilityExecutionContext context, TargetWrapper target) { }
    }

    public sealed class FailManeuver : IGlobalRulebookHandler<RuleCombatManeuver>
    {
        public RuleCombatManeuver Result;
        public void OnEventAboutToTrigger(RuleCombatManeuver evt)
        {
            evt.IgnoreConcealment = true;
            evt.OverrideRoll = RuleRollD20.FromInt(evt.Initiator, 1);
            evt.OverrideBonus = -1000;
        }
        public void OnEventDidTrigger(RuleCombatManeuver evt) => Result = evt;
    }

    public sealed class NestedFailedManeuver : IGlobalRulebookHandler<RuleCombatManeuver>
    {
        public Kingmaker.EntitySystem.Entities.UnitEntityData Actor;
        public bool Ran;
        public void OnEventAboutToTrigger(RuleCombatManeuver evt) { }
        public void OnEventDidTrigger(RuleCombatManeuver evt)
        {
            if (Ran || evt.Initiator != Actor) return;
            Ran = true;
            Rulebook.Trigger(new RuleCombatManeuver(evt.Initiator, evt.Target, evt.Type) { AutoFailure = true, Reason = evt.Reason });
        }
    }

    public sealed class ForceManeuverResult : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCombatManeuver>
    {
        public int Margin;
        public void OnEventAboutToTrigger(RuleCombatManeuver evt)
        {
            evt.OverrideRoll = RuleRollD20.FromInt(evt.Initiator, 10);
            evt.OverrideBonus = Rulebook.Trigger(new RuleCalculateCMD(evt.Initiator, evt.Target, evt.Type)).Result - 10 + Margin;
        }
        public void OnEventDidTrigger(RuleCombatManeuver evt) { }
    }
}
