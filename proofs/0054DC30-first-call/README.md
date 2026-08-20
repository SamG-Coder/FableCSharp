# First `0054DC30` after Press Start factory is not attach/layout

Investigation only. No production `src/` edits.

Question: when is type-11 `0054DC30` (local-map **26 / 31 / 28 /
27 / 32 / 29**) first called after Press Start factory?
Attach/layout, or a later show?

Authority: dump `Fable.exe` `0054DC30` / `0054DCC0` / `0054DB50` /
`0054E0B0` / `0054E4B0` / `0054D660` in
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`;
`0052C730` / `005339B0` / `0053D540` / `0052CF40` in
`listing-00500000.txt`;
`0041DB1D` / `0042EA62` in `listing-00400000.txt`;
`00595A06` / `00596763` / `00598A1C` in `listing-00580000.txt`;
`e8.tsv` (`0055BA20`, `0055AEB0`; **no** `0054DC30` /
`0054DCC0` / `0054DB50` / `0053D540` / `0054E4B0`);
`proofs/type11-subscribe-actions/README.md`;
`proofs/005331A0-first-site/README.md`;
`proofs/cuidef-plus545/README.md`;
`proofs/type12-highlight-plus348/README.md`;
`proofs/newgame-plus380-first/README.md`;
`implementer/frontend/14-container.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**STALE**.

Do not re-prove type 4 → `push 26`, `0055BA20` ctor register,
`[CUIDef+545]` persist `0x9E47F106`, INVISIBLE file `+545==1`,
or the six-id list itself.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| First `.text` `E8` of `0054DC30`? | **None.** Vtbl-only. | **PROVEN** `e8.tsv` |
| During Press Start factory ctor? | **No.** Type-11 `0054E0B0` → `0055B460` → `0055BA20` (list node, **no** local map). | **PROVEN** |
| Factory post-hook `0041DB1D`? | **No.** `vtbl+332` (name), not `0054DC30`. | **PROVEN** |
| Same `00598A1C` insn after `"UI_FRONTEND_PRESS_START_MENU"` `0041DB1D`? | **No.** Store slot `0x14`, then factory `"UI_FRONTEND_PROFILES_MENU"`. | **PROVEN** |
| Attach/layout `vtbl+172`? | **No.** Type 10 `0054E4B0` → `0052C730` → `005339B0` child `vtbl+172`. Type 11 `0054DB50` is that slot’s body (layout fork). Type 12 `0054D660` does **not** call child `0054DC30` / `vtbl+192(3)`. | **PROVEN** bodies; type-11 `01249554+172` dword **PARTIAL** |
| Press Start populate calls root `vtbl+172` like Main Menu `00595A06`? | **No.** `00595ACC` is Main Menu only. `00598A1C` has no slot-`0x14` `+172`. | **PROVEN** |
| After `00598A1C` returns (`0042EA62`)? | **No** widget show. Next is `009BFF40`. | **PROVEN** |
| Later menu switch `00596763`? | **Not** first-seen Press Start. Old current `vtbl+192(6)` + unregister inner. Not `0054DC30`. | **PROVEN** as a later `0xE5` path; **DISPROVEN** as the first Press Start site |
| A later first-seen show / child `vtbl+192(3)` hits type-11 `0054DC30`? | **UNREAD** (no `E8`; slot **PARTIAL**) | **UNREAD** |

**Answer: later than attach/layout — not during Press Start
factory, and not on the recovered layout walk.** The six-id
local map is the **activate** body. First recovered caller of
that vtbl slot on the first-seen tree stays **UNREAD**.

`type11-subscribe-actions` “first-seen attach/layout **calls**
`0054DC30` = **DISPROVEN**” is unchanged. This pass pins that
to Press Start factory `00598A1C` / `0041DB1D` and to layout
`vtbl+172`, and leaves “later show” **UNREAD**.

---

## Verdict

| Claim | Status |
| --- | --- |
| `0054DC30` `ecx` = type-11 **outer**; 0-arg `ret` | **PROVEN** |
| If `[def+545]`: `vtbl+192(3)` then inner `vtbl+12` **26, 31, 28, 27, 32, 29** | **PROVEN** |
| If `[def+545]==0`: skip map **and** `vtbl+192(3)` (`je 0054DCB2`) | **PROVEN** |
| Twin `0054DCC0` erases the same six via `vtbl+16` / `vtbl+192(4)` | **PROVEN** |
| `.text` `E8 0054DC30` / `E8 0054DCC0` / `E8 0054DB50` | **DISPROVEN** (empty) |
| Type-11 ctor `0054E0B0` `E8`s `0054DC30` | **DISPROVEN** |
| `0055BA20` already maps 26/28 | **DISPROVEN** (`type11-subscribe-actions`) |
| First-seen INVISIBLE `[CUIDef+545]` | **1** (`cuidef-plus545`) — activate **would** map if it ran |
| Layout `0054DB50` / `0054E4B0` / `0054D660` / `005339B0` call `0054DC30` | **DISPROVEN** |
| Main Menu attach `00595A06` `vtbl+172` is the Press Start first site | **DISPROVEN** (different fn; after `0xE5` / empty continue) |
| `0052CF40` `vtbl+192` **is** `0054DC30` | **DISPROVEN** (`0054DC30` **calls** `+192(3)`) |
| Exact `01249554` dword for `0054DC30` | **UNREAD** |
| First-seen show-walk that dispatches that slot | **UNREAD** |

---

## 1. Dump callers of `0054DC30`

`e8.tsv` dest `0x0054DC30`: **empty**. Same for `0054DCC0`,
`0054DB50`, type-8 analog `0053D540`, type-10 layout
`0054E4B0`.

No `jmp 0054DC30` in `listing-00540000.txt`. Dispatch is a
**vtbl** call. Slot id **PARTIAL** (no `01249554` rdata dump;
`type34-vtbl588-rdata`).

Shape matches type-8/12 activate `0053D540` (also **no** `E8`):
get-def `vtbl+432`, `[def+545]`, then inner `vtbl+12` the same
six ids. Type 8 also tests parent type-8 (`vtbl+260==8`) before
the map; type 11 does **not**.

```
0054DC30  push ecx / ebx / esi
          mov  esi, ecx              ; outer
          call [eax+432]             ; this CUIDef*
          mov  bl, [edx+545]
          … COM-ptr release …
          test bl, bl
          je   0054DCB2              ; skip everything
          push 3
          call [edx+192]             ; SelectState(3)
          add  esi, 4                ; inner
          push 26 / 31 / 28 / 27 / 32 / 29
          call [inner.vtbl+12]       ; 0052DA20 insert
0054DCB2  pop  esi / ebx / ecx
          ret
```

Ctor `0054E0B0` (`listing-00540000.txt`):

```
0054E0B8  call 0055B460             ; type 34 → 0055BA20
          [esi]    = 01249554
          [esi+4]  = 01249530
          call 0054DF50             ; Action CRC vector
          ret 4
```

No `E8 0054DC30`. `0055BA20` registers `widget+4` on input
`vtbl+8` and **does not** `push 26` (`type11-subscribe-actions`).

---

## 2. Press Start factory does not call it

First-seen populate (`005331A0-first-site` / `00598A1C-only-e5`):

```
0042EA4C  call 005958F5
0042EA62  call 00598A1C          ; arg 0 → skip media error
```

`005958F5` is **before** any Press Start widget (zeros
`ui+152/+156`, profile names). It cannot enter `0054DC30`.

```
00598BA2  push "UI_FRONTEND_PRESS_START_MENU"
00598BB7  mov  [ebp+108], 0x14
00598BD2  call 0041DB1D          ; factory
00598BDA  mov  [ecx], eax        ; slot 0x14
00598BE5  push "UI_FRONTEND_PROFILES_MENU"
```

Factory:

```
0041DB41  call 009AD410          ; def by name
0041DB49  call 0041D21B          ; type switch → 0054E3D0 / children
0041DB57  call [eax+332]         ; name; ret 8 of 0041DB1D
```

Type 10 ctor `0054E3D0` → `0052CC50` → `005334A0` → **`005331A0`**
child walk (`0041D21B` each persist child). First type 11 on
that tree is `UI_FRONTEND_BUTTON_INVISIBLE` (`action26-subscribers`).
Its ctor is §1. **Still no `0054DC30`.**

After `00598A1C` returns:

```
0042EA67  mov  ecx, [0x13B8390]
          push 60 / 0 / 16
          call 009BFF40
          ret
```

No widget `vtbl` show.

The only `vtbl+172` **inside** `00598A1C` is `00599738`, on a
side list of extra `[node+20]` widgets after redefine-keys
setup — **not** slot `0x14` Press Start.

---

## 3. Layout `vtbl+172` is a different body

Main Menu attach **does** layout the root (`00595A06`):

```
00595AB3  call 0041DB1D
00595ACC  call [eax+172]         ; root layout
```

Press Start populate does **not** have that pair. First-seen
Press Start root `vtbl+172` site is therefore **UNREAD**.
Whenever it runs, the recovered bodies still miss `0054DC30`.

| Object | Recovered `vtbl+172` body | Calls `0054DC30`? |
| --- | --- | --- |
| Type 10 | `0054E4B0`: `0052C730`; `+348=[+48]`; optional `UI_ACCEPT` | **no** |
| Type 5/10 shared | `0052C730`: `005339B0`; `+324/+328/+332=0` | **no** |
| `005339B0` | dest fields; `+176` children: `vtbl+204` parent, **`vtbl+172` recurse** | **no** (still `+172`) |
| Type 12 | `0054D660`: `0052C730`; `+348=0`; no child `vtbl+192(3)` | **no** (`type12-highlight-plus348` §7) |
| Type 11 | `0054DB50`: `vtbl+432` / `[def+545]` → `0055AC90` else `0052C730`; copy `+48` → `+404` | **no** |

Type-11 `0054DB50` is the **layout** fork sitting immediately
before apply `0054DBC0` and activate `0054DC30`. Identity
`01249554+172 == 0054DB50` stays **PARTIAL**. The body is 0-arg
and never `E8`s `0054DC30`.

Type-12 nav that later does child `vtbl+192(3)` (`0054C59E`) is
**not** attach (`list-type12-focus`). Even that call is
SelectState on the **button**, which `0054DC30` itself also
invokes — it is **not** proven to *be* `0054DC30`.
`0052CF40` (type-8/12 `vtbl+192`) forwards children `vtbl+188`,
not inner `vtbl+12(26…)`.

---

## 4. Later show is not first-seen Press Start

`00596763` (slot switch, used from `00596917` after `0xE5`):

```
005967C9  call [eax+192]         ; current, push 6
005967DC  call [edx+20]          ; input unregister inner
… insert new current …
0059680A  call 005952D8          ; DFS type-id / child getter only
```

`005952D8` walks `vtbl+212` children and, for type 37, `vtbl+600`.
**No** `0054DC30`. First-seen Press Start never takes this switch
(`00598A1C` fills slots; current Press Start is already the
startup screen).

So: **not** attach/layout, and the recovered “show” helper is a
**later** `0xE5` path that still does not `E8` `0054DC30`.

---

## 5. First-seen INVISIBLE vs the six-id map

Press Start first `0055CB10` node is INVISIBLE type 11
(`action26-subscribers`). File `+545==1`, so **if** `0054DC30`
ran it would insert 26/31/28/27/32/29.

Ctor has already registered the inner. Apply `0054DBC0` still
runs on broadcast 26 when `+545≠0` (`cuidef-plus545`). The
local map is a **separate** accept-set (`0052DA20`), not the
`0055CB10` node. First-seen 26 reaching `0054DBC0` does **not**
prove activate already ran.

Do not treat factory / layout as having armed that map.

---

## 6. C# leftover

Host `FrontendInputMap` does not keep a per-widget local action
tree. Native type 11 accepts 26/31/28/27/32/29 on that map
**only after** a successful `0054DC30`. First-seen Press Start
ctor+layout recovered here never call it. Do not insert those
six ids in `FrontendWidgetFactory` / `AttachFrontendTree`.

---

## Do not invent

- `E8 0054DC30` from ctor, `0041DB1D`, `00598A1C`, or `00595A06`.
- Layout `vtbl+172` / `0054DB50` as activate.
- Main Menu `00595A06` `+172` as the Press Start first site.
- `0052CF40` / list highlight `vtbl+192(3)` as **being** `0054DC30`.
- Exact `01249554` slot dword without an rdata dump.
- Lionhead names for 31 / 32 / 29 beyond the already-proven
  type-4 / type-6 / type-10 producers (26 / 28 / 27).
