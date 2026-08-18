# D — Thing → C3D → world matrix → DX9 draw

Investigation only. No production edits. Agent D owns static C3D +
instance world matrix. PALSKIN dest / `00BD2F91` is agent E.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

## Verdict

**CPU flattening of every triangle is DISPROVEN as native.**

Native keeps C3D vertices in file-local space (centimetres, packed or
float3). Draw-time world lives on the device wrapper at
`[0x1436E14]+496`, written by `009881F0` from a per-instance 3×4, then
`00988A50` multiplies `W * V * P` into wrapper+752 and uploads
`SetVertexShaderConstantF` registers **c5–c8**. First-seen VS
(`VSHADER_STATIC_DIRLIGHT_FOG`, `VSHADER_PALSKIN_DIRLIGHT_FOG`) do
`dp4 oPos, pos, c5–c8`. There is no first-seen
`IDirect3DDevice9::SetTransform(D3DTS_WORLD)` path.

The native world-matrix owner at draw is **that wrapper+496 slot**,
sourced from the **instance 3×4** (PALSKIN draw reads
`[inst+0xEC]`; static-lit `00BB2540` pushes a 3×4 then
`009881F0`). Camera bind `00B2FC50` writes **identity** as the
default; landscape `00BF46A2` overwrites with `T(cam)`. Mesh draws
are supposed to overwrite with the instance matrix — they do not
bake `ObjectTransform` into the C3D blob.

Host `MeshBatches.BuildMeshes` / `WorldGeometry.AddInstances` apply
`Vector3.Transform` on every triangle, then the renderer consumes
`IdentityWorld()`. That can be numerically equivalent if
`ObjectTransform` matches the native 3×4, but it is **not** the
native path and is a plausible source of mangled props if the
baked matrix disagrees with the wrapper layout.

---

## 1. Recovered chain

```
TNG ThingInstance
  CTCPhysicsStandard.PositionX/Y/Z          PROVEN
  CTCPhysicsStandard.RHSetForward/Up        PROVEN
  ObjectScale (thing+156, default 1.0)      PROVEN (persist)
        │
        ▼
004CA010 construct / 009AD410 def → mesh HANDLE
  hash walk [bank+104..+108); miss 009E5170
  does NOT parse C3D                        PROVEN
        │
        ▼
game.bin Graphic.bank_index
  + CMultiStaticMeshDef (6911 then 6909)
  + CReplaceableMeshDef
  FirstSeenInstancesAsC3d (no MARKER / HOLY_SITE / CAMERA / TRACK / PARTICLE)
                                            PROVEN
        │
        ▼
graphics.big C3D (MBANK_ALLMESHES)
  00A89450 serialize / 00A8FD40 primitive
  VB = file vertex blob (Lock 00AC20A0 + 00996610)
  no world multiply at open                 PROVEN
        │
        ▼
local transform   C3D cm, origin = mesh origin
parent transform  TNG things are independently placed;
                  neighbour offset = ΔMapX/ΔMapY
                  attachment chain UNREAD
world transform   instance 3×4 → 009881F0 → wrapper+496
                                            PROVEN (owner)
                                            PARTIAL (3×4 build site)
        │
        ▼
render record     CEnginePrimitiveManagerStaticMeshes
                  / PALSKIN helper 00BCE740 +12 = instance
                  queue 00B84720 / drain 00B849F0 / layer 0x20
                                            PARTIAL
        │
        ▼
DX9 draw          00BB2540 static-lit FVF 0x112 (first-seen house)
                  00BD3070 PALSKIN default
                  DrawPrimitive vtbl+400 / DrawIndexed vtbl+332
                  WVP = 00988A50 → c5–c8
                                            PROVEN
```

---

## 2. Host path (audit, not edited)

### 2.1 Thing → definition → mesh id — **PROVEN**

`ThingFile` reads `DefinitionType` and
`CTCPhysicsStandard.Position*`. `WorldGeometry.ResolveSubmit` uses
`GameBin.FindMeshIds` (Graphic + every `CMultiStaticMeshDef` bank +
`CReplaceableMeshDef`) gated by `FirstSeenInstancesAsC3d`.

`009AD410` is a name→handle lookup. `MeshBank.Get` is the host
equivalent: directory open at `0049E620`, parse on first draw.

`PresentWorld` (`EngineLifecycle.cs`) builds instances with
`expandGeometry: false` — handles + `WorldMeshInstance.Transform`
only. Neighbour maps stay `00B3EFA0` headers.

### 2.2 Submit — **PROVEN** (what the host actually does)

`SubmitCurrentWorld`:

1. `TessellateVisible` landscape (AABB `00BDC2D0`, not C3D).
2. Every `opened.Instances` whose `Map` equals the primary region.
   Not unique-by-mesh-id: `seen` only blocks a second hero
   Graphic. Multiple TNG things sharing one C3D each get a
   `(MeshFile, inst.Transform)` pair.
3. Optional hero Graphic `006AC910` via `ObjectTransform(Hero)`.
4. `MeshBatches.BuildMeshes(props)` then
   `MeshBatches.Concat(land, props)`.
5. `BuildFrame` ships the baked `SubmittedMesh` vertices. Vulkan
   `DrawMeshBatches` binds `_worldViewProj` =
   `IdentityWorld() * V * P` for pass `0x20`.

So: **one parse per mesh id** (bank cache), **one flatten per
instance**, primary map only. The task phrase “unique primary C3Ds”
is the parse cache + primary filter, not a one-draw-per-mesh-id
collapse.

### 2.3 CPU flatten sites — **PROVEN** (host), **DISPROVEN** (native)

`MeshBatches.BuildMeshes`:

```csharp
var source = mesh.BoneCount > 0 ? mesh.TrianglesForPose() : mesh.Triangles;
foreach (var tri in source) {
    A/B/C = Vector3.Transform(tri.*, transform);
    Normal* = Vector3.TransformNormal(...);
}
```

Same multiply exists in `WorldGeometry.AddInstances` and
`WorldGeometry.Expand` when `expandGeometry` is true
(`FirstSceneWorld.Build` still uses that soup for traces).

`transform` is `ObjectTransform(thing) * neighbourShift`.

### 2.4 `ObjectTransform` — **PROVEN** as the host C3D→region-local rule

```
scale    = 0.01 * ObjectScale          // MeshToWorld
forward  = RHSetForward (default +Y)
up       = RHSetUp      (default +Z)
right    = normalize(forward × up)
up       = normalize(right × forward)
basis    = rows (right, forward, up), translation = TNG pos
return   CreateScale(scale) * basis    // Numerics p * M
```

`CreateWorld(pos, RHSetForward, RHSetUp)` is **DISPROVEN**
(`WorldGeometryTests.Streetlamp_stands_on_world_z_not_createworld_y`).
Numerics `CreateWorld` is Y-up and negates forward; lamp 4978 is
345 units tall in **Z**.

Mesh origin (`Vector3.Zero`) lands on TNG position
(`WorldPipelineTests.Static_tng_object_transform_places_local_origin_at_thing`).

---

## 3. Native matrix convention

### 3.1 Storage — **PROVEN**

`009881F0` copies a **3×4, column stride 12** (12 floats, 48 bytes)
into wrapper+496 as a 4×4:

| dest (wrapper+496) | source |
|---|---|
| +496..+508 row0 | src+0, +12, +24, +36 |
| +512..+524 row1 | src+4, +16, +28, +40 |
| +528..+540 row2 | src+8, +20, +32, +44 |
| +544..+552 = 0, +556 = 1.0 | last row |

So source columns are `(axisX, axisY, axisZ, translation)`. Dest
has **translation in the last column** (`M14/M24/M34`). That is
the same layout `WorldShading.ComposeLocalBone` documents for C3D
64-byte bone matrices (`M14/M24/M34`, not Numerics `M41`).

`00988290` writes a sequential 4×4 identity at the same slot and
sets `[wrapper+488]=1` (“world is identity”). `009881F0` clears
`+488`.

This is **DX9 `D3DMATRIX` column-of-basis / last-column
translation**, not a host `Matrix4x4` last-row translation. VS
`dp4 oPos, pos, c5` (c5 = first stored row) implements
`p.x*axisX + p.y*axisY + p.z*axisZ + translation`.

Host `ObjectTransform` stores the same axes as **rows** and
translation in `M41..M43`, then `Vector3.Transform` does `p * M`.
That product is **EQUIVALENT** to the native `dp4` if the axes
match. Uploading a Numerics matrix through `009881F0` without the
3×4 column pack would swap the convention.

Row-major bytes of the **wrapper 4×4** are what `00988A50` multiplies.
System.Numerics “row-major uploaded as-is, no extra transpose” is
locked for the **c5–c8 product**, not for the 3×4 source.

### 3.2 Composition order — **PROVEN**

`00988A50`: `world+496 * view+560 * proj+624` → `+752`, then
`SetVertexShaderConstantF` register `[layout+120]=5`, count 4.

`FirstSeenWvpIsWorldViewProj = true`. First-seen VS do **not**
read separate W/V/P banks (`FirstSeenVsReadsSeparateWvp = false`).

Semantic: `clip = p_local * W_instance * V * P` (row-vector
Numerics / GPU after the documented upload-transpose of P).

`00B2FC50` (camera bind, `push 1`): view from camera+128
(`00988350`), proj from camera+372 (`00988540`), then an identity
3×4 via `009881F0`. That identity is the **bind default**, not a
proof that mesh draws keep W = I.

Landscape `00BF46A2` rebuilds W as identity 3×4 + camera
+84/+88/+92 (`T(cam)`) because the landscape VB is camera-relative.

### 3.3 `SetTransform(D3DTS_WORLD)` — **DISPROVEN** for first-seen C3D

No import/string/site for `SetTransform` on this path. World is
software state on the wrapper, consumed only as the left factor of
the VS WVP product.

---

## 4. C3D vertex basis

### 4.1 Units — **PROVEN** (file), **PARTIAL** (where ×0.01 is applied)

C3D positions are **centimetres**, right-handed, **X right, Y
forward, Z up**, origin = mesh origin.

Evidence: streetlamp 4978 bounds taller in Z (~345) than X/Y;
`ObjectScale` 0.4 shrinks the instance; TNG metres match terrain
Z to ~10 cm. `WorldSpaces.C3dCentimetresToMetres = 0.01`.
`FIRST_SCENE_CONTRACT` / `WORLD_SPACE_CONTRACT` lock this.

`00A8FD40` writes the file vertex blob into the GPU VB unchanged
(plus packed 11-11-10 unpack at consume time). Primitive
scale/offset (8 floats at prim+48) is the **packed-position
decompress**, not the world centimetre scale.

The **0.01 world scale site in the exe is UNREAD** (no
`0x3C23D70A` hit in this pass). It must live in the instance 3×4
and/or a mesh root 3×4 (48 bytes serialized at mesh+176 after
bones — **UNREAD** as a draw multiply). Host folds it into
`ObjectTransform`.

### 4.2 Packed vs float — **PROVEN**

`initFlags` bit 2 (`4`) + not bit 4 → packed pos. House static is
typically unpacked float3. Father PALSKIN stride 20 / flags 4
packed. Kid 4300 stride 28 / flags `0x14` float3 + 8 bone bytes.

### 4.3 Pivot / origin — **PROVEN**

No extra pivot in TNG. `p_local = 0` → TNG
`CTCPhysicsStandard` position. Door/table/house tests land on
those positions.

### 4.4 Scale — **PROVEN** (host rule), **PARTIAL** (native pack)

- File: packed scale/offset per primitive.
- Thing: `ObjectScale` persist `006A5D90` writes `this+156`,
  default `1.0f`.
- World: host `0.01 * ObjectScale` on the 3×3. Native multiply
  site UNREAD.

### 4.5 Handedness — **PROVEN**

Right-handed Z-up. `right = forward × up`, then
`up = right × forward`. TNG `RHSetForward*` persist at physics
`+80/+84/+88`, `RHSetUp*` at `+92/+96/+100` (`00724290`).

### 4.6 Root / bone matrices — **PROVEN** (layout), **UNREAD** (static root apply)

Bones: 60-byte info (`00A4BD70`), 48-byte local TRS (mesh+160),
64-byte row-major 4×4 IBM at mesh+224 (`shl eax,6`). Last row of
file IBM is `(0,0,0,1)` on identity bones. PALSKIN dest =
hierarchy × IBM (`00A9E1E0` / `00BD2F91`); first-seen dest ≈ I
(agent E). Static C3D does not skin.

48-byte root after bones is serialized (`push 48` at
`00A89564`) and **not** applied by `MeshFile.Parse`. Whether
native folds it into the instance 3×4 is **UNREAD**.

---

## 5. Native draw vs host flatten

### 5.1 Static first-seen (`00BB2540` / caller `00BB30A0`) — **PROVEN**

1. Build a dynamic VB, FVF **`0x112`** (`XYZ|NORMAL|TEX1`),
   stride **32** (`00A63150`).
2. Lock (`00AC20A0`). Copy source positions
   `shl index, 4` (16-byte source) → dest+0/+4/+8 **with no
   matrix multiply**. Optional normal sign flip when a flag
   is 1. UV from a parallel stream.
3. Unlock (`00AC1F00`).
4. `009881F0([0x1436E14], 3×4 at [esp+104])`.
5. `00B44F20` point-light pack (uses that world).
6. State flush. If `flags & 1 == 0` **and** `wrapper+488 == 0`,
   `00988290` **rewrites identity**. Then `00988A50` if dirty.
7. `DrawPrimitive` device vtbl+400.

So native static draw **does not transform triangles on the CPU**.
It copies local (or already-unpacked local) verts and expects W
to carry the instance. The `00988290` restore is a flag-gated
fallback; after `009881F0`, `+488` is 0, so **bit 0 of the
draw flags must be set to keep instance W**. First-seen value of
that bit is **UNREAD**.

If first-seen cleared bit 0, W would snap back to identity and
local-cm house verts would pile at the origin — that is not what
SHOT2 looks like. Treat “keep instance W” as the working
assumption (**PARTIAL**), not a locked immediate.

### 5.2 PALSKIN (`00BD3070`) — **PROVEN** (W owner), dest is agent E

Default bind copies `[record+12]+0xEC` through `009881F0` (or
`00A0B140` when wrapper+917 is set), then `DrawIndexed`. Later
tails also `00988290` + `00988A50`. File verts stay local; GPU
skins with dest at c38; WVP still c5–c8.

Host `TrianglesForPose` CPU-skins then `BuildMeshes` CPU-bakes W.
That is a **double host flatten**. Native is GPU dest × instance W.

### 5.3 Primitive pass `00B89C30` — **PROVEN** (identity default)

Several sites: if `wrapper+488 == 0` then `00988290`, then
`00988A50`. That is the “no one wrote a world this batch” path,
consistent with bind-default identity — not a CPU triangle soup.

### 5.4 Mesh renderer ctor sites — **PROVEN** (name only)

`00AF7DD0` / `00AF81E4` / `00B05A90` intern `"Mesh Renderer"` /
`"CPSCRenderMesh"`. They do not draw.

`00B555A0` (mesh-path c4 / inverse-row upload) has **zero
first-seen callers**. Not the New Game house path.

---

## 6. Parent / neighbour / attachment

| Step | Native | Host | Status |
|---|---|---|---|
| TNG thing world | `CTCPhysicsStandard` pos + RH axes | `ObjectTransform` | PROVEN |
| Neighbour map | ΔMapX/ΔMapY on the primary local frame | `shift = T(dx,dy,0)` after object | PROVEN |
| Child attached to parent | `CEnginePrimitiveAttachmentManager` exists | not composed | UNREAD |
| Multi-static 6911+6909 | same thing transform, two mesh ids | `FindMeshIds` both, same `ObjectTransform` | PROVEN |

First-seen house props (bed, table, lamp, door, fence) are
separate TNG objects, not attachments.

---

## 7. Where mangling can come from

Host bake + identity W is **EQUIVALENT** only if `ObjectTransform`
equals the native instance 3×4 under `p * W`. Divergence sites:

1. **Wrong owner (this investigation).** Baking instead of
   per-draw wrapper W. Any later native W (fade, attach, scale
   anim) cannot be expressed. Mixing landscape identity,
   sky identity, and baked props in one VB is a host invention.
2. **Convention mix.** Feeding a Numerics last-row matrix into a
   last-column 3×4 slot (or the reverse) swaps axes — the
   `CreateWorld` failure mode.
3. **0.01 applied twice or never.** File verts stay cm.
   Host always multiplies 0.01. Native site UNREAD.
4. **Primary-only submit.** Neighbour C3Ds stay handles. Not
   a basis bug, but missing geometry.
5. **PALSKIN CPU skin + CPU W** vs GPU dest × instance W.
   First-seen dest ≈ I so this should match if W matches.
6. **Root 3×4 unread.** If native multiplies mesh+176 and host
   ignores it, origins shift.

`WORLD_SPACE_CONTRACT` “static-object world = identity W
(`009881F0`)” is the **bind default / host equivalent after
bake**, not proof that native mesh draws keep W = I. PARITY’s
“house props use instance world” is the correct draw-time rule.

---

## 8. Classification table

| Claim | Status | Evidence |
|---|---|---|
| `009AD410` = def → handle, no C3D parse | PROVEN | disasm; `FORWARD_TREE`; `MeshBank` |
| C3D VB = file-local verts | PROVEN | `00A8FD40` Lock + raw copy |
| C3D space = RH Z-up centimetres | PROVEN | lamp 4978; contract |
| ×0.01 into TNG metres | PROVEN (need); UNREAD (exe site) | bounds vs TNG; no `0x3C23D70A` |
| Packed scale/offset = decompress only | PROVEN | `MeshFile.UnpackPosition` |
| Pivot = mesh origin at TNG pos | PROVEN | `WorldPipelineTests` |
| `ObjectScale` at thing+156, default 1 | PROVEN | `006A5D90` |
| RHSet at physics +80 / +92 | PROVEN | `00724290` |
| `CreateWorld` as object basis | DISPROVEN | streetlamp test |
| Wrapper W = 3×4 columns → last-column 4×4 | PROVEN | `009881F0` |
| WVP = W×V×P → c5–c8 | PROVEN | `00988A50` |
| First-seen VS reads only c5–c8 | PROVEN | shader tokens |
| `SetTransform(D3DTS_WORLD)` first-seen C3D | DISPROVEN | no site; wrapper + VS |
| Camera bind writes identity W | PROVEN | `00B2FC50` |
| Landscape overwrites W with `T(cam)` | PROVEN | `00BF46A2` |
| Mesh draw owner = instance 3×4 via `009881F0` | PROVEN | `00BB2540`, `00BD3070` +0xEC |
| First-seen static flag keeps instance W | PARTIAL | bit 0 UNREAD; identity would break SHOT2 |
| Static-lit copies verts without matrix | PROVEN | `00BB2540` `fld [idx*16]` |
| CPU flatten every triangle is native | **DISPROVEN** | above |
| Host `BuildMeshes` flattens then identity W | PROVEN | `MeshBatches`, `FlyCamera` |
| Host flatten ≡ native if W matches | EQUIVALENT / PARTIAL | same `p * axes + pos` |
| Mesh root 48-byte applied at draw | UNREAD | serialized, not in `MeshFile` tris |
| Attachment / parent 3×4 chain | UNREAD | RTTI only |
| `00B555A0` first-seen house draw | DISPROVEN | 0 callers |
| Mesh-renderer ctor draws | DISPROVEN | name intern only |
| Submit is unique-by-mesh-id | DISPROVEN | every primary instance |
| Submit is primary-map C3Ds only | PROVEN | `SubmitCurrentWorld` map filter |

---

## 9. Direct answers

**Is CPU flattening of every triangle DISPROVEN as native?**
Yes. Native uploads / copies C3D-local vertices and owns world as
GPU VS state (`009881F0` → wrapper+496 → `00988A50` → c5–c8).

**What is the native world-matrix owner at draw time?**
`CDevice` / wrapper `[0x1436E14]+496`, written from the
**instance 3×4** by `009881F0`. PALSKIN’s recorded source is
`instance+0xEC`. Static-lit’s source is the 3×4 pushed at
`00BB2D80`. Not `D3DTS_WORLD`. Not a CPU-baked VB.

Host should keep `ObjectTransform` as that instance 3×4 (or an
equivalent last-row Numerics form for `p * M`) and stop baking
it into vertices if the goal is the native path.
