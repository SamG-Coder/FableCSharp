# NewGameScript.cs vs native New Game script path

Investigation only. No production `src/` edits.

Do **not** start at `00DBDE40` / `StartOakVale` / `S_QNOVI`.
That is later `Q_NewOakValeIntro` slot 2 (`00DABAC0` → `00DBDE40`).
The no-save New Game click is message **15** → Leave `0042F2A2`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Sources: `src/Fable.Game/NewGameScript.cs`, `ScriptRuntime.StartNewGame`,
`QuestFactoryTable`, `EngineLifecycle` (`ActivateNewGame` / `RequestNewGame` /
`InitCharactersAndQuests`); `docs/runtime/FORWARD_TREE.md` §§4–11, 15;
`docs/PARITY.md` New Game / Init Quests / Gameflow rows;
`EngineLifecycleTests` (`Frontend_0059A238_message_15_sets_retail_41`,
`New_game_is_leave_frontend_then_FinalAlbion_wld`,
`Init_quests_004B4260_activates_wld_initial_list`,
`Activate_quests_00CB5AD0_starts_factory_scripts`,
`No_save_does_not_activate_Q_NewOakValeIntro`,
`Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`);
`WorldSceneTests.New_game_intro_runs_through_generic_script_runtime`.

---

## Verdict

**`NewGameScript` is not the New Game script path.**

It is an observation façade over first-seen **`S_QNOVI`**
(`Q_NewOakValeIntro` / `NOVI_LiveFather` / `AttackOver` / Oakvale fade).
Native no-save New Game from UI message 15 never constructs that object
and never `E8`s `00DBDE40`.

The scripts that *do* start after Leave are WLD
`START_INITIAL_QUESTS` plus `user.ini` `ActivateQuest("Gameflow")`.
Gameflow **waits** on `Q_NewOakValeIntro` (`00893610` miss) and
**yields**. It does not activate it.

| Host type | Native New Game (msg 15 → Leave) | Class |
|---|---|---|
| `NewGameScript` | unused | **LEFTOVER** name + pairing |
| `ScriptRuntime.StartNewGame` | unused | **DIVERGE** (invents Oakvale TNG + `S_QNOVI` fiber) |
| `FirstSceneWorld` | unused | **LEFTOVER** (`StartOakValeWest` / SHOT2) |
| `EngineLifecycle.ActivateNewGame` / `RequestNewGame` / `ActivateNamedQuest` | msg 15 → `0042F2A2` → `004B4260` / `00CB5AD0` | **PROVEN** pairing |

---

## Timeline (no-save New Game scripts)

```
0042EC7C retail
  00595B24 menu  UI_TEXT_NEW_GAME id=0
  0059A238 UI vtbl+32 (012521C8)
    msg 15 → 0059A2DA [ui+28].vtbl+16
           → 00594F28 [retail+41]=1
  [esi+41] → 0042F2A2 Leave frontend     // not 00DBDE40
    [0x1375448]=0
    [0x13B8616]==0 skip bank swaps
    00404490 / 004131A0
    FinalAlbion.wld
    0042EBB6 teardown (+41 skip audio stop)
    009BE420 + 009BEEB0
0042F491 Init Game
  00418DCA size 0x161E8 vtbl 0122F180
  vtbl+4 004184BD
    Init World 004A6E30
    vtbl+32 00416953 Load world
      004A1840
        004A0D90 AddQuest / AddTestQuest → world+184
        00CD6E27 00CB5C90 bind Q_NewOakValeIntro / S_QNOVI / 00DBEF70
          BIND ONLY — not 00CB5AD0
        00507C30 START_INITIAL_QUESTS → world+172
        00B23DC0 / 00B428E0 FinalAlbion.stb miss
      [0x13B8648]==0
        0049F180 Init Characters / Init GUI
        004B4260([world+172]) Init Quests
          00CB5AD0 lookup [manager+120]
          004BB720 / 004B3CE0 construct
          00A447D0 fiber
        00416BCF +90584 empty → 004B4A10
    user.ini 009EC890
      ActivateQuest("Gameflow")
        00419CE0 [world+56] vtbl+1104 00892E80
        004B4A10(1,1) → 004B4260 → 00CB5AD0 "Gameflow"
        00CE6CF0 seed OV_INTRO…SNOWSPIRE_ARRIVAL
        00CE75B0 Main watcher 00CDD450 / 00CB7E50
        S_GF CCutsceneDef DISPROVEN at this site
004189C2 first pumps
  type-1 004B4490 / 00CB8220
    00A44880 (generic microthread — not S_QNOVI body)
    00CE7670 state 0
      00893610 Q_NewOakValeIntro → 0
      009D8650 yield
    does not ActivateQuest(Q_NewOakValeIntro)
```

`S_QNOVI` / `00DABAC0` / `00DBDE40` / `NOVI_LiveFather` /
`CS_OAKVALE_INTRO_FATHER` are **not** on this list. **PROVEN**.

---

## 1. What `NewGameScript.cs` actually is

`C:\FableCSharp\src\Fable.Game\NewGameScript.cs` is a thin wrapper:

- constants for the **24-byte VM list** (`00CB5C90` bind /
  `00CB7210` store / `00CB7780` start / `00CB70E0` invoke /
  `00CB6EA0` walk / `00CB6CE0` per-item)
- `S_QNOVI` fiber (`00A44880` / `00A446A0` / `00A44660` / `00A44690`)
- persist **`AttackOver`** (`00DAADA0` / `004045C0` / `this+80`)
- `NOVI_LiveFather` factory `00DAC2C0` / vtbl `012D8388`
- live getters: `Gate80`, `CutsceneStarted` (`CS_OAKVALE_INTRO_FATHER`),
  `PlayMusic` / `FadeOut`

Comment on the type: “Observation façade over `ScriptRuntime` for
first-seen `S_QNOVI`.” That is accurate as a *later-quest* façade.
It is **not** accurate as the New Game click path.

No `EngineLifecycle` member constructs `NewGameScript`.
Production New Game is `DispatchFrontendMessage(15)` →
`RequestNewGame` → `EnterGame` → `ActivateNamedQuest`.

| Constant | Native role | On msg-15 / Leave path? |
|---|---|---|
| `ListWalk` `00CB6EA0` | generic 24-byte VM walk | **UNREAD** as first-seen after Leave (Oakvale list only if `S_QNOVI` runs) |
| `UpdateFn` `00A44880` | generic fiber tick | **PROVEN** reuse (Gameflow / HeroBoasts / `CS_PlayCutscene` vtbl+24) |
| `FiberEntry` `00A446A0` | generic fiber entry | **PROVEN** reuse (`00CE7640` Gameflow Main) |
| `PersistAttackOver` `00DAADA0` | `S_QNOVI` `AttackOver` at +80 | **DISPROVEN** as first New Game persist |
| `LiveFatherFactory` `00DAC2C0` | TNG `NOVI_LiveFather` | **DISPROVEN** (Lookout TNG, adult `CREATURE_HERO`) |
| `ContextGlobal` `0143E8F8` | script context | **PARTIAL** vs manager `0143E8F0` (`006E7740`) |
| `RegisteringScripts` `00CB5D80` | script-name registrar | **DISPROVEN** as New Game registrar (`00CD52D0` is the quest table) |

`WorldSceneTests` pins those constants and then drives
`ScriptRuntime.StartNewGame(install, StartOakValeWest things)`.
That test is **PROVEN** as *Oakvale intro VM behaviour*.
It is **DISPROVEN** as *what Leave starts*.

---

## 2. Message 15 is a flag, not a quest start

| Site | What it does | Class |
|---|---|---|
| `00595B24` | builds menu; `UI_TEXT_NEW_GAME` is item 0 | **PROVEN** |
| `0059A238` vtbl+32 | dispatch | **PROVEN** |
| msg 15 → `0059A2DA` | `[ui+28].vtbl+16` | **PROVEN** |
| `00594F28` | `[retail+41]=1` | **PROVEN** |
| `0042EC7C` | reads +41 → `0042F2A2` | **PROVEN** |
| msg 15 starts `S_QNOVI` / `00DBDE40` / `StartNewGame` | — | **DISPROVEN** |
| Native widget click that posts 15 | id=0 | **UNREAD** |
| Host N/Enter → msg 15 after Press Start | queue only | **PARTIAL** |

Press Start is msg `0xE5`, not 15. Return-from-Press-Start → msg 15
is **DISPROVEN**.

---

## 3. Scripts that actually start after Leave

### 3a. WLD `START_INITIAL_QUESTS` (`world+172` from `00507C30`)

`004B4260` → `00CB5AD0` → `004BB720` / `004B3CE0`.
Gate `[0x1375454]=1` (`.data`) so construct runs. BSS-0 stub
**DISPROVEN**.

| Order | Quest | Script | Factory | Run / init | CCutsceneDef first-seen? |
|---:|---|---|---|---|---|
| 1 | `Q_SunnyvaleMaster` | none | `00CDD550` | `00CDBD20` / `00CDBA10` zeros + `_LIKE`/`_HATE`; persist `00CDC070` | no |
| 2 | `PersonalScriptMain` | `S_PSM` | `00CDE2F0` | reuse `004AFA10` / `00CDBD20`; tick `00CDDCB0` | **DISPROVEN** (`HasStarted("S_PSM")==false`) |
| 3 | `PersonalScript_GlobalThings` | `S_PSGT` | `00CE19A0` | reuse SharedRun | no |
| 4 | `HeroBoasts` | `S_HB` | `00CE6C40` | Main `00CE1A30` watcher `00CDD450` | **DISPROVEN** (`HasStarted("S_HB")==false`) |
| 5 | `V_HeroDolls` | `S_VHDS` | `00E98640` | reuse SharedRun | no |
| 6 | `CS_PlayCutscene` | none | `00F01760` size 72 vtbl `012F72D0` | empty factory | **DISPROVEN** (`ScriptName==null`) |

Do **not** `StartCutscene(S_PSM)` from the factory ctor. **PROVEN**.

`+90584` empty vs `0x122D70E` still calls `004B4A10` for the
Activate-Initial-Quests *site*, but that site is **not**
`Q_NewOakValeIntro`. `004B5080` is save `START_NEW_QUEST`
(0 external `E8` on no-save). **PROVEN**.

### 3b. `user.ini` Gameflow (7th activate)

Not a WLD initial name. Path:

`00419D90` / `00419CE0` → `[world+56]` `006E7740` vtbl+1104
`00892E80` → `004B4A10(1,1)` → `004B4260` → `00CB5AD0 "Gameflow"`.

Direct `00CB5AD0` from the ini parser is **DISPROVEN**.

Factory `00CEF950` flag 0 → `004AFA10` reuse `00CDBD20`.
`00CB7900` → `00CE6CF0` inserts `OV_INTRO`…`SNOWSPIRE_ARRIVAL`
at `[0x13BAE44]`, then `00CE75B0` Main. `S_GF` interpreter
at activate is **DISPROVEN**.

### 3c. Gameflow waits; it does not start Oakvale

First type-1 `00CB8220` after construct:

`00CB7950` `+41=0` → `00A44880` → `00CE7670` state 0 →
`00893610("Q_NewOakValeIntro")` = 0 → `009D8650` yield.

Same wait on later resumes. Host must not invent
`ActivateQuest(Q_NewOakValeIntro)`. **PROVEN**.

Who *does* activate that quest on no-save is **UNREAD**
(not Leave / not `004184BD` / not `004B4260` / not `00CE7670` /
not `004B5080` / not `AddTestQuest` store).

---

## 4. Bind vs start (the leftover trap)

| Event | VA | Starts `S_QNOVI`? | Class |
|---|---|---|---|
| Registrar bind name+factory | `00CD6E27` `00CB5C90` `S_QNOVI`/`00DBEF70` | no | **PROVEN** bind |
| WLD `AddTestQuest("Q_NewOakValeIntro","NOVStartHSP")` | `004A113B` → `world+196` | no | **PROVEN** store |
| `QuestFactoryTable` register | `00CD52D0` | no Oakvale row | **PROVEN** |
| `004B4260` / `00CB5AD0` | initial list + Gameflow | no | **PROVEN** |
| Gameflow `00893610` | wait | no | **PROVEN** |
| `00DABAC0` slot 2 | only caller of `00DBDE40` | yes, *if constructed* | **PROVEN** later leftover |
| `ScriptRuntime.StartNewGame` | host | **yes (invented)** | **DIVERGE** |
| `NewGameScript` ctor | host observation | requires the above | **LEFTOVER** vs Leave |

`00CB5C90` appears in **both** `NewGameScript.BindFactory` and
`QuestFactoryTable.Bind`. Same helper, different tables.
New Game uses the **quest** table (`00CD52D0`), not the
`S_QNOVI` VM list start (`00CB7780`).

---

## 5. First persist / first fiber after Leave

| Native first-seen | `NewGameScript` assumption | Class |
|---|---|---|
| Sunnyvale 38 slots via `00CDC070` (`004045C0` bool / `00410BE0` int), defaults `00CDBA10` zeros | `AttackOver` at +80 | **DISPROVEN** pairing |
| `00A447D0` fiber per initial quest + Gameflow (7 fibers) | one `S_QNOVI` + `AttackOver` fiber | **DIVERGE** |
| `00A44880` ticks Gameflow Main / Sunnyvale `00CDD360` yield | `NewGameScript.Update` → Oakvale interpreter | **DIVERGE** |
| `004045C0` name helper | same helper, different name | **PROVEN** helper; **DISPROVEN** as `AttackOver` first |

`NewGameScript.UpdateFn == 00A44880` is a real native address.
Pairing it exclusively to `S_QNOVI` / `AttackOver` is the leftover.

---

## 6. C# vs native

| Host | Native after msg 15 | Class |
|---|---|---|
| `EngineLifecycle.DispatchFrontendMessage(15)` | `0059A238` → +41 | **PROVEN** |
| `RequestNewGame` / `EnterGame` | `0042F2A2` → `004184BD` → `FinalAlbion.wld` | **PROVEN** |
| `InitCharactersAndQuests` / `ActivateNamedQuest` | `004B4260` / `00CB5AD0` + `user.ini` Gameflow | **PROVEN** |
| `Runtime.ActivateQuest` without `InstallRecoveredBindings` | factory table only | **PROVEN** vs Leave |
| `ScriptRuntime.StartNewGame` | load bank + `InstallRecoveredBindings` + `ActivateThings` on Oakvale TNG | **DIVERGE** |
| `new NewGameScript(runtime)` | none | **LEFTOVER** |
| `FirstSceneWorld.Build` `StartOakValeWest` + SHOT2 | first no-save scene is LookoutPoint + hero 4299 | **LEFTOVER** |
| `RegionTravel.NewGameRegion = StartOakValeWest` | no-save first map is LookoutPoint | **LEFTOVER** name |

---

## Classifications (short)

1. **`NewGameScript` = New Game script path — DISPROVEN.**
   It observes `S_QNOVI`. Leave never starts that quest.
2. **Native New Game scripts — PROVEN:**
   message 15 → `[retail+41]` → `0042F2A2` → `004184BD` →
   `004B4260` six WLD names → `user.ini` `Gameflow` →
   `00CE7670` yield on `Q_NewOakValeIntro`.
3. **`00DBDE40` / `StartOakVale` / `CREATURE_HERO_CHILD` on this click — DISPROVEN.**
   Only caller is later `00DABAC0`.
4. **`ScriptRuntime.StartNewGame` + `NewGameScript` as live New Game — DIVERGE / LEFTOVER.**
   Keep as Oakvale-intro VM notes. Do not call them from Leave.
5. **Who later activates `Q_NewOakValeIntro` on no-save — UNREAD.**
   Not this walk. Do not invent `ActivateQuest` for it.
