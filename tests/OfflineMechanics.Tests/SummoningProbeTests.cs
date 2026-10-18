using System;
using System.Linq;
using WotR.Testing.Offline;
using ACHomebrew.Feats;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using Xunit;

namespace ACHomebrew.OfflineTests
{
    /// <summary>
    /// How far summoner/target logic runs offline (OSS-320, OSS-321). The summon itself uses the game's
    /// pre-made unit path of RuleSummonUnit, because spawning a unit view needs the Unity engine.
    /// Paths that reach Unity native code are skipped as [UNITY_RUNTIME_REQUIRED], never passed.
    /// </summary>
    [Collection(ACHomebrewGameCollection.Name)]
    public sealed class SummoningProbeTests
    {
        // CR2_BoarStandard: a vanilla creature used as the pre-made summon.
        private const string VanillaSummon = "5f968d63d756f994ebff0d774e88e4ab";

        private readonly ACHomebrewGame fixture;

        public SummoningProbeTests(ACHomebrewGame fixture) => this.fixture = fixture;

        [SkippableFact]
        public void BloodlineOfBeastsBuffsTheSummonFromTheSummonersStrengthOnly()
        {
            fixture.RequireGame();
            var feat = OfflineGame.Blueprint<BlueprintFeature>(Guids.Summon.Feature.BloodlineOfBeasts);
            var outerBuff = OfflineGame.Blueprint<BlueprintBuff>(Guids.Summon.OuterBuff.BloodlineOfBeasts);
            var innerBuff = OfflineGame.Blueprint<BlueprintBuff>(Guids.Summon.InnerBuff.BloodlineOfBeasts);

            var summoner = OfflineGame.CreateUnit(MainAttributeFeatTests.VanillaUnit);
            var summonerBefore = MainAttributeFeatTests.Snapshot(summoner);
            summoner.Progression.Features.AddFeature(feat);

            // OSS-321: the summoner carries the trigger buff but must not receive the summon bonus itself.
            Assert.True(summoner.Buffs.GetBuff(outerBuff) != null, "The summoner should carry the on-spawn trigger buff.");
            Assert.False(summoner.Buffs.GetBuff(innerBuff) != null, "The summoner received the summon-only bonus buff.");
            Assert.Equal(summonerBefore, MainAttributeFeatTests.Snapshot(summoner));

            var summon = OfflineGame.CreateUnit(VanillaSummon);
            var summonStrengthBefore = summon.Stats.Strength.ModifiedValue;
            try
            {
                Rulebook.Trigger(new RuleSummonUnit(summoner, summon, new Rounds(10)));
            }
            catch (Exception error) when (UnityNative.IsUnavailable(error))
            {
                fixture.Observations["summoning.unityRuntimeRequired"] = error.ToString();
                Skip.If(true, UnityNative.SkipReason("RuleSummonUnit", error));
            }

            // OSS-320: the bonus equals the summoner's Strength modifier, taken from the caster, not the summon.
            var expected = MainAttributeFeatTests.StrengthModifier(summoner);
            var buff = summon.Buffs.GetBuff(innerBuff);
            fixture.Observations["summoning"] = new
            {
                summoner = summoner.Blueprint.name,
                summonerStrength = summoner.Stats.Strength.ModifiedValue,
                expectedBonus = expected,
                summon = summon.Blueprint.name,
                summonStrength = $"{summonStrengthBefore} -> {summon.Stats.Strength.ModifiedValue}",
                innerBuffApplied = buff != null,
                innerBuffCaster = buff?.Context?.MaybeCaster?.Blueprint?.name,
                summonBuffs = summon.Buffs.Enumerable.Select(b => b.Blueprint.name).ToArray(),
            };
            Assert.NotNull(buff);
            Assert.Same(summoner, buff.Context.MaybeCaster);
            Assert.Equal(expected, summon.Stats.Strength.ModifiedValue - summonStrengthBefore);
            var modifier = Assert.Single(summon.Stats.Strength.Modifiers, m => m.Source?.Blueprint == innerBuff);
            Assert.Equal(expected, modifier.ModValue);
            Assert.False(summoner.Buffs.GetBuff(innerBuff) != null, "The summoner received the summon-only bonus buff after the summon.");
        }
    }
}
