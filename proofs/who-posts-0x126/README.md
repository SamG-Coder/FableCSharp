# How `0x126` reaches `0059A238`

Investigation only. No production `src/` edits.

Authority: `Fable.exe` listings
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`,
`listing-00580000.txt`, `listing-00600000.txt`, `listing-00840000.txt`;
inflated `frontend.bin` via `GameInstall` (`FrontendUiDefTests`);
`implementer/frontend/05-input.md`;
`src/Fable.Game/FrontendInputMap.cs`, `FrontendMessages.cs`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

---

## Verdict

| Claim | Class |
| --- | --- |
| `0059A238` double-derefs the posted pair; `0x126` → `00851920` | **PROVEN** |
| No `.text` `mov […], 0x126` | **PROVEN** |
| File `UI_ACCEPT_NEW_PROFILE` type **38** stores i32 `0x126` after CRC `0x53C644E4` | **PROVEN** |
| That CRC is CUIDef persist `+224` (`00631C60` → `00632500`) | **PROVEN** (slot + file value); name **UNREAD** |
| Type-38 ctor `00558B90` → `0055B460` → `0055B040` copies `[def+224]` through **vtbl+284** | **PROVEN** |
| Type 38 does **not** store the id at widget+352 the way type 10 does | **PROVEN** |
| User post is action **26** on the type-38 inner (`0055AD60`), not type-10 `0054E280` | **PROVEN** site; last hop **PARTIAL** |
| Type-10 New Profile `0054E280` posts a **child’s** stored `0x126` | **DISPROVEN** |
| Type-0 button / generic `0122F5D4+284` `0052F040` | **DISPROVEN** (`ret 4`) |
| `00851770` posts `0x126` | **DISPROVEN** (binds type-37 name only) |
| `00851920` consumes `0x126` (nonempty trim → `[ui+96+5]=1`) | **PROVEN** |
| Return / type 1 / action 33 posts `0x126` | **DISPROVEN** |
| Physical device that builds type 4 | **UNREAD** |

**Path:** persist `0x53C644E4` → def `+224` = `0x126` → type-38 `0055B040` heap object + vtbl+284 → type-4 / action 26 → `0055AD60` → (widget vtbl+584) → `0041E6D3` or UI vtbl+32 → `0059A238` → `00851920`.

---

## 1. Consumer (`0059A238`)

```
0059A281  mov eax, [ebp+8]     ; arg = pointer to pair
0059A284  mov eax, [eax]       ; pair.ptr
0059A286  mov ecx, [eax]       ; [heap] = message id
…
0059A6AB  mov edx, 0x127
0059A6B0  cmp ecx, edx
0059A6BE  sub ecx, 0xE5        ; 0xE5 → 00599D5C
…                              ; 0x124 → 0059899A
0059A6DE  dec ecx
0059A6DF  jne 0059A7FF
0059A6E5  mov esi, [esi+96]    ; id == 0x126
0059A6F2  call 00851920
```

`0xE5 + 21 + 33 + 9 + 1 + 1 = 0x126`. Guard: `[ui+96] != 0`.

`00851920` is **not** a poster. It trims the type-37 name (`00851890`) and, if length > 0 and `[this+5]==0`, writes `[+5]=1` `[+4]=0`. Next `00599E3F` can run `0059697A`.

---

## 2. Persist field (the id is not an immediate)

`00631C60` (CUIDef persist):

```
00631FBD  lea edx, [esi+224]
00631FC6  call 00632500        ; 4-byte load after 00404500 CRC skip
00631FCB  lea eax, [esi+228]
00631FD4  call 00632500
00631FD9  lea ecx, [esi+232]
00631FE2  call 00632500
00631FE7  lea edx, [esi+236]
00631FF0  call 00632500
```

File form is CRC + i32 (`00431102` / `00632500` on load).

| Widget | Type | CRC | i32 |
| --- | ---: | --- | ---: |
| `UI_ACCEPT_NEW_PROFILE` | 38 | `0x53C644E4` | **`0x126`** |
| `UI_FRONTEND_BUTTON_NEW_GAME` | 11 | `0x53C644E4` | **15** |
| same type-11 | 11 | `0xF1A22807` (`Action`) | 15 (second slot `+228`, unused in C#) |
| Press Start type-11 `UI_FRONTEND_BUTTON_INVISIBLE` | 11 | `0xF1A22807` | `0xE5` (229) |

`FableCrc("Message")` and `FableCrc("MessageId")` are **not** `0x53C644E4`. English name **UNREAD**.

`FrontendUiDef.MessageIdCrc` / `ReadPersistI32` / test
`Persist_00631C60_plus189_plus190_are_u8_and_font_is_names_offset`.

---

## 3. Type 38 ctor does not write widget+352 as the id

| VA | Role |
| --- | --- |
| `00558B90` | Type 38 ctor. `call 0055B460`, vtbl `0124B04C` / inner `0124B024` |
| `0055B460` | Type 34 ctor. `call 0055BA20` then `0055B040`. Size `0x194` |
| `0055BA20` | Type 33. Zeros **`+348` dword**, **`+352` byte**, `+356`, `+360`. Subscribes inner via `0041E5F2` vtbl+8 |

Type-33 `+352` is a **flag byte**, not a message pair.

`0055B040` (ecx = widget):

```
call [vtbl+432]            ; def*
mov ecx, [def+224]
je skip                    ; 0 = no message
alloc 16 / 0042BE50
0042AA29                   ; pair
mov [heap], [def+224]      ; heap[0] = 0x126  (no imm)
call [vtbl+284]            ; store pair
; then def+228 → vtbl+320
;     def+232 → vtbl+288
;     def+236 → vtbl+292
```

That is why there is no `mov […], 0x126` in `.text`.

Type-38/34 vtbl+284 is **not** `0054E4F0` and **not** generic `0052F040`. Exact type-38 slot function **UNREAD** here (rdata `0124B04C+284` not in the 01200000 listing). It is **not** a store at widget+352: type 33 already used `+352` as a u8.

---

## 4. Type 10 +352 is a different object (Press Start `0xE5` only)

Type-10 ctor `0054E3D0`: outer vtbl `012497E4`, **inner at +4** `012497BC`, zeros `+352/+356/+360`.

`0054E4F0` (`012497E4+284`, ecx = **widget**): copies the pair to **widget+352 / +356**.

`0054E280` (`012497BC+4`, ecx = **widget+4**):

```
0054E2FA  mov eax, [edi+348]     ; widget+4+348 = widget+352
0054E303  lea esi, [edi+348]
0054E315  call [edx+32]          ; 0059A238(&widget+352)
```

Docs that say “posts widget+352” are correct **from the widget base**. The listing’s `+348` is the inner this-adjust.

Attach writer `00598EE6` `mov [eax], 0xE5` then slot `0x14` vtbl+284. That is Press Start only. New Profile / Main Menu type-10 roots are **not** written `0xE5` and do not persist `0x126`.

`0054E4B0` looks up a child named **`"UI_ACCEPT"`** into type-10 `+360`. That string is not `UI_ACCEPT_NEW_PROFILE`. New Profile action 26 therefore cannot post the accept widget’s persist id through `0054E280`.

---

## 5. Who posts `0x126` (action, not type-10)

`0042E3EE` type **4** (`[record+40]`, built by `00A03C80`) → `push 26`. Not a DIK.

| Kind | Action fn | ecx | Action 26 |
| --- | --- | --- | --- |
| Type 10 | `0054E280` | widget+4 | post `&(widget+352)` if nonzero |
| Type 11 | `0054DBC0` | widget+4 | debounce; `[def+545]`; **`call 0055AD60`** |
| Type 38 | `0055AD60` | widget+4 | `lea eax,[edi-26]`; table `0x55AE88` |

`0055AD60` case 0 (action 26):

```
0055AD7B  mov al, [esi+348]      ; widget+352 flag (inner this)
          je 0055AE3D            ; skip activate if 0
0055AD8F  call [eax+584]         ; widget vtbl+584 (0 args)
0055ADA1  mov [esi+364], 1
0055ADA8  call 0055B9D0          ; only if action==25 → vtbl+580
```

So the recovered **site** is type-38/11 action 26. The **instruction** that pushes the persist pair into `0059A238` is widget **vtbl+584** (or a callee). That slot is **PARTIAL**:

- Type-38 `00558DE0` walks a list and `0041E6D3` (`01230134+56`) with `&node+8`. Frontend (`[0x13B86A0]==0`) is `00595582` → UI vtbl+32 `0059A238`. **But** `00558DE0` takes a list arg; `vtbl+584` is called with **no** arg. Direct identity **UNREAD**.
- No `E8 0059A238` / `E8 00595582` in `0055AD60` itself.

Subscribe-set is **PARTIAL** (type 33 ctor always `vtbl+8`; type 38 `00558C70` also subscribes on select-state 0–6). Type-38 `00558C10` is true only if a type-10 parent’s vtbl+568 (`0054E550` = `[+360]`) equals this widget — the `"UI_ACCEPT"` bind, not the New Profile name.

---

## 6. `00851770` / `00851920`

| VA | Role |
| --- | --- |
| `00851770` | After `00596917` slot `0x17`. Finds `UI_NEW_PROFILE_EDIT_BOX`, `cmp eax, 37`, seeds `004069E0` → `"Default"` (`0x122DE80` when game singleton 0), binds actions **33/34**. **Does not post 0x126.** |
| `00851920` | **Consumer** of `0x126` only. |

---

## 7. C# mismatch

| Native | C# now |
| --- | --- |
| Type-10 posts **its** +352 pair | `MessageFromWidgets`: first `Visible && !Clip && MessageId!=0` among types 10/11/38 |
| Type-38 persist via `0055B040` / vtbl+284 / action 26 / vtbl+584 | Same scan; factory copies `def.MessageId` |
| Press Start `0xE5` is attach, file id 0 | `AttachFrontendTree` patches root `MessageId=0xE5` if 0 (**LEFTOVER**, first-seen MATCH) |
| `MessageFromAction(screen)` | Always **null** (dead overload) |
| Empty-widget type 4 | `MaybeActivateNewGameFromInput` hard `0xE5` iff `FrontendMenuRoot==PRESS_START` (**LEFTOVER**) |
| Type-38 +352 is a flag | C# `MessageId` is the persist i32, not a widget+352 analog |
| Listener walk `0055CB10` | One mapped id per pump, then return |

`05-input.md` still says `0x126` poster **UNREAD**. That is **STALE**: persist + ctor copy + action-26 site are recovered; only vtbl+584 and the type-4 **device** stay unread.

Tests: `Type4_drives_lifecycle_0xE5_then_0x126_then_15` (install, no inject);
`Accept_0x126_and_NewGame_15_have_no_recovered_action` only locks the **screen** overload to null.

---

## 8. Proposed `FrontendInputMap` (do not apply here)

1. Keep `Type4 → ActionType4 (26)`. Do **not** map Return / Escape / Space / Enter.
2. Keep `MessageFromAction(action, screen) == null`. Screen strings are not the poster.
3. Keep `MessageFromWidgets` as the stand-in: action 26 → first visible stored id
   - type 10: attach/pair analog (`0xE5` on Press Start only)
   - type 38: persist `MessageIdCrc` (`0x126` on `UI_ACCEPT_NEW_PROFILE`)
   - type 11: same CRC (`15` on `UI_FRONTEND_BUTTON_NEW_GAME`)
4. Do **not** treat type-38 `MessageId` as widget+352. If a later native dump pins vtbl+284, store the pair on the type-38/34 extra (`+364…`), not `Type10StoredMsgOffset`.
5. Drop the `FrontendMenuRoot==PRESS_START` hard `0xE5` once no-install fixtures carry the attach write.
6. Keep `Queue` / `DispatchFrontendMessage` for consumer tests and unread devices.
7. Constants already present: `Type38ActionFn=0x0055AD60`, `PersistMessageCopyFn=0x0055B040`, `PersistMessageDefOffset=224`, `MessageIdCrc`. Add (when dumping rdata): type-38 vtbl+284 and vtbl+584 VAs.

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `0059A238` | UI vtbl+32; `0x126` → `00851920` | **PROVEN** |
| `00851920` | Commit name / `[+5]=1` | **PROVEN** consumer |
| `00851770` | Bind type-37 + `"Default"` | **PROVEN** bind; **DISPROVEN** poster |
| `00631C60` / `00632500` | Persist def `+224` | **PROVEN** |
| `0x53C644E4` | File CRC before i32 `0x126` | **PROVEN** value; name **UNREAD** |
| `00558B90` / `0055B460` / `0055B040` | Type 38/34 ctor + copy | **PROVEN** |
| `0124B04C+284` | Type-38 store of the pair | **UNREAD** fn |
| `0054E4F0` / `+352` | Type-10 pair store | **PROVEN**; **DISPROVEN** for `0x126` |
| `0054E280` / `0054E2FA` | Type-10 action 26 → `0059A238` | **PROVEN** for `0xE5`; **DISPROVEN** for `0x126` |
| `0054DBC0` | Type 11 → `0055AD60` | **PROVEN** wrapper |
| `0055AD60` | Type 34/38 action 26–32 | **PROVEN** site |
| `vtbl+584` | Activate / post persist pair | **PARTIAL** |
| `00558DE0` | Type-38 list → `0041E6D3` | **PARTIAL** (arg mismatch vs +584) |
| `0041E6D3` | Input vtbl+56 → UI vtbl+32 if no game | **PROVEN** consumer |
| `0052F040` | Type-0 vtbl+284 | **DISPROVEN** poster |
| `0042E3EE` / `00A03C80` | Type 4 → 26 | **PROVEN** |
| DIK 28 | Type 1 / action 33 | **DISPROVEN** as `0x126` |
