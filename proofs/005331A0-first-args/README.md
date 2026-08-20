# First-seen `005331A0` after Press Start factory: args / this / parent / `+302`

Investigation only. No production `src/` edits.

Question: `005331A0` child attach first-seen from Press Start
factory. What args / this / parent? Does it write `+302`
align bits? Host notes every child as `005331A0` after any
root — leftover vs only type-10/5/12/18 parent?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00500000.txt`
(`005331A0` / `005334A0` `005336F6` / `0052CC50` /
`0053B63E`);
`listing-00540000.txt` (`0054E3D0` / `0054C3A0` /
`00547600` / `0054ED90` `0054EEAF`);
`listing-00400000.txt` (`0041DB1D` / `0041D21B`
`0041D512`);
`listing-00580000.txt` (`00598A1C` / `00598BD2`);
`implementer/frontend/fn-005331A0-exact.txt`,
`fn-005334A0-exact.txt`, `fn-0052CC50-exact.txt`;
`export/frontend/persist-tail.txt`;
`tests/Fable.Formats.Tests/FrontendUiDefTests.cs`
(`Press_Start_remap_bits_come_from_def_520_521`);
`src/Fable.Game/EngineLifecycle.cs`
(`AttachFrontendTree`, `InitFrontendUi`);
`src/Fable.Formats/Defs/FrontendWidgetType.cs`;
`proofs/005331A0-first-site`;
`proofs/audit-frontend-leftover`.

Do not re-prove first-site timing vs `00596917`.
Do not invent dest pixels.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| First-seen `005331A0` args? | **None on the stack.** `thiscall`; `ret` (not `ret 4`). Def is already on the widget (`[this+304]`) and re-fetched by `vtbl+432`. | **PROVEN** |
| First-seen `this` (`ecx`)? | The **allocated type-10 widget** (`0x16C` from `0041D4FC`), still in `005334A0` / `0052CC50`. Type-10 vtbl `012497E4` is written **after** return (`0054E3DF`). | **PROVEN** |
| First-seen parent? | **No widget parent.** This is the factory root. Factory owner is `ecx` of `0041DB1D` (`0041E5F2` return), not `ecx` of `005331A0`. | **PROVEN** |
| Does `005331A0` write `+302` **align** bits 3/4/5 (`0x08`/`0x10`/`0x20`)? | **No.** It only `or`s `0x01` / `0x02` / `0x40` / `0x80`. Align is type-6 `0054ED90` from `[def+508]`. | **DISPROVEN** |
| First-seen root `+302` from this call? | **`or 0x40` (bit 6)** from persist `+520=1`. Bit 7 (`+521`) is 0. Align bits stay 0 here. | **PROVEN** bit 6; align **DISPROVEN** |
| Host notes every child as `005331A0` after **any** root? | **Yes, leftover.** `AttachFrontendTree` after Press Start / New Profile / Main Menu. Native first-seen `this` is the **parent being constructed**, not a post-build child tag. `00596917` does not re-run it. | **LEFTOVER** |
| Only type-10/5/12/18 parent? | **No.** Those types do run it (Press Start root + list/swap/group), but so do type 0 / 6 / 32 / … via `0052CC50` → `005334A0`. `DrawsChildList` is draw, not attach. | **DISPROVEN** |

---

## Verdict

**First-seen `005331A0` is `thiscall` on the new Press Start
type-10 object, zero stack args, no parent widget.** It
writes remap/centre/clip bits on `+302`, **not** type-6
align. Host per-child notes after every root are leftover.

| Claim | Status |
| --- | --- |
| `005336F6` / `00533982` are the only `.text` `E8 005331A0` | **PROVEN** (first-site `e8.tsv`) |
| First-seen site is `005336F6` inside `005334A0` during `0054E3D0` | **PROVEN** |
| Factory `0041DB1D` first-seen: name + flag **0** | **PROVEN** `00598A2A xor ebx` / `00598BC6 push ebx` |
| `005331A0` stack args | **PROVEN** none |
| `this` = type-10 alloc, not UI / not child | **PROVEN** |
| Widget parent on first call | **PROVEN** none (root) |
| `+302` bits 4/5 from `005331A0` | **DISPROVEN** |
| First-seen root `+520` → `+302` bit 6 | **PROVEN** persist + listing |
| Host every-child `Note(ChildAttachFn)` after any root | **LEFTOVER** |
| Restrict notes to type-10/5/12/18 parents | **DISPROVEN** as native rule |
| Factory walk analog `FrontendWidgetFactory.Build` | **MATCH** |

---

## 1. Factory args (`0041DB1D`) then ctor (`0054E3D0`)

`00598A1C` first-seen arg 0 skips media error
(`cmp [ebp+124], bl` / `je 00598B90`). Then:

```
00598A2A  xor ebx, ebx
…
00598BA1  push edi
00598BA2  push "UI_FRONTEND_PRESS_START_MENU"
…
00598BB7  mov [ebp+108], 0x14
…
00598BC6  push ebx                 ; flag = 0
00598BC7  lea eax, [ebp+112]
00598BCA  push eax                 ; name
00598BCB  call 0041E5F2
00598BD0  mov ecx, eax             ; factory this
00598BD2  call 0041DB1D
```

`0041DB1D` (`ret 8`):

```
0041DB20  cmp [ebp+12], 0x00       ; flag
0041DB25  push [ebp+8]             ; name
0041DB28  mov esi, ecx             ; this = UI owner
0041DB2A  je 0041DB33              ; flag 0 → lookup
0041DB41  call 009AD410            ; def
0041DB46  mov ecx, esi
0041DB48  push eax
0041DB49  call 0041D21B
0041DB61  ret 8
```

`0041D21B` type 10 (`[def+60]==10`):

```
0041D4FC  push 0x16C
0041D501  call 00BFEA1A
0041D50F  push edi                 ; def
0041D510  mov ecx, eax             ; this = new widget
0041D512  call 0054E3D0
```

`0054E3D0` (`ret 4`):

```
0054E3D0  mov eax, [esp+4]         ; def
0054E3D6  mov esi, ecx             ; this
0054E3D8  call 0052CC50
0054E3DF  mov [esi], 0x12497E4     ; type-10 vtbl AFTER child walk
0054E408  ret 4
```

`0052CC50` pushes the same def and `call 005334A0`.

---

## 2. First-seen `005331A0`: `this` / args / parent

`005334A0` zeros `+302` then calls with **`ecx = esi`**
(the widget). Def is `[esp+32]` into `[esi+304]`:

```
005336C4  mov ecx, esi
005336D2  mov [esi+302], bl        ; 0
005336DE  mov [esi+304], edx       ; def
005336F6  call 005331A0
00533712  ret 4
```

`005331A0` (`fn-005331A0-exact.txt`):

```
005331A5  mov ebx, ecx             ; this
005331B6  call [eax+432]           ; get def
…
00533493  ret                      ; no stdcall pop
```

No `push` of a parent, child index, or name at `005336F6`.

| Slot | First-seen value |
| --- | --- |
| `ecx` / `this` | type-10 alloc (still type-4/5 vtbls) |
| `[esp+4]` | **not an arg** (`ret`) |
| widget parent | none — this **is** the Press Start root |
| factory owner | `0041DB1D` `ecx` (`0041E5F2` return) |
| persist def | `009AD410("UI_FRONTEND_PRESS_START_MENU")` via `vtbl+432` |

`005331A0` then walks `[def+112]..[+116]` and
`0041D21B` each persist child. Those later hits are
nested **inside** the same factory, with `this` = that
child (its own ctor). They are not a second attach pass.

---

## 3. `+302`: remap bits yes, align bits no

`005331A0` (`listing-00500000.txt`):

```
00533288  or [ebx+302], 0x01       ; [def+392]
00533298  or [ebx+302], 0x02       ; [def+188] centre
005332A8  or [ebx+302], al         ; al=0x40 from [def+520]
005332B8  or [ebx+302], dl         ; dl=0x80 from [def+521]
```

No `0x08` / `0x10` / `0x20`.

Type-6 align is **`0054ED90`**, after this walk already
constructed a type-6 child:

```
0054EEAF  mov eax, [ecx+508]
0054EEB5  sub eax, 0
0054EEB8  je 0054EED4              ; or 0x08 left
0054EEBA  dec eax
0054EEBB  je 0054EECA              ; or 0x10 centre
0054EEBD  dec eax
0054EEBE  jne 0054EEE2
0054EEC0  … or al, 0x20            ; right
```

`0054FFF0` **reads** bits 4/5. Other writers
(`0053824F` / `00547523`) are not this first-seen
type-10 call.

First-seen root persist (`FrontendUiDefTests` /
`persist-tail`): `+520=1`, `+521=0`. So this first
`005331A0` does `or [root+302], 0x40` and does **not**
set align bits. Do not treat that `0x40` as centre/right.

---

## 4. Not only type 10 / 5 / 12 / 18

Press Start persist children include type 5 / 18 / 12 /
6 / 32 (`press-start-frame`). Their **parents** are type
10 (and nested 5/12/18). That is the forest, not a
restriction on who **calls** `005331A0`.

Callers of `005334A0` / `0052CC50` (hence `005331A0`):

| Type | Ctor | Chain |
| --- | --- | --- |
| 4 | `005334A0` | direct `005336F6` |
| 5 | `0052CC50` | → `005334A0` |
| 10 | `0054E3D0` | → `0052CC50` |
| 12 | `0054C3A0` | → `0053B63E` → `0052CC50` |
| 18 | `00547600` | → `0052CC50` |
| 0 | `0041B800` | → `0052CC50` |
| 6 | `0054F5C0` | → `0052CC50` (`listing-00540000.txt` `0054F5CA`) |

`FrontendWidgetType.DrawsChildList` is **draw**
`vtbl+8==00530260` on 5/10/12/18. It is not “who
may run `005331A0`.” A type-6 / type-0 ctor still
runs `005331A0` on **itself**; an empty
`[def+112]` walk is a no-op.

So “only note `005331A0` when parent is 10/5/12/18”
is **not** the listing rule.

---

## 5. Host leftover

`FrontendWidgetFactory.Build` / `AttachChildren` is the
first-seen walk analog (**MATCH**): persist indices →
`0041D21B` types, same for every root name.

After `Build` returns, `AttachFrontendTree` notes every
non-root:

```
Note(ChildAttachFn, … "005331A0 child {name} type {type}")
```

Callers: `AttachPressStartWidgets`,
`BindNewProfileFromArmedTick` (`00596917` analog),
`AttachFrontendMainMenu`, commit-main-menu. No type
gate.

That is leftover for three independent reasons:

1. **Subject.** Native first-seen `this` is the **root**
   (then each constructed parent). The note tags the
   **child name**.
2. **When.** Native walk finishes at `005336F6`, before
   `0054E3DF` vtbls and before dest layout. Host also
   notes `"005331A0 children=N"` in `InitFrontendUi`
   **after** `LayoutFrontendWidgets`.
3. **Where.** Native `00596917` switches slot `0x17`;
   it does not factory and does not `E8 005331A0`.
   Rebuild notes on New Profile / Main Menu are leftover.

A host filter “only 10/5/12/18 parents” would still be
wrong (type 0/6 also run the fn). If notes stay, emit
them from the factory walk on first construct, with
`this` = the widget whose ctor is in `005334A0`.

---

## Do not invent

- Stack args on `005331A0`.
- Align bits 4/5 from this fn.
- Pixel dest tables (not this question).
- `005331A0` only on `DrawsChildList` types.
- A third `.text` call site.
- First-seen `this` = child `UI_PRESS_START_TEXT`.
