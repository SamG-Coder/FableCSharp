# Glyph UV / type-6 submit — remaining gaps

Authority: `implementer/frontend/fn-00AB7C20-exact.txt`,
`fn-0054EF00-exact.txt`, `fn-0054FFF0-exact.txt`,
`listing-00540000.txt` (`0054ED90` / `0054F4B0` / `0054F5C0`),
`listing-00500000.txt` (`005339B0` / `0052FFxx`),
`listing-00b80000.txt` (`00BB0970`),
`listing-009c0000.txt` (`009D49B0`),
`export/frontend/persist-tail.txt`,
`src/Fable.Game/FrontendTextDraw.cs`,
`src/Fable.Formats/Fonts/FontFile.cs`,
`EngineLifecycle.CollectFrontendRecords`.

Do not re-prove GPU UV = stored × (atlas−1)/atlas, dest − 0.5,
dest H = CellHeight+1, U0/V0 dest top-left, type-6 +2 pad,
first-seen left (if def+508==0), Press Start measure 301×22.

---

## Status table

| Item | Status | Native | C# |
|---|---|---|---|
| Type-6 record type | **MATCH** | `0054EF00` `mov ebp, 0x27` / `00543910` | `FrontendTextDraw.Type6RecordType=0x27`. No live path still packs type-6 as `0x22`. Default `FrontendDx9DrawRecord.RecordType` is `0x22` (sprite); glyphs pass `0x27`. |
| GPU UV / dest −0.5 / dest H | **MATCH** | `00AB7C20` | `FontFile.GpuU/V`, `FrontendTextDraw.Layout` |
| Type-6 pen +2 | **MATCH** | `fadd [0x122DCDC]` | `Type6OriginPad=2` |
| First-seen align left | **PARTIAL** | `0054FFF0` bits 4/5. `005331A0` does **not** write them. Type-6 ctor `0054ED90` **does** `or [+302],0x10/0x20` from **`[def+508]`**. | `CollectFrontendRecords` hardcodes `AlignLeft`. |
| Type-6 leftover `+204` writer | **UNREAD** | `0054EF00` `fmul [esi+204]` for centre/right only. `0041AC20` writes texture w when `+376!=0`. `0054F5C0` does not measure. `00AB7B00` is not a callee of `0054EF00`. | Uses `DestX1-DestX0` (first-seen 0). Not `00AB7B00` 301. |
| Font leftover as `0041AFA0` dest | **MATCH (first-seen)** | Type-6 GraphicIndex 0 → leftover skip; dest origin only. Font measure is not dest. | `LayoutFrontendWidgets` leftover only if `GraphicId!=0`. TEXT dest is a point; glyphs emit from pen. |
| Persist Font 26051 → `ENG_ARIAL_16` | **UNREAD** | See below. | Hardcoded `FontFile.UiFace`. `FrontendWidget` has no Font field. |
| Colour `+148..+151` vs `PackPersistColour` | **PARTIAL** | See below. First-seen PRESS_START_TEXT is white either way. | `PackPersistColour` = D3D `0xAARRGGBB`. |
| Sprite dest half-pixel on DIPUP | **UNREAD** | `00BB0970` `fmul 0.5` = half dest size; `fsub 0.5` is texel/NDC bias, **not** dest−0.5. No `fsub 0.5` on dest X0/Y0 in `00BAD8A0` / `00BAE2D0`. | `AppliesHalfPixelOffset=false`. Sprites unshifted. |
| Glyph 6-vert order | **EQUIVALENT** | `00AB7C20` BL,TL,BR,TR,BR,TL | `NativeVerts` matches. `BuildDx9GlyphList` remaps to TL,TR,BL,TR,BR,BL (same two tris). |
| Atlas size / A8 | **C# MATCH / docs STALE** | 128×256 RGBA, pitch 512 (`00AB960A`). | `FontFile` decode. `PARITY.md` still says A8 `w*h` and test name `…512x256…`. |

---

## 1. Persist Font 26051 — who resolves it?

`export/frontend/persist-tail.txt`:

- groups / menu: `Font i32=224`
- `UI_PRESS_START_TEXT`: **`Font i32=26051`** (`0x51E278F0`)

Type-6 ctor `0054F5C0` → `0054ED90` (not `0054F4B0`):

```
vtbl+432 → def
push [def+80]                 ; persist Font i32
mov ecx, 0x13CA828
call 009D49B0                 ; names blob: lea eax,[base+esi+4]
call 009E2C80                 ; face lookup by CString
```

`009D49B0`: if id != −1, `add ecx,12` / `00995E70` then
`lea eax,[eax+esi+4]` / `0099EBF0`. That is a **names.bin
offset** (CRC dword then CString), not a `fonts.big`
`BankEntry.Id` and not `FableCrc("ENG_ARIAL_16")`.

`0054F4B0` **hardcodes** `"ENG_ARIAL_16"` then `009E2C80`.
Callers: `00540541`, `00540BE1`, `0054FB4D`. **Not** the
type-6 ctor.

C# `CollectFrontendRecords` always `TryLoad(FrontendUiFontFace)`
=`ENG_ARIAL_16`. `FrontendWidgetFactory` drops `def.Font`.

**Proposed fix:** resolve `FrontendUiDef.Font` via
`NamesBin.Get((uint)font)` (offset 26051 / 224). If the
string is `ENG_ARIAL_16`, lock it. Do not treat `0054F4B0`
as the persist mapper.

**Classification:** UNREAD mapping; C# assumption EQUIVALENT
only if `NamesBin` offset 26051 is `ENG_ARIAL_16`.

---

## 2. Type-6 leftover `+204` writer

`0054EF00` (centre/right only):

```
fld scale          ; +264 if +392!=0 else +124
fmul [esi+204]
; centre: fmul [0x122F59C]
fsubr originX
```

Writers of `+204` recovered elsewhere:

| VA | When | Value |
|---|---|---|
| `0041AC20` | `+376!=0` (first style GraphicIndex) | bank vtbl+84 frame W |
| `0054E640` | other widget; uses `[esi+368]` | bank vtbl+84 |
| `00547D52` | PlayAVI widget | video w |

`0054F5C0` / `0054EF00` do **not** call `00AB7B00`.
First-seen type-6 GraphicIndex 0 → `0041AC20` skips →
`+204` stays ctor 0. Left align ignores it.

C# `leftoverW = DestX1-DestX0` is also 0 (persist W/H=0).
First-seen left: **EQUIVALENT**. Centre/right would need
the real writer (not dest leftover, not invented measure
unless a VA stores `00AB7B00` out+0 into `+204`).

**Proposed fix:** keep leftover 0 on first-seen type-6.
Do not feed font measure into `0041AFA0` dest. Dump the
writer of `+204` after `0054ED90` if centre is ever live.

**Classification:** UNREAD writer; first-seen left MATCH.

---

## 3. Type-6 ctor can set align (`def+508`)

`0054ED90`:

```
eax = [def+508]
0 → or [widget+302], 0x08
1 → or [widget+302], 0x10   ; 0054FFF0 centre
2 → or [widget+302], 0x20   ; right
```

`02-layout.md` “first-seen bits stay 0” is only true if
`def+508==0`. Sequential persist stops at `0x56A59976`.
C# never reads this dword.

**Proposed fix:** dump `CUIDef+508` on `UI_PRESS_START_TEXT`.
If 0, lock left. If 1, use `Type6Pen(..., leftover204, AlignCentre)`.

**Classification:** UNREAD persist; C# AlignLeft is a
first-seen assumption.

---

## 4. Colour `+148..+151` vs `PackPersistColour`

Native:

- ctor `005334A0` zeros `+144..+151`
- `005339B0` writes **`+144..+147=0xFF` only** (not +148)
- `0052FFxx` (`~0052FDD6`) packs persist floats ×
  `[0x1230014]≈255` into a DWORD at **`+148`** (blend with
  `+144` via `1/255` then ×255). Channel order not pinned
  past “four bytes → `mov [esi+148], eax`”
- `0054EF00` copies `[148][149][150][151]` LE into the
  `0x27` record; `00AB7C20` uses that DWORD as D3DDIFFUSE

C# `PackPersistColour`:

```
all-zero persist → 0xFFFFFFFF
else (A<<24)|(R<<16)|(G<<8)|B     // D3D 0xAARRGGBB
```

PRESS_START_TEXT persist RGB**A** = 1,1,1,1 → both white.
`PackPersistColour` does **not** implement `0052FFxx` or
the `+144` vs `+148` split.

If persist A=0, C# yields `00FFFFFF`; native `+151` may be
0 and `0054EF00` `cmp [esi+151],0` / `jbe` takes the
`+394` skip arm.

**Proposed fix:** write persist×255 into `+148` as the
`0052FFxx` DWORD (recover channel order from the
`esp+12/16/20/24` clamp). Stop treating all-zero as a
special case until `+148` ctor+tick is shown to become
white. Keep D3D ARGB on the record once bytes match.

**Classification:** PARTIAL. First-seen TEXT/LEGAL white
is EQUIVALENT. Channel order + all-zero + A=0 skip UNREAD.

---

## 5. Sprite dest half-pixel on DIPUP

`00BB0970` (starts `00BB0970`):

- `fsub dest1, dest0` × `[0x122F59C]` = **half dest size**
- later `fsub [0x122F59C]` on a `fmul [0x1230010]` UV/NDC
  term, then `fistp` / table `0x13CD550` — texel bias,
  not dest X0−0.5

No dest−0.5 on `00BAD8A0` / `00BAE2D0` dest copy.

C# sprites: unshifted dest, `AppliesHalfPixel=false`.
Glyphs: `Layout` already −0.5; `BuildBatch` does not
subtract again.

**Proposed fix:** none for dest pixels. Leave sprite
DIPUP dest unshifted. Do not invent dest−0.5 on `0x22`.

**Classification:** UNREAD as a dest-pixel write;
current C# dest is consistent with recovered uses of 0.5.

---

## 6. `CollectFrontendRecords` leftover vs `0041AFA0`

Type-6 widgets have persist W/H=0, GraphicId=0.

- dest = origin point (e.g. 512,384)–(512,384)
- leftover204 = 0
- pen = origin + 2
- one `0x27` record per glyph

This is **not** “font leftover as `0041AFA0` dest”.
Frame dump `zero-size` on TEXT is dest-rect only.

---

## Proposed C# edits (do not apply here)

1. Keep type `0x27` / 28-byte / −0.5 / GPU UV.
2. Resolve persist Font via `NamesBin` offset; store on
   `FrontendWidget`; load that face.
3. Read `def+508` before hardcoding left.
4. Split `+144` (005339B0 FF) vs `+148` (0052FFxx).
5. Do not use `00AB7B00` width as type-6 `+204` until a
   writer is pinned.
6. Refresh `PARITY.md` A8/512×256 line.
