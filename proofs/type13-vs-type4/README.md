# Type 13 action 25 vs type 4 action 26

Authority: `Fable.exe` `00A03B40`–`00A04060` / `00AB4910` / `00AB4BB0` /
`00AB5420` / `00AB5B3D` / `0042E3EE` / `0054E280`. RTTI
`CInputTypeMouseButtonEvent` / `CInputTypeMouseMovementEvent`.

Investigation only. No production `src/` edits.

Status: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

---

## Verdict

**Click-on-button is type 4 / action 26. Type 13 / action 25 is mouse
move, not click. Accept is also action 26 (not a second event type).**

| Claim | Status |
| --- | --- |
| Event type 4 (`[record+40]`) → `0055CB10(26)` | **PROVEN** `0042E3EE` `0042E4A4` `push 26` |
| Event type 13 → `0055CB10(25)` + cursor store `+176/+180` | **PROVEN** `0042E5DC` |
| Type 4 ctor is `00A03C80` (`+40=4`, `+32=3`) | **PROVEN** |
| Type 13 ctor is `00A03FB0` (`+40=13`, `+32=3`, 12 bytes at `+12`) | **PROVEN** |
| `00A03B40` / `00A03B50` / `00A03B70` are constructors | **DISPROVEN** — getters |
| Type 13 is click-on-button | **DISPROVEN** |
| Type 4 is left mouse button down | **PROVEN** (`00AB4910` DIMOFS_BUTTON0 / `00AB4BB0` primary) |
| Type 13 is mouse / analog motion | **PROVEN** `00AB5B3D` |
| Action 25 posts widget `0xE5` / `0x126` / 15 | **DISPROVEN** `0054E319` |
| Action 26 posts stored widget message (accept / activate) | **PROVEN** `0054E2FA` / `0055AD60` / `0054DBC0` |
| Return (DIK 28) is type 4 | **DISPROVEN** — type 1 / action 33 |
| Xbox pad A also produces type 4 | **UNREAD** (`CInputTypeXboxPadButtonEvent`) |

`FrontendInputMap.TypeMouse=13` / `ActionMouse=25` is motion, not
click. Host must not map click → type 13.

---

## 1. `00A03B??` are getters; ctors are `00A03BE0+`

| VA | Role | Body |
| --- | --- | --- |
| `00A03B30` | nop | `ret` |
| `00A03B40` | type | `mov eax,[ecx+40]; ret` |
| `00A03B50` | device | `mov eax,[ecx+32]; ret` |
| `00A03B60` | field 36 | `mov eax,[ecx+36]; ret` |
| `00A03B70` | key / +0 | `mov eax,[ecx]; ret` |
| `00A03B80` | +0 again | `mov eax,[ecx]; ret` |
| `00A03B90` | +8 | `movzx eax,[ecx+8]; ret` |
| `00A03BA0` | float +12 | `fld [ecx+12]; ret` |
| `00A03BB0` | float +20 | `fld [ecx+20]; ret` |
| `00A03BC0` | set +24/+28 | copy 8 bytes from arg |
| `00A03BE0` | zero ctor | `[+40]=0`, `[+32]=0` |

Record ctors (write `[ecx+40]`):

| Ctor | `+40` type | `+32` device | Extra |
| --- | --- | --- | --- |
| `00A03BF0` | 1 | 2 | key at `+0` (keyboard) |
| `00A03C20` | 2 | 2 | |
| `00A03C50` | 3 | 2 | |
| **`00A03C80`** | **4** | **3** | `+24/+28` from ptr; same double `+44/+48` |
| `00A03CB0` | 15 | 2 | |
| `00A03CE0` | 16 | 2 | |
| `00A03D10` | 5 | 3 | + 12 bytes at `+0` |
| `00A03D60` | 6 | 3 | |
| `00A03D90` | 7 | 3 | |
| `00A03DC0` | 8 | 3 | |
| `00A03E10` | 9 | 3 | |
| `00A03E40` | 10 | 3 | |
| `00A03E70` | 11 | 3 | |
| `00A03EC0` | 12 | 3 | |
| **`00A03FB0`** | **13** | **3** | **12 bytes at `+12`**; `+24/+28` from ptr |
| `00A03FF0` | 14 | 3 | writes `+20` |
| `00A04030` | 17 | 1 | analog |
| `00A04060` | 18 | 1 | analog |

Device `+32=3` is the mouse-like device (same for type 4 and type 13).
Keyboard type 1 is device 2. Analog 17/18 is device 1.

---

## 2. `0042E3EE` classify

`00A03B40` then:

```
type 1  → last-key +192; 0055CB10(33); mask bits
type 4  → push 26                    ; 0042E4A4
type 6  → push 28
type 7  → push 35
type 10 → push 27
type 13 → [action+176]=[record+12]   ; 0042E5DC
          [action+180]=[record+16]
          push 25
type 14 → float +20 vs threshold → 36 / 37
type 15 → last-key from 00A03B80; push 34
type 17 → analog bits (no action 25/26)
```

`record+12/+16` are the first two dwords of the type-13 12-byte
payload (`00A03FB0` dest `lea eax,[ecx+12]`). Type 4 has no `+12`
copy.

---

## 3. Who builds type 4 (click)

`00AB5420` (`FrontendInputMap.Type4TranslateFn`) switch on source
`[esi+8]`. Case `00AB54F0` calls `00A03C80`.

Source kind 1 is left-button **down**:

`00AB4910` (DINPUT `GetDeviceData`, `HRESULT` `0x8007001E` =
`DIERR_INPUTLOST`). `DIDEVICEOBJECTDATA.dwOfs` 0–19 =
`DIMOUSESTATE2` (`lX=0`, `lY=4`, `lZ=8`, `rgbButtons[8]=12..19`).

| `dwOfs` | Kind `[edi+8]` |
| --- | --- |
| 0 X | 7 |
| 4 Y | 8 |
| 8 Z (wheel) | 10 |
| 12 BUTTON0 | down **1** / up 4 |
| 13 BUTTON1 | down 2 / up 5 |
| 14 BUTTON2 | down 3 / up 6 |

Button encode: `and al,0x80; neg; sbb; and -3; add 4` → down=1, up=4.

`00AB4BB0` (no DINPUT: `GetClientRect` / `GetCursorPos` /
`ScreenToClient`) edge-detects `009A4FC0` and writes the same
kind 1 / 4 pair (`dec; neg; sbb; and 3; inc` → press=1, release=4).

`00AB5420` `lea ecx,[eax-1]` then first index byte 0 → `00A03C80`.
Kind 1 (left down) is therefore type 4.

RTTI name: `CInputTypeMouseButtonEvent`.

---

## 4. Who builds type 13 (move)

Not in the `00AB5420` ctor table. Only call of `00A03FB0` is
`00AB5B3D` (same mouse process as `GetCursorPos` / analog accumulate):

```
fabs(esp+20/24/28) vs deadzone
  skip if all small
lea ecx, [esp+32]          ; 12-byte xyz
call [esi.vtbl+8]          ; dest +24/+28 (screen pos)
call 00A03FB0              ; +40=13, +12=xyz
call 00A66B20              ; enqueue
```

That is `CInputTypeMouseMovementEvent` (plus analog xyz at
`this+18508`), not a button.

---

## 5. Action 25 vs 26 on widgets

Type-10 inner `0054E280` (`lea eax,[ebx-26]`; table `0x54E32C`):

- Action **26** → `0054E2FA` UI vtbl+32 `0059A238` with `&widget+352`
  (`0xE5` on Press Start).
- After the table: `cmp ebx,25; je skip`. Action **25** does **not**
  post. It only skips the debounce timestamp at `+344`.

Type 11 / 38 (`0054DBC0` / `0055AD60`) also switch from action 26
and post persist `MessageId` (`0x126` / 15). Action 25 is not that
path.

Widget types 11 (`TypeButton`) and 38 (`TypeAccept`) are **persist
widget types**, not event `[record+40]`. They consume action 26.

---

## 6. “Accept” is not a distinct event type

Frontend accept / activate is **action 26**:

| Screen | Stored id | Poster |
| --- | --- | --- |
| Press Start (type 10) | `0xE5` | `0054E280` action 26 |
| `UI_ACCEPT_NEW_PROFILE` (type 38) | `0x126` | `0055AD60` action 26 |
| `UI_FRONTEND_BUTTON_NEW_GAME` (type 11) | 15 | `0054DBC0` → `0055AD60` |

The event that produces action 26 on frontend is type **4** (left
click). Return stays type 1 / action 33 (`00597BF2(1)`), not accept.

Pad-button → type 4 is **UNREAD**. Do not invent Start / A as type 4
without a `CInputTypeXboxPadButtonEvent` writer.

---

## 7. C#

`EngineInput` / `FrontendInputMap` already match the switch:
type 4 → 26, type 13 → 25. `MessageFromWidgets` posts only on
action 26. Do not treat `TypeMouse` as click. Physical host click
would queue **type 4**, not 13.

`proofs/type4-input-lifecycle` “physical device UNREAD” is now
**PROVEN** as left mouse button (DINPUT BUTTON0 / primary
`009A4FC0`). Pad remain UNREAD.
