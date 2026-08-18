# UI_CANCEL persist (`0x53C644E4`) vs action 26 / ACCEPT

Investigation only. No production `src/` edits.

Authority: `Fable.exe` `0054DBC0` / `0055AD60` / `0055B040` /
`0055CB10` / `0059A238`; inflated `frontend.bin` + `names.bin`
(this note: **names + already-dumped widgets only**);
`src/Fable.Formats/Defs/FrontendUiDef.cs` (`MessageIdCrc`);
`src/Fable.Game/FrontendInputMap.cs` (`MessageFromWidgets`);
`src/Fable.Game/FrontendWidgetFactory.cs` (DFS `Children`);
`implementer/frontend/persist-scan.txt`, `01-widget-construction.md`,
`05-input.md`;
`proofs/who-posts-0x126-and-15/README.md`,
`proofs/who-posts-15/README.md`,
`proofs/audit-lifecycle-input/README.md`;
`tests/Fable.Formats.Tests/FrontendUiDefTests.cs`,
`FrontendInputTests.cs`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER**.

This folder does **not** contain a new `frontend.bin` hex dump of
`UI_CANCEL` / `UI_HELPERS_NEW_PROFILE`. `persist-scan.txt` only
walks the PRESS_START tree. Inflating those two defs is still
required to close the order question.

---

## Verdict

| Claim | Class |
| --- | --- |
| CRC `0x53C644E4` is persist i32 → def `+224` / vtbl+284 | **PROVEN** (`0055B040`) |
| Field English name | **UNREAD** (not `Message` / `MessageId`) |
| Same CRC on type 38 `UI_ACCEPT_NEW_PROFILE` = **`0x126`** | **PROVEN** file + test |
| Same CRC on type 11 `UI_FRONTEND_BUTTON_NEW_GAME` = **15** | **PROVEN** file + test |
| Same CRC on type 11 `UI_FRONTEND_BUTTON_INVISIBLE` = **`0xE5` (229)** | **PROVEN** PRESS_START dump (`Action` `0xF1A22807` also 229) |
| `UI_CANCEL` / `UI_HELPERS_NEW_PROFILE` exist in `names.bin` | **PROVEN** hashes |
| `UI_CANCEL` Type / `0x53C644E4` i32 / `Action` i32 | **UNREAD** (no hex walk in-repo) |
| `UI_HELPERS_NEW_PROFILE` persist `Children` order | **UNREAD** (no hex walk in-repo) |
| Type 11/38 action **26** posts stored persist id | **PROVEN** sites `0054DBC0` / `0055AD60` |
| Subscribe-set / focus / enabled gate on those handlers | **PARTIAL** / **UNREAD** |
| C# action 26 posts **first DFS** visible type 10/11/38 with `MessageId!=0` | **LEFTOVER** vs `0055CB10` listeners |
| Docs claiming NEW_PROFILE first stored id is ACCEPT `0x126` | **UNVERIFIED** until CANCEL (and any type 11 under `UI_NEW_PROFILE_MENU`) are scanned |

**Does CANCEL fire on action 26 before ACCEPT?** **UNREAD** in native
listener order. In C# it would, **if** CANCEL is type 10/11/38, has
nonzero `MessageId`, is visible, and appears earlier in factory DFS
than ACCEPT. That DFS puts `UI_HELPERS_NEW_PROFILE` **after** the
type-12 menu, so a type-11 under the menu would beat **both** helper
buttons.

---

## 1. What persist already proves (other widgets)

File form is `00431102`: CRC then i32. Runtime copy is `0055B040`
`[def+224]` then vtbl+284.

| Widget | Type | `0x53C644E4` | `0xF1A22807` (`Action`) | Action-26 poster |
| --- | ---: | ---: | ---: | --- |
| `UI_ACCEPT_NEW_PROFILE` | **38** | **`0x126`** | unread here | `0055AD60` |
| `UI_FRONTEND_BUTTON_NEW_GAME` | **11** | **15** | **15** | `0054DBC0` |
| `UI_FRONTEND_BUTTON_INVISIBLE` | **11** | **`0xE5`** (hex tail) | **229** | `0054DBC0` |
| PRESS_START / TITLE / TEXT / MOUSE | 10/5/0/6/32 | 0 or absent | 0 | type-10 +352 is attach, not this CRC |

`FrontendUiDef.ReadPersistI32` only indexes `0x53C644E4`. The
duplicate `Action` dword is unused in C#.

`0059A238` consumers recovered so far: `0xE5` → `00599D5C`,
`0x126` → `00851920`, `0x124` → `0059899A`, `15` → `0059A2DA`.
Any other CANCEL id would no-op in `DispatchFrontendMessage` unless
it is one of those four.

---

## 2. Names present, bodies not dumped

`implementer/frontend/persist-scan.txt` names.bin:

| Hash | Name |
| --- | --- |
| `7AFC8A56` | `UI_FRONTEND_NEW_PROFILE_SCREEN` |
| `D18CAE8B` | `UI_NEW_PROFILE_MENU` |
| `A04CD925` | `UI_NEW_PROFILE_BUTTON` |
| `288AB10A` | `UI_NEW_PROFILE_EDIT_BOX` |
| `A24F408D` | `UI_ACCEPT_NEW_PROFILE` |
| `CCA6E7F6` | `UI_CANCEL` |
| `A07FA899` | `UI_CANCEL_TEXT` |
| `E7E0F553` | `UI_OK` |
| `23571A8F` | `UI_HELPERS_OK` |
| `55D1EB11` | `UI_HELPERS_NEW_PROFILE` |
| `C10395E7` | `UI_HELPER_BUTTON_MOUSE_AREA` |

`names.bin` order is **not** persist child order.

Hex walks in that file start at PRESS_START `#620` and never emit
`inst=UI_CANCEL` or `inst=UI_HELPERS_NEW_PROFILE`.
`01-widget-construction.md` only sketches the New Profile root:

```
UI_FRONTEND_NEW_PROFILE_SCREEN #201 Type=10
  title TEXT_GUI_MENU_NEW_PROFILE
  coastal BG
  UI_TABLE_TITLE_WHOLE Type=2
  UI_NEW_PROFILE_MENU Type=12
  helpers
```

“helpers” is the `UI_HELPERS_NEW_PROFILE` slot. Child indices of
that group are **UNREAD**.

Type 39 ctor `00558540` (size `0x1C0`) sits next to type 38
AcceptButton `00558B90`. CANCEL **might** be 39 / 0 / 11 — do not
guess. Only type **11** and **38** have recovered action-26 posters.
Type 0 vtbl+284 is `0052F040` `ret 4` (**DISPROVEN** poster).

---

## 3. Tree order vs C# first-visible

`FrontendWidgetFactory.AttachChildren` walks `ChildIndices` DFS
(parent, then each child in persist order, recurse).

`MessageFromWidgets` (action 26 only):

```
first widget where Visible && !Clip && MessageId != 0
  && Type is 10 or 11 or 38
```

Then `MaybeActivateNewGameFromInput` dispatches **one** id and
returns.

On NEW_PROFILE the type-10 root is **not** patched to `0xE5`
(`AttachFrontendTree` only patches PRESS_START). Root file id is 0
(**MATCH** `audit-lifecycle-input`). Factory then hits:

1. Title / BG / table — not 10/11/38 with a stored id (expected).
2. `UI_NEW_PROFILE_MENU` type **12** — skipped (list). Its children
   may include `UI_NEW_PROFILE_BUTTON` / edit box **37**.
3. `UI_HELPERS_NEW_PROFILE` (group, expected type 5) — skipped.
   Its persist children are where CANCEL vs ACCEPT sit.

If `UI_NEW_PROFILE_BUTTON` is type 11 with a nonzero `0x53C644E4`,
it beats **both** helper buttons. Scan that def in the same pass.

If helpers children are `UI_CANCEL` then `UI_ACCEPT_NEW_PROFILE`
(left B, right A — typical, **not proven**), and CANCEL is type
11/38 with `MessageId != 0`, C# type 4 on New Profile posts
**CANCEL**, not `0x126`.

`audit-lifecycle-input` table “NEW_PROFILE first stored =
`UI_ACCEPT_NEW_PROFILE` `0x126`” is therefore **not closed**.
`FrontendInputTests.Type4_drives_lifecycle_0xE5_then_0x126_then_15`
would still reach MAIN_MENU if CANCEL stored **`0x124`** (direct
`0059899A`). It would **fail** if CANCEL stored an unhandled id
or 15. Passing that test does **not** prove ACCEPT won action 26.

Native `0055CB10` notifies subscribers. If both CANCEL and ACCEPT
listen to 26, both can post in one poll; C# keeps only the first
DFS hit. Focus/enabled is **UNREAD** — do not invent highlight.

---

## 4. Dump recipe (no `src/` change)

Load TLC `frontend.bin` / `names.bin`. For each name:

`UI_HELPERS_NEW_PROFILE`, `UI_CANCEL`, `UI_CANCEL_TEXT`,
`UI_ACCEPT_NEW_PROFILE`, `UI_NEW_PROFILE_MENU`,
`UI_NEW_PROFILE_BUTTON`, `UI_NEW_PROFILE_EDIT_BOX`,
`UI_FRONTEND_NEW_PROFILE_SCREEN`.

Record:

- `Type` (`0x0DA8270B`)
- `Children` indices → instance names (persist order)
- `ReadPersistI32(raw, 0x53C644E4)`
- `ReadPersistI32(raw, 0xF1A22807)` (`Action`)
- factory DFS of `UI_FRONTEND_NEW_PROFILE_SCREEN`: first type
  10/11/38 with `MessageId != 0`

That first hit **is** the C# action-26 message. Compare to native
only after `0054DBC0` / `0055AD60` focus gates are dumped.

---

## Do not invent

- CANCEL id (`0x124` / 0 / other) without the file i32.
- Helper child order from `names.bin` adjacency.
- Type 39 as Cancel without `[def+60]`.
- Enter / Escape / B as action 26.
- English name for `0x53C644E4`.
