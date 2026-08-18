# 15 — Frontend DX9 child draw → Vulkan submit

Present path is `FrontendSubmitBatch`. `CompositeFrontendPresent`
CPU blit into `FrontendPresentRgba` is a **TEMPORARY** test dump
only. Host `SilkEngineHost` no longer `SetVideoFrame`s that bitmap.

Authority: `Fable.exe` `00BAE2D0` / `00BB0970` / `00A0AEA0` /
`0054EF00` / `00AB7C20`.

Statuses: **PROVEN** / **EQUIVALENT** / **UNREAD** / **TEMPORARY**.

---

## Present contract

```
widget dest (PositionX/Y × scale)
  → type 0x22 rec 0xC0 dest rec+12 / instance+72
     or type 0x27 rec 64 (type-6 text)
  → 00BB0970 fills 4 verts at arg+24 (sprites)
     or 00AB7C20 fills 6×28-byte verts (glyphs)
  → 00BAE2D0 00A0AEA0 DIPUP prim 4 INDEX16 stride 32
     or 00A0ABE0 DrawPrimitive vtbl+324 prim 4
  → Dx9VulkanFrontend.BuildBatch
  → EngineFrame.FrontendBatch
  → VulkanLineRenderer.SetFrontendBatch
  → CmdDrawIndexed / CmdDraw
  → 009BEEB0 Present (swapchain)
```

`FrontendRgba` / `SetVideoFrame` is PlayAVI (`009DC870`) only.

---

## 00BAE2D0 does not write dest/UV

Handler draw (`vtbl 0x12A5664+20`). Binds `VSHADER_2D_SPRITE`
(`00988020` handler+12), writes blend RS, PS c0, then DIP.

Type **0x22** tail `00BAF94C`:

| push | meaning |
|---|---|
| ebx=2 | PrimitiveCount (2 triangles) and SetTexture stages |
| arg+24 | vertex pointer (`add ebp, 24`) |
| 32 | stride |
| 4 | NumVertices |
| [esp+20]+44 | INDEX16 (`00BAD040` handler+44 = 0,1,2,1,3,2) |

Type **35** path uses verts at +24 / indices +248, `push 7`.
Frontend widgets are type 34 / record `0x22`.

`00A0AEA0` (`ret 28`): SetTexture vtbl+260 loop, `00A058C0`,
then DrawIndexedPrimitiveUP vtbl+336:

- prim **4** `D3DPT_TRIANGLELIST`
- MinVertex **0**
- index format **101** `D3DFMT_INDEX16`
- stride from caller (**32**)

Empty dest: `00BAD8A0` copies rec+12 → instance+72 then
`00BADB36 ret 8` when rec+32/64/56 are 0. **No** `009DB700`.
Host `AppendRecord` skips `DestX1<=DestX0 || DestY1<=DestY0`.

---

## 00BB0970 vertex fill (which UV on which corner)

`00BAE2D0` consumes a buffer already filled at arg+24.
Fill is `00BB0970` (clip against display, then write 4 verts).

Miss / no-texture default `00BB0EE4`:

```
(u,v) = (0,0), (1,0), (0,1), (1,1)
```

Frame UV from `009FC810`. Rec+68 is an **offset** added to that
frame. First-seen packer `0041BEB0` writes 0,0,0,0 → the frame
quad is left as-is (full texture 0,0,1,1 when frame==tex size).

Dest pixels from instance+72 (`00BAD8A0` copy of rec+12).
No half-pixel on this path (`AppliesHalfPixelOffset=false`).
V=0 is dest **top**. No `1-v`. Persist FlipU/V absent.

Corners in dest space (Y down), stride 32, RHW 1.0 at +12:

| i | dest | UV | name |
|---|---|---|---|
| 0 | (x0,y0) | (u0,v0) | TL |
| 1 | (x1,y0) | (u1,v0) | TR |
| 2 | (x0,y1) | (u0,v1) | BL |
| 3 | (x1,y1) | (u1,v1) | BR |

INDEX16 `0,1,2, 1,3,2` = TL-TR-BL / TR-BR-BL.
`Dx9VulkanFrontend.BuildDx9Quad` + `QuadIndices`.

Host indices are 0-based per draw; `CmdDrawIndexed` adds
`FirstVertex` (native DIPUP MinVertex 0 per handler).

---

## 0054EF00 / 00AB7C20 glyphs

`0054EF00` reads widget RGBA +148..+151, text at +348, packs
**type `0x27`**, size **64**, via `00543910`. Submit dest
vtbl+92 or +112. **Not** type `0x22`.

`00AB7C20` walks UTF-16. One glyph → 2 triangles, **6 verts ×
28 bytes** (`rep movsd` 7 dwords). Writes `0x3F800000` RHW.
Dest `fsub [0x122F59C]=0.5`. GPU UV =
`fileUV * (atlas-1) / atlas`. Flush `00A0ABE0` → DrawPrimitive
vtbl **324** type **4**, `SetTexture` of `face+8240`.

Host packs those 28 used bytes into `FrontendDx9Vertex` (4-byte
pad to stride 32) and emits the 6-vert list
`TL,TR,BL, TR,BR,BL` (`BuildDx9GlyphList`). EQUIVALENT to the
indexed quad; native is unindexed `DrawPrimitive`.

---

## Pixel shader

`00BAD040` binds **`PSHADER_2D_CLOCK_SPRITE`**:

```
ps_1_1
mul r0, t0, c0
mul r0.w, r0, t1.w
```

Not `PSHADER_2D_TEXTURE_DIFFUSE` (`mul r0, v0, c0` …).

c0 is a PS constant at `[dev+972]+32`, written by `00BAE2D0`
from a byte scale `× [0x1231724]` (1/255). If device+913 is
set, c0 is overwritten with **(1,1,1,1)**. Vertex diffuse
(`mov oD0, v1`) is **UNREAD** by this PS.

First-seen c0 is identity. **TEMPORARY:** GLSL
`texture(sprite, fragUv) * vec4(1.0)`. Do not multiply
`fragColor` until a non-white c0 write is recovered as the
clock-sprite path.

Stage-1 `t1.w`: TEMPORARY identity (2 stages pushed; stage 1
unread as a bound texture first-seen).

---

## Blend / scissor / depth

| Field | Native | Vulkan | Status |
|---|---|---|---|
| Blend default | widget +372=2 → `[0x1396F78]=5` `[0x1396F7C]=6` | SRC_ALPHA / ONE_MINUS_SRC_ALPHA | PROVEN |
| Blend offset | type 0x22 `[arg+164]` (`00BAF4B9`) | `HandlerBlendOffset=164` | PROVEN |
| Scissor | no first-seen `SetScissorRect` | none | UNREAD — not invented |
| Alpha test | unread | no discard | UNREAD |
| Z test/write | `00BAE2D0` writes 0 at +10324/+10344 | off | TEMPORARY (slot RS PARTIAL) |
| Topology | prim 4 triangle list | `VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST` | PROVEN |

---

## What this replaced

| Before | After |
|---|---|
| `CompositeFrontendPresent` CPU blit → `FrontendRgba` → `SetVideoFrame` | `CollectFrontendRecords` → `BuildBatch` → `SetFrontendBatch`. Blit lives in `DumpFrontendPresentRgba` (TEMPORARY dump / press-start.bmp) |
| Host fallback: nonempty `FrontendRgba` presents as video | Removed. Empty batch clears; AVI still uses `SetVideoFrame` |
| Glyphs packed as type 0x22 indexed quads | Type **0x27**, stride **28** used bytes, 6-vert `DrawPrimitive` list |
| `PSHADER_2D_TEXTURE_DIFFUSE` + `texture * fragColor` | `PSHADER_2D_CLOCK_SPRITE` + `texture * c0` with c0 TEMPORARY white |
| `HandlerBlendOffset=312` | **164** (0x22). Type 35 still uses handler+312 |
| Nonempty dest note `009DB700 +16020` | `00BAE2D0 no 009DB700` |
| Indexed IB stored `base+i` and draw used `vertexOffset=FirstVertex` | 0-based indices per draw; `vertexOffset=FirstVertex` matches DIPUP |

Pipeline stays Vulkan (`VulkanLineRenderer.Frontend`). No
return to `SetVideoFrame` for UI.

---

## Files

- `src/Fable.Render/Parity/Dx9Vulkan/Dx9VulkanFrontend.cs`
- `src/Fable.Render/FrontendDraw.cs`
- `src/Fable.Render/LineShaders.cs`
- `src/Fable.Render/VulkanLineRenderer.Frontend.cs` (unchanged bind; 0-based IB)
- `src/Fable.Game/EngineLifecycle.cs` (`CollectFrontendRecords` / dump split)
- `src/Fable.Client/SilkEngineHost.cs` (batch-only UI Present)
- `tests/Fable.Formats.Tests/Dx9VulkanFrontendTests.cs`
- `tests/Fable.Formats.Tests/EngineLifecycleTests.cs`
- this note
