# Type-12 highlight `+348` into `+356` (`vtbl+192` 3 vs 4)

Investigation only. No production `src/` edits.

Authority: `Fable.exe` listings
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00500000.txt`
(`0053B63E` / `0053B662` / `0053822B` / `0053D200` / `0053D3B0` /
`0052C730` / `0052CF40`) and
`listing-00540000.txt` (`0054C3A0` / `0054CBF0` / `0054C59E` /
`0054C95E` / `0054D660`);
`00486024` in `listing-00480000.txt`;
`proofs/list-type12-focus/README.md`;
`proofs/type11-msg15/README.md`;
`proofs/who-posts-15/README.md`;
`implementer/frontend/14-container.md`, `01-widget-construction.md`.

Status: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Do not re-prove type 4 → action 26, or that persist 15 lives on
`UI_FRONTEND_BUTTON_NEW_GAME`.

---

## Verdict

| Claim | Class |
| --- | --- |
| Type 12 ctor `0054C3A0` is type 8 `0053B63E` then vtbl `01249224` / inner `012491FC` | **PROVEN** |
| Type 8 ctor `0053B662` `xor eax; mov [esi+348], eax` | **PROVEN** |
| Same ctor zeros vector `+356/+360/+364` then `0053822B` fills `+356` | **PROVEN** |
| Highlight child is `[[esi+356] + [esi+348]*4]` | **PROVEN** |
| Nav `vtbl+192(4)` old / `vtbl+192(3)` new | **PROVEN** |
| Type 12 `vtbl+192` is `0054CBF0` (overrides type 8 `0053D3B0`) | **PROVEN** |
| Attach/layout writer of `+348` is type 12 `0054D660` `mov [esi+348], 0` | **PROVEN** |
| `0053D200` writes list `+348` | **DISPROVEN** |
| First-seen Main Menu `+348 == 0` | **PROVEN** |
| `+356[0]` is persist child 0 `UI_FRONTEND_BUTTON_NEW_GAME` | **PARTIAL** (child 0 is NEW_GAME **PROVEN**; `def+148` filter **PARTIAL**) |
| First-seen New Game already has `vtbl+192(3)` | **PARTIAL** (index 0 is set; attach does **not** call `vtbl+192(3)`) |

**Answer:** first-seen Main Menu list `+348` is **0**. That names
`+356[0]`. Persist child 0 of this list is New Game. Attach writes
the index in type 12 `vtbl+172` `0054D660`, after the type 8 ctor
already zeroed it. That is the highlight **index**, not a recovered
first-seen `vtbl+192(3)` on the button.

---

## 1. Type 12 ctor does not write `+348`

```
0054C3A0  mov eax, [esp+4]
          push esi
          push eax
          mov esi, ecx
          call 0053B63E          ; type 8, size 0x1FC
          mov [esi],     01249224
          mov [esi+4],   012491FC
          mov [esi+24],  012491F4
          mov eax, esi
          pop esi
          ret 4
```

No store of `+348` / `+356` in the type-12 tail. Those slots are
type 8’s.

---

## 2. Type 8 ctor zeros `+348` and the `+356` vector

```
0053B63E  call 0052CC50          ; type 5 → type 4 → 005331A0 children
          xor eax, eax
          mov [esi],     012462E4
          mov [esi+4],   012462BC
          mov [esi+24],  012462B4
0053B662  mov [esi+348], eax     ; highlight index = 0
          lea ecx, [esi+356]
          mov [ecx], eax         ; begin
          mov [ecx+4], eax       ; end
          mov [ecx+8], eax       ; cap
          …
          call 0053822B          ; fill +356 from +176
```

`0052CC50` has already run `005331A0`, so persist children exist on
`+176` before the vector is rebuilt.

`0053822B` (end of type 8 ctor):

```
; eax = CUIDef*
; edi = child index in +176
mov ebx, [eax+152]           ; def+152 end
lea ecx, [ebp-12]            ; key = current index (first 0)
push ecx
mov ecx, [eax+148]           ; def+148 begin
mov edx, ebx
call 0053C8F8                ; find (00535F5A)
cmp eax, ebx
mov eax, [[esi+176]+edi*8]   ; child*
push &child
jne  → lea ecx,[esi+368] / 00486024
fall → lea ecx,[esi+356] / 00486024
```

`00486024` is `vector<void*>::push_back` (`[ecx+4]` vs `[ecx+8]`,
store `*[arg]`, `add [ecx+4], 4`).

So `+356` is a **dword pointer vector** of a subset of `+176`.
Miss in `def+148..+152` → `+356`. Hit → `+368` (not the highlight
walk). Live append `005383DD` also `push_back`s onto `+356`.

`def+148` contents on
`UI_FRONTEND_LIST_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE` are
**UNREAD**. Empty find returns end → every persist child lands in
`+356` in `+176` order.

---

## 3. Highlight is `+348` into that vector

Prev (`0054C59E`, after the empty-vector early-out):

```
child = [[esi+356] + [esi+348]*4]
push 4
call [child.vtbl+192]        ; unhighlight old
dec [esi+348]
jns keep
eax = ([esi+360]-[esi+356])>>2
dec eax                      ; wrap to last
mov [esi+348], eax
push 3
call [new.vtbl+192]          ; highlight new
```

Next (`0054C95E`):

```
old.vtbl+192(4)
inc [esi+348]
if index >= count: index = 0
new.vtbl+192(3)
```

`+360` is vector end (`sar 2` = pointer count). This is **not**
`+332` (style; first-seen 0 from `0052C730`) and **not** type-11
`widget+352` (action-26 click gate).

---

## 4. `vtbl+192` states 3 vs 4

Type 8 `0053D3B0` (`sub eax,3` / `dec eax`):

| Arg | Site | Effect |
| ---: | --- | --- |
| 3 | `0053D447` | parent-attach `+176`; `vtbl+524([+424])`; `0052CF40(3)` |
| 4 | `0053D3D5` | `vtbl+524([+436])`; `0052CF40(4)`; if `def+545` then `vtbl+576` and `[+352]=0` |
| else | `0053D3C7` | `0052CF40(arg)` |

Type 12 **replaces** that slot. `0054CBF0`:

```
cmp eax, 6
ja  0054D143                 ; 0052CF40(arg)
jmp [0x54D154+eax*4]
```

Recovered arms (bodies, not rdata listing):

| Arg | Site | Effect |
| ---: | --- | --- |
| 0 | `0054CC48` | input `vtbl+20` unregister walk; `0052CF40(0)` |
| 1 | `0054CC0A` | input `vtbl+8` register list inner; inner `+12(0,1)`; `0052CF40(1)` |
| 3 | `0054D056` | register list + every `+356` inner; each `+12(25)`; **`+356[+348]`** inner `+12(22)` then `+12(4)`; `0052CF40(3)` |
| 4 | `0054CCEE` | unregister walk; `0052CF40(4)` |
| 5 | `0054CD8E` | `+368` style walk; `0052CF40(5)` |
| 6 | `0054CF94` | unregister; `0052CF40(0 or 6)` from `[+324]` |

`0052CF40` writes `+332 = arg` and forwards `vtbl+188` to own
`+176` children (type-8 children skipped when parent state is
1/3/4). List **navigation** does not go through `0052CF40` on the
row: it calls the **child** `vtbl+192` with 3 or 4.

Type 11 activate `0054DC30` is `push 3; call [vtbl+192]` then
inner `+12(26,31,28,27,32,29)`. Deactivate uses 4 and `+16`.
That is the same 3 = selected / 4 = not.

---

## 5. Who writes `+348` at attach

Frontend attach `00595A06`: factory `0041DB1D` then root
`vtbl+172`. Type 10 `vtbl+172` → `0052C730` → `005339B0` recurse
child `vtbl+172`.

Type 8 layout `0053B91E` calls `0052C730`, copies `[+48]` →
`+352`, walks `+356`. **No** `+348` store.

Type 12 layout `0054D660` (immediately before type-12 dtor
`0054DA70` that resets vtbl `01249224`):

```
0054D669  call 0052C730          ; +324/+328/+332 = 0
          xor ebp, ebp
          xor ebx, ebx
          ; walk +356 → +380
          mov [esi+352], [esi+48]
0054D6F1  mov [esi+348], ebp     ; ebp is still 0
```

Then optional extra widgets from `def+308` / `def+304`. Those
paths do **not** assign a nonzero highlight index.

**Attach writer:** `0054D660` / `0054D6F1` forces `+348 = 0`.
**Earlier writer:** type 8 ctor `0053B662` (runs inside
`0054C3A0` during the same factory attach). No persist field
loads a saved row.

Other recovered `+348` writers on this object: nav
`0054C5BE`/`0054C5DC` and `0054C97F`/`0054C9A4` only.

---

## 6. Dump `0053D200` — not a list-`+348` writer

```
0053D200  sub esp, 8
          mov esi, ecx           ; inner this = widget+4
          call 0041E5F2
          call [edx+44]
          …
          lea edi, [esi-4]       ; outer widget
          call [eax+432]
          mov bl, [def+545]
          test bl, bl
          je  skip
          push ebp               ; action
          mov ecx, esi
          call 0055AD60
```

Type 8 inner apply (subscribe site `0053D5C1` `push 26` /
`[inner+12]`). No `[list+348]` load or store. `0055AD60` reads
**inner** `+348` = **widget+352** (type 11/38 click gate), a
different slot.

Type 12 ctor **overrides** inner vtbl to `012491FC`. Whether slot
`+4` is still `0053D200` is **PARTIAL** (no rdata dump of
`012491FC`). Even if it is, that path does not set the list
highlight index.

---

## 7. First-seen Main Menu

Tree (`005331A0` persist `Children`):

```
UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE     type 10
└── UI_FRONTEND_LIST_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE   type 12
    └── [0] UI_FRONTEND_BUTTON_NEW_GAME                   type 11  persist 15
        … LOAD / OPTIONS / … siblings
```

First persist child is New Game (`who-posts-15`,
`list-type12-focus`). `names.bin` `CHANGE_PROFILE` before
`NEW_GAME` is **not** this list’s persist `[0]`.

First-seen:

1. Ctor `+348 = 0`.
2. `0053822B` builds `+356`. If index `0` is absent from
   `def+148` (empty vector → always), `+356[0]` = `+176[0]` =
   New Game.
3. Attach `0054D660` writes `+348 = 0` again. Does **not** call
   `vtbl+192(3)` on that child.
4. `0052C730` leaves list `+332 = 0`. `0054C430` only does
   `vtbl+192(3)` when `+324 == 5` (not first-seen).

So first-seen **index** is child 0 / New Game. First-seen
**state 3** on that button is only true after a later
`vtbl+192(3)` (list case `0054D056`, or nav). That call is
**UNREAD** on the Main Menu attach path.

---

## 8. C# vs native

| Site | Native | C# |
| --- | --- | --- |
| List `+348` | ctor 0 + attach `0054D6F1` 0 | no slot; `ActiveChild` only on type 18 | **LEFTOVER** |
| `+356` vector | persist subset of `+176` | all persist children stay visible | **PARTIAL** (no exclusive hide; **MATCH** vs type 18) |
| `vtbl+192` 3/4 | highlight / unhighlight | unused on type 12 | **LEFTOVER** |
| Action 26 | not gated on list `+348` | `MessageFromWidgets` first visible 10/11/38 id | leftover vs `list-type12-focus` |

Do **not** treat type-11 `+352` / inner `+348` as the list index.
Do **not** treat `0053D200` as the attach writer of list `+348`.

---

## 9. UNREAD

- `01249224+172` / `+192` rdata dwords (bodies recovered; slots
  inferred from type-12 block + `0054CBF0` switch).
- `012491FC+4` == `0053D200`?
- `CUIDef+148` on the Main Menu list (empty vs skip-set).
- First caller of list `vtbl+192(3)` after `00595A06`.
- Lionhead name for `def+148`.
