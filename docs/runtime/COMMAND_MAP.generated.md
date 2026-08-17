| Verb | Token | Apply | Args | Return | Parse | Dispatch | ReturnSt | ApplySt | Runtime | Overall | Evidence |
|---|---|---|---|---|---|---|---|---|---|---|---|
| PlayMusic | `00CC8EAC` | `00CBF7FE` | track | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | lookup 009E5120 then vtbl+2784; jmp 00CD17FD; host stores track |
| Play2DSound | `00CBF89E` | `009E5120` | name | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | 009E5120 + vtbl+2792; empty skip; jmp 00CD17FD; not PlayAVI |
| PlaySound | `00CC8F4E` | `00CC8FC1` | source,name[,criteria] | YieldAfter | Proven | Proven | Proven | Partial | Partial | Partial | NULL arg0 vtbl+2768; else lookup + vtbl+2756/2760; yield 00CC907D |
| FadeOut | `00CD0987` | `008907E0` | seconds,param | CompleteNow | Proven | Proven | Proven | Proven | Proven | Proven | vtbl+1488 pack black; 00434C00 +188 |
| FadeIn | `00CC4B22` | `0088E4C0` | seconds,param | CompleteNow | Proven | Proven | Proven | Proven | Proven | Proven | vtbl+1496 clear lock; falling overlay |
| CameraPause | `00CC71F1` | `00CC7241` | flag | CompleteNow | Proven | Proven | Proven | Proven | Proven | Proven | IsFalse -> [ebp-37]=0; ctor 00CBFD53=1; gates UseCamera vtbl+28 |
| Teleport | `00CC4678` | `0089B780` | marker[,IsFalse] | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | marker pos 004AA980; vtbl+124; no vtbl+28; yaw write unread |
| LookToThing | `00CC3B3F` | `—` | target[,mode][,IsFalse] | YieldAfterUnlessFalse | Proven | Proven | Proven | Partial | Partial | Partial | vtbl+1992; FOREVER wait; body UNREAD — record + yield |
| DoScriptFrame | `00CC7085` | `—` | [count] | WaitFrames | Proven | Proven | Proven | Proven | Proven | Proven | atoi; each count one vtbl+28 |
| DoCameraPreloading | `00CC86D0` | `00CBF29F` | [IsTrue] | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | collects UseCamera names vtbl+1648; vtbl+1560/1568 UNREAD |
| UseCamera | `00CC9F3A` | `00B23B50` | name | YieldAfter | Proven | Proven | Proven | Proven | Partial | Partial | TNG lookup; bind ScriptedCamera pos/look/fov; one vtbl+28; spline unread |
| NoLoadUseCamera | `00CC9E6A` | `00CC907D` | name | YieldAfter | Proven | Proven | Proven | Proven | Partial | Partial | separate token; same TNG bind; yield helper 00CC907D |
| WaitForCamera | `00CCA41F` | `00CCA58F` |  | YieldAfterOrWait | Proven | Proven | Proven | Partial | Partial | Partial | poll vtbl+1672; idle -> 00CD17FD; busy -> vtbl+28 then re-poll |
| ResetCamera | `00CC9DF1` | `00CC9E40` |  | CompleteNow | Proven | Proven | Proven | Proven | Partial | Partial | vtbl+1668(0.0) then vtbl+1664; jmp 00CD17FD; restores gameplay snapshot |
| ScriptFrame | `00CC7124` | `00CC7181` | [IsFalse] | CompleteNow | Proven | Proven | Proven | Proven | Proven | Proven | IsFalse -> [ebp+103]=!IsFalse yield-enable; jmp 00CC8464 |
| DoOneFrame | `00CC75A8` | `00CC7605` |  | YieldAfter | Proven | Proven | Proven | Proven | Proven | Proven | if [ebp+103] vtbl+28; timecode; jmp 00CC8464 |
| CreateNear | `00CCBEE7` | `00CCC027` | type,near,name[,radius] | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | atof arg3; 004AA980 pos; vtbl+368 factory (not 364/392); offset unread |
| ObjectCreate | `00CCC4FC` | `00CCC62E` | type,marker,name | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | vtbl+392 object factory; empty any skip; jmp 00CC864B |
| CrowdCreate | `00CCC92F` | `00CCCAA1` | type,source,alias[,IsTrue] | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | vtbl+300(source); per-item vtbl+364; alias+i via 0099F570; 00CD3D2E |
| CrowdCreateMixed | `00CCC64D` | `00CCC7A8` | typeA,typeB,source,alias | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | vtbl+300(source); rand 00BFEB16%2 picks typeA/typeB; vtbl+364 each |
| PlayAnimation | `00CC14B8` | `004C7470` | name[,flags] | YieldAfter | Proven | Proven | Proven | Partial | Partial | Partial | vtbl+72; CTCAnimationComplex +68 is 00686920 al=1; inner 0070D580 not this path |
| PlayLoopingAnim | `00CC1731` | `—` | name[,flags] | YieldAfter | Proven | Proven | Proven | Partial | Partial | Partial | separate token after PlayCombatAnim; entity task slot |
| PlayAVI | `00CCA26D` | `006286F0` | file | BlockPump | Proven | Proven | Proven | Proven | Proven | Proven | Data\Video\ prefix; blocking 006286F0; no vtbl+28 |
| MuteSounds | `00CC7258` | `—` | IsFalse? | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | vtbl+2664; jmp 00CC8464; apply body UNREAD |
| StartTimeCode | `00CD1373` | `—` |  | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | and [0x13B83C8],0; leftover increment not a pose clock |
| GamePause | `00CC88D1` | `—` | seconds | WaitScaledFrames | Proven | Proven | Proven | Proven | Proven | Proven | atof * [0x124E640]=15; CLOCK path UNREAD |
| Speak | `00CC25FD` | `—` | target,text[,…] | YieldAfter | Proven | Proven | Proven | Partial | Partial | Partial | vtbl+52/+104 leftover poll; session recorded; no dialogue UI |
| InteractiveSpeak | `00CC2EAA` | `—` | listener,prompt[,wait] | YieldAfterUnlessWait | Proven | Proven | Proven | Partial | Partial | Partial | vtbl+1456/1460/1464; TRUE wait vtbl+1472 UNREAD |
| DialogSpeak | `00CC3165` | `—` | listener,text | YieldAfter | Proven | Proven | Proven | Partial | Partial | Partial | one vtbl+28; bodies UNREAD |
| DialogadSpeak | `00CC3354` | `—` | target,text[,mode] | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | no vtbl+28; father +52 stub; no dialogue UI |
| WaitTask | `00CC0783` | `—` | name | YieldAfter | Proven | Proven | Proven | Partial | Partial | Partial | poll vtbl+104 leftover; entity task slot |
| WaitActiveDialog | `00CC656B` | `—` |  | YieldAfter | Proven | Proven | Proven | Partial | Partial | Partial | session poll vtbl+1472; dismiss UNREAD |
| WaitPlayAnimation | `00CC2518` | `—` |  | YieldAfter | Proven | Proven | Proven | Partial | Partial | Partial | poll current entity anim task |
| SneakTo | `00CC0CB5` | `—` | marker[,speed][,wait] | YieldAfterOrWait | Proven | Proven | Proven | Partial | Partial | Partial | vtbl+20 stub 004C72B0; TRUE wait leftover once; dest stored |
| WalkTo | `00CC083D` | `—` | marker[,speed][,wait] | YieldAfterOrWait | Proven | Proven | Proven | Partial | Partial | Partial | same stub; dest + entity task; nav unread |
| RunTo | `00CC25E4` | `—` | marker[,speed][,wait] | YieldAfterOrWait | Proven | Proven | Proven | Partial | Partial | Partial | same entity task slot as WalkTo |
| PlayCombatAnimation | `00CC15E3` | `—` | name[,flags] | YieldAfter | Proven | Proven | Proven | Partial | Partial | Partial | vtbl+76 does not read name; no TURNING_AC90 pose |
| PlayCombatAnim | `00CC15E3` | `—` | name[,flags] | YieldAfter | Proven | Proven | Proven | Partial | Partial | Partial | exe token alias of PlayCombatAnimation |
| Create | `00CCC246` | `—` | type,marker,name | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | vtbl+364; spawn body UNREAD; C# inserts ThingInstance |
| Remove | `00CD0116` | `008910D0` | name[,dead|IsTrue] | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | empty skip; dead -> vtbl+1608; else vtbl+432 008910D0/004C9B80 |
| RemoveThing | `00CD0116` | `008910D0` | name | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | NOT a separate token. 00BFEAF8 n=6 matches Remove. Same apply. |
| RemoveAll | `00CC67B5` | `00CC6817` | IsFalse? | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | separate path; vtbl+336 collection; vtbl+2044 per item; NOT vtbl+432 |
| RemoveAllThings | `00CC66A7` | `00CC6783` | name | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | separate path; empty skip; vtbl+300(LadyGreyIntro) then vtbl+432 |
| LookInDirection | `00CC3F73` | `0089BDF0` | degrees[,IsFalse] | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | vtbl+1896; heading body UNREAD |
| SetTime | `00CD07D6` | `00CD082A` | hours[,flag][,duration] | CompleteNow | Proven | Proven | Proven | Proven | Proven | Proven | wrap 24 * 1/24 clamp [0,1] at clock+8; vtbl+2584 0088FDC0 |
| Get | `—` | `—` | source,alias | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | script.bin Get NAME,ALIAS binds acquired alias; continue |
| FallbackAcquire | `00CCD344` | `00CCD397` | alias,type[,type…] | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | vtbl+320 candidates; first matching type; jmp 00CD17FD |
| CrowdAnimate | `00CCE4EC` | `00CCE53F` | crowd,anim,_,_,_,flags… | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | 00515700 crowd; per-member 007E73F0; empty skip; jmp 00CD17FD |
| RemoveExtras | `00CC6ACE` | `00CC6B21` | IsTrue,limbo|return | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | limbo/return flags; hide extras; jmp continue |
| StopMusic | `—` | `—` |  | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | clears last track; continue |
| StayFadedOut | `—` | `—` |  | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | runner local stay-faded |
| EnableSounds | `—` | `—` |  | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | unmute; continue |
| NoDialogCam | `—` | `—` | IsTrue | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | runner local |
| AnimationPause | `00CC718B` | `00CC718B` | flag | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | IsFalse store like CameraPause; apply body unread |
| CameraLookAt | `00CCA73F` | `00CCA953` | thing,mode[,floats] | YieldAfter | Proven | Proven | Proven | Partial | Partial | Partial | empty skip; vtbl+1628; yield if [ebp+103] |
| CameraLookBetween | `00CCAA6C` | `00CCADB9` | a,b,mode,dur[,offA][,offB] | YieldAfter | Proven | Proven | Proven | Partial | Partial | Partial | 4 required; vtbl+1632(posA+off,posB+off,dur,-1); yield if [ebp+103] |
| CameraFOVLookBetween | `00CCB479` | `00CCB728` | a,b,mode,dur[,fovDeg] | YieldAfter | Proven | Proven | Proven | Partial | Partial | Partial | same vtbl+1632; arg4 degrees*1/360 or -1; yield if [ebp+103] |
| PutUpYourSwords | `00CC9303` | `—` | IsFalse? | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | token 00CC9303; sheathe apply unread |
| RegisterActor | `00CC662D` | `00CC669B` | name | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | empty skip; 004AC860 register; jmp 00CC7081 |
| CrowdAcquire | `00CCCEA7` | `00515700` | type,alias | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | real members only as alias0..n |
| CrowdClearActions | `—` | `—` | crowd | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | clear member entity tasks |
| GiveHero | `—` | `—` | item[,n] | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | gift list; count default 1 |
| SetDoorOpen | `—` | `—` | door,IsTrue | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | door flag |
| ClearCommands | `—` | `—` | IsTrue[,…] | YieldAfterUnlessFalse | Proven | Proven | Proven | Partial | Partial | Partial | cancel entity task slot; TRUE continue else vtbl+28 |
| AddScriptedMode | `—` | `—` | mode | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | record mode |
| RemoveScriptedMode | `—` | `—` | mode | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | record mode |
| EntitySetMaxWalkingSpeed | `—` | `—` | speed | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | store gait max |
| EntitySetMaxRunningSpeed | `—` | `—` | speed | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | store gait max |
| Drawable | `—` | `—` | IsFalse? | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | entity drawable flag |
| Collide | `—` | `—` | IsFalse? | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | entity collide flag |
| SetAlpha | `—` | `—` | alpha | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | entity alpha |
| LookAt | `—` | `—` | target | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | record look |
| LookAtNothing | `—` | `—` |  | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | clear look |
| PutInFrontOf | `00CD029F` | `00CD0501` | mover,face,distance | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | same dest as WalkUpToThing; vtbl+1892 teleport; vtbl+1900 look; jmp 00CC864B |
| WalkUpToThing | `00CC2331` | `00CC2538` | thing,distance[,…] | YieldAfterOrWait | Proven | Proven | Proven | Partial | Partial | Partial | dest=pos+atof(arg1)*(vtbl+288+12); actor vtbl+16 speed 1; leftover vtbl+104 |
| FollowThing | `00CC19F2` | `00CC1AE9` | target[,speed] | YieldAfter | Proven | Proven | Proven | Partial | Partial | Partial | default speed 1.0; actor vtbl+28; yield 00CC0E96 if [ebp+103] |
| StopFollowingThing | `00CC1B2F` | `00CC1BF4` | [target] | YieldAfter | Proven | Proven | Proven | Partial | Partial | Partial | actor vtbl+32; jmp 00CC568C leftover |
| SetFlag | `00CCA475` | `00CCA4C8` | name,IsFalse?[,IsTrue skip] | YieldAfter | Proven | Proven | Proven | Proven | Proven | Proven | 008ADF10 write 0/1; [ebp-39] latch; jmp 00CC907D |
| WaitFlag | `00CCB840` | `00CCB893` | name,IsTrue? | YieldAfterOrWait | Proven | Proven | Proven | Proven | Proven | Proven | 008ADF10 cmp [eax],bl; match 00CD17FD; else leftover 00CCB8CE |
