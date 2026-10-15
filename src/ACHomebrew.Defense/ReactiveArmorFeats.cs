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
                .SetDisplayName(Common.L("ReactiveArmor_SpikedDefense.Name", Common.Text("ReactiveArmor_SpikedDefense.Name", "Barbed Carapace"), Common.Text("ReactiveArmor_SpikedDefense.Name", "逆鳞铁刺", true)))
                .SetDescription(Common.L(
                    "ReactiveArmor_SpikedDefense.Desc",
                    BuildSpikedDefenseDescriptionEn(),
                    BuildSpikedDefenseDescriptionZh(),
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
                .SetDisplayName(Common.L("ReactiveArmor_BulwarkOfSteel.Name", Common.Text("ReactiveArmor_BulwarkOfSteel.Name", "Citadel of Steel"), Common.Text("ReactiveArmor_BulwarkOfSteel.Name", "铸铁城阙", true)))
                .SetDescription(Common.L(
                    "ReactiveArmor_BulwarkOfSteel.Desc",
                    BuildBulwarkOfSteelDescriptionEn(),
                    BuildBulwarkOfSteelDescriptionZh(),
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
                .SetDisplayName(Common.L("ReactiveArmor_BulwarkOfSteelBuff.Name", Common.Text("ReactiveArmor_BulwarkOfSteel.Name", "Citadel of Steel"), Common.Text("ReactiveArmor_BulwarkOfSteel.Name", "铸铁城阙", true)))
                .SetDescription(Common.L(
                    "ReactiveArmor_BulwarkOfSteelBuff.Desc",
                    Common.Text("ReactiveArmor_BulwarkOfSteelBuff.Desc", "<i>Reactive Armor · Living Fortress</i>\nCitadel of Steel is active, granting temporary hit points each round equal to your current armor bonus.", false),
                    Common.Text("ReactiveArmor_BulwarkOfSteelBuff.Desc", "<i>活性护甲 · 身化铁城</i>\n铸铁城阙已激活，每轮获得等同于你的护甲加值的临时生命值。", true)))
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

        private static string BuildSpikedDefenseDescriptionEn()
            => Common.Format("Text.ReactiveArmorFeats.BuildSpikedDefenseDescriptionEn.Text", "<i>Reactive Armor · Retaliation</i>\n{0}\n\n<b>Effect:</b> While wearing armor, whenever a foe hits you with a melee weapon attack, that attacker takes 1d6 + your armor bonus as untyped damage.\n\n<b>Restrictions:</b> Requires armor to function. While its exclusion group is on, you can have only one Reactive Armor feat.", false, Common.Text("ReactiveArmor_SpikedDefense.Lore", "You imagine armor as a thorned carapace, turning the idea of a close assault back upon its source."));

        private static string BuildSpikedDefenseDescriptionZh()
            => Common.Format("Text.ReactiveArmorFeats.BuildSpikedDefenseDescriptionEn.Text", "<i>活性护甲 · 反伤</i>\n{0}\n\n<b>效果：</b>穿着护甲时，每当有敌人以近战武器攻击命中你，该攻击者将受到1d6+你的护甲加值的无类型伤害。\n\n<b>限制：</b>需穿着护甲方可生效。启用该互斥组时，只能拥有一个反应护甲专长。", true, Common.Text("ReactiveArmor_SpikedDefense.Lore", "你将护甲想象为带刺的甲壳，让贴身攻击的意象反向指向来袭之处。", true));

        private static string BuildBulwarkOfSteelDescriptionEn()
            => Common.Format("Text.ReactiveArmorFeats.BuildBulwarkOfSteelDescriptionEn.Text", "<i>Reactive Armor · Sustained Guard</i>\n{0}\n\n<b>Effect:</b> While wearing medium or heavy armor, you refresh temporary hit points each round equal to your current armor bonus.\n\n<b>Restrictions:</b> Requires medium or heavy armor to function. While its exclusion group is on, you can have only one Reactive Armor feat.", false, Common.Text("ReactiveArmor_BulwarkOfSteel.Lore", "Layered armor becomes the image of a portable fortress, built to endure one exchange at a time."));

        private static string BuildBulwarkOfSteelDescriptionZh()
            => Common.Format("Text.ReactiveArmorFeats.BuildBulwarkOfSteelDescriptionEn.Text", "<i>活性护甲 · 坚守</i>\n{0}\n\n<b>效果：</b>穿着中甲或重甲时，每轮获得等同于你的护甲加值的临时生命值（持续刷新）。\n\n<b>限制：</b>需穿着中甲或重甲方可生效。启用该互斥组时，只能拥有一个反应护甲专长。", true, Common.Text("ReactiveArmor_BulwarkOfSteel.Lore", "层叠甲胄化作移动要塞的意象，准备承受一次又一次交锋。", true));
    }
}
