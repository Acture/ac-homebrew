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
                .SetDisplayName(Common.L("PolearmMaster.Name", Common.Text("PolearmMaster.Name", "Long-Reach Gambit"), Common.Text("PolearmMaster.Name", "长锋险势", true)))
                .SetDescription(Common.L("PolearmMaster.Desc", BuildDescriptionEn(), BuildDescriptionZh(), tagEncyclopediaEntries: true))
                .SetIconIfPresent("PolearmMaster")
                .AddReachMultiplicator(Desc, multiplicator: 2)
                .AddStatBonus(descriptor: Desc, stat: StatType.AdditionalDamage, value: -4)
                .Configure();
        }

        private static string BuildDescriptionEn()
            => Common.Format("Text.PolearmMasterFeats.BuildDescriptionEn.Text", "<i>Polearm Master · Reach Tradeoff</i>\n{0}\n\n<b>Effect:</b> Doubles your reach. You take a -4 untyped penalty to weapon damage.\n\n<b>Restrictions:</b> Works with any weapon category; no polearm requirement is implemented. This is an independent feat.", false, Common.Text("PolearmMaster.Lore", "You commit to a wider arc of engagement, accepting a less forceful blow as the price of distance."));

        private static string BuildDescriptionZh()
            => Common.Format("Text.PolearmMasterFeats.BuildDescriptionEn.Text", "<i>武器特化 · 触及专精</i>\n{0}\n\n<b>效果：</b>使你的触及范围翻倍。你的武器伤害检定承受-4无类型减值。\n\n<b>限制：</b>适用于所有武器类别；实现没有长柄武器限制。此专长独立生效。", true, Common.Text("PolearmMaster.Lore", "你将战线推向更宽的弧面，并接受出手力道减弱作为距离的代价。", true));
    }
}
