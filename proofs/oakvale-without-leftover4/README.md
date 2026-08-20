# Wire Oakvale later without replacing Lookout first Present

Investigation only. No production `src/` or `tests/` edits.

Leftover **#4 CLEAN** dual ledger. Do **not** collapse.

- **No-save first Present:** LookoutPoint, WLD index **1**, adult
  mesh **4299**, `GuildArrivalHSP`, `006B3FF0` helper FOV **70**.
- **Childhood intro view:** `StartOakValeWest` / `HerosOldHouse` /
  `CAM_OVIF_SHOT2` / kid **4300** / `Q_NewOakValeIntro` /
  `CS_OAKVALE_INTRO_FATHER` FOV **72**.

Question: how can the host wire the Oakvale region **later**
without replacing Lookout first Present? Recover the native
switch site (region load / camera / hero mesh) that happens
**AFTER** first Present.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: `listing-00d80000.txt` `00DBDE40`–`00DBE13E`;
`listing-00500000.txt` `00501450` / `00502500` / `005064C0`;
`listing-00880000.txt` `0089B780` / `0089B99E`;
`listing-00480000.txt` `00487C20` / `004A4CB9`;
`e8.tsv` dest `00500540` (6 sites) / dest `00502500` (2 sites);
`calls-startoakvale-00dbde40` (1 hit: `00DAC295`);
`RegionTravel.cs`; `EngineLifecycle.LoadFromFirstRealRegion` /
`RequestLoadRegion` / `ApplyLoadJob` / `ApplyWorldCamera`;
`ScriptedCamera.Bind`; `FirstSceneWorld.cs`;
`EngineLifecycleTests.Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`;
`No_save_does_not_activate_Q_NewOakValeIntro`;
`CameraMatrixParityTests.ApplyWorldCamera_does_not_stomp_UseCamera_bind`;
`docs/status/README.md` leftover #4;
siblings `leftover-4-collapse-audit`, `00DBDE40-after-activate`,
`00DBDE40-host-gap`, `issue-4-verify`, `first-region-after-leave`,
`player-region-name-writer`, `cs-oakvale-intro-father-lines`.

Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** set persist `PlayerRegionName=StartOakVale`.
Do **not** fold leftover **#50** (first-proximity TNG pump)
into this leftover.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Is first no-save Present Oakvale / SHOT2 / kid 4300? | **No.** Lookout index **1**, `GuildArrivalHSP`, adult **4299**, `006B3FF0` FOV **70**. | **DISPROVEN** as first Present |
| Can the host replace `LoadFromFirstRealRegion` with Oakvale to “reach” the intro? | **No.** That is leftover #4 collapse. | **DISPROVEN** |
| Does `00DBDE40` load StartOakVale? | **No.** Context `vtbl+48("StartOakVale")` is a **ready query**. Not-ready → yield `vtbl+28` until current. | **PROVEN** wait, **DISPROVEN** as loader |
| Native later **region** switch (after first Present)? | `00502500` LoadRegionAtMap → `004FC190` map→region; if different `004FEEC0` unload then `00500540(new, …, 1)`. | **PROVEN** machinery; Oakvale-specific caller **UNREAD** |
| Native later **camera** switch? | `00DB86B0` → `00CBFB7D` `CS_OAKVALE_INTRO_FATHER` → `00CC9F3A` `UseCamera CAM_OVIF_SHOT2` → `00B23B50` bind FOV **72**. | **PROVEN** later leftover |
| Native later **hero mesh** switch? | After map-wait + AttackOver READ 0: `"CREATURE_HERO_CHILD"` context `vtbl+280` / `vtbl+376` (mesh **4300**). `004AA840` is CString dtor. | **PROVEN** later leftover |
| Does intro `Hero.Teleport MK_OVI_ID_HERO` change region? | **No.** Same-region first-seen skips `00502500` (`FirstSeenTeleportChangesRegion=false`). Region is already StartOakVale. | **DISPROVEN** as the region switch |
| Does persist `00487C20` write Oakvale on no-save? | **No.** Empty `PlayerRegionName` skips. Continue-only. | **DISPROVEN** |
| Is leftover #4 a live Present bug? | **No.** Host first Present is Lookout. #4 is ledger pairing. | **CLEAN** / **LEFTOVER** titles |

---

## Verdict

**Keep Lookout as first Present. Oakvale is a later
region / camera / hero switch, not a replacement of
`00501450(1)`.**

Native order after a *proven* `00CB5AD0("Q_NewOakValeIntro")`
(activator still **UNREAD** on no-save):

```
Lookout first Present already happened
  00501450 → 00500540(1,0,0)
  006C2170 ContainsMap LookoutPoint / Bridge / GuildExterior
  GuildArrivalHSP → 006AC910 CREATURE_HERO 4299
  006B3FF0 helper FOV 70

later (not Pump / not Leave)
  00DABAC0 registers NOVI_* then E8 00DBDE40
  00DBDE40  vtbl+48("StartOakVale")        // WAIT until current
            yield vtbl+28 while not ready
            00CB7940 abort → ret
            READ [this+80] AttackOver
            fade vtbl+1488(2.0)
            CREATURE_HERO_CHILD vtbl+280/376  // kid 4300
            three 00CDD450 watchers
            Q_NewOakValeIntro_PreAttack
            vtbl+2584(12.0)
            HerosOldHouse
            SPIN +80
  NOVI_LiveFather construct
    00DB86B0 → 00CBFB7D CS_OAKVALE_INTRO_FATHER
      UseCamera CAM_OVIF_SHOT2  00CC9F3A / 00B23B50  FOV 72
      Hero.Teleport MK_OVI_ID_HERO   same-region; no 00502500
```

The host wires Oakvale by implementing that **later**
fiber continuation once the activator is a dump site.
It does **not** change `LoadFromFirstRealRegion`,
`FirstSceneMapName`, `SpawnHeroFromPlayerStart`, or
`ApplyWorldCamera` first-seen FOV **70**.

---

## Dual ledger (do not collapse)

| Ledger | Map / spawn | Camera | Hero | When |
|---|---|---|---|---|
| **No-save first Present** | LookoutPoint, index **1**, `GuildArrivalHSP` | `006B3FF0` / `00A0C130` FOV **70** | `CREATURE_HERO` mesh **4299** | after Leave, first 3D Present |
| **Intro view** (later) | `StartOakValeWest`, persist index **4**, `HerosOldHouse` | `CAM_OVIF_SHOT2` FOV **72** | `CREATURE_HERO_CHILD` mesh **4300** | after proven activate + map-wait + `CS_OAKVALE_INTRO_FATHER` |

WLD: `World.Regions[0].Index == 1` `LookoutPoint`;
`World.Regions[3].Index == 4` `StartOakVale`. Dummy slot 0
is `005066E0`, not Lookout.

`FirstSceneWorld` / `RegionTravel.NewGameRegion` /
`FIRST_SCENE_*` remain the **intro-view fixture**. Live
`EngineLifecycle.Pump` never calls `FirstSceneWorld.Build`.
That split is **CLEAN** (`proofs/leftover-4-collapse-audit`).

---

## 1. Region — wait vs load

### Query (inside `00DBDE40`, after construct)

```
00DBDE49  push "StartOakVale"
00DBDE69  call [eax+48]          ; [esi+64] script context
          neg / sbb / inc        ; bl = !ready
00DBDE7F  je  00DBDECA           ; already ready
00DBDE81  call [eax+28]          ; yield
          call 00CB7940          ; abort → ret 00DBE2CE
          call [edx+48] again
00DBDEC8  jne 00DBDE81           ; still not ready
```

**PROVEN.** `vtbl+48` returns nonzero when StartOakVale is
ready. Zero → yield. Native construct (`00CB5AD0` /
`00DAAC00`) does **not** `00500540`. The fiber waits until
the **current** region is StartOakVale (WLD index **4**).

`00CB7940` is **not** the wait condition (`[this+44]` then
`[eax+5]`; dump name hero-exists). True → **return**, skip
kid / 12 s.

Context `vtbl+48` **body** (request vs query-only): **UNREAD**.
Treat it as a ready predicate until that dump exists. Do **not**
satisfy the wait by loading Oakvale from `ActivateQuest`.

### Load (after first Present; not `00501450(1)`)

`e8.tsv` dest `00500540` — complete set:

| Site | Parent | Role after first Present |
|---|---|---|
| `005014EC` / `00501935` | `00501450` | **First** no-save open. Loop `i=1..141`; first real `i=1` Lookout. Restore `(saved,0,1)`. **Not** Oakvale Present. |
| `00487C55` | `00487C20` persist `PlayerRegionName` | Continue. `004FC210` name → index then `00500540(index,0,1)`. Empty no-save **skip**. |
| `0050255D` / `005025F8` | `00502500` LoadRegionAtMap | **Later switch.** Map→region `004FC190`; if different: `004FEEC0` unload then `00500540(new, …, 1)`. |
| `00506455` | `005064C0` Post Region Load Villages | After a job **already** applied. Not a first open. |

`e8.tsv` dest `00502500` — two sites:

| Site | Parent | First-seen |
|---|---|---|
| `004A4CB9` | `004A3740` WorldUpdate | `[world+260]=0` **skip**. Later when that slot is set. |
| `0089B99E` | `0089B780` Teleport apply (`vtbl+1892`) | Same-region intro Teleport **jumps over** this (`0089B918` `jmp 0089BAE9` after `0049EAF0`). Cross-region Teleport takes it. |

`00502500` body (`listing-00500000.txt`):

```
0050250D  call [eax+64]            ; map handle
          if [0x13756F6]:
00502534    call 004FC190          ; target map → region
0050253E    call 004FC190          ; other map → region
00502547    je  0050257F           ; same region: no load
0050254E    call 004FEEC0(old, 1)  ; unload
0050255D    call 00500540(new, arg, 1)
```

That is the native **later** region switch: unload Lookout
ContainsMap, load StartOakVale ContainsMap
(`StartOakValeWest` / East / MemorialGarden),
`004FC8A0` writes `WorldMap+156 = 4`. First Present’s
`+156=1` is already in the past.

Who first feeds `00502500` a StartOakVale **map** after
Lookout Present: **UNREAD**. `004A3740` is skipped
first-seen. Intro Teleport is same-region. Persist name is
empty. Do **not** fill that gap with
`LoadFromFirstRealRegion` or `PlayerRegionName=StartOakVale`.

Implementable after a proven activate: a slot-2 fiber that
**yields** while `CurrentRegionIndex != 4`. When native
`00502500` / a recovered later `00500540(4)` has run,
`006C2170` applies StartOakValeWest TNG (`NOVI_LiveFather`
lives there). Host `ApplyLoadJob` already walks
`region.ContainsMaps` then `SetRegionAsLoaded`. Previous
ContainsMap teardown in `006C2170` is **UNREAD**
(FORWARD_TREE §13).

### Host region gap

| Evidence | Original | Host | Gap |
|---|---|---|---|
| `00501450` first `00500540(1,0,0)` | Lookout first Present | `LoadFromFirstRealRegion` Notes that loop; tests lock `FirstSceneMapName==LookoutPoint` | **MATCH** first Present. Do not retarget index 4. |
| `00DBDE49` `vtbl+48("StartOakVale")` | Yield until region 4 current | `ActivateQuest` “does not load StartOakVale”; `BindSqnoviFactory` no `NewRegion` | **MATCH** bind-without-region. Map-wait **not implemented**. |
| `00502500` / `004FC190` / `004FEEC0` | Later map-at-region switch | `LoadRegionAtMapFn` constant `0x00502500`; no live call from Pump | **MATCH** VA. **PARTIAL**: no later switch walk |
| `00487C20` `00500540` | Persist named continue | `LoadRegionByName` / `EnqueueAfterDummy` only if `PlayerRegionName` nonempty | **MATCH** skip on no-save |
| `FirstSceneWorld.Region = StartOakValeWest` | Intro-view fixture | Zero `Fable.Client` / Pump callers | **LEFTOVER** soup; **CLEAN** vs Present |
| Who drives `00502500(StartOakValeWest)` after Lookout | **UNREAD** | Do not invent `RequestLoadRegion(4)` from activate / Leave | **UNREAD** caller |

---

## 2. Camera — `006B3FF0` FOV 70 then `UseCamera` FOV 72

First Present (already locked):

```
Init World  006B4900 WorldCamera / 006FD8C0 GameCamera
            00A0C130 helper FOV 0x3E471B48 ≈ 70°
WorldFrame  004A5DF3 006B3FF0 seed
            0049E080 / 006B42F0 apply
Lookout submit uses GameCamera.FirstSeenFovDegrees = 70
```

**PROVEN.** `FirstSeenCallsUseCamera=false`. SHOT2 is not
this Present.

Later (after map + `NOVI_LiveFather` construct):

```
00DB86B0
  bind Hero / Father  00CD3D2E / 008ABD10
  00CBFB7D("CS_OAKVALE_INTRO_FATHER")
    PlayMusic / FadeOut / CameraPause FALSE
    Hero.Teleport MK_OVI_ID_HERO     // no region change
    DoCameraPreloading 00CBF29F      // collects UseCamera names
    …
    UseCamera CAM_OVIF_SHOT2         // 00CC9F3A
      TNG helper bind 00B23B50 push 1
      FOV 0.2 turns = 72°
```

SHOT2 is **not** in the first 20 CStrings
(`proofs/cs-oakvale-intro-father-lines`: PC 18 is
`NoLoadUseCamera CAM_OVI_ID_STANDUP`; `UseCamera CAM_OVIF_SHOT2`
is PC ≥ 25). Still **after** first Present.

Host already has the later bind without stomping first-seen:

```
ApplyWorldCamera
  if (Camera.ScriptCameraActive)
      Note "0049E080 skip helper write; UseCamera sticks"
      return
  else
      ApplyRendererHelper(hero, V4, +Z)
      SetFovDegrees(70)
```

`ScriptedCamera.Bind("CAM_OVIF_SHOT2", …, 72)` sets
`ScriptCameraActive`. Test
`ApplyWorldCamera_does_not_stomp_UseCamera_bind` locks FOV
**72** surviving `0049E080`.

Ctor default FOV **72** on `EngineLifecycle.Camera` before
Lookout seed is SHOT2 leftover on an unused object. After
`SpawnHero` the helper path writes **70**. That leftover
does **not** Present Oakvale geometry.

| Evidence | Original | Host | Gap |
|---|---|---|---|
| First Present FOV 70 `006B3FF0` / `00A0C130` | Lookout helper | `GameCamera.FirstSeenFovDegrees=70` after spawn | **MATCH** |
| `FirstSeenCallsUseCamera=false` | no `00B23B50` on no-save | no `UseCamera` from Pump | **MATCH** |
| Later `00CC9F3A` / `00B23B50` SHOT2 FOV 72 | cutscene after Father construct | `ScriptedCamera.Bind` + `UseCamera sticks` | **MATCH** bind API. **PARTIAL**: no live `00DB86B0` from Pump |
| `ScriptedCamera` ctor FOV 72 | SHOT2 constant | field exists from Bootstrap | **LEFTOVER** default; not Oakvale mesh |

Do **not** seed SHOT2 from `LoadFromFirstRealRegion`.
Do **not** change first-seen helper FOV to 72.

---

## 3. Hero mesh — adult 4299 then kid 4300

First Present (already locked):

```
LookoutPoint.tng has no PlayerCreature
HOLY_SITE_PLAYER_START GuildArrivalHSP
0049F180 / 00449D90 PLAYER_HERO miss
CREATURE_HERO → 00489D40 / 006AC910 mesh 4299
```

**PROVEN.** Tests
`DoesNotContain` `CREATURE_HERO_CHILD` / `00DBDE40`.

Later, only after map-wait **and** AttackOver READ 0
**and** `00CB7940` miss:

```
00DBDEFB  push 0x40000000          ; 2.0f fade vtbl+1488
00DBDF08  push "CREATURE_HERO_CHILD"
00DBDF24  call [eax+280]           ; context find
00DBDF33  call [edi+376]
00DBDF3D  call 004AA840            ; CString dtor, not ctor
```

Then three `00CDD450` watchers (`WatchBarrels` `00DBE890`,
`WatchForGotGold` `00DBE2E0`, `ManageQuestCoreMarkers`
`00DBE4E0`). Kid mesh **4300** `MESH_YOUNGHERO_02`.

`00CB7940` true (quest `+44` already bound) **skips** this
entire body. Semantic of `+44` vs world `CREATURE_HERO`:
**PARTIAL**. Do not assume Lookout’s adult pointer fills
quest `+44` (different object; quest is not constructed on
no-save).

| Evidence | Original | Host | Gap |
|---|---|---|---|
| Lookout `006AC910` `CREATURE_HERO` 4299 | first Present | `SpawnHeroFromPlayerStart` prefers `GuildArrivalHSP` | **MATCH** |
| `00DBDF08` `CREATURE_HERO_CHILD` lookup | after map-wait | constants on `RegionTravel`; no live call | **MATCH** VAs. **PARTIAL**: no fiber |
| `004AA840` as spawn | CString dtor `0099A2E0` | not used as ctor | **DISPROVEN** as spawn |
| `FindPlayerStart` ranks `NOVStartHSP` | intro HSP | unused on no-save spawn | **LEFTOVER** helper |

Do **not** spawn 4300 at GuildArrivalHSP.
Do **not** replace `CREATURE_HERO` with `CREATURE_HERO_CHILD`
in `SpawnHeroFromPlayerStart`.

---

## How the host wires Oakvale later (without leftover #4 collapse)

Ordered. Stop at the first unproven item.

1. **Leave first Present alone.** `LoadFromFirstRealRegion` /
   `00500540(1,0,0)` / `GuildArrivalHSP` / 4299 / FOV **70**.
   `FirstSceneMapName` stays `LookoutPoint`.
2. **Do not invent the activator.** No
   `ActivateQuest("Q_NewOakValeIntro")` from Leave / Pump /
   `user.ini` / `FirstSceneWorld`. Activator
   `004B4A10` / `00CB5AD0` of this name is **UNREAD** on
   no-save (`proofs/oakvale-later-activate`,
   `q-novi-activator-callers`).
3. **After a proven activate**, schedule S_QNOVI slot 2
   (`00DABAC0` → `00DBDE40`). Bind without region is already
   **MATCH**
   (`ActivateQuest_Oakvale_binds_S_QNOVI_without_region_or_raid`).
4. **Map-wait:** yield `vtbl+28` until context
   `vtbl+48("StartOakVale")` / `CurrentRegionIndex==4`.
   Do **not** `RequestLoadRegion(4)` inside `ActivateQuest`.
   Do **not** change index 1 first Present to 4.
5. **Region load** happens on the **later** `00502500` /
   recovered `00500540(4)` site (WorldUpdate wanted-map or
   cross-region Teleport). Host `ApplyLoadJob(4)` then
   ContainsMap `StartOakValeWest` TNG + `SetRegionAsLoaded(4)`.
   Caller of that Oakvale job: **UNREAD** — wait, do not invent.
6. **Kid lookup** after map-wait, AttackOver false,
   `00CB7940` miss. Mesh **4300**. Adult 4299 remains the
   first-Present bind.
7. **Camera:** TNG construct `NOVI_LiveFather` → `00DB86B0`
   → `UseCamera CAM_OVIF_SHOT2`. Existing
   `ScriptCameraActive` / `UseCamera sticks` keeps FOV **72**
   from stomping `0049E080`. First-seen Lookout frames stay 70
   until that bind.
8. Intro Teleport / `HerosOldHouse` / 12 s / `+80` spin stay
   on this fiber. Writer of `+80=1` is `00DBB2A7` (later raid),
   not this switch.

`FirstSceneWorld.Build` is **not** step 4–7. It is fixture
soup. Wiring it onto `SubmitCurrentWorld` would **collapse**
leftover #4.

---

## Timeline (no-save vs later intro)

```
0042F2A2 Leave
00416953 FinalAlbion.wld
  NewRegion 1 LookoutPoint              // first Present region
  NewRegion 4 StartOakVale              // later / persist index
  00CD6E27 bind S_QNOVI / 00DBEF70      // BIND only
  +172 QST TRUE  (no Q_NewOakValeIntro)
user.ini ActivateQuest("Gameflow")
004189C2 Pump
  00CE7670 00893610 wait Q_NewOakValeIntro = 0
00501450 00500540(1,0,0) Lookout        // FIRST PRESENT
  GuildArrivalHSP CREATURE_HERO 4299
  006B3FF0 FOV 70
  FirstSceneMapName = LookoutPoint

????  proven 004B4A10("Q_NewOakValeIntro")   // ACTIVATOR UNREAD
????  00502500 / 00500540(4) StartOakVale    // REGION SWITCH UNREAD caller
00DABAC0 → 00DBDE40
  vtbl+48 ready                            // WAIT (now true)
  CREATURE_HERO_CHILD 4300                 // HERO MESH SWITCH
NOVI_LiveFather → 00DB86B0
  UseCamera CAM_OVIF_SHOT2 FOV 72          // CAMERA SWITCH
```

No-save New Game never enters the `????` lines. **PROVEN**.
Keep it that way until those sites are dump `E8`s.

---

## Leftover #4 vs #50

| Leftover | What it is | Do not fold |
|---|---|---|
| **#4** | Lookout first *rendered* scene vs Oakvale *intro view* ledgers | this file |
| **#50** | Host first-proximity TNG pump OOM workaround (`004FDBC0` / `004FBF60 LookoutPoint.tng`) | TNG pump, not region switch |

---

## Do not

- Replace Lookout first Present with `StartOakValeWest` /
  `FirstSceneWorld` / `RegionTravel.StartingRegion`.
- `RequestLoadRegion(4)` / `LoadRegionByName("StartOakVale")`
  from `Pump` / `RequestNewGame` / `ActivateQuest`.
- Invent persist `PlayerRegionName=StartOakVale` (#4 item 1
  writer is **UNREAD**; empty on no-save).
- Invent `ActivateQuest("Q_NewOakValeIntro")`.
- Call `00DBDE40` from first `004189C2`.
- Seed `CAM_OVIF_SHOT2` / FOV 72 as first Present helper.
- Spawn `CREATURE_HERO_CHILD` / 4300 at `GuildArrivalHSP`.
- Treat intro `Teleport` as the region switch
  (`FirstSeenTeleportChangesRegion=false`).
- Collapse leftover **#4** or fold **#50** into it.
- Follow `ManageQuestCoreMarkers` / PostAttack `00DBE3C0`
  off first-seen StartOakVale.
