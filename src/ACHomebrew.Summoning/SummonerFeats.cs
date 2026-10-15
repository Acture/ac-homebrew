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
    /// <summary>Summoner: links between you and your summoned creatures.</summary>
    internal static class SummonerFeats
    {
        private static bool Initialized;

        public static void ConfigureAll()
        {
            if (Initialized) return;
            Initialized = true;

            ConfigureSummoner();
        }

        private static void ConfigureSummoner()
        {
            Feat(FeatSelection.Summoner, "SwarmCaller", Guids.Summoning.SwarmCaller, Common.Text("Text.SummonerFeats.ConfigureSummoner.Text", "Swarm Caller", false), Common.Text("Text.SummonerFeats.ConfigureSummoner.Text", "万灵应召", true),
                    Desc(Common.Text("Text.SummonerFeats.ConfigureSummoner.Text.2", "Summoner", false), Common.Text("Text.SummonerFeats.ConfigureSummoner.Text.2", "召唤师", true),
                        Common.Text("Text.SummonerFeats.ConfigureSummoner.Text.3", "Whenever you summon creatures, you summon one additional creature per 3 caster levels (minimum 1). This applies to every summoning spell or ability you use, including those from other mods.", false),
                        Common.Text("Text.SummonerFeats.ConfigureSummoner.Text.3", "你每次召唤生物时，每3施法者等级额外召唤一只（至少一只）。适用于你使用的所有召唤法术与能力，包括其他模组添加的。", true)))
                .AddComponent<ExtraSummons>()
                .Configure();

            Feat(FeatSelection.Summoner, "LingeringBond", Guids.Summoning.LingeringBond, Common.Text("Text.SummonerFeats.ConfigureSummoner.Text.4", "Lingering Bond", false), Common.Text("Text.SummonerFeats.ConfigureSummoner.Text.4", "契约长存", true),
                    Desc(Common.Text("Text.SummonerFeats.ConfigureSummoner.Text.2", "Summoner", false), Common.Text("Text.SummonerFeats.ConfigureSummoner.Text.2", "召唤师", true),
                        Common.Text("Text.SummonerFeats.ConfigureSummoner.Text.5", "Creatures you summon remain for 24 hours longer than normal.", false),
                        Common.Text("Text.SummonerFeats.ConfigureSummoner.Text.5", "你召唤的生物比正常多停留24小时。", true)))
                .AddComponent<LongerSummons>()
                .Configure();

            Feat(FeatSelection.Summoner, "SpiritLink", Guids.Dota.SpiritLink, Common.Text("Text.SummonerFeats.ConfigureSummoner.Text.6", "Summoner's Tether", false), Common.Text("Text.SummonerFeats.ConfigureSummoner.Text.6", "契魂连理", true),
                    Desc(Common.Text("Text.SummonerFeats.ConfigureSummoner.Text.2", "Summoner", false), Common.Text("Text.SummonerFeats.ConfigureSummoner.Text.2", "召唤师", true),
                        Common.Text("Text.SummonerFeats.ConfigureSummoner.Text.7", "Whenever you are healed, your summoned creatures within 30 feet heal the same amount. Whenever one of your summoned creatures is healed, you heal half that amount. Healing shared by this link is never shared again, so it does not bounce back and forth.", false),
                        Common.Text("Text.SummonerFeats.ConfigureSummoner.Text.7", "你受到治疗时，你30尺内的召唤物回复等量生命；你的召唤物受到治疗时，你回复其一半的生命。经由本链接传递的治疗不会再次传递，不会来回反弹。", true)))
                .AddComponent<SpiritLinkHealing>()
                .Configure();
        }
    }

    [TypeId("5c6d7e8f90a1b2c3d4e5f60718293a4b")]
    public class SpiritLinkHealing : UnitFactComponentDelegate, IGlobalRulebookHandler<RuleHealDamage>, IGlobalRulebookSubscriber
    {
        [ThreadStatic] private static bool Sharing;

        public void OnEventAboutToTrigger(RuleHealDamage evt) { }

        public void OnEventDidTrigger(RuleHealDamage evt)
        {
            if (Sharing || evt.Value <= 0 || evt.Target == null) return;
            Sharing = true;
            try
            {
                if (evt.Target == Owner)
                {
                    foreach (var summon in GameHelper.GetTargetsAround(Owner.Position, 30.Feet(), checkLOS: false)
                        .Where(unit => unit.Get<UnitPartSummonedMonster>()?.Summoner == Owner))
                        CombatActions.Heal(Owner, summon, evt.Value);
                }
                else if (evt.Target.Get<UnitPartSummonedMonster>()?.Summoner == Owner)
                {
                    CombatActions.Heal(Owner, Owner, evt.Value / 2);
                }
            }
            finally
            {
                Sharing = false;
            }
        }
    }

    /// <summary>Adds summoned creatures through the game's summon-count event.</summary>
    [TypeId("a469ec0c3515441790a96111e172d4e1")]
    public class ExtraSummons : UnitFactComponentDelegate, Kingmaker.PubSubSystem.ICalculateSummonUnitsCount
    {
        public void HandleCalculateSummonUnitsCount(MechanicsContext mechanicsContext, ContextDiceValue contextDiceValue, ref int count)
        {
            if (mechanicsContext?.MaybeCaster != Owner || count <= 0) return;
            count += Math.Max(1, (mechanicsContext.Params?.CasterLevel ?? 0) / 3);
        }
    }

    /// <summary>Extends the duration of creatures the owner summons.</summary>
    [TypeId("bb97efdc5c734c0cbadd5f613d2c46c0")]
    public class LongerSummons : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleSummonUnit>, IInitiatorRulebookSubscriber
    {
        private static readonly Kingmaker.Utility.Rounds OneDay = new(24 * 60 * 10);

        public void OnEventAboutToTrigger(RuleSummonUnit evt) => evt.BonusDuration += OneDay;

        public void OnEventDidTrigger(RuleSummonUnit evt) { }
    }
}
