using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;

namespace ACHomebrew.Feats
{
    internal static class PolearmMasterFeats
    {
        private const ModifierDescriptor Desc = ModifierDescriptor.None;
        private static bool Initialized;

        public static void ConfigureAll()
        {
            if (Initialized) return;
            Initialized = true;

            FeatSelection.Root.NewFeat("PolearmMaster", Guids.PolearmMaster.Feature.PolearmMaster)
                .SetDisplayName(Common.L("PolearmMaster.Name", LocalizedText.Text("PolearmMaster.Name", "Long-Reach Gambit")))
                .SetDescription(Common.L("PolearmMaster.Desc", BuildDescription(), tagEncyclopediaEntries: true))
                .SetIconIfPresent("PolearmMaster")
                .AddReachMultiplicator(Desc, multiplicator: 2)
                .AddStatBonus(descriptor: Desc, stat: StatType.AdditionalDamage, value: -4)
                .Configure();
        }

        private static LocalizedText BuildDescription()
            => LocalizedText.Format("Text.PolearmMasterFeats.BuildDescriptionEn.Text", "<i>Polearm Master · Reach Tradeoff</i>\n{0}\n\n<b>Effect:</b> Doubles your reach. You take a -4 untyped penalty to weapon damage.\n\n<b>Restrictions:</b> Works with any weapon category; no polearm requirement is implemented. This is an independent feat.", LocalizedText.Text("PolearmMaster.Lore", "You commit to a wider arc of engagement, accepting a less forceful blow as the price of distance."));
    }
}
