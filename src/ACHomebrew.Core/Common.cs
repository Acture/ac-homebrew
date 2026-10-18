using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using BlueprintCore.Blueprints.Configurators.Classes.Selection;
using BlueprintCore.Utils;
using BlueprintCore.Utils.Types;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Buffs;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Abilities;
using BlueprintCore.Blueprints.Configurators.UnitLogic.ActivatableAbilities;

namespace ACHomebrew.Feats
{
    internal enum ScalingIntent
    {
        Full,
        Half,
    }

    internal static partial class Common
    {
        private static readonly ConditionalWeakTable<FeatureConfigurator, HashSet<string>> RankRegistrations = new();

        public static ContextValue Rank(AbilityRankType type = AbilityRankType.Default)
            => new()
            {
                ValueType = ContextValueType.Rank,
                ValueRank = type,
            };

        public static void AddRank(FeatureConfigurator cfg, StatType baseStat, AbilityRankType type, ContextRankProgression prog)
        {
            var registrations = RankRegistrations.GetValue(cfg, _ => new HashSet<string>());
            var key = $"{baseStat}:{type}";
            if (!registrations.Add(key))
            {
                Mod.Log?.Log($"ACHomebrew: duplicate ContextRankConfig skipped for {baseStat}/{type}.");
                return;
            }

            var config = ContextRankConfigs.StatBonus(baseStat, ModifierDescriptor.None, type, min: 0);
            switch (prog)
            {
                case ContextRankProgression.AsIs:
                    break;
                case ContextRankProgression.Div2:
                    config = config.WithDiv2Progression();
                    break;
                case ContextRankProgression.HalfMore:
                    config = config.WithHalfMoreProgression();
                    break;
                default:
                    Mod.Log?.Log($"ACHomebrew: unsupported progression {prog}, falling back to AsIs.");
                    break;
            }

            cfg.AddContextRankConfig(config);
        }

        public static ContextRankProgression ResolveProgression(PowerLevel powerLevel, ScalingIntent intent)
            => (powerLevel, intent) switch
            {
                (PowerLevel.Balanced, ScalingIntent.Full) => ContextRankProgression.AsIs,
                (PowerLevel.Balanced, ScalingIntent.Half) => ContextRankProgression.Div2,
                (PowerLevel.Legacy_AllFull, ScalingIntent.Full) => ContextRankProgression.AsIs,
                (PowerLevel.Legacy_AllFull, ScalingIntent.Half) => ContextRankProgression.AsIs,
                _ => ContextRankProgression.AsIs,
            };


        public static FeatureConfigurator SetIconIfPresent(this FeatureConfigurator cfg, string internalName)
        {
            var icon = IconLoader.Get(internalName);
            if (icon != null)
            {
                cfg.SetIcon(icon);
            }
            return cfg;
        }

        public static ParametrizedFeatureConfigurator SetIconIfPresent(this ParametrizedFeatureConfigurator cfg, string internalName)
        {
            var icon = IconLoader.Get(internalName);
            if (icon != null)
            {
                cfg.SetIcon(icon);
            }
            return cfg;
        }

        public static BuffConfigurator SetIconIfPresent(this BuffConfigurator cfg, string internalName)
        {
            var icon = IconLoader.Get(internalName);
            if (icon != null)
            {
                cfg.SetIcon(icon);
            }
            return cfg;
        }

        public static ActivatableAbilityConfigurator SetIconIfPresent(this ActivatableAbilityConfigurator cfg, string internalName)
        {
            var icon = IconLoader.Get(internalName);
            if (icon != null)
            {
                cfg.SetIcon(icon);
            }
            return cfg;
        }

        public static AbilityConfigurator SetIconIfPresent(this AbilityConfigurator cfg, string internalName)
        {
            var icon = IconLoader.Get(internalName);
            if (icon != null)
            {
                cfg.SetIcon(icon);
            }
            return cfg;
        }
    }
}
