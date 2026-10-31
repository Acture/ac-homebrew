# Duel and Finger of the Reaper

Two growth mechanics: winning duels permanently adds weapon damage, and every kill with Finger of Death makes the next one stronger.

| Feat | Where | Budget | Effect |
|---|---|---|---|
| Oath of the Duel / 决斗誓约 | AC's Homebrew → Duel | 1 | Grants **Duel**. Each duel won: +1 damage with weapon attacks, permanently. Your Cavalier challenges also count as duels. |
| Endless Challenge / 无尽挑战 | AC's Homebrew → Duel | 1 | +2 uses of Duel per day. Requires Oath of the Duel. |
| Single Combat / 单挑 | AC's Homebrew → Duel | 1 | While you duel, damage from anyone but your opponent is halved. Requires Oath of the Duel. |
| Unbound Duelist / 决斗自如 | Mythic feat | — | You keep control during your duels; your opponent is still compelled. Requires Oath of the Duel. |
| Finger of the Reaper / 一指索命 | AC's Homebrew → Growth | 1 | Finger of Death deals +1 damage per caster level, plus +1 for each creature it has killed. |

## Duel

Duel is a supernatural ability: a standard action against a conscious enemy within close range. You can use it once per day plus once per 5 character levels (Endless Challenge adds 2). It can be used only once per encounter; it becomes available again after you leave combat. Neither you nor the target can already be dueling.

For 2 rounds (the game's own buff duration in real-time and turn-based mode alike):

- **Both duelists are compelled to attack each other.** They lose control, cannot cast spells, use items or act otherwise, and attack (or charge) their opponent. This uses the game's attack-nearest compulsion (the same machinery as confusion's "attack nearest" result), aimed at the opponent, so it behaves the same for the party and for enemies, in real time and in turn-based mode.
- **Each duelist can attack only the opponent.** Attacking, targeting with a hostile ability or catching another creature in its own area effects is not allowed.
- **Damage a duelist deals to anyone else is prevented.** This also covers area effects that the game always aims at the party, and attacks of opportunity.
- Other creatures are not restricted: they can still attack either duelist.

With **Unbound Duelist** (mythic), you are not compelled: you keep control and may cast, use items and help yourself and your allies, but the two restrictions above still apply to you. Your opponent is still compelled.

The duel ends for both duelists when its 2 rounds pass, when either duelist's duel effect ends, or when either duelist falls.

### Victory

A duelist **falls** when it becomes unconscious or dies. When a duelist falls during the duel, its opponent wins if that opponent is still conscious with hit points above 0; the duel then ends. If both drop in the same exchange, nobody wins. A duel that expired or was ended earlier pays nothing.

A win permanently adds **+1 to damage with weapon attacks** (the game's additional-damage statistic), without limit. Either side can win: if you fall, the enemy you dueled gains the bonus. Victories are a hidden feature of the character, saved with the game.

**Respec clears victories.** The game's respec carries over only source-less, non-class features, and then only as one rank. Victories are marked as a class feature so that a respec removes them instead of truncating them. Adding or removing Oath of the Duel by other means does not change victories already won.

### Challenges as duels

For a character with Oath of the Duel, a **Cavalier's Challenge** (including Order of the Sword's Knight's Challenge, and any ability that applies the vanilla challenge effect) also counts as a duel, without spending Duel or its once-per-encounter use:

- If the challenged creature falls while your challenge on it lasts, you win; if you fall first, the challenged creature wins.
- Each challenge is settled once, even if the challenged creature is revived and drops again.
- A challenge does not compel either side or restrict their targets; the challenge keeps its own rules.
- Single Combat applies while the challenge lasts.

Vanilla Wrath of the Righteous has no samurai. Another mod's challenge counts once its author registers the effect it places on the challenged creature, with the challenger as that effect's caster:

```csharp
ACHomebrew.Feats.DuelRules.RegisterChallenge(myChallengeTargetBuff);
```

### Single Combat

While you are in a duel (Duel, or a challenge that counts as a duel), damage you take from anyone other than your opponent is halved, rounded down, after damage reduction. Damage without a creature as its source is halved too.

## Finger of the Reaper

Your **Finger of Death** deals +1 damage per caster level, plus +1 per creature it has killed, without limit. The bonus is added to the spell's damage on either save result and works with Finger of Death from any source (spellbook, scroll, wand or other ability using the vanilla spell).

A creature counts when **it dies and your Finger of Death dealt the final, lethal damage**:

- A creature that falls unconscious, is healed, or dies later from something else does not count.
- Each death counts once; a creature raised and slain again counts again.
- Another caster's Finger of Death does not count for you.

The kill count is stored in the feat itself and saved with the game. Losing the feat, including through respec, resets it to 0.

## Tests and limits

Offline tests against the real game ([offline-mechanics-testing.md](offline-mechanics-testing.md)) cover the blueprints and prerequisites, daily uses, the real Duel ability command (resource spending, targets, once per encounter), compulsion and target restrictions, prevented and halved damage, victories for either side, expiry and simultaneous falls, challenges, and the Finger of Death bonus and kill rules, including saving the counters.

Not verified offline, and still to be confirmed in the game: compelled units actually walking up and attacking (that needs a scene), the duel in a running turn-based encounter, AI behavior, the ability's visual effects and the respec screen.
