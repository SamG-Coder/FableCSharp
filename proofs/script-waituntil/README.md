# First WaitUntil / WaitFor script command after Leave

Investigation only. No production `src/` edits.

Do **not** start at `CS_OAKVALEINTRO_HESDEADJIM` /
`WaitForCamera` / `CS_OPENGRAVE_CRYPTCAM` /
`CS_PUNCHCLUB_*`. Those are later leftover
`CCutsceneDef` lines, not Leave / Init Game / first
no-save type-1.

Do **not** treat Gameflow `00893610("Q_NewOakValeIntro")`
or Sunnyvale `vtbl+28` as `WaitFor*`. Those are fiber
yields on the 24-byte quest walk, not runner verbs.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Sources: `src/Fable.Game/Scripting/GlobalDispatcher.cs`
(`WaitForCamera` / `WaitForMessageCamera`),
`EntityDispatcher.cs` (`WaitForAnimationEvent` /
`WaitForUnderRadius`), `ScriptedCamera.cs`,
`ScriptCommandMap` (`NativeTokens`, `All`),
`QuestFactoryTable`, `EngineLifecycle`;
`proofs/script-command-map/README.md`,
`proofs/script-interpreter/README.md`,
`proofs/script-global-cmds/README.md`,
`proofs/script-entity-cmds/README.md`,
`proofs/script-flags-first/README.md`,
`proofs/fiber-yield-first/README.md`,
`proofs/camera-after-leave/README.md`,
`proofs/newgame-script/README.md`,
`proofs/cutscene-first/README.md`;
`docs/runtime/COMMAND_MAP.generated.md`;
ExeIndex `script-bank/exe-commands.md`,
`script-bank/entries-tsv.md`,
`script-bank/0481-cs-oakvale-intro-father.md`,
`script-bank/0485-cs-oakvaleintro-hesdeadjim.md`,
`text-map/listing-00cc0000.txt`
(`00CCA41E` / `00CC4044` / `00CC41FB` / `00CCFF3D`);
`EngineLifecycleTests` (`Init_quests_004B4260_*`,
`No_save_does_not_activate_Q_NewOakValeIntro`);
`WorldSceneTests` (`FirstSeenCallsUseCamera=false`);
`ScriptRuntimeArchitectureTests` (`WaitForCamera_*`).

---

## Verdict

**After Leave there is no `WaitUntil` / `WaitFor*`
script command.**

The native verb table has **no** `WaitUntil` or bare
`WaitFor` token. The only `WaitFor*` commands are
`WaitForCamera`, `WaitForMessageCamera`,
`.WaitForAnimationEvent`, and `.WaitForUnderRadius`.
All four live in runner `00CBFB7D`. That runner is
**not** on the no-save Leave tree.

| Question | Answer | Class |
|---|---|---|
| First `WaitUntil` after Leave? | token does not exist | **DISPROVEN** as a command |
| First `WaitFor` / `WaitFor*` after Leave? | **none** — `00CBFB7D` not entered | **PROVEN** |
| First leftover *WaitFor\** line if intro later starts? | `CS_OAKVALEINTRO_HESDEADJIM` `WaitForCamera` | **PROVEN** leftover |
| Father leftover has `WaitFor*`? | no — `WaitTask` / `WaitActiveDialog` only | **DISPROVEN** |
| Gameflow wait on `Q_NewOakValeIntro` is `WaitFor*`? | no — `00893610` miss + `009D8650` | **DISPROVEN** |
| Host `WaitForCamera` after `DispatchFrontendMessage(15)`? | unused | **LEFTOVER** vs Leave |

---

## Timeline (no-save New Game)

```
0042F2A2  Leave frontend
004184BD  Init Game → FinalAlbion.wld
004B4260  START_INITIAL_QUESTS
  Q_SunnyvaleMaster
  PersonalScriptMain    HasStarted(S_PSM)==false
  PersonalScript_GlobalThings
  HeroBoasts            HasStarted(S_HB)==false
  V_HeroDolls
  CS_PlayCutscene       empty 00F01760; ScriptName==null
user.ini ActivateQuest("Gameflow")
  00CE75B0  Main watcher. HasStarted(S_GF)==false
004189C2  first type-1 00CB8220
  Sunnyvale 00CDD360 yield          // vtbl+28, not WaitFor
  Gameflow  00CE7670 state 0
            00893610 Q_NewOakValeIntro miss
  WaitForCamera / WaitForMessageCamera
  / WaitForAnimationEvent / WaitForUnderRadius
  / 00CCA41F / 00CCFF91 / 00CC41FC / 00CC4045
    not on this walk
```

`00CE18BB` `CS_STANDING_STONE` and `00CEE9F1`
`CS_FABLE_CREDITS` are the lowest-VA `E8 00CBFB7D`
sites in `00CE*`. They are endgame / credits.
**DISPROVEN** as first after Leave (`cutscene-first`).

---

## 1. Native command table (no WaitUntil)

Authority: ASCII `0x012C1500`–`0x012C2C00`
(`script-bank/exe-commands.md`).
`ScriptCommandMap.NativeTokens` filters that slice.

| Token VA | String | Family | Dispatch / apply |
|---|---|---|---|
| `0x012C1A28` | `WaitForMessageCamera` | Global | `00CCFF91` / `00CD0006` |
| `0x012C1DBC` | `WaitForCamera` | Global | `00CCA41F` / `00CCA58F` |
| `0x012C2320` | `.WaitForAnimationEvent` | Entity | `00CC41FC` / `00CC4252` |
| `0x012C2338` | `.WaitForUnderRadius` | Entity | `00CC4045` / `00CC409B` |

No `WaitUntil`, `WaitFor`, `WaitUntilFlag`, or
`WaitUntilCamera` string in that slice. Grep of
`WaitUntil` across `src/`, `docs/`, `proofs/`,
`script-bank/`, and `text-map/listing-*.txt` is
empty. **PROVEN** absent.

Near-miss wait tokens that are **not** `WaitFor*`:

| Token | Why not this packet |
|---|---|
| `WaitFlag` | flag map poll (`script-flags-first`) |
| `WaitTask` | entity task slot; first *compared* loop token if entered |
| `WaitPlayAnimation` | anim complete, not `WaitForAnimationEvent` |
| `WaitActiveDialog` / `WaitBossFight` | other polls |
| `GamePause` / `DoScriptFrame` | scaled-frame waits |

The runner is a long `00BFEAF8` if-chain, not a jump
table of those names. `WaitForCamera` sits after
`NoLoadUseCamera` / `UseCamera` / `PlayAVI` and
**before** `SetFlag` (`listing-00cc0000` `00CCA41E` →
`00CCA474`). That order is **PROVEN** as strcmp
sequence. It is **not** an execution order after Leave.

---

## 2. Native `WaitFor*` apply (if the runner were entered)

`WaitForCamera` apply `00CCA58F` (**PROVEN**,
`listing-00cc0000`):

```
00CCA58F  ecx = [0x143E8F8]
          call [eax+1672]          // camera busy?
          test al, al
          jne 00CCA52F             // leftover re-poll
          jmp 00CD17FD             // idle → continue
00CCA52F  vtbl+40 skip?
          00CBEB7E skip?
          [ebp+103] → vtbl+28
          00CBF7FE; inc [0x13B83C8]
          fiber [+5]==0 → re-poll else 00CD17FD
```

Snap `UseCamera` leaves `Playing=false`, so
`vtbl+1672` is idle and the line continues. Host
`GlobalDispatcher` / `ScriptedCamera.WaitForCamera`
matches that: leftover-poll only while
`BeginTransition` (path / rig / rotate). **PROVEN**
as leftover behaviour. **DISPROVEN** as Leave.

`WaitForMessageCamera` apply `00CCFF91`: arg0
required else `00CD17FD`; leftover `00CCFFB2`
polls `vtbl+2316(name)`. Distinct from
`vtbl+1672`. **PROVEN** opcode. No extracted
first-scene def uses it.

`.WaitForAnimationEvent` apply `00CC4252`: `ebx`
+ arg0 required else `00CC7081`; `00CBEB7E` skip;
actor `vtbl+48`; leftover poll `004AAF60` →
`vtbl+236`. Not `WaitPlayAnimation`. **PROVEN**
opcode. No extracted leftover intro line.

`.WaitForUnderRadius` apply `00CC409B`: `ebx` +
arg0 + arg1 required; `atof` radius; leftover
`00CC40CE` / `00CBE2FF` `dist^2 < r^2`. **PROVEN**
opcode. No extracted leftover intro line.

---

## 3. Why WaitFor cannot be first after Leave

| Claim | Class | Evidence |
|---|---|---|
| `00CBFB7D` on Leave / Init Game / first pumps | **DISPROVEN** | `script-command-map` §3; `HasStarted(S_PSM/S_GF/S_HB)==false`; `CS_PlayCutscene` empty |
| First type-1 is a `WaitFor*` | **DISPROVEN** | Sunnyvale `00CDD360` / Gameflow `00893610` |
| Gameflow wait *is* `WaitForCamera` | **DISPROVEN** | `00CE78C7` quest-name lookup, no runner |
| First 3D camera after Leave is `WaitForCamera` | **DISPROVEN** | WorldCamera `006B4900` / seed `006B3FF0`; `FirstSeenCallsUseCamera=false` |
| Startup / Leave PlayAVI `WaitForRenderTime` is a script verb | **DISPROVEN** | `00CA65B0` is quartz pacing before Leave / not `00CBFB7D` |
| Father leftover first line is `WaitFor*` | **DISPROVEN** | `PlayMusic MUSIC_SET_NULL`; later `Hero.WaitTask FOO` / `WaitActiveDialog` |

---

## 4. First leftover `WaitFor*` (not Leave)

Extracted full `script-bank` dumps contain **one**
`WaitFor*` site on the leftover Oakvale intro chain:

`CS_OAKVALEINTRO_HESDEADJIM` (`0485`, newgame
**True**), late in the def:

```
CreateEffect MAZE_HERO_TELEPORT,MK_OVID_DAD
WaitForCamera
UseCamera CAM_OVID_SHOT12A
```

`0481` father / `0482`–`0484` Theresa have
`WaitTask` / `WaitActiveDialog` /
`WaitPlayAnimation` only. **PROVEN** leftover
order. **DISPROVEN** as Leave (father itself is
already leftover `00DABAC0` → `NOVI_LiveFather`;
hesdeadjim is later still).

`entries-tsv.md` first-commands column (truncated)
lists earlier bank-index `WaitForCamera` in
`CS_PUNCHCLUB_BC_ROUNDNEW` (46), teleporter /
`CS_OPENGRAVE_CRYPTCAM`, later punchclub.
Those rows are newgame **0**. **PARTIAL** as
“first in TSV file order.” **DISPROVEN** as
first after Leave and as first leftover intro.

`WaitForMessageCamera` / `.WaitForAnimationEvent`
/ `.WaitForUnderRadius` do not appear in the
extracted leftover intro dumps. Live `script.bin`
scan for those verbs is what
`WaitForCamera_real_script_bank_line` /
`WaitFor*_real_script_bank_or_isolated` do; they
fall back to isolated lines. **PARTIAL** as
bank-wide first. **DISPROVEN** as Leave.

Do not invent a first-seen `WaitFor*` by grepping
`script.bin` ahead of a started `CCutsceneDef`.

---

## 5. C# vs native on this path

| Host | Native after Leave | Class |
|---|---|---|
| `GlobalDispatcher` `WaitForCamera` | unused | **LEFTOVER** |
| `WaitForMessageCamera` | unused | **LEFTOVER** |
| `EntityDispatcher` `WaitForAnimationEvent` | unused | **LEFTOVER** |
| `WaitForUnderRadius` | unused | **LEFTOVER** |
| `ScriptedCamera.WaitForCamera` / `Playing` | WorldCamera construct only | **DISPROVEN** pairing |
| `RunUntilYield` / `PumpUntilSettled` | host pump names, not a verb | **DISPROVEN** as `WaitUntil` |
| Gameflow `00893610` as `WaitFor*` | fiber yield | **DISPROVEN** |

Host `EngineLifecycle` after `DispatchFrontendMessage(15)`
does not enter `ScriptInterpreter`. Same as
`script-global-cmds` / `script-interpreter`.

---

## Classifications (short)

1. **`WaitUntil` as a script command — DISPROVEN.** Token
   is absent from the exe slice.
2. **First `WaitFor*` after Leave — none. PROVEN.**
3. **Gameflow / Sunnyvale wait as `WaitFor*` — DISPROVEN.**
4. **First leftover `WaitFor*` — `CS_OAKVALEINTRO_HESDEADJIM`
   `WaitForCamera`. PROVEN leftover; DISPROVEN as Leave.**
5. **Father / `WaitTask` / `WaitActiveDialog` as that first
   leftover — DISPROVEN** (wrong verb family).
6. **Host `WaitFor*` after New Game click — LEFTOVER.**
