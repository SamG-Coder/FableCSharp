# Who loads `StartOakVale` (WLD index 4) on the childhood path without `PlayerRegionName`

Investigation only. No production `src/` or `tests/` edits.

Do **not** invent a New Game write of persist `PlayerRegionName`.
That key is save-only `00449F90`; empty on no-save. Do **not**
collapse leftover **#4**: no-save first Present is LookoutPoint
native index **1**, not Oakvale.

Question: who loads `StartOakVale` (WLD `NewRegion 4`) on the
childhood path **without** `PlayerRegionName`? If nobody on
no-save first Present, document that and the first later site
that would open Oakvale.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: ExeIndex
`listing-00500000.txt` (`00501450` / `00500540` / `00502500`),
`listing-004c0000.txt` (`004FC8A0` / `004FC210` / `004FB150`),
`listing-00480000.txt` (`00487C20` / `00487F10` / `004A2B05`),
`listing-00440000.txt` (`00449E60` / `00449F90`),
`listing-006c0000.txt` (`006C2671`),
`listing-00d80000.txt` (`00DBDE40`),
`listing-00880000.txt` (`00892D80`),
`00-index/strings.tsv` / `xrefs-by-string.tsv` (`StartOakVale`
`0x012D9D1C`, `PlayerRegionName` `0x01231C98`);
`EngineLifecycle.LoadGlobalThingsFile` /
`CurrentRegionIndex` / `LoadFromFirstRealRegion` /
`EnqueueAfterDummy` / `LoadRegionByName` / `SetRegionAsLoaded`
(read only);
`EngineLifecycleTests.Second_pump_00501450_is_004FEEC0_then_00500540_1_0_0`,
`Persist_PlayerRegionName_is_00487C20_not_new_game`,
`No_save_does_not_activate_Q_NewOakValeIntro`;
`WorldSceneTests` `oak.Index == 4`;
siblings `proofs/wld-first-region`, `first-region-after-leave`,
`player-region-name-writer`, `00DBDE40-host-gap`,
`issue-4-verify`, `q-novi-activator-callers`.

---

## Verdict

**Nobody** on no-save first Present.

Childhood `00DBDE40` does **not** load index 4. It interns
`"StartOakVale"` and **waits** on script-context `vtbl+48`
until that name is already current. The only recovered
loader that **stays** on index 4 is persist `00487C20` after
a nonempty `PlayerRegionName` — continue, not New Game.

No-save first Present is LookoutPoint index **1** (leftover
**#4**). Leave that ledger open.

| Claim | Class |
|---|---|
| WLD `NewRegion 4` `RegionName "StartOakVale"` | **PROVEN** file bytes; `oak.Index == 4` |
| No-save first Present is LookoutPoint index 1 | **PROVEN** leftover **#4** — do not collapse |
| `PlayerRegionName` written on New Game | **DISPROVEN** (`PlayerRegionNameWrittenOnNewGame = false`) |
| Save writer of that key | **PROVEN** `00449F90` from `0049FB5C` inside `0049F4C0` PLAYER |
| Persist load of named region (e.g. index 4) | **PROVEN** `00449E60` → `00487C20` → `00500540(index,0,1)` |
| That persist load runs on empty no-save | **DISPROVEN** |
| `00501450` first `00500540(i,0,0)` is `i=1` Lookout | **PROVEN** body |
| `00501450` `i=4` would apply StartOakVale ContainsMap | **PROVEN** numeric; **not** stay-current; **not** first Present |
| After a full `00501450` loop `WorldMap+156` | **PROVEN** last authored `141` `Filler_NorthernWastes_02`; restore `(saved=0,0,1)` no pump |
| `00DBDE40` loads index 4 / `E8` `00500540` | **DISPROVEN** — wait, not load. Sole `E8` of `00DBDE40` is `00DAC295` |
| `.text` immediate `"StartOakVale"` | **PROVEN** two sites, both inside `00DBDE40` (`00DBDE4A` / `00DBDE9B`) |
| `LoadGlobalThingsFile` first proximity TNG is StartOakVale | **DISPROVEN** — first prox is LookoutPoint; leftover **#50**, do not fold into #4 |
| `00501450` live `E8` / `E9` / imm / vtbl | **UNREAD** (0 hits) |
| Childhood activator `00CB5AD0("Q_NewOakValeIntro")` | **UNREAD** (`blocked-on-activator`) |
| Who leaves `+156=4` for the `00DBDE40` wait without persist | **UNREAD** |

**Answer:** nobody on no-save first Present. First later
*numeric open* of index 4 without persist is `00501450`
`00500540(4,0,0)` **if** that unread-caller body runs — it
does not stay, and it is not leftover #4 Present. First later
*named childhood site* is `00DBDE40` after a proven Oakvale
activate — still a wait, not a loader.

---

## Evidence → Original → Host → Gap

### 1. WLD index 4 is `StartOakVale` (PROVEN)

**Evidence.** TLC `FinalAlbion.wld` / `00507C30`:

```
NewRegion 1;  RegionName "LookoutPoint";     // native index 1
NewRegion 2;  RegionName "PicnicArea";
NewRegion 3;  RegionName "BowerstoneSlums";
NewRegion 4;  RegionName "StartOakVale";
  ContainsMap StartOakValeWest / MemorialGarden / StartOakValeEast
```

Dummy slot 0 is `005066E0` (empty 88-byte record). Authored
row 4 is `World.Regions[3]`. `WorldSceneTests`:
`oak.Index == 4`, `oak.RegionName == "StartOakVale"`.

**Original.** Table index is the `NewRegion N` token. Load
jobs pass that integer into `00500540` / `004FC8A0`.

**Host.** `RegionAtNativeIndex(4)` walks `World.Regions` for
`Index == 4`. `LoadWorld` parses the file; it does **not**
call `00500540`. `CurrentRegionIndex` stays 0 (dummy) until
a later `SetRegionAsLoaded`.

**Gap.** None on the name/index pair. Wiki “New Game is
Oakvale” is this row, not first Present. Leftover **#4**.

---

### 2. `WorldMap+156` / `004FB150` / `004FC8A0` (PROVEN)

**Evidence.** `listing-004c0000.txt`:

```
004FB150  mov eax, [ecx+156]
          ret
```

`004FC8A0` (`SetRegionAsLoaded: Initialise MiniMap`):

```
004FC8A0  mov eax, [esp+4]
          mov esi, ecx
          …
          mov [esi+156], eax          ; write current index
          …
          imul ecx, ecx, 88
          add ecx, [esi+44]           ; record*
          push ecx
          mov ecx, [0x13B8790]
          call 00437CE0
          call 0082BA00               ; MiniMap only
          ret 8
```

Sole listing `call 004FC8A0`: `006C2671` inside apply
`006C2170` after vtbl+88 `005064C0`. Job `+28` is the
native index (`006C27A0`).

Ctor-zeroed `+156=0` is the dummy. Not a host Oakvale seed.

**Original.** Current region **is** `WorldMap+156`. Writers
are apply jobs, not the childhood script.

**Host.** `CurrentRegionIndex` / `GetCurrentRegionIndexFn =
0x004FB150` / `WorldMapCurrentRegionIndexOffset = 156`.
`SetRegionAsLoaded(index)` Notes MiniMap then
`ActivateCurrentRegion()`. Comment: not `005064C0`;
`00B428E0` is `004A1840` vtbl+208.

**Gap.** MiniMap / villages bodies UNREAD. Not this question.

---

### 3. `00500540` is the loader; callers (PROVEN set)

**Evidence.** `listing-00500000.txt` `00500540`: `ecx =
CWorldMap`, arg0 = native index, `imul eax, eax, 88`,
`[table+index*88+36]`. Null `+36` still continues
(`je 005009BE` then still `006C27A0`). Third arg 0 pumps
sync; 1 is async.

Listing `call 00500540` — **six** sites, no others:

| Site | Parent | No-save first Present |
|---|---|---|
| `005014EC` / `00501935` | `00501450` | body recovered; parent `E8` **UNREAD** |
| `00487C55` | `00487C20` persist name | empty no-save — **skip** |
| `0050255D` / `005025F8` | `00502500` | `004A3740` first-seen `[world+260]=0` **skip** |
| `00506455` | `005064C0` Post Region Load Villages | after a job already applied |

Listing `call 00501450`: **0**. Imm / vtbl of `0x00501450`:
**0**. Not `00DBDE40`.

**Original.** Only these parents can open index 4.

**Host.** `RequestLoadRegion(index, sync)` Notes `00500540`
then `006C27A0` / `006C2120`. `EnqueueAfterDummy` is leftover
glue (not a second-`Pump` callee). Tests call
`LoadFromFirstRealRegion()` explicitly.

**Gap.** Live `00501450` jump UNREAD. Do not re-hook it onto
`Pump`.

---

### 4. `00501450` loop — `i=4` is transient, not first Present (PROVEN body)

**Evidence.** `listing-00500000.txt` `00501450`…`00501985`:

```
0050146B  call 00449970
00501472  call 00487DC0              ; player thing (may miss)
          je  00501495               ; miss: no 00449D90
00501495  mov esi, [edi+156]
005014A3  call 004FEEC0              ; 004FEEC0(current, 0) → +156=0
          count = (+48−+44)/88       ; imul 0x2E8BA2E9
          cmp ecx, 1
          jbe 005018F8
          mov [esp+24], 1            ; i = 1
005014E3: push 0; push 0; push i
005014EC  call 00500540              ; 00500540(i, 0, 0)
          0048D400 bit 0x64
          005198B0 CTCActionUseScriptedHook
          inc i                      ; 005018D8
          jb  005014E3               ; i = 1 .. count-1
005018F8  RegionGraph.txt  0x124467C
00501935  call 00500540              ; 00500540(saved, 0, 1) no pump
```

Count is 142 (dummy + 141 authored). `i=1` LookoutPoint.
`i=4` **is** StartOakVale ContainsMap. Then `i=5`…`i=141`.
Last `004FC8A0` leaves `+156=141` `Filler_NorthernWastes_02`.
First-seen saved is 0, so restore is `(0,0,1)` with no sync
pump — current stays 141, not 4.

**Original.** Sweep every real region. Not “go to Oakvale.”
Not leftover #4 Present.

**Host.** `LoadFromFirstRealRegion` Notes
`00500540(1,0,0) first +36 null continues` then
`00500540(i,0,0)` through count-1, then
`00500540(saved,0,1) restore no-pump`. Test
`Second_pump_00501450_*`:

- first action `(1,0,0)` Lookout
- last `(141,0,0)`
- after the API: `CurrentRegionIndex == 141`,
  `CurrentRegion == Filler_NorthernWastes_02`
- **no** `00DBDE40`
- Present / `SubmittedWorld.Region` on the Lookout walk is
  `"LookoutPoint"` (leftover **#4**)

**Gap.** Pairing the body to a live `E8` UNREAD. Host
`EnqueueAfterDummy` on second `Pump` is **DISPROVEN**. Do
not treat `i=4` as first Present or as childhood current.

---

### 5. Persist `00487C20` / `00449E60` — named load, empty on New Game (PROVEN)

**Evidence.** `listing-00480000.txt` `00487C20`:

```
00487C20  lea eax, [edi+8]           ; name CString on the PLAYER blob
          call [world.vtbl+48]
          call 004FC210              ; FindRegionByName from index 1
          je  00487CD7               ; empty / miss → al=0
00487C55  call 00500540              ; 00500540(index, 0, 1) async
          …
          mov al, 1
          ret 4
```

Sole `call 00487C20`: `00487F10` inside `00487EF0`.
Parent of that: `00449F25` inside `00449E60`.

`00449E60` pushes `"PlayerRegionName"` (`00449EDB`) into
`004109A0` (load). Only `call 00449E60`: `004A2B05` after
`push "PLAYER"` in FableSav apply. No-save `[game+90588]`
empty skips `004A3200` — this walk does not run.

Writer is **`00449F90`** (`0044A04B` push the same key),
sole `E8` `0049FB5C` inside `0049F4C0` PLAYER **save**.
`EngineLifecycle.PlayerRegionNameWrittenOnNewGame = false`.

`.text` xrefs of `"PlayerRegionName"` `0x01231C98`: **two**
(`00449EDC` load, `0044A04B` save). No New Game writer.

**Original.** Continue-from-save: blob name → index →
`00500540(index,0,1)` and **stay**. If the saved string is
`StartOakVale`, index is 4. Empty name `je`s.

**Host.** `EnqueueAfterDummy`:

```
if (!string.IsNullOrEmpty(PlayerRegionName))
    LoadRegionByName(PlayerRegionName);   // 00449E60 / 00487C20
else
    LoadFromFirstRealRegion();            // 00501450
```

`Persist_PlayerRegionName_is_00487C20_not_new_game` **sets**
`PlayerRegionName = "StartOakVale"` as a continue stand-in:
`CurrentRegionIndex == 4`, no `00501450`, no `00DBDE40`.
That assignment is **not** a recovered New Game write.

**Gap.** Who writes the persist key on continue: save path
PROVEN; New Game writer **UNREAD** because it does not run.
Do **not** invent `PlayerRegionName=StartOakVale` to reach
index 4 on no-save.

---

### 6. Childhood `00DBDE40` waits; it does not load (PROVEN)

**Evidence.** `xrefs-by-string.tsv` `"StartOakVale"`
`0x012D9D1C`: **only** `00DBDE4A` and `00DBDE9B`, both
`fn=0x00DBDE40`. No persist immediate. No `00501450`
immediate.

`listing-00d80000.txt` `00DBDE40`:

```
00DBDE49  push "StartOakVale"
          call 0099EBF0              ; intern
          mov ecx, [esi+64]          ; quest +64 script context
          call [eax+48]              ; bool: name current / ready?
          ; neg/sbb/inc → wait while false
00DBDE7F  je  00DBDECA               ; already true → skip wait
00DBDE81  call [eax+28]              ; yield (ScriptYieldVtbl 28)
          call 00CB7940              ; keep / hero-exists gate
          jne 00DBE2CE               ; abort
          intern "StartOakVale" again
          call [edx+48]
          jne 00DBDE81               ; still false → yield
00DBDECA  call 00CB7940
          …
          push "CREATURE_HERO_CHILD" ; after map-ready
```

Listing `call 00500540` / `call 00487C20` / `call 004FC8A0`
inside `listing-00d80000.txt`: **0**. Sole `E8` of
`00DBDE40`: `00DAC295` in `00DABAC0` (`S_QNOVI` slot 2).

`00DBDE40` is **not** reached on no-save: Gameflow
`00CE7670` **waits** on inactive `Q_NewOakValeIntro`
(`00893610` miss). Activator `00CB5AD0("Q_NewOakValeIntro")`
is **UNREAD**. Bind `00CD6E27` is not construct.

**Original.** Childhood fiber **requires** index 4 already
current, then spawns the kid. It is not a region loader.

**Host.** `RegionTravel.StartOakValeSetup = 0x00DBDE40` as a
constant. `Pump` / `LoadFromFirstRealRegion` /
`LoadGlobalThingsFile` never Note or call it. Tests
`DoesNotContain(… StartOakValeSetup)` on no-save.
`QuestFactoryTable` has no Oakvale row — even a proven
activate would miss the fiber (`00DBDE40-host-gap`
**DIVERGE**). Do not grow `Pump` to close that.

**Gap.** Activator UNREAD. Map-ready `+156=4` without persist
UNREAD. Do not satisfy either by writing `PlayerRegionName`
or by collapsing leftover #4.

---

### 7. `LoadGlobalThingsFile` is not the Oakvale loader (PROVEN skip; leftover #50)

**Evidence.** `00509859`: `[0x13B8609]==0` → `004FDBC0`
per-map `.tng` (`004FBF60`). Native `ebx=1` skips dummy;
first filled `LoadedOnPlayerProximity` is LookoutPoint
(`NewMap 1`). StartOakVale maps are later in the same walk.

**Original.** Global TNG parse is not `00500540` and not
`WorldMap+156`.

**Host.** `LoadGlobalThingsFile` Notes `004FDBC0` then
**breaks** after the first proximity map
(`004FBF60 LookoutPoint.tng`, `GlobalThingMapsLoaded = 1`).
Comment: leftover **#50** (ThingFile.Parse OOM), not a
recovered NewMap-1 lock and **not** `00501450`. Do not fold
#50 into leftover #4.

**Gap.** Native then continues `ebx` through every prox
slot; host does not. Unrelated to who sets index 4.

---

### 8. Later travel `00502500` — first-seen skip (PROVEN skip; UNREAD as childhood)

**Evidence.** `00502500` from `004A4CB9` (`004A3740`) and
`0089B99E` (`00892D80`). First-seen type-1
`[world+248]=0` / `[world+260]=0` skips `004A3740`.
`00892D80` empty-intern arm is `vtbl+52`/`+48` →
`004FC210` / `004FB490` **index query**, not a load.

If a later dest map’s `004FC190` region is 4,
`0050255D` would `004FEEC0` then `00500540(dest,1)` and
**stay**. That is overworld travel, not no-save Present,
not `00DBDE40`.

**Original.** Possible later opener of index 4 without
persist. Not recovered as the childhood map-ready write.

**Host.** No `00502500` on no-save Pump. Not wired.

**Gap.** First dest that is StartOakVale **UNREAD**. Do not
invent it as New Game.

---

## Childhood path without `PlayerRegionName` (recovered order)

```
no-save Leave / Init Game
  00507C30  parse FinalAlbion.wld
            NewRegion 4 StartOakVale            // table only
  004FDBC0  first prox TNG LookoutPoint         // leftover #50
  PlayerRegionName empty                        // no 00449E60
  00CD6E27  bind Q_NewOakValeIntro / S_QNOVI    // not activate
  00CE7670  wait 00893610 miss                  // not 00DBDE40
  004189C2  dummy +156=0
later (E8 UNREAD; host explicit API)
  00501450  00500540(1,0,0) Lookout             // leftover #4 Present
            00500540(4,0,0) StartOakVale        // transient open
            00500540(141,0,0) Filler_…_02
            +156=141; restore (0,0,1) no pump
00DBDE40  not on this list                      // wait, not load
00487C20  not on this list                      // persist empty
```

No-save first **Present** stays LookoutPoint index **1**.
Leftover **#4**.

---

## First later site that would open Oakvale

Ordered. Stop at the first item that actually **loads**
index 4 without persist.

1. **`00501450` `i=4`** — first recovered **numeric open**
   of StartOakVale ContainsMap without a name. **PROVEN**
   as a loop step; **DISPROVEN** as stay-current and as
   first Present. Parent jump **UNREAD**.
2. **`00DBDE40`** — first recovered **named childhood**
   site. **DISPROVEN** as a loader. Needs a prior `+156=4`
   write. Activator **UNREAD**.
3. **`00502500` dest → region 4** — first recovered
   **stay-current** loader that is not persist. First-seen
   skip **PROVEN**. Childhood use **UNREAD**.
4. **`00487C20`** — stay-current named load. Requires
   nonempty `PlayerRegionName`. **DISPROVEN** on no-save.

There is **no** recovered no-save first-Present loader of
index 4. Do not invent one.

---

## Do not

- Write `PlayerRegionName = "StartOakVale"` on New Game.
- Collapse leftover **#4** (Lookout Present vs Oakvale
  intro *view* `StartOakValeWest` / `HerosOldHouse` /
  `CAM_OVIF_SHOT2`).
- Fold leftover **#50** (first-proximity TNG OOM break)
  into #4.
- Call `00DBDE40` from `Pump` / `LoadFromFirstRealRegion`.
- Treat `00501450 i=4` as the childhood current region.
- Treat `RegionTravel.NewGameRegion` / `StartingRegion`
  (`StartOakValeWest`) as Leave / first Present.

---

## Locking tests (not edited)

- `WorldSceneTests` — `FindRegionContaining("StartOakValeWest").Index == 4`
- `Second_pump_00501450_is_004FEEC0_then_00500540_1_0_0` — first `(1,0,0)`, last 141, no `00DBDE40`
- `Persist_PlayerRegionName_is_00487C20_not_new_game` — continue stand-in index 4; `PlayerRegionNameWrittenOnNewGame == false`
- `No_save_does_not_activate_Q_NewOakValeIntro`
- `Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint` — Present Lookout, adult 4299, no kid / no `00DBDE40`
