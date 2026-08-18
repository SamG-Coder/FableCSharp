# First-scene environment (LookoutPoint, no-save New Game)

Investigation only. No production source was modified.
`EngineLifecycle.cs` was not edited.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Do not invent a green tint, an ambient colour, or a screenshot match.
First-seen Lookout green is leftover `c3=(0,0.125,0)` × `mul_x2` ×
`oT1=(0,0)` ([G-dx9-vulkan.md](G-dx9-vulkan.md),
`Dx9VulkanShaderConstants.UnlitRgbIsC3Leftover`).

Read first: [G-dx9-vulkan.md](G-dx9-vulkan.md),
`src/Fable.Formats/WorldShading.cs`,
`src/Fable.Formats/Levels/LandscapeFrustum.cs`,
`src/Fable.Formats/Sky/SkyPass.cs`,
`docs/PARITY.md` (scene layers / SKY_DEF / dirlight+fog),
`docs/status/README.md` (no-save first Present is LookoutPoint).

Dumps: `tools/Fable.ExeIndex/out/01-sections/newgame-trace/`
(lighting ctor, TOD blend, fog compute, ENVIRONMENT persist),
`tools/Fable.ExeIndex/out/01-sections/landscape-trace/shared-lighting-setup-00b67480.md`,
`implementer/stars/` (`fn-00B46C80*`, `fn-00430900`, `disp-424-env.txt`,
`fn-00B26828`), shipping `FinalAlbion.wld` + `game.bin`.

`FirstSceneWorld` is the Oakvale intro helper (`StartOakValeWest` /
`CAM_OVIF_SHOT2`). It is **not** the no-save first Present.

---

## Verdict (read this first)

No-save New Game’s first *rendered* scene is **LookoutPoint**
(WLD region index **1**, `RegionDef "REGION_LOOKOUT_POINT"`).

The WLD token **`EnvironmentDef` is not the source**. The exe parser
recognises it (`00507C30` / `00508987`), `EngineLifecycle.LoadWldTokens`
lists it, and `WorldFile.Parse` does **not** store it — and shipping
`FinalAlbion.wld` contains **zero** `EnvironmentDef` lines.

What *is* authored:

| Layer | Lookout binding | Class |
|---|---|---|
| WLD `NewRegion 1` | `RegionDef "REGION_LOOKOUT_POINT"` | **PROVEN** |
| `game.bin` `REGION` | field `EnvironmentTheme` → entry **2346** `ENVIRONMENT_THEME1` (`ENVIRONMENT_THEME_DAY`) | **PROVEN** |
| Same theme | `REGION_OAK_VALE_INTRO` also points at **2346** | **PROVEN** |
| Theme TOD table | 8 keys: **0, 4, 5, 10, 12, 18, 20, 21** (269-byte records) | **PROVEN** |
| Global `ENVIRONMENT` | persist object `[0x1436E24]`; `lightning_colours.tga` in an early string; +288/+292/+296 first-seen **0** | **PROVEN** |
| Live VS lighting/fog | lighting-manager ctor `00B482A0` record 0, copied by `00B46C80` at TOD bytes 0 | **PROVEN** |

`ENVIRONMENT_THEME1` record 0 does **not** contain the live fog
immediates `1000` / `2000` (`0x447A0000` / `0x44FA0000`). It does
contain `0.25` (`0x3E800000`) and `100` (`0x42C80000`, sky near).
Copying that 269-byte blob onto the 112-byte lighting record before
first Present is **UNREAD**. Live first-seen constants match the
**ctor**, not a recovered theme unpack.

Do **not** drive Lookout from `ENVIRONMENT_OAKVALE`. That def exists
(7 TOD keys) and is a different `ENVIRONMENT_THEME_DAY` instance.

---

## What LookoutPoint should have active immediately after no-save New Game

This is the first Present after AVI unload + `00501450` →
`00500540(1,0,0)` + `006C2170` Loading objects. Not SHOT2.

### World / maps

| Item | Active? | Class |
|---|---|---|
| Current region index `WorldMap+156` = **1** LookoutPoint | yes | **PROVEN** |
| Primary compiled map `LookoutPoint` (`00B3E820`) | yes | **PROVEN** |
| Region `ContainsMap`: LookoutPoint, BowerstoneBridge, GuildExterior | authored; first-seen *draw* is primary-only | **PROVEN** authored; **PROVEN** primary draw |
| `SeesMap` fillers / Picnic / Greatwood / Fisherman | neighbour headers `00B41E50` / `00BDF010`; not first-seen mesh submit | **PROVEN** open; **PROVEN** not primary submit |
| Hero `CREATURE_HERO` mesh **4299** at `GuildArrivalHSP` `(52.69, 69.60, 36.98)` | yes (`0051FD80` / `006AC910`) | **PROVEN** |
| Camera `006B3FF0` seed (Lookout helper, not SHOT2 72°) | yes | **PROVEN** seed; pose **UNREAD** |
| `Q_NewOakValeIntro` / `CAM_OVIF_SHOT2` / kid 4300 | no | **DISPROVEN** as first no-save Present |

### Time

| Item | Value | Class |
|---|---|---|
| Lighting ctor `+224/+225/+226` | **0** | **PROVEN** (`00B482A0`) |
| `00B46C80` path | `+226==0` → `00B46E17` copy record `[+224]=0` | **PROVEN** |
| Script `SetTime` / `SetTimeOfDay` | not on this path; `TimeOfDayHours` defaults **0** | **PROVEN** no first-seen writer |
| Theme key selected | TOD **0** (first of 8) *if* theme is bound | **PARTIAL** (key exists; apply unread) |

TOD 0 on `ENVIRONMENT_THEME1` is the midnight slot, not midday 12.
Do not pick `GRAPHIC_ATMOSPHERIC_SKY_MIDDAY` from a guessed “daytime
Lookout” screenshot.

### Sun / directional light (live VS)

Pushed every landscape / static / PALSKIN first-seen draw.

| Register | Value | Source | Class |
|---|---|---|---|
| `c19` | `(0, 1, 0, 0)` | ctor `[esi+48]`; apply `00F39D40` writes `w=0` | **PROVEN** |
| `c20` | `(0.25, 0.25, 0.25, 1)` | record 0 `+0` = `0x3E800000` × 3 | **PROVEN** |
| `c35` | `(0, 0, 0, 1)` | `0098B2C0` stack default; apply does **not** store c35 | **PROVEN** |
| leftover `c3` | `(0, 0.125, 0, 0)` | per-cell table `0x0139C614`; fog flush restores `c2` only | **PROVEN** |
| formula | `max(n·−c19, 0)² * c20 + c35 + c3` | FG / static / PALSKIN VS `MAD` not `LIT` | **PROVEN** |
| family slot | **0** (`VSHADER_*_DIRLIGHT_FOG`) | packed count **0**; MainScene remap +32..+52 stay ctor zeros | **PROVEN** |
| lighting mode `[+18068]` | **1** | ctor; setter `00B23C00` has zero `E8` | **PROVEN** |

There is **no** separate ambient register. Invented ambient
`(0.52, 0.58, 0.68)` or `0.28+0.72*n·sun` is **DISPROVEN**.
Unlit faces are leftover **c3**, not a fill-in 1.

Host: `SilkEngineHost.Draw` → `WorldShading.DirLightDirection` /
`DirLightColor` / `LitColor` into `MeshPushConstants`. Formula
**EQUIVALENT**; lighting in FS not VS is **PARTIAL** (G).

### Fog

| Item | Value | Class |
|---|---|---|
| `D3DRS_FOGENABLE` | **1** (`00B46890` from landscape `00B67480` and MainScene `00B32AD0`) | **PROVEN** |
| `FOGCOLOR` | **`0xFF000000`** (record `(0,0,0,1)*255`) | **PROVEN** |
| `FOGTABLEMODE` / `FOGVERTEXMODE` | ctor **0** (NONE); VS `oFog` still blends | **PROVEN** |
| start / end | **1000 / 2000** (record `+80/+84`) | **PROVEN** |
| `c18` | `(0,0,0,1)` | **PROVEN** |
| `c2` | `00B47630` linear view-Z plane from camera **+276** (unscaled) | **PROVEN** |
| VS | `oFog = c0.y − min(dot(pos,c2), c0.y) * c18.w` | **PROVEN** |
| with first-seen `c0.y=1`, `c18.w=1` | `oFog = 1 − min(dot, 1)`; D3D saturates `[0,1]` | **PROVEN** |
| blend | `rgb*oFog + (1−oFog)*black` = `rgb*oFog` | **PROVEN** |
| Clear | same black as fog colour | **PROVEN** |
| 7000 | SKY_DEF flare radius slack, **not** fog end | **DISPROVEN** as fog |

`LandscapeFrustum.FogRecordStart/End/Color` and
`WorldShading.LinearFogPlane` are the locked first-seen payload.
Host push field is misnamed `CameraPos` (it is the plane).

### Sky

Native layer bit **`0x2000`** (else-path `00B662F0`, not `0x400000`).

| Item | First-seen | Class |
|---|---|---|
| Camera for sky | near **100**, far **10000**, minZ **0.99**, maxZ **1**, then `00B66190` | **PROVEN** |
| VS | `VSHADER_INNER_SKY` `dp4 oPos, v0, c5–c8` | **PROVEN** |
| Dome | `00B61DD0` 9×36 ellipsoid **6500 × 3250** at origin; cap/skirt 7000 / −500 / −10000 | **PROVEN** |
| Dome UV | `(this+16 − sin elev) * this+20`; first-seen +16/+20 = **0** → UV **(0,0)** | **PROVEN** |
| Dome colour | RGB white, alpha `max(1 − 1.105 cos(elev), 0)*255` | **PROVEN** |
| `stars.dat` billboards | `00B65A20` `[D+424]==0` → `ret 4` | **DISPROVEN** |
| Weather mesh | `00B64FA0` four ids 0 → `ret 4` | **PROVEN** skip |
| SKY_DEF textures | Sun 384, Star 401, flares 393–399 r=500–6000 | **PROVEN** authored |
| Inner PS `c0/c1/c2` | no `def`; writer unread | **UNREAD** |
| Host Concat | **no** `0x2000` | **DISPROVEN** vs native walk (G) |

`ENVIRONMENT` +292/+288/+296 (sky ctor `00B627E2`) are CString
first-dwords, first-seen 0. That is the video-options /
environment persist object, **not** WLD `EnvironmentDef`.

### Renderer / MainScene state

`00435530` Present order (locked): BeginScene, Clear black,
PlayerOverlay, PlayerInterface, Flush2D, `009DA9F0(1)` layers
`0x4 → 0x40 → 0x20 → 0x2000`, EndScene, Present.

| Bit | Submit | First-seen Lookout | Class |
|---|---|---|---|
| `0x4` | landscape BG `PSHADER_LANDSCAPE_BACKGROUND` `mul_x2 t0*v0` | yes | **PROVEN** |
| `0x40` | landscape FG `PSHADER_LANDSCAPE_FOREGROUND` `mul_x2 t1*v0` | yes | **PROVEN** |
| `0x20` | static-lit + (host) PALSKIN flatten | yes | **PROVEN** static; PALSKIN-in-`0x20` **DISPROVEN** vs native `0x80`/`0x100` |
| `0x80` / `0x100` | MainScene `00B33010` drains PALSKIN slots 14 / 8+10 | native yes; host no | **PROVEN** native |
| `0x2000` | inner sky else-path | native yes; host Concat no | **PROVEN** native / **DISPROVEN** host |
| `0x20000` | water | first-seen empty (`00B7A865 ret 4`) | **PROVEN** skip |
| Diffuse2X | `CEngineStateBlockDiffuse2X` / `0098B5E0` | yes | **PROVEN** |
| Cull | CCW on land/static; PALSKIN inherits | yes | **PROVEN** |
| Viewport | 1024×768, MinZ 0 MaxZ 1 | native yes | **PROVEN** |

Shared lighting setup `00B67480` (both land bits): `00B46C80` +
`00B46890` on **`0x1436E9C`** (the `00B482A0` object), then
`009881F0` identity-like 3×4.

### Runtime lights / effects / particles on Lookout

Lookout TNG has **288** things. Counts that matter:

| Kind | Count | Into MainScene? |
|---|---|---|
| `MARKER_LIGHT` | **0** | n/a |
| `PARTICLE_EMITTER_*` | **0** | n/a |
| `OBJECT_STREETLAMP_LIT_SINGLE_01` | **7** | C3D mesh **4978** yes; packed point-light **no** (first-seen count 0) |
| `HOLY_SITE_PLAYER_START` | 3 (`GuildArrivalHSP`, `LookoutPointHSP`, `MAIN_START_POSITION`) | spawn only |
| Creatures (villagers / traders / beggar / bully) | 8 | C3D / PALSKIN if Graphic |
| Script `CreateLight` / `CreateEffect` / `DummyEffect` | no first-seen Lookout script | **PROVEN** absent on this path |

Streetlamp def `#3095`:

- Sub `CLightDef` `#9180`: colour **`0xFF7D4725`** (ARGB 255,125,71,37),
  inner **4**, outer **6**. Qualifies `00B480E0` (`≥0.1` radii,
  channels `≥1/255`).
- Sub `CParticleAttacherDef` `#11459`.
- TNG persist: `CTCLight.Active=TRUE`, `Overridden=FALSE`. Colour /
  radii are **not** in the TNG; they live on the def.

`WorldShading` locks:

- Add-light is message **16** → `00B481E0` → `00B480E0`.
- **MARKER_LIGHT apply does not call that path.**
- Ctor packed count `[+160]=0`.
- `FirstSeenPackedLightCount = 0` → family slot 0 → 1-dirlight VS.
- `c31` atten flush only when packed count `> 1`.

So the 7 Active streetlamps are **authored lights** and **first-seen
meshes**, but they are **not** first-seen packed point lights.
Whether `CTCLight` on an OBJECT later sends message 16 is **UNREAD**.
Even if all 7 packed, remap is still slot **0** (MainScene
`00B34619` does not refill +32..+52) and the 2/4/5-light VS stay
off.

Particle create `006E0880` looks up `PARTICLE_EMITTER_NORMAL`, not a
C3D. Lookout has no placeable emitters. Streetlamp attacher spawn
on first Present is **UNREAD**; do not emit invented fire/glow
billboards.

---

## Trace: EnvironmentDef → manager → renderer

```
WLD 00507C30 token switch
  EnvironmentDef string at 0x01244D10
    sites 0050801E / 00508987  (NewRegion reader)
    also 0083C230 / 0083C2D0    (TimeToChangeEnvironmentDef persist)
  shipping FinalAlbion.wld: ZERO lines          DISPROVEN as payload

WLD NewRegion 1
  RegionName "LookoutPoint"
  RegionDef  "REGION_LOOKOUT_POINT"              PROVEN
  ContainsMap LookoutPoint, BowerstoneBridge, GuildExterior
  WorldFile.Parse stores RegionDef, not EnvironmentDef

game.bin REGION #7123
  +5 EnvironmentTheme = 2346
    → ENVIRONMENT_THEME1 (ENVIRONMENT_THEME_DAY, 2237 bytes)
  8 × TimeOfDay @ +13+269k : 0,4,5,10,12,18,20,21

game.bin ENVIRONMENT #2345
  persist CEnvironment / [0x1436E24]
  filename lightning_colours.tga
  sky ctor 00B627E2 copies +292/+288/+296 (first-seen 0)

CEngineLightingManager 00B482A0 @ 0x1436E9C
  record 0: c20=0.25, c35 src (0,0,0,1), fog (0,0,0,1), 1000/2000
  +224/+225/+226 = 0, +228 = 0xF, +160 packed = 0, +18068 mode = 1
  TOD blend 00B46C80 (land 00B67480 / FG bind / MainScene)
    +226==0 → copy record 0 → 00989830 dir  → 0098B2C0 c35
  fog 00B47630 → 009886C0 colour + 00988600 plane
  FOGENABLE 00B46890

CRenderManager 00B25950 / 00435530
  0x4 / 0x40 land  → 00B67480 lighting + fog
  0x20 static-lit  → family slot 0 DIRLIGHT_FOG
  0x80 / 0x100     → PALSKIN drain 00B33010
  0x2000 sky       → 00B662F0 else-path
  0x20000 water    → empty

host SilkEngineHost.Draw
  LinearFogPlane + DirLight* + LitColor
  Concat has 0x4, 0x40, 0x20; no 0x2000
```

RTTI (exe index): `CEnvironmentDef` `0x01376AC4`,
`CEnvironmentThemeDaySetDef` `0x01376AE4`,
`CEnvironmentThemeDef` `0x01377748`,
`CEnvironment` `0x0137FA44`,
`CEngineEnvironment` `0x0139ABB0`,
`CEngineLightingManager` `0x0139A2E8`.
Compiled `game.bin` type names are `ENVIRONMENT` /
`ENVIRONMENT_THEME_DAY` / `REGION` / `SKY`, not the `C*` RTTI
strings.

`0083C230` `TimeToChangeEnvironmentDef` is a **persist / change
timer** string (`0x01273028`), not the WLD first-frame bind.
First-seen use is **UNREAD** (likely save / script, not no-save
Lookout).

---

## Theme record 0 vs lighting record 0

Lighting record stride is **112** (`LandscapeFrustum.FogRecordStrideBytes`).
`ENVIRONMENT_THEME1` TOD records are **269** bytes.

Record 0 (offset 13..282) contains:

- `TimeOfDay = 0`
- `0x42C80000` = **100** at rec+25 (sky near, not fog start)
- several `1.0` (`0x3F800000`)
- `0x41700000` = **15** at rec+225
- `0x3E800000` = **0.25** at rec+233 (same bits as ctor `c20`)
- **no** `0x447A0000` (1000) and **no** `0x44FA0000` (2000)

So the authored theme is **not** a drop-in of the live fog record.
Treating `ENVIRONMENT_THEME1` floats as first-seen `c18` / start /
end is **DISPROVEN**. Whether some later unpack writes *other*
slots (sky textures, weather ids, ENVIRONMENT strings) is
**UNREAD**.

---

## Host vs native (environment only)

| Topic | Native first-seen | Host live | Class |
|---|---|---|---|
| WLD `EnvironmentDef` | parsed if present; **absent** in TLC WLD | token listed, not stored | **EQUIVALENT** absence |
| `REGION.EnvironmentTheme` | `ENVIRONMENT_THEME1` | not bound | **UNREAD** host |
| Dirlight `c19/c20/c35` | ctor record 0 | `WorldShading.*` push | **EQUIVALENT** |
| Fog plane / colour / 1000–2000 | `00B47630` | `LinearFogPlane` + black | **EQUIVALENT** |
| Ambient | none; leftover `c3` | GLSL adds same `c3` | **EQUIVALENT** leftover; **DISPROVEN** as ambient |
| Sky pass | `0x2000` | Concat missing | **DISPROVEN** host |
| Sky PS | `c0/c1/c2` unread | mode-2 stand-in | **UNREAD** / **TEMPORARY** |
| Packed point lights | count 0 | none | **EQUIVALENT** |
| Streetlamp C3D | 7 instances | primary C3D if Graphic | **PARTIAL** (mesh yes; light no) |
| Particles | no placeable emitters | none | **EQUIVALENT** empty |
| Green wash | leftover `c3` × `mul_x2` × `oT1=0` | same if oT1 stays 0 | **PROVEN** leftover; **DISPROVEN** as env tint |

---

## Do not do

| Guess | Why |
|---|---|
| Fog RGB `(0.52, 0.58, 0.68)` / end 7000 | ctor / record 0 is black, 1000/2000. 7000 is SKY_DEF. |
| Sun `(0.35, 0.25, 0.90)` or `0.28+0.72 n·L` | `c19=(0,1,0,0)`, `c20=0.25`, square N·L, plus `c3`. |
| Ambient fill so floors are not black | Floors (`n=+Z`) get leftover `c3=(0,0.125,0)`. |
| WLD `EnvironmentDef` on Lookout | Token unused in `FinalAlbion.wld`. |
| Bind `ENVIRONMENT_OAKVALE` because the map is “Oakvale-ish” | Lookout and Oakvale intro both use **`ENVIRONMENT_THEME1`**. |
| Midday sky because Lookout “looks daytime” | First-seen TOD bytes are 0; theme key 0 is not 12. Midday texture 391 is SKY_DEF’s named midday slot, not a proven first-seen bind. |
| `stars.dat` / invented star billboards | `00B65A20` empty-out. |
| Invented dome UV / r=1800 sphere at `(64,64,0)` | 6500×3250 at origin; UV (0,0). |
| Pack 7 streetlamps into 2LIGHTS VS | Packed count 0; remap slot 0. |
| Emit particle C3Ds for placeable / attacher | `006E0880` is `PARTICLE_EMITTER_NORMAL`; Lookout TNG has none. |
| First Present is SHOT2 / HerosOldHouse | That is persist / intro. No-save is Lookout + `006B3FF0`. |
| Tune green until a screenshot matches | Leftover `c3`, not environment. |

---

## Classification table

| Claim | Class | Evidence |
|---|---|---|
| No-save first Present is LookoutPoint index 1 | **PROVEN** | `docs/status/README.md`; `00501450` / `CurrentRegionIndex=1` |
| First *intro view* is Oakvale SHOT2 | **DISPROVEN** as no-save Present | same; `FirstSceneWorld` is a different contract |
| WLD `EnvironmentDef` present on Lookout | **DISPROVEN** | `FinalAlbion.wld` grep: 0 hits |
| Exe can parse `EnvironmentDef` | **PROVEN** | `00507C30` / `00508987` / `0x01244D10` |
| Host `WorldFile` stores `EnvironmentDef` | **DISPROVEN** | `WorldFile.cs` region block has no token |
| `REGION_LOOKOUT_POINT.EnvironmentTheme` = `ENVIRONMENT_THEME1` #2346 | **PROVEN** | `game.bin` CRC `EnvironmentTheme` → 2346 |
| `REGION_OAK_VALE_INTRO` uses the same theme | **PROVEN** | same field |
| `ENVIRONMENT_THEME1` has 8 TOD keys 0/4/5/10/12/18/20/21 | **PROVEN** | `Time` count 8, `TimeOfDay` every 269 bytes |
| Theme record 0 is the live fog record | **DISPROVEN** | no 1000/2000 immediates; size 269 vs 112 |
| First-seen TOD bytes 0 → copy lighting record 0 | **PROVEN** | `00B482A0` + `00B46C80` `jbe 00B46E17` |
| Live dirlight `c19/c20/c35` and fog 1000/2000 black | **PROVEN** | ctor + `WorldShading` / `ShaderFormatTests` / `CameraProjectionTests` |
| Separate first-seen ambient | **DISPROVEN** | leftover `c3` only |
| Packed point-light count 0 | **PROVEN** | ctor `[+160]`; add-light not first-seen |
| Lookout TNG has 0 `MARKER_LIGHT` / 0 particle emitters | **PROVEN** | TNG census 288 |
| 7 streetlamps Active with `CLightDef` 7D4725 / 4 / 6 | **PROVEN** authored | TNG + game.bin #9180 |
| Those 7 are first-seen packed lights | **DISPROVEN** as first-seen pack | `FirstSeenPackedLightCount=0` |
| Streetlamp `CTCLight` later calls `00B480E0` | **UNREAD** | apply site not recovered for OBJECT |
| Streetlamp `CParticleAttacherDef` emits on first Present | **UNREAD** | |
| Sky else-path `0x2000` + dome + UV 0 + star/weather skip | **PROVEN** | `SkyPass` / `WorldGeometryTests` |
| Sky PS `c0/c1/c2` values | **UNREAD** | `FirstSeenSkyPsC2HasWriter=false` |
| Host submits sky on first Lookout Concat | **DISPROVEN** | G: no `0x2000` |
| `ENVIRONMENT` +288/+292/+296 first-seen 0 | **PROVEN** | persist `004310A7` / ctor `0099AED0` |
| `TimeToChangeEnvironmentDef` on first frame | **UNREAD** | persist family `0083BF90` |
| Water env-map / flares / local detail | **UNREAD** / first-seen skip | PARITY leftover 18 |
| Green is missing environment colour | **DISPROVEN** | leftover `c3` × `mul_x2` × `oT1=0` |

---

## Implement next (not this note)

1. Keep live Lookout lighting/fog as **ctor record 0** (`c19/c20/c35`,
   black fog, 1000/2000). Do not replace them with a theme unpack
   until an apply site is dumped.
2. Bind `REGION_LOOKOUT_POINT` → `ENVIRONMENT_THEME1` as the
   **current theme name** only after `SetRegionAsLoaded` is shown to
   do that. Do not bind `ENVIRONMENT_OAKVALE`.
3. Submit native sky **`0x2000`** (dome, UV 0, no stars.dat). Leave
   PS `c0/c1/c2` unread — no invented `*c2=0`.
4. Draw 7 streetlamp **meshes**. Do not switch to 2LIGHTS VS.
5. Do not emit Lookout particle gizmos.
6. Do not retune green / ambient / fog to a screenshot.
