# 05 — Frontend input / messages

Vertical slice: startup videos → PRESS START → (input) → NEW PROFILE → (accept) → MAIN MENU → (msg 15) → Leave.

## Proven transition table

| Msg | `0059A238` | Guard | Same-frame `00599E3F` | Next |
| --- | --- | --- | --- | --- |
| `0xE5` | `00599D5C` | empty `005955AB` | `00595845` `[ui+160]=1` `[ui+100]=1` then `00596917` slot `0x17` | `UI_FRONTEND_NEW_PROFILE_SCREEN` |
| `0xE5` | `00599D5C` | one name (size==4) | `0059899A` | `MAIN_MENU_*` (empty continue → `NO_CONTINUE`) |
| `0xE5` | `00599D5C` | names>1 | `00597B20` | UNREAD (no change) |
| `0x126` | `00851920` | `UI_ACCEPT_NEW_PROFILE` def; trim len>0 | `[ui+96+5]=1` `[+4]=0` then `0059697A` / `004067C0` writable | `UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE` |
| `0x124` | `0059899A` | empty continue list | `00595A06` | `NO_LIVEAWARE_NO_CONTINUE` |
| `15` | `0059A2DA` | `[ui+28].vtbl+16` | `00594F28` `[retail+41]=1` | Leave `0042F2A2` |

`00851770` binds `UI_NEW_PROFILE_EDIT_BOX` type 37 and seeds `004069E0` → UTF-16 `0x122DE80` `"Default"` when game singleton is 0.

## Who posts `0xE5`

**Attach writer (not the user path):** `00598A1C` at `00598EE6` `mov [eax], 0xE5`, then slot `0x14` vtbl+284.

- Type-10 `012497E4+284` = `0054E4F0` stores id at widget+352 (not a no-op).
- Generic `0122F5D4+284` = `0052F040` `ret 4`.

**User poster (recovered):** type-10 inner vtbl `012497BC+4` = `0054E280`.

- Switch actions 26–34 (`lea eax,[ebx-26]`, table `0x54E32C`, index `00 01 03 03 03 03 03 02 02`).
- Action **26** → `0054E2FA` pushes `&widget+352` to UI vtbl+32 `0059A238`.
- `0042E3EE` type **4** (`[record+40]`) is `push 26`. No DIK compare.
- Action 33 (type 1 / any key) is last-key==1 (`0042D506` `[input+192]`) → `00597BF2(1)`, **not** `0xE5`.

Physical device/DIK that produces type 4 is **UNREAD**. Return (DIK 28) is type 1 / action 33 and does **not** post `0xE5`. Host Return→msg 15 from Press Start is **DISPROVEN**. Type-10 subscribe-set for action 26 is **PARTIAL**.

## Who posts `0x126`

**UNREAD.** No `.text` `mov […], 0x126`. frontend.bin `UI_ACCEPT_NEW_PROFILE` stores `0x126`. `0059A238` consumes it → `00851920`. Isolated host uses `FrontendInputMap.Queue(0x126)`.

## `0041E6D3`

Input vtbl `01230134+56`. Consumer: game singleton 0 → UI vtbl+32. Not the Press Start poster (`0054E280` calls UI directly). List-widget `005403D2` key==1 also calls this slot (different widget).

## Isolated code

- `src/Fable.Game/FrontendMessages.cs` — ids, slots, names, `ApplyMessage` / `ApplyTick`.
- `src/Fable.Game/FrontendInputMap.cs` — type 4 / action 26 → `0xE5` on Press Start only; `Queue(msg)` otherwise.
- `tests/Fable.Formats.Tests/FrontendInputTests.cs` — table + lifecycle via `Queue`, no guessed key.

`EngineLifecycle.MaybeActivateNewGameFromInput` already does not post 15 from Press Start. Left untouched.
