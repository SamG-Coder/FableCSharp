# First-seen control after first Present — frontend vs in-game

Investigation only. No production `src/` edits.
Do **not** invent WASD. Do **not** re-enable `Key.N` /
`ActivateNewGame`.

Question: after the first Present, what native input is
first-seen control — frontend vs in-game? What is type-10
(MMB vs RMB vs widget)? Where does persist suffix msg 15
live versus the type-10 attach packet?

Authority: `Fable.exe` listings
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
(`0042E3EE` / `0041E6D3` / `0042DF9E` / `00416E78` /
`004184BD`),
`listing-00540000.txt` (`0055CB10` / `0054E280` / `0054E4F0` /
`0054E2FA` / `0055AF60` / `0055ACF0` / `0055AD60`),
`listing-00580000.txt` (`0059A238` / `00598EE6` / `00595582`),
`listing-00600000.txt` (`00631C60` / `00632500` persist
suffix),
`listing-00a00000.txt` (`00A03C80` / `00A03D60` / `00A03D90` /
`00A03E40` / `00A04090`),
`listing-00a80000.txt` (`00AB5420` / `00AB4910` / `00AB6E40`);
`frontend.bin` CRC `0x53C644E4`;
`src/Fable.Game/EngineInput.cs`,
`FrontendInputMap.cs`,
`EngineLifecycle.cs` (`WriteType10AttachMessage` /
`MaybeActivateNewGameFromInput`),
`IEngineHost.cs` (`Type10Packet` vs `MessageId`);
`src/Fable.Client/Program.cs`,
`SilkNativeInput.cs`;
`RegionTravel.FirstSeenHandsPlayerControl`;
siblings
`proofs/input-type10-mmb/README.md`,
`proofs/input-vtbl56-vs-ui32/README.md`,
`proofs/pad-a-vs-type4/README.md`,
`proofs/0041E6D3-frontend-gate/README.md`,
`proofs/action28-after-26/README.md`,
`proofs/messageid-plus228/README.md`,
`proofs/0054E4F0-store-shape/README.md`,
`proofs/leftover-14-native-key/README.md`,
`proofs/0055CB10-locomotion/README.md`,
`proofs/audit-playerinterface/README.md`,
`proofs/creature-move-first/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **STALE** / **LEFTOVER** / **MATCH**.

Do not re-prove Leave `0042F2A2` / `FinalAlbion.wld`, dest
AABB (#48), or `Q_NewOakValeIntro` activator.

---

## Verdict

**Two first Presents. Two machines. Neither is WASD.**

| Present | Native | First-seen control |
| --- | --- | --- |
| Frontend `0042DF9E` (Press Start on screen) | `0042EC7C` already polled `0042E3EE` this frame | **LMB** type **4** down / type **6** up |
| In-game empty Present after Leave | `004189C2` `WorldFrame<=1` skips `00446A30` | **none** — `FirstSeenHandsPlayerControl=false` |

Frontend posts stored UI ids, not a DIK and not MMB:

| Screen | Event | Action | Posted id | Slot |
| --- | --- | ---: | ---: | --- |
| Press Start | type **4** LMB down | **26** | **`0xE5`** | type-10 **packet*** at widget+352 |
| New Profile | type 4 arm + type **6** LMB up | **28** | **`0x126`** | persist suffix **`+228`** |
| Main Menu | type 4 arm + type 6 | 28 | **15** | same persist **`+228`** |

Input type 10 is **RMB down** (`00A03E40` → action **27**), not
MMB. MMB is type **7** (`00A03D90` → action **35**). Widget type
10 is the Menu container. Those three “10”s are not one object.

`0059A238` (UI `012521A8+32`) **consumes** `0xE5` / `0x126` / 15.
`0041E6D3` (input `01230134+56`) is a **second fan-in**. Type-4
Press Start never enters it. `docs/PARITY.md` / `FORWARD_TREE.md`
“native key UNREAD (`0041E6D3` is the consumer)” is **STALE** on
the consumer and on the **mouse**. It remains **MATCH** as unread
**key**. Host Return→msg 15 from Press Start is **DISPROVEN**.

Type-10 packet vs persist MessageId:

| Word | Native store | First-seen value | Host |
| --- | --- | ---: | --- |
| Type-10 `+352` | `0054E4F0` **packet***; `[packet+0]` from `00598EE6` | **`0xE5`** | `FrontendWidget.Type10Packet` **MATCH** id; pointer shape **LEFTOVER** |
| Persist `+228` | `00631C60` `00632500` CRC `0x53C644E4` | PRESS_START root **0**; Accept **`0x126`**; New Game **15** | `FrontendWidget.MessageId` **MATCH** file i32 |

Do **not** write persist 15 onto the type-10 packet. Do **not**
hand first-seen in-game control to WASD / F2 `FlyCamera` /
`Key.N`.

| Claim | Class |
| --- | --- |
| First frontend Present is `0042DF9E` 2D | **PROVEN** |
| Same-frame poll is `0042E3EE` before that Present | **PROVEN** |
| Type 4 is LMB down, device 3, action 26 | **PROVEN** |
| Type 6 is LMB up, action 28 | **PROVEN** |
| Input type 10 is RMB, action 27 | **PROVEN** (`input-type10-mmb`) |
| Input type 10 is MMB | **DISPROVEN** — MMB is type 7 / action 35 |
| Widget type 10 is the input record | **DISPROVEN** |
| Pad A is type 4 | **DISPROVEN** — type 19 / action 22 (`pad-a-vs-type4`) |
| Return / Enter / `Key.N` posts `0xE5` / `0x126` / 15 | **DISPROVEN** |
| Physical **key** that posts those ids | **UNREAD** |
| `0059A238` consumes the three ids | **PROVEN** |
| `0041E6D3` is the Press Start `0xE5` consumer | **DISPROVEN** (`input-vtbl56-vs-ui32`) |
| First-seen `[0x13B86A0]==0` so a live `0041E6D3` hop would still hit `0059A238` | **PROVEN** (`0041E6D3-frontend-gate`) |
| Type-10 `+352` is packet*, not dword `0xE5` | **PROVEN** (`0054E4F0-store-shape`) |
| Persist suffix `+228` / `0x53C644E4` is New Game **15** | **PROVEN** file (`messageid-plus228`) |
| PRESS_START type-10 persist `MessageId` is `0xE5` | **DISPROVEN** — persist 0; attach packet holds `0xE5` |
| Action 26 on type 11/38 posts persist 15 first-seen | **DISPROVEN** — arms; `+224` list empty; 28 posts `+228` if armed |
| `0055AF60` `push 28` applies case 28 / `0055ACF0` | **DISPROVEN** — local-map insert (`action28-after-26`) |
| First in-game Present hands the player WASD | **DISPROVEN** |
| `0055CB10` is locomotion | **DISPROVEN** |
| Movement bind slots 0–3 are `DIK_W/A/S/D` | **DISPROVEN** (`0x6F/0x70/0x72/0x6D`) |
| Host F2 WASD is native | **DISPROVEN** — **LEFTOVER** debug |
| Host `Type10Packet` vs persist `MessageId` | **MATCH** first-seen ids |

**Answer:** after first frontend Present, recovered control is
**LMB**. After first in-game Present, recovered control is
**none**. Persist suffix msg **15** is CUIDef `+228`, not the
type-10 packet.

---

## 1. Two first Presents

```
0042EC7C  retail frontend
  0042E3EE  poll                    ; 0042F0AC only caller
  0042DC94 / 00599E3F
  0042DF9E  2D Present              ; FIRST frontend Present
  ; FirstSeenHandsPlayerControl=false  (no Hero, no 00446A30)

0042F2A2  Leave  (msg 15 → [retail+41]=1)
  FinalAlbion.wld
0042F491  Init Game
  004184BD  [0x13B86A0]=game        ; FIRST non-zero; after Leave
004189C2  first game pumps
  WorldFrame<=1: skip 004457F0 / 00446A30
  CurrentRegion=null
  00435F70 / 009DA9F0  empty dest   ; FIRST in-game Present
```

Frontend Present is 2D UI (`0042DF9E`). In-game first Present is
empty dest / skip DIP. Neither Present is a walk step. `00501450`
Lookout and `006AC910` hero are **not** this first Present
(`creature-move-first`).

Host `RegionTravel.FirstSeenHandsPlayerControl = false` **MATCH**.

---

## 2. Frontend after first Present — LMB, not MMB, not pad A

`0042E3EE` (`listing-00400000.txt`):

```
0042E456  call 00A03B40            ; [record+40]
0042E47C  sub eax, 3
0042E47F  je  0042E4A4             ; type 4 → push 26
0042E483  je  0042E498             ; type 6 → push 28
0042E46A  cmp eax, 10
0042E473  je  0042E557             ; type 10 → push 27
0042E48C  … type 7 → push 35
0042E4A4  call 0041E5F2
0042E4A9  push 26
0042E4AB  jmp 0042E5AB             ; call [edx] = 0055CB10 vtbl+0
```

Type 1 (Return / any key) is `push 33` into the **same**
`call [edx]`. Never `call [edx+56]`. Never `push 26` from a
DIK.

### Mouse ctors (`00AB5420` / `00AB4910`)

| Button | `dwOfs` | down raw | ctor | `[+40]` | action |
| --- | ---: | ---: | --- | ---: | ---: |
| LMB | 12 `DIMOFS_BUTTON0` | 1 | `00A03C80` | **4** | **26** |
| **RMB** | 13 `BUTTON1` | 2 | **`00A03E40`** | **10** | **27** |
| **MMB** | 14 `BUTTON2` | 3 | `00A03D90` | **7** | **35** |
| LMB up | 12, data clear | 4 | `00A03D60` | **6** | **28** |

Win32 `009A4FE0` (`WM_RBUTTONDOWN`) is the same type-10 path.
`WM_MBUTTONDOWN` `009A4FD0` is type 7. Input type 10 is **not**
MMB (`input-type10-mmb`).

### Two different “type 10”s

| 10 | Object | VA | First-seen role |
| --- | --- | --- | --- |
| Input `[record+40]=10` | RMB down | `00A03E40` | action 27; **no** `0xE5`/`0x126`/15 |
| CUIDef persist type 10 | Menu | `0054E3D0` / apply `0054E280` | PRESS_START / NEW_PROFILE / MAIN_MENU roots |

Widget apply action **26** posts `&widget+352`. Action **27** is
`00597BF2(0)` — Press Start slot `0x14` returns immediately
(`input-type10-mmb` §5). First RMB does not leave Press Start.

### Pad A is type 19, not type 4

`00AB6E40` pad poll → `00A04090` `[+40]=0x13` device 1.
`rgbButtons[0]` → id 2 → `0042E3EE` action **22**, never 26
(`pad-a-vs-type4`). Do not map pad A / Start onto type 4.

---

## 3. Consumer is `0059A238`, not `0041E6D3`

`listing-00580000.txt`:

```
0059A281  mov eax, [ebp+8]         ; pair*
0059A284  mov eax, [eax]           ; boxed*
0059A286  mov ecx, [eax]           ; id
…
0059A2C5  je  0059A2DA             ; 15 → [retail+41]=1
…
0059A6BE  sub ecx, 0xE5
0059A6C4  je  0059A77F             ; 0xE5 → 00599D5C
…
0059A6E5  …                        ; 0x126 → 00851920
```

UI vtbl `012521A8+32` = `0059A238`. Host
`DispatchFrontendMessage` is this switch.

`0041E6D3` is input `vtbl+56` (`FrontendInputMap.InputVtblMessageFn`):

```
0041E6FB  mov esi, [0x13B86A0]
0041E703  jne 0041E718             ; game live: skip UI hop
0041E705  call 00595582
0041E70F  call [edx+32]            ; 0059A238
0041E718  … id switch …
```

First-seen frontend `[0x13B86A0]==0` until Init Game
(`004184D1` is the only non-zero writer). A **live** entry
therefore forwards to `0059A238` (`0041E6D3-frontend-gate`).
Type 4 does **not** enter this function
(`input-vtbl56-vs-ui32`): `0042E3EE` is `call [edx]` =
`0055CB10`.

Known `call [edx+56]` posters (`005403EF` Escape list,
`005405ED` Return list, `00558DFF` type-38 walk) are
**DISPROVEN** as first-seen Press Start `0xE5` (empty circular
lists; no type 38 on Press Start).

`docs/PARITY.md` “Leave Press Start” row and
`docs/runtime/FORWARD_TREE.md` “0041E6D3 consumer” are
**STALE** on the consumer. Native **key** stays **UNREAD**.

---

## 4. Type-10 packet vs persist suffix MessageId

### 4a. Attach packet (Press Start `0xE5`)

`00598A1C` (`listing-00580000.txt`):

```
00598EC5  call 00BFEA1A            ; 16-byte heap
00598ED1  call 0042BE50            ; packet ctor
00598EDE  call 0042AA29            ; wrapper {packet*, ctrl*}
00598EE6  mov [eax], 0xE5          ; packet[0] = 0xE5
          … slot 0x14 …
00598F06  call [eax+284]           ; type-10 0054E4F0
```

`0054E4F0` stores **ebx = wrapper[0] = packet*** at
widget+352, ctrl* at +356. **Not** `mov [widget+352], 0xE5`.

Action 26 (`0054E2FA`) posts `&widget+352`. `0059A238`
double-derefs to `[packet+0]`. A dword `0xE5` at +352 would
load `[0xE5]` — **DISPROVEN**.

PRESS_START persist `MessageId` is **0**
(`export/frontend/press-start-dests.txt` `msg=0`;
`EngineLifecycleTests` `slot.MessageId == 0`).
`0xE5` is attach-only.

Host `WriteType10AttachMessage`:

```
tree[0] = tree[0] with { Type10Packet = FrontendPressStartMessage };
```

`IEngineHost.FrontendWidget.Type10Packet` is the first dword
stand-in. Persist `MessageId` stays 0 on that root. First-seen
id **MATCH**. Packet* / ctrl* pair **LEFTOVER**.

`proofs/0054E4F0-store-shape` “writes `MessageId = 0xE5`” is
**STALE** vs current host (split field). Offset 352 and
`00598EE6` analog **MATCH**.

### 4b. Persist suffix msg 15 (`+228`)

CUIDef persist `00631C60` tail (`listing-00600000.txt`):

```
00631FBD  lea edx, [esi+224]
00631FC6  call 00632500            ; CRC 0x230364D6  Plus224
00631FCB  lea eax, [esi+228]
00631FD4  call 00632500            ; CRC 0x53C644E4  MessageId
00631FD9  lea ecx, [esi+232]
…
```

Helper `FrontendUiDef.PersistTailDwordFn = 0x00632500`.
`+224` as MessageId is **DISPROVEN** (`messageid-plus228`).
CRC name **UNREAD** (not `FableCrc("Message")` /
`"MessageId"`).

File i32 after `0x53C644E4`:

| Widget | Type | `+228` | Poster |
| --- | ---: | ---: | --- |
| `UI_FRONTEND_PRESS_START_MENU` | 10 | **0** | attach packet, not persist |
| `UI_FRONTEND_BUTTON_INVISIBLE` | 11 | **`0xE5`** | **not** the Press Start user post |
| `UI_ACCEPT_NEW_PROFILE` | 38 | **`0x126`** | type 4 arm + type 6 / `0055ACF0` `+380` |
| `UI_FRONTEND_BUTTON_NEW_GAME` | 11 | **15** | same `+228` hop |

`FrontendUiDefTests` locks Accept `MessageId=0x126`, New Game
`MessageId=15`, and `Plus224 != MessageId`.
`export/frontend/main-menu-dests.txt`
`UI_FRONTEND_BUTTON_NEW_GAME … msg=15`.

`0055B040` copies `[def+224]` through vtbl+284 (`+372`) then
`[def+228]` through vtbl+320 (`+380`). First-seen Accept /
New Game `+224` is 0, so action 26’s `+372` list is empty.

---

## 5. Action 28 after 26 — persist 15 is LMB **up**

```
0042E3EE
  type 4 → 0055CB10(26)
    type-10 0054E280 → 0054E2FA → 0059A238(0xE5)     ; Press Start
    type-11/38 0055AD60 → vtbl+584 0055AF60
      arm [+364]=1; post +372 / [def+224] (first-seen empty)
      push 28 / inner.vtbl+12                       ; subscribe, not apply
  type 6 → 0055CB10(28)
    type-10 case 3: no UI message
    type-11/38 armed → vtbl+588 / 0055ACF0
      post +380 / [def+228] → 0x126 or 15
```

`0055AF60` `push 28` / `call [edx+12]` is local-map **insert**
of action 28 (`0052DA20`). It is **not** `vtbl+16`, **not**
`0055AD60` case 28, **not** `0055ACF0` (`action28-after-26`).

Case 28 (`0055ADDE`) unarms or stamps. Persist 15 is **not**
that case. First-seen `[+364]=0` and `[widget+352]` selected
u8 is 0, so the first type-4 on type 11/38 may skip `0055AF60`
until hit-test `0055C0DE`. Host still arms via dest AABB hover
(`FrontendInputMap.NativeType4UsesDestAabb=false` leftover).

Host `MaybeActivateNewGameFromInput`: action 26
`MessageFromType10Attach` (`Type10Packet`); action 28
`MessageFromPlus228List` (`MessageId` if `Armed`). Enter is
type 1 / PlayAVI skip. **MATCH** first-seen ids. **DISPROVEN**
as Return→15.

---

## 6. Keyboard paths that do **not** post

| Event | Native | Result |
| --- | --- | --- |
| Return DIK 28 | type 1 / action 33 | last-key / `00597BF2`; **DISPROVEN** as `0xE5`/`0x126`/15 |
| Type-12 list `cmp edi, 1` / `28` | `005403D2` / `005405C9` → `0041E6D3` | Press Start circular lists empty |
| Host Enter | `SilkNativeInput.QueueKeys` type 1 | **MATCH** type 1; **DISPROVEN** as New Game |
| Host `Key.N` | **absent** in `Fable.Client` | do **not** restore |
| `Key.A` / `Key.B` | type 1 `KeyDikA`/`KeyDikB` | frontend actions 4/5, not walk |

`FrontendInputMap.DikPosterUnread = false` means type 4 is
**not** a DIK, not that a keyboard poster was found.
`NativeKeyPostsE5` / `NativeKeyNPostsNewGame` /
`NativeEnterPostsNewGame` are **false**.

---

## 7. In-game after first Present — not WASD

`0055CB10` is input `vtbl+0`: accept then apply one action on
a listener list. Zero `E8` of nav / physics / `006A9960`.
First-seen consumers are frontend type 11
`UI_FRONTEND_BUTTON_INVISIBLE` and type 32 `UI_MOUSE_POINTER`.
It is **not** locomotion (`0055CB10-locomotion`).

After Leave:

```
004184BD  Init Player Interface 004473A0   ; first construct
004189C2
  WorldFrame<=1: skip 004457F0 / 00446A30
  no 00501450, no 006AC910, no WalkTo
```

`0042E3EE` does not run on the game pump. `00446A30` is
`00416E78` after `WorldFrame>1` (`audit-playerinterface`).
No Hero Thing exists at the first in-game Present.

Keyboard defaults `0041DF10(0)` slots 0–3 are
`0x6F/0x70/0x72/0x6D`. `DIK_W` is not a move bit. Stick type
17 ORs NESW bits and does **not** `0055CB10`. Who applies
actions 0–5 / 20–21 as a mesh step stays **UNREAD**. That
unread is **not** a licence to invent WASD.

Host F2 `FlyCamera` WASD in `Program.cs` is **LEFTOVER**
debug (`Stage==Game` only). `SilkNativeInput` is skipped while
flying. That is not `CAM_OVIF_SHOT2` / `006B3FF0` / hero walk.

Host `Player.Construct` `Register(ActionInputListener)` on the
Init Player Interface arm is **LEFTOVER** vs `004473A0`
(`004473A0-player-iface`). Do not treat that register as
recovered first-seen control.

---

## 8. C# leftover (do not “fix” with N / WASD)

```
SilkNativeInput
  Enter → TypeKey (AVI skip)
  LMB edge → Type4
  LMB-up → Type6
  no Key.N, no ActivateNewGame, no RMB Type10 queue

WriteType10AttachMessage
  Type10Packet = 0xE5 on slot 0x14 root
  persist MessageId stays 0

MaybeActivateNewGameFromInput
  26 → Arm + Type10Packet
  28 → persist MessageId if Armed
  DispatchFrontendMessage            ; 0059A238 analog
```

| Site | Native | Host |
| --- | --- | --- |
| Type 4/6 classify | `0042E3EE` | `ActionFromEvent` **MATCH** |
| Type 10 classify | RMB / 27 | constants **MATCH**; live client never queues type 10 |
| `EngineInput` Type7 comment | MMB `00A03D90` | **LEFTOVER** “RMB down (`00A03D90`)” |
| Press Start id | packet `[0]=0xE5` | `Type10Packet` **MATCH** id |
| New Game 15 | persist `+228` | `MessageId` **MATCH** file |
| Return → 15 | no | **DISPROVEN** (quarantined) |
| In-game WASD | no first-seen listener | F2 **LEFTOVER**; `FirstSeenHandsPlayerControl=false` **MATCH** |

Clicks exist. Dest/hit for New Profile chrome is still invented
(#48). Present `0042DF9E` still Note-only. Leave #14 open on
those leftovers, not on a missing LMB translator and not for a
`Key.N` cheat.

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `0042DF9E` | first frontend Present | **PROVEN** 2D |
| `0042E3EE` | frontend poll | **PROVEN** |
| `00A03C80` / `00A03D60` | type 4 / 6 LMB | **PROVEN** |
| `00A03E40` | type 10 RMB | **PROVEN** |
| `00A03D90` | type 7 MMB | **PROVEN** |
| `00A04090` | type 19 pad | **PROVEN** |
| `0055CB10` | apply `vtbl+0` | **PROVEN** UI; **DISPROVEN** locomotion |
| `0054E4F0` | type-10 store packet* at +352 | **PROVEN** |
| `0054E2FA` | type-10 action 26 → `0059A238` | **PROVEN** `0xE5` |
| `00598EE6` | only `.text` `mov […], 0xE5` | **PROVEN** attach |
| `00632500` / `+228` | persist suffix MessageId | **PROVEN** 15 / `0x126` |
| `0055AF60` | 26 arm / insert 28 | **PROVEN** |
| `0055ACF0` | 28 post `+380` / `+228` | **PROVEN** site |
| `0059A238` | UI vtbl+32 consumer | **PROVEN** |
| `0041E6D3` | input vtbl+56 fan-in | **PROVEN** hop; **DISPROVEN** type-4 consumer |
| `00416E78` | game `WorldFrame>1` | **PROVEN** skip on first Present |
| DIK 28 / `Key.N` / WASD | first-seen control | **DISPROVEN** |
| Some other DIK | posts `0xE5`/`0x126`/15 | **UNREAD** |

---

## Do not invent

- WASD as frontend or first-seen in-game control.
- `Key.N` / `ActivateNewGame` in `Program.cs`.
- Host Return → msg 15 from Press Start.
- Input type 10 as MMB / as widget type 10.
- Pad A as type 4.
- `mov [widget+352], 0xE5`.
- Persist 15 on the type-10 PRESS_START packet.
- `0041E6D3` as the Press Start `0xE5` consumer.
- `0055CB10` / F2 `FlyCamera` as first-seen locomotion.

---

## Sources

- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
- `listing-00540000.txt`
- `listing-00580000.txt`
- `listing-00600000.txt`
- `listing-00a00000.txt`
- `listing-00a80000.txt`
- `proofs/input-type10-mmb/README.md`
- `proofs/input-vtbl56-vs-ui32/README.md`
- `proofs/pad-a-vs-type4/README.md`
- `proofs/0041E6D3-frontend-gate/README.md`
- `proofs/action28-after-26/README.md`
- `proofs/messageid-plus228/README.md`
- `proofs/0054E4F0-store-shape/README.md`
- `proofs/leftover-14-native-key/README.md`
- `src/Fable.Client/Program.cs`
- `src/Fable.Game/FrontendInputMap.cs`
