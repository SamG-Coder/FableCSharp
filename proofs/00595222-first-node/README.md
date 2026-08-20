# `00595222` first `[ui+84]` node after Press Start attach

Investigation only. No production `src/` edits.

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00580000.txt`
(`00595222` / `00596763` / `0059672A` / `00598A1C` /
`00598B90` / `00599CAE` / `00596917` / `0059697A` /
`00595A06` / `0059B5D7` / `00599E3F` `0059A0C4`);
`listing-00400000.txt` (`004292C0` / `0042E085`);
`src/Fable.Game/EngineLifecycle.cs`
(`FrontendUiDrawFn`, `FrontendWidgetListOffset`,
`FrontendWidgetSlotOffset`, `AttachFrontendTree`,
`BindNewProfileFromArmedTick`, `AttachFrontendMainMenu`);
`proofs/draw-type10-fork/README.md`;
`proofs/00598A1C-only-e5/README.md`;
`implementer/frontend/01-widget-construction.md`.

Do not re-prove persist Type=10 on PRESS_START / NEW_PROFILE /
MAIN_MENU, `0xE5` packet `00598EE6`, or `vtbl+8` =
`00530260` vs `0041AFA0`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

---

## Verdict

**No.** After first-seen Press Start attach `00598A1C`,
the first `[ui+84]` node is **slot key 0** with
`[node+20] == 0`. That is **not** the type-10
`UI_FRONTEND_PRESS_START_MENU` root.

`[ui+84]` is a **slot → widget\*** tree, not “the current
screen.” `00595222` walks it in-order (`[head+8]` then
`004292C0`). `[node+20]` is the map **value**. Press Start
is stored at **slot `0x14`**. First nonempty value is
slot **`0x1`** (`UI_FRONTEND_OPTIONS_MENU`).

`0xE5` → `00596917` → `00596763`: **same list**, no new
`[ui+84]` node. Slot `0x17` was already factory-filled.
Switch rewires `[ui+32]` / `[ui+152]` / `[ui+156]`.

`0x126` → `0059697A` → `00595A06`: **same list**, same
slot-0 node. `00595A06` writes a **new** Main Menu
widget into that existing `[node+20]`.

Host `AttachFrontendTree` `Clear()` + `widgets[0]` as the
sole root is **LEFTOVER** vs this map.

| Claim | Status |
| --- | --- |
| `00595222` is `[ui+84]` in-order walk, `[node+20].vtbl+8`, next `004292C0` | **PROVEN** |
| `[ui+84]` is a key/value tree; `+16` key, `+20` widget\* | **PROVEN** (`0059AF83` / `0059B5D7`) |
| First node is `[head+8]` = leftmost = smallest key | **PROVEN** |
| First-seen `00598A1C` inserts slot **0** then `mov [eax], 0` | **PROVEN** `00598B99` |
| First `[node+20]` after attach is the type-10 Press Start | **DISPROVEN** (it is 0) |
| Type-10 PRESS_START lives at slot **`0x14`** `[node+20]` | **PROVEN** `00598BB7` |
| First nonempty `[node+20]` is slot `0x1` OPTIONS | **PROVEN** |
| `0xE5` `00596763` inserts / replaces a `[ui+84]` node | **DISPROVEN** |
| `0x126` `00595A06` allocates a second `[ui+84]` tree | **DISPROVEN** (writes slot 0) |
| Host one-tree `Clear` + `widgets[0]` is the native map | **DISPROVEN** leftover |

---

## 1. Walk (`00595222`)

`0042E085`: `00595582` (UI singleton) then
`00595222` (`ret 8`; the two pushes are the
`vtbl+8` args, first-seen 0).

```
00595222  mov ebx, ecx
00595225  mov eax, [ebx+84]      ; head / sentinel
00595229  mov esi, [eax+8]       ; leftmost
0059522C  cmp esi, eax
          je empty
00595230  mov ecx, [esi+20]      ; widget*
          test ecx, ecx
          je next                ; null value: skip
          mov eax, [ecx]
          push 0, 0, 0, arg, arg
          call [eax+8]           ; vtbl+8
0059524A  push esi
          call 004292C0          ; in-order successor
          cmp esi, [ebx+84]
          jne 00595230
```

Same first-node rule on tick `00599E3F` at
`0059A0C4` (`[node+20].vtbl+4`). No
`cmp` of type 10. No “current screen only”
filter. Null values are skipped; every
non-null slot is called.

`004292C0` is tree increment (`+4` parent,
`+8` left, `+12` right). `0059AF83` find
compares `[node+16]` to the key and walks
`+8` / `+12`. `0059B5D7` find-or-insert
returns **`eax+20`** (`&value`).

---

## 2. First-seen attach fills the map

`00598A1C` first-seen (`[ebp+124]==0` at
`00598A46` → `00598B90`). `ebx=0`.
`esi = ui+84`.

```
00598B99  mov [ebp+108], 0
          call 0059B5D7          ; insert key 0
          mov [eax], 0           ; value = null
00598BB7  mov [ebp+108], 0x14
          call 0059B5D7
          call 0041DB1D          ; UI_FRONTEND_PRESS_START_MENU
          mov [ecx], eax         ; slot 0x14 = type-10
```

Then the same helper factories the rest of
the frontend slot table (same list):

| Slot | Name | Value after attach |
| ---: | --- | --- |
| 0 | *(key only)* | **0** |
| `0x1` | `UI_FRONTEND_OPTIONS_MENU` | widget |
| `0x2` | *(zeroed)* | **0** |
| `0x3` | `UI_FRONTEND_EXTRAS_MENU` | widget |
| `0x4` | `UI_FRONTEND_AUDIO_OPTIONS_MENU` | widget |
| `0x5` | `UI_FRONTEND_SCREEN_VIDEO_OPTIONS_PC` | widget |
| `0x7` | `UI_FRONTEND_PROFILES_MENU` | widget |
| `0x14` | `UI_FRONTEND_PRESS_START_MENU` | **type-10** |
| `0x17` | `UI_FRONTEND_NEW_PROFILE_SCREEN` | type-10 |
| `0x1A` | `UI_FRONTEND_QUIT_PROMPT` | widget |

(Other slots `0x8`…`0x1C` same pattern.
`00598D2F` zeros slot `0x2` like slot 0.)

In-order first key is **0**. First
`[node+20]` is **0**, so the first
`00595222` iteration skips. First
**draw** is slot `0x1`, not Press Start.

End of `00598A1C` (`[ui+192]==0`, arg 0):

```
00599CAE  mov [ebp+124], 0x14
          call 0059B5D7
          push [eax]
          call 0059672A          ; push slot 0x14 onto [ui+32]
```

`0059672A` registers `widget+4` via
`0041E5F2` `vtbl+8` and `0059B61C` onto
`[ui+32]`. It does **not** rewrite
`[ui+84]` and does **not** set
`[ui+152]` / `[ui+156]` (those stay 0
from `005958F5` until a later
`00596763`). First-seen does **not**
call `0059899A` (that arm is
`[ui+192]≠0`).

---

## 3. `0xE5` / `0x126`: same list

### `0xE5` → `00596917` → `00596763`

```
00596921  push 23                 ; 0x17
          lea ebx, [edi+84]
          call 0059B5D7           ; existing slot
          push 0
          push [eax]
          call 00596763
```

`00596763`:

- `0059B039` current from `[ui+32]`
- `0059B5D7` slot **`0x1A`** only to
  compare the *new* widget for a sound
- `vtbl+192(6)` on the old current
- `0059B61C` **push** onto `[ui+32]`
  (vector `+16`/`+24`, not the map)
- `[ui+156] = new`, `[ui+152] = old`

No `0041DB1D`. No store into a
`0059B5D7` value. Slot `0x17` already
holds NEW_PROFILE from `00598FD0`.

**Same `[ui+84]` nodes. Same first node
(slot 0, still null).**

### `0x126` → `0059697A` → `00595A06`

```
00596A36  push "UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE"
          call 00595A06
```

`00595A06` looks up **slot 0**
(`[ebp-8]=0`). First-seen that value is
0, so it skips the destroy/name compare
and factories into the **existing**
node:

```
00595A8E  call 0059B5D7           ; key 0
          cmp [eax], 0
          jne already
          call 0041DB1D
          mov [edi], eax          ; slot 0 = Main Menu type-10
          call [eax+172]          ; 0052C730 / 0054E4B0
```

**Same list object. New widget at first
`[node+20]`.** After this, the first
non-null walk *is* a type-10 root — Main
Menu, not Press Start. Slot `0x14`
stays in the map.

`00596763` is not on this first-seen
`0059697A` path. Later tick sites
(`00599DFA` / `00599FB2` / `0059A053`)
can switch slot 0 after `0059899A`.

---

## 4. Host leftover

`AttachFrontendTree` (Press Start, New
Profile, Main Menu):

```
_frontendWidgets.Clear()
built = Factory.Build(rootName)
FrontendRootType = built[0].Type
```

Native never clears `[ui+84]` on those
switches. `widgets[0]` after Press Start
is the type-10 menu because the factory
walk is **one persist tree**. Native
first `[node+20]` is slot 0 / null.

After `0xE5` the host **replaces** the
list with NEW_PROFILE. Native **keeps**
every earlier slot. After `0x126` the
host replaces again; native **fills
slot 0** and keeps `0x14` / `0x17`.

`DrawFrontendWidgets` “first-seen
nonempty node → `0041AFA0`” is **STALE**
(`draw-type10-fork`). First nonempty
native call is slot `0x1` `vtbl+8`, not
a type-0 leaf.

---

## Do not invent

- `[ui+84]` as a single-screen child list
  (`+176` is that list, on the widget).
- First `[node+20]` == Press Start type 10.
- `00596763` allocating a second map.
- `.text` `mov […], 0x126`.
- Host `Clear` as the native `0xE5` /
  `0x126` attach.

**Proposed (do not apply here):** keep a
slot map; `00595222` walks every
non-null value; `0xE5` only switches
current; `0x126` writes slot 0.
