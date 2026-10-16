# AC's Homebrew

> Formerly **AttributeFeats**. The UMM mod Id and install folder (`AttributeFeats`) and all feat IDs are unchanged, so existing settings and saves keep working. The assembly is now `ACHomebrew.dll`.

A Pathfinder: Wrath of the Righteous mod of homebrew feats that open up play styles: attribute conversion, close-quarters and long-range fighting, retaliation, stealth, solo play, summoning swarms, spell-slot tricks, immunity piercing and more.

> **Design philosophy:** each feat should be about as strong as a slightly-better vanilla or modded feat (or class feature), and cover only two or three effects or one system. Limits and exclusions are configurable, and stronger versions are reserved for mythic feats.

## Feat Families

During character creation and level-up, expand **AC's Homebrew**, then a **feat family**, then choose the **individual feat** (Main Attribute Mastery has one more level: source attribute, then target). Each choice grants one feat. You can return to a family at later feat choices, subject to prerequisites, the [feat budget](#feat-budget) and [exclusion groups](#exclusion-groups). Existing characters keep their learned feats.

Families are grouped by play theme below. Numbers are first-pass values, and **nothing in this release has been verified in the game yet**; see [validation](docs/validation.md).

### Attributes

**Main Attribute Mastery (30 feats, one per character).** Choose a source attribute, then one other attribute: add the source modifier (minimum 0) to the target score as an inherent bonus. The six broad 0.1.x Main feats (Titan's Apotheosis, Quicksilver Incarnate, Adamantine Vessel, Architect of Self, Ocular of the Cosmos, Sovereign of Wills) are retired: no longer offered, kept for characters that have them, and narrowed to half the modifier on the other five attributes. Respec to switch.

**Weapon Insight (6)** — use the attribute for weapon attack rolls.

| Feat | 中文名称 | Attribute |
|---|---|---|
| Titan's Momentum | 巨灵摧破 | Strength |
| Aldori Finesse | 阿尔多里绝诣 | Dexterity |
| Stout Grounding | 磐固千钧 | Constitution |
| Geometer's Edge | 规矩之锋 | Intelligence |
| Karmic Interception | 因果断隙 | Wisdom |
| Swashbuckler's Flourish | 游侠华彩 | Charisma |

**Extended Replacement (6)**

| Feat | 中文名称 | Effect |
|---|---|---|
| Ascetic's Ward | 云水自真 | Wis-to-AC when unarmored/light armor |
| Anatomical Leverage | 筋络推演 | Int-to-CMB |
| Monarch's Stature | 帝胄岳立 | Cha-to-CMD |
| Titan's Footing | 巨灵固步 | Str-to-CMD |
| Zephyr's Grace | 穿风灵步 | Dex-to-AC when unarmored/light armor |
| Adamantine Mettle | 生机洪炉 | Con modifier to HP (minimum 0; added once) |

**Derived Stat Conversion (6)**
- **Arcane Aegis (魔能天衣)** — Half Caster Level to AC.
- **War-Hardened Reflexes (百战身魄)** — Half BAB to all saving throws.
- **Scholar's Positioning (通识御敌)** — One third of total Skill Ranks to AC.
- **Ley-Infused Vitality (灵脉淬体)** — Caster Level to Max HP.
- **Dawn of the Soul (法相初明)** — At combat start, temporary HP equal to Caster Level, for up to 10 minutes or until combat ends.
- **Blade of the Spell-Saint (剑圣咒痕)** — Half BAB to the save DC of your spells (not class or racial abilities).

**Weapon Damage (6)** — choose an attribute, then a proficient weapon category. **Replace** mode (default) uses the attribute instead of the weapon's damage attribute when that is better, keeping the normal two-handed/off-hand multiplier; **Add** mode adds the positive modifier once. Weapons that do not already add an attribute to damage gain nothing. The mode applies on the next damage calculation.

| Mode | Example: Str 12 (+1), Int 20 (+5), one-handed longsword |
|---|---|
| Replace (default) | `1d8 + 5` |
| Add | `1d8 + 1 + 5` |

**Casting Attribute (3)** — Somatic Force (力擎法脉, Strength), Sleight of Arcana (巧指弄玄, Dexterity) or Sanguine Incantation (气血炼真, Constitution): choose one of your spellbooks; it uses that attribute for save DCs, bonus spell slots, the minimum score to cast and concentration. Settings: *Casting Scope* (selected spellbook or all spellbooks) and *Casting Mode* (always, or only when higher). The spellbook screen header may still show the original attribute.

**Resource Attribute (6)** — one per attribute (Reservoir of Might 巨力盈渊, Springs of Agility 灵动渊源, Font of Endurance 骨髓蓄沛, Well of Thought 识海蕴灵, Meditative Spring 澄心源流, Sovereign Font 华威盈座): choose one of your resources whose uses grow with an ability modifier (channel energy, ki, lay on hands, performance, bloodline or domain powers, including resources from other mods); its bonus uses come from that attribute. The new maximum applies the next time the resource is restored.

### Aptitudes

Each aptitude adds the attribute modifier (untyped) to two or three themed values.

| Family | Strength | Dexterity | Constitution | Intelligence | Wisdom | Charisma |
|---|---|---|---|---|---|---|
| **Defensive** | Colossus Bastion (巨灵重障): CMD, Fort | Wind-Dancer's Shroud (风舞虚影): AC, Ref | Inured Carapace (百炼金身): AC, Fort | Analytical Aegis (算律之盾): Initiative, Ref | Third Eye Vigil (天目清照): Will, Initiative | Majesty's Reproach (凛然天威): Will, CMD |
| **Maneuver** | Grip of the Behemoth (比蒙扼击) | Fulcrum of the Viper (灵蛇巧掣) | Deep-Root Clinch (沉洋扼锁) | Anatomical Pivot (机理断节) | Crane's Anticipation (玄鹤听劲) | Audacious Overthrow (叱喝倾山) |
| **Skilled** | Giantwright's Craft (巨匠巧工): Athletics, Intimidate | Thief-King's Panache (妙手绝尘): Mobility, Stealth, Thievery | Ascetic Diligence (苦行研磨): Athletics, Perception | Encyclopedic Synthesis (格物万象): Arcana, World, Use Magic Device | Wanderer's Lucidity (云水澄明): Perception, Nature, Religion | Silver-Tongued Virtuoso (锦绣天潢): Persuasion, Bluff, Diplomacy |
| **Arcane** | Mage-Hammer Inscription (铁骨铸咒) | Somatic Velocity (疾影手印) | Crucible of the Conduit (鼎炉承法) | Archmage's Codex (万法源流) | Gnostic Channel (玄鉴通幽) | Sovereign Decrees (天宪法旨) |

Maneuver adds the modifier to CMB. Arcane adds it to caster level and spell penetration, and half of it to save DCs.

### Martial

**Stance (6)** — activatable trades: gain the attribute modifier on some values and take an equal penalty elsewhere.

| Feat | 中文名称 | Attribute |
|---|---|---|
| Berserker's Overrun | 破阵裂山势 | Strength |
| Willow in the Gale | 惊鸿穿林势 | Dexterity |
| Mountain's Deep Roots | 不动磐峰势 | Constitution |
| Grandmaster's Gambit | 弈者静待势 | Intelligence |
| Mirror of Still Waters | 明镜止水势 | Wisdom (AC and Will; attack and damage penalty) |
| Vanguard's Banner | 金戈铁旌势 | Charisma |

**Conditional Trigger (6)**

| Feat | 中文名称 | Attribute | Trigger |
|---|---|---|---|
| Defiance at the Precipice | 绝境砥柱 | Constitution (AC and Fort) | HP < 50% |
| Ambush of the Viper | 封喉首刃 | Dexterity | Combat round 1 |
| Oath of Retribution | 复仇血誓 | Charisma | Ally dies within 30 meters; 3 rounds |
| Crane's Severance | 蓄势孤峰 | Wisdom | First weapon attack each round, including misses |
| Gorum's Last Stand | 狂神绝唱 | Strength | HP < 25% |
| Cadence Decoded | 阅破机宜 | Intelligence | Weapon attack resolves; up to 10 minutes, removed at combat boundaries |

**Distance Damage (9)** — three styles of three feats; each feat adds up to +4 damage, the three feats of one style stack to +12, and a character can follow only one style.

| Style | Feat | Scores |
|---|---|---|
| Close (melee) | Point-Blank Ruin (咫尺绝杀) | Hit distance: +4 pressed against the target, −1 per 1.5 ft |
| | Short-Blade Discipline (短刃心诀) | Light/unarmed/natural 4, one-handed 3, two-handed 2, reach 0 |
| | In-Fighting Stride (欺身锁步) | Total reach 5 ft 4, 10 ft 2, 15 ft+ 0 |
| Mid (melee) | Harmonic Cleave (流光截角) | Hit distance: +1 per 1.5 ft from the target's edge, up to 4 |
| | Long-Haft Discipline (长柄心诀) | Reach weapons 4, other two-handed 2 |
| | Measured Distance (掌距控势) | Total reach 5 ft 0, 10 ft 2, 15 ft+ 4 |
| Long (ranged, thrown) | Horizon's Deadeye (苍穹神击) | +1 per 10 ft beyond 10 ft, up to 4 |
| | Far-Reaching Arms (挽弓及远) | Weapon range 50 ft+ 4, 40 ft 3, 30 ft 2, 20 ft 1 |
| | Unhurried Aim (静息稳射) | +4 while no enemy engages you in melee |

**Long-Reach Gambit (长锋险势)** — reach × 2 with −4 weapon damage.

**Momentum** — Cascading Carnage (浴血连斩: kills stack +2 attack and +10 ft speed, up to 3), Unbroken Measure (连绵战韵: hits stack +1 attack/+2 damage up to 3; being hit removes one), Quarry's Obsession (步步紧逼: repeated attacks on one target build dodge AC, then an extra attack), Fourth-Beat Ruin (叠浪惊雷: every fourth attack is a guaranteed critical).

**Execution** — Deepening the Cleft (乘隙溃创: damage grows with the target's missing HP and your level), Pyre of Defiance (残躯燃焰: fire aura scaled by your missing HP), Colossus Reaver / Vanguard's Sunder / Arcane Resection / Primal Gluttony (猎巨诛命 / 挫敌锐芒 / 崩元析命 / 荒蛮撕嚼: bonus damage from the target's maximum or current HP via weapons, spells or natural attacks; Primal Gluttony also heals), Sanguine Tithe (饮血淬芒: heal 15% of weapon damage).

**Penetration (weapons)** — Bone Breaker (碎骨断筋: critical hits ignore critical-hit immunity), Hidden Vitals (寻隙刺要: sneak attack and precision damage ignore precision immunity).

### Defense

**Reactive Armor (2)**
- **Barbed Carapace (逆鳞铁刺)** — In armor, return 1d6 + armor bonus as untyped damage on melee hit.
- **Citadel of Steel (铸铁城阙)** — In medium/heavy armor, refresh temporary HP equal to armor bonus each round.

**Retaliation** — Riposte Reflex (截锋逆击: spend an attack of opportunity to strike back at melee attackers), Punish the Overreach (乘虚落刃: a missed melee attack against you triggers a free counter, once per round), Blood-Debt Reckoning (睚眦必报: bonus against enemies that attacked you this round), Read the Blade (明镜识锋: after a parry, the next attack is a guaranteed critical), Batter and Displace (撼步冲撞: bull rush after every attack you make or receive), Anvil's Retribution (铁砧反震: enemies that damage you take half your level plus Strength), Leviathan's Carapace (渊海甲胄: DR, and harmful effects are purged after taking a tenth of your maximum HP).

**Survival** — Undying (向死而生): when damage drops you to 0 HP or lower, you cannot die and keep acting for 1 hour; if you are not healed above the death threshold by then, you die.

### Magic

**Spell School Specialist (8) and Spell Descriptor Specialist (9)** — bonus caster level and DC for one school or descriptor group, with a matching penalty for the others.

- **School:** Aegis of the Pure Warder / 绝界镇魔使 (Abjuration / Wis), Sovereign Gatekeeper / 统界辟门者 (Conjuration / Cha), Eye of the Chronomancer / 溯时先知 (Divination / Int), Sovereign of the Heart / 倾心国主 (Enchantment / Cha), Pyre of the Architect / 灾变筑城师 (Evocation / Int), Phantasmagoria Maestro / 织影幻圣 (Illusion / Cha), Harvester of the Boneyard / 冥河渡魂人 (Necromancy / Wis), Sculptor of Prime Matter / 塑质造化使 (Transmutation / Int).
- **Descriptor:** Pyre of the Phoenix / 炽皇凤涅 (Fire / Cha), Stillness of the Glacial Void / 极渊玄冰 (Cold / Wis), Tempest-Dancer / 御雷疾影 (Electricity / Dex), Vitriolic Equation / 腐解算律 (Acid / Int), Herald of the Shattered Sky / 破霄神音 (Sonic / Cha), Axiom of Unseen Force / 虚空定则 (Force / Int), Fountain of Solar Dawn / 金阳圣晖 (Positive Energy / Cha), Vigil of the Gloom / 死寂枯荣 (Negative Energy / Wis), Puppeteer of the Mind / 惑魂主宰 (Mind-Affecting / Cha).

**Arcana** — Spell-Reaper's Tithe (萃魂蕴法: an enemy you damaged dies: regain a spell slot up to half its HD, once per round), Brimming Reservoir (盈渊射诀: cantrips deal extra damage from your remaining slot levels), Arcane Recirculation (灵脉返流: 15% chance to refund a spell slot), Spell-Shatter Edge and Spell-Well Collapse (断法绝流 / 枯泉引爆: weapon hits or spells burn an enemy caster's highest slot and deal twice its level, once per round).

**Penetration (spells)** — Mind Breaker (破心夺志: mind-affecting, charm, compulsion, emotion), Deathbringer (索命无赦: death), Dread Presence (慑魄凶威: fear), Plaguebearer (疫毒蚀骨: poison, disease), Overwhelming Force (镇岳缚形: paralysis, stun, daze, sleep): your effects of those kinds ignore immunity to them. Elemental Breach (破元裂障): creatures immune to an energy type take half your damage of that type instead, and are never healed by it.

### Summoning

**Greater Summoning (6)**

| Feat | 中文名称 | Attribute | Buffed Summons Stat |
|---|---|---|---|
| Behemoth's Heritage | 比蒙遗脉 | Strength | Strength |
| Zephyr's Covenant | 风灵疾契 | Dexterity | Dexterity & Speed |
| Titan's Lifespring | 巨怪生机 | Constitution | Constitution |
| Aegis of the Schema | 天元魔阵 | Intelligence | AC |
| Empathic Communion | 神契灵犀 | Wisdom | Fortitude, Reflex, Will saves |
| Dominator's Calling | 御统王令 | Charisma | Attack Bonus |

**Summoner Sacrifice (3)**
- **Martyr's Transference (形神替生)** — −4 Str/Dex/Con on yourself, +4 Str/Dex/Con on summons.
- **Eldritch Crucible (双生法炼)** — −2 Str/Dex/Con on yourself, +4 Str/Dex/Con on summons.
- **Tribute of Iron Dominion (夺冕化蛮)** — −4 Charisma on yourself, +8 Strength on summons.

**Summoner** — Summoner's Tether (契魂连理: healing is shared between you and nearby summons, without bouncing back), Swarm Caller (万灵应召: +1 summoned creature per 3 caster levels for every summoning spell or ability, including modded ones), Lingering Bond (契约长存: summons last 24 hours longer).

### Styles

**Stealth** — Vanishing Stroke (刃落无影): a kill, critical hit or sneak attack grants 1 round of greater invisibility, once per round.

**Solo** — Lone Wolf (孤鸿涉远: bonus to dodge AC, saves and damage while no ally is within 30 ft, growing with level) and Truly Solo (+1 to attributes, dodge AC, saves, attack and damage per joined companion who is not in the party, up to 5).

**Growth** — permanent, uncapped growth from kills: Stolen Grace (掠影炼魄: +1 Dex per 100 kills), Corpse-Forged Bulk (尸山淬躯: +1 Str per 100 party kills), Stolen Epiphany (掠识开慧: +1 Int per 10 caster kills), Siphoned Vitality (噬元固魄: +1 max HP per 10 kills), and Soul-Tether Harvest (拘魂蓄怨: souls from kills add damage, up to twice your level).

**Meme** (off by default; enable in settings and restart; free of budget points) — Man, What Can I Say (a speech bubble on kills and critical hits) and Nobody Knows It Better (+2 to a chosen skill and a speech bubble when you use it).

## Settings

> Applied immediately: Weapon Damage Mode, the feat budget, exclusion groups, Casting Scope/Mode and Truly Solo pets. Other settings require restarting. With [ModMenu](https://www.nexusmods.com/pathfinderwrathoftherighteous) installed, the same settings also appear on an AC's Homebrew page in the game's options screen; both write the same settings file.

| Setting | Default | Effect |
|---|---|---|
| Weapon Damage Mode (`WeaponDamage`) | `Replace` | Replace uses a better chosen attribute instead of the existing damage attribute; Add adds the positive modifier once. |
| Feat count limit (`EnableFeatCountLimit`, `MaxFeatCount`) | OFF, 6 | Maximum number of Homebrew feats per character. See [Feat Budget](#feat-budget). |
| Feat point limit (`EnableFeatPointLimit`, `MaxFeatPoints`) | OFF, 10 | Maximum total Homebrew feat cost per character. Can be enabled together with the count limit. |
| Exclusion groups (`EnableMutex`, `FeatGroups`) | ON | Master switch plus a switch and limit for each [exclusion group](#exclusion-groups). |
| Casting Scope (`CastingScope`) | `SelectedSpellbook` | Casting Attribute feats change the chosen spellbook, or all spellbooks. |
| Casting Mode (`CastingMode`) | `Always` | Use the feat's attribute always, or only when it is higher than the spellbook's own. |
| Truly Solo pets (`TrulySoloCountsPets`) | OFF | Pets in the party reduce the number of absent companions. |
| Meme feats (`EnableMemeFeats`) | OFF | Registers the Meme menu (restart required). |
| Power Level | `Balanced` | `Balanced`: full scaling for attributes, defenses, maneuvers, skills, caster level and spell penetration; reduced scaling for spell DC, BAB and Power Mode bonuses. `Legacy_AllFull`: all rank-based bonuses use full scaling. |
| Include Self in Attribute Stack | OFF | A retired broad Main feat may add its attribute to itself. |
| EnableAttributes, EnableDefenses, EnableManeuvers, EnableChecks, EnableSkills, EnableCasterDC, EnableCasterLevel, EnableSpellPenetration | ON | Gate the corresponding bonuses of the attribute and aptitude feats. |
| EnableBAB | OFF | Main Attribute Mastery also adds reduced scaling to Base Attack Bonus. |
| EnablePowerMode | OFF | Main Attribute Mastery also adds attack bonus, damage, AoOs, sneak attack, HP, speed and fixed +1 reach. Intentionally overpowered. |

## Feat Budget

The optional feat budget limits each character's Homebrew feats across all families. Enable a **count limit**, a **point limit**, or both, and set their maximums (0–99) in the mod settings. Changes apply on the next selection check, without restarting.

| Cost | Families |
|---|---|
| 3 | Retired broad Main feats |
| 2 | Main Attribute Mastery, Weapon Insight, Extended Replacement, Derived Stat Conversion, Weapon Damage, Casting Attribute, Stance, Greater Summoning, Summoner Sacrifice, Spell School/Descriptor Specialist, Arcana, Stealth, Solo, Survival, Penetration |
| 1 | Defensive, Maneuver, Skilled and Arcane aptitudes, Resource Attribute, Conditional Trigger, Distance Damage, Long-Reach Gambit, Momentum, Execution, Reactive Armor, Retaliation, Summoner, Growth |
| 0 | Meme |

- Only feats granted to the character count. Opening the AC's Homebrew menu or a family menu is free, and vanilla or other mods' feats are never counted.
- Each parametrized choice (for example each Weapon Damage weapon category or Casting Attribute spellbook) is a separate feat with its own cost. A feat with multiple ranks counts once per rank.
- Picks earlier in the same level-up count toward later picks. Cancelling or changing a pick frees its budget; later picks that no longer fit are removed from the level-up, as with other prerequisites. Respec frees all budget.
- Feat tooltips show used and maximum feats or points, the feat's cost and why it cannot be chosen. The mod settings list each party member's usage.
- Effects that ignore feat prerequisites, such as the Trickster's, do not bypass the budget.
- Lowering a limit or loading an older save never removes feats. A character above the limit keeps every feat and its effects, is shown as over budget, and cannot choose more Homebrew feats until the limit is raised or the character is respecced.

## Exclusion Groups

Each group limits how many of its feats one character may own. Every group has its own switch and limit in the settings; `EnableMutex` turns all of them off. Changes apply immediately.

| Group | Members | Default |
|---|---|---|
| Main Attribute Mastery | All Main feats, including retired ones | 1 |
| Defensive / Maneuver / Skilled / Arcane | Each aptitude family | 1 each |
| Stance, Weapon Insight, Weapon Damage attribute, Greater Summoning, Summoner Sacrifice, Spell School, Spell Descriptor, Reactive Armor | Each family | 1 each |
| Conditional Trigger | The six triggers | 2 |
| Attribute replaces AC / CMD | Ascetic's Ward + Zephyr's Grace / Monarch's Stature + Titan's Footing | 1 each |
| Derived AC; Caster level to HP | Arcane Aegis + Scholar's Positioning; Ley-Infused Vitality + Dawn of the Soul | 1 each |
| Attribute bonuses to AC | AC aptitudes, AC stances, AC replacements and Defiance at the Precipice | 2 |
| One distance style | Close, mid and long feats exclude each other; feats within a style stack | ON |
| Same attribute across Main, aptitudes and stances | One feat keyed to each attribute | OFF |
| Casting and resource attribute | One feat per spellbook and per resource | Always on |

Main Attribute Mastery uses **inherent** bonuses, so it does not stack with tomes and wishes; most other bonuses are untyped or replace an attribute.

## Installation (Unity Mod Manager)

1. Install [Unity Mod Manager](https://www.nexusmods.com/site/mods/21) for Pathfinder: Wrath of the Righteous.
2. Build or download `ACHomebrew-0.2.1.zip`.
3. Drop the zip into UMM.
4. Enable the mod in-game.

## Save Compatibility

- **Upgrading to AC's Homebrew** keeps every existing feat GUID, the UMM Id and the install folder. The six broad Main feats are retired but stay on characters that have them, with narrowed effects; respec to pick the new Main Attribute Mastery. Several families were narrowed (aptitudes, the Wisdom stance, Defiance at the Precipice, Blade of the Spell-Saint, Summoner Sacrifice) and the distance feats were rebuilt; existing characters get the new effects on load. Exclusion rules are now checked at selection time and no longer stored on blueprints.
- Settings files without budget, exclusion group, casting or meme fields use the defaults. Settings files without `WeaponDamage` default to `Replace`; both damage modes use the same new feat GUIDs, so switching modes requires no respec.
- **0.1.1 → 0.1.x is non-breaking.** Settings carry over; XML serialization adds the new `EnableMutex` field as `true` by default.
- **0.1.0 → 0.1.1 upgrades** keep all existing feats (GUIDs unchanged). New feats appear in the level-up feat list and Commanding Presence Stance now has its 30-ft ally aura wired; its Charisma scaling source still requires in-game verification.
- 0.0.x → 0.1.x is a redesign; back up saves first.

## Building from Source

```powershell
git clone --no-recurse-submodules https://github.com/Acture/ac-homebrew.git
cd ac-homebrew
```

- Set `WrathInstallDir`, `WrathPath`, or `WRATH_PATH`, or let the project generate the ignored, repository-root `GamePath.props` from `Player.log`.
- From the repository root, run `dotnet build ACHomebrew.slnx -p:DeployMod=false` to compile without deploying to the game.
- To build the release ZIP without deploying to the game, run `dotnet build ACHomebrew.slnx -c Release -p:DeployToGame=false`.
- Build output is in `artifacts/bin/ACHomebrew/<Configuration>/`; intermediate files are in `artifacts/obj/`.
- To deploy, run `dotnet build ACHomebrew.slnx`. The Deploy target copies files into the local UMM mod folder and creates `artifacts/packages/ACHomebrew-<Version>.zip`.
- Run the game-independent repository checks with `pwsh -NoProfile -File scripts/Test-RepositoryContracts.ps1`. These check blueprint IDs and loader metadata; they do not test combat effects.

- Run the standalone damage calculation checks with the .NET 10 SDK: `dotnet run --project tests/WeaponDamage.Tests`.
- Run initialization failure checks with `dotnet run --project tests/Initialization.Tests`. These exercise the real registry and menu orchestration with stand-ins for game/BlueprintCore APIs and family creation; they do not start Unity or verify in-game UI behavior.
- With Windows PowerShell 5.1, check settings compatibility using `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tests/VerifySettings.ps1 -WrathInstallDir "<game directory>"`.
- After compiling, verify live mode switching on an existing component with `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tests/VerifyLiveWeaponDamageMode.ps1 -WrathInstallDir "<game directory>"`. This loads the compiled mod and game types without starting Unity.
- With a local game installation, run `pwsh -NoProfile -File scripts/Invoke-OfflineMechanicsTests.ps1`. It uses the public [wotr-testing](https://github.com/Acture/wotr-testing) NuGet package `WotR.Testing.Offline`, loads the real game assemblies and blueprint pack on the game's own Unity Mono runtime without starting the game, and applies feats to vanilla units. See [offline mechanics tests](docs/offline-mechanics-testing.md) for inputs, adaptations and limits.

GitHub Actions runs the repository checks on Windows and Linux. GitHub-managed
CodeQL default setup scans C# and Actions for security issues; review its results
on the latest pull-request commit before merging. These static checks do not
replace in-game behavior tests.

### Repository layout

| Path | Contents |
|---|---|
| `src/ACHomebrew/` | Entry project: UMM loader, registration order, icons, `App.config`, loader `Info.json`, optional ModMenu page and packaging (merges every project and BlueprintCore into `ACHomebrew.dll`) |
| `src/ACHomebrew.Logic/` | Game-independent logic shared with the tests: budget costs, exclusion groups, scoring rules and blueprint GUIDs |
| `src/ACHomebrew.Core/` | In-game infrastructure: feat menus, budget and exclusion enforcement, settings, localization and shared feat builders |
| `src/ACHomebrew.Attributes/` | Main Attribute Mastery, Weapon Insight and Extended Replacement, Derived Stat Conversion, Weapon Damage, casting and resource attributes |
| `src/ACHomebrew.Aptitudes/` | Defensive, Maneuver, Skilled and Arcane aptitudes |
| `src/ACHomebrew.Martial/` | Stances, Conditional Triggers, distance feats, Long-Reach Gambit, Momentum and Execution |
| `src/ACHomebrew.Defense/` | Reactive Armor and Retaliation |
| `src/ACHomebrew.Magic/` | Spell School/Descriptor Specialists and Arcana (spell slots) |
| `src/ACHomebrew.Summoning/` | Greater Summoning, Summoner Sacrifice and Summoner links |
| `src/ACHomebrew.Styles/` | Stealth, Solo and Growth |
| `src/WrathMod.props` | Shared game references and `WrathInstallDir` resolution |
| `tests/` | Test projects and compatibility baselines |
| `scripts/`, `.github/` | Local commands and CI workflows |
| [docs/](docs/README.md) | Public documentation |
| `artifacts/` | Ignored build outputs, intermediate files, packages and test reports |
| `vendor/wotr/` | Optional ignored snapshot of game files for offline tests; never committed |

The root keeps `ACHomebrew.slnx`, shared build configuration, repository
configuration, README, CHANGELOG and `Repository.json`. The local `GamePath.props`
is shared by the Mod and test projects and must not be committed.

## Text and Icons

Every feat and selection menu has names and descriptions in all eight game languages: English and Simplified Chinese, plus German, French, Spanish, Italian, Brazilian Portuguese and Russian (unreviewed machine translations; corrections welcome). The original 92 feats and 60 feats added in AC's Homebrew have individual 128×128 PNG icons ([contact sheet](docs/feat-icons.png)). Main Attribute Mastery feats reuse their source attribute's icon; the six Weapon Damage feats do not yet have bespoke artwork. Names, lore and complete rule templates live in one JSON file per language under [src/ACHomebrew.Core/Localization](src/ACHomebrew.Core/Localization), starting with [enGB.json](src/ACHomebrew.Core/Localization/enGB.json). These canonical files are both embedded and shipped in `Localization/`; the loader prefers valid external translations and logs fallbacks. Edit the installed files and restart—no recompilation is needed. See [localization and release delivery](docs/localization-and-release.md).

The release ZIP also includes [English](docs/player/README.en.md) and
[Simplified Chinese](docs/player/README.zh-CN.md) player readmes, versioned
changelogs and both license files. These player guides explain installation,
upgrades, settings, known issues and feedback without contributor-only material.

See the [icon contact sheet](docs/feat-icons.png), [asset manifest](docs/icon-manifest.json), and [validation commands and limits](docs/validation.md). The settings UI remains in English. Source checks and builds do not establish actual combat effects or in-game text layout.

## Changelog

See [CHANGELOG.md](./CHANGELOG.md).

## Documentation

See [docs/](docs/README.md) for usage guides, the feat catalog, and validation
instructions. Release changes are recorded in [CHANGELOG.md](CHANGELOG.md).

## Credits

Thanks to @CasDragon for code snippets and ideas. AC's Homebrew (formerly AttributeFeats) grew out of earlier Redditor class-feat experiments and was rebuilt for the 0.1.0 build-enabler release.

## License

[GNU Affero General Public License v3.0](LICENSE), with an
[additional permission](LICENSE-EXCEPTION.md) to link and convey it together with
the proprietary game and Unity engine assemblies.
