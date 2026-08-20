# First TNG-site DIVERGE is `EnsureLevels` (not construct)

Investigation only. Production `src/` was not edited.

Do **not** start at Oakvale / `00DBDE40` / `StartOakVale` /
`StartOakValeWest` / `CREATURE_HERO_CHILD`. No-save New Game is
message **15** → Leave `0042F2A2` → `FinalAlbion.wld` →
Loading world `004A1840` → `00507C30` → `00509859` →
`004FDBC0`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**DIVERGE** / **MATCH**.

Question: host `EnsureLevels` opens `FinalAlbion.wad` +
`FinalAlbion_RT.stb` **before** `LookoutPoint.tng`. Native
`004FDBC0` opens `.tng` only. Confirm the first **TNG-site**
DIVERGE and whether any construct happens.

Authority: `Fable.exe` (`listing-004c0000.txt` `004FDBC0` /
`004FBF60` / `004FAFF0`; `listing-00500000.txt` `00507C30` /
`00509859` / `005223F0`); `proofs/leave-tng-first-diverge`.

Siblings: `004FDBC0-vs-host`, `004FDBC0-open`,
`host-tng-construct-early`, `lev-first-after-leave`,
`stb-first-open`, `tng-first-after-leave`.

Sources: `EngineLifecycle.LoadWorldMap` /
`LoadGlobalThingsFile` / `EnsureLevels` / `LoadSingleThing`,
`LevelLibrary` ctor / `TryLoadThings`,
`GameInstall.WadPath` / `RuntimeStbPath`,
`EngineLifecycleTests.Gtng_is_stem_gtng_gtg_is_004FE2A0_single_file`.

---

## Verdict

**First TNG-site DIVERGE is `EnsureLevels`. No construct.**

Host `LoadGlobalThingsFile` is the `004FDBC0` arm. The first
line of that arm is `EnsureLevels()`: if `_levels` is null,
`new LevelLibrary(Install, World)` opens
`Data\Levels\FinalAlbion.wad` and
`Data\Levels\FinalAlbion_RT.stb`. Only then does
`TryLoadThings("LookoutPoint")` run. Native `004FDBC0` walks
map slots and `call 004FBF60` — path + `.tng` + open. It
does **not** open WAD or `_RT.stb`.

Construct does **not** happen on this site. Host never
calls `LoadSingleThing` / `InsertThing` here. Native
`00521AE0` / `0051FD80` run only if `005223F0`
`[thing_manager+128]==1`. Live gate **UNREAD**; working
New Game model is parse/open-only. Host skip **MATCH**.

Not Oakvale. First `.tng` name on both sides is
`LookoutPoint.tng` (WLD `NewMap 1` / C# `Maps[0]`).

| Claim | Class |
|---|---|
| Host site is `LoadGlobalThingsFile` (`004FDBC0` arm) | **PROVEN** |
| First extra of that arm is `EnsureLevels` | **PROVEN** |
| Host ctor opens `FinalAlbion.wad` + `FinalAlbion_RT.stb` **before** first `.tng` | **PROVEN** |
| Native `004FDBC0` opens `.tng` only (no WAD, no `_RT.stb`) | **PROVEN** dump |
| That pair is the first **TNG-site** DIVERGE after Leave WLD | **PROVEN** |
| Host constructs here (`LoadSingleThing` / `InsertThing`) | **DISPROVEN** |
| Native this VA constructs unless `[manager+128]==1` | **DISPROVEN** dump; live gate **UNREAD** |
| Host construct vs native skip | **DISPROVEN** (**MATCH** skip) |
| First `.tng` is `LookoutPoint.tng` | **PROVEN** |
| First file is `StartOakVale*.tng` / `00DBDE40` | **DISPROVEN** |
| Token-walk depth (`ThingFile.Parse` vs open-and-drop) | **PARTIAL** (not first DIVERGE) |
| Stream WAD `Read` vs `CreateFileW` | **PARTIAL** (same bytes, different FS) |

Open ≠ parse-depth ≠ construct. Do not collapse them.

This note is **not** “first DIVERGE of the whole Leave
tree”. QST pairing and host Startup WAD being Note-only
sit **earlier** in `004A1840`. They are not this TNG site.

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
            host EnsureLevels()                  ← FIRST TNG-SITE DIVERGE
              FinalAlbion.wad                    // native 004FDBC0 does not
              FinalAlbion_RT.stb                 // native names .stb later; miss
            LookoutPoint.tng                     ← first .tng  MATCH
              host: ThingFile.Parse, concat GLOBAL
              native: open; 0051FD80 iff +128==1  UNREAD
            … 150 more prox maps …
        00509982  region graph
      004A1BD3  Set Static Map                   // AFTER; FinalAlbion.stb miss
004189C2  dummy pumps  0 things
later 00501450 → 006C2170 ContainsMap
  BowerstoneBridge.tng                           // first 0051FD80
```

Frontend / `Bootstrap` / `RequestNewGame` never call
`EnsureLevels`. First host hit is this
`LoadGlobalThingsFile` line, still inside `00507C30`,
**before** Set Static Map.

---

## Native `004FDBC0` opens `.tng` only

`00509859` `"Load global things"` → `[0x13B8609]==0` →
`00509946` `mov ecx, ebx` / `call 004FDBC0`. Only xref
in the listing. Default flag 0 (`DefaultSingleGlobalThingsFlag`).
`.gtg` `004FE2A0` is **not** no-save.

`Fable.exe` (`listing-004c0000.txt`):

```
004FDBC0  sub esp, 8
004FDBC5  mov esi, ecx                // CWorldMap
004FDBC7  eax = [esi+32], ecx = [esi+36]
          count = (end-begin)/72      // 0x38E38E39
004FDBDE  mov ebx, 1                  // skip dummy 0
004FDBF2  mov edi, 0x48               // stride 72
004FDC00  push "Loading global things"  // 0099EBF0 / 009E9F40 / 0099EAE0
004FDC60  if [slot+36] && [slot+40]:    // filled + prox
004FDC71    push ebx
            call 004FBF60               // one map file
004FDC90  inc ebx; add edi, 72
004FDCA8  ret                         // no stack args
```

Callees of this fn: progress strings + `004FBF60`. **No**
`00A39D80` WAD, **no** `00B42750` / `00B428E0` STB, **no**
`00A371C0` Allocate Class.

`004FBF60` first hit: `ebx=1`, slot+24 script
`LookoutPoint`. `004FAFF0` `push 0x12442C4` (`TngExtVa`
`".tng"`). Then `[map+168]==0` first-seen → `0099AD80`
`CreateFileW`, **not** WAD `00A39D80`. Then `005223F0`.

```
005223F7  eax = [esi+128]
005223FF  cmp eax, 1
00522407  jne 00522502                  // drop shared_ptr
0052249F  call 00521AE0                 // token walk
```

| Layer | Native `004FDBC0` |
|---|---|
| Path `LookoutPoint.tng` | **always** |
| Open `0099AD80` / `00A39D80` | **always** (first-seen = disk) |
| WAD `FinalAlbion.wad` | **no** (first-seen `+168==0`) |
| `_RT.stb` | **no** |
| `00521AE0` / `0051FD80` | **only if** `[+128]==1` |
| Else | open + drop; no CThing |

Native STB attempt is **later** Set Static Map
(`004A1BD3` → `00B428E0` → `00B42750(1)`), names
`Data\Levels\FinalAlbion.stb`, **misses**. `_RT.stb`
needs `[0x13B8616]!=0` (`build_retail_static_maps`).
First-seen does not set that byte (`stb-first-open`).

---

## Host `EnsureLevels` is the extra

```
EnsureLevels();                                  // ← first extra
var loaded = new List<ThingInstance>();
foreach (var map in World.Maps)                  // Maps[0] = NewMap 1
{
    if (!map.LoadedOnPlayerProximity)
        continue;
    var tng = _levels?.TryLoadThings(map.ScriptName);
    …
}
GlobalThings = new ThingFile { … section "GLOBAL" … };
```

`EnsureLevels`:

```
if (_levels is not null || Install is null)
    return;
_levels = new LevelLibrary(Install, World);
```

`LevelLibrary` ctor:

```
World = world ?? WorldFile.Load(install.WorldPath);  // World already set
_wad = File.Exists(install.WadPath) ? BbbArchive.Open(…) : null;
_stb = File.Exists(install.RuntimeStbPath) ? StbArchive.Open(…) : null;
```

`WadPath` = `Data\Levels\FinalAlbion.wad` (`File.OpenRead`).
`RuntimeStbPath` = `Data\Levels\FinalAlbion_RT.stb`
(`StbArchive.Open` → `File.OpenRead`). TLC ships `_RT.stb`
and **not** `FinalAlbion.stb`. World is already loaded;
this call does **not** re-parse the WLD. `Defs` /
`LandscapeEnums` stay lazy. `CurrentCompiledLev` / height
stay null at this site.

Then `TryLoadThings("LookoutPoint")`: loose miss, WAD
`Find("LookoutPoint.tng")`, `ThingFile.Parse`. Native
first-seen never opens that WAD for this walk.

Later `EnsureLevels` sites (`PresentWorld`,
`OpenStaticMapBody`, `LoadRegionMapThings` / `006C2170`)
are **after** this call; `_levels` is already set, so they
are no-ops.

| I/O | Host this call | Native `004FDBC0` | Native later Set Static Map |
|---|---|---|---|
| `FinalAlbion.wad` | ctor open | **no** | n/a (Startup WAD was **before** `00507C30`) |
| `FinalAlbion_RT.stb` | ctor open if present | **no** | **no** — names `FinalAlbion.stb`, miss |
| `LookoutPoint.tng` | after ctor | **this** VA | n/a |

---

## No construct

Host this method **does not**:

- Call `LoadSingleThing` (`0051FD80`).
- Call `InsertThing` / `_regionThings.Add`.
- Call Allocate Class / `0052AC90` / `0052B880`.
- Bind `game.bin` Graphic.

`LoadSingleThing` is only from `LoadRegionMapThings` on
the later ContainsMap walk (`006C2170`). First constructed
file after dummy pumps is `BowerstoneBridge.tng`
(`proofs/leave-tng-first-diverge`).

Native `004FDBC0` does not call Allocate Class. Construct
of Lookout is later ContainsMap[1] **if** the skip model
holds. Live `[manager+128]` **UNREAD**.

Do not treat the WAD+STB pair as TNG construct.

---

## Chronology (this TNG site)

| After Leave | Host | Native | Class |
|---|---|---|---|
| `00507C30` WLD tokens | `WorldFile.Load` | same | **MATCH** |
| `.gtng` | miss note | `0050959F` miss | **MATCH** |
| **First extra in `004FDBC0` arm** | `EnsureLevels` WAD + `_RT.stb` | `.tng` only | **DIVERGE** ← first of this site |
| First `.tng` name | `TryLoadThings("LookoutPoint")` | `004FBF60(1)` | **MATCH** |
| Filter / order | prox `Maps[0]…` | `[slot+36]&&[+40]`, `ebx=1…` | **MATCH** |
| Token walk | always `ThingFile.Parse` | iff `+128==1` | **PARTIAL** |
| CThing construct | **none** | gated skip | **MATCH** |
| Set Static Map | `FinalAlbion.stb` miss | same | **MATCH** |

---

## UNREAD / PARTIAL

- Live `[thing_manager+128]` on the first `005223F0`.
- Whether native open-without-`00521AE0` still tokenizes.
- Whether `CreateFileW` bytes == WAD
  `Find("LookoutPoint.tng")`.
- Later native hit of `FinalAlbion_RT.stb` without the
  cmdline flag (`stb-first-open`).

---

## Do not

- Report `LoadGlobalThingsFile` / `004FDBC0` as construct.
- Treat `EnsureLevels` WAD+STB as native `004FDBC0`
  construct.
- “Fix” host by calling `LoadSingleThing` here (that would
  **create** a construct DIVERGE vs the working model).
- Collapse first **open** (Lookout) with first **CThing**
  (Bridge `GuardTrack`).
- Start this walk at `Maps[1]` or Oakvale / `00DBDE40`.
- Move this DIVERGE to after Set Static Map / `00501450`.
- Call this the first DIVERGE of the whole Leave tree
  (QST / Startup WAD sit earlier).
