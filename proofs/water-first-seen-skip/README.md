# Water first-seen empty skip (Oakvale vs Lookout INDEX16)

Investigation only. Production `src/` was not edited.
Do **not** invent water meshes (7363 / `LANDSCAPE_WATER` 442).
Do **not** fill WAD 1 m PATH/WATER cells.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**MATCH** / **DIVERGE** / **LEFTOVER**.

Question: when does water appear (Oakvale vs Lookout)? Native
skip empty INDEX16? Oakvale INDEX16 dest ExtraStrips as
separate DIPs **MATCH**. Do not fill WAD 1 m cells.

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/landscape-trace/`
(`INDEX.md` v5, `per-cell-submit-00bf4570.md`,
`loadwaterdata-00b41fa0.md`,
`sea-name-onto-water-renderer-00b6d4d0.md`);
`tools/Fable.ExeIndex/out/01-sections/newgame-trace/`
(`water-draw-empty-check-00b783f0-00b783f0.md`,
`water-draw-empty-je-00b7851d-00b78513.md`,
`water-draw-empty-ret-00b7a865-00b7a865.md`);
`docs/PARITY.md` (uncovered 1 m / sea-water rows);
`docs/status/investigations/2026-08-18-landscape-draw.md`;
`src/Fable.Formats/Levels/LandscapeCell.cs`,
`LevTileMesh.cs`, `LandscapeTextures.cs`;
`src/Fable.Game/WorldGeometry.cs`;
`src/Fable.Render/MeshBatches.cs`;
siblings `proofs/oakvale-index16-dips`,
`proofs/leftover-36-native-dest`,
`proofs/water-first-draw`.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| When does water appear first-seen? | **Never.** Layer `0x20000` `00B783F0` empty-out. Same skip on Lookout no-save Present and Oakvale intro view | **PROVEN** skip |
| Oakvale vs Lookout **data**? | Oakvale has STB sea bank + `SEA_*`/`WATER_*` cells. Lookout cluster has no Oakvale seas. Neither first-seen **draw**s water | **PROVEN** data; draw skip **MATCH** both |
| Native skip empty INDEX16 (water)? | **Yes.** Empty vectors / not (`[+630]&&[+645]`) → `je 00B7851D` → `00B7A865` bare `ret 4`. No sea INDEX16 DIP | **PROVEN** |
| Native skip empty INDEX16 (landscape)? | Empty `cell+8` skip. Flag-256 stores **no** primary strip (`IndexCount=0`). Extra `<3` indices skip. `00BF4570` does **not** `test mesh+70` before DIP | **PROVEN** skip empty list / no-strip; primCount-0 call **UNREAD** as a gate |
| Oakvale ExtraStrips as separate INDEX16 DIPs? | Native `00BF4E90` `mesh=mesh+60`. Host `MeshBatches.BuildCells` one draw per extra with `PrimitiveCount>0` | **MATCH** |
| Fill uncovered PATH/WATER 1 m from WAD? | **No.** Not `00BF4570`. Host `ToTileTriangles` is stored strips only | **DISPROVEN** |
| Invent 7363 / 442 water mesh first-seen? | **No.** Bind `[+636]==0`. `TryResolve` omits `SEA_*`/`WATER_*` | **DISPROVEN** |

---

## Verdict

**First-seen water is the empty skip. Oakvale ExtraStrip INDEX16
is a separate dest DIP MATCH. Do not fill WAD 1 m. Do not invent
water meshes.**

Water draw site is `CEngineWaterRenderer` vtbl+16 `00B783F0`
(bit `0x20000`). First-seen ctor zeros every tested vector
(`+508`..`+624`) and mesh-ready bytes (`+630`/`+645`). The
function ORs begin==end and **`je 00B7A865`**. That is a
**bare `ret 4`** — no unbind, no INDEX16, no 7363 mesh.

That skip is **not** “Lookout has no sea / Oakvale has sea.”
It is renderer-empty. Oakvale **stores** a sea bank and
`SEA_OAKVALE_*` / `WATER_*` materials. Lookout no-save is
inland (Picnic / GuildExterior AABB). Both first-seen
`00B783F0` take the empty-out.

Oakvale leftover extra INDEX16 vs Lookout is stored
`CPatchTesselationEdgeStrip` dest (`00BF4E90`), plus extra
opened adaptive cells. Host `BuildCells` **MATCH** one
INDEX16 dest DIP per extra. Sibling
`proofs/oakvale-index16-dips` “host merges extras” is
**STALE** vs HEAD. Filling WAD 1 m holes is **DISPROVEN**.

---

## 1. When water appears — Oakvale vs Lookout

### First-seen draw — **PROVEN** empty skip both

`WaterDrawShouldSubmit_00B783F0` is false when every
begin==end and not (`[+630] && [+645]`). Ctor `00B73760`
zeros those fields. Prepare `00B71FB0` **always** copies
end from begin (`cmp eax,eax`) then bind `00B6DC40`.
First-seen `[+636]==0` so bind returns 0
(`FirstSeenWaterPrepareFillsMesh=false`).
`00B23F00` (wanted-name setter, vtbl slot 14) has **zero**
first-seen `E8`.

```
00B783F0
  cmp [+508],[+512] / [+520],[+524] / … / [+616],[+620]
  [+630] && [+645]  → mesh-ready
  all empty → je 00B7851D → 00B7A865  pop×4 / add esp,40 / ret 4
```

Frontend **does** walk water vtbl+16 (`0042E0BB` →
`00B27D90` → `00B2AB80` bit `0x20000`). Query `00B7ED70`
returns **1**. DIP still empty.

No-save first **rendered** region is LookoutPoint
(`RegionThings` + `006B3FF0`). Oakvale intro view
(`StartOakValeWest` / `CAM_OVIF_SHOT2`) is leftover #4.
**Neither** first-seen Present issues a water INDEX16 DIP.

Tests: `FirstSeenWaterDrawShouldSubmit=false` on
Lookout (`Lookout_cells_match_stb_tiles` landscape
path) and Oakvale
(`New_game_oakvale_loads_contains_and_sees_maps`,
`Start_oakvale_has_a_sea_bank_and_no_water_bank`,
`Visibility_and_layers_drive_shipped_first_scene_lists`
water record `"reject 00B783F0 empty-out"`).

### Data that would feed a later draw — **PROVEN** Oakvale store; Lookout cluster inland

| | LookoutPoint (no-save first Present) | Oakvale (`StartOakValeWest` intro view) |
| --- | --- | --- |
| MapX / MapY | 3232 / 3488 | 3456 / 736 |
| Seas in cluster | none (Picnic / GuildExterior / Greatwood AABB) | Sees `StartOakVale_Sea_01`..`_04` |
| STB sea bank | 25 region-named `__ENGINE_SEA_*` blobs exist; bind never reads them first-seen | `StartOakVale` blob 129966 bytes, u32[0]=**7363** (not type 8) |
| Water-prefix STB | none | none (`00B41FA0` intern miss `00B420E4`) |
| LEV materials | land PATH/GROUND | includes `SEA_OAKVALE_*`, `WATER_GREYCLIFF_ET`, `WATER_BWLAKE_*` |
| Landscape 442 | omitted | omitted (`TryResolve` null while empty-out) |
| First-seen `00B783F0` | empty-out | empty-out |

`OpenStaticMaps` `00B42750` first-seen STB miss skips
`00B6D4D0` sea-name (`test bl,bl; je 00B428CA`).
`LoadWaterData` always runs; intern `0x1436EC8` miss.

When bind **would** run (`[+636]≠0`): `00B6DC40` →
`00BE91E0` reads sea prefix as vertex/index counts,
stride **12**, index format **101** (`D3DFMT_INDEX16`),
PrimitiveCount at **+180** = indexCount−2. First-seen
never reaches that reader
(`FirstSeenReadsSeaPrefixWords=false`). Do **not**
plant that mesh on Lookout or Oakvale first-seen.

Type-4 enqueue `00BF44A0` (only `E8` from per-cell
`00BF57D1`, `[obj+28]==4`) pushes onto water `+0x244`.
That is a **pointer list**, not the 7363 INDEX16.
First nonempty `00B783F0` after those lists fill is
**UNREAD** as a clock on both maps.

Host `WorldGeometry.TryAdd` skips `IsSea`. Oakvale
`Sea_*` maps often have WLD `IsSea=false` (SeesMap)
so they still appear in `Regions` as opened headers.
FG still omits 442. **PARTIAL** WLD flag vs material
prefix — do not invent FG water from that list.

---

## 2. Native skip empty INDEX16

### Water INDEX16 — **PROVEN** skip (no DIP)

First-seen never builds the sea IB. Empty-out is
**before** any `DrawIndexedPrimitive`. Format 101
exists only on the unread bind path. Host
`ScenePasses` omitting Water **DIVERGE**s the **call**
(`00B2AB80` still calls `00B783F0`) and **MATCH**es
the **DIP** (none).

### Landscape INDEX16 — **PROVEN** skip empty list / no primary strip

`00BF4570` (bit `0x40` FG):

```
test [cell+60], 0x04          ; else skip DIP
call 00BF3860                 ; cell AABB; al=0 skip
DIP mesh at cell+8            ; vtbl+328 type 5 INDEX16
mesh = mesh+60
loop 00BF4E90                 ; ExtraStrip dest DIP
```

Empty mesh list: `cell+8 == 0` → `je 00BF569C` (no
INDEX16). Flag **256** 17×17 tiles store **no** primary
strip (`LevTileMesh.hasPrimaryStrip = flag != 256`,
`IndexCountFromPrimitiveCount(..., false) = 0`).
Native DIP dest is ExtraStrip dest only on those
tiles. Host soups the 17×17 grid as one non-indexed
draw — extra dest host **writes** that native dest
**skips** on the primary (inverse leftover).

`00BF4570` DIP args are `mesh+68` verts / `mesh+70`
prims into `00A0AD40`. Dump does **not** `test [mesh+70]`
before the call. A live mesh node with prims=0 would
still call DIP. That zero-prim **call** is **UNREAD**
as a taken first-seen path. Host `BuildCells`
requires `primitiveCount > 0` for the INDEX16 arm
and `n==0` returns without a draw — **MATCH** skip
of empty ExtraStrip dest.

`LevTileMesh.ToCells`: extra `Indices.Count < 3`
continue; cell with `faces==0 && extraStrips==0`
continue. `ReadIndices` returns `[]` if count `< 3`.
Water materials `TryResolve` → null → `tex.A < 0` →
face omit (holes), **not** an empty water INDEX16 DIP.

Sea INDEX16 (format 101, PrimitiveCount at +180) is
**not** landscape FG. First-seen skip is the water
empty-out, not a zero-prim `00BF4570` DIP.

---

## 3. Oakvale ExtraStrips as separate INDEX16 DIPs — **MATCH**

Native dest analog (`proofs/leftover-36-native-dest`):

```
00BF4570 dest DIP dest
  dest = mesh at cell+8
  DIP dest mesh+52 IB / mesh+56 VB     ; vtbl+328 type 5 INDEX16 101
  dest = mesh+60
  loop 00BF4E90                         ; ExtraStrip dest DIP again
```

Each `CPatchTesselationEdgeStrip` is its own mesh
node. Cache dest identity native is
`(map, cellX, cellY, meshIndex)`.

Host HEAD:

```
MeshBatches.BuildCells
  EmitMesh(primary StripIndices / PrimitiveCount)
  foreach ExtraStrip
    EmitMesh(extra StripIndices / PrimitiveCount)
```

`Lookout_cells_match_stb_tiles` locks
`draws == cells + extraDraws` with
`extraDraws = ExtraStrips.Count(s => PrimitiveCount > 0)`.
`First_seen_landscape_submits_primary_and_edge_strips`
locks Oakvale stored extras (`withExtras > 0`).
`LandscapeCell.ExtraStrips` is a separate dest field.

Lookout-only ExtraStrip XY `2000–6000` **dropped**
Oakvale MapY=736 dests. That gate is **DISPROVEN**.
After the drop, Oakvale extras **appear** as extra
INDEX16 dest DIPs. Native extra DIP per extra mesh.
Host extra dest draw per extra with prims>0.
**MATCH** live submit.

`LandscapeBufferKey(Map, CellX, CellY)` still skips
`meshIndex` dest identity. Live submit is
`BuildCells`, not that helper. Dest identity leftover
stays open (`leftover-36-native-dest`); it does **not**
undo the ExtraStrip DIP MATCH.

Sibling `oakvale-index16-dips` “Host `BuildCells`
INDEX16 path uses `StripIndices` and **drops extra
faces from the IB**” / “host merges extras” is
**STALE**. Do not restore merge.

---

## 4. Do not fill WAD 1 m cells — **DISPROVEN** leftover

`PARITY` “Did not work”: fill uncovered PATH/WATER
from the WAD table. Extra 1 m quads are not the
`00BF4570` pass. Oakvale adaptive strips omit many
`GROUND_PATH_*` 1 m cells; native still submits
**only** stored tessellation.

`LevHeightField.ToTileTriangles`:

```
STB primary strip + CPatchTesselationEdgeStrip only.
Filling 1 m holes from the cell table is not in the
exe draw path (00BF4570 submits stored tessellation).
Water/decal passes are UNREAD.
```

`WorldGeometryTests.New_game_oakvale_loads_contains_and_sees_maps`
locks `ToTileTriangles` == tile strips (no WAD fill)
and no texture 442. Do **not** re-invent. Do **not**
backfill sea/water holes with 442 or PATH quads.

---

## Host vs native (this leftover)

| Host | Native | Class |
| --- | --- | --- |
| `FirstSeenWaterDrawShouldSubmit=false` | `00B783F0` `je 00B7A865` | **MATCH** skip |
| `TryResolve` drops `SEA_*`/`WATER_*` from FG | not landscape albedo | **PROVEN** omit |
| `ScenePasses` skips Water | still **calls** `00B783F0` | **DIVERGE** call; DIP empty **MATCH** |
| `BuildCells` one INDEX16 per ExtraStrip | `00BF4E90` one DIP per `mesh+60` | **MATCH** |
| Flag-256 17×17 soup draw | no primary INDEX16; extras only | **DIVERGE** primary soup |
| `ToTileTriangles` strip-only | stored tessellation only | **MATCH** |
| WAD 1 m fill | not in `00BF4570` | **DISPROVEN** both |
| 7363 / 442 water mesh | bind unread first-seen | **DISPROVEN** invent |

---

## Classification

| Claim | Status |
| --- | --- |
| First-seen water DIP on Lookout | **DISPROVEN** (empty-out) |
| First-seen water DIP on Oakvale | **DISPROVEN** (empty-out) |
| Oakvale stores sea bank + `SEA_*`/`WATER_*` | **PROVEN** store; draw skip **MATCH** Lookout |
| Native skip empty water INDEX16 (`00B783F0`) | **PROVEN** |
| Native skip empty landscape mesh list (`cell+8`) | **PROVEN** |
| Native skip flag-256 primary INDEX16 (none stored) | **PROVEN** |
| Native `test mesh+70` before DIP | **UNREAD** / not in dump |
| Host skip `PrimitiveCount==0` INDEX16 | **MATCH** `BuildCells` |
| Oakvale ExtraStrip dest as separate INDEX16 DIP | **PROVEN** native / **MATCH** host |
| WAD 1 m PATH/WATER fill | **DISPROVEN** |
| Invent 7363 / 442 first-seen water mesh | **DISPROVEN** |
| First nonempty water DIP clock | **UNREAD** |
| Water/decal as a later INDEX16 pass | **UNREAD** |
| ExtraStrip `fmt` `0x5901`–`0x5904` meaning | **UNREAD** |

**Overall: PROVEN** first-seen empty skip (both maps);
**MATCH** Oakvale ExtraStrip INDEX16 dest DIPs;
**DISPROVEN** WAD 1 m fill and invented water meshes.

---

## Locked (do not revert)

- `FirstSeenWaterDrawShouldSubmit=false`.
- No 442 in Oakvale / first-scene landscape tris.
- ExtraStrips as separate `MeshDraw` dests
  (`draws == cells + extraDraws`).
- Extra XY `2000–6000` gate **DISPROVEN**; Oakvale
  extras stay.
- `ToTileTriangles` == stored strips. No WAD 1 m fill.
- Cell DIP layer `0x40` only.

## Unread leftovers (do not invent)

- First nonempty `00B783F0` after Lookout or Oakvale
  patches (type-3/4/5 enqueue clock).
- Sea prefix words / `00BE91E0` when bind would run.
- Native live Oakvale Present DIP **count**.
- Water/decal INDEX16 later pass.
- ExtraStrip `fmt` as side/LOD.

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\landscape-trace\per-cell-submit-00bf4570.md`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\newgame-trace\water-draw-empty-check-00b783f0-00b783f0.md`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\newgame-trace\water-draw-empty-ret-00b7a865-00b7a865.md`
- `C:\FableCSharp\docs\status\investigations\2026-08-18-landscape-draw.md`
- `C:\FableCSharp\proofs\oakvale-index16-dips\README.md`
- `C:\FableCSharp\proofs\leftover-36-native-dest\README.md`
- `C:\FableCSharp\proofs\water-first-draw\README.md`
- `C:\FableCSharp\src\Fable.Formats\Levels\LandscapeCell.cs`
- `C:\FableCSharp\src\Fable.Formats\Levels\LevTileMesh.cs`
- `C:\FableCSharp\src\Fable.Game\WorldGeometry.cs`
- `C:\FableCSharp\src\Fable.Render\MeshBatches.cs`
- `C:\FableCSharp\src\Fable.Formats\Levels\LandscapeTextures.cs`
