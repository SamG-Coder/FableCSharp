# Intro `PlayCombatAnim` (`CS_OAKVALE_INTRO_FATHER`)

Investigation only. No production `src` edits.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER**.

Sources: `assembly/compiled-defs/script/0481-CS_OAKVALE_INTRO_FATHER.md`,
`tools/Fable.ExeIndex/out/01-sections/script-bank/0481-cs-oakvale-intro-father.md`,
`script-bank/exe-commands.md`,
`assembly/exe/00-index/strings.tsv` (`0x012C2540` `.PlayCombatAnim`),
listing `text-map/listing-00cc0000.txt` (`00CC15E3`–`00CC1879`,
`00CC5691`–`00CC569F`),
ExeIndex `script-runtime/playcombatanim-token-00cc15e3-00cc15e3.md`,
`playcombatanim-apply-00cc16fd-00cc16fd.md`,
`playcombatanim-father-vtbl76-00834760-00834760.md`,
`playcombatanim-player-vtbl76-006ad9d0-006ad9d0.md`,
`playanimation-yield-join-00cc186f-00cc186f.md`,
`playanimation-yield-once-00cc5691-00cc5691.md`,
`docs/runtime/COMMAND_MAP.md`, `COMMAND_COVERAGE.md`,
`FIXTURE_COMMAND_AUDIT.md`,
`src/Fable.Game/RegionTravel.cs`, `ScriptCommandMap.cs`,
`ScriptRuntime.cs`, `ScriptInterpreter.cs`,
`Scripting/EntityDispatcher.cs`, `Scripting/ExecutionContext.cs`,
`WorldSceneTests` (`Father_PlayCombatAnimation_yields_once_without_pose`).

---

## Verdict

**Name = `TURNING_AC90`.** Persist spelling is
`Father.PlayCombatAnimation TURNING_AC90,FALSE,TRUE` (vector 0 of
`CS_OAKVALE_INTRO_FATHER`). Exe token is `.PlayCombatAnim`.

**Opcode site = `00CC15E3`** inside runner `00CBFB7D`. Apply is
`00CC16FD` `call [eax+76]`. Yield is `00CC186F` → `00CC5691`
`call [eax+28]`.

**Host gap = pose / mesh.** `ScriptRuntime` parse, dispatch, and
one-yield return are **PROVEN**. Apply and runtime are **PARTIAL**:
record `CombatAnimations` + `EntityTaskKind.CombatAnimate`, do not
play `TURNING_AC90`. Native Father `vtbl+76` `00834760` also does
not read the name pointer. `FirstSeenPlayCombatAnimationAppliesPose=false`.

---

## 1. Cutscene def string

`CCutsceneDef` index **481**, `this+60` (vector 0). Dump:

`assembly/compiled-defs/script/0481-CS_OAKVALE_INTRO_FATHER.md`

Raw at `0x03A0`:

```
Father.PlayCombatAnimation TURNING_AC90,FALSE,TRUE
```

Hex: `46 61 74 68 65 72 2E 50 6C 61 79 43 6F 6D 62 61 74 41 6E 69 6D 61 74 69 6F 6E 20 54 55 52 4E 49 4E 47 5F 41 43 39 30 2C 46 41 4C 53 45 2C 54 52 55 45 00`

Neighbours: `Hero.SneakTo MK_OVIF_HERO4…` then `GamePause 1.0` then
this line then `GamePause 1.0` then `Hero.PlayAnimation CS_LOOK_LEFT,TRUE`.

There is **no** `PlayCombatAnim` spelling in this def. **PROVEN.**
`WorldSceneTests` asserts `intro.ExecutedVerb("PlayCombatAnim")` is
false and `Father.PlayCombatAnimation TURNING_AC90,FALSE,TRUE` is
present.

`RegionTravel.IntroFatherCombatAnim = "TURNING_AC90"`. **PROVEN.**

---

## 2. Opcode site (`00CBFB7D` → `00CC15E3`)

Runner `00CBFB7D` walks def+60 CStrings. Entity join is
`00CC707C`. Token table string:

| VA | String |
|---|---|
| `0x012C2540` | `.PlayCombatAnim` |

Handler in `listing-00cc0000.txt`:

```
00CC15E3  push ".PlayCombatAnim"
00CC15E8  lea ecx, [ebp-344]
00CC15EE  call 0099EBF0
…
00CC1614  call 00BFEAF8          ; match verb vs token
00CC1634  je 00CC1730            ; miss → .PlayLoopingAnim
00CC163A  test ebx, ebx
00CC163C  je 00CC7081            ; empty actor
00CC1642  lea ecx, [ebp+40]
00CC1645  call 00403A00          ; name CString
00CC164C  je 00CC7081            ; empty name
```

`PlayCombatAnimation` is **not** a second token. Same site
`00CC15E3` (`ScriptCommandMap` alias). **PROVEN.**

`RegionTravel.PlayCombatAnimationOpcode = 0x00CC15E3`.

---

## 3. Args / apply / yield

Defaults then `00CBEDBA` / `00CBEE0C`:

| Slot | Loc | Intro `TURNING_AC90,FALSE,TRUE` |
|---|---|---|
| name | `[ebp+40]` | `TURNING_AC90` |
| arg1 IsTrue | discarded | `FALSE` |
| FlagB | `[ebp-1776]` | arg2 `TRUE` → 1 |
| FlagC | `[ebp-1768]` | missing → 0 |
| FlagD | `[ebp-1760]` default 1 | missing → 1 |
| FlagA | `[ebp-1784]` default 1 | missing → 1 |
| count | `esi` (`0099E7F0`) | missing → 1 |
| FlagE | `[ebp-1752]` | missing → 0 |

Apply loop:

```
00CC16FD  push [ebp-1752]        ; FlagE
          push 0
          push [ebp-1760]        ; FlagD
          lea ecx, [ebp+40]      ; name
          push [ebp-1768]        ; FlagC
          push [ebp-1776]        ; FlagB
          push [ebp-1784]        ; FlagA
          push ecx
          mov ecx, ebx
00CC1725  call [eax+76]          ; thing vtbl+76
00CC1728  dec esi
00CC1729  jne 00CC16FD
00CC172B  jmp 00CC186F
00CC186F  cmp [ebp-22], 0x00
00CC1873  je 00CC7081
00CC1879  jmp 00CC5691
00CC5691  cmp [ebp+103], 0x00
00CC5697  mov ecx, [0x143E8F8]
00CC569F  call [eax+28]          ; one context yield
```

Father `vtbl+76` = `00834760`. Player = `006AD9D0`. First 80
insns of `00834760` use `ecx=this` (`esi`), `[0x13B86D5]`,
`this+348/349`, position helpers. **No load of the pushed name.**
**PROVEN** leftover vs pose.

`CActionPlayCombatAnimation` `009035F0` is a name-setter
(`push "CActionPlayCombatAnimation"` + `0099EBF0`), **DISPROVEN**
as this apply.

---

## 4. Host `ScriptRuntime` coverage

| Dim | Status | Host |
|---|---|---|
| Parse | **PROVEN** | `IsPlayCombatAnimation` accepts both spellings; `ParsePlayCombatAnimation` |
| Dispatch | **PROVEN** | `EntityDispatcher` `PlayCombatAnimation` / `PlayCombatAnim` |
| Return | **PROVEN** | `YieldOnce` `"PlayCombatAnim vtbl+28"` (`YieldAfter`) |
| Apply | **PARTIAL** | `Animation.PlayCombat` records `ScriptCombatAnimation` + `AnimationState` + `CombatAnimate` task. No XSEQ / mesh |
| Runtime | **PARTIAL** | `IScriptHost.PlayCombatAnimation` → same record. Comment: “no TURNING_AC90 pose” |
| Overall | **PARTIAL** | `CommandParity.ScriptLayer` |

`COMMAND_COVERAGE.md`:

| Token | Parse | Dispatch | Return | Apply | Runtime | Overall |
|---|---|---|---|---|---|---|
| `PlayCombatAnim` | Proven | Proven | Proven | Partial | Partial | Partial |
| `PlayCombatAnimation` (script.bin) | Proven | Proven | Proven | Partial | Partial | Partial |

Gap is **not** missing dispatch. Gap is **pose**: host and Father
`vtbl+76` both fail to consume `TURNING_AC90`.
`FirstSeenPlayCombatAnimationYields=true`.
`WorldShading.FirstSeenPlaysAnim=false`.

---

## Classifications

1. **Anim name — `TURNING_AC90`. PROVEN** (def+60 / raw `0x03A0`).
2. **Opcode — `00CC15E3` `.PlayCombatAnim`. PROVEN** (listing + `0x012C2540`).
3. **Persist verb — `PlayCombatAnimation`. PROVEN** leftover alias of that token.
4. **Apply — `00CC16FD` `vtbl+76`. PROVEN** call; name unread. **PARTIAL** pose.
5. **Yield — `00CC5691` `[0x143E8F8] vtbl+28`. PROVEN.**
6. **Host — PARTIAL.** Record + yield; no combat clip.
