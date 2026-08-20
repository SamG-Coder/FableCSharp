# `00596763` after `0xE5` / `00596917`

Investigation only. No production `src/` edits.

Question: after `0xE5` → `00596917` → `00596763`, what
does the switch write at `[ui+32]` / `[ui+152]` /
`[ui+156]`? Does it keep `[ui+84]` slots including
`0x14`? Host `AttachFrontendTree` `Clear()` leftover?

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00580000.txt`
(`00596763` / `00596917` / `0059672A` / `00598A1C`
`00599CAE` / `00599E3F` `00599ED2` / `0059B61C` /
`0059B039` / `0059AEE5` / `0059B5D7` / `00595422`
`0059545C` / `005958F5` `0059593F`);
`functions.tsv` `0x00596763` / `0x00596917`;
`proofs/00595222-first-node/README.md`;
`proofs/ui84-list-after-attach/README.md`;
`src/Fable.Game/EngineLifecycle.cs`
(`BindNewProfileFromArmedTick`, `AttachFrontendTree`).

Do not re-prove persist Type=10 on PRESS_START /
NEW_PROFILE, the `0xE5` packet at `00598EE6`, or
`005331A0` during factory. Do not start Oakvale.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| `[ui+32]`? | Deque **push_back** of the incoming widget (already-built slot `0x17`). `00596917` passes arg2=`0`, so the Press Start entry is **not** popped. | **PROVEN** `00596801` `0059B61C` |
| `[ui+156]`? | **New** current = that slot-`0x17` widget\*. | **PROVEN** `00596812` |
| `[ui+152]`? | **Old** `[ui+32].back()` = first-seen Press Start from `0059672A`. | **PROVEN** `00596818` |
| Keep `[ui+84]` including `0x14`? | **Yes.** No factory, no store through `0059B5D7`. Slot `0x1A` is read-only (audio `cmp`). | **PROVEN** |
| Host `AttachFrontendTree` `Clear()`? | **LEFTOVER.** Native switch keeps the map. Host drops every slot and rebuilds NEW_PROFILE only. | **DISPROVEN** as native |

---

## Verdict

**`00596763` is a current-stack switch, not a slot-table
rebuild.** After `0xE5` → `00599E3F` → `00596917` it
pushes slot `0x17` onto `[ui+32]`, sets `[ui+156]=new`
and `[ui+152]=old`, and leaves `[ui+84]` (including
Press Start `0x14`) untouched.

Host `BindNewProfileFromArmedTick` → `AttachFrontendTree`
`Clear()` + `Build` is leftover vs that.

| Claim | Status |
| --- | --- |
| `00596917` is the first-seen `0xE5` bind (`[ui+160]≠0`) | **PROVEN** `00599ED2` |
| `00596917` looks up slot `0x17` and calls `00596763(widget, 0)` | **PROVEN** |
| `00596763` `[ui+32]` write is `0059B61C` push_back | **PROVEN** |
| `00596917` pops `[ui+32]` first | **DISPROVEN** (`cmp [ebp+12],0` / `je` skip) |
| `[ui+156] = incoming`, `[ui+152] = old back()` | **PROVEN** |
| First-seen old back is slot `0x14` Press Start | **PROVEN** `00599CAE` / `0059672A` |
| `00596763` / `00596917` write a `[ui+84]` cell | **DISPROVEN** |
| Slot `0x14` is dropped on this path | **DISPROVEN** |
| Host `Clear` is the native `0xE5` attach | **DISPROVEN** leftover |

---

## 1. Path: `0xE5` → `00596917` → `00596763`

Tick `00599E3F` (`listing-00580000.txt`):

```
00599E88  cmp [esi+160], bl
          je 00599F04
…
00599ECA  mov ecx, esi
          mov [esi+160], bl
00599ED2  call 00596917
```

`00596917` (`00596917`–`00596979` `ret`):

```
00596921  push 23                 ; slot 0x17
          pop esi
          lea ebx, [edi+84]
          mov [ebp-4], esi
          call 0059B5D7           ; existing cell
          push 0                  ; arg2 = 0
          push [eax]              ; widget* already in the map
          mov ecx, edi
0059693B  call 00596763
          push 16
          call 00BFEA1A
          call 00851700           ; [ui+96]
          mov [edi+96], eax
          call 00851770           ; edit box
```

No `0041DB1D`. Slot `0x17` was factory-filled in
first-seen `00598A1C` (`00598FD0`
`UI_FRONTEND_NEW_PROFILE_SCREEN`). `functions.tsv`
`0x00596917` callees: `0059B5D7`, `00596763`,
`00BFEA1A`, `0059B5D7`, `00851700`, `00851770`.

---

## 2. What `00596763` writes

`00596763`–`0059686A` `ret 8`. `ecx` = UI.
`[ebp+8]` = incoming widget\*. `[ebp+12]` = flags
(`0` from `00596917`).

```
0059676B  lea ebx, [esi+32]
00596771  call 0059B039           ; --end → back()
00596776  mov edi, [eax]          ; old current widget*
0059677C  call 005952D8           ; push 0
          mov [ebp-4], 0x1A
          lea ecx, [esi+84]
          call 0059B5D7
          cmp ecx, [eax]          ; incoming == slot 0x1A?
          jne 005967C3            ; first-seen New Profile: skip audio
          … [0x13B8394] vtbl+184 …
005967C9  call [eax+192]          ; old, push 6
          call 0041E5F2
          call [edx+20]           ; unregister inner (edi+4)
005967DF  cmp [ebp+12], 0
          je 005967FB             ; 00596917: no pop
          … 0059AEE5 / 0059A8F3 pop until empty …
00596801  call 0059B61C           ; push_back [ebp+8] onto [ui+32]
0059680A  call 005952D8           ; push 0
0059680F  mov eax, [ebp+8]
00596812  mov [esi+156], eax      ; new
00596818  mov [esi+152], edi      ; old
```

`0059B039` copies the deque **end** iterator at
`[ui+32+16]` (four dwords) and `0059AC41` decrements
it (`add [ecx], -4`, with block wrap). `mov edi,[eax]`
is therefore **back()**, not begin.

`0059B61C` (`ecx` = `ui+32`):

```
0059B61C  mov edx, [ecx+24]
          mov eax, [ecx+16]       ; write cursor
          sub edx, 4
          cmp eax, edx
          je grow
          mov edx, [[esp+4]]
          mov [eax], edx          ; *back = widget*
          add [ecx+16], 4
          ret 4
```

That is **push_back**, not overwrite of `[ui+32]`
itself (that dword is the deque map base). After
first-seen attach the deque already holds Press
Start (`00599CAE` key `0x14` → `0059672A` → same
`0059B61C`). Arg2=`0` skips `0059AEE5` pop_back.
So `[ui+32]` **grows**: old `0x14` stays, `0x17`
becomes the new back.

Ctor / bind zero both cursors
(`0059545C` / `0059593F`). First-seen `0059672A`
does **not** write `+152`/`+156`. The first store
of those two fields after Press Start attach is
this `00596763`.

`functions.tsv` `0x00596763` has no `0041DB1D`.

---

## 3. `[ui+84]` including slot `0x14` stays

`00596763` calls `0059B5D7` once, key **`0x1A`**,
and only `cmp`s `[eax]`. No `mov [eax],…`. Slot
`0x1A` is already `UI_FRONTEND_QUIT_PROMPT` from
`00598A1C`.

`00596917` calls `0059B5D7` twice for key **`0x17`**:
first to pass `[eax]` into the switch, second to
hand the same widget\* to `00851700`. Both are
reads.

Press Start cell (`00598BB7` key `0x14`,
`00598BDA` `mov [ecx], eax`) is not on either
callee list. `00595222` still walks the same
header. First node is still slot 0 / null
(`proofs/00595222-first-node`,
`proofs/ui84-list-after-attach`).

---

## 4. Host leftover

`BindNewProfileFromArmedTick` notes `00596917` /
`00596763` then:

```
AttachFrontendTree(FrontendNewProfileMenu):
  _frontendWidgets.Clear()
  _frontendSlots.Clear()
  built = Factory.Build(rootName)
```

Native never clears `[ui+84]` and does not
re-factory slot `0x17`. Host drops slot `0x14`
(and every other key) and treats `widgets[0]` as
the sole root. That `Clear` is **LEFTOVER**.

`AttachPressStartWidgets` is the only first-seen
`BindFrontendSlot(0x14)` site. After the rebuild
that index is gone.

---

## Do not invent

- `00596763` allocating a second `[ui+84]` map.
- Replacing or zeroing slot `0x14` on `0xE5`.
- `[ui+32]` as a single widget\* dword
  (it is a deque; the write is `+16` push).
- Host `Clear` as the native switch.
- Oakvale / Main Menu `0059697A` as this path
  (`0059697A` does not call `00596763` first-seen).

**Proposed (do not apply here):** keep the slot
map across `00596917`; only push current onto
`[ui+32]` and set `+152`/`+156`.
