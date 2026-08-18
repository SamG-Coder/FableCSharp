# First Hero mesh 4299 create after Leave

Investigation only. Production `src/` was not edited.

Do **not** start at Oakvale / `00DBDE40` / `CREATURE_HERO_CHILD` /
Graphic **4300** `MESH_YOUNGHERO_02` / `hero_young_set.bncfg`.
That is later `Q_NewOakValeIntro`, not Leave / Init Game /
first no-save 3D Present.

Do **not** collapse Load World `0049F180` into the Thing create.
`0049F180` → `00449D90` is the first **name** bind
(`PLAYER_HERO` miss → `CREATURE_HERO`). First-seen `00489D40`
**misses**. First **mesh 4299** is later `006AC910`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Question: after Leave Frontend, what is the first Hero mesh
create, and what is that Thing’s first-seen appearance /
PALSKIN / bone? Path to hold:

```
0049F180 → 00449D90 PLAYER_HERO miss → CREATURE_HERO
  → 00489D40 → 006AC910
```

Authority: dump listings `0049F180` / `00449D90` / `006AC910`
(`text-map/listing-00480000.txt`, `listing-00440000.txt`,
`listing-00680000.txt`);
siblings `proofs/hero-appearance-first`, `palskin-after-leave`,
`bone-after-leave`;
also `creature-after-leave`, `hero-stats-first`, `tng-spawn`;
`EngineLifecycleTests.Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`,
`Install_banks_and_startup_videos_exist`;
`GameBinFormatTests` (`FindMeshId("CREATURE_HERO")==4299`).

---

## Verdict

**First Hero Graphic after Leave is Lookout adult 4299
`MESH_HERO` on `CThingPlayerCreature::Create` `006AC910`.**
Def name is `CREATURE_HERO` because `PLAYER_HERO` has no
Graphic. Scene is `GuildArrivalHSP`. Not kid 4300.

| Layer | First after Leave | Class |
|---|---|---|
| Frontend / Leave Present | no Hero mesh | **DISPROVEN** |
| `0049F180` Init Characters | `00449D90` name bind only | **PROVEN**. **DISPROVEN** as create |
| `PLAYER_HERO` | `009AD410` then `0044BA90` fail | **PROVEN** miss (no Graphic) |
| Fallback def | `00449E0D` `"CREATURE_HERO"` | **PROVEN** |
| First `00489D40` (Load World) | holy-site miss → `ret 0` | **PROVEN** no `006AC910` |
| First Hero **Thing** | later `006AC910` size `0x208` | **PROVEN** |
| First Hero **Graphic** | **4299** `MESH_HERO` | **PROVEN** |
| Appearance walk | `006A9DD0` → `0042B0A2` `"CAppearanceDef"` | **PROVEN** call. Clip table **UNREAD** |
| Type-5 PALSKIN payload | first `00A243B0` miss on **4299** | **PROVEN** id. Caller **PARTIAL** |
| First dest / bones | 77 C3D bones, bind-identity | **PROVEN** |
| Kid 4300 / `00DBDE40` | Oakvale leftover | **DISPROVEN** |

`00489D40` is the **only** first-seen `E8` of `006AC910`
(`00489FC1`). Other site `0089F660` is leftover. The
successful first-seen take of that `E8` is **after** Lookout
ContainsMap, not inside the Load World `0049F180`.

---

## Timeline (no-save New Game)

```
0042F2A2  Leave frontend
  009BE420 + 009BEEB0 Present            // black; no 4299
0042F491  Init Game → 004184BD
  Init Definition Manager 00416005(1)    // Graphic 4299 lives in game.bin
  Init World 004A6E30
    0049E620 MBANK_ALLMESHES directory   // ParsedCount=0
  00416953  Loading world FinalAlbion.wld
    004A1840
    [0x13B8648]==0
    00416BCA  0049F180(ecx=world, 0)     // PROVEN first-seen site
      "Init Characters"
        00449970 / 00487DC0  miss
        0049F1D7  00449D90
          009AD410 "PLAYER_HERO"
          0044BA90 miss                  // no Graphic
          00449E0D "CREATURE_HERO"
          004498C0
          00449E2D  0048A070
            0048A0AF  00489D40
              00488B20 holy site miss
              [0x13B8647]==0 → ret 0     // NO 006AC910
      "Init GUI" 0043A380
      "Init Quests" 004B4260             // not a mesh
004189C2  dummy pumps                    // HeroSpawned=false
later 00501450 LookoutPoint
  006C2170 Loading objects
    ContainsMap TNG: no PlayerCreature, no CREATURE_HERO
  HOLY_SITE_PLAYER_START GuildArrivalHSP
    (52.688, 69.597, 36.982)
    later 00489D40  (caller UNREAD)
      00489FC1  006AC910                 // FIRST Hero Thing
        004C7380  size 0x208
        0052AB20
        006A9DD0 ConstructFromParams
          00662880 → 008388D0 → 006A5950
            004CA010
              [thing+140] = def id
              0042AF3C([thing+112]) → 009AD9E0
          0042B0A2([esi+112]) "CAppearanceDef"
          004C9D60("CTCPhysicsControlled")
        004C9CA0 activate
      Graphic 4299 MESH_HERO             // FIRST Hero mesh
then first 00A243B0(id=4299) miss
  00A26D40 type 5
    00A89450 / 00A894ED                  // 77 bones, 19 prims
  00BD2D90 dest pack → 00A9E1E0 × IBM ≈ I
```

`CREATURE_HERO_CHILD` / 4300 / `00DBDE40` are **not** on this
list. **PROVEN**.

---

## 1. Listings — bind is not create

### `0049F180` (`listing-00480000.txt`)

```
0049F180  sub esp, 48
0049F18D  push "Init Characters"
0049F1B3  mov ecx, [esi+12]
0049F1B6  call 00449970
0049F1BD  call 00487DC0
0049F1C4  je   0049F1CF          // miss → bind
0049F1C6  test [eax+145], 1
0049F1CD  je   0049F1DC
0049F1D7  call 00449D90          // ONLY E8 of 00449D90
0049F1EA  push "Init GUI"
0049F214  call 0043A380
0049F21B  push "Init Quests"
0049F24E  call 004B4260
```

No `E8 006AC910`. No `E8 0051FD80`. No mesh id.

Callers of `0049F180`: `00416BCA` (`push 0`) **PROVEN**
first-seen; `004A2C80` (`push 1`) insn **PROVEN**, first-seen
take **PARTIAL**.

### `00449D90` (`listing-00440000.txt`)

```
00449D90  sub esp, 8
00449D99  push "PLAYER_HERO"
00449DB7  call 009AD410
00449DD0  call 0044BA90          // appearance attach
00449DD5  test al, al
00449DD7  je   00449E0B          // TLC: no Graphic → miss
00449E0D  push "CREATURE_HERO"
00449E26  call 004498C0
00449E2D  call 0048A070          // both hit and miss
```

`00449E0D` is the miss immediate, not a function.
`0048A070` empty `[esi+52]` → `0048A0AF call 00489D40`.

### `00489D40` then `006AC910` (`listing-00480000.txt` / `listing-00680000.txt`)

```
00489D40  …
00489D65  call 00488B20          // find holy site
00489D6E  test al
00489D70  mov al, [0x13B8647]
00489D75  jne  00489D86
00489D77  cmp al, bl
00489D79  jne  00489D8E
00489D7B  xor al, al
00489D83  ret 4                  // first-seen Load World
…
00489FC1  call 006AC910          // create body only
```

```
006AC910  sub esp, 64
006AC91C  mov ecx, 0x208
006AC923  call 004C7380
006AC933  call 0052AB20
006AC950  call 006A9DD0
006AC9D4  call 004C9CA0
006ACA13  ret 8
```

Load World first-seen: `[0x13B866C]` empty, `[0x13B8647]==0`
→ `ret 0`. **PROVEN** (`hero-stats-first`, `creature-after-leave`).

So the user’s chain is the **identity** path (which def, which
factory). It is **not** one stack at `00416BCA`.

---

## 2. First 4299 Thing — `006AC910` at GuildArrivalHSP

Lookout TNG has **no** `PlayerCreature` / `CREATURE_HERO`.
Marker is `HOLY_SITE_PLAYER_START` **`GuildArrivalHSP`**.
**PROVEN** (`tng-spawn`, first-scene dump).

| Field | Value | Class |
|---|---|---|
| Def | `CREATURE_HERO` (after `PLAYER_HERO` miss) | **PROVEN** |
| ScriptName | `Hero` | **PROVEN** host / dump |
| Factory | `0052B880` size `0x208` → `0052AB20` | **PROVEN** |
| Pose | (52.688, 69.597, 36.982) | **PROVEN** |
| Axes | HSP +X / +Z | native copy **PROVEN** intent. Host RHSet **PARTIAL** historically |
| Graphic | **4299** `MESH_HERO` | **PROVEN** |
| Kid 4300 | not in `RegionThings` | **DISPROVEN** |

`0051FD80` on Lookout does **not** create this Hero. First
creature Thing is villager `FH_Villager` (`npc-first-create`).
Hero is appended after the three ContainsMap walks.

Which later `E8 00489D40` first reaches `00489FC1` is
**UNREAD** (`004A2C80` retry vs `0066FF20` vs region holy-site
list). Host folds create into `LoadFromFirstRealRegion`.
**MATCH** order vs native “after maps”. Noting `0049F180` at
that site is **LEFTOVER**.

`005B37F7` DEFAULT is **not** on `006AC910`. Callers are
clothing GUI `005B6881` and `PC_UI_FRAME` `005B8743` only.
`FirstSeenAppearancePlaysDefault=false`. First Lookout frame
is **bind pose**. **PROVEN**.

---

## 3. First-seen appearance (Graphic 4299)

Live `game.bin`:

| Def | Type | Graphic | Sub-defs |
|---|---|---|---|
| `PLAYER_HERO` | `PLAYER` | **none** (raw 21) | 0 |
| `CREATURE_HERO` | `CREATURE` | **4299** | 32 |
| `CREATURE_HERO_TRAINING` | `CREATURE` | **4299** | 32 |
| `CREATURE_HERO_CHILD` | `CREATURE` | **4300** | 33 |
| `CREATURE_YOUNG_HERO` | `CREATURE` | **4300** | 26 |

`FindMeshId("CREATURE_HERO")==4299`. `FindMeshIds` on that def
is **only** the Graphic field. **PROVEN**. That is still not a
live C3D until `006AC910`.

Attach (`hero-appearance-first`):

```
00662880 → 008388D0 → 006A5950 → 004CA010
  [esi+140] = def id
  0042AF3C(manager+32, id, &esi+112) → 009AD9E0
006A9DD0
  0042B0A2([esi+112]) push "CAppearanceDef"   // idx 10533
  004C9D60("CTCPhysicsControlled")
```

`0042AF3C` is generic def attach, not a mesh load.
`0042B0A2` does **not** change Graphic 4299. +52 clip table
`00662A00` **UNREAD**.

Clothing / hair Graphic **4126** (`MESH_HERO_FOLDED_HAT_BANDITCAMP`)
is a static 0-bone trap. **DISPROVEN** as first appearance.
Create does not `004C9D60("CTCWeapon")`. **PROVEN**
(`weapon-anim-first`).

First-scene dump after 3 pumps:

```
palskin=[4299] heroPalskin=True
HERO def=CREATURE_HERO script=Hero mesh=4299 pos=52.688,69.597,36.982
hero mesh 4299 parsed=True bones=77 skinFaces=2117 palskin=True
```

---

## 4. First-seen PALSKIN (type 5 on 4299)

Three different “PALSKIN opens”. Only the third is this create.

| What | First site vs Leave | Skin? |
|---|---|---|
| `SHADERS_PALSKIN` name | Init Engine `00B3B6D0` **before** Leave | **DISPROVEN** |
| `MBANK_ALLMESHES` directory | Init World `0049E620` after Leave | **DISPROVEN** as parse |
| Type-5 blob `00A26D40` | first `00A243B0` miss on **4299** after `006AC910` | **PROVEN** |

Live `graphics.big` id **4299**, type 5, `anim=1`, name
`MESH_HERO`:

| Field | Value | Class |
|---|---|---|
| Bones | **77** | **PROVEN** file / dump |
| Skin verts / faces | 3378 / 2117 | **PROVEN** |
| Primitives | **19** | **PROVEN** |
| Prim0 | stride **36**, flags **22**, group **9** (`torso_back`) | **PROVEN** file |
| Prim 16 / 18 | stride **28**, flags **20** (`mouth` / `eye shadow`) | **PROVEN** file |
| First dest | hierarchy × IBM ≈ **I** | **PROVEN** (`FirstSeenPlaysAnim=false`) |
| Submit palskin set | **`[4299]` only** | **PROVEN** |

Kid **4300** is 76 bones / 4 prims / stride 28 / flags `0x14`.
**DISPROVEN** as this Present.

Native family is bits `0x80` / `0x100` → `00BD71B0`, not
static `0x20`. First primitive DIP **PARTIAL**. Host
`SubmittedHeroPalskin` means `BoneCount>0` membership.
**DISPROVEN** as that DIP (`palskin-after-leave`).

`WorldShading.FirstSeenPalskinStrideBytes=28` is a **kid
LEFTOVER** name vs adult prim0 **36**.

---

## 5. First-seen bone (77-bone C3D, not `.bncfg`)

First *skeleton data* is the type-5 blocks on 4299
(`00A89450` / `00A894ED`):

```
[esi+152]  bone count 77
  u16[count] name offsets
  framed LZO names
  60-byte info     (getter 00A4BD70: base+156 + i*60)
  48-byte local TRS
  64-byte IBM
```

Chain (live dump / `E` §6): `Scene Root` → `Movement_dummy` →
`Sub_movement_dummy` → `Bip01` … Weapon slots 73–76 idle.

First *evaluate*: packer `00BD2D90` → `00AA0090` (channels **0**)
→ tail `00A9E1E0` parent walk → `00BD2F91` dest = S × IBM ≈ I
→ `00BCFB00` `c38`. **PROVEN** (`bone-after-leave`).

Not these:

| Candidate | Why not first 4299 skeleton |
|---|---|
| Frontend / Leave | no C3D |
| `CSkeletalMorphDef` | kid 4300 name intern |
| Mixer `00AA0F60` | empty dest holder |
| `.bncfg` `006C37D0` | XYZ scales, not C3D |
| Static Lookout `0x20` | 0 bones |
| Lookout AICreature type-5 | exist; not first Present set |

---

## Host vs native

| Host | Native first-seen | Class |
|---|---|---|
| `PumpFrontendFrame` no `Meshes.Get` | no type 5 | **MATCH** |
| `ResolveHeroDefinition` PLAYER_HERO miss → CREATURE_HERO | `00449D90` / `00449E0D` | **MATCH** id. Site on `SpawnHero` is **LEFTOVER** vs first `0049F180` |
| `HeroMeshId=4299` / `ScriptName=Hero` | Graphic field after `006AC910` | **MATCH** |
| `SpawnHero` after ContainsMap | first `006AC910` after maps | **MATCH** order |
| Second `Note(InitCharactersFn)` in `SpawnHeroFromPlayerStart` | `0049F180` is not called from `006AC910` | **LEFTOVER** |
| Recover text “Hero via `0049F180` → `006AC910`” | two different times | **LEFTOVER** wording |
| `SubmittedPalskinMeshIds == [4299]` | one PALSKIN Graphic | **MATCH** set |
| `SubmittedHeroPalskin` | not `00BD71B0` | **DISPROVEN** as draw |
| `FirstSceneWorld` kid 4300 / SHOT2 | Lookout adult | **LEFTOVER** |

---

## Classification table

| Claim | Status |
|---|---|
| Frontend / Leave creates mesh 4299 | **DISPROVEN** |
| `0049F180` creates the Hero Thing | **DISPROVEN** (bind + failed `00489D40`) |
| First Hero **name** after Leave is `00449D90` `PLAYER_HERO` miss → `CREATURE_HERO` | **PROVEN** |
| First Hero **Thing** is `006AC910` at `GuildArrivalHSP` | **PROVEN** identity. Native retry caller **UNREAD** |
| First Hero **Graphic** is **4299** `MESH_HERO` | **PROVEN** |
| That Graphic is kid 4300 / father / 4126 | **DISPROVEN** |
| `PLAYER_HERO` supplies the mesh | **DISPROVEN** (no Graphic) |
| First PALSKIN payload / submit set is `[4299]` | **PROVEN** |
| First C3D skeleton is 77 bones on 4299, dest ≈ I | **PROVEN** |
| Create plays DEFAULT / STAND / a clip | **DISPROVEN** |
| `00DBDE40` / `CREATURE_HERO_CHILD` is this create | **DISPROVEN** |

---

## Do not

- Draw or parse 4299 / 4300 on frontend frames.
- Treat `0049F180` / first `00489D40` as the Thing create.
- Use `CREATURE_HERO_CHILD` / 4300 / Oakvale kid as first
  Lookout appearance, PALSKIN, or skeleton.
- Submit Graphic **4126** as worn hair / hat / beard.
- Call `005B37F7` from `006AC910`.
- Treat `SubmittedHeroPalskin` as `00BD71B0`.
- Treat `.bncfg` / mixer / `CSkeletalMorphDef` as the 4299
  hierarchy.

Next recoverable slice is still the later `00489D40` caller
that first hits `00489FC1`, plus per-prim PALSKIN / `c38` for
**4299** and `CAppearanceDef` +52 — not a second Graphic id.
