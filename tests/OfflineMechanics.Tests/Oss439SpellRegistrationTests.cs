using System;
using System.Linq;
using ACHomebrew.Feats;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using WotR.Testing.Offline;
using Xunit;

namespace ACHomebrew.OfflineTests
{
    /// <summary>OSS-439: verify the 7 learnable spells are registered with correct school,
    /// level, and spell-list entries. Effect verification needs in-game testing.</summary>
    [Collection(ACHomebrewGameCollection.Name)]
    public sealed class Oss439SpellRegistrationTests
    {
        private readonly ACHomebrewGame fixture;
        public Oss439SpellRegistrationTests(ACHomebrewGame fixture) => this.fixture = fixture;

        /// <summary>The mod registry initialized successfully, which includes spell registration.</summary>
        [Fact]
        public void ModRegistryIncludesSpellFamilies()
        {
            fixture.RequireGame();
            Assert.Contains(fixture.Session.ModLog,
                line => line.Contains("ACHomebrew: registry initialized."));
        }

        /// <summary>Each OSS-439 spell blueprint exists and has type Spell.</summary>
        [Theory]
        [InlineData(Guids.Oss439Spells.MeatHook,     "MeatHook",     3, SpellSchool.Conjuration)]
        [InlineData(Guids.Oss439Spells.Vacuum,        "Vacuum",       4, SpellSchool.Conjuration)]
        [InlineData(Guids.Oss439Spells.WallOfDuplicates, "WallOfDuplicates", 6, SpellSchool.Illusion)]
        [InlineData(Guids.Oss439Spells.BlackHole,     "BlackHole",    7, SpellSchool.Conjuration)]
        [InlineData(Guids.Oss439Spells.ChainFrost,    "ChainFrost",   6, SpellSchool.Evocation)]
        [InlineData(Guids.Oss439Spells.SonicWave,     "SonicWave",    5, SpellSchool.Evocation)]
        [InlineData(Guids.Oss439Spells.GlobalSilence, "GlobalSilence",7, SpellSchool.Enchantment)]
        public void SpellBlueprintExistsWithCorrectSchool(
            string guid, string name, int expectedLevel, SpellSchool expectedSchool)
        {
            fixture.RequireGame();

            var spell = OfflineGame.Blueprint<BlueprintAbility>(guid);
            Assert.NotNull(spell);
            Assert.Equal(name, spell.name);
            Assert.Equal(AbilityType.Spell, spell.Type);

            var spellComp = spell.GetComponent<SpellComponent>();
            Assert.NotNull(spellComp);
            Assert.Equal(expectedSchool, spellComp.School);
        }

        /// <summary>Spells are on their declared spell lists at the right level.</summary>
        [Theory]
        [InlineData(Guids.Oss439Spells.MeatHook,    3)]
        [InlineData(Guids.Oss439Spells.Vacuum,       4)]
        [InlineData(Guids.Oss439Spells.WallOfDuplicates, 6)]
        [InlineData(Guids.Oss439Spells.BlackHole,    7)]
        [InlineData(Guids.Oss439Spells.ChainFrost,   6)]
        [InlineData(Guids.Oss439Spells.SonicWave,    5)]
        [InlineData(Guids.Oss439Spells.GlobalSilence,7)]
        public void SpellHasSpellListComponentEntries(string guid, int expectedLevel)
        {
            fixture.RequireGame();

            var spell = OfflineGame.Blueprint<BlueprintAbility>(guid);
            Assert.NotNull(spell);

            // BlueprintCore's AddToSpellLists adds SpellListComponent entries.
            var listEntries = spell.GetComponents<
                Kingmaker.Blueprints.Classes.Spells.SpellListComponent>();
            Assert.NotEmpty(listEntries);
            Assert.All(listEntries, entry => Assert.Equal(expectedLevel, entry.SpellLevel));
        }
    }
}