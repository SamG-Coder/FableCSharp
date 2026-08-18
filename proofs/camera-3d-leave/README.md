# First 3D camera after Leave — `WorldCamera.cs`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `CAM_OVIF_SHOT2` / `00DBDE40`.
That path is later `Q_NewOakValeIntro`, not Leave / Init World /
first no-save 3D Present.

Question: after Leave frontend `0042F2A2`, what is the first
**3D camera**? Is it `WorldCamera.cs`? Does frontend already
own a world camera?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**DIVERGE** / **LEFTOVER** / **EQUIVALENT**.

Sources: PE listings
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
(`00417001` / `0042F2A2`),
`listing-00480000.txt` (`0049E080` / `004A5DF3` / `004A6F50` /
`004A7352` / `004A749B`),
`listing-00680000.txt` (`006B2CA0` / `006B3FF0` / `006B42F0` /
`006B4900`);
`src/Fable.Game/WorldCamera.cs`;
`src/Fable.Game/EngineLifecycle.cs`
(`PumpFrontendFrame` / `InitWorldCameras` / `TickWorld` /
`ApplyWorldCamera` / `SpawnHero`);
`src/Fable.Game/ScriptedCamera.cs`;
`docs/runtime/FORWARD_TREE.md` §§7, 11;
`docs/status/investigations/B-camera-matrices.md`;
`docs/status/investigations/2026-08-18-camera.md`;
siblings `proofs/camera-after-leave/`, `proofs/audit-worldcamera/`,
`proofs/dx9-3d-submit/`, `proofs/vulkan-3d/`, `proofs/cutscene-first/`;
`EngineLifecycleTests.World_camera_006B4900_slots_lerp_into_ScriptedCamera`,
`Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`.

---

## Verdict

**First 3D camera *object* after Leave is WorldCamera
`006B4900` at `world+24`.** It is constructed in Init World
`004A6E30`, not during frontend. First *seed* is
`004A5DF3 006B3FF0` on the first type-1 world tick. First
*apply* is `0041707E` → `0049E080` → `006B42F0` after
`WorldFrame>1`.

That object is **game state**, not the DX9 helper the VS
consumes. First-seen `+6296` is the ctor axis `(1,0,0)`, not
an eye. Live host WVP inputs (`ApplyRendererHelper(hero, V4,
+Z, 70°)`) are **policy**, not a dump of `[0x1436EA0]+12`.

| Claim | Status |
| --- | --- |
| Frontend / Leave constructs or ticks WorldCamera | **DISPROVEN** |
| First 3D camera object after Leave is `006B4900` `world+24` | **PROVEN** |
| Same Init World also builds GameCameraManager `0069AE80` then GameCamera `006FD8C0` | **PROVEN** (later in the same `004A6E30`) |
| First seed is `004A5DF3 006B3FF0`, not frontend | **PROVEN** |
| First apply is WorldFrame>1 `0049E080` / `006B42F0` | **PROVEN** |
| `WorldCamera.cs` ctor axes / weights / `+68=0` / timer −1 | **PROVEN** vs `006B4900` |
| First-seen pose V2/V3 `(1,0,0)`, V4 `(−1,0,0)` | **EQUIVALENT** zero-angle `006B2CA0` |
| `+6296` first-seen is an eye | **DISPROVEN** (`IsCtorAxis`) |
| Host `SeedAt(1.6 m)` on New Game / Leave | **DISPROVEN** (test leftover) |
| `006B42F0` tail writes the render helper / WVP | **DISPROVEN** (`008857E0` + vtbl+244 = colour filter) |
| Host helper-bind is the PE helper | **DISPROVEN** as dump. **DIVERGE** vs both ctor helpers |
| Scene of first no-save 3D is Lookout, not Oakvale SHOT2 | **PROVEN** |

---

## Recovered order (no-save New Game)

```
0042EC7C retail
  0042DF9E 2D UI  (no 006B4900 / 006B3FF0 / 006B42F0)
  render cam [0x1436EA0] already exists from Init Engine
    ctor 00B31700; +12 = 0                  // not WorldCamera
0042F2A2 Leave frontend                    // push "Leave frontend"
  009BE420 clear + 009BEEB0 Present (black)
  no WorldCamera alloc
0042F491 Init Game → 00418DCA → 004184BD
  Init World 004A6E30
    004A6F50 push 0x1970
    004A6F55 call 00BFEA1A
    004A6F67 call 006B4900                 // FIRST 3D camera object
    004A6F75 mov [esi+24], eax             // world+24
    004A7352 call 0069AE80                 // GameCameraManager world+48
    004A749B call 006FD8C0                 // GameCamera world+44
  00416953 FinalAlbion.wld
004189C2 GamePump
  WorldFrame 0→1: 004A5DF3 006B3FF0        // first seed
    push [environment+8]; ecx = [world+24]
    006B2CA0 / 006B3030 / 006B3B80
    +68 was 0 → copy banks; mov bl,1; [this+68]=bl
  WorldFrame<=1: 00417001 cmp eax,1 / jle  // skip 0049E080 and 00435F70
  WorldFrame>1: 0041707E → 0049E080
    004C74F0 / 0051EBD0
    006B42F0([esi+24], [record+4])         // first apply; +68 already 1
  first 3D Present still gated (004AEA70 often skip)
later 00501450 LookoutPoint + hero 4299
  nonempty 00B27D90 3D DIP                 // not this file
```

`Q_NewOakValeIntro` / `UseCamera CAM_OVIF_SHOT2` are **not**
on this list. **PROVEN**.

C# `InitWorldCameras` is only from `EnterGame` after Leave.
`PumpFrontendFrame` never calls it. **PROVEN** absence.

---

## 1. Frontend is not a 3D WorldCamera

Dump `0042EC7C` is AVI strings, QPC, `0042E3EE` input,
`0042DF9E` 2D flush, `009BEEB0` Present. Zero `E8` to
`006B4900` / `006B3FF0` / `006B42F0` / `0049E080`.

Dump `0042F2A2`:

```
0042F2A0  push -1
0042F2A2  push "Leave frontend"
0042F2D3  push 0x1F4
0042F2D8  call [eax+72]          ; fade
```

Then optional bank swaps when `[0x13B8616]!=0`. No camera
alloc. Leave Present is teardown (`009BE420` + `009BEEB0`),
not `0042DF9E` and not a 3D DIP. **DISPROVEN** as a 3D camera
frame (`dx9-3d-submit`).

The DX9 render-cam object `[0x1436EA0]` exists from Init
Engine. Helper `+12` is 0. That is **not** `WorldCamera.cs`.
Frontend DIP is type `0x22` / `0x27` (`mov oPos, v0`).

Host leftovers on frontend frames:

| Site | Class |
| --- | --- |
| `EngineLifecycle.WorldCamera = new()` at field init; `Construct()` not yet | **LEFTOVER** host |
| `Camera = new ScriptedCamera()` FOV `IntroCameraFovDegrees=72` on every `BuildFrame` | **LEFTOVER**. 72 is SHOT2 |
| `SilkEngineHost.Draw` reads `_frame.Camera` | **LEFTOVER** on frontend (batch path; verts empty) |

---

## 2. `WorldCamera.cs` vs dump

### 2.1 Ctor `006B4900` — tracked fields **PROVEN**

```
004A6F50  push 0x1970
004A6F67  call 006B4900
004A6F75  mov [esi+24], eax
```

| Dump | C# | Class |
| --- | --- | --- |
| vtbl `0125D53C`, size `0x1970`, world+24 | `Vtbl` / `ObjectSize` / `WorldOffset` | **PROVEN** |
| six `008864A0` at +84 and six at +3188, stride `0x1F4` | noted; not stored | leftover bank |
| `fld qword [0x1236700]` → `+24 = −1.0` | `TickTimerDefault` | **PROVEN** |
| `+3092/+3108 = (1,0,0)` (`0x3F800000`) | `SlotA.V0/V1` | **PROVEN** |
| `+3088/+3104 = 0x3E4CCCCD` (0.2) | `DefaultWeight` | **PROVEN** |
| `+3084 = 0`, `+61 = 0`, `+68 = 0` | `Param=0`, `PoseSkipFlag=false`, `Seeded=false` | **PROVEN** |
| `+6496` `008852E0`, `+6500` `006B84B0` | not modelled | leftover (colour-filter bank) |

`IsCtorAxis` is the right label for `+3092` / first-seen
`+6296`. Those vectors are **not** Lookout eyes.

Same `004A6E30` later:

```
004A7343  push 0x160
004A7352  call 0069AE80          ; world+48
004A7487  push 0xC8
004A749B  call 006FD8C0          ; world+44
```

Ctor helpers pack through `00A0C130`: GameCamera look `+Z`
up `(1,1,1)` FOV `0x3E471B48` ≈ 70°; manager look `+Z` up
`+X`. **PROVEN** as packed ctor state. **UNREAD** as the
first-Present helper pointer.

### 2.2 Seed `006B3FF0` vs `SeedHero`

Dump `004A5A40` tail:

```
004A5DEC  mov eax, [edx+8]       ; environment+8
004A5DEF  mov ecx, [esi+24]
004A5DF2  push eax
004A5DF3  call 006B3FF0
004A5E10  inc [0x13B89BC]        ; WorldFrame
```

Native seed: `006B63C0(+84→+3188)`, `006B8640(+6500)`,
`+3084 = arg`, 6× `008889C0`, then:

```
006B42A4  call 006B2CA0
006B42AB  call 006B3030
006B42B2  call 006B3B80
006B42B7  mov al, [ebp+68]
006B42BA  test al, al
006B42BD  jne 006B42D1
006B42C9  call 006B63C0
006B416C  mov bl, 0x01           ; earlier in the same fn
006B42CE  mov [ebp+68], bl       ; +68 = 1
```

C# `SeedHero`: `ComputePose`; `SlotB = SlotA`; `Seeded = true`.

`SlotB = SlotA` is **EQUIVALENT** to the trailing bank copy
when unused banks stay ctor-zero. C# does not write `+3084`
from env+8. First-seen value of that float is **UNREAD**.
If it is not 0, C# dirs are invented zeros.

`SeedAt` writes `V0=position`, `V1=lookAt`, `V2=up`. That is
**not** `006B3FF0`. Production `EngineLifecycle` never calls
it. Only `World_camera_006B4900_slots_lerp_into_ScriptedCamera`
does. **DISPROVEN** as New Game / Leave.

### 2.3 Pose / spring / tick

| VA | Dump first-seen | C# | Class |
| --- | --- | --- | --- |
| `006B2CA0` | `+61!=0` skip; `+3084` / pitch / `[0x122DEDC]=0` → dirs `(1,0,0)`; `+412=0` → V4 `(−1,0,0)`; `00A14440` | hard-coded those three vectors | **EQUIVALENT** first-seen; live trig **PARTIAL** |
| `006B3030` | `+3168==0`; LCG seed **UNREAD**; no yaw rotate; weight stays 0.2 | clamp weight only | **EQUIVALENT** first-seen |
| `006B3B80` | `+460==0` and `+24==−1` → ret | `CameraTickSkipped=true` | **PROVEN** skip |

### 2.4 Apply `0049E080` / `006B42F0` vs `Blend`

Dump `0049E080`:

```
0049E08A  call 004C74F0
0049E097  call 0051EBD0
0049E09F  mov ecx, [esi+24]
0049E0A3  call 006B42F0
```

`006B42F0`: if `+68==0` call `006B3FF0`; clamp t to `[0,1]`;
lerp B→A into `+6292/+6296/+6308/+6312/+6328/+6340/+6352`;
`00A14440` on the three dir slots.

```
006B42FA  mov al, [ebx+68]
006B4301  jne 006B430A
006B4305  call 006B3FF0
006B451F  lea edx, [ebx+6296]
006B4525  fmul [ebx+3092]        ; t * SlotA.V0
006B454D  fmul [ebx+6196]        ; (1-t) * SlotB.V0
```

Lerp offsets match `CameraSlot.Lerp`. Math **PROVEN**.
Semantic of `+6296` as eye **DISPROVEN** first-seen.

C# `Blend` when `!Seeded` only flips `Seeded`; it does **not**
call `SeedHero`. After ctor, SlotB is `Zero()`, so `Blend(0)`
returns `(0,0,0)`. The unit test **asserts** that Zero. Native
first `006B42F0` with `+68==0` would seed first (A→B) so t=0
stays ctor `(1,0,0)`. Live `TickWorld` / `SpawnHero` call
`SeedHero` first, so the broken path is not the first Game
apply. Still a **DIVERGE** in `WorldCamera.cs`: `Seeded` is
not `+68`, and `Blend` is not `006B42F0`.

`ApplyWorldCamera` only **Notes** `006B3FF0` when `!Seeded`;
it does not call `SeedHero`. Relying on `TickWorld` order is
host scheduling, not the function.

`00417001`:

```
00417042  call 0049D870          ; WorldFrame
00417047  cmp eax, 1
0041704A  jle 0041725F           ; skip camera body and Present
0041707E  … 0049E080
```

First-seen t after clamp is 0. After seed A=B so t does not
matter. Extra host `ApplyWorldCamera(1f)` at `SpawnHero` is
**PARTIAL** timing, not a second matrix if A=B.

### 2.5 Tail is not the 3D helper

After the lerp and three `00A14440`:

```
006B482D  mov ecx, [ebx+6496]
006B483C  call 008857E0
006B485C  call 00885900
006B4889  call 008859F0
```

then engine `vtbl+244` = `00B23EC0` into colour-filter
`[0x1436E40]+16` when `+12==0`. **DISPROVEN** as a writer of
`[0x1436EA0]+12` or helper `+0/+12/+24`
(`2026-08-18-camera.md` §1.2–1.3).

---

## 3. What the first 3D Present actually consumes

`WorldCamera.cs` does not build WVP. Consume path
(`00B314E0` / `00B30B50` / `00988A50`) reads a **helper**:

```
helper +0  eye
helper +12 look dir   (not look-at)
helper +24 up
helper +44 FOV turns
```

`00B2FBF0` stores that pointer at camera `+12`. Bind is
engine vtbl+16 `00B23B50` (zero `E8`; vtbl / script). First
no-save `UseCamera` is **DISPROVEN**. Which object is
`[0x1436EA0]+12` on the first nonempty 3D Present is still
**UNREAD**.

Recovered ctor helpers vs host live bind:

| Source | pos | look / forward | up | FOV |
| --- | --- | --- | --- | --- |
| GameCamera `00A0C130` | `(0,0,0)` | `+Z` | `(1,1,1)` | `0x3E471B48` turns ≈ 70° |
| GameCameraManager | `(0,0,0)` | `+Z` | `+X` | same 70° turns |
| WorldCamera `+6296` first-seen | ctor axis `(1,0,0)` | not an eye | V2 pose `(1,0,0)` | none |
| Host `ApplyRendererHelper` | Hero XYZ | pose `V4=(−1,0,0)` | `(0,0,1)` | 70° |

P / letterbox / `CotScaledView` algebra (`009883F0` /
`00B30B50` at 1024×768) is **EQUIVALENT**. The **inputs**
are host policy. Do not call them recovered WorldCamera
matrices.

`ApplyWorldCamera` **replaces** `006B42F0` output when
`IsCtorAxis(V0)` with hero + V4 + `FirstSeenCameraUp`. That
swap is not in `WorldCamera.cs` and not in `006B42F0`.
`Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`
asserts `RendererHelperBound`, Lookout eye, up `(0,0,1)`,
FOV 70. That is host first-Present policy after spawn, not
the PE helper dump.

First nonempty 3D DIP is after `00501450` / maps
(`00BDC060` / `00BF4570` / `00BB2540`) — `dx9-3d-submit`.
WorldCamera apply can run on WorldFrame>1 **before** that
DIP if `004AEA70` later opens Present with an empty dest.

---

## Classifications (short)

1. **Frontend 3D WorldCamera — DISPROVEN.** Leave /
   `0042DF9E` never call `006B4900`. C# `Construct()` is
   after Leave. `ScriptedCamera` + FOV 72 on frontend
   frames is **LEFTOVER**.
2. **First 3D camera object after Leave — WorldCamera
   `006B4900` then seed `006B3FF0` then apply
   `0049E080`/`006B42F0`. PROVEN.** Scene is Lookout, not
   Oakvale.
3. **First-seen slot/pose/lerp numbers in `WorldCamera.cs`
   — PROVEN / EQUIVALENT** against `006B4900` / zero-angle
   `006B2CA0` / `006B42F0` offsets. `+6296` is an axis.
4. **Invented 1.6 m `SeedAt` — DISPROVEN on live path.**
   Method remains a test helper. Do not restore it.
5. **`Blend` marking `Seeded` without `006B3FF0` —
   DIVERGE** vs dump. Live `SeedHero` hides it.
6. **Matrices the renderer consumes — not in this file.**
   Tail is colour-filter. Host hero+V4+70° is **DIVERGE** vs
   ctor helpers; live helper pointer **UNREAD**.
7. **env+8 → `+3084` before pose — UNREAD** first-seen
   value. C# assumes 0.

Do not add `CreateLookAt`, baked Y in P, SHOT2/72 as first
Present, or a 1.6 m eye to “fix” the picture.
