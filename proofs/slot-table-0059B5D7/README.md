# `0059B5D7` slot lookup: table base, stride, `0x14` vs `0x17`

Investigation only. No production `src/` edits.

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00580000.txt`
(`0059B5D7` / `0059AF83` / `0059AFAB` / `0059B559` /
`0059B1BE` / `00598BB7` / `00598A1C` / `00596917` /
`00595222`);
`listing-00400000.txt` (`004292C0`);
`functions.tsv` (`0x0059B5D7`);
`proofs/slot-0x14-lookup/README.md` (cited; directory
was empty this pass — listing is the dump);
`proofs/type10-who-subscribes/README.md`;
`proofs/00598A1C-only-e5/README.md`;
`proofs/type10-plus352-writers/README.md`;
`proofs/type39-keyredefiner/README.md`;
`src/Fable.Game/EngineLifecycle.cs`
(`FrontendWidgetListOffset`, `FrontendWidgetSlotOffset`,
`FrontendPressStartSlot`, `FrontendNewProfileSlot`).

Do not re-prove type 4 → action 26, persist `+224` is 0 on
Press Start, `00598EE6` packet `0xE5` + `vtbl+284`, or
`0059A238` consume.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **MATCH**.

---

## Verdict

`0059B5D7` is **not** `base + slot * stride`. It is
`operator[]` on the **int → widget\*** map embedded at
**`ui+84`**.

| Piece | Value | Status |
| --- | --- | --- |
| Table `this` | `ui+84` (`lea` / `add esi, 84`) | **PROVEN** |
| `[ui+84]` | header node\* (end / walk sentinel) | **PROVEN** |
| `[ui+88]` | map size (`inc [edi+4]` on insert) | **PROVEN** |
| Array stride | **none** | **DISPROVEN** dense `slot*4` / `slot*20` |
| Node alloc | **24** (`0059AFAB` `push 24`) | **PROVEN** |
| Key | `node+16` | **PROVEN** |
| Value / widget\* cell | `node+20` (`add eax, 20`) | **PROVEN** |
| Slot `0x14` | `UI_FRONTEND_PRESS_START_MENU` | **PROVEN** |
| Slot `0x17` | `UI_FRONTEND_NEW_PROFILE_SCREEN` | **PROVEN** |
| Same cell for `0x14` and `0x17` | no — different keys | **DISPROVEN** |

**Who stores widget\* at slot `0x14` after `0041DB1D`?**

**`00598A1C` at `00598BDA`** `mov [ecx], eax`.
`ecx` is the cell\* `0059B5D7` returned for key `0x14`
(`00598BC3` `mov [ebp+108], eax`). The factory result
is the store. `0059B5D7` itself only inserts a **0**
value if the key is missing.

The later `00598EF9` lookup of `0x14` **reads** that
widget\* for `vtbl+284`. It does not write widget\*.

| Claim | Status |
| --- | --- |
| `0059B5D7` returns `&node+20` | **PROVEN** |
| Missing key inserts `{key, 0}` then returns that cell | **PROVEN** |
| `00598BDA` is the first-seen `0x14` widget\* store | **PROVEN** |
| `0059B5D7` stores the factory widget\* | **DISPROVEN** |
| `00598EF9` overwrites slot `0x14` widget\* | **DISPROVEN** (`mov ecx, [eax]` then `vtbl+284`) |
| `00596917` factories / stores slot `0x17` | **DISPROVEN** (read + `00596763` / `00851700`) |
| `functions.tsv` `0059B5D7` is 180 bytes / `CTattooDef` | **DISPROVEN** (fn is `0059B5D7`…`0059B619` `ret 4`) |

---

## 1. `0059B5D7` — keyed cell, not an array

Entire function (`listing-00580000.txt`):

```
0059B5D7  push ebp
0059B5D8  mov ebp, esp
0059B5DA  push ecx
0059B5DB  push ecx
0059B5DC  push esi
0059B5DD  mov esi, [ebp+8]       ; &key
0059B5E0  push edi
0059B5E1  push esi
0059B5E2  mov edi, ecx           ; this = map at ui+84
0059B5E4  call 0059AF83          ; lower_bound
0059B5E9  cmp eax, [edi]         ; == header? → miss
0059B5EB  je  0059B5F4
0059B5ED  mov ecx, [esi]
0059B5EF  cmp ecx, [eax+16]      ; key vs node+16
0059B5F2  jge 0059B613           ; equal (lower_bound hit)
0059B5F4  mov ecx, [esi]
0059B5F6  and [ebp-4], 0         ; pair.value = 0
0059B5FA  mov [ebp-8], ecx       ; pair.key
          … push &pair …
0059B60C  call 0059B559          ; insert
0059B611  mov eax, [eax]         ; node*
0059B613  pop edi
0059B614  add eax, 20            ; return &node+20
0059B617  pop esi
0059B618  leave
0059B619  ret 4
```

Find (`0059AF83`) walks `[header+4]` (root) with
`node+16` as the int key, `+8` left, `+12` right:

```
0059AF83  mov eax, [ecx]         ; header*
0059AF85  mov ecx, [eax+4]       ; root
          …
0059AF92  cmp [ecx+16], edx      ; node.key ? key
0059AF95  jl  0059AF9E           ; key > node → right
0059AF97  mov eax, ecx
0059AF99  mov ecx, [ecx+8]       ; left
0059AF9C  jmp 0059AFA1
0059AF9E  mov ecx, [ecx+12]      ; right
```

Insert alloc (`0059AFAB`) is **24** bytes and copies the
8-byte pair to `+16`/`+20`. Link (`0059B1BE`) zeros
`+8`/`+12`, sets parent at `+4`, `inc [map+4]`.

**Node**

| Off | Field |
| ---: | --- |
| `+4` | parent |
| `+8` | left |
| `+12` | right |
| `+16` | key (slot id) |
| `+20` | value (widget\*) |

**Map at `ui+84`**

| Off | Field |
| ---: | --- |
| `ui+84` | header\* |
| `ui+88` | size |

`+20` is the **value offset**, not a stride. A dense
read `*(ui+84) + 0x14*4` or `+ 0x14*20` is
**DISPROVEN**.

`functions.tsv` `0x0059B5D7` size 180 with callees
`CTattooDef` / `CHeroTitleDef` is a **bad boundary**.
Next real fn is `0059B61C`. Do not treat those strings
as slot-table types.

---

## 2. Call-site `this` is always `ui+84`

First-seen populate (`00598A1C`, arg 0 skips media
error and lands here):

```
00598B8D  mov esi, [ebp+52]      ; ui (saved this)
00598B90  lea eax, [ebp+108]
00598B93  add esi, 84            ; map
00598B96  push eax
00598B97  mov ecx, esi
00598B99  mov [ebp+108], ebx     ; key 0 (ebx=0)
00598B9C  call 0059B5D7
00598BAA  mov [eax], ebx         ; *slot0 = 0
```

Then the Press Start factory:

```
00598BA2  push "UI_FRONTEND_PRESS_START_MENU"
00598BB1  lea eax, [ebp+108]
00598BB4  push eax               ; &key
00598BB5  mov ecx, esi           ; ui+84
00598BB7  mov [ebp+108], 0x14
00598BBE  call 0059B5D7          ; cell*
00598BC3  mov [ebp+108], eax     ; save cell* (overwrites key)
00598BCB  call 0041E5F2
00598BD2  call 0041DB1D          ; widget*
00598BD7  mov ecx, [ebp+108]
00598BDA  mov [ecx], eax         ; *cell = widget*
```

Same `this` on New Profile switch (`00596917`):

```
00596921  push 23                ; 0x17
00596923  pop esi
00596927  lea ebx, [edi+84]
0059692D  mov [ebp-4], esi
00596930  call 0059B5D7
00596937  push [eax]             ; already-stored widget*
0059693B  call 00596763
```

`00595AD9` / `00595CAB` also `add ecx, 84` then
`0059B5D7`.

Draw / tick walk the **same** header, inorder, not a
second list:

```
00595222  mov eax, [ebx+84]      ; header*
00595229  mov esi, [eax+8]       ; begin = header.left
0059522C  cmp esi, eax
          je  empty
00595230  mov ecx, [esi+20]      ; widget*
          call [eax+8]
0059524B  call 004292C0          ; tree successor
00595252  cmp esi, [ebx+84]
```

`004292C0` is parent/`+12` right / `+8` left successor.
Host comment “circular list at `[ui+84]`” is the walk
shape, not a ring of widgets. Each node is one **slot**.

---

## 3. Slot `0x14` vs `0x17`

Same map. Different keys. Different cells.

`00598A1C` first-seen factory fill (arg 0). Pattern is
`mov [ebp+108], key` / `0059B5D7` / `0041DB1D` /
`mov [ecx], eax` except the two zero-only keys:

| Key | Name | Store after factory |
| ---: | --- | --- |
| `0` | (cleared) | `00598BAA` `*cell=0` — no factory |
| `0x14` | `UI_FRONTEND_PRESS_START_MENU` | **`00598BDA`** |
| `0x7` | `UI_FRONTEND_PROFILES_MENU` | `00598C1B` |
| `0xA` | `UI_FRONTEND_DELETE_PROFILE_MENU` | `00598C5C` |
| `0x8` | `UI_FRONTEND_PROFILE_SAVED_GAMES_MENU` | `00598C9D` |
| `0x1` | `UI_FRONTEND_OPTIONS_MENU` | `00598CDE` |
| `0x4` | `UI_FRONTEND_AUDIO_OPTIONS_MENU` | `00598D1F` |
| `0x2` | (cleared) | `00598D44` `*cell=0` — no factory |
| `0x3` | `UI_FRONTEND_EXTRAS_MENU` | `00598D74` |
| `0x9` | `UI_FRONTEND_CREDITS_MENU` | `00598DB5` |
| `0x13` | `UI_FRONTEND_PROFILE_ALREADY_EXISTS_MENU` | `00598DF6` |
| `0xC` | `UI_FRONTEND_NO_PROFILES_MENU` | `00598E37` |
| `0xF` | `UI_FRONTEND_INVALID_PROFILE_MENU` | `00598E78` |
| `0x10` | `UI_FRONTEND_INVALID_SAVE_MENU` | `00598EB9` |
| `0x15` | `UI_FRONTEND_SCREEN_PROFILES_FOR_DELETE_PC` | `00598F43` |
| `0x16` | `UI_FRONTEND_SCREEN_REDEFINE_KEYS_PC` | `00598F84` |
| `0x5` | `UI_FRONTEND_SCREEN_VIDEO_OPTIONS_PC` | `00598FC5` |
| `0x17` | `UI_FRONTEND_NEW_PROFILE_SCREEN` | **`00599006`** |
| `0x18` | `UI_FRONTEND_OPTIONS_SUB_MENU` | `00599047` |
| `0x19` | `UI_FRONTEND_OPTIONS_SCOREBOARD` | `00599088` |
| `0x1B` | `UI_FRONTEND_PROFILE_ERROR_PC` | `005990C9` |
| `0x1A` | `UI_FRONTEND_QUIT_PROMPT` | `0059910A` |

Between `0x10` and `0x15` the **same** function looks
`0x14` up again and does **not** store widget\*:

```
00598EF2  mov [ebp+108], 0x14
00598EF9  call 0059B5D7
00598EFE  mov ecx, [eax]         ; widget* already there
00598F00  mov eax, [ecx]
00598F06  call [eax+284]         ; 0054E4F0 → +352 packet*
```

That is attach, not a second factory. Slot `0x17` is
filled **after** this write. `0x17` never receives the
`0xE5` packet (`00598A1C-only-e5`).

`00596917` only **reads** `0x17` (already stored at
`00599006`) and switches / binds the edit box. No
`0041DB1D`. No `mov [cell], widget*`.

---

## 4. Host leftover

| Native | Host |
| --- | --- |
| `ui+84` map, key `0x14` / `0x17` | `FrontendPressStartSlot` / `FrontendNewProfileSlot` in Notes only |
| `00598BDA` `*cell = 0041DB1D()` | `AttachFrontendTree` appends a flat `_frontendWidgets` list |
| `00595222` inorder `[node+20].vtbl+8` | walk of that list; comment says “circular list” |
| `FrontendWidgetSlotOffset = 20` | value offset **MATCH**es `node+20`; not an index |
| Whole `00598A1C` slot table | first-seen host builds **Press Start only** |

First-seen Press Start root still lands in the only
populated list, so draw/input **MATCH** the `0x14`
widget. The keyed table is **LEFTOVER**. Do not invent
`widgets[0x14]` as a C# index.

---

## Answer

- **Base:** map object at **`ui+84`**. Header\* at
  `[ui+84]`, size at `[ui+88]`.
- **Stride:** **none**. 24-byte tree nodes. Widget\* at
  **`node+20`**. Key at **`node+16`**.
- **`0x14` vs `0x17`:** two keys. Press Start vs New
  Profile. Not adjacent array slots.
- **Store after `0041DB1D`:** **`00598A1C` `00598BDA`**.

---

## Do not invent

- `*(ui+84) + slot * 4` or `* 20`.
- `0059B5D7` writing the factory pointer.
- Slot `0x14` and `0x17` sharing a cell.
- `00596917` / `00595A06` as the first `0x14` store.
- `00598EF9` as a widget\* store.
- `functions.tsv` `CTattooDef` as this helper.
- `[node+20]` as the type-0 factory / `0041AFA0` menu
  (`docs/status` leftover: PRESS_START is type 10).
