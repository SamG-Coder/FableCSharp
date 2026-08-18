# Audit: `ScriptInterpreter.cs` vs native `00CBFB7D` dump

Investigation only. No production `src/` edits.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** / **LEFTOVER** / **DIVERGE** / **INVENTED**.

This file answers three questions against the exact dump, not
`proofs/script-interpreter/README.md` (that packet’s “first token
is `.WaitTask`” claim is **wrong** — see §3).

Sources: `src/Fable.Game/ScriptInterpreter.cs`,
`Scripting/ScriptLine.cs`, `GlobalDispatcher.cs`, `EntityDispatcher.cs`,
`ScriptArguments.cs`, `ScriptBank.cs`;
`tools/Fable.ExeIndex/out/01-sections/script-runtime/`
(`cutscene-runner-exact-00cbfb7d`, `command-loop-index-00cc0205`,
`command-loop-continue-00cd17fd`, `gamepause-token-00cc88d1`,
`doscriptframe-token-00cc7085`, `fadeout-opcode-exact-00cd0987`,
`cstring-atoi-0099e7f0`, `gamepause-atof-0099e690`,
`ccutscenedef-persist-00f2a1d0`, `cstring-vector-read-00433273`);
`text-map/listing-00cc0000.txt` `00CC0122`–`00CC083D`;
`script-bank/exe-commands.md`, `0481-cs-oakvale-intro-father.md`.

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| Invented decode? | **No bytecode ISA.** Host walks persist CStrings. Three real decode **DIVERGE**s (exact `Eq`, `"` quotes, unbounded args). `Get` as an exe token is already **INVENTED** on the map, not in this loop. | **PROVEN** parse model; **DIVERGE** details |
| Wrong operand size? | First-seen leftover sizes match (`u32` count, `sar 2` = 4-byte CString, `atoi`/`atof` → host `int`/`float`, `*15`). **Do not** treat the 24-byte quest VM as this runner. Empty `GamePause` length-0 skip is a leftover **DIVERGE**. | **PROVEN** first-seen; **DIVERGE** empty GamePause |
| First-seen opcode mismatch? | After Leave: **no** `00CBFB7D` opcode. First *compared* verb if entered: **`.SummonerAttack`** `00CC04FA`, not `.WaitTask`. First leftover *executed* line: `PlayMusic MUSIC_SET_NULL`. Dump titled `00CBFB7D` that starts at `00CBFACA` is a walk misalign. | **PROVEN** |

---

## 1. Invented decode?

Native is not an opcode stream. `00CBFB7D` copies `CCutsceneDef+60`
(or `+108` when `[ebp+120]==1`) with `00432EE9`, then walks
`[ebp-72]` as a dword PC over a CString vector.

`listing-00cc0000.txt` `00CC0205`: `0099E5A0(46)` last `'.'`, then
`0099E5A0(32)` first space. Args: only quote `0x27` (`'`), comma
`0x2C`, space `0x20` inside quotes kept. Slots are CStrings at
`[ebp+40]`, stride **4**.

Host `ScriptLine.Parse` is that model. The loop does **not** consult
`ScriptCommand.Classify`. **PROVEN.**

| Decode | Native | Host | Class |
|---|---|---|---|
| Bytecode / packed opcode | none | none | **PROVEN** not invented |
| Line = persist CString | `00432EE9` / `004331F9` | `Commands[Pc]` | **PROVEN** |
| Actor.verb | last `'.'` | `LastIndexOf('.')` | **PROVEN** |
| Verb match | `00BFEAF8` = `strnicmp(verb, token, n)` `n=00403A00` | Dispatcher `Eq` = **exact** ignore-case. `TokenMatches` exists but verbs do not use it | **DIVERGE** |
| `RemoveThing` / `PlayCombatAnim` | prefix `Remove` / token `.PlayCombatAnim` | extra exact aliases | **LEFTOVER** alias (not a 2nd opcode) |
| Quotes | `0x27` only (`00CC0334`) | `'` **and** `"` | **INVENTED** `"` |
| Arg cap | init 10 (`push 10`), split `cmp …,11`, subst `0xB` walks | unbounded `List<string>` | **DIVERGE** (not first-seen) |
| `$` slots | arg walk `00CC03B5` + `009EF360` when `[ebp+116]!=0` | `ScriptArguments` `ARG1`–`4` `ANIM` `LOOP` `LINE` `CAMERA` | **PARTIAL** (`$ARG*` proven as idea; named extras **UNREAD**) |
| `ExtractCommands` ASCII scrape | not what the runner walks | marked discovery-only | **DISPROVEN** as decode |
| `Get` in `ScriptCommandMap.All` | no site in `0x012C1500`–`0x012C2C00` | map row TokenSite=`0` | **INVENTED** as exe token; unused by this fetch |

Unknown verb: native falls out of the `00BFEAF8` chain (**UNREAD** as
Leave). Host `CommandResult.Blocked("UNKNOWN")` — do not invent
Continue.

`IScriptHost` / leftover `ScriptCommand.Parse*` / `ScriptCommand.SplitArgs`
(comma-split, no quotes) are **not** the fetch path. Loop uses
`ScriptLine` then the dispatchers.

---

## 2. Wrong operand size?

| Operand | Native size | Host | First-seen leftover | Class |
|---|---|---|---|---|
| Persist skip + count | `00433273` reads **4** (`cmp edx,4` / `movsd`) after `00404500` mode-2 skip | `cursor += 4`; `ToInt32` count | father 2017-byte def | **PROVEN** |
| Vector element | `(end-begin) sar 2` (`00CC020D`, `00CD1865`) | `IReadOnlyList<string>` | n/a | **PROVEN** (4-byte CString, not 24) |
| Quest VM record | 24 bytes (`00CB6EA0`) | **not this class** | Leave uses that walk | **DISPROVEN** as interpreter opcode |
| PC `[ebp-72]` | dword index | `int Pc` | 0 | **PROVEN** |
| `DoScriptFrame` | `0099E7F0` atoi; empty → `esi=1`; `esi<=0` skip | `ParseScriptFrame` same digit/`-`/`.` rules; default 1 | `DoScriptFrame 1` / `2` / `4` | **PROVEN** |
| `GamePause` seconds | `0099E690` atof → `fstp [ebp+124]` (float); `* [0x124E640]=15`; add `[0x122DED8]=1` | `float` × `GamePauseScale=15` + `Increment=1` | `GamePause 1.6` | **PROVEN** |
| `GamePause` empty | `00403A00==0` → `je 00CD17FD` (no wait) | `TryFloat("")` → `WaitScaledFrames` target 0 | not on father list | **DIVERGE** leftover |
| `GamePause clock` | strcmp `"clock"` after atof | unused | `FirstSeenGamePauseHasClockArg=false` | **UNREAD** / **DISPROVEN** first-seen |
| `FadeOut` defaults | `fld [0x122F59C]=0.5`; second atof; black `(0,0,0,255)` | `FadeSpecialCaseSeconds=0.5`, param 0 | line `[1]` `FadeOut 0.5,0` | **PROVEN** |
| Head special-case | full-line `00BFEBA8("FadeOut 0.5,0")` then `vtbl+1488` | `TryFadeSpecialCase` exact `Commands[0]` | first line is `PlayMusic` so skip | **PROVEN** leftover |
| Frame `0x18DC` | `mov eax, 0x18DC` at `00CBFB7E` | no host twin | n/a | dump **PROVEN** 32-bit imm |

`TickWait` passes `""` into `TickScriptFrame` / `TickGamePause`.
That is safe on the live path because `GlobalDispatcher` already
wrote `ScriptFrameRemaining` / `GamePausePhase=1`. Re-parse of
empty would be a size/control bug **only** if those slots were 0.

`int.TryParse` on `"1.0"` would be the wrong atoi. The host custom
atoi (stop at `.`) matches `0099E7F0` (`cmp bl, 0x2E` → break).

---

## 3. First-seen opcode mismatch?

Three different “first opcodes.” Do not collapse them.

### A. After Leave frontend (`0042F2A2`)

**None in `00CBFB7D`.** **PROVEN.**

Init Quests / Gameflow / fibers are the other VM
(`00CB6EA0` 24-byte records, `00A44880`). `CS_PlayCutscene`
`00F01760` is empty. See `proofs/newgame-script` /
`proofs/script-command-map` §3.

### B. Dump titled “runner `00CBFB7D`”

`cutscene-runner-00cbfb7d-00cbfb7d.md` is a **walk** from
**`00CBFACA`** (PlayAnimation comma splitter). `leave` at
`00CBFB7B` is that function’s x86 epilogue.

`cutscene-runner-exact-00cbfb7d-00cbfb7d.md` is the real
prologue (`push ebp` / `mov eax, 0x18DC`). That is **not**
an invented 16-bit decode; the 32-bit immediates in the
exact packet match `listing-00cc0000.txt`.

### C. First `00BFEAF8` *inside* the line loop

`proofs/script-interpreter` said first recovered token is
`.WaitTask` `00CC0783`. **DISPROVEN** by the listing.

After parse / optional `$` walk (`00CC0410` vs imm
`0x12C2620`, which is **not** a verb — it sits between
`.SummonerAttack` `0x012C2610` and `HeroFollower0`
`0x012C2624`):

| Order | VA | Token | Host |
|---:|---|---|---|
| 1 | `00CC04FA` | `.SummonerAttack` | **UNREAD** (no `Eq`) → would **Block** if hit |
| 2 | `00CC0616` | `.DoBossFight` | **UNREAD** → Block |
| 3 | `00CC0783` | `.WaitTask` | `EntityDispatcher` later `Eq` |
| 4 | `00CC083D` | `.WalkTo` | later `Eq` |
| … | `00CC8EAC` | `PlayMusic` | **first** `GlobalDispatcher.Eq` |

First *compared* ≠ first *executed*. A `PlayMusic` line misses
1–3 and hits `00CC8EAC` → `jmp 00CD17FD`.

Host compare order is **DIVERGE** (`PlayMusic` / `Teleport`
first). Exact `Eq` plus leftover aliases keep first-seen
father verbs on the same handlers. Prefix collisions that
native longer-first order would win are **UNREAD** here.

### D. First leftover *executed* line (not Leave)

`CS_OAKVALE_INTRO_FATHER[0]` = `PlayMusic MUSIC_SET_NULL`.
`FirstSeenFadeSpecialCaseRuns=false`. Next line `FadeOut 0.5,0`
is the in-loop opcode `00CD0987`, not the head special-case.

`FirstSeenStartsIntroCutscene=true` is that leftover pairing,
**not** “first opcode after Leave.”

---

## Classifications (short)

1. **Invented bytecode decode — DISPROVEN.** CString fetch is **PROVEN**.
2. **`"` quote + exact `Eq` + unbounded args — INVENTED / DIVERGE** vs dump.
3. **Operand widths on first leftover father lines — PROVEN.** Empty `GamePause` skip is the leftover size/control **DIVERGE**.
4. **24-byte quest records as this interpreter — DISPROVEN.**
5. **First opcode after Leave — none. PROVEN.**
6. **First compared loop token — `.SummonerAttack` `00CC04FA`. PROVEN.** `.WaitTask`-first is **DISPROVEN**.
7. **First leftover execute — `PlayMusic MUSIC_SET_NULL`. PROVEN.**
8. **Walk dump `00CBFACA` labeled `00CBFB7D` — dump misalign, not a host opcode.**
