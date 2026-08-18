# First TNG object def after Leave

Investigation only. Production `src/` was not edited.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Question: after Leave Frontend, what is the first TNG
`DefinitionType` that `0051FD80` constructs? `ThingFile.cs`
`DefinitionType` is the name.

Sources: `src/Fable.Formats/Tng/ThingFile.cs`;
`src/Fable.Game/EngineLifecycle.cs` (`LeaveFrontendSite`,
`LoadGtngFile`, `LoadGlobalThingsFile`, `ApplyLoadJob`,
`LoadRegionMapThings`, `LoadSingleThing`);
`docs/runtime/FORWARD_TREE.md` §§9–10;
`docs/status/investigations/2026-08-18-first-scene-things.md`
(+ dump);
`EngineLifecycleTests.Loading_objects_00521AE0_loads_LookoutPoint_tng`,
`Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`;
host trace `load-single-thing-0051FD80.txt`.

---

## Verdict

**`TRACK_NODE_BASIC`.**

First `0051FD80` after Leave is the first `NewThing` in
`BowerstoneBridge.tng` (WLD NewRegion 1 `ContainsMap[0]`).
`ThingFile.Parse` puts that name in `ThingInstance.DefinitionType`.

Not Lookout’s first authored thing. Not `CREATURE_HERO`. Not
Oakvale / `00DBDE40`. Not `.gtng` / `.gtg`.

| Claim | Status |
|---|---|
| First `0051FD80` def name is `TRACK_NODE_BASIC` | **PROVEN** |
| That file is `BowerstoneBridge.tng` (88 things) | **PROVEN** |
| First `NewThing Object` def is later `OBJECT_STREETLAMP_LIT_SINGLE_01` | **PROVEN** |
| `004FDBC0` creates that instance | **DISPROVEN** (parse only; no `0051FD80`) |
| First create is Lookout / `CREATURE_HERO` / kid | **DISPROVEN** |
| TLC `FinalAlbion.gtng` | **DISPROVEN** (missing skip `0050959F`) |

---

## Path from Leave (no-save New Game)

```
0042F2A2  Leave frontend
0042F491  Init Game → 004184BD
  00416953  Loading world  FinalAlbion.wld
    00507C30  WLD
    0050959F  stem+.gtng          missing  PROVEN
    004FDBC0  proximity .tng parse
              151 maps / ~22087 things     PROVEN parse
              insert / 0051FD80            UNREAD / host skip
    0049F180  Init Characters
              00449970 / 00487DC0          no 0051FD80
004189C2  dummy pumps  (index 0, 0 things)
later 00501450(1)  LookoutPoint
  006C27A0 / 006C2710
  006C2170  index=1
    pass 1 topology: Bridge, Lookout, Guild
    pass 2 Loading objects BowerstoneBridge     // ContainsMap[0]
      00522720 / 00521AE0
      00520D00  NewThing loop
      0051FD80  Load Single Thing 2 TRACK_NODE_BASIC   ← first
        00A371C0 Allocate Class
        004CA010 TRACK_NODE_BASIC
      ×8 TRACK_NODE_BASIC
      0051FD80  OBJECT_STREETLAMP_LIT_SINGLE_01        ← first Object
    Loading objects LookoutPoint (288)
    Loading objects GuildExterior (88)
    0049F180 / 00449D90 / 006AC910 CREATURE_HERO       later
```

`ContainsMap` file order is **BowerstoneBridge, LookoutPoint,
GuildExterior** (`2026-08-18-first-scene-things.dump.txt`).
`FORWARD_TREE` “LookoutPoint first” is authored-region wording,
not this list.

---

## `ThingFile` name

`ThingFile.Parse` (`src/Fable.Formats/Tng/ThingFile.cs`):

- `NewThing <Kind>` opens a builder (`Kind` = Object / TrackNode / …)
- `DefinitionType <name>` is stored as a property
- `EndThing` → `ThingInstance.DefinitionType`

Host `LoadRegionMapThings` walks `tng.Things` (section order =
file NewThing order) and calls `LoadSingleThing`. That is the
`00520D00` / `0051FD80` pair.

BowerstoneBridge (88): Object 67, TrackNode **8**, Marker 6,
Thing 4, Building 2, Holy Site 1. The eight track nodes are
the first eight `EndThing`s.

---

## Not these

| Candidate | Why not first create |
|---|---|
| `004FDBC0` first proximity (`LookoutPoint` `Maps[0]`) | parse into `GlobalThings` only; no `0051FD80` |
| `FinalAlbion.gtg` | `[0x13B8609]=0` → per-map path |
| `FinalAlbion.gtng` | missing |
| `0049F180` at Load world | player slots / GUI / quests; hero create is after maps |
| Lookout first camera / wall | second `00521AE0` |
| `CREATURE_HERO` | `006AC910` after all three maps |
| `CREATURE_HERO_CHILD` / `StartOakValeWest` | not this tree |

---

## First Object-kind name

If “object def” means `NewThing Object` only (not every
`DefinitionType`):

**`OBJECT_STREETLAMP_LIT_SINGLE_01`**

Same file, after the eight `TRACK_NODE_BASIC`. Mesh **4978**.
Then `OBJECT_BS_BANNER_01`.

---

## UNREAD

Native `00521AE0` token walk is not re-listed here. Host
`ThingFile` order matches the NewThing-loop comment and the
eight leading track nodes in the census. A native sort-by-kind
before `0051FD80` would **DIVERGE**; no such sort is on
`006C2170` pass 2.
