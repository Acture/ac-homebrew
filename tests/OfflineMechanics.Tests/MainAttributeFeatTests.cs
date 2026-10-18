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
    /// Main Attribute Mastery on a real vanilla unit: add, check the game's stat system, remove, check every stat returns.
    /// Expected values follow the feat descriptions with default settings (Balanced, attribute bonuses on, power options off).
    /// </summary>
    [Collection(ACHomebrewGameCollection.Name)]
    public sealed class MainAttributeFeatTests
    {
        // MC_Human_M_Cavalier_Base: a vanilla human pregen; its class levels are applied by the game's own AddClassLevels.
        internal const string VanillaUnit = "5def1061f2e84909a1235ac8bb42578f";

        private static readonly StatType[] Attributes =
        {
            StatType.Strength, StatType.Dexterity, StatType.Constitution, StatType.Intelligence, StatType.Wisdom, StatType.Charisma,
        };

        private readonly ACHomebrewGame fixture;

        public MainAttributeFeatTests(ACHomebrewGame fixture) => this.fixture = fixture;

        /// <summary>Strength Mastery: Dexterity adds the Strength modifier (minimum 0) to Dexterity only, as an inherent bonus.</summary>
        [Fact]
        public void StrengthToDexterityMasteryAddsTheModifierToDexterityOnly()
        {
            fixture.RequireGame();
            var feat = OfflineGame.Blueprint<BlueprintFeature>(Guids.MainAttribute.Str.Dex);
            var expected = AddAndRemove(feat, unit => new Dictionary<StatType, int> { [StatType.Dexterity] = AbilityModifier(unit, StatType.Strength) }, "mainAttribute.strToDex");
            Assert.True(expected > 0);
        }

        /// <summary>Retired Titan's Apotheosis: half the Strength modifier (rounded down, minimum 0) on the other five attributes.</summary>
        [Fact]
        public void RetiredTitansApotheosisAddsHalfTheModifierToTheOtherAttributes()
        {
            fixture.RequireGame();
            var feat = OfflineGame.Blueprint<BlueprintFeature>(Guids.str_main_to_everything);
            var expected = AddAndRemove(feat, unit =>
            {
                var half = AbilityModifier(unit, StatType.Strength) / 2;
                return Attributes.Where(stat => stat != StatType.Strength).ToDictionary(stat => stat, _ => half);
            }, "mainAttribute.retiredTitansApotheosis");
            Assert.True(expected > 0);
        }

        [Fact]
        public void FailureControlWithoutStatComponentsFailsTheSameAssertion()
        {
            fixture.RequireGame();
            var feat = OfflineGame.Blueprint<BlueprintFeature>(Guids.MainAttribute.Str.Dex);
            var original = feat.ComponentsArray;
            var removed = original.Count(c => c is AddContextStatBonus);
            Assert.True(removed > 0);
            try
            {
                // Deliberately remove the effect from the real blueprint; everything else stays the same.
                feat.ComponentsArray = original.Where(c => c is not AddContextStatBonus).ToArray();
                var unit = OfflineGame.CreateUnit(VanillaUnit);
                var before = OfflineGame.StatSnapshot(unit);
                var expected = new Dictionary<StatType, int> { [StatType.Dexterity] = AbilityModifier(unit, StatType.Strength) };
                var fact = unit.Progression.Features.AddFeature(feat);
                Assert.True(fact.IsActive, "The control feat must still be added; only its effect is missing.");

                var failure = Assert.ThrowsAny<XunitException>(() => AssertInherentBonuses(unit, feat, before, expected));
                fixture.Observations["mainAttribute.failureControl"] = new { removedComponents = removed, assertionMessage = failure.Message };
                UnitHelper.RemoveFact(unit, fact);
            }
            finally
            {
                feat.ComponentsArray = original;
            }
        }

        /// <summary>Adds the feat, checks the expected inherent bonuses, removes it and checks every stat returns. Returns the smallest expected bonus.</summary>
        private int AddAndRemove(BlueprintFeature feat, Func<UnitEntityData, Dictionary<StatType, int>> expectedFor, string observation)
        {
            var unit = OfflineGame.CreateUnit(VanillaUnit);
            AssertLiving(unit);
            var before = OfflineGame.StatSnapshot(unit);
            var expected = expectedFor(unit);

            var fact = unit.Progression.Features.AddFeature(feat);
            Assert.True(fact.IsActive && fact.IsTurnedOn, "The game did not activate the feat.");
            AssertInherentBonuses(unit, feat, before, expected);
            fixture.Observations[observation] = Describe(unit, before, expected);

            UnitHelper.RemoveFact(unit, fact);
            Assert.False(unit.Progression.Features.HasFact(feat));
            Assert.Equal(before, OfflineGame.StatSnapshot(unit));
            Assert.Empty(ModifiersFrom(unit, feat));
            return expected.Values.Min();
        }

        /// <summary>
        /// The single effect assertion shared by the normal cases and the failure control: each expected attribute rises by
        /// exactly its bonus through one inherent modifier from the feat, and the feat modifies no other stat.
        /// </summary>
        internal static void AssertInherentBonuses(UnitEntityData unit, BlueprintFeature feat, IReadOnlyDictionary<StatType, int> before, IReadOnlyDictionary<StatType, int> expected)
        {
            foreach (var pair in expected)
            {
                var stat = unit.Stats.GetStat(pair.Key);
                Assert.True(stat.ModifiedValue - before[pair.Key] == pair.Value,
                    $"{pair.Key}: expected +{pair.Value}, got {stat.ModifiedValue - before[pair.Key]:+0;-0;0} ({DescribeStat(stat)})");
                var modifiers = stat.Modifiers.Where(m => m.Source?.Blueprint == feat).ToArray();
                Assert.True(modifiers.Length == 1, $"{pair.Key}: expected one modifier from {feat.name}, found {modifiers.Length}");
                Assert.Equal(ModifierDescriptor.Inherent, modifiers[0].ModDescriptor);
                Assert.Equal(pair.Value, modifiers[0].ModValue);
            }
            var unexpected = Enum.GetValues(typeof(StatType)).Cast<StatType>().Distinct()
                .Where(type => !expected.ContainsKey(type))
                .Where(type => unit.Stats.GetStat(type)?.Modifiers.Any(m => m.Source?.Blueprint == feat) == true)
                .ToArray();
            Assert.True(unexpected.Length == 0, $"{feat.name} also modified: {string.Join(", ", unexpected)}");
        }

        /// <summary>Undead and constructs have no Constitution in the game rules; the tests need a living creature with a Strength modifier of at least +2.</summary>
        internal static void AssertLiving(UnitEntityData unit)
        {
            var types = unit.Facts.List.Select(f => f.Blueprint.name).Where(n => n is "UndeadType" or "ConstructType").ToArray();
            Assert.True(types.Length == 0, $"Precondition: {unit.Blueprint.name} must be a living creature, has {string.Join(", ", types)}.");
            Assert.True(AbilityModifier(unit, StatType.Strength) > 1, $"Precondition: {unit.Blueprint.name} needs a Strength modifier of at least +2, has Strength {unit.Stats.Strength.ModifiedValue}.");
        }

        /// <summary>Pathfinder ability modifier, independent of the mod: floor((score - 10) / 2), minimum 0 for these feats.</summary>
        internal static int AbilityModifier(UnitEntityData unit, StatType stat)
            => Math.Max(0, (int)Math.Floor((unit.Stats.GetStat(stat).ModifiedValue - 10) / 2.0));

        internal static int StrengthModifier(UnitEntityData unit) => AbilityModifier(unit, StatType.Strength);

        internal static Dictionary<StatType, int> Snapshot(UnitEntityData unit) => OfflineGame.StatSnapshot(unit);

        private static IEnumerable<ModifiableValue.Modifier> ModifiersFrom(UnitEntityData unit, BlueprintFeature feat)
            => Enum.GetValues(typeof(StatType)).Cast<StatType>().Distinct()
                .Select(unit.Stats.GetStat).Where(s => s != null)
                .SelectMany(s => s.Modifiers).Where(m => m.Source?.Blueprint == feat);

        private static string DescribeStat(ModifiableValue stat)
            => $"base {stat.BaseValue}, modified {stat.ModifiedValue}, modifiers: "
               + string.Join(", ", stat.Modifiers.Select(m => $"{m.ModDescriptor} {m.ModValue:+0;-0;0} from {m.Source?.Blueprint?.name ?? m.SourceComponent ?? "?"}"));

        private static object Describe(UnitEntityData unit, IReadOnlyDictionary<StatType, int> before, IReadOnlyDictionary<StatType, int> expected)
            => new
            {
                unit = unit.Blueprint.name,
                strength = unit.Stats.Strength.ModifiedValue,
                expected = expected.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value),
                changes = OfflineGame.StatSnapshot(unit).Where(kv => before.TryGetValue(kv.Key, out var old) && old != kv.Value)
                    .ToDictionary(kv => kv.Key.ToString(), kv => $"{before[kv.Key]} -> {kv.Value}"),
            };
    }
}
