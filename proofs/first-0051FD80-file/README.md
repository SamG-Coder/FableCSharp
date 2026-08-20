# First `0051FD80` construct file after dummy pumps

Investigation only. Production `src/` was not edited.

Do **not** start at Oakvale / `00DBDE40` / `StartOakVale` /
`StartOakValeWest` / `CREATURE_HERO_CHILD`. No-save New Game is
message **15** → Leave `0042F2A2` → `FinalAlbion.wld`.

Do **not** collapse first **TNG open** (`LookoutPoint.tng` inside
`00507C30`) with first **Load Single Thing** construct.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER**.

Question: after dummy `004189C2` pumps, what is the first
`0051FD80` **file**? Confirm ContainsMap order. When is
LookoutPoint **constructed** vs only **parsed** in `00507C30`?
Where is `GuildArrivalHSP` constructed?

Authority: dump `00501450` / `0051FD80`;
`proofs/tng-first-after-leave`, `proofs/wld-first-region`
(first *region name* after Leave — there is no
`proofs/first-region-after-leave`); siblings
`tng-after-leave`, `tng-first-def`, `tng-spawn`,
`thing-manager-activate`, `load-job`, `creature-after-leave`;
`docs/status/investigations/2026-08-18-first-scene-things.md`
(+ dump); listings `listing-00500000.txt` / `listing-006c0000.txt`;
`EngineLifecycle.LoadFromFirstRealRegion` / `ApplyLoadJob` /
`LoadRegionMapThings` / `LoadSingleThing` /
`SpawnHeroFromPlayerStart`.

---

## Verdict

**After dummy pumps the first `0051FD80` file is
`BowerstoneBridge.tng`.** First `NewThing` in that file is
**`TRACK_NODE_BASIC` / `GuardTrack`.**

`00501450`’s first real job is **region** LookoutPoint
(`00500540(1,0,0)`). The job’s object pass walks WLD
`ContainsMap` **file** order, not the region name.

LookoutPoint in `00507C30` is **parse only** (WLD tokens +
`004FDBC0` first `.tng` open). Its CThings are constructed
later as **ContainsMap[1]**.

`GuildArrivalHSP` is a Lookout `HOLY_SITE_PLAYER_START`
constructed by that second `0051FD80` pass. Hero `006AC910`
is **not** the HSP construct.

| Claim | Class |
|---|---|
| Dummy pumps construct no Things | **PROVEN** |
| First post-dummy `0051FD80` file is `BowerstoneBridge.tng` | **PROVEN** |
| First construct is `TRACK_NODE_BASIC` `GuardTrack` | **PROVEN** (def/script; UID/pos **PARTIAL** vs TLC WAD) |
| File order after dummy: Bridge → Lookout → Guild | **PROVEN** |
| Lookout is constructed in `00507C30` | **DISPROVEN** |
| Lookout WLD + TNG **parse** is in `00507C30` | **PROVEN** |
| First construct file is Lookout / Oakvale | **DISPROVEN** |
| `GuildArrivalHSP` construct site is Lookout `0051FD80` | **PROVEN** |
| `GuildArrivalHSP` is created by `006AC910` | **DISPROVEN** (that is Hero) |

---

## Path (no-save New Game)

```
0042F2A2  Leave frontend                         // no TNG
00416953  Loading world  FinalAlbion.wld
  004A1840
    00507C30  Load .wld file                     // PARSE only
      NewMap 1     LevelScriptName LookoutPoint  // Maps[0]
      NewRegion 1  RegionName LookoutPoint       // Regions[0]
        ContainsMap BowerstoneBridge.lev
        ContainsMap LookoutPoint.lev
        ContainsMap GuildExterior.lev
      NewRegion 4  StartOakVale                  // later; not this tree
      0050959F  stem+.gtng  TLC miss             PROVEN skip
      00509859  Load global things
        [0x13B8609]==0 → 004FDBC0                ← first .tng OPEN
          skip slot 0; first hit Maps[0]
          LookoutPoint.tng  parse NewThing
            MARKER_BASIC M_Maze                  // not 0051FD80
          005223F0  [manager+128]==1?            UNREAD live
            host: GlobalThings concat, no 0051FD80
      00509982  region graph
    004A1BD3  Set Static Map                     // after parse
    0049F180  Init Characters                    // no 0051FD80
004189C2  dummy pumps  [WorldMap+156]=0
  no 006C2170 / 0 things                         PROVEN
later 00501450                                   E8 caller UNREAD
  00449970 / 00487DC0
  004FEEC0(saved,0)  +156=0
  count=(+48−+44)/88  (142)
  count>1: 00500540(i,0,0)  i=1..141
    first i=1  LookoutPoint                      // REGION job
      006C27A0 / 006C2120 / 006C2710
      006C2170
        pass 1  Loading topology  ×3
        pass 2  Loading objects   ContainsMap
          [0] BowerstoneBridge.tng  88
            00522720 / 00521AE0 / 00520D00
            0051FD80  TRACK_NODE_BASIC GuardTrack  ← first
            ×8 TRACK_NODE_BASIC
            then OBJECT_STREETLAMP_LIT_SINGLE_01
          [1] LookoutPoint.tng  288              ← Lookout CONSTRUCT
            first: MARKER_BASIC M_Maze
            later: HOLY_SITE GuildArrivalHSP     ← HSP CONSTRUCT
          [2] GuildExterior.tng  88
        006C2470  0051E2F0 Activate Things
      no PlayerCreature in those files
      006AC910  CREATURE_HERO at GuildArrivalHSP pose
  … i=2..141 then RegionGraph.txt
  00500540(saved,0,1)  restore, no pump
```

`Q_NewOakValeIntro` / `00DBDE40` never open or construct a TNG
on this walk. **PROVEN**.

---

## 1. Dump `00501450` — region loop, not a file opener

`listing-00500000.txt` `00501450`…`00501985` `ret`:

```
00501450  ecx = WorldMap
          00449970 / 00487DC0           player Thing (miss ok)
005014A3  004FEEC0(current, 0)          +156=0 dummy
          count = (+48−+44) * 0x2E8BA2E9  // /88
005014CA  cmp ecx, 1
          jbe 005018F8                  // skip if only dummy
005014EC  call 00500540(i, 0, 0)        // i starts 1
          0048D400 / 005198B0           collectors
005018D8  inc esi / add ebx, 88
005018EA  jb 005014E3                   // i=2..count-1
005018F8  push 0x124467C                // RegionGraph.txt
00501935  call 00500540(saved, 0, 1)    // no sync pump
```

First taken `00500540` is **native index 1 = LookoutPoint**
(`wld-first-region`). That is the **job**, not the first
`.tng` construct.

`00501450` does **not** `E8 0051FD80`. It does not name
`BowerstoneBridge`. File apply is inside `00500540` →
`006C27A0` → `006C2710` → `006C2170`.

E8 caller of `00501450` is still **UNREAD**. Not first
`004189C2`. Host `EnqueueAfterDummy` on second `Pump` is
**DISPROVEN**. Tests call `LoadFromFirstRealRegion` after
dummy pumps.

The loop continues `i=2…141` (`Filler_NorthernWastes_02`).
Those later jobs are **not** the first `0051FD80`.

---

## 2. Dump `0051FD80` — construct one `NewThing`

`listing-00500000.txt` `0051FD80` (`ret 8` from the
`00520D00` site):

```
0051FDA6  push "Load Single Thing 1"
          esi+24 in {2,3} → def stream 004C81F0
          else tokenizer 009BA330
0051FE7F  push "EndThing"
0051FECC  push "EndThing;"
0051FEF1  push "Load Single Thing 2"
0051FF1B  push "NULL"                 // default DefinitionType
          00528760  def lookup
          [world+258] && "PlayerCreature" → bind, not alloc
          else:
00520114  "Load Single Thing: Allocate Class"   00A371C0
00520159  "Load Single Thing: Construct Thing"
005201BA  "Load Single Thing: Initial Activate"
00520246  "Load Single Thing 3"
```

`0051FD80` does **not** `00999230` / `0099AD80` a path.
Open is `00522720` / `00521AE0` (region) or `004FAFF0`
(global). This note is the **construct**.

Region-walk caller (`00520D00`):

```
00520F13  push "NewThing"
00520F44  push "Loading entities from script"
00520F9A  call 0051FD80
          eax≠0 → push pointer into job vector
```

`006C2170` pass 2 (`listing-006c0000.txt`):

```
006C22F1  push "Loading objects"
006C232D  call 00522720               // map id from job+16
          push 3
006C2368  call 00521AE0               // Thing Manager: Load From File
          add edi, 28                 // next ContainsMap record
```

Job map vector is WLD `ContainsMap` copy (`006C2D40`,
stride 28). Lookout’s three names in **file bytes**:

```
ContainsMap BowerstoneBridge.lev
ContainsMap LookoutPoint.lev
ContainsMap GuildExterior.lev
```

Dump: 88 + 288 + 88. Host `ActivatedMaps` same order.
SeesMap / Picnic / Greatwood are **not** this pass.

---

## 3. First Load Single Thing files after dummy

Dummy `004189C2` `[WorldMap+156]=0`: `005066E0` empty slot,
no `006C2170`, `HeroSpawned=false`, 0 Things. **PROVEN**.

First construct files (i=1 job only):

| # | File | Things | First `0051FD80` |
|---|---|---|---|
| 1 | `BowerstoneBridge.tng` | 88 | `TRACK_NODE_BASIC` `GuardTrack` |
| 2 | `LookoutPoint.tng` | 288 | `MARKER_BASIC` `M_Maze` |
| 3 | `GuildExterior.tng` | 88 | first `NewThing` of that file |

Bridge census (dump): Object 67, TrackNode **8**, Marker 6,
Thing 4, Building 2, Holy Site 1. Sections
`NULL:85, QR_EscortTrader:1, Q_FireHeart:1, Q_WaspBoss:1`.
The eight `TRACK_NODE_BASIC` are the first eight `EndThing`s.
No sort-by-kind on `006C2170` pass 2.

Anniversary first block (UID/pos **PARTIAL** vs TLC WAD;
def/script/kind **PROVEN**):

```
NewThing TrackNode
  DefinitionType "TRACK_NODE_BASIC"
  ScriptName     GuardTrack
  UID            18446741874686299399
  pos            (76.686, 30.849, 17.517)
```

Then ×8 track nodes, then first Object
`OBJECT_STREETLAMP_LIT_SINGLE_01` (mesh 4978). Type name
`THING_TYPE_TRACK_NODE` `004C76A5`. Not a first-Present C3D
(`FirstSeenInstancesAsC3d` false).

Host `LoadRegionMapThings` walks `tng.Things` in file
`NewThing` order and Notes `Load Single Thing 2` + def.
First note after dummy is that Bridge track node.

`tng-first-after-leave` is the **open** (`LookoutPoint.tng`
inside `004FDBC0`). This note is the later **construct**.

---

## 4. LookoutPoint: parsed in `00507C30`, constructed later

`00507C30` (`"Load .wld file"` `00507C9A`) is world
`vtbl+8` `0049E220` after Startup WAD. Token switch includes
`NewMap` (`00507F0A`) and `NewRegion` (`00507FD2`).

| In `00507C30` | What happens to Lookout |
|---|---|
| `NewMap 1` / `LevelScriptName` | table slot 1 = Lookout. **Parse.** |
| `NewRegion 1` / `RegionName` | first authored region. **Parse.** |
| `ContainsMap` three stems | stored on the region. **Parse.** |
| `0050959F` `.gtng` | miss. No construct. |
| `004FDBC0` first prox hit | **opens** `LookoutPoint.tng`, walks `NewThing`. Host: `GlobalThings` only. |
| `005223F0` | construct **only if** `[manager+128]==1`. Live take **UNREAD**. Host skip **PROVEN**. First-seen `00416392==0` after this walk is **PARTIAL** evidence the countable list stayed empty. |

So first-seen New Game: Lookout is **only parsed** here.
No `0051FD80` on the host path. Native first `0051FD80`
during this function stays **UNREAD** (would be `M_Maze`,
still **before** dummy / `00501450`).

Construct of Lookout CThings is
`00501450` → `00500540(1,0,0)` → `006C2170` Loading objects
**ContainsMap[1]** — after 88 Bridge constructs.

FORWARD_TREE “LookoutPoint first” is the **region name**
(`wld-first-region`). It is **not** first construct file.

---

## 5. `GuildArrivalHSP` construct site

Lookout TNG has **three** `NewThing Holy Site`
`HOLY_SITE_PLAYER_START`:

| Script | Pos | Role |
|---|---|---|
| **`GuildArrivalHSP`** | (52.688, 69.597, 36.982) fwd +X / up +Z | no-save start |
| `LookoutPointHSP` | ~102.8, 74.1 | not this start |
| `MAIN_START_POSITION` | ~102.9, 74.1 | not this start |

**Construct** of the HSP Thing:

```
00501450 i=1
  006C2170 Loading objects LookoutPoint.tng     // file 2
    0051FD80  DefinitionType HOLY_SITE_PLAYER_START
              ScriptName     GuildArrivalHSP
      00A371C0 Allocate Class
      0052AC90 HOLY_SITE factory
      004CA010 insert
```

Not first `NewThing` in that file (Gameflow is two
`MARKER_BASIC`). Not in Bridge (Bridge has a **different**
holy site, after its eight track nodes). Not in
`00507C30` as a CThing (parse only). Not `006AC910`.

**Use** after all three ContainsMap files (Lookout TNG has
**no** `PlayerCreature`):

```
0049F180  Init Characters          // post-apply; first-seen miss was earlier
00449D90  PLAYER_HERO miss
00489D40  00488B20 finds GuildArrivalHSP
006AC910  CREATURE_HERO ScriptName=Hero  mesh 4299
          pose copied from that HSP
```

Host `SpawnHeroFromPlayerStart` prefers script
`GuildArrivalHSP`, then any `HOLY_SITE_PLAYER_START`.
`FirstSceneMapName=LookoutPoint`. `ThingsForMap("LookoutPoint")`
holds both the HSP and the appended Hero.

`006AC910` constructs **Hero**, not the marker.

---

## Not these

| Candidate | Why not first post-dummy `0051FD80` file |
|---|---|
| `LookoutPoint.tng` | first **open** (`004FDBC0`); second **construct** file |
| `StartOakValeWest.tng` / `00DBDE40` | `NewRegion 4`; not this tree |
| `PicnicArea` / Greatwood / Guild Woods | SeesMap / later prox; not `006C2170` |
| `FinalAlbion.gtng` | missing; `0050959F` skip |
| `FinalAlbion.gtg` | `[0x13B8609]=0` unused |
| Dummy index 0 | no objects pass |
| `GuildArrivalHSP` / `CREATURE_HERO` | later in file 2 / after file 3 |
| `CREATURE_BS_VILLAGER_MALE` | first creature; Lookout `Q_FireHeart` |
| `MARKER_BASIC` `M_Maze` | first *parsed* type; first Lookout construct, not first overall |

---

## Host vs native

| Site | Host | Native |
|---|---|---|
| Dummy pumps | 0 things | same |
| `00501450` | `LoadFromFirstRealRegion` after dummy | body **PROVEN**; E8 **UNREAD** |
| First job | `00500540(1,0,0)` Lookout | same |
| First `LoadSingleThing` file | Bridge `TRACK_NODE_BASIC` | same if global `+128` off |
| Lookout in `00507C30` | `LoadGlobalThingsFile` parse | `004FDBC0`; construct gated |
| HSP | `0052AC90` note on Lookout pass | same factory |
| Hero | `SpawnHeroFromPlayerStart` after maps | `006AC910` after apply |

---

## UNREAD / PARTIAL

- `00501450` first E8 caller (body recovered).
- Live `[manager+128]` on first `005223F0` (would make a
  **pre-dummy** `0051FD80` on Lookout `M_Maze`).
- Live factory miss on first TrackNode (`004C9B80`).
- TLC WAD vs Anniversary bytes for `GuardTrack` UID/pos.
- Whether later `00501450` i=2…141 construct additional
  files **before** first Present (loop **PROVEN**; first
  file still Bridge).

---

## Do not

- Report first construct file as Lookout because the
  **region** is LookoutPoint.
- Treat `00507C30` TNG parse as `0051FD80`.
- Move `GuildArrivalHSP` construct to `006AC910` or to
  Oakvale / `00DBDE40`.
- Skip `BowerstoneBridge` because first **open** was
  Lookout or first **Present** C3D is Lookout.
- Bind `StartOakVale` / kid `CREATURE_HERO_CHILD` as this
  first Load Single Thing file.
