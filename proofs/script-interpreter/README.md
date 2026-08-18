# ScriptInterpreter.cs vs native `00CBFB7D`

Investigation only. No production `src/` edits.

Do **not** start at `00DB86B0` / `CS_OAKVALE_INTRO_FATHER` / `PlayMusic`.
That is later `Q_NewOakValeIntro` (`00DABAC0` → TNG `NOVI_LiveFather`).
Leave is `0042F2A2`. First no-save 3D Present does not enter the runner.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Sources: `src/Fable.Game/ScriptInterpreter.cs`,
`src/Fable.Game/Scripting/ExecutionContext.cs`,
`src/Fable.Game/Scripting/ScriptLine.cs`,
`GlobalDispatcher.cs` / `EntityDispatcher.cs`,
`docs/runtime/SCRIPT_FORMAT.md`, `docs/runtime/COMMAND_MAP.md`,
`docs/runtime/FORWARD_TREE.md` §§10–11,
`proofs/newgame-script/README.md`, `proofs/camera-after-leave/README.md`;
ExeIndex family **`script-runtime` v59**
(`tools/Fable.ExeIndex/out/01-sections/script-runtime/`).

---

## Verdict

**Leave does not execute a `CCutsceneDef` opcode.**

`ScriptInterpreter` is the host analog of runner `00CBFB7D`.
After frontend Leave the native pump starts **quest factories**
(`004B4260` / `00CB5AD0`), not this loop.
`CS_PlayCutscene` is an empty 72-byte factory (`00F01760`).
`S_PSM` / `S_GF` / Oakvale father are **DISPROVEN** as first-seen.

| Question | Answer | Class |
|---|---|---|
| First `00CBFB7D` opcode after Leave? | **none** — runner is not on the tree | **PROVEN** |
| First token the *loop* would test if entered? | `.WaitTask` `00CC0783` `00BFEAF8` | **PROVEN** chain order |
| First leftover *command line* if father later starts? | `PlayMusic MUSIC_SET_NULL` | **PROVEN** leftover |
| FadeOut 0.5,0 special-case on that leftover? | skipped (`FirstSeenFadeSpecialCaseRuns=false`) | **PROVEN** leftover |
| Host `ScriptInterpreter` after `DispatchFrontendMessage(15)`? | unused | **LEFTOVER** vs Leave |

---

## Dump: `script-runtime`

Family INDEX: `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\script-runtime\INDEX.md`
(version **59**, exe `42D7DBDF-0106C000-16666624`).

The packet titled “cutscene runner `00CBFB7D`” is **misaligned**:
`cutscene-runner-00cbfb7d-00cbfb7d.md` walks from **`00CBFACA`**.

```
00CBFACA  PlayAnimation splitter  (0099E4C0 walk, comma 0x2C)
  …
00CBFB7B  leave
00CBFB7C  ret
00CBFB7D  push ebp                 ← real interpreter prologue
00CBFB7E  mov eax, 0x18DC          ← 6364-byte frame
00CBFB83  lea ebp, [esp-104]
00CBFB87  call 00BFEA30
```

`leave` here is the **x86 epilogue** of `00CBFACA`, not Leave frontend.
Use `cutscene-runner-exact-00cbfb7d-00cbfb7d.md` for the runner.

| Part | VA | File |
|---|---|---|
| PlayAnimation splitter | `00CBFACA` | `cutscene-runner-00cbfb7d-00cbfb7d.md` |
| Runner exact | `00CBFB7D` | `cutscene-runner-exact-00cbfb7d-00cbfb7d.md` |
| FadeOut 0.5,0 site | `00CBFDD0` | `cutscene-fadeout-0-5-site-00cbfdd0-00cbfdd0.md` |
| `[ebp+120]` vector pick | `00CBFD95` | `cutscene-arg120-00cbfd95-00cbfd95.md` |
| Loop index / `.` ` ` split | `00CC0205` | `command-loop-index-00cc0205-00cc0205.md` |
| First loop token `.WaitTask` | `00CC0783` | `waittask-token-00cc0783-00cc0783.md` |
| Inc PC / `jb 00CC012E` | `00CD17FD` | `command-loop-continue-00cd17fd-00cd17fd.md` |
| Context global | `0x0143E8F8` | `imm-script-global-143e8f8-0143e8f8.md` |

---

## Timeline (no-save New Game)

```
0042F2A2 Leave frontend
0042F491 Init Game → 004184BD
  00416953 Load FinalAlbion.wld
    00CD6E27 00CB5C90 bind Q_NewOakValeIntro / S_QNOVI   BIND ONLY
    00507C30 START_INITIAL_QUESTS → world+172
  004B4260 Init Quests
    00CB5AD0 six WLD names
    CS_PlayCutscene 00F01760 empty   // not 00CBFB7D
    00CB8690 START_SCRIPT_DATA parse // not 00CBFB7D
  user.ini ActivateQuest("Gameflow")
    00CE75B0 Main watcher
    S_GF CCutsceneDef DISPROVEN
004189C2 first pumps
  00CB8220 → 00A44880
  00CE7670 state 0 yields on Q_NewOakValeIntro miss
```

`00DB86B0` → `00CBFB7D` is **not** on this list. **PROVEN**.

---

## 1. Native runner (`00CBFB7D`)

One blocking function. Copies `CCutsceneDef+60` (or `+108` when
`[ebp+120]==1`) via `00432EE9` into a working CString vector.
Walks `[ebp-72]` as PC. Token match is `00BFEAF8(verb, token, n)`
(`strnicmp` of length `00403A00`). Continue is `00CD17FD`:
`inc [ebp-72]` then `jb 00CC012E`.

Ctor locals (exact dump):

| Slot | Init | Host |
|---|---|---|
| `[ebp+103]` | `1` at `00CBFC65` | `CutsceneState.YieldEnable` |
| `[ebp-37]` | `1` at `00CBFD53` | `CameraPauseEnabled` |
| `[ebp-22]` | `1` at `00CBFD57` | `AnimationPauseEnabled` |
| `[ebp-21]` | `0` | skip-list already applied |
| `[ebp-59]` | `0` (`00CC5E97` later 1) | `KeepEntityMap` |
| `[ebp-564]` | `0` | `BlackScreenSubtitles` |
| `[ebp-38]` | `0` | `QuestionLock` |
| `[ebp-39]` | `0` | `FlagRewriteDone` |
| `[ebp-112]` | `0` | `TintHold` |
| `[ebp-72]` | PC | `CutsceneState.Pc` |
| `[ebp-56]…[ebp-48]` | working vector | `Commands` |
| `[0x143E8F8]` | script interface | `ScriptExecutionContext` services |

Head, **before** the token loop (`[ebp+120]!=1`):

1. Compare first line to `"FadeOut 0.5,0"` (`00BFEBA8`).
2. Match → `vtbl+1488(0.5, 0)` (`00CBFDD0`).
3. Always `00CBF29F` UseCamera / CameraLookAt name collect.

First-seen leftover father first line is `PlayMusic`, so step 2
misses. Host `TryFadeSpecialCase` matches that site.

`00DB86B0` pushes `0,0,0` so `+60` is used.
`00CC017C` skip-list (`def+72`) is **DISPROVEN** on first-seen
(`00CBEB7E` false).

---

## 2. First opcode after Leave — three readings

### A. After Leave frontend (`0042F2A2`)

**No `00CBFB7D` opcode.** **PROVEN.**

First script-shaped work is quest construct + fiber
`00A447D0` / tick `00A44880`. Gameflow Main
`00CE7670` **waits**; it does not start a cutscene.

`00CB8690` `START_SCRIPT_DATA` is token parse on the factory
table, **not** the runner. **PROVEN.**

### B. After x86 `leave` at `00CBFB7B`

Next insn is `ret`, then runner `push ebp` at `00CBFB7D`.
That is the **function after** the PlayAnimation splitter,
not a script verb. Do not treat `leave` as frontend Leave.

### C. First token *inside* the loop (`00CC012E`)

Parse at `00CC0205`: `0099E5A0(46)` = `'.'`, then
`0099E5A0(32)` = space. Then the first recovered
`00BFEAF8` is **`.WaitTask`** at `00CC0783`
(push `".WaitTask"` → miss `je 00CC083C` → WalkTo).

That is **first compared**, not first executed after Leave.
The runner is not entered on this path.

| Later leftover (not Leave) | Line | Token | Return |
|---|---|---|---|
| `CS_OAKVALE_INTRO_FATHER[0]` | `PlayMusic MUSIC_SET_NULL` | `00CC8EAC` | `jmp 00CD17FD` no yield |
| `[1]` same slice | `FadeOut 0.5,0` | `00CD0987` | `vtbl+1488` then continue |

---

## 3. `ScriptLine`

`C:\FableCSharp\src\Fable.Game\Scripting\ScriptLine.cs`

One persist CString (`def+60`). Host does **not** invent a
bytecode ISA.

| Piece | Native | Host |
|---|---|---|
| Raw | CString as persisted | `Raw` |
| Actor.verb | last `.` via `0099E5A0(46)` | `LastIndexOf('.')` on unquoted head |
| Family | `ebx` null → global join `00CD17FD`; else entity `00CC707C` | `Target` set → `Entity` |
| Args | comma `0x2C`; quotes kept until unquote | `SplitArgs` |
| TRUE/FALSE/NULL/FOREVER | `00CBEDBA` / `00CBEE0C` / `00CBEE5E` | `IsTrue` / `IsFalse` / `IsNull` / `IsForever` |
| Token prefix | `00BFEAF8` n = `00403A00` | `TokenMatches` |
| `$ARG*` | substitute before apply | `ScriptArguments.Substitute` |

`RemoveThing` is **not** a separate exe token. Native
`00BFEAF8("Remove", 6)` hits it. Host `Eq("Remove") \|\| Eq("RemoveThing")`.
Longer tokens are compared first in the runner. **PROVEN.**

Unknown verbs: native fall-through after the if-chain is
**UNREAD** as a first-seen Leave site (never reached).
Host `CommandResult.Blocked("UNKNOWN")` — **EQUIVALENT** to
“do not no-op”, **DIVERGE** vs inventing Continue.

---

## 4. `ScriptExecutionContext`

`C:\FableCSharp\src\Fable.Game\Scripting\ExecutionContext.cs`

Not a native C++ type name. It is the host bundle of:

1. **`[0x143E8F8]`** `CGameScriptInterface` vtbls (camera 1648…,
   fade 1488/1496, music 2784, yield **+28**).
2. **Runner frame** (`CutsceneState`).
3. **Name env** (`ScriptBindings` ≈ `00CD3D2E` / `008ABD10`).

`ScriptRuntime.BindInterpreter` builds one per
`ScriptInterpreter`. `FindThing` is persist / HERO /
`vtbl+280` analog.

| Host field | Native |
|---|---|
| `Runtime` | process + bank |
| `Bindings` | actor map |
| `Arguments` | invocation `$` slots |
| `Persist` / `Flags` | `004045C0` / WaitFlag bytes |
| `Camera` / `Audio` / `Dialogue` / `Animation` / `Movement` / `World` | interface vtbls |
| `Cutscene` | `00CBFB7D` locals |

Yield is **not** generic. Each handler either
`jmp 00CD17FD` (Continue) or `call [eax+28]` when
`[ebp+103]` (YieldOnce / Wait*). Host
`CommandResult.Kind` is that split.

---

## 5. `ScriptInterpreter.cs` vs the loop

`C:\FableCSharp\src\Fable.Game\ScriptInterpreter.cs`

```
PC==0 → TryFadeSpecialCase          // 00CBFDD0
while PC < Count:
  if Wait* → TickWait else yield    // leftover vtbl+28
  ScriptLine.Parse
  Arguments.Substitute              // unresolved → block
  Entity vs Global dispatcher       // 00BFEAF8 chain
  Continue → PC++
  YieldOnce / Wait* → Yielded
Finished
Resume = 00A44660 analog (clear YieldOnce, re-enter)
```

| Host | Native | Class |
|---|---|---|
| Fetch / PC / `00CD17FD` | same | **PROVEN** |
| `ScriptCommand.Classify` unused | no verb→flow table | **PROVEN** |
| Dispatcher `if Eq(verb)` order | `00BFEAF8` prefix, longer first | **DIVERGE** order; **PARTIAL** match |
| `ApplySkipList` | `00CC017C` | **PROVEN** helper; **DISPROVEN** first-seen |
| `StartCutscene` from Leave | no | **DIVERGE** if host calls it |
| Oakvale `PlayMusic` first | leftover father only | **LEFTOVER** vs Leave |

---

## 6. What Leave *does* start (not this class)

See `proofs/newgame-script/README.md`. Short:

| Order | Name | Factory | `00CBFB7D`? |
|---:|---|---|---|
| 1–5 | Sunnyvale / PSM / PSGT / HeroBoasts / HeroDolls | `00CDBD20` / `00CDE2F0` / … | **DISPROVEN** |
| 6 | `CS_PlayCutscene` | `00F01760` empty | **DISPROVEN** |
| 7 | `Gameflow` | `00CEF950` → `00CE75B0` | **DISPROVEN** |

---

## Classifications (short)

1. **First opcode after Leave frontend — none in `00CBFB7D`. PROVEN.**
2. **Dump “runner `00CBFB7D`” starts at `00CBFACA`; `leave` at `00CBFB7B` is that splitter. PROVEN.**
3. **First loop token if the runner ran — `.WaitTask` `00CC0783`. PROVEN** as compare order, **not** as Leave execute.
4. **`ScriptLine` = persist CString parse. PROVEN.**
5. **`ScriptExecutionContext` = `[0x143E8F8]` + ebp locals. PROVEN** pairing; not a native type name.
6. **`ScriptInterpreter` as what Leave runs — DISPROVEN / LEFTOVER.** Keep as the `CCutsceneDef` VM. Do not `StartCutscene` from Init Quests.
7. **Who later calls `00CBFB7D` on no-save — UNREAD** (not this walk). First leftover line if father starts is `PlayMusic MUSIC_SET_NULL`.
