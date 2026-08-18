# Audit: `NewGameScript.cs` / `QuestFactoryTable.cs` / `ScriptFactoryTable.cs` vs dump

Investigation only. No production `src/` edits.

Do **not** start at `00DBDE40` / `StartOakVale` / `S_QNOVI` /
`NOVI_LiveFather`. That is later `Q_NewOakValeIntro` slot 2
(`00DABAC0` → `00DBDE40`), not Leave / first `004B4260`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: `src/Fable.Game/NewGameScript.cs`, `QuestFactoryTable.cs`,
`ScriptFactoryTable.cs`, `ScriptRuntime.StartNewGame` /
`InstallRecoveredBindings`, `EngineLifecycle.ActivateNamedQuest`;
ExeIndex `listing-00cc0000.txt` (`00CD52D0`–`00CDB35C`, `00CE6CF0`),
`listing-00d80000.txt` (`00DABAC0`), `listing-00480000.txt`
(`004A0D90` / `004A10C4`);
`calls-script-bind-00cb5c90` (161 `E8`);
`out/01-sections/script-bank/quests-qst.md`;
TLC `FinalAlbion.wld` `START_INITIAL_QUESTS` head;
`proofs/newgame-script`, `script-factory-tables`, `qst-first-quest`,
`qst-first-load`, `wld-parse`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Invented quest list? | **Three different lists. None is the native 161-row fill.** Names that exist in the three C# tables are dump-backed. Completeness / pairing as “the New Game list” is invented. | see §1 |
| Wrong first quest? | **`QuestFactoryTable.Recovered[0]` is the right first name (`Q_SunnyvaleMaster`). `NewGameScript` / `ScriptFactoryTable` / `StartNewGame` use the wrong first quest (`S_QNOVI` / `NOVI_LiveFather`).** | see §2 |

---

## Verdict

`QuestFactoryTable` is the New Game table (`00CD52D0` via
`00CB5C90`). Dump first push is **`Q_SunnyvaleMaster`** /
factory `00CDD550` / run `00CDBD20` / persist **1**.
`Recovered` keeps **7 of 161** bind rows. Those seven names
are **PROVEN** dump strings. Treating the array as the native
table, or iterating it as register / activate order, is
**DIVERGE**.

`NewGameScript` and `ScriptFactoryTable` are **not** that
table. They observe later `S_QNOVI` / `00DABAC0`. Leave never
constructs `Q_NewOakValeIntro`. **DISPROVEN** as first New Game
quest.

`GameflowStateNames` is **not** invented: 54 strings match
`00CE6CF0` `push` order `OV_INTRO` … `SNOWSPIRE_ARRIVAL`.

---

## Dump vs C# (what each file is)

Two native tables. Do not collapse them.

| Table | Fill | Lookup | Record |
|---|---|---|---|
| Quest factory | `00CD52D0` via `00CB5C90` | `00CB5AD0` `[manager+120]` | quest name + factory + run + persist |
| TNG / script name | `00CB8230` (generic) | `00CB8960` / `004C97B0` | name + factory at `+16` |

`00CB5C90` sites in `00CD52D0`–`00CDB31A`: **161**
(`calls-script-bind-00cb5c90`, last `0x00CDB31A`
`CS_PlayCutscene`). **PROVEN**.

---

## 1. Invented quest list?

### 1a. `QuestFactoryTable.Recovered` — subset, not invented names

Dump fill head (`listing-00cc0000.txt`):

| # | `push` | Script | `[esp+32]` factory | Persist |
|--:|---|---|---|---|
| 1 | `00CD5307` `Q_SunnyvaleMaster` | empty `0x122D70E` | `00CDD550` | `[esp+48]=1` |
| 2 | `00CD53B1` `HeroBoasts` | `S_HB` | `00CE6C40` | `bl` |
| 3 | `00CD544A` `PersonalScriptMain` | `S_PSM` | `00CDE2F0` | `bl` |
| 4 | `00CD54E3` `PersonalScript_GlobalThings` | `S_PSGT` | `00CE19A0` | `bl` |
| 5 | `00CD557C` `Gameflow` | `S_GF` | `00CEF950` | `bl` |
| 6 | `00CD5615` `GameflowAssistance` | `S_GFA` | `00CF0640` | `bl` |
| … | `00CD6E27` `Q_NewOakValeIntro` | `S_QNOVI` | `00DBEF70` | `bl` |
| late | `00CD8B9A` `V_HeroDolls` | `S_VHDS` | `00E98640` | `bl` |
| last | `00CDB2D4` `CS_PlayCutscene` | empty `0x122D70E` | `00F01760` | `bl` |

`ebp = 00CDBD20` on every early row (`00CD532D`). **PROVEN**.

C# `Recovered` (7):

```
Q_SunnyvaleMaster, HeroBoasts, PersonalScriptMain,
PersonalScript_GlobalThings, V_HeroDolls, CS_PlayCutscene, Gameflow
```

| Claim | Class |
|---|---|
| Those seven names + factories + `S_*` + empty PlayCutscene | **PROVEN** vs listing |
| Sunnyvale persist 1, others 0 | **PROVEN** |
| SharedRun `00CDBD20` | **PROVEN** (`ebp`) |
| Omitting `GameflowAssistance` from *activate* subset | **PROVEN** omit (fill row 6; not WLD / not `user.ini`) |
| `Recovered` length 7 = native table | **DISPROVEN** (161) |
| `Recovered` order = fill order | **LEFTOVER** (HeroBoasts before Personal is fill; Dolls/Play/Gameflow are not consecutive in fill) |
| `Recovered` order = first `004B4260` walk | **LEFTOVER** (WLD / QST TRUE order is Personal* then HeroBoasts) |
| `Find` by name for Leave constructs | **PROVEN** pairing |

`GameflowStateNames` (54): dump `00CE6D06`–`00CE7583` is the
same sequence, then `00CE75D0` `push "Main"` (watcher name, not
a state). **PROVEN** 1:1. Not a quest list.

### 1b. Native first *construct* list is not `Recovered` either

`004A10B2` `test bl` then `lea esi, [ebp+172]`
(`listing-00480000.txt` `004A10C4`): `AddQuest` TRUE →
`CWorld+172`. `00507C30` has **no** `START_INITIAL_QUESTS`
case (`wld-parse`).

QST TRUE after both files (`quests-qst.md` + `GlobalQuests.qst`):

1. `Q_SunnyvaleMaster`
2. `ChapterAndSceneManager` (no `00CD52D0` PE string)
3. `PersonalScriptMain`
4. `PersonalScript_GlobalThings`
5. `NPCDeath` (no PE string)
6. `HeroBoasts`
7. `V_HeroDolls`
8. `CS_PlayCutscene`
9. `Global_WatchForHeroDeath` (`00CD9A1C` factory `00EE90A0`,
   empty script, run **`ebx`** not `ebp`)

WLD file head (host `WorldFile.InitialQuests`, unused by
`00507C30`):

```
Q_SunnyvaleMaster, PersonalScriptMain, PersonalScript_GlobalThings,
HeroBoasts, V_HeroDolls, CS_PlayCutscene
```

Then `user.ini` `ActivateQuest("Gameflow")` (7th host activate).

| List | Length | Invented? |
|---|---:|---|
| Native fill `00CD52D0` | 161 | no |
| Native `world+172` QST TRUE | 9 | no |
| Host `ActivatedQuests` (WLD six + Gameflow) | 7 | **DIVERGE** vs 9 (drops 2, 5, 9) |
| `QuestFactoryTable.Recovered` | 7 | dump names; **invented completeness / order** |
| `ScriptFactoryTable.Recovered` | 1 | dump name; **invented as New Game list** |
| `NewGameScript` | 0 quests | leftover façade, not a list |

Host `EngineLifecycle` walks `World.InitialQuests` (WLD six),
not `Recovered` and not QST TRUE. First *name* still matches.
Missing `Global_WatchForHeroDeath` `00CB5AD0` is **DIVERGE**.

### 1c. `ScriptFactoryTable.Recovered` — leftover Oakvale slice

Dump `00DABAC0` (`listing-00d80000.txt`) registers **15** names
via `00CB8230`, factory at `+16`:

| # | Name | `+16` |
|--:|---|---|
| 1 | `NOVI_LiveFather` | `00DAC2C0` (`00DABB0C`) |
| 2 | `NOVI_Theresa` | `00DAC420` |
| 3–15 | Guard, Villager, Bully, Victim, TeddyGirl, Affair*, BookTrader, Barrel*, CreatedBeetle | per-row |

C# keeps **row 1 only** plus cutscene `CS_OAKVALE_INTRO_FATHER`.
Accurate as later slot-2 row 1. **LEFTOVER** vs Leave (0
`00CB8230` on first `004B4260`). **DIVERGE** if
`StartNewGame` / `InstallRecoveredBindings` is treated as New
Game.

### 1d. `NewGameScript` has no quest list

Constants are generic VM / fiber VAs plus leftover
`AttackOver` / `NOVI_LiveFather`. Comment says first-seen
`S_QNOVI`. That pairing is **LEFTOVER** vs Leave.

---

## 2. Wrong first quest?

### Native first name after Leave — **PROVEN: `Q_SunnyvaleMaster`**

Same string in four dump-backed places:

| Site | Evidence |
|---|---|
| `00CD52D0` first `00CB5C90` | `00CD5307` push `Q_SunnyvaleMaster` |
| `FinalAlbion.qst` first `AddQuest` | TRUE (`quests-qst.md`) |
| WLD `START_INITIAL_QUESTS` line 1 | same (file head; **not** `00507C30`) |
| First `004B4260` / `00CB5AD0` | `world+172[0]` |

`Q_NewOakValeIntro` is fill-only (`00CD6E27` / `S_QNOVI` /
`00DBEF70`, persist `bl`). QST **FALSE** + `AddTestQuest` →
`world+196`. Not in `+172`. **DISPROVEN** as first.

### C# first quest

| Host | First name it implies | vs dump New Game | Class |
|---|---|---|---|
| `QuestFactoryTable.Recovered[0]` | `Q_SunnyvaleMaster` | match | **PROVEN** |
| `EngineLifecycle` `World.InitialQuests[0]` | `Q_SunnyvaleMaster` | match (wrong writer: WLD vs QST) | **PARTIAL** |
| `NewGameScript` façade | `S_QNOVI` / `AttackOver` / `NOVI_LiveFather` | wrong | **DISPROVEN** |
| `ScriptFactoryTable.Recovered[0]` | `NOVI_LiveFather` | wrong for Leave; right for later `00DABAC0` | **LEFTOVER** |
| `ScriptRuntime.StartNewGame` | `InstallRecoveredBindings` → `S_QNOVI` fiber | invented activate | **DIVERGE** |
| `ScriptFiberTable.Recovered[0]` | `S_QNOVI` + `AttackOver` | first persist is Sunnyvale `00CDC070` | **DISPROVEN** pairing |

`00DABAC0` has **0** `E8` callers (`calls-s-qnovi-run`). It is
a vtbl+8. Only runs if `Q_NewOakValeIntro` is constructed.
Leave does not. **PROVEN**.

---

## 3. Bind vs start (do not mix)

| Event | Starts a quest body? | Class |
|---|---|---|
| `00CD52D0` `00CB5C90` 161 rows | no | **PROVEN** bind |
| `00CD6E27` Oakvale row | no | **PROVEN** bind |
| `004A0D90` TRUE → `+172` | no | **PROVEN** store |
| `004B4260` / `00CB5AD0` | factory + `00A447D0` fiber | **PROVEN** first use |
| `user.ini` `Gameflow` | 7th construct (`00CE75B0` Main, not `S_GF`) | **PROVEN** |
| `00DABAC0` / `00DAC2C0` | later, if Oakvale constructed | **DISPROVEN** first-seen |
| `ScriptRuntime.StartNewGame` | host yes | **DIVERGE** |

`NewGameScript.BindFactory` and `QuestFactoryTable.Bind` are the
same `00CB5C90`. Different tables. First-seen after Leave is the
**quest** table.

---

## Classifications (short)

1. **Invented quest list — PARTIAL.**
   `QuestFactoryTable.Recovered` names/factories **MATCH** dump.
   The list as native table (7 vs 161) and as activate order is
   **invented**. `GameflowStateNames` is **not** invented.
   `ScriptFactoryTable` / `StartNewGame` invent an Oakvale TNG
   list as New Game.

2. **First quest — PROVEN `Q_SunnyvaleMaster`.**
   Dump fill, QST TRUE, WLD head, first `00CB5AD0`.
   `QuestFactoryTable.Recovered[0]` is correct.

3. **`NewGameScript` first quest `S_QNOVI` — DISPROVEN.**
   Bind-only on this walk. Façade is leftover Oakvale VM notes.

4. **Host activate list (WLD six + Gameflow) — DIVERGE vs
   `world+172` nine.** First name still Sunnyvale. Missing
   `Global_WatchForHeroDeath` (`00EE90A0`). Do not invent
   `ActivateQuest(Q_NewOakValeIntro)`.
