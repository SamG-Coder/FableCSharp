# First script fiber yield / wait after Leave — `ScriptScheduler`

Investigation only. No production `src/` edits.

Do **not** start at `S_QNOVI` / `00DABAC0` / `00DBDE40` /
`AttackOver` / `00CBFB7D` `.WaitTask`. That is later
`Q_NewOakValeIntro`. Leave is `0042F2A2`. First no-save type-1
does not enter the cutscene runner.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: `src/Fable.Game/Scripting/ScriptScheduler.cs`,
`ScriptRuntime.ActivateQuest` / `Update`, `EngineLifecycle.PumpQuests`,
`QuestFactoryTable`, `ScriptFiberTable`;
`proofs/newgame-script/README.md`, `proofs/script-interpreter/README.md`,
`proofs/script-factory-tables/README.md`;
`docs/runtime/FORWARD_TREE.md` §§10–11, `docs/PARITY.md` type-1 rows;
`EngineLifecycleTests` (`Init_quests_004B4260_activates_wld_initial_list`,
`Gameflow_00CE75B0_is_Main_watcher_not_S_GF`,
`Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`,
`Type1_resume_00CB8220_is_00A44880_then_00893610_yield`);
`ScriptSchedulerTests`;
ExeIndex `script-runtime/` (`microthread-create-00a447d0`,
`microthread-update-00a44880`, `microthread-resume-00a44660`,
`microthread-yield-00a44690`, `microthread-fiber-entry-00a446a0`,
`microthread-00a44840`, `novi-name-register-00cb8230` = **`00CB8220`**,
`hero-exists-00cb7940` = **`00CB7950`**,
`q-newoakvaleintro-script-00ce7670` / `00ce78c7`).

---

## Verdict

**First fiber *create* after Leave is `00A447D0` at Init Quests.
First fiber *yield/wait* is the first type-1 `00CB8220` walk,
not construct and not `ScriptInterpreter`.**

`[esi+56]` is tail-insert: WLD `START_INITIAL_QUESTS` first,
`user.ini` Gameflow last. The first body on that walk is
`Q_SunnyvaleMaster` `00CDD360` → `vtbl+28` `006E7410` →
`009D8650`. Gameflow `00CE7670` state 0 wait on inactive
`Q_NewOakValeIntro` is the same pump, **later** in the list.

`ScriptScheduler` is the host analog of `00A44880` /
`00A44660` / `00A44690`. Pairing it to `S_QNOVI` /
`AttackOver` is **LEFTOVER**. `EngineLifecycle` after Leave
**notes** the native yield; it does not call
`Scheduler.Pump`.

| Question | Answer | Class |
|---|---|---|
| First `00A447D0` after Leave? | `004B4260` ×6 WLD + Gameflow (7) | **PROVEN** |
| First `009D8650` after Leave? | first type-1 `00CB8220`, Sunnyvale `00CDD360` first | **PROVEN** order |
| What Sunnyvale waits *on*? | `vtbl+28` / `006E7410` only | **UNREAD** predicate |
| First *named* wait (quest string)? | Gameflow `00893610("Q_NewOakValeIntro")` = 0 | **PROVEN** |
| `00CBFB7D` / `.WaitTask` on this path? | none | **DISPROVEN** |
| `ScriptScheduler.Create("S_QNOVI","AttackOver")` as Leave first fiber? | leftover Oakvale pairing | **DISPROVEN** |
| `Runtime.Update` / `Scheduler.Pump` from Leave `Pump()`? | unused | **LEFTOVER** |

---

## Timeline (no-save New Game)

```
0042F2A2 Leave frontend
0042F491 Init Game → 004184BD
  004A6E30 Init World
    004A6550 Init Scripts 006E7740 → world+56
    00CB5D80 / 00CD52D0 fill 161 factory rows   // BIND ONLY
  00416953 FinalAlbion.wld
    00507C30 START_INITIAL_QUESTS → world+172
    004B4260([world+172])
      00CB5AD0 / 004BB720 / 004B3CE0
      00A447D0 fiber ×6                         // CREATE, no yield
    user.ini ActivateQuest("Gameflow")
      00CE6CF0 seed OV_INTRO…SNOWSPIRE_ARRIVAL
      00CE75B0 Main 00CDD450 / 00CB7E50
      00A447D0 7th fiber                        // CREATE, GameflowYieldQuest=null
004189C2 first pumps
  first type-1 004A5A40 → 004B4490 → 00CB8220
    00CB7C40 then 00CB8170
    [esi+56] tail-insert:
      Q_SunnyvaleMaster  00CDD360 vtbl+28 006E7410 009D8650   ← FIRST YIELD
      PersonalScriptMain 00CDDCB0
      PersonalScript_GlobalThings 00CDDCB0
      HeroBoasts         00CE1AF0 00CE1C24 yield
      V_HeroDolls
      CS_PlayCutscene    empty factory
      Gameflow Main      00CB7950 +41=0 vtbl+4 00A44880
                         00CE7670 state 0
                         00893610 Q_NewOakValeIntro → 0
                         006E7410 / 00A44840 / 009D8650
      CoreQuestReminder  00CEF3B0 [+72]=0 yield
      CheckBarrowFieldsGuards 00CEF550 trader miss yield
  006E75C0 [world+56] [this+60] empty           // not a fiber yield
```

`00DABAC0` / `00DBDE40` / `00CBFB7D` / `AttackOver` are
**not** on this list. **PROVEN**.

Construct vs first yield is locked by
`Gameflow_00CE75B0_*` (`GameflowYieldQuest==null` after
`EnterGame`) then `Type1_00CB8220_*` (set after the first
type-1 `Pump(0.1f)`). **PROVEN**.

---

## 1. Native scheduler (not `ScriptInterpreter`)

`00A447D0` / `00A44880` / `00A44660` / `00A44690` have
**0** `E8` callers. They are vtbl / fiber-entry only.

| VA | Role | Body (dumps) |
|---|---|---|
| `00A447D0` | create | if `[this+16]` destroy `009D8640`; `009D8710(entry=00A446A0)`; store handle at `+16`; `[+5]=0`. Fail string `"Failed to create Microthread"`. |
| `00A446A0` | fiber entry | while `[+5]==0`: first pass `[vtbl+16]`, then loop `[vtbl+8]`; dead path `009D8650` forever. |
| `00A44880` | tick (Gameflow `vtbl+4`) | `[0x13D2838]==0`: enqueue on `0x13D2828`, `009E1BC0` → `[this+8]`, `00A44660`. Nested: `00A44690` then restore global. |
| `00A44660` | resume | `[0x13D2838]=this`; `009D87F0([this+16])`; clear global. |
| `00A44690` | yield | `009D8650` only. |
| `00A44840` | wait thunk | `00A4B220` else `00A44690` then retry. Gameflow `006E7410` `vtbl+8`. |
| `00CB7950` | attach / dispatch | `[+44]=arg`; `[+40]!=0` skip; `00F35A00`; `[+41]!=0` → `vtbl+24` then clear `+41`; **`[+41]==0` → `vtbl+4` `00A44880`**. First-seen `+41=0`. |
| `00CB8220` | type-1 list | `00CB7C40` then `jmp 00CB8170`. Dump title “NOVI name register” is the **next** fn (`00CB8230`). |

`NewGameScript.Scheduler = 0x013D2828` is the native queue
object `00A44880` uses. Host `ScriptScheduler` is a
`List<FiberState>`, not that object. **PROVEN** address;
**DIVERGE** representation.

`00CB78D0` writes `+41`. `00A447D0` writes `+5`, not `+41`.
First-seen stays 0 so `00CB7950` always takes `00A44880`.
Host “skip `00A44880` when parked” is **DISPROVEN**
(`Type1_resume_*`).

---

## 2. First yield on the type-1 walk

`004B4490` walks constructed quests. Empty `[esi+56]`
skips `00CB8220` (no install). After Gameflow construct
the list is seven factory objects plus Gameflow watchers
inserted at tail during `00CE7670`.

Host `PumpQuestList` matches native tail-insert: named WLD
quests first, Gameflow Main / Core / Barrow last.
`QuestPumpWalked==9` on the first type-1. **PROVEN**
(`Type1_00CB8220_*`).

| Walk # | Object | Tick | Yield site | Wait target |
|--:|---|---|---|---|
| 1 | `Q_SunnyvaleMaster` | `00CDD360` | `vtbl+28` `006E7410` `009D8650` | **UNREAD** (no `00CDD360` dump) |
| 2 | `PersonalScriptMain` | `00CDDCB0` | `vtbl+72` `0089AC10` empty | empty thing list |
| 3 | `PersonalScript_GlobalThings` | `00CDDCB0` | same | empty |
| 4 | `HeroBoasts` | `00CE1AF0` | `00CE1C24` | empty Main |
| 5 | `V_HeroDolls` | `00CB7950` Main | `009D8650` | **UNREAD** body |
| 6 | `CS_PlayCutscene` | empty `00F01760` | `009D8650` note | no `CCutsceneDef` |
| 7 | Gameflow `Main` | `00A44880` → `00CE7670` | `006E7410` / `00A44840` / `009D8650` | `Q_NewOakValeIntro` inactive |
| 8 | `CoreQuestReminder` | `00CEF3B0` | `006E7410` | `Gameflow+72==0` (not guild `TEXT_QST_078`) |
| 9 | `CheckBarrowFieldsGuards` | `00CEF550` | `006E7410` | `Q_TraderConflict*` miss |

**First `009D8650` after Leave is row 1.** **PROVEN** as
order (`FORWARD_TREE` “`00CDD360` first; Gameflow last”,
PARITY first-seen `006874B0` row, host notes
`SunnyvaleMainTick` before `GameflowTickFn`).

**First recovered *predicate* is row 7.** `00CE78C7`:
push `"Q_NewOakValeIntro"` / `"OBJECT_QUEST_CARD_OAKVALE_INTRO"`;
`[esi+64].vtbl+100` (`00893610`) → `al`; invert; `je` skip
activate; miss → yield. Does **not** `ActivateQuest`.
**PROVEN**.

`00CE7670` first half attaches `CoreQuestReminder`
(`+52=00CEF3B0`) and `CheckBarrowFieldsGuards`
(`+52=00CEF550`) via `00CDD450` / `00CB7E50`. That attach
is **first type-1 only**. Later type-1 resumes
`00893610` still 0 and does not re-attach. **PROVEN**.

`006E75C0` (script-manager pump, `world+56`) is the same
type-1 tick **after** `00CB8220`. Flag=1, `[this+60]`
empty circular → skip `0059299D`. **DISPROVEN** as first
fiber yield. Different object from `00A44880`.

---

## 3. `ScriptScheduler.cs` vs Leave

`C:\FableCSharp\src\Fable.Game\Scripting\ScriptScheduler.cs`

Comment: analog of `00A44880` / resume `00A44660` / yield
`00A44690`. Addresses match. Behaviour:

| Host | Native after Leave | Class |
|---|---|---|
| `Create(name, persist)` | `00A447D0` at `004B4260` / Gameflow | **PROVEN** timing if called from `ActivateQuest` |
| `FiberState.DtAtPlus8` | `009E1BC0` → `[this+8]` | **PROVEN** offset |
| `Pump` foreach Ready/Waiting | `00A44880` drain of `0x13D2828` | **PARTIAL** (host has no nested-global / `00A44930`) |
| `Pump` resumes `ScriptInterpreter` | `00A44660` / `009D87F0` resumes C++ stack (`00CE7670` / `00CDD360`) | **DIVERGE** |
| `ScriptFiberTable.Recovered` = `S_QNOVI`+`AttackOver` | first fibers are seven WLD/ini quests | **LEFTOVER** |
| `NewGameScript.UpdateFn=00A44880` | generic tick, not Oakvale-only | **PROVEN** address; **DISPROVEN** exclusive pairing |
| `EngineLifecycle.PumpQuests` | `00CB8220` notes | **PROVEN** notes; **LEFTOVER** vs `Scheduler.Pump` |
| `Runtime.Update` from Leave `Pump()` | unused (`EngineLifecycle` comment: leftover) | **LEFTOVER** |

`ActivateQuest` does create a `Scheduler` fiber per WLD /
Gameflow name. After `EnterGame`,
`Runtime.Scheduler.Fibers.Count==7` and every
`QuestInstance.Fiber` is set. **PROVEN**
(`Init_quests_004B4260_*`). Those fibers stay `Ready`.
Host never marks `Waiting` on `Q_NewOakValeIntro` or
Sunnyvale `vtbl+28`. **DIVERGE** vs native first type-1.

`ScriptSchedulerTests` first `Create` is
`"S_QNOVI","AttackOver"`. Valid as a generic two-fiber
unit test. **DISPROVEN** as the Leave first-yield case.

---

## 4. What is *not* the first wait

| Candidate | Why not first after Leave | Class |
|---|---|---|
| `00CBFB7D` `.WaitTask` `00CC0783` | runner not entered | **DISPROVEN** |
| `S_QNOVI` `vtbl+2584(12.0)` / `HerosOldHouse` / `+80` | quest not activated | **DISPROVEN** |
| `AttackOver` persist `00DAADA0` | first persist is Sunnyvale `00CDC070` | **DISPROVEN** pairing |
| Gameflow `S_GF` opcode wait | `HasStarted("S_GF")==false`; Main is `00CE75B0` / `00CE7670` | **DISPROVEN** |
| `006E75C0` script list | empty `+60` | **DISPROVEN** |
| Create-time `00A447D0` | `009D8710` allocates; resume is `00A44880` | **PROVEN** no yield at construct |
| `006E7410` `0049D870` WorldFrame compare | after resume, not first yield | **PROVEN** (PARITY) |
| Host `WaitFlag` / `WaitFrames` / `WaitActiveDialog` | `ScriptInterpreter` leftover | **LEFTOVER** |

Who later activates `Q_NewOakValeIntro` (so Gameflow’s
wait completes) is **UNREAD**. Not Leave / not `004B4260` /
not `00CE7670`. Do not invent `ActivateQuest` for it.

`00CDD360` instruction body is **UNREAD** (no ExeIndex
part). Host/PARITY pairing to `vtbl+28` `006E7410` is
**PARTIAL** until that walk exists.

---

## Classifications (short)

1. **First fiber after Leave — PROVEN create `00A447D0` ×7
   at `004B4260` + Gameflow. First yield — PROVEN first
   type-1 `00CB8220`, Sunnyvale `00CDD360` first in
   tail-insert order.**
2. **First *named* wait — PROVEN Gameflow `00CE7670`
   `00893610("Q_NewOakValeIntro")` miss → `009D8650`.
   Same walk, not first row.**
3. **`ScriptScheduler` as Leave scheduler — PARTIAL
   addresses / 7 fibers. DIVERGE as interpreter pump.
   `S_QNOVI`/`AttackOver` Create — LEFTOVER.**
4. **`00CBFB7D` / Oakvale 12 s wait / `WaitTask` as first
   yield — DISPROVEN.**
5. **Sunnyvale `00CDD360` wait predicate — UNREAD.**
   Dump that fn before claiming a flag name.
