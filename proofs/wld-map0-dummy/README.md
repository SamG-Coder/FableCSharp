# Dummy map slot 0 vs host `Maps[0]`

Investigation only. No production `src/` edits.

Do **not** treat C# `Maps[0]` as native index 0.
Do **not** start at Oakvale / `00DBDE40`. First authored map
is LookoutPoint.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **DIVERGE** / **MATCH** / **LEFTOVER**.

Question: `00507C30` first `NewMap` is LookoutPoint index **1**.
Dummy slot **0**? Host `Maps[0]` **DIVERGE**?

Authority: `proofs/wld-00507C30-switch`, `proofs/wld-map-index-0`;
`src/Fable.Formats/Wld/WorldFile.cs`.

Sources: `listing-00500000.txt` (`005066E0` / `00515AD0` /
`00507C30` / `0050833F` / `005083A9`);
`listing-004c0000.txt` (`004FDDE0` / `004FDBC0`);
TLC `FinalAlbion.wld` head;
`EngineLifecycle.LoadGlobalThingsFile`;
`TlcInstallTests.World_starts_at_lookout_point`;
`WorldSceneTests` (`Maps.Count==398`).
Siblings: `004FDBC0-open`, `004FDBC0-vs-host`,
`first-region-after-leave`.

---

## Verdict

**Yes dummy 0 on the native map vector. Host `Maps[0]` is
not that slot.**

`00507C30` first matched map token on `FinalAlbion.wld` is
**`NewMap 1`** / `LevelScriptName "LookoutPoint"`. That integer
is a **sparse table index**. `EndMap` writes stride-72 slot
**1**. File never emits `NewMap 0`; `eax==0` is rejected.

Ctor `005066E0` `00515AD0(1)` already planted **one 72-byte
row at index 0** (`004FDDE0`: `[+36]=0`, empty intern at
`+24`). `004FDBC0` starts `ebx=1`. Dummy 0 is never opened.

C# `WorldFile.Parse` **appends** authored `EndMap`s. There is
**no** dummy row. `Maps[0].Index==1` `ScriptName=="LookoutPoint"`.

| Claim | Class |
|---|---|
| First `NewMap` is **1** LookoutPoint | **PROVEN** |
| Native map slot **0** is a ctor dummy | **PROVEN** |
| Native slot 0 **is** LookoutPoint | **DISPROVEN** |
| Dummy 0 is *only* the 88-byte region row | **DISPROVEN** (`wld-00507C30-switch` leftover) |
| Host inserts dummy `Maps[0]` | **DISPROVEN** |
| Host `Maps[0]` == native slot **1** (name / `.Index`) | **MATCH** |
| Host `Maps[0]` == native slot **0** | **DIVERGE** |
| Length 398 vs native 399 | **DIVERGE** |
| First `004FDBC0` name (Lookout `.tng`) | **MATCH** |

**Answer:** dummy slot 0 **exists** (map vector *and* a
separate region vector). Host `Maps[0]` **DIVERGE**s from
that dummy. It **MATCH**es first authored `NewMap 1`.
Do not start at `Maps[1]` to “skip dummy”.

---

## Path (this table only)

```
00418784  Init World
  004A6EC0  005066E0  CWorldMap ctor
    push 1; lea ecx, [esi+32]; call 00515AD0
      alloc 1×72; size=1                    ← dummy MAP slot 0
    push 88; [esi+44] one region row        ← dummy REGION slot 0
    [esi+156]=0
00416953  Loading world
  00507C30  Load .wld file
    MapUIDCount / ThingManagerUIDCount
    NewMap 1  009BA540 → [esp+36]=1         ← first NewMap
    EndMap    eax=1; skip if 0
              grow [+32] to 2
              write slot ebx=1 LookoutPoint
              [slot+36]=1  [slot+40]=prox
    NewMap 2…398; size ends 399
    EOF → 00509859 → 004FDBC0
      ebx=1; first 004FBF60(1)              ← skip dummy 0
```

---

## First `NewMap` is index 1 LookoutPoint

`listing-00500000` empty intern `00507F0A` and `004115A0`
arm `0050833F` share the same body:

```
0050833F  push "NewMap"
          004115A0
          "Loading maps"
          009BA540 → [esp+36]               // file integer N

005083A9  mov eax, [esp+36]
005083AD  test eax, eax
005083AF  je 0050933B                       // NewMap 0: no write
          ebx = N
          edi = N+1
          if count < N+1: 0051BE20(N+1)
          ebp = begin + N*72                // lea edx,[ebx+ebx*8]; *8
          copy script to [ebp+24]
          [ebp+40] = LoadedOnPlayerProximity
          [ebp+36] = 1
          [ebp+64] = IsSea
```

TLC file head after the unmatched quest sentinels:

```
MapUIDCount 72;
ThingManagerUIDCount 1;
NewMap 1;
MapX 3232; MapY 3488;
LevelName "FinalAlbion\LookoutPoint.lev";
LevelScriptName "LookoutPoint";
MapUID 162441;
IsSea FALSE;
LoadedOnPlayerProximity TRUE;
EndMap;
```

First *matched* tokens are `MapUIDCount` then
`ThingManagerUIDCount` then this `NewMap`. **PROVEN**
(`wld-00507C30-switch`). Census **398** `NewMap` `1…398`,
no gaps, no `NewMap 0`.

---

## Dummy slot 0 (map vector)

```
00506738  push 1
0050673A  lea ecx, [esi+32]
00506740  call 00515AD0
            edi=1
            alloc 72                         // lea eax,[edi+edi*8]; shl 3
            [begin,end,cap] = [p, p+72, p+72]
            005110B0 copy of 004FDDE0
```

`004FDDE0` default 72-byte record:

```
[+24] empty intern (0x122D70E)
[+36..39] = 0                               // not filled
[+40] = 1                                   // prox TRUE default
[+64] = 0                                   // IsSea
```

`004FDBC0` (`listing-004c0000`):

```
004FDBDE  mov ebx, 0x1                      // skip dummy 0
004FDBF2  mov edi, 0x48                     // 72
loop:
          slot = begin + edi                // first: begin+72 = index 1
          if [slot+36] && [slot+40]:
            call 004FBF60                   // first: ebx=1
          inc ebx
          add edi, 72
```

Dummy fails `[+36]` even if `[+40]` is 1. Starting `ebx=1`
is belt-and-braces with 1-based `NewMap`. After 398
`EndMap`s, count=399, walk is slots **1..398**.

Region dummy is a **different** vector (`[+44]`, stride 88,
one `006BC410` row). `EngineLifecycle.CurrentRegionIndex`
ctor 0 is **that** dummy, not `Maps[0]`. Both tables have
an index-0 dummy. Sibling `wld-00507C30-switch` (“dummy
slot 0 is the 88-byte region row, **not** this 72-byte
map table”) is **DISPROVEN** as a map-table claim
(`wld-map-index-0`).

Dummy *pumps* (`004189C2` / `004FC180` index 0) are the
**region** job, not this map slot.

---

## Host `Maps[0]`

```csharp
// WorldFile.Parse
if (StartsWithToken(line, "NewMap", out var newMapRest))
    current = new WorldMapBuilder { Index = ParseInt(newMapRest) };
if (EndMap)
    maps.Add(current.Build());   // append; no dummy
```

| Native `[+32]` stride 72 | C# `World.Maps` |
|---|---|
| size **399** (0..398) | count **398** |
| slot 0 dummy (`[+36]=0`) | **absent** |
| slot 1 LookoutPoint | `Maps[0]`, `.Index==1` |
| slot *k* = `NewMap k` | `Maps[k-1]`, `.Index==k` |

`TlcInstallTests.World_starts_at_lookout_point`:
`Maps[0].Index==1`, `ScriptName=="LookoutPoint"`,
MapX 3232 / MapY 3488. `WorldSceneTests`:
`Maps.Count==398`, same first script.

`LoadGlobalThingsFile` `foreach (var map in World.Maps)`
therefore opens Lookout first — same **name** as
`004FBF60(1)`. First increment **MATCH**.

### DIVERGE (the question)

| Sense | Host `Maps[0]` | Native | Class |
|---|---|---|---|
| Native index 0 / dummy | LookoutPoint | empty `[+36]=0` | **DIVERGE** |
| Vector length | 398 | 399 | **DIVERGE** |
| First authored map | Lookout `.Index==1` | slot 1 `NewMap 1` | **MATCH** |
| First `004FDBC0` file | `TryLoadThings("LookoutPoint")` | `ebx=1` | **MATCH** |
| Skip dummy by using `Maps[1]` | PicnicArea | would skip Lookout | **DIVERGE** |

`FindRegionContaining` comment (“New-game Oakvale is
`StartOakVale`, not `Maps[0]`”) and `RegionTravel` type
header (“`Maps[0]` Lookout is the adult overworld first
map, not new-game”) are **LEFTOVER** vs no-save: `Maps[0]`
**is** LookoutPoint; Oakvale is `NewRegion 4` / `NewMap 203`.

Host does **not** need a dummy `Maps[0]` to match the first
`004FDBC0` name. Inserting one would make `Maps[0]` track
native 0 and **break** every `Maps[0]==Lookout` test.

---

## Host notes (no edit)

- Keep `WorldMap.Index` as the WLD integer. Do not re-base
  to 0.
- Do not insert a dummy `Maps[0]`.
- Do not start `LoadGlobalThingsFile` at `Maps[1]`.
- `RegionAtNativeIndex(1)` already skips region dummy 0.
  Map-side equivalent is `FindMap("LookoutPoint")` /
  `Maps[0]`, not `Maps[1]`.

---

## Open

| Item | Class |
|---|---|
| Live `005223F0` `[+128]` on first `004FBF60(1)` | **UNREAD** (`wld-map-index-0`) |
| BWD declared 399 vs parsed 398 as this dummy | **PARTIAL** (same numbers; not this dump) |
