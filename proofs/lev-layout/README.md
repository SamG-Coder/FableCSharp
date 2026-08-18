# LEV header / cell layout (`00B3EFA0` vs C#)

Investigation only. Production `src/` was not edited.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Question: what does `ParseMapHeader` `00B3EFA0` actually write, and
where does C# `LevFile` / `LevCellGrid` diverge? First LEV after
Leave. Dump: `landscape-trace/parsemapheader-00b3efa0.md` (capped at
**80** insns). Full body: `listing-00b00000.txt` `00B3EFA0`–
`00B3F5DC`.

---

## Verdict

**`00B3EFA0` is not a 48-byte memcpy of the compiled `.lev` prefix.**

It is a fast-path stream reader (`edx` = Lionhead stream, `ecx` =
dest). Sixteen `u32`s, then a 24-byte block, then one more `u32`.
**92 file bytes** land on a dest object whose packing is **not**
file order.

C# `LevFile.NativeHeaderBytes = 48` and `ReadHeader` (file `+0 / +4 /
+36>>16 / +40>>16 / +44/65536`) therefore **do not** implement
`00B3EFA0`. Version / constant / Lookout grid numbers still match
tests because those file dwords exist; dest offsets and the four
`u16` compares do not.

First `00B428E0` after Leave **never calls** `00B3EFA0`
(`proofs/stb-first-open`: `FinalAlbion.stb` miss). The first time
this function can run is a later STB-hit `00B41E50` or miss
`00B42530`.

WAD **21-byte / 1 m** cells (`LevCellGrid`) are not the **72-byte /
16 m** records `00BDC2D0` walks into `00BF4570`.

---

## Why the landscape-trace dump is not enough

`Fable.ExeIndex` `WriteFnPart(..., ParseMapHeader, 0x00B3EFA0, 80)`
stops at `00B3F06D`. The first dest stores in that window are only
`[ebx]=stream0` and `[ebx+4]=stream1`. C-terrain already noted 599
insns; listing is 00B3EFA0…00B3F5DC then `int3`. **PROVEN**
truncation.

Callers (full listing):

| Site | VA | dest | After parse |
|---|---|---|---|
| STB-hit attach | `00B41E84` | `esp+24` | copy dest+64 (24 B) → slot+36; **no** size/origin compare |
| STB-miss open | `00B4260A` | `esp+68` | `movzx` map+92/+94/+96/+98 vs dest+32/+36/+40/+44; then same 24 B copy |

New Game first `00B428E0` takes neither: `[+52].vtbl+12` misses,
`test bl; je 00B428CA`. **PROVEN** (`stb-first-open`).

---

## Stream object (`edx` / `esi`)

Inline fast path (repeated ~16 times) plus slow `00993CA0`:

```
[esi+4]   consumed count
[esi+12]  current pointer
[esi+20]  remaining bytes
```

Fast `u32`: if remaining ≥ 4, `edi = [[esi+12]]`, advance pointer /
remaining / count by 4, stash at `[esp+12]`. Else `push 4; lea dest;
call 00993CA0`.

24-byte block at `00B3F4F7`: `edi=0x18`, dest `lea ecx,[ebx+64]`,
six dwords from `[esi+12]`. Last dword at `00B3F5A5 mov [ebx+88],edx`.

Calling convention: `__fastcall`-like `ecx=dest`, `edx=stream`.
`ebx` holds dest for the whole body. **PROVEN.**

---

## Dest remap (stream dword → dest offset)

File order is sequential. Dest is **not**.

| i | file | dest | Evidence | Status |
|---|---|---|---|---|
| 0 | +0 | +0 | `00B3EFFA mov [ebx], ecx` | **PROVEN** |
| 1 | +4 | +4 | `00B3F04E mov [ebx+4], eax` | **PROVEN** |
| 2 | +8 | +12 | `00B3F0A3 mov [ebx+12], edx` | **PROVEN** |
| 3 | +12 | +32 | `00B3F0F8 mov [ebx+32], ecx` | **PROVEN** |
| 4 | +16 | +36 | `00B3F14D mov [ebx+36], eax` | **PROVEN** |
| 5 | +20 | +40 | `00B3F1A2 mov [ebx+40], edx` | **PROVEN** |
| 6 | +24 | +44 | `00B3F1F7 mov [ebx+44], ecx` | **PROVEN** |
| 7 | +28 | +8 | `00B3F24C mov [ebx+8], eax` | **PROVEN** |
| 8 | +32 | +16 | `00B3F2A1 mov [ebx+16], edx` | **PROVEN** |
| 9 | +36 | +20 | `00B3F2F6 mov [ebx+20], ecx` | **PROVEN** |
| 10 | +40 | +48 | `00B3F34B mov [ebx+48], eax` | **PROVEN** |
| 11 | +44 | +52 | `00B3F395 mov [ebx+52], edx` | **PROVEN** |
| 12 | +48 | +56 | `00B3F3F5 mov [ebx+56], ecx` | **PROVEN** |
| 13 | +52 | +60 | `00B3F44A mov [ebx+60], eax` | **PROVEN** |
| 14 | +56 | +24 | `00B3F49F mov [ebx+24], edx` | **PROVEN** |
| 15 | +60 | +28 | `00B3F4F4 mov [ebx+28], ecx` | **PROVEN** |
| — | +64…+84 | +64…+84 | `00B3F503 lea ecx,[ebx+64]` 24 B | **PROVEN** |
| 22 | +88 | +88 | `00B3F5A5 mov [ebx+88], edx` | **PROVEN** |

Dest spans at least **92 bytes**. Holes at dest `+8/+16/+20/+24/+28`
are filled late; dest `+32…+44` are filled early (stream 3–6).

C-terrain one-line map (`0→+0, 1→+4, 2→+12, 3→+32, … then
+8/+16/+20/+48…, 24 B at +64, last at +88`) is **PROVEN** and now
complete.

Resource-manager “copy dest+88 into slot+36 (24 bytes)” is
**DISPROVEN**. Both callers copy **dest+64**:

```
00B41E80  lea ecx, [esp+24]     ; dest
00B41E89  mov edx, [esp+88]     ; dest+64
00B41E91  lea ecx, [esi+36]

00B42606  lea ecx, [esp+68]
00B42647  mov ecx, [esp+132]    ; dest+64 = 68+64
00B42655  lea eax, [esi+36]
```

Six dwords → map `+36…+56`. **PROVEN.**

---

## File values vs dest (compiled WAD `.lev`)

`LevFormatTests.Header_is_version_25_with_format_constant_and_16_16_grid`
locks the **file** prefix on Lookout / Picnic / Guild door / Oakvale
East:

| file | value | dest |
|---|---|---|
| +0 | 25 | +0 |
| +4 | `0x1904` | +4 |
| +36 | `width << 16` | **+20** |
| +40 | `height << 16` | **+48** |
| +44 | `65536` (16.16 `1.0`) | **+52** |

C# `ReadHeader`:

```csharp
width  = u32(file+36) >> 16   // dest+20
height = u32(file+40) >> 16   // dest+48
cell   = u32(file+44) / 65536f // dest+52
```

`PeekMapHeader` reads **48** WAD bytes
(`LevelLibrary` / `LevFile.NativeHeaderBytes`). Native consumes
**92**. Comment on `LevHeader` (“`00B3EFA0` fields at 0 / 4 / 36 /
40 / 44”) treats dest as file. **DISPROVEN** as dest packing.
**PROVEN** as those five **file** dwords.

Material table at file **179** (`INVALID_THEME_STANDIN`, 255×132)
is **not** part of `00B3EFA0`. Open does not parse it. **PROVEN.**

File `+8…+32` and `+48…+88` (dest `+12 / +32…+44 / +8 / +16` and
the tail) are **UNREAD** as numbers in this repo. Do not invent
them as a second width.

---

## Miss-path compares (`00B42530`) ≠ C# width/height

```
00B4260F  movzx edx, [esi+92]
00B42613  cmp [esp+100], edx     ; dest+32 = file+12
00B4261D  movzx eax, [esi+94]
00B42621  cmp [esp+104], eax     ; dest+36 = file+16
00B4262B  movzx ecx, [esi+96]
00B4262F  cmp [esp+108], ecx     ; dest+40 = file+20
00B42639  movzx edx, [esi+98]
00B4263D  cmp [esp+112], edx     ; dest+44 = file+24
          jne 00B42723           ; reject, no patch
```

Map `+92/+94` size and `+96/+98` origin are the `u16`s
`00BF6F80` uses for patch AABB (`LandscapeFrustum.MapSizeXOffset`
= 92). Compared as **zero-extended u16 vs dest dword**.

That group is dest `+32…+44` = **file `+12…+24`**, not C#’s
16.16 width/height at file `+36/+40` (dest `+20/+48`).

If dest+32 were `width<<16` (Lookout `0x00800000`), `cmp` against
map size 128 would fail. So dest+32 is **not** the C# 16.16 field.
**PROVEN** as different slots. Whether file+12 equals the integer
grid size is **UNREAD** (WAD prefix not dumped here).

STB-hit `00B41E50` **skips** these four compares and still copies
dest+64. First-seen New Game miss never reaches either caller.

---

## First LEV after Leave

```
0042F2A2 Leave
00416953 Load world
  004A1840 → 0049DDD0 Data\Levels\FinalAlbion.stb
  00B23DC0 jmp 00B428E0
    00B42750(1)
      vtbl+12 MISS          ; TLC has FinalAlbion_RT.stb only
      no 00B420F0 / 00B41E50 / 00B42530
      no 00B3EFA0
```

**PROVEN** (`stb-first-open`, `terrain-first-draw`). Host
`PeekMapHeader("LookoutPoint")` / `LoadCompiledLev` at that moment
is **DIVERGE**: native patches stay closed (`[+424]==0`).

When a later hit runs, `00B420F0` wraps the intern
`__STATIC_MAP_COMMON_HEADER__` blob and `00B41E50` feeds **that**
stream to `00B3EFA0`, not a 48-byte WAD prefix. Whether those 92
bytes match WAD `.lev` `[0..92)` is **UNREAD**.

First-seen **playable** map after Leave/New Game is
`StartOakValeWest` (WLD `MapX 3456 / MapY 736`), not Lookout.
Lookout is the C# header fixture. Do not swap them.

---

## C# vs native — header

| C# | Native | Class |
|---|---|---|
| `NativeHeaderBytes = 48` | 16×u32 + 24 + u32 = **92** | **DISPROVEN** as `00B3EFA0` length |
| dest = file prefix | remap table above | **DISPROVEN** |
| width = `u32(file+36)>>16` | that dword is dest+20; compare uses dest+32 | **PARTIAL** (file number **PROVEN**; dest slot **DIVERGE**) |
| height = `u32(file+40)>>16` | dest+48 | same |
| cell = `u32(file+44)/65536` | dest+52 | **PROVEN** number; dest offset unread by C# |
| `PeekMapHeader` WAD 48 B | hit: intern stream; miss: full blob then 92 B | **DIVERGE** |
| no copy to map+36 | dest+64 → slot+36 (24 B) | **DISPROVEN** as implemented |
| `CurrentCompiledLev` null at `PresentWorld` | open is header + handles, not material/cell parse | **PROVEN** match at open |
| `LevHeaderVersion=25` / `0x1904` | dest+0 / dest+4 | **PROVEN** |

---

## Two cell layouts (do not conflate)

### A. WAD compiled `.lev` — 1 m theme slots

`LevCellGrid`: after sound themes, records of **21** bytes, tag
`u32==21`, count = `GridWidth*GridHeight` (Lookout 128×128 =
16384). Bytes 10–13 material-table slots (`0xFF` unused). `u16` at
+8 is a constant ~60, **not** height. **PROVEN** (`LevFormatTests`).

`00B3EFA0` / open / first DIP do **not** walk this table.
**PROVEN** absence.

### B. In-memory patch cells — 16 m draw records

`00BDC2D0` after AABB:

```
eax = [patch+16]  rows
[patch+12]        cols
index = row*cols + col
lea eax, [eax+eax*8]
lea ecx, [cells + eax*8]   ; stride 72
call 00BF4570
```

`LandscapeCells.RecordBytes = 72`. Lookout 128/16 → 8×8 if
`[+12]/[+16]` is metres/16 (**PARTIAL**, inferred).

`00BF4570` (`ecx` = cell):

| cell | use | Status |
|---|---|---|
| +60 bit `0x4` | required or `je` skip DIP | **PROVEN** `00BF4579` |
| +32 / +44 | AABB min/max; `add ebp,32` then `00BF3860` | **PROVEN** |
| +52 / +56 | IB / VB (via mesh nodes; host comment `cell+8`) | **PROVEN** as DIP source (`C-terrain`) |
| +68 / +70 | NumVerts / PrimitiveCount | **PROVEN** (prior) |
| GPU vert | 24 B after `00BFE050` | **PROVEN** |

Host `LevCell` (four material bytes + `Constant60`) is **not** this
record. Texture ids on FG come from the STB strip / name map
(`GROUND_*` → `LANDSCAPE_*`), not from walking 21-byte cells at
submit. Filling leftover 1 m PATH cells from the WAD table is
**DISPROVEN** (`PARITY`, `LevFormatTests`).

STB tessellation (36-byte directory, raw LZO, 15-byte verts, strip
`PrimitiveCount+2`) is the **stored** mesh those 72-byte cells
submit. Separate from both the 48-byte C# peek and the 21-byte
WAD grid.

---

## Classification table

| Claim | Status |
|---|---|
| `00B3EFA0` remaps 92 stream bytes onto dest (not memcpy 48) | **PROVEN** |
| Dest map in the table above | **PROVEN** |
| Callers copy dest+64 (24 B) to map+36 | **PROVEN** |
| Miss path compares dest+32…+44 to map+92…+98 as u16 | **PROVEN** |
| Those compare slots are C# width/height at file+36/+40 | **DISPROVEN** |
| C# `NativeHeaderBytes=48` equals `00B3EFA0` | **DISPROVEN** |
| File v25 / `0x1904` / `width<<16` @36 / `height<<16` @40 / 16.16 1.0 @44 | **PROVEN** (WAD tests) |
| First Leave/`00B428E0` parses a LEV | **DISPROVEN** (STB miss, no `00B3EFA0`) |
| First-seen map is LookoutPoint | **DISPROVEN** as New Game (`StartOakValeWest`) |
| WAD 21-byte cells are the 72-byte FG records | **DISPROVEN** |
| `00BDC2D0` cell stride 72, flag `+60` bit 4 | **PROVEN** |
| File+12…+32 numeric contents | **UNREAD** |
| Intern common-header 92 B == WAD `[0..92)` | **UNREAD** |
| Later native `_RT.stb` hit without cmdline flag | **UNREAD** (`stb-first-open`) |

Dumps: `listing-00b00000.txt` `00B3EFA0`, `listing-00b40000.txt`
`00B41E50` / `00B42530` / `00B42750`, `listing-00bc0000.txt`
`00BDC2D0` / `00BF4570` / `00BF6F80`,
`landscape-trace/parsemapheader-00b3efa0.md` (truncated),
`LevFile.cs`, `LevCellGrid.cs`, `LandscapeCell.cs`.
Prior: `C-terrain-static-map.md` leftover “dest↔file beyond version
and four u16s” is closed for **offsets**; file numbers at +12 still
open.

No production edit. Proposed C# (not done): `NativeHeaderBytes=92`;
document dest remap; peek dest+32 group separately from 16.16 at
file+36; do not treat 21-byte WAD cells as 72-byte draw cells.
