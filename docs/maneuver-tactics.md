# Maneuver Tactics

Choose these feats under **AC's Homebrew → Maneuver Tactics**. Both can be taken together, alongside the Maneuver aptitude feats. Each costs 2 points if the feat-point budget is enabled.

| Feat | Default effect |
|---|---|
| Seize the Opening / 乘隙制敌 | One free-action maneuver each round in combat, plus three extra free uses per encounter. An extra use permits a second free maneuver that round; two per round is the maximum. |
| Lingering Disruption / 余势未消 | Double the duration of timed maneuver effects, adding at least two rounds. Thus 1 → 3, 2 → 4 and 3 → 6 rounds. |

Seize the Opening works on active maneuver abilities. After the allowance is spent, the ability retains its normal action cost. Attempting a maneuver spends the allowance even if the check fails; opening a menu, queueing or canceling before execution does not. The feat preserves resources, cooldowns, range, targets and prerequisites. Already free abilities do not spend the allowance. It does not grant maneuver abilities you have not learned.

Lingering Disruption affects timed disarm, sunder armor and dirty trick effects, plus supported timed effects from other mods. Prone, forced movement and permanent effects keep their normal behavior. Duration is extended on each new application before the game's stacking rules; it does not repeatedly multiply an existing buff's remaining duration.

## Settings

UMM and the optional ModMenu page expose these settings. Changes apply to subsequent actions and effects; existing buffs keep their duration.

| Saved setting | Default | Meaning |
|---|---|---|
| `ManeuverFreePerRound` | 1 | Free maneuvers renewed each combat round |
| `ManeuverBurstPerCombat` | 3 | Extra free uses available each encounter |
| `ManeuverFreePerRoundCap` | 2 | Total free maneuvers allowed in a round |
| `ManeuverDurationMultiplier` | 2 | Duration multiplier, from 1 to 10 |
| `ManeuverMinimumExtraRounds` | 2 | Minimum added duration in rounds |

Count settings accept 0–99. Set the per-round cap to 0 to disable free conversion. With a duration multiplier of 1 and minimum extra rounds of 0, durations stay unchanged. Existing saves use the defaults for missing settings.

## Other mods

Standalone physical maneuver abilities built with the game's combat-maneuver actions are recognized automatically, including supported conditional and variant wrappers. A spell or a weapon-attack sequence containing a maneuver does not become free. Custom actions cannot always be identified from their blueprint; their author can register the maneuver and its timed effects after creating the blueprints:

```csharp
ACHomebrew.Feats.ManeuverCompatibility.Register(myManeuverAbility, myTimedDebuff);

// Inside the adapter, after its custom maneuver has succeeded:
using (ACHomebrew.Feats.ManeuverDuration.SuccessfulEffects(context, target))
{
    target.AddBuff(myTimedDebuff, context, duration);
}
```

The API is in `ACHomebrew.dll`. Register concrete executable variants separately where appropriate. Registration identifies the complete ability as a maneuver: use it only when the whole action should receive the free-action benefit. Pass only the timed effects that should be prolonged. Standard maneuver success actions are tracked automatically; opaque custom implementations must wrap their successful effect application in `SuccessfulEffects`. Dispose that scope before returning or scheduling later work. Merely retaining an ability context does not extend later buffs. The mod does not require Tabletop Tweaks.

Automatic recognition is not a guarantee for every mod. Abilities implemented through opaque custom execution, unusual stacking or separate spell systems need an adapter and testing with that mod enabled.
