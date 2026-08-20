# Lookout `MARKER_BASIC` `M_Maze` Graphic (and `M_LadyGameflow`)

Investigation only. Production `src/` was not edited.

Do **not** start at Oakvale / `00DBDE40` / `StartOakVale` /
`StartOakValeWest` / `CREATURE_HERO_CHILD`. No-save New Game is
message **15** → Leave `0042F2A2` → `FinalAlbion.wld` →
LookoutPoint. Sibling `proofs/tng-first-after-leave` is the
**file open**. Sibling `proofs/lookout-tng-walk` is the
**NewThing walk** of that file. This note is **Graphic on the
first two blocks**, and whether host `LoadGlobalThingsFile`
**parse vs construct** is a **DIVERGE**.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **DIVERGE** / **MATCH**.

Question: `LookoutPoint.tng` first `NewThing` `MARKER_BASIC`
`M_Maze` — Graphic? Then `M_LadyGameflow`. Host
`LoadGlobalThings` parse vs construct **DIVERGE**?

Authority: TLC WAD `LookoutPoint.tng` (host `ThingFile.Parse`);
Anniversary loose `LookoutPoint.tng` (UID/pos MATCH);
`proofs/lookout-tng-walk`; `proofs/tng-first-after-leave`.

Sources: `src/Fable.Formats/Tng/ThingFile.cs`;
`src/Fable.Formats/Defs/GameBin.cs`
(`FindMeshId` / `FirstSeenInstancesAsC3d`);
`EngineLifecycle.LoadGlobalThingsFile` / `LoadRegionMapThings`
/ `LoadSingleThing`;
`TlcInstallTests.Tng_text_parser_reads_sample`;
`GameBinFormatTests.Markers_and_cameras_do_not_resolve_to_editor_meshes`;
`MeshFormatTests.Object_prefix_maps_bigrock_to_mesh_enum`;
`WorldGeometryTests` (`FirstSeenInstancesAsC3d("MARKER",
"MARKER_BASIC")`);
`docs/status/investigations/2026-08-18-first-scene-things.dump.txt`;
`proofs/004FDBC0-vs-host`; `proofs/004FDBC0-open`.

---

## Verdict

**No Graphic on `M_Maze`.** Second `NewThing` is
**`MARKER_BASIC` `M_LadyGameflow`**, also **no Graphic**.
Host `LoadGlobalThingsFile` **parses** those two (and the rest
of the 288) and **does not construct**. That split is
**MATCH** vs native skip, **not DIVERGE**. Do not report
`LoadGlobalThingsFile` as `0051FD80`.

| Claim | Status |
|---|---|
| First `NewThing` in `LookoutPoint.tng` is `Marker` / `MARKER_BASIC` **`M_Maze`** | **PROVEN** |
| Second is `Marker` / `MARKER_BASIC` **`M_LadyGameflow`** (not reverse) | **PROVEN** |
| TNG property `Graphic` on either | **DISPROVEN** (key absent) |
| `game.bin` Graphic / `FindMeshId("MARKER_BASIC")` | **DISPROVEN** (`null`; editor `CAppearanceDef` only) |
| `FirstSeenInstancesAsC3d("MARKER","MARKER_BASIC")` | **DISPROVEN** (`false`; dump `mesh=-`) |
| First Graphic-bearing def in this **file** is `OBJECT_SILVER_KEY` **7934** | **PROVEN** (later; not these two) |
| Host `LoadGlobalThingsFile` **parses** Lookout (`ThingFile.Parse` 288) | **PROVEN** |
| Host `LoadGlobalThingsFile` **constructs** (`LoadSingleThing` / `InsertThing`) | **DISPROVEN** |
| Host parse-without-construct vs native **skip** (`[+128]!=1`) | **MATCH** |
| Host parse-without-construct vs a **taken** `+128==1` | would be **DIVERGE**; live gate **UNREAD** |
| Working New Game: first construct of `M_Maze` is later ContainsMap[1] | **PROVEN** |
| `EnsureLevels` WAD + `_RT.stb` inside the same host call | **DIVERGE** (side effect; **not** Graphic / construct) |

`CAppearanceDef` mesh **4511/4512** on `MARKER_BASIC` is an
editor gizmo, not a world Graphic. Do not bind it to
`M_Maze`.

---

## First two `NewThing`s (file order)

Gameflow is the first section. Host sample
(`TlcInstallTests.Tng_text_parser_reads_sample`) and
Anniversary loose (UID/pos MATCH TLC dump):

```
XXXSectionStart Gameflow;

NewThing Marker;                         // #1
  UID            18446741874686296750
  DefinitionType "MARKER_BASIC"
  ScriptName     M_Maze
  Player         -1
  Position       (49.669189, 76.648438, 35.252132)
  // no Graphic key

NewThing Marker;                         // #2
  UID            18446741874686296749
  DefinitionType "MARKER_BASIC"
  ScriptName     M_LadyGameflow
  Player         -1
  Position       (50.62085, 78.385986, 35.616848)
  // no Graphic key

XXXSectionEnd;
```

**Order is `M_Maze` then `M_LadyGameflow`.** Not the reverse.
Dump `exist-all` lists holy sites / lamps **above** these two
because it groups by category, **not** file order.

`ThingInstance` has no Graphic field. `ThingFile.Parse` only
lifts `DefinitionType` / `ScriptName` / UID / Player /
`CTCPhysicsStandard` pos. A TNG `Graphic` key would sit in
`Properties`; these two blocks do not have one. Lookout TNG
text has **no** `Graphic` property on **any** `NewThing`
(`lookout-tng-walk`). Graphic is `game.bin` on the **def** at
`0051FD80` / `004CA010`, not a TNG key.

---

## Graphic? Three senses. All no.

| Sense | `M_Maze` / `M_LadyGameflow` |
|---|---|
| TNG `Graphic` property | absent **PROVEN** |
| `GameBin.FindMeshId("MARKER_BASIC")` | `null` **PROVEN** (`GameBinFormatTests`, `MeshFormatTests`) |
| `FirstSeenInstancesAsC3d("MARKER","MARKER_BASIC")` | `false` **PROVEN** |
| First-scene dump | `type=MARKER mesh=- asC3d=False submitted=False src=ContainsTng` |

`FindMeshId` returns null when `IsEditorOnly` /
`!FirstSeenInstancesAsC3d`. `MARKER_BASIC` **does** exist in
`game.bin` (`TypeName` `MARKER`) and **does** have a
`CAppearanceDef` sub-def (PARITY: editor mesh 4511/4512).
That is **not** `entry.MeshId` Graphic and **not**
`CReplaceableMeshDef`. `FindMeshId` skips it.

Dump lines (TLC install; same UIDs as the TNG sample):

```
Marker … mesh=- … def=MARKER_BASIC script=M_Maze
         uid=18446741874686296750 pos=49.669,76.648,35.252
Marker … mesh=- … def=MARKER_BASIC script=M_LadyGameflow
         uid=18446741874686296749 pos=50.621,78.386,35.617
```

`src=ContainsTng` is the **later** region apply, not the
global concat. Same bytes, still no mesh.

First def in this file that **would** bind a Graphic (if
construct ran) is later **`OBJECT_SILVER_KEY` mesh 7934**
(13th `NewThing` on Anniversary order; ordinal **PARTIAL** vs
TLC WAD compressed). Hero **4299** is **not** this TNG.

---

## Host `LoadGlobalThings`: parse vs construct

`LoadWorldMap` (`004A1840` / `00507C30`):

```
LoadGtngFile()            // 0050959F stem+.gtng  TLC miss
LoadGlobalThingsFile()    // 00509859 → 004FDBC0   ← THIS
LoadRegionGraphFile()
SetStaticMapFileForUse()  // AFTER
```

Host (`EngineLifecycle.LoadGlobalThingsFile`, per-map arm;
`SingleGlobalThingsFile` default false):

```
EnsureLevels();
foreach (map in World.Maps)                 // Maps[0] = NewMap 1
  if !LoadedOnPlayerProximity: continue
  tng = TryLoadThings(map.ScriptName)       // ThingFile.Parse
  loaded.AddRange(tng.Things)               // no LoadSingleThing
GlobalThings = ThingFile { section GLOBAL }
```

First increment: `"LookoutPoint"`, **288** things, first two
`M_Maze` / `M_LadyGameflow`. Stored on `GlobalThings` only.
`RegionThings` stays empty. **No** `LoadSingleThing`. **No**
`InsertThing`. **No** `_regionThings.Add`.

Native `004FDBC0` always `004FAFF0` `.tng` / `0099AD80`.
`00521AE0` / `0051FD80` run **only if** `005223F0`
`[thing_manager+128]==1`. Else open + drop. Live first
`[+128]` is **UNREAD**. First-seen `00416392==0` after Init
Game is **PARTIAL** evidence countable Things stayed empty.

Construct of these markers is later:

```
00501450 → 006C2170 ContainsMap
  BowerstoneBridge.tng     first 0051FD80  TRACK_NODE_BASIC
  LookoutPoint.tng  reopen
    0051FD80  MARKER_BASIC M_Maze          ← first Lookout CThing
    0051FD80  MARKER_BASIC M_LadyGameflow
```

Host `LoadRegionMapThings("LookoutPoint")` is that site:
`foreach` → `LoadSingleThing`. Still **no Graphic** on the
def.

### Is parse vs construct **DIVERGE**?

Three layers. Do not collapse them.

| Layer | Host this call | Native this call | Class |
|---|---|---|---|
| File I/O `LookoutPoint.tng` | `TryLoadThings` | `004FBF60(1)` `.tng` | **MATCH** |
| Token **parse** | always `ThingFile.Parse` 288 | `00521AE0` iff `+128==1` | **PARTIAL** (host always; native gated) |
| CThing **construct** | **no** | gated **UNREAD** | **MATCH** vs skip; **DIVERGE** vs taken `+128==1` |
| `EnsureLevels` WAD + `FinalAlbion_RT.stb` | yes | `004FDBC0` opens `.tng` only | **DIVERGE** (`lev-first-after-leave`) |

Answer to the question:

- **Parse and construct are different** on this site. Host
  parses; host does **not** construct. **PROVEN.**
- That is **not** a host/native construct **DIVERGE** under
  the working New Game model (gate off; first `0051FD80` is
  Bridge `GuardTrack`). **MATCH** skip.
- Do **not** “fix” host by calling `LoadSingleThing` here to
  match an unread `+128==1` branch.
- The real **DIVERGE** of the same call is `EnsureLevels`,
  not `M_Maze` Graphic.

If the unread gate **is** 1, native would construct `M_Maze`
**during** `00416953` (still no Graphic). Host would then
**DIVERGE** on construct timing only — still no mesh.

---

## Not these

| Candidate | Why not |
|---|---|
| Graphic 4511/4512 | `CAppearanceDef` editor; `FindMeshId` null |
| Graphic 4299 / kid 4300 | not in this TNG; hero is later `006AC910` |
| First Graphic = `M_Maze` | **DISPROVEN**; first mesh def is silver key |
| `M_LadyGameflow` before `M_Maze` | **DISPROVEN** file order |
| `LoadGlobalThingsFile` is `0051FD80` | **DISPROVEN** |
| Parse-without-construct is host/native **DIVERGE** | **DISPROVEN** vs skip; gate **UNREAD** |
| First construct file is Lookout | **DISPROVEN** (`BowerstoneBridge` after `00501450`) |
| `HOLY_SITE` / `GuildArrivalHSP` as first NewThing | 35th; Gameflow is two markers |
| Oakvale / `00DBDE40` | `NewRegion 4`; not this tree |

---

## UNREAD / PARTIAL

- Live `[thing_manager+128]` on the first `005223F0`.
- TLC WAD raw text (compressed). First two UID/pos **PROVEN**
  from TLC host parse + dump; Anniversary ordinals for later
  Graphic objects **PARTIAL**.
- Whether native open-without-`00521AE0` still tokenizes.

---

## Do not

- Bind a Graphic to `M_Maze` / `M_LadyGameflow`.
- Treat dump category groups as `NewThing` order.
- Report `LoadGlobalThingsFile` as construct **DIVERGE**.
- Collapse first **open/parse** (Lookout Gameflow) with first
  **CThing** (Bridge `GuardTrack`).
- Start at Oakvale / `00DBDE40`.
- Call `CAppearanceDef` 4511/4512 a world mesh.
