# First landscape / LEV submit after Leave / Set Static Map

Investigation only. Production `src/` was not edited.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** / **DIVERGE**.

Question: after no-save Leave and `Set Static Map for Engine`, what
is the first landscape / LEV **submit**? Not Oakvale house
(`HerosOldHouse` / `StartOakValeWest`). Which **map cell**, which
**texture stages**, which **draw fn**? Compare host
`LandscapeDraw`.

Authority: dump `tools/Fable.ExeIndex/out/01-sections/landscape-trace/`,
`proofs/landscape-after-leave/`, `proofs/landscape-first-draw/`,
`proofs/lev-layout/`. Siblings: `landscape-tex-stages/`,
`stb-first-open/`, `wld-first-region/`, `terrain-first-draw/`.

---

## Verdict

**Leave + Set Static Map does not submit a LEV cell.**
`004A1BD3` → `00B23DC0` → `00B428E0` names `Data\Levels\FinalAlbion.stb`,
misses, leaves `[0x1436E8C]+44` empty. No `00B3EFA0`. No `00BF4570`.

**First stored-cell LEV DIP is later LookoutPoint**, after a
real STB attach fills `+44`. Draw fn is **`00BF4570`** (layer
`0x40`) → **`00A0AD40`** device vtbl+**328** type **5** strip.

Not Oakvale house. House 6909/6911 is bit `0x20` C3D. First
WLD region / first opened playable map is **`LookoutPoint`**.

| What | Native | Status |
|---|---|---|
| Map of the first LEV DIP | **`LookoutPoint`** (WLD `NewMap 1` / `NewRegion 1`, MapX **3232** MapY **3488**) | **PROVEN** as the first real region; first patch on `+44` **PARTIAL** |
| First `00BF4570` **call** | first owner on `+44`, then **col=0, row=0** (72-byte cell, origin `+56/+58` = **(0, 0)** m) | **PROVEN** as walk |
| That cell in file space | STB **section-2 / tile 0**: world `[3232,3248]×[3488,3504]`, region-local `[0,16]×[0,16]` | **PROVEN** as the origin 16 m tile |
| First `00BF4570` **DIP** | same cell only if `+60` bit `0x4` and `00BF3860` pass | **PARTIAL** (no first-frame capture) |
| Texture stages | **0** at `00BF510D`, **1** at `00BF5491`; unbind **0/1/2** at `00B67510` | **PROVEN** as sites |
| Draw fn | **`00BF4570`** (FG stored mesh). First *patch* walk is **`00BDC060` → `00BF71D0`** bit `0x4` (not a LEV cell) | **PROVEN** |

Host `LandscapeDraw` is a decoded FG helper. It is **not** the
native DIP site. `BothPasses` is now FG-only (**MATCH** intent).
Stage bind and cell `+52/+56` comments still **DIVERGE**.

---

## Recovered order (no-save New Game)

```
0042EC7C retail pump
  0042DF9E  frontend
    0042E0BB → 00B27D90 → 00B6B0B0
      [0x1436E8C]+44 sentinel (esi==eax)
      no 00BDC060 / no 00BDC2D0 / no 00BF4570
0042F2A2 Leave
  0042EBB6  teardown Present (not a landscape draw)
  FinalAlbion.wld
0042F491 Init Game → 00416953 Load world
  004A1840
    00507C30  first NewRegion = LookoutPoint     ; not StartOakVale
    004A1B7D  "Set Static Map for Engine"
    004A1BD3  display vtbl+208 00B23DC0 → 00B428E0
      00B42750(1)  Data\Levels\FinalAlbion.stb
        [+52].vtbl+12  MISS
        no 00B3EFA0 / no 00BDD0E0 / no 00BDF010
        +44 still empty
      00B41FA0 LoadWaterData (intern miss)
004189C2 game pump
  dest empty; no region; no landscape DIP
later 00501450(1) LookoutPoint
  006C2170 / 004FCBB0 / 0051FD80 / 006AC910
    GuildArrivalHSP (52.688, 69.597, 36.982)   ; ~cell (3,4)
    004FC8A0 MiniMap only  NOT 00B428E0
  later STB hit → 00BDD0E0 / 00BDF010 → +44 nonempty
    exact later 00B428E0 site UNREAD (not 004FC8A0)
next 00B27D90 → 00B6B0B0
  bit 0x4  → 00BDC060 → 00BF71D0     first patch walk (BG)
  bit 0x40 → 00BDC2D0 → 00BF4570     first stored-cell LEV DIP
    first call: col=0,row=0 of first list owner
    stage 0 00BF510D / stage 1 00BF5491 / DIP 00A0AD40
  00B67510 unbind 0/1/2
```

Frontend never issues a landscape DIP. **PROVEN.**
Game caller of `012A0F3C+32` after Leave is still **UNREAD**.

---

## Not Oakvale house

`proofs/wld-first-region`: first `RegionName` in `FinalAlbion.wld`
is **`LookoutPoint`**. `StartOakVale` is `NewRegion 4`.

| Candidate | Why not first LEV submit |
|---|---|
| `HerosOldHouse` C3D 6909 / 6911 | bit `0x20` static; `proofs/c3d-first-submit` |
| PATH_STONEY **4130** under SHOT2 | Oakvale leftover `FirstSceneWorld` / `docs/render/traces/landscape-submit.txt` |
| `StartOakValeWest` MapX 3456 / MapY 736 | later persist / intro; `RegionTravel.NewGameRegion` **LEFTOVER** |
| `lev-layout` “first-seen playable = StartOakValeWest” | **DISPROVEN** as no-save Leave (`wld-first-region`) |
| Dummy WorldMap index 0 | no `RegionName`; dest-empty Present |
| `FinalAlbion.stb` at Set Static Map | **MISS**; no cells attached |

Do not take wiki / intro Oakvale as the first landscape submit.

---

## Which map cell

### Walk (native)

`00BDC2D0` after a live tessellator 4-plane AABB (`+168/+180`, Z=0):

```
ebx = 0                         ; row
esi = 0                         ; col
cell = [owner+8] + (row*[+12] + col) * 72
call 00BF4570
```

Lookout compiled grid is **128×128** m. Coarse lattice **16 m**
→ **8×8** cells (**PARTIAL**: `[owner+12]/[+16]` inferred, not
dumped as immediates). First-seen AABB start is **(0, 0)**
(`LandscapeFrustum.FirstSeenAabbStartX/Y`).

Cell ctor `00BF3700`: `+56 = col*16`, `+58 = row*16` (u16 metres).
First call is therefore **Lookout cell (0, 0)**, origin **(0, 0)**
region-local = WLD **(3232, 3488)**.

`00BF4570` then:

```
test [cell+60], 0x04
je  skip
call 00BF3860                   ; 3D AABB cell+32 / +44
je  skip DIP
… mesh list at cell+8 …
```

Hero / first 3D eye is **`GuildArrivalHSP`** ≈ `(52.7, 69.6, 37)`
→ floor(/16) = **cell (3, 4)**, origin (48, 64). That is **not**
the first walk index. Whether (0, 0) actually DIPs on the first
Lookout Present is **UNREAD** (need `+60` and the four planes).

Missing `[owner+4]` tessellator submits **every** cell in the
same (row, col) order. **PROVEN** (`je 00BDC382`).

### File (STB / LEV)

Two layouts (`lev-layout`). Do not conflate.

| Layout | Size | Role at submit |
|---|---|---|
| WAD 21-byte `LevCellGrid` | 1 m, Lookout 128×128 | themes / host `TextureId` sample. **Not** walked by `00BF4570`. **PROVEN** absence |
| 72-byte in-memory cell | 16 m | `00BDC2D0` walk. Flag `+60` bit 4 required |
| STB tile (36-byte rec + LZO) | 16 m stored mesh | ingested onto the 72-byte cell; GPU expand `00BFE050` |

Lookout STB (`LevFormatTests`):

- `u32@2048` section-2 = **origin** tile, verts[0] = **(3232, 3488)**,
  span to **(3248, 3504)**. 289 verts (17×17).
- Table tile **index 0** is the same origin cell.

That is the file body of native cell **(0, 0)**.

First patch on `[0x1436E8C]+44` is **UNREAD** (current Lookout vs
a neighbour such as `BowerstoneBridge` / `PicnicArea` depends on
attach order). Current handle `00B3E820` is LookoutPoint.
`ContainsMap` order is Bridge, Lookout, GuildExterior — **not**
the same as the draw-list walk.

---

## Texture stages

Inside the first nonempty `00BF4570` (`landscape-tex-stages`):

| Site | Stage | Source |
|---|---|---|
| `00BF50C5` → `00BF510D` `push 0` vtbl+**260** | **0** | `[ [0x1436EA8] + (mesh+40)*8 + 1468 ]` — five ctor **128×128** textures (`00B67270` at Init Engine) |
| `00BF5477` → `00BF5491` `push 1` | **1** | resolved mesh **`+20`** wrapper, or **0** if missing |
| `00B67510` after the pass | **0 / 1 / 2** | NULL |

`00BF50E0` is **not** a function. `cell+1468` does not exist
(cell is 72 bytes). **DISPROVEN.**

`mesh+40` (stream u8 at `00BFE15D`) must be slot **0..4**.
First-seen Lookout value **UNREAD**. Stage-1 live WAD pointer
**UNREAD**.

FG PS `PSHADER_LANDSCAPE_FOREGROUND`: RGB is **`t1`**
(`mul_x2 t1, v0`); `t0` is **alpha**. VS `oT0.xy = v3.yz`
(extra); albedo `oT1` from first-seen **c40=c41=0**.

Pass setup (not per-cell `SetTexture`): bit `0x40` `00B68DA0`
(`VSHADER_` / `PSHADER_LANDSCAPE_FOREGROUND`) + `00B67480` +
`0098B5E0(2)`.

---

## Draw fn

Stored LEV mesh:

```
00B6B0B0  vtbl+16  arg+4 == 0x40
  00BDC2D0  patch AABB
    00BF4570  per 72-byte cell
      SetStreamSource vtbl+400  (VB mesh+56, stride 24)
      SetIndices      vtbl+416  (IB mesh+52)
      00A0AD40 → IDirect3DDevice9 vtbl+328
        type=5  D3DPT_TRIANGLESTRIP
        NumVerts = [mesh+68]  prims = [mesh+70]
        startIndex=0  baseVertex=0
```

Cell `+52/+56/+68` are AABB max.Z / origin X / refcount.
Buffers live on the **`00BFE050`** mesh linked from **cell+8**.

Bit `0x4` is **`00BDC060` → `00BF71D0`**: tessellator BG
(`CLandscapeBackgroundPatch` `+192/+188`). **Not** the 16 m
STB cell. Registration `00B26A75` walks `0x4` then `0x40`, so
the first nonempty **patch** call after maps exist is BG.

`00B3EFA0` is header parse on an STB **hit**. First Leave
`00B428E0` never calls it. Parse is not submit.

---

## Host `LandscapeDraw`

`src/Fable.Render/LandscapeDraw.cs` (not edited).

| Host | Native | Class |
|---|---|---|
| `LandscapeDraw.Foreground` bit `0x40` | `00BF4570` stored cell | **PROVEN** pairing |
| `BothPasses` = FG only | bit `0x4` is `00BF71D0` | **MATCH** (old same-VB pair **DISPROVEN**) |
| `Background(cell)` still exists | that cell VB is never bit `0x4` | leftover API |
| `LandscapeBufferKey` comment “VB/IB on cell +56/+52” | those are origin / AABB; buffers on `00BFE050` | **DISPROVEN** |
| `HostWorld` = I | native `T(cam)` on cam-rel VB | **EQUIVALENT** |
| `SrcAlphaBlend` false | FG alphablend off | **PROVEN** |
| `TextureBind` FG `(mask=TextureId1, albedo=TextureId)` | t0 = 128² table; t1 = mesh `+20` | **DIVERGE** as opcode; RGB-on-t1 contract **PARTIAL** |
| `CollectVisibleCells` then `MeshBatches.BuildCells` | per-cell `00BF4570` + `+60&4` + `00BF3860` | **DIVERGE** (host dumps every primary tile with faces; first list entry is STB section-2 / CellX=0) |
| `SubmitCurrentWorld` after `HeroSpawned` | after `006C2170` + opened `+44` | **PROVEN** timing |
| Concat land+C3D+sky one mesh | layers `0x4` → `0x40` → `0x20` → `0x2000` | **DISPROVEN** as native DIP |
| `FirstSceneWorld` Oakvale house + PATH_STONEY 4130 | leftover intro | **DISPROVEN** as this submit |
| `LevelLibrary` ctor opens `FinalAlbion_RT.stb` | first-seen names `.stb` and **misses** | **DIVERGE** (`stb-first-open`) |
| `PeekMapHeader` 48 B / `LevCellGrid` 21 B as the DIP record | 92-byte remap; 72-byte cell + `00BFE050` | **DISPROVEN** (`lev-layout`) |

Host first `BuildCells` draw is the first `LoadCells` tile with
faces (section-2, **CellX/CellY default 0**). That **can** match
native first **call** on Lookout (0, 0). It is **not** proven to
be the first **DIP** (cull / flag). Host does not emit
`LandscapeDraw` on the live Present path; it emits `MeshDraw`
`PassBit=0x40`.

---

## Classification

| Claim | Status |
|---|---|
| First LEV / landscape DIP is after Leave + Set Static Map **miss**, not at that call | **PROVEN** |
| That miss is `FinalAlbion.stb`; no `00B3EFA0` | **PROVEN** |
| First stored-cell DIP is `00BF4570` on Lookout, bit `0x40` | **PROVEN** as site / map family; first-frame clock **PARTIAL** |
| First walk cell is Lookout **(0, 0)** / STB origin tile | **PROVEN** as call order; DIP **PARTIAL** |
| First submit is Oakvale house / `StartOakValeWest` / 4130 | **DISPROVEN** |
| Frontend `0042DF9E` DIPs landscape | **DISPROVEN** |
| `00BDC060` DIPs the 16 m LEV mesh | **DISPROVEN** |
| Stage 0 / 1 sites `00BF510D` / `00BF5491` | **PROVEN** |
| Stage 0 from `cell+1468` / `00BF50E0` is a function | **DISPROVEN** |
| Host `LandscapeDraw` is the native DIP | **DISPROVEN** |
| `LandscapeDraw.BothPasses` FG-only | **MATCH** |
| First-seen `mesh+40` / live t1 / which `+44` node is first | **UNREAD** |
| Later native `_RT.stb` hit site | **UNREAD** (`stb-first-open`) |

Dumps: `landscape-trace/INDEX.md`,
`setstaticmapfileforuse-00b428e0.md`, `openstaticmaps-00b42750.md`,
`landscape-draw-vtbl16-00b6b0b0.md`, `patch-submit-bit4-00bdc060.md`,
`patch-submit-bit40-frustum-00bdc2d0.md`,
`per-cell-submit-00bf4570.md`,
`per-cell-settexture-stage0-00bf50e0.md` (misaligned),
`parsemapheader-00b3efa0.md` (capped).
Listings `0042DF9E` / `00B27D90` / `00BF4570` / `00BDC2D0`.
Tests: `LevFormatTests.Stb_section_two_is_the_map_origin_tile`,
`WorldGeometryTests.Lookout_cells_match_stb_tiles`,
`EngineLifecycleTests` Set Static Map / Lookout spawn.

No production edit. Proposed host (not done): keep `LandscapeDraw`
as FG-only; fix the `+52/+56` comment; do not treat Oakvale house
or `FirstSceneWorld` as the first Leave LEV submit; do not
`SetTexture` WAD ids on stage 0.
