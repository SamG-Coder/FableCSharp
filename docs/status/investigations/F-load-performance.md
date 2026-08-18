# F — New Game / startup load performance

Investigation only. Measured 2026-08-18 against TLC
`C:\Program Files (x86)\Steam\steamapps\common\Fable The Lost Chapters`.
Host path: `EngineLifecycle.Bootstrap` → skip AVI → `EnterGame` →
`Pump` dummy → `Pump` Lookout (`00501450` / `006C2170` / `00B428E0`)
→ `SubmitCurrentWorld` → `PresentToHost`.

Throwaway probe: `tools/_loadprobe` (`dotnet run --project tools/_loadprobe -c Release`
and `… -- breakdown`). No production `src/` edits by this investigation.

Statuses: **PROVEN** body+clock, **PARTIAL** code path + proxy,
**UNREAD** native body not walked here, **DISPROVEN** claimed cost is not
where wall time goes.

Do **not** fix this by “load everything”. Native is fast because open is
names + directories + headers + TNG text; C3D / DXT stay handles until
draw. The host already matches that on open. The hitch is a **draw-time
CPU rebuild** of neighbour landscape as a world-space triangle soup,
with a per-triangle `textures.h` fuzzy match.

---

## 1. Where the wall time goes

Release, process-warm disk cache. Two full New Game spines and one
instrumented breakdown (load region, then time submit pieces without a
second tessellate).

| Stage | Wall | Class | Notes |
|---|---:|---|---|
| WLD parse (`WorldFile.Load`) | **4–16 ms** | **PROVEN** | 194 KB, 398 maps / 141 regions |
| Region graph | **0.1–0.5 ms** | **PROVEN** | 5 KB, 92 nodes |
| BWD | **3 ms** | **PROVEN** | used only to expand static-map set |
| WAD open/index | **0.6–1.4 ms** | **PROVEN** | 174 MB file, 796 entries |
| STB open/index | **0.5–1.8 ms** | **PROVEN** | 598 MB file, 424 entries |
| Per-map TNG (Lookout ContainsMap ×3) | **13 ms** | **PROVEN** | 88+288+88 things |
| All proximity TNG (`004FDBC0`) | **139 ms** | **PROVEN** | **151 maps, 21 746 things** |
| names.bin | **8–18 ms** | **PROVEN** | 397 KB |
| game.bin | **32–36 ms** | **PROVEN** | 996 KB, 14 761 defs |
| script.bin + `ScriptBank` | **16–20 ms** | **PROVEN** | during `LoadQuestsAndActivate` |
| `LevelLibrary` ctor (WLD+WAD+STB again) | **2 ms** | **PROVEN** | after OS cache |
| PeekMapHeader 48-byte LEV + STB size | **0.1–0.8 ms / map** | **PROVEN** | native `00B3EFA0` shape |
| LEV full parse (Lookout 517 KB) | **0.1–1.3 ms / map** | **PROVEN** | cached in `LevelLibrary` |
| STB height `LevHeightField.Parse` | **1–23 ms / map** | **PROVEN** | includes unused fine bilinear grid |
| graphics.big `MeshBank.Open` | **4–20 ms** | **PROVEN** | directory, **0 C3Ds**, 6729 ids |
| textures.big `TextureLibrary` ctor | **5–6 ms** | **PROVEN** | `GBANK_MAIN_PC` directory |
| `EnterGame` (sum of the above + notes) | **284–301 ms** | **PROVEN** | c3d=0 at end of EnterGame |
| Pump1 dummy (index 0) | **1.5 ms** | **PROVEN** | no region, no submit |
| `LoadFromFirstRealRegion` | **119 ms** | **PROVEN** | 3 TNG + wad + 14 headers + hero |
| `PresentWorld` (header-only) | **58 ms** | **PROVEN** | `expandGeometry:false`, **re-parses game.bin** |
| **`TessellateVisible` (first, +planes)** | **30 502 ms** | **PROVEN** | **~99% of submit**; 190 747 tris this run |
| `MeshBatches.Build` land | **31–63 ms** | **PROVEN** | 572 k–1.43 M verts depending on plane set |
| C3D `Meshes.Get` 45 primary ids | **4–37 ms** | **PROVEN** | 25 494 file tris; cache hit 0.1 ms |
| `MeshBatches.BuildMeshes` (CPU xform) | **15–25 ms** | **PROVEN** | 193 instances, 157 k verts |
| Texture first decode (41–43 ids) | **89–199 ms** | **PROVEN** | 35–37 MB RGBA; cached 0–199 ms (GC) |
| Full Pump2 (load+submit) | **43 393 / 45 347 ms** | **PROVEN** | two New Game runs |
| Pump3 already submitted | **0.3 ms** | **PROVEN** | no tessellate, no C3D |
| CPU memcpy mesh upload proxy | **4.3 ms** | **PARTIAL** | 70 MB (`1 161 759 × 60`) |
| CPU memcpy texture upload proxy | **4.4 ms** | **PARTIAL** | 36.7 MB RGBA |
| GPU texture upload | — | **PARTIAL** | see §4; no Vulkan clock |
| Vulkan mesh upload | — | **PARTIAL** | `SetMesh` map+memcpy; every Present |

**Verdict:** New Game hitch is **not** WLD, WAD, STB directory, mesh-bank
open, C3D, or texture decode. It is **`WorldGeometry.TessellateVisible`
on the first `SubmitCurrentWorld`**, ~30–43 s, driven by
`LandscapeTextures.Resolve` against `textures.h` **once per landscape
triangle** on **every opened static map that passes the whole-map AABB**.

Lookout-only `ToTileTriangles`:

| Mode | Time | Tris |
|---|---:|---:|
| `textures=null` (id 414) | **82 ms** | 62 591 |
| `+ textures.h` (submit path) | **807 ms** | 62 591 |

Ten times on one map. Nine to fourteen maps with worse name hits walk
all **3676** `textures.h` keys (136 `LANDSCAPE_*`) and allocate a
`HashSet` per key (`LandscapeTextures.Resolve` fallback). That is the
30–57 s clock.

---

## 2. Requested buckets

### WLD parse — **PROVEN**, cheap

`LoadWorldMap` → `WorldFile.Load(FinalAlbion.wld)`. 4–16 ms.
`LevelLibrary` ctor parses it **again** (~2 ms warm). Native `00507C30`
once.

### Region graph — **PROVEN**, cheap

`LoadRegionGraphFile` → `FinalAlbion_StartingRegionGraph.txt`. <1 ms.
92 nodes. Not on the hitch.

### Per-map TNG — **PROVEN**, two different walks

1. **`004FDBC0` at WLD load** (`SingleGlobalThingsFile` default false):
   every `LoadedOnPlayerProximity` map. **151 files, 139 ms, 21 746
   things.** Native does this walk. Host opens **its own** WAD, then
   disposes it.
2. **`006C2170` ContainsMap** for Lookout: `BowerstoneBridge`,
   `LookoutPoint`, `GuildExterior`. **13 ms**, 464 things. Opens **WAD
   again**. Cached only in `_thingsByMap`, not in `LevelLibrary`.

Filler `SeesMap` TNGs are empty / unused at load. Neighbour object TNG
is **not** loaded at New Game (MATCH: instances stay primary-only at
submit).

### STB open/read — **PROVEN**, split

| Action | Time | Native |
|---|---:|---|
| `StbArchive.Open` directory | 0.5–1.8 ms | once, kept |
| `PeekMapHeader` size only | <1 ms / map | `00B3EFA0` + STB size |
| `LoadHeightField` full parse | 1–23 ms / map | **not** at open |

`EnsureLevels` / `OpenStaticMapBody` leave
`CurrentCompiledLev` / `CurrentHeightField` **null** (MATCH,
`Install_banks` asserts this). Full STB bytes are first read at
**submit tessellate**, not at open. `LevHeightField.Parse` still builds
a fine 1 m bilinear grid and stamps tiles onto it, then
`ToTileTriangles` uses **stored strips only** — the fine grid is unused
work (~100 ms across 14 maps, not the hitch).

Lookout STB body **3.1 MB**; GuildExterior **4.7 MB**; fillers 0.4–2.7 MB.

### LEV parse — **PROVEN**, cheap if header-only

`PeekMapHeader` reads 48 bytes (`LevFile.NativeHeaderBytes`). Full
`LevFile.Parse` is 0.1–1.3 ms and happens at tessellate via
`LoadCompiledLev` (cached). Native open is header-only.

### graphics.big — **PROVEN**, directory-only at init

`EnterGame` / `Init Mesh Bank` → `MeshBank.Open`:
`BigArchive.Open` + MESH sub-bank `ReadEntries`. **4–20 ms, 6729 ids,
`ParsedCount=0`.** MATCH `0049E620` / `00A09F20`. Not the hitch.

### textures.big — **PROVEN**, directory-only at init

`Init Graphics` → `TextureLibrary` ctor: `GBANK_MAIN_PC` entries.
**5–6 ms.** Decode is later (`009BE8B0` per submitted id). MATCH.

### Mesh-bank lookup — **PROVEN**

`009AD410` analogue is `MeshBank.TryGetEntry` / `Get`.
`PresentWorld` (`expandGeometry:false`) only `TryGetEntry` — **no parse**.
`SubmitCurrentWorld` then `Get`s **primary-map Graphic ids only** (45
unique, 193 instances + hero 4299). Neighbour C3Ds stay handles. MATCH
FORWARD_TREE §14.

### C3D parsing — **PROVEN**, not the hitch

First `Get` of 45 Lookout ids: **4–37 ms**, 25 494 file triangles.
Second `Get`: **0.1 ms**. `MeshBank` caches. Native parse is also
draw-later, not open.

### Terrain tessellation — **PROVEN**, the hitch

`SubmitCurrentWorld` → `TessellateVisible` (not `TessellatePrimary`).
`OpenedStaticMaps` for Lookout is **14 maps** (Contains+Sees + BWD
touch):

`LookoutPoint, BowerstoneBridge, GuildExterior, LookoutPoint_Filler_01/05/06/07, PicnicArea, PicnicArea_Filler_02/03, Greatwood_1/2, Greatwood_Filler_04, Fisherman`

`AddTerrain` **always** `LoadHeightField` (full STB) **before** the
whole-map `00BDC2D0` AABB. Reject still paid the parse.
Accepted maps this install: 9 (first spine) / 190 747 tris (breakdown
with planes) / 334 k tris (isolated sum of those 9, matches 1 161 759
submit verts after props).

`TessellatePrimary` is **not** on the New Game pump. Warm call after
submit: 886 ms (Lookout only, still with `textures.h`).

Cost inside `ToTileTriangles` + `LayersAt` + `LandscapeTextures.TryResolve`:
with enums, every triangle (and up to 3 material slots) can fall into
`Resolve`’s O(names) token scan over 3676 header keys. **No per-material
cache.** Native `00BF4570` submits stored patch strips with already-bound
landscape slots. **UNREAD** in this pass: exact native slot→texture
table (not `textures.h` fuzzy match).

### `MeshBatches.Build` — **PROVEN**, small

31–63 ms grouping 191–477 k tris. Once per submit.

### `MeshBatches.BuildMeshes` — **PROVEN**, small

15–25 ms. Re-walks C3D triangles, `Vector3.Transform` per vertex, then
calls `Build` again. Once per submit. PALSKIN (`TrianglesForPose`) only
for bone meshes (hero 4299).

### CPU vertex transforms — **PROVEN**, small

Same 15–25 ms. Host **flattens** world-space soup. Native keeps C3D
local and draws with a world matrix / VS. Lifetime DIVERGE, cost is
not the 43 s.

### Texture decode — **PROVEN**, ~0.1 s

`BindSubmittedTextures` → `TextureLibrary.LoadMany`: LZO framed top mip
+ **CPU DXT→RGBA**. 41–43 unique ids, **35–37 MB** RGBA, **89 ms** first
decode, cache after. Native `009BE8B0` `CreateTexture` DXT FourCC,
`D3DPOOL_SCRATCH` — compressed on device, not a host RGBA bitmap.
DIVERGE, but not the hitch.

### GPU texture upload — **PARTIAL**

No Vulkan clock this run. Code path:

`Pump` (game, after submit) → `PresentToHost` → `SilkEngineHost.Present`
→ **`renderer.SetTextures(engineTex)` every frame** if
`frame.Textures.Length > 0`.

`VulkanLineRenderer.SetTextures`:

- `DeviceWaitIdle`
- `DestroyTextures` (all images)
- upload fallback + white + **every** `GpuTexture` as **R8G8B8A8**
  (staging + `QueueWaitIdle` per image)

Native first-seen create is once per id; Unlock does not wait the GPU
(AVI path comments). Host first Present **and every later Present**
rebuilds the whole set. CPU memcpy proxy 4 ms; real cost is
`DeviceWaitIdle` + N image creates. **Not measured on device.**

### Vulkan mesh upload — **PARTIAL**

`SetMesh` every Present: grow host-visible VB, map, copy 70 MB, unmap.
No `DeviceWaitIdle` here. CPU proxy 4 ms. Native landscape VB stride
is **24** (`00BFE050`), not 60-byte `MeshVertex`. DIVERGE.

---

## 3. Repeated work

| Symbol | Times on New Game spine | Guard | Verdict |
|---|---|---|---|
| `EnsureLevels` | Submit, PresentWorld, Expand, every `OpenStaticMapBody` | `_levels != null` | **Construct once.** Ctor still **re-parses WLD** and **re-opens WAD+STB**. |
| `OpenMeshBank` | `EnterGame` Init Mesh Bank, Submit, PresentWorld, Expand | `Meshes.Opened` | **Once.** Directory only. |
| `PresentWorld` | Once from `SubmitCurrentWorld` (Pump2) | `HeroSpawned && !WorldSubmitted` | **Once** on New Game. Tests call again (58 ms, re-`GameBin.Load`). **Not every frame.** |
| `TessellatePrimary` | 0 on pump | — | **Not** the New Game path. |
| `TessellateVisible` | **Once** per successful submit | via `WorldSubmitted` | **Once**, but that once is **30–43 s**. Reloads `textures.h` every call. |
| `MeshBatches.Build` | 1 land + 1 inside `BuildMeshes` | submit | Twice per submit, cheap. |
| `MeshBatches.BuildMeshes` | 1 | submit | Once, cheap. |
| `LoadGpuTextures` | 0 if engine tex present | Silk fallback | Unused on the live engine-tex path. |
| `BindSubmittedTextures` | 1 | submit | Once. `OpenTextureBank` no-ops if already open. |
| WAD `BbbArchive.Open` | **3** | none shared | Global things, `ApplyLoadJob`, `LevelLibrary`. **Repeated.** |
| STB `StbArchive.Open` | 1 | `LevelLibrary` | Once. |
| graphics.big index | 1 | `MeshBank.Opened` | Once. |
| textures.big index | 1 | `Textures != null` | Once. |
| C3D parse | 45 unique | `_parsed` | Once per id. |
| Texture decode | 41–43 unique | `_cache` | Once per id. |
| `GameBin.Load(game.bin)` | **≥2** | `_defs` vs `WorldGeometry.Build` | `EnsureDefs` caches; **`PresentWorld` loads a second copy every call.** `ScriptBank` loads **script.bin** separately. |
| `HeaderEnums.Load` | 3+ / submit | none | `meshdata.h` + `textures.h` in `Build`, `textures.h` again in `TessellateVisible`. 3–10 ms each. |
| `SetTextures` / `SetMesh` | **every Present** | none in Silk | **Repeated after load.** |

`SubmitCurrentWorld` is **not** every frame. Gate is
`PumpGameUpdate`: `if (HeroSpawned && !WorldSubmitted)`. After a
non-empty mesh, Pump3 is 0.3 ms. `PresentToHost` **is** every game
frame once `WorldSubmitted && WorldCamera.Seeded`.

`WorldSubmitted` is false if `SubmittedMesh.Vertices.Length == 0`, so a
miss would retry every frame. First-seen Lookout submit is nonempty
(**PROVEN**).

---

## 4. Native resource lifetime vs host

Recovered from `FORWARD_TREE.md` §14–15, `EngineLifecycle` constants,
and this clock. Native bodies not re-dumped this pass are marked
**UNREAD**.

| Resource | Native (intended) | Host now | Match |
|---|---|---|---|
| Retail bank names `009A8150` | Insert names only. No `.big` read. | `RegisterRetailBankTable` names only | **MATCH** |
| `MBANK_ALLMESHES` `0049E620` | One 0x460 handle; directory; `004BBFD0` global | One `MeshBank`, directory, `ParsedCount=0` at open | **MATCH** |
| `009AD410` | Hash → handle. **Does not parse C3D.** | `TryGetEntry` at open; `Get` at submit | **MATCH** (parse still at first draw/submit, cached) |
| `GBANK_MAIN_PC` `00416C8A` | Open directory at Init Graphics | `TextureLibrary` at Init Graphics | **MATCH** |
| `009BE8B0` texture | `CreateTexture` DXT, scratch pool, per id | CPU LZO+DXT→RGBA, then Vulkan RGBA8 | **DIVERGE** (format + when) |
| WLD `00507C30` | Once on Loading world | Once in `LoadWorldMap` + again in `LevelLibrary` | **DIVERGE** (double parse, cheap) |
| GTNG | Stem+.gtng; TLC miss | Miss | **MATCH** |
| Global things `004FDBC0` | Per-map proximity TNG | 151 maps, 139 ms | **MATCH** (host extra WAD open) |
| Region graph `00506D40` | Once | Once | **MATCH** |
| `006C2170` objects | ContainsMap `.tng` only | 3 maps | **MATCH** |
| `00B428E0` / `00B41E50` | Mode-1 STB hit; header + patch objects | 14 `PeekMapHeader`; LEV/STB **null** on current | **MATCH** at open |
| Neighbour maps | Headers / background patches | Headers at open; **full tessellate at submit** | **DIVERGE** (submit) |
| `00B3EFA0` | 48-byte LEV + STB size | `PeekMapHeader` | **MATCH** |
| `00BDC2D0` | **Per opened patch** AABB, then `00BF4570` cells | **Whole-map** AABB, then **all stored tiles** | **DIVERGE** (FORWARD_TREE §15 B) |
| Landscape texture bind | Slot already on the patch / material | Per-tri `textures.h` fuzzy `Resolve` | **DIVERGE** (**PROVEN** cost) |
| C3D GPU | Local VB, world matrix, draw later | Flatten to 60-byte world soup, upload | **DIVERGE** |
| First Present | After `006B3FF0` seed + opened maps | Same gate | **MATCH** |
| GPU residency | Texture/mesh stay until evict **UNREAD** | `SetTextures` **destroys and reuploads every Present** | **DIVERGE** |
| Unload | `00B40000` / `00B3EF40` close list from index 1 | `CloseStaticMapFile` clears headers only | **PARTIAL** |
| Quests / script.bin | Load during Loading world | `ScriptBank.Load` ~20 ms | **PARTIAL** (not the hitch) |

**What native initializes once and keeps:** bank manager names, mesh-bank
handle, graphic-bank handle, WLD/region table, def bins, opened static-map
**headers/patches**.

**What native loads per region:** ContainsMap TNG + topology flags +
level-loader job.

**What native loads per map (open):** STB/LEV **header**, optional
background patch. Not C3D. Not full height tessellation.

**What native does at draw:** handle → blob, per-visible-patch strips,
DXT bind.

**What host does extra at first draw:** tessellate **all** opened maps
that pass a **map-sized** AABB, resolve **every triangle’s** material
through `textures.h`, flatten C3Ds, decode DXT to RGBA, upload RGBA.

---

## 5. Call tree (host New Game)

```
Bootstrap                    ~1 ms   names only + D3D notes
FinishStartupVideo ×3        <1 ms   (AVI not timed; skipped)
EnterGame
  OpenTextureBank            5 ms    textures.big index
  OpenMeshBank               4–20 ms graphics.big MESH index
  LoadWorldMap
    WorldFile.Load           4–16 ms
    LoadGtng                 miss
    LoadGlobalThingsFile     139 ms  WAD#1 + 151 TNG
    LoadRegionGraphFile      <1 ms
    LoadQuestsAndActivate    ~40 ms  names + script.bin + game.bin later
Pump1 004189C2 dummy         1.5 ms
Pump2
  EnqueueAfterDummy / 00501450
    ApplyLoadJob             ~100 ms WAD#2 + 3 TNG + InsertThing
      EnsureDefs             36 ms   game.bin #1 (if not via script)
    SetRegionAsLoaded
      OpenStaticMaps         ~5 ms   EnsureLevels = WAD#3+STB+WLD#2
                                     PeekMapHeader ×14
  PumpGameUpdate
    SubmitCurrentWorld       30–43 s
      PresentWorld           58 ms   game.bin #2, handles only
      TessellateVisible      30.5 s  <<< wall
      Meshes.Get ×45         4–37 ms
      Build / BuildMeshes    45–90 ms
      BindSubmittedTextures  89 ms
PresentToHost / every frame
  SilkEngineHost.SetTextures  DeviceWaitIdle + RGBA upload ×43   PARTIAL
  SilkEngineHost.SetMesh      70 MB memcpy                       PARTIAL
```

---

## 6. What not to do

- Do **not** parse every C3D in `graphics.big` at `0049E620`. Native
  does not. Host already does not. 6729 meshes would be a regression.
- Do **not** decode `textures.big` at Init Graphics. Native does not.
- Do **not** pre-tessellate every WLD map. Native open is headers.
- Do **not** treat 151 proximity TNGs as the bug. Native `004FDBC0`
  does that; it is 139 ms.
- Caching more WLD/WAD handles saves milliseconds, not the hitch.

Native-style fix order (not implemented here):

1. **Stop per-triangle `textures.h` scoring.** Bind landscape ids the
   way `00BF4570` already has them (material slot / bank id). Cache
   `Resolve(materialName)` if the fuzzy path must stay. Lookout alone
   drops 807 ms → 82 ms with `enums=null`.
2. **Keep neighbour maps as headers** until a **patch** AABB hits
   (`00BDC2D0`), then submit **that patch’s stored strip**. Do not
   `ToTileTriangles` 14 maps.
3. **Do not `SetTextures`/`SetMesh` every Present.** Upload on change.
   Keep DXT if the device path can take it (`009BE8B0`).
4. Share one WAD/`GameBin`/`HeaderEnums` with `LevelLibrary` /
   `PresentWorld`. Hygiene, not the 43 s.

---

## 7. File sizes (this install)

| File | Bytes |
|---|---:|
| FinalAlbion.wld | 193 539 |
| FinalAlbion.wad | 173 940 664 |
| FinalAlbion_RT.stb | 597 979 518 |
| graphics.big | 243 841 923 |
| textures.big | 533 633 077 |
| game.bin | 996 375 |
| names.bin | 396 920 |
| script.bin | 154 496 |
| textures.h | 445 092 (3676 names) |
| meshdata.h | 330 764 |

---

## 8. Classification index

| Claim | Status |
|---|---|
| Native open does not parse C3Ds | **PROVEN** (host + FORWARD_TREE §14; `ParsedCount=0` after EnterGame) |
| `009A8150` names-only | **PROVEN** |
| `0049E620` MESH directory | **PROVEN** |
| `00B3EFA0` header-only | **PROVEN** |
| `009AD410` handle, draw later | **PROVEN** (parse at first `Get` / submit) |
| `LevelLibrary` caches LEV/STB/TNG | **PROVEN** (dicts); TNG in `ApplyLoadJob` uses a **separate** wad and `_thingsByMap` |
| `SubmitCurrentWorld` every frame | **DISPROVEN** — once, gated by `WorldSubmitted` |
| `PresentToHost` every game frame after seed | **PROVEN** |
| `SilkEngineHost` re-uploads textures every Present | **PROVEN** (code) |
| Hitch is graphics.big / C3D | **DISPROVEN** (4–37 ms) |
| Hitch is texture decode | **DISPROVEN** (89 ms) |
| Hitch is WLD / WAD / STB directory | **DISPROVEN** (<20 ms each) |
| Hitch is `TessellateVisible` + `textures.h` Resolve | **PROVEN** (30.5 s / 43 s Pump2) |
| Per-patch `00BDC2D0` | **UNREAD** (host still whole-map AABB; FORWARD_TREE §15 B) |
| Native GPU eviction / residency | **UNREAD** |
| Vulkan `SetTextures` device time | **UNREAD** (no swapchain in probe) |
