# First-scene path audit

Trace of the live New Game submit path at the time of the
DX9→Vulkan parity layer. Each value is one of:

1. **proven Fable behaviour**
2. **mathematically equivalent DX9→Vulkan translation**
3. **generic Vulkan default**
4. **earlier visual approximation**
5. **assumption**
6. **temporary/debug**

DISPROVEN items were corrected in this session. UNREAD items
were not filled with look-better values.

## Trace

```
asset (graphics.big / textures.big / STB .lev / TNG / shaders.big)
 → MeshFile / TextureFile / LevTileMesh / SkyGeometry
 → WorldGeometry (TNG transform, landscape planes, kid bind-pose)
 → MeshBatches (pass bits 0x4, 0x40, 0x20, 0x2000)
 → ScriptedCamera (SHOT2 helper view + 009883F0 P)
 → Dx9VulkanProjection.ToVulkanWvp (clip Y only)
 → LineShaders mesh VS/PS (token math)
 → Vulkan pipeline (Dx9VulkanDepth / Raster / Blend / Sampler)
 → DrawMeshBatches
 → fade / PlayAVI / Present
```

## Classification

| Site | Value | Bucket | Notes |
|---|---|---|---|
| C3D parse (pos, UV, indices, materials, bones) | packed/unpacked layouts | proven | `MeshFile`, `MeshFormatTests` |
| C3D vertex normal | packed 11-11-10 / float3 at `PackedNormalOffset` | proven | was dropped (face-normal only) — **DISPROVEN, corrected** |
| PALSKIN decl / skin | `00A8FD40` stride/flags; father 20/4 packed; `00A8E770` group+23/+24; `00BCFB00` dest[bone*64]→c38; file byte = `a0` | proven | Host was indexing `palettes[fileByte]` as mesh bone — **DISPROVEN**, remapped `group[a0/3]` |
| Landscape `T(cam)` on host STB | Fable VB is cam-relative; file verts are world | proven / equivalent | **DISPROVEN** host `p_world*T(cam)` — submit uses identity |
| C3D → TNG transform | cm × 0.01, RHSetForward/Up | proven | `WorldGeometry.ObjectTransform` |
| Landscape tile verts | 15-byte + 11-11-10 normal + extra | proven | `LevTileMesh` |
| Landscape strip unwind | PrimitiveCount+2, odd swap | proven | |
| Landscape `oT1` | `(0,0)` from c40=c41=0 | proven | `ProjectOt1` |
| Landscape `n.Z < 0` rewind | force +Z face | DISPROVEN | no exe write; removed. Strip unwind is D3D odd `(b,a,c)` via `LandscapeStrip` |
| Texture header / DXT FourCC | 31/32/35, SCRATCH create | proven | `009BE8xx` |
| DXT → RGBA8 CPU decode | top mip | equivalent | sampled GPU format after SCRATCH UNREAD |
| Lower mips | stored, not uploaded | temporary | MaxLod=1 |
| sRGB | UNORM | unread / temporary | |
| SHOT2 helper / view axes | `(right, up, look)`, cot-scaled | proven | `00B314E0` / `00B30B50`; look-on-Z so `clip.w=view.z` |
| `009883F0` P | XY identity, Z terms, `clip.w=view.z` | proven | `FirstSeenDx9Projection` (upload-transpose) |
| WVP product | `p * W * V * P` → c5–c8 | proven | `00988A50` |
| Vulkan clip Y | `wvp * diag(1,-1,1,1)` | equivalent | **was baked into Fable P as `VulkanNdcYSign` — DISPROVEN site, moved** |
| Half-pixel | none | unread | not invented |
| 2D ortho / cinematic leftover | — | unread | |
| Fog plane / `oFog` | linear view-Z, saturate | proven | `00B47630` |
| Dirlight | `dp3 n,-c19; max; sq; *c20; mad c35; +c3` | proven | tokens |
| Static/PALSKIN UV | `oT0 = v2 / v4` = mesh UV | proven | **GLSL sampled `extra.yz` — DISPROVEN, corrected** |
| Landscape FG/BG UV | extra swizzle + `oT1=(0,0)` | proven | |
| Sky PS `mul_x2 c2` | stand-in `t1*v0*v0.w` | temporary | `FirstSeenSkyMode2IsStandIn` |
| Sky PS constants | — | unread | no first-seen writer |
| Pass order | 0x4 → 0x40 → 0x20 → 0x2000 | proven | `ScenePasses.Registration` |
| Water | empty-out | proven | |
| Intra-pass texture sort | group by tex id | assumption | convenience; pass rank still exe order |
| ZFUNC | LESSEQUAL | proven | PARITY lock |
| ZENABLE / ZWRITE | TRUE | temporary | D3D default; first-seen write UNREAD |
| CULL CCW | 3 | proven | `0x01396FB0` |
| FrontFace after Y flip | CCW + Back | equivalent | `Dx9VulkanRasterState` |
| PALSKIN blend | SRCALPHA / INVSRCALPHA | proven | `00BD3867` |
| Sampler LINEAR/REPEAT | — | temporary | D3DSAMP UNREAD |
| FILL / COLORWRITE | FILL / RGBA | temporary | UNREAD writes |
| Alpha test | off | temporary | UNREAD |
| Stencil | off | unread | |
| FFP lighting / specular | unused (VS oD0) | proven unused | `oD1` not written |
| Clear | black `0xFF000000` | proven | FOGCOLOR |
| Viewport 0..1 depth | — | equivalent | MinZ/MaxZ already in P |
| Push-constant WVP | memcpy row-major | equivalent | GLSL reads as transpose |
| Gizmo lines / F2 fly | — | temporary/debug | not first-seen |
| PlayAVI | COM / 2D blit | proven separate | not rewritten |
| DLSS / HDR / RT | — | not in parity | |

## Corrections this session

1. **Projection Y site.** `009883F0` writes `M22=+1`. The Vulkan NDC Y
   flip is `Dx9VulkanProjection.ToVulkanWvp`, applied in
   `VulkanLineRenderer.Draw`. Cameras emit the DX9 WVP.
2. **Static/PALSKIN UV.** GLSL mode ≥ 2.5 samples `fragUv` (`oT0=v2/v4`),
   not `extra.yz`.
3. **C3D vertex normals.** `MeshFile` decodes packed/unpacked normals
   into `NormalA/B/C`; `WorldGeometry` transforms them. First-seen VS
   `dp3 n, v1, -c19` no longer sees face-only normals.
4. **PALSKIN skin + normal.** VS `dp3 v3, r4/r5/r6` after the same
   palette rows. Host now calls `SkinNormal`. D3DCOLOR `.zyxw` is
   memory BGRA `[0,1,2,3]` (not `.zwxy`). First-seen dest is
   identity (full 16 entries).
5. **Landscape world space.** `00BF46A2` `T(cam)` is for a
   camera-relative VB. Host STB tiles are world-space. Submit uses
   identity world (`HostLandscapeViewProjection`). Applying `T(cam)`
   to world verts put the exterior at `p+cam` (black).


## UNREAD leftover (do not invent)

- D3DSAMP MAG/MIN/MIP/ADDRESS / anisotropy / LOD bias
- sRGB vs linear on the sampled view
- ALPHATESTENABLE / ALPHAREF / ALPHAFUNC
- FILLMODE / COLORWRITEENABLE first-seen writes
- ZENABLE / ZWRITE SetRenderState site
- stencil, scissor
- half-pixel
- 2D / ortho projection
- cinematic vs gameplay leftover projection
- sky PS `c0/c1/c2` bank values and quality bit
- water mesh when bind would run
- particles / HUD / shadows / `0x400000` sky
- PlayAVI apply body (separate subsystem)
