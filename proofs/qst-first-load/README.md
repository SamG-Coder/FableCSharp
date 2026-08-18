# QuestFile.cs / FinalAlbion.qst / GlobalQuests.qst first load after Leave

Investigation only. No production `src/` edits.

Do **not** start at `Q_NewOakValeIntro` / `S_QNOVI` / `00DBDE40`.
That name is `AddQuest(..., FALSE)` plus an `AddTestQuest` card.
It is **not** on the no-save `world+172` walk.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MISMATCH**.

Sources: `src/Fable.Formats/Qst/QuestFile.cs`;
`src/Fable.Game/EngineLifecycle.cs` (`LoadWorldMap` / `LoadQuestDefs` /
`InitCharactersAndQuests`);
`src/Fable.Core/GameInstall.cs`;
`docs/runtime/FORWARD_TREE.md` §10;
`docs/PARITY.md` (`004A1840` / GlobalQuests rows);
`proofs/wld-parse/README.md` (`world+172` from QST, not WLD);
`proofs/newgame-script/README.md`;
`EngineLifecycleTests.LoadWorld_004A1840_after_wad_is_00507C30_then_empty_006C20A0`;
ExeIndex `listing-00480000.txt` `0049D770` / `004A08D0` / `004A0D90` /
`004A1840`, `listing-00400000.txt` `00416ABA`, `listing-00cc0000.txt`
`00CD9A12`; TLC `Data\Levels\FinalAlbion.qst` + `GlobalQuests.qst`.

---

## Verdict

**First QST I/O after Leave is inside `004A1840` "Load Quests",
before Startup WAD and before `00507C30`.**

`0049D770` builds `Data\Levels\FinalAlbion.qst` from the WLD stem.
`00999230` exists → `004A0D90(..., 1)` (clears then parse).
Hard path `0x01238F38` `Data\Levels\GlobalQuests.qst` exists →
`004A0D90(..., 0)` (append, no clear).

`AddQuest` TRUE fills `CWorld+172`. That vector is what later
`0049F24E` `004B4260` activates. WLD `START_INITIAL_QUESTS` is
**not** the writer. **PROVEN** (`wld-parse`).

`QuestFile.cs` is a name+bool regex over `AddQuest` only.
Host `LoadQuestDefs` loads **FinalAlbion.qst** into `life.Quests`
and only **notes** GlobalQuests. Activation still walks
`World.InitialQuests` (the unused WLD block). That pairing is
**DIVERGE**.

---

## Timeline (no-save New Game)

```
0042F2A2 Leave frontend
  0042F44D  FinalAlbion.wld → game+90576
0042F491 Init Game → 00418DCA → 004184BD
  Init World 004A6E30
    004A68AE  [world+172/+176/+180]=0
    00CD52D0  factory fill (bind only)
  00416953 vtbl+32 Loading world
    00416ABA  call 004A1840(world, path)     // site, not a function
      0049DDD0  stem from WLD path
      0049D770  Data\Levels\ + stem + .qst   // 0x01238C40
      00999230  exists FinalAlbion.qst
        004A0D90(ecx=world, path, 1)
          004A08D0  clear +184 / +172 / +196
          tokenizer 009BA4F0
            AddQuest     → +184; TRUE → +172; 004B2850 manager+44
            AddTestQuest → +196 only (no 004B2850 / 004B4A10)
      0x01238F38  Data\Levels\GlobalQuests.qst
      00999230  exists
        004A0D90(ecx=world, path, 0)         // no 004A08D0
          same tokens; append
      004FDAB0  empty 0x122D70C
      Startup WAD
      0049E220 → 00507C30                    // no START_INITIAL_QUESTS case
      Set Static Map 00B23DC0 → 00B428E0
    [0x13B8648]==0
      0049F180
        0049F247  lea edx, [esi+172]
        0049F24E  004B4260([world+172])      // QST TRUE names
        0049F259  004B2890
      00416BCF  "Activate Initial Quests"
        +90584 vs 0x122D70E → 004B4A10       // empty string, not Oakvale
```

`00DBDE40` / `S_QNOVI` are **not** on this list. **PROVEN**.

---

## 1. When is the first QST open?

| Claim | Class | Evidence |
|---|---|---|
| First open is after Leave, inside `004A1840` | **PROVEN** | `00416ABA` is the no-save site; string `"Load Quests"` at `004A18DD` |
| QST before Startup WAD / `00507C30` | **PROVEN** | listing order; `LoadWorld_004A1840_after_wad_*` event order |
| Host WLD-before-WAD | **DISPROVEN** | same test |
| Frontend / Leave itself reads `.qst` | **DISPROVEN** | zero E8 to `004A0D90` on `0042F2A2` |
| Path is `+90576` `FinalAlbion.wld` stem, not `updatedscenic.wld` | **PROVEN** | Leave `0042F44D` / ctor `00415E17` |
| `0049D770` = `Data\Levels\` + stem + `.qst` (`0x01238C40`) | **PROVEN** | `0049D794` push suffix; host `DeriveQuestFileName` |
| Global path is intern `0x01238F38`, not derived from the WLD stem | **PROVEN** | `004A1965` |
| Missing file skips parse (`00999230` / `je`) | **PROVEN** | TLC ships both files |
| Second `004A1840` site `004A2A01` on this no-save walk | **UNREAD** | not `00416ABA` |

Host `GameInstall.QuestPath` / `GlobalQuestPath` match those two
paths. **PROVEN**.

---

## 2. Native `004A0D90` vs `QuestFile.cs`

`004A0D90` is a token walk (`009BA4F0` / `009B9C60` / `009BA330`),
not a regex. Empty intern `0x122D70E` uses `rep cmpsb`; else
`004115A0`.

| Token | Native | `QuestFile.Parse` |
|---|---|---|
| `AddQuest("Name", TRUE\|FALSE)` | store name at `world+184`; `00BFEBA8("TRUE")` → also `world+172`; then `004B2850` (`[0x13B89FC]+44` push) | name + `Persistent` bool only |
| `AddTestQuest("Name", "HSP", …)` | `004A113B` → `world+196` only | **dropped** (no regex) |
| other tokens | `004A17B5` continue / `004A17FF` EOF | ignored |
| flag arg `[esp+8]` | `1` → `004A08D0` clear `+184/+172/+196` first; `0` append | no clear; one file |

`004B2850` is **not** activate. It is `push_back` of the name onto
the quest-manager vector. **PROVEN** (`mov eax,[ecx+52]` /
`00433530`). Activation is later `004B4260`.

Shipped `FinalAlbion.qst` (TLC):

| Kind | Count | Host sees |
|---|---:|---|
| `AddQuest` | 187 | **yes** (`quests-qst.md` / `QuestFile.Load`) |
| `AddQuest` TRUE | **8** | bool only; not used as activate list |
| `AddTestQuest` | **112** | **no** |
| `AddQuest("Q_NewOakValeIntro", FALSE)` | 1 | present, `Persistent=false` |
| `AddTestQuest("Q_NewOakValeIntro","NOVStartHSP",…)` | 1 | **dropped** |

Shipped `GlobalQuests.qst`: **14** `AddQuest`, **1** TRUE
(`Global_WatchForHeroDeath`). No `AddTestQuest`.

`QuestFile` regex is enough for the shipped `AddQuest` lines
(quotes + `TRUE`/`FALSE`). Tabs / extra spaces match. **PROVEN**
as a name table. **MISMATCH** as a `004A0D90` stand-in (no
`AddTestQuest`, no `world+*` stores, no flag-1 clear).

---

## 3. What `world+172` contains after both files

`004A08D0` (flag 1, FinalAlbion only) resets the three vectors.
Then TRUE `AddQuest` push_back order:

**FinalAlbion.qst TRUE**

1. `Q_SunnyvaleMaster`
2. `ChapterAndSceneManager`
3. `PersonalScriptMain`
4. `PersonalScript_GlobalThings`
5. `NPCDeath`
6. `HeroBoasts`
7. `V_HeroDolls`
8. `CS_PlayCutscene`

**GlobalQuests.qst TRUE (append)**

9. `Global_WatchForHeroDeath`

WLD file head (host `WorldFile.InitialQuests`) is a **subset** of
that list, minus `ChapterAndSceneManager` / `NPCDeath` / the
global, and it is **not parsed** by `00507C30`. **PROVEN**.

`Gameflow` is `AddQuest(..., FALSE)` so it is **not** in `+172`.
It starts later from `user.ini`. **PROVEN**.

`Q_NewOakValeIntro` is FALSE + `AddTestQuest` → `+196` only.
**PROVEN** not in `+172`.

---

## 4. First *use* of that table (`004B4260`)

Not during `004A0D90`. First walk is `0049F24E` after Set Static
Map / `0049F180` Init Characters / Init GUI:

`004B4260` → `"QuestManager: Activate Quest"` → `004B00C0` gate →
`00CB5AD0` (`[manager+120]`) → `004BB720` / `004B3CE0`.

| # | `world+172` name | `00CD52D0` row | Host `ActivateNamedQuest` |
|--:|---|---|---|
| 1 | `Q_SunnyvaleMaster` | `00CDD550` | yes (WLD[0]) |
| 2 | `ChapterAndSceneManager` | **none** (no PE string) | **no** |
| 3 | `PersonalScriptMain` | `00CDE2F0` / `S_PSM` | yes |
| 4 | `PersonalScript_GlobalThings` | `00CE19A0` / `S_PSGT` | yes |
| 5 | `NPCDeath` | **none** | **no** |
| 6 | `HeroBoasts` | `00CE6C40` / `S_HB` | yes |
| 7 | `V_HeroDolls` | `00E98640` / `S_VHDS` | yes |
| 8 | `CS_PlayCutscene` | `00F01760` | yes |
| 9 | `Global_WatchForHeroDeath` | `00EE90A0` / empty script `0x122D70E` (`00CD9A12`) | **no** |
| — | `Gameflow` | `00CEF950` | yes, from **ini**, not QST |

`00CB5AD0` miss on `ChapterAndSceneManager` / `NPCDeath` still
hits `004BB720` with factory `0`. Whether that allocates a fiber
is **UNREAD**. Do not invent a second registrar.

`Global_WatchForHeroDeath` **does** have a factory. Host never
`00CB5AD0`s it after Leave. **DIVERGE**.

`Init_quests_004B4260_activates_wld_initial_list` locks the six
WLD names + `Gameflow`. Names overlap authored TRUE quests;
attributing the list to `00507C30` is **DISPROVEN**. Omitting
row 9 is **DIVERGE**.

---

## 5. C# vs native

| Host | Native after Leave | Class |
|---|---|---|
| `LoadQuestDefs` before WAD / WLD | `004A1840` child order | **PROVEN** |
| `DeriveQuestFileName` → `Data\Levels\FinalAlbion.qst` | `0049D770` | **PROVEN** |
| `QuestFile.Load(QuestPath)` | `004A0D90` flag 1 `AddQuest` names | **PARTIAL** (no `AddTestQuest` / no `004A08D0`) |
| `Note` GlobalQuests exists; do not merge | `004A0D90` flag 0 append | **DIVERGE** |
| `life.Quests` = FinalAlbion `AddQuest` only | `+184` = both files | **DIVERGE** |
| `InitCharactersAndQuests` uses `World.InitialQuests` | `004B4260([world+172])` QST TRUE | **DIVERGE** (6 vs 9; wrong writer) |
| `ActivateQuest(name, persistent)` from `life.Quests` | persist bit from that `AddQuest` | **PROVEN** helper for names it does start |
| `004A113B` Note only | `+196` HSP / card / `.end` / `.ini` | **LEFTOVER** vs first load |
| `RegionTravel` `NOVStartHSP` from `AddTestQuest` | store only; not activate | **PROVEN** store; **LEFTOVER** as New Game start |
| `NewGameScript` / `StartNewGame` / Oakvale | unused on this walk | **LEFTOVER** / **DIVERGE** |

`QuestFile` is the right *format* reader for `AddQuest`.
It is **not** the first-load *runtime* table. First-load table is
the TRUE slice of **both** `.qst` files, in file order, after
`004A08D0`.

---

## Classifications (short)

1. **First QST load after Leave — PROVEN:** `00416ABA` →
   `004A1840` → `0049D770` `FinalAlbion.qst` (`004A0D90` flag 1) →
   `GlobalQuests.qst` (`004A0D90` flag 0) → then WAD / WLD.
2. **`world+172` from QST `AddQuest` TRUE, not WLD — PROVEN.**
   Nine names: eight FinalAlbion + `Global_WatchForHeroDeath`.
3. **`QuestFile.cs` as `004A0D90` — PARTIAL / MISMATCH.**
   `AddQuest` names+bool only. Drops 112 `AddTestQuest` rows.
4. **Host first-load activate list — DIVERGE.**
   Uses WLD `START_INITIAL_QUESTS` (6) and never merges
   GlobalQuests. Native would `00CB5AD0` `Global_WatchForHeroDeath`
   (`00EE90A0`). `ChapterAndSceneManager` / `NPCDeath` are QST-only
   misses.
5. **`Q_NewOakValeIntro` / `00DBDE40` on this load — DISPROVEN.**
   FALSE + `AddTestQuest` → `+196`. Not `+172`. Not `004B4260`.
