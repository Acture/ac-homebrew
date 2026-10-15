using BlueprintCore.Actions.Builder;
using BlueprintCore.Actions.Builder.ContextEx;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Buffs;
using BlueprintCore.Conditions.Builder;
using BlueprintCore.Conditions.Builder.ContextEx;
using BlueprintCore.Utils;
using BlueprintCore.Utils.Types;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;

namespace ACHomebrew.Feats
{
    internal static class ConditionalFeats
    {
        private static bool Initialized;

        public static void ConfigureAll()
        {
            if (Initialized) return;
            Initialized = true;

            var endlessResolveBuff = CreateEndlessResolveBuff();
            var firstBloodBuff = CreateFirstBloodBuff();
            var vendettaBuff = CreateVendettaBuff();
            var patientHunterBuff = CreatePatientHunterBuff();
            var berserkersLastStandBuff = CreateBerserkersLastStandBuff();
            var tacticalReadingBuff = CreateTacticalReadingBuff();

            CreateEndlessResolve(endlessResolveBuff);
            CreateFirstBlood(firstBloodBuff);
            CreateVendetta(vendettaBuff);
            CreatePatientHunter(patientHunterBuff);
            CreateBerserkersLastStand(berserkersLastStandBuff);
            CreateTacticalReading(tacticalReadingBuff);
        }

        private static ContextValue SimpleValue(int value)
            => new() { ValueType = ContextValueType.Simple, Value = value };

        private static ActionsBuilder ApplySelfBuff(BlueprintBuff buff, int rounds)
            => ActionsBuilder.New().ApplyBuff(buff.ToReference<BlueprintBuffReference>(), ContextDuration.Fixed(rounds), toCaster: true);

        private static ActionsBuilder ApplySelfBuff(string buffGuid, int duration, DurationRate rate)
            => ActionsBuilder.New().ApplyBuff(buffGuid, ContextDuration.Fixed(duration, rate), toCaster: true);

        private static ActionsBuilder ApplySelfBuffIfMissing(string buffGuid, int duration, DurationRate rate)
            => ActionsBuilder.New().Conditional(
                conditions: ConditionsBuilder.New().CasterHasFact(buffGuid, negate: true).Build(),
                ifTrue: ApplySelfBuff(buffGuid, duration, rate).Build(),
                ifFalse: ActionsBuilder.New().Build());

        private static ActionsBuilder RemoveSelfBuff(BlueprintBuff buff)
            => ActionsBuilder.New().RemoveBuff(buff.ToReference<BlueprintBuffReference>(), toCaster: true);

        private static ActionsBuilder RemoveSelfBuff(string buffGuid)
            => ActionsBuilder.New().RemoveBuff(BlueprintTool.GetRef<BlueprintBuffReference>(buffGuid), toCaster: true);


        private static BuffConfigurator NewBuff(
            string internalName,
            string guid,
            string nameKey,
            LocalizedText name,
            string descKey,
            LocalizedText desc,
            StatType baseStat)
        {
            return BuffConfigurator.New(internalName, guid)
                .SetDisplayName(Common.L(nameKey, name))
                .SetDescription(Common.L(descKey, LocalizedText.Format("Text.ConditionalFeats.NewBuff.Text", "{0} Attribute modifiers used for bonuses and matching penalties have a minimum of 0.", desc)))
                .SetIconIfPresent(internalName)
                .SetStacking(StackingType.Replace)
                .AddContextRankConfig(ContextRankConfigs.StatBonus(baseStat, ModifierDescriptor.None, AbilityRankType.Default, min: 0))
                .AddRecalculateOnStatChange(stat: baseStat);
        }

        private static FeatureConfigurator NewFeature(
            string internalName,
            string guid,
            string nameKey,
            LocalizedText name,
            string descKey,
            LocalizedText desc)
        {
            return FeatSelection.Conditional.NewFeat(internalName, guid)
                .SetDisplayName(Common.L(nameKey, name))
                .SetDescription(Common.L(descKey, desc, tagEncyclopediaEntries: true))
                .SetIconIfPresent(internalName);
        }

        private static LocalizedText BuildConditionalDescription(
            LocalizedText attr,
            LocalizedText loreText,
            LocalizedText effect)
            => LocalizedText.Format("Text.ConditionalFeats.BuildConditionalDescription.Description", "<i>Conditional · {0}</i>\n{1}\n\n<b>Effect:</b> {2} Attribute modifiers used for these bonuses and matching penalties have a minimum of 0.\n\n<b>Restrictions:</b> None. This feat is independent and stacks with other feats.", attr, loreText, effect);

        private static BlueprintBuff CreateEndlessResolveBuff()
        {
            return NewBuff(
                    internalName: "EndlessResolveTriggerBuff",
                    guid: Guids.Conditional.TriggerBuff.EndlessResolve,
                    nameKey: "Conditional_EndlessResolve.Buff.Name",
                    name: LocalizedText.Text("Conditional_EndlessResolve.Name", "Defiance at the Precipice"),
                    descKey: "Conditional_EndlessResolve.Buff.Desc",
                    desc: LocalizedText.Text("Text.ConditionalFeats.CreateEndlessResolveBuff.descEn", "Defiance at the Precipice is active, adding your Constitution modifier as an untyped bonus to AC and Fortitude saves."),
                    baseStat: StatType.Constitution)
                .AddContextStatBonus(StatType.AC, Common.Rank(), descriptor: ModifierDescriptor.None)
                .AddContextStatBonus(StatType.SaveFortitude, Common.Rank(), descriptor: ModifierDescriptor.None)
                .Configure();
        }

        private static BlueprintBuff CreateFirstBloodBuff()
        {
            return NewBuff(
                    internalName: "FirstBloodTriggerBuff",
                    guid: Guids.Conditional.TriggerBuff.FirstBlood,
                    nameKey: "Conditional_FirstBlood.Buff.Name",
                    name: LocalizedText.Text("Conditional_FirstBlood.Name", "Ambush of the Viper"),
                    descKey: "Conditional_FirstBlood.Buff.Desc",
                    desc: LocalizedText.Text("Text.ConditionalFeats.CreateFirstBloodBuff.descEn", "Ambush of the Viper is active, adding your Dexterity modifier as an untyped bonus to attack rolls and automatically confirming your critical threats."),
                    baseStat: StatType.Dexterity)
                .AddContextStatBonus(StatType.AdditionalAttackBonus, Common.Rank(), descriptor: ModifierDescriptor.None)
                .AddInitiatorCritAutoconfirm()
                .Configure();
        }

        private static BlueprintBuff CreateVendettaBuff()
        {
            return NewBuff(
                    internalName: "VendettaTriggerBuff",
                    guid: Guids.Conditional.TriggerBuff.Vendetta,
                    nameKey: "Conditional_Vendetta.Buff.Name",
                    name: LocalizedText.Text("Conditional_Vendetta.Name", "Oath of Retribution"),
                    descKey: "Conditional_Vendetta.Buff.Desc",
                    desc: LocalizedText.Text("Text.ConditionalFeats.CreateVendettaBuff.descEn", "Oath of Retribution is active, adding your Charisma modifier as an untyped bonus to attack rolls and damage for 3 rounds."),
                    baseStat: StatType.Charisma)
                .AddContextStatBonus(StatType.AdditionalAttackBonus, Common.Rank(), descriptor: ModifierDescriptor.None)
                .AddContextStatBonus(StatType.AdditionalDamage, Common.Rank(), descriptor: ModifierDescriptor.None)
                .Configure();
        }

        private static BlueprintBuff CreatePatientHunterBuff()
        {
            return NewBuff(
                    internalName: "PatientHunterTriggerBuff",
                    guid: Guids.Conditional.TriggerBuff.PatientHunter,
                    nameKey: "Conditional_PatientHunter.Buff.Name",
                    name: LocalizedText.Text("Conditional_PatientHunter.Name", "Crane's Severance"),
                    descKey: "Conditional_PatientHunter.Buff.Desc",
                    desc: LocalizedText.Text("Text.ConditionalFeats.CreatePatientHunterBuff.descEn", "Crane's Severance is active, adding twice your Wisdom modifier as an untyped bonus to damage on your first weapon attack each combat round; it is consumed even if that attack misses."),
                    baseStat: StatType.Wisdom)
                .AddContextStatBonus(StatType.AdditionalDamage, Common.Rank(), descriptor: ModifierDescriptor.None)
                .AddContextStatBonus(StatType.AdditionalDamage, Common.Rank(), descriptor: ModifierDescriptor.None)
                .AddInitiatorAttackWithWeaponTrigger(action: RemoveSelfBuff(Guids.Conditional.TriggerBuff.PatientHunter), actionsOnInitiator: true)
                .Configure();
        }

        private static BlueprintBuff CreateBerserkersLastStandBuff()
        {
            return NewBuff(
                    internalName: "BerserkersLastStandTriggerBuff",
                    guid: Guids.Conditional2.TriggerBuff.BerserkersLastStand,
                    nameKey: "Conditional_BerserkersLastStand.Buff.Name",
                    name: LocalizedText.Text("Conditional_BerserkersLastStand.Name", "Gorum's Last Stand"),
                    descKey: "Conditional_BerserkersLastStand.Buff.Desc",
                    desc: LocalizedText.Text("Text.ConditionalFeats.CreateBerserkersLastStandBuff.descEn", "Gorum's Last Stand is active, adding your Strength modifier as an untyped bonus to attack rolls and damage while applying an equal untyped penalty to AC."),
                    baseStat: StatType.Strength)
                .AddContextStatBonus(StatType.AdditionalAttackBonus, Common.Rank(), descriptor: ModifierDescriptor.None)
                .AddContextStatBonus(StatType.AdditionalDamage, Common.Rank(), descriptor: ModifierDescriptor.None)
                .AddContextStatBonus(StatType.AC, Common.Rank(), descriptor: ModifierDescriptor.None, multiplier: -1)
                .Configure();
        }

        private static BlueprintBuff CreateTacticalReadingBuff()
        {
            return NewBuff(
                    internalName: "TacticalReadingTriggerBuff",
                    guid: Guids.Conditional2.TriggerBuff.TacticalReading,
                    nameKey: "Conditional_TacticalReading.Buff.Name",
                    name: LocalizedText.Text("Conditional_TacticalReading.Name", "Cadence Decoded"),
                    descKey: "Conditional_TacticalReading.Buff.Desc",
                    desc: LocalizedText.Text("Text.ConditionalFeats.CreateTacticalReadingBuff.descEn", "Cadence Decoded grants your Intelligence modifier (minimum 0) as an untyped attack bonus for up to 10 minutes. Combat beginning or ending removes it; attacks do not refresh an active buff."),
                    baseStat: StatType.Intelligence)
                .AddContextStatBonus(StatType.AdditionalAttackBonus, Common.Rank(), descriptor: ModifierDescriptor.None)
                .Configure();
        }

        private static BlueprintFeature CreateEndlessResolve(BlueprintBuff buff)
        {
            return NewFeature(
                    internalName: "EndlessResolve",
                    guid: Guids.Conditional.EndlessResolve,
                    nameKey: "Conditional_EndlessResolve.Name",
                    name: LocalizedText.Text("Conditional_EndlessResolve.Name", "Defiance at the Precipice"),
                    descKey: "Conditional_EndlessResolve.Desc",
                    desc: BuildConditionalDescription(
                        LocalizedText.Text("Text.ConditionalFeats.CreateEndlessResolve.Text", "Constitution"),
                        LocalizedText.Text("Conditional_EndlessResolve.Lore", "When injury narrows your choices, you answer with the stubborn patience of a cornered animal."),
                        LocalizedText.Text("Text.ConditionalFeats.CreateEndlessResolve.Text.2", "While you are below half health, gain a buff that adds your Constitution modifier as an untyped bonus to AC and Fortitude saves.")))
                .AddBuffOnHealthTickingTrigger(healthPercent: 0.5f, triggeredBuff: buff.ToReference<BlueprintBuffReference>())
                .AddRecalculateOnStatChange(stat: StatType.Constitution)
                .Configure();
        }

        private static BlueprintFeature CreateFirstBlood(BlueprintBuff buff)
        {
            return NewFeature(
                    internalName: "FirstBlood",
                    guid: Guids.Conditional.FirstBlood,
                    nameKey: "Conditional_FirstBlood.Name",
                    name: LocalizedText.Text("Conditional_FirstBlood.Name", "Ambush of the Viper"),
                    descKey: "Conditional_FirstBlood.Desc",
                    desc: BuildConditionalDescription(
                        LocalizedText.Text("Text.ConditionalFeats.CreateFirstBlood.Text", "Dexterity"),
                        LocalizedText.Text("Conditional_FirstBlood.Lore", "You practice the opening exchange until the first heartbeat of a fight feels familiar."),
                        LocalizedText.Text("Text.ConditionalFeats.CreateFirstBlood.Text.2", "During the first round of combat, gain a 1-round buff that adds your Dexterity modifier as an untyped bonus to attack rolls and automatically confirms your critical threats.")))
                .AddCombatStateTrigger(
                    combatStartActions: ApplySelfBuff(buff, 1),
                    combatEndActions: RemoveSelfBuff(buff))
                .Configure();
        }

        private static BlueprintFeature CreateVendetta(BlueprintBuff buff)
        {
            return NewFeature(
                    internalName: "Vendetta",
                    guid: Guids.Conditional.Vendetta,
                    nameKey: "Conditional_Vendetta.Name",
                    name: LocalizedText.Text("Conditional_Vendetta.Name", "Oath of Retribution"),
                    descKey: "Conditional_Vendetta.Desc",
                    desc: BuildConditionalDescription(
                        LocalizedText.Text("Text.ConditionalFeats.CreateVendetta.Text", "Charisma"),
                        LocalizedText.Text("Conditional_Vendetta.Lore", "A companion's death gives grief a direction, lending fierce purpose to the strokes that follow."),
                        LocalizedText.Text("Text.ConditionalFeats.CreateVendetta.Text.2", "When an ally within 30 meters dies, gain a 3-round buff that adds your Charisma modifier as an untyped bonus to attack rolls and damage.")))
                .AddUnitDeathTrigger(
                    actions: ApplySelfBuff(buff, 3),
                    deathTrigger: UnitDeathTrigger.DeathTrigger.OnUnitDeath,
                    faction: UnitDeathTrigger.FactionType.Ally,
                    radiusInMeters: SimpleValue(30),
                    withUnconsciousLifeState: false)
                .Configure();
        }

        private static BlueprintFeature CreatePatientHunter(BlueprintBuff buff)
        {
            return NewFeature(
                    internalName: "PatientHunter",
                    guid: Guids.Conditional.PatientHunter,
                    nameKey: "Conditional_PatientHunter.Name",
                    name: LocalizedText.Text("Conditional_PatientHunter.Name", "Crane's Severance"),
                    descKey: "Conditional_PatientHunter.Desc",
                    desc: BuildConditionalDescription(
                        LocalizedText.Text("Text.ConditionalFeats.CreatePatientHunter.Text", "Wisdom"),
                        LocalizedText.Text("Conditional_PatientHunter.Lore", "You reserve your fullest commitment for the first clean opportunity of each exchange."),
                        LocalizedText.Text("Text.ConditionalFeats.CreatePatientHunter.Text.2", "At combat start and at the start of each round, gain twice your Wisdom modifier (minimum 0) as an untyped damage bonus for your first weapon attack. It is consumed whether that attack hits or misses.")))
                .AddCombatStateTrigger(
                    combatStartActions: ApplySelfBuff(buff, 1),
                    combatEndActions: RemoveSelfBuff(buff))
                .AddNewRoundTrigger(newRoundActions: ApplySelfBuff(buff, 1))
                .Configure();
        }

        private static BlueprintFeature CreateBerserkersLastStand(BlueprintBuff buff)
        {
            return NewFeature(
                    internalName: "BerserkersLastStand",
                    guid: Guids.Conditional2.BerserkersLastStand,
                    nameKey: "Conditional_BerserkersLastStand.Name",
                    name: LocalizedText.Text("Conditional_BerserkersLastStand.Name", "Gorum's Last Stand"),
                    descKey: "Conditional_BerserkersLastStand.Desc",
                    desc: BuildConditionalDescription(
                        LocalizedText.Text("Text.ConditionalFeats.CreateBerserkersLastStand.Text", "Strength"),
                        LocalizedText.Text("Conditional_BerserkersLastStand.Lore", "At the edge of defeat, you trade caution for a final display of the Iron Lord's battle fervor."),
                        LocalizedText.Text("Text.ConditionalFeats.CreateBerserkersLastStand.Text.2", "While you are below 25% health, gain a buff that adds your Strength modifier as an untyped bonus to weapon attack rolls and damage, and applies an untyped penalty to AC equal to your Strength modifier.")))
                .AddBuffOnHealthTickingTrigger(healthPercent: 0.25f, triggeredBuff: buff.ToReference<BlueprintBuffReference>())
                .AddRecalculateOnStatChange(stat: StatType.Strength)
                .Configure();
        }

        private static BlueprintFeature CreateTacticalReading(BlueprintBuff buff)
        {
            return NewFeature(
                    internalName: "TacticalReading",
                    guid: Guids.Conditional2.TacticalReading,
                    nameKey: "Conditional_TacticalReading.Name",
                    name: LocalizedText.Text("Conditional_TacticalReading.Name", "Cadence Decoded"),
                    descKey: "Conditional_TacticalReading.Desc",
                    desc: BuildConditionalDescription(
                        LocalizedText.Text("Text.ConditionalFeats.CreateTacticalReading.Text", "Intelligence"),
                        LocalizedText.Text("Conditional_TacticalReading.Lore", "One exchange gives you a pattern to study, letting later attacks follow a better-informed line."),
                        LocalizedText.Text("Text.ConditionalFeats.CreateTacticalReading.Text.2", "After a weapon attack resolves, gain your Intelligence modifier (minimum 0) as an untyped attack bonus for up to 10 minutes if this buff is absent. Combat beginning or ending removes it; attacks do not refresh an active buff.")))
                .AddCombatStateTrigger(
                    combatStartActions: RemoveSelfBuff(buff),
                    combatEndActions: RemoveSelfBuff(buff))
                .AddInitiatorAttackWithWeaponTrigger(
                    action: ApplySelfBuffIfMissing(Guids.Conditional2.TriggerBuff.TacticalReading, 10, DurationRate.Minutes),
                    actionsOnInitiator: true,
                    triggerBeforeAttack: false)
                .Configure();
        }
    }
}
