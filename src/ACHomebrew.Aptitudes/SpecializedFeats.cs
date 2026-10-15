using System.Collections.Generic;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Mechanics.Components;

namespace ACHomebrew.Feats
{
    /// <summary>
    /// Creates the 24 Specialized Adept feats.
    /// </summary>
    internal static class SpecializedFeats
    {
        private static readonly ModifierDescriptor Desc = ModifierDescriptor.None;
        private static readonly StatType[] NoStats = new StatType[0];

        private static readonly StatType[] ManeuverStats =
        {
            StatType.AdditionalCMB,
        };

        // Each Adept covers two or three themed targets for its attribute.
        private static readonly Dictionary<StatType, (StatType[] stats, string en, string zh)> DefensiveThemes = new()
        {
            [StatType.Strength] = (new[] { StatType.AdditionalCMD, StatType.SaveFortitude }, Common.Text("Text.SpecializedFeats.Labels.Text", "CMD and Fortitude saves", false), Common.Text("Text.SpecializedFeats.Labels.Text", "战技防御（CMD）与强韧豁免", true)),
            [StatType.Dexterity] = (new[] { StatType.AC, StatType.SaveReflex }, Common.Text("Text.SpecializedFeats.Labels.Text.2", "AC and Reflex saves", false), Common.Text("Text.SpecializedFeats.Labels.Text.2", "防御等级（AC）与反射豁免", true)),
            [StatType.Constitution] = (new[] { StatType.AC, StatType.SaveFortitude }, Common.Text("Text.SpecializedFeats.Labels.Text.3", "AC and Fortitude saves", false), Common.Text("Text.SpecializedFeats.Labels.Text.3", "防御等级（AC）与强韧豁免", true)),
            [StatType.Intelligence] = (new[] { StatType.Initiative, StatType.SaveReflex }, Common.Text("Text.SpecializedFeats.Labels.Text.4", "Initiative and Reflex saves", false), Common.Text("Text.SpecializedFeats.Labels.Text.4", "先攻与反射豁免", true)),
            [StatType.Wisdom] = (new[] { StatType.SaveWill, StatType.Initiative }, Common.Text("Text.SpecializedFeats.Labels.Text.5", "Will saves and Initiative", false), Common.Text("Text.SpecializedFeats.Labels.Text.5", "意志豁免与先攻", true)),
            [StatType.Charisma] = (new[] { StatType.SaveWill, StatType.AdditionalCMD }, Common.Text("Text.SpecializedFeats.Labels.Text.6", "Will saves and CMD", false), Common.Text("Text.SpecializedFeats.Labels.Text.6", "意志豁免与战技防御（CMD）", true)),
        };

        private static readonly Dictionary<StatType, (StatType[] stats, string en, string zh)> SkilledThemes = new()
        {
            [StatType.Strength] = (new[] { StatType.SkillAthletics, StatType.CheckIntimidate }, Common.Text("Text.SpecializedFeats.Labels.Text.7", "Athletics and Intimidate checks", false), Common.Text("Text.SpecializedFeats.Labels.Text.7", "运动与威吓检定", true)),
            [StatType.Dexterity] = (new[] { StatType.SkillMobility, StatType.SkillStealth, StatType.SkillThievery }, Common.Text("Text.SpecializedFeats.Labels.Text.8", "Mobility, Stealth and Thievery", false), Common.Text("Text.SpecializedFeats.Labels.Text.8", "灵活、潜行与巧手", true)),
            [StatType.Constitution] = (new[] { StatType.SkillAthletics, StatType.SkillPerception }, Common.Text("Text.SpecializedFeats.Labels.Text.9", "Athletics and Perception", false), Common.Text("Text.SpecializedFeats.Labels.Text.9", "运动与察觉", true)),
            [StatType.Intelligence] = (new[] { StatType.SkillKnowledgeArcana, StatType.SkillKnowledgeWorld, StatType.SkillUseMagicDevice },
                Common.Text("Text.SpecializedFeats.Labels.Text.10", "Knowledge (Arcana), Knowledge (World) and Use Magic Device", false), Common.Text("Text.SpecializedFeats.Labels.Text.10", "知识（奥秘）、知识（世界）与使用魔法装置", true)),
            [StatType.Wisdom] = (new[] { StatType.SkillPerception, StatType.SkillLoreNature, StatType.SkillLoreReligion },
                Common.Text("Text.SpecializedFeats.Labels.Text.11", "Perception, Lore (Nature) and Lore (Religion)", false), Common.Text("Text.SpecializedFeats.Labels.Text.11", "察觉、学识（自然）与学识（宗教）", true)),
            [StatType.Charisma] = (new[] { StatType.SkillPersuasion, StatType.CheckBluff, StatType.CheckDiplomacy },
                Common.Text("Text.SpecializedFeats.Labels.Text.12", "Persuasion plus Bluff and Diplomacy checks", false), Common.Text("Text.SpecializedFeats.Labels.Text.12", "说服，以及欺诈与交涉检定", true)),
        };

        private static bool Initialized;

        private enum SpecializedFamily
        {
            Defensive,
            Maneuver,
            Skilled,
            Arcane,
        }

        public static void ConfigureAll()
        {
            if (Initialized) return;
            Initialized = true;

            var defensive = new[]
            {
                CreateSpecialized(
                    SpecializedFamily.Defensive,
                    StatType.Strength,
                    NoStats,
                    Guids.Specialized.Defensive.Str,
                    "TitansStance",
                    Common.Text("Defensive_Str.Name", "Colossus Bastion"),
                    Common.Text("Defensive_Str.Name", "巨灵重障", true),
                    Common.Text("Defensive_Str.Lore", "You brace like a fortress buttress, letting practiced strength support your whole defense."),
                    Common.Text("Defensive_Str.Lore", "你如要塞的扶壁般支撑阵线，让娴熟的力量托住整个防御。", true)),
                CreateSpecialized(
                    SpecializedFamily.Defensive,
                    StatType.Dexterity,
                    NoStats,
                    Guids.Specialized.Defensive.Dex,
                    "FlowingForm",
                    Common.Text("Defensive_Dex.Name", "Wind-Dancer's Shroud"),
                    Common.Text("Defensive_Dex.Name", "风舞虚影", true),
                    Common.Text("Defensive_Dex.Lore", "You borrow the rhythm of a swaying reed, keeping your guard in motion rather than fixing it in place."),
                    Common.Text("Defensive_Dex.Lore", "你借芦苇摇曳的韵律不断调整守势，不将防守锁在一处。", true)),
                CreateSpecialized(
                    SpecializedFamily.Defensive,
                    StatType.Constitution,
                    NoStats,
                    Guids.Specialized.Defensive.Con,
                    "IronBulwark",
                    Common.Text("Defensive_Con.Name", "Inured Carapace"),
                    Common.Text("Defensive_Con.Name", "百炼金身", true),
                    Common.Text("Defensive_Con.Lore", "Old scars remind you how to endure a blow and keep your composure under strain."),
                    Common.Text("Defensive_Con.Lore", "旧伤教会你承受冲击，也教会你在压力下保持镇定。", true)),
                CreateSpecialized(
                    SpecializedFamily.Defensive,
                    StatType.Intelligence,
                    NoStats,
                    Guids.Specialized.Defensive.Int,
                    "CalculatedDefense",
                    Common.Text("Defensive_Int.Name", "Analytical Aegis"),
                    Common.Text("Defensive_Int.Name", "算律之盾", true),
                    Common.Text("Defensive_Int.Lore", "You read the angle of an approaching strike and place your guard where it will matter."),
                    Common.Text("Defensive_Int.Lore", "你读出来袭兵刃的角度，将防守放在真正需要的位置。", true)),
                CreateSpecialized(
                    SpecializedFamily.Defensive,
                    StatType.Wisdom,
                    NoStats,
                    Guids.Specialized.Defensive.Wis,
                    "StoicVigilance",
                    Common.Text("Defensive_Wis.Name", "Third Eye Vigil"),
                    Common.Text("Defensive_Wis.Name", "天目清照", true),
                    Common.Text("Defensive_Wis.Lore", "Patient attention catches small changes in a foe's bearing before they become a committed attack."),
                    Common.Text("Defensive_Wis.Lore", "耐心的观察捕捉敌人姿态的细微变化，先于其决意出手作出准备。", true)),
                CreateSpecialized(
                    SpecializedFamily.Defensive,
                    StatType.Charisma,
                    NoStats,
                    Guids.Specialized.Defensive.Cha,
                    "IndomitablePresence",
                    Common.Text("Defensive_Cha.Name", "Majesty's Reproach"),
                    Common.Text("Defensive_Cha.Name", "凛然天威", true),
                    Common.Text("Defensive_Cha.Lore", "Your unyielding bearing turns defense into a contest of resolve as much as steel."),
                    Common.Text("Defensive_Cha.Lore", "你的坚定威仪使防守既是兵刃的较量，也是意志的交锋。", true)),
            };

            var maneuver = new[]
            {
                CreateSpecialized(
                    SpecializedFamily.Maneuver,
                    StatType.Strength,
                    ManeuverStats,
                    Guids.Specialized.Maneuver.Str,
                    "CrushingGrip",
                    Common.Text("Maneuver_Str.Name", "Grip of the Behemoth"),
                    Common.Text("Maneuver_Str.Name", "比蒙扼击", true),
                    Common.Text("Maneuver_Str.Lore", "You bring the weight of a great beast to every grip, push, and struggle for position."),
                    Common.Text("Maneuver_Str.Lore", "你将巨兽般的力量投入每一次抓握、推挤与位置争夺。", true)),
                CreateSpecialized(
                    SpecializedFamily.Maneuver,
                    StatType.Dexterity,
                    ManeuverStats,
                    Guids.Specialized.Maneuver.Dex,
                    "DeftHand",
                    Common.Text("Maneuver_Dex.Name", "Fulcrum of the Viper"),
                    Common.Text("Maneuver_Dex.Name", "灵蛇巧掣", true),
                    Common.Text("Maneuver_Dex.Lore", "A small turn at the right pivot can redirect more force than a head-on struggle."),
                    Common.Text("Maneuver_Dex.Lore", "在恰当支点的一次轻转，往往比迎面角力更能改变力量的方向。", true)),
                CreateSpecialized(
                    SpecializedFamily.Maneuver,
                    StatType.Constitution,
                    ManeuverStats,
                    Guids.Specialized.Maneuver.Con,
                    "UnyieldingHold",
                    Common.Text("Maneuver_Con.Name", "Deep-Root Clinch"),
                    Common.Text("Maneuver_Con.Name", "沉洋扼锁", true),
                    Common.Text("Maneuver_Con.Lore", "Endurance lets you maintain a hold after the first contest of strength has passed."),
                    Common.Text("Maneuver_Con.Lore", "耐力让你在最初的力量较量之后，仍能维持稳固的控制。", true)),
                CreateSpecialized(
                    SpecializedFamily.Maneuver,
                    StatType.Intelligence,
                    ManeuverStats,
                    Guids.Specialized.Maneuver.Int,
                    "TacticalBind",
                    Common.Text("Maneuver_Int.Name", "Anatomical Pivot"),
                    Common.Text("Maneuver_Int.Name", "机理断节", true),
                    Common.Text("Maneuver_Int.Lore", "You study how joints move together and use that knowledge to guide a maneuver."),
                    Common.Text("Maneuver_Int.Lore", "你研究关节如何协同活动，并以这份知识引导战技。", true)),
                CreateSpecialized(
                    SpecializedFamily.Maneuver,
                    StatType.Wisdom,
                    ManeuverStats,
                    Guids.Specialized.Maneuver.Wis,
                    "PredictiveLock",
                    Common.Text("Maneuver_Wis.Name", "Crane's Anticipation"),
                    Common.Text("Maneuver_Wis.Name", "玄鹤听劲", true),
                    Common.Text("Maneuver_Wis.Lore", "You feel an opponent's balance through contact, answering their movement with your own."),
                    Common.Text("Maneuver_Wis.Lore", "你从接触中感知对手的重心，以自己的动作回应其变化。", true)),
                CreateSpecialized(
                    SpecializedFamily.Maneuver,
                    StatType.Charisma,
                    ManeuverStats,
                    Guids.Specialized.Maneuver.Cha,
                    "DomineeringThrow",
                    Common.Text("Maneuver_Cha.Name", "Audacious Overthrow"),
                    Common.Text("Maneuver_Cha.Name", "叱喝倾山", true),
                    Common.Text("Maneuver_Cha.Lore", "An assertive step and a forceful challenge lend conviction to your attempt to unseat a foe."),
                    Common.Text("Maneuver_Cha.Lore", "果断的步伐与强势的挑战，让你动摇敌人站位的行动更有决心。", true)),
            };

            var skilled = new[]
            {
                CreateSpecialized(
                    SpecializedFamily.Skilled,
                    StatType.Strength,
                    NoStats,
                    Guids.Specialized.Skilled.Str,
                    "PracticedHand",
                    Common.Text("Skilled_Str.Name", "Giantwright's Craft"),
                    Common.Text("Skilled_Str.Name", "巨匠巧工", true),
                    Common.Text("Skilled_Str.Lore", "Work that tires others teaches you the discipline of applying strength with care."),
                    Common.Text("Skilled_Str.Lore", "让他人疲惫的劳作，教会你谨慎运用力量的技艺。", true)),
                CreateSpecialized(
                    SpecializedFamily.Skilled,
                    StatType.Dexterity,
                    NoStats,
                    Guids.Specialized.Skilled.Dex,
                    "EffortlessSkill",
                    Common.Text("Skilled_Dex.Name", "Thief-King's Panache"),
                    Common.Text("Skilled_Dex.Name", "妙手绝尘", true),
                    Common.Text("Skilled_Dex.Lore", "Your hands learn each task as a rhythm, returning to its fine motions with a performer's ease."),
                    Common.Text("Skilled_Dex.Lore", "你的双手将工作记作节奏，细小动作也能如表演般自然重现。", true)),
                CreateSpecialized(
                    SpecializedFamily.Skilled,
                    StatType.Constitution,
                    NoStats,
                    Guids.Specialized.Skilled.Con,
                    "TirelessPractice",
                    Common.Text("Skilled_Con.Name", "Ascetic Diligence"),
                    Common.Text("Skilled_Con.Name", "苦行研磨", true),
                    Common.Text("Skilled_Con.Lore", "Where inspiration fades, patient repetition keeps your craft moving forward."),
                    Common.Text("Skilled_Con.Lore", "灵感消退之处，耐心的反复练习仍推动你的技艺前行。", true)),
                CreateSpecialized(
                    SpecializedFamily.Skilled,
                    StatType.Intelligence,
                    NoStats,
                    Guids.Specialized.Skilled.Int,
                    "PolymathsTouch",
                    Common.Text("Skilled_Int.Name", "Encyclopedic Synthesis"),
                    Common.Text("Skilled_Int.Name", "格物万象", true),
                    Common.Text("Skilled_Int.Lore", "You connect unfamiliar problems to knowledge already gathered, building bridges between disciplines."),
                    Common.Text("Skilled_Int.Lore", "你将陌生问题与已有知识相连，在不同学问之间架起桥梁。", true)),
                CreateSpecialized(
                    SpecializedFamily.Skilled,
                    StatType.Wisdom,
                    NoStats,
                    Guids.Specialized.Skilled.Wis,
                    "QuietMastery",
                    Common.Text("Skilled_Wis.Name", "Wanderer's Lucidity"),
                    Common.Text("Skilled_Wis.Name", "云水澄明", true),
                    Common.Text("Skilled_Wis.Lore", "Travel and observation have taught you to hear what a task requires before reaching for a tool."),
                    Common.Text("Skilled_Wis.Lore", "行旅与观察教会你先听懂事情的需要，再伸手取用工具。", true)),
                CreateSpecialized(
                    SpecializedFamily.Skilled,
                    StatType.Charisma,
                    NoStats,
                    Guids.Specialized.Skilled.Cha,
                    "InspiredVersatility",
                    Common.Text("Skilled_Cha.Name", "Silver-Tongued Virtuoso"),
                    Common.Text("Skilled_Cha.Name", "锦绣天潢", true),
                    Common.Text("Skilled_Cha.Lore", "Confidence carries your performance through unfamiliar work, inviting others to believe in your command."),
                    Common.Text("Skilled_Cha.Lore", "自信让你在陌生事务中仍保持从容，也让旁人愿意相信你的掌握。", true)),
            };

            var arcane = new[]
            {
                CreateSpecialized(
                    SpecializedFamily.Arcane,
                    StatType.Strength,
                    NoStats,
                    Guids.Specialized.Arcane.Str,
                    "SpellForgedWill",
                    Common.Text("Arcane_Str.Name", "Mage-Hammer Inscription"),
                    Common.Text("Arcane_Str.Name", "铁骨铸咒", true),
                    Common.Text("Arcane_Str.Lore", "You approach an incantation as a smith approaches iron, shaping it with disciplined exertion."),
                    Common.Text("Arcane_Str.Lore", "你如铁匠对待生铁般对待咒语，以严整的发力塑成法术。", true)),
                CreateSpecialized(
                    SpecializedFamily.Arcane,
                    StatType.Dexterity,
                    NoStats,
                    Guids.Specialized.Arcane.Dex,
                    "QuickcastReflex",
                    Common.Text("Arcane_Dex.Name", "Somatic Velocity"),
                    Common.Text("Arcane_Dex.Name", "疾影手印", true),
                    Common.Text("Arcane_Dex.Lore", "Exact, practiced gestures give your spellwork the cadence of a deft duelist."),
                    Common.Text("Arcane_Dex.Lore", "精准而娴熟的手势，使你的施法带上灵巧剑客的节奏。", true)),
                CreateSpecialized(
                    SpecializedFamily.Arcane,
                    StatType.Constitution,
                    NoStats,
                    Guids.Specialized.Arcane.Con,
                    "SpellTemperedBody",
                    Common.Text("Arcane_Con.Name", "Crucible of the Conduit"),
                    Common.Text("Arcane_Con.Name", "鼎炉承法", true),
                    Common.Text("Arcane_Con.Lore", "You make bodily endurance part of your magical practice, learning to bear the effort of channeling power."),
                    Common.Text("Arcane_Con.Lore", "你将身体的耐力纳入魔法修习，学会承受引导力量的消耗。", true)),
                CreateSpecialized(
                    SpecializedFamily.Arcane,
                    StatType.Intelligence,
                    NoStats,
                    Guids.Specialized.Arcane.Int,
                    "ScholarOfTheWeave",
                    Common.Text("Arcane_Int.Name", "Archmage's Codex"),
                    Common.Text("Arcane_Int.Name", "万法源流", true),
                    Common.Text("Arcane_Int.Lore", "Every spell becomes a proposition to study, refine, and set beside the work of earlier arcanists."),
                    Common.Text("Arcane_Int.Lore", "每一道法术都是可研读与改进的命题，与前人的奥术成果相互印证。", true)),
                CreateSpecialized(
                    SpecializedFamily.Arcane,
                    StatType.Wisdom,
                    NoStats,
                    Guids.Specialized.Arcane.Wis,
                    "OraclesIntuition",
                    Common.Text("Arcane_Wis.Name", "Gnostic Channel"),
                    Common.Text("Arcane_Wis.Name", "玄鉴通幽", true),
                    Common.Text("Arcane_Wis.Lore", "You listen for the cadence beneath an incantation and let attentive instinct guide its expression."),
                    Common.Text("Arcane_Wis.Lore", "你聆听咒语深处的节律，以专注的直觉引导它的表达。", true)),
                CreateSpecialized(
                    SpecializedFamily.Arcane,
                    StatType.Charisma,
                    NoStats,
                    Guids.Specialized.Arcane.Cha,
                    "SorcerousPresence",
                    Common.Text("Arcane_Cha.Name", "Sovereign Decrees"),
                    Common.Text("Arcane_Cha.Name", "天宪法旨", true),
                    Common.Text("Arcane_Cha.Lore", "You speak an incantation with the confidence of a decree, giving its form the weight of conviction."),
                    Common.Text("Arcane_Cha.Lore", "你以宣告法旨般的自信吟诵咒语，让信念为法术的形式添上分量。", true)),
            };

        }

        private static BlueprintFeature CreateSpecialized(
            SpecializedFamily family,
            StatType baseStat,
            IReadOnlyList<StatType> stats,
            string guid,
            string internalName,
            string flavorNameEn,
            string flavorNameZh,
            string loreTextEn,
            string loreTextZh)
        {
            var settings = Mod.Settings ?? new ModSettings();
            var attributeKey = GetAttributeKey(baseStat);
            var attributeNameEn = GetAttributeName(baseStat);
            var attributeNameZh = GetAttributeNameZh(baseStat);
            var familyKey = GetFamilyKey(family);

            var desc = BuildDescription(
                family,
                baseStat,
                attributeNameEn,
                attributeNameZh,
                loreTextEn,
                loreTextZh);

            var selection = family switch
            {
                SpecializedFamily.Defensive => FeatSelection.Defensive,
                SpecializedFamily.Maneuver => FeatSelection.Maneuver,
                SpecializedFamily.Skilled => FeatSelection.Skilled,
                SpecializedFamily.Arcane => FeatSelection.Arcane,
                _ => throw new System.ArgumentOutOfRangeException(nameof(family)),
            };
            var cfg = selection.NewFeat(internalName, guid)
                .SetDisplayName(Common.L($"{familyKey}_{attributeKey}.Name", flavorNameEn, flavorNameZh))
                .SetDescription(Common.L(
                    $"{familyKey}_{attributeKey}.Desc",
                    desc.en,
                    desc.zh,
                    tagEncyclopediaEntries: true))
                .SetIconIfPresent(internalName);

            Common.AddRank(cfg, baseStat, AbilityRankType.Default, Common.ResolveProgression(settings.powerLevel, ScalingIntent.Full));
            if (family == SpecializedFamily.Arcane)
            {
                Common.AddRank(cfg, baseStat, AbilityRankType.StatBonus, Common.ResolveProgression(settings.powerLevel, ScalingIntent.Half));
            }

            switch (family)
            {
                case SpecializedFamily.Defensive:
                    if (settings.EnableDefenses)
                    {
                        AddContextBonuses(cfg, DefensiveThemes[baseStat].stats, AbilityRankType.Default);
                    }
                    break;
                case SpecializedFamily.Maneuver:
                    if (settings.EnableManeuvers)
                    {
                        AddContextBonuses(cfg, stats, AbilityRankType.Default);
                    }
                    break;
                case SpecializedFamily.Skilled:
                    foreach (var stat in SkilledThemes[baseStat].stats)
                    {
                        var isCheck = stat == StatType.CheckBluff || stat == StatType.CheckDiplomacy || stat == StatType.CheckIntimidate;
                        if (isCheck ? settings.EnableChecks : settings.EnableSkills)
                            AddContextBonuses(cfg, new[] { stat }, AbilityRankType.Default);
                    }
                    break;
                case SpecializedFamily.Arcane:
                    if (settings.EnableCasterLevel)
                    {
                        cfg.AddComponent<IncreaseCasterLevel>(c =>
                        {
                            c.Value = Common.Rank(AbilityRankType.Default);
                            c.Descriptor = Desc;
                        });
                    }

                    if (settings.EnableCasterDC)
                    {
                        cfg.AddComponent<IncreaseAllSpellsDC>(c =>
                        {
                            c.Value = Common.Rank(AbilityRankType.StatBonus);
                            c.Descriptor = Desc;
                            c.SpellsOnly = false;
                        });
                    }

                    if (settings.EnableSpellPenetration)
                    {
                        cfg.AddComponent<SpellPenetrationBonus>(c =>
                        {
                            c.Value = Common.Rank(AbilityRankType.Default);
                            c.Descriptor = Desc;
                        });
                    }
                    break;
            }

            cfg.AddRecalculateOnStatChange(stat: baseStat);
            return cfg.Configure();
        }


        private static void AddContextBonuses(FeatureConfigurator cfg, IReadOnlyList<StatType> stats, AbilityRankType rankType)
        {
            foreach (var stat in stats)
            {
                cfg.AddContextStatBonus(stat, Common.Rank(rankType), Desc);
            }
        }

        private static (string en, string zh) BuildDescription(
            SpecializedFamily family,
            StatType baseStat,
            string attributeNameEn,
            string attributeNameZh,
            string loreTextEn,
            string loreTextZh)
        {
            var familyNameEn = GetFamilyDisplayName(family);
            var familyNameZh = GetFamilyDisplayNameZh(family);
            var effectEn = Common.Format("Text.SpecializedFeats.BuildDescription.effect", "{0} Attribute-based bonuses use a minimum modifier of 0 and apply only when their corresponding mod settings are enabled.", false, GetEffectText(family, baseStat, attributeNameEn));
            var effectZh = Common.Format("Text.SpecializedFeats.BuildDescription.effect", "{0} 属性加值以调整值最低0计算，且仅在对应模组设置启用时生效。", true, GetEffectTextZh(family, baseStat, attributeNameZh));
            var restrictionEn = GetRestrictionText(family);
            var restrictionZh = GetRestrictionTextZh(family);

            var en = Common.Format("Text.SpecializedFeats.BuildDescription.Description", "<i>{0} · {1}</i>\n{2}\n\n<b>Effect:</b> {3}\n\n<b>Restrictions:</b> {4}", false, familyNameEn, attributeNameEn, loreTextEn, effectEn, restrictionEn);
            var zh = Common.Format("Text.SpecializedFeats.BuildDescription.Description", "<i>{0} · {1}</i>\n{2}\n\n<b>效果：</b>{3}\n\n<b>限制：</b>{4}", true, familyNameZh, attributeNameZh, loreTextZh, effectZh, restrictionZh);
            return (en, zh);
        }

        private static string GetEffectText(SpecializedFamily family, StatType baseStat, string attributeName)
        {
            switch (family)
            {
                case SpecializedFamily.Defensive:
                    return Common.Format("Text.SpecializedFeats.GetEffectText.Text", "Adds your {0} modifier (untyped) to {1}.", false, attributeName, DefensiveThemes[baseStat].en);
                case SpecializedFamily.Maneuver:
                    return Common.Format("Text.SpecializedFeats.GetEffectText.Text.2", "Adds your {0} modifier (untyped) to CMB.", false, attributeName);
                case SpecializedFamily.Skilled:
                    return Common.Format("Text.SpecializedFeats.GetEffectText.Text", "Adds your {0} modifier (untyped) to {1}.", false, attributeName, SkilledThemes[baseStat].en);
                case SpecializedFamily.Arcane:
                    return Common.Format("Text.SpecializedFeats.GetEffectText.Text.3", "Adds your {0} modifier (untyped) to caster level and spell penetration checks, plus half your {0} modifier (rounded down) to spell and ability save DCs in Balanced mode, or the full modifier in Legacy_AllFull mode.", false, attributeName);
                default:
                    return string.Empty;
            }
        }

        private static string GetEffectTextZh(SpecializedFamily family, StatType baseStat, string attributeZh)
        {
            switch (family)
            {
                case SpecializedFamily.Defensive:
                    return Common.Format("Text.SpecializedFeats.GetEffectText.Text", "将你的{0}调整值（无类型加值）附加至{1}。", true, attributeZh, DefensiveThemes[baseStat].zh);
                case SpecializedFamily.Maneuver:
                    return Common.Format("Text.SpecializedFeats.GetEffectText.Text.2", "将你的{0}调整值（无类型加值）附加至战技加值（CMB）。", true, attributeZh);
                case SpecializedFamily.Skilled:
                    return Common.Format("Text.SpecializedFeats.GetEffectText.Text", "将你的{0}调整值（无类型加值）附加至{1}。", true, attributeZh, SkilledThemes[baseStat].zh);
                case SpecializedFamily.Arcane:
                    return Common.Format("Text.SpecializedFeats.GetEffectText.Text.3", "将你的{0}调整值（无类型加值）附加至施法者等级与法术抗力穿透检定，在Balanced模式下将半数{0}调整值（向下取整）附加至法术及能力豁免DC，在Legacy_AllFull模式下使用完整调整值。", true, attributeZh);
                default:
                    return string.Empty;
            }
        }

        private static string GetRestrictionText(SpecializedFamily family)
        {
            switch (family)
            {
                case SpecializedFamily.Defensive:
                    return Common.Text("Text.SpecializedFeats.GetRestrictionText.Text", "While its exclusion group is on (configurable in mod settings), you can have only one Defensive Adept feat.", false);
                case SpecializedFamily.Maneuver:
                    return Common.Text("Text.SpecializedFeats.GetRestrictionText.Text.2", "While its exclusion group is on (configurable in mod settings), you can have only one Maneuver Adept feat.", false);
                case SpecializedFamily.Skilled:
                    return Common.Text("Text.SpecializedFeats.GetRestrictionText.Text.3", "While its exclusion group is on (configurable in mod settings), you can have only one Skilled feat.", false);
                case SpecializedFamily.Arcane:
                    return Common.Text("Text.SpecializedFeats.GetRestrictionText.Text.4", "While its exclusion group is on (configurable in mod settings), you can have only one Arcane Insight feat.", false);
                default:
                    return string.Empty;
            }
        }

        private static string GetRestrictionTextZh(SpecializedFamily family)
        {
            switch (family)
            {
                case SpecializedFamily.Defensive:
                    return Common.Text("Text.SpecializedFeats.GetRestrictionText.Text", "启用该互斥组时（可在模组设置中调整），只能拥有一个防御行家专长。", true);
                case SpecializedFamily.Maneuver:
                    return Common.Text("Text.SpecializedFeats.GetRestrictionText.Text.2", "启用该互斥组时（可在模组设置中调整），只能拥有一个战技行家专长。", true);
                case SpecializedFamily.Skilled:
                    return Common.Text("Text.SpecializedFeats.GetRestrictionText.Text.3", "启用该互斥组时（可在模组设置中调整），只能拥有一个技能行家专长。", true);
                case SpecializedFamily.Arcane:
                    return Common.Text("Text.SpecializedFeats.GetRestrictionText.Text.4", "启用该互斥组时（可在模组设置中调整），只能拥有一个奥术洞察专长。", true);
                default:
                    return string.Empty;
            }
        }

        private static string GetFamilyKey(SpecializedFamily family)
        {
            switch (family)
            {
                case SpecializedFamily.Defensive:
                    return "Defensive";
                case SpecializedFamily.Maneuver:
                    return "Maneuver";
                case SpecializedFamily.Skilled:
                    return "Skilled";
                case SpecializedFamily.Arcane:
                    return "Arcane";
                default:
                    return family.ToString();
            }
        }

        private static string GetFamilyDisplayName(SpecializedFamily family)
        {
            switch (family)
            {
                case SpecializedFamily.Defensive:
                    return Common.Text("Text.SpecializedFeats.GetFamilyDisplayName.Text", "Defensive Adept", false);
                case SpecializedFamily.Maneuver:
                    return Common.Text("Text.SpecializedFeats.GetFamilyDisplayName.Text.2", "Maneuver Adept", false);
                case SpecializedFamily.Skilled:
                    return Common.Text("Text.SpecializedFeats.GetFamilyDisplayName.Text.3", "Skilled", false);
                case SpecializedFamily.Arcane:
                    return Common.Text("Text.SpecializedFeats.GetFamilyDisplayName.Text.4", "Arcane Insight", false);
                default:
                    return family.ToString();
            }
        }

        private static string GetFamilyDisplayNameZh(SpecializedFamily family)
        {
            switch (family)
            {
                case SpecializedFamily.Defensive:
                    return Common.Text("Text.SpecializedFeats.GetFamilyDisplayName.Text", "防御行家", true);
                case SpecializedFamily.Maneuver:
                    return Common.Text("Text.SpecializedFeats.GetFamilyDisplayName.Text.2", "战技行家", true);
                case SpecializedFamily.Skilled:
                    return Common.Text("Text.SpecializedFeats.GetFamilyDisplayName.Text.3", "技能行家", true);
                case SpecializedFamily.Arcane:
                    return Common.Text("Text.SpecializedFeats.GetFamilyDisplayName.Text.4", "奥术洞察", true);
                default:
                    return family.ToString();
            }
        }

        private static string GetAttributeKey(StatType baseStat)
        {
            switch (baseStat)
            {
                case StatType.Strength:
                    return "Str";
                case StatType.Dexterity:
                    return "Dex";
                case StatType.Constitution:
                    return "Con";
                case StatType.Intelligence:
                    return "Int";
                case StatType.Wisdom:
                    return "Wis";
                case StatType.Charisma:
                    return "Cha";
                default:
                    return baseStat.ToString();
            }
        }

        private static string GetAttributeName(StatType baseStat)
        {
            switch (baseStat)
            {
                case StatType.Strength:
                    return Common.Text("Text.SpecializedFeats.GetAttributeName.Text", "Strength", false);
                case StatType.Dexterity:
                    return Common.Text("Text.SpecializedFeats.GetAttributeName.Text.2", "Dexterity", false);
                case StatType.Constitution:
                    return Common.Text("Text.SpecializedFeats.GetAttributeName.Text.3", "Constitution", false);
                case StatType.Intelligence:
                    return Common.Text("Text.SpecializedFeats.GetAttributeName.Text.4", "Intelligence", false);
                case StatType.Wisdom:
                    return Common.Text("Text.SpecializedFeats.GetAttributeName.Text.5", "Wisdom", false);
                case StatType.Charisma:
                    return Common.Text("Text.SpecializedFeats.GetAttributeName.Text.6", "Charisma", false);
                default:
                    return baseStat.ToString();
            }
        }

        private static string GetAttributeNameZh(StatType baseStat)
        {
            switch (baseStat)
            {
                case StatType.Strength:
                    return Common.Text("Text.SpecializedFeats.GetAttributeName.Text", "力量", true);
                case StatType.Dexterity:
                    return Common.Text("Text.SpecializedFeats.GetAttributeName.Text.2", "敏捷", true);
                case StatType.Constitution:
                    return Common.Text("Text.SpecializedFeats.GetAttributeName.Text.3", "体质", true);
                case StatType.Intelligence:
                    return Common.Text("Text.SpecializedFeats.GetAttributeName.Text.4", "智力", true);
                case StatType.Wisdom:
                    return Common.Text("Text.SpecializedFeats.GetAttributeName.Text.5", "感知", true);
                case StatType.Charisma:
                    return Common.Text("Text.SpecializedFeats.GetAttributeName.Text.6", "魅力", true);
                default:
                    return baseStat.ToString();
            }
        }
    }
}
