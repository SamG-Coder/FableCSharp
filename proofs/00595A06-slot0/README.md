# `00595A06` after `0x126` / `0059697A`: slot-0 write, not a new tree

Investigation only. No production `src/` edits.

Question: after message `0x126` → `0059697A`, does
`00595A06` overwrite existing `[ui+84]` key **0** with
Main Menu type-10? First node after that? Host leftover
vs `Clear()` + new tree?

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00580000.txt`
(`00595A06` / `0059697A` / `0059B5D7` / `00598A1C` /
`00598B90` / `00595222` / `0059899A` / `00599E3F`
`0059A008` / `00596763` / `00595B24`);
`listing-00400000.txt` (`0041DB1D` / `004292C0`);
`implementer/frontend/01-widget-construction.md`
(`UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE` `#216`
Type=10);
`src/Fable.Game/EngineLifecycle.cs`
(`CommitNewProfileFromArmedEdit`, `AttachFrontendTree`,
`FrontendWidgetListOffset`);
`proofs/00595222-first-node/README.md`;
`proofs/slot-table-0059B5D7/README.md`;
`proofs/draw-type10-fork/README.md`.

Do not re-prove persist Type=10 on PRESS_START /
NEW_PROFILE, `0xE5` packet `00598EE6`, or
`0059B5D7` node layout (`+16` key / `+20` value).

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER**.

---

## Verdict

**Yes — store into the existing key-0 cell, not a second
`[ui+84]` tree.** First-seen after Press Start, key 0
already exists with `[node+20]==0`. `00595A06` factories
`UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE` (persist
type **10**) and writes that widget\* into the same
`&node+20`.

First in-order node after the write is **still key 0**.
Its value is now the Main Menu type-10. Slots `0x14` /
`0x17` / `0x1` stay on the same map.

Host `AttachFrontendTree` `Clear()` + `widgets[0]` as a
sole new tree is **LEFTOVER** vs this path.

| Claim | Status |
| --- | --- |
| `0x126` next tick `00599E3F` → `0059697A` → `00595A06` | **PROVEN** (`0059A008` / `00596A49`) |
| `00595A06` `this` map is `[ui+84]`; every lookup key is **0** | **PROVEN** (`lea esi,[ecx+84]`; `xor ebx,ebx`) |
| First-seen key 0 already exists; value is **0** | **PROVEN** `00598B99` / `mov [eax],ebx` |
| Null value skips destroy; factory `0041DB1D` then `mov [edi],eax` | **PROVEN** `00595A21` `je` / `00595AB8` |
| Stored def is Main Menu no-continue; persist Type=10 | **PROVEN** string + `#216` |
| Same list object; no second `[ui+84]` head | **PROVEN** (find-or-insert only) |
| First walk node after write is key 0 / Main Menu type-10 | **PROVEN** (`00595222` `[head+8]`) |
| Native `Clear()` of `[ui+84]` on this switch | **DISPROVEN** |
| Host `Clear()` + new `widgets[0]` tree matches native | **DISPROVEN** leftover |
| Same-name slot 0 is rebuilt anyway | **DISPROVEN** (`jne 00595AD3`) |
| First-seen `0059697A` itself rewires `[ui+32]` | **DISPROVEN** (caller `00596763` after return) |

---

## 1. `0x126` reaches `00595A06` through `0059697A`

`0059A238` consumes `0x126` → `00851920` (`[ui+96+5]=1`).
Next `00599E3F` at `0059A008`:

```
0059A002  lea eax, [ebp-8]
          push eax
          mov ecx, esi
0059A008  call 0059697A
```

`0059697A` (`ret 4`): `004067C0` miss jumps to
`00596A63` and **does not** attach. Hit:

```
00596A34  push -1
00596A36  push "UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE"
00596A3B  lea ecx, [ebp-8]
          call 0099EBF0
          lea eax, [ebp-8]
          push eax
          mov ecx, esi
00596A49  call 00595A06
          lea ecx, [ebp-8]
          call 0099EAE0
          push [ebp+8]
          mov ecx, esi
00596A5B  call 00595B24
```

`00595B24` only binds labels through `00595AD9` (slot
args `0` / `24` / `1` / …). It does not replace
`[ui+84]`. Twin empty-continue attach is `0059899A` →
same `00595A06`; that is **not** the `0x126` site.

---

## 2. `00595A06` only writes key 0 on the existing map

Entire body (`00595A06`–`00595AD6` `ret 4`):

```
00595A0E  lea esi, [ecx+84]      ; same map as Press Start
          xor ebx, ebx           ; key = 0
          mov [ebp-8], ebx
          call 0059B5D7          ; &node+20
          cmp [eax], ebx
          je  00595A85           ; value 0 → skip name/destroy
          … vtbl+336 name vs arg …
          je  00595A85           ; same name → keep
          … vtbl+0(1) destroy …
          mov [eax], ebx         ; zero cell
00595A85  mov [ebp-12], ebx      ; key 0 again
00595A8E  call 0059B5D7
          cmp [eax], ebx
          jne 00595AD3           ; already occupied → no factory
          push [ebp+8]
          call 0041E5F2
          call 0041DB1D          ; factory name
00595AB8  mov [edi], eax         ; store into existing cell
          mov [ebp+8], ebx       ; reuse arg as key 0
          call 0059B5D7
          mov ecx, [eax]
          call [eax+172]         ; layout 0052C730 / 0054E4B0
```

`0059B5D7` is find-or-insert: miss allocates `{key,0}`
and returns `eax+20`. Hit returns the **existing** cell.
No new map head. No walk of other keys.

First-seen Press Start (`00598B90`) already inserted
key 0 and stored null:

```
00598B93  add esi, 84
          mov [ebp+108], ebx     ; 0
          call 0059B5D7
          mov [eax], ebx         ; value = 0
          mov [ebp+108], 0x14
          call 0059B5D7
          call 0041DB1D          ; PRESS_START
          mov [ecx], eax
```

So the `0x126` call hits `je 00595A85` then the factory
arm. That is a **value overwrite** of an existing key-0
node, not a new tree and not a destroy of a previous
Main Menu.

If slot 0 already held the **same** name, `cmp [eax],ebx`
at `00595A93` is nonzero and the factory is skipped.

---

## 3. First node after the store

`00595222` in-order walk (unchanged):

```
00595225  mov eax, [ebx+84]      ; sentinel
          mov esi, [eax+8]       ; leftmost = smallest key
          mov ecx, [esi+20]
          test ecx, ecx
          je  next               ; skip null
          call [vtbl+8]
          call 004292C0
```

After `00595A06`, leftmost key is still **0**.
`[node+20]` is now the Main Menu widget. First non-null
draw is that type-10. Previously first non-null was
slot `0x1` OPTIONS because key 0 was null.

`0x14` PRESS_START and `0x17` NEW_PROFILE remain in
the map. `0059697A` does not call `00596763`. The tick
caller after a successful `0059697A` does:

```
0059A01D  mov [ebp-20], ebx      ; key 0
          call 0059B5D7
          push 1
          push [eax]
          call 00596763          ; current [ui+32] / +152/+156
```

That rewires the **current** stack, not `[ui+84]`.

---

## 4. Host leftover: `Clear()` + new tree

`CommitNewProfileFromArmedEdit` → `AttachFrontendTree`
(`EngineLifecycle.cs`):

```
_frontendWidgets.Clear()
_frontendSlots.Clear()
built = Factory.Build(MAIN_MENU_NO_CONTINUE)
FrontendRootType = built[0].Type   ; 10
```

Native never clears `[ui+84]` on `0xE5` or `0x126`.
Host drops Press Start / New Profile / OPTIONS slots
and treats `widgets[0]` as the only root. That is
**LEFTOVER**, not the slot map.

---

## Do not invent

- A second `[ui+84]` allocation on `00595A06`.
- `00595A06` writing slot `0x14` or `0x17`.
- `.text` `mov […], 0x126`.
- Host `Clear()` as the native attach.
- First-seen destroy of a live slot-0 widget (value
  was 0).
