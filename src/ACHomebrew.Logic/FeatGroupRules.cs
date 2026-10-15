using System.Collections.Generic;
using System.Linq;

namespace ACHomebrew.Feats
{
    /// <summary>
    /// A configurable exclusion group: a character may own at most <see cref="DefaultMax"/>
    /// distinct member feats. Members are explicit feats and/or every menu family whose key
    /// starts with <see cref="Family"/> (so "MainAttribute" covers its source menus and legacy feats).
    /// Groups sharing a <see cref="SettingId"/> share one on/off switch and limit.
    /// </summary>
    internal sealed class FeatGroup
    {
        public string Id;
        public string SettingId;
        /// <summary>English name; translations live in the catalog under <see cref="NameKey"/>.</summary>
        public string NameEn;
        public string NameKey => "FeatGroup." + SettingId;
        public bool DefaultEnabled = true;
        public int DefaultMax = 1;
        public string Family;
        public string[] Members = new string[0];
    }

    internal readonly struct FeatGroupLimit
    {
        public readonly bool Enabled;
        public readonly int Max;

        public FeatGroupLimit(bool enabled, int max)
        {
            Enabled = enabled;
            Max = FeatBudgetRules.ClampLimit(max);
        }
    }

    internal static class FeatGroupRules
    {
        private static FeatGroup Family(string family, string en, int max = 1)
            => new() { Id = family, SettingId = family, Family = family, NameEn = en, DefaultMax = max };

        private static FeatGroup Explicit(string id, string en, int max, bool enabled, params string[] members)
            => new() { Id = id, SettingId = id, NameEn = en, DefaultMax = max, DefaultEnabled = enabled, Members = members };

        // Every Main feat whose source is this attribute, plus the retired broad Main feat.
        private static IEnumerable<string> MainFrom(string attribute)
            => typeof(Guids.MainAttribute).GetNestedType(attribute).GetFields()
                .Select(field => (string)field.GetValue(null));

        private static FeatGroup SameAttribute(string attribute, params string[] members)
            => new()
            {
                Id = "SameAttribute_" + attribute,
                SettingId = "SameAttribute",
                NameEn = "Same attribute across Main, Adept and Stance",
                DefaultEnabled = false,
                Members = members.Concat(MainFrom(attribute)).ToArray(),
            };

        // Feats within one distance style stack; feats from different styles exclude each other.
        private static readonly string[][] DistanceStyleSets =
        {
            new[] { Guids.DistanceDamage.AggressorsEdge, Guids.DistanceDamage.ShortBlade, Guids.DistanceDamage.CloseQuarters },
            new[] { Guids.DistanceDamage.OptimalRange, Guids.DistanceDamage.LongHaft, Guids.DistanceDamage.ReachControl },
            new[] { Guids.DistanceDamage.MarksmansFocus, Guids.DistanceDamage.StrongBow, Guids.DistanceDamage.SteadySniper },
        };

        private static IEnumerable<FeatGroup> DistanceStyles()
        {
            var index = 0;
            for (var a = 0; a < DistanceStyleSets.Length; a++)
                for (var b = a + 1; b < DistanceStyleSets.Length; b++)
                    foreach (var first in DistanceStyleSets[a])
                        foreach (var second in DistanceStyleSets[b])
                            yield return new FeatGroup
                            {
                                Id = "DistanceStyle_" + index++,
                                SettingId = "DistanceStyle",
                                NameEn = "One distance style (close, mid or long range)",
                                Members = new[] { first, second },
                            };
        }

        public static readonly FeatGroup[] All = new[]
        {
            Family("MainAttribute", "Main Attribute Mastery (one per character)"),
            Family("Defensive", "Defensive Adept"),
            Family("Maneuver", "Maneuver Adept"),
            Family("Skilled", "Skilled Adept"),
            Family("Arcane", "Arcane Adept"),
            Family("Stance", "Stance"),
            Family("WeaponInsight", "Weapon Insight"),
            Family("WeaponDamage", "Weapon Damage attribute"),
            Family("GreaterSummoning", "Greater Summoning"),
            Family("SummonerSacrifice", "Summoner Sacrifice"),
            Family("SpellSchool", "Spell School Specialist"),
            Family("SpellDescriptor", "Spell Descriptor Specialist"),
            Family("ReactiveArmor", "Reactive Armor"),
            Family("Conditional", "Conditional Trigger", max: 2),
            Explicit("AcReplacement", "Attribute replaces armor class", 1, true,
                Guids.Replacement.Extended.InnerSentinel, Guids.ExtendedReplacement2.LightfootDefense),
            Explicit("CmdReplacement", "Attribute replaces CMD", 1, true,
                Guids.Replacement.Extended.UnyieldingWill, Guids.ExtendedReplacement2.BrutalDefender),
            Explicit("DerivedAc", "Derived armor class", 1, true,
                Guids.Derived.ArcaneAegis, Guids.Derived.SkilledDefender),
            Explicit("CasterLevelVitality", "Caster level to hit points", 1, true,
                Guids.Derived.MysticVitality, Guids.Derived.SoulBulwark),
            Explicit("AttributeToAc", "Attribute bonuses to armor class", 2, true,
                Guids.Specialized.Defensive.Dex, Guids.Specialized.Defensive.Con,
                Guids.Stance.Feature.Dex, Guids.Stance.Feature.Wis,
                Guids.Replacement.Extended.InnerSentinel, Guids.ExtendedReplacement2.LightfootDefense,
                Guids.Conditional.EndlessResolve),
            SameAttribute("Str", Guids.str_main_to_everything, Guids.Specialized.Defensive.Str, Guids.Specialized.Maneuver.Str,
                Guids.Specialized.Skilled.Str, Guids.Specialized.Arcane.Str, Guids.Stance.Feature.Str),
            SameAttribute("Dex", Guids.dex_main_to_everything, Guids.Specialized.Defensive.Dex, Guids.Specialized.Maneuver.Dex,
                Guids.Specialized.Skilled.Dex, Guids.Specialized.Arcane.Dex, Guids.Stance.Feature.Dex),
            SameAttribute("Con", Guids.con_main_to_everything, Guids.Specialized.Defensive.Con, Guids.Specialized.Maneuver.Con,
                Guids.Specialized.Skilled.Con, Guids.Specialized.Arcane.Con, Guids.Stance.Feature.Con),
            SameAttribute("Int", Guids.int_main_to_everything, Guids.Specialized.Defensive.Int, Guids.Specialized.Maneuver.Int,
                Guids.Specialized.Skilled.Int, Guids.Specialized.Arcane.Int, Guids.Stance.Feature.Int),
            SameAttribute("Wis", Guids.wis_main_to_everything, Guids.Specialized.Defensive.Wis, Guids.Specialized.Maneuver.Wis,
                Guids.Specialized.Skilled.Wis, Guids.Specialized.Arcane.Wis, Guids.Stance.Feature.Wis),
            SameAttribute("Cha", Guids.cha_main_to_everything, Guids.Specialized.Defensive.Cha, Guids.Specialized.Maneuver.Cha,
                Guids.Specialized.Skilled.Cha, Guids.Specialized.Arcane.Cha, Guids.Stance.Feature.Cha),
        }.Concat(DistanceStyles()).ToArray();

        /// <summary>Normalizes "N" and "D" GUID spellings for comparison.</summary>
        public static string Key(string guid) => guid.Replace("-", "").ToLowerInvariant();

        /// <summary>One entry per configurable switch, in display order.</summary>
        public static IEnumerable<FeatGroup> Settings => All.GroupBy(group => group.SettingId).Select(group => group.First());

        /// <summary>Resolves each group's members from registered (guid, family) feats.</summary>
        public static Dictionary<FeatGroup, HashSet<string>> Resolve(IEnumerable<(string guid, string family)> feats)
        {
            var list = feats.ToList();
            return All.ToDictionary(group => group, group => new HashSet<string>(
                group.Members.Concat(list.Where(feat => group.Family != null && feat.family != null
                        && feat.family.StartsWith(group.Family, System.StringComparison.Ordinal)).Select(feat => feat.guid))
                    .Select(Key)));
        }

        /// <summary>
        /// Groups that block <paramref name="candidate"/>. Only distinct other members count,
        /// so re-taking a parametrized member for a new parameter is never blocked by its own group.
        /// </summary>
        public static List<FeatGroup> Blocking(IReadOnlyDictionary<FeatGroup, HashSet<string>> members,
            System.Func<FeatGroup, FeatGroupLimit> limit, ISet<string> owned, string candidate)
        {
            var blocking = new List<FeatGroup>();
            foreach (var entry in members)
            {
                if (!entry.Value.Contains(candidate)) continue;
                var groupLimit = limit(entry.Key);
                if (!groupLimit.Enabled) continue;
                var others = entry.Value.Count(member => member != candidate && owned.Contains(member));
                if (others + 1 > groupLimit.Max) blocking.Add(entry.Key);
            }
            return blocking;
        }
    }
}
