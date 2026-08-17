# First-scene render contract

First-seen New Game is `StartOakValeWest` / `Q_NewOakValeIntro`,
camera `CAM_OVIF_SHOT2`, inside `HerosOldHouse`. This file is the
contract the Vulkan submit path must execute. Values are either
PROVEN from Fable.exe / assets / tests, EQUIVALENT DX9→Vulkan
translations, or UNREAD (left unread — not replaced by a look-better
value).

Classification of the live path is in
[FIRST_SCENE_AUDIT.md](FIRST_SCENE_AUDIT.md). The DX9↔Vulkan matrix
is [DX9_VULKAN_PARITY.md](DX9_VULKAN_PARITY.md).

## ASSETS

| Item | Proven value | Evidence |
|---|---|---|
| Region | `StartOakValeWest` (+ Contains/Sees maps) | `RegionTravel.StartingRegion`, `WorldGeometryTests` |
| House exterior | C3D **6909**, 2 prims, diffuse **345** + **3180** | `MeshFormatTests`, game.bin Graphic |
| House interior | C3D **6911** (`HerosOldHouseInteriorMeshId`) | `MeshFormatTests` |
| House floor 3184 / roof 3182 | materials exist, **no prims** | `GameBin.FirstSeenHouseFloor3184HasPrims=false` |
| Kid | C3D **4300** `MESH_YOUNGHERO_02`, 76 bones, bind-pose | `WorldShading.FirstSeenPlaysAnim=false` |
| Indoor props | bed / table / lamp / rug / fireplace / door / chairs / cupboard / bookshelf | `WorldGeometryTests` |
| Landscape | STB tile strips + edge strips; no invented 1 m fill | `LevFormatTests` |
| House floor land | `PATH_STONEY` / tex **4130** | `WorldGeometryTests` |
| Water | layer `0x20000` empty-out; no 7363 mesh | `LandscapeTextures.FirstSeenWaterDrawShouldSubmit=false` |
| Sky dome | 9×37 ellipsoid, UV (0,0), dest+12 alpha | `SkyPass` |
| Stars / weather mesh | not emitted first-seen | `FirstSeenEmitsInventedStarBillboards=false` |
| Textures | DXT1/DXT5 top mip decoded RGBA8; leftover raw mips 256..4 stored, not uploaded | `TextureFile` |
| Bump on rugs/books | stored in C3D, **not bound** | `FirstSeenBindsC3dBump=false` |

## CAMERA

| Item | Proven value | Evidence |
|---|---|---|
| Name | `CAM_OVIF_SHOT2` | TNG + `00B23B50` bind |
| Kind | helper pos/look/up, **not** `CreateLookAt` | `00B314E0`, `FirstSeenViewUsesCreateLookAt=false` |
| Up | (0,0,1) | `FirstSeenCameraUp` |
| FOV | 0.2 turns = 72° | TNG spline key 0 |
| Two-FOV flag | clear | `FirstSeenTwoFovFlag=false` |
| Letterbox | `cotH` from scaled FOV, `cotV = cotH * (w/h)`; 4:3 leaves H unchanged | `00B30B50` |
| Near / far | 0.1 / 4000 | helper +88/+92 |
| MinZ / MaxZ | 0.1 / 0.99 | `0x01399D44` / `0x3F7D70A4` |
| View | `(right, up, look)` on Z, cot-scaled +128 | `CotScaledView` |
| World (static/PALSKIN/sky) | identity | `00B2FC50` / `009881F0` |
| World (landscape 4 / 0x40) | `T(cam)` | `00BF46A2` |
| Proj (DX9) | XY identity, `clip.w = view.z` (Numerics transpose of exe `M34=Q`/`M43=1`) | `009883F0` |
| WVP | `p * W * V * P` → `c5–c8` | `00988A50` |
| Vulkan Y | `Dx9VulkanProjection.ToVulkanWvp` only | not `009883F0` |

## GEOMETRY

| Item | Proven value |
|---|---|
| C3D space | Z-up centimetres × 0.01 to TNG metres |
| Object basis | `RHSetForward` / `RHSetUp`, right = forward × up |
| Vertex pos | packed 11-11-10 + scale/offset, or float3 |
| Vertex normal | packed 11-11-10 (`PackedDirection`) or float3 at `PackedNormalOffset` |
| UV | packed `int16/2048-8`, or float2; static `oT0=v2`, PALSKIN `oT0=v4` |
| Indices | uint16; strip odd-`t` swap `b,a,c`; list `t*3` |
| Landscape verts | 15-byte file → 24-byte GPU: XYZ + unpacked normal + D3DCOLOR extra |
| Landscape UV | `oT1 = dp4(pos,c40/c41) = (0,0)` first-seen |
| Landscape extra | `oT0.xy = v3.yz` (FG) / `v3` (BG) |
| Skin | bind-pose palettes at `c38`, 3 float4s / bone; no anim |

## SHADERS

| Pass | VS | PS |
|---|---|---|
| Landscape 0x4 | `VSHADER_LANDSCAPE_FOREGROUND` family slot 0 (1-light) | `PSHADER_LANDSCAPE_BACKGROUND` (`mul_x2 t0*v0`) |
| Landscape 0x40 | `VSHADER_LANDSCAPE_FOREGROUND` | `PSHADER_LANDSCAPE_FOREGROUND` (`mul_x2 t1*v0`; `t0.a*v0.a`) |
| Primitives 0x20 static | `VSHADER_STATIC_DIRLIGHT_FOG` | `PSHADER_TEXTURE_DIFFUSE` (`mul v0*c0`; `mul_x2 t0`) |
| Primitives 0x20 kid | `VSHADER_PALSKIN_DIRLIGHT_FOG` | inherit compact PS / `PSHADER_TEXTURE_DIFFUSE` |
| Sky 0x2000 | `VSHADER_INNER_SKY` (`dp4 oPos, v0, c5–c8`) | `PSHADER_INNER_SKY` or `_SIMPLE` — first-seen PS `c0/c1/c2` **UNREAD** |

Constants (first-seen):

- `c0 = (0,1,2,0.5)`, `c1 = (256)×4`
- `c2` = `LinearFogPlane` (start 1000 / end 2000), not inverse row 0
- `c3 = (0, 0.125, 0, 0)` leftover table `0x0139C614`
- `c4 = (0,0,0,0)` on landscape
- `c5–c8` = WVP product
- `c18 = (0,0,0,1)`, `c19 = (0,1,0,0)`, `c20 = (0.25)×3+1`, `c35 = (0,0,0,1)`
- `c40=c41=c42=0`
- PS `c0` = `PSCONST_OUTPUT_FACTOR` `(1,1,1,1)`
- Fog: `oFog = saturate(c0.y - min(dot(pos,c2), c0.y) * c18.w)`
- Light: `dp3 n,-c19; max; square; *c20; mad c35; add c3`

## STATE

| RS | First-seen | Vulkan |
|---|---|---|
| ZFUNC | LESSEQUAL | `LESS_OR_EQUAL` |
| ZENABLE / ZWRITE | UNREAD (D3D default TRUE used) | test+write on |
| CULLMODE | CCW (3) | FrontFace CCW + Cull Back (after Y flip) |
| FILLMODE | UNREAD | FILL (TEMPORARY) |
| ALPHABLEND | off (opaque); PALSKIN SRCALPHA/INVSRCALPHA | matching pipelines |
| ALPHATEST | UNREAD | no discard (TEMPORARY) |
| COLORWRITE | UNREAD | RGBA |
| FOGENABLE | 1 | VS `oFog` + `rgb * oFog` |
| FOGCOLOR | `0xFF000000` | clear/blend black |
| FOGTABLE/VERTEX | NONE | unused (VS fog) |
| Sampler | UNREAD | LINEAR/REPEAT/MaxLod=1 (TEMPORARY) |
| Stencil / FFP lights / specular addend | UNREAD / oD1 unused | off |

## FRAME

Walk `ScenePasses.Registration` (34 layers):

1. bit `0x4` landscape BG (`mul_x2` t0)
2. bit `0x40` landscape FG (`mul_x2` t1)
3. bit `0x20` primitives (static + PALSKIN)
4. bit `0x2000` sky else-path
5. bit `0x20000` water — empty-out, no mesh
6. Fade overlay if script alpha &gt; 0
7. PlayAVI 2D blit if the interpreter is in AVI (separate COM path)
8. Present

Do not reorder for Vulkan convenience. Shadows / `0x400000` sky / HUD / particles stay UNREAD and are not submitted.
