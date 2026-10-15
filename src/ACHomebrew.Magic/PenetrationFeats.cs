using Kingmaker.Blueprints.Classes.Spells;
using static ACHomebrew.Feats.FeatBuilders;

namespace ACHomebrew.Feats
{
    /// <summary>Penetration (spells): effects that pierce specific immunities.</summary>
    internal static class PenetrationFeats
    {
        private static bool Initialized;

        public static void ConfigureAll()
        {
            if (Initialized) return;
            Initialized = true;

            Piercing("MindBreaker", Guids.Penetration.MindBreaker, Common.Text("Text.PenetrationFeats.ConfigureAll.Text", "Mind Breaker", false), Common.Text("Text.PenetrationFeats.ConfigureAll.Text", "破心夺志", true), SpellDescriptor.MindAffecting | SpellDescriptor.Charm | SpellDescriptor.Compulsion | SpellDescriptor.Emotion, PiercedImmunity.None,
                Common.Text("Text.PenetrationFeats.ConfigureAll.Text.2", "Your mind-affecting, charm, compulsion and emotion effects ignore immunity to those effects (for example, undead and constructs).", false),
                Common.Text("Text.PenetrationFeats.ConfigureAll.Text.2", "你的心灵效果、魅惑、强制与情绪效果无视对这些效果的免疫（例如不死生物与构装生物）。", true));
            Piercing("Deathbringer", Guids.Penetration.Deathbringer, Common.Text("Text.PenetrationFeats.ConfigureAll.Text.3", "Deathbringer", false), Common.Text("Text.PenetrationFeats.ConfigureAll.Text.3", "索命无赦", true), SpellDescriptor.Death, PiercedImmunity.None,
                Common.Text("Text.PenetrationFeats.ConfigureAll.Text.4", "Your death effects ignore immunity to death effects.", false),
                Common.Text("Text.PenetrationFeats.ConfigureAll.Text.4", "你的死亡效果无视对死亡效果的免疫。", true));
            Piercing("Dread", Guids.Penetration.Dread, Common.Text("Text.PenetrationFeats.ConfigureAll.Text.5", "Dread Presence", false), Common.Text("Text.PenetrationFeats.ConfigureAll.Text.5", "慑魄凶威", true), SpellDescriptor.Fear | SpellDescriptor.Shaken | SpellDescriptor.Frightened, PiercedImmunity.None,
                Common.Text("Text.PenetrationFeats.ConfigureAll.Text.6", "Your fear effects ignore immunity to fear.", false),
                Common.Text("Text.PenetrationFeats.ConfigureAll.Text.6", "你的恐惧效果无视对恐惧的免疫。", true));
            Piercing("Plaguebearer", Guids.Penetration.Plaguebearer, Common.Text("Text.PenetrationFeats.ConfigureAll.Text.7", "Plaguebearer", false), Common.Text("Text.PenetrationFeats.ConfigureAll.Text.7", "疫毒蚀骨", true), SpellDescriptor.Poison | SpellDescriptor.Disease, PiercedImmunity.None,
                Common.Text("Text.PenetrationFeats.ConfigureAll.Text.8", "Your poison and disease effects ignore immunity to poison and disease.", false),
                Common.Text("Text.PenetrationFeats.ConfigureAll.Text.8", "你的毒素与疾病效果无视对毒素与疾病的免疫。", true));
            Piercing("Paralyzer", Guids.Penetration.Paralyzer, Common.Text("Text.PenetrationFeats.ConfigureAll.Text.9", "Overwhelming Force", false), Common.Text("Text.PenetrationFeats.ConfigureAll.Text.9", "镇岳缚形", true), SpellDescriptor.Paralysis | SpellDescriptor.Stun | SpellDescriptor.Daze | SpellDescriptor.Sleep, PiercedImmunity.None,
                Common.Text("Text.PenetrationFeats.ConfigureAll.Text.10", "Your paralysis, stun, daze and sleep effects ignore immunity to those effects.", false),
                Common.Text("Text.PenetrationFeats.ConfigureAll.Text.10", "你的麻痹、震慑、恍惚与睡眠效果无视对这些效果的免疫。", true));
            Piercing("ElementalBreach", Guids.Penetration.ElementalBreach, Common.Text("Text.PenetrationFeats.ConfigureAll.Text.11", "Elemental Breach", false), Common.Text("Text.PenetrationFeats.ConfigureAll.Text.11", "破元裂障", true), SpellDescriptor.None, PiercedImmunity.Energy,
                Common.Text("Text.PenetrationFeats.ConfigureAll.Text.12", "Creatures immune to an energy type take half damage from your damage of that type instead, and are never healed by it.", false),
                Common.Text("Text.PenetrationFeats.ConfigureAll.Text.12", "免疫某种能量的生物改为受到你该类型伤害的一半，且不会因此回复生命。", true));
        }

        private static void Piercing(string name, string guid, string nameEn, string nameZh, SpellDescriptor descriptors, PiercedImmunity other,
            string effectEn, string effectZh)
            => Feat(FeatSelection.Penetration, name, guid, nameEn, nameZh, Desc(Common.Text("Text.PenetrationFeats.Piercing.Text", "Penetration", false), Common.Text("Text.PenetrationFeats.Piercing.Text", "穿透", true), effectEn, effectZh))
                .AddComponent<ImmunityPiercer>(c =>
                {
                    c.Descriptors = descriptors;
                    c.Other = other;
                })
                .Configure();
    }
}
