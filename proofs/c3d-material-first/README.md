# First C3D / static-mesh material bind after Leave

Investigation only. Production `src/` was not edited.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Question: when does native first **bind a C3D / static-mesh
material** after Leave Frontend? That means DiffuseMapID →
device `SetTexture`, not frontend sprites, not landscape
cells, not FFP `SetMaterial`.

Sibling: `proofs/c3d-first-submit/` (DIP / insert). Bank
open: `proofs/texture-library-open/`. Land bind:
`proofs/landscape-first-draw/`. Packets:
`docs/status/investigations/2026-08-18-static-c3d.md`,
`2026-08-18-materials.md`, `2026-08-18-resource-manager.md`.

---

## Verdict

**First static C3D material bind is after Leave + Lookout
things + bit `0x20` drain.** Not frontend.

| Step | Native VA | What |
|---|---|---|
| Parse (not bind) | `00A89450` → `00ABF6B0` | stride-48 record; Diffuse at **+16** |
| Resolve | `00BB30A0` → `00BA9360` | `edi = [mat+16]`; 44-byte `GBANK_MAIN` slot |
| Program | `00988020` / `00988140` | `VSHADER_STATIC_DIRLIGHT_FOG` / `PSHADER_TEXTURE_DIFFUSE` |
| Count | `00BB3D91` `push 1` | one stage |
| Device bind | `00BB2540` @ `00BB301E` | `SetTexture(stage, [array])` vtbl+260 |
| After DIP | `00BB3E13` | `SetTexture(stage, 0)` unbind |

Frontend **does** `SetTexture` (`00A0AEA0` type `0x22`,
`frontend.big`). **DISPROVEN** as C3D. Landscape
`00BF50E0` / `00BF5491` on the same later Present is LEV,
**before** bit `0x20`. **DISPROVEN** as C3D material.

Which Lookout Graphic is first in slot-0 order is **UNREAD**.
The **site** is **PROVEN**.

---

## Recovered order (no-save New Game)

```
0042EC7C retail pump
  0042DF9E  frontend frame
    0041BEB0 type 0x22
      00BAE2D0 → 00A0AEA0  SetTexture vtbl+260   FRONT_END sprites
    0042E0BB  [retail+88].vtbl+32 = 00B27D90
      no type 0x18 → no 00BB30A0 / no 00BB301E
0042F2A2 Leave
  0042EBB6  teardown Present (not a C3D bind)
  004184BD Init Game
    "Init Graphics" 00416C8A
      009F83D0("GBANK_MAIN")     directory only
      00416D1B 009BE8B0          FourCC probe, not a DiffuseMapID
    00416953 Load world
      00A09F20 MBANK_ALLMESHES   directory
      009AD410 HANDLE            no parse, no bind
004189C2 game pump
  first 00435530: no region, no C3D
later 00501450(1) LookoutPoint
  006C2170 Loading objects
    0077BA40 / 007E15C0 + 009AD410
    first MeshBank get → 00A89450
      [mesh+140] materials, add ebp,48
      00ABF6B0                   PARSE
then next 00B27D90 with type 0x18
  0x4 / 0x40  00BF50E0 / 00BF5491     landscape (not C3D)
  0x20        00B33010 → 00B32610 → 00B849F0(0)
              → 00BBC460 → 00BBC130 → 00B81640
              → 00BB30A0
                   mat = [mesh+140] + 48*index
                   00BA9360([mat+16])
                   00988020 / 00988140
                   push 1 @ 00BB3D91
                   00BB2540 → 00BB301E     FIRST C3D bind
  0x100 / 0x80  00BD3070 00BA9360          PALSKIN later
```

Game caller of `012A0F3C+32` after Leave is still **UNREAD**
(same as `c3d-first-submit`). Pairing is the nonempty
`00B27D90` after `006C2170`, not `0042DF9E`.

Engine ctor `00B26662` → compact `00B8B630` looks up the
shader **names**. That is Init Engine, **before** Leave, and
it does **not** read a C3D material. **DISPROVEN** as first
C3D bind.

---

## 1. Material record is not the bind

Serialize `00ABF6B0` (`00A8958B`, only `E8` from
`00A89450`). `esi` is the 48-byte record (`add ebp,48` at
`00A89597`). Layout matches `MeshMaterial`:

| Off | Field | Serialize |
|---|---|---|
| +0 | vtbl | — |
| +4 | id | `00993EB0` |
| +8 | name | `00995AE0` |
| +12 | DecalID | `00993EB0` |
| **+16** | **DiffuseMapID** | `00993EA0` |
| +20 | bump | `00993EA0` |
| +24 | reflection | `00993EA0` |
| +28 | illumination | `00993EA0` |
| +32 | MapFlags | `00993EA0` |
| +36 | SelfIllumination | `00993EA0` |
| +40..+43 | Flag0..Flag3 | `00993E30` |

`DiffuseMapID` **is** the `textures.big` / `GBANK_MAIN` entry
id (`MeshFormatTests`; oak 3880, leaves 2119). First parse of
a Lookout Graphic is after `006C2170` / first
`MeshBank.Get`. **PROVEN** as file layout. **DISPROVEN** as
device bind.

First-seen static-lit does **not** consume Flag1
(`FirstSeenStaticLitReadsFlag1=false`). Caller
`00BB30EC` reads `[mat+40]` (Flag0) into a state byte.
Bump / illumination stay in the record.
`FirstSeenBindsC3dBump=false`.

---

## 2. Resolve: `00BA9360` reads `[mat+16]`

`00BB30A0` (listing `00b80000.txt`):

```
00BB3BC5  mov ebx, [edx+140]     ; materials base (mesh+140)
00BB3BD5  lea edi, [eax+eax*2]
00BB3BDA  shl edi, 4             ; *48
00BB3BDD  add edi, ebx           ; mat = materials[index]
…
00BB3C09  call 00BA9360          ; ecx = mat
```

`00BA9360`:

```
00BA9362  mov edi, [ecx+16]      ; DiffuseMapID
00BA9365  test edi, edi
00BA936B  jne 00BA937C
00BA9372  call 009FD150          ; id==0 → lea [this+id*8+692]; not CreateTexture
00BA93A2  mov edx, [esi+480]     ; 44-byte graphic cache
00BA93CE  imul edx, edx, 44      ; same stride as 009FD910
```

Non-zero id walks the `GBANK_MAIN` 44-byte table
(`2026-08-18-resource-manager.md` §4.3). Occupied slot
`[+40]` is the runtime texture object. Later
`00BB2540` does `mov eax,[wrapper]; SetTexture(stage,eax)`.

`009FD910` (graphic vtbl+52) is the named id→blob load that
can reach `009BE8B0` DXT `CreateTexture`. First clock of
that CreateTexture for the first Lookout DiffuseMapID is
**UNREAD**. It is **not** Init Graphics (`00416D1B` is a
FourCC probe). It is **not** frontend
(`0042DE1D` is `GBANK_FRONT_END`).

---

## 3. Device bind: `00BB301E`, count 1

After VS/PS attach (`00BB3A1A` `00988020`, `00BB3A36`
`00988140` at `[0x1436E78]+0x1AC`):

```
00BB3D8D  push 9
00BB3D8F  push 1
00BB3D91  push 1                 ; texture count
00BB3D93  push edx               ; [rec+4] array
…
00BB3DB6  call 00BB2540
```

Inside `00BB2540` (after stream `vtbl+400`):

```
00BB2FDE  mov eax, [esp+116]     ; count
00BB2FE2  xor ebx, ebx           ; stage
00BB2FF0  mov edx, [esp+112]     ; array
00BB2FF4  mov eax, [edx+edi-0x3D00]
00BB3005  mov eax, [eax]         ; IDirect3DTexture9
00BB3010  push eax
00BB301C  push ebx
00BB301E  call [edx+260]         ; SetTexture
```

`edi` starts at `0x3D00`; `0x3D00-15616=0`, then
`add edi,4`. Stage 0 only when count is 1.

Other `SetTexture` hits in `00BB2540` / `00BB30A0`
(`00BB3C94`, `00BB3E13`) push **0** — unbind. **DISPROVEN**
as the first C3D albedo bind.

PS is `PSHADER_TEXTURE_DIFFUSE` (`tex t0`; `mul v0*c0`;
`mul_x2 t0`). VS `mov oT0, v2` (FVF `0x112` TEX1). First-seen
PS `c0` is identity `PSCONST_OUTPUT_FACTOR`;
`[wrapper+913]=0` so the material colour overwrite of slot 2
does not run (`FirstSeenStaticPsC0HasWriter=true`).

No first-seen `SetMaterial` / `D3DRS_LIGHTING` on this path.

---

## 4. Not frontend sprites

| | Frontend `0042DF9E` | First C3D after Leave |
|---|---|---|
| Packer | type `0x22` `0041BEB0` | type `0x18` (insert **UNREAD**; factory `00BBCF30`) |
| Bind helper | `00A0AEA0` (`push 2` stages) | `00BB301E` (count **1**) |
| Bank | `GBANK_FRONT_END` / `frontend.big` | `GBANK_MAIN` / `textures.big` |
| Id | widget `GraphicIndex` | C3D `[mat+16]` |
| Site | `00BAE2D0` | `00BB30A0` → `00BB2540` |
| After Leave | stopped (`0042F2A2`) | first nonempty `0x20` |

`0042E0BB` does reach `00B27D90` on Press Start.
Slot 0 is empty. **DISPROVEN** that frontend issues
`00BB301E`.

Leave drops the FRONT_END handle (`0042DD28`). Host
`TextureLibrary` is constructed in `EnterGame` / after
Leave — **MATCH** first-open (`texture-library-open`).

---

## 5. Same Present, not first 3D texture

Layer rank on the first nonempty game Present:

`0x4` / `0x40` landscape → **`0x20` static C3D** →
`0x2000` sky → `0x100`/`0x80` PALSKIN.

So the first **device** `SetTexture` after Leave on that
frame is landscape `00BF50E0` (cell mask / albedo), not
C3D. First **C3D material** bind is still `00BB301E`.

PALSKIN hero **4299** also calls `00BA9360`, on bits
`0x100`/`0x80`. **DISPROVEN** as first static bind.

Oakvale house **6909 / 6911** / kid **4300** are not on
this Present (`c3d-first-submit`). Rugs **1740** / books
**2315** bump ids are stored and **unbound**.

---

## Host compare (read only)

`EngineLifecycle.SubmitCurrentWorld` after `HeroSpawned`:
`Meshes.Get` then `MeshBatches.BuildMeshes`.
`MeshTriangle.TextureId` = `materials[i].DiffuseMapId`.
`InstanceMaterialKey` is one `DiffuseId`, no bump.

| Host | Native | Class |
|---|---|---|
| `TextureLibrary` after Leave | `00416C8A` `GBANK_MAIN` then first-id load | **PROVEN** timing of **open** |
| `TryLoad(DiffuseMapId)` | `00BA9360` / `009FD910` | **EQUIVALENT** id |
| RGBA cache + `SetTextures` rebuild | DXT SCRATCH `009BE8B0`, persist-by-id | **DIVERGE** format / lifetime |
| Concat land+C3D+sky one `TexturedMesh` | separate families / binds | **DISPROVEN** as native |
| Two descriptor sets on static | one stage | **DIVERGE** |
| `InstanceDraw.StaticLit` t0 = Diffuse | `00BB301E` stage 0 | **EQUIVALENT** |

---

## Classification table

| Claim | Status |
|---|---|
| First C3D material **bind** is `00BA9360` + `00BB301E` on bit `0x20` after Lookout things | **PROVEN** (site) / first-id clock **UNREAD** |
| DiffuseMapID is `[mat+16]` / `textures.big` id | **PROVEN** |
| Texture count 1; bump / illum unbound | **PROVEN** |
| `00ABF6B0` is the GPU bind | **DISPROVEN** (serialize) |
| Compact `00B8B630` is the first C3D bind | **DISPROVEN** (engine ctor, shader names) |
| Init Graphics `009BE8B0` is a Lookout Diffuse | **DISPROVEN** (FourCC probe) |
| Frontend `00A0AEA0` is a C3D material | **DISPROVEN** |
| Landscape `00BF50E0` is a C3D material | **DISPROVEN** |
| FFP `SetMaterial` first-seen static | **DISPROVEN** |
| Flag1 / type-20 / NONE first-seen static | **DISPROVEN** |
| First set is Oakvale 6909/6911 | **DISPROVEN** |
| First Lookout DiffuseMapID in walk order | **UNREAD** |
| First MAIN `009FD910` CreateTexture vs `00BA9360` | **UNREAD** |
| Game `vtbl+32` after Leave | **UNREAD** |

Dumps: `newgame-trace/static-lit-ccw-00bb2540`,
`static-lit-caller-00bb30a0`,
`static-settexture-bind-00bb301e`,
`c3d-material-serialize-00abf6b0`,
`static-compact-ctor-00b8b630`,
`calls-createtexture-dxt1-009be8b0`,
listing `00BA9360` / `00BB3D91` / `00A8958B` / `00B26662`.
Host (unread-only): `MeshFile.MeshMaterial`,
`TextureLibrary`, `InstanceDraw.InstanceMaterialKey`,
`EngineLifecycle.SubmitCurrentWorld`.
