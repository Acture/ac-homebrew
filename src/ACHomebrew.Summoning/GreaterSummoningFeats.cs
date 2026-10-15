using System.Collections.Generic;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Buffs;
using BlueprintCore.Utils.Types;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Mechanics.Components;

namespace ACHomebrew.Feats
{
    internal static class GreaterSummoningFeats
    {
        private static readonly ModifierDescriptor Desc = ModifierDescriptor.None;
        private static bool Initialized;

        public static void ConfigureAll()
        {
            if (Initialized) return;
            Initialized = true;

            var feats = new[]
            {
                CreateFeat(
                    internalName: "BloodlineOfBeasts",
                    featureGuid: Guids.Summon.Feature.BloodlineOfBeasts,
                    outerBuffGuid: Guids.Summon.OuterBuff.BloodlineOfBeasts,
                    innerBuffGuid: Guids.Summon.InnerBuff.BloodlineOfBeasts,
                    name: LocalizedText.Text("Summon_BloodlineOfBeasts.Name", "Behemoth's Heritage"),
                    desc: BuildDescription(
                        LocalizedText.Text("Text.GreaterSummoningFeats.ConfigureAll.Text", "Strength"),
                        LocalizedText.Text("Summon_BloodlineOfBeasts.Lore", "You lend your summoned companions the forceful bearing that guides your own movements."),
                        LocalizedText.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.2", "Your summoned creatures gain an untyped bonus to Strength equal to your Strength modifier.")),
                    baseStat: StatType.Strength,
                    buffedStats: new[] { StatType.Strength }),
                CreateFeat(
                    internalName: "QuickenedPact",
                    featureGuid: Guids.Summon.Feature.QuickenedPact,
                    outerBuffGuid: Guids.Summon.OuterBuff.QuickenedPact,
                    innerBuffGuid: Guids.Summon.InnerBuff.QuickenedPact,
                    name: LocalizedText.Text("Summon_QuickenedPact.Name", "Zephyr's Covenant"),
                    desc: BuildDescription(
                        LocalizedText.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.3", "Dexterity"),
                        LocalizedText.Text("Summon_QuickenedPact.Lore", "The rhythm of your agile steps becomes a pattern for the creatures answering your call."),
                        LocalizedText.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.4", "Your summoned creatures gain untyped bonuses to Dexterity and Speed equal to your Dexterity modifier.")),
                    baseStat: StatType.Dexterity,
                    buffedStats: new[] { StatType.Dexterity, StatType.Speed }),
                CreateFeat(
                    internalName: "VitalPact",
                    featureGuid: Guids.Summon.Feature.VitalPact,
                    outerBuffGuid: Guids.Summon.OuterBuff.VitalPact,
                    innerBuffGuid: Guids.Summon.InnerBuff.VitalPact,
                    name: LocalizedText.Text("Summon_VitalPact.Name", "Titan's Lifespring"),
                    desc: BuildDescription(
                        LocalizedText.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.5", "Constitution"),
                        LocalizedText.Text("Summon_VitalPact.Lore", "You draw on your own hardiness when preparing a body for a summoned companion."),
                        LocalizedText.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.6", "Your summoned creatures gain an untyped bonus to Constitution equal to your Constitution modifier.")),
                    baseStat: StatType.Constitution,
                    buffedStats: new[] { StatType.Constitution }),
                CreateFeat(
                    internalName: "TacticalBinding",
                    featureGuid: Guids.Summon.Feature.TacticalBinding,
                    outerBuffGuid: Guids.Summon.OuterBuff.TacticalBinding,
                    innerBuffGuid: Guids.Summon.InnerBuff.TacticalBinding,
                    name: LocalizedText.Text("Summon_TacticalBinding.Name", "Aegis of the Schema"),
                    desc: BuildDescription(
                        LocalizedText.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.7", "Intelligence"),
                        LocalizedText.Text("Summon_TacticalBinding.Lore", "You plan a summoned creature's defenses as carefully as an architect plans a wall."),
                        LocalizedText.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.8", "Your summoned creatures gain an untyped bonus to AC equal to your Intelligence modifier.")),
                    baseStat: StatType.Intelligence,
                    buffedStats: new[] { StatType.AC }),
                CreateFeat(
                    internalName: "InsightfulSummons",
                    featureGuid: Guids.Summon.Feature.InsightfulSummons,
                    outerBuffGuid: Guids.Summon.OuterBuff.InsightfulSummons,
                    innerBuffGuid: Guids.Summon.InnerBuff.InsightfulSummons,
                    name: LocalizedText.Text("Summon_InsightfulSummons.Name", "Empathic Communion"),
                    desc: BuildDescription(
                        LocalizedText.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.9", "Wisdom"),
                        LocalizedText.Text("Summon_InsightfulSummons.Lore", "Attentive guidance helps a summoned companion meet danger with steadier instincts."),
                        LocalizedText.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.10", "Your summoned creatures gain untyped bonuses to Fortitude, Reflex, and Will saves equal to your Wisdom modifier.")),
                    baseStat: StatType.Wisdom,
                    buffedStats: new[] { StatType.SaveFortitude, StatType.SaveReflex, StatType.SaveWill }),
                CreateFeat(
                    internalName: "MagneticCalling",
                    featureGuid: Guids.Summon.Feature.MagneticCalling,
                    outerBuffGuid: Guids.Summon.OuterBuff.MagneticCalling,
                    innerBuffGuid: Guids.Summon.InnerBuff.MagneticCalling,
                    name: LocalizedText.Text("Summon_MagneticCalling.Name", "Dominator's Calling"),
                    desc: BuildDescription(
                        LocalizedText.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.11", "Charisma"),
                        LocalizedText.Text("Summon_MagneticCalling.Lore", "Your confident command gives summoned companions a clear purpose when they enter the fray."),
                        LocalizedText.Text("Text.GreaterSummoningFeats.ConfigureAll.Text.12", "Your summoned creatures gain an untyped bonus to attack rolls equal to your Charisma modifier.")),
                    baseStat: StatType.Charisma,
                    buffedStats: new[] { StatType.AdditionalAttackBonus }),
            };

        }

        private static BlueprintFeature CreateFeat(
            string internalName,
            string featureGuid,
            string outerBuffGuid,
            string innerBuffGuid,
            LocalizedText name,
            LocalizedText desc,
            StatType baseStat,
            IReadOnlyList<StatType> buffedStats)
        {
            var displayName = Common.L($"Summon_{internalName}.Name", name);
            var description = Common.L(
                $"Summon_{internalName}.Desc",
                desc,
                tagEncyclopediaEntries: true);

            var innerBuff = BuffConfigurator.New($"{internalName}InnerBuff", innerBuffGuid)
                .SetDisplayName(displayName)
                .SetDescription(description)
                .SetIconIfPresent(internalName);
            AddRank(innerBuff, baseStat);
            AddContextBonuses(innerBuff, buffedStats);
            var configuredInnerBuff = innerBuff.Configure();

            var outerBuff = BuffConfigurator.New($"{internalName}OuterBuff", outerBuffGuid)
                .SetDisplayName(displayName)
                .SetDescription(description)
                .SetIconIfPresent(internalName)
                .AddOnSpawnBuff(buff: configuredInnerBuff, isInfinity: true)
                .Configure();

            return FeatSelection.GreaterSummoning.NewFeat(internalName, featureGuid)
                .SetDisplayName(displayName)
                .SetDescription(description)
                .SetIconIfPresent(internalName)
                .AddFacts(new() { outerBuff })
                .Configure();
        }

        private static void AddRank(BuffConfigurator cfg, StatType baseStat)
        {
            cfg.AddContextRankConfig(ContextRankConfigs.StatBonus(baseStat, ModifierDescriptor.None, AbilityRankType.Default, min: 0));
        }

        private static void AddContextBonuses(BuffConfigurator cfg, IReadOnlyList<StatType> stats)
        {
            foreach (var stat in stats)
            {
                cfg.AddContextStatBonus(stat, Common.Rank(), Desc);
            }
        }


        private static LocalizedText BuildDescription(
            LocalizedText attr,
            LocalizedText lore,
            LocalizedText effect)
        {
            return LocalizedText.Format("Text.GreaterSummoningFeats.BuildDescription.Description", "<i>Greater Summoning · {0}</i>\n{1}\n\n<b>Effect:</b> {2} Attribute modifiers used for these bonuses and matching penalties have a minimum of 0.\n\n<b>Restrictions:</b> While its exclusion group is on (configurable in mod settings), you can have only one Greater Summoning feat.", attr, lore, effect);
        }
    }
}
