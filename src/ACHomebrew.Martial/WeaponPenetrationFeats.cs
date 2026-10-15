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

            Feat(FeatSelection.Penetration, "BoneBreaker", Guids.Penetration.BoneBreaker, Common.Text("Text.WeaponPenetrationFeats.ConfigureAll.Text", "Bone Breaker", false), Common.Text("Text.WeaponPenetrationFeats.ConfigureAll.Text", "碎骨断筋", true),
                    Desc(Common.Text("Text.WeaponPenetrationFeats.ConfigureAll.Text.2", "Penetration", false), Common.Text("Text.WeaponPenetrationFeats.ConfigureAll.Text.2", "穿透", true),
                        Common.Text("Text.WeaponPenetrationFeats.ConfigureAll.Text.3", "Your critical hits apply even to creatures immune to critical hits.", false),
                        Common.Text("Text.WeaponPenetrationFeats.ConfigureAll.Text.3", "你的重击对免疫重击的生物同样生效。", true)))
                .AddComponent<Kingmaker.Designers.Mechanics.Facts.IgnoreCritImmunity>()
                .Configure();

            Feat(FeatSelection.Penetration, "HiddenVitals", Guids.Penetration.HiddenVitals, Common.Text("Text.WeaponPenetrationFeats.ConfigureAll.Text.4", "Hidden Vitals", false), Common.Text("Text.WeaponPenetrationFeats.ConfigureAll.Text.4", "寻隙刺要", true),
                    Desc(Common.Text("Text.WeaponPenetrationFeats.ConfigureAll.Text.2", "Penetration", false), Common.Text("Text.WeaponPenetrationFeats.ConfigureAll.Text.2", "穿透", true),
                        Common.Text("Text.WeaponPenetrationFeats.ConfigureAll.Text.5", "Your sneak attacks and other precision damage apply even to creatures immune to precision damage.", false),
                        Common.Text("Text.WeaponPenetrationFeats.ConfigureAll.Text.5", "你的偷袭与其他精准伤害对免疫精准伤害的生物同样生效。", true)))
                .AddComponent<ImmunityPiercer>(c => c.Other = PiercedImmunity.Precision)
                .Configure();
        }
    }
}
