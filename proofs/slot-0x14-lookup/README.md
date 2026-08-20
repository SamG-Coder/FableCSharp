# `0059B5D7` slot `0x14` lookup: `[ui+84]` map, not a linear table

Investigation only. No production `src/` edits.

Authority: `Fable.exe` `0059B5D7` / `0059AF83` / `0059B559` /
`0059B32A` / `005953E2` / `00595222` / `00598A1C` (`00598EF2`)
(`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00580000.txt`);
`0054E4F0` / `0054E3D0`
(`listing-00540000.txt`);
`src/Fable.Game/EngineLifecycle.cs`
(`FrontendPressStartSlot`, `FrontendWidgetListOffset`,
`InitFrontendUi`, `AttachPressStartWidgets`,
`WriteType10AttachMessage`, `DrawFrontendWidgets`);
`src/Fable.Game/FrontendInputMap.cs`;
`src/Fable.Game/FrontendMessages.cs`
(`PressStartSlot`);
`implementer/frontend/05-input.md`;
`proofs/press-start-e5-attach/README.md`;
`proofs/type10-plus352/README.md`;
`proofs/draw-type10-fork/README.md`;
`tests/Fable.Formats.Tests/EngineLifecycleTests.cs`,
`FrontendInputTests.cs`.

Do not re-prove persist `+224` is 0 on the type-10, type 4 →
action 26, or `0059A238` `0xE5` → `00599D5C`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

---

## Verdict

**`0059B5D7` is `operator[]` on the UI slot map at
`[ui+84]`.** Key is an `int` slot id (`0x14` for Press
Start). Value at **node+20** is the constructed widget*.
It is **not** a flat `widget[0x14]` array and **not**
`frontend.bin` persist.

`vtbl+284` `0054E4F0` does **not** look up slot `0x14`.
`00598A1C` does:

1. `00598EF2` `mov [ebp+108], 0x14`
2. `0059B5D7` → `&cell` (`node+20`)
3. `ecx = [cell]` → type-10 widget*
4. `call [vtbl+284](&wrapper)` → `0054E4F0` writes
   packet* / ctrl* at **widget+352 / +356**

Host analog is **not** a slot map. C# keeps a flat
`_frontendWidgets` list for the **current named tree**.
`FrontendPressStartSlot = 0x14` and
`FrontendWidgetListOffset = 84` are note constants.
`WriteType10AttachMessage` is the analog of
`00598EE6` + slot-`0x14` `vtbl+284`: it patches
`MessageId=0xE5` onto `widgets[0]` when that root is
type 10 with id 0. First-seen id **MATCH**. Layout
and multi-slot residency are **LEFTOVER**.

| Claim | Status |
| --- | --- |
| `0059B5D7` keys `[ui+84]` | **PROVEN** `ecx = UI+84` |
| `[ui+84]` is an int→widget* tree/map | **PROVEN** `0059AF83` / node+16 key / +20 value |
| Slot `0x14` is Press Start type-10 | **PROVEN** `00598BB7` factory + `00598EF2` reuse |
| `0054E4F0` itself indexes slot `0x14` | **DISPROVEN** (caller deref, then `this`=widget) |
| Type-10 `012497E4+284` = `0054E4F0` | **PROVEN** (`05-input.md`, `type10-plus352`) |
| Generic `0122F5D4+284` is this call | **DISPROVEN** (`0052F040` `ret 4`) |
| Host has `[ui+84]` map / `0059B5D7` | **DISPROVEN** (flat list + name attach) |
| `WriteType10AttachMessage` is the attach analog | **MATCH** first-seen id; **LEFTOVER** packet* |
| `FrontendPressStartSlot` / `PressStartSlot` = `0x14` | **MATCH** constants |
| `functions.tsv` `0059B5D7` = `CTattooDef` (180 B) | **DISPROVEN** (fn is `0059B5D7`…`0059B619`; next is `0059B666`) |

---

## 1. What table: `[ui+84]` int→widget* map

UI ctor `005953E2` (`vtbl 012521A8`, size `0xE0`):

```
00595407  lea ecx, [esi+32]
0059540A  call 0059B310          ; different container
00595417  lea ecx, [esi+84]
0059541D  call 0059B32A          ; slot map
```

`00598A1C` this-pointer is the UI singleton. First-seen
arg 0 jumps to `00598B90`:

```
00598A32  mov [ebp+52], esi      ; UI*
…
00598B8D  mov esi, [ebp+52]
00598B93  add esi, 84            ; ecx for every 0059B5D7
```

`0059B5D7` (`ret 4`, `ecx` = map, arg = `&int` key):

```
0059B5DD  mov esi, [ebp+8]       ; &key
0059B5E2  mov edi, ecx           ; map
0059B5E4  call 0059AF83          ; lower_bound
0059B5E9  cmp eax, [edi]         ; header? → insert
0059B5ED  mov ecx, [esi]
0059B5EF  cmp ecx, [eax+16]      ; key vs node+16
0059B5F2  jge 0059B613           ; hit
          call 0059B559          ; insert
0059B614  add eax, 20            ; return &node+20
0059B619  ret 4
```

Find `0059AF83` is a binary-tree walk:

```
eax = [ecx]                      ; header*
ecx = [eax+4]                    ; root
cmp [ecx+16], key
jl  ecx = [ecx+12]               ; right
else candidate = ecx; ecx = [ecx+8]  ; left
```

Node layout:

| Off | Field |
| ---: | --- |
| +4 | parent (header+4 = root) |
| +8 | left / in-order successor start |
| +12 | right |
| +16 | **key** (`int` slot) |
| +20 | **value** (widget*) |

`[map+0]` = header*. `[map+4]` = size
(`0059B35C` `cmp [ebx+4], 0`). That is MSVC
`map<int, widget*>` / `_Tree` `operator[]`: find or
insert, return mapped reference.

Draw `00595222` walks the **same** object in-order:

```
eax = [ebx+84]                   ; header
esi = [eax+8]                    ; leftmost
cmp esi, eax → empty
ecx = [esi+20]                   ; widget*
call [vtbl+8]
call 004292C0                    ; next node
```

Dtor `005954AD` uses the same `lea ebx,[esi+84]` /
`[node+20]` destroy loop. `[ui+32]` is a different
container (`0059B310`) and is **not** this lookup.

`functions.tsv` labels `0x0059B5D7` `CTattooDef|CHeroTitleDef`
and size 180. That merge starts at `0059B666`
`push "CTattooDef"`. Real `0059B5D7` ends `0059B619`.

---

## 2. Slot `0x14` is the Press Start type-10 cell

Factory (same `00598A1C`, before the attach write):

```
00598BA2  push "UI_FRONTEND_PRESS_START_MENU"
00598BB7  mov [ebp+108], 0x14
00598BBE  call 0059B5D7          ; &cell
00598BC3  mov [ebp+108], eax
          call 0041E5F2
00598BD2  call 0041DB1D          ; → 0054E3D0 type 10
00598BDA  mov [ecx], eax         ; *cell = widget*
```

Ctor `0054E3D0` sets `012497E4` / inner `012497BC` and
zeros +352/+356. First-seen `[ui+84][0x14]` is that
type-10.

The same function fills **many** other keys. First-seen
arg-0 subset (not exhaustive after `0x17`):

| Slot | Name |
| ---: | --- |
| `0` | cleared (`mov [eax], ebx`) before Press Start |
| `0x1` | `UI_FRONTEND_OPTIONS_MENU` |
| `0x2` | cleared (no factory at this site) |
| `0x3` | `UI_FRONTEND_EXTRAS_MENU` |
| `0x4` | `UI_FRONTEND_AUDIO_OPTIONS_MENU` |
| `0x5` | `UI_FRONTEND_SCREEN_VIDEO_OPTIONS_PC` |
| `0x7` | `UI_FRONTEND_PROFILES_MENU` |
| `0x8` | `UI_FRONTEND_PROFILE_SAVED_GAMES_MENU` |
| `0x9` | `UI_FRONTEND_CREDITS_MENU` |
| `0xA` | `UI_FRONTEND_DELETE_PROFILE_MENU` |
| `0xC` | `UI_FRONTEND_NO_PROFILES_MENU` |
| `0xF` | `UI_FRONTEND_INVALID_PROFILE_MENU` |
| `0x10` | `UI_FRONTEND_INVALID_SAVE_MENU` |
| `0x13` | `UI_FRONTEND_PROFILE_ALREADY_EXISTS_MENU` |
| **`0x14`** | **`UI_FRONTEND_PRESS_START_MENU`** |
| `0x15` | `UI_FRONTEND_SCREEN_PROFILES_FOR_DELETE_PC` |
| `0x16` | `UI_FRONTEND_SCREEN_REDEFINE_KEYS_PC` |
| `0x17` | `UI_FRONTEND_NEW_PROFILE_SCREEN` |

Arg ≠ 0 also factories `UI_FRONTEND_MEDIA_PLAYER_ERROR`
into key `-1` (`[ebp+92]`). First-seen `00598A1C(0)`
skips that.

Later `00596917` only **switches** to slot `0x17`. It
does not write `0xE5` and does not call `vtbl+284`.

---

## 3. How `vtbl+284` `0054E4F0` gets that widget

After `INVALID_SAVE` (slot `0x10`), still in `00598A1C`:

```
00598EC3  push 16
00598EC5  call 00BFEA1A          ; packet
00598ED1  call 0042BE50          ; [packet]=0
00598EDE  call 0042AA29          ; wrapper {packet*, ctrl*}
00598EE6  mov [eax], 0xE5        ; packet[0]
00598EEC  lea eax, [ebp+108]
00598EEF  push eax
00598EF0  mov ecx, esi           ; UI+84
00598EF2  mov [ebp+108], 0x14
00598EF9  call 0059B5D7          ; &cell for key 0x14
00598EFE  mov ecx, [eax]         ; widget* = *cell
00598F00  mov eax, [ecx]         ; vtbl
00598F02  lea edx, [ebp-56]
00598F05  push edx               ; &wrapper
00598F06  call [eax+284]
```

Displacement **284** is decimal (same listing style as
widget `+352`). Slot index `284/4 = 71`. Type-10 vtbl
`012497E4+284` = `01249900` → `0054E4F0`. Generic
`0122F5D4+284` = `0052F040` `ret 4` is a different
class and is **not** this `ecx`.

`0054E4F0` (`ecx` = widget already):

```
0054E4F0  mov eax, [esp+4]       ; &wrapper
0054E4F5  mov ebx, [eax]         ; packet*
0054E4F9  mov edi, [eax+4]       ; ctrl*
…
0054E530  mov [esi+352], ebx
0054E536  mov [esi+356], edi
```

No `[ui+84]`, no key `0x14`, no name compare. The map
lookup is entirely the caller. `e8.tsv` has **no**
direct `E8` to `0054E4F0`; first-seen indirect is
`00598F06` only.

Some notes write type-10 slot VA as `01249800`
(`012497E4+0x1C`). That arithmetic is **DISPROVEN**.
Identity of the **function** is still **PROVEN** from
the type-10 vtbl and this call.

---

## 4. Host analog

| Native | Host | Status |
| --- | --- | --- |
| `[ui+84]` map | `FrontendWidgetListOffset = 84` used in draw **notes** only | constant **MATCH**; no map |
| `0059B5D7(0x14)` | `FrontendPressStartSlot` / `FrontendMessages.PressStartSlot` = `0x14` | constant **MATCH**; no lookup |
| `*cell = 0041DB1D(...)` | `AttachFrontendTree(PRESS_START)` → `_frontendWidgets` | **LEFTOVER** (one tree, not all slots) |
| `00598EE6` + `vtbl+284` | `WriteType10AttachMessage` | **MATCH** id `0xE5` on type-10 root |
| packet* at +352 | `FrontendWidget.MessageId` | **LEFTOVER** layout |
| `00595222` walk every `[node+20]` | `DrawFrontendWidgets` walks current list | **LEFTOVER** multi-slot |

`WriteType10AttachMessage`:

```
Note 00598EE6 slot 0x14 vtbl+284 0054E4F0 +352 0xE5
if widgets.Count == 0 → return
root = widgets[0]
if root.Type != 10 || root.MessageId != 0 → return
widgets[0].MessageId = 0xE5
```

Called from `AttachPressStartWidgets` after
`InitFrontendUi` / `00598A1C` notes. The host finds
the type-10 by **building that named tree** and taking
index 0, not by key `0x14`. Other `00598A1C` slots stay
unconstructed until a later `AttachFrontendTree`.

`InitFrontendUi` still notes `0052F040 ret 4` on
`vtbl+284` before the attach note. That generic no-op
is **LEFTOVER** for this type-10; the attach note names
`0054E4F0`.

Do not add a C# `map<int, widget*>` unless a later
proof needs simultaneous residency of slot `0x14` and
`0x17`. First-seen Press Start only needs the type-10
root and the attach patch.

---

## Sources

- `listing-00580000.txt` (`0059B5D7`, `0059AF83`, `005953E2`,
  `00595222`, `00598A1C` / `00598EF2`)
- `listing-00540000.txt` (`0054E3D0`, `0054E4F0`)
- `implementer/frontend/05-input.md`
- `EngineLifecycle.cs` / `FrontendInputMap.cs` / `FrontendMessages.cs`
- `proofs/press-start-e5-attach/README.md`
- `proofs/type10-plus352/README.md`
- `proofs/draw-type10-fork/README.md`
