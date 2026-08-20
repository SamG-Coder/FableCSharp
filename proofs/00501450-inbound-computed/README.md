# Remaining inbound encodings to `00501450` — `call r32` / FF /2 / dispatcher / rdata / vtbl-load

Investigation only. No production `src/` or `tests/` edits.

Do **not** start at Oakvale / `00DBDE40` / `CREATURE_HERO_CHILD`.
Do **not** treat `00501450` as Init Characters.
Do **not** invent a host `Pump` site. Dummy `Pump` /
`PumpGame` must **not** call `LoadFromFirstRealRegion`.

Do **not** re-prove `E8` / `E9` / `.text` imm / vtbl-run
(`n≥4`). Those are already **PROVEN** empty in
`proofs/00501450-inbound-ff`,
`proofs/00501450-rdata-dwords`,
`proofs/00501450-no-00449D90`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: remaining inbound encodings to `00501450`:
`call r32`, FF /2, computed dispatcher, rdata function
pointer tables, vtbl slots that load the address then
`call`. Who first reaches `00501450` after no-save
New Game?

Authority: TLC `Fable.exe` via `GameInstall`
(`C:\Program Files (x86)\Steam\steamapps\common\Fable The Lost Chapters\Fable.exe`);
exe id `42D7DBDF-0106C000-16666624`;
`assembly/exe/01-sections/text-map/`
(`listing-*.txt`, `ff.tsv`, `calls-by-dest.tsv`,
`functions.tsv`, `abs.tsv`, `branches.tsv`,
`switch*.tsv`);
`assembly/exe/00-index/` (`vtbl.tsv`, `xrefs.tsv`,
`rtti.txt`, `sections.txt`);
`GrepFacts.TryFf` (indexes `call [` / `jmp [` only);
siblings `proofs/00501450-inbound-ff`,
`proofs/00501450-rdata-dwords`,
`proofs/00501450-no-00449D90`,
`proofs/00501450-host-leftover`,
`proofs/dummy-pumps-before-region`,
`proofs/first-region-after-leave`,
`proofs/host-00501450-timing`.

`ff.tsv` does **not** index `call eax` / `call ecx`
(mod=11 FF /2). That was the inbound-ff leftover this
note closes.

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| Listing-decoded `call r32` that names `00501450`? | **0** / **1179** sites. 24-insn window never contains `00501450` | **PROVEN** absence |
| Listing-decoded `jmp r32` that names `00501450`? | **0** / **10** sites | **PROVEN** absence |
| Any listing operand text `00501450` (not the address column)? | **0** across every `listing-*.txt` | **PROVEN** absence |
| FF /2 `call [0x00501450]` / `jmp [0x00501450]`? | **0** (`ff.tsv`; restated as the abs form) | **PROVEN** absence |
| FF /2 `call [r]` / `call [r+disp]` dest = `00501450`? | Needs a stored dword. Whole-PE VA **0**, RVA **0** | **PROVEN** absence of the dest |
| Type-table computed dispatcher (`call edx` via `0x13B9288`)? | All **6** `call edx` sites are that table. Dest dwords live in `.data`. Datascan **0** | **PROVEN** not this dest |
| Other `call ecx` (12 sites)? | IAT / D3D / audio callbacks. None load `00501450` | **DISPROVEN** as inbound |
| rdata / data function-pointer table dword `00501450`? | VA `50 14 50 00` **0** (sibling). RVA `50 14 10 00` **0** (this scan) | **PROVEN** absence |
| vtbl slot that holds `00501450` then `call r32` / `call [eax+N]`? | `vtbl.tsv` dest **0**. CWorld `0x01244AEC` slots 0–30 have nearby `00502*` / `00507*`, **no** `00501450` | **PROVEN** absence |
| `add` / `lea` of VA `00501450` or RVA `00101450`? | **0** | **PROVEN** absence |
| Split `0x00500000 + 0x1450`? | Operand `0x00500000` **0**. Sole `mov eax, 0x1450` is alloc size at `00AB6246` | **DISPROVEN** as inbound |
| Who first reaches `00501450` after no-save New Game? | **Nobody recovered.** Dummy `004189C2` / type-1 / Present skip it. Host `Pump` never calls it | **PROVEN** skip; **UNREAD** as a live native site |
| Host leftover? | `EnqueueAfterDummy` glue + tests that call `LoadFromFirstRealRegion()` after dummy | **LEFTOVER** glue; **DIVERGE** site; body **MATCH** |

**Hit count: 0.** No inbound VAs.

**Inbound mechanism: still UNKNOWN** as a live site.
Every encoding that *names* `00501450` is empty,
including the register-call / FF /2 / table / vtbl-load
forms inbound-ff left open. Do **not** invent `Pump`.

---

## Evidence → Original → Host → Gap

| Layer | What |
|---|---|
| **Evidence** | Listing-decoded FF /2 `call r32` / `jmp r32`; whole-PE dword VA + RVA; type-table `call edx` complete set; CWorld vtbl `0x01244AEC`; no-save walk after Leave |
| **Original** | `00501450` is a CWorldMap thiscall (`mov edi, ecx`) that pair-gets the player, `004FEEC0`, then loops `00500540(i,0,0)`. **No** instruction, table, or vtbl *names* that VA. Contrast: sibling `00501990` **does** have `E8` (`004B2652` / `004B3C2D`); sibling `00502500` **does** (`004A4CB9` / `0089B99E`). Both also missing from `vtbl.tsv` |
| **Host** | `Pump` / `PumpGame` never call `LoadFromFirstRealRegion` / `EnqueueAfterDummy` (**MATCH** skip vs dummy). Tests fire the body as a stand-in after dummy pumps (**DIVERGE** site) |
| **Gap** | Who (if anyone) enters native `00501450` on a later unread walk. Next encoding that is **not** 0: two-immediate ALU / get-PC add that produces `00501450` from values other than VA, RVA, or `0x00500000+0x1450` |

---

## Direct answers

| Hunt | Hits | Class |
|---|---|---|
| Listing `call eax/ecx/edx/ebx/esp/ebp/esi/edi` | **1179** (519/12/6/149/0/153/121/219) | indexed |
| Those sites whose 24-insn prelude contains `00501450` | **0** | **PROVEN** |
| Listing operand/text `00501450` (address column stripped) | **0** | **PROVEN** |
| Listing `jmp r32` | **10**; windows **0** | **PROVEN** |
| `ff.tsv` `call [0x00501450]` / `jmp [0x00501450]` | **0** | **PROVEN** |
| Whole-PE dword `50 14 50 00` (VA) | **0** | **PROVEN** (sibling; re-scanned) |
| Whole-PE dword `50 14 10 00` (RVA `00101450`) | **0** | **PROVEN** (new) |
| Whole-PE dword `50 14 01 00` | **0** | **PROVEN** |
| `add r32, 0x00501450` / `0x00101450` | **0** | **PROVEN** |
| `lea r32, [r+0x00501450]` / `[r+0x00101450]` | **0** | **PROVEN** |
| Operand `0x00500000` | **0** | **PROVEN** |
| `call edx` (complete set) | **6**, all `0x13B9288` type table | **PROVEN** not this dest |
| `vtbl.tsv` dest `0x00501450` | **0** | **PROVEN** (restated) |
| CWorld `0x01244AEC` slot = `00501450` | **0** / 31 slots | **PROVEN** |
| `xrefs.tsv` `00501450` | **0** | **PROVEN** empty as string xref |
| `calls-by-dest` dest `0x00501450` | **0** (not re-proved; contrast below) | restated |

Raw `.text` bytes `FF D0–D7` count **1273** (includes
misaligned false positives). Listing-decoded **1179**
is the authority.

---

## `call r32` / `jmp r32` (the `ff.tsv` hole)

`X86.Ff` prints mod=11 FF /2 as `call eax` (no `[`).
`GrepFacts.TryFf` only matches `call [` / `jmp [`, so
`ff.tsv` never listed register calls. `TryRelTarget`
also rejects `call eax` (rest is not a hex dest).

Listing-decoded `call r32`:

| `r32` | Count |
|---|---|
| eax | 519 |
| ecx | 12 |
| edx | 6 |
| ebx | 149 |
| esp | 0 |
| ebp | 153 |
| esi | 121 |
| edi | 219 |
| **total** | **1179** |

A 24-instruction prelude on **every** site: **0**
mentions of `00501450`. Operand scan of the whole
`.text` listing (address column removed): **0**.
There is no `mov r32, 0x00501450` / `lea r32,
[0x00501450]` / `push 0x00501450` sitting above a
`call r32`.

`jmp r32` (10): `0083F483` `0098F9B9` `00996E99`
`0099AE89` `00A5063D` `00BE4F42` `00BE5901`
`00C07226` `00C216BC` `00C3D0AE`. CRT / D3D / mesh.
None name `00501450`.

---

## FF /2 memory forms

`call [mem]` listing count **76272**. Those **are**
in `ff.tsv`.

| Form | Dest `00501450`? |
|---|---|
| `call [0x00501450]` / `jmp [0x00501450]` | **0** |
| `call [r]` / `call [r+disp]` / `call [r+disp32]` | dest is the dword at that address. VA dword **0**, so no static cell |

A vtbl-shaped `mov eax, [ecx]; call [eax+N]` still
needs slot N to hold `00501450` on disk. `vtbl.tsv`
dest **0**. Runtime fill of that slot still needs the
immediate. Whole-PE `50 14 50 00` **0**.

---

## Computed dispatcher

Every listing `call edx` (6) is the object-type table
at `0x13B9288` (type id `shl 6`):

```
00434A4E  call edx     ; [table+32]  + this-adjust [table+36]
00434A96  call edx     ; [table+48]
0049D99D  call edx     ; [table+0]
0049E00A  call edx
0049E050  call edx     ; [0x13B92C8]  type-1 slot
0049E1EE  call edx     ; [table+16]
```

Destinations are `.data` function dwords. Whole-PE
datascan of `00501450` is **0**, so **no** type slot
is this fn. First-seen type-1 `[0x13B92F8]=0`
already skips the `+48` arm
(`docs/PARITY.md`); even if it ran, the dword is not
`00501450`.

`call ecx` (12) loads from object fields / globals
(`[esi+4]+12`, `[0x143BB48]`, `[esi+220]`, …).
Audio / D3D / filter callbacks. Not CWorldMap, not
this VA.

No `add r32, 0x00101450` / `lea [r+RVA]` that would
turn a relative table into this VA. No `.reloc`
section (`sections.txt`: `.text` `.rdata` `.data`
`.idata` `_PDATA` `.rsrc` only). Shipping pointers
in this PE are absolute VAs.

---

## rdata function pointer tables

Sibling `00501450-rdata-dwords` already **PROVEN**-emptied
the VA dword (`50 14 50 00`) in `.rdata` / `.data` /
whole PE, including `n<4` short runs `ScanVtbls` drops.

This scan adds the RVA encoding a relative table
would store (`00101450` = `50 14 10 00`): **0**.
File-offset-shaped `50 14 01 00`: **0**.

`switch*.tsv` dest `00501450`: **0** (sibling;
tables live in `.text` as absolute VAs).

No on-disk table, short or long, names this fn as a
pointer or as an RVA.

---

## vtbl slots that load then `call`

CWorld-shaped vtbl `0x01244AEC` (`rtti.txt` CWorldMap
`0x0137B720`). Slots 0–30:

`0051D1E0` `005022F0` `00507610` `00507C30`
`004FDA60` `004FF8B0` `004FF080` `004FF440` …
`00502460`. Nearby `00502*` / `00507*` methods.
**No** `00501450`. **No** `00501990`.

`0x01244AB4`: `004FB*` / `004FC*` getters, **no**
`00501450`.

A slot that *held* `00501450` would appear as
`vtbl.tsv` dest and as the VA dword. Both **0**.

A slot that was a thunk (`jmp 00501450` / `mov eax,
00501450; jmp eax`) would still be an `E9` / imm /
`jmp r32` naming the dest. Those are **0**.

`005022F0` (slot 1) is a real body (`sub esp, 8`;
`call [eax+12]`; `0051FBA0`). Not a trampoline to
`00501450`.

---

## Contrast — siblings that *do* have inbound

`functions.tsv` start `0x00501450` size **2248**
swallows `00501990` UpdateNavMaps (real enqueue
`ret` is `00501985` / `int3` / `00501990`). That
tag is containing-fn, not a caller.

| Fn | Role | `calls-by-dest` | vtbl dest |
|---|---|---|---|
| `00501450` | region enqueue (thiscall `ecx=map`) | **0** | **0** |
| `00501990` | `CWorldMap::UpdateNavMaps` | `004B2652` `004B3C2D` (`004B2510`) | **0** |
| `00502500` | other region path | `004A4CB9` (`004A3740`), `0089B99E` (`00892D80`) | n/a |
| `005025B0` | same pair then `004FEEC0` / `00500540(saved,0,1)` | **0** | **0** |

`004A4CB9`: `mov ecx, eax` after `call [eax+48]`,
then `E8 00502500`. First-seen `[world+260]=0`
skips that parent. The index **would** have listed
`00501450` if an `E8` existed.

`004B2652`: `mov ecx, eax` after `call [edx+48]`,
then `E8 00501990`. Same thiscall shape, recovered
caller. Enqueue does not get that treatment.

---

## Who first reaches `00501450` after no-save New Game?

**Nobody on the recovered tree.**

```
0042F2A2  Leave frontend
0042F491  Init Game → 004184BD
  00416953  Loading world / 0049F180(0)     // 0 inbound of 00501450
  user.ini Gameflow                         // 0
004189C2  dummy pumps                       // 0
  00418A48  004FB150 / 004FC180 index 0
  00418A52  call [eax+52]                   // FF /2 vtbl; dest not 00501450
  fade 00B239A0 once
  loop 00418AB1 until WM_DESTROY            // type-1 still 0
```

Dummy prefix, fade install, first inner, later dummy
inners, first type-1, and first Present all skip it
(siblings `dummy-pumps-before-region`,
`host-00501450-timing`, `docs/PARITY.md`).

`00501450` is **thiscall** `ecx=CWorldMap`
(`[0x13B86A0]` is used *inside* for the player pair,
not as the inbound). Who sets `ecx` and enters the
prologue is not a stored pointer, not a register
call that names the dest, not the type table, not a
CWorld vtbl slot.

Host `Pump` / `PumpGame`: **never**
`LoadFromFirstRealRegion` / `EnqueueAfterDummy`.
`FirstRealRegionLoadDone` stays false through dummy
pumps. **MATCH** skip.

Host leftover is the **when**, not a recovered
native site:

- `EnqueueAfterDummy` exists to fire `00501450` (or
  persist `PlayerRegionName`) as if dummy pumps were
  the trigger. Production `Pump` never calls it.
- Tests call `life.LoadFromFirstRealRegion()`
  **after** dummy `Pump`s (`Second_pump_00501450_*`).
  Stand-in. **DIVERGE** site, body **MATCH**.

Do **not** wire dummy `Pump` to this fn.

---

## Next unread encoding (still 0 named dests)

Closed by this note:

| Encoding | Class |
|---|---|
| `call r32` / `jmp r32` | **PROVEN** absence |
| FF /2 abs `[0x00501450]` | **PROVEN** absence |
| FF /2 `[r]` / `[r+disp]` with a stored dest | **PROVEN** absence (VA+RVA 0) |
| Type-table / `call edx` dispatcher | **PROVEN** not this dest |
| rdata/data ptr tables (VA and RVA) | **PROVEN** absence |
| vtbl load-then-`call` | **PROVEN** absence |

Still not recovered:

| Item | Class |
|---|---|
| Two-immediate ALU / get-PC `add` that yields `00501450` from values **other** than VA, RVA, or `0x00500000+0x1450` | **UNREAD** (not exhaustible without dataflow; no `.reloc` PIC pattern found) |
| Who sets `ecx=CWorldMap` and enters `00501450` on some later unread walk | **UNREAD** as a live site |
| Function unreferenced on this PE (dead for no-save New Game) | **PARTIAL** — every *named* encoding is empty; do not treat that as a recovered “never” without closing the ALU hole |

The `00AB6246` `mov eax, 0x1450` / `call 00BFEA30`
is an **alloc size**, not a code-base add.
**DISPROVEN** as inbound.

---

## Not these

| Candidate | Class |
|---|---|
| `E8` / `E9` / `.text` imm / vtbl-run `n≥4` / switch dest | already **PROVEN** empty (siblings; not re-proved) |
| Isolated `.rdata`/`.data` dword `n<4` = VA | already **PROVEN** empty (`rdata-dwords`) |
| RVA dword / relative ptr table | **PROVEN** empty (this note) |
| Dummy `004189C2` / `004FB150` / `004FC180` / type-1 / Present | **DISPROVEN** (`dummy-pumps-before-region`) |
| Host `Pump` → `LoadFromFirstRealRegion` | **DISPROVEN** as native inbound; leftover glue only |
| `006C2120` inbound | **DISPROVEN** (callee of `00501990`) |
| `00502500` / `004A3740` as this fn | **DISPROVEN** (other path; has `E8`) |
| `0x13B9288` type-table dest | **DISPROVEN** |
| `00AB6246` `0x1450` | **DISPROVEN** (heap size) |
| Fall-through / `int3` pad `00501444`…`0050144F` | **DISPROVEN** (sibling) |
| Oakvale / `00DBDE40` as this entry | **DISPROVEN** |

---

## Classifications (short)

1. **`call r32` / `jmp r32` inbound to `00501450`: PROVEN absence** (1179+10 listing sites; 0 operand `00501450`).
2. **FF /2 abs and FF /2 through a stored dword: PROVEN absence** (VA 0, RVA 0).
3. **Computed type-table dispatcher: PROVEN not this dest.** All six `call edx` are `0x13B9288`.
4. **rdata ptr tables + vtbl load-then-call: PROVEN absence.**
5. **Who first reaches `00501450` after no-save New Game: nobody recovered. PROVEN skip** on Leave → dummy → type-1. Host `Pump` **MATCH** skip. **UNREAD** as a live native site. Next encoding: two-immediate ALU.

---

## Open

| Item | Class |
|---|---|
| Two-immediate ALU / get-PC add → `00501450` | **UNREAD** |
| Live native entry after dummy pumps | **UNREAD** |
| Drop unused `EnqueueAfterDummy` | leftover API; not an inbound encoding |

---

## Files read

- `C:\Program Files (x86)\Steam\steamapps\common\Fable The Lost Chapters\Fable.exe` (whole-PE dword VA/RVA; `add`/`lea` of VA/RVA)
- `C:\FableCSharp\assembly\exe\manifest.json`
- `C:\FableCSharp\assembly\exe\00-index\sections.txt` / `vtbl.tsv` (`0x01244AEC` / dest `00501450` **0**) / `xrefs.tsv` / `rtti.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\INDEX.md`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-*.txt` (`call r32` / `jmp r32` / operand `00501450` / `0x1450` / `0x00500000`)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00500000.txt` (`00501450`…`00501985`, `00501990`, `005022F0`, `00502500`, `005025B0`)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00400000.txt` (`00434A4E` / `00434A96` `call edx`)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00480000.txt` (`0049D99D`…`0049E1EE`; `004A4CB9` → `00502500`; `004B2652` → `00501990`)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00a80000.txt` (`00AB6246`)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\ff.tsv` / `calls-by-dest.tsv` / `functions.tsv` / `abs.tsv` / `branches.tsv` / `switch*.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\GrepFacts.cs` (`TryFf` `call [` only)
- `C:\FableCSharp\tools\Fable.ExeIndex\X86.cs` (`Ff` mod=11 → `call eax`)
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\world\` (UpdateNavMaps strings; CWorldMap RTTI)
- `C:\FableCSharp\proofs\00501450-inbound-ff\README.md`
- `C:\FableCSharp\proofs\00501450-rdata-dwords\README.md`
- `C:\FableCSharp\proofs\00501450-no-00449D90\README.md`
- `C:\FableCSharp\proofs\00501450-host-leftover\README.md`
- `C:\FableCSharp\proofs\dummy-pumps-before-region\README.md`
- `C:\FableCSharp\proofs\first-region-after-leave\README.md`
- `C:\FableCSharp\proofs\host-00501450-timing\README.md`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs` (`LoadFromFirstRealRegion` / `EnqueueAfterDummy`; read only)
