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

            Piercing("MindBreaker", Guids.Penetration.MindBreaker, LocalizedText.Text("Text.PenetrationFeats.ConfigureAll.Text", "Mind Breaker"), SpellDescriptor.MindAffecting | SpellDescriptor.Charm | SpellDescriptor.Compulsion | SpellDescriptor.Emotion, PiercedImmunity.None,
                LocalizedText.Text("Text.PenetrationFeats.ConfigureAll.Text.2", "Your mind-affecting, charm, compulsion and emotion effects ignore immunity to those effects (for example, undead and constructs)."));
            Piercing("Deathbringer", Guids.Penetration.Deathbringer, LocalizedText.Text("Text.PenetrationFeats.ConfigureAll.Text.3", "Deathbringer"), SpellDescriptor.Death, PiercedImmunity.None,
                LocalizedText.Text("Text.PenetrationFeats.ConfigureAll.Text.4", "Your death effects ignore immunity to death effects."));
            Piercing("Dread", Guids.Penetration.Dread, LocalizedText.Text("Text.PenetrationFeats.ConfigureAll.Text.5", "Dread Presence"), SpellDescriptor.Fear | SpellDescriptor.Shaken | SpellDescriptor.Frightened, PiercedImmunity.None,
                LocalizedText.Text("Text.PenetrationFeats.ConfigureAll.Text.6", "Your fear effects ignore immunity to fear."));
            Piercing("Plaguebearer", Guids.Penetration.Plaguebearer, LocalizedText.Text("Text.PenetrationFeats.ConfigureAll.Text.7", "Plaguebearer"), SpellDescriptor.Poison | SpellDescriptor.Disease, PiercedImmunity.None,
                LocalizedText.Text("Text.PenetrationFeats.ConfigureAll.Text.8", "Your poison and disease effects ignore immunity to poison and disease."));
            Piercing("Paralyzer", Guids.Penetration.Paralyzer, LocalizedText.Text("Text.PenetrationFeats.ConfigureAll.Text.9", "Overwhelming Force"), SpellDescriptor.Paralysis | SpellDescriptor.Stun | SpellDescriptor.Daze | SpellDescriptor.Sleep, PiercedImmunity.None,
                LocalizedText.Text("Text.PenetrationFeats.ConfigureAll.Text.10", "Your paralysis, stun, daze and sleep effects ignore immunity to those effects."));
            Piercing("ElementalBreach", Guids.Penetration.ElementalBreach, LocalizedText.Text("Text.PenetrationFeats.ConfigureAll.Text.11", "Elemental Breach"), SpellDescriptor.None, PiercedImmunity.Energy,
                LocalizedText.Text("Text.PenetrationFeats.ConfigureAll.Text.12", "Creatures immune to an energy type take half damage from your damage of that type instead, and are never healed by it."));
        }

        private static void Piercing(string name, string guid, LocalizedText displayName, SpellDescriptor descriptors, PiercedImmunity other,
            LocalizedText effect)
            => Feat(FeatSelection.Penetration, name, guid, displayName, Desc(LocalizedText.Text("Text.PenetrationFeats.Piercing.Text", "Penetration"), effect))
                .AddComponent<ImmunityPiercer>(c =>
                {
                    c.Descriptors = descriptors;
                    c.Other = other;
                })
                .Configure();
    }
}
