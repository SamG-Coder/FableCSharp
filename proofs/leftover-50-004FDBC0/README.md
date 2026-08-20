# Leftover #50: host first-proximity `break` vs native `004FDBC0`

Investigation only. Production `src/` was not edited.

Do **not** fold this into leftover #4 (Lookout first *region* /
first *rendered* scene vs Oakvale intro view). This leftover
is the **global TNG pump** (`004FDBC0` / `LoadGlobalThingsFile`).

Do **not** parse every `LoadedOnPlayerProximity` `.tng` (OOM).
Census 151 / ~21746 is already locked in `004FDBC0-vs-host`.

Do **not** start at Oakvale / `00DBDE40` / `StartOakVale`.
No-save New Game is Leave `0042F2A2` → `FinalAlbion.wld` →
Loading world `004A1840` → `00507C30` → `00509859` →
`004FDBC0`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: native `004FDBC0` first-seen `ebx`? Skip dummy
slot 0? First `004FBF60` map name? Host `break` vs that
walk?

Authority: dump `listing-004c0000.txt` (`004FDBC0` /
`004FBF60` / `004FAFF0`);
`proofs/004FDBC0-open`, `proofs/004FDBC0-vs-host`,
`proofs/004FDBC0-host-leftover`, `proofs/wld-map-index-0`;
host notes only: `EngineLifecycle.LoadGlobalThingsFile`.

This is **not** a recovered NewMap-1 lock. Host first-file
name happening to be LookoutPoint is WLD parse order, not
`map.Index == 1`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Native first-seen `ebx` | **`1`** (`004FDBDE  mov ebx, 1`) | **PROVEN** |
| Skip dummy slot 0? | **Yes.** Loop starts `ebx=1`, `edi=0x48` (slot `begin+72`). Index 0 is never `push`ed to `004FBF60`. | **PROVEN** |
| First `004FBF60` map | **`LookoutPoint.tng`** (`ebx=1` = WLD `NewMap 1`, slot+24 script + `004FAFF0` `".tng"`) | **PROVEN** |
| Native stop after that first prox map? | **No.** `inc ebx` / `add edi, 72` / `cmp ebx, count` / `jb 004FDC00`. Visits slots **1..count-1**. | **PROVEN** |
| Host Lookout-only pump native? | **No.** Host `break` after first `LoadedOnPlayerProximity` is **DIVERGE**. | **DIVERGE** |

**Native first map + ebx: `LookoutPoint` (index 1), `ebx=1`.**

---

## Verdict

**Open first file is MATCH. Pump width is DIVERGE.**

Native `004FDBC0` skips dummy 0, opens `LookoutPoint.tng`
first, then keeps walking every filled+proximity slot.
Host `LoadGlobalThingsFile` `break`s on the first
`LoadedOnPlayerProximity` map, Notes `"004FBF60 " +
lookout.ScriptName + ".tng"`, sets
`GlobalThingMapsLoaded = 1`. Real reason in code: parsing
every proximity `.tng` OOMs the New Game pump.

Comment claims `004FDBC0 ebx=1` skips dummy slot 0 and
first `004FBF60` is LookoutPoint (NewMap 1); later maps
stay closed until `00501450` / `ContainsMap`. The ebx /
first-name half is **PROVEN**. The “later maps stay closed”
half is **DISPROVEN** as native `004FDBC0` (those maps
**are** opened here; they stay closed for *region apply*
`006C2170`, a different walk).

Tests lock `GlobalThingMapsLoaded == 1` + LookoutPoint in
the Note, no Bowerstone Note
(`New_Game_004FDBC0_opens_LookoutPoint_only`). That is a
**host** lock, not a recovered native Lookout-only walk.

Leave leftover **#50** open. Do not fold into #4.

---

## Native `004FDBC0` (`listing-004c0000`)

Thiscall, `ecx=CWorldMap`, **no** stack args, `ret`.
Caller `00509948` after `"Load global things"` when
`[0x13B8609]==0` (`.gtg` arm is **not** no-save).

```
004FDBC0  sub esp, 8
004FDBC3  push ebx
004FDBC4  push esi
004FDBC5  mov esi, ecx                  ; CWorldMap
004FDBC7  mov eax, [esi+32]             ; map-table begin
004FDBCA  mov ecx, [esi+36]             ; end
          count = (end-begin)/72        ; imul 0x38E38E39
004FDBDE  mov ebx, 1                    ; FIRST-SEEN ebx
004FDBE5  cmp eax, ebx
004FDBEB  jbe 004FDCA3                  ; count<=1: dummy only, ret
004FDBF2  mov edi, 0x48                 ; stride 72
loop:
004FDC00  push -1
          push "Loading global things"
          …
004FDC5D  mov eax, [esi+32]
004FDC60  mov cl, [eax+edi+36]          ; filled
004FDC64  add eax, edi
004FDC66  test cl, cl
004FDC68  je 004FDC79                   ; skip unfilled
004FDC6A  mov cl, [eax+40]              ; LoadedOnPlayerProximity
004FDC6D  test cl, cl
004FDC6F  je 004FDC79
004FDC71  push ebx
004FDC72  mov ecx, esi
004FDC74  call 004FBF60                 ; ret 4
004FDC90  inc ebx
004FDC93  add edi, 72
004FDC96  cmp ebx, eax                  ; eax = recount
004FDC9C  jb 004FDC00
004FDCA8  ret
```

| Slot | First-seen |
|---|---|
| `ebx` | **1** |
| `edi` | **0x48** (72) |
| first slot | `begin + 72` = native index **1** |
| first `push ebx` | **1** |
| dummy 0 | never in this loop |

Dummy skip is **two** gates, not one:

1. Loop never loads slot 0 (`ebx` starts at 1).
2. Ctor dummy (`004FDDE0` / `00515AD0(1)`) has `[+36]=0`
   even though `[+40]` defaults to 1, so a `ebx=0` walk
   would still skip. Starting at 1 is belt-and-braces
   with 1-based `NewMap` (`wld-map-index-0`).

There is **no** `break` / early `ret` after the first
taken `004FBF60`. After TLC `NewMap 1…398`, `count=399`,
loop visits **1..398**. Every `[+36] && [+40]` slot
opens a `.tng`. PicnicArea is `NewMap 2` (second prox
open). BowerstoneBridge is a later slot on **this**
walk, and the first *construct* file after `00501450`.

### First `004FBF60` name

```
004FBF60  esi = [esp+4]                 ; map index (ebx)
          edi = ecx                     ; CWorldMap
004FBF6B  eax = [edi+32]
004FBF72  lea edx, [esi+esi*8]          ; index * 9
004FBF76  lea ecx, [eax+edx*8+24]       ; slot+24 script
004FBF7A  call 0099E480
004FBF85  call 004FAFF0                 ; append 0x12442C4 ".tng"
```

First call: `esi=1`. WLD `NewMap 1` `LevelScriptName`
`"LookoutPoint"`. Path is **`LookoutPoint.tng`**. **PROVEN.**

Do not parse the 151 files to name the rest. First name
is enough for this leftover.

---

## Host `LoadGlobalThingsFile` — DIVERGE

`EngineLifecycle.LoadGlobalThingsFile` (read at this
investigation):

```csharp
// 004FDBC0 ebx=1 skips dummy slot 0.
// First 004FBF60 is LookoutPoint (NewMap 1).
// Later proximity maps stay closed until
// 00501450 / ContainsMap. Parsing every
// LoadedOnPlayerProximity .tng here is
// leftover and OOMs the New Game pump.
WorldMap? first = null;
foreach (var map in World.Maps)
{
    if (!map.LoadedOnPlayerProximity)
        continue;
    first = map;
    break;
}
if (first is { } lookout)
{
    Note(LoadGlobalThingsPerMap, …,
        "004FBF60 " + lookout.ScriptName + ".tng");
    var tng = _levels?.TryLoadThings(lookout.ScriptName);
    …
    GlobalThingMapsLoaded = 1;
}
```

C# `World.Maps` has **no** dummy row (`Maps[0].Index==1`,
`ScriptName=="LookoutPoint"`). `foreach` first prox **is**
Lookout on TLC. Host does **not** look up `map.Index == 1`
or the name `LookoutPoint`.

| Site | Native `004FDBC0` | Host | Class |
|---|---|---|---|
| First-seen `ebx` / start index | **1** | C# `Maps[0]` (no dummy) | **MATCH** numbering |
| Dummy slot 0 | skipped | absent from list | **MATCH** skip |
| First file | `004FBF60(1)` → `LookoutPoint.tng` | `TryLoadThings(first.ScriptName)` | **MATCH** first name on TLC |
| After first prox | keep looping 1..398 | **`break`** | **DIVERGE** |
| Maps loaded | 151 prox (census) | **`1`** | **DIVERGE** |
| Later prox `.tng` this VA | **open** | **not opened** | **DIVERGE** |
| Name lock | native index 1 | first prox in WLD order | **PARTIAL** (TLC coincides) |
| Comment “later maps stay closed until `00501450`” | **DISPROVEN** as this pump (they open here); **PROVEN** as region apply | host excuse for `break` | **DIVERGE** |
| OOM if 151 parsed | native opens (construct gated) | host would OOM `ThingFile.Parse` | host reason **PROVEN**; not native width |
| `EnsureLevels` WAD + `_RT.stb` | not this VA | ctor extra | **DIVERGE** (I/O leftover, not #50 width) |

Lookout-only is **host**. Do not claim it is native.

---

## Not leftover #4 / not NewMap-1 lock

| Claim | Class |
|---|---|
| First *open* is LookoutPoint | **PROVEN** (this VA) |
| First *rendered* region is LookoutPoint | leftover **#4**, different tree |
| Oakvale intro view | leftover **#4** (`FIRST_SCENE_*`) |
| Host `break` recovers native “only NewMap 1” | **DISPROVEN** |
| Tests lock `ebx=1` / `004FBF60` callee / `00501450` | **DISPROVEN** (they lock Note text + count 1) |
| First-proximity TNG pump | leftover **#50** — leave open |

`004FDBC0-host-leftover` leftover is **construct skip**
(`005223F0` `[manager+128]==1`). That is a **different**
leftover from pump **width**. #50 is the `break`. Do not
collapse them, and do not implement `LoadSingleThing` here
to “fix” either.

---

## Do not

- Fold #50 into #4.
- Claim Lookout-only is native `004FDBC0`.
- Parse every proximity TNG to prove first name.
- Start host at `Maps[1]` to skip a dummy the C# list
  does not have.
- Treat host `GlobalThingMapsLoaded == 1` as a recovered
  NewMap-1 lock.
- Call first **open** the first **CThing** (Bridge after
  `00501450` / `006C2170`).
- Bind Oakvale / `00DBDE40` / kid `CREATURE_HERO_CHILD`.
