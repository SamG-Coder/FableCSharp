# `00598EE6` `0xE5` write is only on Press Start attach `00598A1C`

Investigation only. No production `src/` edits.

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00580000.txt`
(`00598A1C` / `00598EE6` / `00596917` / `0059697A` /
`0059A6BE`); `listing-00400000.txt` (`0042EA62`);
`src/Fable.Game/EngineLifecycle.cs`
(`InitFrontendUi`, `AttachPressStartWidgets`,
`WriteType10AttachMessage`, `AttachFrontendTree`,
`BindNewProfileFromArmedTick`, `CommitNewProfileFromArmedEdit`,
`AttachFrontendMainMenu`);
`src/Fable.Game/FrontendInputMap.cs` (`AttachWriteE5`);
`proofs/press-start-e5-attach/README.md`;
`proofs/type10-plus352/README.md`;
`proofs/plus380-poster/README.md`;
`proofs/audit-frontend-leftover/README.md`.

Do not re-prove type 4 → action 26, Return ≠ `0xE5`,
persist `+224` on the type-10 is 0, `0054E4F0` → widget+352,
or `0059A238` consume (`0xE5` → `00599D5C`).

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **MATCH**.

---

## Verdict

**`mov [eax], 0xE5` at `00598EE6` lives only inside Press
Start attach `00598A1C` (`00598A1C`–`00599D15`).** It writes
a 16-byte packet then slot **`0x14`** `vtbl+284`. It is
**not** on New Profile `00596917` or Main Menu `0059697A`.

Same listing has one other `0xE5` immediate: consume
`0059A6BE` `sub ecx, 0xE5`. That is **not** a store.

Host used to name-check `rootName == PRESS_START` inside
shared `AttachFrontendTree`. That fork is **gone**. The
write is **`AttachPressStartWidgets` → `WriteType10AttachMessage`**
only — the `00598A1C` analog. New Profile / Main Menu rebuilds
call `AttachFrontendTree` and **do not** write `0xE5`.
Call-site split **MATCH**. Remaining leftover is the write
shape (type-10 + `MessageId==0` walk vs slot `0x14` packet).

| Claim | Status |
| --- | --- |
| `00598A1C` is first-seen Press Start populate | **PROVEN** `0042EA62` after `005958F5`; factory `"UI_FRONTEND_PRESS_START_MENU"` slot `0x14` |
| `00598EE6` is inside `00598A1C` | **PROVEN** `00598A1C`…`00599D15` `ret 4` |
| `00598EE6` is `mov [eax], 0xE5` then slot `0x14` `vtbl+284` | **PROVEN** listing |
| Same write exists on `00596917` | **DISPROVEN** (slot `0x17` + `00851700` / `00851770` only) |
| Same write exists on `0059697A` | **DISPROVEN** (`00595A06` / `00595B24` `MAIN_MENU_NO_CONTINUE`) |
| `listing-00580000.txt` has another `mov […], 0xE5` | **DISPROVEN** (only `00598EE6` store; `0059A6BE` is `sub`) |
| `00598A1C` factory of `NEW_PROFILE` slot `0x17` also writes `0xE5` | **DISPROVEN** (factory after the write; write already targeted `0x14`) |
| Shared `AttachFrontendTree` name-check is native | **DISPROVEN** leftover; native write is site-local |
| Host still name-checks in `AttachFrontendTree` | **DISPROVEN** (removed) |
| Host write only from `00598A1C` analog | **MATCH** `AttachPressStartWidgets` → `WriteType10AttachMessage` |
| Host write is slot `0x14` packet + `vtbl+284` | **LEFTOVER** (`MessageId` on type-10 with id 0) |

---

## 1. `00598A1C` is Press Start attach

`0042E98F` bind (`listing-00400000.txt`):

```
0042EA46  mov ecx, [esi+180]
0042EA4C  call 005958F5
0042EA51  cmp [esi+324], 0
0042EA58  mov ecx, [esi+180]
0042EA5E  sete eax
0042EA61  push eax
0042EA62  call 00598A1C
```

First-seen after AVI is arg **0** (skip
`UI_FRONTEND_MEDIA_PLAYER_ERROR`). Then
`00598A1C` factories Press Start:

```
00598BA2  push "UI_FRONTEND_PRESS_START_MENU"
00598BB7  mov [ebp+108], 0x14
00598BBE  call 0059B5D7
00598BD2  call 0041DB1D
```

Prologue / epilogue pin the function:

```
00598A1C  push ebp
00598A1D  lea ebp, [esp-116]
00598A21  sub esp, 0xC4
…
00599D11  add ebp, 116
00599D14  leave
00599D15  ret 4
```

`0059899A` (Main Menu helper) is a **different** function
that ends at `00598A19`. First-seen populate is **not**
that path.

The same `00598A1C` later factories many other menus into
other slots, including New Profile **after** the `0xE5`
write (`00598FCF` `"UI_FRONTEND_NEW_PROFILE_SCREEN"` slot
`0x17`). That is a slot table fill, not a second `0xE5`
store.

---

## 2. `00598EE6` is only that site

Inside `00598A1C`, after `INVALID_SAVE` factory:

```
00598EC3  push 16
00598EC5  call 00BFEA1A          ; packet
00598ED1  call 0042BE50          ; [packet]=0
00598EDE  call 0042AA29          ; wrapper {packet*, ctrl*}
00598EE3  mov eax, [ebp-56]
00598EE6  mov [eax], 0xE5        ; packet[0] = 0xE5
00598EF2  mov [ebp+108], 0x14
00598EF9  call 0059B5D7          ; slot 0x14
00598F00  mov eax, [ecx]
00598F05  push edx               ; &wrapper
00598F06  call [eax+284]
```

`listing-00580000.txt` `0xE5` immediates (complete):

| VA | Insn | Role |
| --- | --- | --- |
| `00598EE6` | `mov [eax], 0xE5` | attach store |
| `0059A6BE` | `sub ecx, 0xE5` | `0059A238` consume |

No other `mov […], 0xE5` in that listing. The store is
not a helper shared with `00596917` / `0059697A`.

---

## 3. New Profile `00596917` does not write `0xE5`

`00596917`–`00596979` (`ret`). Tick `00599E3F` when
`[ui+160]≠0` calls it (`00599ED2`).

```
00596921  push 23                ; slot 0x17
00596923  pop esi
00596930  call 0059B5D7
0059693B  call 00596763          ; switch current menu
00596940  push 16
00596942  call 00BFEA1A
00596962  call 00851700          ; [ui+96] ctor
00596970  call 00851770          ; bind edit box
```

The 16-byte alloc is the `00851700` object, **not**
`0042BE50` / `0042AA29` / `mov [eax], 0xE5`. No
`vtbl+284`. Slot is **`0x17`**, not `0x14`.

`00598A1C` already factory-built that slot earlier
without an `0xE5` packet. `00596917` only switches to
it and binds the edit box.

---

## 4. Main Menu `0059697A` does not write `0xE5`

`0059697A`–`00596A65` (`ret 4`). Tick after
`[ui+96+5]≠0` `[+4]==0` and empty `005955AB` calls it
(`0059A008`).

```
0059698D  call 004067C0          ; can-create
00596A36  push "UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE"
00596A3E  call 0099EBF0
00596A49  call 00595A06          ; attach
00596A5B  call 00595B24          ; build
```

No `00BFEA1A` packet, no `0xE5`, no slot-`0x14`
`vtbl+284`. `00596763` (switch) also has no `0xE5`.

---

## 5. Host: name-check leftover vs `00598A1C` analog

**Native:** write is an instruction in `00598A1C`. Later
attaches never run it. There is no screen-string compare.

**Old host leftover** (documented in
`proofs/press-start-e5-attach/README.md`, now **STALE**
on this point):

```
AttachFrontendTree(rootName):
  build tree
  if rootName == UI_FRONTEND_PRESS_START_MENU &&
     built[0].MessageId == 0
    built[0].MessageId = 0xE5
```

That gated a **shared** rebuild used by Press Start, New
Profile, and Main Menu. First-seen id was right only
because the string matched. Native has no such check.

**Current host** (`EngineLifecycle.cs`):

| Host site | Native | Writes `0xE5`? |
| --- | --- | --- |
| `InitFrontendUi` → `AttachPressStartWidgets` | `00598A1C` | **yes** — `WriteType10AttachMessage` |
| `BindNewProfileFromArmedTick` → `AttachFrontendTree` | `00596917` | **no** |
| `CommitNewProfileFromArmedEdit` → `AttachFrontendTree` | `0059697A` | **no** |
| `AttachFrontendMainMenu` → `AttachFrontendTree` | `0059899A` / `00595A06` | **no** |
| `AttachFrontendTree` itself | generic factory / child walk | **no** |

```
AttachPressStartWidgets():
  AttachFrontendTree(PRESS_START)
  WriteType10AttachMessage()     ; 00598EE6 analog
```

Call-site **MATCH**. Do not put the write back into
`AttachFrontendTree` behind a name check.

Still **LEFTOVER** inside `WriteType10AttachMessage`:
it notes `00598EE6` then patches every **type-10** with
`MessageId==0` in the current list. Native targets
**slot `0x14`** via `0059B5D7` and stores a heap packet
through `vtbl+284` (`0054E4F0` → `+352`). C# collapses
that to `FrontendWidget.MessageId`. First-seen Press
Start tree is only that menu, so the posted id
**MATCH**es; the walk and field are leftover.

`AttachFrontendTree` still scans
`UI_PRESS_START_TEXT` / `TEXT_GUI_MENU_PRESS_BUTTON`
after **any** root (`audit-frontend-leftover`). That is
a label leftover, not an `0xE5` write.

---

## Do not invent

- `0xE5` packet on `00596917` / `0059697A`.
- Re-introducing `AttachFrontendTree`
  `rootName == PRESS_START` as the writer.
- Patching NEW_PROFILE / MAIN_MENU type-10 roots.
- Treating `0059A6BE` `sub ecx, 0xE5` as a store.
- Persist `0xE5` on `UI_FRONTEND_PRESS_START_MENU`
  (still 0; see `press-start-e5-attach`).
