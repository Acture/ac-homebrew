using BlueprintCore.Utils;
using Kingmaker.Blueprints.Classes;
using static ACHomebrew.Feats.FeatBuilders;

namespace ACHomebrew.Feats
{
    internal static class ManeuverFeats
    {
        public static void ConfigureAll()
        {
            Feat(FeatSelection.ManeuverTactics, "SeizeOpening", Guids.ManeuverTactics.SeizeOpening,
                    LocalizedText.Text("SeizeOpening.Name", "Seize the Opening"),
                    LocalizedText.Text("SeizeOpening.Desc", "<i>A moment's hesitation is all the invitation you need.</i>\n\nEach round in combat, perform one eligible combat maneuver as a free action. You also have three extra free uses per encounter, allowing a second free maneuver that round. At most two maneuvers per round become free; further uses keep their normal action cost. Failed attempts count, canceled actions do not. Resources, range, targeting and other requirements still apply. These limits can be configured."))
                .SetIcon(BlueprintTool.Get<BlueprintFeature>("4cc71ae82bdd85b40b3cfe6697bb7949").Icon)
                .AddComponent<ManeuverTempo>()
                .Configure();
            Feat(FeatSelection.ManeuverTactics, "LingeringDisruption", Guids.ManeuverTactics.LingeringDisruption,
                    LocalizedText.Text("LingeringDisruption.Name", "Lingering Disruption"),
                    LocalizedText.Text("LingeringDisruption.Desc", "<i>They regain their footing, but never quite their composure.</i>\n\nTimed effects of your successful combat maneuvers last twice as long, gaining at least two rounds: 1 round becomes 3, 2 become 4, and 3 become 6. Applies to disarm, sunder armor, dirty tricks and supported effects from other mods. It does not prolong prone, displacement or permanent effects. Reapplication follows the effect's normal stacking rules. The multiplier and minimum extension can be configured."))
                .SetIcon(BlueprintTool.Get<BlueprintFeature>("52c6b07a68940af41b270b3710682dc7").Icon)
                .Configure();
        }
    }
}
