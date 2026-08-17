# Fixture command audit

Driven from real `script.bin` via `ScriptRuntime.StartCutscene` +
`PumpUntilSettled`. Status is the recovered **script-layer** apply
(00CBFB7D parse + handler + return), not the unread inner mesh/UI
bodies those handlers call.

Traces: `{SCRATCH}/traces/<cutscene>.txt`.

## Outcome

| Fixture | End |
|---|---|
| CS_OAKVALE_INTRO_FATHER | Finished def+60 |
| CS_OAKVALE_INTRO_THERESA_MEET | Finished |
| CS_BANDITRAID_GATESTART | Finished |
| CS_CHICKING_START | Finished |
| CS_CHICKING_TOPPRIZE | Finished; CrowdAnimate `SPECTATORCS,CS_CHEER` / `CS_IDLE` with anim ops |
| CS_CHICKING_HITGUYBOTTOM | Finished; `RemoveThing` is `00BFEAF8` n=6 → `Remove` `00CD0116` / `vtbl+432` |

Grep of traces for UNKNOWN/FALLBACK/HARDCODED/APPROXIMATE/NO-OP/UNRESOLVED/UNPROVEN YIELD: none after RemoveThing recovery.

Added discovery fixtures: `CS_OPENGRAVE_CRYPTCAM` (`WaitForCamera`), `CS_PUNCHCLUB_BS_RUNFORESTRUN` (`WaitPlayAnimation`).

## Exercised commands

| Verb | Family | Args | Side effect | Return | Yield/wait | Binding / lifetime |
|---|---|---|---|---|---|---|
| PlayMusic | G | track | store track; 009E5120+vtbl+2784 | Continue | none | audio last-track |
| FadeOut | G | sec[,p] | overlay rise, lock | Continue | none | fade record |
| FadeIn | G | sec[,p] | clear lock, fall | Continue | none | fade record |
| CameraPause | G | FALSE | `[ebp-37]=0` | Continue | gates later UseCamera | runner local |
| Teleport | E | marker[,IsFalse] | marker pos → actor | Continue | none | World.Positions |
| LookToThing | E | target[,FOREVER][,IsFalse] | store look target | YieldOnce unless arg2 FALSE | one vtbl+28 | World.LookTargets |
| LookInDirection | E | deg[,IsFalse] | store heading | Continue | none | World.Looks |
| DoScriptFrame | G | [n] | atoi count | WaitFrames | n × vtbl+28 | runner remaining |
| DoCameraPreloading | G | [IsTrue] | collect UseCamera names | Continue | none | Camera.Preloaded |
| UseCamera | G | name | TNG bind | YieldOnce if `[ebp-37]` | vtbl+28 | Camera.ActiveName |
| NoLoadUseCamera | G | name | same bind | same gate | 00CC907D | Camera.ActiveName |
| PlayAVI | G | file | Data\Video\ + 006286F0 | BlockPump | skip/EOF | AviPlaying |
| MuteSounds | G | IsFalse? | mute flag | Continue | none | Audio.Muted |
| StartTimeCode | G | — | zero 0x13B83C8 | Continue | none | TimeCode |
| GamePause | G | sec | target=sec×15 | WaitScaledFrames | increment 1 | runner counter |
| Speak | E | target,text | queue line | YieldOnce | vtbl+28 leftover | Dialogue.Active |
| InteractiveSpeak | E | lis,prompt[,wait] | queue; TRUE → ispeak-N | YieldOnce / WaitOp | FALSE once; TRUE until CompleteWait | Dialogue.WaitOp |
| DialogSpeak | E | lis,text | queue | YieldOnce | vtbl+28 | Dialogue |
| DialogadSpeak | E | tgt,text | queue | Continue | no vtbl+28 | Dialogue |
| WaitActiveDialog | G | — | leftover poll op | YieldOnce | one leftover | dialog-N |
| WaitTask | E | name | leftover vtbl+104 | YieldOnce | once | recorded |
| PlayAnimation | E | name,flags | enqueue Play | YieldOnce | vtbl+28 | Animation.ByActor |
| PlayCombatAnimation | E | name,flags | enqueue | YieldOnce | vtbl+28 | Animation.Combat |
| SneakTo / WalkTo / RunTo | E | marker[,spd][,wait] | enqueue move | YieldOnce (WalkTo TRUE waits leftover) | stub 004C72B0 | Movement.ByActor |
| Create | G | type,marker,name | alias at marker | Continue | none | Bindings.Created |
| Remove | G | name | unbind | Continue | none | Bindings |
| RemoveThing | G | name | same apply as Remove (`00BFEAF8` n=6) | Continue | none | Destroy + unbind |
| RemoveExtras | G | IsTrue,limbo | ExtrasHidden + mode | Continue | none | World.ExtraOps |
| SetDoorOpen | G | door,IsTrue | door flag | Continue | none | World.Doors |
| RegisterActor | G | name | register slot | Continue | none | Bindings.Registered |
| Get | G | src,alias | acquired alias | Continue | none | Bindings.Acquired |
| FallbackAcquire | G | alias,types… | first matching type | Continue | none | Bindings.Acquired |
| CrowdAcquire | G | type,alias | real members only as alias0..n | Continue | none | Bindings.Crowd* |
| CrowdClearActions | G | crowd | clear member tasks | Continue | none | Movement |
| CrowdAnimate | G | crowd,anim,… | Play on each member; empty skip | Continue | none | Animation.Plays |
| GiveHero | G | item[,n] | gift list | Continue | none | World.HeroGifts |
| ClearCommands | E | IsTrue[,…] | clear anim+move | Continue if TRUE else YieldOnce | vtbl+28 if not TRUE | tasks complete |
| AddScriptedMode / RemoveScriptedMode | E | mode | record | Continue | none | World.Modes |
| EntitySetMaxWalkingSpeed / RunningSpeed | E | speed | store | Continue | none | Movement.*Speed |

## TOPPRIZE CrowdAnimate

With three `DefinitionType=Spectator` things bound before start, `CrowdAcquire Spectator, SPECTATORCS` creates SPECTATORCS0..2. Each `CrowdAnimate` records `side=SPECTATORCS,CS_CHEER` or `SPECTATORCS,CS_IDLE` and `animation=` that name. `$ANIM`/`$LOOP` do not reach the handler.
