# First-scene world-space contract

First New Game / Oakvale: `StartOakValeWest` / `Q_NewOakValeIntro` /
`CAM_OVIF_SHOT2` / `HerosOldHouse`.

No conversion is implicit. Each space records units, handedness, axes,
origin, absolute vs relative, the conversion into the next space,
evidence, and status. Locked findings from
[FIRST_SCENE_CONTRACT.md](FIRST_SCENE_CONTRACT.md),
[FIRST_SCENE_AUDIT.md](FIRST_SCENE_AUDIT.md), and
[DX9_VULKAN_PARITY.md](DX9_VULKAN_PARITY.md) stay locked.

Statuses: **PROVEN**, **EQUIVALENT**, **UNREAD**, **DISPROVEN**,
**TEMPORARY**.

The live functions are `Fable.Formats.World.WorldSpaces` and
`Fable.Game.FirstSceneWorld`.

## Spaces

| Space | Units | Handedness | Axes | Origin | Abs/Rel | Conversion into next | Evidence | Status |
|---|---|---|---|---|---|---|---|---|
| graphics.big C3D local | centimetres | right | X right, Y forward, Z up | mesh origin | relative | × 0.01 → C3D metres | `MeshFile`; `WorldGeometry.MeshToWorld` | PROVEN |
| C3D units / centimetres | centimetres | right | same | mesh origin | relative | × 0.01 → TNG metres | FIRST_SCENE_CONTRACT GEOMETRY | PROVEN |
| TNG object local transform | metres | right | RHSetForward / RHSetUp; right = forward × up | `CTCPhysicsStandard` position | object then translation | `ObjectTransform` → region-local | `WorldGeometry.ObjectTransform`; `CreateWorld` **DISPROVEN** | PROVEN |
| region-local coordinates | metres | right | X east, Y north, Z up | current map `(MapX, MapY)` | absolute in the region (Oakvale house ~34,129) | neighbour offset = ΔMapX/ΔMapY | TNG XY; SHOT2 helper; `WorldGeometryTests` | PROVEN |
| WLD/global map coordinates | metres | right | same | WLD world origin | absolute overworld | local = WLD − (MapX, MapY) | `StartOakValeWest` 3456/736; Lookout 3232/3488; BWD min | PROVEN |
| STB file coordinates | metres (ushort XY, float Z) | right | XY = WLD, Z up | WLD origin | absolute WLD | `WorldSpaces.StbFileToRegionLocal` | `LevTileVertex.WorldX/Y` 3456+ / 3232+ | PROVEN |
| expanded Fable landscape VB | metres | right | region-local after convert | camera | camera-relative (native GPU) | `p_camrel = p_local − cam` | `00BFE050`; `FirstSeenLandscapeDeviceVbIsCameraRelative` | PROVEN |
| camera-relative landscape | metres | right | same | camera | relative | `T(cam)` `00BF46A2` → region-local | `LandscapeWorld`; host `T(cam)` on file verts **DISPROVEN** | PROVEN |
| camera/world | metres | right | Z-up; look on view Z; right = up × look | SHOT2 helper +0/+12/+24 | absolute region-local | `CotScaledView` → view | `00B314E0` / `00B30B50`; not `CreateLookAt` | PROVEN |
| static-object world | metres | right | region-local | region origin | absolute region-local | identity W → view | `009881F0` | PROVEN |
| skinned-character | metres after 0.01 + palette | right | C3D then `dest[group[a0/3]]` | bind-pose dest ≈ I | object then world | `SkinPosition` → `ObjectTransform` | file byte = VS register offset, **not** mesh bone id | PROVEN |
| view space | metres, cot-scaled XY | right | X right, Y camera-up, Z look | camera | relative | `009883F0` P; `clip.w = view.z` | `CotScaledView`; `FirstSeenViewLookIsZ` | PROVEN |
| clip space | homogeneous | DX9 Y-up clip | `clip.xy=view.xy`; `clip.z=m33*z+Q`; `clip.w=view.z` | clip origin | homogeneous | `Dx9VulkanProjection.ToVulkanWvp` Y flip | Y flip is **not** Fable P (**DISPROVEN** bake) | PROVEN |
| Vulkan NDC | NDC −1..1, Y down | Vulkan | X right, Y down | NDC origin | clip/w after Y flip | framebuffer | `NdcYSign=-1` EQUIVALENT translation | EQUIVALENT |

## Map / region origin

`STB world XY − MapX/MapY` is the conversion that puts STB verts in
the same region-local metres as TNG and the SHOT2 helper.

Native camera is the TNG helper position (local ~40, 130), not WLD
~3496, 866. Native landscape `T(cam)` uses that helper. If STB stayed
WLD, terrain would sit kilometres from the house. First-scene house,
father, kid, path, fence, and nearby STB verts meet numerically in
region-local space (`WorldPipelineTests`).

BWD `minX/minY` equals WLD `MapX/MapY`. Neighbour maps use
`ΔMapX/ΔMapY` into the primary local frame.

This subtract is the **semantic equivalent** of the native
file → local step (EQUIVALENT / PROVEN as the unique meeting-space
rule). It is not a leftover host convenience. Applying Fable
`T(cam)` to host STB world verts remains **DISPROVEN**.

`ReadExtras` used to reject verts unless XY was 2000–6000. That
Lookout-only gate dropped Oakvale edge strips (MapY=736).
**DISPROVEN**; extras now accept WLD ushorts 0–20000.

## Landscape WVP

Native: `file → region-local → cam-relative → T(cam) → V → P`

Host: `file → region-local → identity W → V → P`

These clip coordinates match on real STB verts
(`WorldSpaces.NativeLandscapeClip` /
`WorldSpaces.HostLandscapeClip`). Host identity W is therefore
**EQUIVALENT**, not a second projection.

## Locked (do not revert)

- DX9 → Vulkan projection translation (`ToVulkanWvp` only)
- `clip.w = view.z`
- clip-Y at submit, not baked into Fable P
- C3D normals
- static/PALSKIN UV sources
- PALSKIN file blend bytes are VS register offsets
- PALSKIN `group[a0/3]` + packed c38
- father C3D stride 20 / flags 4
- kid C3D stride 28 / flags `0x14`
- host STB vertices are world-space (WLD file; region-local after subtract)
- `T(cam)` on host STB world vertices is **DISPROVEN**
- host landscape uses identity W
