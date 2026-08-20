# `0055CB10` is not first-seen locomotion

Investigation only. No production `src/` edits.

Do **not** invent WASD / Unity physics / a solver from
`CTCPhysicsStandard`. Do **not** start no-save player
control at `0055CB10`, Lookout rocks, or F2 `FlyCamera`.

Starting label was **UNKNOWN** (unread). Status words
after this dump: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**. Unread stays
**UNREAD**, not a guessed apply.

Authority: `listing-00540000.txt` `0055CB10` /
`0055CBE0` / `0055CF50`; `listing-00400000.txt`
`0042E3EE` / `0042E5AB` / `0042E608` / `0042E899`;
`assembly/exe/00-index/vtbl.tsv` `01230134+0`;
`src/Fable.Game/EngineInput.cs`
(`ActionApply` / `ActionApplyIsLocomotion`);
`RegionTravel.FirstSeenHandsPlayerControl`;
`proofs/collision-first-seen`;
`proofs/creature-move-first`;
`proofs/action26-subscribers`;
`proofs/audit-playerinterface`;
`proofs/input-vtbl56-vs-ui32`;
`proofs/pad-a-vs-type4`;
`docs/status/README.md` leftover
"`0055CB10` frontend player-move listener".

Do not re-prove type 4 → `push 26` consumer
`0059A238`, Leave / `FinalAlbion.wld`, or
`CTCPhysicsStandard` factory size.

---

## Verdict

**`0055CB10` is input `vtbl+0`: accept then apply one
action id on a listener list.** It is **not** a walk
step, **not** a pose writer, **not** first-seen no-save
player control.

| Question | Answer | Class |
|---|---|---|
| What is `0055CB10`? | `01230134+0` on `[0x13B8710]` (`0xD0`). Focused `[this+8]` else broadcast `[this+12]` else `[this+4]`. Accept `listener.vtbl+8(action)`, apply `vtbl+4(action)`. | **PROVEN** listing |
| Locomotion apply / mesh step / dest write? | **No.** Zero `E8` of nav / physics / `006A9960`. Arg is the pushed action, not XYZ. | **DISPROVEN** |
| First-seen no-save *player control*? | **No.** `FirstSeenHandsPlayerControl=false`. Frontend is 2D (`0042DF9E`). No Hero Thing. | **DISPROVEN** |
| First-seen consumers of the walk? | Press Start: type 11 `UI_FRONTEND_BUTTON_INVISIBLE`, type 32 `UI_MOUSE_POINTER`. | **PROVEN** objects |
| Type 4 / 6 / 13 / 1 on this fn? | `push 26` / `28` / `25` / `33` then `call [edx]`. UI / last-key, not a step. | **PROVEN** |
| Mask encoder actions 0–5 / 20–21? | After the poll (`0042E899`). Same `call [edx]`. Host **records**. | **PROVEN** classify |
| Who applies 0–5 / 20–21 as a move? | No recovered listener. Status leftover. | **UNREAD** |
| Stick type 17 posts `0055CB10`? | **No.** `0042E608` ORs NESW bits only. `TypeAnalogPostsActionApply=false`. | **DISPROVEN** |
| Movement keys are WASD? | Slots 0–3 are `0x6F/0x70/0x72/0x6D`. `DIK_W` is not a move bit. | **DISPROVEN** |
| Collision after Leave? | `CTCPhysicsStandard` alloc `0x88`, TNG pose. No solver. | **PROVEN** pose; solver **UNREAD**; first-seen solver **DISPROVEN** |
| Host `ActionApplyIsLocomotion` | `false` (locked). | **MATCH** |

Host `EngineInput.Dispatch` records the id. That is
**PARTIAL** vs the native walk (no listener apply).
It is **not** a licence to invent WASD.

---

## Timeline (no-save New Game)

```
0042EC7C retail frontend
  0042E3EE  only caller 0042F0AC
    harvest 009F4ED0 / 009F4F10
    type 1  → +192=key; push 33; mask OR
    type 4  → push 26                 // LMB; Press Start 0xE5
    type 6  → push 28
    type 10 → push 27
    type 13 → +176/+180; push 25      // pointer; not a step
    type 17 → OR NESW bits; no push   // DISPROVEN 0055CB10
    type 19 → pad bits (A → action 22)
    EndPoll priority encoder
      0x400/0x100 → 4
      0x800/0x200 → 5
      0x20000     → 22
      0x44        → 2 then 20
      0x88        → 3 then 21
      0x11        → 0
      0x22        → 1
    each push → call [edx] = 0055CB10
  0042DF9E  2D Present                // no Things
  // FirstSeenHandsPlayerControl=false

0042F2A2 Leave
  FinalAlbion.wld
0042F491 Init Game
  004EE790  CTCPhysicsStandard  00BFEA1A(0x88) → 00723FD0
  00A15670  CNavigatorManager                 // register, not a step
004189C2 first pumps
  WorldFrame<=1: skip 004457F0 / 00446A30
  no 00501450, no 006AC910, no WalkTo
later
  00501450 Lookout  → pose persist; 006A80A0 bit 0x64
  006AC910 CREATURE_HERO @ GuildArrivalHSP
    004C9D60("CTCPhysicsControlled")          // placement
  00416E78 WorldFrame>1 → 00446A30            // not 0042E3EE
```

`00DBDE40` / Oakvale `Hero.WalkTo` / F2 WASD are
**not** on this list. **PROVEN.**

---

## 1. Listing `0055CB10` (`listing-00540000.txt`)

`vtbl.tsv`: `01230134` slot **0** = `0x0055CB10`.
Ctor `0041E3F6` writes that vtbl. Getter `0041E5F2`
returns `[0x13B8710]`.

```
0055CB10  push ecx
          push esi
          mov  esi, ecx              ; this = input singleton
          mov  eax, [esi+8]
          test eax, eax
          push edi
          je   0055CB3F              ; no focus → broadcast
0055CB1C  mov  edi, [esp+16]         ; action
          mov  ecx, eax              ; focused listener*
          mov  eax, [ecx]
          push edi
          call [eax+8]               ; accept
          test al, al
          je   0055CBCD              ; exclusive even if reject
          mov  ecx, [esi+8]
          mov  edx, [ecx]
          push edi
          call [edx+4]               ; apply
          ret  4
0055CB3F  mov  ecx, [esi+12]
          mov  edx, [ecx]            ; head->next
          lea  eax, [esi+12]
          cmp  edx, ecx
          je   0055CB90              ; +12 empty → +4
          push eax
          call 0055CF50              ; circular 12-byte copy
          ; for node in copy:
          ;   listener = [node+8]
          ;   if listener.vtbl+8(action): listener.vtbl+4(action)
          call 0042AC25
          ret  4
0055CB90  add  esi, 4                ; list +4
          push esi
          call 0055CF50
          ; same accept / apply walk
          ret  4
```

| Claim | Class |
|---|---|
| `this` is the action singleton | **PROVEN** (`0042E5AB` `call [edx]` after `0041E5F2`) |
| Arg0 is the pushed action dword | **PROVEN** `[esp+16]` / `[esp+20]` |
| Focused path is exclusive | **PROVEN** (`je 0055CBCD` then `ret 4`) |
| Broadcast prefers `+12` else `+4` | **PROVEN** |
| `0055CF50` copies so apply can mutate | **PROVEN** (`action26-subscribers`) |
| Body reads `+176/+180` / Thing XYZ / `0x88` pose | **DISPROVEN** (no such loads) |
| Body `E8` nav / `006A80A0` / `006A9960` / `00723FD0` | **DISPROVEN** (callees are `0055CF50` / `0042AC25` only) |
| First-seen frontend `+8` | ctor 0; writer **UNREAD**; classify broadcast **PARTIAL** |

Sibling `0055CBE0` is `01230134+1` (two-arg walk that
matches one listener). It is **not** the `0042E3EE`
site. **DISPROVEN** as first-seen apply.

C# / tests:

```
EngineInput.ActionApply              = 0x0055CB10
EngineInput.ActionApplyIsLocomotion  = false
EngineInput.TypeAnalogPostsActionApply = false
RegionTravel.FirstSeenHandsPlayerControl = false
```

`Dispatch` is `_actions.Add(action)` with the comment
"No recovered player-move listener yet". **MATCH**
record; **DISPROVEN** as a step.

---

## 2. How ids reach `call [edx]` (`0042E3EE`)

`listing-00400000.txt`. Only PE caller is `0042F0AC`
(retail frontend). Game after Leave is `00416E78` →
`00446A30` (`audit-playerinterface`). **DISPROVEN**
as the first game pump.

### 2a. Per-event pushes (then `0042E5AB`)

```
0042E5AB  mov edx, [eax]
          mov ecx, eax
          call [edx]                 ; 01230134+0 = 0055CB10
```

| Type | Site | Push | First-seen |
| ---: | --- | ---: | --- |
| 1 | `0042E4B0` | 33 after last-key `vtbl+48` | last-key / mask bits |
| 4 | `0042E4A4` | 26 | Press Start `0xE5` if type-10 apply runs |
| 6 | `0042E498` | 28 | armed release |
| 7 | `0042E48C` | 35 | RMB down |
| 10 | `0042E557` | 27 | hover-in; **not** world Use |
| 13 | `0042E5DC` | 25 after `+176/+180` | pointer; `0055CB10` does not read the pair |
| 15 | `0042E56F` | 34 | WM_CHAR |
| 17 | `0042E608` | **none** | NESW `or [ebp-4], 2/1/8/4` |
| 19 id 2 | `0042E72E` | later 22 | pad A; **not** type 4 |

Type 17 never takes `0042E5AB`. **PROVEN.**
`EngineInput.TypeAnalogPostsActionApply=false`.

### 2b. EndPoll encoder (`0042E899`)

Same `call [edx]` at `0042E971` / in-line 2+20 / 3+21.

| `[ebp-4]` / `edi+252` | Actions |
| --- | ---: |
| `bh & 0x04` or `0x01` (`0x400` / `0x100`) | 4 |
| `bh & 0x08` or `0x02` (`0x800` / `0x200`) | 5 |
| `0x20000` | 22 |
| `bl & 0x44` | **2 then 20** |
| `bl & 0x88` | **3 then 21** |
| `bl & 0x11` | 0 |
| `bl & 0x22` | 1 |
| `bh` 0x10 / 0x20 / 0x40 / sign | 8 / 9 / 10 / 11 |
| `0x10000` | 23 |

`EngineInput.EndPoll` **MATCH**es this priority.
Host then `Dispatch` records. Native still walks
listeners. Who accepts 0–5 / 20–21 as XYZ
**UNREAD** (status leftover). Do not fill that
gap with WASD.

### 2c. Keys that set the bits — not WASD

`0041DF10(0)` defaults at `+36`. Type-1 compares
(`0042E4C8`…):

| Slot | Key | Mask | Encoder |
| ---: | ---: | ---: | --- |
| 0 | `0x6F` (111) | `0x4` | 2, 20 |
| 1 | `0x70` (112) | `0x8` | 3, 21 |
| 2 | `0x72` (114) | `0x2` | 1 |
| 3 | `0x6D` (109) | `0x1` | 0 |
| 6 | `0x1E` `DIK_A` | `0x100` | **4** |
| 7 | `0x30` `DIK_B` | `0x200` | **5** |
| — | `0x15` `DIK_Y` | `0x20000` | 22 |

`DIK_W=0x11` is default slot 11. `KeyBit` 0. `DIK_S`
/ `DIK_D` are **absent** from `KeyboardDefaults`.
`DIK_A` is action 4, not walk.

Host F2 `FlyCamera` WASD is **LEFTOVER** debug
(`audit-playerinterface`). It never `QueueInput`.

---

## 3. First-seen listeners are UI, not a mover

`action26-subscribers` / `0055C650-type32-ctor`:

| Node | Widget | Apply |
| ---: | --- | --- |
| 1 | `UI_FRONTEND_BUTTON_INVISIBLE` type 11 | `0054DBC0` → arm `0055AD60` |
| 2 | `UI_MOUSE_POINTER` type 32 | `0055C6F0` (action 25 only) |

Ctor register is input `vtbl+8(inner)`. Type 10
ctor has **no** that call — type-10 as a first-seen
node stays **UNREAD**. Type-10 apply `0054E2FA` is
UI `vtbl+32` (`0xE5`), **DISPROVEN** as locomotion.

`0055CB10` does **not** call `0041E6D3` (input
`vtbl+56`) and does **not** call `0059A238`.
`input-vtbl56-vs-ui32`.

No recovered first-seen node whose `vtbl+4` writes
Hero / Thing XYZ. **PROVEN** absence on frontend.
After Leave, a game-mode consumer of 0–5 / 20–21
stays **UNREAD**.

---

## 4. First-seen no-save is not player control

`RegionTravel.FirstSeenHandsPlayerControl = false`
(`WorldSceneTests` / `EngineLifecycleTests`).

| Stage | Control? | Class |
|---|---|---|
| Frontend Present | 2D UI. No Hero, no `004473A0`, no `006AC910`. | **DISPROVEN** |
| Hands after Leave, first pumps | `WorldFrame<=1` skips `00446A30`. First hit `al=0`, no `0041649C`. | **PROVEN** skip |
| Oakvale smash / WalkTo | leftover `00DBDE40` / `00CBFB7D`. | **LEFTOVER**; **DISPROVEN** as Leave |
| Player smash on first Present | needs control + barrels | **DISPROVEN** (`watchbarrels-smash-vtbl20`) |

So `0055CB10` on the no-save spine is the **frontend
action fan-out**, then (after Leave) is **not** the
game pump. It does not hand control.

---

## 5. Collision sibling — pose `0x88`, no solver

`proofs/collision-first-seen` (do not re-open):

| Object | Role | First-seen |
|---|---|---|
| `CTCPhysicsStandard` `004EE790` | factory `004D297B` `push 0x88` / ctor `00723FD0` | type **PROVEN** |
| Persist `00724290` | TNG `Position*` + `RHSetForward/Up` | pose **PROVEN** |
| `006A80A0` | bit `0x64` on `thing+32` after `00501450` | collect **PROVEN**; "collidable" **UNREAD** |
| Solver / capsule / contact | none recovered | **UNREAD**; invent **DISPROVEN** |
| First locomotion / mesh step | none (`creature-move-first`) | **PROVEN** absence |

`0055CB10` never touches the `0x88` object. A later
unread move listener would still have **no** first-seen
solver to bounce off. Do not invent one to "complete"
this fn.

---

## 6. Host already models vs leftover

| Host | Native | Class |
|---|---|---|
| `ActionApply = 0x0055CB10` | `01230134+0` | **MATCH** |
| `ActionApplyIsLocomotion = false` | listing: listener walk | **MATCH** |
| `Dispatch` records 0–5 / 20–21 / 26 / … | `call [edx]` | **PARTIAL** (no walk) |
| `TypeAnalogPostsActionApply = false` | `0042E608` no push | **MATCH** |
| `FirstSeenHandsPlayerControl = false` | no first-seen handoff | **MATCH** |
| KeyboardDefaults `0x6F/0x70/0x72/0x6D` | `0041DF10(0)` | **MATCH** |
| Type 4 → 26 | `push 26` | **MATCH** classify |
| F2 WASD `FlyCamera` | not `0042E3EE` / not `00416E78` | **LEFTOVER** |
| `TickMove` lerp | `004C72B0` stub | **LEFTOVER** |
| Player-move listener | none | **UNREAD** |

---

## Classifications (short)

1. **`0055CB10` identity — PROVEN.** Input apply /
   broadcast. Listing read. Starting **UNKNOWN**
   is closed as **not locomotion**.
2. **Locomotion / player-control apply — DISPROVEN.**
   No pose, no dest, no solver call. First-seen
   consumers are frontend type 11 / 32.
3. **First-seen no-save hands — DISPROVEN.**
   `FirstSeenHandsPlayerControl=false`. Frontend
   2D. After Leave: skip then empty `00446A30`.
4. **Actions 0–5 / 20–21 classify — PROVEN.** Encoder
   + `call [edx]`. Consumer as a move **UNREAD**.
5. **WASD — DISPROVEN.** Slots `0x6F/0x70/0x72/0x6D`.
   Type 17 does not enter this fn.
6. **Collision — pose `0x88` PROVEN; solver UNREAD;
   first-seen solver DISPROVEN.** Sibling
   `collision-first-seen`. Not this fn.

Do not wire `Key.W/A/S/D` into `0055CB10`.
Do not treat recorded actions as a Hero step.
Do not invent a physics tick so those actions
"have somewhere to go."
