using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Components;
using static ACHomebrew.Feats.FeatBuilders;

namespace ACHomebrew.Feats
{
    /// <summary>Survival: refusing to fall.</summary>
    internal static class SurvivalFeats
    {
        private static bool Initialized;

        public static void ConfigureAll()
        {
            if (Initialized) return;
            Initialized = true;

            var undying = Buff("UndyingBuff", Guids.Survival.UndyingBuff, Common.Text("Text.SurvivalFeats.ConfigureAll.Text", "Undying", false), Common.Text("Text.SurvivalFeats.ConfigureAll.Text", "向死而生", true),
                    Common.Text("Text.SurvivalFeats.ConfigureAll.Text.2", "You cannot die and keep acting at negative hit points. When this ends, you die if your hit points are still at or below the negative of your Constitution score.", false),
                    Common.Text("Text.SurvivalFeats.ConfigureAll.Text.2", "你不会死亡，生命值为负时仍可行动。效果结束时，若生命值仍不高于负的体质值，你将死亡。", true))
                .AddComponent<UndyingState>()
                .Configure();
            Feat(FeatSelection.Survival, "Undying", Guids.Survival.Undying, Common.Text("Text.SurvivalFeats.ConfigureAll.Text", "Undying", false), Common.Text("Text.SurvivalFeats.ConfigureAll.Text", "向死而生", true),
                    Desc(Common.Text("Text.SurvivalFeats.ConfigureAll.Text.3", "Survival", false), Common.Text("Text.SurvivalFeats.ConfigureAll.Text.3", "生存", true),
                        Common.Text("Text.SurvivalFeats.ConfigureAll.Text.4", "When damage reduces you to 0 hit points or fewer, you become Undying for 1 hour: you cannot die and keep acting at negative hit points. If you are not healed above the death threshold (negative Constitution score) before it ends, you die. It can trigger again after it ends.", false),
                        Common.Text("Text.SurvivalFeats.ConfigureAll.Text.4", "伤害使你的生命值降至0或以下时，你进入不死状态1小时：不会死亡，生命值为负时仍可行动。若结束前未被治疗到死亡阈值（负的体质值）之上，你将死亡。结束后可再次触发。", true)))
                .AddComponent<UndyingTrigger>(c => c.Buff = undying.ToReference<BlueprintBuffReference>())
                .Configure();
        }
    }

    /// <summary>Starts Undying when damage drops the owner to 0 hit points or fewer.</summary>
    [TypeId("d4a845970fbd4c05b6bec5d709af430e")]
    public class UndyingTrigger : UnitFactComponentDelegate, ITargetRulebookHandler<RuleDealDamage>, ITargetRulebookSubscriber
    {
        public BlueprintBuffReference Buff;

        public void OnEventAboutToTrigger(RuleDealDamage evt) { }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
            if (Owner.HPLeft > 0 || Owner.State.IsDead || Owner.Buffs.GetBuff(Buff.Get()) != null) return;
            Owner.AddBuff(Buff.Get(), Context, TimeSpan.FromHours(1));
        }
    }

    /// <summary>Keeps the owner alive and acting; applies the normal death check when the hour runs out.</summary>
    [TypeId("5e37af249e81424d84c96f16ee8c9d74")]
    public class UndyingState : UnitBuffComponentDelegate, ITickEachRound
    {
        protected override void OnActivate()
        {
            Owner.State.Features.Immortality.Retain();
            Owner.State.Features.Ferocity.Retain();
        }

        protected override void OnDeactivate()
        {
            Owner.State.Features.Immortality.Release();
            Owner.State.Features.Ferocity.Release();
        }

        public void OnNewRound()
        {
            // Only the expiring round decides; unloading or leaving an area never kills.
            if (Buff.TimeLeft > TimeSpan.FromSeconds(6)) return;
            if (Owner.HPLeft > -Owner.Stats.Constitution.ModifiedValue) return;
            var owner = Owner;
            Buff.Remove(); // OnDeactivate releases immortality and ferocity.
            GameHelper.KillUnit(owner);
        }
    }
}
