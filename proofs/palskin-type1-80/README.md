# PALSKIN type1 `0x80` unread on kid 4300 vs Present 4299

Investigation only. Production `src/` was not edited.

Do **not** swap first Present to kid **4300**.
Do **not** invent kid **4300** on Pump / `SubmitCurrentWorld`.
Kid **4300** is a `FirstSceneWorld` / `WorldGeometry.IsPrimaryStart`
fixture, not `EngineLifecycle.Pump`.
`FirstSeenHandsPlayerControl=false`.

Question: what is PALSKIN **type1** bit **`0x80`**? How does
first-seen **apply** that bit on Pump Graphic **4299** versus
fixture Graphic **4300**?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH** / **DIVERGE**.

Authority: `proofs/hero-palskin-first-submit`;
`src/Fable.Formats/Meshes/MeshFile.cs`;
`src/Fable.Game/WorldGeometry.cs`;
`docs/render/FIRST_SCENE_CONTRACT.md`;
C3D **4300** (`Kid_c3d_stores_hair_flag1_and_bones`,
`Kid_4300_flag1_hair_drains_0x200_after_sky`);
listings `00BD780D` / `00B33010`
(`palskin-queue-slots-00bd7838-00bd780d.md`);
`WorldShading.cs` / `ScenePass.cs` / `InstanceDraw.cs`
(read only); siblings `palskin-type1-0x80-kid`,
`palskin-type1-0x80-4300`, `palskin-first-present-id`,
`palskin-inst104-writer`.

---

## Verdict

**Type1 `0x80` is MainScene drain of prim-queue slot 14
after sky.** `00BD780D` fills that slot only when
`[[inst+104]+8] == 1`. It is **not** C3D bank type 5,
not material Flag1, not helper `+28` type index 4, not
`Duration=1`.

**First no-save Present PALSKIN is Lookout adult Graphic
4299 `MESH_HERO` (`CREATURE_HERO` at `GuildArrivalHSP`).**
Kid **4300** `MESH_YOUNGHERO_02` is the Oakvale
`FirstSceneWorld` fixture. Do **not** swap those Graphics.

On **4300**, type1 `0x80` stays **UNREAD as a submit**.
The C3D file is type0 + Flag1 hair (`0x100` then `0x200`
after sky). Live `[[inst+104]+8]` is **UNREAD as 1**.
Host never emits `0x80` for 4300 — **MATCH** skip.

On **4299**, native PALSKIN **family** walks bits `0x80` /
`0x100` through `00BD71B0`. Filling slot 14 still needs
type==1; that live dword on 4299 is **UNREAD**. Host Pump
`DrawnPasses` emits only `0x100` (**DIVERGE** vs native
drain visit of `0x80`, **not** a 4300 layer).

`FIRST_SCENE_CONTRACT` still lists kid **4300** as the
intro-view asset and folds PALSKIN under bit `0x20`.
That contract is leftover **#4** vs Pump Present. Do not
use it to move first Present onto 4300 or to invent a
4300 DIP on `0x80`.

| Claim | Class |
|---|---|
| Type1 `0x80` = slot **14** drain after sky when `[[inst+104]+8]==1` | **PROVEN** |
| First Present PALSKIN Graphic is **4299** at `GuildArrivalHSP` | **PROVEN** |
| First Present PALSKIN Graphic is kid **4300** | **DISPROVEN** |
| Kid **4300** is `FirstSceneWorld` fixture, not Pump | **PROVEN** (host). **LEFTOVER** as first Present |
| Type1 dword on 4300 is 1 | **UNREAD** as 1 |
| First-seen 4300 submit on `0x80` | **skip** / **UNREAD** as a 4300 DIP |
| `FirstSeenHandsPlayerControl` | **false** (**MATCH**) |
| Swap first Present to kid to “apply” `0x80` | **DISPROVEN** |

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| What is PALSKIN type1 `0x80`? | Drain of queue slot **14** after sky (`00B33010` `cmp eax, 0x80` → `00B849F0(14)`). Filled only on queue type **1**. | **PROVEN** |
| Is type1 C3D 4300 / bank type 5 / Flag1 / helper+28? | **No.** 4299 and 4300 are both type 5. Flag1 is type0 slot **9** / `0x200`. Helper+28 is hair MapFlags index 4. | **DISPROVEN** |
| First-seen apply on **4299**? | Pump Present Graphic. Native family **visits** `0x80`+`0x100`. Live type dword **UNREAD**. Host only `0x100`. | **PROVEN** Graphic. Type **UNREAD**. Host **DIVERGE** |
| First-seen apply on **4300**? | Fixture C3D is type0 + Flag1 hair. `0x80` not submitted. Type dword **UNREAD as 1**. | **MATCH** skip. **UNREAD** as DIP |
| Swap first Present to kid? | **No.** Lookout `CREATURE_HERO` **4299**. Hands stay off. | **DISPROVEN** |

---

## 1. Type1 `0x80` meaning — recovered

Queue inside PALSKIN draw `00BD71B0` at `00BD780D`
(`hero-palskin-first-submit` §4; listing
`palskin-queue-slots`):

```
00BD77FE  mov eax, [edi+104]        ; inst+104 (pointer)
          test eax, eax
          je  skip
00BD780D  mov eax, [eax+8]          ; [[inst+104]+8]
          sub eax, 0
          je  type0                 ; 00BD789C → slot 8
          dec eax
          jne skip                  ; not 0/1
; type1:
          00BCE740 helper
          00B84720(10)              ; slot 10 → bit 0x100
          00BCE740 helper
          00B84720(14)              ; slot 14 → bit 0x80 after sky
```

Type0 (`00BD789C`) then reads C3D material Flag1
(`[mat+41]` at `00BD78D9`) and may add slot **9**.
Type1 **never** reads Flag1.

Drain `00B33010`:

```
cmp eax, 0x100  → slots 8 then 10     ; before sky
cmp eax, 0x2000 → sky else-path
cmp eax, 0x80   → slot 14             ; after sky
cmp eax, 0x200  → slot 9              ; Flag1 extra, after sky
```

`WorldShading`: `PalskinQueueSlotType1A=10`,
`PalskinQueueSlotType1B=14`, type0=8, Flag1 extra=9.
`ScenePasses.FirstSeenLayers` walks `0x80` as
“PALSKIN type1 slot 14” (registration index 25).
`DrawnPasses(Palskin, flag1)` **omits** `0x80`
unless the live type dword is 1 — that gate is
**not** wired on host (`ScenePass.cs`: “Type1 slot
14 / `0x80` stays off unless `[inst+104]+8==1`”).

`MeshFile` does **not** encode queue type. Bone C3Ds
stamp `SceneLayer.Palskin`; Flag1 copies onto
triangles for the type0 extra slot (`MeshFile.cs`:
“Flag1 is extra drain slot 9 / `0x200` after sky”).
`SceneLayer` comment lists PALSKIN `0x100` and Flag1
`0x200` — not `0x80`. That is the file-side skip of
type1, not a missing prim.

Not these aliases:

| Tempting name | Why not |
|---|---|
| C3D bank type 5 | 4299 and 4300 are both type 5 |
| Material Flag1 / hair | Type0 slot **9** / `0x200` |
| Helper `+28` type index 4 | `PalskinTypeIndex`; first-seen bind does not read it |
| `Duration=1` / XSEQ / `ApplyInner` | Integer after `sub 0` / `dec` |
| FIRST_SCENE_CONTRACT bit `0x20` kid | Static slot 0. PALSKIN is not `00BB2540` |

---

## 2. First Present is 4299, not 4300

`proofs/hero-palskin-first-submit`:

```
Leave frontend
  Init World / Init Characters
    00449D90 PLAYER_HERO miss → CREATURE_HERO
  later 00501450 LookoutPoint
    HOLY_SITE_PLAYER_START GuildArrivalHSP
      006AC910 CThingPlayerCreature::Create
        004CA010 Graphic field 4299
      NO 00A243B0 / NO 00BD71B0
    first 00A243B0(id=4299) type 5
    00B27D90
      0x80 / 0x100 PALSKIN → 00BD71B0     // FIRST Hero PALSKIN submit
```

`CREATURE_HERO_CHILD` / 4300 / `00DBDE40` are **not**
on that list. **PROVEN**.

Host Pump (`EngineLifecycleTests`):

```
HeroMeshId == 4299
SubmittedPalskinMeshIds contains 4299
SubmittedWorld.Expanded == false
SubmittedWorld.Triangles empty          // PresentWorld handles
```

`game.bin`: `CREATURE_HERO` / `CREATURE_HERO_TRAINING`
→ Graphic **4299** `MESH_HERO`. `CREATURE_HERO_CHILD` /
`CREATURE_YOUNG_HERO` → Graphic **4300**
`MESH_YOUNGHERO_02`. Two defs, two C3Ds.

`FirstSeenHandsPlayerControl=false`
(`RegionTravel`, `WorldSceneTests`,
`EngineLifecycleTests`). First-seen does **not**
hand WASD / `00446A30`. Kid 4300 is not a playable
first Present, and `0x80` is not a control-handoff
layer. Do not invent hands on 4300 to justify a
type1 DIP.

---

## 3. Kid 4300 is FirstSceneWorld fixture, not Pump

`FirstSceneWorld` type comment:

> Reconstructed intro-view fixture:
> `StartOakValeWest` / `CAM_OVIF_SHOT2` /
> `ScriptRuntime.StartNewGame` / `WorldGeometry.Build`.
> Not `EngineLifecycle.Pump` (no-save Present is
> LookoutPoint). Do not collapse leftover #4.

`WorldGeometry.Build` injects the kid only when:

```
existingHero = first CREATURE_HERO / _TRAINING / _CHILD
if existingHero
    PlayerMeshId = that Graphic          // Lookout: 4299
else if IsPrimaryStart(region)           // StartOakValeWest
         && FindPlayerStart
    CloneAs(NOVStartHSP, CREATURE_HERO_CHILD)
    PlayerMeshId = 4300
```

Pump `PresentWorld` is `expandGeometry: false` on
LookoutPoint and already has adult `CREATURE_HERO`.
The clone **does not run**. `WorldGeometryTests`
locks `PlayerMeshId==4300` on the Oakvale soup
only. `FirstSceneWorld.Build` has **no** production
callers (`PumpFrontendFrame` / `SubmitCurrentWorld`).

`FIRST_SCENE_CONTRACT` ASSETS row “Kid | C3D **4300**
… bind-pose” is that intro-view contract
(`FirstSeenPlaysAnim=false`). FRAME still lists
“bit `0x20` primitives (static + PALSKIN)” and
SHADERS “Primitives 0x20 kid |
`VSHADER_PALSKIN_DIRLIGHT_FOG`”. Recovered
`ScenePasses.FirstSeenLayers` split static `0x20`
from PALSKIN `0x100` / type1 `0x80` / Flag1 `0x200`.
The contract fold is **LEFTOVER**. It does **not**
move first Present onto 4300, and it does **not**
make type1 `0x80` a 4300 submit.

---

## 4. First-seen apply — 4299 vs 4300

C3D **4300** (`MeshFile.Parse` / `Kid_c3d_*`):

```
#4300 type=5 MESH_YOUNGHERO_02  bones=76  prims=4
  stride 28  initFlags 0x14     ; float3 pos + 8 blend + packed n/UV
  face / torso / mouth          Flag1=0
  Young Hero Hair               Flag1=1 MapFlags=1
  all triangles SceneLayer.Palskin
  FirstSeenPalettes ≈ I         ; 00A9E1E0 × IBM, 0 mixer channels
```

`0x14` = `(4|0x10)`: packed-pos test fails because
bit `0x10` is set; packed-norm stays on. Blend index
at +12, weight +16, packed n +20, packed UV +24
(`MeshFile.PalskinBlendIndexOffset`). Dest identity
because `FirstSeenPlaysAnim=false`, not because this
Graphic is Pump.

C3D **4299** (`hero-palskin-first-submit` live file):

```
#4299 type=5 MESH_HERO  bones=77  prims=19
  prim0 stride 36 flags 22 group 9
  First dest bind identity
  submit set [4299]
```

`WorldShading.FirstSeenPalskinStrideBytes=28` is the
**4300** file field. Adult prim0 **36** is a leftover
name vs this Present (`hero-palskin-first-submit`
host table). Do not apply the kid stride to 4299.

Apply table:

| | Pump **4299** | Fixture **4300** |
|---|---|---|
| Site | `SubmitCurrentWorld` Lookout | `FirstSceneWorld.Build` Oakvale |
| Thing | `006AC910` `CREATURE_HERO` `GuildArrivalHSP` | `WorldGeometry` `CloneAs(NOVStartHSP, CHILD)` |
| C3D | 77 bones / 19 prims / prim0 36/22 | 76 bones / 4 prims / 28/`0x14` |
| Native PALSKIN family | `0x80` + `0x100` through `00BD71B0` | type0 `0x100` + Flag1 `0x200`; slot 14 empty |
| Live `[[inst+104]+8]` | **UNREAD** as 1 | **UNREAD** as 1 |
| Host `DrawnPasses` | `[0x100]` only | `[0x100]` body, `[0x100,0x200]` hair; **no `0x80`** |
| Host vs native | **DIVERGE** (missing `0x80` visit) | **MATCH** skip of slot 14 |
| `SubmittedPalskinMeshIds` | contains **4299** | not on Pump |
| Hands | `FirstSeenHandsPlayerControl=false` | same; fixture is not control |

Tests that lock the 4300 skip:

```
Kid_4300_flag1_hair_drains_0x200_after_sky
  Contains 0x100 and 0x200
  DoesNotContain 0x80
WorldPipelineTests FirstSceneWorld MeshBatches
  DoesNotContain 0x80
MeshFormatTests Kid_c3d DrawnPasses(Palskin, 1)
  [0x100, 0x200]  DoesNotContain 0x80
Palskin_submit_uses_file_triangles_not_repose (4299)
  Contains 0x100  DoesNotContain 0x80
InstanceDraw.PalskinPassBit80
  "Not first-seen 4300 ([inst+104]+8 unread as 1)"
```

If 4300 were type1, Flag1 would **drop** slot 9 and
hair would land on `0x80` instead of `0x200`. First-seen
MATCH cannot be both (`palskin-type1-0x80-kid`).

Registration still **walks** bit `0x80`. Empty drain of
slot 14 is not a kid DIP. Closing the 4299 host DIVERGE
by inventing 4300 triangles on that bit would swap
Present Graphics. **DISPROVEN**.

Writer of `[[inst+104]+8]` stays **UNREAD**
(`palskin-inst104-writer`: pointer copy `00BD2920`
is PROVEN; type dword store is not). Do not treat
ctor-zero of the *pointer* as type0, and do not
invent type1 on 4300 to fill `0x80`.

---

## 5. Host vs native

| Host | Native first-seen | Class |
|---|---|---|
| Pump palskin **4299** at `GuildArrivalHSP` | Lookout adult `00BD71B0` | **MATCH** Graphic. **DISPROVEN** as 4300 |
| `FirstSceneWorld.PlayerMeshId=4300` | Oakvale intro leftover | **PROVEN** fixture. **DISPROVEN** as Pump |
| `DrawnPasses` no `0x80` on 4300 | type0 skip of slot 14 | **MATCH** skip. Type dword **UNREAD** as 1 |
| `DrawnPasses` only `0x100` on Pump 4299 | native also visits `0x80` | **DIVERGE** vs 4299. Not a 4300 layer |
| `FIRST_SCENE_CONTRACT` kid on `0x20` | PALSKIN is `00BD71B0`, not `00BB2540` | **LEFTOVER** fold |
| `MeshFile` Flag1 → `0x200` after sky | type0 extra slot 9 | **MATCH** on 4300 hair |
| `FirstSeenHandsPlayerControl=false` | no first-seen `00446A30` | **MATCH** |
| File tris as dest | empty-channel pack ≈ I | **MATCH** first-seen dest |

---

## Do not

- Swap first Present from Lookout **4299** to kid **4300**.
- Wire `FirstSceneWorld.Build` / `WorldGeometry` kid clone
  into `PumpGameUpdate` / `SubmitCurrentWorld`.
- Invent a 4300 DIP on type1 `0x80` (hair, mouth, second
  body, clothing 4126, `Duration=1`, hands).
- Treat Flag1 `0x200` as type1 `0x80`.
- Fold type1 into `DrawnPasses` without `[[inst+104]+8]==1`.
- Use `FIRST_SCENE_CONTRACT` “0x20 kid” to collapse leftover
  #4 or to submit PALSKIN as static `00BB2540`.
- Apply kid stride 28 / flags `0x14` as adult prim0 (36/22).
- Turn `FirstSeenHandsPlayerControl=false` into a 4300
  control Present.

Next leftover on **4300** is still the **writer** of
`[[inst+104]+8]` (type1 `0x80` **UNREAD** as a submit)
and Oakvale spawn, not a Pump CreateCharacter.
On Pump the leftover is 4299 per-prim + `c38` / host
`0x80` visit, not this id.
