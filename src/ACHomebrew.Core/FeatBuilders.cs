using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Buffs;
using BlueprintCore.Utils.Types;
using Kingmaker.UnitLogic.Mechanics;

namespace ACHomebrew.Feats
{
    /// <summary>Shared builders for feat families: localized descriptions, feats and buffs.</summary>
    internal static class FeatBuilders
    {
        public static LocalizedText Desc(LocalizedText family, LocalizedText effect)
            => LocalizedText.Format("Text.FeatBuilders.Desc.Text", "<i>{0}</i>\n\n<b>Effect:</b> {1}", family, effect);

        public static FeatureConfigurator Feat(FeatSelection family, string name, string guid, LocalizedText displayName, LocalizedText desc)
        {
            desc = FeatLore.Apply(name, desc);
            return family.NewFeat(name, guid)
                .SetDisplayName(Common.L($"{name}.Name", displayName))
                .SetDescription(Common.L($"{name}.Desc", desc, tagEncyclopediaEntries: true))
                .SetIconIfPresent(name);
        }

        public static BuffConfigurator Buff(string name, string guid, LocalizedText displayName, LocalizedText desc)
            => BuffConfigurator.New(name, guid)
                .SetDisplayName(Common.L($"{name}.Name", displayName))
                .SetDescription(Common.L($"{name}.Desc", desc))
                .SetIconIfPresent(name);

        public static ContextDurationValue Rounds(int rounds) => ContextDuration.Fixed(rounds);
    }
}
