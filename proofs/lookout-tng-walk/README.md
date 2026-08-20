# First NewThing walk of LookoutPoint.tng inside `00507C30`

Investigation only. Production `src/` was not edited.

Do **not** start at Oakvale / `00DBDE40` / `StartOakVale` /
`StartOakValeWest` / `CREATURE_HERO_CHILD`. No-save New Game is
message **15** → Leave `0042F2A2` → `FinalAlbion.wld` →
LookoutPoint. Sibling `proofs/tng-first-after-leave` is the
**file open**. This note is the **NewThing walk of that file**.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Question: after the first TNG **open** of `LookoutPoint.tng`
inside `00507C30` (still Loading world, **before** Set Static
Map): what is the first `NewThing` walk? `MARKER_BASIC`
`M_Maze` then `M_LadyGameflow` — exact order, count, first
Graphic? When is `HOLY_SITE_PLAYER_START` `GuildArrivalHSP`
parsed vs constructed?

Authority: dump `004FDBC0` / `00507C30`
(`listing-004c0000.txt`, `listing-00500000.txt`);
TLC `FinalAlbion.wld` + TLC WAD `LookoutPoint.tng` via host
parse; Anniversary loose `LookoutPoint.tng` (UID/pos MATCH
TLC dump); `proofs/tng-first-after-leave`.

Sources: `tools/Fable.ExeIndex/out/01-sections/text-map/listing-004c0000.txt`
(`004FDBC0` / `004FBF60` / `004FAFF0`),
`listing-00500000.txt` (`00507C30` / `0050959F` / `00509859` /
`00521AE0` / `00520D00` / `005223F0` / `0051FD80`);
`docs/runtime/FORWARD_TREE.md` §§9–10;
`docs/status/investigations/2026-08-18-first-scene-things.md`
(+ dump; install **TheLostChapters**);
TLC `data\Levels\FinalAlbion.wld` `NewMap 1`;
Anniversary
`WellingtonGame\FableData\Build\Data\Levels\FinalAlbion\LookoutPoint.tng`;
`src/Fable.Formats/Tng/ThingFile.cs`;
`EngineLifecycle.LoadWorldMap` / `LoadGlobalThingsFile` /
`LoadRegionMapThings` / `LoadSingleThing` /
`SpawnHeroFromPlayerStart`;
`TlcInstallTests.Tng_text_parser_reads_sample`;
`GameBinFormatTests.Markers_and_cameras_do_not_resolve_to_editor_meshes`;
`EngineLifecycleTests.Loading_objects_00521AE0_loads_LookoutPoint_tng`,
`Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`.

---

## Verdict

**Yes: `MARKER_BASIC` `M_Maze`, then `MARKER_BASIC`
`M_LadyGameflow`.** That is the first two `NewThing`s in
the first opened TNG. Neither has a Graphic. Count of this
file is **288**. First Graphic-bearing def in the same
walk is later **`OBJECT_SILVER_KEY`** mesh **7934**
(13th `NewThing`). `GuildArrivalHSP` is **parsed** on this
walk (35th `NewThing`) and **constructed** later, after
Set Static Map / `00501450`, not here.

| Claim | Status |
|---|---|
| First TNG open inside `00507C30` is `LookoutPoint.tng` | **PROVEN** |
| That open is **before** Set Static Map `004A1BD3` | **PROVEN** |
| First two `NewThing`s: `Marker` / `MARKER_BASIC` **`M_Maze`**, then **`M_LadyGameflow`** | **PROVEN** |
| File count **288** (`Gameflow:2` + `NULL:252` + quests) | **PROVEN** |
| First two have TNG `Graphic` / `FindMeshId` | **DISPROVEN** (`FindMeshId("MARKER_BASIC")==null`) |
| First Graphic def in this file: `OBJECT_SILVER_KEY` **7934** | **PROVEN** (Anniversary file order; TLC UID/pos MATCH) |
| Ordinal **13** for that key on TLC WAD bytes | **PARTIAL** (loose file order; TLC WAD compressed) |
| `GuildArrivalHSP` **parsed** on this open/walk | **PROVEN** (same file, before EndMap of global walk) |
| `GuildArrivalHSP` **constructed** (`0051FD80` / `0052AC90`) on this open | **DISPROVEN** host; native `[manager+128]` **UNREAD** |
| First construct of that marker is later `006C2170` Lookout pass | **PROVEN** |
| Hero `006AC910` at that pose is this walk | **DISPROVEN** (after all three ContainsMap files) |
| First NewThing is `HOLY_SITE` / Oakvale / `00DBDE40` | **DISPROVEN** |

Do **not** collapse this walk with the later ContainsMap
`0051FD80` walk (`BowerstoneBridge` `TRACK_NODE_BASIC` first).
That is `proofs/tng-first-def`.

---

## Path (no-save New Game, this window only)

```
0042F2A2  Leave frontend                         // no TNG
0042F491  Init Game → 00418DCA → 004184BD
  00416953  Loading world
    004A1840
      QST / Startup WAD
      00507C30  Load .wld file                   // this function
        NewMap 1  LookoutPoint  prox TRUE        // slot 1; Maps[0]
        NewMap 2  PicnicArea                     // later slot
        NewRegion 1  ContainsMap Bridge, Lookout, Guild
        NewRegion 4  StartOakVale                // later; not this tree
        0050959F  Load GTNG  stem+.gtng          PROVEN skip (TLC miss)
        00509859  Load global things
          [0x13B8609]==0 → 004FDBC0              ← FIRST OPEN
            ebx=1, edi=0x48  skip unused slot 0
            [slot+36] && [slot+40]  prox
            004FBF60 → 004FAFF0 (0x12442C4 ".tng")
            LookoutPoint.tng                     ← this file
              005223F0  [manager+128]==1?
                taken → 00521AE0 / 00520D00 / 0051FD80   UNREAD live
                host  → ThingFile.Parse, no 0051FD80     PROVEN skip
              NewThing walk of THIS file:
                1  Marker MARKER_BASIC M_Maze            ← first
                2  Marker MARKER_BASIC M_LadyGameflow
                …  288 EndThing
                13 Object OBJECT_SILVER_KEY Graphic 7934 ← first Graphic
                35 Holy Site HOLY_SITE_PLAYER_START GuildArrivalHSP  ← parse
        00509982  Load region graph
      004A1BD3  "Set Static Map for Engine"      // AFTER the walk
        00B23DC0 → 00B428E0  FinalAlbion.stb miss
later 00501450 → 006C2170 ContainsMap
  BowerstoneBridge.tng  first 0051FD80 TRACK_NODE_BASIC
  LookoutPoint.tng  reopen 288
    0051FD80 HOLY_SITE GuildArrivalHSP           ← construct
  GuildExterior.tng
  006AC910 CREATURE_HERO at GuildArrivalHSP pose
```

`Q_NewOakValeIntro` / `00DBDE40` never open this file.

---

## Dump `00507C30` / `004FDBC0`

### `00507C30` Load `.wld` file  (`listing-00500000.txt`)

```
00507C30  sub esp, 0x1BC
00507C9A  push "Load .wld file"
…
0050959F  push "Load GTNG"
00509634  call 00999230                 // miss → 00509857
00509859  push "Load global things"
0050987B  mov al, [0x13B8609]
00509880  test al, al
00509882  je 00509946                   // BSS 0 taken
00509946  mov ecx, ebx
00509948  call 004FDBC0                 ← per-map .tng
0050994F  push "Load global things end"
00509982  push "Load region graph"
```

Nonzero `[0x13B8609]` would `004FE2A0` `.gtg`. No-save default
is **0**. TLC `.gtng` missing skip is **PROVEN**.

`004A1840` then `"Set Static Map for Engine"` `004A1BD3` /
`00B428E0`. STB, not TNG. **After** `00507C30` returns.

### `004FDBC0` Loading global things  (`listing-004c0000.txt`)

```
004FDBC0  sub esp, 8
004FDBDE  mov ebx, 0x1                  // skip slot 0
004FDBF2  mov edi, 0x48                 // 72-byte map stride
004FDC02  push "Loading global things"
004FDC60  mov cl, [eax+edi*1+36]        // filled
004FDC6A  mov cl, [eax+40]              // LoadedOnPlayerProximity
004FDC71  push ebx
004FDC74  call 004FBF60                 // first hit: ebx=1
004FDC90  inc ebx
004FDC93  add edi, 72
004FDC9C  jb 004FDC00
```

`004FBF60` → `004FAFF0` `push 0x12442C4` (`.tng`) → open
`0099AD80` → `005223F0`.

TLC WLD `NewMap 1`:

```
NewMap 1;
  LevelName "FinalAlbion\LookoutPoint.lev";
  LevelScriptName "LookoutPoint";
  LoadedOnPlayerProximity TRUE;
EndMap;
```

First prox slot is LookoutPoint. **PROVEN**. PicnicArea is
`NewMap 2` (second open, not this walk). Bridge is `NewMap 4`.

Dump `2026-08-18-first-scene-things.dump.txt` (TLC install):
`prox LookoutPoint things=288 alsoContains=True`. Loose
`Levels\FinalAlbion\LookoutPoint.tng` is absent; bytes come
from `FinalAlbion.wad`. Grep of the WAD for `ScriptName M_Maze`
is empty (BBBB payload). Host `ThingFile.Parse` of that entry
is the TLC authority for counts/UIDs.

### `005223F0` gate  (`listing-00500000.txt`)

```
005223F7  mov eax, [esi+128]
005223FF  cmp eax, 1
00522403  jne 00522502                  // skip 00521AE0
0052249F  call 00521AE0                 // Thing Manager: Load From File
005224AB  call 0051E2F0
```

Host `LoadGlobalThingsFile` concatenates into `GlobalThings`
and does **not** `LoadSingleThing`. Live `[manager+128]` on
this first call is **UNREAD**. First-seen `00416392==0` after
the walk is **PARTIAL** evidence the construct branch did not
fill countable Things.

If the gate **is** 1, this is also the first native
`0051FD80` (`M_Maze`). That would **not** change file order.

### `00521AE0` / `00520D00`  (the NewThing walk)

```
00521B24  push "Thing Manager: Load From File"
00521BC4  push "Version"
00520D91  push "XXXSectionStart"
00520F13  push "NewThing"
00520F44  push "Loading entities from script"
00520F9A  call 0051FD80                 // construct one block
0052203B  push "NewThing "
00522074  push "EndThing;"
```

File order = `XXXSectionStart` order = `ThingFile.Things`.
No sort-by-kind.

---

## Exact first NewThings (this file)

TLC census (host parse of WAD): **288** things,
sections **`Gameflow:2`**, **`NULL:252`**, `Q_FireHeart:3`,
`Q_GuildTraining:10`, `Q_WaspBoss:3`, `V_BeggarAndChild:14`,
`V_SickChild_Activate:3`, `V_StatueMaster:1`.

Kinds: Object 192, Marker 45, Thing 39, AICreature 9,
Holy Site **3**. **No** `PlayerCreature`. **No**
`CREATURE_HERO` / `_CHILD`.

Gameflow is the first section. Both blocks are
`NewThing Marker` / `DefinitionType "MARKER_BASIC"`.

Anniversary loose (UID/pos MATCH TLC dump and the
`TlcInstallTests` sample):

```
XXXSectionStart Gameflow;

NewThing Marker;                         // #1
  UID            18446741874686296750
  DefinitionType "MARKER_BASIC"
  ScriptName     M_Maze
  pos            (49.669189, 76.648438, 35.252132)
  RHSetForward   (-0.651341, 0.758778, 0)
  // no Graphic key

NewThing Marker;                         // #2
  UID            18446741874686296749
  DefinitionType "MARKER_BASIC"
  ScriptName     M_LadyGameflow
  pos            (50.62085, 78.385986, 35.616848)
  RHSetForward   (-0.94881, 0.31583, 0)
  // no Graphic key

XXXSectionEnd;
XXXSectionStart NULL;
  NewThing Marker MARKER_BASIC VC        // #3, #4
  …
```

**Order is `M_Maze` then `M_LadyGameflow`.** Not the reverse.
`Player` on those two is `-1`. NULL then starts at `VC`
(`Player 4`), not another Gameflow marker.

| # | Kind | DefinitionType | ScriptName | Graphic |
|--:|---|---|---|---|
| 1 | Marker | `MARKER_BASIC` | `M_Maze` | none |
| 2 | Marker | `MARKER_BASIC` | `M_LadyGameflow` | none |
| 3–4 | Marker | `MARKER_BASIC` | `VC` | none |
| 5–9 | Thing | `CAMERA_POINT_SCRIPTED_SPLINE` | `FH10`, `HEROWALK10`, `GMROSE`, `EXITDD`, `CAM_FH_HERODOOR` | none |
| 10–12 | Marker | `GAZE_OUT_OF_BUILDING_MARKER` | `NULL` | none |
| **13** | **Object** | **`OBJECT_SILVER_KEY`** | `NULL` | **7934** |
| 35 | Holy Site | `HOLY_SITE_PLAYER_START` | `GuildArrivalHSP` | none (not C3D) |

`GameBin.FindMeshId("MARKER_BASIC")` is **null**
(`GameBinFormatTests`). Cameras / gaze / holy site
`FirstSeenInstancesAsC3d` is false. Dump `mesh=-` for those.

TNG text has **no** `Graphic` property on any Lookout
`NewThing`. Graphic is `game.bin` on the **def** at
`0051FD80` / `004CA010`, not a TNG key.

---

## First Graphic

**`OBJECT_SILVER_KEY`**, mesh **7934**, UID
`18446741874686298187`, pos `(103.027, 62.399, 36.731)`.

That is the first `NewThing Object` and the first def
with a mesh. Next Graphic objects in this file:

```
#15–16  OBJECT_DEGRADABLE_THORN_VINES_01   3977
#17     OBJECT_BS_SIGN_POST_DIRECTION_01   4911
```

Hero Graphic **4299** is **not** this walk (no
`PlayerCreature` in the file). Kid **4300** is **DISPROVEN**.

If this open only **parses** (host; native gate off), Graphic
is **not bound** here. The **first def that would bind a
Graphic** is still the silver key.

---

## `GuildArrivalHSP`: parse vs construct

Three `NewThing Holy Site` / `HOLY_SITE_PLAYER_START` in
this file (TLC count **3**):

| Script | UID | pos | When in file |
|---|---|---|---|
| **`GuildArrivalHSP`** | `…297902` | `(52.688, 69.597, 36.982)` fwd `+X` | first holy site (Anniversary **#35**) |
| `LookoutPointHSP` | `…296778` | `(102.781, 74.156, 37.494)` | later NULL |
| `MAIN_START_POSITION` | `…296065` | `(102.887, 74.127, 37.488)` | later NULL |

**Parsed** as soon as the Lookout TNG tokenizer hits that
`NewThing` / `EndThing` pair — on this `004FDBC0` open if
`00521AE0` (or host `ThingFile.Parse`) runs, and again on the
later `006C2170` reopen. Create Players `00522A20` only
registers TypeName `"HOLY_SITE"` / factory `0052AC90`; it does
not parse the file.

**Constructed** (`0051FD80` → `00A371C0` → `0052AC90`):

| Site | Construct `GuildArrivalHSP`? |
|---|---|
| `004FDBC0` / `005223F0` first open | host **no**. native **UNREAD** (`[+128]`) |
| `006C2170` Lookout ContainsMap[1] | **yes** (`LoadRegionMapThings`) |
| `006AC910` after all three maps | **no** — that creates `CREATURE_HERO` **at** the already-constructed marker pose |

`SpawnHeroFromPlayerStart` prefers script `GuildArrivalHSP`
over the other two holy sites. Lookout TNG has no
`PlayerCreature`, so hero create is **not** a TNG `NewThing`.

`HOLY_SITE` is **not** the first parsed type. Gameflow is two
markers. Dump `exist-all` lists holy sites above `M_Maze` only
because it groups by category, **not** file order.

---

## Host vs native

| Site | Host | Native |
|---|---|---|
| First open | `LoadGlobalThingsFile` `TryLoadThings("LookoutPoint")` | `004FDBC0` ebx=1 → `004FAFF0` `.tng` |
| NewThing walk | `ThingFile.Parse` 288, file order | `00521AE0` / `00520D00` if `[+128]==1` |
| First two | `M_Maze`, `M_LadyGameflow` | same bytes |
| Construct | skip | gated **UNREAD** |
| `GuildArrivalHSP` construct | later `LoadRegionMapThings` | later `006C2170` |
| Hero | `006AC910` after three maps | same |

---

## Not these

| Candidate | Why not this walk |
|---|---|
| `HOLY_SITE` / `GuildArrivalHSP` as **first** `NewThing` | 35th; Gameflow is two `MARKER_BASIC` |
| First Graphic = 4299 / `MESH_HERO` | not in this TNG |
| `BowerstoneBridge` `TRACK_NODE_BASIC` | later construct walk after `00501450` |
| `StartOakVale*.tng` / `00DBDE40` | `NewRegion 4`; not this tree |
| `FinalAlbion.gtng` / `.gtg` | miss / BSS 0 |
| Set Static Map / `FinalAlbion.stb` | after `00507C30` |
| Picnic / Greatwood prox TNG | later `004FDBC0` slots |

---

## UNREAD / PARTIAL

- Live `[manager+128]` on the first `005223F0` (would make
  `M_Maze` the first `0051FD80` **during** `00416953`).
- TLC WAD raw text (compressed). Counts/UIDs **PROVEN** from
  TLC host parse; ordinals 13 / 35 **PARTIAL** vs Anniversary
  loose (UID/pos of those things MATCH the TLC dump).
- Whether `0099AD80` without `00521AE0` still tokenizes
  (open-without-walk). Host always parses.

---

## Do not

- Treat dump `exist-all` kind groups as `NewThing` order.
- Call `HOLY_SITE` the first parsed type.
- Bind Graphic **4299** or kid **4300** to this walk.
- Move this walk after Set Static Map / `00501450`.
- Start at Oakvale / `00DBDE40`.
- Collapse first **open** (Lookout) with first **CThing**
  (Bridge `GuardTrack`).
