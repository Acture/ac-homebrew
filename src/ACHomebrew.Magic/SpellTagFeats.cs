using System.Collections.Generic;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Parts;

namespace ACHomebrew.Feats
{
    internal static class SpellTagFeats
    {
        private static readonly ModifierDescriptor Desc = ModifierDescriptor.None;
        private const SpellDescriptor PositiveEnergyDescriptor = SpellDescriptor.Cure
            | SpellDescriptor.RestoreHP
            | SpellDescriptor.ChannelPositiveHeal
            | SpellDescriptor.ChannelPositiveHarm;
        private const SpellDescriptor NegativeEnergyDescriptor = SpellDescriptor.ChannelNegativeHeal
            | SpellDescriptor.ChannelNegativeHarm
            | SpellDescriptor.NegativeLevel;
        private static readonly SpellSchool[] Schools =
        {
            SpellSchool.Abjuration,
            SpellSchool.Conjuration,
            SpellSchool.Divination,
            SpellSchool.Enchantment,
            SpellSchool.Evocation,
            SpellSchool.Illusion,
            SpellSchool.Necromancy,
            SpellSchool.Transmutation,
        };

        private static readonly SpellDescriptor[] Descriptors =
        {
            SpellDescriptor.Fire,
            SpellDescriptor.Cold,
            SpellDescriptor.Electricity,
            SpellDescriptor.Acid,
            SpellDescriptor.Sonic,
            SpellDescriptor.Force,
            PositiveEnergyDescriptor,
            NegativeEnergyDescriptor,
            SpellDescriptor.MindAffecting,
        };

        private static bool Initialized;

        private sealed class SchoolFeatDefinition
        {
            public SchoolFeatDefinition(
                string internalName,
                string nameEn,
                string nameZh,
                string guid,
                SpellSchool school,
                StatType attribute,
                string loreEn,
                string loreZh)
            {
                InternalName = internalName;
                NameEn = nameEn;
                NameZh = nameZh;
                Guid = guid;
                School = school;
                Attribute = attribute;
                LoreEn = loreEn;
                LoreZh = loreZh;
            }

            public string InternalName { get; }
            public string NameEn { get; }
            public string NameZh { get; }
            public string Guid { get; }
            public SpellSchool School { get; }
            public StatType Attribute { get; }
            public string LoreEn { get; }
            public string LoreZh { get; }
        }

        private sealed class DescriptorFeatDefinition
        {
            public DescriptorFeatDefinition(
                string internalName,
                string nameEn,
                string nameZh,
                string guid,
                SpellDescriptor descriptor,
                StatType attribute,
                string loreEn,
                string loreZh)
            {
                InternalName = internalName;
                NameEn = nameEn;
                NameZh = nameZh;
                Guid = guid;
                Descriptor = descriptor;
                Attribute = attribute;
                LoreEn = loreEn;
                LoreZh = loreZh;
            }

            public string InternalName { get; }
            public string NameEn { get; }
            public string NameZh { get; }
            public string Guid { get; }
            public SpellDescriptor Descriptor { get; }
            public StatType Attribute { get; }
            public string LoreEn { get; }
            public string LoreZh { get; }
        }

        public static void ConfigureAll()
        {
            if (Initialized) return;
            Initialized = true;

            var schools = new[]
            {
                CreateSchoolFeat(new SchoolFeatDefinition(
                    internalName: "PureWarder",
                    nameEn: Common.Text("SpellTag_School_PureWarder.Name", "Aegis of the Pure Warder"),
                    nameZh: Common.Text("SpellTag_School_PureWarder.Name", "绝界镇魔使", true),
                    guid: Guids.SpellTag.School.PureWarder,
                    school: SpellSchool.Abjuration,
                    attribute: StatType.Wisdom,
                    loreEn: Common.Text("SpellTag_School_PureWarder.Lore", "You approach abjuration with patient attention, choosing its wards over other branches of magic."),
                    loreZh: Common.Text("SpellTag_School_PureWarder.Lore", "你以耐心而专注的觉察修习防护术，并将它的结界置于其他魔法之前。", true))),
                CreateSchoolFeat(new SchoolFeatDefinition(
                    internalName: "MasterCaller",
                    nameEn: Common.Text("SpellTag_School_MasterCaller.Name", "Sovereign Gatekeeper"),
                    nameZh: Common.Text("SpellTag_School_MasterCaller.Name", "统界辟门者", true),
                    guid: Guids.SpellTag.School.MasterCaller,
                    school: SpellSchool.Conjuration,
                    attribute: StatType.Charisma,
                    loreEn: Common.Text("SpellTag_School_MasterCaller.Lore", "Your magic finds its clearest voice at the threshold between a call and an answer."),
                    loreZh: Common.Text("SpellTag_School_MasterCaller.Lore", "你的魔法在呼唤与回应的门槛之间，找到最清晰的表达。", true))),
                CreateSchoolFeat(new SchoolFeatDefinition(
                    internalName: "SeersEdge",
                    nameEn: Common.Text("SpellTag_School_SeersEdge.Name", "Eye of the Chronomancer"),
                    nameZh: Common.Text("SpellTag_School_SeersEdge.Name", "溯时先知", true),
                    guid: Guids.SpellTag.School.SeersEdge,
                    school: SpellSchool.Divination,
                    attribute: StatType.Intelligence,
                    loreEn: Common.Text("SpellTag_School_SeersEdge.Lore", "You compare signs and patterns until divination feels like reading a carefully kept record."),
                    loreZh: Common.Text("SpellTag_School_SeersEdge.Lore", "你反复比对征兆与规律，让预言术如阅读详尽的记录般有章可循。", true))),
                CreateSchoolFeat(new SchoolFeatDefinition(
                    internalName: "HeartsTyrant",
                    nameEn: Common.Text("SpellTag_School_HeartsTyrant.Name", "Sovereign of the Heart"),
                    nameZh: Common.Text("SpellTag_School_HeartsTyrant.Name", "倾心国主", true),
                    guid: Guids.SpellTag.School.HeartsTyrant,
                    school: SpellSchool.Enchantment,
                    attribute: StatType.Charisma,
                    loreEn: Common.Text("SpellTag_School_HeartsTyrant.Lore", "You study how conviction and desire give enchantment a foothold in another mind."),
                    loreZh: Common.Text("SpellTag_School_HeartsTyrant.Lore", "你研究信念与欲望如何为惑控术提供触及他人心智的立足点。", true))),
                CreateSchoolFeat(new SchoolFeatDefinition(
                    internalName: "Spellforge",
                    nameEn: Common.Text("SpellTag_School_Spellforge.Name", "Pyre of the Architect"),
                    nameZh: Common.Text("SpellTag_School_Spellforge.Name", "灾变筑城师", true),
                    guid: Guids.SpellTag.School.Spellforge,
                    school: SpellSchool.Evocation,
                    attribute: StatType.Intelligence,
                    loreEn: Common.Text("SpellTag_School_Spellforge.Lore", "You favor the direct expression of energy, shaping evocation with an architect's deliberate design."),
                    loreZh: Common.Text("SpellTag_School_Spellforge.Lore", "你偏爱能量的直接表达，以建筑师般的审慎规划塑成塑能术。", true))),
                CreateSchoolFeat(new SchoolFeatDefinition(
                    internalName: "Veilweaver",
                    nameEn: Common.Text("SpellTag_School_Veilweaver.Name", "Phantasmagoria Maestro"),
                    nameZh: Common.Text("SpellTag_School_Veilweaver.Name", "织影幻圣", true),
                    guid: Guids.SpellTag.School.Veilweaver,
                    school: SpellSchool.Illusion,
                    attribute: StatType.Charisma,
                    loreEn: Common.Text("SpellTag_School_Veilweaver.Lore", "Light, expectation, and careful presentation become the instruments of your illusion craft."),
                    loreZh: Common.Text("SpellTag_School_Veilweaver.Lore", "光影、期待与精心安排的呈现，成为你编织幻术的工具。", true))),
                CreateSchoolFeat(new SchoolFeatDefinition(
                    internalName: "DeathSpeaker",
                    nameEn: Common.Text("SpellTag_School_DeathSpeaker.Name", "Harvester of the Boneyard"),
                    nameZh: Common.Text("SpellTag_School_DeathSpeaker.Name", "冥河渡魂人", true),
                    guid: Guids.SpellTag.School.DeathSpeaker,
                    school: SpellSchool.Necromancy,
                    attribute: StatType.Wisdom,
                    loreEn: Common.Text("SpellTag_School_DeathSpeaker.Lore", "You study mortality's thresholds; the Boneyard is an image for that inquiry, not a claim of Pharasma's approval."),
                    loreZh: Common.Text("SpellTag_School_DeathSpeaker.Lore", "你研习死生的界限；骨园只是这份探究的意象，并不代表获得法拉斯玛的认可。", true))),
                CreateSchoolFeat(new SchoolFeatDefinition(
                    internalName: "ShapeShifter",
                    nameEn: Common.Text("SpellTag_School_ShapeShifter.Name", "Sculptor of Prime Matter"),
                    nameZh: Common.Text("SpellTag_School_ShapeShifter.Name", "塑质造化使", true),
                    guid: Guids.SpellTag.School.ShapeShifter,
                    school: SpellSchool.Transmutation,
                    attribute: StatType.Intelligence,
                    loreEn: Common.Text("SpellTag_School_ShapeShifter.Lore", "You consider every material form a structure whose relationships can be understood and rearranged."),
                    loreZh: Common.Text("SpellTag_School_ShapeShifter.Lore", "你将物质的形态视作可被理解与重新安排的结构关系。", true))),
            };

            var descriptors = new[]
            {
                CreateDescriptorFeat(new DescriptorFeatDefinition(
                    internalName: "InnerFlame",
                    nameEn: Common.Text("SpellTag_Descriptor_InnerFlame.Name", "Pyre of the Phoenix"),
                    nameZh: Common.Text("SpellTag_Descriptor_InnerFlame.Name", "炽皇凤涅", true),
                    guid: Guids.SpellTag.Descriptor.InnerFlame,
                    descriptor: SpellDescriptor.Fire,
                    attribute: StatType.Charisma,
                    loreEn: Common.Text("SpellTag_Descriptor_InnerFlame.Lore", "You give fire magic the fierce expression of your own conviction, borrowing the phoenix as its image."),
                    loreZh: Common.Text("SpellTag_Descriptor_InnerFlame.Lore", "你让火焰魔法承载自身热烈的信念，借凤凰作为它的意象。", true))),
                CreateDescriptorFeat(new DescriptorFeatDefinition(
                    internalName: "FrozenHeart",
                    nameEn: Common.Text("SpellTag_Descriptor_FrozenHeart.Name", "Stillness of the Glacial Void"),
                    nameZh: Common.Text("SpellTag_Descriptor_FrozenHeart.Name", "极渊玄冰", true),
                    guid: Guids.SpellTag.Descriptor.FrozenHeart,
                    descriptor: SpellDescriptor.Cold,
                    attribute: StatType.Wisdom,
                    loreEn: Common.Text("SpellTag_Descriptor_FrozenHeart.Lore", "Still attention lends your cold magic the patient character of a glacier."),
                    loreZh: Common.Text("SpellTag_Descriptor_FrozenHeart.Lore", "沉静的专注赋予寒冷魔法如冰川般耐心而坚定的气质。", true))),
                CreateDescriptorFeat(new DescriptorFeatDefinition(
                    internalName: "StormChannel",
                    nameEn: Common.Text("SpellTag_Descriptor_StormChannel.Name", "Tempest-Dancer"),
                    nameZh: Common.Text("SpellTag_Descriptor_StormChannel.Name", "御雷疾影", true),
                    guid: Guids.SpellTag.Descriptor.StormChannel,
                    descriptor: SpellDescriptor.Electricity,
                    attribute: StatType.Dexterity,
                    loreEn: Common.Text("SpellTag_Descriptor_StormChannel.Lore", "Your practiced gestures trace the paths by which lightning takes shape."),
                    loreZh: Common.Text("SpellTag_Descriptor_StormChannel.Lore", "娴熟的手势描出雷电逐渐成形的路径。", true))),
                CreateDescriptorFeat(new DescriptorFeatDefinition(
                    internalName: "EtchingMind",
                    nameEn: Common.Text("SpellTag_Descriptor_EtchingMind.Name", "Vitriolic Equation"),
                    nameZh: Common.Text("SpellTag_Descriptor_EtchingMind.Name", "腐解算律", true),
                    guid: Guids.SpellTag.Descriptor.EtchingMind,
                    descriptor: SpellDescriptor.Acid,
                    attribute: StatType.Intelligence,
                    loreEn: Common.Text("SpellTag_Descriptor_EtchingMind.Lore", "You bring an alchemist's attention to acid magic, considering how each surface yields to corrosion."),
                    loreZh: Common.Text("SpellTag_Descriptor_EtchingMind.Lore", "你以炼金术师般的专注修习强酸魔法，思量不同表面如何承受腐蚀。", true))),
                CreateDescriptorFeat(new DescriptorFeatDefinition(
                    internalName: "ResonantVoice",
                    nameEn: Common.Text("SpellTag_Descriptor_ResonantVoice.Name", "Herald of the Shattered Sky"),
                    nameZh: Common.Text("SpellTag_Descriptor_ResonantVoice.Name", "破霄神音", true),
                    guid: Guids.SpellTag.Descriptor.ResonantVoice,
                    descriptor: SpellDescriptor.Sonic,
                    attribute: StatType.Charisma,
                    loreEn: Common.Text("SpellTag_Descriptor_ResonantVoice.Lore", "You shape sonic magic with the same conviction that carries your voice across a hall."),
                    loreZh: Common.Text("SpellTag_Descriptor_ResonantVoice.Lore", "你以让声音响彻厅堂的同一种信念，塑成音波魔法。", true))),
                CreateDescriptorFeat(new DescriptorFeatDefinition(
                    internalName: "EthericMind",
                    nameEn: Common.Text("SpellTag_Descriptor_EthericMind.Name", "Axiom of Unseen Force"),
                    nameZh: Common.Text("SpellTag_Descriptor_EthericMind.Name", "虚空定则", true),
                    guid: Guids.SpellTagDescriptor2.EthericMind,
                    descriptor: SpellDescriptor.Force,
                    attribute: StatType.Intelligence,
                    loreEn: Common.Text("SpellTag_Descriptor_EthericMind.Lore", "You picture invisible force as an exact arrangement of pressures and boundaries."),
                    loreZh: Common.Text("SpellTag_Descriptor_EthericMind.Lore", "你将无形的力场想象为压力与边界的精确排列。", true))),
                CreateDescriptorFeat(new DescriptorFeatDefinition(
                    internalName: "RadiantSoul",
                    nameEn: Common.Text("SpellTag_Descriptor_RadiantSoul.Name", "Fountain of Solar Dawn"),
                    nameZh: Common.Text("SpellTag_Descriptor_RadiantSoul.Name", "金阳圣晖", true),
                    guid: Guids.SpellTagDescriptor2.RadiantSoul,
                    descriptor: PositiveEnergyDescriptor,
                    attribute: StatType.Charisma,
                    loreEn: Common.Text("SpellTag_Descriptor_RadiantSoul.Lore", "The image of first light gives your positive-energy magic a clear and generous expression."),
                    loreZh: Common.Text("SpellTag_Descriptor_RadiantSoul.Lore", "初升晨光的意象，让正能量魔法有了明晰而慷慨的表达。", true))),
                CreateDescriptorFeat(new DescriptorFeatDefinition(
                    internalName: "HollowHeart",
                    nameEn: Common.Text("SpellTag_Descriptor_HollowHeart.Name", "Vigil of the Gloom"),
                    nameZh: Common.Text("SpellTag_Descriptor_HollowHeart.Name", "死寂枯荣", true),
                    guid: Guids.SpellTagDescriptor2.HollowHeart,
                    descriptor: NegativeEnergyDescriptor,
                    attribute: StatType.Wisdom,
                    loreEn: Common.Text("SpellTag_Descriptor_HollowHeart.Lore", "You contemplate endings until negative-energy magic finds a restrained, deliberate form."),
                    loreZh: Common.Text("SpellTag_Descriptor_HollowHeart.Lore", "你静思万物的终结，使负能量魔法呈现克制而审慎的形式。", true))),
                CreateDescriptorFeat(new DescriptorFeatDefinition(
                    internalName: "SubtleTyrant",
                    nameEn: Common.Text("SpellTag_Descriptor_SubtleTyrant.Name", "Puppeteer of the Mind"),
                    nameZh: Common.Text("SpellTag_Descriptor_SubtleTyrant.Name", "惑魂主宰", true),
                    guid: Guids.SpellTagDescriptor2.SubtleTyrant,
                    descriptor: SpellDescriptor.MindAffecting,
                    attribute: StatType.Charisma,
                    loreEn: Common.Text("SpellTag_Descriptor_SubtleTyrant.Lore", "You study the threads of attention through which mind-affecting magic takes hold."),
                    loreZh: Common.Text("SpellTag_Descriptor_SubtleTyrant.Lore", "你研究影响心灵的魔法如何沿注意力的细线取得立足点。", true))),
            };

        }

        private static BlueprintFeature CreateSchoolFeat(SchoolFeatDefinition definition)
        {
            var desc = BuildSchoolDescription(definition);
            var settings = Mod.Settings ?? new ModSettings();
            var cfg = FeatSelection.SpellSchool.NewFeat(definition.InternalName, definition.Guid)
                .SetDisplayName(Common.L($"SpellTag_School_{definition.InternalName}.Name", definition.NameEn, definition.NameZh))
                .SetDescription(Common.L(
                    $"SpellTag_School_{definition.InternalName}.Desc",
                    desc.en,
                    desc.zh,
                    tagEncyclopediaEntries: true))
                .SetIconIfPresent(definition.InternalName);

            Common.AddRank(cfg, definition.Attribute, AbilityRankType.Default, ContextRankProgression.Div2);

            if (settings.EnableCasterDC)
            {
                cfg.AddComponent<ContextIncreaseSpellSchoolDC>(c =>
                {
                    c.School = definition.School;
                    c.BonusDC = Common.Rank();
                    c.Descriptor = Desc;
                });

                foreach (var otherSchool in Schools)
                {
                    if (otherSchool == definition.School) continue;
                    cfg.AddComponent<ContextIncreaseSpellSchoolDC>(c =>
                    {
                        c.School = otherSchool;
                        c.BonusDC = Common.Rank();
                        c.Multiplier = -1;
                        c.Descriptor = Desc;
                    });
                }
            }

            if (settings.EnableCasterLevel)
            {
                cfg.AddComponent<ContextIncreaseSpellSchoolCasterLevel>(c =>
                {
                    c.School = definition.School;
                    c.BonusCasterLevel = Common.Rank();
                    c.Descriptor = Desc;
                });

                foreach (var otherSchool in Schools)
                {
                    if (otherSchool == definition.School) continue;
                    cfg.AddComponent<ContextIncreaseSpellSchoolCasterLevel>(c =>
                    {
                        c.School = otherSchool;
                        c.BonusCasterLevel = Common.Rank();
                        c.Multiplier = -1;
                        c.Descriptor = Desc;
                    });
                }
            }

            cfg.AddRecalculateOnStatChange(stat: definition.Attribute);
            return cfg.Configure();
        }

        private static BlueprintFeature CreateDescriptorFeat(DescriptorFeatDefinition definition)
        {
            var desc = BuildDescriptorDescription(definition);
            var settings = Mod.Settings ?? new ModSettings();
            var cfg = FeatSelection.SpellDescriptor.NewFeat(definition.InternalName, definition.Guid)
                .SetDisplayName(Common.L($"SpellTag_Descriptor_{definition.InternalName}.Name", definition.NameEn, definition.NameZh))
                .SetDescription(Common.L(
                    $"SpellTag_Descriptor_{definition.InternalName}.Desc",
                    desc.en,
                    desc.zh,
                    tagEncyclopediaEntries: true))
                .SetIconIfPresent(definition.InternalName);

            Common.AddRank(cfg, definition.Attribute, AbilityRankType.Default, ContextRankProgression.Div2);

            if (settings.EnableCasterDC)
            {
                cfg.AddComponent<ContextIncreaseSpellDescriptorDC>(c =>
                {
                    c.Descriptor = definition.Descriptor;
                    c.BonusDC = Common.Rank();
                    c.ModifierDescriptor = Desc;
                });

                foreach (var otherDescriptor in Descriptors)
                {
                    if (otherDescriptor == definition.Descriptor) continue;
                    cfg.AddComponent<ContextIncreaseSpellDescriptorDC>(c =>
                    {
                        c.Descriptor = otherDescriptor;
                        c.BonusDC = Common.Rank();
                        c.Multiplier = -1;
                        c.ModifierDescriptor = Desc;
                    });
                }
            }

            if (settings.EnableCasterLevel)
            {
                cfg.AddComponent<ContextIncreaseSpellDescriptorCasterLevel>(c =>
                {
                    c.Descriptor = definition.Descriptor;
                    c.BonusCasterLevel = Common.Rank();
                    c.ModifierDescriptor = Desc;
                });

                foreach (var otherDescriptor in Descriptors)
                {
                    if (otherDescriptor == definition.Descriptor) continue;
                    cfg.AddComponent<ContextIncreaseSpellDescriptorCasterLevel>(c =>
                    {
                        c.Descriptor = otherDescriptor;
                        c.BonusCasterLevel = Common.Rank();
                        c.Multiplier = -1;
                        c.ModifierDescriptor = Desc;
                    });
                }
            }

            cfg.AddRecalculateOnStatChange(stat: definition.Attribute);
            return cfg.Configure();
        }


        private static (string en, string zh) BuildSchoolDescription(SchoolFeatDefinition def)
        {
            var schoolEn = GetSchoolName(def.School, false);
            var schoolZh = GetSchoolName(def.School, true);
            var attrEn = GetAttributeName(def.Attribute, false);
            var attrZh = GetAttributeName(def.Attribute, true);

            var en = Common.Format("Text.SpellTagFeats.BuildSchoolDescription.Description", "<i>Spell Tag · School Specialist · {0} · {1}</i>\n{2}\n\n<b>Effect:</b> When their corresponding mod settings are enabled, adds half your {1} modifier (rounded down, minimum 0, untyped) to matching {0} spell and ability save DC and caster level, while all other schools take an equal penalty.\n\n<b>Restrictions:</b> While its exclusion group is on (configurable in mod settings), you can have only one Spell Tag School Specialist feat, but can stack with Descriptor Specialist feats. Your mastery comes at a price — DC and caster level of all other schools suffer an equal penalty.", false, schoolEn, attrEn, def.LoreEn);
            var zh = Common.Format("Text.SpellTagFeats.BuildSchoolDescription.Description", "<i>法术专精 · 学派特化 · {0} · {1}</i>\n{2}\n\n<b>效果：</b>对应模组设置启用时，将你的{1}调整值的一半（向下取整、最低0）作为无类型加值附加至{0}法术及能力的豁免难度（DC）与施法者等级，但所有其他学派的法术及能力承受等额减值。\n\n<b>限制：</b>启用该互斥组时（可在模组设置中调整），只能拥有一个“学派特化”专长，但可与“描述符特化”专长叠加。极致的专精伴随代价——其他学派的法术及能力DC与施法者等级将承受等额减值惩罚。", true, schoolZh, attrZh, def.LoreZh);
            return (en, zh);
        }

        private static (string en, string zh) BuildDescriptorDescription(DescriptorFeatDefinition def)
        {
            var descEn = GetDescriptorName(def.Descriptor, false);
            var descZh = GetDescriptorName(def.Descriptor, true);
            var attrEn = GetAttributeName(def.Attribute, false);
            var attrZh = GetAttributeName(def.Attribute, true);
            var otherEn = GetOtherDescriptorList(def.Descriptor, false);
            var otherZh = GetOtherDescriptorList(def.Descriptor, true);

            var en = Common.Format("Text.SpellTagFeats.BuildDescriptorDescription.Description", "<i>Spell Tag · Descriptor Specialist · {0} · {1}</i>\n{2}\n\n<b>Effect:</b> When their corresponding mod settings are enabled, adds half your {1} modifier (rounded down, minimum 0, untyped) to save DC and caster level for spells and abilities with the {0} descriptor group. Subtracts the same amount for EACH other matching group ({3}). Bonuses and penalties combine on spells and abilities with multiple matching groups.\n\n<b>Restrictions:</b> While its exclusion group is on (configurable in mod settings), you can have only one Spell Tag Descriptor Specialist feat, but can stack with School Specialist feats. Your mastery comes at a price — DC and caster level of {3} spells and abilities suffer an equal penalty.", false, descEn, attrEn, def.LoreEn, otherEn);
            var zh = Common.Format("Text.SpellTagFeats.BuildDescriptorDescription.Description", "<i>法术专精 · 描述符特化 · {0} · {1}</i>\n{2}\n\n<b>效果：</b>对应模组设置启用时，将你的{1}调整值的一半（向下取整、最低0）作为无类型加值附加至匹配{0}描述符组的法术及能力豁免DC与施法者等级。每匹配一个其他描述符组（{3}），便承受等额减值；多描述符法术及能力同时计算相应加值与减值。\n\n<b>限制：</b>启用该互斥组时（可在模组设置中调整），只能拥有一个“描述符特化”专长，但可与“学派特化”专长叠加。极致的专精伴随代价——{3}法术及能力的DC与施法者等级将承受等额减值惩罚。", true, descZh, attrZh, def.LoreZh, otherZh);
            return (en, zh);
        }

        private static string GetSchoolName(SpellSchool school, bool zh = false)
        {
            if (zh)
            {
                switch (school)
                {
                    case SpellSchool.Abjuration: return Common.Text("Text.SpellTagFeats.GetSchoolName.Text", "防护系", true);
                    case SpellSchool.Conjuration: return Common.Text("Text.SpellTagFeats.GetSchoolName.Text.2", "咒法系", true);
                    case SpellSchool.Divination: return Common.Text("Text.SpellTagFeats.GetSchoolName.Text.3", "预言系", true);
                    case SpellSchool.Enchantment: return Common.Text("Text.SpellTagFeats.GetSchoolName.Text.4", "惑控系", true);
                    case SpellSchool.Evocation: return Common.Text("Text.SpellTagFeats.GetSchoolName.Text.5", "塑能系", true);
                    case SpellSchool.Illusion: return Common.Text("Text.SpellTagFeats.GetSchoolName.Text.6", "幻术系", true);
                    case SpellSchool.Necromancy: return Common.Text("Text.SpellTagFeats.GetSchoolName.Text.7", "死灵系", true);
                    case SpellSchool.Transmutation: return Common.Text("Text.SpellTagFeats.GetSchoolName.Text.8", "变化系", true);
                    default: return school.ToString();
                }
            }
            switch (school)
            {
                case SpellSchool.Abjuration: return Common.Text("Text.SpellTagFeats.GetSchoolName.Text", "Abjuration", false);
                case SpellSchool.Conjuration: return Common.Text("Text.SpellTagFeats.GetSchoolName.Text.2", "Conjuration", false);
                case SpellSchool.Divination: return Common.Text("Text.SpellTagFeats.GetSchoolName.Text.3", "Divination", false);
                case SpellSchool.Enchantment: return Common.Text("Text.SpellTagFeats.GetSchoolName.Text.4", "Enchantment", false);
                case SpellSchool.Evocation: return Common.Text("Text.SpellTagFeats.GetSchoolName.Text.5", "Evocation", false);
                case SpellSchool.Illusion: return Common.Text("Text.SpellTagFeats.GetSchoolName.Text.6", "Illusion", false);
                case SpellSchool.Necromancy: return Common.Text("Text.SpellTagFeats.GetSchoolName.Text.7", "Necromancy", false);
                case SpellSchool.Transmutation: return Common.Text("Text.SpellTagFeats.GetSchoolName.Text.8", "Transmutation", false);
                default: return school.ToString();
            }
        }

        private static string GetDescriptorName(SpellDescriptor descriptor, bool zh = false)
        {
            if (zh)
            {
                switch (descriptor)
                {
                    case SpellDescriptor.Fire: return Common.Text("Text.SpellTagFeats.GetDescriptorName.Text", "火焰", true);
                    case SpellDescriptor.Cold: return Common.Text("Text.SpellTagFeats.GetDescriptorName.Text.2", "寒冷", true);
                    case SpellDescriptor.Electricity: return Common.Text("Text.SpellTagFeats.GetDescriptorName.Text.3", "电击", true);
                    case SpellDescriptor.Acid: return Common.Text("Text.SpellTagFeats.GetDescriptorName.Text.4", "强酸", true);
                    case SpellDescriptor.Sonic: return Common.Text("Text.SpellTagFeats.GetDescriptorName.Text.5", "音波", true);
                    case SpellDescriptor.Force: return Common.Text("Text.SpellTagFeats.GetDescriptorName.Text.6", "力场", true);
                    case PositiveEnergyDescriptor: return Common.Text("Text.SpellTagFeats.GetDescriptorName.Text.7", "正能量", true);
                    case NegativeEnergyDescriptor: return Common.Text("Text.SpellTagFeats.GetDescriptorName.Text.8", "负能量", true);
                    case SpellDescriptor.MindAffecting: return Common.Text("Text.SpellTagFeats.GetDescriptorName.Text.9", "影响心灵", true);
                    default: return descriptor.ToString();
                }
            }
            switch (descriptor)
            {
                case SpellDescriptor.Fire: return Common.Text("Text.SpellTagFeats.GetDescriptorName.Text", "Fire", false);
                case SpellDescriptor.Cold: return Common.Text("Text.SpellTagFeats.GetDescriptorName.Text.2", "Cold", false);
                case SpellDescriptor.Electricity: return Common.Text("Text.SpellTagFeats.GetDescriptorName.Text.3", "Electricity", false);
                case SpellDescriptor.Acid: return Common.Text("Text.SpellTagFeats.GetDescriptorName.Text.4", "Acid", false);
                case SpellDescriptor.Sonic: return Common.Text("Text.SpellTagFeats.GetDescriptorName.Text.5", "Sonic", false);
                case SpellDescriptor.Force: return Common.Text("Text.SpellTagFeats.GetDescriptorName.Text.6", "Force", false);
                case PositiveEnergyDescriptor: return Common.Text("Text.SpellTagFeats.GetDescriptorName.Text.7", "Positive Energy", false);
                case NegativeEnergyDescriptor: return Common.Text("Text.SpellTagFeats.GetDescriptorName.Text.8", "Negative Energy", false);
                case SpellDescriptor.MindAffecting: return Common.Text("Text.SpellTagFeats.GetDescriptorName.Text.9", "Mind-Affecting", false);
                default: return descriptor.ToString();
            }
        }

        private static string GetOtherDescriptorList(SpellDescriptor chosenDescriptor, bool zh = false)
        {
            var names = new List<string>();
            foreach (var descriptor in Descriptors)
            {
                if (descriptor == chosenDescriptor) continue;
                names.Add(GetDescriptorName(descriptor, zh));
            }

            return JoinWithAnd(names, zh);
        }

        private static string JoinWithAnd(IReadOnlyList<string> values, bool zh = false)
        {
            if (values.Count == 0) return string.Empty;
            if (zh)
            {
                return string.Join("、", values);
            }
            if (values.Count == 1) return values[0];
            if (values.Count == 2) return $"{values[0]} and {values[1]}";

            var text = values[0];
            for (var i = 1; i < values.Count - 1; i++)
            {
                text += $", {values[i]}";
            }

            return $"{text}, and {values[values.Count - 1]}";
        }

        private static string GetAttributeName(StatType baseStat, bool zh = false)
        {
            if (zh)
            {
                switch (baseStat)
                {
                    case StatType.Strength: return Common.Text("Text.SpellTagFeats.GetAttributeName.Text", "力量", true);
                    case StatType.Dexterity: return Common.Text("Text.SpellTagFeats.GetAttributeName.Text.2", "敏捷", true);
                    case StatType.Constitution: return Common.Text("Text.SpellTagFeats.GetAttributeName.Text.3", "体质", true);
                    case StatType.Intelligence: return Common.Text("Text.SpellTagFeats.GetAttributeName.Text.4", "智力", true);
                    case StatType.Wisdom: return Common.Text("Text.SpellTagFeats.GetAttributeName.Text.5", "感知", true);
                    case StatType.Charisma: return Common.Text("Text.SpellTagFeats.GetAttributeName.Text.6", "魅力", true);
                    default: return baseStat.ToString();
                }
            }
            switch (baseStat)
            {
                case StatType.Strength: return Common.Text("Text.SpellTagFeats.GetAttributeName.Text", "Strength", false);
                case StatType.Dexterity: return Common.Text("Text.SpellTagFeats.GetAttributeName.Text.2", "Dexterity", false);
                case StatType.Constitution: return Common.Text("Text.SpellTagFeats.GetAttributeName.Text.3", "Constitution", false);
                case StatType.Intelligence: return Common.Text("Text.SpellTagFeats.GetAttributeName.Text.4", "Intelligence", false);
                case StatType.Wisdom: return Common.Text("Text.SpellTagFeats.GetAttributeName.Text.5", "Wisdom", false);
                case StatType.Charisma: return Common.Text("Text.SpellTagFeats.GetAttributeName.Text.6", "Charisma", false);
                default: return baseStat.ToString();
            }
        }
    }

    [TypeId("4de42b15f8964e32935e82f193b5ce97")]
    internal class ContextIncreaseSpellSchoolDC : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>, ISubscriber, IInitiatorRulebookSubscriber
    {
        public SpellSchool School;
        public ContextValue BonusDC;
        public int Multiplier = 1;
        public ModifierDescriptor Descriptor = ModifierDescriptor.None;

        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            if (!SpellTagMechanics.MatchesSchool(evt, School)) return;
            evt.AddBonusDC(BonusDC.Calculate(Context) * Multiplier, Descriptor);
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
        }
    }

    [TypeId("9ab3dc9b7ec340f7bf9863ffbbca2cb8")]
    internal class ContextIncreaseSpellSchoolCasterLevel : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>, ISubscriber, IInitiatorRulebookSubscriber
    {
        public SpellSchool School;
        public ContextValue BonusCasterLevel;
        public int Multiplier = 1;
        public ModifierDescriptor Descriptor = ModifierDescriptor.None;

        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            if (!SpellTagMechanics.MatchesSchool(evt, School)) return;
            evt.AddBonusCasterLevel(BonusCasterLevel.Calculate(Context) * Multiplier, Descriptor);
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
        }
    }

    [TypeId("55318f5b9ee3432682d70a86ae58f0da")]
    internal class ContextIncreaseSpellDescriptorDC : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>, ISubscriber, IInitiatorRulebookSubscriber
    {
        public SpellDescriptorWrapper Descriptor;
        public ContextValue BonusDC;
        public int Multiplier = 1;
        public ModifierDescriptor ModifierDescriptor = ModifierDescriptor.None;
        public bool SpellsOnly = false;

        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            if (SpellsOnly && evt.Spellbook == null) return;
            if (!SpellTagMechanics.MatchesDescriptor(evt, Owner, Descriptor)) return;
            evt.AddBonusDC(BonusDC.Calculate(Context) * Multiplier, ModifierDescriptor);
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
        }
    }

    [TypeId("13ee1911c67b4ddf8e7314f524968312")]
    internal class ContextIncreaseSpellDescriptorCasterLevel : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>, ISubscriber, IInitiatorRulebookSubscriber
    {
        public SpellDescriptorWrapper Descriptor;
        public ContextValue BonusCasterLevel;
        public int Multiplier = 1;
        public ModifierDescriptor ModifierDescriptor = ModifierDescriptor.None;
        public bool SpellsOnly = false;

        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            if (SpellsOnly && evt.Spellbook == null) return;
            if (!SpellTagMechanics.MatchesDescriptor(evt, Owner, Descriptor)) return;
            evt.AddBonusCasterLevel(BonusCasterLevel.Calculate(Context) * Multiplier, ModifierDescriptor);
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
        }
    }

    internal static class SpellTagMechanics
    {
        public static bool MatchesSchool(RuleCalculateAbilityParams evt, SpellSchool school)
        {
            var spell = GetEffectiveSpell(evt);
            if (spell == null) return false;
            if (school == SpellSchool.None) return true;
            return spell.School == school || spell.SpellComponent != null && spell.SpellComponent.School == school;
        }

        public static bool MatchesDescriptor(RuleCalculateAbilityParams evt, UnitEntityData owner, SpellDescriptorWrapper descriptor)
        {
            if ((SpellDescriptor)descriptor == SpellDescriptor.None) return true;
            var effectiveDescriptor = GetEffectiveDescriptor(evt, owner);
            return effectiveDescriptor.HasAnyFlag((SpellDescriptor)descriptor);
        }

        public static SpellDescriptor GetEffectiveDescriptor(RuleCalculateAbilityParams evt, UnitEntityData owner)
        {
            var spell = GetEffectiveSpell(evt);
            if (spell == null) return SpellDescriptor.None;
            return UnitPartChangeSpellElementalDamage.ReplaceSpellDescriptorIfCan<UnitEntityData>(owner, spell.SpellDescriptor);
        }

        private static BlueprintAbility GetEffectiveSpell(RuleCalculateAbilityParams evt)
        {
            var convertedFrom = evt.AbilityData?.ConvertedFrom;
            if (convertedFrom?.Blueprint?.AbilityShadowSpell != null)
            {
                return convertedFrom.Blueprint;
            }

            return evt.Spell;
        }
    }
}
