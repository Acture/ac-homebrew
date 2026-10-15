using System;
using BlueprintCore.Blueprints.Configurators.Classes.Selection;
using Kingmaker;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using static ACHomebrew.Feats.FeatBuilders;

namespace ACHomebrew.Feats
{
    /// <summary>Meme feats: off by default, registered only when enabled in settings.</summary>
    internal static class MemeFeats
    {
        private static bool Initialized;

        public static void ConfigureAll()
        {
            if (Initialized) return;
            Initialized = true;
            if (Mod.Settings?.EnableMemeFeats != true) return;

            Feat(FeatSelection.Meme, "WhatCanISay", Guids.Meme.WhatCanISay, LocalizedText.Text("Text.MemeFeats.ConfigureAll.Text", "Man, What Can I Say"),
                    Desc(LocalizedText.Text("Text.MemeFeats.ConfigureAll.Text.2", "Meme"),
                        LocalizedText.Text("Text.MemeFeats.ConfigureAll.Text.3", "When your weapon attack drops an enemy or scores a critical hit, you say \"Man, what can I say.\" At most once per round. No other effect.")))
                .AddComponent<MemeBarkOnKill>()
                .Configure();

            ParametrizedFeatureConfigurator.New("NobodyKnowsBetter", Guids.Meme.NobodyKnowsBetter)
                .SetGroups(Kingmaker.Blueprints.Classes.FeatureGroup.Feat)
                .SetDisplayName(Common.L("NobodyKnowsBetter.Name", LocalizedText.Text("NobodyKnowsBetter.Name", "Nobody Knows It Better")))
                .SetIconIfPresent("NobodyKnowsBetter")
                .SetDescription(Common.L("NobodyKnowsBetter.Desc",
                    Desc(LocalizedText.Text("Text.MemeFeats.ConfigureAll.Text.2", "Meme"), LocalizedText.Text("Text.MemeFeats.ConfigureAll.Text.4", "Choose a skill. You gain +2 on it, and whenever you use it you announce that nobody knows it better than you (at most once per round).")), tagEncyclopediaEntries: true))
                .SetParameterType(FeatureParameterType.Skill)
                .AddComponent<NobodyKnowsBetterSkill>()
                .OnConfigure(feat => FeatSelection.Meme.Register(feat))
                .Configure();
        }

        internal static void Bark(UnitEntityData unit, string text)
            => Game.Instance.UI.Bark(unit, text, 3f);
    }

    /// <summary>Speech bubble on kills and critical hits, once per round.</summary>
    [TypeId("a904467590ac45bda8719e6f8fb197b8")]
    public class MemeBarkOnKill : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleAttackWithWeapon>, IInitiatorRulebookSubscriber
    {
        private int LastRound = -1;

        public void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }

        public void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            var dropped = evt.Target != null && evt.Target.HPLeft <= 0;
            if (!evt.AttackRoll.IsHit || (!dropped && !evt.AttackRoll.IsCriticalConfirmed)) return;
            var round = CombatActions.CurrentRound();
            if (round == LastRound) return;
            LastRound = round;
            MemeFeats.Bark(Owner, "Man, what can I say.");
        }
    }

    /// <summary>+2 on the chosen skill and a speech bubble when it is used.</summary>
    [TypeId("51a8f0d4dcd54d30a3535b5cc9db9d1f")]
    public class NobodyKnowsBetterSkill : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleSkillCheck>, IInitiatorRulebookSubscriber
    {
        private int LastRound = -1;

        private StatType Skill => Param.StatType ?? StatType.Unknown;

        protected override void OnTurnOn()
        {
            if (Skill != StatType.Unknown) Owner.Stats.GetStat(Skill)?.AddModifier(2, Runtime, ModifierDescriptor.UntypedStackable);
        }

        protected override void OnTurnOff()
        {
            if (Skill != StatType.Unknown) Owner.Stats.GetStat(Skill)?.RemoveModifiersFrom(Runtime);
        }

        public void OnEventAboutToTrigger(RuleSkillCheck evt) { }

        public void OnEventDidTrigger(RuleSkillCheck evt)
        {
            if (evt.StatType != Skill) return;
            var round = CombatActions.CurrentRound();
            if (round == LastRound) return;
            LastRound = round;
            var skill = LocalizedTexts.Instance.Stats.GetText(Skill);
            MemeFeats.Bark(Owner, string.Format(Common.CurrentText("Meme.NobodyKnowsBetter.Bark", "Nobody knows {0} better than me."), skill));
        }
    }
}
