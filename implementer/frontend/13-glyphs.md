# 13 — ENG_ARIAL_16 Press Start glyphs

Authority: `Fable.exe` `00AB96B0` / `00AB7A10` / `00AB7B00` /
`00AB7C20` / `0054EF00` / `0054FFF0`. Face `fonts.big`
`FONT_ENGLISH_MAIN` / `ENG_ARIAL_16`.

## 00AB7C20 vertex UV assignment

GPU UV = stored × (atlas−1)/atlas. Dest − `[0x122F59C]=0.5`.
Dest height = `CellHeight+1`. Six 28-byte XYZRHW verts,
prim 4, flush `00A0ABE0`.

| Vert | Dest | UV | Role |
|---|---|---|---|
| 0 | (X0, Y1) | (U0, V1) | BL |
| 1 | (X0, Y0) | (U0, V0) | TL |
| 2 | (X1, Y1) | (U1, V1) | BR |
| 3 | (X1, Y0) | (U1, V0) | TR |
| 4 | (X1, Y1) | (U1, V1) | BR |
| 5 | (X0, Y0) | (U0, V0) | TL |

**U0/V0 is dest top-left. V is not inverted.**

## 0054EF00 colour, align, +204

- Colour: widget `+148..+151` packed as a DWORD
  (`[148][149][150][151]` LE) into the type `0x27` record.
- Align `vtbl+600` `0054FFF0`: `+302` bit4 → 1 centre,
  bit5 → 2 right, else 0 left. First-seen bits stay 0 → left.
- Scale of `+204`: `scale * [esi+204]`. Scale is `+264`
  when `+392!=0`, else `+124`. Centre also × 0.5, then
  `originX - that`. Writer of leftover `+204` is not this fn.
- Then `+ [0x122DCDC]=2` on X and Y before the record.

## First scramble cause

Type-6 glyphs were submitted as type `0x22` sprite records
(stride 32, UV 0,0,1,1 family) instead of type `0x27` /
`00AB7C20` 6×28-byte verts with per-glyph GPU UV.
A whole-atlas 0,0,1,1 quad at the text dest is scrambled.

## Press Start table (`00AB7C20` at 0,0)

`TEXT_GUI_MENU_PRESS_BUTTON` = `Press Left Mouse Button To Continue`.
Measure **301 × 22**. destX includes bearing − 0.5. destX is monotonic.

| i | char | ch | atlas X0 Y0 X1 Y1 | width | advance | destX | destY |
|---|---|---|---|---|---|---|---|
| 0 | `P` | 80 | 43 88 55 110 | 12 | 12 | 0.5 | -0.5 |
| 1 | `r` | 114 | 73 154 80 176 | 7 | 6 | 12.5 | -0.5 |
| 2 | `e` | 101 | 64 132 75 154 | 11 | 10 | 17.5 | -0.5 |
| 3 | `s` | 115 | 81 154 91 176 | 10 | 9 | 27.5 | -0.5 |
| 4 | `s` | 115 | 81 154 91 176 | 10 | 9 | 36.5 | -0.5 |
| 5 | `sp` | 32 | 0 0 2 22 | 2 | 5 | 45.5 | -0.5 |
| 6 | `L` | 76 | 106 66 116 88 | 10 | 10 | 51.5 | -0.5 |
| 7 | `e` | 101 | 64 132 75 154 | 11 | 10 | 60.5 | -0.5 |
| 8 | `f` | 102 | 76 132 83 154 | 7 | 5 | 70.5 | -0.5 |
| 9 | `t` | 116 | 92 154 98 176 | 6 | 5 | 75.5 | -0.5 |
| 10 | `sp` | 32 | 0 0 2 22 | 2 | 5 | 80.5 | -0.5 |
| 11 | `M` | 77 | 0 88 14 110 | 14 | 15 | 86.5 | -0.5 |
| 12 | `o` | 111 | 39 154 50 176 | 11 | 10 | 100.5 | -0.5 |
| 13 | `u` | 117 | 99 154 108 176 | 9 | 10 | 111.5 | -0.5 |
| 14 | `s` | 115 | 81 154 91 176 | 10 | 9 | 120.5 | -0.5 |
| 15 | `e` | 101 | 64 132 75 154 | 11 | 10 | 129.5 | -0.5 |
| 16 | `sp` | 32 | 0 0 2 22 | 2 | 5 | 139.5 | -0.5 |
| 17 | `B` | 66 | 101 44 112 66 | 11 | 12 | 145.5 | -0.5 |
| 18 | `u` | 117 | 99 154 108 176 | 9 | 10 | 157.5 | -0.5 |
| 19 | `t` | 116 | 92 154 98 176 | 6 | 5 | 166.5 | -0.5 |
| 20 | `t` | 116 | 92 154 98 176 | 6 | 5 | 171.5 | -0.5 |
| 21 | `o` | 111 | 39 154 50 176 | 11 | 10 | 176.5 | -0.5 |
| 22 | `n` | 110 | 29 154 38 176 | 9 | 10 | 187.5 | -0.5 |
| 23 | `sp` | 32 | 0 0 2 22 | 2 | 5 | 196.5 | -0.5 |
| 24 | `T` | 84 | 99 88 110 110 | 11 | 12 | 202.5 | -0.5 |
| 25 | `o` | 111 | 39 154 50 176 | 11 | 10 | 213.5 | -0.5 |
| 26 | `sp` | 32 | 0 0 2 22 | 2 | 5 | 223.5 | -0.5 |
| 27 | `C` | 67 | 0 66 13 88 | 13 | 13 | 229.5 | -0.5 |
| 28 | `o` | 111 | 39 154 50 176 | 11 | 10 | 241.5 | -0.5 |
| 29 | `n` | 110 | 29 154 38 176 | 9 | 10 | 252.5 | -0.5 |
| 30 | `t` | 116 | 92 154 98 176 | 6 | 5 | 261.5 | -0.5 |
| 31 | `i` | 105 | 105 132 108 154 | 3 | 4 | 267.5 | -0.5 |
| 32 | `n` | 110 | 29 154 38 176 | 9 | 10 | 271.5 | -0.5 |
| 33 | `u` | 117 | 99 154 108 176 | 9 | 10 | 281.5 | -0.5 |
| 34 | `e` | 101 | 64 132 75 154 | 11 | 10 | 290.5 | -0.5 |

destX is strictly increasing. Width comes from
`WidthMinus1+1` in the 22-byte record, not column scanning.
Advance = BearingX + AdvanceTail.

## Host path

`FontFile.GlyphAt` / `GpuU` / `AtlasRect` match `00AB7A10` /
`00AB7C20`. `FrontendTextDraw.Layout` emits those dest/UV quads.
`EngineLifecycle` submits type `0x27` per glyph (not sprite
0,0,1,1). Type-6 pen adds `+2` (`0x122DCDC`).
