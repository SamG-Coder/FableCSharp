# First TNG thing after Leave

Investigation only. Production `src/` was not edited.

Do **not** start at Oakvale / `00DBDE40` / `CREATURE_HERO_CHILD` /
`StartOakValeWest`. No-save New Game is message **15** → Leave
`0042F2A2` → LookoutPoint.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Question: after Leave Frontend, what is the first TNG *thing*?
Parse of a `NewThing` block, or the first `0051FD80` CThing?

Sources: `src/Fable.Formats/Tng/ThingFile.cs`;
`src/Fable.Game/EngineLifecycle.cs` (`LeaveFrontendSite`,
`LoadGtngFile`, `LoadGlobalThingsFile`, `ApplyLoadJob`,
`LoadRegionMapThings`, `LoadSingleThing`);
`src/Fable.Game/LevelLibrary.cs`;
`docs/runtime/FORWARD_TREE.md` §§9–10;
`docs/status/investigations/2026-08-18-first-scene-things.md`
(+ dump);
`proofs/tng-first-def`, `tng-spawn`, `thing-manager-activate`,
`wld-first-region`, `npc-first-create`;
`EngineLifecycleTests.Loading_objects_00521AE0_loads_LookoutPoint_tng`,
`Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`;
`TlcInstallTests.Tng_text_parser_reads_sample`;
Anniversary loose `BowerstoneBridge.tng` / `LookoutPoint.tng`.

---

## Verdict

Two different “first”s. Do not collapse them.

| Sense | Answer | Class |
|---|---|---|
| First TNG *file* opened after Leave | `LookoutPoint.tng` inside `004FDBC0` (WLD NewMap 1, first prox slot) | **PROVEN** open |
| First `NewThing` in that file | `MARKER_BASIC` / `M_Maze` (section `Gameflow`) | **PROVEN** file order |
| That parse calls `0051FD80` | **no** on host; native `[manager+128]==1` | **DISPROVEN** host; **UNREAD** live |
| First *constructed* CThing (`0051FD80`) | **`TRACK_NODE_BASIC` / `GuardTrack`** from **`BowerstoneBridge.tng`** | **PROVEN** |
| First `NewThing Object` | later `OBJECT_STREETLAMP_LIT_SINGLE_01` (same Bridge file, after 8 track nodes) | **PROVEN** |
| First Lookout `0051FD80` | `MARKER_BASIC` `M_Maze` (second ContainsMap file) | **PROVEN** order |
| Hero / Oakvale kid | after all three ContainsMap files / not this tree | **DISPROVEN** as first |

**The Leave answer for “first TNG thing” as a CThing is
`TRACK_NODE_BASIC` `GuardTrack`.** It is a patrol node, not a
mesh, not a creature, not the hero.

Anniversary Bridge block (UID/pos **PARTIAL** vs TLC WAD bytes;
def/script/kind **PROVEN** vs dump census of 8× `TRACK_NODE_BASIC`):

```
NewThing TrackNode
  DefinitionType "TRACK_NODE_BASIC"
  ScriptName     GuardTrack
  UID            18446741874686299399
  pos            (76.686, 30.849, 17.517)
```

`ThingFile.Parse` stores `DefinitionType` on `ThingInstance`.
Host `LoadRegionMapThings` walks `tng.Things` in file
`NewThing` order and calls `LoadSingleThing` (`0051FD80`).

---

## Path from Leave (no-save New Game)

```
0042F2A2  Leave frontend                         // no TNG
0042F491  Init Game → 00418DCA → 004184BD
  Create Players 004166A8                        // slots, not TNG
  00416953  Loading world  FinalAlbion.wld
    00507C30  WLD
    0050959F  stem+.gtng          missing        PROVEN
    00509859  Load global things
      [0x13B8609]==0 → 004FDBC0                  PROVEN
        skip slot 0; Maps[0] = LookoutPoint
        first .tng open: LookoutPoint.tng
          NewThing Marker  MARKER_BASIC  M_Maze  // parse only
        151 maps / ~21746–22087 things
        005223F0 [manager+128]==1 → 00521AE0     UNREAD live
        host: concat GlobalThings, no 0051FD80   PROVEN skip
    0049F180  Init Characters                    // no 0051FD80
004189C2  dummy pumps  index 0                   // 0 things
later 00501450 → 00500540(1,0,0) LookoutPoint
  006C2170
    pass 1 topology: Bridge, Lookout, Guild
    pass 2 Loading objects  ContainsMap order
      BowerstoneBridge.tng  88                   // ContainsMap[0]
        00522720 / 00521AE0 / 00520D00
        0051FD80  TRACK_NODE_BASIC GuardTrack    ← first CThing
          00A371C0 Allocate Class
          004CA010 / 004C76A5 THING_TYPE_TRACK_NODE
        ×8 TRACK_NODE_BASIC
        0051FD80  OBJECT_STREETLAMP_LIT_SINGLE_01  ← first Object
      LookoutPoint.tng  288
        Gameflow  MARKER_BASIC  M_Maze, M_LadyGameflow
      GuildExterior.tng  88
    006C2470  0051E2F0 Activate Things
      vector[0] = that GuardTrack
    0049F180 / 00449D90 / 006AC910 CREATURE_HERO later
```

WLD `NewRegion 1` `ContainsMap` order is **BowerstoneBridge,
LookoutPoint, GuildExterior** (file bytes). Region *name*
LookoutPoint is first; TNG *apply* is not. FORWARD_TREE
“LookoutPoint first” is authored-region wording.

`Q_NewOakValeIntro` / `00DBDE40` / kid `CREATURE_HERO_CHILD`
are **DISPROVEN** on this tree.

---

## 1. Leave and frontend — no TNG  **PROVEN**

| Claim | Class |
|---|---|
| Leave `0042F2A2` opens `.tng` / `.gtng` / `.gtg` | **DISPROVEN** |
| Frontend Present is 2D UI | **PROVEN** (`camera-after-leave`) |
| Create Players is TNG | **DISPROVEN** (5× `0x22C` slots) |
| `hero_swap_*.tng` on no-save | **DISPROVEN** |

First TNG I/O is inside `00416953` / `00507C30`, still Init
Game, after Leave.

---

## 2. Global walk — first *file*, not first CThing

`004FDBC0` (map stride 72, `ebx` starts 1):

- Slot 0 unused. `NewMap 1` is LookoutPoint. C# `World.Maps[0]`
  is that map. **MATCH**.
- Gate `[slot+36] && [slot+40]` (`LoadedOnPlayerProximity`).
  TLC writes the token. **MATCH**.
- Per hit: `004FBF60` → `004FAFF0` (`".tng"`) → `005223F0`.

So the first TNG *bytes* after Leave are **`LookoutPoint.tng`**.

Lookout file section order (`2026-08-18-first-scene-things.dump.txt`):

`Gameflow:2`, `NULL:252`, then quest sections. Gameflow is two
`MARKER_BASIC`: **`M_Maze`**, **`M_LadyGameflow`**. The host
sample in `TlcInstallTests` is that first marker
(UID `18446741874686296750`, pos `(49.669, 76.648, 35.252)`).

`005223F0` only calls `00521AE0` if `[manager+128]==1`. Host
`LoadGlobalThingsFile` concatenates into `GlobalThings` named
`GLOBAL` and does **not** `LoadSingleThing`. First-seen
`00416392==0` after this walk is **PROVEN**; that is **PARTIAL**
evidence the live `+128` branch did not fill countable Things.

`.gtg` exists; BSS `[0x13B8609]=0` so no-save does **not** take
`004FE2A0`. TLC `.gtng` missing skip **PROVEN**.

SeesMap / Picnic / Greatwood TNG are **not** this first file.

---

## 3. Region walk — first constructed CThing

`006C2170` pass 2, ContainsMap order, `push 3` then `00521AE0`.
Host `ApplyLoadJob` → `foreach ContainsMaps LoadRegionMapThings`.
Counts **88+288+88**. **MATCH**.

Bridge census (88): Object 67, TrackNode **8**, Marker 6,
Thing 4, Building 2, Holy Site 1. Sections
`NULL:85, QR_EscortTrader:1, Q_FireHeart:1, Q_WaspBoss:1`.
The eight `TRACK_NODE_BASIC` are the first eight `EndThing`s
(NULL section). No sort-by-kind on `006C2170` pass 2.

`00520D00` on `"NewThing"`: `0051FD80`; if eax≠0, push the
pointer into the job vector. `0051E2F0` later walks that
vector. First slot is this TrackNode if construct succeeds
(factory miss would `004C9B80` and skip the push — **UNREAD**
as a live miss). Type name `THING_TYPE_TRACK_NODE` `004C76A5`.
`FirstSeenInstancesAsC3d("THING","TRACK_NODE_BASIC")` is
false — **not** a first-Present C3D.

Then 8th TrackNode done → first Object-kind
**`OBJECT_STREETLAMP_LIT_SINGLE_01`** mesh **4978**.

Lookout’s first `0051FD80` is only after those 88. Guild
after Lookout’s 288. Hero `006AC910` after all three
(`HOLY_SITE_PLAYER_START` `GuildArrivalHSP`
`(52.688, 69.597, 36.982)`, mesh 4299). Lookout TNG has
**no** `PlayerCreature`.

---

## 4. Host vs native

| Site | Host | Native |
|---|---|---|
| `LoadGlobalThingsFile` | parse 151 / ~21746, no insert | `004FDBC0` open; `00521AE0` gated **UNREAD** |
| `LoadRegionMapThings` | `LoadSingleThing` per instance | `00522720` / `00521AE0` / `00520D00` / `0051FD80` |
| First `Load Single Thing 2` note | `TRACK_NODE_BASIC` | same file order if no cache prepend |
| `00522720` cache `[manager+160]` | not modelled | first-visit empty **PARTIAL** |
| `InsertThing` | `004CA010` note | listing **PROVEN** |
| `0051E2F0` | one Note after objects; no vtbl walk | three-pass Activate Things **PROVEN** |

---

## Not these

| Candidate | Why not first TNG CThing after Leave |
|---|---|
| `MARKER_BASIC` `M_Maze` | first *bytes* in first *opened* file; not `0051FD80` |
| `004FDBC0` / `FinalAlbion.gtg` / `.gtng` | parse-only / unused / missing |
| `OBJECT_STREETLAMP_LIT_SINGLE_01` | first Object-kind, after 8 track nodes |
| Lookout cameras / walls | second ContainsMap file |
| `CREATURE_BS_VILLAGER_MALE` `FH_Villager` | first NPC, later `Q_FireHeart` |
| `CREATURE_HERO` / `GuildArrivalHSP` | after all three maps |
| `CREATURE_HERO_CHILD` / `00DBDE40` | not this tree |
| Create Players / `004AE9D0` / `00416392` | not Thing spawn |
| Script `Create` `00CCC246` | no `00CBFB7D` after Leave |

---

## UNREAD / PARTIAL

- Live `[manager+128]` on first `005223F0`.
- Live `0051FD80` miss on first TrackNode (`004C9B80`).
- TLC WAD vs Anniversary TNG *bytes* for `GuardTrack` UID/pos
  (def/script/count MATCH).
- Exact `+148` after construct (`0051E2F0` which pass fires).
- `00501450` E8 caller (already UNREAD in FORWARD_TREE).

---

## Do not

- Treat first opened TNG (`LookoutPoint.tng` / `M_Maze`) as the
  first CThing.
- Treat `LoadGlobalThingsFile` as `0051FD80`.
- Skip `BowerstoneBridge` because the camera is on Lookout.
- Spawn `CREATURE_HERO_CHILD` or bind `StartOakVale` on this path.
- Submit `TRACK_NODE_BASIC` as a first-scene mesh.
