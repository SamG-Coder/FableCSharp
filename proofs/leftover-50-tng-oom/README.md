# Leftover #50: TNG OOM / `ebx` leftover on first-seen Present

Investigation only. Production `src/` was not edited.

Do **not** parse every `LoadedOnPlayerProximity` `.tng`
to “close” leftover **#50**. Census 151 / ~21746 is
already locked in `proofs/004FDBC0-vs-host`. Host
`ThingFile.Parse` of that set **OOMs** the New Game
pump. That is the leftover. Do **not** invent TNG fill.

Do **not** fold leftover **#4** (Lookout first *rendered*
scene vs Oakvale intro *view*) into this leftover. #50
is the **global TNG pump** (`004FDBC0` / host
`LoadGlobalThingsFile` first-prox `break`).

Do **not** start at Oakvale / `00DBDE40` / `StartOakVale`.
No-save New Game is Leave `0042F2A2` → `FinalAlbion.wld`
→ Loading world `004A1840` → `00507C30` → `00509859`
→ `004FDBC0`. First Present is dummy region index **0**.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: what TNG OOM / `ebx` leftover remains on
first-seen? Does `StartOakVale` TNG **construct** on
no-save first Present (expected **no** — dummy region)?

Dump: `004FDBC0`, `004FBF60`, `ebx=1..n`, NewMap slot,
`LookoutPoint.tng` first, `StartOakValeWest` `ebx=203`.
Host leftover **#50**. First Present dummy
`004189C2` / `004FC180` index 0.

Authority: `listing-004c0000.txt` (`004FDBC0` /
`004FBF60` / `004FAFF0`), `listing-00500000.txt`
(`00507C30` / `00509859` / `005223F0` / `006C2170`),
TLC `FinalAlbion.wld` (`NewMap 1` / `NewMap 203` /
`NewRegion 1` / `NewRegion 4`),
`EngineLifecycle.LoadGlobalThingsFile` /
`PresentWorld` / `PumpGame` / `ActivateCurrentRegion` /
`LoadRegionMapThings` / `ApplyLoadJob` (read only).
Siblings: `proofs/leftover-50-004FDBC0`,
`proofs/leftover-50-tng-ebx`,
`proofs/004FDBC0-open`, `proofs/004FDBC0-vs-host`,
`proofs/004FDBC0-host-leftover`,
`proofs/host-tng-construct-early`,
`proofs/dummy-pumps-host-leftover`,
`proofs/first-region-after-leave`,
`proofs/startoakvale-index4-loader`,
`proofs/leftover-4-collapse-audit`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Native first-seen `ebx` | **`1`** (`004FDBDE  mov ebx, 1`) | **PROVEN** |
| Dummy map slot 0 in `004FDBC0`? | **Never.** `edi=0x48`, first slot `begin+72` | **PROVEN** |
| First `004FBF60` file | **`LookoutPoint.tng`** (`ebx=1` = WLD `NewMap 1`) | **PROVEN** / **MATCH** |
| Native stop after that first prox? | **No.** `inc ebx` / `add edi, 72` / recount / `cmp ebx, eax` / `jb 004FDC00`. TLC `count=399` → **`ebx=1..398`**. **No** `break` | **PROVEN** |
| Host leftover that remains | **`break` after first prox.** Notes `"004FBF60 LookoutPoint.tng"`, `GlobalThingMapsLoaded = 1`, `"004FDC00 leftover host break ebx=2..{prox} unparsed={prox-1}"`. Reason: `ThingFile.Parse` of 151 files OOMs New Game | **LEFTOVER** / **DIVERGE** |
| Native `StartOakValeWest.tng` this VA | **`004FBF60(ebx=203)`** during Loading world, **before** first Present | **PROVEN** open; **not** Present |
| Host opens West at this VA? | **No.** `break` after Lookout | **DIVERGE** leftover **#50** |
| No-save first Present region | **Dummy index 0.** `WorldMap+156=0`, `004FC180` `[record+36]=0`, `CurrentRegion=null` | **PROVEN** |
| `StartOakVale` TNG **construct** on no-save first Present? | **NO.** Dummy apply has **0** ContainsMap / **0** `006C2170` objects. Expected | **DISPROVEN** / **MATCH** skip |
| Invent TNG fill (151 files or Oakvale on dummy) to close #50? | **No.** OOM is the leftover. Dummy Present is empty without fill | **DISPROVEN** as work |

**Native first map + ebx: `LookoutPoint` (NewMap 1), `ebx=1`.**
**OOM leftover remaining: host `ebx=2..prox` unparsed.**
**StartOakVale construct on first Present: NO (dummy).**
Leave leftover **#50** open.

---

## Verdict

**Open first file is MATCH. Pump width is DIVERGE (OOM).
StartOakVale construct on first Present is DISPROVEN.**

Native `004FDBC0` starts `ebx=1`, walks every filled +
`LoadedOnPlayerProximity` slot through `ebx=count-1`,
and opens `LookoutPoint.tng` first. `StartOakValeWest`
is WLD `NewMap 203` / prox **TRUE**, so native **does**
`004FBF60(203)` inside the **same** pump. That open is
Loading world (`CurrentRegion` still dummy / unset).
It is **not** first Present and **not** `006C2170`
construct.

Host `LoadGlobalThingsFile` `break`s on the first prox
map because parsing every proximity `.tng` OOMs the
New Game pump. Tests lock count **1** + Lookout in the
Note, plus the leftover-break Note, no Bowerstone Note
(`New_Game_004FDBC0_opens_LookoutPoint_only`). That is
a **host** lock, not a recovered Lookout-only native
walk, and **not** a recovered “Oakvale on first Present”
walk.

First-seen Present is dummy `004189C2`. Dummy region
does not construct TNG. Do not invent fill of the 150
skipped files, and do not invent `StartOakValeWest`
construct on that frame, to “close” #50.

Do not treat leftover #50 as leftover #4.
Do not collapse #50 into `004FDBC0-host-leftover`
(`005223F0` `[manager+128]==1` construct skip). #50
is the **`break`**.

---

## What leftover remains on first-seen

Three layers. Do not collapse them.

| Layer | First-seen | Leftover remaining? |
|---|---|---|
| **`ebx` start / dummy map skip / first name** | `ebx=1`, slot 0 never pushed, `LookoutPoint.tng` | **None.** **MATCH** |
| **Pump width (`004FDBC0` loop)** | native `ebx=1..398` / 151 prox opens; host **1** file | **Yes — leftover #50.** Host OOM `break`. **DIVERGE** |
| **First Present construct** | dummy index 0, `CurrentRegion=null`, 0 TNG apply | **None on Oakvale.** **MATCH** skip. Width leftover does **not** change dummy empty |

The leftover that **remains** is only the host width
cut: `ebx=2..prox` unparsed. Constants already name it
(`LoadGlobalThingsEbxStart = 1`,
`StartOakValeWestTngEbx = 203`,
`LoadGlobalThingsHostBreaksAfterFirstProx = true`).
First-seen Present does **not** consume those unparsed
files. Dummy `PresentWorld` returns **null** when
`CurrentRegion is null`. `SubmitCurrentWorld` also
requires `HeroSpawned`.

`004FDBC0-host-leftover` leftover (native dump-static
`+128==1` → taken `00521AE0` / `0051FD80` during this
open) is a **different** leftover. Live `[manager+128]`
at first `005223F7` is **UNREAD**. Even if that arm
were live, stuffing 21746 Things into dummy Present is
**DISPROVEN** as first-scene C3Ds. Do not invent that
fill here.

---

## Native `004FDBC0` (`listing-004c0000`) — width, not Present

Caller `00509948` after `"Load global things"` when
`[0x13B8609]==0` (`.gtg` arm `004FE2A0` is **not**
no-save):

```
004FDBC0  sub esp, 8
004FDBC5  mov esi, ecx                  ; CWorldMap
004FDBC7  mov eax, [esi+32]             ; map-table begin
004FDBCA  mov ecx, [esi+36]             ; end
          count = (end-begin)/72
004FDBDE  mov ebx, 1                    ; FIRST-SEEN ebx
004FDBE5  cmp eax, ebx
004FDBEB  jbe 004FDCA3                  ; count<=1: dummy only
004FDBF2  mov edi, 0x48                 ; stride 72
loop:
004FDC00  push -1
          push "Loading global things"
          …
004FDC60  mov cl, [eax+edi+36]          ; EndMap filled
004FDC6A  mov cl, [eax+40]              ; LoadedOnPlayerProximity
004FDC71  push ebx
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
| dummy map 0 | never in this loop |
| stop | `ebx >= count` after `inc` |
| early `ret` after first prox | **none** |

`004FBF60(ebx)` builds `LevelScriptName + ".tng"`
(`004FAFF0` / `0x12442C4`). First name is
**`LookoutPoint.tng`**. Oakvale West is **`ebx=203`**.

`ebx` is the **map index**, not the ordinal of prox
opens. Non-prox slots still `inc ebx` and skip
`004FBF60`. Do not parse the 151 files to name the rest.
Census is already locked.

That whole walk finishes **inside** `00507C30`, **before**
Set Static Map, **before** dummy `004189C2`. Open ≠
Present.

---

## Host OOM `break` — leftover #50

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
No `LoadSingleThing`. No `_regionThings`. Parse+store
into `GlobalThings` section `GLOBAL` only.

| Site | Original | Host | Class |
|---|---|---|---|
| `ebx` start | **1** | C# `Maps[0]` (no dummy) | **MATCH** numbering |
| Dummy map slot 0 | skipped | absent | **MATCH** skip |
| First file | `004FBF60(1)` → `LookoutPoint.tng` | `TryLoadThings(first.ScriptName)` | **MATCH** first name on TLC |
| Stop | `ebx>=399`; walk **1..398** | **`break`** after first prox | **DIVERGE** leftover **#50** |
| Maps parsed this VA | 151 prox | **1** | **DIVERGE** |
| OOM if 151 parsed | native opens (construct gated) | host `ThingFile.Parse` OOM | host reason **PROVEN**; not native width |
| `StartOakValeWest.tng` this VA | **`004FBF60(203)`** | **not opened** | **DIVERGE** |
| Invent 151-file fill | native width | would OOM New Game | **DISPROVEN** as host work |

Lookout-only is **host**. Do not claim it is native
`004FDBC0`. Do not parse the remaining prox set to
recover width. The OOM **is** leftover #50.

---

## First Present is dummy — no StartOakVale construct

Expected: **no**. Recovered tree matches.

### Native dummy `004189C2`

After Loading world `004BBC00 ret 4` / Init Game suffix:

```
004189C2  first pumps
  WorldMap+156 = 0
  004FC180  dummy 88-byte slot  (005066E0)
  [record+36] = 0
  CurrentRegion unset
  no 00501450 / 00500540 / 006C27A0 / 006C2170
  0 TNG apply
```

`ActivateCurrentRegion` on index 0 Notes
`"index=0 dummy 005066E0 record+36 null"` and
**returns**. Dummy has no `ContainsMap`. Type-1 later
inners still sit on index 0 (`dummy-pumps-host-leftover`
**MATCH** skip of `00501450`).

`006C2170` `"Loading objects"` is the construct walk.
It runs only from a real `00500540` apply job. Dummy
index 0 is **not** that job.

### Host first Present

`PumpGame` first `004189C2`:

```
CurrentRegionIndex ctor 0
ActivateCurrentRegion skip (index 0)
ApplyFirstPumpAviAndFade
PumpGameUpdate
  SubmitCurrentWorld only if HeroSpawned && CurrentRegion != null
```

`PresentWorld`:

```
if (Install is null || CurrentRegion is null)
    return null;
```

Live client `Pump` never calls `LoadFromFirstRealRegion`
/ `EnqueueAfterDummy`. `FirstSceneWorld.Build` (Oakvale
soup) has **zero** callers in `Fable.Client` /
`EngineLifecycle`. Tests after `RequestNewGame` +
`EnterGame` + first `Pump`:

- `CurrentRegionIndex == 0`
- `CurrentRegion == null`
- `ActivatedMaps` empty
- no `SetRegionAsLoaded`
- no `00DBDE40`
- `GlobalThingMapsLoaded == 1` (Lookout only)

Dummy Present is **empty**. Not Lookout geometry, not
Oakvale geometry. Later Lookout submit (if `00501450`
runs) is leftover **#4**, a different ledger.

### `StartOakVale` TNG construct — three senses

Do not collapse open / parse / construct / Present.

| Sense | No-save first Present | Class |
|---|---|---|
| **File I/O** of `StartOakValeWest.tng` | native: `004FBF60(203)` during Loading world, **before** Present. Host: **not opened** (`break`) | native open **PROVEN**; host **DIVERGE** #50; **not** Present |
| **Token parse** | host never `TryLoadThings("StartOakValeWest")` on this walk. Native `00521AE0` iff `+128==1` at that open — live **UNREAD** | **DISPROVEN** as first Present |
| **CThing construct** (`0051FD80` / `LoadSingleThing`) | dummy apply: **none**. First later construct of West is `006C2170` when `NewRegion 4` is applied | **DISPROVEN** as first Present; later site **UNREAD** as no-save |
| **Present geometry** | dummy `PresentWorld` **null**. West ContainsMap is region 4, not index 0 | **DISPROVEN** |

WLD:

```
NewRegion 1;  RegionName "LookoutPoint";
  ContainsMap BowerstoneBridge / LookoutPoint / GuildExterior

NewRegion 4;  RegionName "StartOakVale";
  ContainsMap StartOakValeWest / MemorialGarden / StartOakValeEast
```

Region 4 is table-only during `00507C30`. Nobody on
no-save first Present writes `WorldMap+156=4`
(`startoakvale-index4-loader`). Childhood `00DBDE40`
**waits** on `"StartOakVale"`; it does **not** load
TNG. Persist `PlayerRegionName` is empty.

`LoadRegionMapThings` would parse West **if** region 4
were applied. Live Pump does not. Do not invent that
apply to close #50.

---

## Original (no-save first-seen)

```
00416953  Loading world
  00507C30
    NewMap 1   LookoutPoint      ebx slot 1    prox TRUE
    NewMap 203 StartOakValeWest  ebx slot 203  prox TRUE
    NewRegion 1 LookoutPoint
    NewRegion 4 StartOakVale     table only
    00509859  Load global things
      [0x13B8609]==0
      004FDBC0
        ebx=1    004FBF60 → LookoutPoint.tng      ← FIRST OPEN
        ebx=2..  filled+prox
        ebx=203  004FBF60 → StartOakValeWest.tng  ← SAME PUMP, NOT PRESENT
        stop ebx>=399
        host: break after ebx=1                   ← LEFTOVER #50 OOM
    Set Static Map                                AFTER
004189C2  dummy pumps  index 0                    0 TNG apply
          PresentWorld CurrentRegion=null         EMPTY
          StartOakVale TNG construct              NO
later 00501450 → 00500540(1,0,0) Lookout          leftover #4 Present
  006C2170 Loading objects
    BowerstoneBridge.tng                          first 0051FD80
    LookoutPoint.tng                              reopen construct
later 006C2170 when region 4 is current
  Loading objects StartOakValeWest.tng            first Oakvale construct
```

---

## Gap

| Site | Original | Host | Class |
|---|---|---|---|
| First-seen `ebx` / first name | `1` / LookoutPoint | same on TLC | **MATCH** |
| Pump width | 151 prox opens | **1** (`break`) | **DIVERGE** leftover **#50** |
| OOM reason | native width does not OOM this way | `ThingFile.Parse` 151 files | host **PROVEN**; not native |
| West **open** this VA | `ebx=203` | skipped | **DIVERGE** |
| Dummy first Present | index 0, 0 TNG apply | same | **MATCH** |
| West **construct** on first Present | **NO** | **NO** | **MATCH** skip |
| First later West **construct** | `006C2170` region 4 | `LoadRegionMapThings` if applied | **UNREAD** as no-save |
| Invent 151-file fill | native width | OOM | **DISPROVEN** as work |
| Invent West construct on dummy | **no** | **no** | **DISPROVEN** as work |
| `+128` construct during `004FDBC0` | dump-static taken; live **UNREAD** | no `LoadSingleThing` | **different leftover** (`004FDBC0-host-leftover`) |
| `GlobalThings` → dummy Present | **DISPROVEN** as first-scene C3Ds | no leak to `_regionThings` | **MATCH** skip |

Leave leftover **#50** open as the **`break`**. Dummy
Present skip of Oakvale TNG construct is **not** the
open item.

---

## Not leftover #4 / not TNG fill

| Claim | Class |
|---|---|
| First *open* is LookoutPoint | **PROVEN** (this VA) |
| Host `break` is leftover #50 OOM workaround | **PROVEN** |
| Host `break` recovers native “only NewMap 1” | **DISPROVEN** |
| First *rendered* region is LookoutPoint | leftover **#4** (after dummy, if `00501450` runs) |
| Oakvale intro view | leftover **#4** (`FIRST_SCENE_*`) |
| StartOakVale TNG construct on no-save first Present | **DISPROVEN** (dummy) |
| Fill 151 prox files to MATCH native width | leftover **#50** — **do not invent**; OOM |
| Open `StartOakValeWest.tng` on dummy Present to close #50 | **DISPROVEN** as work |
| `005223F0` construct skip | **different leftover** — do not collapse |

---

## Do not

- Fold #50 into #4.
- Parse every proximity TNG (OOM). Census is locked.
- Invent TNG fill of `ebx=2..prox` to “close” #50.
- Invent `StartOakValeWest` construct / `LoadSingleThing`
  / `_regionThings` on no-save first Present.
- Call dummy empty Present a Lookout or Oakvale scene.
- Treat host `GlobalThingMapsLoaded == 1` as a recovered
  NewMap-1 lock.
- Claim Lookout-only is native `004FDBC0`.
- Collapse first **open** (Lookout / native West at
  `ebx=203`) with first **CThing** (Bridge after
  `00501450`) or first Present (dummy).
- Bind Oakvale / `00DBDE40` / kid `CREATURE_HERO_CHILD`
  onto this pump.
- Wire `EnqueueAfterDummy` / `LoadFromFirstRealRegion` /
  `FirstSceneWorld.Build` onto `Pump` to force region 4.
- Collapse #50 (`break`) into `004FDBC0-host-leftover`
  (`+128` construct skip).
