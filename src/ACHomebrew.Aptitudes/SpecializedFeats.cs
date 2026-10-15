using System.Collections.Generic;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Mechanics.Components;

namespace ACHomebrew.Feats
{
    /// <summary>
    /// Creates the 24 Specialized Adept feats.
    /// </summary>
    internal static class SpecializedFeats
    {
        private static readonly ModifierDescriptor Desc = ModifierDescriptor.None;
        private static readonly StatType[] NoStats = new StatType[0];

        private static readonly StatType[] ManeuverStats =
        {
            StatType.AdditionalCMB,
        };

        // Each Adept covers two or three themed targets for its attribute.
        private static readonly Dictionary<StatType, (StatType[] stats, LocalizedText label)> DefensiveThemes = new()
        {
            [StatType.Strength] = (new[] { StatType.AdditionalCMD, StatType.SaveFortitude }, LocalizedText.Text("Text.SpecializedFeats.Labels.Text", "CMD and Fortitude saves")),
            [StatType.Dexterity] = (new[] { StatType.AC, StatType.SaveReflex }, LocalizedText.Text("Text.SpecializedFeats.Labels.Text.2", "AC and Reflex saves")),
            [StatType.Constitution] = (new[] { StatType.AC, StatType.SaveFortitude }, LocalizedText.Text("Text.SpecializedFeats.Labels.Text.3", "AC and Fortitude saves")),
            [StatType.Intelligence] = (new[] { StatType.Initiative, StatType.SaveReflex }, LocalizedText.Text("Text.SpecializedFeats.Labels.Text.4", "Initiative and Reflex saves")),
            [StatType.Wisdom] = (new[] { StatType.SaveWill, StatType.Initiative }, LocalizedText.Text("Text.SpecializedFeats.Labels.Text.5", "Will saves and Initiative")),
            [StatType.Charisma] = (new[] { StatType.SaveWill, StatType.AdditionalCMD }, LocalizedText.Text("Text.SpecializedFeats.Labels.Text.6", "Will saves and CMD")),
        };

        private static readonly Dictionary<StatType, (StatType[] stats, LocalizedText label)> SkilledThemes = new()
        {
            [StatType.Strength] = (new[] { StatType.SkillAthletics, StatType.CheckIntimidate }, LocalizedText.Text("Text.SpecializedFeats.Labels.Text.7", "Athletics and Intimidate checks")),
            [StatType.Dexterity] = (new[] { StatType.SkillMobility, StatType.SkillStealth, StatType.SkillThievery }, LocalizedText.Text("Text.SpecializedFeats.Labels.Text.8", "Mobility, Stealth and Thievery")),
            [StatType.Constitution] = (new[] { StatType.SkillAthletics, StatType.SkillPerception }, LocalizedText.Text("Text.SpecializedFeats.Labels.Text.9", "Athletics and Perception")),
            [StatType.Intelligence] = (new[] { StatType.SkillKnowledgeArcana, StatType.SkillKnowledgeWorld, StatType.SkillUseMagicDevice },
                LocalizedText.Text("Text.SpecializedFeats.Labels.Text.10", "Knowledge (Arcana), Knowledge (World) and Use Magic Device")),
            [StatType.Wisdom] = (new[] { StatType.SkillPerception, StatType.SkillLoreNature, StatType.SkillLoreReligion },
                LocalizedText.Text("Text.SpecializedFeats.Labels.Text.11", "Perception, Lore (Nature) and Lore (Religion)")),
            [StatType.Charisma] = (new[] { StatType.SkillPersuasion, StatType.CheckBluff, StatType.CheckDiplomacy },
                LocalizedText.Text("Text.SpecializedFeats.Labels.Text.12", "Persuasion plus Bluff and Diplomacy checks")),
        };

        private static bool Initialized;

        private enum SpecializedFamily
        {
            Defensive,
            Maneuver,
            Skilled,
            Arcane,
        }

        public static void ConfigureAll()
        {
            if (Initialized) return;
            Initialized = true;

            var defensive = new[]
            {
                CreateSpecialized(
                    SpecializedFamily.Defensive,
                    StatType.Strength,
                    NoStats,
                    Guids.Specialized.Defensive.Str,
                    "TitansStance",
                    LocalizedText.Text("Defensive_Str.Name", "Colossus Bastion"),
                    LocalizedText.Text("Defensive_Str.Lore", "You brace like a fortress buttress, letting practiced strength support your whole defense.")),
                CreateSpecialized(
                    SpecializedFamily.Defensive,
                    StatType.Dexterity,
                    NoStats,
                    Guids.Specialized.Defensive.Dex,
                    "FlowingForm",
                    LocalizedText.Text("Defensive_Dex.Name", "Wind-Dancer's Shroud"),
                    LocalizedText.Text("Defensive_Dex.Lore", "You borrow the rhythm of a swaying reed, keeping your guard in motion rather than fixing it in place.")),
                CreateSpecialized(
                    SpecializedFamily.Defensive,
                    StatType.Constitution,
                    NoStats,
                    Guids.Specialized.Defensive.Con,
                    "IronBulwark",
                    LocalizedText.Text("Defensive_Con.Name", "Inured Carapace"),
                    LocalizedText.Text("Defensive_Con.Lore", "Old scars remind you how to endure a blow and keep your composure under strain.")),
                CreateSpecialized(
                    SpecializedFamily.Defensive,
                    StatType.Intelligence,
                    NoStats,
                    Guids.Specialized.Defensive.Int,
                    "CalculatedDefense",
                    LocalizedText.Text("Defensive_Int.Name", "Analytical Aegis"),
                    LocalizedText.Text("Defensive_Int.Lore", "You read the angle of an approaching strike and place your guard where it will matter.")),
                CreateSpecialized(
                    SpecializedFamily.Defensive,
                    StatType.Wisdom,
                    NoStats,
                    Guids.Specialized.Defensive.Wis,
                    "StoicVigilance",
                    LocalizedText.Text("Defensive_Wis.Name", "Third Eye Vigil"),
                    LocalizedText.Text("Defensive_Wis.Lore", "Patient attention catches small changes in a foe's bearing before they become a committed attack.")),
                CreateSpecialized(
                    SpecializedFamily.Defensive,
                    StatType.Charisma,
                    NoStats,
                    Guids.Specialized.Defensive.Cha,
                    "IndomitablePresence",
                    LocalizedText.Text("Defensive_Cha.Name", "Majesty's Reproach"),
                    LocalizedText.Text("Defensive_Cha.Lore", "Your unyielding bearing turns defense into a contest of resolve as much as steel.")),
            };

            var maneuver = new[]
            {
                CreateSpecialized(
                    SpecializedFamily.Maneuver,
                    StatType.Strength,
                    ManeuverStats,
                    Guids.Specialized.Maneuver.Str,
                    "CrushingGrip",
                    LocalizedText.Text("Maneuver_Str.Name", "Grip of the Behemoth"),
                    LocalizedText.Text("Maneuver_Str.Lore", "You bring the weight of a great beast to every grip, push, and struggle for position.")),
                CreateSpecialized(
                    SpecializedFamily.Maneuver,
                    StatType.Dexterity,
                    ManeuverStats,
                    Guids.Specialized.Maneuver.Dex,
                    "DeftHand",
                    LocalizedText.Text("Maneuver_Dex.Name", "Fulcrum of the Viper"),
                    LocalizedText.Text("Maneuver_Dex.Lore", "A small turn at the right pivot can redirect more force than a head-on struggle.")),
                CreateSpecialized(
                    SpecializedFamily.Maneuver,
                    StatType.Constitution,
                    ManeuverStats,
                    Guids.Specialized.Maneuver.Con,
                    "UnyieldingHold",
                    LocalizedText.Text("Maneuver_Con.Name", "Deep-Root Clinch"),
                    LocalizedText.Text("Maneuver_Con.Lore", "Endurance lets you maintain a hold after the first contest of strength has passed.")),
                CreateSpecialized(
                    SpecializedFamily.Maneuver,
                    StatType.Intelligence,
                    ManeuverStats,
                    Guids.Specialized.Maneuver.Int,
                    "TacticalBind",
                    LocalizedText.Text("Maneuver_Int.Name", "Anatomical Pivot"),
                    LocalizedText.Text("Maneuver_Int.Lore", "You study how joints move together and use that knowledge to guide a maneuver.")),
                CreateSpecialized(
                    SpecializedFamily.Maneuver,
                    StatType.Wisdom,
                    ManeuverStats,
                    Guids.Specialized.Maneuver.Wis,
                    "PredictiveLock",
                    LocalizedText.Text("Maneuver_Wis.Name", "Crane's Anticipation"),
                    LocalizedText.Text("Maneuver_Wis.Lore", "You feel an opponent's balance through contact, answering their movement with your own.")),
                CreateSpecialized(
                    SpecializedFamily.Maneuver,
                    StatType.Charisma,
                    ManeuverStats,
                    Guids.Specialized.Maneuver.Cha,
                    "DomineeringThrow",
                    LocalizedText.Text("Maneuver_Cha.Name", "Audacious Overthrow"),
                    LocalizedText.Text("Maneuver_Cha.Lore", "An assertive step and a forceful challenge lend conviction to your attempt to unseat a foe.")),
            };

            var skilled = new[]
            {
                CreateSpecialized(
                    SpecializedFamily.Skilled,
                    StatType.Strength,
                    NoStats,
                    Guids.Specialized.Skilled.Str,
                    "PracticedHand",
                    LocalizedText.Text("Skilled_Str.Name", "Giantwright's Craft"),
                    LocalizedText.Text("Skilled_Str.Lore", "Work that tires others teaches you the discipline of applying strength with care.")),
                CreateSpecialized(
                    SpecializedFamily.Skilled,
                    StatType.Dexterity,
                    NoStats,
                    Guids.Specialized.Skilled.Dex,
                    "EffortlessSkill",
                    LocalizedText.Text("Skilled_Dex.Name", "Thief-King's Panache"),
                    LocalizedText.Text("Skilled_Dex.Lore", "Your hands learn each task as a rhythm, returning to its fine motions with a performer's ease.")),
                CreateSpecialized(
                    SpecializedFamily.Skilled,
                    StatType.Constitution,
                    NoStats,
                    Guids.Specialized.Skilled.Con,
                    "TirelessPractice",
                    LocalizedText.Text("Skilled_Con.Name", "Ascetic Diligence"),
                    LocalizedText.Text("Skilled_Con.Lore", "Where inspiration fades, patient repetition keeps your craft moving forward.")),
                CreateSpecialized(
                    SpecializedFamily.Skilled,
                    StatType.Intelligence,
                    NoStats,
                    Guids.Specialized.Skilled.Int,
                    "PolymathsTouch",
                    LocalizedText.Text("Skilled_Int.Name", "Encyclopedic Synthesis"),
                    LocalizedText.Text("Skilled_Int.Lore", "You connect unfamiliar problems to knowledge already gathered, building bridges between disciplines.")),
                CreateSpecialized(
                    SpecializedFamily.Skilled,
                    StatType.Wisdom,
                    NoStats,
                    Guids.Specialized.Skilled.Wis,
                    "QuietMastery",
                    LocalizedText.Text("Skilled_Wis.Name", "Wanderer's Lucidity"),
                    LocalizedText.Text("Skilled_Wis.Lore", "Travel and observation have taught you to hear what a task requires before reaching for a tool.")),
                CreateSpecialized(
                    SpecializedFamily.Skilled,
                    StatType.Charisma,
                    NoStats,
                    Guids.Specialized.Skilled.Cha,
                    "InspiredVersatility",
                    LocalizedText.Text("Skilled_Cha.Name", "Silver-Tongued Virtuoso"),
                    LocalizedText.Text("Skilled_Cha.Lore", "Confidence carries your performance through unfamiliar work, inviting others to believe in your command.")),
            };

            var arcane = new[]
            {
                CreateSpecialized(
                    SpecializedFamily.Arcane,
                    StatType.Strength,
                    NoStats,
                    Guids.Specialized.Arcane.Str,
                    "SpellForgedWill",
                    LocalizedText.Text("Arcane_Str.Name", "Mage-Hammer Inscription"),
                    LocalizedText.Text("Arcane_Str.Lore", "You approach an incantation as a smith approaches iron, shaping it with disciplined exertion.")),
                CreateSpecialized(
                    SpecializedFamily.Arcane,
                    StatType.Dexterity,
                    NoStats,
                    Guids.Specialized.Arcane.Dex,
                    "QuickcastReflex",
                    LocalizedText.Text("Arcane_Dex.Name", "Somatic Velocity"),
                    LocalizedText.Text("Arcane_Dex.Lore", "Exact, practiced gestures give your spellwork the cadence of a deft duelist.")),
                CreateSpecialized(
                    SpecializedFamily.Arcane,
                    StatType.Constitution,
                    NoStats,
                    Guids.Specialized.Arcane.Con,
                    "SpellTemperedBody",
                    LocalizedText.Text("Arcane_Con.Name", "Crucible of the Conduit"),
                    LocalizedText.Text("Arcane_Con.Lore", "You make bodily endurance part of your magical practice, learning to bear the effort of channeling power.")),
                CreateSpecialized(
                    SpecializedFamily.Arcane,
                    StatType.Intelligence,
                    NoStats,
                    Guids.Specialized.Arcane.Int,
                    "ScholarOfTheWeave",
                    LocalizedText.Text("Arcane_Int.Name", "Archmage's Codex"),
                    LocalizedText.Text("Arcane_Int.Lore", "Every spell becomes a proposition to study, refine, and set beside the work of earlier arcanists.")),
                CreateSpecialized(
                    SpecializedFamily.Arcane,
                    StatType.Wisdom,
                    NoStats,
                    Guids.Specialized.Arcane.Wis,
                    "OraclesIntuition",
                    LocalizedText.Text("Arcane_Wis.Name", "Gnostic Channel"),
                    LocalizedText.Text("Arcane_Wis.Lore", "You listen for the cadence beneath an incantation and let attentive instinct guide its expression.")),
                CreateSpecialized(
                    SpecializedFamily.Arcane,
                    StatType.Charisma,
                    NoStats,
                    Guids.Specialized.Arcane.Cha,
                    "SorcerousPresence",
                    LocalizedText.Text("Arcane_Cha.Name", "Sovereign Decrees"),
                    LocalizedText.Text("Arcane_Cha.Lore", "You speak an incantation with the confidence of a decree, giving its form the weight of conviction.")),
            };

        }

        private static BlueprintFeature CreateSpecialized(
            SpecializedFamily family,
            StatType baseStat,
            IReadOnlyList<StatType> stats,
            string guid,
            string internalName,
            LocalizedText flavorName,
            LocalizedText loreText)
        {
            var settings = Mod.Settings ?? new ModSettings();
            var attributeKey = GetAttributeKey(baseStat);
            var attributeName = GetAttributeName(baseStat);
            var familyKey = GetFamilyKey(family);

            var desc = BuildDescription(
                family,
                baseStat,
                attributeName,
                loreText);

            var selection = family switch
            {
                SpecializedFamily.Defensive => FeatSelection.Defensive,
                SpecializedFamily.Maneuver => FeatSelection.Maneuver,
                SpecializedFamily.Skilled => FeatSelection.Skilled,
                SpecializedFamily.Arcane => FeatSelection.Arcane,
                _ => throw new System.ArgumentOutOfRangeException(nameof(family)),
            };
            var cfg = selection.NewFeat(internalName, guid)
                .SetDisplayName(Common.L($"{familyKey}_{attributeKey}.Name", flavorName))
                .SetDescription(Common.L(
                    $"{familyKey}_{attributeKey}.Desc",
                    desc,
                    tagEncyclopediaEntries: true))
                .SetIconIfPresent(internalName);

            Common.AddRank(cfg, baseStat, AbilityRankType.Default, Common.ResolveProgression(settings.powerLevel, ScalingIntent.Full));
            if (family == SpecializedFamily.Arcane)
            {
                Common.AddRank(cfg, baseStat, AbilityRankType.StatBonus, Common.ResolveProgression(settings.powerLevel, ScalingIntent.Half));
            }

            switch (family)
            {
                case SpecializedFamily.Defensive:
                    if (settings.EnableDefenses)
                    {
                        AddContextBonuses(cfg, DefensiveThemes[baseStat].stats, AbilityRankType.Default);
                    }
                    break;
                case SpecializedFamily.Maneuver:
                    if (settings.EnableManeuvers)
                    {
                        AddContextBonuses(cfg, stats, AbilityRankType.Default);
                    }
                    break;
                case SpecializedFamily.Skilled:
                    foreach (var stat in SkilledThemes[baseStat].stats)
                    {
                        var isCheck = stat == StatType.CheckBluff || stat == StatType.CheckDiplomacy || stat == StatType.CheckIntimidate;
                        if (isCheck ? settings.EnableChecks : settings.EnableSkills)
                            AddContextBonuses(cfg, new[] { stat }, AbilityRankType.Default);
                    }
                    break;
                case SpecializedFamily.Arcane:
                    if (settings.EnableCasterLevel)
                    {
                        cfg.AddComponent<IncreaseCasterLevel>(c =>
                        {
                            c.Value = Common.Rank(AbilityRankType.Default);
                            c.Descriptor = Desc;
                        });
                    }

                    if (settings.EnableCasterDC)
                    {
                        cfg.AddComponent<IncreaseAllSpellsDC>(c =>
                        {
                            c.Value = Common.Rank(AbilityRankType.StatBonus);
                            c.Descriptor = Desc;
                            c.SpellsOnly = false;
                        });
                    }

                    if (settings.EnableSpellPenetration)
                    {
                        cfg.AddComponent<SpellPenetrationBonus>(c =>
                        {
                            c.Value = Common.Rank(AbilityRankType.Default);
                            c.Descriptor = Desc;
                        });
                    }
                    break;
            }

            cfg.AddRecalculateOnStatChange(stat: baseStat);
            return cfg.Configure();
        }


        private static void AddContextBonuses(FeatureConfigurator cfg, IReadOnlyList<StatType> stats, AbilityRankType rankType)
        {
            foreach (var stat in stats)
            {
                cfg.AddContextStatBonus(stat, Common.Rank(rankType), Desc);
            }
        }

        private static LocalizedText BuildDescription(
            SpecializedFamily family,
            StatType baseStat,
            LocalizedText attributeName,
            LocalizedText loreText)
        {
            var familyName = GetFamilyDisplayName(family);
            var effect = LocalizedText.Format("Text.SpecializedFeats.BuildDescription.effect", "{0} Attribute-based bonuses use a minimum modifier of 0 and apply only when their corresponding mod settings are enabled.", GetEffectText(family, baseStat, attributeName));
            var restriction = GetRestrictionText(family);

            return LocalizedText.Format("Text.SpecializedFeats.BuildDescription.Description", "<i>{0} · {1}</i>\n{2}\n\n<b>Effect:</b> {3}\n\n<b>Restrictions:</b> {4}", familyName, attributeName, loreText, effect, restriction);
        }

        private static LocalizedText GetEffectText(SpecializedFamily family, StatType baseStat, LocalizedText attributeName)
        {
            switch (family)
            {
                case SpecializedFamily.Defensive:
                    return LocalizedText.Format("Text.SpecializedFeats.GetEffectText.Text", "Adds your {0} modifier (untyped) to {1}.", attributeName, DefensiveThemes[baseStat].label);
                case SpecializedFamily.Maneuver:
                    return LocalizedText.Format("Text.SpecializedFeats.GetEffectText.Text.2", "Adds your {0} modifier (untyped) to CMB.", attributeName);
                case SpecializedFamily.Skilled:
                    return LocalizedText.Format("Text.SpecializedFeats.GetEffectText.Text", "Adds your {0} modifier (untyped) to {1}.", attributeName, SkilledThemes[baseStat].label);
                case SpecializedFamily.Arcane:
                    return LocalizedText.Format("Text.SpecializedFeats.GetEffectText.Text.3", "Adds your {0} modifier (untyped) to caster level and spell penetration checks, plus half your {0} modifier (rounded down) to spell and ability save DCs in Balanced mode, or the full modifier in Legacy_AllFull mode.", attributeName);
                default:
                    return LocalizedText.Invariant(string.Empty);
            }
        }

        private static LocalizedText GetRestrictionText(SpecializedFamily family)
        {
            switch (family)
            {
                case SpecializedFamily.Defensive:
                    return LocalizedText.Text("Text.SpecializedFeats.GetRestrictionText.Text", "While its exclusion group is on (configurable in mod settings), you can have only one Defensive Adept feat.");
                case SpecializedFamily.Maneuver:
                    return LocalizedText.Text("Text.SpecializedFeats.GetRestrictionText.Text.2", "While its exclusion group is on (configurable in mod settings), you can have only one Maneuver Adept feat.");
                case SpecializedFamily.Skilled:
                    return LocalizedText.Text("Text.SpecializedFeats.GetRestrictionText.Text.3", "While its exclusion group is on (configurable in mod settings), you can have only one Skilled feat.");
                case SpecializedFamily.Arcane:
                    return LocalizedText.Text("Text.SpecializedFeats.GetRestrictionText.Text.4", "While its exclusion group is on (configurable in mod settings), you can have only one Arcane Insight feat.");
                default:
                    return LocalizedText.Invariant(string.Empty);
            }
        }

        private static string GetFamilyKey(SpecializedFamily family)
        {
            switch (family)
            {
                case SpecializedFamily.Defensive:
                    return "Defensive";
                case SpecializedFamily.Maneuver:
                    return "Maneuver";
                case SpecializedFamily.Skilled:
                    return "Skilled";
                case SpecializedFamily.Arcane:
                    return "Arcane";
                default:
                    return family.ToString();
            }
        }

        private static LocalizedText GetFamilyDisplayName(SpecializedFamily family)
        {
            switch (family)
            {
                case SpecializedFamily.Defensive:
                    return LocalizedText.Text("Text.SpecializedFeats.GetFamilyDisplayName.Text", "Defensive Adept");
                case SpecializedFamily.Maneuver:
                    return LocalizedText.Text("Text.SpecializedFeats.GetFamilyDisplayName.Text.2", "Maneuver Adept");
                case SpecializedFamily.Skilled:
                    return LocalizedText.Text("Text.SpecializedFeats.GetFamilyDisplayName.Text.3", "Skilled");
                case SpecializedFamily.Arcane:
                    return LocalizedText.Text("Text.SpecializedFeats.GetFamilyDisplayName.Text.4", "Arcane Insight");
                default:
                    return LocalizedText.Invariant(family.ToString());
            }
        }

        private static string GetAttributeKey(StatType baseStat)
        {
            switch (baseStat)
            {
                case StatType.Strength:
                    return "Str";
                case StatType.Dexterity:
                    return "Dex";
                case StatType.Constitution:
                    return "Con";
                case StatType.Intelligence:
                    return "Int";
                case StatType.Wisdom:
                    return "Wis";
                case StatType.Charisma:
                    return "Cha";
                default:
                    return baseStat.ToString();
            }
        }

        private static LocalizedText GetAttributeName(StatType baseStat)
        {
            switch (baseStat)
            {
                case StatType.Strength:
                    return LocalizedText.Text("Text.SpecializedFeats.GetAttributeName.Text", "Strength");
                case StatType.Dexterity:
                    return LocalizedText.Text("Text.SpecializedFeats.GetAttributeName.Text.2", "Dexterity");
                case StatType.Constitution:
                    return LocalizedText.Text("Text.SpecializedFeats.GetAttributeName.Text.3", "Constitution");
                case StatType.Intelligence:
                    return LocalizedText.Text("Text.SpecializedFeats.GetAttributeName.Text.4", "Intelligence");
                case StatType.Wisdom:
                    return LocalizedText.Text("Text.SpecializedFeats.GetAttributeName.Text.5", "Wisdom");
                case StatType.Charisma:
                    return LocalizedText.Text("Text.SpecializedFeats.GetAttributeName.Text.6", "Charisma");
                default:
                    return LocalizedText.Invariant(baseStat.ToString());
            }
        }
    }
}
