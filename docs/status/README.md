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

Snapshot: **2026-08-19**, previous snapshot runtime HEAD
`d628952` via PR #51 merge `b11c063` (~4:46pm AEST),
runtime HEAD `b216990` (*0055BF10 hover / Accept 0x126,
0042DF9E 2D present host skip, type 11/38 DrawsChildList
+ drop hover hacks, persist clip/align + drop sibling
hide / exclusive-walk, first-seen colour style 0 +
children-by-index*).
Just locked: 5 runtime commits since `d628952`. Headline
locks in order: 0055BF10 hover / Accept 0x126 (`681620d`);
0042DF9E 2D present host skip (`dd0a0f0`); type 11/38
DrawsChildList + drop hover hacks (`e3208eb`); persist
clip/align + drop sibling hide / exclusive-walk
(`88a9ab8`); first-seen colour style 0 + children-by-index
(`b216990`).
Ignore merge `b11c063` / docs `b7f45c1`, and ignore
ExeIndex-only `694221a`. CSS unchanged.
Freeze at `b216990`. Do not include any later runtime if
master moves. dest still invented 512,384,512,384; DIP
`(0,0)` stand-in; native skip via vtbl+400 (+504) still
unused; New Profile dest/hit is host stand-in;
first-proximity TNG is host OOM workaround. leave #14 and #20 open
(also leave #36 #42 #46 #48 #50 open). Master is still
proving **boot / world clock**, not animation.
README’s long-term priority list still starts with animation;
that list is not the current phase.

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
| [docs/runtime/FORWARD_TREE.md](../runtime/FORWARD_TREE.md) | PE → WinMain → no-save New Game function tree |
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
`FirstSeenPlayAnimationAppliesPose=false`. Type-6 XSEQ first-key
sample drives `PaletteForPose` 48-byte locals (`00A999B0` /
`00AA4680` / `00A4C5E0`). Time interpolation `00AA0090` unread.
`WaitPlayAnimation` apply is now **PROVEN**; leftover is vtbl+104.

Do not grind the 29 UNREAD tokens (Crowd\*, debug, `LadyGreyIntro`,
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
| DX9 → Vulkan first-scene submit (layers `0x4` → `0x40` → `0x20` → `0x100` → `0x2000`) | PROVEN / EQUIVALENT | [DX9_VULKAN_PARITY.md](../render/DX9_VULKAN_PARITY.md); `0x100` drain `676bf63` |
| `T(cam)` on host world-space STB verts | DISPROVEN | world parity; submit uses identity W |
| Invented `stars.dat` billboards / 1 m landscape fill | DISPROVEN | PARITY / world parity |
| `S_QNOVI` + `NOVI_LiveFather` → `CS_OAKVALE_INTRO_FATHER` | PROVEN | [COMMAND_MAP.md](../runtime/COMMAND_MAP.md), PARITY |
| `00DB8680` starts the intro | DISPROVEN | start is `00DB86B0` (PARITY) |
| Intro fixture `CS_OAKVALE_INTRO_FATHER` | Finished def+60 (script layer) | [FIXTURE_COMMAND_AUDIT.md](../runtime/FIXTURE_COMMAND_AUDIT.md) |
| Fade / PlayAVI / `DoScriptFrame` / `UseCamera` / `GamePause` at script layer | PROVEN | command map + PARITY 0b |
| `return` is interpreter stop | DISPROVEN | it is a `RemoveExtras` named-arg |

### Phase 1 in progress — boot / world clock (current master)

Recent commits (`681620d` … `b216990`) lock 0055BF10
hover so Accept LMB posts 0x126, Present frontend as
0042DF9E 2D only (host skip world/gizmos), type 11/38
DrawsChildList + drop leftover hover hacks, persist
clip/align/style ColourA + drop sibling hide /
exclusive-walk, and first-seen colour style 0 +
children-by-index. Previous
snapshot `d628952` via PR #51 `b11c063` (~4:46pm AEST).
Just locked this batch: 0055BF10 hover / Accept 0x126,
0042DF9E 2D present host skip, type 11/38 DrawsChildList
+ drop hover hacks, persist clip/align + drop sibling
hide / exclusive-walk, first-seen colour style 0 +
children-by-index. MATCH for
named-stage Add Def Class is still Note-only +
`*DefClassRegistered` flag, not a live constructed
object. dest `0044C72B` as `[01232C24+8]` is not
rdata-locked (leave #42 open).
`DisplayFlushShouldDip(0, 0)` is always false;
host never stores `[this+16020]`. Empty DIP Note is
stand-in, not a recovered queue read (leave #36
open). dest is still invented 512,384,512,384 and
Press Start dest table is unchanged (type-6 still
remapped point). `CompositeFrontendPresent` still
`BuildBatch(..., 0, 0, width, height)`.
`DrawContainerWalk` / `IsPresented` no longer
exclusive-walk `kids[ActiveChild]` (dropped in
`88a9ab8`). Native skip via vtbl+400 (+504) still
unused; type-18 inactive siblings present (leave #46
open). New Profile dest/hit
is host stand-in: `TryChromeHit` still invents
type-16/37 hit; `TryMouseAreaDest` dest-copy was
removed in `e3208eb` (dest stays a point);
persist-size skip still host (leave #48
open). First-proximity TNG is host OOM workaround,
not a locked native `ebx=1` / NewMap slot /
`00501450` (leave #50 open). Style-6 persist /
style-tick / style-lookup still named-only
(constants), not applied. `StyleFlagsZeroDest` 0x20
still unused. #5 grok-goal dump still
written — do not re-file. Resident slots 0 / `0x14` /
`0x17` still MATCH `00595222`. Present `0042DF9E`
still Note. Host still skips world/gizmos on
`_frontendReady`. New Game still not locked (Enter is
PlayAVI skip). leave #14 open. PlayAVI still
`host.Draw`. leave #20 open. CSS unchanged.

| Item | Status | Evidence |
|---|---|---|
| Launch from PE `00401067` / WinMain `00403480` / bootstrap `00402510` | PROVEN | `EngineLifecycleTests.Pe_entry_is_crt_not_new_game` |
| New Game is leave-frontend → `FinalAlbion.wld` → Init Game `004184BD` | PROVEN | same + `New_game_is_leave_frontend_then_FinalAlbion_wld` |
| Init World Map `005066E0`; Load `.wld` `00507C30` token switch | PROVEN | `Load_wld_is_00507C30_not_00DBDE40` |
| Region graph `00506D40` / `00828710` | PROVEN | `Install_banks_and_startup_videos_exist` |
| Create Players: 5 × `0x22C` slots, 4 active — not `hero_swap_*.tng` | PROVEN | `CreatePlayers_is_five_0x22C_slots_not_hero_swap`. Runs at the named `004166A8` stage (`b8a2b21`). Hero swap names `hero_swap_1.tng`…`_4.tng` belong on the player owner at game+28 (`4a03969`), not here. |
| Load GTNG `0050959F` stem+`.gtng`; TLC missing → skip | PROVEN | same install test (`Gtng` null, “missing”) |
| Global things: BSS `[0x13B8609]=0` → per-map `.tng` `004FDBC0`; flag → `.gtg` `004FE2A0` | PROVEN | `Gtng_is_stem_gtng_gtg_is_004FE2A0_single_file` |
| Current region index is `WorldMap+156` (`004FB150`); ctor 0 is dummy | PROVEN | `Game_pump_is_004189C2_not_00DBDE40` |
| First `004189C2` pump does **not** `SetRegionAsLoaded` | PROVEN | install test |
| No-save enqueue `00501450` → `00500540(1,0,0)` **LookoutPoint** (native index 1) | PROVEN | first i=1; then i=2..141 |
| `00501450` is only `00500540(1,0,0)` | DISPROVEN | loops all i; last `+156=141` `Filler_NorthernWastes_02`; restore `(0,0,1)` no pump |
| `005198B0` releases the `0048D400` list | DISPROVEN | second collector: `+145` then `CTCActionUseScriptedHook` |
| `00501450` E8/E9/imm/vtbl | UNREAD | 0 hits; not `004162B5` / `00418289` / `004189C2` (`0049D9E0` is `ret`) |
| Host seeds type-1 tick on InitGame | DISPROVEN | first-seen `game+164` empty; `0041726D` skips |
| Inner loop `009F8BA0` before `004162B5` | PROVEN | `[game+52]==0`; IAT `0x14404B4`; `+90556` |
| `00416E78` skipped when `WorldFrame<=1` | DISPROVEN | prefix always runs; only `004457F0` is gated |
| Persist `PlayerRegionName` `00487C20` / `00449E60` loads named region (e.g. `StartOakVale` = 4) | PROVEN | `Persist_PlayerRegionName_is_00487C20_not_new_game` |
| `SetRegionAsLoaded` `004FC8A0` writes `+156`, then `00B42750` mode 1 | DISPROVEN | `004FC8A0` is MiniMap only. `00B428E0` is `004A1840` vtbl+208 |
| `004FC8A0` calls `005064C0` / `00B428E0` | DISPROVEN | `004FC8A0` is `+156`/`00437CE0`/`0082BA00`. `005064C0` is vtbl+88 before it. `SetRegionAsLoaded_004FC8A0_is_minimap_after_005064C0` |
| `00B428E0` after `004AFC00` | DISPROVEN | `00500540` tail is dtor/`ret 12`. Caller is `00B23DC0` from `004A1840` `004A1BD3` |
| `004A1840` WLD before Startup WAD | DISPROVEN | `00507C30` is world `vtbl+8` `0049E220` after WAD; `006C20A0` empty; Generate Offline skipped |
| First-seen `00B428E0` opens Lookout STB | DISPROVEN | arg is `Data\Levels\FinalAlbion.stb`; file absent; `00B42750` miss |
| `OpenStaticMap` `00B42530` STB height + compiled `.lev` (v25 / `0x1904`) | PROVEN | `7869e8e` / install test |
| `00418289` / `004AEBA0` gate player/world on `[player+9826]`; `00417001` still skips camera body while `WorldFrame<=1` | PROVEN | `7cc44c0` / `Update_00418289_*` / `Render_00417001_*` |
| `WorldFrame` inc at `004A5E10` via `0049DFB0` type-1 (`00629270` / `004A5A40`) | PROVEN | `ced722f` / `WorldFrame_004A5E10_unblocks_004164E0` |
| Camera body `004164E0` steps `arg/15` when `[0x13B8630]>0` | PROVEN | `6d7545a` |
| `0041707E` interpolates when catchup ticks are 0 (default New Game) | PROVEN | `c3be891` |
| `006B4900` world+24 slots; `006B42F0` lerps `+6296/+6312/+6328` into `ScriptedCamera` | PROVEN | `6e1ff8e` / `World_camera_006B4900_slots_lerp_into_ScriptedCamera` |
| Leave Press Start: `0059A238` msg `0xE5` → `00599D5C` empty `005955AB` → `00595845` → `00596917` slot `0x17` `NEW_PROFILE` | PROVEN | `Frontend_0059A238_msg_E5_empty_005955AB_is_00595845_then_00596917`. Type-4 posts type-10 +352 only (`48133e9`). Host LMB edge queues Type4 (`5dcc1fc`). Host Return quarantined as accept. Return→msg 15 from Press Start DISPROVEN. Type-10 `0xE5` is the `00598A1C` attach write (`00598EE6` is the mov inside it) → widget+352 (`61e430f`). #37 already locked. Later `59fde69` writes `0xE5` through slot `0x14` `0059B5D7` then vtbl+284 `0054E4F0`. |
| New Profile accept: `00851770` seeds `0x122DE80` "Default"; msg `0x126` → `00851920` → `0059697A` `MAIN_MENU_NO_CONTINUE` | PROVEN | `Frontend_00851770_seeds_Default_then_0x126_is_0059697A_main_menu`. Persist CRC `0x53C644E4` as MessageId (def+228 via `0055B040` / `00632500`): type-38 `UI_ACCEPT_NEW_PROFILE` stores `0x126` (`f38f9f9`). Action 26 posts widget+372 from def+224 (0 first-seen). LMB-up action 28 posts +228 after arm (`48133e9`). |
| Frontend New Game click: `0059A238` msg 15 → `[retail+41]=1` → Leave `0042F2A2` | PROVEN | `64a2e14` / `Frontend_00595582_new_game_message_leaves_without_RequestNewGame`. Type-11 `UI_FRONTEND_BUTTON_NEW_GAME` stores 15 at +228 (`f38f9f9`). Action 26 posts empty +224 first-seen (`9e94ef9`). LMB-up action 28 posts +228 after arm (`48133e9`). |
| Menu built at `00595B24` (`UI_TEXT_NEW_GAME` id=0); not `00DBDE40` | PROVEN | same |
| Frontend frame `0042EC7C`: input `0042E3EE` → fill → draw `0042DF9E` (BeginScene / UI vtbl+8 / EndScene / Present) | PROVEN | `0d8f5e5` / `Frontend_0042EC7C_frame_is_input_then_0042DF9E_Present` |
| Same Present as PlayAVI (`009BEEB0`); extra `.wmv` after draw skipped (`00595A03` always 0) | PROVEN | same |
| `006C2170` Loading objects → `00522720` / `00521AE0` current-map `.tng` (LookoutPoint on no-save) | PROVEN | `1ebece6` / `Loading_objects_00521AE0_loads_LookoutPoint_tng` |
| `0051FD80` Load Single Thing: no-save LookoutPoint TNG has no PlayerCreature; `HOLY_SITE_PLAYER_START` `GuildArrivalHSP` → `00489D40` / `006AC910` inserts PlayerCreature at that pose. RHSet axes later (`ff808b1`); #18 done | PROVEN | `8f89aad` / `Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint` (refined by `e0e0511`) |
| No-save hero is `00DBDE40` / `CREATURE_HERO_CHILD` / StartOakVale | DISPROVEN | same |
| `004AE940` Create Players writes `[player+9826]=1` because `0099A350` always returns 1 (`al=1`) | PROVEN | `ea479d2` / `CreatePlayers_004AE940_sets_plus9826_via_0099A350` |
| `+9826` stays 0 after Create Players (first `004162B5` skips world) | DISPROVEN | same; first pump takes player/world/vtbl+24; `WorldFrame` increments |
| No-save first *rendered* scene is LookoutPoint `RegionThings` + hero camera `006B3FF0`; client `BindLifecycleFirstRegion` skips if map name contains StartOakVale | PROVEN | `fe6a11e` / same LookoutPoint test + client early return |
| `00662880` / `004CA010` insert binds `CREATURE_HERO` mesh 4299 after the `PLAYER_HERO` miss chain | PROVEN | `e0e0511` / `Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint` (`HeroMeshId=4299`) |
| `0049F180` after characters: Init GUI `0043A380` `PLAYER_GUI_PC` | PROVEN | `21491ac` / same test (`PlayerGuiReady`); bind still #17 / PARTIAL (Note-only) |
| `00403079` / `009C0E50` display defaults 1024×768, title `TEXT_GUI_WINDOW_TITLE` | PROVEN | `48a879ac` / `Window_00403079_defaults_1024x768_and_title`. Client Size = `BackBufferWidth`/`Height`. `DefaultVulkan` 1600×900 DISPROVEN (issue #8 done). |
| `00413C50` `userst.ini` `SetFullscreen` / `009A64B0` style `0xCA0000` / `009BF7E0` `Windowed=![0x137544A]` | PROVEN (name→byte PARTIAL) | `Userst_00413C50_SetFullscreen_false_is_009BF7E0_windowed`. d3d9 `ForceWindowedMode` wrapper is not authority. |
| `004B4260` activates QST AddQuest TRUE as world+172 (FinalAlbion then GlobalQuests, not WLD `START_INITIAL_QUESTS`) | PROVEN | `5dcc1fc` / `Init_quests_004B4260_activates_wld_initial_list` (`QuestsInitDone=true`). `00507C30` has no START_INITIAL_QUESTS case. |
| `00CB5AD0` starts `QuestFactoryTable` factory scripts (not `S_QNOVI`) | PROVEN | `48a879ac` / `Activate_quests_00CB5AD0_starts_factory_scripts`. QM+44 (`004B2850` / `004B00C0`) gates activate before `00CB5AD0` (`f38f9f9`). |
| `0042E3EE` type/key events dispatch `0041E5F2` actions 0–5 / 20–21 (not WASD) | PROVEN | `e7b3c76` / `Input_0042E3EE_dispatches_0041E5F2_actions`. `0055CB10` records actions; no recovered player-move listener. Not WASD. Host LMB edge queues Type4 (`5dcc1fc`). New Game keyboard N/Enter leftover still stands (leave #14 open). |
| `009BEF80` SetViewport vtbl+188 full backbuffer 1024×768 MinZ 0 MaxZ 1 | PROVEN | `12e0d75` / `Window_00403079_defaults_1024x768_and_title` + `Game_00435530_Presents_009BEEB0_and_pumps_input` |
| Game `00435530` / `00435F50` Present is the same `009BEEB0` as frontend and PlayAVI | PROVEN | `12e0d75` / `Game_00435530_Presents_009BEEB0_and_pumps_input`. `00417001` does not Present; it calls `00435F70` → `00435530` after WorldFrame>1. Client Draw is that Present, not a second swapchain. |
| `00435530` flushes ScenePasses via `009DA9F0(1)` DIP vtbl+332 (bits `0x4` → `0x40` → `0x20` → `0x100` → `0x2000`) | PROVEN | `be8545e` / `676bf63` / same `Game_00435530` test (`SubmittedLayerBits`). Order BeginScene, Clear, PlayerOverlay `00435000`→`00639E40`, PlayerInterface `00435070`, Flush2D `009D9C80`, FlushLayers `009DA9F0(1)`, EndScene, Present. Overlay/interface still Note. |
| `00CDC070` binds `Q_SunnyvaleMaster` persist slots via `004045C0` bool / `00410BE0` int | PROVEN | `a708e60` / `Activate_quests_00CB5AD0_starts_factory_scripts`. PersistTable.Sunnyvale length 38, defaults `00CDBA10` zeros. BindBool `004045C0`, BindInt `00410BE0`, SunnyvaleBind `00CDC070`. |
| `00416E78` vtbl+24 after WorldFrame>1 pumps `[game+32].vtbl+4` `00446A30` player-interface listeners (`0123758C` / `0041649C`) | PROVEN | `012dccad` / `Player_interface_00446A30_pumps_listeners_after_WorldFrame`. Init Player Interface `004473A0` alloc `0x898` vtbl `01231BDC` stored at game+32. Then `004457F0` `[+2196]=0`, poll `00446330` / `009F4ED0`. Skip device 2 / key 15 / type 0. Listeners vtbl+32 accept `00687DB0`, vtbl+16 apply `00687FD0` (`0123758C`). Zero E8 of `00446A30` itself; caller is `00416E78`. After hit: `0041649C` unless paused. Not the retail frontend `0042E3EE` walk. Not `00DBDE40`. |
| `0123758C` ActionInputListener: accept `00687DB0`, apply `00687FD0`; factory `00488D20`. `RecordingInputListener` is gone | PROVEN | `6b02b3b` / `Player_interface_00446A30_pumps_listeners_after_WorldFrame` |
| After `00446A30` hit: `0041649C` unless paused; action==2 queues `009F1650`. Default KeyMove3 `DeliveredCount=0` until owner ResultSelect (recovered) | PROVEN | `6b02b3b` / `Player_apply_0041649C_queues_009F1650_on_action_2` |
| `WorldGeometry.ApplyActorPositions` consumes `006A9960` dest via `World.Positions`. Father still `NOVI_LiveFather` via `00DB86B0`. Not a renderer hack | PROVEN | `666df8f` / `WalkTo_writes_destination_and_entity_task` |
| `PlayAnimation` apply `004C7470` / `0070D580` (vtbl+72 walk; +68 `00686920` accept; `00662A00` table; `0070C050`+`0070D580` inner) | PROVEN | apply `f778853`. XSEQ first-key `PaletteForPose` (`00A999B0`/`00AA4680`/`00A4C5E0`) / `XSeqFormatTests`. Runtime still PARTIAL. Time interp `00AA0090` unread. `FirstSeenPlayAnimationAppliesPose=false`. |
| Engine primary-map draw, unload AVI, seed camera `006B3FF0` (LookoutPoint only; neighbour headers stay closed) | PROVEN | `9c625bc` / `Install_banks_and_startup_videos_exist` / `Open_records_instances_without_c3d_or_tiles` (`primaryOnly`) |
| PlayAVI `00A3B380`/`00A3BC20` unload graph before the next `006286F0` slot | PROVEN | `0ace433` / `PlayAvi_rewrites_xmv_to_installed_wmv_and_blocks` (`WmvPlayer.ReleaseGraphFn`, `PlayerDtorFn`, `GraphReleased`). Does not lock leftover #20 3D Draw. |
| Client is `IEngineHost`; `Pump` owns AVI, New Game, world submit. Host only queues input and Presents | PROVEN | `d9977fb` / `EngineFrame_constructs_for_host_present` / `Unexpanded_world_is_not_a_geometry_submit` / `Install_banks` `WorldSubmitted` |
| Engine submit: unique primary C3Ds + primary terrain, no world triangle soup. `SubmittedWorld` stays unexpanded | PROVEN | `f63b741` / `Install_banks_and_startup_videos_exist` (`SubmittedMesh` verts, LookoutPoint) |
| Engine submit skins PALSKIN C3Ds (`TrianglesForPose` / `00BD2F91`) not static flatten. Hero 4299 is in `SubmittedPalskinMeshIds` | PROVEN | `5cb3435` / `Install_banks_and_startup_videos_exist` + `Kid_c3d` / `Wake_loop_3420`. First-seen dest later file triangles (`cb22533`); no CPU re-skin. |
| Submit before `00435530`. Terrain is `00BDC2D0` AABB then `00BF4570` cells on opened patches | PROVEN | `b062c5d` / `TessellateVisible_uses_00bdc2d0_aabb` / `Install_banks`. Instances stay primary-only. Later persistent cells (`3dba4a1`); cell DIP `0x40` only (`40037b1`). |
| Init Graphics opens `GBANK_MAIN_PC`; submitted ids go on `EngineFrame.Textures`. Game Present waits for seed+submit | PROVEN | `991bab2` / `Install_banks` `SubmittedTextures`. Program no longer constructs `TextureLibrary`. |
| First-seen Lookout green is leftover `c3=(0,0.125,0)` × `mul_x2` × `oT1=(0,0)`, not a missing sampler | PROVEN | `Dx9VulkanShaderConstants.UnlitRgbIsC3Leftover`. Do not invent world UV. |
| `Q_NewOakValeIntro` / SHOT2 is not first no-save Present. First playable is Lookout `006B3FF0` | PROVEN | `004162B5` dump: +20 then +28. Frontend `00595222` is the resident-slot vtbl+8 walk (`7adf621` / `b4a2c89`). Present `0042DF9E` still Note-only. |
| `0042DF9E` walks `[ui+84]` vtbl+8, then `009D9C80`/`009DA9F0(1)` twice. `00404A80` is getter `0x13B7CD8` | PROVEN | `5657176` / `6607c1e` / `Frontend_0042EC7C_frame_is_input_then_0042DF9E_Present` (`FrontendFlushCount=2`, `FrontendDisplayHelperFn`, `FrontendDisplaySingletonVa`). Walks every resident `[ui+84]` slot (`b4a2c89` / `Frontend_tick_and_draw_walk_resident_ui84_slots`). Present `0042DF9E` still Note-only. New Game still keyboard N/Enter (leave #14 open). |
| PRESS_START is type 10 `0054E3D0` vtbl `012497E4`; draw `00530260` +176; `005339B0` +272=1; `UI_PRESS_START_TEXT` `TEXT_GUI_MENU_PRESS_BUTTON` | PROVEN | `Frontend_PRESS_START_is_type_10_with_text_child`. Type-0 `0041B800` / dest-always-0 / `0041AFA0` as the menu draw is DISPROVEN. `00595222` calls `[node+20].vtbl+8`. Types 5/10/12/18 are `00530260`. Attach no longer Finds `UI_PRESS_START_TEXT` after every root (`7adf621`). |
| Press Start sprites are `frontend.big` `FRONTEND_TITLE_01/02_SPRITE` + `FORREST_1_*` + mouse | PROVEN | decoded via `TextureFile` (Rgba8 framed LZO); title 256×128 gold logo; forest DXT1 oak grove |
| `frontend.bin` drives widget tree, dest, sprites and DX9-to-Vulkan submit | PROVEN | `0d77c2c`. `e79f7b4` drops unused usings from `FrontendWidgetFactory` (cleanup of `0d77c2c`). Type-10 draw is `00530260` vtbl+8 DrawsChildList (`7adf621`). Present `0042DF9E` still Note-only. Host LMB edge queues Type4 (`5dcc1fc`). Host queues LMB-up as type 6 (`48133e9`). leave #14 and #20 open. |
| Press Start dest inherits type-10 remap scale. Leftover `+204/+208` only when `GraphicIndex != 0`. leftover+204 GraphicIndex gate recovered (`76edbbd`): index 0 → leftover 0. dest still invented 512,384,512,384; tests lock that dest. Type-6 leftover 16×16 is gone; leftover204 is widget +204 (0 first-seen). Type-6 dest is a point at remapped origin (512,384,512,384), not ctor 0,0,0,0. `UI_TITLE_01` dest still from texture FrameWidth 256. Nonempty dest draw path is still `00BAE2D0` / `00BAD8A0`. `EnqueuesDisplayQueue` still false. No E8 `009DB700`. `FrontendEnqueueRan` still true on nonempty dest / PRESS START. Host Notes `009DA9F0(1) [+16020] empty` / skip DIP — stand-in, not a recovered queue read. `DisplayFlushShouldDip(0, 0)` always false | PROVEN / leftover #36 | `1a08cc0` / later `b1d6877` / `76edbbd` / `Type6_leftover204_is_widget_plus204_not_dest_width` / `Leftover204_is_0041AC20_graphic_index_not_persist_size` / `Press_Start_type6_dest_is_a_point` / `Frontend_PRESS_START_is_type_10_with_text_child` / `Press_Start_first_seen_dest_table_matches_0041AFA0` / `Frontend_0041AC20_dest_and_0xE5_new_profile_0x126_main_menu_15` / `Nonempty_dest_draws_via_00BAE2D0_not_009DB700`. leave #36 open. |
| Type-4 posts `0xE5` on Press Start. Host Return quarantined as accept. `0x126`/15 were still injected at this SHA. CUIDef persist `00631C60` +189/+190 u8s consumed so Absolute stays aligned. Type-6 Font 26051 is a names.bin offset that resolves to `ENG_ARIAL_24`, not the `0054F4B0` `ENG_ARIAL_16` helper | PROVEN | `db36334` |
| Persist CRC `0x53C644E4` as MessageId (def+224 via `0055B040`). Type-38 `UI_ACCEPT_NEW_PROFILE` stores `0x126`; type-11 `UI_FRONTEND_BUTTON_NEW_GAME` stores 15. Type 4 is `00A03C80` (+40=4), not a DIK. Action 26 posts stored id. Lifecycle no longer injects `0x126`/15 | PROVEN | `c180b8c` / `Type4_drives_lifecycle_0xE5_then_0x126_then_15`. Later `f38f9f9` moves MessageId to persist +228. |
| QST AddQuest TRUE as world+172 (FinalAlbion then GlobalQuests, not WLD `START_INITIAL_QUESTS`). Host LMB edge queues Type4. `DikPosterUnread=false` | PROVEN | `5dcc1fc` |
| MessageId is persist +228 / `0x53C644E4` (`00632500`), not +224 (`0x230364D6`). QM+44 (`004B2850` / `004B00C0`) gates activate before `00CB5AD0` | PROVEN | `f38f9f9` |
| Action 26 posts widget+372 from def+224 (0 first-seen on Accept/New Game). `0x53C644E4` at +228 still holds `0x126`/15. Widgets carry Plus224 | PROVEN | `9e94ef9` |
| Type 11/38 +228 via `0055ACF0`. Type-10 `0xE5` is only the `00598A1C` attach write (`00598EE6` is the mov inside it). Empty widget lists no longer invent `0xE5`. Press Start name-check invent gone. `0xE5` host fill gone | PROVEN | `61e430f` / `Frontend_press_start_type4_without_widgets_does_not_invent_0xE5`. #37 first half locked. |
| Type 11/38 +228 on LMB-up action 28 after action 26 arms. Type 4 still posts type-10 +352 only (not first-visible +228). Type 6 (`00A03D60`) is action 28 / vtbl+588 / `0055ACF0`. Host queues LMB-up as type 6. Unarmed +228 lists stay silent | PROVEN | `48133e9` / `Type4_action_26_posts_stored_widget_message` (ActionType4 accept == null) / `Type4_drives_lifecycle_0xE5_then_0x126_then_15` (Type4 then Type6). #37 second half locked. |
| Press Start `0xE5` host fill gone; Type4 posts type-10 +352 only; Type6 LMB-up action 28 posts +228 after arm. Unarmed lists silent. #37 already locked | PROVEN | `61e430f` / `Frontend_press_start_type4_without_widgets_does_not_invent_0xE5`. `48133e9` / `Type4_action_26_posts_stored_widget_message` / `Type4_drives_lifecycle_0xE5_then_0x126_then_15` |
| Draw type-10 menus via vtbl+8 DrawsChildList. `00595222` calls `[node+20].vtbl+8`. Types 5/10/12/18 are `00530260`. Attach no longer Finds `UI_PRESS_START_TEXT` after every root. `00598EE6` stays on the `00598A1C` slot-0x14 write | PROVEN | `7adf621`. Present `0042DF9E` still Note-only. |
| Write Press Start `0xE5` through slot `0x14` `0059B5D7`. Native attach looks up `[ui+84]` key `0x14` then vtbl+284 `0054E4F0`. Dropped leftover widgets[0] type-10 / MessageId==0 walk. Host still collapses `[packet+0]` onto MessageId (leftover field) | PROVEN | `59fde69` / `Frontend_attach_0xE5_is_slot_0x14_0059B5D7_not_type10_walk` |
| Keep `[ui+84]` slots `0x14` and `0x17` across menu switch. `00596763` switches current screen; it does not drop the map. `00595A06` overwrites existing key `0` with Main Menu. Stop AttachFrontendTree from clearing the slot map. Host current `_frontendWidgets` is still the switched screen (input leftover) | PROVEN | `84a8350` / `Frontend_ui84_keeps_slot_0x14_and_0x17_after_main_menu` |
| Walk every resident `[ui+84]` slot on tick and draw. `00595222` vtbl+8 and `0059A0C4` vtbl+4 are the same in-order map walk; they do not filter to the current screen. Keep `_frontendWidgets` as the switched tree for input | PROVEN | `b4a2c89` / `Frontend_tick_and_draw_walk_resident_ui84_slots` (`FrontendResidentSlots`) |
| Write `+332` via SelectState(6) on slot switch; construct world in Init World. `0052CF40` stores vtbl+192 arg at `+332`. `004A67D0`/`004A6E30` belong inside `"Init World"` `0041735A`, before `"Init Display Engine"` `00417418`. CreatePlayers runs at the named `004166A8` stage | PROVEN | `b8a2b21` / `Frontend_attach_0xE5_is_slot_0x14_0059B5D7_not_type10_walk` (`kept.State == 6`, Note `+332=6`) / `Init_World_004A67D0_runs_inside_0041735A_before_00417418` |
| `+332` SelectState(6) is a `+302` hide | DISPROVEN | `0052CF40` stores vtbl+192 arg at `+332` (`b8a2b21`) |
| Run Init Fonts after Graphics and select +332=5 on the new current. `004168DC` looks up `ENG_ARIAL_18` via `009E2C80` and stores it at `game+90444`. That is not frontend type-6 `ENG_ARIAL_16`/`ENG_ARIAL_24`. `0059A119` applies vtbl+192(5) to the incoming slot after the old current got vtbl+192(6) | PROVEN | `9901d3b` / `Init_Fonts_004168DC_stores_ENG_ARIAL_18_at_game_plus90444` |
| Ensure the 0xE0 `[0x13B879C]` singleton before Thing Components. `0044C6B6` is a present-check; first-seen miss constructs `0044C6C2` / `0044C71F`. Init frontend `005952C3` applies vtbl+192(5) to Press Start | PROVEN | `6ae001f` / `Init_Game_0044C6B6_ensures_0xE0_singleton_before_Thing_Components` |
| `0044C6B0` is the 0xE0 singleton ctor | DISPROVEN | `0044C6B0` remains the later getter (`6ae001f`) |
| Register `Data\Defs\misc_def_types.h` into `[0x13B8A54]` at Init Subtitled. `004CDB10` via `00A39010`. Not Speak. `00A38E50` payload UNREAD | PROVEN | `540e30c` / `Init_Subtitled_004CDB10_registers_00A39010_at_13B8A54` |
| Bind STANDARD_TALK and CONVERSATION names at Init Conversation Attitude. `004CD670` via `0099EFE0` (18/12/12). Not Speak. Later Thing Components Notes Add Def Class for CHeroMorphDef then CHighlightItemDef / CSmokeGeneratorDef / CTimeAppearanceFadeDef / CCreatureNavigationDef / CInventoryItemDef / CLookDef / CReadableDef / CVillageDef / CVillageMemberDef / CBuyableHouseDef / CBuyHouseDef / CWifeDef / CDoorDef / CLightDef / CSpotLightDef / CClockDef / CHeroDef; MATCH is Note-only + `*DefClassRegistered` flag, not a live constructed object | PROVEN | `fe6c09c` / `Init_Conversation_004CD670_binds_STANDARD_TALK_tables` |
| Store the 44-byte player owner at `game+28` during Init Player Manager. `0041732A`: `00BFEA1A(44)` / `0044C6B0` / `0044A3B0` vtbl `01231CD0` size 44 / `004193A0` `[game+28]`. Not Create Players (`004166A8`). Hero swap names `hero_swap_1.tng`…`_4.tng` belong on this owner | PROVEN | `4a03969` / `Init_Player_Manager_0041732A_stores_44byte_owner_at_game_plus28` |
| Host ctor note of `0044A3B0` under Init Player Interface | DISPROVEN | moved to Init Player Manager (`4a03969`) |
| `+24=0` write on the player owner | DISPROVEN | `4a03969` / same owner test |
| Add CHeroMorphDef against the live +40 cap during Init Thing Components. `004EE337` / `0044C6B0` / `009B0AC0` / factory `004E4219` / `009AD6E0` / `009FC4F0` `[this+40]=0x80000`. Host Note-only + `FirstDefClassRegistered`. MATCH is Notes+flag, not a live object. LoadDef field walk stays PARTIAL. Do not invent `00A38E50` | PROVEN | `b7f4c34` / `Init_Thing_Components_004EE23F_adds_CHeroMorphDef_against_plus40` |
| Prepare the definition manager via vtbl+8 `0044C72B` and `009ACB10`. Host `PrepareDefinitionManager()` Notes `0044C6B0` / `0044C72B [vtbl+8]` / `009ACB10` / `009E5250` and sets `DefinitionManagerPrepared`. dest `0044C72B` as `[01232C24+8]` is not rdata-locked (sibling `proofs/00416005-def-manager` dest UNREAD). Leave #42 open. Do not call dest PROVEN. Not a `game.bin` parse. Different object from later Subtitled `[0x13B8A54]` | PROVEN | `587baae` / `Init_Definition_Manager_00416005_resets_plus88_via_vtbl8` |
| Add CHighlightItemDef after CHeroMorphDef during Init Thing Components. Site `004EE565` / dest `009B0AC0` / factory `004D8671` size 72 vtbl `0123BD14`. Note-only + `SecondDefClassRegistered`. MATCH is Notes+flag, not a 72-byte object | PROVEN | `acfe46f` / `Init_Thing_Components_004EE565_adds_CHighlightItemDef` |
| Add CSmokeGeneratorDef after CHighlightItemDef. Site `004EE62B` (`0044C6B0`), dest `009B0AC0` at `004EE632`, factory `004DA82B` size 48 vtbl `0123E924`, name `CSmokeGeneratorDef`. Host Note-only + `ThirdDefClassRegistered`. MATCH is Notes+flag, not a live 48-byte object | PROVEN | `100e5cf` / `Init_Thing_Components_004EE632_adds_CSmokeGeneratorDef` |
| `CTCSmokeGenerator` `004EE588` is the next `009B0AC0` / Add Def Class | DISPROVEN | CTC row, not Add Def Class (`100e5cf` / `Init_Thing_Components_004EE632_adds_CSmokeGeneratorDef`) |
| Add CTimeAppearanceFadeDef after CSmokeGeneratorDef. Site `004EE6FD` (`0044C6B0`), dest `009B0AC0` at `004EE704`, factory `004D84C8` size 56 vtbl `0123B7CC`, name `CTimeAppearanceFadeDef`. Host Note-only + `FourthDefClassRegistered`. MATCH is Notes+flag, not a live 56-byte object | PROVEN | `1a4c51d` / `Init_Thing_Components_004EE704_adds_CTimeAppearanceFadeDef` |
| `CTCTimeAppearanceFade` `004EE64E` is the next `009B0AC0` / Add Def Class | DISPROVEN | CTC row, not Add Def Class (`1a4c51d` / `Init_Thing_Components_004EE704_adds_CTimeAppearanceFadeDef`) |
| Add CCreatureNavigationDef after CTimeAppearanceFadeDef. Site `004EE92B` / dest `009B0AC0` at `004EE932` / factory `004DA871` size 56 vtbl `0123E98C`. Host Note-only + `FifthDefClassRegistered`. MATCH is Notes+flag, not a live 56-byte object | PROVEN | `ee08490` / `Init_Thing_Components_004EE932_adds_CCreatureNavigationDef` |
| CTC physics/nav rows (`CTCPhysicsLight` / `CTCPhysicsStandard` / `CTCPhysicsControlled` / `CTCCreatureNavigation`) are the next `009B0AC0` / Add Def Class | DISPROVEN | CTC rows, not Add Def Class (`ee08490` / `Init_Thing_Components_004EE932_adds_CCreatureNavigationDef`) |
| Add CInventoryItemDef after CCreatureNavigationDef. Site `004EF23D` / dest `009B0AC0` at `004EF244` / factory `0044F644` jmp `0044C108` size 112 vtbl `01231DBC`. Host Note-only + `SixthDefClassRegistered`. MATCH is Notes+flag, not a live 112-byte object. Also started `proofs/github-issues-verify` (index only at that SHA). Do not close issues from Note-only | PROVEN | `3a7b594` / `Init_Thing_Components_004EF244_adds_CInventoryItemDef` |
| Add CLookDef after CInventoryItemDef. Site `004EF37F` / dest `009B0AC0` at `004EF386` / factory `004D80E4` ctor `0044C0C0` size 88 vtbl `0123AE14`. Host Note-only + `SeventhDefClassRegistered`. MATCH is Notes+flag, not a live 88-byte object. Type-6 leftover 16×16 is gone; leftover204 is widget +204 (0 first-seen). Type-6 dest is a point at remapped origin (512,384), not ctor 0,0,0,0. `UI_TITLE_01` dest still from texture FrameWidth 256. Host Notes `009DA9F0(1) [+16020] empty` / skip DIP: `DisplayFlushShouldDip(0, 0)` is always false; host never stores `[this+16020]`. That empty Note is stand-in, not a recovered queue read. `FrontendEnqueueRan` still true on nonempty dest / PRESS START. `EnqueuesDisplayQueue` still false. No E8 `009DB700`. Draw path is still `00BAE2D0` / `00BAD8A0`. Leave #36 open | PROVEN | `b1d6877` / `Init_Thing_Components_004EF386_adds_CLookDef` / `Type6_leftover204_is_widget_plus204_not_dest_width` / `Press_Start_type6_dest_is_a_point` / `Frontend_PRESS_START_is_type_10_with_text_child` / `Press_Start_first_seen_dest_table_matches_0041AFA0` / `Nonempty_dest_draws_via_00BAE2D0_not_009DB700` |
| `CTCCreatureExpression` / `CTCLook` are the next `009B0AC0` / Add Def Class | DISPROVEN | CTC rows, not Add Def Class (`b1d6877` / `Init_Thing_Components_004EF386_adds_CLookDef`) |
| Add CReadableDef after CLookDef. Site `004EF5AD` / dest `009B0AC0` at `004EF5B4` / factory `004DAA0E` ctor `0044C0C0` size 38 vtbl `0123E9F4`. Host Note-only + `EighthDefClassRegistered`. MATCH is Notes+flag, not a live object. Size 38 is thin (no factory dump) | PROVEN | `113a514` / `Init_Thing_Components_004EF5B4_adds_CReadableDef` |
| `CTCActionUseTorch` / `CTCActionUseReadable` are the next `009B0AC0` / Add Def Class | DISPROVEN | CTC rows, not Add Def Class (`113a514` / `Init_Thing_Components_004EF5B4_adds_CReadableDef`) |
| Type-6 host Notes `00543910` / `0x27` size 64; add CVillageDef after CReadableDef. Site `004F0171` / dest `009B0AC0` at `004F0178` / factory `004E213B` pack `0042DAE0` ctor `004DFF04` size `0x10C` vtbl `01241DDC`. Host Note-only + `NinthDefClassRegistered`. MATCH is Notes+flag, not a live object. Type-6 `0x27` is the host packer Note vs old `0x22` host pack (`Type6_widget_packs_00543910_type_27_not_0041BEB0`). Last present packer stays `0041BEB0` / `0x22`. `EnqueuesDisplayQueue` still false. No native pack into +16020. Host still Notes `009DA9F0(1) [+16020] empty`. Leave #36 open | PROVEN | `a141c27` / `Init_Thing_Components_004F0178_adds_CVillageDef` / `Type6_widget_packs_00543910_type_27_not_0041BEB0` |
| `CTCActionUseSearch` is the next `009B0AC0` / Add Def Class | DISPROVEN | CTC row, not Add Def Class (`a141c27` / `Init_Thing_Components_004F0178_adds_CVillageDef`) |
| Recover 0041AC20 leftover +204 from GraphicIndex (index 0 → leftover 0); dest still invented 512,384,512,384 through 0xE5 leave; add CVillageMemberDef after CVillageDef. Site `004F0227` / dest `009B0AC0` at `004F022E` / factory `004DA7AD` ctor `0044C0C0` size 38 vtbl `0123E854`. Host Note-only + `TenthDefClassRegistered`. MATCH is Notes+flag, not a live object. DIP still `DisplayFlushShouldDip(0, 0)` always-empty stand-in. Leave #36 open | PROVEN | `76edbbd` / `Init_Thing_Components_004F022E_adds_CVillageMemberDef` / `Leftover204_is_0041AC20_graphic_index_not_persist_size` / `Frontend_0041AC20_dest_and_0xE5_new_profile_0x126_main_menu_15` |
| Add CBuyableHouseDef after CVillageMemberDef. Site `004F02DD` / dest `009B0AC0` at `004F02E4` / factory `004E0148` ctor `004DDB2C` size 76 vtbl `0124131C`. Host Note-only + `EleventhDefClassRegistered`. MATCH is Notes+flag, not a live object. `proofs/frontend-no-screen-draw-fork` is a host grep labeled PROVEN, not a native dest lock. Quiet leftover; do not file a new issue | PROVEN | `91564bd` / `Init_Thing_Components_004F02E4_adds_CBuyableHouseDef` |
| Dump three frontend screens after AVI skip; add CBuyHouseDef after CBuyableHouseDef. Site `004F0393` / dest `009B0AC0` at `004F039A` / factory `004D7B5B` ctor `0044C0C0` size 38 vtbl `0123A61C`. Host Note-only + `TwelfthDefClassRegistered`. MATCH is Notes+flag, not a live object. Optional `FABLE_SKIP_STARTUP_AVI` is host `FinishStartupVideo`, not native DIK skip. Screen dumps re-lock invented dest. `proofs/frontend-screens-vs-native` dest MATCH oversell restates #36. Leave #36 open. Do not treat the AVI skip env as a 3D Draw fix (leave #20 open) | PROVEN | `f30c099` / `Init_Thing_Components_004F039A_adds_CBuyHouseDef` / `Frontend_dumps_press_start_new_profile_main_menu_after_avi_skip` |
| Present type-16/18 persist child 0; add CWifeDef after CBuyHouseDef. Site `004F04B4` / dest `009B0AC0` at `004F04BB` / factory `004D7BA1` ctor `0044C0C0` size 44 vtbl `0123A69C`. Host Note-only + `ThirteenthDefClassRegistered`. MATCH is Notes+flag, not a live object. Type 16 CTextSlider vtbl+8 is `00530260`. First-seen +348=0 keeps ARROWS/NORMAL. Type-6 dest stays a POINT (512,384,512,384). At this SHA host `DrawContainerWalk` / `IsPresented` exclusive-walked `kids[ActiveChild]` for SelectsChild. Proof `proofs/type16-18-present-child` DISPROVES `00530260` exclusive-walk (native walks every +176 child, skip vtbl+400 / +420). Unused `ApplySelectState` maps one state onto all type 16/18. Filed issue #46. Later `88a9ab8` dropped exclusive-walk; native skip vtbl+400 (+504) still unused. Leave #46 open. Dest still invented. Leave #36 open | PROVEN | `71ae66e` / `Init_Thing_Components_004F04BB_adds_CWifeDef` / `Factory_builds_press_start_then_main_menu_from_the_same_walk` |
| Present skip from listing (constants + proof only); add CDoorDef after CWifeDef. Site `004F0640` / dest `009B0AC0` at `004F0647` / factory `004D7BE7` ctor `0044C0C0` size 60 vtbl `0123A714`. Host Note-only + `FourteenthDefClassRegistered`. MATCH is Notes+flag, not a live object. Test `Init_Thing_Components_004F0647_adds_CDoorDef`. `405b1e8` also fixes the thirteenth early-return (`if (!ThirteenthDefClassRegistered)` then fourteenth) so CWifeDef and CDoorDef both register — a real host fix. Shape-2 pack `0042DAE0`. `FrontendWidgetType` constants: `BorrowedVisibleFn` `0052F180` (`[+300]>>7`, persist def+504), `ClipBitFn` `0052F1D0` (`[+302]&1`, persist def+392, CRC UNREAD), `ForwardSelectFn` `0041C5A0`, `TextSliderIndexOffset` 348. Those VAs are never read by `DrawContainerWalk` or `IsPresented`. At this SHA host exclusive-walked `kids[ActiveChild]` for SelectsChild. `Frontend_dumps_press_start_new_profile_main_menu_after_avi_skip` now asserts `FrontendResidentSlots` keep 0 / `0x14` / `0x17`. Proof `proofs/listing-present-skip`: `00530260` walks every +176 then +188; skip is vtbl+400 / vtbl+420. Native `00530260` does NOT exclusive-walk. Later `88a9ab8` dropped exclusive-walk / sibling `Visible=false`; native skip vtbl+400 (+504) still unused. Do not treat skip as recovered. Leave #46 open. Dest still invented 512,384,512,384. Leave #36 open. Next leftover (not an issue): CLightDef `004F06F6` / `004D7C73` size 92 vtbl `0123A814` | PROVEN | `405b1e8` / `Init_Thing_Components_004F0647_adds_CDoorDef` / `Factory_builds_press_start_then_main_menu_from_the_same_walk` / `Frontend_dumps_press_start_new_profile_main_menu_after_avi_skip` |
| Forward SelectState +332 via `0041C5A0`; add CLightDef after CDoorDef. Site `004F06F6` / dest `009B0AC0` at `004F06FD` / factory `004D7C73` / ctor `0044C0C0` / size 92 / vtbl `0123A814`. Host Note-only + `FifteenthDefClassRegistered`. MATCH is Note-only + `*DefClassRegistered` flag, not a live constructed object. `SelectFrontendState` now `ForwardSelectState`: writes +332 and Notes `0041C5A0` to persist +176 children. Not ActiveChild, not Visible=false, not current-slot-only draw. Tests lock CLightDef Notes plus leftover Press Start State=6 / `UI_PRESS_START_TEXT` Visible / `FORREST_1_1` Visible / `0041C5A0` Note. Later `88a9ab8` dropped exclusive-walk; native skip vtbl+400 (+504) still unused. Leave #46 open. Dest still invented. Leave #36 open | PROVEN | `46663e3` / `Init_Thing_Components_004F06FD_adds_CLightDef` / `Factory_builds_press_start_then_main_menu_from_the_same_walk` / `Frontend_dumps_press_start_new_profile_main_menu_after_avi_skip` |
| Skip leaf DIP when packed alpha is 0; add CSpotLightDef after CLightDef. Site `004F07AC` / dest `009B0AC0` / factory `004D7CB9` / ctor `0044C0C0` / size 68 / vtbl `0123A88C`. Host Note-only + `SixteenthDefClassRegistered`. MATCH is Note-only + `*DefClassRegistered` flag, not a live constructed object. `0041AFA0` and `0054EF00` skip `0041BEB0` when +151==0. Dest-zero already skips sprites. Not current-slot-only draw. `LeafDipSkipped` is a real `CompositeFrontendPresent` gate, but `PackPersistColour(0,0,0,0)` returns `0xFFFFFFFF` so unread/ctor colour never skips. Tests lock CSpotLightDef Notes and `LeafPresentFn` `0041AFA0` / `Type6PresentFn` `0054EF00` / `LeafDipSkipped` helper, not leftover-slot actual +151. Style-6 persist flags remain UNREAD and are not applied. Later `88a9ab8` dropped exclusive-walk; native skip vtbl+400 (+504) still unused. Do not file style-6 UNREAD. Leave #46 #36 open | PROVEN | `db3899a` / `Init_Thing_Components_004F07B3_adds_CSpotLightDef` / `Factory_builds_press_start_then_main_menu_from_the_same_walk` |
| Add CClockDef after CSpotLightDef; name `0052C7E0` style-tick flags. Site `004F0862` / dest `009B0AC0` / factory `004E4477` / ctor `004E380E` / size 56 / vtbl `01242C34`. Host Note-only + `SeventeenthDefClassRegistered`. MATCH is Note-only + `*DefClassRegistered` flag, not a live constructed object. Style-tick dword0 bits `0x10`/`0x20`/`0x40` are named from `StyleTickFn` `0x0052C7E0`. They are **not applied**. Persist style-6 dword0 remains UNREAD. `StyleFlagsZeroDest` comment misnames native `0x20` (zeros +76/+80 parent-local, not dest) — do not file that comment as a leftover. Later `88a9ab8` dropped exclusive-walk; dest still invented. Native skip vtbl+400 (+504) still unused. Leave #46 #36 open | PROVEN | `065eb28` / `Init_Thing_Components_004F0869_adds_CClockDef` / `Factory_builds_press_start_then_main_menu_from_the_same_walk` |
| Add CHeroDef after CClockDef; name `0052CEB0` style lookup +20. Site `004F0918` / dest `009B0AC0` / factory `004D7CFF` / ctor `0044C0C0` / size 48 / vtbl `0123A904`. Host Note-only + `EighteenthDefClassRegistered`. MATCH is Note-only + `*DefClassRegistered` flag, not a live constructed object. `00631C60` persist of CUIDef+64 is `00632E00` stride 124; `00433FE0` starts at style +60. `StyleLookupFn` `0x0052CEB0` returns map node +20. Named only, not applied. Persist style-6 dword0 is UNREAD and is not applied. Next leftover CCreatureModeDef `004F0D26` / `004E0B4B` is not shipped — do not file. Later `88a9ab8` dropped exclusive-walk; dest still invented. Native skip vtbl+400 (+504) still unused. Leave #46 #36 open | PROVEN | `6577614` / `Init_Thing_Components_004F091F_adds_CHeroDef` / `Factory_builds_press_start_then_main_menu_from_the_same_walk` |
| Recover New Profile frontend layout and hit-test from exe. **Not** an Add-Def-Class MATCH. This is a recover commit. Persist parse of type-12 `UI_NEW_PROFILE_MENU` `+326=30` (CRC `0xD7495328`) and Sprites `(key, defIndex)` / `Plus96` CRC `0x38BB7ED4` is real file recover. Layout writes authored Y `index * 30` into `PersistY`. Type-2 leftover `+204/+208` claimed as persist W/H (`00551340`); `UI_BUTTON_OPTIONS_LEFT` leftover (180, 0). `PlaceTableCell` claimed `00551EA0` when +96 bit 0; `count==3` invents left/right leftover W and a middle fill. `ExpandTableDests` sets table DestY1 from children when dest height is 0. Hit-test: new `FrontendHitTest` claimed `0055B8F0` / `0055BF10`; dest AABB if area else union of presented descendants; smallest-area reverse-walk. Type6 maps `MessageFromPlus228List`. Empty space does not Accept. Client maps LMB to dest space (fb / BackBuffer, else 1024×768). Enter still queues TypeKey (AVI skip), not New Game. No `Key.N` / `ActivateNewGame` in Program.cs. Press Start dest table **unchanged**: type-6 still remapped point `512,384,512,384`. DIP path not touched (`DisplayFlushShouldDip(0,0)` still false; no `[this+16020]`). Later `88a9ab8` dropped exclusive-walk (`IsPresented` no longer `kids[ActiveChild]`). Native skip vtbl+400 (+504) still unused. Style-6 still UNREAD. Tests do **not** lock dest numbers: they assert ≥4 distinct live DestY0, Apply/Cancel hits disjoint, empty-space no Accept, Apply `ClickNamed` → Main Menu, tautology VAs. Oversell: “from exe” pins `00551EA0` / `0055B8F0` / `00551340` onto host heuristics. Filed #48. Leave #14 #20 #36 #42 #46 #48 open. leave #14 open on `ClickNamed` | PROVEN | `b075dd3` / `New_Profile_type12_rows_use_persist_plus326_not_equal_Y` / `New_Profile_apply_cancel_hit_rects_are_disjoint` / `New_Profile_empty_space_Type4_Type6_does_not_accept` / `New_Profile_Apply_hit_posts_0x126_Cancel_does_not` |
| Pack type-12 rows so persist X=-100 is not dest X=-45. Persist `Plus322` CRC `0xA04E63BE`. `ListChildAuthoredPos` packs children; persist X=-100 discarded. Stopped writing packed Y back onto `PersistY` (partial #48 item 4). Added `ExpandControlDests` (type-16/37 dest grown from rightmost type-2 — invented dest). Tests lock host pack formula and “has area”, not dest numbers. Press Start dest table **unchanged**: type-6 still remapped point `512,384,512,384`. DIP `(0,0)` stand-in. Later `88a9ab8` dropped exclusive-walk; native skip vtbl+400 (+504) still unused. Style-6 still named-only. #48 stays open. Leave #14 #20 #36 #42 #46 #48 open | PROVEN | `43b1cf2` / `New_Profile_type12_rows_use_persist_plus326_not_equal_Y` |
| Hit New Profile widgets from dest, not leftover unions. `UnionDescendants` deleted. `HitRect` = dest AABB only. Point dests miss. Dest writers unchanged (`ExpandControlDests`, `PlaceTableCell` n==3, `ExpandTableDests`). #48 stays open (union stop is item 2 only). Press Start dest table unchanged. DIP `(0,0)` stand-in. Later `88a9ab8` dropped exclusive-walk; native skip vtbl+400 (+504) still unused. Leave #14 #20 #36 #42 #46 #48 open | PROVEN | `6e76ac5` / `New_Profile_per_control_LMB_uses_dest_not_empty_space` / `New_Profile_apply_cancel_hit_rects_are_disjoint` |
| Tile type-2 leftover cells and keep type-16 dest a point. **`ExpandControlDests` deleted**. Type-16/37 dest is a point again (honest undo of `43b1cf2` size invent). `PlaceTableCell` n==3 fill is NOT gone — first/last sibling leftover W as caps. `BindEditBoxSeed` / `SpritesThenGlyphs` claimed `00851770` / `0054EF00` — oversell. Tests: slider/edit dest is a **point**; live cells tile-the-bar, not dest tuples. #5 grok-goal dump still written — do not re-file. #48 stays open. Press Start dest table unchanged. DIP `(0,0)` stand-in. Later `88a9ab8` dropped exclusive-walk; native skip vtbl+400 (+504) still unused. Leave #14 #20 #36 #42 #46 #48 open | PROVEN | `68eb1c5` / `New_Profile_persist_child_order_and_layers` / `New_Profile_type12_rows_use_persist_plus326_not_equal_Y` |
| Pair type-2 sprite keys as left/right caps and stretch. Persist Sprites sorted by key → clone order L, R, M. `SpriteKeys == [0,1,4]` MATCH persist. `PlaceTableCell` index 1 is now right cap; n==3 leftover fill still invented. **`TryChromeHit`:** if dest is a point, hit = dest origin + rightmost type-2 table dest size. Claimed `0055B8F0`. **Regresses** `6e76ac5` “hit dest AABB only” for type-16/37. `SpritesThenGlyphs` now per `[ui+84]` tree (`0054EF00` oversell). Tests lock tiling / nonempty chrome hit / click `(700,300)`, not dest numbers. Persist leftover LEFT still `(180, 0)`; helper uses 288×32. #48 stays open. Press Start dest table unchanged. DIP `(0,0)` stand-in. Later `88a9ab8` dropped exclusive-walk; native skip vtbl+400 (+504) still unused. Leave #14 #20 #36 #42 #46 #48 open | PROVEN | `069c6a9` / `New_Profile_persist_child_order_and_layers` / `New_Profile_apply_cancel_hit_rects_are_disjoint` |
| New Game TNG load + menu cap slice. **TNG:** `LoadGlobalThingsFile` now `break`s on the first `LoadedOnPlayerProximity` map, Notes `LoadGlobalThingsPerMap` (`004FDBC0`) as `"004FBF60 " + lookout.ScriptName + ".tng"`, loads that one TNG, sets `GlobalThingMapsLoaded = 1`. Does **not** look up `map.Index == 1` or name `LookoutPoint`; takes first proximity map in WLD parse order. Comment claims `004FDBC0 ebx=1` skips dummy slot 0 and first `004FBF60` is LookoutPoint (NewMap 1); later maps stay closed until `00501450` / `ContainsMap`. Real reason in code: parsing every proximity `.tng` OOMs the New Game pump. **Menu slice:** Type-2 `PlaceTableCell` only when `persistW == 0 && persistH == 0`. Persist-size kids (mouse-area) skip the table helper and fall through to generic dest / inherit-scale `005339B0`. `PlaceTableCell` body **unchanged** from `069c6a9` (`count==3` host L/R/mid fill claimed as `00551EA0`). `AttachSpriteCells` still attaches every `SpriteDefIndices` entry. Tests: `New_Game_004FDBC0_opens_LookoutPoint_only` locks `GlobalThingMapsLoaded == 1` + LookoutPoint in the Note, no Bowerstone Note. Does **not** lock NewMap index, `ebx=1`, `004FBF60` callee, or `00501450`. `Main_Menu_button_slices_are_sprite_keys_not_mouse_area` locks `SpriteKeys == [0,1,4]`, mouse-area persist W>0 / alpha 0, kids[1..3] tile the table dest. Host dest tiling, no native dest tuple. Filed leftover **#50**: host first-proximity TNG / OOM workaround sold as recovered `004FDBC0` NewMap 1. Do not fold #50 into #4 (ledgers vs TNG pump). Persist-size skip is #48 family, not a new leftover. Press Start dest table unchanged. DIP `(0,0)` stand-in. Later `88a9ab8` dropped exclusive-walk; native skip vtbl+400 (+504) still unused. `CompositeFrontendPresent` still `BuildBatch(..., 0, 0, width, height)`. Leave #14 #20 #36 #42 #46 #48 #50 open | PROVEN | `d628952` / `New_Game_004FDBC0_opens_LookoutPoint_only` / `Main_Menu_button_slices_are_sprite_keys_not_mouse_area` |
| Recover 0055BF10 hover so Accept LMB posts 0x126. Host HitIndex + Hovered. `HoverSelectFn == 0x0055BF10` is a tautology. No +352 byte. Later rewritten by `e3208eb`. dest still invented. DIP `(0,0)` stand-in. leave #14 #20 #36 #42 #46 #48 #50 open | PROVEN | `681620d` / `New_Profile_hover_0055BF10_swaps_type38_on_off` |
| Present frontend as 0042DF9E 2D only; skip State-6 DIP. Host gate `_frontendReady && !playAviOnly` skips mesh/gizmos in `VulkanLineRenderer.Record`. `window.Render` still always `host.Draw`. **Not** a recovered 0042DF9E walk (no BeginScene / `00595222` / EndScene / `009BEEB0`). `CompositeFrontendPresent` State-6 tree skip was host and was REMOVED in `e3208eb`. Present `0042DF9E` still Note. `DisplayFlushShouldDip(0,0)` still false. dest still 512,384,512,384. New Game still not locked (Enter is PlayAVI skip). leave #14 open. PlayAVI still `host.Draw`. leave #20 open. Leave #14 #20 #36 #42 #46 #48 #50 open | PROVEN | `dd0a0f0` |
| Dump type 11/38 vtbls; drop leftover hover hacks. `DrawsChildList` now includes type 11 and 38 (comment dumps `01249554+8` / `0124B04C+8` as `00530260`). `Table[11].Vtbl` is still 0; dump is comment-only. Tests lock `ContainerDrawFn == 0x00530260` tautology. Hover is host Contains\|\|HitIndex on type 11/38 when TypeMouse is in Applied. Hovered stand-in for +352 on Arm. Dropped `TryMouseAreaDest` dest copy, ON/OFF colour/texture swap, type-32 pointer dest, State-6 present skip. Test `New_Profile_hover_0055BF10_swaps_type38_on_off` no longer checks ON/OFF colour swap. dest still a point. leave #14 #20 #36 #42 #46 #48 #50 open | PROVEN | `e3208eb` / `New_Profile_hover_0055BF10_swaps_type38_on_off` |
| Parse persist clip/align/style ColourA and drop leftover sibling hide. Wire persist +392 Clip (vtbl+420), +504 CRC parsed but NOT applied to vtbl+400 skip, +508 type-6 align (tests lock arrows `Plus508=1` and Flag302 centre bit). `00530260` walks every child: `DrawContainerWalk` / `IsPresented` no longer exclusive-walk `kids[ActiveChild]`; `ApplyFirstSeenState` no longer `Visible=false` on siblings. Type-18 inactive forest tiles now Visible dest 410x410 opaque (dump batchDraws 193→223). Sampler D3D POINT locked (`Dx9VulkanParityTests` `Sampler_and_blend_translations` MagFilter Nearest). `StyleFlagsZeroDest` 0x20 still unused. dest still invented. Native skip via vtbl+400 (+504) still unused; type-18 inactive siblings present. leave #46 open. Leave #14 #20 #36 #42 #46 #48 #50 open | PROVEN | `88a9ab8` / `Factory_builds_press_start_then_main_menu_from_the_same_walk` / `Sampler_and_blend_translations` |
| First-seen colour is persist style 0 (`005339B0` packs style 0 into +132). Type-16 child 0 `StyleIndex=3` because vtbl+176 `0041C5C0` is style-exists; present colour stays style 0. Unselected siblings stay `StyleIndex` 0. `ChildrenOf` by parent index, not name (duplicate persist names no longer steal `DrawContainerWalk` / `ForwardSelectState`). Dropped `StyleFlagsForceOpaque` pack. Tests: `Factory_builds_press_start_then_main_menu_from_the_same_walk` locks arrows Visible + `StyleIndex` `TextSliderFirstSeenSelect` (3), wasd Visible + `StyleIndex` `FirstSeenState` (0), `arrows.Colour == wasd.Colour`, both `LeafDipSkipped`. `EngineLifecycleTests.Frontend_dumps_press_start_new_profile_main_menu_after_avi_skip` locks the same `StyleIndex` + `LeafDipSkipped` on ARROWS/WASD. VA tautologies `StyleExistsFn` `0x0041C5C0` and `InheritPackedColourFn` `0x0052E930`. dest still invented 512,384,512,384; DIP `(0,0)`; `BuildBatch(..., 0, 0, width, height)`. Present `0042DF9E` still Note. Native skip via vtbl+400 (+504) still unused. leave #14 #20 #36 #42 #46 #48 #50 open | PROVEN | `b216990` / `Factory_builds_press_start_then_main_menu_from_the_same_walk` / `Frontend_dumps_press_start_new_profile_main_menu_after_avi_skip` |
| `[node+20]` is `0041DB1D`/`0041D21B` type0 `0041B800` vtbl `0122F5D4`; draw `0041AFA0` not `0052D900` | DISPROVEN | PRESS_START Type=10. Type 0 is `UI_FRONTEND_BUTTON`. |
| `00A09F20` miss: `[bank].vtbl+4` is `009D56C0` Open Bank File Async then `009A7F80` on `[0x13CA79C]` | PROVEN | `bbee903` / `Pe_entry_is_crt_not_new_game` / `Install_banks_and_startup_videos_exist` (`MeshBank.OpenVtbl4`) |
| `00404C00` first-seen `[0x13B7CD8+8]==0` skip; `0041AFA0` packs `0041BEB0` type `0x22` (not sibling `0041BF60`) dest `[edx+92]` `this+0x15C` size `0xC0` | PROVEN | `c612ad5` / same frontend test (`Frontend2dLastPacker=0041BEB0`, `FrontendDisplayFlag=false`). Type-0 stays `0041BEB0` / `0x22`; type-6 host Notes `00543910` / `0x27` (`a141c27` / `Type6_widget_packs_00543910_type_27_not_0041BEB0`). Dest leftover `+204` is widget +204 (`b1d6877`). leave #14 open. |
| `00418289` dump: `00416296`/`00490A22` frontend+GUI gate; `009E1BC0` → `[game+90544]`; fade/player `004AEBA0`; world `0049D9E0`; vtbl+24 `00416E78`; `0041726D` WorldFrame. START_INITIAL_QUESTS factories `00CDE2F0`/`00F01760`/`00CDBD20`/`00CB8690` (vtbls `012C3000`/`012F72D0`). `006B3FF0` seed PROVEN (208); `006B63C0` bank copy 6×`0x1F4` PROVEN (91). Pose leftover `006B8640`/`008889C0` stay UNREAD; `006B2CA0` later PROVEN (`204a214`). Host `SeedAt(1.6m)` later DISPROVEN as live New Game (`204a214`). FOV 72 is SHOT2 leftover; Lookout helper FOV later 70 (`be3339e`). Do not StartCutscene(S_PSM) from the factory ctor. | PROVEN | `18ef09b` / [FORWARD_TREE.md](../runtime/FORWARD_TREE.md) §11 |
| `006B3FF0` +68 → `006B2CA0` pose (not invented `SeedAt` 1.6 m eye). First-seen V2/V3 `(1,0,0)`, V4 `(-1,0,0)`. `00A14440` normalize. Host `SeedAt(1.6m)` is not the New Game path | PROVEN | `204a214` / `World_camera_006B4900_slots_lerp_into_ScriptedCamera` / `Camera_004164E0_runs_on_install_after_WorldFrame` / `Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint` (`PoseComputed`, `WorldCamera.PoseFn`). Do not reopen #6 |
| `004A5A40` type-1 tick pumps `004B4490` `[0x13B89FC]` → `00CB8220` / `00CB7C40` / `00CB8170`. First-seen `QuestVtbl24Calls=0` (`+41==0` takes vtbl+4 start). `006B3030` V0 spring: Weight0 stays ctor 0.2; V0 stays `(1,0,0)` (no invented V0). `004978A0` LCG seed UNREAD; `00A14260` yaw/pitch not applied first-seen | PROVEN | `52e26bc` / `Camera_004164E0_runs_on_install_after_WorldFrame` (`QuestPumpRan`, `QuestPumpWalked>=6`, `FollowSpringRan`, `SubjectFillNoted`). FORWARD_TREE §11 |
| First-seen `006B3B80` skip (`+460=0`, qword +24 = −1.0 from `[0x1236700]`, fcomp `[0x122ED70]=0` → ret). No V0 write. Fiber +41 setter `00CB78D0` (`mov al,[esp+4]; mov [ecx+41],al; ret 4`). `00CB7950` clears +41 after update; first-seen stays 0. Not in factory vtbls `012C3000` / `012F72D0` / `012C3688` / `0129B938` / `012C1648` / `012C2748` | PROVEN | `fab17be` / `World_camera_006B4900_slots_lerp_into_ScriptedCamera` (`CameraTickSkipped`, `CameraTickTimer=-1.0`, `PoseTickFn`) / `Camera_004164E0_runs_on_install_after_WorldFrame` (`FiberUpdateFlagSetter`) |
| `009DA9F0(1)` first-seen empty → `009DB6E6` skip DIP. Nonempty would be `00A058C0` then `[device+88].vtbl+332`. No `cmp …,0x22`. Type-0x22 DIP is vtbl+332, not a 0x22 switch. `009D9C80` first 250: dirty-list only. Queue begin +16020. Later `b1d6877` host Notes `009DA9F0(1) [+16020] empty` / skip DIP: `DisplayFlushShouldDip(0, 0)` is always false; host never stores `[this+16020]`. That is stand-in, not a recovered queue read. `FrontendEnqueueRan` can still be true. `Frontend2dDipIssued=false`. Leave #36 open | PROVEN | `6493c77` / `b1d6877` / `Frontend_present_runs_on_install_after_videos` / `Frontend_009DA9F0_first_seen_is_empty_skip_not_type_22` (`DisplayQueueBeginOffset=16020`, `DrawIndexedPrimitiveVtbl=332`, `DisplayPrimitiveFn=00A058C0`). leave #14 open |
| `0042E204` Init Engine: `00B26340` alloc `0x178` ctor `00B260B0` vtbl `012A0F3C` at retail+88. `0041AFA0` `[012A0F3C+92]` = `00B23BC0` → `00B324A0([0x1436E80], widget+0x15C, rec, 0xC0, 0)`. Type `[rec]=0x22` → `[0x1436E84]+16+0x22*4`. dest+4=0 first-seen. Handler vtbl+20 UNREAD (not memcpy +16020) | PROVEN | `d6821b8` / `Frontend_0042EC7C_frame_is_input_then_0042DF9E_Present` (`FrontendSubmitFn=00B23BC0`, `FrontendSubmitDispatchFn=00B324A0`, `FrontendEngineVtbl=012A0F3C`, `FrontendEngineObjectSize=0x178`). leave #14 open |
| First-seen WorldCamera +6296 is the ctor axis, not the eye (`IsCtorAxis`). `00B314E0` consumes helper from the hero (look `006B2CA0` V4, up `(0,0,1)` `FirstSeenCameraUp`, FOV 70 from `00A0C130` / `0x3E471B48` turns). Letterbox 1024×768 (`00B30B50` camera +176/+180). AABB-cull neighbour STB before height parse (`00BDC2D0` / `00BF6F80`). `textures.h` resolve cached (`LevelLibrary.LandscapeEnums`). Skip `SetMesh`/`SetTextures` when payload unchanged | PROVEN | `be3339e` / `Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint` / `Install_banks_and_startup_videos_exist` (`RendererHelperBound`, `GameCamera.FirstSeenFovDegrees`) / `CameraProjectionTests` (`ApplyRendererHelper`, `LandscapeFrustum.CameraUpdate=00B314E0`) / `World_submit_is_stable_between_frames` / `BuildFrame_reuses_texture_array` / `Texture_decode_is_cached` / `Lookout_tile_origin_is_region_local` / `MeshBank_does_not_reparse_c3d` / `Native_draw_order_is_begin_layers_end_present`. Do not reopen #6 / #13 |
| Landscape `00BF4570` cells as persistent draws. Each STB 16 m tile is one LandscapeCell (cached on LevelLibrary). Submit builds per-cell MeshDraws; landscape and C3D on separate VBs. Concat stays a test rollup. Print LoadTiming on first New Game submit | PROVEN | `3dba4a1` / `Lookout_cells_match_stb_tiles` / `World_submit_is_stable_between_frames` (`SubmittedLandscapeCells`, `LastLoadTiming`). First commit submitted bits `0x4`/`0x40`; cell DIP later `0x40` only (`40037b1`) |
| Static C3D: preserve native instance transform. C3D verts stay file-local. `MeshDraw.World` is the ObjectTransform 3x4 (`009881F0` wrapper+496). Draw multiplies W*VP. No per-triangle bake | PROVEN | `5e57e64` / `Instance_world_is_009881f0_not_baked_verts` |
| Sky: submit `0x2000` midday dome on New Game. CEngineSkyRenderer `GRAPHIC_ATMOSPHERIC_SKY_MIDDAY` after static C3D (layer order `0x4`/`0x40`/`0x20`/`0x2000` at this commit). Uses `SkyViewProjection` | PROVEN | `63336fd` / `Install_banks_and_startup_videos_exist` (`PassBit == 0x2000`) / `Native_draw_order_is_begin_layers_end_present` / `ScenePassTests.Registration_is_34_layers_and_walks_landscape_before_sky`. PALSKIN `0x100` later (`676bf63`) |
| PALSKIN: drain on layer `0x100` before sky. `00B33010` type-0/1 first slots use bit `0x100` (registration index 7), after static `0x20` and before sky `0x2000`. `0x80`/`0x200` after sky later (`f4a1efc`) | PROVEN | `676bf63` / `Game_00435530_Presents_009BEEB0_and_pumps_input` (`SubmittedLayerBits`) / `Native_draw_order_is_begin_layers_end_present` / `ScenePassTests.Registration_is_34_layers_and_walks_landscape_before_sky` / `Instance_world_is_009881f0_not_baked_verts` (static stays `0x20`) |
| Resources: unload region patches; one WAD for TNG. `00B40000` / `00B3EF40` release map-slot LEV/STB/cells. Banks and WAD handle stay process-lifetime | PROVEN | `7c93828` / `LevelLibrary_unload_map_drops_region_not_wad` / `Install_banks_and_startup_videos_exist` (`HasCachedCells`, sky `0x2000`) |
| Landscape: `00BF4570` cell DIP is layer `0x40` only. Bit `0x4` is `00BF71D0` tessellator background, not the stored cell mesh | PROVEN | `40037b1` / `Lookout_cells_match_stb_tiles` (`LayerForeground`; no `LayerBackground`) |
| Resources: cache `game.bin` and `meshdata.h` at process lifetime. `009AD410`-style: one def table, not a per-map GameBin open | PROVEN | `cf0ee50` / `Install_banks_and_startup_videos_exist` / `LevelLibrary_reuses_lev_and_height_parses`. `LevelLibrary.Defs` / `MeshEnums` |
| Camera: lock `00A0C130` ctor helper and `00988A50` WVP numbers. `00A0C130` is a packer. GameCamera ctor look is +Z, up `(1,1,1)`. `006B42F0` tail `008857E0` is bank lerp; vtbl+244 is colour-filter. Does not change the consumed first-Present helper (still UNREAD) | PROVEN | `a6f939a` / `CameraMatrixParityTests.GameCamera_ctor_look_default_is_plus_z` / `WorldCamera_tail_is_colour_filter_not_helper` / `GameCamera_ctor_helper_wvp_at_1024x768` / `Projection_numbers_match_009883F0`. Do not reopen #6 / #13 |
| PALSKIN: submit file dest triangles, print load timing. `MeshFile.Triangles` already apply `00A9E1E0` × IBM. First-seen does not re-skin on the CPU. Hero 4299 stays on layer `0x100` | PROVEN | `cb22533` / `Palskin_submit_uses_file_triangles_not_repose` / `Install_banks_and_startup_videos_exist` (`SubmittedPalskinMeshIds` 4299) / `World_submit_is_stable_between_frames` (`LastLoadTiming`) |
| Static C3D: one file-local VB per mesh, instance W on the draw. `00BB2540` copies locals once (shl idx,4, no matrix). `009881F0` owns the 3x4 per instance. Repeated Lookout lamps no longer duplicate the C3D soup | PROVEN | `306e83c` / `Instance_world_is_009881f0_not_baked_verts` (shared verts, distinct Worlds) |
| Environment: bind `REGION.EnvironmentTheme` name, not lighting. Lookout RegionDef is `ENVIRONMENT_THEME1` #2346 (same as Oakvale intro), not `ENVIRONMENT_OAKVALE`. Live first-seen lights stay `00B482A0` record 0 (c19/c20/c35, fog 1000/2000, packed count 0). Seven Lookout streetlamps are C3D 4978, not 2LIGHTS | PROVEN | `4429113` / `Install_banks_and_startup_videos_exist` / `GameBinFormatTests` (`AuthoredEnvironmentTheme`, `FirstSeenPackedLightCount`) |
| Scene: drain PALSKIN `0x80`/`0x200` after sky. `00B33010` walks slots 8+10 on bit `0x100`, then sky `0x2000`, then slot 14 on `0x80` and Flag1 slot 9 on `0x200`. Static `0x18` stays slot 0 on `0x20`. PALSKIN geometry still submits on `0x100` until type1/Flag1 routing is wired (leftover research, not a new issue) | PROVEN | `f4a1efc` / `Dx9VulkanParityTests` / `EngineLifecycleTests` / `ScenePassTests` / `WorldPipelineTests` |
| Hero: copy GuildArrivalHSP RHSet axes on `006AC910` spawn. Lookout start marker faces +X / +Z. #18 done. Exist set 465/465; primary C3Ds 193/193 | PROVEN | `ff808b1` / `Install_banks_and_startup_videos_exist` (hero ObjectTransform forward +X) |
| Resources: skip unused 1 m height grid on `00BF4570` cell load. Native draw is stored STB tiles, not bilinear FineHeights. LevelLibrary reuses WLD already parsed at Init World. Open/Present records primary Graphic handles only | PROVEN | `8bfbf39` |
| PALSKIN: bind adult Hero as the script/move/draw Thing. BindScene no longer only accepts `CREATURE_HERO_CHILD`. `006AC910` spawn binds HERO/Hero and seeds `World.Positions`. PlayLoopingAnim `00CC186C` now ApplyInner so ClipKey is set. Thing XYZ writable so WalkTo writes back. c38 dest upload is later GPU path (do not file). leave #19 open | PROVEN | `27cb7ee` / `Install_banks_and_startup_videos_exist` (Resolve HERO/Hero same as life.Hero) / `PlayLoopingAnim_is_vtbl80_not_PlayAnimation` (`ClipKey`, `InnerApplied`) |
| Render: INNER_SKY does not write oFog. `VSHADER_INNER_SKY` is dp4 oPos + mov oD0, v1. D3D default oFog is 1. Applying 1000/2000 land plane to the 6500 dome made native brightness path black. Land leftover c3 unchanged. No invented ambient or extra x2 | PROVEN | `d7615a6` / `GpuTextureTests` / `ShaderFormatTests` / `WorldGeometryTests` |
| Landscape: INDEX16 strip on the `00BFE050` mesh, DrawIndexed. `00BF4570` DIPs mesh+52/+56 (vtbl+328, type 5, prims+2), not the 72-byte cell. Bit `0x4` is still `00BF71D0`, not this IB. Host unwinds strip to a list; verts unique per cell. W = I on region-local STB | PROVEN | `35f3d20` / `WorldGeometryTests` |
| PlayAVI: native `00628B79` dest and clear leftover frames. Scale to viewport width, center leftover height. Recreate video image when height changes so 640×400/480/360 do not share leftover rows. Clear one frame between startup videos. #11 done. leave #20 open | PROVEN | `98c4acc` / `PlayAVI_00628B79_resizes_to_viewport_width_and_centers` / `PlayAvi_rewrites_xmv_to_installed_wmv_and_blocks` |
| `WaitPlayAnimation` `00CC18E0` plays via vtbl+72 (or vtbl+76 if IsTrue arg3) then leftover vtbl+104 | PROVEN | `1eec3bc` / `WaitPlayAnimation_plays_then_polls_vtbl104` |
| FORWARD_TREE PE→WinMain→no-save New Game. Mesh bank `MBANK_ALLMESHES` at `0049E620` / `00A09F20` / `00A27030` / `004BBFD0`. Engine owns map open/close `00B40000` / `00B42750`. Client must not open a second graphics.big dump | PROVEN | `f0fb184` / `Pe_entry_is_crt_not_new_game` / `Install_banks_and_startup_videos_exist` |
| Game vtbl+32 `00416953` is Loading world (no-save `[+90588]` empty skips `004A3200` Loading save). Then `004A1840` WLD/quests. `00B3E820` current handle LookoutPoint; `00B41E50` neighbours (PicnicArea); `00B420F0` name table; `00BDF010` neighbour patch. `00B42530` is STB-miss fallback only | PROVEN | `fc8b261` / same install test + `Pe_entry` constants |
| `00416953` no-save tail: path `+90576` `FinalAlbion.wld` (Leave `0042F44D` / `00415E17`); `0x122EE14` `updatedscenic.wld` is fallback only. `004A1840` then `[0x13B8648]==0` → `0049F180` / `004B4A10`; `004BBC00` is `ret 4`. Not a region | PROVEN | `LoadWorld_00416953_no_save_is_004A1840_then_0049F180` |
| `004184BD` after vtbl+32: `0049BA70` / `00416392` / `004AE9D0` / `default_user.ini` miss / `user.ini` `009EC890` / seed `004167DA`+`+90592`. Not first pump or region | PROVEN | `InitGame_004184BD_after_00416953_reserves_then_user_ini` |
| `user.ini` `ActivateQuest("Gameflow")`: `00419CE0` `[world+56]` `006E7740` vtbl `01260F0C+1104` `00892E80` → `004B4A10` → `00CB5AD0` | PROVEN | `UserIni_009EC890_RunScript_joystick_is_00999230_miss`. Direct `00CB5AD0` from user.ini DISPROVEN. |
| `[0x1375454]=1` `.data`; `004B3CE0` constructs. Gameflow `00CE6CF0`/`00CE75B0` Main watcher. `S_GF` cutscene at activate DISPROVEN. `00501450` not reached | PROVEN | `Gameflow_00CE75B0_is_Main_watcher_not_S_GF` |
| Type-1 `00CB8220` Main `00A44880` → `00CE7670` state 0 yields on `Q_NewOakValeIntro`. Same walk `00CEF3B0`/`00CEF550` yield. No Oakvale/trader activate. `004167DA` not called | PROVEN | `Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro` |
| After parked watchers: `[esi+56]` Gameflow last; `006874B0` 7×`00687540(55,50)` skip fire (`[0x13B89BC]+50`). Empty-list DISPROVEN | PROVEN | same type-1 test |
| After `006874B0`: `006BB990` 1/15 time; `004C5E90` ret; `006E60F0`/`0051F070`/`006E37D0` empty; `006BDC60` miss; `0043A080`; `006B2260` | PROVEN | same type-1 test |
| First `00416E78` with `0049D870>1`: `004457F0` then empty `00446A30` (`00446330` miss, `00446220` `+168=0`, no `0041649C`). Prefix `009A57B0` always. `00416E78` runs before `004A5E10` | PROVEN | `After_WorldFrame_gt_1_00416E78_is_004457F0_then_00446A30` |
| First `00417001` WorldFrame>1: `[0x13B8630]=0` → `0041707E`/`0049E080`. `004AEA70=0` and `[0x13B8688]=0` skip `00435F70`. Always-Present leftover DISPROVEN | PROVEN | `After_WorldFrame_gt_1_00417001_is_0041707E_then_004AEA70_skip` |
| Later `004AEA70=1`: `Pump(0)×5` after skip then `00435F70`/`009BEEB0`. `WorldSubmitted` Present gate DISPROVEN. No region | PROVEN | `After_004AEA70_eq_1_00417001_is_00435F70_Present` |
| First-seen `00435530` dest empty: `00434CD0`→`009D8250 ret`; `00435000`/`00435070` miss; `009DA9F0` empty no DIP. Invented layer bits DISPROVEN | PROVEN | same Present test |
| Later type-1 `00CB8220`: `00A44880` resume, `00893610` still 0, yield. Parked-skip DISPROVEN. No Oakvale activate | PROVEN | `Type1_resume_00CB8220_is_00A44880_then_00893610_yield` |
| No-save does not activate `Q_NewOakValeIntro`. `00CD6E27` bind-only; WLD/`+90584`/`004B5080`/`AddTestQuest`/`00896A30` are not the activator | PROVEN | `No_save_does_not_activate_Q_NewOakValeIntro` |
| First `004189C2` after dummy record: `0040D2A0`/`0040BC80`/`00B239A0(12,20)`/`009F2660`. Not a region | PROVEN | `First_pump_004189C2_is_0040D2A0_then_00B239A0_not_a_region` |
| `009A8150` names-only (`RegisterRetailBankTable` does not open graphics.big/textures.big). LevelLibrary caches LEV/STB/TNG parses | PROVEN | `253126e` / `Install_banks_and_startup_videos_exist` / `LevelLibrary_reuses_lev_and_height_parses` |
| `PresentWorld` opens instances + LEV/STB headers only (`expandGeometry` false). `00B3EFA0` PeekMapHeader 48-byte LEV + STB size. `009AD410` handles; C3D parse is later at submit/draw. `CurrentCompiledLev`/`CurrentHeightField` null at open | PROVEN | `17908c3` / `Install_banks_and_startup_videos_exist` PresentWorld asserts / `Open_records_instances_without_c3d_or_tiles` / `PeekMapHeader_is_00b3efa0_not_full_parse` |
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
| Slot fields beyond `+6296/+6312/+6328` (weights / `+6340/+6352`) | UNREAD | Lerp into `ScriptedCamera` is PROVEN; first-seen Weight0 ctor 0.2 is locked (`52e26bc`). Leftover slot bodies are not |
| `00435530` overlay `00435000` / interface `00435070` bodies | PARTIAL | Present + `009DA9F0` layer bits PROVEN; overlay/interface still Note |
| Frontend `00595222` widget DIP body | DISPROVEN as DIP | `00595222` calls `[node+20].vtbl+8`. Types 5/10/11/12/16/18/38 are `00530260` DrawsChildList (`7adf621` / later `e3208eb` type 11/38). Tick/draw walk every resident `[ui+84]` slot (`b4a2c89` / `Frontend_tick_and_draw_walk_resident_ui84_slots`). Resident slots 0 / `0x14` / `0x17` still MATCH `00595222` (`405b1e8` / `Frontend_dumps_press_start_new_profile_main_menu_after_avi_skip`). Present skip VAs `0052F180` / `0052F1D0` / `0041C5A0` / +348 are constants + proof only (`Factory_builds_press_start_then_main_menu_from_the_same_walk`); native skip via vtbl+400 (+504) still unused (leave #46 open). `DrawContainerWalk` / `IsPresented` no longer exclusive-walk `kids[ActiveChild]` (dropped in `88a9ab8`). Present `0042DF9E` still Note-only. Present/UI submit still not locked. Clicks now exist (`ClickNamed` / host hit midpoint). New Profile dest/hit is host stand-in: persist-size skip / `TryChromeHit` are #48 family; `TryMouseAreaDest` dest-copy was removed in `e3208eb` (leave #48 open). `CompositeFrontendPresent` State-6 tree skip was host and was REMOVED in `e3208eb`. Host still skips world/gizmos on `_frontendReady`. Host still Notes `009DA9F0` DIP. leave #14 open. |
| `00B324A0` type-0x22 handler vtbl+20 | PARTIAL | dest+4=0 only while dest is 0. Nonempty dest draw path is still `00BAE2D0` / `00BAD8A0`. `EnqueuesDisplayQueue` still false. No E8 `009DB700` (`1a08cc0` / `Nonempty_dest_draws_via_00BAE2D0_not_009DB700`). Host Notes `009DA9F0(1) [+16020] empty` stand-in (`b1d6877`); `DisplayFlushShouldDip(0, 0)` always false; host never stores `[this+16020]`. Type-6 host Notes `00543910` / `0x27` size 64, not `0041BEB0` / `0x22` (`a141c27` / `Type6_widget_packs_00543910_type_27_not_0041BEB0`). Type-0 stays `0041BEB0` / `0x22`. Type-6 Font 26051 is a names.bin offset that resolves to `ENG_ARIAL_24`, not the `0054F4B0` `ENG_ARIAL_16` helper (`db36334`). Init Fonts `ENG_ARIAL_18` at `game+90444` is a different object (`9901d3b`). leave #36 open. |
| `0041AC20` dest rect from +204/+248 | PROVEN ctor 0,0,0,0; type-6 dest (512,384,512,384) | leftover+204 GraphicIndex gate recovered (`76edbbd` / `Leftover204_is_0041AC20_graphic_index_not_persist_size`): index 0 → leftover 0. dest still invented 512,384,512,384; tests lock that dest (`Frontend_0041AC20_dest_and_0xE5_new_profile_0x126_main_menu_15`). Leftover `+204` is widget +204, not dest width (`b1d6877` / `Type6_leftover204_is_widget_plus204_not_dest_width`). Type-6 leftover 16×16 is gone (0 first-seen). Type-6 dest is a point at remapped origin (512,384,512,384), not ctor 0,0,0,0 (`Press_Start_type6_dest_is_a_point` / `Frontend_PRESS_START_is_type_10_with_text_child`). `UI_TITLE_01` dest still from texture FrameWidth 256 (`Press_Start_first_seen_dest_table_matches_0041AFA0`). Leftover `+204/+208` only when `GraphicIndex != 0` (`1a08cc0`). New Profile type-2 leftover (180,0) and `PlaceTableCell` n==3 fill are host heuristics, not dest lock; persist-size skip / `TryChromeHit` are #48 family (leave #48 open). `DisplayFlushShouldDip(0, 0)` stand-in. leave #36 open. |
| New Game keyboard N/Enter (host stand-in) | PARTIAL | Client still has no `Key.N` / `ActivateNewGame`. Enter still TypeKey (PlayAVI skip). Clicks exist (`ClickNamed` / host hit midpoint) but dest is invented (leave #48 open). Host Return quarantined as accept (`db36334`). Host LMB edge queues Type4 (`5dcc1fc`). Host queues LMB-up as type 6 (`48133e9`). Host current `_frontendWidgets` is still the switched screen (input leftover) (`84a8350`). `DispatchFrontendMessage(15)` is `0059A238` vtbl+32. Enter still queues that message. Present `0042DF9E` still Note-only. `CompositeFrontendPresent` State-6 tree skip was host and was REMOVED in `e3208eb`. Host still skips world/gizmos on `_frontendReady`. New Game still not locked. leave #14 open. |
| `006B8640` / `008889C0` leftover (do not write V0 first-seen) | UNREAD | `006B2CA0` pose is PROVEN (`204a214`). Host `SeedAt(1.6m)` is DISPROVEN as live New Game. Lookout helper FOV 70 from `00A0C130` (`be3339e`). `00A0C130` is a packer (`a6f939a`); ctor look +Z, up `(1,1,1)`. SHOT2 FOV 72 is intro-view leftover — do not collapse into Lookout. Do not reopen #6 / #13 |
| Consumed first-Present helper | UNREAD | `a6f939a` locks ctor packer / `00988A50` WVP / bank lerp `008857E0` / vtbl+244 colour-filter. Does not change the consumed first-Present helper. Do not reopen #6 / #13 |
| `004978A0` LCG seed for `006B3030` | UNREAD | Spring ran; Weight0/V0 first-seen locked (`52e26bc`). Seed unread |
| `0055CB10` frontend player-move listener | UNREAD | Actions 0–5 / 20–21 recorded; no recovered listener |
| Game input poll `00446462` / `004963E6` | UNREAD | `e7b3c76` recover note (separate from `00446A30`) |
| Who writes persist `PlayerRegionName` on New Game | UNREAD | Click/message path is PROVEN; persist HEADER writer is not |
| `[esi+42]` load/save | UNREAD | `recover-00595582`; `[esi+41]` Leave is PROVEN |
| Global-things *use* after `004FDBC0` / `.gtg` parse | UNREAD | Load switch is PROVEN; `00521AE0` is per-map TNG, not this apply |
| GTNG file body | N/A on TLC | Missing skip is PROVEN |
| MiniMap `0082BA00` / villages `005064C0` bodies | UNREAD | Named from `SetRegionAsLoaded`; not claimed as runtime. Loading world vtbl+32 `00416953` is PROVEN (`fc8b261`); leftover is MiniMap/villages bodies only |
| Wire persist-Oakvale (or a proven New Game region write) to `FirstSceneWorld` | UNREAD | Host first-scene lists are a separate reconstructed path |
| PALSKIN type1/Flag1 routing (slot 14 `0x80` / Flag1 slot 9 `0x200`) | UNREAD | leftover research (`f4a1efc`); geometry still submits on `0x100`. Not a new issue |
| PALSKIN c38 dest upload | later | `27cb7ee` — later GPU path; do not file |
| Init Definition Manager dest `[01232C24+8]` | dest UNREAD | Host Notes `0044C6B0` / `0044C72B [vtbl+8]` / `009ACB10` / `009E5250` + `DefinitionManagerPrepared` are locked (`587baae` / `Init_Definition_Manager_00416005_resets_plus88_via_vtbl8`). dest `0044C72B` as `[01232C24+8]` is not rdata-locked (sibling `proofs/00416005-def-manager` dest UNREAD). Leave #42 open. Do not call dest PROVEN. Different object from later Subtitled `[0x13B8A54]`. Do not invent a `game.bin` parser. |
| Init Thing Components further Add Def Class | leftover Note-only | Thing Components now Notes Add Def Class for CHeroMorphDef then CHighlightItemDef / CSmokeGeneratorDef / CTimeAppearanceFadeDef / CCreatureNavigationDef / CInventoryItemDef / CLookDef / CReadableDef / CVillageDef / CVillageMemberDef / CBuyableHouseDef / CBuyHouseDef / CWifeDef / CDoorDef / CLightDef / CSpotLightDef / CClockDef / CHeroDef (`b7f4c34` / `acfe46f` / `100e5cf` / `1a4c51d` / `ee08490` / `3a7b594` / `b1d6877` / `113a514` / `a141c27` / `76edbbd` / `91564bd` / `f30c099` / `71ae66e` / `405b1e8` / `46663e3` / `db3899a` / `065eb28` / `6577614`). MATCH is Note-only + `*DefClassRegistered` flag, not live constructed. LoadDef field walk stays PARTIAL. Do not invent a live object. |
| Lookout/GuildArrival vs Oakvale intro view (#4) | leftover #4 | No-save first region / first *rendered* scene is LookoutPoint (`RegionThings` + `006B3FF0` / GuildArrivalHSP). First-scene *intro view* is still `StartOakValeWest` / `HerosOldHouse` / `CAM_OVIF_SHOT2` (`FIRST_SCENE_*`). Do not collapse those ledgers. Do not fold first-proximity TNG (#50) into this leftover. Leave #4 open. |
| New Profile dest/hit (#48) | leftover #48 | New Profile dest/hit still not dest-locked (`b075dd3` … `b216990`). Persist parse of type-12 `+326=30` / Sprites / `SpriteKeys == [0,1,4]` is real file recover. `PlaceTableCell` n==3 leftover fill still host. `TryChromeHit` still invents type-16/37 hit (regresses `6e76ac5` dest AABB only). `TryMouseAreaDest` dest-copy was removed in `e3208eb` (dest stays a point). Persist-size skip (`persistW==0 && persistH==0` before `PlaceTableCell`) is extra host heuristic on the same helper — #48 family, not a new leftover. Tests lock pack formula / tiling / nonempty chrome hit / click `(700,300)` / hover tautology, not dest numbers. Leave #48 open. |
| First-proximity TNG pump (#50) | leftover #50 | Host `LoadGlobalThingsFile` `break`s on the first `LoadedOnPlayerProximity` map and Notes `004FDBC0` / `004FBF60 LookoutPoint.tng` (`d628952`). Real reason is New Game pump OOM if every proximity `.tng` is parsed. Tests lock `GlobalThingMapsLoaded == 1` + LookoutPoint in the Note, no Bowerstone Note. Does **not** lock NewMap index, `ebx=1`, `004FBF60` callee, or `00501450`. First-proximity TNG is host OOM workaround, not a recovered `004FDBC0` NewMap-1 lock. Do not fold #50 into #4 (ledgers vs TNG pump). Leave #50 open. |
| Native `00530260` skip vtbl+400 (+504) still unused | leftover #46 | Exclusive-walk was dropped in `88a9ab8`. `DrawContainerWalk` / `IsPresented` now walk every child; `ApplyFirstSeenState` no longer `Visible=false` on siblings. Type-18 inactive forest tiles now Visible dest 410x410 opaque (dump batchDraws 193→223). Native skip via vtbl+400 (+504 CRC parsed but NOT applied) still unused; type-18 inactive siblings present. Present skip from listing stays constants + proof only: `BorrowedVisibleFn` `0052F180` / `ClipBitFn` `0052F1D0` / `ForwardSelectFn` `0041C5A0` / `TextSliderIndexOffset` 348. Do not treat skip as recovered. Leave #46 open. |
| Init Player Interface leftover `Register(ActionInputListener)` / `00488D20` notes | leftover | Not a function; factory is Create Players. Host ctor note of `0044A3B0` under Init Player Interface is DISPROVEN (moved to Init Player Manager, `4a03969`). Do not file a new issue. |

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
| `PlayAnimation` runtime / clip sample (`vtbl+72` `004C7470` walk; +68 `00686920` accept; `00662A00` table; `0070C050`+`0070D580` inner). Apply is **PROVEN**; first-key XSEQ sample is in `PaletteForPose`. Leftover is time interp `00AA0090`. `FirstSeenPlayAnimationAppliesPose=false` | PARTIAL | COMMAND_MAP, PARITY 0b; `XSeqFormatTests` |
| `WaitPlayAnimation` leftover vtbl+104 poll (apply `00CC18E0` is **PROVEN**; not unread apply) | PARTIAL | `1eec3bc` / COMMAND_MAP |
| `Create` `008A9100` / `Remove` `004C9B80` mesh | UNREAD | PARITY 0b “next unread” |
| Skip-key bodies / vector 1 | UNREAD | first-seen skip does not fire |
| Who writes `[quest+80]` / `AttackOver` after `vtbl+2584(12)` + `HerosOldHouse` | UNREAD | not a `mov` in `00DBDE00–00DBF000` |
| `LookToThing` / `LookInDirection` heading bodies | PARTIAL | record + yield / no yaw write |
| `MuteSounds` apply; `DoCameraPreloading` `vtbl+1560/1568` | PARTIAL | |
| Dialogue UI (`Speak` / `InteractiveSpeak` / `DialogSpeak`) | PARTIAL | one yield; no invented UI |
| `SneakTo` / `WalkTo` mesh move (`004C72B0` stub) | PARTIAL | `FirstSeenSneakToAppliesMove=false`. Dest via `006A9960` / `World.Positions` is PROVEN (`666df8f`); Thing XYZ writable so WalkTo writes back (`27cb7ee`); mesh body is not. |
| `PlayCombatAnimation` pose | PARTIAL | `vtbl+76` does not read the name |
| `call [vtbl+8]` resume site; `vtbl+28` yield body; `Main` `00CDD440` | UNREAD | PARITY 0b |
| Startup PlayAVI still runs 3D Draw | PARTIAL | issue #20. Unload `00A3B380`/`00A3BC20` before next slot is recovered (`0ace433` / `PlayAvi_rewrites_xmv_to_installed_wmv_and_blocks`); leftover is the 3D Draw, not unload |
| `WmvPlayer` never QIs `IBasicAudio` (native `00A3B9D0` does) | PARTIAL | issue #9 |

`DoScriptFrame` / `PlayAVI` / cameras / fades are **PROVEN** at the
script layer. PlayAVI dest vs 1600×900 (#8) is done. Native
`00628B79` dest + recreate-on-height (#11) is done (`98c4acc`).
Unload `00A3B380`/`00A3BC20` before the next slot is now recovered
(`0ace433`); leftover #20 is still 3D Draw during startup PlayAVI.
`IBasicAudio` (#9) stays a **PARTIAL** leftover. Do not invent
fade/AVI/wake playback beyond those bodies.

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
`Silk.WindowOptions.DefaultVulkan` 1600×900 (#8) is done.
Native `00628B79` dest + recreate-on-height (#11) is done
(`98c4acc`). Unload-before-next-slot is recovered; leftover #20 is
3D Draw during startup PlayAVI. `IBasicAudio` (#9) stays a PARTIAL
leftover. Steam timing is PARITY Open item 0, not a first-scene
3D invent.

### 4. Animation (after boot)

README item 1, *after* a ticking world. First-seen wake lines
(`CS_WAKING_UP_*`) need the clock; create still has
`FirstSeenPlaysAnim=false`.

| Left | Status |
|---|---|
| Animation resource lookup | UNREAD / PARTIAL |
| Clip first-key sample | in `PaletteForPose` (`00A999B0` / `00AA4680` / `00A4C5E0`); leftover is time interp `00AA0090` |
| Skeletal pose | UNREAD (`FirstSeenPlayAnimationAppliesPose=false`) |
| PALSKIN dest beyond file triangles | first-seen dest is `MeshFile.Triangles` (`00A9E1E0` × IBM, `cb22533`); no CPU re-skin. Play-anim product is not. Type1/Flag1 routing stays UNREAD (`f4a1efc`). c38 dest upload is later GPU path (`27cb7ee`; do not file) |

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
- Grinding Crowd\* / debug / `LadyGreyIntro` / boss UNREAD tokens
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
   `006B2CA0` first-seen pose is now PROVEN (`204a214`); invented
   `SeedAt(1.6m)` is not the New Game path. Lookout helper FOV is
   70 from `00A0C130` (`be3339e`); `00A0C130` is a packer
   (`a6f939a`). Consumed first-Present helper stays UNREAD — do
   not reopen #6 / #13. SHOT2 FOV 72 stays intro-view leftover.
   `006B8640`/`008889C0` leftover UNREAD.    Frontend `00595222` is the resident-slot vtbl+8
   DrawsChildList walk (`7adf621` / `b4a2c89`). Present
   `0042DF9E` still Note-only. New Game keyboard N/Enter
   leftover stands (#14). Host current `_frontendWidgets`
   is still the switched screen (input leftover)
   (`84a8350`). `CompositeFrontendPresent` State-6 tree
   skip was host and was REMOVED in `e3208eb`. Host still
   skips world/gizmos on `_frontendReady`.
   Host still collapses `[packet+0]` onto MessageId.
   Press Start `0xE5` is slot `0x14` `0059B5D7`
   (`59fde69`). `[ui+84]` keeps `0x14`/`0x17` across
   switch (`84a8350`). `+332` via SelectState(6) is not
   a `+302` hide (`b8a2b21`). Init World `004A67D0` /
   `004A6E30` run inside `0041735A` before `00417418`.
   Recent commits `681620d` …
   `b216990` lock 0055BF10 hover / Accept 0x126,
   0042DF9E 2D present host skip, type 11/38
   DrawsChildList + drop hover hacks, persist
   clip/align + drop sibling hide / exclusive-walk,
   and first-seen colour style 0 + children-by-index.
   MATCH for named-stage Add Def Class is
   still Note-only + `*DefClassRegistered` flag, not a
   live constructed object. dest `0044C72B` as
   `[01232C24+8]` is not rdata-locked (leave #42
   open). Different object from later Subtitled
   `[0x13B8A54]`. Init Player Interface still leftover
   Register(ActionInputListener) / `00488D20` notes.
   Host ctor note of `0044A3B0` under Init Player
   Interface is DISPROVEN (moved).
   Host LMB edge queues Type4 (`5dcc1fc`). Host queues LMB-up
   as type 6 (`48133e9`). leftover+204 GraphicIndex
   gate recovered (index 0 → leftover 0); dest still
   invented 512,384,512,384 and tests lock that dest.
   Type-6 leftover 16×16 is gone; leftover204 is
   widget +204 (0 first-seen). Type-6 dest is a point
   at remapped origin (512,384,512,384), not ctor
   0,0,0,0. `UI_TITLE_01` dest still from
   texture FrameWidth 256. Present skip from listing
   is constants + proof only (`0052F180` / `0052F1D0`
   / `0041C5A0` / +348); native skip via vtbl+400
   (+504) still unused. `DrawContainerWalk` /
   `IsPresented` no longer exclusive-walk
   `kids[ActiveChild]` (dropped in `88a9ab8`);
   type-18 inactive siblings present (leave #46 open).
   New Profile dest/hit is host
   stand-in: `TryChromeHit` still invents type-16/37
   hit; `TryMouseAreaDest` dest-copy was removed in
   `e3208eb` (dest stays a point); persist-size skip
   is #48 family (leave #48 open). First-proximity TNG is
   host OOM workaround, not a locked native `ebx=1` /
   NewMap slot / `00501450` (leave #50 open). Do not
   collapse Lookout vs Oakvale (leave #4 open). dest
   still invented 512,384,512,384; DIP `(0,0)`
   stand-in; native skip via vtbl+400 (+504) still
   unused. Present `0042DF9E` still Note. Host still
   skips world/gizmos on `_frontendReady`.
   `CompositeFrontendPresent` still
   `BuildBatch(..., 0, 0, width, height)`.
   Resident slots 0 / `0x14` / `0x17` still MATCH
   `00595222`. Host Notes
   `009DA9F0(1) [+16020] empty` / skip DIP:
   `DisplayFlushShouldDip(0, 0)` is always false;
   host never stores `[this+16020]`. That is stand-in,
   not a recovered queue read. `FrontendEnqueueRan`
   still true on nonempty dest / PRESS START.
   `EnqueuesDisplayQueue` still false. No E8
   `009DB700`. Draw path is still `00BAE2D0` /
   `00BAD8A0` (#36). leave #36 open.
   #37 already locked
   at the previous freeze (`61e430f` / `48133e9`):
   `0xE5` host fill gone
   (`Frontend_press_start_type4_without_widgets_does_not_invent_0xE5`);
   Type4 posts type-10 +352 only; Type6 LMB-up action 28
   posts +228 after arm
   (`Type4_action_26_posts_stored_widget_message`,
   `Type4_drives_lifecycle_0xE5_then_0x126_then_15`).
   PlayAVI still 3D Draw (#20). leave #14 and #20 open
   (also leave #36 #42 #46 #48 #50 open). New Game submit now walks `0x4`/`0x40`/`0x20`/`0x100`/
   `0x2000` (`676bf63`); cell DIP is `0x40` only (`40037b1`).
   #11 done (`98c4acc` recreate-on-height + `00628B79` dest).
   #18 done (`ff808b1` RHSet +X). Lookout EnvironmentTheme is
   `ENVIRONMENT_THEME1` #2346, not `ENVIRONMENT_OAKVALE`. Do not
   collapse Oakvale intro view into Lookout.
2. **GTNG is not an unread file on TLC** — missing skip is PROVEN.
   `00521AE0` loads the current map `.tng`. Host first-proximity
   TNG (`d628952`) is leftover #50 (OOM workaround), not a
   `004FDBC0` NewMap-1 lock and not folded into #4. Remaining
   UNREAD is global-things *use* after `004FDBC0` / `.gtg`
   parse, plus `[record+36]` and catchup-tick writers.
3. **Intro opcodes are already walked.** Phase 2 is apply/runtime
   leftovers on that fiber, not “finish UNREAD tokens.”
4. **README animation-first is the long-term engine list**, not
   today’s master. Animation stays after the clock.

When this page and a ledger disagree, the ledger wins.
