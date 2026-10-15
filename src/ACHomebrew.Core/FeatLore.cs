using System.Collections.Generic;

namespace ACHomebrew.Feats
{
    /// <summary>One-line flavour text shown under the family line of newer feats, keyed by blueprint name.</summary>
    internal static class FeatLore
    {
        private static readonly Dictionary<string, LocalizedText> Lore = new()
        {
            ["CounterAttack"] = LocalizedText.Text("Text.FeatLore.Labels.Text", "You treat an enemy's swing not as an assault to weather, but as an open invitation to strike."),
            ["MissCounter"] = LocalizedText.Text("Text.FeatLore.Labels.Text.2", "When an adversary's strike cuts empty air, you slip past their recovered guard before they can reset their stance."),
            ["Punisher"] = LocalizedText.Text("Text.FeatLore.Labels.Text.3", "A blow aimed at you leaves a vivid trace in your mind, guiding your weapon back toward its author with cold purpose."),
            ["ParryCrit"] = LocalizedText.Text("Text.FeatLore.Labels.Text.4", "You read the subtle tremor along an opponent's guard at the moment of deflection, guiding your reply into their exposed core."),
            ["Shove"] = LocalizedText.Text("Text.FeatLore.Labels.Text.5", "You lean your entire bulk into the friction of combat, ensuring every clash sends the opponent staggering backward."),
            ["ReturnBlow"] = LocalizedText.Text("Text.FeatLore.Labels.Text.6", "Your flesh meets incoming weapons like cold iron, shuddering hard enough under impact to splinter the hands that strike you."),
            ["KrakenShell"] = LocalizedText.Text("Text.FeatLore.Labels.Text.7", "You weather blows like the armored hide of a deep-sea horror, sloughing off crippling afflictions as the pain crests."),
            ["KillingSpree"] = LocalizedText.Text("Text.FeatLore.Labels.Text.8", "Each felled foe frees your motion rather than slows it, pulling you eagerly toward the next engagement."),
            ["BattleRhythm"] = LocalizedText.Text("Text.FeatLore.Labels.Text.9", "You treat a melee as a continuous melody of strikes, sharpening your edge with every hit so long as your defense remains unsullied."),
            ["Fervor"] = LocalizedText.Text("Text.FeatLore.Labels.Text.10", "By fixing your gaze entirely upon a single opponent, you read their evasions in real time and weave extra strikes into their blind spots."),
            ["CrushingRhythm"] = LocalizedText.Text("Text.FeatLore.Labels.Text.11", "You structure your attacks like verse, building steady pressure until the fourth beat breaks the defense entirely."),
            ["CorrosiveFinish"] = LocalizedText.Text("Text.FeatLore.Labels.Text.12", "You know how to strike where flesh has already torn, widening an existing agony into a fatal collapse."),
            ["PhoenixFury"] = LocalizedText.Text("Text.FeatLore.Labels.Text.13", "When your own blood spills, you channel the searing burn of mortal distress into a furnace that engulfs everything near you."),
            ["Feast"] = LocalizedText.Text("Text.FeatLore.Labels.Text.14", "You strike not merely against armor, but against the vital momentum that keeps towering monsters upright."),
            ["FeastCurrent"] = LocalizedText.Text("Text.FeatLore.Labels.Text.15", "Your first engagements aim to shatter an opponent's pristine vigor before they have settled into the rhythm of the duel."),
            ["FeastSpell"] = LocalizedText.Text("Text.FeatLore.Labels.Text.16", "You shape your spells to unravel the underlying weave that binds an immense creature's vital essence together."),
            ["FeastNatural"] = LocalizedText.Text("Text.FeatLore.Labels.Text.17", "With tooth and claw, you draw upon the ancient law of the hunt, devouring an adversary's vitality to renew your own."),
            ["Lifesteal"] = LocalizedText.Text("Text.FeatLore.Labels.Text.18", "You align your weapon strokes to draw vigor from spilled life, turning every successful cut into renewed stamina."),
            ["SlotHarvest"] = LocalizedText.Text("Text.FeatLore.Labels.Text.19", "As a foe's breath departs, you gather the unraveling magical filaments of their dying consciousness to rekindle your own reserves."),
            ["ArcaneOrb"] = LocalizedText.Text("Text.FeatLore.Labels.Text.20", "With your deeper spell slots preserved intact, your simplest cantrips inherit the radiant pressure of an overflowing mind."),
            ["EssenceFlux"] = LocalizedText.Text("Text.FeatLore.Labels.Text.21", "You weave incantations so cleanly that stray echoes of power fold back into your mental repertoire rather than dispersing into the air."),
            ["ManaBreak"] = LocalizedText.Text("Text.FeatLore.Labels.Text.22", "You hone your weapon to disrupt the subtle concentration of spellcasters, sundering their memory of spells with each ringing blow."),
            ["ManaBreakSpell"] = LocalizedText.Text("Text.FeatLore.Labels.Text.23", "You direct your magic to pierce an opponent's prepared reservoir, turning the strain of their expended intellect into a concussive rupture."),
            ["ReStealth"] = LocalizedText.Text("Text.FeatLore.Labels.Text.24", "In the confusion born of a sudden, brutal wound, you dissolve into the periphery of sight before onlookers can locate your blade."),
            ["LoneWolf"] = LocalizedText.Text("Text.FeatLore.Labels.Text.25", "Having trained without companions to shield your flanks, your vigilance sharpens only when the ground around you is clear."),
            ["EssenceShift"] = LocalizedText.Text("Text.FeatLore.Labels.Text.26", "Through hundreds of life-or-death skirmishes, your muscles absorb the evasive reflexes of every agile prey you run down."),
            ["FleshHeap"] = LocalizedText.Text("Text.FeatLore.Labels.Text.27", "Walking beside heaps of fallen enemies thickens your sinews and bones, packing your frame with stubborn, brutal mass."),
            ["ArcaneSiphon"] = LocalizedText.Text("Text.FeatLore.Labels.Text.28", "By dissecting the final patterns of dying arcanists, you piece together new insights that expand the architecture of your thought."),
            ["DevouredVigor"] = LocalizedText.Text("Text.FeatLore.Labels.Text.29", "Each life cut short before you leaves a flicker of vital warmth that settles into your marrow, steadily fortifying your survival."),
            ["Necromastery"] = LocalizedText.Text("Text.FeatLore.Labels.Text.30", "You bind the vengeful remnants of slain foes to your wake, compelling their restless sorrow to sharpen your next blow."),
            ["SpiritLink"] = LocalizedText.Text("Text.FeatLore.Labels.Text.31", "You interweave your vital humors with the conjured forms at your side, letting restorative medicine mend you as one organism."),
            ["ShortBlade"] = LocalizedText.Text("Text.FeatLore.Labels.Text.32", "Understanding that long steel demands wide arcs, you practice burying compact blades into tight crevices where polearms falter."),
            ["CloseQuarters"] = LocalizedText.Text("Text.FeatLore.Labels.Text.33", "You crowd inside your opponent's ideal guard, turning what should be a lack of weapon reach into an oppressive, suffocating press."),
            ["CastingStat_Str"] = LocalizedText.Text("Text.FeatLore.Labels.Text.34", "You bypass abstract scholastic formulas, tearing spell formulas into reality through sheer bodily exertion and vocal thunder."),
            ["CastingStat_Dex"] = LocalizedText.Text("Text.FeatLore.Labels.Text.35", "Your somatic components mimic the blinding precision of a master embroiderer, threading magical currents through agile fingertips."),
            ["CastingStat_Con"] = LocalizedText.Text("Text.FeatLore.Labels.Text.36", "You treat your own bloodstream and lungs as the crucible for magic, anchoring spells to the unyielding stamina of your flesh."),
            ["ResourceStat_Str"] = LocalizedText.Text("Text.FeatLore.Labels.Text.37", "You draw deep upon your physical bulk, converting sheer muscular mass into an endurance that outlasts lesser bodies."),
            ["ResourceStat_Dex"] = LocalizedText.Text("Text.FeatLore.Labels.Text.38", "You preserve energy through frictionless movement, allowing balance and reflex to fuel your special techniques without fatigue."),
            ["ResourceStat_Con"] = LocalizedText.Text("Text.FeatLore.Labels.Text.39", "Your vitality runs deep into your bones, ensuring that moments requiring extraordinary resolve never find you emptied."),
        };

        // Inserts the lore line after the first (family) line of a description.
        public static LocalizedText Apply(string name, LocalizedText desc)
        {
            if (!Lore.TryGetValue(name, out var lore)) return desc;
            return LocalizedText.Compose(locale => Insert(desc[locale], FeatTextCatalog.Find($"{name}.Lore", locale) ?? lore[locale]));
        }

        private static string Insert(string text, string lore)
        {
            var end = text.IndexOf('\n');
            return end < 0 ? lore + "\n\n" + text : text.Substring(0, end) + "\n" + lore + text.Substring(end);
        }
    }
}
