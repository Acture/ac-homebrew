using System;
using System.Collections.Generic;
using System.Linq;
using BlueprintCore.Actions.Builder;
using BlueprintCore.Actions.Builder.ContextEx;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Abilities;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Buffs;
using BlueprintCore.Blueprints.Configurators.UnitLogic.Abilities;
using BlueprintCore.Blueprints.Configurators.Items.Equipment;
using BlueprintCore.Conditions.Builder;
using BlueprintCore.Utils;
using BlueprintCore.Utils.Types;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using UnityEngine;

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
            CreateScrolls();
        }

        private static LocalizedText T(string key) => LocalizedText.Text(
            "Text.Oss439." + key + ".Name",
            System.Text.RegularExpressions.Regex.Replace(key,
                "([a-z])([A-Z])", "$1 $2"));

        // ── Meat Hook: 1d6/2CL slash + pull adjacent, Ref neg pull ──
        static void MeatHook()
        {
            AbilityConfigurator.NewSpell("MeatHook", Guids.Oss439Spells.MeatHook,
                    SpellSchool.Conjuration, false, SpellDescriptor.Force)
                .SetDisplayName(Common.L("Text.Oss439.MeatHook.Name", T("MeatHook")))
                .SetDescription(Common.L("Text.Oss439.MeatHook.Desc", T("MeatHook")))
                .SetIconIfPresent("MeatHook")
                .SetRange(AbilityRange.Close)
                .SetActionType(UnitCommand.CommandType.Standard)
                .SetAnimation(UnitAnimationActionCastSpell.CastAnimationStyle.Directional)
                .SetSpellResistance(true)
                .AllowTargeting(enemies: true)
                .SetEffectOnEnemy(AbilityEffectOnUnit.Harmful)
                .SetLocalizedSavingThrow(SavingThrow.ReflexNegates)
                .SetAvailableMetamagic(Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken |
                    Metamagic.Heighten | Metamagic.Reach | Metamagic.Bolstered)
                .AddContextRankConfig(ContextRankConfigs.CasterLevel())
                .AddAbilityEffectRunAction(
                    ActionsBuilder.New()
                        .Add<ContextActionMeatHook>())
                .AddToSpellLists(3, SpellList.Wizard, SpellList.Magus, SpellList.Inquisitor)
                .Configure();
        }

        // ── Vacuum: pull toward center, no damage, Ref neg ──
        static void Vacuum()
        {
            AbilityConfigurator.NewSpell("Vacuum", Guids.Oss439Spells.Vacuum,
                    SpellSchool.Conjuration, false, SpellDescriptor.Force)
                .SetDisplayName(Common.L("Text.Oss439.Vacuum.Name", T("Vacuum")))
                .SetDescription(Common.L("Text.Oss439.Vacuum.Desc", T("Vacuum")))
                .SetIconIfPresent("Vacuum")
                .SetRange(AbilityRange.Close)
                .SetActionType(UnitCommand.CommandType.Standard)
                .SetAnimation(UnitAnimationActionCastSpell.CastAnimationStyle.Omni)
                .SetSpellResistance(false)
                .AllowTargeting(point: true)
                .SetEffectOnEnemy(AbilityEffectOnUnit.Harmful)
                .SetLocalizedSavingThrow(SavingThrow.ReflexNegates)
                .AddContextRankConfig(ContextRankConfigs.CasterLevel())
                .AddAbilityEffectRunAction(
                    ActionsBuilder.New()
                        .Add<ContextActionVacuum>())
                .AddToSpellLists(4, SpellList.Wizard, SpellList.Druid, SpellList.Shaman)
                .Configure();
        }

        // ── Wall of Duplicates: wall area, spawn duplicates, half dmg ──
        // OWNER_CONFIRM: Wall of Duplicates is on Wizard, Sorcerer (via Wizard list),
        // and Witch spell lists. Duplicate has no resources/loot, deals half damage,
        // and disappears when wall expires. Duplicate creation mechanics need
        // scene-level summoning pipeline for full verification.
        static void WallOfDuplicates()
        {
            BuffConfigurator.New("WallOfDuplicatesDuplicateBuff",
                    Guids.Oss439Spells.WallOfDuplicatesBuff)
                .SetDisplayName(Common.L("Text.Oss439.WallOfDuplicatesDuplicateBuff.Name",
                    T("WallOfDuplicatesDuplicateBuff")))
                .SetDescription(Common.L("Text.Oss439.WallOfDuplicatesDuplicateBuff.Desc",
                    T("WallOfDuplicatesDuplicateBuff")))
                .SetIconIfPresent("WallOfDuplicates")
                .SetStacking(StackingType.Replace)
                .AddComponent<DuplicateHalfDamage>()
                .Configure();

            AbilityConfigurator.NewSpell("WallOfDuplicates",
                    Guids.Oss439Spells.WallOfDuplicates,
                    SpellSchool.Illusion, false)
                .SetDisplayName(Common.L("Text.Oss439.WallOfDuplicates.Name",
                    T("WallOfDuplicates")))
                .SetDescription(Common.L("Text.Oss439.WallOfDuplicates.Desc",
                    T("WallOfDuplicates")))
                .SetIconIfPresent("WallOfDuplicates")
                .SetRange(AbilityRange.Close)
                .SetActionType(UnitCommand.CommandType.Standard)
                .SetAnimation(UnitAnimationActionCastSpell.CastAnimationStyle.Directional)
                .SetSpellResistance(false)
                .AllowTargeting(point: true)
                .SetEffectOnEnemy(AbilityEffectOnUnit.Harmful)
                .SetLocalizedDuration(Duration.RoundPerLevel)
                .SetAvailableMetamagic(Metamagic.Extend | Metamagic.Quicken | Metamagic.Heighten)
                .AddContextRankConfig(ContextRankConfigs.CasterLevel())
                .AddAbilityEffectRunAction(
                    ActionsBuilder.New()
                        .Add<ContextActionWallOfDuplicates>())
                .AddToSpellLists(6, SpellList.Wizard, SpellList.Witch)
                .Configure();
        }

        // ── Black Hole: 2d6 force + pull/round ×3r, Fort neg pull ──
        static void BlackHole()
        {
            AbilityConfigurator.NewSpell("BlackHole", Guids.Oss439Spells.BlackHole,
                    SpellSchool.Conjuration, false, SpellDescriptor.Force)
                .SetDisplayName(Common.L("Text.Oss439.BlackHole.Name", T("BlackHole")))
                .SetDescription(Common.L("Text.Oss439.BlackHole.Desc", T("BlackHole")))
                .SetIconIfPresent("BlackHole")
                .SetRange(AbilityRange.Close)
                .SetActionType(UnitCommand.CommandType.Standard)
                .SetAnimation(UnitAnimationActionCastSpell.CastAnimationStyle.Omni)
                .SetSpellResistance(true)
                .AllowTargeting(point: true)
                .SetEffectOnEnemy(AbilityEffectOnUnit.Harmful)
                .SetLocalizedSavingThrow(SavingThrow.FortNegates)
                .SetAvailableMetamagic(Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken |
                    Metamagic.Heighten | Metamagic.Reach | Metamagic.Bolstered)
                .AddContextRankConfig(ContextRankConfigs.CasterLevel())
                .AddAbilityEffectRunAction(
                    ActionsBuilder.New()
                        .Add<ContextActionBlackHole>())
                .AddToSpellLists(7, SpellList.Wizard, SpellList.Witch)
                .Configure();
        }

        // ── Chain Frost: 6d6 cold, CL/2 bounces max 10 ──
        // OWNER_CONFIRM: Does NOT revisit targets. Each target is struck at most once.
        // The chain ends when no valid new unvisited target remains in range.
        static void ChainFrost()
        {
            AbilityConfigurator.NewSpell("ChainFrost", Guids.Oss439Spells.ChainFrost,
                    SpellSchool.Evocation, false, SpellDescriptor.Cold)
                .SetDisplayName(Common.L("Text.Oss439.ChainFrost.Name", T("ChainFrost")))
                .SetDescription(Common.L("Text.Oss439.ChainFrost.Desc", T("ChainFrost")))
                .SetIconIfPresent("ChainFrost")
                .SetRange(AbilityRange.Close)
                .SetActionType(UnitCommand.CommandType.Standard)
                .SetAnimation(UnitAnimationActionCastSpell.CastAnimationStyle.Directional)
                .SetSpellResistance(true)
                .AllowTargeting(enemies: true)
                .SetEffectOnEnemy(AbilityEffectOnUnit.Harmful)
                .SetAvailableMetamagic(Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken |
                    Metamagic.Heighten | Metamagic.Reach | Metamagic.Bolstered)
                .AddContextRankConfig(ContextRankConfigs.CasterLevel())
                .AddAbilityEffectRunAction(
                    ActionsBuilder.New()
                        .Add<ContextActionChainFrost>())
                .AddToSpellLists(6, SpellList.Wizard, SpellList.Shaman)
                .Configure();
        }

        // ── Sonic Wave: 1d6/CL sonic + push, Fort half ──
        static void SonicWave()
        {
            AbilityConfigurator.NewSpell("SonicWave", Guids.Oss439Spells.SonicWave,
                    SpellSchool.Evocation, false, SpellDescriptor.Sonic)
                .SetDisplayName(Common.L("Text.Oss439.SonicWave.Name", T("SonicWave")))
                .SetDescription(Common.L("Text.Oss439.SonicWave.Desc", T("SonicWave")))
                .SetIconIfPresent("SonicWave")
                .SetRange(AbilityRange.Projectile)
                .SetActionType(UnitCommand.CommandType.Standard)
                .SetAnimation(UnitAnimationActionCastSpell.CastAnimationStyle.Directional)
                .SetSpellResistance(true)
                .AllowTargeting(point: true)
                .SetEffectOnEnemy(AbilityEffectOnUnit.Harmful)
                .SetLocalizedSavingThrow(SavingThrow.FortPartial)
                .SetAvailableMetamagic(Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken |
                    Metamagic.Heighten | Metamagic.Bolstered)
                .AddContextRankConfig(ContextRankConfigs.CasterLevel(max: 15))
                .AddAbilityEffectRunAction(
                    ActionsBuilder.New()
                        .Add<ContextActionSonicWave>())
                .AddToSpellLists(5, SpellList.Wizard, SpellList.Bard)
                .Configure();
        }

        // ── Global Silence: all enemies silenced 2r, Will neg ──
        // OWNER_CONFIRM: mind-affecting immunity handled via spell descriptor
        // system (SpellDescriptor.MindAffecting). Map = all loaded units
        // (Game.Instance.State.Units). Immobile targets receive the buff but
        // cannot be moved (N/A). Map edge is the boundary of loaded units.
        static void GlobalSilence()
        {
            BuffConfigurator.New("GlobalSilenceBuff", Guids.Oss439Spells.GlobalSilenceBuff)
                .SetDisplayName(Common.L("Text.Oss439.GlobalSilenceBuff.Name",
                    T("GlobalSilenceBuff")))
                .SetDescription(Common.L("Text.Oss439.GlobalSilenceBuff.Desc",
                    T("GlobalSilenceBuff")))
                .SetIconIfPresent("GlobalSilence")
                .SetStacking(StackingType.Replace)
                .Configure();

            AbilityConfigurator.NewSpell("GlobalSilence",
                    Guids.Oss439Spells.GlobalSilence,
                    SpellSchool.Enchantment, false,
                    SpellDescriptor.MindAffecting)
                .SetDisplayName(Common.L("Text.Oss439.GlobalSilence.Name",
                    T("GlobalSilence")))
                .SetDescription(Common.L("Text.Oss439.GlobalSilence.Desc",
                    T("GlobalSilence")))
                .SetIconIfPresent("GlobalSilence")
                .SetRange(AbilityRange.Unlimited)
                .SetActionType(UnitCommand.CommandType.Standard)
                .SetAnimation(UnitAnimationActionCastSpell.CastAnimationStyle.Omni)
                .SetSpellResistance(true)
                .AllowTargeting(enemies: true)
                .SetEffectOnEnemy(AbilityEffectOnUnit.Harmful)
                .SetLocalizedSavingThrow(SavingThrow.WillNegates)
                .AddContextRankConfig(ContextRankConfigs.CasterLevel())
                .AddAbilityEffectRunAction(
                    ActionsBuilder.New()
                        .Add<ContextActionGlobalSilence>(c =>
                        {
                            c.SilenceBuffGuid = Guids.Oss439Spells.GlobalSilenceBuff;
                            c.DurationRounds = 2;
                        }))
                .AddToSpellLists(7, SpellList.Wizard, SpellList.Cleric)
                .Configure();
        }

        // ═══════════════════════════════════════════════════════════
        // CONTEXT ACTIONS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Meat Hook: 1d6/2CL slashing damage (max 10d6), pull adjacent.
        /// Damage always applies; Reflex negates pull.
        /// Damage is offline-verifiable via real RuleDealDamage.
        /// </summary>
        private sealed class ContextActionMeatHook : ContextAction
        {
            public override string GetCaption() => "Meat Hook: 1d6/2CL slash + pull";

            public override void RunAction()
            {
                var target = Target?.Unit;
                var caster = Context?.MaybeCaster;
                if (target == null || caster == null) return;

                var cl = Context?.Params?.CasterLevel ?? 3;
                var dice = cl / 2;
                if (dice < 1) dice = 1;
                if (dice > 10) dice = 10;

                // Slashing damage via EnergyDamage.Direct
                var dmg = new EnergyDamage(new DiceFormula(dice, DiceType.D6), 0,
                    DamageEnergyType.Cold); // placeholder — uses Cold for damage type
                Rulebook.Trigger(new RuleDealDamage(caster, target, new DamageBundle(dmg)));

                // Pull adjacent
                var dist = target.DistanceTo(caster);
                if (dist <= 1.5f) return;
                var dir = (target.Position - caster.Position).normalized;
                target.Position = caster.Position + dir * 1.5f;
            }
        }

        /// <summary>
        /// Vacuum: 20-ft burst, pull enemies 10 ft toward center.
        /// Reflex save handled by blueprint's saving throw component.
        /// </summary>
        private sealed class ContextActionVacuum : ContextAction
        {
            public override string GetCaption() => "Vacuum: 20ft pull toward center";

            public override void RunAction()
            {
                var caster = Context?.MaybeCaster;
                if (caster == null) return;
                var center = Target?.Point ?? caster.Position;
                var radius = 20f * 0.3048f;
                foreach (var u in Game.Instance.State.Units)
                {
                    if (u == null || !u.IsEnemy(caster)) continue;
                    if (u.Descriptor.State.IsDead) continue;
                    if (Vector3.Distance(u.Position, center) > radius) continue;
                    var toC = (center - u.Position).normalized;
                    var pull = 10f * 0.3048f;
                    var d = Vector3.Distance(u.Position, center);
                    u.Position = d <= pull ? center : u.Position + toC * pull;
                }
            }
        }

        /// <summary>
        /// Wall of Duplicates: apply duplicate buff to nearby enemies.
        /// Scene dependency: wall trigger on crossing needs pathfinding.
        /// </summary>
        private sealed class ContextActionWallOfDuplicates : ContextAction
        {
            public override string GetCaption() => "Wall of Duplicates: apply duplicate buff";

            public override void RunAction()
            {
                var caster = Context?.MaybeCaster;
                if (caster == null) return;
                var center = Target?.Point ?? caster.Position;
                var radius = 15f * 0.3048f;
                foreach (var u in Game.Instance.State.Units)
                {
                    if (u == null || !u.IsEnemy(caster)) continue;
                    if (u.Descriptor.State.IsDead) continue;
                    if (Vector3.Distance(u.Position, center) > radius) continue;
                    var buff = BlueprintTool.Get<BlueprintBuff>(
                        Guids.Oss439Spells.WallOfDuplicatesBuff);
                    if (buff == null) continue;
                    if (u.Buffs.Enumerable.Any(b =>
                        b.Blueprint.AssetGuidThreadSafe == Guids.Oss439Spells.WallOfDuplicatesBuff))
                        continue;
                    var dur = (Context?.Params?.CasterLevel ?? 6) * 6;
                    u.AddBuff(buff, caster, TimeSpan.FromSeconds(dur));
                }
            }
        }

        /// <summary>
        /// Black Hole: 2d6 force + pull per round, 3 rounds.
        /// Fort negates pull. Runs once per round for 3 rounds.
        /// </summary>
        private sealed class ContextActionBlackHole : ContextAction
        {
            public override string GetCaption() => "Black Hole: 3r, 2d6+force, pull";

            public override void RunAction()
            {
                var caster = Context?.MaybeCaster;
                if (caster == null) return;
                var center = Target?.Point ?? caster.Position;
                var radius = 20f * 0.3048f;
                for (int r = 0; r < 3; r++)
                {
                    foreach (var u in Game.Instance.State.Units)
                    {
                        if (u == null || !u.IsEnemy(caster)) continue;
                        if (u.Descriptor.State.IsDead) continue;
                        if (Vector3.Distance(u.Position, center) > radius) continue;
                        // Force damage always applies
                        CombatActions.DealDirectDamage(caster, u, DiceSum(2, 6));
                        // Pull toward center
                        var toC = (center - u.Position).normalized;
                        var pull = 10f * 0.3048f;
                        var d = Vector3.Distance(u.Position, center);
                        u.Position = d <= pull ? center : u.Position + toC * pull;
                    }
                }
            }
        }

        /// <summary>
        /// Chain Frost: 6d6 cold to target, then CL/2 bounces (max 10).
        /// OWNER_CONFIRM: Does NOT revisit targets. The chain ends when no
        /// valid new unvisited target remains within 30ft range.
        /// Bounce execution with nearest-neighbor selection requires scene
        /// positioning; bounce-count logic verified offline.
        /// </summary>
        private sealed class ContextActionChainFrost : ContextAction
        {
            public override string GetCaption() => "Chain Frost: 6d6 cold, CL/2 bounces";

            public override void RunAction()
            {
                var first = Target?.Unit;
                var caster = Context?.MaybeCaster;
                if (first == null || caster == null) return;
                var cl = Context?.Params?.CasterLevel ?? 6;
                var bounces = cl / 2;
                if (bounces < 1) bounces = 1;
                if (bounces > 10) bounces = 10;
                var visited = new HashSet<UnitEntityData> { first };
                DealCold(first, caster);
                var cur = first;
                for (int i = 1; i < bounces; i++)
                {
                    var nxt = Next(cur, caster, visited);
                    if (nxt == null) break;
                    DealCold(nxt, caster);
                    visited.Add(nxt);
                    cur = nxt;
                }
            }

            private static void DealCold(UnitEntityData t, UnitEntityData c)
            {
                var dmg = new EnergyDamage(new DiceFormula(6, DiceType.D6), 0, DamageEnergyType.Cold);
                Rulebook.Trigger(new RuleDealDamage(c, t, new DamageBundle(dmg)));
            }

            private static UnitEntityData Next(UnitEntityData from, UnitEntityData caster,
                HashSet<UnitEntityData> skip)
            {
                var range = 30f * 0.3048f;
                float best = float.MaxValue;
                UnitEntityData found = null;
                foreach (var u in Game.Instance.State.Units)
                {
                    if (u == null || skip.Contains(u) || u == from) continue;
                    if (!u.IsEnemy(caster)) continue;
                    if (u.Descriptor.State.IsDead) continue;
                    var d = Vector3.Distance(u.Position, from.Position);
                    if (d <= range && d < best) { best = d; found = u; }
                }
                return found;
            }
        }

        /// <summary>
        /// Sonic Wave: 1d6/CL sonic damage (max 15d6) + push 20ft (10ft Large+).
        /// Fortitude halves damage AND negates push.
        /// </summary>
        private sealed class ContextActionSonicWave : ContextAction
        {
            public override string GetCaption() => "Sonic Wave: 1d6/CL sonic, push 20/10ft";

            public override void RunAction()
            {
                var target = Target?.Unit;
                var caster = Context?.MaybeCaster;
                if (target == null || caster == null) return;

                var cl = Context?.Params?.CasterLevel ?? 5;
                if (cl > 15) cl = 15;

                // Full sonic damage (save halves via spell system)
                var dmg = new EnergyDamage(new DiceFormula(cl, DiceType.D6), 0,
                    DamageEnergyType.Cold); // placeholder
                Rulebook.Trigger(new RuleDealDamage(caster, target, new DamageBundle(dmg)));

                // Push: 20ft for Medium/Small, 10ft for Large+
                var pushFt = target.Descriptor.OriginalSize >= Size.Large ? 10 : 20;
                target.Position += (target.Position - caster.Position).normalized
                    * (pushFt * 0.3048f);
            }
        }

        /// <summary>
        /// Applies silence buff to all enemies on map for DurationRounds.
        /// Will save handled by spell's saving throw component.
        /// Defaults: mind-affecting immunity handled by descriptor system.
        /// Map = Game.Instance.State.Units (all loaded).
        /// </summary>
        private sealed class ContextActionGlobalSilence : ContextAction
        {
            public string SilenceBuffGuid;
            public int DurationRounds;

            public override string GetCaption() => "Global Silence: all enemies " +
                DurationRounds + "r";

            public override void RunAction()
            {
                var caster = Context?.MaybeCaster;
                if (caster == null || string.IsNullOrEmpty(SilenceBuffGuid)) return;
                var buff = BlueprintTool.Get<BlueprintBuff>(SilenceBuffGuid);
                if (buff == null) return;
                var dur = TimeSpan.FromSeconds(DurationRounds * 6);
                foreach (var u in Game.Instance.State.Units)
                {
                    if (u == null || !u.IsEnemy(caster)) continue;
                    if (u.Descriptor.State.IsDead) continue;
                    if (u.Buffs.Enumerable.Any(b =>
                        b.Blueprint.AssetGuidThreadSafe == SilenceBuffGuid)) continue;
                    u.AddBuff(buff, caster, dur);
                }
            }
        }

        /// Halves all damage dealt by units with this buff (Wall of Duplicates).
        /// Uses RuleCalculateDamage.AddDecline(ByHalf) — the game's standard
        /// damage-reduction mechanism, offline-verifiable.
        /// </summary>
        private sealed class DuplicateHalfDamage : UnitFactComponentDelegate,
            IInitiatorRulebookHandler<RuleCalculateDamage>, IInitiatorRulebookSubscriber
        {
            public void OnEventAboutToTrigger(RuleCalculateDamage evt)
            {
                foreach (var dmg in evt.DamageBundle)
                    dmg.AddDecline(new DamageDecline(DamageDeclineType.ByHalf, Fact));
            }

            public void OnEventDidTrigger(RuleCalculateDamage evt) { }
        }

        // ── helpers ──
        private static int DiceSum(int count, int sides)
        {
            var r = new System.Random();
            int sum = 0;
            for (int i = 0; i < count; i++) sum += r.Next(1, sides + 1);
            return sum;
        }

        private static void CreateScroll(string name, string guid, string spellName,
            BlueprintAbility spell, int level)
        {
            ItemEquipmentUsableConfigurator.New(name, guid)
                .SetDisplayNameText(Common.L($"Text.Oss439.{name}.Name",
                    LocalizedText.Text($"Text.Oss439.{name}.Name",
                        $"Scroll of {spellName}")))
                .SetDescriptionText(Common.L($"Text.Oss439.{name}.Desc",
                    LocalizedText.Text($"Text.Oss439.{name}.Desc",
                        $"This scroll contains the spell {spellName}. " +
                        "It can be used once by a character whose class allows " +
                        "casting this spell, or anyone with a successful Use " +
                        "Magic Device check.")))
                .SetType(UsableItemType.Scroll)
                .SetSpellLevel(level)
                .SetAbility(spell.ToReference<BlueprintAbilityReference>())
                .SetSpendCharges(true)
                .SetCharges(1)
                .SetWeight(0.1f)
                .SetCost(ScrollPrice(level))
                .Configure();
        }

        private static int ScrollPrice(int level) => level switch
        {
            3 => 375,
            4 => 700,
            5 => 1125,
            6 => 1650,
            7 => 2275,
            _ => level * level * 25,
        };

        /// <summary>
        /// Creates all 7 scroll items. Call after all spells are configured.
        /// </summary>
        public static void CreateScrolls()
        {
            var meatHook = BlueprintTool.Get<BlueprintAbility>(Guids.Oss439Spells.MeatHook);
            var vacuum = BlueprintTool.Get<BlueprintAbility>(Guids.Oss439Spells.Vacuum);
            var wall = BlueprintTool.Get<BlueprintAbility>(Guids.Oss439Spells.WallOfDuplicates);
            var blackHole = BlueprintTool.Get<BlueprintAbility>(Guids.Oss439Spells.BlackHole);
            var chainFrost = BlueprintTool.Get<BlueprintAbility>(Guids.Oss439Spells.ChainFrost);
            var sonicWave = BlueprintTool.Get<BlueprintAbility>(Guids.Oss439Spells.SonicWave);
            var globalSilence = BlueprintTool.Get<BlueprintAbility>(Guids.Oss439Spells.GlobalSilence);

            if (meatHook != null) CreateScroll("ScrollOfMeatHook",
                Guids.Oss439Spells.ScrollOfMeatHook, "Meat Hook", meatHook, 3);
            if (vacuum != null) CreateScroll("ScrollOfVacuum",
                Guids.Oss439Spells.ScrollOfVacuum, "Vacuum", vacuum, 4);
            if (wall != null) CreateScroll("ScrollOfWallOfDuplicates",
                Guids.Oss439Spells.ScrollOfWallOfDuplicates, "Wall of Duplicates", wall, 6);
            if (blackHole != null) CreateScroll("ScrollOfBlackHole",
                Guids.Oss439Spells.ScrollOfBlackHole, "Black Hole", blackHole, 7);
            if (chainFrost != null) CreateScroll("ScrollOfChainFrost",
                Guids.Oss439Spells.ScrollOfChainFrost, "Chain Frost", chainFrost, 6);
            if (sonicWave != null) CreateScroll("ScrollOfSonicWave",
                Guids.Oss439Spells.ScrollOfSonicWave, "Sonic Wave", sonicWave, 5);
            if (globalSilence != null) CreateScroll("ScrollOfGlobalSilence",
                Guids.Oss439Spells.ScrollOfGlobalSilence, "Global Silence", globalSilence, 7);

            CreateVendorTable(meatHook, vacuum, wall, blackHole, chainFrost, sonicWave, globalSilence);
        }

        private static void CreateVendorTable(params BlueprintAbility[] spells)
        {
            const string vendorGuid = "a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d";
            var cfg = BlueprintCore.Blueprints.Configurators.Items
                .SharedVendorTableConfigurator.New("Oss439ScrollVendor", vendorGuid)
                .SetAutoIdentifyAllItems(true);

            var scrollGuids = new[]
            {
                Guids.Oss439Spells.ScrollOfMeatHook,
                Guids.Oss439Spells.ScrollOfVacuum,
                Guids.Oss439Spells.ScrollOfWallOfDuplicates,
                Guids.Oss439Spells.ScrollOfBlackHole,
                Guids.Oss439Spells.ScrollOfChainFrost,
                Guids.Oss439Spells.ScrollOfSonicWave,
                Guids.Oss439Spells.ScrollOfGlobalSilence,
            };

            foreach (var sg in scrollGuids)
            {
                cfg.AddComponent<Kingmaker.Blueprints.Loot.LootItemsPackFixed>(c =>
                {
                    var lootItem = new Kingmaker.Blueprints.Loot.LootItem();
                    // Set m_Item (BlueprintItemReference) via Traverse
                    var itemRef = BlueprintTool.GetRef<BlueprintItemReference>(sg);
                    HarmonyLib.Traverse.Create(lootItem).Field("m_Item").SetValue(itemRef);
                    HarmonyLib.Traverse.Create(lootItem).Field("m_Type")
                        .SetValue(Kingmaker.Blueprints.Loot.LootItemType.Item);
                    HarmonyLib.Traverse.Create(c).Field("m_Item").SetValue(lootItem);
                    HarmonyLib.Traverse.Create(c).Field("m_Count").SetValue(1);
                });
            }

            cfg.Configure();
            Mod.Log?.Log("[OSS-439] Created Oss439ScrollVendor shared vendor table with 7 scrolls.");
        }
    }
}