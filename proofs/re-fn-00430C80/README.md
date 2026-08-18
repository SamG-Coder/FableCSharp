# `00430C80` is mid-instruction in ENVIRONMENT Transfer

Investigation only. Production `src/` was not edited. `X86.cs` was not edited.

Question: does the fn decoder at `00430C80` start mid-instruction (`db 0x8E`)?
Is FunctionMap wrong? What should `X86.cs` / `FunctionMap.cs` change?

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Sources:

- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
- `tools/Fable.ExeIndex/out/01-sections/newgame-trace/fnmap.md`
- `implementer/stars/disasm-00430C80.txt`
- `implementer/stars/fn-00430900.txt`
- `implementer/stars/fn-00430BF0-exact.txt`
- `tools/Fable.ExeIndex/X86.cs` (`TryDecode`, `FindPrologue`, `IsFramePrologue`)
- `tools/Fable.ExeIndex/FunctionMap.cs` (`ScanRangeStarts`, `NewGameRanges`)

---

## Verdict

**`00430C80` is not a function.** Linear decode from that VA is the
ModRM of `lea ecx, [esi+328]` at `00430C7F`. Opcode `0x8E` is not in
`X86.TryDecode`, so the first line is `db 0x8E`. **PROVEN.**

The containing function is ENVIRONMENT Transfer **`00430900`**
(`push esi; mov esi, ecx`), `ret 4` at `0043101D`. **PROVEN.**

New Game FunctionMap already lists `0x00430900` (seed `range`, 539
insns) and does **not** list `00430C80`. **FunctionMap is not wrong
about this VA.** **PROVEN.**

Do **not** add `0x8E` (`MOV Sreg, r/m16`) to “fix” this dump. That
would decode garbage and hide the mid-instruction. **DISPROVEN** as
the fix.

---

## What `00430C80` actually is

`listing-00400000.txt` (linear `.text` walk from section start):

```
00430C7A  call 004310A7
00430C7F  lea ecx, [esi+328]     ; 8D 8E 48 01 00 00
00430C85  push ecx
00430C86  mov ecx, edi
00430C88  call 00431020
```

Bytes at `00430C80`: `8E 48 01 00 00 51 8B CF …`

`disasm` / `fn --exact` from `00430C80` (`implementer/stars/disasm-00430C80.txt`):

```
00430C80  db 0x8E
00430C81  dec eax            ; 48
00430C82  add [eax], eax     ; 01 00
00430C84  add [ecx-117], dl  ; 00 51 8B
00430C87  db 0xCF
00430C88  call 00431020      ; re-syncs by luck
```

Same body, aligned, from Transfer start (`fn-00430900.txt` continues
into this lea/+328 persist). **PROVEN.**

`00430C80` is 16-byte aligned. That is not a prologue.

`X86.TryDecode` has `0x8D` (lea) and `0x8F` (pop r/m), **no** `0x8E`.
Failed decode emits `db` and advances one byte (`WalkRange`). **PROVEN.**

---

## Real function

```
004308EE  ret                  ; ENVIRONMENT ctor 004304E0
004308EF  int3
004308F0  mov eax, 0x260       ; size stub (608)
004308F5  ret
004308F6  int3 … 004308FF int3
00430900  push esi             ; Transfer
00430901  mov esi, ecx
00430903  push edi
00430904  mov edi, [esp+12]    ; persist stream
… lea / push / mov ecx,edi / call persist helpers …
00430C7F  lea ecx, [esi+328]
0043100D  add esi, 0x25C
00431016  call 00431061
0043101B  pop edi
0043101C  pop esi
0043101D  ret 4
00431020  push ebp             ; first frame-prologue helper
00431021  mov ebp, esp
```

Helpers used from Transfer (already seeded / range-scanned):

| VA | Role |
|---|---|
| `004310A7` | CString persist |
| `00431020` | int persist |
| `00431061` | float persist |
| `00431143` | NString persist |
| `00431242` | vector persist |

`fn 00430C80` **without** `--exact` uses `FindPrologue`. Walking back
hits `CC CC` at `004308FE`/`004308FF` and returns **`00430900`**.
**PROVEN** from `FindPrologue` + listing padding.

Program sites `00430BF3` (+288) and `00430DC1` (+424) are **instruction**
starts inside this function, not entries. `00430BF0` / `00430DC0`
(`--exact`) are mid-instruction, same class as `00430C80`.

---

## Is FunctionMap wrong?

| Path | `00430C80` | `00430900` |
|---|---|---|
| `FunctionMap.WalkNewGame` / `fnmap.md` | absent | present, seed `range`, 539 insns, calls `004310A7`… |
| `ScanRangeStarts` INT3 after `004308F6` | no | yes (`56 8B F1`) |
| `IsFramePrologue` | no | **no** (`push esi`, not `push ebp`) |
| text-map `functions.tsv` | absent | **absent** — swallowed into `00430345` (980 insns until `00431020`) |

So:

- **New Game fnmap is correct** for this VA: Transfer is `00430900`.
- **`IsFramePrologue` is incomplete**: thiscall `56 8B F1` /
  `53 56 8B F1` never start a function in `WalkAllCode` or the
  text-map function list. Those lists merge Transfer into the
  previous `push ebp` function `00430345`. **PROVEN.**
- Range `environment-persist` is `0x00430800`–`0x00431200`, so ctor
  `004304E0` is **outside** the window (`InNewGameRange` drops
  `FindPrologue` snaps to it). **PROVEN.** Size stub `004308F0` is
  also unlisted.

No NewGame seed names ENVIRONMENT Transfer; discovery is INT3 range
scan only.

---

## Proposed `X86.cs` fix (do not apply here)

**Do not** decode `0x8E` to paper over this VA.

1. **`IsFunctionStart` / extend `IsFramePrologue`** (keep the frame
   forms; add thiscall):

   ```
   // push ebp; mov ebp, esp          55 8B EC
   // push ebp; lea ebp, [esp+disp8]  55 8D 6C 24
   // push ebp; lea ebp, [esp+disp32] 55 8D AC 24
   // thiscall: push esi; mov esi, ecx           56 8B F1
   // thiscall: push ebx; push esi; mov esi, ecx 53 56 8B F1
   ```

   Use the new predicate in `FindPrologue`, `WalkFunction` (stop at
   the **next** start, including thiscall), `ScanRangeStarts`, and
   text-map function flush (`Program.cs` `IsFramePrologue`).

2. **Optional `FindInsnStart(pe, va)`**: if `TryDecode` at `va` fails
   or the first step is `db`, try `va-1 … va-15` and keep the offset
   whose decode covers `va` and does not emit `db`. That would snap
   `00430C80` → `00430C7F` even for `disasm` / `--exact`. Do not treat
   that snap as a function entry.

3. Leave `0x8E` unimplemented unless a real `MOV Sreg` site is proven.
   `db 0x8E` is the mid-instruction alarm.

---

## Proposed `FunctionMap.cs` fix (do not apply here)

1. Seed `("ENVIRONMENT Transfer", 0x00430900)` next to the persist
   helpers. Keep +288/+424 as **sites**, not functions.

2. Extend `environment-persist` to cover the ctor, e.g.
   `(0x004304E0, 0x00431200, "environment-persist")` (or `004303F0` if
   the scalar dtor/size stubs should be in-range).

3. `ScanRangeStarts`: add thiscall starts (`56 8B F1` / `53 56 8B F1`),
   not only `IsFramePrologue` + two INT3s. Same change feeds
   `WalkAllCode` if that should match New Game.

`fnmap.md` after the seed change should still show one Transfer row
at `0x00430900`, not `0x00430C80`.

---

## Claims

| Claim | Status |
|---|---|
| `00430C80` is a function / persist entry | **DISPROVEN** |
| First byte `0x8E` is opcode `MOV Sreg` | **DISPROVEN** (ModRM of lea) |
| Containing fn is `00430900` Transfer | **PROVEN** |
| New Game FunctionMap lists `00430C80` | **DISPROVEN** |
| New Game FunctionMap lists `00430900` | **PROVEN** |
| `IsFramePrologue` sees `00430900` | **DISPROVEN** |
| text-map `functions.tsv` splits Transfer | **DISPROVEN** (merged into `00430345`) |
| Fix = decode `0x8E` | **DISPROVEN** |
| Fix = thiscall prologue + seed `00430900` | **PROVEN** as the intended change |
