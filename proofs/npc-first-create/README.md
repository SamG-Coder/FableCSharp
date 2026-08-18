# First non-Hero NPC / creature create after Leave

Investigation only. Production `src/` was not edited.

Do **not** start at Oakvale / `00DBDE40` / `CREATURE_HERO_CHILD` /
`NOVI_LiveFather` / script `Create CREATURE_OAKVALE_*`.
Do **not** treat first `0051FD80` (`TRACK_NODE_BASIC`) or Hero
`006AC910` as this answer.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Question: after Leave Frontend, what is the first *non-Hero*
NPC / creature that is actually *created*? Is that a TNG
`NewThing AICreature` (`0051FD80`) or a quest / script factory
(`00CB5AD0` / `00CB8230` / `Create` `00CCC246`)?

Sources: `src/Fable.Formats/Tng/ThingFile.cs`;
`src/Fable.Game/EngineLifecycle.cs` (`LoadSingleThing`,
`ThingTypeRegistrarFn`, `AllocateClassFn`,
`PlayerCreatureFactoryFn`);
`QuestFactoryTable.cs` / `ScriptFactoryTable.cs`;
`docs/runtime/FORWARD_TREE.md` §§9–10;
`docs/status/investigations/2026-08-18-first-scene-things.md`
(+ dump);
Anniversary loose `LookoutPoint.tng` (UID set matches TLC dump);
listings `0051FD80` / `00522A20` / `005272E0` / `00831F80` /
`00CD5FCF` / `00D35090` / `00CCC246`;
`proofs/tng-first-def`, `tng-spawn`, `thing-manager-activate`,
`script-factory-tables`, `newgame-script`, `player-bind-world`;
`EngineLifecycleTests.Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`;
`GameBinFormatTests` mesh **5149**.

---

## Verdict

**TNG.** First non-Hero creature after Leave is

**`CREATURE_BS_VILLAGER_MALE` / `ScriptName=FH_Villager` /
`ScriptData="0"` / UID `18446741874686300555`**

from `LookoutPoint.tng` section **`Q_FireHeart`**, constructed
by `0051FD80` → `00A371C0` Allocate Class → kind factory
**`005272E0`** → ctor **`00831F80`** (`CThingAICreature` vtbl
`0127293C`, size `0x1D8`). Navigator pose
**(124.357, 81.083, 30.100)**. Graphic mesh **5149**.

It is **not** a quest-factory object. It is **not** script
`Create`. It is **not** `LookoutPointBeggar`. It is **not**
Hero.

| Claim | Status |
|---|---|
| First non-Hero NPC create is TNG `0051FD80` | **PROVEN** |
| Def / script / UID / section as above | **PROVEN** (Anniversary file order; TLC dump UIDs MATCH) |
| Kind factory is `005272E0`, not Hero `0052B880` | **PROVEN** |
| First create is quest `00CB5AD0` / `Q_SunnyvaleMaster` | **DISPROVEN** (quest object, not `CThingAICreature`) |
| First create is `00CB8230` `FH_Villager` (`00D35090`) | **DISPROVEN** (that registrar runs only if `Q_FireHeart` / `S_QFHT` is constructed; Leave does not) |
| First create is script `Create` `00CCC246` / `008A9100` | **DISPROVEN** (no `00CBFB7D` after Leave) |
| First create is `CREATURE_BEGGAR_01` / `LookoutPointBeggar` | **DISPROVEN** (later section `V_BeggarAndChild`) |
| First create is Hero / kid / father | **DISPROVEN** |
| Bridge / Gameflow / NULL contain an `AICreature` | **DISPROVEN** |
| Host first-Present C3D of this villager | **DISPROVEN** (`Submitted=false`; dump used `CTCPhysicsStandard` only) |

---

## Path from Leave (no-save New Game)

```
0042F2A2  Leave frontend
0042F491  Init Game → 004184BD
  Init World 004A6E30
    00CB5D80 Registering Scripts
      00CD52D0  161× 00CB5C90                  // BIND only
        includes Q_FireHeart / S_QFHT / 00D3AFC0
        includes Q_NewOakValeIntro / S_QNOVI     // also BIND only
  00416953  Loading world FinalAlbion.wld
    0050959F  .gtng miss
    004FDBC0  proximity .tng parse              // no 0051FD80
    0049F180  Init Characters                   // no NPC
    004B4260  WLD START_INITIAL_QUESTS
      Q_SunnyvaleMaster … CS_PlayCutscene       // quest factories
      NOT Q_FireHeart, NOT Q_NewOakValeIntro
    user.ini ActivateQuest("Gameflow")          // 7th quest
004189C2  dummy pumps  index 0
later 00501450 → 00500540(1,0,0) LookoutPoint
  006C2170 Loading objects
    ContainsMap[0] BowerstoneBridge   88  0 AICreature
      first 0051FD80 TRACK_NODE_BASIC           // not a creature
    ContainsMap[1] LookoutPoint       288
      Gameflow  2× MARKER_BASIC  (M_Maze, M_LadyGameflow)
      NULL      252  (no AICreature)
      Q_FireHeart  3× AICreature
        0051FD80  CREATURE_BS_VILLAGER_MALE  FH_Villager "0"   ← first NPC
        0051FD80  CREATURE_BS_VILLAGER_FEMALE FH_Villager "2"
        0051FD80  CREATURE_BS_VILLAGER_MALE  FH_Villager "1"
      Q_GuildTraining  10  (no AICreature in this first-NPC question)
      Q_WaspBoss       FleeingWoman, WaspHelper
      V_BeggarAndChild BeggarBully, LookoutPointBeggar
      V_SickChild_Activate TalkingTrader2, TalkingTrader1
    ContainsMap[2] GuildExterior      88  6 AICreature (later)
    006AC910 CREATURE_HERO at GuildArrivalHSP               // after maps
```

`00D35090` (`00CB8230` `FH_Villager` / factory `00D373D0`) is
**not** on this list. **PROVEN** as a later `S_QFHT` name table,
same pattern as leftover `00DABAC0` / `NOVI_LiveFather`.

---

## TNG vs factory (do not collapse)

Two different “factory” words.

### A. TNG kind factory — **this create**

`00522A20` (`ThingTypeRegistrarFn`) at Create Players time:

| Kind string | Family | Alloc | Ctor / vtbl |
|---|---|---|---|
| `PlayerCreature` | (hero) | **`0052B880`** size `0x208` → `0052AB20` | `006AC910` path |
| **`AICreature`** | **`CREATURE`** | **`005272E0`** size **`0x1D8`** → **`00831F80`** | vtbl **`0127293C`** |

`0051FD80` non-`PlayerCreature`: `"Load Single Thing: Allocate Class"`
→ `00A371C0` → `call ecx` of that kind row.

Lookout TNG has **no** `PlayerCreature` / `CREATURE_HERO`.
First `AICreature` therefore takes **`005272E0`**, not `0052B880`.

`004CA010` / `00662880` insert the Thing. `CThingAICreature`
vtbl+40 is `004C97B0` (phase bump on `+148`, component walk).
That is **activate**, not allocate. Host `ScriptFactoryTable.ThingConstruct`
naming that VA is **LEFTOVER** vs this first NPC (Oakvale
`NOVI_LiveFather` comments).

### B. Quest factory table — **not this create**

`00CD52D0` / `00CB5C90` fills 161 quest rows at Init World.

`Q_FireHeart` row (listing `00CD5FCF`):

| Field | Value |
|---|---|
| Quest | `Q_FireHeart` |
| Script | `S_QFHT` |
| Factory | `00D3AFC0` |
| Persist | 0 (`bl`) |

`004B4260` / `00CB5AD0` after Leave constructs only the WLD
`START_INITIAL_QUESTS` six plus `user.ini` `Gameflow`.
`Q_FireHeart` is **not** in that seven. QST persist is **False**.
`QuestFactoryTable.Recovered` has no `Q_FireHeart` row — correct
omit.

Those seven constructs are quest / watcher objects
(`00CDBD20` shared run), **not** `CThingAICreature`.

### C. TNG script-name table — **later bind, not this create**

`00D35090` is the `S_QFHT` analogue of `00DABAC0`:
`00CB8230("FH_Villager")` with factory at `+16` = `00D373D0`
(also `FH_Scythe`, `FH_VillagerEscapeMarker`, …).

It runs only if `Q_FireHeart` is constructed. Leave does not.
So first `0051FD80` of `FH_Villager` happens **without** that
name in `00CB8960`. Whether `004C97B0` / `00CB8960` then miss
is **PARTIAL**.

### D. Script `Create` command — **not this create**

`00CCC246` token `"Create"` → apply `00CCC3E6` `vtbl+364`
`008A9100` (def lookup `009AD410` / `00513160` then
`00833800`). Needs runner `00CBFB7D`. First no-save pumps
never enter it (`proofs/script-entity-cmds`,
`FirstSeenCallsPlayAnimationDispatcher=false`).

Oakvale leftover
`Create CREATURE_OAKVALE_VILLAGER_FEMALE_NORMAL_MESH,…`
is **DISPROVEN** as first NPC.

---

## File order (Lookout `AICreature` ×9)

Anniversary `LookoutPoint.tng` (version 2). TLC dump UIDs /
defs / scripts MATCH this list. Bridge has **0** `AICreature`.

| # | Section | Def | Script | Data | UID (low) |
|--:|---|---|---|---|---|
| **1** | **`Q_FireHeart`** | **`CREATURE_BS_VILLAGER_MALE`** | **`FH_Villager`** | **`0`** | **300555** |
| 2 | `Q_FireHeart` | `CREATURE_BS_VILLAGER_FEMALE` | `FH_Villager` | `2` | 300554 |
| 3 | `Q_FireHeart` | `CREATURE_BS_VILLAGER_MALE` | `FH_Villager` | `1` | 300549 |
| 4 | `Q_WaspBoss` | `CREATURE_BS_VILLAGER_FEMALE` | `FleeingWoman` | | 296835 |
| 5 | `Q_WaspBoss` | `CREATURE_BS_VILLAGER_MALE` | `WaspHelper` | | 296870 |
| 6 | `V_BeggarAndChild` | `CREATURE_BS_VILLAGER_BULLY_MALE` | `BeggarBully` | | 296448 |
| 7 | `V_BeggarAndChild` | `CREATURE_BEGGAR_01` | `LookoutPointBeggar` | | 296088 |
| 8 | `V_SickChild_Activate` | `CREATURE_TRADER_02` | `TalkingTrader2` | | 300231 |
| 9 | `V_SickChild_Activate` | `CREATURE_TRADER_01` | `TalkingTrader1` | | 300230 |

`Q_FireHeart` census **3** is exactly these three villagers.
Gameflow is two markers (`M_Maze`, `M_LadyGameflow`). NULL
starts at `MARKER_BASIC` `VC`. No sort-by-kind before
`0051FD80` (`tng-first-def`).

GuildExterior’s first `AICreature` is later
`CREATURE_RIVAL_HERO_SCYTHE` / `FH_Scythe` (same
`00D35090` name family, still after Lookout’s nine).

---

## Pose / draw

First NPC uses **`StartCTCPhysicsNavigator`**, not
`CTCPhysicsStandard`. Host dump `pos=-` / `Submitted=false`
is that field pick, not a failed `0051FD80`. Native still
constructs. Who first writes a world graphic pose is
**UNREAD** (same leftover as `2026-08-18-first-scene-things`
§7). Do not invent `MK_BB_*` teleports as first-Present C3Ds.

Mesh **5149** (`GameBin.FindMeshId`). First-Present submitted
creature C3D is still Hero **4299** only.

---

## Host

`LoadRegionMapThings` walks `ThingFile.Things` (section /
`NewThing` order) and `LoadSingleThing` every instance,
including `Kind=AICreature`. It does **not** special-case
`005272E0`. Hero is appended after the three maps.

`QuestFactoryTable` / `ActivateNamedQuest` never constructs
`FH_Villager`. `ScriptFactoryTable.Recovered` is Oakvale
`NOVI_LiveFather` — **LEFTOVER** vs this NPC.
`ScriptRuntime.StartNewGame` `Create` lines **DIVERGE**.

---

## Not these

| Candidate | Why not first non-Hero NPC |
|---|---|
| `TRACK_NODE_BASIC` `GuardTrack` | first `0051FD80`, not a creature |
| `OBJECT_STREETLAMP_LIT_SINGLE_01` | first Object-kind |
| `004AE9D0` / Create Players slots | not a Thing |
| `Q_SunnyvaleMaster` `00CDD550` | quest factory object |
| `CS_PlayCutscene` `00F01760` | empty factory |
| `LookoutPointBeggar` | 7th Lookout `AICreature` |
| `CREATURE_HERO` `006AC910` | after all ContainsMap TNG |
| `CREATURE_HERO_CHILD` / Oakvale `Create` | **DISPROVEN** tree |
| Global `004FDBC0` first prox Lookout | parse only; no `0051FD80` |

---

## UNREAD / PARTIAL

- Live `0051FD80` miss (`004C9B80`) on first `AICreature`.
- TLC WAD vs Anniversary TNG *bytes* (UID/def/script MATCH is
  already **PROVEN** via the census dump).
- Whether `00CB8960` runs on `FH_Villager` at first
  `004C97B0` (name table empty).
- First-seen draw / navigator → world pose.
- Who later `00CB5AD0`s `Q_FireHeart` so `00D35090` can run.
