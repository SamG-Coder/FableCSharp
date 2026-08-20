# First `00B46C80` / TOD `+224` submit after Leave

Investigation only. Production `src/` was not edited.

Do **not** start at Oakvale / `CAM_OVIF_SHOT2` /
`ENVIRONMENT_OAKVALE` / invented ambient
`(0.52, 0.58, 0.68)` / fog end **7000**.
Those are leftover `Q_NewOakValeIntro` or SKY_DEF slack,
not Leave / first no-save 3D Present.

Question: after no-save Leave, what is the first
`00B46C80` / TOD `+224` **consume**, and which **map**
does the first fog **submit** (setup + `oFog` DIP)?
Host leftover vs skip?

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Authority: dump `00B46C80`
(`tools/Fable.ExeIndex/out/01-sections/newgame-trace/tod-blend-00b46c80-00b46c80.md`,
`lighting-time-of-day-blend-00b46c80.md`,
`lighting-mgr-ctor-defaults-00b482a0.md`);
`implementer/stars/fn-00B46C80.txt` /
`fn-00B46E17-exact.txt`;
`landscape-trace/shared-lighting-setup-00b67480.md`,
`landscape-draw-vtbl16-00b6b0b0.md`,
`fg-compactbind-00b68da0.md`.
Siblings: `proofs/fog-after-leave/` (Leave spine + payload),
`proofs/fog-lighting-3d/` (ctor + frontend flush),
`proofs/landscape-submit-leave/` (first LEV DIP map),
`proofs/c3d-first-submit/`, `proofs/wld-first-region/`.

---

## Verdict

**First fog *submit* after Leave is LookoutPoint.**
Not Oakvale. Not frontend. Not Leave Present.

`00B46C80` is TOD **setup**, not a DIP. After Leave the
bytes are still ctor **0**. `+226==0` takes
`jbe 00B46E17` and copies lighting **record `[+224]=0`**.
That setup first *matters* on the first nonempty landscape
walk after Lookout STB attach. The first `oFog` DIP on that
walk is bit `0x40` `00BF4570` (LEV cell), then bit `0x20`
static `00BB2540`.

| What | Native | Status |
|---|---|---|
| TOD `+224/+225/+226` after Leave | **0** (ctor `00B482A0` `mov [esi+224], bl`) | **PROVEN** |
| `00B46C80` first-seen path | `+226==0` → `00B46E17` copy record `[+224]=0` (112-byte stride) | **PROVEN** |
| First *setup* after Leave + maps | land bits `0x4` / `0x40`: `00B67480` = `00B46C80` + `00B46890` | **PROVEN** |
| First fog **submit map** | **`LookoutPoint`** (WLD `NewMap 1` / `NewRegion 1`) | **PROVEN** |
| First `oFog` DIP | Lookout `00BF4570` (bit `0x40`) then static `00BB2540` (bit `0x20`) | **PROVEN** as DIP; first-frame capture **PARTIAL** |
| Oakvale / `StartOakVale` / house 6909 | later WLD `NewRegion 4` | **DISPROVEN** as this submit |
| Host `SilkEngineHost.Draw` plane on empty frames | always `LinearFogPlane(_frame.Camera)` | **LEFTOVER** |
| Host mesh / fog push | `_meshCount==0` → skip | **PROVEN** skip (matches dest-empty) |

Leave itself does not construct, copy, or DIP lighting.
**PROVEN** (`0042F2A2` is audio/UI teardown + path write).

---

## Recovered order (no-save New Game)

```
0042EC7C retail
  Init Engine 0042E204
    00B26360  alloc 0x46D0 → ctor 00B482A0
      [esi+224/+225/+226] = 0
      [esi+228] = 0xF
      record 0: c20=0.25, fog (0,0,0,1), start/end 1000/2000
      store [0x1436E9C]
  0042DF9E  2D UI
    0042E0BB → 00B27D90 → 00B6B0B0
      bit 4 still 00B67480 → 00B46C80     ; setup, empty +44, no DIP
      009DA9F0 VSHADER_2D_SPRITE          ; no oFog
0042F2A2 Leave
  009BE420 + 009BEEB0                     ; black; no 00B46C80
0042F491 Init Game → 00416953 FinalAlbion.wld
  00507C30  first NewRegion = LookoutPoint
  004A1BD3  Set Static Map → 00B428E0
    FinalAlbion.stb MISS                  ; +44 still empty
004189C2 first pumps
  WorldFrame<=1: skip 00435530
  first 00435530 often empty dest         ; skip 3D consume
later 00501450(1) LookoutPoint
  006C2170 / 0051FD80 / 006AC910
  later STB hit → +44 nonempty
next 00B27D90 → 00B6B0B0
  bit 0x4:  00B6B1CC 00B67480
              00B46C80  TOD +224=0 copy record 0
              00B46890  FOGENABLE=1
            then 00BDC060 → 00BF71D0     ; BG patch (no oFog VS)
  bit 0x40: 00B68DA0 also 00B46C80
            00B6B177 00B67480 same copy
            then 00BDC2D0 → 00BF4570     ; first oFog LEV DIP
  bit 0x20: 00B32AD0 FOGENABLE; 00BB2540 ; first static oFog DIP
```

`00B46C80` during frontend bit-4 is **not** the first fog
submit. Empty `+44` → no `00BF4570`. After Leave the first
dummy Present is also a **skip**. First *submit* is Lookout.

---

## 1. `00B46C80` / TOD `+224` (dump)

`this` = lighting manager `[0x1436E9C]`.

```
00B46C86  mov al, [esi+226]
00B46C93  test [esi+228], 0x01
00B46CB0  test al, al
00B46CB6  jbe 00B46E17          ; first-seen: +226==0
00B46E17  movzx ecx, [esi+224]
00B46E1E  imul ecx, ecx, 112
00B46E21  add eax, ecx          ; eax = [esi+60] + record
          copy 16 bytes → [esi+72]     colour
          copy 16 bytes → [esi+104]    c35 path
00B46EE3  call 00B49950
00B46EF5  call 00989830(0)      ; dirlight
          then 0098B2C0         ; c35 default
```

When `+226 != 0` the lerp uses `[+224]` and `[+225]` as
112-byte record indices. First-seen never takes that arm.

Ctor (`00B482A0`):

| Byte | Store | First-seen |
|---|---|---|
| `+224` | `00B48316 mov [esi+224], bl` | **0** |
| `+225` | `00B4831C` | **0** |
| `+226` | `00B48322` | **0** |
| `+228` | `00B4849D mov [esi+228], 0xF` | dirty; bit 0 lets `00B46C80` run |

No first-seen `SetTime` / theme unpack writes these bytes
before Lookout Present. **PROVEN** absence on this path.
Theme 269-byte TOD blob is **DISPROVEN** as the live record
(no 1000/2000 immediates).

`00B67480` listing:

```
00B67483  mov ecx, [0x1436E9C]
00B67489  call 00B46C80
00B6748E  mov ecx, [0x1436E9C]
00B67494  call 00B46890          ; FOGENABLE slot +10568 = 1
00B67503  call 009881F0          ; identity 3×4 on 0x1436E14
```

Bit `0x4` calls it at `00B6B1CC` **before** the `+44`
sentinel walk. Bit `0x40` calls `00B68DA0` (another
`00B46C80` at `00B68F19`) then `00B6B177 00B67480`, gated
on `[renderer+1552]`.

---

## 2. First fog submit map (not Oakvale)

Same map as first stored-cell LEV DIP
(`proofs/landscape-submit-leave`).

| Claim | Class |
|---|---|
| First WLD `RegionName` / `NewMap` is `LookoutPoint` | **PROVEN** (`wld-first-region`) |
| First `00501450` after dummy is index **1** Lookout | **PROVEN** |
| First nonempty `+44` walk is that region’s STB | **PROVEN** as region; first patch on list **PARTIAL** |
| First `oFog` land DIP is Lookout `00BF4570` bit `0x40` | **PROVEN** as site |
| First `oFog` static DIP is Lookout slot-0 `00BB2540` | **PROVEN** as site |
| First map is `StartOakVale` / house 6909 | **DISPROVEN** (`NewRegion 4`; house is bit `0x20` C3D) |

Payload on that Present is still ctor record 0:

| Item | Value | Class |
|---|---|---|
| `c19` | `(0, 1, 0, 0)` | **PROVEN** |
| `c20` | `(0.25, 0.25, 0.25, 1)` | **PROVEN** |
| `FOGCOLOR` | `0xFF000000` | **PROVEN** |
| start / end | **1000 / 2000** | **PROVEN** |
| packed count | **0** → `*_DIRLIGHT_FOG` | **PROVEN** |
| 7000 / Oakvale RGB | SKY_DEF / intro leftover | **DISPROVEN** |

---

## 3. Host leftover vs skip

Native skip sites (no `00B46C80` *submit*):

| Site | What | Class |
|---|---|---|
| Frontend 2D `009DA9F0` | sprites; no `oFog` | **PROVEN** skip of 3D fog draw |
| Empty `+44` bit-4 `00B67480` | setup may run; no DIP | **PROVEN** unused FOGENABLE |
| Leave Present | `009BE420` black | **PROVEN** skip |
| `WorldFrame<=1` | skip `00435530` | **PROVEN** |
| First `00435530` empty dest | no `E8 00B25950` | **PROVEN** skip of 3D consume |

Host:

| Site | Behaviour | Class |
|---|---|---|
| `SubmitCurrentWorld` | `return` unless `HeroSpawned` + region | **PROVEN** skip until Lookout spawn |
| `VulkanLineRenderer.SetMesh` | `_meshCount==0` → return | **PROVEN** skip |
| `Record` mesh pass | skip unless `_meshCount>0` (or objects) | **PROVEN** skip |
| `SilkEngineHost.Draw` | `LinearFogPlane(cam.Position, cam.Forward)` whenever `_frame.Camera` is set | **LEFTOVER** on frontend / dest-empty / pre-hero frames |
| Mesh push `CameraPos` | host field is the plane, not eye | **EQUIVALENT** *if* verts exist |
| `WorldShading` statics | ctor record 0 | **EQUIVALENT** payload, not a runtime apply |
| `AuthoredEnvironmentTheme` | stores `REGION.EnvironmentTheme` name | **PROVEN** not applied |
| `RequestNewGame` / `PumpFrontendFrame` | no `00B46C80` / FOGENABLE | **PROVEN** absence |

So: host **leftover** is computing the fog plane on frames
native would skip. Host **skip** is the mesh push
(`_meshCount==0` / `!HeroSpawned`), which matches native
empty dest. After Lookout verts exist the numbers are
**EQUIVALENT**; Concat land+C3D+sky is still **DISPROVEN**
as native DIP boundaries (`landscape-submit-leave`,
`c3d-first-submit`).

Do not “fix” frontend by binding 3D fog. Do not wait for
Oakvale theme. First submit is Lookout + record 0.

---

## Classifications (short)

1. **First `00B46C80` after Leave that submits — Lookout
   `00B67480` on nonempty `00B27D90`.** TOD `+224=0` copy
   record 0. **PROVEN.** Frontend bit-4 call is setup only.
2. **First fog submit map — `LookoutPoint`.** First `oFog`
   DIP `00BF4570` then `00BB2540`. Oakvale **DISPROVEN**.
3. **Host leftover vs skip — leftover plane on empty
   frames; skip of mesh push until `HeroSpawned`.** After
   Lookout verts, constants **EQUIVALENT**.
