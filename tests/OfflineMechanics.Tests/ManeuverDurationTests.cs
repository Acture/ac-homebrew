using System;
using System.Linq;
using ACHomebrew;
using ACHomebrew.Feats;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.PubSubSystem;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using WotR.Testing.Offline;
using Xunit;

namespace ACHomebrew.OfflineTests
{
    [Collection(ACHomebrewGameCollection.Name)]
    public sealed class ManeuverDurationTests
    {
        private readonly ACHomebrewGame fixture;
        public ManeuverDurationTests(ACHomebrewGame fixture) => this.fixture = fixture;

        [Theory]
        [InlineData(false, 0, 6)]
        [InlineData(true, 0, 18)]
        [InlineData(true, 5, 24)]
        [InlineData(true, 10, 36)]
        public void SuccessfulDirtyTrickExtendsTheActualGameBuff(bool feat, int margin, int seconds)
        {
            fixture.RequireGame();
            using var playerCommand = ContextData<Kingmaker.AreaLogic.Cutscenes.CommandAction.PlayerData>.Request();
            var actor = Unit(feat);
            var target = Unit();
            var rule = Trigger(actor, target, CombatManeuver.DirtyTrickSickened, margin);
            Assert.True(rule.Success);
            Assert.Equal(margin, rule.SuccessMargin);
            Assert.Equal(seconds, target.Buffs.GetBuff(Effect(rule.Type)).TimeLeft.TotalSeconds);
        }

        [Theory]
        [InlineData(CombatManeuver.Disarm, false, 6)]
        [InlineData(CombatManeuver.Disarm, true, 18)]
        [InlineData(CombatManeuver.SunderArmor, true, 18)]
        [InlineData(CombatManeuver.DirtyTrickBlind, true, 18)]
        [InlineData(CombatManeuver.DirtyTrickEntangle, true, 18)]
        public void OtherNativeTimedManeuversAlsoExtend(CombatManeuver type, bool feat, int seconds)
        {
            fixture.RequireGame();
            using var playerCommand = ContextData<Kingmaker.AreaLogic.Cutscenes.CommandAction.PlayerData>.Request();
            var actor = Unit(feat);
            var target = Unit();
            if (type == CombatManeuver.Disarm)
            {
                target.Progression.Features.AddFeature(OfflineGame.Blueprint<BlueprintFeature>("62e27ffd9d53e14479f73da29760f64e"));
                Assert.True(target.Body.PrimaryHand.InsertItem(new ItemEntityWeapon(
                    OfflineGame.Blueprint<BlueprintItemWeapon>("533e10c8b4c6a4940a3767d096f4f05d"))), "Could not equip the disarm target's longsword");
                Assert.False(target.Body.PrimaryHand.MaybeWeapon.Blueprint.IsUnarmed);
                Assert.False(target.Body.PrimaryHand.MaybeWeapon.Blueprint.IsNatural);
            }
            // Vanilla DisarmMainHandBuff removes itself immediately outside party combat.
            // Supply this world-state input without replacing its condition or activation actions.
            var player = Kingmaker.Game.Instance.Player;
            var wasInCombat = player.IsInCombat;
            var combatProperty = HarmonyLib.AccessTools.Property(typeof(Kingmaker.Player), nameof(Kingmaker.Player.IsInCombat));
            try
            {
                combatProperty.SetValue(player, true);
                var result = Trigger(actor, target, type);
                Assert.True(result.Success, $"{result.CancellationReason}: margin={result.SuccessMargin}");
                var applied = target.Buffs.GetBuff(Effect(type));
                Assert.NotNull(applied);
                Assert.Equal(seconds, applied.TimeLeft.TotalSeconds);
                if (type == CombatManeuver.Disarm)
                {
                    Assert.True(target.Body.PrimaryHand.Lock);
                    applied.Remove();
                    Assert.False(target.Body.PrimaryHand.Lock);
                }
            }
            finally { combatProperty.SetValue(player, wasInCombat); }
        }

        [Fact]
        public void FailureAndTripDoNotCreateTimedEffectsAndScopeDoesNotLeak()
        {
            fixture.RequireGame();
            using var playerCommand = ContextData<Kingmaker.AreaLogic.Cutscenes.CommandAction.PlayerData>.Request();
            var actor = Unit(true);
            var target = Unit();
            var count = target.Buffs.Enumerable.Count();
            Assert.False(Trigger(actor, target, CombatManeuver.DirtyTrickSickened, -1).Success);
            Assert.Equal(count, target.Buffs.Enumerable.Count());
            Assert.True(Trigger(actor, target, CombatManeuver.Trip).Success);
            Assert.True(target.State.Prone.ShouldBeActive);
            Assert.Equal(count, target.Buffs.Enumerable.Count());
            // Same blueprint, but applied outside the maneuver: must retain its original duration.
            var unrelated = target.AddBuff(Effect(CombatManeuver.DirtyTrickSickened), actor, TimeSpan.FromSeconds(6), null);
            Assert.Equal(6, unrelated.TimeLeft.TotalSeconds);
        }

        [Fact]
        public void RegisteredThirdPartyEffectExtendsOnceBeforeNativeReapplication()
        {
            fixture.RequireGame();
            using var playerCommand = ContextData<Kingmaker.AreaLogic.Cutscenes.CommandAction.PlayerData>.Request();
            var actor = Unit(true);
            var target = Unit();
            var ability = new BlueprintAbility { AssetGuid = BlueprintGuid.Parse(Guid.NewGuid().ToString("N")), Type = AbilityType.Physical };
            var effect = Effect(CombatManeuver.DirtyTrickSickened);
            ManeuverCompatibility.Register(ability, effect);
            ResourcesLibrary.BlueprintsCache.AddCachedBlueprint(ability.AssetGuid, ability);
            var context = new MechanicsContext(actor, target.Descriptor, ability);
            using var successScope = ManeuverDuration.SuccessfulEffects(context, target);
            var first = target.AddBuff(effect, context, TimeSpan.FromSeconds(6));
            Assert.Equal(18, first.TimeLeft.TotalSeconds);
            var again = target.AddBuff(effect, context, TimeSpan.FromSeconds(6));
            Assert.Equal(18, again.TimeLeft.TotalSeconds);
            var unrelated = target.AddBuff(Effect(CombatManeuver.DirtyTrickBlind), context, TimeSpan.FromSeconds(6));
            Assert.Equal(6, unrelated.TimeLeft.TotalSeconds);
            using (ManeuverDuration.SuccessfulEffects(context, actor))
                Assert.Equal(6, actor.AddBuff(effect, context, TimeSpan.FromSeconds(6)).TimeLeft.TotalSeconds);
            var permanentTarget = Unit();
            Assert.True(permanentTarget.AddBuff(effect, context, null).IsPermanent);
            var otherActor = Unit();
            var otherTarget = Unit();
            var noFeat = new MechanicsContext(otherActor, otherTarget.Descriptor, ability);
            using (ManeuverDuration.SuccessfulEffects(noFeat, otherTarget))
                Assert.Equal(6, otherTarget.AddBuff(effect, noFeat, TimeSpan.FromSeconds(6)).TimeLeft.TotalSeconds);
        }

        [Fact]
        public void SettingsApplyToNewEffectsWithoutRewritingExistingBuffs()
        {
            fixture.RequireGame();
            using var playerCommand = ContextData<Kingmaker.AreaLogic.Cutscenes.CommandAction.PlayerData>.Request();
            var actor = Unit(true);
            var target = Unit();
            var oldMultiplier = Mod.Settings.ManeuverDurationMultiplier;
            var oldExtra = Mod.Settings.ManeuverMinimumExtraRounds;
            try
            {
                Mod.Settings.ManeuverDurationMultiplier = 3;
                Mod.Settings.ManeuverMinimumExtraRounds = 0;
                Trigger(actor, target, CombatManeuver.DirtyTrickSickened, 5);
                var buff = target.Buffs.GetBuff(Effect(CombatManeuver.DirtyTrickSickened));
                Assert.Equal(36, buff.TimeLeft.TotalSeconds);
                Mod.Settings.ManeuverDurationMultiplier = 1;
                Assert.Equal(36, buff.TimeLeft.TotalSeconds);
                var next = Unit();
                Trigger(actor, next, CombatManeuver.DirtyTrickSickened, 5);
                Assert.Equal(12, next.Buffs.GetBuff(Effect(CombatManeuver.DirtyTrickSickened)).TimeLeft.TotalSeconds);
            }
            finally
            {
                Mod.Settings.ManeuverDurationMultiplier = oldMultiplier;
                Mod.Settings.ManeuverMinimumExtraRounds = oldExtra;
            }
        }

        [Theory]
        [InlineData(-1, 6, false)]
        [InlineData(0, 24, false)]
        [InlineData(0, 24, true)]
        public void CustomManeuverOnlyExtendsItsSuccessfulBranch(int margin, int seconds, bool nested)
        {
            fixture.RequireGame();
            using var playerCommand = ContextData<Kingmaker.AreaLogic.Cutscenes.CommandAction.PlayerData>.Request();
            var actor = Unit(true);
            var target = Unit();
            actor.Progression.Features.AddFeature(new BlueprintFeature
            {
                AssetGuid = BlueprintGuid.Parse(Guid.NewGuid().ToString("N")),
                ComponentsArray = new BlueprintComponent[] { new ForceManeuverResult { Margin = margin } },
            });
            var buff = new BlueprintBuff { name = "CustomTimedManeuver", AssetGuid = BlueprintGuid.Parse(Guid.NewGuid().ToString("N")) };
            ResourcesLibrary.BlueprintsCache.AddCachedBlueprint(buff.AssetGuid, buff);
            var flags = HarmonyLib.AccessTools.Field(typeof(BlueprintBuff), "m_Flags");
            flags.SetValue(buff, Enum.Parse(flags.FieldType, "Harmful"));
            ContextActionApplyBuff Apply(float duration)
            {
                var apply = new ContextActionApplyBuff { UseDurationSeconds = true, DurationSeconds = duration, AsChild = false };
                HarmonyLib.AccessTools.Field(typeof(ContextActionApplyBuff), "m_Buff").SetValue(apply, buff.ToReference<BlueprintBuffReference>());
                return apply;
            }
            var action = new ContextActionCombatManeuverCustom
            {
                Type = CombatManeuver.Grapple,
                Success = new ActionList { Actions = new GameAction[] { Apply(12) } },
                Failure = new ActionList { Actions = new GameAction[] { Apply(6) } },
            };
            var ability = new BlueprintAbility
            {
                AssetGuid = BlueprintGuid.Parse(Guid.NewGuid().ToString("N")), Type = AbilityType.Physical,
                ComponentsArray = new BlueprintComponent[]
                {
                    new AbilityEffectRunAction { Actions = new ActionList { Actions = new GameAction[] { action } } },
                },
            };
            ResourcesLibrary.BlueprintsCache.AddCachedBlueprint(ability.AssetGuid, ability);
            var context = new MechanicsContext(actor, target.Descriptor, ability);
            Assert.True(ManeuverCompatibility.CanExtendEffect(ability, buff));
            var subscriber = new NestedFailedManeuver { Actor = actor };
            if (nested) EventBus.Subscribe(subscriber);
            try { using (context.GetDataScope(target)) action.RunAction(); }
            finally { if (nested) EventBus.Unsubscribe(subscriber); }
            if (nested) Assert.True(subscriber.Ran);
            Assert.Equal(seconds, target.Buffs.GetBuff(buff).TimeLeft.TotalSeconds);
            target.Buffs.GetBuff(buff).Remove();
            // The same target and context after RunAction must no longer inherit the bonus.
            Assert.Equal(6, target.AddBuff(buff, context, TimeSpan.FromSeconds(6)).TimeLeft.TotalSeconds);
        }

        [Fact]
        public void RegisteredProneEffectAndExceptionCleanupPreserveOriginalDurations()
        {
            fixture.RequireGame();
            using var playerCommand = ContextData<Kingmaker.AreaLogic.Cutscenes.CommandAction.PlayerData>.Request();
            var actor = Unit(true);
            var target = Unit();
            var ability = new BlueprintAbility { AssetGuid = BlueprintGuid.Parse(Guid.NewGuid().ToString("N")), Type = AbilityType.Physical };
            ResourcesLibrary.BlueprintsCache.AddCachedBlueprint(ability.AssetGuid, ability);
            var prone = new BlueprintBuff
            {
                AssetGuid = BlueprintGuid.Parse(Guid.NewGuid().ToString("N")),
                ComponentsArray = new BlueprintComponent[] { new Kingmaker.UnitLogic.FactLogic.AddCondition { Condition = UnitCondition.Prone } },
            };
            ResourcesLibrary.BlueprintsCache.AddCachedBlueprint(prone.AssetGuid, prone);
            var effect = Effect(CombatManeuver.DirtyTrickSickened);
            ManeuverCompatibility.Register(ability, prone, effect);
            var context = new MechanicsContext(actor, target.Descriptor, ability);
            Assert.Throws<InvalidOperationException>((Action)(() =>
            {
                using (ManeuverDuration.SuccessfulEffects(context, target))
                {
                    Assert.Equal(6, target.AddBuff(prone, context, TimeSpan.FromSeconds(6)).TimeLeft.TotalSeconds);
                    throw new InvalidOperationException("Scope must unwind on error");
                }
            }));
            Assert.Equal(6, target.AddBuff(effect, context, TimeSpan.FromSeconds(6)).TimeLeft.TotalSeconds);
        }

        private static UnitEntityData Unit(bool feat = false)
        {
            var unit = OfflineGame.CreateUnit("fbd199907bdc8b948a5afc7313ac29f5");
            if (feat) unit.Progression.Features.AddFeature(OfflineGame.Blueprint<BlueprintFeature>(Guids.ManeuverTactics.LingeringDisruption));
            return unit;
        }

        private static RuleCombatManeuver Trigger(UnitEntityData actor, UnitEntityData target, CombatManeuver type, int margin = 0)
        {
            var cmd = Rulebook.Trigger(new RuleCalculateCMD(actor, target, type)).Result;
            return Rulebook.Trigger(new RuleCombatManeuver(actor, target, type)
            {
                IgnoreConcealment = true,
                OverrideRoll = RuleRollD20.FromInt(actor, 10),
                OverrideBonus = cmd - 10 + margin,
            });
        }

        private static BlueprintBuff Effect(CombatManeuver type)
        {
            var system = BlueprintRoot.Instance.SystemMechanics;
            return type switch
            {
                CombatManeuver.Disarm => system.DisarmMainHandBuff,
                CombatManeuver.SunderArmor => system.SunderArmorBuff,
                CombatManeuver.DirtyTrickBlind => system.DirtyTrickBlindnessBuff,
                CombatManeuver.DirtyTrickEntangle => system.DirtyTrickEntangledBuff,
                _ => system.DirtyTrickSickenedBuff,
            };
        }
    }
}
