# `00501450` body — every E8 / first-seen region current

Investigation only. No production `src/` or `tests/` edits.

Do **not** invent `PlayerRegionName` as a no-save current.
Do **not** start at Oakvale / `00DBDE40` / `CREATURE_HERO_CHILD`.
Do **not** treat `00501450` as Init Characters.
Do **not** invent a host `Pump` site. Dummy `Pump` /
`PumpGame` must **not** call `LoadFromFirstRealRegion`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: `00501450` body — every `E8` / first-seen region
current. Who calls `00501450` (`calls-by-dest.tsv`)? Does
it load `StartOakVale` as current on no-save? Host
seed-only-after-`00501450` leftover in `docs/PARITY.md`.

`docs/PARITY.md` (After `004A5E10` on type-1 tick):
`00501450` still 0 `E8`/`imm`; `00501935` is inside
`00501450`. Host seed-only-after-`00501450` is leftover.
Verify.

Authority: TLC listing
`assembly/exe/01-sections/text-map/listing-00500000.txt`
(`00501450`…`00501985` `ret`), `e8.tsv`,
`calls-by-dest.tsv`, `functions.tsv`, `abs.tsv`;
`listing-004c0000.txt` (`004FEEC0` / `004FC8A0` /
`004FB150`); `listing-00480000.txt` (`004A5DF3` /
`00487C20`); `listing-00680000.txt` (`006B3FF0` /
`006B42F0`); siblings
`proofs/00501450-inbound-computed`,
`proofs/00501450-inbound-ff`,
`proofs/00501450-host-leftover`,
`proofs/00501450-no-00449D90`,
`proofs/00501450-rdata-dwords`,
`proofs/first-region-after-leave`,
`proofs/wld-first-region`,
`proofs/startoakvale-index4-loader`;
`docs/PARITY.md` type-1 / no-save enqueue rows;
`docs/runtime/FORWARD_TREE.md` §§8–9;
`EngineLifecycle.LoadFromFirstRealRegion` /
`TickWorld` / `EnqueueAfterDummy` (read only).

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| Inbound `E8` / `imm` of `00501450`? | **0.** `calls-by-dest` dest **0** rows. Listing `call 00501450` **0**. `abs` operand **0** | **PROVEN** absence |
| `00501935` is inside `00501450`? | **Yes.** `E8` `00500540(saved,0,1)` at `00501935`, before `00501985 ret` | **PROVEN** |
| Body `E8` count `00501450`…`00501985`? | **47** sites, **29** unique dests. Plus one FF `/2` `005016B9` `call [ebx+64]` | **PROVEN** |
| Who calls `00501450`? | **Nobody indexed.** Dest row **0**. Containing-fn tags are outbound / swallow | **PROVEN** absence; live site **UNREAD** |
| First-seen region current (no-save, before this fn)? | Dummy `WorldMap+156=0`. Type-1 does not change it. This fn is **not** first-seen | **PROVEN** skip |
| First `00500540` if the body ran? | `005014EC` `00500540(1,0,0)` LookoutPoint (native index **1**) | **PROVEN** body |
| Load `StartOakVale` as **current** on no-save? | **No.** Index 4 is a loop `i`, not stay-current, not first. No name immediate. No `00487C20` | **DISPROVEN** |
| Invent `PlayerRegionName` here? | **No.** Persist is `00487C20` `00500540(index,0,1)` — continue, empty no-save | **DISPROVEN** |
| Host seed-only-after-`00501450`? | Native `006B3FF0` is `004A5DF3` on type-1 (other `E8` `006B4305` in `006B42F0`). Body has **0** `E8` of `006B3FF0` | **LEFTOVER** (tying seed to this fn); type-1 site **PROVEN** |
| PARITY “still 0 `E8`/`imm`”? | Inbound **yes**. Body is **not** 0 `E8` (47 outbound) | **PROVEN** inbound; do not misread as empty body |

**Hit count inbound: 0.**

Do **not** wire dummy `Pump` to this fn.
Do **not** treat `00501935` as a caller of `00501450`.

---

## PARITY claims (verify)

Type-1 row (`docs/PARITY.md` After `004A5E10`):

> `006B3FF0` only other `E8` is `006B42F0`. `00501450`
> still 0 `E8`/`imm`; `00501935` is inside `00501450`.
> `00487C20` is persist `PlayerRegionName` … Not region
> load. Host seed-only-after-`00501450` is leftover.

| Claim | Dump | Class |
|---|---|---|
| `00501450` 0 inbound `E8` | `e8.tsv` dest **0**. `calls-by-dest` dest **0**. Listing `call 00501450` **0** | **PROVEN** |
| `00501450` 0 inbound `imm` | `abs.tsv` operand `0x00501450` **0**. Whole-PE dword already **PROVEN** empty (`rdata-dwords`) | **PROVEN** |
| `00501935` inside `00501450` | `00501935 E8 00500540` then player `004C8CF0` then `00501985 ret` / `int3` / `00501990` | **PROVEN** |
| `006B3FF0` `E8`s | `004A5DF3` (type-1 `004A5A40`) and `006B4305` (`006B42F0` when `+68==0`). **Not** this body | **PROVEN** |
| Type-1 is not region load | No `E8` `00501450` / `00500540` / `00487C20` on that tick | **PROVEN** |
| Host seed-only-after-`00501450` | Native seed already ran at `004A5DF3` with dummy current. Body never `E8`s `006B3FF0` | **LEFTOVER** |

`00501935` is a **callee site** (`00500540` restore).
`functions.tsv` start `0x00501450` size **2248** swallows
`00501990` UpdateNavMaps. Real enqueue end is `00501985`.
Swallow is not a caller.

---

## Direct answers

| Hunt | Hits | Class |
|---|---|---|
| `calls-by-dest.tsv` dest `0x00501450` | **0** | **PROVEN** |
| `e8.tsv` dest `0x00501450` | **0** | **PROVEN** |
| listing `call 00501450` / `jmp 00501450` | **0** | **PROVEN** |
| `e8.tsv` sites in `00501450`…`00501985` | **47** | **PROVEN** outbound |
| Those dests include `006B3FF0` / `00487C20` / `00DBDE40` | **0** | **PROVEN** |
| `00501935` dest | `0x00500540` (parent tagged `0x00501450`) | **PROVEN** |
| `00500540` dest rows | `00487C55`, `005014EC`, `00501935`, `0050255D`, `005025F8`, `00506455` | **PROVEN** |
| Hardcoded index 4 / `"StartOakVale"` in this body | **0** | **PROVEN** |

---

## Body (listing `00501450`…`00501985`)

`ecx` = `CWorldMap`. Prologue `push ebp` / `and esp,-8` /
`sub esp,120`. Pad `int3` `00501444`…`0050144F`.

```
00501459  mov eax, [0x13B86A0]
00501464  mov ecx, [eax+28]
0050146B  call 00449970
00501472  call 00487DC0
          ebx = player; xor ebp, ebp
          je 00501495                    // miss: no 00449D90
          test [ebx+145], 1
          jne 00501495
00501490  call 004C8CF0(1)
00501495  mov esi, [edi+156]             // saved current
005014A3  call 004FEEC0(esi, 0)          // +156=0  (004FF03F)
          count = ([edi+48]-[edi+44])/88   // imul 0x2E8BA2E9
          [esp+24] = 1
          jbe 005018F8                   // count<=1
loop i = [esp+24] .. count-1
005014EC  call 00500540(i, 0, 0)         // first i=1
00501515  call 0048D400                  // bit 0x64
005015F0  call 005198B0                  // CTCActionUseScriptedHook
00501839  call 004FEEC0(i, 0)            // +156=0 again
          inc i; jb 005014E3
005018F8  push 0x124467C                 // RegionGraph.txt
00501901  call 0099B6B0
00501935  call 00500540(saved, 0, 1)     // no sync pump
0050194A  call 004C8CF0(0)               // if player live
00501985  ret
00501990  UpdateNavMaps                  // next fn; not this body
```

No-save first-seen `+156=0` (dummy). `saved=0`.
Count 142 → `i=1..141`. First `00500540` is **LookoutPoint**.
Last pumped apply is `i=141` `Filler_NorthernWastes_02`
(`004FC8A0` writes `+156=141`), then `00501839`
`004FEEC0(141,0)` writes `+156=0`. Restore
`00500540(0,0,1)` does **not** pump, so native current
after `ret` is dummy **0**.

Host `LoadFromFirstRealRegion` Notes the first
`004FEEC0(saved,0)` and the restore, **omits**
`00501839`. Tests assert `CurrentRegionIndex==141`.
That is **DIVERGE** vs this `E8`, not a recovered
stay-current.

---

## Every `E8` (`e8.tsv` `0050146B`…`00501979`)

Region / player first, then CRT / string / heap.

| Site | Dest | Role (this body) |
|---|---|---|
| `0050146B` | `00449970` | player getter `[game+28]+28` |
| `00501472` | `00487DC0` | creature slot `+44` |
| `00501490` | `004C8CF0` | live Thing (`push 1`); miss skips |
| `005014A3` | `004FEEC0` | unload saved current; `+156=0` |
| `005014AC` | `0099E4B0` | string ctor |
| `005014EC` | `00500540` | **`00500540(i,0,0)`** loop; first `i=1` |
| `00501515` | `0048D400` | collect bit `0x64` |
| `00501520` | `00BFEA0E` | heap |
| `0050156D` | `004FC190` | map → region |
| `00501597` | `004365B0` | helper |
| `005015AD` | `007792D0` | helper |
| `005015BB` | `0099E960` | string vs `0x122D70E` |
| `005015CE` | `004AC380` | list insert |
| `005015F0` | `005198B0` | `CTCActionUseScriptedHook` |
| `0050162D` | `004FC190` | map → region |
| `0050169B` | `00A01B50` | slot get |
| `005016B1` | `004C73D0` | thing id |
| `005016BF` | `004FC190` | map → region |
| `005016D8` | `004AC380` | list insert |
| `0050170C` | `0099F690` | string |
| `00501717` | `0099F600` | string |
| `00501721` | `0099F0A0` | string |
| `0050172A` | `0099EAE0` | string dtor |
| `00501733` | `0099EAE0` | string dtor |
| `005017A2` | `0099F690` | string |
| `005017AD` | `0099F600` | string |
| `005017B7` | `0099F0A0` | string |
| `005017C0` | `0099EAE0` | string dtor |
| `005017C9` | `0099EAE0` | string dtor |
| `005017DB` | `0099F100` | string |
| `0050182B` | `0099F100` | string |
| `00501839` | `004FEEC0` | unload `i`; `+156=0` |
| `00501859` | `004ABA70` | tree erase |
| `00501864` | `0099EAE0` | string dtor |
| `0050186A` | `00BFEA14` | heap free |
| `005018A0` | `00BFEA14` | heap free |
| `005018B1` | `00BFEA14` | heap free |
| `00501901` | `0099B6B0` | `RegionGraph.txt` `0x124467C` |
| `00501913` | `0099AD80` | file helper |
| `0050191C` | `0099B510` | string dtor |
| `0050192A` | `0099E5F0` | string |
| `00501935` | `00500540` | **`00500540(saved,0,1)`** restore, no pump |
| `0050194A` | `004C8CF0` | live Thing (`push 0`) |
| `0050195B` | `0099A920` | string |
| `00501967` | `0099B510` | string dtor |
| `00501970` | `0099A300` | string |
| `00501979` | `0099EAE0` | string dtor |

`005016B9` `FF 53 40` `call [ebx+64]` is **not** an `E8`.

`006C27A0` / `006C2120` / `006C20A0` / `004FC8A0` are
**not** `E8`s of this fn. They sit inside `00500540`
(sync when arg3=0) / apply. `functions.tsv` callee list
after `0099EAE0` (`00501979`) belongs to swallowed
`00501990`.

---

## First-seen region current (no-save)

Recovered tree never enters this body:

```
0042F2A2  Leave
004184BD  Init Game
  00416953  Loading world / 0049F180     // 0 E8 00501450
  user.ini Gameflow                      // 0
004189C2  dummy pumps  +156=0
  type-1 004A5A40
    004A5DF3  006B3FF0                   // seed; still dummy
    004A5E10  inc WorldFrame
    no 00501450 / 00500540 / 00487C20
00435F70  first Present                  // still dummy
```

| When | `+156` current | Class |
|---|---|---|
| Dummy / type-1 / first Present | **0** dummy | **PROVEN** |
| First-seen this fn | **not reached** | **PROVEN** skip; caller **UNREAD** |
| Body start `esi=[edi+156]` | saved **0** | **PROVEN** if it ran |
| First `005014EC` | `00500540(1,0,0)` LookoutPoint | **PROVEN** body |
| Loop `i=4` | `00500540(4,0,0)` then `004FEEC0(4,0)` | **PROVEN** transient; **not** stay-current |
| After `ret` native | `004FEEC0` last write `+156=0`; restore no pump | **PROVEN** body |
| Host after explicit API | asserts **141** | **DIVERGE** (skips `00501839`) |

WLD native index **1** = `LookoutPoint`. Native index **4**
= `StartOakVale` (`Regions[3]`). File bytes, not this
prologue.

---

## Who calls `00501450`

`calls-by-dest.tsv` header `dest site fn`. Dest
`0x00501450`: **0 rows**.

Rows with last-col `0x00501450` are **outbound** from the
swallowed blob (`0050146B`…`00502D*` including
`00502500`). Not inbound.

Siblings already emptied `E9` / vtbl-run / rdata dword /
`call r32`. Not re-proved here.

`00500540` dest **does** list `005014EC` / `00501935`
as sites **in** this fn. Contrast: if an `E8` of
`00501450` existed, dest would have a row (see
`00502500` ← `004A4CB9`).

---

## `StartOakVale` as current on no-save?

**No.**

| Candidate | Why not |
|---|---|
| Immediate `"StartOakVale"` in this body | **0** |
| Hardcoded `push 4` as region index | loop starts `[esp+24]=1`; no `4` |
| `005014EC` first `i` | **1** LookoutPoint |
| `i=4` in the `1..141` walk | opens then `004FEEC0(4,0)` zeros current. Not stay. Not first-seen |
| Persist `00487C20` | `E8` `00500540` at `00487C55`, **not** `00501450`. Needs nonempty `PlayerRegionName`. Empty no-save. **Do not invent that key here** |
| `00DBDE40` | wait on name already current; sole `E8` of it is `00DAC295`. Not this entry |

Do **not** collapse leftover **#4** (first Present is
Lookout, not Oakvale intro view).

---

## Host leftover (seed-only-after-`00501450`)

Native first `006B3FF0`:

```
004A5DF3  E8 F8 E1 20 00   call 006B3FF0    ; ecx=[world+24], arg [world+28]+8
006B4305  E8 E6 FC FF FF   call 006B3FF0    ; inside 006B42F0 when [this+68]==0
```

`00501450`…`00501985`: **0** of those.

Type-1 already seeds on dummy current. Waiting for
`LoadFromFirstRealRegion` to seed is leftover.

Host now:

| Site | vs dump | Class |
|---|---|---|
| `TickWorld` Notes `004A5DF3 006B3FF0` without `FirstRealRegionLoadDone` | **MATCH** site |
| `Pump` / `PumpGame` never call `LoadFromFirstRealRegion` | **MATCH** skip |
| `SpawnHeroFromPlayerStart` still Notes `006B3FF0 +68` after explicit `00501450` apply | **LEFTOVER** fold |
| `EnqueueAfterDummy` glue | leftover **when**, unused by live `Pump` |

Do **not** re-hook seed to `00501450`.

---

## Not these

| Candidate | Class |
|---|---|
| Body has 0 `E8` (misread of PARITY inbound) | **DISPROVEN** (47 outbound) |
| `00501935` is an inbound / a separate fn | **DISPROVEN** |
| `calls-by-dest` last-col `00501450` is a caller | **DISPROVEN** (containing-fn / swallow) |
| No-save current is `StartOakVale` / index 4 | **DISPROVEN** |
| `PlayerRegionName` written or read here | **DISPROVEN** |
| Type-1 `004A5DF3` is this fn | **DISPROVEN** |
| `006B3FF0` only after region enqueue | **DISPROVEN** (type-1 first) |
| Dummy second `Pump` → this fn | **DISPROVEN** (siblings) |
| Oakvale / `00DBDE40` as this entry | **DISPROVEN** |

---

## Classifications (short)

1. **Inbound to `00501450`: PROVEN absence** (`calls-by-dest` dest 0, `e8` dest 0, listing `call` 0, `imm` 0). PARITY “still 0 `E8`/`imm`” is that inbound.
2. **`00501935` is inside `00501450`: PROVEN** (`00500540(saved,0,1)` before `00501985 ret`).
3. **Body `E8`s: PROVEN 47.** Region opens are `005014EC` / `00501935` (`00500540`) and unloads `005014A3` / `00501839` (`004FEEC0`). No `006B3FF0`. No `StartOakVale` name.
4. **First-seen no-save current is dummy 0. This fn is not first-seen. DISPROVEN as StartOakVale current.** First body open is index 1 LookoutPoint.
5. **Host seed-only-after-`00501450`: LEFTOVER.** Native seed is `004A5DF3`. `TickWorld` MATCH that site. Do not invent `PlayerRegionName`.

---

## Open

| Item | Class |
|---|---|
| Who sets `ecx=CWorldMap` and enters `00501450` | **UNREAD** (named encodings 0) |
| Drop unused `EnqueueAfterDummy` | leftover API |
| Host in-loop `004FEEC0` omit (`CurrentRegionIndex==141`) | **DIVERGE** vs `00501839`; not this leftover |

---

## Files read

- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00500000.txt` (`00501450`…`00501990`; `00500540` pump skip)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-004c0000.txt` (`004FEEC0` `004FF03F` `+156=0`; `004FC8A0` `+156=arg`)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00480000.txt` (`004A5DF3` `006B3FF0`; `00487C20` `00487C55`)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00680000.txt` (`006B3FF0`; `006B4305` in `006B42F0`)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\e8.tsv` (dest `00501450` **0**; body sites **47**; dest `006B3FF0` = `004A5DF3` / `006B4305`)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\calls-by-dest.tsv` (dest `0x00501450` **0**; dest `0x00500540` six sites)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\functions.tsv` (`0x00501450` size 2248 swallow)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\abs.tsv` (operand `0x00501450` **0**)
- `C:\FableCSharp\docs\PARITY.md` (type-1 row; no-save enqueue row)
- `C:\FableCSharp\docs\runtime\FORWARD_TREE.md` §§8–9
- `C:\FableCSharp\proofs\00501450-inbound-computed\README.md`
- `C:\FableCSharp\proofs\00501450-inbound-ff\README.md`
- `C:\FableCSharp\proofs\00501450-host-leftover\README.md`
- `C:\FableCSharp\proofs\00501450-no-00449D90\README.md`
- `C:\FableCSharp\proofs\00501450-rdata-dwords\README.md`
- `C:\FableCSharp\proofs\first-region-after-leave\README.md`
- `C:\FableCSharp\proofs\wld-first-region\README.md`
- `C:\FableCSharp\proofs\startoakvale-index4-loader\README.md`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs` (`LoadFromFirstRealRegion` / `TickWorld` / `EnqueueAfterDummy`; read only)
- `C:\FableCSharp\tests\Fable.Formats.Tests\EngineLifecycleTests.cs` (`Second_pump_00501450_*` / `Pump_004166E2_*`; read only)
