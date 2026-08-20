# First TNG DIVERGE after Leave WLD (`LoadGlobalThingsFile` vs `004FDBC0`)

Investigation only. Production `src/` was not edited.

Do **not** start at Oakvale / `00DBDE40` / `StartOakVale` /
`StartOakValeWest` / `CREATURE_HERO_CHILD`. No-save New Game is
message **15** → Leave `0042F2A2` → `FinalAlbion.wld` →
Loading world `004A1840` → `00507C30` → `00509859` →
`004FDBC0`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**DIVERGE** / **MATCH**.

Question: host `LoadGlobalThingsFile` **construct** vs native
`004FDBC0` **parse-only**. What is the **first DIVERGE** after
Leave WLD?

Authority: `EngineLifecycle.LoadGlobalThingsFile`;
`proofs/tng-first-after-leave`, `proofs/host-tng-construct-early`.
Siblings: `004FDBC0-vs-host`, `004FDBC0-open`,
`lookout-marker-graphic`, `lev-first-after-leave`,
`stb-first-open`, `host-first-0051FD80`.

Sources: `listing-004c0000.txt` (`004FDBC0` / `004FBF60` /
`004FAFF0`), `listing-00500000.txt` (`00507C30` / `00509859` /
`005223F0` / `00521AE0` / `0051FD80`),
`src/Fable.Game/EngineLifecycle.cs` (`LoadWorldMap` /
`LoadGlobalThingsFile` / `EnsureLevels` / `LoadSingleThing`),
`src/Fable.Game/LevelLibrary.cs` (ctor / `TryLoadThings`),
`src/Fable.Core/GameInstall.cs` (`WadPath` /
`RuntimeStbPath`),
`EngineLifecycleTests.Gtng_is_stem_gtng_gtg_is_004FE2A0_single_file`.

---

## Verdict

**Construct is not the first DIVERGE. It is not a DIVERGE.**

Host `LoadGlobalThingsFile` is the `004FDBC0` arm. It
**parses** proximity `.tng` into `GlobalThings` and **does
not** `LoadSingleThing` / `InsertThing`. Native `004FDBC0`
**always opens**; it constructs **only if** `005223F0`
`[thing_manager+128]==1`. That gate is **UNREAD** live; the
working New Game model is parse-only. Host skip **MATCH**.

**First DIVERGE after Leave on this WLD / `004FDBC0` site is
`EnsureLevels`.** `LevelLibrary` ctor opens
`FinalAlbion.wad` + `FinalAlbion_RT.stb` **before** the first
`.tng` I/O. Native `004FDBC0` opens `.tng` only. Native STB
attempt is **later** (`Set Static Map` names
`FinalAlbion.stb` and **misses**).

Not Oakvale. First file on both sides is
`LookoutPoint.tng` (WLD `NewMap 1` / `Maps[0]`).

| Claim | Class |
|---|---|
| Host site is `LoadGlobalThingsFile` (`004FDBC0` arm) | **PROVEN** |
| First file after Leave TNG I/O is `LookoutPoint.tng` | **PROVEN** (`tng-first-after-leave`) |
| Host **constructs** here (`LoadSingleThing` / `InsertThing`) | **DISPROVEN** |
| Native this VA **is** `0051FD80` unless `+128==1` | **DISPROVEN** dump; live gate **UNREAD** |
| Host construct-early vs native parse-only | **DISPROVEN** (**MATCH** skip) |
| First **DIVERGE** of this call after Leave WLD | **`EnsureLevels`** WAD + `_RT.stb` **PROVEN** |
| That I/O is **before** `LookoutPoint.tng` | **PROVEN** (first line of the per-map arm) |
| Token-walk depth (`ThingFile.Parse` vs open-and-drop) | **PARTIAL** (not the first DIVERGE) |
| Stream WAD `Read` vs `CreateFileW` | **PARTIAL** (same bytes, different FS) |
| First file is `StartOakVale*.tng` / `00DBDE40` | **DISPROVEN** |

Open ≠ parse-depth ≠ construct. Do not collapse them.

This note is **not** “first DIVERGE of the whole Leave
tree”. QST activate pairing (`qst-first-load`) and host
Startup WAD being Note-only sit **earlier** in `004A1840`.
They are not this TNG site. First **TNG-site** DIVERGE is
`EnsureLevels`.

---

## Path from Leave (no-save)

```
0042F2A2  Leave frontend                         // no TNG
0042F44D  FinalAlbion.wld → game+90576
0042F491  Init Game
  00416953  Loading world
    004A1840
      QST / Startup WAD                          // not this TNG site
      00507C30  Load .wld file
        NewMap 1  LookoutPoint                   // Maps[0]
        0050959F  stem+.gtng  TLC miss           MATCH skip
        00509859  Load global things
          [0x13B8609]==0 → 004FDBC0              ← THIS
            host EnsureLevels()                  ← FIRST DIVERGE
              FinalAlbion.wad                    // native 004FDBC0 does not
              FinalAlbion_RT.stb                 // native names .stb later; miss
            LookoutPoint.tng                     ← first .tng  MATCH
              host: ThingFile.Parse 288, concat GLOBAL
              native: open; 0051FD80 iff +128==1  UNREAD
            … 150 more prox maps …
        00509982  region graph
      004A1BD3  Set Static Map                   // AFTER; FinalAlbion.stb miss
004189C2  dummy pumps  0 things
later 00501450 → 006C2170 ContainsMap
  BowerstoneBridge.tng                           // first 0051FD80
```

`Q_NewOakValeIntro` / `00DBDE40` never open a TNG on this
tree.

---

## Host `LoadGlobalThingsFile` is not construct

`SingleGlobalThingsFile` default is false
(`DefaultSingleGlobalThingsFlag=0`, `[0x13B8609]=0`). The
`.gtg` arm `004FE2A0` is **not** no-save.

```
EnsureLevels();                                  // ← first extra
var loaded = new List<ThingInstance>();
foreach (var map in World.Maps)                  // Maps[0] = NewMap 1
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
- Call Allocate Class / `0052AC90` / `0052B880`.
- Bind `game.bin` Graphic / `FindMeshId`.
- Feed `GlobalThings` into later construct.
  `LoadRegionMapThings` re-parses ContainsMap files into
  `_regionThings`.

So “host construct vs native parse-only” is a false pairing.
Both sides are parse/store (or open-and-drop) on this VA.
**PROVEN** host; native live gate **UNREAD**, skip is the
working model (`host-tng-construct-early`).

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
Then `005223F0`.

```
005223F7  eax = [esi+128]
005223FF  cmp eax, 1
00522407  jne 00522502                  // drop shared_ptr
0052249F  call 00521AE0                 // token walk
```

| Layer | Always? |
|---|---|
| Path `LookoutPoint.tng` | **yes** |
| Open `0099AD80` / `00A39D80` | **yes** |
| `00521AE0` `NewThing` walk | **only if** `[+128]==1` |
| `0051FD80` / `00A371C0` | same gate |
| WAD / `_RT.stb` | **no** |
| Else | open + drop; no CThing |

`004FDBC0` does **not** call Allocate Class. Construct of
Lookout is later `006C2170` ContainsMap[1]
(`proofs/first-0051FD80-file` / `host-first-0051FD80`).

---

## First DIVERGE: `EnsureLevels` (not construct)

Order **inside** the host per-map arm:

1. `EnsureLevels()` — if `_levels` is null, `new
   LevelLibrary(Install, World)`.
2. Then `TryLoadThings("LookoutPoint")`.

`LevelLibrary` ctor:

```
_wad = File.Exists(install.WadPath) ? BbbArchive.Open(…) : null;
_stb = File.Exists(install.RuntimeStbPath) ? StbArchive.Open(…) : null;
```

`WadPath` = `Data\Levels\FinalAlbion.wad`.
`RuntimeStbPath` = `Data\Levels\FinalAlbion_RT.stb`.
World is already loaded; this call does **not** re-parse
the WLD. `Defs` / `LandscapeEnums` stay lazy (not this
ctor).

Frontend / `RequestNewGame` never call `EnsureLevels`
(`stb-first-open`). First host hit is this
`LoadGlobalThingsFile` line, still inside `00507C30`,
**before** Set Static Map.

| I/O | Host this call | Native `004FDBC0` | Native later Set Static Map |
|---|---|---|---|
| `FinalAlbion.wad` | ctor open | **no** | n/a (Startup WAD was **before** `00507C30`) |
| `FinalAlbion_RT.stb` | ctor open if present | **no** | **no** — names `FinalAlbion.stb`, miss |
| `LookoutPoint.tng` | after ctor | **this** VA | n/a |

Native first-seen never names `_RT.stb` unless
`[0x13B8616]!=0` (`build_retail_static_maps`). TLC ships
`_RT.stb` and **not** `FinalAlbion.stb`. Host therefore
opens the file native does **not** open on this walk, and
opens it **before** any `.tng`. **DIVERGE.**

Do not treat that bank pair as TNG construct. No
`LoadSingleThing`. No Graphic bind. `CurrentCompiledLev` /
height stay null at this site (`lev-first-after-leave`).

---

## Chronology after Leave WLD (this question)

| After Leave | Host | Native | Class |
|---|---|---|---|
| `00507C30` WLD tokens | `WorldFile.Load` | same | **MATCH** |
| `.gtng` | miss note | `0050959F` miss | **MATCH** |
| **First extra in `004FDBC0` arm** | `EnsureLevels` WAD + `_RT.stb` | `.tng` only | **DIVERGE** ← first of this site |
| First `.tng` name | `TryLoadThings("LookoutPoint")` | `004FBF60(1)` | **MATCH** |
| Filter / order / 151 / ~21746 | prox `Maps[0]…` | `[slot+36]&&[+40]`, `ebx=1…` | **MATCH** |
| Token walk | always `ThingFile.Parse` | iff `+128==1` | **PARTIAL** |
| CThing construct | **none** | gated skip | **MATCH** |
| Set Static Map | `FinalAlbion.stb` miss | same | **MATCH** |
| First `0051FD80` | Bridge after `00501450` | same if gate off | **MATCH** |

Earlier `004A1840` leftovers (not this TNG DIVERGE):

- QST activate pairing (`World.InitialQuests` vs `world+172`)
  — `qst-first-load`.
- Host `"Startup WAD"` is a **Note** only. Native opened a
  bank **before** `00507C30`. Host WAD I/O is deferred to
  `EnsureLevels`.

---

## UNREAD / PARTIAL

- Live `[thing_manager+128]` on the first `005223F0`.
- Native *use* of the 21k set (UID table, PersonalScript,
  later cache). **DISPROVEN** as first-Present C3Ds.
- Whether native open-without-`00521AE0` still tokenizes.
- Whether `CreateFileW` bytes == WAD
  `Find("LookoutPoint.tng")`.
- Later native hit of `FinalAlbion_RT.stb` without the
  cmdline flag (`stb-first-open`).

---

## Do not

- Report `LoadGlobalThingsFile` / `004FDBC0` as construct.
- Call construct-early the first DIVERGE after Leave WLD.
- “Fix” host by calling `LoadSingleThing` here (that would
  **create** a construct DIVERGE vs the working model).
- Treat `EnsureLevels` WAD+STB as native `004FDBC0`
  construct.
- Collapse first **open** (Lookout) with first **CThing**
  (Bridge `GuardTrack`).
- Start this walk at `Maps[1]` or Oakvale / `00DBDE40`.
- Move this DIVERGE to after Set Static Map / `00501450`.
