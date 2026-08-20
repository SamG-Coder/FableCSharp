# `00DBDE40` StartOakVale vs host (after a proven activate)

Investigation only. No production `src/` or `tests/` edits.
Do **not** wire `StartOakVale` / `00DBDE40` into
`EngineLifecycle.Pump`. Do **not** invent
`ActivateQuest("Q_NewOakValeIntro")` from no-save Leave.
Do **not** collapse leftover **#4** (Lookout first Present vs
Oakvale intro view).

Question: map `00DBDE40` to the host. What is still missing
to **RUN** this fiber when `Q_NewOakValeIntro` is **actually
activated** — not on no-save first Present?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH** /
**blocked-on-activator**.

Authority: `src/Fable.Game/RegionTravel.cs`,
`ScriptFactoryTable.cs`, `ScriptRuntime.cs`,
`QuestFactoryTable.cs`, `EngineLifecycle.cs`,
`FirstSceneWorld.cs`, `NewGameScript.cs`;
`EngineLifecycleTests.No_save_does_not_activate_Q_NewOakValeIntro`,
`Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`,
`Type1_resume_00CB8220_is_00A44880_then_00893610_yield`;
`docs/PARITY.md` “Who activates `Q_NewOakValeIntro`”;
`docs/status/README.md` leftover #4;
ExeIndex `calls-startoakvale-00dbde40` (1 hit: `00DAC295`);
`out/01-sections/script-bank/native-sqnovi.md`;
siblings `proofs/region-travel-first`, `script-factory-tables`,
`00DBB2A7-attackover-store`, `audit-newgamescript`,
`audit-firstsceneworld`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Does no-save Leave / first Present run `00DBDE40`? | **No.** Only PE `E8` is `00DAC295` inside `00DABAC0`. | **DISPROVEN** |
| Does Gameflow activate Oakvale? | **No.** `00CE7670` `00893610` **waits**. Host hardcodes miss `0`. | **PROVEN** wait |
| Is the activator on this no-save walk? | **No.** Bind / QST / `+90584` / `004B5080` / `AddTestQuest` are not construct. | **blocked-on-activator** |
| After a *proven* `00CB5AD0("Q_NewOakValeIntro")`, does the host run `00DBDE40`? | **No.** Table miss → generic fiber → generic `TickNamedQuestMain` yield. | **DIVERGE** |
| Are the VAs / names already on the host? | **Yes**, as constants + bind Notes. Not as a runnable fiber. | **MATCH** data |
| Is `FirstSceneWorld` / `StartNewGame` this fiber? | **No.** Fixture soup. Leftover #4 ledger. | **LEFTOVER** |

---

## Verdict

**`00DBDE40` is recovered as constants and as a later
`S_QNOVI` slot-2 callee. It is not a live host fiber.**

Native first-seen body (after construct, after map-ready /
`00CB7940`): `CREATURE_HERO_CHILD`, three `00CDD450`
watchers, `Q_NewOakValeIntro_PreAttack`, `vtbl+2584(12.0)`,
`HerosOldHouse`, spin `[this+80]`.

Host New Game Pump **must not** jump there. No-save first
Present is Lookout (`leftover #4`). Even if
`ActivateNamedQuest("Q_NewOakValeIntro")` were called from a
**proven** site, the host still would not enter `00DBDE40`.

Do **not** grow `Pump` / `TickWorld` / `PumpQuests` to Note
or execute `StartOakValeSetup`. Do **not** collapse
`FIRST_SCENE_*` / `FirstSceneWorld` into this gap.

---

## Native path (only after construct)

```
00CD6E27  00CB5C90 bind Q_NewOakValeIntro / S_QNOVI / 00DBEF70   // BIND
          (161-row 00CD52D0 fill; persist bl=0)

????      proven 004B00C0 → 00CB5AD0("Q_NewOakValeIntro")       // ACTIVATOR UNREAD
          004BB720 / 004B3CE0
          00DBEF70 alloc 0x10C ctor 00DAAC00 vtbl 012D7A28
          00CB7900 vtbl+12 then vtbl+4
          slot1 00DAACE0 Main 00CDD450 / 00CDD440               // not a frame tick
          00A447D0 fiber
          00DAADA0 004045C0("AttackOver", this+80)              // bind, not store 1
          00A44880 / 00A446A0 [vtbl+8] = 00DABAC0               // zero E8 of slot 2
00DABAC0  00CB8230 NOVI_* factories (LiveFather 00DAC2C0 …)
          E8 00DBDE40                                           // only caller 00DAC295
00DBDE40  yield ctx vtbl+28
          wait map-ready + 00CB7940 hero-exists
          CREATURE_HERO_CHILD
          00CDD450 ×3 (0.1f / 64 / 1, vtbl 012D7A3C)
            WatchBarrels        00DBE890
            WatchForGotGold     00DBE2E0
            ManageQuestCoreMarkers 00DBE4E0
          Q_NewOakValeIntro_PreAttack
          vtbl+2592(1, &+76) then vtbl+2584(12.0)               // 00DBE128
          lookup HerosOldHouse                                  // 00DBE15E
          spin [this+80]                                        // AttackOver; no mov here
          // +80=1 is 00DBB2A7 after Theresa CS + raid AVI
```

Gameflow `00CE7670` state 0 is a **peer waiter**
(`00893610` miss → `006E7410` / `009D8650`). It does not
construct Oakvale. After a proven activate, native
`00893610` would return 1 and Gameflow would leave that
yield. That is **not** `00DBDE40`.

---

## Host map (MATCH vs DIVERGE vs leftover vs blocked)

### MATCH (data / notes already locked)

| Host | Native | Evidence |
|---|---|---|
| `RegionTravel.StartOakValeSetup = 00DBDE40` | same | `WorldSceneTests` |
| `IntroQuest` / `S_QNOVI` / `00DBEF70` / `00DAAC00` / vtbl `012D7A28` / size `0x10C` | factory record | `RegionTravel` + tests |
| `IntroQuestRun = 00DABAC0` slot 2, call site `00DAC295` | only `E8` | ExeIndex 1 hit |
| Watcher VAs / `0.1f` / 64 / 1 / PreAttack 12 s / `HerosOldHouse` / gate `+80` | first-seen body | `RegionTravel` |
| `FirstSeenPlus80WrittenInStartOakVale = false` | no `mov [esi+80]` in `00DBDE00–00DBF000` | `00DBB2A7-attackover-store` |
| `OakvaleBindSite` Note `00CD6E27` bind not `00CB5AD0` | fill row | `LoadQuestDefs` |
| `WorldPlus184` / `QuestManagerPlus44` contain the name | QST `AddQuest` FALSE | `No_save_does_not_activate_*` |
| `WorldPlus172` / `ActivatedQuests` omit the name | not QST TRUE / not WLD initial | same |
| `GameflowWaitQuest` yield | `00893610` miss | type-1 tests |
| `ScriptFactoryTable.Recovered` `NOVI_*` + `00DAC2C0` | `00DABAC0` name table | later leftover, dump-backed |
| `PersistTable.AttackOverWrite = 00DAADA0` | persist bind | `00DBB2A7` is the later store |
| Pump traces never contain `Va==00DBDE40` on no-save | 0 `E8` from Leave / `004189C2` | dozens of tests |

### DIVERGE (would still miss after a proven activate)

| Host site | Native after `00CB5AD0` | Gap |
|---|---|---|
| `QuestFactoryTable.Recovered` (8 rows, no Oakvale) | 161-row fill includes `00CD6E27` | `Find` returns null |
| `ScriptRuntime.ActivateQuest` | “Does not install `S_QNOVI` / `00DBDE40`” | generic `QuestInstance` + fiber; no `StartFactory(00DBEF70, 00DABAC0, …)` |
| `ActivateNamedQuest` factory arm | `004B3CE0` `00DBEF70` / `00DAAC00` | skipped when `Find` misses |
| Persist on activate | `00DAADA0` `AttackOver` at `+80` | QST `!Persistent` + no factory bind |
| `TickNamedQuestMain` else-arm | `00A44880` → `00DABAC0` → `00DBDE40` | Note `00CB7950` + `009D8650` yield only |
| `ResumeGameflowWait` | `00893610` reads live active | Note always `"… 0"`; `GameflowYieldQuest` stays parked |
| `PumpGame` / `PumpGameUpdate` | `00A44880` resumes slot 2 | no `Runtime.Update`; comment: host walk of `00CB7950` / `Runtime.Update` is leftover |
| `PumpScripts` | not this fiber | `+60` empty Note; `ScriptPumpWalked=0` |
| `LoadFromFirstRealRegion` | `00DBDE40` map-wait `StartOakVale` | first real region is Lookout index **1** |

`004B00C0` itself would **MATCH** if someone called
`ActivateNamedQuest`: the name is already on QM+44. The
construct/run after that gate is the diverge.

### leftover (do not treat as the fiber; do not collapse #4)

| Host | What it is | Why leftover |
|---|---|---|
| leftover **#4** | Lookout first *rendered* scene vs Oakvale *intro view* (`StartOakValeWest` / `HerosOldHouse` / `CAM_OVIF_SHOT2`) | Keep `FIRST_SCENE_*` + Lookout ledgers. This file is the **fiber**, not the view. |
| `FirstSceneWorld.Build` | TNG + SHOT2 + `ScriptRuntime.StartNewGame` soup | Not `Pump`. Not `00DBDE40`. No client caller. |
| `ScriptRuntime.StartNewGame` / `InstallRecoveredBindings` | registers `NOVI_*`, creates `S_QNOVI`/`AttackOver` fiber | Observation / fixture. Never called from `EngineLifecycle`. Does not execute `00DABAC0`. |
| `NewGameScript` | façade over `Runtime.Update` | “does not assert CutsceneStarted”. Not the quest object. |
| `ScriptFactoryTable` as “New Game list” | TNG name table recovered from later `00DABAC0` | Wrong table vs `00CD52D0`. |
| `RegionTravel.NewGameRegion = StartOakValeWest` | intro-view contract | **DISPROVEN** as first no-save region. |

### blocked-on-activator

| Candidate | Why it is not the construct |
|---|---|
| `00CD6E27` | `00CB5C90` bind only |
| WLD `START_INITIAL_QUESTS` / `world+172` | `Q_SunnyvaleMaster` … not Oakvale |
| `+90584` / `00416BCF` | empty vs `0x122D70E` → skip `004B4A10` |
| `004B5080` | save `START_NEW_QUEST`; **0** external `E8` on no-save |
| `004A113B` `AddTestQuest` | `[world+196]` store only |
| `00896A30` / `004B0D30` | need `004AF610` already active |
| `00CE7670` / `00893610` | wait, not activate |
| `user.ini` | `ActivateQuest("Gameflow")` only |
| Click `UI_TEXT_NEW_GAME` / leftover PARITY “click path” | no-save first Present is Lookout; do not revive as activator |

Activator site remains **UNREAD**. Host must not invent it.

---

## What is still missing to RUN `00DBDE40` after a proven activate

Ordered dependencies. Stop at the first unproven item.
Do **not** satisfy these by calling `00DBDE40` from `Pump`.

1. **Proven activator** — a dump `E8`/`vtbl` of
   `004B4A10` / `00CB5AD0("Q_NewOakValeIntro")` that is
   **not** no-save first Present. **blocked-on-activator**.
2. **`QuestFactoryTable` row** —
   `Q_NewOakValeIntro` / `S_QNOVI` / factory `00DBEF70` /
   run slot 2 `00DABAC0` / persist `0`. Without this,
   `ActivateQuest` stays a nameless fiber.
3. **Construct** — `00DBEF70` → `00DAAC00` size `0x10C`
   vtbl `012D7A28`; `00CB7900` vtbl+12 then vtbl+4;
   slot 1 `00DAACE0` Main watcher `00CDD450`/`00CDD440`.
4. **Persist bind** — `00DAADA0` / `004045C0("AttackOver",
   this+80)`. Do **not** write `+80=1` here
   (`00DBB2A7` is later).
5. **Fiber slot 2** — `00A447D0` then `00A44880` /
   `00A446A0` `[vtbl+8]=00DABAC0`. Zero `E8` of slot 2
   from Pump. `TickNamedQuestMain` else-arm is not this.
6. **`00DABAC0` name table** — `00CB8230` `NOVI_*` (already
   listed in `ScriptFactoryTable`) **before** the map-wait.
7. **Map-ready `StartOakVale`** — region index **4** /
   `StartOakValeWest` TNG. Not Lookout. Not leftover #4
   first Present. `00DBDE40` yields until this.
8. **`00CB7940` + `CREATURE_HERO_CHILD`** — hero-exists
   predicate then kid lookup. No-save bind is adult
   `CREATURE_HERO` / `Hero`.
9. **Three `00CDD450` watchers** — `WatchBarrels`
   `00DBE890`, `WatchForGotGold` `00DBE2E0`,
   `ManageQuestCoreMarkers` `00DBE4E0` (`NOVI_*` later;
   do not follow off StartOakVale).
10. **`Q_NewOakValeIntro_PreAttack`** + **12 s**
    `vtbl+2592(1,&+76)` then `vtbl+2584(12.0f)`.
11. **`HerosOldHouse` lookup** then **`+80` spin**.
    Writer is not this function.
12. **Gameflow peer** — after activate, `00893610` must
    read live `ActivatedQuests`, not the host `" 0"`
    string in `ResumeGameflowWait`. Separate from the
    Oakvale fiber.

`ScriptRuntime.Update` already resumes **cutscene**
interpreters (`00CBFB7D` / `CS_OAKVALE_INTRO_FATHER` via
`NOVI_LiveFather` construct). That is **after** names are
registered and TNG constructs. It is **not** a substitute
for steps 2–7.

---

## Timeline (no-save New Game — `00DBDE40` absent)

```
0042F2A2 Leave
00416953 Load world FinalAlbion.wld
  00CD6E27 bind S_QNOVI / 00DBEF70                 // MATCH bind
  +172 QST TRUE  (no Q_NewOakValeIntro)            // MATCH omit
  +184 / QM+44 include Q_NewOakValeIntro           // MATCH name list
004B4260 [world+172] constructs  (not Oakvale)
user.ini ActivateQuest("Gameflow")
004189C2 Pump
  00CB8220 / 00A44880 / 00CE7670
    00893610 Q_NewOakValeIntro = 0                 // MATCH wait
    yield 006E7410                                 // not 00DBDE40
  first real region Lookout (leftover #4)
```

No `Note(StartOakValeSetup)`. **PROVEN**. Keep it that way
on this walk.

---

## Do not

- Call `00DBDE40` from `Pump` / `RequestNewGame` /
  `EnterGame` / first `004189C2`.
- Invent `ActivateQuest("Q_NewOakValeIntro")` to “reach”
  the fiber.
- Collapse leftover **#4** (Lookout Present vs Oakvale
  intro view / `FIRST_SCENE_*` / `FirstSceneWorld`).
- Treat `StartNewGame` / `InstallRecoveredBindings` as
  live New Game construct.
- Write `[this+80]=1` inside a host `00DBDE40` analog.
- Follow `ManageQuestCoreMarkers` / PostAttack
  (`00DBE3C0`) off first-seen StartOakVale.
