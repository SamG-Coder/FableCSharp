# Leftover #50: native `ebx` / `004FBF60` vs Oakvale TNG on first Present

Investigation only. Production `src/` and `tests/` were
not edited.

Do **not** parse every `LoadedOnPlayerProximity` `.tng`
(host OOM). Census 151 / ~21746 is already locked in
`proofs/004FDBC0-vs-host`. This note uses dump
`004FDBC0` / `004FBF60` plus WLD `NewMap` tokens only.

Do **not** fold leftover **#4** (Lookout first *rendered*
scene vs Oakvale intro *view*) into this leftover. #50
is the **global TNG pump** (`004FDBC0` / host
`LoadGlobalThingsFile` first-prox `break`).

Do **not** start at Oakvale / `00DBDE40` / `StartOakVale`.
No-save New Game is Leave `0042F2A2` → `FinalAlbion.wld`
→ Loading world `004A1840` → `00507C30` → `00509859`
→ `004FDBC0`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Question: native `ebx` start, stop condition, whether
Oakvale TNG is opened on no-save first Present
(**must be NO**). First later site that opens
`StartOakValeWest.tng`.

Dump: `004FDBC0`, `004FBF60`, `ebx=1..n`, NewMap slot,
`LookoutPoint.tng` first. Host leftover **#50**.

Authority: `listing-004c0000.txt` (`004FDBC0` /
`004FBF60` / `004FAFF0`), `listing-00500000.txt`
(`00507C30` / `00509859` / `00509948` / `00507059`),
TLC `FinalAlbion.wld` (`NewMap 1` / `NewMap 203` /
`NewRegion 1` / `NewRegion 4`),
`EngineLifecycle.LoadGlobalThingsFile` /
`PresentWorld` / `LoadFromFirstRealRegion` /
`LoadRegionMapThings` (read only).
Siblings: `proofs/leftover-50-004FDBC0`,
`proofs/004FDBC0-open`, `proofs/004FDBC0-vs-host`,
`proofs/wld-map-index-0`,
`proofs/leftover-4-collapse-audit`,
`proofs/first-region-after-leave`,
`proofs/ctcexpression-quest-names`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Native first-seen `ebx` | **`1`** (`004FDBDE  mov ebx, 1`) | **PROVEN** |
| Dummy slot 0 in this loop? | **Never.** `edi=0x48`, first slot `begin+72` | **PROVEN** |
| Stop condition | `inc ebx` / `add edi, 72` / recount `(end-begin)/72` / `cmp ebx, eax` / `jb 004FDC00`. Stops when `ebx >= count`. TLC `count=399` → visits **`ebx=1..398`**. **No** `break` after first prox | **PROVEN** |
| First `004FBF60` file | **`LookoutPoint.tng`** (`ebx=1` = WLD `NewMap 1`) | **PROVEN** |
| Oakvale TNG on no-save first Present? | **NO.** First Present is Lookout ContainsMap + hero (or dummy empty). Not `StartOakValeWest` | **PROVEN** |
| Native first **open** of `StartOakValeWest.tng` | Same `004FDBC0` loop, **`004FBF60(ebx=203)`**, still Loading world, **before** first Present | **PROVEN** |
| Host leftover #50 opens Oakvale at this VA? | **No.** `break` after first prox (`LookoutPoint`) | **DIVERGE** / **LEFTOVER** |
| First **later construct** of that file | `006C2170` `"Loading objects"` when `NewRegion 4` `StartOakVale` is applied (`ContainsMap[0]` West). **Not** no-save first Present. Live region write **UNREAD** | **PROVEN** body; **UNREAD** as no-save first-seen |

**Native first map + ebx: `LookoutPoint` (NewMap 1), `ebx=1`.**
**Oakvale on first Present: NO.**
Leave leftover **#50** open.

---

## Verdict

**Open first file is MATCH. Pump width is DIVERGE.
Oakvale on first Present is DISPROVEN.**

Native `004FDBC0` starts `ebx=1`, walks every filled +
`LoadedOnPlayerProximity` slot through `ebx=count-1`,
and opens `LookoutPoint.tng` first. `StartOakValeWest`
is WLD `NewMap 203` / prox **TRUE**, so native
**does** `004FBF60(203)` inside the **same** pump.
That open is Loading world (`CurrentRegion` still
unset). It is **not** first Present.

Host `LoadGlobalThingsFile` `break`s on the first
prox map, Notes `"004FBF60 " + lookout.ScriptName +
".tng"`, sets `GlobalThingMapsLoaded = 1`. Real
reason in code: parsing every proximity `.tng` OOMs
the New Game pump. Tests lock count **1** + Lookout
in the Note, no Bowerstone Note
(`New_Game_004FDBC0_opens_LookoutPoint_only`). That
is a **host** lock, not a recovered Lookout-only
native walk, and **not** a recovered “Oakvale on
first Present” walk.

Do not treat leftover #50 as leftover #4.

---

## Evidence

### Listing `004FDBC0` (`listing-004c0000.txt`)

Caller `00509948` after `"Load global things"` when
`[0x13B8609]==0` (`.gtg` arm `004FE2A0` is **not**
no-save):

```
00509857  push -1
00509859  push "Load global things"
…
0050987B  mov al, [0x13B8609]
00509880  test al, al
00509882  je 00509946                 ; first-seen 0
…
00509946  mov ecx, ebx                ; CWorldMap
00509948  call 004FDBC0
```

Thiscall, `ecx=CWorldMap`, **no** stack args, `ret`.

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
004FDBEB  jbe 004FDCA3                  ; count<=1: dummy only
004FDBF2  mov edi, 0x48                 ; stride 72
loop:
004FDC00  push -1
          push "Loading global things"
          …
004FDC5D  mov eax, [esi+32]
004FDC60  mov cl, [eax+edi+36]          ; EndMap filled
004FDC64  add eax, edi
004FDC66  test cl, cl
004FDC68  je 004FDC79                   ; skip unfilled
004FDC6A  mov cl, [eax+40]              ; LoadedOnPlayerProximity
004FDC6D  test cl, cl
004FDC6F  je 004FDC79
004FDC71  push ebx                      ; native map index
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
| `ebx` start | **1** |
| `edi` start | **0x48** (72) |
| first slot | `begin + 72` = native index **1** |
| first `push ebx` | **1** |
| dummy 0 | never in this loop |
| stop | `ebx >= count` after `inc` |
| early `ret` after first prox | **none** |

Dummy skip is two gates: loop never loads slot 0,
and ctor dummy (`005066E0` / `00515AD0(1)`) has
`[+36]=0` even though `[+40]` defaults to 1.

### Listing `004FBF60` (`listing-004c0000.txt`)

| Slot | Value |
|---|---|
| `ecx` | `CWorldMap` |
| `[esp+4]` | map index (`ebx` from `004FDBC0`) |
| ret | `ret 4` |

```
004FBF60  sub esp, 8
004FBF64  mov esi, [esp+16]             ; map index
004FBF69  mov edi, ecx                  ; CWorldMap
004FBF6B  mov eax, [edi+32]
004FBF72  lea edx, [esi+esi*8]          ; index * 9
004FBF76  lea ecx, [eax+edx*8+24]       ; slot+24 script
004FBF7A  call 0099E480
004FBF81  lea ecx, [esp+20]
004FBF85  call 004FAFF0                 ; append 0x12442C4 ".tng"
          …
004FBFD4  push 28                       ; first-seen [map+168]==0
004FBFED  call 0099AD80                 ; CreateFileW
004FC023  call 005223F0                 ; gated construct
004FC04D  ret 4
```

`004FAFF0` pushes `0x12442C4` (host `TngExtVa`) then
`00997620` / `00997780` / `0099B720`. First name is
therefore **`LookoutPoint.tng`**. **PROVEN.**

Three `.text` `E8 004FBF60` sites. Only the first is
this leftover:

| Site | Function | No-save first Present? |
|---|---|---|
| `004FDC74` | `004FDBC0` global prox walk | **open during Loading world**, not Present |
| `004FE128` | `004FE2A0` `.gtg` compile; `xor ebx, ebx` starts **0** | **DISPROVEN** (`[0x13B8609]==0`) |
| `00507059` | `00506F30` map-add after `[slot+36]=1` | **DISPROVEN** as New Game first-seen |

### WLD NewMap slots (TLC `FinalAlbion.wld`, no TNG parse)

Native table after parse: dummy 0 + `NewMap 1..398`
→ `count=399`. Loop `ebx=1..398`.

```
NewMap 1;
MapX 3232;
MapY 3488;
LevelName "FinalAlbion\LookoutPoint.lev";
LevelScriptName "LookoutPoint";
LoadedOnPlayerProximity TRUE;
EndMap;

NewMap 2;
LevelScriptName "PicnicArea";
LoadedOnPlayerProximity TRUE;
EndMap;
…
NewMap 203;
MapX 3456;
MapY 736;
LevelName "FinalAlbion\StartOakValeWest.lev";
LevelScriptName "StartOakValeWest";
LoadedOnPlayerProximity TRUE;
EndMap;
…
NewMap 398;
LevelScriptName "NorthernWastes3_Filler_09";
LoadedOnPlayerProximity FALSE;
EndMap;
```

`ebx` is the **map index**, not the ordinal of prox
opens. Non-prox slots still `inc ebx` and skip
`004FBF60`. Oakvale West is therefore
**`004FBF60(203)`**, not “the 203rd prox file.”
PicnicArea is the second taken `004FBF60` (`ebx=2`).
Do not parse the 151 files to name the rest.

`WorldSceneTests` already locks `FindMap("StartOakValeWest").Index == 203`
and `Maps[0].ScriptName == "LookoutPoint"` / `.Index==1`.
C# `World.Maps` has **no** dummy row (`Maps.Count==398`).

### First Present is not Oakvale

```
NewRegion 1;  RegionName "LookoutPoint";
  ContainsMap BowerstoneBridge / LookoutPoint / GuildExterior

NewRegion 4;  RegionName "StartOakVale";
  ContainsMap StartOakValeWest / MemorialGarden / StartOakValeEast
```

No-save first *applied* region (if `00501450` runs)
is `00500540(1,0,0)` Lookout. First *CThing* is
`BowerstoneBridge.tng` (`006C2170` pass 2). First
Present geometry is Lookout + `GuildArrivalHSP` /
mesh **4299** / `006B3FF0` FOV **70**, or dummy
empty. **PROVEN** (`leftover-4-collapse-audit`,
`first-region-after-leave`).

Oakvale intro view (`StartOakValeWest` /
`HerosOldHouse` / `CAM_OVIF_SHOT2` / kid **4300**)
is leftover **#4**, a different ledger.

`004FDBC0` open ≠ Present. Native would **parse**
Oakvale West during Loading world. That still:

- does not Present Oakvale;
- does not construct first Present CThings
  (`005223F0` `[manager+128]==1` live **UNREAD**;
  first `0051FD80` after dummy is Bridge);
- does not run `00DBDE40` / `Q_NewOakValeIntro`.

---

## Original

```
00416953  Loading world
  00507C30
    NewMap 1   LookoutPoint     ebx slot 1   prox TRUE
    NewMap 2   PicnicArea       ebx slot 2   prox TRUE
    …
    NewMap 203 StartOakValeWest ebx slot 203 prox TRUE
    NewRegion 1 LookoutPoint    ContainsMap Bridge/Lookout/Guild
    NewRegion 4 StartOakVale    ContainsMap West/Garden/East
    00509859  Load global things
      [0x13B8609]==0
      004FDBC0
        ebx=1  004FBF60 → LookoutPoint.tng     ← FIRST OPEN
        ebx=2  004FBF60 → PicnicArea.tng
        …
        ebx=203 004FBF60 → StartOakValeWest.tng ← SAME PUMP
        ebx=204..398  filled+prox only
        stop ebx>=399
    Set Static Map                              AFTER
004189C2  dummy pumps  index 0                  0 TNG apply
later 00501450 → 00500540(1,0,0) Lookout
  006C2170 Loading objects
    BowerstoneBridge.tng                        first 0051FD80
    LookoutPoint.tng                            reopen construct
    GuildExterior.tng
first Present: Lookout                          NOT Oakvale
later 006C2170 when region 4 is current
  Loading objects StartOakValeWest.tng          first Oakvale construct
```

Native **first open** of `StartOakValeWest.tng` is
`004FDC74` / `004FBF60(203)` inside `004FDBC0`.
That is the first **later** site after
`LookoutPoint.tng` that opens this file. It is
**not** first Present and **not** `00501450`.

Native **first construct** of that file is later
`006C2170` `"Loading objects"` for `NewRegion 4`
`ContainsMap[0]`. Live no-save never applies that
region on the recovered tree (region writer
**UNREAD**; leftover #4 stays open).

---

## Host

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

`PresentWorld` uses `FirstSceneMapName` /
`CurrentRegion.ContainsMaps` / `_regionThings`.
`Pump` never calls `LoadFromFirstRealRegion`.
`FirstSceneWorld.Build` (Oakvale soup) has **zero**
callers in `Fable.Client` / `EngineLifecycle`.
No-save first Present therefore does **not**
`TryLoadThings("StartOakValeWest")`. **MATCH** skip
vs “must be NO.”

`LoadRegionMapThings` would open that file if
region 4 were applied. Live Pump does not.

---

## Gap

| Site | Original | Host | Class |
|---|---|---|---|
| `ebx` start | **1** | C# `Maps[0]` (no dummy) | **MATCH** numbering |
| Dummy slot 0 | skipped | absent | **MATCH** skip |
| First file | `004FBF60(1)` → `LookoutPoint.tng` | `TryLoadThings(first.ScriptName)` | **MATCH** first name on TLC |
| Stop | `ebx>=399`; walk **1..398** | **`break`** after first prox | **DIVERGE** leftover **#50** |
| Maps parsed this VA | 151 prox | **1** | **DIVERGE** |
| `StartOakValeWest.tng` this VA | **`004FBF60(203)`** | **not opened** | **DIVERGE** |
| Oakvale TNG on first Present | **NO** (Lookout / dummy) | **NO** | **MATCH** skip |
| First later **open** of West | same `004FDBC0`, `ebx=203` | none on this VA | host **LEFTOVER** |
| First later **construct** of West | `006C2170` region 4 | `LoadRegionMapThings` if applied | **UNREAD** as no-save |
| Name lock | native index 1 / 203 | first prox in WLD order | **PARTIAL** (TLC coincides) |
| OOM if 151 parsed | native opens (construct gated) | host `ThingFile.Parse` OOM | host reason **PROVEN**; not native width |
| Comment “later maps stay closed until `00501450`” | **DISPROVEN** as this pump (they **open** here); **PROVEN** as region *apply* | host excuse for `break` | **DIVERGE** |

Lookout-only is **host**. Do not claim it is native
`004FDBC0`. Do not claim native skip of Oakvale at
this VA. Do not claim first Present opens Oakvale.

`004FDBC0-host-leftover` leftover is **construct skip**
(`005223F0` `[manager+128]==1`). Different leftover
from pump **width**. #50 is the `break`. Do not
collapse them.

---

## Not leftover #4 / not NewMap-1 lock

| Claim | Class |
|---|---|
| First *open* is LookoutPoint | **PROVEN** (this VA) |
| Native `ebx` start **1**, stop `ebx>=count` | **PROVEN** |
| First *rendered* region is LookoutPoint | leftover **#4** |
| Oakvale intro view | leftover **#4** (`FIRST_SCENE_*`) |
| Oakvale TNG on no-save first Present | **DISPROVEN** |
| Host `break` recovers native “only NewMap 1” | **DISPROVEN** |
| Host `break` recovers “no Oakvale until Present” | **DISPROVEN** as native (native opens West at `ebx=203` *before* Present) |
| Tests lock `ebx=1` / `004FBF60` callee / `00501450` | **DISPROVEN** (they lock Note text + count 1) |
| First-proximity TNG pump | leftover **#50** — leave open |

---

## Do not

- Fold #50 into #4.
- Claim Lookout-only is native `004FDBC0`.
- Parse every proximity TNG to prove first name or
  Oakvale index.
- Open `StartOakValeWest.tng` on no-save first Present
  to “close” #50.
- Start host at `Maps[1]` to skip a dummy the C# list
  does not have.
- Treat host `GlobalThingMapsLoaded == 1` as a recovered
  NewMap-1 lock.
- Call first **open** the first **CThing** or first
  Present (Bridge after `00501450` / `006C2170`; Present
  is Lookout).
- Bind Oakvale / `00DBDE40` / kid `CREATURE_HERO_CHILD`
  onto this pump.
- Call `00507059` / `004FE128` the New Game opener.
