# Intro leftover `Hero.SneakTo` (`00CBFB7D` / `00CC0CB5`)

Investigation only. No production `src/` edits.

Do **not** start this at Leave / Init Game / first no-save Present.
Runner `00CBFB7D` is later leftover `Q_NewOakValeIntro`
(`00DABAC0` → TNG `NOVI_LiveFather` → `00DB86B0` → `00CBFB7D`).
`FirstSeenCallsPlayAnimationDispatcher=false`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE** / **INVENTED**.

Sources:

- listing `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00cc0000.txt`
  (`00CC0CB5`–`00CC0F2A`)
- dumps `script-runtime/sneakto-token-00cc0cb5-00cc0cb5.md`,
  `sneakto-apply-00cc0e5a-00cc0e5a.md`,
  `sneakto-yield-once-00cc0e96-00cc0e96.md`,
  `sneakto-wait-poll-00cc0f1a-00cc0f1a.md`,
  `sneakto-wait-yield-00cc0ecd-00cc0ecd.md`,
  `sneakto-thing-vtbl20-stub-004c72b0-004c72b0.md`,
  `vtbl-player-thing-vtbl-012457fc-012457fc.md`,
  `thing-activate-wrapper-00664370-00664370.md`
- def `script-bank/0481-cs-oakvale-intro-father.md`
- `RegionTravel.cs` SneakTo constants; `ScriptCommandMap` Spec
- `EntityDispatcher.cs` / `MovementRuntime` / `EntityTaskQueue.TickMove`
- `WorldSceneTests` (`Hero_SneakTo_*`, intro pump);
  `ScriptRuntimeArchitectureTests.WalkTo_writes_destination_and_entity_task`
- siblings `proofs/entity-task-queue/`, `creature-move-first/`,
  `script-entity-cmds/`, `audit-scriptscheduler/`

---

## Verdict

**Intro `SneakTo` is a leftover opcode that looks up a marker,
pushes sneak mode 2, and calls a stub. It does not walk a path,
play a sneak clip, or swap a mesh.**

| Question | Answer | Class |
|---|---|---|
| Site in intro? | `CS_OAKVALE_INTRO_FATHER` vector 0, three `Hero.SneakTo` lines; token `00CC0CB5` inside runner `00CBFB7D` | **PROVEN** leftover |
| Path / A\* / navmesh step? | dest lookup `vtbl+280/288`; apply is thing `vtbl+20` `004C72B0` (`al=1; ret 4`) | **DISPROVEN** as locomotion |
| Sneak anim / gait clip? | mode `2` is an argument to the stub; no `PlayAnimation` / XSEQ | **DISPROVEN** |
| Mesh swap? | none | **DISPROVEN** |
| First-seen mesh XYZ change from this verb? | `FirstSeenSneakToAppliesMove=false` | **PROVEN** absence |
| Visual land on `MK_OVIF_HERO4` / `HERO5`? | later `.Teleport`, not this verb. Skip-list `Hero.Teleport MK_OVIF_HERO5` is vector 1 | **PROVEN** leftover Teleport; **DISPROVEN** as SneakTo |
| Host coverage? | parse / dispatch / one leftover yield **PROVEN**; `TickMove` lerp + `ResolveSpeed(0→0.3)` **INVENTED** | **PARTIAL** apply / runtime |

---

## 1. Site

### 1a. Runner join

```
00DB86B0  start CS_OAKVALE_INTRO_FATHER
00CBFB7D  CCutsceneDef walk of def+60 CString vector
00CC0CB5  token ".SneakTo"  (00BFEAF8; persist 0x012C25D8)
00CC0E5A  apply  call [edx+20]
00CC0E96  FALSE wait: one [0x143E8F8] vtbl+28 → 00CC7081
00CC0F1A  TRUE wait: thing vtbl+104 leftover
00CC0ECD  TRUE leftover yield then idle 00CC7081
```

Empty actor (`ebx==0`) or empty marker (`ebp+40`) → `jmp 00CC7081`.
**PROVEN.**

### 1b. Intro lines (`CS_OAKVALE_INTRO_FATHER`)

Vector 0 (`def+60`; last persist-vector-0 command is the TRUE sneak):

| Order | Raw | Wait | Marker |
|---:|---|---|---|
| after `Hero.WaitTask FOO` | `Hero.SneakTo MK_OVIF_HERO4,0.0,FALSE,FALSE,FALSE` | no | `MK_OVIF_HERO4` |
| after `NoLoadUseCamera CAM_OVIF_SHOT6` | `Hero.SneakTo MK_OVIF_HERO5,0.0,FALSE,FALSE,FALSE` | no | `MK_OVIF_HERO5` |
| last of vector 0 | `Hero.SneakTo MK_OVIF_HERO5,0.0,TRUE` | leftover poll | `MK_OVIF_HERO5` |

Pins: `RegionTravel.IntroSneakMarker`, `IntroSneakWaitMarker`,
`IntroCutsceneLastCommand`, `IntroSneakSpeed=0`.

Nearby lines that are **not** SneakTo:

| Raw | Role |
|---|---|
| `Hero.PlayAnimation CS_WAKING_UP_*` / `CS_TIRED` / `CS_LOOK_LEFT` / `ST_IDLE_SUBTLE` | anim slot `vtbl+72` |
| `Father.PlayCombatAnimation TURNING_AC90` | combat `vtbl+76`; name unread |
| `VILL1.WalkTo MK_OVI_ID_VW1` | same apply stub, mode 0 |
| `Hero.Teleport MK_OVIF_HERO4` | snap after FALSE sneak 4 |
| vector 1 `Hero.Teleport MK_OVIF_HERO5` | skip-list snap; `FirstSeenCutsceneVector1AutoRuns=false` |

---

## 2. Native behaviour (`00CC0CB5`)

### 2a. Parse

```
default speed [ebp-1664] = 0x3E99999A   ; 0.3 if arg1 empty
arg1 atof 0099E690                      ; intro writes 0.0
arg2 IsTrue  00CBEDBA → wait [ebp+127]
arg3 IsFalse 00CBEE0C → [ebp-1656]=0    ; default 1
arg4 IsTrue  → [ebp-1648]=1             ; default 0
arg5 IsFalse → [ebp-1640]=0             ; default 1
push 2                                  ; SneakToMode (WalkTo 0, RunTo 1)
```

First-seen wait is false (`0.0,FALSE,FALSE,FALSE` so arg2/arg3
IsTrue is false). Last line arg2 `TRUE` takes the leftover poll.

### 2b. Path — dest lookup, not a walk

```
thing vtbl+48  00664370   ; component wrap (type 0x31), not A*
context vtbl+2048
if marker CString already matches:  context vtbl+280
else: or [ebp+92],16 ; context vtbl+288(marker → stack obj)
then:
  push flags, fld speed, push 0, push 2, push dest
  call [edx+20]           ; 00CC0E5A
```

Player `CThingPlayerCreature` `012457FC`:

| Off | VA | This opcode |
|---|---|---|
| +16 | `006A9960` | dest+gait sibling. **Not** the SneakTo call |
| +20 | `004C72B0` | **this apply**: `mov al,1; ret 4` |
| +24 | `004C72C0` | FollowNavRoute, not SneakTo |
| +48 | `00664370` | dest-side wrap before `+2048` |
| +104 | `006A9550` → `00661A40` | TRUE leftover busy poll (`ret 4`) |

`004C72B0` pops one stdcall dword and returns success. It does
not store XYZ, does not `or [this+146],2`, does not copy gait
`[+176]`, does not sample an XSEQ, does not call `006CC800` /
`CTCCreatureNavigation`. **PROVEN** stub.

Pairing “SneakTo dest+gait = `006A9960`” is a **sibling slot**,
not this handler. `COMMAND_MAP.generated.md` Apply=`00CC0E5A`
is the `vtbl+20` call. Mesh move via `+16` on this intro line
is **UNREAD** as an actual `E8`; the recovered apply is the stub.

No A\* query, no nav route, no spline. Marker name only.

### 2c. Anim — none

SneakTo does not call thing `vtbl+72` (`004C7470` PlayAnimation)
or `+76` / `+80`. Mode `2` is the gait enum pushed to the stub
and discarded. Intro sneak clips (`CS_WAKING_UP_*`, `CS_TIRED`,
`CS_LOOK_LEFT`, `ST_IDLE_SUBTLE`) are **separate** `.PlayAnimation`
lines. **DISPROVEN** as a sneak walk cycle.

### 2d. Mesh swap — none

No `SetMesh` / appearance / child-mesh change on this token.
Hero Thing stays the intro bind (not Lookout adult 4299).
`Create CREATURE_OAKVALE_VILLAGER_FEMALE_NORMAL_MESH,…,VILL1`
is a different verb. **DISPROVEN.**

### 2e. Return

| Line | Branch | Effect |
|---|---|---|
| FALSE wait | `00CC0E96` | if `[ebp+103]` one `vtbl+28`; then idle `00CC7081` |
| TRUE wait | `00CC0F1A` `call [eax+104]` | leftover busy → `00CC0ECD` one `vtbl+28` then idle |
| skip-key during wait | `00CBEB7E` | `jne 00CC7081` |

`FirstSeenSneakToTruePollsArrival=true` and
`FirstSeenSneakToTrueYieldsOnce=true` mean **one leftover poll**,
not “block until the mesh arrives.” Arrival is **DISPROVEN**
because apply never started a move.

---

## 3. Host coverage

Script-layer (`EntityDispatcher` + interpreter yield):

| Dimension | Host | Native | Class |
|---|---|---|---|
| Token / parse marker,speed,wait | `ParseSneakTo` / `ScriptLine` | `00CC0CB5` | **PROVEN** |
| Dispatch family Entity | `target.verb` | `00CC707C` join | **PROVEN** leftover |
| Empty marker continue | `CommandResult.Continue` | `00CC7081` | **PROVEN** |
| FALSE wait | `YieldOnce` `"SneakTo vtbl+20 stub"` | `00CC0E96` | **PROVEN** leftover |
| TRUE wait | `YieldOnce` `"SneakTo TRUE leftover once"` | `00CC0F1A` once | **PROVEN** leftover; **DISPROVEN** as arrival wait |
| Record `ScriptSneakTo` | `Movement.Sneaks` | n/a | host log |
| Apply stub / no mesh | pin `FirstSeenSneakToAppliesMove=false` | `004C72B0` | **PROVEN** pin |

Apply / runtime gaps:

| Host | Native | Class |
|---|---|---|
| `Movement.Sneak` → `EntityTaskKind.Sneak` | no C# task; Thing slot | **DIVERGE** object |
| `SeedStart` copies Thing XYZ into `World.Positions` | placement seed | **PARTIAL** |
| `ResolveSpeed`: script `0.0` → **0.3** | intro stores **0.0** on the stub | **INVENTED** |
| `TickMove` lerp `World.Positions` by `Speed*dt` | stub does not write XYZ | **INVENTED** |
| `WorldGeometry.ApplyActorPositions` writes Thing | dest sibling `006A9960` **UNREAD** here | **DIVERGE** vs this opcode |
| WalkTo TRUE → `WaitOperation`; SneakTo TRUE → `YieldOnce` | both leftover `+104` | WalkTo wait **DIVERGE** vs SneakTo leftover |
| `IScriptHost.SneakTo(..., dest: null)` | unused when interpreter uses dispatcher | dead record path |

`COMMAND_MAP`: Parse/Dispatch/Return **Proven**; Apply/Runtime
**Partial**. Overall leftover **PARTIAL**.

`TickMove` is the same invented integrator already classified in
`proofs/audit-scriptscheduler/` and `entity-task-queue/`. Do not
treat a moving host Hero during the intro pump as native sneak.

---

## 4. What actually places the intro Hero

| Event | Moves mesh? |
|---|---|
| `Hero.SneakTo MK_OVIF_HERO4,0.0,FALSE…` | **no** |
| `Hero.Teleport MK_OVIF_HERO4` | **yes** (teleport apply `0089B780`) |
| `Hero.SneakTo MK_OVIF_HERO5,0.0,FALSE…` | **no** |
| `Hero.SneakTo MK_OVIF_HERO5,0.0,TRUE` | **no**; leftover yield ends vector 0 |
| skip vector 1 `Hero.Teleport MK_OVIF_HERO5` | **yes** if skip fires; first-seen skip **false** |

So the recovered “path” of the intro sneak is: **record a dest
name, yield once, let a later Teleport snap.** Gait 2 never
reaches a clip or a mesh.

---

## Classifications (short)

1. **Site — leftover `00CC0CB5` in `00CBFB7D` on
   `CS_OAKVALE_INTRO_FATHER`.** Three Hero lines; last is
   `MK_OVIF_HERO5,0.0,TRUE`. **PROVEN.**
2. **Path — marker lookup + mode 2 into `vtbl+20` stub.
   DISPROVEN as nav / A\* / XYZ step.**
3. **Anim — none. DISPROVEN.** Intro clips are other verbs.
4. **Mesh swap — none. DISPROVEN.**
5. **Host — parse/yield leftover **PROVEN**; `TickMove` +
   `ResolveSpeed(0→0.3)` + Thing write-back **INVENTED** /
   **DIVERGE** vs `004C72B0`.**

Do not start New Game locomotion at `Hero.SneakTo`.
Do not pair this opcode to a sneak XSEQ or a crouch mesh.
Keep `FirstSeenSneakToAppliesMove=false`.
