# 2026-08-18 — New Game load profile

Clocked this session against TLC
`C:\Program Files (x86)\Steam\steamapps\common\Fable The Lost Chapters`.
Host path: `EngineLifecycle.Bootstrap` → skip AVI → frontend New Game
flag → `EnterGame` → `Pump` dummy → `Pump` Lookout (`00501450` /
`006C2170` / `00B428E0`) → `SubmitCurrentWorld` → (no host in probe)
first Present would be `PresentToHost`.

**How measured**

- `dotnet run --project tools/_loadprobe -c Release -- spine` — fresh
  process, New Game only. **Primary cold numbers.**
- `… -- breakdown` — isolated HeaderEnums / Lookout tessellate / C3D /
  decode after a warm disk cache.
- `dotnet run --project tools/_loadprobe -c Release` — isolated WAD/STB
  / per-map TNG / LEV / STB height + a process-warm New Game.
- In-process clocks: `EngineLifecycle.Timing` and
  `LastLoadTiming` (`src/Fable.Game/LoadTiming.cs`).
- Probe does **not** attach Silk/Vulkan. GPU upload is a CPU memcpy
  proxy plus a code-path audit. Do **not** treat that as device time.

Statuses: **PROVEN** body+clock, **PARTIAL** code path + proxy,
**UNREAD** native body or device not walked, **DISPROVEN** claimed
cost is not where wall time goes.

Lifetime class: **PROCESS** once per OS process, **ENGINE** once per
`EngineLifecycle` / Init Graphics, **WORLD** once per WLD load,
**REGION** once per region job, **MAP** once per static-map name,
**THING** per instance / C3D id, **FRAME** every Present.

`LoadTiming.Format()` “First World” **sums nested rows** (`submit/*`
plus `submit`). Ignore that total. Non-overlapping cold sum is below.

Compared with `F-load-performance.md` (same day, earlier tree): live
submit no longer walks `TessellateVisible` + `MeshBatches.Build` as a
world-space soup. It uses `CollectVisibleCells` +
`MeshBatches.BuildCells`. `LandscapeTextures.Resolve` now caches by
material name. Whole-map AABB reject happens **before** STB parse.
Silk skips `SetTextures`/`SetMesh` when the uploaded arrays are the
same reference. The 30–43 s hitch is **gone on this tree**.

---

## 1. Measured New Game spine

Release, TLC. **Cold** = `spine` (fresh process). **Warm** = same
process after isolated reads, or a second `EngineLifecycle`.

| Stage | Cold ms | Warm ms | Class | Lifetime | Status |
|---|---:|---:|---|---|---|
| Locate install / file sizes | — | — | PROCESS | PROCESS | **PROVEN** (path only) |
| `Bootstrap` (names, no `.big`) | **1.7** | 1.5 | ENGINE | ENGINE | **PROVEN** |
| Videos done (`FinishStartupVideo` ×3, no decode) | **0.8** | 0.6 | ENGINE | ENGINE | **PARTIAL** — skip, not play |
| Frontend `RequestNewGame` | **0.1** | — | ENGINE | ENGINE | **PROVEN** (flag only) |
| `EnterGame` (sum of next rows) | **440** | 232–350 | WORLD | WORLD | **PROVEN** |
| `textures.big` `TextureLibrary` ctor | **16** | 5–13 | ENGINE | ENGINE | **PROVEN** directory |
| `graphics.big` `MeshBank.Open` | **14** | 4–16 | ENGINE | ENGINE | **PROVEN** directory, c3d=0 |
| WLD `WorldFile.Load` | **3** | 1–20 | WORLD | WORLD | **PROVEN** 398 maps / 141 regions |
| Region graph | **1** | 0–1 | WORLD | WORLD | **PROVEN** 92 nodes |
| Global TNG `004FDBC0` | **353** | 215–256 | WORLD | WORLD | **PROVEN** 151 maps, 21 746 things |
| Quests + `script.bin` | **50** | 25–42 | WORLD | WORLD | **PROVEN** 6 activated |
| Pump1 dummy (index 0) | **1.4** | 1.4 | REGION | REGION | **PROVEN** no submit |
| Enqueue `00501450` + region TNG | **65** | 58–75 | REGION | REGION | **PROVEN** 3 maps, 464 things |
| STB/LEV open (14 `PeekMapHeader`) | **26** | 8–12 | MAP | REGION | **PROVEN** headers only |
| **`SubmitCurrentWorld`** | **715** | 409–446 | FRAME* | once | **PROVEN** |
| `PresentWorld` (handles, `expandGeometry:false`) | **69** | 100–103 | REGION | once | **PROVEN** c3d=0, inst=280 |
| Terrain `CollectVisibleCells` | **426** | 140 | MAP | once | **PROVEN** 279 cells / 6 maps |
| `MeshBatches.BuildCells` land | **14** | 11 | MAP | once | **PROVEN** 760 014 verts, 558 draws |
| C3D `Meshes.Get` 45 ids | **49** | 21–42 | THING | once/id | **PROVEN** 193 inst + hero 4299 |
| `MeshBatches.BuildMeshes` | **33** | 17–28 | THING | once | **PROVEN** 157 482 verts |
| Texture decode `LoadMany` | **115** | 87–115 | THING | once/id | **PROVEN** 40 files, 33.6 MB RGBA |
| CPU memcpy mesh upload proxy | **7.8** | 3.2 | FRAME | first Present | **PARTIAL** 55.0 MB (`917 496 × 60`) |
| CPU memcpy texture upload proxy | **3.0** | 1.9 | FRAME | first Present | **PARTIAL** 33.6 MB RGBA |
| Vulkan `SetTextures` / `SetMesh` device | — | — | FRAME | first Present† | **UNREAD** no swapchain |
| Pump3 already submitted | **0.2** | 0.2 | FRAME | FRAME | **PROVEN** no tessellate |
| First GPU frame | — | — | FRAME | FRAME | **UNREAD** probe has no host |

\*Submit is **not** every frame. Gate is `HeroSpawned && !WorldSubmitted`.
After a nonempty mesh, Pump3 is 0.2 ms. `PresentToHost` **is** every
game frame once `WorldSubmitted && WorldCamera.Seeded`.

†Silk now uploads only when the vertex/texture **array reference**
changes. First Present still pays `SetTextures` (`DeviceWaitIdle` +
RGBA create) and `SetMesh` (map + memcpy). Later frames with the same
`SubmittedMesh` / `_submittedTextureArray` skip. **PROVEN** code,
**UNREAD** device clock.

**Non-overlapping cold sum to submitted world:**
1.7 + 0.8 + 0.1 + 440 + 1.4 + 65 + 26 + 715 ≈ **1.25 s**.
Matches Pump2 811 ms ≈ region TNG + STB/LEV + submit (65+26+715).
Matches EnterGame 440 ms ≈ banks + WLD + TNG + graph + quests
(16+14+3+353+1+50).

**Verdict:** On this tree the New Game hitch is **not** 30 s of
`textures.h` scoring. Cold wall is **~1.3 s** to a submitted mesh,
**~0.7 s** of that in `SubmitCurrentWorld`. Largest pieces: global
TNG (353 ms, native-shaped), first terrain cell build (426 ms cold /
140 ms warm), texture decode (115 ms), `PresentWorld` game.bin+handles
(69–103 ms), C3D parse (21–49 ms). GPU first Present is still
**PARTIAL**.

---

## 2. Requested buckets

### Videos done — **PARTIAL**, skipped

`PlayStartupVideos` default true. Probe calls `FinishStartupVideo`
until `Frontend`. **0.6–0.8 ms**. Does **not** decode
`lionhead_logo.xmv` / `Microsoft_Logo.xmv` / `intro_comp.xmv`.
Real AVI play is `WmvPlayer` / `PlayAVI` — **UNREAD** this pass.
**ENGINE**.

### Frontend — **PROVEN** cheap on this path

`RequestNewGame` writes LeaveFrontend + `FinalAlbion.wld`. **0.1 ms**.
No menu widget draw, no `00595222`. Frontend Present loop is
**UNREAD**. **ENGINE**.

### New Game / EnterGame — **PROVEN** 232–440 ms

`Init Graphics` opens `textures.big` directory. Init World Init opens
`graphics.big` MESH directory. Then `LoadWorld`. **c3d=0** at end of
EnterGame. **WORLD** + **ENGINE** banks.

### WLD parse — **PROVEN** 1–20 ms

`WorldFile.Load(FinalAlbion.wld)` 194 KB, 398 maps / 141 regions.
`LevelLibrary` ctor parses it **again** (2.9–3.1 ms warm). Native
`00507C30` once. **WORLD**. Repeat is hygiene, not the hitch.

### Region graph — **PROVEN** <1 ms

`FinalAlbion_StartingRegionGraph.txt` 5 KB, 92 nodes. **WORLD**.

### TNG — **PROVEN**, two walks

1. **`004FDBC0` at WLD load:** every `LoadedOnPlayerProximity` map.
   **151 files, 215–353 ms, 21 746 things.** Host opens **its own**
   WAD, then disposes it. **WORLD**.
2. **`006C2170` ContainsMap** for Lookout: `BowerstoneBridge`,
   `LookoutPoint`, `GuildExterior`. **58–75 ms**, 464 things. Opens
   **WAD again**. Cached in `_thingsByMap`, not in `LevelLibrary`.
   **REGION**.

SeesMap filler TNGs are empty at load. Neighbour object TNG is **not**
loaded at New Game (MATCH: instances stay primary-only at submit).

Isolated ContainsMap+around TNG (one WAD): Lookout 9.7 ms / 288 things;
others 0–2.5 ms. Fillers 0 things / 0.1 ms.

### Enqueue — **PROVEN** 58–75 ms region TNG + 8–26 ms headers

Pump2: `EnqueueAfterDummy` → `LoadFromFirstRealRegion` (`00501450`
index 1) → `ApplyLoadJob` (`006C2170`) → `SetRegionAsLoaded` →
`OpenStaticMapsForCurrentRegion`. Breakdown
`LoadFromFirstRealRegion` **73 ms** (TNG + headers + hero spawn).
**REGION**.

### STB open/read — **PROVEN**, split

| Action | Cold/isolated | Native | Lifetime |
|---|---:|---|---|
| `StbArchive.Open` directory | 1.8 ms (0.5 ms repeat) | once, kept | ENGINE/WORLD |
| `PeekMapHeader` size + 48-byte LEV | 0.0–1.2 ms / map | `00B3EFA0` | MAP at open |
| 14 headers in `OpenStaticMaps` | 8–26 ms | mode-1 attach | REGION |
| `LoadHeightField` full parse | 1–33 ms / map | **not** at open | MAP at submit |
| Lookout STB body | 3.1 MB, 32.5 ms first | — | MAP |

`EnsureLevels` / `OpenStaticMapBody` leave `CurrentCompiledLev` /
`CurrentHeightField` **null** (MATCH). Full STB bytes are first read
at **submit cell build**, and only for maps that pass the whole-map
AABB. **PROVEN** this tree (earlier F-doc: reject still paid parse).

### LEV parse — **PROVEN** cheap if header-only

`PeekMapHeader` 48 bytes. Full `LevFile.Parse` 0.1–1.8 ms, cached in
`LevelLibrary`. Submit `LoadCompiledLev` Lookout after height parse
is **0.0 ms** (same `LoadHeightField` already pulled it). **MAP**.

### Mesh bank / graphics.big — **PROVEN** directory-only at init

`MeshBank.Open`: `BigArchive.Open` + MESH `ReadEntries`. **4–16 ms,
6729 ids, `ParsedCount=0`.** MATCH `0049E620` / `00A09F20`. **ENGINE**.

### C3D / `MeshFile` — **PROVEN**, not the hitch

First `Get` of 45 Lookout primary ids: **21–49 ms**, 25 494 file
triangles. Cached `Get`: **0.1 ms**. Neighbour Graphic ids stay
handles. MATCH FORWARD_TREE §14. **THING**.

### Textures.big / `TextureLibrary` — **PROVEN** split

Ctor: `GBANK_MAIN_PC` directory **5–16 ms**. Decode is
`BindSubmittedTextures` → `LoadMany` LZO + CPU DXT→RGBA. First
**87–115 ms**, 40–42 ids, 33.6–35.7 MB RGBA. Cached **0–1 ms**.
Native `009BE8B0` keeps DXT. DIVERGE format, not the hitch. **ENGINE**
open, **THING** decode.

### Terrain — **PROVEN**, largest submit piece, no longer 30 s

Live submit: `CollectVisibleCells` + `BuildCells`, **not**
`TessellateVisible` + `MeshBatches.Build`.

Opened static maps (14): Lookout Contains+Sees + BWD touch.

Accepted after whole-map AABB + planes (6):
`LookoutPoint, PicnicArea_Filler_02, PicnicArea, Greatwood_2,
PicnicArea_Filler_03, Fisherman`. **279 cells**, 253 338 tris →
760 014 land verts.

Rejected maps pay **PeekMapHeader only** (AABB uses grid size).
**PROVEN** (`AddTerrain` / `CollectVisibleCells` return before
`LoadHeightField`).

`CollectVisibleCells` cold **426 ms** / warm **140 ms**. That is
`LoadHeightField` (fine 1 m bilinear grid still built, then unused)
+ `LevTileMesh.ToCells` (stored strips + `LayersAt` /
`LandscapeTextures.TryResolve`). `Resolve` now has a
`ResolveCache` — Lookout `ToTileTriangles` +`textures.h` is **71 ms**
vs **88 ms** with `enums=null` (was **807 ms** in F-doc).

`TessellateVisible` still exists (tests / breakdown). First +planes
**419 ms** / 253 338 tris. Second call no planes **221 ms** /
476 670 tris (all opened maps). `TessellatePrimary` **17 ms** /
62 591 tris. **Not** on the New Game pump.

`LevHeightField.Parse` still stamps a fine grid `ToTileTriangles`
does not use. Isolated Lookout 32.5 ms of 96.7 ms tessellate is
parse. Across 6 accepted maps this is tens of ms, not seconds.

### `MeshBatches` — **PROVEN** small

`BuildCells` (live): **11–14 ms**, 760 k verts, 558 draws (2 per
cell). `Build` (breakdown soup): **40–59 ms**. `BuildMeshes`:
**17–33 ms**, then `Build` again. **MAP** / **THING**, once per submit.

### GPU upload / first frame — **PARTIAL**

Probe memcpy: mesh 55.0 MB **3–8 ms**, RGBA 33.6 MB **2–3 ms**.

Code path (no device clock):

`Pump` (game, after submit) → `PresentToHost` → `SilkEngineHost.Present`

- Engine textures present: `SetTextures` **once** until the array
  reference changes (`ReferenceEquals`). **PROVEN** code.
- `SetMesh` / `SetObjects` same guard. Landscape VB and object VB
  are separate. **PROVEN** code.
- `LoadGpuTextures` only if `frame.Textures` is null (Silk fallback).
  Live engine-tex path does **not** call it. **PROVEN**.
- `VulkanLineRenderer.SetTextures` still `DeviceWaitIdle` + destroy
  all images + upload fallback + white + every `GpuTexture` as
  **R8G8B8A8** (staging + `QueueWaitIdle` per image). **PROVEN** code,
  **UNREAD** device ms.
- `SetMesh` grows host-visible VB, map, copy, unmap. No
  `DeviceWaitIdle`. Native landscape stride **24** (`00BFE050`), host
  `MeshVertex` is 60. DIVERGE.

Pump3 **0.2 ms** proves submit does not rerun. First real frame cost
is the first `PresentToHost` after camera seed. **UNREAD** on device.

---

## 3. Isolated clocks (same install)

| Work | First ms | Repeat ms | Notes |
|---|---:|---:|---|
| WLD parse | 4.5 | 15.6 | 398 / 141; repeat slower (GC) |
| Region graph | 0.5 | 0.1 | 92 nodes |
| BWD | 3.1 | — | 14 maps around Lookout |
| WAD `BbbArchive.Open` | 1.4 | 0.6 | 174 MB, 796 entries |
| STB `StbArchive.Open` | 1.8 | 0.5 | 598 MB, 424 entries |
| All proximity TNG | 215.5 | — | 151 / 21 746 |
| names.bin | 18.3 | 7.2 | 397 KB |
| game.bin | 36.6 | 34.5 | 996 KB, 14 761 defs |
| script.bin + `ScriptBank` | 22.0 | 17.7 | |
| `LevelLibrary` ctor | 3.1 | 2.9 | WLD+WAD+STB again |
| HeaderEnums `textures.h` | 8.5 | 2.8 | 3676 names, 136 `LANDSCAPE_*` |
| HeaderEnums `meshdata.h` | 11.3 | — | |
| Lookout STB height | 24.9–32.5 | cached 0 | includes unused fine grid |
| Lookout LEV parse | 1.8 | cached 0 | 517 KB |
| `ToTileTriangles` enums=null | 88.4 | — | 62 591 tris |
| `ToTileTriangles` +textures.h | 71.0 | — | cache; **not** 807 ms |
| `MeshBank.Open` isolated | 16.3 | 12.7 | 6729 |
| `TextureLibrary` ctor isolated | 5.8 | 5.2 | |
| Fresh 45 C3D parse | 21.4 | 0.1 | 25 494 tris |

Per-map Peek / LEV / STB / tessellate (no `textures.h`):

| Map | Peek | LEV | STB height | Tessellate | Tris | STB bytes |
|---|---:|---:|---:|---:|---:|---:|
| LookoutPoint | 1.2 | 1.8 | 32.5 | 96.7 | 62 591 | 3.09 MB |
| BowerstoneBridge | 0.1 | 0.1 | 6.1 | 56.0 | 53 160 | 3.31 MB |
| GuildExterior | 0.1 | 0.4 | 18.7 | 60.8 | 16 361 | 4.75 MB |
| LookoutPoint_Filler_01 | 0.1 | 0.3 | 8.6 | 49.6 | 42 945 | 3.08 MB |
| LookoutPoint_Filler_05 | 0.2 | 0.3 | 6.4 | 37.6 | 675 | 2.17 MB |
| PicnicArea_Filler_02 | 0.4 | 0.1 | 4.9 | 51.4 | 48 327 | 2.68 MB |
| PicnicArea | 0.0 | 0.1 | 5.3 | 50.7 | 46 041 | 2.66 MB |
| Greatwood_1 | 0.0 | 0.3 | 20.2 | 93.7 | 80 497 | 3.36 MB |
| Greatwood_2 | 0.1 | 0.1 | 7.4 | 83.0 | 68 732 | 3.71 MB |
| Greatwood_Filler_04 | 0.1 | 0.2 | 12.1 | 16.8 | 29 642 | 1.12 MB |
| PicnicArea_Filler_03 | 0.1 | 0.2 | 1.3 | 6.1 | 12 236 | 0.41 MB |
| Fisherman | 0.0 | 0.1 | 3.7 | 16.2 | 15 411 | 1.85 MB |
| LookoutPoint_Filler_06 | 0.1 | 0.3 | 1.0 | 9.1 | 52 | 1.51 MB |
| LookoutPoint_Filler_07 | 0.6 | 0.3 | 4.4 | 18.2 | 0 | 2.12 MB |

Submit only tessellates the 6 AABB-visible maps. The other 8 stay
headers. Isolated tessellate of all 14 is **not** the New Game path.

---

## 4. Repeated parse / read / build

| Symbol | Times on New Game spine | Guard | Lifetime | Verdict |
|---|---|---|---|---|
| `EnsureLevels` / `LevelLibrary` ctor | Submit, PresentWorld, Expand, every `OpenStaticMapBody` | `_levels != null` | WORLD | **Once.** Ctor still **re-parses WLD** and **re-opens WAD+STB**. |
| `OpenMeshBank` | EnterGame, Submit, PresentWorld | `Meshes.Opened` | ENGINE | **Once.** Directory only. |
| `OpenTextureBank` | Init Graphics, BindSubmittedTextures | `Textures != null` | ENGINE | **Once.** |
| `PresentWorld` | Once from submit | `HeroSpawned && !WorldSubmitted` | REGION | **Once** on New Game. Tests/probe second call 46–58 ms, **re-`GameBin.Load`**. |
| `WorldGeometry.Build` | every `PresentWorld` | none for defs | REGION | Reloads **meshdata.h** + **game.bin** every call. `EnsureDefs` cache is unused here. |
| `TessellateVisible` | 0 on pump | — | MAP | **Not** the live submit. |
| `CollectVisibleCells` | 1 | submit | MAP | **Once**, 140–426 ms. `LoadCells` cached after. |
| `MeshBatches.BuildCells` | 1 | submit | MAP | Once. |
| `MeshBatches.Build` | inside `BuildMeshes` only | submit | THING | Once (props). Land no longer uses `Build`. |
| `MeshBatches.BuildMeshes` | 1 | submit | THING | Once. |
| `LoadGpuTextures` | 0 if engine tex present | Silk fallback | FRAME | Unused on live path. |
| `BindSubmittedTextures` | 1 | submit | THING | Once. Decode cached in `TextureLibrary._cache`. |
| WAD `BbbArchive.Open` | **3** | none shared | WORLD/REGION | Global things, `ApplyLoadJob`, `LevelLibrary`. **Repeated.** |
| STB `StbArchive.Open` | 1 | `LevelLibrary` | WORLD | Once. |
| graphics.big index | 1 | `Opened` | ENGINE | Once. |
| textures.big index | 1 | `Textures != null` | ENGINE | Once. |
| C3D `MeshFile.Parse` | 45 unique | `_parsed` | THING | Once per id. |
| Texture decode | 40 unique | `_cache` | THING | Once per id. |
| `GameBin.Load` | **≥2** | `_defs` vs `Build` | WORLD | `EnsureDefs` caches; **PresentWorld loads a second copy**. |
| `HeaderEnums.Load` | 2 / submit | `LandscapeEnums` once; `Build` always | WORLD | `textures.h` cached on `LevelLibrary`. `meshdata.h` **every** `PresentWorld`. 3–11 ms. |
| `LandscapeTextures.Resolve` | per unique material | `ResolveCache` | MAP | **Cached** this tree. Fallback still O(names) on first miss. |
| `SetTextures` / `SetMesh` | first Present, then skip | `ReferenceEquals` | FRAME | **Not** every Present if arrays are stable. Device still rebuilds the whole set **when** `SetTextures` runs. |

`WorldSubmitted` is false if `SubmittedMesh.Vertices.Length == 0`, so
a miss would retry every frame. First Lookout submit is nonempty
(**PROVEN**, 917 496 verts).

---

## 5. Audit (parse vs index vs build)

| Type | What New Game does | Repeat? | Status |
|---|---|---|---|
| `BigArchive` | Footer + named sub-banks. `Read` only on `Get` / `TryLoad`. | Open once per bank | **PROVEN** |
| `BbbArchive` | Footer + 796 entries. `Read` TNG / 48-byte LEV prefix. | **Opened 3×** | **PROVEN** |
| `StbArchive` | Footer + 424 entries (truncated last record). `Read` full LEV body at `LoadHeightField`. | Open once | **PROVEN** |
| `HeaderEnums` | Regex scan of `textures.h` / `meshdata.h`. | textures.h once; meshdata.h per `PresentWorld` | **PROVEN** |
| `TextureLibrary` | Directory at ctor. LZO+DXT→RGBA at `TryLoad`. | Decode once/id | **PROVEN** |
| `MeshBank` | MESH directory. `Get` → `MeshFile.TryParse`. | Parse once/id | **PROVEN** |
| `MeshFile` | C3D / LZO helpers / strips. Only via `Get`. | 45 ids | **PROVEN** |
| `LevFile` | `ReadHeader` 48 B at peek. Full material table at `Parse`. | Cached / map | **PROVEN** |
| `LevHeightField` | Vertex stream + **unused** fine bilinear grid + tile stamp. | Cached / map; 6 maps at submit | **PROVEN** (unused grid **PARTIAL** cost) |
| `ThingFile` | ASCII line parse. No binary. | 151 + 3 files | **PROVEN** |
| `Tessellate*` | Soup path still in API. Submit uses cells. | 0 on pump | **PROVEN** |
| `MeshBatches` | `BuildCells` land, `BuildMeshes` props. | Once | **PROVEN** |
| `LoadGpuTextures` | Silk fallback decode+wrap. | 0 live | **PROVEN** |
| `SetTextures` | WaitIdle + destroy + RGBA upload. | First Present | **PARTIAL** |
| `SetMesh` | Host-visible map+copy 55 MB. | First Present | **PARTIAL** |

---

## 6. Call tree (host New Game, this clock)

```
Bootstrap                         2 ms    names only          ENGINE
FinishStartupVideo ×3             1 ms    skipped AVI         ENGINE
RequestNewGame                    0 ms    flag                ENGINE
EnterGame                       440 ms
  OpenTextureBank                16 ms    textures.big index  ENGINE
  OpenMeshBank                   14 ms    graphics.big MESH   ENGINE
  LoadWorldMap
    WorldFile.Load                3 ms                        WORLD
    LoadGtng                      miss
    LoadGlobalThingsFile        353 ms    WAD#1 + 151 TNG     WORLD
    LoadRegionGraphFile           1 ms                        WORLD
    LoadQuestsAndActivate        50 ms    qst + script.bin    WORLD
Pump1 dummy                       1 ms                        REGION
Pump2
  EnqueueAfterDummy / 00501450
    ApplyLoadJob                 65 ms    WAD#2 + 3 TNG       REGION
    SetRegionAsLoaded
      OpenStaticMaps             26 ms    WAD#3+STB+WLD#2     REGION
                                      PeekMapHeader ×14       MAP
  PumpGameUpdate
    SubmitCurrentWorld          715 ms
      PresentWorld               69 ms    game.bin #2         REGION
      CollectVisibleCells       426 ms    6 maps / 279 cells  MAP
      BuildCells                 14 ms                        MAP
      Meshes.Get ×45             49 ms                        THING
      BuildMeshes                33 ms                        THING
      BindSubmittedTextures     115 ms    40 RGBA             THING
Pump3                             0 ms    WorldSubmitted      FRAME
PresentToHost / first frame       ? ms    SetTextures+SetMesh FRAME  UNREAD
Later Present                     0 ms    same refs skip      FRAME
```

---

## 7. Classification index

| Claim | Status |
|---|---|
| Native open does not parse C3Ds | **PROVEN** (`ParsedCount=0` after EnterGame) |
| `009A8150` names-only | **PROVEN** |
| `0049E620` MESH directory | **PROVEN** |
| `00B3EFA0` header-only at open | **PROVEN** |
| `009AD410` handle, parse at first `Get` | **PROVEN** |
| Hitch is `TessellateVisible` + uncached `textures.h` | **DISPROVEN** on this tree (was **PROVEN** in F-doc) |
| Live submit is `CollectVisibleCells` + `BuildCells` | **PROVEN** |
| AABB reject before STB parse | **PROVEN** (6/14 maps tessellated) |
| `Resolve` uncached per triangle | **DISPROVEN** (`ResolveCache`; Lookout 71 ms vs 88 ms null) |
| Hitch is graphics.big / C3D | **DISPROVEN** (14–49 ms) |
| Hitch is texture decode | **DISPROVEN** (87–115 ms) |
| Hitch is WLD / WAD / STB directory | **DISPROVEN** (<20 ms each) |
| Largest remaining CPU is global TNG + first cell build | **PROVEN** (353 + 426 ms cold) |
| `SubmitCurrentWorld` every frame | **DISPROVEN** — once, `WorldSubmitted` |
| `PresentToHost` every game frame after seed | **PROVEN** (code) |
| Silk re-uploads textures every Present | **DISPROVEN** (same-ref skip). First Present still full `SetTextures`. |
| Vulkan `SetTextures` device time | **UNREAD** |
| Native GPU eviction / residency | **UNREAD** |
| Per-patch `00BDC2D0` (not whole-map AABB) | **UNREAD** / host still whole-map |
| Real AVI + frontend UI draw cost | **UNREAD** |

---

## 8. What not to do

- Do **not** parse every C3D in `graphics.big` at `0049E620`. Host
  already does not. 6729 meshes would be a regression.
- Do **not** decode `textures.big` at Init Graphics.
- Do **not** pre-tessellate every WLD map. Native open is headers.
- Do **not** treat 151 proximity TNGs as a bug. Native `004FDBC0`
  does that; it is the largest **EnterGame** slice (0.2–0.4 s).
- Do **not** “load everything” to hide the remaining 0.7 s submit.
  Cell build is already region-visible maps only.

Remaining native-shaped cuts (not implemented here):

1. Share one WAD / `GameBin` / `HeaderEnums` across global TNG,
   `ApplyLoadJob`, `LevelLibrary`, `PresentWorld`. Saves the
   **69–103 ms** `PresentWorld` reload and two extra WAD opens.
2. Skip the unused fine bilinear grid in `LevHeightField.Parse`.
3. Bind landscape ids from the patch/material slot (`00BF4570`)
   instead of `textures.h` even with a cache.
4. Keep neighbour maps as headers until a **patch** AABB hits.
5. First Present: do not `DeviceWaitIdle` + recreate every image.
   Keep DXT if the device path can take it (`009BE8B0`).

---

## 9. File sizes (this install)

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
