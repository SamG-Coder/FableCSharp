# Audit: `WorldCamera.cs` vs dump after Leave

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `CAM_OVIF_SHOT2` / `00DBDE40`.
That path is later `Q_NewOakValeIntro`, not Leave / Init World /
first no-save Present.

Authority: PE listings
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
(`0042EC7C` / `0042F2A2`) and `listing-00680000.txt`
(`006B2CA0` / `006B3FF0` / `006B42F0` / `006B4900` / `006BBC30`);
`listing-00480000.txt` (`0049E080` / `004A5DF3` / `004A6F50`);
`src/Fable.Game/WorldCamera.cs`;
`src/Fable.Game/EngineLifecycle.cs`
(`PumpFrontendFrame` / `InitWorldCameras` / `TickWorld` /
`ApplyWorldCamera` / `SpawnHero`);
`src/Fable.Game/ScriptedCamera.cs`;
`docs/runtime/FORWARD_TREE.md` §§4, 7, 11;
`docs/status/investigations/B-camera-matrices.md`;
`docs/status/investigations/2026-08-18-camera.md`;
`proofs/camera-after-leave/README.md`;
`CameraMatrixParityTests`;
`EngineLifecycleTests.World_camera_006B4900_slots_lerp_into_ScriptedCamera`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

---

## Verdict

**No WorldCamera during frontend. `WorldCamera.cs` does not invent
a 1.6 m `SeedAt` eye on the live New Game path. The live WVP
inputs after Leave are still a host policy, not a dump of the
render helper.**

| Claim | Status |
| --- | --- |
| `0042EC7C` / `0042F2A2` construct or tick WorldCamera | **DISPROVEN** |
| WorldCamera lives only after Init World `004A6E30` → `004A6F67 006B4900` | **PROVEN** |
| First seed is `004A5DF3 006B3FF0` (`TickWorld`), not frontend | **PROVEN** |
| First apply is `0041707E` → `0049E080` → `006B42F0` after WorldFrame>1 | **PROVEN** |
| Ctor axes `+3092/+3108=(1,0,0)`, weights `0.2`, timer `+24=−1`, `+61=0`, `+68=0` | **PROVEN** vs `006B4900` |
| First-seen pose dirs V2/V3 `(1,0,0)`, V4 `(−1,0,0)` when yaw/pitch 0 | **PROVEN** as `006B2CA0` with `[0x122DEDC]=0` |
| `+6296` first-seen is the ctor axis, not an eye | **PROVEN** |
| Host `SeedAt(1.6 m)` on New Game / Leave | **DISPROVEN** (method leftover; tests only) |
| `WorldCamera.Blend` without `006B3FF0` is native first `006B42F0` | **DISPROVEN** (`+68==0` must seed; C# only flips `Seeded`) |
| Host `ApplyRendererHelper(hero, V4, +Z, 70°)` is the PE helper | **DISPROVEN** as a dump. **DIVERGE** vs both ctor helpers |
| `006B42F0` tail writes the render helper / WVP | **DISPROVEN** (`008857E0` + vtbl+244 = colour filter `00B23EC0`) |
| C# `ScriptedCamera` + FOV 72 on frontend `EngineFrame` | **LEFTOVER** |

---

## 1. Camera during frontend?

Dump `0042EC7C` (retail frontend pump) is AVI strings, QPC,
`0042E3EE` input, `0042DF9E` 2D flush, `009BEEB0` Present.
No `E8 006B4900` / `006B3FF0` / `006B42F0` / `0049E080`.

Dump `0042F2A2` Leave is fade `vtbl+72(0x1F4)`, optional bank
swaps when `[0x13B8616]!=0`, then `FinalAlbion.wld` later at
`0042F44D`. No WorldCamera alloc.

WorldCamera appears only in Init World:

```
004A6F50  push 0x1970
004A6F55  call 00BFEA1A
004A6F67  call 006B4900
004A6F75  mov [esi+24], eax      ; world+24
```

That site is `004A6E30`, reached from `004184BD` **after** Leave
`0042F491` Init Game. **PROVEN**.

C# matches that gate if `WorldCameraPresent` is the object flag:
`Construct()` runs in `InitWorldCameras` from `EnterGame`, not
from `PumpFrontendFrame`.

Host leftovers on frontend frames:

| Site | What | Class |
| --- | --- | --- |
| `EngineLifecycle.WorldCamera = new()` at field init | C# object exists; `Construct()` not yet | **LEFTOVER** host |
| `Camera = new ScriptedCamera()` FOV `IntroCameraFovDegrees=72` | every `BuildFrame()` carries it | **LEFTOVER**. 72 is SHOT2 |
| `SilkEngineHost.Draw` reads `_frame.Camera` | 3D WVP only if verts exist | **LEFTOVER** on frontend (batch path; verts empty) |

**Answer:** no WorldCamera / GameCamera / apply during frontend.
Native Present is 2D UI + AVI texture. The DX9 render-cam object
`[0x1436EA0]` exists from Init Engine with helper `+12=0` — that
is not `WorldCamera.cs`.

---

## 2. Invented matrices?

Three different inventions show up in this file. Only one was
ever claimed as the New Game eye.

### 2.1 `SeedAt` 1.6 m eye — leftover, not live

`WorldCamera.SeedAt` writes `V0=position`, `V1=lookAt`, `V2=up`
and copies A→B. That is **not** `006B3FF0`. Native seed writes
`+3084` from the pushed arg, walks `008889C0`, then `006B2CA0`
(angles → dirs). It does not store a world eye in `+3092`.

Production `EngineLifecycle` never calls `SeedAt`. The only
`SeedAt(` in the tree is
`EngineLifecycleTests.World_camera_006B4900_slots_lerp_into_ScriptedCamera`.
Live path is `SeedHero` → `ComputePose`. **DISPROVEN** as New
Game / Leave.

### 2.2 `Blend` without seed — invented lerp from Zero

Native `006B42F0`:

```
006B42FA  mov al, [ebx+68]
006B42FF  jne 006B430A
006B4303  push 0
006B4305  call 006B3FF0          ; then clamp t, lerp B→A
```

C# `Blend`:

```
if (!Seeded) Seeded = true;      // no SeedHero / ComputePose
Output = CameraSlot.Lerp(SlotB, SlotA, t);
```

After ctor, SlotB is `Zero()`. `Blend(0)` therefore returns
`(0,0,0)`. The unit test **asserts** that Zero. Native first
`006B42F0` would have copied A→B (`006B63C0` + trailing
`+3084…` → `+6188…`) so t=0 stays ctor `(1,0,0)`.

Live `TickWorld` / `SpawnHero` call `SeedHero` first, so the
broken `Blend` path is not the first Game apply. Still a
**DIVERGE** in `WorldCamera.cs` vs the dump: `Seeded` is not
`+68`, and `Blend` is not `006B42F0`.

`ApplyWorldCamera` only **Notes** `006B3FF0` when `!Seeded`;
it does not call `SeedHero`. Relying on `TickWorld` order is
host scheduling, not the function.

### 2.3 Host helper-bind (hero + V4 + Z-up + 70°) — invented consume

`006B42F0` after the `+6296/+6312/+6328/+6340/+6352` lerp:

```
008857E0 / 00885900 / 008859F0   ; bank on [this+6496]
call [engine.vtbl+244]           ; 012A0F3C+244 = 00B23EC0
```

`00B23EC0` copies an 8-float packet into colour-filter
`[0x1436E40]+16` when `+12==0`. **DISPROVEN** as a WVP / helper
writer (`2026-08-18-camera.md` §1.2–1.3).

The consumed first-Present helper (`[0x1436EA0]+12`) is still
**UNREAD**. Recovered ctor helpers are **not** the host bind:

| Source | pos | look / forward | up | FOV |
| --- | --- | --- | --- | --- |
| GameCamera `00A0C130` | `(0,0,0)` | `+Z` | `(1,1,1)` | `0x3E471B48` turns ≈ 70° |
| GameCameraManager | `(0,0,0)` | `+Z` | `+X` | same 70° turns |
| WorldCamera `+6296` first-seen | ctor axis `(1,0,0)` | not an eye | V2 pose `(1,0,0)` | none |
| Host `ApplyRendererHelper` | Hero XYZ | pose `V4=(−1,0,0)` | `(0,0,1)` | 70° |

P / letterbox / `CotScaledView` algebra (`009883F0` /
`00B30B50` at 1024×768) is **EQUIVALENT**. The **inputs** to
that algebra are host policy. Do not call them recovered
WorldCamera matrices.

`ComputePose` hard-codes the zero-angle result instead of the
`006B2CA0` sin/cos. **EQUIVALENT** first-seen **if** `+3084`
and `+424…+444` stay 0 after seed. Dump `006B3FF0` stores the
pushed arg at `+3084` before pose; `004A5DF3` pushes
`[environment+8]`. Env ctor `006BBC30` writes `+8` from
`[eax+180] * [0x122DC88]` (time-of-day scale). First-seen
value of that float is **UNREAD** here. If it is not 0, C#
dirs are invented zeros.

---

## 3. `WorldCamera.cs` vs dump (field / control)

### 3.1 Ctor `006B4900` — match on the tracked fields

| Dump | C# `Construct` / consts | Class |
| --- | --- | --- |
| vtbl `0125D53C`, size `0x1970`, world+24 | `Vtbl` / `ObjectSize` / `WorldOffset` | **PROVEN** |
| six `008864A0` at +84 and six at +3188, stride `0x1F4` | noted in comment; not stored | leftover bank |
| `fld qword [0x1236700]` → `+24 = −1.0` | `TickTimerDefault` | **PROVEN** |
| `+3092/+3108 = (1,0,0)` | `SlotA.V0/V1` | **PROVEN** |
| `+3088/+3104 = 0x3E4CCCCD` (0.2) | `DefaultWeight` | **PROVEN** |
| `+3084 = 0`, `+61 = 0`, `+68 = 0` | `Param=0`, `PoseSkipFlag=false`, `Seeded=false` | **PROVEN** |
| `+6496` `008852E0`, `+6500` `006B84B0` | not modelled | leftover (colour-filter bank) |

`IsCtorAxis` is the right label for `+3092` / first-seen
`+6296`. Those vectors are **not** Lookout eyes.

### 3.2 Seed `006B3FF0` vs `SeedHero`

Native order (dump): `006B63C0(+84→+3188)`, `006B8640(+6500)`,
`+3084 = arg`, 6× `008889C0`, then `006B2CA0` / `006B3030` /
`006B3B80`, then if `+68==0` copy banks again and `+68=1`.

C# `SeedHero`: `ComputePose`; `SlotB = SlotA`; `Seeded = true`.

`SlotB = SlotA` is **EQUIVALENT** to the trailing copy
(`+3084…` sits after the six `0x1F4` slots at +84, dest
`+6188…`) when the unused banks stay ctor-zero. The list walk
and `006B8640` do not write V0 first-seen (**UNREAD leftover**,
FORWARD_TREE §11).

C# does not model `+68` separately from `Seeded`, and does not
write `+3084` from env+8.

### 3.3 Pose / spring / tick

| VA | Dump first-seen | C# | Class |
| --- | --- | --- | --- |
| `006B2CA0` | `+61!=0` skip; zero angles + `[0x122DEDC]=0` → dirs `(1,0,0)`; `+412=0` → V4 `(−1,0,0)` at +3144; `00A14440` | hard-coded those three vectors | **EQUIVALENT** first-seen; live trig **PARTIAL** |
| `006B3030` | `+3168==0`; LCG seed **UNREAD**; no yaw rotate; weight stays 0.2; V0 stays ctor | clamp weight only | **EQUIVALENT** first-seen |
| `006B3B80` | `+460==0` and `+24==−1` → `006B3E59` ret | `CameraTickSkipped=true` | **PROVEN** skip |

### 3.4 Apply `0049E080` / `006B42F0` vs `Blend` + `ApplyWorldCamera`

Dump `0049E080`: `004C74F0`, `0051EBD0`,
`006B42F0([esi+24], [record+4])`. Debug `[0x13B8394]` optional.

Lerp offsets match `CameraSlot.Lerp`:

- `+6292` ← `(1−t)*+6188 + t*+3084`
- `+6296` ← V0 pair `+6196` / `+3092`
- `+6308` ← weights `+6192` / `+3088`
- `+6312` / `+6328` / `+6340` / `+6352` ← V1…V4
- then `00A14440` on the three dir slots

Math **PROVEN**. Semantic of `+6296` as eye **DISPROVEN**
first-seen.

`ApplyWorldCamera` then **replaces** that output when
`IsCtorAxis(V0)` with hero + V4 + `FirstSeenCameraUp`. That is
not in `WorldCamera.cs` and not in `006B42F0`.

`SpawnHero` also calls `ApplyWorldCamera(1f)` at TNG insert.
Native first `0041707E` uses `t = clamp(DisplayTime*15 −
[game+72], 0, 1)`; default ticks 0 → **t=0**. After seed A=B
so t does not matter. Extra host apply at spawn is
**PARTIAL** timing, not a second matrix if A=B.

---

## 4. After Leave — what actually runs

```
0042F2A2 Leave frontend          // no WorldCamera
0042F491 Init Game
  004A6E30 Init World
    006B4900 WorldCamera world+24
    0069AE80 GameCameraManager world+48
    006FD8C0 GameCamera world+44
  00416953 FinalAlbion.wld
004189C2 GamePump
  WorldFrame 0→1: 004A5DF3 006B3FF0
  WorldFrame<=1: skip 0049E080 and 00435F70
  WorldFrame>1: 0041707E → 0049E080 → 006B42F0
```

C# `InitWorldCameras` / `TickWorld.SeedHero` / `ApplyCameraInterpolation`
follow that order. `SeedWorldTick` does **not** seed the camera
(empty type-1 queue). First seed is the first `TickWorld`.

---

## Classifications (short)

1. **Frontend WorldCamera — DISPROVEN.** Dump Leave / retail
   pump never call `006B4900`. C# `Construct()` is after Leave.
   `ScriptedCamera` on frontend frames is **LEFTOVER**.
2. **Invented 1.6 m `SeedAt` — DISPROVEN on live path.**
   Method remains a test helper. Do not restore it.
3. **`Blend` marking `Seeded` without `006B3FF0` — DIVERGE**
   vs dump. Live `SeedHero` hides it. Test `Blend(0)==Zero`
   documents the wrong function.
4. **First-seen slot/pose/lerp numbers in `WorldCamera.cs` —
   PROVEN / EQUIVALENT** against `006B4900` / zero-angle
   `006B2CA0` / `006B42F0` offsets. `+6296` is an axis.
5. **Matrices the renderer consumes — not in this file.**
   Tail is colour-filter. Host hero+V4+70° is **DIVERGE** vs
   ctor helpers; live helper pointer **UNREAD**. P/V algebra
   elsewhere is **EQUIVALENT**.
6. **env+8 → `+3084` before pose — UNREAD** first-seen value.
   C# assumes 0.

Do not add `CreateLookAt`, baked Y in P, SHOT2/72 as first
Present, or a 1.6 m eye to “fix” the picture.
