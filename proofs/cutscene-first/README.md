# First cutscene / scripted camera after Leave

Investigation only. No production `src` edits.

Do **not** start at startup PlayAVI (`0042EC7C` / `006286F0` ×3).
That is **before** Leave.

Do **not** start at Oakvale / `CAM_OVIF_SHOT2` / `00DB86B0` /
`CS_OAKVALE_INTRO_FATHER`. That path is later leftover
`Q_NewOakValeIntro` (`00DABAC0` → `00DBDE40`), not Leave /
Init Game / first no-save 3D Present.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: `proofs/camera-after-leave/README.md`,
`proofs/newgame-script/README.md`,
`proofs/script-command-map/README.md`,
`proofs/audio-frontend/README.md`,
`docs/runtime/FORWARD_TREE.md` §§4–11, 15,
`docs/status/investigations/2026-08-18-first-scene-things.md`,
`WorldCamera.cs` / `ScriptedCamera.cs` / `QuestFactoryTable.cs` /
`RegionTravel.cs` / `EngineLifecycle.cs`,
`EngineLifecycleTests` (`Init_quests_004B4260_activates_wld_initial_list`,
`Activate_quests_00CB5AD0_starts_factory_scripts`,
`No_save_does_not_activate_Q_NewOakValeIntro`,
`World_camera_006B4900_slots_lerp_into_ScriptedCamera`,
`Retail_0042EC7C_after_AVI_clears_then_inits_frontend`),
`DataCatalogTests` / `WorldSceneTests` (`FirstSeenCallsUseCamera=false`),
ExeIndex `calls-cutscene-runner-00cbfb7d` +
`text-map/listing-00cc0000.txt` + `script-bank/entries-tsv.md`.

---

## Verdict

**After Leave there is no `CCutsceneDef` runner and no `UseCamera` /
`NoLoadUseCamera` bind.**

The first camera that *does* exist is the Init World gameplay stack:
WorldCamera `006B4900` then GameCamera `006FD8C0` helper (70°), seeded
by `006B3FF0` on the first WorldFrame tick. Scene is LookoutPoint +
hero 4299. That is **not** a scripted TNG camera.

The first *named* “cutscene” object after Leave is the empty
`CS_PlayCutscene` factory `00F01760` (`ScriptName==null`). It is a
quest-table row, not a `CCutsceneDef`.

The first later leftover that *is* a scripted camera is
`CS_OAKVALE_INTRO_FATHER` → `00CBFB7D` → `UseCamera CAM_OVIF_SHOT2`.
That is **not** first after Leave. Its inner `PlayAVI dream_sequence`
is also **not** the startup AVI.

---

## Timeline (no-save New Game)

```
0042EC7C retail
  006286F0 ×3 blocking PlayAVI     // STARTUP AVI — before Leave
  0042E98F frontend UI bind
  Init Engine 0042E204
    00B26600 render cam [0x1436EA0]; +12 = 0
  0042DF9E 2D UI flush
  msg 15 → [retail+41]=1
0042F2A2 Leave frontend
  009BE420 clear + 009BEEB0 Present (black)
  no StartCutscene / no 00CBFB7D
0042F491 Init Game → 00418DCA → 004184BD
  Init World 004A6E30
    006B4900 WorldCamera world+24
    0069AE80 GameCameraManager world+48
    006FD8C0 GameCamera world+44 (00A0C130 +Z / 70°)
  00416953 Load world FinalAlbion.wld
  004B4260 START_INITIAL_QUESTS
    … CS_PlayCutscene 00F01760 empty factory …
  user.ini ActivateQuest("Gameflow")
    00CE75B0 Main watcher; S_GF CCutsceneDef DISPROVEN
004189C2 first pumps
  WorldFrame 0→1: 004A5DF3 006B3FF0 seed
  WorldFrame>1: 0041707E → 0049E080 → 006B42F0
  type-1 00CB8220 Gameflow state 0 yields on inactive Q_NewOakValeIntro
  FirstSeenCallsUseCamera=false
```

`CS_ATTRACT_*` / `CS_OAKVALE_INTRO_FATHER` / `UseCamera CAM_OVIF_SHOT2`
/ `00CBFB7D` / Lookout `CAM_GTA_*` binds are **not** on this list.
**PROVEN.**

---

## 1. Startup AVI is not this walk

| Claim | Class | Evidence |
|---|---|---|
| Retail plays three blocking `006286F0` slots before frontend | **PROVEN** | `audio-frontend`; `Retail_0042EC7C_after_AVI_clears_then_inits_frontend` (`complete(intro_comp) < 0042E98F`) |
| Those slots are texture-renderer PlayAVI, not a world / helper bind | **PROVEN** | `camera-after-leave`; PARITY PlayAVI |
| Startup AVI is after Leave | **DISPROVEN** | Leave is `0042F2A2` after the menu click |
| Father-cutscene `PlayAVI dream_sequence_comp.xmv` is the startup AVI | **DISPROVEN** | Lives in `CS_OAKVALE_INTRO_FATHER` (`DataCatalogTests`); `FirstSeenPlayAvi=false` |
| Leave starts another PlayAVI | **DISPROVEN** | Leave is fade / clear / black Present (`0042F2A2` / `009BE420`) |

**Answer:** ignore startup AVI. After Leave the screen is black then
Lookout 3D, not a video.

---

## 2. First camera after Leave is gameplay, not scripted

Constructed at Init World (`004A6E30`), only after Leave:

| Order | VA | Object | Class |
|---|---|---|---|
| 1 | `006B4900` | WorldCamera `world+24` size `0x1970` vtbl `0125D53C` | **PROVEN** construct. Not a TNG eye. |
| 2 | `0069AE80` | GameCameraManager `world+48`. Helper look `+Z` up `+X` FOV 70° | **PROVEN** ctor pack. **UNREAD** as first-Present source. |
| 3 | `006FD8C0` | GameCamera `world+44`. Helper look `+Z` up `(1,1,1)` FOV `0x3E471B48` ≈ 70° (`00A0C130`) | **PROVEN** ctor. |

First seed / apply (still not a cutscene):

| Site | When | Class |
|---|---|---|
| `004A5DF3` → `006B3FF0` | first type-1 tick, WorldFrame 0→1 | **PROVEN** |
| `006B2CA0` | pose dirs `(1,0,0)`, V4 `(−1,0,0)` | **PROVEN** first-seen |
| `0041707E` → `0049E080` → `006B42F0` | first WorldFrame>1 | **PROVEN** first apply |
| Consumed helper pointer | GameCamera+4 vs `[0x1436EA0]+12` | **UNREAD** native |
| Scene | LookoutPoint + `GuildArrivalHSP` hero 4299 | **PROVEN** |
| `UseCamera` / `00B23B50` / `00CC9F3A` | first no-save | **DISPROVEN** (`FirstSeenCallsUseCamera=false`) |

**Answer:** first live camera after Leave is the WorldCamera /
GameCamera follow helper at 70°, not `ScriptedCamera.Bind` and not a
`CAMERA_POINT_*` thing.

---

## 3. Lookout TNG cameras exist and stay gizmos

LookoutPoint TNG after no-save load (`2026-08-18-first-scene-things`):

| Bucket | n | Notes |
|---|---|---|
| `CAMERA_POINT_SCRIPTED` | 12 | `CAM_GTA_SHOT1/3/4*`, `CAM_GTA_OPENING`, `CAM_BB_*`, `P_BHCAM2/3` |
| `CAMERA_POINT_SCRIPTED_SPLINE` | 20 | includes **`CAM_GTA_SHOT2`** near the hero |
| `CAMERA_POINT_SCRIPTED_GLOBAL` | 2 | `GlobalFishingCamera1/2` |

| Claim | Class |
|---|---|
| Those things exist after Leave + region load | **PROVEN** |
| They render as C3D | **DISPROVEN** (`asC3d=False`, host Submit 0 gizmos) |
| First Present binds `CAM_GTA_SHOT2` / any Lookout `CAM_*` | **DISPROVEN** (`FirstSeenCallsUseCamera=false`; seed is `006B3FF0`) |
| `CAM_GTA_SHOT2` == `CAM_OVIF_SHOT2` | **DISPROVEN** | Guild Training Arrival vs Oakvale father intro |

**Answer:** Lookout is full of scripted-camera *things*. None of them
are the first camera after Leave.

---

## 4. First “cutscene” object after Leave is an empty factory

`004B4260` → `00CB5AD0` walks WLD `START_INITIAL_QUESTS`
(`world+172` from `00507C30`):

| Order | Quest | Script | Factory | `CCutsceneDef` first-seen? |
|---:|---|---|---|---|
| 1 | `Q_SunnyvaleMaster` | none | `00CDD550` | no |
| 2 | `PersonalScriptMain` | `S_PSM` | `00CDE2F0` | **DISPROVEN** (`HasStarted("S_PSM")==false`) |
| 3 | `PersonalScript_GlobalThings` | `S_PSGT` | `00CE19A0` | no |
| 4 | `HeroBoasts` | `S_HB` | `00CE6C40` | **DISPROVEN** (`HasStarted("S_HB")==false`) |
| 5 | `V_HeroDolls` | `S_VHDS` | `00E98640` | no |
| 6 | **`CS_PlayCutscene`** | **none** | **`00F01760` size 72 vtbl `012F72D0`** | **DISPROVEN** (`play.ScriptName==null`) |

Then `user.ini` `ActivateQuest("Gameflow")` → `00CE75B0` Main watcher.
`S_GF` as a `CCutsceneDef` at this site is **DISPROVEN**
(`HasStarted("S_GF")==false`). First type-1 `00CE7670` state 0
(`OV_INTRO`) looks up `Q_NewOakValeIntro` and **yields**.

Do **not** `StartCutscene(S_PSM)` from a factory ctor. **PROVEN.**

`CS_PlayCutscene` vtbl+24 is generic fiber `00A44880`, same as
Gameflow / HeroBoasts. That tick is **not** `00CBFB7D`.

**Answer:** the only cutscene-*named* object after Leave is an empty
quest factory. It does not play a def.

---

## 5. `00CBFB7D` callers exist. None are Leave / first pump

ExeIndex `E8` list for `00CBFB7D` is long. Address-order head:

| Site | Pushed name (text-map) | On Leave / first pump? |
|---|---|---|
| `00CE18BB` | `CS_STANDING_STONE` (near `EndGameFocalSite`) | **DISPROVEN** |
| `00CEE9F1` | `CS_FABLE_CREDITS` | **DISPROVEN** |
| … later quest sites … | other `CS_*` | not this walk |
| `00DB88F8` (`00DB86B0`) | `CS_OAKVALE_INTRO_FATHER` | **PROVEN** leftover; **DISPROVEN** as first after Leave |

Frontend / Leave / `0042EC7C` / `0042F2A2` / `004184BD` / `004B4260` /
`00F01760` / first `004189C2` have **no** `E8 00CBFB7D`.

`CS_ATTRACT_1`…`CS_ATTRACT_12` exist in `script.bin` (611 entries,
mostly `CCutsceneDef`). Several use `NoLoadUseCamera CAM_AM_*` /
`CAM_GTA_END`. Frontend / Leave starting any of them is **DISPROVEN**.

**Answer:** interpreter first-seen after Leave is **none**. The empty
factory is the only cutscene-named construct. The first *documented
later* runner is Oakvale father, not Leave.

---

## 6. Leftover first *real* scripted camera (not this walk)

If / when `Q_NewOakValeIntro` is later constructed:

```
00DABAC0 registers NOVI_LiveFather
  TNG CREATURE_HERO_FATHER / NOVI_LiveFather
  00DB8630 → [+52].vtbl+4 = 00DB86B0
00DB86B0 binds Hero / Father
  00CBFB7D("CS_OAKVALE_INTRO_FATHER")
    first line PlayMusic MUSIC_SET_NULL  (FadeOut 0.5,0 special-case skip)
    later UseCamera CAM_OVIF_SHOT2       (00CC9F3A)
    later PlayAVI dream_sequence_comp    (NOT startup AVI)
```

| Flag | Value | Class as *first after Leave* |
|---|---|---|
| `FirstSeenStartsIntroCutscene` | true (as Oakvale leftover pairing) | **LEFTOVER** vs Leave |
| `FirstSeenCallsUseCamera` | false | **PROVEN** on no-save first Present |
| `FirstSeenPlayAvi` | false | **PROVEN** |
| Who activates `Q_NewOakValeIntro` on no-save | — | **UNREAD** (not Leave / not `004B4260` / not `00CE7670`) |

Host `ScriptedCamera` default FOV 72 / `FirstSceneWorld.UseCamera(CAM_OVIF_SHOT2)`
is **LEFTOVER**. Native Lookout helper FOV is 70.

---

## 7. C# vs native

| Host | Native after Leave | Class |
|---|---|---|
| `EngineLifecycle.Camera = new ScriptedCamera()` at Bootstrap | WorldCamera does not exist until Init World | **LEFTOVER** object + 72° default |
| `InitWorldCameras` in `EnterGame` | `004A6E30` `006B4900` / `0069AE80` / `006FD8C0` | **PROVEN** timing |
| `ApplyWorldCamera` / `006B3FF0` seed | first WorldFrame | **PROVEN** |
| `ApplyRendererHelper(hero, V4, +Z, 70°)` | helper pointer **UNREAD** | **PARTIAL** / **DIVERGE** vs ctor helpers |
| `StartCutscene` / `UseCamera` on New Game | none | **PROVEN** absence |
| `ScriptRuntime.StartNewGame` + father cutscene | invented Oakvale | **DIVERGE** |
| `NewGameScript.CutsceneStarted` | observes leftover `CS_OAKVALE_INTRO_FATHER` | **LEFTOVER** vs Leave |

---

## Classifications (short)

1. **Startup AVI as first cutscene after Leave — DISPROVEN.**
   Three `006286F0` slots run on retail **before** Leave.
2. **First camera after Leave — WorldCamera `006B4900` + seed
   `006B3FF0` + apply `0049E080`/`006B42F0`. PROVEN.** Lookout
   follow-helper, 70°, not a TNG `CAM_*`.
3. **First cutscene *object* after Leave — empty `CS_PlayCutscene`
   `00F01760`. PROVEN.** No `CCutsceneDef`, no `00CBFB7D`.
4. **Any `UseCamera` / `NoLoadUseCamera` / `CS_ATTRACT_*` /
   `S_PSM` / `S_GF` on this walk — DISPROVEN.**
5. **Lookout `CAM_GTA_SHOT2` as first scripted camera — DISPROVEN.**
   Gizmo only. Name is not Oakvale `CAM_OVIF_SHOT2`.
6. **`CS_OAKVALE_INTRO_FATHER` / `UseCamera CAM_OVIF_SHOT2` as
   first after Leave — DISPROVEN.** Later leftover. Inner PlayAVI
   is not the startup AVI. Activator on no-save is **UNREAD**.
