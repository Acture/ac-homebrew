using System.Collections.Generic;
using System.Linq;
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
                LocalizedText name,
                string guid,
                SpellSchool school,
                StatType attribute,
                LocalizedText lore)
            {
                InternalName = internalName;
                Name = name;
                Guid = guid;
                School = school;
                Attribute = attribute;
                Lore = lore;
            }

            public string InternalName { get; }
            public LocalizedText Name { get; }
            public string Guid { get; }
            public SpellSchool School { get; }
            public StatType Attribute { get; }
            public LocalizedText Lore { get; }
        }

        private sealed class DescriptorFeatDefinition
        {
            public DescriptorFeatDefinition(
                string internalName,
                LocalizedText name,
                string guid,
                SpellDescriptor descriptor,
                StatType attribute,
                LocalizedText lore)
            {
                InternalName = internalName;
                Name = name;
                Guid = guid;
                Descriptor = descriptor;
                Attribute = attribute;
                Lore = lore;
            }

            public string InternalName { get; }
            public LocalizedText Name { get; }
            public string Guid { get; }
            public SpellDescriptor Descriptor { get; }
            public StatType Attribute { get; }
            public LocalizedText Lore { get; }
        }

        public static void ConfigureAll()
        {
            if (Initialized) return;
            Initialized = true;

            var schools = new[]
            {
                CreateSchoolFeat(new SchoolFeatDefinition(
                    internalName: "PureWarder",
                    name: LocalizedText.Text("SpellTag_School_PureWarder.Name", "Aegis of the Pure Warder"),
                    guid: Guids.SpellTag.School.PureWarder,
                    school: SpellSchool.Abjuration,
                    attribute: StatType.Wisdom,
                    lore: LocalizedText.Text("SpellTag_School_PureWarder.Lore", "You approach abjuration with patient attention, choosing its wards over other branches of magic."))),
                CreateSchoolFeat(new SchoolFeatDefinition(
                    internalName: "MasterCaller",
                    name: LocalizedText.Text("SpellTag_School_MasterCaller.Name", "Sovereign Gatekeeper"),
                    guid: Guids.SpellTag.School.MasterCaller,
                    school: SpellSchool.Conjuration,
                    attribute: StatType.Charisma,
                    lore: LocalizedText.Text("SpellTag_School_MasterCaller.Lore", "Your magic finds its clearest voice at the threshold between a call and an answer."))),
                CreateSchoolFeat(new SchoolFeatDefinition(
                    internalName: "SeersEdge",
                    name: LocalizedText.Text("SpellTag_School_SeersEdge.Name", "Eye of the Chronomancer"),
                    guid: Guids.SpellTag.School.SeersEdge,
                    school: SpellSchool.Divination,
                    attribute: StatType.Intelligence,
                    lore: LocalizedText.Text("SpellTag_School_SeersEdge.Lore", "You compare signs and patterns until divination feels like reading a carefully kept record."))),
                CreateSchoolFeat(new SchoolFeatDefinition(
                    internalName: "HeartsTyrant",
                    name: LocalizedText.Text("SpellTag_School_HeartsTyrant.Name", "Sovereign of the Heart"),
                    guid: Guids.SpellTag.School.HeartsTyrant,
                    school: SpellSchool.Enchantment,
                    attribute: StatType.Charisma,
                    lore: LocalizedText.Text("SpellTag_School_HeartsTyrant.Lore", "You study how conviction and desire give enchantment a foothold in another mind."))),
                CreateSchoolFeat(new SchoolFeatDefinition(
                    internalName: "Spellforge",
                    name: LocalizedText.Text("SpellTag_School_Spellforge.Name", "Pyre of the Architect"),
                    guid: Guids.SpellTag.School.Spellforge,
                    school: SpellSchool.Evocation,
                    attribute: StatType.Intelligence,
                    lore: LocalizedText.Text("SpellTag_School_Spellforge.Lore", "You favor the direct expression of energy, shaping evocation with an architect's deliberate design."))),
                CreateSchoolFeat(new SchoolFeatDefinition(
                    internalName: "Veilweaver",
                    name: LocalizedText.Text("SpellTag_School_Veilweaver.Name", "Phantasmagoria Maestro"),
                    guid: Guids.SpellTag.School.Veilweaver,
                    school: SpellSchool.Illusion,
                    attribute: StatType.Charisma,
                    lore: LocalizedText.Text("SpellTag_School_Veilweaver.Lore", "Light, expectation, and careful presentation become the instruments of your illusion craft."))),
                CreateSchoolFeat(new SchoolFeatDefinition(
                    internalName: "DeathSpeaker",
                    name: LocalizedText.Text("SpellTag_School_DeathSpeaker.Name", "Harvester of the Boneyard"),
                    guid: Guids.SpellTag.School.DeathSpeaker,
                    school: SpellSchool.Necromancy,
                    attribute: StatType.Wisdom,
                    lore: LocalizedText.Text("SpellTag_School_DeathSpeaker.Lore", "You study mortality's thresholds; the Boneyard is an image for that inquiry, not a claim of Pharasma's approval."))),
                CreateSchoolFeat(new SchoolFeatDefinition(
                    internalName: "ShapeShifter",
                    name: LocalizedText.Text("SpellTag_School_ShapeShifter.Name", "Sculptor of Prime Matter"),
                    guid: Guids.SpellTag.School.ShapeShifter,
                    school: SpellSchool.Transmutation,
                    attribute: StatType.Intelligence,
                    lore: LocalizedText.Text("SpellTag_School_ShapeShifter.Lore", "You consider every material form a structure whose relationships can be understood and rearranged."))),
            };

            var descriptors = new[]
            {
                CreateDescriptorFeat(new DescriptorFeatDefinition(
                    internalName: "InnerFlame",
                    name: LocalizedText.Text("SpellTag_Descriptor_InnerFlame.Name", "Pyre of the Phoenix"),
                    guid: Guids.SpellTag.Descriptor.InnerFlame,
                    descriptor: SpellDescriptor.Fire,
                    attribute: StatType.Charisma,
                    lore: LocalizedText.Text("SpellTag_Descriptor_InnerFlame.Lore", "You give fire magic the fierce expression of your own conviction, borrowing the phoenix as its image."))),
                CreateDescriptorFeat(new DescriptorFeatDefinition(
                    internalName: "FrozenHeart",
                    name: LocalizedText.Text("SpellTag_Descriptor_FrozenHeart.Name", "Stillness of the Glacial Void"),
                    guid: Guids.SpellTag.Descriptor.FrozenHeart,
                    descriptor: SpellDescriptor.Cold,
                    attribute: StatType.Wisdom,
                    lore: LocalizedText.Text("SpellTag_Descriptor_FrozenHeart.Lore", "Still attention lends your cold magic the patient character of a glacier."))),
                CreateDescriptorFeat(new DescriptorFeatDefinition(
                    internalName: "StormChannel",
                    name: LocalizedText.Text("SpellTag_Descriptor_StormChannel.Name", "Tempest-Dancer"),
                    guid: Guids.SpellTag.Descriptor.StormChannel,
                    descriptor: SpellDescriptor.Electricity,
                    attribute: StatType.Dexterity,
                    lore: LocalizedText.Text("SpellTag_Descriptor_StormChannel.Lore", "Your practiced gestures trace the paths by which lightning takes shape."))),
                CreateDescriptorFeat(new DescriptorFeatDefinition(
                    internalName: "EtchingMind",
                    name: LocalizedText.Text("SpellTag_Descriptor_EtchingMind.Name", "Vitriolic Equation"),
                    guid: Guids.SpellTag.Descriptor.EtchingMind,
                    descriptor: SpellDescriptor.Acid,
                    attribute: StatType.Intelligence,
                    lore: LocalizedText.Text("SpellTag_Descriptor_EtchingMind.Lore", "You bring an alchemist's attention to acid magic, considering how each surface yields to corrosion."))),
                CreateDescriptorFeat(new DescriptorFeatDefinition(
                    internalName: "ResonantVoice",
                    name: LocalizedText.Text("SpellTag_Descriptor_ResonantVoice.Name", "Herald of the Shattered Sky"),
                    guid: Guids.SpellTag.Descriptor.ResonantVoice,
                    descriptor: SpellDescriptor.Sonic,
                    attribute: StatType.Charisma,
                    lore: LocalizedText.Text("SpellTag_Descriptor_ResonantVoice.Lore", "You shape sonic magic with the same conviction that carries your voice across a hall."))),
                CreateDescriptorFeat(new DescriptorFeatDefinition(
                    internalName: "EthericMind",
                    name: LocalizedText.Text("SpellTag_Descriptor_EthericMind.Name", "Axiom of Unseen Force"),
                    guid: Guids.SpellTagDescriptor2.EthericMind,
                    descriptor: SpellDescriptor.Force,
                    attribute: StatType.Intelligence,
                    lore: LocalizedText.Text("SpellTag_Descriptor_EthericMind.Lore", "You picture invisible force as an exact arrangement of pressures and boundaries."))),
                CreateDescriptorFeat(new DescriptorFeatDefinition(
                    internalName: "RadiantSoul",
                    name: LocalizedText.Text("SpellTag_Descriptor_RadiantSoul.Name", "Fountain of Solar Dawn"),
                    guid: Guids.SpellTagDescriptor2.RadiantSoul,
                    descriptor: PositiveEnergyDescriptor,
                    attribute: StatType.Charisma,
                    lore: LocalizedText.Text("SpellTag_Descriptor_RadiantSoul.Lore", "The image of first light gives your positive-energy magic a clear and generous expression."))),
                CreateDescriptorFeat(new DescriptorFeatDefinition(
                    internalName: "HollowHeart",
                    name: LocalizedText.Text("SpellTag_Descriptor_HollowHeart.Name", "Vigil of the Gloom"),
                    guid: Guids.SpellTagDescriptor2.HollowHeart,
                    descriptor: NegativeEnergyDescriptor,
                    attribute: StatType.Wisdom,
                    lore: LocalizedText.Text("SpellTag_Descriptor_HollowHeart.Lore", "You contemplate endings until negative-energy magic finds a restrained, deliberate form."))),
                CreateDescriptorFeat(new DescriptorFeatDefinition(
                    internalName: "SubtleTyrant",
                    name: LocalizedText.Text("SpellTag_Descriptor_SubtleTyrant.Name", "Puppeteer of the Mind"),
                    guid: Guids.SpellTagDescriptor2.SubtleTyrant,
                    descriptor: SpellDescriptor.MindAffecting,
                    attribute: StatType.Charisma,
                    lore: LocalizedText.Text("SpellTag_Descriptor_SubtleTyrant.Lore", "You study the threads of attention through which mind-affecting magic takes hold."))),
            };

        }

        private static BlueprintFeature CreateSchoolFeat(SchoolFeatDefinition definition)
        {
            var desc = BuildSchoolDescription(definition);
            var settings = Mod.Settings ?? new ModSettings();
            var cfg = FeatSelection.SpellSchool.NewFeat(definition.InternalName, definition.Guid)
                .SetDisplayName(Common.L($"SpellTag_School_{definition.InternalName}.Name", definition.Name))
                .SetDescription(Common.L(
                    $"SpellTag_School_{definition.InternalName}.Desc",
                    desc,
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
                .SetDisplayName(Common.L($"SpellTag_Descriptor_{definition.InternalName}.Name", definition.Name))
                .SetDescription(Common.L(
                    $"SpellTag_Descriptor_{definition.InternalName}.Desc",
                    desc,
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

        private static LocalizedText BuildSchoolDescription(SchoolFeatDefinition def)
            => LocalizedText.Format("Text.SpellTagFeats.BuildSchoolDescription.Description", "<i>Spell Tag · School Specialist · {0} · {1}</i>\n{2}\n\n<b>Effect:</b> When their corresponding mod settings are enabled, adds half your {1} modifier (rounded down, minimum 0, untyped) to matching {0} spell and ability save DC and caster level, while all other schools take an equal penalty.\n\n<b>Restrictions:</b> While its exclusion group is on (configurable in mod settings), you can have only one Spell Tag School Specialist feat, but can stack with Descriptor Specialist feats. Your mastery comes at a price — DC and caster level of all other schools suffer an equal penalty.", GetSchoolName(def.School), GetAttributeName(def.Attribute), def.Lore);

        private static LocalizedText BuildDescriptorDescription(DescriptorFeatDefinition def)
            => LocalizedText.Format("Text.SpellTagFeats.BuildDescriptorDescription.Description", "<i>Spell Tag · Descriptor Specialist · {0} · {1}</i>\n{2}\n\n<b>Effect:</b> When their corresponding mod settings are enabled, adds half your {1} modifier (rounded down, minimum 0, untyped) to save DC and caster level for spells and abilities with the {0} descriptor group. Subtracts the same amount for EACH other matching group ({3}). Bonuses and penalties combine on spells and abilities with multiple matching groups.\n\n<b>Restrictions:</b> While its exclusion group is on (configurable in mod settings), you can have only one Spell Tag Descriptor Specialist feat, but can stack with School Specialist feats. Your mastery comes at a price — DC and caster level of {3} spells and abilities suffer an equal penalty.", GetDescriptorName(def.Descriptor), GetAttributeName(def.Attribute), def.Lore, GetOtherDescriptorList(def.Descriptor));

        private static LocalizedText GetSchoolName(SpellSchool school)
        {
            switch (school)
            {
                case SpellSchool.Abjuration: return LocalizedText.Text("Text.SpellTagFeats.GetSchoolName.Text", "Abjuration");
                case SpellSchool.Conjuration: return LocalizedText.Text("Text.SpellTagFeats.GetSchoolName.Text.2", "Conjuration");
                case SpellSchool.Divination: return LocalizedText.Text("Text.SpellTagFeats.GetSchoolName.Text.3", "Divination");
                case SpellSchool.Enchantment: return LocalizedText.Text("Text.SpellTagFeats.GetSchoolName.Text.4", "Enchantment");
                case SpellSchool.Evocation: return LocalizedText.Text("Text.SpellTagFeats.GetSchoolName.Text.5", "Evocation");
                case SpellSchool.Illusion: return LocalizedText.Text("Text.SpellTagFeats.GetSchoolName.Text.6", "Illusion");
                case SpellSchool.Necromancy: return LocalizedText.Text("Text.SpellTagFeats.GetSchoolName.Text.7", "Necromancy");
                case SpellSchool.Transmutation: return LocalizedText.Text("Text.SpellTagFeats.GetSchoolName.Text.8", "Transmutation");
                default: return LocalizedText.Invariant(school.ToString());
            }
        }

        private static LocalizedText GetDescriptorName(SpellDescriptor descriptor)
        {
            switch (descriptor)
            {
                case SpellDescriptor.Fire: return LocalizedText.Text("Text.SpellTagFeats.GetDescriptorName.Text", "Fire");
                case SpellDescriptor.Cold: return LocalizedText.Text("Text.SpellTagFeats.GetDescriptorName.Text.2", "Cold");
                case SpellDescriptor.Electricity: return LocalizedText.Text("Text.SpellTagFeats.GetDescriptorName.Text.3", "Electricity");
                case SpellDescriptor.Acid: return LocalizedText.Text("Text.SpellTagFeats.GetDescriptorName.Text.4", "Acid");
                case SpellDescriptor.Sonic: return LocalizedText.Text("Text.SpellTagFeats.GetDescriptorName.Text.5", "Sonic");
                case SpellDescriptor.Force: return LocalizedText.Text("Text.SpellTagFeats.GetDescriptorName.Text.6", "Force");
                case PositiveEnergyDescriptor: return LocalizedText.Text("Text.SpellTagFeats.GetDescriptorName.Text.7", "Positive Energy");
                case NegativeEnergyDescriptor: return LocalizedText.Text("Text.SpellTagFeats.GetDescriptorName.Text.8", "Negative Energy");
                case SpellDescriptor.MindAffecting: return LocalizedText.Text("Text.SpellTagFeats.GetDescriptorName.Text.9", "Mind-Affecting");
                default: return LocalizedText.Invariant(descriptor.ToString());
            }
        }

        private static LocalizedText GetOtherDescriptorList(SpellDescriptor chosenDescriptor)
        {
            var names = new List<LocalizedText>();
            foreach (var descriptor in Descriptors)
            {
                if (descriptor == chosenDescriptor) continue;
                names.Add(GetDescriptorName(descriptor));
            }

            return LocalizedText.Compose(locale => Common.JoinList(names.Select(name => name[locale]).ToList(), locale));
        }

        private static LocalizedText GetAttributeName(StatType baseStat)
        {
            switch (baseStat)
            {
                case StatType.Strength: return LocalizedText.Text("Text.SpellTagFeats.GetAttributeName.Text", "Strength");
                case StatType.Dexterity: return LocalizedText.Text("Text.SpellTagFeats.GetAttributeName.Text.2", "Dexterity");
                case StatType.Constitution: return LocalizedText.Text("Text.SpellTagFeats.GetAttributeName.Text.3", "Constitution");
                case StatType.Intelligence: return LocalizedText.Text("Text.SpellTagFeats.GetAttributeName.Text.4", "Intelligence");
                case StatType.Wisdom: return LocalizedText.Text("Text.SpellTagFeats.GetAttributeName.Text.5", "Wisdom");
                case StatType.Charisma: return LocalizedText.Text("Text.SpellTagFeats.GetAttributeName.Text.6", "Charisma");
                default: return LocalizedText.Invariant(baseStat.ToString());
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
