# Camera / cutscene first use after Leave Frontend

Investigation only. No production `src` edits.
Do **not** start at Oakvale / `CAM_OVIF_SHOT2` / `00DB86B0`.
That path is later `Q_NewOakValeIntro` (`00DABAC0` → `00DBDE40`),
not Leave / Init Game / first no-save 3D Present.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** / **LEFTOVER**.

Sources: `docs/runtime/FORWARD_TREE.md` §§4–11, 15;
`docs/status/investigations/B-camera-matrices.md`;
`docs/status/investigations/2026-08-18-camera.md`;
`WorldCamera.cs` / `ScriptedCamera.cs` / `EngineLifecycle.cs`;
`EngineLifecycleTests` (`New_game_is_leave_frontend_then_FinalAlbion_wld`,
`Pump_004166E2_is_009E1BC0_minus_game_plus96`,
`After_WorldFrame_gt_1_00417001_is_0041707E_then_004AEA70_skip`,
`Activate_quests_00CB5AD0_starts_factory_scripts`);
ExeIndex dumps `00B23B50` / `00B31700` / `00CBFB7D` / `00F01760`.

---

## Timeline (no-save New Game)

```
0042EC7C retail
  PlayAVI slots (texture renderer, not world cam)
  Init Engine 0042E204
    00B26340 engine vtbl 012A0F3C
    00B26600 constructs render cam [0x1436EA0] ctor 00B31700; +12 = 0
  Init frontend 0042EF6F
  0042DF9E 2D UI flush → 009BEEB0
  msg 15 → [retail+41]=1
0042F2A2 Leave frontend          // not 00DBDE40
  009BE420 clear + 009BEEB0 Present (black)
0042F491 Init Game → 00418DCA → 004184BD
  Init World 004A6E30
    006B4900 WorldCamera world+24
    0069AE80 GameCameraManager world+48
    006FD8C0 GameCamera world+44 (00A0C130 helper +Z / 70°)
  00416953 Load world FinalAlbion.wld
  004B4260 START_INITIAL_QUESTS (CS_PlayCutscene factory only)
004189C2 first pumps
  WorldFrame 0→1: 004A5DF3 006B3FF0 seed
  WorldFrame<=1: skip 0049E080 and 00435F70
  WorldFrame>1: 0041707E → 0049E080 → 006B42F0
  first 3D Present still gated (004AEA70 often skip)
```

`Q_NewOakValeIntro` / `S_QNOVI` / `UseCamera CAM_OVIF_SHOT2` are
**not** on this list. **PROVEN**.

---

## 1. Camera during frontend besides 2D UI?

| Claim | Class | Evidence |
|---|---|---|
| Frontend Present is 2D UI (`0042DF9E` / `00530260` / `VSHADER_2D_SPRITE` / `009DA9F0`) | **PROVEN** | FORWARD_TREE §4; `PumpFrontendFrame`; `Frontend_PRESS_START_*` |
| PlayAVI is `Fable Texture Renderer Filter`, not a world/helper bind | **PROVEN** | PARITY PlayAVI; `SilkEngineHost` video path |
| `CS_ATTRACT_*` exists in `script.bin` | **PROVEN** | `DataCatalogTests` |
| Frontend / Leave starts `CS_ATTRACT_*` or any `CCutsceneDef` | **DISPROVEN** | No `StartCutscene` on retail pump; `00CBFB7D` not on `0042EC7C` |
| WorldCamera / GameCamera / GameCameraManager live during frontend | **DISPROVEN** | Alloc only in Init World after Leave (`004A6E30`) |
| `UseCamera` / `00B23B50` during frontend | **DISPROVEN** | Zero E8; vtbl-only; first no-save `FirstSeenCallsUseCamera=false` |
| Render object `[0x1436EA0]` exists after Init Engine (still retail, before Leave) | **PROVEN** | `00B26600` / `00B31700`; `2026-08-18-camera.md` |
| That object is an *active* 3D view during frontend | **DISPROVEN** | `+12` zeroed; no `00B23B50`; frontend DIP is 2D; first-seen `009DA9F0` empty skip |
| Host `EngineFrame.Camera` on frontend frames | **LEFTOVER** | `Camera = new ScriptedCamera()` at lifecycle ctor; `BuildFrame` always passes it. Native frontend does not consume WorldCamera/ScriptedCamera. FOV default 72 is SHOT2 leftover. |

**Answer:** no active 3D / cutscene camera during frontend. Only 2D UI (plus AVI as a texture). The DX9 render-camera *object* is constructed at Init Engine with a null helper.

---

## 2. First 3D camera after Leave / Init Game / world load

Not Oakvale. First *constructed* 3D camera state is Init World:

| Order | VA | Object | Class |
|---|---|---|---|
| 1 | `006B4900` | WorldCamera `world+24` size `0x1970` vtbl `0125D53C`. Slots ctor axis `(1,0,0)`, weight 0.2, timer −1 | **PROVEN** construct. Not a world eye. |
| 2 | `0069AE80` | GameCameraManager `world+48`. Helper `(0,0,0)` look `+Z` up `+X` FOV 70° turns | **PROVEN** ctor pack. **UNREAD** as first-Present source. |
| 3 | `006FD8C0` | GameCamera `world+44`. Helper look `+Z` up `(1,1,1)` FOV `0x3E471B48` ≈ 70° | **PROVEN** ctor. **UNREAD** whether `+4` is `[0x1436EA0]+12` on first Present. |

First *seed* (still not Oakvale):

| Site | When | Class |
|---|---|---|
| `004A5DF3` → `006B3FF0` | first `004A5A40` type-1 tick, WorldFrame 0→1 | **PROVEN**. Host `TickWorld` / `SeedHero`. |
| `006B2CA0` | inside seed; dirs `(1,0,0)`, V4 `(−1,0,0)` | **PROVEN** first-seen. |
| `006B3030` / `006B3B80` | spring clamp; tick skip `+460=0` `+24=−1` | **PROVEN** first-seen skip of rotate / V0 write. |
| Host extra seed in `SpawnHero` / `LoadFromFirstRealRegion` | after `00501450` Lookout | **PARTIAL** vs native: `006B3FF0` also has an E8 from `006B42F0`; first pump seed is `004A5DF3` **before** `00501450` (0 E8 on first `004189C2`). |

First *apply* / first 3D Present:

| Site | When | Class |
|---|---|---|
| `00417001` WorldFrame≤1 | skip camera body **and** `00435F70` | **PROVEN** |
| `0041707E` → `0049E080` → `006B42F0` | first WorldFrame>1 | **PROVEN** first apply. t first-seen 0 after clamp. A=B. |
| `00435F70` on that same first WorldFrame>1 | often skip (`004AEA70`) | **PROVEN** in `After_WorldFrame_gt_1_*` (`DisplayPresentSkipped`) |
| Consumed first-Present helper | GameCamera+4 vs unread bind vs host hero+V4 | **UNREAD** native pointer. Host `ApplyRendererHelper(hero, V4, +Z, 70°)` is **policy / DIVERGE** vs both ctor helpers. |
| Scene of first no-save 3D | LookoutPoint + hero 4299, **not** `StartOakValeWest` / SHOT2 | **PROVEN** (B §0). `00501450` itself is **not** a first-pump callee. |

**Answer:** first 3D camera *object* is WorldCamera `006B4900` at Init World. First *live* seed is `006B3FF0` on the first WorldFrame tick. First *apply* is `0049E080` after WorldFrame>1. First *drawn* 3D is Lookout follow-helper, not Oakvale SHOT2.

---

## 3. Cutscene manager first-seen

There is no cutscene runner on Leave / Init Game / WLD activate.

| Object | Role after Leave | Class |
|---|---|---|
| `006E6150` Init Script Conversation Manager | conversation list, not `00CBFB7D` | **PROVEN** construct; **UNREAD** as a cutscene player |
| `00CD52D0` / `00CB5AD0` / `004B4260` | WLD `START_INITIAL_QUESTS` | **PROVEN** |
| `CS_PlayCutscene` factory `00F01760` size 72 vtbl `012F72D0` | empty quest object; **no** `CCutsceneDef` | **PROVEN**. `play.ScriptName==null`. |
| `00CB8690` START_SCRIPT_DATA | token parse, not runner | **PROVEN** |
| `PersonalScriptMain` / `S_PSM` | factory only; `HasStarted("S_PSM")==false` | **PROVEN**. Do not `StartCutscene(S_PSM)` from ctor. |
| `Gameflow` / `S_GF` | `00CE75B0` Main watcher; `S_GF` CCutsceneDef **not** this site | **DISPROVEN** as first runner |
| `00CBFB7D` | first-seen later from `NOVI_LiveFather` → `00DB86B0` (`CS_OAKVALE_INTRO_FATHER`) | **PROVEN** as Oakvale leftover. **Not** first after Leave. |
| `UseCamera` / `00B23B50` | not first no-save | **PROVEN** (`FirstSeenCallsUseCamera=false`) |

**Answer:** first-seen “cutscene manager” after Leave is the empty `CS_PlayCutscene` factory. The interpreter `00CBFB7D` is **not** first-seen here.

---

## 4. C# that sets a world camera during frontend

| Site | What it does | Class |
|---|---|---|
| `EngineLifecycle.Camera = new()` | `ScriptedCamera` exists from Bootstrap, FOV `IntroCameraFovDegrees=72` | **LEFTOVER**. Native WorldCamera does not exist yet. 72 is SHOT2 leftover. |
| `EngineLifecycle.WorldCamera/GameCamera/GameCameraManager` fields | C# objects exist; `Construct()` only in `InitWorldCameras` after Leave | **EQUIVALENT** timing if `WorldCameraPresent` is the gate. Field `new()` is host-only. |
| `BuildFrame()` always passes `Camera` | frontend `EngineFrame` carries a 3D camera | **LEFTOVER**. Native frontend Present is 2D batch + clear. |
| `SilkEngineHost.Draw` uses `_frame.Camera` | would apply 3D WVP if verts exist | **LEFTOVER** on frontend frames (verts empty; batch path ignores cam). |
| `RequestNewGame` / `PumpFrontendFrame` | no `InitWorldCameras` / `ApplyWorldCamera` / `UseCamera` | **PROVEN** absence |
| `InitWorldCameras` | only `EnterGame` after Leave | **PROVEN** |
| `FirstSceneWorld.UseCamera(CAM_OVIF_SHOT2)` | Oakvale contract helper | **LEFTOVER** vs live New Game. Do not treat as first camera after Leave. |
| `ApplyWorldCamera` stomps `ScriptCameraActive` | later DIVERGE | not a frontend issue |

---

## Classifications (short)

1. **Frontend 3D camera besides 2D UI — DISPROVEN.** Render cam object exists after Init Engine with null helper. Attract cutscenes exist on disk, unused.
2. **First 3D camera after Leave — WorldCamera `006B4900` then seed `006B3FF0` then apply `0049E080`/`006B42F0`. PROVEN.** Scene is Lookout, not Oakvale. Live helper pointer **UNREAD**. Host hero+V4+70° **DIVERGE** vs ctor helpers.
3. **Cutscene manager first-seen — `CS_PlayCutscene` `00F01760` empty factory. PROVEN.** `00CBFB7D` / `S_PSM` / `S_GF` / Oakvale father **DISPROVEN** as this site.
4. **C# world camera during frontend — LEFTOVER** (`ScriptedCamera` + 72° on every `EngineFrame`). Native does not bind a world camera until Init World after Leave.
