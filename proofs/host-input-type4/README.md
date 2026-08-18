# Host never queues type 4 (LMB down)

Investigation only. No production `src/` edits.

Authority: `src/Fable.Client/SilkEngineHost.cs`,
`src/Fable.Client/Program.cs`,
`src/Fable.Game/EngineInput.cs`,
`src/Fable.Game/FrontendInputMap.cs`,
`src/Fable.Game/EngineLifecycle.cs` (`QueueInput` / `PumpInput` /
`PumpFrontendFrame` / `MaybeActivateNewGameFromInput` /
`QueuedPlayAviSkip`),
`src/Fable.Game/IEngineHost.cs`,
`src/Fable.Game/RegionTravel.cs` (`PlayAviSkip*`),
`src/Fable.Render/FlyCamera.cs`;
dump `Fable.exe` `00A03C80` / `0042E3EE` / `00AB5420` /
`00AB4910` / `00AB4BB0` / `00A03BF0` / `00A03FB0`
(`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00a00000.txt`,
`listing-00400000.txt`, `listing-00a80000.txt`);
`proofs/type4-dinput-raw/README.md`,
`proofs/type13-vs-type4/README.md`,
`proofs/audit-lifecycle-input/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **MATCH**.

Do not invent pixel click targets.

---

## Verdict

**No. Live `Fable.Client` never `QueueInput`s type 4.**
`SilkEngineHost` has no input path at all. `Program.cs` only
queues type 1 (`EngineInput.TypeKey`). Silk LMB is unread.
Silk Enter is type 1 / DIK 28, not type 4.

| Claim | Status |
| --- | --- |
| `SilkEngineHost` calls `QueueInput` / `Input.Queue` | **DISPROVEN** — Present-only `009BEEB0` |
| `Program.cs` queues `EngineInput.Type4` (4) | **DISPROVEN** — six type-1 sites only |
| Live LMB → type 4 | **DISPROVEN** — `MouseButton.Left` never read |
| Native type 4 ctor is `00A03C80` (`[+40]=4`, `[+32]=3`) | **PROVEN** |
| Native type 4 is LMB down (`DIMOFS_BUTTON0` / primary `009A4FC0`) | **PROVEN** `00AB4910` / `00AB4BB0` / `00AB5420` |
| Native type 1 ctor is `00A03BF0` (`[+40]=1`, `[+32]=2`, key at `+0`) | **PROVEN** |
| Live Enter → type 1 / DIK 28 | **PROVEN** `Program.cs` + `PlayAviSkipReturn` |
| Enter / type 1 → action 26 / `0xE5` / `0x126` / 15 | **DISPROVEN** `0042E3EE` type 1 → action 33 |
| Type 13 is click | **DISPROVEN** — mouse move (`00A03FB0`) |
| Host RMB / move / F2-WASD as engine events | **LEFTOVER** debug; never queued |
| Pad A / Start as type 4 | **UNREAD** |
| Click-at-widget dest as the type-4 producer | **UNREAD** / do not invent |

---

## 1. Live client: click vs Enter

`window.Update` in `Program.cs` is the only host poll.
`SilkEngineHost` does not implement input; `IEngineHost` is
width / height / title / `Present` / `Quit`.

### Click (LMB)

Nothing is sent to the engine.

- No `MouseButton.Left` in `src/Fable.Client`.
- RMB is only `looking = debugFly && mouse.IsButtonPressed(MouseButton.Right)`.
- `MouseMove` writes `debugCam.Look` when `looking`; never `QueueInput`.
- Cursor mode is `Disabled` only while that debug look is on.

A live click therefore does **not** produce `00A03C80`, does
**not** walk `0042E3EE` type 4, and does **not** post action 26.

### Enter

Every frame `Key.Enter` is down:

```
life.QueueInput(EngineInput.TypeKey, RegionTravel.PlayAviSkipReturn);
```

`TypeKey = 1`, `PlayAviSkipReturn = 28` (`DIK_RETURN`).
That is the native type-1 record (`00A03BF0`), not type 4.

`EngineLifecycle.Pump`:

- Startup AVI: `PumpInput` then `QueuedPlayAviSkip()` — DIK 1 / 57 / 28 / 62
  skip the video (**MATCH** PlayAVI skip set).
- Frontend: `PumpFrontendFrame` → `PumpInput` →
  `MaybeActivateNewGameFromInput`. Type 1 maps to action 33;
  `MessageFromWidgets` posts only action 26. Enter does not
  leave PRESS_START.

Held keys re-queue every `Update` (`IsKeyPressed`). Native
`GetDeviceData` is a buffered edge. That hold-repeat is
**LEFTOVER**, not a type-4 substitute.

---

## 2. Native: `00A03C80` then `0042E3EE`

### Record ctor `00A03C80` (`listing-00a00000.txt`)

```
00A03C80  mov eax, [esp+4]
00A03C84  fld qword [esp+8]
00A03C88  mov [ecx+32], 0x3        ; mouse-like device
00A03C8F  mov [ecx+40], 0x4        ; event type 4
00A03C96  mov edx, [eax]
00A03C98  mov [ecx+24], edx        ; origin pair
00A03C9B  mov eax, [eax+4]
00A03C9E  fst [ecx+48]
00A03CA1  fstp [ecx+44]
00A03CA4  mov [ecx+28], eax
00A03CA7  ret 12
```

No DIK at `[record+0]`. Getter `00A03B40` is
`mov eax, [ecx+40]; ret`. Sole `.text` E8: `00AB5500`
inside translator `00AB5420`.

Sibling keyboard ctor `00A03BF0`: `[+40]=1`, `[+32]=2`,
key dword at `[ecx]`. Mouse-move ctor `00A03FB0`:
`[+40]=13`, `[+32]=3`, 12 bytes at `+12`.

### Translator: LMB down → raw 1 → type 4

`00AB5420` `lea ecx, [eax-1]` on sample `[esi+8]`. Index
`0xAB56EC` maps **only raw 1** to jt[0] `00AB54F0` →
`00A03C80`. See `proofs/type4-dinput-raw`.

`00AB4910` (`GetDeviceData`, `DIERR_INPUTLOST` `0x8007001E`):
`dwOfs` 12 = `DIMOFS_BUTTON0`. `dwData & 0x80`:

```
and al, 0x80; neg; sbb; and -3; add 4
→ down raw 1, up raw 4
```

Raw 4 is type 6 (`00A03D60`), not type 4.

Non-DINPUT `00AB4BB0` (`GetClientRect` / `GetCursorPos` /
`ScreenToClient`): primary getter `009A4FC0`
(`mov al, [ecx+221]`). Edge vs `[this+13356]`:

```
dec; neg; sbb; and 3; inc
→ press raw 1, release raw 4
```

Same raw 1 → `00A03C80`. Cursor origin copied into type 4
`+24/+28` from `this+13332/+13336`, not a host-chosen widget
pixel.

### Classify `0042E3EE` (`listing-00400000.txt`)

`00A03B40` type, then (`dec eax` / `sub eax, 3` chain):

| `[record+40]` | Site | Action |
| ---: | --- | --- |
| 1 | `0042E4B0` | last-key `00A03B70`; `push 33` |
| **4** | **`0042E4A4`** | **`push 26`** (no DIK compare) |
| 6 | `0042E498` | `push 28` |
| 7 | `0042E48C` | `push 35` |
| 10 | `0042E557` | `push 27` |
| 13 | `0042E5DC` | store `+12/+16` → action `+176/+180`; `push 25` |
| 15 | `0042E56F` | last-key `00A03B80`; `push 34` |
| 17 | `0042E608` | analog bits |

Type 1 also ORs movement / A / B mask bits (`111/112/109/114/30/48/21`)
before action 33. Type 4 does not.

C# `EngineInput.ApplyEvent` / `FrontendInputMap.ActionFromEvent`
**MATCH** that classify: type 4 → 26, type 1 → 33, type 13 → 25.
`MaybeActivateNewGameFromInput` posts stored widget id only
on action 26.

---

## 3. Host mouse / keyboard → native types

What the **live** host actually emits vs what native would
build. Silk names are GLFW/Silk enums, not DIK.

| Host | `Program.cs` | Queued `(type, key)` | Native ctor / type | Native action (`0042E3EE`) | Class |
| --- | --- | --- | --- | --- | --- |
| LMB | unread | **none** | `00A03C80` type **4** device 3 | **26** | **DISPROVEN** as host; native **PROVEN** |
| LMB up | unread | none | raw 4 → type **6** | 28 | unread on host |
| RMB | debug look only | none | `DIMOFS_BUTTON1` raw 2/5 | not 26 | **LEFTOVER** |
| Mouse move | `debugCam.Look` if F2-look | none | `00A03FB0` type **13** | 25 (cursor store) | **LEFTOVER** |
| Enter | `QueueInput(TypeKey, 28)` | **(1, 28)** | `00A03BF0` type 1 device 2 | 33; PlayAVI skip | **MATCH** skip / **DISPROVEN** as accept |
| Escape | `(TypeKey, 1)` | (1, 1) | type 1 `DIK_ESCAPE` | 33; PlayAVI skip | **MATCH** skip |
| Space | `(TypeKey, 57)` | (1, 57) | type 1 `DIK_SPACE` | 33; PlayAVI skip | **MATCH** skip |
| F4 | `(TypeKey, 62)` | (1, 62) | type 1 `DIK_F4` | 33; PlayAVI skip | **MATCH** skip |
| A | `(TypeKey, 0x1E)` | (1, 30) | type 1; mask `0x100` | 33 then poll action 4 | **LEFTOVER** every stage |
| B | `(TypeKey, 0x30)` | (1, 48) | type 1; mask `0x200` | 33 then poll action 5 | **LEFTOVER** every stage |
| F2 | toggles `debugFly` | none | none | none | **LEFTOVER** invented debug |
| W/S/D/Q/E (+A while fly) | `debugCam.Move` | none | not WASD (`0x6F/0x70/0x72/0x6D`) | none | **LEFTOVER** |
| Shift (fly speed) | `debugCam` only | none | none | none | **LEFTOVER** |

`SilkEngineHost` column is empty for every row: no queue,
no mouse, no keyboard.

Tests may `QueueInput(EngineInput.Type4, 0)`. That is not
the live client. Key is unused on type 4 (**MATCH** dump).

---

## 4. Leftover host binds (classified)

| Bind | What it does | Native | Class |
| --- | --- | --- | --- |
| `Key.Enter/Escape/Space/F4` → type 1 | PlayAVI skip on startup; frontend action 33 | skip set **PROVEN**; UI accept **DISPROVEN** | skip **MATCH**; UI **LEFTOVER** if treated as New Game |
| `Key.A` / `Key.B` → type 1 every stage | `ApplyEvent` mask → poll actions 4 / 5 | frontend classify **EQUIVALENT**; not walk, not Leave | **LEFTOVER** on frontend / AVI |
| F2 + WASD/QE + RMB look | `FlyCamera` only; `Present` still engine camera unless `debugFly` | no `QueueInput`; native move keys are not WASD | **LEFTOVER** invented debug |
| `FlyCamera.cs` | must not write script camera | comment is the contract | **LEFTOVER** |
| `IsKeyPressed` hold | re-queues type 1 every frame | DINPUT buffer / edge | **LEFTOVER** / **DIVERGE** |
| `MouseButton.Left` | absent | type 4 producer | host **UNREAD** (native **PROVEN**) |
| Click dest / widget rect | absent | type 4 is device down; origin pair is poll state | do not invent |
| Xbox pad → type 4 | absent | `CInputTypeXboxPadButtonEvent` | **UNREAD** |

Stale: `audit-frontend-leftover` §3.3 “do not guess … click”
was written before `00AB4910` recovered LMB. Device is now
**PROVEN** as button 0. Live host still does not queue it.
`type4-second-ctor` “which `00AB56C4` id” is recovered as
raw **1** (`type4-dinput-raw`).

---

## 5. Do not invent pixel click targets

Type 4 is **left-button down on the mouse device**, not
“click this dest”. `00A03C80` does not take a widget id.
`+24/+28` are the current origin from the `0129EA14` poll
object, not a picked `0041AFA0` rect.

Do **not**:

- Map LMB → type 13 (`TypeMouse`).
- Map Enter / Space / A / Start → type 4.
- Queue type 4 only if the cursor is inside PRESS_START /
  NEW_GAME dest (no recovered host hit-test here).
- Invent a click pixel, forest-tile UV, or TITLE rect as
  the accept producer.

If the host later queues type 4, it is the recovered
`00A03C80` record (type 4, unused key). Widget posting stays
`0054E280` / `0054DBC0` / `0055AD60` on action 26.

---

## 6. Answers

**Does `Fable.Client` / `SilkEngineHost` ever `QueueInput` type 4 (LMB down)?**
**No.** `SilkEngineHost` never queues. `Program.cs` never
queues type 4. LMB is not sampled.

**Live click vs Enter?**
Click: no engine event. Enter: `(TypeKey=1, 28)` every
held frame — PlayAVI skip and frontend action 33, not
action 26.

**Native analog the host is missing?**
`00AB4910`/`00AB4BB0` raw 1 → `00AB5420` → `00A03C80`
`[+40]=4` → `0042E3EE` `push 26`.
