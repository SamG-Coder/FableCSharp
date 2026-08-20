# Host audio path: AVI → frontend → Leave → Gameflow park → (future) intro `PlayMusic`

Investigation only. Dump + `src/` notes. No production edits.

Do **not** invent FMOD, a title `MUSIC_SET_*`, forest
ambience, or a WAV mixer for this walk. Native objects
on this tree are:

- PlayAVI quartz graph (`00A3B9D0` / `IBasicAudio`
  `0x12AA054`) — **dies with the slot**.
- Engine singleton `[0x13B8394]` (`vtbl+68` start,
  `vtbl+72` fade, `vtbl+64` stop) — **frontend voice**.
- Init Game register `00417A58` / Atmos ctor `vtbl+144`
  / tick `006B2260` — **not** a new `MUSIC_SET_*`.
- Leftover script `PlayMusic` `00CC8EAC` `vtbl+2784` —
  **after** Gameflow unparks `Q_NewOakValeIntro`.

Status words: **MATCH** / **DIVERGE** / **UNREAD** /
**PROVEN** / **PARTIAL** / **DISPROVEN** / **LEFTOVER**.

Authority: `src/Fable.Game/WmvPlayer.cs` `BuildGraph`;
`src/Fable.Game/EngineLifecycle.cs` (`EnterFrontendAfterAvi`
`0042DED5` Note, `RequestNewGame` `0042EBB6 +41 skip audio
stop`, `InitGameStages` `"Init Sound"` `00417A58`,
`TickGameflowMain` / `ResumeGameflowWait`, `TickAtmos`);
`src/Fable.Game/Scripting/ExecutionContext.cs`
`AudioRuntime`; `src/Fable.Game/ScriptRuntime.cs`
`LookupMusic` / `IScriptHost.PlayMusic`;
`proofs/leftover-9-20-status` (#9 live QI);
`proofs/audio-frontend`; `proofs/audio-after-leave`;
`proofs/audio-initgame-first`; `proofs/leave-first-sound`;
`proofs/00417A58-init-sound-body`; `proofs/script-playmusic`;
`proofs/leave-0042F2A2-host`; `proofs/issue-15-verify`.

Siblings stay the per-hop notes. This file is the **host
path** across them.

---

## Direct answers

| Hop | Native | Host | Class |
|---|---|---|---|
| 1. Startup / leftover AVI | `00A3B9D0` QI `IBasicAudio` then `put_Volume(0)` | `WmvPlayer.BuildGraph` RCW QI + `put_Volume(0)` | **MATCH** site; graph is **not** `[0x13B8394]` |
| 2. Frontend voice | `0042F00A call 0042DED5` `[0x13B8394].vtbl+68` after last AVI | `Note(0042DED5 0)` only | **MATCH** site; **DIVERGE** player |
| 3. Frontend click / `SND_*` | none on `0059A238` | none | **MATCH** skip |
| 4. Leave New Game | `0042F2A2` `vtbl+72(0x1F4)` then `0042EBB6 +41` skip stop | skip-stop Note; **no** fade | skip **MATCH**; fade **DIVERGE** |
| 5. Init Sound | `00417A58` register (`009919C0`) | `Note("Init Sound")` | name **MATCH**; body **DIVERGE** (Note-only) |
| 6. First type-1 Atmos | `004A5E7B` `006B2260` dummy rain miss | `TickAtmos` Note | timing **MATCH**; object **DIVERGE** |
| 7. Gameflow park | `00CE7670` state 0 `00893610` miss → yield on `Q_NewOakValeIntro` | same wait Notes; no `00CBFB7D` | **MATCH** park |
| 8. Future intro `PlayMusic` | leftover father `[0]` `MUSIC_SET_NULL` `00CC8EAC` | `AudioRuntime` fields / `LastMusic` **if** leftover runner starts | **UNREAD** fire on this walk; opcode **LEFTOVER** |

**Answer:** the live host audio device on this path is
**only the AVI FilterGraph**. Frontend “has audio” in
**native**. Host **Notes** `0042DED5` and then **keeps
nothing** across Leave. Gameflow **parks** before
`PlayMusic MUSIC_SET_NULL`. Do not plug FMOD, ogg, or
`MUSIC_SET_OAKVALE` into `RequestNewGame`.

---

## Verdict (path)

```
AVI graph (IBasicAudio)     MATCH play on WMV; dies on unload
        ↓
0042DED5 vtbl+68            MATCH Note; DIVERGE no [0x13B8394]
        ↓
Leave 0042F2A2              MATCH +41 skip stop; DIVERGE no vtbl+72(500)
        ↓
Init Sound 00417A58         MATCH name; DIVERGE register body
        ↓
Gameflow park               MATCH yield; DISPROVEN PlayMusic
        ↓
(future) PlayMusic NULL     LEFTOVER / UNREAD on this pump
```

`proofs/audio-frontend` §5 still says `BuildGraph` never
QIs `IBasicAudio`. That row is **stale**. Live QI is
`proofs/leftover-9-20-status` **#9 CLOSED**.

---

## Timeline (no-save New Game)

```
0042EC7C retail
  006286F0 ×3 PlayAVI
    00A3B9D0 FilterGraph + IBasicAudio put_Volume(0)
    00A3B380 / 00A3BC20 unload before next slot
  0042E98F bind UI
  0042E204 Init Engine
  009D8CF0 + 009BEEB0 black Present
  0042F00A call 0042DED5          // ONLY first [0x13B8394].vtbl+68
    path UNREAD (0x1230C3C / 0x1230C48)
  005952C3 UI show
  loop 0042F041
    0059A238  0xE5 / 0x126 / 15   // no 00A01920, no SND_*
  msg 15 → [retail+41]=1
0042F2A2 Leave frontend
  0042F2D8 vtbl+72(0x1F4)         // FIRST post-Leave audio op
  0042EBB6 +41 skip
    vtbl+64 / vtbl+72(0) / 00991750 / 009918F0
0042F491 Init Game 004184BD
  "Init Sound" 00417A58           // register; no vtbl+68
  00416953
    004A6550 "Init Atmos" 006B1960 vtbl+144
    user.ini ActivateQuest("Gameflow")
      00CE75B0 Main watcher
004189C2 first type-1
  00CB8220 → 00A44880 → 00CE7670 state 0
    00893610 Q_NewOakValeIntro = 0 → 009D8650 yield   // PARK
  004A5E7B 006B2260 dummy MARKER_POSITIONAL_ATMOS miss
  // not 00CBFB7D, not 00CC8EAC
```

Later leftover, **not** this pump:

```
(unpark UNREAD)
  00DABAC0 → 00DB86B0 → 00CBFB7D
    PlayMusic MUSIC_SET_NULL      // first leftover script music
    … later PlayMusic MUSIC_SET_OAKVALE
```

---

## 1. AVI — `IBasicAudio` (**MATCH**)

Native open `00A3B9D0` after `RenderFile`: QI Control /
Position / Seeking / Event / **`IBasicAudio` `0x12AA054`**,
then `put_Volume(0)` (DirectShow 0 = 0 dB). Not
`IVideoWindow`. Voice is the WMV audio pin + default
DirectSound renderer. **PROVEN** (`leftover-9-20-status`,
ExeIndex).

Host `WmvPlayer.BuildGraph`:

```383:403:src/Fable.Game/WmvPlayer.cs
        _control = (IMediaControl)_graph;
        _position = (IMediaPosition)_graph;
        _events = (IMediaEvent)_graph;
        // 00A3B9D0 QI IBasicAudio 0x12AA054 then
        // put_Volume(0) = 0 dB. Not a WAV mixer.
        LastBasicAudioQi = false;
        LastBasicAudioVolume = int.MinValue;
        try
        {
            _audio = (IBasicAudio)_graph;
            var volHr = _audio.put_Volume(0);
            if (volHr >= 0)
            {
                LastBasicAudioQi = true;
                LastBasicAudioVolume = 0;
            }
        }
```

ComImport GUID `56a868b3-0ad4-11ce-b03a-0020af0ba770` is
the same IID as `RegionTravel.PlayAviBasicAudioIid`.
`WorldSceneTests.PlayAvi_rewrites_xmv_to_installed_wmv_and_blocks`
asserts `LastBasicAudioQi` and volume 0.

| Claim | Class |
|---|---|
| Live QI + `put_Volume(0)` | **MATCH** |
| `PlayAviBasicAudioIid` **symbol** used by `BuildGraph` | **DIVERGE** (duplicate GUID, not the field) |
| `IMediaSeeking` stored | **DIVERGE** (native QI; host uses `IMediaPosition`) |
| Class summary lists BasicAudio | **DIVERGE** (summary still Control/Position/Event) |
| AVI graph is frontend `vtbl+68` | **DISPROVEN** |
| Unload `00A3B380` before next slot / before `0042E98F` | **MATCH** (`UnloadStartupAvi` / `FinishStartupVideo`) |
| `IBasicAudio` gates `0042E98F` | **DISPROVEN** (`audio-frontend`) |

Startup AVI and leftover script `PlayAVI` share
`WmvPlayer`. Script `PlayAVI` is **not** on the no-save
Leave tree (`script-playmusic`). Do not treat intro
voice-over as `PlayMusic`.

---

## 2. Frontend — unnamed `[0x13B8394].vtbl+68` (**DIVERGE** player)

Native after last AVI Present: `0042F00A call 0042DED5`
(`fldz` fade 0). Singleton `[0x13B8394]`. Null → no-op.
Path via `0099B6B0(0x1230C48)` + `0099B6B0(0x1230C3C)` +
`0099C1E0`. **PROVEN** (`audio-frontend`). File name
**UNREAD**. Not `PlayMusic`. Not AVI `IBasicAudio`.

Host `EnterFrontendAfterAvi`:

```3916:3916:src/Fable.Game/EngineLifecycle.cs
        Note(RetailAudioFadeFn, "InitFrontend", "Audio", "0042DED5 0");
```

No `[0x13B8394]`, no `vtbl+68`, no ogg. **DIVERGE.**

Frontend click / `SND_MENU_04` / `Play2DSound UI_CLICK`
on Press Start / New Game: **DISPROVEN** (`audio-frontend`
§2). Host `DispatchFrontendMessage` has no sound side
effect. **MATCH** skip.

Native “frontend has audio” means the **unnamed
`0042DED5` voice is running** while the UI is up. Host
UI is silent.

---

## 3. Leave `0042F2A2` +41 skip stop (**MATCH** skip, **DIVERGE** fade)

Native:

```
0042F2A2  push "Leave frontend"
0042F2C7  mov ecx, [0x13B8394]
          je  0042F2DB
0042F2D8  call [eax+72]   ; push 0x1F4
0042EBB6  cmp [esi+41],bl
          jne 0042EC2A    ; skip vtbl+64 / vtbl+72(0) / 00991750 / 009918F0
```

`LeaveFrontendAudioVtbl=72`, `LeaveFrontendAudioMs=0x1F4`.
Quit / load (`+41==0`) **does** stop. New Game must not.

Host `RequestNewGame`:

```4675:4676:src/Fable.Game/EngineLifecycle.cs
        Note(LeaveFrontendTeardownFn, "LeaveFrontend", "Frontend",
            "0042EBB6 +41 skip audio stop");
```

No `vtbl+72(500)`. Nothing to fade (no frontend player).
**DIVERGE** fade; **MATCH** “do not hard-stop.”

Constants exist (`LeaveFrontendAudioVtbl` /
`LeaveFrontendAudioMs`) and are unused on the live path.
**LEFTOVER** declare.

---

## 4. Init Sound `00417A58` then Atmos (**DIVERGE** body)

Native after Create Players: `"Init Sound"` `00417A58`.
Gate `[0x13B8394]==0` skip. After Leave the singleton is
live so the body **registers** (`009919C0` / `00991840` /
`00991C10`). No `vtbl+68`. No `00A01920`. No `SND_*`.
**PROVEN** (`00417A58-init-sound-body`,
`audio-initgame-first`).

Host `InitGameStages` twelfth name is
`("Init Sound", 0x00417A58)`. `EnterGame` Notes it.
No `if (name == "Init Sound")` body. **MATCH** name.
**DIVERGE** work (`issue-15-verify` STILL OPEN).

`"Init Atmos"` `006B1960` `vtbl+144` is later
`00416953` → `004A6550`. Host has **no** `Init Atmos`
string and **no** `world+36` object. First type-1
`TickAtmos` Notes `006B2260` dummy
`MARKER_POSITIONAL_ATMOS`. Timing **MATCH**. Player
**DIVERGE**. `SOUND_THEME` `vtbl+160` first fire
**UNREAD**. Not `MUSIC_SET_*`.

Do not treat `LookupMusic` / `data/Sound/*.ogg` as
this stage. That helper is leftover `PlayMusic` only.

---

## 5. Gameflow park (**MATCH**)

`user.ini` `ActivateQuest("Gameflow")` → `00CE75B0`
Main watcher. First type-1 `00CB8220` → `00A44880` →
`00CE7670` state 0: `00893610 Q_NewOakValeIntro` miss
→ `009D8650` yield. Does **not** construct the quest,
does **not** start `00DABAC0`, does **not** enter
`00CBFB7D`. **PROVEN**
(`Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`).

Host `TickGameflowMain` / `ResumeGameflowWait` Notes
the same wait (`GameflowWaitQuest = "Q_NewOakValeIntro"`)
and parks. `PumpScripts` Notes empty `006E75C0`.
**MATCH** park. **DISPROVEN** `PlayMusic` on this pump.

Who later activates `Q_NewOakValeIntro` is **UNREAD**
(`No_save_does_not_activate_Q_NewOakValeIntro`). Host
must not invent that activate.

---

## 6. (Future) intro `PlayMusic MUSIC_SET_NULL` (**LEFTOVER**)

If the leftover father later starts:

| Item | Native | Host | Class |
|---|---|---|---|
| First line | `PlayMusic MUSIC_SET_NULL` | `GlobalDispatcher` → `AudioRuntime.PlayMusic` / `LastMusic` | opcode **MATCH** if runner starts |
| Apply | `009E5120` then `vtbl+2784`; `jmp 00CD17FD` | `LookupMusic("MUSIC_SET_NULL")` → **null** (NULL stem) | kill **MATCH**; device **UNREAD** / **DIVERGE** |
| Player | `[0x143E8F8].vtbl+2784` | field assign only (`issue-15`) | **DIVERGE** |
| Next named set | `MUSIC_SET_OAKVALE` later in same leftover def | would `LookupMusic` → `data/Sound/OAKVALE.ogg` if present; still not played | **LEFTOVER** |

`FirstSeenPlayMusicDoesNotYield=true`. **PROVEN.**

`MUSIC_SET_NULL` **kills** the music bank. It is **not**
a start of the frontend `0042DED5` file. Native frontend
voice should already have been faded 500 ms at Leave
and **kept**. Host has no voice left to kill.

Do not invent FMOD as `vtbl+2784`. Destination of that
slot is **UNREAD**.

---

## Host vs native (one table)

| Host | Native | Class |
|---|---|---|
| `WmvPlayer` `IBasicAudio` + `put_Volume(0)` | `00A3B9D0` | **MATCH** |
| `Note(0042DED5)` | `0042F00A` `vtbl+68` | **MATCH** site; **DIVERGE** player |
| Frontend click silent | `0059A238` no sound | **MATCH** |
| `0042EBB6 +41 skip audio stop` Note | skip quartet | **MATCH** pairing |
| no `vtbl+72(500)` | `0042F2D8` | **DIVERGE** |
| `Note("Init Sound")` | `00417A58` register | **MATCH** name; **DIVERGE** body |
| no `006B1960` | Atmos ctor `vtbl+144` | **DIVERGE** |
| `TickAtmos` Note | `006B2260` dummy miss | **MATCH** timing |
| Gameflow yield `Q_NewOakValeIntro` | `00CE7670` state 0 | **MATCH** |
| no `00CC8EAC` on Leave / first type-1 | runner not on tree | **MATCH** skip |
| `AudioRuntime` / `LastMusic` | leftover father | **LEFTOVER**; fire **UNREAD** here |
| FMOD / ogg device | none recovered | **DISPROVEN** invent |

---

## Do not invent

- FMOD, XAudio, or a host mixer for `[0x13B8394]`.
- `MUSIC_SET_TITLE` / forest loop as `0042DED5`.
- `PlayMusic MUSIC_SET_NULL` from `RequestNewGame`.
- `SND_MENU_04` / `UI_CLICK` because New Game left the menu.
- Hard-stop of AVI `IBasicAudio` as the Leave fade (wrong object).
- Collapsing AVI quartz, frontend `vtbl+68`, Atmos `vtbl+160`,
  and script `vtbl+2784` into one player.

---

## Sources

- `C:\FableCSharp\src\Fable.Game\WmvPlayer.cs`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\src\Fable.Game\ScriptRuntime.cs`
- `C:\FableCSharp\src\Fable.Game\Scripting\ExecutionContext.cs`
- `C:\FableCSharp\src\Fable.Game\Scripting\GlobalDispatcher.cs`
- `C:\FableCSharp\proofs\leftover-9-20-status\README.md`
- `C:\FableCSharp\proofs\audio-frontend\README.md`
- `C:\FableCSharp\proofs\audio-after-leave\README.md`
- `C:\FableCSharp\proofs\audio-initgame-first\README.md`
- `C:\FableCSharp\proofs\leave-first-sound\README.md`
- `C:\FableCSharp\proofs\00417A58-init-sound-body\README.md`
- `C:\FableCSharp\proofs\script-playmusic\README.md`
- `C:\FableCSharp\proofs\leave-0042F2A2-host\README.md`
- `C:\FableCSharp\proofs\issue-15-verify\README.md`
