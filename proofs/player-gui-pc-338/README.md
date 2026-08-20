# `PLAYER_GUI_PC` persist `+0x338`: `CPlayerGuiDef` i32, not first HUD dest

Investigation only. No production `src/` / `tests/` edits.

Do **not** treat `0x338` as a field on the
`CPlayerGui` **instance**. That object's
**size** is `0x338`. Do **not** treat file
blob offset `0x338` as the runtime field.
Do **not** treat Init Player Interface
`004473A0` (`PlayerInterface.cs`, size
`0x898`) or `FrontendLayout` as this
object. Do **not** start Oakvale /
`00DBDE40` / `Q_NewOakValeIntro`.
Do **not** invent first-seen HUD dest,
orbs, MiniMap, or `CDraw*` names for
this dword.

Question: what is `PLAYER_GUI_PC`
persist `+0x338`? First-seen after
Leave? Does first Present draw HUD?
Native skip?

Status words: **PROVEN** / **PARTIAL** /
**UNREAD** / **DISPROVEN** / **LEFTOVER** /
**MATCH**.

Authority: dump `Fable.exe`
`listing-00440000.txt` `00462F93` /
`00459BB6` / `00459E24` / `004736C4` /
`00473E87` / `0046CE28` / `0046D67A` /
`00431102` / `0044C72B`;
`listing-00480000.txt` `00487FB0` /
`00487FC3`;
`listing-00400000.txt` `0043B570` /
`0043A380` / `00435000` / `00435070`;
`vtbl.tsv` `0x012352DC` slot 18
`004736C4`;
`rtti.txt` `CPlayerGuiDef` /
`CPlayerGui`;
`xrefs.tsv` `"PLAYER_GUI"` `0x01235F20`
/ `"PLAYER_GUI_PC"` `0x0123173C`;
compiled `game.bin` `#7075`
`PLAYER_GUI_PC` raw **18148**;
host `EngineLifecycle.cs`
`PlayerGuiObjectSize` / `CreatePlayers`
/ `ApplyDisplayCamera` /
`PlayerGuiReady`;
`PlayerInterface.cs`;
`FrontendLayout.cs`;
siblings `proofs/hud-after-leave`,
`proofs/hud-first-present-skip`,
`proofs/hud-first-present-gate`,
`proofs/init-gui-0043A380`.

Listing displacements are **decimal**
(`[esi+824]` = `0x338`). Size `0x338` =
824.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Persist object? | `CPlayerGuiDef`, factory `00462F93` `00BFEA1A(0xAB4)` `jmp 00459BB6`, vtbl `012352DC` | **PROVEN** |
| Compiled name? | `"PLAYER_GUI_PC"` `#7075` type `PLAYER_GUI` raw 18148 | **PROVEN** |
| Runtime `+0x338` (`+824`)? | persist i32 via `004736C4` `lea eax,[esi+824]` `call 00431102` | **PROVEN** type |
| Lionhead field name / file CRC? | not recovered (nested `00466A47` at `+336` before this dword) | **UNREAD** |
| File blob offset `0x338`? | packed stream, unaligned; **not** the runtime field | **DISPROVEN** as dest |
| Field on `CPlayerGui` instance `+0x338`? | **No.** instance **size** is `0x338`; last ctor write `+816` | **DISPROVEN** |
| Unique `.text` `push 0x338`? | **One** site: `00487FC3` instance alloc | **PROVEN** |
| First-seen after Leave? | def already compiled; instance ctor Create Players `00487FB0` | **PROVEN** |
| Init GUI constructs it? | **No.** `0043A380` reset on `[0x13B8790]` | **DISPROVEN** |
| First Present draw HUD? | **No.** overlay / interface miss; dest empty | **PROVEN** skip |
| Native skip vs host hide? | native would not enqueue; dest **MATCH** empty | **MATCH**; hide **DISPROVEN** |
| `HUD_ORB_*` at this dword? | those CStrings sit **later** in the 18148 blob (~`0x29ED`) | **DISPROVEN** as `+0x338` |
| `PlayerInterface.cs`? | `004473A0` size `0x898` vtbl `01231BDC` `game+32` | **DISPROVEN** |
| `FrontendLayout`? | frontend dest; Leave already tore type-10 | **DISPROVEN** |

**Answer:** persist `+0x338` is an
**i32 on `CPlayerGuiDef`**. It is **not**
the `CPlayerGui` instance (that object's
size is `0x338`). First Present after
Leave is a **native empty skip**, not HUD.

---

## Verdict

Two different `0x338`s share the hex:

| Object | VA / size | `0x338` means |
|---|---|---|
| `CPlayerGui` singleton `[0x13B8790]` | alloc `00487FC3 push 0x338` vtbl `0123177C` | **instance size** 824 |
| `CPlayerGuiDef` `[0x13B878C]` | size **`0xAB4`** vtbl `012352DC` | **field** `+824` persist i32 |

`PLAYER_GUI_PC` persist is the def.
Slot 18 `004736C4` writes `+824` with
`00431102` (CRC skip `00404500`, mode 2
`0040FE60` four bytes). Copy `0046CE28`
copies the same dword. Size getter
`00459E24` returns `0xAB4`, not `0x338`.

First HUD Present does **not** read this
dword. `00435000` / `00435070` miss the
player Thing. `009DA9F0(1)` empty →
`009DB6E6` no DIP. `HUD_ORB_*` /
`MINIMAP_*` / tutorial `TEXT_QST_*`
strings live **later** in the compiled
blob. Binding them to `+0x338` or to
first Present is **DISPROVEN**.

---

## Timeline (no-save New Game)

```
0044C72B  register "PLAYER_GUI" factory 00462F93
          009B08C0 compile game.bin
            PLAYER_GUI_PC → CPlayerGuiDef 0xAB4
              persist 004736C4  +824 i32
0042F2A2  Leave frontend              // type-10 dest gone
0042F491  Init Game
  "Init Player Interface" 004473A0    // 0x898, NOT persist +0x338
  "Create Players"        004166A8
    0048A210
      00487FB0
        push 0x338  00BFEA1A          // INSTANCE size
        0043B570 vtbl 0123177C
        004195AF [0x13B8790]
  0049F180
    "Init GUI" 0043A380               // reset, not ctor
later WorldFrame>1
  00435F70 jmp 00435530 Present
    00435000  00487DD0 miss → skip 00639E40
    00435070  00487DC0 miss → skip 0057B43F
    009DA9F0(1) empty dest
```

---

## 1. Persist object is `CPlayerGuiDef`

`0044C72B` `listing-00440000.txt`:

```
0044CFBC  push "PLAYER_GUI"           ; 0x01235F20
0044CFDB  mov  [ebp-16], 0x462F93    ; factory
0044CFE2  call 009B0AC0               ; Add Def Class
```

Factory:

```
00462F93  push 0xAB4
00462F98  call 00BFEA1A
          test eax, eax
          je   00462FA9
          mov  ecx, eax
          jmp  00459BB6               ; ctor vtbl 012352DC
```

Ctor `00459BB6` `mov [esi], 0x12352DC`.
Size `00459E24` `mov eax, 0xAB4; ret`.
RTTI `0x0137760C` `CPlayerGuiDef`.

`vtbl.tsv` `0x012352DC`:

| Slot | VA | Role |
|---|---|---|
| 0 | `00459E2A` | dtor |
| 18 | `004736C4` | persist |
| 19 | `0046CE28` | copy |
| 20 | `00459E24` | size `0xAB4` |

Compiled `#7075` name `PLAYER_GUI_PC`
raw **18148** is the packed stream, not
the `0xAB4` runtime object. Def bind
`[0x13B878C]` is Create Players ctor
`0043B570` (`0099EBF0` /
`"PLAYER_GUI_PC"` / `0043FF30`). Path
CString at def `+0xA94` (`+2708`) is a
**different** field (`0099E4B0` in the
same ctor; `PlayerGuiGraphPathOffset`).

---

## 2. Runtime field `+0x338` = persist i32

`004736C4` (slot 18), listing decimal
`+824`:

```
00473E4F  lea  eax, [esi+808]
          call 00431102
00473E5D  lea  eax, [esi+812]
          call 00431102
00473E6B  lea  eax, [esi+816]
          call 00431102
00473E79  lea  eax, [esi+820]
          call 00431102
00473E87  lea  eax, [esi+824]         ; +0x338
          call 00431102
00473E95  lea  eax, [esi+828]
          call 00431102
00473EA3  lea  eax, [esi+832]
          call 00431102
00473EB1  lea  eax, [esi+836]
          call 004568BC               ; CString
```

`00431102`: push type tag `0x122D70E`,
`00404500` skips the 4-byte file CRC,
mode 2 (`[esi+24]==2`) `0040FE60` copies
four payload bytes. **PROVEN i32.**

Copy `0046CE28`:

```
0046D67A  mov eax, [ebp+824]
0046D680  mov [ebx+824], eax
```

Ctor `00459BB6` does **not** store
`+824` (scalar gap between vector inits).
Default after `00BFEA1A` is **UNREAD**.
First-seen **file** value is **UNREAD**:
field index 144 in `004736C4`, and a
`00466A47` vector at `+336` sits
**before** this dword in the stream.

`listing-00400000.txt` has **no**
`38 03 00 00` (`+824`) on `CPlayerGui`
(`0043xxxx`). Def readers in that range
use other offsets (`+80`…`+83` colour
bytes, `+1252`, `+2044`, `+2316`,
`+0xA94`). First Present does not take
this i32.

Lionhead name of the CRC is **UNREAD**.
Do not label it Health / Will / Orb.

---

## 3. File blob `+0x338` is not that field

`game.bin` `PLAYER_GUI_PC` hdr
`01 00 01`, then packed CRC+payload.
Hex at file `+0x330`:

```
00 23 CE 23 B3 E0 15 00 00 B3 A5 87 C5 …
```

Unaligned. Early stream is `+60` i32
then two f32s (`1.0`, `255.0` on PC)
then colours / CString `CCCPt` at
file `~0x4F`. Runtime `+824` is **not**
file offset `0x824` / `0x338`.

Later packed CStrings (`HUD_ORB_QUEST_CORE`
at `~0x2A89`, MiniMap names, tutorial
`TEXT_QST_*`) are **other** persist
fields. **DISPROVEN** as `+0x338`.

---

## 4. Instance size `0x338` is the other object

Unique `.text` `68 38 03 00 00` /
`push 0x338`: **one** hit
`listing-00480000.txt`:

```
00487FB0  mov  esi, ecx               ; CPlayer
          call 00449700
          cmp  edi, eax
          jne  00488010
00487FC3  push 0x338
00487FC8  call 00BFEA1A
          …
00487FEE  call 0043B570               ; CPlayerGui vtbl 0123177C
00487FFE  call 004195AF               ; [0x13B8790]
```

Ctor last write `0043B905 mov [esi+816], 1`.
Nothing at instance `+0x338`. RTTI
`CPlayerGui`. Host
`PlayerGuiObjectSize=0x338` names **this**
alloc, not the def field.

---

## 5. First Present after Leave: native skip

Leave `0042F2A2` already ended frontend
`0042DF9E`. `FrontendLayout` dest is
**not** live.

`00435F70 jmp 00435530`:

```
00435000  mov ecx, [ecx+12]
          call 00449960
          call 00487DD0              ; +44 jmp 00A01B50
          test eax, eax
          je   0043505E              ; miss → ret
          … call 00639E40            ; not taken

00435070  mov eax, [0x13B86A0]
          mov ecx, [eax+28]
          call 00449970
          call 00487DC0
          test eax, eax
          je   004350C9              ; miss → ret
          … call 0057B43F            ; not taken
```

No-save first Present: no player Thing.
`009DA9F0(1)` `[+16020]==[+16024]` →
`009DB6E6` no DIP
(`hud-first-present-gate`, `hud-after-leave`,
`hud-first-present-skip`).

Native skip is **MATCH** empty dest, not
a host hide of HUD pixels. Inventing
orbs / MiniMap / childhood chrome on
this frame is **DISPROVEN**.

---

## 6. Host

`PlayerInterface.cs`: ctor `004473A0`,
size `0x898`, vtbl `01231BDC`,
`game+32`. **Zero** GUI / `0x338` /
`0123177C` / `[0x13B8790]` fields.
`DisplayPlayerInterfaceFn=0x00435070`
is the Present **Thing** interface
lookup, not that class.

`CreatePlayers` Notes:

```
00487FB0 alloc 0x338
0043B570 vtbl 0123177C PLAYER_GUI_PC
004195AF [0x13B8790]
```

Site **MATCH**; live `CPlayerGuiDef`
/`+824` **LEFTOVER**.

`InitCharactersAndQuests` Notes
`0043A380 reset PLAYER_GUI_PC` then
`PlayerGuiReady=true`. Flag is **not**
the persist i32 and **not** a dest
(`issue-17-verify`).

`ApplyDisplayCamera` Notes overlay /
interface skip and
`009DA9F0(1) [+16020] empty dest`.
**MATCH** first-seen skip.
`DisplayFlushShouldDip(0,0)` is a
stand-in; on this no-save Present the
native producers are also idle.

---

## Gap

| Native | Host | Class |
|---|---|---|
| `CPlayerGuiDef+824` i32 persist | none | **LEFTOVER** |
| `CPlayerGui` `0x338` singleton | Notes only | **LEFTOVER** body |
| First Present skip | `ApplyDisplayCamera` skip Notes | **MATCH** |
| `PlayerGuiReady` without meters | flag after `0043A380` | **LEFTOVER** |
| `PlayerInterface.cs` as persist `+0x338` | `0x898` input | **DISPROVEN** |
| File `+0x338` as the field | n/a | **DISPROVEN** |

---

## Classification (VAs)

| VA | Role | Class |
|---|---|---|
| `00462F93` | `PLAYER_GUI` factory `0xAB4` | **PROVEN** |
| `00459BB6` | `CPlayerGuiDef` ctor vtbl `012352DC` | **PROVEN** |
| `004736C4` | persist slot 18 | **PROVEN** |
| `00473E87` | `+824` / `+0x338` `00431102` | **PROVEN** i32 |
| `0046D67A` | copy `+824` | **PROVEN** |
| `00431102` | persist i32 | **PROVEN** |
| `00487FC3` | unique `push 0x338` instance size | **PROVEN** |
| `0043B570` | `CPlayerGui` ctor | **PROVEN**; not persist `+0x338` |
| `0043A380` | Init GUI reset | **DISPROVEN** as ctor |
| `004473A0` / `PlayerInterface.cs` | input `0x898` | **DISPROVEN** as this field |
| `00435000` / `00435070` | first Present HUD | **PROVEN** skip |
| `00DBDE40` | Oakvale HUD feeder | **DISPROVEN** here |

---

## Open

| Item | Class |
|---|---|
| File CRC / Lionhead name of def `+824` | **UNREAD** |
| First-seen i32 payload on `PLAYER_GUI_PC` vs `PLAYER_GUI_DEFAULT` | **UNREAD** (walk `00466A47` at `+336`) |
| First `0043xxxx` reader of `[def+824]` | **UNREAD** (none on first Present) |
| Names of `0065431D` / `0064xxxx` instance children vs `CDrawBase@NPlayerGui` | **UNREAD** (instance, not this dword) |

---

## Sources

- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00440000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00480000.txt`
- `C:\FableCSharp\assembly\exe\00-index\vtbl.tsv`
- `C:\FableCSharp\assembly\exe\00-index\rtti.txt`
- `C:\FableCSharp\assembly\exe\00-index\xrefs.tsv`
- `C:\FableCSharp\assembly\compiled-defs\game\entries.tsv`
- `C:\FableCSharp\src\Fable.Game\PlayerInterface.cs`
- `C:\FableCSharp\src\Fable.Game\FrontendLayout.cs`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\proofs\hud-after-leave\README.md`
- `C:\FableCSharp\proofs\hud-first-present-skip\README.md`
- `C:\FableCSharp\proofs\hud-first-present-gate\README.md`
- `C:\FableCSharp\proofs\init-gui-0043A380\README.md`
