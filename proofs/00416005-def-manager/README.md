# `00416005` Init Definition Manager first-seen body

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave this
walk is `FinalAlbion.wld` → `"Init Game"` →
`00418DCA` → vtbl+4 `004184BD`. Do **not** invent
a `game.bin` / `00A38E50` parser.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: Init Game named stage `"Init Definition
Manager"` `00416005`. What is the first-seen body?
Does it construct `[0x13B8A54]` (`00A38500` already
ran at `0121A630`) or a different object? Host
`EnterGame` only Notes the name — leftover?
Relation to later `004CDB10` `00A39010` fill of
`[0x13B8A54]`?

Authority: Fable.exe dump
`listing-00400000.txt` (`004184BD` around
`004185D9` `call 00416005`, `00416005`–
`00416044`);
`listing-00440000.txt` (`0044C6B0` / `0044C6C2` /
`0044C71F` / `0044C72B`);
`listing-00980000.txt` (`009ACB10` / `009E5250`);
`listing-009c0000.txt` (`009F2450` / `009F2870`);
`listing-00a00000.txt` (`00A38500` / `00A39010`);
`listing-01200000.txt` (`0121A630`);
`e8.tsv` dests `00416005` / `00A38500` /
`009ACB10` / `009F2450` / `0044C6B0`;
`functions.tsv` (`004184BD` callee list;
`00416005` folded under `00415E85`);
`src/Fable.Game/EngineLifecycle.cs`
(`InitGameStages` / `EnterGame`);
siblings `proofs/004CDB10-00A39010`,
`proofs/004CDB10-host-register`,
`proofs/0044C6C2-plus40`,
`proofs/0044C6B6-host-ensure`,
`proofs/morph-first`.

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| First-seen body of `00416005`? | Getter `[0x13B879C]` then **`[vtbl+8]`** on that live `0xE0` object. Arg `1` then `009ACB10` → `[this+88]` → `009E5250` list reset. Optional `009F2450` on `[0x13CAA90]`. | **PROVEN** |
| Construct `[0x13B8A54]` here? | **No.** Zero hits of `0x13B8A54` in `00416005`. `00A38500` already ran at `0121A630`. | **DISPROVEN** |
| Different object? | **Yes.** Work this-ptr is `[0x13B879C]` (`0044C6B0`), vtbl `01232C24`, already stored by `0044C71F`. Incoming `ecx=game` is overwritten. | **PROVEN** |
| Host `EnterGame` only Notes the name? | **Yes.** `InitGameStages` row **MATCH**. No `0044C6B0`, no `[vtbl+8]`, no `009ACB10`. | **PROVEN** leftover Note-only |
| Relation to later `004CDB10` `00A39010`? | **Different BSS singleton.** Subtitled fills `[0x13B8A54]` (`vtbl 0129CF84`). This site does not. | **PROVEN** |

**Answer:** first-seen body is **`0044C6B0` +
`[edx+8]`** on **`[0x13B879C]`**, not a ctor of
`[0x13B8A54]`. Host leftover **is** that work.
`004CDB10` is a later named sibling that fills the
already-live enum object.

---

## Direct answers

| Claim | Class |
|---|---|
| Sole `.text` `E8` of dest `00416005` is `004185D9` | **PROVEN** |
| Parent logs `"Init Definition Manager"` then `push 1` / `ecx=esi` / `call 00416005` | **PROVEN** |
| After `"Init Thing Components"` `004EE23F`, before `"Init Graphics"` `00416C8A` | **PROVEN** |
| `00416005` stores a new heap / BSS object | **DISPROVEN** — no stores in the fn |
| `00A38500` / `0121A630` construct `[0x13B8A54]` before Init Game | **PROVEN** |
| `00416005` is that ctor | **DISPROVEN** |
| `[edx+8]` identity is `0044C72B` | **UNREAD** (rdata vtbl `01232C24` not listed; `morph-first`) |
| `0044C72B` `"Game Definition Manager: Compile"` / `009B08C0` is `game.bin` analog | **PROVEN** as that fn; **PARTIAL** as this virtual |
| Host named stage present | **MATCH** |
| Host body | **LEFTOVER** |
| Oakvale / `00DBDE40` | **DISPROVEN** |

---

## 1. Site on `004184BD`

`listing-00400000.txt`:

```
00418585  call 004EE23F          ; Init Thing Components
0041858A  push edi
0041858B  push "Init Definition Manager"
… log trio 0099EBF0 / 009D8240 / 0099EAE0 …
004185AA  push edi
004185AB  push "Init Definition Manager"
… log trio 0099EBF0 / 009E9F40 / 0099EAE0 …
004185D5  push 1
004185D7  mov ecx, esi           ; game; discarded inside
004185D9  call 00416005          ; THIS SITE
004185DE  push edi
004185DF  push "Init Graphics"
…
00418600  call 00416C8A
```

`esi` is game (`004184D1` `[0x13B86A0]=esi`).
`e8.tsv` dest `00416005`: **only** `004185D9`.

`functions.tsv` `004184BD` callee list includes
`00416005` after Thing Components, before
`00416C8A`.

`xrefs.tsv`: string `0x0122F130` at `0041858C` /
`004185AC` (`fn=0x004184BD`).

---

## 2. First-seen body

`00416005`–`00416044` (`ret 4`):

```
00416005  mov ecx, [0x13CAA90]
0041600B  test ecx, ecx
0041600D  je 00416014
0041600F  call 009F2450          ; tally; no object store
00416014  call 0044C6B0          ; eax = [0x13B879C]
00416019  mov edx, [eax]
0041601B  mov ecx, eax
0041601D  call [edx+8]           ; FIRST WORK
00416020  cmp [esp+4], 0x00
00416025  je 00416033
00416027  call 0044C6B0
0041602C  mov ecx, eax
0041602E  call 009ACB10          ; arg!=0 (here 1)
00416033  mov ecx, [0x13CAA90]
00416039  test ecx, ecx
0041603B  je 00416042
0041603D  call 009F2450
00416042  mov al, 0x01
00416044  ret 4
```

Incoming `ecx=game` is **never** used.

| Step | VA | this-ptr | Role |
|---|---|---|---|
| 1 | `009F2450` | `[0x13CAA90]` if ≠0 | walk `+1040` / `[vtbl+28]`; **no** ctor |
| 2 | `0044C6B0` | — | `mov eax, [0x13B879C]` / `ret` |
| 3 | `[edx+8]` | `eax` from getter | virtual on live `0xE0` |
| 4 | `009ACB10` | same getter | `[ecx+88]` then `009E5250` |
| 5 | `009F2450` | `[0x13CAA90]` if ≠0 | same tally |

`e8.tsv` dest `009ACB10`: `0041602E`, later
script `00CD4232`. First-seen is **this** arm.

`009ACB10`:

```
009ACB10  mov ecx, [ecx+88]
009ACB13  jmp 009E5250
```

`009E5250` first-seen **stores** (list reset, not
a new singleton):

```
009E5274  mov [eax+8], eax      ; node at [obj+20]
009E5279  mov [edx+4], 0
009E5282  mov [eax+12], eax
009E5285  mov [esi+4], 0        ; count
```

`obj` is `[[0x13B879C]+88]`. Type of `+88`
**UNREAD**. **DISPROVEN** as a `+40` cap read
(`0044C6C2-plus40`).

`00416005` itself has **no** stores.

`functions.tsv` has **no** `0x00416005` row. The
scanner folded the body into `00415E85` (`Mem use
log`) after several `ret`s (`00415EE9`,
`00415FC1`…`00416002`). Next real row is
`0x00416056`. Treat `00416005` as its own fn from
the listing.

### `[0x13CAA90]`

`009F2870` can `009F20B0` then
`mov [0x13CAA90], eax`. Whether that pointer is
already live at `004185D9` is **PARTIAL**. If 0,
first `E8` is `0044C6B0`. The virtual is still
the first work.

---

## 3. Not `[0x13B8A54]` — different object

Static ctor (`listing-01200000.txt`):

```
0121A630  mov ecx, 0x13B8A54
0121A635  call 00A38500
```

`e8.tsv` dest `00A38500`: `0121A635`, later sound
`00A01A0C`. Init Game does **not** call it.

`00A38500` stores on **that** BSS object:

```
00A38509  mov [esi], 0x129CF84   ; vtbl
00A38511  mov [esi+4..12], 0
00A3851D  push 20 / 00BFEA0E
00A38527  mov [esi+20], eax      ; list
```

`00416005` never loads `0x13B8A54`.

This-ptr of the named stage:

| VA | Object | When live | Class |
|---|---|---|---|
| `[0x13B879C]` | `0xE0`, vtbl `01232C24`, `+40=0x80000` | `0041852D` `0044C6C2` / `0044C71F` | **PROVEN** |
| `[0x13B8A54]` | BSS, vtbl `0129CF84`, list `+20` | `0121A630` `00A38500` | **PROVEN** other |
| `ecx=game` at `004185D7` | `0x161E8` | `00418DCA` | **DISPROVEN** as body this |

Host names `01232C24` `PlayerManagerVtbl`. Listing
RTTI name **UNREAD**; family **PARTIAL**
(`CDefinitionManager` / `LoadDef` /
`"Game Definition Manager: Compile"` on neighbor
`0044C72B`). Not `0044A3B0` vtbl `01231CD0`.

`0044C72B` has **0** `.text` `E8` (virtual
candidate). Slot `01232C24+8` bytes are **UNREAD**.
Do not treat `game.bin` open as proven on this
`[edx+8]` until rdata is read. `script-bank-open`
places `009B08C0` / GLOBAL / ENGINE on `0044E95C`
inside `0044C72B` — other-bank analog, **PARTIAL**
here.

---

## 4. Host leftover: Note-only

`InitGameStages` (13; test length 13):

```
Init Thing Components          004EE23F   ; Note only
Init Definition Manager        00416005   ; Note only  ← this site
Init Graphics                  00416C8A   ; Note + OpenTextureBank
Init Fonts                     004168DC   ; Note + 009E2C80 / 00419463
Init Subtitled Message         004CDB10   ; Note + 00A39010
```

`EnterGame`:

```
Note(apply, name, "InitGame", name);
```

No `if (name == "Init Definition Manager")`.
`EnsurePlayerManagerSingleton` already **MATCH**
the earlier `0044C6B6` miss. `EnsureDefs()` is a
later lookup helper, not this named apply.

### If we keep Note-only

Named notes **MATCH**. Leftover **on this site**
is `0044C6B0` + `[vtbl+8]` + `009ACB10`.

### If we add the virtual + `009ACB10`

Name + getter + `[vtbl+8]` + list reset **MATCH**
the listed body (slot body still **UNREAD**).
Do **not** implement that as `00A39010` into
`[0x13B8A54]`. Do **not** invent a parser.

---

## 5. Later `004CDB10` `00A39010` is the other object

`00418637` `call 004CDB10` after Init Fonts.

```
004CDB41  mov ecx, 0x13B8A54
004CDB46  call 00A39010          ; fill enum list +20
```

`00A39010` `this=ebx=ecx`: lock `+4`, clear
`+20`, file-stack `0099B7D0`, parse
`00A38E50("enum")`. Host now Notes that register
(`SubtitledSymbolsRegistered`). **PROVEN**
sibling leftover closed as fill, **not** ctor.

| Site | Singleton | Op | Class |
|---|---|---|---|
| `0121A630` | `[0x13B8A54]` | construct `00A38500` | **PROVEN** pre-Init |
| `00416005` | `[0x13B879C]` | `[vtbl+8]` + `009ACB10` | **PROVEN** this stage |
| `004CDB10` | `[0x13B8A54]` | fill `00A39010` | **PROVEN** later sibling |

Same English word “Definition Manager”; two
objects. **DISPROVEN** that Init Definition
Manager constructs or fills `[0x13B8A54]`.

---

## Classification (VAs)

| VA | Role | Class |
|---|---|---|
| `004185D9` | sole caller | **PROVEN** |
| `00416005` | named apply | **PROVEN** on walk |
| `push 1` | enables `009ACB10` | **PROVEN** |
| `ecx=game` | parent thiscall | **LEFTOVER** unused |
| `0044C6B0` / `[0x13B879C]` | work this | **PROVEN** |
| `[edx+8]` | first work | **PROVEN** call; dest **UNREAD** |
| `0044C72B` | compile neighbor | **PARTIAL** as slot |
| `009ACB10` / `009E5250` | first-seen stores (list reset) | **PROVEN** |
| `[0x13CAA90]` / `009F2450` | optional tally | **PARTIAL** live at site |
| `[0x13B8A54]` / `00A38500` / `0121A630` | other ctor | **PROVEN**; **DISPROVEN** here |
| `004CDB10` / `00A39010` | later fill of the other object | **PROVEN** sibling |
| Host named row | present | **MATCH** |
| Host `EnterGame` body | Note only | **LEFTOVER** |
| `00DBDE40` | Oakvale | **DISPROVEN** |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00440000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00980000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-009c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00a00000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-01200000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\functions.tsv`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\proofs\004CDB10-00A39010\README.md`
- `C:\FableCSharp\proofs\004CDB10-host-register\README.md`
- `C:\FableCSharp\proofs\0044C6C2-plus40\README.md`
- `C:\FableCSharp\proofs\0044C6B6-host-ensure\README.md`
- `C:\FableCSharp\proofs\morph-first\README.md`
