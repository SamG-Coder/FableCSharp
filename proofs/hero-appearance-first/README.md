# First Hero Graphic / PALSKIN mesh after Leave Frontend

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `CREATURE_HERO_CHILD` / mesh **4300** /
`00DBDE40` / Graphic **4126**. Those are later leftover intro /
clothing traps, not Leave / Init Game / first no-save Present.

Do **not** treat `PlayerInterface` (`004473A0` / `game+32`) or
`004AE9D0` as Hero appearance. Sibling: `proofs/player-bind-world`.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**DIVERGE** / **LEFTOVER**.

Sources: listings `0042AF3C` / `0042B0A2` / `004CA010` / `006A5950`
/ `00662880` / `006A9DD0` / `006AC910`; newgame-trace dumps of those
VAs; `docs/status/investigations/E-player-palskin.md`,
`2026-08-18-palskin.md`, `2026-08-18-first-scene-things.md` (+ dump);
`docs/runtime/FORWARD_TREE.md` §§4, 7–10; `GameBinFormatTests`
(`FindMeshId("CREATURE_HERO")==4299`);
`EngineLifecycleTests.Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`,
`Install_banks_and_startup_videos_exist`.

Siblings: `proofs/palskin-open` (first type-5 *payload*),
`proofs/hero-stats-first` (PLAYER_HERO *name* bind, no Thing),
`proofs/morph-first` (expression names, not Graphic),
`proofs/c3d-first-submit` (static `0x20`, not PALSKIN),
`proofs/weapon-anim-first` (no weapon mesh on create),
`proofs/tng-spawn` (Lookout TNG has no PlayerCreature).

---

## Verdict

**First Hero graphic after Leave is Lookout adult Graphic 4299
`MESH_HERO` (PALSKIN type 5, 19 prims, 77 bones).** Not frontend.
Not Init Game. Not kid 4300. Not folded-hat 4126.

| Layer | First after Leave | Class |
|---|---|---|
| Frontend 2D / Leave Present | no Hero mesh | **DISPROVEN** as appearance |
| `SHADERS_PALSKIN` name | Init Engine, still retail | **PROVEN** tokens. **DISPROVEN** as Graphic |
| `MBANK_ALLMESHES` directory | Init World `0049E620` | **PROVEN** open. **DISPROVEN** as parse |
| `PLAYER_HERO` / `CREATURE_HERO` *name* | `0049F180` → `00449D90` | **PROVEN** bind. **DISPROVEN** as mesh |
| Thing + Graphic field | later `006AC910` at `GuildArrivalHSP` | **PROVEN** |
| Def object at `[thing+112]` | `004CA010` → `0042AF3C` → `009AD9E0` | **PROVEN** attach. Body **UNREAD** |
| `CAppearanceDef` named walk | `006A9DD0` → `0042B0A2` | **PROVEN** call. Clip table **UNREAD** |
| Type-5 C3D payload | first `00A243B0` miss on **4299** | **PROVEN** id. Caller **PARTIAL** |
| First-Present PALSKIN set | **`[4299]` only** | **PROVEN** |
| Native PALSKIN DIP | bits `0x80`/`0x100` `00BD71B0` | **PROVEN** family. First hero DIP **PARTIAL** |
| Host `SubmittedHeroPalskin` | `BoneCount>0` flatten | **DISPROVEN** as character draw |

`00DBDE40` / kid **4300** / father / `PalskinPipelineTests` fixtures
are **not** this appearance. **PROVEN**.

---

## Timeline (no-save New Game)

```
0042EC7C retail
  0042DF9E 2D UI type 0x22            // no Graphic 4299
  Init Engine 0042E204
    00B3B6D0 3, "SHADERS_PALSKIN"     // NAME only
0042F2A2 Leave frontend
  009BE420 + 009BEEB0 Present         // black; no mesh
0042F491 Init Game → 004184BD
  Init Thing Components 004EE23F      // CTCHeroMorph names; no C3D
  Init Definition Manager 00416005(1) // game.bin (Graphic 4299 lives here)
  Create Players 004166A8             // 5×0x22C slots; NOT a Graphic
  Init World 004A6E30
    0049E620 Opening Mesh Bank        // directory; ParsedCount=0
  00416953 FinalAlbion.wld
    0049F180 → 00449D90
      009AD410 "PLAYER_HERO"          // no Graphic → miss
      00449E0D "CREATURE_HERO"
      0048A070 / 00489D40
        holy-site miss → no 006AC910  // hero-stats-first
004189C2 pumps                        // dummy index 0; HeroSpawned=false
later 00501450(1) LookoutPoint
  006C2170 Loading objects
    ContainsMap TNG: no PlayerCreature, no CREATURE_HERO
    Graphic apply 0077BA40            // static props, not Hero
  GuildArrivalHSP (52.688, 69.597, 36.982)
    006AC910 CThingPlayerCreature::Create  size 0x208
      0052AB20
      006A9DD0 ConstructFromParams
        00662880 → 008388D0 → 006A5950
          004CA010
            [thing+140] = def id
            0042AF3C([thing+112]) → 009AD9E0   // compiled def
        0042B0A2([esi+112]) "CAppearanceDef"
        004C9D60("CTCPhysicsControlled")
      004C9CA0 activate
    Graphic 4299 MESH_HERO            // FIRST Hero mesh
then first 00A243B0(id=4299) miss → 00A26D40 type 5
first 3D Present
  0x4 / 0x40 landscape
  0x20 static C3D (props; not Hero)
  0x80 / 0x100 PALSKIN dest 4299 bind locals
```

---

## 1. Frontend / Leave — no Hero Graphic

| Claim | Class | Evidence |
|---|---|---|
| Frontend Present is 2D (`0042DF9E` / type `0x22`) | **PROVEN** | FORWARD_TREE §4 |
| `MBANK_ALLMESHES` during `0042EC7C` | **DISPROVEN** | first `0049E620` is Init World after Leave (`palskin-open`) |
| Press Start `009AD410` opens 4299 / 4300 | **DISPROVEN** | UI Type=10 (`0041D21B`) |
| Leave teardown draws PALSKIN | **DISPROVEN** | `0042F2A2` fade / clear+Present |
| `PumpFrontendFrame` calls `Meshes.Get(4299\|4300)` | **PROVEN** absence | `EngineLifecycle` frontend body |

**Answer:** no Hero appearance on frontend or Leave.

---

## 2. Init Game binds the *name*, not the mesh

`0049F180` (Load World, no-save) always reaches `00449D90` because
there is no player Thing yet (`hero-stats-first`).

```
009AD410("PLAYER_HERO")
0044BA90 → 009AD9E0          // eax<=0 fail
00449E0D push "CREATURE_HERO"
0048A070 InitCharacterAs
  00489D40 CreateCharacter
    00488B20 holy site miss → ret 0   // no 006AC910
```

Live `game.bin`:

| Def | Type | Graphic | Sub-defs |
|---|---|---|---|
| `PLAYER_HERO` | `PLAYER` | **none** (raw 21) | 0 |
| `CREATURE_HERO` | `CREATURE` | **4299** | 32 |
| `CREATURE_HERO_TRAINING` | `CREATURE` | **4299** | 32 |
| `CREATURE_HERO_CHILD` | `CREATURE` | **4300** | 33 |
| `CREATURE_YOUNG_HERO` | `CREATURE` | **4300** | 26 |

`FindMeshId("CREATURE_HERO")==4299` **PROVEN** (`GameBinFormatTests`).
`FindMeshIds` on that def is **only** the Graphic field (no
`CMultiStaticMeshDef` / `CReplaceableMeshDef` extra). **PROVEN**
parser. That is still **not** a live C3D until `006AC910`.

Create Players / `004AE9D0` / `PlayerInterface` never store 4299.
**DISPROVEN** as appearance.

---

## 3. First Thing Graphic — `006AC910` at GuildArrivalHSP

Lookout TNG has **no** `PlayerCreature` / `CREATURE_HERO`. Marker is
`HOLY_SITE_PLAYER_START` **`GuildArrivalHSP`**. **PROVEN**
(`tng-spawn`, first-scene dump).

```
006AC910  alloc 0x208 (004C7380)
          0052AB20
          006A9DD0
00662880  call 008388D0
008388D0  arg0>0 → 006A5950
006A5950  vtbl+64 lookup then 004CA010
004CA010  [esi+140]=def; if id>0: 0042AF3C(manager+32, id, &esi+112)
0042AF3C  arg<=0 fail; else 009AD9E0 → refcount store at dest
006A9DD0  then 0042B0A2([esi+112]) push "CAppearanceDef"
          then 004C9D60("CTCPhysicsControlled")
004C9CA0  activate
```

`0042AF3C` is **generic def attach**, not a mesh load. `009AD9E0` is
id→object. `009AD410` is the *name* HANDLE used earlier for
`PLAYER_HERO`; host `DefLookupFn=009AD410` Notes on insert are
**LEFTOVER** vs this id path.

`0042B0A2` is the **appearance** walk: vtbl+56 `"CAppearanceDef"`
then `009ADA10`. That is `CAppearanceDef` idx **10533** (type
**PROVEN**; +52 clip table `00662A00` **UNREAD**). It does **not**
change Graphic 4299.

`005B37F7` DEFAULT is **not** on `006AC910`. Callers are clothing GUI
`005B6881` and `PC_UI_FRAME` `005B8743` only.
`FirstSeenAppearancePlaysDefault=false`. First Lookout frame is
**bind pose**. **PROVEN**.

Host: `SpawnHero` copies HSP XYZ + RHSet axes onto
`DefinitionType=CREATURE_HERO` `ScriptName="Hero"`, `InsertThing` →
`HeroMeshId=4299`. `HeroSpawned` first becomes true on pump3
Lookout, not Init Game. **MATCH** timing / id.

Which later `00489D40` first returns 1 and hits `006AC910` is
**UNREAD** (`hero-stats-first` open). Host site is
`LoadFromFirstRealRegion`. Identity of the Thing is **PROVEN**.

---

## 4. Mesh 4299 is the appearance Graphic

Live `graphics.big` id **4299**, type 5, anim=1, name `MESH_HERO`:

| Field | Value | Class |
|---|---|---|
| Bones | **77** | **PROVEN** file |
| Skin verts / faces | 3378 / 2117 | **PROVEN** |
| Primitives | **19** (one per material) | **PROVEN** |
| Helpers / dummies | 12 / 7 (parse **drops**) | **PROVEN** sizes. Sockets **UNREAD** |
| Prim0 | stride **36**, flags **22**, group **9** (`torso_back`) | **PROVEN** file |
| Prim 16 / 18 | stride **28**, flags **20** (`mouth` / `eye shadow`) | **PROVEN** file |
| Materials | `face`, `eye shadow`, `torso_*`, limbs, `hips`, `mouth` | **PROVEN** names. **DISPROVEN** as face *clips* (`morph-first`) |

Kid **4300** `MESH_YOUNGHERO_02` is 76 bones / 4 prims / stride 28.
**DISPROVEN** as this Present.

First-scene dump after 3 pumps:

```
palskin=[4299] heroPalskin=True
HERO def=CREATURE_HERO script=Hero mesh=4299 pos=52.688,69.597,36.982
uniqueMeshes=45 palskinMeshes=1
Hero exist=1 render=1
```

Submit palskin set is **only 4299**. Lookout AICreature C3Ds exist
as Things and are **not** submitted (`ResolveSubmit` bails on null
XYZ). **PROVEN** exist / **DISPROVEN** as first PALSKIN.

Native payload: `00A243B0(4299)` miss → `00A26D40` type 5 →
`00A89450` / `00A8FD40`. Same as `palskin-open`. First *Hero*
payload is that id. Native first-miss caller **PARTIAL**.

---

## 5. Clothing / hair are not the first Graphic

| Script / OBJECT | `FindMeshId` | Real attach | First-seen? |
|---|---|---|---|
| `OBJECT_HERO_HAIR_YOUNG_01` | **4126** `MESH_HERO_FOLDED_HAT_BANDITCAMP` (static, 0 bones) | `CAppearanceModifierDef` 11656 → PALSKIN **4275/4276/4277** | **DISPROVEN** as first Graphic |
| `OBJECT_HERO_BEARD_TRAMP_01` | 4126 | modifier 11692 | **DISPROVEN** |
| `OBJECT_HERO_NO_HAT` | 4126 | modifier 12108 (empty) | **DISPROVEN** |
| `HeroWear` / `HeroHair` / `SetHeroWeapon` | script apply **PROVEN** later | mesh **UNREAD** | **DISPROVEN** on create / first Present |

Create does **not** `004C9D60("CTCWeapon")`. Weapon bones 73–76 sit
idle on 4299. **PROVEN** (`weapon-anim-first`).

Until modifier apply + dummy/helper sockets are recovered, **omit**
attachments. Do **not** submit 4126.

`CHeroMorphDef` persist (`Strength`/`Will`/`Skill`/`Morality`/
`Fatness`/`Teenager`) is names-only here. Teenager is training.
Adult Lookout apply **UNREAD**. **DISPROVEN** as a second mesh.

---

## 6. First *drawn* Hero is PALSKIN, not static `0x20`

Same Present as first Lookout 3D frame, **after** landscape and
static props:

| Bit | Family | Hero? |
|---|---|---|
| `0x4` / `0x40` | landscape | **no** |
| `0x20` | static C3D `00BB2540` | **DISPROVEN** (props only; `c3d-first-submit`) |
| `0x80` / `0x100` | PALSKIN helper `00BCE740` → `00BD7110` / `00BD71B0` | **yes** 4299 |
| `0x2000` | sky | **no** |

Native dest: hierarchy × IBM (`00A9E1E0` + `00BD2F91`); first-seen
identity (`FirstSeenPlaysAnim=false`). Pack `00BD2D90` / upload
`00BCFB00` `c38`. VS `VSHADER_PALSKIN_DIRLIGHT_FOG`. **PROVEN**
family. Which primitive is the first DIP **PARTIAL**.

Host `SubmitCurrentWorld` concatenates primary C3Ds (incl. 4299)
through `MeshBatches.BuildMeshes` → `TrianglesForPose()` with **no**
clip, then world `Vector3.Transform`. `SubmittedHeroPalskin=true`
only means 4299 is in `SubmittedPalskinMeshIds` because
`BoneCount>0`. That is **DISPROVEN** as `00BD71B0`. Keep as
**membership**. Fallback extra-submit of 4299 does **not** fire
(PresentWorld already has the instance). **PROVEN**.

---

## Host vs native

| Host | Native first-seen | Class |
|---|---|---|
| `PumpFrontendFrame` no `Meshes.Get` | no type 5 | **MATCH** |
| `EnterGame` `OpenMeshBank` directory | `0049E620` | **MATCH** |
| `ResolveHeroDefinition` PLAYER_HERO miss → CREATURE_HERO | `00449D90` / `00449E0D` | **MATCH** id. Site on `SpawnHero` is **LEFTOVER** vs first `0049F180` |
| `SpawnHero` / `HeroMeshId=4299` | `006AC910` + Graphic field | **MATCH** |
| `InsertThing` `004CA010` / `0042AF3C [thing+112]` | listing **PROVEN** | **MATCH** Notes. Does **not** attach `CAppearanceDef` body / `0042B0A2` |
| `FindMeshIds` first Graphic only | unclothed 4299 | **MATCH** body. Drops modifiers / weapon |
| `SubmittedPalskinMeshIds == [4299]` | one PALSKIN Graphic | **MATCH** set |
| `SubmittedHeroPalskin` | not `00BD71B0` | **DISPROVEN** as draw |
| `MeshBatches.BuildMeshes` soup | 19 prims + `c38` | **DIVERGE** |
| `FirstSceneWorld` kid 4300 / SHOT2 | Lookout adult | **LEFTOVER** |
| `WorldShading.FirstSeenPalskinStrideBytes=28` | adult prim0 **36** | **LEFTOVER** name (`palskin-open`) |

---

## Classification table

| Claim | Status |
|---|---|
| Frontend / Leave draws Hero | **DISPROVEN** |
| First Hero Graphic after Leave is **4299** `MESH_HERO` | **PROVEN** |
| That Graphic is kid 4300 / father / 4126 | **DISPROVEN** |
| `PLAYER_HERO` supplies the mesh | **DISPROVEN** (no Graphic) |
| First `0049F180` / `0048A070` creates the Thing | **DISPROVEN** (holy-site miss) |
| `006AC910` at `GuildArrivalHSP` is first Thing | **PROVEN** identity. Native retry caller **UNREAD** |
| `004CA010` → `0042AF3C` → `009AD9E0` fills `[thing+112]` | **PROVEN** |
| `0042B0A2` looks up `CAppearanceDef` | **PROVEN** call. Table **UNREAD** |
| Create plays DEFAULT / STAND / a clip | **DISPROVEN** |
| First PALSKIN payload / submit set is `[4299]` | **PROVEN** |
| First static `0x20` DIP is Hero | **DISPROVEN** |
| Native PALSKIN DIP family `00BD71B0` | **PROVEN**. First primitive **PARTIAL** |
| Host flatten is that DIP | **DISPROVEN** |
| Clothing Graphic 4126 is hair / hat / beard | **DISPROVEN** |

---

## Do not

- Draw Hero on frontend frames.
- Use `CREATURE_HERO_CHILD` / 4300 / father as the first Lookout appearance.
- Submit Graphic **4126** as worn hair / hat / beard.
- Treat `004AE9D0` / `PlayerInterface` / Create Players as a mesh.
- Call `005B37F7` from `006AC910`.
- Treat `SubmittedHeroPalskin` as `00BD71B0`.
- Collapse PALSKIN 4299 into static `0x20` / `MeshBatches` soup and call that appearance.
- Invent a second Hero Thing beside `EngineLifecycle.Hero`.

Next recoverable slice is still per-prim PALSKIN records + `c38` for
**4299**, plus `CAppearanceDef` +52 and modifier sockets — not a
second Graphic id.
