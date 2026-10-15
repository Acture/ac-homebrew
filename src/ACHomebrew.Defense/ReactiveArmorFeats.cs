using BlueprintCore.Actions.Builder;
using BlueprintCore.Actions.Builder.ContextEx;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Buffs;
using BlueprintCore.Conditions.Builder;
using BlueprintCore.Conditions.Builder.BasicEx;
using BlueprintCore.Utils.Types;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Mechanics;

namespace ACHomebrew.Feats
{
    internal static class ReactiveArmorFeats
    {
        private static readonly ArmorProficiencyGroup[] AnyArmor =
        {
            ArmorProficiencyGroup.Light,
            ArmorProficiencyGroup.Medium,
            ArmorProficiencyGroup.Heavy,
        };

        private static readonly ArmorProficiencyGroup[] MediumHeavyArmor =
        {
            ArmorProficiencyGroup.Medium,
            ArmorProficiencyGroup.Heavy,
        };

        private static bool Initialized;

        public static void ConfigureAll()
        {
            if (Initialized) return;
            Initialized = true;

            ConfigureBulwarkBuff();
            ConfigureSpikedDefense();
            ConfigureBulwarkOfSteel();
        }

        private static void ConfigureSpikedDefense()
        {
            var cfg = FeatSelection.ReactiveArmor.NewFeat("SpikedDefense", Guids.ReactiveArmor.SpikedDefense)
                .SetDisplayName(Common.L("ReactiveArmor_SpikedDefense.Name", LocalizedText.Text("ReactiveArmor_SpikedDefense.Name", "Barbed Carapace")))
                .SetDescription(Common.L(
                    "ReactiveArmor_SpikedDefense.Desc",
                    BuildSpikedDefenseDescription(),
                    tagEncyclopediaEntries: true))
                .SetIconIfPresent("SpikedDefense");

            cfg.AddContextRankConfig(ContextRankConfigs.StatBonus(StatType.AC, ModifierDescriptor.Armor, min: 0));
            cfg.AddTargetAttackWithWeaponTrigger(
                actionsOnAttacker: ActionsBuilder.New()
                    .Conditional(
                        conditions: ConditionsBuilder.New()
                            .UnitArmor(includeArmorCategories: AnyArmor)
                            .Build(),
                        ifTrue: ActionsBuilder.New().DealDamage(
                            DamageTypes.Untyped(),
                            ContextDice.Value(DiceType.D6, bonus: Common.Rank()))
                            .Build(),
                        ifFalse: ActionsBuilder.New().Build())
                    .Build(),
                onlyHit: true,
                onlyMelee: true,
                waitForAttackResolve: true);
            cfg.Configure();
        }

        private static void ConfigureBulwarkOfSteel()
        {
            var refreshBuff = ActionsBuilder.New()
                .RemoveBuff(Guids.ReactiveArmor.BulwarkOfSteelBuff)
                .Conditional(
                    conditions: ConditionsBuilder.New()
                        .UnitArmor(includeArmorCategories: MediumHeavyArmor)
                        .Build(),
                    ifTrue: ActionsBuilder.New()
                        .ApplyBuff(Guids.ReactiveArmor.BulwarkOfSteelBuff, ContextDuration.Fixed(1), toCaster: true)
                        .Build(),
                    ifFalse: ActionsBuilder.New().Build());

            FeatSelection.ReactiveArmor.NewFeat("BulwarkOfSteel", Guids.ReactiveArmor.BulwarkOfSteel)
                .SetDisplayName(Common.L("ReactiveArmor_BulwarkOfSteel.Name", LocalizedText.Text("ReactiveArmor_BulwarkOfSteel.Name", "Citadel of Steel")))
                .SetDescription(Common.L(
                    "ReactiveArmor_BulwarkOfSteel.Desc",
                    BuildBulwarkOfSteelDescription(),
                    tagEncyclopediaEntries: true))
                .SetIconIfPresent("BulwarkOfSteel")
                .AddFactContextActions(
                    activated: refreshBuff,
                    deactivated: ActionsBuilder.New().RemoveBuff(Guids.ReactiveArmor.BulwarkOfSteelBuff),
                    dispose: ActionsBuilder.New().RemoveBuff(Guids.ReactiveArmor.BulwarkOfSteelBuff),
                    newRound: refreshBuff)
                .Configure();
        }

        private static void ConfigureBulwarkBuff()
        {
            BuffConfigurator.New("BulwarkOfSteelBuff", Guids.ReactiveArmor.BulwarkOfSteelBuff)
                .SetDisplayName(Common.L("ReactiveArmor_BulwarkOfSteelBuff.Name", LocalizedText.Text("ReactiveArmor_BulwarkOfSteel.Name", "Citadel of Steel")))
                .SetDescription(Common.L(
                    "ReactiveArmor_BulwarkOfSteelBuff.Desc",
                    LocalizedText.Text("ReactiveArmor_BulwarkOfSteelBuff.Desc", "<i>Reactive Armor · Living Fortress</i>\nCitadel of Steel is active, granting temporary hit points each round equal to your current armor bonus.")))
                .SetIconIfPresent("BulwarkOfSteel")
                .AddContextRankConfig(ContextRankConfigs.StatBonus(StatType.AC, ModifierDescriptor.Armor, min: 0))
                .AddComponent<TemporaryHitPointsFromAbilityValue>(c =>
                {
                    c.Descriptor = ModifierDescriptor.None;
                    c.RemoveWhenHitPointsEnd = true;
                    c.Value = Common.Rank();
                })
                .Configure();
        }

        private static LocalizedText BuildSpikedDefenseDescription()
            => LocalizedText.Format("Text.ReactiveArmorFeats.BuildSpikedDefenseDescriptionEn.Text", "<i>Reactive Armor · Retaliation</i>\n{0}\n\n<b>Effect:</b> While wearing armor, whenever a foe hits you with a melee weapon attack, that attacker takes 1d6 + your armor bonus as untyped damage.\n\n<b>Restrictions:</b> Requires armor to function. While its exclusion group is on, you can have only one Reactive Armor feat.", LocalizedText.Text("ReactiveArmor_SpikedDefense.Lore", "You imagine armor as a thorned carapace, turning the idea of a close assault back upon its source."));

        private static LocalizedText BuildBulwarkOfSteelDescription()
            => LocalizedText.Format("Text.ReactiveArmorFeats.BuildBulwarkOfSteelDescriptionEn.Text", "<i>Reactive Armor · Sustained Guard</i>\n{0}\n\n<b>Effect:</b> While wearing medium or heavy armor, you refresh temporary hit points each round equal to your current armor bonus.\n\n<b>Restrictions:</b> Requires medium or heavy armor to function. While its exclusion group is on, you can have only one Reactive Armor feat.", LocalizedText.Text("ReactiveArmor_BulwarkOfSteel.Lore", "Layered armor becomes the image of a portable fortress, built to endure one exchange at a time."));
    }
}
