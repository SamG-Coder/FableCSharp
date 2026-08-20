# `00501450` computed inbound leftover — still 0; native enqueue is `00500540` / `006C2120`

Investigation only. No production `src/` or `tests/` edits.

Do **not** start at Oakvale / `00DBDE40` / `CREATURE_HERO_CHILD`.
Do **not** treat `00501450` as Init Characters.
Do **not** invent a host `Pump` site. Dummy `Pump` /
`PumpGame` must **not** call `LoadFromFirstRealRegion`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: leftover from `proofs/00501450-inbound-computed`
and `proofs/00501450-inbound-ff`. Re-verify `ff.tsv` /
`vtbl.tsv` / `abs.tsv` / jmp tables for dest `00501450`.
If inbound is still 0, how does native ever enqueue
regions? Alternative: `006C2120` / `00500540` callers.

Authority: TLC dump `assembly/exe/01-sections/text-map/`
(`ff.tsv`, `abs.tsv`, `e8.tsv`, `calls-by-dest.tsv`,
`calls.tsv`, `branches.tsv`, `switch.tsv`,
`switch-ptrs.tsv`, `switch-index.tsv`, `functions.tsv`,
`listing-00500000.txt`, `listing-00480000.txt`,
`listing-006c0000.txt`, `listing-00880000.txt`,
`listing-00a80000.txt`);
`assembly/exe/00-index/` (`vtbl.tsv`, `xrefs.tsv`,
`rtti.txt`, `sections.txt`);
exe id `42D7DBDF-0106C000-16666624`;
siblings `proofs/00501450-inbound-computed`,
`proofs/00501450-inbound-ff`,
`proofs/00501450-rdata-dwords`,
`proofs/00501450-no-00449D90`,
`proofs/00501450-host-leftover`,
`proofs/00501450-e8-callers`,
`proofs/dummy-pumps-before-region`,
`proofs/first-region-after-leave`,
`proofs/host-00501450-timing`;
`EngineLifecycle.Pump` / `PumpGame` /
`LoadFromFirstRealRegion` / `EnqueueAfterDummy`
(read only).

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| Inbound dest `00501450` still 0? | **Yes.** `E8` / `E9` / `call` / `jmp` / FF abs / vtbl dest / switch dest / `abs` operand / `xrefs` / listing operand: **0** | **PROVEN** absence |
| inbound-ff leftover (isolated `.rdata`/`.data` dword `n<4`)? | **Closed.** Whole-PE `50 14 50 00` **0** (`rdata-dwords`). Re-grep dest **0** | **PROVEN** absence |
| inbound-computed leftover (two-imm ALU / get-PC)? | Named splits **0** (`0x00500000+0x1450`, image base `0x00400000`, RVA `0x00101450`, `0x00501400`, `0x00501000`). Sole `0x1450` is heap size `00AB6246` | **PROVEN** absence of named splits; unbounded get-PC **UNREAD** as a class |
| Who first reaches `00501450` after no-save New Game? | **Nobody recovered.** Dummy / type-1 / Present skip it | **PROVEN** skip; live site **UNREAD** |
| How does native enqueue regions without this inbound? | `00500540` apply → `006C27A0` job → `006C2120` list-insert `[loader+20]`. **Not** an inbound to `00501450` | **PROVEN** |
| `006C2120` inbound to `00501450`? | **No.** Four `E8`s, all **callees** (three in `00500540`, one in `00501990`) | **DISPROVEN** as inbound |
| `00500540` callers that skip `00501450`? | Persist `00487C55`; other-path `0050255D` (`00502500`); travel `00506455` (`00502E90`). `005025F8` (`005025B0`) has dest **0** like this fn | **PROVEN** |
| No-save recovered tree ever `E8`s `00500540` / `006C2120`? | **No.** Dummy index 0 only. WLD `004A1840` `006C20A0` empty, **no** `006C27A0` | **PROVEN** skip |
| Host leftover? | `EnqueueAfterDummy` unused by live `Pump`. Tests call `LoadFromFirstRealRegion()` **after** dummy | **LEFTOVER** glue; **DIVERGE** site; body **MATCH**; Pump skip **MATCH** |

**Hit count inbound: 0.** No inbound VAs.

**Inbound mechanism: still UNKNOWN** as a live site.
Every encoding that *names* `00501450` is empty.
Do **not** close that with dummy `Pump`.

Native *does* enqueue regions: through `00500540`,
which this fn would drive **if entered**. On the
recovered no-save walk it is never entered, and
neither is `00500540`. First real open (Lookout
index 1) is inside this body, not a recovered `E8`.

---

## Leftover from the two inbound notes

### `inbound-ff` leftover — **closed**

That note left isolated `.rdata`/`.data` dwords
(`ScanVtbls` drops `n<4`) **UNREAD**.
`00501450-rdata-dwords` already **PROVEN**-emptied
whole-PE `50 14 50 00` (aligned and unaligned).

This re-grep of the indexes that note used:

| Index | Dest / mem / operand `0x00501450` | Class |
|---|---|---|
| `ff.tsv` mem `[0x00501450]` | **0** | **PROVEN** |
| `ff.tsv` last col `0x00501450` | 21 **outbound** (`005016B9`…`00502E6D`; containing-fn / swallow) | **DISPROVEN** as inbound |
| `00-index/vtbl.tsv` dest | **0** | **PROVEN** |
| `switch.tsv` / `switch-ptrs.tsv` / `switch-index.tsv` | **0** | **PROVEN** |
| `abs.tsv` operand `\t0x00501450\t` | **0** | **PROVEN** |
| `branches.tsv` dest `\t0x00501450\t` | **0** | **PROVEN** |
| `calls-by-dest.tsv` dest `^0x00501450\t` | **0** | **PROVEN** |
| `e8.tsv` dest `0x00501450` | **0** | **PROVEN** |
| listing `call 00501450` / `jmp 00501450` | **0** | **PROVEN** |
| `xrefs.tsv` `00501450` | **0** | **PROVEN** empty as string xref |

`ff.tsv` header `site kind mem disp fn`. Last
column is **containing function**, not callee.
`functions.tsv` start `0x00501450` size **2248**
swallows `00501990` UpdateNavMaps (real enqueue
`ret` is `00501985` / `int3` / `00501990`). None
of those 21 rows have mem `[0x00501450]`.

CWorld-shaped vtbl `0x01244AEC` (RTTI `CWorldMap`
`0x0137B720`) slots 0–30: `0051D1E0` `005022F0`
`00507610` `00507C30` … `00502460`. **No**
`00501450`. **No** `00501990`.

### `inbound-computed` leftover — named splits **closed**; get-PC class **unread**

That note left two-immediate ALU / get-PC add
that yields `00501450` from values **other** than
VA, RVA, or `0x00500000+0x1450`.

| Split | Hits | Class |
|---|---|---|
| Operand `0x00500000` | **0** listings | **PROVEN** (sibling) |
| `mov eax, 0x1450` | **1:** `00AB6246` then `call 00BFEA30` (heap size, not code-base add) | **DISPROVEN** as inbound |
| `abs.tsv` `0x00400000` (image base) | **0** | **PROVEN** (this scan) |
| `abs.tsv` / listing `0x00101450` (RVA) | **0** | **PROVEN** |
| `abs.tsv` `0x00501400` / `0x00501000` | **0** | **PROVEN** |
| Whole-PE dword VA `50 14 50 00` / RVA `50 14 10 00` | **0** (siblings) | **PROVEN** |
| `add` / `lea` of VA or RVA | **0** (sibling) | **PROVEN** |
| `call r32` 24-insn window names dest | **0** / 1179 (sibling) | **PROVEN** |
| Get-PC `call $+5` / `pop` / add of an unnamed pair | not exhaustible without dataflow | **UNREAD** as a class |

No `.reloc` (`sections.txt`: `.text` `.rdata`
`.data` `.idata` `_PDATA` `.rsrc` only). Shipping
code pointers in this PE are absolute VAs. A
computed result that was **stored** would still
be dword `50 14 50 00` (**0**). The remaining
hole is a register-only ALU that never writes
the VA. Do **not** treat that as a recovered
caller.

---

## How native enqueues regions (without inbound to this fn)

`00501450` is a CWorldMap thiscall that *would*
loop `00500540(i,0,0)`. The apply / queue is
**`00500540`**, not the unread entry.

```
00500540  sub esp, 0x98
          mov edi, ecx                  ; CWorldMap
          index = [esp+172]
          record = [map+44]+index*88
          …
00500D7A  call 006C27A0                 ; push job
00500D8A  call 006C2120                 ; insert [loader+20]
00500DA0  call 006C20A0                 ; pump-until-empty when arg3=0
          ; same pair at 005010AE/005010BE and 00501319/00501329
```

`006C2120` (`listing-006c0000.txt`):

```
006C2120  mov esi, ecx                  ; loader
          call 006C20B0
          mov esi, [esi+20]
          push 16
          call 00BFEA0E                 ; alloc node
          ; link onto [loader+20] list
          ret 4
006C2170  … "Loading topology" …        ; apply; not this inbound
```

`006C2120` is **SetAsLoading** list-insert.
String `"CWorldMap::UpdateNavMaps - SetAsLoading"`
sits at `00501C16` **after** `00501985 ret` —
inside `00501990`, not this enqueue body.

### `00500540` — every `E8` (`calls-by-dest` dest `0x00500540`)

| Site | Tagged fn | Real parent | Args | No-save recovered? |
|---|---|---|---|---|
| `00487C55` | `00487BD0` | persist `00487C20` | `(index,0,1)` after name lookup | **No.** Needs nonempty `PlayerRegionName`. Empty no-save. **DISPROVEN** as this walk |
| `005014EC` | `00501450` | **this fn** | `(i,0,0)` loop; first `i=1` Lookout | Body **PROVEN**; inbound **UNREAD** |
| `00501935` | `00501450` | **this fn** | `(saved,0,1)` restore, no pump | same |
| `0050255D` | `00501450` (swallow) | **`00502500`** | after `004FEEC0`; `(ebx, arg, 1)` | Parent has `E8`; first-seen `[world+260]` skip. **DISPROVEN** as no-save first |
| `005025F8` | `00501450` (swallow) | **`005025B0`** | `(saved,0,1)` | Dest `005025B0` **0** — same unread class as this fn |
| `00506455` | `00502E90` | **`00502E90`** | after `004FEEC0` | Parent has `E8` `0065C7B4` / `008A1CAD`. Later travel, not dummy |

Six sites. Two live inside the unread bulk walk.
The other four do **not** need `00501450`.

### `006C2120` — every `E8` (dest `0x006C2120`)

| Site | Tagged fn | Real parent |
|---|---|---|
| `00500D8A` | `004FF900` (gap) | **`00500540`** |
| `005010BE` | `004FF900` (gap) | **`00500540`** |
| `00501329` | `004FF900` (gap) | **`00500540`** |
| `00501C48` | `00501450` (swallow) | **`00501990` UpdateNavMaps** |

**0** of these call `00501450`. `006C27A0` dest
rows are the same four parents (`00500D7A` /
`005010AE` / `00501319` / `00501C0E`).

`00501990` itself **does** have inbound
(`004B2652` / `004B3C2D` in `004B2510`). That is
nav-map update **after** maps exist, not the
unread bulk enqueue.

`004A1840` WLD parse **does** `E8` `006C20A0`
(`004A1AA3`) and it is **empty** (no
`006C27A0` / `006C2120`). Parse is not apply.

### Other recovered region paths (not this fn)

| Fn | Inbound | Role |
|---|---|---|
| `00501450` | **0** | bulk `00500540(i,0,0)` then restore |
| `005025B0` | **0** | pair + `004FEEC0` + `00500540(saved,0,1)` |
| `00502500` | `004A4CB9` (`004A3740` ← `004A5BFB`), `0089B99E` (`00892D80`) | other apply; `[eax+48]` thiscall then `E8`. First-seen `[world+260]=0` skips `004A3740` |
| `00501990` | `004B2652` `004B3C2D` | UpdateNavMaps; `006C2120` callee |
| `00487C20` | `00487F10` (`00487BD0`) | persist name → `00500540`. **Do not invent** on no-save |
| `00502E90` | `0065C7B4` `008A1CAD` (+ self `00506598`) | later `00500540` at `00506455` |

Contrast: if an `E8` of `00501450` existed, dest
would have a row (see `00502500`).

---

## No-save recovered tree (still skip)

```
0042F2A2  Leave frontend
0042F491  Init Game → 004184BD
  00416953  Loading world
    004A1840  00507C30 .wld / 004FDBC0 .tng     // 006C20A0 empty
    00416BCA  0049F180(0)                        // 0 of 00501450 / 00500540
  user.ini Gameflow                              // 0
004189C2  dummy pumps
  00418A48  004FB150 / 004FC180 index 0          // +36 null
  fade 00B239A0 once
  loop 00418AB1 until WM_DESTROY                 // type-1 still 0 of these dests
00435F70  first Present                          // still dummy
```

| When | `+156` | `00501450` | `00500540` | `006C2120` |
|---|---|---|---|---|
| Dummy / type-1 / first Present | **0** dummy | **0** | **0** | **0** |
| Persist `00487C20` | n/a | **0** | would, name empty | would if apply ran |
| This body **if** entered | saved 0 then loop | — | `i=1` Lookout first | inside each sync apply |

Native no-save **does not enqueue a real region**
on the recovered tree. Dummy probes index 0.
First authored region remains Lookout (WLD
`NewRegion 1`) **unapplied** until some unread
entry hits `00501450` or a later recovered
`00500540` path (`00502500` / persist /
`00502E90`).

That is **not** a license to wire dummy `Pump`
to this fn. Host already **MATCH**es the skip.

---

## Host leftover (do not wire `Pump`)

```
Pump(float)  Game → PumpGame only
PumpGame dummy: 004FB150 / 004FC180 / fade / one inner
LoadFromFirstRealRegion: explicit API (00501450 body)
EnqueueAfterDummy: leftover glue; production Pump never calls it
PumpCallsLoadFromFirstRealRegion = false
LoadFromFirstRealRegionNamedInbound = 0
```

| Site | vs dump | Class |
|---|---|---|
| `Pump` / `PumpGame` never `LoadFromFirstRealRegion` | dummy skip | **MATCH** |
| `EnqueueAfterDummy` exists; unused by live `Pump` | invented **when** | **LEFTOVER** glue |
| Tests `Second_pump_00501450_*`: dummy `Pump`s **then** `life.LoadFromFirstRealRegion()` | stand-in | **DIVERGE** site; body **MATCH** |
| `Persist_PlayerRegionName_is_00487C20_not_new_game` calls `EnqueueAfterDummy` with a **set** name | persist arm, not no-save | leftover **when** on dummy; persist dest **MATCH** `00487C20` |

Do **not** hook `00501450` onto dummy `Pump`.
Do **not** treat `006C2120` as the missing
inbound.

---

## Not these

| Candidate | Class |
|---|---|
| Isolated rdata dword `n<4` still UNREAD | **DISPROVEN** (closed by `rdata-dwords`; re-grep 0) |
| `ff.tsv` last-col `00501450` is inbound | **DISPROVEN** (containing-fn / swallow) |
| `006C2120` / `006C27A0` inbound to this fn | **DISPROVEN** (callees of `00500540` / `00501990`) |
| Dummy `004189C2` / `004FB150` / `004FC180` | **DISPROVEN** |
| Persist `00487C20` as no-save first open | **DISPROVEN** (empty `PlayerRegionName`) |
| `00502500` / `004A3740` as this fn | **DISPROVEN** (other path; has `E8`; first-seen skip) |
| `00501990` UpdateNavMaps as this body | **DISPROVEN** (`ret` `00501985` then `int3`) |
| Host dummy `Pump` → `LoadFromFirstRealRegion` | **DISPROVEN**; leftover glue only |
| Fall-through / `int3` pad `00501444`…`0050144F` | **DISPROVEN** (`00501441 ret 32`) |
| `00AB6246` `0x1450` | **DISPROVEN** (heap) |
| Oakvale / `00DBDE40` as this entry | **DISPROVEN** |
| Image-base + RVA / `0x00501400` / `0x00501000` ALU | **PROVEN** absence of those immediates |

---

## Classifications (short)

1. **Inbound to `00501450` still 0. PROVEN absence** of every named encoding (`E8`/`E9`/FF abs/vtbl/switch/`abs`/listing/`call r32`/VA dword/RVA dword). inbound-ff rdata leftover is **closed**.
2. **Named ALU splits leftover from inbound-computed: PROVEN absence.** Unbounded get-PC add remains **UNREAD** as a class, not a hit. Do not invent `Pump`.
3. **Native enqueue is `00500540` → `006C27A0` / `006C2120`. PROVEN.** `006C2120` is a **callee**, not the missing inbound.
4. **No-save recovered tree never enters that apply. PROVEN skip.** Dummy stays index 0. First real open is inside this unread body (Lookout `i=1`) or a later recovered `00500540` path.
5. **Host leftover is unused `EnqueueAfterDummy` + test stand-in. MATCH skip on live `Pump`. Do not wire dummy `Pump` to `00501450`.**

---

## Open

| Item | Class |
|---|---|
| Who sets `ecx=CWorldMap` and enters `00501450` | **UNREAD** (named encodings 0) |
| Register-only get-PC / two-imm ALU that never stores the VA | **UNREAD** as a class |
| Drop unused `EnqueueAfterDummy` | leftover API; not an inbound encoding |
| When `00502500` / `00502E90` first run after dummy | later recovered paths; not this leftover |

---

## Files read

- `C:\FableCSharp\proofs\00501450-inbound-computed\README.md`
- `C:\FableCSharp\proofs\00501450-inbound-ff\README.md`
- `C:\FableCSharp\proofs\00501450-rdata-dwords\README.md`
- `C:\FableCSharp\proofs\00501450-no-00449D90\README.md`
- `C:\FableCSharp\proofs\00501450-host-leftover\README.md`
- `C:\FableCSharp\proofs\00501450-e8-callers\README.md`
- `C:\FableCSharp\proofs\dummy-pumps-before-region\README.md`
- `C:\FableCSharp\proofs\first-region-after-leave\README.md`
- `C:\FableCSharp\proofs\host-00501450-timing\README.md`
- `C:\FableCSharp\assembly\exe\manifest.json` (exe id `42D7DBDF-0106C000-16666624`)
- `C:\FableCSharp\assembly\exe\00-index\sections.txt` / `vtbl.tsv` (`0x01244AEC` slots 0–30; dest `00501450` **0**) / `xrefs.tsv` / `rtti.txt` (`CWorldMap` `0x0137B720`)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\INDEX.md`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\ff.tsv` (header; mem `[0x00501450]` **0**; last-col outbound 21)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\abs.tsv` (operand `0x00501450` **0**; `0x00400000` **0**; `0x00101450` **0**; `0x00501400` **0**; `0x00501000` **0**)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\e8.tsv` / `calls-by-dest.tsv` / `calls.tsv` / `branches.tsv` / `functions.tsv`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\switch.tsv` / `switch-ptrs.tsv` / `switch-index.tsv` (dest **0**)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00500000.txt` (`00500540` / `00500D7A`–`00501329` / `00501441`–`00501990` / `00502500` / `005025B0` / `00506455`)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00480000.txt` (`00487C20` `00487C55`; `004A4CB9` → `00502500`; `004A5BFB` → `004A3740`)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-006c0000.txt` (`006C2120` list-insert; `006C2170` apply)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00880000.txt` (`0089B99E` → `00502500`)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00a80000.txt` (`00AB6246` `mov eax, 0x1450`)
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs` (`Pump` / `PumpGame` / `LoadFromFirstRealRegion` / `EnqueueAfterDummy`; read only)
- `C:\FableCSharp\tests\Fable.Formats.Tests\EngineLifecycleTests.cs` (`Second_pump_00501450_*` / `Persist_PlayerRegionName_*`; read only)
