# Audit: `FirstSceneWorld.cs` — invented Oakvale first scene?

Investigation only. No production `src/` edits.

Do **not** start at `StartOakValeWest` / `CAM_OVIF_SHOT2` /
`HerosOldHouse` / kid `4300` / `ScriptRuntime.StartNewGame`.
That is the later Oakvale intro contract (`Q_NewOakValeIntro` /
`00DABAC0` → `00DBDE40`), not frontend and not the first
no-save 3D Present.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: `src/Fable.Game/FirstSceneWorld.cs`;
`RegionTravel.cs` (`NewGameRegion`, `IntroFirstSeenCamera`);
`WorldGeometry.Build` (`expandGeometry` default, `IsPrimaryStart`);
`ScriptRuntime.StartNewGame`;
`EngineLifecycle.PumpFrontendFrame` / `SubmitCurrentWorld` /
`FirstSceneMapName`;
`Fable.Client/Program.cs` / `SilkEngineHost.cs`;
`tests/Fable.Formats.Tests/WorldPipelineTests.cs`;
`docs/render/FIRST_SCENE_CONTRACT.md`;
`docs/status/README.md` Lookout correction;
`EngineLifecycleTests.Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`;
`proofs/audit-lifecycle-newgame`, `newgame-script`, `world-spaces`,
`camera-after-leave`, `region-travel-first`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Invented Oakvale first scene during frontend? | **The Oakvale soup is a host first-scene fixture. Frontend never builds it.** Assets (`StartOakValeWest` TNG / SHOT2 / house 6909) are dump-backed. Calling that soup “first New Game” / “same as `Fable.Client`” / a frontend frame is **invented**. | **DIVERGE** (pairing) + **LEFTOVER** (class) |
| Does frontend Present Oakvale? | **No.** `PumpFrontendFrame` is 2D widgets / `009BEEB0`. No `FirstSceneWorld`, no TNG, no region. | **DISPROVEN** |
| Is `FirstSceneWorld` on the live client path? | **No callers** in `Fable.Client` or `EngineLifecycle`. Only `WorldPipelineTests` / `ScriptRuntimeParityTests`. | **LEFTOVER** |
| First no-save 3D after Leave? | **LookoutPoint** + adult **4299** + `GuildArrivalHSP`. WLD index **1**. Oakvale is persist index **4**. | **PROVEN** |
| Are Oakvale names invented strings? | **No.** WLD `StartOakVale` / maps `StartOakValeWest`+East+MemorialGarden; TNG `CAM_OVIF_SHOT2`, `HerosOldHouse`, `NOVI_LiveFather`; registrar binds `Q_NewOakValeIntro`. | **PROVEN** data, **LEFTOVER** as first scene |
| Stale comment on the type? | Yes: “built the same way as `Fable.Client`”. Client never calls `Build`. `WorldPipelineTests` repeats that lie. | **DISPROVEN** |

---

## Verdict

`FirstSceneWorld` is **not** the frontend scene and **not** the
first no-save Present. It is a reconstructed Oakvale-intro soup
(SHOT2 72° 4:3, house/father/kid traces, `WorldGeometry.Build`
default `expand=true`, `ScriptRuntime.StartNewGame` actor
positions) that older `FIRST_SCENE_*` docs still title “first
New Game”.

Live New Game from UI message **15** is Leave `0042F2A2` →
`FinalAlbion.wld` → dummy then **LookoutPoint**. No-save never
activates `Q_NewOakValeIntro`. Gameflow **waits** on that name
and **yields**.

Do not wire `FirstSceneWorld.Region` onto `PumpFrontendFrame`
or `SubmitCurrentWorld`. Do not treat SHOT2 / kid 4300 as the
first 3D frame.

---

## What `Build` actually does

```
FirstSceneWorld.Build(install)
  Region   = RegionTravel.NewGameRegion          // "StartOakValeWest"
  Camera   = UseCamera("CAM_OVIF_SHOT2")
  Aspect   = 4/3
  runtime  = ScriptRuntime.StartNewGame(...)     // ActivateThings on Oakvale TNG
  geometry = WorldGeometry.Build(..., default expand=true, adjacent maps)
  pick     HerosOldHouse, NOVI_LiveFather, kid from ActorPositions
           or FindPlayerStart (NOVStartHSP first)
  pick     nearest PATH_STONEY 4130 tri < 12 m of house
  pick     FindFence: FENCE|GATE|WALL|STREETLAMP else OBJECT_BUILDING_DOOR_3
  traces   A land / B house 6909 / C fence / D father PALSKIN / E sky
```

| Host choice | Native pairing | Class |
|---|---|---|
| Hardcoded `StartOakValeWest` as “first region” | No-save first real region is WLD index 1 `LookoutPoint` (`00501450`) | **DISPROVEN** as first |
| `UseCamera(CAM_OVIF_SHOT2)` | `FirstSeenCallsUseCamera=false`; first 3D cam is WorldCamera `006B4900` then seed `006B3FF0` | **LEFTOVER** |
| `ScriptRuntime.StartNewGame` | unused on Leave; invents Oakvale TNG + `S_QNOVI` fiber | **DIVERGE** |
| `WorldGeometry.Build` default expand + neighbours + sky soup | live `PresentWorld` uses `expandGeometry: false` | **LEFTOVER** soup |
| `IsPrimaryStart` injects `CREATURE_HERO_CHILD` / 4300 | no-save spawn is `CREATURE_HERO` / **4299** at `GuildArrivalHSP` | **DISPROVEN** as first |
| Identity palettes on father | bind-pose stand-in | **TEMPORARY** vs later clip |
| `FindFence` name soup + door fallback | reconstructed prop picker, not a native first-seen set | **DIVERGE** (picker) |
| `Classify` maps via `IsSea` + things `< 25 m` of house | not `00BDC2D0` live Lookout walk | **LEFTOVER** audit dump |
| `WorldViewProj()` SHOT2 72° | third matrix vs live helper / 70° Lookout | **LEFTOVER** |
| `FormatLandscapeSubmit` `invented1mFill=false` | 1 m fill already **DISPROVEN** and removed | **PROVEN** (negative) |

Oakvale **file** facts used by those traces (house C3D 6909/6911,
SHOT2 TNG helper, PATH_STONEY 4130, MapX/Y 3456/736) stay
**PROVEN** as Oakvale-intro data. They are not first Present.

---

## Frontend vs this type

```
0042EC7C retail / PumpFrontendFrame
  0042E3EE input
  0042DC94 / 00599E3F widgets
  0042DF9E 2D draw          // VSHADER_2D_SPRITE
  009BEEB0 Present          // no 3D W, no TNG, no FirstSceneWorld
  [retail+41] → 0042F2A2 Leave
    FinalAlbion.wld         // not StartOakValeWest
0042F491 Init Game
  00416953 LoadWorld
    00CD6E27 bind Q_NewOakValeIntro only
    004B4260 WLD initial (Q_SunnyvaleMaster, …)
    Gameflow 00CE7670 waits 00893610 miss, yields
  00501450 RequestLoadRegion(1)   // LookoutPoint
  006AC910 CREATURE_HERO / 4299
  SubmitCurrentWorld → PresentWorld expand=false
```

`EngineLifecycle.FirstSceneMapName` is the map that owns the
spawned hero (`LookoutPoint`). It is a different symbol from
`FirstSceneWorld.Region`.

| Claim in `FirstSceneWorld` / tests / `FIRST_SCENE_CONTRACT` | Live | Class |
|---|---|---|
| “First New Game Oakvale world” | first 3D is Lookout | **DISPROVEN** pairing |
| “same way as `Fable.Client`” | client: `EngineLifecycle` + `SilkEngineHost`; no `FirstSceneWorld` | **DISPROVEN** |
| `WorldPipelineTests`: “same SHOT2 path as the client” | same lie | **DISPROVEN** |
| `FIRST_SCENE_CONTRACT.md` “First-seen New Game is StartOakValeWest” | intro-view contract only | **LEFTOVER** title |
| Wire persist-Oakvale to `FirstSceneWorld` | `docs/status/README.md` still **UNREAD** | do not invent a no-save write |

---

## Call graph (C#)

| Site | Uses `FirstSceneWorld`? |
|---|---|
| `Fable.Client/Program.cs` | no |
| `SilkEngineHost` | no |
| `EngineLifecycle.PumpFrontendFrame` | no |
| `EngineLifecycle.SubmitCurrentWorld` / `PresentWorld` | no |
| `WorldPipelineTests.Load` | **yes** — Oakvale soup fixture |
| `ScriptRuntimeParityTests.First_scene_world_still_shares_space_from_runtime_state` | **yes** — house/terrain clip algebra |

Production `src/Fable.Game/FirstSceneWorld.cs` is therefore an
unused (by the client) helper kept for those tests and the
`FIRST_SCENE_WORLD_PARITY` traces.

---

## What is leftover vs invented

Two different mistakes. Do not collapse them.

1. **Invented pairing (this audit).** Host named an Oakvale
   reconstruction “first scene” and “what the client builds”.
   Frontend and no-save Present are Lookout 2D then Lookout 3D.
   `StartNewGame` as New Game **DIVERGE**.

2. **Not invented assets.** Oakvale maps, SHOT2, house meshes,
   father/kid creatures, and `S_QNOVI` exist. They belong to a
   **later** intro fiber that no-save does not start.

Keep `FirstSceneWorld` as an Oakvale-intro **audit fixture**.
Do not re-hook it onto frontend or first Present.
)