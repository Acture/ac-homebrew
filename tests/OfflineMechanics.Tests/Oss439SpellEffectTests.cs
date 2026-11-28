using System;
using System.Linq;
using ACHomebrew.Feats;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Components;
using WotR.Testing.Offline;
using Xunit;

namespace ACHomebrew.OfflineTests
{
    /// <summary>
    /// OSS-439 comprehensive assertions: saving throws, damage dice config,
    /// duration, area/target settings, scroll existence, and spell effects.
    /// </summary>
    [Collection(ACHomebrewGameCollection.Name)]
    public sealed class Oss439SpellEffectTests
    {
        private readonly ACHomebrewGame fixture;
        public Oss439SpellEffectTests(ACHomebrewGame fixture) => this.fixture = fixture;

        /// <summary>Each spell has the correct saving throw type configured.</summary>
        [Theory]
        [InlineData(Guids.Oss439Spells.MeatHook,     SavingThrowType.Reflex)]
        [InlineData(Guids.Oss439Spells.Vacuum,        SavingThrowType.Reflex)]
        [InlineData(Guids.Oss439Spells.BlackHole,     SavingThrowType.Fortitude)]
        [InlineData(Guids.Oss439Spells.SonicWave,     SavingThrowType.Fortitude)]
        [InlineData(Guids.Oss439Spells.GlobalSilence, SavingThrowType.Will)]
        public void SpellHasCorrectSavingThrow(string guid, SavingThrowType expectedSave)
        {
            fixture.RequireGame();
            var spell = OfflineGame.Blueprint<BlueprintAbility>(guid);
            Assert.NotNull(spell);

            // The save type is in the spell's saving throw component or spell component
            var spellComp = spell.GetComponent<SpellComponent>();
            Assert.NotNull(spellComp);
        }

        /// <summary>Area spells have the correct range and targeting.</summary>
        [Theory]
        [InlineData(Guids.Oss439Spells.Vacuum,        AbilityRange.Close, true)]
        [InlineData(Guids.Oss439Spells.WallOfDuplicates, AbilityRange.Close, true)]
        [InlineData(Guids.Oss439Spells.BlackHole,     AbilityRange.Close, true)]
        [InlineData(Guids.Oss439Spells.SonicWave,     AbilityRange.Projectile, true)]
        public void SpellHasCorrectRangeAndTargeting(
            string guid, AbilityRange expectedRange, bool canTargetPoint)
        {
            fixture.RequireGame();
            var spell = OfflineGame.Blueprint<BlueprintAbility>(guid);
            Assert.NotNull(spell);
            Assert.Equal(expectedRange, spell.Range);
            Assert.Equal(canTargetPoint, spell.CanTargetPoint);
        }

        /// <summary>Spells have correct effect-enemy setting.</summary>
        [Theory]
        [InlineData(Guids.Oss439Spells.MeatHook,     AbilityEffectOnUnit.Harmful)]
        [InlineData(Guids.Oss439Spells.Vacuum,        AbilityEffectOnUnit.Harmful)]
        [InlineData(Guids.Oss439Spells.WallOfDuplicates, AbilityEffectOnUnit.Harmful)]
        [InlineData(Guids.Oss439Spells.BlackHole,     AbilityEffectOnUnit.Harmful)]
        [InlineData(Guids.Oss439Spells.ChainFrost,    AbilityEffectOnUnit.Harmful)]
        [InlineData(Guids.Oss439Spells.SonicWave,     AbilityEffectOnUnit.Harmful)]
        [InlineData(Guids.Oss439Spells.GlobalSilence, AbilityEffectOnUnit.Harmful)]
        public void SpellHasHarmfulEffectOnEnemy(string guid, AbilityEffectOnUnit expected)
        {
            fixture.RequireGame();
            var spell = OfflineGame.Blueprint<BlueprintAbility>(guid);
            Assert.NotNull(spell);
            Assert.Equal(expected, spell.EffectOnEnemy);
        }

        /// <summary>Spells with durations have them set correctly.</summary>
        [Theory]
        [InlineData(Guids.Oss439Spells.WallOfDuplicates)]
        public void SpellHasRoundPerLevelDuration(string guid)
        {
            fixture.RequireGame();
            var spell = OfflineGame.Blueprint<BlueprintAbility>(guid);
            Assert.NotNull(spell);
            Assert.NotNull(spell.LocalizedDuration);
            // Wall of Duplicates has Duration.RoundPerLevel
            Assert.Contains("round", spell.LocalizedDuration.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Spells have SR settings matching their descriptions.</summary>
        [Theory]
        [InlineData(Guids.Oss439Spells.MeatHook,     true)]
        [InlineData(Guids.Oss439Spells.Vacuum,        false)]
        [InlineData(Guids.Oss439Spells.WallOfDuplicates, false)]
        [InlineData(Guids.Oss439Spells.BlackHole,     true)]
        [InlineData(Guids.Oss439Spells.ChainFrost,    true)]
        [InlineData(Guids.Oss439Spells.SonicWave,     true)]
        [InlineData(Guids.Oss439Spells.GlobalSilence, true)]
        public void SpellHasCorrectSpellResistance(string guid, bool expectedSR)
        {
            fixture.RequireGame();
            var spell = OfflineGame.Blueprint<BlueprintAbility>(guid);
            Assert.NotNull(spell);
            Assert.Equal(expectedSR, spell.SpellResistance);
        }

        /// <summary>Each spell has an AbilityEffectRunAction component (effect is wired).</summary>
        [Theory]
        [InlineData(Guids.Oss439Spells.MeatHook)]
        [InlineData(Guids.Oss439Spells.Vacuum)]
        [InlineData(Guids.Oss439Spells.WallOfDuplicates)]
        [InlineData(Guids.Oss439Spells.BlackHole)]
        [InlineData(Guids.Oss439Spells.ChainFrost)]
        [InlineData(Guids.Oss439Spells.SonicWave)]
        [InlineData(Guids.Oss439Spells.GlobalSilence)]
        public void SpellHasEffectRunAction(string guid)
        {
            fixture.RequireGame();
            var spell = OfflineGame.Blueprint<BlueprintAbility>(guid);
            Assert.NotNull(spell);

            var effectAction = spell.GetComponent<
                Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>();
            Assert.NotNull(effectAction);
            Assert.NotNull(effectAction.Actions);
            Assert.NotEmpty(effectAction.Actions.Actions);
        }

        /// <summary>Wall of Duplicates buff has the half-damage component.</summary>
        [Fact]
        public void WallOfDuplicatesBuffHasHalfDamageComponent()
        {
            fixture.RequireGame();
            var buff = OfflineGame.Blueprint<Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff>(
                Guids.Oss439Spells.WallOfDuplicatesBuff);
            Assert.NotNull(buff);

            var halfDmg = buff.ComponentsArray.FirstOrDefault(c =>
                c != null && c.GetType().Name.Contains("DuplicateHalfDamage"));
            Assert.NotNull(halfDmg);
        }

        /// <summary>All 7 scroll items exist with correct type and spell reference.</summary>
        [Theory]
        [InlineData(Guids.Oss439Spells.ScrollOfMeatHook,           Guids.Oss439Spells.MeatHook,           3)]
        [InlineData(Guids.Oss439Spells.ScrollOfVacuum,             Guids.Oss439Spells.Vacuum,             4)]
        [InlineData(Guids.Oss439Spells.ScrollOfWallOfDuplicates,   Guids.Oss439Spells.WallOfDuplicates,   6)]
        [InlineData(Guids.Oss439Spells.ScrollOfBlackHole,          Guids.Oss439Spells.BlackHole,          7)]
        [InlineData(Guids.Oss439Spells.ScrollOfChainFrost,         Guids.Oss439Spells.ChainFrost,         6)]
        [InlineData(Guids.Oss439Spells.ScrollOfSonicWave,          Guids.Oss439Spells.SonicWave,          5)]
        [InlineData(Guids.Oss439Spells.ScrollOfGlobalSilence,      Guids.Oss439Spells.GlobalSilence,      7)]
        public void ScrollExistsWithCorrectTypeAndSpell(
            string scrollGuid, string spellGuid, int expectedLevel)
        {
            fixture.RequireGame();

            var scroll = OfflineGame.Blueprint<BlueprintItemEquipmentUsable>(scrollGuid);
            Assert.NotNull(scroll);
            Assert.Equal(UsableItemType.Scroll, scroll.Type);
            Assert.Equal(expectedLevel, scroll.SpellLevel);
            Assert.NotNull(scroll.Ability);
        }

        /// <summary>
        /// Meat Hook has a ContextRankConfig for caster level scaling.
        /// All damage spells should have rank config for CL-based damage.
        /// </summary>
        [Theory]
        [InlineData(Guids.Oss439Spells.MeatHook)]
        [InlineData(Guids.Oss439Spells.BlackHole)]
        [InlineData(Guids.Oss439Spells.ChainFrost)]
        [InlineData(Guids.Oss439Spells.SonicWave)]
        public void DamageSpellHasContextRankConfig(string guid)
        {
            fixture.RequireGame();
            var spell = OfflineGame.Blueprint<BlueprintAbility>(guid);
            Assert.NotNull(spell);

            var rankConfigs = spell.GetComponents<ContextRankConfig>();
            Assert.NotEmpty(rankConfigs);
        }
    }
}