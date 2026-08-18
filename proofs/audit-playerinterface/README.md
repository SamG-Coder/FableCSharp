# Audit: `PlayerInterface.cs` vs exe — Hero on frontend? Invented WASD?

Investigation only. No production `src/` edits.

Authority: `Fable.exe` `0042E3EE` / `0042F0AC` / `004473A0` /
`00446A30` / `00416E78` / `004184BD` / `006AC910`;
`src/Fable.Game/PlayerInterface.cs`;
`src/Fable.Game/EngineInput.cs`;
`src/Fable.Game/EngineLifecycle.cs`;
`src/Fable.Game/FrontendInputMap.cs`;
`src/Fable.Client/Program.cs`;
`src/Fable.Render/FlyCamera.cs`;
`docs/runtime/FORWARD_TREE.md` §§4, 7–8, 11;
`docs/status/README.md` (input / Player Interface rows);
`proofs/player-bind-world/README.md`;
`proofs/type4-input-lifecycle/README.md`;
`proofs/camera-after-leave/README.md`;
`EngineLifecycleTests.Input_0042E3EE_dispatches_0041E5F2_actions`,
`Player_interface_00446A30_pumps_listeners_after_WorldFrame`,
`Player_apply_0041649C_queues_009F1650_on_action_2`,
`After_WorldFrame_gt_1_00416E78_is_004457F0_then_00446A30`,
`Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

---

## Verdict

**No Hero during frontend. WASD is not game / frontend input.**

| Claim | Status |
| --- | --- |
| Frontend frame input is `0042E3EE` (`0042F0AC` only caller) | **PROVEN** |
| `0042E3EE` is WASD / Hero walk | **DISPROVEN** |
| `PlayerInterface` (`004473A0` / `game+32`) exists on frontend | **DISPROVEN** |
| `00446A30` runs on the retail frontend loop | **DISPROVEN** |
| Hero Thing / mesh 4299 exists during frontend | **DISPROVEN** |
| `PlayerInterface.cs` stores Hero, mesh, or XYZ | **DISPROVEN** |
| Keyboard defaults slots 0–3 are WASD (`DIK_W/A/S/D`) | **DISPROVEN** |
| Host F2 `FlyCamera` WASD is native | **DISPROVEN** — **LEFTOVER** debug |
| Host `Key.A`/`Key.B` → `KeyDikA`/`KeyDikB` as walk | **DISPROVEN** — frontend actions 4 / 5 |

---

## 1. Two different input machines

Native retail and game do **not** share one “player WASD” pump.

```
0042EC7C retail frontend
  0042E3EE  [0x13B8388] / 009F4ED0     ← UI / action singleton
  0042DC94  / 00599E3F
  0042DF9E  2D Present 009BEEB0
  msg 15 → 0042F2A2 Leave

004184BD Init Game  (after Leave)
  "Init Player Interface" 004473A0     ← first construct
    alloc 0x898, vtbl 01231BDC, game+32
    owner 0044A3B0 at game+28
    listener 00488D20 / 00687A30 / 0123758C

004189C2 game pump
  00418289 → 004AEBA0 (+9826)
    00416E78 vtbl+24
      WorldFrame<=1: skip 004457F0 / 00446A30
      WorldFrame>1:  004457F0 then 00446A30
```

`0042E3EE` only caller is `0042F0AC` (retail frontend).
`00446A30` has **zero** E8 of itself; caller is `00416E78`.
Not the same walk. Host comments and
`Player_interface_00446A30_pumps_listeners_after_WorldFrame`
already lock this.

Host mapping:

| Stage | Native | Host |
| --- | --- | --- |
| `StartupVideos` | blocking `006286F0`; no `0042E3EE` | `PumpInput()` — **DIVERGE** (poll only; no bank / Hero) |
| `Frontend` | `0042E3EE` | `PumpFrontendFrame` → `PumpInput` + `FrontendInputMap` |
| `Game` | `00416E78` → `00446A30` | `UpdateGameMode` → `PumpPlayerInterface` |

`PumpInput()` has **two** call sites: AVI skip and frontend.
Game never calls it. Game dequeues via `Player.Pump(Input)`.

---

## 2. Hero during frontend?

**DISPROVEN.**

| Object | When constructed | Frontend live? |
| --- | --- | --- |
| `PlayerInterface` `004473A0` `game+32` | Init Game stage list | **No** |
| Create Players `004AE940` `+9826` | after Init World | **No** |
| Hero Thing `006AC910` mesh **4299** | first real region (`LookoutPoint` / `GuildArrivalHSP`) | **No** |
| WorldCamera / GameCamera | Init World `004A6E30` | **No** (see `camera-after-leave`) |

`PlayerInterface.cs` has **zero** `Hero` references. It is an
event list + poll/fallback. `E-player-palskin.md`:
`PlayerInterface` / `NewGameScript` draw the player = **DISPROVEN**.

`Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint` requires
Leave → `ActivateNewGame` → `LoadFromFirstRealRegion`.
`After_WorldFrame_gt_1_*` still has `CurrentRegion==null` and
does not call `LoadFromFirstRealRegion`.

Frontend Present is 2D (`0042DF9E` / `VSHADER_2D_SPRITE`).
No 3D Hero to move.

Display name `00435070` in `DisplaySubmitStages` is the
**game HUD skip** (`00487DC0` miss → skip `0057B43F`), not
`PlayerInterface.cs` and not frontend.

---

## 3. Frontend `0042E3EE` is not WASD

`EngineInput` / test recover note (`e7b3c76`):

| Event | Result |
| --- | --- |
| type 1 (`[record+40]`) | `+192=key`; `0055CB10(33)` then mask bits |
| type 4 | action **26** (no DIK compare) → Press Start `0xE5` |
| type 10 | action 27 |
| type 13 | action 25 |

Type-1 mask encoder (`EndPoll`) → actions **0–5 / 20–21 / 8–11 / 22–23**.
`0055CB10` records. **No recovered player-move listener.**
Those actions do not write Thing XYZ on frontend.

`0041DF10(0)` keyboard defaults at `+36`:

| Slot | Key | Role in `0042E3EE` |
| --- | --- | --- |
| 0 | `0x6F` (111) | mask `0x4` → actions 2, 20 |
| 1 | `0x70` (112) | mask `0x8` → actions 3, 21 |
| 2 | `0x72` (114) | mask `0x2` → action 1 |
| 3 | `0x6D` (109) | mask `0x1` → action 0 |
| 6 | `0x1E` `DIK_A` | mask `0x100` → action **4** |
| 7 | `0x30` `DIK_B` | mask `0x200` → action **5** |
| 11 | `0x11` `DIK_W` | **not** a movement slot; `KeyBit` 0 |

`DIK_W=0x11`, `DIK_A=0x1E`, `DIK_S=0x1F`, `DIK_D=0x20`.
Slots 0–3 are **not** those codes. `DIK_S` / `DIK_D` are
**absent** from `KeyboardDefaults`. `DIK_A` is action 4, not walk.
`DIK_W` does not set a movement bit.

Press Start / New Game:

- type 4 → action 26 → widget+352 `0xE5` (**PROVEN**)
- Return `DIK 28` is type 1 / action 33 — **not** `0xE5` (**DISPROVEN**)
- Physical device for type 4 still **UNREAD** (`DikPosterUnread`)
- Message 15 Leave is a later UI poster, not WASD

---

## 4. `PlayerInterface.cs` vs exe

Match (game path only):

| Native | Host |
| --- | --- |
| `004473A0` size `0x898` vtbl `01231BDC` `game+32` | `Ctor` / `Construct` in Init Game |
| `004457F0` `[+2196]=0` | `Preprocess` |
| `00446A30` → `00446330` / miss `00446220` | `Pump` / `TryPoll` / `TryFallback` |
| skip type 0, key 15, device 2 | `SkipKey=15`; type 0 continue |
| `0123758C` accept `00687DB0` apply `00687FD0` | `ActionInputListener` |
| `0041649C` then `004AE9A0`/`009F1650` if action 1/2 | `ApplyInputEvent` / `QueueAction` |

Does **not** do:

- spawn / bind Hero
- write `World.Positions["Hero"]`
- interpret WASD
- run on `EngineStage.Frontend`

Default owner result is 0, so a queued `KeyMove3` on the
game pump yields `DeliveredCount=0` until a recovered
`ResultSelect` item (`Player_interface_00446A30_*`).
Even **after** Leave, first empty `00446A30` is `al=0`,
no `0041649C`.

`004AE9A0` is a sibling on `game+80568`, not Hero
(`player-bind-world`).

Game poll `00446462` / `004963E6` remains **UNREAD**
(status leftover). Host does not invent those as WASD.

---

## 5. C# leftovers / diverges (not Hero)

| Site | What | Class |
| --- | --- | --- |
| `Program.cs` F2 + `Key.W/A/S/D/Q/E` | moves `FlyCamera` only; never `QueueInput`; never `life.Hero` | **LEFTOVER** invented debug |
| `FlyCamera.cs` | comment: must not write game / script camera | **LEFTOVER** |
| `Program.cs` `Key.A`/`Key.B` | `TypeKey` + `KeyDikA`/`KeyDikB` on **every** stage | frontend actions 4/5 **EQUIVALENT** classify; **not** walk. Does not Leave. |
| `PumpInput` during AVI | extra `0041E5F2` vs blocking `006286F0` | **DIVERGE** (already `texture-library-open`) |
| `BuildFrame().Camera` on frontend | `ScriptedCamera` always attached | **LEFTOVER** (`camera-after-leave`) |
| `EngineInput.Dispatch` | records actions; no listener applies move | **PARTIAL** / matches “no recovered listener” |

`SilkEngineHost` has no Hero / WASD. It Presents `EngineFrame`.

---

## 6. Answers

**Hero during frontend?**
**No.** Frontend has no `004473A0`, no `006AC910`, no mesh 4299,
no world camera. Input is UI (`0042E3EE` → `0055CB10` / type-4
`0xE5`). 3D Hero is after Leave → Init Game → first real region.

**Invented WASD?**
**As game / frontend input: yes, if treated as native — DISPROVEN.**
Native movement slots are `0x6F/0x70/0x72/0x6D`, not WASD.
Host WASD exists only as F2 `FlyCamera` debug and must stay
off the lifecycle queue.

Do not wire `Key.W/A/S/D` into `PlayerInterface` or
`EngineLifecycle.Hero` on frontend frames.
Do not treat `PlayerInterface` as the frontend input path.

---

## Open

- Physical device that posts type 4 (**UNREAD**).
- Game-mode poll `00446462` / `004963E6` (**UNREAD**).
- Who consumes `0055CB10` actions 0–5 / 20–21 as actual
  locomotion after Leave (**UNREAD**; not recovered on frontend).
- `00435070` HUD body after the first-seen miss (**Note** only).
