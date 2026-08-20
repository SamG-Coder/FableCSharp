# First LEV file after no-save Leave

Investigation only. Production `src/` was not edited.

Do **not** start at `StartOakVale` / `StartOakValeWest` /
`OakValeEast_v2` / `00DBDE40`. No-save New Game is message
**15** → Leave `0042F2A2` → `FinalAlbion.wld` → `00507C30`.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**DIVERGE**.

Question: first LEV **open** after no-save Leave. Which map?
Before or after `LookoutPoint.tng` `004FDBC0`? Cell count,
heightfield, first texture? Not Oakvale LEV.

Authority: dump `00507C30` / LEV load (`00B3EFA0` /
`00B428E0`). Siblings: `proofs/lev-layout`,
`proofs/landscape-after-leave`, `proofs/tng-first-after-leave`,
`proofs/stb-first-open`, `proofs/wld-first-region`,
`proofs/landscape-submit-leave`.

Sources: `listing-00500000.txt` (`00507C30` / `005086B1` /
`00509948`), `listing-004c0000.txt` (`004FDBC0` / `004FBF60`),
`listing-00b40000.txt` (`00B3EFA0` / `00B42530` / `00B428E0`),
`landscape-trace/parsemapheader-00b3efa0.md`,
`LevFormatTests`, `LevFile.cs` / `LevHeightField.cs` /
`LevCellGrid.cs`, `EngineLifecycle.LoadWorldMap` /
`LoadGlobalThingsFile` / `PeekMapHeader`.

---

## Verdict

**`LookoutPoint.lev`.** WLD `NewMap 1` / `Maps[0]`. Not Oakvale.

`00507C30` **does not open** a `.lev`. It stores the
`LevelName` path (`0041A060` prefix `Data\Levels\`) while
walking `FinalAlbion.wld`. That string write is **before**
`004FDBC0`.

The first TNG **file** after Leave is still
`LookoutPoint.tng` inside the same `00507C30`, at
`00509948` → `004FDBC0`. The first LEV **parse**
(`00B3EFA0` / WAD body) is **after** that TNG open.

First-seen Set Static Map then **misses** `FinalAlbion.stb`
and never calls `00B3EFA0`. Header + height + materials
are a later Lookout attach, not Init Game.

| Claim | Status |
|---|---|
| First named LEV is `LookoutPoint.lev` | **PROVEN** (WLD `LevelName`) |
| Map is LookoutPoint (native index 1, not Oakvale) | **PROVEN** |
| `00507C30` `LevelName` is a **file open** | **DISPROVEN** (path join only) |
| First TNG open `004FDBC0` is **after** that path store | **PROVEN** |
| First LEV **header/body** is **after** `004FDBC0` | **PROVEN** |
| First-seen `00B428E0` parses a LEV | **DISPROVEN** (STB miss, no `00B3EFA0`) |
| Cell count 128×128 = **16384** (1 m WAD) | **PROVEN** |
| Heightfield 8×8 × 16 m, origin **3232 / 3488** | **PROVEN** (STB; null at native open) |
| First usable cell texture `GROUND_PATH_SAND` → **4133** | **PROVEN** as file cell (0,0) |
| First file is `StartOakVale*.lev` / Oakvale East | **DISPROVEN** |

`lev-layout` “first-seen playable = `StartOakValeWest`” is
**DISPROVEN** as this no-save Leave walk (`wld-first-region`).

---

## Path from Leave (no-save New Game)

```
0042F2A2  Leave frontend                         // no LEV
0042F44D  FinalAlbion.wld → game+90576
0042F491  Init Game → 00418DCA → 004184BD
  00416953  Loading world
    004A1840
      QST / Startup WAD                          // bank, not a map .lev
      00507C30  Load .wld file
        NewMap 1
          LevelName "FinalAlbion\LookoutPoint.lev"
            005086B1 / 0050873F  0041A060        // path only
          LevelScriptName "LookoutPoint"
          MapX 3232; MapY 3488
        NewRegion 1  LookoutPoint
          ContainsMap Bridge, Lookout, Guild
        NewRegion 4  StartOakVale                // later
        0050959F  stem+.gtng  TLC miss           PROVEN skip
        00509859  Load global things
          [0x13B8609]==0 → 00509948 004FDBC0     ← FIRST TNG OPEN
            ebx=1 skip unused slot 0
            LookoutPoint.tng                     // after LevelName
        00509982  region graph
      004A1BD3  Set Static Map for Engine        // AFTER 00507C30
        00B23DC0 → 00B428E0
          00B42750(1)  Data\Levels\FinalAlbion.stb
            vtbl+12 MISS                         // no 00B3EFA0
004189C2  dummy pumps
later 00501450 → 00500540(1,0,0) LookoutPoint
  006C2170  TNG apply (Bridge first construct)
  later STB hit → 00B41E50 / 00B3EFA0            // first LEV parse
    exact later 00B428E0 site UNREAD
    current handle 00B3E820 LookoutPoint
```

`00507C30` listing has **no** `E8 00B3EFA0`, **no**
`00999230` of a `.lev`, **no** `0099AD80` of a `.lev`.
The only `00999230` in this fn is `.gtng` at `00509634`.

---

## Before or after `LookoutPoint.tng` `004FDBC0`?

| Event | vs `004FDBC0` | What |
|---|---|---|
| `LevelName` `LookoutPoint.lev` | **before** | `0041A060` + store. **Not** I/O |
| All other `NewMap` / `NewRegion` tokens | **before** | table fill |
| `.gtng` miss `0050959F` | **before** | skip |
| `004FDBC0` `LookoutPoint.tng` | **this** | first `.tng` open |
| Set Static Map / `FinalAlbion.stb` miss | **after** | no LEV parse |
| `00B3EFA0` 92-byte header | **after** | first LEV **open** |
| `LoadCompiledLev` / `LoadHeightField` | **after** | host body; native open is header only |

So:

- **Name** of the first LEV: **before** `004FDBC0`.
- **Open / parse** of that LEV: **after** `004FDBC0`.

Do not collapse the WLD string with `00B3EFA0`. Same split
as `tng-first-after-leave`: path vs `004FAFF0`.

`004FDBC0` (`listing-004c0000.txt`):

```
004FDBC0  ebx=1, edi=0x48          ; skip dummy slot 0
004FDC60  [slot+36] && [slot+40]   ; LoadedOnPlayerProximity
          call 004FBF60            ; 004FAFF0 + ".tng"
```

Lookout is Maps[0] / native slot 1 and is proximity-true.
That walk never names a `.lev`.

---

## Dump `00507C30` / LEV load

### `00507C30` Load `.wld` file

```
00507C30  sub esp, 0x1BC
00507C9A  push "Load .wld file"
          token loop 009BA4F0 → 00507EA0
00507FB9  "LevelName" → 005086B1
0050873F  call 0041A060            ; prefix 0x122F3B4 "Data\Levels\"
          0099B2C0 / 0099F570 / 0099EFB0   store
0050959F  "Load GTNG"  stem+.gtng
00509634  call 00999230            ; existence; miss
00509859  "Load global things"
0050987B  [0x13B8609]==0
00509948  call 004FDBC0            ; first .tng
00509982  "Load region graph"
```

`0041A060` is `push "Data\Levels\"; call 0099B6B0`. It
builds `Data\Levels\FinalAlbion\LookoutPoint.lev`. It does
not `00999230` / `0099AD80` that path.

### `00B3EFA0` is the LEV open

`lev-layout`: 16×u32 + 24 + u32 = **92** stream bytes onto
a remapped dest. **Not** a 48-byte memcpy. Material table
at file **179** is **not** read.

Callers: STB-hit `00B41E50` (`00B41E84`) or miss fallback
`00B42530` (`00B4260A`). First Leave `00B428E0` takes
neither (`stb-first-open`).

Native open leaves `CurrentCompiledLev` /
`CurrentHeightField` **null**. Full WAD cells and STB
height are later / host.

---

## Which map (not Oakvale)

Shipped `FinalAlbion.wld`:

```
NewMap 1;
  LevelName "FinalAlbion\LookoutPoint.lev";
  LevelScriptName "LookoutPoint";
  MapX 3232; MapY 3488; MapUID 162441;

NewRegion 1;  RegionName "LookoutPoint";
  ContainsMap BowerstoneBridge.lev
  ContainsMap LookoutPoint.lev
  ContainsMap GuildExterior.lev

NewRegion 4;  RegionName "StartOakVale";
  ContainsMap StartOakValeWest / MemorialGarden / StartOakValeEast
```

| Candidate | Why not first LEV |
|---|---|
| `StartOakValeWest.lev` MapX 3456 / MapY 736 | `NewRegion 4`; persist / intro leftover |
| `OakValeEast_v2.lev` 96×160 | test fixture only |
| `BowerstoneBridge.lev` | first **ContainsMap** after `00501450`; not `Maps[0]` |
| `FinalAlbion.stb` | world bank name; first-seen **miss** |
| Dummy WorldMap index 0 | no `LevelName` |
| `HerosOldHouse` / PATH_STONEY **4130** | C3D / leftover `FirstSceneWorld` |

ContainsMap file order is **not** the first `LevelName`.
Current handle after a later hit is **LookoutPoint**
(`00B3E820`). First intern-table slot that `00B420F0`
feeds `00B3EFA0` is **UNREAD** (could be a neighbour).
First **authored** / first **primary** LEV is Lookout.

---

## Cell count

Two layouts (`lev-layout`). Do not mix them.

| Layout | Lookout | Status |
|---|---|---|
| WAD compiled `.lev` 21-byte `LevCellGrid` | **128×128 = 16384** cells, 1 m, tag 21 | **PROVEN** (`LevFormatTests`) |
| STB / draw 16 m lattice | **8×8 = 64** | **PROVEN** as file; `[patch+12]/[+16]` **PARTIAL** |
| `00B3EFA0` dest | version 25 / `0x1904` / width<<16 @file+36 / height<<16 @file+40 / 16.16 `1.0` @file+44 | **PROVEN** numbers; dest remap not file order |

Header (`LevFile.ReadHeader` / tests):

```
+0   25
+4   0x1904
+36  128 << 16
+40  128 << 16
+44  65536          ; cell size 1.0
```

Picnic is 128×96. Oakvale East v2 is 96×160. Those are
**not** this open.

`00BDC2D0` walks 72-byte records, not the 21-byte WAD
table. Open does not walk either.

---

## Heightfield

STB copy of Lookout (`LevHeightField` / `LevFormatTests`):

| Field | Value | At native open? |
|---|---|---|
| `CellsX` / `CellsY` | **8 / 8** | size implied by header÷16; stream unread |
| `OriginX` / `OriginY` | **3232 / 3488** (WLD MapX/MapY) | on the map slot, not `00B3EFA0` dest+20/+48 |
| `SampleSpacing` | 16 m | — |
| `SampleCount` | ≥ 64 | **not** parsed at open |
| `Heights[0,0]` / `[4,4]` | 20..80 | **not** parsed at open |
| `TileCount` | **64** (section-2 + table) | **not** parsed at open |
| Fine 1 m bilinear 128×128 | host `FineHeights` | **DISPROVEN** as native open |

STB prefix: `u32[0]=1`, pad to 2048, `u32@2048` section-2
= **6144** (origin tile `[3232,3248]×[3488,3504]`, 289
verts). Vertex stream at 2056, 36-byte records.

`CurrentHeightField` stays **null** at `PresentWorld`
open (`EngineLifecycleTests`). Native `00B3EFA0` is
header only. Host `LoadHeightField` at
`LoadGlobalThingsFile` / `EnsureLevels` is **DIVERGE**
(`stb-first-open`: native has no STB handle yet).

---

## First texture

`00B3EFA0` does **not** read the material table. First
texture is a **file** fact, not a SetTexture at open.

Lookout WAD (`LevFormatTests.Cell_material_slots_*`):

| Slot / site | Name | textures.h |
|---|---|---|
| materials[0] file+179 | `INVALID_THEME_STANDIN` | unused |
| materials[1] file+311 | starts with `GROUND_` (suffix not dumped here) | — |
| materials[2] | **`GROUND_PATH_SAND`** (id **1911** is **not** a bank id) | **`LANDSCAPE_PATH_SAND_01` = 4133** |
| WAD cell **(0,0)** `Material0` | slot **2** | 4133 |
| `GROUND_GRASS` | later slot | **414** (`LANDSCAPE_GRASS_PLAIN`) |

So the first **used** 1 m cell texture is
**`GROUND_PATH_SAND` / 4133**. Slot 0 is the stand-in.
1911 is villager-legs leftover, not landscape.

Native first **SetTexture** is later, inside the first
nonempty `00BF4570` (`landscape-tex-stages`):

- stage 0 = `CEngineLandscapeRenderer+1468+(mesh+40)*8`
  (five ctor **128×128** textures from Init Engine)
- stage 1 = mesh `+20` or 0

That bind is **not** the WAD name, and it is **after**
Leave + opened patches. Not this open.

Oakvale leftover PATH_STONEY **4130** under SHOT2 is
**DISPROVEN** as this LEV.

---

## Host vs native

| Site | Host | Native | Class |
|---|---|---|---|
| First LEV **name** | `World.Maps[0].LevelName` Lookout | `00507C30` `LevelName` | **MATCH** |
| `EnsureLevels` in `LoadGlobalThingsFile` | ctor opens WAD + `_RT.stb` | `004FDBC0` opens `.tng` only | **DIVERGE** |
| Set Static Map | names `FinalAlbion.stb`, miss | same | **MATCH** |
| First `00B3EFA0` | `PeekMapHeader` 48 B later | 92 B intern after STB **hit** | **DIVERGE** length; later site **UNREAD** |
| `CurrentCompiledLev` / height at open | null | null | **MATCH** |
| `LoadCompiledLev` 16384 cells | submit / tests | not at open | **PROVEN** as file |
| First scene Oakvale LEV | `FirstSceneWorld` / `RegionTravel` | not this walk | **DISPROVEN** |

---

## UNREAD / PARTIAL

- Later first-seen site that hits a Lookout STB blob
  without `build_retail_static_maps` (`stb-first-open`).
- First `00B420F0` intern slot name (Lookout vs Bridge
  neighbour).
- Exact `GROUND_` string at file+311 (slot 1).
- File+12…+32 numeric contents (`lev-layout`).
- Whether intern 92 B == WAD `[0..92)`.

---

## Do not

- Call `00507C30` `LevelName` a LEV file open.
- Place the first LEV parse **before** `004FDBC0`.
- Report Oakvale / `StartOakValeWest` / 96×160 as this
  open (`lev-layout` leftover).
- Treat 21-byte WAD cells as the 72-byte draw records.
- Treat `CurrentHeightField` / material table as filled
  at `00B3EFA0`.
- Use PATH_STONEY **4130** or material u32 **1911** as
  the first landscape texture.
- Skip `BowerstoneBridge` on the later ContainsMap
  construct walk because the first **named** LEV was
  Lookout.
