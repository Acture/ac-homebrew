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
    /// <summary>Solo: rewards for fighting without allies nearby.</summary>
    internal static class SoloFeats
    {
        private static bool Initialized;

        public static void ConfigureAll()
        {
            if (Initialized) return;
            Initialized = true;

            ConfigureSolo();
        }

        private static void ConfigureSolo()
        {
            var buff = Buff("LoneWolfBuff", Guids.Playstyle.LoneWolfBuff, Common.Text("Text.SoloFeats.ConfigureSolo.Text", "Lone Wolf", false), Common.Text("Text.SoloFeats.ConfigureSolo.Text", "孤鸿涉远", true),
                    Common.Text("Text.SoloFeats.ConfigureSolo.Text.2", "No ally is within 30 feet: dodge bonus to AC, bonus on saving throws and damage of +1 per 4 character levels (minimum +1).", false),
                    Common.Text("Text.SoloFeats.ConfigureSolo.Text.2", "30尺内没有队友：防御等级获得闪避加值，豁免与伤害获得加值，每4角色等级+1（最低+1）。", true))
                .AddContextRankConfig(ContextRankConfigs.CharacterLevel(min: 1).WithDivStepProgression(4))
                .AddContextStatBonus(StatType.AC, Common.Rank(), descriptor: ModifierDescriptor.Dodge)
                .AddContextStatBonus(StatType.SaveFortitude, Common.Rank(), descriptor: ModifierDescriptor.UntypedStackable)
                .AddContextStatBonus(StatType.SaveReflex, Common.Rank(), descriptor: ModifierDescriptor.UntypedStackable)
                .AddContextStatBonus(StatType.SaveWill, Common.Rank(), descriptor: ModifierDescriptor.UntypedStackable)
                .AddContextStatBonus(StatType.AdditionalDamage, Common.Rank(), descriptor: ModifierDescriptor.UntypedStackable)
                .Configure();
            Feat(FeatSelection.Solo, "LoneWolf", Guids.Playstyle.LoneWolf, Common.Text("Text.SoloFeats.ConfigureSolo.Text", "Lone Wolf", false), Common.Text("Text.SoloFeats.ConfigureSolo.Text", "孤鸿涉远", true),
                    Desc(Common.Text("Text.SoloFeats.ConfigureSolo.Text.3", "Solo", false), Common.Text("Text.SoloFeats.ConfigureSolo.Text.3", "独行", true),
                        Common.Text("Text.SoloFeats.ConfigureSolo.Text.4", "While no conscious ally is within 30 feet of you, you gain a dodge bonus to AC and a bonus on saving throws and damage rolls equal to +1 per 4 character levels (minimum +1). Summoned creatures and pets count as allies. Checked each round.", false),
                        Common.Text("Text.SoloFeats.ConfigureSolo.Text.4", "30尺内没有清醒的队友时，你的防御等级获得闪避加值，豁免与伤害骰获得加值，每4角色等级+1（最低+1）。召唤物与宠物算作队友。每轮检查一次。", true)))
                .AddComponent<LoneWolfCheck>(c => c.Buff = buff.ToReference<BlueprintBuffReference>())
                .Configure();

            var solo = Buff("TrulySoloBuff", Guids.Solo.TrulySoloBuff, Common.Text("Text.SoloFeats.ConfigureSolo.Text.5", "Truly Solo", false), Common.Text("Text.SoloFeats.ConfigureSolo.Text.5", "真·独行", true),
                    Common.Text("Text.SoloFeats.ConfigureSolo.Text.6", "Per absent companion: +1 to all ability scores, dodge AC, saving throws, attack and damage.", false),
                    Common.Text("Text.SoloFeats.ConfigureSolo.Text.6", "每名不在队伍中的同伴：所有属性值、闪避AC、豁免、攻击与伤害+1。", true))
                .SetStacking(StackingType.Rank)
                .SetRanks(5)
                .AddContextRankConfig(ContextRankConfigs.BuffRank(Guids.Solo.TrulySoloBuff));
            foreach (var stat in new[] { StatType.Strength, StatType.Dexterity, StatType.Constitution, StatType.Intelligence,
                StatType.Wisdom, StatType.Charisma, StatType.SaveFortitude, StatType.SaveReflex, StatType.SaveWill,
                StatType.AdditionalAttackBonus, StatType.AdditionalDamage })
                solo.AddContextStatBonus(stat, Common.Rank(), descriptor: ModifierDescriptor.UntypedStackable);
            var soloBuff = solo.AddContextStatBonus(StatType.AC, Common.Rank(), descriptor: ModifierDescriptor.Dodge).Configure();
            Feat(FeatSelection.Solo, "TrulySolo", Guids.Solo.TrulySolo, Common.Text("Text.SoloFeats.ConfigureSolo.Text.5", "Truly Solo", false), Common.Text("Text.SoloFeats.ConfigureSolo.Text.5", "真·独行", true),
                    Desc(Common.Text("Text.SoloFeats.ConfigureSolo.Text.3", "Solo", false), Common.Text("Text.SoloFeats.ConfigureSolo.Text.3", "独行", true),
                        Common.Text("Text.SoloFeats.ConfigureSolo.Text.7", "For each companion who has joined you but is not in your party (up to 5), you gain +1 to all ability scores, dodge AC, saving throws, attack and damage. Companions who have not joined yet do not count. A setting decides whether pets in the party reduce the count.", false),
                        Common.Text("Text.SoloFeats.ConfigureSolo.Text.7", "每有一名已加入但不在队伍中的同伴（最多5名），你的所有属性值、闪避AC、豁免、攻击与伤害+1。尚未加入的同伴不计入。设置可决定队伍中的宠物是否抵扣数量。", true)))
                .AddComponent<TrulySoloCheck>(c => c.Buff = soloBuff.ToReference<BlueprintBuffReference>())
                .Configure();
        }
    }

    /// <summary>Each round, applies the lone-wolf buff while no conscious ally is within 30 feet.</summary>
    [TypeId("7c9e1a3b5d7f4c2e8a0b6d4f2e1c3a5b")]
    public class LoneWolfCheck : UnitFactComponentDelegate, ITickEachRound
    {
        public BlueprintBuffReference Buff;

        protected override void OnActivate() => OnNewRound();

        protected override void OnDeactivate() => Owner.Buffs.RemoveFact(Buff.Get());

        public void OnNewRound()
        {
            var unit = Owner;
            var alone = !GameHelper.GetTargetsAround(unit.Position, 30.Feet(), checkLOS: false)
                .Any(other => other != unit && other.IsAlly(unit) && !other.State.IsUnconscious && !other.State.IsDead);
            var buff = Buff.Get();
            var has = Owner.Buffs.GetBuff(buff) != null;
            if (alone && !has) Owner.AddBuff(buff, Context);
            else if (!alone && has) Owner.Buffs.RemoveFact(buff);
        }
    }

    /// <summary>Each round, sets the Truly Solo rank to the number of joined companions outside the party.</summary>
    [TypeId("048b15f72fe04d9b971c048214dc7ba4")]
    public class TrulySoloCheck : UnitFactComponentDelegate, ITickEachRound
    {
        public BlueprintBuffReference Buff;

        protected override void OnActivate() => OnNewRound();

        protected override void OnDeactivate() => Owner.Buffs.RemoveFact(Buff.Get());

        public void OnNewRound()
        {
            var player = Kingmaker.Game.Instance.Player;
            if (player == null) return;
            var party = player.Party;
            var absent = player.AllCharacters.Count(unit => unit != Owner && !unit.IsPet && !party.Contains(unit) && !unit.State.IsFinallyDead);
            if (Mod.Settings?.TrulySoloCountsPets == true) absent -= player.PartyAndPets.Count - party.Count;
            var rank = Math.Max(0, Math.Min(5, absent));

            var buff = Buff.Get();
            var current = Owner.Buffs.GetBuff(buff)?.Rank ?? 0;
            if (current == rank) return;
            Owner.Buffs.RemoveFact(buff);
            for (var i = 0; i < rank; i++) Owner.AddBuff(buff, Context);
        }
    }
}
