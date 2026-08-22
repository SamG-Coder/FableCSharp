# New Game retail-to-managed parity matrix

This is the dependency-ordered map for the active no-save New Game route. It
does not promote a screenshot, a test, or a convenient managed state to retail
evidence. Native addresses and data-file definitions come from `docs/PARITY.md`,
the assembly listings, and the generated system/script/UI exports. Tests only
verify the managed implementation of rows already supported by that evidence.
The post-cutscene father loop has a dedicated address map in
`docs/runtime/S_QNOVI_FATHER_LOOP_MAP.md`.

Status meanings:

- **MATCH**: the managed owner follows the recovered native owner and order.
- **PARTIAL**: the native call edge is known but a child body or data binding is
  not implemented completely.
- **DIVERGE**: managed code has behavior contradicted by recovered native code.
- **UNREAD**: the retail behavior needed to implement the row is not recovered.

## Ordered control and presentation path

| Order | Retail owner and evidence | Managed owner | Status | Required parity work |
|---:|---|---|---|---|
| 1 | Frontend New Game message 15: `0059A238` → `0059A2DA` → `0042F2A2` | `EngineLifecycle` frontend message/leave stages | MATCH | Keep this as the sole frontend-to-game edge. |
| 2 | Game construct/start `00418DCA` / `004184BD`; load world `00416953` | `EngineLifecycle.InitGame`, `LoadWorld` | MATCH spine / PARTIAL children | Save/editor branches remain outside this no-save slice. |
| 3 | `FinalAlbion.wld` / QST / Startup WAD / initial quests in `004A1840` and `0049F180` | world/QST/bank initialization in `EngineLifecycle` | PARTIAL | Several definition/component bodies remain field-only, but they must not be bypassed with an Oakvale hard-code. |
| 4 | Region load `00500540` → `006C27A0` / `006C2170`; StartOakVale chosen by the active `S_QNOVI` path | `EnsureFirstPlayableRegionLoaded`, `RequestLoadRegion` | PARTIAL | Exact native caller of the catalogue/region-selection edge is still unread. Current selection is dependency-gated, not proof of that caller. |
| 5 | Quest factory `00CD6E27`: `Q_NewOakValeIntro` / `S_QNOVI` / `00DBEF70` | `QuestFactoryTable`, `ScriptScheduler` | MATCH binding / PARTIAL activation | Activation ownership must remain the recovered quest path; never synthesize a second host quest. |
| 6 | Native quest microthread `00A44880` / `00A44660`; `S_QNOVI` run slot `00DABAC0` | `ScriptRuntime.UpdateAtClock`, scheduler, interpreter list | PARTIAL | Basic one-fiber/one-interpreter scheduling is mapped. The complete native quest object is not yet a managed state machine. |
| 7 | `NOVI_LiveFather` factory `00DAC2C0` reaches `00DB86B0` and child `00CBFB7D("CS_OAKVALE_INTRO_FATHER")` | thing activation + generic `ScriptInterpreter` | MATCH edge / PARTIAL opcode bodies | Command order/yields are mapped; several actor animation, movement, spawn/remove and dialogue application bodies remain unread or record-only. |
| 8 | PlayAVI `0088F890` → blocking `006286F0`; DirectShow sample clock owns pacing and skip scan | `WmvPlayer`, `ScriptRuntime.TickAvi`, lifecycle AVI pump | PARTIAL | Architecture and skip keys are mapped. Decoder/presentation behavior must remain isolated from world/script ticks while active. |
| 9 | Screen fade record `00434C00` / `00434870` / `004348D0`; `FadeIn` `00434C90` | `ScriptRuntime` fade record + Vulkan overlay | MATCH record math / PARTIAL submission | Record timing/math follows assembly. Exact 2D record/layer integration still needs parity with the normal display queue. |
| 10 | Dialogue opcodes call script-interface slots `+52`, `+1456..+1472`; some first-seen actor vtable targets are stubs | `DialogueRuntime`, text/audio binding | PARTIAL | Voice lookup, subtitle layout and session completion are not fully recovered. Do not manufacture a global dialogue queue. |
| 11 | Script cameras bind through `00CC9F3A` (`+1648/+1656`); reset `00CC9DF1` (`+1668`, `+1664`) | `ScriptedCamera`, `CameraRuntime`, `ApplyWorldCamera` | PARTIAL, corrected | Script ownership is sticky until reset. Reset releases ownership; `0049E080`/`006B42F0` supplies the next gameplay camera. Implicit zero-camera snapshots are not retail behavior. Spline/rig interpolation bodies remain unread. |
| 12 | Child return `00DB88FD`: clear `+1484(0)`, unmute `+2664(0)`, quest `+82=1` | `ApplyIntroFatherParentContinuation` | MATCH order | Keep presentation clearing local to this parent, never global. |
| 13 | `00DB8925` `+1504(1.0)` → `0088E4F0` → `006E71F0`: `WorldFrame + 15 * seconds`, yield until target | intro parent continuation | MATCH, corrected | This must advance only on world/script ticks, never render dt or wall clock. |
| 14 | `00DB8935/46`: release script camera; next normal camera update owns view | `CameraRuntime.Reset`, `ScriptedCamera.Reset` | MATCH ownership / PARTIAL camera solver | World-camera solver is still an approximation, but stale pre-bind zero state is no longer restored. |
| 15 | `00DB894C–00DB8A3B`: platform branch, `+460` instruction text, `+160` (`00894370`) world-event-journal type `0x12` (`CHEERING`) poll, `+28` yield | `QuestInstructionRuntime`, `WorldEventJournal`, intro parent phases | MATCH owner/order / PARTIAL producer | The gate is timestamped world-event data, never a keypress. The native producer chain is identified; the managed expression/AI producer is not yet connected. |
| 16 | `00DB8A83`: create `HUD_DEED_GOOD_ICON` via `+1308`, store quest `+92`, enable with `+1284(1)` | `QuestHudRuntime`, quest `+92`-equivalent handle | MATCH lifecycle / PARTIAL presentation | The handle is created and enabled in native order. Its independent retail HUD graphics/submission remain a render/UI task. |
| 17 | `00DB8B00/00DB8BBE`: CGameScriptInterface slot 8 (`0089B5B0`) acquires `CScriptGameResourceObjectScriptedThing` modes 3 then 4 around the `SCRIPT_NAME_HERO` test; `00DB8C0C–00DB972F` owns the repeated father/good-deed dialogue, reward, inventory and quest-counter branches | `ScriptedThingLeaseRuntime`, intro parent phases through `FatherGoodDeedLoop` | MATCH acquisition spine / PARTIAL loop | RTTI disproves game-mode/player-control interpretations of modes 3/4. The dialogue/reward/inventory branch bodies and their completion predicates still require address-by-address implementation. |
| 18 | Later childhood quest chain and PostAttack handoff | constants/observations only | UNREAD | Player-control handoff must not be claimed or invented until its native owner is recovered. |

## Player/input ownership

| Retail subsystem | Managed subsystem | Status | Difference |
|---|---|---|---|
| Game player object `004AE940`, action-ready flag, `00416E78` player-interface pump | `EngineLifecycle.PlayerActionReady`, `PlayerInterface` | PARTIAL | Player update exists independently of the cutscene. The parent gate is a separate `CHEERING` world-event-journal query, not player input. |
| Raw event records `009F4ED0`; listener/owner routing `00446330` / `00449990` | `EngineInput`, `PlayerInterface` | PARTIAL | Generic records are consumed, but many retail owner tables/action mappings are unresolved. |
| `00894370` → `008ABED0`: record `+8 == 0x12` and record `+52` in the active time window; `0x12` factory/name is `CHEERING` | `WorldEventJournal` query | MATCH query / PARTIAL producer | Connect the recovered CHEERING expression/AI producer and journal expiry. It is not equivalent to input, “any key”, or automatic completion. |

## World, actors, animation, and audio

| Retail subsystem | Managed subsystem | Status | Difference |
|---|---|---|---|
| Thing construction `004C97B0` / `004CA010`, component attach and per-definition mesh lookup | inserted things + `WorldGeometry` | PARTIAL | Core instances render, but component-driven appearance and several spawn/remove bodies are not implemented. |
| PALSKIN hierarchy/palette `00BD2D90` / `00BCFB00`; animation request path ultimately `0070D580` | palette building + `AnimationRuntime` | PARTIAL | Script `vtbl+72` first-seen target is a stub on this route, while the actual live clip ownership/rate binding is not fully connected. Broken character triangles cannot be treated as a script timing issue. |
| Movement/teleport actor components | `MovementRuntime`, direct transform refresh | PARTIAL | Teleport position is mapped; yaw, locomotion and arrival bodies are incomplete. |
| Music/sound script slots and dialogue voice records | `ScriptAudioRuntime`, host audio | PARTIAL | Track names and mute state exist; retail mixer/event ownership and all voice/subtitle binding do not. |
| Environment tick `006BB990`; authored region environment/TOD definitions drive lighting/fog/sky | `TickEnvironment`, `BindAuthoredEnvironmentTheme`, static `WorldShading` constants | DIVERGE | Managed code advances a counter and records a theme name but does not apply the authored TOD blob. This is a primary cause of wrong colour/lighting and must be solved before shader “look better” edits. |

## Render path kept separate from lifecycle

| Stage | Retail evidence | Managed mapping | Status |
|---|---|---|---|
| Camera matrices/frustum | `00B314E0`, `00B30B50`, `009883F0`, `00BDC2D0` | `ScriptedCamera`, `LandscapeFrustum`, patch submission | PARTIAL |
| Scene/layer order | `00B27D90` → `00B25950` → `00B2AB80`; registered layer order | `ScenePasses`, Vulkan batch order | MATCH across recovered first-scene bits |
| Mesh decode/transforms | C3D layouts, TNG transforms, PALSKIN packing | `MeshFile`, `WorldGeometry` | PARTIAL; actor deformation still defective |
| Shader token math | recovered `VSHADER_*` / `PSHADER_*` programs and constant registers | `LineShaders` | PARTIAL | Some constants are first-seen stand-ins; authored environment is not applied. Do not compensate in GLSL for missing lifecycle data. |
| Textures/samplers | texture formats/mips recovered; sampler state still partly unread | texture cache + Vulkan descriptors/sampler | PARTIAL |
| Depth/raster/blend | recovered CCW, LESSEQUAL and PALSKIN blend; several first writes unread | DX9→Vulkan parity state classes | PARTIAL |
| Fade/video/HUD/particles | independent 2D/layer owners | overlay/video paths; HUD/particles incomplete | DIVERGE |

## Dependency order for implementation

1. Finish the `S_QNOVI` parent state map from `00DB8C0C` to the function exit,
   including dialogue sessions, inventory/reward operations, quest counters and
   world-event predicates. The `0x12` gate is a world-event query, not input.
2. Route that state through the existing world-tick scheduler; never through the
   render loop or a global presentation queue.
3. Complete world-camera ownership after reset and recover any remaining camera
   interpolation used by this route.
4. Bind authored environment/TOD data before changing shader constants.
5. Fix actor component/animation/palette ownership, then validate geometry.
6. Complete the independent HUD/subtitle/audio paths.
7. Only then close Vulkan state gaps (samplers, remaining constants and 2D layer
   submission) against the same captured scene packet.
