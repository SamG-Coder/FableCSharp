# G — DX9 → Vulkan translation

Investigation only. No production source was modified.
`EngineLifecycle.cs` was not edited.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** / **EQUIVALENT**.

Read first: [A-dx9-submit.md](A-dx9-submit.md), [B-camera-matrices.md](B-camera-matrices.md),
[C-terrain-static-map.md](C-terrain-static-map.md), [D-c3d-transforms.md](D-c3d-transforms.md),
[E-player-palskin.md](E-player-palskin.md), [H-regression-audit.md](H-regression-audit.md).

Audited:

- `src/Fable.Render/Parity/Dx9Vulkan/*`
- `src/Fable.Render/VulkanLineRenderer.cs`
- `src/Fable.Render/VulkanLineRenderer.Textures.cs`
- `src/Fable.Render/WorldShadingPush.cs` (`MeshPushConstants`)
- `src/Fable.Render/MeshBatches.cs` / `MeshVertex.cs` / `LineShaders.cs`
- `src/Fable.Client/SilkEngineHost.cs` `Present` / `Draw`
- `src/Fable.Formats/Levels/LandscapeFrustum.cs`
- `tests/Fable.Formats.Tests/Dx9VulkanParityTests.cs`

No visual tuning. A formula that matches the exe is not a screenshot fix.

---

## Verdict (read this first)

The `Dx9Vulkan*` types are a **first-seen opcode catalog**. Several
rows are **PROVEN** or **EQUIVALENT** (clip-Y flip only at submit,
`D3DCULL_CCW` → `FrontFace.CCW + Cull Back`, `D3DCMP_LESSEQUAL`,
`00988A50` `W*V*P` → push `mat4`, landscape identity W on world STB,
strip odd-t unwind, PALSKIN `SRCALPHA/INVSRCALPHA`, fog `oFog` math
with black `FOGCOLOR`).

The **live Present payload is not a DX9 translation**. Native submits
per landscape cell (`00BF4570` DIP), per static instance
(`00BB2540` `DrawPrimitive`), per PALSKIN primitive (`00BD3070`
`DrawIndexed`), then sky (`00B662F0` bit `0x2000`, different P).
Host `SubmitCurrentWorld` does:

```
TessellateVisible → MeshBatches.Build(land)
BuildMeshes(primary C3Ds + hero 4299 CPU flatten)
Concat → one TexturedMesh
SilkEngineHost.Present → SetMesh + SetTextures
VulkanLineRenderer.Draw → one VB, CmdDraw ranges
```

That Concat / one-`TexturedMesh` / one-`SetMesh` is **DISPROVEN** as
native-equivalent (A, C, D, E, H). Grouping by
`(layer, tex, tex1, blend)` plus `ScenePasses.Rank` is a **VALID
BACKEND TRANSLATION** of *rank order* only. It is **not**
semantics-neutral batching of objects native submits separately.

Do **not** treat one giant VB as proven. Preserve native logical
submission boundaries (patch → cell; instance 3×4 + local VB;
PALSKIN dest `c38` + helper record) unless a later dump proves a
software flatten. None did.

---

## Mapping table

| Topic | Native DX9 (first-seen 3D) | Vulkan / host live | Class |
|---|---|---|---|
| **Vertex buffers** | Landscape cell `+56` stride **24** (`00BFE050`: u16 X/Y, f32 Z, float3 n, D3DCOLOR +20). Static-lit FVF `0x112` stride **32**. PALSKIN file stride (kid 28 / flags `0x14`; adult 4299 is 36 / `0x16`). File-local verts; no CPU world multiply (D). | One `MeshVertex[]` stride **60** (pos3 n3 uv2 color4 extra3). `SetMesh` host-visible VB. Decoded semantics, not native layout. | **EQUIVALENT** decoded streams (pos/n/uv/extra). **DISPROVEN** as native VB object / stride / FVF. |
| **Index buffers** | Landscape cell `+52` strip, `IndexCount = PrimitiveCount+2`. PALSKIN `DrawIndexed` vtbl+332. Static-lit `DrawPrimitive` vtbl+400 (no IB). | **No IB.** `CmdDraw` non-indexed after CPU unwind. | **DISPROVEN** as structure. **EQUIVALENT** faces *if* `UnwindStripTriangle` matches (locked). |
| **Primitive topology** | Landscape: D3D strip. C3D: strip (odd-t swap) or list. 2D `009DA9F0`: prim **2** list or **4** strip. | `Dx9VulkanPrimitive.World = TriangleList`. Gizmo `LineList`. | **EQUIVALENT** unwound faces. **DISPROVEN** as submitting native strips. |
| **World / view / proj** | Wrapper W `009881F0` (instance 3×4 or bind I or land `T(cam)`). V cot-scaled camera+128. P `009883F0` / camera+372 (`M11=M22=1`, near 0.1 far 4000 minZ 0.1 maxZ 0.99). `00988A50` `W*V*P` → `SetVSConstantF(c5,4)`. No first-seen `SetTransform`. | `LandscapeFrustum.ComposeWvp`. Host land W = I (`HostWorldSpaceLandscapeWorld`). `Dx9VulkanProjection.ToVulkanWvp` = `* diag(1,-1,1,1)` **only in `VulkanLineRenderer.Draw`**. Push `MeshPushConstants.ViewProj`. | **EQUIVALENT** algebra + Y-flip site. **DISPROVEN** baking Y into `009883F0`. **DIVERGE** (B) on the pose/FOV that enter V (ScriptedCamera ctor axes + 72° leftover). |
| **Viewport** | `009BEF80` vtbl+188: x/y=0, 1024×768, **MinZ 0 MaxZ 1**. P already bakes 0.1/0.99. | `Dx9VulkanViewport.FromFramebuffer(swapchain)`: x/y=0, `minDepth=0` `maxDepth=1`. Pixel size = window extent. | **EQUIVALENT** Z range (do not rescale 0.1/0.99 twice). **PARTIAL** pixel rect (native 1024×768 vs swapchain). |
| **Scissor** | First-seen `SetScissorRect` **UNREAD**. | Full `_extent` `CmdSetScissor`. | **UNREAD** native. Host full-frame is a stand-in, not proven. |
| **Depth** | `D3DRS_ZFUNC = LESSEQUAL` (4). `ZENABLE`/`ZWRITE` first-seen **write UNREAD**; D3D defaults TRUE. | `Dx9VulkanDepth.FirstSeenOpaque`: test=1 write=1 `LessOrEqual`. Depth image `D32Sfloat`. | **PROVEN** compare-op. **PARTIAL** enable/write (defaults, not a recovered RS site). |
| **Culling** | `D3DCULL_CCW` (3) from `0x01396FB0`. Landscape `00B24850` / static `00BB2540`. PALSKIN inherits (no CULLMODE write). `0x01396FB8=NONE` is other passes, then restore. | `FrontFace.CounterClockwise` + `CullMode.Back`. After clip-Y flip a clip-CCW face is framebuffer-CCW. | **PROVEN** / **EQUIVALENT**. |
| **Winding** | Strip even `(a,b,c)`, odd `(b,a,c)`. No rewind on `n.Z<0`. | `Dx9VulkanPrimitive.UnwindStripTriangle`. Same swap. | **PROVEN** / **EQUIVALENT**. |
| **Alpha test** | `D3DRS_ALPHATESTENABLE` slot exists. First-seen landscape / static-lit write **UNREAD**. | `FirstSeenAlphaTest=false`. Mesh FS has **no `discard`**. | **UNREAD**. Host “no test” is **TEMPORARY**. |
| **Alpha blend** | Landscape **off**. PALSKIN `00BD3867/00BD38D4` SRCALPHA(5) / INVSRCALPHA(6), enable 1, no Flag1 test. `BLENDOP` write **UNREAD** (D3D default ADD). | Opaque pipeline + `_meshAlphaPipeline`. `SrcAlpha` / `OneMinusSrcAlpha` / `Add`. Switch on `MeshDraw.SrcAlphaBlend`. | **PROVEN** PALSKIN factors + landscape off. **PARTIAL** `BLENDOP` and alpha-channel factors (host copies color factors). |
| **Fog** | `FOGENABLE=1` (`00B46890`). `FOGCOLOR 0xFF000000`. table/vertex mode 0. VS `mad oFog, min(dp4(pos,c2),c0.y), -c18.w, c0.y`. `c2` = linear view-Z plane start 1000 / end 2000. D3D interpolator saturates. Blend `rgb*oFog + (1-oFog)*black`. | VS: same `clamp(min(dp,1)*(-1)+1)`. `pc.cameraPos` is the fog **plane** (misnamed). FS: `lit * fragFog`. | **EQUIVALENT** first-seen formula + black color. Field name is host-only. |
| **Texture stages** | FG: stage 0 `00BF50E0` (t0 mask = `TextureId1`), stage 1 `00BF5491` (t1 albedo = `TextureId`). `PSHADER_LANDSCAPE_FOREGROUND` `mul_x2 t1*v0`, `t0.a*v0.a`. BG: `mul_x2 t0*v0`. Static: **1** stage, `PSHADER_TEXTURE_DIFFUSE` t0 only. Bump stored, **not** bound first-seen. | Two descriptor sets. FG: set0=mask set1=albedo. Else set0=albedo set1=mask. Mode 0/1/2/3 picks PS contract. | **EQUIVALENT** first-seen FG/BG/static RGB contracts. **PARTIAL**: host always binds two sets; native static is one stage. |
| **Samplers** | First-seen `SetSamplerState` MAG/MIN/MIP/ADDRESS **UNREAD**. D3D defaults POINT / NONE / WRAP. | `Dx9VulkanSamplerState.FirstSeenTemporary`: LINEAR / LINEAR / REPEAT / `MaxLod=1` (top mip only). One sampler for every texture. | **UNREAD** native. Host is **TEMPORARY**. |
| **Fixed-function lighting** | No first-seen `SetLight` / FFP `D3DLIGHT`. VS: `dp3 n,-c19`; `max(.,c0.x)`; square; `*c20`; `mad c35`; `add c3`. First-seen `c19=(0,1,0,0)`, `c20=(0.25)×3`, `c35=(0,0,0,1)`, leftover `c3=(0,0.125,0)`. Point-light pack exists; first-seen take **UNREAD**. | FS replica of that formula. Push `LightDir`/`LightColor`/`Pass.yzw` (=c35 rgb). `c3` hardcoded in GLSL. | **EQUIVALENT** first-seen dirlight RGB. **PARTIAL**: native lights in VS (`oD0` interpolated); host lights in FS (interpolated n). Point lights **UNREAD**. |
| **Shaders / constants** | Separate `VSHADER_*` / `PSHADER_*`. WVP `c5–c8`. Fog `c2`/`c18`/`c0`. Lights `c19`/`c20`/`c35`/`c3`. PALSKIN dest `SetVSConstantF(c38, n*3)`. Static PS `c0=(1,1,1,1)`. Sky PS `c0/c1/c2` **UNREAD**. | One GLSL mesh pair + `pass.x` mode (0 BG / 1 FG / 2 sky / 3 static). Push 128-byte `MeshPushConstants`. **No `c38`**. Row-major Numerics memcpy; GLSL column-major reads as needed transpose. | **EQUIVALENT** upload convention + first-seen mode contracts. **DISPROVEN** as PALSKIN (`c38` / `VSHADER_PALSKIN_*`). Sky PS **UNREAD**. |
| **Render target** | Device backbuffer, PE default **1024×768**. Depth format **UNREAD**. Present `009BEEB0`. | Swapchain prefer `B8G8R8A8Unorm` + `SpaceSrgbNonlinearKhr`. Depth `D32Sfloat` (no stencil). 3D `Mailbox` when available; AVI `FIFO`. | **EQUIVALENT** role (one color + one depth, Present). **PARTIAL** size / format / present interval / sRGB space on the view. |
| **Clear** | `009D8CF0` after `009BEF20` BeginScene (A). Color used as first-seen fog ARGB `0xFF000000`. Flags (TARGET / Z / STENCIL) **UNREAD**. | `LoadOp.Clear` color `Dx9VulkanColor.FirstSeenClear=(0,0,0,1)`, depth **1**. Stencil `DontCare`. | **EQUIVALENT** black color. **PARTIAL** flags / stencil. |
| **Draw ordering** | `00B25950` layers: `0x4` → `0x40` → `0x20` → `0x2000` (registration). Landscape vtbl+16 only draws 4 and `0x40`. PALSKIN also drains `0x80`/`0x100` (E). 2D `009DA9F0` after 3D. Water `0x20000` first-seen empty. | `MeshBatches.Build` sorts `ScenePasses.Rank` then tex. `Concat(land, props)` **does not re-sort**. Live Concat has land `0x4`/`0x40` then props `0x20`. **No `0x2000`**. PALSKIN flattened into `0x20`. | Rank `0x4<0x40<0x20<0x2000` **PROVEN**. Live Concat order **PARTIAL** (accidentally OK for land-then-props). Sky missing **DISPROVEN** vs native walk. PALSKIN-in-`0x20` **DISPROVEN** vs `0x80`/`0x100`. |
| **Submission boundaries** | Per-cell DIP; per-instance local VB + wrapper W; per-prim PALSKIN + `c38`. Not one soup. | One `TexturedMesh` Concat. `CmdDraw` per `(layer,tex,tex1,blend)` range. | **DISPROVEN**. Not semantics-neutral. |

---

## 1. What the parity layer actually is

`Fable.Render.Parity.Dx9Vulkan` does **not** issue DX9 calls. Each
type documents one first-seen opcode and the Vulkan field the
renderer already consumes.

| Type | Native fact it encodes | Used by |
|---|---|---|
| `Dx9VulkanProjection` | `009883F0` Y=+1; Vulkan NDC Y-down is `* diag(1,-1,1,1)` | `VulkanLineRenderer.Draw` |
| `Dx9VulkanShaderConstants` | `00988A50` `W*V*P` → c5–c8; land identity W | `PackWvp` / tests; Draw uses `ToVulkanWvp` directly |
| `Dx9VulkanViewport` | P bakes minZ/maxZ; viewport Z stays 0..1; half-pixel UNREAD | pipeline + `CmdSetViewport` |
| `Dx9VulkanRasterState` | `D3DCULL_CCW` after Y flip → CCW+Back; FILL UNREAD | mesh pipeline |
| `Dx9VulkanDepth` | `LESSEQUAL`; Z enable/write UNREAD defaults | mesh pipeline |
| `Dx9VulkanBlendState` | PALSKIN 5/6; landscape off; alpha test UNREAD | opaque + alpha pipelines |
| `Dx9VulkanPrimitive` | strip odd-t swap; world = triangle list | mesh input assembly |
| `Dx9VulkanSamplerState` | TEMPORARY LINEAR/REPEAT/MaxLod=1 | `CreateSamplerAndLayout` |
| `Dx9VulkanTextureFormat` | DXT FourCC SCRATCH → host RGBA8 UNORM mip0; sRGB UNREAD | `UploadTexture` `R8G8B8A8Unorm` |
| `Dx9VulkanVertexFormat` | decoded `MeshVertex`, not FVF 0x112 / stride 24/28/32 | attribute formats |
| `Dx9VulkanColor` | fog/clear ARGB `0xFF000000`; landscape extra BGR | render-pass clear |

`Dx9VulkanParityTests` locks **those translations + Formats math**,
not Concat, not one VB, not “Vulkan looks like SHOT2”. Goldens are
exe/asset numbers (UV decompress, strip swap, LESSEQUAL, CCW, c5–c8,
c38 layout, kid 4300 bind palettes, grass DXT1 512). The SHOT2 house
clip test is the Oakvale-intro contract (B), not first no-save
Lookout Present.

---

## 2. Vertex / index / topology

### Native

- Landscape GPU expand `00BFE050`: **24-byte** VB, IB strip
  (`C`, `LevTileMesh`). DIP inside `00BF4570` is **PARTIAL** (A).
- Static-lit `00BB2540`: dynamic VB FVF `0x112` stride 32, **copy
  local verts with no matrix**, `DrawPrimitive` vtbl+400 (D).
- PALSKIN: file VB + IB, `DrawIndexed` after `00BD3070` (E).
- C3D / STB strips: even `t → (a,b,c)`, odd `t → (b,a,c)` (C).

### Host

```
MeshBatches.Build / BuildMeshes
  → MeshVertex[triCount*3]   // unwound list
  → MeshDraw ranges
VulkanLineRenderer.SetMesh
  → one VkBuffer, host visible
DrawMeshBatches
  → CmdDraw(vertexCount, 1, firstVertex, 0)
```

No `CmdDrawIndexed`. `Dx9VulkanVertexFormat.HostStride = 60`.

**Class:** decoded pos/normal/UV/extra **EQUIVALENT**. Native VB/IB
objects **DISPROVEN**. Do not invent an IB “for parity” by re-indexing
the Concat soup — restore **per-cell / per-prim** buffers.

---

## 3. World / view / projection

B owns the pose. This row is only the **builder + Vulkan site**.

```
Native:  clip = p_local * W * V * P     // 00988A50 → c5–c8
         W_obj  = instance 3×4 (mesh) or I (bind default)
         W_land = T(cam) on camera-relative VB
         P      = 009883F0  (host stores camera+372 form, M22=+1)
Host:    clip_dx9 = p_world * I * V * P
         clip_vk  = clip_dx9 * diag(1,-1,1,1)   // Draw only
```

`LandscapeFrustum.HostTcamOnWorldSpaceLandscapeIsDisproven`.
`SilkEngineHost.Draw` passes `HostLandscapeViewProjection` (identity
W). `DrawMeshBatches` then picks:

| `PassBit` | Push WVP |
|---|---|
| `0x4` / `0x40` | `_landscapeViewProj` |
| `0x2000` | `_skyViewProj` (sky P 100/10000/0.99/1) |
| else (`0x20`) | `_worldViewProj` |

On the live path land and object matrices are the **same** I*V*P
(B §5). Sky P split is **PROVEN** but live Concat has **no** `0x2000`.

`WorldShadingPush` / `MeshPushConstants` (128 bytes):

| Field | Native stand-in |
|---|---|
| `ViewProj` | `c5–c8` after Y flip |
| `CameraPos` | **fog plane `c2`**, not eye |
| `LightDir` | `c19` |
| `LightColor` | `c20` |
| `Pass.x` | host mode 0/1/2/3 |
| `Pass.yzw` | `c35.rgb` (`LitColor`) |

GLSL `gl_Position = pc.viewProj * vec4(pos,1)` with row-major
upload is the locked transpose (**EQUIVALENT**,
`CameraProjectionTests.Gpu_upload_keeps_row_major_bytes`).

Baking the Y flip into `FirstSeenDx9Projection` is **DISPROVEN**.

**Aspect (update vs B):** `SilkEngineHost.Draw` now **discards**
window aspect and letterboxes at native 1024/768:

```154:163:src/Fable.Client/SilkEngineHost.cs
        var nativeAspect = EngineLifecycle.DisplayDefaultWidth
            / (float)EngineLifecycle.DisplayDefaultHeight;
        _ = aspect;
        var fogPlane = WorldShading.LinearFogPlane(cam.Position, cam.Forward);
        Renderer.Draw(
            cam.ViewProjection(nativeAspect),
            ...
            cam.HostLandscapeViewProjection(nativeAspect));
```

WVP letterbox is **EQUIVALENT** to native 4:3. The **viewport pixel
rect** is still the swapchain. A 16:9 window stretches 4:3 clip into
a wide framebuffer — not native (native stays 1024×768). **PARTIAL**.

The numbers **inside** V (eye / look / FOV) remain B’s **DIVERGE**.
Do not flip axes to compensate.

---

## 4. Viewport / scissor / render target / clear / Present

```
Native 00435530:
  00A0BF20 / 00A0B560 viewport-ish
  009BEF20 BeginScene
  009D8CF0 Clear
  ... layer draws ...
  009BEF50 EndScene
  009BEEB0 Present

Vulkan Draw:
  AcquireNextImage
  Record: video copy; BeginRenderPass Clear; viewport+scissor;
          mesh batches; gizmos; fade; video blit
  QueueSubmit + QueuePresent
```

| Item | Native | Vulkan | Class |
|---|---|---|---|
| Color RT | backbuffer 1024×768 | swapchain `B8G8R8A8Unorm` (prefer) | **PARTIAL** |
| Depth RT | format UNREAD | `D32Sfloat`, no stencil | **PARTIAL** / stencil **UNREAD** |
| Viewport Z | 0..1 (`009BEF80`) | 0..1 | **EQUIVALENT** |
| Viewport XYWH | 1024×768 | framebuffer extent | **PARTIAL** |
| Half-pixel | UNREAD | `AppliesHalfPixelOffset=false` | **UNREAD** |
| Scissor | UNREAD | full extent | **UNREAD** |
| Clear color | first-seen fog black | `(0,0,0,1)` | **EQUIVALENT** color |
| Clear depth | UNREAD (D3D 1.0 typical) | 1.0 | **PARTIAL** |
| Present | `009BEEB0` | `QueuePresent` | **EQUIVALENT** role |
| Interval | 3D UNREAD; AVI vsync-shaped | 3D Mailbox / AVI FIFO | **PARTIAL** |

`SilkEngineHost.Present` is the `009BEEB0` adapter (H: keep). It
does not BeginScene/Clear; `Draw` does. Native Clear is inside
BeginScene…EndScene. Host Clear is render-pass load. **EQUIVALENT**
as “once per Present, black + Z=1 before 3D”.

AVI-only path (`_playAviPump`) skips mesh/fade and still Clears —
native `006286F0` blit path also does not draw landscape (renderer
comment). **EQUIVALENT** skip.

---

## 5. Depth / cull / winding / fill

Locked by `Dx9VulkanParityTests.Fresh_consumer_shot2_house_vertex_clip_ndc_and_rs_maps`
and `Winding_preserve_and_depth_compare_equivalence`.

- Compare: `CompareOp(4) = LessOrEqual`. **PROVEN**.
- Test/write: host TRUE/TRUE. Native RS site **UNREAD**. **PARTIAL**.
- Front face: CCW after Y flip. **PROVEN**.
- Cull: Back for `D3DCULL_CCW`; Front for CW; None for NONE. **PROVEN**.
- Fill: `PolygonMode.Fill`. Native `FILLMODE` **UNREAD**. **TEMPORARY**.
- Color write: host RGBA. Native `COLORWRITEENABLE` **UNREAD**.
- Stencil: host unused. Native **UNREAD**.

Line / overlay / video pipelines force `CullMode.None` and no depth.
Those are debug / 2D / AVI, not first-seen 3D.

---

## 6. Alpha test / alpha blend

```
Landscape 0x4 / 0x40: alphablend OFF          PROVEN (C)
Static 0x20 first-seen: OFF                   PROVEN (ScenePasses contract)
PALSKIN: SRCALPHA / INVSRCALPHA, enable 1     PROVEN (00BD3867 / 00BD38D4)
BLENDOP: UNREAD, D3D ADD                      TEMPORARY
ALPHATEST / ALPHAREF / ALPHAFUNC: UNREAD      TEMPORARY (no discard)
```

Host has two mesh pipelines. `DrawMeshBatches` rebinds when
`SrcAlphaBlend` flips and re-pushes constants. `MeshFile` sets
`SrcAlphaBlend: hasBones` — **every** PALSKIN triangle, including
hero 4299, takes the alpha pipeline. That matches
`FirstSeenPalskinSrcAlphaBlend` and **does not** match native
per-prim Flag1/opacity (E: first-seen opacity `0xFF` skips a
block). Adult 4299 19 prims become one soup with blend on.
**PARTIAL** (factors yes; granularity no).

---

## 7. Fog

First-seen:

- `FOGENABLE=1`, `FOGCOLOR=0xFF000000`, table=0, vertex=0.
- VS writes `oFog`; D3D still blends toward FOGCOLOR.
- `c2 = LinearFogPlane(pos, look)` from unscaled view +276,
  start 1000 / end 2000. Inverse-row `c2` is **DISPROVEN**
  (`FirstSeenUploadsInverseRow0AsC2=false`).
- `oFog = min(world·c2, 1) * (-1) + 1`, then saturate.

Host mesh VS is that formula with `c0.y=1` `c18.w=1` hardcoded.
FS `outColor.rgb = lit * fragFog`. **EQUIVALENT**.

`SilkEngineHost.Draw` builds the plane from `cam.Position` /
`cam.Forward`. Those vectors are B’s **DIVERGE** (ctor axes), so
the *plane numbers* on first Lookout Present are not native even
though the *opcode* is.

---

## 8. Texture stages / samplers / formats

### Stages

Native FG (C): primary albedo on **t1**, mask on **t0**.
`LandscapeTextures.ProjectOt1` = `(0,0)` because `c40=c41=0`.
BG `oT0 = ExtraRgb.XY`. Static `oT0 = v2` (mesh UV), one stage.

Host FS:

```
ot0 = mode0 ? extra.xy : mode1 ? extra.yz : fragUv
ot1 = fragUv
t0 = texture(set0, ot0)
t1 = texture(set1, ot1)
```

FG bind swaps so set0=mask set1=albedo. Mode 3 uses `t0` at mesh UV
=`PSHADER_TEXTURE_DIFFUSE`. **EQUIVALENT** first-seen contracts.

`FirstSeenBindsC3dBump=false` — host also drops bump. **EQUIVALENT**
omit.

### Samplers

**UNREAD.** Host LINEAR / REPEAT / `MaxLod=1` is labelled
TEMPORARY in both the type and `CreateSamplerAndLayout`. D3D
default POINT would look different; do not “fix” by changing
filters without a `SetSamplerState` dump.

### Format

`009BE8B0` CreateTexture DXT FourCC, `D3DPOOL_SCRATCH`. Host
`Dxt.Decode` top mip → `R8G8B8A8Unorm`. Lower mips stay in the
file (`FirstSeenTextureStoresRawLowerMips`) and are **not**
uploaded. Sampled-view sRGB vs linear **UNREAD**.
`TreatAsSrgb=false`. **PARTIAL**.

---

## 9. Fixed-function lighting vs VS lighting

First-seen 3D is **not** FFP. `SetTransform` / `SetLight` absent
on the `00988A50` path (B). Lighting is VS `oD0`.

Host evaluates the same polynomial in the **fragment** shader
from interpolated normals. Per-vertex `oD0` vs per-pixel n is
**PARTIAL** (same formula, different interpolant). Unlit RGB is
leftover `c3=(0,0.125,0)` then `mul_x2` → dark green, **not** a
missing ambient (`UnlitRgbIsC3Leftover`). Do not invent ambient.

Point / extra lights (`00B44F20` / `00B46280`) first-seen take
**UNREAD**. Host has no point-light push.

`fragColor.rgb` multiplies the lit term. `MeshBatches.Vert`
defaults tint to `(1,1,1)` when Color is zero. Static FVF has no
diffuse. **EQUIVALENT** first-seen (multiply by 1).

---

## 10. Shaders / constants / PALSKIN

One GLSL program is a **VALID BACKEND TRANSLATION** of four
first-seen families **only** as RGB contracts:

| Mode | Native |
|---|---|
| 0 | `PSHADER_LANDSCAPE_BACKGROUND` `mul_x2 t0*v0` |
| 1 | `PSHADER_LANDSCAPE_FOREGROUND` `mul_x2 t1*v0` |
| 2 | inner sky; PS c0/c1/c2 **UNREAD** (host does not invent 0) |
| 3 | `PSHADER_TEXTURE_DIFFUSE` `mul_x2 t0*(v0*c0)` `c0=(1,1,1,1)` |

WVP / fog / dirlight constants: **EQUIVALENT** packing.

PALSKIN: native `00BCFB00` `c38`, `VSHADER_PALSKIN_DIRLIGHT_FOG`,
`a0`-relative. Host `BuildMeshes` → `TrianglesForPose()` bind
locals → world bake → mode 3 static PS. **DISPROVEN** as character
draw (E). `Dx9VulkanShaderConstants.PaletteStartRegister=38` is a
**constant lock**, not an upload. `FirstSeenPaletteIsBindPose=true`
is the dest math, not the DIP.

Do not add a Vulkan PALSKIN hack on the Concat soup (E).

---

## 11. Draw ordering vs Concat

`ScenePasses.Registration` rank is the `00B26A75` layer list.
`MeshBatches.Build` sorts draws by `Rank` then texture.

`MeshBatches.Concat` appends `b` verts and shifts `FirstVertex`.
**It does not merge-sort Rank.** Today land draws are only
`0x4`/`0x40` and props only `0x20`, so the concatenation happens
to be rank order. That is **accidental**, not a proof.

Live `SubmitCurrentWorld` (read, not edited):

```
land  = Build(TessellateVisible)     // 0x4 and 0x40 from same tris
props = BuildMeshes(primary + hero)  // 0x20, PALSKIN blend on
SubmittedMesh = Concat(land, props)
```

Missing vs native:

| Native submit | Live host |
|---|---|
| BG walk `00BDC060` / `00BF71D0` (bit 4) | **Same** CPU tiles drawn twice, mode 0 then 1 |
| FG per-cell `00BF4570` (bit 0x40) | Dump-all tiles after patch AABB (C **DIVERGE**) |
| Static `00BB2540` per instance (0x20) | Flattened into Concat |
| PALSKIN `00BD3070` per prim (`0x80`/`0x100`) | Flattened into 0x20 |
| Sky `00B662F0` (0x2000) | **Absent** (H) |
| Water 0x20000 | Empty first-seen — **EQUIVALENT** omit |
| 2D `009DA9F0` | Not this VB (fade/video are separate pipelines) |

`DrawnPasses(Landscape)` emitting **both** 0x4 and 0x40 from one
triangle list is a host convenience. Native BG is a different
walk. **PARTIAL**.

H: `WorldSubmitted` is one-shot; frustum frozen. Not a Vulkan
translation; it starves later native redraws.

`SilkEngineHost.Present` now `ReferenceEquals`-skips mesh/texture
when `BuildFrame` reuses `SubmittedMesh.Vertices` /
`_submittedTextureArray`. H’s “re-upload every Present” is
**stale for the skip**. First submit still `SetTextures`
(`DeviceWaitIdle` + destroy + re-upload every id) + `SetMesh`
full copy. **LIKELY PERF**, not a semantic map.

---

## 12. Submission boundaries (do not collapse)

Native logical units that Vulkan must keep **separate** unless a
dump proves otherwise:

1. **Landscape patch** — `00BDC2D0` AABB, then **cells**, DIP from
   stored VB/IB. Not `TessellateVisible` soup.
2. **Static C3D instance** — local verts + instance 3×4 on
   wrapper+496. Not `Vector3.Transform` into the land VB (D).
3. **PALSKIN primitive** — dest `c38` + helper + family shader.
   Not `TrianglesForPose` + Concat (E).
4. **Sky** — same V, sky P, bit `0x2000`, after 0x20.
5. **2D / fade / AVI** — already separate host pipelines. Keep.

`MeshBatches.Build` group-by texture is allowed **inside** one
native unit (one cell already has one stage-0/1 bind) as a
backend of *that* DIP. Concatenating land+every prop is
**not** that.

A later split should flush **N draw lists** in `ScenePasses`
order — not restore `WorldGeometry.Triangles` as the Present
payload (H).

---

## 13. Classification index (parity types + live path)

| Claim | Status |
|---|---|
| Clip-Y flip only at `ToVulkanWvp` / `Draw` | **PROVEN** / **EQUIVALENT** |
| `00988A50` W*V*P → push mat4, no extra `Transpose()` | **PROVEN** / **EQUIVALENT** |
| Host land W = I on world STB | **EQUIVALENT** (native `T(cam)` on cam-rel VB) |
| `T(cam)` on host STB | **DISPROVEN** |
| Viewport depth 0..1 (P holds 0.1/0.99) | **PROVEN** / **EQUIVALENT** |
| `D3DCULL_CCW` → CCW+Back after Y flip | **PROVEN** / **EQUIVALENT** |
| Strip odd-t unwind | **PROVEN** / **EQUIVALENT** |
| `ZFUNC` LESSEQUAL | **PROVEN** |
| `ZENABLE`/`ZWRITE` first-seen write | **UNREAD** (host defaults TRUE) |
| Landscape alphablend off | **PROVEN** |
| PALSKIN SRCALPHA/INVSRCALPHA | **PROVEN** |
| `BLENDOP` / alpha-test / fill / color-write / stencil / scissor / half-pixel | **UNREAD** |
| Fog enable + black + VS oFog math | **PROVEN** / **EQUIVALENT** |
| FG t1 albedo / t0 mask; `oT1=(0,0)` | **PROVEN** / **EQUIVALENT** |
| Sampler MAG/MIN/MIP/ADDRESS | **UNREAD** (TEMPORARY LINEAR/REPEAT) |
| DXT top-mip decode RGBA8 | **PROVEN** asset; sampled format **UNREAD** |
| Dirlight `n·-c19` square `*c20` + c3 | **PROVEN** / **EQUIVALENT** formula; FS vs VS **PARTIAL** |
| Sky PS constants | **UNREAD** |
| PALSKIN `c38` uploaded to Vulkan | **DISPROVEN** (not on live path) |
| One Concat `TexturedMesh` / one `SetMesh` | **DISPROVEN** as native |
| `MeshBatches.Build` rank sort | **VALID BACKEND TRANSLATION** of layer order |
| Concat without re-sort | **PARTIAL** (ok only while land/prop bits stay disjoint) |
| Live path emits sky `0x2000` | **DISPROVEN** |
| Clear color = fog black | **EQUIVALENT** (flags **PARTIAL**) |
| `SilkEngineHost` is `009BEEB0` | **PROVEN** role (H) |
| Draw letterbox uses 1024/768 | **EQUIVALENT** (current `Draw`; B’s window-aspect row is stale) |
| Viewport pixels = swapchain | **PARTIAL** vs 1024×768 |
| Camera pose/FOV in that WVP | **DIVERGE** (B) — not a Vulkan map |

---

## 14. What not to do

- Do not visually tune (LOD, extra ambient, invented UV scale,
  `CreateLookAt`, baked Y in P, `SeedAt(1.6m)`).
- Do not treat Concat / one VB as proven because
  `Dx9VulkanParityTests` or `SubmittedMesh.Vertices.Length > 128`
  pass (H: those lock the **bridge**).
- Do not implement PALSKIN as a Vulkan special case on the soup (E).
- Do not apply `T(cam)` to host landscape verts (B, C).
- Do not edit `EngineLifecycle.cs` from this investigation.
- Do not change LINEAR/REPEAT / no-alpha-test / Z write-on without
  a recovered `SetRenderState` / `SetSamplerState` site.

---

## Evidence index

Parity types: `src/Fable.Render/Parity/Dx9Vulkan/*.cs`.

Live consume: `VulkanLineRenderer.Draw` (`ToVulkanWvp`),
`CreatePipeline` (depth/raster/blend/topology),
`VulkanLineRenderer.Textures.DrawMeshBatches` / `SetTextures`,
`WorldShadingPush.cs` (`MeshPushConstants`),
`SilkEngineHost.Present` / `Draw`,
`MeshBatches.Build` / `BuildMeshes` / `Concat`,
`LineShaders.MeshVertex` / `MeshFragment`,
`LandscapeFrustum` WVP / fog / AABB helpers.

Tests: `Dx9VulkanParityTests` (translations only).

Peer investigations: A (submit graph), B (matrices), C (cells),
D (instance W), E (PALSKIN), H (Concat bridge).
