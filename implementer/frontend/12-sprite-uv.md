# 12 — sprite UV, winding, TextureFile orientation

Authority: `Fable.exe` `00BAD8A0` / `00BAE2D0` / `0041BEB0` /
`00BB0970` / `009FC810` / `00BAD040`, plus `frontend.big`
info headers. Isolated types:
`FrontendDx9Submit`, `Dx9VulkanFrontend.BuildDx9Quad`,
`TextureFile`. No logo / `FORREST` name map.

Statuses: **PROVEN** / **UNREAD**.

---

## Record UVs (`0041BEB0` / `00BAD8A0`)

`0041AFA0` first-seen writes two vec2s `0,0` / `0,0`
(`0041B4C6`) then `call 0041BEB0`. Packer copies them
to rec **+68 / +72 / +76 / +80** (`U0 V0 U1 V1`).
`+56` is 0 (dl). No persist `FlipU` / `FlipV` CRC is
read on this path.

`00BAD8A0` copies rec+68..+80 → instance **+117 / +121 /
+125 / +129**. If both corners have length² ≤
`[0x129BA3C]²` (`0.0001²`) it stores 0 at +133
(degenerate). First-seen 0,0,0,0 takes that branch.

Those four floats are **not** a dest-space UV rect
used as-is by DIPUP.

---

## Submitted corners (`00BB0970` / `009FC810`)

Handler vtbl+56 `00BB0970` (type 34 / `0x22`) builds
the 0xA8 draw object `00BACE50`. `00BAE2D0` only
DIPUPs verts already at arg+24.

Texture frame UV: `009FC810` (`GetLevelDesc` vtbl+68
for w/h). Wrapper +16/+18 is origin, +20/+22 is
frame size. Origin is **not** in the 34-byte info
(+6/+8 is size). First-seen origin 0. `fmul
[0x129C81C]≈1/32768` on origin; size / texSize
gives the opposite corner. **No `1-v`.** V=0 is
frame Y origin = texture top (DX9).

Texture-miss default `00BB0EE4`:

| slot | value |
|---|---|
| +100 / +104 / +108 / +112 | 0, 1, 1, 0 |
| +116 / +120 / +124 / +128 | 0, 0, 1, 1 |

Mapped onto verts (see below) as
**(0,0), (1,0), (0,1), (1,1)**.

Rec UV is then added as an offset
(`U0+(U1-U0)*scale` from `[0x143B934+3*rec+4]`;
PE table is BSS 0). Degenerate rec adds 0.

So first-seen rec **0,0,0,0** + full-frame texture
(info +6/+8 == +0/+2) → submitted **0,0,1,1**.
A smaller frame is `0, 0, FrameW/W, FrameH/H`.
Sub-rect origin other than 0 is **UNREAD** in the
bank header.

`00BAE2D0` binds textures via `00A0AEA0` SetTexture
vtbl+260, stage count 2. First-seen **D3DTSS**
COLOROP write is UNREAD (shader path).

Half-pixel: none in `00BAD8A0` / `00BAE2D0`.
`00BB0970` uses `[0x122F59C]=0.5` as **half dest
size** (center) and later an NDC bias. Not a UV
flip. Sprite dest half-pixel stays UNREAD on the
DIPUP filler.

---

## Vertex order (`00BAD040` / `00BB0970`)

`00BAD040` INDEX16 at handler+44 (66-prefix words):

```
+44: 0
+46: 1
+48: 2
+50: 1
+52: 3
+54: 2
```

`00BB0970` writes four stride-32 verts at draw+24:

| i | dest | UV |
|---|---|---|
| 0 TL | (x0,y0) | (u0,v0) |
| 1 TR | (x1,y0) | (u1,v0) |
| 2 BL | (x0,y1) | (u0,v1) |
| 3 BR | (x1,y1) | (u1,v1) |

Winding **TL-TR-BL / TR-BR-BL**. `00A0AEA0` prim 4
`D3DPT_TRIANGLELIST`. Not inverted for Vulkan;
clip Y is `DestPixelToDx9Clip` only.

RHW store at +24 is still UNREAD; we write 1.0.

---

## TextureFile orientation

CreateTexture `009BE8B0` FourCC DXT1/DXT3, pool
SCRATCH. DXT block row 0 is the image top
(DX9 LockRect). `Dxt.Decode` writes `by=0` to
`y=0`. **Decode does not flip.** A host V flip
would invert a correct top-down RGBA.

RGBA8 (format 1) is the same row order.

Header +10 is not a flip flag. No FlipU/FlipV
persist field.

TITLE_01 info: 256×128, frame 256×128, format 1.
FORREST_1_1: 256×256, frame 256×256, format 31.
Both **full atlas** → FrameUv 0,0,1,1.

---

## Isolated API

- `FrontendDx9Submit.SubmittedSpriteUv` /
  `RecUvDegenerate` / `QuadIndices`
- `Dx9VulkanFrontend.BuildDx9Quad` (TL-TR-BL-BR)
- `TextureFile.FrameUv` / `DecodeRowZeroIsTop` /
  `FirstSeenDecodeFlipsVertical=false`

Do not invent a global V flip. Do not special-case
widget names.
