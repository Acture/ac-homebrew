using System.Linq;
using ACHomebrew.Feats;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using WotR.Testing.Offline;
using Xunit;

namespace ACHomebrew.OfflineTests
{
    /// <summary>
    /// Grandmaster's Gambit (Intelligence stance) on a real unit: attacks of opportunity gain the Intelligence
    /// modifier and other attack rolls take an equal penalty, both from a modifier of at least 0.
    /// The game clamps a rank config after its progression, so this guards the sign of the penalty.
    /// </summary>
    [Collection(ACHomebrewGameCollection.Name)]
    public sealed class StanceFeatTests
    {
        private readonly ACHomebrewGame fixture;

        public StanceFeatTests(ACHomebrewGame fixture) => this.fixture = fixture;

        [Theory]
        [InlineData(18, 4)]
        [InlineData(10, 0)]
        [InlineData(6, 0)] // a negative modifier must not turn the penalty into a bonus
        public void IntelligenceStanceTradesOtherAttacksForAttacksOfOpportunity(int intelligence, int modifier)
        {
            fixture.RequireGame();
            var buffBlueprint = OfflineGame.Blueprint<BlueprintBuff>(Guids.Stance.Buff.Int);
            var unit = OfflineGame.CreateUnit(MainAttributeFeatTests.VanillaUnit);
            unit.Stats.Intelligence.BaseValue = intelligence;
            var buff = unit.AddBuff(buffBlueprint, unit, null, null);
            Assert.NotNull(buff);

            var bonuses = buffBlueprint.ComponentsArray.OfType<AttackOfOpportunityAttackBonus>().ToArray();
            int Bonus(bool otherAttacks) => bonuses.Single(c => c.NotAttackOfOpportunity == otherAttacks).Bonus.Calculate(buff.Context);
            fixture.Observations[$"stance.intelligence.{intelligence}"] = new
            {
                intelligence = unit.Stats.Intelligence.ModifiedValue,
                attacksOfOpportunity = Bonus(false),
                otherAttacks = Bonus(true),
            };
            Assert.Equal(modifier, Bonus(false));
            Assert.Equal(-modifier, Bonus(true));
        }
    }
}
