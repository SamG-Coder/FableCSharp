# First type-1 `00CB8220` after `004B4260`

Investigation only. No production `src/` edits.

Question: `00CB8220` type-1 quest pump after
`004B4260`. First-seen walk: which quests, how
many, which EventPosts? Host
`InitCharactersAndQuests` / type-1 counts
leftover vs native 12/10?

Do **not** treat WLD `START_INITIAL_QUESTS` as
the walk. Do **not** start Oakvale as New Game
(`Q_NewOakValeIntro` / `00DBDE40` /
`S_QNOVI`).

Status words: **PROVEN** / **PARTIAL** /
**UNREAD** / **DISPROVEN** / **LEFTOVER** /
**DIVERGE**.

Authority: Fable.exe dump + QST.

- `listing-00480000.txt` `004B4260` /
  `004B3CE0` `004B4042` `00687540(55,50)` /
  `004B4063` stub / `004B4490` `004B453E` /
  `004A5D82`
- `listing-00c80000.txt` `00CB8220` /
  `00CB7C40` / `00CB7950` / `00CB8170`
- `listing-00680000.txt` `006874B0` /
  `00687540`
- TLC `FinalAlbion.qst` + `GlobalQuests.qst`
- Host `EngineLifecycle.InitCharactersAndQuests`
  / `ActivateNamedQuest` / `PumpQuestList`
- Tests `Init_quests_004B4260_*` /
  `Type1_00CB8220_*` (host lock 12/10)

---

## Verdict table

| Question | Answer | Class |
|---|---|---|
| First `00CB8220` after `004B4260`? | first type-1 `004A5A40` → `004A5D88` `004B4490` → `004B453E`. Dummy inner skips. | **PROVEN** |
| List walked? | `[QM+56]` slots, then per-factory `[run+4]` watchers | **PROVEN** |
| WLD `InitialQuests` is that walk? | **No.** Six WLD names. Walk is QST `AddQuest` TRUE (`world+172`, nine) + later `user.ini` Gameflow | **DISPROVEN** |
| Oakvale / `Q_NewOakValeIntro` on the walk? | **No.** FALSE + `AddTestQuest`. Name wait only inside last factory `00CE7670` | **DISPROVEN** |
| How many `[QM+56]` visits? | **10** (9 QST TRUE + Gameflow) | **PROVEN** |
| How many `00CB8220`? | **8** (`[quest+8]!=0`). Skip `ChapterAndSceneManager`, `NPCDeath` | **PROVEN** |
| How many `00CB7950`? | **10** (7 `+172` Mains + Gameflow Main + Core + Barrow) | **PROVEN** |
| Which EventPosts? | `00687540(kind=55, delay=50)` at **construct** (`004B3CE0` factory arm). **8** first-seen. Type-1 `006874B0` walks them, fires **0** | **PROVEN** |
| New `00687540` inside `00CB8220`? | **No** `E8` in `00c80000` / Gameflow listings | **DISPROVEN** |
| Host `EventPosts==10`? | leftover: `ActivateNamedQuest` always `++`, including two factory-0 stubs | **LEFTOVER** / **DIVERGE** |
| Host `QuestPumpWalked==12`? | leftover: 9 WLD names (incl. stubs) + 3 Gameflow watchers. Not native `00CB8220` count | **LEFTOVER** / **DIVERGE** |
| Native is 12/10? | **No.** Native **8/8** (`00CB8220` / posts) or **8/10** (`00CB8220` / `00CB7950`). 12/10 is the host test lock | **DISPROVEN** as native |

---

## Timeline (no-save New Game)

```
004A1840 Load Quests
  FinalAlbion.qst flag 1  004A0D90 AddQuest TRUE → world+172
  GlobalQuests.qst flag 0 append
0049F24E 004B4260([world+172])            // NOT WLD InitialQuests
  004B3CE0
    factory → 004B0310 [+8]=run 00CB7900
              00687540(55,50)             // 7 posts
    factory 0 → 004B4063 [+8]=0  no post  // 2 stubs
00418969 user.ini ActivateQuest("Gameflow")
  004B4A10 → 004B4260 → 004B3CE0
    00687540(55,50)                       // 8th post
    00CE75B0 attach Main only             // not 00CB8220
004189C2 first pumps
  dummy inner [game+260] not 0/9 → no 004B4490
  first type-1 004A5D82
    004B4490 walk [esi+56]
      8 × 00CB8220([quest+8])             // FIRST 00CB8220
        00CB7C40 [run+4] → 00CB7950
        jmp 00CB8170 [run+8] empty
      last factory Gameflow
        00CE7670 attach Core / Barrow then wait
    006874B0 walk 8 nodes; now+50 >= now → skip 006872B0
```

---

## 1. QST TRUE is the `004B4260` list — not WLD

`FinalAlbion.qst` `AddQuest(..., TRUE)` file
order (8):

1. `Q_SunnyvaleMaster`
2. `ChapterAndSceneManager`
3. `PersonalScriptMain`
4. `PersonalScript_GlobalThings`
5. `NPCDeath`
6. `HeroBoasts`
7. `V_HeroDolls` (later in file)
8. `CS_PlayCutscene`

`GlobalQuests.qst` TRUE append (1):

9. `Global_WatchForHeroDeath`

`Gameflow` is `AddQuest(..., FALSE)` — **not**
in `world+172`. It is the later `user.ini`
`ActivateQuest`. **PROVEN**.

WLD `START_INITIAL_QUESTS` (six):

`Q_SunnyvaleMaster` `PersonalScriptMain`
`PersonalScript_GlobalThings` `HeroBoasts`
`V_HeroDolls` `CS_PlayCutscene`.

`00507C30` has no that case. Using those six
as the type-1 walk **DISPROVEN**.

`Q_NewOakValeIntro` is FALSE + `AddTestQuest`
→ `world+196` only. **DISPROVEN** on this
pump.

---

## 2. First `00CB8220` is type-1 `004B4490`

`00CB8220` (`listing-00c80000.txt`):

```
00CB8220  push esi
00CB8221  mov esi, ecx
00CB8223  call 00CB7C40
00CB8228  mov ecx, esi
00CB822A  pop esi
00CB822B  jmp 00CB8170
```

One `.text` `call 00CB8220`: `004B453E`.
One `.text` `call 00CB7C40`: `00CB8223`.
Not `004B4260` / `00CE75B0` / `00CB7900`.
**PROVEN**.

`004B4490` (`listing-00480000.txt`):

```
004B4517  eax = [esi+56]
004B4522:
  eax = [edi+8]                 // 52-byte slot
  cmp [eax+8], ebx              // ebx=0
  je  004B4549                  // no run → skip
  ecx = [eax+8]
  call 00CB8220
  edi = [edi]
  cmp edi, [esi+56]
  jne 004B4522
```

Site: `004A5D82` `mov ecx,[0x13B89FC]` then
`004A5D88` `call 004B4490` when
`[game+260]==0` or `==9` (type-1). First
dummy inner does not take that. **PROVEN**.

`00CB7C40` walks **that factory’s**
`[this+4]`, not names. Head of the first
call is Sunnyvale `"Main"`. Factory 0 never
builds `[+4]`. **PROVEN**.

---

## 3. First-seen walk (quests / counts)

`[QM+56]` is tail-insert from first
`004B3CE0` then Gameflow.

| # | Name | `[+8]` | `00CB8220` | `00CB7950` |
|--:|---|---|---|---|
| 1 | `Q_SunnyvaleMaster` | run | **first** | Main `00CDD360` |
| 2 | `ChapterAndSceneManager` | **0** | skip | — |
| 3 | `PersonalScriptMain` | run | yes | Main `00CDDCB0` |
| 4 | `PersonalScript_GlobalThings` | run | yes | Main `00CDDCB0` |
| 5 | `NPCDeath` | **0** | skip | — |
| 6 | `HeroBoasts` | run | yes | Main `00CE1AF0` |
| 7 | `V_HeroDolls` | run | yes | Main |
| 8 | `CS_PlayCutscene` | run | yes | Main (empty def) |
| 9 | `Global_WatchForHeroDeath` | run | yes | Main |
| 10 | `Gameflow` (ini) | run | **last** | Main `00CE7670` then Core / Barrow |

`00CE7670` attaches `CoreQuestReminder` /
`CheckBarrowFieldsGuards` on **Gameflow**
`[this+4]` (insert-at-tail) during that last
`00CB7C40`. Same factory, not new `[QM+56]`
slots. **PROVEN**.

Counts on this first type-1:

| Token | N |
|---|---:|
| `[QM+56]` visits | 10 |
| `00CB8220` / `00CB7C40` / `00CB8170` | **8** |
| `00CB7950` | **10** |
| Factory-0 `00CB7950` | **0** |

`V_HeroDolls` / `Global_WatchForHeroDeath`
Main **bodies** stay **PARTIAL** (factory
exists; first-seen yield path not fully
dumped here). They **do** enter
`00CB8220`. **PROVEN** as walk members.

---

## 4. Which EventPosts

`004B3CE0` factory arm only
(`listing-00480000.txt`):

```
004B403E  push 50
004B4040  push 55
004B4042  call 00687540
```

Stub `004B4063` has **no** `00687540`.
**PROVEN**.

First-seen posts (kind **55**, delay **50**):

| When | Who | N |
|---|---|---:|
| first `004B4260` | 7 live `+172` factories | 7 |
| `user.ini` Gameflow | 1 | 8 |
| type-1 `00CB8220` | none | 8 |

`006874B0` (`004A5D99`, after `004B4490`):

```
[node+64] != 0
0049D870 now
cmp [node+56]+[node+60], now
jae skip                    // construct+50 >= now
```

`004A5E10` WorldFrame inc is **after** this
call. First-seen **no** `006872B0`.
**PROVEN** walk; **DISPROVEN** fire.

`004B2890` also `E8 00687540` (events 41/73)
but first-seen `+112` empty and no player
Thing — those posts are **not** on this
path (`004B2890-first`).

---

## 5. Host leftover vs 12/10

`InitCharactersAndQuests` walks
`_worldPlus172` (nine QST TRUE). Names
**MATCH**. Then later
`ActivateNamedQuest("Gameflow")`.

`ActivateNamedQuest` always:

```
Runtime.ActivateQuest(...)
EventPosts++
Note 00687540 kind=55 delay=50
```

including `ChapterAndSceneManager` /
`NPCDeath`. Native stub posts **0**. Host
`EventPosts==10`. Native **8**.
**DIVERGE**.

`PumpQuestList` `QuestPumpWalked++` on
every `TickNamedQuestMain` (all nine
`+172` names) plus Gameflow Main / Core /
Barrow → **12**. Native `00CB8220` is
**8**; native `00CB7950` is **10**. The
test lock `12` / `10` is **host**
(`QuestPumpWalked` / `EventPosts`), not
native. Treating 12/10 as dump counts is
**DISPROVEN**.

| Host | Native | Class |
|---|---|---|
| 9 `+172` names then Gameflow | same `[QM+56]` order | **MATCH** names |
| `EventPosts==10` | 8 `00687540(55,50)` | **DIVERGE** |
| `QuestPumpWalked==12` | 8 `00CB8220` / 10 `00CB7950` | **DIVERGE** |
| else-arm `00CB7950` on stubs | `[+8]==0` skip | **DIVERGE** |
| one trampoline note | one `00CB8220` per live factory | **DIVERGE** shape |
| `GameflowWaitQuest` as first walk | last factory `00CE7670` wait | **LEFTOVER** |
| WLD `InitialQuests` as activate list | unused on this path | **DISPROVEN** |

---

## Classifications (short)

1. **First `00CB8220` after `004B4260` —
   PROVEN** type-1 `004B4490` on live
   `[QM+56]` factories. First factory
   Sunnyvale. Not WLD six. Not Oakvale.
2. **How many — PROVEN** 10 slots, **8**
   `00CB8220`, **10** `00CB7950`.
3. **EventPosts — PROVEN** eight
   `00687540(55,50)` at construct. Type-1
   `006874B0` does not fire them. No post
   from the trampoline.
4. **Host 12/10 leftover — PROVEN
   leftover.** Native is 8/8 posts or
   8/10 watchers. 12/10 is host
   `QuestPumpWalked` / `EventPosts`.
