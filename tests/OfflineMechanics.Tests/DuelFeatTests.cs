using System;
using System.Linq;
using ACHomebrew.Feats;
using HarmonyLib;
using Kingmaker;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.ElementsSystem;
using Kingmaker.Controllers;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Newtonsoft.Json;
using WotR.Testing.Offline;
using Xunit;

namespace ACHomebrew.OfflineTests
{
    /// <summary>
    /// Oath of the Duel, Endless Challenge, Single Combat and Unbound Duelist on real vanilla units: a human cavalier
    /// pregen and CR2 Dretches of the Mobs faction, which the game itself treats as enemies.
    /// </summary>
    [Collection(ACHomebrewGameCollection.Name)]
    public sealed class DuelFeatTests
    {
        internal const string Dretch = "2d643696b0653b846bf95c62be792e85";
        private const string CavalierChallengeTarget = "4f0218323ad379248b69de8a9501159f";
        private const string MythicFeatSelection = "9ee0f6745f555484299b0a1563b99d81";

        private readonly ACHomebrewGame fixture;
        public DuelFeatTests(ACHomebrewGame fixture) => this.fixture = fixture;

        [Fact]
        public void FamilyRegistersTheAbilityResourcePrerequisitesAndMythicFeat()
        {
            fixture.RequireGame();
            var oath = OfflineGame.Blueprint<BlueprintFeature>(Guids.Duel.Oath);
            var family = OfflineGame.Blueprint<BlueprintFeatureSelection>(Guids.Duel.Selection);
            Assert.True(Lists(family, oath));
            Assert.True(Lists(family, OfflineGame.Blueprint<BlueprintFeature>(Guids.Duel.EndlessChallenge)));
            Assert.True(Lists(family, OfflineGame.Blueprint<BlueprintFeature>(Guids.Duel.SingleCombat)));
            var mythic = OfflineGame.Blueprint<BlueprintFeature>(Guids.Duel.MythicOath);
            Assert.Equal(FeatureGroup.MythicFeat, mythic.Groups.Single(group => group == FeatureGroup.MythicFeat));
            Assert.True(Lists(OfflineGame.Blueprint<BlueprintFeatureSelection>(MythicFeatSelection), mythic));
            Assert.False(Lists(family, mythic));
            foreach (var guid in new[] { Guids.Duel.EndlessChallenge, Guids.Duel.SingleCombat, Guids.Duel.MythicOath })
            {
                var prerequisite = OfflineGame.Blueprint<BlueprintFeature>(guid).GetComponent<Kingmaker.Blueprints.Classes.Prerequisites.PrerequisiteFeature>();
                Assert.Same(oath, prerequisite.Feature);
            }
            var ability = OfflineGame.Blueprint<BlueprintAbility>(Guids.Duel.Ability);
            Assert.Equal(AbilityType.Supernatural, ability.Type);
            Assert.Equal(UnitCommand.CommandType.Standard, ability.ActionType);
            Assert.Equal(AbilityRange.Close, ability.Range);
            Assert.True(DuelRules.IsChallengeEffect(OfflineGame.Blueprint<BlueprintBuff>(CavalierChallengeTarget)));
            Assert.Single(OfflineGame.Blueprint<BlueprintBuff>(CavalierChallengeTarget).ComponentsArray.OfType<ChallengeDuelTracker>());
        }

        [Fact]
        public void DailyUsesAreOnePlusOnePerFiveLevelsAndEndlessChallengeAddsTwo()
        {
            fixture.RequireGame();
            var resource = OfflineGame.Blueprint<BlueprintAbilityResource>(Guids.Duel.Resource);
            var amount = AccessTools.Field(typeof(BlueprintAbilityResource), "m_MaxAmount").GetValue(resource);
            T Field<T>(string name) => (T)AccessTools.Field(amount.GetType(), name).GetValue(amount);
            Assert.Equal(1, Field<int>("BaseValue"));
            Assert.True(Field<bool>("IncreasedByLevelStartPlusDivStep"));
            Assert.Equal(0, Field<int>("StartingLevel"));
            Assert.Equal(0, Field<int>("StartingIncrease"));
            Assert.Equal(5, Field<int>("LevelStep"));
            Assert.Equal(1, Field<int>("PerStepIncrease"));
            Assert.Equal(1f, Field<float>("OtherClassesModifier"));

            var hero = Hero();
            var level = hero.Descriptor.Progression.CharacterLevel;
            Assert.Equal(1 + level / 5, resource.GetMaxAmount(hero.Descriptor));
            Assert.Equal(1 + level / 5, hero.Descriptor.Resources.GetResourceAmount(resource));
            hero.Progression.Features.AddFeature(OfflineGame.Blueprint<BlueprintFeature>(Guids.Duel.EndlessChallenge));
            Assert.Equal(3 + level / 5, resource.GetMaxAmount(hero.Descriptor));
            fixture.Observations["duel.uses"] = new { level, max = resource.GetMaxAmount(hero.Descriptor) };
        }

        [Fact]
        public void TheAbilitySpendsAUseLinksBothDuelistsAndIsOncePerEncounter()
        {
            fixture.RequireGame();
            var hero = Hero();
            var dretch = Enemy();
            dretch.IsInGame = true; // UnitUseAbility rejects targets that are not in the game
            var resource = OfflineGame.Blueprint<BlueprintAbilityResource>(Guids.Duel.Resource);
            hero.CombatState.JoinCombat(true);
            var ability = new AbilityData(OfflineGame.Blueprint<BlueprintAbility>(Guids.Duel.Ability), hero);
            Assert.False(ability.CanTarget(new TargetWrapper(Enemy(ally: true, of: hero))));
            Assert.False(ability.CanTarget(new TargetWrapper(hero)));
            Assert.True(ability.IsAvailable);
            var before = hero.Descriptor.Resources.GetResourceAmount(resource);
            var command = Cast(ability, hero, dretch);
            fixture.Observations["duel.cast"] = new
            {
                canTarget = ability.CanTarget(new TargetWrapper(dretch)),
                available = ability.IsAvailableForCast,
                distance = hero.DistanceTo(dretch),
            };
            Assert.Equal(UnitCommand.ResultType.Success, Execute(command));
            AbilityExecutionProcess.ApplyEffectImmediate(command.ExecutionProcess.Context, dretch);
            Assert.Equal(before - 1, hero.Descriptor.Resources.GetResourceAmount(resource));
            Assert.Same(dretch, DuelRules.DuelOpponent(hero));
            Assert.Same(hero, DuelRules.DuelOpponent(dretch));
            Assert.False(ability.IsAvailableForCast); // already dueling, and used this encounter
            DuelRules.End(hero);
            Assert.Null(DuelRules.DuelOpponent(dretch));
            Assert.False(ability.IsAvailableForCast); // still once per encounter
            Assert.False(ability.CanTarget(new TargetWrapper(Downed(Enemy()))));
            hero.Descriptor.Resources.FullRestoreAll();
            Assert.Equal(before, hero.Descriptor.Resources.GetResourceAmount(resource)); // uses remain; only the encounter limit blocks it
            Assert.False(ability.IsAvailableForCast);
            hero.CombatState.LeaveCombat();
            Assert.False(DuelRules.UsedThisEncounter(hero));
            hero.CombatState.JoinCombat(true);
            Assert.True(ability.IsAvailableForCast);
        }

        [Fact]
        public void DuelistsAreCompelledToAttackEachOtherForTwoRounds()
        {
            fixture.RequireGame();
            var hero = Hero();
            var dretch = Enemy();
            var bystander = Enemy();
            Assert.True(DuelRules.Start(hero, dretch));
            var buff = hero.Buffs.GetBuff(OfflineGame.Blueprint<BlueprintBuff>(Guids.Duel.Buff));
            Assert.Equal(2.Rounds().Seconds, buff.TimeLeft);
            foreach (var (duelist, opponent) in new[] { (hero, dretch), (dretch, hero) })
            {
                Assert.True(duelist.State.HasCondition(UnitCondition.AttackNearest));
                Assert.True(duelist.CanAttack(opponent));
                Assert.False(duelist.CanAttack(bystander));
                var command = CompelledCommand(duelist);
                Assert.Same(opponent, command.TargetUnit);
                Assert.True((bool)duelist.PreventDirectControl);
            }
            Assert.True(bystander.CanAttack(hero)); // others are not restricted
            Assert.False(DuelRules.Start(bystander, hero)); // nobody joins a running duel
            buff.Remove(); // expiry of either side ends the duel for both
            Assert.Null(DuelRules.DuelOpponent(dretch));
            Assert.False(hero.State.HasCondition(UnitCondition.AttackNearest));
            Assert.False(dretch.State.HasCondition(UnitCondition.AttackNearest));
            Assert.Null(hero.Get<UnitPartConfusion>());
            Assert.False((bool)hero.PreventDirectControl);
            Assert.True(hero.CanAttack(bystander));
            Assert.Equal(0, DuelRules.VictoryCount(hero) + DuelRules.VictoryCount(dretch));
        }

        [Fact]
        public void UnboundDuelistKeepsControlButStillOnlyTargetsTheOpponent()
        {
            fixture.RequireGame();
            var hero = Hero(mythic: true);
            var dretch = Enemy();
            var bystander = Enemy();
            Assert.True(DuelRules.Start(hero, dretch));
            Assert.False(hero.State.HasCondition(UnitCondition.AttackNearest));
            Assert.True(dretch.State.HasCondition(UnitCondition.AttackNearest));
            Assert.Same(hero, CompelledCommand(dretch).TargetUnit);
            Assert.False(hero.CanAttack(bystander));
            Assert.True(hero.CanAttack(dretch));
            var area = new AbilityData(OfflineGame.Blueprint<BlueprintAbility>(Guids.Duel.Ability), hero);
            Assert.False(area.CanTarget(new TargetWrapper(bystander)));
            DuelRules.End(dretch);
            Assert.False(dretch.State.HasCondition(UnitCondition.AttackNearest));
        }

        [Fact]
        public void DamageToAnyoneButTheOpponentIsPreventedAndSingleCombatHalvesInterlopers()
        {
            fixture.RequireGame();
            var hero = Hero();
            var dretch = Enemy();
            var bystander = Enemy();
            var interloper = Enemy();
            hero.Progression.Features.AddFeature(OfflineGame.Blueprint<BlueprintFeature>(Guids.Duel.SingleCombat));
            var fullFromInterloper = Hit(interloper, hero, 10);
            var halfReference = Hit(interloper, hero, 5);
            Assert.Equal(10, Hit(hero, bystander, 10));
            Assert.True(DuelRules.Start(hero, dretch));
            Assert.Equal(0, Hit(hero, bystander, 10));
            Assert.Equal(0, Hit(dretch, bystander, 10));
            Assert.Equal(6, Hit(hero, dretch, 6));
            Assert.Equal(halfReference, Hit(interloper, hero, 10));
            Assert.Equal(fullFromInterloper, Hit(dretch, hero, 10));
            Assert.Equal(fullFromInterloper, Hit(interloper, dretch, 10)); // only Single Combat's owner is protected
            DuelRules.End(hero);
            Assert.Equal(fullFromInterloper, Hit(interloper, hero, 10));
            Assert.Equal(10, Hit(hero, bystander, 10));
            fixture.Observations["duel.damage"] = new { fullFromInterloper, halfReference };
        }

        [Fact]
        public void TheSurvivorWinsOncePermanentDamageAndTheDuelEnds()
        {
            fixture.RequireGame();
            var hero = Hero();
            var dretch = Enemy();
            var damageBefore = hero.Stats.AdditionalDamage.ModifiedValue;
            Assert.True(DuelRules.Start(hero, dretch));
            Fall(dretch);
            Assert.Equal(1, DuelRules.VictoryCount(hero));
            Assert.Equal(damageBefore + 1, hero.Stats.AdditionalDamage.ModifiedValue);
            Assert.Null(DuelRules.DuelOpponent(hero));
            Assert.False(hero.State.HasCondition(UnitCondition.AttackNearest));
            RaiseLifeState(dretch, UnitLifeState.Conscious); // a repeated event for the same fall settles nothing
            Assert.Equal(1, DuelRules.VictoryCount(hero));

            var second = Enemy();
            Assert.True(DuelRules.Start(hero, second));
            Fall(second);
            Assert.Equal(2, DuelRules.VictoryCount(hero));
            Assert.Equal(damageBefore + 2, hero.Stats.AdditionalDamage.ModifiedValue);
            var victories = hero.Progression.Features.GetFact(OfflineGame.Blueprint<BlueprintFeature>(Guids.Duel.Victories));
            Assert.True(victories.Blueprint.IsClassFeature);
            // Respec keeps only source-less non-class features (UnitHelper.IsFeatureSurvivesRespec): victories are cleared.
            Assert.False((bool)AccessTools.Method(typeof(UnitHelper), "IsFeatureSurvivesRespec").Invoke(null, new object[] { victories }));
            var saved = JsonConvert.SerializeObject(victories, DefaultJsonSettings.DefaultSettings);
            Assert.Contains("\"Count\":2", saved.Replace(" ", ""));
            fixture.Observations["duel.victories.saved"] = saved.Length;
        }

        [Fact]
        public void TheOpponentWinsWhenTheDuelistFallsAndExpiredOrSimultaneousDuelsPayNothing()
        {
            fixture.RequireGame();
            var hero = Hero();
            var dretch = Enemy();
            var damageBefore = dretch.Stats.AdditionalDamage.ModifiedValue;
            Assert.True(DuelRules.Start(hero, dretch));
            Fall(hero);
            Assert.Equal(1, DuelRules.VictoryCount(dretch));
            Assert.Equal(damageBefore + 1, dretch.Stats.AdditionalDamage.ModifiedValue);
            Assert.Equal(0, DuelRules.VictoryCount(hero));

            var expired = Enemy();
            var survivor = Hero();
            Assert.True(DuelRules.Start(survivor, expired));
            DuelRules.End(expired);
            Fall(expired);
            Assert.Equal(0, DuelRules.VictoryCount(survivor));

            var together = Enemy();
            var doomed = Hero();
            Assert.True(DuelRules.Start(doomed, together));
            doomed.Damage = doomed.MaxHP + 1; // both drop in the same exchange: the first fall settles with no survivor
            Fall(together);
            Assert.Equal(0, DuelRules.VictoryCount(doomed));
            Assert.Equal(0, DuelRules.VictoryCount(together));
        }

        [Fact]
        public void AChallengeCountsAsADuelOnceWithoutSpendingDuelUses()
        {
            fixture.RequireGame();
            var challenge = OfflineGame.Blueprint<BlueprintBuff>(CavalierChallengeTarget);
            var resource = OfflineGame.Blueprint<BlueprintAbilityResource>(Guids.Duel.Resource);
            var hero = Hero();
            hero.Progression.Features.AddFeature(OfflineGame.Blueprint<BlueprintFeature>(Guids.Duel.SingleCombat));
            var uses = hero.Descriptor.Resources.GetResourceAmount(resource);
            var dretch = Enemy();
            var interloper = Enemy();
            var halfReference = Hit(interloper, hero, 5);
            dretch.AddBuff(challenge, hero);
            Assert.Contains(dretch, DuelRules.Opponents(hero));
            Assert.Contains(hero, DuelRules.Opponents(dretch));
            Assert.False(hero.State.HasCondition(UnitCondition.AttackNearest)); // challenges do not compel
            Assert.True(hero.CanAttack(interloper));
            Assert.Equal(halfReference, Hit(interloper, hero, 10)); // Single Combat applies to a challenge duel
            Fall(dretch);
            Assert.Equal(1, DuelRules.VictoryCount(hero));
            Revive(dretch);
            Fall(dretch); // revived and dropped again while the same challenge lasts
            Assert.Equal(1, DuelRules.VictoryCount(hero)); // the same challenge is settled once
            Assert.Equal(uses, hero.Descriptor.Resources.GetResourceAmount(resource));

            var rival = Enemy();
            rival.AddBuff(challenge, hero);
            Fall(hero); // the challenger falls first: the challenged creature wins
            Assert.Equal(1, DuelRules.VictoryCount(rival));

            var plain = OfflineGame.CreateUnit(MainAttributeFeatTests.VanillaUnit); // no Oath: an ordinary challenge
            var target = Enemy();
            target.AddBuff(challenge, plain);
            Assert.Empty(DuelRules.Opponents(plain));
            Fall(target);
            Assert.Equal(0, DuelRules.VictoryCount(plain));
        }

        // Assert.Contains cannot bind blueprint types without Owlcat.Runtime.Validation, so compare references here.
        private static bool Lists(BlueprintFeatureSelection selection, BlueprintFeature feature) => selection.AllFeatures.Any(f => f == feature);

        internal static UnitEntityData Hero(bool mythic = false)
        {
            var hero = OfflineGame.CreateUnit(MainAttributeFeatTests.VanillaUnit);
            hero.IsInGame = true;
            hero.Progression.Features.AddFeature(OfflineGame.Blueprint<BlueprintFeature>(Guids.Duel.Oath));
            if (mythic) hero.Progression.Features.AddFeature(OfflineGame.Blueprint<BlueprintFeature>(Guids.Duel.MythicOath));
            return hero;
        }

        internal static UnitEntityData Enemy(bool ally = false, UnitEntityData of = null)
        {
            var unit = OfflineGame.CreateUnit(ally ? MainAttributeFeatTests.VanillaUnit : Dretch);
            Assert.Equal(!ally, of == null || of.IsEnemy(unit));
            return unit;
        }

        private static UnitEntityData Downed(UnitEntityData unit)
        {
            Fall(unit);
            return unit;
        }

        internal static int Hit(UnitEntityData source, UnitEntityData target, int amount)
        {
            var hp = target.Damage;
            var rule = Deal(new RuleDealDamage(source, target, new DirectDamage(new DiceFormula(0, DiceType.Zero), amount)));
            target.Damage = hp; // keep the target standing for the next comparison
            return rule.Result;
        }

        /// <summary>
        /// Resolves a damage rule. Its "green NPC" check compares the target with the main character, which an offline
        /// session lacks; the game skips that check inside cutscene commands, so the rule runs in that context.
        /// </summary>
        internal static RuleDealDamage Deal(RuleDealDamage rule)
        {
            using (ContextData<CommandAction.PlayerData>.Request()) return Rulebook.Trigger(rule);
        }

        internal static void InDamageContext(Action action)
        {
            using (ContextData<CommandAction.PlayerData>.Request()) action();
        }

        /// <summary>
        /// Drops a unit through the game's life controller. A view-less unit cannot play its death animation
        /// (UnitLifeController.OnUnitDeath calls View.HandleDeath after updating the life state), so for deaths the
        /// test raises the life-state event the controller raises right after that call.
        /// </summary>
        internal static void Fall(UnitEntityData unit)
        {
            var previous = unit.Descriptor.State.LifeState;
            unit.Damage = Math.Max(unit.Damage, unit.MaxHP + unit.Stats.Constitution + 10);
            try
            {
                UnitLifeController.ForceTickOnUnit(unit);
            }
            catch (NullReferenceException) when (unit.View == null && unit.Descriptor.State.LifeState == UnitLifeState.Dead)
            {
                RaiseLifeState(unit, previous);
            }
        }

        internal static void RaiseLifeState(UnitEntityData unit, UnitLifeState previous)
            => EventBus.RaiseEvent<IUnitLifeStateChanged>(handler => handler.HandleUnitLifeStateChanged(unit, previous));

        /// <summary>Brings a fallen unit back to full health and consciousness.</summary>
        internal static void Revive(UnitEntityData unit)
        {
            unit.Damage = 0;
            unit.Descriptor.State.UpdateLifeState(UnitLifeState.Conscious);
        }

        /// <summary>
        /// The state the game's confusion controller enters for the AttackNearest condition (it rolls 100), and the
        /// command its AttackNearest step issues. Running the command needs a scene, so it is only created.
        /// </summary>
        private static UnitCommand CompelledCommand(UnitEntityData unit)
        {
            // The charge alternative needs pathfinding in a scene; without it the compulsion attacks directly.
            var charge = unit.Abilities.GetAbility(Kingmaker.Blueprints.Root.BlueprintRoot.Instance.SystemMechanics.ChargeAbility);
            if (charge != null) unit.Descriptor.RemoveFact(charge);
            var part = unit.Ensure<UnitPartConfusion>();
            part.State = ConfusionState.AttackNearest;
            part.RetainControl();
            return (UnitCommand)AccessTools.Method(typeof(UnitConfusionController), "AttackNearest").Invoke(null, new object[] { part });
        }

        private static UnitUseAbility Cast(AbilityData ability, UnitEntityData caster, UnitEntityData target)
        {
            var command = new UnitUseAbility(ability, new TargetWrapper(target)) { NeedLoS = false };
            AccessTools.Property(typeof(UnitCommand), "Executor").SetValue(command, caster, null);
            return command;
        }

        private static UnitCommand.ResultType Execute(UnitUseAbility command)
            => (UnitCommand.ResultType)AccessTools.Method(typeof(UnitUseAbility), "OnAction").Invoke(command, null);
    }
}
