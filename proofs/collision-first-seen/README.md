# First-seen collision after Leave, and Oakvale intro

Investigation only. No production `src/` edits.

Do **not** invent Unity-style physics (rigidbodies, capsules,
sweeps, generic gravity). Native first-seen after Leave is
**pose persist + a Thing bitset collect**, not a collision
solver. Childhood barrel / gold / bully **deeds** sit on the
later leftover `Q_NewOakValeIntro` / `00DBDE40` watchers, not
Lookout first Present.

Do **not** start at Oakvale / `00DBDE40` / `WatchBarrels` as
the first no-save scene. That fiber is leftover
(`proofs/00DBDE40-host-gap`, leftover **#4**). Native first
authored region is **LookoutPoint**.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Authority: listings `006A80A0` (`listing-00680000.txt`),
`0048D400` (`listing-00480000.txt`), `004EE790` /
`004D297B` (`listing-004c0000.txt`), `00CBE2FF` /
`00CC409B` / `00CC8348` (`listing-00c80000.txt` /
`listing-00cc0000.txt`), `00DBE890` / `00DBE2E0`
(`listing-00d80000.txt`);
`proofs/audit-worldgeometry`, `proofs/creature-move-first`,
`proofs/first-region-after-leave`, `proofs/script-waituntil`,
`proofs/00DBDE40-host-gap`, `proofs/004EE23F-fifth-class`;
`docs/runtime/FORWARD_TREE.md` §9; `docs/PARITY.md` no-save
enqueue; `EngineLifecycle.CollectThingsBitTestFn`;
`WorldGeometry.ObjectTransform`; `RegionTravel.WatchBarrels*`;
ExeIndex `watchbarrels-callback-00dbe890`,
`watchforgotgold-00dbe2e0`.

---

## Verdict

**After Leave there is no recovered world collision tick.**
`CTCPhysicsStandard` is a **136-byte pose component**.
`006A80A0` is a **bitset test on `thing+32`**, not a hit.

Childhood barrel smash / gold pickup / bully proximity are
**Oakvale leftover watchers + script distances**, not Lookout
physics. Host models **pose and script radii**. Host does
**not** model a solver those deeds can bounce off.

| Question | Answer | Class |
|---|---|---|
| Frontend collision / physics tick? | **No.** 2D UI. No Things. | **DISPROVEN** |
| First physics *object* after Leave? | Type row `CTCPhysicsStandard` `004EE790` / factory `004D297B` (`00BFEA1A(0x88)` → `00723FD0`). Hero later adds **`CTCPhysicsControlled`**. | **PROVEN** type; live instance **PARTIAL** |
| First *pose* used as world XYZ? | TNG `CTCPhysicsStandard.Position*` + `RHSetForward/Up` (`00724290` persist `+80/+92`). Lookout props **PROVEN**. Lookout AI **no** Standard XYZ. | **PROVEN** / **UNREAD** AI |
| First `006A80A0` after Leave? | Inside `0048D400` after each `00501450` `00500540(i,0,0)`. Bit **`0x64`**. `thing+32` dword array. | **PROVEN** call; bit **meaning UNREAD** |
| First locomotion / mesh step? | **None.** `creature-move-first`. | **PROVEN** absence |
| First *script* distance? | `.WaitForUnderRadius` `00CBE2FF`: both `vtbl+300` then pos `vtbl+24`; success iff `dist^2 < r^2` (strict, `fnstsw` `test ah,0x41`). | **PROVEN** opcode; **DISPROVEN** as Leave |
| Oakvale barrels / gold / bully as Leave collision? | **No.** `WatchBarrels` `00DBE890`, `WatchForGotGold` `00DBE2E0` after `CREATURE_HERO_CHILD` on `00DBDE40`. | **LEFTOVER** |
| Unity-style collider on barrels/gold/bully? | **Not recovered.** Do not invent. | **UNREAD** solver; **DISPROVEN** as first-seen |

---

## Timeline (no-save New Game)

```
0042EC7C retail
  0042DF9E 2D UI                         // no Things, no 006A80A0
0042F2A2 Leave
  FinalAlbion.wld
0042F491 Init Game
  004EE23F Init Thing Components
    004EE790  CTCPhysicsStandard  004D2EF0 / 0x4D297B
    004EE80C  CTCPhysicsControlled
    004EE87C  CTCCreatureNavigation
    // not a solver; not a region
  00A15670  CNavigatorManager            // register A*, not a step
  00416953 Load world
    004FDBC0 LookoutPoint.tng parse
004189C2 first pumps
  dummy WorldMap+156=0
  no 00501450, no 006A80A0, no WalkTo    PROVEN
later (E8 of 00501450 UNREAD)
  00501450
    for i=1..141  00500540(i,0,0)
      i=1 LookoutPoint
        006C2170 topology then objects
        0051FD80 Things (pose persist)
        006AC910 CREATURE_HERO @ GuildArrivalHSP
          004C9D60("CTCPhysicsControlled")
      after each i:
        0048D400  +145 need 0x0C forbid 0x21
                  006A80A0 bit 0x64 thing+32     PROVEN
        005198B0  CTCActionUseScriptedHook
  // still no dist^2 test, no WatchBarrels
```

`00DBDE40` / `WatchBarrels` / `WatchForGotGold` /
`NOVI_Bully` are **not** on this list. **PROVEN.**

---

## 1. `CTCPhysicsStandard` — pose, not a collider

### 1a. Type row (Init Thing Components, after Leave)

`listing-004c0000.txt` / `004EE23F-fifth-class`:

```
004EE720  push "CTCPhysicsLight"        ; 004D2EF0 / 0x4D294B
004EE790  push "CTCPhysicsStandard"     ; 004D2EF0 / 0x4D297B
004EE80C  push "CTCPhysicsControlled"   ; 004D2EF0 / 0x4D29AE
004EE87C  push "CTCCreatureNavigation"  ; 004D2EF0 / 0x4D291B
004EE8F8  push "CCreatureNavigationDef" ; next 009B0AC0
```

Factory `004D297B`:

```
004D297B  push 0x88
          call 00BFEA1A
          call 00723FD0                 ; ctor
004D29AE  push 0xAC → 00724F50          ; CTCPhysicsControlled
004D29E1  push 0xB0 → 007266C0          ; CTCPhysicsNavigator
```

| Claim | Class |
|---|---|
| `CTCPhysicsStandard` is the next `009B0AC0` / Add Def Class | **DISPROVEN** (CTC `004D2EF0` row) |
| Alloc size `0x88` / ctor `00723FD0` | **PROVEN** |
| `CPhysicsDef` load-payload / field walk | **UNREAD** (`004EE23F-sixth-class`) |
| This row is a collision world / broadphase | **DISPROVEN** (name table only) |

### 1b. Persist = TNG pose (`00724290`)

Investigations `D-c3d-transforms` / `2026-08-18-static-c3d`:

| Field | Native | Host |
|---|---|---|
| `PositionX/Y/Z` | `ThingFile` | `ThingInstance.Position*` **PROVEN** |
| `RHSetForward*` | physics `+80/+84/+88` | `ObjectTransform` / `CopyPhysicsAxes` **PROVEN** |
| `RHSetUp*` | physics `+92/+96/+100` | same **PROVEN** |
| `ObjectScale` | `006A5D90` `[+156]=1.0` | `0.01 * scale` **PROVEN** host; native multiply site **UNREAD** |

`WorldGeometry.ObjectTransform` builds a 3×4 from that basis
(`right = forward × up`). `audit-worldgeometry`: that is
**C3D submit**, not landscape, **not** a hit.

Lookout Graphic props (185): Standard pos + RH **PROVEN**.
Lookout 9 `AICreature`s: TNG has **no**
`CTCPhysicsStandard.Position`. Host `ResolveSubmit` drops
them. Native `0051FD80` still constructs them; first-seen
world pose writer **UNREAD** (`creature-move-first` §3b,
`creature-ai-first`: villagers use `CTCPhysicsNavigator`
XYZ — host dump `pos=-` is **DIVERGE** field pick).

Hero after `006AC910`: **`CTCPhysicsControlled` only named
add**. `CTCCreatureNavigation` on create **DISPROVEN**.
HSP copy is **placement**, not a step.

### 1c. What this is **not**

No recovered:

- radius / AABB / capsule on the `0x88` object used as a
  blocker
- tick that rejects hero XYZ against barrels
- contact manifold

`006CC800` (`CTCCreatureNavigation` tick) is vtbl-only; first
call **UNREAD**; **DISPROVEN** as Leave.

**Do not invent a solver from the component name.**

---

## 2. `006A80A0` bits — collect filter, not collision

### 2a. Body (**PROVEN** listing)

`ecx` = 4-byte index object. Arg0 = Thing.

```
006A80A0  mov eax, [ecx]            ; bit index
          cmp eax, 0x112            ; 274
          jb  006A80BB
          push "bitset"             ; 00BFEAF2 assert
006A80BB  mov ecx, eax
          and ecx, 31
          mov edx, 1
          shl edx, cl
          mov ecx, [esp+4]          ; thing
          shr eax, 5
          test [ecx+eax*4+32], edx  ; thing+32 dword[]
          setne al
          ret 4
```

First-seen index after Leave is **`0x64`** (100): dword 3,
mask bit 4, byte `thing+44`. **Meaning of that bit UNREAD.**
Do not name it “collidable.”

### 2b. First call after Leave — `0048D400` on `00501450`

`listing-00480000.txt` `0048D400`:

```
0048D400  call 0049C770             ; [map+8]+32 list
          for each node:
            al = [thing+145]
            test al, 0x04  je skip  ; need 0x0C
            test al, 0x08  je skip
            not cl; test cl, 0x01  je skip   ; forbid bit0
            test al, 0x20  jne skip          ; forbid 0x21
            ecx = bit-index (0x64)
            push thing
            call 006A80A0
            test al, al    je skip
            append thing
```

Host constants (`EngineLifecycle`):

| Name | Value |
|---|---|
| `CollectRegionThingsFn` | `0x0048D400` |
| `CollectThingsListFn` | `0x0049C770` |
| `CollectThingsBitTestFn` | `0x006A80A0` |
| `CollectThingsBitIndex` | `0x64` |
| `ThingCollectFlagsOffset` | 145 |
| `ThingCollectFlagsNeed` | `0x0C` |
| `ThingCollectFlagsForbid` | `0x21` |

`LoadFromFirstRealRegion` **Notes** those VAs. It does **not**
walk `+145` or `thing+32`. Occupancy of the collected list
**PARTIAL** (`FORWARD_TREE` §9; `+145` ctor **UNREAD**).

Sibling collector `005198B0` / `00518DC0`
`CTCActionUseScriptedHook` key `0xC2` is **PROVEN** name,
**DISPROVEN** as a release of the `0048D400` list
(`docs/status/README.md`). **DISPROVEN** as childhood Use.

Frontend / dummy `004189C2`: **no** `0048D400`. Exact `E8` of
`00501450` **UNREAD**.

---

## 3. `audit-worldgeometry` — draw AABB ≠ physics

`proofs/audit-worldgeometry`:

| Host / native | Physics? | Class |
|---|---|---|
| `00BDC2D0` four-plane AABB then 72-byte cells | **No.** Frustum / landscape DIP | **PROVEN** draw |
| `WorldGeometry.ObjectTransform` Standard pose | **No.** C3D instance 3×4 | **PROVEN** pose |
| `FirstSceneWorld` Oakvale soup | leftover intro contract | **DISPROVEN** as Leave |
| Concat land+C3D+sky | not native layers | **DIVERGE** |
| Game `00B27D90` after Leave | **UNREAD** | — |

Do not reuse patch AABB as barrel/hero collision.

---

## 4. `creature-move-first` — no step to collide

`proofs/creature-move-first`: after Leave, navigator
**registers**, creatures **place**, locomotion **absent**.

| Slot | VA | First-seen |
|---|---|---|
| dest `vtbl+16` | player `006A9960` / AI `008315C0` | unused |
| WalkTo / SneakTo `vtbl+20` | `004C72B0` `mov al,1; ret 4` | stub |
| FollowNavRoute `vtbl+24` | `004C72C0` same | stub |
| `00CBFB7D` | interpreter | **off** until leftover Oakvale |

Host `EntityTaskQueue.TickMove` lerp of `World.Positions` is
**LEFTOVER** vs mesh (`entity-task-queue`). Without a step
there is nothing for a solver to reject on this spine.

---

## 5. Interaction distances from script vtbl

### 5a. `.WaitForUnderRadius` — recovered distance test

Token `00CC4045` / apply `00CC409B` / leftover `00CC40CE`.

```
00CC409B  arg0 + arg1 required else 00CC7081
          0099E690 atof → [ebp+32] radius
00CBE2FF  ecx,edx = two Things
          both vtbl+300 else fail
          both vtbl+24 → XYZ
          dx,dy,dz; dist^2 cmp r^2
          fnstsw; test ah, 0x41
          jne fail                 ; NOT (dist^2 < r^2)
          al = 1                   ; strict inside
```

`00CBEB7E` skip latch. Host `WorldRuntime.IsUnderRadius` uses
`Positions` `LengthSquared() < r*r`. **MATCH** inequality.
**PARTIAL** vs native (`vtbl+300` validity not modelled).

| First-seen after Leave? | **DISPROVEN** (`script-waituntil`: no leftover intro line; `HasStarted` false) |
| Oakvale leftover intro dumps? | **DISPROVEN** (father/Theresa have `WaitTask` / speak, not this) |
| Bank example | `CS_MOTHER_SPLITTING_UP_WORKING`: `Hero.WaitForUnderRadius OUTRO_MARKER_HERO,2.2` **LEFTOVER** later CS |

Do not hard-code `2.2` as childhood barrel radius. The
**opcode** takes the script float.

### 5b. Gold count is **not** a distance

`GiveGold` apply `00CC8348`: atoi, lookup `"Gold"`
`00515700`, `vtbl+504(requested-have)`. Host
`WorldRuntime.GiveGold` **MATCH** “ensure at least.”
**DISPROVEN** as Leave (`hero-stats-first`).

`WatchForGotGold` `00DBE2E0` (Oakvale leftover):

```
call [ctx+64].vtbl+508
cmp eax, 2
jg  objective
else yield vtbl+28 / 00CB7940
```

That is **inventory count > 2**, then
`TEXT_QUEST_OAKVALE_INTRO_OBJECTIVE_03` via `vtbl+2620` /
`vtbl+1184`. **Not** `dist^2`. **Not** `GiveGold`.

### 5c. Other radii (do not steal for barrels)

| Site | What | First-seen Leave |
|---|---|---|
| `CTCDRegionExit.Radius` default **3.5** XY `HitExit` | region exit | host helper; client unused (`issue-19-verify`) **LEFTOVER** |
| WatchBarrels beetle `push 0x40000000` (2.0f) `vtbl+1064` | leftover spawn scale/dist | **`FirstSeenWatchBarrelsSpawnsBeetle=false`** |
| `vtbl+2584(12.0)` | PreAttack **time**, not metres | leftover `00DBDE40` |

---

## 6. Oakvale leftover — barrels / gold / bully (not Leave)

Reached only after a proven `00CB5AD0("Q_NewOakValeIntro")`
then `00DABAC0` `E8 00DBDE40` (`00DAC295` only PE caller).
Activator **UNREAD**. Host Pump **must not** jump here.

### 6a. `WatchBarrels` `00DBE890` (first-seen *on that fiber*)

After `CREATURE_HERO_CHILD`, three `00CDD450` (0.1f / 64 / 1,
vtbl `0x012D7A3C`):

| Watcher | Callback | First-seen on fiber |
|---|---|---|
| `WatchBarrels` | `00DBE890` | **yes** |
| `WatchForGotGold` | `00DBE2E0` | **yes** |
| `ManageQuestCoreMarkers` | `00DBE4E0` | later `NOVI_*` — **do not follow** off StartOakVale |

`00DBE890`:

```
vtbl+300("NOVI_Barrel") → vector (stride 12)
setle  → keep yielding vtbl+28 until count > 0
count = (end-begin)/12
[this+116] smash latch
  edi==1     → 00DAEA70(0)                 // first smash; not beetle
  edi==n-1   → vtbl+288(NOVI_Barrel)
               vtbl+2340(OBJECT_GOLD_1)    // leftover gold spawn
  edi>n-4    → vtbl+364 NOVI_CreatedBeetle
               CREATURE_OAKVALE_STAG_BEETLE
               vtbl+1064(1, 2.0f)
```

**PROVEN** collect-by-name. **DISPROVEN** as a physics
contact. What writes `[+116]=1` (the smash) **UNREAD**.
`FirstSeenWatchBarrelsSpawnsBeetle=false` stays correct.

Host: `RegionTravel.WatchBarrels*` constants +
`ScriptFactoryTable` `NOVI_Barrel` `00DB7D00`. **No** live
`00DBE890` fiber (`00DBDE40-host-gap`).

### 6b. Gold pickup

| Path | Native | Host | Class vs deeds |
|---|---|---|---|
| Last barrel → `OBJECT_GOLD_1` `vtbl+2340` | leftover WatchBarrels | **missing** | **UNREAD** apply |
| Player walks into coin | solver **UNREAD** | none | **UNREAD** |
| `WatchForGotGold` `vtbl+508 > 2` | leftover poll | **missing** watcher | **DIVERGE** |
| `GiveGold` `vtbl+504` | script grant | `HeroGold` ensure | **LEFTOVER** opcode; **not** barrel loot |

### 6c. Bully

| Object | Where | Collision |
|---|---|---|
| Lookout `BeggarBully` | TNG `AICreature`, no Standard XYZ | pose **UNREAD**; **DISPROVEN** as Oakvale |
| `NOVI_Bully` factory | `00DABAC0` name table | **LEFTOVER** |
| `CS_OAKVALEINTRO_BRATHIT` | `TeleportThing NOVI_Bully,MK_OIBR_BULLY1` | teleport, **not** a hit |
| Proximity / punch radius | **UNREAD** | do not invent |

---

## 7. Host already models vs missing for deeds

### Already modelled (do not re-invent)

| Host | Native pairing | Class |
|---|---|---|
| `ThingFile` Standard `Position*` | TNG persist | **PROVEN** |
| `ObjectTransform` RH basis | `00724290` | **PROVEN** pose / **DISPROVEN** as hit |
| `CopyPhysicsAxes` HSP → Hero | `006AC910` edx pose | **PARTIAL** (was +Y fallback) |
| Note `004C9D60("CTCPhysicsControlled")` | `006A9EAB` | **MATCH** name |
| Note `006A80A0 bit 0x64` / `0048D400` +145 | `00501450` loop | **MATCH** Note; **no** filter |
| `IsUnderRadius` `dist^2 < r^2` | `00CBE2FF` | **MATCH** math; unused Leave |
| `GiveGold` ensure-at-least | `vtbl+504` | **MATCH** opcode; unused Leave |
| Watcher VAs / 0.1f / 64 / `NOVI_Barrel` | `00DBDE40` | **MATCH** data; **not** run |
| `TickMove` lerp | `004C72B0` stub | **LEFTOVER** |
| `HitExit` XY radius 3.5 | `CTCDRegionExit` | **LEFTOVER**; not barrels |
| Landscape frustum AABB | `00BDC2D0` | **PROVEN** draw |

### Missing for childhood deeds (barrels / gold / bully)

Stop at the first unproven item. Do **not** fill with Unity
physics.

1. **Do not run Oakvale on no-save Pump.** First Present is
   Lookout. Activator of `Q_NewOakValeIntro` **UNREAD**.
2. **`00DBDE40` fiber** after a *proven* activate: factory
   row, `00DABAC0` names, map-ready `StartOakVale`, kid
   `CREATURE_HERO_CHILD` (`00DBDE40-host-gap` steps 2–8).
3. **Live `WatchBarrels`** `vtbl+300("NOVI_Barrel")` occupancy
   + `[+116]` smash writer **UNREAD**.
4. **How a barrel dies** — not recovered as Standard vs
   Controlled contact. **UNREAD.**
5. **`OBJECT_GOLD_1` `vtbl+2340`** spawn / pickup **UNREAD**.
6. **`WatchForGotGold` `vtbl+508`** vs host `HeroGold`
   **DIVERGE** (no watcher).
7. **`NOVI_Bully` construct + any radius** **UNREAD**.
   Teleport lines are not collision.
8. **`.WaitForUnderRadius` on a real childhood line** —
   **DISPROVEN** in leftover intro dumps. Do not invent a
   metre value for “use barrel.”
9. **Bit `0x64` at `thing+32`** — keep as collect filter.
   **UNREAD** as collidable.

`00501450` `006A80A0` collect is the first *bit* after Leave.
It is **not** the childhood deed.

---

## Classifications (short)

1. **Frontend collision — DISPROVEN.**
2. **`CTCPhysicsStandard` after Leave — type `004EE790` /
   factory `0x88` `00723FD0` + TNG pose persist. PROVEN.**
   Solver / blocker **UNREAD**. Treating the name as Unity
   physics **DISPROVEN**.
3. **First `006A80A0` — `0048D400` bit `0x64` on
   `thing+32` after each `00500540`. PROVEN.** Semantic
   **UNREAD**. Host Notes only.
4. **`audit-worldgeometry` AABB — draw. DISPROVEN as
   collision.**
5. **`creature-move-first` — no mesh step. PROVEN
   absence.** Host `TickMove` **LEFTOVER**.
6. **Script distance — `00CBE2FF` `vtbl+300` + `vtbl+24`
   `dist^2 < r^2`. PROVEN opcode. DISPROVEN as Leave /
   Oakvale leftover intro line.** Host `IsUnderRadius`
   **MATCH** math.
7. **Oakvale barrels / gold / bully — leftover
   `00DBE890` / `00DBE2E0` / `NOVI_Bully`. PROVEN names.
   Contact solver UNREAD.** Beetle `2.0f` **not**
   first-seen.

Do not start childhood collision at Lookout rocks, WASD, or
a generic physics engine.
