# S_GF / Gameflow after Leave — wait on `Q_NewOakValeIntro`

Investigation only. No production `src/` edits.

Do **not** start at `00DBDE40` / `S_QNOVI` / `CS_OAKVALE_INTRO_FATHER`.
That is later leftover `Q_NewOakValeIntro` slot 2
(`00DABAC0` → `00DBDE40`). Leave is `0042F2A2`. First no-save
type-1 does **not** start that quest.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: `src/Fable.Game/QuestFactoryTable.cs` (`Gameflow` / `S_GF` /
`00CEF950` / `00CE75B0` / `00CE6CF0`);
`EngineLifecycle` (`ActivateNamedQuest` / `SeedGameflowStates` /
`TickGameflowMain` / `ResumeGameflowWait`);
`ScriptRuntime.StartNewGame` / `InstallRecoveredBindings` /
`ActivateQuest`; `FirstSceneWorld`;
`proofs/ini-activate-quest/README.md`,
`proofs/newgame-script/README.md`,
`proofs/fiber-yield-first/README.md`,
`proofs/script-factory-tables/README.md`;
`docs/PARITY.md` Init Game / type-1 / who-activates rows;
`docs/runtime/FORWARD_TREE.md` §§6–11;
`EngineLifecycleTests`
(`Gameflow_00CE75B0_is_Main_watcher_not_S_GF`,
`Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`,
`Type1_resume_00CB8220_is_00A44880_then_00893610_yield`,
`No_save_does_not_activate_Q_NewOakValeIntro`,
`Init_quests_004B4260_activates_wld_initial_list`);
`ScriptRuntimeParityTests.StartNewGame_uses_recovered_factory_table_not_oakvale_literals`.

---

## Verdict

**After Leave, Gameflow is a factory Main watcher. It is not
`S_GF` as a `CCutsceneDef`.**

`00CD52D0` binds quest `"Gameflow"` to script name `"S_GF"`
and factory `00CEF950`. `user.ini` `ActivateQuest("Gameflow")`
constructs that factory (`00CE6CF0` seed + `00CE75B0` Main).
`HasStarted("S_GF")==false` at construct and after the first
type-1.

First type-1 `00CB8220` reaches Gameflow last (tail-insert).
`00CE7670` state 0 looks up `Q_NewOakValeIntro` (`00893610`)
and **yields** (`006E7410` / `00A44840` / `009D8650`). It
does **not** `ActivateQuest` that name. Later resumes take
the same miss.

**Yes: C# `ScriptRuntime.StartNewGame` invents Oakvale as a
New Game path.** The method body has no Oakvale string
literals. Callers pass `StartOakValeWest` TNG and
`InstallRecoveredBindings` installs leftover `NOVI_LiveFather`
/ `S_QNOVI` / `AttackOver`. Live Leave uses
`ScriptRuntime.Detached()` + WLD six + Gameflow only.
`EngineLifecycle` never calls `StartNewGame`.

| Question | Answer | Class |
|---|---|---|
| What starts Gameflow after Leave? | TLC `user.ini` `ActivateQuest("Gameflow")` via `00419CE0` | **PROVEN** |
| Is that `S_GF` opcode / `CCutsceneDef`? | no; Main is `00CE75B0`; `HasStarted("S_GF")==false` | **DISPROVEN** |
| First Gameflow body after Leave? | first type-1 `00CE7670` state 0 | **PROVEN** |
| Does that body start Oakvale? | `00893610("Q_NewOakValeIntro")=0` → yield | **PROVEN** wait |
| Who activates `Q_NewOakValeIntro` on no-save? | not this walk | **UNREAD** |
| Does Leave call `StartNewGame`? | no | **PROVEN** unused |
| Does `StartNewGame` invent Oakvale? | recovered TNG + `S_QNOVI` fiber + Oakvale callers | **DIVERGE** |

---

## Timeline (no-save New Game)

```
0042F2A2 Leave frontend
0042F491 Init Game → 004184BD
  Init World 004A6E30
    00CB5D80 / 00CD52D0 fill 161 factory rows
      00CD557C Gameflow / S_GF / 00CEF950          // BIND ONLY
      00CD6E27 Q_NewOakValeIntro / S_QNOVI / 00DBEF70  // BIND ONLY
  00416953 FinalAlbion.wld
    00507C30 START_INITIAL_QUESTS → world+172
    004B4260 six WLD names                         // not Gameflow
  user.ini 009EC890
    ActivateQuest("Gameflow")
      00419CE0 [world+56] vtbl+1104 00892E80
      004B4A10(1,1) → 004B4260 → 00CB5AD0 "Gameflow"
      flag 0 → 004AFA10 reuse 00CDBD20
      00CB7900 vtbl+12 00CE6CF0 then vtbl+4 00CE75B0
        008A9DB0 / 008AE660 insert OV_INTRO…SNOWSPIRE_ARRIVAL
        Main watcher 00CDD450 / 00CB7E50
        S_GF CCutsceneDef DISPROVEN
        GameflowYieldQuest == null                 // construct, no yield
004189C2 first pumps
  first type-1 004B4490 → 00CB8220
    [esi+56] tail-insert:
      Q_SunnyvaleMaster … CS_PlayCutscene          // first yield is Sunnyvale
      Gameflow Main  00CB7950 +41=0 vtbl+4 00A44880
                     00CE7670 state 0
                     00893610 Q_NewOakValeIntro → 0
                     00896A30 OBJECT_QUEST_CARD_OAKVALE_INTRO miss
                     006E7410 / 00A44840 / 009D8650
      CoreQuestReminder / CheckBarrowFieldsGuards  // attached this walk
  later type-1
    00A44660 / 009D87F0 resume 00893610 still 0
    no re-attach Core/Barrow; no ActivateQuest
```

`00DABAC0` / `00DBDE40` / `S_QNOVI` / `NOVI_LiveFather` are
**not** on this list. **PROVEN**.

Construct vs first yield is locked by
`Gameflow_00CE75B0_*` (`GameflowYieldQuest==null` after
`EnterGame`, watchers=`Main` only) then
`Type1_00CB8220_*` (set after first type-1 `Pump(0.1f)`).

---

## 1. `S_GF` is a bind string, not the first runner

`QuestFactoryTable.GameflowScript = "S_GF"`. Native fill
row (`00CD557C`): quest `Gameflow`, script `S_GF`, factory
`00CEF950`, persist `bl=0`, shared run `00CDBD20`.

| Event | Starts `S_GF` interpreter? | Class |
|---|---|---|
| `00CD52D0` `00CB5C90` bind | no | **PROVEN** bind |
| `00CB5AC0` script-def map `S_GF` | no | **PROVEN** name bind |
| `user.ini` `00CB5AD0 "Gameflow"` | factory object + fiber; no `00CBFB7D` | **PROVEN** |
| `00CE75B0` Main | watcher `00CDD450` / `00CB7E50` | **PROVEN** |
| `HasStarted("S_GF")` after Leave / first type-1 | false | **PROVEN** |
| `GameflowAssistance` / `S_GFA` | fill row 6; not WLD / not ini | **PROVEN** omit |

Host `ActivateQuest("Gameflow")` sets
`QuestInstance.ScriptName = "S_GF"` and
`Init = 00CE75B0`. `ChildCutscene` stays null.
`HasStarted` only becomes true when
`StartCutscene` constructs a `ScriptInterpreter`.
Leave never does that for `S_GF`. **PROVEN**.

`GameflowStateNames` (54: `OV_INTRO` …
`SNOWSPIRE_ARRIVAL`) is the `00CE6CF0` insert at
`[0x13BAE44]`. That is script-state, not `FlagStore`
and not `ActivateQuest`. **PROVEN** names; **DISPROVEN**
as Oakvale start.

---

## 2. First Gameflow tick yields on inactive Oakvale

`00CB7950` first-seen: `+40=0` `+44=0` `00F35A00=1`
`+41=0` → `vtbl+4` `00A44880` → `00A446A0` `vtbl+16`
`00CE7640` → `00CE7670`.

State 0 (`00CE77D7`, `SharedRun+4=0`):

| Step | VA | Result |
|---|---|---|
| tattoo / named object | `008902E0` | `00487DC0` miss |
| story log | `00CBE87F` | `TEXT_QST_LOG_STORY_10` |
| quest card | `00896A30` `OBJECT_QUEST_CARD_OAKVALE_INTRO` | `004B0C80` miss |
| is-active | `00893610` `"Q_NewOakValeIntro"` | **0** |
| yield | `006E7410` `vtbl+8` `00A44840` `009D8650` | wait |

Invert + `je` skip-activate: miss → yield. Does **not**
call `004B4A10` / `00CB5AD0`. **PROVEN**
(`00CE78C7` dump; host `TickGameflowMain`).

Same first walk attaches `CoreQuestReminder` (`00CEF3B0`,
`[+72]=0`) and `CheckBarrowFieldsGuards` (`00CEF550`,
trader miss). Host `QuestPumpWalked==9` (six WLD +
Gameflow Main + Core + Barrow). **PROVEN**.

Later type-1: `00A44880` / `00A44660` / `009D87F0`
resumes the same wait. `00893610` still 0. Does not
re-run tattoo/card or re-attach watchers. Host
parked-skip-`00A44880` is **DISPROVEN**.

Host `GameflowYieldQuest` is a **note** of that wait.
`Runtime.Quests` still has no `Q_NewOakValeIntro` row.
`ActivatedQuests` does not contain it. **PROVEN**.

First *named* wait on the type-1 walk is this Gameflow
row. First *fiber* yield on the walk is earlier:
Sunnyvale `00CDD360` `vtbl+28`. See
`proofs/fiber-yield-first/README.md`.

---

## 3. What does **not** activate `Q_NewOakValeIntro`

| Candidate | What it actually does | Class |
|---|---|---|
| `00CD6E27` `00CB5C90` | bind `S_QNOVI` / `00DBEF70` | **PROVEN** bind only |
| WLD `world+172` | `Q_SunnyvaleMaster` … `CS_PlayCutscene` | **PROVEN** not Oakvale |
| `+90584` vs `0x122D70E` | empty → skip `004B4A10` | **PROVEN** skip |
| `004B5080` `START_NEW_QUEST` | save path; 0 external `E8` no-save | **PROVEN** unused |
| `004A113B` `AddTestQuest` | store `world+196` / `NOVStartHSP` | **PROVEN** store |
| `00896A30` / `004B0D30` | need `004AF610` already active | **DISPROVEN** as start |
| `00CE7670` | wait | **PROVEN** wait |
| `user.ini` | `Gameflow` only | **PROVEN** |
| `userst.ini` `NOVStartHSP` | holy-site store `[0x13B866C]` | **DISPROVEN** as quest |

Host must not invent `ActivateQuest("Q_NewOakValeIntro")`
to “unblock” Gameflow. Who later constructs that quest
on no-save is **UNREAD**.

---

## 4. C# `StartNewGame` invents Oakvale

`ScriptRuntime.StartNewGame`:

```
Load(script.bin)
BindScene(things, camera)
InstallRecoveredBindings()
ActivateThings(things)
```

No `"Q_NewOakValeIntro"` / `"StartOakVale"` / father
cutscene literals in that method. A source grep of the
body is locked by
`StartNewGame_uses_recovered_factory_table_not_oakvale_literals`.
That only proves the **strings are not inlined**. It does
not prove Leave uses this helper.

### How Oakvale still runs

| Host table / caller | Native after Leave | Class |
|---|---|---|
| `ScriptFactoryTable.Recovered[0]` = `NOVI_LiveFather` → `CS_OAKVALE_INTRO_FATHER` | TNG name table inside later `00DABAC0` | **LEFTOVER** |
| `ScriptFiberTable.Recovered[0]` = `S_QNOVI` / `AttackOver` | first fibers are seven WLD/ini quests | **LEFTOVER** |
| `PersistTable.Recovered` = `AttackOver` at +80 | first persist is Sunnyvale `00CDC070` | **LEFTOVER** |
| `ActivateThings` on `StartOakValeWest` TNG | Lookout TNG; no father ScriptName | **DIVERGE** |
| `FirstSceneWorld.Build` | first no-save scene is Lookout | **LEFTOVER** |
| `RegionTravel.NewGameRegion = StartOakValeWest` | WLD index 1 is `LookoutPoint` | **LEFTOVER** name |
| `EngineLifecycle.InitCharactersAndQuests` | `ScriptRuntime.Detached()` + `ActivateNamedQuest` | **PROVEN** |
| `EngineLifecycle` → `StartNewGame` | 0 call sites | **PROVEN** unused |

`InstallRecoveredBindings` comment: “`StartNewGame` only
calls this.” True. Leave `ActivateQuest` looks up
`QuestFactoryTable` only and does **not** install the
recovered TNG/fiber tables.

`New_game_trace_is_deterministic_and_drives_shipped_runtime`
loads `RegionTravel.NewGameRegion` things and asserts
`PlayMusic` / `FadeOut` / Hero `Teleport` /
`LookToThing` plus a quest named `S_QNOVI`. That is
**PROVEN** as leftover Oakvale VM behaviour.
**DISPROVEN** as what Leave starts
(`Init_quests_*` `DoesNotContain` `S_QNOVI`;
`No_save_*` `DoesNotContain` `00DBDE40`).

So: **the helper is generic; the recovered tables and
every production/test caller invent Oakvale.** Treating
`StartNewGame` as New Game is **DIVERGE**.

---

## 5. Host Gameflow notes vs native

| Host | Native after Leave | Class |
|---|---|---|
| `ActivateNamedQuest("Gameflow")` from `user.ini` | `00419CE0` → `00892E80` → `004B4A10` | **PROVEN** |
| `SeedGameflowStates` 54 slots + `Main` | `00CE6CF0` / `00CE75B0` | **PROVEN** |
| `TickGameflowMain` notes `00CE7670` / `00893610` | first type-1 | **PROVEN** notes |
| `GameflowYieldQuest = Q_NewOakValeIntro` | wait, not an activate | **PROVEN** note |
| `ResumeGameflowWait` | later `00A44880` | **PROVEN** |
| `HasStarted("S_GF")` | no interpreter | **PROVEN** |
| `Runtime.Update` / `Scheduler.Pump` from Leave `Pump()` | unused | **LEFTOVER** |
| Host fibers stay `Ready` (never `Waiting`) | native `009D8650` | **DIVERGE** |

---

## Classifications (short)

1. **Gameflow after Leave — PROVEN.** `user.ini`
   `ActivateQuest("Gameflow")` constructs factory
   `00CEF950`. Main is `00CE75B0`, not `S_GF`
   `CCutsceneDef`.
2. **`S_GF` as first runner — DISPROVEN.** Bind
   string only. `HasStarted("S_GF")==false`.
3. **First Gameflow tick — PROVEN yield on inactive
   `Q_NewOakValeIntro`.** `00893610` miss → `009D8650`.
   Same wait on resume. Does not activate Oakvale.
4. **`ScriptRuntime.StartNewGame` as New Game —
   DIVERGE.** Unused on Leave. Recovered
   `NOVI_LiveFather` / `S_QNOVI` plus Oakvale TNG
   callers invent the intro VM.
5. **Who later activates `Q_NewOakValeIntro` —
   UNREAD.** Not Leave / not `004B4260` / not
   `00CE7670` / not `user.ini`. Do not invent
   `ActivateQuest` for it.
