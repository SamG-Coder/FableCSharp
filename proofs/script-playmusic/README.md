# First `PlayMusic` / `MUSIC_SET_*` after Leave

Investigation only. No production `src` edits.

Do **not** start at `00DB86B0` / `CS_OAKVALE_INTRO_FATHER` /
`PlayMusic MUSIC_SET_NULL`. That path is later leftover
`Q_NewOakValeIntro` (`00DABAC0` → TNG `NOVI_LiveFather` →
`00DB86B0` → `00CBFB7D`). Leave is `0042F2A2`. First no-save
3D Present does not enter the runner.

Do **not** chase frontend audio `0042DED5` / `0x1230C3C` /
`0x1230C48`. That is pre-Leave, not `PlayMusic`, and the file
name is **UNREAD**. See `proofs/audio-frontend/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: `docs/runtime/FORWARD_TREE.md` §§4–11;
`proofs/audio-frontend/README.md`; `proofs/newgame-script/README.md`;
`proofs/script-interpreter/README.md`; `proofs/script-global-cmds/README.md`;
`proofs/cutscene-first/README.md`; `proofs/dialogue-first/README.md`;
`src/Fable.Game/Scripting/GlobalDispatcher.cs`;
`src/Fable.Game/ScriptCommandMap.cs`; `src/Fable.Game/RegionTravel.cs`;
`EngineLifecycleTests.No_save_does_not_activate_Q_NewOakValeIntro`;
`DataCatalogTests` / `WorldSceneTests` (`FirstSeenPlayMusicDoesNotYield`,
`IntroPlayMusic`);
ExeIndex `script-runtime/playmusic-interpreter-00cc8eac-00cc8eac.md`,
`playmusic-helper-00cbf7fe-00cbf7fe.md`,
`script-bank/0481-cs-oakvale-intro-father.md`,
`script-bank/native-sqnovi.md`.

---

## Verdict

**Leave does not execute `PlayMusic` and does not start a
`MUSIC_SET_*`.**

`S_QNOVI` is **bind-only** on this walk (`00CD6E27` /
`00CB5C90`). Gameflow **waits** on inactive `Q_NewOakValeIntro`
and **yields**. It does not construct the quest, does not
start `00DABAC0`, and does not enter `00CBFB7D`.

The first *leftover* script music, **if** that quest later
starts, is father def+60 `[0]`:

`PlayMusic MUSIC_SET_NULL`

Token `00CC8EAC` (`0x012C1904`). Lookup `009E5120` then
context `vtbl+2784`. `jmp 00CD17FD` (no yield). Later in the
same leftover def: `PlayMusic MUSIC_SET_OAKVALE` (after
`StartTimeCode`, before `NoLoadUseCamera CAM_OVI_ID_STANDUP`).

| Question | Answer | Class |
|---|---|---|
| First `PlayMusic` / `MUSIC_SET_*` after Leave? | **none** — runner not on the tree | **PROVEN** |
| First leftover line if father later starts? | `PlayMusic MUSIC_SET_NULL` | **PROVEN** leftover |
| Is that leftover `S_QNOVI` itself? | **No.** Native quest object starts `CS_OAKVALE_INTRO_FATHER`. `S_QNOVI` is not in `script.bin`. | **PROVEN** leftover pairing |
| First leftover *named* set after `NULL`? | `MUSIC_SET_OAKVALE` | **PROVEN** leftover (same def, later PC) |
| Frontend `0042DED5` as `PlayMusic` / `MUSIC_SET_*`? | **No.** Different object; name **UNREAD**. | **DISPROVEN** |
| `CacheMusic` / `StopMusic` / `UseTheme` after Leave? | **No.** Same runner. | **DISPROVEN** as first-seen |
| Who starts `Q_NewOakValeIntro` on no-save? | not Leave / not `004B4260` / not `00CE7670` | **UNREAD** |
| Player body of `vtbl+2784` | — | **UNREAD** |

**Answer:** first-seen after Leave is **no script music**.
Do not play `MUSIC_SET_NULL` or `MUSIC_SET_OAKVALE` from
`RequestNewGame`. Do not invent a Lookout / WLD track.

---

## Timeline (no-save New Game)

```
0042EC7C retail
  006286F0 ×3 PlayAVI
  0042DED5 [0x13B8394].vtbl+68     // frontend voice; NOT PlayMusic
  005952C3 UI show
  msg 15 → [retail+41]=1
0042F2A2 Leave frontend            // not 00DB86B0, not 00CC8EAC
  [0x13B8394].vtbl+72(0x1F4)       // 500 ms fade; keep voice
  0042EBB6 +41 skip vtbl+64 / vtbl+72(0)
0042F491 Init Game 004184BD
  Init World 004A6E30              // no music opcode
  00416953 Load FinalAlbion.wld
    00CD6E27 00CB5C90 bind Q_NewOakValeIntro / S_QNOVI / 00DBEF70
      BIND ONLY — not 00CB5AD0
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
  00CE7670 state 0
    00893610 Q_NewOakValeIntro = 0 → 009D8650 yield
```

`00CC8EAC` / `009E5120` / `vtbl+2784` / `MUSIC_SET_*` are
**not** on this list. **PROVEN.**

`EngineLifecycleTests.No_save_does_not_activate_Q_NewOakValeIntro`
locks: no `00DBDE40`, quest not in `ActivatedQuests` /
`Runtime.Quests`, Gameflow yield name is the wait, bind note
is `bind not 00CB5AD0`. **PROVEN.**

---

## 1. Not the frontend voice

| Claim | Class | Evidence |
|---|---|---|
| Who starts frontend audio | **PROVEN** | After last AVI, `0042F00A call 0042DED5`. `[0x13B8394].vtbl+68`. |
| Track / file name | **UNREAD** | `0x1230C3C` / `0x1230C48` not in `strings.tsv`. |
| That call is `PlayMusic` / `MUSIC_SET_*` | **DISPROVEN** | Different singleton. No `00CC8EAC`. No frontend.bin sound field. |
| Leave New Game stops that voice | **DISPROVEN** | `+41!=0` fades 500 ms and skips teardown stop. |
| That voice is first *post-Leave* `MUSIC_SET_*` | **DISPROVEN** | Pre-Leave site. Unnamed file. Still playing (fading) after Leave. |

See `proofs/audio-frontend/README.md`. This note stops there.

`Play2DSound UI_CLICK` is also **DISPROVEN** as frontend /
Leave (`00CBF89E` leftover helper only).

---

## 2. No script music after Leave

`PlayMusic` lives only in runner `00CBFB7D` (token
`00BFEAF8` at `00CC8EAC`). After Leave the pump starts
quest factories, not that loop.

| Claim | Class | Evidence |
|---|---|---|
| `00CBFB7D` on Leave / Init Game / first type-1 | **DISPROVEN** | no `E8`; empty `CS_PlayCutscene`; Gameflow yield |
| WLD initial `S_PSM` / `S_HB` / `S_GF` interpreter | **DISPROVEN** | `HasStarted==false` |
| `CS_PlayCutscene` first line | **DISPROVEN** | factory `00F01760` empty; `ScriptName==null` |
| Lookout TNG / WLD region field starts `MUSIC_SET_*` | **DISPROVEN** as a recovered first-seen site | no opcode on this tree; do not invent ambience |
| `CacheMusic` `00CC8E1B` / `vtbl+2792` | **DISPROVEN** as first-seen | same runner |
| `StopMusic` `0x012C18F8` | **DISPROVEN** as first-seen | token exists; `TokenSite` apply **0** in map; not on this tree |
| `UseTheme` `00CCFA38` (`vtbl+2624` / `RESET` `+2628`) | **DISPROVEN** as first-seen | not on father head; not on Leave |
| Host `RequestNewGame` / `ActivateNamedQuest` plays a track | **DISPROVEN** | `EngineLifecycle` has no `PlayMusic`; `LastMusic` only via leftover `StartNewGame` |

Do **not** invent a first-seen track by grepping `script.bin`
for `MUSIC_SET_` or by walking `GlobalDispatcher` (host
if-chain starts at `PlayMusic`; native first *compared*
loop token is `.WaitTask` `00CC0783`).

---

## 3. Opcode (when the leftover runner *does* hit it)

Two different sites share the label “PlayMusic.” Do not
collapse them.

### 3a. Interpreter token `00CC8EAC` (father `[0]`)

Dump: `playmusic-interpreter-00cc8eac-00cc8eac.md`.

```
00CC8EAC  00BFEAF8("PlayMusic")     // prefix vs line verb
          miss → 00CC8F4B PlaySound
00CC8EFE  00403A00(arg0) == 0  → jmp 00CD17FD     // empty skip
00CC8F0B  ecx = [0x143E900]
          009E5120(arg0)
          eax == -1            → jmp 00CD17FD     // miss skip
00CC8F23  [0x143E8F8].vtbl+2784(id, 1, edi)
00CC8F35  00CBEE0C(arg1) → [ebp-58]               // IsFalse 2nd arg
          jmp 00CD17FD                              // no vtbl+28
```

`FirstSeenPlayMusicDoesNotYield=true`. **PROVEN.**

Second arg on leftover `[0]` is absent (`PlayMusic MUSIC_SET_NULL`).
`00CBEE0C` still runs. What `[ebp-58]` gates later is **UNREAD**
as a first-seen Leave site (never reached). Father later
`PlayMusic MUSIC_SET_NULL,FALSE` lives on
`CS_OAKVALEINTRO_HESDEADJIM`, not this leftover.

`edi` as the third `vtbl+2784` argument is **UNREAD** (do not
name it fade / loop). Helper arm (below) pushes **0** instead.

### 3b. Leftover helper `00CBF7FE` (not the opcode apply)

Dump: `playmusic-helper-00cbf7fe-00cbf7fe.md`. 69 `E8` callers
are post-verb leftover walks (`LookToThing`, `DoScriptFrame`,
…), not `00CC8EAC`.

Inside the helper, after `Play2DSound` (`vtbl+2768`):

```
00CBF8F3  "PlayMusic"
00CBF924  ecx = [0x143E900]
          009E5120
          eax == -1 → skip
00CBF938  [0x143E8F8].vtbl+2784(id, 1, 0)
00CBF94B  [0x143E8FD] = 1
          then "StopMusic"
```

`COMMAND_MAP` / `ScriptCommandMap` list ApplySite `00CBF7FE`.
That is the **helper arm**, not the inline apply at
`00CC8F0B`. Pairing is **PARTIAL**. Father `[0]` uses **3a**.

### 3c. `CacheMusic` is not PlayMusic

Token `00CC8E1B` / apply `00CC8E6D`: same `009E5120`, miss
skip, then **`vtbl+2792`**, `jmp 00CD17FD`. Not 2784.
Not on the leftover father head.

---

## 4. Leftover first *script* music (not Leave)

When `Q_NewOakValeIntro` later runs:

```
00CD6E27 bind only (already done on Load world)
00DABAC0  slot 2  registers NOVI_LiveFather (00DAC2C0)
          E8 00DBDE40 StartOakVale          // only caller
004C97B0 → 00CB8960 → 00DB8520 → 00DAC2C0
          vtbl 012D8388
00DB8630  [+52].vtbl+4 = 00DB86B0
00DB86B0  bind Hero / Father
          00CBFB7D("CS_OAKVALE_INTRO_FATHER")
```

`S_QNOVI` is **not** a `script.bin` entry
(`FirstSeenScriptBinHasSqnovi=false`). It is the native
factory `00DBEF70` / ctor `00DAAC00` / vtbl `012D7A28`.
The first leftover *line* is a `CCutsceneDef` the quest
starts, not a verb on `S_QNOVI` itself.

Dump: `script-bank/0481-cs-oakvale-intro-father.md`.
Pinned: `DataCatalogTests` `commands[0]`,
`WorldSceneTests` `intro.Executed[0] == IntroPlayMusic`.

| PC | Line | Token | Apply | Return |
|---:|---|---|---|---|
| 0 | `PlayMusic MUSIC_SET_NULL` | `00CC8EAC` | `009E5120` + `vtbl+2784` | `jmp 00CD17FD` |
| 1 | `FadeOut 0.5,0` | `00CD0987` | `vtbl+1488` | Continue |
| … | (teleports, LookToThing yield, frames, AVI, mute, wake anims) | | | |
| 16 | `StartTimeCode` | `00CD1373` | `[0x13B83C8]&=0` | Continue |
| 17 | `PlayMusic MUSIC_SET_OAKVALE` | `00CC8EAC` | same 2784 | Continue |
| 18 | `NoLoadUseCamera CAM_OVI_ID_STANDUP` | `00CC9E6A` | leftover bind | Yield |

Head special-case `00CBFDD0` compares `commands[0]` to
`"FadeOut 0.5,0"`. First line is PlayMusic → **skip**.
`FirstSeenFadeSpecialCaseRuns=false`. **PROVEN** leftover.

Host `ScriptRuntime.StartNewGame` drives that leftover and
stores `LastMusic = MUSIC_SET_NULL`, then later
`MUSIC_SET_OAKVALE`. That test is **PROVEN** as Oakvale VM
behaviour and **DISPROVEN** as what Leave starts.
`runtime-trace.txt` frame 0 is that leftover façade.

`FirstSeenStartsIntroCutscene=true` is the leftover
`00DABAC0` → `00DB86B0` pairing, **not** Leave.

---

## 5. `MUSIC_SET_NULL` vs a file

| Item | Class | Evidence |
|---|---|---|
| Name is a bank string, not empty | **PROVEN** | def+60 ASCII; empty arg0 would skip before `009E5120` |
| Empty arg0 skips `vtbl+2784` | **PROVEN** | `00CC8F03 je 00CD17FD` |
| Lookup miss (`eax==-1`) skips `vtbl+2784` | **PROVEN** | `00CC8F1A` |
| Whether `MUSIC_SET_NULL` is a mapped id or `-1` | **UNREAD** | no `009E5120` body dump in this packet |
| Host `LookupMusic("MUSIC_SET_NULL")` | **DIVERGE** vs inventing a file | strips `MUSIC_SET_`, stem `NULL` → no `Sound/*.ogg` |
| Host still stores `Audio.Music = "MUSIC_SET_NULL"` | **PROVEN** host | `GlobalDispatcher` / `WorldSceneTests` |
| Comment “native still calls vtbl+2784 with id 0” | **PARTIAL** / possibly **DIVERGE** | interpreter skips on `-1`; id-0 path not in the 60-insn dump |
| `MUSIC_SET_OAKVALE` → `data/Sound/OAKVALE.ogg` (or case twin) | **PROVEN** host file | `LookupMusic` + architecture test |
| What `vtbl+2784(id, 1, edi)` plays | **UNREAD** | player body not in this walk |

Do **not** treat `MUSIC_SET_NULL` as “silence no-op” *or* as
a titled forest loop. Bank name **PROVEN**. Player
**UNREAD**.

---

## 6. C# vs native after Leave

| Host | Native after Leave | Class |
|---|---|---|
| `GlobalDispatcher.PlayMusic` | unused | **LEFTOVER** vs Leave |
| `ScriptRuntime.StartNewGame` first line | invented Oakvale TNG + father | **DIVERGE** |
| `NewGameScript.PlayMusicRan` | leftover father getter | **LEFTOVER** |
| `EngineLifecycle.RequestNewGame` | `0042F2A2` → `004B4260` / Gameflow wait | **PROVEN** pairing; no music |
| `Note(0042DED5)` only | frontend vtbl+68 | **DISPROVEN** as `MUSIC_SET_*` |
| Leave fade 500 ms | `vtbl+72(0x1F4)` | **PROVEN** site; host has no player |
| `LookupMusic` stem→ogg | `009E5120` + `[0x143E900]` | **PARTIAL** (file analog; bank id UNREAD) |

Tests that assert `intro.Executed[0] == "PlayMusic MUSIC_SET_NULL"`
are **PROVEN** leftover VM and **DISPROVEN** as first after
Leave.

---

## Classifications (short)

1. **First `PlayMusic` / `MUSIC_SET_*` after Leave — none.
   PROVEN.** Runner not entered. `S_QNOVI` bind only.
2. **Frontend voice is not `MUSIC_SET_*`. PROVEN** as a
   different call. Name **UNREAD**.
3. **First leftover script music — `PlayMusic MUSIC_SET_NULL`
   via `00DB86B0` → `00CBFB7D`. PROVEN leftover.**
   `S_QNOVI` is the native quest that *would* start that
   cutscene, not the opcode itself.
4. **Second leftover set in that def — `MUSIC_SET_OAKVALE`.
   PROVEN leftover** (after AVI / wake / `StartTimeCode`).
5. **Apply is inline `009E5120` + `vtbl+2784`, no yield.
   PROVEN.** Helper `00CBF7FE` is a different leftover arm.
6. **Who activates `Q_NewOakValeIntro` on no-save — UNREAD.**
   Do not invent `ActivateQuest` from Leave.
7. **`vtbl+2784` player — UNREAD.** Do not invent an ogg
   for `NULL` and do not play Oakvale from Init Game.

Do not start New Game music at `00CC8EAC`. Do not play
`MUSIC_SET_NULL` / `MUSIC_SET_OAKVALE` / `MUSIC_SET_TITLE`
as the first post-Leave track.
