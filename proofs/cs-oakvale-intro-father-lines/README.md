# `CS_OAKVALE_INTRO_FATHER` first 20 lines vs `ScriptInterpreter`

Investigation only. No production `src/` edits.

Do **not** treat this as Leave / first `004189C2`. Runner
`00CBFB7D` is leftover `00DB86B0` (`NOVI_LiveFather` →
`CS_OAKVALE_INTRO_FATHER`). First line is **not** the
`FadeOut 0.5,0` head special-case
(`FirstSeenFadeSpecialCaseRuns=false`).
`FirstSeenFadeOpcodeInStartOakVale=true` is the **in-loop**
line after PlayMusic (`00CD0987`), not `00CBFDD0`.

Do **not** skip `PlayAVI dream_sequence_comp.xmv`. That is
def+60 line 9 inside this leftover, not the retail startup
AVI (`006286F0` ×3 on `0042EC7C`).

Status words: **MATCH** / **UNREAD** / **PROVEN** /
**PARTIAL** / **DISPROVEN** / **LEFTOVER**.

**MATCH** = host fetch + dispatcher `Eq` + return class
match native `00CBFB7D` for that CString.
**UNREAD** = inner apply body not recovered (line still
dispatches; not `UNKNOWN` / Block).

Sources: `assembly/compiled-defs/script/0481-CS_OAKVALE_INTRO_FATHER.md`,
`assembly/compiled-defs/script/entries.tsv` row 481,
`src/Fable.Game/ScriptInterpreter.cs` (`TryFadeSpecialCase`,
`RunUntilYield`),
`Scripting/GlobalDispatcher.cs` / `EntityDispatcher.cs`,
`ScriptCommandMap.cs` / `RegionTravel.cs`,
`docs/runtime/COMMAND_MAP.md`,
`docs/runtime/FIXTURE_COMMAND_AUDIT.md`,
`docs/runtime/traces/runtime-trace.txt`,
`proofs/audit-scriptinterpreter/README.md`.

---

## Verdict

Compiled `CCutsceneDef` index **481**, raw **2017**,
persist vector **0** at def+60 (60 CStrings).
`00DB86B0` pushes `0,0,0` then `00CBFB7D`;
`[ebp+120]!=1` copies **+60**, not +108.

Head compare `00BFEBA8("FadeOut 0.5,0")` **misses**.
Host `TryFadeSpecialCase` exact-equals `Commands[0]` and
skips. First executed CString is `PlayMusic MUSIC_SET_NULL`.

All 20 head lines have a host handler. None Block.
Apply bodies that stay unread are called out per line.

Vector **1** (def+72, 7 skip lines) does **not** auto-run
(`FirstSeenCutsceneVector1AutoRuns=false`; `00CBEB7E` false).

---

## Line table (vector 0, first 20)

PC is 0-based CString index. Trace `pc` is 1-based after
the Continue increment.

| PC | Raw | Family | Native | Host | Return | Interp | Apply |
|---:|---|---|---|---|---|---|---|
| 0 | `PlayMusic MUSIC_SET_NULL` | G | `00CC8EAC` / `00CBF7FE` | `GlobalDispatcher` PlayMusic | Continue (`jmp 00CD17FD`) | **MATCH** | **UNREAD** (`vtbl+2784` player) |
| 1 | `FadeOut 0.5,0` | G | `00CD0987` / `008907E0` | `FadeOut` (in-loop, not `00CBFDD0`) | Continue | **MATCH** | **MATCH** (`vtbl+1488` black 0.5,0) |
| 2 | `CameraPause FALSE` | G | `00CC71F1` / `00CC7241` | `CameraPause` `IsFalse` → `[ebp-37]=0` | Continue | **MATCH** | **MATCH** |
| 3 | `Hero.Teleport MK_OVI_ID_HERO,FALSE` | E | `00CC4678` / `0089B780` | `EntityDispatcher` Teleport | Continue (no `vtbl+28`) | **MATCH** | **UNREAD** (yaw write) |
| 4 | `Father.Teleport MK_OVI_ID_DAD` | E | `00CC4678` | same | Continue | **MATCH** | **UNREAD** (yaw write) |
| 5 | `Father.LookToThing Hero,FOREVER` | E | `00CC3B3F` / `vtbl+1992` | `LookToThing`; 2 args, not arg2 FALSE | YieldOnce (`vtbl+28`) | **MATCH** | **UNREAD** (look body) |
| 6 | `DoScriptFrame 1` | G | `00CC7085` | atoi → `WaitFrames` 1 | WaitFrames | **MATCH** | **MATCH** |
| 7 | `DoCameraPreloading` | G | `00CC86D0` / `00CBF29F` | walk later `UseCamera` names | Continue | **MATCH** | **UNREAD** (`vtbl+1560/1568`) |
| 8 | `DoScriptFrame 1` | G | `00CC7085` | same as PC 6 | WaitFrames | **MATCH** | **MATCH** |
| 9 | `PlayAVI dream_sequence_comp.xmv` | G | `00CCA26D` / `006286F0` | `BeginAvi` + `BlockPump` | BlockPump | **MATCH** | **MATCH** (`Data\Video\` + player) |
| 10 | `MuteSounds false` | G | `00CC7258` / `vtbl+2664` | `Mute(!IsFalse("false"))` → unmute | Continue | **MATCH** | **UNREAD** (mixer body) |
| 11 | `FadeOut 0.5,0` | G | `00CD0987` | same as PC 1 | Continue | **MATCH** | **MATCH** |
| 12 | `DoScriptFrame 2` | G | `00CC7085` | atoi 2 | WaitFrames | **MATCH** | **MATCH** |
| 13 | `Hero.PlayAnimation CS_WAKING_UP_LOOP,FALSE,FALSE,TRUE,FALSE` | E | `00CC14B8` / `vtbl+72` | `PlayAnimation`; `AnimationPause` default 1 | YieldOnce | **MATCH** | **UNREAD** (XSEQ time interp) |
| 14 | `Hero.PlayAnimation CS_WAKING_UP_ON_STEPS,FALSE,FALSE,TRUE,FALSE` | E | `00CC14B8` | same | YieldOnce | **MATCH** | **UNREAD** (XSEQ time interp) |
| 15 | `DoScriptFrame 4` | G | `00CC7085` | atoi 4 | WaitFrames | **MATCH** | **MATCH** |
| 16 | `StartTimeCode` | G | `00CD1373` | `TimeCode=0` (`and [0x13B83C8],0`) | Continue | **MATCH** | **UNREAD** (later increment) |
| 17 | `PlayMusic MUSIC_SET_OAKVALE` | G | `00CC8EAC` | same as PC 0 | Continue | **MATCH** | **UNREAD** (player) |
| 18 | `NoLoadUseCamera CAM_OVI_ID_STANDUP` | G | `00CC9E6A` / `00CC907D` | `UseCamera`/`NoLoadUseCamera`; pause already FALSE → no yield | Continue (gate) | **MATCH** | **UNREAD** (spline) |
| 19 | `FadeIn` | G | `00CC4B22` / `0088E4C0` | `ParseFade` defaults 0.5 / 0 | Continue | **MATCH** | **MATCH** |

---

## Head special-case (not a line)

| Site | Native | Host | Class |
|---|---|---|---|
| `00CBFD95` pick vector | `[ebp+120]!=1` → def+60 | `Commands` = vector 0 | **MATCH** |
| `00CBFDD0` `FadeOut 0.5,0` | first-line strcmp miss | `TryFadeSpecialCase` miss | **MATCH** skip |
| `FirstSeenFadeSpecialCaseRuns` | false | `RegionTravel` false | **PROVEN** leftover |

---

## Control flow on this slice

```
00DB86B0
  bind Hero / Father
  push 0,0,0
  00CBFB7D("CS_OAKVALE_INTRO_FATHER")
    copy +60
    FadeOut 0.5,0 special-case MISS
    [0] PlayMusic MUSIC_SET_NULL     jmp 00CD17FD
    [1] FadeOut 0.5,0                vtbl+1488
    [2] CameraPause FALSE            [ebp-37]=0
    [3] Hero.Teleport …              no yield
    [4] Father.Teleport …
    [5] Father.LookToThing …FOREVER  vtbl+28
    [6] DoScriptFrame 1
    [7] DoCameraPreloading           00CBF29F
    [8] DoScriptFrame 1
    [9] PlayAVI dream_sequence_comp.xmv   BlockPump 006286F0
    …
```

`DoCameraPreloading` / `00CBF29F` collect later **`UseCamera`**
names (`CAM_OVIF_SHOT2`, `CAM_OVIF_SHOT3`, `CAM_OVIF_SHOT7`).
It does **not** collect `NoLoadUseCamera`. Host walk is the
same four verbs. **MATCH.**

After PC 2, `CameraPauseEnabled=false`, so PC 18
`NoLoadUseCamera` does **not** take `vtbl+28`.
`AnimationPause` is never set in this head; default 1, so
PC 13–14 still YieldOnce.

---

## Not these 20

| Item | Why omitted |
|---|---|
| Vector 1 `FadeOut` / `GamePause 0.5` / `UseCamera CAM_OVIF_SHOT7` / … | skip list; `00CBEB7E` false |
| `Father.Speak` / `InteractiveSpeak` / `UseCamera CAM_OVIF_SHOT2` | PC ≥20 / ≥25 |
| Retail startup AVI | not this `PlayAVI` line |
| `.SummonerAttack` first *compare* | loop token order, not this def |

---

## Classifications (short)

1. **First executed leftover line — `PlayMusic MUSIC_SET_NULL`. MATCH.**
2. **Head FadeOut special-case — does not run. MATCH skip.**
3. **In-loop `FadeOut 0.5,0` is PC 1 (and PC 11). MATCH apply.**
4. **PlayAVI on PC 9 is this leftover, not startup AVI. MATCH. Do not skip.**
5. **No line in this head is interpreter UNREAD (no Block).**
6. **Apply UNREAD:** PlayMusic player, Teleport yaw, LookToThing
   body, DoCameraPreloading `vtbl+1560/1568`, MuteSounds mixer,
   PlayAnimation time interp, StartTimeCode increment,
   NoLoadUseCamera spline.
