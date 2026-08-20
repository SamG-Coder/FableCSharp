# First `MUSIC_SET_*` after Leave (no-save) — never on first Present

Investigation only. No production `src/` or `tests/` edits.

Do **not** invent `MUSIC_SET_*` on New Game.
`RequestNewGameStartsMusicSet=false`. Live host audio
device on this walk is the **AVI FilterGraph**
(`IBasicAudio`) only. `0042DED5` is **Note-only**.
Retail singleton `[0x13B8394]` `vtbl+68` / `vtbl+72`
has **no** host player.

Do **not** start at Oakvale / `CS_OAKVALE_INTRO_FATHER` /
`PlayMusic MUSIC_SET_NULL` / `MUSIC_SET_OAKVALE`. That
path is later leftover `Q_NewOakValeIntro`
(`00DABAC0` → `00DB86B0` → `00CBFB7D`). Leave is
`0042F2A2`. First no-save 3D Present is **Lookout**.
Gameflow **parks** before the runner.

Do **not** treat frontend `0042DED5` / `0x1230C3C` /
`0x1230C48` as a `MUSIC_SET_*`. That is pre-Leave
`vtbl+68`. Track name is **UNREAD**.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Question: first `MUSIC_SET_*` after Leave frontend on
no-save. Is it **after Oakvale intro**, **after
Lookout**, or **never on first Present**? Host gap.

Authority: `proofs/audio-path-to-intro`,
`proofs/audio-after-leave`,
`proofs/audio-initgame-first`,
`proofs/00A01A4F-sound-symbols`,
`proofs/script-playmusic`,
`proofs/audio-frontend`,
`proofs/00DBDE40-host-gap`,
`proofs/cutscene-first`,
`proofs/issue-15-verify`;
`EngineLifecycle.cs` (`RetailAudioFadeFn` `0x0042DED5`,
`RetailAudioEngineVa` `0x013B8394`,
`RetailAudioStartVtbl` 68, `RetailAudioFadeVtbl` 72,
`RequestNewGameStartsMusicSet=false`);
`EngineLifecycleTests.No_save_does_not_activate_Q_NewOakValeIntro`
(`LastMusic` empty);
`WorldSceneTests` leftover father
(`MUSIC_SET_NULL` then `MUSIC_SET_OAKVALE`);
ExeIndex `script-bank/0481-cs-oakvale-intro-father.md`;
`e8.tsv` dest `0042DED5` (3 sites, all in `0042EC7C`).

---

## Direct answer

| Option | Class | Why |
|---|---|---|
| After Oakvale intro | **DISPROVEN** as first `MUSIC_SET_*` | Leftover first line is `PlayMusic MUSIC_SET_NULL` at **start** of `CS_OAKVALE_INTRO_FATHER`, not after it. Named `MUSIC_SET_OAKVALE` is **mid-intro** (after dream AVI / `StartTimeCode`). Intro itself is **not** first Present. |
| After Lookout | **DISPROVEN** as a recovered first-seen site | First Present **is** Lookout. Lookout TNG / WLD / ContainsMap do not run `00CC8EAC`. No `PlayMusic` on that tree. |
| **Never on first Present** | **PROVEN** | First no-save Present is Lookout 3D. Gameflow `00CE7670` state 0 yields on inactive `Q_NewOakValeIntro`. Runner `00CBFB7D` is not entered. Host `LastMusic` is empty. |

**Answer: never on first Present.**

The first leftover script `MUSIC_SET_*`, **if**
`Q_NewOakValeIntro` later starts, is father def+60
`[0]` `PlayMusic MUSIC_SET_NULL` — **at the start of**
Oakvale intro, **not** after it, **not** after Lookout
load, **not** on first Present. Who activates that
quest on no-save remains **UNREAD**. Do not invent
the activate from `RequestNewGame`.

---

## Verdict

Leave does not start a `MUSIC_SET_*`. First Present
does not start a `MUSIC_SET_*`. Host must not play one.

Native first *audio op* after Leave is
`[0x13B8394].vtbl+72(0x1F4)` — a **500 ms fade** of
the unnamed pre-Leave frontend voice (`0042F00A`
`vtbl+68`). That file is **not** a `MUSIC_SET_*`.
Init Sound **registers**. First type-1 Atmos
**ticks** (`006B2260` dummy miss). Script music is
**parked**.

Host `RequestNewGame` Notes `0042EBB6 +41 skip audio
stop` and does **not** set `LastMusic`.
`EnterFrontendAfterAvi` Notes `0042DED5 0` only.
Live device after AVI unload is **nothing**.

```
AVI FilterGraph IBasicAudio     MATCH play; dies on unload
        ↓
0042DED5 vtbl+68                MATCH Note; DIVERGE no [0x13B8394]
        ↓
Leave 0042F2A2                  MATCH +41 skip stop; DIVERGE no vtbl+72(500)
        ↓
Init Sound 00417A58             MATCH name; DIVERGE register body
        ↓
first Present Lookout           MATCH park; DISPROVEN MUSIC_SET
        ↓
(future leftover) PlayMusic NULL  LEFTOVER / UNREAD fire
```

---

## Evidence → Original → Host → Gap

### E1. Frontend `0042DED5` is not `MUSIC_SET_*`

| | |
|---|---|
| **Evidence** | `e8.tsv` dest `0042DED5`: only `0042F00A` / `0042F07A` / `0042F1FD`, all inside retail `0042EC7C` **before** Leave. Body: `[0x13B8394].vtbl+68(path, 0, 0, 1.0, fade, -1)`. Path via `0099B6B0(0x1230C48)` + `0099B6B0(0x1230C3C)` + `0099C1E0`. Not in `strings.tsv`. No `00CC8EAC`. (`audio-frontend`, `audio-after-leave`) |
| **Original** | After last AVI Present, `0042F004 fldz` / `0042F00A call 0042DED5`. Singleton `RetailAudioEngineVa` `0x013B8394`. Null → `je 0042DF9A`. Retrigger `0042F07A` only if `[engine+364]!=0`. Attract `0042F1FD` after `.wmv`. **PROVEN** pre-Leave `vtbl+68`. Name **UNREAD**. |
| **Host** | `EnterFrontendAfterAvi` `Note(RetailAudioFadeFn, …, "0042DED5 0")`. `RequestNewGameStartsMusicSet=false`. No `[0x13B8394]`, no `vtbl+68`, no ogg. **MATCH** site. |
| **Gap** | Host **DIVERGE** player (Note-only). Do not invent `MUSIC_SET_TITLE` / forest as the unnamed path. Frontend voice is a **different object** from script `vtbl+2784`. |

### E2. Leave fades that voice; it does not start `MUSIC_SET_*`

| | |
|---|---|
| **Evidence** | `0042F2C7 mov ecx,[0x13B8394]` / `push 0x1F4` / `call [eax+72]`. Then `0042EBB6 cmp [esi+41],bl; jne 0042EC2A` skips `vtbl+64` / `vtbl+72(0)` / `00991750` / `009918F0`. Zero `E8` of `0042DED5` past `0042F2A2`. (`audio-after-leave`) |
| **Original** | New Game (`+41!=0`) **keeps** the frontend voice, faded 500 ms. Quit / load (`+41==0`) **does** stop. First post-Leave audio op is fade, not a new start. **PROVEN.** |
| **Host** | `RequestNewGame` Notes `"0042EBB6 +41 skip audio stop"`. No `vtbl+72(500)`. `LeaveFrontendAudioVtbl` / `LeaveFrontendAudioMs` exist unused. **MATCH** skip-stop. Fade **DIVERGE** (nothing to fade). |
| **Gap** | Constants leftover-declare. Do not hard-stop AVI `IBasicAudio` as this fade (wrong object; graph already dead). Do not start a `MUSIC_SET_*` because “we left the menu.” |

### E3. Init Sound / Init Atmos / first type-1 are not `MUSIC_SET_*`

| | |
|---|---|
| **Evidence** | `"Init Sound"` `00417A58` after Create Players: `[0x13B8394]==0` skip (not taken). `009919C0` / `00991840` / `00991C10` register. Nested `"Sound Bank: Init Symbols"` `00A01A4F` is **DISPROVEN** first-seen fire (`UseCompiledSoundSymbols` TRUE → `00A38C20`). `"Init Atmos"` `006B1960` `vtbl+144`. First type-1 `004A5E7B call 006B2260` dummy `MARKER_POSITIONAL_ATMOS` miss; candidate `SOUND_THEME` `vtbl+160` fire **UNREAD**. Zero `E8` of `00A01920` / `00CC8EAC` on this tree. (`audio-initgame-first`, `00A01A4F-sound-symbols`, `00417A58-init-sound-body`) |
| **Original** | Register + construct + Atmos tick. Names if `vtbl+160` later fires are `SOUND_THEME` / `NIGHT`, **not** `MUSIC_SET_*`. **PROVEN** skip of script music. |
| **Host** | `InitGameStages` twelfth name `"Init Sound"` `0x00417A58` — Note only, no `if` body. `TickAtmos` Notes `006B2260`. No `world+36` object. **MATCH** names / timing. Body **DIVERGE**. |
| **Gap** | Issue #15 still open for register / mixer. Not this file’s work. Do not treat `LookupMusic` / `data/Sound/*.ogg` as Init Sound. |

### E4. First Present is Lookout. Gameflow parks. No runner.

| | |
|---|---|
| **Evidence** | No-save first region / first *rendered* scene is LookoutPoint (`RegionThings` + `006B3FF0` / GuildArrivalHSP). Leftover **#4**. `user.ini` `ActivateQuest("Gameflow")` → `00CE75B0`. First type-1 `00CB8220` → `00A44880` → `00CE7670` state 0: `00893610 Q_NewOakValeIntro = 0` → `009D8650` yield. Bind `00CD6E27` `00CB5C90` is **bind only**, not `00CB5AD0`. `EngineLifecycleTests.No_save_does_not_activate_Q_NewOakValeIntro`: quest not in `ActivatedQuests` / `Runtime.Quests`; `LastMusic` empty; `RequestNewGameStartsMusicSet==false`; no `Va==00DBDE40`. (`00DBDE40-host-gap`, `script-playmusic`, `cutscene-first`) |
| **Original** | First Present = Lookout 3D + hero. Empty `CS_PlayCutscene` factory. Gameflow **waits**. Does **not** construct Oakvale, does **not** start `00DABAC0`, does **not** enter `00CBFB7D`. **PROVEN** never-on-first-Present. Lookout TNG has no recovered `PlayMusic`. **DISPROVEN** “after Lookout” as a music site. |
| **Host** | `TickGameflowMain` / `ResumeGameflowWait` Notes the same wait (`GameflowWaitQuest = "Q_NewOakValeIntro"`). `PumpScripts` empty. `RequestNewGame` / `EnterGame` / `Pump` never call `ScriptRuntime.StartNewGame`. **MATCH** park. |
| **Gap** | Activator of `Q_NewOakValeIntro` **UNREAD**. Host must not invent `ActivateNamedQuest("Q_NewOakValeIntro")` from Leave / first Present. `FirstSceneWorld.Build` leftover soup is **not** this pump. |

### E5. Leftover first `MUSIC_SET_*` is **start of** Oakvale intro, not after it

| | |
|---|---|
| **Evidence** | Dump `0481-cs-oakvale-intro-father.md`. Pinned `DataCatalogTests` `commands[0]`, `WorldSceneTests` `intro.Executed[0]`. Token `00CC8EAC` (`0x012C1904`): `009E5120` then `[0x143E8F8].vtbl+2784`; `jmp 00CD17FD` (no yield). `FirstSeenPlayMusicDoesNotYield=true`. Who starts the quest on no-save: **UNREAD**. (`script-playmusic`) |
| **Original** | **If** leftover father later starts via `00DB86B0` → `00CBFB7D`: **PC 0** `PlayMusic MUSIC_SET_NULL` (kill bank; not a start of the `0042DED5` file). Then `FadeOut 0.5,0` … teleports, LookToThing, frames, `PlayAVI dream_sequence_comp.xmv`, mute, wake anims, `StartTimeCode`. **PC 17** `PlayMusic MUSIC_SET_OAKVALE`. **PC 18** `NoLoadUseCamera CAM_OVI_ID_STANDUP`. Father speak / `CAM_OVIF_SHOT2` is **after** that. **DISPROVEN** “after Oakvale intro.” First leftover set is **at intro start**. Named Oakvale set is **mid-intro**, after the dream AVI. |
| **Host** | `GlobalDispatcher.PlayMusic` → `AudioRuntime.PlayMusic` field assign + `LookupMusic`. `MUSIC_SET_NULL` stem → **null** (no ogg). `MUSIC_SET_OAKVALE` → `data/Sound/OAKVALE.ogg` if present; **still not played**. `IScriptHost.PlayMusic` is `LastMusic = track`. Reached only from leftover `StartNewGame` / `WorldSceneTests`, **not** `RequestNewGame`. `runtime-trace.txt` frame 0 is that leftover façade. |
| **Gap** | Opcode **MATCH** *if* runner starts. Fire on this walk **UNREAD** / **LEFTOVER**. Device **DIVERGE** (`vtbl+2784` body **UNREAD**; host has none). `RequestNewGameStartsMusicSet=false` locks the skip. Do not plug FMOD / ogg into New Game. |

### E6. Live host device is AVI FilterGraph only

| | |
|---|---|
| **Evidence** | Native open `00A3B9D0` after `RenderFile`: QI `IBasicAudio` `0x12AA054` then `put_Volume(0)`. Unload `00A3B380` / `00A3BC20` before next slot / before `0042E98F`. Leftover #9 **CLOSED**. (`audio-path-to-intro`, `leftover-9-20-status`) |
| **Original** | Startup AVI voice is quartz graph. It **dies with the slot**. Not `[0x13B8394]`. Not script `vtbl+2784`. Intro leftover `PlayAVI dream_sequence` is **not** on the no-save Leave tree. |
| **Host** | `WmvPlayer.BuildGraph` RCW QI + `put_Volume(0)`. `LastBasicAudioQi` / volume 0 asserted. After `FinishStartupVideo` ×3 the graph is torn down. Frontend / Leave / first Present have **no** second device. Client has no `PlayMusic` path. |
| **Gap** | **MATCH** AVI play + death. After Leave the host **keeps nothing**. Native still has the fading unnamed `vtbl+68` voice. That gap is Note-only `0042DED5`, **not** a licence to start `MUSIC_SET_*`. |

---

## Timeline (no-save New Game)

```
0042EC7C retail
  006286F0 ×3 PlayAVI
    00A3B9D0 FilterGraph + IBasicAudio put_Volume(0)   // ONLY live host device
    00A3B380 / 00A3BC20 unload before next slot
  009D8CF0 + 009BEEB0 black Present
  0042F00A call 0042DED5                               // ONLY first [0x13B8394].vtbl+68
    path UNREAD (0x1230C3C / 0x1230C48)                // NOT MUSIC_SET_*
  005952C3 UI show
  loop 0042F041
    0059A238  0xE5 / 0x126 / 15                        // no PlayMusic, no SND_*
  msg 15 → [retail+41]=1
0042F2A2 Leave frontend                                // not 00CC8EAC
  0042F2D8 vtbl+72(0x1F4)                              // fade; keep voice
  0042EBB6 +41 skip vtbl+64 / vtbl+72(0)
  009BE420 + 009BEEB0 black Present
0042F491 Init Game 004184BD
  "Init Sound" 00417A58                                // register; no MUSIC_SET
  00416953 Load FinalAlbion.wld
    00CD6E27 bind Q_NewOakValeIntro                    // BIND ONLY
  user.ini ActivateQuest("Gameflow")
004189C2 first type-1
  00CE7670 state 0  00893610 miss → 009D8650 yield     // PARK
  004A5E7B 006B2260 dummy MARKER miss
  first 3D Present = Lookout + hero                    // NEVER MUSIC_SET
  // not 00CBFB7D, not 00CC8EAC, LastMusic empty
```

Later leftover, **not** this Present:

```
(unpark UNREAD — not Leave, not Lookout load)
  00DABAC0 → 00DB86B0 → 00CBFB7D
    [0]  PlayMusic MUSIC_SET_NULL                      // first leftover set (kill)
    … FadeOut / teleports / dream AVI / wake …
    [17] PlayMusic MUSIC_SET_OAKVALE                   // mid-intro, not after intro
    [18] NoLoadUseCamera CAM_OVI_ID_STANDUP
```

---

## Host vs native (one table)

| Host | Native after Leave / first Present | Class |
|---|---|---|
| `RequestNewGameStartsMusicSet=false` | no `00CC8EAC` on tree | **MATCH** skip |
| `LastMusic` empty after Pump | runner not entered | **MATCH** |
| `Note(0042DED5)` | `0042F00A` `vtbl+68` | **MATCH** site; **DIVERGE** player |
| `0042EBB6 +41 skip audio stop` Note | skip quartet | **MATCH** pairing |
| no `vtbl+72(500)` | `0042F2D8` | **DIVERGE** fade |
| `Note("Init Sound")` | `00417A58` register | **MATCH** name; **DIVERGE** body |
| `TickAtmos` Note | `006B2260` dummy miss | **MATCH** timing |
| Gameflow yield `Q_NewOakValeIntro` | `00CE7670` state 0 | **MATCH** park |
| first Present Lookout | LookoutPoint + hero | **MATCH** leftover #4 |
| AVI `IBasicAudio` | `00A3B9D0` | **MATCH**; dies before Leave |
| `AudioRuntime` / `LastMusic` | leftover father | **LEFTOVER**; fire **UNREAD** here |
| `FirstSceneWorld` / `StartNewGame` | invented Oakvale TNG + father | **DISPROVEN** as this pump |
| FMOD / ogg device | none recovered on this walk | **DISPROVEN** invent |

Tests that assert `intro.Executed[0] == "PlayMusic MUSIC_SET_NULL"`
are **PROVEN** leftover VM and **DISPROVEN** as first after
Leave / first Present.

---

## Classifications (short)

1. **First `MUSIC_SET_*` after Leave on no-save — never
   on first Present. PROVEN.** Lookout 3D. Gameflow
   parked. Runner not entered. Host `LastMusic` empty.
2. **“After Lookout” as a music start — DISPROVEN.**
   Lookout *is* first Present. No `PlayMusic` there.
3. **“After Oakvale intro” as first `MUSIC_SET_*` —
   DISPROVEN.** Leftover first line is `MUSIC_SET_NULL`
   at intro **start**. `MUSIC_SET_OAKVALE` is **mid**
   leftover intro after dream AVI. Intro is not first
   Present. Activator **UNREAD**.
4. **Frontend `0042DED5` `vtbl+68` is not `MUSIC_SET_*`.
   PROVEN** as a different object. Name **UNREAD**.
   Host Note-only.
5. **`RequestNewGameStartsMusicSet=false` MATCH.** Do
   not invent New Game music. Live device is AVI
   FilterGraph only.

Do not play `MUSIC_SET_NULL`, `MUSIC_SET_OAKVALE`,
`MUSIC_SET_TITLE`, or a Lookout / forest loop from
`RequestNewGame` / first Present.

---

## Do not invent

- `MUSIC_SET_*` from `RequestNewGame` / first Present.
- `MUSIC_SET_TITLE` / forest as `0042DED5`.
- Lookout TNG / WLD ambience as first script music.
- `ActivateQuest("Q_NewOakValeIntro")` from Leave.
- FMOD, XAudio, or a host mixer for `[0x13B8394]` or
  `vtbl+2784`.
- Collapsing AVI quartz, frontend `vtbl+68`, Atmos
  `vtbl+160`, and script `vtbl+2784` into one player.

---

## Sources

- `C:\FableCSharp\proofs\audio-path-to-intro\README.md`
- `C:\FableCSharp\proofs\audio-after-leave\README.md`
- `C:\FableCSharp\proofs\audio-initgame-first\README.md`
- `C:\FableCSharp\proofs\00A01A4F-sound-symbols\README.md`
- `C:\FableCSharp\proofs\script-playmusic\README.md`
- `C:\FableCSharp\proofs\audio-frontend\README.md`
- `C:\FableCSharp\proofs\00DBDE40-host-gap\README.md`
- `C:\FableCSharp\proofs\cutscene-first\README.md`
- `C:\FableCSharp\proofs\issue-15-verify\README.md`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\src\Fable.Game\ScriptRuntime.cs`
- `C:\FableCSharp\src\Fable.Game\Scripting\GlobalDispatcher.cs`
- `C:\FableCSharp\src\Fable.Game\Scripting\ExecutionContext.cs`
- `C:\FableCSharp\tests\Fable.Formats.Tests\EngineLifecycleTests.cs`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\script-bank\0481-cs-oakvale-intro-father.md`
- `C:\FableCSharp\docs\status\README.md` leftover #4
