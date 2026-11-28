using System;
using System.Linq;
using ACHomebrew.Feats;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using WotR.Testing.Offline;
using Xunit;
using Xunit.Sdk;

namespace ACHomebrew.OfflineTests
{
    /// <summary>
    /// Martyr's Transference (Body of My Pact): the summoner takes -4 penalties to Strength, Dexterity,
    /// and Constitution, and summoned creatures gain +4 bonuses to the same attributes.
    /// Tests the self-penalty application, removal, summon bonus (where Unity allows), and a failure control.
    /// </summary>
    [Collection(ACHomebrewGameCollection.Name)]
    public sealed class SummonerSacrificeFeatTests
    {
        private const string VanillaSummon = "5f968d63d756f994ebff0d774e88e4ab"; // CR2_BoarStandard

        private static readonly StatType[] PhysicalStats = { StatType.Strength, StatType.Dexterity, StatType.Constitution };

        private readonly ACHomebrewGame fixture;

        public SummonerSacrificeFeatTests(ACHomebrewGame fixture) => this.fixture = fixture;

        /// <summary>Adding the feat applies the outer buff to the summoner with -4 penalties to the three physical stats.
        /// Removing the feat restores the original stats.</summary>
        [Fact]
        public void MartyrsTransferenceAppliesSelfPenaltiesAndRemovesThem()
        {
            fixture.RequireGame();
            var feat = OfflineGame.Blueprint<BlueprintFeature>(Guids.SummonerSacrifice.Feature.BodyOfMyPact);
            var outerBuff = OfflineGame.Blueprint<BlueprintBuff>(Guids.SummonerSacrifice.OuterBuff.BodyOfMyPact);
            var unit = OfflineGame.CreateUnit(MainAttributeFeatTests.VanillaUnit);
            MainAttributeFeatTests.AssertLiving(unit);

            var before = OfflineGame.StatSnapshot(unit);

            var fact = unit.Progression.Features.AddFeature(feat);
            Assert.True(fact.IsActive && fact.IsTurnedOn, "The game did not activate the feat.");

            // The outer buff should be on the summoner with -4 to physical stats.
            var buff = unit.Buffs.GetBuff(outerBuff);
            Assert.NotNull(buff);

            foreach (var stat in PhysicalStats)
            {
                var statValue = unit.Stats.GetStat(stat);
                var diff = statValue.ModifiedValue - before[stat];
                Assert.Equal(-4, diff);
                var modifiers = statValue.Modifiers.Where(m => m.Source?.Blueprint == outerBuff).ToArray();
                Assert.Single(modifiers);
                Assert.Equal(-4, modifiers[0].ModValue);
            }

            // Non-physical stats must not be affected by the outer buff.
            foreach (var stat in Enum.GetValues(typeof(StatType)).Cast<StatType>()
                .Where(s => !PhysicalStats.Contains(s)))
            {
                var statValue = unit.Stats.GetStat(stat);
                if (statValue == null) continue;
                Assert.DoesNotContain(statValue.Modifiers, m => m.Source?.Blueprint == outerBuff);
            }

            // Removal restores stats.
            UnitHelper.RemoveFact(unit, fact);
            Assert.False(unit.Progression.Features.HasFact(feat));
            Assert.Null(unit.Buffs.GetBuff(outerBuff));

            foreach (var stat in PhysicalStats)
            {
                Assert.Equal(before[stat], unit.Stats.GetStat(stat).ModifiedValue);
            }

            fixture.Observations["martyrsTransference"] = new
            {
                unit = unit.Blueprint.name,
                penaltiesApplied = PhysicalStats.ToDictionary(s => s.ToString(), s =>
                {
                    var v = unit.Stats.GetStat(s);
                    return $"{before[s]} -> {v.ModifiedValue}";
                }),
            };
        }

        /// <summary>When a summon is created, the inner buff with +4 bonuses is applied to the summoned creature
        /// through the game's summon rule, not to the summoner.</summary>
        [SkippableFact]
        public void SummonedCreatureReceivesBonusesFromTheSummonersFeat()
        {
            fixture.RequireGame();
            var feat = OfflineGame.Blueprint<BlueprintFeature>(Guids.SummonerSacrifice.Feature.BodyOfMyPact);
            var outerBuff = OfflineGame.Blueprint<BlueprintBuff>(Guids.SummonerSacrifice.OuterBuff.BodyOfMyPact);
            var innerBuff = OfflineGame.Blueprint<BlueprintBuff>(Guids.SummonerSacrifice.InnerBuff.BodyOfMyPact);

            var summoner = OfflineGame.CreateUnit(MainAttributeFeatTests.VanillaUnit);
            MainAttributeFeatTests.AssertLiving(summoner);
            var summonerBefore = OfflineGame.StatSnapshot(summoner);
            summoner.Progression.Features.AddFeature(feat);

            // The summoner must carry the outer (trigger) buff but not receive the inner (summon) buff.
            Assert.NotNull(summoner.Buffs.GetBuff(outerBuff));
            Assert.Null(summoner.Buffs.GetBuff(innerBuff));

            var summon = OfflineGame.CreateUnit(VanillaSummon);
            var summonBefore = OfflineGame.StatSnapshot(summon);
            try
            {
                Rulebook.Trigger(new RuleSummonUnit(summoner, summon, new Rounds(10)));
            }
            catch (Exception error) when (UnityNative.IsUnavailable(error))
            {
                fixture.Observations["martyrsTransference.summon.unityRequired"] = error.ToString();
                Skip.If(true, UnityNative.SkipReason("RuleSummonUnit", error));
            }

            // The summon should get the inner buff with +4 to physical stats.
            var inner = summon.Buffs.GetBuff(innerBuff);
            Assert.NotNull(inner);
            Assert.Same(summoner, inner.Context?.MaybeCaster);

            foreach (var stat in PhysicalStats)
            {
                var diff = summon.Stats.GetStat(stat).ModifiedValue - summonBefore[stat];
                Assert.Equal(4, diff);
                var modifiers = summon.Stats.GetStat(stat).Modifiers
                    .Where(m => m.Source?.Blueprint == innerBuff).ToArray();
                Assert.Single(modifiers);
                Assert.Equal(4, modifiers[0].ModValue);
            }

            // The summoner must still NOT have the inner buff.
            Assert.Null(summoner.Buffs.GetBuff(innerBuff));

            fixture.Observations["martyrsTransference.summon"] = new
            {
                summoner = summoner.Blueprint.name,
                summon = summon.Blueprint.name,
                summonBonuses = PhysicalStats.ToDictionary(s => s.ToString(), s =>
                {
                    var v = summon.Stats.GetStat(s);
                    return $"{summonBefore[s]} -> {v.ModifiedValue}";
                }),
            };
        }

        /// <summary>Removing the stat-bonus components from the outer buff makes the self-penalty assertion fail,
        /// proving the test measures the effect, not just the fixture.</summary>
        [Fact]
        public void FailureControlWithoutPenaltyComponentsFailsTheSameAssertion()
        {
            fixture.RequireGame();
            var feat = OfflineGame.Blueprint<BlueprintFeature>(Guids.SummonerSacrifice.Feature.BodyOfMyPact);
            var outerBuff = OfflineGame.Blueprint<BlueprintBuff>(Guids.SummonerSacrifice.OuterBuff.BodyOfMyPact);

            var originalOuter = outerBuff.ComponentsArray;
            var removed = originalOuter.Count(c => c is Kingmaker.UnitLogic.FactLogic.AddStatBonus);
            Assert.True(removed > 0);
            try
            {
                outerBuff.ComponentsArray = originalOuter
                    .Where(c => c is not Kingmaker.UnitLogic.FactLogic.AddStatBonus).ToArray();

                var unit = OfflineGame.CreateUnit(MainAttributeFeatTests.VanillaUnit);
                MainAttributeFeatTests.AssertLiving(unit);
                var fact = unit.Progression.Features.AddFeature(feat);
                Assert.True(fact.IsActive, "The control feat must still be added.");

                var failure = Assert.ThrowsAny<XunitException>(() =>
                {
                    var statValue = unit.Stats.GetStat(StatType.Strength);
                    var modifiers = statValue.Modifiers
                        .Where(m => m.Source?.Blueprint == outerBuff).ToArray();
                    Assert.Single(modifiers);
                });
                fixture.Observations["martyrsTransference.failureControl"] = new
                {
                    removedComponents = removed,
                    assertionMessage = failure.Message,
                };
                UnitHelper.RemoveFact(unit, fact);
            }
            finally
            {
                outerBuff.ComponentsArray = originalOuter;
            }
        }
    }
}