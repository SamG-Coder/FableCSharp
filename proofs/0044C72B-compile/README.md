# `0044C72B` first-seen body on Init Definition Manager

Investigation only. No production `src/` or `tests/`
edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave this
walk is `FinalAlbion.wld` → `"Init Game"` →
`00418DCA` → vtbl+4 `004184BD` →
`"Init Definition Manager"` `004185D9`
`call 00416005`. Do **not** invent a `game.bin`
/ `00A38E50` parser.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: `01232C24+8` is `0044C72B` (recovered
from Fable.exe rdata). `00416005` `call [edx+8]`
therefore is `0044C72B`. First-seen body of
`0044C72B` on Init Definition Manager? Path
strings `0x122DAA4` / `0x1236094`? Does it open
`game.bin` / `009B08C0`? Host leftover after
Note-only `00416005`?

Authority: Fable.exe dump
`listing-00440000.txt` (`0044C72B`–
`0044EA4D`, `0044E92F` / `0044E95C`);
`listing-00400000.txt` (`00416005`–
`00416044`, `004185D9`, `0041A080`,
`004128A0`, `0042F5A9` / `0042F6FA` /
`00433C7B`);
`listing-00980000.txt` (`009B08C0`,
`009A76A0`, `0099B720`);
`listing-00cc0000.txt` (`00CD3F50`);
exe rdata `01232C2C=0044C72B`;
`e8.tsv` dests `00416005` / `0044C72B` /
`009B08C0` / `004336BC`;
`functions.tsv` (`0044C72B` 4297 insns);
`strings.tsv` / `xrefs.tsv` / `sections.txt`;
`src/Fable.Game/EngineLifecycle.cs`
(`PrepareDefinitionManager`);
siblings `proofs/00416005-def-manager`,
`proofs/00416005-vtbl8`,
`proofs/00416005-host-prepare`,
`proofs/0044C6C2-plus40`,
`proofs/script-bank-open`.

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| `01232C24+8` dest? | `0044C72B`. File `0xE32C2C` dword. Sibling `00416005-vtbl8` left this **UNREAD**; this pass treats the rdata hit as closed. | **PROVEN** |
| `00416005` `[edx+8]` is that dest? | Live vtbl is `01232C24` (`0044C6F0`). 0-arg thiscall matches `0044C72B`. **0** `.text` `E8` of dest `0044C72B`. First-seen is this virtual. | **PROVEN** |
| First-seen body? | Path list (`0x122DAA4` / `0x1236094` / `0041A080` `Data\Defs\`) then `009B0AC0` bank (`CHeroPostcardGeneratorDef` first) then `"Game Definition Manager: Compile"` / `009B08C0` then GetDefs into `+208..+220`. | **PROVEN** |
| `0x122DAA4`? | UTF-16 **`pc\`**. 8-byte gap before ASCII `PARTICLE_FRONTEND_PC` @ `0x0122DAAC`. `0099B6B0` wchar intern. | **PROVEN** |
| `0x1236094`? | UTF-16 glob. Sibling `00416005-host-prepare` decodes **`*.h`**. Not in ASCII `strings.tsv` (gap after `CHeroPostcardGeneratorDef`). Neighbors `0x1236088` / `0x123607C` (12-byte stride) stay unread. | **PARTIAL** |
| Open `game.bin` / `009B08C0`? | **Yes** `009B08C0` @ `0044E95C` on this object. No `.bin` literal. Host analog is `CompiledDefs/game.bin`. Do **not** parse it here. Process-first `009B08C0` is earlier frontend `00433C7B`. | **PROVEN** open; **PARTIAL** filename; **DISPROVEN** as first process open |
| Host leftover after Note-only `00416005`? | Stage name is **not** Note-only anymore (`PrepareDefinitionManager`). Notes assume dest + reset. Leftover is the **Compile body**. | **PROVEN** leftover body |

**Answer:** first-seen `0044C72B` is the
`[0x13B879C]` Compile virtual. Recovered path
prefix is **`pc\`**; first glob is **`*.h`**
(**PARTIAL**). It does call `009B08C0` (game.bin
analog). Host leftover is that body, not the
stage name.

---

## Direct answers

| Claim | Class |
|---|---|
| `01232C2C` dword is `0044C72B` | **PROVEN** (exe rdata) |
| Sole first-seen of `0044C72B` is `0041601D` | **PROVEN** (0 `e8.tsv` rows) |
| First insns intern `0x122DAA4` then `0x1236094` | **PROVEN** |
| `0x122DAA4` = UTF-16 `pc\` | **PROVEN** |
| `0x1236094` = UTF-16 `*.h` | **PARTIAL** |
| First `009B0AC0` name is `CHeroPostcardGeneratorDef` | **PROVEN** |
| `"Game Definition Manager: Compile"` then `009B08C0` | **PROVEN** |
| `009B08C0` is `game.bin` bytes | **PARTIAL** analog; no literal |
| Host opens / parses `game.bin` here | **DISPROVEN** |
| Host named row + getter + vtbl+8 + reset Notes | **MATCH** |
| Host Compile body (path / `009B0AC0` table / open / GetDefs) | **LEFTOVER** |
| Oakvale / `00DBDE40` | **DISPROVEN** |

---

## 1. Slot is now the dest

`0044C6C2` writes vtbl `01232C24`. Slot+8 is
`01232C2C`. Exe rdata dword there is
`0044C72B`.

`00416005` (`listing-00400000.txt`):

```
00416014  call 0044C6B0          ; eax = [0x13B879C]
00416019  mov edx, [eax]
0041601B  mov ecx, eax
0041601D  call [edx+8]           ; 0044C72B
00416020  cmp [esp+4], 0x00
00416025  je 00416033
00416027  call 0044C6B0
0041602C  mov ecx, eax
0041602E  call 009ACB10          ; parent push 1
```

No stack args. `0044C72B` is `push ebp` /
`mov esi, ecx` / `ret` (not `ret 4`).
`e8.tsv` dest `0044C72B`: **0** rows.

Sibling `00416005-vtbl8` could not read
`.rdata` and left dest **UNREAD**. That row
is **stale**.

---

## 2. First-seen body

`functions.tsv` `0x0044C72B` 4297 insns,
`0044C72B`–`0044EA4D`. Four phases.

### 2a. Path list (first work)

```
0044C738  push 0x122DAA4
0044C73D  lea ecx, [ebp-8]
          call 0099B6B0          ; intern prefix
0044C757  mov edi, 0x1236094
0044C75C  push edi
          lea ecx, [ebp-12]
          call 009A76A0          ; intern glob
          call 0099BF30          ; concat
          lea ecx, [ebp-32]
          call 004128A0          ; vector push
```

Then `pc\` + glob again (`0099BE70` /
`0099BF30` / `004128A0`), then
`0041A080` (`push 0x122F3D0` /
`Data\Defs\`) + `0x1236094`, then the same
prefix with `0x1236088` and `0x123607C`.
Second list at `[ebp-44]`.

`004128A0` is a CString vector push
(`0099B720` or grow `00412330`). Twin
script `00CD3F50` starts with the same
`0x122DAA4` / `0x1236094` pair.

**First-seen callees** (first hit only):

| Order | VA | Role |
|---|---|---|
| 1 | `0099B6B0` | intern `0x122DAA4` |
| 2 | `009A76A0` | intern `0x1236094` |
| 3 | `0099BF30` | wchar concat |
| 4 | `004128A0` | path-list push |
| 5 | `0099B510` | string dtor |
| 6 | `0099BE70` | wchar join |
| 7 | `0041A080` | intern `Data\Defs\` |

### 2b. `009B0AC0` bank

`or edi, -1` then the usual
`0099EBF0` / `0099EC30` / `009B0AC0` /
`0099EAE0` island. First name
`CHeroPostcardGeneratorDef`, factory
immediate `0x45D613`. Then `GLOBAL`,
`CREATURE`, `CCreatureDef`, … through
`CONFIG_OPTIONS_DEFAULTS_DEF`.
`xrefs.tsv` lists the island under
`fn=0x0044C72B`.

This is **after** Init Thing Components
`004EE23F` already registered
`CHeroMorphDef`. Re-register, not the
first `009B0AC0` on the walk.

Then `005DD670` / `0067F9B0` (bodies
**UNREAD** this pass) and `00433DF0`
copies `[ebp-32]` → `[this+64]`,
`[ebp-44]` → `[this+76]`.

### 2c. Compile open

```
0044E92F  push "Game Definition Manager: Compile"
          … 009E9F40 / 0099EAE0 …
0044E95A  mov ecx, esi
0044E95C  call 009B08C0
0044E962  push "Game Definition Manager: GetDefs"
```

`009B08C0` (`listing-00980000.txt`):
`009B05F0` on `[this+64]`; if
`[0x13CA7D8]` and `00999230([this+184])`
then `00994700(0x4000)` + `009AFB90`.
Else `009AEF60` / `009AF480` on
`[this+76]`. `[this+184]` is the
`0099B6B0(0x1232C30)` name from
`0044C6C2` (wchar; ASCII **UNREAD**).

`e8.tsv` dest `009B08C0` (exactly 3):

| Site | Manager | Analog | First-seen? |
|---|---|---|---|
| `00433C7B` | frontend `004336BC` via `0042F6FA` (`0042F5A9` `FRONT_END`) | `frontend.bin` | **process-first** (before Leave) |
| `0044E95C` | this Compile | `game.bin` | **first on `[0x13B879C]`** |
| `00CD422B` | script `00CD3F50` | `script.bin` | later Init Scripts |

No `.bin` literal in the exe
(`script-bank-open`). Host
`GameInstall.FindCompiledDef("game.bin")`
is the analog. **Do not** invent a parser.

### 2d. GetDefs stores

`0043FEB0` `"GLOBAL"` → `[this+208]`;
`00430096` `"ENGINE"` → `+212`;
`00451354` `"ENVIRONMENT"` → `+216`;
`004513B9` `+220` (push `ebx`; name
**PARTIAL**). These are the four dwords
`0044C6C2` zeroed. Then `00412130`
drops the path lists.

`00416005` then `009ACB10` →
`009E5250` on `[this+88]` (sibling;
different list).

---

## 3. Path strings

`.rdata` starts `0122D000`. ASCII
`strings.tsv` skips wchar.

| VA | Layout | Decode | Class |
|---|---|---|---|
| `0x0122DAA4` | 8 bytes then `PARTICLE_FRONTEND_PC` @ `0x0122DAAC` | UTF-16 `pc\` + NUL | **PROVEN** |
| `0x0122F3D0` | `0041A080` immediate | UTF-16 `Data\Defs\` (sibling `0041A0A0-data-misc`) | **PROVEN** prefix |
| `0x01236094` | after ASCII `CHeroPostcardGeneratorDef` @ `0x01236060`; next ASCII `MiniMapGraphics` @ `0x012362A8` | sibling `*.h` | **PARTIAL** |
| `0x01236088` / `0x0123607C` | 12-byte stride before `0x01236094` | other globs | **UNREAD** |

`0099B6B0` on `0x122DAA4` is the same
wchar intern as `0041A080`.
`00416005-host-prepare` already printed
`pc\` + `*.h` as the first pair.

Joined search shapes (not a file open
by themselves): `pc\`+`*.h`,
`Data\Defs\`+`*.h`, plus the two unread
globs. Same pair feeds script
`00CD3F50` before `009B08C0`.

---

## 4. Host leftover

`EnterGame` is **not** Note-only on this
name:

```
Note(apply, name, "InitGame", name);
if (name == "Init Definition Manager")
    PrepareDefinitionManager();
```

`PrepareDefinitionManager` Notes
`0044C6B0`, `0044C72B [vtbl+8]`,
`009ACB10`, `009E5250`. Sets
`DefinitionManagerPrepared`. Comment:
“Not a game.bin parse.”

| Piece | Host | Class |
|---|---|---|
| Stage name | Note | **MATCH** |
| Dest `0044C72B` | Note | **MATCH** (rdata now **PROVEN**) |
| `009ACB10` / `009E5250` | Note | **MATCH** name; body still Note-only |
| Path list `pc\` / `*.h` / `Data\Defs\` | absent | **LEFTOVER** |
| `009B0AC0` Compile table | absent | **LEFTOVER** |
| `009B08C0` / GetDefs `+208..+220` | absent | **LEFTOVER** |

Adding a `game.bin` parse here would
still be invention. Later `EnsureDefs`
lookup is **not** this first-seen body.

---

## Classification (VAs)

| VA | Role | Class |
|---|---|---|
| `01232C24` | live vtbl | **PROVEN** |
| `01232C2C` | slot+8 = `0044C72B` | **PROVEN** |
| `0041601D` | first-seen call | **PROVEN** |
| `0044C72B` | Compile body | **PROVEN** |
| `0x122DAA4` | `pc\` | **PROVEN** |
| `0x1236094` | `*.h` | **PARTIAL** |
| `0x1236088` / `0x123607C` | other globs | **UNREAD** |
| `0041A080` / `0x122F3D0` | `Data\Defs\` | **PROVEN** |
| `004128A0` | path-list push | **PROVEN** |
| `009B0AC0` first here | `CHeroPostcardGeneratorDef` | **PROVEN** |
| `0044E95C` `009B08C0` | Compile open | **PROVEN** |
| `game.bin` literal | — | **DISPROVEN** |
| `00433C7B` | process-first `009B08C0` | **PROVEN** other |
| `0043FEB0` / `00430096` / `00451354` | GetDefs | **PROVEN** |
| Host Notes dest + reset | — | **MATCH** |
| Host Compile body | — | **LEFTOVER** |
| `00DBDE40` | Oakvale | **DISPROVEN** |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00440000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00980000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00cc0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\functions.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\strings.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\xrefs.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\sections.txt`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\proofs\00416005-def-manager\README.md`
- `C:\FableCSharp\proofs\00416005-vtbl8\README.md`
- `C:\FableCSharp\proofs\00416005-host-prepare\README.md`
- `C:\FableCSharp\proofs\0044C6C2-plus40\README.md`
- `C:\FableCSharp\proofs\script-bank-open\README.md`
