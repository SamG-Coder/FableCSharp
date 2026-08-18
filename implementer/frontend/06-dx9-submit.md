# Frontend native DX9 submit

Authority: `Fable.exe` only. Isolated types:
`src/Fable.Game/FrontendDx9Submit.cs`,
`src/Fable.Render/Parity/Dx9Vulkan/Dx9FrontendState.cs`.
Agent 7 translates these records. Do not invent
blend, UV flip, vertex format, or half-pixel.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

---

## Two queues, one immediate family

| Path | Insert | Drain | First-seen frontend |
|---|---|---|---|
| Type `0x22` widget sprite | `0041BEB0` → dest vtbl+92 `00B23BC0` → `00B324A0` | factory vtbl+20 `00BAE2D0` → `00A0AEA0` DIPUP | dest 0,0,0,0; `00BAD8A0` early-out; **no** `009DB700` |
| Display +16020 | `009DB700` (`009DBFF0` / `009DD8F0` only) | `009DA9F0` DrawPrimitiveUP vtbl+332 | empty skip `009DB6E6` |
| Type-6 glyphs | `0054EF00` → `00543910` type `0x27` size 64 | `00AB7C20` → `00A0ABE0` DrawPrimitive vtbl+324 | PRESS_START text widget |

**DISPROVEN:** nonempty dest `00BAD8A0` `E8 009DB700`.
Callers of `009DB700` are only `009DC00E` (`009DBFF0`)
and `009DD93D` (`009DD8F0`). `00BAD8A0` callees are
`009FE620` / `009F9DB0`.

---

## Frame (`0042DF9E`)

Order (E8s):

1. `00A0B560` → `009BF490` → `009BEF80` viewport
2. `009BECE0`
3. **`009D8CF0` Clear** (before BeginScene)
4. **`009BEF20` BeginScene** device vtbl+164
5. `00595582` / `00595222` walk `[ui+84]` vtbl+8
6. `[retail+88].vtbl+32` (`00B27D90`)
7. `009D9C80` / `009DA9F0(1)`
8. `00404A80` / `00404C00`
9. `009D9C80` / `009DA9F0(1)` again
10. **`009BEF50` EndScene** vtbl+168
11. **`009BEEB0` Present** vtbl+68, four NULL args

Same Present as PlayAVI.

### Clear `009D8CF0` / `009BE420`

`0042DF9E` colour bytes `00 00 00 FF` = **`0xFF000000`**.
Flags arg `0` → body default **`7`**. `009BE420` remaps
to D3DCLEAR: bit0→Z (2), bit1→stencil if format has it
(4), bit2→TARGET (1). Z=1.0, stencil=0, Count=0
(full surface). Device vtbl+172.

### Viewport `009BEF80`

SetViewport vtbl+188. Writes MinZ **0** at +492, MaxZ
**1.0** (`0x3F800000`) at +496. Clips the incoming
rect to device+404 / +408 (`009BEDC0`). `0042DF9E`
requests `(0,0, qword 0x13961E8, qword 0x13961F0)`
and the clip is the backbuffer. PE default display is
1024×768 (`EngineLifecycle.DisplayDefaultWidth`).

Half-pixel is **not** applied here (`fistp` only).

---

## Family 1 — full-screen / widget sprites

Packer `0041BEB0` (`ret 68`), dest `this+0x15C`,
size `0xC0`, type **`0x22`**. First-seen UVs written
`0,0` / `0,0` at `0041B4C6`. Blend arg push **2**.
`00595222` passes the two optional dest args as 0 →
`[edx+92]` = `00B23BC0`.

`00B324A0`: factory `[0x1436E84]+16+type*4`. dest+4=0
→ factory vtbl+4 `00BACFD0` (type 34 → vtbl
`0x12A54BC`, size `0x8C`) store dest+4, then
**instance** vtbl+20 = `00BAD8A0`. dest+4 already
set → same `00BAD8A0`. Factory vtbl `0x12A5664+20`
= `00BAE2D0` is **draw**, not this insert.

`00BAD8A0` copies rec+12 (4 floats) → instance+72,
rec+28..48 → +88..104, rec+32 → +92. If +92==0 and
rec+64==0 / rec+56==0 → `00BADB36 ret 8`. UV length²
≤ `0.0001²` (`0x129BA3C`) is treated as degenerate.

### Draw `00BAE2D0` → `00A0AEA0`

Dirty-list on `0x1436E18`. VS bind `00987FE0` /
`00988020`. Colour `fild` × `0x1231724` (1/255) into
`[dev+972]+32`, dirty bit 2. PS attach `00988140`.
Then `00A058C0` + **DrawIndexedPrimitiveUP** vtbl+336:

- prim **4** (`D3DPT_TRIANGLELIST`)
- MinVertex **0**
- index format **101** (`D3DFMT_INDEX16`)
- stride **32**
- texture-stage walk arg **2** (`SetTexture` vtbl+260)
- verts at handler+24, indices at handler+44

### Blend (`00BAF4B9`)

`[this+164] - 3`:

| value | SRC | DST | table |
|---|---|---|---|
| 3 | 2 ONE | 2 ONE | `0x1396F6C` |
| 4 | 2 ONE | 4 INVSRCCOLOR | `0x1396F74` |
| else (ctor **2**) | 5 SRCALPHA | 6 INVSRCALPHA | `0x1396F78` / `7C` |

Alphablend slot +10424 written **1**.

### VSHADER_2D_SPRITE (`SHADERS_POINT_SPRITE1`)

```
mov oPos, v0
mul r0, v0, c92.xyyy
add oT1.xy, r0, c92.zwww
mov oT1.zw, c0.y
mov oT0, v2
mov oD0, v1
```

No `dp4 oPos`. v0 already clip/screen. v1 colour,
v2 UV. Handler ctor `00BAD040` also names
`VSHADER_2D_CLOCK_SPRITE`,
`PSHADER_2D_CLOCK_SPRITE` (`mul r0, t0, c0` /
`mul r0.w, r0, t1.w`),
`PSHADER_2D_CLOCK_SPRITE_ADDITIVE`.

`SetFVF 0x144` write is **UNREAD**. Stride 32 with
28 used bytes (XYZRHW+DIFFUSE+TEX1 + 4 pad) is the
matching layout, not a recovered FVF immediate.
Sprite RHW store at +24 is **UNREAD**.

`+16020` HUD path uses **`VSHADER_BBBLIB_2D`**
(`mov oPos,v0` / `oT0,v3` / `oD0,v1` / `oD1,v2`)
and is a different family.

---

## Family 2 — type-6 text glyphs

`0054EF00` packs **type `0x27`**, size **64**, via
`00543910` (`mov [esi], 0x27`). Submit dest vtbl+92
or +112. Not type `0x22`.

`00AB7C20` walks UTF-16, one glyph → 2 triangles,
**6 verts × 28 bytes**. `rep movsd` 7 dwords. Writes
`0x3F800000` RHW. GPU UV =
`fileUV * (atlas-1) / atlas` (file stores
`pixel/(atlas-1)`). Dest x/y after scale
**`fsub [0x122F59C]=0.5`**. Flush `00A0ABE0` →
DrawPrimitive vtbl+324 type **4**, `SetTexture`
of `face+8240`. Face helper `0054F4B0` =
`ENG_ARIAL_16` A8/RGBA atlas (see `04-fonts.md`).

---

## Family 3 — display +16020 (HUD / PlayAVI 2D)

`009DB700` (`ret 24`): skip if `[device+472]`;
build 60-byte local; `add [+16024], 60` or grow
`009E1750`. Copies viewport via `00A0AAA0`
(device+0x204).

`009DA9F0(1)`: count `(end-begin)*0x88888889`
(60-byte). Zero → `009DB6E6`. Nonempty:
`00A058C0` then `[device+88].vtbl+332`
DrawPrimitiveUP, stride 32, VB +16008, prim
**2** (LINELIST) or **4** (TRIANGLELIST).
Then `009E15E0` / `009E1440` clear.

`009D9C80` is the sibling flush of +15996 /
VB +15984, same DIPUP 2-or-4.

`009DB810` NDC uses `[0x122DED8]=1.0`, **not** 0.5.

---

## Per-family checklist

| Field | Sprites 0x22 | Glyphs 0x27 | Frame |
|---|---|---|---|
| Vertex format | stride 32; v0/v1/v2; FVF write UNREAD | 28-byte XYZRHW+DIFFUSE+TEX1 | n/a |
| Positions | dest rec+12 screen px | pen + bearing − 0.5 | n/a |
| RHW | UNREAD | 1.0 written | n/a |
| UVs | first-seen 0,0; no V flip | file UV, V0 top, no 1−v | n/a |
| Texture | rec+32 / `009FE620`; stages 2 | face+8240 stage 0 | n/a |
| Sampler | UNREAD | UNREAD | n/a |
| Alpha test | UNREAD | UNREAD | n/a |
| Blend | +164; default SRCALPHA/INVSRCALPHA | UNREAD as a first-seen RS write | n/a |
| Colour | ×1/255 into c at +972+32 | widget RGBA in vert +16 | clear `0xFF000000` |
| Transform | `mov oPos, v0` no WVP | same | viewport clip |
| Viewport | inherited | inherited | `009BEF80` MinZ 0 MaxZ 1 |
| Scissor | UNREAD | UNREAD | UNREAD |
| Topology | TRIANGLELIST (4) DIPUP | TRIANGLELIST (4) UP | n/a |
| VB/IB | +24 / +44, INDEX16 | immediate 28-byte verts | n/a |
| Batching | one DIPUP per handler draw | flush every 0x126 tris | two flush pairs |
| RT / clear | none | none | TARGET+Z(+S) |
| Begin/End/Present | inside 0042DF9E | inside 0042DF9E | Clear → Begin → … → End → Present |

---

## Host DIVERGE (do not keep)

`EngineLifecycle.QueueFrontend2dRecord` nonempty dest
notes `00BAD8A0 009DB700 +16020`. Native does not.
`DisplayDipPrimTris=4` is **list**, not strip.
`Dx9VulkanFrontend.PixelShaderName` =
`PSHADER_2D_TEXTURE_DIFFUSE` is the wrong program;
handler binds **`PSHADER_2D_CLOCK_SPRITE`**.
`HandlerBlendOffset=312` is **164**.
