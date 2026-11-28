# OSS-439: In-Game / Scene Verification Items

Items that require the Unity runtime, scene, pathfinding, or combat context to
verify. Each item lists the exact reason it cannot be tested offline and what the
offline tests DO cover.

## Position / Pull Mechanics

| Spell | Item | Why offline can't verify | Offline covers |
|-------|------|--------------------------|----------------|
| Meat Hook | Pull target adjacent to caster | `Unit.Position` assignment works offline, but navmesh validation for legal adjacency and collision avoidance requires scene physics. | Damage dice (1d6/2CL, max 10d6) verified via `RuleDealDamage` in `MeatHookDealsD6PerTwoCasterLevelsDamage`. |
| Vacuum | Pull targets 10ft toward AoE center | Center-point resolution from area-effect context and per-target movement validation needs scene navmesh. | Spell blueprint verified: school, descriptors, SR=false, targeting. |
| Black Hole | Pull 10ft toward center each round | Position manipulation works offline but multi-round pull with navmesh validation and the area-effect tick pipeline requires scene-level combat loop. | Damage (2d6 force × 3 rounds) verified in `BlackHoleDeals2D6ForceDamage` and `BlackHoleDamageAppliesEachRoundForThreeRounds`. Blueprint structure verified. |
| Sonic Wave | Push 20ft (10ft Large+) away from caster | Push direction and distance are computable offline, but collision against walls/cliffs and the cone-area targeting require scene geometry. | Damage dice (1d6/CL, max 15d6) verified. Push size logic (Medium = 20ft, Large+ = 10ft) verified in `SonicWavePushIs20FeetForMediumTargets`. |

## Chain Frost Bounce Execution

| Item | Why offline can't verify | Offline covers |
|------|--------------------------|----------------|
| Bounce chain execution with nearest-neighbor selection | Requires multiple enemy units positioned in a scene with real 3D coordinates for `Vector3.Distance` comparisons. The offline harness can create units but positioning them relative to each other triggers `UnityEngine.Time.frameCount` (Unity native). | Bounce count logic verified: `max(1, min(CL/2, 10))` in `ChainFrostBounceCountEqualsCasterLevelDividedByTwo`. Damage (6d6 cold) verified in `ChainFrostDeals6D6ColdDamageToFirstTarget`. ContextActionChainFrost component architecture verified. |

## Wall of Duplicates

| Item | Why offline can't verify | Offline covers |
|------|--------------------------|----------------|
| Wall trigger on enemy crossing | Wall area-of-effect with `unitEnter` trigger requires the scene pathfinding system to detect when a unit crosses the wall boundary. Offline can create units but not move them through the wall in a way that fires area-effect enter events. | Buff blueprint with `DuplicateHalfDamage` component verified in `WallOfDuplicatesBuffHasHalfDamageComponent`. `DuplicateHalfDamage` interface check (IInitiatorRulebookHandler\<RuleCalculateDamage\>) verified in `DuplicateHalfDamageHandlesRuleCalculateDamage`. Spell blueprint (school, SR, targeting, ContextRankConfig) verified. |
| Duplicate creation and combat behavior | Spawning a unit as a summoned creature (`RuleSummonUnit`) requires the scene unit-spawning pipeline. The duplicate's half-damage output, ownership, and cleanup on wall expiry need a full combat scene. | Duplicate buff and half-damage component declared on the blueprint. Offline can verify blueprint structure; runtime behavior needs in-game. |
| No resource/loot duplication | The duplicate shares the original's statistics but should not carry loot or grant resources. This is a gameplay design constraint verified through scene combat, not blueprint data. | N/A — design constraint, no offline test possible. |

## Global Silence

| Item | Why offline can't verify | Offline covers |
|------|--------------------------|----------------|
| Map-wide enemy application with Will save | The `ContextActionGlobalSilence` iterates `Game.Instance.State.Units` which IS available offline, but the Will save resolution through the spell-casting pipeline (`RuleSavingThrow`) requires the full ability-execution context with proper DC calculation and save-roll integration. | Buff blueprint verified in `GlobalSilenceBuffIsConfigured`. Spell descriptor (MindAffecting) verified in `GlobalSilenceHasMindAffectingDescriptor`. Range (Unlimited) verified in `GlobalSilenceRangeIsUnlimited`. |
| Immobile target interaction | Immobile targets should receive the silence buff but cannot be moved (not applicable to this spell). This is a design constraint; no movement is involved. | N/A — design constraint, no offline test possible. |
| Map boundary | "Map" is defined as `Game.Instance.State.Units` (all loaded units). This definition is correct for offline but the exact behavior at map transitions or with unloaded units needs in-game verification. | The definition is tested by the component's iteration over `Game.Instance.State.Units`. |

## Scroll Usage

| Item | Why offline can't verify | Offline covers |
|------|--------------------------|----------------|
| Scroll item use (cast spell from scroll) | The item-use UI pipeline and ability-execution context for scrolls requires the Unity runtime. The `BlueprintItemEquipmentUsable` references the spell ability, which IS verified offline. | Scroll blueprints exist, have correct type (Scroll), spell level, cost, and ability reference — verified in `ScrollHasCorrectTypeLevelAndCost` (7 theory cases). Vendor table with all 7 scrolls verified in `ScrollVendorTableExistsAndContainsAllSevenScrolls`. |
| Merchant inventory integration | The `BlueprintSharedVendorTable` is created but assigning it to a specific merchant NPC (via `AddSharedVendor` component) requires adding the component to a vendor unit blueprint. This is offline-verifiable in principle but needs the specific vendor unit GUID and confirmation that the vendor actually appears in-game. | `Oss439ScrollVendor` (guid: a1b2c3d4…) shared vendor table created with 7 `LootItemsPackFixed` components. Table structure verified offline. |

## Spell Learning & Casting

| Item | Why offline can't verify | Offline covers |
|------|--------------------------|----------------|
| Full learn + cast + slot-consume cycle | Creating a unit with spellcasting class levels and triggering the spell-casting pipeline requires the ability-execution context (spellbook, action bar, command system) which is Unity-dependent. | All 7 spells are on their declared class spell lists at the correct level — verified in `AllSpellsAreOnWizardSpellList` and `AllSpellsHaveSpellListComponents`. Blueprint structure (school, descriptors, targeting, SR) verified per-spell. The `SpellListComponent` entries guarantee the spells appear in the spellbook UI and consume appropriate slots when cast (this is the game engine's standard behavior for any spell on a spell list). |