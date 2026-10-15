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
                name: LocalizedText.Text("DistanceDamage_AggressorsEdge.Name", "Point-Blank Ruin"),
                desc: BuildDescription(
                    rangeLabel: LocalizedText.Text("Text.DistanceDamageFeats.ConfigureAll.rangeLabelEn", "Close Range"),
                    lore: LocalizedText.Text("DistanceDamage_AggressorsEdge.Lore", "You train to deliver a committed blow in the cramped space inside an opponent's guard."),
                    effect: LocalizedText.Text("Text.DistanceDamageFeats.ConfigureAll.effectEn", "Melee weapon attacks gain an untyped damage bonus of up to +4 that depends on how close you are when you hit. Measured from your centre to the target's edge, the bonus is +4 when pressed against the target and drops by 1 for every 1.5 feet. Smaller creatures can stand closer and so reach the higher values.")),
                closeQuarters: CloseQuartersMeasure.Distance);

            CreateCloseQuartersFeat(
                "ShortBlade",
                Guids.DistanceDamage.ShortBlade,
                LocalizedText.Text("Text.DistanceDamageFeats.ConfigureAll.Text", "Short-Blade Discipline"),
                LocalizedText.Text("Text.DistanceDamageFeats.ConfigureAll.Text.2", "Weapon Length"),
                LocalizedText.Text("Text.DistanceDamageFeats.ConfigureAll.Text.3", "Melee weapon attacks gain an untyped damage bonus by weapon type: +4 with light weapons, unarmed strikes and natural attacks; +3 with one-handed weapons; +2 with two-handed weapons; no bonus with reach weapons."),
                CloseQuartersMeasure.Weapon);

            CreateCloseQuartersFeat(
                "CloseQuarters",
                Guids.DistanceDamage.CloseQuarters,
                LocalizedText.Text("Text.DistanceDamageFeats.ConfigureAll.Text.4", "In-Fighting Stride"),
                LocalizedText.Text("Text.DistanceDamageFeats.ConfigureAll.Text.5", "Total Reach"),
                LocalizedText.Text("Text.DistanceDamageFeats.ConfigureAll.Text.6", "Melee weapon attacks gain an untyped damage bonus by your total reach with that weapon (your own reach plus the weapon's): +4 at 5 feet, +2 at 10 feet, and no bonus at 15 feet or more."),
                CloseQuartersMeasure.Reach);

            CreateCloseQuartersFeat(
                "LongHaft",
                Guids.DistanceDamage.LongHaft,
                LocalizedText.Text("Text.DistanceDamageFeats.ConfigureAll.Text.7", "Long-Haft Discipline"),
                LocalizedText.Text("Text.DistanceDamageFeats.ConfigureAll.Text.2", "Weapon Length"),
                LocalizedText.Text("Text.DistanceDamageFeats.ConfigureAll.Text.8", "Melee weapon attacks gain an untyped damage bonus by weapon type: +4 with reach weapons, +2 with other two-handed weapons, none otherwise."),
                CloseQuartersMeasure.ReachWeapon);

            CreateCloseQuartersFeat(
                "ReachControl",
                Guids.DistanceDamage.ReachControl,
                LocalizedText.Text("Text.DistanceDamageFeats.ConfigureAll.Text.9", "Measured Distance"),
                LocalizedText.Text("Text.DistanceDamageFeats.ConfigureAll.Text.5", "Total Reach"),
                LocalizedText.Text("Text.DistanceDamageFeats.ConfigureAll.Text.10", "Melee weapon attacks gain an untyped damage bonus by your total reach with that weapon: none at 5 feet, +2 at 10 feet, +4 at 15 feet or more."),
                CloseQuartersMeasure.LongReach);

            CreateCloseQuartersFeat(
                "StrongBow",
                Guids.DistanceDamage.StrongBow,
                LocalizedText.Text("Text.DistanceDamageFeats.ConfigureAll.Text.11", "Far-Reaching Arms"),
                LocalizedText.Text("Text.DistanceDamageFeats.ConfigureAll.Text.12", "Weapon Range"),
                LocalizedText.Text("Text.DistanceDamageFeats.ConfigureAll.Text.13", "Ranged and thrown weapon attacks gain an untyped damage bonus by the weapon's range: +4 at 50 feet or more, +3 at 40 feet, +2 at 30 feet, +1 at 20 feet."),
                CloseQuartersMeasure.RangedWeapon);

            CreateCloseQuartersFeat(
                "SteadySniper",
                Guids.DistanceDamage.SteadySniper,
                LocalizedText.Text("Text.DistanceDamageFeats.ConfigureAll.Text.14", "Unhurried Aim"),
                LocalizedText.Text("Text.DistanceDamageFeats.ConfigureAll.Text.15", "Composure"),
                LocalizedText.Text("Text.DistanceDamageFeats.ConfigureAll.Text.16", "Ranged and thrown weapon attacks gain a +4 untyped damage bonus while no enemy is engaging you in melee."),
                CloseQuartersMeasure.Unengaged);

            CreateFeat(
                internalName: "MarksmansFocus",
                featureGuid: Guids.DistanceDamage.MarksmansFocus,
                name: LocalizedText.Text("DistanceDamage_MarksmansFocus.Name", "Horizon's Deadeye"),
                desc: BuildDescription(
                    rangeLabel: LocalizedText.Text("Text.DistanceDamageFeats.ConfigureAll.rangeLabelEn.2", "Long Range"),
                    lore: LocalizedText.Text("DistanceDamage_MarksmansFocus.Lore", "Distance gives you room to read a target's line and settle the weapon before release."),
                    effect: LocalizedText.Text("Text.DistanceDamageFeats.ConfigureAll.effectEn.2", "Ranged and thrown weapon attacks gain an untyped damage bonus of +1 for every full 10 feet beyond 10 feet to the target, up to +4.")),
                closeQuarters: CloseQuartersMeasure.LongDistance);

            CreateFeat(
                internalName: "OptimalRange",
                featureGuid: Guids.DistanceDamage.OptimalRange,
                name: LocalizedText.Text("DistanceDamage_OptimalRange.Name", "Harmonic Cleave"),
                desc: BuildDescription(
                    rangeLabel: LocalizedText.Text("Text.DistanceDamageFeats.ConfigureAll.rangeLabelEn.3", "Mid Range"),
                    lore: LocalizedText.Text("DistanceDamage_OptimalRange.Lore", "You study the middle ground of an engagement, where spacing lets a weapon do its best work."),
                    effect: LocalizedText.Text("Text.DistanceDamageFeats.ConfigureAll.effectEn.3", "Melee weapon attacks gain an untyped damage bonus of up to +4 that grows with the distance to the target when you hit: +1 for every 1.5 feet between you and the target's edge, reaching +4 at the edge of a reach weapon's range.")),
                closeQuarters: CloseQuartersMeasure.MidDistance);
        }

        // No longer applied by any feat; kept registered because its GUID was published in 0.1.x.
        private static void ConfigureDamageBuff()
        {
            BuffConfigurator.New("DistanceDamageFlatBonusBuff", Guids.DistanceDamage.Buff.FlatBonus)
                .SetDisplayName(Common.L("DistanceDamage.Buff.Name", LocalizedText.Text("DistanceDamage.Buff.Name", "Distance Damage")))
                .SetDescription(Common.L(
                    "DistanceDamage.Buff.Desc",
                    LocalizedText.Text("DistanceDamage.Buff.Desc", "Distance Damage is active, granting a +4 untyped bonus to damage for the current weapon attack.")))
                .SetIconIfPresent("AggressorsEdge")
                .SetStacking(StackingType.Replace)
                .AddContextStatBonus(StatType.AdditionalDamage, SimpleValue(4), descriptor: Desc)
                .Configure();
        }

        private static BlueprintFeature CreateFeat(
            string internalName,
            string featureGuid,
            LocalizedText name,
            LocalizedText desc,
            CloseQuartersMeasure closeQuarters)
            => FeatSelection.DistanceDamage.NewFeat(internalName, featureGuid)
                .SetDisplayName(Common.L($"DistanceDamage_{internalName}.Name", name))
                .SetDescription(Common.L($"DistanceDamage_{internalName}.Desc", desc, tagEncyclopediaEntries: true))
                .SetIconIfPresent(internalName)
                .AddComponent<CloseQuartersDamage>(c => c.Measure = closeQuarters)
                .Configure();

        // New close-quarters feats stay out of CreateFeat, which lists the published feats.
        private static BlueprintFeature CreateCloseQuartersFeat(string internalName, string guid, LocalizedText name,
            LocalizedText label, LocalizedText effect, CloseQuartersMeasure measure)
        {
            var desc = FeatLore.Apply(internalName, LocalizedText.Format("Text.DistanceDamageFeats.CreateCloseQuartersFeat.Description", "<i>Distance Damage · {0}</i>\n\n<b>Effect:</b> {1}\n\n<b>Stacking:</b> Stacks with the other feats of the same distance style (up to +12 together).", label, effect));
            return FeatSelection.DistanceDamage.NewFeat(internalName, guid)
                .SetDisplayName(Common.L($"DistanceDamage_{internalName}.Name", name))
                .SetDescription(Common.L($"DistanceDamage_{internalName}.Desc", desc, tagEncyclopediaEntries: true))
                .SetIconIfPresent(internalName)
                .AddComponent<CloseQuartersDamage>(c => c.Measure = measure)
                .Configure();
        }

        private static ContextValue SimpleValue(int value)
            => new() { ValueType = ContextValueType.Simple, Value = value };

        private static LocalizedText BuildDescription(
            LocalizedText rangeLabel,
            LocalizedText lore,
            LocalizedText effect)
            => LocalizedText.Format("Text.DistanceDamageFeats.BuildDescription.Description", "<i>Distance Damage · {0}</i>\n{1}\n\n<b>Effect:</b> {2}\n\n<b>Restrictions:</b> Feats within one distance style stack; feats from different styles exclude each other.", rangeLabel, lore, effect);
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
