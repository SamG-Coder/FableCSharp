# Host `LoadGlobalThingsFile` vs native `004FDBC0` parse-only

Investigation only. Production `src/` was not edited.

Do **not** start at Oakvale / `00DBDE40` / `StartOakVale` /
`StartOakValeWest` / `CREATURE_HERO_CHILD`. No-save New Game is
message **15** → Leave `0042F2A2` → `FinalAlbion.wld` →
Loading world `004A1840` → `00507C30`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**DIVERGE** / **MATCH**.

Question: host `LoadGlobalThingsFile` vs native `004FDBC0`
parse-only. Does the host construct Things **too early**?
First Graphic?

Authority: `EngineLifecycle.LoadGlobalThingsFile`;
`proofs/tng-first-after-leave`, `proofs/004FDBC0-open`.
Siblings: `004FDBC0-vs-host`, `lookout-marker-graphic`,
`lookout-tng-walk`, `tng-first-def`, `first-0051FD80-file`,
`tng-spawn`, `tng-after-leave`.

Sources: `listing-004c0000.txt` (`004FDBC0` / `004FBF60` /
`004FAFF0`), `listing-00500000.txt` (`00507C30` / `00509859` /
`005223F0` / `00521AE0` / `0051FD80`),
`src/Fable.Game/EngineLifecycle.cs` (`LoadWorldMap` /
`LoadGlobalThingsFile` / `LoadRegionMapThings` /
`LoadSingleThing` / `InsertThing` / `EnsureLevels`),
`src/Fable.Game/LevelLibrary.cs` (`TryLoadThings`),
`src/Fable.Formats/Tng/ThingFile.cs`,
`src/Fable.Formats/Defs/GameBin.cs` (`FindMeshId` /
`FirstSeenInstancesAsC3d`),
`TlcInstallTests.Tng_text_parser_reads_sample`,
`GameBinFormatTests.Markers_and_cameras_do_not_resolve_to_editor_meshes`,
`WorldGeometryTests` (`FirstSeenInstancesAsC3d("THING",
"TRACK_NODE_BASIC")`).

---

## Verdict

**No. Host does not construct Things too early.**

`LoadGlobalThingsFile` is the `004FDBC0` arm. It **parses**
proximity `.tng` into `GlobalThings` and **does not**
`LoadSingleThing` / `InsertThing`. Native `004FDBC0` **always
opens**; it constructs **only if** `005223F0`
`[thing_manager+128]==1`. That gate is **UNREAD** live; the
working New Game model is parse-only (first-seen
`00416392==0` after Init Game). Host skip **MATCH** that
model.

**No Graphic is bound on this site.** First Graphic-bearing
def in the first opened file is later `OBJECT_SILVER_KEY`
mesh **7934** (Lookout file order; still not constructed
here). First Graphic that **is** constructed is later
`OBJECT_STREETLAMP_LIT_SINGLE_01` mesh **4978** from
`BowerstoneBridge.tng` after eight `TRACK_NODE_BASIC`.

| Claim | Class |
|---|---|
| Host site is `LoadGlobalThingsFile` (`004FDBC0` arm) | **PROVEN** |
| Native this VA is parse/open, not `0051FD80` unless `+128==1` | **PROVEN** dump; live gate **UNREAD** |
| Host first file is `LookoutPoint.tng` (`Maps[0]`) | **PROVEN** |
| Host **constructs** here (`LoadSingleThing` / `InsertThing`) | **DISPROVEN** |
| Host `GlobalThings` leak into `RegionThings` / first scene | **DISPROVEN** |
| Host constructs **too early** vs native skip | **DISPROVEN** (**MATCH** skip) |
| Host constructs too early vs a **taken** `+128==1` | would be the **opposite** (host later); live **UNREAD** |
| First Graphic on this call | **none bound** **PROVEN** |
| First Graphic-bearing def in first **opened** file | `OBJECT_SILVER_KEY` **7934** **PROVEN** (not constructed here) |
| First Graphic **constructed** after dummy pumps | Bridge `OBJECT_STREETLAMP_LIT_SINGLE_01` **4978** **PROVEN** |
| `EnsureLevels` WAD + `_RT.stb` inside the same host call | **DIVERGE** (I/O side effect; **not** construct) |

Open ≠ parse-depth ≠ construct. Do not collapse them.

---

## Host `LoadGlobalThingsFile` (`004FDBC0` arm)

`LoadWorldMap` is `004A1840` after Startup WAD:

```
WorldFile.Load(FinalAlbion.wld)     // 00507C30 tokens
LoadGtngFile()                      // 0050959F stem+.gtng  TLC miss
LoadGlobalThingsFile()              // 00509859 → 004FDBC0   ← THIS
LoadRegionGraphFile()               // 00509982
SetStaticMapFileForUse()            // AFTER the TNG open
```

`SingleGlobalThingsFile` default is false
(`DefaultSingleGlobalThingsFlag=0`, `[0x13B8609]=0`). The
`.gtg` arm `004FE2A0` is **not** no-save.

```
EnsureLevels();
var loaded = new List<ThingInstance>();
foreach (var map in World.Maps)              // Maps[0] = NewMap 1
{
    if (!map.LoadedOnPlayerProximity)
        continue;
    var tng = _levels?.TryLoadThings(map.ScriptName);
    if (tng is null)
        continue;
    loaded.AddRange(tng.Things);
    GlobalThingMapsLoaded++;
}
GlobalThings = new ThingFile {
    Version = 2,
    Sections = [new ThingSection { Name = "GLOBAL", Things = loaded }]
};
```

What this method **does**:

- Walks `World.Maps` with `LoadedOnPlayerProximity` (C# list
  has **no** dummy slot 0; `Maps[0].Index==1`
  `LookoutPoint`).
- `TryLoadThings` = `ThingFile.Parse` (loose then WAD). TLC
  has no loose `LookoutPoint.tng`; bytes are
  `FinalAlbion.wad`.
- Concatenates ~**21746** `ThingInstance`s from **151** prox
  maps into one section named `GLOBAL`.

What this method **does not**:

- Call `LoadSingleThing` (`0051FD80`).
- Call `InsertThing` / `_regionThings.Add`.
- Call `Allocate Class` / `0052AC90` / `0052B880`.
- Bind `game.bin` Graphic / `FindMeshId`.
- Feed `GlobalThings` into any later construct. The only
  `GlobalThings` writes are this method (and the unused
  `.gtg` arm). `LoadRegionMapThings` re-parses ContainsMap
  files into `_regionThings`.

Census (host parse, TLC): **151** / **21746**. First
increment: `"LookoutPoint"`, **288** things, first
`NewThing` `MARKER_BASIC` `M_Maze`.

---

## Native `004FDBC0` is parse-only (construct gated)

`00509859` `"Load global things"` → `[0x13B8609]==0` →
`call 004FDBC0` (`ecx=CWorldMap`, **no** stack args).

```
004FDBC5  esi = ecx
004FDBDE  ebx = 1                       // skip dummy 0
004FDBF2  edi = 0x48                    // stride 72
          if [slot+36] && [slot+40]:    // filled + prox
004FDC71    push ebx
            call 004FBF60               // one map file
```

First hit: `ebx=1`, WLD `NewMap 1` `LevelScriptName
"LookoutPoint"`. `004FAFF0` appends `0x12442C4` `".tng"`.
Stream: first-seen `[map+168]==0` → `00BFEA1A(28)` +
`0099AD80` / `CreateFileW`. Then `005223F0`.

```
005223F7  eax = [esi+128]
005223FF  cmp eax, 1
00522407  jne 00522502                  // drop shared_ptr
0052249F  call 00521AE0                 // token walk
005224AB  call 0051E2F0
```

| Layer | Always? |
|---|---|
| Path `LookoutPoint.tng` | **yes** |
| Open `0099AD80` / `00A39D80` | **yes** |
| `00521AE0` `NewThing` walk | **only if** `[+128]==1` |
| `0051FD80` / `00A371C0` | same gate |
| Else | open + drop; no CThing |

`004FDBC0` does **not** call Allocate Class. Construct of
Lookout is later `006C2170` ContainsMap[1]
(`proofs/first-0051FD80-file`).

---

## Does host construct too early?

Three senses. Do not collapse them.

| Sense | Host this call | Native this call | Too early? |
|---|---|---|---|
| **File I/O** | `TryLoadThings("LookoutPoint")` first | `004FBF60(1)` `.tng` first | **no** — **MATCH** |
| **Token parse** | always `ThingFile.Parse` 288 then 150 more | `00521AE0` iff `+128==1` | host **deeper**, not earlier construct. **PARTIAL** |
| **CThing construct** | **no** | gated **UNREAD** | **no** vs skip (**MATCH**). vs taken gate host is **later**, not earlier |

“Too early” would mean host `0051FD80` / `InsertThing` /
Graphic bind **during** `00507C30`, before dummy pumps /
`00501450`. That path is **absent**. `RegionThings` stays
empty until `LoadRegionMapThings` on the later ContainsMap
walk.

If the unread `+128==1` branch **were** live, native would
construct Lookout `M_Maze` **during** `00416953`, before
dummy pumps. Host would then be **late**, not early. Do
**not** “fix” host by calling `LoadSingleThing` here to
match that unread branch. First-seen `00416392==0` is
**PARTIAL** evidence native did not fill countable Things.

Construct timing that **is** taken (working model):

```
00416953  Loading world
  00507C30
    004FDBC0                         ← THIS: parse/store only
      LookoutPoint.tng               first open
      … 150 more prox maps …
    00509982  region graph
  Set Static Map                     AFTER
004189C2  dummy pumps  0 things
00501450 → 00500540(1,0,0) Lookout region
  006C2170  ContainsMap
    BowerstoneBridge.tng             first 0051FD80
    LookoutPoint.tng                 reopen, construct
    GuildExterior.tng
```

---

## First Graphic?

Not on this call. Three “first Graphic”s; only the last is
a constructed mesh.

### 1. This `004FDBC0` / host call — **none**

First two `NewThing`s in `LookoutPoint.tng` (Gameflow):

```
NewThing Marker;                         // #1
  DefinitionType "MARKER_BASIC"
  ScriptName     M_Maze
  // no Graphic key

NewThing Marker;                         // #2
  DefinitionType "MARKER_BASIC"
  ScriptName     M_LadyGameflow
  // no Graphic key
```

| Sense | `M_Maze` / `M_LadyGameflow` |
|---|---|
| TNG `Graphic` property | absent **PROVEN** |
| `GameBin.FindMeshId("MARKER_BASIC")` | `null` **PROVEN** |
| `FirstSeenInstancesAsC3d("MARKER","MARKER_BASIC")` | `false` **PROVEN** |

Lookout TNG text has **no** `Graphic` key on **any**
`NewThing`. Graphic is `game.bin` on the **def** at
`0051FD80` / `004CA010`. Host does not run that here.
`CAppearanceDef` 4511/4512 on `MARKER_BASIC` is an editor
gizmo, not a world Graphic.

### 2. First Graphic-bearing **def** in the first opened file

**`OBJECT_SILVER_KEY`**, mesh **7934** (Lookout file ~#13;
first `NewThing Object`). Next in that file:
`OBJECT_DEGRADABLE_THORN_VINES_01` **3977**,
`OBJECT_BS_SIGN_POST_DIRECTION_01` **4911**.

Host **parses** that def into `GlobalThings`. It does
**not** bind the mesh. Native skip does not either.

### 3. First Graphic **constructed** (later; not this VA)

After dummy pumps, ContainsMap[0] is
`BowerstoneBridge.tng`. First eight `0051FD80` are
`TRACK_NODE_BASIC` (`GuardTrack` first).
`FirstSeenInstancesAsC3d("THING","TRACK_NODE_BASIC")` is
**false**. First `NewThing Object` / first bound Graphic:

**`OBJECT_STREETLAMP_LIT_SINGLE_01`**, mesh **4978**.

Lookout `OBJECT_SILVER_KEY` is ContainsMap[1], after the
whole Bridge file. Hero **4299** is later `006AC910`, not
this TNG.

---

## MATCH / DIVERGE (this question)

### MATCH

| Site | Host | Native |
|---|---|---|
| When | inside `00507C30`, before Set Static Map | same |
| Switch | `SingleGlobalThingsFile==false` | `[0x13B8609]==0` |
| First name | `TryLoadThings("LookoutPoint")` | `004FBF60(1)` → `.tng` |
| Filter | `LoadedOnPlayerProximity` | `[slot+36] && [slot+40]` |
| Walk order | `Maps[0]…` = NewMap 1… | `ebx=1…` skip dummy 0 |
| Count | 151 / ~21746 | same census |
| Construct here | none | gated; host skip **MATCH** vs off |
| First `0051FD80` | Bridge after `00501450` | same if gate off |
| First constructed Graphic | Bridge lamp **4978** | same if gate off |

### DIVERGE / PARTIAL (not construct-early)

| Site | Host | Native | Class |
|---|---|---|---|
| `EnsureLevels` | WAD + `FinalAlbion_RT.stb` | `.tng` only | **DIVERGE** (`lev-first-after-leave`, `stb-first-open`) |
| Token walk if `+128!=1` | always `ThingFile.Parse` | open + drop | **PARTIAL** |
| Stream | WAD `Read` | `CreateFileW` | **PARTIAL** (same bytes, different FS) |
| Store | one `ThingFile` `GLOBAL` | no host-like concat; *use* **UNREAD** | **PARTIAL** |

`EnsureLevels` is the real host extra during this VA, not a
false construct. Do not treat WAD+STB as native `004FDBC0`
construct.

---

## UNREAD / PARTIAL

- Live `[thing_manager+128]` on the first `005223F0`.
- Native *use* of the 21k `GlobalThings` set (UID table,
  PersonalScript, later cache). **DISPROVEN** as first-Present
  C3Ds.
- Whether native open-without-`00521AE0` still tokenizes.
- Anniversary ordinal of Lookout silver key vs TLC WAD
  compressed text (**PARTIAL**); UID/pos of first two markers
  **PROVEN**.

---

## Do not

- Report `LoadGlobalThingsFile` / `004FDBC0` as construct.
- “Fix” host by calling `LoadSingleThing` here (that would
  be **early** vs the working model).
- Bind a Graphic to `M_Maze` / `M_LadyGameflow`.
- Call first opened Graphic (`OBJECT_SILVER_KEY` **7934**)
  the first **constructed** Graphic (Bridge lamp **4978**).
- Collapse first **open** (Lookout) with first **CThing**
  (Bridge `GuardTrack`).
- Treat `EnsureLevels` WAD+STB as TNG construct.
- Start this walk at `Maps[1]` or Oakvale / `00DBDE40`.
