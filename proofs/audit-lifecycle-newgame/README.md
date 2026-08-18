# Audit: New Game / `RequestNewGame` / `ActivateNewGame` / world load

Investigation only. No production `src/` edits.

Do **not** start at `00DBDE40` / `StartOakVale` / `S_QNOVI` /
`Q_NewOakValeIntro` / kid `CREATURE_HERO_CHILD`.
The no-save New Game click is UI message **15** →
Leave `0042F2A2` → `FinalAlbion.wld`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: `src/Fable.Game/EngineLifecycle.cs`
(`ActivateNewGame` / `DispatchFrontendMessage` / `RequestNewGame` /
`Pump` / `EnterGame` / `LoadWorld` / `InitCharactersAndQuests` /
`ActivateNamedQuest` / `SpawnHero` / `LoadFromFirstRealRegion`);
`src/Fable.Game/RegionTravel.cs` (`StartOakValeSetup = 0x00DBDE40`);
`docs/PARITY.md` New Game / Leave / Loading world / Gameflow rows;
`docs/runtime/FORWARD_TREE.md` §§4–10;
`EngineLifecycleTests`
(`Frontend_0059A238_message_15_sets_retail_41`,
`Frontend_00595582_new_game_message_leaves_without_RequestNewGame`,
`New_game_is_leave_frontend_then_FinalAlbion_wld`,
`LoadWorld_00416953_no_save_is_004A1840_then_0049F180`,
`No_save_does_not_activate_Q_NewOakValeIntro`,
`Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`);
`proofs/newgame-script/README.md`;
`proofs/wld-first-region/README.md`.

---

## Verdict

**C# does not jump to Oakvale, Hero, or quests during frontend.**

`ActivateNewGame` is message **15** only: `0059A238` →
`0059A2DA` → `00594F28` writes `[retail+41]=1`. Stage stays
`Frontend`. The next `Pump` that sees that flag calls
`RequestNewGame` (`0042F2A2` Leave) which records
`FinalAlbion.wld` at `0042F44D` and sets `LeaveFrontend`.
World parse, WLD quests, and Lookout hero spawn are
**after** Leave, on `0042F491` / `004184BD` / `00416953`.
None of those sites is `00DBDE40`.

| Claim | Class |
|---|---|
| Frontend New Game is msg 15 → `[retail+41]=1` (`00594F28`) | **PROVEN** |
| Pump then Leave `0042F2A2` (not `00DBDE40`) | **PROVEN** |
| Leave writes `FinalAlbion.wld` at `0042F44D` → `game+90576` | **PROVEN** |
| `00416953` opens that path, not `0x122EE14` `updatedscenic.wld` | **PROVEN** |
| First WLD `RegionName` is `LookoutPoint` (index 1); Oakvale is later `NewRegion 4` | **PROVEN** |
| Frontend / Leave / first `004189C2` traces `Va==00DBDE40` | **DISPROVEN** |
| C# `ActivateNewGame` / `RequestNewGame` spawn Hero | **DISPROVEN** |
| C# frontend activates `Q_NewOakValeIntro` / `S_QNOVI` | **DISPROVEN** |
| Hero at Lookout `GuildArrivalHSP` / adult 4299 | **PROVEN** after `00501450`, not frontend |
| `NewGameScript` / `ScriptRuntime.StartNewGame` / `FirstSceneWorld` as this path | **LEFTOVER** / **DIVERGE** |

---

## Path (must be Leave `0042F2A2` → `FinalAlbion.wld`)

```
0042EC7C  retail pump
  0042E3EE  input
  0059A238  UI vtbl+32 (012521C8)
    msg 15 → 0059A2DA [ui+28].vtbl+16
           → 00594F28 [retail+41]=1          // ActivateNewGame
    Stage stays Frontend                     // PROVEN
  [esi+41] → 0042F2A2 Leave frontend         // RequestNewGame
    [0x1375448]=0
    [0x13B8616]==0 skip 009A78D0/009A8840
    00404490 / 004131A0
    0042F44D  "FinalAlbion.wld"              // not Oakvale
    0042EBB6  teardown (+41 skip audio stop)
    009BE420 + 009BEEB0 Present
0042F491  Init Game                          // EnterGame, after Leave
  00418DCA  size 0x161E8 vtbl 0122F180
  vtbl+4 004184BD
    Init World 0041735A / 004A6E30
    Create Players 004166A8                  // slots, not Hero
    vtbl+32 00416953 Load world
      [+90588] empty skip 004A3200
      +90576 FinalAlbion.wld                 // 00415E17
      004A1840
        004A0D90  FinalAlbion.qst / GlobalQuests.qst
        00CD6E27  00CB5C90 bind Q_NewOakValeIntro
                  / S_QNOVI / 00DBEF70       // BIND ONLY
        00507C30  WLD parse
        00B23DC0 → 00B428E0  FinalAlbion.stb miss
      [0x13B8648]==0
        0049F180  Init Characters / Init GUI
        004B4260([world+172])                // WLD START_INITIAL_QUESTS
        00416BCF +90584 empty skip 004B4A10
      004BBC00  ret 4
  user.ini 009EC890 ActivateQuest("Gameflow")  // after Leave
004189C2  first game pump
  004FB150 [WorldMap+156]=0 dummy
  CurrentRegion = null                       // not Oakvale
later (host / unread E8)
  00501450  00500540(1,0,0) LookoutPoint
    GuildArrivalHSP → Hero CREATURE_HERO 4299
```

`00DBDE40` is later `Q_NewOakValeIntro` slot 2
(`00DABAC0` → `00DBDE40`). No `E8` on this walk.

---

## C# pairing (what each method actually does)

### `ActivateNewGame` — flag only

```
ActivateNewGame()
  → DispatchFrontendMessage(FrontendNewGameMessage=15)
      Stage must be Frontend
      0059A238 msg=15 vtbl+32
      0059A2DA [ui+28] vtbl+16
      00594F28 [retail+41]=1
      RetailNewGameFlag = true
      // no RequestNewGame, no World, no Hero, no quests
```

Constants: `FrontendUiMessageFn=0x0059A238`,
`FrontendNewGameApply=0x0059A2DA`,
`FrontendNewGameThunk=0x00594F28`,
`RetailNewGameFlagOffset=41`.

Press Start is a different message (`0xE5` → `00599D5C`).
Return on Press Start does **not** post 15.
New Game is not WASD.

### `RequestNewGame` — Leave `0042F2A2` only

If `Stage != Frontend`, return. Else:

| VA | Note | Side effect |
|---|---|---|
| `0042F2A2` | Leave frontend | `Stage = LeaveFrontend` |
| `0x01375448` | PlayAVI flag = 0 | none |
| `0x013B8616` | skip bank swaps | none |
| `00404490` / `004131A0` | path / record | none |
| `0042F44D` | `FinalAlbion.wld` | `WorldFileName = FinalAlbion.wld` |
| `0042EBB6` | +41 skip audio stop | none |
| `009BE420` / `009BEEB0` | clear + Present | `FrontendBatch = null` |

No `LoadWorld`. No `ActivateNamedQuest`. No `SpawnHero`.
No `00DBDE40`. No `StartOakVale`.

### `Pump` — Leave then Init Game, still not Oakvale

```
Frontend:
  PumpFrontendFrame()          // 0042E3EE / 0042DC94 / 0042DF9E
  if RetailNewGameFlag:
    RequestNewGame()           // 0042F2A2 → FinalAlbion.wld
    EnterGame()                // 0042F491 after Leave
LeaveFrontend:
  EnterGame()
Game:
  PumpGame()                   // 004189C2; dummy region
```

Same-frame `EnterGame` after the flag is **after** Leave,
not a frontend shortcut. Tests that call `RequestNewGame`
directly stop at `LeaveFrontend` until the next `Pump`.

### `EnterGame` / `LoadWorld` — after Leave

`EnterGame` refuses unless stage is `LeaveFrontend` or
`Frontend` (and if still Frontend it calls `RequestNewGame`
first). Then:

- `0042F491` Init Game
- `00418DCA` / `004184BD` named stages (not `00DBDE40`)
- `CreatePlayers` `004166A8` — five `0x22C` slots, `+9826=1`
- `LoadWorld` `00416953` → `+90576 FinalAlbion.wld` → `004A1840`
- `InitCharactersAndQuests` only when `[0x13B8648]==0`

`LoadWorld` comment in source: not a region load.

---

## Disproof: Oakvale during frontend

| VA / name | What it is | On frontend New Game? |
|---|---|---|
| `00DBDE40` `StartOakValeSetup` | later `S_QNOVI` body | **no** — tests `DoesNotContain` from bootstrap through Leave |
| `00416268` `NamedStartFn` | `[0x13B85F6]` named start | BSS 0; first pump skips |
| `00CD6E27` `OakvaleBindSite` | bind `Q_NewOakValeIntro` → `00DBEF70` | **after** Leave, inside `004A1840`; bind not `00CB5AD0` |
| WLD `StartOakVale` | `NewRegion 4` | later table row; first name is `LookoutPoint` |
| `FirstSceneWorld` / SHOT2 | Oakvale intro contract | unused on this path |

`GetCurrentRegionIndexFn` `004FB150` is
`mov eax,[ecx+156]; ret`. Ctor-zeroed. Not a host
StartOakVale index. First `004189C2` keeps
`CurrentRegion == null`.

---

## Disproof: Hero during frontend

| Site | When | Creature |
|---|---|---|
| `004166A8` Create Players | Init Game after Leave | slots 0–4; no TNG |
| `0049F180` Init Characters | after `004A1840` | bind `00449970` / `00487DC0`; no spawn |
| `0052B880` / `006AC910` `SpawnHero` | `LoadFromFirstRealRegion` `00501450` | Lookout `GuildArrivalHSP` → `CREATURE_HERO` / mesh **4299** |

`HeroSpawned` stays false through `RequestNewGame` and the
first game pumps. Tests that assert `HeroSpawned` call
`LoadFromFirstRealRegion` after Leave. Kid `4300` /
`CREATURE_HERO_CHILD` is **DISPROVEN** as this no-save
spawn (`Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`).

`PumpFrontendFrame` walks input / widgets / `009BEEB0`.
It never calls `SpawnHero` or `LoadFromFirstRealRegion`.

---

## Disproof: quests during frontend

| Site | After Leave? | Activates `Q_NewOakValeIntro`? |
|---|---|---|
| `004A1840` / `004A0D90` parse QST | yes | no — defs into `world+184` |
| `00CD6E27` / `00CB5C90` | yes | **no** — bind factory only |
| `004A113B` AddTestQuest | yes | store `world+196` only |
| `004B4260` Init Quests | yes | WLD `+172` (`Q_SunnyvaleMaster`, …) |
| `00416BCF` / `004B4A10` | yes | **skipped** (`+90584` empty vs `0x122D70E`) |
| `004B5080` START_NEW_QUEST | save only | 0 external `E8` no-save |
| `user.ini` `00419CE0` | yes (`009EC890` after vtbl+32) | `Gameflow` only |
| `00CE7670` Gameflow state 0 | first quest pump | **waits** `00893610` miss, yields |

`ActivateNamedQuest` is not reachable from
`DispatchFrontendMessage` or `RequestNewGame`.
`InitCharactersAndQuests` constructs `ScriptRuntime`
only after `00416953`. Frontend frames have
`Runtime == null` and `ActivatedQuests` empty.

Install-less `LoadWorld_00416953_*` also asserts
`ActivatedQuests` empty (no WLD file) and still
`DoesNotContain(StartOakValeSetup)`.

---

## Test locks (VA)

- `Frontend_0059A238_message_15_sets_retail_41`:
  msg 15 sets flag; stage Frontend; one `Pump` →
  `Game` + `WorldFileName == FinalAlbion.wld`;
  no `00DBDE40`.
- `Frontend_00595582_new_game_message_leaves_without_RequestNewGame`:
  `ActivateNewGame` does **not** change stage;
  next `Pump` notes `0042F2A2` and `FinalAlbion.wld`.
- `New_game_is_leave_frontend_then_FinalAlbion_wld`:
  `RequestNewGame` → `LeaveFrontend` + filename;
  next `Pump` is `00418DCA` / `004184BD`;
  `0042EBB6` Present **before** ctor; world/GTNG null
  until install; first `004189C2` dummy region.
- `LoadWorld_00416953_no_save_is_004A1840_then_0049F180`:
  order `00416953` → vtbl+28 → skip save →
  `+90576 FinalAlbion.wld` → `004A1840` →
  `00507C30` → empty `006C20A0` → `00B23DC0` →
  `0049F180` → `004B4A10` → `004BBC00 ret 4`.
- `No_save_does_not_activate_Q_NewOakValeIntro`:
  WLD has `Q_SunnyvaleMaster`, not Oakvale intro;
  bind-only `00CD6E27`; skip `004B4A10`;
  `GameflowYieldQuest == Q_NewOakValeIntro`.

---

## What is leftover vs this walk

| Host | Native New Game (msg 15 → Leave) | Class |
|---|---|---|
| `NewGameScript` | unused | **LEFTOVER** |
| `ScriptRuntime.StartNewGame` | unused | **DIVERGE** (invents Oakvale + `S_QNOVI`) |
| `FirstSceneWorld` / `CAM_OVIF_SHOT2` | unused | **LEFTOVER** |
| `EngineLifecycle.ActivateNewGame` / `RequestNewGame` | msg 15 → `0042F2A2` → `FinalAlbion.wld` | **PROVEN** |

---

## Same-pump note (not a frontend Oakvale jump)

`Pump` on `Frontend` with the flag set calls
`RequestNewGame` then `EnterGame` before
`PresentToHost`. That is host scheduling: Leave
notes (`0042F2A2` / `0042F44D`) still precede
Init Game notes (`0042F491` / `004184BD` /
`00416953`). It is **not** a jump from the menu
widget to `00DBDE40` / Hero / Oakvale quests.
Hero still requires a later `00501450`.
`Q_NewOakValeIntro` still does not activate.

---

## Open / UNREAD (out of this claim)

- Native key that posts message 15 (after main menu) —
  host injects the message; keyboard N/Enter is only
  after Press Start.
- Editor `[0x13B8648]!=0` and Loading save `004A3200`.
- `00501450` first-seen `E8` caller.
- `0040A7F0` PlayAVI apply body **PARTIAL**.
