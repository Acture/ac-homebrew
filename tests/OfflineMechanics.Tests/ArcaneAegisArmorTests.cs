using System.Linq;
using ACHomebrew.Feats;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Shields;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using WotR.Testing.Offline;
using Xunit;

namespace ACHomebrew.OfflineTests
{
    /// <summary>
    /// Arcane Aegis (OSS-319) on a real vanilla character: the runtime armor and shield gate
    /// (<c>ArcaneAegisArmorPropertyGetter</c>) reads the worn armor and the held shield and decides
    /// whether the feat's AC bonus is applied. The bonus is a half-caster-level bonus to AC, so the
    /// test reads the AC modifier the feat's own stat bonus component adds.
    /// <para>
    /// The offline unit is the vanilla human pregen. Its equipment slots are empty, because the
    /// game's own starting equipment does not equip offline: the offline unit has no armor or
    /// weapon proficiencies, and the game asks the unit for them before it equips an item. The
    /// test therefore grants the proficiency each vanilla item needs and equips the item through
    /// the game's slot API, so the slot, the item and its proficiency group are the real ones.
    /// </para>
    /// <para>
    /// The feat's rank is half the highest spellbook class level, read from the unit through the
    /// mod's own unit property, so the test gives the pregen wizard levels with the game's own
    /// <c>UnitProgressionData.AddClassLevel</c>. The pregen carries the Angel mythic class at
    /// level 10, which has a spellbook; twelve wizard levels make the wizard the highest, and
    /// the expected bonus is half of that.
    /// </para>
    /// </summary>
    [Collection(ACHomebrewGameCollection.Name)]
    public sealed class ArcaneAegisArmorTests
    {
        // LeatherStandard, ChainmailStandard, HalfplateStandard and LightShield: vanilla items.
        private const string LightArmor = "9f76e9a3353e914479c5ddb4b4a82fb4";
        private const string MediumArmor = "02e9f83be5d1c5d4e927b5c44ed34840";
        private const string HeavyArmor = "ed6bbd7ecd050c04690fe11d4c3b3f7d";
        private const string LightShield = "a85d51d0fb905f940b951eec60388bac";

        // WizardClass: a vanilla class with a spellbook, added with the game's own AddClassLevel.
        // Twelve levels outrank the pregen's Angel mythic class (level 10, also a spellbook class).
        private const string WizardClass = "ba34257984f4c41408ce1dc2004e342e";
        private const int WizardLevels = 12;

        private readonly ACHomebrewGame fixture;

        public ArcaneAegisArmorTests(ACHomebrewGame fixture) => this.fixture = fixture;

        /// <summary>No armor and no shield: the bonus applies.</summary>
        [Fact]
        public void NoArmorAndNoShieldAddsTheBonus()
        {
            fixture.RequireGame();
            var unit = Wearer();

            Assert.Equal(ArmorProficiencyGroup.None, Armor(unit));
            Assert.False(unit.Body.PrimaryHand.HasShield);
            Assert.False(unit.Body.SecondaryHand.HasShield);

            var feat = Aegis(unit);
            Assert.True(feat.IsTurnedOn, "The feat should be on the unit.");
            Assert.Equal(WizardLevels / 2, AegisBonus(unit));
        }

        /// <summary>Light armor: the bonus still applies.</summary>
        [Fact]
        public void LightArmorKeepsTheBonus()
        {
            fixture.RequireGame();
            var unit = Wearer(armor: LightArmor);

            Assert.Equal(ArmorProficiencyGroup.Light, Armor(unit));
            Assert.False(unit.Body.SecondaryHand.HasShield);

            var feat = Aegis(unit);
            Assert.True(feat.IsTurnedOn, "The feat should be on the unit.");
            Assert.Equal(WizardLevels / 2, AegisBonus(unit));
        }

        /// <summary>Medium armor: the bonus does not apply.</summary>
        [Fact]
        public void MediumArmorRemovesTheBonus()
        {
            fixture.RequireGame();
            var unit = Wearer(armor: MediumArmor);

            Assert.Equal(ArmorProficiencyGroup.Medium, Armor(unit));

            var feat = Aegis(unit);
            Assert.True(feat.IsTurnedOn, "The feat should be on the unit.");
            Assert.Null(AegisModifier(unit));
        }

        /// <summary>Heavy armor: the bonus does not apply.</summary>
        [Fact]
        public void HeavyArmorRemovesTheBonus()
        {
            fixture.RequireGame();
            var unit = Wearer(armor: HeavyArmor);

            Assert.Equal(ArmorProficiencyGroup.Heavy, Armor(unit));

            var feat = Aegis(unit);
            Assert.True(feat.IsTurnedOn, "The feat should be on the unit.");
            Assert.Null(AegisModifier(unit));
        }

        /// <summary>A held shield blocks the bonus even without armor.</summary>
        [Fact]
        public void AShieldRemovesTheBonus()
        {
            fixture.RequireGame();
            var unit = Wearer(shield: LightShield);

            Assert.Equal(ArmorProficiencyGroup.None, Armor(unit));
            Assert.True(unit.Body.SecondaryHand.HasShield);

            var feat = Aegis(unit);
            Assert.True(feat.IsTurnedOn, "The feat should be on the unit.");
            Assert.Null(AegisModifier(unit));
        }

        /// <summary>A held shield blocks the bonus with light armor as well.</summary>
        [Fact]
        public void AShieldRemovesTheBonusWithLightArmor()
        {
            fixture.RequireGame();
            var unit = Wearer(armor: LightArmor, shield: LightShield);

            Assert.Equal(ArmorProficiencyGroup.Light, Armor(unit));
            Assert.True(unit.Body.SecondaryHand.HasShield);

            var feat = Aegis(unit);
            Assert.True(feat.IsTurnedOn, "The feat should be on the unit.");
            Assert.Null(AegisModifier(unit));
        }

        /// <summary>
        /// The gate is read again when the feat is applied again: the same unit that gained the
        /// bonus while unarmored loses it once the medium armor is worn and the feat is re-applied.
        /// The observation records that wearing the armor alone does not re-apply the feat.
        /// </summary>
        [Fact]
        public void TheGateIsReadAgainWhenTheFactIsAppliedAgain()
        {
            fixture.RequireGame();
            var unit = Wearer();
            var feat = Aegis(unit);
            Assert.Equal(WizardLevels / 2, AegisBonus(unit));

            var armor = OfflineGame.Blueprint<BlueprintItemArmor>(MediumArmor);
            unit.Descriptor.Proficiencies.Add(armor.ProficiencyGroup);
            Assert.True(unit.Body.Armor.InsertItem(armor.CreateEntity()));
            Assert.Equal(ArmorProficiencyGroup.Medium, Armor(unit));
            fixture.Observations["arcaneAegis.wearingArmorDoesNotReapplyTheFact"] = AegisBonus(unit);

            unit.Progression.Features.RemoveFact(feat);
            Aegis(unit);
            Assert.Null(AegisModifier(unit));
        }

        /// <summary>The proficiency group of the armor in the armor slot, or None when nothing is worn.</summary>
        private static ArmorProficiencyGroup Armor(UnitEntityData unit)
            => unit.Body.Armor.HasArmor ? unit.Body.Armor.Armor.Blueprint.ProficiencyGroup : ArmorProficiencyGroup.None;

        /// <summary>
        /// A vanilla character with the given equipment. The unit gets the proficiency each item
        /// needs, because the offline pregen has none, and the item goes into the real slot.
        /// </summary>
        private static UnitEntityData Wearer(string armor = null, string shield = null)
        {
            var unit = OfflineGame.CreateUnit(MainAttributeFeatTests.VanillaUnit);
            var wizard = OfflineGame.Blueprint<BlueprintCharacterClass>(WizardClass);
            for (var i = 0; i < WizardLevels; i++)
            {
                unit.Descriptor.Progression.AddClassLevel(wizard);
            }
            if (armor != null)
            {
                var blueprint = OfflineGame.Blueprint<BlueprintItemArmor>(armor);
                unit.Descriptor.Proficiencies.Add(blueprint.ProficiencyGroup);
                Assert.True(unit.Body.Armor.InsertItem(blueprint.CreateEntity()));
            }
            if (shield != null)
            {
                var blueprint = OfflineGame.Blueprint<BlueprintItemShield>(shield);
                unit.Descriptor.Proficiencies.Add(blueprint.ArmorComponent.ProficiencyGroup);
                Assert.True(unit.Body.SecondaryHand.InsertItem(blueprint.CreateEntity()));
            }
            return unit;
        }

        /// <summary>Adds the feat; its rank reads the unit's highest spellbook class level.</summary>
        private static Feature Aegis(UnitEntityData unit)
        {
            var blueprint = OfflineGame.Blueprint<BlueprintFeature>(Guids.Derived.ArcaneAegis);
            return unit.Progression.Features.AddFeature(blueprint);
        }

        /// <summary>The AC modifier the feat adds, or null when the feat adds none.</summary>
        private static ModifiableValue.Modifier AegisModifier(UnitEntityData unit)
        {
            var blueprint = OfflineGame.Blueprint<BlueprintFeature>(Guids.Derived.ArcaneAegis);
            var modifiers = unit.Stats.AC.Modifiers.Where(m => m.Source?.Blueprint == blueprint).ToList();
            Assert.True(modifiers.Count <= 1, "The feat should add at most one AC modifier.");
            return modifiers.SingleOrDefault();
        }

        /// <summary>The bonus the feat adds to AC.</summary>
        private static int AegisBonus(UnitEntityData unit) => AegisModifier(unit)?.ModValue ?? 0;
    }
}
