# `01232C24+8` dest of `00416005` `[edx+8]`

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

Question: `00416005` does `0044C6B0` then
`call [edx+8]` then `009ACB10`. Sibling
`proofs/00416005-def-manager` left `[edx+8]`
**UNREAD**. Recover `.rdata` vtbl `01232C24+8`
dest. Is it `0044C72B` Compile? Host leftover vs
Note-only?

Authority: Fable.exe dump
`listing-00400000.txt` (`004185D9`,
`00416005`–`00416044`, `004336B0` /
`004336BC`, `0042F6FA`);
`listing-00440000.txt` (`0044C6B0` /
`0044C6C2` / `0044C71F` / `0044C72B` /
`0044E92F` / `0044E95C`);
`listing-00980000.txt` (`009ACB10`);
`e8.tsv` dests `00416005` / `0044C72B` /
`004336BC` / `00CD3F50` / `009B08C0`;
`functions.tsv` (`0044C6C2` / `0044C72B`);
`out/00-index/sections.txt` / `strings.tsv` /
`xrefs.tsv` / `rtti.txt`;
`src/Fable.Game/EngineLifecycle.cs`
(`InitGameStages` / `EnterGame` /
`PrepareDefinitionManager` /
`DefinitionManagerVtbl8Fn`);
siblings `proofs/00416005-def-manager`,
`proofs/0044C6C2-plus40`,
`proofs/script-bank-open`,
`proofs/morph-first`.

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| `[edx+8]` dest VA? | **Not recovered.** Slot dword at `01232C24+8` = `01232C2C` is past `listing-01200000` (`.text` ends `0122CFFE`; `.rdata` starts `0122D000`). `xrefs.tsv` has **no** `01232C24` row (string xrefs only). No `WriteVtblPart` / implementer `vtbl` dump of this table. PE bytes at file `0xE32C2C` were not readable this pass. | **UNREAD** |
| Is dest `0044C72B` Compile? | **Candidate only.** 0-arg thiscall; **0** `.text` `E8`; sits after `0044C6C2` / `0044C71F`; body logs `"Game Definition Manager: Compile"` then `009B08C0`. Twin of frontend `004336BC` (1 `E8`) and script `00CD3F50` (0 `E8`, still **PARTIAL** as `[0x143E920].vtbl+8`). | **PARTIAL** |
| Host leftover vs Note-only? | Named row **MATCH**. Host now Notes getter + **assumed** `0044C72B` + `009ACB10`. Compile **body** (`009B08C0` / bank open) is still **LEFTOVER**. Not name-only leftover. Do **not** treat Notes as a proven rdata hit. | **PROVEN** leftover body; dest still **UNREAD** |

**Answer:** `[vtbl+8]` dest VA is **UNREAD**.
It is **not** proven `0044C72B`. Host leftover
is the Compile / reset **work**, not a missing
stage name.

---

## Direct answers

| Claim | Class |
|---|---|
| `00416005` `call [edx+8]` this = `[0x13B879C]` from `0044C6B0` | **PROVEN** |
| Live vtbl of that object is `01232C24` (`0044C6F0`) | **PROVEN** |
| `01232C24+8` dword listed / dumped | **UNREAD** |
| Dest is `0044C72B` | **PARTIAL** (not PROVEN) |
| `0044C72B` is `"Game Definition Manager: Compile"` + `009B08C0` | **PROVEN** as that fn |
| `0044C72B` has a `.text` `E8` | **DISPROVEN** (0 rows in `e8.tsv`) |
| Host `DefinitionManagerVtbl8Fn = 0044C72B` | host assumption; **not** a listing dword |
| Host named stage present | **MATCH** |
| Host runs Compile / `009B08C0` / `game.bin` | **DISPROVEN** (Notes only; comment “Not a game.bin parse”) |
| Oakvale / `00DBDE40` | **DISPROVEN** |

---

## 1. Call site (locked by sibling)

`listing-00400000.txt` `00416005`–`00416044`:

```
00416014  call 0044C6B0          ; eax = [0x13B879C]
00416019  mov edx, [eax]
0041601B  mov ecx, eax
0041601D  call [edx+8]           ; THIS SLOT
00416020  cmp [esp+4], 0x00
00416025  je 00416033
00416027  call 0044C6B0
0041602C  mov ecx, eax
0041602E  call 009ACB10          ; parent push 1
```

No stack args between getter and `[edx+8]`.
0-arg thiscall. Incoming `ecx=game` is
overwritten.

`0044C6C2` (`listing-00440000.txt`):

```
0044C6F0  mov [esi], 0x1232C24
…
0044C71F  push ecx
0044C720  mov ecx, 0x13B879C
0044C725  call 00450142
0044C72A  ret
0044C72B  push ebp               ; next frame
```

---

## 2. Why `01232C24+8` stays UNREAD

| Check | Result |
|---|---|
| `listing-01200000.txt` last VA | `0122CFFE` (`add [eax], al`) |
| `.text` | rva `0x1000` size `0xE2C000` → VA end `0122D000` |
| `.rdata` | rva `0xE2D000` → VA `0122D000` |
| `01232C24` | `.rdata` + `0x5C24`; file `0xE32C24` |
| `01232C24` / `01232C2C` in listings | **zero** hits |
| `xrefs.tsv` dest `0x01232C24` | **zero** (string table) |
| `strings.tsv` `0x01232C24` / `0x01232C30` | **absent** (gap `01231B60`…`012331A8`) |
| Existing `vtbl` dump of `01232C24` | **none** |

`push 0x1232C30` in the ctor is the
`0099B6B0` name source copied to
`[this+184]`, **not** a printed C string and
**not** a recovered slot+12. It does **not**
fill slot+8.

`rtti.txt` has `CDefinitionManager`
`0x01375C24`, `CStartupDefinitionManager`
`0x01375C48`, `CGameDefinitionManager`
`0x01376718`, `CPlayerManager` `0x01376174`.
No COL / vtbl link to `01232C24` this pass.
Family stays **PARTIAL**
(`CGameDefinitionManager` is the nearest
unused name). Host `PlayerManagerVtbl` is a
label, not RTTI.

Until `dotnet run --project tools/Fable.ExeIndex
-- vtbl 0x01232C24 8` (or `WriteU32Part` of
`01232C2C`) is persisted, dest is **UNREAD**.

---

## 3. Why `0044C72B` is only PARTIAL

**PROVEN about the function**

- Next frame after store `0044C71F`.
- 0-arg thiscall (`push ebp` / `mov ebp, esp` /
  `sub esp, 44` / `mov esi, ecx`). Matches
  `0041601D`.
- `functions.tsv` `0x0044C72B` 4297-callee
  bank registrar. String island includes
  `Game Definition Manager: Compile` /
  `GetDefs`.
- `listing-00440000.txt`:

```
0044E92F  push "Game Definition Manager: Compile"
…
0044E95C  call 009B08C0
0044E962  push "Game Definition Manager: GetDefs"
```

- `e8.tsv` dest `0044C72B`: **0** rows.
  Virtual-only candidate (same test the sibling
  used).

**Same shape, other managers**

| Object | Ctor / store | Next frame | `.text` `E8` | Slot |
|---|---|---|---|---|
| `[0x13B879C]` `0xE0` `01232C24` | `0044C6C2` / `0044C71F` | `0044C72B` | **0** | this question |
| `[0x13B8760]` `0xD0` `0123117C` | `00433693` / `004336B0` | `004336BC` | **1** (`0042F6FA`) | frontend Compile analog |
| `[0x143E920]` `0xD0` `012C2648` | `00CD3F00` / `00CD3F40` | `00CD3F50` | **0** | script `vtbl+8` still **PARTIAL** |

`004336BC` also starts `push 0x122DAA4` /
`0099B6B0` then path-list `004128A0`.
`0042F6FA` is a **direct** frontend call, so
that twin is **not** virtual-only.

`script-bank-open` already left
`00CD3F50 == [0x143E920].vtbl+8` **PARTIAL**
for the same missing rdata reason. Do not
promote this slot while that analog is unread.

`009B08C0` @ `0044E95C` is the **other-bank**
open (`GLOBAL` / `ENGINE` / `ENVIRONMENT`).
Sibling called that a `game.bin` analog.
Identifying the virtual as Compile would still
**not** prove a host parse. Do not invent one.

**What would flip this to PROVEN**

`[ 2] +  8  0x0044C72B` from `vtbl 0x01232C24`.
Any other mapped `.text` VA at that dword
**DISPROVES** the host constant.

---

## 4. Host leftover vs Note-only

Sibling `00416005-def-manager` said
`EnterGame` only Notes the name. That is
**stale**. Current `EnterGame`:

```
foreach InitGameStages
  Note(apply, name, "InitGame", name);
  if (name == "Init Definition Manager")
    PrepareDefinitionManager();
```

`PrepareDefinitionManager` Notes:

- `0044C6B0 [0x13B879C]`
- `0044C72B [vtbl+8]` ← **assumes** dest
- `009ACB10 [this+88] arg=1`
- `009E5250` list reset

`EngineLifecycle.DefinitionManagerVtbl8Fn =
0x0044C72B`. Tests assert that constant. Those
are host/test claims, **not** an rdata read.

| If dest dump is… | Host notes | Leftover |
|---|---|---|
| `0044C72B` | dest **MATCH** (Notes) | Compile **body** `009B08C0` / GetDefs / `+208` fills. Reset body still Note-only. |
| other `.text` VA | dest Notes **DIVERGE** | that VA + `009ACB10` |
| still unread | dest Notes **PARTIAL** | same as now |

Keep Note-only of the name: **already not**
the host state. Adding a `game.bin` open here
would still be invention.

---

## Classification (VAs)

| VA | Role | Class |
|---|---|---|
| `004185D9` | sole `E8` of `00416005` | **PROVEN** |
| `0041601D` | `call [edx+8]` | **PROVEN** call |
| `01232C24` | live vtbl | **PROVEN** |
| `01232C2C` | slot+8 dword | **UNREAD** |
| `[vtbl+8]` dest | — | **UNREAD** |
| `0044C72B` | Compile neighbor / host guess | **PARTIAL** as slot; **PROVEN** as Compile fn |
| `0044E95C` `009B08C0` | Compile open | **PROVEN** inside `0044C72B`; **not** proven as this virtual |
| `004336BC` | frontend twin | **PROVEN** fn; **DISPROVEN** as virtual-only |
| `00CD3F50` | script twin | **PARTIAL** as `vtbl+8` |
| `009ACB10` / `009E5250` | after virtual, arg 1 | **PROVEN** |
| Host name row | present | **MATCH** |
| Host `PrepareDefinitionManager` | Notes assumed dest | **PARTIAL** dest; Compile body **LEFTOVER** |
| `00DBDE40` | Oakvale | **DISPROVEN** |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00440000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00980000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-01200000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\functions.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\sections.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\strings.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\xrefs.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\rtti.txt`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\proofs\00416005-def-manager\README.md`
- `C:\FableCSharp\proofs\0044C6C2-plus40\README.md`
- `C:\FableCSharp\proofs\script-bank-open\README.md`
- `C:\FableCSharp\proofs\morph-first\README.md`
