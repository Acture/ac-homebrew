using System.Collections.Generic;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Damage;

namespace ACHomebrew.OfflineTests
{
    /// <summary>Records every damage rule the game resolves while subscribed.</summary>
    public sealed class DamageRecorder : IGlobalRulebookHandler<RuleDealDamage>
    {
        public readonly List<RuleDealDamage> Rules = new();
        public void OnEventAboutToTrigger(RuleDealDamage evt) { }
        public void OnEventDidTrigger(RuleDealDamage evt) => Rules.Add(evt);
    }
}
