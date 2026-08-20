# Who sets current after dummy `WorldMap+156=0` on no-save first Present

Investigation only. No production `src/` or `tests/` edits.

Do **not** invent persist `PlayerRegionName` on New Game
(`PlayerRegionNameWrittenOnNewGame` is already `false`).
Do **not** invent a host `Pump` site. Dummy `Pump` /
`PumpGame` must **not** call `LoadFromFirstRealRegion`.
Do **not** collapse leftover **#4**.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: who sets the current region after dummy
`WorldMap+156=0` on no-save first Present, without
inventing `PlayerRegionName`? Computed inbound of
`00501450` (`ff.tsv` / vtbl / jmp). If still 0, how
does Lookout become current **without** this fn?

Authority: TLC dump
`assembly/exe/01-sections/text-map/`
(`listing-00500000.txt` `00501450` / `00500540` /
`005066E0`, `listing-004c0000.txt` `004FB150` /
`004FC180` / `004FC8A0` / `004FEEC0`,
`listing-006c0000.txt` `006C2170` / `006C2671` /
`006C27A0`, `listing-00400000.txt` dummy `004189C2`,
`listing-00480000.txt` `00487C20` / `004A5DF3`,
`e8.tsv`, `ff.tsv`, `calls-by-dest.tsv`, `abs.tsv`,
`branches.tsv`, `switch*.tsv`, `functions.tsv`);
`assembly/exe/00-index/vtbl.tsv` / `xrefs.tsv`;
siblings `proofs/00501450-e8-callers`,
`proofs/00501450-inbound-computed`,
`proofs/00501450-inbound-ff`,
`proofs/00501450-rdata-dwords`,
`proofs/first-region-after-leave`,
`proofs/wld-first-region`,
`proofs/startoakvale-index4-loader`,
`proofs/dummy-pumps-before-region`,
`proofs/startoakvale-current-writer`,
`proofs/worldmap-plus156-index4`,
`proofs/leftover-4-collapse-audit`,
`proofs/host-00501450-timing`;
`EngineLifecycle.Pump` / `PumpGame` /
`LoadFromFirstRealRegion` / `EnqueueAfterDummy` /
`SetRegionAsLoaded` / `CurrentRegionIndex`
(read only);
`EngineLifecycleTests`
`First_pump_004189C2_*`,
`Second_pump_004189C2_loops_inner_not_00501450`,
`Second_pump_00501450_is_004FEEC0_then_00500540_1_0_0`,
`Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`,
`Persist_PlayerRegionName_is_00487C20_not_new_game`.

---

## Verdict

**Nobody** on recovered no-save first-seen Present.

Dummy `004FC180` index **0** leaves `WorldMap+156=0`.
Type-1 and first `00435F70` Present do not write the
slot. The only recovered **nonzero** writer is
`004FC8A0` at sole `E8` `006C2671` (apply
`006C2170`, `job+28>0`). Nonzero `job+28` comes from
`00500540(index,…)`. First no-save open of index **1**
(LookoutPoint) is `00501450` `00500540(1,0,0)` **if
that body runs**.

Computed inbound of `00501450` is still **0**
(`E8` / `E9` / listing `call` / `jmp` / `ff.tsv`
abs / vtbl dest / `jmp r32` / `call r32` / PE dword).
Dummy `Pump` never calls `LoadFromFirstRealRegion`.

**Lookout does not become current without this fn**
on the recovered tree. Other `00500540` parents
skip on no-save, or they need a nonempty persist
name — do **not** invent `PlayerRegionName`. Host
tests that Present Lookout call
`LoadFromFirstRealRegion()` **explicitly** after
dummy. That API **is** `00501450`. Leftover **when**,
not a recovered Pump site.

Leave leftover **#4** open as dual ledgers. Do not
fold dummy-empty Present into Oakvale intro, and do
not replace Lookout first *rendered* scene with
`StartOakVale`.

| Claim | Class |
|---|---|
| Dummy first `004189C2` `+156` | **PROVEN** `0` (`004FB150` / `004FC180` index 0) |
| First-seen Present writes current? | **No.** Slot stays ctor 0. Empty `009DA9F0` skip | **PROVEN** skip |
| `WorldMap+156` writers | **PROVEN** three: ctor `0050682F` `0`; unload `004FF03F` `0`; apply `004FC8B2` job index |
| Every `E8` of `004FC8A0` | **PROVEN** one: `006C2671` |
| `job+28<=0` skips `004FC8A0` | **PROVEN** `006C25E8` `jle 006C267F` |
| Nonzero `job+28` | **PROVEN** from `00500540` (`00500D64` push index). UpdateNavMaps `00501C07 push ebx` is **0** |
| First `00500540(1,0,0)` if `00501450` ran | **PROVEN** body `005014EC`; WLD index **1** = LookoutPoint |
| `00501450` inbound `E8` / `ff` / vtbl / jmp | **PROVEN** absence **0** (restated; ALU hole **UNREAD**) |
| `StartOakVale` index 4 as stay-current / first Present | **DISPROVEN** (loop `i=4` then `004FEEC0`; not first) |
| Invent `PlayerRegionName` / New Game write | **DISPROVEN** (`PlayerRegionNameWrittenOnNewGame=false`) |
| Persist `00487C20` on empty no-save | **DISPROVEN** skip |
| Dummy `Pump` → `LoadFromFirstRealRegion` | **DISPROVEN**; host **MATCH** skip |
| Lookout current **without** `00501450` | **DISPROVEN** as a recovered first-seen path |
| Leftover **#4** dual (Lookout rendered vs Oakvale intro view) | **LEFTOVER** — leave open; **CLEAN** vs collapse |

**Answer:** nobody first-seen. Lookout current is
`004FC8A0(1)` after `00500540(1,0,0)`, and that
loader’s only recovered no-save parent is
`00501450`, which has **0** named inbound. Without
this fn, current stays dummy **0**. Host leftover
glue **is** this fn (tests after dummy). Do not
wire it onto `Pump`. Do not invent
`PlayerRegionName`.

---

## Leftover #4 (do not collapse)

| Ledger | Native pairing | No-save first *rendered* scene? |
|---|---|---|
| LookoutPoint WLD index **1** | first `00500540(1,0,0)` ContainsMap; `GuildArrivalHSP` → `006AC910` `CREATURE_HERO` mesh **4299**; `WorldCamera.SeedHero` `006B3FF0` FOV **70** | **yes** (this leftover’s Present) |
| Childhood intro *view* | `StartOakValeWest` / `HerosOldHouse` / `CAM_OVIF_SHOT2` / kid **4300** / `Q_NewOakValeIntro` / `CS_OAKVALE_INTRO_FATHER` FOV **72** | **no** |

Dummy `00435F70` while `+156=0` is an **empty**
Present (`SubmitCurrentWorld` needs
`HeroSpawned`; native `009DA9F0` skip). That
empty frame is **not** Oakvale and is **not** a
reason to call `00501450` from `Pump`. It is also
**not** leftover #4 collapse: #4 is Lookout
geometry vs intro *view*, not dummy vs Lookout.

WLD: `World.Regions[0].Index == 1` `LookoutPoint`;
`World.Regions[3].Index == 4` `StartOakVale`.
Host native table index 0 is dummy `005066E0`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Computed inbound of `00501450` (`ff.tsv` / vtbl / jmp)? | **0.** Same as `E8` / imm / rdata dword | **PROVEN** absence |
| Who writes current after dummy on first-seen Present? | **Nobody.** Slot stays `0` | **PROVEN** |
| Who *can* write Lookout (`+156=1`)? | `004FC8A0(1)` via `006C2671` after `00500540(1,…)` | **PROVEN** body |
| How does Lookout become current **without** `00501450`? | **It does not** on recovered no-save first-seen. Other `00500540` parents skip or need persist | **DISPROVEN** as a path |
| Host Lookout Present? | Tests call `LoadFromFirstRealRegion()` after dummy. `Pump` does not | **DIVERGE** site; **MATCH** skip; body **MATCH** first open |
| Invent `PlayerRegionName` to pick Lookout or Oakvale? | **No** | **DISPROVEN** |

---

## 1. Computed inbound of `00501450` — still 0

Siblings already emptied `E8` / `E9` / `.text` imm /
rdata VA+RVA dword / `call r32` / type-table
`call edx`. This note **re-greps** `ff.tsv`,
`vtbl.tsv`, listing `jmp` / `call`,
`calls-by-dest`. Still **0** dests.

| Hunt | Hits | Class |
|---|---|---|
| `e8.tsv` dest `0x00501450` | **0** | **PROVEN** |
| listing `call 00501450` / `jmp 00501450` | **0** | **PROVEN** |
| `calls-by-dest.tsv` dest `0x00501450` | **0** rows | **PROVEN** |
| `ff.tsv` mem `[0x00501450]` | **0** | **PROVEN** |
| `ff.tsv` last col `0x00501450` | 21 **outbound** sites `005016B9`…`00502E6D` (containing-fn swallow) | **DISPROVEN** as inbound |
| `vtbl.tsv` dest `0x00501450` | **0** | **PROVEN** |
| CWorld `0x01244AEC` slots 0–30 | `0051D1E0` `005022F0` `00507610` `00507C30` … `00502460`. **No** `00501450` | **PROVEN** |
| `abs.tsv` operand `0x00501450` | **0** dest (rows tagged last-col are **inside** the blob) | **PROVEN** |
| `branches.tsv` dest `0x00501450` | **0** inbound (internal `je`/`jmp` only) | **PROVEN** |
| `switch*.tsv` dest `0x00501450` | **0** | **PROVEN** |
| `xrefs.tsv` `00501450` | **0** | **PROVEN** empty as string xref |

`ff.tsv` header `site kind mem disp fn`. Last
column is the **containing function**, not the
callee. `functions.tsv` start `0x00501450` size
**2248** swallows `00501990` UpdateNavMaps (real
`ret` is `00501985`). FF `[ebx+64]` at
`005016B9` is outbound **from** the blob.

`LoadFromFirstRealRegionNamedInbound = 0`.
`PumpCallsLoadFromFirstRealRegion = false`.

Still **UNREAD** as a live native site: two-immediate
ALU / get-PC add that yields `00501450` from values
other than VA / RVA / `0x00500000+0x1450`
(`proofs/00501450-inbound-computed`). Do **not**
treat 0 named encodings as a recovered “never”
without that hole. Do **not** invent `Pump`.

---

## 2. Dummy `+156=0` is still current at first-seen Present

```
005066E0  World Map ctor
  0050682F  mov [esi+156], ebx          ; ebx=0 dummy

004189C2  first Game pump
  00418A48  call 004FB150               ; eax=[ecx+156] = 0
  00418A57  call 004FC180               ; [map+44]+0*88
  00418A5C  mov ecx, [eax+36]
  00418A61  je  00418A70                ; dummy +36 null
  00418A70  fade 00B239A0 once
  loop 00418AB1
    type-1 004A5A40  004A5DF3 006B3FF0  ; seed; still dummy
    00435F70 Present                    ; still +156=0
```

`004FB150` / `004FC180` **read**. They do not
`E8` `00501450` / `00500540` / `004FC8A0`.

Host `PumpGame` dummy arm Notes those getters,
`ActivateCurrentRegion` (`index==0` return),
fade, one inner. **No** `EnqueueAfterDummy`.
**No** `LoadFromFirstRealRegion`. Tests:

- first dummy `Pump`: `CurrentRegionIndex==0`,
  `CurrentRegion==null`,
  `FirstRealRegionLoadDone==false`
- next dummy `Pump`: still those; no Note of
  `LoadFromFirstRealRegionFn`

**MATCH** skip vs dummy. **PROVEN.**

---

## 3. Who writes `WorldMap+156`

WorldMap object (getter `004FB150`):

| Site | Parent | Value |
|---|---|---|
| `0050682F` | ctor `005066E0` | `ebx=0` dummy |
| `004FF03F` | unload `004FEEC0` | `0` |
| `004FC8B2` | `004FC8A0` after apply | `eax` = job native index |

```
004FC8A0  mov eax, [esp+4]
          mov esi, ecx
          …
004FC8B2  mov [esi+156], eax            ; current INDEX
          imul ecx, ecx, 88
          add ecx, [esi+44]
          call 00437CE0
          call 0082BA00                 ; MiniMap only
```

`e8.tsv` dest `0x004FC8A0`: **one** site,
`006C2671` inside `006C2170`:

```
006C25E8  mov eax, [esi+28]             ; job+28
          test eax, eax
          jle 006C267F                  ; skip 004FC8A0
          …
          call [edx+88]                 ; 005064C0 villages
006C266D  push ecx                      ; job+28
          mov ecx, [edx+28]             ; CWorldMap
006C2671  call 004FC8A0
```

`006C27A0` stores the index at `+28`:

```
006C27BC  mov edx, [esp+16]
006C27C0  mov [esi+28], edx             ; job+28
```

`e8.tsv` dest `0x006C27A0`:

| Site | Parent | `job+28` |
|---|---|---|
| `00500D7A` / `005010AE` / `00501319` | `00500540` | native index (`00500D64 push edx` from arg0) |
| `00501C0E` | `00501990` UpdateNavMaps | `00501C07 push ebx` = **0** → skip `004FC8A0` |

No other `.text` `E8` of `004FC8A0`. Other
`[reg+156]` stores are widgets / fade / camera,
not this WorldMap (`proofs/startoakvale-current-writer`).

First-seen loader queue is empty
(`004A1AA3` `006C20A0` → skip vtbl+4).
`006C2671` is **not** first-seen. **PROVEN.**

---

## 4. `00500540` callers — only six; first Lookout is `00501450`

`e8.tsv` dest `0x00500540`:

| Site | Parent | No-save first Present |
|---|---|---|
| `005014EC` / `00501935` | `00501450` | body recovered; parent inbound **0** |
| `00487C55` | `00487C20` persist `PlayerRegionName` | empty no-save **skip**. Do **not** invent the key |
| `0050255D` | `00502500` map switch | first-seen `[world+260]=0` **skip** |
| `005025F8` | `005025B0` reload current | only if already nonzero — not first |
| `00506455` | `005063E0` map→region | after a job already applied / persist-adjacent |

Body `00501450`…`00501985` if it ran:

```
00501495  mov esi, [edi+156]            ; saved (first-seen 0)
005014A3  call 004FEEC0(saved, 0)       ; +156=0
          count = (+48-+44)/88          ; 142
          [esp+24] = 1
005014EC  call 00500540(i, 0, 0)        ; first i=1 LookoutPoint
          0048D400 / 005198B0
00501839  call 004FEEC0(i, 0)           ; +156=0 again
          inc i; jb 005014E3            ; i=1..141
005018F8  RegionGraph.txt
00501935  call 00500540(saved, 0, 1)    ; no sync pump
00501985  ret
```

`i=1` LookoutPoint. `i=4` StartOakVale is a
**transient** open then `004FEEC0(4,0)`. Not
stay-current. Not leftover #4 Present.

After native `ret`: last `00501839` leaves
`+156=0`; restore `(0,0,1)` does not pump.
Current is dummy **0** again — **not** Lookout
stay, **not** 141 stay.

Host `LoadFromFirstRealRegion` Notes the first
`004FEEC0(saved,0)` and the restore, **omits**
in-loop `00501839`. Tests assert
`CurrentRegionIndex==141`
`Filler_NorthernWastes_02`. **DIVERGE** vs this
`E8`, not a recovered stay-current of index 1.

Leftover #4 Lookout *rendered* scene on that
host walk is `FirstSceneMapName` from
`GuildArrivalHSP` during the **first** apply
(`SpawnHeroFromPlayerStart` sets it on the map
that owns the holy site), plus adult **4299**
and `006B3FF0` FOV **70**. That pairing stays.
It is **not** `+156` staying 1 through Present,
and it is **not** Oakvale.

---

## 5. How Lookout becomes current **without** `00501450`

**It does not**, on recovered no-save first-seen.

Required chain:

```
00500540(1, 0, 0)
  006C27A0  job+28 = 1
  006C2120  enqueue
  sync 006C2710 → 006C2170
    ContainsMap BowerstoneBridge / LookoutPoint / GuildExterior
    GuildArrivalHSP → 006AC910 CREATURE_HERO 4299
    006C2671 004FC8A0(1)  →  +156=1
```

Every no-save parent of `00500540(1)` except
`00501450` `i=1`:

| Candidate | Why not |
|---|---|
| Persist `00487C20` | Needs nonempty `PlayerRegionName` → `004FC210` index. Empty `je`. Writer is save `00449F90`, not New Game. **Do not invent** `PlayerRegionName=LookoutPoint` (or StartOakVale) |
| `00502500` dest region 1 | First-seen `004A3740` skip. Later travel, not dummy Present |
| `005025B0` | Reloads `+156` already set. First-seen 0 inbound (`e8` dest **0**) |
| `005063E0` / `00506455` | After apply / persist-adjacent. Not first dummy Present |
| Dummy `004FC180` | Getter of slot 0. Not a loader |
| `00DBDE40` | Waits on `"StartOakVale"` already current. **0** `E8` of `00500540` / `004FC8A0`. Oakvale leftover, not Lookout |
| Host `Pump` | Never `LoadFromFirstRealRegion` / `EnqueueAfterDummy` |

Host leftover **is** this fn:

```
tests / _loadprobe:
  dummy Pump(s)                 // +156=0 MATCH
  life.LoadFromFirstRealRegion() // 00501450 body; DIVERGE site
```

`EnqueueAfterDummy`: nonempty persist name →
`LoadRegionByName`; else `LoadFromFirstRealRegion`.
Production `Pump` never calls it. Continue
stand-in tests that **set**
`PlayerRegionName="StartOakVale"` are **not** a
New Game write (`PlayerRegionNameWrittenOnNewGame=false`).

Do **not** close the inbound-0 hole by calling
`00501450` from dummy `Pump`. Do **not** close
leftover #4 by activating Oakvale.

---

## 6. Host vs dump after dummy

| Site | vs dump | Class |
|---|---|---|
| `PumpGame` dummy `004FB150` / `004FC180` / fade / one inner | **MATCH** |
| `Pump` / `PumpGame` never `LoadFromFirstRealRegion` | **MATCH** skip |
| `ActivateCurrentRegion` index 0 return | **MATCH** |
| `SubmitCurrentWorld` only if `HeroSpawned` | dummy empty; **MATCH** skip vs no apply |
| Type-1 `004A5DF3` `006B3FF0` on dummy current | **PROVEN** native; host seed-only-after-`00501450` is leftover **when** (not #4) |
| Explicit `LoadFromFirstRealRegion` after dummy | **DIVERGE** site; first open Lookout **MATCH** body |
| After that API `CurrentRegionIndex==141` | **DIVERGE** vs native `00501839` → `+156=0` |
| `FirstSceneMapName==LookoutPoint` / hero **4299** / `GuildArrivalHSP` / FOV **70** | leftover **#4** Present ledger; **MATCH** first apply, not stay-`+156=1` |
| `EnqueueAfterDummy` on second `Pump` | **DISPROVEN** |

---

## Not these

| Candidate | Class |
|---|---|
| Dummy `004189C2` / type-1 / first `00435F70` as the Lookout current write | **DISPROVEN** |
| `00501450` inbound via `ff` / vtbl / jmp | **PROVEN** absence |
| `PlayerRegionName` New Game write | **DISPROVEN** |
| `StartOakVale` / index 4 as first Present or stay-current | **DISPROVEN** |
| `00DBDE40` as a region loader | **DISPROVEN** |
| Host dummy `Pump` → `LoadFromFirstRealRegion` | **DISPROVEN** |
| Fold leftover **#50** (first-proximity TNG OOM) into #4 | **DISPROVEN** as work |
| Collapse leftover **#4** (Lookout Present vs Oakvale intro view) | **DISPROVEN** as work |
| Lookout current without `00501450` on recovered first-seen | **DISPROVEN** |

---

## Classifications (short)

1. **`00501450` computed inbound still 0. PROVEN absence**
   (`ff.tsv` abs 0, vtbl dest 0, listing `call`/`jmp` 0,
   `call r32`/`jmp r32` already empty). ALU hole **UNREAD**.
2. **Nobody writes current after dummy on no-save first-seen Present. PROVEN.**
   Slot stays ctor `0`. Dummy Pump **MATCH** skip.
3. **Lookout current without this fn: DISPROVEN** as a
   recovered path. Only `004FC8A0(1)` after
   `00500540(1,0,0)`; only recovered no-save parent is
   unread-inbound `00501450`. Do not invent
   `PlayerRegionName`.
4. **Host leftover is the when of this same fn**
   (explicit `LoadFromFirstRealRegion` after dummy).
   Do not wire dummy `Pump` to it.
5. **Leftover #4 stays open** as Lookout first *rendered*
   scene vs Oakvale intro *view*. Dummy empty Present is
   not Oakvale.

---

## Open

| Item | Class |
|---|---|
| Two-immediate ALU / get-PC add → `00501450` | **UNREAD** |
| Live native entry after dummy pumps | **UNREAD** |
| Drop unused `EnqueueAfterDummy` | leftover API |
| Host omit of in-loop `004FEEC0` (`CurrentRegionIndex==141`) | **DIVERGE**; not leftover #4 |

---

## Files read

- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00500000.txt` (`00501450`…`00501985`, `00500540` `00500D7A`, `00501C0E`, `0050682F`)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-004c0000.txt` (`004FB150`, `004FC180`, `004FC8A0` `004FC8B2`, `004FF03F`)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-006c0000.txt` (`006C25E8`…`006C2671`, `006C27A0` `+28`)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00400000.txt` (`00418A48` dummy)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\e8.tsv` (dest `00501450` **0**; dest `00500540` six; dest `004FC8A0` one; dest `006C27A0` four)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\ff.tsv` (mem `[0x00501450]` **0**; last-col outbound)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\calls-by-dest.tsv` (dest `0x00501450` **0**)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\abs.tsv` / `branches.tsv` / `switch*.tsv` / `functions.tsv`
- `C:\FableCSharp\assembly\exe\00-index\vtbl.tsv` (`0x01244AEC`; dest `00501450` **0**) / `xrefs.tsv`
- `C:\FableCSharp\proofs\00501450-e8-callers\README.md`
- `C:\FableCSharp\proofs\00501450-inbound-computed\README.md`
- `C:\FableCSharp\proofs\00501450-inbound-ff\README.md`
- `C:\FableCSharp\proofs\first-region-after-leave\README.md`
- `C:\FableCSharp\proofs\wld-first-region\README.md`
- `C:\FableCSharp\proofs\startoakvale-index4-loader\README.md`
- `C:\FableCSharp\proofs\dummy-pumps-before-region\README.md`
- `C:\FableCSharp\proofs\startoakvale-current-writer\README.md`
- `C:\FableCSharp\proofs\worldmap-plus156-index4\README.md`
- `C:\FableCSharp\proofs\leftover-4-collapse-audit\README.md`
- `C:\FableCSharp\proofs\host-00501450-timing\README.md`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs` (`PumpGame` / `LoadFromFirstRealRegion` / `SetRegionAsLoaded` / `SpawnHeroFromPlayerStart`; read only)
- `C:\FableCSharp\tests\Fable.Formats.Tests\EngineLifecycleTests.cs` (`Second_pump_*` / Lookout spawn; read only)
