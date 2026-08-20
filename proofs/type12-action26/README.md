# Type-12 list action 26 (`0053D200` / `0053D5C1`) does not post 15

Investigation only. No production `src/` edits.

Authority: `Fable.exe` listings
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00500000.txt`
(`0053B63E` / `0053D200` / `0053D394` / `0053D540` / `0053D5C1` /
`0053D970` / `005367F3` / `00536A0B` / `0053848F` / `005384DD` /
`0052DA20`),
`listing-00540000.txt`
(`0054C3A0` / `0054C430` / `0054C59E` / `0055AD60` / `0055AF60` /
`0055B9D0` / `0055C0DE` / `0055CB10`);
inflated `frontend.bin` + `implementer/frontend/persist-scan.txt`;
`proofs/type11-msg15/README.md`,
`proofs/list-type12-focus/README.md`,
`proofs/action26-subscribers/README.md`,
`proofs/who-posts-15/README.md`.

Status: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Do not re-prove type 4 → `push 26`, type-11 persist **15**, or
`0059A238` consume of 15.

---

## Verdict

Main Menu list
`UI_FRONTEND_LIST_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE` (type **12**,
first persist child of the type-10 menu) **subscribes** action 26
through type-8 activate `0053D540` (`0053D5C1` `push 26`). That is
**not** a 15 post.

If apply runs (`0053D200`), action 26 only forwards to `0055AD60`
when `[def+545]`. That arm uses list **`+352`** as the click gate
and list `vtbl+584` — **not** highlight index `+348`, **not**
`+428/+432`, **not** child persist 15.

| Claim | Class |
| --- | --- |
| Type 12 ctor `0054C3A0` = type 8 `0053B63E` then vtbl `01249224` / inner `012491FC` | **PROVEN** |
| Type 8 ctor zeros `+348` (highlight) and `+428/+432` (packet*) | **PROVEN** `0053B662` / `0053B6A9` |
| Activate `0053D540` / `0053D5C1` inserts **26**, 31, 28, 27, 32, 29 via inner `vtbl+12` if `[def+545]` | **PROVEN** |
| Inner `vtbl+12` is local map insert `0052DA20`, not `0055CB10` register | **PROVEN** shape (same site as type 11) |
| Apply `0053D200` action 26 → `0055AD60` iff `[def+545]`; no extra 2–5 switch | **PROVEN** |
| `0055AD60` case 26 posts persist 15 / `vtbl+524([+372])` | **DISPROVEN** (that is type-11 `vtbl+584` `0055AF60` / later action 27) |
| Action 26 posts list `+428` / `+432` | **DISPROVEN** (`0053D200` never loads those slots) |
| Type-8 tick `005367F3` posts `+428` when `+324==0`, `+432` when `+324==3` | **PROVEN** `00536A01` / `00536A1A` |
| Type-12 tick `0054C430` posts `+428/+432` | **DISPROVEN** (override drops those arms) |
| Action 26 changes list `+348` or child `vtbl+192` | **DISPROVEN** |
| Action 26 focuses first child type 11 | **DISPROVEN** in `0053D200`; first-child highlight is index 0, other sites |
| List persist id is 15 / the New Game poster | **DISPROVEN** (list Action / `0x53C644E4` **0**; poster is type-11 child) |
| Action 26 on the list changes **which** widget posts 15 | **DISPROVEN** |
| Type-12 inner `012491FC+4` is still `0053D200` | **PARTIAL** (no `.rdata` dump; no type-12 apply clone) |
| Type 8/12 inner is a `0055CB10` list node first-seen | **UNREAD** (ctor has no `0041E5F2` + input `vtbl+8`) |
| `[def+545]` first-seen on this list | **UNREAD** |
| List `vtbl+584` VA | **UNREAD** (no rdata) |

**Answer:** action 26 on the Main Menu type-12 is **subscribe +
optional arm of the list itself**. It does **not** post 15, does
**not** post `+428/+432`, does **not** retarget the poster. The
widget that posts 15 is still the highlighted type-11
(`UI_FRONTEND_BUTTON_NEW_GAME`). Highlight / `vtbl+192(3)` is a
**different** path (`+348`, ctor 0 = first child).

---

## 1. Object (`0054C3A0` / `0053B63E`)

```
0054C3A0  push def
          call 0053B63E          ; type 8, alloc 0x1FC
          mov [esi],     01249224
          mov [esi+4],   012491FC
          mov [esi+24],  012491F4
```

Type 8 extra fields after type 5 (`0x15C` = 348):

| Off | Ctor | Role |
| ---: | --- | --- |
| `+348` | `xor eax; mov [esi+348], eax` | highlight index into dword vector `+356` |
| `+352` | **not** written in `0053B63E` | byte/dword click gate used as inner `+348` |
| `+356` | empty vector | highlighted children |
| `+428` | 0 | message packet* (store `0053848F`) |
| `+432` | 0 | message packet* (store `005384DD`) |

`0052C730` after layout still sets `+324/+328/+332=0` (style, not
the highlight index).

Press Start list persist `Action` `0xF1A22807` = **0**. Main Menu
list persist id is **0** (`who-posts-15`). Child
`UI_FRONTEND_BUTTON_NEW_GAME` type 11 holds **15**.

---

## 2. `0053D5C1` is activate subscribe, not apply

`0053D540` (widget `this`):

```
call [vtbl+432]              ; CUIDef*
mov  bl, [def+545]
test bl, bl
je   0053D5F5                ; skip
add  esi, 4                  ; inner
push 26
call [inner.vtbl+12]         ; 0053D5C1
push 31 / 28 / 27 / 32 / 29  ; same slot
```

Deactivate `0053D970` erases the same ids via inner `vtbl+16`.

`0052DA20` (type-5 family inner `vtbl+12`): insert `arg` into the
map at inner `+4`. Action **25** also calls inner `vtbl+4(25)`.
Action **26** only inserts.

This matches type-11 `0054DC30` / `0054DC7E`. It is **not**
`0055CB10` and **not** a UI `vtbl+32` post.

Type 8/12 ctor never does `0041E5F2` + input `vtbl+8`. Type 11/38
do, via `0055BA20`. So first-seen `0055CB10` delivery to the list
inner is **UNREAD**. The list **can** be in the local accept-set
for 26 after `0053D540`; that does not make it the 15 poster.

---

## 3. Dump `0053D200` (apply)

`ecx` = inner (`widget+4`), same as type-10 `0054E280`.

```
0053D200  esi = this (inner)
          call 0041E5F2 ; input.vtbl+44 gate
          if that and action==25 → ret
          call 0041E5F2 ; [input+164] → ret
          debounce [inner+44]-[+400] vs [+392]  (skip if action==25)
0053D285  edi = widget (esi-4)
          widget.vtbl+432 → def*
          bl = [def+545]
          if bl: 0055AD60(action)          ; ecx = inner
          lea eax, [action-2]
          cmp eax, 3
          ja  0053D387                     ; action 26 → here
          jmp [0x53D394+eax*4]             ; actions 2..5 only
```

Jump table at `0053D394` (dwords, listing as code):

| `action-2` | VA | Meaning |
| ---: | --- | --- |
| 0 (action 2) | `0053D2D9` | widget `vtbl+624` |
| 1 (action 3) | `0053D2F1` | optional `+196` type-8 child / `vtbl+620` |
| 2 (action 4) | `0053D2F1` | same |
| 3 (action 5) | `0053D2D9` | `vtbl+624` |

Action **26** is `26-2=24` → `ja` → return. No `+348` bump, no
child walk, no `+428/+432`.

Type 12 adds **no** second apply body after `0054C3A0` (next real
fn is tick `0054C430`). Inner `+4` is **PARTIAL** rdata, but there
is no recovered type-12 override of `0053D200`.

---

## 4. `0055AD60(26)` on a list is not a 15 post

Called with `ecx` = list **inner**:

```
0055AD66  lea eax, [action-26]     ; 0 for 26
          jmp [0x55AE88+eax*4]
0055AD7B  mov al, [esi+348]        ; inner+348 = widget+352
          test al, al
          je  0055AE3D             ; 0055B9D0 only
          lea ecx, [esi-4]         ; the LIST widget
          call [vtbl+584]
          mov [esi+364], 1
          call 0055B9D0
```

`0055B9D0` is `if action==25: widget.vtbl+580; ret`. Not
`0059A238`.

Contrast type-11 click `0055AF60` (type-11 `vtbl+584`):
`vtbl+524([widget+372])` posts the persist-15 list from
`0055B040`. That slot lives on the **type-11** object (size
`0x1B4`), not on the type-12 list (`+372` there is a different
vector).

List `+348` is the **highlight index** (ctor 0). `0055AD60` does
**not** read it. It reads **`+352`**. Type 8 clears `+352` on
deselect (`0053D43A` / `0053D706` `mov [esi+352], 0`). Type 11
sets `+352=1` in `0055C0DE` when that button takes selection.
No recovered type-8 `mov [esi+352], 1` in this dump; first-seen
`+352` is **UNREAD** (ctor hole; heap may be zero).

If `+352==0` or `[def+545]==0`, action 26 on the list is a
no-op besides debounce / `0055B9D0`.

---

## 5. `+428` / `+432` are a **tick** poster, not action 26

Ctor packet* = 0. Stores `0053848F` / `005384DD` alloc a 4-byte
object (`00535140`) and `00535710` the arg. Posted only as
`push [esi+428|+432]; call [vtbl+524]`.

Type-8 tick `005367F3` (after `0052C7E0` + `vtbl+548`):

```
if +324 == 0:  vtbl+524([+428])     ; 00536A01
if +324 == 3:  vtbl+524([+432])     ; 00536A1A
if +324 == 5:  vtbl+192(3) on list
               and child[+348].vtbl+192(3)
```

Type-12 **overrides** that tick (`0054C430`):

```
call 0052C7E0
call vtbl+548
if al:
  +324==6 → vtbl+192(0)
  +324==5 → vtbl+192(3)     ; list only — no child, no +428/+432
```

First-seen `+324=0`. Even the type-8 `+324==0` post is **not**
on type 12. Persist payloads in those packets are **0** on the
recovered Main Menu / Press Start lists — they are **not** 15.

---

## 6. First child type 11 is highlight 0, not action 26

```
0054C59E  child = [[esi+356] + [esi+348]*4]
          child.vtbl+192(4)     ; old
          dec / wrap +348
          child.vtbl+192(3)     ; new
```

That is list **navigation**, not `0053D200`. First-seen `+348=0`
→ `UI_FRONTEND_BUTTON_NEW_GAME` is the highlighted row
(`list-type12-focus`). Type-11 activate `0054DC30` then
`vtbl+192(3)` + local subscribe 26, and `0055C0DE` can set that
child `+352=1`.

Action 26 on the **list** never writes `+348` and never calls a
child `vtbl+192`. It does **not** “focus first child” and does
**not** pick a different poster.

`0055CB10` still broadcasts to every registered inner whose
accept is true. Type-11 children are the proven listeners that
can arm (`action26-subscribers`). The list’s local 26 bit does
not unregister them and does not replace their persist 15.

---

## 7. C# leftover

`FrontendInputMap.MessageFromWidgets` walks visible type 10/11/38
with `MessageId≠0`. Type 12 is skipped (list `MessageId` 0).
Happy-path first-seen 15 **MATCH**es the type-11 child, not a
list `+428` post.

Do **not** treat `0053D5C1` as “list posts 15”.
Do **not** route action 26 through `0054C59E` / `005403D2`
(`00540320` is a **different** widget: string `+356` / circular
`+352`; not type 12).

---

## 8. UNREAD / proposed (do not apply here)

1. Dump `012491FC+4` / `01249224+584` before naming the list click.
2. Dump input `01230134+8` before claiming the list is on
   `0055CB10`.
3. `[def+545]` first-seen on
   `UI_FRONTEND_LIST_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE`.
4. Keep type 4 → action 26 → type-11 persist 15. Do not invent a
   list-owned 15.

## Do not invent

- List action 26 → message 15.
- `+428/+432` as the New Game id.
- Action 26 as “select first child”.
- A DIK / Return path onto this list.
- Lionhead name for CRC `0x53C644E4`.
