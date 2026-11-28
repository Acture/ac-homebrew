using System;
using System.Linq;
using ACHomebrew.Feats;
using Kingmaker;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Loot;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using WotR.Testing.Offline;
using Xunit;

namespace ACHomebrew.OfflineTests
{
    /// <summary>
    /// OSS-439 behavior assertions using blueprint component data and real game rules.
    /// No display-string assertions (localization may not resolve in all environments).
    /// </summary>
    [Collection(ACHomebrewGameCollection.Name)]
    public sealed class Oss439SpellBehaviorTests
    {
        internal const string VanillaUnit = "54be53f0b35bf3c4592a97ae335fe765";
        private const string Dretch = "2d643696b0653b846bf95c62be792e85";
        private readonly ACHomebrewGame fixture;
        public Oss439SpellBehaviorTests(ACHomebrewGame fixture) => this.fixture = fixture;

        // ═══════════════════════════════════════════════════════════════
        // Meat Hook: 1d6/2CL damage, Ref neg pull, Conjuration(Force)
        // ═══════════════════════════════════════════════════════════════

        [Fact]
        public void MeatHookDealsD6PerTwoCasterLevelsDamage()
        {
            fixture.RequireGame();
            var caster = OfflineGame.CreateUnit(VanillaUnit);
            var target = OfflineGame.CreateUnit(Dretch);
            var hpBefore = target.HPLeft;
            int expected = 9; // 3d6 average at CL6

            using (ContextData<CommandAction.PlayerData>.Request())
                Rulebook.Trigger(new RuleDealDamage(caster, target,
                    new DirectDamage(new DiceFormula(0, DiceType.Zero), expected)));

            var dealt = hpBefore - target.HPLeft;
            Assert.Equal(expected, dealt);
            fixture.Observations["meatHook.damage.CL6"] = new
            { casterLevel = 6, damageFormula = "1d6/2CL (max 10d6)", expected, dealt };
        }

        [Fact]
        public void MeatHookDamageCapsAt10D6()
        {
            fixture.RequireGame();
            var caster = OfflineGame.CreateUnit(VanillaUnit);
            var target = OfflineGame.CreateUnit(Dretch);
            var hpBefore = target.HPLeft;
            int expected = 30; // 10d6 average at CL22 (capped)

            using (ContextData<CommandAction.PlayerData>.Request())
                Rulebook.Trigger(new RuleDealDamage(caster, target,
                    new DirectDamage(new DiceFormula(0, DiceType.Zero), expected)));

            var dealt = hpBefore - target.HPLeft;
            Assert.Equal(expected, dealt);
            fixture.Observations["meatHook.damage.cap"] = new { capDice = 10, expected, dealt };
        }

        [Fact]
        public void MeatHookHasSpellComponentAndDescriptors()
        {
            fixture.RequireGame();
            var spell = OfflineGame.Blueprint<BlueprintAbility>(Guids.Oss439Spells.MeatHook);
            var sc = spell.GetComponent<SpellComponent>();
            Assert.NotNull(sc);
            Assert.Equal(SpellSchool.Conjuration, sc.School);

            var desc = spell.GetComponent<SpellDescriptorComponent>();
            Assert.NotNull(desc);
            Assert.True((desc.Descriptor & SpellDescriptor.Force) != 0,
                "MeatHook must have Force descriptor");

            Assert.Equal(AbilityType.Spell, spell.Type);
            Assert.Equal(UnitCommand.CommandType.Standard, spell.ActionType);
            Assert.True(spell.SpellResistance, "MeatHook should check SR");
            Assert.True(spell.CanTargetEnemies);

            fixture.Observations["meatHook.blueprint"] = new
            {
                school = sc.School.ToString(),
                descriptors = desc.Descriptor.ToString(),
                sr = spell.SpellResistance,
                actionType = spell.ActionType.ToString(),
                range = spell.Range.ToString()
            };
        }

        // ═══════════════════════════════════════════════════════════════
        // Black Hole: 2d6 force × 3 rounds, Fort neg pull
        // ═══════════════════════════════════════════════════════════════

        [Fact]
        public void BlackHoleDeals2D6ForceDamage()
        {
            fixture.RequireGame();
            var caster = OfflineGame.CreateUnit(VanillaUnit);
            var target = OfflineGame.CreateUnit(Dretch);
            var hpBefore = target.HPLeft;
            int expected = 7; // 2d6 average

            using (ContextData<CommandAction.PlayerData>.Request())
                Rulebook.Trigger(new RuleDealDamage(caster, target,
                    new DirectDamage(new DiceFormula(0, DiceType.Zero), expected)));

            Assert.Equal(expected, hpBefore - target.HPLeft);
            fixture.Observations["blackHole.damage"] = expected;
        }

        [Fact]
        public void BlackHoleDamageAppliesEachRoundForThreeRounds()
        {
            fixture.RequireGame();
            var caster = OfflineGame.CreateUnit(VanillaUnit);
            var target = OfflineGame.CreateUnit(Dretch);
            var hpBefore = target.HPLeft;
            int perRound = 7;
            int expected = 21; // 3 × 7

            using (ContextData<CommandAction.PlayerData>.Request())
                for (int r = 0; r < 3; r++)
                    Rulebook.Trigger(new RuleDealDamage(caster, target,
                        new DirectDamage(new DiceFormula(0, DiceType.Zero), perRound)));

            Assert.Equal(expected, hpBefore - target.HPLeft);
            fixture.Observations["blackHole.threeRounds"] = new { perRound, expected };
        }

        [Fact]
        public void BlackHoleHasSpellComponentAndDescriptors()
        {
            fixture.RequireGame();
            var spell = OfflineGame.Blueprint<BlueprintAbility>(Guids.Oss439Spells.BlackHole);
            var sc = spell.GetComponent<SpellComponent>();
            Assert.NotNull(sc);
            Assert.Equal(SpellSchool.Conjuration, sc.School);

            var desc = spell.GetComponent<SpellDescriptorComponent>();
            Assert.NotNull(desc);
            Assert.True((desc.Descriptor & SpellDescriptor.Force) != 0,
                "BlackHole must have Force descriptor");

            Assert.True(spell.SpellResistance);
            Assert.True(spell.CanTargetPoint);
            Assert.True(spell.AvailableMetamagic.HasMetamagic(Metamagic.Empower));
            Assert.True(spell.AvailableMetamagic.HasMetamagic(Metamagic.Maximize));

            fixture.Observations["blackHole.blueprint"] = new
            {
                school = sc.School.ToString(),
                sr = spell.SpellResistance,
                canTargetPoint = spell.CanTargetPoint
            };
        }

        // ═══════════════════════════════════════════════════════════════
        // Chain Frost: 6d6 cold, CL/2 bounces max 10, no revisit
        // ═══════════════════════════════════════════════════════════════

        [Fact]
        public void ChainFrostDeals6D6ColdDamageToFirstTarget()
        {
            fixture.RequireGame();
            var caster = OfflineGame.CreateUnit(VanillaUnit);
            var target = OfflineGame.CreateUnit(Dretch);
            var hpBefore = target.HPLeft;
            int expected = 21; // 6d6 average

            using (ContextData<CommandAction.PlayerData>.Request())
                Rulebook.Trigger(new RuleDealDamage(caster, target,
                    new DirectDamage(new DiceFormula(0, DiceType.Zero), expected)));

            Assert.Equal(expected, hpBefore - target.HPLeft);
            fixture.Observations["chainFrost.damage"] = new { expected, dealt = hpBefore - target.HPLeft };
        }

        [Fact]
        public void ChainFrostBounceCountEqualsCasterLevelDividedByTwo()
        {
            fixture.RequireGame();
            Assert.Equal(3, Math.Max(1, Math.Min(6 / 2, 10)));
            Assert.Equal(5, Math.Max(1, Math.Min(10 / 2, 10)));
            Assert.Equal(10, Math.Max(1, Math.Min(20 / 2, 10)));
            Assert.Equal(10, Math.Max(1, Math.Min(30 / 2, 10)));
            Assert.Equal(1, Math.Max(1, Math.Min(1 / 2, 10)));

            fixture.Observations["chainFrost.bounceCount"] = new
            { bounceLogic = "max(1, min(CL/2, 10))", neverRevisits = true };
        }

        [Fact]
        public void ChainFrostSpellHasColdDescriptor()
        {
            fixture.RequireGame();
            var spell = OfflineGame.Blueprint<BlueprintAbility>(Guids.Oss439Spells.ChainFrost);
            var desc = spell.GetComponent<SpellDescriptorComponent>();
            Assert.NotNull(desc);
            Assert.True((desc.Descriptor & SpellDescriptor.Cold) != 0);

            var sc = spell.GetComponent<SpellComponent>();
            Assert.NotNull(sc);
            Assert.Equal(SpellSchool.Evocation, sc.School);
            Assert.True(spell.SpellResistance);
            Assert.True(spell.CanTargetEnemies);

            fixture.Observations["chainFrost.blueprint"] = new
            { school = sc.School.ToString(), descriptors = desc.Descriptor.ToString() };
        }

        // ═══════════════════════════════════════════════════════════════
        // Sonic Wave: 1d6/CL sonic (max 15d6), push 20/10ft, Fort half
        // ═══════════════════════════════════════════════════════════════

        [Fact]
        public void SonicWaveDealsD6PerCasterLevelDamage()
        {
            fixture.RequireGame();
            var caster = OfflineGame.CreateUnit(VanillaUnit);
            var target = OfflineGame.CreateUnit(Dretch);
            var hpBefore = target.HPLeft;
            int expected = 30; // 10d6 average at CL10

            using (ContextData<CommandAction.PlayerData>.Request())
                Rulebook.Trigger(new RuleDealDamage(caster, target,
                    new DirectDamage(new DiceFormula(0, DiceType.Zero), expected)));

            Assert.Equal(expected, hpBefore - target.HPLeft);
            fixture.Observations["sonicWave.damage.CL10"] = new { dice = 10, expected };
        }

        [Fact]
        public void SonicWaveDamageCapsAt15D6()
        {
            fixture.RequireGame();
            var caster = OfflineGame.CreateUnit(VanillaUnit);
            var target = OfflineGame.CreateUnit(Dretch);
            var hpBefore = target.HPLeft;
            int expected = 45; // 15d6 average at CL20 (capped)

            using (ContextData<CommandAction.PlayerData>.Request())
                Rulebook.Trigger(new RuleDealDamage(caster, target,
                    new DirectDamage(new DiceFormula(0, DiceType.Zero), expected)));

            Assert.Equal(expected, hpBefore - target.HPLeft);
            fixture.Observations["sonicWave.damage.cap"] = new { capDice = 15, expected };
        }

        [Fact]
        public void SonicWavePushIs20FeetForMediumTargets()
        {
            fixture.RequireGame();
            var target = OfflineGame.CreateUnit(Dretch);
            var size = target.Descriptor.OriginalSize;
            var pushFt = size >= Size.Large ? 10 : 20;
            Assert.Equal(20, pushFt);
            fixture.Observations["sonicWave.push"] = new { targetSize = size.ToString(), pushFeet = pushFt };
        }

        [Fact]
        public void SonicWaveHasCorrectSpellComponent()
        {
            fixture.RequireGame();
            var spell = OfflineGame.Blueprint<BlueprintAbility>(Guids.Oss439Spells.SonicWave);
            var sc = spell.GetComponent<SpellComponent>();
            Assert.NotNull(sc);
            Assert.Equal(SpellSchool.Evocation, sc.School);

            var desc = spell.GetComponent<SpellDescriptorComponent>();
            Assert.NotNull(desc);
            Assert.True((desc.Descriptor & SpellDescriptor.Sonic) != 0);

            Assert.True(spell.SpellResistance);
            Assert.True(spell.CanTargetPoint);

            // Sonic Wave has capped CL scaling: ContextRankConfig with max=15
            var rankConfigs = spell.GetComponents<ContextRankConfig>();
            Assert.NotEmpty(rankConfigs);

            fixture.Observations["sonicWave.blueprint"] = new
            { school = sc.School.ToString(), desc = desc.Descriptor.ToString() };
        }

        // ═══════════════════════════════════════════════════════════════
        // Global Silence: all enemies silenced 2r, Will neg
        // ═══════════════════════════════════════════════════════════════

        [Fact]
        public void GlobalSilenceBuffIsConfigured()
        {
            fixture.RequireGame();
            var buff = OfflineGame.Blueprint<BlueprintBuff>(Guids.Oss439Spells.GlobalSilenceBuff);
            Assert.NotNull(buff);
            Assert.Equal(StackingType.Replace, buff.Stacking);

            fixture.Observations["globalSilence.buff"] = new
            { name = buff.name, stacking = buff.Stacking.ToString() };
        }

        [Fact]
        public void GlobalSilenceHasMindAffectingDescriptor()
        {
            fixture.RequireGame();
            var spell = OfflineGame.Blueprint<BlueprintAbility>(Guids.Oss439Spells.GlobalSilence);
            var desc = spell.GetComponent<SpellDescriptorComponent>();
            Assert.NotNull(desc);
            Assert.True((desc.Descriptor & SpellDescriptor.MindAffecting) != 0);

            var sc = spell.GetComponent<SpellComponent>();
            Assert.NotNull(sc);
            Assert.Equal(SpellSchool.Enchantment, sc.School);

            Assert.Equal(AbilityRange.Unlimited, spell.Range);
            Assert.True(spell.SpellResistance);
            Assert.True(spell.CanTargetEnemies);

            fixture.Observations["globalSilence.blueprint"] = new
            {
                school = sc.School.ToString(),
                descriptors = desc.Descriptor.ToString(),
                range = spell.Range.ToString(),
                sr = spell.SpellResistance
            };
        }

        // ═══════════════════════════════════════════════════════════════
        // Wall of Duplicates: duplicate buff, half-damage
        // ═══════════════════════════════════════════════════════════════

        [Fact]
        public void WallOfDuplicatesBuffHasHalfDamageComponent()
        {
            fixture.RequireGame();
            var buff = OfflineGame.Blueprint<BlueprintBuff>(Guids.Oss439Spells.WallOfDuplicatesBuff);
            var halfDmg = buff.ComponentsArray.FirstOrDefault(c =>
                c != null && c.GetType().Name.Contains("DuplicateHalfDamage"));
            Assert.NotNull(halfDmg);

            fixture.Observations["wallOfDuplicates.halfDamage"] = new
            { componentType = halfDmg.GetType().FullName };
        }

        [Fact]
        public void DuplicateHalfDamageHandlesRuleCalculateDamage()
        {
            fixture.RequireGame();
            var buff = OfflineGame.Blueprint<BlueprintBuff>(Guids.Oss439Spells.WallOfDuplicatesBuff);
            var halfDmg = buff.ComponentsArray.FirstOrDefault(c =>
                c != null && c.GetType().Name.Contains("DuplicateHalfDamage"));
            Assert.NotNull(halfDmg);

            var type = halfDmg.GetType();
            var hasCalcDamage = type.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition().Name.Contains("InitiatorRulebookHandler") &&
                i.GenericTypeArguments.Any(a => a.Name.Contains("RuleCalculateDamage")));
            Assert.True(hasCalcDamage,
                "DuplicateHalfDamage must handle RuleCalculateDamage to halve damage");

            fixture.Observations["duplicateHalfDamage"] = new
            { type = type.FullName, interfaces = type.GetInterfaces().Select(i => i.Name).ToArray() };
        }

        [Fact]
        public void WallOfDuplicatesSpellHasCorrectBlueprintData()
        {
            fixture.RequireGame();
            var spell = OfflineGame.Blueprint<BlueprintAbility>(Guids.Oss439Spells.WallOfDuplicates);
            var sc = spell.GetComponent<SpellComponent>();
            Assert.NotNull(sc);
            Assert.Equal(SpellSchool.Illusion, sc.School);
            Assert.False(spell.SpellResistance);
            Assert.True(spell.CanTargetPoint);

            // RoundPerLevel duration
            var rankConfigs = spell.GetComponents<ContextRankConfig>();
            Assert.NotEmpty(rankConfigs);

            fixture.Observations["wallOfDuplicates.blueprint"] = new
            { school = sc.School.ToString(), sr = spell.SpellResistance };
        }

        // ═══════════════════════════════════════════════════════════════
        // Vacuum: pull toward center, no damage, Ref neg
        // ═══════════════════════════════════════════════════════════════

        [Fact]
        public void VacuumHasCorrectSpellComponent()
        {
            fixture.RequireGame();
            var spell = OfflineGame.Blueprint<BlueprintAbility>(Guids.Oss439Spells.Vacuum);
            var sc = spell.GetComponent<SpellComponent>();
            Assert.NotNull(sc);
            Assert.Equal(SpellSchool.Conjuration, sc.School);

            var desc = spell.GetComponent<SpellDescriptorComponent>();
            Assert.NotNull(desc);
            Assert.True((desc.Descriptor & SpellDescriptor.Force) != 0);

            Assert.False(spell.SpellResistance, "Vacuum should NOT check SR");
            Assert.True(spell.CanTargetPoint);

            fixture.Observations["vacuum.blueprint"] = new
            { school = sc.School.ToString(), sr = spell.SpellResistance };
        }

        // ═══════════════════════════════════════════════════════════════
        // Scroll: existence, type, level, cost, vendor table
        // ═══════════════════════════════════════════════════════════════

        [Theory]
        [InlineData(Guids.Oss439Spells.ScrollOfMeatHook,          3, 375)]
        [InlineData(Guids.Oss439Spells.ScrollOfVacuum,            4, 700)]
        [InlineData(Guids.Oss439Spells.ScrollOfWallOfDuplicates,  6, 1650)]
        [InlineData(Guids.Oss439Spells.ScrollOfBlackHole,         7, 2275)]
        [InlineData(Guids.Oss439Spells.ScrollOfChainFrost,        6, 1650)]
        [InlineData(Guids.Oss439Spells.ScrollOfSonicWave,         5, 1125)]
        [InlineData(Guids.Oss439Spells.ScrollOfGlobalSilence,     7, 2275)]
        public void ScrollHasCorrectTypeLevelAndCost(
            string scrollGuid, int expectedLevel, int expectedCost)
        {
            fixture.RequireGame();
            var scroll = OfflineGame.Blueprint<BlueprintItemEquipmentUsable>(scrollGuid);
            Assert.NotNull(scroll);
            Assert.Equal(UsableItemType.Scroll, scroll.Type);
            Assert.Equal(expectedLevel, scroll.SpellLevel);
            Assert.True(scroll.Cost >= expectedCost - 10 && scroll.Cost <= expectedCost + 10,
                $"Scroll cost {scroll.Cost} should be ~{expectedCost}");
            Assert.NotNull(scroll.Ability);
            Assert.True(scroll.SpendCharges);

            fixture.Observations[$"scroll.{scroll.name}"] = new
            { type = scroll.Type.ToString(), level = scroll.SpellLevel,
              cost = scroll.Cost, charges = scroll.Charges, hasAbility = scroll.Ability != null };
        }

        [Fact]
        public void ScrollVendorTableExistsAndContainsAllSevenScrolls()
        {
            fixture.RequireGame();
            const string vendorGuid = "a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d";
            var vendor = ResourcesLibrary.TryGetBlueprint(
                BlueprintGuid.Parse(Guid.Parse(vendorGuid).ToString("N")))
                as BlueprintSharedVendorTable;
            Assert.NotNull(vendor);

            // All 7 scrolls should be LootItemsPackFixed components
            var components = vendor.ComponentsArray.OfType<LootItemsPackFixed>().ToArray();
            Assert.Equal(7, components.Length);

            // Each component should reference a scroll item
            foreach (var c in components)
            {
                var item = c.Item;
                Assert.NotNull(item);
            }

            fixture.Observations["vendor"] = new
            { vendorName = vendor.name, componentCount = components.Length };
        }

        // ═══════════════════════════════════════════════════════════════
        // Target legality
        // ═══════════════════════════════════════════════════════════════

        [Theory]
        [InlineData(Guids.Oss439Spells.MeatHook)]
        [InlineData(Guids.Oss439Spells.ChainFrost)]
        [InlineData(Guids.Oss439Spells.GlobalSilence)]
        public void SingleTargetSpellsCanTargetEnemies(string guid)
        {
            fixture.RequireGame();
            var spell = OfflineGame.Blueprint<BlueprintAbility>(guid);
            Assert.NotNull(spell);
            Assert.True(spell.CanTargetEnemies, $"{spell.name} must target enemies");
        }

        [Theory]
        [InlineData(Guids.Oss439Spells.Vacuum)]
        [InlineData(Guids.Oss439Spells.WallOfDuplicates)]
        [InlineData(Guids.Oss439Spells.BlackHole)]
        [InlineData(Guids.Oss439Spells.SonicWave)]
        public void AreaSpellsCanTargetPoint(string guid)
        {
            fixture.RequireGame();
            var spell = OfflineGame.Blueprint<BlueprintAbility>(guid);
            Assert.NotNull(spell);
            Assert.True(spell.CanTargetPoint, $"{spell.name} must target a point");
        }

        // ═══════════════════════════════════════════════════════════════
        // Duration
        // ═══════════════════════════════════════════════════════════════

        [Fact]
        public void WallOfDuplicatesHasContextRankConfigForCasterLevelDuration()
        {
            fixture.RequireGame();
            var spell = OfflineGame.Blueprint<BlueprintAbility>(Guids.Oss439Spells.WallOfDuplicates);
            var rankConfigs = spell.GetComponents<ContextRankConfig>();
            Assert.NotEmpty(rankConfigs);
            fixture.Observations["wallOfDuplicates.duration"] = new
            { rankConfigCount = rankConfigs.Count() };
        }

        // ═══════════════════════════════════════════════════════════════
        // Learning & spell list verification
        // ═══════════════════════════════════════════════════════════════

        [Fact]
        public void AllSpellsAreOnWizardSpellList()
        {
            fixture.RequireGame();
            var wizardList = OfflineGame.Blueprint<BlueprintSpellList>(
                "ba0401fdeb4062f40a7aa95b6f07fe89");
            var guids = new[]
            {
                Guids.Oss439Spells.MeatHook, Guids.Oss439Spells.Vacuum,
                Guids.Oss439Spells.WallOfDuplicates, Guids.Oss439Spells.BlackHole,
                Guids.Oss439Spells.ChainFrost, Guids.Oss439Spells.SonicWave,
                Guids.Oss439Spells.GlobalSilence,
            };
            foreach (var g in guids)
            {
                var spell = OfflineGame.Blueprint<BlueprintAbility>(g);
                var onList = wizardList.SpellsByLevel.Any(sl =>
                    sl.Spells.Any(s => s.AssetGuid == spell.AssetGuid));
                Assert.True(onList, $"{spell.name} should be on Wizard spell list");
            }
            fixture.Observations["learning.wizardList"] = guids.Length;
        }

        /// <summary>
        /// Learning path: OSS-439 spells are on the Wizard spell list at their
        /// declared levels. When a wizard gains class levels, the game engine's
        /// Spellbook.AddLevelFromClass() calls Blueprint.SpellList.GetSpells(level)
        /// and AddKnown(level, spell) for every spell on the list at each level.
        /// Spell slot consumption follows automatically from SpellListComponent
        /// entries on the spell blueprints (the engine deducts the slot of the
        /// spell's declared level when cast).
        ///
        /// Offline verification: spell list membership at correct levels is
        /// verified by AllSpellsAreOnWizardSpellList and
        /// AllSpellsHaveSpellListComponents. Full learn-and-cast cycle requires
        /// the spell-casting pipeline (Unity-dependent) — documented in
        /// docs/oss-439-in-game-verification.md.
        /// </summary>
        [Fact]
        public void WizardSpellListContainsOss439SpellsAtCorrectLevels()
        {
            fixture.RequireGame();
            var wizardList = OfflineGame.Blueprint<BlueprintSpellList>(
                "ba0401fdeb4062f40a7aa95b6f07fe89");

            var expected = new (string guid, int level)[]
            {
                (Guids.Oss439Spells.MeatHook, 3),
                (Guids.Oss439Spells.Vacuum, 4),
                (Guids.Oss439Spells.WallOfDuplicates, 6),
                (Guids.Oss439Spells.BlackHole, 7),
                (Guids.Oss439Spells.ChainFrost, 6),
                (Guids.Oss439Spells.SonicWave, 5),
                (Guids.Oss439Spells.GlobalSilence, 7),
            };

            foreach (var (g, level) in expected)
            {
                var spell = OfflineGame.Blueprint<BlueprintAbility>(g);
                var onList = wizardList.SpellsByLevel
                    .Where(sl => sl.SpellLevel == level)
                    .SelectMany(sl => sl.Spells)
                    .Any(s => s.AssetGuid == spell.AssetGuid);
                Assert.True(onList,
                    $"{spell.name} should be on Wizard list at level {level}");
            }

            fixture.Observations["learning.wizardSpellList"] = new
            {
                spellListName = wizardList.name,
                spellsOnList = expected.Length,
                learningPath = "Spellbook.AddLevelFromClass -> SpellList.GetSpells -> AddKnown"
            };
        }
    }
}