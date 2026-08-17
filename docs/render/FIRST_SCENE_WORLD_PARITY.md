# First-scene world parity checklist

First New Game is `StartOakValeWest` / `CAM_OVIF_SHOT2` /
`HerosOldHouse`. Success is the proven chain

FILES → geometry → space → placement → visibility → W → V → P →
pass → material/state → Vulkan equivalent

not a better-looking screenshot.

Statuses: **PROVEN**, **EQUIVALENT**, **UNREAD**, **DISPROVEN**,
**TEMPORARY**.

## REGION

| Semantic | Evidence | Current host behaviour | Status | Test |
|---|---|---|---|---|
| New Game map is `StartOakValeWest` (3456, 736), not Lookout | WLD; `RegionTravel.StartingRegion`; `WorldSceneTests` | `FirstSceneWorld.Region` | PROVEN | `WorldPipelineTests.House_father_kid_path_fence_terrain_share_region_local_world` |
| TNG XY is region-local | Lookout 0–128; Oakvale house ~34,129 | TNG used as local | PROVEN | same |
| STB XY is WLD | ushorts 3456+ / 3232+ | `LevTileVertex.WorldX/Y` | PROVEN | same |
| `STB − (MapX, MapY)` is the meeting-space rule | house vs nearest STB vert; camera is TNG local | `WorldSpaces.StbFileToRegionLocal` | PROVEN | same |
| Neighbour offset is ΔMapX/ΔMapY | `WorldGeometry.AddTerrain` dx/dy | same | PROVEN | `WorldGeometryTests.New_game_oakvale_loads_contains_and_sees_maps` |
| Adult Lookout is not first scene | `00DBDE40` StartOakVale | not used as start | PROVEN | `WorldSceneTests` |

## LANDSCAPE

| Semantic | Evidence | Current host behaviour | Status | Test |
|---|---|---|---|---|
| 15-byte file → 24-byte GPU (XYZ, unpacked n, D3DCOLOR extra) | `00BFE050` / `00BFDEC0` | `LevTileMesh.TryReadPayload` | PROVEN | `LevFormatTests` |
| Primary strip `IndexCount = PrimitiveCount+2` | D3D strip; PARITY | `LandscapeStrip.IndexCountFromPrimitiveCount` | PROVEN | `WorldPipelineTests.First_seen_landscape_submits_primary_and_edge_strips` |
| Edge tessellation strips submit when stored | `CPatchTesselationEdgeStrip`; Lookout extras fill holes | extras always emitted, including on 17×17 tiles | PROVEN | same |
| Extra-vert XY must be 2000–6000 | Lookout-only leftover; Oakvale MapY=736 | rejected first-scene edge strips | DISPROVEN | `LevTileMesh.ReadExtras`; `WorldPipelineTests` |
| 17×17 grid only when v=289 and lattice complete | PARITY DISPROVEN fill of adaptive tiles | `useGrid` then extras | PROVEN | same |
| No invented 1 m fill | `00BF4570` stored tessellation only | `ToTileTriangles` == tile strips | PROVEN | same |
| Native cam-rel+`T(cam)` == host world+I | `00BF46A2`; identity W | `WorldSpaces.NativeLandscapeClip` / `HostLandscapeClip` | EQUIVALENT | `WorldPipelineTests.Native_cam_relative_Tcam_clip_equals_host_identity_W` |
| `T(cam)` on world STB verts | put exterior at `p+cam` | not applied | DISPROVEN | same |
| `n.Z < 0` rewind | no exe write | removed; `LandscapeStrip.FirstSeenRewindsNegativeNz=false` | DISPROVEN | `WorldPipelineTests.Landscape_strip_unwind_does_not_rewind_on_negative_nz` |
| Odd-t strip unwind `(b,a,c)` | FIRST_SCENE_CONTRACT; D3D strip | `LandscapeStrip.Unwind` | PROVEN | same |
| Sea/water tiles omitted from FG | `00B783F0` empty; no type-8 bank | texture 442 not submitted | PROVEN | `LevFormatTests` / visibility test |

## STATIC

| Semantic | Evidence | Current host behaviour | Status | Test |
|---|---|---|---|---|
| C3D cm × 0.01 | `MeshToWorld` | `WorldSpaces.C3dLocalToMetres` | PROVEN | `WorldPipelineTests.Static_tng_object_transform_places_local_origin_at_thing` |
| `RHSetForward` / `RHSetUp`, right = forward × up | `WorldGeometry.ObjectTransform` | same | PROVEN | same |
| `CreateWorld` lays Z-up meshes on their side | streetlamp test | not used | DISPROVEN | `WorldGeometryTests.Streetlamp_stands_on_world_z_not_createworld_y` |
| Object origin lands on TNG pos | house / door / table | `ObjectTransform` translation | PROVEN | static TNG test |
| Static world matrix identity | `009881F0` / `00B2FC50` | `IdentityWorld` | PROVEN | camera test |

## PALSKIN

| Semantic | Evidence | Current host behaviour | Status | Test |
|---|---|---|---|---|
| File blend bytes are VS `a0` register offsets | `00A8E770` / VS `a0` | `PalskinGpuAddressOffset` 0,3,6… | PROVEN | `PalskinPipelineTests`; `Locked_palskin_and_projection_findings_are_not_reverted` |
| `group[a0/3]` then packed c38 | `00BCFB00` dest[group[i]*64] | `SkinPosition(..., group)` | PROVEN | same |
| Father stride 20 / flags 4 packed pos | `00A8FD40` | `FatherPalskinStrideBytes` | PROVEN | same |
| Kid stride 28 / flags `0x14` | file fields | mesh parse | PROVEN | `MeshFormatTests` |
| First-seen dest ≈ identity | bind-pose | identity palettes | PROVEN | `WorldPipelineTests` trace D |
| PALSKIN is one submit category, not the world fix | this contract | not used to move terrain | PROVEN | traces A–E |

## HOUSE

| Semantic | Evidence | Current host behaviour | Status | Test |
|---|---|---|---|---|
| Exterior C3D **6909** | Graphic; 2 prims; tex 345 + 3180 | both multi-static meshes | PROVEN | `WorldPipelineTests.Visibility_and_layers_drive_shipped_first_scene_lists` |
| Interior C3D **6911** | CMultiStatic first entry; wall 3172 | submitted | PROVEN | same |
| Materials 3184 / 3182 have no prims | `FirstSeenHouseFloor3184HasPrims=false` | not replaced | PROVEN | same |
| Floor under SHOT2 is landscape PATH_STONEY **4130** | `WorldGeometryTests` | landscape layer | PROVEN | shared-space test |
| InsideBuilding does not drop a mesh | `FirstSeenInsideBuildingFlag=false` | both submit | PROVEN | visibility |
| Layer `0x20`, static VS/PS | `ScenePasses` | MeshBatches | PROVEN | visibility / traces |

## SKY

| Semantic | Evidence | Current host behaviour | Status | Test |
|---|---|---|---|---|
| Else-path bit `0x2000`, not `0x400000` | `00B662F0` | `SkyPass.FirstSeenLayerBit` | PROVEN | `WorldPipelineTests.Sky_and_materials_keep_unread_constants_unread` |
| Near 100 / far 10000 / MinZ 0.99 / MaxZ 1 | `00B30B50` sky source | `SkyViewProjection` | PROVEN | same + camera test |
| `VSHADER_INNER_SKY` c5–c8 | `dp4 oPos, v0, c5–c8` | WVP product | PROVEN | trace E |
| PS c0/c1/c2 | no `def`; writer unread | stand-in; classified UNREAD | UNREAD | trace E shader line |
| 9×37 ellipsoid 6500×3250 at origin | `00B61DD0` | `SkyGeometry` | PROVEN | `WorldGeometryTests.First_seen_sky_dome_is_6500_by_3250_ellipsoid` |
| Stars / invented billboards | `00B65A20` first dword==0 | not emitted | PROVEN | visibility |
| Do not invent sky colour | black exterior is FOGCOLOR | clear black | PROVEN | `Dx9VulkanColor` |

## CAMERA

| Semantic | Evidence | Current host behaviour | Status | Test |
|---|---|---|---|---|
| SHOT2 helper pos/look/up, Z-up | TNG + `00B314E0` | `ScriptedCamera` | PROVEN | `WorldPipelineTests.Camera_world_view_clip_ndc_on_real_first_scene_points` |
| `right = up × look`; look on view Z | `HelperViewAxes` | `CotScaledView` | PROVEN | same |
| `clip.w = view.z` | VS c8 | `FirstSeenDx9Projection` | PROVEN | same |
| Cot on view, not in P | `00B30B50` | letterbox cots | PROVEN | `CameraProjectionTests` |
| First-seen P ≠ Numerics perspective | `009883F0` XY identity | not `CreatePerspectiveFieldOfView` | PROVEN | `CameraProjectionTests` |
| Vulkan Y flip at submit | `Dx9VulkanProjection` | `ToVulkanWvp` | EQUIVALENT | camera test dx9Y = −vkY |
| Not redesigned for looks | this goal | same helper | PROVEN | camera test |

## MATERIAL

| Semantic | Evidence | Current host behaviour | Status | Test |
|---|---|---|---|---|
| DXT/SCRATCH top mip → RGBA8 | `009BE8xx` | CPU decode top mip | EQUIVALENT | `TextureFormatTests` |
| Lower mips stored, not uploaded | MaxLod=1 | TEMPORARY | TEMPORARY | `Dx9VulkanSamplerState.MaxLod` |
| sRGB | unread | UNORM | UNREAD | `DX9_VULKAN_PARITY.md` |
| Sampler MIN/MAG/MIP/ADDRESS | unread | LINEAR/REPEAT | TEMPORARY | `Dx9VulkanSamplerState` |
| Static/PALSKIN sample diffuse t0 only | first-seen VS/PS | bump unbound | PROVEN | `WorldShading.FirstSeenBindsC3dBump=false` |
| Landscape oT0 from extra.yz; oT1=(0,0) | c40=c41=0 | `ProjectOt1` | PROVEN | `LevFormatTests` |
| Alpha test / fill / color write / stencil | unread | TEMPORARY/UNREAD | UNREAD / TEMPORARY | `Dx9Vulkan*` |

## LAYERS

| Semantic | Evidence | Current host behaviour | Status | Test |
|---|---|---|---|---|
| 34-layer registration `00B26A75` | `ScenePasses.Registration` | same rank sort | PROVEN | `ScenePassTests`; visibility test |
| First-seen draws `0x4`, `0x40`, `0x20`, `0x2000` | FIRST_SCENE_CONTRACT FRAME | `MeshBatches` | PROVEN | same |
| Not flattened to one opaque pass | unread bits stay unread | `FirstSeenLayerContract` | PROVEN | `ScenePasses.FirstSeenLayers` |
| Water `0x20000` empty-out | `00B783F0` | no draw | PROVEN | visibility |
| Shadows / `0x400000` / HUD / particles | unread | not submitted | UNREAD | non-goal |

## VISIBILITY

| Semantic | Evidence | Current host behaviour | Status | Test |
|---|---|---|---|---|
| Maps = Contains/Sees ∪ BWD AABB touch | `OpenStaticMaps` / `CLandscapeBackgroundPatch` | `WorldGeometry.StaticMapsAround` | PROVEN | `WorldGeometryTests` |
| Sea maps skipped | `WorldMap.IsSea` | `TryAdd` | PROVEN | visibility records |
| Landscape patch AABB 4-plane | `00BDC2D0` | `landscapePlanes` on Build | PROVEN | visibility + `WorldSceneTests` |
| Object Graphic / CMultiStatic | `FirstSeenInstancesAsC3d` | gizmos rejected | PROVEN | visibility (marker reject) |
| LOD is ready-or-not | `00A23DE0`; no mesh swap | `MeshLodInfoReady_00A23DE0(0)==1` | PROVEN | visibility |
| Indoor/outdoor both house meshes | InsideBuilding false | 6909+6911 | PROVEN | house rows |
| Stars rejected | `00B65A20` | not emitted | PROVEN | visibility |
| Do not submit everything | recovered predicates, not a dump-all | `FirstSceneWorld.Visibility` | PROVEN | visibility test |

## Traces

Persisted under `docs/render/traces/world-trace-{A..E}.txt`.

| Id | Category | Built by |
|---|---|---|
| A | landscape | `FirstSceneWorld.TraceLandscape` |
| B | static house | `FirstSceneWorld.TraceHouse` |
| C | static prop | `FirstSceneWorld.TraceProp` |
| D | PALSKIN father | `FirstSceneWorld.TracePalskin` |
| E | sky | `FirstSceneWorld.TraceSky` |
