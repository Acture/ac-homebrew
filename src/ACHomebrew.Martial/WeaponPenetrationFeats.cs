using static ACHomebrew.Feats.FeatBuilders;

namespace ACHomebrew.Feats
{
    /// <summary>Penetration (weapons): critical hits and precision damage against immune creatures.</summary>
    internal static class WeaponPenetrationFeats
    {
        private static bool Initialized;

        public static void ConfigureAll()
        {
            if (Initialized) return;
            Initialized = true;

            Feat(FeatSelection.Penetration, "BoneBreaker", Guids.Penetration.BoneBreaker, LocalizedText.Text("Text.WeaponPenetrationFeats.ConfigureAll.Text", "Bone Breaker"),
                    Desc(LocalizedText.Text("Text.WeaponPenetrationFeats.ConfigureAll.Text.2", "Penetration"),
                        LocalizedText.Text("Text.WeaponPenetrationFeats.ConfigureAll.Text.3", "Your critical hits apply even to creatures immune to critical hits.")))
                .AddComponent<Kingmaker.Designers.Mechanics.Facts.IgnoreCritImmunity>()
                .Configure();

            Feat(FeatSelection.Penetration, "HiddenVitals", Guids.Penetration.HiddenVitals, LocalizedText.Text("Text.WeaponPenetrationFeats.ConfigureAll.Text.4", "Hidden Vitals"),
                    Desc(LocalizedText.Text("Text.WeaponPenetrationFeats.ConfigureAll.Text.2", "Penetration"),
                        LocalizedText.Text("Text.WeaponPenetrationFeats.ConfigureAll.Text.5", "Your sneak attacks and other precision damage apply even to creatures immune to precision damage.")))
                .AddComponent<ImmunityPiercer>(c => c.Other = PiercedImmunity.Precision)
                .Configure();
        }
    }
}
