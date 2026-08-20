# `00595222` all-slot `vtbl+8`: why Options / New Profile stay off Press Start?

Investigation only. No production `src/` edits.

Question: `00595222` calls `vtbl+8` on every non-null
`[ui+84]` value. How do Options / New Profile stay
undrawn on Press Start? `+302` bit 0? `[ui+32]`
current? Host draw-current-only leftover vs all-slot
walk?

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00580000.txt`
(`00595222` / `00599E3F` `0059A0C4` / `0059672A` /
`00596763` / `00598A1C`);
`listing-00500000.txt` (`00530260` / `0052F1D0` /
`0052F180` / `005331A0` `00533288` / `005334A0`
`005336D2` / `00531EC0` `00532200` / `0052C7E0` /
`0052CF40`);
`listing-00400000.txt` (`0042E085` `0042E091` /
`0041AFA0`);
`listing-00600000.txt` (`00632065` persist `+392`);
`src/Fable.Game/EngineLifecycle.cs`
(`DrawFrontendWidgets`, `AttachFrontendTree`);
`proofs/00595222-first-node`,
`proofs/ui84-list-after-attach`,
`proofs/00596763-switch`,
`proofs/slot-table-0059B5D7`,
`implementer/frontend/14-container.md`.

Do not re-prove `[ui+84]` key/value shape, slot
`0x14` / `0x17` factory names, or type-10 `vtbl+8`
=`00530260`. Do not invent dest pixels or DIP counts.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER**.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| `00595222` `vtbl+8` every non-null `[ui+84]`? | **Yes.** Null `[node+20]` is the only skip. | **PROVEN** |
| `[ui+32]` current filters that walk? | **No.** `00595222` never loads `ui+32`. Args are `0042E088` device + **0**. | **DISPROVEN** |
| `+302` bit 0 at `00595222`? | **No.** | **DISPROVEN** |
| `+302` bit 0 on the slot root inside `00530260`? | **No.** `vtbl+420` is only on **`+176` / `+188` children**. | **DISPROVEN** as root early-out |
| `+302` bit 0 as a runtime “not current” hide? | **No writer.** Only ctor `or [+302],1` from persist `def+392`. | **DISPROVEN** |
| Persist `+392` on OPTIONS / NEW_PROFILE trees? | Not in the inflated Press Start dump. | **UNREAD** |
| Host draws current named tree only? | **Yes.** `Clear` + `Build` then `ChildrenOf(..., null)`. Native walks every slot. | **LEFTOVER** vs all-slot |

---

## Verdict

**Native draw is an all-slot walk. Host is current-only leftover.**

`00595222` does **not** consult `[ui+32]`, type, or
`+302`. After first-seen `00598A1C`, slot `0x1`
`UI_FRONTEND_OPTIONS_MENU` and slot `0x17`
`UI_FRONTEND_NEW_PROFILE_SCREEN` are non-null, so
both get `vtbl+8` (`00530260` on those type-10
roots). `[ui+32]` is the current **deque**
(`0059672A` / `00596763`), used for input register
and later `vtbl+192(6)` on the *old* current — not
for this Present walk.

`+302` bit 0 (`vtbl+420` `0052F1D0` = `[+302]&1`
from persist `+392`) skips **container children**,
not the slot root `this`, and is a **ctor persist
bit**. There is no `.text` `or [+302],1` on menu
switch. It cannot be “hide inactive slot.”

In-order keys put Options **under** Press Start
(`0x1` then `0x14`) and New Profile **after**
(`0x17`). Covering cannot explain New Profile.
Whether those `vtbl+8` calls enqueue a DIP is
**UNREAD** (no pixel metrics). Parent≠`this` skip
is **DISPROVEN** after the same-frame tick:
`00599E3F` → `0052C7E0` → `00531EC0` writes
`+200` parent on every slot’s `+176` kids before
`0042DF9E` / `00595222`.

| Claim | Status |
| --- | --- |
| `00595222` in-order `[ui+84]`, skip only `[node+20]==0`, `call [vtbl+8]` | **PROVEN** |
| Caller args are `( [esi+88], 0 )` — not current widget | **PROVEN** `0042E085` |
| `00595222` / `00530260` load `[ui+32]` | **DISPROVEN** |
| `[ui+32]` is current deque (`0059B61C` / `0059B039`) | **PROVEN** |
| Slot `0x1` OPTIONS and `0x17` NEW_PROFILE are non-null after `00598A1C` | **PROVEN** |
| `0x17` sorts after `0x14` (cannot sit under Press Start) | **PROVEN** |
| Type-10 `vtbl+8` `00530260` tests `this.+302` bit 0 | **DISPROVEN** |
| `00530260` skips a child if `vtbl+420` (`+302` bit 0) | **PROVEN** (twice) |
| Leaf `0041AFA0` / type-6 `0054EF00` read `+302` bit 0 | **DISPROVEN** (no `+302` in those bodies) |
| Runtime hide writes `+302` bit 0 | **DISPROVEN** (ctor persist only) |
| OPTIONS / NEW_PROFILE persist `+392` | **UNREAD** |
| `parent!=this && !vtbl+400` hides other slots on first Present | **DISPROVEN** (`00531EC0` sets `+200`) |
| Host `DrawFrontendWidgets` = native all-slot walk | **DISPROVEN leftover** |
| DIP / dest pixels of OPTIONS / NEW_PROFILE on Press Start | **UNREAD** |

---

## 1. Walk is every non-null slot (`PROVEN`)

`0042E085` (`listing-00400000.txt`):

```
0042E085  mov eax, [esi+88]
          push ebx                 ; ebx already 0
          push eax
          call 00595582            ; UI*
          mov ecx, eax
          call 00595222            ; ret 8
```

`00595222` (`listing-00580000.txt`):

```
00595225  mov eax, [ebx+84]
00595229  mov esi, [eax+8]         ; leftmost
00595230  mov ecx, [esi+20]
          test ecx, ecx
          je 0059524A              ; null value only
          mov eax, [ecx]
          push 0
          push 0
          push 0
          push [esp+28]            ; forwarded arg1
          push [esp+28]            ; forwarded arg0
          call [eax+8]
0059524A  call 004292C0
          cmp esi, [ebx+84]
          jne 00595230
```

No `cmp` of type 10. No `[ebx+32]`. No `vtbl+420`.
Same all-slot rule on tick `0059A0C4` (`vtbl+4`).

First-seen fill (`slot-table-0059B5D7`): key `0x1`
OPTIONS, key `0x17` NEW_PROFILE, both `mov [ecx],eax`.
Those nodes are not null, so they are called.

---

## 2. `[ui+32]` is current, not a draw gate (`DISPROVEN` as filter)

`0059672A` (tail of `00598A1C` when `[ui+192]==0`)
pushes slot `0x14` onto **`ui+32`** (`0059B61C`) and
registers `widget+4` with input `0041E5F2` `vtbl+8`.
It does not store `[ui+84]` and does not call draw.

`00596763` (later `0xE5` → `00596917`) does
`vtbl+192(6)` on the **old** `[ui+32].back()`, then
push_back the new widget. That is the current-stack
switch. First-seen Press Start Present has **not**
run it. OPTIONS / NEW_PROFILE were never current, so
they never received state 6 as a hide.

`0052CF40` (`vtbl+192`) writes `+332` and forwards
`vtbl+188` to own children. It does **not**
`or [+302],1`.

---

## 3. `+302` bit 0 is a child skip, persist ctor (`PROVEN` / `DISPROVEN` as slot hide)

`00530260` (type 5/10/12/18 `vtbl+8`) starts with
`vtbl+404` / `vtbl+416` (layer), then walks `+176`
then `+188`. Per child:

```
parent = child.vtbl+208            ; [child+200]
if parent != this && !child.vtbl+400: skip   ; +300 bit 7
if child.vtbl+420: skip                      ; +302 bit 0
if child.vtbl+420: skip                      ; same, twice
else child.vtbl+8(...)
```

`0052F1D0`: `mov al,[ecx+302]; and 1`.
`005331A0`: `cmp [def+392],0` / `or [ebx+302],1`.
`005334A0` zeros `+302` then that `or`. Persist
writer `00632065` `lea edx,[esi+392]` / `0043314A`.
Listing `or […+302]` sites are ctor / type-6 align
(`0x08/0x10/0x20`), not menu switch.

`0041AFA0` and `0054EF00` have **no** `+302`. A
clipped leaf is skipped by the **parent** walk, not
by the leaf.

Same-frame `00599E3F` (`0042DC94` before
`0042DF9E`) calls every non-null slot `vtbl+4`
`0052C7E0` → `00531EC0`. That walk:

```
0053220B  call [edx+208]           ; parent
          jne already
          push esi
          call [eax+204]           ; set parent = this
00532230  cmp esi, [ecx+200]
```

After that, OPTIONS / NEW_PROFILE children have
`parent==root`. The `parent!=this && !vtbl+400`
arm does **not** hide them on the Present that
follows.

Forest tiles on Press Start draw with persist `+392`
**0** (`persist-flag-names`). OPTIONS / NEW_PROFILE
file `+392` was **not** extracted (zlib blobs). If
those children are also 0 — required to draw when
that slot *is* current, with no unclip writer —
bit 0 is **not** the Press Start hide.

---

## 4. Host leftover (`PROVEN`)

`AttachFrontendTree`:

```
_frontendWidgets.Clear()
built = Factory.Build(rootName)
```

`DrawFrontendWidgets` then `ChildrenOf(_frontendWidgets, null)`
(one persist root). Native never clears `[ui+84]` on
Press Start populate and does not restrict `00595222`
to `[ui+32].back()`.

Host `DrawContainerWalk` `if (!Visible || Clip) return`
is the analog of **child** `vtbl+420`, applied inside
the **current** list only. That is why the host does
not enqueue Options / New Profile on Press Start. It
is **not** the native slot walk.

---

## Do not invent

- Dest rectangles or DIP counts for OPTIONS / NEW_PROFILE
  on a Press Start Present.
- Lionhead name of persist `+392`.
- `[ui+84]` as a single-screen `+176` list.
- `+302` bit 0 as “not `[ui+32]` current.”

**Proposed (do not apply here):** keep the slot map;
`00595222` still calls every non-null `vtbl+8`; do not
gate that walk on `[ui+32]`; do not treat persist clip
as the current-screen flag without OPTIONS / NEW_PROFILE
`+392` bytes.
