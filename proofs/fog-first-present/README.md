# Native fog on first Present — Lookout? Oakvale later?

Investigation only. Production `src/` was not edited.

Do **not** invent fog values. Do **not** start at Oakvale /
`CAM_OVIF_SHOT2` / `ENVIRONMENT_OAKVALE` / invented ambient
`(0.52, 0.58, 0.68)` / fog end **7000**. Those are leftover
`Q_NewOakValeIntro` or SKY_DEF slack, not Leave / first
no-save 3D Present.

Question: native fog on first Present — **Lookout**?
**Oakvale later?**

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Authority: dump `00B482A0` /
`lighting-mgr-ctor-defaults-00b482a0.md` /
`lighting-ctor-fog-defaults-00b4844c-00b4844c.md`
(`0x447A0000` / `0x44FA0000`);
`fog-compute-00b47630-00b47630.md`;
`landscape-fog-slot-00b46890-00b46890.md`;
`landscape-trace/shared-lighting-setup-00b67480.md`;
shader tokens `vshader-landscape-foreground.md` /
`vshader-static-dirlight-fog.md`;
`src/Fable.Formats/WorldShading.cs`;
`src/Fable.Render/VulkanDx9Device.cs`.
Siblings: `proofs/fog-first-submit-leave/`,
`proofs/fog-leave-first/`,
`proofs/fog-after-leave/`,
`proofs/fog-lighting-3d/`,
`proofs/landscape-submit-leave/`,
`proofs/wld-first-region/`,
`proofs/dx9-3d-submit/`.

---

## Verdict

**Yes: first fog-lit Present is LookoutPoint.
Oakvale is later.**

Native fog on that Present is still lighting-manager
**ctor record 0**. Start/end **1000 / 2000**. Colour
**(0, 0, 0, 1)** → `FOGCOLOR 0xFF000000`. Do not
replace those with Oakvale theme RGB or `FogEnd=7000`.

First *game* Present after Leave (`00435F70` →
`00435530`) is dest-empty. That Present has **no**
`oFog` DIP. Leave Present is black. Frontend Present
is 2D sprites. None of those are the fog Present.

| Question | Answer | Class |
|---|---|---|
| Native fog on first *fog-lit* Present? | **Yes.** Lookout `00B67480` + `oFog` DIPs | **PROVEN** as site |
| That Present’s map? | **`LookoutPoint`** (WLD `NewMap 1` / `NewRegion 1`) | **PROVEN** |
| First *game* Present after Leave (`00435530`)? | dest empty; no `E8 00B25950`; no `00BF4570` | **PROVEN** skip. **DISPROVEN** as fog Present |
| Leave Present? | `009BE420` + `009BEEB0` black | **PROVEN** skip |
| Frontend Present? | `VSHADER_2D_SPRITE`; no `oFog` | **PROVEN** skip of 3D fog |
| Oakvale / `StartOakVale` / house 6909? | later `NewRegion 4`; house is bit `0x20` C3D | **DISPROVEN** as this Present |
| Invented `(0.52, 0.58, 0.68)` / end **7000**? | SKY_DEF / intro leftover | **DISPROVEN** as live fog |

---

## Recovered order (no-save New Game)

```
0042EC7C retail
  Init Engine 0042E204
    00B26360  alloc 0x46D0 → ctor 00B482A0
      record 0 fog (0,0,0,1), +80/+84 = 1000/2000
      [esi+224/+225/+226] = 0
      store [0x1436E9C]
  0042DF9E  2D UI Present
    009DA9F0 VSHADER_2D_SPRITE          ; no oFog
    bit-4 00B67480 may run; empty +44; no DIP
0042F2A2 Leave
  009BE420 + 009BEEB0                   ; black; no fog DIP
0042F491 Init Game → 00416953 FinalAlbion.wld
  00507C30  first NewRegion = LookoutPoint
  NewRegion 4 StartOakVale              ; later in the same file
  004A1BD3  Set Static Map → 00B428E0
    FinalAlbion.stb MISS                ; +44 still empty
004189C2 first pumps
  WorldFrame<=1: skip 00435530
  first 00435530 dest empty             ; FIRST game Present; no fog DIP
later 00501450(1) LookoutPoint
  006C2170 / 0051FD80 / 006AC910
  later STB hit → +44 nonempty
next 00B27D90 → 00B6B0B0                ; FIRST fog-lit Present
  bit 0x4:  00B6B1CC 00B67480
              00B46C80  copy record 0
              00B46890  FOGENABLE=1
            then 00BDC060 → 00BF71D0    ; BG walk (DIP clock PARTIAL)
  bit 0x40: 00B68DA0 / 00B6B177 00B67480
            then 00BDC2D0 → 00BF4570    ; first stored-cell oFog DIP
  bit 0x20: 00B32AD0 FOGENABLE; 00BB2540 ; first static oFog DIP
```

Frontend / Leave / dummy `00435530` are **not** this
Present. First *submit* that blends `oFog` is Lookout.

---

## 1. First Present is not Oakvale

Same map as first stored-cell LEV DIP
(`landscape-submit-leave`, `wld-first-region`).

| Claim | Class |
|---|---|
| First WLD `RegionName` / `NewMap` is `LookoutPoint` | **PROVEN** |
| First `00501450` after dummy is index **1** Lookout | **PROVEN** as body; live `E8` **UNREAD** |
| First nonempty `+44` walk is that region’s STB | **PROVEN** as region; first patch on list **PARTIAL** |
| First stored-cell `oFog` DIP is Lookout `00BF4570` bit `0x40` | **PROVEN** as site; first-frame capture **PARTIAL** |
| First static `oFog` DIP is Lookout slot-0 `00BB2540` | **PROVEN** as site |
| First Present map is `StartOakVale` / house 6909 | **DISPROVEN** (`NewRegion 4`; house is bit `0x20` C3D) |
| `ENVIRONMENT_OAKVALE` / `Q_NewOakValeIntro` | later persist / intro leftover | **DISPROVEN** as this Present |
| Lookout `REGION.EnvironmentTheme` | `ENVIRONMENT_THEME1` (#2346), not Oakvale | **PROVEN** name; **DISPROVEN** as live 112-byte fog record |

`00DBDE40` / `CAM_OVIF_SHOT2` are **not** no-save first
Present (`00DAAC00-sqnovi-no-save`, `camera-after-leave`).

---

## 2. Native fog on that Lookout Present (dump — no invented numbers)

Payload is ctor record 0. Copied by `00B46C80`
`+226==0` → `jbe 00B46E17`. No first-seen `SetTime` /
theme unpack writes `+224/+225/+226` before this Present.
**PROVEN** absence. Theme 269-byte TOD blob has no
1000/2000 immediates — **DISPROVEN** as the live record.

Ctor stores (`00B4844C`):

```
00B4844F  mov [eax+64], ebx          ; fog rgb 0
00B48452  mov [eax+68], ebx
00B48455  mov [eax+72], ebx
00B48458  mov [eax+76], ecx          ; 0x3F800000 = 1
00B48461  mov [edx+80], 0x447A0000   ; 1000
00B4846B  mov [eax+84], 0x44FA0000   ; 2000
```

| Item | Value | Class |
|---|---|---|
| record fog colour `+64..+76` | `(0, 0, 0, 1)` | **PROVEN** |
| `FOGCOLOR` | **`0xFF000000`** | **PROVEN** |
| start / end `+80/+84` | **1000 / 2000** | **PROVEN** |
| `c18` | `(0, 0, 0, 1)` | **PROVEN** |
| `D3DRS_FOGENABLE` | **1** after `00B46890` (`mov [ecx+4], 0x1`) | **PROVEN** |
| `FOGTABLEMODE` / `FOGVERTEXMODE` | ctor **0** (NONE); VS `oFog` still blends | **PROVEN** |
| `c2` | `00B47630` linear view-Z from camera **+276**, start/end above | **PROVEN** as formula. Inverse row 0 **DISPROVEN**. Plane *numbers* follow the eye (**PARTIAL** / B) |
| `c0.y` | **1** after LayoutBasic dirty-2 | **PROVEN** |
| VS | `mad oFog, min(dp4(pos,c2), c0.y), -c18.w, c0.y` | **PROVEN** tokens |
| first-seen `c18.w=1` | `oFog = 1 − min(dot, 1)` then D3D sat `[0,1]` | **PROVEN** |
| blend | `rgb*oFog + (1−oFog)*black` | **PROVEN** |
| packed light count | **0** → `*_DIRLIGHT_FOG` | **PROVEN** |
| 7000 | `WorldShading.FogEnd` SKY_DEF flare slack | **DISPROVEN** as fog end |
| `(0.52, 0.58, 0.68)` | invented Oakvale ambient / theme RGB | **DISPROVEN** |

Do not write a different start, end, or RGB for “Lookout
first Present.” Those floats are not in the dump for this
path.

`VSHADER_LANDSCAPE_BACKGROUND` tokens also write `oFog`.
Bit `0x4` `00BF71D0` walks **before** `00BF4570`. Whether
that BG call DIPs on the first Lookout Present is
**PARTIAL** (same DIP-clock hole as `dx9-3d-submit`).
First *stored-cell* `oFog` DIP stays `00BF4570`.

---

## 3. Host leftover vs skip (`VulkanDx9Device` / `Draw`)

`VulkanDx9Device` is the NativeSemantic **2D** device.
`Present` packs a `FrontendSubmitBatch` and optionally
`PresentDx9`. It does **not** upload fog `c2`/`c18`,
does **not** `DrawIndexedPrimitive` (throws UNREAD),
and `SetRenderState` only stores. Frontend 2D Present
is therefore **not** a fog apply. **PROVEN** absence.

3D fog on the host is `SilkEngineHost.Draw` →
`WorldShading.LinearFogPlane` → `VulkanLineRenderer`
mesh push (`CameraPos` is the plane, misnamed).

| Site | Behaviour | Class |
|---|---|---|
| `VulkanDx9Device.Present` | 2D batch; no `oFog` | **PROVEN** skip of 3D fog |
| `SubmitCurrentWorld` | `return` unless `HeroSpawned` + region | **PROVEN** skip until Lookout spawn |
| `VulkanLineRenderer.SetMesh` | `_meshCount==0` → return | **PROVEN** skip |
| `Record` mesh pass | skip unless `_meshCount>0` (or objects) | **PROVEN** skip (matches dest-empty) |
| `SilkEngineHost.Draw` | `LinearFogPlane` whenever `_frame.Camera` is set | **LEFTOVER** on frontend / dest-empty / pre-hero |
| Mesh push after Lookout verts | ctor record 0 start/end/colour | **EQUIVALENT** payload. Plane *numbers* **PARTIAL** (camera B) |
| Concat land+C3D+sky | one `SubmittedMesh` | **DISPROVEN** as native DIP boundaries |

Do not “fix” frontend by binding 3D fog on
`VulkanDx9Device.Present`. Do not wait for Oakvale
theme. Do not invent a second fog record for later
Oakvale on this page.

---

## Classifications (short)

1. **Native fog on first fog-lit Present — Lookout.
   PROVEN.** Dummy `00435530` / Leave / frontend are
   skips. Values are ctor record 0 (1000/2000, black).
2. **Oakvale later — DISPROVEN as this Present.**
   `NewRegion 4` `StartOakVale`. Invented RGB / 7000
   are not the live fog record.
3. **Host leftover vs skip — leftover plane on empty
   frames; skip of mesh push until `HeroSpawned`.**
   `VulkanDx9Device` has no fog. After Lookout verts,
   constants **EQUIVALENT**.
