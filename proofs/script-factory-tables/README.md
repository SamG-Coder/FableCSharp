# ScriptFactoryTable.cs / QuestFactoryTable.cs vs native factory tables

Investigation only. No production `src/` edits.

Do **not** start at `NOVI_LiveFather` / `00DABAC0` / `00DB86B0` /
`CS_OAKVALE_INTRO_FATHER`. That is later `Q_NewOakValeIntro` slot 2,
not Leave / Init World / first `004B4260`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Sources: `src/Fable.Game/ScriptFactoryTable.cs`, `QuestFactoryTable.cs`,
`EngineLifecycle.ActivateNamedQuest`, `ScriptRuntime.InstallRecoveredBindings`;
`docs/runtime/FORWARD_TREE.md` §§6–7, 10–11; `docs/PARITY.md`;
`EngineLifecycleTests` (`Init_quests_004B4260_activates_wld_initial_list`,
`Activate_quests_00CB5AD0_starts_factory_scripts`,
`Gameflow_00CE75B0_is_Main_watcher_not_S_GF`);
ExeIndex `listing-00cc0000.txt` `00CD52D0`–`00CDB35C`,
`calls-script-bind-00cb5c90` (161 `E8`),
`registering-scripts-00cb5d80`, `s-qnovi-slot2-run-00dabac0`.

Two native tables. Do not collapse them.

| Table | Fill | Lookup | Record |
|---|---|---|---|
| Quest factory | `00CD52D0` via `00CB5C90` | `00CB5AD0` `[manager+120]` | quest name + factory + shared run + persist flag |
| TNG / script name | `00CB8230` (generic) | `00CB8960` / `004C97B0` | script name + factory at `+16` |

`00CB5AC0` is a third helper: `00CD52D0` pushes `S_HB` / `S_PSM` / …
into the script-def map **before** each `00CB5C90`. It is **not**
`ScriptFactoryTable`.

---

## Timeline (no-save New Game, after Leave)

```
0042F2A2 Leave frontend
0042F491 Init Game 004184BD
  Init World 004A6E30
    004A6550 / 004A6638  Init Scripts 006E7740 → world+56
    004A6661  00CB5C70  empty record ctor
    004A6677  00CB5D80  "Registering Scripts"
      00F2A0F0  Registering Script Defs
      00CD52D0  Registering Master + Important Scripts   // TABLE FILL
        161× 00CB5C90  (Q_SunnyvaleMaster … CS_PlayCutscene)
        includes Q_NewOakValeIntro / S_QNOVI / 00DBEF70   // BIND ONLY
    004A66AD  "Engine Set World"
  00416953 Load world FinalAlbion.wld
    00507C30 START_INITIAL_QUESTS → world+172
    0049F180 Init Characters / Init GUI
    004B4260([world+172])                                // FIRST CONSTRUCT
      00CB5AD0 lookup
      004BB720 / 004B3CE0 factory + 00CDBD20 run
      00A447D0 fiber
  user.ini 009EC890
    ActivateQuest("Gameflow")
      00419CE0 [world+56] vtbl+1104 00892E80
      004B4A10 → 004B4260 → 00CB5AD0 "Gameflow"          // 7th CONSTRUCT
004189C2 first pumps
  00CB8220 / 00A44880 / 00CE7670 yield on Q_NewOakValeIntro
```

`00DABAC0` / `00CB8230(NOVI_*)` / `00DAC2C0` are **not** on this
list. **PROVEN**.

---

## 1. Native `00CD52D0` (quest factory table)

One function `00CD52D0`–`00CDB35C` (`listing-00cc0000.txt`).
Caller is only `00CB5E12` inside `00CB5D80`, itself only from
`004A6677` (Init World after Leave). **PROVEN**.

Bind record written at each `00CB5C90`:

| Stack slot | Field | First-seen Sunnyvale | Later rows |
|---|---|---|---|
| `[esp+32]` | factory | `00CDD550` | per-quest |
| `[esp+36]` | run | `ebp = 00CDBD20` | same SharedRun |
| `[esp+44]` | `edi = 1` | always 1 | always 1 |
| `[esp+48]` | persist | `1` | `bl = 0` |
| CString | quest name | `Q_SunnyvaleMaster` | … |
| CString | script name | empty `0x122D70E` | `S_HB` / `S_PSM` / … or empty |

`00CB5C90` sites in this function: **161**. That is the native
table length. **PROVEN** (`calls-script-bind-00cb5c90`).

Register order of the first rows (listing, not WLD order):

| # | Quest | Script | Factory | Persist |
|--:|---|---|---|---|
| 1 | `Q_SunnyvaleMaster` | empty | `00CDD550` | 1 |
| 2 | `HeroBoasts` | `S_HB` | `00CE6C40` | 0 |
| 3 | `PersonalScriptMain` | `S_PSM` | `00CDE2F0` | 0 |
| 4 | `PersonalScript_GlobalThings` | `S_PSGT` | `00CE19A0` | 0 |
| 5 | `Gameflow` | `S_GF` | `00CEF950` | 0 |
| 6 | `GameflowAssistance` | `S_GFA` | `00CF0640` | 0 |
| … | `Q_ArenaHoldingScript` … `Q_NewOakValeIntro`/`S_QNOVI`/`00DBEF70` … | | | 0 |
| late | `V_HeroDolls` | `S_VHDS` | `00E98640` | 0 |
| last | `CS_PlayCutscene` | empty `0x122D70E` | `00F01760` | 0 |

`QST` names `ChapterAndSceneManager` / `NPCDeath` have **no** PE
string and **no** `00CD52D0` row. They are QST-only. **PROVEN**
absence from this table. Another registrar is **UNREAD**.

---

## 2. First-seen *constructs* after Leave (not the fill)

Fill happens at Init World. First *use* is `004B4260` /
`00CB5AD0` on `world+172` then `user.ini`.

WLD `START_INITIAL_QUESTS` order (host `WorldFile.InitialQuests`,
locked by `Init_quests_004B4260_*`):

| Order | Quest | In `QuestFactoryTable.Recovered`? | `CCutsceneDef` started? |
|--:|---|---|---|
| 1 | `Q_SunnyvaleMaster` | yes | no (`ScriptName==null`; persist `00CDC070`) |
| 2 | `PersonalScriptMain` | yes | **DISPROVEN** (`HasStarted("S_PSM")==false`) |
| 3 | `PersonalScript_GlobalThings` | yes | no |
| 4 | `HeroBoasts` | yes | **DISPROVEN** (`HasStarted("S_HB")==false`) |
| 5 | `V_HeroDolls` | yes | no |
| 6 | `CS_PlayCutscene` | yes | **DISPROVEN** (`ScriptName==null`) |
| 7 | `Gameflow` (ini, not WLD) | yes | **DISPROVEN** (`HasStarted("S_GF")==false`; Main is `00CE75B0`) |

Seven fibers via `00A447D0`. Shared run: Sunnyvale allocates
`00CDBD20` size `0x144` vtbl `012C2748`; the other six
`004AFA10` reuse that object. **PROVEN**.

`Q_NewOakValeIntro` is in the 161-row fill and in QST /
`AddTestQuest` store. It is **not** in `world+172` and **not**
constructed here. **PROVEN**.

---

## 3. `QuestFactoryTable.cs` vs native

`Recovered` has **7** of **161** native rows. The seven are
exactly the first-seen constructs after Leave. **PROVEN** as a
first-seen subset. **DISPROVEN** as the full native table.

Per-row fields for those seven **MATCH** the listing
(`factory` / `00CDBD20` / persist 1 only on Sunnyvale /
script names / empty `CS_PlayCutscene`).

| Host choice | Native | Class |
|---|---|---|
| `Register = 00CD52D0` | fill, not lookup | **PROVEN** |
| `Bind = 00CB5C90` | record store | **PROVEN** |
| `Lookup = 00CB5AD0` | construct path | **PROVEN** |
| `SharedRun = 00CDBD20` | `ebp` on every row | **PROVEN** |
| `GameflowAssistance` omitted | 6th fill row; not activated | **PROVEN** omit |
| `Recovered` order Sunnyvale, HeroBoasts, Personal*, Dolls, Play, Gameflow | neither fill order nor WLD order | **LEFTOVER** order (`Find` is by name; activate uses WLD + ini) |
| `GameflowScript = "S_GF"` | bind string only | **PROVEN** name; **DISPROVEN** as first runner |
| `HeroBoastsMain` / `GameflowMain` / `SunnyvaleInit` | factory vtbls, not the `00CB5C90` record | **PROVEN** elsewhere; not table columns |
| QST persist vs table persist | Sunnyvale QST True + table flag 1; HeroBoasts QST True + table flag 0 | **PARTIAL** (two persist meanings) |

Host `EngineLifecycle.ActivateNamedQuest` looks up by WLD / ini
name. That pairing is **PROVEN** vs Leave. Iterating `Recovered`
as “register order” would **DIVERGE**.

---

## 4. Native `00CB8230` (script name table)

`00CB8230` allocates a 12-byte name record (`+4 = 00CB8D30`,
`+8 = name`) and inserts at `this+24` via `00CBD310`. Generic.
**PROVEN**.

`00DABAC0` (`S_QNOVI` vtbl+8) is the recovered *example*:

| Order | Name | Factory at `+16` |
|--:|---|---|
| 1 | `NOVI_LiveFather` | `00DAC2C0` |
| 2 | `NOVI_Theresa` | `00DAC420` |
| 3 | `NOVI_Guard` | … |
| 4–15 | `NOVI_Villager`, `Bully`, `Victim`, `TeddyGirl`, `AffairMan/Woman/Wife`, `BookTrader`, `BarrelMan/Thug/Barrel`, `CreatedBeetle` | per-row |

Same helper is also called from many later quests (`00E46D92`,
`00E47AA6`, …). **PROVEN** generic. First `E8` after Leave is
**UNREAD** (not `00CD52D0`; that path is `00CB5C90`).

`00DABAC0` itself runs only if `Q_NewOakValeIntro` is
constructed. Leave does not. **PROVEN**.

---

## 5. `ScriptFactoryTable.cs` vs native

| Host | Native after Leave | Class |
|---|---|---|
| `NameRegister = 00CB8230` | generic insert | **PROVEN** helper |
| `SqnoviRun = 00DABAC0` | Oakvale slot 2 | **LEFTOVER** vs first-seen |
| `Recovered[0] = NOVI_LiveFather` / `00DAC2C0` / `CS_OAKVALE_INTRO_FATHER` | 1 of ≥15 `00DABAC0` names; 0 of first-seen | **LEFTOVER** |
| `ConstructBind = 00CB8960` / `ThingConstruct = 004C97B0` | TNG `ScriptName` path | **PROVEN** later; **DISPROVEN** first no-save (Lookout TNG, no father) |
| `Find` used from `StartNewGame` | Leave uses `QuestFactoryTable` only | **DIVERGE** |

`ScriptRuntime.InstallRecoveredBindings` registers
`NOVI_LiveFather` and creates an `S_QNOVI` / `AttackOver` fiber.
`EngineLifecycle` New Game does **not** call it.
`StartNewGame` does. **DIVERGE** as live New Game; **PROVEN** as
Oakvale-intro notes.

First persist after Leave is Sunnyvale `00CDC070` (38 slots),
not `AttackOver`. See `PersistTable.Sunnyvale`. **PROVEN**.

---

## 6. Bind vs start (do not mix)

| Event | Table | Starts a cutscene? | Class |
|---|---|---|---|
| `00CD52D0` `00CB5C90` | quest factory fill | no | **PROVEN** bind |
| `00CD6E27` `Q_NewOakValeIntro`/`S_QNOVI`/`00DBEF70` | one fill row | no | **PROVEN** bind (inside `00CD52D0`, **not** `004A1840`) |
| `004B4260` / `00CB5AD0` | lookup + construct | factory object + fiber; no `00CBFB7D` | **PROVEN** |
| `00CB5AC0` `S_*` | script-def map | no | **PROVEN** name bind |
| `00DABAC0` `00CB8230` | TNG name table | later, if Oakvale runs | **DISPROVEN** first-seen |
| `00DAC2C0` → `00DB86B0` | father factory | `CS_OAKVALE_INTRO_FATHER` | **DISPROVEN** first-seen |

`NewGameScript.BindFactory` and `QuestFactoryTable.Bind` are the
same `00CB5C90`. Different tables. First-seen after Leave is the
**quest** table.

---

## Classifications (short)

1. **Two tables — PROVEN.** Quest fill `00CD52D0`/`00CB5C90` (161).
   TNG names `00CB8230` (generic; `00DABAC0` is Oakvale).
2. **First-seen after Leave — quest table fill then 7 constructs.
   PROVEN.** Fill at Init World `004A6677`. Constructs: WLD six
   then `user.ini` `Gameflow`. No `00CB8230`. No Oakvale factory.
3. **`QuestFactoryTable.Recovered` (7) — PROVEN first-seen subset,
   DISPROVEN as the native table.** Field values MATCH. Order is
   **LEFTOVER**. `GameflowAssistance` omit is correct.
4. **`ScriptFactoryTable.Recovered` (`NOVI_LiveFather`) — LEFTOVER
   vs Leave.** Accurate as later `S_QNOVI` slot-2 row 1 of ~15.
   `StartNewGame` / `InstallRecoveredBindings` **DIVERGE** from
   the no-save click path.
5. **Who later constructs `Q_NewOakValeIntro` — UNREAD.** Bind is
   already done at Init World. Do not invent `ActivateQuest` for it.
