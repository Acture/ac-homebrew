using System;

namespace ACHomebrew.Feats
{
    internal static class FeatRegistry
    {
        private static bool InitializationAttempted;

        /// <summary>
        /// Attempts each registration independently, then publishes the available menus.
        /// A partial attempt is not replayed because created blueprints cannot be rolled back.
        /// </summary>
        public static void ConfigureAll()
        {
            if (InitializationAttempted) return;
            InitializationAttempted = true;

            // Lambdas keep type loading and registration inside each exception boundary.
            // Use &= so later steps still run after an earlier failure.
            var succeeded = TryConfigureFamily(nameof(MainAbilityToEverything_Feats), () => MainAbilityToEverything_Feats.ConfigureAll());
            succeeded &= TryConfigureFamily(nameof(SpecializedFeats), () => SpecializedFeats.ConfigureAll());
            succeeded &= TryConfigureFamily(nameof(ManeuverFeats), () => ManeuverFeats.ConfigureAll());
            succeeded &= TryConfigureFamily(nameof(DuelFeats), () => DuelFeats.ConfigureAll());
            succeeded &= TryConfigureFamily(nameof(StanceFeats), () => StanceFeats.ConfigureAll());
            succeeded &= TryConfigureFamily(nameof(ConditionalFeats), () => ConditionalFeats.ConfigureAll());
            succeeded &= TryConfigureFamily(nameof(StatReplacementFeats), () => StatReplacementFeats.ConfigureAll());
            succeeded &= TryConfigureFamily(nameof(ReactiveArmorFeats), () => ReactiveArmorFeats.ConfigureAll());
            succeeded &= TryConfigureFamily(nameof(DerivedStatFeats), () => DerivedStatFeats.ConfigureAll());
            succeeded &= TryConfigureFamily(nameof(GreaterSummoningFeats), () => GreaterSummoningFeats.ConfigureAll());
            succeeded &= TryConfigureFamily(nameof(SpellTagFeats), () => SpellTagFeats.ConfigureAll());
            succeeded &= TryConfigureFamily(nameof(SummonerSacrificeFeats), () => SummonerSacrificeFeats.ConfigureAll());
            succeeded &= TryConfigureFamily(nameof(PolearmMasterFeats), () => PolearmMasterFeats.ConfigureAll());
            succeeded &= TryConfigureFamily(nameof(DistanceDamageFeats), () => DistanceDamageFeats.ConfigureAll());
            succeeded &= TryConfigureFamily(nameof(WeaponDamageFeats), () => WeaponDamageFeats.ConfigureAll());
            succeeded &= TryConfigureFamily(nameof(CastingStatFeats), () => CastingStatFeats.ConfigureAll());
            succeeded &= TryConfigureFamily(nameof(ResourceStatFeats), () => ResourceStatFeats.ConfigureAll());
            succeeded &= TryConfigureFamily(nameof(RetaliationFeats), () => RetaliationFeats.ConfigureAll());
            succeeded &= TryConfigureFamily(nameof(MomentumFeats), () => MomentumFeats.ConfigureAll());
            succeeded &= TryConfigureFamily(nameof(ExecutionFeats), () => ExecutionFeats.ConfigureAll());
            succeeded &= TryConfigureFamily(nameof(ArcanaFeats), () => ArcanaFeats.ConfigureAll());
            succeeded &= TryConfigureFamily(nameof(SummonerFeats), () => SummonerFeats.ConfigureAll());
            succeeded &= TryConfigureFamily(nameof(StealthFeats), () => StealthFeats.ConfigureAll());
            succeeded &= TryConfigureFamily(nameof(SoloFeats), () => SoloFeats.ConfigureAll());
            succeeded &= TryConfigureFamily(nameof(GrowthFeats), () => GrowthFeats.ConfigureAll());
            succeeded &= TryConfigureFamily(nameof(SurvivalFeats), () => SurvivalFeats.ConfigureAll());
            succeeded &= TryConfigureFamily(nameof(MemeFeats), () => MemeFeats.ConfigureAll());
            succeeded &= TryConfigureFamily(nameof(PenetrationFeats), () => PenetrationFeats.ConfigureAll());
            succeeded &= TryConfigureFamily(nameof(WeaponPenetrationFeats), () => WeaponPenetrationFeats.ConfigureAll());
            succeeded &= TryConfigure(nameof(MutexPass), () => MutexPass.ApplyAll());

            var menusSucceeded = false;
            succeeded &= TryConfigure(nameof(FeatSelection), () => menusSucceeded = FeatSelection.ConfigureAll());
            succeeded &= menusSucceeded;
            succeeded &= TryConfigure(nameof(FeatBudget), FeatBudget.Install);
            Main.Log?.Log(succeeded
                ? "ACHomebrew: registry initialized."
                : "ACHomebrew: initialization finished with errors; some feats or menus may be unavailable. See the named failures above.");
        }

        private static bool TryConfigureFamily(string name, Action configure)
            => TryConfigure(name, () => FeatSelection.CollectRegistration(configure));

        /// <summary>Logs a failed registration step without preventing unrelated steps from running.</summary>
        internal static bool TryConfigure(string name, Action configure) => FeatSelection.TryConfigure(name, configure);
    }
}
