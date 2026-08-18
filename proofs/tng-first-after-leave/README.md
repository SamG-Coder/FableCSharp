# First TNG file open after no-save Leave

Investigation only. Production `src/` was not edited.

Do **not** start at `00DBDE40` / `StartOakVale` /
`StartOakValeWest` / `CREATURE_HERO_CHILD`. No-save New Game is
message **15** → Leave `0042F2A2` → `FinalAlbion.wld`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**DIVERGE**.

Question: first TNG **file open** after no-save Leave. Which
map? After Set Static Map / `00501450` / `0051FD80`? Which
thing types are first parsed?

Authority: dump `0051FD80` / `NewThing` / `HOLY_SITE`.
Siblings: `proofs/tng-after-leave`, `proofs/tng-first-def`,
`proofs/tng-spawn`, `proofs/wld-first-region`,
`proofs/stb-first-open`.

Sources: `listing-004c0000.txt` (`004FDBC0` / `004FAFF0`),
`listing-00500000.txt` (`0051FD80` / `00520D00` / `005223F0` /
`00522A20` / `0051DC30`),
`docs/runtime/FORWARD_TREE.md` §§9–10,
`docs/status/investigations/2026-08-18-first-scene-things.md`
(+ dump),
`src/Fable.Formats/Tng/ThingFile.cs`,
`EngineLifecycle.LoadWorldMap` / `LoadGlobalThingsFile` /
`LoadRegionMapThings` / `LoadSingleThing`,
`TlcInstallTests.Tng_text_parser_reads_sample`,
`EngineLifecycleTests.Loading_objects_00521AE0_loads_LookoutPoint_tng`.

---

## Verdict

**`LookoutPoint.tng`.** WLD `NewMap 1` / `Maps[0]`.

First TNG I/O after Leave is the global proximity walk
`004FDBC0` **inside** `00507C30`, still in Loading world
`004A1840`. That is **before** Set Static Map, **before**
`00501450`, and is **not** `0051FD80`.

| Claim | Status |
|---|---|
| First opened file is `LookoutPoint.tng` | **PROVEN** |
| Map is LookoutPoint (native index 1, not Oakvale) | **PROVEN** |
| Open is **before** Set Static Map `004A1BD3` | **PROVEN** |
| Open is **before** `00501450` | **PROVEN** |
| That open **is** `0051FD80` | **DISPROVEN** (`0051FD80` is construct) |
| First parsed `NewThing` kind / def | `Marker` / `MARKER_BASIC` `M_Maze` **PROVEN** |
| First parsed type is `HOLY_SITE` | **DISPROVEN** |
| First file is `StartOakVale*.tng` / `00DBDE40` | **DISPROVEN** |

Do not collapse this with the later region apply. After
`00501450` the first file that **`0051FD80` constructs** is
`BowerstoneBridge.tng` (`TRACK_NODE_BASIC`). That is a
different “first”. This note is the **open**.

---

## Path from Leave (no-save New Game)

```
0042F2A2  Leave frontend                         // no TNG
0042F44D  FinalAlbion.wld → game+90576
0042F491  Init Game → 00418DCA → 004184BD
  Create Players 004166A8
    00522A20  registrar  "Holy Site" / HOLY_SITE → 0052AC90
              // type table only; no file
  00416953  Loading world
    004A1840
      QST / Startup WAD
      00507C30  Load .wld file
        NewMap 1  LookoutPoint                   // slot 1
        NewRegion 1  ContainsMap Bridge, Lookout, Guild
        NewRegion 4  StartOakVale                // later
        0050959F  stem+.gtng  TLC miss           PROVEN skip
        00509859  Load global things
          [0x13B8609]==0 → 004FDBC0              ← FIRST OPEN
            ebx=1, edi=0x48  skip unused slot 0
            [slot+36] && [slot+40]  prox
            004FBF60 → 004FAFF0 (0x12442C4 ".tng")
            LookoutPoint.tng                     ← this file
              parse NewThing Marker MARKER_BASIC M_Maze
            005223F0  [manager+128]==1?          UNREAD live
              taken  → 00521AE0 / 0051FD80       UNREAD
              host   → concat GlobalThings, no 0051FD80
        00509982  region graph
      004A1BD3  "Set Static Map for Engine"      // AFTER the open
        00B23DC0 → 00B428E0  FinalAlbion.stb miss
      0049F180  Init Characters                  // no TNG
004189C2  dummy pumps  index 0                   // 0 things
later 00501450 → 00500540(1,0,0) LookoutPoint
  006C2170  Loading objects  ContainsMap order
    BowerstoneBridge.tng                         // first 0051FD80
      00522720 / 00521AE0 / 00520D00 NewThing
      0051FD80  TRACK_NODE_BASIC GuardTrack
    LookoutPoint.tng  288                        // reopen
    GuildExterior.tng  88
    HOLY_SITE_PLAYER_START GuildArrivalHSP → 006AC910 later
```

`Q_NewOakValeIntro` / `00DBDE40` never open a TNG on this tree.

---

## After Set Static Map / `00501450` / `0051FD80`?

**No** for the first open. Those three are later or different.

| Site | When vs first `.tng` open | Role |
|---|---|---|
| Set Static Map `004A1BD3` / `00B428E0` | **after** `00507C30` (WLD + `004FDBC0`) | STB name `FinalAlbion.stb`; TLC miss. Not TNG. |
| Dummy `004189C2` index 0 | after Set Static Map | no `006C2170`, 0 things |
| `00501450` | after dummy pumps (E8 caller **UNREAD**) | `00500540(1,0,0)` Lookout **region** job |
| `0051FD80` | Load Single Thing | construct one `NewThing`; **not** the opener |

Host `LoadWorldMap` matches native order: `LoadGtngFile` →
`LoadGlobalThingsFile` → region graph →
`SetStaticMapFileForUse`. First TNG parse is in
`LoadGlobalThingsFile`. **MATCH**.

`00501450` then re-opens the three ContainsMap files for
construct. First of those is **`BowerstoneBridge.tng`**, not
Lookout. FORWARD_TREE “LookoutPoint first” is the **region
name**, not ContainsMap file order.

---

## Dump `0051FD80` / `NewThing` / `HOLY_SITE`

### `0051FD80` Load Single Thing  (`listing-00500000.txt`)

```
0051FD80  sub esp, 52
0051FDA6  push "Load Single Thing 1"
          esi+24 in {2,3} → def stream 004C81F0
          else tokenizer 009BA330
0051FE7F  push "EndThing"
0051FECC  push "EndThing;"
0051FEF1  push "Load Single Thing 2"
0051FF1B  push "NULL"                 // default DefinitionType
          00528760  def lookup
          00A371C0  Allocate Class    (not PlayerCreature)
```

Callers on the region walk:

```
00520F13  push "NewThing"
00520F44  push "Loading entities from script"
00520F9A  call 0051FD80               // 00520D00 loop
          eax≠0 → push pointer into job vector
```

Also `0052062E` (`00520570` `"NewThing"` / `"END_THING_LEVEL"`).
`00521AE0` is the file token walk that feeds that loop.

`0051FD80` does **not** `00999230` / `0099AD80` a path. File
open is `004FAFF0` (global) or `00522720` / `00521AE0` (region).

### `HOLY_SITE` is a factory TypeName, not the first parse

| VA | What |
|---|---|
| `00522E69` / `00522E79` | Create Players registrar: `"Holy Site"` + `"HOLY_SITE"`, dword `0x52AC90`, kind `7` |
| `0051DC30` | TypeName getter: `push "HOLY_SITE"`; `ret` |
| `0052AC90` | factory noted by host when `DefinitionType==HOLY_SITE_PLAYER_START` |

Lookout TNG has **three** `NewThing Holy Site`
`HOLY_SITE_PLAYER_START` (`GuildArrivalHSP`, `LookoutPointHSP`,
`MAIN_START_POSITION`). They are **not** the first two
`EndThing`s. Gameflow is two `MARKER_BASIC`. Bridge has one
holy site, after eight `TRACK_NODE_BASIC`.

`FirstSeenInstancesAsC3d("HOLY_SITE", …)` is false. Hero create
`006AC910` uses `GuildArrivalHSP` **after** all three
ContainsMap files. **DISPROVEN** as first parsed type.

---

## Thing types first parsed (first opened file)

`ThingFile.Parse` / native `00521AE0`: `Version 2` →
`XXXSectionStart` → `NewThing <Kind>` → properties →
`EndThing`. Host stores `Kind` + `DefinitionType`.

`LookoutPoint.tng` (288) section order from the census dump:

`Gameflow:2`, `NULL:252`, then quest sections.

First two `NewThing`s (Gameflow) — also the
`TlcInstallTests` sample:

```
NewThing Marker
  DefinitionType "MARKER_BASIC"
  ScriptName     M_Maze
  UID            18446741874686296750
  pos            (49.669, 76.648, 35.252)

NewThing Marker
  DefinitionType "MARKER_BASIC"
  ScriptName     M_LadyGameflow
```

So the first parsed types are:

| Field | First value |
|---|---|
| `NewThing` kind | `Marker` |
| `DefinitionType` | `MARKER_BASIC` |
| `ScriptName` | `M_Maze` |
| TypeName if constructed | `MARKER` (not `HOLY_SITE`) |

File kind census (not first-in-file order): Object 192,
Marker 45, Thing 39, AICreature 9, Holy Site 3. **No**
`PlayerCreature`. **No** `CREATURE_HERO` / `_CHILD`.

If the live `005223F0` `[+128]==1` branch is taken, those two
markers would also be the first native `0051FD80`s — **UNREAD**
as a taken New Game branch. First-seen `00416392==0` after this
walk is **PARTIAL** evidence it did not fill countable Things.
Host skips construct. **MATCH** that skip.

---

## Later firsts (not this open)

After `00501450`, ContainsMap apply order is WLD bytes
**BowerstoneBridge, LookoutPoint, GuildExterior**.

First constructed CThing (`0051FD80` on the host / proven
region path):

```
NewThing TrackNode
  DefinitionType "TRACK_NODE_BASIC"
  ScriptName     GuardTrack
```

Then ×8 `TRACK_NODE_BASIC`, then first Object-kind
`OBJECT_STREETLAMP_LIT_SINGLE_01` (mesh 4978). See
`tng-first-def` / `tng-after-leave`.

Lookout is **re-opened** as ContainsMap[1] (288). Its first
`0051FD80` on that pass is again `MARKER_BASIC` `M_Maze`.

---

## Not these

| Candidate | Why not first TNG **file** after Leave |
|---|---|
| `StartOakValeWest.tng` / `00DBDE40` | NewRegion **4**; not this tree |
| `BowerstoneBridge.tng` | first **construct** file after `00501450`, second-or-later **open** (Lookout is Maps[0]) |
| `FinalAlbion.gtng` | missing; `0050959F` skip **PROVEN** |
| `FinalAlbion.gtg` | exists; BSS `[0x13B8609]=0` → not taken |
| Picnic / Greatwood / Guild Woods | later prox slots; SeesMap not in `004FDBC0` first hit |
| Set Static Map / `FinalAlbion.stb` | after the open; STB not TNG |
| `00501450` / dummy index 0 | after Init Game suffix |
| `HOLY_SITE` / `GuildArrivalHSP` | later def in the same files; spawn after apply |
| Create Players `00522A20` | registers TypeName; no `.tng` |

---

## Host vs native

| Site | Host | Native |
|---|---|---|
| First open | `LoadGlobalThingsFile` `TryLoadThings("LookoutPoint")` | `004FDBC0` ebx=1 → `004FAFF0` `.tng` |
| Global construct | no `LoadSingleThing` | `005223F0` gated **UNREAD** |
| Region reopen | `ApplyLoadJob` ContainsMap | `006C2170` pass 2 |
| First `0051FD80` note | Bridge `TRACK_NODE_BASIC` | same if global gate off |

---

## UNREAD / PARTIAL

- Live `[manager+128]` on first `005223F0` (would make first
  `0051FD80` Lookout `M_Maze` **during** `00416953`).
- `00501450` E8 caller (already UNREAD in FORWARD_TREE).
- TLC WAD vs Anniversary bytes for `GuardTrack` UID/pos (not
  needed for this file-open claim).

---

## Do not

- Treat first **open** (`LookoutPoint.tng`) as first **CThing**.
- Move the first open to after Set Static Map or `00501450`.
- Call `0051FD80` the opener.
- Bind `StartOakVale` / `00DBDE40` / kid `CREATURE_HERO_CHILD`.
- Report `HOLY_SITE` as the first parsed `NewThing` type.
- Skip `BowerstoneBridge` on the later construct walk because
  the first **open** was Lookout.
