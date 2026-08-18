# Audit: `ScriptRuntime.StartNewGame` vs native `004B4260`

Investigation only. No production `src/` edits.

Do **not** start at `00DBDE40` / `StartOakVale` / `S_QNOVI` /
`NOVI_LiveFather` / `CS_OAKVALE_INTRO_FATHER`. That is later
`Q_NewOakValeIntro` slot 2 (`00DABAC0` → `00DBDE40`), not Leave /
first `004B4260`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: `src/Fable.Game/ScriptRuntime.cs` (`StartNewGame` /
`InstallRecoveredBindings` / `ActivateQuest` / `ActivateThings`);
`ScriptFactoryTable.cs` / `ScriptFiberTable` / `PersistTable.Recovered`;
`QuestFactoryTable.cs`; `EngineLifecycle.InitCharactersAndQuests` /
`ActivateNamedQuest`; `FirstSceneWorld.Build`;
`listing-00480000.txt` `0049F180` / `004B4260` / `004B4A10`;
`listing-00cc0000.txt` `00CD6E14`; `strings.tsv` `0x012F789C`;
`EngineLifecycleTests.Init_quests_004B4260_activates_wld_initial_list`;
`ScriptRuntimeParityTests.StartNewGame_*`;
`proofs/audit-newgamescript`, `audit-lifecycle-newgame`, `qst-first-load`,
`wld-parse`, `fiber-first`, `newgame-script`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Invented `S_QNOVI`? | **The name is native. Activating it from `StartNewGame` as New Game is invented.** PE string `0x012F789C`. Registrar `00CD6E14` / `00CD6E48` binds it to `Q_NewOakValeIntro` / factory `00DBEF70` (`00CB5C90`). It is **not** in `script.bin`. Host `StartNewGame` creates a `QuestInstance` **named** `S_QNOVI` and starts the father cutscene. Native first `004B4260` never does that. | see §1 |
| `StartNewGame` == `004B4260`? | **No.** Native is QuestManager: Activate Quest on `[world+172]`. Host leftover is bank load + Oakvale TNG + recovered father/`S_QNOVI` fiber. Live New Game is `EngineLifecycle.InitCharactersAndQuests`, not `StartNewGame`. | see §2–4 |

---

## Verdict

**`S_QNOVI` is not a host-invented token.** It is the exe script
name for `Q_NewOakValeIntro`. Bind-only on the no-save walk
(`00CD6E27`). Not in `world+172`. Not started by `004B4260`.

**`ScriptRuntime.StartNewGame` is not `004B4260`.** Treating it as
New Game **DIVERGE**. It plants `ScriptFiberTable.Recovered[0]`
(`S_QNOVI` + `AttackOver`) and `ActivateThings` on Oakvale TNG
(`NOVI_LiveFather` → `CS_OAKVALE_INTRO_FATHER`). Native first
activate is `Q_SunnyvaleMaster` via `00CB5AD0`.

Live client (`Fable.Client` / `EngineLifecycle`) never calls
`StartNewGame`. **PROVEN** absence. Callers are
`FirstSceneWorld.Build` and tests.

---

## Path (must be Leave → `004B4260([world+172])`)

```
0042F2A2  Leave frontend
00416953  Loading world FinalAlbion.wld
  004A1840
    00CD52D0 fill (161) includes
      00CD6E14 push "S_QNOVI"
      00CD6E27 push "Q_NewOakValeIntro"
      [esp+32]=00DBEF70
      00CB5C90 bind                         // BIND ONLY
    004A0D90 AddQuest TRUE → CWorld+172     // not WLD START_INITIAL_QUESTS
  0049F180 "Init Characters" / "Init GUI"
  0049F21B "Init Quests"
  0049F247 lea edx, [esi+172]
  0049F24E call 004B4260                    // FIRST ACTIVATE
    loop:
      "QuestManager: Activate Quest"
      004B00C0 gate
      [manager+120] 00CB5AD0                // lookup
      004BB720 enqueue 12-byte
    004B3CE0 walk queue                     // factory + fiber
  00416BCF +90584 empty skip 004B4A10
  user.ini ActivateQuest("Gameflow")
    00419CE0 → 00892E80 → 004B4A10 → 004B4260
```

`00DABAC0` / `00DBDE40` / `00CB8230` / `004C97B0` are **not** on
this list. **PROVEN**.

---

## 1. Invented `S_QNOVI`?

### 1a. Name — **PROVEN native**

| Store | Evidence |
|---|---|
| PE string | `strings.tsv` `0x012F789C` `S_QNOVI` |
| Registrar | `00CD6E14` / `00CD6E48` `push "S_QNOVI"` then `00CB5AC0` / `00CB5C90` |
| Quest name | `00CD6E27` `push "Q_NewOakValeIntro"` |
| Factory | `[esp+32]=0xDBEF70` (`00CD6E55`) |
| `script.bin` | **absent** (`script-bank/newgame.md`, `native-sqnovi.md`) |

Pairing is **quest** `Q_NewOakValeIntro` + **script** `S_QNOVI` +
factory `00DBEF70`. Persist on that bind row is `bl` (not 1).
QST `AddQuest` is **FALSE**; `AddTestQuest` → `world+196` only.

### 1b. Host object named `S_QNOVI` — **invented as New Game**

`ScriptFiberTable.Recovered[0]`:

```
new(RegionTravel.IntroScriptName,   // "S_QNOVI"
    NewGameScript.PersistAttackOverName,  // "AttackOver"
    …)
```

`InstallRecoveredBindings` then:

```
CreateFiber(fiber.Name, fiber.PersistField);
new QuestInstance(…, fiber.Name, …);   // quest.Name == "S_QNOVI"
Scheduler.Create(fiber.Name, …);
```

Native constructed quest name would be **`Q_NewOakValeIntro`**,
script field `S_QNOVI`. Host collapses them into one
`QuestInstance` named the script. First persist after Leave is
Sunnyvale `00CDC070`, not `AttackOver` `00DAADA0`.

| Claim | Class |
|---|---|
| String `S_QNOVI` exists in exe | **PROVEN** |
| Bind at `00CD6E27` during `00CD52D0` | **PROVEN** bind |
| `S_QNOVI` is a `script.bin` row | **DISPROVEN** |
| First `004B4260` name is `S_QNOVI` | **DISPROVEN** (`Q_SunnyvaleMaster`) |
| `StartNewGame` `QuestInstance.Name == "S_QNOVI"` | **DIVERGE** (wrong name + invented activate) |
| `AttackOver` first persist | **DISPROVEN** pairing |
| Who later `00CB5AD0`s `Q_NewOakValeIntro` | **UNREAD** (not this walk) |

Do not invent `ActivateQuest(Q_NewOakValeIntro)` to “fix” the
name. Activator is **UNREAD**.

---

## 2. Native `004B4260` (what it actually does)

`listing-00480000.txt` `004B4260`:

```
004B4260  sub esp, 44
          ebp = name-vector (arg)
          edi = ecx (QuestManager)
          eax = (end-begin)>>2
          jbe 004B437F                    // empty list
004B42A2  push "QuestManager: Activate Quest"
004B42D7  call 004B00C0                   // gate
          je skip
004B42E4  ecx = [edi+120]
004B42E8  call 00CB5AD0                   // factory record or 0
004B431A  call 004BB720                   // enqueue (0 factory still)
          inc index; jb loop
004B4386  call 004B3CE0                   // construct queued
004B43C4  ret 12
```

First no-save site: `0049F247` `lea edx, [esi+172]` /
`0049F24E` `call 004B4260`. Same fn later from `004B4A5A`
(Activate Initial Quests / `user.ini` Gameflow).

`world+172` writer is QST `AddQuest` TRUE (`004A10C4`), **not**
`00507C30` `START_INITIAL_QUESTS` (`wld-parse`: token **not** in
that switch). Nine TRUE names; host walks WLD six + ini Gameflow.

| # | Native `+172` (QST TRUE) | Host `ActivateNamedQuest` |
|--:|---|---|
| 1 | `Q_SunnyvaleMaster` | yes |
| 2 | `ChapterAndSceneManager` | **no** |
| 3 | `PersonalScriptMain` | yes |
| 4 | `PersonalScript_GlobalThings` | yes |
| 5 | `NPCDeath` | **no** |
| 6 | `HeroBoasts` | yes |
| 7 | `V_HeroDolls` | yes |
| 8 | `CS_PlayCutscene` | yes |
| 9 | `Global_WatchForHeroDeath` | **no** |
| — | `Gameflow` (ini, not `+172`) | yes, 7th |

`Q_NewOakValeIntro` / `S_QNOVI` are **not** in this table.
**PROVEN**.

---

## 3. C# `StartNewGame` (what it actually does)

```
StartNewGame(install, things, camera)
  Load(ScriptBank.Load)                 // script.bin
  BindScene(things, camera)
  InstallRecoveredBindings()
    ScriptFactoryTable.Recovered        // NOVI_LiveFather → CS_OAKVALE_INTRO_FATHER
    Persist.InstallRecovered()          // AttackOver bool +80
    ScriptFiberTable.Recovered          // QuestInstance "S_QNOVI"
  ActivateThings(things)
    TNG ScriptName in registry → StartCutscene
```

Comments say “generic engine startup” and “Oakvale facts live in
`ScriptFactoryTable`.” The recovered tables **are** the Oakvale
slice. `StartNewGameFactoryKind` / `StartNewGameFiberKind` always
return `BindingKind.ProvenGeneric`. That label is **LEFTOVER** vs
Leave (helpers are real; pairing is not first-seen).

`ActivateQuest` (the `004B4260` analog on `ScriptRuntime`) is
**not** called from `StartNewGame`. `QuestFactoryTable` is unused
on this path.

Production New Game:

```
EngineLifecycle.InitCharactersAndQuests
  Runtime = ScriptRuntime.Detached()
  Load script.bin
  foreach World.InitialQuests: ActivateNamedQuest   // 004B4260
  // no InstallRecoveredBindings
user.ini → ActivateNamedQuest("Gameflow")
```

`Fable.Client` constructs `EngineLifecycle`. **0** `StartNewGame`
call sites in `EngineLifecycle.cs` / `SilkEngineHost.cs`.

---

## 4. Side-by-side

| Step | Native first `004B4260` | `ScriptRuntime.StartNewGame` | Class |
|---|---|---|---|
| When | after Leave, `0049F24E` | tests / `FirstSceneWorld` only | **DIVERGE** as live New Game |
| Arg | `[world+172]` name vector | Oakvale TNG `things` | **DIVERGE** |
| Lookup | `00CB5AD0` quest factory | `ScriptFactoryTable` TNG names | **DIVERGE** |
| First name | `Q_SunnyvaleMaster` | `S_QNOVI` fiber | **DISPROVEN** |
| Persist | Sunnyvale `00CDC070` | `AttackOver` | **DISPROVEN** pairing |
| Fiber | per queued quest after `004B3CE0` | one `S_QNOVI` | **DIVERGE** |
| Cutscene | none (`00CBFB7D` 0) | `CS_OAKVALE_INTRO_FATHER` via TNG | **DIVERGE** |
| `script.bin` load | native earlier `00CB5D80` @ `00416968`; host load is here | load here | **PARTIAL** / late |
| Region | LookoutPoint later `00501450` | `StartOakValeWest` | **LEFTOVER** |
| `Q_NewOakValeIntro` | bind only | invented start as `S_QNOVI` | **DIVERGE** |

`ScriptRuntime.ActivateQuest` **is** the host helper for
`00CB5AD0` + fiber. `InitCharactersAndQuests` uses it.
`StartNewGame` does not.

---

## 5. Bind vs start (do not mix)

| Event | Starts `S_QNOVI` body? | Class |
|---|---|---|
| `00CD52D0` 161-row fill | no | **PROVEN** bind |
| `00CD6E27` Oakvale row | no | **PROVEN** bind |
| `004A0D90` TRUE → `+172` | no Oakvale | **PROVEN** |
| `004B4260` / `00CB5AD0` | factories for `+172` + later Gameflow | **PROVEN**; **DISPROVEN** Oakvale |
| `00DABAC0` vtbl+8 | yes, *if* quest constructed | **PROVEN** later; 0 `E8` |
| `StartNewGame` | **yes (host)** | **DIVERGE** |

---

## Classifications (short)

1. **`S_QNOVI` name — PROVEN native, not invented.**
   String + `00CB5C90` bind to `Q_NewOakValeIntro` / `00DBEF70`.
   Not a `script.bin` entry.

2. **`S_QNOVI` as first New Game quest / `StartNewGame` fiber —
   DISPROVEN / invented activate.** First `004B4260` name is
   `Q_SunnyvaleMaster`. Host names the quest the script.

3. **`StartNewGame` vs `004B4260` — DIVERGE.** Different arg,
   table, first name, persist, cutscene, region. Live path is
   `InitCharactersAndQuests` (`ScriptRuntime.Detached` +
   `ActivateQuest`).

4. **`FirstSceneWorld` / Oakvale TNG `ActivateThings` — LEFTOVER.**
   Accurate as later `00DABAC0` notes. Not Leave.

5. **Who constructs `Q_NewOakValeIntro` on no-save — UNREAD.**
   Do not invent `ActivateQuest` for it from this audit.
