# Leftover #36 — unused `LandscapeBufferKey` vs `meshIndex`

Investigation only. Production `src/` and `tests/` were
not edited. Do **not** invent dest `512,384`. Do **not**
invent WAD 1 m cell fill. Do **not** mark leftover #36
closed. Do **not** add `meshIndex` to the unused helper.

Question: unused helper `LandscapeBufferKey(Map, CellX,
CellY)` is missing `meshIndex`. Is adding `meshIndex`
to that key a **recovered** dest identity, or
**inventing** a key native does not use on the live
path? Live ExtraStrip INDEX16 DIP submit already
**MATCH**. Dest dump of `[esp+36..48]` stays **UNREAD**.

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00bc0000.txt`
(`00BF4570` / `00BF4E90` / `00BF55F6` `mesh+60` /
`00BFE050`);
`tools/Fable.ExeIndex/out/01-sections/landscape-trace/`
(`INDEX.md` v5, `per-cell-submit-00bf4570.md`);
`docs/status/investigations/2026-08-18-landscape-draw.md`,
`2026-08-18-vulkan.md`;
`src/Fable.Render/LandscapeDraw.cs`,
`src/Fable.Render/MeshBatches.cs`,
`src/Fable.Formats/Levels/LandscapeCell.cs`,
`src/Fable.Game/EngineLifecycle.cs`
(`SubmitCurrentWorld`);
siblings `proofs/leftover-36-dest-dump`,
`proofs/leftover-36-native-dest`,
`proofs/landscape-submit-leave`,
`proofs/oakvale-index16-dips`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH** /
**STALE**.

Do not re-prove dest dump, dest formula, GraphicIndex
leftover, type-6 `0x27` pack, or DIP enqueue
`009DB700`. Dest dump stays in
`proofs/leftover-36-dest-dump`. Dest writer vs
enqueue stays in `proofs/leftover-36-dip-enqueue`.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| Native live dest identity of ExtraStrip dest? | Mesh object `*` linked from cell `+8` via mesh `+60`. VB `mesh+56` / IB `mesh+52` already on that object (`00BFE050`). DIP walks the pointer | **PROVEN** listing |
| Native live dest lookup `(map, cellX, cellY, meshIndex)`? | **None.** No integer `meshIndex`. No hashmap of dest buffers at `00BF4570` | **DISPROVEN** as native dest key |
| Where did `(map, cellX, cellY, meshIndex)` come from? | `2026-08-18-landscape-draw.md` “**Suggested host records** (for the main agent; not added here)” | **PROVEN** as host suggestion, **not** recovered dest |
| Is `LandscapeBufferKey` used on live submit? | **No.** `SubmitCurrentWorld` → `CollectVisibleCells` → `MeshBatches.BuildCells`. No `new LandscapeCellMesh`. No test names the key | **PROVEN** unused |
| Adding `meshIndex` to unused `LandscapeBufferKey`? | **Inventing** a host cache key native does not use on the live path. Not recovered dest identity | **DISPROVEN** as recover |
| Live ExtraStrip INDEX16 DIP? | Native **yes** (`00BF4E90` loop). Host `BuildCells` **yes**. `draws == cells + extraDraws` | **MATCH** |
| If live MATCH, change unused helper? | **No.** Do not add `meshIndex`. Do not wire `LandscapeBufferKey` | **PROVEN** leave helper |
| Dest dump `[esp+36..48]` after `0041B173`? | Still **UNREAD**. Not `512,384` | **UNREAD** |
| Close leftover #36? | **No.** Dest dump missing. Unused-key skip is not dest dump | **LEFTOVER** open |

---

## Verdict

**Adding `meshIndex` to unused `LandscapeBufferKey`
is inventing a key native does not use on the live
path. Do not change the unused helper. Leave leftover
#36 open.**

Native dest identity is the `00BFE050` mesh **pointer**.
Ingest links `if (prev) prev+60 = mesh; else cell+8 =
mesh`. Submit binds that mesh’s IB/VB and
`mov ebp, [ebp+60]` / `jmp 00BF4E90`. There is no
dest cache keyed by map name, cell XY, or an ordinal
`meshIndex`.

`(mapName, cellX, cellY, meshIndex)` is a **suggested
host** persistent cache from
`2026-08-18-landscape-draw.md`. It was never added.
`LandscapeBufferKey(Map, CellX, CellY)` is the same
class of unused host record. Extending it with
`meshIndex` would still be a host Dictionary, not
the native dest walk.

Live ExtraStrip dest as a separate INDEX16 DIP is
already **MATCH** on `MeshBatches.BuildCells`. The
task is dest dump of `[esp+36..48]`, which stays
**UNREAD**. Do **not** plant dest `512,384`. Do
**not** fill WAD 1 m cells. Do **not** edit `src/`.

---

## Evidence → Original → Host → Gap

### 1. Native dest identity is mesh `*` — **PROVEN**

**Evidence** (`listing-00bc0000.txt`,
`2026-08-18-landscape-draw.md` §1–§3):

Ingest (`00BF3B60` / `00BF3E17` `00BFE050`):

```
if (prev) prev+60 = mesh; else cell+8 = mesh;
prev = mesh;
```

Submit (`00BF4570` / `00BF4E90`):

```
00BF4E6F  mov ebx, [esp+20]        ; dest = mesh
00BF4E90  fld [ebx+16]             ; ExtraStrip dest bind
…
00BF55DB  movzx eax, [ebp+70]      ; PrimitiveCount dest
00BF55DF  movzx ecx, [ebp+68]      ; NumVerts dest
00BF55EF  call 00A0AD40            ; dest DIP vtbl+328
00BF55F6  mov ebp, [ebp+60]        ; dest = mesh+60 next
00BF5640  cmp ebp, esi
00BF564C  jmp 00BF4E90             ; ExtraStrip dest again
```

Buffers live on the mesh (`00BFE050`):

| Off | Field | Class |
| --- | --- | --- |
| mesh `+52` | IB object `*` | **PROVEN** |
| mesh `+56` | VB object `*` | **PROVEN** |
| mesh `+60` | next mesh `*` | **PROVEN** |
| mesh `+68` / `+70` | NumVerts / PrimitiveCount | **PROVEN** |
| cell `+8` | first mesh `*` | **PROVEN** |
| cell `+56/+58` | origin metres | **PROVEN** — **not** VB |

No listing site hashes dest by map string, cell
column/row, or an integer `meshIndex`. Dest DIP
does not look up a cache; it uses the pointers
already on the mesh.

**Original:** ExtraStrip dest identity is the next
mesh object. Ordinal `meshIndex` is not a native
dest field.

**Host:** unused `LandscapeBufferKey(Map, CellX,
CellY)` is a host record. `meshIndex` would be a
second invented field.

**Gap:** siblings that call
`(map, cellX, cellY, meshIndex)` “native dest
identity” restated the **suggested host** cache
(`leftover-36-native-dest`, `oakvale-index16-dips`,
`leftover-36-dest-dump` §3). That restatement is
**STALE** vs the listing walk. Native dest identity
is mesh `*`.

---

### 2. `(map, cellX, cellY, meshIndex)` is a suggested host cache — **PROVEN** suggestion; **DISPROVEN** as recovered dest

**Evidence** (`2026-08-18-landscape-draw.md`):

```
### Suggested host records (for the main agent; not added here)

LandscapeDraw.cs can stay as a decoded helper if the comments
are fixed: VB/IB live on the mesh, bit 0x4 is not this DIP.
A persistent cache key is (mapName, cellX, cellY, meshIndex)
— not (map, cellX, cellY) alone, because extras are extra DIPs.
```

`2026-08-18-vulkan.md` “What the main agent wires”:

```
cells → LandscapeCellMesh persisted by LandscapeBufferKey.
Do not TessellateVisible dump-all into one List<MeshTriangle>.
```

That persist-by-key path was **not** wired.
`LandscapeBufferKey` exists only as a type and as
`LandscapeCellMesh.BufferKey`. Grep of `src/*.cs`
finds **no** `new LandscapeCellMesh` and **no**
`Dictionary<LandscapeBufferKey, …>`. Tests name
neither type.

Adding `meshIndex` would implement the **suggested**
host cache, not a listing dest key. Native already
persists dest VB/IB **on the mesh object**. A C#
tuple is not dest identity recovered from
`00BF4570`.

**Original:** suggested host cache to avoid soup.
Never a native dest lookup.

**Host:** unused helper still `(Map, CellX, CellY)`.
Live dest is `MeshDraw` ranges in `BuildCells`.

**Gap:** do **not** “recover” `meshIndex` onto the
unused key. That is dest invention.

---

### 3. Live submit already MATCH ExtraStrip dest DIP — **MATCH**

**Evidence** (`EngineLifecycle.SubmitCurrentWorld`,
`MeshBatches.BuildCells`,
`WorldGeometryTests.Lookout_cells_match_stb_tiles`):

```
SubmitCurrentWorld
  CollectVisibleCells
  MeshBatches.BuildCells(cells)
    EmitMesh primary strip dest
    foreach ExtraStrip
      EmitMesh extra strip dest
```

`Lookout_cells_match_stb_tiles` locks
`draws == cells + extraDraws` with
`extraDraws = ExtraStrips.Count(s => PrimitiveCount > 0)`.

Live dest does **not** go through
`LandscapeBufferKey`. ExtraStrip dest as a
separate INDEX16 DIP is **MATCH**. Sibling
`oakvale-index16-dips` “host merges extras” is
**STALE** vs HEAD `BuildCells`.

If live MATCH, do **not** change the unused helper.
Wiring `meshIndex` would not dest-lock leftover #36
dest dump and would not alter live dest DIP.

**Original:** dest DIP dest is mesh `+60` walk.

**Host:** dest DIP dest is extra `MeshDraw`. Unused
key is unused.

**Gap:** leftover #36 dest dump, not dest identity
of an unused helper.

---

### 4. Unused helper skip is not leftover #36 dest dump — **PROVEN** split

**Evidence** (`proofs/leftover-36-dest-dump`):

Leftover #36 dest dump is native numbers of
`[esp+36..48]` after `0041B173`. Listing
immediates **DISPROVEN**. PNG `tEXt` dest
**DISPROVEN**. Numbers **UNREAD**. Do not plant
`512,384`.

`LandscapeBufferKey` missing `meshIndex` is dest
of a decoded helper that live Present does not
call. It is **not** dest dump. Treating that skip
as leftover #36 dest-lock oversells a host cache
that native does not use.

Remaining leftover #36 dest (unchanged):

| Remaining | Class |
| --- | --- |
| Native `[esp+36..48]` after `0041B173` | **UNREAD** dest dump |
| Native type-`0x22` rec `+12..+24` dest | **UNREAD** dest dump |
| Host dest analog 4-tuple on type-6 | **LEFTOVER** analog |
| `IssueRecoveredDraws` skip `DestX1<=DestX0` | **PROVEN** dest skip of dest analog |
| `FrontendEnqueueRan` on nonempty dest | **LEFTOVER** name |
| `DisplayFlushShouldDip(0, 0)` stand-in | **LEFTOVER** |
| Unused `LandscapeBufferKey` without `meshIndex` | **not** dest dump; **do not** patch |
| WAD 1 m dest fill | **DISPROVEN** |

**Original:** dest dump is frontend stack dest.

**Host:** ExtraStrip DIP MATCH. Dest analog lock
remains. Unused helper stays unused.

**Gap:** dest dump **UNREAD**. Do not close #36
from this key note.

---

## What is already MATCH (not a close)

| Item | Class |
| --- | --- |
| ExtraStrip dest DIP native `mesh+60` | **PROVEN** |
| `BuildCells` extra dest draw | **MATCH** live dest |
| Native dest identity is mesh `*` | **PROVEN** |
| Native dest key `(map, cellX, cellY, meshIndex)` | **DISPROVEN** |
| `LandscapeBufferKey` live persist | **DISPROVEN** — unused |
| Adding `meshIndex` as dest recover | **DISPROVEN** — invented host cache |
| Dest dump `[esp+36..48]` | **UNREAD** |
| WAD 1 m dest fill | **DISPROVEN** |

---

## Classification

| Claim | Status |
| --- | --- |
| Native ExtraStrip dest identity is mesh `*` / `mesh+60` | **PROVEN** |
| Native dest lookup `meshIndex` | **DISPROVEN** |
| `(map, cellX, cellY, meshIndex)` recovered from listing | **DISPROVEN** — suggested host cache |
| `LandscapeBufferKey` used on live Present | **DISPROVEN** |
| Adding `meshIndex` to unused helper is dest recover | **DISPROVEN** — dest invention |
| Live ExtraStrip INDEX16 DIP | **MATCH** |
| Change unused helper because key lacks `meshIndex` | **DISPROVEN** — leave helper |
| Dest dump `[esp+36..48]` | **UNREAD** |
| Host dest analog `512,384` | **LEFTOVER** analog — do not plant |
| Leftover #36 closed | **DISPROVEN** — stays **LEFTOVER** |

**Overall: PARTIAL** (native dest identity is
mesh `*`; unused `meshIndex` key is invented;
live ExtraStrip DIP **MATCH**; dest dump
**UNREAD**). **Leave #36 open. Prefer no `src/`.**

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00bc0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\landscape-trace\INDEX.md`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\landscape-trace\per-cell-submit-00bf4570.md`
- `C:\FableCSharp\docs\status\investigations\2026-08-18-landscape-draw.md`
- `C:\FableCSharp\docs\status\investigations\2026-08-18-vulkan.md`
- `C:\FableCSharp\proofs\leftover-36-dest-dump\README.md`
- `C:\FableCSharp\proofs\leftover-36-native-dest\README.md`
- `C:\FableCSharp\proofs\landscape-submit-leave\README.md`
- `C:\FableCSharp\src\Fable.Render\LandscapeDraw.cs`
- `C:\FableCSharp\src\Fable.Render\MeshBatches.cs`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
