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

Snapshot: **2026-08-18**, previous snapshot runtime HEAD
`306e83c` via PR #33 merge `4e8ec58`,
runtime HEAD `98c4acc` (*PlayAVI: native 00628B79 dest and
clear leftover frames.*).
Just locked on this path: `4429113`, `f4a1efc`, `ff808b1`,
`8bfbf39`, `27cb7ee`, `d7615a6`, `35f3d20`, `98c4acc`.
Do not include any later runtime if master moves. Freeze at
`98c4acc`. Ignore merges `e0d97cd` and `e2fca3d`. Ignore docs
commits `d5ed2e9` / `4e8ec58`. Docs PR #29
`bedcf919` is iOS Settings chrome only — CSS unchanged.
Master is still proving **boot / world clock**, not animation.
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

Recent commits (`9c625bc` … `98c4acc`) lock engine submit / host
Present / frontend flush / camera seed / helper camera /
landscape cells / instance W, not `00DBDE40`. Previous snapshot
`306e83c` via PR #33 `4e8ec58`.
Just locked: REGION.EnvironmentTheme name (`4429113`), PALSKIN
`0x80`/`0x200` after sky (`f4a1efc`), GuildArrivalHSP RHSet
axes (`ff808b1`), skip unused 1 m height grid (`8bfbf39`),
adult Hero Thing bind (`27cb7ee`), INNER_SKY no oFog
(`d7615a6`), INDEX16 strip DrawIndexed (`35f3d20`), PlayAVI
`00628B79` dest (`98c4acc`). Docs PR #29 `bedcf919` is iOS
Settings chrome only — CSS unchanged.

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
| Leave Press Start: `0059A238` msg `0xE5` → `00599D5C` empty `005955AB` → `00595845` → `00596917` slot `0x17` `NEW_PROFILE` | PROVEN | `Frontend_0059A238_msg_E5_empty_005955AB_is_00595845_then_00596917`. Native key UNREAD. Return→msg 15 from Press Start DISPROVEN. |
| New Profile accept: `00851770` seeds `0x122DE80` "Default"; msg `0x126` → `00851920` → `0059697A` `MAIN_MENU_NO_CONTINUE` | PROVEN | `Frontend_00851770_seeds_Default_then_0x126_is_0059697A_main_menu`. Native 0xE5/0x126 poster UNREAD. |
| Frontend New Game click: `0059A238` msg 15 → `[retail+41]=1` → Leave `0042F2A2` | PROVEN | `64a2e14` / `Frontend_00595582_new_game_message_leaves_without_RequestNewGame` |
| Menu built at `00595B24` (`UI_TEXT_NEW_GAME` id=0); not `00DBDE40` | PROVEN | same |
| Frontend frame `0042EC7C`: input `0042E3EE` → fill → draw `0042DF9E` (BeginScene / UI vtbl+8 / EndScene / Present) | PROVEN | `0d8f5e5` / `Frontend_0042EC7C_frame_is_input_then_0042DF9E_Present` |
| Same Present as PlayAVI (`009BEEB0`); extra `.wmv` after draw skipped (`00595A03` always 0) | PROVEN | same |
| `006C2170` Loading objects → `00522720` / `00521AE0` current-map `.tng` (LookoutPoint on no-save) | PROVEN | `1ebece6` / `Loading_objects_00521AE0_loads_LookoutPoint_tng` |
| `0051FD80` Load Single Thing: no-save LookoutPoint TNG has no PlayerCreature; `HOLY_SITE_PLAYER_START` `GuildArrivalHSP` → `00489D40` / `006AC910` inserts PlayerCreature at that pose. RHSet axes later (`ff808b1`); closed #18 | PROVEN | `8f89aad` / `Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint` (refined by `e0e0511`) |
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
| `Q_NewOakValeIntro` / SHOT2 is not first no-save Present. First playable is Lookout `006B3FF0` | PROVEN | `004162B5` dump: +20 then +28. Frontend widgets `00595222` UNREAD (black). |
| `0042DF9E` walks `[ui+84]` vtbl+8, then `009D9C80`/`009DA9F0(1)` twice. `00404A80` is getter `0x13B7CD8` | PROVEN | `5657176` / `6607c1e` / `Frontend_0042EC7C_frame_is_input_then_0042DF9E_Present` (`FrontendFlushCount=2`, `FrontendDisplayHelperFn`, `FrontendDisplaySingletonVa`). Widget DIP body still UNREAD (`00595222` Note). New Game still keyboard N/Enter (does not close #14). |
| `[node+20]` is `0041DB1D`/`0041D21B` type0 `0041B800` vtbl `0122F5D4`; draw `0041AFA0` not `0052D900` | PROVEN | `128c8e1` / `Frontend_0042EC7C_frame_is_input_then_0042DF9E_Present` (`FrontendWidgetsDrawn=1`, `FrontendMenuRoot`). Not UI singleton `012521A8+8` `0052D900`. |
| `00A09F20` miss: `[bank].vtbl+4` is `009D56C0` Open Bank File Async then `009A7F80` on `[0x13CA79C]` | PROVEN | `bbee903` / `Pe_entry_is_crt_not_new_game` / `Install_banks_and_startup_videos_exist` (`MeshBank.OpenVtbl4`) |
| `00404C00` first-seen `[0x13B7CD8+8]==0` skip; `0041AFA0` packs `0041BEB0` type `0x22` (not sibling `0041BF60`) dest `[edx+92]` `this+0x15C` size `0xC0` | PROVEN | `c612ad5` / same frontend test (`Frontend2dLastPacker=0041BEB0`, `FrontendDisplayFlag=false`). Dest rect `0041AC20` UNREAD. Does not close #14. |
| `00418289` dump: `00416296`/`00490A22` frontend+GUI gate; `009E1BC0` → `[game+90544]`; fade/player `004AEBA0`; world `0049D9E0`; vtbl+24 `00416E78`; `0041726D` WorldFrame. START_INITIAL_QUESTS factories `00CDE2F0`/`00F01760`/`00CDBD20`/`00CB8690` (vtbls `012C3000`/`012F72D0`). `006B3FF0` seed PROVEN (208); `006B63C0` bank copy 6×`0x1F4` PROVEN (91). Pose leftover `006B8640`/`008889C0` stay UNREAD; `006B2CA0` later PROVEN (`204a214`). Host `SeedAt(1.6m)` later DISPROVEN as live New Game (`204a214`). FOV 72 is SHOT2 leftover; Lookout helper FOV later 70 (`be3339e`). Do not StartCutscene(S_PSM) from the factory ctor. | PROVEN | `18ef09b` / [FORWARD_TREE.md](../runtime/FORWARD_TREE.md) §11 |
| `006B3FF0` +68 → `006B2CA0` pose (not invented `SeedAt` 1.6 m eye). First-seen V2/V3 `(1,0,0)`, V4 `(-1,0,0)`. `00A14440` normalize. Host `SeedAt(1.6m)` is not the New Game path | PROVEN | `204a214` / `World_camera_006B4900_slots_lerp_into_ScriptedCamera` / `Camera_004164E0_runs_on_install_after_WorldFrame` / `Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint` (`PoseComputed`, `WorldCamera.PoseFn`). Does not close #6 |
| `004A5A40` type-1 tick pumps `004B4490` `[0x13B89FC]` → `00CB8220` / `00CB7C40` / `00CB8170`. First-seen `QuestVtbl24Calls=0` (`+41==0` takes vtbl+4 start). `006B3030` V0 spring: Weight0 stays ctor 0.2; V0 stays `(1,0,0)` (no invented V0). `004978A0` LCG seed UNREAD; `00A14260` yaw/pitch not applied first-seen | PROVEN | `52e26bc` / `Camera_004164E0_runs_on_install_after_WorldFrame` (`QuestPumpRan`, `QuestPumpWalked>=6`, `FollowSpringRan`, `SubjectFillNoted`). FORWARD_TREE §11 |
| First-seen `006B3B80` skip (`+460=0`, qword +24 = −1.0 from `[0x1236700]`, fcomp `[0x122ED70]=0` → ret). No V0 write. Fiber +41 setter `00CB78D0` (`mov al,[esp+4]; mov [ecx+41],al; ret 4`). `00CB7950` clears +41 after update; first-seen stays 0. Not in factory vtbls `012C3000` / `012F72D0` / `012C3688` / `0129B938` / `012C1648` / `012C2748` | PROVEN | `fab17be` / `World_camera_006B4900_slots_lerp_into_ScriptedCamera` (`CameraTickSkipped`, `CameraTickTimer=-1.0`, `PoseTickFn`) / `Camera_004164E0_runs_on_install_after_WorldFrame` (`FiberUpdateFlagSetter`) |
| `009DA9F0(1)` first-seen empty → `009DB6E6` skip DIP. Nonempty would be `00A058C0` then `[device+88].vtbl+332`. No `cmp …,0x22`. Type-0x22 DIP is vtbl+332, not a 0x22 switch. `009D9C80` first 250: dirty-list only. Queue begin +16020. `Frontend2dDipIssued=false` | PROVEN | `6493c77` / `Frontend_present_runs_on_install_after_videos` (`DisplayQueueBeginOffset=16020`, `DrawIndexedPrimitiveVtbl=332`, `DisplayPrimitiveFn=00A058C0`). Does not close #14 |
| `0042E204` Init Engine: `00B26340` alloc `0x178` ctor `00B260B0` vtbl `012A0F3C` at retail+88. `0041AFA0` `[012A0F3C+92]` = `00B23BC0` → `00B324A0([0x1436E80], widget+0x15C, rec, 0xC0, 0)`. Type `[rec]=0x22` → `[0x1436E84]+16+0x22*4`. dest+4=0 first-seen. Handler vtbl+20 UNREAD (not memcpy +16020) | PROVEN | `d6821b8` / `Frontend_0042EC7C_frame_is_input_then_0042DF9E_Present` (`FrontendSubmitFn=00B23BC0`, `FrontendSubmitDispatchFn=00B324A0`, `FrontendEngineVtbl=012A0F3C`, `FrontendEngineObjectSize=0x178`). Does not close #14 |
| First-seen WorldCamera +6296 is the ctor axis, not the eye (`IsCtorAxis`). `00B314E0` consumes helper from the hero (look `006B2CA0` V4, up `(0,0,1)` `FirstSeenCameraUp`, FOV 70 from `00A0C130` / `0x3E471B48` turns). Letterbox 1024×768 (`00B30B50` camera +176/+180). AABB-cull neighbour STB before height parse (`00BDC2D0` / `00BF6F80`). `textures.h` resolve cached (`LevelLibrary.LandscapeEnums`). Skip `SetMesh`/`SetTextures` when payload unchanged | PROVEN | `be3339e` / `Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint` / `Install_banks_and_startup_videos_exist` (`RendererHelperBound`, `GameCamera.FirstSeenFovDegrees`) / `CameraProjectionTests` (`ApplyRendererHelper`, `LandscapeFrustum.CameraUpdate=00B314E0`) / `World_submit_is_stable_between_frames` / `BuildFrame_reuses_texture_array` / `Texture_decode_is_cached` / `Lookout_tile_origin_is_region_local` / `MeshBank_does_not_reparse_c3d` / `Native_draw_order_is_begin_layers_end_present`. Does not close #6 or #13 |
| Landscape `00BF4570` cells as persistent draws. Each STB 16 m tile is one LandscapeCell (cached on LevelLibrary). Submit builds per-cell MeshDraws; landscape and C3D on separate VBs. Concat stays a test rollup. Print LoadTiming on first New Game submit | PROVEN | `3dba4a1` / `Lookout_cells_match_stb_tiles` / `World_submit_is_stable_between_frames` (`SubmittedLandscapeCells`, `LastLoadTiming`). First commit submitted bits `0x4`/`0x40`; cell DIP later `0x40` only (`40037b1`) |
| Static C3D: preserve native instance transform. C3D verts stay file-local. `MeshDraw.World` is the ObjectTransform 3x4 (`009881F0` wrapper+496). Draw multiplies W*VP. No per-triangle bake | PROVEN | `5e57e64` / `Instance_world_is_009881f0_not_baked_verts` |
| Sky: submit `0x2000` midday dome on New Game. CEngineSkyRenderer `GRAPHIC_ATMOSPHERIC_SKY_MIDDAY` after static C3D (layer order `0x4`/`0x40`/`0x20`/`0x2000` at this commit). Uses `SkyViewProjection` | PROVEN | `63336fd` / `Install_banks_and_startup_videos_exist` (`PassBit == 0x2000`) / `Native_draw_order_is_begin_layers_end_present` / `ScenePassTests.Registration_is_34_layers_and_walks_landscape_before_sky`. PALSKIN `0x100` later (`676bf63`) |
| PALSKIN: drain on layer `0x100` before sky. `00B33010` type-0/1 first slots use bit `0x100` (registration index 7), after static `0x20` and before sky `0x2000`. `0x80`/`0x200` after sky later (`f4a1efc`) | PROVEN | `676bf63` / `Game_00435530_Presents_009BEEB0_and_pumps_input` (`SubmittedLayerBits`) / `Native_draw_order_is_begin_layers_end_present` / `ScenePassTests.Registration_is_34_layers_and_walks_landscape_before_sky` / `Instance_world_is_009881f0_not_baked_verts` (static stays `0x20`) |
| Resources: unload region patches; one WAD for TNG. `00B40000` / `00B3EF40` release map-slot LEV/STB/cells. Banks and WAD handle stay process-lifetime | PROVEN | `7c93828` / `LevelLibrary_unload_map_drops_region_not_wad` / `Install_banks_and_startup_videos_exist` (`HasCachedCells`, sky `0x2000`) |
| Landscape: `00BF4570` cell DIP is layer `0x40` only. Bit `0x4` is `00BF71D0` tessellator background, not the stored cell mesh | PROVEN | `40037b1` / `Lookout_cells_match_stb_tiles` (`LayerForeground`; no `LayerBackground`) |
| Resources: cache `game.bin` and `meshdata.h` at process lifetime. `009AD410`-style: one def table, not a per-map GameBin open | PROVEN | `cf0ee50` / `Install_banks_and_startup_videos_exist` / `LevelLibrary_reuses_lev_and_height_parses`. `LevelLibrary.Defs` / `MeshEnums` |
| Camera: lock `00A0C130` ctor helper and `00988A50` WVP numbers. `00A0C130` is a packer. GameCamera ctor look is +Z, up `(1,1,1)`. `006B42F0` tail `008857E0` is bank lerp; vtbl+244 is colour-filter. Does not change the consumed first-Present helper (still UNREAD) | PROVEN | `a6f939a` / `CameraMatrixParityTests.GameCamera_ctor_look_default_is_plus_z` / `WorldCamera_tail_is_colour_filter_not_helper` / `GameCamera_ctor_helper_wvp_at_1024x768` / `Projection_numbers_match_009883F0`. Does not close #6 or #13 |
| PALSKIN: submit file dest triangles, print load timing. `MeshFile.Triangles` already apply `00A9E1E0` × IBM. First-seen does not re-skin on the CPU. Hero 4299 stays on layer `0x100` | PROVEN | `cb22533` / `Palskin_submit_uses_file_triangles_not_repose` / `Install_banks_and_startup_videos_exist` (`SubmittedPalskinMeshIds` 4299) / `World_submit_is_stable_between_frames` (`LastLoadTiming`) |
| Static C3D: one file-local VB per mesh, instance W on the draw. `00BB2540` copies locals once (shl idx,4, no matrix). `009881F0` owns the 3x4 per instance. Repeated Lookout lamps no longer duplicate the C3D soup | PROVEN | `306e83c` / `Instance_world_is_009881f0_not_baked_verts` (shared verts, distinct Worlds) |
| Environment: bind `REGION.EnvironmentTheme` name, not lighting. Lookout RegionDef is `ENVIRONMENT_THEME1` #2346 (same as Oakvale intro), not `ENVIRONMENT_OAKVALE`. Live first-seen lights stay `00B482A0` record 0 (c19/c20/c35, fog 1000/2000, packed count 0). Seven Lookout streetlamps are C3D 4978, not 2LIGHTS | PROVEN | `4429113` / `Install_banks_and_startup_videos_exist` / `GameBinFormatTests` (`AuthoredEnvironmentTheme`, `FirstSeenPackedLightCount`) |
| Scene: drain PALSKIN `0x80`/`0x200` after sky. `00B33010` walks slots 8+10 on bit `0x100`, then sky `0x2000`, then slot 14 on `0x80` and Flag1 slot 9 on `0x200`. Static `0x18` stays slot 0 on `0x20`. PALSKIN geometry still submits on `0x100` until type1/Flag1 routing is wired (leftover research, not a new issue) | PROVEN | `f4a1efc` / `Dx9VulkanParityTests` / `EngineLifecycleTests` / `ScenePassTests` / `WorldPipelineTests` |
| Hero: copy GuildArrivalHSP RHSet axes on `006AC910` spawn. Lookout start marker faces +X / +Z. Closed #18. Exist set 465/465; primary C3Ds 193/193 | PROVEN | `ff808b1` / `Install_banks_and_startup_videos_exist` (hero ObjectTransform forward +X) |
| Resources: skip unused 1 m height grid on `00BF4570` cell load. Native draw is stored STB tiles, not bilinear FineHeights. LevelLibrary reuses WLD already parsed at Init World. Open/Present records primary Graphic handles only | PROVEN | `8bfbf39` |
| PALSKIN: bind adult Hero as the script/move/draw Thing. BindScene no longer only accepts `CREATURE_HERO_CHILD`. `006AC910` spawn binds HERO/Hero and seeds `World.Positions`. PlayLoopingAnim `00CC186C` now ApplyInner so ClipKey is set. Thing XYZ writable so WalkTo writes back. c38 dest upload is later GPU path (do not file). Does not close #19 | PROVEN | `27cb7ee` / `Install_banks_and_startup_videos_exist` (Resolve HERO/Hero same as life.Hero) / `PlayLoopingAnim_is_vtbl80_not_PlayAnimation` (`ClipKey`, `InnerApplied`) |
| Render: INNER_SKY does not write oFog. `VSHADER_INNER_SKY` is dp4 oPos + mov oD0, v1. D3D default oFog is 1. Applying 1000/2000 land plane to the 6500 dome made native brightness path black. Land leftover c3 unchanged. No invented ambient or extra x2 | PROVEN | `d7615a6` / `GpuTextureTests` / `ShaderFormatTests` / `WorldGeometryTests` |
| Landscape: INDEX16 strip on the `00BFE050` mesh, DrawIndexed. `00BF4570` DIPs mesh+52/+56 (vtbl+328, type 5, prims+2), not the 72-byte cell. Bit `0x4` is still `00BF71D0`, not this IB. Host unwinds strip to a list; verts unique per cell. W = I on region-local STB | PROVEN | `35f3d20` / `WorldGeometryTests` |
| PlayAVI: native `00628B79` dest and clear leftover frames. Scale to viewport width, center leftover height. Recreate video image when height changes so 640×400/480/360 do not share leftover rows. Clear one frame between startup videos. Closed #11. Does not close #20 | PROVEN | `98c4acc` / `PlayAVI_00628B79_resizes_to_viewport_width_and_centers` / `PlayAvi_rewrites_xmv_to_installed_wmv_and_blocks` |
| `WaitPlayAnimation` `00CC18E0` plays via vtbl+72 (or vtbl+76 if IsTrue arg3) then leftover vtbl+104 | PROVEN | `1eec3bc` / `WaitPlayAnimation_plays_then_polls_vtbl104` |
| FORWARD_TREE PE→WinMain→no-save New Game. Mesh bank `MBANK_ALLMESHES` at `0049E620` / `00A09F20` / `00A27030` / `004BBFD0`. Engine owns map open/close `00B40000` / `00B42750`. Client must not open a second graphics.big dump | PROVEN | `f0fb184` / `Pe_entry_is_crt_not_new_game` / `Install_banks_and_startup_videos_exist` |
| Game vtbl+32 `00416953` is Loading world (no-save `[+90588]` empty skips `004A3200` Loading save). Then `004A1840` WLD/quests. `00B3E820` current handle LookoutPoint; `00B41E50` neighbours (PicnicArea); `00B420F0` name table; `00BDF010` neighbour patch. `00B42530` is STB-miss fallback only | PROVEN | `fc8b261` / same install test + `Pe_entry` constants |
| `00416953` no-save tail: path `+90576` `FinalAlbion.wld` (Leave `0042F44D` / `00415E17`); `0x122EE14` `updatedscenic.wld` is fallback only. `004A1840` then `[0x13B8648]==0` → `0049F180` / `004B4A10`; `004BBC00` is `ret 4`. Not a region | PROVEN | `LoadWorld_00416953_no_save_is_004A1840_then_0049F180` |
| `004184BD` after vtbl+32: `0049BA70` / `00416392` / `004AE9D0` / `default_user.ini` miss / `user.ini` `009EC890` / seed `004167DA`+`+90592`. Not first pump or region | PROVEN | `InitGame_004184BD_after_00416953_reserves_then_user_ini` |
| `user.ini` `ActivateQuest("Gameflow")`: `00419CE0` `[world+56]` `006E7740` vtbl `01260F0C+1104` `00892E80` → `004B4A10` → `00CB5AD0` | PROVEN | `UserIni_009EC890_RunScript_joystick_is_00999230_miss`. Direct `00CB5AD0` from user.ini DISPROVEN. |
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
| Frontend `00595222` widget DIP body | DISPROVEN as DIP | Walk only (`[ui+84]` → `0041AFA0`). First-seen dest `0,0,0,0`. DIP remains `009DA9F0` empty skip |
| `00B324A0` type-0x22 handler vtbl+20 | UNREAD | `d6821b8` dispatch is PROVEN; dest+4=0 first-seen; not memcpy +16020 |
| `0041AC20` dest rect from +204/+248 | PROVEN first-seen 0,0,0,0 | Empty `+376` skips `+204/+208`. `0041AFA0` uses `+248/+264` ctor 0 |
| New Game keyboard N/Enter (host stand-in) | PARTIAL | `DispatchFrontendMessage(15)` is `0059A238` vtbl+32. Enter still queues that message. Click widget id=0 UNREAD |
| `006B8640` / `008889C0` leftover (do not write V0 first-seen) | UNREAD | `006B2CA0` pose is PROVEN (`204a214`). Host `SeedAt(1.6m)` is DISPROVEN as live New Game. Lookout helper FOV 70 from `00A0C130` (`be3339e`). `00A0C130` is a packer (`a6f939a`); ctor look +Z, up `(1,1,1)`. SHOT2 FOV 72 is intro-view leftover — do not collapse into Lookout. Do not close #6 / #13 |
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
script layer. PlayAVI dest vs 1600×900 (#8) is closed. Native
`00628B79` dest + recreate-on-height (#11) is closed (`98c4acc`).
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
`Silk.WindowOptions.DefaultVulkan` 1600×900 (#8) is closed.
Native `00628B79` dest + recreate-on-height (#11) is closed
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
   `006B8640`/`008889C0` leftover UNREAD. Frontend leftover is
   still `00595222` Note and New Game keyboard N/Enter (#14).
   `0041AC20` dest rect and `00B324A0` handler vtbl+20 stay
   UNREAD. New Game submit now walks `0x4`/`0x40`/`0x20`/`0x100`/
   `0x2000` (`676bf63`); cell DIP is `0x40` only (`40037b1`).
   #11 closed (`98c4acc` recreate-on-height + `00628B79` dest).
   #18 closed (`ff808b1` RHSet +X). Lookout EnvironmentTheme is
   `ENVIRONMENT_THEME1` #2346, not `ENVIRONMENT_OAKVALE`. Do not
   collapse Oakvale intro view into Lookout.
2. **GTNG is not an unread file on TLC** — missing skip is PROVEN.
   `00521AE0` loads the current map `.tng`. Remaining UNREAD is
   global-things *use* after `004FDBC0` / `.gtg` parse, plus
   `[record+36]` and catchup-tick writers.
3. **Intro opcodes are already walked.** Phase 2 is apply/runtime
   leftovers on that fiber, not “finish UNREAD tokens.”
4. **README animation-first is the long-term engine list**, not
   today’s master. Animation stays after the clock.

When this page and a ledger disagree, the ledger wins.
