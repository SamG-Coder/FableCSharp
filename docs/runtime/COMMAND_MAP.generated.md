| Verb | Token | Apply | Args | Return | Parse | Dispatch | ReturnSt | ApplySt | Runtime | Overall | Evidence |
|---|---|---|---|---|---|---|---|---|---|---|---|
| PlayMusic | `00CC8EAC` | `00CBF7FE` | track | CompleteNow | Proven | Proven | Proven | Proven | Partial | Partial | 009E5120 map then vtbl+2784; jmp 00CD17FD; Sound/*.ogg; player UNREAD |
| Play2DSound | `00CBF89E` | `00CBF8DA` | name | CompleteNow | Proven | Proven | Proven | Proven | Partial | Partial | leftover helper 00CBF7FE vtbl+2768; not 00BFEAF8; not PlayAVI |
| PlaySound | `00CC8F4E` | `00CC8FC1` | source,name[,criteria] | YieldAfter | Proven | Proven | Proven | Proven | Partial | Partial | empty skip; IsNull vtbl+2768; else 00CBF9DE + 2756/2760; leftover +28 |
| CacheMusic | `00CC8E1B` | `00CC8E6D` | track | CompleteNow | Proven | Proven | Proven | Proven | Partial | Partial | 009E5120 then vtbl+2792; miss skip; jmp 00CD17FD; not PlayMusic 2784 |
| FadeOut | `00CD0987` | `008907E0` | seconds,param | CompleteNow | Proven | Proven | Proven | Proven | Proven | Proven | vtbl+1488 pack black; 00434C00 +188 |
| FadeIn | `00CC4B22` | `0088E4C0` | seconds,param | CompleteNow | Proven | Proven | Proven | Proven | Proven | Proven | vtbl+1496 clear lock; falling overlay |
| CameraPause | `00CC71F1` | `00CC7241` | flag | CompleteNow | Proven | Proven | Proven | Proven | Proven | Proven | IsFalse -> [ebp-37]=0; ctor 00CBFD53=1; gates UseCamera vtbl+28 |
| Teleport | `00CC4678` | `0089B780` | marker[,IsFalse] | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | marker pos 004AA980; vtbl+124; no vtbl+28; yaw write unread |
| LookToThing | `00CC3B3F` | `—` | target[,mode][,IsFalse] | YieldAfterUnlessFalse | Proven | Proven | Proven | Partial | Partial | Partial | vtbl+1992; FOREVER wait; body UNREAD — record + yield |
| DoScriptFrame | `00CC7085` | `—` | [count] | WaitFrames | Proven | Proven | Proven | Proven | Proven | Proven | atoi; each count one vtbl+28 |
| DoCameraPreloading | `00CC86D0` | `00CBF29F` | [IsTrue] | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | collects UseCamera names vtbl+1648; vtbl+1560/1568 UNREAD |
| UseCamera | `00CC9F3A` | `00B23B50` | name | YieldAfter | Proven | Proven | Proven | Proven | Partial | Partial | TNG lookup; bind ScriptedCamera pos/look/fov; one vtbl+28; spline unread |
| NoLoadUseCamera | `00CC9E6A` | `00CC907D` | name | YieldAfter | Proven | Proven | Proven | Proven | Partial | Partial | separate token; same TNG bind; yield helper 00CC907D |
| WaitForCamera | `00CCA41F` | `00CCA58F` |  | YieldAfterOrWait | Proven | Proven | Proven | Proven | Partial | Partial | vtbl+1672 on live camera; snap idle continue; spline leftover re-poll |
| WaitForMessageCamera | `00CCFF91` | `00CD0006` | name | YieldAfterOrWait | Proven | Proven | Proven | Partial | Partial | Partial | poll vtbl+2316(name); idle continue; leftover 00CCFFB2 |
| ResetCamera | `00CC9DF1` | `00CC9E40` |  | CompleteNow | Proven | Proven | Proven | Proven | Partial | Partial | vtbl+1668(0.0) then vtbl+1664; jmp 00CD17FD; restores gameplay snapshot |
| DrawThing | `00CC9D07` | `00CC9DDD` | name,IsFalse? | CompleteNow | Proven | Proven | Proven | Proven | Partial | Partial | lookup arg0; IsFalse(arg1)->0 else 1; vtbl+2044; jmp 00CC864B; World.Drawable |
| UseCameraFOVMarkerList | `00CC96BD` | `00CC9C53` | a,b,m0,m1,m2,m3,dur[,fov][,IsFalse] | CompleteNow | Proven | Proven | Proven | Proven | Partial | Partial | 7 required; 00CBF13C best XY projection; vtbl+1632(pos,pos,B,dur,fov); jmp 00CC864B |
| CameraRig | `00CC93E3` | `00CC965D` | a,b,ox,oy,oz,sec | WaitScaledFrames | Proven | Proven | Proven | Proven | Partial | Partial | 6 required; A to B+off vtbl+1892; vtbl+1644; loop arg5*15; yield if [ebp+103] |
| CameraShake | `00CD131F` | `00CD1366` | arg0,arg1 | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | atof both; vtbl+1696(arg1,arg0); jmp 00CD17FD; decay unread |
| CameraEffect | `00CD1258` | `00CD12C2` | arg0,arg1,arg2 | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | atof three; vtbl+1676(a,b,c); jmp 00CD17FD; filter unread |
| RemoveEffect | `00CD0071` | `00CD00F8` | name | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | walk extras 12-byte list; match vtbl+432(item,0,1); not Remove lookup |
| ScriptFrame | `00CC7124` | `00CC7181` | [IsFalse] | CompleteNow | Proven | Proven | Proven | Proven | Proven | Proven | IsFalse -> [ebp+103]=!IsFalse yield-enable; jmp 00CC8464 |
| DoOneFrame | `00CC75A8` | `00CC7605` |  | YieldAfter | Proven | Proven | Proven | Proven | Proven | Proven | if [ebp+103] vtbl+28; timecode; jmp 00CC8464 |
| CreateNear | `00CCBEE7` | `00CCC027` | type,near,name[,radius] | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | atof arg3; 004AA980 pos; vtbl+368 factory (not 364/392); offset unread |
| CreateEffect | `00CCBB9A` | `00CCBCDA` | type,marker[,name][,z][,IsTrue] | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | 00CBF9DE marker; vtbl+400; z on marker+8; jmp 00CC864B |
| DummyEffect | `00CCBD62` | `00CCBE5F` | type,marker,param[,name][,IsTrue] | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | 00CBF9DE marker; vtbl+404 not 400; jmp 00CC864B |
| CreateLight | `00CCB933` | `00CCBB61` | marker,R,G,B,f,f,flag,name,IsTrue | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | 9 required; 00BFEA70 RGB; vtbl+408; extras if IsTrue; jmp 00CC864B |
| ObjectCreate | `00CCC4FC` | `00CCC62E` | type,marker,name | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | vtbl+392 object factory; empty any skip; jmp 00CC864B |
| CrowdCreate | `00CCC92F` | `00CCCAA1` | type,source,alias[,IsTrue] | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | vtbl+300(source); per-item vtbl+364; alias+i via 0099F570; 00CD3D2E |
| CrowdCreateMixed | `00CCC64D` | `00CCC7A8` | typeA,typeB,source,alias | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | vtbl+300(source); rand 00BFEB16%2 picks typeA/typeB; vtbl+364 each |
| PlayAnimation | `00CC14B8` | `00CC15DA` | name[,IsTrue]x3[,IsFalse][,IsTrue] | YieldAfter | Proven | Proven | Proven | Partial | Partial | Partial | vtbl+72; CTC+68 00686920 stub — 0070D580 is 005B37F7 DEFAULT not this path |
| PlayLoopingAnim | `00CC1731` | `00CC186C` | name,loops[,flags] | YieldAfter | Proven | Proven | Proven | Partial | Partial | Partial | vtbl+80(name,atoi arg1,f0-4); not PlayAnimation; same [ebp-22] yield |
| PlayAVI | `00CCA26D` | `006286F0` | file | BlockPump | Proven | Proven | Proven | Proven | Proven | Proven | Data\Video\ prefix; blocking 006286F0; no vtbl+28 |
| MuteSounds | `00CC7258` | `—` | IsFalse? | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | vtbl+2664; jmp 00CC8464; apply body UNREAD |
| StartTimeCode | `00CD1373` | `—` |  | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | and [0x13B83C8],0; leftover increment not a pose clock |
| GamePause | `00CC88D1` | `—` | seconds | WaitScaledFrames | Proven | Proven | Proven | Proven | Proven | Proven | atof * [0x124E640]=15; CLOCK path UNREAD |
| Speak | `00CC25FD` | `00CC27EA` | listener,text[,hold][,mode] | YieldAfter | Proven | Proven | Proven | Proven | Partial | Partial | vtbl+52(text,mode,0,1); leftover vtbl+104; random=1 norepeat=2 sequence=3 |
| InteractiveSpeak | `00CC2EAA` | `00CC2F50` | listener,prompt[,wait] | YieldAfterUnlessWait | Proven | Proven | Proven | Partial | Partial | Partial | vtbl+1456 handle; TRUE leftover 1472; FALSE one vtbl+28 |
| DialogSpeak | `00CC3165` | `00CC31BC` | listener,text | YieldAfter | Proven | Proven | Proven | Partial | Partial | Partial | vtbl+1456 handle; one vtbl+28; 1472 unread UI |
| DialogadSpeak | `00CC3354` | `—` | target,text[,mode] | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | no vtbl+28; father +52 stub; no dialogue UI |
| WaitTask | `00CC0783` | `—` | name | YieldAfter | Proven | Proven | Proven | Partial | Partial | Partial | poll vtbl+104 leftover; entity task slot |
| WaitActiveDialog | `00CC656B` | `00CC6612` |  | YieldAfterOrWait | Proven | Proven | Proven | Proven | Partial | Partial | [ebp-44]==0 continue; else leftover +28 then [0x13D2838]+5 → next |
| WaitPlayAnimation | `00CC2518` | `—` |  | YieldAfter | Proven | Proven | Proven | Partial | Partial | Partial | poll current entity anim task |
| SneakTo | `00CC0CB5` | `00CC0E5A` | marker[,speed][,wait] | YieldAfterOrWait | Proven | Proven | Proven | Proven | Partial | Partial | thing vtbl+20 is 004C72B0 stub; dest+gait via vtbl+16 006A9960; TickMove |
| WalkTo | `00CC083D` | `00CC09E2` | marker[,speed][,wait] | YieldAfterOrWait | Proven | Proven | Proven | Proven | Partial | Partial | 012457FC/0127293C +20=004C72B0; dest+gait 006A5D90 or[this+146],2; no warp |
| RunTo | `00CC0A79` | `00CC09E2` | marker[,speed][,wait] | YieldAfterOrWait | Proven | Proven | Proven | Proven | Partial | Partial | same dest+gait slot as WalkTo; mode 1 |
| PlayCombatAnimation | `00CC15E3` | `—` | name[,flags] | YieldAfter | Proven | Proven | Proven | Partial | Partial | Partial | vtbl+76 does not read name; no TURNING_AC90 pose |
| PlayCombatAnim | `00CC15E3` | `—` | name[,flags] | YieldAfter | Proven | Proven | Proven | Partial | Partial | Partial | exe token alias of PlayCombatAnimation |
| Create | `00CCC246` | `00CCC3E6` | type,marker,name[,extra][,suffix][,unique][,IsFalse] | CompleteNow | Proven | Proven | Proven | Proven | Partial | Partial | vtbl+364 008A9100 at marker; extras 008ADF90; unique skip; jmp 00CD17F8 |
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
| CameraRotateThing | `00CCA5B6` | `00CCA712` | thing,param,x,y,z | YieldAfter | Proven | Proven | Proven | Partial | Partial | Partial | 5 required; vtbl+1616(thing,xyz,param); jmp 00CC907D |
| CameraLookBetween | `00CCAA6C` | `00CCADB9` | a,b,mode,dur[,offA][,offB] | YieldAfter | Proven | Proven | Proven | Partial | Partial | Partial | 4 required; vtbl+1632(posA+off,posB+off,dur,-1); yield if [ebp+103] |
| CameraFOVLookBetween | `00CCB479` | `00CCB728` | a,b,mode,dur[,fovDeg] | YieldAfter | Proven | Proven | Proven | Partial | Partial | Partial | same vtbl+1632; arg4 degrees*1/360 or -1; yield if [ebp+103] |
| CameraFOVLookBetweenPos | `00CCB07C` | `00CCB42C` | a,b,pos,dur[,xyz/fov] | YieldAfter | Proven | Proven | Proven | Partial | Partial | Partial | vtbl+1636(posA,posB,camPos+off,dur,fov); yield if [ebp+103] |
| CameraPath | `00CCAF1D` | `00CCB048` | a,b,c,d,dur | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | 4 thing lookups; vtbl+1640(pos0,pos2,pos1,pos3,dur); jmp 00CC864B |
| SetLightScene | `00CD1425` | `00CD172A` | index | YieldAfter | Proven | Proven | Proven | Partial | Partial | Partial | atoi; +84 defs NAME:r,g,b; +96 comma indices; vtbl+2180; yield if [ebp+103] |
| TintScreenOut | `00CD11D0` | `00CD11F7` | seconds | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | atof; vtbl+2704([ebp-112],dur); clear hold; jmp 00CD17FD |
| TintScreenTo | `00CD0CE4` | `00CD115A` | 5floats,rgb,filter | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | 7 required; RGB*1/255; ALL:/ALLDEF: vtbl+300/320; vtbl+2700 handle |
| PutUpYourSwords | `00CC9303` | `—` | IsFalse? | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | token 00CC9303; sheathe apply unread |
| RegisterActor | `00CC662D` | `00CC669B` | name | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | empty skip; 004AC860 register; jmp 00CC7081 |
| CrowdAcquire | `00CCCEA7` | `00515700` | type,alias | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | real members only as alias0..n |
| CrowdClearActions | `—` | `—` | crowd | CompleteNow | Proven | Proven | Proven | Partial | Partial | Partial | clear member entity tasks |
| GiveHero | `00CC6392` | `00CC63E5` | item[,n][,extra][,silent][,yield] | YieldAfterUnlessFalse | Proven | Proven | Proven | Proven | Partial | Partial | vtbl+484 x (count-have); already-have skip; leftover if arg4&&!arg3 |
| GiveHeroHealth | `00CC62A0` | `00CC62F5` | amount|MAX | CompleteNow | Proven | Proven | Proven | Proven | Partial | Partial | MAX: vtbl+1032-vtbl+1028; else atof; vtbl+1052(amt,1,0); jmp 00CC7081 |
| GiveHeroMorality | `00CC6222` | `00CC6281` | amount | CompleteNow | Proven | Proven | Proven | Proven | Partial | Partial | atof arg0; vtbl+624(amount); jmp 00CC7081; scale unread |
| GiveHeroExpression | `00CC6132` | `00CC6185` | name[,flag][,param] | CompleteNow | Proven | Proven | Proven | Proven | Partial | Partial | 007ADB30 lookup; miss skip; vtbl+900(name,esi,flag); jmp 00CC2C6B |
| TakeFromHero | `00CCFB51` | `00CCFBA3` | item | CompleteNow | Proven | Proven | Proven | Proven | Partial | Partial | vtbl+556(name); jmp 00CD17FD; not TakeObjectFromHero |
| PutInHeroHands | `00CCFBCA` | `00CCFC20` | item[,NAME] | CompleteNow | Proven | Proven | Proven | Proven | Partial | Partial | NULL vtbl+572 empty; NAME vtbl+572(name); else thing vtbl+568(1,1) |
| SetHeroWeapon | `00CCFD57` | `00CCFDA9` | item | CompleteNow | Proven | Proven | Proven | Proven | Partial | Partial | arg0 required; vtbl+488(name); jmp 00CD17FD; not PutInHeroHands 572 |
| RemoveHeroWeapons | `00CC90B4` | `00CC9106` | IsFalse? | CompleteNow | Proven | Proven | Proven | Proven | Partial | Partial | IsFalse(arg0) vtbl+560 else vtbl+552; jmp 00CD17FD; bag body unread |
| UseTheme | `00CCFA38` | `00CCFA8B` | name[,param][,flag] | CompleteNow | Proven | Proven | Proven | Proven | Partial | Partial | RESET vtbl+2628(param); else vtbl+2624(name,param); jmp 00CD17FD |
| SetDoorOpen | `00CC8A8D` | `00CC8BEB` | name,IsFalse? | CompleteNow | Proven | Proven | Proven | Proven | Partial | Partial | IsFalse(arg1)->vtbl+1704 close else vtbl+1700 open; jmp 00CD17F8 |
| SetChestOpen | `00CC8C14` | `00CC8D73` | name,IsFalse? | CompleteNow | Proven | Proven | Proven | Proven | Partial | Partial | IsFalse(arg1)->vtbl+1744 close else vtbl+1740(thing,0) open; jmp 00CD17F8 |
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
