using System.Collections.Generic;
using BlueprintCore.Actions.Builder;
using BlueprintCore.Blueprints.CustomConfigurators;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Abilities;
using BlueprintCore.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using static ACHomebrew.Feats.FeatBuilders;

namespace ACHomebrew.Feats
{
    /// <summary>Duel: a two-round duel whose victories permanently add weapon damage. Rules in docs/duel-feats.md.</summary>
    internal static class DuelFeats
    {
        private const string CavalierChallengeAbility = "9d5d58ff40e39ff4681670463abe99c9";

        public static void ConfigureAll()
        {
            // Feats use their own artwork through Feat(); the ability and its effect share the oath's icon.
            var icon = IconLoader.Get("DuelOath") ?? BlueprintTool.Get<BlueprintAbility>(CavalierChallengeAbility).Icon;

            FeatureConfigurator.New("DuelVictories", Guids.Duel.Victories)
                .SetDisplayName(Common.L("DuelVictories.Name", LocalizedText.Text("DuelVictories.Name", "Duel Victories")))
                .SetDescription(Common.L("DuelVictories.Desc", LocalizedText.Text("DuelVictories.Desc", "+1 damage with weapon attacks for each duel won.")))
                .SetHideInUI(true)
                .SetHideInCharacterSheetAndLevelUp(true)
                // Class features do not survive respec, so a respec clears victories instead of keeping one rank.
                .SetIsClassFeature(true)
                .AddComponent<DuelVictoryBonus>()
                .Configure();

            var duelBuff = Buff("DuelBuff", Guids.Duel.Buff, LocalizedText.Text("DuelBuff.Name", "Dueling"),
                    LocalizedText.Text("DuelBuff.Desc", "Can attack only the opponent, and damage dealt to anyone else is prevented. Unless protected by Unbound Duelist, compelled to attack the opponent and cannot act otherwise. Ends after 2 rounds or when either duelist falls."))
                .SetIcon(icon)
                .SetStacking(StackingType.Replace)
                .AddComponent<DuelLink>()
                .Configure();

            var resource = AbilityResourceConfigurator.New("DuelResource", Guids.Duel.Resource)
                // 1 + 1 per 5 character levels: Levels = CharacterLevel, step 5, +1 per step from level 0.
                .SetMaxAmount(ResourceAmountBuilder.New(1).IncreaseByLevelStartPlusDivStep(new string[0], 1f, 0, 0, 5, 1, 0))
                .Configure();

            var ability = AbilityConfigurator.New("DuelAbility", Guids.Duel.Ability)
                .SetDisplayName(Common.L("DuelAbility.Name", LocalizedText.Text("DuelAbility.Name", "Duel")))
                .SetDescription(Common.L("DuelAbility.Desc", LocalizedText.Text("DuelAbility.Desc", "Standard action, supernatural. Challenge a conscious enemy within close range that is not dueling. For 2 rounds, you and it can attack only each other, damage either of you deals to anyone else is prevented, and each duelist is compelled to attack the other unless it has Unbound Duelist. The duel ends early when either duelist falls. If your opponent falls unconscious or dies while you remain standing, you win and permanently gain +1 damage with weapon attacks; if you fall, your opponent gains it. Once per encounter.")))
                .SetIcon(icon)
                .SetType(AbilityType.Supernatural)
                .SetRange(AbilityRange.Close)
                .SetActionType(UnitCommand.CommandType.Standard)
                .SetAnimation(UnitAnimationActionCastSpell.CastAnimationStyle.Omni)
                .SetCanTargetEnemies(true)
                .SetCanTargetFriends(false)
                .SetCanTargetSelf(false)
                .SetCanTargetPoint(false)
                .SetEffectOnEnemy(AbilityEffectOnUnit.Harmful)
                .AddAbilityResourceLogic(amount: 1, isSpendResource: true, requiredResource: resource)
                .AddComponent<DuelAbilityRestriction>()
                .AddAbilityEffectRunAction(ActionsBuilder.New().Add<ContextActionStartDuel>())
                .Configure();

            Feat(FeatSelection.Duel, "DuelOath", Guids.Duel.Oath, LocalizedText.Text("DuelOath.Name", "Oath of the Duel"),
                    LocalizedText.Text("DuelOath.Desc", "<i>You name your foe, and the rest of the battle falls away.</i>\n\nYou gain the supernatural ability Duel. As a standard action, choose a conscious enemy within close range that is not dueling. For 2 rounds, you and it are compelled to attack only each other: neither can cast spells, use items or act otherwise, and damage either deals to anyone else is prevented. Use it 1 time per day plus 1 per 5 character levels, at most once per encounter. The duel ends early when either duelist falls.\n\nIf your opponent falls unconscious or dies during the duel while you remain standing, you win and permanently gain +1 damage with weapon attacks, without limit. If you fall, your opponent gains it instead; if both fall together, nobody wins.\n\nYour Cavalier's Challenge (including Knight's Challenge) also counts as a duel without spending Duel: when the challenged foe falls while the challenge lasts, you win, and if you fall first, it wins. A challenge does not compel either side and is settled once. Victories are kept until you respec."))
                .AddFacts(new List<Blueprint<BlueprintUnitFactReference>> { ability })
                .AddAbilityResources(resource: resource, restoreAmount: true)
                .Configure();

            Feat(FeatSelection.Duel, "EndlessChallenge", Guids.Duel.EndlessChallenge, LocalizedText.Text("EndlessChallenge.Name", "Endless Challenge"),
                    LocalizedText.Text("EndlessChallenge.Desc", "<i>Each victory only sharpens your appetite for the next rival.</i>\n\nYou can use Duel 2 more times per day. Duel remains limited to once per encounter."))
                .AddPrerequisiteFeature(Guids.Duel.Oath)
                .AddIncreaseResourceAmount(resource, 2)
                .Configure();

            Feat(FeatSelection.Duel, "SingleCombat", Guids.Duel.SingleCombat, LocalizedText.Text("SingleCombat.Name", "Single Combat"),
                    LocalizedText.Text("SingleCombat.Desc", "<i>While your eyes are on your rival, interlopers barely register.</i>\n\nWhile you are dueling, through Duel or a challenge that counts as a duel, damage you take from anyone other than your opponent is halved."))
                .AddPrerequisiteFeature(Guids.Duel.Oath)
                .AddComponent<SingleCombatGuard>()
                .Configure();

            // A mythic feat, chosen in a mythic feat slot rather than from the Homebrew menus and budget.
            FeatureConfigurator.New("MythicDuelOath", Guids.Duel.MythicOath, FeatureGroup.MythicFeat)
                .SetDisplayName(Common.L("MythicDuelOath.Name", LocalizedText.Text("MythicDuelOath.Name", "Unbound Duelist")))
                .SetDescription(Common.L("MythicDuelOath.Desc", LocalizedText.Format("Text.FeatBuilders.Desc.Text", "<i>{0}</i>\n\n<b>Effect:</b> {1}",
                    LocalizedText.Text("MythicDuelOath.Lore", "The duel binds your blade, but no longer your will."),
                    LocalizedText.Text("MythicDuelOath.Effect", "Requires Oath of the Duel. Your duels no longer compel you: you keep full control and can still cast spells, use items and help yourself or your allies, but you can attack and target hostile effects only at your opponent, and damage you deal to anyone else is still prevented. Your opponent remains compelled to attack only you.")), tagEncyclopediaEntries: true))
                .SetIconIfPresent("MythicDuelOath")
                .AddPrerequisiteFeature(Guids.Duel.Oath)
                .Configure();

            DuelRules.RegisterVanillaChallenges();
            DuelEvents.Subscribe();
        }
    }
}
