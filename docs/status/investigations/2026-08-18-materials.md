# 2026-08-18 — DX9 material / lighting pipeline

Investigation only. No production source was modified.
`EngineLifecycle.cs` was not edited. No brightness multiplier
was invented.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**EQUIVALENT** / **TEMPORARY**.

Read first: [G-dx9-vulkan.md](G-dx9-vulkan.md),
[C-terrain-static-map.md](C-terrain-static-map.md),
[DX9_VULKAN_PARITY.md](../../render/DX9_VULKAN_PARITY.md).

Audited:

- `src/Fable.Render/Parity/Dx9Vulkan/*`
- `src/Fable.Formats/WorldShading.cs`
- `src/Fable.Formats/Levels/LandscapeTextures.cs`
- `src/Fable.Formats/Shaders/ShaderProgram.cs`
- `src/Fable.Formats/Scene/ScenePass.cs` (`D3dDeviceState`)
- `src/Fable.Formats/Meshes/MeshFile.cs` (`MeshMaterial`)
- `src/Fable.Render/LineShaders.cs` / `WorldShadingPush.cs`
- `src/Fable.Render/VulkanLineRenderer.Textures.cs`
- `tools/Fable.ExeIndex/out/01-sections/shader-tokens/*`
- `tools/Fable.ExeIndex/out/01-sections/landscape-trace/*`
- `tools/Fable.ExeIndex/out/01-sections/newgame-trace/{tod-blend,light-apply,lighting-mgr-ctor,c3d-material-serialize,diffuse2x}*`
- Live exe floats at `0x0139C5D8` / `0x0139C614`

No visual tuning. A formula that matches the exe is not a
screenshot fix.

---

## Verdict (read this first)

Native first-seen 3D is **programmable** (`vs_1_1` / `ps_1_1`),
not FFP `SetMaterial` / `SetLight` / `D3DRS_LIGHTING`. RGB is:

```
v0.rgb = max(n · −c19, 0)² * c20 + mad(−min(ndl,0), c35) + c3
ps.rgb = sat(2 * tex * v0)     // landscape FG/BG, TEXTURE_DIFFUSE
```

First-seen constants after lighting ctor `00B482A0` + TOD copy
of **record 0**:

| Reg | Value | Source |
|---|---|---|
| `c19` | `(0, 1, 0, 0)` | ctor `[esi+48]` |
| `c20` | `(0.25, 0.25, 0.25, 1)` | record 0 `+0` (`0x3E800000` × 3) |
| `c35` | `(0, 0, 0, 1)` | record 0 `+32` |
| `c3` | `(0, 0.125, 0, 0)` | per-cell table `0x0139C614` row 0 |
| `c0` (VS) | `(0, 1, 2, 0.5)` | LayoutBasic |
| `c0` (PS static) | `(1, 1, 1, 1)` | `PSCONST_OUTPUT_FACTOR` |
| `c40`/`c41` | `(0,0,0,0)` | D3D default; no first-seen writer |

Lookout tiles are unit **+Z** (`LevFormatTests`). `n · −c19` =
`n · (0,−1,0) = 0`. Unlit RGB is leftover **`c3` only**.
`mul_x2` then yields `(0, 0.25, 0) * tex`. Combined with
first-seen `oT1 = dp4(pos, c40/c41) = (0,0)`, Lookout FG is
**one texel × leftover green**. That is
`Dx9VulkanShaderConstants.UnlitRgbIsC3Leftover`.

**This is why the host scene is dark.** It is also why first-seen
**native** Lookout is the same dark green. Native is **not**
brighter because a `*k` is missing on this formula.

`ps_1_1` dest shift `_x2` is already in every first-seen world
PS (`PSHADER_LANDSCAPE_FOREGROUND` / `_BACKGROUND` /
`PSHADER_TEXTURE_DIFFUSE` / sky). Inventing another ×2 / ×4 /
0.28+0.72 N·L / gray ambient is **DISPROVEN**
(`PARITY.md` invented-lighting rows; `ShaderFormatTests`).

What **does** make a running native frame look brighter, and is
**not** a material multiplier:

1. **Sky bit `0x2000`** — native `00B662F0` after `0x20`,
   `PSHADER_INNER_SKY` `mul_x2_sat …, c2` on a bright dome.
   Live Concat often has **no** `0x2000`. Clear is fog black.
   Backdrop black is not a land/prop scale.
2. **TOD blend `00B46C80`** runs **every** landscape draw
   (ctor writes `[esi+228]=0xF`, so bit 0 is set). First-seen
   `+224/+225/+226 = 0` copies **record 0** (the 0.25 table).
   Later 112-byte records / `+224` updates are **UNREAD** as
   first Present. Host always pushes ctor `DirLight*`.
3. **`oT1=(0,0)`** is the first-seen albedo UV. Host
   `ProjectOt1` is the same. World-XY `×0.125` as albedo UV
   is **DISPROVEN** (`UvScale` is the **c3 leftover table**,
   not `c40`).
4. **Point-light pack** exists (`c21+`, family 2/4/5-light
   VS). First-seen packed count is **0**, family slot **0**.
   Gather `00B46280` take is **UNREAD**.

Do not “fix” darkness by raising `c20`, adding ambient, or
inventing UVs. Recover a later writer (TOD record, `c40`,
sky PS `c2`, or a `SetVSConstantF(19/20)` site after
environment bind) or accept first-seen leftover green.

---

## Mapping table (requested states → `Dx9Vulkan*`)

There is **no** `Dx9VulkanMaterial` / `Dx9VulkanTextureStage`
type. First-seen material state lands on the existing catalog.

| Requested state | Native first-seen 3D | `Dx9Vulkan*` field | Class |
|---|---|---|---|
| **Texture lookup** | FG: `tex t0` (mask) + `tex t1` (albedo). BG/static: `tex t0` only. UV: FG `oT0=v3.yz`, `oT1=dp4(c40/c41)=(0,0)`; BG `oT0=v3`; static `oT0=v2`; PALSKIN `oT0=v4`. | `Dx9VulkanSamplerState` (how); `Dx9VulkanTextureFormat` (what); `Dx9VulkanShaderConstants` (`Ot1` via `LandscapeTextures.ProjectOt1`); `Dx9VulkanVertexFormat` (streams) | **PROVEN** contracts. Host FG `ot1=fragUv` is **EQUIVALENT** because landscape `UvA=(0,0)`. |
| **Stages** | FG: stage 0 `00BF50E0` (`TextureId1` mask from `cell+1468`), stage 1 `00BF5491` (`TextureId` albedo). Static: **1** stage, `00BB301E`. After pass: `00B67510` unbinds 0/1/2 (`SetTexture(stage,0)` vtbl+260). Bump stored, **not** bound. | No stage type. Host `DrawMeshBatches` swaps set0/set1 on FG. `Dx9VulkanTextureFormat` upload. | **EQUIVALENT** FG/BG/static RGB binds. **PARTIAL**: host always binds two sets; native static is one. |
| **Stage ops** | Not FFP `COLOROP`. RGB is the **PS token**. FG: `mul_x2_sat r0.xyz, t1, v0` + `mul_sat r0.w, t0.w, v0.w`. BG: `mul_x2_sat t0*v0`. Static: `mul r0, v0, c0` then `mul_x2 r0.xyz, t0, r0` (**no `_sat`** on the ×2). | `Dx9VulkanShaderConstants` / GLSL replica. No `COLOROP` enum. | **PROVEN** tokens. Host `clamp()` on static ×2 is **PARTIAL** vs unsaturating `mul_x2`. |
| **Diffuse** | VS `oD0.xyz` = dirlight polynomial, **not** `D3DMATERIAL9.Diffuse`, **not** tile ExtraRgb, **not** C3D vertex colour. Static FVF `0x112` = `XYZ\|NORMAL\|TEX1` (no diffuse). | `Dx9VulkanShaderConstants.DirLightDirection/Color/LitColor/C3`; `Dx9VulkanVertexFormat` (color unused by first-seen `oD0`) | **PROVEN**. Multiplying albedo by ExtraRgb is **DISPROVEN** (Lookout magenta). |
| **Ambient** | No FFP ambient. Unlit addend is leftover **`c3`** from UV table `0x0139C614`, **not** a designed ambient. `c35.rgb=0` so the `mad c35` term is 0. | `Dx9VulkanShaderConstants.C3` / `UnlitRgbIsC3Leftover` / `LitColor` | **PROVEN** leftover. Invented gray/sky ambient is **DISPROVEN**. |
| **Specular** | `D3DRS_SPECULARENABLE` slot exists. PALSKIN bind writes **1**. First-seen VS families do **not** write `oD1`. FFP specular addend stays 0. No `LIT` opcode. | No specular type. `D3dDeviceState.SpecularEnable` only. | **PROVEN** unused addend. |
| **Emissive** | C3D `MeshMaterial.SelfIllumination` + `IlluminationMapId` parsed (`00ABF6B0` +32/+36). First-seen PS does not sample them. `FirstSeenBindsC3dBump=false`. | None. | **PROVEN** omit on first-seen. Host also drops. **EQUIVALENT** omit. |
| **Alpha test / ref / func** | Slots exist (`D3DRS_ALPHATESTENABLE=15`). First-seen landscape / static-lit **write UNREAD**. | `Dx9VulkanBlendState.FirstSeenAlphaTest=false` (no `discard`) | **UNREAD** / host **TEMPORARY**. |
| **Blend** | Landscape **off**. PALSKIN `SRCALPHA(5)` / `INVSRCALPHA(6)`, enable 1. `BLENDOP` write **UNREAD** (D3D ADD). | `Dx9VulkanBlendState.Opaque` / `PalskinSrcAlpha` | **PROVEN** factors. **PARTIAL** `BLENDOP` + host copies color factors to alpha. |
| **Sampler / address / filter** | First-seen `SetSamplerState` MAG/MIN/MIP/ADDRESS **UNREAD**. D3D defaults POINT / NONE / WRAP. | `Dx9VulkanSamplerState.FirstSeenTemporary`: LINEAR / REPEAT / `MaxLod=1` | **UNREAD** native. Host **TEMPORARY**. Do not switch to POINT without a dump. |
| **Lighting enable** | No first-seen `D3DRS_LIGHTING` / `LightEnable` / `SetLight` / `SetMaterial` on the `00988A50` path. Lighting is VS `oD0`. | `Dx9VulkanShaderConstants` (VS polynomial). No FFP enable flag. | **PROVEN** unused FFP. |
| **Vertex colour** | Landscape Extra at dest+20 is **oT0**, not `oD0`. Static has no diffuse. Host `Vert()` maps Color=0 → `(1,1,1)`; landscape writes `Color=1`. | `Dx9VulkanVertexFormat` (decoded `MeshVertex.Color`); `Dx9VulkanColor.FromD3dColorBgr` for Extra | **EQUIVALENT** first-seen (multiply by 1). Extra-as-colour **DISPROVEN**. |
| **Fog interaction** | `FOGENABLE=1`, `FOGCOLOR=0xFF000000`, table/vertex mode 0. VS `mad oFog, min(dp4(pos,c2),c0.y), −c18.w, c0.y`. Blend `rgb*oFog + (1−oFog)*black`. | `Dx9VulkanShaderConstants.FogPlane/FogColor`; `Dx9VulkanColor.FirstSeenClear` | **EQUIVALENT** opcode + black. Plane **numbers** follow camera (B **DIVERGE**). |
| **Material constants** | VS: `c0–c8`, `c18–c20`, `c35`, leftover `c3`, unread `c40–c42`. PS static `c0=PSCONST_OUTPUT_FACTOR=(1,1,1,1)`. Sky PS `c0/c1/c2` **UNREAD**. | `Dx9VulkanShaderConstants` (`C0/C1/C3/DirLight*/LitColor/Fog*/PackWvp`) | **EQUIVALENT** first-seen packing. Sky PS **UNREAD**. |
| **Shader selection** | Family slot **0** (`SelectFamilySlot` ignores packed count; remap is ctor zero). VS: `VSHADER_LANDSCAPE_FOREGROUND` / `VSHADER_STATIC_DIRLIGHT_FOG` / `VSHADER_PALSKIN_DIRLIGHT_FOG`. PS as above. | Host `ScenePasses.ShaderMode` 0/1/2/3 → one GLSL pair. `Dx9VulkanShaderConstants` | **EQUIVALENT** RGB contracts. **DISPROVEN** as PALSKIN `c38` / sky PS. |
| **FFP vs programmable** | First-seen 3D is **vs_1_1 / ps_1_1**. `CEngineStateBlockDiffuse2X` `0098B5E0(2)` is a **device-wrapper state-block apply** on bit `0x40`, body **UNREAD**. Name is RTTI, not a second ×2 on top of the PS. | No FFP type. Blend/raster/depth/sampler cover the RS the block might restore. | **PROVEN** programmable. Diffuse2X body **UNREAD**. Do not add a second ×2. |

---

## 1. Texture lookup

### Native

Dumps: `shader-tokens/vshader-landscape-foreground.md`,
`vshader-landscape-background.md`,
`vshader-static-dirlight-fog.md`,
`vshader-palskin-dirlight-fog.md`,
`pshader-landscape-foreground.md`.

| Pass | Sample | UV |
|---|---|---|
| FG `0x40` | `tex t0`, `tex t1`; RGB is **t1** | `mov oT0.xy, v3.yz`; `dp4 oT1, pos, c40/c41` |
| BG `0x4` | `tex t0` | `mov oT0, v3` (then `oT0.w=c0.y`) |
| Static `0x20` | `tex t0` | `mov oT0, v2` (FVF TEX1) |
| PALSKIN | `tex t0` (same PS) | `mov oT0, v4` |

`c40`/`c41`: no `def` in the FG VS, no layout field 40, no
first-seen `SetVSConstantF(40/41)`. `push 40` hits in
`0x00B60000–0x00C00000` are other functions, not per-cell
`00BF4570`. Shared setup `00B674B3` `mov [esp+40],0` is a
**stack** offset. D3D default unwritten VS const is 0 →
**`oT1=(0,0)`**. `LandscapeTextures.FirstSeenOt1UsesDeviceDefault`.

`v3` is the dest+20 D3DCOLOR from `00BFE050` (BGR extra).
Lookout extra: byte0=`0xFF` (`v3.x=1`), G/B ≈ 0.5. That is
**t0 alpha / BG UV**, not albedo.

### Host

`LevTileMesh.Add` writes `Uv = ProjectOt1(pos) = (0,0)` and
`Color = (1,1,1)`, Extra = file extra. GLSL:

```
ot0 = mode0 ? extra.xy : mode1 ? extra.yz : fragUv
ot1 = fragUv
```

FG samples set1 at `fragUv=(0,0)`. **EQUIVALENT** first-seen.

World-XY `×0.125` as albedo UV is **DISPROVEN**. `UvScale=0.125`
is table `0x0139C5D8` uploaded to **c2** then fog-restored.

### Map

`Dx9VulkanShaderConstants` (no `c40` pack — default 0),
`Dx9VulkanVertexFormat` (decoded UV/extra),
`Dx9VulkanColor.FromD3dColorBgr` (extra).

---

## 2. Stages and ops

### Bind

`00BF50E0`: `SetTexture(0, [cell+1468+…])` device vtbl+260.
`00BF5491`: `push 1` then the same vtbl — stage 1.
Primary `TextureId` is **t1** (FG RGB). `TextureId1` is **t0**.

Static `00BB30A0` pushes texture-count **1**. `00BB301E`
`SetTexture(stage, [array+4*stage])`. House C3D stores bump
ids (rugs 1740, books 2315); first-seen still one stage.
`WorldShading.FirstSeenBindsC3dBump=false`.

`00B67510` after the landscape pass: `SetIndices(0)`,
`SetStreamSource(0,0,0,0)`, `SetTexture(0/1/2, 0)`.

### Ops

Not `D3DTSS_COLOROP`. The PS **is** the op.

```
PSHADER_LANDSCAPE_FOREGROUND:
  tex t0
  tex t1
  mul_x2_sat r0.xyz, t1, v0
  mul_sat    r0.w,   t0.w, v0.w

PSHADER_LANDSCAPE_BACKGROUND:
  tex t0
  mul_sat    r0.w,   t0, v0
  mul_x2_sat r0.xyz, t0, v0

PSHADER_TEXTURE_DIFFUSE / _FOG:
  tex t0
  mul r0, v0, c0
  mul r0.w, t0, r0
  mul_x2 r0.xyz, t0, r0     ; NO _sat (tokens)
```

`PARITY.md` once called `TEXTURE_DIFFUSE_FOG` a “plain mul”.
The dump is **`mul_x2`**. That row is **DISPROVEN**.

Host GLSL `clamp(t * v0 * 2, 0, 1)` matches landscape `_sat`.
Static unsaturating `mul_x2` vs host `clamp` is **PARTIAL**
(8-bit RT still saturates at present).

`CEngineStateBlockDiffuse2X` `0098B5E0(2)` on bit `0x40` is
**PROVEN as the call**. The apply body compares wrapper
records and dirties bits; it is **not** a recovered
`COLOROP=MODULATE2X` write. Adding a second ×2 because the
RTTI says “2X” is **DISPROVEN**.

### Map

`Dx9VulkanBlendState` (alphablend off on these passes).
No stage-op type — do not invent one to hold MODULATE2X.

---

## 3. Diffuse / ambient / specular / emissive

### Diffuse = VS `oD0`, not a D3DMATERIAL

All three first-seen VS families:

```
dp3 r, n, −c19
max r.x, r.x, c0.x        ; c0.x = 0
min r.y, r.y, c0.x        ; min(ndl, 0)
mul r.x, r.x, r.x
mul r, r.x, c20
mad r, −r.y, c35, r
add …, c3                 ; oD0.xyz
```

`c35.rgb=0` → `mad` is a no-op. RGB =
`max(n·−c19, 0)² * c20 + c3`.

`WorldShading.EvaluateDirLightRgb`:

| n | v0 | after `mul_x2` on white tex |
|---|---|---|
| `+Z` (floors) | `(0, 0.125, 0)` | `(0, 0.25, 0)` |
| `−Y` (faces the ctor dir) | `(0.25, 0.375, 0.25)` | `(0.5, 0.75, 0.5)` |
| 0 / degenerate | leftover `c3` | `(0, 0.25, 0)` |

Locked by `Dx9VulkanParityTests` /
`CameraProjectionTests.First_seen_fog_c2_…`.

Host evaluates the **same polynomial in the FS** from
interpolated n. Per-vertex `oD0` vs per-pixel n is
**PARTIAL** (same formula, different interpolant). Not a
brightness scale.

`MeshBatches.Vert` uses Color if non-zero else `(1,1,1)`.
Landscape writes `(1,1,1)`. C3D leaves Color default 0 →
host substitutes 1. First-seen VS does **not** multiply
by a vertex-colour stream. **EQUIVALENT**.

### Ambient = leftover `c3`, not a light

Exe floats (2026-08-18 `floats 0x0139C614 16`):

```
0x0139C614  0, 0.125, 0, 0, 0, -0.125, 0, 0, 0, -0.125, …
```

Per-cell `00BF5170` `00989A60(3, 0x0139C614 + [ebx+40]*12)`
uploads **one float4** (`SetVSConstantF` count 1, vtbl+376)
to **c3** (inner register base 0 + slot 3). Row 0 is
`(0, 0.125, 0, w)` with `w` from the third stdcall arg
(first-seen lock `w=0`).

`[ebx+40] ≠ 0` selects another 12-byte row (`(0,0,−0.125)`
…). First-seen Lookout lock is **row 0**. Fog flush
restores **c2 only**. Draw order is landscape `0x40` then
`0x20`, so house / kid **keep** this leftover.

`Dx9VulkanShaderConstants.UnlitRgbIsC3Leftover = true`.
GLSL hardcodes `vec3(0.0, 0.125, 0.0)`.

**This is a UV-scale table reused as a lighting addend**,
not `D3DMATERIAL.Ambient` and not SKY_DEF haze.

### Specular

`D3dDeviceState.SpecularEnable=29`. PALSKIN
`FirstSeenPalskinSpecularEnable=1`. No `oD1`, no `LIT`.
**PROVEN** unused.

### Emissive

`00ABF6B0` serializes C3D material stride 48: name,
decal/diffuse/bump/reflection/illumination ids, MapFlags,
SelfIllumination float, Flag0–3. First-seen static PS
samples **t0 only**. Host `MeshFile` parses the fields and
does not push them. **EQUIVALENT** omit.

### Map

`Dx9VulkanShaderConstants.{DirLightDirection,DirLightColor,LitColor,C3,UnlitRgbIsC3Leftover}`.

---

## 4. Alpha test / ref / blend

| RS | Native first-seen | Host | Class |
|---|---|---|---|
| `ALPHABLENDENABLE` landscape | off | `Dx9VulkanBlendState.Opaque` | **PROVEN** |
| PALSKIN src/dst | 5 / 6 (`00BD3867` / `00BD38D4`) | `PalskinSrcAlpha` | **PROVEN** |
| `BLENDOP` | UNREAD (D3D ADD) | `BlendOp.Add` | **TEMPORARY** |
| `ALPHATESTENABLE` / `ALPHAREF` / `ALPHAFUNC` | slots exist; write UNREAD | no `discard` | **UNREAD** |
| FG `oD0.w` | `(dp3(r2,c42)+c42.w)*v3.x`; `c42` unwritten → **0** | GLSL `v0a=0` in mode 1 | **EQUIVALENT** |
| BG/static/PALSKIN `oD0.w` | `mov oD0.w, c0.y` = 1 | `v0a=1` | **EQUIVALENT** |

Landscape alphablend is off, so FG `oD0.w=0` does **not**
hide RGB.

Host `SrcAlphaBlend: hasBones` puts **every** PALSKIN
triangle on the alpha pipeline. Native first-seen opacity
`0xFF` skips a block. **PARTIAL** granularity (E).

### Map

`Dx9VulkanBlendState`.

---

## 5. Sampler / addressing / filter / format

`SetSamplerState` first-seen **UNREAD**. D3D9 defaults:
MAG/MIN **POINT**, MIP **NONE**, ADDRESS **WRAP**.

Host `Dx9VulkanSamplerState.FirstSeenTemporary`: LINEAR /
LINEAR mip / REPEAT / `MaxLod=1` (only the uploaded top
mip). One sampler for every texture.

`009BE8B0` CreateTexture DXT FourCC, `D3DPOOL_SCRATCH`.
Host `Dxt.Decode` mip0 → `R8G8B8A8Unorm`
(`Dx9VulkanTextureFormat.SampledFormat`). Lower mips stay
in the file. Sampled-view sRGB vs linear **UNREAD**.
`TreatAsSrgb=false`. **PARTIAL**.

Missing-id bind uses `GpuTexture.Fallback` =
`(115, 128, 97)` olive, **not** black. A failed lookup is
muted olive × leftover lighting, not a black framebuffer.

Filter choice does not explain “extremely dark”. Do not
retune LINEAR/POINT as a brightness fix.

### Map

`Dx9VulkanSamplerState`, `Dx9VulkanTextureFormat`.

---

## 6. Lighting enable, FFP vs programmable, shader selection

### FFP is unused

No first-seen `SetTransform` / `SetLight` / `SetMaterial` /
`LightEnable` / `D3DRS_LIGHTING` on the WVP/`oD0` path
(B, G). `WorldShading.FirstSeenVsUsesLitC35=false`.

### Programmable family

Ctor `00BB5040` / `00B69000` resize the family to 6 slots.
Draw `00BA2677` `min(count,5)` then `remap[count]` at
family+32. MainScene leaves +32..+52 as ctor **zeros**.
`SelectFamilySlot` is **0**. First-seen packed count is
**0** (`[esi+160]=0`; add-light message 16 is not sent
from `0089FAA8`).

| Slot 0 VS | Used by |
|---|---|
| `VSHADER_LANDSCAPE_FOREGROUND` | bit `0x40` (and host mode 0/1) |
| `VSHADER_STATIC_DIRLIGHT_FOG` | bit `0x20` static |
| `VSHADER_PALSKIN_DIRLIGHT_FOG` | PALSKIN drain |

2/4/5-light VS exist and read `c21+`. First-seen does
**not** bind them.

Host: one GLSL pair, `pass.x` = `ScenePasses.ShaderMode`
(0 BG / 1 FG / 2 sky / 3 static). **VALID BACKEND
TRANSLATION** of those four RGB contracts only.

### TOD / apply — why later native can be brighter

Lighting ctor `00B482A0` (dump
`lighting-mgr-ctor-defaults-00b482a0.md`):

- `[esi+224/225/226] = 0` (TOD indices / blend byte)
- `[esi+206] = 0` (extra scale off)
- `[esi+228] = 0xF` at `00B4849D` — **all dirty bits set**
- `[esi+48] = (0, 1, 0)`
- record 0 at `[esi+60]`: `+0 = (0.25)×3+1`,
  `+32 = (0,0,0,1)`, `+64 = (0,0,0,1)` fog,
  `+80=1000`, `+84=2000`
- `[esi+204]=1` gather **enabled**

`00B67480` (every landscape 4 / `0x40`) calls `00B46C80`
then `00B46890` (FOGENABLE=1). `00B46C80`:

```
test [esi+228], 1     ; ctor 0xF → taken
al = [esi+226]        ; 0 → copy record [esi+224]
copy record+0  → [esi+72]    ; colour → c20 path
copy record+32 → [esi+104]   ; → c35 path
if [esi+206]: scale colour by record+92
00B49950(dir+48, colour+72, ambient+104)
00989830(0, …)        ; VS const upload
… later callees include 0098B2C0 (c35 setter)
```

First-seen therefore **re-uploads record 0**. Host
`VulkanLineRenderer.Draw` pushes the same ctor vectors
every frame (`LightDir` / `LightColor` / `Pass.yzw=c35`).
**EQUIVALENT** to that first-seen flush.

If a later writer fills more 112-byte records and bumps
`+224/+225/+226`, native `00B46C80` lerps brighter
colours. Host never reads those records. That is a
**recovered later path**, not a missing `*k` on record 0.

`00F39D40` copies a `CShaderDirectionalLight` into a
record (`type=2`, dir from `this+4`, colour from
`this+28`) and writes `+32=1`, `+36=0`. It does **not**
write c35 (`FirstSeenLightApplyWritesC35=false`).
MARKER_LIGHT apply does not call add-light
(`FirstSeenPackedLightCount=0`).

`00B484C0` copies environment vectors (`edi+36/+48/+60`)
into a lighting object (`+12/+24/+36`). First-seen
**take UNREAD**. Do not invent sun colour from SKY_DEF
names (`_envprobe` string list is not a first-seen write).

### Map

`Dx9VulkanShaderConstants` (`DirLight*`, `LitColor`,
`PaletteStartRegister` lock only). No FFP-light type.

---

## 7. Vertex colour and fog

### Vertex colour

FG/BG/static/PALSKIN write `oD0.xyz` from the dirlight
add, not from `v#` colour. Landscape extra is `oT0`.
Static FVF has no diffuse. Host Color stream is a tint
that first-seen leaves at 1. **EQUIVALENT**.

### Fog

`00B46890` FOGENABLE=1 from `00B67480`. FOGCOLOR from
record `+64` packed `*255` → first-seen
`0xFF000000`. Table/vertex mode 0. VS still writes
`oFog`; D3D interpolator saturates.

```
oFog = c0.y − min(dp4(pos,c2), c0.y) * c18.w
     = 1 − min(world · LinearFogPlane, 1)     ; first-seen
rgb' = rgb * oFog + (1−oFog) * (0,0,0)
```

`c2` is `00B47630` linear view-Z (start 1000 / end 2000),
**not** inverse row 0 (`FirstSeenFogC2IsLinearViewZ`).
Inverse row at SHOT2 would force `oFog=0` (full black) —
**DISPROVEN** as first-seen.

Host VS is that clamp; FS `lit * fragFog`. **EQUIVALENT**.
If B’s camera pose/forward is wrong, the **plane numbers**
over-fog. That is a camera bug, not a material scale.

### Map

`Dx9VulkanShaderConstants.FogPlane/FogColor`,
`Dx9VulkanColor.FirstSeenClear` / `FromD3dArgb`.

---

## 8. Material constants and shader selection (host push)

`MeshPushConstants` 128 bytes (`WorldShadingPush.cs`):

| Field | Native stand-in | Type |
|---|---|---|
| `ViewProj` | `c5–c8` after clip-Y flip | `Dx9VulkanProjection` + `Dx9VulkanShaderConstants.PackWvp` |
| `CameraPos` | fog plane `c2` (misnamed) | `Dx9VulkanShaderConstants.FogPlane` |
| `LightDir` | `c19` | `Dx9VulkanShaderConstants.DirLightDirection` |
| `LightColor` | `c20` | `Dx9VulkanShaderConstants.DirLightColor` |
| `Pass.x` | host mode 0/1/2/3 | `ScenePasses.ShaderMode` |
| `Pass.yzw` | `c35.rgb` | `Dx9VulkanShaderConstants.LitColor` |

`c3` is **hardcoded** in GLSL, not pushed.
`UnlitRgbIsC3Leftover` documents that.

Sky mode 2: `t1 * v0 * v0.w` stand-in. Native
`PSHADER_INNER_SKY` is `lrp c0.w` / `lrp c1.w` /
`mul_x2_sat …, c2` with **no `def`**. `SkyPsConstantsUnread`.
Do not invent `*c2=0` (that would black the dome).

---

## 9. Why the scene looks extremely dark (causes, not knobs)

Ordered by proof. None is “multiply the framebuffer”.

| # | Cause | Status |
|---|---|---|
| 1 | First-seen dirlight is **0.25 × ndl²** and Lookout floors are **+Z** against `c19=+Y` → **ndl=0** | **PROVEN**. Native first-seen is the same. |
| 2 | Leftover **`c3=(0, 0.125, 0)`** then PS **`mul_x2`** → `(0, 0.25, 0)*tex` | **PROVEN**. `UnlitRgbIsC3Leftover`. |
| 3 | FG albedo UV **`(0,0)`** (unwritten `c40/c41`) | **PROVEN**. Host matches. |
| 4 | Live Concat often **omits sky `0x2000`**; clear is **black fog** | **PROVEN** vs native walk (G, H). Makes the *frame* darker without changing land/prop RGB. |
| 5 | Fog `* oFog` toward **black**. Wrong camera plane can crush | **EQUIVALENT** opcode; plane **PARTIAL** (B). |
| 6 | Host lights in **FS**; native in **VS** | **PARTIAL** interpolant only. |
| 7 | TOD / environment may later replace record 0 | **UNREAD** as first Present. Host has no path. |
| 8 | Point lights / gather | **UNREAD** take; first-seen count 0. |
| 9 | Sampler / sRGB | **UNREAD**. Not a 10× scale. |
| 10 | Failed texture → olive fallback, not black | **PROVEN** host. |

**DISPROVEN** “fixes”:

- Extra ambient / 0.28+0.72 N·L / guessed sun
  `(0.35,0.25,0.90)`
- World-XY `×0.125` albedo UV
- Second ×2 from Diffuse2X RTTI
- ExtraRgb as `oD0`
- Inverse-row `c2` (full black house)
- `LIT c35` (tokens are `MAD`)
- FFP `SetMaterial` diffuse
- Binding bump on first-seen static
- `* sky PS c2 = 0`

---

## 10. Classification index

| Claim | Status |
|---|---|
| First-seen 3D is vs_1_1 / ps_1_1, not FFP lighting | **PROVEN** |
| `oD0 = max(n·−c19,0)² * c20 + c35 + c3` | **PROVEN** |
| First-seen `c19=(0,1,0)`, `c20=0.25`, `c35=0` | **PROVEN** (ctor + record 0) |
| `c3` row 0 = `(0, 0.125, 0)` from `0x0139C614` | **PROVEN** (exe floats 2026-08-18) |
| `c3` is UV leftover, not ambient | **PROVEN** |
| Lookout +Z floors → leftover only → dark green after `mul_x2` | **PROVEN** |
| `oT1=(0,0)` first-seen | **PROVEN** |
| `UvScale=0.125` is albedo UV | **DISPROVEN** |
| ExtraRgb is `oD0` | **DISPROVEN** |
| PS already has `_x2`; missing extra ×2 | **DISPROVEN** |
| `TEXTURE_DIFFUSE_FOG` is a plain mul | **DISPROVEN** (tokens `mul_x2`) |
| Landscape alphablend off; PALSKIN 5/6 | **PROVEN** |
| Alpha test / sampler MAGMINMIP / sRGB / `BLENDOP` / fill / stencil | **UNREAD** |
| Fog enable + black + VS `oFog` | **PROVEN** / **EQUIVALENT** |
| Family slot 0 / packed lights 0 | **PROVEN** |
| `00B46C80` runs every land draw; first-seen copies record 0 | **PROVEN** |
| Later TOD records / `00B484C0` env copy on first Present | **UNREAD** |
| Sky PS `c0/c1/c2` writer | **UNREAD** |
| Self-illum / bump / illumination map first-seen | **PROVEN** omit |
| Specular enable without `oD1` | **PROVEN** unused |
| Host FS replica of dirlight | **EQUIVALENT** formula, **PARTIAL** interpolant |
| Live missing `0x2000` sky | **DISPROVEN** as native-equivalent submit |
| Invented brightness multiplier | **DISPROVEN** |

---

## 11. What not to do

- Do not add ambient, raise `c20`, or invent a sun direction
  to “match screenshots”.
- Do not treat `UvScale` / table `0x0139C5D8` as albedo UV.
- Do not apply a second ×2 because of `Diffuse2X` RTTI.
- Do not multiply albedo by ExtraRgb.
- Do not invent sky `*c2=0`.
- Do not change LINEAR/REPEAT / no-alpha-test / UNORM
  without a recovered `SetSamplerState` / `SetRenderState`.
- Do not edit `EngineLifecycle.cs` from this investigation.

---

## Evidence index

Parity types: `src/Fable.Render/Parity/Dx9Vulkan/*.cs`.

Constants: `WorldShading` (`DirLight*`, `FirstSeenC3`,
`EvaluateDirLightRgb`, `EvaluateTextureDiffuseRgb`,
`FirstSeenBindsC3dBump`, family slot helpers).

Tokens: `tools/Fable.ExeIndex/out/01-sections/shader-tokens/`
(`pshader-landscape-*`, `pshader-texture-diffuse*`,
`vshader-*-dirlight-fog`, `vshader-landscape-*`).

Tables: exe `0x0139C5D8` / `0x0139C614` (`floats` 2026-08-18).

TOD / ctor: `00B46C80`, `00B482A0`, `00F39D40`,
`00989A60` (count-1 `SetVSConstantF`).

Tests: `Dx9VulkanParityTests` (unlit `(0,0.125,0)` →
PS `(0,0.25,0)`), `ShaderFormatTests.First_seen_vs_read_c20_and_c35`,
`CameraProjectionTests` N·L locks, `LevFormatTests` +Z /
`UvA=(0,0)` / ExtraRgb.

Peer: G (Vulkan map), C (cells / leftover `c3`), B (camera /
fog plane numbers), E (PALSKIN blend granularity).
