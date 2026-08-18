# H — Regression audit: temporary bridges that became production

Audit of `17908c3` … `HEAD` (`d6821b8`). Investigation only.
Do not revert the IEngineHost / Pump-owns-AVI-and-New-Game split.
The suspect is **one giant `TexturedMesh` baked at first submit**.

Range: `17908c3^..HEAD` (runtime spine `17908c3`, `9c625bc`, `d9977fb`,
`f63b741`, `5cb3435`, `b062c5d`, `991bab2`, `204a214`, then frontend /
quest / status commits). Live New Game Present is LookoutPoint + hero
4299, not Oakvale / SHOT2.

## Verdict

`PresentWorld` as header + `009AD410` handles is native-shaped and
should stay. `Pump` owning AVI, New Game, seed, and *when* to Present
is native-shaped and should stay.

What became the live draw path at `f63b741` and is still HEAD:

1. One-shot `SubmitCurrentWorld` after `HeroSpawned`.
2. CPU-flatten visible LEV cells + every primary C3D (PALSKIN bind dest
   included) into **one** `TexturedMesh`.
3. `EngineFrame.Vertices` / `Draws` / `Textures` carry that blob.
4. `SilkEngineHost.Present` `SetMesh` + `SetTextures` the whole blob
   **every** game Present.

That is a temporary bridge, not `009DA9F0` DIP-per-layer / per-object
draw. `docs/status/README.md` currently lists `f63b741` “no world
triangle soup” as PROVEN. That is only true for
`SubmittedWorld.Triangles`. The soup moved to `SubmittedMesh`.

---

## Classification table

| Part | Commit(s) | Class | Why |
|---|---|---|---|
| `IEngineHost` / `EngineFrame` as Present surface (`009BEEB0`) | `d9977fb` | **PROVEN NATIVE SEMANTIC** | Host does not decide New Game, region, or expand. Engine chose camera / AVI / world. Keep. |
| `Pump` owns AVI skip/unload, New Game from input, `EnterGame` | `d9977fb` | **PROVEN NATIVE SEMANTIC** | Matches `00412F90` / `006286F0` / frontend Return. Client `Program.cs` only queues keys. Keep. |
| `PresentWorld` `expandGeometry: false` (instances + LEV/STB headers) | `17908c3` | **PROVEN NATIVE SEMANTIC** | `00B3EFA0` PeekMapHeader; `009AD410` handles; `CurrentCompiledLev` / `CurrentHeightField` stay null at open. |
| `MeshBank.Open` directory; `Get` parse-on-id cache | `17908c3` + bank commits | **PROVEN NATIVE SEMANTIC** | `0049E620` / `009AD410`. “Unique C3Ds” means parse once, not one instance per id. |
| `OpenTextureBank` at Init Graphics; decode submitted ids (`009BE8B0`) | `991bab2` | **PROVEN NATIVE SEMANTIC** | `00416C8A` `GBANK_MAIN_PC`. Not `window.Load`. |
| Game Present waits for seed + submit; skip LeaveFrontend empty origin | `991bab2` / `b062c5d` | **PROVEN NATIVE SEMANTIC** | `00417001` does not Present until WorldFrame>1 / maps open. `006B3FF0` before first 3D Present. |
| Submit *timing* after `006C2170` / OpenStaticMaps, before `00435530` | `b062c5d` | **PROVEN NATIVE SEMANTIC** | Native draw consumes already-opened maps. Keep the call site. |
| `006B3FF0` / `006B2CA0` seed (no invented 1.6 m `SeedAt`) | `204a214` | **PROVEN NATIVE SEMANTIC** | First-seen ctor angles. Keep. Do not restore `SeedAt`. |
| `SubmitSidePlanes` four-plane extract (`00B2FD60` / `00BDC2D0`) | `b062c5d` | **PROVEN NATIVE SEMANTIC** | Same extract `PresentWorld` already builds when seeded. |
| Per-opened-map AABB reject-fully-outside, then stored STB cells | `b062c5d` `AddTerrain` / `TessellateVisible` | **VALID BACKEND TRANSLATION** | Matches `00BDC2D0` n-vertex AABB (`Z=0`) then `00BF4570` cells. Not per-tile cull; native also submits the patch’s stored cells after the patch test. |
| `MeshBatches.Build` group-by (layer, tex, tex1, blend) + `ScenePasses` rank | pre-range + still used | **VALID BACKEND TRANSLATION** | Vulkan stand-in for `009DA9F0` layer walk + texture bind. Rank order is native. |
| `MeshBank.Get` once + transform **every** primary instance | `f63b741` | **VALID BACKEND TRANSLATION** | `seen` is a unique-id set for the log / hero fallback, **not** a skip. All primary instances go into `props`. |
| Hero 4299 PALSKIN stream kept (`SkinVertices` / `SkinFaces`) | `5cb3435` | **PROVEN NATIVE SEMANTIC** (data) | Hero is not a static flatten of `mesh.Triangles`. `00BD2F91` dest exists. |
| `TrianglesForPose()` CPU-skinned into world triangles at submit | `5cb3435` | **TEMPORARY BRIDGE** | Native is `VSHADER_PALSKIN_*` per DIP with `c38` dest. Host bakes bind-pose verts once. First-seen dest is bind locals (`FirstSeenPlaysAnim=false`) — correct *pose*, wrong *site*. |
| `MeshBatches.BuildMeshes` → triangle list → `Build` | `f63b741` | **TEMPORARY BRIDGE** | Comment says “no `WorldGeometry` triangle soup”. It is the same soup, one function down. |
| `MeshBatches.Concat(land, props)` one `TexturedMesh` | `f63b741` | **TEMPORARY BRIDGE** | Native never concatenates landscape + every C3D into one VB. Concat does **not** re-sort `ScenePasses.Rank` (accidentally OK if land is only `0x4`/`0x40` and props only `0x20`). |
| `EngineFrame.Vertices` / `Draws` / `Textures` as the draw payload | `f63b741` / `991bab2` | **TEMPORARY BRIDGE** | `Fable.Game` now references `Fable.Render` and owns backend `MeshVertex[]`. Native Present is device DIP of live layers, not a baked array. |
| `WorldSubmitted` one-shot (`if (HeroSpawned && !WorldSubmitted)`) | `d9977fb` → `b062c5d` | **TEMPORARY BRIDGE** | Never cleared (not on `CloseStaticMapFile`, not on camera move, not on travel). Native redraws opened maps each frame. Frustum used at first submit is frozen. |
| Production submit drops `SkyGeometry` | `f63b741` (vs `Expand`) | **TEMPORARY BRIDGE** / regression | Sky is only in `WorldGeometry.Build(expand=true)` and `Expand`. `TessellateVisible` + `BuildMeshes` emit no `0x2000` dome. Live Lookout Present has no sky. |
| Instances stay primary-only at submit | `f63b741` / `b062c5d` | **TEMPORARY BRIDGE** | Neighbour terrain can enter via `TessellateVisible`. Neighbour C3Ds stay handles. Native draws visible objects on opened patches. |
| `TessellatePrimary` | `f63b741` | **DISPROVEN** as current path | Defined, unused. Production uses `TessellateVisible`. Keep as a helper; do not restore it as the live path. |
| `Expand` / `ExpandPresentedWorld(primaryOnly: true)` | `17908c3` / `9c625bc` | **DISPROVEN** as current production | Dead after `f63b741` (`ExpandPresentedWorld` has no callers). Still the *old* client draw. Tests still call it. |
| `WorldGeometry.Build` default `expandGeometry: true` (neighbours + sky soup) | pre-range, still default | **DISPROVEN** as live New Game | Production always `false`. Default `true` is the first-scene / test soup (Oakvale SHOT2, Lookout full cluster). |
| `SilkEngineHost` leftover `frame.World.Expanded` → `MeshBatches.Build(triangles)` | `d9977fb` | **DISPROVEN** as live path | Unreachable while submit sends unexpanded `SubmittedWorld` + `Vertices`. Fallback still present. |
| `SilkEngineHost.Present` Vertices branch: `SetMesh` + `SetTextures` every frame | `f63b741` / `991bab2` | **LIKELY PERFORMANCE BUG** | Expanded branch skips on `ReferenceEquals(_uploadedWorld, world)`. Vertices branch never skips. `SetTextures` does `DeviceWaitIdle` + destroy + re-upload every GBANK id. `SetMesh` maps and copies the whole VB. `BuildFrame` also allocates `[.. _submittedTextures]` every Present. |
| One-shot `TessellateVisible` under a moving camera | `b062c5d` + one-shot flag | **LIKELY PERFORMANCE BUG** + semantic freeze | AABB is correct *if* re-run. Frozen mesh means culled neighbours never appear when the camera turns, and accepted maps stay even when they leave the frustum. |
| Host `LoadGpuTextures` dummy `TexturedMesh` when `frame.Textures` empty | `d9977fb` | **TEMPORARY BRIDGE** | Production now sends engine textures. Fallback still decodes on the host. |

---

## Commit walk (what changed, what stuck)

### `17908c3` — PresentWorld opens instances and LEV/STB headers only

**Keep.** Split open vs draw. `WorldMeshInstance` is the `009AD410`
handle + transform. `expandGeometry: false` leaves `Triangles` empty.

`Expand()` was the temporary draw: tessellate every opened map + every
instance + `SkyGeometry` into `WorldGeometry.Triangles`. That was the
old “world soup”.

### `9c625bc` — Client: engine primary-map draw, unload AVI, seed camera

Client still called `ExpandPresentedWorld(opened)` with `primaryOnly:
true`, then `MeshBatches.Build(world.Triangles)` + `SetMesh`. AVI
unload / engine camera were the good parts. The expand call was still
the soup. `d9977fb` deleted this client path.

### `d9977fb` — Client is IEngineHost; Pump owns AVI, New Game, world submit

**Keep the split.** `SilkEngineHost.Present` is `009BEEB0`. First
`SubmitCurrentWorld` still did `ExpandPresentedWorld` (soup). First
`EngineFrame` had no `Vertices` — host built the mesh from
`World.Triangles` when `Expanded`.

`EngineFrameTests.Unexpanded_world_is_not_a_geometry_submit` correctly
locks the open/draw split. It does **not** lock Concat.

### `f63b741` — Engine submit: unique C3Ds + primary terrain, no world soup

**This is where the bridge became production.**

```
land  = MeshBatches.Build(opened.TessellatePrimary(_levels))
props = MeshBatches.BuildMeshes(primary instances)
SubmittedMesh = MeshBatches.Concat(land, props)
EngineFrame.Vertices / Draws = that mesh
SilkEngineHost: if (frame.Vertices.Length > 0) SetMesh(verts, draws)
SubmittedWorld stays unexpanded
```

Intent: stop `WorldGeometry` soup; parse C3Ds via `Meshes.Get`; primary
terrain only. Result: one backend vertex buffer of every land tri plus
every prop tri, submitted once.

`Fable.Game` gained a `Fable.Render` reference so the engine can own
`TexturedMesh`. That coupling is the bridge.

`Install_banks_and_startup_videos_exist` was rewritten from
`SubmittedWorld.Expanded && Triangles.Count > 128` to
`!Expanded && empty Triangles && SubmittedMesh.Vertices.Length > 128`.
That test now **locks the giant mesh** as success.

### `5cb3435` — Engine submit skins PALSKIN C3Ds; hero 4299 is not static

**Keep the PALSKIN *data* and the hero-as-skinned-thing fact.**
`TrianglesForPose` / `PaletteForPose` (`00A9E1E0` / `00BD2F91`) is the
right dest math. Baking it into `BuildMeshes` at submit is the bridge.
Hero fallback (`006AC910` is a Thing, not a TNG Graphic) is defensive
and native-shaped.

### `b062c5d` — Submit before Present; terrain uses 00BDC2D0 AABB then cells

**Keep submit-before-`00435530` and the AABB test.** Switched
`TessellatePrimary` → `TessellateVisible` (all opened maps, neighbour
offset, reject fully outside, then cells). Instances stay primary-only.
Moved the `HeroSpawned && !WorldSubmitted` call into `PumpGameUpdate`.

One-shot + camera-dependent tessellate is the leftover hazard.

### `991bab2` — Engine owns GBANK textures; game Present waits for seed+submit

**Keep bank ownership and the Present gate.**
`BindSubmittedTextures` walks `SubmittedMesh.Draws` ids, `LoadMany`,
puts `GpuTexture[]` on `EngineFrame`. Host prefers `frame.Textures`.

Does **not** append `GpuTexture.White()` (host fallback still does).
Does re-upload every Present (see perf).

### `204a214` — Replace invented SeedAt with first-seen 006B2CA0 pose

**Keep.** Unrelated to the mesh soup. Status README still has a stale
“Host `SeedAt(1.6m)` DIVERGE” row from `18ef09b`; the code is
`SeedHero` / `ComputePose` now.

### After `204a214` through `d6821b8`

Frontend flush (`0042DF9E`, `0041AFA0` / `0041BEB0` type `0x22`),
quest pump, `006B3030` spring, `006B3B80` skip. None of these change
`SubmitCurrentWorld` / `Concat` / `SetMesh`. The giant mesh is still
the live 3D path.

---

## Current production path (HEAD)

```
Pump(Game)
  PumpGame → ActivateCurrentRegion (first frame)
    OpenStaticMaps (headers) + TNG + SpawnHero
      WorldCamera.SeedHero (006B3FF0 / 006B2CA0)
  PumpGameUpdate
    if (HeroSpawned && !WorldSubmitted) SubmitCurrentWorld
      PresentWorld()                         // unexpanded handles
      TessellateVisible(planes)              // land tris
      BuildMeshes(primary C3Ds + hero)       // prop tris, PALSKIN CPU
      Concat → SubmittedMesh                 // ONE TexturedMesh
      BindSubmittedTextures
      WorldSubmitted = verts > 0             // never cleared
    RenderGameMode / 00435530 Notes
  if (WorldSubmitted && WorldCamera.Seeded) PresentToHost
    EngineFrame { World=unexpanded, Vertices, Draws, Textures }

SilkEngineHost.Present
  SetTextures(engineTex)                     // WaitIdle + rebuild ALL
  SetMesh(verts, draws)                      // copy entire VB
  return                                     // skip Expanded fallback

SilkEngineHost.Draw
  Renderer.Draw(camera VP) → DrawMeshBatches
```

`ExpandPresentedWorld` is unused. `TessellatePrimary` is unused.
Sky is not on this path.

---

## Tests that lock the temporary bridge

Do **not** “fix” these to restore `WorldGeometry` soup, and do **not**
treat a failing verts-count assert as proof that native wants one VB.

| Test | What it locks | Treat as |
|---|---|---|
| `EngineLifecycleTests.Install_banks_and_startup_videos_exist` | `WorldSubmitted`; `SubmittedWorld` unexpanded + empty `Triangles`; **`SubmittedMesh.Vertices.Length > 128`**; hero 4299 in `SubmittedPalskinMeshIds`; `SubmittedHeroPalskin`; `SubmittedTerrainMaps` contains LookoutPoint; `SubmittedTextures.Count > 0` and `BuildFrame().Textures` same length | **Bridge lock** on Concat mesh + texture blob. Keep the *unexpanded world* / hero PALSKIN / GBANK / Lookout asserts. Do not require one giant VB if submit is later split. |
| `EngineFrameTests.Unexpanded_world_is_not_a_geometry_submit` | `EngineFrame.World.Expanded == false`, empty triangles | **Native lock** (open ≠ draw). Safe. |
| `EngineFrameTests.EngineFrame_constructs_for_host_present` | Frame fields + `Textures` null default | Safe. |
| `WorldGeometryTests.Open_records_instances_without_c3d_or_tiles` | Open does not parse C3Ds; **`Expand(..., primaryOnly: true)`** then `Triangles.Count > 128` | **Old draw path.** Encodes Expand soup, not Concat. Do not retarget live submit at this. |
| `WorldGeometryTests.TessellateVisible_uses_00bdc2d0_aabb` | Unculled count; Lookout accepted; PicnicArea vs AABB | **Native lock** on the frustum helper. Safe. Does not lock Concat. |
| `WorldGeometryTests.Lookout_point_instances_world_meshes` | Default `Build` soup (`expandGeometry: true`), `Triangles.Count > 128`, instances > 150 | **Old soup / first-scene helper.** Not live New Game. |
| `WorldGeometryTests.Lookout_scene_opens_aabb_adjacent_static_maps` | Neighbour maps + sky triangle + sand tint on **expanded** `Build` | **Old soup.** Sky assert is *not* true of `SubmitCurrentWorld`. |
| `ScenePassTests.Lookout_draws_follow_exe_layer_bits` | `MeshBatches.Build(world.Triangles)` has `0x4`/`0x40`/`0x20`/`0x2000` in rank order | **Old soup including sky.** Production Concat currently has no `0x2000`. |
| `GpuTextureTests.Lookout_batches_by_texture_and_keeps_uvs` | `Vertices.Length == Triangles.Count * 3` on expanded Build | Encodes `Build(soup)`, not engine submit. |
| `WorldPipelineTests.Visibility_and_layers_drive_shipped_first_scene_lists` | Oakvale / SHOT2 `scene.Geometry.Triangles` + `0x2000` | **FIRST_SCENE contract**, not Lookout live Present. Do not collapse into Concat. |
| PALSKIN / `XSeqFormatTests` / `Kid_c3d` / `Wake_loop_3420` | File stride, `TrianglesForPose`, hero bones | **Native data.** Safe. Do not require CPU flatten at submit. |

`docs/status/README.md` rows for `f63b741` / `5cb3435` / `b062c5d`
call the Concat path PROVEN. That documentation is the other lock —
status text, not a test. Correct the status row when the mesh is split;
do not use it as evidence that native submitted one VB.

`docs/render/FIRST_SCENE_AUDIT.md` still traces
`WorldGeometry → MeshBatches → DrawMeshBatches` (the Expand soup).
That is the Oakvale first-scene audit, not the live Lookout pump.

---

## What not to revert

- `IEngineHost` / `SilkEngineHost` / `Pump(dt)` owning AVI and New Game.
- `PresentWorld(expandGeometry: false)` and header-only `OpenStaticMapBody`.
- `MeshBank` directory + on-demand parse.
- Init Graphics `GBANK_MAIN_PC` + per-id decode.
- Submit *before* `00435530`, Present only after seed + submit.
- `006B2CA0` pose (not `SeedAt(1.6m)`).
- Per-patch `00BDC2D0` AABB then cells.
- PALSKIN dest math and hero 4299 as a skinned Thing.

## What to treat as the suspect (do not expand the soup)

- `SubmittedMesh = Concat(Build(TessellateVisible), BuildMeshes(props))`.
- `WorldSubmitted` never reset.
- `EngineFrame` carrying one `MeshVertex[]`.
- `Present` re-uploading that array and every texture every frame.
- Missing sky on the live path.
- Primary-only instances while neighbour terrain can appear.

A later split should keep **two (or N) draw lists** (landscape patches,
static C3Ds, PALSKIN objects, sky) flushed in `ScenePasses` order —
not restore `WorldGeometry.Triangles` as the Present payload.
)
