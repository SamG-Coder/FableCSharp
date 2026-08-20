# fonts.big MAIN face + type-6 text

Authority: `Fable.exe` MAIN ctor `00AB8E10`, glyph reader
`00AB96B0`, lookup `00AB7A10`, measure `00AB7B00`, draw
`00AB7C20`. Type-6 widget ctor `0054F5C0`, draw `0054EF00`,
face helper `0054F4B0` (`ENG_ARIAL_16`). GPU wrap `009FE620`
is not the file parse.

## File header (`00AB8E10`)

CString family, then in order:

| Face offset | File | ENG_ARIAL_16 | Use |
|---|---|---|---|
| +4 | u32 | 21 | `CellHeight`. Dest height and measure line height = this + 1 |
| +20 | u32 | 400 | Weight (700 on ENG_ARIAL_18) |
| +24 | u8 | 0 | Flag |
| +16 | u32 | 21 | Equals cell height on MAIN faces; unused by draw |
| +28 | u32 | 128 | Atlas / UV width |
| +32 | u32 | 256 | Atlas / UV height |
| +36 | u32 | 32 | Min char |
| +40 | u32 | 127 | Max char |
| | u32 | 2 | Page bucket count |

Each bucket: u32 page index, u32 first (low 16 stored), u32
count. Glyphs follow packed 22 bytes each. Page table is 1024
slots of 8 bytes at `face+48` (first u16, count u16, ptr).
Texture object at `face+8240`.

`00AB7A10`: page = `ch >> 6`, index = `(ch & 63) - first`.

## Glyph record — 22 file bytes, 24 stride

`00AB96B0` reads four floats then three int16s (2 bytes
consumed, dword-stored at +16/+18/+20). **No height field.**

| Off | Type | Name | `!` | `A` |
|---|---|---|---|---|
| +0 | f32 | U0 | 3/127 | 86/127 |
| +4 | f32 | V0 | 0 | 44/255 |
| +8 | f32 | U1 | 6/127 | 100/127 |
| +12 | f32 | V1 | 22/255 | 66/255 |
| +16 | i16 | BearingX | 2 | -1 |
| +18 | i16 | WidthMinus1 | 2 | 13 |
| +20 | i16 | AdvanceTail | 4 | 12 |

Pen: `x += BearingX`, emit dest, `x += AdvanceTail`.
Advance = BearingX + AdvanceTail. Dest width = WidthMinus1+1.
Dest height = CellHeight+1 (22), not from the record.
Newline in draw (`00AB7C20`) adds **CellHeight** (no +1).
Measure (`00AB7B00`) uses CellHeight+1 as line height and
returns max line width + total height.

Stored UV is `pixel / (atlas-1)`. Draw converts:

```
gpuU = U * (UvWidth-1) / UvWidth   // pixel / width
gpuV = V * (UvHeight-1) / UvHeight
```

Pixel origin is `round(U * (W-1))`. `!` is atlas x 3..6,
y 0..22. A 512-wide A8 atlas is DISPROVEN: `U0*(511)` is not
integer; `U0*127` is.

Dest is shifted by D3D9 half-pixel `0x122F59C` = 0.5.

## Atlas

After pages: u32 blob size, then 18-byte prefix (`add eax,18`
/ `edx=0x12`), then `size-18` pixels.

18-byte prefix on ENG_ARIAL_16:

| Off | Value |
|---|---|
| +0 u32 | 131072 (payload bytes) |
| +4 u32 | 0 |
| +8 u32 | 0 |
| +12 u16 | 128 |
| +14 u16 | 256 |
| +16 u16 | 0x2820 |

Ctor ignores the prefix for CreateTexture and uses face
+28/+32. Format is 8,8,8,8 via `009E3790`; pitch
`shl width,2` at `00AB960A` = 512 bytes/row. Payload is
128×256×4 RGBA, white RGB + 6-level alpha (not file A8 at
`w*h`, not 512×256). `009BE870` DXT3 is a scratch format
object, not the atlas. `009FE620` is GPU cache wrap.

## Native draw path

Type-6 `0054F5C0` → `0054ED90` looks up a face through
`009E2C80`. Nearby helper `0054F4B0` names `ENG_ARIAL_16`.
Draw `0054EF00` reads widget RGBA at +148..+151, resolves
text at +348, looks up a style via singleton `0041E5F2`
vtbl+144 `0041E3B2`, packs a **type 0x27** 64-byte record
(`00543910`, `[rec]=0x27`). Submitted with size 64 through
vtbl+112. **Not type 0x22.**

Font vtbl `0129EAD4+20` = `00AB7C20` tessellates UTF-16
(advance 2). One glyph → 2 triangles (`add eax,2`), 6 verts
of 28 bytes (XYZRHW + diffuse + uv). Flush through
`00A0ABE0` → `DrawPrimitive` type **4** (`D3DPT_TRIANGLELIST`),
`SetTexture` of `face+8240`. No CPU composite.

Type-6 adds +2.0 (`0x122DCDC`) to widget x/y before the
record; that pad is widget-level, not `00AB7C20`.

`0054EF00` constructs and submits **two** 64-byte type `0x27`
records. Stack bytes `+36..+39` are zero RGB plus the widget
alpha; `+32..+35` are the widget RGBA. The black record is
submitted first, followed by the normal-colour record on the
next layer. Both contain the complete string. With the font
atlas alpha and SRCALPHA/INVSRCALPHA blending, the first pass
forms the black antialiased edge visible behind the white text.

## Localisation / colour / tags

`UI_PRESS_START_TEXT` TextTag `TEXT_GUI_MENU_PRESS_BUTTON`
is UTF-16 in `text.big` / `TEXT_ENGLISH_MAIN`:
`Press Left Mouse Button To Continue`. `00AB7C20` has no
inline colour-tag parser; it emits one tinted quad per
code unit plus newline `0xA`. Colour is the widget RGBA
copied into the 0x27 record.

Host authority path is `FrontendTextDraw.Layout` (glyph
quads + atlas UV). `FontFile.Blit` is export-only.
