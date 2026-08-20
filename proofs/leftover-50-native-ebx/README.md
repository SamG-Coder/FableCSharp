# Leftover #50: native `004FDBC0` `ebx` / NewMap index / loop bound

Investigation only. Production `src/` and `tests/` were
not edited.

Do **not** parse every `LoadedOnPlayerProximity` `.tng`
(host OOM). Census 151 / ~21746 is already locked in
`proofs/004FDBC0-vs-host`. This note uses dump
`004FDBC0` / `004FBF60` plus WLD `NewMap` tokens only.
Loading the prox set here would invent a host-width
MATCH that native does **not** have as a skip.

Do **not** fold leftover **#4** (Lookout first *rendered*
scene vs Oakvale intro *view*) into this leftover. #50
is the **global TNG pump** (`004FDBC0` / host
`LoadGlobalThingsFile` first-prox `break`).

Do **not** start at Oakvale / `00DBDE40` / `StartOakVale`.
No-save New Game is Leave `0042F2A2` → `FinalAlbion.wld`
→ Loading world `004A1840` → `00507C30` → `00509859`
→ `004FDBC0`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: native `004FDBC0` args, first-seen `ebx`,
NewMap index of the first `004FBF60`, whether the
walk loads **only Lookout** or **all** filled+prox
maps, exact loop bound. First-seen no-save?

Authority: dump `listing-004c0000.txt` (`004FDBC0` /
`004FBF60` / `004FAFF0`), `listing-00500000.txt`
(`00507C30` / `0050833F` / `00509859` / `00509948`),
`assembly/exe/01-sections/text-map/e8.tsv` (one
`E8 004FDBC0`), TLC `FinalAlbion.wld` (`NewMap 1`
LookoutPoint; C# `Maps.Count==398`),
`proofs/004FDBC0-open`, `proofs/004FDBC0-vs-host`,
`proofs/004FDBC0-host-leftover`,
`proofs/wld-map-index-0`,
`proofs/leftover-50-004FDBC0`,
`proofs/leftover-50-tng-ebx`,
`proofs/leftover-50-tng-oom`.
Host notes only: `EngineLifecycle.LoadGlobalThingsFile`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| `004FDBC0` args | thiscall `ecx=CWorldMap`, **no** stack args, `ret` | **PROVEN** |
| First-seen no-save takes this VA? | **Yes.** `[0x13B8609]==0` → `je 00509946` → `call 004FDBC0`. Only `.text` `E8` site. `.gtg` `004FE2A0` is **not** no-save. | **PROVEN** |
| Native first-seen `ebx` | **`1`** (`004FDBDE  mov ebx, 0x1`) | **PROVEN** |
| Dummy slot 0 in this loop? | **Never.** `edi=0x48`, first slot `begin+72` | **PROVEN** |
| NewMap index of first `004FBF60` | **`1`** (`push ebx` with `ebx=1` = WLD `NewMap 1`) | **PROVEN** |
| First file | **`LookoutPoint.tng`** (slot+24 script + `004FAFF0` `0x12442C4` `".tng"`) | **PROVEN** |
| Loads only Lookout? | **No.** No `break` / early `ret` after the first taken `004FBF60`. | **DISPROVEN** |
| Loads all proximity maps? | **Yes — every filled + `LoadedOnPlayerProximity` slot in the bound.** Non-prox slots still `inc ebx` and skip `004FBF60`. | **PROVEN** |
| Exact native loop bound | **`ebx < count` after `inc ebx`.** First-seen `ebx=1`. Visits **`ebx=1..count-1`**. TLC `count=399` → **`ebx=1..398`**. | **PROVEN** |

**Native first map + ebx: `LookoutPoint` (NewMap 1), `ebx=1`.**
**Native width: all filled+prox in `1..398`, not Lookout-only.**
**First-seen no-save: yes (`004FDBC0`, not `004FE2A0`).**
Leave leftover **#50** open (host `break` / OOM).

---

## Verdict

**Args / `ebx` start / NewMap-1 first file are MATCH.
Pump width is DIVERGE (leftover #50).**

Native `004FDBC0` is thiscall `ecx=CWorldMap` with no
stack args. First-seen no-save reaches it from the only
`E8` (`00509948`) after `"Load global things"` when BSS
`[0x13B8609]==0`. `ebx` starts at **1**, dummy slot 0 is
never pushed, first `004FBF60` is **NewMap 1**
`LookoutPoint.tng`, then the loop walks **every** later
filled+prox slot through **`ebx=count-1`**.

Host `LoadGlobalThingsFile` `break`s after the first
`LoadedOnPlayerProximity` map, Notes `"004FBF60 " +
lookout.ScriptName + ".tng"`, sets
`GlobalThingMapsLoaded = 1`. Tests
(`New_Game_004FDBC0_opens_LookoutPoint_only`) lock that
count **1** + Lookout in the Note, no Bowerstone Note.
That is a **host OOM workaround**, not a recovered
native Lookout-only / NewMap-1 lock.

Do not treat leftover #50 as leftover #4.

---

## `004FDBC0` args (dump)

Caller `00509948` (`listing-00500000.txt`). `ebx` there
is still the `CWorldMap` from `00507C30`:

```
00509857  push -1
00509859  push "Load global things"
…
0050987B  mov al, [0x13B8609]
00509880  test al, al
00509882  je 00509946                 ; first-seen 0 TAKEN
…
00509946  mov ecx, ebx                ; CWorldMap
00509948  call 004FDBC0               ; only E8 of this dest
0050994D  push -1
0050994F  push "Load global things end"
```

| Slot | Value |
|---|---|
| `ecx` | `CWorldMap` (same as `00507C30`) |
| stack | **none** |
| ret | `ret` (`004FDCA8`) |
| BSS | `[0x13B8609]==0` else `004FE2A0` `.gtg` **not** this path |

`listing-004c0000.txt` / `e8.tsv`: **one** `.text`
`E8 004FDBC0`. First-seen no-save **is** this site.

---

## Native loop (`listing-004c0000.txt`)

```
004FDBC0  sub esp, 8
004FDBC3  push ebx
004FDBC4  push esi
004FDBC5  mov esi, ecx                  ; CWorldMap
004FDBC7  mov eax, [esi+32]             ; map-table begin
004FDBCA  mov ecx, [esi+36]             ; end
004FDBCD  sub ecx, eax
004FDBCF  mov eax, 0x38E38E39           ; signed /72
004FDBD4  imul ecx
004FDBD6  sar edx, 4
004FDBD9  mov eax, edx
004FDBDB  shr eax, 31
004FDBDE  mov ebx, 0x1                  ; FIRST-SEEN ebx
004FDBE3  add eax, edx                  ; count = (end-begin)/72
004FDBE5  cmp eax, ebx
004FDBEB  jbe 004FDCA3                  ; count<=1: dummy only, ret
004FDBF1  push edi
004FDBF2  mov edi, 0x48                 ; stride 72
004FDBF7  jmp 004FDC00
loop:
004FDC00  push -1
          push "Loading global things"  ; progress, not the bound
          …
004FDC5D  mov eax, [esi+32]
004FDC60  mov cl, [eax+edi*1+36]        ; EndMap filled
004FDC64  add eax, edi
004FDC66  test cl, cl
004FDC68  je 004FDC79                   ; skip unfilled
004FDC6A  mov cl, [eax+40]              ; LoadedOnPlayerProximity
004FDC6D  test cl, cl
004FDC6F  je 004FDC79
004FDC71  push ebx                      ; native map index
004FDC72  mov ecx, esi
004FDC74  call 004FBF60                 ; ret 4
004FDC79  mov edx, [esi+32]
          mov ecx, [esi+36]
          recount count                 ; same /72 magic
004FDC90  inc ebx
004FDC93  add edi, 72
004FDC96  cmp ebx, eax                  ; eax = recount
004FDC9C  jb 004FDC00                   ; ebx < count
004FDCA8  ret
```

| Slot | First-seen |
|---|---|
| `ebx` start | **1** |
| `edi` start | **0x48** (72) |
| first slot | `begin + 72` = native index **1** |
| first `push ebx` | **1** |
| dummy 0 | never in this loop |
| compare | `cmp ebx, eax` / `jb 004FDC00` |
| stop | `ebx >= count` after `inc` |
| visited | **`ebx = 1 .. count-1`** |
| TLC `count` | **399** (ctor dummy + `NewMap 1..398`) |
| TLC visited | **`ebx = 1 .. 398`** |
| early `ret` after first prox | **none** |

**Exact native loop bound: `ebx < count`, `ebx` ∈ `[1, count)`.**
On TLC first-seen no-save that is **`ebx=1..398`**.

The progress helper at `004FDC2B`
`lea edx, [edx+ecx*1+1]` is **`count+1`** as a
denominator for `"Loading global things"`. That is
**not** the loop bound. Do not report `1..399` or
`0..398` as this walk.

Dummy skip is two gates, not one:

1. Loop never loads slot 0 (`ebx` starts at 1).
2. Ctor dummy (`005066E0` / `00515AD0(1)` / `004FDDE0`)
   has `[+36]=0` even though `[+40]` defaults to 1, so a
   `ebx=0` walk would still skip. Starting at 1 is
   belt-and-braces with 1-based `NewMap`
   (`wld-map-index-0`).

`ebx` is the **map-table index**, the same `N` that
`EndMap` wrote (`005083C4  mov ebx, eax` from
`NewMap N` / `009BA540`). It is **not** the ordinal of
prox opens. Non-prox slots still `inc ebx` and skip
`004FBF60`.

---

## First `004FBF60` is NewMap 1 `LookoutPoint.tng`

```
004FBF60  sub esp, 8
004FBF64  mov esi, [esp+16]             ; map index (ebx)
004FBF69  mov edi, ecx                  ; CWorldMap
004FBF6B  mov eax, [edi+32]
004FBF72  lea edx, [esi+esi*8]          ; index * 9
004FBF76  lea ecx, [eax+edx*8+24]       ; slot+24 script
004FBF7A  call 0099E480
004FBF81  lea ecx, [esp+20]
004FBF85  call 004FAFF0                 ; append 0x12442C4 ".tng"
…
004FC04D  ret 4
```

| Slot | Value |
|---|---|
| `ecx` | `CWorldMap` |
| `[esp+4]` | map index (`ebx` from `004FDBC0`) |
| ret | `ret 4` |

`004FAFF0` `push 0x12442C4` (host `TngExtVa` /
`TngExtension=".tng"`). First call: `esi=1`. WLD
`NewMap 1` `LevelScriptName "LookoutPoint"`. Path is
**`LookoutPoint.tng`**. **PROVEN.**

Do **not** parse the 151 files to name the rest. First
name + WLD tokens are enough. PicnicArea is `NewMap 2`
(second taken `004FBF60` on TLC). `StartOakValeWest` is
`NewMap 203` / prox TRUE, so native **does**
`004FBF60(203)` inside the **same** bound. That open is
Loading world, **not** first Present (leftover #4).

Three `.text` `E8 004FBF60` sites. Only `004FDC74` is
this leftover:

| Site | Function | First-seen no-save? |
|---|---|---|
| `004FDC74` | `004FDBC0` global prox walk | **yes** (this VA) |
| `004FE128` | `004FE2A0` `.gtg` compile; `xor ebx, ebx` starts **0** | **DISPROVEN** (`[0x13B8609]==0`) |
| `00507059` | `00506F30` map-add after `[slot+36]=1` | **DISPROVEN** as New Game first-seen |

---

## WLD NewMap slots (TLC, no TNG parse)

`EndMap` (`00508395`) writes sparse index `N`:

```
005083A9  mov eax, [esp+36]             ; pending NewMap N
005083AD  test eax, eax
005083AF  je 0050933B                   ; NewMap 0: no write
005083C4  mov ebx, eax                  ; ebx = N
005083D5  lea edi, [ebx+1]
          grow [+32] to N+1 if needed
005083E8  lea edx, [ebx+ebx*8]
005083EB  lea ebp, [ebp+edx*8]          ; slot N, stride 72
          copy script to [ebp+24]
0050843B  mov [ebp+40], dl              ; LoadedOnPlayerProximity
00508449  mov [ebp+36], 0x01            ; filled
```

Ctor dummy is **not** overwritten: file `NewMap 0` is
rejected, and TLC never emits it. After `NewMap 1…398`
with no gaps, native length is **399**.

C# `World.Maps` has **no** dummy row (`Maps.Count==398`,
`Maps[0].Index==1`, `ScriptName=="LookoutPoint"`,
`LoadedOnPlayerProximity` TRUE —
`TlcInstallTests.World_starts_at_lookout_point`,
`WorldSceneTests`). `FindMap("StartOakValeWest").Index
== 203`.

```
NewMap 1;
LevelScriptName "LookoutPoint";
LoadedOnPlayerProximity TRUE;
EndMap;

NewMap 2;
LevelScriptName "PicnicArea";
LoadedOnPlayerProximity TRUE;
EndMap;
…
NewMap 203;
LevelScriptName "StartOakValeWest";
LoadedOnPlayerProximity TRUE;
EndMap;
…
NewMap 398;
LevelScriptName "NorthernWastes3_Filler_09";
LoadedOnPlayerProximity FALSE;
EndMap;
```

Native table: dummy 0 + those rows. Loop bound
`ebx=1..398` visits **every authored map**. `004FBF60`
runs only when `[+36] && [+40]`. Census of taken opens
is 151 (`004FDBC0-vs-host`, WLD prox token). **Do not
open those 151 `.tng` files here.**

---

## Host leftover #50 — not a native skip

`EngineLifecycle.LoadGlobalThingsFile` (read at this
investigation):

```
// 004FDBC0 ebx=1 skips dummy slot 0.
// First 004FBF60 is LookoutPoint (NewMap 1).
// Native then inc ebx through every filled
// LoadedOnPlayerProximity slot (1..count-1).
// Host break after the first prox file is
// leftover #50 (ThingFile.Parse OOM), not a
// recovered NewMap-1 lock and not 00501450.
foreach (var map in World.Maps)
{
    if (!map.LoadedOnPlayerProximity) continue;
    prox++;
    first ??= map;
}
if (first is { } lookout)
{
    Note(…, "004FBF60 " + lookout.ScriptName + ".tng");
    TryLoadThings(lookout.ScriptName);
    GlobalThingMapsLoaded = 1;
    if (prox > 1)
        Note(LoadGlobalThingsMapFile, …,
            $"004FDC00 leftover host break ebx=2..{prox} unparsed={prox-1}");
}
```

C# `World.Maps` has no dummy. `foreach` first prox
**is** Lookout on TLC. Host does **not** look up
`map.Index == 1` or the name `LookoutPoint`.

| Site | Native `004FDBC0` | Host | Class |
|---|---|---|---|
| Args | `ecx=CWorldMap`, no stack | `LoadGlobalThingsFile` on `World` | **MATCH** site |
| `ebx` start | **1** | C# `Maps[0]` (no dummy) | **MATCH** numbering |
| Dummy slot 0 | skipped | absent | **MATCH** skip |
| First file | `004FBF60(1)` → `LookoutPoint.tng` | `TryLoadThings(first.ScriptName)` | **MATCH** first name on TLC |
| Stop | **`ebx>=399`**; walk **1..398** | **`break`** after first prox | **DIVERGE** leftover **#50** |
| Maps opened this VA | 151 prox | **1** | **DIVERGE** |
| Name lock | native index 1 | first prox in WLD order | **PARTIAL** (TLC coincides) |
| Tests `GlobalThingMapsLoaded==1` | native would be 151 | host lock | **DIVERGE**; not a recovered NewMap-1 lock |
| OOM if 151 parsed | native opens (construct gated) | host `ThingFile.Parse` OOM | host reason **PROVEN**; not native width |

Lookout-only is **host**. Do not claim it is native
`004FDBC0`. Do not parse the remaining prox set to
“recover” a skip: native has **no** such skip.

`004FDBC0-host-leftover` leftover is **construct skip**
(`005223F0` `[manager+128]==1`). Different leftover
from pump **width**. #50 is the `break`. Do not
collapse them.

---

## First-seen no-save

```
0042F2A2  Leave
00416953  Loading world
  004A1840
    00507C30  FinalAlbion.wld
      NewMap 1   LookoutPoint     ebx slot 1   prox TRUE
      NewMap 2   PicnicArea       ebx slot 2   prox TRUE
      …
      NewMap 203 StartOakValeWest ebx slot 203 prox TRUE
      NewMap 398 …                ebx slot 398
      count = 399
      00509859  Load global things
        [0x13B8609]==0
        004FDBC0                         ← THIS, only E8
          ebx=1    004FBF60 → LookoutPoint.tng     FIRST OPEN
          ebx=2    004FBF60 → PicnicArea.tng
          …
          ebx=203  004FBF60 → StartOakValeWest.tng SAME PUMP
          ebx=204..398  filled+prox only
          stop ebx>=399
          host: break after first prox             LEFTOVER #50
    Set Static Map                              AFTER
004189C2  dummy pumps  index 0                  0 TNG apply
```

`.gtg` `004FE2A0` is the **untaken** arm. Save load is
not this tree. First-seen no-save **does** run the full
native bound; host does not.

Open ≠ Present. Native would parse Oakvale West during
Loading world. That still does not Present Oakvale
(leftover **#4**).

---

## Not leftover #4 / not NewMap-1 lock

| Claim | Class |
|---|---|
| First *open* is LookoutPoint | **PROVEN** (this VA) |
| Native `ebx` start **1**, bound `ebx=1..count-1` | **PROVEN** |
| Native loads **only** NewMap 1 | **DISPROVEN** |
| Native loads all filled+prox in that bound | **PROVEN** |
| First *rendered* region is LookoutPoint | leftover **#4** |
| Oakvale intro view | leftover **#4** (`FIRST_SCENE_*`) |
| Host `break` recovers native “only NewMap 1” | **DISPROVEN** |
| Tests lock `ebx=1` / `004FBF60` callee / `00501450` | **DISPROVEN** (they lock Note text + count 1) |
| First-proximity TNG pump | leftover **#50** — leave open |

---

## Do not

- Fold #50 into #4.
- Claim Lookout-only is native `004FDBC0`.
- Parse every proximity TNG to prove first name, Oakvale
  index, or a native skip. Census is locked; native skip
  after first prox **does not exist**.
- Report loop bound as `0..398`, `1..399`, or “until
  first prox.” Exact bound is **`ebx < count`**, TLC
  **`1..398`**.
- Start host at `Maps[1]` to skip a dummy the C# list
  does not have.
- Treat host `GlobalThingMapsLoaded == 1` as a recovered
  NewMap-1 lock.
- Call first **open** the first **CThing** or first
  Present (Bridge after `00501450` / `006C2170`; Present
  is leftover #4).
- Bind Oakvale / `00DBDE40` / kid `CREATURE_HERO_CHILD`
  onto this pump.
- Call `00507059` / `004FE128` the New Game opener.
- Collapse #50 (`break`) into `004FDBC0-host-leftover`
  (`+128` construct skip).
