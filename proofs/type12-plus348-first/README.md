# Type-12 list `+348` first-seen (Press Start / New Profile / Main Menu)

Investigation only. No production `src/` edits.

Question: Type-12 list `+348` highlight first-seen on Press Start /
New Profile / Main Menu. Who writes `+348`? Is it related to type-11
`+352` selected `u8` or type-10 packet*? Host leftover?

Authority: `Fable.exe` listings
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00500000.txt`
(`0053B63E` / `0053B662` / `0053B717` / `0053B73B` / `0053822B`),
`listing-00540000.txt`
(`0054C3A0` / `0054C59E` / `0054C95E` / `0054D660` / `0054D6F1` /
`0054DA70` / `0054E3D0` / `0054E4F0` / `0055BA20` / `0055C0DE`),
`listing-00400000.txt` (`0041D53C` / `0041D552`);
inflated `frontend.bin` + `implementer/frontend/persist-scan.txt`,
`01-widget-construction.md`;
`proofs/type12-highlight-plus348/README.md`,
`proofs/list-type12-focus/README.md`,
`proofs/type11-plus352-select/README.md`,
`proofs/type10-plus352/README.md`,
`proofs/type10-plus352-writers/README.md`,
`proofs/who-posts-15/README.md`;
`FrontendWidgetFactory.ApplyFirstSeenState`,
`FrontendInputMap.Type10StoredMsgOffset`.

Status: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **MATCH**.

Do not re-prove type 4 → action 26, persist 15 on
`UI_FRONTEND_BUTTON_NEW_GAME`, or Press Start `0xE5` packet fill
`00598EE6`.

---

## Verdict

| Claim | Class |
| --- | --- |
| Type 12 factory `0041D53C` alloc `0x1FC` → ctor `0054C3A0` | **PROVEN** |
| `0054C3A0` is type 8 `0053B63E` then vtbl `01249224` / inner `012491FC` | **PROVEN** |
| Type 8 ctor `0053B662` `xor eax; mov [esi+348], eax` | **PROVEN** |
| Same ctor zeros vector `+356/+360/+364` then `0053822B` fills `+356` | **PROVEN** |
| Type 12 layout `0054D660` `xor ebp` then `0054D6F1` `mov [esi+348], ebp` | **PROVEN** |
| First-seen list `+348` is **0** (ctor then attach rewrite) | **PROVEN** |
| Highlight child is `[[esi+356] + [esi+348]*4]` | **PROVEN** |
| Nav `0054C59E` / `0054C95E` write `+348` first-seen | **DISPROVEN** (input later) |
| Persist field loads a saved row into `+348` | **DISPROVEN** |
| Press Start list is `UI_FRONTEND_LIST_PRESS_START_MENU` type 12, one persist child `UI_FRONTEND_BUTTON_INVISIBLE` | **PROVEN** |
| Main Menu list is `UI_FRONTEND_LIST_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE` type 12, persist `[0]` = `UI_FRONTEND_BUTTON_NEW_GAME` | **PROVEN** |
| New Profile list is `UI_NEW_PROFILE_MENU` type 12 | **PROVEN** (`01-widget-construction`) |
| `+356[0]` == persist child 0 on those lists | **PARTIAL** (`def+148` skip-set **UNREAD**) |
| New Profile `+356[0]` widget name | **UNREAD** (no hex child walk in `persist-scan.txt`) |
| List `+348` is type-11 `+352` selected `u8` | **DISPROVEN** |
| List `+348` is type-10 packet* | **DISPROVEN** |
| Type-12 `+352` is `[esi+48]` cache (`0054D6E8`), not packet* / not the `u8` | **PROVEN** |
| `0053D200` writes list `+348` | **DISPROVEN** |
| Host has a list-`+348` slot | **DISPROVEN** |
| Host leftover vs native highlight | **LEFTOVER** |

**Answer:** first-seen writers are type 8 ctor `0053B662` and type 12
layout `0054D6F1`. Both store **0**. Same pair runs on Press Start,
New Profile, and Main Menu because each tree factories a type-12
through `0041D552`. The dword is a highlight **index** into `+356`.
It is **not** the type-11/38 selected `u8` at widget `+352` and
**not** the type-10 packet* at widget `+352`. C# has no `+348`;
`ActiveChild` only exclusive-hides type **18**.

---

## 1. Dump — who writes type-12 `+348`

Factory type 12 (`listing-00400000.txt`):

```
0041D53C  push 0x1FC
0041D541  call 00BFEA1A
0041D552  call 0054C3A0
```

Ctor (`listing-00540000.txt`):

```
0054C3A0  push def
          mov esi, ecx
          call 0053B63E          ; type 8
          mov [esi],     0x1249224
          mov [esi+4],   0x12491FC
          mov [esi+24],  0x12491F4
          ret 4
```

No `+348` store in the type-12 tail. Type 8 (`listing-00500000.txt`):

```
0053B63E  call 0052CC50
          xor eax, eax
          mov [esi],     0x12462E4
          …
0053B662  mov [esi+348], eax     ; highlight = 0
          lea ecx, [esi+356]
          mov [ecx], eax         ; begin
          mov [ecx+4], eax       ; end
          mov [ecx+8], eax       ; cap
          …
          call 0053822B          ; push_back subset of +176 onto +356
```

Copy-ctor `0053B717` / `0053B73B` is the same zero (not first-seen
factory). `0053822B` does **not** write `+348`.

Layout (`listing-00540000.txt`), immediately before type-12 dtor
`0054DA70` that resets vtbl `01249224`:

```
0054D660  mov esi, ecx
0054D669  call 0052C730          ; +324/+328/+332 = 0 (style)
0054D67D  xor ebp, ebp
0054D67F  xor ebx, ebx
          ; walk +356 → +380
0054D6DF  mov ecx, [esi+48]
0054D6E8  mov [esi+352], ecx     ; cache of +48, not highlight
0054D6F1  mov [esi+348], ebp     ; ebp still 0
```

`01249224+172` rdata dword is **UNREAD**. Body lives in the type-12
cluster and is the only recovered attach rewrite of list `+348`.
Type 8 layout `0053B91E` has **no** `mov […+348]`
(`listing-00500000.txt` only `0053B662` / `0053B73B` in that family).

Other type-12 stores of `+348` in `listing-00540000.txt`:

| VA | Fn | When |
| --- | --- | --- |
| `0054C5BE` / `0054C5DC` | prev `0054C59E` | `dec` / wrap, then child `vtbl+192(3)` |
| `0054C97F` / `0054C9A4` | next `0054C95E` | `inc` / wrap, then child `vtbl+192(3)` |
| `0054D6F1` | layout `0054D660` | attach, **0** |

Nav is not first-seen. No persist CRC loads the index.

Highlight walk (same listing):

```
0054C59E  mov eax, [esi+348]
          mov ecx, [esi+356]
          mov ecx, [ecx+eax*4]   ; child*
          push 4
          call [child.vtbl+192]
```

---

## 2. Three first-seen screens — same writers

`0041D21B` type 12 is one ctor. `005331A0` builds persist children
during the parent factory. Root attach then recurses `vtbl+172`
(`0052C730` → `005339B0`). Every type-12 child hits `0054C3A0`
then `0054D660`.

### Press Start

`UI_FRONTEND_PRESS_START_MENU` type 10. Persist child `#624`
`UI_FRONTEND_LIST_PRESS_START_MENU` type **12**, `Children` **1**
(`persist-scan.txt`, `FrontendUiDefTests`). Nested `#625`
`UI_FRONTEND_BUTTON_INVISIBLE` type 11, persist `0xE5`.

List persist `Action` `0xF1A22807` = **0**. First-seen `+348 = 0`
names `+356[0]` = that only persist child **if** `def+148` is empty
(empty find → every `+176` child lands in `+356`;
`type12-highlight-plus348`).

### New Profile

`UI_FRONTEND_NEW_PROFILE_SCREEN` type 10. Child
`UI_NEW_PROFILE_MENU` type **12** (`01-widget-construction.md`
`#201` walk). Helpers `UI_HELPERS_NEW_PROFILE` sit **after** the
menu. Same ctor/layout → first-seen `+348 = 0`. Which persist child
is `+356[0]` is **UNREAD** (no `inst=UI_NEW_PROFILE_MENU` hex in
`persist-scan.txt`; `ui-cancel-message`).

### Main Menu

`UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE` type 10. First
persist child `UI_FRONTEND_LIST_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE`
type **12**. Persist `[0]` = `UI_FRONTEND_BUTTON_NEW_GAME` type 11
id **15** (`who-posts-15`, factory test). First-seen `+348 = 0`
names that row as the highlight **index**. Attach does **not** call
child `vtbl+192(3)` (`type12-highlight-plus348` §7).

---

## 3. Not type-11 `+352` selected `u8`

Type 33 ctor (type 11/38 go through `0055B460` → `0055BA20`):

```
0055BA46  mov [esi+348], eax     ; dword, different object
0055BA4C  mov [esi+352], al      ; selected u8 = 0
```

Action 26 on type 11/38 (`0055AD60`, inner = widget+4):

```
0055AD7B  mov al, [esi+348]      ; widget+352 u8
          test al, al
          je  skip 0055AF60
```

Only recovered `+352 = 1` on that family is `0055C0DE` inside
`0055BF10` (hit-test / take selection). First-seen attach leaves
the byte **0** (`type11-plus352-select`).

| Object | Offset | Width | First-seen | Role |
| --- | ---: | --- | --- | --- |
| Type 12 list | **`+348`** | dword | **0** | highlight index into `+356` |
| Type 11/38 | **`+352`** | **u8** | **0** | click gate |
| Type 11/38 | `+348` | dword | 0 | not the gate (`0055BA46`) |

Same numeric `348` on **inner** type 11 is widget `+352`. That is
a different object from the list.

---

## 4. Not type-10 packet*

Type 10 ctor `0054E3D0` size `0x16C`:

```
0054E3F3  mov [esi+352], eax     ; packet* = 0
```

Nonzero store is only `0054E4F0` (`vtbl+284`):

```
0054E530  mov [esi+352], ebx     ; packet*
```

Press Start attach writes `0xE5` into the **packet**, then slot
`0x14` `vtbl+284`. Main Menu / New Profile type-10 roots never
call that path; their `+352` stay **0** (`type10-plus352-writers`).

Type-10 action 26 (`0054E2FA`) is `mov eax, [edi+348]` /
`test eax` / `push &inner+348` = **`&widget+352`** (packet*).
Width is a **pointer**, consumer is UI `vtbl+32`.

Type-12 layout writes **`[esi+352] = [esi+48]`** at `0054D6E8`
and **`[esi+348] = 0`** at `0054D6F1`. Those are list slots.
Type-10 `0054E4B0` also caches `[esi+48]` at type-10 `+348` —
again a **different** object, and **not** the packet*.

| Object | `+348` | `+352` |
| --- | --- | --- |
| Type 10 | `+48` cache (`0054E4B0`) | packet* (`0054E4F0`) |
| Type 12 | **highlight index** | `+48` cache (`0054D6E8`) |
| Type 11/38 | unused dword | selected **u8** |

---

## 5. Host leftover

`FrontendWidgetFactory.ApplyFirstSeenState` sets `ActiveChild = 0`
and exclusive-hides children only when `SelectsChild` (type **18**).
Type 12 keeps every persist child visible. That **MATCH**es native
`+332=0` (style) and **does not** model list `+348` /
`vtbl+192(3/4)`.

`FrontendInputMap.Type10StoredMsgOffset = 352` is the type-10
packet*. `MessageFromType10Attach` (action 26) returns the first
visible type-10 `MessageId`. Native first-seen Main / New Profile
type-10 `+352` is 0; Press Start posts the attach packet, not the
list index.

C# does **not** gate type-11/38 clicks on a selected `u8`, and
does **not** walk `+356[+348]`. First-seen highlight 0 is therefore
a **LEFTOVER** hole: native already names persist child 0; host
does not store the index.

---

## 6. UNREAD

- `01249224+172` / `+192` rdata dwords (bodies recovered).
- `CUIDef+148` on the three lists (empty vs skip-set).
- New Profile persist `ChildIndices` / `+356[0]` name.
- First caller of list `vtbl+192(3)` after attach (`0054D056`
  vs later show).
- Lionhead name for `def+148`.

Do **not** invent:

- List `+348` as type-11 selected or type-10 packet*.
- A persist-saved first-seen row.
- Nav as the first-seen writer.
- Host `ActiveChild` as list highlight.
