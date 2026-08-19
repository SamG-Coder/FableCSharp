# `AttachFrontendTree` still scans `UI_PRESS_START_TEXT` after any root

Investigation only. No production `src/` edits.

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00580000.txt`
(`00596917` / `0059697A` / `00596763` / `00595A06` /
`00595B24` / `00599ED2` / `0059A008`);
`listing-00840000.txt` (`00851770`);
`tools/Fable.ExeIndex/out/01-sections/text-map/functions.tsv`;
`tools/Fable.ExeIndex/out/00-index/strings.tsv`, `xrefs.tsv`;
`src/Fable.Game/EngineLifecycle.cs`
(`AttachFrontendTree`, `BindNewProfileFromArmedTick`,
`CommitNewProfileFromArmedEdit`, `AttachFrontendMainMenu`,
`AttachPressStartWidgets`);
`tests/Fable.Formats.Tests/FrontendUiDefTests.cs`
(`Factory_builds_press_start_then_main_menu_from_the_same_walk`);
`proofs/audit-frontend-leftover/README.md`;
`proofs/00598A1C-only-e5/README.md`.

Do not re-prove `0xE5` attach (`00598EE6` / slot `0x14`),
type 4 → action 26, or dest layout forks.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **MATCH**.

---

## Verdict

**Yes, leftover.** Host `AttachFrontendTree` always
`Find`s `UI_PRESS_START_TEXT` /
`TEXT_GUI_MENU_PRESS_BUTTON` after the factory walk,
then writes `FrontendPressStartLabel`. That runs for
**every** root: Press Start, New Profile (`00596917`),
Main Menu (`0059697A` / `00595A06`). Native
`00596917` and `0059697A` do **not** scan those names.

The type-6 label exists only as a persist child of
`UI_FRONTEND_PRESS_START_MENU`. Native never looks it
up by string at attach. The exe has **no**
`UI_PRESS_START_TEXT` or `TEXT_GUI_MENU_PRESS_BUTTON`
literal. `FrontendPressStartLabel` is write-only.

Related leftover (same loop): every non-root child is
noted as `005331A0` via `FrontendPressStartCtorFn`
(`0054E3D0`) after **any** root. Native child apply
`005331A0` is generic persist, not a Press Start name
walk. `00596917` / `0059697A` do not call it.

| Claim | Status |
| --- | --- |
| Host `AttachFrontendTree` scans `UI_PRESS_START_TEXT` / `TEXT_GUI_MENU_PRESS_BUTTON` after **any** root | **PROVEN** `EngineLifecycle.cs` |
| That scan is gated on `rootName == PRESS_START` | **DISPROVEN** (no name check) |
| Native `00596917` scans those strings | **DISPROVEN** (slot `0x17` + `00851770` `UI_NEW_PROFILE_EDIT_BOX` only) |
| Native `0059697A` scans those strings | **DISPROVEN** (`00595A06` `MAIN_MENU_NO_CONTINUE` + `00595B24` label table) |
| `.text` / `strings.tsv` contain `UI_PRESS_START_TEXT` or `TEXT_GUI_MENU_PRESS_BUTTON` | **DISPROVEN** (only `UI_FRONTEND_PRESS_START_MENU`) |
| NEW_PROFILE / MAIN_MENU persist trees include `UI_PRESS_START_TEXT` | **DISPROVEN** (factory walk: profile title `UI_TEXT_NEW_PROFILE_MENU_TITLE` / `TEXT_GUI_MENU_NEW_PROFILE`) |
| `FrontendPressStartLabel` is read after attach | **DISPROVEN** (set / clear only) |
| Host child notes `0054E3D0` / `005331A0` after `00596917` / `7A` | **LEFTOVER** |
| Dropping the scan changes native attach | **DISPROVEN** (native never did it) |

---

## 1. Host still scans after any root

`EngineLifecycle.AttachFrontendTree` (shared by
`AttachPressStartWidgets`,
`BindNewProfileFromArmedTick` → `00596917`,
`CommitNewProfileFromArmedEdit` → `0059697A`,
`AttachFrontendMainMenu` → `0059899A` / `00595A06`):

```
_frontendWidgets.Clear()
FrontendPressStartLabel = null
built = FrontendWidgetFactory.Build(..., rootName, ...)
text = Find(w =>
  w.Name == "UI_PRESS_START_TEXT" ||
  w.TextTag == "TEXT_GUI_MENU_PRESS_BUTTON")
if text.Name is not null
  FrontendPressStartLabel = text.Text ?? text.TextTag
foreach widget except root
  Note(FrontendPressStartCtorFn, ... "005331A0 child ...")
```

No `rootName == PRESS_START` gate. The `0xE5` name
check previously in this helper is **gone**
(`00598A1C-only-e5`); this label `Find` remains.

`FrontendPressStartLabel` is only assigned here and
nulled at the start of the same method. No draw /
input / test reads it.

---

## 2. Native `00596917` does not scan that

Tick `00599E3F` when `[ui+160]≠0` (`00599ED2`).
`functions.tsv` `0x00596917` (44 insns): callees
`0059B5D7`, `00596763`, `00BFEA1A`, `00851700`,
`00851770`. **No** string column.

`listing-00580000.txt` `00596917`–`00596979` `ret`:

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

`00596763` compares the looked-up slot pointer and
rewires `[ui+152]/[+156]`. No widget-name `Find`.
No `005331A0`.

`00851770` (`listing-00840000.txt`) is the only
name lookup on this path:

```
0085177F  push "UI_NEW_PROFILE_EDIT_BOX"
00851784  call 0099EBF0
0085179D  call [edx+260]
008517A3  cmp eax, 37
```

That is type-37 edit bind, not the type-6 Press
Start label. `xrefs.tsv`: `0x00851780` →
`UI_NEW_PROFILE_EDIT_BOX`. Slot `0x17` was already
factory-built earlier in `00598A1C`
(`00598FD0` `"UI_FRONTEND_NEW_PROFILE_SCREEN"`).

---

## 3. Native `0059697A` does not scan that

Tick after `[ui+96+5]≠0` `[+4]==0` and empty
`005955AB` (`0059A008`). `functions.tsv`
`0x0059697A` (76 insns): one string
`UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE`.
Callees include `004067C0`, `00595A06`, `00595B24`.
**No** `005331A0`.

```
0059698D  call 004067C0
00596A36  push "UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE"
00596A3E  call 0099EBF0
00596A49  call 00595A06
00596A5B  call 00595B24
```

`00595A06` has an empty string column (slot lookup +
factory). `00595B24` is a **menu label-slot** table:

```
00595B41  push "UI_TEXT_NEW_GAME"
00595B68  push "UI_TEXT_LOAD_GAME"
00595B8F  mov ebx, "UI_TEXT_OPTIONS_MENU_TITLE"
```

(`functions.tsv` also `UI_TEXT_GAME_OPTIONS_MENU_TITLE`
/ `UI_TEXT_VIDEO_MENU_TITLE` /
`UI_TEXT_SCOREBOARD_MENU_TITLE` /
`UI_TEXT_REDEFINE_KEYS_MENU_TITLE` /
`UI_TEXT_AUDIO_OPTIONS_MENU_TITLE`.) None of those is
`UI_PRESS_START_TEXT` / `TEXT_GUI_MENU_PRESS_BUTTON`.

---

## 4. Those names are not even in the exe

`strings.tsv` PRESS_START hits: **one** —
`0x01252930` `UI_FRONTEND_PRESS_START_MENU`
(`xrefs.tsv` site `0x00598BA3` inside `00598A1C`).

No `UI_PRESS_START_TEXT`. No
`TEXT_GUI_MENU_PRESS_BUTTON`. They live in
`frontend.bin` / `TEXT_ENGLISH_MAIN`, recovered as
the type-6 child of Press Start only.

Factory walk (`FrontendUiDefTests`):

| Root | Type-6 that the host scan would need |
| --- | --- |
| `UI_FRONTEND_PRESS_START_MENU` | **has** `UI_PRESS_START_TEXT` / `TEXT_GUI_MENU_PRESS_BUTTON` |
| `UI_FRONTEND_NEW_PROFILE_SCREEN` | `UI_TEXT_NEW_PROFILE_MENU_TITLE` / `TEXT_GUI_MENU_NEW_PROFILE` |
| `UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE` | not that label |

After New Profile / Main Menu rebuild the host
`Find` **misses**, so `FrontendPressStartLabel`
stays `null`. Harmless no-op, still a Press Start
name leftover on a generic attach.

On Press Start the scan **hits**, but native attach
never stored that string on a UI field — type-6
draw reads persist / text bank through the widget,
not `FrontendPressStartLabel`.

---

## 5. Related: `005331A0` notes tagged Press Start ctor

Same `AttachFrontendTree` loop notes every child
with `FrontendPressStartCtorFn` (`0054E3D0`) and
text `"005331A0 child …"`. Native:

- `0054E3D0` is type-10 **root** ctor (Press Start /
  New Profile / Main Menu roots are all type 10).
- `005331A0` is generic persist apply / child walk
  (`implementer/frontend/fn-005331A0-exact.txt`),
  called from container construction (`00531EC0`),
  **not** from `00596917` / `0059697A`.
- `00596917` switches to an already-built slot.
  Host instead `Clear`s and re-`Build`s, then
  pretends each child is a Press Start `005331A0`.

Do not treat that note VA as proof that New Profile
re-runs Press Start ctor.

---

## Do not invent

- A native strcmp / slot lookup of
  `UI_PRESS_START_TEXT` on `00596917` / `0059697A`.
- Copying the type-6 Press Start label onto
  NEW_PROFILE / MAIN_MENU trees.
- Using `FrontendPressStartLabel` as the type-6
  draw source (native draws the widget).
- Putting the `0xE5` write back into
  `AttachFrontendTree` (`00598A1C-only-e5`).
- Calling `005331A0` from `00596917` / `7A`.

**Proposed (do not apply here):** drop the
`UI_PRESS_START_TEXT` / `TEXT_GUI_MENU_PRESS_BUTTON`
`Find` and `FrontendPressStartLabel`. Keep generic
factory + child walk. Retarget child notes off
`FrontendPressStartCtorFn` if they stay.
