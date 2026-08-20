# Host first `0051FD80` construct file after `00501450`

Investigation only. Production `src/` was not edited.

Do **not** start at Oakvale / `00DBDE40` / `StartOakVale` /
`StartOakValeWest` / `CREATURE_HERO_CHILD`. No-save New Game is
message **15** → Leave `0042F2A2` → `FinalAlbion.wld`.

Do **not** collapse first **TNG open** (`LookoutPoint.tng` in
`004FDBC0`) with first **Load Single Thing** construct.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **MATCH** / **DIVERGE**.

Question: authority says after `00501450` the first `0051FD80`
file is `BowerstoneBridge.tng`. What is the **host** first
construct file?

Authority: `proofs/first-0051FD80-file`;
`EngineLifecycle.LoadFromFirstRealRegion` / `ApplyLoadJob` /
`LoadRegionMapThings` / `LoadSingleThing`;
siblings `tng-first-after-leave`, `tng-first-def`,
`tng-after-leave`, `tng-spawn`, `wld-first-region`;
`docs/status/investigations/2026-08-18-first-scene-things.md`
(+ dump);
`EngineLifecycleTests.Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`,
`Loading_objects_00521AE0_loads_LookoutPoint_tng`,
`Apply_006C2170_is_topology_then_objects_then_004FCBB0`.

---

## Verdict

**Host first construct file is `BowerstoneBridge.tng`.**
**MATCH** native `first-0051FD80-file`.

`00501450`’s first job is **region** LookoutPoint
(`00500540(1,0,0)`). Host `LoadFromFirstRealRegion` does the
same. The job’s object pass walks WLD `ContainsMap` **file**
order. First name is Bridge. First `LoadSingleThing` is the
first `NewThing` in that file: **`TRACK_NODE_BASIC` /
`GuardTrack`.**

Lookout is the **region**. It is the **second** construct
file. It is the first **open** (`004FDBC0`), not this
construct.

| Claim | Class |
|---|---|
| Native first post-`00501450` `0051FD80` file is `BowerstoneBridge.tng` | **PROVEN** (`first-0051FD80-file`) |
| Host first `LoadSingleThing` file is the same | **PROVEN** **MATCH** |
| First host construct is `TRACK_NODE_BASIC` `GuardTrack` | **PROVEN** (def/script; UID/pos **PARTIAL** vs TLC WAD) |
| Host `ContainsMaps` order Bridge → Lookout → Guild | **PROVEN** |
| Host first construct file is Lookout / Oakvale | **DISPROVEN** |
| Host `004FDBC0` / `LoadGlobalThingsFile` is first construct | **DISPROVEN** (parse only; no `LoadSingleThing`) |
| Dummy pumps construct Things | **DISPROVEN** |
| Host `EnqueueAfterDummy` on second `Pump` is this site | **DISPROVEN** leftover |
| Tests name the first file as Bridge | **PARTIAL** (order is in source; asserts are Contains / Va present) |

---

## Direct answers

| Question | Answer |
|---|---|
| After `00501450`, first native `0051FD80` file? | `BowerstoneBridge.tng` |
| Host first construct file? | **`BowerstoneBridge.tng`** |
| Host first `LoadSingleThing` def? | `TRACK_NODE_BASIC` (`GuardTrack`) |
| Host first **open** `.tng`? | `LookoutPoint.tng` (earlier; not construct) |
| Host Lookout construct file #? | **2** (`ContainsMap[1]`) |

---

## Host path (no-save)

```
Leave / LoadWorld
  LoadGlobalThingsFile  004FDBC0                 // PARSE only
    LookoutPoint.tng  first open                 // no LoadSingleThing
004189C2  dummy Pump  index 0
  0 things  HeroSpawned=false                    PROVEN
tests: LoadFromFirstRealRegion()                 // 00501450
  count = Regions.Count + dummy  (142)
  004FEEC0(saved,0)
  for i=1 .. 141
    first i=1  RequestLoadRegion(1, sync)
      RegionAtNativeIndex(1) = LookoutPoint
      006C27A0 / 006C2120 / PumpLevelLoader
      ApplyLoadJob(1)
        foreach ContainsMaps  Loading topology
          BowerstoneBridge, LookoutPoint, GuildExterior
          ActivatedMaps add in that order
        foreach ContainsMaps  Loading objects
          [0] LoadRegionMapThings("BowerstoneBridge")
                ThingFile  88
                foreach tng.Things
                  LoadSingleThing                    ← FIRST 0051FD80
                    "Load Single Thing 2 TRACK_NODE_BASIC"
                    00A371C0 / Construct / InsertThing
          [1] LookoutPoint.tng  288
          [2] GuildExterior.tng  88
        SpawnHeroFromPlayerStart  after the three
  … i=2..141  later jobs, not first file
  00500540(saved,0,1)  restore, no pump
```

`LoadSingleThing` is **only** called from
`LoadRegionMapThings`. That method is **only** called from
`ApplyLoadJob`’s Loading objects pass. No other host site
Notes `0051FD80`.

---

## 1. Authority native file

`proofs/first-0051FD80-file`: after dummy, first `0051FD80`
is `BowerstoneBridge.tng`. First `NewThing` is
`TRACK_NODE_BASIC` `GuardTrack`. File order Bridge →
Lookout → Guild. Lookout in `00507C30` is parse only.

This note does **not** re-list `0051FD80` / `006C2170`.
It asks whether the host walk hits the same first file.

---

## 2. Host `00501450` → first job, not first file

`LoadFromFirstRealRegion` (`LoadFromFirstRealRegionFn =
0x00501450`):

```
Note 00501450 count/saved
00449970 / 00487DC0
004FEEC0(saved,0)
if count<=1 return
for i=1 .. count-1
  00500540(i,0,0)  RequestLoadRegion(i, sync:true)
RegionGraph.txt
00500540(saved,0,1)  no pump
```

First taken index is **1** = LookoutPoint
(`wld-first-region`, `World.Regions[0]`). That is the
**region job**. It does not name a `.tng`.

`EnqueueAfterDummy` also calls this body, but it is **not**
on the second `Pump` (`dummy-pumps-before-region`). Tests
call `LoadFromFirstRealRegion` after dummy pumps. **MATCH**
body; site **DIVERGE** vs unread native E8.

---

## 3. Host object pass is `ContainsMaps` order

WLD `NewRegion 1` (`WorldFile.Parse` `ContainsMaps.Add`):

```
ContainsMap BowerstoneBridge.lev
ContainsMap LookoutPoint.lev
ContainsMap GuildExterior.lev
```

Dump: `ContainsMap (3): BowerstoneBridge, LookoutPoint,
GuildExterior`. Counts 88 + 288 + 88.

`ApplyLoadJob`:

```
foreach (var map in region.ContainsMaps)
    Loading topology + _activatedMaps.Add
foreach (var map in region.ContainsMaps)
    "Loading objects " + map
    LoadRegionMapThings(map)
```

No sort-by-kind. SeesMap / Picnic / Greatwood are **not**
this first job. `ActivatedMaps[0]` is Bridge (topology
add). Tests only `Assert.Contains` the three names —
order is still the WLD list.

---

## 4. Host `LoadSingleThing` is the construct

`LoadRegionMapThings`:

```
00522720 / 00521AE0
TryLoadThings(ScriptName) else FileStem
_regionThings.AddRange(tng.Things)
_thingsByMap[mapName] = loaded
foreach thing in loaded
    LoadSingleThing(thing)
0051E5A0
```

`ThingFile.Things` is `Sections.SelectMany` — file
`NewThing` / `EndThing` order. Bridge census: Object 67,
TrackNode **8** first in file, then lamps.

`LoadSingleThing` Notes `0051FD80` `"Load Single Thing 2 "
+ DefinitionType`, then Allocate / Construct /
`InsertThing` (or `PlayerCreature` bind). Bridge has no
`PlayerCreature`.

First host construct:

```
NewThing TrackNode
  DefinitionType "TRACK_NODE_BASIC"
  ScriptName     GuardTrack
```

Then ×8 track nodes, then first Object
`OBJECT_STREETLAMP_LIT_SINGLE_01`. Same as
`tng-first-def`.

Lookout construct is the **next** `LoadRegionMapThings`.
First thing there is `MARKER_BASIC` `M_Maze`.
`GuildArrivalHSP` is later in that file. Hero `006AC910`
is after all three maps. **DISPROVEN** as first file.

---

## 5. What host does *before* `00501450`

| Site | Host | Construct? |
|---|---|---|
| Create Players | type table | no |
| `LoadGlobalThingsFile` | concat prox `ThingFile`s into `GlobalThings` | **no** `LoadSingleThing` |
| Dummy `Pump` index 0 | `005066E0` empty | 0 things |
| `InitCharacters` at world load | player / GUI / QST | no `0051FD80` |

So the first host `0051FD80` cannot be Lookout’s global
parse. That is `tng-first-after-leave` (open).

---

## Host vs native

| Site | Host | Native | Class |
|---|---|---|---|
| Dummy | 0 things | same | **MATCH** |
| `00501450` | explicit `LoadFromFirstRealRegion` | body **PROVEN**; E8 **UNREAD** | **MATCH** body |
| First job | `00500540(1,0,0)` Lookout | same | **MATCH** |
| Object files | `foreach ContainsMaps` | `006C2170` pass 2 stride 28 | **MATCH** |
| First construct file | `BowerstoneBridge.tng` | same | **MATCH** |
| First def | `TRACK_NODE_BASIC` | same if global `+128` off | **MATCH** |
| Global parse | no `LoadSingleThing` | `005223F0` gated **UNREAD** | **MATCH** skip |
| Later `i=2..141` | host constructs those jobs too | loop **PROVEN**; first file still Bridge | **MATCH** loop; first file unchanged |

`Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`
proves `0051FD80` Notes exist and hero is Lookout HSP.
It does **not** assert the first Note’s map. File identity
is the `ApplyLoadJob` / `ContainsMaps` walk, not that
assert list.

---

## Not these

| Candidate | Why not host first construct file |
|---|---|
| `LookoutPoint.tng` | first **open**; second **construct** |
| `GuildExterior.tng` | ContainsMap[2] |
| `StartOakValeWest.tng` | NewRegion 4; not this tree |
| Picnic / Greatwood / fillers | SeesMap or later `i` |
| `FinalAlbion.gtng` / `.gtg` | miss / flag 0; no `LoadSingleThing` |
| Dummy index 0 | no objects pass |
| `GuildArrivalHSP` / `CREATURE_HERO` | Lookout file / after file 3 |
| `MARKER_BASIC` `M_Maze` | first Lookout construct, not first overall |

---

## UNREAD / PARTIAL

- Native `00501450` first E8 (body recovered).
- Live `[manager+128]` on first `005223F0` (would be a
  **pre-`00501450`** Lookout `M_Maze` construct). Host
  skip **PROVEN**.
- TLC WAD vs Anniversary `GuardTrack` UID/pos.
- Test suite does not pin `ActivatedMaps[0]` or the first
  `"Load Single Thing 2"` string. Source order is enough
  for the file claim.

---

## Do not

- Report host first construct as Lookout because
  `FirstSceneMapName` / the **region** is LookoutPoint.
- Treat `LoadGlobalThingsFile` as `LoadSingleThing`.
- Skip Bridge because first **open** or first Present C3D
  is Lookout.
- Bind `EnqueueAfterDummy` to the second `Pump`.
- Bind Oakvale / `00DBDE40` / kid as this file.
- Move `GuildArrivalHSP` construct to `006AC910`.
