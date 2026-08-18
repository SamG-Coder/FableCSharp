# `WorldGeometry.cs` / `FirstSceneWorld.cs` vs landscape-trace

Investigation only. Production `src/` was not edited.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Question: is host first-scene invented? Does it draw landscape during
frontend? Compare `WorldGeometry` / `FirstSceneWorld` to dump
`tools/Fable.ExeIndex/out/01-sections/landscape-trace/` (**INDEX** v5).

Prior: `proofs/landscape-first-draw/`, `terrain-first-draw/`,
`wld-first-region/`, `c3d-first-submit/`, `newgame-script/`,
`world-spaces/`, `stb-first-open/`.

---

## Verdict

**`FirstSceneWorld` is an invented Oakvale-intro soup, not the first
no-save scene. It does not run on frontend. Native frontend reaches
landscape `vtbl+16` with an empty patch list and issues no DIP.**

| Claim | Class |
|---|---|
| First nonempty landscape submit is after Leave + map attach, not `0042DF9E` | **PROVEN** (dump + listing) |
| `FirstSceneWorld.Region` / `CAM_OVIF_SHOT2` / house 6909 is first New Game | **DISPROVEN** as no-save first scene (**LEFTOVER** intro contract) |
| `FirstSceneWorld.Build` is called from `Fable.Client` or `EngineLifecycle` | **DISPROVEN** (zero production callers) |
| Host frontend `Pump` submits `WorldGeometry` / `SubmitCurrentWorld` | **DISPROVEN** |
| Native `0042DF9E` `E8`s `00B6B0B0` / `00BF4570` | **DISPROVEN** |
| Native `0042DF9E` never reaches landscape `vtbl+16` | **DISPROVEN** (`0042E0BB` → `00B27D90` → `00B6B0B0`) |
| That walk DIPs a cell | **DISPROVEN** (`[0x1436E8C]+44` empty) |
| `WorldGeometry.Build(expand=true)` default is the live Present | **DISPROVEN** |
| `PresentWorld(expand=false)` is open-shaped (`00B3EFA0` / `009AD410`) | **PROVEN** as open, not draw |

---

## What `FirstSceneWorld` invents

`src/Fable.Game/FirstSceneWorld.cs` `Build`:

```
Region     = RegionTravel.NewGameRegion          // "StartOakValeWest"
CameraName = RegionTravel.IntroFirstSeenCamera   // "CAM_OVIF_SHOT2"
HouseScript = "HerosOldHouse"
Aspect     = 4/3
runtime    = ScriptRuntime.StartNewGame(...)
geometry   = WorldGeometry.Build(..., expand default true)
```

That is the later `Q_NewOakValeIntro` / `00DBDE40` contract
(`docs/render/FIRST_SCENE_CONTRACT.md`), **not** Leave.

Native no-save New Game (`proofs/wld-first-region`,
`proofs/newgame-script`):

```
0042F2A2 Leave
  FinalAlbion.wld
00416953 Load world → first NewRegion = LookoutPoint
00501450(1) Lookout
  006C2170 ContainsMap TNG
  0051FD80 / 006AC910 hero 4299
```

Oakvale West / SHOT2 / `HerosOldHouse` / kid 4300 / father
`NOVI_LiveFather` are **not** on that path.

| Host constant / helper | Native first scene | Class |
|---|---|---|
| `StartOakValeWest` (3456, 736) | `LookoutPoint` (3232, 3488) | **LEFTOVER** |
| `UseCamera(CAM_OVIF_SHOT2)` FOV 72 | first Present does not `UseCamera` | **LEFTOVER** |
| `ScriptRuntime.StartNewGame` | unused (`S_QNOVI` bind only) | **DIVERGE** |
| `WorldGeometry.IsPrimaryStart` injects `CREATURE_HERO_CHILD` | hero 4299 adult at Lookout | **DISPROVEN** as first Present |
| `FindFence` (FENCE/GATE/WALL/STREETLAMP or `OBJECT_BUILDING_DOOR_3`) | no such first-scene picker | **invented** |
| `TraceLandscape` file line `FinalAlbion_RT.stb .lev` | first-seen STB is `FinalAlbion.stb` **MISS**; `_RT` needs `[0x13B8616]!=0` | **DIVERGE** + wrong region |
| `TraceHouse` C3D 6909/6911 | first static DIP is Lookout, not the house | **LEFTOVER** |
| `Classify` one whole-map AABB labeled `00BDC2D0` | native is per-patch 16 m grid then per-cell | **DIVERGE** |

`docs/render/FIRST_SCENE_WORLD_PARITY.md` still writes “New Game map
is `StartOakValeWest` … not Lookout” as **PROVEN**. That is the
Oakvale contract table, **DISPROVEN** as Leave / first Present.

Production callers of `FirstSceneWorld`: **none** under `src/`. Tests
(`WorldPipelineTests`, `ScriptRuntimeParityTests`) and dump tools
only.

---

## Frontend draw

### Native (`landscape-trace` + listing `0042DF9E`)

```
0042EC7C retail
  0042DF9E
    009D8CF0 / 009BEF20
    00595582 type 0x22 UI only
    0042E0BB  [retail+88].vtbl+32 = 00B27D90
      00B25950 → 00B2AB80 → 00B6B0B0   // dump: landscape-draw-vtbl16
      ebp = [0x1436E8C]+44
      mov esi,[eax]; cmp esi,eax; je   // empty → no 00BDC060 / 00BDC2D0
    009D9C80 / 009DA9F0(1) ×2
    009BEF50 / 009BEEB0
0042F2A2 Leave
  0042EBB6 teardown Present (not 0042DF9E)
  00B428E0 first-seen FinalAlbion.stb MISS   // setstaticmapfileforuse
```

`00B6B0B0` (`landscape-draw-vtbl16-00b6b0b0.md`):

| `arg+4` | walk |
|---|---|
| `4` | list → `00BDC060` → `00BF71D0` BG |
| `0x40` (`64`) | list → `00BDC2D0` → `00BF4570` FG cell |
| other | `00B67510` unbind, no DIP |

Empty sentinel: **no patch call**. **PROVEN.**

Direct `E8` from `0042DF9E`: none of `00B6B0B0` / `00BDC060` /
`00BDC2D0` / `00BF4570` / `00BF71D0`. Frontend does **not** open
STB (`proofs/stb-first-open`).

### Host

`EngineLifecycle.Pump` frontend:

```
PumpFrontendFrame()
maybe RequestNewGame + EnterGame
PresentToHost()   // EngineFrame.FrontendBatch only
```

`SubmitCurrentWorld` is only in `PumpGameUpdate`:

```
if (HeroSpawned && !WorldSubmitted)
    SubmitCurrentWorld();
```

`HeroSpawned` is after Leave + `006C2170` / `006AC910`. Frontend
never builds `FirstSceneWorld` and never calls `WorldGeometry.Build`.

`RequestNewGame` nulls `FrontendBatch`; it does not construct Oakvale
geometry.

`SilkEngineHost.Present` can keep a leftover `SubmittedMesh` **and**
a `FrontendBatch`. `VulkanLineRenderer.Draw` then draws mesh batches
**then** `DrawFrontend`. That would be a host overlay of 3D under
UI — **not** native, and **not** first-run (mesh is still null).
**DIVERGE** if a previous world is still uploaded; first frontend
has no land DIP.

---

## `WorldGeometry` vs dump (open vs draw)

Dump open path (`INDEX.md`):

```
00B428E0 SetStaticMapFileForUse
  00B42750 OpenStaticMaps
    00B42530 OpenOneMap
      00B3EFA0 ParseMapHeader          // on STB hit
      00BDD0E0 / 00BDF010 attach → list +44
00B41FA0 LoadWaterData
```

Dump draw path (after `+44` nonempty, every `00B27D90`):

```
00B6B0B0 vtbl+16
  bit 0x4  → 00BDC060 → 00BF71D0     BG frustum + procedural
  bit 0x40 → 00BDC2D0 → 00BF4570     4-plane AABB then 72-byte cells
00BF4570: test [ebp+60], 0x04; je skip; 00BF3860 cell AABB; DIP
```

| Host | Native (dump) | Class |
|---|---|---|
| `Build(expand=true)` default: neighbours + `SkyGeometry` soup | open is headers; sky is later `0x2000` | **DISPROVEN** as first draw |
| `PresentWorld` `expand=false` | `00B3EFA0` / `009AD410` handles | **PROVEN** as open-shaped |
| `AddTerrain`: map AABB (neighbours only) then **all** tiles | `00BDC2D0` patch AABB then per **72-byte** cell | **DIVERGE** |
| `CollectVisibleCells`: skip `Faces.Count==0`; no `+60 & 4` | `00BF4570` requires `[cell+60] & 0x04` | **DIVERGE** |
| `TessellatePrimary` | unused; not the walk | **DISPROVEN** as path |
| `TessellateVisible` = `AddTerrain` per opened name | stored-cell DIP, not full-tile soup | **DIVERGE** |
| `StaticMapsAround` Contains/Sees + BWD touch | `00B42750` mode + attach | **PARTIAL** (cluster yes; BWD extra) |
| `TryAdd` skips `IsSea` | sea → `00B6D4D0` water renderer, not FG land | **PROVEN** intent |
| `SubmitCurrentWorld` `Concat(land, C3D, sky)` one mesh | layers `0x4` → `0x40` → `0x20` → `0x2000` | **DISPROVEN** as native |
| one-shot `WorldSubmitted` after `HeroSpawned` | every `00B27D90` | **TEMPORARY BRIDGE** |
| `ScenePasses.DrawnPasses(Landscape)` = bits `0x4` **and** `0x40` on the **same** STB tris | bit `0x4` is `00BF71D0` BG; stored cells are `0x40` only | **DISPROVEN** pairing |
| `FirstSceneWorld.TraceLandscape` layer `0x4` for PATH_STONEY | that texture is FG stored strip, bit `0x40` | **DISPROVEN** |
| `MeshBatches.BuildCells` FG `0x40` only | matches `00BF4570` family | **PARTIAL** (still CPU unwind / one VB) |
| `LandscapeCell` comment “VB mesh+56 / IB mesh+52” | those offsets are origin / AABB; buffers on `00BFE050` | **DISPROVEN** (see `landscape-first-draw`) |
| `ObjectTransform` cm × 0.01, RHSetForward/Up | not in landscape-trace; C3D path | **PROVEN** elsewhere |

`00BDC2D0` (`patch-submit-bit40-frustum-00bdc2d0.md`): if `[this+8]`
cells exist and `[this+4]` tessellator exists, four planes at
`[0x1436EA0]+0x1C8` vs tessellator `+168/+180` (Z=0). Missing
tessellator submits every cell. Then:

```
cell = [this+8] + (row * cols + col) * 72
call 00BF4570
```

Host `AddTerrain` uses `PeekMapHeader` grid size then
`ToTileTriangles` for the **whole** height field. That is not the
72-byte walk and does not test `+60` bit 4.

---

## Recovered no-save order (host vs native)

```
0042EC7C / host Frontend
  0042DF9E  2D UI
  host: PumpFrontendFrame + FrontendBatch Present
  FirstSceneWorld: not called
  WorldGeometry.Build: not called
  00B6B0B0: reached, +44 empty, no DIP

0042F2A2 Leave
  host: RequestNewGame (null batch; FinalAlbion.wld)
  no land soup

00416953 / host EnterGame
  00B428E0 FinalAlbion.stb MISS
  host PresentWorld not yet

later 00501450 Lookout + 006C2170 + hero
  host: HeroSpawned → SubmitCurrentWorld
    PresentWorld(expand=false)          // open-shaped
    CollectVisibleCells + BuildCells    // not 00BF4570
    Concat land+objects+sky             // not native layers
  next 00B27D90
    bit 0x4  → 00BDC060 → 00BF71D0
    bit 0x40 → 00BDC2D0 → 00BF4570      // first stored-cell DIP
```

Game caller of `012A0F3C+32` after Leave remains **UNREAD**.

---

## Classification

| Claim | Status |
|---|---|
| `FirstSceneWorld` is reconstructed Oakvale intro, not first Leave scene | **PROVEN** leftover |
| That helper draws during frontend | **DISPROVEN** (no caller; frontend has no land submit) |
| Native frontend issues first landscape DIP | **DISPROVEN** |
| Live host open is `PresentWorld(expand=false)` after hero spawn | **PROVEN** as host timing |
| Live host draw equals `00B6B0B0` / `00BF4570` | **DISPROVEN** (Concat + missing `+60` + dual 0x4/0x40 soup) |
| `WorldGeometry.Build` default `expand=true` | **DISPROVEN** as live New Game; still the test / first-scene soup |
| Game `00B27D90` site after Leave | **UNREAD** |
| Native Lookout STB hit after `FinalAlbion.stb` miss | **UNREAD** |

Dumps: `landscape-trace/INDEX.md`, `landscape-draw-vtbl16-00b6b0b0.md`,
`patch-submit-bit4-00bdc060.md`, `patch-submit-bit40-frustum-00bdc2d0.md`,
`per-cell-submit-00bf4570.md`, `setstaticmapfileforuse-00b428e0.md`,
`openstaticmaps-00b42750.md`. Host: `src/Fable.Game/WorldGeometry.cs`,
`src/Fable.Game/FirstSceneWorld.cs`, `EngineLifecycle.SubmitCurrentWorld`
/ `PresentWorld` / `Pump`.
