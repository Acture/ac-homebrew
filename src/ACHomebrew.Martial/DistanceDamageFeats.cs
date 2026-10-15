using BlueprintCore.Actions.Builder;
using BlueprintCore.Actions.Builder.ContextEx;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Buffs;
using BlueprintCore.Conditions.Builder;
using BlueprintCore.Conditions.Builder.ContextEx;
using BlueprintCore.Utils.Types;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;

namespace ACHomebrew.Feats
{
    internal static class DistanceDamageFeats
    {
        private const ModifierDescriptor Desc = ModifierDescriptor.None;
        private static bool Initialized;

        public static void ConfigureAll()
        {
            if (Initialized) return;
            Initialized = true;

            ConfigureDamageBuff();

            CreateFeat(
                internalName: "AggressorsEdge",
                featureGuid: Guids.DistanceDamage.AggressorsEdge,
                nameEn: Common.Text("DistanceDamage_AggressorsEdge.Name", "Point-Blank Ruin"),
                nameZh: Common.Text("DistanceDamage_AggressorsEdge.Name", "咫尺绝杀", true),
                desc: BuildDescription(
                    rangeLabelEn: Common.Text("Text.DistanceDamageFeats.ConfigureAll.rangeLabelEn", "Close Range", false),
                    rangeLabelZh: Common.Text("Text.DistanceDamageFeats.ConfigureAll.rangeLabelEn", "近距爆发", true),
                    loreEn: Common.Text("DistanceDamage_AggressorsEdge.Lore", "You train to deliver a committed blow in the cramped space inside an opponent's guard."),
                    loreZh: Common.Text("DistanceDamage_AggressorsEdge.Lore", "你磨炼在逼近敌人守势的狭窄空间内全力出手的技巧。", true),
                    effectEn: Common.Text("Text.DistanceDamageFeats.ConfigureAll.effectEn", "Melee weapon attacks gain an untyped damage bonus of up to +4 that depends on how close you are when you hit. Measured from your centre to the target's edge, the bonus is +4 when pressed against the target and drops by 1 for every 1.5 feet. Smaller creatures can stand closer and so reach the higher values.", false),
                    effectZh: Common.Text("Text.DistanceDamageFeats.ConfigureAll.effectEn", "近战武器攻击按命中时与目标的距离获得最高+4的无类型伤害加值：以你的中心到目标边缘计，贴身时为+4，每远1.5英尺减1。体型越小越能贴近目标，也就越容易获得较高加值。", true)),
                closeQuarters: CloseQuartersMeasure.Distance);

            CreateCloseQuartersFeat(
                "ShortBlade",
                Guids.DistanceDamage.ShortBlade,
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text", "Short-Blade Discipline", false),
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text", "短刃心诀", true),
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.2", "Weapon Length", false),
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.2", "武器长度", true),
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.3", "Melee weapon attacks gain an untyped damage bonus by weapon type: +4 with light weapons, unarmed strikes and natural attacks; +3 with one-handed weapons; +2 with two-handed weapons; no bonus with reach weapons.", false),
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.3", "近战武器攻击按武器类型获得无类型伤害加值：轻型武器、徒手打击与天生武器+4；单手武器+3；双手武器+2；长柄（触及）武器无加值。", true),
                CloseQuartersMeasure.Weapon);

            CreateCloseQuartersFeat(
                "CloseQuarters",
                Guids.DistanceDamage.CloseQuarters,
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.4", "In-Fighting Stride", false),
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.4", "欺身锁步", true),
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.5", "Total Reach", false),
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.5", "总触及", true),
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.6", "Melee weapon attacks gain an untyped damage bonus by your total reach with that weapon (your own reach plus the weapon's): +4 at 5 feet, +2 at 10 feet, and no bonus at 15 feet or more.", false),
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.6", "近战武器攻击按该武器的总触及（自身触及加武器触及）获得无类型伤害加值：5英尺+4，10英尺+2，15英尺及以上无加值。", true),
                CloseQuartersMeasure.Reach);

            CreateCloseQuartersFeat(
                "LongHaft",
                Guids.DistanceDamage.LongHaft,
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.7", "Long-Haft Discipline", false),
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.7", "长柄心诀", true),
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.2", "Weapon Length", false),
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.2", "武器长度", true),
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.8", "Melee weapon attacks gain an untyped damage bonus by weapon type: +4 with reach weapons, +2 with other two-handed weapons, none otherwise.", false),
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.8", "近战武器攻击按武器类型获得无类型伤害加值：长柄（触及）武器+4，其他双手武器+2，其余无加值。", true),
                CloseQuartersMeasure.ReachWeapon);

            CreateCloseQuartersFeat(
                "ReachControl",
                Guids.DistanceDamage.ReachControl,
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.9", "Measured Distance", false),
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.9", "掌距控势", true),
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.5", "Total Reach", false),
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.5", "总触及", true),
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.10", "Melee weapon attacks gain an untyped damage bonus by your total reach with that weapon: none at 5 feet, +2 at 10 feet, +4 at 15 feet or more.", false),
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.10", "近战武器攻击按该武器的总触及获得无类型伤害加值：5尺无加值，10尺+2，15尺及以上+4。", true),
                CloseQuartersMeasure.LongReach);

            CreateCloseQuartersFeat(
                "StrongBow",
                Guids.DistanceDamage.StrongBow,
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.11", "Far-Reaching Arms", false),
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.11", "挽弓及远", true),
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.12", "Weapon Range", false),
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.12", "武器射程", true),
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.13", "Ranged and thrown weapon attacks gain an untyped damage bonus by the weapon's range: +4 at 50 feet or more, +3 at 40 feet, +2 at 30 feet, +1 at 20 feet.", false),
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.13", "远程与投掷武器攻击按武器射程获得无类型伤害加值：50尺及以上+4，40尺+3，30尺+2，20尺+1。", true),
                CloseQuartersMeasure.RangedWeapon);

            CreateCloseQuartersFeat(
                "SteadySniper",
                Guids.DistanceDamage.SteadySniper,
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.14", "Unhurried Aim", false),
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.14", "静息稳射", true),
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.15", "Composure", false),
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.15", "从容", true),
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.16", "Ranged and thrown weapon attacks gain a +4 untyped damage bonus while no enemy is engaging you in melee.", false),
                Common.Text("Text.DistanceDamageFeats.ConfigureAll.Text.16", "没有敌人与你近战缠斗时，远程与投掷武器攻击获得+4无类型伤害加值。", true),
                CloseQuartersMeasure.Unengaged);

            CreateFeat(
                internalName: "MarksmansFocus",
                featureGuid: Guids.DistanceDamage.MarksmansFocus,
                nameEn: Common.Text("DistanceDamage_MarksmansFocus.Name", "Horizon's Deadeye"),
                nameZh: Common.Text("DistanceDamage_MarksmansFocus.Name", "苍穹神击", true),
                desc: BuildDescription(
                    rangeLabelEn: Common.Text("Text.DistanceDamageFeats.ConfigureAll.rangeLabelEn.2", "Long Range", false),
                    rangeLabelZh: Common.Text("Text.DistanceDamageFeats.ConfigureAll.rangeLabelEn.2", "远距绝杀", true),
                    loreEn: Common.Text("DistanceDamage_MarksmansFocus.Lore", "Distance gives you room to read a target's line and settle the weapon before release."),
                    loreZh: Common.Text("DistanceDamage_MarksmansFocus.Lore", "距离为你留出判断目标轨迹的余地，也让兵刃在出手前更加稳定。", true),
                    effectEn: Common.Text("Text.DistanceDamageFeats.ConfigureAll.effectEn.2", "Ranged and thrown weapon attacks gain an untyped damage bonus of +1 for every full 10 feet beyond 10 feet to the target, up to +4.", false),
                    effectZh: Common.Text("Text.DistanceDamageFeats.ConfigureAll.effectEn.2", "远程与投掷武器攻击按与目标的距离获得无类型伤害加值：超过10尺后每满10尺+1，最高+4。", true)),
                closeQuarters: CloseQuartersMeasure.LongDistance);

            CreateFeat(
                internalName: "OptimalRange",
                featureGuid: Guids.DistanceDamage.OptimalRange,
                nameEn: Common.Text("DistanceDamage_OptimalRange.Name", "Harmonic Cleave"),
                nameZh: Common.Text("DistanceDamage_OptimalRange.Name", "流光截角", true),
                desc: BuildDescription(
                    rangeLabelEn: Common.Text("Text.DistanceDamageFeats.ConfigureAll.rangeLabelEn.3", "Mid Range", false),
                    rangeLabelZh: Common.Text("Text.DistanceDamageFeats.ConfigureAll.rangeLabelEn.3", "中距定势", true),
                    loreEn: Common.Text("DistanceDamage_OptimalRange.Lore", "You study the middle ground of an engagement, where spacing lets a weapon do its best work."),
                    loreZh: Common.Text("DistanceDamage_OptimalRange.Lore", "你研究交锋的中间距离，让恰当间隔帮助兵刃发挥所长。", true),
                    effectEn: Common.Text("Text.DistanceDamageFeats.ConfigureAll.effectEn.3", "Melee weapon attacks gain an untyped damage bonus of up to +4 that grows with the distance to the target when you hit: +1 for every 1.5 feet between you and the target's edge, reaching +4 at the edge of a reach weapon's range.", false),
                    effectZh: Common.Text("Text.DistanceDamageFeats.ConfigureAll.effectEn.3", "近战武器攻击按命中时与目标的距离获得最高+4的无类型伤害加值：你与目标边缘每相隔1.5尺+1，在长柄武器触及边缘达到+4。", true)),
                closeQuarters: CloseQuartersMeasure.MidDistance);
        }

        // No longer applied by any feat; kept registered because its GUID was published in 0.1.x.
        private static void ConfigureDamageBuff()
        {
            BuffConfigurator.New("DistanceDamageFlatBonusBuff", Guids.DistanceDamage.Buff.FlatBonus)
                .SetDisplayName(Common.L("DistanceDamage.Buff.Name", Common.Text("DistanceDamage.Buff.Name", "Distance Damage", false), Common.Text("DistanceDamage.Buff.Name", "距离伤害", true)))
                .SetDescription(Common.L(
                    "DistanceDamage.Buff.Desc",
                    Common.Text("DistanceDamage.Buff.Desc", "Distance Damage is active, granting a +4 untyped bonus to damage for the current weapon attack.", false),
                    Common.Text("DistanceDamage.Buff.Desc", "距离伤害已激活，为当前武器攻击提供+4无类型伤害加值。", true)))
                .SetIconIfPresent("AggressorsEdge")
                .SetStacking(StackingType.Replace)
                .AddContextStatBonus(StatType.AdditionalDamage, SimpleValue(4), descriptor: Desc)
                .Configure();
        }

        private static BlueprintFeature CreateFeat(
            string internalName,
            string featureGuid,
            string nameEn,
            string nameZh,
            (string en, string zh) desc,
            CloseQuartersMeasure closeQuarters)
            => FeatSelection.DistanceDamage.NewFeat(internalName, featureGuid)
                .SetDisplayName(Common.L($"DistanceDamage_{internalName}.Name", nameEn, nameZh))
                .SetDescription(Common.L($"DistanceDamage_{internalName}.Desc", desc.en, desc.zh, tagEncyclopediaEntries: true))
                .SetIconIfPresent(internalName)
                .AddComponent<CloseQuartersDamage>(c => c.Measure = closeQuarters)
                .Configure();

        // New close-quarters feats stay out of CreateFeat, which lists the published feats.
        private static BlueprintFeature CreateCloseQuartersFeat(string internalName, string guid, string nameEn, string nameZh,
            string labelEn, string labelZh, string effectEn, string effectZh, CloseQuartersMeasure measure)
        {
            var en = Common.Format("Text.DistanceDamageFeats.CreateCloseQuartersFeat.Description", "<i>Distance Damage · {0}</i>\n\n<b>Effect:</b> {1}\n\n<b>Stacking:</b> Stacks with the other feats of the same distance style (up to +12 together).", false, labelEn, effectEn);
            var zh = Common.Format("Text.DistanceDamageFeats.CreateCloseQuartersFeat.Description", "<i>距离特化 · {0}</i>\n\n<b>效果：</b>{1}\n\n<b>叠加：</b>可与同一距离风格的其他专长叠加（合计最高+12）。", true, labelZh, effectZh);
            (en, zh) = FeatLore.Apply(internalName, (en, zh));
            return FeatSelection.DistanceDamage.NewFeat(internalName, guid)
                .SetDisplayName(Common.L($"DistanceDamage_{internalName}.Name", nameEn, nameZh))
                .SetDescription(Common.L($"DistanceDamage_{internalName}.Desc", en, zh, tagEncyclopediaEntries: true))
                .SetIconIfPresent(internalName)
                .AddComponent<CloseQuartersDamage>(c => c.Measure = measure)
                .Configure();
        }

        private static ContextValue SimpleValue(int value)
            => new() { ValueType = ContextValueType.Simple, Value = value };

        private static (string en, string zh) BuildDescription(
            string rangeLabelEn,
            string rangeLabelZh,
            string loreEn,
            string loreZh,
            string effectEn,
            string effectZh)
        {
            var en = Common.Format("Text.DistanceDamageFeats.BuildDescription.Description", "<i>Distance Damage · {0}</i>\n{1}\n\n<b>Effect:</b> {2}\n\n<b>Restrictions:</b> Feats within one distance style stack; feats from different styles exclude each other.", false, rangeLabelEn, loreEn, effectEn);
            var zh = Common.Format("Text.DistanceDamageFeats.BuildDescription.Description", "<i>距离特化 · {0}</i>\n{1}\n\n<b>效果：</b>{2}\n\n<b>限制：</b>同一距离风格的专长可叠加；不同风格互相排斥。", true, rangeLabelZh, loreZh, effectZh);
            return (en, zh);
        }
    }

    /// <summary>Adds one close-quarters score to melee weapon damage when the hit is resolved.</summary>
    [TypeId("23590e2e43ad4616b25dbeaea472167f")]
    public class CloseQuartersDamage : UnitFactComponentDelegate,
        IInitiatorRulebookHandler<RuleCalculateWeaponStats>, IInitiatorRulebookSubscriber
    {
        public CloseQuartersMeasure Measure;

        public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            var weapon = evt.Weapon;
            var target = evt.AttackWithWeapon?.Target;
            if (weapon == null || target == null || weapon.Blueprint.IsMelee == CloseQuartersRules.IsRanged(Measure)) return;

            var bonus = Score(evt.Initiator, weapon, target);
            if (bonus > 0) evt.AddDamageModifier(bonus, Fact);
        }

        public void OnEventDidTrigger(RuleCalculateWeaponStats evt) { }

        private int Score(UnitEntityData attacker, ItemEntityWeapon weapon, UnitEntityData target)
        {
            var blueprint = weapon.Blueprint;
            var weaponReach = CloseQuartersRules.NominalWeaponReach(blueprint.AttackRange.Value);
            switch (Measure)
            {
                case CloseQuartersMeasure.Weapon:
                    return CloseQuartersRules.WeaponScore(blueprint.IsLight, blueprint.IsTwoHanded,
                        blueprint.IsNatural, blueprint.IsUnarmed, weaponReach > 5);
                case CloseQuartersMeasure.Reach:
                    return CloseQuartersRules.ReachScore(weaponReach + Owner.Stats.ReachRange.Value);
                case CloseQuartersMeasure.MidDistance:
                    return CloseQuartersRules.MidDistanceScore(
                        (attacker.DistanceTo(target) - attacker.Corpulence - target.Corpulence) / Feet.FeetToMetersRatio);
                case CloseQuartersMeasure.ReachWeapon:
                    return CloseQuartersRules.ReachWeaponScore(blueprint.IsTwoHanded, weaponReach > 5);
                case CloseQuartersMeasure.LongReach:
                    return CloseQuartersRules.LongReachScore(weaponReach + Owner.Stats.ReachRange.Value);
                case CloseQuartersMeasure.LongDistance:
                    return CloseQuartersRules.LongDistanceScore(
                        (attacker.DistanceTo(target) - target.Corpulence) / Feet.FeetToMetersRatio);
                case CloseQuartersMeasure.RangedWeapon:
                    return CloseQuartersRules.RangedWeaponScore(blueprint.AttackRange.Value);
                case CloseQuartersMeasure.Unengaged:
                    return CloseQuartersRules.UnengagedScore(attacker.CombatState.IsEngaged);
                default:
                    var feet = (attacker.DistanceTo(target) - target.Corpulence) / Feet.FeetToMetersRatio;
                    var score = CloseQuartersRules.DistanceScore(feet);
                    // Calibration aid for game testing; distances depend on creature corpulence.
                    if (Mod.Settings?.LogDistanceCalibration == true)
                        Mod.Log?.Log($"AttributeFeats: close-quarters distance {feet:0.00} ft (attacker corpulence {attacker.Corpulence:0.00} m) -> +{score}.");
                    return score;
            }
        }
    }
}
