# C — Terrain / static-map render path

Investigation only. No production source was modified.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

---

## Verdict (read this first)

**`WorldGeometry.TessellatePrimary()` is NOT the native semantic equivalent.**

Native landscape is **opened patch objects → per-patch AABB → per-cell stored tessellation → DX9 DIP**. It is not a host-side giant `List<MeshTriangle>`.

`TessellatePrimary` walks only the **primary** map and CPU-unwinds every stored strip into one triangle soup. Native never does that.

`SubmitCurrentWorld` already calls `TessellateVisible`, not `TessellatePrimary`. That is closer (opened maps + 4-plane AABB) but still **DIVERGE**: after the AABB it **dump-all tiles** of the surviving map into one CPU list, then `MeshBatches.Build` regroups by texture. Native iterates **72-byte cells** and issues **DrawIndexedPrimitive** per cell from a 24-byte GPU VB.

Correct host structure (proposed, not implemented here):

```
OpenedStaticMapBody / CEngineLandscapePatch | CLandscapeBackgroundPatch
  origin (WLD MapX/MapY; AABB uses map +96/+98)
  size   (compiled LEV GridWidth/Height metres; map +92/+94)
  AABB   +168 min / +180 max, Z = 0
  cells[cols=+12][rows=+16]   // 16 m, 72-byte records
    flags +60 (bit 0x4 required for FG 00BF4570)
    AABB  +32 min / +44 max   // 3D; 00BF3860
    VB +56 (stride 24), IB +52
    NumVerts +68, PrimitiveCount +70
    stage-0/1 textures
    stored primary strip + CPatchTesselationEdgeStrip extras
```

Do not flatten to a giant CPU triangle list unless a later dump proves a software path. None did.

---

## Recovered native path

```
CLevelLoader apply 006C2170
  Activate Topology 004FCBB0  [record+38]=1   (current ContainsMap only)
  Load things 00522720 / 00521AE0             (ContainsMap TNG)
  SetRegionAsLoaded 004FC8A0
    WorldMap+156 = index
    0082BA00 MiniMap / 005064C0 villages
    SetStaticMapFileForUse 00B428E0
      CloseStaticMapFile 00B40000
      EnablePoolAllocation 00BDA070(1)
      OpenStaticMaps 00B42750(mode=1)
        if [+424]==mode  return
        00B40070
        [+52].vtbl+12(98, +48)
        00B3E820 current handle [+280/+284]
        00B6D4D0 sea name (__ENGINE_SEA_*)
        009CCDC0 STB lookup (__STATIC_MAP_COMMON_HEADER__)
        hit  → 00B420F0 name table
                 for each [+32..+36) slot:
                   00B41E50 (close, 00B3EFA0, 00BE03A0,
                             00BDD0E0, neighbour 00BDF010)
        miss → 00B42530(mode) per list slot   (STB-miss fallback)
      [+432]=3
      LoadWaterData 00B41FA0

Present / flush 00435530 → 009DA9F0(1)
  bits 0x4 → 0x40 → 0x20 → 0x2000
  Landscape vtbl+16 00B6B0B0
    arg+4 == 4    → 00B67480 + 00B671A0 + walk 00BDC060 → 00BF71D0
    arg+4 == 0x40 → 00B68DA0 + 00B67480 + 00B677D0 + 0098B5E0(2)
                    + walk opened-patch list 00BDC2D0
                      AABB then cells 00BF4570
                    + 00B67510 unbind 0/1/2
    other bits    → profiler only
```

| Step | VA | Status |
|---|---|---|
| `PresentWorld` is instances + LEV/STB **headers** (`expandGeometry: false`) | host `17908c3`; `PeekMapHeader` | **PROVEN** |
| `00B3EFA0` remaps stream dwords onto a dest object (not memcpy of 48 file bytes) | `fn 0x00B3EFA0` 599 insns | **PROVEN** |
| Compiled LEV **file** prefix is 48 bytes: v25 / `0x1904` / width@36 / height@40 / cell@44 | `LevFile.ReadHeader`; `LevFormatTests` | **PROVEN** |
| `00B42530` is STB-**miss** fallback | `00B42750` mode-1 hit takes `00B420F0` and **returns**; miss falls to `00B42530` | **PROVEN** |
| `00B428E0` is the STB-hit driver: close → pool → `00B42750(1)` → water | dump + FORWARD_TREE | **PROVEN** |
| Engine owns open/close `00B40000` / `00B42750` | `00B40000` walks list from index 1, water `00B6DB80`, `[+424]=0` | **PROVEN** |
| Current handle `00B3E820` is a refcounted pointer store at `+280/+284` | `fn 0x00B3E820` | **PROVEN** |
| Neighbour attach `00B41E50` = close + header + `00BE03A0` + `00BDD0E0` + `00BDF010` | `fn 0x00B41E50` | **PROVEN** |
| Name table `00B420F0` reads intern blob then `00B41E50` per `[+32..+36)` | `fn 0x00B420F0` | **PROVEN** |
| Terrain submit: patch AABB `00BDC2D0` then cell `00BF4570` | dumps + `b062c5d` | **PROVEN** |
| Host `TessellateVisible` AABB-then-dump-all-tiles | `WorldGeometry.AddTerrain`; FORWARD_TREE §15 | **PROVEN DIVERGE** |

---

## 1. Patch dimensions

| Fact | Evidence | Status |
|---|---|---|
| Compiled LEV grid is **metres at 1.0** (`u32@44 = 65536` = 16.16 `1.0`) | `LevFormatTests.Header_is_version_25_…`; `LevFile.CellSize` | **PROVEN** |
| Lookout `128×128`, Picnic `128×96`, DemonDoor_Guild `64×64`, OakValeEast_v2 `96×160` | same test | **PROVEN** |
| Coarse STB lattice is **16 m** (`LevHeightField.SampleSpacing`) | 36-byte records; Lookout 8×8, Picnic 8×6 | **PROVEN** |
| Fine / WAD cell table is **1 m** (Lookout 16384 records of 21 bytes) | `LevCellGrid`; PARITY | **PROVEN** |
| Drawn FG cell is **16 m**: `00BDC2D0` walks `[patch+16]` rows × `[patch+12]` cols | dump `patch-submit-bit40-frustum-00bdc2d0.md` | **PROVEN** |
| Cell record stride is **72 bytes** (`lea eax,[eax+eax*8]; lea ecx,[ecx+eax*8]`) | same dump `00BDC39E` | **PROVEN** |
| Lookout therefore 8×8 FG cells, Picnic 8×6 | 128/16, 96/16 | **PARTIAL** (inferred from grid÷16; `[+12]/[+16]` not dumped as immediates) |
| AABB fill `00BF6F80` stores start at `+216/+218`, size at `+220/+222` (u16), origin from map `+96/+98` | `fn 0x00BF6F80`; `LandscapeFrustum.MapSizeXOffset=92` etc. | **PROVEN** |
| First-seen AABB start is `(0,0)`; `min.z = max.z = 0` | `LandscapeFrustum.AabbZ`; `00BF708B` / `00BF709C` write 0 | **PROVEN** |
| Map object `+92/+94` size, `+96/+98` origin (u16) | OpenStaticMap compares those against header dest+32..+44 | **PROVEN** as the compare slots; **PARTIAL** that dest+32 is file-width (remap, not memcpy) |

`00B3EFA0` dest layout (stream dword → dest offset): `0→+0`, `1→+4`, `2→+12`, `3→+32`, `4→+36`, `5→+40`, `6→+44`, then `+8/+16/+20/+48…`, then 24 bytes at `+64`, last dword at `+88`. Host `PeekMapHeader` reads the **file** prefix (width at file+36), not this remapped dest. **PARTIAL** equivalence: version/constant/grid numbers match tests; dest packing is not what the host copies.

---

## 2. Tile / chunk organisation

Two files, two jobs:

**WAD compiled `.lev`** — material/theme table + 1 m cell slots. Not the draw mesh.

| Fact | Status |
|---|---|
| v25 / `0x1904`, 255×132 materials from 179, payload tag 21 | **PROVEN** |
| 21-byte cells: bytes 10–13 material slots (`0xFF` unused); u16@+8 is a constant ~60, **not** height | **PROVEN** |
| Material-slot `u32` is **not** a `textures.big` id | **PROVEN** (PARITY; 1911 = villager legs) |

**STB runtime copy** — stored tessellation the GPU submits.

| Fact | Status |
|---|---|
| Bytes 0–2047 pad (`u32[0]=1`). `u32@2048` = section-2 tile (Lookout/Oakvale 6144, Picnic 4096) | **PROVEN** |
| Table at 2056: 36-byte records, two WLD verts + payload `off@+28` / `size@+32`, magic `0x012EC900` | **PROVEN** |
| Payload = `u32` dest size + `u32` packed + **raw LZO** | **PROVEN** |
| Inflated header: extra-count@0, vert-count@+2 (289 = 17×17), PrimitiveCount@+4, flag@+18 (`256` = no primary strip) | **PROVEN** |
| Verts: 15 bytes = u16 X, u16 Y, f32 Z, packed 11-11-10 normal, 3 extra bytes | **PROVEN** |
| IndexCount = PrimitiveCount+2 (D3D strip) | **PROVEN** |
| After the primary strip: `CPatchTesselationEdgeStrip` (30-byte hdr + 15-byte verts + strip) | **PROVEN** |
| Lookout ~63 tiles / Picnic ~47; last table record is a 0,0 sentinel | **PROVEN** (`LevFormatTests`) |
| Section-2 blob is the **west-south** 16 m cell the table does not store (Lookout WLD `[3232,3248]×[3488,3504]`) | **PROVEN** |
| 17×17 grid unwind only when `v=289` and the 16×16 lattice is complete; else the stored strip. Invented 1 m fill is **DISPROVEN** | **PROVEN** / **DISPROVEN** |

`00BDD0E0` (current patch) copies a 20-byte header into `this+36`, then `00BF9290` tile stream into `this+64`. `00BDF010` (neighbour) reads 12 bytes then `00BDE290` / `00BDEDD0`. Tile-stream first byte is a present-flag; 0 → empty vector via `00BF97A0`. **PROVEN** as the ingest, **UNREAD** as the exact in-memory tile-object layout past `+64`.

GPU expand `00BFE050` (only `E8` from `00BF3E17`): 15-byte file → **24-byte** VB (u16 X/Y, f32 Z, float3 normal via `00BFDEC0`, D3DCOLOR extra at +20 BGR). **PROVEN**.

---

## 3. Origin

| Space | Origin | Status |
|---|---|---|
| WLD / STB file XY | WLD world origin (Lookout 3232/3488, Oakvale 3456/736) | **PROVEN** |
| Region-local | current map `(MapX, MapY)` | **PROVEN** |
| Native GPU VB | camera (camera-relative after expand) | **PROVEN** (`FirstSeenLandscapeDeviceVbIsCameraRelative`) |
| Patch AABB | map `+96/+98` + start `(0,0)` first-seen; Z=0 | **PROVEN** |
| Neighbour in primary frame | `ΔMapX / ΔMapY` | **PROVEN** |
| TNG / SHOT2 helper | already region-local | **PROVEN** |

BWD `minX/minY` equals WLD `MapX/MapY`. **PROVEN**.

Host STB verts after `StbFileToRegionLocal` are region-local world. Applying native `T(cam)` to those is **DISPROVEN**. Host identity W ≡ native `(p−cam)*T(cam)`. **EQUIVALENT** (locked in `WORLD_SPACE_CONTRACT.md`).

---

## 4. MapX / MapY conversion

```
localXY = STB.WorldXY − (MapX, MapY)
neighbourOffset = (neighbour.MapX − primary.MapX, neighbour.MapY − primary.MapY)
```

No scale. Units are metres. **PROVEN** (`WorldSpaces`, `WorldPipelineTests`, house vs nearest STB vert).

The old extras gate `2000 ≤ XY ≤ 6000` dropped Oakvale (MapY=736). **DISPROVEN**.

---

## 5. Height scale

Z is **metres as stored**. Coarse STB and tile verts are `f32` Z. Lookout heights sit 20–80 and match TNG Z (~10 cm median residual). There is **no** `×0.01` on landscape (that factor is C3D centimetres only). **PROVEN**.

Host `TryReadSample` rejects Z outside `[0, 200]`. That is a host parse gate, not an exe scale. **PARTIAL**.

---

## 6. Winding

| Fact | Status |
|---|---|
| D3D triangle strip: even `t` → `(a,b,c)`, odd `t` → `(b,a,c)` | **PROVEN** (`LandscapeStrip.Unwind`) |
| No exe write rewinds a face because `n.Z < 0` | **DISPROVEN** as native (`FirstSeenRewindsNegativeNz=false`) |
| First-seen landscape / static-lit apply **D3DCULL_CCW** (`0x01396FB0=3`) | **PROVEN** |
| `0x01396FB8=1` (NONE) is other primitive passes, then restore CCW — not first-seen landscape | **PROVEN** |

---

## 7. Normals

| Fact | Status |
|---|---|
| File: packed signed 11-11-10 after XYZ | **PROVEN** (`PackedDirection.Unpack`) |
| GPU: `00BFDEC0` writes float3 at dest+8 | **PROVEN** |
| Lookout 17×17 tiles: 289/289 unit +Z | **PROVEN** |
| Extra byte0 is always `0xFF` (v3.x = 1 → oD0.w path) | **PROVEN** |
| Host `ToTriangles` uses file packed n as `NormalA/B/C` and `Cross(b−a,c−a)` as face n | **PARTIAL** (face n is host-derived; native VS lights the unpacked vert n) |
| Tile RGB extra is **not** oD0. Multiplying albedo by ExtraRgb made Lookout magenta | **DISPROVEN** as lighting |

FG VS: `dp3 n, −c19`; `max(., c0.x)`; square; `* c20`; `mad c35`; then `add oD0.xyz, lit, c3`. First-seen `c19=(0,1,0,0)`, `c20=(0.25)×3`, `c35=(0,0,0,1)`, leftover `c3=(0,0.125,0)`. **PROVEN**.

---

## 8. UV generation

Tile verts have **no UV**.

| Channel | First-seen source | Status |
|---|---|---|
| FG `oT0.xy` | `v3.yz` = ExtraRgb.YZ (t0 **alpha**, not albedo) | **PROVEN** |
| BG `oT0` | `mov oT0, v3` = ExtraRgb.XY | **PROVEN** |
| FG albedo `oT1` | `dp4(pos, c40/c41)` | **PROVEN** |
| First-seen `c40=c41` | D3D default **0** — no `def`, no `SetVSConstantF(40/41)` | **PROVEN** → `oT1=(0,0)` |
| `UvScale=0.125` table `0x0139C5D8` | written to **c2** then fog-restored; `0x0139C614` stays **c3** (lighting `add r3,r3,c3`) | **PROVEN** — **not** albedo UV |

Host `LandscapeTextures.ProjectOt1` returns `(0,0)`. **PROVEN** match.

World-XY `×0.125` as albedo UV is **DISPROVEN**.

---

## 9. Texture IDs

| Fact | Status |
|---|---|
| Names: `GROUND_PATH_SAND` → `LANDSCAPE_PATH_SAND_01` (4133); `GROUND_GRASS` → `LANDSCAPE_GRASS_PLAIN` (414); cobbles 4118 | **PROVEN** (`LevFormatTests`) |
| Default id 414 | **PROVEN** |
| `SetTexture` stage 0 from `cell+1468` (`00BF50E0`, device vtbl+260) | **PROVEN** |
| Stage 1 at `00BF5491` (`push 1`) | **PROVEN** |
| `PSHADER_LANDSCAPE_FOREGROUND` RGB is **`t1 * v0` ×2**; alpha `t0.a * v0.a` | **PROVEN** |
| Host bind: primary `TextureId` on t1, `TextureId1` on t0 | **PROVEN** as the locked first-seen contract |
| Strip material sampled at triangle **centroid**, not vertex A | **PROVEN** (host `LayersAt(mid)`; PARITY) |
| `SEA_*` / `WATER_*` / `*LAKE*` are the water renderer, not landscape FG | **PROVEN** |
| First-seen water draw empty-out (`00B783F0` → `00B7A865`) | **PROVEN** |
| After the pass `00B67510` unbinds stages 0/1/2 | **PROVEN** |

---

## 10. Neighbour / static-map participation

`00B42750` modes:

| Mode | Who | What |
|---|---|---|
| 1 (`SetStaticMapFileForUse`) | current + neighbours | STB hit: `00B420F0` → `00B41E50` each list slot. Miss: `00B42530(1)` |
| 2 | pointer list `[+32..+36)` | always `00B42530(2)` |

Current map is `CEngineLandscapePatch` (vtbl `0x012A8200`). Neighbour is `CLandscapeBackgroundPatch` (vtbl `0x012A803C`, ctor `00BE6090`). **PROVEN**.

`Activate Topology` `004FCBB0` sets **current-only** 72-byte map-record `+38=1` (`lea eax,[eax+eax*8]; [ecx+eax*8+38]`). Neighbours are **terrain**, not extra `006C2170` TNG loads. **PROVEN**.

Opened set = WLD `NewRegion` `ContainsMap` ∪ `SeesMap` ∪ BWD rectangles that **touch**. Sea maps skipped (`WorldMap.IsSea`). Teleport-graph names (`BowerstoneSlums`, `GreatwoodEntrance`) are exits, not tiles. **PROVEN** (`WorldGeometry.StaticMapsAround`, `WorldGeometryTests`).

Lookout cluster: LookoutPoint + PicnicArea + BowerstoneBridge + GuildExterior + Greatwood_1/2 + Picnic fillers. Oakvale: West/East/Garden + five fillers + four seas (seas opened as headers, no FG tris). **PROVEN**.

Host `PresentWorld` passes `onlyMaps: OpenedStaticMaps` and `expandGeometry: false`. `SubmitCurrentWorld` tessellates every opened name that survives AABB; **instances stay primary-only**. **PROVEN**.

---

## 11. Visibility / culling

Two-level AABB, then stored cells. No host-style “whole world one box then dump”.

| Level | VA | Box | Reject | Status |
|---|---|---|---|---|
| Patch (bit 0x40) | `00BDC2D0` | `+168` n>0 / `+180` n≤0; Z=0 rectangle | `n·p > d` fully outside; missing `[+4]` submits every cell | **PROVEN** |
| Cell (inside 00BF4570) | `00BF3860` | cell `+32` min / `+44` max (3D) | same 4-plane n-vertex; `al=0` early-outs the cell | **PROVEN** |
| Patch (bit 4) | `00BDC060` → `00BF71D0` | same +168/+180 | same 4 planes; then `00BE7BE0` layer-bind, not FG DIP | **PROVEN** |
| Extra sphere / light gather | `00B46280` cap 5; optional sphere at `00BF4784` | | | **PARTIAL** (present; first-seen take **UNREAD**) |

Planes: camera `[0x1436EA0]+448`, stride 16, four side planes from `00B2FD60` NDC `(±1,±1,1)`. `0x122DEDC=0`. **PROVEN**.

Host `TessellateVisible` / `AddTerrain`: one AABB per **opened map** using `FineWidth×FineHeight` and neighbour offset, Z=0. That matches patch AABB grain. Then it emits **every** stored tile of that map. Native then walks cells and can reject per-cell via `00BF3860`. **DIVERGE** (dump-all tiles vs per-cell).

`TessellatePrimary` ignores neighbours entirely. **DISPROVEN** as the submit set.

---

## 12. Render layer

| Bit | Native | Host | Status |
|---|---|---|---|
| `0x4` | landscape BG: `00B671A0` + walk `00BDC060` / `00BF71D0`; `PSHADER_LANDSCAPE_BACKGROUND` `mul_x2 t0*v0` | `SceneLayer.Landscape` → both 0x4 and 0x40 | **PROVEN** |
| `0x40` | landscape FG: compact-bind `00B68DA0` + `0098B5E0(2)` + `00BDC2D0`/`00BF4570`; `PSHADER_LANDSCAPE_FOREGROUND` | same | **PROVEN** |
| `0x20` | static + PALSKIN (MainScene+616) | instances, primary-only | **PROVEN** (not terrain) |
| `0x2000` | sky else-path | `SkyGeometry` (host `Build` adds it; `SubmitCurrentWorld` terrain path does **not** re-add sky — sky is already in expand/`Build`) | **PARTIAL** as to whether `SubmitCurrentWorld` emits sky (it tessellates land only, then concats primary C3Ds) |
| `0x20000` | water | first-seen empty | **PROVEN** omit |

Registration order `00B26A75`: `0x4` then `0x40` then `0x20` then `0x2000`. **PROVEN**.

`00B6B0B0` compares `arg+4` to **4** and **64** only. Other bits are profiler + unbind. **PROVEN**.

---

## 13. Material state (first-seen landscape)

| State | Value | Status |
|---|---|---|
| VS FG | `VSHADER_LANDSCAPE_FOREGROUND` | **PROVEN** |
| PS FG | `PSHADER_LANDSCAPE_FOREGROUND` (2 tex, `mul_x2 t1*v0`) | **PROVEN** |
| PS BG | `PSHADER_LANDSCAPE_BACKGROUND` (`mul_x2 t0*v0`) | **PROVEN** |
| World | native `T(cam)` on cam-rel VB (`00BF46A2`); host identity on region-local STB | **PROVEN** / **EQUIVALENT** |
| WVP | `p * W * V * P`; `SetVSConstantF(c5, 4)` via `00988A50` | **PROVEN** |
| Fog | `FOGENABLE=1` (`00B46890` from `00B67480`); FOGCOLOR `0xFF000000`; table/vertex mode 0; VS `oFog` | **PROVEN** |
| Cull | CCW | **PROVEN** |
| Z | LESSEQUAL; ZENABLE/ZWRITE first-seen write **UNREAD** (D3D defaults TRUE) | **PARTIAL** |
| Alphablend | **off** (landscape) | **PROVEN** |
| Alpha test / fill / color write / stencil | | **UNREAD** |
| Diffuse2X | `0098B5E0(2)` on bit 0x40 | **PROVEN** as the call; state-block body **UNREAD** |
| Sampler MIN/MAG/MIP/ADDRESS | | **TEMPORARY** / **UNREAD** (host LINEAR/REPEAT) |
| Layer-type 4 `c1` flip `(1,0)` / `(0,−1)` | water enqueue only; first-seen FG is not type 4 | **PROVEN** not written |

Shared lighting `00B67480`: `00B46C80` + `00B46890` on `0x1436E9C`, then identity-like 3x4 via `009881F0`. **PROVEN**.

---

## Host vs native — TessellatePrimary / TessellateVisible

| | Native | `TessellatePrimary` | `TessellateVisible` (what submit uses) |
|---|---|---|---|
| Open | headers + patch objects | N/A (needs already-opened `Regions`) | same |
| Maps | current patch + neighbour patches | **primary only** | all `Regions` |
| Cull | per-patch AABB, then per-cell AABB | optional whole-primary AABB | per-opened-map AABB, **no** per-cell |
| Geometry | stored VB/IB, DIP per cell | CPU unwind all tiles → `List<MeshTriangle>` | same soup, more maps |
| Texture bind | per cell, stage 0/1 | deferred to `MeshBatches` group-by id | same |
| Neighbour props | not in landscape submit | n/a | n/a (instances stay primary) |

`TessellatePrimary` has **no callers** outside its definition. Submit does not use it.

`MeshBatches.Build(land)` then `Concat` with primary C3Ds. That is a Vulkan convenience: one vertex buffer, draws sorted by `ScenePasses.Rank`. Native is many DIP calls on per-cell buffers. **DISPROVEN** as native structure; **EQUIVALENT** only if the emitted faces, UVs, and layer bits match.

FORWARD_TREE §15 still records: *“Still DIVERGE: whole-map AABB then dump-all tiles (not per-patch `00BDC2D0`)”*. `TessellateVisible` now AABBs **each opened map** (patch grain) — that part of the note is stale — but **dump-all tiles** and the missing **per-cell `00BF3860` / DIP** remain.

---

## Proposed structure (do not implement in this investigation)

Keep `PresentWorld` header-only.

At submit, for each opened body:

1. Build / reuse a **patch record** (origin, size, Z=0 AABB).
2. `00BDC2D0` four-plane test; skip the patch if fully outside.
3. Iterate **16 m cells** (not 1 m WAD cells, not a flat triangle list).
4. Per cell: require flag bit `0x4`; `00BF3860` cell AABB; bind stage 0/1; submit the **stored** primary strip + extras (IndexCount = PrimitiveCount+2, odd-t unwind). No invented 1 m fill.
5. Emit two layer draws (`0x4` / `0x40`) with the locked VS/PS contract.
6. Leave C3D instances on `0x20`, primary-only.

A `List<MeshTriangle>` is acceptable only as a **debug dump** of those stored strips, not as the runtime object model.

---

## Unread leftovers (do not invent)

- Exact native fill of cell `+32/+44` AABB (likely `00BE5E70` / tessellator `00BE6880`).
- In-memory tile object after `00BF9290` past `+64`.
- Whether `[patch+12]/[+16]` is always `Grid/16` on every map.
- `00B3EFA0` dest↔file field map beyond version/constant and the four u16 compares.
- First-seen take of the `00BF4784` sphere test and light gather `00B46280`.
- `CEngineStateBlockDiffuse2X` apply body.
- Water/decal/proc-texture/shadow/bump landscape variants.
- `00B69330` BLACKOUT VS bind (dump starts mid-instruction; not on the first-seen `00B6B0B0` 4 / 0x40 walk).
- Sampler / alpha-test / color-write / stencil first-seen writes.

---

## Evidence index

Dumps (read first): `tools/Fable.ExeIndex/out/01-sections/landscape-trace/` — especially `INDEX.md`, `openstaticmaps-00b42750.md`, `openonemap-00b42530.md`, `setstaticmapfileforuse-00b428e0.md`, `patch-submit-bit40-frustum-00bdc2d0.md`, `per-cell-submit-00bf4570.md`, `per-cell-settexture-stage0-00bf50e0.md`, `landscape-draw-vtbl16-00b6b0b0.md`, both landscape vtbls.

Live `fn` / `disasm` this session: `00B42750`, `00B3E820`, `00B41E50`, `00B420F0`, `00BF6F80`, `00B3EFA0`, `00BF71D0`, `00BF3860`, `00B40000`, `00A0AD40`, `00BF4649`, `00BF5480`, `00BF55DB`.

Host (not edited): `WorldGeometry.cs`, `EngineLifecycle.cs` (`PresentWorld` / `SubmitCurrentWorld` / `OpenStaticMapsForCurrentRegion`), `LevTileMesh.cs`, `LevHeightField.cs`, `LevCellGrid.cs`, `LandscapeFrustum.cs`, `LandscapeTextures.cs`, `WorldSpaces.cs`, `LandscapeStrip.cs`, `MeshBatches.cs`, `ScenePass.cs`.

Docs: `docs/render/WORLD_SPACE_CONTRACT.md`, `FIRST_SCENE_WORLD_PARITY.md`, `docs/PARITY.md` landscape rows, `docs/runtime/FORWARD_TREE.md` §9 / §15, `docs/status/README.md`.

Tests: `WorldGeometryTests.TessellateVisible_uses_00bdc2d0_aabb`, `LevFormatTests`, `WorldPipelineTests`.
