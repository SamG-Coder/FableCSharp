# ExeIndex function boundary at `00430C80` (`db 0x8E`)

Investigation only. `src/Fable.Game` was not edited. `tools/Fable.ExeIndex`
was not patched here.

Question: is `Fable.ExeIndex` function-boundary **wrong** at `00430C80`
because the dump starts `db 0x8E`? If yes, exact tool bug and
file/function to patch.

Authority: `tools/Fable.ExeIndex`, `proofs/re-fn-00430C80`.
Sibling tool note: `proofs/re-fn-boundary-fix`.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Sources:

- `tools/Fable.ExeIndex/X86.cs` (`IsFramePrologue`, `FindPrologue`,
  `WalkFunction`, `WalkRange`, `TryDecode`)
- `tools/Fable.ExeIndex/FunctionMap.cs` (`WalkAllCode`,
  `ScanRangeStarts`, `WalkNewGame`, `NewGameRanges`, `NewGameSeeds`)
- `tools/Fable.ExeIndex/Program.cs` (`RunFn`, `RunMapText` `FlushFn`)
- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
- `tools/Fable.ExeIndex/out/01-sections/text-map/functions.tsv`
- `tools/Fable.ExeIndex/out/01-sections/newgame-trace/fnmap.md`
- `implementer/stars/disasm-00430C80.txt`
- `implementer/frontend/fn-00430C80-exact.txt`
- `implementer/stars/fn-00430900.txt`
- `proofs/re-fn-00430C80/README.md`

---

## Verdict

**`00430C80` is not a function start.** Linear `.text` has
`lea ecx, [esi+328]` at `00430C7F` (`8D 8E 48 01 00 00`). Byte
`0x8E` at `00430C80` is that lea's ModRM, not `MOV Sreg`.
`TryDecode` has `0x8D` / `0x8F` and no `0x8E`, so `WalkRange`
emits `db 0x8E` and advances one byte. **PROVEN.**

**`fn --exact` / `disasm` at `00430C80` is not a function-boundary
miss.** `--exact` skips `FindPrologue` and walks from the given VA.
That is the only path that prints `db 0x8E`. **PROVEN.**

**New Game FunctionMap is not wrong about this VA.** `fnmap.md`
lists Transfer `0x00430900` (seed `range`, 539 insns) and does
**not** list `00430C80`. Two INT3s at `004308FE`/`004308FF` plus
`FindPrologue` already seed `00430900`. **PROVEN.**

**Whole-`.text` function-boundary *is* wrong, one function earlier.**
`IsFramePrologue` only matches `push ebp` frames. Transfer
`00430900` is thiscall `56 8B F1`. `functions.tsv` therefore has
one row `0x00430345` **980** insns, next row `0x00431020`.
Ctor `004304E0`, size stub `004308F0`, and Transfer are swallowed.
**PROVEN.**

**Do not decode `0x8E`.** That would hide the mid-instruction
alarm. **DISPROVEN** as the fix.

---

## What `00430C80` is

`listing-00400000.txt` (linear walk from section start):

```
00430C7A  call 004310A7
00430C7F  lea ecx, [esi+328]     ; 8D 8E 48 01 00 00
00430C85  push ecx
00430C86  mov ecx, edi
00430C88  call 00431020
```

`disasm` / `fn --exact` from `00430C80`:

```
00430C80  db 0x8E
00430C81  dec eax            ; 48
00430C82  add [eax], eax     ; 01 00
00430C84  add [ecx-117], dl  ; 00 51 8B
00430C87  db 0xCF
00430C88  call 00431020      ; re-syncs by luck
```

Containing function (ENVIRONMENT Transfer):

```
004308EE  ret                    ; ctor 004304E0
004308EF  int3
004308F0  mov eax, 0x260         ; size stub
004308F5  ret
004308F6  int3 … 004308FF int3
00430900  push esi               ; Transfer  56 8B F1
00430901  mov esi, ecx
00430903  push edi
00430904  mov edi, [esp+12]
…
00430C7F  lea ecx, [esi+328]
…
0043101B  pop edi
0043101C  pop esi
0043101D  ret 4
00431020  push ebp               ; next frame  55 8B EC
```

`fn 00430C80` **without** `--exact` uses `FindPrologue`. Walking
back hits `CC CC` at `004308FE`/`004308FF` and returns
**`00430900`**. **PROVEN.**

Sites `00430BF3` (`+288`) and `00430DC1` (`+424`) are instruction
starts inside Transfer, not entries. `fn --exact 00430BF0` is the
same mid-instruction class.

---

## Exact tool bug

One predicate is shared. `X86.IsFramePrologue` (`X86.cs`):

```
55 8B EC     push ebp; mov ebp, esp
55 8D 6C 24  push ebp; lea ebp, [esp+disp8]
55 8D AC 24  push ebp; lea ebp, [esp+disp32]
```

`data[i] != 0x55` → false. No `56 8B F1` / `53 56 8B F1`.

| Consumer | Predicate | This site |
|---|---|---|
| `Program.RunFn` (`--exact`) | none; start = given VA | `db 0x8E` |
| `Program.RunFn` (default) | `FindPrologue` | snaps to `00430900` |
| `X86.WalkFunction` stop | `IsFramePrologue` or INT3 | walk from `00430345` does not stop at Transfer |
| `Program.RunMapText` `FlushFn` | `IsFramePrologue` only | start `00430345`, 980 insns until `00431020` |
| `FunctionMap.WalkAllCode` | `IsFramePrologue` only | same missing starts |
| `FunctionMap.ScanRangeStarts` | frame **or** two INT3s then `FindPrologue` | **does** seed `00430900` |
| `X86.TryDecode` | `0x8D` lea, `0x8F` pop; **no** `0x8E` | `--exact` at ModRM → `db 0x8E` (correct alarm) |

New Game is saved by the INT3 branch in `ScanRangeStarts` plus
range `environment-persist` `(0x00430800, 0x00431200)`. There is
no seed named ENVIRONMENT Transfer; `00430900` is `range` only.
Ctor `004304E0` is **outside** that window.

`TryDecode` failing at `0x8E` is **not** the boundary bug. The
boundary bug is `IsFramePrologue` used as “function start” while
MSVC thiscall entries after INT3 pad are invisible to
`WalkAllCode` / `map-text`.

False-positive hazard: `56 8B F1` also appears **inside** bodies.
Do **not** OR it into `IsFramePrologue` unconditionally.
`WalkFunction` would truncate at the first inner
`push esi; mov esi, ecx`. Guard: thiscall is a start only when
the previous byte is `0xCC`, matching `FindPrologue` /
`ScanRangeStarts`.

---

## File / function to patch

Do **not** edit `src/Fable.Game`. Do **not** add `case 0x8E` to
`TryDecode`.

### 1. `tools/Fable.ExeIndex/X86.cs` — primary

Keep `IsFramePrologue` as the `push ebp` forms. Add
`IsThiscallPrologue` + `IsFunctionStart`:

```csharp
public static bool IsThiscallPrologue(byte[] data, int i)
{
    if (i + 2 >= data.Length)
        return false;
    // push esi; mov esi, ecx
    if (data[i] == 0x56 && data[i + 1] == 0x8B && data[i + 2] == 0xF1)
        return true;
    // push ebx; push esi; mov esi, ecx
    return i + 3 < data.Length
        && data[i] == 0x53 && data[i + 1] == 0x56
        && data[i + 2] == 0x8B && data[i + 3] == 0xF1;
}

public static bool IsFunctionStart(byte[] data, int i)
{
    if (IsFramePrologue(data, i))
        return true;
    // INT3 pad only — do not split on a mid-body 56 8B F1.
    if (i > 0 && data[i - 1] == 0xCC && IsThiscallPrologue(data, i))
        return true;
    return false;
}
```

Switch these to `IsFunctionStart`:

| Site | Why |
|---|---|
| `X86.FindPrologue` first loop | thiscall after INT3 is an entry |
| `X86.WalkFunction` stop (`i >= 1`) | walk from `00430345` must stop at `00430370` / `004304E0` / `00430900` |

### 2. Callers that still test `IsFramePrologue` only

| Site | Change |
|---|---|
| `FunctionMap.WalkAllCode` | `IsFunctionStart` |
| `FunctionMap.ScanRangeStarts` | `IsFunctionStart` (INT3 branch still covers size stub `004308F0`) |
| `Program.RunMapText` flush | `IsFunctionStart` |

### 3. Optional, not required to kill `db 0x8E`

`FindInsnStart(pe, va)`: if `TryDecode` at `va` fails or the first
step is `db`, try `va-1 … va-15` and keep the offset whose decode
**covers** `va` and is not `db`. That snaps `00430C80` →
`00430C7F`. Print `snapped 0x00430C7F from 0x00430C80`. Do **not**
treat the snap as a function entry. `--exact` may keep the raw
mid-VA dump.

Optional New Game seeds in `FunctionMap.cs`:
`("ENVIRONMENT Transfer", 0x00430900)` and widen
`environment-persist` to include ctor `004304E0`. `fnmap.md` must
still have one Transfer row at `0x00430900`, never `0x00430C80`.

---

## After the patch

`functions.tsv` around ENVIRONMENT:

| va | why |
|---|---|
| `00430345` | frame helper; ends `0043035D` (short, not 980) |
| `00430370` | thiscall after INT3 (`56 8B F1`) |
| `004304E0` | ctor (`53 56 8B F1`) |
| `004308F0` | size stub via INT3 scan (not thiscall) |
| `00430900` | Transfer |
| `00431020` | int persist (already present) |

`fn --exact 00430C80` still shows `db 0x8E` unless `FindInsnStart`
is wired. That remaining line is a **start-VA** issue, not a
function-boundary miss.

---

## Claims

| Claim | Status |
|---|---|
| `00430C80` is a function start | **DISPROVEN** |
| `db 0x8E` is ModRM of `lea` at `00430C7F` | **PROVEN** |
| Containing fn is `00430900` Transfer | **PROVEN** |
| New Game map lists `00430C80` | **DISPROVEN** |
| New Game map lists `00430900` | **PROVEN** |
| `IsFramePrologue` matches `00430900` | **DISPROVEN** |
| text-map splits Transfer | **DISPROVEN** (`00430345` 980 insns) |
| `fn --exact` `db 0x8E` means boundary is wrong **at** `00430C80` | **DISPROVEN** |
| Fix = decode `0x8E` | **DISPROVEN** |
| Fix = `IsFunctionStart` (frame **or** INT3+thiscall) in `X86.cs` + flush / walk / scan | **PROVEN** as the intended change |
| Unguarded `56 8B F1` in `IsFramePrologue` | **DISPROVEN** (mid-body false split) |
