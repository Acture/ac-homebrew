using System.Collections.Generic;

namespace ACHomebrew.Feats
{
    /// <summary>
    /// Caster level source for the derived feats: the highest class level among the unit's
    /// classes that have a spellbook. Never a sum across classes, and 0 when no class has one.
    /// </summary>
    internal static class DerivedStatRules
    {
        public static int HighestSpellbookClassLevel(IEnumerable<int> spellbookClassLevels)
        {
            var highest = 0;
            foreach (var level in spellbookClassLevels)
            {
                if (level > highest)
                {
                    highest = level;
                }
            }
            return highest;
        }
    }
}
