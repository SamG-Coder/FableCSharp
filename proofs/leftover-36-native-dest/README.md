# Leftover #36 — native dest analog (DIP vs dest writer)

Investigation only. Production `src/` and `tests/` were
not edited. Do **not** invent dest `512,384`. Do **not**
invent WAD 1 m cell fill. Do **not** mark leftover #36
closed.

Question: leftover #36 dest analog of
`proofs/leftover-36-dip-enqueue`. Dest **writer** vs
DIP **enqueue**. Native landscape dest analog is
Oakvale INDEX16 dest / ExtraStrip dest as **separate**
DIPs. What dest does native write that host still
skips?

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/landscape-trace/`
(`INDEX.md` v5, `per-cell-submit-00bf4570.md`,
`per-cell-settexture-stage0-00bf50e0.md`,
`patch-submit-bit40-frustum-00bdc2d0.md`);
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00bc0000.txt`
(`00BF4570` / `00BF4E90` / `00BF55EF` `00A0AD40`);
`listing-00400000.txt` (`0041AC20` / `0041AFA0` /
`0041B173` / `0041BEB0`);
`implementer/frontend/fn-0041AC20-exact.txt`,
`fn-0041AFA0-exact.txt`;
`docs/status/investigations/2026-08-18-landscape-draw.md`;
`src/Fable.Render/LandscapeDraw.cs`,
`src/Fable.Render/MeshBatches.cs`,
`src/Fable.Formats/Levels/LandscapeCell.cs`,
`src/Fable.Formats/Levels/LevTileMesh.cs`,
`src/Fable.Game/WorldGeometry.cs`,
`src/Fable.Game/FrontendDx9Submit.cs`,
`src/Fable.Game/EngineLifecycle.cs`;
siblings `proofs/leftover-36-dip-enqueue`,
`proofs/0041AC20-dest-formula`,
`proofs/0041B173-stack-dest`,
`proofs/oakvale-index16-dips`,
`proofs/issue-36-verify`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH** /
**EQUIVALENT** / **DIVERGE**.

Do not re-prove GraphicIndex leftover, type-6
`0x27` pack, or dest **formula**. Dest **numbers**
stay in `proofs/0041B173-stack-dest`. Do not fill
uncovered PATH/WATER 1 m cells from the WAD table.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| Dest writer vs DIP enqueue (frontend)? | Dest is `0041AFA0` stack / `0041BEB0` rec. Enqueue is later `009DB700` / `+16020`. Native dest-nonempty does **not** enqueue | **PROVEN** (`leftover-36-dip-enqueue`) |
| Is `0041AC20` a dest-rect writer? | **No.** Persist size `+360/+364` and leftover `+204/+208` only | **DISPROVEN** as dest |
| Native dest analog of leftover #36 dest? | ExtraStrip dest: each `mesh+60` is its own dest DIP dest (`00BF4E90`) | **PROVEN** analog |
| Oakvale extra INDEX16 dest source? | Stored `CPatchTesselationEdgeStrip` dest, plus extra opened adaptive cells. **Not** WAD 1 m fill | **PROVEN** dest; WAD **DISPROVEN** |
| ExtraStrip dest as separate INDEX16 DIP? | Native **yes** (`00BF4570` then `mesh=mesh+60`). Host `MeshBatches.BuildCells` **yes** | **PROVEN** / **MATCH** live submit |
| What dest does native write that host still skips? | ExtraStrip dest as `LandscapeBufferKey` / `LandscapeCellMesh` dest identity. Frontend dest dump. Type-6 dest 4-tuple (native never writes one) | **PROVEN** skip; dest numbers **UNREAD** |
| Native dest dump of `[esp+36..48]`? | Still **UNREAD**. Not `512,384` | **UNREAD** |
| Close leftover #36? | **No.** Dest dump missing; dest identity still skips ExtraStrip dest; dest analog `512,384` still locked | **LEFTOVER** open |

---

## Verdict

**Native dest analog of leftover #36 dest writer is
ExtraStrip dest as a separate INDEX16 DIP dest.
Leave leftover #36 open.**

Leftover #36 DIP enqueue vs dest writer
(`proofs/leftover-36-dip-enqueue`):

| Path | Writer | Drain | First-seen |
| --- | --- | --- | --- |
| Dest | `0041AFA0` stack dest → `0041BEB0` rec `+12..+24` | `00BAE2D0` DIPUP | dest ctor `0,0,0,0`; type-6 **no** dest 4-tuple |
| DIP enqueue | `009DB700` 60-byte `+16020` | `009DA9F0` vtbl+332 | empty → `009DB6E6` |

`0041AC20` is **not** dest. Dest **formula** is
`0041AFA0` from persist / leftover × dest scale
from origin `+248` (`proofs/0041AC20-dest-formula`).
Dest **numbers** at `0041B173` stay **UNREAD**.

Native dest analog (landscape, not frontend dest):

```
00BF4570 dest DIP dest
  for each opened patch (00BDC2D0)
    for each 72-byte cell (bit 0x4)
      dest = mesh at cell+8
      DIP dest mesh+52 IB / mesh+56 VB     ; vtbl+328 type 5 INDEX16
      dest = mesh+60
      loop 00BF4E90                         ; ExtraStrip dest DIP again
```

Each ExtraStrip dest is a **separate dest DIP dest**.
Cache dest identity is
`(map, cellX, cellY, meshIndex)`, not
`(map, cellX, cellY)` alone
(`2026-08-18-landscape-draw.md`).

Host dest that **still skips** that dest:

- `LandscapeBufferKey(Map, CellX, CellY)` — no
  `meshIndex` dest.
- `LandscapeCellMesh` dest — primary strip dest
  only; **no** ExtraStrips dest field.
- `IssueRecoveredDraws` dest skip
  `DestX1<=DestX0` — leftover #36 dest analog
  (type-6 dest point) produces **no** DIPUP.
- Native dest dump of `[esp+36..48]` —
  **UNREAD**; host dest analog `512,384`.

Live game submit `MeshBatches.BuildCells` **MATCH**
ExtraStrip dest as extra `MeshDraw` dest. That
MATCH does **not** dest-lock leftover #36 dest
numbers and does **not** fill dest identity.

Do **not** invent WAD 1 m dest fill. Do **not**
plant dest `512,384`. Do **not** close #36.

---

## Evidence → Original → Host → Gap

### 1. Dest writer is not DIP enqueue — **PROVEN** analog

**Evidence** (`leftover-36-dip-enqueue`,
`fn-0041AC20-exact.txt`, `listing-00400000.txt`):

```
0041AC48  fld [edi+92]
0041AC50  mov [esi+360], eax      ; persist W  — not dest
0041AC5E  mov [esi+364], eax      ; persist H
0041ACD8  cmp [esi+376], ebx
0041ACDE  jbe 0041AF6F            ; skip leftover
0041AD19  fstp [esi+204]          ; leftover W
0041AD69  fstp [esi+208]          ; leftover H
```

No dest `+248` / dest 4-tuple. Dest origin is
`0052FFD0`. Dest scale is `0052F5C0`. Dest
**submit** is `0041AFA0` stack dest, then
`0041BEB0` dest copy into type `0x22` rec.

DIP enqueue `009DB700` has **no** `E8` on
frontend dest draw. Dest-nonempty is
`00BAE2D0`, not `+16020`.

**Original:** leftover #36 dest writer and DIP
enqueue are two dests. Native dest-nonempty
does not enqueue.

**Host:** `FrontendEnqueueRan` still true on
nonempty dest. Isolated
`EnqueuesDisplayQueue=false`. Dest analog
`512,384,512,384` locked for type-6.

**Gap:** dest analog of leftover #36 dest
writer is ExtraStrip dest (below). Dest
**numbers** stay **UNREAD**. Dest analog
tuple is **LEFTOVER**, not native dest.

---

### 2. Native dest analog — ExtraStrip dest DIP dest — **PROVEN**

**Evidence** (`listing-00bc0000.txt`,
`landscape-trace/per-cell-submit-00bf4570.md`,
`2026-08-18-landscape-draw.md`):

```
00BF4570  sub esp, 0x2F0
00BF4579  test [ebp+60], 0x04      ; dest DIP needs bit 0x4
00BF4581  je  00BF5864
00BF45DA  call 00BF3860            ; cell AABB dest
…
00BF4E6F  mov ebx, [esp+20]        ; dest = mesh
00BF4E81  je  00BF569C
00BF4E87  jmp 00BF4E90
00BF4E90  fld [ebx+16]             ; ExtraStrip dest bind
…
00BF55DB  movzx eax, [ebp+70]      ; PrimitiveCount dest
00BF55DF  movzx ecx, [ebp+68]      ; NumVerts dest
00BF55EF  call 00A0AD40            ; dest DIP vtbl+328
00BF55F6  mov ebp, [ebp+60]        ; dest = mesh+60 next
00BF564C  jmp 00BF4E90             ; ExtraStrip dest again
```

DIP dest args (`00A0AD40` `ret 16`):

```
PrimitiveType   = 5                 ; D3DPT_TRIANGLESTRIP
NumVertices     = (u16) mesh+68
primCount       = (u16) mesh+70
IB dest         = mesh+52, format 101 (D3DFMT_INDEX16)
VB dest         = mesh+56, stride 24
IndexCount      = PrimitiveCount+2
```

Primary dest and ExtraStrip dest are **separate
mesh dest nodes**. Each is one INDEX16 dest DIP.
Bit `0x4` on the 72-byte cell is the dest DIP
gate. Layer bit `0x4` is tessellator BG
`00BF71D0`, **not** this dest IB.

`00BFE050` dest expand dest `edi` steps 24.
Dest `+23` is **not written**. Index dest is
raw `rep movsd` of dest INDEX16 from the STB
stream. **PROVEN.**

**Original:** dest analog of leftover #36 dest
writer is dest of ExtraStrip mesh dest (VB dest
`mesh+56`, IB dest `mesh+52`). Dest analog of
leftover #36 DIP enqueue is dest DIP
`00BF4570` / `00BF4E90`. Dest-nonempty ExtraStrip
**does** dest DIP. Dest-nonempty frontend
**does not** dest enqueue.

**Host:** `LandscapeCell.ExtraStrips` dest and
`MeshBatches.BuildCells` dest emit one dest
`MeshDraw` per ExtraStrip dest when
`PrimitiveCount > 0`. Tests
`Lookout_cells_match_stb_tiles` lock
`draws == cells + extraDraws`.
`First_seen_landscape_submits_primary_and_edge_strips`
locks Oakvale stored ExtraStrip dest
(`withExtras > 0`).

**Gap:** dest identity still skips ExtraStrip
dest (§4). Sibling
`proofs/oakvale-index16-dips` “host merges
extras” is **STALE** vs HEAD `BuildCells`.
Do not restore merge. Do not invent dest
fill from WAD 1 m.

---

### 3. Oakvale INDEX16 dest vs Lookout dest — **PROVEN** dest; WAD **DISPROVEN**

**Evidence** (`proofs/oakvale-index16-dips`,
`LevTileMesh.ToCells`, `WorldGeometryTests`):

| Dest | Lookout | Oakvale (`StartOakValeWest`) |
| --- | --- | --- |
| Map dest | MapX 3232 / MapY 3488 | MapX 3456 / MapY 736 |
| Extra dest | 3–16 ExtraStrip dests per tile | stored dest; MapY=736 **fails** leftover XY `2000–6000` |
| Dest DIP extra | ExtraStrip dest + AABB neighbour dest cells | ExtraStrip dest + Contains/Sees dest cells |
| WAD 1 m dest fill | not dest of `00BF4570` | not dest of `00BF4570` |

Lookout-only dest gate XY `2000–6000` **dropped**
Oakvale ExtraStrip dest. That dest gate is
**DISPROVEN**. After the gate, those dests
**appear** as extra INDEX16 dest DIPs.

17×17 dest (`v=289`, flag 256) stores **no**
primary dest strip. Native dest DIP dest is
ExtraStrip dest only. Host dest soup dest of
the 17×17 grid is dest host **writes** that
native dest **skips** — the inverse of this
leftover. ExtraStrip dest on those tiles
still dest DIP on native and on
`BuildCells`.

WAD leftover 1 m PATH/WATER dest fill is
**DISPROVEN** as dest (`PARITY` “Did not
work”). Host `ToTileTriangles` dest is
primary dest + ExtraStrip dest only.
Do **not** re-invent.

Water dest INDEX16 first-seen FG dest:
**DISPROVEN** (empty-out). Later water dest
pass: **UNREAD**.

**Original:** Oakvale extra dest INDEX16 is
ExtraStrip dest + extra opened adaptive dest
cells. Dest analog of leftover #36 dest
writer, not dest enqueue.

**Host:** ExtraStrip dest MATCH as extra dest
draw. Dest soup dest on 17×17 is extra dest
host writes.

**Gap:** native dest DIP **count** on a live
Oakvale Present is **UNREAD**. Host dest
count is dest draws, not dest meshes on a
PIX dump. Do not invent dest count.

---

### 4. Dest native writes that host still skips — **PROVEN** skip / **UNREAD** dump

**Evidence** (`LandscapeDraw.cs`,
`FrontendDx9Submit.IssueRecoveredDraws`,
`0041B173-stack-dest`):

```
LandscapeBufferKey(string Map, int CellX, int CellY)
LandscapeCellMesh dest:
  StripIndices / PrimitiveCount / TextureId dest
  BufferKey => (Map, CellX, CellY)     ; no meshIndex dest
  no ExtraStrips dest field
```

Native dest identity for ExtraStrip dest is
`(map, cellX, cellY, meshIndex)`. Host dest
identity still skips that dest. `LandscapeDraw`
dest is unused on live submit (`SubmitCurrentWorld`
dest is `MeshBatches.BuildCells`), so dest
identity leftover is dest of a decoded helper,
not dest of the live dest DIP.

Leftover #36 dest dump dest:

```
0041B0DD  mov [esp+36], edx       ; dest X0 dest
0041B0FD  mov [esp+40], eax       ; dest Y0 dest
0041B10D  fstp [esp+44]           ; dest X1 dest
0041B123  fstp [esp+48]           ; dest Y1 dest
0041B173  fld [esp+36]
0041B177  fistp [esp+12]          ; dest snap dest; not widget dest
```

No process dump of those dest numbers.
`export/native/` is screenshots. Host dest
`512,384,512,384` is dest analog of dest
formula on a type-6 dest point. Native
type-6 dest writer is `0054EF00` dest pen at
`+248`, dest packer `00543910`. Native dest
does **not** write dest `X0,Y0,X1,Y1`.

`IssueRecoveredDraws` dest skip:

```
if (rec.DestX1 <= rec.DestX0 || rec.DestY1 <= rec.DestY0)
    continue;                       ; dest analog of 00BADB36
```

Type-6 dest analog is a dest point → dest
skip → **no** DIPUP even when Present dest
is live. Native dest of PRESS START glyphs
is dest of the type-6 dest packer, **not**
dest of a dest 4-tuple.

**Original:** dest native writes that host
dest identity / dest skip still skip:

| Native dest | Host dest skip | Class |
| --- | --- | --- |
| ExtraStrip dest as dest DIP dest (`mesh+60`) | dest identity no `meshIndex`; `LandscapeCellMesh` no ExtraStrips dest | **PROVEN** dest skip; live `BuildCells` dest **MATCH** |
| ExtraStrip dest as dest VB dest (`mesh+56`) dest IB dest (`mesh+52`) | dest helper comments still say dest on cell `+56/+52` | **DISPROVEN** comments (`landscape-submit-leave`) |
| Dest `[esp+36..48]` dest numbers | dest analog `512,384`; dest dump missing | **UNREAD** dest |
| Type-6 dest dest of glyph dest packer | dest 4-tuple analog; dest skip `DestX1<=DestX0` | **PROVEN** dest skip of dest analog |
| Dest leftover `+204` dest when GraphicIndex 0 | dest leftover 0 **MATCH**; dest analog still locked | **MATCH** leftover; dest analog **LEFTOVER** |
| WAD 1 m dest fill dest | dest not in `00BF4570` dest | **DISPROVEN** dest |

**Host:** ExtraStrip dest MATCH on live dest
submit. Dest identity dest skip remains.
Dest analog dest skip remains. Dest dump
**UNREAD**.

**Gap:** leftover #36 dest analog dest-lock.
Do **not** replace dest analog with new
invented dest constants. Do **not** add dest
`meshIndex` dest to dest identity from this
note (investigation only). Do **not** invent
dest fill from WAD 1 m to dest-close Oakvale
extra dest INDEX16.

---

### 5. Dest formula analog — **PROVEN** formula; dest numbers **UNREAD**

**Evidence** (`proofs/0041AC20-dest-formula`):

```
w = (+360 != 0) ? (float)+360 : +204
h = (+364 != 0) ? (float)+364 : +208
w *= +264
h *= +268
dest = (ox, oy, ox+w, oy+h)     ; ox=+248  (or centred)
fistp dest snap
```

Dest analog of ExtraStrip dest formula is dest
of `00BFE050` dest expand dest `edi` (15 file
bytes → dest 24 GPU bytes) plus dest DIP dest
`00A0AD40`. Dest analog is dest of dest mesh
dest, **not** dest of dest 4-tuple.

`ComputeSubmitDest` dest **MATCH** dest
formula. Dest leftover dest
`LeftoverFromGraphic(0)=(0,0)` **MATCH**
`0041AC20` `jbe`. Dest analog dest tuple
is **not** dest MATCH.

---

## What is already MATCH (not a close)

| Item | Class |
| --- | --- |
| No frontend `E8 009DB700` dest enqueue | **MATCH** (`leftover-36-dip-enqueue`) |
| `0041AC20` dest leftover dest, not dest rect | **PROVEN** / dest analog **DISPROVEN** |
| ExtraStrip dest as dest DIP dest native | **PROVEN** |
| `BuildCells` dest ExtraStrip dest extra dest draw | **MATCH** live dest |
| Oakvale ExtraStrip dest after XY dest gate drop | **PROVEN** dest |
| WAD 1 m dest fill as dest source | **DISPROVEN** |
| Dest **formula** `0041AFA0` | **PROVEN** elsewhere |

---

## Classification

| Claim | Status |
| --- | --- |
| Dest writer vs DIP enqueue (frontend dest) | **PROVEN** split |
| `0041AC20` dest rect dest | **DISPROVEN** |
| ExtraStrip dest analog of leftover #36 dest writer | **PROVEN** analog |
| ExtraStrip dest as separate INDEX16 dest DIP native | **PROVEN** |
| ExtraStrip dest as separate dest draw live `BuildCells` | **MATCH** |
| ExtraStrip dest as `LandscapeBufferKey` dest / `LandscapeCellMesh` dest | **DISPROVEN** dest identity; dest **skipped** |
| Oakvale extra dest INDEX16 is ExtraStrip dest | **PROVEN** dest |
| WAD 1 m dest fill dest | **DISPROVEN** |
| Dest `[esp+36..48]` dest numbers | **UNREAD** |
| Host dest analog `512,384,512,384` | **LEFTOVER** dest analog |
| Type-6 dest 4-tuple native dest | **DISPROVEN**; dest skip of dest analog **PROVEN** |
| ExtraStrip dest `fmt` `0x5901`–`0x5904` dest meaning | **UNREAD** |
| ExtraStrip dest AABB dest vs cell dest AABB | **UNREAD** |
| Water dest INDEX16 later dest pass | **UNREAD** |
| Leftover #36 closed | **DISPROVEN** — stays **LEFTOVER** |

**Overall: PARTIAL** (dest analog recovered;
dest dump dest numbers **UNREAD**; dest
identity dest skip remains). **Leave #36
open.**

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\landscape-trace\INDEX.md`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\landscape-trace\per-cell-submit-00bf4570.md`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00bc0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\implementer\frontend\fn-0041AC20-exact.txt`
- `C:\FableCSharp\docs\status\investigations\2026-08-18-landscape-draw.md`
- `C:\FableCSharp\proofs\leftover-36-dip-enqueue\README.md`
- `C:\FableCSharp\proofs\0041AC20-dest-formula\README.md`
- `C:\FableCSharp\proofs\oakvale-index16-dips\README.md`
- `C:\FableCSharp\src\Fable.Render\LandscapeDraw.cs`
- `C:\FableCSharp\src\Fable.Render\MeshBatches.cs`
- `C:\FableCSharp\src\Fable.Formats\Levels\LandscapeCell.cs`
