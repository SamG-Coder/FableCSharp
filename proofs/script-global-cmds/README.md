# First `GlobalDispatcher` verbs after Leave

Investigation only. No production `src/` edits.

Do **not** start at `00DB86B0` / `CS_OAKVALE_INTRO_FATHER` /
`PlayMusic MUSIC_SET_NULL`. That is later leftover
`Q_NewOakValeIntro` (`00DABAC0` → TNG `NOVI_LiveFather`).
Leave is `0042F2A2`. First no-save 3D Present does not enter
`00CBFB7D` and therefore does not call `GlobalDispatcher`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: `src/Fable.Game/Scripting/GlobalDispatcher.cs`,
`ScriptInterpreter.cs` / `ScriptLine.cs`,
`ScriptCommandMap.cs`, `RegionTravel.cs`;
`docs/runtime/COMMAND_MAP.md`;
`tools/Fable.ExeIndex/out/01-sections/script-bank/0481-cs-oakvale-intro-father.md`,
`native-sqnovi.md`;
`proofs/script-interpreter/README.md`,
`proofs/script-command-map/README.md`,
`proofs/camera-after-leave/README.md`,
`proofs/newgame-script/README.md`;
`DataCatalogTests` / `WorldSceneTests`
(`FirstSeenCallsUseCamera=false`,
`FirstSeenFadeSpecialCaseRuns=false`,
`FirstSeenPlayMusicDoesNotYield=true`).

---

## Verdict

**Leave does not execute a `GlobalDispatcher` verb.**

`GlobalDispatcher` is the host analog of the *no-target* arm of
runner `00CBFB7D` (`ebx` null → `jmp 00CD17FD`). After frontend
Leave the native pump starts quest factories (`004B4260` /
`00CB5AD0`), not this if-chain.

`PlayMusic`, `FadeOut`, `CameraPause`, `UseCamera` /
`NoLoadUseCamera` are **LEFTOVER** father lines. They are the
first *global* slice *if* that leftover later starts. They are
**DISPROVEN** as first-seen after Leave.

| Question | Answer | Class |
|---|---|---|
| First `GlobalDispatcher` call after Leave? | **none** — runner not on the tree | **PROVEN** |
| First leftover *global* lines if father later starts? | `PlayMusic` → `FadeOut` → `CameraPause` | **PROVEN** leftover |
| FadeOut 0.5,0 *special-case* (`00CBFDD0`) on that leftover? | skipped (`commands[0]` is PlayMusic) | **PROVEN** leftover |
| First leftover *camera bind*? | later `NoLoadUseCamera CAM_OVI_ID_STANDUP`, then `UseCamera CAM_OVIF_SHOT2` | **PROVEN** leftover |
| First 3D camera after Leave? | WorldCamera `006B4900` / seed `006B3FF0` — not a script verb | **PROVEN** |
| Host `GlobalDispatcher` after `DispatchFrontendMessage(15)`? | unused | **LEFTOVER** vs Leave |

---

## Timeline (no-save New Game)

```
0042F2A2 Leave frontend
0042F491 Init Game → 004184BD
  Init World 004A6E30
    006B4900 WorldCamera            // not UseCamera
  00416953 Load FinalAlbion.wld
    00CD6E27 00CB5C90 bind Q_NewOakValeIntro / S_QNOVI   BIND ONLY
    00507C30 START_INITIAL_QUESTS → world+172
  004B4260 Init Quests
    00CB5AD0 six WLD names
    CS_PlayCutscene 00F01760 empty   // no CCutsceneDef
    00CB8690 START_SCRIPT_DATA parse // not 00CBFB7D
  user.ini ActivateQuest("Gameflow")
    00CE75B0 Main watcher
    S_GF CCutsceneDef DISPROVEN
004189C2 first pumps
  00CB8220 → 00A44880
  00CE7670 state 0 yields on Q_NewOakValeIntro miss
```

`00DB86B0` → `00CBFB7D` → `GlobalDispatcher` is **not** on this
list. **PROVEN.**

---

## 1. How host gets to `GlobalDispatcher`

`C:\FableCSharp\src\Fable.Game\Scripting\GlobalDispatcher.cs`

`ScriptInterpreter.RunUntilYield`:

```
PC==0 → TryFadeSpecialCase          // 00CBFDD0, not a dispatcher verb
ScriptLine.Parse
  last '.' on unquoted head
  Target set → Entity else Global   // native ebx null vs 00CC707C
GlobalDispatcher.Dispatch           // this file
  if Eq(verb) …                     // host order ≠ 00BFEAF8 chain
  else Blocked UNKNOWN UNREAD
```

`ScriptLine.Family` is **no-target = Global**. `PlayMusic`,
`FadeOut`, `CameraPause`, `DoCameraPreloading`, `PlayAVI`,
`MuteSounds`, `NoLoadUseCamera`, `UseCamera`, `FadeIn`,
`GamePause` are all Global. `Hero.Teleport` /
`Father.LookToThing` are Entity (`EntityDispatcher`).

Host if-chain **starts** at `PlayMusic`. That is a C# listing
order, not native compare order. Native first *compared* token
inside the loop is `.WaitTask` `00CC0783`. **PROVEN** as
compare order, **not** as Leave execute.

Unknown verbs: host `CommandResult.Blocked("UNKNOWN")`.
**EQUIVALENT** to “do not no-op.” Native fall-through after
the if-chain is **UNREAD** as a first-seen Leave site
(never reached).

---

## 2. PlayMusic / fade / camera after Leave?

**No.** Those three families do not run on this path.

| Claim | Class | Evidence |
|---|---|---|
| `00CBFB7D` on Leave / Init Game / first pumps | **DISPROVEN** | no E8; empty `CS_PlayCutscene`; Gameflow yield |
| Script `PlayMusic` on frontend or Leave | **DISPROVEN** | `proofs/audio-frontend`; first `PlayMusic` is leftover father |
| Fade overlay from a script verb on first Present | **DISPROVEN** | no `FadeOut` / `FadeIn` opcode; Lookout follow-helper |
| `UseCamera` / `NoLoadUseCamera` / `00B23B50` first no-save | **DISPROVEN** | `FirstSeenCallsUseCamera=false` |
| WorldCamera construct after Leave | **PROVEN** | `006B4900` at Init World; not a Global verb |
| Host `EngineLifecycle.Camera` on frontend frames | **LEFTOVER** | `ScriptedCamera` exists from Bootstrap; FOV 72 is SHOT2 leftover |

Do **not** invent a first-seen global list by grepping
`script.bin` or by walking `GlobalDispatcher` top-to-bottom.

---

## 3. Leftover first *global* slice (not Leave)

When `Q_NewOakValeIntro` later runs, `00DABAC0` registers
`NOVI_LiveFather` → fiber `00DB8630` → `00DB86B0` pushes
`CS_OAKVALE_INTRO_FATHER` into `00CBFB7D`.
Dump: `script-bank/0481-cs-oakvale-intro-father.md`.
Pinned by `DataCatalogTests` / `WorldSceneTests`
(`father.Commands[0..2]`).

Head of def+60 (all Global until line 4):

| PC | Line | Token | Apply | Return | Host |
|---:|---|---|---|---|---|
| 0 | `PlayMusic MUSIC_SET_NULL` | `00CC8EAC` `0x012C1904` | `00CBF7FE` lookup `009E5120` then `vtbl+2784` | `jmp 00CD17FD` no yield | `ctx.Audio.PlayMusic` Continue |
| 1 | `FadeOut 0.5,0` | `00CD0987` `0x012C19A0` | `vtbl+1488` `008907E0` black `(0,0,0,255)` | `jmp 00CD17FD` | `ApplyFadeOut(0.5,0)` Continue |
| 2 | `CameraPause FALSE` | `00CC71F1` `0x012C2058` | `[ebp-37]=0` (`00CBEE0C` IsFalse) | Continue | `CameraPauseEnabled=false` Continue |

Then Entity: `Hero.Teleport` / `Father.Teleport` /
`Father.LookToThing`. Then Global again:
`DoScriptFrame` → `DoCameraPreloading` (`00CBF29F` name
collect, **not** a bind) → `PlayAVI` → `MuteSounds false` →
second `FadeOut 0.5,0` → `PlayMusic MUSIC_SET_OAKVALE` →
**first leftover camera bind**
`NoLoadUseCamera CAM_OVI_ID_STANDUP` (`00CC9E6A` /
`00CC907D`) → `FadeIn` → much later
`UseCamera CAM_OVIF_SHOT2` (`00CC9F3A` / `00B23B50`).

### PlayMusic

`FirstSeenPlayMusicDoesNotYield=true`. Empty track → Continue
with empty side (native empty skip). `MUSIC_SET_NULL` is a
bank name, not silence-as-no-op: helper still `vtbl+2784`.
Player body **UNREAD**. `CacheMusic` is a different token
(`vtbl+2792`). Host if-chain lists PlayMusic first; native
token match is later in the `00BFEAF8` chain.

### Fade

Two different FadeOut sites:

1. **Special-case** `00CBFDD0` before the loop: compare
   `commands[0]` to `"FadeOut 0.5,0"` then `vtbl+1488(0.5,0)`.
   Father `[0]` is PlayMusic so **skip**.
   `FirstSeenFadeSpecialCaseRuns=false`. Host
   `TryFadeSpecialCase` matches that site and does **not**
   increment PC.
2. **Opcode** `00CD0987` for the actual line. `ParseFade`
   defaults seconds `0.5` / param `0` then atof args.
   Apply packs black; overlay `006496BC` type `0x22`.
   **CompleteNow.** Same values as the skipped special-case,
   but it is the *opcode*, not the head special-case.

Bare later `FadeOut` / `FadeIn` use the same `ParseFade`
defaults. `FadeIn` apply is `vtbl+1496` `0088E4C0` (clear
lock). First leftover `FadeIn` is after
`NoLoadUseCamera CAM_OVI_ID_STANDUP`, not after Leave.

### Camera (script)

`CameraPause FALSE` does **not** move the eye. Ctor
`00CBFD53` sets `[ebp-37]=1`; this line clears it so later
`UseCamera` / `NoLoadUseCamera` **skip** `vtbl+28` leftover
when pause is off **and** yield is on — host:

```
if (!CameraPauseEnabled || !YieldEnable) Continue
else YieldOnce vtbl+28
```

`DoCameraPreloading` walks later `UseCamera` /
`CameraLookAt` / `CameraLookBetween` /
`CameraFOVLookBetween` names into `vtbl+1648`. First-seen
father has no args → `00CBF29F(dl=0)` then
`jmp 00CD17FD`. **Not** `UseCamera`.

First leftover *bind* is `NoLoadUseCamera`, **not**
`UseCamera CAM_OVIF_SHOT2`. Treating SHOT2 as first scripted
camera after Leave is **INVENTED**. Live 3D after Leave is
Lookout follow-helper 70°, not a TNG `CAM_*`.

---

## 4. `GlobalDispatcher` vs native table

Authority for verb *names* is ASCII `0x012C1500`–`0x012C2C00`.
`GlobalDispatcher` implements the recovered no-target arm.
It is **not** a first-seen-after-Leave list.

| Host first `if` | Native first loop compare | First leftover *executed* global |
|---|---|---|
| `PlayMusic` | `.WaitTask` `00CC0783` | leftover `PlayMusic MUSIC_SET_NULL` |

Dispatcher `if Eq` order **DIVERGE**s from `00BFEAF8`
(longer prefixes first). Match on these three leftovers is
**PROVEN** (unique prefixes).

`Get` is in the host Global if-chain and is **INVENTED** as
an exe token (`TokenSite=0`). Not on the father head.
`RemoveThing` is a leftover alias of `Remove`.

---

## 5. C# vs native after Leave

| Host | Native after Leave | Class |
|---|---|---|
| `GlobalDispatcher.Dispatch` | unused | **LEFTOVER** vs Leave |
| `ScriptInterpreter` / `StartCutscene` | no `00CBFB7D` | **DISPROVEN** as Leave path |
| `ScriptRuntime.StartNewGame` father walk | invented Oakvale TNG | **DIVERGE** |
| `NewGameScript.PlayMusic` / `FadeOut` getters | observe leftover father | **LEFTOVER** |
| `EngineLifecycle.Camera` 72° | WorldCamera after Init World | **LEFTOVER** object |
| `InitWorldCameras` in `EnterGame` | `006B4900` / `0069AE80` / `006FD8C0` | **PROVEN** timing |

Tests that assert `intro.Executed[0] == IntroPlayMusic` are
**PROVEN** as Oakvale VM behaviour and **DISPROVEN** as
what Leave starts.

---

## Classifications (short)

1. **First global script command after Leave — none. PROVEN.**
   `GlobalDispatcher` is the leftover `00CBFB7D` no-target arm.
2. **`PlayMusic` / `FadeOut` / `CameraPause` as first after
   Leave — DISPROVEN.** They are father def+60 `[0..2]`.
3. **Fade special-case on that leftover — DISPROVEN.**
   First line is PlayMusic. Opcode FadeOut still runs as `[1]`.
4. **First leftover camera *script* — `CameraPause FALSE`,
   then later `DoCameraPreloading`, then bind
   `NoLoadUseCamera CAM_OVI_ID_STANDUP`. PROVEN leftover.**
   `UseCamera CAM_OVIF_SHOT2` is later still.
5. **First 3D camera after Leave — WorldCamera construct /
   seed / apply. PROVEN.** Not a Global verb.
6. **Who later calls `00CBFB7D` on no-save — UNREAD**
   (not Leave / not `004B4260` / not `00CE7670`).
   Do not invent `ActivateQuest(Q_NewOakValeIntro)`.
