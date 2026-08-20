# `0055CB10` remaining — UI action record, not player locomotion

Investigation only. No production `src/` or `tests/` edits.

Starting leftover label was **UNKNOWN** locomotion
(`docs/status/README.md` "`0055CB10` frontend
player-move listener"). Type4 current-inner apply
**is** this VA. Input `0042E3EE` dispatches
`0041E5F2` actions **0–5 / 20–21**, **not WASD**.
Host `Dispatch` records; native walks listeners.
No recovered player-move listener.

Do **not** invent WASD / Unity physics / a solver
from `CTCPhysicsStandard`. Do **not** treat recorded
actions as a Hero step. Do **not** start no-save
player control at this VA.

Authority: `proofs/0055CB10-body/README.md`,
`proofs/0055CB10-locomotion/README.md`;
dump `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
(`0055CB10`–`0055CBD0` / `0055CBE0` / `0055CF50`),
`listing-00400000.txt`
(`0041E5F2` / `0042E3EE` / `0042E4A4` / `0042E5AB` /
`0042E899` / `0042E971`);
`assembly/exe/00-index/vtbl.tsv` `01230134+0` /
`01230044+0`;
`src/Fable.Game/EngineInput.cs`
(`ActionApply` / `ActionApplyIsLocomotion` /
`Dispatch`);
`src/Fable.Game/PlayerInterface.cs`
(`PumpFn` `00446A30`);
`src/Fable.Game/FrontendInputMap.cs`
(`NativeType4UsesCurrentInner`);
`src/Fable.Game/RegionTravel.cs`
(`FirstSeenHandsPlayerControl`);
siblings `proofs/type4-current-inner-apply`,
`proofs/action26-subscribers`,
`proofs/audit-playerinterface`,
`proofs/hero-move-first-seen`,
`proofs/input-first-seen-control`.

Do not re-prove type 4 = LMB down → `push 26`, type 6
= LMB up → `push 28`, dest AABB hover `0055B8F0`,
Leave / `FinalAlbion.wld`, or `CTCPhysicsStandard`
factory size.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| Remaining body: UI action record only, or player locomotion? | **UI accept-then-apply.** Arg is an action dword. Callees are list copy `0055CF50` and dtor `0042AC25`. Zero XYZ / nav / `0x88` pose. | **PROVEN** UI; **DISPROVEN** locomotion |
| Type4 current-inner apply is this VA? | **Yes.** `0042E3EE` type 4 → `call 0041E5F2` / `push 26` / `call [edx]` = `01230134+0` = `0055CB10`. Apply is inner `vtbl+4`. | **PROVEN** |
| `0042E3EE` actions 0–5 / 20–21? | EndPoll encoder `0042E899` same `call [edx]` (`0042E971`; in-line 2+20 / 3+21). More **ids**, not a second fn. | **PROVEN** classify |
| Those ids are WASD? | **No.** Slots 0–3 are `0x6F/0x70/0x72/0x6D`. `DIK_A` is action **4**. `DIK_W` is slot 11 `KeyBit` 0. | **DISPROVEN** |
| Host `0055CB10`? | `EngineInput.Dispatch` = `_actions.Add(action)`. Records. No listener apply. | **PARTIAL** record |
| Recovered player-move listener? | **None.** First-seen nodes are type 11 `UI_FRONTEND_BUTTON_INVISIBLE` then type 32 `UI_MOUSE_POINTER`. | **UNREAD** leftover |
| First-seen `HandsPlayerControl`? | **`false`.** Frontend 2D (`0042DF9E`). No Hero. After Leave: `WorldFrame<=1` skips `00446A30`. | **MATCH** / **DISPROVEN** hands |
| Game pump is this VA? | **No.** Game is `PlayerInterface` `00416E78` → `00446A30`. Only PE caller of `0042E3EE` is `0042F0AC` (retail frontend). | **DISPROVEN** |

**Answer:** remaining UNKNOWN locomotion is **closed as
not walk**. First-seen no-save is **UI Type4
current-inner**. Host **records** 0–5 / 20–21. Who
accepts those ids as XYZ stays **UNREAD**. Do not
invent WASD to fill that gap.

---

## Verdict

**UI action record / broadcast. Not player locomotion.**

One `ret 4` body. INT3 pad `0055CB0A`–`0055CB0F`,
prologue `0055CB10`, `ret 4` `0055CBD0`. Indirect
only (`call [edx]` after `0041E5F2`). Two leftover
labels (Type4 current-inner vs locomotion UNKNOWN)
name the **same pointer**, not two fns.

| Slice | Native | Host | Remaining? |
| --- | --- | --- | --- |
| Body identity | focused `[this+8]` else `+12` else `+4`; accept `vtbl+8`, apply `vtbl+4` | `ActionApply=0x0055CB10`, `ActionApplyIsLocomotion=false` | **closed MATCH** |
| Type4 apply | `push 26` current-inner | dest AABB `HitIndex` | **LEFTOVER #14** (hover, not this body) |
| Encoder 0–5 / 20–21 | same `call [edx]` | `EndPoll` then `Dispatch` record | **PARTIAL** record |
| Player-move consumer | none recovered | none | **UNREAD leftover** |
| First-seen hands | frontend 2D, no Hero | `FirstSeenHandsPlayerControl=false` | **closed MATCH** |
| WASD | slots `0x6F/0x70/0x72/0x6D` | F2 `FlyCamera` never `QueueInput` | **DISPROVEN** invent |

Starting **UNKNOWN** is **not** a licence to wire
`Key.W/A/S/D` into `0055CB10`.

---

## 1. Body (listing — not a step)

`listing-00540000.txt`. `vtbl.tsv`: `01230134` slot
**0** = `0x0055CB10` (base `01230044` slot 0 is the
same pointer). Getter `0041E5F2` returns
`[0x13B8710]` (`0xD0`).

```
0055CB10  push ecx
0055CB11  push esi
0055CB12  mov  esi, ecx              ; this = input singleton
0055CB14  mov  eax, [esi+8]          ; focused inner*
0055CB17  test eax, eax
0055CB19  push edi
0055CB1A  je   0055CB3F              ; no focus → broadcast
0055CB1C  mov  edi, [esp+16]         ; action dword
0055CB20  mov  ecx, eax
0055CB22  mov  eax, [ecx]
0055CB24  push edi
0055CB25  call [eax+8]               ; accept
0055CB28  test al, al
0055CB2A  je   0055CBCD              ; exclusive even if reject
0055CB30  mov  ecx, [esi+8]
0055CB33  mov  edx, [ecx]
0055CB35  push edi
0055CB36  call [edx+4]               ; apply
0055CB3C  ret  4
0055CB3F  ; +12 nonempty → 0055CF50 copy, walk [node+8]
          ; else +4 same walk; 0042AC25 free copy
0055CBD0  ret  4
```

| Claim | Class |
| --- | --- |
| Arg0 is the pushed action | **PROVEN** `[esp+16]` / `[esp+20]` |
| Body loads Thing XYZ / dest AABB / pose `0x88` | **DISPROVEN** |
| Body `E8` nav / `006A9960` / `00723FD0` | **DISPROVEN** (only `0055CF50` / `0042AC25`) |
| Sibling `0055CBE0` (`ret 8`) is the `0042E3EE` site | **DISPROVEN** (`vtbl+1` match-one) |

Full decode: `proofs/0055CB10-body`. Not re-opened.

---

## 2. Type4 current-inner **is** this apply

`listing-00400000.txt`. Type from `00A03B40`
(`[record+40]`). `dec; sub 3; je` is type 4:

```
0042E4A4  call 0041E5F2
          push 26
          jmp  0042E5AB
0042E5AB  mov  edx, [eax]
          mov  ecx, eax
          call [edx]                 ; vtbl+0 = 0055CB10
```

First-seen `[input+8]==0` → broadcast of whoever
`input.vtbl+8(inner)` registered. Press Start nodes
(`action26-subscribers`):

| Node | Widget | Type4 (26) |
| ---: | --- | --- |
| 1 | type 11 `UI_FRONTEND_BUTTON_INVISIBLE` | `0054DBC0` → parent `+545` then `0055AD60` |
| 2 | type 32 `UI_MOUSE_POINTER` | `0055C6F0` (action 25 only) |

`FrontendInputMap.NativeType4UsesCurrentInner = true`.
`NativeType4UsesDestAabb = false`. **MATCH** dump.
Host Type4 via dest AABB `HitIndex` stays leftover
#14 (hover `0055BF10` / `0055B8F0` writes `+352`;
Type4 **reads** it). Not this remaining slice.

---

## 3. Encoder 0–5 / 20–21 — same `call [edx]`, not WASD

After harvest, `0042E899` priority-encodes `[ebp-4]`
/ `edi+252` and `call [edx]` at `0042E971`:

```
0042E8A8  call 0041E5F2 / push 4  / jmp 0042E971
0042E8B9  call 0041E5F2 / push 5  / jmp 0042E971
0042E8CD  call 0041E5F2 / push 22 / jmp 0042E971
0042E95D  call 0041E5F2 / push 2  / call [edx]   ; then push 20
0042E947  call 0041E5F2 / push 3  / call [edx]   ; then push 21
0042E93E  call 0041E5F2 / push 0  / jmp 0042E971
0042E935  call 0041E5F2 / push 1  / jmp 0042E971
0042E971  mov edx, [eax]
          mov ecx, eax
          call [edx]                 ; same 0055CB10
```

| Mask | Push |
| --- | ---: |
| `bh & 0x04` or `0x01` | 4 |
| `bh & 0x08` or `0x02` | 5 |
| `bl & 0x44` | **2 then 20** |
| `bl & 0x88` | **3 then 21** |
| `bl & 0x11` | 0 |
| `bl & 0x22` | 1 |

Type-1 compares (`0042E4C8`…) vs `0041DF10(0)`:

| Slot | Key | Mask | Encoder |
| ---: | ---: | --- | ---: |
| 0 | `0x6F` (111) | `0x4` | 2, 20 |
| 1 | `0x70` (112) | `0x8` | 3, 21 |
| 2 | `0x72` (114) | `0x2` | 1 |
| 3 | `0x6D` (109) | `0x1` | 0 |
| 6 | `0x1E` `DIK_A` | `0x100` | **4** (not walk) |
| 11 | `0x11` `DIK_W` | — | `KeyBit` 0 |

`DIK_S` / `DIK_D` are **absent** from
`EngineInput.KeyboardDefaults`. Type 17 stick
(`0042E608`) ORs NESW bits and **never** takes
`0042E5AB`. `TypeAnalogPostsActionApply=false`.

`docs/status` row `0042E3EE` type/key events
dispatch `0041E5F2` actions 0–5 / 20–21 (not WASD)
is **PROVEN**. That row is classify, not a move.

---

## 4. Host records; `PlayerInterface` is a different machine

```
EngineInput.ActionApply             = 0x0055CB10
EngineInput.ActionApplyIsLocomotion = false
EngineInput.Dispatch(action)        => _actions.Add(action)
RegionTravel.FirstSeenHandsPlayerControl = false
PlayerInterface.PumpFn              = 0x00446A30
```

Comment on `Dispatch`: "No recovered player-move
listener yet — actions are recorded." **MATCH**
record; **DISPROVEN** as a step.

Two pumps (`audit-playerinterface`):

```
0042EC7C retail frontend
  0042F0AC → 0042E3EE → 0041E5F2 → 0055CB10
  0042DF9E  2D Present
  // FirstSeenHandsPlayerControl=false

004184BD Init Game (after Leave)
  "Init Player Interface" 004473A0   // first construct
004189C2
  00416E78
    WorldFrame<=1: skip 004457F0 / 00446A30
    WorldFrame>1:  004457F0 then 00446A30
```

`PlayerInterface.cs` has **zero** `0055CB10`
references. It is `00446A30` → `00446330` /
`00446220`. Frontend has **no** that object.
**DISPROVEN** as first-seen apply.

`PumpInput` Notes `0055CB10 n={Input.Actions.Count}`
after `Input.Pump()`. Game never calls `PumpInput`.

---

## 5. First-seen no-save is not hands

```
0042EC7C  frontend Type4 0055CB10(26)   // UI
0042DF9E  2D Present                    // no Things
0042F2A2  Leave → FinalAlbion.wld
0042F491  Init Game
  CTCPhysicsStandard 0x88 pose          // sibling; this fn never touches it
004189C2  WorldFrame<=1 skip 00446A30
          no WalkTo / 006A9960
```

Tests: `EngineLifecycleTests` /
`WorldSceneTests` assert
`FirstSeenHandsPlayerControl==false` and
`ActionApplyIsLocomotion==false`.

No recovered first-seen node whose `vtbl+4` writes
Hero XYZ. **PROVEN** absence on frontend. Oakvale
`Hero.WalkTo` / player smash / F2 WASD are **not**
this spine (`hero-move-first-seen`).

---

## 6. What remains UNREAD

The leftover name “frontend player-move listener”
is **not** a second body. It is **who accepts
encoder ids 0–5 / 20–21 as XYZ**.

| Remaining | Class | Do not |
| --- | --- | --- |
| Listener whose `vtbl+4` treats 0–5 / 20–21 as a step | **UNREAD** | invent WASD / bind `Key.W` |
| Host listener walk (`Dispatch` is record-only) | **LEFTOVER** / **PARTIAL** | treat `_actions` as pose |
| Type4 apply via dest AABB `HitIndex` | leftover #14 | collapse into this VA |
| Game poll `00446462` / `004963E6` | status sibling UNREAD | merge into `0055CB10` |
| First-seen `input+8` writer | **UNREAD** (ctor 0) | guess focus |
| Physics solver so 0–5 “have somewhere to go” | first-seen **DISPROVEN**; solver **UNREAD** | invent capsule |

Close the **UNKNOWN locomotion** label on this VA.
Leave the **consumer** UNREAD.

---

## 7. Do not invent

- Player walk / WASD / `DIK_W` as `0055CB10`.
- Two functions because Type4 apply and locomotion
  UNKNOWN were two leftover rows.
- First-seen no-save Hero control at this VA
  (`FirstSeenHandsPlayerControl` stays `false`).
- `PlayerInterface.Pump` as this apply.
- A physics tick so encoder ids complete this fn.

**Proposed (do not apply here):** keep
`ActionApplyIsLocomotion=false`. Keep Type4
current-inner classify. Keep `Dispatch` record.
Leave 0–5 / 20–21 consumer **UNREAD**. Do not
invent WASD.

---

## Classifications (short)

1. **Remaining body — PROVEN UI action record.**
   Input `vtbl+0` accept/apply. Starting
   **UNKNOWN** locomotion is **not walk**.
2. **Type4 current-inner — PROVEN** this VA
   (`push 26`). Dest AABB is hover leftover #14.
3. **Actions 0–5 / 20–21 — PROVEN classify**,
   **DISPROVEN WASD**. Host **PARTIAL** record.
4. **Player-move listener — UNREAD leftover.**
   First-seen consumers are frontend type 11 / 32.
5. **First-seen hands — DISPROVEN.**
   `FirstSeenHandsPlayerControl=false`.
6. **`PlayerInterface` — DISPROVEN** as this
   apply. Game pump is `00446A30` after
   `WorldFrame>1`.

---

## Sources

- `C:\FableCSharp\proofs\0055CB10-body\README.md`
- `C:\FableCSharp\proofs\0055CB10-locomotion\README.md`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00540000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\assembly\exe\00-index\vtbl.tsv`
- `C:\FableCSharp\src\Fable.Game\EngineInput.cs`
- `C:\FableCSharp\src\Fable.Game\PlayerInterface.cs`
- `C:\FableCSharp\src\Fable.Game\FrontendInputMap.cs`
- `C:\FableCSharp\src\Fable.Game\RegionTravel.cs`
- `C:\FableCSharp\proofs\type4-current-inner-apply\README.md`
- `C:\FableCSharp\proofs\action26-subscribers\README.md`
- `C:\FableCSharp\proofs\audit-playerinterface\README.md`
- `C:\FableCSharp\proofs\hero-move-first-seen\README.md`
- `C:\FableCSharp\docs\status\README.md`
