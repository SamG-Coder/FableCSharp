# 07 — Frontend DX9 → Vulkan mapping

Replaces the CPU shortcut
`frontend.bin → FrontendRgba → SetVideoFrame (PlayAVI 009DC870)`.

Native path this table implements:

```
frontend.bin UI persist
  → FrontendUiDef dest (PositionX/Y × scale)
  → 0041BEB0 type 0x22 rec 0xC0 dest +0x15C
  → 00B23BC0 / 00B324A0
  → 00BACFD0 + 00BAE2D0  (first dest+4=0)
     or 00BAD8A0 dest copy then 00BAE2D0
  → 00A0AEA0 DrawIndexedPrimitiveUP
  → explicit DX9→Vulkan below
```

Agent 6 `FrontendDx9Submit.cs` / `06-dx9-submit.md` were not present.
Facts below are from `tools/Fable.ExeIndex` text-map listings, the
`VSHADER_2D_SPRITE` token dump, and already-proven wrapper constants
in `EngineLifecycle`.

Statuses: **PROVEN** / **EQUIVALENT** / **UNREAD** / **TEMPORARY**.

---

## Recovered VSHADER_2D_SPRITE

Bank `SHADERS_POINT_SPRITE1` in `data/shaders/pc/shaders.big`.
Looked up by `00BAD040` (`push "VSHADER_2D_SPRITE"`). Bound by
`00BAE2D0` `00988020(handler+12)`.

```
vs_1_1
mov oPos, v0
mul r0, v0, c92.xyyy
add oT1.xy, r0, c92.zwww
mov oT1.zw, c0.y
mov oT0, v2
mov oD0, v1
end
```

ctab name `LayoutLightsBones` (bank leftover). Inputs:

| VS reg | Semantic | Source |
|---|---|---|
| v0 | oPos passthrough | XYZRHW float4 |
| v1 | oD0 | D3DCOLOR DIFFUSE |
| v2 | oT0 | TEX1 float2 |
| c92 | oT1 only | not oPos |

`mov oPos, v0` means the device verts are already clip-space
(or treated as clip once a VS is bound). FVF
`D3DFVF_XYZRHW|DIFFUSE|TEX1` = `0x144` is 28 bytes; both
`00BAE2D0` and `009DA9F0` **push 32**. Extra 4 is stride pad.

## Recovered PSHADER_2D_TEXTURE_DIFFUSE

Bank `PIXEL_SHADERS`. `00BAE2D0` `push 2` is **SetTexture
stage count** (loop in `00A0AEA0`), matching `tex t0` / `tex t1`.

```
ps_1_1
tex t0
tex t1
mul r0, v0, c0
mul r0.w, t0, r0
mul r0.w, r0, t1.w
mul_x2 r0.xyz, t0, r0
end
```

c0 = `PSCONST_OUTPUT_FACTOR`. First-seen attach slot at
`[0x1436E78]+0x1AC/0x19C` is **PARTIAL** (mode 2 uses that family).
t1.w identity when stage 1 is unread: **TEMPORARY**.

---

## Mapping table

| Semantic | Fable DX9 | Vulkan | Status | Evidence |
|---|---|---|---|---|
| Record | type `0x22`, dest `+0x15C`, size `0xC0` | `FrontendDx9DrawRecord` | PROVEN | `0041BEB0`, `EngineLifecycle.Frontend2dRecordType` |
| Dest rect | rec+12 / `00BAD8A0` instance+72 four floats | dest pixels on the DX9 vert XY | PROVEN | listing `00BAD8A0` |
| Empty dest | `0,0,0,0` → no `009DB700`, no DIP | skip `AppendRecord` | PROVEN | first-seen `Frontend2dDipIssued=false` |
| VS | `VSHADER_2D_SPRITE` `mov oPos,v0` `oD0=v1` `oT0=v2` | pos/color/uv attributes | PROVEN | shaders.big dump |
| PS | `PSHADER_2D_TEXTURE_DIFFUSE` `2*t0*(v0*c0)`, alpha `t0*v0*t1.w` | sample * vertex color; `mul_x2` in FS when wired | PARTIAL | tokens PROVEN; attach slot PARTIAL |
| Vertex layout | stride **32**, RHW **1.0** at +12, DIFFUSE +16, UV +20 | `FrontendDx9Vertex` / `FrontendGpuVertex` | PROVEN / EQUIVALENT | `push 32`; `009DB810` `0x3F800000` at +12; VS v0/v1/v2 |
| FVF | `0x144` XYZRHW\|DIFFUSE\|TEX1 (28) + 4 pad | same semantics | EQUIVALENT | VS inputs; pad TEMPORARY |
| Topology | `00A0AEA0` `push 4` `D3DPT_TRIANGLELIST` | `VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST` | PROVEN | listing `00A0AF23` |
| Index | `push 101` `D3DFMT_INDEX16`; verts `esi+24`, IB `esi+248` | `ushort[]` list 0,1,2,1,3,2 | PROVEN format; EQUIVALENT quad | `00A0AF18` |
| Draw call | `DrawIndexedPrimitiveUP` vtbl **336** | `CmdDrawIndexed` | PROVEN | `00A0AF26` |
| HUD/PlayAVI other path | `009DB700` 60-byte `+16020`; `009DA9F0` `00A058C0` then vtbl **332** `DrawPrimitiveUP` prim **2** or **4** | not the type-0x22 path | PROVEN | listing `009DB612` / `009DB640` |
| Prim 2 vs 4 | D3D `LINELIST=2` `TRIANGLELIST=4` | `LineList` / `TriangleList` | PROVEN immediates | do **not** call 2 a triangle list |
| Viewport | `009BEF80` vtbl+188: 1024×768 MinZ **0** MaxZ **1** | same pixel rect, depth 0..1 | PROVEN | `EngineLifecycle.SetViewportFn` |
| Dest → clip | D3D viewport inverse so `oPos=v0` hits dest pixels | then `Y *= -1` for Vulkan NDC | EQUIVALENT | rasterizer + `Dx9VulkanProjection.NdcYSign`. **+24 filler UNREAD** |
| Half-pixel | `009DB810` subtracts `[0x122DED8]=1.0`, not 0.5. Frontend +24 UNREAD | none | UNREAD | do not invent −0.5 |
| Y flip (UV) | `009DC870` v=0 at dest **top** (video blit) | UV `(0,0)` dest top-left; **no** V invert | PROVEN UV origin | video `1-t.y` is RGB24 DIB only |
| Y flip (clip) | D3D clip Y-up | Vulkan NDC Y-down | EQUIVALENT | same world-path clip-Y, not a Fable 2D write |
| Blend default | widget +372=2 → `00BAE2D0` else: `[0x1396F78]=5` `[0x1396F7C]=6` | `SRC_ALPHA` / `ONE_MINUS_SRC_ALPHA` | PROVEN | dump `01396F78`; `+10424=1` |
| Blend mode 3 | SRC/DST `[0x1396F6C]=2` ONE | `ONE` / `ONE` | PROVEN | `00BAEA66 sub ecx,3; je` |
| Blend mode 4 | SRC ONE, DST `[0x1396F74]=4` INVSRCCOLOR | `ONE` / `ONE_MINUS_SRC_COLOR` | PROVEN | same switch |
| BLENDOP | unread | ADD | TEMPORARY | D3D default; `Dx9VulkanBlendState.FirstSeenBlendOp` |
| Z enable/write | `00BAE2D0` writes 0 at +10324/+10344 | test=0 write=0 | TEMPORARY | slot RS numbers PARTIAL |
| Cull | `009DA9F0` copies `0x1396FB0=3` CCW. `00BAE2D0` CULL write unread | CCW + Back after clip-Y | EQUIVALENT / PARTIAL | inherit display CCW |
| Scissor | unread | none | UNREAD | not invented |
| Alpha test | unread | no discard | UNREAD | not invented |
| Sampler | unread | LINEAR / REPEAT / MaxLod 1 | TEMPORARY | `Dx9VulkanSamplerState` |
| Texture | frontend.big DXT/RGBA via `009BE8xx` SCRATCH | decode top mip `R8G8B8A8_UNORM` | EQUIVALENT | `Dx9VulkanTextureFormat` |
| Color | bytes `* 0x1231724` (1/255); D3DCOLOR ARGB | `Dx9VulkanColor.FromD3dArgb` | PROVEN scale | `00BAEE03` |
| Present | `009BEF20` Begin / `009D8CF0` Clear / `009BEF50` End / `009BEEB0` Present | host swapchain Present | PROVEN wrappers | not PlayAVI `SetVideoFrame` |

---

## What 00A058C0 / 00A0AEA0 / 009DB700 actually are

| VA | Role |
|---|---|
| `00A058C0` | Dirty RS flush (`SetRenderState` vtbl+228 / +268 / +276). **Not** a primitive helper. |
| `00A0AEA0` | Bind up to N textures (`SetTexture` vtbl+260) then `DrawIndexedPrimitiveUP` prim **4**, INDEX16, stride from caller. |
| `009DB700` | Enqueue 60-byte record onto display `+16020`. Callers: `009DC00E`, `009DD93D` only. **Not** type `0x22`. |
| `009DA9F0` | Drain `+16020`. Empty → skip. Else `00A058C0` + `DrawPrimitiveUP` vtbl+332 prim 2 or 4, stride 32, VB `+16008`. |
| `00BAE2D0` | Type-0x22 handler: bind `VSHADER_2D_SPRITE`, write blend slots, `00A0AEA0`. |
| `00BAD8A0` | Instance vtbl+20: copy dest, optional texture size. **No** `E8 009DB700` in the body. |

---

## Host wiring (main agent)

Do **not** keep `CompositeFrontendPresent` as Present.

```
FrontendDx9DrawRecord (dest + uv + tex + blend 2)
  → Dx9VulkanFrontend.BuildBatch
  → FrontendSubmitBatch
  → EngineFrame new optional field
  → SilkEngineHost submits verts/indices/textures
```

`FrontendSubmitBatch` lives in `src/Fable.Render/FrontendDraw.cs`.
`IEngineHost.cs` / `SilkEngineHost.cs` / `EngineLifecycle.cs` are
untouched here.

---

## Files

- `src/Fable.Render/Parity/Dx9Vulkan/Dx9VulkanFrontend.cs`
- `src/Fable.Render/FrontendDraw.cs`
- `tests/Fable.Formats.Tests/Dx9VulkanFrontendTests.cs`
- this note
