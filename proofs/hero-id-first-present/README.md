# Kid 4300 vs adult 4299 — first Present (leftover #4 dual ledger)

Investigation only. No production `src/` edits.

Do **not** collapse leftover **#4**. First no-save Present
hero is Lookout adult **4299**. Childhood **4300** is the
intro-view ledger, not that Present.

Do **not** invent persist `PlayerRegionName`. Empty on
no-save. Writer stays **UNREAD**.

Status words: **PROVEN** / **MATCH** / **PARTIAL** /
**UNREAD** / **DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Authority: `src/Fable.Game/FirstSceneWorld.cs`,
`WorldGeometry.cs` `IsPrimaryStart` / `CloneAs`,
`EngineLifecycle.SpawnHeroFromPlayerStart` /
`ResolveHeroDefinition` / `LoadFromFirstRealRegion`,
`RegionTravel.cs` (`StartOakValeSetup`, `KidCreature`,
`AdultCreature`);
`listing-00d80000.txt` `00DBDE40`–`00DBDF46`;
`listing-00440000.txt` `00449D90`;
`listing-00680000.txt` `006AC910`;
`xrefs.tsv` `"CREATURE_HERO_CHILD"` **one** hit
`00DBDF09` `fn=00DBDE40`;
`GameBinFormatTests` `FindMeshId("CREATURE_HERO")==4299`
/ `FindMeshId("CREATURE_HERO_CHILD")==4300`;
`EngineLifecycleTests.Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`
(`HeroMeshId==4299`, `DoesNotContain` kid);
`No_save_does_not_activate_Q_NewOakValeIntro`;
`WorldGeometryTests.New_game_oakvale_loads_contains_and_sees_maps`
(`PlayerMeshId==4300` on expand soup);
`docs/status/README.md` leftover #4;
`docs/status/investigations/2026-08-18-first-scene-things.md`;
siblings `leftover-4-collapse-audit`, `palskin-child-hero`,
`0049F180-first-children`, `hero-4299-create`,
`00DBDE40-host-gap`, `00DBDE40-after-activate`,
`oakvale-without-leftover4`, `player-region-name-writer`.

Question: no-save first Present hero id? Is kid **4300**
that Present? Who would spawn **4300** when Oakvale intro
**actually** runs?

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| No-save first Present hero? | `CREATURE_HERO` Graphic **4299** `MESH_HERO` at `GuildArrivalHSP`. WLD index **1** LookoutPoint. Camera `006B3FF0` FOV **70**. | **PROVEN** |
| Childhood **4300** that Present? | **No.** Intro-view ledger: `StartOakValeWest` / `HerosOldHouse` / `CAM_OVIF_SHOT2` / `Q_NewOakValeIntro` / kid **4300**. | **DISPROVEN** as first Present |
| Collapse leftover #4 into one scene? | **No.** Keep Lookout Present vs Oakvale intro *ledgers*. | **LEFTOVER** open |
| Invent `PlayerRegionName=StartOakVale` (or Lookout) to pick a hero? | **No.** Empty no-save. `00487C20` is continue load. | **DISPROVEN** |
| `0049F180` / `00449D90` first bind? | `"CREATURE_HERO"`, not `"CREATURE_HERO_CHILD"`. Load-World `00489D40` **misses**. | **PROVEN** |
| First Thing create? | Later Lookout `006AC910` size `0x208`. Mesh **4299**. | **PROVEN** |
| Does `00DBDE40` spawn 4300? | **No.** After map-wait: intern `"CREATURE_HERO_CHILD"`, context `vtbl+280` / `vtbl+376` **lookup**. `004AA840` is CString dtor. Next heap is 60-byte `WatchBarrels`. | **DISPROVEN** as create |
| Who *would* spawn 4300 if Oakvale intro ran? | Native factory **UNREAD**. Host leftover inject is `WorldGeometry.IsPrimaryStart` `CloneAs(NOVStartHSP, CREATURE_HERO_CHILD)` from `FirstSceneWorld.Build` only. Pump `SpawnHeroFromPlayerStart` still resolves **4299**. | lookup **PROVEN**; create **UNREAD**; host inject **LEFTOVER** |

---

## Verdict

**No-save first Present is Lookout adult 4299.
Childhood 4300 is leftover #4’s other ledger.**

Two meshes, two scenes, one open pairing. Do not
retarget `LoadFromFirstRealRegion` / `006AC910` /
`FirstSceneMapName` to Oakvale to “reach” the kid.
Do not activate `Q_NewOakValeIntro` from Leave /
Pump / `user.ini`. Do not invent `PlayerRegionName`.

When Oakvale intro **actually** runs (later, after a
**proven** `00CB5AD0("Q_NewOakValeIntro")` and after
StartOakVale is **current**), native `00DBDE40` **looks
up** `CREATURE_HERO_CHILD` (mesh **4300**). It does not
`006AC910`. The only `.text` xref of that string is
this lookup. Native Thing **create** of Graphic 4300
stays **UNREAD**. Host `FirstSceneWorld` /
`IsPrimaryStart` clone is leftover soup, not Pump.

Leave leftover **#4** open.

---

## Dual ledger (do not collapse)

| Ledger | Map / spawn | Camera | Hero | First no-save Present? |
|---|---|---|---|---|
| **Lookout Present** | WLD index **1** LookoutPoint; `00501450` → `00500540(1,0,0)`; `HOLY_SITE_PLAYER_START` `GuildArrivalHSP` | `006B3FF0` / `00A0C130` FOV **70** | `CREATURE_HERO` mesh **4299** (`006AC910`) | **yes** |
| **Oakvale intro view** | persist index **4** `StartOakVale` / map `StartOakValeWest`; `HerosOldHouse`; `NOVStartHSP` | `CAM_OVIF_SHOT2` FOV **72** | `CREATURE_HERO_CHILD` mesh **4300** | **no** |

WLD: `World.Regions[0].Index == 1` `LookoutPoint`;
`World.Regions[3].Index == 4` `StartOakVale`. Dummy
slot 0 is `005066E0`, not Lookout.

`FirstSceneWorld` type comment already splits:

```
Reconstructed intro-view fixture:
StartOakValeWest / CAM_OVIF_SHOT2 /
ScriptRuntime.StartNewGame / WorldGeometry.Build.
Not EngineLifecycle.Pump (no-save Present
is LookoutPoint). Do not collapse leftover #4.
```

`docs/status/README.md` leftover #4 stays that
pairing. `FIRST_SCENE_*` remains the intro-view
contract. Do not fold leftover **#50** (first-proximity
TNG pump) into this leftover.

---

## 1. No-save first Present — 4299 Lookout

```
0042F2A2  Leave frontend
0042F491  Init Game
  00416953  Loading world FinalAlbion.wld
    00416BCA  0049F180(ecx=world, 0)     // only no-save site
      "Init Characters"
        00449D90
          009AD410 "PLAYER_HERO"
          0044BA90 miss                  // no Graphic
          00449E0D "CREATURE_HERO"       // not CHILD
          0048A070 → 00489D40
            holy-site miss + [0x13B8647]==0
            ret 0                        // NO 006AC910
      "Init GUI" / "Init Quests"         // Q_SunnyvaleMaster; not Oakvale
004189C2  dummy pumps                    // HeroSpawned=false
later 00501450
  00500540(1,0,0) LookoutPoint
  006C2170 ContainsMap TNG
    LookoutPoint.tng: no PlayerCreature, no CREATURE_HERO,
                      no CREATURE_HERO_CHILD
  HOLY_SITE_PLAYER_START GuildArrivalHSP
    later 00489D40 (caller UNREAD vs Load World miss)
      00489FC1  006AC910                 // FIRST Hero Thing
        Graphic 4299 MESH_HERO
  WorldCamera.SeedHero 006B3FF0 FOV 70
```

`EngineLifecycleTests.Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`
locks:

- `FirstSceneMapName == "LookoutPoint"`
- `Hero.DefinitionType == CREATURE_HERO`
- `HeroMeshId == 4299`
- `DoesNotContain` `CREATURE_HERO_CHILD` in `RegionThings`
- `DoesNotContain` `Va==00DBDE40`
- `StartingHolySiteIsNovStartOnNoSave == false`
- `DoesNotContain("StartOakVale", FirstSceneMapName)`

`0049F180` first children (`proofs/0049F180-first-children`):
immediate miss string is `"CREATURE_HERO"`. Kid **4300** is
a different def. **DISPROVEN** on Leave.

`GameBinFormatTests`: `CREATURE_HERO` → **4299**
`MESH_HERO` (77 bones, prim0 stride 36 / flags 22).
`CREATURE_HERO_CHILD` / `CREATURE_YOUNG_HERO` → **4300**
`MESH_YOUNGHERO_02` (76 bones, stride 28 / flags `0x14`).
Do not mix.

Pump `ResolveHeroDefinition` Notes `00449E0D CREATURE_HERO
fallback` and returns `AdultCreature`. It never names
`KidCreature`.

---

## 2. Childhood 4300 is intro view, not first Present

`FirstSceneWorld.Build` (tests only; **zero**
`Fable.Client` / `EngineLifecycle` callers):

```
Region   = RegionTravel.NewGameRegion     // StartOakValeWest
Camera   = UseCamera("CAM_OVIF_SHOT2")    // FOV 72
runtime  = ScriptRuntime.StartNewGame
geometry = WorldGeometry.Build(..., expand default true)
kid      = ActorPositions[IntroHeroActor]
           else FindPlayerStart (NOVStartHSP)
```

`WorldGeometry.Build` when `region==StartOakValeWest`
and TNG has no Adult/Tween/Kid Thing:

```
IsPrimaryStart && FindPlayerStart
  playerMeshId = FindMeshId(CREATURE_HERO_CHILD)   // 4300
  CloneAs(NOVStartHSP, KidCreature, teleported Hero)
```

`WorldGeometryTests` locks `PlayerMeshId==4300` on that
expand soup. `PresentWorld` live path uses
`expandGeometry: false` on **LookoutPoint** and already
has adult `CREATURE_HERO`. The clone **does not run**
on Pump.

Live submit PALSKIN ids are `[4299]`
(`2026-08-18-first-scene-things.md`). Kid PALSKIN file
facts stay dump-backed (`palskin-child-hero`); they are
not this Present.

`Q_NewOakValeIntro` is registrar **bind** `00CD6E27`
(`00CB5C90` / `S_QNOVI` / factory `00DBEF70`, persist
`bl=0`). Init Quests `004B4260([world+172])` never
names it. Type-1 Gameflow **waits**
`00893610("Q_NewOakValeIntro")` miss. **PROVEN**
(`No_save_does_not_activate_*`).

---

## 3. Who would spawn 4300 when Oakvale intro actually runs

Native path **after** a proven activate (activator on
no-save still **UNREAD** — do not invent it; do not
invent `PlayerRegionName` to force region 4):

```
00CD6E27  bind only
????      00CB5AD0("Q_NewOakValeIntro")     // ACTIVATOR UNREAD
          00DBEF70 / 00DAAC00 size 0x10C
00DABAC0  register NOVI_* then
          E8 00DBDE40                       // only caller 00DAC295
00DBDE40  vtbl+48("StartOakVale")            // WAIT; not 00500540
          yield vtbl+28 until current
          00CB7940 abort → ret
          READ [this+80] AttackOver          // 0 first-seen
00DBDF08  push "CREATURE_HERO_CHILD"         // ONLY .text xref
00DBDF24  call [eax+280]                    // context find
00DBDF33  call [edi+376]
00DBDF3D  call 004AA840                     // CString dtor, NOT spawn
          then 00BFEA1A(60) WatchBarrels …
```

`xrefs.tsv`:

```
0x012D9D08  0x00DBDF09  fn=0x00DBDE40  CREATURE_HERO_CHILD
```

One hit. No second intern. No `006AC910` of this name.
No TNG NewThing of this def on Lookout
(`CREATURE_HERO_CHILD in TNG: False`). StartOakValeWest
TNG has `NOVStartHSP` + `CREATURE_HERO_FATHER` /
`NOVI_LiveFather`; `IsPrimaryStart` injects because
`existingHero` is null — the file does **not** already
place a kid Thing.

`004AA840` (`jmp 0099A2E0`) releases the stack CString
after the lookup. **DISPROVEN** as creature ctor
(`PARITY.md`, `oakvale-without-leftover4`).

`00DBDE40` does **not** load StartOakVale
(`StartOakValeSetupLoadsRegion=false`). Context
`vtbl+48` is a ready query. Do **not** satisfy it with
`PlayerRegionName=StartOakVale`. That persist key is
empty on no-save (`player-region-name-writer`:
**UNKNOWN** writer; `00487C20` is continue **load**).

### Native create of Graphic 4300 — UNREAD

| Candidate | Why not the 4300 factory |
|---|---|
| `006AC910` / `00489D40` | `CREATURE_HERO` **4299** at `GuildArrivalHSP`. Listing immediate is not CHILD. |
| `00449D90` | Name bind only; first take `ret 0`. |
| `00DBDE40` `vtbl+280/+376` | Lookup after map-wait. No `00BFEA1A(0x208)`. |
| `004AA840` | CString dtor. |
| `0051FD80` NewThing | StartOakValeWest has no `CREATURE_HERO_CHILD` row. |
| `SpawnHeroFromPlayerStart` on Oakvale TNG | Host still `ResolveHeroDefinition` → **4299**. `NOVStartHSP` is unused on no-save (`StartingHolySiteIsNovStartOnNoSave=false`). |
| Persist `PlayerRegionName` | Empty no-save. Do not invent. |

So: when intro **runs**, native **uses** def
`CREATURE_HERO_CHILD` / mesh **4300** at the lookup.
Who **constructed** that Thing (or swapped Graphic on
the already-spawned Lookout hero) is **UNREAD**.
`vtbl+376` body is **UNREAD**. Do not fill it with
`CloneAs` / `006AC910(CHILD)` / a `PlayerRegionName`
write.

### Host leftover that *would* inject 4300

Only if someone called `FirstSceneWorld.Build` /
`WorldGeometry.Build("StartOakValeWest", expand=true)`:

```
IsPrimaryStart(StartOakValeWest)
  && no TNG CREATURE_HERO / _TRAINING / _CHILD
  CloneAs(NOVStartHSP, CREATURE_HERO_CHILD)
  PlayerMeshId = 4300
```

That is leftover intro soup (`audit-firstsceneworld`,
`palskin-type1-0x80-4300`). Wiring it onto `Pump` /
`SubmitCurrentWorld` would **collapse leftover #4**.
Do not.

Pump after a later StartOakVale `00502500` would still
not spawn 4300: `HeroSpawned` is already true from
Lookout **4299**, or a fresh `SpawnHero` still returns
`CREATURE_HERO`.

---

## 4. Not these

| Candidate | Why not first-Present 4300 / why not a 4300 create |
|---|---|
| `FirstSceneWorld.Build` on Pump | Zero production callers. Fixture ≠ Present. |
| `ScriptRuntime.StartNewGame` | Tests / fixture. Invents `S_QNOVI` fiber. Client never calls it. |
| `ActivateQuest("Q_NewOakValeIntro")` | No literal. No-save Gameflow **waits**. Activator **UNREAD**. |
| `LoadFromFirstRealRegion` index 4 | Loop starts at **1** Lookout. Collapse of leftover #4. |
| `004A2C80` `0049F180(1)` | Save `004A21F0`, not no-save. |
| `0066FF89` coop kid | Oakvale leftover vs this Present (`0048A0AF-first-miss`). |
| `WorldGeometry.PlayerMeshId=4300` | Expand soup test, not `HeroMeshId`. |
| `ScriptedCamera` ctor FOV 72 | SHOT2 constant on unused object. Lookout seed writes **70**. |

---

## MATCH vs leftover vs UNREAD

**MATCH / PROVEN**

- First Present: Lookout index 1, `GuildArrivalHSP`, `CREATURE_HERO` **4299**, FOV 70.
- Leave bind `"CREATURE_HERO"`; first `00489D40` miss; later `006AC910` is 4299.
- Kid def / mesh ids: **4300** `MESH_YOUNGHERO_02`. Different Graphic.
- `"CREATURE_HERO_CHILD"` one xref `00DBDF09` inside `00DBDE40`.
- `00DBDE40` only `E8` is `00DAC295` in `00DABAC0`. Not Leave / `00501450`.
- `004AA840` dtor. Map-wait is `vtbl+48`, not a region load.
- No-save does not activate Oakvale. `PlayerRegionName` empty.
- `FirstSceneWorld` / `IsPrimaryStart` clone is tests-only.

**LEFTOVER** (keep #4 open)

- Status / `FIRST_SCENE_*` still title Oakvale house / SHOT2 / kid as “first-seen New Game”.
- Host `IsPrimaryStart` 4300 inject vs live 4299 Present.
- `RegionTravel.NewGameRegion = StartOakValeWest` as a name.

**UNREAD**

- Native Thing factory / appearance swap that makes Graphic **4300** exist for the `vtbl+280` lookup.
- Context `vtbl+376` body.
- Who `00CB5AD0`s `Q_NewOakValeIntro` on no-save (not this Present).
- Persist `PlayerRegionName` **writer** (do not invent a string).
- Later `00502500` caller that feeds StartOakVale after Lookout Present.

---

## Do not

- Collapse leftover #4 (Lookout Present vs Oakvale intro view).
- Spawn `CREATURE_HERO_CHILD` / 4300 at `GuildArrivalHSP`.
- Replace `CREATURE_HERO` in `SpawnHeroFromPlayerStart` / `ResolveHeroDefinition`.
- Wire `FirstSceneWorld` / `StartNewGame` / `IsPrimaryStart` onto Pump.
- Invent `ActivateQuest("Q_NewOakValeIntro")`.
- Invent `PlayerRegionName=StartOakVale` (or Lookout) as a New Game write.
- Treat `004AA840` / `00DBDE40` as `006AC910`.
- Fold leftover #50 into this leftover.
