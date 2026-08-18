# First fog / lighting after Leave `0042F2A2`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `CAM_OVIF_SHOT2` /
`ENVIRONMENT_OAKVALE` / invented ambient
`(0.52, 0.58, 0.68)` / fog end **7000**.
Those are leftover `Q_NewOakValeIntro` or SKY_DEF slack,
not Leave / Init Game / first no-save 3D Present.

Do **not** treat frontend 2D (`0042DF9E` /
`VSHADER_2D_SPRITE`) or an empty landscape bit-4
`00B67480` as a 3D fog/light *draw*.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Sources: `docs/runtime/FORWARD_TREE.md` §§4, 14–15;
`docs/PARITY.md` scene layers / dirlight+fog;
`docs/status/investigations/2026-08-18-environment.md`,
`2026-08-18-scene-layers.md`, `G-dx9-vulkan.md`;
`WorldShading.cs` / `LandscapeFrustum.cs` /
`ScenePass.cs` (`D3dDeviceState`);
`EngineLifecycle.cs` (`AuthoredEnvironmentTheme` comment);
`EngineLifecycleTests` (`New_game_is_leave_frontend_then_FinalAlbion_wld`);
`CameraProjectionTests` / `ShaderFormatTests` /
`ScenePassTests` first-seen fog locks.

Siblings: `proofs/fog-lighting-3d` (ctor + frontend flush +
payload locks), `proofs/camera-after-leave` (same Leave spine),
`proofs/landscape-after-leave` (empty `+44` then Lookout DIP),
`proofs/terrain-first-draw`, `proofs/c3d-first-submit`,
`proofs/palskin-after-leave`.

Dumps: `newgame-trace/lighting-mgr-ctor-defaults-00b482a0.md`,
`fog-compute-00b47630-00b47630.md`,
`fog-colour-setter-009886c0-009886c0.md`,
`c35-setter-default-0098b2c0-0098b2c0.md`,
`landscape-trace/shared-lighting-setup-00b67480.md`,
`landscape-draw-vtbl16-00b6b0b0.md`,
`implementer/stars/fn-00B46C80.txt` / `fn-00B46E17-exact.txt`,
listings `00B24850` / `00B32AD0` / `00B25950`.

---

## Verdict

**First 3D fog / lighting *consume* after Leave `0042F2A2`
is Lookout landscape `00B67480` then MainScene `00B32AD0`
on the first nonempty `00B27D90`.** Not frontend. Not
Leave Present. Not Oakvale.

The lighting **object** is constructed *before* Leave
(`00B482A0` at Init Engine). After Leave the first live
apply still copies **ctor record 0**. Do not wait for
`ENVIRONMENT_THEME1` unpack.

| What | First native site | Relative to Leave `0042F2A2` |
|---|---|---|
| `CEngineLightingManager` ctor | Init Engine `00B26360` → `00B482A0` `[0x1436E9C]` | **before** Leave **PROVEN** |
| Fog plane / `FOGCOLOR` helper | `00B25950` → `00B24850` → `00B47630` | can run on frontend empty walk; **not** a 3D draw |
| Leave Present | `0042EBB6` `009BE420` + `009BEEB0` | teardown black **PROVEN** |
| First 3D *setup* after maps | landscape bits `0x4` / `0x40`: `00B67480` = `00B46C80` + `00B46890` | **after** Leave + Lookout patches **PROVEN** |
| First static / PALSKIN FOGENABLE | MainScene `00B32AD0` bits `4/8/0x10/0x20/0x40/…` | same nonempty walk **PROVEN** |
| First *drawn* `oFog` / dirlight | Lookout `00BF4570` (bit `0x40`) then static `0x20` slot 0 | **PROVEN** as DIP; game `00B25950` caller **UNREAD** |

`Q_NewOakValeIntro` / `ENVIRONMENT_OAKVALE` / fog RGB
`(0.52, 0.58, 0.68)` / end 7000 are **not** this path.
**PROVEN** (`2026-08-18-environment.md`;
`WorldShading.FogEnd` is SKY_DEF slack).

---

## Timeline (no-save New Game)

```
0042EC7C retail
  Init Engine 0042E204                    // still before Leave
    00B26340 engine
    00B26360 layer register
      alloc 0x46D0 → ctor 00B482A0        // lighting object
      store [0x1436E9C] via 00B2A160
      camera [0x1436EA0] ctor 00B31700
  Init frontend 0042EF6F
  0042DF9E 2D UI
    0042E0BB [retail+88].vtbl+32 = 00B27D90
      00B25950 → 00B24850 → 00B47630      // plane/colour flush
      00B24DA3 FOGENABLE forced 0
      landscape vtbl+16 empty +44
      bit 4 still 00B67480 (setup, no DIP)
    009DA9F0 VSHADER_2D_SPRITE            // no oFog / no c19
0042F2A2 Leave frontend                   // not 00DBDE40
  009BE420 clear + 009BEEB0 Present
0042F491 Init Game → 00418DCA → 004184BD
  Init World 004A6E30                     // cameras; not a theme apply
  00416953 Load world FinalAlbion.wld
    004A1840 → 00B428E0 FinalAlbion.stb MISS
004189C2 first pumps
  WorldFrame<=1: skip 00435530
  first 00435530 often empty dest         // no 00B25950 E8
later 00501450 Lookout + 006C2170
  next nonempty 00B27D90
    bit 0x4:  00B6B1CC 00B67480 then 00BDC060
    bit 0x40: 00B6B177 00B67480 then 00BDC2D0 → 00BF4570
    bit 0x20: 00B32AD0 FOGENABLE; slot-0 DIRLIGHT_FOG
    bit 0x100: same 00B32AD0; PALSKIN DIRLIGHT_FOG
```

Leave itself does not construct, copy, or DIP lighting.
**PROVEN** (`0042F2A2` is audio/UI teardown + path write).

---

## 1. Frontend is not the first 3D fog / light

The object exists. The UI does not consume it.

| Claim | Class | Evidence |
|---|---|---|
| Lighting manager exists after Init Engine (still retail) | **PROVEN** | `00B26360` `push 0x46D0` / `call 00B482A0` / `[0x1436E9C]` |
| Ctor record 0 is the live payload | **PROVEN** | dump `00B482A0`; see §3 |
| Frontend Present is 2D (`VSHADER_2D_SPRITE`) | **PROVEN** | `camera-after-leave`; `mov oPos,v0` / `oD0,v1` / `oT0,v2` |
| That VS writes `oFog` / reads `c19` | **DISPROVEN** | 2D sprite program has neither |
| `00B47630` can run on the empty layer walk | **PROVEN** | `00B259B5 call 00B24850`; `00B25871 mov ecx,[0x1436E9C]` / `00B25877 call 00B47630` |
| `00B24850` forces `D3DRS_FOGENABLE` to **0** before compute | **PROVEN** | `00B24DA3` slot `+10568`; `xor ebp,ebp` |
| Landscape bit 4 still calls `00B67480` with empty `+44` | **PROVEN** | `00B6B122 cmp eax,4` → `00B6B1CC call 00B67480` **before** the sentinel walk |
| Bit `0x40` `00B67480` gated on `[renderer+1552]` | **PROVEN** | `00B6B13B test al,al; je 00B6B23C` |
| Frontend issues a 3D dirlight / `oFog` DIP | **DISPROVEN** | empty `[0x1436E8C]+44`; type `0x22` only; `landscape-after-leave` |

**Answer:** lighting object + fog flush exist during frontend.
That is not a 3D light/fog *draw*. UI sprites have no `oFog`.

---

## 2. First 3D apply after Leave

Not Oakvale. Not a theme unpack. First *drawn* scene is
LookoutPoint (WLD index **1**) after `00501450` / `006C2170`.

### Construct (before Leave — payload only)

| Order | VA | Object | Class |
|---|---|---|---|
| 1 | `00B482A0` | `CEngineLightingManager` size `0x46D0` at `[0x1436E9C]` vtbl `012A2274` | **PROVEN** Init Engine |
| 2 | same ctor | record alloc `00B4A4C0`; types `0xF`/`0x10` via `00B8FAD0` | **PROVEN**. vtbl+20 `00B4A450` = `ret 8` — **DISPROVEN** as a DIP family |

### First live apply (after Leave + maps)

| Site | When | Class |
|---|---|---|
| `00B67480` | landscape bits `0x4` (`00B6B1CC`) and `0x40` (`00B6B177`) | **PROVEN** first 3D *setup*: `00B46C80` then `00B46890` then identity 3×4 `009881F0` (`0x1436E14`) |
| `00B46C80` `+226==0` | ctor TOD bytes 0 | **PROVEN** `jbe 00B46E17` copy record `[+224]=0` → `[esi+72]` colour / `[esi+104]` c35 path; `00B49950`; `00989830(0)`; `0098B2C0` |
| `00B46890` | FOGENABLE slot `+10568` = **1** | **PROVEN** (`D3dDeviceState.FogEnableSetter`) |
| `00B32AD0` | MainScene bits `4/8/0x10/0x20/0x40/0x400/0x40000` | **PROVEN** also `00B46890`. First nonempty `0x20` is static slot 0 |
| `00B47630` | `00B24850` inside `00B25950` | **PROVEN** plane + `FOGCOLOR`. Game caller of `00B25950` after Leave **UNREAD** (same hole as terrain) |
| First dummy `00435530` | empty `009DA9F0`; no `E8 00B25950` | **PROVEN** skip of 3D consume. **DISPROVEN** as first fog-lit Present |
| First *drawn* fog/light | Lookout `00BF4570` + static `00BB2540` after `00501450` / `006C2170` | **PROVEN** as the DIP; native STB re-open **UNREAD** |

`00B67480` listing (`shared-lighting-setup-00b67480.md`):

```
00B67483  mov ecx, [0x1436E9C]
00B67489  call 00B46C80          ; TOD / record 0 copy
00B6748E  mov ecx, [0x1436E9C]
00B67494  call 00B46890          ; FOGENABLE = 1
00B67499  mov ecx, [0x1436E14]
          stack identity 3×4
00B67503  call 009881F0          ; world wrapper
```

Load is not apply: `004A1840` / `00B428E0` during Init Game
does not copy record 0 onto VS constants and does not DIP.

---

## 3. Locked first-seen constants (still ctor record 0)

`WorldShading` / `LandscapeFrustum` / `D3dDeviceState`.
Theme 269-byte TOD blob is **DISPROVEN** as the live fog record
(no 1000/2000 immediates). Copy onto the 112-byte lighting
record before first Present is **UNREAD**.

### Directional light

| Item | Value | Class |
|---|---|---|
| `c19` | `(0, 1, 0, 0)` | **PROVEN** ctor `[esi+48]`; apply `00F39D40` writes `w=0` |
| `c20` | `(0.25, 0.25, 0.25, 1)` | **PROVEN** record 0 `+0` = `0x3E800000` × 3 |
| `c35` | `(0, 0, 0, 1)` | **PROVEN** `0098B2C0` stack default; apply does **not** store it |
| leftover `c3` | `(0, 0.125, 0, 0)` | **PROVEN** table `0x0139C614`. **DISPROVEN** as ambient |
| formula | `max(n·−c19,0)² * c20 + c35 + c3` | **PROVEN** MAD, not LIT |
| packed count | **0** → family slot 0 → `VSHADER_*_DIRLIGHT_FOG` | **PROVEN** |
| lighting mode `[+18068]` | **1** | **PROVEN** ctor; setter `00B23C00` has zero `E8` |
| TOD `+224/+225/+226` | **0** | **PROVEN** |

There is **no** separate ambient register. Invented
`0.28+0.72*n·sun` is **DISPROVEN**. Unlit faces are leftover
**c3**, not a fill-in 1.

### Fog

| Item | Value | Class |
|---|---|---|
| `D3DRS_FOGENABLE` | **1** after `00B46890` | **PROVEN** |
| `FOGCOLOR` | **`0xFF000000`** (record `(0,0,0,1)*255`) | **PROVEN** |
| `FOGTABLEMODE` / `FOGVERTEXMODE` | ctor **0** (NONE); VS `oFog` still blends | **PROVEN** |
| start / end | **1000 / 2000** (`0x447A0000` / `0x44FA0000` at record `+80/+84`) | **PROVEN** |
| `c18` | `(0, 0, 0, 1)` | **PROVEN** |
| `c2` | `LinearFogPlane` from camera **+276** unscaled view-Z | **PROVEN**. Inverse row 0 **DISPROVEN** (`00B54310` is mesh-path `00B555A0` only) |
| `oFog` | `c0.y − min(dp4(pos,c2), c0.y) * c18.w` then D3D sat `[0,1]` | **PROVEN** |
| first-seen `c0.y=1`, `c18.w=1` | `oFog = 1 − min(dot, 1)` | **PROVEN** |
| blend | `rgb*oFog + (1−oFog)*black` | **PROVEN** |
| 7000 | SKY_DEF flare slack | **DISPROVEN** as fog end |
| `ENVIRONMENT_THEME1` record 0 | 269-byte TOD; no 1000/2000 | **DISPROVEN** as live fog record |

Host push field `CameraPos` is the plane
(`SilkEngineHost.Draw` → `WorldShading.LinearFogPlane`).

---

## 4. C# after Leave (not frontend)

| Site | What it does | Class |
|---|---|---|
| `WorldShading` statics | ctor record 0 constants | **EQUIVALENT** payload. Not a runtime apply. |
| `AuthoredEnvironmentTheme` | host stores `REGION.EnvironmentTheme` name only | **PROVEN** not applied |
| `SilkEngineHost.Draw` | `LinearFogPlane` every Present | **LEFTOVER** on frontend / dest-empty frames. Native 2D path does not bind that plane to sprites. After Lookout verts exist → **EQUIVALENT** plane |
| `VulkanLineRenderer` mesh push | `LightDir`/`LightColor`/`CameraPos`=plane when `_meshCount>0` | **EQUIVALENT** *if* 3D verts exist. First `004189C2` mesh count 0 → skip |
| `RequestNewGame` / `PumpFrontendFrame` | no `00B46C80` / FOGENABLE | **PROVEN** absence |
| `SubmitCurrentWorld` after `HeroSpawned` | timing after `006C2170` | **PROVEN** as gate. Concat land+C3D+sky is **DISPROVEN** as native DIP |
| Lighting in FS not VS | host interpolates n then evaluates | **PARTIAL** vs native VS `oD0` (G) |

---

## Classifications (short)

1. **Frontend 3D fog/lighting besides 2D UI — DISPROVEN as a draw.**
   Lighting object + `00B47630` flush exist after Init Engine.
   Sprites do not use `oFog` / `c19`. Empty landscape bit 4 may
   set FOGENABLE=1 unused.
2. **Leave Present — DISPROVEN as fog/light.** Black
   `009BE420` + `009BEEB0`. No record copy, no DIP.
3. **First 3D setup after Leave — `00B67480` (`00B46C80`
   copy record 0 + `00B46890`) then `00B32AD0` on Lookout
   bits `0x4` / `0x40` / `0x20`. PROVEN.** Scene is Lookout,
   not Oakvale. Game `00B25950` site after Leave **UNREAD**.
   First `00435530` empty **DISPROVEN** as the lit Present.
4. **Theme / Oakvale fog — DISPROVEN** as this site.
   Live numbers are Init-Engine ctor record 0 (1000/2000,
   black, `c19=(0,1,0)`, `c20=0.25`).
5. **C# world fog during frontend — LEFTOVER**
   (`SilkEngineHost.Draw` plane from `ScriptedCamera`).
   After Leave + verts, constants are **EQUIVALENT**.
