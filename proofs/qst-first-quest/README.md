# First quest name after Leave: `FinalAlbion.qst` / `GlobalQuests.qst`

Investigation only. No production `src/` edits.

Do **not** start at `Q_NewOakValeIntro` / `S_QNOVI` / `00DBDE40`.
That name is `AddQuest(..., FALSE)` plus `AddTestQuest` in
`FinalAlbion.qst`. Leave never activates it.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: `src/Fable.Formats/Qst/QuestFile.cs`;
`GameInstall.QuestPath` / `GlobalQuestPath`;
TLC `data\Levels\FinalAlbion.qst`, `data\Levels\GlobalQuests.qst`;
`EngineLifecycle.LoadQuestDefs` (`0049D770` / `004A0D90`);
`docs/runtime/FORWARD_TREE.md` §10; `docs/PARITY.md` QST / Init Quests;
`proofs/wld-parse/README.md`; `proofs/newgame-script/README.md`;
`TlcInstallTests.Quest_table_includes_opening`;
`WorldSceneTests.Global_quests_file_parses_watch_for_hero_death`;
`EngineLifecycleTests` (`LoadWorld_004A1840_after_wad_is_00507C30_then_empty_006C20A0`,
`Init_quests_004B4260_activates_wld_initial_list`,
`Activate_quests_00CB5AD0_starts_factory_scripts`).

---

## Verdict

**First quest name after Leave is `Q_SunnyvaleMaster`.**

Leave records `FinalAlbion.wld`. `004A1840` derives
`Data\Levels\FinalAlbion.qst` (`0049D770`) and parses it with
`004A0D90` **before** `GlobalQuests.qst`. `QuestFile.Parse` walks
`AddQuest("name", TRUE|FALSE)` in file order. The first match in
the master table is `Q_SunnyvaleMaster` / persistent **TRUE**.

`GlobalQuests.qst` is second. Its first `AddQuest` is
`Global_WatchForHeroDeath` / persistent **TRUE**. That is **not**
the first name on this walk.

`Q_NewOakValeIntro` is in the same master file as `FALSE`.
**DISPROVEN** as first.

---

## Path after Leave

```
0042F2A2  Leave frontend
  0042F44D  FinalAlbion.wld
00416953  Load world
  004A1840
    0049D770  Data\Levels\ + WLD stem + .qst
              → Data\Levels\FinalAlbion.qst     PROVEN
    004A0D90  AddQuest / AddTestQuest
              first AddQuest = Q_SunnyvaleMaster TRUE
    0x01238F38 Data\Levels\GlobalQuests.qst exists
    004A0D90  first AddQuest = Global_WatchForHeroDeath TRUE
    004FDAB0 / Startup WAD / 00507C30 / Set Static Map
  [0x13B8648]==0
    0049F180  Init Quests
      004B4260([world+172])
        first activate = Q_SunnyvaleMaster
    +90584 empty → 004B4A10
user.ini  ActivateQuest("Gameflow")             7th, not QST-first
```

Order of the two files: **PROVEN**
(`LoadWorld_004A1840_after_wad_is_00507C30_then_empty_006C20A0`).

---

## 1. `QuestFile.cs`

`C:\FableCSharp\src\Fable.Formats\Qst\QuestFile.cs` is a regex over

```
AddQuest( "name" , TRUE|FALSE )
```

It does **not** parse `AddTestQuest`. First `AddTestQuest` in
`FinalAlbion.qst` is `Gameflow` / `NOVStartHSP` — a later leftover
row, not the first name after Leave.

`GameInstall`:

| Property | Path |
|---|---|
| `QuestPath` | `data\Levels\FinalAlbion.qst` |
| `GlobalQuestPath` | `data\Levels\GlobalQuests.qst` |

`EngineLifecycle.LoadQuestDefs` loads `QuestPath` into
`life.Quests` and only **notes** `GlobalQuests.qst`. Host does not
merge the two lists into `QuestFile`. Native still parses both
(`004A0D90` twice).

---

## 2. Shipped `AddQuest` heads

### `FinalAlbion.qst` (187 `AddQuest`)

| # | Name | Persistent |
|--:|---|---|
| 1 | **`Q_SunnyvaleMaster`** | TRUE |
| 2 | `ChapterAndSceneManager` | TRUE |
| 3 | `PersonalScriptMain` | TRUE |
| 4 | `PersonalScript_GlobalThings` | TRUE |
| 5 | `NPCDeath` | TRUE |
| 6 | `HeroBoasts` | TRUE |
| … | `Gameflow` | FALSE |
| 45 | `V_HeroDolls` | TRUE |
| 129 | `Q_NewOakValeIntro` | FALSE |
| 179 | `CS_PlayCutscene` | TRUE |

Eight `TRUE` rows total. `QuestFile` first element is
`Q_SunnyvaleMaster`. **PROVEN** (file head + regex).

### `GlobalQuests.qst` (13 `AddQuest`)

| # | Name | Persistent |
|--:|---|---|
| 1 | **`Global_WatchForHeroDeath`** | TRUE |
| 2 | `Global_DebugCycleThroughSpeech` | FALSE |
| 3–9 | `Expression_*` | FALSE |
| 10–13 | `Global_TeleportToHeroGuild` … `Global_OpenChest` | FALSE |

One `TRUE` row. First name is `Global_WatchForHeroDeath`.
**PROVEN** (`WorldSceneTests`).

---

## 3. First *activated* name is the same string

`004B4260` first-seen list (WLD `START_INITIAL_QUESTS` /
`life.ActivatedQuests.Take(6)`):

1. `Q_SunnyvaleMaster`
2. `PersonalScriptMain`
3. `PersonalScript_GlobalThings`
4. `HeroBoasts`
5. `V_HeroDolls`
6. `CS_PlayCutscene`

Then `user.ini` `Gameflow` (7th). **PROVEN**
(`Init_quests_004B4260_activates_wld_initial_list`).

That six-name list is **not** “every `AddQuest` TRUE”.
`ChapterAndSceneManager`, `NPCDeath`, and
`Global_WatchForHeroDeath` are persistent TRUE and are **not**
in the first-seen activate list. `wld-parse` already classifies
“`world+172` = every QST TRUE” vs the WLD six as **PARTIAL**.
The first string is still `Q_SunnyvaleMaster` either way.

Factory for that first activate: `00CDD550` / init `00CDBD20`.
No `ScriptName`. Not a `CCutsceneDef`. **PROVEN**.

---

## 4. What this is not

| Claim | Class |
|---|---|
| First QST name is `Q_NewOakValeIntro` / `S_QNOVI` | **DISPROVEN** (`FALSE`; not in `004B4260`) |
| First QST name is `Gameflow` | **DISPROVEN** as QST-first (`FALSE`; 7th activate from `user.ini`) |
| First QST name is `Global_WatchForHeroDeath` | **DISPROVEN** as first (second file) |
| `QuestFile` first = `AddTestQuest` first | **DISPROVEN** (regex ignores `AddTestQuest`) |
| Host `life.Quests` includes `GlobalQuests.qst` | **DIVERGE** (notes only) |
| Who later `ActivateQuest(Q_NewOakValeIntro)` on no-save | **UNREAD** |

---

## Classifications (short)

1. **First name after Leave — PROVEN: `Q_SunnyvaleMaster`.**
   `FinalAlbion.qst` `AddQuest` line 1, persistent TRUE.
   Same string as first `004B4260` activate.
2. **`GlobalQuests.qst` first — PROVEN: `Global_WatchForHeroDeath`.**
   Parsed second. Not the first name on the Leave walk.
3. **`QuestFile.cs` matches that head — PROVEN.**
   `AddQuest("…", TRUE|FALSE)` only; `AddTestQuest` leftover.
4. **Oakvale intro as first QST row — DISPROVEN.**
