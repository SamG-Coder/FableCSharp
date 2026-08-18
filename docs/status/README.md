# Status — done vs left

Human overview of FableCSharp. **Not** a rewrite of the evidence
ledgers. If a number, address, or “done” claim is not in those files
or in tests/code, treat it as **UNREAD**.

| This page | The ledgers |
|---|---|
| Skim what’s locked and what’s next | Authoritative tables, addresses, tests |
| Sequenced by dependency | Do not copy their raw tables here |

**North star:** close first-scene New Game
(`StartOakValeWest` / `Q_NewOakValeIntro` / `HerosOldHouse` /
`CAM_OVIF_SHOT2`) so `CS_OAKVALE_INTRO_FATHER` can run on a real
world clock.

Snapshot: **2026-08-18**, master merge `9086f1c` (docs PR #22),
runtime HEAD `1eec3bc` (*Runtime: WaitPlayAnimation plays via vtbl+72
then leftover vtbl+104.*).
Just locked on this path: `e7b3c76`, `12e0d75`, `be8545e`, `a708e60`,
`012dccad`, `6b02b3b`, `666df8f`, `f778853`, `1eec3bc`.
Master is still proving **boot / world clock**, not animation.
README’s long-term priority list still starts with animation; that
list is not the current phase.

Live site: <https://samg-coder.github.io/FableCSharp/docs/status/index.html>
([index.html](index.html) locally; GitHub Pages from `master` `/docs`).

## How to read this

Statuses are the project’s own words:

| Word | Meaning here |
|---|---|
| **PROVEN** | Locked from exe / assets / tests |
| **PARTIAL** | Script-layer or host path recovered; apply or runtime body not |
| **UNREAD** | Not filled with a look-better guess |
| **DISPROVEN** | A previous assumption, recorded so it does not return |
| **TEMPORARY** | Stand-in classified as such (often a D3D default) |

Parse / dispatch / return ≠ apply ≠ runtime. A finished fixture is
script-layer apply, not a mesh or UI body.

This is **not** a “finish every UNREAD command” roadmap.

## Authoritative files

| Ledger | What it owns |
|---|---|
| [docs/PARITY.md](../PARITY.md) | Worked / DISPROVEN / Open |
| [docs/runtime/COMMAND_COVERAGE.md](../runtime/COMMAND_COVERAGE.md) | Token counts and per-verb overall |
| [docs/runtime/COMMAND_MAP.generated.md](../runtime/COMMAND_MAP.generated.md) | Generated parse / dispatch / apply / runtime |
| [docs/runtime/COMMAND_MAP.md](../runtime/COMMAND_MAP.md) | Hand map + StartNewGame bindings |
| [docs/runtime/FIXTURE_COMMAND_AUDIT.md](../runtime/FIXTURE_COMMAND_AUDIT.md) | Script-layer fixture outcomes |
| [docs/render/DX9_VULKAN_PARITY.md](../DX9_VULKAN_PARITY.md) | DX9 → Vulkan matrix |
| [docs/render/FIRST_SCENE_CONTRACT.md](../FIRST_SCENE_CONTRACT.md) | First-scene submit contract |
| [docs/render/FIRST_SCENE_AUDIT.md](../FIRST_SCENE_AUDIT.md) | Live-path classification + leftover UNREAD |
| [docs/render/FIRST_SCENE_WORLD_PARITY.md](../FIRST_SCENE_WORLD_PARITY.md) | World/space/visibility checklist |

Boot/clock facts below are from `EngineLifecycle` /
`EngineLifecycleTests` (not yet copied into `PARITY.md`).

## Coverage snapshot

From [COMMAND_COVERAGE.md](../runtime/COMMAND_COVERAGE.md) (generated
from exe token list `0x012C1500–0x012C2C00` + `ScriptCommandMap`):

| Metric | Count |
|---|---|
| Native command tokens | 185 |
| Recovered dispatch / return | 156 |
| Recovered apply | 95 |
| Implemented runtime | 13 |
| UNREAD tokens | 29 |

The 13 **PROVEN** overall runtimes are `FadeOut` / `FadeIn` (global and
entity), `SetTime`, `WaitFlag`, `SetFlag`, `PlayAVI`, `GamePause`,
`DoOneFrame`, `CameraPause`, `ScriptFrame`, `DoScriptFrame`.
`PlayAnimation` apply is now **PROVEN**; runtime still **PARTIAL**.
`FirstSeenPlayAnimationAppliesPose=false`. Clip keyframes unread;
PALSKIN stays bind pose.
`WaitPlayAnimation` apply is now **PROVEN**; leftover is vtbl+104.

Do not grind the 29 UNREAD tokens (Crowd*, debug, `LadyGreyIntro`,
boss fights, …) as a phase.

---

## What’s done

Sequenced as the engine actually runs. Wrong assumptions named only
when a ledger or test already records them.

### Formats / first-scene world (already locked)

| Item | Status | Where |
|---|---|---|
| TLC data formats (WLD, WAD/BBB, LEV/TNG, QST, bins, C3D, STB, …) | PROVEN | [PARITY.md](../PARITY.md) Worked |
| New Game *view* is `StartOakValeWest` / `HerosOldHouse` / `CAM_OVIF_SHOT2`, not Lookout | PROVEN | [FIRST_SCENE_CONTRACT.md](../render/FIRST_SCENE_CONTRACT.md), `WorldSceneTests` |
| Adult Lookout is not the first-scene camera | PROVEN | [FIRST_SCENE_WORLD_PARITY.md](../render/FIRST_SCENE_WORLD_PARITY.md) |
| TNG local vs STB WLD; `STB − (MapX, MapY)` meeting space | PROVEN | same |
| House 6909/6911, kid 4300 bind-pose, landscape strips, sky else-path `0x2000` | PROVEN | contract + world parity |
| DX9 → Vulkan first-scene submit (layers `0x4` → `0x40` → `0x20` → `0x2000`) | PROVEN / EQUIVALENT | [DX9_VULKAN_PARITY.md](../render/DX9_VULKAN_PARITY.md) |
| `T(cam)` on host world-space STB verts | DISPROVEN | world parity; submit uses identity W |
| Invented `stars.dat` billboards / 1 m landscape fill | DISPROVEN | PARITY / world parity |
| `S_QNOVI` + `NOVI_LiveFather` → `CS_OAKVALE_INTRO_FATHER` | PROVEN | [COMMAND_MAP.md](../runtime/COMMAND_MAP.md), PARITY |
| `00DB8680` starts the intro | DISPROVEN | start is `00DB86B0` (PARITY) |
| Intro fixture `CS_OAKVALE_INTRO_FATHER` | Finished def+60 (script layer) | [FIXTURE_COMMAND_AUDIT.md](../runtime/FIXTURE_COMMAND_AUDIT.md) |
| Fade / PlayAVI / `DoScriptFrame` / `UseCamera` / `GamePause` at script layer | PROVEN | command map + PARITY 0b |
| `return` is interpreter stop | DISPROVEN | it is a `RemoveExtras` named-arg |

### Phase 1 in progress — boot / world clock (current master)

Recent commits (`8bccec3` … `1eec3bc`) lock the retail pump, not
`00DBDE40`. Just locked: input dispatch (`e7b3c76`), game Present +
viewport (`12e0d75`), ScenePasses flush (`be8545e`), Sunnyvale persist
(`a708e60`), `00416E78` / `00446A30` listeners (`012dccad`),
`0123758C` / `0041649C` (`6b02b3b`), World.Positions dest
(`666df8f`), PlayAnimation apply (`f778853`), WaitPlayAnimation
(`1eec3bc`).

| Item | Status | Evidence |
|---|---|---|
| Launch from PE `00401067` / WinMain `00403480` / bootstrap `00402510` | PROVEN | `EngineLifecycleTests.Pe_entry_is_crt_not_new_game` |
| New Game is leave-frontend → `FinalAlbion.wld` → Init Game `004184BD` | PROVEN | same + `New_game_is_leave_frontend_then_FinalAlbion_wld` |
| Init World Map `005066E0`; Load `.wld` `00507C30` token switch | PROVEN | `Load_wld_is_00507C30_not_00DBDE40` |
| Region graph `00506D40` / `00828710` | PROVEN | `Install_banks_and_startup_videos_exist` |
| Create Players: 5 × `0x22C` slots, 4 active — not `hero_swap_*.tng` | PROVEN | `CreatePlayers_is_five_0x22C_slots_not_hero_swap` |
| Load GTNG `0050959F` stem+`.gtng`; TLC missing → skip | PROVEN | same install test (`Gtng` null, “missing”) |
| Global things: BSS `[0x13B8609]=0` → per-map `.tng` `004FDBC0`; flag → `.gtg` `004FE2A0` | PROVEN | `Gtng_is_stem_gtng_gtg_is_004FE2A0_single_file` |
| Current region index is `WorldMap+156` (`004FB150`); ctor 0 is dummy | PROVEN | `Game_pump_is_004189C2_not_00DBDE40` |
| First `004189C2` pump does **not** `SetRegionAsLoaded` | PROVEN | install test |
| No-save enqueue `00501450` → `00500540(1,0,0)` **LookoutPoint** (native index 1) | PROVEN | install test (`CurrentRegionIndex=1`) |
| Persist `PlayerRegionName` `00487C20` / `00449E60` loads named region (e.g. `StartOakVale` = 4) | PROVEN | `Persist_PlayerRegionName_is_00487C20_not_new_game` |
| `SetRegionAsLoaded` `004FC8A0` writes `+156`, then `00B42750` mode 1 | PROVEN | `e9952b8` / install test |
| `OpenStaticMap` `00B42530` STB height + compiled `.lev` (v25 / `0x1904`) | PROVEN | `7869e8e` / install test |
| `00418289` / `004AEBA0` gate player/world on `[player+9826]`; `00417001` still skips camera body while `WorldFrame<=1` | PROVEN | `7cc44c0` / `Update_00418289_*` / `Render_00417001_*` |
| `WorldFrame` inc at `004A5E10` via `0049DFB0` type-1 (`00629270` / `004A5A40`) | PROVEN | `ced722f` / `WorldFrame_004A5E10_unblocks_004164E0` |
| Camera body `004164E0` steps `arg/15` when `[0x13B8630]>0` | PROVEN | `6d7545a` |
| `0041707E` interpolates when catchup ticks are 0 (default New Game) | PROVEN | `c3be891` |
| `006B4900` world+24 slots; `006B42F0` lerps `+6296/+6312/+6328` into `ScriptedCamera` | PROVEN | `6e1ff8e` / `World_camera_006B4900_slots_lerp_into_ScriptedCamera` |
| Frontend New Game click: `0059A238` msg 15 → `[retail+41]=1` → Leave `0042F2A2` | PROVEN | `64a2e14` / `Frontend_00595582_new_game_message_leaves_without_RequestNewGame` |
| Menu built at `00595B24` (`UI_TEXT_NEW_GAME` id=0); not `00DBDE40` | PROVEN | same |
| Frontend frame `0042EC7C`: input `0042E3EE` → fill → draw `0042DF9E` (BeginScene / UI vtbl+8 / EndScene / Present) | PROVEN | `0d8f5e5` / `Frontend_0042EC7C_frame_is_input_then_0042DF9E_Present` |
| Same Present as PlayAVI (`009BEEB0`); extra `.wmv` after draw skipped (`00595A03` always 0) | PROVEN | same |
| `006C2170` Loading objects → `00522720` / `00521AE0` current-map `.tng` (LookoutPoint on no-save) | PROVEN | `1ebece6` / `Loading_objects_00521AE0_loads_LookoutPoint_tng` |
| `0051FD80` Load Single Thing: no-save LookoutPoint TNG has no PlayerCreature; `HOLY_SITE_PLAYER_START` `GuildArrivalHSP` → `00489D40` / `006AC910` inserts PlayerCreature at that pose | PROVEN | `8f89aad` / `Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint` (refined by `e0e0511`) |
| No-save hero is `00DBDE40` / `CREATURE_HERO_CHILD` / StartOakVale | DISPROVEN | same |
| `004AE940` Create Players writes `[player+9826]=1` because `0099A350` always returns 1 (`al=1`) | PROVEN | `ea479d2` / `CreatePlayers_004AE940_sets_plus9826_via_0099A350` |
| `+9826` stays 0 after Create Players (first `004162B5` skips world) | DISPROVEN | same; first pump takes player/world/vtbl+24; `WorldFrame` increments |
| No-save first *rendered* scene is LookoutPoint `RegionThings` + hero camera `006B3FF0`; client `BindLifecycleFirstRegion` skips if map name contains StartOakVale | PROVEN | `fe6a11e` / same LookoutPoint test + client early return |
| `00662880` / `004CA010` insert binds `CREATURE_HERO` mesh 4299 after the `PLAYER_HERO` miss chain | PROVEN | `e0e0511` / `Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint` (`HeroMeshId=4299`) |
| `0049F180` after characters: Init GUI `0043A380` `PLAYER_GUI_PC` | PROVEN | `21491ac` / same test (`PlayerGuiReady`); bind still #17 / PARTIAL (Note-only) |
| `00403079` / `009C0E50` display defaults 1024×768, title `TEXT_GUI_WINDOW_TITLE` | PROVEN | `48a879ac` / `Window_00403079_defaults_1024x768_and_title`. Client Size = `BackBufferWidth`/`Height`. `DefaultVulkan` 1600×900 DISPROVEN (issue #8 closed). |
| `004B4260` activates WLD `START_INITIAL_QUESTS` (world+172 from `00507C30`) | PROVEN | `efa0e541` / `Init_quests_004B4260_activates_wld_initial_list` (`QuestsInitDone=true`) |
| `00CB5AD0` starts `QuestFactoryTable` factory scripts (not `S_QNOVI`) | PROVEN | `48a879ac` / `Activate_quests_00CB5AD0_starts_factory_scripts` |
| `0042E3EE` type/key events dispatch `0041E5F2` actions 0–5 / 20–21 (not WASD) | PROVEN | `e7b3c76` / `Input_0042E3EE_dispatches_0041E5F2_actions`. `0055CB10` records actions; no recovered player-move listener. Not WASD. New Game is still keyboard N/Enter (does not close #14). |
| `009BEF80` SetViewport vtbl+188 full backbuffer 1024×768 MinZ 0 MaxZ 1 | PROVEN | `12e0d75` / `Window_00403079_defaults_1024x768_and_title` + `Game_00435530_Presents_009BEEB0_and_pumps_input` |
| Game `00435530` / `00435F50` Present is the same `009BEEB0` as frontend and PlayAVI | PROVEN | `12e0d75` / `Game_00435530_Presents_009BEEB0_and_pumps_input`. `00417001` does not Present; it calls `00435F70` → `00435530` after WorldFrame>1. Client Draw is that Present, not a second swapchain. |
| `00435530` flushes ScenePasses via `009DA9F0(1)` DIP vtbl+332 (bits `0x4` → `0x40` → `0x20` → `0x2000`) | PROVEN | `be8545e` / same `Game_00435530` test (`SubmittedLayerBits`). Order BeginScene, Clear, PlayerOverlay `00435000`→`00639E40`, PlayerInterface `00435070`, Flush2D `009D9C80`, FlushLayers `009DA9F0(1)`, EndScene, Present. Overlay/interface still Note. |
| `00CDC070` binds `Q_SunnyvaleMaster` persist slots via `004045C0` bool / `00410BE0` int | PROVEN | `a708e60` / `Activate_quests_00CB5AD0_starts_factory_scripts`. PersistTable.Sunnyvale length 38, defaults `00CDBA10` zeros. BindBool `004045C0`, BindInt `00410BE0`, SunnyvaleBind `00CDC070`. |
| `00416E78` vtbl+24 after WorldFrame>1 pumps `[game+32].vtbl+4` `00446A30` player-interface listeners (`0123758C` / `0041649C`) | PROVEN | `012dccad` / `Player_interface_00446A30_pumps_listeners_after_WorldFrame`. Init Player Interface `004473A0` alloc `0x898` vtbl `01231BDC` stored at game+32. Then `004457F0` `[+2196]=0`, poll `00446330` / `009F4ED0`. Skip device 2 / key 15 / type 0. Listeners vtbl+32 accept `00687DB0`, vtbl+16 apply `00687FD0` (`0123758C`). Zero E8 of `00446A30` itself; caller is `00416E78`. After hit: `0041649C` unless paused. Not the retail frontend `0042E3EE` walk. Not `00DBDE40`. |
| `0123758C` ActionInputListener: accept `00687DB0`, apply `00687FD0`; factory `00488D20`. `RecordingInputListener` is gone | PROVEN | `6b02b3b` / `Player_interface_00446A30_pumps_listeners_after_WorldFrame` |
| After `00446A30` hit: `0041649C` unless paused; action==2 queues `009F1650`. Default KeyMove3 `DeliveredCount=0` until owner ResultSelect (recovered) | PROVEN | `6b02b3b` / `Player_apply_0041649C_queues_009F1650_on_action_2` |
| `WorldGeometry.ApplyActorPositions` consumes `006A9960` dest via `World.Positions`. Father still `NOVI_LiveFather` via `00DB86B0`. Not a renderer hack | PROVEN | `666df8f` / `WalkTo_writes_destination_and_entity_task` |
| `PlayAnimation` apply `004C7470` / `0070D580` (vtbl+72 walk; +68 `00686920` accept; `00662A00` table; `0070C050`+`0070D580` inner) | PROVEN | `f778853` / `PlayAnimation_sets_clip_and_yields_unless_animation_pause` / `PlayAnimation_real_script_bank_line`. Runtime still PARTIAL. Clip sample unread. `FirstSeenPlayAnimationAppliesPose=false`. |
| `WaitPlayAnimation` `00CC18E0` plays via vtbl+72 (or vtbl+76 if IsTrue arg3) then leftover vtbl+104 | PROVEN | `1eec3bc` / `WaitPlayAnimation_plays_then_polls_vtbl104` |
| Game pump / first region is `00DBDE40` / StartOakVale setup | DISPROVEN | tests above |
| No-save writes `[record+36]` | DISPROVEN | `recover-record36` text in `Camera_004164E0_runs_on_install_after_WorldFrame`; null still loads |

**Correction vs a “first region is Oakvale” reading:**
`WorldMap+156` is the *index field*. No-save New Game’s first real
write is **1 = LookoutPoint**. No-save first *rendered* scene is
that map’s `RegionThings` plus `006B3FF0` hero camera. First-scene
*intro view* is still `StartOakValeWest` / `HerosOldHouse` /
`CAM_OVIF_SHOT2` / kid (`FIRST_SCENE_*` — do not collapse into
Lookout). Persist name `StartOakVale` is index **4**. The retail
New Game *click/message* is now **PROVEN** (`0059A238` /
`[retail+41]`). Who writes persist `PlayerRegionName` is still
**UNREAD**. `[esi+42]` load/save is **UNREAD**. `00521AE0` is
per-map TNG, not global-things apply.

---

## What’s left

Dependency order. Reorder only if a ledger or test disagrees.

### 1. Finish boot / clock (now)

Keep locking today’s exe facts. Do not invent an Oakvale write on
the no-save path.

| Left | Status | Notes |
|---|---|---|
| First non-null `[NewRegion record+36]` writer | UNREAD | Null is the native no-save state |
| `[0x13B8630]` catchup-tick writers | UNREAD | 3 immediate sites; default 0 |
| `0041714D` when `world+164 != 0` | UNREAD | Default New Game is `world+164==0` |
| Slot fields beyond `+6296/+6312/+6328` (weights / `+6340/+6352`) | UNREAD | Lerp into `ScriptedCamera` is PROVEN; leftover slot bodies are not |
| `00435530` overlay `00435000` / interface `00435070` bodies | PARTIAL | Present + `009DA9F0` layer bits PROVEN; overlay/interface still Note |
| `0055CB10` frontend player-move listener | UNREAD | Actions 0–5 / 20–21 recorded; no recovered listener |
| Game input poll `00446462` / `004963E6` | UNREAD | `e7b3c76` recover note (separate from `00446A30`) |
| Who writes persist `PlayerRegionName` on New Game | UNREAD | Click/message path is PROVEN; persist HEADER writer is not |
| `[esi+42]` load/save | UNREAD | `recover-00595582`; `[esi+41]` Leave is PROVEN |
| Global-things *use* after `004FDBC0` / `.gtg` parse | UNREAD | Load switch is PROVEN; `00521AE0` is per-map TNG, not this apply |
| GTNG file body | N/A on TLC | Missing skip is PROVEN |
| MiniMap `0082BA00` / villages `005064C0` bodies | UNREAD | Named from `SetRegionAsLoaded`; not claimed as runtime |
| Wire persist-Oakvale (or a proven New Game region write) to `FirstSceneWorld` | UNREAD | Host first-scene lists are a separate reconstructed path |

No-save `WorldFrame` now ticks after Create Players (`+9826=1`).
The Oakvale intro fiber still needs a proven region write (persist
`PlayerRegionName` writer stays UNREAD) — do not invent one on
the no-save Lookout path.

### 2. First-scene intro fiber (apply / runtime)

Script-layer walk of `CS_OAKVALE_INTRO_FATHER` is already **Finished
def+60**. Leftover is apply/runtime, not “map the next unread
opcode.” Last persist-vector-0 command:
`Hero.SneakTo MK_OVIF_HERO5,0.0,TRUE`
(`RegionTravel.IntroCutsceneLastCommand`).

| Leftover on this fiber | Status | Where |
|---|---|---|
| `PlayAnimation` runtime / clip sample (`vtbl+72` `004C7470` walk; +68 `00686920` accept; `00662A00` table; `0070C050`+`0070D580` inner). Apply is **PROVEN**; leftover is clip sample. `FirstSeenPlayAnimationAppliesPose=false` | PARTIAL | COMMAND_MAP, PARITY 0b; `f778853` |
| `WaitPlayAnimation` leftover vtbl+104 poll (apply `00CC18E0` is **PROVEN**; not unread apply) | PARTIAL | `1eec3bc` / COMMAND_MAP |
| `Create` `008A9100` / `Remove` `004C9B80` mesh | UNREAD | PARITY 0b “next unread” |
| Skip-key bodies / vector 1 | UNREAD | first-seen skip does not fire |
| Who writes `[quest+80]` / `AttackOver` after `vtbl+2584(12)` + `HerosOldHouse` | UNREAD | not a `mov` in `00DBDE00–00DBF000` |
| `LookToThing` / `LookInDirection` heading bodies | PARTIAL | record + yield / no yaw write |
| `MuteSounds` apply; `DoCameraPreloading` `vtbl+1560/1568` | PARTIAL | |
| Dialogue UI (`Speak` / `InteractiveSpeak` / `DialogSpeak`) | PARTIAL | one yield; no invented UI |
| `SneakTo` / `WalkTo` mesh move (`004C72B0` stub) | PARTIAL | `FirstSeenSneakToAppliesMove=false`. Dest via `006A9960` / `World.Positions` is PROVEN (`666df8f`); mesh body is not. |
| `PlayCombatAnimation` pose | PARTIAL | `vtbl+76` does not read the name |
| `call [vtbl+8]` resume site; `vtbl+28` yield body; `Main` `00CDD440` | UNREAD | PARITY 0b |
| `WmvPlayer` never QIs `IBasicAudio` (native `00A3B9D0` does) | PARTIAL | issue #9 |

`DoScriptFrame` / `PlayAVI` / cameras / fades are **PROVEN** at the
script layer. PlayAVI dest vs 1600×900 (#8) is closed. `IBasicAudio`
(#9) stays a **PARTIAL** leftover. Do not invent fade/AVI/wake
playback beyond those bodies.

### 3. First-scene render leftovers

Only items already listed. No visual guesses.

From [FIRST_SCENE_AUDIT.md](../render/FIRST_SCENE_AUDIT.md)
“UNREAD leftover (do not invent)” and the contract STATE table:

- D3DSAMP MAG/MIN/MIP/ADDRESS / anisotropy / LOD bias
- sRGB vs linear on the sampled view
- `ALPHATESTENABLE` / `ALPHAREF` / `ALPHAFUNC`
- `FILLMODE` / `COLORWRITEENABLE` first-seen writes
- `ZENABLE` / `ZWRITE` SetRenderState site
- stencil, scissor, half-pixel
- 2D / ortho and cinematic-vs-gameplay leftover projection
- sky PS `c0/c1/c2` bank values and quality bit
- water mesh *when bind would run* (first-seen draw is empty-out)
- particles / HUD / shadows / `0x400000` sky (not submitted)

TEMPORARY stand-ins (LINEAR/REPEAT, MaxLod=1, Z test+write on) stay
classified. PlayAVI *script* apply is PROVEN; dest vs
`Silk.WindowOptions.DefaultVulkan` 1600×900 (#8) is closed.
`IBasicAudio` (#9) stays a PARTIAL leftover. Steam timing is PARITY
Open item 0, not a first-scene 3D invent.

### 4. Animation (after boot)

README item 1, *after* a ticking world. First-seen wake lines
(`CS_WAKING_UP_*`) need the clock; create still has
`FirstSeenPlaysAnim=false`.

| Left | Status |
|---|---|
| Animation resource lookup | UNREAD / PARTIAL |
| Clip evaluation | UNREAD |
| Skeletal pose | UNREAD (`FirstSeenPlayAnimationAppliesPose=false`) |
| PALSKIN dest beyond bind-pose identity | first-seen dest ≈ identity is PROVEN; play-anim product is not |

### 5. Later (README order, after first-scene boot)

| # | Area | Status |
|---|---|---|
| 2 | Movement / navigation / entity task completion | PARTIAL (dest+gait recovered; `vtbl+20` stub) |
| 3 | Dialogue / voice / audio lifecycle | PARTIAL (session recorded; UI/voice unread) |
| 4 | Generic quest lifecycle beyond `S_QNOVI` | PARTIAL |
| 5 | Create / remove ownership | PARTIAL (script record; mesh unread) |
| 6 | Runtime effects / lights / state → Vulkan | PARTIAL |
| 7 | Remaining world / shader / state | see phase 3 |

---

## Out of scope for this page

- Optional enhancements (HDR, upscale, DLSS)
- Linux / non-DirectShow `PlayAVI`
- Grinding Crowd* / debug / `LadyGreyIntro` / boss UNREAD tokens
- Making scenes “look like Fable”

---

## Phase check vs 2026-08-18 hypothesis

The boot-first sequence holds. Corrections from the repo:

1. **No-save first real region and first *rendered* scene are
   LookoutPoint** (index 1, `RegionThings` + `006B3FF0` hero
   camera). Oakvale is persist-name index 4 or the first-scene
   *intro view* contract (`StartOakValeWest` / `HerosOldHouse` /
   `CAM_OVIF_SHOT2` / kid). Do not collapse that into Lookout.
   New Game *click* is PROVEN; persist `PlayerRegionName` writer is not.
2. **GTNG is not an unread file on TLC** — missing skip is PROVEN.
   `00521AE0` loads the current map `.tng`. Remaining UNREAD is
   global-things *use* after `004FDBC0` / `.gtg` parse, plus
   `[record+36]` and catchup-tick writers.
3. **Intro opcodes are already walked.** Phase 2 is apply/runtime
   leftovers on that fiber, not “finish UNREAD tokens.”
4. **README animation-first is the long-term engine list**, not
   today’s master. Animation stays after the clock.

When this page and a ledger disagree, the ledger wins.
