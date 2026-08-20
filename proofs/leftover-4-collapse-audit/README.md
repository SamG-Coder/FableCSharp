# Leftover #4 collapse audit — Oakvale on first no-save Present?

Investigation only. Production `src/` was not edited.

Do **not** collapse no-save first Present into the childhood
intro. First Present is LookoutPoint (WLD index **1**, adult
mesh **4299**, `GuildArrivalHSP`, camera `006B3FF0`, FOV **70**).
Childhood intro remains `StartOakValeWest` / `HerosOldHouse` /
`CAM_OVIF_SHOT2` / kid **4300** / `Q_NewOakValeIntro` /
`CS_OAKVALE_INTRO_FATHER` FOV **72**. Do **not** “fix” by
activating Oakvale.

Status words: **PROVEN** / **MATCH** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Authority: `src/Fable.Game/FirstSceneWorld.cs`,
`ScriptRuntime.StartNewGame`, `EngineLifecycle.Pump` /
`LoadFromFirstRealRegion` / `ActivateNamedQuest` /
`SubmitCurrentWorld` / `PresentWorld` /
`SpawnHeroFromPlayerStart`, `src/Fable.Client/Program.cs` /
`SilkEngineHost.cs`, `RegionTravel.cs`,
`tests/Fable.Formats.Tests/EngineLifecycleTests.cs`
(`No_save_does_not_activate_Q_NewOakValeIntro`,
`Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`),
`docs/status/README.md` leftover #4,
`proofs/audit-firstsceneworld`, `proofs/audit-startnewgame`,
`proofs/issue-4-verify`.

Question: any host DIVERGE that would **show Oakvale** on first
Present?

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| FirstSceneWorld on `EngineLifecycle.Pump`? | **No.** Type comment and body are Oakvale intro soup. Zero callers in `Fable.Client` / `EngineLifecycle`. | **MATCH** split (fixture ≠ Pump) |
| `LoadFromFirstRealRegion` from Pump? | **No.** `Pump` / `PumpGame` never call it. Callers: tests, `_loadprobe`, and `EnqueueAfterDummy` (tests only). Body is `00500540(1,0,0)` Lookout, not Oakvale. | **MATCH** body; **DIVERGE** *when* (not this leftover) |
| `StartNewGame` on live Present? | **No.** Client never calls it. | **PROVEN** absence |
| `ActivateQuest("Q_NewOakValeIntro")` in `src/`? | **No literal.** Generic `ActivateNamedQuest` never receives that name on no-save (`+172` persistent / user.ini `Gameflow`). Gameflow **yields** on the name. | **MATCH** no-save |
| First Present geometry Oakvale? | **No.** Live submit is Lookout `FirstSceneMapName` + `006B3FF0` FOV 70 after hero seed, or empty (dummy index 0, no `HeroSpawned`). | **CLEAN** |

**Verdict: CLEAN.** No host collapse site would Present
`StartOakValeWest` / SHOT2 / kid 4300 as the first no-save
frame. Leftover #4 stays **open as ledger pairing**, not as a
live Present bug.

Do not invent `ActivateQuest(Q_NewOakValeIntro)` to close #4.

---

## Contract (do not collapse)

| Ledger | Native pairing | First no-save Present? |
|---|---|---|
| LookoutPoint WLD index 1 | `00501450` → `00500540(1,0,0)`; `HOLY_SITE_PLAYER_START` `GuildArrivalHSP`; `006AC910` `CREATURE_HERO` mesh **4299**; `WorldCamera.SeedHero` `006B3FF0` FOV **70** | **yes** |
| Oakvale intro view | `StartOakValeWest` / `HerosOldHouse` / `CAM_OVIF_SHOT2` / kid **4300** / `Q_NewOakValeIntro` / `S_QNOVI` / `00DABAC0` → `00DBDE40` / `CS_OAKVALE_INTRO_FATHER` FOV **72** | **no** |

WLD: `World.Regions[0].Index == 1` `LookoutPoint`;
`World.Regions[3].Index == 4` `StartOakVale`. Host native
table index 0 is dummy `005066E0`, not Lookout.

---

## 1. `FirstSceneWorld.cs` — reconstructed intro fixture, not Pump

`C:\FableCSharp\src\Fable.Game\FirstSceneWorld.cs` header:

```
Reconstructed intro-view fixture:
StartOakValeWest / CAM_OVIF_SHOT2 /
ScriptRuntime.StartNewGame / WorldGeometry.Build.
Not EngineLifecycle.Pump (no-save Present
is LookoutPoint). Do not collapse leftover #4.
```

`Build`:

| Choice | Value |
|---|---|
| `Region` | `RegionTravel.NewGameRegion` = `StartOakValeWest` |
| Camera | `UseCamera("CAM_OVIF_SHOT2")` FOV 72, aspect 4:3 |
| Runtime | `ScriptRuntime.StartNewGame(install, things, camera)` |
| Geometry | `WorldGeometry.Build(..., expand default true)` |
| Picks | `HerosOldHouse`, `NOVI_LiveFather`, kid from `ActorPositions` |

Callers of `FirstSceneWorld.Build` (whole tree):

| File | Role |
|---|---|
| `tests/.../WorldPipelineTests.cs` | fixture tests |
| `tests/.../ScriptRuntimeParityTests.cs` | `First_scene_world_still_shares_space_from_runtime_state` |

Zero hits in `src/Fable.Client` and `src/Fable.Game/EngineLifecycle.cs`.

**Stale comment (ledger only, not Present):**
`WorldPipelineTests` still says the soup is “the same
StartOakValeWest / SHOT2 / WorldGeometry path as the client.”
Client `Program.cs` is `life.Pump(dt)` + `SilkEngineHost.Present`.
That comment is **DISPROVEN** as live pairing. It does not
wire Oakvale into Present.

---

## 2. `LoadFromFirstRealRegion` callers — tests (and probe), not Pump

Definition: `EngineLifecycle.LoadFromFirstRealRegion`
(`0x00501450`). Loop `00500540(i,0,0)` for `i=1..count-1`.
First real index is Lookout. Does not invent StartOakVale.

`EnqueueAfterDummy` (src) is the only production *callee* of
`LoadFromFirstRealRegion`. `EnqueueAfterDummy` itself is only
called from `EngineLifecycleTests` (one site). `Pump` /
`PumpGame` never call either.

| Caller | Kind |
|---|---|
| `tests/Fable.Formats.Tests/EngineLifecycleTests.cs` | tests (explicit after dummy pumps) |
| `tests/Fable.Formats.Tests/Dx9DeviceRecordTests.cs` | tests |
| `tools/_loadprobe/Breakdown.cs` | probe tool |
| `EngineLifecycle.EnqueueAfterDummy` | src helper; tests-only caller |

After explicit `LoadFromFirstRealRegion` the Lookout test
locks:

- `FirstSceneMapName == "LookoutPoint"`
- `GuildArrivalHSP` in `ThingsForMap("LookoutPoint")`
- hero mesh **4299**, not `CREATURE_HERO_CHILD`
- `Camera.FovDegrees == GameCamera.FirstSeenFovDegrees` (**70**)
- `DoesNotContain("StartOakVale", FirstSceneMapName)`
- `DoesNotContain` `RegionTravel.StartOakValeSetup` (`00DBDE40`)

`PresentWorld` primary is `FirstSceneMapName` (Lookout from
`GuildArrivalHSP`), not `StartOakValeWest`. `SubmitCurrentWorld`
requires `HeroSpawned`; Pump dummy ticks never spawn.

`00501450` *when* vs Pump is leftover glue
(`proofs/host-00501450-timing`). That leftover is empty vs
Lookout, **not** Oakvale.

---

## 3. `ScriptRuntime.StartNewGame` — leftover DIVERGE, not first Present

```
StartNewGame(install, things, camera)
  Load(script.bin)
  BindScene
  InstallRecoveredBindings   // ScriptFiberTable.Recovered[0] = S_QNOVI + AttackOver
  ActivateThings(list)       // TNG ScriptName → factory cutscene
```

No Oakvale string literals in the method body. Oakvale facts
live in `ScriptFactoryTable.Recovered[0]`
(`NOVI_LiveFather` → `CS_OAKVALE_INTRO_FATHER`) and
`ScriptFiberTable.Recovered[0]` (`S_QNOVI`). Feeding Oakvale
TNG therefore **starts the father cutscene**.

That is **DIVERGE** vs native first `004B4260` (`Q_SunnyvaleMaster`
via `00CB5AD0`; bind-only `00CD6E27` for `Q_NewOakValeIntro`).
Treating `StartNewGame` as New Game / first Present is leftover
pairing (`proofs/audit-startnewgame`).

Callers of `StartNewGame`:

| File | Role |
|---|---|
| `FirstSceneWorld.Build` | intro fixture |
| `WorldSceneTests.cs` | tests |
| `WorldGeometryTests.cs` | tests |
| `ScriptRuntimeParityTests.cs` | tests |
| `ScriptRuntimeArchitectureTests.cs` | tests |

Live client: `EngineLifecycle.InitCharactersAndQuests` uses
`ScriptRuntime.Detached()` then `ActivateNamedQuest` on
`world+172` persistent names. Never `StartNewGame`.
`PumpScripts` is Note-only (`006E75C0`); no `Runtime.Update`
of `S_QNOVI`.

**Would this show Oakvale on first Present?** Only if someone
wired `StartNewGame` / `FirstSceneWorld` onto `Pump` /
`SubmitCurrentWorld`. That wire is **absent**. Do not add it.

---

## 4. `ActivateQuest(Q_NewOakValeIntro)` in `src` — none

`ActivateQuest(` call sites:

| Site | What it activates |
|---|---|
| `ScriptRuntime.ActivateQuest(string name, …)` | definition |
| `EngineLifecycle.ActivateNamedQuest` → `Runtime.ActivateQuest(name)` | `world+172` names, then user.ini arg |

No `ActivateQuest("Q_NewOakValeIntro")` anywhere in the tree.

`StoreAddQuestNames`: persistent QST rows → `world+172`. TLC
merged QST has `Q_NewOakValeIntro` **not** persistent
(`TlcInstallTests`). Test
`No_save_does_not_activate_Q_NewOakValeIntro` after
`RequestNewGame` + dummy Pumps:

- `DoesNotContain` name in `WorldPlus172` / `ActivatedQuests` /
  `Runtime.Quests`
- `GameflowYieldQuest == "Q_NewOakValeIntro"` (wait, not activate)
- `00CD6E27` Note is **bind not `00CB5AD0`**
- user.ini `ActivateQuest("Gameflow")` only

`QuestFactoryTable` has no `Q_NewOakValeIntro` row.

---

## 5. Live first Present walk (host)

```
Fable.Client Program
  EngineLifecycle.BootstrapUntilGraphics
  CompleteRetailLoop
  window.Update → life.Pump(dt)
    StartupVideos → AVI Present
    Frontend → PumpFrontendFrame (2D widgets; no TNG)
      RetailNewGameFlag → RequestNewGame / EnterGame
    LeaveFrontend → EnterGame; no Present (empty origin skip)
    Game → PumpGame
      first 004189C2: UseNamedStart false
        CurrentRegionIndex ctor 0 → dummy 004FC180
        ActivateCurrentRegion skip (index 0)
        no LoadFromFirstRealRegion
        PumpGameUpdate: SubmitCurrentWorld only if HeroSpawned
```

`SilkEngineHost.Present` draws `EngineFrame` the engine already
built. It does not expand, enter a region, or start New Game.

`EngineLifecycle.Camera` is a `ScriptedCamera` whose **ctor
default** FOV is `RegionTravel.IntroCameraFovDegrees` (**72**).
That constant is SHOT2 leftover on an unused object. After
Lookout `SpawnHero` → `ApplyWorldCamera` the helper path
`SetFovDegrees(GameCamera.FirstSeenFovDegrees)` (**70**). First
Lookout submit uses 70. Default 72 is **not** Oakvale geometry.

`BindLifecycleFirstRegion` is **absent** from `src/` (stale
status / test comment). Live client is Pump only.

---

## Collapse sites vs CLEAN

| Candidate | Would first Present show Oakvale? | Class |
|---|---|---|
| `FirstSceneWorld.Build` | **No** (tests/fixture only) | leftover intro soup; **MATCH** vs Pump |
| `ScriptRuntime.StartNewGame` | **No** unless wired to Pump | **DIVERGE** as New Game pairing; **CLEAN** as Present |
| `LoadFromFirstRealRegion` | **No** (Lookout index 1) | **MATCH** body |
| `ActivateNamedQuest` / user.ini | **No** (`Gameflow` + `+172` persistents) | **MATCH** |
| `Pump` / `SubmitCurrentWorld` / `PresentWorld` | dummy empty or Lookout `FirstSceneMapName` | **CLEAN** |
| `WorldPipelineTests` “same path as the client” | comment only | **DISPROVEN** pairing |
| `ScriptedCamera` default FOV 72 | FOV leftover before seed; no Oakvale mesh | not collapse |
| Stale `BindLifecycleFirstRegion` docs | symbol gone | **STALE** |

No production call feeds `StartOakValeWest` / `CAM_OVIF_SHOT2` /
kid 4300 / `CS_OAKVALE_INTRO_FATHER` into first Present.

**CLEAN.** Leave leftover #4 open as Lookout-vs-intro *ledgers*
(`FIRST_SCENE_*` / status north star). Do not fold #50
(first-proximity TNG) into this leftover. Do not activate
`Q_NewOakValeIntro`.
