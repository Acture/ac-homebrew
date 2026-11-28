using BlueprintCore.Actions.Builder;
using BlueprintCore.Actions.Builder.ContextEx;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Abilities;
using BlueprintCore.Blueprints.Configurators.UnitLogic.Abilities;
using BlueprintCore.Utils;
using BlueprintCore.Utils.Types;
using Kingmaker;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using Kingmaker.Visual.Animation.Kingmaker.Actions;

namespace ACHomebrew.Feats
{
    internal static class Oss439Spells
    {
        private static bool Initialized;

        public static void ConfigureAll()
        {
            if (Initialized) return;
            Initialized = true;
            MeatHook();
            Vacuum();
            WallOfDuplicates();
            BlackHole();
            ChainFrost();
            SonicWave();
            GlobalSilence();
        }

        private static LocalizedText T(string key) => LocalizedText.Text(
            "Text.Oss439." + key + ".Name",
            System.Text.RegularExpressions.Regex.Replace(key,
                "([a-z])([A-Z])", "$1 $2"));

        static void MeatHook()
        {
            AbilityConfigurator.NewSpell("MeatHook", Guids.Oss439Spells.MeatHook,
                    SpellSchool.Conjuration, false, SpellDescriptor.Force)
                .SetDisplayName(Common.L("Text.Oss439.MeatHook.Name", T("MeatHook")))
                .SetDescription(Common.L("Text.Oss439.MeatHook.Desc", T("MeatHook")))
                .SetRange(AbilityRange.Close)
                .SetActionType(UnitCommand.CommandType.Standard)
                .SetAnimation(UnitAnimationActionCastSpell.CastAnimationStyle.Directional)
                .SetSpellResistance(true)
                .AllowTargeting(enemies: true)
                .SetEffectOnEnemy(AbilityEffectOnUnit.Harmful)
                .SetLocalizedSavingThrow(SavingThrow.ReflexNegates)
                .AddContextRankConfig(ContextRankConfigs.CasterLevel())
                .AddToSpellLists(3, SpellList.Wizard, SpellList.Magus, SpellList.Inquisitor)
                .Configure();
        }

        static void Vacuum()
        {
            AbilityConfigurator.NewSpell("Vacuum", Guids.Oss439Spells.Vacuum,
                    SpellSchool.Conjuration, false, SpellDescriptor.Force)
                .SetDisplayName(Common.L("Text.Oss439.Vacuum.Name", T("Vacuum")))
                .SetDescription(Common.L("Text.Oss439.Vacuum.Desc", T("Vacuum")))
                .SetRange(AbilityRange.Close)
                .SetActionType(UnitCommand.CommandType.Standard)
                .SetAnimation(UnitAnimationActionCastSpell.CastAnimationStyle.Omni)
                .SetSpellResistance(false)
                .AllowTargeting(point: true)
                .SetEffectOnEnemy(AbilityEffectOnUnit.Harmful)
                .SetLocalizedSavingThrow(SavingThrow.ReflexNegates)
                .AddContextRankConfig(ContextRankConfigs.CasterLevel())
                .AddToSpellLists(4, SpellList.Wizard, SpellList.Druid, SpellList.Shaman)
                .Configure();
        }

        static void WallOfDuplicates()
        {
            AbilityConfigurator.NewSpell("WallOfDuplicates",
                    Guids.Oss439Spells.WallOfDuplicates,
                    SpellSchool.Illusion, false)
                .SetDisplayName(Common.L("Text.Oss439.WallOfDuplicates.Name",
                    T("WallOfDuplicates")))
                .SetDescription(Common.L("Text.Oss439.WallOfDuplicates.Desc",
                    T("WallOfDuplicates")))
                .SetRange(AbilityRange.Close)
                .SetActionType(UnitCommand.CommandType.Standard)
                .SetAnimation(UnitAnimationActionCastSpell.CastAnimationStyle.Directional)
                .SetSpellResistance(false)
                .AllowTargeting(point: true)
                .SetEffectOnEnemy(AbilityEffectOnUnit.Harmful)
                .SetLocalizedDuration(Duration.RoundPerLevel)
                .AddContextRankConfig(ContextRankConfigs.CasterLevel())
                .AddToSpellLists(6, SpellList.Wizard, SpellList.Witch)
                .Configure();
        }

        static void BlackHole()
        {
            AbilityConfigurator.NewSpell("BlackHole", Guids.Oss439Spells.BlackHole,
                    SpellSchool.Conjuration, false, SpellDescriptor.Force)
                .SetDisplayName(Common.L("Text.Oss439.BlackHole.Name", T("BlackHole")))
                .SetDescription(Common.L("Text.Oss439.BlackHole.Desc", T("BlackHole")))
                .SetRange(AbilityRange.Close)
                .SetActionType(UnitCommand.CommandType.Standard)
                .SetAnimation(UnitAnimationActionCastSpell.CastAnimationStyle.Omni)
                .SetSpellResistance(true)
                .AllowTargeting(point: true)
                .SetEffectOnEnemy(AbilityEffectOnUnit.Harmful)
                .SetLocalizedSavingThrow(SavingThrow.FortNegates)
                .AddContextRankConfig(ContextRankConfigs.CasterLevel())
                .AddToSpellLists(7, SpellList.Wizard, SpellList.Witch)
                .Configure();
        }

        static void ChainFrost()
        {
            AbilityConfigurator.NewSpell("ChainFrost", Guids.Oss439Spells.ChainFrost,
                    SpellSchool.Evocation, false, SpellDescriptor.Cold)
                .SetDisplayName(Common.L("Text.Oss439.ChainFrost.Name", T("ChainFrost")))
                .SetDescription(Common.L("Text.Oss439.ChainFrost.Desc", T("ChainFrost")))
                .SetRange(AbilityRange.Close)
                .SetActionType(UnitCommand.CommandType.Standard)
                .SetAnimation(UnitAnimationActionCastSpell.CastAnimationStyle.Directional)
                .SetSpellResistance(true)
                .AllowTargeting(enemies: true)
                .SetEffectOnEnemy(AbilityEffectOnUnit.Harmful)
                .AddContextRankConfig(ContextRankConfigs.CasterLevel())
                .AddToSpellLists(6, SpellList.Wizard, SpellList.Shaman)
                .Configure();
        }

        static void SonicWave()
        {
            AbilityConfigurator.NewSpell("SonicWave", Guids.Oss439Spells.SonicWave,
                    SpellSchool.Evocation, false, SpellDescriptor.Sonic)
                .SetDisplayName(Common.L("Text.Oss439.SonicWave.Name", T("SonicWave")))
                .SetDescription(Common.L("Text.Oss439.SonicWave.Desc", T("SonicWave")))
                .SetRange(AbilityRange.Projectile)
                .SetActionType(UnitCommand.CommandType.Standard)
                .SetAnimation(UnitAnimationActionCastSpell.CastAnimationStyle.Directional)
                .SetSpellResistance(true)
                .AllowTargeting(point: true)
                .SetEffectOnEnemy(AbilityEffectOnUnit.Harmful)
                .SetLocalizedSavingThrow(SavingThrow.FortPartial)
                .AddContextRankConfig(ContextRankConfigs.CasterLevel(max: 15))
                .AddToSpellLists(5, SpellList.Wizard, SpellList.Bard)
                .Configure();
        }

        static void GlobalSilence()
        {
            AbilityConfigurator.NewSpell("GlobalSilence",
                    Guids.Oss439Spells.GlobalSilence,
                    SpellSchool.Enchantment, false,
                    SpellDescriptor.MindAffecting)
                .SetDisplayName(Common.L("Text.Oss439.GlobalSilence.Name",
                    T("GlobalSilence")))
                .SetDescription(Common.L("Text.Oss439.GlobalSilence.Desc",
                    T("GlobalSilence")))
                .SetRange(AbilityRange.Unlimited)
                .SetActionType(UnitCommand.CommandType.Standard)
                .SetAnimation(UnitAnimationActionCastSpell.CastAnimationStyle.Omni)
                .SetSpellResistance(true)
                .AllowTargeting(enemies: true)
                .SetEffectOnEnemy(AbilityEffectOnUnit.Harmful)
                .SetLocalizedSavingThrow(SavingThrow.WillNegates)
                .AddToSpellLists(7, SpellList.Wizard, SpellList.Cleric)
                .Configure();
        }
    }
}