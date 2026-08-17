# Recovered `00CBFB7D` command map

Statuses: **PROVEN** (recovered script-layer apply + return),
**PARTIAL** (token + return proven; apply unread),
**UNREAD** (not implemented; interpreter blocks).

Unknown verbs are **UNREAD**. They are not complete no-ops.

Generated from `ScriptCommandMap.All` (see `COMMAND_MAP.generated.md`).

| Verb | Token | Apply | Args | Return | Status | Evidence |
|---|---|---|---|---|---|---|
| PlayMusic | `00CC8EAC` | `00CBF7FE` | track | CompleteNow | Partial | lookup 009E5120 then vtbl+2784; jmp 00CD17FD; host stores track |
| FadeOut | `00CD0987` | `008907E0` | seconds,param | CompleteNow | Proven | vtbl+1488 pack black; 00434C00 +188 |
| FadeIn | `00CC4B22` | `0088E4C0` | seconds,param | CompleteNow | Proven | vtbl+1496 clear lock; falling overlay |
| CameraPause | `00CC71F1` | `00CC7241` | flag | CompleteNow | Proven | IsFalse → [ebp-37]=0; ctor 00CBFD53=1; gates UseCamera vtbl+28 |
| Teleport | `00CC4678` | `0089B780` | marker[,IsFalse] | CompleteNow | Proven | marker pos 004AA980; vtbl+124; no vtbl+28; yaw write unread |
| LookToThing | `00CC3B3F` | `—` | target[,mode][,IsFalse] | YieldAfterUnlessFalse | Partial | vtbl+1992; FOREVER wait; body UNREAD — record + yield |
| DoScriptFrame | `00CC7085` | `—` | [count] | WaitFrames | Proven | atoi; each count one vtbl+28 |
| DoCameraPreloading | `00CC86D0` | `00CBF29F` | [IsTrue] | CompleteNow | Partial | collects UseCamera names vtbl+1648; vtbl+1560/1568 UNREAD |
| UseCamera | `00CC9F3A` | `00B23B50` | name | YieldAfter | Proven | TNG lookup; bind helper; one vtbl+28 |
| NoLoadUseCamera | `00CC9E6A` | `00CC907D` | name | YieldAfter | Proven | separate token; yield helper 00CC907D |
| PlayAnimation | `00CC14B8` | `004C7470` | name[,flags] | YieldAfter | Partial | vtbl+72; CTC +68 00686920 al=1; inner 0070D580 not this path |
| PlayAVI | `00CCA26D` | `006286F0` | file | BlockPump | Proven | Data\Video\ prefix; blocking 006286F0; no vtbl+28 |
| MuteSounds | `00CC7258` | `—` | IsFalse? | CompleteNow | Partial | vtbl+2664; jmp 00CC8464; apply body UNREAD |
| StartTimeCode | `00CD1373` | `—` |  | CompleteNow | Partial | and [0x13B83C8],0; leftover increment not a pose clock |
| GamePause | `00CC88D1` | `—` | seconds | WaitScaledFrames | Proven | atof * [0x124E640]=15; CLOCK path UNREAD |
| Speak | `00CC25FD` | `—` | target,text[,…] | YieldAfter | Partial | vtbl+52/+104 leftover poll; no dialogue UI |
| InteractiveSpeak | `00CC2EAA` | `—` | listener,prompt[,wait] | YieldAfterUnlessWait | Partial | vtbl+1456/1460/1464; TRUE wait vtbl+1472 UNREAD |
| DialogSpeak | `00CC3165` | `—` | listener,text | YieldAfter | Partial | one vtbl+28; bodies UNREAD |
| DialogadSpeak | `00CC3354` | `—` | target,text[,mode] | CompleteNow | Partial | no vtbl+28; father +52 stub; no dialogue UI |
| WaitTask | `00CC0783` | `—` | name | YieldAfter | Partial | poll vtbl+104 leftover; no task table |
| WaitActiveDialog | `00CC656B` | `—` |  | YieldAfter | Partial | session poll vtbl+1472; dismiss UNREAD |
| SneakTo | `00CC0CB5` | `—` | marker[,speed][,wait] | YieldAfterOrWait | Partial | vtbl+20 stub 004C72B0; TRUE wait leftover once; no mesh move |
| WalkTo | `00CC083D` | `—` | marker[,speed][,wait] | YieldAfterOrWait | Partial | same stub; first-seen does not wait |
| PlayCombatAnimation | `00CC15E3` | `—` | name[,flags] | YieldAfter | Partial | vtbl+76 does not read name; no TURNING_AC90 pose |
| PlayCombatAnim | `00CC15E3` | `—` | name[,flags] | YieldAfter | Partial | exe token alias of PlayCombatAnimation |
| Create | `00CCC246` | `—` | type,marker,name | CompleteNow | Partial | vtbl+364; spawn body UNREAD |
| Remove | `00CD0116` | `—` | name | CompleteNow | Partial | vtbl+432; teardown UNREAD |
| LookInDirection | `00CC3F73` | `0089BDF0` | degrees[,IsFalse] | CompleteNow | Partial | vtbl+1896; heading body UNREAD |
| SetTime | `00CD07D6` | `00CD082A` | hours[,flag][,duration] | CompleteNow | Proven | wrap 24 * 1/24 clamp [0,1] at clock+8; vtbl+2584 0088FDC0 |

## StartNewGame bindings

| Binding | Kind | Evidence |
|---|---|---|
| `NOVI_LiveFather` → `CS_OAKVALE_INTRO_FATHER` | PROVEN GENERIC | `ScriptFactoryTable` / `00DAC2C0` / `00DB86B0` |
| Fiber `S_QNOVI` + persist `AttackOver` | PROVEN GENERIC | `ScriptFiberTable` / `00DAADA0` |
| `StartNewGame` Oakvale strings | removed | load → `InstallRecoveredBindings` → `ActivateThings` |
