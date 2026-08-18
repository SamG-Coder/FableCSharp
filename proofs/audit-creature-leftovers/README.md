# Audit: `CREATURE_HERO` / `InitHero` / PALSKIN / XSeq on frontend or invented first-scene

Investigation only. Production `src/` was not edited.

Do **not** start at `00DBDE40` / `StartOakVale` / kid `CREATURE_HERO_CHILD`
/ father `NOVI_LiveFather` / `CS_OAKVALE_DREAM_INTRO_YOUNG_HERO_WAKING_UP_LOOP`
`3420`. Those are Oakvale-intro leftovers, not frontend and not the
first no-save 3D Present.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources (read only):

- `src/Fable.Game/EngineLifecycle.cs` (`Pump`, `PumpFrontendFrame`,
  `InitFrontendUi`, `RequestNewGame`, `EnterGame`,
  `SpawnHeroFromPlayerStart`, `ResolveHeroDefinition`,
  `SubmitCurrentWorld`, `OpenMeshBank`, `InitHeroDefFn`)
- `src/Fable.Game/FirstSceneWorld.cs`
- `src/Fable.Game/WorldGeometry.cs` (`IsPrimaryStart` kid inject)
- `src/Fable.Game/ScriptRuntime.cs` (`IsHeroThing`, `StartNewGame`)
- `src/Fable.Game/RegionTravel.cs`
- `src/Fable.Game/MeshBank.cs` (`Get` / `GetAnim`)
- `src/Fable.Formats/Anims/XSeqFile.cs`
- `src/Fable.Formats/WorldShading.cs` (`FirstSeenPalskinStrideBytes`)
- `src/Fable.Render/Parity/Dx9Vulkan/Dx9VulkanFrontend.cs`
- `src/Fable.Client/Program.cs` / `SilkEngineHost.cs`
- siblings: `proofs/palskin-open`, `xseq-first`, `audit-firstsceneworld`,
  `audit-worldgeometry`, `audit-lifecycle-newgame`

---

## Verdict

**Frontend never constructs a hero Thing, never calls `InitHero`
(`00449D90`), never parses a PALSKIN C3D, and never opens type-6
XSEQ.** Invented `FirstSceneWorld` *does* skin a leftover father
and injects kid `CREATURE_HERO_CHILD` — that helper is unused by
`Fable.Client` / `EngineLifecycle`. Live no-save Present is Lookout
adult `CREATURE_HERO` mesh **4299** after Leave + `006C2170`.

| Symbol | Frontend (`PumpFrontendFrame`) | Invented first-scene (`FirstSceneWorld.Build`) | Live after Leave |
|---|---|---|---|
| `CREATURE_HERO` | **DISPROVEN** | **DISPROVEN** (injects **CHILD**, traces **FATHER**) | **PROVEN** `00449E0D` fallback |
| `InitHeroDefFn` `00449D90` | **DISPROVEN** | **DISPROVEN** (no caller) | **PROVEN** Note on Lookout spawn |
| PALSKIN C3D / `Meshes.Get` | **DISPROVEN** | **LEFTOVER** father `TracePalskin` | **PROVEN** 4299 after `HeroSpawned` |
| `SHADERS_PALSKIN` **name** | **PROVEN** as Init-Engine token leftover (not C3D) | n/a | same register |
| `MeshBank.GetAnim` / `XSeqFile.Parse` | **DISPROVEN** | **DISPROVEN** | **DISPROVEN** first Present (`FirstSeenPlaysAnim=false`) |
| `XSeqFile.WakeLoopId` `3420` | unused | unused | **LEFTOVER** Oakvale dream fixture |

---

## 1. Frontend must not touch these

`EngineLifecycle.Pump` `Stage==Frontend`:

```
UnloadStartupAvi
InitFrontendUi          // widgets Type=10 Press Start
PumpFrontendFrame       // 0042E3EE / 0042DC94 / 0042DF9E
PresentToHost           // FrontendBatch only
```

`PumpFrontendFrame` (`EngineLifecycle.cs` ~3094) walks input, ticks
2D widgets, builds `FrontendBatch`, Presents `009BEEB0`. It does
**not** call `OpenMeshBank`, `SpawnHero`, `ResolveHeroDefinition`,
`SubmitCurrentWorld`, `FirstSceneWorld.Build`, `Meshes.Get`, or
`GetAnim`.

`InitFrontendUi` (~3027) resolves `UI_PRESS_START` via `009AD410`
on a UI def (`Type=10`). That is **not** a mesh handle and **not**
`InitHero`.

`SubmitCurrentWorld` (~2644) is gated:

```
if (Install is null || CurrentRegion is null || !HeroSpawned) return;
```

`HeroSpawned` is set only in `SpawnHero` after region TNG
(`006C2170` → `SpawnHeroFromPlayerStart`). Frontend never reaches
that. `BuildFrame` still *has* `SubmittedWorld` / mesh fields; they
stay null. `SilkEngineHost.Present` takes `FrontendBatch` when
AVI is off.

`OpenMeshBank` first runs in `EnterGame` Init World
(`0049E620`), **after** `RequestNewGame` Leave `0042F2A2`.
Directory only (`ParsedCount=0`). Not frontend.

| Claim | Class |
|---|---|
| Frontend Present is 2D (`0042DF9E` / type `0x22` / `VSHADER_2D_SPRITE`) | **PROVEN** |
| Frontend `009AD410` opens `CREATURE_HERO` / 4299 / 4300 | **DISPROVEN** (UI Type=10) |
| Frontend UI type 6 is XSEQ | **DISPROVEN** (`0054EF00` glyphs) |
| `InitFrontendUi` / `PumpFrontendFrame` call `InitHeroDefFn` | **DISPROVEN** (zero sites) |
| `MBANK_ALLMESHES` during `0042EC7C` | **DISPROVEN** |
| `GetAnim` / `XSeqFile.Parse` on frontend | **DISPROVEN** |

---

## 2. `CREATURE_HERO` — src sites

Production string uses under `src/`:

| File | Use | Class vs frontend / invented first-scene |
|---|---|---|
| `RegionTravel.AdultCreature` | `"CREATURE_HERO"` | constant |
| `EngineLifecycle.CreatureHeroDefName` | alias of Adult | live Lookout only |
| `ResolveHeroDefinition` | `009AD410 PLAYER_HERO` miss → `00449E0D CREATURE_HERO` + `0048A070` | **PROVEN** after Leave + Lookout TNG. **DISPROVEN** on frontend |
| `ScriptRuntime.IsHeroThing` | Adult / Tween / Kid | live bind prefers Adult. Kid rank is Oakvale leftover |
| `WorldGeometry.Build` existing-hero check | Adult **or** Tween **or** Kid | live Lookout has no TNG hero |
| `WorldGeometry.IsPrimaryStart` | injects **`CREATURE_HERO_CHILD`** on `StartOakValeWest` | **LEFTOVER** invented scene |
| `FirstSceneWorld.TracePalskin` | `"graphics.big CREATURE_HERO_FATHER"` | **LEFTOVER** |
| `ScriptFactoryTable.Recovered` | TNG `CREATURE_HERO_FATHER` / `NOVI_LiveFather` | **LEFTOVER** (only if `StartNewGame`) |

`Fable.Client` never mentions `CREATURE_HERO`.
`EngineLifecycle` never mentions `StartOakVale` as a load target.

`IsPrimaryStart` (`WorldGeometry.cs` ~206):

```
region == RegionTravel.NewGameRegion   // "StartOakValeWest"
  → FindMeshId(CREATURE_HERO_CHILD)
  → CloneAs(start, KidCreature, teleported Hero)
```

Live `PresentWorld` builds **Lookout** (`FirstSceneMapName` from
`GuildArrivalHSP`) with `expandGeometry: false`. `IsPrimaryStart`
is false. Kid inject **does not run** on Present.

`FirstSceneWorld.Build` **does** call `WorldGeometry.Build` on
`StartOakValeWest` with default `expand=true` → kid **4300** soup.
Zero production callers (`Fable.Client`, `EngineLifecycle`). Tests
only (`WorldPipelineTests`, `ScriptRuntimeParityTests`).

---

## 3. `InitHero` — there is no host method

Host has **no** `InitHero()`. The recovered VA is a Note tag:

```
EngineLifecycle.InitHeroDefFn = 0x00449D90
```

Call sites (both `Note(...)` only):

| Site | When | String |
|---|---|---|
| `SpawnHeroFromPlayerStart` ~6357 | after `006C2170` TNG, `HOLY_SITE_PLAYER_START` / `GuildArrivalHSP` | `"00449D90 PLAYER_HERO then CREATURE_HERO"` |
| `ResolveHeroDefinition` ~6697 | PLAYER_HERO Graphic miss | `"00449E0D CREATURE_HERO fallback"` |

`CreatePlayers` (`004166A8`) and `InitCharactersAndQuests`
(`0049F180` during `LoadWorld`) **do not** spawn a Thing and
**do not** Note `InitHeroDefFn`. `InitCharactersAndQuests` builds
`ScriptRuntime.Detached()` and WLD `InitialQuests` — not
`StartNewGame`, not kid, not `00DBDE40`.

Hero Thing is `SpawnHero` (`006AC910`):
`DefinitionType=CREATURE_HERO`, `ScriptName=Hero`, XYZ from HSP.

**Frontend:** no `SpawnHeroFromPlayerStart`. **Invented first-scene:**
no `InitHeroDefFn` Note; kid clone is `WorldGeometry`, not `00449D90`.

---

## 4. PALSKIN

### 4a. Frontend

Init Engine (retail, **before** Leave) already **names**
`SHADERS_PALSKIN` / `VSHADER_PALSKIN_DIRLIGHT_FOG` on the shader
manager (`00B3B6D0` slot 3). That is tokens, not a C3D.

`Dx9VulkanFrontend.BlendFromHandlerMode` default branch reuses
`D3dDeviceState.FirstSeenPalskinSrcBlend` / `DestBlend` (5/6 =
SRCALPHA / INVSRCALPHA). **Name leftover.** Same D3D enums the
2D sprite handler uses. Not a hero skin.

`VulkanLineRenderer` palskin blend attachment is the 3D path,
not `SetFrontendBatch`.

### 4b. Invented first-scene

`FirstSceneWorld.TracePalskin` (~209):

- resolve `FatherThing` (`CREATURE_HERO_FATHER` / `NOVI_LiveFather`)
- `LoadFatherMesh` → `defs.FindMeshId` + `MeshFile.Parse`
- `WorldShading.SkinPosition` with **identity** palettes
- layer bit **`0x20`** (static) — native first PALSKIN drain is
  `0x100` / `0x80`

That is Oakvale father, not Lookout 4299. **LEFTOVER** +
**DIVERGE** layer.

`WorldGeometry.Build(expand=true)` flattens `mesh.Triangles`
(file dest already `00A9E1E0` × IBM). It does **not** call
`TrianglesForPose` / `PaletteForPose` / `GetAnim`.

### 4c. Live Present (after Leave, not this leftover)

`SubmitCurrentWorld` after `HeroSpawned`:

- `Meshes.Get` each primary instance; `BoneCount > 0` →
  `_submittedPalskin`
- if PresentWorld missed the spawn Graphic, force-add
  `HeroMeshId` (4299) via `ObjectTransform(Hero)`
- `MeshBatches.BuildMeshes` uses **file** `mesh.Triangles`, layer
  `SceneLayer.Palskin` when `BoneCount > 0`

`WorldShading.FirstSeenPalskinStrideBytes = 28` /
`FirstSeenPalskinInitFlags = 0x14` are **kid 4300** file fields.
Adult 4299 prim0 is stride **36** / flags **22**. The constant
name “FirstSeen” is a **LEFTOVER** vs no-save Present
(`proofs/palskin-open`).

---

## 5. XSeq

Production `GetAnim` / `FindAnim` callers under `src/`: **none**
except `MeshBank` itself. `EngineLifecycle` never calls them.
`AnimationClipRecord.Sequence` is never filled from the bank
(`Clips[name] = new AnimationClipRecord(name, 1f)`).

`FirstSceneWorld` passes `runtime.Animation.PoseNames()` into
`WorldGeometry.Build` as `actorPoses` — **clip name strings
only**. No type-6 read. `TracePalskin` does not take a sequence.

`XSeqFile` leftover fixture constants:

```
WakeLoopId   = 3420
WakeLoopName = CS_OAKVALE_DREAM_INTRO_YOUNG_HERO_WAKING_UP_LOOP
```

Used by `XSeqFormatTests`, not by frontend or `FirstSceneWorld`.
`RegionTravel.IntroWakeLoop` / `IntroCutscene`
(`CS_OAKVALE_INTRO_FATHER`) feed `NewGameScript` /
`ScriptFactoryTable` — **LEFTOVER** vs Leave (`proofs/xseq-first`,
`newgame-script`).

`PaletteForPose(..., sequence)` / `TrianglesForPose(sequence)`
exist as format helpers. First Present dest is bind locals /
file triangles (`FirstSeenPlaysAnim=false`). Time interp
`00AA0090` remains **UNREAD**.

| Claim | Class |
|---|---|
| Frontend opens type-6 / `00A999B0` | **DISPROVEN** |
| `FirstSceneWorld` parses XSEQ 3420 / wake loop | **DISPROVEN** |
| `StartNewGame` → `GetAnim` | **DISPROVEN** |
| First 3DAF/XSEQ *object* after Leave is empty `00AA4710` persist helper | **PROVEN** (`xseq-first`); not a clip payload |
| First type-6 **payload** on no-save first Present | **DISPROVEN** |

---

## 6. Invented first-scene vs live pairing

```
INVENTED (FirstSceneWorld — tests only)
  StartOakValeWest
  CAM_OVIF_SHOT2 72° 4:3
  ScriptRuntime.StartNewGame  → recovered father factory + S_QNOVI fiber
  WorldGeometry.IsPrimaryStart → CREATURE_HERO_CHILD / 4300
  TracePalskin CREATURE_HERO_FATHER identity dest on 0x20
  no InitHeroDefFn, no GetAnim

LIVE no-save (EngineLifecycle)
  Frontend 2D
  Leave 0042F2A2 FinalAlbion.wld
  Init World 0049E620 mesh directory
  00501450 LookoutPoint
  006C2170 ContainsMap TNG
  00449D90 / 00449E0D / 0048A070 / 006AC910
    CREATURE_HERO ScriptName=Hero mesh 4299 at GuildArrivalHSP
  PumpGameUpdate SubmitCurrentWorld palskin 4299
  no FirstSceneWorld, no kid inject, no XSEQ sample
```

Stale comment on `FirstSceneWorld`: “built the same way as
`Fable.Client`”. Client never calls `Build`. **DISPROVEN**.

---

## Do not

- Call `FirstSceneWorld.Build`, `WorldGeometry.Build(StartOakValeWest)`,
  or `ScriptRuntime.StartNewGame` from `PumpFrontendFrame`.
- `Meshes.Get(4299|4300)`, `GetAnim(3420)`, or
  `LoadCreature(CREATURE_HERO*)` during frontend.
- Treat `IsPrimaryStart` kid inject / father `TracePalskin` as the
  first no-save Present.
- Treat `InitHeroDefFn` Notes as a frontend constructor.
- Treat `FirstSeenPalskinStrideBytes=28` as adult 4299.
- Treat `XSeqFile.WakeLoopId` as first-seen New Game.

---

## Files (absolute)

- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\src\Fable.Game\FirstSceneWorld.cs`
- `C:\FableCSharp\src\Fable.Game\WorldGeometry.cs`
- `C:\FableCSharp\src\Fable.Game\ScriptRuntime.cs`
- `C:\FableCSharp\src\Fable.Game\RegionTravel.cs`
- `C:\FableCSharp\src\Fable.Game\MeshBank.cs`
- `C:\FableCSharp\src\Fable.Formats\Anims\XSeqFile.cs`
- `C:\FableCSharp\src\Fable.Formats\WorldShading.cs`
- `C:\FableCSharp\src\Fable.Client\Program.cs`
