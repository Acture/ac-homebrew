using System;
using System.Collections.Generic;
using System.Linq;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using BlueprintCore.Blueprints.CustomConfigurators.Classes.Selection;
using BlueprintCore.Blueprints.Configurators.Classes.Selection;
using BlueprintCore.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;

namespace ACHomebrew.Feats
{
    internal sealed class FeatSelection
    {
        public static readonly FeatSelection Root = new("Root", Guids.ACHomebrewSelection, "AC's Homebrew");
        public static readonly FeatSelection MainAttribute = new("MainAttribute", Guids.FeatSelections.MainAttribute, "Main Attribute Mastery");
        public static readonly FeatSelection Defensive = new("Defensive", Guids.FeatSelections.Defensive, "Defensive Adept");
        public static readonly FeatSelection Maneuver = new("Maneuver", Guids.FeatSelections.Maneuver, "Maneuver Adept");
        public static readonly FeatSelection ManeuverTactics = new("ManeuverTactics", Guids.ManeuverTactics.Selection, "Maneuver Tactics");
        public static readonly FeatSelection Skilled = new("Skilled", Guids.FeatSelections.Skilled, "Skilled Adept");
        public static readonly FeatSelection Arcane = new("Arcane", Guids.FeatSelections.Arcane, "Arcane Adept");
        public static readonly FeatSelection Stance = new("Stance", Guids.FeatSelections.Stance, "Stance");
        public static readonly FeatSelection Conditional = new("Conditional", Guids.FeatSelections.Conditional, "Conditional Trigger");
        public static readonly FeatSelection WeaponInsight = new("WeaponInsight", Guids.FeatSelections.WeaponInsight, "Weapon Insight");
        public static readonly FeatSelection ExtendedReplacement = new("ExtendedReplacement", Guids.FeatSelections.ExtendedReplacement, "Extended Replacement");
        public static readonly FeatSelection GreaterSummoning = new("GreaterSummoning", Guids.FeatSelections.GreaterSummoning, "Greater Summoning");
        public static readonly FeatSelection SummonerSacrifice = new("SummonerSacrifice", Guids.FeatSelections.SummonerSacrifice, "Summoner Sacrifice");
        public static readonly FeatSelection ReactiveArmor = new("ReactiveArmor", Guids.FeatSelections.ReactiveArmor, "Reactive Armor");
        public static readonly FeatSelection DerivedStat = new("DerivedStat", Guids.FeatSelections.DerivedStat, "Derived Stat Conversion");
        public static readonly FeatSelection SpellSchool = new("SpellSchool", Guids.FeatSelections.SpellSchool, "Spell School Specialist");
        public static readonly FeatSelection SpellDescriptor = new("SpellDescriptor", Guids.FeatSelections.SpellDescriptor, "Spell Descriptor Specialist");
        public static readonly FeatSelection DistanceDamage = new("DistanceDamage", Guids.FeatSelections.DistanceDamage, "Distance Damage");
        public static readonly FeatSelection WeaponDamage = new("WeaponDamage", Guids.FeatSelections.WeaponDamage, "Weapon Damage");
        public static readonly FeatSelection CastingStat = new("CastingStat", Guids.FeatSelections.CastingStat, "Casting Attribute");
        public static readonly FeatSelection ResourceStat = new("ResourceStat", Guids.FeatSelections.ResourceStat, "Resource Attribute");
        public static readonly FeatSelection Retaliation = new("Retaliation", Guids.FeatSelections.Retaliation, "Retaliation");
        public static readonly FeatSelection Momentum = new("Momentum", Guids.FeatSelections.Momentum, "Momentum");
        public static readonly FeatSelection Stealth = new("Stealth", Guids.FeatSelections.Stealth, "Stealth");
        public static readonly FeatSelection Solo = new("Solo", Guids.FeatSelections.Solo, "Solo");
        public static readonly FeatSelection Growth = new("Growth", Guids.FeatSelections.Growth, "Growth");
        public static readonly FeatSelection Survival = new("Survival", Guids.FeatSelections.Survival, "Survival");
        public static readonly FeatSelection Meme = new("Meme", Guids.FeatSelections.Meme, "Meme");
        public static readonly FeatSelection Penetration = new("Penetration", Guids.FeatSelections.Penetration, "Penetration");
        public static readonly FeatSelection Execution = new("Execution", Guids.FeatSelections.Execution, "Execution");
        public static readonly FeatSelection Arcana = new("Arcana", Guids.FeatSelections.Arcana, "Arcana");
        public static readonly FeatSelection Summoner = new("Summoner", Guids.FeatSelections.Summoner, "Summoner");

        // Main Attribute Mastery: choose a source attribute, then the one attribute it improves.
        public static readonly FeatSelection MainFromStr = new("MainAttribute_Str", Guids.MainAttribute.Source.Str, "Strength Mastery", MainAttribute);
        public static readonly FeatSelection MainFromDex = new("MainAttribute_Dex", Guids.MainAttribute.Source.Dex, "Dexterity Mastery", MainAttribute);
        public static readonly FeatSelection MainFromCon = new("MainAttribute_Con", Guids.MainAttribute.Source.Con, "Constitution Mastery", MainAttribute);
        public static readonly FeatSelection MainFromInt = new("MainAttribute_Int", Guids.MainAttribute.Source.Int, "Intelligence Mastery", MainAttribute);
        public static readonly FeatSelection MainFromWis = new("MainAttribute_Wis", Guids.MainAttribute.Source.Wis, "Wisdom Mastery", MainAttribute);
        public static readonly FeatSelection MainFromCha = new("MainAttribute_Cha", Guids.MainAttribute.Source.Cha, "Charisma Mastery", MainAttribute);

        // Retired 0.1.x Main feats: kept for existing characters and budgets, never offered.
        public static readonly FeatSelection MainLegacy = new("MainAttributeLegacy", null, "Main Attribute Mastery (legacy)", hidden: true);

        // Child menus come before their parent so the parent sees their configured selections.
        private static readonly FeatSelection[] Families =
        {
            MainFromStr, MainFromDex, MainFromCon, MainFromInt, MainFromWis, MainFromCha, MainLegacy,
            MainAttribute, Defensive, Maneuver, ManeuverTactics, Skilled, Arcane, Stance, Conditional,
            WeaponInsight, ExtendedReplacement, GreaterSummoning, SummonerSacrifice,
            ReactiveArmor, DerivedStat, SpellSchool, SpellDescriptor, DistanceDamage, WeaponDamage,
            CastingStat, ResourceStat, Retaliation, Momentum, Stealth, Solo, Growth, Execution, Arcana, Summoner, Survival, Meme, Penetration,
        };

        private readonly string Key;
        private readonly string Name;
        private readonly string Guid;
        private readonly string DisplayName;
        private readonly FeatSelection Parent;
        private readonly bool Hidden;
        private readonly Dictionary<BlueprintGuid, BlueprintFeature> Feats = new();
        private bool Configured;

        private FeatSelection(string name, string guid, string displayName, FeatSelection parent = null, bool hidden = false)
        {
            Key = name;
            Name = $"ACHomebrewSelection_{name}";
            Guid = guid;
            DisplayName = displayName;
            Parent = parent;
            Hidden = hidden;
        }

        // Keep the Feat group for game mechanics, but expose each feat only inside its family.
        public FeatureConfigurator NewFeat(string name, string guid)
            => FeatureConfigurator.New(name, guid, FeatureGroup.Feat)
                .SkipAddToSelections()
                .OnConfigure(feat => Feats[feat.AssetGuid] = feat);

        /// <summary>Adds an already configured feat to this family.</summary>
        public void Register(BlueprintFeature feat) => Feats[feat.AssetGuid] = feat;

        // Parametrized configurators do not auto-register in outer selections.
        public ParametrizedFeatureConfigurator NewParametrizedFeat(string name, string guid)
            => ParametrizedFeatureConfigurator.New(name, guid)
                .SetGroups(FeatureGroup.Feat)
                .OnConfigure(feat => Feats[feat.AssetGuid] = feat);

        /// <summary>
        /// Collects a batch only if its registration finishes, including prerequisites and mutex.
        /// On failure, removes its menu entries without deleting already-created blueprints.
        /// </summary>
        internal static void CollectRegistration(Action register)
        {
            var snapshots = Families.Concat(new[] { Root })
                .ToDictionary(family => family, family => family.Feats.ToArray());
            try
            {
                register();
            }
            catch
            {
                foreach (var snapshot in snapshots)
                {
                    snapshot.Key.Feats.Clear();
                    foreach (var feat in snapshot.Value)
                        snapshot.Key.Feats.Add(feat.Key, feat.Value);
                }
                throw;
            }
        }

        /// <summary>
        /// Registered leaf feats with their family's budget cost. Menus are excluded, so
        /// opening the root or a family never consumes budget.
        /// </summary>
        internal static IEnumerable<(BlueprintFeature feat, string family, int cost)> BudgetedFeats()
            => Families.Concat(new[] { Root })
                .SelectMany(family => family.Feats.Values
                    .Where(feat => feat is not BlueprintFeatureSelection)
                    .Select(feat => (feat, family.Key, FeatBudgetRules.FamilyCost(family.Key))));

        /// <summary>Logs a failed registration step without preventing unrelated steps from running.</summary>
        internal static bool TryConfigure(string name, Action configure)
        {
            try
            {
                configure();
                return true;
            }
            catch (Exception error)
            {
                Mod.Log?.Log($"ACHomebrew: {name} failed - {error}");
                return false;
            }
        }

        /// <summary>Publishes successful family menus even when another family's menu fails.</summary>
        public static bool ConfigureAll()
        {
            var succeeded = true;
            foreach (var family in Families)
            {
                succeeded &= TryConfigure(family.Name, family.Configure);
            }
            succeeded &= TryConfigure(Root.Name, Root.Configure);
            return succeeded;
        }

        private void Configure()
        {
            if (Configured || Hidden || Feats.Count == 0) return;

            var selection = FeatureSelectionConfigurator.New(Name, Guid, FeatureGroup.Feat)
                .SkipAddToSelections()
                .SetDisplayName(Common.L($"{Name}.Name", DisplayName))
                .SetDescription(Common.L(
                    $"{Name}.Description",
                    "Choose one feat. Each choice grants only the selected feat. " +
                    "You can return to this selection whenever you gain another feat; " +
                    "each feat's normal prerequisites and mutual-exclusion rules still apply."))
                .SetGroup(FeatureGroup.Feat)
                // Family selections can be revisited; only the final feats must be new.
                // Parametrized families can be revisited for a new parameter.
                .SetMode(this == Root || this == WeaponDamage || this == CastingStat || this == ResourceStat || this == Meme ? SelectionMode.Default : SelectionMode.OnlyNew)
                .SetIgnorePrerequisites(false)
                .SetAllFeatures(Feats.Values.Select(feat => (Blueprint<BlueprintFeatureReference>)feat).ToArray())
                .Configure();

            if (this == Root)
            {
                // Register only the root in the same outer lists as the original feats.
                FeatureSelectionConfigurator.For(selection, updateSelections: true).Configure();
            }
            else
            {
                (Parent ?? Root).Feats[selection.AssetGuid] = selection;
            }
            Configured = true;
            Mod.Log?.Log($"ACHomebrew: grouped {Feats.Count} options under {DisplayName}.");
        }
    }
}
