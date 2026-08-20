# Inbound to `00501450` (region enqueue) — FF / vtbl / switch / rdata

Investigation only. No production `src/` or `tests/` edits.

Do **not** start at Oakvale / `00DBDE40` / `CREATURE_HERO_CHILD`.
Do **not** treat `00501450` as Init Characters.
Do **not** invent a host `Pump` site. Dummy `Pump` /
`PumpGame` must **not** call `LoadFromFirstRealRegion`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: how native transfers control to `00501450`
(region enqueue). `calls-by-dest.tsv` has **0** dest
row (confirmed). Hunt FF / vtbl / switch / rdata
dwords, not a fake `E8`.

Authority: siblings
`proofs/00501450-host-leftover`,
`proofs/00501450-no-00449D90`,
`proofs/host-00501450-timing`,
`proofs/first-region-after-leave`,
`proofs/dummy-pumps-before-region`;
dump `assembly/exe/01-sections/text-map/`
(`calls-by-dest.tsv`, `e8.tsv`, `ff.tsv`,
`abs.tsv`, `branches.tsv`, `switch.tsv`,
`switch-ptrs.tsv`, `switch-index.tsv`,
`functions.tsv`, `listing-00500000.txt`,
`listing-004c0000.txt`, `listing-006c0000.txt`,
`listing-00400000.txt`, `listing-00480000.txt`,
`listing-01200000.txt`);
`assembly/exe/00-index/vtbl.tsv`,
`xrefs.tsv`, `rtti.txt`, `sections.txt`;
`EngineLifecycle.Pump` / `PumpGame` /
`LoadFromFirstRealRegion` / `EnqueueAfterDummy`
(read only).

There is **no**
`assembly/exe/01-sections/text-map/vtbl.tsv`.
Rdata code-pointer runs live in
`assembly/exe/00-index/vtbl.tsv`.

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| `calls-by-dest` dest `0x00501450`? | **0 rows.** No dest starting `0x005014` | **PROVEN** absence |
| `.text` `E8` / `E9` / `call` / `jmp` of `00501450`? | **0.** Listings: only the prologue `push ebp` | **PROVEN** absence |
| `ff.tsv` inbound (`[0x00501450]` / dest)? | **0.** Rows tagged `fn=0x00501450` are **outbound** FF **inside** the swallowed blob | **PROVEN** not inbound |
| `vtbl.tsv` dest `0x00501450`? | **0** (runs of ≥4 code pointers in `.rdata`/`.data`) | **PROVEN** absence |
| `switch*.tsv` dest `0x00501450`? | **0** | **PROVEN** absence |
| `.text` abs / branch dest `0x00501450`? | **0** (`abs.tsv` / `branches.tsv` last col is containing fn) | **PROVEN** absence |
| `004FB150` / `004FC180` / dummy `004189C2` call it? | **No.** Getters + dummy probe. `PumpGame` Notes them, never `LoadFromFirstRealRegion` | **DISPROVEN** as inbound |
| `006C2120` call it? | **No.** Callee of `CWorldMap::UpdateNavMaps` `00501990` (`E8` `00501C48`) | **DISPROVEN** as inbound |
| World vtbl `01244AEC` / `01244AB4`? | Nearby `004FB*` / `004FC*` / `00502*` slots; **no** `00501450` / `004FB150` / `004FC180` / `00501990` | **DISPROVEN** as vtbl method |
| Isolated `.rdata`/`.data` dword `n<4`? | No dword dump of rdata in-tree; `ScanVtbls` drops runs shorter than 4 | **UNREAD** |
| Host dummy `Pump` site? | **None.** `Pump` → `PumpGame` only. Tests call `LoadFromFirstRealRegion()` **after** dummy | **DISPROVEN** as native inbound; **MATCH** skip |

**Inbound mechanism: remaining UNKNOWN.**

Every indexed PE path that would *name* `00501450`
as a target is empty. Body is recovered; who
sets `ecx=CWorldMap` and enters `00501450` is
not in `E8` / `E9` / `ff` abs / vtbl-run /
switch / `.text` imm. Do **not** wire dummy
`Pump` to `LoadFromFirstRealRegion`.

---

## Direct answers

| Hunt | Hits | Class |
|---|---|---|
| `calls-by-dest.tsv` dest `0x00501450` | **0** | **PROVEN** |
| `e8.tsv` any `0x00501450` (site or dest) | **0** | **PROVEN** |
| listings `call 00501450` / `jmp 00501450` / bytes `50 14 50 00` | **0** (prologue only at `00501450`) | **PROVEN** |
| `ff.tsv` mem `[0x00501450]` | **0** | **PROVEN** |
| `ff.tsv` last col `0x00501450` | 21 **outbound** sites `005016B9`…`00502E6D` | **DISPROVEN** as inbound (containing-fn tag) |
| `00-index/vtbl.tsv` dest `0x00501450` | **0** | **PROVEN** |
| `01-sections/text-map/vtbl.tsv` | **file absent** | n/a |
| `switch.tsv` / `switch-ptrs.tsv` / `switch-index.tsv` | **0** | **PROVEN** |
| `abs.tsv` operand `0x00501450` (`\t0x00501450\t`) | **0** | **PROVEN** |
| `branches.tsv` dest `0x00501450` | **0** | **PROVEN** |
| `xrefs.tsv` `00501450` | **0** (string xrefs only; not a pointer index) | **PROVEN** empty as string xref |
| rdata listing | `.text` listing ends `0122CFFE`; `.rdata` VA `0122D000` has **no** listing | **UNREAD** isolated dwords |
| Fall-through from previous fn | `00501441 ret 32` then `int3` pad `00501444`…`0050144F` | **DISPROVEN** |

---

## `ff.tsv` is not a dest index

Header: `site	kind	mem	disp	fn`.

Last column is the **containing function**, not the
callee. `functions.tsv` start `0x00501450` size
**2248** swallows `00501990` UpdateNavMaps (real
end of enqueue is `00501985 ret` / `int3` /
`00501990`). FF rows tagged `0x00501450` are
calls **from** that blob (`[ebx+64]`, `[eax+4]`,
…), including `0050250D` inside **`00502500`**.
None of those rows have mem `[0x00501450]`.

Inbound FF would be an absolute `call [0x00501450]`
or a resolved slot whose rdata dword **is**
`00501450`. First: **0**. Second: vtbl dest **0**.

---

## Dummy getters are not the feeder

`listing-00400000.txt` first `004189C2`:

```
00418A48  call 004FB150              ; [ecx+156]
00418A52  call [eax+52]              ; world vtbl+52
00418A57  call 004FC180              ; [map+44]+index*88
00418A5C  mov ecx, [eax+36]
00418A61  je  00418A70               ; dummy +36 null
```

`listing-004c0000.txt`:

```
004FB150  mov eax, [ecx+156]
          ret
004FC180  mov eax, [esp+4]
          mov edx, [ecx+44]
          imul eax, eax, 88
          add eax, edx
          ret 4
```

Neither body `E8`s `00501450`. `calls-by-dest`
dest `0x004FB150` includes `0x00418A48` fn
`0x004189C2`; dest `0x004FC180` includes
`0x00418A57`. **Not** the reverse.

Host `PumpGame` dummy arm Notes `004FB150` /
`004FC180`, `ActivateCurrentRegion`, fade, one
inner. **No** `EnqueueAfterDummy`. **No**
`LoadFromFirstRealRegion`. `Pump(float)` Game
arm only `PumpGame` then maybe Present.

---

## `006C2120` is a callee, not a caller

`calls-by-dest` dest `0x006C2120`:

| Site | Tagged fn | Real parent |
|---|---|---|
| `00500D8A` / `005010BE` / `00501329` | `004FF900` | that fn |
| `00501C48` | `00501450` (swallow) | **`00501990` UpdateNavMaps** |

`listing-006c0000.txt` `006C2120`: enqueue onto
`[this+20]` / alloc `00BFEA0E`. String
`"CWorldMap::UpdateNavMaps - SetAsLoading"` at
`00501C16`. **After** `00501985 ret` / `int3`.

`00501450` loop `E8`s `00500540`; that later
`006C27A0` / `006C2120` is **downstream** of
the unread entry, not the entry.

---

## World vtbl — nearby, not this fn

`rtti.txt`: `CWorld` `0x01378AAC`, `CWorldMap`
`0x0137B720`.

`vtbl.tsv` `0x01244AEC` (CWorld-shaped: slot 3
`00507C30` Load `.wld`, slot 6 `004FF080`
topology, slot 30 `00502460`). Slots 0–30:
**no** `00501450`. Nearby `004FC130` /
`004FC150` are **not** `004FC180`.

`0x01244AB4` has `004FB4B0` / `004FB2B0` / …
**no** `004FB150`.

Grep dest `\t0x00501` in `vtbl.tsv`: **0**.
So `00501450` and `00501990` are **not**
virtual on any ≥4-pointer run.

Sibling `00502500` **does** have dest rows
(`004A4CB9` `004A3740`, `0089B99E` `00892D80`).
That is the **other** region path
(`[world+260]` first-seen skip). Contrast
proves the index would have listed
`00501450` if an `E8` existed.

---

## Isolated rdata dwords — UNREAD

`ScanVtbls` (`tools/Fable.ExeIndex/Program.cs`)
walks non-code READ sections and emits a vtbl
only when **n ≥ 4** consecutive code pointers.
A lone (or 2–3) `.rdata`/`.data` dword equal
to `00501450` would **not** appear in
`vtbl.tsv`.

Workspace has **no** rdata listing (`.text`
chunk `listing-01200000.txt` stops
`0122CFFE`; `.rdata` rva `0xE2D000` → VA
`0x0122D000`). No in-tree `datascan` of
`50145000`. That hole is **UNREAD**, not a
hit.

`.text` imm encoding `50 14 50 00`: **0** in
listings. `abs.tsv` operand `0x00501450`: **0**.

---

## Host — do not invent a Pump site

```
Pump  Game → PumpGame only
PumpGame dummy: 004FB150 / 004FC180 / fade / one inner
LoadFromFirstRealRegion: explicit API (00501450 body)
EnqueueAfterDummy: leftover glue; only calls the API;
  production Pump never calls it
```

`EngineLifecycleTests.Second_pump_004189C2_loops_inner_not_00501450`:
third `Pump` still `FirstRealRegionLoadDone==false`,
**no** Note `LoadFromFirstRealRegionFn`.

`Second_pump_00501450_is_004FEEC0_then_00500540_1_0_0`:
dummy `Pump`s, **then** explicit
`life.LoadFromFirstRealRegion()`. That is a
**stand-in**, not a recovered native `E8`.

---

## Not these

| Candidate | Class |
|---|---|
| Dummy `004189C2` / second inner / type-1 / Present | **DISPROVEN** (`dummy-pumps-before-region`) |
| `004FB150` / `004FC180` as vtbl or as `E8` of enqueue | **DISPROVEN** |
| `006C2120` inbound | **DISPROVEN** (callee) |
| `01244AEC` / `0122D06C` slot = `00501450` | **DISPROVEN** (`0122D06C` is CString-shaped; used **inside** the blob) |
| Persist `00487C20` | **DISPROVEN** as no-save (`E8` `00500540` at `00487C55`) |
| `00502500` / `004A3740` | **DISPROVEN** as this fn (other path; first-seen skip) |
| Host dummy `Pump` → `LoadFromFirstRealRegion` | **DISPROVEN**; leftover glue only |
| Fall-through / `int3` pad | **DISPROVEN** |

---

## Classifications (short)

1. **No indexed inbound to `00501450`. PROVEN absence** of `E8`/`E9`/`ff` abs / vtbl-run dest / switch dest / `.text` imm.
2. **`ff.tsv` last-col hits are outbound, not inbound. PROVEN.**
3. **Dummy getters + `006C2120` are not the caller. DISPROVEN.**
4. **Isolated rdata/data dwords shorter than a 4-slot run: UNREAD** (no dump).
5. **Who enters `00501450`: remaining UNKNOWN.** Do not invent host `Pump`.

---

## Open

| Item | Class |
|---|---|
| Who sets `ecx=CWorldMap` and enters `00501450` | **UNREAD** |
| Isolated `.rdata`/`.data` dword `n<4` | **UNREAD** (needs `datascan 50145000` on the PE) |
| Runtime-computed pointer / unread dispatcher | **UNREAD** |

## Files read

- `C:\FableCSharp\proofs\00501450-host-leftover\README.md`
- `C:\FableCSharp\proofs\00501450-no-00449D90\README.md`
- `C:\FableCSharp\proofs\first-region-after-leave\README.md`
- `C:\FableCSharp\proofs\host-00501450-timing\README.md`
- `C:\FableCSharp\proofs\dummy-pumps-before-region\README.md`
- `C:\FableCSharp\proofs\load-job\README.md`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\INDEX.md`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\ff.tsv` (header + `fn=0x00501450` rows)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\calls-by-dest.tsv` (dest `00501450` **0**; `004FB150` / `004FC180` / `006C2120` / `00502500`)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\e8.tsv` (dest `00501450` **0**; `004FB150` sites)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\abs.tsv` / `branches.tsv` / `crc.tsv` / `functions.tsv`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\switch.tsv` / `switch-ptrs.tsv` / `switch-index.tsv`
- `C:\FableCSharp\assembly\exe\00-index\vtbl.tsv` (`01244AEC` / `01244AB4` / dest greps)
- `C:\FableCSharp\assembly\exe\00-index\xrefs.tsv` / `xrefs-by-string.tsv` / `rtti.txt` / `sections.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00500000.txt` (`00501417`…`00501990`, `00502500`)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-004c0000.txt` (`004FB150`, `004FC180`)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-006c0000.txt` (`006C2120`)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00400000.txt` (`00418A48` dummy)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00480000.txt` (`004A4CB9` → `00502500`)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-01200000.txt` (end `0122CFFE`)
- `C:\FableCSharp\assembly\README.md`
- `C:\FableCSharp\tools\Fable.ExeIndex\Program.cs` (`ScanVtbls` n≥4)
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\world\INDEX.md` / `rtti.md`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\03-pseudo\world.md`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs` (`Pump` / `PumpGame` / `LoadFromFirstRealRegion` / `EnqueueAfterDummy`)
- `C:\FableCSharp\tests\Fable.Formats.Tests\EngineLifecycleTests.cs` (`Second_pump_*`)
