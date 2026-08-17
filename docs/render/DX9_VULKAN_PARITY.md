# DX9 → Vulkan parity matrix

Statuses: **PROVEN**, **EQUIVALENT**, **UNREAD**, **DISPROVEN**, **TEMPORARY**.

DISPROVEN rows were corrected in the shipped first-scene path.
See [FIRST_SCENE_CONTRACT.md](FIRST_SCENE_CONTRACT.md) and
[FIRST_SCENE_AUDIT.md](FIRST_SCENE_AUDIT.md).

Translations live in `src/Fable.Render/Parity/Dx9Vulkan/`.

| Semantic | Fable evidence | DX9 value | Vulkan equivalent | Status | Notes |
|---|---|---|---|---|---|
| Depth compare | PARITY lock; `D3dDeviceState.FirstSeenZFunc` | `D3DCMP_LESSEQUAL` (4) | `VK_COMPARE_OP_LESS_OR_EQUAL` | PROVEN | `Dx9VulkanDepth` |
| Z enable / write | D3D default; first-seen write site unread | TRUE / TRUE | test=1 write=1 | TEMPORARY | not invented off |
| Cull mode | `0x01396FB0` = 3; `00B24850` / `00BB2540` | `D3DCULL_CCW` | `VK_CULL_MODE_BACK_BIT` | PROVEN | PALSKIN inherits |
| Front face | same + clip-Y flip proof | screen-CCW culled; clip-CCW kept | `VK_FRONT_FACE_COUNTER_CLOCKWISE` | EQUIVALENT | `Dx9VulkanRasterState` |
| Projection | `009883F0` XY identity, Z terms, VS `clip.w=view.z` | `M22=+1` (Numerics `M34=1`/`M43=Q`) | `wvp * diag(1,-1,1,1)` | EQUIVALENT | Y flip is **not** Fable P. Exe memory is `M34=Q`/`M43=1`; host stores the upload-transpose |
| Viewport | camera +176/+180; MinZ/MaxZ baked in P | pixel viewport; MinZ/MaxZ in P | x/y=0, w/h=fb, depth 0..1 | EQUIVALENT | no second MinZ scale |
| Half-pixel | — | UNREAD | none | UNREAD | not invented |
| Sampler filtering | — | UNREAD | LINEAR / LINEAR / LINEAR mip | TEMPORARY | `Dx9VulkanSamplerState` |
| Address modes | — | UNREAD (D3D default WRAP) | REPEAT | TEMPORARY | |
| Texture formats | `009BE8xx` FourCC DXT1/DXT3, pool SCRATCH | DXT1/DXT5 / RGBA8 | decode top mip → `R8G8B8A8_UNORM` | EQUIVALENT | sampled format after SCRATCH UNREAD |
| sRGB handling | — | UNREAD | UNORM | UNREAD | |
| Blend state | PALSKIN `00BD3867` SRCALPHA/INVSRCALPHA | 5 / 6, enable 1 | `SRC_ALPHA` / `ONE_MINUS_SRC_ALPHA` | PROVEN | opaque = off |
| Alpha test | slot `D3DRS_ALPHATESTENABLE` | UNREAD first-seen | no discard | UNREAD | |
| Fog | `00B46890` FOGENABLE=1; VS `oFog`; FOGCOLOR black | vertex fog, table/vertex NONE | `oFog` interpolate + `rgb*oFog` | PROVEN | |
| Vertex layout | FVF `0x112` stride 32; land stride 24; packed C3D | pos/n/uv / extra D3DCOLOR | `MeshVertex` float streams | EQUIVALENT | normals now decoded |
| Index format | C3D / STB uint16 | INDEX16 | unwound triangle list | EQUIVALENT | |
| Primitive topology | strip or list | D3DSTRIP / D3DLIST | `TRIANGLE_LIST` | EQUIVALENT | odd-index swap preserved |
| Shader constants | `00988A50` c5–c8; fog c2; lights c19/c20/c35 | register file | push `mat4` + fog/light vec4s | EQUIVALENT | no extra transpose |
| Draw order | `00B26A75` 34 layers | 0x4, 0x40, 0x20, 0x2000 | same rank sort | PROVEN | water 0x20000 empty |
| Color / clear | FOGCOLOR `0xFF000000` | ARGB black | clear (0,0,0,1) | PROVEN | `Dx9VulkanColor` |
| Fill mode | — | UNREAD (D3D SOLID) | FILL | TEMPORARY | |
| Color write | — | UNREAD (0xF) | RGBA | TEMPORARY | |
| Stencil | — | UNREAD | disabled | UNREAD | |
| FFP lighting | first-seen VS writes `oD0` | unused | unused | PROVEN | no FFP |
| 2D / ortho | PlayAVI 2D only | separate | overlay/video pipelines | UNREAD | not first-seen 3D |
