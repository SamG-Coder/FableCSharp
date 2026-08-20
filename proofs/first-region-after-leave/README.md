# First real region after no-save Leave / Init Game

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `StartOakVale` / `StartOakValeWest` /
`00DBDE40` / `Q_NewOakValeIntro` / `NOVStartHSP`. Those are later
quest / persist leftovers. Native first authored region is
**LookoutPoint**. First player-start pose on that region is
**GuildArrivalHSP**.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Question: first `00501450` (or first real region *open*) after
no-save Leave / Init Game. Exact first call site after `0049F180` /
`004B4260` / `user.ini` Gameflow. What loads the first LEV / TNG?

Authority: dump listings (`e8.tsv`, `listing-00400000.txt`,
`listing-00480000.txt`, `listing-00500000.txt`, `listing-006c0000.txt`),
`docs/runtime/FORWARD_TREE.md` §§6–10, `docs/PARITY.md` Init Game /
no-save enqueue rows, `proofs/wld-first-region`,
`proofs/region-travel-first`, `proofs/load-job`, `proofs/tng-after-leave`,
`proofs/navmesh-first`, `proofs/ini-activate-quest`,
`proofs/script-gameflow`,
`EngineLifecycle.LoadFromFirstRealRegion` / `RequestLoadRegion` /
`ApplyLoadJob`,
`EngineLifecycleTests.LoadWorld_00416953_no_save_is_004A1840_then_0049F180`,
`Second_pump_004189C2_loops_inner_not_00501450`,
`Second_pump_00501450_is_004FEEC0_then_00500540_1_0_0`,
`Loading_objects_00521AE0_loads_LookoutPoint_tng`,
`Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`.

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| First *named* region after Leave? | WLD `NewRegion 1` **`LookoutPoint`** (native index **1**; dummy 0 is empty) | **PROVEN** |
| First *applied* region job? | `00500540(1,0,0)` Lookout ContainsMap, inside `00501450` **if that body runs** | **PROVEN** body; **UNREAD** as a live `E8` |
| Exact `E8` / `E9` / imm / vtbl of `00501450`? | **0 hits** | **PROVEN** absence |
| First call *after* `0049F180` / `004B4260` / Gameflow that *is* a region open? | **none on the recovered tree.** Init Game suffix and first `004189C2` pumps do not `E8` `00501450` / `00500540` / `006C27A0` | **PROVEN** skip |
| Host stand-in? | explicit `LoadFromFirstRealRegion` (`00501450`) after dummy pumps. `EnqueueAfterDummy` on the second `Pump` is **DISPROVEN** | **DIVERGE** site; **MATCH** body |
| First LEV *name*? | `00507C30` `LevelName "FinalAlbion\LookoutPoint.lev"` during `004A1840`, **before** `0049F180` | **PROVEN** parse |
| First LEV *apply*? | `006C2170` pass 1 `"Loading topology"` `004FF080` / `00638310` / `004FF440` after first `00500540(1,0,0)` | **PROVEN** apply; **UNREAD** as compiled `.lev` bytes (`00B3EFA0` first-seen miss) |
| First TNG *bytes*? | `004FDBC0` opens **`LookoutPoint.tng`** (WLD `Maps[0]`) during global walk, **before** `0049F180` | **PROVEN** open |
| First TNG *CThing*? | `006C2170` pass 2 ContainsMap order: **`BowerstoneBridge.tng`** first `0051FD80` | **PROVEN** |
| Oakvale / `00DBDE40` as this first region? | **no** | **DISPROVEN** |

**Do not invent Oakvale as the first region.** Wiki kid start is
`NewRegion 4` `StartOakVale` / later `S_QNOVI`. Native no-save first
real region is **LookoutPoint**. Hero pose on that load is
**GuildArrivalHSP**, adult `CREATURE_HERO` mesh **4299**.

---

## Recovered order (no-save New Game)

```
0042F2A2  Leave frontend                         PROVEN
  0042F44D  FinalAlbion.wld → game+90576         PROVEN
0042F491  Init Game 00418DCA → vtbl+4 004184BD
  Init World 004A6E30
    005066E0 World Map  (dummy slot 0, +156=0)
  Create Players 004166A8                        // slots, not TNG
  00416953  Loading world                        // NOT a region load
    004A1840
      00507C30  Load .wld file                   PROVEN
        NewMap 1  LevelName LookoutPoint.lev     // first LEV name
        NewRegion 1  RegionName "LookoutPoint"   // native index 1
        ContainsMap BowerstoneBridge / LookoutPoint / GuildExterior
        NewRegion 4  RegionName "StartOakVale"   // later leftover
        START_INITIAL_QUESTS → world+172
      0050959F  stem+.gtng  TLC miss
      00509859  Load global things
        [0x13B8609]==0 → 004FDBC0                PROVEN
          first .tng open: LookoutPoint.tng      // first TNG bytes
          005223F0 [manager+128]==1 → 00521AE0   UNREAD live
      006C20A0 empty                             PROVEN  (no 006C27A0)
      00B23DC0 → 00B428E0  FinalAlbion.stb miss  PROVEN
    [0x13B8648]==0
      0049F180  Init Characters / GUI / Quests   PROVEN  not a region
        004B4260([world+172])  six WLD names
      +90584 empty → skip extra 004B4A10
    004BBC00  ret 4
  0049BA70 / 00416392 / 004AE9D0
  default_user.ini miss
  user.ini 009EC890
    ActivateQuest("Gameflow") 00419CE0
      00892E80 → 004B4A10(1,1) → 004B4260 → 00CB5AD0
      00CE75B0 Main watcher                      // not S_GF opcode
      00501450 not reached                       PROVEN  0 E8 / 0 imm
  seed 004167DA / +90592
004189C2  first pumps
  WorldMap+156=0 dummy 004FC180
  CurrentRegion=null
  no 00501450 / 00500540 / 006C27A0              PROVEN
  inner loop until [game+8]  (WM_DESTROY only)
later  (E8 caller UNREAD; host LoadFromFirstRealRegion)
  00501450                                       PROVEN body
    00449970 / 00487DC0  player (may miss)
    004FEEC0(0,0)  +156=0
    count=(+48−+44)/88  = 142
    for i=1..141  00500540(i,0,0)
      i=1 LookoutPoint                           ← first real open
        006C27A0 / 006C2120 / 006C2710
        006C2170
          pass 1 topology 004FF080               ← first LEV apply
          pass 2 objects  00522720 / 00521AE0    ← first TNG CThing
      i=2 PicnicArea …
      i=141 Filler_NorthernWastes_02
    RegionGraph.txt
    00500540(saved=0,0,1)  no sync pump
```

`00DBDE40` / `StartOakVale` / kid `CREATURE_HERO_CHILD` are **not**
on this list. **PROVEN.**

---

## 1. After `0049F180` / `004B4260` / Gameflow — no region open

### `0049F180` is not a load

`listing-00480000.txt` `0049F180`…`0049F24E`:

```
"Init Characters"  00449970 / 00487DC0 / 00449D90
"Init GUI"         0043A380 [0x13B8790]
"Init Quests"      004B4260([world+172]) then 004B2890
```

Site from Loading world: `00416BCA call 0049F180` (`ecx=world`,
`push 0`). No `00500540`, no `006C27A0`, no `.lev` / `.tng`.
Holy-site lookup `00489D40` **misses** `NOVStartHSP` and does not
`006AC910`. **PROVEN** (`proofs/creature-after-leave`,
`proofs/script-setnewstart`).

### `004B4260` is WLD names, then Gameflow

First `004B4260` is Init Quests on `world+172`
(`Q_SunnyvaleMaster`, `PersonalScriptMain`,
`PersonalScript_GlobalThings`, `HeroBoasts`, `V_HeroDolls`,
`CS_PlayCutscene`). Not Gameflow. Not Oakvale.

`004B3CE0` construct on that walk reaches `004B2510` →
`00501990` (`CWorldMap::UpdateNavMaps`). WorldMap `+144` is
still empty → **no-op**. **PROVEN** (`proofs/navmesh-first`).
That is **not** `00501450`.

### `user.ini` Gameflow is a watcher

After `00416953` returns, `004184BD` runs `user.ini`
`ActivateQuest("Gameflow")` via `00419CE0` → `00892E80` →
`004B4A10(1,1)` → **second** `004B4260` → `00CB5AD0`.
Factory `00CEF950` / Main `00CE75B0`. `S_GF` as a
`CCutsceneDef` at this site is **DISPROVEN**. State 0 later
**yields** on inactive `Q_NewOakValeIntro`; it does not
`ActivateQuest` that name and does not load a region.
`00501450` **not reached** (0 `E8` / 0 imm on this suffix).
**PROVEN** (`proofs/script-gameflow`, `proofs/ini-activate-quest`).

### First `004189C2` is dummy

`WorldMap+156=0`. `004FC180` dummy 88-byte slot. `[record+36]=0`.
`CurrentRegion=null`. Type-1 `004A5A40` first-seen
`[world+248]=0` / `[world+260]=0` skips `004A3740` (the
*other* `00502500` region path). After `009AC9E0`,
`[game+8]==0` so the inner loop repeats. Second host `Pump`
is that iteration, **not** `00501450`. Loop exit is
`WM_DESTROY` only. **PROVEN.**

---

## 2. `00501450` itself

`listing-00500000.txt` `00501450`…`00501985` `ret`. Real
prologue (`push ebp` / `and esp,-8` / `sub esp,120`).
`int3` pad before it. `ecx` = `CWorldMap`.

```
00449970 / 00487DC0           player thing (may miss)
004FEEC0(current, 0)          +156=0
count = (end-begin)/88        imul 0x2E8BA2E9
count<=1 → 005018F8
else for i=1 .. count-1
  00500540(i, 0, 0)           site 005014EC
  0048D400  bit 0x64
  005198B0  CTCActionUseScriptedHook
RegionGraph.txt  0x124467C
00500540(saved, 0, 1)         site 00501935  no sync pump
ret  00501985
```

Next function is **`00501990` UpdateNavMaps** (`int3` gap).
`functions.tsv` size 2248 on `00501450` **swallows**
`00501990` / `00501D30` — that merge is a dump heuristic, not
a call. `006C2120` at `00501C0E` is **UpdateNavMaps**, not
the `00501450` loop.

### Callers of `00501450`

| Kind | Hits | Class |
|---|---|---|
| `e8.tsv` dest `0x00501450` | **0** | **PROVEN** |
| `E9` / listing `call 00501450` / `jmp 00501450` | **0** | **PROVEN** |
| imm / vtbl dword `0x00501450` (`xrefs.tsv`) | **0** | **PROVEN** |
| `004162B5` / `00418289` / `004189C2` | not a callee (`0049D9E0` is `ret`) | **DISPROVEN** |
| `00DBDE40` | 0 `E8` of `00501450`; sole `E8` of `00DBDE40` is `00DAC295` | **DISPROVEN** |
| persist `00487C20` | `E8` `00500540` at `00487C55`, **not** `00501450` | **DISPROVEN** as no-save |

**Exact first `E8` site after Gameflow: UNREAD.** The body is
recovered; who jumps to `00501450` is not in the PE image as a
direct call or stored pointer. Do not invent a second-`Pump`
callee.

### Callers of `00500540` (`e8.tsv`)

| Site | Parent | First-seen no-save |
|---|---|---|
| `005014EC` / `00501935` | `00501450` | body recovered; parent UNREAD |
| `00487C55` | `00487C20` persist `PlayerRegionName` | empty no-save — **skip** |
| `0050255D` / `005025F8` | `00502500` | from `004A4CB9` inside `004A3740`; first-seen `[world+260]=0` **skip** |
| `00506455` | `005064C0` Post Region Load Villages | after a job already applied |

So the **only** recovered no-save path that would *first* open
index 1 is the `00501450` loop. Continue-save and the
`004A3740` state machine are different callers.

---

## 3. What the first real open loads (Lookout, not Oakvale)

`00500540(1,0,0)`:

```
record = [WorldMap+44] + 1*88
[+36] may be null → 006BB2F0 then still 006C27A0   PROVEN
006C27A0  job+16 = ContainsMap vector (stride 28)
          job tree via 006B9E00
          job+28 = 1
006C2120  enqueue [WorldMap+188]+20
sync: while 006C20A0  006C2710 → 006C2170 → 006C2BA0
```

WLD `NewRegion 1` ContainsMap order (**file bytes**, not wiki):

1. `BowerstoneBridge`
2. `LookoutPoint`
3. `GuildExterior`

SeesMap / BWD neighbours are **not** extra `006C2170` jobs.

### First LEV

| Sense | Site | What |
|---|---|---|
| Name in WLD | `00507C30` `LevelName` / `ContainsMap *.lev` | `LookoutPoint.lev` first `NewMap` | **PROVEN** parse, **before** `0049F180` |
| Apply after dummy | `006C2170` pass 1 `[rec+4]` `"Loading topology"` | `vtbl+24` `004FF080` → alloc `0x1D40` `008224E0`; `00638310`; `vtbl+28` `004FF440` | **PROVEN** |
| Compiled header `00B3EFA0` | STB-hit `00B41E50` / miss `00B42530` | first-seen `FinalAlbion.stb` **miss**; `00B3EFA0` **not** this Init Game | **PROVEN** skip |
| Host `LevelLibrary.LoadCompiledLev("LookoutPoint")` | later Present / landscape | WAD blob; not an `E8` of `00501450` | **PARTIAL** vs native topology object |

`004FF080` is a **grid / topology object**, not `CNavQuadTree`.
First-seen job `+12=0` skips `00500230` / `0050AF10`. **PROVEN.**

### First TNG

| Sense | Site | What |
|---|---|---|
| First file *opened* after Leave | `004FDBC0` / `004FBF60` / `004FAFF0(".tng")` | **`LookoutPoint.tng`** (`Maps[0]`, skip slot 0) | **PROVEN**; **before** `0049F180` |
| First `NewThing` in that file | `MARKER_BASIC` `M_Maze` (section Gameflow) | parse | **PROVEN** |
| That parse is `0051FD80` | host: no; native `[manager+128]==1` | **DISPROVEN** host; **UNREAD** live |
| First *constructed* CThing | `006C2170` pass 2 ContainsMap[0] | **`TRACK_NODE_BASIC` `GuardTrack`** from **`BowerstoneBridge.tng`** | **PROVEN** |
| First Lookout `0051FD80` | after those 88 Bridge things | `MARKER_BASIC` `M_Maze` | **PROVEN** order |
| Hero | after all three maps | `HOLY_SITE_PLAYER_START` **`GuildArrivalHSP`** → `006AC910` `CREATURE_HERO` **4299** | **PROVEN** |
| Kid / Oakvale TNG | `StartOakValeWest` / `NOVStartHSP` | **not this job** | **DISPROVEN** |

Lookout TNG has **no** `PlayerCreature`. **PROVEN.**

---

## 4. Host `LoadFromFirstRealRegion`

`EngineLifecycle.LoadFromFirstRealRegionFn = 0x00501450`.
Comment on the constant already records 0 `E8`/`E9`/imm/vtbl.

```
LoadFromFirstRealRegion
  Note 00501450 count/saved
  004FEEC0(saved,0)
  for i=1 .. Regions.Count
    RequestLoadRegion(i, sync: true)   // 00500540(i,0,0)
  RegionGraph.txt
  00500540(saved,0,1)
```

`RequestLoadRegion` notes `006C27A0` / `006C2120` then
`ApplyLoadJob`: topology then objects in **ContainsMap**
order, nav skip, `004FCBB0`, `005064C0` then `004FC8A0`.
Does **not** invent `StartOakVale`. First `i=1` is
`RegionAtNativeIndex(1)` = `World.Regions[0]` =
**LookoutPoint**.

`EnqueueAfterDummy` on the second `Pump` is **DISPROVEN**
(`Second_pump_004189C2_loops_inner_not_00501450`). Tests that
need Lookout TNG / hero call `LoadFromFirstRealRegion`
**explicitly** after dummy (`Second_pump_00501450_is_004FEEC0_then_00500540_1_0_0`).
That is a **host site**, not a recovered native `E8`.

A full native loop leaves `+156=141` `Filler_NorthernWastes_02`.
Host tests that assert that last write are pairing the **whole**
`00501450` body, not “only index 1”. Invented “only Lookout”
as the entire function is **DISPROVEN**. First *open* is still
`i=1` Lookout.

---

## 5. Not these

| Candidate | Why not first real region after Leave |
|---|---|
| Dummy index 0 | `005066E0` empty 88-byte slot. No `RegionName`. |
| `StartOakVale` / West / `00DBDE40` | WLD index **4** / later `S_QNOVI` slot 2 (`00DAC295` only). |
| `RegionTravel.StartingRegion` / `NewGameRegion` | hardcodes `StartOakValeWest`. Unused on Leave / `00416953`. **LEFTOVER** |
| `00416953` / `00507C30` | fills tables + global TNG parse. **Not** `006C2170`. |
| `0049F180` | bind / GUI / Init Quests. No spawn, no job. |
| `004B4260` / Gameflow | quest factories + Main watcher. Empty `UpdateNavMaps`. |
| `00501990` at Init Quests | `+144` empty. **DISPROVEN** as a load. |
| `00487C20` | continue persist name. No-save empty. |
| `00502500` / `004A3740` | first-seen `[world+260]=0` skip. |
| `userst.ini` `SetStartingHolySite("NOVStartHSP")` | **before** frontend; store only; miss at `0049F180`. |
| Host second `Pump` | inner `004189C2` iteration. **DISPROVEN**. |

---

## Classifications (short)

1. **First region name after Leave — `LookoutPoint` (native 1). PROVEN.**
   Oakvale is `NewRegion 4`.
2. **`00501450` body — PROVEN.** First `00500540(i,0,0)` is `i=1`
   Lookout; loop continues through 141.
3. **`00501450` `E8` caller after Gameflow — UNREAD (0 hits).**
   Not Init Game suffix, not first/second `004189C2`, not
   `00DBDE40`. Host `LoadFromFirstRealRegion` is the stand-in.
4. **First LEV apply — `006C2170` pass 1 `004FF080` on that
   Lookout job. PROVEN.** First *name* is earlier WLD parse.
   First compiled `00B3EFA0` is **not** first-seen.
5. **First TNG apply — same job pass 2, Bridge then Lookout then
   Guild. PROVEN.** First *file open* is global `LookoutPoint.tng`
   before `0049F180`. Hero `GuildArrivalHSP` is after all three.
6. **Oakvale as first region — DISPROVEN.**

---

## Open

| Item | Class |
|---|---|
| Who actually transfers control to `00501450` (computed ptr / unread vtbl write / never) | **UNREAD** |
| Live `[manager+128]` on first `005223F0` (does global TNG construct?) | **UNREAD** |
| Whether a later type-1 `004B3CE0` / `00501990` runs after `004FF080` fills `+144` *before* first Present | **UNREAD** |
| `008224E0` vs WAD compiled `.lev` bytes | **PARTIAL** |
| Host `List<int>` job vs native 32-byte job + stride-28 vector | **PARTIAL** |
