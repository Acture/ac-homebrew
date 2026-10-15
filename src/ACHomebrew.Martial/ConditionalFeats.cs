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
            string nameEn,
            string nameZh,
            string descKey,
            string descEn,
            string descZh,
            StatType baseStat)
        {
            return BuffConfigurator.New(internalName, guid)
                .SetDisplayName(Common.L(nameKey, nameEn, nameZh))
                .SetDescription(Common.L(descKey, Common.Format("Text.ConditionalFeats.NewBuff.Text", "{0} Attribute modifiers used for bonuses and matching penalties have a minimum of 0.", false, descEn), Common.Format("Text.ConditionalFeats.NewBuff.Text", "{0}加值及对应减值均以属性调整值最低0计算。", true, descZh)))
                .SetIconIfPresent(internalName)
                .SetStacking(StackingType.Replace)
                .AddContextRankConfig(ContextRankConfigs.StatBonus(baseStat, ModifierDescriptor.None, AbilityRankType.Default, min: 0))
                .AddRecalculateOnStatChange(stat: baseStat);
        }

        private static FeatureConfigurator NewFeature(
            string internalName,
            string guid,
            string nameKey,
            string nameEn,
            string nameZh,
            string descKey,
            (string en, string zh) desc)
        {
            return FeatSelection.Conditional.NewFeat(internalName, guid)
                .SetDisplayName(Common.L(nameKey, nameEn, nameZh))
                .SetDescription(Common.L(descKey, desc.en, desc.zh, tagEncyclopediaEntries: true))
                .SetIconIfPresent(internalName);
        }

        private static (string en, string zh) BuildConditionalDescription(
            string attrEn,
            string attrZh,
            string loreTextEn,
            string loreTextZh,
            string effectEn,
            string effectZh)
        {
            var en = Common.Format("Text.ConditionalFeats.BuildConditionalDescription.Description", "<i>Conditional · {0}</i>\n{1}\n\n<b>Effect:</b> {2} Attribute modifiers used for these bonuses and matching penalties have a minimum of 0.\n\n<b>Restrictions:</b> None. This feat is independent and stacks with other feats.", false, attrEn, loreTextEn, effectEn);
            var zh = Common.Format("Text.ConditionalFeats.BuildConditionalDescription.Description", "<i>触发 · {0}</i>\n{1}\n\n<b>效果：</b>{2}这些加值及对应减值均以属性调整值最低0计算。\n\n<b>限制：</b>无。此专长独立生效，可与其他专长正常叠加。", true, attrZh, loreTextZh, effectZh);
            return (en, zh);
        }

        private static BlueprintBuff CreateEndlessResolveBuff()
        {
            return NewBuff(
                    internalName: "EndlessResolveTriggerBuff",
                    guid: Guids.Conditional.TriggerBuff.EndlessResolve,
                    nameKey: "Conditional_EndlessResolve.Buff.Name",
                    nameEn: Common.Text("Conditional_EndlessResolve.Name", "Defiance at the Precipice"),
                    nameZh: Common.Text("Conditional_EndlessResolve.Name", "绝境砥柱", true),
                    descKey: "Conditional_EndlessResolve.Buff.Desc",
                    descEn: Common.Text("Text.ConditionalFeats.CreateEndlessResolveBuff.descEn", "Defiance at the Precipice is active, adding your Constitution modifier as an untyped bonus to AC and Fortitude saves.", false),
                    descZh: Common.Text("Text.ConditionalFeats.CreateEndlessResolveBuff.descEn", "绝境砥柱已激活，将你的体质调整值作为无类型加值附加至防御等级（AC）与强韧豁免。", true),
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
                    nameEn: Common.Text("Conditional_FirstBlood.Name", "Ambush of the Viper"),
                    nameZh: Common.Text("Conditional_FirstBlood.Name", "封喉首刃", true),
                    descKey: "Conditional_FirstBlood.Buff.Desc",
                    descEn: Common.Text("Text.ConditionalFeats.CreateFirstBloodBuff.descEn", "Ambush of the Viper is active, adding your Dexterity modifier as an untyped bonus to attack rolls and automatically confirming your critical threats.", false),
                    descZh: Common.Text("Text.ConditionalFeats.CreateFirstBloodBuff.descEn", "封喉首刃已激活，将你的敏捷调整值作为无类型加值附加至攻击检定，并自动确认重击威胁。", true),
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
                    nameEn: Common.Text("Conditional_Vendetta.Name", "Oath of Retribution"),
                    nameZh: Common.Text("Conditional_Vendetta.Name", "复仇血誓", true),
                    descKey: "Conditional_Vendetta.Buff.Desc",
                    descEn: Common.Text("Text.ConditionalFeats.CreateVendettaBuff.descEn", "Oath of Retribution is active, adding your Charisma modifier as an untyped bonus to attack rolls and damage for 3 rounds.", false),
                    descZh: Common.Text("Text.ConditionalFeats.CreateVendettaBuff.descEn", "复仇血誓已激活，将你的魅力调整值作为无类型加值附加至攻击检定与伤害检定，持续3轮。", true),
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
                    nameEn: Common.Text("Conditional_PatientHunter.Name", "Crane's Severance"),
                    nameZh: Common.Text("Conditional_PatientHunter.Name", "蓄势孤峰", true),
                    descKey: "Conditional_PatientHunter.Buff.Desc",
                    descEn: Common.Text("Text.ConditionalFeats.CreatePatientHunterBuff.descEn", "Crane's Severance is active, adding twice your Wisdom modifier as an untyped bonus to damage on your first weapon attack each combat round; it is consumed even if that attack misses.", false),
                    descZh: Common.Text("Text.ConditionalFeats.CreatePatientHunterBuff.descEn", "蓄势孤峰已激活，每个战斗轮的首次武器攻击获得感知调整值（最低0）两倍的无类型伤害加值；该次攻击即使命中失败也会消耗此增益。", true),
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
                    nameEn: Common.Text("Conditional_BerserkersLastStand.Name", "Gorum's Last Stand"),
                    nameZh: Common.Text("Conditional_BerserkersLastStand.Name", "狂神绝唱", true),
                    descKey: "Conditional_BerserkersLastStand.Buff.Desc",
                    descEn: Common.Text("Text.ConditionalFeats.CreateBerserkersLastStandBuff.descEn", "Gorum's Last Stand is active, adding your Strength modifier as an untyped bonus to attack rolls and damage while applying an equal untyped penalty to AC.", false),
                    descZh: Common.Text("Text.ConditionalFeats.CreateBerserkersLastStandBuff.descEn", "狂神绝唱已激活，将你的力量调整值作为无类型加值附加至攻击检定与伤害检定，同时防御等级（AC）承受等同于力量调整值的无类型减值。", true),
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
                    nameEn: Common.Text("Conditional_TacticalReading.Name", "Cadence Decoded"),
                    nameZh: Common.Text("Conditional_TacticalReading.Name", "阅破机宜", true),
                    descKey: "Conditional_TacticalReading.Buff.Desc",
                    descEn: Common.Text("Text.ConditionalFeats.CreateTacticalReadingBuff.descEn", "Cadence Decoded grants your Intelligence modifier (minimum 0) as an untyped attack bonus for up to 10 minutes. Combat beginning or ending removes it; attacks do not refresh an active buff.", false),
                    descZh: Common.Text("Text.ConditionalFeats.CreateTacticalReadingBuff.descEn", "阅破机宜提供等同于智力调整值（最低0）的无类型攻击加值，至多持续10分钟。战斗开始或结束时移除；增益存在时，攻击不会刷新持续时间。", true),
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
                    nameEn: Common.Text("Conditional_EndlessResolve.Name", "Defiance at the Precipice"),
                    nameZh: Common.Text("Conditional_EndlessResolve.Name", "绝境砥柱", true),
                    descKey: "Conditional_EndlessResolve.Desc",
                    desc: BuildConditionalDescription(
                        Common.Text("Text.ConditionalFeats.CreateEndlessResolve.Text", "Constitution", false),
                        Common.Text("Text.ConditionalFeats.CreateEndlessResolve.Text", "体质", true),
                        Common.Text("Conditional_EndlessResolve.Lore", "When injury narrows your choices, you answer with the stubborn patience of a cornered animal."),
                        Common.Text("Conditional_EndlessResolve.Lore", "伤势使选择愈发有限时，你以困兽般的坚忍回应危局。", true),
                        Common.Text("Text.ConditionalFeats.CreateEndlessResolve.Text.2", "While you are below half health, gain a buff that adds your Constitution modifier as an untyped bonus to AC and Fortitude saves.", false),
                        Common.Text("Text.ConditionalFeats.CreateEndlessResolve.Text.2", "当生命值低于50%时，获得一个增益状态，将你的体质调整值作为无类型加值附加至防御等级（AC）与强韧豁免。", true)))
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
                    nameEn: Common.Text("Conditional_FirstBlood.Name", "Ambush of the Viper"),
                    nameZh: Common.Text("Conditional_FirstBlood.Name", "封喉首刃", true),
                    descKey: "Conditional_FirstBlood.Desc",
                    desc: BuildConditionalDescription(
                        Common.Text("Text.ConditionalFeats.CreateFirstBlood.Text", "Dexterity", false),
                        Common.Text("Text.ConditionalFeats.CreateFirstBlood.Text", "敏捷", true),
                        Common.Text("Conditional_FirstBlood.Lore", "You practice the opening exchange until the first heartbeat of a fight feels familiar."),
                        Common.Text("Conditional_FirstBlood.Lore", "你反复磨炼交锋的起手，让战斗最初的一瞬也有迹可循。", true),
                        Common.Text("Text.ConditionalFeats.CreateFirstBlood.Text.2", "During the first round of combat, gain a 1-round buff that adds your Dexterity modifier as an untyped bonus to attack rolls and automatically confirms your critical threats.", false),
                        Common.Text("Text.ConditionalFeats.CreateFirstBlood.Text.2", "在战斗的第一轮内，获得持续1轮的增益状态，将你的敏捷调整值作为无类型加值附加至攻击检定，并自动确认重击威胁。", true)))
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
                    nameEn: Common.Text("Conditional_Vendetta.Name", "Oath of Retribution"),
                    nameZh: Common.Text("Conditional_Vendetta.Name", "复仇血誓", true),
                    descKey: "Conditional_Vendetta.Desc",
                    desc: BuildConditionalDescription(
                        Common.Text("Text.ConditionalFeats.CreateVendetta.Text", "Charisma", false),
                        Common.Text("Text.ConditionalFeats.CreateVendetta.Text", "魅力", true),
                        Common.Text("Conditional_Vendetta.Lore", "A companion's death gives grief a direction, lending fierce purpose to the strokes that follow."),
                        Common.Text("Conditional_Vendetta.Lore", "同伴的阵亡为悲痛赋予方向，让接下来的挥击承载复仇的决心。", true),
                        Common.Text("Text.ConditionalFeats.CreateVendetta.Text.2", "When an ally within 30 meters dies, gain a 3-round buff that adds your Charisma modifier as an untyped bonus to attack rolls and damage.", false),
                        Common.Text("Text.ConditionalFeats.CreateVendetta.Text.2", "当30米内的盟友阵亡时，获得持续3轮的增益状态，将你的魅力调整值作为无类型加值附加至攻击检定与伤害检定。", true)))
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
                    nameEn: Common.Text("Conditional_PatientHunter.Name", "Crane's Severance"),
                    nameZh: Common.Text("Conditional_PatientHunter.Name", "蓄势孤峰", true),
                    descKey: "Conditional_PatientHunter.Desc",
                    desc: BuildConditionalDescription(
                        Common.Text("Text.ConditionalFeats.CreatePatientHunter.Text", "Wisdom", false),
                        Common.Text("Text.ConditionalFeats.CreatePatientHunter.Text", "感知", true),
                        Common.Text("Conditional_PatientHunter.Lore", "You reserve your fullest commitment for the first clean opportunity of each exchange."),
                        Common.Text("Conditional_PatientHunter.Lore", "你将最充足的心力留给每一次交锋中最先到来的出手机会。", true),
                        Common.Text("Text.ConditionalFeats.CreatePatientHunter.Text.2", "At combat start and at the start of each round, gain twice your Wisdom modifier (minimum 0) as an untyped damage bonus for your first weapon attack. It is consumed whether that attack hits or misses.", false),
                        Common.Text("Text.ConditionalFeats.CreatePatientHunter.Text.2", "战斗开始与每轮开始时，首次武器攻击获得感知调整值（最低0）两倍的无类型伤害加值；该次攻击无论命中与否均消耗此增益。", true)))
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
                    nameEn: Common.Text("Conditional_BerserkersLastStand.Name", "Gorum's Last Stand"),
                    nameZh: Common.Text("Conditional_BerserkersLastStand.Name", "狂神绝唱", true),
                    descKey: "Conditional_BerserkersLastStand.Desc",
                    desc: BuildConditionalDescription(
                        Common.Text("Text.ConditionalFeats.CreateBerserkersLastStand.Text", "Strength", false),
                        Common.Text("Text.ConditionalFeats.CreateBerserkersLastStand.Text", "力量", true),
                        Common.Text("Conditional_BerserkersLastStand.Lore", "At the edge of defeat, you trade caution for a final display of the Iron Lord's battle fervor."),
                        Common.Text("Conditional_BerserkersLastStand.Lore", "败亡临近时，你放下谨慎，以铁甲之神般的战斗热忱作最后一搏。", true),
                        Common.Text("Text.ConditionalFeats.CreateBerserkersLastStand.Text.2", "While you are below 25% health, gain a buff that adds your Strength modifier as an untyped bonus to weapon attack rolls and damage, and applies an untyped penalty to AC equal to your Strength modifier.", false),
                        Common.Text("Text.ConditionalFeats.CreateBerserkersLastStand.Text.2", "当生命值低于25%时，获得一个增益状态，将你的力量调整值作为无类型加值附加至武器攻击检定与伤害检定，同时防御等级（AC）承受等同于力量调整值的无类型减值。", true)))
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
                    nameEn: Common.Text("Conditional_TacticalReading.Name", "Cadence Decoded"),
                    nameZh: Common.Text("Conditional_TacticalReading.Name", "阅破机宜", true),
                    descKey: "Conditional_TacticalReading.Desc",
                    desc: BuildConditionalDescription(
                        Common.Text("Text.ConditionalFeats.CreateTacticalReading.Text", "Intelligence", false),
                        Common.Text("Text.ConditionalFeats.CreateTacticalReading.Text", "智力", true),
                        Common.Text("Conditional_TacticalReading.Lore", "One exchange gives you a pattern to study, letting later attacks follow a better-informed line."),
                        Common.Text("Conditional_TacticalReading.Lore", "一次交锋便为你提供可研读的线索，让之后的攻击沿着更明晰的轨迹展开。", true),
                        Common.Text("Text.ConditionalFeats.CreateTacticalReading.Text.2", "After a weapon attack resolves, gain your Intelligence modifier (minimum 0) as an untyped attack bonus for up to 10 minutes if this buff is absent. Combat beginning or ending removes it; attacks do not refresh an active buff.", false),
                        Common.Text("Text.ConditionalFeats.CreateTacticalReading.Text.2", "武器攻击结算后，若尚无此增益，获得等同于智力调整值（最低0）的无类型攻击加值，至多持续10分钟。战斗开始或结束时移除；增益存在时，攻击不会刷新持续时间。", true)))
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
