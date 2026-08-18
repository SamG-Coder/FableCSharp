# `004B4260` walks QST `AddQuest` TRUE (`world+172`)

Authority: `listing-00480000.txt` `004A0D90` /
`004A10C4` / `004A08D0` / `0049F24E`;
`listing-00cc0000.txt` `00CD9A12`; TLC
`FinalAlbion.qst` + `GlobalQuests.qst`.
WLD `START_INITIAL_QUESTS` is **not** the writer
(`00507C30` has no that case).

## Native list after both files

1. `Q_SunnyvaleMaster`
2. `ChapterAndSceneManager` (`00CB5AD0` miss)
3. `PersonalScriptMain`
4. `PersonalScript_GlobalThings`
5. `NPCDeath` (`00CB5AD0` miss)
6. `HeroBoasts`
7. `V_HeroDolls`
8. `CS_PlayCutscene`
9. `Global_WatchForHeroDeath` (`00EE90A0`)

Then `user.ini` `ActivateQuest("Gameflow")` — not in
`+172` (`AddQuest` FALSE).

`Q_NewOakValeIntro` is FALSE + `AddTestQuest` →
`+196` only. **DISPROVEN** on this walk.

## Host

`LoadQuestDefs` flag 1 `FinalAlbion.qst` then flag 0
`GlobalQuests.qst`. `WorldPlus172` is the TRUE slice.
`InitCharactersAndQuests` walks that list, not
`World.InitialQuests`. WLD parse stays a file table.
