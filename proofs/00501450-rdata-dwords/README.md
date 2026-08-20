# Isolated `.rdata` / `.data` dwords equal to `00501450`

Investigation only. No production `src/` or `tests/` edits.

Do **not** start at Oakvale / `00DBDE40` / `CREATURE_HERO_CHILD`.
Do **not** treat `00501450` as Init Characters.
Do **not** invent a host `Pump` site. Dummy `Pump` /
`PumpGame` must **not** call `LoadFromFirstRealRegion`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: does any isolated `.rdata` / `.data` dword
(ScanVtbls drops `n<4`) equal `00501450`?
`proofs/00501450-inbound-ff` left that hole **UNREAD**
(no in-tree rdata listing; no `datascan 50145000`).

Authority: TLC `Fable.exe` via `GameInstall`
(`C:\Program Files (x86)\Steam\steamapps\common\Fable The Lost Chapters\Fable.exe`);
exe id `42D7DBDF-0106C000-16666624` (stamp `0x42D7DBDF`
SizeOfImage `0x0106C000` file `16666624`);
`assembly/exe/00-index/sections.txt`, `vtbl.tsv`;
`tools/Fable.ExeIndex` `ScanVtbls` (`n >= 4`) /
`datascan` / `imm` / `calls` / `X86.GetSwitchMap`;
`assembly/exe/01-sections/text-map/`
(`switch.tsv`, `switch-ptrs.tsv`, `switch-index.tsv`,
`functions.tsv`, `listing-00500000.txt`,
`listing-01200000.txt`);
siblings `proofs/00501450-inbound-ff`,
`proofs/00501450-host-leftover`,
`proofs/dummy-pumps-before-region`.

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| `.rdata` dword `50 14 50 00`? | **0** (file `0xE2D000` size `0x147000`, VA `0122D000`…`01373FFF`) | **PROVEN** absence |
| `.data` dword `50 14 50 00`? | **0** (file-backed `0xF74000` size `0x44000`, VA `01374000`…`013B7FFF`) | **PROVEN** absence |
| `.data` BSS tail? | Virtual size `0xCA9A4`; bytes past file size are zeros at load, not `00501450` | **PROVEN** absence |
| 1- / 2- / 3-pointer run dest `00501450`? | **None.** No dword to sit in a short run | **PROVEN** absence |
| vtbl (`n≥4`) dest `00501450`? | **0** (`vtbl.tsv`; restated) | **PROVEN** absence |
| Switch / jmp4 table dest `00501450`? | **0** (`switch*.tsv`; tables live in `.text`) | **PROVEN** absence |
| rdata singleton / computed jump-table slot? | **No stored VA.** `datascan` of the whole PE is **0** | **PROVEN** absence of the dword |
| Whole-PE `datascan 50145000 0 FFFFFFFF`? | **`scan  0`** (headers + `.text` + `.rdata` + `.data` + `.idata` + `_PDATA` + `.rsrc`) | **PROVEN** absence |
| `imm 00501450` / `calls 00501450`? | **0** / **0** | **PROVEN** (restated) |
| Function body present? | **Yes.** `00501450` `55 8B EC 83 E4 F8` at file `0x101450` | **PROVEN** (the dest exists; no inbound pointer) |

**Hit count: 0.** No VAs.

**Inbound mechanism: still UNKNOWN.**

The inbound-ff **UNREAD** hole (isolated rdata/data
dword `n<4`) is now **PROVEN** empty. Nothing on disk
names `00501450` as a pointer, switch dest, or
immediate. Who sets `ecx=CWorldMap` and enters the
body is not a stored dword. Do **not** invent that
site as `Pump` → `LoadFromFirstRealRegion`.

---

## PE window

Preferred base `0x00400000`. No `.reloc` section
(`sections.txt` is `.text` `.rdata` `.data` `.idata`
`_PDATA` `.rsrc` only). A relocatable code pointer
at this base would still be the raw LE dword
`50 14 50 00` on disk.

| Section | RVA | File | File size | VA (file-backed) | char |
|---|---|---|---|---|---|
| `.text` | `0x1000` | `0x1000` | `0xE2C000` | `00401000`…`0122CFFF` | `0x60000020` |
| `.rdata` | `0xE2D000` | `0xE2D000` | `0x147000` | `0122D000`…`01373FFF` | `0x40000040` READ |
| `.data` | `0xF74000` | `0xF74000` | `0x44000` | `01374000`…`013B7FFF` | `0xC0000040` READ+WRITE |
| `.idata` | `0x103F000` | `0xFB8000` | `0x4000` | `0143F000`… | `0xC0000040` |
| `_PDATA` | `0x1043000` | `0xFBC000` | `0x22000` | `01443000`… | `0x40000040` |
| `.rsrc` | `0x1065000` | `0xFDE000` | `0x7000` | `01465000`… | `0x40000040` |

`.data` virtual size `0xCA9A4` (`01374000`…`0143E9A4`)
out-runs the file. The unbacked tail is BSS zeros.

`.text` listing ends `0122CFFE` (`listing-01200000.txt`).
`.rdata` has **no** listing. That is why inbound-ff
could not grep rdata and left this **UNREAD**.

---

## Scan (what inbound-ff asked for)

Needle: little-endian VA `00501450` = bytes
`50 14 50 00` = datascan hex `50145000`.

`GameInstall.Locate()` → TLC `Fable.exe`.
`PeImage` identity matches `assembly/exe/manifest.json`.

```
dotnet run --project tools/Fable.ExeIndex -- datascan 50145000 0122D000 013B7FFF
exeId 42D7DBDF-0106C000-16666624
scan  0

dotnet run --project tools/Fable.ExeIndex -- datascan 50145000 0 FFFFFFFF
scan  0

dotnet run --project tools/Fable.ExeIndex -- imm 00501450 0 FFFFFFFF
imm  0

dotnet run --project tools/Fable.ExeIndex -- calls 00501450
calls  0
```

`datascan` walks every file offset in every section
(not 4-aligned only). `imm` is the same u32 test
with a containing-insn adjust in `.text`. Both **0**.

Independent whole-file walk of the same bytes
(PowerShell, every offset `0 … Length-4`): **0** hits
in headers and all six sections.

No hit → no file offset, no VA, no surrounding
16 bytes, no 1/2/3-pointer run, no vtbl slot.

The dest **function** is on disk. File `0x101450`
= VA `00501450`:

```
00501450  55                        push ebp
00501451  8B EC                     mov ebp, esp
00501453  83 E4 F8                  and esp, -8
00501456  83 EC 78                  sub esp, 120
```

Those bytes are `55 8B EC 83 …`, not `50 14 50 00`.
`functions.tsv` start `0x00501450` size **2248**
(swallow; real enqueue `ret` is `00501985`).

---

## Why `vtbl.tsv` could not answer this

`ScanVtbls` (`tools/Fable.ExeIndex/Program.cs`) walks
non-code READ sections (`.rdata` `.data` `.idata`
`_PDATA` `.rsrc`) at 4-aligned offsets and emits a
run only when **`n >= 4`** consecutive dwords are
code pointers (file maps into `.text`).

A lone, pair, or triple dword equal to `00501450`
would **not** appear in `vtbl.tsv`. That was the
inbound-ff hole.

This scan does not use the `n>=4` cut. It matches
the raw four bytes at every offset. Result is still
**0**, so the dropped short-run case is empty too.

`vtbl.tsv` dest `0x00501450`: **0** (grep). No
≥4-slot run hid the dest either.

---

## Jump table / rdata singleton

`X86.GetSwitchMap` finds MSVC `jmp [disp32+reg*4]`
(`FF 24 …`) and nearby `movzx` index tables in
`.text`. Comment: table sits after the function
`ret`. `switch.tsv` table column is all `0x004*`
(`.text`). Grep `0x012` in `switch.tsv`: **0**.
No switch pointer table lives in `.rdata`.

| Index | dest `0x00501450` |
|---|---|
| `switch.tsv` | **0** |
| `switch-ptrs.tsv` | **0** |
| `switch-index.tsv` | **0** |

A computed / hand-built rdata slot would still be
the dword `50 14 50 00`. Whole-PE `datascan` is
**0**, so there is **no** rdata singleton and **no**
unindexed jump-table entry.

Runtime-computed entry (`lea` / add / register
`call` with no stored dest) is outside this scan.
That remains **UNREAD**, not a hit.

---

## Hits

None.

| file offset | VA | 16-byte surround | run vs vtbl |
|---|---|---|---|
| — | — | — | no dword |

---

## Inbound after this scan

`00501450-inbound-ff` already **PROVEN**-emptied
`E8` / `E9` / `ff` abs / vtbl-run / switch /
`.text` imm. This note **PROVEN**-empties the last
indexed static store: isolated `.rdata`/`.data`
dwords and any other on-disk `50 14 50 00`.

Still not recovered:

| Item | Class |
|---|---|
| Who sets `ecx=CWorldMap` and enters `00501450` | **UNREAD** |
| Runtime-computed pointer / unread dispatcher | **UNREAD** |

Do **not** close that with host dummy `Pump`.
`Pump` → `PumpGame` only. `EnqueueAfterDummy` is
leftover glue. Tests that call
`LoadFromFirstRealRegion()` after dummy pumps are
a stand-in, not a recovered native site
(`dummy-pumps-before-region`,
`00501450-host-leftover`).

---

## Not these

| Candidate | Class |
|---|---|
| Isolated `.rdata`/`.data` dword `n<4` = `00501450` | **PROVEN** absence (was UNREAD) |
| rdata singleton / jmp-table dest | **PROVEN** absence |
| `vtbl.tsv` dest | **PROVEN** absence (restated) |
| Host `Pump` → `LoadFromFirstRealRegion` | **DISPROVEN** as native inbound (siblings) |
| Function missing / wrong exe | **DISPROVEN** (prologue + exeId match) |

---

## Classifications (short)

1. **`.rdata` / `.data` dword `00501450`: PROVEN absence** (aligned and unaligned; short-run and vtbl).
2. **Whole-PE `datascan 50145000`: PROVEN absence.** No jump-table slot, no rdata singleton, no `.text` imm encoding.
3. **Inbound mechanism: still UNKNOWN.** Static stores are empty. Do not invent `Pump`.

---

## Files read

- `C:\Program Files (x86)\Steam\steamapps\common\Fable The Lost Chapters\Fable.exe` (`datascan` / `imm` / `calls` / raw bytes at `0x101450`)
- `C:\FableCSharp\assembly\exe\manifest.json`
- `C:\FableCSharp\assembly\exe\00-index\sections.txt`
- `C:\FableCSharp\assembly\exe\00-index\vtbl.tsv` (dest `00501450` **0**)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\INDEX.md`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\switch.tsv` / `switch-ptrs.tsv` / `switch-index.tsv`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\functions.tsv` (`0x00501450`)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00500000.txt` (`00501450` prologue)
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-01200000.txt` (ends `0122CFFE`)
- `C:\FableCSharp\tools\Fable.ExeIndex\Program.cs` (`ScanVtbls` `n >= 4`, `RunScan` datascan)
- `C:\FableCSharp\tools\Fable.ExeIndex\X86.cs` (`GetSwitchMap` / `TrySwitchTableVa`)
- `C:\FableCSharp\tools\Fable.ExeIndex\PeImage.cs`
- `C:\FableCSharp\src\Fable.Core\GameInstall.cs` (path only)
- `C:\FableCSharp\proofs\00501450-inbound-ff\README.md`
- `C:\FableCSharp\proofs\00501450-host-leftover\README.md`
- `C:\FableCSharp\proofs\dummy-pumps-before-region\README.md`
