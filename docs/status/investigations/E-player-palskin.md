# E — Player / PALSKIN path (investigation)

**Scope:** recover the real player → appearance → PALSKIN → DX9 draw
chain. Host-only audit. No production edits. No Vulkan PALSKIN hacks.

**Verdict:** the no-save player Thing is real (`CREATURE_HERO` / mesh
**4299** / `MESH_HERO`). The *draw* that currently claims
`SubmittedHeroPalskin` is **not** character rendering. It is a CPU
bind-pose flatten of dest verts stuffed into the same static C3D
triangle soup as Lookout props. That path must not masquerade as
PALSKIN.

---

## Masquerade (DISPROVEN as character render)

`EngineLifecycle.SubmitCurrentWorld` extra-submits `HeroMeshId=4299`
when `PresentWorld` missed the instance, then:

```69:99:src/Fable.Render/MeshBatches.cs
    public static TexturedMesh BuildMeshes(
        IReadOnlyList<(MeshFile Mesh, Matrix4x4 Transform)> instances)
    {
        ...
            var source = mesh.BoneCount > 0
                ? mesh.TrianglesForPose()
                : mesh.Triangles;
            foreach (var tri in source)
            {
                var a = Vector3.Transform(tri.A, transform);
                ...
```

What this actually does:

1. `TrianglesForPose()` with **no clip** → bind locals
   (`FirstSeenPalettes`). Dest ≈ identity. Positions match file
   triangles already skinned at parse time.
2. Immediate `Vector3.Transform(..., ObjectTransform)` into world
   metres. Dest palette is discarded. No `c38` upload. No helper
   record. No queue slot. No `VSHADER_PALSKIN_*`.
3. Result is concatenated with landscape + primary C3Ds and Presented
   as unlit/static verts.

`SubmittedHeroPalskin=true` only means “4299 is in
`SubmittedPalskinMeshIds` because `BoneCount>0`”. It does **not** mean
the PALSKIN renderer ran.

Status README line “Engine submit skins PALSKIN C3Ds
(`TrianglesForPose` / `00BD2F91`) not static flatten” is therefore
**DISPROVEN as DX9 character draw**. The dest multiply exists as a
format helper; submit then treats the product as a static mesh.

`WorldGeometry.AddInstances` (expand path) is worse: it walks
`mesh.Triangles` (already bind-flattened at parse) and never calls
`TrianglesForPose`. Helpers/dummies are decompressed and thrown away
(`MeshFile.Parse` `20*helperCount` / `56*dummyCount`).

Do **not** “fix” this by GPU-skinning the same single Graphic 4299
flatten. The native player is appearance + attachments + PALSKIN
records.

---

## Recovered chain

### 1. Create Players → player object  `PROVEN`

`004166A8` / `EngineLifecycle.CreatePlayers`:

| Step | VA | Host |
|---|---|---|
| singleton | `0044C6B0` `[0x13B879C]` | Note |
| 5 × `0x22C` slots, 4 active | `0044A530` / `0044BC10` | `PlayerSlotsCreated=5` `PlayerActiveCount=4` |
| not `hero_swap_*.tng` | `0044A3B0` is owner ctor, not spawn | DISPROVEN as spawn |
| `004AE940` game+80568 | writes `[player+9826]=1` | `PlayerActionReady` |
| predicate | `0099A350` always `al=1` | PROVEN |
| type registrar | `00522A20` `PlayerCreature` / factory `0052B880` | Note only |

Slots are player *objects*, not a HERO Thing. The Thing arrives later.

### 2. No-save first rendered scene  `PROVEN`

Not `00DBDE40` / `StartOakVale` / `CREATURE_HERO_CHILD`.

| Item | Status |
|---|---|
| No-save enqueue `00501450` → `00500540(1,0,0)` native index **1 = LookoutPoint** | PROVEN |
| Persist `PlayerRegionName` `00487C20` is continue, not New Game | PROVEN |
| `006C2170` / `00521AE0` loads LookoutPoint `.tng` | PROVEN |
| Lookout TNG has **no** `PlayerCreature` NewThing | PROVEN |
| First Present = Lookout `RegionThings` + hero camera `006B3FF0` | PROVEN |
| `Q_NewOakValeIntro` / SHOT2 is first *playable* no-save Present | DISPROVEN |
| `RegionTravel` comment “Lookout is not new-game” | leftover vs no-save Present; intro click path is a *different* scene |

### 3. Load Single Thing → insert PlayerCreature  `PROVEN`

`0051FD80`:

- `PlayerCreature` + `[world+258]` → bind `00449970` / `00487DC0`
  (`player+44` → `00A01B50`) then `006AC910`.
- Else allocate `00A371C0` (factory table). `HOLY_SITE` factory
  `0052AC90`.

No-save Lookout has no `PlayerCreature`. Native start marker:

`HOLY_SITE_PLAYER_START` **`GuildArrivalHSP`** → `0049F180` Init
Characters → `00489D40` CreateCharacter → `006AC910`
`CThingPlayerCreature::Create`.

Dump `tools/Fable.ExeIndex/out/01-sections/newgame-trace/cthingplayercreature-create-006ac910.md`:

- alloc `004C7380` size `0x208`
- `0052AB20` then `006A9DD0` ConstructFromParams
- ConstructFromParams dump: **`call 00662880`** (parent), then
  `[esi+112]` `0042B0A2`, then **`004C9D60("CTCPhysicsControlled")`**
- activate `004C9CA0`

Host: `SpawnHeroFromPlayerStart` copies GuildArrivalHSP XYZ into a
new `ThingInstance` (`DefinitionType=CREATURE_HERO`,
`ScriptName="Hero"`) and `InsertThing`s it.

### 4. PLAYER_HERO miss → CREATURE_HERO mesh 4299  `PROVEN`

`00449D90`: `009AD410("PLAYER_HERO")` then `0044BA90`. Miss →
`CREATURE_HERO` / `0048A070` InitCharacterAs.

Live game.bin:

| Def | Type | Graphic | SubDefs |
|---|---|---|---|
| `PLAYER_HERO` | `PLAYER` | **none** (raw 21, 0 subs) | 0 |
| `CREATURE_HERO` | `CREATURE` | **4299** | 32 |
| `CREATURE_HERO_TRAINING` | `CREATURE` | **4299** | 32 |
| `CREATURE_HERO_CHILD` | `CREATURE` | **4300** | 33 |
| `CREATURE_YOUNG_HERO` | `CREATURE` | **4300** | 26 |

`ResolveHeroDefinition` therefore always takes the
`00449E0D CREATURE_HERO` fallback on TLC. Host
`HeroDefinition="CREATURE_HERO"` `HeroMeshId=4299` matches
`Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`.

`00662880` / `004CA010` bind def `[thing+140/+112]`.
`0042AF3C` / `009AD9E0` is appearance attach (Note only; body
PARTIAL). `FindMeshIds("CREATURE_HERO")` returns **only** Graphic
4299. That is the unclothed / base body C3D, not the dressed player.

### 5. Creature / appearance (body + unread layers)  `PARTIAL`

`CREATURE_HERO` sub-defs (game.bin, 32 entries). Relevant to render:

| Type | Role | Status |
|---|---|---|
| Graphic 4299 `MESH_HERO` | base body C3D | PROVEN |
| `CAppearanceDef` idx 10533 owner 1470 | appearance table +52 (20-byte clip names, `00662A00`) | type present; **body UNREAD** |
| `CHeroMorphDef` idx 10535 | Strength/Will/Skill/Morality/Fatness + Teenager persist `0071D102` / `0071D037` | persist names PROVEN; apply UNREAD |
| `CHeroDef` | hero component | UNREAD |
| `CCreatureDef` | creature component | UNREAD |
| `CWeaponDef` idx 10526 owner 1469 | weapon attach | type present; **mesh UNREAD** |
| `CCarryingDef` idx 10527 | hands / carried | UNREAD |
| `CPhysicsDef` | `CTCPhysicsStandard` pose | TNG axes PROVEN; live Thing axes not copied onto `Hero` |
| `CSkeletalMorphDef` | **kid only** (4300), not adult 4299 | n/a for Lookout adult |

`005B37F7` DEFAULT play **does** reach `0070C050` → `0070D580`.
Only `E8` callers are clothing GUI `005B6881` and `PC_UI_FRAME`
`005B8743`. **`006AC910` does not call it.**
`FirstSeenAppearancePlaysDefault=false`. `FirstSeenPlaysAnim=false`.
First Lookout frame is bind pose. Do not invent STAND / DEFAULT.

### 6. Mesh parts (4299)  `PROVEN` as file / `PARTIAL` as submit

Live `MESH_HERO` (graphics.big id **4299**, type 5, anim=1):

| Field | Value |
|---|---|
| Bones | **77** (`Scene Root` → `Movement_dummy` → `Sub_movement_dummy` → `Bip01` …) |
| Skin verts / faces | 3378 / 2117 |
| Primitives | **19** (one per material) |
| Helpers / dummies | **12** / **7** (header; parse decompresses then **drops**) |
| Palskin samples | 16 |
| Prim0 | stride **36**, flags **22** (`0x16`), 1 animated block, **groupBones=9** |

Materials (body parts, not separate GameBin meshes):

`face` (diff 1250 bump 1233, MapFlags=3), `eye shadow` (Flag1=1,
diff 1045), `torso_front` / `torso_arms` / `torso_back`,
`L_/R_upperarm|forearm|hand`, `hips`, `L_/R_thigh|calf|foot`,
`mouth`.

Kid **4300** `MESH_YOUNGHERO_02` is a different file: 76 bones, 4
prims, stride 28 / flags `0x14`, hair material Flag1=1. Do not use
4300 on the no-save Lookout player.

Adult stride 36 / flags `0x16` is **not** the kid 28/`0x14` first-seen
decl and **not** father 20/flags 4. `PalskinPipelineTests` lock
father + kid. **Adult 4299 decl is untested.** PARTIAL.

Submit today ignores primitive groups / bump / Flag1 type index and
emits one `SrcAlphaBlend=true` soup.

### 7. Clothing / hair / weapon attachments  `PARTIAL` script / `UNREAD` mesh

Script apply is recovered; render attach is not.

| Verb | Apply | World field | Mesh |
|---|---|---|---|
| `HeroHair` `00CC9182` vtbl+764 | PROVEN | `World.HeroHairs` accumulate | **not bound** |
| `HeroWear` `00CC9274` vtbl+760 | PROVEN | `World.HeroClothes` | **not bound** |
| `HeroTattoo` `00CC91FB` vtbl+576 | PROVEN | `World.HeroTattoos` | **not bound** |
| `RemoveHeroClothes` `00CC92ED` vtbl+756 | PROVEN | clears clothes only | n/a |
| `SetHeroWeapon` `00CCFDA9` vtbl+488 | PROVEN | `World.HeroWeapon` | **not bound** |
| `PutInHeroHands` vtbl+572/568 | PROVEN | `World.HeroHands` | **not bound** |
| `RemoveHeroWeapons` vtbl+552/560 | PROVEN apply; bag body UNREAD | clears name | n/a |

Live clothing objects:

| Script name | Graphic `FindMeshId` | Real attach def |
|---|---|---|
| `OBJECT_HERO_HAIR_YOUNG_01` | **4126** | `CAppearanceModifierDef` 11656 |
| `OBJECT_HERO_BEARD_TRAMP_01` | **4126** | `CAppearanceModifierDef` 11692 |
| `OBJECT_HERO_NO_HAT` | **4126** | `CAppearanceModifierDef` 12108 |

**4126 is `MESH_HERO_FOLDED_HAT_BANDITCAMP`** (static type-1, anim=0,
936 tris, helpers=1 dummies=1). Submitting `FindMeshId(HeroHair)` as
an extra C3D would draw a bandit-camp hat on the player. That is a
second masquerade. The Graphic field on these OBJECTs is **not** the
worn mesh.

Native attach is `CAppearanceModifierDef` + dummy/helper sockets on
4299 (7 dummies / 12 helpers, unread). `CWeaponDef` / `CCarryingDef`
are the weapon/hands slots. All three def bodies are **UNREAD**.

`PARITY.md` leftover 2 (“Creature clothing / appearance layers …
`CAppearanceDef` / morphs are unread”) still holds.

### 8. Skeleton → animation pose  `PROVEN` apply / `UNREAD` time / `DISPROVEN` wired to submit

Native PlayAnimation:

```
script 00CC14B8 / apply 00CC15DA
  actor.vtbl+72(name, …)
  004C7470 walk [this+68..+72]
    CTC type 90 +68 = 00686920 (al=1 ret 4)   PROVEN accept
  00662A00 appearance+52 20-byte table         miss → DEFAULT
  0070C050 request → 0070B460 [comp+12] → 0070D580   PROVEN apply
```

`WaitPlayAnimation` `00CC18E0` also plays via vtbl+72 (or +76) then
polls leftover vtbl+104 (`PARTIAL`).

Host `AnimationRuntime` records `States[actor].ClipKey` and ticks
`PlayTime`. `PoseNames()` is the map FirstSceneWorld already passes
into `WorldGeometry.Build(..., actorPoses:)`.

**Lookout engine path never calls it:**

- `LoadQuestsAndActivate` does `ScriptRuntime.Detached()` +
  `ActivateQuest` on WLD `START_INITIAL_QUESTS`
  (`Q_SunnyvaleMaster`, `PersonalScriptMain`, …). **Not**
  `BindScene(RegionThings)` and **not** `BindHero(Hero)`.
- `ScriptRuntime.BindScene` only sets the `HERO` alias when
  `DefinitionType == CREATURE_HERO_CHILD`. Adult Lookout Hero is
  `CREATURE_HERO` / `ScriptName="Hero"` — alias stays empty.
- `PumpQuests` Notes `00CB7950` per activated name and does **not**
  call `Runtime.Update`. Fibers, WalkTo, PlayAnimation do not run
  on the no-save pump.

`FirstSeenPlayAnimationAppliesPose=false` stays correct for the
first Lookout frame (create does not play). It is **not** a reason
to keep submit on bind locals after a clip has been requested.

XSEQ first-key sample (`00A999B0` / `00AA4680` / `00A4C5E0`) into
`PaletteForPose` is **PROVEN** by `XSeqFormatTests` (synthetic +
wake loop 3420 vs kid 4300). `PaletteForPose` **ignores `time`**.
Hierarchy walk / interp `00AA0090` is **UNREAD**.

### 9. PALSKIN dest / bone palette  `PROVEN` format / `UNREAD` on adult submit

Dest = hierarchy(48-byte local TRS, parent from 60-byte) × 64-byte
IBM (`00A9E1E0` + `00BD2F91` `dest=S*C3D`). SSE path first-seen
(`[0x13D2880]=1`). Packer `00BD2D90` when `[this+288]==0`. Upload
`00BCFB00` copies 12 dwords from `dest[bone*64]` and
`SetVSConstantF(c38, count*3)`. VS `VSHADER_PALSKIN_DIRLIGHT_FOG`
`a0`-relative `c[38+a0]`. File blend bytes are register offsets
into the `00A8E770` group list.

Kid 4300 / father samples: `PalskinPipelineTests` + `MeshFormatTests`.
Adult 4299 prim0 groupBones=9, stride 36 / flags 22: **not locked**.

`TrianglesForPose(palettes)` is the CPU equivalent of that VS. It is
a test/debug skin, not the draw. Host submit calls it with bind
palettes and then throws the palette away.

### 10. Render records → DX9 draw  `PROVEN` (first-seen family) / `DISPROVEN` (host)

Native (from `WorldShading` + newgame-trace shaders/PALSKIN dumps):

| Step | VA | Notes |
|---|---|---|
| helper ctor | `00BCE740` vtbl `012A6C5C` | +28 type index, +32 fade |
| queue | `00B84720` on `0x1436E74` | type1 → slots 10 then 14; type0 → 8 (+Flag1 → 9) |
| MainScene drain | `00B33010` | layer `0x80` slot 14; `0x100` slots 8+10 |
| drain | `00B849F0` → vtbl+20 `00BD7110` | first-seen helper+32==0 → `00BD3070` |
| bind switch | `00BD3070` | first-seen default `00BD549D` (not type4 `00BD3C04`) |
| pack + upload | `00BD2D90` / `00BCFB00` | c38, 3 float4s / influence |
| draw entry | `00BD71B0` | `[this+8]` enable; inherits CCW; no Flag1 cull |
| family shader | `VSHADER_PALSKIN_DIRLIGHT_FOG` | packed-light count 0 |

Host: none of these run. `SilkEngineHost.Present` uploads
`EngineFrame.Vertices/Draws` from `SubmittedMesh` (the flatten).

---

## Host audit (do not edit)

### `EngineLifecycle.cs`

| Site | What it does | Gap |
|---|---|---|
| `CreatePlayers` | slots + `[+9826]=1` | no Thing |
| `LoadSingleThing` / `SpawnHeroFromPlayerStart` / `SpawnHero` | inserts `Hero` at GuildArrivalHSP, `HeroMeshId` from Graphic | no appearance/components |
| `InsertThing` | `ResolveSubmit` → first Graphic only | drops `CAppearanceDef` / modifiers / weapon |
| `LoadQuestsAndActivate` | `Runtime.Detached()` + WLD initial quests | no `BindScene` / `BindHero` |
| `PumpQuests` | Note walk only | no `Runtime.Update` |
| `SubmitCurrentWorld` | primary C3Ds + extra 4299 + `BuildMeshes` | CPU flatten masquerade |
| `PresentWorld` | `WorldGeometry.Build(..., expandGeometry:false)` **no** `actorPositions` / `actorPoses` | Lookout Hero pose is TNG spawn XYZ only |
| `BuildFrame` | `Camera` + `SubmittedMesh` + Runtime fade/AVI | mesh not from Runtime |
| `Camera` | `ScriptedCamera` (SHOT2 / `UseCamera`) | Lookout uses `WorldCamera.SeedHero` `006B3FF0` lerped by `006B42F0` into that object |

`Hero` fields actually populated: `Kind`, `Section`,
`DefinitionType=CREATURE_HERO`, `ScriptName=Hero`, XYZ from HSP.
**No** `CTCPhysicsStandard.RHSetForward/Up` copied from the start
marker, so `ObjectTransform` uses default forward=+Y up=+Z.

### `MeshFile.TrianglesForPose` / `PaletteForPose`

Correct dest math for a supplied palette. Null clip = bind locals.
`time` discarded. Adult 4299 stride/group untested. Helpers/dummies
not retained — attachment sockets cannot be recovered from the
parsed `MeshFile`.

### `MeshBatches.BuildMeshes`

The flatten. Keep for **static** C3Ds. Do not feed PALSKIN dest
through it and call that character rendering.

### `ScriptedCamera` / `World.Positions` / Hero Thing

- `ScriptedCamera`: intro / `UseCamera` / `00CC9F3A`. First-seen
  Oakvale SHOT2. Not the Lookout follow camera.
- `WorldCamera`: `006B4900` / seed `006B3FF0` / pose `006B2CA0`.
  Seeded from Hero XYZ. Follow spring / tick leftover UNREAD.
- `WorldRuntime.Positions`: WalkTo dest (`006A9960`). Consumed by
  `WorldGeometry.ApplyActorPositions` **only** when a caller passes
  `actorPositions`. FirstSceneWorld does. `SubmitCurrentWorld` does
  not. Engine `Hero.Position*` never reads `Positions["Hero"]`.
- Script name `"Hero"` == `RegionTravel.IntroHeroActor` ==
  `EngineLifecycle.HeroScriptName`. The string already matches.
  The objects are not linked.

### `NewGameScript` / `PlayerInterface`

- `NewGameScript`: observation façade over `S_QNOVI`. Not the
  no-save Lookout player. Do not start it from
  `Q_SunnyvaleMaster` factory ctor.
- `PlayerInterface`: `004473A0` / pump `00446A30`. Input listeners
  (`0123758C`). No mesh, no appearance, no PALSKIN.

### `FirstSceneWorld` (contrast)

Oakvale intro **does** thread `runtime.ActorPositions` +
`runtime.Animation.PoseNames()` into `WorldGeometry.Build`. That is
the kid/SHOT2 scene, mesh 4300, not no-save Lookout 4299. Reusing it
as the player path is **DISPROVEN** for first Present.

---

## Exact integration points

Connect **existing** script/runtime Hero state to the **actual**
rendered player. Do not invent a second hero, do not flatten 4299,
do not submit clothing Graphics 4126.

1. **Thing identity**
   - Source: `EngineLifecycle.Hero` (`CREATURE_HERO`, `ScriptName="Hero"`,
     GuildArrivalHSP XYZ).
   - Bind: `Runtime.Bindings.BindHero(life.Hero)` **without** the
     `CREATURE_HERO_CHILD` filter in `ScriptRuntime.BindScene`.
   - Seed: `World.Positions["Hero"] = RegionTravel.PositionOf(Hero)`
     (same as `MovementRuntime.SeedStart`).
   - After WalkTo/Teleport: write back `Hero.Position*` from
     `World.Positions["Hero"]` (or submit via
     `ApplyActorPositions` on `ThingsForMap(LookoutPoint)`).

2. **Quest pump**
   - `PumpQuests` must `Runtime.Update(dt)` (`00A44880` /
     `009E1BC0`) so `PlayAnimation` / `WalkTo` / `HeroWear` actually
     mutate `Animation.States` and `World.*`.
   - Keep WLD initial list. Do not inject `S_QNOVI`.

3. **Appearance seed (create)**
   - After `006AC910` / `004CA010`: attach `CAppearanceDef` (idx
     10533) at `[thing+112]` (`0042AF3C`). That table is
     `00662A00` appearance+52.
   - Do **not** call `005B37F7` DEFAULT on create.
   - Morph: `CHeroMorphDef` persist (Teenager only on
     `CREATURE_HERO_TRAINING`). Adult Lookout: unread scales; bind
     pose until recovered.

4. **Mesh parts**
   - Draw **4299’s 19 PALSKIN primitives**, not `mesh.Triangles` as
     one batch.
   - Keep group-bone lists (`00A8E770` +24). Adult prim0 has 9.
   - Material Flag1 / MapFlags → `PalskinTypeIndex` (eye-shadow
     Flag1=1). Bump 1233–1237 are file fields; host currently
     drops them.

5. **Attachments**
   - Hair/clothes/tattoo names: `World.HeroHairs` / `HeroClothes` /
     `HeroTattoos` → resolve **`CAppearanceModifierDef`**, not
     `FindMeshId` (4126 hat trap).
   - Weapon/hands: `World.HeroWeapon` / `HeroHands` → `CWeaponDef` /
     `CCarryingDef`.
   - Socket: 4299 dummy/helper stream (7 / 12). Parser must
     **keep** the 56-byte dummy / 20-byte helper records.
   - Until those defs are read, **omit** attachments rather than
     drawing Graphic 4126.

6. **Pose → palette**
   - Clip name: `Runtime.Animation.PoseNames()["Hero"]` /
     `States["Hero"].ClipKey`.
   - Resource: type-6 XSEQ via `MeshBank.GetAnim` (wake 3420 is the
     locked sample; Lookout clip table is appearance+52 UNREAD).
   - `PaletteForPose(heroMesh.Bones, clip, time, sequence)`.
   - Time: `States["Hero"].PlayTime` once `00AA0090` is read;
     until then first-key only (already implemented).
   - Null clip → bind locals (correct for first Lookout frame).

7. **Palette → records → draw**
   - Dest palettes → pack 3×4 rows → `c38` (`00BCFB00`).
   - One PALSKIN helper/record per primitive (not one world
     triangle list).
   - Drain layers `0x80` / `0x100` as native, **after** landscape
     `0x4/0x40/0x20/0x2000`.
   - Shader `VSHADER_PALSKIN_DIRLIGHT_FOG`. Do not implement this
     as a Vulkan special case on the flatten.

8. **Transform**
   - Instance W = `ObjectTransform(Hero)` (cm→m, RHSetForward/Up).
   - Copy start-marker axes onto `Hero.Properties` at spawn
     (currently default +Y/+Z).
   - Camera stays `WorldCamera` `006B3FF0` → `006B42F0` into
     `ScriptedCamera` for Present. Do not switch Lookout to SHOT2.

9. **What submit must stop doing**
   - `seen.Add(4299)` + `BuildMeshes` as the player.
   - Counting `BoneCount>0` as `SubmittedHeroPalskin`.
   - `WorldGeometry` expand of `mesh.Triangles` for creatures.
   - FirstSceneWorld / 4300 / `CREATURE_HERO_CHILD` on this path.

---

## Classification table

| Claim | Status | Evidence |
|---|---|---|
| Create Players 5×`0x22C`, `[+9826]=1` via `0099A350` | PROVEN | `CreatePlayers`, status README, `ea479d2` |
| No-save first Present = Lookout + `006B3FF0` | PROVEN | `EngineLifecycleTests`, `fe6a11e` |
| Lookout TNG has no PlayerCreature; HSP → `006AC910` | PROVEN | `Load_single_thing_0051FD80_*`, create dump |
| `PLAYER_HERO` has no Graphic; fallback `CREATURE_HERO` 4299 | PROVEN | live game.bin + `ResolveHeroDefinition` |
| 4299 is PALSKIN (`BoneCount=77`, skin stream, anim=1) | PROVEN | live `MESH_HERO` dump |
| 4299 is 19 body-part prims + 12 helpers + 7 dummies | PROVEN file / UNREAD sockets | Dump + `MeshFile` drop |
| Adult stride 36 / flags `0x16` / groupBones 9 | PROVEN file / PARTIAL vs tests | Dump; tests lock 28/`0x14` and 20/4 |
| Create does not play DEFAULT / STAND | PROVEN | `FirstSeenPlaysAnim=false`, `005B37F7` callers |
| PlayAnimation apply `004C7470` / `0070D580` | PROVEN | `XSeqFormatTests`, `f778853` |
| XSEQ first-key → `PaletteForPose` | PROVEN | `00A999B0` / `00AA4680` / `00A4C5E0` |
| Time interp `00AA0090` | UNREAD | `PaletteForPose` drops `time` |
| Engine Runtime bound to Lookout Hero | DISPROVEN | no `BindScene`/`BindHero`; kid-only filter |
| `PumpQuests` runs `Runtime.Update` | DISPROVEN | Note-only walk |
| `World.Positions["Hero"]` drives submit | DISPROVEN | `PresentWorld` omits `actorPositions` |
| `HeroHair`/`HeroWear`/`SetHeroWeapon` script apply | PROVEN | `GlobalDispatcher` + tests |
| Those names become worn meshes | UNREAD | `CAppearanceModifierDef` / `CWeaponDef` |
| Clothing `FindMeshId` is the worn C3D | DISPROVEN | all three resolve **4126** folded hat |
| `CAppearanceDef` / `CHeroMorphDef` bodies | UNREAD | PARITY leftover 2 |
| Dest = S×IBM (`00BD2F91`) bind identity | PROVEN | `MeshFormatTests` kid palettes |
| Host submit is PALSKIN records / `c38` / `00BD71B0` | DISPROVEN | `BuildMeshes` flatten |
| `SubmittedHeroPalskin` means character rendering | DISPROVEN | flag is `BoneCount>0` membership |
| `ScriptedCamera` is the Lookout player camera | DISPROVEN as owner | seed is `WorldCamera` `006B3FF0` |
| `PlayerInterface` / `NewGameScript` draw the player | DISPROVEN | input / S_QNOVI façade |
| FirstSceneWorld 4300 path is no-save Present | DISPROVEN | different region + creature |

---

## Do not

- Implement PALSKIN as a Vulkan hack on the static soup.
- Edit `EngineLifecycle.cs` from this investigation.
- Submit Graphic **4126** as hair/hat/beard.
- Draw `CREATURE_HERO_CHILD` / 4300 on Lookout no-save.
- Call `005B37F7` from `006AC910`.
- Treat `SubmittedHeroPalskin` as done.

Next recoverable slices, in order: (1) bind Runtime ↔ `Hero` +
`Positions["Hero"]` + `PumpQuests`→`Update`; (2) keep dummy/helper
bytes and `CAppearanceModifierDef`; (3) per-prim PALSKIN records +
`c38` instead of `BuildMeshes`.
