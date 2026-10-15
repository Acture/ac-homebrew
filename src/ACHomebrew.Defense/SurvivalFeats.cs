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

            var undying = Buff("UndyingBuff", Guids.Survival.UndyingBuff, LocalizedText.Text("Text.SurvivalFeats.ConfigureAll.Text", "Undying"),
                    LocalizedText.Text("Text.SurvivalFeats.ConfigureAll.Text.2", "You cannot die and keep acting at negative hit points. When this ends, you die if your hit points are still at or below the negative of your Constitution score."))
                .AddComponent<UndyingState>()
                .Configure();
            Feat(FeatSelection.Survival, "Undying", Guids.Survival.Undying, LocalizedText.Text("Text.SurvivalFeats.ConfigureAll.Text", "Undying"),
                    Desc(LocalizedText.Text("Text.SurvivalFeats.ConfigureAll.Text.3", "Survival"),
                        LocalizedText.Text("Text.SurvivalFeats.ConfigureAll.Text.4", "When damage reduces you to 0 hit points or fewer, you become Undying for 1 hour: you cannot die and keep acting at negative hit points. If you are not healed above the death threshold (negative Constitution score) before it ends, you die. It can trigger again after it ends.")))
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
