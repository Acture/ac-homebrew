using System.Linq;
using ACHomebrew.Feats;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Newtonsoft.Json;
using WotR.Testing.Offline;
using Xunit;

namespace ACHomebrew.OfflineTests
{
    /// <summary>Finger of the Reaper with the vanilla Finger of Death blueprint and the game's damage and life rules.</summary>
    [Collection(ACHomebrewGameCollection.Name)]
    public sealed class FingerOfTheReaperTests
    {
        private const string MagicMissile = "4ac47ddb9fa1eaf43a1b6809980cfbd2";
        private const int CasterLevel = 15;

        private readonly ACHomebrewGame fixture;
        public FingerOfTheReaperTests(ACHomebrewGame fixture) => this.fixture = fixture;

        [Fact]
        public void TheVanillaSpellGainsOnePerCasterLevelThroughItsOwnEffect()
        {
            fixture.RequireGame();
            var reaper = Reaper();
            var target = DuelFeatTests.Enemy();
            target.Stats.HitPoints.BaseValue = 5000; // keep it alive so only the damage rule is observed
            var recorder = new DamageRecorder();
            EventBus.Subscribe(recorder);
            try
            {
                var context = Context(reaper, target, ReaperFinger.FingerOfDeath);
                DuelFeatTests.InDamageContext(() => AbilityExecutionProcess.ApplyEffectImmediate(context, target));
            }
            finally { EventBus.Unsubscribe(recorder); }
            var rule = Assert.Single(recorder.Rules);
            var fact = reaper.Progression.Features.GetFact(OfflineGame.Blueprint<BlueprintFeature>(Guids.Reaper.FingerOfTheReaper));
            var bonus = rule.DamageBundle.First().Modifiers.Where(m => m.Fact == fact).Sum(m => m.Value);
            Assert.Equal(CasterLevel, bonus);
            Assert.True(rule.Result > 0);
            fixture.Observations["reaper.vanillaEffect"] = new { rule.Result, bonus, saved = rule.HalfBecauseSavingThrow };
        }

        [Fact]
        public void BonusIsCasterLevelPlusKillsAndOnlyForFingerOfDeath()
        {
            fixture.RequireGame();
            var reaper = Reaper();
            var plain = OfflineGame.CreateUnit(MainAttributeFeatTests.VanillaUnit);
            var target = Sturdy();
            Assert.Equal(10, Hit(plain, target, ReaperFinger.FingerOfDeath, 10));
            Assert.Equal(10 + CasterLevel, Hit(reaper, target, ReaperFinger.FingerOfDeath, 10));
            Assert.Equal(10, Hit(reaper, target, MagicMissile, 10));
            Assert.Equal(10, DirectHit(reaper, target, 10));
            Kill(reaper, DuelFeatTests.Enemy());
            Kill(reaper, DuelFeatTests.Enemy());
            Assert.Equal(2, Kills(reaper));
            Assert.Equal(10 + CasterLevel + 2, Hit(reaper, target, ReaperFinger.FingerOfDeath, 10));
        }

        [Fact]
        public void OnlyALethalFingerOfDeathCountsOncePerDeath()
        {
            fixture.RequireGame();
            var reaper = Reaper();
            var rival = Reaper();

            var victim = DuelFeatTests.Enemy();
            Kill(reaper, victim);
            Assert.Equal(1, Kills(reaper));
            DuelFeatTests.RaiseLifeState(victim, UnitLifeState.Conscious); // a repeated event for the same death
            Assert.Equal(1, Kills(reaper));

            DuelFeatTests.Revive(victim); // raised and slain again: a new death
            Kill(reaper, victim);
            Assert.Equal(2, Kills(reaper));

            var finishedByOther = DuelFeatTests.Enemy();
            Hit(reaper, finishedByOther, ReaperFinger.FingerOfDeath, 1); // not lethal
            finishedByOther.Damage = finishedByOther.MaxHP - 1;
            DirectHit(reaper, finishedByOther, 50, keep: true); // final damage from another source
            DuelFeatTests.Fall(finishedByOther);
            Assert.Equal(2, Kills(reaper));

            var stolen = DuelFeatTests.Enemy();
            Kill(rival, stolen); // another caster's Finger of Death
            Assert.Equal(2, Kills(reaper));
            Assert.Equal(1, Kills(rival));

            var survivor = DuelFeatTests.Enemy();
            Hit(reaper, survivor, ReaperFinger.FingerOfDeath, survivor.MaxHP + 100, keep: true);
            Rulebook.Trigger(new RuleHealDamage(reaper, survivor, survivor.Damage)); // healed before the life controller ran: it never died
            Assert.True(survivor.HPLeft > 0);
            DuelFeatTests.Fall(survivor); // killed by something else later
            Assert.Equal(2, Kills(reaper));
        }

        [Fact]
        public void KillsAreSavedWithTheFeatAndResetWhenItIsLost()
        {
            fixture.RequireGame();
            var reaper = Reaper();
            Kill(reaper, DuelFeatTests.Enemy());
            Kill(reaper, DuelFeatTests.Enemy());
            var feat = OfflineGame.Blueprint<BlueprintFeature>(Guids.Reaper.FingerOfTheReaper);
            var fact = reaper.Progression.Features.GetFact(feat);
            var saved = JsonConvert.SerializeObject(fact, DefaultJsonSettings.DefaultSettings);
            Assert.Contains("\"Kills\":2", saved.Replace(" ", ""));
            reaper.Progression.Features.RemoveFact(fact);
            reaper.Progression.Features.AddFeature(feat);
            Assert.Equal(0, Kills(reaper));
            Assert.Equal(10 + CasterLevel, Hit(reaper, Sturdy(), ReaperFinger.FingerOfDeath, 10));
            fixture.Observations["reaper.saved"] = saved.Length;
        }

        private static UnitEntityData Reaper()
        {
            var unit = OfflineGame.CreateUnit(MainAttributeFeatTests.VanillaUnit);
            unit.Progression.Features.AddFeature(OfflineGame.Blueprint<BlueprintFeature>(Guids.Reaper.FingerOfTheReaper));
            return unit;
        }

        private static UnitEntityData Sturdy()
        {
            var unit = DuelFeatTests.Enemy();
            unit.Stats.HitPoints.BaseValue = 5000;
            return unit;
        }

        private static int Kills(UnitEntityData unit)
        {
            var kills = -1;
            unit.Progression.Features.GetFact(OfflineGame.Blueprint<BlueprintFeature>(Guids.Reaper.FingerOfTheReaper))
                .CallComponents<ReaperFinger>(component => kills = component.Kills);
            return kills;
        }

        private static AbilityExecutionContext Context(UnitEntityData caster, UnitEntityData target, string ability)
        {
            var data = new AbilityData(OfflineGame.Blueprint<BlueprintAbility>(ability), caster);
            var parameters = data.CalculateParams();
            parameters.CasterLevel = CasterLevel;
            return data.CreateExecutionContext(new TargetWrapper(target), parameters);
        }

        private static int Hit(UnitEntityData caster, UnitEntityData target, string ability, int amount, bool keep = false)
        {
            var hp = target.Damage;
            var rule = new RuleDealDamage(caster, target, new DirectDamage(new DiceFormula(0, DiceType.Zero), amount)) { Reason = Context(caster, target, ability) };
            DuelFeatTests.Deal(rule);
            if (!keep) target.Damage = hp;
            return rule.Result;
        }

        private static int DirectHit(UnitEntityData source, UnitEntityData target, int amount, bool keep = false)
        {
            var hp = target.Damage;
            var rule = DuelFeatTests.Deal(new RuleDealDamage(source, target, new DirectDamage(new DiceFormula(0, DiceType.Zero), amount)));
            if (!keep) target.Damage = hp;
            return rule.Result;
        }

        /// <summary>A lethal Finger of Death hit followed by the game's life controller.</summary>
        private static void Kill(UnitEntityData caster, UnitEntityData victim)
        {
            Hit(caster, victim, ReaperFinger.FingerOfDeath, victim.HPLeft + victim.Stats.Constitution + 50, keep: true);
            DuelFeatTests.Fall(victim);
            Assert.Equal(UnitLifeState.Dead, victim.Descriptor.State.LifeState);
        }
    }
}
