using BlueprintCore.Actions.Builder.ContextEx;
using BlueprintCore.Actions.Builder;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Buffs;
using BlueprintCore.Conditions.Builder.ContextEx;
using BlueprintCore.Conditions.Builder;
using BlueprintCore.Utils.Types;
using HarmonyLib;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic;
using Kingmaker.Utility;
using Kingmaker;
using System.Collections.Generic;
using System.Linq;
using System;
using static ACHomebrew.Feats.FeatBuilders;

namespace ACHomebrew.Feats
{
    /// <summary>Arcana: restoring, refunding and burning spell slots.</summary>
    internal static class ArcanaFeats
    {
        private static bool Initialized;

        public static void ConfigureAll()
        {
            if (Initialized) return;
            Initialized = true;

            ConfigureArcana();
        }

        private static void ConfigureArcana()
        {
            Feat(FeatSelection.Arcana, "SlotHarvest", Guids.Dota.SlotHarvest, LocalizedText.Text("Text.ArcanaFeats.ConfigureArcana.Text", "Spell-Reaper's Tithe"),
                    Desc(LocalizedText.Text("Text.ArcanaFeats.ConfigureArcana.Text.2", "Arcana"),
                        LocalizedText.Text("Text.ArcanaFeats.ConfigureArcana.Text.3", "Once per round, when an enemy you damaged dies, regain one spent spell slot of a level no higher than half that enemy's Hit Dice (minimum 1).")))
                .AddComponent<SlotHarvestOnKill>()
                .Configure();

            Feat(FeatSelection.Arcana, "ArcaneOrb", Guids.Dota.ArcaneOrb, LocalizedText.Text("Text.ArcanaFeats.ConfigureArcana.Text.4", "Brimming Reservoir"),
                    Desc(LocalizedText.Text("Text.ArcanaFeats.ConfigureArcana.Text.2", "Arcana"),
                        LocalizedText.Text("Text.ArcanaFeats.ConfigureArcana.Text.5", "Your cantrips deal extra damage equal to the total spell levels of your unspent spell slots divided by 5.")))
                .AddComponent<ArcaneOrbDamage>()
                .Configure();

            Feat(FeatSelection.Arcana, "EssenceFlux", Guids.Dota.EssenceFlux, LocalizedText.Text("Text.ArcanaFeats.ConfigureArcana.Text.6", "Arcane Recirculation"),
                    Desc(LocalizedText.Text("Text.ArcanaFeats.ConfigureArcana.Text.2", "Arcana"),
                        LocalizedText.Text("Text.ArcanaFeats.ConfigureArcana.Text.7", "When you cast a spell of 1st level or higher from a spellbook, there is a 15% chance the spell slot is restored.")))
                .AddComponent<EssenceFluxRefund>()
                .Configure();

            Feat(FeatSelection.Arcana, "ManaBreak", Guids.Dota.ManaBreak, LocalizedText.Text("Text.ArcanaFeats.ConfigureArcana.Text.8", "Spell-Shatter Edge"),
                    Desc(LocalizedText.Text("Text.ArcanaFeats.ConfigureArcana.Text.2", "Arcana"),
                        LocalizedText.Text("Text.ArcanaFeats.ConfigureArcana.Text.9", "When your weapon hits a spellcaster, it loses its highest unspent spell slot and takes extra damage equal to twice that slot's level. Once per round.")))
                .AddComponent<ManaBreakOnHit>()
                .Configure();

            Feat(FeatSelection.Arcana, "ManaBreakSpell", Guids.Dota.ManaBreakSpell, LocalizedText.Text("Text.ArcanaFeats.ConfigureArcana.Text.10", "Spell-Well Collapse"),
                    Desc(LocalizedText.Text("Text.ArcanaFeats.ConfigureArcana.Text.2", "Arcana"),
                        LocalizedText.Text("Text.ArcanaFeats.ConfigureArcana.Text.11", "When your spell damages a spellcaster, it loses its highest unspent spell slot and takes extra damage equal to twice that slot's level. Once per round.")))
                .AddComponent<ManaBreakOnSpell>()
                .Configure();
        }
    }

    [TypeId("c3d4e5f60718293a4b5c6d7e8f90a1b2")]
    public class SlotHarvestOnKill : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleDealDamage>, IInitiatorRulebookSubscriber
    {
        private int LastRound = -1;

        public void OnEventAboutToTrigger(RuleDealDamage evt) { }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
            var target = evt.Target;
            if (target == null || evt.Result <= 0 || !target.IsEnemy(Owner)) return;
            if (target.HPLeft > 0 || target.HPLeft + evt.Result <= 0) return;
            var round = CombatActions.CurrentRound();
            if (round == LastRound) return;
            if (SpellSlots.RestoreOne(Owner, Math.Max(1, target.Descriptor.Progression.CharacterLevel / 2))) LastRound = round;
        }
    }

    [TypeId("d4e5f60718293a4b5c6d7e8f90a1b2c3")]
    public class ArcaneOrbDamage : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleDealDamage>, IInitiatorRulebookSubscriber
    {
        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {
            var ability = evt.Reason?.Ability;
            if (ability?.Spellbook == null || ability.SpellLevel != 0) return;
            var first = evt.DamageBundle.FirstOrDefault();
            var bonus = SpellSlots.RemainingSpellLevels(Owner) / 5;
            if (first != null && bonus > 0) first.AddModifier(bonus, Fact);
        }

        public void OnEventDidTrigger(RuleDealDamage evt) { }
    }

    [TypeId("e5f60718293a4b5c6d7e8f90a1b2c3d4")]
    public class EssenceFluxRefund : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCastSpell>, IInitiatorRulebookSubscriber
    {
        public void OnEventAboutToTrigger(RuleCastSpell evt) { }

        public void OnEventDidTrigger(RuleCastSpell evt)
        {
            var spell = evt.Spell;
            if (!evt.Success || spell?.Spellbook == null || spell.SpellLevel < 1) return;
            if (UnityEngine.Random.value >= 0.15f) return;
            var book = spell.Spellbook;
            if (book.Blueprint.Spontaneous) book.RestoreSpontaneousSlots(spell.SpellLevel, 1);
            else if (spell.SpellSlot != null) spell.SpellSlot.Available = true;
        }
    }

    /// <summary>Burns one spell slot once per round and deals twice its level as damage.</summary>
    internal sealed class ManaBurner
    {
        private int LastRound = -1;

        public void TryBurn(UnitEntityData source, UnitEntityData target)
        {
            if (target == null || target == source) return;
            var round = CombatActions.CurrentRound();
            if (LastRound == round) return;
            var level = SpellSlots.BurnHighest(target);
            if (level <= 0) return;
            LastRound = round;
            CombatActions.DealDirectDamage(source, target, 2 * level);
        }
    }

    [TypeId("f60718293a4b5c6d7e8f90a1b2c3d4e5")]
    public class ManaBreakOnHit : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleAttackWithWeapon>, IInitiatorRulebookSubscriber
    {
        private readonly ManaBurner Burner = new();

        public void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }

        public void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            if (evt.AttackRoll.IsHit) Burner.TryBurn(Owner, evt.Target);
        }
    }

    [TypeId("0269a2e308aa441fa6d157db1b6debb8")]
    public class ManaBreakOnSpell : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleDealDamage>, IInitiatorRulebookSubscriber
    {
        private readonly ManaBurner Burner = new();

        public void OnEventAboutToTrigger(RuleDealDamage evt) { }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
            if (evt.Result > 0 && evt.Reason?.Ability?.Spellbook != null) Burner.TryBurn(Owner, evt.Target);
        }
    }

    /// <summary>Spell slot helpers shared by the Arcana feats.</summary>
    internal static class SpellSlots
    {
        private static readonly AccessTools.FieldRef<Spellbook, int[]> Spontaneous = AccessTools.FieldRefAccess<Spellbook, int[]>("m_SpontaneousSlots");

        public static bool RestoreOne(UnitEntityData unit, int maxLevel)
        {
            foreach (var book in unit.Descriptor.Spellbooks)
            {
                for (var level = Math.Min(maxLevel, book.MaxSpellLevel); level >= 1; level--)
                {
                    if (book.Blueprint.Spontaneous)
                    {
                        if (book.GetSpontaneousSlots(level) >= book.GetSpellsPerDay(level)) continue;
                        book.RestoreSpontaneousSlots(level, 1);
                        return true;
                    }
                    var slot = book.GetMemorizedSpellSlots(level).FirstOrDefault(s => !s.Available && s.SpellShell != null);
                    if (slot == null) continue;
                    slot.Available = true;
                    return true;
                }
            }
            return false;
        }

        /// <summary>Removes the highest unspent slot; returns its level, or 0 if none.</summary>
        public static int BurnHighest(UnitEntityData unit)
        {
            foreach (var book in unit.Descriptor.Spellbooks.OrderByDescending(b => b.MaxSpellLevel))
            {
                for (var level = book.MaxSpellLevel; level >= 1; level--)
                {
                    if (book.Blueprint.Spontaneous)
                    {
                        var slots = Spontaneous(book);
                        if (slots == null || level >= slots.Length || slots[level] <= 0) continue;
                        slots[level]--;
                        return level;
                    }
                    var slot = book.GetMemorizedSpellSlots(level).FirstOrDefault(s => s.Available && s.SpellShell != null);
                    if (slot == null) continue;
                    slot.Available = false;
                    return level;
                }
            }
            return 0;
        }

        public static int RemainingSpellLevels(UnitEntityData unit)
        {
            var total = 0;
            foreach (var book in unit.Descriptor.Spellbooks)
            {
                for (var level = 1; level <= book.MaxSpellLevel; level++)
                {
                    total += level * (book.Blueprint.Spontaneous
                        ? book.GetSpontaneousSlots(level)
                        : book.GetMemorizedSpellSlots(level).Count(s => s.Available && s.SpellShell != null));
                }
            }
            return total;
        }

    }
}
