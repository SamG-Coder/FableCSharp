# First dialogue / speech after Leave Frontend

Investigation only. No production `src` edits.

Do **not** start at Oakvale / `Father.Speak` / `CS_OAKVALE_INTRO_FATHER`.
That path is later `Q_NewOakValeIntro` (`00DABAC0` → `00DBDE40` →
`00DB86B0`), not Leave / Init Game / first no-save type-1 pump.

Do **not** chase frontend music `0042DED5` / `0x1230C3C` /
`0x1230C48`. Track name is **UNREAD**. That is the AVI→frontend
audio start, not post-Leave speech. See
`proofs/audio-frontend/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: `docs/runtime/FORWARD_TREE.md` §§4–11;
`proofs/newgame-script/README.md`; `proofs/audio-frontend/README.md`;
`proofs/script-command-map/README.md`; `proofs/camera-after-leave/README.md`;
`EngineLifecycle.cs` (`TickGameflowMain` / `TickCoreReminder` /
`TickConversations` / `TickSpeechGain`);
`EngineLifecycleTests.Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`;
`WorldSceneTests` father Speak constants (leftover);
ExeIndex `listing-00cc0000.txt` (`00CE7670` / `00CEF3B0`),
`listing-00c80000.txt` (`00CBE87F`), `listing-006c0000.txt`
(`006E37D0` / `006E3EC0`), `listing-00e00000.txt` (`00E0F1D0`).

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| Script `Speak` / `InteractiveSpeak` / `DialogSpeak` / `DialogadSpeak` after Leave? | **No.** Tokens `00CC25FD` / `00CC2EAA` / `00CC3165` / `00CC3354` are not on the Leave / `004184BD` / type-1 tree. | **DISPROVEN** as first-seen |
| Conversation walk after Leave? | `006E60F0` / ctor `006E6150` `[+8]=self` → empty. `ConversationWalked=0`. | **PROVEN** empty |
| Speech-gain voices after Leave? | `006E37D0` `[0x13BABA0]` circular empty. | **PROVEN** empty |
| First *text* Gameflow writes? | `00CBE87F(10)` → `TEXT_QST_LOG_STORY_10` via context `vtbl+1232`. Journal NAME/DESC, not `Speak`. | **PROVEN** |
| First guild-seal line the code is armed to play? | `00CEF3B0` `TEXT_QST_078_GM_MSG_NEW_QUEST_AT_GUILD` via `vtbl+1096`. | **DISPROVEN** first-seen (`[+72]=0` yield) |
| `TEXT_QST_078_GM_MSG_FIRST`? | `00E0F1D0` on `Q_WaspBoss` `WatchForTermination` after `PicnicArea`. | **DISPROVEN** as Leave / first pump |
| `Father.Speak TEXT_QST_048_FATHER_INTRO_10`? | `S_QNOVI` leftover. Leave never constructs that quest. | **LEFTOVER** |
| Frontend music as first post-Leave speech? | `0042DED5` is pre-Leave; name **UNREAD**. New Game Leave only `vtbl+72(500)`. | **DISPROVEN** pairing |

**Answer:** first-seen after Leave is **no spoken line**. Managers
construct empty. Gameflow state 0 writes the story-log journal key
then yields on `Q_NewOakValeIntro`. Guild `TEXT_QST_078_*` and
script `Speak` are later / leftover.

---

## Timeline (no-save New Game)

```
0042EC7C retail
  006286F0 ×3 PlayAVI
  0042DED5 [0x13B8394].vtbl+68     // frontend music; name UNREAD
  005952C3 UI show
  msg 15 → [retail+41]=1
0042F2A2 Leave frontend            // not 00DBDE40, not Speak
  [0x13B8394].vtbl+72(0x1F4)       // 500 ms fade; keep voice
  0042EBB6 +41 skip vtbl+64 / vtbl+72(0)
0042F491 Init Game 004184BD
  "Init Subtitled Message"         004CDB10  (register only)
  "Init Conversation Attitude"     004CD670
  Init World 004A6E30
    "Init Script Conversation Manager"  alloc 20 → 006E6150
    "Init Speech Gain Manager"          006E3EC0 [0x13BABA0]
  004B4260 START_INITIAL_QUESTS    // no Speak; S_PSM not started
  user.ini ActivateQuest("Gameflow")
004189C2 first type-1 004A5A40
  004B4490 / 00CB8220
    00CE7670 state 0
      attach CoreQuestReminder 00CEF3B0
      attach CheckBarrowFieldsGuards 00CEF550
      00CE77D7 [SharedRun+4]=0
      vtbl+2664(1) PlayAVI flag
      tattoo GiveNamedObject miss
      00CBE87F ecx=0xA → TEXT_QST_LOG_STORY_10     // journal
      00896A30 OBJECT_QUEST_CARD_OAKVALE_INTRO miss
      00893610 Q_NewOakValeIntro = 0 → 009D8650 yield
    same walk: 00CEF3B0 [+72]=0 → yield            // no TEXT_QST_078
               00CEF550 trader miss → yield
  006E60F0 conversation empty
  006E37D0 speech-gain empty
```

`S_QNOVI` / `00DB86B0` / `Father.Speak` / `00CC25FD` are **not**
on this list. **PROVEN**.

---

## 1. Not the frontend music UNREAD path

| Claim | Class | Evidence |
|---|---|---|
| Who starts frontend audio | **PROVEN** | After last AVI, `0042F00A call 0042DED5`. Singleton `[0x13B8394].vtbl+68`. |
| Track / file name | **UNREAD** | `0x1230C3C` / `0x1230C48` not in `strings.tsv`. Do not invent `MUSIC_SET_*`. |
| Script `PlayMusic` on frontend | **DISPROVEN** | First `PlayMusic` is leftover `S_QNOVI` / father cutscene. |
| Leave New Game stops that voice | **DISPROVEN** | `+41!=0` fades 500 ms and skips teardown stop. |
| That voice is first post-Leave *speech* | **DISPROVEN** | Different object, pre-Leave site, unnamed file. Not `Speak`, not `TEXT_QST_*`. |

See `proofs/audio-frontend/README.md`. This note stops there.

---

## 2. Dialogue objects constructed after Leave (empty)

Init Game / Init World after `0042F2A2`:

| Order | VA | Object | First-seen use | Class |
|---|---|---|---|---|
| 1 | `004CDB10` | Init Subtitled Message | Register at `[0x13B8A54]` via `00A39010`. No line queued. | **PROVEN** construct. **DISPROVEN** as a spoken line. |
| 2 | `004CD670` | Init Conversation Attitude | Named stage only. | **PROVEN** site. Body not needed for first-seen empty. |
| 3 | `006E6150` | Script Conversation Manager `world+124` | `[node+8]=self` so `006E60F0` walks nothing. | **PROVEN** empty |
| 4 | `006E3EC0` | Speech Gain Manager | Seeds `[0x13BABA0]`. Tick `006E37D0` `cmp [eax],eax` → `je 006E3875`. | **PROVEN** empty |

Type-1 after `004A5E10` WorldFrame inc still hits `006E37D0` empty
(`EngineLifecycleTests` locks `SpeechGainTicked` and the empty
note). **PROVEN**.

`008906C0` (dialog begin `vtbl+1456` → `006E61A0`) is the later
`InteractiveSpeak` / `DialogSpeak` handle. Zero callers on this
tree. **DISPROVEN** as first-seen.

---

## 3. First text after Leave is a journal key, not Speak

`00CE7670` state 0 (`00CE77D7` jump-table slot; `[ecx+4]=0`):

```
00CE77DA  [esi+64].vtbl+2664(1)     PlayAVI flag
00CE77EC  vtbl+24
          al!=0 → 00CE7905
          else GiveNamedObject tattoo cards (00487DC0 miss)
00CE7905  mov ecx, 0xA
00CE790A  call 00CBE87F
00CE7915  vtbl+1524(0)
00CE7957  vtbl+1180 Q_NewOakValeIntro + OBJECT_QUEST_CARD_OAKVALE_INTRO
00CE7995  vtbl+100 00893610 → 0
          → 00CB7940 / 006E7410 / 009D8650 yield
```

`00CBE87F` builds `TEXT_QST_LOG_STORY_` + decimal `ecx`, then
`NAME` / `DESC` suffixes, then `[0x143E8F8].vtbl+1232(...)`.
First-seen `ecx=0xA` → **`TEXT_QST_LOG_STORY_10`**.

That is a story-log bind, not:

- `.Speak` `00CC25FD` / apply `00CC27EA` `vtbl+52`
- guild-seal `vtbl+1096`
- conversation insert `006E61A0`

Host notes `00CBE87F TEXT_QST_LOG_STORY_10` on the first Gameflow
tick. **PROVEN** site and key. UI presentation of the journal row
is **UNREAD** (no invented subtitle).

---

## 4. First *guild speech* candidate does not fire

Same `00CB7C40` walk, insert-at-tail, after Main yield:

### 4a. `CoreQuestReminder` `00CEF3B0`

```
00CEF3C4  [esi+72]
00CEF3C7  test eax,eax
00CEF3C9  jne 00CEF3EE          // have reminder
00CEF3D0  vtbl+28 / 00CB7940    // yield while +72==0
…
00CEF4B0  push "TEXT_QST_078_GM_MSG_NEW_QUEST_AT_GUILD"
00CEF4D1  [esi+64].vtbl+1096(text, empty, 1, 0)
```

First-seen `[+72]=0` → yield. No `vtbl+1096`. PARITY and
`Type1_00CB8220_*` lock the `+72]=0` note. **PROVEN** skip.

Who writes Gameflow `+72` on no-save is **UNREAD**. Do not invent
a timer that pops the guild line on Lookout Present.

`vtbl+1096` is the guild-seal / HUD message channel (same slot as
`008913F0`: `HUD_ORB_QUEST_CORE` + `TEXT_QST_078_GM_MSG_NEW_QUEST`).
**PARTIAL** as “speech”: it is a TEXT_QST line, not `Speak`.

### 4b. `TEXT_QST_078_GM_MSG_FIRST` `00E0F1D0`

Stored at `00E0F079` as `WatchForTermination` `+52` from
`Q_WaspBoss` (`WaspChaser` / `PicnicArea` in the same body).
`vtbl+1096("TEXT_QST_078_GM_MSG_FIRST")` only after a PicnicArea
wait. `Q_WaspBoss` is a Lookout TNG *section*, not
`START_INITIAL_QUESTS`. **DISPROVEN** as first after Leave.

### 4c. Other `00CE7670` `TEXT_QST_078_GM_MSG_*`

`KNOTHOLE_GLADE_CLOSED`, `FOUND_TROPHY_DEALER`, `MAZE_AGAIN`, …
sit on later state-table arms (`cmp [esi+68]+4` vs `0xC8` / `0x12C`
/ …). First-seen state is **0**. **DISPROVEN** as first-seen.

---

## 5. Script Speak is leftover Oakvale

| Item | On Leave / first type-1? | Class |
|---|---|---|
| `.Speak` `00CC25FD` / `00CC27EA` `vtbl+52` | no | **LEFTOVER** interpreter |
| `Father.Speak Father,'TEXT_QST_048_FATHER_INTRO_10'` | no | **LEFTOVER**. `WorldSceneTests` / `RegionTravel.IntroFatherSpeak` |
| `InteractiveSpeak` `00CC2EAA` / `TEXT_QST_048_FATHER_INTRO_20` | no | **LEFTOVER** |
| `DialogSpeak` `00CC3165` / `_60` | no | **LEFTOVER** |
| `DialogadSpeak` `00CC3354` / `_100` | no | **LEFTOVER** |
| Father cutscene first line | `PlayMusic`, not Speak | **LEFTOVER** (`FirstSeenFadeSpecialCaseRuns=false`) |
| `ScriptRuntime.StartNewGame` drives those lines | host only | **DIVERGE** vs Leave |
| `CS_PlayCutscene` factory `00F01760` | empty; `ScriptName==null` | **PROVEN**. No `00CBFB7D`. |
| `S_PSM` / `S_HB` / `S_GF` interpreter | `HasStarted==false`; Gameflow is Main watcher | **DISPROVEN** as Speak runner |

`proofs/script-command-map/README.md` §5: first-seen after Leave
has **no** `00CBFB7D` verbs.

Lookout TNG has `TalkingTrader1/2`, `V_BeggarAndChild`,
`Q_GuildTraining`, `Q_WaspBoss`, `Gameflow:2` sections. Those
Things exist after `00521AE0`. Their *quests* are not in
`world+172`. Pose / AI activate is **UNREAD**. Do not invent
trader chatter on first Present.

---

## 6. C# vs native

| Host | Native after msg 15 | Class |
|---|---|---|
| `TickSpeechGain` empty note | `006E37D0` empty | **PROVEN** |
| `TickConversations` empty | `006E60F0` empty | **PROVEN** |
| `TickGameflowMain` notes `00CBE87F TEXT_QST_LOG_STORY_10` | `00CE790A` `ecx=0xA` | **PROVEN** pairing |
| `TickCoreReminder` `[+72]=0` yield | `00CEF3C7` | **PROVEN** |
| `Runtime.Speeches` after `RequestNewGame` | empty | **PROVEN** vs Leave (if host does not `StartNewGame`) |
| `StartNewGame` / `NewGameScript` queues father Speak | unused native | **DIVERGE** / **LEFTOVER** |
| Host frontend music player | none; `0042DED5` Note only | **DISPROVEN** as speech |

---

## Classifications (short)

1. **First spoken dialogue after Leave — none. PROVEN empty.**
   Conversation list and speech-gain list are empty. No `Speak`
   opcode. No `vtbl+1096` guild line.
2. **First Gameflow text — `TEXT_QST_LOG_STORY_10` via `00CBE87F`.
   PROVEN.** Journal bind, not speech.
3. **First armed guild line — `TEXT_QST_078_GM_MSG_NEW_QUEST_AT_GUILD`
   in `00CEF3B0`. DISPROVEN first-seen** (`[+72]=0`). Writer of
   `+72` is **UNREAD**.
4. **`Father.Speak TEXT_QST_048_FATHER_INTRO_10` — LEFTOVER.**
   Requires `Q_NewOakValeIntro`, which Leave does not activate.
5. **Frontend music UNREAD name — not this path.** Pre-Leave
   `0042DED5`. Do not invent a track and do not call it speech.

Do not start New Game dialogue at `00DB86B0`. Do not play
`MUSIC_SET_*` or `UI_CLICK` as the first post-Leave voice.
