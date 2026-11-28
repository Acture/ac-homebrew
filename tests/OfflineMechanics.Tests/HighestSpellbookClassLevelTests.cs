using System;
using System.Linq;
using ACHomebrew.Feats;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Properties;
using WotR.Testing.Offline;
using Xunit;

namespace ACHomebrew.OfflineTests
{
    /// <summary>
    /// The caster level source of Arcane Aegis, Ley-Infused Vitality and Dawn of the Soul: the
    /// mod's unit property "highest spellbook class level" — the highest level among the unit's
    /// classes that have a spellbook, never a sum. A feature or buff context carries no caster
    /// level of its own (the game's empty ability parameters carry caster level 1), so the feats
    /// read this property off the unit instead of the context parameters.
    /// <para>
    /// The offline unit is the vanilla dretch, a monster without class levels: the shared
    /// human pregen carries the Angel mythic class at level 10, which has a spellbook and
    /// would outrank the test classes. Class levels come from the game's own
    /// <c>UnitProgressionData.AddClassLevel</c>: the <c>ClassData</c> it creates copies the
    /// class blueprint's spellbook, so a wizard level counts and a fighter level does not.
    /// The level-up case calls <c>UnitProgressionData.ReapplyFeaturesOnLevelUp</c>, the game's
    /// own tail call of a level up, which reapplies every feature whose blueprint sets
    /// <c>ReapplyOnLevelUp</c>.
    /// </para>
    /// </summary>
    [Collection(ACHomebrewGameCollection.Name)]
    public sealed class HighestSpellbookClassLevelTests
    {
        // Dretch: a vanilla monster without class levels or a spellbook (same unit DuelFeatTests uses).
        private const string Dretch = "2d643696b0653b846bf95c62be792e85";

        // WizardClass and ClericClass: vanilla classes with a spellbook. FighterClass: none.
        private const string WizardClass = "ba34257984f4c41408ce1dc2004e342e";
        private const string ClericClass = "67819271767a9dd4fbfd4ae700befea0";
        private const string FighterClass = "48ac8db94d5de7645906c7d0ad3bcfbd";

        // SoulBulwarkTempBuff, the combat-start buff of the Soul Bulwark feat (a private constant in DerivedStatFeats).
        private const string SoulBulwarkTempBuff = "dbbbf07b-3c5f-4d4c-84a9-952f531e242e";

        private readonly ACHomebrewGame fixture;

        public HighestSpellbookClassLevelTests(ACHomebrewGame fixture) => this.fixture = fixture;

        /// <summary>Wizard 8 with Cleric 4 and Fighter 4 stays 8: the highest spellbook class wins, nothing is summed.</summary>
        [Fact]
        public void PropertyIsTheHighestSpellbookClassLevelNeverASum()
        {
            fixture.RequireGame();
            var unit = OfflineGame.CreateUnit(Dretch);
            var monsterLevels = unit.Descriptor.Progression.CharacterLevel;
            AddClassLevels(unit, WizardClass, 8);
            AddClassLevels(unit, ClericClass, 4);
            AddClassLevels(unit, FighterClass, 4);

            Assert.Equal(8, Property(unit));
            // The fighter levels raise the character level like any other, yet never the property.
            Assert.Equal(monsterLevels + 16, unit.Descriptor.Progression.CharacterLevel);
        }

        /// <summary>The vanilla pregen (a cavalier, no spellbook class) reads 0 and gains nothing from the feats.</summary>
        [Fact]
        public void UnitWithoutASpellbookClassGainsNothing()
        {
            fixture.RequireGame();
            var unit = UnitWithClasses();

            Assert.Equal(0, Property(unit));

            var aegis = OfflineGame.Blueprint<BlueprintFeature>(Guids.Derived.ArcaneAegis);
            var vitality = OfflineGame.Blueprint<BlueprintFeature>(Guids.Derived.MysticVitality);
            unit.Progression.Features.AddFeature(aegis);
            unit.Progression.Features.AddFeature(vitality);

            Assert.Equal(0, ModifierFrom(unit.Stats.AC, aegis)?.ModValue ?? 0);
            Assert.Equal(0, ModifierFrom(unit.Stats.HitPoints, vitality)?.ModValue ?? 0);

            ApplySoulBulwarkBuff(unit);
            Assert.Equal(0, ModifierFrom(unit.Stats.TemporaryHitPoints, Buff())?.ModValue ?? 0);
        }

        /// <summary>Arcane Aegis on a Wizard 8 adds half the highest spellbook class level to AC.</summary>
        [Fact]
        public void ArcaneAegisAddsHalfTheHighestSpellbookClassLevel()
        {
            fixture.RequireGame();
            var unit = UnitWithClasses((WizardClass, 8));

            var aegis = OfflineGame.Blueprint<BlueprintFeature>(Guids.Derived.ArcaneAegis);
            unit.Progression.Features.AddFeature(aegis);

            Assert.Equal(4, ModifierFrom(unit.Stats.AC, aegis)?.ModValue ?? 0);
        }

        /// <summary>Ley-Infused Vitality on a Wizard 8 adds the highest spellbook class level to Hit Points.</summary>
        [Fact]
        public void MysticVitalityAddsTheHighestSpellbookClassLevel()
        {
            fixture.RequireGame();
            var unit = UnitWithClasses((WizardClass, 8));

            var vitality = OfflineGame.Blueprint<BlueprintFeature>(Guids.Derived.MysticVitality);
            unit.Progression.Features.AddFeature(vitality);

            Assert.Equal(8, ModifierFrom(unit.Stats.HitPoints, vitality)?.ModValue ?? 0);
        }

        /// <summary>The Soul Bulwark combat buff grants temporary hit points equal to the highest spellbook class level.</summary>
        [Fact]
        public void SoulBulwarkBuffGrantsTemporaryHitPointsEqualToTheHighestSpellbookClassLevel()
        {
            fixture.RequireGame();
            var unit = UnitWithClasses((WizardClass, 8));

            ApplySoulBulwarkBuff(unit);

            Assert.Equal(8, ModifierFrom(unit.Stats.TemporaryHitPoints, Buff())?.ModValue ?? 0);
        }

        /// <summary>
        /// Wizard 4 gains +2 AC and +4 HP; four more wizard levels raise both only because the
        /// feats reapply on level up — the game's level-up tail call is what refreshes them.
        /// </summary>
        [Fact]
        public void LevelUpRefreshesTheBonusesThroughReapplyOnLevelUp()
        {
            fixture.RequireGame();
            var unit = UnitWithClasses((WizardClass, 4));
            var aegis = OfflineGame.Blueprint<BlueprintFeature>(Guids.Derived.ArcaneAegis);
            var vitality = OfflineGame.Blueprint<BlueprintFeature>(Guids.Derived.MysticVitality);
            Assert.True(aegis.ReapplyOnLevelUp, "Arcane Aegis must reapply on level up.");
            Assert.True(vitality.ReapplyOnLevelUp, "Ley-Infused Vitality must reapply on level up.");

            unit.Progression.Features.AddFeature(aegis);
            unit.Progression.Features.AddFeature(vitality);
            Assert.Equal(2, ModifierFrom(unit.Stats.AC, aegis)?.ModValue ?? 0);
            Assert.Equal(4, ModifierFrom(unit.Stats.HitPoints, vitality)?.ModValue ?? 0);

            AddClassLevels(unit, WizardClass, 4);
            Assert.Equal(2, ModifierFrom(unit.Stats.AC, aegis)?.ModValue ?? 0); // stale until the level-up tail call
            unit.Descriptor.Progression.ReapplyFeaturesOnLevelUp();

            Assert.Equal(4, ModifierFrom(unit.Stats.AC, aegis)?.ModValue ?? 0);
            Assert.Equal(8, ModifierFrom(unit.Stats.HitPoints, vitality)?.ModValue ?? 0);
        }

        /// <summary>The dretch with the given class levels, each added with the game's own AddClassLevel.</summary>
        private static UnitEntityData UnitWithClasses(params (string Class, int Levels)[] classes)
        {
            var unit = OfflineGame.CreateUnit(Dretch);
            foreach (var (characterClass, levels) in classes)
            {
                AddClassLevels(unit, characterClass, levels);
            }
            return unit;
        }

        private static void AddClassLevels(UnitEntityData unit, string characterClass, int levels)
        {
            var blueprint = OfflineGame.Blueprint<BlueprintCharacterClass>(characterClass);
            for (var i = 0; i < levels; i++)
            {
                unit.Descriptor.Progression.AddClassLevel(blueprint);
            }
        }

        private static int Property(UnitEntityData unit)
            => OfflineGame.Blueprint<BlueprintUnitProperty>(Guids.Derived.HighestSpellbookClassLevelProperty).GetInt(unit);

        private static BlueprintBuff Buff() => OfflineGame.Blueprint<BlueprintBuff>(SoulBulwarkTempBuff);

        /// <summary>Applies the feat's combat buff to the unit itself, the game's own caster-based AddBuff.</summary>
        private static void ApplySoulBulwarkBuff(UnitEntityData unit)
        {
            unit.AddBuff(Buff(), unit, TimeSpan.FromMinutes(10));
        }

        /// <summary>The single modifier the fact adds to the stat, or null when it adds none.</summary>
        private static ModifiableValue.Modifier ModifierFrom(ModifiableValue stat, BlueprintScriptableObject fact)
        {
            var modifiers = stat.Modifiers.Where(m => m.Source?.Blueprint == fact).ToList();
            Assert.True(modifiers.Count <= 1, $"The fact should add at most one modifier to {stat}, found {modifiers.Count}.");
            return modifiers.SingleOrDefault();
        }
    }
}
