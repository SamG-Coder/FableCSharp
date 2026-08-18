# First If / Flag / SetFlag script command after Leave

Investigation only. No production `src` edits.

Do **not** start at Oakvale / `S_QNOVI` / `CS_OAKVALE_INTRO_FATHER` /
`CS_OAKVALE_REVISITED` / `SetFlag fire`. Those are later leftover
`CCutsceneDef` lines, not Leave / Init Game / first no-save type-1.

Do **not** treat Gameflow `OV_INTRO` / `008AE660` as `SetFlag`.
That write is script-state `[0x13BAE44]`, not `FlagStore`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: `src/Fable.Game/Scripting/FlagStore.cs`,
`GlobalDispatcher.ApplySetFlag` / `ApplyWaitFlag`,
`ScriptCommandMap` (`NativeTokens`, `All`),
`QuestFactoryTable`, `EngineLifecycle`;
`proofs/flag-persist-stores/README.md`,
`proofs/script-command-map/README.md`,
`proofs/script-interpreter/README.md`,
`proofs/newgame-script/README.md`,
`proofs/fiber-yield-first/README.md`,
`proofs/cutscene-first/README.md`;
`docs/runtime/COMMAND_MAP.generated.md`;
ExeIndex `script-bank/exe-commands.md`,
`script-bank/0481-cs-oakvale-intro-father.md`,
`script-bank/0496-cs-oakvale-revisited.md`,
`script-runtime/calls-cutscene-runner-00cbfb7d`,
`script-runtime/00db86b0-calls-runner-00db88db`,
`text-map/listing-00cc0000.txt` / `listing-00880000.txt` /
`listing-01200000.txt`.

---

## Verdict

**After Leave there is no `If` / `Flag` / `SetFlag` / `WaitFlag`
script command.**

The native verb table has **no** `If`, `IfFlag`, or `Flag` token.
The only flag *commands* are `WaitFlag` then `SetFlag`. Both live
in runner `00CBFB7D` and need `[ebp+112]` (first stack arg) as the
`008ADF10` map. That runner is **not** on the no-save Leave tree.

| Question | Answer | Class |
|---|---|---|
| First `SetFlag` / `WaitFlag` after Leave? | **none** — `00CBFB7D` not entered | **PROVEN** |
| First `If` / `IfFlag` / `Flag` verb? | tokens do not exist | **DISPROVEN** as commands |
| First leftover interpreter line? | `PlayMusic MUSIC_SET_NULL` (father) | **LEFTOVER** (no flag verb) |
| First leftover `SetFlag` in extracted banks? | `CS_OAKVALE_REVISITED` `SetFlag fire,true` | **LEFTOVER** |
| First named write that *looks* like a flag? | Gameflow `OV_INTRO` at `[0x13BAE44]` | **DISPROVEN** as `FlagStore` / SetFlag |
| First `[0x13BAE2C]` named byte after Leave? | **UNREAD** | still open |

---

## Timeline (no-save New Game)

```
0121F0E0  CRT  alloc 24 → [0x13BAE2C]   // FlagStore map, empty
0121F120         alloc 24 → [0x13BAE38]   // gossip sibling
0121F160         alloc 32 → [0x13BAE44]   // script-state map
0042EC7C  frontend. No 00CBFB7D. No 008ADF10.
0042F2A2  Leave frontend
004184BD  Init Game → FinalAlbion.wld
004B4260  START_INITIAL_QUESTS
  Q_SunnyvaleMaster     00CDBA10 zeros  // PersistStore, not flags
  PersonalScriptMain    HasStarted(S_PSM)==false
  PersonalScript_GlobalThings
  HeroBoasts            HasStarted(S_HB)==false
  V_HeroDolls
  CS_PlayCutscene       empty 00F01760; ScriptName==null
user.ini ActivateQuest("Gameflow")
  00CE6CF0  vtbl+2868 → 008AE660 [0x13BAE44]  OV_INTRO…
  00CE75B0  Main watcher. HasStarted(S_GF)==false
004189C2  first type-1 00CB8220
  Sunnyvale 00CDD360 yield
  Gameflow  00CE7670 state 0 → 00893610 Q_NewOakValeIntro miss
  SetFlag / WaitFlag / 00CCA4C8 / 00CCB893  not on this walk
```

`00CE18BB` `CS_STANDING_STONE` and `00CEE9F1` `CS_FABLE_CREDITS`
are the lowest-VA `E8 00CBFB7D` sites in `00CE*`. They are
endgame / credits. **DISPROVEN** as first after Leave
(`cutscene-first`).

---

## 1. Native command table (not an If table)

Authority: ASCII `0x012C1500`–`0x012C2C00`
(`script-bank/exe-commands.md`). `ScriptCommandMap.NativeTokens`
filters that slice.

| Token VA | String | In `NativeTokens`? | Dispatch / apply |
|---|---|---|---|
| `0x012C1D70` | `WaitFlag` | yes | `00CCB840` / `00CCB893` |
| `0x012C1D7C` | `CameraFOVLookBetweenPos` | yes | (between the two flag verbs) |
| `0x012C1D94` | `CameraPath` | yes | |
| `0x012C1DA0` | `CameraRotateThing` | yes | |
| `0x012C1DB4` | `SetFlag` | yes | `00CCA475` / `00CCA4C8` |

No `If`, `IfFlag`, `CheckFlag`, or bare `Flag` string in that
slice. Grep of `G("If…")` in `ScriptCommandMap.cs` is empty.

The runner is a long `00BFEAF8` if-chain, not a jump table of
those names. First *loop* token the chain tests (if entered) is
`.WaitTask` `00CC0783` (`script-interpreter`). `SetFlag` sits
after `WaitForCamera`. `WaitFlag` sits later, before
`CreateLight`. That order is **PROVEN** as strcmp sequence.
It is **not** an execution order after Leave.

---

## 2. Native flag table vs `FlagStore.cs`

`FlagStore` is the host analog of helper `008ADF10`:

```
ecx = map
007ACBB0(name) lookup
miss / name mismatch → 008ACA90 insert, default byte 0
return esi+20
```

| Object | VA | C# | Class |
|---|---|---|---|
| Global named-byte map | `[0x13BAE2C]` CRT `0121F0E0` | `FlagStore` | **PROVEN** |
| Gossip sibling | `[0x13BAE38]` `0121F120` | not in `FlagStore` | **PROVEN** map |
| Script-state map | `[0x13BAE44]` insert `008AE660` | not in `FlagStore` | **PROVEN** |
| Runner arg | `[ebp+112]` = first stack arg of `00CBFB7D` | `ctx.Flags` (always non-null) | **PROVEN** slot. **DIVERGE** null |

SetFlag apply `00CCA4C8` (**PROVEN**, `listing-00cc0000`):

```
arg0 + arg1 + [ebp+112] required else 00CD17FD
IsTrue(arg2) && [ebp-39]!=0 → jmp 00CC907D   // skip rewrite
[ebp-39]=1
IsFalse(arg1) → 008ADF10; mov [eax],0
else            008ADF10; mov [eax],1
jmp 00CC907D                                 // YieldOnce
```

WaitFlag apply `00CCB893` (**PROVEN**):

```
arg0 + arg1 + [ebp+112] required else 00CD17FD
IsTrue(arg1) → expected=1 else 0
008ADF10; cmp [eax],bl
match → 00CD17FD
else leftover 00CCB8CE (skip / vtbl+28 / re-poll)
```

Wrappers `008A96C0` / `008AE060` hardcode `ecx=0x13BAE2C`.
0 `E8` (vtbl). First caller after Leave **UNREAD**.

C++ `vtbl+2592` (`NewGameScript.WaitFlagVtbl`) is **not** the
script command. `00DBE12E` does `lea eax,[esi+76]; push 1;
call [edx+2592]` on leftover `00DBDE40` (`Q_NewOakValeIntro_PreAttack`).
**DISPROVEN** as Leave. Offset 76 is a *per-object* `008ADF10`
map (`OPENCAGE` at `00D39027` uses the same `+76` pattern).

---

## 3. Why SetFlag cannot be first after Leave

| Claim | Class | Evidence |
|---|---|---|
| `00CBFB7D` on Leave / Init Game / first pumps | **DISPROVEN** | `script-command-map` §3; `HasStarted(S_PSM/S_GF/S_HB)==false`; `CS_PlayCutscene` empty |
| `call 008ADF10` in `listing-004*` / `listing-005*` | **DISPROVEN** | zero sites |
| Gameflow seed is SetFlag | **DISPROVEN** | `00CE6CF0` → `[esi+64].vtbl+2868` → `008AE660` `[0x13BAE44]` |
| First type-1 is a flag wait | **DISPROVEN** | Sunnyvale `00CDD360` / Gameflow `00893610` quest lookup |
| Lowest-VA `00CBFB7D` (`00CE18BB` `CS_STANDING_STONE`) | **DISPROVEN** as this site | `cutscene-first`; `Q_MinionCamp` / `EndGameFocalSite` |
| Father leftover has SetFlag/WaitFlag | **DISPROVEN** | `0481-cs-oakvale-intro-father.md` strings start `PlayMusic` … no flag verb |

Father call `00DB88F8` (**PROVEN** leftover):

```
xor ebp, ebp
push 1
push ebp
push ebp
push ebp          ; [ebp+112] = 0
call 00CBFB7D
```

Same first-arg 0 at `00CE18BB` and `00F0168A` (`ecx=0x143E91C`).
With `[ebp+112]==0`, SetFlag/WaitFlag take `00CD17FD` and write
nothing. Host `ApplySetFlag` always uses `ctx.Flags` (never
null). **DIVERGE** if a later runner is invoked the same way.

---

## 4. First leftover SetFlag (not Leave)

Extracted `script-bank` dumps contain **one** `SetFlag` site:

`CS_OAKVALE_REVISITED` (`0496-cs-oakvale-revisited.md`):

1. `SetFlag fire,true`  (after flashback `Girl.Speak` …)
2. `SetFlag fire,false` (after `EnableSounds false`)

No `WaitFlag` in those dumps. Bank-order first `SetFlag` /
`WaitFlag` in live `script.bin` is what
`SetFlag_WaitFlag_real_script_bank_lines` scans; the test
falls back to `CS_OAKVALE_REVISITED` / `SetFlag fire,true`.
**PARTIAL** as “first in bank file order.” **DISPROVEN** as
first after Leave (revisited is post-intro Oakvale).

Do not invent a first-seen flag name by grepping `script.bin`
ahead of a started `CCutsceneDef`.

---

## 5. C# vs native on this path

| Host | Native after Leave | Class |
|---|---|---|
| `new FlagStore()` at `ScriptRuntime` ctor | CRT map already empty | **EQUIVALENT** timing (host Runtime is after Leave) |
| `Flags.Set` from SetFlag | unused | **EQUIVALENT** (nothing to write) |
| Always-non-null `ctx.Flags` | `[ebp+112]` often 0 → skip | **DIVERGE** on leftover runners that pass 0 |
| Gameflow `OV_INTRO` in `FlagStore` | `[0x13BAE44]` | **DISPROVEN** pairing |
| `WaitFlag` as first type-1 wait | Gameflow `00893610` | **DISPROVEN** |

---

## Classifications (short)

1. **First If / Flag / SetFlag / WaitFlag after Leave — DISPROVEN (none).**
   No such `If*` token. Runner not entered. Gameflow seed is
   `[0x13BAE44]`.
2. **Native flag commands — PROVEN as table + apply, not first-seen.**
   `WaitFlag` `0x012C1D70` / `00CCB893`. `SetFlag` `0x012C1DB4` /
   `00CCA4C8`. Map helper `008ADF10`. Table arg `[ebp+112]`.
3. **`FlagStore` `[0x13BAE2C]` first named write after Leave — UNREAD.**
   Construct **PROVEN**. Wrappers 0 `E8`.
4. **Leftover first `SetFlag` line — `CS_OAKVALE_REVISITED` `SetFlag fire,true`.**
   Father has none. Do not run it from Leave.
5. **Host `Flags.Set` on first no-save pumps — EQUIVALENT unused.**
   Null-table skip is **DIVERGE** vs later `00CBFB7D` sites that
   push 0.
