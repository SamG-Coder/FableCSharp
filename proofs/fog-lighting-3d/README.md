# First fog / lighting setup after Leave Frontend

Investigation only. No production `src` edits.
Do **not** start at Oakvale / `CAM_OVIF_SHOT2` / invented ambient.
Live first-seen 3D payload is lighting-manager **ctor record 0**,
consumed by landscape / static / PALSKIN `*_DIRLIGHT_FOG` VS.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** / **LEFTOVER**.

Sources: `src/Fable.Formats/WorldShading.cs`,
`LandscapeFrustum.cs`, `ScenePass.cs` (`D3dDeviceState`);
`docs/status/investigations/2026-08-18-environment.md`,
`2026-08-18-materials.md`, `2026-08-18-scene-layers.md`,
`G-dx9-vulkan.md`, `A-dx9-submit.md`;
`proofs/camera-after-leave/README.md`,
`proofs/terrain-first-draw/README.md`;
ExeIndex `newgame-trace/lighting-mgr-ctor-defaults-00b482a0.md`,
`fog-compute-00b47630-00b47630.md`,
`landscape-fog-slot-00b46890-00b46890.md`,
`shared-lighting-setup-00b67480.md`,
listings `00B26360` / `00B24850` / `00B6B0B0` / `00B46C80`.

---

## Timeline (no-save New Game)

```
0042EC7C retail
  Init Engine 0042E204
    00B26340 engine vtbl 012A0F3C
    00B26360 layer register
      alloc 0x46D0 → ctor 00B482A0          // lighting object
      store [0x1436E9C] via 00B2A160
      camera [0x1436EA0] ctor 00B31700
  Init frontend 0042EF6F
  0042DF9E 2D UI flush
    0042E0BB [retail+88].vtbl+32 = 00B27D90
      00B25950 → 00B24850 → 00B47630        // fog plane/colour flush
      landscape vtbl+16 empty list
      00B67480 may still run on bit 4
    009DA9F0 VSHADER_2D_SPRITE              // no oFog
0042F2A2 Leave frontend
  009BE420 clear + 009BEEB0 Present (black)
0042F491 Init Game → 00418DCA
  Init World 004A6E30
  00416953 Load world FinalAlbion.wld
004189C2 first pumps
  WorldFrame<=1: skip 00435530
  first 00435530 often empty dest           // no 00B25950 E8
later 00501450 Lookout + 006C2170
  next nonempty 00B27D90
    bit 0x4 / 0x40: 00B67480 = 00B46C80 + 00B46890
    bit 0x20: 00B32AD0 FOGENABLE; slot-0 DIRLIGHT_FOG
```

`Q_NewOakValeIntro` / `ENVIRONMENT_OAKVALE` / fog RGB
`(0.52,0.58,0.68)` / end 7000 are **not** this path. **PROVEN**
(environment investigation; `WorldShading.FogEnd` is SKY_DEF slack).

---

## 1. Fog / lighting during frontend besides 2D UI?

| Claim | Class | Evidence |
|---|---|---|
| Lighting manager exists after Init Engine (still retail, before Leave) | **PROVEN** | `00B26360` `push 0x46D0` / `call 00B482A0` / `push 0x1436E9C` |
| Ctor writes record 0: `c20=(0.25)×3+1`, fog `(0,0,0,1)`, start/end `1000/2000`, packed count 0, mode 1, TOD bytes 0, dirty `0xF` | **PROVEN** | dump `00B482A0`; `WorldShading` / `LandscapeFrustum` locks |
| Dirlight `c19=(0,1,0)` at `[esi+48]` | **PROVEN** | `00B483D5`..`00B483FF` |
| Frontend Present is 2D UI (`VSHADER_2D_SPRITE`: `mov oPos,v0` / `oD0,v1` / `oT0,v2`) | **PROVEN** | `FrontendDraw.cs`; `camera-after-leave` |
| That VS writes `oFog` / reads `c19` | **DISPROVEN** | 2D sprite program has neither |
| `00B27D90` → `00B25950` runs after Init Engine | **PROVEN** | `terrain-first-draw`; listing `0042E0BB` |
| `00B25950` calls `00B24850` then `00B24850` calls `00B47630` | **PROVEN** | `00B259B5 call 00B24850`; `00B25871 mov ecx,[0x1436E9C]` / `00B25877 call 00B47630` |
| `00B24850` forces `D3DRS_FOGENABLE` to **0** before that compute | **PROVEN** | `00B24DA3` slot `+10568`; `xor ebp,ebp`; `mov [edx+4], ebp` |
| Landscape bit 4 still `00B67480` even when patch list empty | **PROVEN** | `00B6B0B0` `cmp eax,4` → `call 00B67480` **before** the empty-sentinel walk |
| Bit `0x40` `00B67480` gated on `[renderer+1552]` | **PROVEN** | `00B6B13B test al,al; je 00B6B23C` |
| Frontend issues a 3D dirlight / `oFog` DIP | **DISPROVEN** | empty `[0x1436E8C]+44`; type `0x22` only; `terrain-first-draw` |
| Host `SilkEngineHost.Draw` builds `LinearFogPlane` on frontend frames | **LEFTOVER** | always uses `_frame.Camera`. Mesh push is skipped when `_meshCount==0` (`VulkanLineRenderer.Record`) |

**Answer:** the lighting **object** and fog **flush helper** exist during frontend. `00B47630` can run on the empty layer walk. That is not a 3D light/fog *draw*. UI is 2D sprites with no `oFog`. First-seen FOGENABLE after an empty bit-4 setup would be 1, then unused.

---

## 2. First 3D fog / lighting after Leave / Init Game / world load

Not Oakvale. Not a theme unpack. Payload is still ctor record 0.

### Construct (before Leave)

| Order | VA | Object | Class |
|---|---|---|---|
| 1 | `00B482A0` | `CEngineLightingManager` size `0x46D0` at `[0x1436E9C]` vtbl `012A2274` | **PROVEN** Init Engine |
| 2 | same ctor | record alloc `00B4A4C0`; types `0xF`/`0x10` via `00B8FAD0` | **PROVEN**. vtbl+20 `00B4A450` = `ret 8` — **DISPROVEN** as a DIP family |

### First live apply (after Leave + maps)

| Site | When | Class |
|---|---|---|
| `00B67480` | landscape bits `0x4` and `0x40` | **PROVEN** first 3D *setup* on those bits: `00B46C80` then `00B46890` then identity 3×4 `009881F0` |
| `00B46C80` `+226==0` | ctor TOD bytes 0 | **PROVEN** `jbe 00B46E17` copy record `[+224]=0` → `[esi+72]` colour / `[esi+104]` c35 path; `00B49950`; `00989830(0)`; `0098B2C0` |
| `00B46890` | FOGENABLE slot `+10568` = **1** | **PROVEN** |
| `00B32AD0` | MainScene bits `4/8/0x10/0x20/0x40/0x400/0x40000` | **PROVEN** also `00B46890`. First nonempty `0x20` is static slot 0 |
| `00B47630` | `00B24850` inside `00B25950` | **PROVEN** plane + `FOGCOLOR`. Game caller of `00B25950` after Leave **UNREAD** (same hole as terrain) |
| First dummy `00435530` | empty `009DA9F0`; no `E8 00B25950` | **PROVEN** skip of 3D consume. **DISPROVEN** as first fog-lit Present |
| First *drawn* fog/light | Lookout landscape `00BF4570` + static `0x20` after `00501450` / `006C2170` | **PROVEN** as the DIP; native STB re-open **UNREAD** |

### Locked first-seen constants (`WorldShading`)

| Item | Value | Class |
|---|---|---|
| `c19` | `(0, 1, 0, 0)` | **PROVEN** |
| `c20` | `(0.25, 0.25, 0.25, 1)` | **PROVEN** |
| `c35` | `(0, 0, 0, 1)` setter default; apply `00F39D40` does **not** store it | **PROVEN** |
| leftover `c3` | `(0, 0.125, 0, 0)` table `0x0139C614` | **PROVEN**. **DISPROVEN** as ambient |
| formula | `max(n·−c19,0)² * c20 + c35 + c3` | **PROVEN** MAD, not LIT |
| packed count | **0** → family slot 0 → `VSHADER_*_DIRLIGHT_FOG` | **PROVEN** |
| `c18` | `(0, 0, 0, 1)` | **PROVEN** |
| start / end | **1000 / 2000** (`0x447A0000` / `0x44FA0000`) | **PROVEN** |
| `c2` | `LinearFogPlane` from camera **+276** unscaled view-Z | **PROVEN**. Inverse row 0 **DISPROVEN** |
| `oFog` | `c0.y − min(dp4(pos,c2), c0.y) * c18.w` then D3D sat `[0,1]` | **PROVEN** |
| blend | `rgb*oFog + (1−oFog)*black` | **PROVEN** |
| `FOGTABLEMODE` / `FOGVERTEXMODE` | ctor **0** (NONE) | **PROVEN** |
| 7000 | SKY_DEF flare slack | **DISPROVEN** as fog end |
| `ENVIRONMENT_THEME1` record 0 | 269-byte TOD blob; no 1000/2000 | **DISPROVEN** as live fog record |

**Answer:** first 3D *consume* is `00B67480` / `00B32AD0` on the first nonempty landscape / static walk after Lookout load. The numbers are still Init-Engine ctor record 0. Do not wait for a theme apply.

---

## 3. C# that sets world fog / lighting during frontend

| Site | What it does | Class |
|---|---|---|
| `WorldShading` statics | ctor record 0 constants | **EQUIVALENT** payload. Not a runtime apply. |
| `EngineLifecycle.Camera = new()` | `ScriptedCamera` from Bootstrap; FOV 72 leftover | **LEFTOVER** (camera proof) |
| `SilkEngineHost.Draw` | `LinearFogPlane(cam.Position, cam.Forward)` every Present | **LEFTOVER** on frontend frames. Native 2D path does not bind that plane to sprites. |
| `VulkanLineRenderer.Record` mesh push | `LightDir`/`LightColor`/`CameraPos`=plane when `_meshCount>0` | **EQUIVALENT** *if* 3D verts exist. Frontend mesh count 0 → skip. |
| `RequestNewGame` / `PumpFrontendFrame` | no `00B46C80` / FOGENABLE | **PROVEN** absence |
| Binding `REGION.EnvironmentTheme` into lights | host stores the name only | **PROVEN** not applied (`AuthoredEnvironmentTheme` comment) |

---

## Classifications (short)

1. **Frontend 3D fog/lighting besides 2D UI — DISPROVEN as a draw.** Lighting object + `00B47630` flush exist after Init Engine. Sprites do not use `oFog` / `c19`. Empty landscape bit 4 may set FOGENABLE=1 unused.
2. **First 3D setup after Leave — `00B67480` (`00B46C80` copy record 0 + `00B46890`) then `oFog` DIPs on Lookout bits `0x4`/`0x40`/`0x20`. PROVEN.** Scene is Lookout, not Oakvale. Game `00B25950` site after Leave **UNREAD**. First `00435530` empty **DISPROVEN** as the lit Present.
3. **C# world fog during frontend — LEFTOVER** (`SilkEngineHost.Draw` plane from `ScriptedCamera`). Native does not shade UI with the 3D fog record.

Dumps: `newgame-trace/lighting-mgr-ctor-defaults-00b482a0.md`,
`fog-compute-00b47630-00b47630.md`,
`landscape-trace/shared-lighting-setup-00b67480.md`,
listing `00B2674D` (ctor store), `00B25877` (fog compute),
`00B6B0B0` (land bits), `00B32E5F` (MainScene FOGENABLE).
