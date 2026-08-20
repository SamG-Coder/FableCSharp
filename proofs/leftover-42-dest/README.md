# Leftover #42 dest: `[01232C24+8]` is `0044C72B`

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

Question: leftover #42. Host Notes dest
`0044C72B` as `[vtbl+8]` of live
`01232C24`. Freeze sibling
`proofs/00416005-def-manager` left that
dword **UNREAD**. `proofs/00416005-vtbl8`
could not read PE `0xE32C2C`. Does
`assembly/exe/00-index/vtbl.tsv` lock the
slot? Host Notes `0044C6B0` / `0044C72B`
`[vtbl+8]` / `009ACB10` / `009E5250`.

Authority: Fable.exe dump
`assembly/exe/00-index/vtbl.tsv` (`0x01232C24`
slots 0–6);
`assembly/exe/00-index/sections.txt`;
`assembly/exe/01-sections/text-map/listing-00400000.txt`
(`00416005`–`00416044`, `004185D9`);
`listing-00440000.txt` (`0044C6B0` /
`0044C6C2` / `0044C6F0` / `0044C71F` /
`0044C72B`);
`listing-00980000.txt` (`009ACB10`);
`e8.tsv` dest `0044C72B` (**0** rows);
`ff.tsv` `0041601D` `call [edx+8]`;
`src/Fable.Game/EngineLifecycle.cs`
(`PrepareDefinitionManager`);
siblings `proofs/00416005-def-manager`,
`proofs/00416005-vtbl8`,
`proofs/00416005-host-prepare`,
`proofs/0044C72B-compile`,
`proofs/0044C72B-pc-glob`,
`proofs/issue-42-verify`.

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| `[01232C24+8]` dest? | `0x0044C72B`. `vtbl.tsv` row `0x01232C24` slot **2**. Slot `2` × 4 = **+8**. Dword VA `01232C2C`. | **PROVEN** |
| `00416005` `[edx+8]` is that dest? | Getter `0044C6B0` `[0x13B879C]`. Live vtbl store `0044C6F0` `mov [esi], 0x1232C24`. 0-arg thiscall. **0** `.text` `E8` of dest `0044C72B`. First-seen is this virtual. | **PROVEN** |
| Freeze / vtbl8 dest **UNREAD**? | **Stale.** Those proofs predate `vtbl.tsv`. Listings still stop at `.text` end `0122CFFE`; the dump that closes #42 is the rdata index. | **DISPROVEN** as still unread |
| Host dest Note `0044C72B [vtbl+8]`? | **MATCH** the locked dword. | **MATCH** |
| Invent a `game.bin` parser from this lock? | **No.** Scanner rows 3–6 are UTF-16 **`game.bin`** at `01232C30` (ctor `push 0x1232C30` name), **not** methods. Compile open `009B08C0` stays host **LEFTOVER**. | **DISPROVEN** as dest work |

**Answer:** dest is **PROVEN** `0044C72B`.
Leftover #42 dest identity is closed. Host
leftover after the dest Note is still the
Compile **body**, not a missing rdata dword.
Do **not** parse `game.bin` here.

---

## Direct answers

| Claim | Class |
|---|---|
| `vtbl.tsv` `0x01232C24` slot 2 = `0x0044C72B` | **PROVEN** |
| Slot 2 is `[vtbl+8]` | **PROVEN** |
| Live object vtbl is `01232C24` | **PROVEN** (`0044C6F0`) |
| Work this is `[0x13B879C]` via `0044C6B0` | **PROVEN** |
| Sole first-seen of `0044C72B` is `0041601D` | **PROVEN** (0 `e8.tsv` rows) |
| `0044C72B` is Compile neighbor / `"Game Definition Manager: Compile"` | **PROVEN** as that fn (sibling) |
| Scanner slots 3–6 are methods | **DISPROVEN** (wchar `game.bin`) |
| Host Notes dest VA | **MATCH** |
| Host Compile body / `009B08C0` / GetDefs | **LEFTOVER** |
| Host `game.bin` parse here | **DISPROVEN** |
| This stage is `[0x13B8A54]` / `00A38500` / `00A39010` | **DISPROVEN** |
| Oakvale / `00DBDE40` | **DISPROVEN** |

---

## 1. Quoted rdata (`vtbl.tsv`)

`C:\FableCSharp\assembly\exe\00-index\vtbl.tsv`
header `vtbl	slot	dest`. Run for
`0x01232C24`:

```
0x01232C24	0	0x00450BE3
0x01232C24	1	0x009FC540
0x01232C24	2	0x0044C72B
0x01232C24	3	0x00610067
0x01232C24	4	0x0065006D
0x01232C24	5	0x0062002E
0x01232C24	6	0x006E0069
```

| Slot | Offset | Dword VA | Dest | Role |
|---|---|---|---|---|
| 0 | +0 | `01232C24` | `00450BE3` | method |
| 1 | +4 | `01232C28` | `009FC540` | method |
| 2 | **+8** | **`01232C2C`** | **`0044C72B`** | **this dest** |
| 3–6 | +12… | `01232C30`… | not `.text` fns | over-read |

`.rdata` (`sections.txt`): rva `0xE2D000` →
VA `0122D000`. Slot+8 file offset
`0xE32C2C`. Index is v2 (`assembly/README.md`).

`assembly/README.md` already greps
`0x0044C72B` in this TSV as the example
vtbl lookup.

Slots 3–6 little-endian UTF-16:

`67 00 61 00 6D 00 65 00 2E 00 62 00 69 00 6E 00`
= **`game.bin`**. That is VA `01232C30`, the
ctor `0099B6B0` immediate copied to
`[this+184]` (`0044C6C2`). ASCII
`strings.tsv` skips wchar (gap). **Not** a
fourth method. **Not** a parse.

---

## 2. Call site (already locked)

`listing-00400000.txt` `00416005`:

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

`ff.tsv`: `0x0041601D	call	[edx+8]	8`
(folded under `0x00415E85`; treat
`00416005` as its own fn from the listing).

`listing-00440000.txt`:

```
0044C6B0  mov eax, [0x13B879C]
          ret
…
0044C6F0  mov [esi], 0x1232C24
…
0044C71F  push ecx
          mov ecx, 0x13B879C
          call 00450142
          ret
0044C72B  push ebp               ; Compile; 0-arg thiscall
          mov ebp, esp
          sub esp, 44
          …
          mov esi, ecx
          push 0x122DAA4         ; wchar pc\
```

`listing-00980000.txt`:

```
009ACB10  mov ecx, [ecx+88]
          jmp 009E5250
```

`e8.tsv` dest `0x0044C72B`: **zero** rows.
Virtual-only. First-seen is `0041601D`.

---

## 3. Why freeze dest-UNREAD is stale

| Proof | Dest row | Now |
|---|---|---|
| `00416005-def-manager` | `[edx+8]` **UNREAD** (rdata not listed) | **STALE** |
| `00416005-vtbl8` | PE `0xE32C2C` unread this pass | **STALE** |
| `issue-42-verify` | no `WriteVtblPart` / no quoted dword | **STALE** vs `vtbl.tsv` |
| `00416005-host-prepare` | dest **PROVEN** from exe, no TSV quote | dest now **quoted** |
| `0044C72B-compile` | dest **PROVEN** as file `0xE32C2C` without TSV | **MATCH** dest; quote is this file |
| `docs/status` leftover dest **UNREAD** | leave #42 open | dest **PROVEN**; body leftover remains |

What would have **DISPROVEN** dest: any other
mapped `.text` VA in slot 2. Slot 2 is
`0044C72B`. Host `DefinitionManagerVtbl8Fn`
**MATCH**.

---

## 4. Host leftover after dest lock

`PrepareDefinitionManager` Notes:

- `0044C6B0 [0x13B879C]`
- `0044C72B [vtbl+8]`
- `009ACB10 [this+88] arg=1`
- `009E5250` list reset

Sets `DefinitionManagerPrepared`. Comment:
“Not a game.bin parse.”

| Piece | Host | Class |
|---|---|---|
| Dest `0044C72B` | Note | **MATCH** (rdata **PROVEN**) |
| Getter + `009ACB10` / `009E5250` | Notes | **MATCH** names; reset body still Note-only |
| Path list / `009B0AC0` table / `009B08C0` / GetDefs `+208` | absent | **LEFTOVER** (not #42 dest) |
| `game.bin` parse | absent | **MATCH** (must stay so) |

Different object from later Subtitled
`[0x13B8A54]` (`00A38500` / `00A39010`).

---

## Classification (VAs)

| VA | Role | Class |
|---|---|---|
| `01232C24` | live vtbl | **PROVEN** |
| `01232C2C` | slot+8 dword | **PROVEN** `0044C72B` |
| `0041601D` | first-seen `[edx+8]` | **PROVEN** |
| `0044C72B` | dest / Compile fn | **PROVEN** dest |
| `0044C6B0` / `[0x13B879C]` | work this | **PROVEN** |
| `009ACB10` / `009E5250` | after virtual, arg 1 | **PROVEN** call |
| `01232C30` | wchar `game.bin` name | **PROVEN** neighbor string; **DISPROVEN** as slot |
| Host dest Note | — | **MATCH** |
| Host Compile body | — | **LEFTOVER** |
| `00DBDE40` | Oakvale | **DISPROVEN** |

---

## Sources

- `C:\FableCSharp\assembly\exe\00-index\vtbl.tsv`
- `C:\FableCSharp\assembly\exe\00-index\sections.txt`
- `C:\FableCSharp\assembly\README.md`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00440000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00980000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\ff.tsv`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\proofs\00416005-def-manager\README.md`
- `C:\FableCSharp\proofs\00416005-vtbl8\README.md`
- `C:\FableCSharp\proofs\00416005-host-prepare\README.md`
- `C:\FableCSharp\proofs\0044C72B-compile\README.md`
- `C:\FableCSharp\proofs\0044C72B-pc-glob\README.md`
- `C:\FableCSharp\proofs\issue-42-verify\README.md`
