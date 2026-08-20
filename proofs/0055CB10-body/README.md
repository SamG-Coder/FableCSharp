# `0055CB10` body — one apply, not player walk, not dest AABB

Investigation only. No production `src/` or `tests/` edits.

Starting label was **UNKNOWN** (locomotion). Question: is
`0055CB10` **player walk**, **UI Type4**, or **both**
(overloaded name)? First-seen no-save? Type4 apply vs dest
AABB leftover #14.

Do **not** invent WASD / Unity physics / a solver from
`CTCPhysicsStandard`. Do **not** treat recorded actions
0–5 / 20–21 as a Hero step.

Authority: dump `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
(`0055CB10`–`0055CBD0` / `0055CBE0` / `0055CF50`),
`listing-00400000.txt`
(`0041E3F6` / `0041E5F2` / `0042BE7B` / `0042E3EE` /
`0042E4A4` / `0042E5AB` / `0042E899` / `0042AC25`),
`e8.tsv` (no `.text` `E8 0055CB10`),
`assembly/exe/00-index/vtbl.tsv` `01230044+0` /
`01230134+0`;
`src/Fable.Game/EngineInput.cs`
(`ActionApply` / `ActionApplyIsLocomotion`);
`src/Fable.Game/FrontendInputMap.cs`
(`NativeType4UsesCurrentInner`);
`src/Fable.Client/SilkNativeInput.cs`;
`proofs/0055CB10-locomotion/README.md`;
`proofs/type4-current-inner-apply/README.md`;
`proofs/leftover-14-dest-aabb/README.md`;
`proofs/action26-subscribers/README.md`;
`docs/status/README.md` leftover
"`0055CB10` frontend player-move listener".

Do not re-prove type 4 = LMB down → `push 26`, type 6 =
LMB up → `push 28`, dest AABB writer on Present, or
`CTCPhysicsStandard` factory size.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| Player walk / mesh step / dest write? | **No.** Arg is an action dword. Callees are list copy `0055CF50` and dtor `0042AC25`. Zero XYZ / nav / `0x88` pose. | **DISPROVEN** |
| UI Type4 apply? | **Yes.** `0042E3EE` type 4 → `push 26` → `call [edx]` = this body. Apply is **current-inner** (`widget+4`). | **PROVEN** |
| Both via overloaded name? | **No.** One VA, one `ret 4` body. Two vtbls share slot 0 because `01230134` overwrites base `01230044` after `0042BE7B`. Same pointer, not two fns. | **DISPROVEN** overload |
| Same body also sees 0–5 / 20–21? | **Yes**, EndPoll encoder `0042E899` `call [edx]`. That is more **action ids**, not a second function. | **PROVEN** classify |
| Who applies 0–5 / 20–21 as a move? | No recovered listener. Status leftover. | **UNREAD** |
| First-seen no-save *player control*? | **No.** Frontend 2D (`0042DF9E`). `FirstSeenHandsPlayerControl=false`. No Hero. | **DISPROVEN** |
| First-seen no-save *use* of this body? | Press Start Type4 `0055CB10(26)` on type 11 `UI_FRONTEND_BUTTON_INVISIBLE` then type 32 `UI_MOUSE_POINTER`. | **PROVEN** objects |
| Dest AABB at Type4 apply? | **No.** Hover `0055BF10` / `0055B8F0` writes type-11/38 `+352` u8. Leftover #14 host `HitIndex` is analog of hover, not this body. | **DISPROVEN** apply; **LEFTOVER** host |
| WASD? | Slots 0–3 are `0x6F/0x70/0x72/0x6D`. `SilkNativeInput` does not queue W/S/D. | **DISPROVEN** |

**Answer:** `0055CB10` is input `vtbl+0` **accept-then-apply**.
First-seen no-save is **UI Type4 current-inner**, not player
walk. The leftover “player-move listener” name is the unread
**consumer** of encoder ids 0–5 / 20–21, not a second body.

---

## Verdict

**One function.** INT3 pad `0055CB0A`–`0055CB0F`, prologue
`0055CB10`, `ret 4` `0055CBD0`, INT3 `0055CBD3`. Indirect
only: `e8.tsv` has **zero** `E8 0055CB10`. Callers are
`call [edx]` after `0041E5F2` (`0042E5AF` / `0042E975`).

Type4 is one producer into that slot (`push 26`). EndPoll
is another producer (`push 0`…`5` / `20`/`21` / …). The
body does not switch on the id. Listeners do.

Starting **UNKNOWN** locomotion is closed as **not walk**.
Leftover #14 dest AABB is **not this apply**.

---

## 1. Dump body (`listing-00540000.txt`)

```
0055CB10  push ecx
0055CB11  push esi
0055CB12  mov  esi, ecx              ; this = input
0055CB14  mov  eax, [esi+8]          ; focused inner*
0055CB17  test eax, eax
0055CB19  push edi
0055CB1A  je   0055CB3F              ; no focus → broadcast
0055CB1C  mov  edi, [esp+16]         ; action
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
0055CB39  pop  edi / esi / ecx
0055CB3C  ret  4
0055CB3F  mov  ecx, [esi+12]
0055CB42  mov  edx, [ecx]            ; head->next
0055CB44  lea  eax, [esi+12]
0055CB47  cmp  edx, ecx
0055CB49  push ebx
0055CB4A  lea  ecx, [esp+12]
0055CB4E  je   0055CB90              ; +12 empty → +4
0055CB50  push eax
0055CB51  call 0055CF50              ; 12-byte circular copy
          ; ebx = copy head
          ; for esi = [ebx]; esi != ebx; esi = [esi]:
          ;   edi = action [esp+20]
          ;   ecx = [esi+8] listener
          ;   if listener.vtbl+8(edi): listener.vtbl+4(edi)
0055CB80  lea  ecx, [esp+12]
0055CB84  call 0042AC25              ; free copy
0055CB8D  ret  4
0055CB90  add  esi, 4                ; list +4
0055CB93  push esi
0055CB94  call 0055CF50
          ; same accept / apply walk
0055CBC3  lea  ecx, [esp+12]
0055CBC7  call 0042AC25
0055CBD0  ret  4
```

Focused path returns even when accept is false (`je 0055CBCD`).
Broadcast prefers nonempty `+12`, else `+4`.

`0055CF50`: `push 12` / `00BFEA0E` alloc, circular
`[eax]=[eax+4]=eax`, `0055CE90` copy from the live list.
`0042AC25`: `0042A1E3` then `00BFEA14` free `[this]`.

| Claim | Class |
| --- | --- |
| Entire fn is `0055CB10`–`0055CBD0` | **PROVEN** (INT3 both sides) |
| Arg0 is the pushed action dword | **PROVEN** `[esp+16]` / `[esp+20]` |
| Focused `[this+8]` is exclusive | **PROVEN** |
| Broadcast `+12` else `+4` | **PROVEN** |
| Accept `listener.vtbl+8`; apply `vtbl+4` | **PROVEN** |
| Body loads dest AABB / pointer XY / Thing pose | **DISPROVEN** |
| Body `E8` `0055B8F0` / `0055BF10` / `006A9960` / `00723FD0` | **DISPROVEN** (only `0055CF50` / `0042AC25`) |
| Direct `.text` `E8 0055CB10` | **DISPROVEN** (`e8.tsv` empty) |

Sibling `0055CBE0` (`ret 8`) is `vtbl+1`: two-arg match-one
listener. **DISPROVEN** as the `0042E3EE` site.

---

## 2. Not an overloaded name — two vtbls, one pointer

`vtbl.tsv`:

| Vtbl | Slot 0 | Ctor write |
| --- | ---: | --- |
| `01230044` | `0x0055CB10` | `0042BE7B` `mov [esi], 0x1230044` |
| `01230134` | `0x0055CB10` | `0041E3F6` `call 0042BE7B` then `mov [esi], 0x1230134` |

Slots **0–10** of `01230134` copy `01230044`. Slot 11+
(`0042D4F0` last-key, `0041E6D3` vtbl+56, …) are derived
only.

Getter `0041E5F2` returns `[0x13B8710]` (alloc `0xD0`,
ctor `0041E3F6`). First-seen `this` is that singleton.
Base ctor zeros `[esi+8]` and builds empty circular
nodes at `+4` / `+12` (`0042AC0A`).

That is C++ prefix sharing, not two apply fns. Do not
read “locomotion UNKNOWN” and “Type4 apply” as two
names for two bodies.

---

## 3. Type4 current-inner, not dest AABB (leftover #14)

`listing-00400000.txt`. Type from `00A03B40` (`[record+40]`).
`dec; sub 3; je` is type 4:

```
0042E4A4  call 0041E5F2
          push 26
          jmp  0042E5AB
0042E5AB  mov  edx, [eax]
          mov  ecx, eax
          call [edx]                 ; vtbl+0 = 0055CB10
```

No dest origin/size. No widget walk in the poll. Action
is the immediate `push 26`.

First-seen `[input+8]==0` → broadcast of whoever
`input.vtbl+8(inner)` registered. Type 11/38 **PROVEN**
(`0055BA20`). Type 32 **PROVEN** (`0055C650`, second
Press Start node). Type-10 ctor does **not** register
(**UNREAD** as a first-seen node). Type-4 **widget**
ctor `005334A0` does **not** register — Type4 **event**
is not type-4 **widget**.

Inner apply (no AABB):

| Listener | Inner apply | Type4 (26) |
| --- | --- | --- |
| type 11 | `0054DBC0` | parent `+545` then `0055AD60` |
| type 38 / armed 11 | `0055AD60` | `[inner+348]` (`widget+352` u8); 0 skips `0055AF60` |
| type 32 | `0055C6F0` | action 25 only |
| type 10 if on list | `0054E2FA` | posts `&widget+352` packet* |

Dest AABB is hover `0055ACB0` → `0055B890` → `0055BF10`
→ `vtbl+568` `0055B8F0`. That tick **writes** `+352`.
Type4 **reads** the byte. Leftover-14 dest AABB analog
is host `ArmType34Widgets` `HitIndex` + Present skip
on stored dests — **not** this body.

`FrontendInputMap.NativeType4UsesCurrentInner = true`.
`NativeType4UsesDestAabb = false`. **MATCH** dump.

---

## 4. Encoder ids share the body — they are not walk

After harvest, `0042E899` priority-encodes `[ebp-4]` /
`edi+252` and `call [edx]` (`0042E971`; in-line 2+20 /
3+21):

| Mask | Push |
| --- | ---: |
| `bh & 0x04` or `0x01` | 4 |
| `bh & 0x08` or `0x02` | 5 |
| `0x20000` | 22 |
| `bl & 0x44` | **2 then 20** |
| `bl & 0x88` | **3 then 21** |
| `bl & 0x11` | 0 |
| `bl & 0x22` | 1 |

Type 17 stick (`0042E608`) ORs NESW bits and **never**
takes `0042E5AB`. `TypeAnalogPostsActionApply=false`.

Keys that set those bits (`0042E4C8`… vs `0041DF10(0)`):

| Slot | Key | Not |
| ---: | ---: | --- |
| 0–3 | `0x6F/0x70/0x72/0x6D` | not `DIK_W/A/S/D` |
| 6 | `0x1E` `DIK_A` | action **4**, not walk |
| 11 | `0x11` `DIK_W` | `KeyBit` 0 |

Same `call [edx]`. Different immediate. The body still
only walks inners. Status leftover “player-move listener”
is **who accepts 0–5 / 20–21 as XYZ** — **UNREAD**. Do
not fill that gap by renaming this fn locomotion.

After Leave the game pump is `00416E78` → `00446A30`,
**not** `0042E3EE`. **DISPROVEN** as first game apply.

---

## 5. First-seen no-save

```
0042EC7C retail frontend
  0042E3EE
    type 13 → 0055CB10(25)     pointer; not dest AABB
    type  4 → 0055CB10(26)     current-inner Type4
    type  6 → 0055CB10(28)     armed release
  0042DF9E  2D Present         // no Things
  // FirstSeenHandsPlayerControl=false

0042F2A2 Leave → FinalAlbion.wld
0042F491 Init Game
  CTCPhysicsStandard 0x88 pose     // sibling; this fn never touches it
004189C2 first pumps
  WorldFrame<=1 skip 00446A30
  no WalkTo / 006A9960
```

Press Start `0055CB10` nodes: type 11 INVISIBLE, type 32
mouse. No recovered node whose `vtbl+4` writes Hero XYZ.
**PROVEN** absence on frontend. Player smash / Oakvale
`Hero.WalkTo` / F2 WASD are **not** this spine.

---

## 6. Host

`EngineInput.ActionApply = 0x0055CB10`.
`ActionApplyIsLocomotion = false`. `Dispatch` is
`_actions.Add(action)` — **PARTIAL** vs native walk
(no listener apply).

`PumpFrontendFrame` → `PumpInput` then
`MaybeActivateNewGameFromInput`:

- `TickType11Type38Hover` dest AABB from pointer
  **before** Type4 — **MATCH** player LMB hover order.
- Type4 → `ArmType34Widgets` `HitIndex` dest AABB —
  **LEFTOVER** vs current-inner `0055CB10`.
- `MessageFromWidgets` first visible type-10 —
  **LEFTOVER** vs inner node.

`SilkNativeInput.QueuePointer`: set dest, optional type
13, Type4 on LMB edge, Type6 on LMB up. **MATCH**
classify. `QueueKeys` posts Escape / Space / Enter / F4
/ A / B as type 1. **No W/S/D.** Host F2 `FlyCamera`
WASD never `QueueInput`.

Do **not** wire `Key.W/A/S/D` into `0055CB10`. Do not
invent a physics tick so encoder ids “have somewhere
to go.”

---

## 7. Gap

```
Evidence                 Original                      Host                         Gap
0055CB10 body            accept then apply             Dispatch records             LEFTOVER missing walk
two vtbls slot 0         01230044 prefix / 01230134    ActionApply = 0055CB10       MATCH one pointer
Type4 classify           push 26 / call [edx]          Silk Type4 LMB edge          MATCH
Type4 apply              current inner vtbl+4          HitIndex dest AABB           LEFTOVER #14
Hover dest AABB          tick 0055BF10 writes +352     TickType11Type38Hover        MATCH hover
0–5 / 20–21              same call [edx]               EndPoll Dispatch             PARTIAL record
player-move listener     unread                        none                         UNREAD leftover
WASD / physics           not this body                 not queued as Type4          DISPROVEN invent
first-seen no-save       frontend Type4 UI             FirstSeenHands=false         MATCH
```

| Slice | Native | Host | Class |
| --- | --- | --- | --- |
| Body identity | listener walk `ret 4` | `ActionApplyIsLocomotion=false` | **MATCH** |
| Type4 apply | current inner | dest AABB `HitIndex` | **LEFTOVER** |
| Overload / walk name | one VA | one const | **DISPROVEN** two fns |
| First-seen hands | false | `FirstSeenHandsPlayerControl=false` | **MATCH** |
| Player-move consumer | unread | record only | **UNREAD** |

---

## 8. Do not invent

- Player walk / WASD / `DIK_W` as this apply.
- Dest AABB as Type4 apply (`0055B8F0` is hover).
- Two functions because two leftover labels
  (locomotion UNKNOWN vs Type4 current-inner).
- Type-4 **widget** inner as a first-seen node.
- Physics solver so encoder 0–5 “complete” this fn.
- First-seen no-save Hero control at this VA.

**Proposed (do not apply here):** keep dest AABB on
`TickType11Type38Hover`. Point Type4/Type6 at this
current-inner walk. Leave 0–5 / 20–21 consumer
**UNREAD**. Do not invent WASD.

---

## Classifications (short)

1. **`0055CB10` identity — PROVEN.** Input `vtbl+0`
   accept/apply. Listing read. Starting **UNKNOWN**
   locomotion is **not walk**.
2. **UI Type4 apply — PROVEN.** `push 26` current-inner.
   Dest AABB leftover #14 is hover, not this body.
3. **Overloaded name / both-as-two-fns — DISPROVEN.**
   One pointer on base `01230044` and derived
   `01230134`. Encoder ids share the same `call [edx]`.
4. **First-seen no-save player control — DISPROVEN.**
   Frontend Type4 UI. `FirstSeenHandsPlayerControl=false`.
5. **Player-move listener for 0–5 / 20–21 — UNREAD.**
   Status leftover. Not a licence to invent WASD.
6. **Physics — DISPROVEN** as this body. Sibling pose
   `0x88` is not a callee.

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00540000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\assembly\exe\00-index\vtbl.tsv`
- `C:\FableCSharp\proofs\0055CB10-locomotion\README.md`
- `C:\FableCSharp\proofs\type4-current-inner-apply\README.md`
- `C:\FableCSharp\proofs\leftover-14-dest-aabb\README.md`
- `C:\FableCSharp\proofs\action26-subscribers\README.md`
- `C:\FableCSharp\src\Fable.Game\EngineInput.cs`
- `C:\FableCSharp\src\Fable.Game\FrontendInputMap.cs`
- `C:\FableCSharp\src\Fable.Client\SilkNativeInput.cs`
