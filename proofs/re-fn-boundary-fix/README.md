# Function-boundary miss: thiscall vs `00430C80` `db 0x8E`

Investigation only. Production `src/Fable.Game` was not edited.
`tools/Fable.ExeIndex` was **not** patched here; this file is the
proposed patch.

Question: `00430C80` fn dump starts mid-instruction (`db 0x8E`).
Is `Fable.ExeIndex` function-boundary wrong? If yes, exact tool
bug and proposed fix.

Sibling: `proofs/re-fn-00430C80` (site identity). This note is
the **tool** half.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Sources:

- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
- `tools/Fable.ExeIndex/out/01-sections/text-map/functions.tsv`
- `tools/Fable.ExeIndex/out/01-sections/newgame-trace/fnmap.md`
- `implementer/stars/disasm-00430C80.txt`
- `implementer/stars/fn-00430900.txt`
- `implementer/frontend/fn-00430C80-exact.txt`
- `tools/Fable.ExeIndex/X86.cs` (`IsFramePrologue`, `FindPrologue`,
  `WalkFunction`, `WalkRange`, `TryDecode`)
- `tools/Fable.ExeIndex/FunctionMap.cs` (`WalkAllCode`,
  `ScanRangeStarts`)
- `tools/Fable.ExeIndex/Program.cs` (`RunMapText` `FlushFn`,
  `RunFn`)

---

## Verdict

**`00430C80` is not a function start.** Linear listing has
`lea ecx, [esi+328]` at `00430C7F` (`8D 8E 48 01 00 00`). The
dump that starts at `00430C80` is the ModRM of that lea.
`TryDecode` has no `0x8E`, so `WalkRange` emits `db 0x8E`.
**PROVEN.**

**New Game FunctionMap is not wrong about this VA.** `fnmap.md`
lists Transfer `0x00430900` (seed `range`, 539 insns) and does
not list `00430C80`. Range scan after two INT3s at
`004308FE`/`004308FF` already finds `00430900`. **PROVEN.**

**Text-map / `WalkAllCode` function-boundary is wrong.**
`IsFramePrologue` only matches `push ebp` frames. Thiscall
`push esi; mov esi, ecx` (`56 8B F1`) at `00430900` is not a
split. `functions.tsv` therefore has one row
`0x00430345` **980** insns, next row `0x00431020`. Transfer,
ctor `004304E0`, and size stub `004308F0` are swallowed.
**PROVEN.**

**Do not decode `0x8E` (`MOV Sreg`).** That would hide the
mid-instruction alarm. **DISPROVEN** as the fix.

---

## What `00430C80` is

`listing-00400000.txt`:

```
00430C7A  call 004310A7
00430C7F  lea ecx, [esi+328]     ; 8D 8E 48 01 00 00
00430C85  push ecx
00430C86  mov ecx, edi
00430C88  call 00431020
```

`fn --exact` / `disasm` from `00430C80`:

```
00430C80  db 0x8E
00430C81  dec eax
00430C82  add [eax], eax
00430C84  add [ecx-117], dl
00430C87  db 0xCF
00430C88  call 00431020          ; re-syncs by luck
```

`fn 00430C80` **without** `--exact` uses `FindPrologue`. Two
INT3s at `004308FE`/`004308FF` return **`00430900`**.
`--exact` is the only path that dumps `db 0x8E`.

Containing function:

```
004308EE  ret                    ; ENVIRONMENT ctor 004304E0
004308EF  int3
004308F0  mov eax, 0x260         ; size stub
004308F5  ret
004308F6  int3 … 004308FF int3
00430900  push esi               ; Transfer  56 8B F1
00430901  mov esi, ecx
00430903  push edi
…
00430C7F  lea ecx, [esi+328]
…
0043101B  pop edi
0043101C  pop esi
0043101D  ret 4
00431020  push ebp               ; next frame prologue  55 8B EC
00431021  mov ebp, esp
```

---

## Exact tool bug

Three consumers share one predicate.

```
X86.IsFramePrologue
  55 8B EC     push ebp; mov ebp, esp
  55 8D 6C 24  push ebp; lea ebp, [esp+disp8]
  55 8D AC 24  push ebp; lea ebp, [esp+disp32]
```

No `56 8B F1` / `53 56 8B F1`. `data[i] != 0x55` → false.

| Consumer | Uses | Effect at this site |
|---|---|---|
| `RunMapText` `FlushFn` | `IsFramePrologue` only | start `00430345`, no flush at `00430370` / `004304E0` / `00430900`; 980 insns until `00431020` |
| `FunctionMap.WalkAllCode` | `IsFramePrologue` only | same missing starts |
| `WalkFunction` stop | `IsFramePrologue` or INT3 | walk from `00430345` does not stop at Transfer |
| `ScanRangeStarts` | frame **or** two INT3s then `FindPrologue` | **does** seed `00430900` |
| `FindPrologue` | frame **or** two INT3s | `fn 00430C80` (no `--exact`) snaps to Transfer |
| `TryDecode` | no `case 0x8E` | `--exact` at ModRM → `db 0x8E` (correct alarm) |

New Game map is saved by the INT3 branch. Whole-`.text` map is not.

False-positive hazard: `56 8B F1` also appears **inside** bodies.
Do **not** OR it into `IsFramePrologue` unconditionally.
`WalkFunction` would then truncate at the first inner
`push esi; mov esi, ecx`. Guard: thiscall is a start only when
the previous byte is `0xCC` (MSVC INT3 pad), matching
`FindPrologue` / `ScanRangeStarts`.

---

## Proposed patch (do not apply in this investigation)

### 1. `X86.cs` — `IsThiscallPrologue` + `IsFunctionStart`

Keep `IsFramePrologue` as the `push ebp` forms. Add:

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

Call sites:

| Site | Change |
|---|---|
| `FindPrologue` first loop | `IsFunctionStart` instead of `IsFramePrologue` (INT3 path stays) |
| `WalkFunction` stop (`i >= 1`) | `IsFunctionStart` so a walk from `00430345` stops at `00430370` / `004304E0` / `00430900` |
| `FunctionMap.WalkAllCode` | `IsFunctionStart` |
| `FunctionMap.ScanRangeStarts` | `IsFunctionStart` (INT3 branch still covers size stubs like `004308F0`) |
| `Program.RunMapText` flush | `IsFunctionStart` |

Do **not** add `case 0x8E` to `TryDecode`.

### 2. Optional `FindInsnStart` for `disasm` / `--exact`

If `TryDecode` at `va` fails or the first step is `db`, try
`va-1 … va-15` and keep the offset whose decode **covers** `va`
and is not `db`. `00430C80` → `00430C7F` (`lea ecx, [esi+328]`).
Print `snapped 0x00430C7F from 0x00430C80`. Do **not** treat
the snap as a function entry. `--exact` may keep the old
behaviour if agents need a raw mid-VA dump.

### 3. `FunctionMap.cs` seeds / range (optional, New Game already works)

```
("ENVIRONMENT Transfer", 0x00430900)   // next to 004310A7 / 00431143
environment-persist: (0x004304E0, 0x00431200)  // include ctor
```

`fnmap.md` must still have one Transfer row at `0x00430900`,
never `0x00430C80`. +288 / +424 stay **sites**.

---

## After the patch

`functions.tsv` around ENVIRONMENT:

| va | why |
|---|---|
| `00430345` | frame ctor helper; ends `0043035D` (short, not 980) |
| `00430370` | thiscall after INT3 |
| `004304E0` | `53 56 8B F1` ctor |
| `004308F0` | size stub via INT3 scan (not thiscall) |
| `00430900` | Transfer |
| `00431020` | int persist (already present) |

`fn --exact 00430C80` still shows `db 0x8E` unless
`FindInsnStart` is wired. That is not a function-boundary miss.

---

## Claims

| Claim | Status |
|---|---|
| `00430C80` is a function | **DISPROVEN** |
| `db 0x8E` is ModRM of `lea` at `00430C7F` | **PROVEN** |
| Containing fn is `00430900` Transfer | **PROVEN** |
| New Game map lists `00430C80` | **DISPROVEN** |
| New Game map lists `00430900` | **PROVEN** |
| `IsFramePrologue` matches `00430900` | **DISPROVEN** |
| text-map splits Transfer | **DISPROVEN** (`00430345` 980 insns) |
| Fix = decode `0x8E` | **DISPROVEN** |
| Fix = `IsFunctionStart` (frame **or** INT3+thiscall) in flush / walk / scan | **PROVEN** as the intended change |
| Unguarded `56 8B F1` in `IsFramePrologue` | **DISPROVEN** (mid-body false split) |
