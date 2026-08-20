# First-seen in-game input does not move Lookout hero

Investigation only. No production `src/` or `tests/` edits.

Question: after first **world** Present (LookoutPoint, adult
mesh **4299**, no-save), what native input actually moves the
hero? What first-seen in-game action would **translate**
`CTCPhysicsStandard` pose?

`0055CB10` is UI Type4 current-inner apply (**PROVEN**, not
walk). `FirstSeenHandsPlayerControl=false`.

Do **not** invent WASD. Do **not** invent a physics solver
from `CTCPhysicsStandard`. Do **not** treat HSP copy or
recorded actions 0–5 / 20–21 as a Hero step.

Authority: dump `Fable.exe`
`listing-00400000.txt` (`0041649C` / `00415FF2` / `0042E3EE`),
`listing-00440000.txt` (`00446330` / `00446462` / `00446A30`),
`listing-004c0000.txt` (`004C72B0`),
`listing-00540000.txt` (`0055CB10`),
`listing-00680000.txt` (`00687DB0` / `00687FD0` / `006A9960`),
`listing-00700000.txt` (`00723FD0` / `00724290`);
`src/Fable.Game/EngineInput.cs`;
`src/Fable.Game/PlayerInterface.cs`;
`src/Fable.Game/RegionTravel.cs`
(`FirstSeenHandsPlayerControl`);
`src/Fable.Client/SilkNativeInput.cs`;
`proofs/0055CB10-body/README.md`;
`proofs/input-type10-mmb/README.md`;
`proofs/creature-move-first/README.md`;
`proofs/collision-first-seen/README.md`;
`proofs/audit-playerinterface/README.md`;
`proofs/type4-type6-ring/README.md`.

Do not re-prove type 4 → `push 26`, type 10 → action 27 is
RMB not MMB, or `CTCPhysicsStandard` factory size `0x88`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| Native input that moves hero on first world Present? | **None.** Pose is HSP placement + TNG persist. No input writes physics `+80`. | **PROVEN** omit |
| `0055CB10` after Leave? | Frontend-only `0042E3EE` (`0042F0AC`). Game pump is `00416E78` → `00446A30`. | **DISPROVEN** as in-game walk |
| First `00446A30` on that Present? | `009F4ED0` miss, `00446220` `[+168]=0`, `al=0`, no `0041649C`. | **PROVEN** empty |
| Listener apply `00687FD0` writes pose? | **No.** Expression slot 53 / HUD `0x10E` / type≠1 gate. | **DISPROVEN** |
| `0041649C` writes pose? | **No.** `0049D8C0` / `00415FF2` (action==2) / `004AE9A0` / `0049E1D0` / `00434A30`. | **DISPROVEN** |
| Type 10 RMB as world Use / walk? | Action 27 hover-in. `ActionType10IsWorldUse=false`. | **DISPROVEN** |
| WASD? | Slots `0x6F/0x70/0x72/0x6D`. Host F2 `FlyCamera` never `QueueInput`. | **DISPROVEN** |
| `CTCPhysicsStandard` live translate from input? | Persist `00724290` is TNG RH load onto `+80/+92`. Ctor `00723FD0` zeros. No input callee. | **DISPROVEN** as input; persist **PROVEN** |
| Action that *would* dest-store later? | Player `vtbl+16` `006A9960` → `00662930` + gait + `or [+146],2`. Unused first-seen. Does **not** write mesh XYZ. | **PROVEN** body; first-seen **DISPROVEN** |
| WalkTo apply? | `004C72B0` `mov al,1; ret 4`. `FirstSeenWalkToAppliesMove=false`. | **PROVEN** stub |
| Physics solver / capsule? | None recovered. First-seen solver **DISPROVEN**. | **UNREAD** solver |
| `FirstSeenHandsPlayerControl` | `false` | **MATCH** |

**Answer:** no-save first world Present **omits** hero-move
input. Leftover is a later unread consumer / dest apply /
solver — not a licence to invent WASD.

---

## Verdict

**PROVEN omit.** Lookout adult `CREATURE_HERO` mesh **4299**
sits at `GuildArrivalHSP` `(52.688, 69.597, 36.982)`. That
is **placement**. The first `009BEEB0` with that mesh does
not run an input path that translates `CTCPhysicsStandard`
pose.

Two machines stay distinct after Leave:

```
frontend  0042EC7C  0042E3EE → 0055CB10     // UI Type4; not this Present
game      00416E78  00446A30 → 00446330     // listeners; empty first-seen
```

`0055CB10` never becomes walk because the game pump never
calls it. Encoder ids 0–5 / 20–21 are a **frontend**
`0042E899` classify. Game harvest `00446462` is
`009F4ED0` inside `00446330`: raw records to
`vtbl+32` accept / `vtbl+16` apply. No `push 0`…`5`.

---

## 1. First world Present is placement, not a step

No-save spine (`creature-move-first` / `collision-first-seen`):

```
0042F2A2 Leave
0042F491 Init Game
  004EE790  CTCPhysicsStandard  00BFEA1A(0x88) → 00723FD0
  004EE80C  CTCPhysicsControlled
004189C2 dummy pumps
  WorldFrame<=1 skip 00446A30 / 00435530
  no 00501450, no 006AC910
later
  00501450 LookoutPoint
    0051FD80 TNG persist 00724290 (props)
    006AC910 CREATURE_HERO @ GuildArrivalHSP mesh 4299
      004C9D60("CTCPhysicsControlled")
  00416E78 WorldFrame>1
    00446A30 empty  (al=0, no 0041649C)
  00417001 → later 00435F70 / 009BEEB0   // first world Present
```

| Object | First-seen | Move? |
| --- | --- | --- |
| Hero XYZ | HSP copy | **placement PROVEN**; step **DISPROVEN** |
| `CTCPhysicsControlled` | named add `006A9EAB` | **PROVEN** name; **DISPROVEN** as solver |
| `CTCCreatureNavigation` on create | not in `006AC910` | **DISPROVEN** |
| `006A9960` dest | unused | **DISPROVEN** first-seen |
| `00CBFB7D` WalkTo | Oakvale leftover | **LEFTOVER**; **DISPROVEN** as Leave |

`RegionTravel.FirstSeenHandsPlayerControl = false`.
**MATCH.**

---

## 2. Dump — `0055CB10` is not the game apply

`0055CB10-body`: one `ret 4` accept/apply. Type 4 →
`push 26`. Only PE caller of `0042E3EE` is retail
`0042F0AC`.

After Leave, `00416E78` calls `[game+32].vtbl+4` =
`00446A30`. Zero `E8` of `00446A30`; zero `E8` of
`0055CB10`. Game does **not** `call [edx]` on
`01230134+0`.

Type 10 on the frontend machine is RMB → action **27**
(`input-type10-mmb`). `EngineInput.ActionType10IsWorldUse
= false`. Not barrels, not walk.

Do not reopen Type4 dest AABB (leftover #14). That is
hover, not this Present.

---

## 3. Dump — first in-game pump is empty `00446A30`

`listing-00440000.txt`:

```
00446A99  push esi                  ; event*
00446A9C  call 00446330             ; harvest
00446AA1  test al, al
00446AA3  jne  00446AB1             ; hit → copy 10 dwords
00446AA8  call 00446220             ; fallback
00446AAD  test al, al
00446AAF  je   00446B08             ; al=0 → return
```

`00446330` `00446462 call 009F4ED0`. Skip device 2 /
key 15 / type 0. Then listener `vtbl+32` (`00687DB0`)
/ `00449990` / `vtbl+16` (`00687FD0`). Result 1
selects.

First WorldFrame>1 (`After_WorldFrame_gt_1_*`): mux
empty → miss; fallback `[+168]=0` → **no**
`0041649C`. Host notes `"00446A30 al=0 no 0041649C"`.
**MATCH.**

`00446462` is that harvest site, **not** a second WASD
poll. Status leftover “game poll `00446462` /
`004963E6`” as *move classify* stays **UNREAD** for
`004963A0` (other `009F4ED0`; jump table on
`[esi+28]`; **not** `00416E78`). Do not invent it as
first-Present walk.

---

## 4. Dump — hit path still does not translate pose

### 4a. Accept `00687DB0`

Device==1 → lookup `[esi+24]` vs field36. Else
`al=1`. No XYZ.

### 4b. Apply `00687FD0`

```
00687FF2  00687CF0(edi, 53)         ; expression
          00687A90
          [game+36]+260 in {0,9}
          [+248]==0
          HUD [eax+64] bit 0x40
          map key 0x10E
          type = 00A03B40(edi)
          cmp eax, 1 / je leave
```

HUD / expression. **DISPROVEN** as `00724290` /
`006A9960`.

### 4c. After hit: `0041649C`

```
0041649C  0049D8C0(world, event)    ; occupied slot
          else 00415FF2             ; [event]==2
          then 004AE9A0             ; player+80568 → 009F1650
          always 0049E1D0 / 00434A30
```

`00415FF2` is `dec; dec; je al=1` (action==2 only).
Default owner ResultSelect is 0, so first KeyMove3
`DeliveredCount=0` until a recovered item. Even a
later hit is **UNREAD** as locomotion
(`audit-playerinterface`). **DISPROVEN** as pose
write on first Present.

Owner default 0 is why “press movement key” does not
hand control. That is the same fact as
`FirstSeenHandsPlayerControl=false`.

---

## 5. Dump — `CTCPhysicsStandard` pose is load, not input

Ctor `00723FD0`: `006B0730`, zero `+64/+128`, flags
`+132/+133=4`, vtbl `01265F84`. No input arg.

Persist `00724290`: `00410620` `"RHSetForwardX"` …
`"RHSetUpZ"` then `mov [edi],…` onto `+80/+92`. TNG
load. Lookout **props** **PROVEN**. Hero create names
**`CTCPhysicsControlled`**, not a second Standard
write from a key.

`006A9960` (player `vtbl+16`):

```
006A9968  call 00662930             ; dest store
          fld [this+224]+80
          fst [this+176]            ; gait
          or  [this+146], 2
```

Success does **not** `fstp` mesh XYZ or Standard
`+80`. First-seen **no** `E8` from `00446A30` /
`0041649C` / `00687FD0`. WalkTo apply `004C72B0` is
`mov al,1; ret 4`.

Solver / capsule / contact: **UNREAD**. First-seen
solver **DISPROVEN** (`collision-first-seen`). Do not
invent one so input “has somewhere to go.”

---

## 6. Keys are not WASD

`EngineInput.KeyboardDefaults` slots 0–3:
`0x6F/0x70/0x72/0x6D`. `DIK_W=0x11` is slot 11,
`KeyBit` 0. `DIK_A` is action **4**. `DIK_S` / `DIK_D`
absent.

Those compares live in **frontend** `0042E3EE`. Game
`00446330` has **no** `cmp eax, 111` encoder. Stick
type 17 never `0055CB10` (`TypeAnalogPostsActionApply
= false`).

`SilkNativeInput.QueueKeys`: Escape / Space / Enter /
F4 / A / B. **No W/S/D.** Host F2 `FlyCamera` WASD
never `QueueInput` (`Program.cs`). **LEFTOVER** debug.

---

## 7. Host

| Host | Native first Present | Class |
| --- | --- | --- |
| `ActionApplyIsLocomotion=false` | `0055CB10` UI | **MATCH** |
| `FirstSeenHandsPlayerControl=false` | empty `00446A30` | **MATCH** |
| `Dispatch` records 0–5 | frontend encoder only | **PARTIAL** (not game pump) |
| `Player.Pump` empty → no `0041649C` | `al=0` | **MATCH** |
| `ApplyInputEvent` world-tick / action 2 | `0041649C` | **MATCH** classify; **DISPROVEN** pose |
| `TickMove` lerp | `004C72B0` stub | **LEFTOVER** |
| F2 WASD `FlyCamera` | not `00416E78` | **LEFTOVER** |
| `SeedStart` HSP | `006AC910` | **MATCH** placement |

---

## 8. Gap

```
Evidence              Original                         Host                    Gap
0055CB10              frontend Type4 inner             records; not walk       MATCH not locomotion
00446A30 first        empty, no 0041649C               same note               MATCH omit
00687FD0 / 0041649C   HUD / queue; no +80              ApplyInputEvent         MATCH not pose
00724290              TNG RH persist                   ObjectTransform         MATCH load
006A9960              dest+gait unused                 none on Present         PROVEN omit
004C72B0              stub                             TickMove lerp           LEFTOVER
solver                unread                           none                    DISPROVEN invent
WASD                  not slots 0–3                    F2 FlyCamera            DISPROVEN invent
hands                 false                            false                   MATCH
```

**Leftover (do not fill here):** who, *after* first
Present, would accept a live key and write Standard
`+80` or mesh XYZ. Keep **UNREAD**. Host `TickMove` /
F2 WASD stay **LEFTOVER**.

---

## 9. Do not invent

- WASD / `Key.W` as Lookout walk.
- `0055CB10` as in-game locomotion (Type4 UI).
- Type 10 / action 27 as `CTCActionUse*` / walk.
- Empty first `00446A30` as “hands given.”
- `00724290` as a per-frame solver.
- `006A9960` dest as a mesh step on this Present.
- Unity capsule so encoder ids “complete.”

**Proposed (do not apply here):** leave
`FirstSeenHandsPlayerControl=false`. Do not wire
WASD into `PlayerInterface` or `EngineInput.Dispatch`.
Do not start no-save control at `0055CB10`, Lookout
rocks, or F2 `FlyCamera`.

---

## Classifications (short)

1. **First-seen in-game hero-move input — PROVEN omit.**
   Lookout Present shows mesh 4299 at HSP. No input
   translates `CTCPhysicsStandard` pose.
2. **`0055CB10` — DISPROVEN as that input.** UI Type4.
3. **First `00446A30` — PROVEN empty.** No `0041649C`.
4. **Hit path `00687FD0` / `0041649C` — DISPROVEN as
   pose write.**
5. **WASD / physics solver — DISPROVEN invent.**
   Solver body **UNREAD**; first-seen solver
   **DISPROVEN**.
6. **Later dest `006A9960` / WalkTo — PROVEN bodies,
   unused first-seen.** Leftover Oakvale / host
   `TickMove`.

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00440000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-004c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00540000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00680000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00700000.txt`
- `C:\FableCSharp\proofs\0055CB10-body\README.md`
- `C:\FableCSharp\proofs\input-type10-mmb\README.md`
- `C:\FableCSharp\proofs\creature-move-first\README.md`
- `C:\FableCSharp\proofs\collision-first-seen\README.md`
- `C:\FableCSharp\proofs\audit-playerinterface\README.md`
- `C:\FableCSharp\src\Fable.Game\EngineInput.cs`
- `C:\FableCSharp\src\Fable.Game\PlayerInterface.cs`
- `C:\FableCSharp\src\Fable.Client\SilkNativeInput.cs`
