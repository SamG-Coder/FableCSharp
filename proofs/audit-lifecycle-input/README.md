# Audit: `MaybeActivateNewGameFromInput` vs type 4 / action 26 / stored id

Investigation only. No production `src/` edits.

Authority: `Fable.exe` `0042E3EE` / `0054E280` / `0054E2FA` /
`00598EE6` / `0054E4F0` / `0054DBC0` / `0055AD60` / `0055B040` /
`0059A238`; `frontend.bin` persist CRC `0x53C644E4`;
`src/Fable.Game/EngineLifecycle.cs`
(`PumpFrontendFrame`, `MaybeActivateNewGameFromInput`,
`AttachFrontendTree`, `DispatchFrontendMessage`);
`src/Fable.Game/FrontendInputMap.cs`;
`src/Fable.Game/EngineInput.cs`;
`src/Fable.Game/FrontendMessages.cs`;
`src/Fable.Client/Program.cs`;
`src/Fable.Formats/Defs/FrontendUiDef.cs`;
`implementer/frontend/05-input.md`;
`proofs/type4-input-lifecycle/README.md`;
`proofs/who-posts-0x126-and-15/README.md`;
`proofs/audit-frontend-leftover/README.md`;
`tests/Fable.Formats.Tests/FrontendInputTests.cs`,
`EngineLifecycleTests.cs`, `FrontendUiDefTests.cs`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **MATCH** / **STALE**.

Do not re-prove Return≠`0xE5`, dest layout, or Leave/`FinalAlbion.wld`.

---

## Verdict

Lifecycle input is no longer a New Game helper. `PumpFrontendFrame`
calls `MaybeActivateNewGameFromInput` after `0042E3EE` analog
`PumpInput`. That method posts **one** `0059A238` id from a recovered
type-4 → action 26 stored message.

| Claim | Status |
| --- | --- |
| Type 4 (`[record+40]`) → action 26, no DIK | **PROVEN** `0042E3EE` |
| Type-10 action 26 posts widget+352 | **PROVEN** `0054E2FA` |
| Press Start +352 is attach `0xE5` (`00598EE6`), not persist | **PROVEN** |
| Type 38 persist `0x126`; type 11 persist 15 (`0x53C644E4`) | **PROVEN** file |
| Type 38/11 action 26 handlers `0055AD60` / `0054DBC0` | **PROVEN** sites; subscribe-set **PARTIAL** |
| Return / type 1 → `0xE5` / `0x126` / 15 | **DISPROVEN** |
| `MessageFromAction(screen)` posts anything | **DISPROVEN** (always null) |
| Screen-name map is the native poster | **DISPROVEN** |
| C# posts stored id via `MessageFromWidgets` | **MATCH** first-seen when widgets exist |
| Press Start type 4 with empty widgets → hard `0xE5` | **LEFTOVER** screen-name fork |
| Attach patches PRESS_START root `MessageId=0xE5` | **LEFTOVER** name stand-in for `00598A1C` |
| First-visible type 10/11/38 vs `0055CB10` listeners | **LEFTOVER** / **PARTIAL** |
| `0x126` / 15 inject (`Queue` / `DispatchFrontendMessage`) | **LEFTOVER** (consumer tests / no-install) |
| Physical type-4 device | **UNREAD** |
| Host Enter/Escape/Space/A/B leave frontend | **DISPROVEN** (type 1 only) |

---

## Native (recovered)

```
0042EC7C  frontend frame
  0042E3EE  poll
    00A03B40  type = [record+40]
    00A03B70  key  = [record+0]
    type 4 → push 26          // 00A03C80 built the record
    type 1 → action 33        // DIK 28 Return is this
  0055CB10  action listeners
    type-10 0054E280 action 26 → 0054E2FA push &widget+352 → 0059A238
    type-11 0054DBC0 action 26 → persist id (def+224 / vtbl+284)
    type-38 0055AD60 action 26 → persist id
  0042DC94 / 00599E3F  same-frame tick
```

| Stored id | Writer | Consumer |
| --- | --- | --- |
| `0xE5` | Attach `00598EE6` then type-10 `0054E4F0` at +352 | `00599D5C` |
| `0x126` | File `UI_ACCEPT_NEW_PROFILE` type 38 + `0055B040` | `00851920` |
| 15 | File `UI_FRONTEND_BUTTON_NEW_GAME` type 11 + same CRC | `0059A2DA` |

No `.text` `mov […], 0x126`. Name of CRC `0x53C644E4` is **UNREAD**.
Second file 15 after `0xF1A22807` is unused in C#.

---

## C# path

`PumpFrontendFrame` (`EngineLifecycle.cs`): `PumpInput` then
`MaybeActivateNewGameFromInput` then `0042DC94` / `00599E3F` notes.
Same-frame `0xE5` → arm → tick bind is **MATCH**.

```
MaybeActivateNewGameFromInput
  Stage==Frontend
  foreach Input.Applied (type, key)
    mapped = TryMapEvent(type, key, _frontendWidgets)
             → ActionFromEvent → MessageFromWidgets (action 26 only)
    if mapped==null && action==26 && FrontendMenuRoot==PRESS_START
      mapped = 0xE5                          // leftover fork
    if mapped is int: DispatchFrontendMessage; return
```

`EngineInput.ApplyEvent` still records action 26 on type 4. Frontend
does **not** walk `Input.Actions` (`0055CB10` leftover). Messages come
from `Applied` types.

`MessageFromWidgets`: first `Visible && !Clip && MessageId!=0` of
type 10 / 11 / 38.

`MessageFromAction(action, screen)` ignores `screen` and always
returns null. `TryMapEvent(type, key, screen)` is a dead overload.

---

## Screen-name forks

Dest / factory walk is **not** a name fork (already **DISPROVEN**).
Input still has two PRESS_START name gates.

### 1. Attach +352 stand-in — LEFTOVER, first-seen MATCH

`AttachFrontendTree`:

```
if rootName == UI_FRONTEND_PRESS_START_MENU && built[0].MessageId == 0
  built[0].MessageId = 0xE5
```

Native `0xE5` is slot `0x14` attach `00598A1C` / `00598EE6`, not persist
`MessageIdCrc`. PRESS_START type-10 file id is 0, so the patch is the
C# analog of that write. NEW_PROFILE / MAIN_MENU roots are also type
10 and are **not** patched — **MATCH** (those screens do not get the
Press Start attach write).

### 2. `MaybeActivate` fallback — LEFTOVER

When `_frontendWidgets` is empty (`Bootstrap(null)`, no
`frontend.bin`), `MessageFromWidgets` is null. Type 4 still posts
`0xE5` **iff** `FrontendMenuRoot == PressStartMenu`.

That is a screen string, not widget+352. It exists so
`Frontend_press_start_type4_posts_0xE5_then_new_profile` works without
an install. NEW_PROFILE / MAIN_MENU type 4 in that fixture posts
nothing (tests inject).

No remaining Return-by-screen fork. `TryMapEvent(TypeKey, 28, *)` is
null on every screen.

---

## Stored-id type 4 (install) vs injected 0x126 / 15

With `GameInstall` + factory tree:

| Screen | First stored type 10/11/38 | Type 4 posts |
| --- | --- | --- |
| PRESS_START | type-10 root after +352 patch = `0xE5` | `0xE5` |
| NEW_PROFILE | type-38 `UI_ACCEPT_NEW_PROFILE` = `0x126` | `0x126` |
| MAIN_MENU | type-11 `UI_FRONTEND_BUTTON_NEW_GAME` = 15 (first list child) | 15 |

Test `Type4_drives_lifecycle_0xE5_then_0x126_then_15` queues type 4
three times and does **not** inject. That is the recovered poster.

Still inject-only:

| Site | What |
| --- | --- |
| `FrontendInputMap.Queue` | Unread-device stand-in |
| `DispatchFrontendMessage(0x126)` / `(15)` | Direct `0059A238` |
| `Frontend_type4_then_injected_0x126_then_15_leaves` | no-install: type 4 then inject |
| `Queue_drives_lifecycle_without_a_key` | inject all three ids |
| `ActivateNewGame()` | still `Dispatch(15)` |

Those injects are **consumer** coverage (`0059A238` / tick), not a
recovered DIK. Do not map Enter to them.

`MaybeActivate` name is leftover: it posts `0xE5` / `0x126` / 15, not
only New Game.

---

## First-visible leftover

Native action 26 is a listener walk. C# returns the first matching
widget in factory DFS and then **returns** from the pump (one message
per poll).

Type-10 subscribe-set is already **PARTIAL** (`05-input.md`). If
several type 11 buttons listen, native could post more than 15 in one
poll. First-seen MAIN_MENU child order makes NEW_GAME first, so the
happy path **MATCH**es until focus/selection is recovered. Do not
invent list highlight.

`MessageFromWidgets` ignores `Enabled`. Native focus / enabled is
**UNREAD** here.

---

## Host leftover

`Program.cs` queues type 1 only (Escape / Space / Enter / F4 / A / B).
PlayAVI skip is that path. Frontend type 1 is action 33, not a UI
message. Live host cannot leave PRESS_START without a type-4 record.
Physical producer of type 4 stays **UNREAD**.

---

## Stale docs

| Doc | Claim | Now |
| --- | --- | --- |
| `audit-frontend-leftover` §2.2 / §3 | `MessageFromAction` is `screen==PressStart` → `0xE5`; other screens null | Always null. Widgets + PRESS_START leftover instead. |
| `05-input.md` | `MaybeActivate` left untouched; type 4 → `0xE5` on Press Start only | Widgets overload; install type 4 also posts `0x126` / 15. |
| `type4-input-lifecycle` | `0x126` / 15 posters UNREAD | Persist + type 11/38 action 26 **PROVEN**; device UNREAD. |
| `PARITY.md` Leave Press Start | Native key that posts `0xE5`/`0x126` UNREAD | Type 4 is the recovered **event**; device still UNREAD. |

---

## Proposed (do not apply here)

1. Keep type 4 → action 26. Do not map Return / Escape / Space.
2. Prefer stored widget id (type-10 +352 analog, type 11/38 persist).
   Drop the `FrontendMenuRoot == PressStartMenu` fallback once
   no-install fixtures carry the attach write.
3. Keep the PRESS_START `MessageId=0xE5` attach analog; do not copy it
   onto NEW_PROFILE / MAIN_MENU.
4. Keep `Queue` / `DispatchFrontendMessage` as consumer injects until
   a `.text` type-4 **device** exists. Host must not guess Start / A /
   click.
5. Rename `MaybeActivateNewGameFromInput` to a `0059A238` dispatch
   from recovered events when touching.
6. Delete or stop testing the dead `MessageFromAction(screen)`
   overload.

## Do not invent

- Enter → `0xE5` / `0x126` / 15.
- Per-screen dest numbers.
- A DIK for type 4.
- `.text` `mov […], 0x126`.
- Focus / list highlight as the first-visible scan without a dump.
- English name for CRC `0x53C644E4`.
