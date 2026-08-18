# First script camera command after Leave

Investigation only. No production `src/` edits.

Do **not** start at `UseCamera CAM_OVIF_SHOT2` / `00B23B50` /
`00DB86B0` / `CS_OAKVALE_INTRO_FATHER`. That path is later leftover
`Q_NewOakValeIntro` (`00DABAC0` → TNG `NOVI_LiveFather`).
Leave is `0042F2A2`. First no-save 3D Present does not enter
`00CBFB7D` and therefore does not call any camera verb.

Do **not** treat `ScriptedCamera` ctor FOV 72 / host
`FirstSceneWorld.UseCamera(CAM_OVIF_SHOT2)` as Leave.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE** / **INVENTED**.

Sources:

- `C:\FableCSharp\src\Fable.Game\ScriptedCamera.cs`
- `src/Fable.Game/Scripting/GlobalDispatcher.cs` (camera verbs)
- `src/Fable.Game/Scripting/ExecutionContext.cs` (`CameraRuntime`,
  `CutsceneState.CameraPauseEnabled`)
- `src/Fable.Game/ScriptCommandMap.cs`, `RegionTravel.cs`
- `src/Fable.Game/EngineLifecycle.cs` (`Camera = new()`,
  `ApplyWorldCamera`)
- `docs/runtime/COMMAND_MAP.md` / `COMMAND_MAP.generated.md`
- `tools/Fable.ExeIndex/out/01-sections/script-bank/0481-cs-oakvale-intro-father.md`
- `script-bank/native-sqnovi.md`
- `script-runtime/usecamera-token-00cc9f39-00cc9f39.md`
- `script-runtime/usecamera-name-bind-00cca1aa-00cca1aa.md`
- `script-runtime/usecamera-yield-00cca22c-00cca22c.md`
- `script-runtime/noloadusecamera-token-00cc9e69-00cc9e69.md`
- `script-runtime/noloadusecamera-yield-00cc9f28-00cc9f28.md`
- `script-runtime/noloadusecamera-yield-helper-00cc907d-00cc907d.md`
- `script-runtime/usecamera-ebp-37-ctor-00cbfd53-00cbfd53.md`
- `script-runtime/docamerapreloading-token-00cc86d0-00cc86d0.md`
- `proofs/camera-after-leave/README.md`, `cutscene-first`,
  `script-global-cmds`, `script-command-map`
- `WorldSceneTests` / `DataCatalogTests`
  (`FirstSeenCallsUseCamera=false`,
  `FirstSeenUseCameraYields=true`,
  `FirstSeenNoLoadUseCameraYields=true`,
  `FirstSeenDoCameraPreloadingDoesNotYield=true`)
- `ScriptRuntimeArchitectureTests`
  (`CameraPause_false_makes_UseCamera_continue`,
  `ResetCamera_restores_gameplay_snapshot_after_UseCamera`)
- `CameraMatrixParityTests`
  (`Scripted_bind_survives_until_host_apply_stomps`)

---

## Verdict

**Leave does not execute a script camera command.**

`ScriptedCamera` is the host analog of the *live game camera
object* that `UseCamera` / `NoLoadUseCamera` bind through
engine vtbl+16 `00B23B50` (and context `vtbl+1648` / `vtbl+1656`).
After frontend Leave the native pump constructs **WorldCamera**
(`006B4900`) and seeds a Lookout follow-helper at 70°. It does
not run `00CBFB7D`, so it never matches a camera token.

| Question | Answer | Class |
|---|---|---|
| First script camera *verb* after Leave? | **none** — runner not on the tree | **PROVEN** |
| First live 3D camera after Leave? | WorldCamera `006B4900` / seed `006B3FF0` / apply `006B42F0` — 70° helper, not a TNG `CAM_*` | **PROVEN** |
| First leftover *camera-named* line if father later starts? | `CameraPause FALSE` (`00CC71F1`) | **PROVEN** leftover; **not** a bind |
| First leftover *preload*? | `DoCameraPreloading` (`00CC86D0` / `00CBF29F`) | **PROVEN** leftover; names only |
| First leftover *eye bind*? | `NoLoadUseCamera CAM_OVI_ID_STANDUP` (`00CC9E69` / `00CC907D`) | **PROVEN** leftover |
| First leftover `UseCamera`? | later `UseCamera CAM_OVIF_SHOT2` (`00CC9F39` / `00B23B50`) | **PROVEN** leftover; **not** first bind |
| Host `ScriptedCamera` from Bootstrap (FOV 72)? | unused on Leave / first Present | **LEFTOVER** |

Treating `CAM_OVIF_SHOT2` as first scripted camera after Leave
is **INVENTED**.

---

## Timeline (no-save New Game)

```
0042F2A2 Leave frontend
  no 00CBFB7D / no CameraPause / no UseCamera
0042F491 Init Game → 004184BD
  Init World 004A6E30
    006B4900 WorldCamera world+24
    0069AE80 GameCameraManager world+48
    006FD8C0 GameCamera world+44 (00A0C130 +Z / 70°)
  00416953 Load FinalAlbion.wld
    00CD6E27 bind Q_NewOakValeIntro / S_QNOVI   BIND ONLY
  004B4260 START_INITIAL_QUESTS
    CS_PlayCutscene 00F01760 empty              // no CCutsceneDef
004189C2 first pumps
  WorldFrame 0→1: 004A5DF3 006B3FF0 seed
  WorldFrame>1: 0041707E → 0049E080 → 006B42F0
  FirstSeenCallsUseCamera=false
later (E8 caller UNREAD — not Leave)
  00DABAC0 registers NOVI_LiveFather
  00DB86B0 → 00CBFB7D("CS_OAKVALE_INTRO_FATHER")
    [2]  CameraPause FALSE
    [7]  DoCameraPreloading
    [14] NoLoadUseCamera CAM_OVI_ID_STANDUP     // first leftover bind
    [17] UseCamera CAM_OVIF_SHOT2               // later leftover
```

`00CC9F3A` / `00B23B50` / `CAM_OVIF_SHOT2` are **not** on the
Leave list. **PROVEN.**

---

## 1. What `ScriptedCamera.cs` is

`C:\FableCSharp\src\Fable.Game\ScriptedCamera.cs`

Host live camera owned by script / TNG helper state. Comment
claims first-seen bind is TNG `CAM_OVIF_SHOT2` via `00B23B50` /
`00B314E0`. That is **PROVEN** as the leftover Oakvale *script*
bind and **DISPROVEN** as first after Leave.

Pinned constants (opcode sites, not Leave execute):

| Host | Native | Role |
|---|---|---|
| `CutsceneStart` `00DB86B0` | father start | leftover runner push |
| `CutsceneRunner` `00CBFB7D` | interpreter | leftover |
| `UseCameraPreload` `00CBF29F` | name walk | `DoCameraPreloading` apply |
| `UseCameraActivate` `00CC9F3A` | token+1 of `push "UseCamera"` at `00CC9F39` | leftover activate |
| `PreloadVtbl` 1648 | name bind | snap |
| `ActivateVtbl` 1656 | thing bind | when the line has a thing handle |

Default `FovDegrees = IntroCameraFovDegrees` (72). That is
SHOT2 spline FOV `0.2` turns × 360, **not** GameCamera 70°.
Ctor `ScriptCameraActive=false`, `Playing=false`, `ActiveName=""`.

Methods vs native:

| Method | Native | Leave? |
|---|---|---|
| `Bind` / `UseCamera(things,name)` | `00CC9F3A` / `00CC9E6A` TNG lookup then helper bind | **DISPROVEN** |
| `Reset` | `00CC9DF1` `vtbl+1668(0)` then `vtbl+1664`; `jmp 00CD17FD` | unused; **not** on father def |
| `BeginTransition` / `EndTransition` | `Playing` ↔ leftover `vtbl+1672` busy | snap bind keeps `Playing=false` |
| `ApplyManagerOutput` | `006B42F0` writes `+6296/+6312/+6328` | **PROVEN** after Leave (WorldCamera, not a script verb) |
| `ApplyRendererHelper` | `00B314E0` helper `+0/+12/+24` | first-Present **PARTIAL** (helper pointer UNREAD) |

`LookAt = position + normalised look × 8` is host
`RegionTravel.TryCameraFromThing`. Native spline/look body on
the first leftover bind is **PARTIAL** (TNG name proven;
exact helper pack **PARTIAL**).

---

## 2. First script camera command after Leave — none

| Claim | Class | Evidence |
|---|---|---|
| `00CBFB7D` on Leave / Init Game / first pumps | **DISPROVEN** | no E8; empty `CS_PlayCutscene`; Gameflow yield |
| Any camera token (`CameraPause` … `UseCamera`) | **DISPROVEN** | `FirstSeenCallsUseCamera=false`; same runner |
| Lookout TNG `CAM_GTA_*` exists after region load | **PROVEN** | gizmos; `asC3d=False` |
| First Present binds any Lookout `CAM_*` | **DISPROVEN** | seed is `006B3FF0`, not `00B23B50` |
| WorldCamera construct after Leave | **PROVEN** | `006B4900` at Init World |
| Host `EngineLifecycle.Camera = new ScriptedCamera()` | **LEFTOVER** | Bootstrap; FOV 72; frontend / first Present do not consume it as a script bind |

`ApplyWorldCamera` (`0049E080` / `006B42F0`) writes that same
host object after WorldFrame>1. That is gameplay follow, not
`ScriptedCamera.Bind`. **PROVEN.**

---

## 3. Leftover first *camera* slice (not Leave)

Dump `script-bank/0481-cs-oakvale-intro-father.md`. Pinned by
`WorldSceneTests` (`New_game_intro_runs_through_generic_script_runtime`).

All camera *verbs* on that def, in execute order:

| PC (def+60) | Line | Token | Apply | Eye? | Host |
|---|---|---|---|---|---|
| 2 | `CameraPause FALSE` | `00CC71F1` `0x012C2058` | `[ebp-37]=0` via `00CBEE0C` IsFalse | **no** | `CameraPauseEnabled=false` Continue |
| 7 | `DoCameraPreloading` | `00CC86D0` `0x012C18C8` | `00CBF29F(dl=0)` then `vtbl+1568`; no args so skip `vtbl+1560` | **no** | collect later `UseCamera` names into `Preloaded` |
| 14 | `NoLoadUseCamera CAM_OVI_ID_STANDUP` | `00CC9E69` `0x012C1DF8` | TNG bind; yield `00CC9F28` | **yes** | `Camera.Bind` |
| 17 | `UseCamera CAM_OVIF_SHOT2` | `00CC9F39` `0x012C18AC` | TNG bind `vtbl+1648`/`1656` → `00B23B50` | **yes** | `Camera.Bind` |
| later | `UseCamera CAM_OVIF_SHOT3` | same | same | yes | same |
| later | `NoLoadUseCamera CAM_OVIF_SHOT4` / `SHOT3` / `SHOT6START` / `SHOT6` | same NoLoad | same | yes | same |
| later | `UseCamera CAM_OVIF_SHOT7` (twice) | same | same | yes | same |

Not on the father def (do not invent as first leftover):

`ResetCamera` `WaitForCamera` `WaitForMessageCamera`
`CameraLookAt` `CameraLookBetween` `CameraFOVLookBetween`
`CameraFOVLookBetweenPos` `CameraPath` `CameraRotateThing`
`CameraRig` `UseCameraFOVMarkerList` `CameraShake`
`CameraEffect` `CameraPreload` `DebugCamera`
entity `LookToCamera`.

`DoCameraPreloading` **does** collect `CAM_OVIF_SHOT2` (and later
`UseCamera` names) into `vtbl+1648` preload. Host
`WorldSceneTests` asserts `PreloadedCameras` contains
`CAM_OVIF_SHOT2` while `ExecutedVerb("UseCamera")` is still
false and `ActiveName` is still the *test setup* SHOT2 bind.
That preload is **not** a live helper bind. **PROVEN.**

`LookToCamera` is Entity (`00CC3CE4` / `vtbl+1996`). Not a
`ScriptedCamera` pose write. Not on father. **LEFTOVER** family,
**DISPROVEN** as this walk.

`DebugCamera` is a native token (`0x012C2040`) with **UNREAD**
handler (no `All` apply). Not on father.

---

## 4. `CameraPause` is the first leftover camera *command*

Ctor `00CBFD53`: `mov [ebp-37],1`. Default pause **on**.

`CameraPause FALSE` (`00CC71F1` apply `00CC7241`):
`IsFalse(arg0)` → `[ebp-37]=0`. CompleteNow. No `vtbl+28`.
Does not call `00B23B50`. Does not touch `ScriptedCamera`
pose / FOV / `ScriptCameraActive`.

That flag gates **both** leftover binds:

```
UseCamera yield 00CCA22C:
  cmp [ebp-37], 0
  je  skip yield          ; pause off → CompleteNow
  cmp [ebp+103], 0
  je  skip vtbl+28
  call [eax+28]

NoLoadUseCamera yield 00CC9F28:
  cmp [ebp-37], 0
  je  00CD17FD            ; same: pause off → CompleteNow
  jmp 00CC907D            ; else [ebp+103] then vtbl+28
```

Host (`GlobalDispatcher`):

```
if (!CameraPauseEnabled || !YieldEnable) Continue
else YieldOnce vtbl+28
```

After leftover `CameraPause FALSE`, both binds Continue.
`CameraPause_false_makes_UseCamera_continue` pins this.
`FirstSeenUseCameraYields` / `FirstSeenNoLoadUseCameraYields`
are the **opcode capability** when pause is still on, not
the leftover father path.

Host treats `UseCamera` and `NoLoadUseCamera` as one `if`.
Native tokens / yield joins differ (`00CC9F39` vs `00CC9E69`,
`00CCA22C` vs `00CC907D`). Match on leftover father (pause
off) is **EQUIVALENT**. Spline / extra float args on
`UseCamera` (`[ebp-172]` default `0xBF800000` = −1) are
**UNREAD** on host Bind.

---

## 5. First leftover *bind* is `NoLoadUseCamera`, not SHOT2

`NoLoadUseCamera CAM_OVI_ID_STANDUP` (`RegionTravel.IntroStandupCamera`):

- empty / null name → `jmp 00CD17FD` skip
- TNG lookup by `ScriptName`
- bind through the same context camera as `UseCamera`
- then yield gate above

SHOT2 (`IntroFirstSeenCamera`) is **later**, after
`PlayMusic MUSIC_SET_OAKVALE`, standup bind, `FadeIn`,
two `GamePause`s, `Speak`, `InteractiveSpeak`.
`WorldSceneTests` asserts `ActiveName == CAM_OVI_ID_STANDUP`
before `UseCamera CAM_OVIF_SHOT2` runs.

SHOT2 TNG is `CAMERA_POINT_SCRIPTED_SPLINE` on
`StartOakValeWest`. FOV property `0.2` turns → 72°.
`00B23B50` (`LandscapeFrustum.BindSource`) is the engine
helper bind. First no-save does **not** call it.

Host tests that call `camera.UseCamera(things, SHOT2)`
*before* `StartNewGame` (`WorldSceneTests` line ~798,
`FirstSceneWorld.Build`) are **LEFTOVER** setup, not Leave.

---

## 6. `ScriptedCamera` vs WorldCamera on the same object

Host uses **one** `EngineLifecycle.Camera` for both:

1. Leftover script `Bind` (`ScriptCameraActive=true`, FOV 72)
2. Every `ApplyWorldCamera` (`006B42F0` / helper 70°)

`ApplyWorldCamera` does **not** read `ScriptCameraActive`.
`Scripted_bind_survives_until_host_apply_stomps` pins:
after `Bind` + `ApplyRendererHelper` the flag stays true
but pose/FOV become Lookout helper / 70°.

Native: `00B23B50` sticks `camera+12` until the next bind;
WorldCamera lerp does not unbind. Host rewrite every frame
is **DIVERGE** on a *live* leftover `UseCamera`, and
**irrelevant** on Leave (no script bind).

`Reset` restores the first `Bind` snapshot. Father never
issues `ResetCamera`. First leftover unbind is **UNREAD**
(who, if anyone, calls `vtbl+1664` after Oakvale).

---

## 7. C# vs native after Leave

| Host | Native after Leave | Class |
|---|---|---|
| `new ScriptedCamera()` at lifecycle ctor | WorldCamera does not exist until Init World | **LEFTOVER** object |
| default FOV 72 | GameCamera helper ~70° (`0x3E471B48`) | **LEFTOVER** |
| `FirstSceneWorld.UseCamera(SHOT2)` | no `00B23B50` | **DIVERGE** vs Leave |
| `GlobalDispatcher` `UseCamera` / `NoLoadUseCamera` | unused | **LEFTOVER** vs Leave |
| `InitWorldCameras` / `ApplyWorldCamera` in `EnterGame` | `006B4900` / `006B3FF0` / `006B42F0` | **PROVEN** timing |
| `ApplyWorldCamera` ignores `ScriptCameraActive` | later DIVERGE if leftover bind is live | not a Leave bug |

---

## Classifications (short)

1. **First script camera command after Leave — none. PROVEN.**
   Runner `00CBFB7D` is not on the tree. `ScriptedCamera.Bind`
   is unused.
2. **First 3D camera after Leave — WorldCamera construct /
   seed / apply. PROVEN.** Lookout follow-helper 70°, written
   *into* the host `ScriptedCamera` object without a script
   verb.
3. **`UseCamera CAM_OVIF_SHOT2` as first after Leave —
   DISPROVEN / INVENTED.** Leftover father line after standup.
4. **First leftover camera *command* — `CameraPause FALSE`.
   PROVEN leftover.** Flag only. No eye.
5. **First leftover camera *bind* — `NoLoadUseCamera
   CAM_OVI_ID_STANDUP`. PROVEN leftover.** `DoCameraPreloading`
   is names only.
6. **`ScriptedCamera` ctor FOV 72 / Bootstrap instance —
   LEFTOVER** vs Leave. Native first Present is 70°.
7. **Who later calls `00CBFB7D` on no-save — UNREAD**
   (not Leave / not `004B4260` / not `00CE7670`).
   Do not invent `ActivateQuest(Q_NewOakValeIntro)`.
8. **Spline play / `Playing=true` on leftover snap bind —
   DISPROVEN** (`UseCamera` snap, `vtbl+1672` idle). Path /
   rig / look-between would set it; those verbs are not on
   father.
