# Leftover #36 — dest dump of `[esp+36..48]` after `0041B173`

Investigation only. Production `src/` and `tests/` were
not edited. Do **not** invent dest `512,384`. Do **not**
invent WAD 1 m cell fill. Do **not** mark leftover #36
closed.

Question: native dest 4-tuple dump of
`[esp+36],[esp+40],[esp+44],[esp+48]` after
`0041B173` snap? Listing immediates? Does host
skip ExtraStrip dest identity? If **MATCH**
ExtraStrip as a separate INDEX16 DIP, what
remains for leftover #36?

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
(`0041AFA0` / `0041B173` / `0041BEB0`);
`implementer/frontend/fn-0041AFA0-exact.txt`,
`fn-0041BEB0-exact.txt`, `fn-0054EF00-exact.txt`;
`export/native/` PNG chunk scan;
`src/Fable.Render/MeshBatches.cs`,
`src/Fable.Render/LandscapeDraw.cs`,
`src/Fable.Formats/Levels/LandscapeCell.cs`,
`src/Fable.Formats/Levels/LevTileMesh.cs`,
`src/Fable.Game/WorldGeometry.cs`,
`src/Fable.Game/FrontendDx9Submit.cs`,
`src/Fable.Game/EngineLifecycle.cs`;
siblings `proofs/leftover-36-native-dest`,
`proofs/leftover-36-dip-enqueue`,
`proofs/0041B173-stack-dest`,
`proofs/oakvale-index16-dips`,
`proofs/0041AC20-dest-formula`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH** /
**STALE**.

Do not re-prove GraphicIndex leftover, type-6
`0x27` pack, dest **formula**, or DIP enqueue
`009DB700`. Dest **formula** stays in
`proofs/0041AC20-dest-formula`. Dest **writer**
vs enqueue stays in `proofs/leftover-36-dip-enqueue`.
Do not fill uncovered PATH/WATER 1 m cells from
the WAD table.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| Listing immediates of dest `X0,Y0,X1,Y1` at `0041B173`? | **None.** Snap of widget `+248/+252` origin, persist/leftover size, dest scale `+264/+268`. Centre uses `[0x122F59C]=0.5`, not dest numbers. No `push 0x200` / `0x180`, no `0x44000000` / `0x43C00000` | **DISPROVEN** immediates |
| Native dest dump of `[esp+36..48]` after `0041B173`? | **None.** No process dump, minidump, PIX/ETL. `export/native/` PNGs have **no** `tEXt` / `iTXt` dest tuples | **UNREAD** |
| Native dest 4-tuple recovered from listing? | **No.** Formula recovered elsewhere. Numbers stay unread | **UNREAD** |
| Type-6 `UI_PRESS_START_TEXT` writes that dest 4-tuple? | **No.** `0054EF00` pen at `+248`. Never reaches `0041B173` | **DISPROVEN** |
| Host skip ExtraStrip dest identity? | **Yes** on unused helper: `LandscapeBufferKey(Map, CellX, CellY)` has no `meshIndex`; `LandscapeCellMesh` has no ExtraStrips field. Live submit does **not** use that key | **PROVEN** skip of unused helper |
| ExtraStrip as separate INDEX16 DIP live? | Native **yes** (`00BF4570` then `mesh=mesh+60`). Host `MeshBatches.BuildCells` **yes**. `Lookout_cells_match_stb_tiles` locks `draws == cells + extraDraws` | **MATCH** live submit |
| Sibling `oakvale-index16-dips` “host merges extras”? | **STALE** vs HEAD `BuildCells` | **STALE** |
| If MATCH ExtraStrip DIP, what remains #36? | Dest dump **UNREAD**; dest analog 4-tuple lock; type-6 dest skip `DestX1<=DestX0`; enqueue name leftover; empty `+16020` stand-in; dest identity skip of unused helper | **LEFTOVER** open |
| WAD 1 m dest fill? | **Not** dest of `00BF4570` | **DISPROVEN** — do not fill |
| Close leftover #36? | **No.** Dest dump missing. ExtraStrip MATCH does not dest-lock numbers | **LEFTOVER** open |

---

## Verdict

**No native dest 4-tuple dump of `[esp+36..48]`
after `0041B173`. Listing immediates do not
exist. ExtraStrip dest as a separate INDEX16
DIP is MATCH on live `BuildCells`. Leave
leftover #36 open.**

`0041B173`…`0041B1AF` snaps stack dest back
onto the **stack**. It does not plant dest
numbers as immediates and does not write
widget `+248`. Recovering dest **formula**
does not recover dest **numbers**. Host dest
`512,384,512,384` is dest analog of that
formula on a type-6 point. Native type-6
never writes a dest 4-tuple.

Host **does** skip ExtraStrip dest identity
on `LandscapeBufferKey` / `LandscapeCellMesh`.
That skip is dest of a decoded helper that
live `SubmitCurrentWorld` does **not** use.
Live dest DIP is `MeshBatches.BuildCells`:
one `MeshDraw` per ExtraStrip with
`PrimitiveCount > 0`. That MATCH does **not**
close leftover #36 dest dump.

Do **not** invent dest `512,384`. Do **not**
fill WAD 1 m cells. Do **not** add
`meshIndex` dest identity from this note.

---

## Evidence → Original → Host → Gap

### 1. Listing immediates of dest 4-tuple — **DISPROVEN**

**Evidence** (`listing-00400000.txt`,
`fn-0041AFA0-exact.txt`):

```
0041B065  mov eax, [edi+360]
0041B06D  jne 0041B077
0041B06F  fld [edi+204]           ; size W leftover
0041B089  mov eax, [edi+364]
0041B091  jne 0041B09B
0041B093  fld [edi+208]           ; size H leftover
0041B0AD  mov eax, [edi+248]      ; origin X bits
0041B0B5  fmul [edi+264]          ; dest scale X
0041B0BB  mov ecx, [edi+252]
0041B0DD  mov [esp+36], edx       ; dest X0 = origin
0041B0FD  mov [esp+40], eax       ; dest Y0
0041B10D  fstp [esp+44]           ; dest X1
0041B123  fstp [esp+48]           ; dest Y1
0041B127  call [edx+424]          ; centre? else 0041B173
0041B12F  je 0041B173
0041B135  fmul [0x122F59C]        ; 0.5 half-size, not dest
…
0041B173  fld [esp+36]
0041B177  fistp [esp+12]          ; snap; not a widget store
0041B17B  fild [esp+12]
0041B17F  fstp [esp+36]
          … same for +40 / +44 / +48 …
0041B1AF  mov eax, [edi]
0041B1B3  fstp [esp+48]
```

`fn-0041AFA0-exact.txt` has **no** `push 0x200`,
`push 0x180`, `0x44000000` (512.0f), or
`0x43C00000` (384.0f). `listing-00400000.txt`
`push 0x2000000` at `00402D70` is unrelated.
Listing immediates at this site:

| Immediate | Meaning | Dest 4-tuple? |
| --- | --- | --- |
| `[0x122F59C]` | centre half (`0.5`) | **No** |
| `[0x122DCB4]` | unsigned `fild` bias | **No** |
| `0x22` at `0041BEB0` | record type | **No** |
| `0xC0` at `0041B503` | record size | **No** |

Later `0041B4E6 call 0041BEB0` copies a dest
**pointer** into type-`0x22` rec `+12..+24`
(`fn-0041BEB0-exact.txt`). That copy is still
not dest numbers.

Type-6 `0054EF00` (`fn-0054EF00-exact.txt`):
pen `+248/+252`, snap `[esp+40]`, packer
`00543910` type `0x27`. No `[esp+36..48]`
dest 4-tuple. No `0041AFA0`.

**Original:** dest 4-tuple exists only as
runtime stack floats after snap, then rec
`+12..+24` for type-0. Type-6 has **no**
dest 4-tuple field.

**Host:** tests lock dest analog
`(512,384,512,384)` on
`UI_PRESS_START_TEXT` and forest `410`
lattice. Those are formula analogs
(`320*(1024/640)`, `256*1.6`), **not**
listing immediates.

**Gap:** leftover #36 dest-lock. Do **not**
plant dest constants from this listing.

---

### 2. Dest dump of `[esp+36..48]` — **UNREAD**

**Evidence** — what a dest dump would have
to contain (any one would close the unread
site):

| Dump | Site | Status in repo |
| --- | --- | --- |
| Stack `[esp+36..48]` after `0041B1AF` | type-0 present | **UNREAD** — none |
| Type-`0x22` rec `+12,+16,+20,+24` | `0041BEB0` | **UNREAD** — none |
| Widget `+248/+252` after `005301B0` | layout origin | **UNREAD** — none |
| Debugger / minidump / PIX of first-seen | process | **UNREAD** — no `*.dmp` `*.pix` `*.etl` under `proofs/` `export/` `implementer/` |

`export/native/` PNG chunk scan (2026-08-20):

| File | Chunks | `tEXt` / `iTXt` dest |
| --- | --- | --- |
| `01-after-launch.png` | IHDR,sRGB,gAMA,pHYs,IDAT,IEND | **none** |
| `01-window.png` | same family | **none** |
| `02-skip-*.png` / `*-wnd.png` | same family | **none** |
| `03-menu-desktop.png` | same family | **none** |
| `03-menu-window.png` | same family | **none** |
| `Fable01.png` | IHDR,sRGB,gAMA,pHYs,IDAT×16,IEND | **none** |

Pixels are not `[esp+36..48]`. Binary
substring `esp` in compressed IDAT is
**not** a dest dump.

`export/frontend/press-start-dests.txt` is
a host walk (`LayoutFrontendWidgets`).
`implementer/frontend/17-press-start-frame.txt`
header: “not a screenshot”.

**Original:** dest **numbers** at first-seen
`0041B173` are **UNREAD**. Dest **formula**
is recovered (`proofs/0041AC20-dest-formula`).

**Host:** dest analog lock remains.

**Gap:** leftover #36 dest dump. Formula
recover does not close it. ExtraStrip MATCH
does not close it.

---

### 3. ExtraStrip dest identity skip — **PROVEN** skip of unused helper

**Evidence** (`LandscapeDraw.cs`,
`LandscapeCell.cs`,
`2026-08-18-landscape-draw.md`):

Native dest identity for ExtraStrip dest is
`(map, cellX, cellY, meshIndex)` because
`00BF4E90` DIPs each `mesh+60` node as its
own INDEX16 dest DIP.

Host dest identity still skips that dest:

```
LandscapeBufferKey(string Map, int CellX, int CellY)
LandscapeCellMesh dest:
  StripIndices / PrimitiveCount / TextureId dest
  BufferKey => (Map, CellX, CellY)     ; no meshIndex dest
  no ExtraStrips dest field
```

`LandscapeBufferKey` / `LandscapeCellMesh`
are defined only in `LandscapeDraw.cs`.
Grep of `src/` finds **no**
`new LandscapeCellMesh`. Live dest submit
is `SubmitCurrentWorld` →
`CollectVisibleCells` →
`MeshBatches.BuildCells`. Dest identity
leftover is dest of a decoded helper, not
dest of the live dest DIP.

`LandscapeCell` **does** keep ExtraStrips
(`LevTileMesh.ToCells`,
`WorldGeometry.OffsetExtraStrips`).
`BuildCells` walks that list.

**Original:** ExtraStrip dest is a separate
dest DIP dest (`mesh+60`). Cache dest
identity includes `meshIndex`.

**Host:** helper dest identity skips
ExtraStrip dest. Live dest DIP MATCH
ExtraStrip dest as extra `MeshDraw`.

**Gap:** dest identity skip remains on the
helper. Do **not** add `meshIndex` dest
from this dest-dump note. Dest identity
skip does **not** dest-lock leftover #36
dest numbers.

---

### 4. ExtraStrip as separate INDEX16 DIP — **MATCH** live submit

**Evidence** (`MeshBatches.BuildCells`,
`WorldGeometryTests.Lookout_cells_match_stb_tiles`,
`listing-00bc0000.txt` via
`proofs/leftover-36-native-dest`):

```
00BF4570 dest DIP dest
  for each opened patch (00BDC2D0)
    for each 72-byte cell (bit 0x4)
      dest = mesh at cell+8
      DIP dest mesh+52 IB / mesh+56 VB     ; vtbl+328 type 5 INDEX16
      dest = mesh+60
      loop 00BF4E90                         ; ExtraStrip dest DIP again
```

Host:

```
BuildCells(cells)
  for each cell
    EmitMesh primary strip dest
    for each ExtraStrip
      EmitMesh extra strip dest
```

`Lookout_cells_match_stb_tiles`:

```
extraDraws = ExtraStrips with PrimitiveCount > 0
Assert.Equal(visible.Count + extraDraws, mesh.Draws.Length)
```

Sibling `proofs/oakvale-index16-dips`
“host merges extras” / “one MeshDraw per
cell” is **STALE** vs HEAD `BuildCells`.
Do not restore merge. Do not invent dest
fill from WAD 1 m.

WAD leftover 1 m PATH/WATER dest fill is
**DISPROVEN** as dest of `00BF4570`
(`PARITY` “Did not work”). Host
`ToTileTriangles` dest is primary dest +
ExtraStrip dest only.

**Original:** ExtraStrip dest DIP dest is
the dest analog of leftover #36 dest
**writer**, not dest dump numbers.

**Host:** ExtraStrip dest MATCH as extra
dest draw. Dest dump still **UNREAD**.

**Gap:** MATCH ExtraStrip DIP is already
MATCH. It is **not** leftover #36 dest
dump.

---

### 5. If MATCH ExtraStrip DIP, what remains for leftover #36

ExtraStrip dest as a separate INDEX16 DIP
is **not** remaining dest skip on live
submit. Remaining leftover #36 dest:

| Remaining | Class | Why ExtraStrip MATCH does not close it |
| --- | --- | --- |
| Native `[esp+36..48]` after `0041B173` | **UNREAD** dest dump | Landscape DIP is not frontend dest snap |
| Native type-`0x22` rec `+12..+24` dest | **UNREAD** dest dump | same |
| Native widget `+248/+252` first-seen | **UNREAD** dest dump | same |
| Host dest analog 4-tuple on type-6 | **LEFTOVER** analog | native type-6 never writes dest `X0,Y0,X1,Y1`; pen `+248` |
| `IssueRecoveredDraws` skip `DestX1<=DestX0` | **PROVEN** dest skip of dest analog | type-6 dest analog is a dest point → no DIPUP |
| `FrontendEnqueueRan` on nonempty dest | **LEFTOVER** name | dest-nonempty is `00BAE2D0`, not `009DB700` |
| `DisplayFlushShouldDip(0, 0)` | **MATCH** first-seen empty; stand-in **LEFTOVER** | host never stores `[this+16020]` |
| ExtraStrip dest identity on `LandscapeBufferKey` | **PROVEN** skip of unused helper | live `BuildCells` already MATCH ExtraStrip dest |
| WAD 1 m dest fill | **DISPROVEN** | do not fill to “close” Oakvale extra dest INDEX16 |

Do **not** replace dest analog with new
invented dest constants. Do **not** treat
ExtraStrip MATCH as dest dump. Do **not**
close #36.

---

## What is already MATCH (not a close)

| Item | Class |
| --- | --- |
| Dest **formula** `0041AFA0` | **PROVEN** elsewhere |
| Listing dest immediates `512` / `384` | **DISPROVEN** |
| Type-6 dest 4-tuple native writer | **DISPROVEN** |
| ExtraStrip dest as dest DIP dest native | **PROVEN** |
| `BuildCells` dest ExtraStrip dest extra dest draw | **MATCH** live dest |
| `Lookout_cells_match_stb_tiles` `draws == cells + extraDraws` | **MATCH** |
| WAD 1 m dest fill as dest source | **DISPROVEN** |
| No frontend `E8 009DB700` dest enqueue | **MATCH** (`leftover-36-dip-enqueue`) |
| `export/native/` PNG `tEXt` dest tuples | **DISPROVEN** as dest dump |

---

## Classification

| Claim | Status |
| --- | --- |
| Listing immediates of dest 4-tuple at `0041B173` | **DISPROVEN** |
| Native dest dump of `[esp+36..48]` after snap | **UNREAD** |
| Native dest dump of rec `+12..+24` | **UNREAD** |
| Host dest analog `512,384,512,384` | **LEFTOVER** dest analog |
| Type-6 dest 4-tuple native dest | **DISPROVEN** |
| ExtraStrip dest identity on `LandscapeBufferKey` / `LandscapeCellMesh` | **DISPROVEN** dest identity; dest **skipped** |
| ExtraStrip dest identity skip is live dest DIP skip | **DISPROVEN** — helper unused |
| ExtraStrip dest as separate INDEX16 dest DIP native | **PROVEN** |
| ExtraStrip dest as separate dest draw live `BuildCells` | **MATCH** |
| `oakvale-index16-dips` host merge extras | **STALE** |
| WAD 1 m dest fill dest | **DISPROVEN** |
| ExtraStrip MATCH closes leftover #36 dest dump | **DISPROVEN** |
| Leftover #36 closed | **DISPROVEN** — stays **LEFTOVER** |

**Overall: PARTIAL** (listing immediates
**DISPROVEN**; ExtraStrip dest DIP **MATCH**;
dest dump dest numbers **UNREAD**; dest
identity dest skip remains on unused helper).
**Leave #36 open.**

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\implementer\frontend\fn-0041AFA0-exact.txt`
- `C:\FableCSharp\implementer\frontend\fn-0041BEB0-exact.txt`
- `C:\FableCSharp\implementer\frontend\fn-0054EF00-exact.txt`
- `C:\FableCSharp\export\native\`
- `C:\FableCSharp\export\frontend\press-start-dests.txt`
- `C:\FableCSharp\proofs\leftover-36-native-dest\README.md`
- `C:\FableCSharp\proofs\leftover-36-dip-enqueue\README.md`
- `C:\FableCSharp\proofs\0041B173-stack-dest\README.md`
- `C:\FableCSharp\proofs\0041AC20-dest-formula\README.md`
- `C:\FableCSharp\proofs\oakvale-index16-dips\README.md`
- `C:\FableCSharp\src\Fable.Render\MeshBatches.cs`
- `C:\FableCSharp\src\Fable.Render\LandscapeDraw.cs`
- `C:\FableCSharp\src\Fable.Formats\Levels\LandscapeCell.cs`
- `C:\FableCSharp\src\Fable.Game\FrontendDx9Submit.cs`
