# `0044C72B` first glob `pc\` + `*.h` on Init Definition Manager

Investigation only. No production `src/` or `tests/`
edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave this
walk is `FinalAlbion.wld` → `"Init Game"` →
`00418DCA` → vtbl+4 `004184BD` →
`"Init Definition Manager"` `004185D9`
`call 00416005` → `[vtbl+8]` `0044C72B`.
Do **not** invent a `*.h` / `game.bin` /
`00A38E50` parser.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: `0044C72B` first wchar are `pc\` +
`*.h`. What directory does it walk first-seen on
Init Definition Manager? Which files if any?
Host leftover after `PrepareDefinitionManager`
notes only the VA?

Authority: Fable.exe dump
`listing-00440000.txt` `0044C72B`–
`0044C8B0` / `0044E90C`–`0044EA4D`;
`listing-00400000.txt` `00416005`–
`00416044`, `004025D5`–`004025DF`,
`00402D44`–`00402D4B`, `0041A080`,
`004128A0`, `00414236` `UseCompiledDefs`,
`0041440D` `AllowDataGeneration`;
`listing-00980000.txt` `0099B6B0` /
`009A76A0` / `0099BE70` / `0099BF30` /
`00999760` / `00999C50` / `009B05F0` /
`009B08C0` / `009AEF60`;
`listing-01200000.txt` `01224030`;
`strings.tsv` (ASCII; wchar **absent**);
TLC `GameInstall` `data\` existence only;
`userst.ini`;
`src/Fable.Game/EngineLifecycle.cs`
(`PrepareDefinitionManager`);
siblings `proofs/00416005-host-prepare`,
`proofs/0044C72B-compile`,
`proofs/0041A0A0-data-misc`,
`proofs/00A01A4F-sound-symbols`,
`proofs/ini-activate-quest`.

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| First wchar immediates? | `0x122DAA4` UTF-16 **`pc\`**, then `0x1236094` glob (sibling / host-prepare **`*.h`**). | **PROVEN** prefix; glob **PARTIAL** |
| First directory of that pair? | **`pc\`** relative to process cwd (TLC install root). Joined pattern `pc\*.h`. Not `data\pc`, not `Data\Defs\`, not `RetailHeaders\pc`. | **PROVEN** |
| First `004128A0` slot? | **Not** `pc\`. Empty BSS `0x13CA7D4` + glob → cwd `*.h`, then `pc\*.h`, then `Data\Defs\` + glob. | **PROVEN** order |
| Which files if any? | TLC `{Root}\pc` **absent**. `{DataRoot}\pc` **absent**. `{Root}\*.h` **zero**. First pair hits **no files**. | **PROVEN** empty |
| Does Init Definition Manager FindFirst that pair? | Path list is **built**. Retail `AllowDataGeneration FALSE` skips `009B05F0`. `UseCompiledDefs TRUE` then `009B08C0` compiled open (sibling `game.bin` analog). `009AEF60` `"CreateSymbolsFromPathList"` is the FALSE / miss arm. | **PROVEN** list; first-seen FS walk **DISPROVEN** on TLC |
| Host leftover after VA Note? | **Yes.** `PrepareDefinitionManager` Notes `0044C72B [vtbl+8]` only. No `pc\`, no glob, no `004128A0`, no `009B08C0`. | **PROVEN** **LEFTOVER** body |

**Answer:** first `pc\`+`*.h` directory is
install-root **`pc\`**. TLC has **no** such
folder and **no** matching files. Host leftover
after the dest VA Note **is** that glob list
(and Compile `009B08C0`), not a missing VA.
Do **not** parse `*.h`. Do **not** start
Oakvale.

---

## Direct answers

| Claim | Class |
|---|---|
| `0041601D` `[edx+8]` is `0044C72B` | **PROVEN** (sibling rdata) |
| First work of `0044C72B` is path-list build | **PROVEN** |
| `004128A0` is FindFirst / dir walk | **DISPROVEN** — CString vector push |
| `0x122DAA4` = UTF-16 `pc\` | **PROVEN** |
| `0x1236094` = UTF-16 `*.h` | **PARTIAL** (wchar; ASCII `strings.tsv` skip) |
| `0x13CA7D4` empty at this site | **PROVEN** (static `0099AED0`; no later listing store) |
| First slot = cwd `*.h`; second = `pc\*.h` | **PROVEN** |
| TLC `{Root}\pc` / `data\pc` exist | **DISPROVEN** |
| TLC first-pair files | **PROVEN** none |
| `data\Defs\*.h` is this first pair | **DISPROVEN** (later `0041A080` slot) |
| `data\Defs\RetailHeaders\pc\*.h` is this pair | **DISPROVEN** |
| Retail `009B05F0` glob (`[0x138E189]`) | **DISPROVEN** fire (`AllowDataGeneration` FALSE) |
| Retail `009AEF60` first-seen | **DISPROVEN** fire if compiled open hits (`UseCompiledDefs` TRUE; `game.bin` exists) |
| Host Notes dest VA | **MATCH** |
| Host glob / files / `009B08C0` | **LEFTOVER** |
| Host `game.bin` parse here | **DISPROVEN** (comment “Not a game.bin parse”) |
| Oakvale / `00DBDE40` | **DISPROVEN** |

---

## 1. First wchar and first two list slots

`listing-00440000.txt` `0044C72B`:

```
0044C738  push 0x122DAA4
          lea ecx, [ebp-8]
          call 0099B6B0          ; intern pc\ → [ebp-8]
0044C757  mov edi, 0x1236094
0044C75C  push edi
          lea ecx, [ebp-12]
          call 009A76A0          ; copy [0x13CA7D4] → [ebp-12]
          mov edx, eax
          lea ecx, [ebp-4]
          call 0099BF30          ; dest = that copy + stack glob
          lea ecx, [ebp-32]
          call 004128A0          ; slot 0
0044C788  push edi               ; glob again
          lea eax, [ebp-8]       ; pc\
          push eax
          lea ecx, [ebp-16]
          call 009A76A0
          call 0099BE70          ; join pc\
          call 0099BF30          ; + glob
          lea ecx, [ebp-32]
          call 004128A0          ; slot 1  ← this pair
0044C7CA  push edi
          lea ecx, [ebp-12]
          call 0041A080          ; Data\Defs\
          call 0099BF30
          lea ecx, [ebp-32]
          call 004128A0          ; slot 2  (not this question)
```

`009A76A0` is `0099B720` from BSS
`0x13CA7D4` (`ret`, does **not** consume the
glob push). `0099BF30` is `ret 4` and feeds
that push to `0099B940`. `01224030` is
`0099AED0` empty ctor of `0x13CA7D4`. No
other listing store. Slot 0 is therefore
**empty prefix + glob**. Slot 1 is the first
use of interned `pc\`.

`004128A0` (`listing-00400000.txt`):
`0099B720` or grow `00412330`, `add [esi+4], 4`,
`ret 4`. Vector push, **not** a directory
walk.

`0041A080` is `push 0x122F3D0` /
`0099B6B0` / `ret` — UTF-16 `Data\Defs\`
(sibling `0041A0A0-data-misc`). Later slots
and `[ebp-44]` use that prefix plus
`0x1236088` / `0x123607C` (**UNREAD** globs).
Do **not** treat those as the first pair.

`0044E916` `00433DF0` copies `[ebp-32]` →
`[this+64]`, `[ebp-44]` → `[this+76]`, then
`"Game Definition Manager: Compile"` /
`009B08C0`.

---

## 2. Who would walk `pc\` — and TLC files

`009B08C0` (`listing-00980000.txt`):

```
009B08D0  lea edi, [esi+64]
          push edi
          call 009B05F0          ; gated [0x138E189]
          mov al, [0x13CA7D8]
          test al, al
          je 009B09BB            ; else compiled 00994700 / 009AFB90
009B09C2  push 1
          push edi
          call 009AEF60          ; "DefinitionManager : CreateSymbolsFromPathList"
```

`009B05F0` / `009AEF60` iterate `[list]` →
`[list+4]`. First live slot first.
`00999C50` → `00999760` → `00BFED7C`
(`jmp [0x1440094]`, `cmp eax, -1`). That is
the FindFirst analog. `00999CB0` is next.

Bootstrap copies (`00402510`, before
frontend / Init Game):

| Flag | Ini name | Slot | Dest used here |
|---|---|---|---|
| `0x1375459` | `AllowDataGeneration` | `0041443D` | `004025DF` → `[0x138E189]` |
| `0x13B8617` | `UseCompiledDefs` | `00414266` | `00402D4B` → `[0x13CA7D8]` |

TLC `userst.ini` (Parse Command Line
`00414C66`, sibling `ini-activate-quest` /
`00A01A4F-sound-symbols`):

```
UseCompiledDefs TRUE;
AllowDataGeneration FALSE;
```

`user.ini` has **no** override. So first-seen
`009B05F0` **`je 009B08A8`** (no glob).
Compiled arm of `009B08C0` is the live
path. `data\CompiledDefs\game.bin` **exists**
(existence only). `009AEF60` is **not** the
taken arm when that open hits. Do **not**
parse `game.bin` here.

If the miss arm ran, slot 1 would FindFirst
`pc\*.h` under cwd = TLC install root.

GameInstall `data\` inventory (existence
only; not a parser):

| Path | `*.h` |
|---|---|
| `{Root}\pc` | **absent** |
| `{DataRoot}\pc` (`data\pc`) | **absent** |
| `{Root}\*.h` | **zero** |
| `data\Defs\*.h` (non-recursive) | 4 files — **later** `Data\Defs\` slot |
| `data\Defs\RetailHeaders\pc\*.h` | 5 files — **not** pattern `pc\*.h` |

`pc` folders that **do** exist
(`graphics\pc`, `Misc\pc`, `shaders\pc`)
hold `*.big`, not `*.h`.

First-pair files on TLC: **none**.

---

## 3. Host leftover after the VA Note

`PrepareDefinitionManager`:

```
Note(0044C6B0, … "[0x13B879C]");
Note(0044C72B, … "0044C72B [vtbl+8]");   ← dest VA only
Note(009ACB10, … "[this+88] arg=1");
Note(009E5250, … "list reset");
```

Comment: “Not a game.bin parse.”
`DefinitionManagerPrepared = true`.

| Piece | Host | Class |
|---|---|---|
| Dest `0044C72B` | Note | **MATCH** |
| `pc\` / `*.h` / `004128A0` list | absent | **LEFTOVER** |
| FindFirst of `pc\*.h` | absent | **MATCH** TLC (no fire) |
| `009B08C0` / GetDefs | absent | **LEFTOVER** |
| `*.h` parser / `game.bin` parse | absent | **MATCH** (must stay so) |

Leftover after “notes only the VA” is the
Compile **body** (path list + `009B08C0`),
not a missing dest constant.

---

## Classification (VAs)

| VA | Role | Class |
|---|---|---|
| `0041601D` | first-seen `[edx+8]` | **PROVEN** |
| `0044C72B` | Compile / path list | **PROVEN** |
| `0x122DAA4` | `pc\` | **PROVEN** |
| `0x1236094` | `*.h` | **PARTIAL** |
| `0x13CA7D4` | empty join prefix | **PROVEN** empty |
| `004128A0` slot 0 | cwd + glob | **PROVEN** |
| `004128A0` slot 1 | `pc\` + glob | **PROVEN** first pair |
| `0041A080` | `Data\Defs\` later | **PROVEN** other |
| TLC `pc\` dir / files | — | **DISPROVEN** / **PROVEN** none |
| `00999760` / `00BFED7C` | FindFirst analog | **PROVEN** helper; **DISPROVEN** first-seen fire |
| `009B05F0` | AllowDataGeneration walk | **DISPROVEN** first-seen |
| `009AEF60` | CreateSymbolsFromPathList | **DISPROVEN** first-seen if compiled hits |
| `0044E95C` `009B08C0` | Compile open | **PROVEN** call; filename **PARTIAL** |
| Host dest Note | — | **MATCH** |
| Host glob / Compile body | — | **LEFTOVER** |
| `00DBDE40` | Oakvale | **DISPROVEN** |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00440000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00980000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-01200000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\strings.tsv`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\src\Fable.Core\GameInstall.cs`
- TLC `C:\Program Files (x86)\Steam\steamapps\common\Fable The Lost Chapters\` (`data\`, `userst.ini`)
- `C:\FableCSharp\proofs\00416005-host-prepare\README.md`
- `C:\FableCSharp\proofs\0044C72B-compile\README.md`
- `C:\FableCSharp\proofs\0041A0A0-data-misc\README.md`
- `C:\FableCSharp\proofs\00A01A4F-sound-symbols\README.md`
- `C:\FableCSharp\proofs\ini-activate-quest\README.md`
