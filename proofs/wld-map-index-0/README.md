# WLD `NewMap 1` LookoutPoint: native slot 1 vs C# `Maps[0]`

Investigation only. No production `src/` edits.

Do **not** treat C# `Maps[0]` as native index 0.
Do **not** start at Oakvale / `00DBDE40`. First authored map
is LookoutPoint.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**MISMATCH** / **LEFTOVER**.

Question: WLD `NewMap 1` LookoutPoint — native map index **1**
vs C# `Maps[0]`. Dummy slot 0? First `004FDBC0` uses which
index? Host `GlobalThingMapsLoaded` order.

Authority: dump `00507C30` `NewMap` / `EndMap`;
`src/Fable.Formats/Wld/WorldFile.cs`;
`proofs/tng-first-after-leave`, `proofs/wld-00507C30-switch`.

Sources: `listing-00500000.txt` (`005066E0` / `00507C30` /
`0050833F` / `00515AD0`);
`listing-004c0000.txt` (`004FDBC0` / `004FBF60` / `004FDDE0`);
TLC `FinalAlbion.wld`;
`EngineLifecycle.LoadGlobalThingsFile`;
`TlcInstallTests.World_starts_at_lookout_point`;
`WorldSceneTests` (`Maps.Count==398`).

---

## Verdict

**Same map, two numberings.** Native table index **1**.
C# list index **0** with `WorldMap.Index==1`.

Dummy slot **0** exists on the **72-byte map vector**, not only
on the 88-byte region table. First `004FDBC0` pushes **ebx=1**
(`LookoutPoint.tng`). Host `GlobalThingMapsLoaded` first
increment is the same file (`Maps[0]`), because the C# list
has no dummy row.

| Claim | Class |
|---|---|
| `NewMap 1` writes native **slot 1** (stride 72) | **PROVEN** |
| C# `Maps[0]` is that row (`Index==1`, `LookoutPoint`) | **PROVEN** |
| Native map index 0 **is** LookoutPoint | **DISPROVEN** |
| Dummy 72-byte map slot 0 from `005066E0` `00515AD0(1)` | **PROVEN** |
| Dummy 0 is *only* the 88-byte region row | **DISPROVEN** (`wld-00507C30-switch` leftover) |
| First `004FDBC0` index is **1** | **PROVEN** |
| First `004FDBC0` index is 0 / `Maps[0]` as a native slot | **DISPROVEN** |
| Host first global TNG is LookoutPoint | **PROVEN** (`foreach World.Maps`) |
| Host inserts a dummy `Maps[0]` | **DISPROVEN** |
| Native map-vector length after parse is **399** (0..398) | **PROVEN** (grow to `N+1`) |
| C# `Maps.Count` is **398** | **PROVEN** |

---

## Path (no-save, this table only)

```
00418784  Init World
  004A6EC0  005066E0  CWorldMap ctor
    push 1; lea ecx, [esi+32]; call 00515AD0
      allocate 1×72; size=1                  ← dummy MAP slot 0
    push 88; [esi+44] one region row         ← dummy REGION slot 0
    [esi+156]=0  current region = dummy
00416953  Loading world
  00507C30  Load .wld file
    NewMap 1  009BA540 → [esp+36]=1
    EndMap    eax=1; skip if 0
              grow [+32] to 2; write slot ebx=1
              LookoutPoint  [slot+36]=1  [slot+40]=prox
    NewMap 2…398  same; size ends 399
    NewRegion 1  0051D200 append +44          // file N discarded
    EOF
    00509859  Load global things
      [0x13B8609]==0 → 004FDBC0
        ebx=1; edi=72
        first 004FBF60(ebx=1)                ← this question
          slot 1 +24 script → LookoutPoint.tng
```

---

## Dump `00507C30` `NewMap` / `EndMap`

Empty intern `00507F0A` and `004115A0` `0050833F` share the
same body.

```
0050833F  push "NewMap"
          004115A0
          "Loading maps"
          009BA540 → [esp+36]               // file integer N

00508395  push "EndMap"
005083A9  mov eax, [esp+36]
005083AD  test eax, eax
005083AF  je 0050933B                       // NewMap 0: no write
          count = ([+36]−[+32]) / 72        // magic 0x38E38E39
          ebx = N
          edi = N+1
          if count < N+1: 0051BE20(N+1)     // grow
          ebp = begin + N*72                // lea edx,[ebx+ebx*8]; *8
          copy script to [ebp+24]
          [ebp+40] = LoadedOnPlayerProximity
          [ebp+36] = 1                      // filled
          [ebp+64] = IsSea
          004FCA50  overlap test
          [esp+36] = 0                      // clear pending N
```

`NewMap N` is a **sparse table index**, not an append.
`FinalAlbion.wld` uses `1…398` with no gaps, so slot `i`
is `NewMap i`.

Ctor dummy is **not** overwritten: `N=0` is rejected, and
the file never emits `NewMap 0`.

### Dummy map slot 0 (`005066E0` / `00515AD0` / `004FDDE0`)

```
00506738  push 1
0050673A  lea ecx, [esi+32]                 // map vector
00506740  call 00515AD0
            edi=1
            alloc 72
            [begin, end, cap] = [p, p+72, p+72]
            005110B0 copy of 004FDDE0 default
```

Default 72-byte record (`004FDDE0`):

```
[+24] empty intern
[+36] = 0                                   // not filled
[+37..39] = 0
[+40] = 1                                   // prox TRUE default
[+64] = 0                                   // IsSea
```

`004FDBC0` tests `[slot+36]` first. Dummy fails even if
`[+40]` is 1. Starting `ebx=1` is therefore belt-and-braces
with 1-based `NewMap`, not the only skip.

Region dummy is a **different** vector (`[+44]`, stride 88,
one row from `006BC410`). Both tables have an index-0 dummy.
Sibling `wld-00507C30-switch` (“dummy slot 0 is the 88-byte
region row, **not** this 72-byte map table”) is **DISPROVEN**
as a map-table claim.

`NewRegion` **appends** (`0051D200` on `+44`) and **discards**
`009BA540`. File `1,2,3…` happens to equal native index after
the dummy. Maps do **not** append.

---

## First `004FDBC0` index

```
004FDBC0  ecx = CWorldMap
          count = ([+36]−[+32]) / 72
004FDBDE  mov ebx, 1
          if count <= 1: ret                // dummy only
004FDBF2  mov edi, 0x48                     // 72
loop:
          slot = begin + edi                // first: begin+72 = index 1
          if [slot+36] && [slot+40]:
            push ebx
            call 004FBF60                   // first: ebx=1
          inc ebx
          add edi, 72
          cmp ebx, count
          jb loop                           // ebx = 1 .. count-1
```

After 398 `EndMap`s, `count=399`, loop visits slots **1..398**.

`004FBF60(esi=index)`:

```
eax = [map+32]
lea edx, [esi+esi*8]
lea ecx, [eax+edx*8+24]                     // slot index, +24 script
0099E480 → 004FAFF0 (".tng")
```

First taken open: native **index 1**, script `LookoutPoint`.
**PROVEN** (`tng-first-after-leave`). PicnicArea is `NewMap 2`
(second prox open). Bridge is `NewMap 4` (later slot; first
*construct* file after `00501450`, not this walk).

Index **0** is never passed to `004FBF60` on this function.

---

## C# `WorldFile.Maps[0]`

```csharp
// WorldFile.Parse
if (StartsWithToken(line, "NewMap", out var newMapRest))
    current = new WorldMapBuilder { Index = ParseInt(newMapRest) };
if (EndMap)
    maps.Add(current.Build());   // append; no dummy
```

| Native | C# |
|---|---|
| vector `[+32]`, stride 72, size **399** | `IReadOnlyList<WorldMap>`, count **398** |
| slot 0 dummy (`[+36]=0`) | **absent** |
| slot 1 LookoutPoint | `Maps[0]`, `.Index==1` |
| slot *k* = `NewMap k` | `Maps[k-1]`, `.Index==k` |

`TlcInstallTests.World_starts_at_lookout_point`:
`Maps[0].Index==1`, `ScriptName=="LookoutPoint"`.
`WorldSceneTests`: `Maps.Count==398`.

`FindMap` / `foreach Maps` therefore see Lookout first.
Writing `Maps[0]` as “the first authored map” is **MATCH**.
Writing `Maps[0]` as “native index 0” is **DISPROVEN**.

`WorldFile.FindRegionContaining` comment (“New-game Oakvale is
`StartOakVale`, not `Maps[0]`”) is **LEFTOVER** vs no-save:
`Maps[0]` **is** LookoutPoint; Oakvale is `NewRegion 4`.

---

## Host `GlobalThingMapsLoaded` order

`EngineLifecycle.LoadGlobalThingsFile` (`004FDBC0` arm):

```csharp
foreach (var map in World.Maps)          // Maps[0] first
{
    if (!map.LoadedOnPlayerProximity)
        continue;
    var tng = _levels?.TryLoadThings(map.ScriptName);
    if (tng is null)
        continue;
    loaded.AddRange(tng.Things);
    GlobalThingMapsLoaded++;             // first: LookoutPoint
}
```

| Step | Native `ebx` | Host |
|---|---|---|
| skip | 0 dummy | no row |
| first prox open | **1** Lookout | `Maps[0]` Lookout |
| second | 2 PicnicArea (if prox) | `Maps[1]` if prox |
| count | slots with `+36 && +40` | prox maps whose `.tng` loads |

First increment **MATCH**. Walk order **MATCH** for the
shipped sequential `NewMap 1…398` file.

DIVERGE (not this first hit):

- Host has no slot 0 and does not loop a native index.
- Native vector length 399 vs `Maps.Count` 398.
- Host skips missing `.tng` without increment; native still
  `004FBF60`. Live miss count **UNREAD**. TLC census both
  sides: **151** prox maps / ~21746 things (**MATCH** as a
  total, not as a per-slot log).
- Host concatenates `GlobalThings` and does not
  `LoadSingleThing`. Native `005223F0` `[manager+128]`
  **UNREAD** (`tng-first-after-leave`).

---

## Do not confuse with region index 1

| Table | Dummy 0 | Lookout native | C# |
|---|---|---|---|
| Maps `+32` / 72 | `00515AD0(1)` | slot **1** = `NewMap 1` | `Maps[0].Index==1` |
| Regions `+44` / 88 | ctor 88-byte row | first **append** = 1 | `Regions[0].Index==1` |

`00501450` / `00500540(1,0,0)` is the **region** job
(Lookout cluster, ContainsMap Bridge / Lookout / Guild).
That is not `004FDBC0`. First TNG **open** is still map
slot 1 Lookout (`tng-first-after-leave`). First TNG
**construct** is Bridge (`first-0051FD80-file`).

---

## Host notes (no edit)

- Keep `WorldMap.Index` as the WLD integer. Do not re-base
  to 0.
- Do not insert a dummy `Maps[0]` unless matching the native
  vector (not required for the first `004FDBC0` name).
- `LoadGlobalThingsFile` foreach is the right **order** for
  this file. Do not start at `Maps[1]` to “skip dummy”.
- `RegionAtNativeIndex(1)` already skips 0. There is no
  `MapAtNativeIndex`; `FindMap("LookoutPoint")` /
  `Maps[0]` is the map-side equivalent.

---

## Open

| Item | Class |
|---|---|
| Live `005223F0` `[+128]` on first `004FBF60(1)` | **UNREAD** |
| Any TLC prox `.tng` miss that would desync the 151 count | **UNREAD** |
| BWD declared 399 vs parsed 398 as the dummy slot | **PARTIAL** (same numbers; not this dump) |
