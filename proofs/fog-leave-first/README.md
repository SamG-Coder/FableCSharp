# First `00B46C80` after Leave

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `CAM_OVIF_SHOT2` /
`ENVIRONMENT_OAKVALE` / invented ambient
`(0.52, 0.58, 0.68)` / fog end **7000**.
Those are leftover `Q_NewOakValeIntro` or SKY_DEF slack,
not Leave / first no-save 3D Present.

Question: first `00B46C80` after Leave. Host skip leftover?
First map?

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Authority: `proofs/fog-after-leave/`,
`proofs/fog-first-submit-leave/`.
Dumps: `tod-blend-00b46c80-00b46c80.md`,
`lighting-time-of-day-blend-00b46c80.md`,
`lighting-mgr-ctor-defaults-00b482a0.md`,
`landscape-trace/shared-lighting-setup-00b67480.md`,
`landscape-draw-vtbl16-00b6b0b0.md`,
`fg-compactbind-00b68da0.md`,
`implementer/stars/fn-00B46C80.txt` /
`fn-00B46E17-exact.txt`.
Siblings: `fog-lighting-3d/`, `landscape-submit-leave/`,
`wld-first-region/`, `dummy-pumps-before-region/`.

---

## Verdict

**First `00B46C80` after Leave that *submits* is LookoutPoint
`00B67480` on the first nonempty `00B27D90`.** Not Oakvale.
Not Leave Present. Not frontend.

`00B46C80` is TOD **setup**, not a DIP. After Leave the
bytes are still ctor **0**. `+226==0` takes
`jbe 00B46E17` and copies lighting **record `[+224]=0`**.
That copy first *matters* after Lookout STB attach fills
`+44`. First `oFog` DIP on that walk is bit `0x40`
`00BF4570`, then bit `0x20` static `00BB2540`.

Host **leftover** is the fog *plane* on frames native
would skip. Host **skip** is the mesh push until
`HeroSpawned` / `_meshCount>0`. First map is
**LookoutPoint**.

| Question | Answer | Class |
|---|---|---|
| First `00B46C80` *after Leave* that submits? | Lookout `00B67480` bits `0x4` / `0x40` on nonempty `00B27D90` | **PROVEN** |
| Same call on Leave Present? | no (`009BE420` + `009BEEB0` only) | **PROVEN** skip |
| Frontend bit-4 `00B46C80`? | **before** Leave; empty `+44`; unused FOGENABLE | **DISPROVEN** as this site |
| Empty dest after Leave? | `WorldFrame<=1` skip `00435530`; first `00435530` no `E8 00B25950` | **PROVEN** skip of 3D consume |
| Game `00B27D90` after Leave, dest still empty? | would still `00B6B1CC 00B67480` | **UNREAD** caller; unused setup if it runs |
| First **map**? | **`LookoutPoint`** (WLD `NewMap 1` / `NewRegion 1`) | **PROVEN** |
| Oakvale / `StartOakVale` / house 6909? | later `NewRegion 4`; house is bit `0x20` C3D | **DISPROVEN** |
| Host leftover? | `SilkEngineHost.Draw` `LinearFogPlane` whenever `_frame.Camera` is set | **LEFTOVER** on dest-empty / pre-hero |
| Host skip? | `SubmitCurrentWorld` unless `HeroSpawned`; `SetMesh` / `Record` if `_meshCount==0` | **PROVEN** skip (matches empty dest) |

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
      bit 4 00B67480 → 00B46C80     ; BEFORE Leave; empty +44; no DIP
      009DA9F0 VSHADER_2D_SPRITE    ; no oFog
0042F2A2 Leave
  009BE420 + 009BEEB0               ; black; no 00B46C80
0042F491 Init Game → 00416953 FinalAlbion.wld
  00507C30  first NewRegion = LookoutPoint
  004A1BD3  Set Static Map → 00B428E0
    FinalAlbion.stb MISS            ; +44 still empty
004189C2 first pumps
  WorldFrame<=1: skip 00435530
  first 00435530 often empty dest   ; skip 3D consume
later 00501450(1) LookoutPoint
  006C2170 / 0051FD80 / 006AC910
  later STB hit → +44 nonempty
next 00B27D90 → 00B6B0B0            ; FIRST 00B46C80 after Leave that submits
  bit 0x4:  00B6B1CC 00B67480
              00B46C80  TOD +224=0 copy record 0
              00B46890  FOGENABLE=1
            then 00BDC060 → 00BF71D0     ; BG patch (no oFog VS)
  bit 0x40: 00B68DA0  00B68F19 00B46C80
            00B6B177 00B67480 same copy
            then 00BDC2D0 → 00BF4570     ; first oFog LEV DIP
  bit 0x20: 00B32AD0 FOGENABLE; 00BB2540 ; first static oFog DIP
```

`00B46C80` during frontend bit-4 is **not** after Leave.
Leave Present is a **skip**. Dummy dest-empty is a **skip**.
First *submit* is Lookout.

---

## 1. First `00B46C80` after Leave (dump)

`this` = lighting manager `[0x1436E9C]`. First-seen path
is copy, not lerp.

```
00B46C86  mov al, [esi+226]
00B46C93  test [esi+228], 0x01
00B46CB0  test al, al
00B46CB6  jbe 00B46E17          ; first-seen: +226==0
00B46E17  movzx ecx, [esi+224]
00B46E1E  imul ecx, ecx, 112
00B46E21  add eax, ecx          ; [esi+60] + record
          copy 16 bytes → [esi+72]     colour
          copy 16 bytes → [esi+104]    c35 path
00B46EE3  call 00B49950
00B46EF5  call 00989830(0)
          then 0098B2C0
```

Ctor (`00B482A0`, Init Engine, **before** Leave):

| Byte | Store | After Leave |
|---|---|---|
| `+224` | `00B48316 mov [esi+224], bl` | **0** |
| `+225` | `00B4831C` | **0** |
| `+226` | `00B48322` | **0** |
| `+228` | `00B4849D mov [esi+228], 0xF` | dirty; bit 0 lets `00B46C80` run |

No first-seen `SetTime` / theme unpack writes these bytes
before Lookout Present. **PROVEN** absence. Theme 269-byte
TOD blob is **DISPROVEN** as the live record (no 1000/2000).

Caller that first *submits* after Leave:

```
00B67483  mov ecx, [0x1436E9C]
00B67489  call 00B46C80
00B6748E  mov ecx, [0x1436E9C]
00B67494  call 00B46890          ; FOGENABLE slot +10568 = 1
00B67503  call 009881F0          ; identity 3×4 on 0x1436E14
```

Bit `0x4` (`00B6B122 cmp eax,4` → `00B6B1CC`) calls it
**before** the `+44` sentinel walk. Bit `0x40` calls
`00B68DA0` (`00B68F19 00B46C80`) then `00B6B177 00B67480`,
gated on `[renderer+1552]`.

Native skip of *this* `00B46C80` after Leave:

| Site | What | Class |
|---|---|---|
| Leave Present | `009BE420` black; no TOD copy | **PROVEN** skip |
| `WorldFrame<=1` | skip `00435530` | **PROVEN** |
| First `00435530` empty dest | no `E8 00B25950` | **PROVEN** skip of 3D consume |
| Empty `+44` bit-4 `00B67480` | setup may run; no DIP | **PROVEN** unused FOGENABLE *if* `00B27D90` runs |

An empty post-Leave `00B27D90` would look like the frontend
bit-4 leftover (copy record 0, no `00BF4570`). Game caller
of `012A0F3C+32` after Leave is **UNREAD**. Do not treat
that unused setup as the first submit.

---

## 2. First map (not Oakvale)

Same map as first stored-cell LEV DIP
(`landscape-submit-leave`, `wld-first-region`).

| Claim | Class |
|---|---|
| First WLD `RegionName` / `NewMap` is `LookoutPoint` | **PROVEN** |
| First `00501450` after dummy is index **1** Lookout | **PROVEN** body; live `E8` **UNREAD** |
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

Host has no `00B46C80` function. `WorldShading` statics are
the ctor-record-0 **payload**, not a runtime apply.
`AuthoredEnvironmentTheme` stores `REGION.EnvironmentTheme`
and is **PROVEN** not applied (`EngineLifecycle` comment).
`RequestNewGame` / `PumpFrontendFrame` do not copy TOD or
set FOGENABLE. **PROVEN** absence.

| Site | Behaviour | Class |
|---|---|---|
| `SubmitCurrentWorld` | `return` unless `HeroSpawned` + region | **PROVEN** skip until Lookout spawn |
| `VulkanLineRenderer.SetMesh` | `_meshCount==0` → return | **PROVEN** skip |
| `Record` mesh pass | skip unless `_meshCount>0` (or objects) | **PROVEN** skip |
| `SilkEngineHost.Draw` | `LinearFogPlane(cam.Position, cam.Forward)` whenever `_frame.Camera` is set | **LEFTOVER** on frontend / dest-empty / pre-hero |
| Mesh push `CameraPos` | host field is the plane, not eye | **EQUIVALENT** *if* verts exist |
| Concat land+C3D+sky | one `SubmittedMesh` | **DISPROVEN** as native DIP boundaries |

So: leftover = computing the fog **plane** on frames native
would skip. Skip = mesh / FOGENABLE / TOD consume until
Lookout verts exist. After those verts, numbers are
**EQUIVALENT**. Do not “fix” frontend by binding 3D fog.
Do not wait for Oakvale theme.

---

## Classifications (short)

1. **First `00B46C80` after Leave that submits — Lookout
   `00B67480` on nonempty `00B27D90`.** TOD `+224=0` copy
   record 0. **PROVEN.** Frontend bit-4 call is *before*
   Leave. Leave Present is a skip.
2. **First map — `LookoutPoint`.** First `oFog` DIP
   `00BF4570` then `00BB2540`. Oakvale **DISPROVEN**.
3. **Host leftover vs skip — leftover plane on empty
   frames; skip of mesh push until `HeroSpawned`.** After
   Lookout verts, constants **EQUIVALENT**.
