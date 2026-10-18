using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.ElementsSystem;
using System;
using System.Reflection;
using HarmonyLib;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Commands;
using ACHomebrew.Feats;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Newtonsoft.Json;
using Kingmaker.UnitLogic;
using System.Linq;
using System.Runtime.Serialization;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Units;
using TurnBased.Controllers;
using WotR.Testing.Offline;
using Xunit;

namespace ACHomebrew.OfflineTests
{
    [Collection(ACHomebrewGameCollection.Name)]
    public sealed class ManeuverTempoTests
    {
        private readonly ACHomebrewGame fixture;
        public ManeuverTempoTests(ACHomebrewGame fixture) => this.fixture = fixture;

        [Fact]
        public void SeizeOpeningRegistersItsActionBudgetComponent()
        {
            fixture.RequireGame();
            var feat = OfflineGame.Blueprint<BlueprintFeature>("f35f427c-c793-4f79-9aa0-61b1c088a2df");
            Assert.NotNull(feat);
            Assert.Contains(feat.ComponentsArray, c => c.GetType().Name == "ManeuverTempo");
        }

        [Fact]
        public void PhysicalManeuverIsFreeOnlyForItsOwnerInCombat()
        {
            fixture.RequireGame();
            var unit = OfflineGame.CreateUnit(MainAttributeFeatTests.VanillaUnit);
            var other = OfflineGame.CreateUnit(MainAttributeFeatTests.VanillaUnit);
            var blueprint = NewManeuver();
            var ability = new AbilityData(blueprint, unit);
            var unrelated = new AbilityData(blueprint, other);
            Assert.Equal(UnitCommand.CommandType.Standard, ability.ActionType);
            unit.Progression.Features.AddFeature(OfflineGame.Blueprint<BlueprintFeature>("f35f427c-c793-4f79-9aa0-61b1c088a2df"));
            Assert.Equal(UnitCommand.CommandType.Standard, ability.ActionType);
            unit.CombatState.JoinCombat(true);
            Assert.Equal(UnitCommand.CommandType.Free, ability.ActionType);
            Assert.Equal(UnitCommand.CommandType.Free, ability.RuntimeActionType);
            other.CombatState.JoinCombat(true);
            Assert.Equal(UnitCommand.CommandType.Standard, unrelated.ActionType);
        }

        [Fact]
        public void QueuedCommandsShareTwoFreeAttemptsAndThreeEncounterBursts()
        {
            fixture.RequireGame();
            var unit = OwnerInCombat();
            var first = new AbilityData(NewManeuver(), unit);
            var second = new AbilityData(NewManeuver(), unit);
            var queued = new[] { Command(first, unit), Command(second, unit), Command(first, unit) };
            Assert.All(queued, command => Assert.Equal(UnitCommand.CommandType.Free, command.Type));
            for (var i = 0; i < 20; ++i) Assert.Equal(UnitCommand.CommandType.Free, first.RuntimeActionType);
            Execute(queued[0]);
            Execute(queued[1]);
            Assert.Equal(UnitCommand.CommandType.Free, queued[1].Type); // spending cannot change its cooldown
            Assert.Equal(UnitCommand.CommandType.Standard, queued[2].Type);
            Execute(queued[2]); // ordinary use remains available
            for (var round = 0; round < 2; ++round)
            {
                Game.Instance.UnitTicksController.TickNextRound(unit);
                Execute(Command(first, unit));
                Assert.Equal(UnitCommand.CommandType.Free, second.RuntimeActionType);
                Execute(Command(second, unit));
            }
            Game.Instance.UnitTicksController.TickNextRound(unit);
            Execute(Command(first, unit));
            Assert.Equal(UnitCommand.CommandType.Standard, second.RuntimeActionType);
        }

        [Fact]
        public void InvalidTargetAndUnexecutedCommandsDoNotSpend()
        {
            fixture.RequireGame();
            var unit = OwnerInCombat();
            var blueprint = NewManeuver();
            var ability = new AbilityData(blueprint, unit);
            var canceled = Command(ability, unit);
            // A command that is discarded before OnAction has no execution event.
            Assert.Equal(UnitCommand.CommandType.Free, canceled.Type);
            var invalid = Command(ability, unit);
            blueprint.CanTargetSelf = false;
            Assert.Equal(UnitCommand.ResultType.Fail, Execute(invalid));
            blueprint.CanTargetSelf = true;
            Execute(Command(ability, unit));
            Assert.Equal(UnitCommand.CommandType.Free, ability.RuntimeActionType);
            Execute(Command(ability, unit));
            Assert.Equal(UnitCommand.CommandType.Standard, ability.RuntimeActionType);
        }

        [Fact]
        public void CompatibilityRecognizesConditionalManeuversButRequiresAdaptersForComplexAbilities()
        {
            fixture.RequireGame();
            var unit = OfflineGame.CreateUnit(MainAttributeFeatTests.VanillaUnit);
            var conditional = NewManeuver();
            conditional.GetComponent<AbilityEffectRunAction>().Actions = new ActionList
            {
                Actions = new GameAction[] { new Conditional { IfTrue = new ActionList { Actions = new GameAction[] { new ContextActionCombatManeuver() } }, IfFalse = new ActionList() } },
            };
            Assert.True(ManeuverCompatibility.IsEligible(new AbilityData(conditional, unit)));
            var complex = NewManeuver();
            complex.GetComponent<AbilityEffectRunAction>().Actions = new ActionList
            {
                Actions = new GameAction[] { new ContextActionCombatManeuver(), new ContextActionDealDamage() },
            };
            Assert.False(ManeuverCompatibility.IsEligible(new AbilityData(complex, unit)));
            var opaque = NewManeuver();
            opaque.ComponentsArray = Array.Empty<BlueprintComponent>();
            Assert.False(ManeuverCompatibility.IsEligible(new AbilityData(opaque, unit)));
            ManeuverCompatibility.Register(opaque);
            Assert.True(ManeuverCompatibility.IsEligible(new AbilityData(opaque, unit)));
            opaque.Type = AbilityType.Spell;
            Assert.False(ManeuverCompatibility.IsEligible(new AbilityData(opaque, unit)));
            opaque.Type = AbilityType.SpellLike;
            Assert.False(ManeuverCompatibility.IsEligible(new AbilityData(opaque, unit)));
        }

        [Fact]
        public void CompatibilityExtendsOnlyRecognizedTargetDebuffsOrExplicitEffects()
        {
            fixture.RequireGame();
            var blueprint = NewManeuver();
            var debuff = new BlueprintBuff { name = "OfflineManeuverDebuff", AssetGuid = BlueprintGuid.Parse(Guid.NewGuid().ToString("N")) };
            ResourcesLibrary.BlueprintsCache.AddCachedBlueprint(debuff.AssetGuid, debuff);
            var flags = AccessTools.Field(typeof(BlueprintBuff), "m_Flags");
            flags.SetValue(debuff, Enum.Parse(flags.FieldType, "Harmful"));
            var success = new ContextActionApplyBuff();
            AccessTools.Field(typeof(ContextActionApplyBuff), "m_Buff").SetValue(success, debuff.ToReference<BlueprintBuffReference>());
            ((ContextActionCombatManeuverCustom)blueprint.GetComponent<AbilityEffectRunAction>().Actions.Actions[0]).Success = new ActionList { Actions = new GameAction[] { success } };
            Assert.True(ManeuverCompatibility.CanExtendEffect(blueprint, debuff));
            success.ToCaster = true;
            Assert.False(ManeuverCompatibility.CanExtendEffect(blueprint, debuff));
            var explicitEffect = new BlueprintBuff { name = "OfflineAdapterEffect", AssetGuid = BlueprintGuid.Parse(Guid.NewGuid().ToString("N")) };
            ManeuverCompatibility.Register(blueprint, explicitEffect);
            Assert.True(ManeuverCompatibility.CanExtendEffect(blueprint, explicitEffect));
        }

        [Theory]
        [InlineData("6fd05c4ecfebd6f4d873325de442fc17")]
        [InlineData("7ab6f70c996fe9b4597b8332f0a3af5f")]
        [InlineData("45d94c6db453cfc4a9b99b72d6afe6f6")]
        [InlineData("4921b86ee42c0b54e87a2f9b20521ab9")]
        public void RecognizesVanillaStandaloneManeuvers(string guid)
        {
            fixture.RequireGame();
            var unit = OwnerInCombat();
            var blueprint = OfflineGame.Blueprint<BlueprintAbility>(guid);
            var ability = new AbilityData(blueprint, unit);
            Assert.True(ManeuverCompatibility.IsEligible(ability), blueprint.name);
            Assert.Equal(UnitCommand.CommandType.Free, ability.RuntimeActionType);
            fixture.Observations["maneuver.native." + blueprint.name] = new { type = blueprint.Type.ToString(), action = ability.RuntimeActionType.ToString() };
        }

        [Fact]
        public void BudgetSurvivesGameSerializerAndFeatRemovalUntilRealCombatTransition()
        {
            fixture.RequireGame();
            var unit = OwnerInCombat();
            var ability = new AbilityData(NewManeuver(), unit);
            Execute(Command(ability, unit));
            Execute(Command(ability, unit));
            var before = unit.Get<UnitPartManeuverTempo>();
            var saved = JsonConvert.SerializeObject(before, DefaultJsonSettings.DefaultSettings);
            // UnitEntityData.OnPostLoad reconstructs CombatState, whose m_InCombat is not serialized.
            // Load the saved part onto that fresh state before the engine rejoins the ongoing fight.
            unit = OfflineGame.CreateUnit(MainAttributeFeatTests.VanillaUnit);
            unit.Progression.Features.AddFeature(OfflineGame.Blueprint<BlueprintFeature>("f35f427c-c793-4f79-9aa0-61b1c088a2df"));
            var restored = unit.Get<UnitPartManeuverTempo>();
            JsonConvert.PopulateObject(saved, restored, DefaultJsonSettings.DefaultSettings);
            unit.CombatState.LeaveCombat(); // redundant cleanup on a newly created, inactive combat state
            unit.CombatState.JoinCombat(true);
            ability = new AbilityData(NewManeuver(), unit);
            Assert.Equal(2, restored.FreeAttemptsThisRound);
            Assert.Equal(1, restored.BurstUsesSpent);
            Assert.Equal(UnitCommand.CommandType.Standard, ability.ActionType);
            var feat = OfflineGame.Blueprint<BlueprintFeature>("f35f427c-c793-4f79-9aa0-61b1c088a2df");
            UnitHelper.RemoveFact(unit, unit.Facts.List.First(f => f.Blueprint == feat));
            unit.Progression.Features.AddFeature(feat);
            Assert.Same(restored, unit.Get<UnitPartManeuverTempo>());
            unit.CombatState.JoinCombat(true); // already in combat, not a new encounter
            Assert.Equal(UnitCommand.CommandType.Standard, ability.ActionType);
            unit.CombatState.LeaveCombat();
            Assert.Equal(UnitCommand.CommandType.Standard, ability.ActionType);
            unit.CombatState.JoinCombat(true);
            Assert.Equal(0, restored.BurstUsesSpent);
            Assert.Equal(UnitCommand.CommandType.Free, ability.ActionType);
            fixture.Observations["maneuver.serializedBudget"] = saved;
        }

        [Fact]
        public void ActiveSlotKeepsItsTypeAndReleasesReservationWhenCanceled()
        {
            fixture.RequireGame();
            var unit = OwnerInCombat();
            var ability = new AbilityData(NewManeuver(), unit);
            Execute(Command(ability, unit));
            var active = Command(ability, unit);
            unit.Commands.Raw[(int)UnitCommand.CommandType.Free] = active;
            Assert.Equal(UnitCommand.CommandType.Free, active.Type);
            Assert.True(unit.Commands.Contains(active));
            Assert.Equal(UnitCommand.CommandType.Standard, ability.RuntimeActionType);
            Assert.Equal(1, unit.Get<UnitPartManeuverTempo>().FreeAttemptsThisRound);
            active.Interrupt(false);
            unit.Commands.Raw[(int)UnitCommand.CommandType.Free] = null;
            Assert.Equal(UnitCommand.CommandType.Free, ability.RuntimeActionType);
            Assert.Equal(0, unit.Get<UnitPartManeuverTempo>().BurstUsesSpent);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ReservedLastFreeFullRoundCommandPassesCooldownAfterMovementOrAttack(bool spentStandard)
        {
            fixture.RequireGame();
            var unit = OwnerInCombat();
            var blueprint = NewManeuver();
            blueprint.SetIsFullRoundAction(true);
            var ability = new AbilityData(blueprint, unit);
            Execute(Command(ability, unit));
            var active = Command(ability, unit);
            unit.Commands.Raw[(int)UnitCommand.CommandType.Free] = active;
            Assert.Equal(UnitCommand.CommandType.Free, active.Type);
            using (new ManeuverTurnBasedTestScope(unit))
            {
                Assert.True(CombatController.IsInTurnBasedCombat());
                unit.SpendAction(spentStandard ? UnitCommand.CommandType.Standard : UnitCommand.CommandType.Move, false, 0f);
                Assert.False(unit.HasFullRoundAction());
                Assert.True(ability.RequireFullRoundAction); // ordinary UI/new command cost
                Assert.False(unit.CombatState.HasCooldownForCommand(active));
                Assert.False(active.IsFullRoundAbility());
                var normal = Command(ability, unit);
                Assert.Equal(UnitCommand.CommandType.Standard, normal.Type);
                Assert.True(normal.IsFullRoundAbility());
                Assert.True(unit.CombatState.HasCooldownForCommand(normal));
                Assert.True(ability.RequireFullRoundAction); // the command check must not leak
                Assert.True(new AbilityData(blueprint, unit).RequireFullRoundAction);
                Assert.Equal(1, unit.Get<UnitPartManeuverTempo>().FreeAttemptsThisRound);
            }
        }

        [Fact]
        public void ReservedLastFreeFullRoundCommandPassesNativeStaggeredPrecheckOnlyForThatCommand()
        {
            fixture.RequireGame();
            var unit = OwnerInCombat();
            var blueprint = NewManeuver();
            blueprint.SetIsFullRoundAction(true);
            var ability = new AbilityData(blueprint, unit);
            Execute(Command(ability, unit));
            // A distant point keeps this precheck before movement/animation, which need a scene view.
            var target = new Kingmaker.Utility.TargetWrapper(unit.Position + new UnityEngine.Vector3(100f, 0f, 0f));
            var active = Command(ability, unit, target);
            active.NeedLoS = false;
            unit.Commands.Raw[(int)UnitCommand.CommandType.Free] = active;
            Assert.Equal(UnitCommand.CommandType.Free, active.Type);
            using (new ManeuverTurnBasedTestScope(unit))
            {
                unit.State.AddCondition(UnitCondition.Staggered);
                Assert.False(ability.IsAvailableForCast);
                var controller = new UnitCommandController();
                var precheck = AccessTools.Method(typeof(UnitCommandController), "TickCommandTurnBased");
                Assert.True((bool)precheck.Invoke(controller, new object[] { active }));
                Assert.Equal(UnitCommand.ResultType.None, active.Result);
                Assert.False(ability.IsAvailableForCast);
                var normal = Command(ability, unit, target);
                normal.NeedLoS = false;
                Assert.Equal(UnitCommand.CommandType.Standard, normal.Type);
                Assert.False((bool)precheck.Invoke(controller, new object[] { normal }));
                Assert.Equal(UnitCommand.ResultType.Interrupt, normal.Result);
                Assert.False(ability.IsAvailableForCast);
                Assert.Equal(1, unit.Get<UnitPartManeuverTempo>().FreeAttemptsThisRound);
                Assert.Equal(0, unit.Get<UnitPartManeuverTempo>().BurstUsesSpent);
                var separateAbility = new AbilityData(blueprint, unit);
                Assert.False(separateAbility.IsAvailableForCast);
                Assert.Equal(UnitCommand.CommandType.Standard, Command(separateAbility, unit).Type);
            }
        }

        [Fact]
        public void CommandReconstructedWithoutConstructorStillRevalidatesItsFreeSnapshot()
        {
            fixture.RequireGame();
            var unit = OwnerInCombat();
            var ability = new AbilityData(NewManeuver(), unit);
            var reconstructed = (UnitUseAbility)FormatterServices.GetUninitializedObject(typeof(UnitUseAbility));
            AccessTools.Property(typeof(UnitUseAbility), nameof(UnitUseAbility.Ability)).SetValue(reconstructed, ability, null);
            AccessTools.Property(typeof(UnitCommand), nameof(UnitCommand.Executor)).SetValue(reconstructed, unit, null);
            AccessTools.Property(typeof(UnitCommand), nameof(UnitCommand.Type)).SetValue(reconstructed, UnitCommand.CommandType.Free, null);
            Execute(Command(ability, unit));
            Execute(Command(ability, unit));
            Assert.Equal(UnitCommand.CommandType.Standard, reconstructed.Type);
        }

        [Fact]
        public void CustomOverridesRequireAnExplicitCompatibilityAdapter()
        {
            fixture.RequireGame();
            var unit = OwnerInCombat();
            var maneuver = NewManeuver();
            maneuver.GetComponent<AbilityEffectRunAction>().Actions.Actions = new GameAction[] { new CustomManeuverAction() };
            Assert.False(ManeuverCompatibility.IsEligible(new AbilityData(maneuver, unit)));
            var conditional = NewManeuver();
            conditional.GetComponent<AbilityEffectRunAction>().Actions.Actions = new GameAction[]
            {
                new CustomConditional { IfTrue = new ActionList { Actions = new GameAction[] { new ContextActionCombatManeuverCustom() } } },
            };
            Assert.False(ManeuverCompatibility.IsEligible(new AbilityData(conditional, unit)));
            var effect = NewManeuver();
            effect.ComponentsArray = new BlueprintComponent[] { new CustomRunAction { Actions = effect.GetComponent<AbilityEffectRunAction>().Actions } };
            Assert.False(ManeuverCompatibility.IsEligible(new AbilityData(effect, unit)));
            var success = NewManeuver();
            ((ContextActionCombatManeuverCustom)success.GetComponent<AbilityEffectRunAction>().Actions.Actions[0]).Success = new ActionList { Actions = new GameAction[] { new CustomApplyBuff() } };
            Assert.False(ManeuverCompatibility.IsEligible(new AbilityData(success, unit)));
            ManeuverCompatibility.Register(maneuver);
            Assert.True(ManeuverCompatibility.IsEligible(new AbilityData(maneuver, unit)));
        }

        [Fact]
        public void NormalActionOverridesAndResourceCostsArePreserved()
        {
            fixture.RequireGame();
            var unit = OwnerInCombat();
            var blueprint = NewManeuver();
            var resource = new BlueprintAbilityResource { name = "OfflineManeuverResource", AssetGuid = BlueprintGuid.Parse(Guid.NewGuid().ToString("N")) };
            var maxAmount = AccessTools.Field(typeof(BlueprintAbilityResource), "m_MaxAmount");
            var amount = Activator.CreateInstance(maxAmount.FieldType);
            AccessTools.Field(maxAmount.FieldType, "BaseValue").SetValue(amount, 2);
            maxAmount.SetValue(resource, amount);
            ResourcesLibrary.BlueprintsCache.AddCachedBlueprint(resource.AssetGuid, resource);
            var cost = new AbilityResourceLogic { Amount = 1 };
            AccessTools.Field(typeof(AbilityResourceLogic), "m_RequiredResource").SetValue(cost, resource.ToReference<BlueprintAbilityResourceReference>());
            AccessTools.Field(typeof(AbilityResourceLogic), "m_IsSpendResource").SetValue(cost, true);
            blueprint.ComponentsArray = blueprint.ComponentsArray.Concat(new BlueprintComponent[] { cost }).ToArray();
            unit.Descriptor.Resources.Add(resource, true);
            unit.Ensure<UnitPartAbilityModifiers>().AddEntry(new UnitPartAbilityModifiers.ActionEntry(null, UnitCommand.CommandType.Move, blueprint));
            var ability = new AbilityData(blueprint, unit);
            Assert.Equal(2, unit.Descriptor.Resources.GetResourceAmount(resource));
            Assert.Equal(UnitCommand.ResultType.Success, Execute(Command(ability, unit)));
            Assert.Equal(1, unit.Descriptor.Resources.GetResourceAmount(resource));
            Assert.Equal(UnitCommand.ResultType.Success, Execute(Command(ability, unit)));
            Assert.Equal(0, unit.Descriptor.Resources.GetResourceAmount(resource));
            Assert.Equal(UnitCommand.CommandType.Move, ability.RuntimeActionType);
            Assert.False(ability.IsAvailable);
            Assert.Equal(UnitCommand.ResultType.Fail, Execute(Command(ability, unit)));
            Assert.Equal(2, unit.Get<UnitPartManeuverTempo>().FreeAttemptsThisRound);
        }

        [Fact]
        public void AnActualFailedManeuverRollStillSpendsTheAttempt()
        {
            fixture.RequireGame();
            var unit = OwnerInCombat();
            var blueprint = NewManeuver();
            ((ContextActionCombatManeuverCustom)blueprint.GetComponent<AbilityEffectRunAction>().Actions.Actions[0]).Type = CombatManeuver.DirtyTrickSickened;
            var command = Command(new AbilityData(blueprint, unit), unit);
            var forceFailure = new FailManeuver();
            EventBus.Subscribe(forceFailure);
            try
            {
                Assert.Equal(UnitCommand.ResultType.Success, Execute(command));
                AbilityExecutionProcess.ApplyEffectImmediate(command.ExecutionProcess.Context, unit);
                Assert.NotNull(forceFailure.Result);
                Assert.False(forceFailure.Result.Success);
                Assert.Equal(1, unit.Get<UnitPartManeuverTempo>().FreeAttemptsThisRound);
                Assert.Equal(0, unit.Get<UnitPartManeuverTempo>().BurstUsesSpent);
            }
            finally { EventBus.Unsubscribe(forceFailure); }
        }

        [Fact]
        public void FreeCommandCostsPreserveExistingGameCooldownsAfterTheirBudgetIsSpent()
        {
            fixture.RequireGame();
            var unit = OwnerInCombat();
            var ability = new AbilityData(NewManeuver(), unit);
            var cooldown = unit.CombatState.Cooldown;
            cooldown.StandardAction = 4.5f;
            cooldown.MoveAction = 1.5f;
            cooldown.SwiftAction = 2f;
            var queued = new[] { Command(ability, unit), Command(ability, unit), Command(ability, unit) };
            for (var i = 0; i < 2; ++i)
            {
                Assert.False(unit.CombatState.HasCooldownForCommand(queued[i]));
                Assert.Equal(UnitCommand.ResultType.Success, Execute(queued[i]));
                // UnitCommandController performs this call after OnAction returns and the budget changed.
                unit.SpendAction(queued[i].Type, ability.RequireFullRoundAction, 0f);
                Assert.Equal(4.5f, cooldown.StandardAction);
                Assert.Equal(1.5f, cooldown.MoveAction);
                Assert.Equal(2f, cooldown.SwiftAction);
            }
            Assert.Equal(UnitCommand.CommandType.Standard, queued[2].Type);
            Assert.True(unit.CombatState.HasCooldownForCommand(queued[2]));
        }

        private UnitEntityData OwnerInCombat()
        {
            var unit = OfflineGame.CreateUnit(MainAttributeFeatTests.VanillaUnit);
            unit.IsInGame = true;
            unit.Progression.Features.AddFeature(OfflineGame.Blueprint<BlueprintFeature>("f35f427c-c793-4f79-9aa0-61b1c088a2df"));
            unit.CombatState.JoinCombat(true);
            return unit;
        }

        private static UnitUseAbility Command(AbilityData ability, UnitEntityData unit, Kingmaker.Utility.TargetWrapper target = null)
        {
            var command = new UnitUseAbility(ability, target ?? unit);
            // Skip Init's view/animation setup. OnAction itself and the complete RuleCastSpell path remain real.
            AccessTools.Property(typeof(UnitCommand), "Executor").SetValue(command, unit, null);
            return command;
        }

        private static UnitCommand.ResultType Execute(UnitUseAbility command)
            => (UnitCommand.ResultType)AccessTools.Method(typeof(UnitUseAbility), "OnAction").Invoke(command, null);

        internal static BlueprintAbility NewManeuver() => new BlueprintAbility
        {
            name = "OfflineIndependentManeuver",
            AssetGuid = BlueprintGuid.Parse(Guid.NewGuid().ToString("N")),
            Type = AbilityType.Physical,
            ActionType = UnitCommand.CommandType.Standard,
            CanTargetSelf = true,
            Range = AbilityRange.Personal,
            ComponentsArray = new Kingmaker.Blueprints.BlueprintComponent[]
            {
                new AbilityEffectRunAction { Actions = new ActionList { Actions = new GameAction[]
                {
                    new ContextActionCombatManeuverCustom
                    {
                        Success = new ActionList { Actions = Array.Empty<GameAction>() },
                        Failure = new ActionList { Actions = Array.Empty<GameAction>() },
                    },
                } } },
            },
        };
    }
}
