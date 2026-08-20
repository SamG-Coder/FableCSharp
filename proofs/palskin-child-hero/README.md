# Audit: kid PALSKIN vs leftover (`WorldShading` / `XSeqFile`)

Investigation only. No production `src/` edits.

Question: leftover still says mixer eval `00AA0090` unread,
`PaletteForPose` drops `time`, and submit CPU-skins dest. Does
current host MATCH native first-seen kid PALSKIN, or what remains
to draw childhood hero **4300**?

Authority: `src/Fable.Formats/WorldShading.cs`;
`src/Fable.Formats/Anims/XSeqFile.cs`;
`src/Fable.Formats/Meshes/MeshFile.cs`;
`src/Fable.Render/MeshBatches.cs`;
`src/Fable.Game/EngineLifecycle.cs`;
`src/Fable.Game/RegionTravel.cs`;
`src/Fable.Formats/Scene/ScenePass.cs`;
tests `XSeqFormatTests`, `MeshFormatTests.Kid_c3d_*`,
`WorldGeometryTests.Palskin_submit_uses_file_triangles_not_repose`,
`GameBinFormatTests`, `EngineLifecycleTests`;
listings `00AA0090` / `00A52650` / `00BD2F91` / `00BD71B0` /
`00BD76D2`; leftover `docs/status/investigations/E-player-palskin.md`,
`2026-08-18-palskin.md`, `proofs/anim-blend-first`,
`proofs/audit-xseq`, `proofs/xseq-walk-first`.

Status words: **MATCH** / **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Do **not** invent a CPU-skinned dest as the draw.

---

## Verdict

**Kid PALSKIN *format* MATCH. Kid PALSKIN *draw* is leftover vs
live New Game. Mixer eval `00AA0090` is still UNREAD.**

| Claim leftover used | Current host | Class |
|---|---|---|
| `PaletteForPose` drops `time` / `ApplyFirstLocals` | `FloorKey(time)` → `ApplyLocals` | leftover **stale**. Host **MATCH** floor key |
| `TrySample` ignores `time` | `FloorKey` then `RotationAt` | leftover **stale**. Host **MATCH** floor |
| XSEQ first stored quat only | `XSeqTrack.RotationKeys` all `f32[4]` | leftover **stale**. Host **MATCH** store |
| Submit `TrianglesForPose` CPU re-skin | `MeshBatches` uses `MeshFile.Triangles` | leftover **stale**. First-seen dest **MATCH** file |
| `00AA0090` unread entirely | addr locked; body not evaluated | leftover **still UNREAD** as mixer |
| `00A52650` unread | `WorldShading.TimeToKey` | **MATCH** listing. Units **PARTIAL** |
| `FirstSeenPlayAnimationAppliesPose` | stays `false` | **MATCH** first-seen |
| Kid **4300** is no-save Present | live Graphic **4299** `MESH_HERO` | **DISPROVEN** as this Present. **4300** is Oakvale leftover |
| First-seen PALSKIN draw Flag1 | `00BD76D2` `or ebx, 5`; hair Flag1=1 | **MATCH** read. Slot `0x200` **UNREAD** |

Childhood hero **render** leftover is not dest math. It is:
Oakvale spawn of `CREATURE_HERO_CHILD` / 4300, mixer lerp
(`00AA0090` + `00A4C1F0` frac), Flag1 extra drain, GPU `c38`.
File triangles already *are* bind dest (`00A9E1E0` × IBM).

---

## 1. Two meshes (do not mix)

| Id | Def | C3D | Bones | First-seen stride | Site |
|---|---|---|---|---|---|
| **4300** | `CREATURE_HERO_CHILD` / `CREATURE_YOUNG_HERO` | `MESH_YOUNGHERO_02` | **76** | 28 / flags `0x14` | Oakvale intro leftover (`00DBDE40`) |
| **4299** | `CREATURE_HERO` / `CREATURE_HERO_TRAINING` | `MESH_HERO` | **77** | prim0 **36** / flags **22** | live no-save Lookout (`006AC910`) |

`GameBinFormatTests` locks both ids. `Kid_c3d_stores_hair_flag1_and_bones`
locks 4300: bone 0 `Scene Root` parent −1 identity; `Bip01` parent 2;
all first-seen palettes near-identity (`FirstSeenPlaysAnim=false`).

`WorldShading.FirstSeenPalskinStrideBytes=28` is the **kid** file
field (`00A8FD40`), not adult 4299. Father is 20 / flags 4. Live
Present does **not** draw 4300 (`EngineLifecycle.HeroMeshId=4299`,
`SubmittedPalskinMeshIds` contains 4299). `FirstSceneWorld` /
`WorldGeometry.PlayerMeshId=4300` is the click-intro fixture.

**DISPROVEN:** draw 4300 on Lookout. **LEFTOVER:** childhood scene
is `Q_NewOakValeIntro` / `StartOakVale`, not `Q_SunnyvaleMaster`.

---

## 2. First-seen dest = file triangles, not CPU re-skin

Native packer `00BD2D90` → evaluate `00AA0090` (first-seen channel
count **0**) → `00A9E1E0` hierarchy × IBM. x87 sibling `00BD2F91`
is `dest = S * C3D`. With no clip that product is identity, so
positions **equal bind**.

Host:

```
MeshBatches.BuildMeshes
  // First-seen dest is already in
  // MeshFile.Triangles (00A9E1E0 × IBM).
  var source = mesh.Triangles;
  layer = BoneCount > 0 ? Palskin : Prop;   // bit 0x100
```

`Palskin_submit_uses_file_triangles_not_repose` asserts
`built.Vertices.Length == mesh.Triangles.Count * 3` and
`PassBit == 0x100` on 4299. Kid bind-pose tris MATCH the same
rule (`Kid_c3d`: `TrianglesForPose()` count/verts equal
`Triangles` at time 0).

`TrianglesForPose` remains a **format helper** (`XSeqFormatTests`
wake 3420 vs 4300). Submit does not call it.

Stale leftover still in `MeshFile` comments (“Submit re-skins
these via `TrianglesForPose`”). That comment is **LEFTOVER** vs
`MeshBatches`. Do not re-introduce CPU dest as the draw.

`TrianglesForPose(XSeqFile)` still passes **`time=0f`** into
`PaletteForPose`. Floor-key at t=0 is key 0. That helper is
not the first-seen path.

---

## 3. `00A52650` time→key **MATCH**; `00AA0090` mixer **UNREAD**

`WorldShading.TimeToKeyFn = 0x00A52650`. Listing
(`proofs/anim-blend-first` §2b):

```
time * [clip+80]  → integer key + frac
key %= [clip+84]
```

Host:

```
TimeToKey(time, rate, wrap)
  scaled = time * rate
  key = floor(scaled); frac = scaled - key
  return (key % wrap, frac)
```

`XSeqFormatTests.Xseq_persist_addrs_match_*` locks `(0, 0)` at
t=0 rate 15 wrap 8; mid key 1 at t=0.1 rate 15 wrap 30; wrap of
`TimeToKey(2, 15, 15).Key == 0`.

`XSeqFile.FloorKey`:

```
period = Tracks[0].FrameCount & 0xFF
TimeToKey(time, SamplesPerSecond, period).Key   // Frac discarded
```

`PaletteForPose(bones, clip, time, sequence)`:

```
if sequence null or no tracks → FirstSeenPalettes(bones)
else FirstSeenPalettes(sequence.ApplyLocals(bones, FloorKey(time)))
```

`ApplyLocals` / `TrySample` / `RotationAt(key)` index
`RotationKeys` (all stored quats). Translation stays
`FirstTranslation`.

Leftover (`E-player-palskin`, `2026-08-18-palskin`,
`anim-blend-first` §4, `audit-xseq` §7) still writes
“`PaletteForPose` drops `time`” / “`ApplyFirstLocals`”.
That is **stale**. `WorldShading` summary text still says
“samples the first stored key” — leftover **comment** vs
`FloorKey` **code**.

Still **UNREAD** / **PARTIAL** vs native mixer:

| Native `00AA0090` | Host | Class |
|---|---|---|
| mixer `bank+960`, `n*48` scratch | none | **UNREAD** |
| `00A9F2F0` header lerp of two sources | none | **UNREAD** |
| 20-byte channels `+8` index / `+12` time / `+16` weight | none | **UNREAD** |
| `00A52650` then `00A4C1F0` slerp of two 16-byte keys | floor key only; `Frac` unused | **PARTIAL** |
| `[clip+80]` / `[clip+84]` | XSEQ fps + `FrameCount&0xFF` | **PARTIAL** (host comment: does not map fps onto +80) |
| first-seen channel count 0 → `00AA097D` + `00A9E1E0` | `FirstSeenPalettes` bind locals | **MATCH** first-seen |

`XSeqFile.HierarchyFn = 0x00AA0090` is the addr lock, not the
eval.

---

## 4. `FirstSeenPlayAnimationAppliesPose` stays false

`RegionTravel.FirstSeenPlayAnimationAppliesPose = false`.
`WorldShading.FirstSeenPlaysAnim = false`.
`FirstSeenAppearancePlaysDefault = false` (`005B37F7` only
clothing GUI / `PC_UI_FRAME`, not create `006AC910`).

`XSeqFormatTests` asserts the flag after 3420 first-key **moves**
kid dest off bind. That is a format experiment. Engine submit
does not apply it. Wake loop 3420
(`CS_OAKVALE_DREAM_INTRO_YOUNG_HERO_WAKING_UP_LOOP`) is Oakvale
leftover, not Lookout.

Keep the flag **false**. Wiring `PaletteForPose` into submit
would be a later clip product, not first-seen dest.

---

## 5. First-seen PALSKIN draw Flag1 **MATCH** read, **UNREAD** extra slot

C3D material serialize `00ABF6B0`, Flag1 at +41.

Kid 4300: only **hair** Flag1=1, MapFlags=1.
`PalskinTypeIndex(1, 0, 0xFF, 1) == 4`
(`FirstSeenPalskinHairTypeIndex`).

Draw `00BD71B0` at `00BD76D2` / `00BD7705`: opacity `0xFF`
skips `[inst+12]` bit-9 fill; Flag2 → `or ebx, 2`; else Flag1
→ `or ebx, 5`. `FirstSeenPalskinReadsFlag1=true`.
Static-lit does **not** read Flag1.

Flag1 does **not** pick cull (`FirstSeenAppliesCullNoneFromFlag1=false`,
PALSKIN inherits CCW) and does **not** pick blend
(`FirstSeenFlag1SelectsAlphaBlend=false`; SRCALPHA/INVSRCALPHA
anyway). First-seen bind pass 4 does **not** consume helper+28
(`FirstSeenPalskinBindUsesHelperTypeIndex=false`).

Queue: type 0 → slot 8; Flag1 extra → slot **9**. MainScene
`00B33010` drains 8+10 on `0x100`, slot 14 on `0x80`, Flag1
slot 9 on `0x200` after sky.

Host `ScenePasses.DrawnPasses(Palskin)` is **`0x100` only**.
Type1/Flag1 routing stays leftover research (`docs/status`
UNREAD, `f4a1efc`). Kid hair would need slot 9 / bit `0x200`
to MATCH native extra drain. Geometry soup on `0x100` is
**PARTIAL** vs Flag1 hair.

---

## 6. What remains for childhood hero render

Needed for **4300** on the Oakvale leftover scene, not Lookout:

| Remaining | Class | Why |
|---|---|---|
| Spawn `CREATURE_HERO_CHILD` / Graphic 4300 (`00DBDE40`) | **LEFTOVER** vs live 4299 | live `ResolveHeroDefinition` is `CREATURE_HERO` |
| Mixer eval `00AA0090` (frac, channels, `00A4C1F0`) | **UNREAD** | `FloorKey` is not slerp |
| `[clip+80]` / `[clip+84]` units | **PARTIAL** | fps + `FrameCount&0xFF` is a host map |
| Translation keys beyond first i16×factor | **UNREAD** | `ApplyLocals` uses `FirstTranslation` |
| Flag1 slot 9 / layer `0x200` | **UNREAD** | host PALSKIN is `0x100` |
| Type1 slots 10+14 / `0x80` | **UNREAD** | same |
| GPU `c38` dest upload `00BCFB00` | later GPU path | do not file; do not CPU-skin dest |
| Runtime pose apply | flag stays false | first-seen channel count 0 |
| `CSkeletalMorphDef` on kid | **UNREAD** | kid-only; n/a adult 4299 |
| Hair attach `MESH_HERO_HAIR_YOUNG_01` **4275** | **UNREAD** attach | Graphic 4126 is **not** the worn mesh |
| `00AA0090` mixer object / 20-byte channels | **UNREAD** | `MeshBank` has no mixer |

Not remaining (do not invent):

- CPU `TrianglesForPose` as the first-seen dest.
- Kid 4300 on Lookout `PLAYER_HERO` miss (`CREATURE_HERO` 4299).
- Flag1 as NONE cull or as the SRCALPHA selector.
- `005B37F7` DEFAULT play on create.

---

## MATCH vs remaining UNREAD (kid PALSKIN)

**MATCH**

- 4300 file: 76 bones, stride 28, flags `0x14`, hair Flag1=1 / MapFlags=1 / type index 4.
- Bind dest = hierarchy × IBM; first-seen palettes identity; submit `MeshFile.Triangles`.
- PALSKIN layer `0x100` (slots 8+10); VS family slot 0 `VSHADER_PALSKIN_DIRLIGHT_FOG`.
- Draw reads Flag1 (`00BD76D2`); does not pick cull/blend.
- `TimeToKey` `00A52650` listing; `PaletteForPose` / `TrySample` / `FloorKey(time)`.
- `XSeqFile` stores **all** rotation keys; `RotationAt` wraps by count.
- `FirstSeenPlayAnimationAppliesPose=false`; `FirstSeenPlaysAnim=false`.
- Adult live Graphic 4299 vs kid 4300 split (defs **PROVEN**).

**UNREAD** (childhood hero leftover)

- `00AA0090` mixer eval (header lerp, 20-byte channels, key slerp, frac).
- Clip rate/wrap field units vs XSEQ fps.
- Pos keys after first.
- Flag1 extra slot 9 / `0x200` (and type1 `0x80`).
- GPU `c38` (later; not a CPU dest).
- Oakvale `CREATURE_HERO_CHILD` spawn + morph + hair modifier attach.
- Applying a clip to first-seen dest (flag stays false).
