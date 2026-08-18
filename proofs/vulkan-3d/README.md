# Dx9Vulkan 3D path vs frontend 2D batch (after Leave)

Investigation only. Production `src/` was not edited.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** / **EQUIVALENT** / **TEMPORARY** / **DIVERGE** / **LEFTOVER**.

Question: what does the host actually implement for the **3D**
Dx9Vulkan path versus the **frontend 2D** batch, and what is
still **UNREAD** on the no-save New Game path **after Leave**?

Siblings: `proofs/c3d-first-submit/`, `proofs/landscape-first-draw/`,
`proofs/terrain-first-draw/`, `proofs/stb-first-open/`,
`proofs/camera-after-leave/`, `docs/render/DX9_VULKAN_PARITY.md`,
`implementer/frontend/06-dx9-submit.md`, `07-dx9-to-vulkan.md`.

---

## Verdict

**Two Present families, one engine layer walk.** Frontend 2D is a
separate dest-pixel / XYZRHW pipeline. World 3D is WVP +
`MeshVertex` + `00B26A75` bits. They share `00B27D90` during
`0042DF9E`, but that walk is **empty** until after Leave +
world/region load.

| Family | Native DIP site | First nonempty | Host object |
|---|---|---|---|
| Type `0x22` sprites | `00BAE2D0` → `00A0AEA0` DIPUP vtbl **336** | Press Start dest nonempty | `Dx9VulkanFrontend.BuildBatch` → `FrontendSubmitBatch` |
| Type `0x27` glyphs | `00AB7C20` → `00A0ABE0` DP vtbl **324** | PRESS_START text | same batch, 6 verts, no IB |
| Display `+16020` | `009DA9F0` DPUP vtbl **332** | first-seen **empty skip** `009DB6E6` | **not** the type-`0x22` batch |
| Landscape FG | `00BF4570` strip type 5, device vtbl **328** | after Leave + patches | `MeshBatches.BuildCells` / land VB |
| Landscape BG | `00BDC060` → `00BF71D0` | after patches; **not** stored cells | host bit `0x4` is **not** the cell mesh |
| Static C3D | `00BB2540` FVF `0x112`, `009881F0` W | after `00501450` / `006C2170` | `MeshDraw.World` + objects VB |
| PALSKIN | slots 8+10 on bit `0x100` | hero 4299 with Lookout | same soup, pass `0x100` |
| Sky | `0x2000` midday dome | New Game submit | `SkyViewProjection` |

Frontend **does** call engine `vtbl+32` (`0042E0BB` → `00B27D90`).
That is **DISPROVEN** as first terrain / static-C3D DIP (empty
`+44`, no type `0x18`). First 3D DIP is **after Leave**.

`009DA9F0` is **DISPROVEN** as the 3D layer walker. Host
`FlushSubmittedLayers` on that site is a **DISPROVEN pairing**.

---

## Two Vulkan translations (do not collapse)

Translations live in `src/Fable.Render/Parity/Dx9Vulkan/`.

### A. Frontend 2D — `Dx9VulkanFrontend`

```
frontend.bin persist
  → FrontendLayout dest pixels
  → 0041BEB0 type 0x22 rec 0xC0 dest +0x15C
  → 00B23BC0 / 00B324A0
  → 00BACFD0 + 00BAE2D0   (or 00BAD8A0 dest copy)
  → 00A0AEA0 DrawIndexedPrimitiveUP
  → DestPixelToDx9Clip → Dx9ClipToVulkanNdc
  → FrontendSubmitBatch
  → VulkanLineRenderer.DrawFrontend
```

| Semantic | DX9 | Vulkan | Status |
|---|---|---|---|
| VS | `VSHADER_2D_SPRITE` `mov oPos, v0` | passthrough `gl_Position = inPos` | **PROVEN** |
| PS | `PSHADER_2D_CLOCK_SPRITE` `mul r0, t0, c0` | `texture * c0=(1,1,1,1)` | **TEMPORARY** identity c0; vertex diffuse **UNREAD** by this PS |
| Vert | stride 32, RHW 1, DIFFUSE+TEX1 (FVF `0x144` 28 + pad) | `FrontendGpuVertex` NDC+color+uv | **EQUIVALENT** |
| Topology | prim 4 TRIANGLELIST, INDEX16 `0,1,2,1,3,2` | `CmdDrawIndexed` | **PROVEN** |
| Dest | rec+12 / instance+72 screen px | viewport inverse then `Y *= -1` | **EQUIVALENT** (clip-Y is not a Fable 2D write) |
| Half-pixel | sprite dest `fsub 0.5` **UNREAD**; glyphs `fsub [0x122F59C]=0.5` | sprites unshifted | **UNREAD** as dest-pixel write |
| Blend default | +164 else SRCALPHA/INVSRCALPHA | `SRC_ALPHA` / `ONE_MINUS_SRC_ALPHA` | **PROVEN** |
| Z | `00BAE2D0` writes 0 at +10324/+10344 | test=0 write=0 | **TEMPORARY** (slot RS **PARTIAL**) |
| Sampler / scissor / alphatest | first-seen writes **UNREAD** | LINEAR/REPEAT; no scissor; no discard | **UNREAD** / **TEMPORARY** |
| `+16020` HUD | `009DB700` enqueue; `009DA9F0` drain | **not** `BuildBatch` | **PROVEN** separate family |
| Empty dest | `0,0,0,0` → `00BAD8A0` early-out | `AppendRecord` skip | **PROVEN** |

`00BAD8A0` `E8 009DB700` is **DISPROVEN**. Callers of `009DB700`
are only `009DBFF0` / `009DD8F0`. Status README row that still
pairs nonempty dest with `009DB700` is leftover vs
`implementer/frontend/06-dx9-submit.md`.

Host Present: `EngineLifecycle.CompositeFrontendPresent` →
`FrontendBatch`. `RequestNewGame` **nulls** the batch at Leave
(`0042F2A2` / `0042EBB6` teardown Present is clear+Present, not
`0042DF9E`).

### B. World 3D — the rest of `Dx9Vulkan*`

```
Leave 0042F2A2
  → Init Game 004184BD → 00416953 Load FinalAlbion.wld
  → 00B23DC0 → 00B428E0 → 00B42750  FinalAlbion.stb MISS
  → later 00501450 Lookout + 006C2170 TNG
  → 00B27D90  (game caller UNREAD)
       0x4   00B6B0B0 → 00BDC060 → 00BF71D0
       0x40  00BDC2D0 → 00BF4570
       0x8/10 enqueue type 0x18 → slot 0
       0x20  00B33010 → 00B849F0(0) → 00BB2540
       0x100 PALSKIN slots 8+10
       0x2000 sky
  → 00988A50 W×V×P → c5–c8
  → Dx9VulkanProjection.ToVulkanWvp  (clip Y only)
  → VulkanLineRenderer.DrawMeshBatches
```

| Semantic | DX9 | Vulkan | Status |
|---|---|---|---|
| VS oPos | `dp4` c5–c8 | push `mat4` after clip-Y | **EQUIVALENT** |
| P | `009883F0` `M22=+1`, `clip.w=view.z` | do **not** bake Y into Fable P | **PROVEN** / flip **EQUIVALENT** |
| Static verts | `00BB2540` file-local copy, no `fmul` | `MeshVertex` local + `MeshDraw.World` | **PROVEN** |
| Land W | native cam-relative + `T(cam)` | host STB world-space → identity W | **EQUIVALENT** |
| Land DIP | INDEX16 strip, type 5, mesh `+52/+56` | unwind to list, `BuildCells` | **EQUIVALENT** |
| ZFUNC | `D3DCMP_LESSEQUAL` | `LESS_OR_EQUAL` | **PROVEN** |
| Z enable/write | first-seen write **UNREAD** | test=1 write=1 | **TEMPORARY** |
| Cull | `0x01396FB0=3` CCW | BACK + `FRONT_FACE_CCW` | **PROVEN** / **EQUIVALENT** |
| Fog / dirlight / leftover `c3` | `oFog`; c19/c20/c35; c3=`(0,0.125,0)` | same in GLSL | **PROVEN** (green is leftover, not invented ambient) |
| Sky PS `c0/c1/c2` | writer **UNREAD** | stand-in `t1*v0*v0.w` | **UNREAD** / **TEMPORARY** |
| Half-pixel / sRGB / stencil / alphatest | **UNREAD** | none / UNORM / off / no discard | **UNREAD** |

`DX9_VULKAN_PARITY.md` row “2D / ortho = PlayAVI only / UNREAD”
is leftover vs the **implemented** frontend batch. PlayAVI stays a
third pipeline (`SetVideoFrame`). First-seen **3D** still does not
use an ortho P.

---

## Native frame split (`0042DF9E` vs after Leave)

```
0042EC7C retail pump
  0042DF9E  frontend frame
    009D8CF0 / 009BEF20
    00595582 / 00595222  [ui+84] vtbl+8
      0041BEB0 type 0x22 → 00B23BC0 → 00B324A0     2D insert
    0042E0BB  [retail+88].vtbl+32 = 00B27D90       3D walk EMPTY
    009D9C80 / 009DA9F0(1) ×2                      2D dest empty
    009BEF50 / 009BEEB0
msg 15 → [retail+41]=1
0042F2A2 Leave
  0042EBB6  009BE420 + 009BEEB0   teardown Present, not 0042DF9E
  FrontendBatch = null            host
  FinalAlbion.wld
  00418DCA Init Game → 00416953
    00B428E0 / 00B42750(1)  Data\Levels\FinalAlbion.stb MISS
004189C2
  WorldFrame<=1: skip 00435530
  first 004AEA70=1: 00435530 dest EMPTY, no region, no DIP
later 00501450 Lookout + 006C2170
  UNREAD type-0x18 packer → 00B324A0
  later 00B428E0 Lookout STB   site UNREAD
next 00B27D90 → first 00BF4570 / 00BB2540
```

| Claim | Status |
|---|---|
| Frontend Present is 2D UI + empty `009DA9F0` | **PROVEN** |
| Frontend never reaches `00B27D90` / `00B6B0B0` / `00B33010` | **DISPROVEN** (`0042E0BB`) |
| Frontend issues first land / static C3D DIP | **DISPROVEN** |
| Type `0x22` is C3D/landscape packer | **DISPROVEN** (UI) |
| `009DA9F0` walks 3D layers | **DISPROVEN** (2D `+16020`) |
| First 3D DIP is after Leave + maps/things | **PROVEN** |
| First `00435530` already has terrain/C3D | **DISPROVEN** (empty dest) |
| Game caller of `012A0F3C+32` after Leave | **UNREAD** (`00435530` has no `E8`/`[reg+32]`) |
| `00B25950` inside `00435530` | **DISPROVEN** pairing |
| Frontend / Leave opens STB | **DISPROVEN** |
| First-seen STB name is `FinalAlbion_RT.stb` | **DISPROVEN** (`FinalAlbion.stb` miss) |
| WorldCamera during frontend | **DISPROVEN** (alloc at Init World) |
| Attract / `CS_ATTRACT_*` / `UseCamera` on frontend | **DISPROVEN** |

---

## What host implements

### Implemented (live path)

| Piece | File | Matches native? |
|---|---|---|
| 2D dest → NDC batch | `Dx9VulkanFrontend.cs`, `FrontendDraw.cs` | **EQUIVALENT** family A |
| 2D pipeline + `DrawFrontend` | `VulkanLineRenderer.Frontend.cs`, `LineShaders.Frontend*` | **PROVEN** site; PS c0 **TEMPORARY** |
| Leave clears 2D batch | `EngineLifecycle.RequestNewGame` | **PROVEN** timing |
| WVP + clip-Y | `Dx9VulkanProjection`, `Dx9VulkanShaderConstants` | **EQUIVALENT** |
| Depth / cull / blend / color | `Dx9VulkanDepth` / `RasterState` / `BlendState` / `Color` | as PARITY table |
| Layer rank `0x4→0x40→0x20→0x100→0x2000` | `ScenePasses.Registration` | **PROVEN** order |
| Land cells `00BF4570` | `MeshBatches.BuildCells`, `LandscapeStrip` | **EQUIVALENT** unwind |
| Static local verts + instance W | `MeshDraw.World`, `InstanceDraw.StaticLit` | **PROVEN** as type; live submit still Concat |
| PALSKIN file triangles on `0x100` | `MeshBatches.BuildMeshes` | **PROVEN** dest; type1/Flag1 **UNREAD** |
| Sky `0x2000` | `SkyGeometry` + `SkyViewProjection` | **PROVEN** bit |
| One-shot `SubmitCurrentWorld` after `HeroSpawned` | `PumpGameUpdate` | **PROVEN** *timing* vs `006C2170`; redraw **TEMPORARY BRIDGE** |
| Host Present order | mesh → gizmos → fade → **frontend** → video | 2D after 3D is host; native frontend frame has no 3D DIP |

`SilkEngineHost` mutually excludes AVI vs `FrontendBatch`. After
Leave the batch is null, so DrawFrontend no-ops even though
`Draw` still binds the leftover `ScriptedCamera` (**LEFTOVER**).

### Host DIVERGE (do not treat as native)

| Host | Native | Class |
|---|---|---|
| `SubmitCurrentWorld` `Concat(land, objects+sky)` one `TexturedMesh` | separate families / VBs / bits | **DISPROVEN** structure |
| `FlushSubmittedLayers` on `009DA9F0` | walk is `00B25950` via `00B27D90` | **DISPROVEN** pairing |
| One-shot `WorldSubmitted` | every `00B27D90` | **TEMPORARY BRIDGE** |
| `InstanceDraw.StaticLit` unused on live Concat | `00BB2540` | closer type, **not** live feed |
| PALSKIN 4299 in same `BuildMeshes` soup as static | slots 8/10 on `0x100`, not `0x20` | pass bit set; container **DIVERGE** |
| `LevelLibrary` ctor opens `FinalAlbion_RT.stb` | first-seen miss `.stb`; `+424=0` | **DIVERGE** (after Leave, not frontend) |
| `BuildFrame` always carries `Camera` | frontend Present is 2D | **LEFTOVER** |
| Intra-pass sort by texture id | exe walk order | assumption |
| Sampler LINEAR / Z on / fill / colorwrite | first-seen RS **UNREAD** | **TEMPORARY** |

`WorldGeometry.Build(expand=true)` / `FirstSceneWorld` Oakvale soup
is **DISPROVEN** as this Present. Live open is
`PresentWorld(expand=false)`.

---

## UNREAD after Leave (do not invent)

Render / submit leftovers on the no-save Lookout path. Oakvale
intro fiber leftovers stay in `docs/status/README.md` §2.

### Must-have for a native 3D Present

| Left | Status | Why it matters |
|---|---|---|
| Game caller of engine `vtbl+32` `00B27D90` | **UNREAD** | first nonempty land/C3D drain site after Leave |
| Native Lookout STB hit after `FinalAlbion.stb` miss | **UNREAD** | `004FC8A0` is MiniMap (**DISPROVEN**). Without `+424` native list `+44` stays empty |
| Thing packer `[rec+0]=0x18` | **UNREAD** | `0077BA40` / `007E15C0` / `004C0050` are **DISPROVEN** as that packer |
| First `00BB2540` instance in slot-0 order | **UNREAD** | site **PROVEN**; clock not |
| Consumed first-Present helper (`GameCamera+4` vs unread bind) | **UNREAD** | ctor packer / `00988A50` / FOV 70 **PROVEN**. Host hero+V4 is **DIVERGE** |
| `00988290` identity-W flag bit 0 | **PARTIAL** | identity would pile cm verts at origin |
| Mesh root 48-byte (`00A89564`) into 3×4 | **UNREAD** | exe `0.01` site **UNREAD**; host `ObjectTransform` product **EQUIVALENT** |

### Same after-Leave clock, not first DIP

| Left | Status |
|---|---|
| `004978A0` LCG seed for `006B3030` | **UNREAD** (Weight0/V0 first-seen locked) |
| `006B8640` / `008889C0` leftover | **UNREAD** (do not write V0 first-seen) |
| `00435530` overlay `00435000` / interface `00435070` bodies | **PARTIAL** (Present + empty dest **PROVEN**) |
| PALSKIN type1/Flag1 (`0x80` / `0x200`) | **UNREAD** leftover; geometry still `0x100` |
| PALSKIN c38 dest upload | later GPU; do not file |
| Who writes persist `PlayerRegionName` | **UNREAD** (do not invent Oakvale on no-save) |

### Shared Dx9Vulkan holes (both families)

| Left | 2D | 3D |
|---|---|---|
| Half-pixel | dest write **UNREAD**; glyphs −0.5 **PROVEN** | none invented |
| D3DSAMP MAG/MIN/MIP/ADDRESS | **TEMPORARY** LINEAR/REPEAT | same |
| sRGB | UNORM **UNREAD** | same |
| ALPHATESTENABLE | no discard | no discard |
| Stencil / scissor | off / none | off |
| Sky PS `c0/c1/c2` | n/a | **UNREAD** |
| Sprite RHW store / SetFVF `0x144` | **UNREAD** | n/a |
| `+16020` first-seen fill | empty skip **PROVEN** | n/a |

---

## Classification table

| Claim | Status |
|---|---|
| Frontend 2D batch and world 3D are different Dx9Vulkan tables | **PROVEN** |
| Host implements both pipelines (2D `DrawFrontend`, 3D `DrawMeshBatches`) | **PROVEN** |
| They share one Present / one WVP | **DISPROVEN** |
| First nonempty 3D DIP is after Leave | **PROVEN** |
| First frontend frame already draws land/C3D | **DISPROVEN** |
| `009DA9F0` is the 3D walker | **DISPROVEN** |
| Concat land+C3D+sky is native | **DISPROVEN** |
| Game `00B27D90` after Leave | **UNREAD** |
| Lookout STB re-open | **UNREAD** |
| Type `0x18` packer | **UNREAD** |
| Consumed first-Present helper | **UNREAD** |
| Half-pixel / sampler / sRGB / alphatest / stencil / sky PS c | **UNREAD** (do not invent) |

Dumps / tests: `Dx9VulkanFrontendTests`, `Dx9VulkanParityTests`,
`EngineLifecycleTests` (`Frontend_0042EC7C_*`,
`After_004AEA70_eq_1_*`, `Install_banks_*`),
`landscape-trace/`, `A-dx9-submit.md`, listings `0042DF9E` /
`00435530` / `00B27D90` / `00BAE2D0` / `00BB2540`.
