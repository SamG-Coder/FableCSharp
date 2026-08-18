# First script opcode after Leave / `004B4260` `START_INITIAL_QUESTS`

Investigation only. No production `src` edits.

Do **not** start at `S_QNOVI` / `00DBDE40` / `00DB86B0` /
`CS_OAKVALE_INTRO_FATHER` / `PlayMusic MUSIC_SET_NULL`.
That path is later leftover `Q_NewOakValeIntro`
(`00DABAC0` → TNG `NOVI_LiveFather`), not Leave /
Init Game / first `004B4260`.

Do **not** treat table order in `exe-commands.md` /
`NativeTokens[0]` as first-seen. Those strings are not
executed after Leave.

Do **not** treat the x86 `leave` at `00CBFB7B` as frontend
Leave. That is the PlayAnimation splitter epilogue.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **INVENTED**.

Sources:

- `docs/runtime/FORWARD_TREE.md` §§7–11
- `docs/PARITY.md` Init Quests / Who activates
  `Q_NewOakValeIntro`
- `proofs/newgame-script/README.md`
- `proofs/audit-newgamescript/README.md`
- `proofs/script-interpreter/README.md`
- `proofs/audit-scriptinterpreter/README.md`
- `proofs/script-command-map/README.md`
- `proofs/script-opcode-table/README.md`
- `proofs/fiber-first/README.md`
- `proofs/fiber-yield-first/README.md`
- `proofs/cutscene-first/README.md`
- `EngineLifecycleTests`
  (`Init_quests_004B4260_activates_wld_initial_list`,
  `Activate_quests_00CB5AD0_starts_factory_scripts`,
  `No_save_does_not_activate_Q_NewOakValeIntro`,
  `Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`)
- ExeIndex `script-runtime/`
  (`cutscene-runner-exact-00cbfb7d`,
  `command-loop-index-00cc0205`,
  `listing-00cc0000.txt` `00CC0122`–`00CC083D`)
- `QuestFactoryTable.cs` / `EngineLifecycle.cs`

---

## Verdict

**After Leave, `004B4260` does not execute a script opcode.**

`START_INITIAL_QUESTS` constructs quest factories and
attaches fibers. None of those rows enters runner
`00CBFB7D`. There is no persist CString line, no
`00BFEAF8` verb match, no `00CD17FD` PC increment.

`S_QNOVI` is **bind-only** on this walk
(`00CD6E27` / `00CB5C90`). It is **not** activated,
not constructed, and not the first opcode source.

| Question | Answer | Class |
|---|---|---|
| First `00CBFB7D` opcode after Leave / first `004B4260`? | **none** | **PROVEN** |
| First WLD name / first factory? | `Q_SunnyvaleMaster` / `00CDD550` | **PROVEN** (not an opcode) |
| Any `S_*` `CCutsceneDef` started? | **no** — `HasStarted(S_PSM/S_HB/S_GF)==false` | **PROVEN** |
| `CS_PlayCutscene` a def / opcode source? | **no** — empty `00F01760`, `ScriptName==null` | **PROVEN** |
| `S_QNOVI` / `PlayMusic MUSIC_SET_NULL` first after Leave? | **no** | **DISPROVEN** as this walk; **LEFTOVER** later |
| First *compared* loop token *if* the runner ran? | `.SummonerAttack` `00CC04FA` | **PROVEN** compare order; **not** Leave execute |
| `exe-commands` / `NativeTokens[0]` first after Leave? | `CameraFOVLookBetween` is table head only | **DISPROVEN** as first-seen |
| Native bytecode ISA? | **none** — runner walks persist CStrings | **PROVEN** |

---

## Timeline (no-save New Game)

```
0042F2A2 Leave frontend                 // no 00CBFB7D
0042F491 Init Game → 004184BD
  Init World 004A6E30
    00CB5D80 / 00CD52D0 fill 161 rows   // BIND ONLY
      00CD6E27 00CB5C90 S_QNOVI 00DBEF70   BIND ONLY
  00416953 Load FinalAlbion.wld
    00507C30 START_INITIAL_QUESTS → world+172
  004B4260([world+172])                 // FIRST ACTIVATE
    00CB5AD0 lookup [manager+120]
    004BB720 enqueue
    004B3CE0 construct + 00CB7900
      Q_SunnyvaleMaster   00CDD550 / 00CDBD20   no script
      PersonalScriptMain  00CDE2F0 / S_PSM      factory; not started
      PersonalScript_GlobalThings 00CE19A0
      HeroBoasts          00CE6C40 / S_HB       factory; not started
      V_HeroDolls         00E98640
      CS_PlayCutscene     00F01760 empty        ScriptName==null
    00CB8690 START_SCRIPT_DATA          // parse, not 00CBFB7D
  user.ini ActivateQuest("Gameflow")
    00CE75B0 Main watcher               // S_GF CCutsceneDef DISPROVEN
004189C2 first pumps
  type-1 00CB8220
    Sunnyvale 00CDD360 → vtbl+28 006E7410 009D8650   // C++ yield
    Gameflow 00CE7670 state 0
      00893610 Q_NewOakValeIntro → 0 → yield
```

`00DABAC0` / `00DBDE40` / `00DB86B0` / `00CBFB7D` /
`PlayMusic` / `FadeOut` / `UseCamera` are **not** on
this list. **PROVEN.**

---

## 1. What “opcode” means here

Native is **not** a packed bytecode stream.
`00CBFB7D` copies `CCutsceneDef+60` (or `+108` when
`[ebp+120]==1`) via `00432EE9`, then walks `[ebp-72]`
as a dword PC over CStrings. Token match is
`00BFEAF8` (`strnicmp` of length `00403A00`).
Continue is `00CD17FD` (`inc` then `jb 00CC012E`).

That loop is the only recovered *script opcode*
machine. Quest factories, 24-byte VM records
(`00CB6EA0`), and `00A44880` fibers are **other**
machines. Calling those an opcode is **INVENTED**.

`00CB8690` `START_SCRIPT_DATA` is factory-table
token parse. It is **not** the runner. **PROVEN**
(`FORWARD_TREE` §11).

---

## 2. After Leave / `004B4260` — no opcode

| Claim | Class | Evidence |
|---|---|---|
| Leave `0042F2A2` `E8`s `00CBFB7D` | **DISPROVEN** | 0 E8; Leave is fade / clear / black Present |
| First `004B4260` `E8`s `00CBFB7D` | **DISPROVEN** | lookup `00CB5AD0` + construct `004B3CE0` |
| WLD six + Gameflow start `CCutsceneDef`s | **DISPROVEN** | `HasStarted("S_PSM")==false`; `S_HB` / `S_GF` same |
| `CS_PlayCutscene` plays a def | **DISPROVEN** | `play.ScriptName==null`; size 72 vtbl `012F72D0` |
| `Q_NewOakValeIntro` in `world+172` | **DISPROVEN** | WLD head / QST TRUE start at Sunnyvale; Oakvale is `+196` test store |
| `00CD6E27` starts `S_QNOVI` | **DISPROVEN** | bind `00CB5C90` only |
| First type-1 enters the runner | **DISPROVEN** | `00CB8220` is C++ `Main` / `vtbl+28` yield |
| Host `ScriptInterpreter` after msg 15 | unused | **LEFTOVER** vs Leave |

`Init_quests_004B4260_*` and
`Activate_quests_00CB5AD0_*` lock the activate list
and the `HasStarted` falses. **PROVEN.**

First *script-shaped* work after Leave is still not
an opcode:

| Order | Event | Opcode? |
|---|---|---|
| 1 | `00507C30` world-map fiber `00A44740` | no |
| 2 | `004B4260` factory construct + watcher `00CDD450` | no |
| 3 | `00CB8690` parse | no |
| 4 | type-1 Sunnyvale `00CDD360` → `009D8650` | no |
| 5 | type-1 Gameflow `00CE7670` wait | no |

Sunnyvale wait predicate is **UNREAD**. Gameflow
wait is `00893610("Q_NewOakValeIntro")==0`. Neither
is `00BFEAF8`. **PROVEN** as not-opcode.

---

## 3. Not `S_QNOVI`

| Event | Starts `S_QNOVI`? | Class |
|---|---|---|
| `00CD52D0` fill row `00CD6E27` | no — bind | **PROVEN** |
| `AddTestQuest` → `world+196` | no | **PROVEN** |
| First `004B4260` / `00CB5AD0` | no | **PROVEN** |
| `user.ini` Gameflow | no — Main watcher | **PROVEN** |
| First type-1 `00CE7670` | no — yields | **PROVEN** |
| `00DABAC0` slot 2 / `00DB86B0` | yes, *if* constructed | **DISPROVEN** as first after Leave |
| `ScriptRuntime.StartNewGame` | host invents the fiber | **DIVERGE** |

First leftover *executed* line **if** father later
starts is `PlayMusic MUSIC_SET_NULL` (`00CC8EAC` →
`jmp 00CD17FD`). Head special-case
`FadeOut 0.5,0` misses. That pairing is
**LEFTOVER**, not “first opcode after Leave.”
Who activates `Q_NewOakValeIntro` on no-save is
**UNREAD** (not Leave / not `004B4260` / not
`00CE7670`).

---

## 4. Other “first opcode” readings (do not collapse)

### A. Frontend Leave (`0042F2A2`) / first `004B4260`

**None.** This is the question this packet answers.

### B. x86 `leave` at `00CBFB7B`

`cutscene-runner-00cbfb7d` walks from **`00CBFACA`**.
`leave` / `ret` then `00CBFB7D push ebp` is the
runner prologue. Not frontend Leave. Not a verb.

### C. First `00BFEAF8` *inside* the line loop

`proofs/script-interpreter` said `.WaitTask`
`00CC0783`. **DISPROVEN** by
`listing-00cc0000.txt` (see
`proofs/audit-scriptinterpreter` §3).

After parse / optional `$` walk:

| Order | VA | Token |
|---:|---|---|
| 1 | `00CC04FA` | `.SummonerAttack` |
| 2 | `00CC0616` | `.DoBossFight` |
| 3 | `00CC0783` | `.WaitTask` |
| 4 | `00CC083D` | `.WalkTo` |
| … | `00CC8EAC` | `PlayMusic` |

First *compared* ≠ first *executed* ≠ first after
Leave. The runner is not entered on this path.

### D. `exe-commands` / `NativeTokens` table head

`CameraFOVLookBetween` `0x012C1870` is the first
verb **string** in the dump slice. After Leave:
**none** of that table runs. Calling it first-seen
is **INVENTED**.

---

## 5. C# vs native

| Host | Native after Leave | Class |
|---|---|---|
| `EngineLifecycle` `World.InitialQuests` + Gameflow | `004B4260` then `user.ini` | **PROVEN** first names |
| `HasStarted(S_PSM/S_HB/S_GF)==false` | factory only | **PROVEN** |
| `ScriptInterpreter` / `StartCutscene` | none | **LEFTOVER** if wired here |
| `ScriptRuntime.StartNewGame` + `S_QNOVI` | invented Oakvale | **DIVERGE** |
| Dispatcher `Eq` order (`PlayMusic` first) | native `.SummonerAttack` first | **DIVERGE** (unread on this walk) |
| `NativeTokens[0]` as first-seen | table, not execute | **INVENTED** pairing |

Do **not** `StartCutscene(S_PSM)` from a factory
ctor. **PROVEN.**

---

## Classifications (short)

1. **First script opcode after Leave / first
   `004B4260` — none. PROVEN.**
2. **First constructed quest —
   `Q_SunnyvaleMaster`. PROVEN.** Not an opcode.
3. **`S_QNOVI` / father `PlayMusic` as first after
   Leave — DISPROVEN.** Bind-only; leftover later.
4. **`S_PSM` / `S_HB` / `S_GF` / `CS_PlayCutscene`
   as opcode sources — DISPROVEN.**
5. **First compared loop token if entered —
   `.SummonerAttack` `00CC04FA`. PROVEN.**
   `.WaitTask`-first is **DISPROVEN**.
6. **Table-order first verb as first-seen —
   INVENTED.**
7. **Who later enters `00CBFB7D` on no-save —
   UNREAD** (not this walk).
