# Leftover #50 remaining: first-proximity TNG is host OOM, not `ebx=1` / NewMap / `00501450`

Investigation only. Production `src/` and `tests/` were
not edited.

Do **not** parse every `LoadedOnPlayerProximity` `.tng`
(host OOM). Census 151 / ~21746 is already locked in
`proofs/004FDBC0-vs-host`.

Do **not** invent persist `PlayerRegionName` as a no-save
current (`PlayerRegionNameWrittenOnNewGame` is already
`false`). Do **not** call `LoadFromFirstRealRegion` from
dummy `Pump` / `PumpGame`.

Do **not** fold leftover **#4** (Lookout first *rendered*
scene vs Oakvale intro *view*) into this leftover. #50
is the **global TNG pump** (`004FDBC0` / host
`LoadGlobalThingsFile` first-prox `break`).

Do **not** start at Oakvale / `00DBDE40` / `StartOakVale`.
No-save New Game is Leave `0042F2A2` → `FinalAlbion.wld`
→ Loading world `004A1840` → `00507C30` → `00509859`
→ `004FDBC0`. First no-save *region* is LookoutPoint
native index **1** (**PROVEN**). First dummy Present is
index **0**.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: after recovering native `ebx=1` / NewMap slot,
what leftover **remains**? Is first-proximity TNG a
locked native `004FDBC0` NewMap-1 walk, or a host OOM
workaround? Does it lock `00501450`?

Authority: `proofs/leftover-50-*`,
`proofs/00501450-e8-callers`,
`proofs/00501450-host-leftover`,
`proofs/004FDBC0-open`;
`listing-00500000.txt` (`00501450` / `005014EC`
`00500540(i,0,0)` first `i=1`);
`listing-004c0000.txt` (`004FDBC0` / `004FBF60`);
`EngineLifecycle.LoadFromFirstRealRegion` /
`LoadGlobalThingsFile` / `Pump` / `PumpGame` /
`EnqueueAfterDummy` (read only).
Siblings: `proofs/leftover-50-native-ebx`,
`proofs/leftover-50-004FDBC0`,
`proofs/leftover-50-tng-ebx`,
`proofs/leftover-50-tng-oom`,
`proofs/leftover-50-lazy-parse`,
`proofs/004FDBC0-vs-host`,
`proofs/004FDBC0-host-leftover`,
`proofs/first-region-after-leave`,
`proofs/wld-first-region`,
`proofs/lookout-newmap-without-00501450`,
`proofs/current-region-no-save`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Native first-seen `ebx` recovered? | **Yes. `1`.** `004FDBDE  mov ebx, 0x1`. Dummy slot 0 never pushed (`edi=0x48`). | **PROVEN** (`leftover-50-native-ebx`) |
| First NewMap slot recovered? | **Yes. WLD `NewMap 1` `LookoutPoint`.** First `004FBF60` is `LookoutPoint.tng`. | **PROVEN** (`004FDBC0-open`) |
| Native walk only that first prox? | **No.** Bound `ebx=1..count-1` (TLC **`1..398`**). 151 filled+prox opens. **No** `break`. | **PROVEN** |
| Host first-proximity TNG remaining? | **`break` after first prox.** Notes `"004FBF60 LookoutPoint.tng"`, `GlobalThingMapsLoaded = 1`, leftover-break Note. Reason: `ThingFile.Parse` of 151 OOMs New Game. | **LEFTOVER** / **DIVERGE** |
| That `break` a recovered NewMap-1 / `ebx=1` lock? | **No.** Constants name native start (`LoadGlobalThingsEbxStart=1`) and the **host** cut (`LoadGlobalThingsHostBreaksAfterFirstProx=true`). Tests lock count **1** + Lookout Note, not native width. | **DISPROVEN** as native skip |
| That `break` a recovered `00501450` lock? | **No.** `00501450` is later region enqueue. Comment in `LoadGlobalThingsFile` already says the `break` is **not** `00501450`. | **DISPROVEN** |
| First no-save region? | **LookoutPoint native index 1.** `NewRegion 1`. If `00501450` ran: first `00500540(1,0,0)`. Dummy current stays **0**. | **PROVEN** |
| Invent `PlayerRegionName` as no-save current? | **No.** Empty; `PlayerRegionNameWrittenOnNewGame=false`. Persist is `00487C20` continue. | **DISPROVEN** |
| Dummy `Pump` → `LoadFromFirstRealRegion`? | **No.** `Pump` / `PumpGame` never call it. `PumpCallsLoadFromFirstRealRegion=false`. Named inbound **0**. | **PROVEN** skip; do **not** wire |

**Recovered: native `ebx=1` / NewMap 1 / first region Lookout index 1.**
**Remaining leftover #50: host first-prox TNG `break` (OOM).**
**Not remaining as unread: NewMap slot, `ebx` start, Lookout-first name.**
Leave leftover **#50** open as the **width cut**.

---

## Verdict

**Native `ebx=1` / NewMap slot are recovered. First-proximity
TNG on the host is still an OOM workaround, not those locks,
and not `00501450`.**

Siblings already locked the dump:

- `004FDBC0` starts `ebx=1`, skips dummy 0, first
  `004FBF60` is **NewMap 1** `LookoutPoint.tng`, then
  walks **every** filled + `LoadedOnPlayerProximity`
  slot through `ebx=count-1`.
- First no-save *authored region* is **LookoutPoint**
  index **1**. `00501450` body first apply is
  `00500540(1,0,0)`. Inbound `E8` of that fn is **0**.
  Dummy pumps do not enter it.

Host `LoadGlobalThingsFile` still parses **one** prox
file because parsing 151 `ThingFile`s OOMs the New Game
pump. `New_Game_004FDBC0_opens_LookoutPoint_only` locks
that count **1** + Lookout in the Note + leftover-break
Note, no Bowerstone Note. That is a **host** lock, not a
recovered Lookout-only native walk, not a recovered
NewMap-1 skip, and not a recovered `00501450` site.

Do not treat leftover #50 as leftover #4.
Do not close #50 by wiring dummy `Pump` to
`LoadFromFirstRealRegion`.
Do not invent `PlayerRegionName`.

---

## Recovered — native `ebx=1` / NewMap slot (not remaining)

`listing-004c0000.txt` `004FDBC0`:

```
004FDBC0  sub esp, 8
004FDBC5  mov esi, ecx                  ; CWorldMap
          count = ([esi+36]-[esi+32])/72
004FDBDE  mov ebx, 0x1                  ; RECOVERED first-seen ebx
004FDBE5  cmp eax, ebx
004FDBEB  jbe 004FDCA3                  ; count<=1: dummy only
004FDBF2  mov edi, 0x48                 ; stride 72
loop:
004FDC00  …
004FDC60  test [begin+edi+36]           ; EndMap filled
          je 004FDC79
004FDC6A  test [begin+edi+40]           ; LoadedOnPlayerProximity
          je 004FDC79
004FDC71  push ebx
004FDC74  call 004FBF60                 ; ret 4
004FDC90  inc ebx
004FDC93  add edi, 72
004FDC96  cmp ebx, eax                  ; recount
004FDC9C  jb 004FDC00                   ; ebx < count
004FDCA8  ret
```

| Slot | Recovered |
|---|---|
| `ebx` start | **1** |
| Dummy slot 0 | never pushed |
| First `push ebx` | **1** = WLD `NewMap 1` |
| First file | **`LookoutPoint.tng`** (`004FAFF0` `".tng"`) |
| Bound | **`ebx=1..count-1`** (TLC **`1..398`**) |
| Early `ret` after first prox | **none** |
| `StartOakValeWest.tng` this VA | **`004FBF60(203)`**, Loading world, **not** Present |

Host constants already name the recovered start, not the
cut as native:

```
LoadGlobalThingsEbxStart = 1
StartOakValeWestTngEbx = 203
LoadGlobalThingsHostBreaksAfterFirstProx = true   // leftover flag
```

C# `World.Maps` has **no** dummy (`Maps.Count==398`,
`Maps[0].Index==1`, `ScriptName=="LookoutPoint"`). First
prox in WLD order **is** Lookout on TLC. That coincidence
is **MATCH** first name, **not** a recovered “only NewMap 1”
filter. Host does **not** look up `map.Index == 1`.

`ebx` is the **map-table index**. Non-prox slots still
`inc ebx` and skip `004FBF60`. Census of taken opens is
151 (`004FDBC0-vs-host`). Do **not** parse those files
here.

---

## Remaining leftover — host first-prox `break` (OOM)

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

`New_Game_004FDBC0_opens_LookoutPoint_only` locks:

- `GlobalThingMapsLoaded == 1`
- PerMap Note contains `LookoutPoint`
- MapFile Note contains `004FDC00 leftover host break`
- PerMap Note does **not** contain `Bowerstone`

It does **not** lock NewMap index, native width, `004FBF60`
as a callee walk, or `00501450`. `Gtng_is_stem_gtng_*`
locks `LoadGlobalThingsEbxStart==1` and
`LoadGlobalThingsHostBreaksAfterFirstProx==true` as
**named** dump vs leftover, not as “host width MATCH.”

| Site | Native `004FDBC0` | Host today | Class |
|---|---|---|---|
| `ebx` start / dummy skip / first name | **1** / skip 0 / Lookout | C# `Maps[0]` / absent dummy / first prox | **MATCH** recovered |
| Stop | `ebx>=399`; 151 opens | **`break` after first prox** | **DIVERGE** leftover **#50 remaining** |
| OOM if 151 parsed | native CRT open; construct gated | `ThingFile.Parse` ~21746 | host reason **PROVEN**; not native width |
| `StartOakValeWest.tng` this VA | `ebx=203` | not opened | **DIVERGE** width |
| `GlobalThingMapsLoaded==1` | native would be 151 | host lock | **DIVERGE**; not a recovered NewMap-1 lock |

`leftover-50-lazy-parse` already checked subset /
lazy / current+adj: each **invents a skip**. Full parse
OOMs. **No smallest MATCH change remains.** Leave #50
open as the `break`.

`004FDBC0-host-leftover` leftover is **construct skip**
(`005223F0` `[manager+128]==1`). Different leftover from
pump **width**. Do not collapse them.

---

## Not `00501450` — first no-save region is Lookout index 1

`00501450` is **region enqueue**, not the global TNG pump.
`listing-00500000.txt`:

```
00501450  push ebp
          ecx = CWorldMap
0050146B  call 00449970
00501472  call 00487DC0
00501495  mov esi, [edi+156]            ; saved current
005014A3  call 004FEEC0(esi, 0)         ; +156=0
          count = ([edi+48]-[edi+44])/88
005014CE  mov [esp+24], 0x1             ; i = 1
          jbe 005018F8                  ; count<=1
005014EC  call 00500540(i, 0, 0)        ; FIRST i=1 LookoutPoint
          …
          inc i; jb 005014E3
00501935  call 00500540(saved, 0, 1)    ; restore, no pump
00501985  ret
```

| Claim | Class |
|---|---|
| First body apply is index **1** LookoutPoint | **PROVEN** (`wld-first-region`, `first-region-after-leave`) |
| No-save first *named* region is `NewRegion 1` Lookout | **PROVEN** (WLD bytes) |
| Inbound `E8` / `imm` / `calls-by-dest` of `00501450` | **0** (`00501450-e8-callers`) |
| Dummy `Pump` / `PumpGame` call this fn? | **No.** `LoadFromFirstRealRegionNamedInbound=0`. `PumpCallsLoadFromFirstRealRegion=false`. | **MATCH** skip |
| Host `LoadFromFirstRealRegion` body first `00500540(1,0,0)` | **MATCH** body; **DIVERGE** *when* (tests / `EnqueueAfterDummy` only) |
| First-prox TNG `break` recovered as this fn | **DISPROVEN** |
| Comment “later maps stay closed until `00501450`” | **DISPROVEN** as `004FDBC0` (they **open** here); **PROVEN** as region *apply* `006C2170` |

`Pump` (`00412F90` / Game) → `PumpGame` (`004189C2`) →
`ActivateCurrentRegion` on ctor index **0**. No
`LoadFromFirstRealRegion`. `EnqueueAfterDummy` exists as
leftover glue and is unused by live `Pump`. Do **not**
wire it to close #50.

Open ≠ apply ≠ Present:

```
00416953  Loading world
  00507C30
    NewMap 1     LookoutPoint            ebx slot 1
    NewRegion 1  LookoutPoint            native index 1
    00509859  Load global things
      004FDBC0  ebx=1..398 / 151 prox    ← #50 WIDTH
        host: break after first prox     ← REMAINING
  dummy 004189C2  +156=0                 0 TNG apply
later (E8 UNREAD; host stand-in only)
  00501450  00500540(1,0,0) Lookout      ← REGION, not TNG pump
    006C2170  Bridge / Lookout / Guild   first CThing
```

First CThing after dummy is **BowerstoneBridge**
(`004FDBC0-open`). First Present geometry (if the body
ran) is leftover **#4**. Dummy first Present is empty.
None of those close the `004FDBC0` width cut.

---

## Do not invent `PlayerRegionName` as no-save current

At `004FDBC0` time, `WorldMap+156` is still dummy **0**.
There is **no** current map for “only current+adj” without
inventing a write.

| Candidate | Class |
|---|---|
| Persist `PlayerRegionName` on New Game | **DISPROVEN** (`PlayerRegionNameWrittenOnNewGame=false`) |
| `00487C20` `LoadRegionByName` as this pump | **DISPROVEN** (continue; needs nonempty name) |
| `EnqueueAfterDummy` persist arm as no-save | **DISPROVEN** (empty; then would fall through to `00501450`) |
| Seed `"StartOakVale"` so region 4 is current | leftover **#4**; **DISPROVEN** as #50 work |
| Dummy `Pump` → `LoadFromFirstRealRegion` so “current” exists | **DISPROVEN**; host **MATCH** skip; do **not** wire |

`lookout-newmap-without-00501450`: NewMap 1 is WLD parse
during `00507C30`, **not** a `+156` write. Nobody recovered
sets current to Lookout without unread `00501450`. Dummy
current stays **0**. Do not invent a writer so a subset
TNG pump looks MATCH.

---

## Layers that remain distinct

| Layer | Recovered? | Remaining leftover? |
|---|---|---|
| Native `ebx=1` / dummy skip / NewMap 1 first file | **Yes.** **MATCH** | **None** |
| Native bound `1..count-1` / 151 prox opens | **Yes** as dump | Host **does not walk it** — **#50 remaining** |
| Host first-prox `break` / count 1 tests | named as leftover | **Yes — this leftover** |
| First no-save region Lookout index 1 | **Yes** (table + `00501450` body) | **None** on the name; live `E8` **UNREAD** |
| Dummy `Pump` skip of `00501450` | **Yes.** **MATCH** skip | Do not re-hook |
| `PlayerRegionName` no-save current | **DISPROVEN** invention | Do not invent |
| First Present Lookout vs Oakvale intro | leftover **#4** | **Different ledger** |
| `005223F0` `+128` construct during `004FDBC0` | dump-static vs live **UNREAD** | **Different leftover** (`004FDBC0-host-leftover`) |

---

## Gap

| Site | Original | Host | Class |
|---|---|---|---|
| `ebx` / NewMap 1 / first name | recovered | first prox on TLC | **MATCH** recovered |
| Pump width | 151 | **1** (`break`) | **DIVERGE** leftover **#50 remaining** |
| Tests | — | count 1 + Lookout Note + break Note | **host lock**, not native |
| `00501450` inbound | **0** | `Pump` skip | **MATCH** skip |
| First region if body ran | `00500540(1,0,0)` Lookout | same body API | **MATCH** body; **UNREAD** live site |
| Dummy first Present | index 0, 0 TNG apply | same | **MATCH** |
| Invent persist current | empty no-save | empty | **DISPROVEN** as work |
| Parse 151 to MATCH width | native opens | OOM | **DISPROVEN** as host work |

Leave leftover **#50** open as the **`break`**. Recovered
`ebx=1` / NewMap slot / Lookout-first name / Lookout
region index 1 are **not** the remaining item.

---

## Not leftover #4 / not NewMap-1 lock / not `00501450`

| Claim | Class |
|---|---|
| Native `ebx` start **1**, first file LookoutPoint | **PROVEN** recovered |
| Native loads **only** NewMap 1 | **DISPROVEN** |
| Host `break` is leftover #50 OOM workaround | **PROVEN** remaining |
| Host `break` recovers native “only NewMap 1” | **DISPROVEN** |
| Host `break` recovers `00501450` | **DISPROVEN** |
| Tests lock `ebx=1` / `004FBF60` callee walk / `00501450` | **DISPROVEN** (they lock Note text + count 1 + leftover flag) |
| First no-save region LookoutPoint index 1 | **PROVEN** |
| Dummy `Pump` → `LoadFromFirstRealRegion` | **DISPROVEN**; do **not** wire |
| Invent `PlayerRegionName` as no-save current | **DISPROVEN** |
| First *rendered* region is LookoutPoint | leftover **#4** |
| Oakvale intro view | leftover **#4** (`FIRST_SCENE_*`) |
| `005223F0` construct skip | **different leftover** |

---

## Do not

- Fold #50 into #4.
- Claim Lookout-only is native `004FDBC0`.
- Treat recovered `ebx=1` / NewMap 1 as the remaining leftover.
- Treat host `GlobalThingMapsLoaded == 1` as a recovered
  NewMap-1 lock.
- Treat the first-prox `break` as `00501450`.
- Call `LoadFromFirstRealRegion` from dummy `Pump` /
  `PumpGame` to “close” #50.
- Invent `PlayerRegionName` as a no-save current so a
  subset TNG walk looks MATCH.
- Parse every proximity TNG (OOM). Census is locked.
- Start host at `Maps[1]` to skip a dummy the C# list
  does not have.
- Collapse first **open** (Lookout; native also West at
  `ebx=203`) with first **CThing** (Bridge after
  `00501450` / `006C2170`) or first Present (dummy /
  leftover #4).
- Collapse #50 (`break`) into `004FDBC0-host-leftover`
  (`+128` construct skip).
- Bind Oakvale / `00DBDE40` / kid `CREATURE_HERO_CHILD`
  onto this pump.

---

## Files read

- `C:\FableCSharp\proofs\leftover-50-native-ebx\README.md`
- `C:\FableCSharp\proofs\leftover-50-004FDBC0\README.md`
- `C:\FableCSharp\proofs\leftover-50-tng-ebx\README.md`
- `C:\FableCSharp\proofs\leftover-50-tng-oom\README.md`
- `C:\FableCSharp\proofs\leftover-50-lazy-parse\README.md`
- `C:\FableCSharp\proofs\004FDBC0-open\README.md`
- `C:\FableCSharp\proofs\004FDBC0-vs-host\README.md`
- `C:\FableCSharp\proofs\004FDBC0-host-leftover\README.md`
- `C:\FableCSharp\proofs\00501450-e8-callers\README.md`
- `C:\FableCSharp\proofs\00501450-host-leftover\README.md`
- `C:\FableCSharp\proofs\first-region-after-leave\README.md`
- `C:\FableCSharp\proofs\wld-first-region\README.md`
- `C:\FableCSharp\proofs\lookout-newmap-without-00501450\README.md`
- `C:\FableCSharp\proofs\leftover-4-collapse-audit\README.md`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-004c0000.txt` (`004FDBC0` / `004FDBDE` `ebx=1` / `004FDC74`)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00500000.txt` (`00501450` / `005014CE` `i=1` / `005014EC` `00500540`)
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs` (`LoadGlobalThingsFile` / `LoadFromFirstRealRegion` / `EnqueueAfterDummy` / `Pump` / `PumpGame`; read only)
- `C:\FableCSharp\tests\Fable.Formats.Tests\FrontendLayoutTests.cs` (`New_Game_004FDBC0_opens_LookoutPoint_only`; read only)
- `C:\FableCSharp\tests\Fable.Formats.Tests\EngineLifecycleTests.cs` (`LoadGlobalThingsEbxStart` / `PumpCallsLoadFromFirstRealRegion`; read only)
- `C:\FableCSharp\docs\status\README.md` leftover #50 row
