# B — Native camera / matrices / coordinate system vs host

Investigation only. No production edits. Status words:
**PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** / **EQUIVALENT**.

Do not treat `docs/render/WORLD_SPACE_CONTRACT.md` as gospel where
it contradicts the exe or the no-save first Present. That document
is the Oakvale intro / SHOT2 *contract*, not the no-save first
Present. Transform parity is **not proven**.

---

## 0. What first New Game Present actually is

No-save first *rendered* 3D scene is **LookoutPoint** (WLD NewRegion
index 1) + adult hero mesh 4299 + `006B3FF0` seed. It is **not**
`StartOakValeWest` / `Q_NewOakValeIntro` / `CAM_OVIF_SHOT2`.

| Claim | Status | Evidence |
|---|---|---|
| No-save enqueue `00501450` → `00500540(1,0,0)` LookoutPoint | PROVEN | `docs/status/README.md`; `EngineLifecycleTests` |
| `Q_NewOakValeIntro` / SHOT2 is not first no-save Present | PROVEN | `00DBDE40` only from `00DABAC0`; `FirstSeenCallsUseCamera=false` |
| `WorldFrame<=1` skips camera **and** `00435F70` Present | PROVEN | `00417001` `cmp eax,1` / `jle 0041725F`. Present is **after** WorldFrame>1, not a skipped-camera Present |
| Default New Game `[0x13B8630]==0` → `0041707E` not `004164E0` | PROVEN | `00417050` `test eax,eax` / `jle 0041707E` |
| `004164E0` steps `arg*(1/15)` when ticks>0 | PROVEN | `fild [ebp+8]` `fmul [0x122EDB8]` `00BFEA70` |
| `0041707E` interpolates, then `0049E080` → `006B42F0`, then `00435F70` | PROVEN | dump 186 insns |

`WORLD_SPACE_CONTRACT.md` rows that name SHOT2 as the camera origin
are the Oakvale-intro space, **not** first no-save Present. Using
that contract as the live host camera is a leftover.

---

## 1. Native matrix pipeline (VA → classification)

Two objects must not be collapsed:

1. **WorldCamera** at `world+24`, ctor `006B4900`, size `0x1970`,
   vtbl `0125D53C`. Gameplay follow / blend bank.
2. **Render camera** at `[0x1436EA0]`, ctor `00B31700`, vtbl
   `012A12A4`. This is what `00B30B50` / `00B2FC50` turn into
   wrapper W/V/P and VS `c5–c8`.

The link from (1) output to (2) source is **UNREAD**.

### 1.1 WorldCamera bank (game state, not the VS)

| VA | Role | Status |
|---|---|---|
| `006B4900` | Ctor. Vtbl `0125D53C`. Six `008864A0` slots at +84 and six at +3188, stride `0x1F4`. `+24 = -1.0` from `[0x1236700]`. `+3092/+3108=(1,0,0)`. `+3088/+3104=0.2`. `+3084=0`. `+61=0` so pose runs. `+68=0` so first `006B42F0` seeds. Allocates +6496 (`008852E0`) and +6500 (`006B84B0`). | **PROVEN** (220) |
| `006B63C0` | Bank copy 6×`0x1F4` via `006B5DF0`, then trailing floats +3000…+3100. `006B3FF0` uses this A→B (`+84` → `+3188`) twice when `+68==0`. | **PROVEN** (91) |
| `006B3FF0` | Seed when `+68==0`. Order: `006B63C0`, `006B8640` on `[this+6500]`, loop 6× `008889C0`, `006B2CA0`, `006B3030`, `006B3B80`, then `006B63C0` again and `+68=1`. | **PROVEN** (208) as control flow. Slot *contents* after the list walk are **UNREAD** |
| `006B8640` | Runs on the +6500 object (`006B84B0`), not on V0/V1. Copies +144 / +172 / +220…+248 / +276 / +316 through `00884E90`/`00884F60`/`00884F90`/`00884FE0`. | **UNREAD leftover** (65). Does not write WorldCamera +3092 |
| `008889C0` | Follow-slot list helper → `008884D0` / `008886E0`. Stride-48 walk. FORWARD_TREE: not a V0 writer. | **UNREAD leftover** (59) |
| `006B2CA0` | Pose. `+61!=0` skip. First-seen `+3084/+424…+444/+412=0` and `[0x122DEDC]=0` → two normalised dirs `(1,0,0)`; blend `+412=0` writes `V4=(-1,0,0)` at +3144 (`add esi, 0xC48`). Writes +3120 / +3132 along the way. General non-zero yaw/pitch trig is **PARTIAL**. | **PROVEN** first-seen (262). **PARTIAL** for live follow angles |
| `006B3030` | Follow spring. First-seen `+3168==0` runs; `004978A0` LCG seed **UNREAD** so yaw rotate is not applied. Weight clamp `[0.04,0.2]`; ctor 0.2 stays. V0/V1 stay ctor `(1,0,0)`. | **PROVEN** first-seen skip of rotate |
| `006B3B80` | Tick. First-seen `+460==0` and qword `+24==-1` → `jne 006B3E59` ret. No V0 write. | **PROVEN** first-seen skip |
| `006B42F0` | If `+68==0` call `006B3FF0`. Clamp t to `[0,1]`. Lerp SlotB `+6188…` with SlotA `+3084…` into `+6292` (param), `+6296` (3f), `+6308` (weight), `+6312` (3f), `+6324` (weight), `+6328` / `+6340` / `+6352` (dirs; then `00A14440` on those three). Then `008857E0` / `00885900` / `008859F0` on `[this+6496]`. Then `[0x13B86A0]+40 → +44 → vtbl+244`. | Lerp math **PROVEN**. `+6296` *semantic* (eye vs axis) **PARTIAL**. vtbl+244 apply **UNREAD** |
| `0049E080` | `004C74F0`, `0051EBD0`, `006B42F0(world+24, record+4)`. Optional `[0x13B8394]` debug path. | **PROVEN** (92) as the apply caller |

**First-seen WorldCamera vectors after seed (PROVEN):**

- SlotA.V0 / V1 stay ctor `(1,0,0)` — these are **not** a Lookout
  hero eye. Host `ApplyManagerOutput(V0,V1,V2)` therefore writes
  ScriptedCamera to a degenerate `(1,0,0)` look-at.
- `+3092` in the ctor is the same `(1,0,0)` axis, **not** a world
  position. Treating `+6296` as eye is a **PARTIAL** host label
  until vtbl+244 is read.

`SeedAt(1.6m)` is **not** on the live path (`SeedHero` →
`ComputePose` only). The method still exists as a test helper.
Status README “Host SeedAt(1.6m) DIVERGE” is **stale for the
call site**; the leftover FOV-72 / V0-as-eye problem is not.

### 1.2 GameCamera helper (default FOV)

| VA | Role | Status |
|---|---|---|
| `006FD8C0` | GameCamera at `world+44`, size `0xC8`, vtbl `01264A8C`. Calls `00A0C130` on `this+4`. | **PROVEN** (63) |
| `00A0C130` | Packs helper: `+0` pos, `+12` look, `+24` up, `+40` flags (1 or 3), `+44` FOV, `+48` extra. | **PROVEN** (37) |
| imm `0x3E471B48` | Pushed as FOV into `00A0C130`. Float ≈ `0.194444` = **70/360 turns** (70°). | **PROVEN** bits; unit is turns because `00B314E0` does `* 2π` |
| `0069AE80` | GameCameraManager at `world+48` / copy `world+52`. | **PARTIAL** (not walked this pass) |

Whether GameCamera+4 is the `[0x1436EA0]+12` source on first
Present is **UNREAD** (no `00B2FBF0` xref recovered to this
helper). It is the only first-seen FOV constant that is *not*
SHOT2's 72.

### 1.3 Render camera → wrapper → VS

```
00B23B50 bind (UseCamera; NOT first no-save)
  └ 00B2FBF0  [camera+12] = helper pointer          PROVEN (3)
  └ 00B314E0(1)
00B2799D pre-pass 00B314E0(0)                        PROVEN callers=2

00B314E0  (+536==0 first-seen; spline 00B31160 skipped)
  helper +0 / +12 / +24 → pos / look / up            PROVEN
  00A14440 normalize look, up
  right = up × look, then 00A14440                   PROVEN
  pack (right, look, up) column-stride-12 at src+16
  FOV: 00A0BE90 [helper+44] * 360 * 1/360 * 2π
       = helper+44 turns → radians                   PROVEN
  near 0.1 (0x3DCCCCCD), far 4000 (0x457A0000)
  minZ [0x01399D44]=0.1, maxZ 0.99 (0x3F7D70A4)      PROVEN
  00B30B50(source, 1, arg)

00B30B50
  copy 0x1B dwords source → camera+20
  +176/+180 = viewport w/h (source rect, or [0x1436EB0]+8/+12)
  +84 two-FOV flag clear (first-seen):
    letterbox = (0.75 − h/w)*0.5 + 1                 PROVEN
    0.75 at [0x1238174]=0x3F400000
    cotH = 1/tan(letterbox * fovRad * 0.5)
    cotV = cotH * (w/h)
    stored +212 / +216
  copy helper+16 (12 floats) → camera+128
  translation +164/+168/+172 = −axis·pos
  copy unscaled 3x4 → +276 (fog reads this)
  scale camera+128 columns 0 and 1 by cotH / cotV    PROVEN
  invert cot-scaled 3x4 → +228 (stride 12)
  write camera+372 projection (see §1.5)
  arg1≠0 → 00B2FD60 frustum
  arg2≠0 → 00B2FC50 write wrapper                    PROVEN
    bind pushes 1 so this runs; pre-pass pushes 0 and skips

00B2FC50
  00988350(camera+128) → wrapper+560 VIEW            PROVEN
  00988540(camera+372) → wrapper+624 PROJ            PROVEN
  00988320(camera+84)  → wrapper+896 cam pos         PROVEN
  009881F0(identity 3x4 on stack) → wrapper+496 WORLD PROVEN
```

Sky `00B662F0` else-path calls `00B30B50` again with near 100 /
far 10000 / minZ 0.99 / maxZ 1, flushes WVP, then restores the
world camera. Same V, different P. **PROVEN**.

### 1.4 World / view / proj / WVP builders

| VA | What it writes | Status |
|---|---|---|
| `009881F0` | 3x4 column-stride-12 → wrapper+496 as rows; bottom `(0,0,0,1)`. Dirty `0xC990`. | **PROVEN** (57) |
| `00988290` | Identity world into +496 if `+488==0`. | **PROVEN** (falls through same fn) |
| `00988350` | Same gather as world, dest wrapper+560 (view). | **PROVEN** (32) |
| `00988320` | 3 floats → wrapper+896 (camera world pos; landscape `T(cam)` source). | **PROVEN** (12) |
| `009883F0` | Build P *directly* at wrapper+624: `M11=M22=1`, `M33` at +664, `M34=Q` at +668, `M43=1` at +680, `M44=0`. Args near/far/minZ/maxZ. | **PROVEN** (32). First-seen *bind* uses `00988540` instead, same numeric P |
| `00988540` | `rep movsd` 16 dwords camera+372 → wrapper+624, then `00A5CAB0` **transpose**. | **PROVEN** (18 + 31) |
| `00A5CAB0` | In-place 4×4 transpose. Camera+372 is Numerics/host form (`M34=1`,`M43=Q`); wrapper is VS-row form (`M34=Q`,`M43=1`). | **PROVEN** |
| `00988A50` | `W(+496) * V(+560)` then `* P(+624)` → +752. `SetVertexShaderConstantF` `[dev+376]`, register `[inner+120]=5`, count 4. | **PROVEN** (301). SSE path `[0x13D2880]` first-seen |
| `00989A60` | `SetVSConstantF` count **1**. Landscape per-cell UV / c3. **Not** WVP. | **PROVEN** (40) |
| `00989B00` | `SetVSConstantF` count 4. Inverse rows / mesh path. First-seen landscape does **not** call `00B54310`. | **PROVEN** unused on first landscape |

**SetTransform (`D3DTS_VIEW=2`, `D3DTS_PROJECTION=3`, `D3DTS_WORLD=256`):**
no first-seen 3D site. WVP is VS constants, not FFP. **PROVEN
absence** on the `00988A50` / `00B2FC50` / per-cell path.
`IDirect3DDevice9::SetViewport` **is** used (`009BEF80` → vtbl+188).

### 1.5 Projection formula (native memory)

`00B3106C` / `009883F0`:

```
Q    = ((minZ − maxZ) * near * far) / (far − near)     // M34 in wrapper
M33  = minZ − Q / near
```

First-seen world: near **0.1**, far **4000**, minZ **0.1**, maxZ **0.99**.
Sky: near **100**, far **10000**, minZ **0.99**, maxZ **1**.

Wrapper (after `009883F0` or after `00988540`+transpose):

```
[ 1  0   0   0 ]
[ 0  1   0   0 ]
[ 0  0  M33  Q ]
[ 0  0   1   0 ]
```

VS `dp4 oPos, pos, c5–c8` on those rows:

```
clip.xy = view.xy
clip.z  = M33 * view.z + Q
clip.w  = view.z
```

Host `LandscapeFrustum.FirstSeenDx9Projection` stores the
**upload-transpose** (`M34=1`, `M43=Q`) so `p*P` matches that VS.
That is the camera+372 layout, **not** a second Fable P.

Y sign of Fable P is **+1**. Vulkan NDC Y-down is
`Dx9VulkanProjection.ToVulkanWvp` (`* diag(1,-1,1,1)`), applied
in `VulkanLineRenderer.Draw` only. Baking the flip into `009883F0`
is **DISPROVEN**.

### 1.6 View formula

`00B314E0` does **not** `CreateLookAt`. Look is **not**
re-orthogonalized against up.

```
lookN = normalize(helper+12)
upN   = normalize(helper+24)
right = normalize(upN × lookN)
```

Packed `(right, look, up)` at source+16, translation 0; `00B30B50`
overwrites translation as `−dot(axis, pos)` then scales the first
two columns of the 3x4 by cotH / cotV.

Host Numerics 4×4 consumed with that P (look on **Z**, up on **Y**):

```
[ r.x  u.x  l.x  0 ]
[ r.y  u.y  l.y  0 ]
[ r.z  u.z  l.z  0 ]
[ −r·p*cotH  −u·p*cotV  −l·p  1 ]   // after XY cot scale
```

`FirstSeenViewLookIsZ` is **PROVEN**. Putting look on Y sends a
SHOT2 look-at to NDC Y≈−8 (contract note). That is a host packing
rule, not a world-axis flip.

`LandscapeFrustum.CotScaledInverse` rebuilds `right = look × up`
(the opposite cross). Native inverts the **already-built**
`up × look` 3x4. Inverse/frustum is therefore **PARTIAL** vs exe;
it is **not** the WVP the VS consumes.

### 1.7 Landscape world vs object world

| Path | Native W | Host W | Status |
|---|---|---|---|
| Static / PALSKIN / bind | `00B2FC50` → `009881F0` **identity** | `IdentityWorld()` | **PROVEN** / **EQUIVALENT** |
| Landscape per-cell | `00BF46A2`: identity 3x4 + `[0x1436EA0]+84/+88/+92` in last column → `009881F0`. VB is **camera-relative** (`00BFE050`). `T(cam)` restores region-local. | Host STB is already region-local world → **identity** W | Native `T(cam)` **PROVEN**. `T(cam)` on host world verts **DISPROVEN**. Host identity **EQUIVALENT** for clip |
| Sky | identity W, same V, sky P | same | **PROVEN** |

Same V and (world) P for landscape and objects. Landscape differs
only in W, and that difference is cancelled by the cam-relative VB.
Host `HostLandscapeViewProjection` == object `ViewProjection` when
both use identity W. `LandscapeViewProjection` (`T(cam)` on world
STB) is the **DISPROVEN** path; `SilkEngineHost.Draw` correctly
passes `HostLandscapeViewProjection`.

### 1.8 Viewport / depth range

| Item | Native | Status |
|---|---|---|
| Backbuffer | PE default 1024×768 | **PROVEN** (`00403079` / `EngineLifecycle.DisplayDefaultWidth`) |
| `009BEF80` | SetViewport vtbl+188; x/y=0; w/h=full backbuffer; **MinZ 0, MaxZ 1** at +492/+496 | **PROVEN** (145) |
| Camera +176/+180 | pixel width/height used by `00B30B50` letterbox | **PROVEN** |
| Depth in P | minZ/maxZ 0.1/0.99 baked into M33/Q; viewport does **not** rescale | **PROVEN** |
| Half-pixel | — | **UNREAD** |

### 1.9 Object transform

| Item | Native / file | Host | Status |
|---|---|---|---|
| C3D local | cm, RH, X right Y forward Z up | `MeshToWorld=0.01` | **PROVEN** |
| TNG basis | `RHSetForward`, `RHSetUp`; `right = forward × up`; then `up = right × forward` | `WorldGeometry.ObjectTransform` | **PROVEN** (lamp Z-span test) |
| `Matrix4x4.CreateWorld` | Y-up, negates forward | **DISPROVEN** (lays lamp on its side) | **DISPROVEN** |
| Composition | scale * basis (rows = right, forward, up, translation) | same | **PROVEN** as the host matrix that matches file axes. Exact native multiply site **UNREAD** this pass |

### 1.10 Handedness / axes / multiply / transpose

| Rule | Native | Host | Status |
|---|---|---|---|
| World handedness | right, Z-up, X east, Y north | same | **PROVEN** |
| View handedness | right = up × look; look is +Z in view; up is +Y | `CotScaledView` | **PROVEN** |
| Multiply | wrapper product W then V then P; VS `dp4` is row-register | `ComposeWvp = W*V*P`; `p * WVP` | **PROVEN** / **EQUIVALENT** |
| 3x4 → wrapper | gather columns → rows (`009881F0` / `00988350`) | Numerics already row-vector | **PROVEN** |
| P camera+372 → wrapper | `00A5CAB0` transpose | host stores pre-transpose so `p*P` matches VS | **PROVEN** |
| GPU upload | `SetVSConstantF` 16 floats in wrapper order | memcpy Numerics; GLSL column-major reads as needed transpose | **EQUIVALENT** (`CameraProjectionTests.Gpu_upload_keeps_row_major_bytes`) |
| Vulkan Y | not in Fable P | `ToVulkanWvp` only | **PROVEN** / **EQUIVALENT** |

---

## 2. Host vs native — every difference

| # | Topic | Native (first no-save Present) | Host live path | Class |
|---|---|---|---|---|
| 1 | Which scene | LookoutPoint + hero 4299 | EngineLifecycle submits LookoutPoint (`SubmitCurrentWorld`) | match |
| 2 | Which camera object | Render camera `[0x1436EA0]` from helper `[+12]` after `00B30B50`/`00B2FC50` | `EngineFrame.Camera` = `ScriptedCamera` after `ApplyManagerOutput(WorldCamera.V0,V1,V2)` | **DIVERGE** (wrong object / wrong fields) |
| 3 | Eye / look consumed | Helper `+0/+12/+24` (GameCamera `00A0C130` or a later bind). WorldCamera `+6296` is **not** proven to be eye | `ScriptedCamera.Position/LookAt/Up` = SlotA V0/V1/V2 = ctor `(1,0,0)` | **DIVERGE**. Degenerate look-at; `Forward` falls back to `UnitY` |
| 4 | `SeedAt(1.6m)` | never | not called on live path (`SeedHero`/`ComputePose` only). Method remains as a test helper | **fixed as a call**; README row stale |
| 5 | FOV | Helper+44 is **turns**. GameCamera ctor writes `0x3E471B48` ≈ 70/360 turns (70°). `00B314E0` → radians. SHOT2 72 is UseCamera leftover | `ScriptedCamera.FovDegrees` default / `RegionTravel.IntroCameraFovDegrees` = **72**. `LetterboxCots(DegreesToRadians(72))` | **DIVERGE** (SHOT2 leftover, and degrees vs turns) |
| 6 | Aspect / letterbox | `00B30B50` uses camera +176/+180 = **1024×768** (h/w=0.75). `0.75−h/w=0` → letterbox scale 1. `cotV = cotH * 4/3` | `ScriptedCamera.ViewMatrixAt(aspect)` → `LetterboxCots(..., aspect, 1f)` so letterbox uses **window** w/h. `Program.cs` `aspect = FramebufferSize.X/Y` | **DIVERGE** on any non-4:3 window |
| 7 | `SubmitSidePlanes` aspect | same 4:3 viewport | hard-codes `4f/3f` (good) while Draw uses window aspect | **internal host split** |
| 8 | FirstSceneWorld / tests | n/a for no-save Present | still bind `CAM_OVIF_SHOT2`, FOV 72, aspect 4:3. Valid as Oakvale-intro contract, **not** first Present | leftover / wrong scene |
| 9 | `WORLD_SPACE_CONTRACT` camera origin | first Present is not SHOT2 helper | contract row “SHOT2 helper +0/+12/+24” | contract **over-applied** |
| 10 | WorldCamera `+6296` meaning | lerp of ctor axis `(1,0,0)` first-seen; apply is vtbl+244 **UNREAD** | mapped to `ScriptedCamera.Position` | **PARTIAL** / likely **DISPROVEN** as eye |
| 11 | `006B42F0` tail | `008857E0` + `[0x13B86A0]+40+44 vtbl+244` is the consume site | host stops at writing V0/V1/V2 | **UNREAD** on host; **DIVERGE** if that call writes the helper |
| 12 | `006B8640` / `008889C0` | run, do not write V0 first-seen | noted, not implemented as pose | match as leftover |
| 13 | Projection Z | 0.1 / 4000 / 0.1 / 0.99; `clip.w=view.z` | `WorldProj()` same | match |
| 14 | Sky P | 100 / 10000 / 0.99 / 1 | `SkyViewProjection` same | match |
| 15 | View basis | `up × look`, look on Z, cot on XY, not `CreateLookAt` | `CotScaledView` | match for WVP |
| 16 | Inverse / frustum right | invert the `up × look` 3x4 | `CotScaledInverse` uses `look × up` | **PARTIAL** (frustum only) |
| 17 | Object W | identity (`009881F0`) | identity | match |
| 18 | Landscape W | `T(cam)` on **cam-relative** VB | identity on **world** STB (`HostLandscapeViewProjection`) | **EQUIVALENT** clip. `LandscapeViewProjection` (`T(cam)` on world verts) is **DISPROVEN** and is **not** what Draw passes |
| 19 | Object basis | RHSetForward/Up, `right=forward×up`, ×0.01 | `ObjectTransform` | match vs file; native compose VA **UNREAD** |
| 20 | `CreateWorld` | not used | tests prove it lays lamps on their side | **DISPROVEN** |
| 21 | Multiply / transpose | W*V*P; `00A5CAB0` on P copy; VS rows = wrapper | `ComposeWvp`; host P is camera+372 form | **EQUIVALENT** |
| 22 | SetTransform | not on first-seen 3D | n/a (Vulkan push constants) | **EQUIVALENT** absence |
| 23 | Viewport | 1024×768 MinZ 0 MaxZ 1 | window framebuffer; Vulkan viewport = swapchain extent | **DIVERGE** if the window is resized / not 4:3 |
| 24 | Vulkan Y flip | not Fable | `ToVulkanWvp` in `VulkanLineRenderer.Draw` | **EQUIVALENT** |
| 25 | When Present happens | `00417001` only after WorldFrame>1, after camera apply | `PresentToHost` when `WorldSubmitted && WorldCamera.Seeded` (can be the first Game pump after spawn) | **PARTIAL** timing |
| 26 | FlyCamera FOV | debug only | default 65°, `UnitZ` up, same P/V builders | debug-only; must not write game camera (it does not) |
| 27 | Depth range | viewport 0..1; P bakes 0.1..0.99 | same P; Vulkan depth 0..1 | **EQUIVALENT** |

---

## 3. Matrix the renderer consumes on first New Game Present

### Native (after WorldFrame>1, ticks=0)

```
0041707E
  t = clamp(DisplayTime * 15 − [game+72], 0, 1)
  0049E080 → 006B42F0(world+24, t)
    first call seeds 006B3FF0 (A copied to B, so lerp is the seed)
    UNREAD: vtbl+244 / 008857E0 must push that into the helper
  00435F70 → 00435530
    00B25950 layer flush
      00B2FC50 (from bind or sky restore)
        W = I
        V = cot-scaled camera+128
        P = camera+372 transposed into wrapper+624
      00988A50  W*V*P → c5–c8
      landscape cells overwrite W with T(cam) then 00988A50 again
    009BEEB0 Present
```

**Consumed WVP** = `00988A50(+752)` = identity (or `T(cam)` for
landscape VB) × cot-scaled view × 0.1/4000/0.1/0.99 P.

The **numbers inside V** (eye, look, FOV) for first Present are
**UNREAD** at the helper. They are **not** SHOT2. They are **not**
proven to be WorldCamera `+6296`. Best documented default FOV is
GameCamera `0x3E471B48` turns (70°). Best documented first-seen
WorldCamera vectors are ctor axes `(1,0,0)` / pose `V4=(-1,0,0)`.

### Host (`SilkEngineHost.Draw` ← `EngineFrame.Camera`)

```
cam = ScriptedCamera after ApplyWorldCamera
  Position = (1,0,0)
  LookAt   = (1,0,0)
  Up       = (1,0,0)          // V2 after ComputePose
  Forward  = UnitY fallback
  Fov      = 72°
W_obj  = I
W_land = I                    // HostLandscapeViewProjection
V      = CotScaledView(pos, UnitY, (1,0,0), letterbox(72°, windowAspect))
P      = FirstSeenDx9Projection(0.1, 4000, 0.1, 0.99)
WVP    = W*V*P
Vulkan = ToVulkanWvp(WVP)
```

That is **not** native first Present. Parity is not proven.

`FirstSceneWorld.WorldViewProj()` is a **third** matrix (SHOT2,
4:3, 72°) used by traces/tests. It is the Oakvale-intro contract,
not `EngineFrame.Camera`.

---

## 4. `SilkEngineHost.Draw(aspect)` vs native 4:3 1024×768

**Yes — Draw uses the window framebuffer aspect, not native 4:3.**

```114:132:src/Fable.Client/SilkEngineHost.cs
    public void Draw(float aspect)
    {
        ...
        Renderer.Draw(
            cam.ViewProjection(aspect),
            cam.Position,
            fogPlane,
            cam.SkyViewProjection(aspect),
            cam.HostLandscapeViewProjection(aspect));
    }
```

```118:132:src/Fable.Client/Program.cs
    var aspect = window.FramebufferSize.X / (float)window.FramebufferSize.Y;
    ...
        host.Draw(aspect);
```

`ScriptedCamera.FlyView` / `LetterboxCots(fov, aspect, 1f)` therefore
sees `height/width = 1/aspect`. Native `00B30B50` sees `768/1024=0.75`.

On a 1024×768 window the letterbox term is 1 and only the
pos/look/FOV diverges. On 16:9 the host **widens** horizontal FOV
(`(0.75 − 0.5625)*0.5 + 1 = 1.09375`) and sets `cotV = cotH * 16/9`.
Native stays 4:3 letterbox-neutral, `cotV = cotH * 4/3`.

Window create size is 1024×768 (`life.BackBufferWidth/Height`).
Resize / DPI / debug maximize changes aspect without a matching
native viewport change.

`SubmitSidePlanes` still uses `4f/3f`. Frustum and draw FOV can
disagree on a wide window.

---

## 5. Landscape vs objects — different view/proj?

**No. Same V and same world P. Different W, cancelled on host.**

Native:

- Objects / PALSKIN / sky geometry: `W = I`, `V = camera+128`,
  `P = camera+372` (sky swaps P only).
- Landscape: `W = T(cam)` on a **camera-relative** VB, same V, same P.
  `p_camrel * T(cam) * V * P = p_world * I * V * P`.

Host:

- `ViewProjection` = `I * V * P`
- `HostLandscapeViewProjection` = `I * V * P` (same matrix)
- `LandscapeViewProjection` = `T(cam) * V * P` is the DISPROVEN
  “apply T(cam) to world STB” path and is **not** submitted

Sky: same V, sky P (100/10000/0.99/1). Host `SkyViewProjection`
matches that split.

So landscape does **not** use a different view or a different world
projection. It uses a different W that is identity-equivalent on the
host because the VB space differs.

---

## 6. `WORLD_SPACE_CONTRACT.md` audit vs exe

Keep (still match the exe):

- C3D cm → ×0.01, RH Z-up
- TNG `ObjectTransform`; `CreateWorld` DISPROVEN
- Region-local = STB/WLD − (MapX, MapY); Lookout 3232/3488
- Landscape device VB camera-relative; `T(cam)` on **file** verts DISPROVEN
- Host landscape identity W EQUIVALENT
- View look on Z; not `CreateLookAt`
- `clip.w = view.z`; Y flip not Fable P
- Static world identity W

Do **not** treat as first-Present gospel:

- Opening frame “SHOT2 / HerosOldHouse / Oakvale” — that is the
  intro cutscene contract. First no-save Present is LookoutPoint.
- Camera origin “SHOT2 helper +0/+12/+24” — true for `00B23B50`
  UseCamera, **not** proven for first Present.
- Implied FOV 72 / aspect 4:3 as *the* first WVP — 72 is SHOT2;
  first-Present helper FOV is UNREAD (70° turns is the only
  first-seen default recovered).

---

## 7. Recommended integration (no invented flips)

Do **not** add rotations, axis swaps, `CreateLookAt`, baked Y
flips, or a 1.6 m eye to “fix” the picture.

1. **Keep** the recovered WVP algebra: `CotScaledView` (`up × look`,
   look on Z, cot on XY) × `009883F0` P (host stores camera+372
   form) × `ToVulkanWvp` only at submit. That path is PROVEN.

2. **Stop feeding WorldCamera V0/V1/V2 to the renderer** until
   `006B42F0` vtbl+244 / `008857E0` is read. First-seen those
   vectors are ctor axes, not an eye. Wiring them is a labelled
   guess, not a fix.

3. **Walk the missing consume edge** (next dump, not a flip):
   - `[0x13B86A0]+40+44 vtbl+244`
   - `008857E0` / `00885900` / `008859F0` / `00885A80`
   - who calls `00B2FBF0` on no-save (if anyone)
   - whether GameCamera+4 (`00A0C130`, FOV `0x3E471B48` turns)
     is `[0x1436EA0]+12` on first Present

4. **FOV:** drop 72 from the no-save Present path. Do not invent a
   replacement degree value. Either consume helper+44 as **turns**
   (`TurnsToRadians`) from the object `00B314E0` actually reads,
   or leave FOV UNREAD and do not letterbox a SHOT2 constant.

5. **Aspect:** letterbox and `cotV` must use the **native viewport
   1024×768 (4:3)**, not `FramebufferSize`. Window aspect belongs
   only to the Vulkan viewport pixel rect (already `FromFramebuffer`).
   Mixing them is how a 16:9 window silently changes Fable FOV.

6. **Landscape:** keep `HostLandscapeViewProjection` (identity W).
   Do not apply `T(cam)` to host STB.

7. **Objects:** keep `ObjectTransform` (0.01, RHSetForward/Up,
   `right = forward × up`). Do not revive `CreateWorld`.

8. **SHOT2 / FirstSceneWorld:** keep as the Oakvale-intro contract
   and its tests. Do not bind it from `EngineLifecycle` first
   Present.

9. **SetTransform:** do not add an FFP path. First-seen 3D is
   `SetVSConstantF` `c5–c8`.

Until step 3 is PROVEN, camera/matrix parity stays **unproven**.
The WVP *builder* is recovered; the *pose and FOV that enter it*
on first Present are not.
