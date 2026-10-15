using System.Collections.Generic;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Buffs;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;

namespace ACHomebrew.Feats
{
    internal static class SummonerSacrificeFeats
    {
        private const ModifierDescriptor Desc = ModifierDescriptor.None;
        private static readonly StatType[] PhysicalAttributes =
        {
            StatType.Strength,
            StatType.Dexterity,
            StatType.Constitution,
        };

        private static bool Initialized;

        public static void ConfigureAll()
        {
            if (Initialized) return;
            Initialized = true;

            var feats = new[]
            {
                CreateFeat(
                    internalName: "BodyOfMyPact",
                    featureGuid: Guids.SummonerSacrifice.Feature.BodyOfMyPact,
                    outerBuffGuid: Guids.SummonerSacrifice.OuterBuff.BodyOfMyPact,
                    innerBuffGuid: Guids.SummonerSacrifice.InnerBuff.BodyOfMyPact,
                    name: LocalizedText.Text("SummonerSacrifice_BodyOfMyPact.Name", "Martyr's Transference"),
                    desc: BuildDescription(
                        mode: LocalizedText.Text("Text.SummonerSacrificeFeats.ConfigureAll.modeEn", "1:1 Trade"),
                        lore: LocalizedText.Text("SummonerSacrifice_BodyOfMyPact.Lore", "You accept a body's burden so that the creatures bound to your call may stand stronger."),
                        effect: LocalizedText.Text("Text.SummonerSacrificeFeats.ConfigureAll.effectEn", "You take a -4 untyped penalty to Strength, Dexterity, and Constitution. Your summoned creatures gain a +4 untyped bonus to Strength, Dexterity, and Constitution.")),
                    selfBonuses: CreateUniformBonuses(-4),
                    summonBonuses: CreateUniformBonuses(4)),
                CreateFeat(
                    internalName: "DoubledBond",
                    featureGuid: Guids.SummonerSacrifice.Feature.DoubledBond,
                    outerBuffGuid: Guids.SummonerSacrifice.OuterBuff.DoubledBond,
                    innerBuffGuid: Guids.SummonerSacrifice.InnerBuff.DoubledBond,
                    name: LocalizedText.Text("SummonerSacrifice_DoubledBond.Name", "Eldritch Crucible"),
                    desc: BuildDescription(
                        mode: LocalizedText.Text("Text.SummonerSacrificeFeats.ConfigureAll.modeEn.2", "1:2 Amplification"),
                        lore: LocalizedText.Text("SummonerSacrifice_DoubledBond.Lore", "An exacting pact magnifies what you surrender, letting sacrifice feed a companion's strength."),
                        effect: LocalizedText.Text("Text.SummonerSacrificeFeats.ConfigureAll.effectEn.2", "You take a -2 untyped penalty to Strength, Dexterity, and Constitution. Your summoned creatures gain a +4 untyped bonus to Strength, Dexterity, and Constitution.")),
                    selfBonuses: CreateUniformBonuses(-2),
                    summonBonuses: CreateUniformBonuses(4)),
                CreateFeat(
                    internalName: "EmpoweredSacrifice",
                    featureGuid: Guids.SummonerSacrifice.Feature.EmpoweredSacrifice,
                    outerBuffGuid: Guids.SummonerSacrifice.OuterBuff.EmpoweredSacrifice,
                    innerBuffGuid: Guids.SummonerSacrifice.InnerBuff.EmpoweredSacrifice,
                    name: LocalizedText.Text("SummonerSacrifice_EmpoweredSacrifice.Name", "Tribute of Iron Dominion"),
                    desc: BuildDescription(
                        mode: LocalizedText.Text("Text.SummonerSacrificeFeats.ConfigureAll.modeEn.3", "Focused Trade"),
                        lore: LocalizedText.Text("SummonerSacrifice_EmpoweredSacrifice.Lore", "You yield some commanding presence to give a summoned body greater physical force."),
                        effect: LocalizedText.Text("Text.SummonerSacrificeFeats.ConfigureAll.effectEn.3", "You take a -4 untyped penalty to Charisma. Your summoned creatures gain a +8 untyped bonus to Strength.")),
                    selfBonuses: new[] { new StatBonus(StatType.Charisma, -4) },
                    summonBonuses: new[] { new StatBonus(StatType.Strength, 8) }),
            };

        }

        private static BlueprintFeature CreateFeat(
            string internalName,
            string featureGuid,
            string outerBuffGuid,
            string innerBuffGuid,
            LocalizedText name,
            LocalizedText desc,
            IReadOnlyList<StatBonus> selfBonuses,
            IReadOnlyList<StatBonus> summonBonuses)
        {
            var localizedName = Common.L($"SummonerSacrifice_{internalName}.Name", name);
            var localizedDescription = Common.L($"SummonerSacrifice_{internalName}.Desc", desc, tagEncyclopediaEntries: true);

            var innerBuff = BuffConfigurator.New($"{internalName}InnerBuff", innerBuffGuid)
                .SetDisplayName(localizedName)
                .SetDescription(localizedDescription)
                .SetIconIfPresent(internalName)
                .AddSummonedUnitBuff();
            AddStatBonuses(innerBuff, summonBonuses);
            var configuredInnerBuff = innerBuff.Configure();

            var outerBuff = BuffConfigurator.New($"{internalName}OuterBuff", outerBuffGuid)
                .SetDisplayName(localizedName)
                .SetDescription(localizedDescription)
                .SetIconIfPresent(internalName)
                .AddOnSpawnBuff(buff: configuredInnerBuff, isInfinity: true);
            AddStatBonuses(outerBuff, selfBonuses);
            var configuredOuterBuff = outerBuff.Configure();

            return FeatSelection.SummonerSacrifice.NewFeat(internalName, featureGuid)
                .SetDisplayName(localizedName)
                .SetDescription(localizedDescription)
                .SetIconIfPresent(internalName)
                .AddFacts(new() { configuredOuterBuff })
                .Configure();
        }

        private static void AddStatBonuses(BuffConfigurator cfg, IReadOnlyList<StatBonus> bonuses)
        {
            foreach (var bonus in bonuses)
            {
                cfg.AddStatBonus(descriptor: Desc, stat: bonus.Stat, value: bonus.Value);
            }
        }

        private static StatBonus[] CreateUniformBonuses(int value)
        {
            var bonuses = new StatBonus[PhysicalAttributes.Length];
            for (var i = 0; i < PhysicalAttributes.Length; i++)
            {
                bonuses[i] = new StatBonus(PhysicalAttributes[i], value);
            }

            return bonuses;
        }


        private static LocalizedText BuildDescription(
            LocalizedText mode,
            LocalizedText lore,
            LocalizedText effect)
        {
            return LocalizedText.Format("Text.SummonerSacrificeFeats.BuildDescription.Description", "<i>Summoner Sacrifice · {0}</i>\n{1}\n\n<b>Effect:</b> {2}\n\n<b>Restrictions:</b> While its exclusion group is on (configurable in mod settings), you can have only one Summoner Sacrifice feat.", mode, lore, effect);
        }

        private readonly struct StatBonus
        {
            public StatBonus(StatType stat, int value)
            {
                Stat = stat;
                Value = value;
            }

            public StatType Stat { get; }
            public int Value { get; }
        }
    }
}
