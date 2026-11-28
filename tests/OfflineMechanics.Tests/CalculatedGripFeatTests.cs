using System;
using System.Collections.Generic;
using System.Linq;
using ACHomebrew.Feats;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.FactLogic;
using WotR.Testing.Offline;
using Xunit;
using Xunit.Sdk;

namespace ACHomebrew.OfflineTests
{
    /// <summary>
    /// Calculated Grip (Anatomical Leverage): adds the Intelligence modifier as an untyped bonus to CMB.
    /// Tests cover addition, Intelligence recalculation, removal, and a failure control.
    /// </summary>
    [Collection(ACHomebrewGameCollection.Name)]
    public sealed class CalculatedGripFeatTests
    {
        private readonly ACHomebrewGame fixture;

        public CalculatedGripFeatTests(ACHomebrewGame fixture) => this.fixture = fixture;

        /// <summary>Adding the feat increases the AdditionalCMB stat by the Intelligence modifier (minimum 0).
        /// Changing Intelligence recalculates the bonus. Removing the feat restores the original value.</summary>
        [Fact]
        public void CalculatedGripAddsIntelligenceModifierToAdditionalCmbAndRecalculates()
        {
            fixture.RequireGame();
            var feat = OfflineGame.Blueprint<BlueprintFeature>(Guids.Replacement.Extended.CalculatedGrip);
            var unit = OfflineGame.CreateUnit(MainAttributeFeatTests.VanillaUnit);
            MainAttributeFeatTests.AssertLiving(unit);
            // Seelah has INT 10 (modifier 0); set it to 14 so the feat has a non-zero bonus to test.
            unit.Stats.Intelligence.BaseValue = 14;
            var before = OfflineGame.StatSnapshot(unit);

            // The feat modifies AdditionalCMB, so record its baseline.
            var cmbBefore = unit.Stats.GetStat(StatType.AdditionalCMB).ModifiedValue;

            var fact = unit.Progression.Features.AddFeature(feat);
            Assert.True(fact.IsActive && fact.IsTurnedOn, "The game did not activate the feat.");

            var intMod = MainAttributeFeatTests.AbilityModifier(unit, StatType.Intelligence);
            var cmbAfter = unit.Stats.GetStat(StatType.AdditionalCMB).ModifiedValue;
            var stat = unit.Stats.GetStat(StatType.AdditionalCMB);
            Assert.Equal(intMod, cmbAfter - cmbBefore);

            // Verify the bonus comes from exactly one untyped modifier from the feat.
            var modifiers = stat.Modifiers.Where(m => m.Source?.Blueprint == feat).ToArray();
            Assert.Single(modifiers);
            Assert.Equal(ModifierDescriptor.None, modifiers[0].ModDescriptor);
            Assert.Equal(intMod, modifiers[0].ModValue);

            var initialCmb = cmbAfter;

            // Verify the bonus tracks Intelligence by testing a second unit with a different INT value.
            var unit2 = OfflineGame.CreateUnit(MainAttributeFeatTests.VanillaUnit);
            MainAttributeFeatTests.AssertLiving(unit2);
            unit2.Stats.Intelligence.BaseValue = 18; // modifier 4
            var cmbBefore2 = unit2.Stats.GetStat(StatType.AdditionalCMB).ModifiedValue;
            var fact2 = unit2.Progression.Features.AddFeature(feat);
            Assert.True(fact2.IsActive && fact2.IsTurnedOn);
            var intMod2 = MainAttributeFeatTests.AbilityModifier(unit2, StatType.Intelligence);
            Assert.Equal(4, intMod2); // sanity: INT 18 => modifier 4
            var cmbAfter2 = unit2.Stats.GetStat(StatType.AdditionalCMB).ModifiedValue;
            Assert.Equal(intMod2, cmbAfter2 - cmbBefore2);
            var modifiers2 = unit2.Stats.GetStat(StatType.AdditionalCMB).Modifiers
                .Where(m => m.Source?.Blueprint == feat).ToArray();
            Assert.Single(modifiers2);
            Assert.Equal(intMod2, modifiers2[0].ModValue);
            UnitHelper.RemoveFact(unit2, fact2);

            // Remove the feat from the original unit.
            UnitHelper.RemoveFact(unit, fact);
            Assert.False(unit.Progression.Features.HasFact(feat));
            Assert.Equal(cmbBefore, unit.Stats.GetStat(StatType.AdditionalCMB).ModifiedValue);
            Assert.DoesNotContain(stat.Modifiers, m => m.Source?.Blueprint == feat);

            fixture.Observations["calculatedGrip"] = new
            {
                unit = unit.Blueprint.name,
                intelligence = unit.Stats.Intelligence.ModifiedValue,
                intelligenceModifier = intMod,
                intMod2,
                cmbBefore,
                cmbWithFeat = initialCmb,
                cmbAfterRemoval = unit.Stats.GetStat(StatType.AdditionalCMB).ModifiedValue,
            };
        }

        /// <summary>Removing the stat-bonus component from the feat blueprint makes the same assertion fail,
        /// proving the test is sensitive to the effect, not just the fixture.</summary>
        [Fact]
        public void FailureControlWithoutStatBonusComponentFailsTheSameAssertion()
        {
            fixture.RequireGame();
            var feat = OfflineGame.Blueprint<BlueprintFeature>(Guids.Replacement.Extended.CalculatedGrip);
            var original = feat.ComponentsArray;
            var removed = original.Count(c => c is AddContextStatBonus);
            Assert.True(removed > 0);
            try
            {
                feat.ComponentsArray = original.Where(c => c is not AddContextStatBonus).ToArray();
                var unit = OfflineGame.CreateUnit(MainAttributeFeatTests.VanillaUnit);
                MainAttributeFeatTests.AssertLiving(unit);
                unit.Stats.Intelligence.BaseValue = 14;
                var fact = unit.Progression.Features.AddFeature(feat);
                Assert.True(fact.IsActive, "The control feat must still be added; only its effect is missing.");

                var failure = Assert.ThrowsAny<XunitException>(() =>
                {
                    var stat = unit.Stats.GetStat(StatType.AdditionalCMB);
                    var modifiers = stat.Modifiers.Where(m => m.Source?.Blueprint == feat).ToArray();
                    Assert.Single(modifiers);
                });
                fixture.Observations["calculatedGrip.failureControl"] = new
                {
                    removedComponents = removed,
                    assertionMessage = failure.Message,
                };
                UnitHelper.RemoveFact(unit, fact);
            }
            finally
            {
                feat.ComponentsArray = original;
            }
        }
    }
}