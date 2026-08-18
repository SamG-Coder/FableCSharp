# 2026-08-18 — Native landscape cell draw (finish the renderer)

Investigation only. `EngineLifecycle.cs`, `SilkEngineHost.cs`, and
`VulkanLineRenderer.cs` were not edited.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Start: recovered `00B6B0B0`, `00BDC2D0`, `00BF4570`.
Prior: [C-terrain-static-map.md](C-terrain-static-map.md),
[A-dx9-submit.md](A-dx9-submit.md), [G-dx9-vulkan.md](G-dx9-vulkan.md).
Dumps: `tools/Fable.ExeIndex/out/01-sections/landscape-trace/`.
Live this session: `fn --exact` / `calls` / `disasm` / `scan 8D04C0`
on `00BF4570` (1337 insns), `00A0AD40`, `00BFE050`, `00BDC680`,
`00BF3700`, `00BF3A10`, `00BF3A90`, `00BF4220`, `00BF3860`,
`00AC1F70`, `00A63150`, `00B3B060`, `00BFDEC0`, `00BE5E70`.

`TessellatePrimary` is **not** the native path. Native is
**opened owner → tessellator AABB → 72-byte cell grid → linked
00BFE050 mesh objects → per-mesh DIP**.

Host `LandscapeDraw.cs` (already in tree) stores one decoded
`MeshVertex[]` per 16 m cell and comments that VB/IB live at
cell `+56`/`+52`. That comment is **DISPROVEN** (see §2).
`BothPasses` also **DISPROVEN**: bit `0x4` does not DIP the FG
cell meshes.

---

## Verdict (read this first)

There are **two native objects** the old notes fused into one
“72-byte cell”:

| Object | Size | Who | What draw uses |
|---|---|---|---|
| **Cell** (`CEngineLandscapePatch` ctor `00BF3A10`) | **72 bytes** | grid at owner `+8` | flags `+60` bit `0x4`, AABB `+32`/`+44`, origin `+56`/`+58`, mesh-list `+8` |
| **Mesh / tile** (loader `00BFE050`) | **>72** (writes `+72`) | linked from cell `+8` via mesh `+60` | VB `+56`, IB `+52`, NumVerts `+68`, PrimitiveCount `+70` |

Final DX9 call is **PROVEN**:

```
00BF55DB  movzx eax, [mesh+70]          ; PrimitiveCount
          movzx ecx, [mesh+68]          ; NumVerts
          push eax / push 0 / push ecx / push 0
          call 00A0AD40                 ; wrapper 0x1436E18
00A0AD40  PrimitiveType = [IB+12] = 5   ; D3DPT_TRIANGLESTRIP
          call IDirect3DDevice9.vtbl+328 DrawIndexedPrimitive
            (type=5, baseVertex=0, minIndex=0,
             numVerts=[+68], startIndex=0, primCount=[+70])
```

Vulkan must keep **one persistent VB+IB per 00BFE050 mesh**,
indexed `vkCmdDrawIndexed` of a **triangle strip**
(`indexCount = PrimitiveCount+2`). Do not soup the map through
`TessellatePrimary`.

---

## 1. Patch creation / ownership

**PROVEN**

```
SetStaticMapFileForUse 00B428E0
  OpenStaticMaps 00B42750(mode=1)
    STB hit  → 00B420F0 name table
               00B41E50 per [+32..+36) slot:
                 00BE03A0 attach name
                 00BDD0E0 build current (20-byte hdr → this+36,
                          tile stream 00BF9290 → this+64)
                 00BDF010 neighbour ingest
    STB miss → 00B42530(mode)
```

Current-map runtime object and each **16 m cell** share ctor
`00BF3A10` / vtbl `0x012A8200` (`CEngineLandscapePatch`).
Neighbour maps are `CLandscapeBackgroundPatch` vtbl `0x012A803C`
(ctor `00BE5E70` inits that vtbl; `00BE6090` is the dtor-shaped
cleanup that writes the same vtbl then `0x0129B860`).

Cell **array** is built on the **owner/grid** (the object
`00BDC2D0` uses as `this`), not on the tessellator:

```
00BDC680 / 00BDC70E
  n = [owner+16] * [owner+12]          ; rows * cols
  00BF36C0(4 + n*72)
  [alloc] = n
  004038C0(base+4, 72, n, ctor=00BF3A10)
  [owner+8] = base+4                   ; cell[0]
```

Per cell `(col,row)`:

```
00BF3700(cell, owner, col*16, row*16)
  cell+4  = owner*
  cell+56 = (u16) col*16               ; metres
  cell+58 = (u16) row*16
00BF3A90(cell, stream)                 ; 36-byte hdr → AABB + flags
00BF4220(cell, stream)                 ; near-sphere, then 00BF3B60 ingest
```

`00BF3B60` (only reached from `00BF43AB`) is the tile ingest that
calls `00BFE050` (`00BF3E17`). Meshes are singly linked:

```
if (prev) prev+60 = mesh; else cell+8 = mesh;
prev = mesh;
```

Owner layout used at submit (**PROVEN**):

| Off | Field |
|---|---|
| `+4` | tessellator* (`00BF6E20`, vtbl `0x012A8240`) |
| `+8` | `CEngineLandscapePatch[rows*cols]` |
| `+12` | cols |
| `+16` | rows |

Tessellator AABB (**PROVEN**, `00BF6F80`): `+168` min / `+180` max,
Z = 0; start `+216`/`+218`, size `+220`/`+222` (u16); origin from
map `+96`/`+98`. First-seen start is `(0,0)`. Default lod bytes
`+224/+225/+226 = 8,8,8` (Lookout 128/16 = 8).

Opened set = WLD `ContainsMap` ∪ `SeesMap` ∪ touching BWD rects,
seas skipped. **PROVEN** (C, `WorldGeometry.StaticMapsAround`).
`Activate Topology` `004FCBB0` sets current-only map-record `+38=1`.
Neighbours are terrain, not extra TNG.

List walked at draw: `[0x1436E8C]+44` doubly-linked;
`00B6B1A0` `ecx = [[node+8]]` → `00BDC2D0`.

**PARTIAL:** exact C++ type name of the owner that holds `+8/+12/+16`
(larger than the 72-byte cell; `00BDC180` also has `+32/+48/+52/+88/+92`).
Whether `[owner+12]/[+16]` is always `Grid/16` on every map (Lookout
8×8, Picnic 8×6 inferred).

---

## 2. 72-byte cell layout (every field)

Ctor `00BF3A10` + origin `00BF3700` + header `00BF3A90` + ingest
`00BF3E00` / `00BF4220`. Stride **PROVEN** (`push 72` at `00BDC72C`;
`lea eax,[eax+eax*8]; lea ecx,[ecx+eax*8]` at `00BDC39E`).

| Off | Type | Field | Writer | Reader | Status |
|---:|---|---|---|---|---|
| +0 | ptr | vtbl `0x012A8200` | `00BF3A14` | dtor `00BF3A40` | **PROVEN** |
| +4 | ptr | owner / grid * | `00BF3709` | `00BF441E` `00BDC150` | **PROVEN** |
| +8 | ptr | first `00BFE050` mesh * (0 if empty) | `00BF3E43` | `00BF3980` fixup; draw walks mesh `+60` | **PROVEN** |
| +12 | ptr | u16 texture-id table * | `00BF3E6C` `00BD9AF0` | `00BF3E80` copy from mesh `+4`/`+12` | **PROVEN** |
| +16 | u16 | texture-slot count | `inc [ebp+16]` `00BF3E2B` | `00BF3E51` | **PROVEN** |
| +18 | u16 | pad / unused | ctor zeros via `+16` dword | | **PARTIAL** |
| +20 | ptr | helper * (water/type owner) | `00BF40A9` `00BEB490` | `00BF44A0` `[ecx+20]` | **PROVEN** as pointer |
| +24 | u32 | 36-byte hdr dword 0 | `00BF3B13` | | **PARTIAL** semantic |
| +28 | u32 | 36-byte hdr dword 1 (payload size / present) | `00BF3B16` | `00BF4393` `test/jle` before ingest | **PARTIAL** (nonzero ⇒ try `00BF3B60`) |
| +32 | f32×3 | AABB min (X Y Z) | `00BF3B1D` 6 dwords | `00BF3860`, `00BF4220` sphere | **PROVEN** |
| +44 | f32×3 | AABB max (X Y Z) | same | `00BF3860` n-vertex | **PROVEN** |
| +56 | u16 | origin X metres (`col*16`) | `00BF3711` | `00BF4415` | **PROVEN** |
| +58 | u16 | origin Y metres (`row*16`) | `00BF3715` | `00BF4411` | **PROVEN** |
| +60 | u8 | flags (see below) | ctor / `00BF4220` / ingest | `00BF4579` bit `0x4` | **PROVEN** |
| +61 | u8×3 | rest of flags dword | ctor `and/or` on byte `+60` | | **PARTIAL** (high nibble kept) |
| +64 | ptr | resource / stream handle | ctor 0; `009D68A0` | dtor, `00BF4462` | **PARTIAL** |
| +68 | ptr | handle refcount block | ctor 0 | dtor `00BF3A4E` `dec [eax]` | **PROVEN** as refcount, **not** NumVerts |

**DISPROVEN** (C / `LandscapeDraw.cs` comments / A):
cell `+52` is AABB **max.Z**, not an IB.
cell `+56` is origin X, not a VB.
cell `+68` is a refcount pointer, not NumVerts.

### Flags at `+60` (byte)

| Bit | Set | Clear | Meaning | Status |
|---|---|---|---|---|
| `0x10` | ctor `or dl, 0x10`; stream `00BF3A90` may toggle | | must be set for `00BF4220` to run | **PROVEN** |
| `0x08` | `00BF437F` when camera-to-AABB sphere hits | `and al, 0xF2` far | “near / wanted”; `00BDC0C0` tests it | **PROVEN** |
| `0x04` | `00BF442E` after `00BDC150(origin, 16, 16)` ok | `00BF5692` `and …, 0xFB` | **required** for `00BF4570` DIP | **PROVEN** |
| `0x02` | `00BF40DC` after successful ingest | `and al, 0xF2` | tiles resident | **PROVEN** |
| `0x01` | `00BF4130` path | | extra-load latch | **PARTIAL** |

`00BDC0C0` walks `base + (row*cols+col)*72 + 60` after `>>4` on
world XY — that is the 16 m cell address. **PROVEN**.

---

## 3. `00BFE050` mesh object (the DIP subject)

Not 72 bytes: it writes a shared-IB flag at `+72`. Linked from
cell `+8`. Primary strip plus `CPatchTesselationEdgeStrip` extras
are **separate mesh nodes** (`mesh+60 = next`).

| Off | Type | Field | Status |
|---:|---|---|---|
| +0 | ptr | vtbl / dtor (`00BF3F4B` `[edx](1)`) | **PARTIAL** (vtbl id unread) |
| +4 | u32 | texture intern 0 (stream, name-remap) | **PROVEN** |
| +8 | u32 | texture intern 1 | **PROVEN** |
| +12 | u32 | texture intern 2 | **PROVEN** |
| +16 | f32 | scale used `fld [ebx+16]; fmul 0x1230A08` before lights | **PARTIAL** (formula unread) |
| +20 | ptr | resolved tex 0 (`id*44` table `+40`) | **PROVEN** |
| +24 | ptr | resolved tex 1 | **PROVEN** |
| +28 | ptr / u32 | resolved tex 2; water type when helper (`cmp eax,4/5/8`) | **PROVEN** as field; type-vs-ptr overlap **PARTIAL** |
| +32 | u32 | stream dword | **UNREAD** semantic |
| +36 | u32 | stream dword | **UNREAD** |
| +40 | u32 | texture-table index (stream u8) | **PROVEN** |
| +52 | ptr | IB object * | **PROVEN** |
| +56 | ptr | VB object * | **PROVEN** |
| +60 | ptr | next mesh * | **PROVEN** |
| +68 | u16 | NumVerts | **PROVEN** |
| +70 | u16 | PrimitiveCount | **PROVEN** |
| +72 | u8 | shared-IB: 1 → `[0x1436EA8]+1172`, else `00BDA360` | **PROVEN** |

IB object (`00AC1F70` / `00AC1EC0`, 20 bytes):

| Off | Field |
|---:|---|
| +0 | vtbl |
| +4 | `IDirect3DIndexBuffer9*` |
| +8 | Length bytes (`IndexCount*2`) |
| +12 | **PrimitiveType = 5** (`D3DPT_TRIANGLESTRIP`) |

VB object (`00BDA3D0` / `00A63150`, 52 bytes):

| Off | Field |
|---:|---|
| +4 | `IDirect3DVertexBuffer9*` |
| +8 | Length = NumVerts * 24 |
| +12 | FVF = **0** (shader verts, not FVF) |
| +16 | stride = **24** |

CreateIndexBuffer (`device vtbl+108`):
`Length = (PrimitiveCount+2)*2`, `Format = 101` (`D3DFMT_INDEX16`),
`Pool = 1` (`D3DPOOL_MANAGED`), `Usage = 00B3B060` → `8`
(`D3DUSAGE_WRITEONLY`) or `0x18` (`WRITEONLY|DYNAMIC`) if device
version ≥ `0x44C`.

CreateVertexBuffer (`device vtbl+104`):
`FVF = 0`, stride 24, same Usage helper `00B3B030`.

---

## 4. VB / IB / vertex / index / topology

**PROVEN**

GPU expand `00BFE050` (only `E8` from `00BF3E17`), per vert
15 file bytes → 24 GPU bytes, dest `edi` steps 24:

| GPU off | Type | Source |
|---:|---|---|
| +0 | u16 | file X (world metres as integer) |
| +2 | u16 | file Y |
| +4 | f32 | file Z metres |
| +8 | f32×3 | `00BFDEC0` unpack signed 11-11-10 at dest+8/+12/+16 |
| +20 | u8 | extra[2] (B) |
| +21 | u8 | extra[1] (G) |
| +22 | u8 | extra[0] (R), first-seen `0xFF` |
| +23 | — | **not written** |

`00BFDEC0`: low 11 → n.x, mid 11 → n.y, top 10 → n.z; sign via
bit 10 / bit 9; scale `0x129EF84` / `0x129EF80`. Writes float3 at
`dest+8`. **PROVEN**.

Index buffer: raw `rep movsd` of `(PrimitiveCount+2)` little-endian
u16s from the STB stream. **PROVEN** (`00BFE865`–`00BFE8C6`).

Topology: **D3DPT_TRIANGLESTRIP (5)** stored on IB `+12`, consumed
by `00A0AD40`. IndexCount = PrimitiveCount+2. Odd-t unwind
`(b,a,c)` is D3D strip winding (`LandscapeStrip.Unwind`).
**PROVEN**. No exe rewind on `n.Z < 0` (**DISPROVEN** as native).

Host `Dx9VulkanPrimitive.World = TriangleList` is **EQUIVALENT**
only after unwind; it is **DISPROVEN** as the native submit.

---

## 5. World transform, origin, MapX/MapY, height

**PROVEN**

Per-cell `00BF46A2` builds a 3×4 at `esp+144`: identity diagonal,
then camera `[0x1436EA0]+84/+88/+92` into the translation column;
`009881F0` copies it to wrapper world `+496`. Native GPU VB is
**camera-relative** (`FirstSeenLandscapeDeviceVbIsCameraRelative`).
Host STB after `StbFileToRegionLocal` is region-local world, so
host W = **I** is **EQUIVALENT** (`(p-cam)*T(cam)`). Applying
`T(cam)` to host world verts is **DISPROVEN**.

```
localXY = STB.WorldXY − (MapX, MapY)
neighbourOffset = (nb.MapX − primary.MapX, nb.MapY − primary.MapY)
```

No scale. Units metres. Cell origin `+56/+58` is
`(col*16, row*16)` **plus** map origin when the AABB is filled
(`00BF6F80` adds map `+96/+98`). Height Z is stored metres; no
`×0.01` (that factor is C3D cm only).

WVP: `00988A50` `world+496 * view+560 * proj+624` → `SetVSConstantF(c5, 4)`.
View is cot-scaled camera `+128`. Proj `009883F0`: `M11=M22=1`,
near 0.1 / far 4000 / minZ 0.1 / maxZ 0.99. **PROVEN** (B / C).

---

## 6. Winding, normals, UVs

| Fact | Status |
|---|---|
| Strip even `(a,b,c)`, odd `(b,a,c)` | **PROVEN** |
| `D3DCULL_CCW` (3) from `0x01396FB0` | **PROVEN** |
| Packed 11-11-10 → float3 in VB | **PROVEN** |
| Tile verts have **no UV** | **PROVEN** |
| FG `oT0.xy = v3.yz` = ExtraRgb.YZ (t0 **alpha**) | **PROVEN** |
| BG `oT0 = v3` = ExtraRgb.XY | **PROVEN** |
| FG albedo `oT1 = dp4(pos, c40/c41)` | **PROVEN** |
| First-seen `c40=c41=0` (no writer) → `oT1=(0,0)` | **PROVEN** |
| `UvScale=0.125` table is **c3** lighting, not albedo UV | **PROVEN** |
| World-XY `×0.125` as albedo UV | **DISPROVEN** |

---

## 7. Stage-0 texture, material, lighting

**DISPROVEN:** “SetTexture stage 0 from cell+1468”.
`00BF50E0` is mid-instruction in the old dump. Live:

```
00BF50C5  eax = [mesh+40]                 ; table index
          ecx = [0x1436EA8]               ; landscape renderer
          lea eax, [ecx + eax*8 + 1468]
          eax = [eax]                     ; IDirect3DBaseTexture9*
          SetTexture(stage=0, eax)        ; device vtbl+260
```

Stage 0 = `renderer+1468 + index*8`. **PROVEN**.

`00BF5491` `push 1; call [edx+260]` is **SetTexture(1, NULL)** when
wrapper `+15620` is nonempty (unbind). **PROVEN** as that site.
FG albedo t1 is **not** bound there.

FG VS/PS come from layer compact-bind `00B68DA0` (renderer
`+1508` list) + shared lighting `00B67480` (`00B46C80` lights,
`00B46890` `FOGENABLE=1`, identity-like 3×4). Bit `0x40` also
`0098B5E0(2)` Diffuse2X. **PROVEN** as the call chain.

PS FG: `mul_x2 t1*v0`, alpha `t0.a * v0.a`. PS BG:
`mul_x2 t0*v0`. **PROVEN**. Host contract (primary id on t1,
`TextureId1` on t0) stays the first-seen lock.

Material jump table `00BF4F18` `jmp [0xBF586C+(type-1)*4]` on
layer type (`ebp`); type 4 writes c1 flip and is water enqueue,
not first-seen FG. **PROVEN** not taken.

Lighting gather `00B46280` cap 5 from cell AABB (`add ebp, 32`
then push). First-seen take **PARTIAL**. FG VS:
`dp3 n, −c19`; `max`; square; `* c20`; `mad c35`; `add oD0, lit, c3`.
First-seen `c19=(0,1,0,0)`, `c20=(0.25)×3`, `c35=(0,0,0,1)`, leftover
`c3=(0,0.125,0)`. **PROVEN**.

After the cell: `00BF44A0` — if `[cell+20].+28 == 4/5/8` enqueue
water lists. First-seen water draw empty. **PROVEN** omit.

---

## 8. Layer `0x4` vs `0x40`

`00B6B0B0` (`CEngineLandscapeRenderer` vtbl+16) compares `arg+4`
to **4** and **64** only.

| Bit | Path | Geometry | Status |
|---|---|---|---|
| `0x4` BG | `00B67480` + `00B671A0` + walk `00BDC060` | `00BDC060` = `if ([this+4]) 00BF71D0(tessellator)` — **tessellator AABB + procedural BG mesh** (`00BE6880` VB at bg-patch `+192`, IB `+188`, prims at `+260/+262`). **Not** `00BF4570`. | **PROVEN** |
| `0x40` FG | `00B68DA0` + `00B67480` + `00B677D0` + `0098B5E0(2)` + walk `00BDC2D0` | patch AABB then **per 72-byte cell** `00BF4570` → mesh DIP | **PROVEN** |
| other | `00B67510` unbind stages 0/1/2 | no landscape DIP | **PROVEN** |

Registration order `00B26A75`: `0x4` then `0x40` then `0x20` then `0x2000`.

**DISPROVEN:** `LandscapeDraw.BothPasses` submitting the **same**
cell VB/IB on bit 4 and bit 0x40. Native BG is a different mesh
(skirt/quad on the background patch / tessellator). First-seen
host `SceneLayer.Landscape` collapsing both bits onto the FG
strip soup is a **DIVERGE**.

---

## 9. AABB / frustum / neighbour maps

Two-level, then meshes. **PROVEN**.

| Level | VA | Box | Reject |
|---|---|---|---|
| Patch / tessellator | `00BDC2D0` | `[esi+168]` n>0 / `[esi+180]` n≤0; Z=0 | `n·p > d`; missing `[this+4]` submits every cell |
| Cell | `00BF3860` inside `00BF4570` | cell `+32` min / `+44` max (3D) | same 4-plane n-vertex; `al=0` skips the cell |
| Near sphere | `00BF4220` / optional `00BF4784` | camera vs cell AABB | sets/clears bit `0x08`; first-seen take **PARTIAL** |
| BG (`0x4`) | `00BF71D0` | tessellator `+168/+180` | same 4 planes, then BG draw, not FG DIP |

Planes: camera `[0x1436EA0]+448` (`0x1C8` from the add), stride 16,
four side planes. `0x122DEDC = 0`.

Neighbours participate as opened patches on the same
`[0x1436E8C]+44` list. **PROVEN**.

Host `TessellateVisible` AABBs each opened map then **dumps every
tile**. Native can reject per cell (`00BF3860`) and per mesh
(empty `+8`). **DIVERGE**.

---

## 10. DX9 render states (first-seen landscape)

| State | Value | Status |
|---|---|---|
| VS FG | `VSHADER_LANDSCAPE_FOREGROUND` via `00B68DA0` | **PROVEN** as bind path |
| PS FG | `PSHADER_LANDSCAPE_FOREGROUND` (`mul_x2 t1*v0`) | **PROVEN** |
| PS BG | `PSHADER_LANDSCAPE_BACKGROUND` (`mul_x2 t0*v0`) | **PROVEN** |
| Cull | `D3DCULL_CCW` (3) | **PROVEN** |
| Fog | enable 1, color `0xFF000000`, table/vertex mode 0, VS `oFog` | **PROVEN** |
| Alphablend | off | **PROVEN** |
| Z func | LESSEQUAL | **PROVEN** |
| ZENABLE / ZWRITE | D3D defaults TRUE; first-seen write | **PARTIAL** |
| Diffuse2X | `0098B5E0(2)` on bit `0x40` | **PROVEN** call; body **UNREAD** |
| Sampler MAG/MIN/MIP/ADDRESS | | **UNREAD** (host LINEAR/REPEAT is TEMPORARY) |
| Alpha test / stencil / color write / fill | | **UNREAD** |
| FVF | 0 (declaration / VS) | **PROVEN** |
| Vertex declaration bytes | | **UNREAD** (layout is the 24-byte expand) |

Dirty flush before DIP (`00BF5515` wrapper `+984` bits) calls
`00988A50` (WVP), `009897C0` (fog color), `0098A540` (lights),
`00989700`, etc. **PROVEN** as the bit tests; each helper body is
in B / WorldShading.

---

## 11. Final DIP

**PROVEN** — this was the C / A leftover.

```
IDirect3DDevice9::DrawIndexedPrimitive          ; vtbl+328, NOT +332
  PrimitiveType     = 5                         ; D3DPT_TRIANGLESTRIP (IB+12)
  BaseVertexIndex   = 0
  MinVertexIndex    = 0
  NumVertices       = (u16) mesh+68
  startIndex        = 0
  primCount         = (u16) mesh+70

SetStreamSource(0, VB+4, offset=0, stride=VB+16=24)   ; vtbl+400
SetIndices(IB+4)                                      ; vtbl+416
```

`009DA9F0` 2D path uses **vtbl+332** (`DrawPrimitiveUP`). That is
**not** landscape. A labeled landscape DIP as +332; **DISPROVEN**.

Wrapper `00A0AD40` `ret 16`. After DIP, `mesh = mesh+60` and the
loop at `00BF4E90` repeats bind+DIP for extras. Then unbind
streams/textures, `00BF44A0` water enqueue, restore state blocks.

`00A0ACA0` (BG `00BE7919`) is a **different** wrapper (extra
args, used by `00BE6880` procedural BG). Do not mix with FG.

---

## 12. Exact Vulkan representation of ONE native cell

One **16 m cell** is not one triangle soup. It is:

```
NativeLandscapeCell                // 72-byte CEngineLandscapePatch
  flags            // need bit 0x4 to emit FG
  aabbMin/aabbMax  // cell+32 / +44, 3D, metres, region-local after MapX/Y
  originX/originY  // cell+56/+58 = (col*16, row*16) + map origin
  meshes[]         // cell+8 linked list, one NativeLandscapeMesh each
```

Each **mesh** (primary or extra) is one persistent GPU draw:

```
NativeLandscapeMesh
  vertexCount      // mesh+68
  primitiveCount   // mesh+70
  indexCount       // primitiveCount + 2
  topology         // VK_PRIMITIVE_TOPOLOGY_TRIANGLE_STRIP
  indexType        // VK_INDEX_TYPE_UINT16
  vb               // VkBuffer, stride 24, VERTEX | TRANSFER_DST
  ib               // VkBuffer, INDEX  | TRANSFER_DST
  stage0Index      // mesh+40 → renderer table +1468
  // optional decoded bind:
  maskId           // t0 (FG extra / TextureId1)
  albedoId         // t1 (FG primary TextureId)
```

### Vertex attribute (native 24-byte, preferred persistent layout)

| Location | Format | Offset | VS use |
|---|---|---:|---|
| 0 | `R16G16Uint` | 0 | pos.xy (convert to float in VS, or pre-expand) |
| 1 | `R32Sfloat` | 4 | pos.z |
| 2 | `R32G32B32Sfloat` | 8 | normal |
| 3 | `B8G8R8A8Unorm` | 20 | extra / v3 (A unused; byte3 not written) |

Host may upload already-decoded `float3 pos + float3 n + float3 extra`
if the VS matches `LandscapeTextures.Ot0FromExtra` / `ProjectOt1`.
That is **EQUIVALENT** decoded, **DISPROVEN** as native stride.

Do **not** subtract camera on host STB verts.

### Draw (FG bit `0x40`, once per mesh)

```
vkCmdBindVertexBuffers(0, vb, 0)
vkCmdBindIndexBuffer(ib, 0, UINT16)
vkCmdBindPipeline(graphics, landscapeFg)   // strip, CCW, cull back, no blend
vkCmdBindDescriptorSets(set0=mask t0, set1=albedo t1)
vkCmdPushConstants(WVP = ComposeWvp(I, V, P) * clipYFlip)
vkCmdDrawIndexed(
    indexCount   = primitiveCount + 2,
    instanceCount= 1,
    firstIndex   = 0,
    vertexOffset = 0,
    firstInstance= 0)
```

Same mesh may be skipped if cell AABB fails `00BF3860` or flags
lack `0x4`.

### Draw (BG bit `0x4`) — **not** this cell mesh

Use the tessellator / `CLandscapeBackgroundPatch` procedural
buffers (`+192` VB / `+188` IB, `+260` verts / `+262` prims,
`00A0ACA0`). First-seen host may omit BG if that mesh is unread
as a stored STB strip. Do **not** re-DIP the FG cell on bit 4.

### Pipeline state

| Native | Vulkan |
|---|---|
| `D3DPT_TRIANGLESTRIP` | `TRIANGLE_STRIP` (or LIST + `Unwind` if sharing the mesh pipeline) |
| `D3DCULL_CCW` | `frontFace=CCW`, `cullMode=BACK` after clip-Y flip |
| alphablend off | opaque pipeline |
| `ZFUNC=LESSEQUAL` | `compareOp=LESS_OR_EQUAL`, depth test/write on |
| fog `oFog` + black | existing `WorldShading` fog push |
| `FOGENABLE=1` | keep |
| Diffuse2X / `mul_x2` | `* 2` in FG/BG FS (already in PS contract) |

### What **not** to do

- `TessellatePrimary` / one `List<MeshTriangle>` as the runtime object.
- One giant `SetMesh` Concat with C3D / PALSKIN.
- `vkCmdDraw` non-indexed unless you unwind and document it as
  translation, not native.
- `T(cam)` on region-local STB verts.
- Bind albedo on t0 for FG.
- Treat cell `+52/+56/+68` as IB/VB/NumVerts.

### Suggested host records (for the main agent; not added here)

`LandscapeDraw.cs` can stay as a **decoded** helper if the comments
are fixed: VB/IB live on the **mesh**, bit `0x4` is not this DIP.
A persistent cache key is `(mapName, cellX, cellY, meshIndex)` —
not `(map, cellX, cellY)` alone, because extras are extra DIPs.

---

## Classification summary

| Item | Status |
|---|---|
| Patch open / current vs neighbour vtbls | **PROVEN** |
| Cell array alloc `n*72`, ctor `00BF3A10` | **PROVEN** |
| 72-byte field table above | **PROVEN** except +18/+24/+28 semantic, +61..+63, +64 |
| Mesh object VB/IB/counts/next | **PROVEN** |
| Vertex 24-byte expand + INDEX16 + strip 5 | **PROVEN** |
| DIP `00A0AD40` → vtbl+328 args | **PROVEN** |
| World `T(cam)` / host I equivalent | **PROVEN** / **EQUIVALENT** |
| Height metres, MapX/Y subtract | **PROVEN** |
| Winding / CCW / no n.Z rewind | **PROVEN** |
| UVs (oT0 extra, oT1 c40=0) | **PROVEN** |
| Stage 0 = renderer+1468+index*8 | **PROVEN** |
| Stage 1 per-cell site is unbind 0 | **PROVEN**; FG t1 bind **PARTIAL** (`00B68DA0`) |
| Layer 0x4 ≠ cell DIP; 0x40 = cell DIP | **PROVEN** |
| Tessellator + cell AABB | **PROVEN** |
| DX9 fog / cull / blend-off / Diffuse2X call | **PROVEN** |
| Sampler / Z write / stencil / decl blob | **UNREAD** |
| `00BE6880` BG mesh as first-seen payload | **PARTIAL** (fn decoder desync at `+262`) |
| `00B46280` first-seen light take | **PARTIAL** |
| Owner C++ type name / always Grid/16 | **PARTIAL** |
| Vertex declaration COM object | **UNREAD** |
| Cell+52 IB / +56 VB / +68 NumVerts | **DISPROVEN** |
| SetTexture stage0 from cell+1468 | **DISPROVEN** |
| Landscape DIP is vtbl+332 | **DISPROVEN** |
| Same VB/IB on bits 4 and 0x40 | **DISPROVEN** |
| `TessellatePrimary` as native submit | **DISPROVEN** |

---

## Unread leftovers (do not invent)

- `00BFE050` mesh `+0` vtbl id and `+32/+36` stream dwords.
- Exact first-seen bind of t1 albedo inside `00B68DA0` (compact list
  at renderer `+1508`).
- `CEngineStateBlockDiffuse2X` apply body.
- BG procedural index/vert contents after the `00BE6880` desync.
- Sampler / alpha-test / stencil first-seen writes.
- Whether extras share the cell AABB or have their own (draw walks
  them after the cell test already passed).

---

## Evidence index

Exe: `fn 0x00BF4570 --exact` (DIP at `00BF55EF`), `fn 0x00A0AD40`,
`fn 0x00BFE050`, `fn 0x00BDC680` (alloc + `push 72` + `00BF3A10`),
`fn 0x00BF3700`, `fn 0x00BF3A10`, `fn 0x00BF3A90`, `fn 0x00BF4220`,
`fn 0x00BF3860`, `fn 0x00AC1F70`, `fn 0x00A63150`, `fn 0x00B3B060`,
`fn 0x00BFDEC0`, `fn 0x00B6B0B0`, `fn 0x00BDC2D0`, `fn 0x00BDC060`,
`scan 8D04C0` at `00BDC39E` / `00BDC6A2` / `00BDC851` / `00BDC911` /
`00BDC0F2`.

Host (not edited): `LandscapeDraw.cs` (fix comments before wiring),
`LandscapeFrustum.cs`, `LandscapeTextures.cs`, `LandscapeStrip.cs`,
`LevTileMesh.cs`, `WorldGeometry.cs`, `Dx9VulkanPrimitive.cs`.
