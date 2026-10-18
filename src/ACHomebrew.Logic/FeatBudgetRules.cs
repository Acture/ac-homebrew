using System.Collections.Generic;

namespace ACHomebrew.Feats
{
    /// <summary>Per-character limits. A disabled limit never blocks a choice.</summary>
    internal readonly struct FeatBudgetLimits
    {
        public readonly bool CountEnabled;
        public readonly int MaxCount;
        public readonly bool PointsEnabled;
        public readonly int MaxPoints;

        public FeatBudgetLimits(bool countEnabled, int maxCount, bool pointsEnabled, int maxPoints)
        {
            CountEnabled = countEnabled;
            MaxCount = FeatBudgetRules.ClampLimit(maxCount);
            PointsEnabled = pointsEnabled;
            MaxPoints = FeatBudgetRules.ClampLimit(maxPoints);
        }

        public bool AnyEnabled => CountEnabled || PointsEnabled;
    }

    internal readonly struct FeatBudgetUsage
    {
        public readonly int Count;
        public readonly int Points;

        public FeatBudgetUsage(int count, int points)
        {
            Count = count;
            Points = points;
        }
    }

    internal enum FeatBudgetBlock
    {
        None,
        CountFull,
        PointsShort,
    }

    internal readonly struct FeatBudgetVerdict
    {
        public readonly FeatBudgetLimits Limits;
        public readonly FeatBudgetUsage Usage;
        public readonly int Cost;
        public readonly FeatBudgetBlock Block;

        public FeatBudgetVerdict(FeatBudgetLimits limits, FeatBudgetUsage usage, int cost, FeatBudgetBlock block)
        {
            Limits = limits;
            Usage = usage;
            Cost = cost;
            Block = block;
        }

        public bool Allowed => Block == FeatBudgetBlock.None;
        public int RemainingCount => Limits.MaxCount - Usage.Count;
        public int RemainingPoints => Limits.MaxPoints - Usage.Points;
        public int PointsShortBy => Cost - RemainingPoints;

        /// <summary>Existing feats exceed an enabled limit, e.g. after lowering it or loading an old save.</summary>
        public bool OverBudget => (Limits.CountEnabled && Usage.Count > Limits.MaxCount)
            || (Limits.PointsEnabled && Usage.Points > Limits.MaxPoints);
    }

    /// <summary>
    /// Game-independent budget rules. Only leaf feats granted to a character are
    /// tallied; menus are never passed in. Each owned fact counts once per rank, so
    /// every parameter of a parametrized feat (e.g. each Weapon Damage category) is
    /// a separate feat with its own cost. Limits never remove owned feats.
    /// </summary>
    internal static class FeatBudgetRules
    {
        public const int MaxLimit = 99;
        public const int DefaultMaxCount = 6;
        public const int DefaultMaxPoints = 10;

        // Initial strength tiers; numeric rebalancing of individual feats is tracked separately.
        private static readonly Dictionary<string, int> FamilyCosts = new()
        {
            // Main: a source menu per attribute, one target attribute per feat.
            ["MainAttribute_Str"] = 2,
            ["MainAttribute_Dex"] = 2,
            ["MainAttribute_Con"] = 2,
            ["MainAttribute_Int"] = 2,
            ["MainAttribute_Wis"] = 2,
            ["MainAttribute_Cha"] = 2,
            // Retired broad Main feats still owned by existing characters.
            ["MainAttributeLegacy"] = 3,
            ["WeaponInsight"] = 2,
            ["ExtendedReplacement"] = 2,
            ["Stance"] = 2,
            ["GreaterSummoning"] = 2,
            ["SummonerSacrifice"] = 2,
            ["DerivedStat"] = 2,
            ["SpellSchool"] = 2,
            ["SpellDescriptor"] = 2,
            ["WeaponDamage"] = 2,
            ["CastingStat"] = 2,
            ["ResourceStat"] = 1,
            ["Retaliation"] = 1,
            ["Momentum"] = 1,
            ["Stealth"] = 2,
            ["Solo"] = 2,
            ["Growth"] = 1,
            ["Survival"] = 2,
            ["Penetration"] = 2,
            // Meme feats are jokes and do not consume budget points.
            ["Meme"] = 0,
            ["Execution"] = 1,
            ["Arcana"] = 2,
            ["Summoner"] = 1,
            ["Defensive"] = 1,
            ["Maneuver"] = 1,
            ["ManeuverTactics"] = 2,
            ["Skilled"] = 1,
            ["Arcane"] = 1,
            ["Conditional"] = 1,
            ["ReactiveArmor"] = 1,
            ["DistanceDamage"] = 1,
            // Feats placed directly in the root menu (Long-Reach Gambit).
            ["Root"] = 1,
        };

        public static IReadOnlyDictionary<string, int> Costs => FamilyCosts;

        public static int FamilyCost(string family)
            => family != null && FamilyCosts.TryGetValue(family, out var cost) ? cost : 1;

        public static int ClampLimit(int value) => value < 0 ? 0 : value > MaxLimit ? MaxLimit : value;

        /// <summary>Tallies owned feat facts as (cost, rank) pairs.</summary>
        public static FeatBudgetUsage Tally(IEnumerable<(int cost, int rank)> owned)
        {
            var count = 0;
            var points = 0;
            foreach (var (cost, rank) in owned)
            {
                if (rank <= 0) continue;
                count += rank;
                points += cost * rank;
            }
            return new FeatBudgetUsage(count, points);
        }

        /// <summary>Decides whether one more feat of <paramref name="cost"/> fits within both enabled limits.</summary>
        public static FeatBudgetVerdict Evaluate(FeatBudgetLimits limits, FeatBudgetUsage usage, int cost)
        {
            var block = FeatBudgetBlock.None;
            if (limits.CountEnabled && usage.Count + 1 > limits.MaxCount)
                block = FeatBudgetBlock.CountFull;
            else if (limits.PointsEnabled && usage.Points + cost > limits.MaxPoints)
                block = FeatBudgetBlock.PointsShort;
            return new FeatBudgetVerdict(limits, usage, cost, block);
        }
    }
}
