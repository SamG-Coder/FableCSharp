# Lookout vs Oakvale landscape INDEX16 DIPs

Investigation only. Production `src/` was not edited.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**EQUIVALENT** / **DIVERGE** / **LEFTOVER**.

Question: leftover extra INDEX16 DIPs when Oakvale opened. Compare
Lookout vs Oakvale STB/LEV submit. Native `00BF4570` stored
tessellation only. Do not fill leftover 1 m cells from WAD
(invented, removed). What extra INDEX16 DIPs appear on Oakvale?
Host `WorldGeometry` / `LevFormat`.

Evidence: `docs/PARITY.md` terrain rows, `docs/render/FIRST_SCENE_WORLD_PARITY.md`,
`LevFormatTests`, `WorldGeometryTests`, `WorldPipelineTests`,
`src/Fable.Formats/Levels/LevTileMesh.cs`,
`src/Fable.Game/WorldGeometry.cs`,
`src/Fable.Render/MeshBatches.cs`,
`tools/Fable.ExeIndex/out/01-sections/landscape-trace/per-cell-submit-00bf4570.md`,
`docs/status/investigations/2026-08-18-landscape-draw.md`.

---

## Verdict

**Extra INDEX16 DIPs on Oakvale are stored STB tessellation, not
WAD 1 m fill.** Native `00BF4570` DIPs each mesh on cell `+8`
(`mesh+60` next). Oakvale extras that Lookout-only XY `2000–6000`
used to drop are `CPatchTesselationEdgeStrip` INDEX16 strips
(MapY=736). Host `ToTileTriangles` equals those strips. Filling
uncovered PATH/WATER 1 m cells from the WAD table is **DISPROVEN**
and **removed**.

| Claim | Class |
|---|---|
| `00BF4570` submits stored tessellation only | **PROVEN** |
| WAD leftover 1 m PATH/WATER fill is an extra INDEX16 source | **DISPROVEN** (removed) |
| Oakvale extra INDEX16 vs Lookout includes stored edge-strip meshes | **PROVEN** (native DIP count) |
| Lookout-only extra XY `2000–6000` dropped Oakvale extras | **DISPROVEN** as a gate |
| Extra opened STB tiles (Contains/Sees + AABB) add more cell DIPs | **PROVEN** as extra cells |
| Host `BuildCells` issues one DIP per `LandscapeCell`, extras merged | **DIVERGE** vs native extra DIP |
| Sea/water INDEX16 (`D3DFMT` 101) is first-seen landscape FG | **DISPROVEN** (empty-out) |
| Edge-strip `fmt` `0x5901`–`0x5904` / `0x5801`–`0x5804` meaning | **UNREAD** |
| Water/decal as a later INDEX16 pass | **UNREAD** |

**Extra DIP source:** `CPatchTesselationEdgeStrip` stored extras
(`00BF4E90` / `mesh+60`) plus extra opened STB adaptive primary
strips. **Not** WAD 1 m leftover fill.

---

## Native submit (`00BF4570`)

Patch walk `00B6B0B0` bit `0x40` → `00BDC2D0` AABB → per 72-byte
cell `00BF4570`. DIP is device vtbl **328**, type **5**
(`D3DPT_TRIANGLESTRIP`), IB format **101** (`D3DFMT_INDEX16`),
`IndexCount = PrimitiveCount + 2`. Subject is the **mesh** at
cell `+8` (`mesh+52` IB / `mesh+56` VB), not the 72-byte cell.

```
00BF4570
  test [cell+60], 0x04          ; required FG flag
  call 00BF3860                 ; cell AABB
  DIP mesh+68 verts / mesh+70 prims   ; wrapper 00A0AD40 vtbl+328
  mesh = mesh+60
  loop 00BF4E90                 ; extras: bind + DIP again
```

Primary strip and `CPatchTesselationEdgeStrip` extras are
**separate mesh nodes**. Each is one INDEX16 DIP. Cache key is
`(map, cellX, cellY, meshIndex)`, not `(map, cellX, cellY)`
alone (`2026-08-18-landscape-draw.md`).

Bit `0x4` is tessellator BG `00BF71D0`, **not** this IB.

17×17 (`v=289`, flag `256`) stores **no** primary strip. Adaptive
tiles store PrimitiveCount+2. Extras sit after the primary strip,
or immediately when flag=256 (`PARITY` STB rows).

---

## Host Lookout vs Oakvale

| | Lookout | Oakvale (`StartOakValeWest`) |
|---|---|---|
| MapX/MapY | 3232 / 3488 | 3456 / 736 |
| STB tiles | 64 (table + section 2 origin) | village tiles + extras |
| Full 17×17 | many (`LevFormatTests` `full >= 8`) | fewer; adaptive omits PATH 1 m |
| Edge extras | 3–16 per tile; XY in 3200–3400 / 3450–3650 | stored; MapY=736 **fails** 2000–6000 |
| Opened maps | AABB neighbours (Picnic, GuildExterior, …) | Contains West/East/Garden + Sees fillers/seas |
| WAD 1 m fill | not in `ToTileTriangles` | not in `ToTileTriangles` |
| Water 442 | omitted | omitted (`SEA_*` / `WATER_*`) |

`LevHeightField.ToTileTriangles` comment: STB primary strip +
`CPatchTesselationEdgeStrip` only. `00BF4570` stored tessellation.
Water/decal **UNREAD**.

`WorldGeometryTests.New_game_oakvale_loads_contains_and_sees_maps`
locks `ToTileTriangles` count == `Tiles.ToTriangles` (no WAD fill).
`WorldPipelineTests.First_seen_landscape_submits_primary_and_edge_strips`
locks Oakvale stored extras (`withExtras > 0`).
`Lookout_cells_match_stb_tiles` locks one host draw per cell,
INDEX16 when `PrimitiveCount > 0`, format 101, vtbl 328, type 5.

---

## What extra INDEX16 DIPs are

### 1. Stored edge strips — **PROVEN** native extra DIP

`CPatchTesselationEdgeStrip` after the primary (or after verts
when flag=256). 30-byte header, 15-byte world verts,
PrimitiveCount+2 INDEX16.

Lookout extras fill ~11% of 1 m cells the primary does not cover
(`PARITY`). Oakvale adaptive strips omit many `GROUND_PATH_*` 1 m
cells; native still submits **only** the stored extras, not a WAD
backfill.

`LevTileMesh.ReadExtras` used to reject verts unless XY was
2000–6000. That was a Lookout-only leftover. Oakvale MapY=736
dropped first-scene edge strips (`FIRST_SCENE_WORLD_PARITY`
**DISPROVEN**; `WORLD_SPACE_CONTRACT`). After the gate, those
stored extras **appear** as extra INDEX16 DIPs on Oakvale.

Native: extra DIP per extra mesh (`00BF4E90`).

Host `ToCells`: extras `AddStrip` into the **same**
`LandscapeCell.Faces`. `StripIndices` is primary only
(`!useGrid && tile.Indices.Count >= 3`). `MeshBatches.BuildCells`
INDEX16 path uses `StripIndices` and **drops extra faces from the
IB**. 17×17 `useGrid` cells have `PrimitiveCount=0` and soup the
grid **plus** extras as **one** non-indexed draw.

So Oakvale extra INDEX16 vs Lookout on **native** is the edge-strip
list. Host **does not** emit those as extra INDEX16 DIPs
(**DIVERGE**). `LandscapeBufferKey` is still `(Map, CellX, CellY)`.

### 2. Extra opened STB tiles — **PROVEN** extra cells

`WorldGeometry.StaticMapsAround` unions WLD `ContainsMap` /
`SeesMap` with BWD AABB touch. Oakvale cluster is West / East /
MemorialGarden + five fillers + four seas. Seas skip FG (`IsSea`
/ texture 442). Fillers with STB `.lev` add 16 m cells.

Each `LoadCells` tile becomes one `LandscapeCell`. Adaptive tiles
among those maps add more host INDEX16 draws (`PrimitiveCount > 0`).
Lookout’s extra cells are AABB neighbours, not the Oakvale cluster.

Section 2 (`u32@2048`) is the west-south origin tile the table does
not store (`PARITY`). Extra stored cell on **both** maps, not
Oakvale-only.

### 3. More adaptive primary strips — **PROVEN** as tile mix

Lookout has many flag-256 17×17 tiles → host soup DIP, **not**
INDEX16. Oakvale village is more adaptive → more primary INDEX16
DIPs from stored strips. Still `00BF4570` stored tessellation.
Village path cover is strip-only (less than every PATH cell).

### 4. WAD leftover 1 m fill — **DISPROVEN**

`PARITY` “Did not work”: fill uncovered PATH/WATER from the WAD
table. Extra 1 m quads are not the `00BF4570` pass. Host comment
and Oakvale test lock strip-only. Do not re-invent.

### 5. Water / sea INDEX16 — **DISPROVEN** first-seen FG; **UNREAD** later

Sea prefix format 101 exists. First-seen `[+636]==0` so bind does
not run. Draw `00B783F0` empty-out. Not landscape FG extra DIPs.

Decal / water mesh as a later INDEX16 pass: **UNREAD**.

---

## Host vs native DIP count

```
Native 00BF4570
  for each opened patch (00BDC2D0)
    for each 16 m cell (bit 0x4)
      for each mesh on cell+8          // primary + extras
        DrawIndexedPrimitive INDEX16

Host SubmitCurrentWorld
  CollectVisibleCells                  // one LandscapeCell per STB tile
  MeshBatches.BuildCells               // one MeshDraw per cell
    PrimitiveCount>0 → unwind primary strip to list IB
    else             → Faces soup (17×17 + merged extras)
```

| Unit | Native INDEX16 DIPs | Host draws |
|---|---|---|
| 17×17 flag 256 | stored extra meshes only (no primary strip) | 1 soup draw (grid+extras) |
| Adaptive primary | 1 | 1 INDEX16 (unwind) |
| Each edge extra | 1 | 0 extra (merged or dropped from IB) |
| WAD 1 m hole | 0 | 0 |

Oakvale leftover extra INDEX16 is **not** a second host fill pass.
It is native extra meshes + extra opened adaptive tiles.

---

## Locked (do not revert)

- `ToTileTriangles` == tile strips (`LevFormatTests` /
  Oakvale `WorldGeometryTests`).
- No WAD 1 m leftover fill (`PARITY` uncovered-cells row).
- Extra XY 2000–6000 gate **DISPROVEN**; Oakvale extras stay.
- Cell DIP layer `0x40` only; bit `0x4` is tessellator BG.
- Water 442 / type-8 bank omitted first-seen.

## Unread leftovers (do not invent)

- Edge-strip `fmt` as side/LOD (`PARITY` open 4 / 14).
- Whether extras share the cell AABB or have their own.
- Water/decal INDEX16 when bind would run.
- Native DIP count on a live Oakvale Present (host count is
  cells, not meshes).
