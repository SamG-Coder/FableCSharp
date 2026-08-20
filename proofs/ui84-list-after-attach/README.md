# After `00598A1C`, first `[ui+84]` `node+20`; `00596917` / `7A` replace or append?

Investigation only. No production `src/` edits.

Question: after first-seen Press Start attach `00598A1C`,
what is the first `[ui+84]` `[node+20]`? Is it the
type-10 Press Start root? After `00596917` / `0059697A`,
does that list **replace** or **append**?

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00580000.txt`
(`00595222` / `00596763` / `0059672A` / `00598A1C` /
`00598B90` / `00598FD0` / `00596917` / `0059697A` /
`00595A06` / `0059B5D7` / `0059AF83` / `00599E3F`);
`listing-00400000.txt` (`004292C0`);
`proofs/00595222-first-node/README.md`;
`proofs/slot-0x14-lookup/README.md`;
`proofs/00598A1C-only-e5/README.md`;
`proofs/draw-type10-fork/README.md`;
`src/Fable.Game/EngineLifecycle.cs`
(`AttachFrontendTree`, `BindNewProfileFromArmedTick`,
`CommitNewProfileFromArmedEdit`, `DrawFrontendWidgets`).

Do not re-prove persist Type=10 on PRESS_START /
NEW_PROFILE / MAIN_MENU, the `0xE5` packet at
`00598EE6`, or `vtbl+8` = `00530260` vs `0041AFA0`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| First `[ui+84]` `[node+20]` after `00598A1C`? | **0** (slot key **0**, value null). Walk starts at `[head+8]` = leftmost = smallest key. | **PROVEN** `00598B99` / `00595229` |
| Type-10 Press Start? | **No.** That widget is slot **`0x14`**, later in the same tree. First nonempty value is slot **`0x1`** `UI_FRONTEND_OPTIONS_MENU`. | **DISPROVEN** as first node |
| `00596917` replace or append `[ui+84]`? | **Neither.** Same nodes. `00596763` switches `[ui+32]` / `[ui+152]` / `[ui+156]` to already-built slot `0x17`. | **PROVEN** |
| `0059697A` replace or append `[ui+84]`? | **Neither** the node list. `00595A06` **overwrites slot 0's value** (null → Main Menu). No second tree, no extra key. | **PROVEN** |
| Host `Clear` + `Build` on those attaches | **Replace** of a single persist walk. Not the native map. | **LEFTOVER** |

---

## Verdict

**First `[node+20]` is not type-10 Press Start. Later
attaches keep the same `[ui+84]` tree.**

`[ui+84]` is `map<int, widget*>` (`0059B5D7`
find-or-insert, key at `+16`, value at `+20`).
`00595222` walks it in-order and skips nulls. First-seen
`00598A1C(0)` inserts key **0** then `mov [eax], 0`.
Press Start is stored at **`0x14`**.

`00596917` / `00596763` do **not** factory a new
`[ui+84]` node and do **not** drop the old ones.
`0059697A` / `00595A06` write Main Menu into the
**existing** slot-0 cell. Host
`AttachFrontendTree` `Clear()` + `widgets[0]` is
**LEFTOVER** vs this residency.

| Claim | Status |
| --- | --- |
| `00595222` first node is `[ [ui+84]+8 ]`, value `[node+20]` | **PROVEN** |
| First-seen first `[node+20]` is type-10 `UI_FRONTEND_PRESS_START_MENU` | **DISPROVEN** (it is 0) |
| PRESS_START lives at slot `0x14` after `00598A1C` | **PROVEN** |
| `00596917` replaces `[ui+84]` with NEW_PROFILE | **DISPROVEN** |
| `00596917` appends a second map / extra key | **DISPROVEN** |
| `0059697A` replaces the whole list | **DISPROVEN** |
| `0059697A` appends a new slot-0 node | **DISPROVEN** (key 0 already exists) |
| `00595A06` writes Main Menu into slot 0 `[node+20]` | **PROVEN** |
| Host one-tree `Clear` is native `00596917` / `7A` | **DISPROVEN** leftover |

---

## 1. What “first `[ui+84]` node+20” means

Draw (`listing-00580000.txt` `00595222`–`0059525A`
`ret 8`):

```
00595225  mov eax, [ebx+84]      ; header*
00595229  mov esi, [eax+8]       ; leftmost
0059522C  cmp esi, eax           ; empty
00595230  mov ecx, [esi+20]      ; widget*
          test ecx, ecx
          je 0059524A            ; skip null
          call [vtbl+8]
0059524A  call 004292C0          ; in-order next
          cmp esi, [ebx+84]
```

`004292C0` (`listing-00400000.txt`) is tree increment
(`+4` parent, `+8` left, `+12` right). `0059AF83`
compares `[node+16]` to the key. `0059B5D7` returns
`eax+20` (`&value`). First node is therefore the
**smallest key**, not “the screen we just attached.”

Same first-node rule: `proofs/00595222-first-node`.

---

## 2. After `00598A1C`: first value is 0

First-seen `00598A1C` (`[ebp+124]==0` → `00598B90`).
`xor ebx, ebx`. `esi = ui+84`.

```
00598B99  mov [ebp+108], ebx     ; key 0
          call 0059B5D7
00598BAA  mov [eax], ebx         ; *cell = 0
00598BB7  mov [ebp+108], 0x14
          call 0059B5D7
          call 0041DB1D          ; UI_FRONTEND_PRESS_START_MENU
          mov [ecx], eax         ; slot 0x14 = type-10
```

Arg 0 skips `UI_FRONTEND_MEDIA_PLAYER_ERROR` key
`-1`, so leftmost key is **0**. First
`[node+20]` is **null**. `00595222` skips it.

Press Start is **not** that cell. It is slot
`0x14` (type-10 persist `#620`). First nonempty
in-order value is slot `0x1` (`00598CBB`
`UI_FRONTEND_OPTIONS_MENU`). Slot `0x17`
`UI_FRONTEND_NEW_PROFILE_SCREEN` is already
filled later in the same `00598A1C`
(`00598FD0`).

Tail of `00598A1C` (`[ui+192]==0`):
`00599CAE` looks up `0x14` and
`0059672A` pushes that widget onto
**`[ui+32]`**. `0059672A` has no
`[ui+84]` store and does not write
`[ui+152]` / `[ui+156]`.

---

## 3. After `00596917` / `7A`: same list

### `00596917` — neither replace nor append

```
00596921  push 23                 ; slot 0x17
          lea ebx, [edi+84]
          call 0059B5D7           ; existing cell
          push 0
          push [eax]
          call 00596763
```

`00596763` (`listing` through `0059686A` `ret 8`):

- `005952D8(0)` then `0059B5D7` key **`0x1A`**
  only to `cmp` the incoming widget (audio).
  Slot `0x1A` is already
  `UI_FRONTEND_QUIT_PROMPT` from `00598A1C`.
- `vtbl+192(6)` on the old `[ui+32]` current
- `0059B61C` **push** onto `[ui+32]` (not the map)
- `[ui+156] = new`, `[ui+152] = old`

No `0041DB1D`. No `mov` into a `0059B5D7`
value. Slot `0x17` was factory-built at
`00598FD0`. First `[ui+84]` node is still
slot 0 / null.

### `0059697A` — overwrite slot 0, do not rebuild

`0059697A` itself does not call `00596763`.
It factories via `00595A06`:

```
00596A36  push "UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE"
          call 00595A06           ; ecx = UI, looks up key 0
          call 00595B24           ; label slots
```

`00595A06`: all `0059B5D7` keys in this
function are **0**. After `00598A1C` that
value is 0, so the name/destroy arm is
skipped (`cmp [eax], 0` / `je` factory):

```
00595AA1  call 0059B5D7           ; key 0, existing node
          call 0041DB1D
          mov [edi], eax          ; same [node+20] = Main Menu
          call [vtbl+172]
```

**Same header. Same keys. New widget only
at slot 0.** Slot `0x14` / `0x17` stay.
After this write, the first nonempty
`00595222` call is that Main Menu type-10
— still not Press Start.

Later tick sites (`0059A008` then
`0059A027` / `0059A053`) can
`00596763` the slot-0 widget. That is
again a current-switch, not a list rebuild.

---

## 4. Host leftover

`AttachFrontendTree` (Press Start,
`BindNewProfileFromArmedTick` → `00596917`,
`CommitNewProfileFromArmedEdit` → `0059697A`):

```
_frontendWidgets.Clear()
built = Factory.Build(rootName)
FrontendRootType = built[0].Type
```

Native never clears `[ui+84]` on those
paths. Host `widgets[0]` after Press Start
is the persist root because C# builds
**one** named tree. Native first
`[node+20]` is slot 0 / null.

| Event | Native `[ui+84]` | Host list |
| --- | --- | --- |
| `00598A1C` | fill many keys; first value 0 | **replace** with PRESS_START walk |
| `00596917` | keep; switch current to `0x17` | **replace** with NEW_PROFILE |
| `0059697A` | keep; write Main Menu into key 0 | **replace** with MAIN_MENU |

`DrawFrontendWidgets` walks the current
host list only. Native `00595222` walks
**every** non-null slot. That multi-slot
draw is **LEFTOVER** in the host
(`slot-0x14-lookup`, `draw-type10-fork`).

---

## Do not invent

- First `[node+20]` == type-10 Press Start.
- `[ui+84]` as a single-screen child list
  (`+176` is that list, on the widget).
- `00596763` allocating a second map or
  appending NEW_PROFILE as a new key.
- `0059697A` destroying the `0x14` / `0x17`
  cells.
- Host `Clear` as the native `0xE5` /
  `0x126` attach.

**Proposed (do not apply here):** keep a
slot map; `00595222` walks every non-null
value; `00596917` only switches current;
`0059697A` writes slot 0.
