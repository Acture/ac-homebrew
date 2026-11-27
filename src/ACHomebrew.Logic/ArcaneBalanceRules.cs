namespace ACHomebrew.Feats
{
    /// <summary>Armor the wearer has on, for Arcane Aegis. Names match the game's armor groups.</summary>
    internal enum ArcaneAegisArmor
    {
        Missing,
        Unarmored,
        Light,
        Medium,
        Heavy,
        Shield,
        Barding,
        Other,
    }

    /// <summary>
    /// Scheme A: half caster level (rounded down) to AC, only with no armor or light armor and no shield.
    /// </summary>
    internal static class ArcaneAegisRules
    {
        public static int ArmorClassBonus(int casterLevel)
            => casterLevel > 0 ? casterLevel / 2 : 0;

        public static bool Applies(ArcaneAegisArmor armor, bool holdingShield)
            => !holdingShield && (armor == ArcaneAegisArmor.Missing
                || armor == ArcaneAegisArmor.Unarmored
                || armor == ArcaneAegisArmor.Light);

        public static ArcaneAegisArmor ClassifyGroup(string proficiencyGroup)
        {
            switch (proficiencyGroup)
            {
                case null:
                case "":
                case "None":
                    return ArcaneAegisArmor.Unarmored;
                case "Light":
                    return ArcaneAegisArmor.Light;
                case "Medium":
                    return ArcaneAegisArmor.Medium;
                case "Heavy":
                    return ArcaneAegisArmor.Heavy;
                case "Buckler":
                case "LightShield":
                case "HeavyShield":
                case "TowerShield":
                    return ArcaneAegisArmor.Shield;
                case "LightBarding":
                case "MediumBarding":
                case "HeavyBarding":
                    return ArcaneAegisArmor.Barding;
                default:
                    return ArcaneAegisArmor.Other;
            }
        }
    }

    /// <summary>
    /// Scheme C for Arcane Insight. Divisors are the Balanced fractions.
    /// Legacy_AllFull keeps every bonus on the full modifier. Division is toward zero
    /// after a minimum modifier of 0, matching the game's rank progressions.
    /// </summary>
    internal static class ArcaneSpecializationRules
    {
        public const int CasterLevelDivisor = 2;
        public const int SpellPenetrationDivisor = 1;
        public const int SpellDcDivisor = 4;

        public static int Divisor(int balancedDivisor, bool legacyAllFull)
            => legacyAllFull || balancedDivisor < 1 ? 1 : balancedDivisor;

        public static int CasterLevelBonus(int attributeModifier, bool legacyAllFull)
            => Scale(attributeModifier, CasterLevelDivisor, legacyAllFull);

        public static int SpellPenetrationBonus(int attributeModifier, bool legacyAllFull)
            => Scale(attributeModifier, SpellPenetrationDivisor, legacyAllFull);

        public static int SpellDcBonus(int attributeModifier, bool legacyAllFull)
            => Scale(attributeModifier, SpellDcDivisor, legacyAllFull);

        public static int Scale(int attributeModifier, int balancedDivisor, bool legacyAllFull)
        {
            if (attributeModifier <= 0) return 0;
            return attributeModifier / Divisor(balancedDivisor, legacyAllFull);
        }
    }
}
