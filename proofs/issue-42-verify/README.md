# Issue #42 vs HEAD: dest `[01232C24+8]` still oversold

Investigation only. No `src/` or `tests/` edits.

Do **not** invent a `game.bin` / `00A38E50` /
`009B08C0` parse from this note.

Status words: **PROVEN** / **PARTIAL** / **UNREAD**
/ **DISPROVEN** / **LEFTOVER** / **DIVERGE** /
**MATCH**.

Issue: https://github.com/SamG-Coder/FableCSharp/issues/42
(`Open`, 2026-08-19). Title: `587baae` treats
`0044C72B` as `[01232C24+8]` without quoting the
rdata dword.

HEAD: `ee084901e8212814d4ca7df599180117f9be5cec`
(`Add CCreatureNavigationDef…`). Offending commit
still an ancestor:
`587baae04be2490a572aec33f556ff0b78c89816`
(`Prepare the definition manager via vtbl+8
0044C72B and 009ACB10`). Four later commits add
def classes only. None attach an rdata dump.

---

## Verdict vs HEAD

| Question | Answer | Class |
|---|---|---|
| Status vs HEAD? | **STILL OPEN.** Dest identity is still asserted as `0044C72B` without a quoted dword. | **STILL OPEN** |
| Later dword dump in-repo? | **No.** No `WriteVtblPart` / `WriteU32Part` / implementer `vtbl` of `01232C24`. `listing-01200000.txt` ends `0122CFFE`. `strings.tsv` / `xrefs.tsv` have **zero** `01232C24` / `01232C2C`. | **UNREAD** |
| Dest still oversold? | **Yes.** Freeze proof still **UNREAD**. Short host-prepare proof still **PROVEN**. Src comment + constant + test still lock `0x0044C72B`. | **DIVERGE** |

**Answer:** issue #42 is **STILL OPEN** at HEAD
`ee08490`. Host *shape* (Note getter + virtual +
`009ACB10` / `009E5250`; not `[0x13B8A54]`; not a
`game.bin` parse) is still the right leftover.
The oversell is dest identity.

---

## Direct answers (issue “Done looks like”)

| Done item | HEAD | Class |
|---|---|---|
| Quote rdata dword at `01232C24+8` before dest **PROVEN** | not present | **UNREAD**; dest still claimed **PROVEN** |
| Demote src / short proof / test to dest **PARTIAL** or **UNREAD** | not done | still **PROVEN** / tautology |
| Keep `0044C72B` as compile-neighbor candidate | freeze proof still does; host + src do **not** | **DIVERGE** |
| Do not implement `game.bin` / `00A38E50` / `009B08C0` as this virtual | host comment still “Not a game.bin parse”; Notes only | **MATCH** leftover body |
| Align `00416005-host-prepare` with `00416005-def-manager` | still disagree | **DIVERGE** |

Does **not** lock #14 / #20 / #36 (issue text).

---

## 1. Issue claim (quoted)

From #42 body:

> `587baae` … wires `DefinitionManagerVtbl8Fn =
> 0x0044C72B` into production comments, host
> Notes (`"0044C72B [vtbl+8]"`), a 22-line proof
> that marks dest **PROVEN**, and a test that
> tautology-checks the constant.
>
> The freeze sibling `proofs/00416005-def-manager`
> is unchanged … and still says:
> * `[edx+8]` identity is `0044C72B` — **UNREAD**
> * Slot `01232C24+8` bytes are **UNREAD**
>
> The new proof claims `rdata 01232C24+8=0044C72B`
> with no dword dump attached.

That package is still on `ee08490`.

---

## 2. Freeze proof still says dest **UNREAD**

`proofs/00416005-def-manager/README.md` (HEAD;
same text as issue SHA `af13a9ef`):

```
| `[edx+8]` identity is `0044C72B` | **UNREAD** (rdata vtbl `01232C24` not listed; `morph-first`) |
| `0044C72B` `"Game Definition Manager: Compile"` / `009B08C0` is `game.bin` analog | **PROVEN** as that fn; **PARTIAL** as this virtual |
```

Lines 231–233:

```
`0044C72B` has **0** `.text` `E8` (virtual
candidate). Slot `01232C24+8` bytes are **UNREAD**.
Do not treat `game.bin` open as proven on this
`[edx+8]` until rdata is read.
```

Classification table line 314:

```
| `[edx+8]` | first work | **PROVEN** call; dest **UNREAD** |
| `0044C72B` | compile neighbor | **PARTIAL** as slot |
```

Call site itself is still **PROVEN** from
`listing-00400000.txt`:

```
00416014  call 0044C6B0
00416019  mov edx, [eax]
0041601B  mov ecx, eax
0041601D  call [edx+8]
0041602E  call 009ACB10
```

Live vtbl write is still **PROVEN** from
`listing-00440000.txt` `0044C6F0  mov [esi], 0x1232C24`.
Next frame after store `0044C71F` is `0044C72B`
(`push ebp`). That neighbor fact is **not** a
slot+8 dword.

---

## 3. Short proof still claims dest **PROVEN** with no dword

`proofs/00416005-host-prepare/README.md` (22 lines;
added in `587baae`; **unchanged** on HEAD):

```
Authority: `Fable.exe`
`listing-00400000.txt` `00416005`–
`00416044`; rdata
`01232C24+8`=`0044C72B`;
```

```
| `[edx+8]` dest is `0044C72B` | **PROVEN** from exe |
| Host getter + vtbl+8 + reset | **MATCH** |
| Host opens / parses `game.bin` | **DISPROVEN** |
```

No file offset, no little-endian bytes, no
`[ 2] +  8  0x0044C72B` line. The `=` is the
claim, not a dump.

---

## 4. Src + test still lock the invention

`src/Fable.Game/EngineLifecycle.cs` (HEAD
`ee08490`; working tree still same constants):

```
public const uint PlayerManagerVtbl = 0x01232C24;
/// <c>[vtbl+8]=0044C72B</c>
/// Not a game.bin parse.
public const uint InitDefinitionManagerFn = 0x00416005;
public const uint DefinitionManagerVtbl8Fn = 0x0044C72B;
public const int DefinitionManagerVtbl8 = 8;
public const uint DefinitionManagerResetFn = 0x009ACB10;
public const uint DefinitionManagerResetApply = 0x009E5250;
public const int DefinitionManagerPlus88 = 88;
public const int DefinitionManagerArg = 1;
```

`PrepareDefinitionManager` Notes only:

```
Note(..., "0044C6B0 [0x13B879C]");
Note(DefinitionManagerVtbl8Fn, ...,
    $"0044C72B [vtbl+{DefinitionManagerVtbl8}]");
Note(..., $"009ACB10 [this+{DefinitionManagerPlus88}] arg={DefinitionManagerArg}");
Note(..., "009E5250 list reset");
DefinitionManagerPrepared = true;
```

No list object. No `009B08C0`. Comment on the
property: `After 00416005 0044C72B / 009ACB10`.
**Not a game.bin parse** is still true.

`tests/Fable.Formats.Tests/EngineLifecycleTests.cs`
`Init_Definition_Manager_00416005_resets_plus88_via_vtbl8`:

```
Assert.Equal(0x00416005u, EngineLifecycle.InitDefinitionManagerFn);
Assert.Equal(0x0044C72Bu, EngineLifecycle.DefinitionManagerVtbl8Fn);
Assert.Equal(8, EngineLifecycle.DefinitionManagerVtbl8);
Assert.Equal(0x009ACB10u, EngineLifecycle.DefinitionManagerResetFn);
Assert.Equal(0x009E5250u, EngineLifecycle.DefinitionManagerResetApply);
Assert.Equal(88, EngineLifecycle.DefinitionManagerPlus88);
Assert.Equal(1, EngineLifecycle.DefinitionManagerArg);
Assert.Equal(0x01232C24u, EngineLifecycle.PlayerManagerVtbl);
```

That does **not** read dword `[01232C24+8]`.
Name implies `+88` reset; host only Notes
`009ACB10` / `009E5250` and sets
`DefinitionManagerPrepared`.

---

## 5. No later dump on HEAD (or in ExeIndex)

Checked vs HEAD + current tree:

| Place | Hit |
|---|---|
| `listing-01200000.txt` last VA | `0122CFFE  add [eax], al` (`.text` end) |
| `.rdata` | rva `0xE2D000` → VA `0122D000` (`sections.txt`) |
| `01232C24` | `.rdata` + `0x5C24`; file `0xE32C24`; slot+8 VA `01232C2C` / file `0xE32C2C` |
| `01232C24` / `01232C2C` in listings | **zero** |
| `strings.tsv` | ASCII gap `0x01231FF0` `CSpecialEffectsDef` → `0x012331A8` `HasWeatherMask` (wchar / vtbl skipped) |
| `xrefs.tsv` dest `0x01232C24` | **zero** |
| `e8.tsv` dest `0044C72B` | **zero** (virtual-only candidate; freeze already used this) |
| `functions.tsv` `0x0044C72B` | 4297-insn compile registrar (**PROVEN** as that fn) |
| `WriteU32Part` / implementer `vtbl` of `01232C24` | **none** |

Working-tree extras **not** on HEAD
`ee08490` (GitHub 404):

- `proofs/00416005-vtbl8` — dest still
  **UNREAD**; PE `0xE32C2C` “not readable this
  pass.” Does **not** close #42.
- `proofs/0044C72B-compile` — writes
  `exe rdata 01232C2C=0044C72B` / “File
  `0xE32C2C` dword” and marks dest **PROVEN**.
  Still **no** quoted little-endian bytes / no
  persisted `vtbl` dump. Same oversell as
  host-prepare. Do **not** treat as a dump.
- `proofs/0044C72B-pc-glob` — cites “sibling
  rdata” as **PROVEN**.

Those files make dest **more** oversold if
committed. They do **not** satisfy Done item 1.

---

## 6. What is **not** the leftover

| Piece | Class |
|---|---|
| Named stage `"Init Definition Manager"` `00416005` | **MATCH** |
| Work this = `[0x13B879C]` via `0044C6B0` | **PROVEN** |
| `call [edx+8]` first work | **PROVEN** call |
| Live vtbl `01232C24` | **PROVEN** store |
| Dest VA at `01232C24+8` | **UNREAD** |
| `0044C72B` Compile neighbor / 0 `E8` | **PARTIAL** as this slot |
| Host Notes dest as `0044C72B` | dest **PARTIAL** (oversold as proven) |
| Host Compile body / `009B08C0` / `game.bin` | **DISPROVEN** as implemented; do **not** add |
| `+88` list reset body | Note-only **LEFTOVER** |
| `[0x13B8A54]` / `00A38500` / `00A39010` | **DISPROVEN** as this stage |

---

## Leftover

1. Persist a real dword at `01232C2C` (listing or
   `vtbl 0x01232C24 8` / `WriteU32Part`) **before**
   dest is **PROVEN**.
2. Until that dump exists: dest stays **UNREAD** /
   **PARTIAL**. Keep `0044C72B` as the
   compile-neighbor candidate only.
3. Align `proofs/00416005-host-prepare` with
   `proofs/00416005-def-manager`. Demote src
   comment `[vtbl+8]=0044C72B` and the test
   tautology so a swarm cannot copy dest as
   proven.
4. Do **not** implement `game.bin` / `009B08C0` /
   `00A38E50` from the short proof.

---

## Proposed next step

Run against a local `Fable.exe` and **commit the
output**:

```
dotnet run --project tools/Fable.ExeIndex -- vtbl 0x01232C24 8
```

or `WriteU32Part` of VA `0x01232C2C` (1 dword).
Quote the line `[ 2] +  8  0x........`.

- If the dword is `0x0044C72B`, dest flips to
  **PROVEN**. Then leftover is Compile **body**
  (path list / `009B0AC0` table / `009B08C0` /
  GetDefs), still **not** a host `game.bin`
  parser.
- Any other mapped `.text` VA **DISPROVES**
  `DefinitionManagerVtbl8Fn`.

Until that file exists in-repo, leave #42 open.

---

## Sources

- https://github.com/SamG-Coder/FableCSharp/issues/42
- https://github.com/SamG-Coder/FableCSharp/commit/587baae04be2490a572aec33f556ff0b78c89816
- https://github.com/SamG-Coder/FableCSharp/commit/ee084901e8212814d4ca7df599180117f9be5cec
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\tests\Fable.Formats.Tests\EngineLifecycleTests.cs`
- `C:\FableCSharp\proofs\00416005-def-manager\README.md`
- `C:\FableCSharp\proofs\00416005-host-prepare\README.md`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00440000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-01200000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\functions.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\sections.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\strings.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\xrefs.tsv`
