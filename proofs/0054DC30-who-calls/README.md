# Who first calls type-11 activate `0054DC30` after Press Start / New Profile / Main Menu?

Investigation only. No production `src/` edits.

Question: type-11 activate `0054DC30` is vtbl-only. Who
**first** calls it after Press Start / New Profile / Main
Menu? Not factory, not layout. Host leftover?

Authority: dump `Fable.exe` `0054DC30` / `0054DCC0` /
`0054DB50` / `0054DBC0` / `0054E0B0` in
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`;
`0053D540` / `0052C730` / `005339B0` / `0052CF40` in
`listing-00500000.txt`;
`0041DB1D` / `0042EA62` in `listing-00400000.txt`;
`005952D8` / `00595A06` / `0059672A` / `00596763` /
`00596917` / `0059697A` / `00598A1C` / `0059899A` in
`listing-00580000.txt`;
`e8.tsv` (**no** dest `0x0054DC30` / `0x0054DCC0` /
`0x0054DB50` / `0x0053D540`);
`src/Fable.Game/EngineLifecycle.cs`
(`AttachFrontendTree`, `BindNewProfileFromArmedTick`,
`CommitNewProfileFromArmedEdit`, `AttachFrontendMainMenu`,
`ArmType34Widgets`);
`src/Fable.Game/FrontendWidgetFactory.cs`;
`src/Fable.Game/FrontendInputMap.cs`;
`src/Fable.Game/FrontendLayout.cs`;
`proofs/0054DC30-first-call/README.md`;
`proofs/005331A0-first-site/README.md`;
`proofs/ui84-list-after-attach/README.md`;
`proofs/00598A1C-only-e5/README.md`;
`proofs/type11-subscribe-actions/README.md`;
`proofs/newgame-plus380-first/README.md`;
`proofs/type12-highlight-plus348/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **STALE**.

Do not re-prove the six-id list, `[CUIDef+545]`,
`0055BA20` list-register (no ids), or type 4 → `push 26`.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| First `.text` `E8` / `jmp` of `0054DC30`? | **None.** Vtbl-only. | **PROVEN** `e8.tsv`; listing INT3 pad |
| Press Start factory `00598A1C` / `0041DB1D` / ctor `0054E0B0`? | **No.** | **PROVEN** |
| Layout `vtbl+172` (`0054DB50` / `0054E4B0` / `005339B0` / Main Menu `00595ACC`)? | **No.** | **PROVEN** bodies |
| New Profile switch `00596917` / `00596763`? | **No.** Old current `vtbl+192(6)` + input `vtbl+20` + `005952D8` DFS. | **PROVEN** |
| Push current `0059672A` (Press Start onto `[ui+32]`)? | **No.** Input `vtbl+8(root+4)` + inner `vtbl+24` + `0059B61C`. | **PROVEN** |
| Main Menu attach `0059697A` / `00595A06` / `00595B24`? | **No.** Factory + root `vtbl+172` + labels. **No** `00596763`. | **PROVEN** |
| Type-12 child `vtbl+192(3)` **is** `0054DC30`? | **No.** SelectState on the button. `0054DC30` **calls** `+192(3)`. | **PROVEN** |
| Host leftover is the first caller? | **No.** Leftover is `AttachFrontendTree` `Clear`+`Build` (rebuild). C# never enters / analogs `0054DC30`. | **DISPROVEN** as caller; rebuild **LEFTOVER** |
| First recovered native caller after that sequence? | **None recovered.** Slot dword **UNREAD**. | **UNREAD** |

**Answer: nobody recovered.** After Press Start populate,
New Profile `00596763`, and Main Menu `00595A06`, every
walked site still misses `0054DC30`. Dispatch stays a
**vtbl** call whose slot on `01249554` is **UNREAD**.
Host leftover on this sequence is tree **rebuild**, not
activate.

`0054DC30-first-call` “later show **UNREAD**” is
unchanged for the **slot**. This pass **DISPROVEN**s
the remaining named attach/switch helpers as that
first site, and **DISPROVEN**s host leftover as a
caller.

---

## Verdict

| Claim | Status |
| --- | --- |
| `0054DC30` `ecx` = type-11 **outer**; 0-arg `ret` | **PROVEN** |
| If `[def+545]`: `vtbl+192(3)` then inner `vtbl+12` **26, 31, 28, 27, 32, 29** | **PROVEN** |
| If `[def+545]==0`: skip map **and** `vtbl+192(3)` | **PROVEN** |
| Twin `0054DCC0` erases the same six | **PROVEN** |
| `.text` `E8` / `jmp` `0054DC30` | **DISPROVEN** (empty) |
| Factory / ctor / `0041DB1D` calls it | **DISPROVEN** |
| Layout `vtbl+172` calls it | **DISPROVEN** |
| `0059672A` / `00596763` / `005952D8` call it | **DISPROVEN** |
| `00595A06` / `0059697A` / `00595B24` call it | **DISPROVEN** |
| `0052CF40` / list `vtbl+192(3)` **is** `0054DC30` | **DISPROVEN** |
| Type 11 uses enable `0055AEB0` on this path | **DISPROVEN** (`0055AEB0` is type 34/38; `e8` from `00557863` / `00557883` / `0055A5B0` only) |
| Exact `01249554` dword for `0054DC30` | **UNREAD** |
| Host `AttachFrontendTree` on `00596917` / `7A` is native | **DISPROVEN** leftover rebuild |
| Host leftover **calls** / local-maps the six ids | **DISPROVEN** (`FrontendWidgetFactory` / `FrontendInputMap` / `FrontendLayout` have no analog) |
| First-seen show-walk that dispatches the activate slot | **UNREAD** |

---

## 1. Still vtbl-only

`e8.tsv` dest `0x0054DC30`: **empty**. Same for
`0054DCC0`, layout `0054DB50`, type-8 analog
`0053D540`.

`listing-00540000.txt`: `0054DC23 ret 4` then INT3
through `0054DC2F`. No `jmp 0054DC30`.

```
0054DC30  push ecx / ebx / esi
          mov  esi, ecx              ; outer
          call [eax+432]             ; this CUIDef*
          mov  bl, [edx+545]
          test bl, bl
          je   0054DCB2
          push 3
          call [edx+192]             ; SelectState(3)
          add  esi, 4                ; inner
          push 26 / 31 / 28 / 27 / 32 / 29
          call [inner.vtbl+12]       ; 0052DA20 insert
0054DCB2  pop  esi / ebx / ecx
          ret
```

Ctor `0054E0B0` → `0055B460` → `0055BA20` (list node,
**no** local map). Still no `E8 0054DC30`.

Enable `0055AEB0` **does** have `E8`s
(`00557863`, `00557883`, `0055A5B0`) but those are
type-40 / type-35 wraps, **not** type 11, and none
of those types sit on Press Start / New Profile /
Main Menu (`0055ACF0-first-caller`).

---

## 2. Press Start — not factory, not current-push

`0042EA62 call 00598A1C`. First factory:

```
00598BA2  push "UI_FRONTEND_PRESS_START_MENU"
00598BB7  mov  [ebp+108], 0x14
00598BD2  call 0041DB1D
```

`0041DB1D` ends `vtbl+332` (name). Type-11 child ctor
is §1. Tail of `00598A1C` (`[ui+192]==0`):

```
00599CAE  mov  [ebp+124], 0x14
00599CDA  call 0059672A
```

```
0059672A  call 0041E5F2
          call [input.vtbl+8]        ; root+4 or 0
          call [inner.vtbl+24]
          call 0059B61C              ; push [ui+32]
          ret 4
```

No activate. After return (`0042EA67`) next is
`009BFF40`, not a widget show.

---

## 3. New Profile — switch, not factory, not activate

`0xE5` → `00596917`:

```
00596921  push 23                    ; slot 0x17
          call 0059B5D7              ; already-built cell
          call 00596763
          call 00851700 / 00851770   ; edit box
```

`00596763` (`00596763`–`0059686A` `ret 8`):

| Site | Call | `0054DC30`? |
| --- | --- | --- |
| `0059677C` / `0059680A` | `005952D8` | **no** — `vtbl+260` type-id; type 37 `vtbl+600`; `vtbl+212` recurse |
| `005967BD` | `[0x13B8394].vtbl+184` | **no** — audio (`push 0x100` + floats), only if incoming == slot `0x1A` |
| `005967C9` | current `vtbl+192(6)` | **no** — SelectState 6 on the **old** current |
| `005967DC` | input `vtbl+20` | **no** — unregister old inner |
| `00596834` | input `vtbl+168` | **no** |

Slot `0x17` was factory-filled earlier in the **same**
`00598A1C` (`00598FD0`). No second `0041DB1D`. No
`vtbl+172`. No `0054DC30`.

---

## 4. Main Menu — factory + layout only

`0x126` → `0059697A` (`0059697A`–`00596A65` `ret 4`):

```
0059698D  call 004067C0              ; can-create
00596A36  push "UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE"
00596A49  call 00595A06
00596A5B  call 00595B24
```

**No** `00596763`. Current `[ui+32]` is **not**
rewired here.

`00595A06` (`ret 4`):

```
00595AB3  call 0041DB1D              ; factory into slot 0
00595ACC  call [eax+172]             ; **root** layout
          ret 4
```

Root layout is type-10 `0054E4B0` → `0052C730` →
`005339B0` child `vtbl+172`. Type-11 body on that
slot is `0054DB50` (`+545` → `0055AC90` else
`0052C730`; copy `+48` → `+404`). **No** `E8`
`0054DC30`. Identity `01249554+172 == 0054DB50`
stays **PARTIAL**.

`00595B24` is label / `UI_TEXT_NEW_GAME` setup
(`vtbl+576` / `+236` / `+328`), not activate.

`0059899A` empty-continue path also ends a **different**
builder with `0059894A call [eax+172]` only.

Type-12 attach `0054D660` writes `+348=0` and does
**not** child `vtbl+192(3)`
(`type12-highlight-plus348`). Even later nav
`0054C59E` is SelectState on the **button**, which
`0054DC30` itself also invokes — it is **not**
`0054DC30`.

---

## 5. Host leftover is rebuild, not activate

Native vs C# on this sequence
(`005331A0-first-site`, `ui84-list-after-attach`,
`00598A1C-only-e5`):

| Site | Native | Host | Activate? |
| --- | --- | --- | --- |
| Press Start | `00598A1C` factory slots; `0059672A` current | `AttachFrontendTree` + `WriteType10AttachMessage` | neither |
| New Profile | `00596917` switch already-built `0x17` | `BindNewProfileFromArmedTick` → **`Clear`+`Build`** | neither |
| Main Menu | `00595A06` overwrite slot 0 + `+172` | `CommitNewProfileFromArmedEdit` → **`Clear`+`Build`** | neither |

`AttachFrontendTree` leftover is a **second** persist
walk (`005331A0` analog). Factory still does not call
`0054DC30` (§2). Layout analog `ApplyFrontendScaleInit`
/ `LayoutFrontendWidgets` notes `0054E4B0` /
`0052C730` / `005339B0` — the **layout** fork, not
activate.

`FrontendWidgetFactory`, `FrontendInputMap`, and
`FrontendLayout` have **no** six-id local map and
**no** `0054DC30` constant. `ArmType34Widgets` writes
type-11/38 `Armed` (`+352` / `+364` stand-in) — a
**different** leftover (`type11-plus352-select`:
activate does **not** write `+352`).

Do not treat host rebuild or `ArmType34Widgets` as
proof that `0054DC30` ran.

`0054DC30-first-call` §6 still holds: do **not**
insert 26/31/28/27/32/29 in factory or attach.

---

## 6. What stays UNREAD

- `01249554` slot dword for `0054DC30` (and the
  type-8 twin `0053D540`).
- Any later first-seen walk (pointer `0055BF10`,
  list nav `vtbl+192(3)`, or an unread 0-arg vtbl
  DFS) that actually dispatches that slot.
- Whether first-seen New Game `[CUIDef+545]` is 1
  (INVISIBLE is 1; if the slot ever ran, that
  button **would** map).

---

## Do not invent

- An `E8 0054DC30` from ctor, `0041DB1D`,
  `00598A1C`, `0059672A`, `00596763`, `00596917`,
  `00595A06`, `0059697A`, or `00595B24`.
- Layout `vtbl+172` / `0054DB50` as activate.
- `0052CF40` / list highlight `vtbl+192(3)` as
  **being** `0054DC30`.
- Host `Clear`+`Build` as a native activate.
- Exact `01249554` slot dword without an rdata
  dump (`ExeIndex vtbl 0x01249554 160`).
