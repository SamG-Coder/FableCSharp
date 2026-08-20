# First Present PALSKIN id: 4299, not kid 4300

Investigation only. Production `src/` was not edited.

Do **not** invent kid **4300** on first Present.
Do **not** invent `CreateCharacter` **4300** on Pump.
Do **not** collapse leftover **#4** (Lookout Present vs
Oakvale intro view). Kid **4300** is a `FirstSceneWorld`
fixture, not `EngineLifecycle.Pump`.

`CHeroCentreDef` is **not** the `SI_HERO_CHILD` cluster.

Question: does first Present PALSKIN draw Graphic **4299**?
When does **4300** bind? What is PALSKIN type1 bit **`0x80`**
on unread 4300?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH** / **DIVERGE**.

Authority: listings `00BD77FE` / `00BD780D` / `00B33010`
(`palskin-queue-slots-00bd7838-00bd780d.md`,
`palskin-draw-entry-00bd71b0-00bd71b0.md`,
`listing-00bc0000.txt`, `listing-00b00000.txt`);
`00DBDF08` `"CREATURE_HERO_CHILD"`
(`q-newoakvaleintro-preattack-00dbde40.md`);
siblings `proofs/palskin-child-hero`,
`proofs/palskin-type1-0x80-4300`,
`proofs/palskin-type1-0x80-kid`,
`proofs/hero-palskin-first-submit`,
`proofs/hero-appearance-first`,
`proofs/0049F180-first-children`,
`proofs/004F3338-hero-centre`,
`proofs/leftover-4-collapse-audit`,
`proofs/audit-firstsceneworld`,
`proofs/00DBDE40-after-activate`;
`docs/status/README.md` leftover #4;
`WorldShading.cs`, `FirstSceneWorld.cs`,
`EngineLifecycle.SubmitCurrentWorld` / `PresentWorld`
(read only); `assembly/compiled-defs/game/entries.tsv`;
tests `EngineLifecycleTests`
`HeroMeshId==4299` / `SubmittedPalskinMeshIds` contains 4299,
`GameBinFormatTests` `FindMeshId`,
`Kid_4300_flag1_hair_drains_0x200_after_sky`,
`WorldGeometryTests` `PlayerMeshId==4300`,
`WorldPipelineTests` FirstSceneWorld `DoesNotContain 0x80`,
`ScenePassTests` `DrawnPasses(Palskin)` no `0x80`.

---

## Verdict

**Yes. First no-save Present PALSKIN draws Lookout adult
Graphic 4299 `MESH_HERO` (`CREATURE_HERO` at
`GuildArrivalHSP`). Not kid 4300.**

Kid **4300** `MESH_YOUNGHERO_02` binds as a **def Graphic
field** on `CREATURE_HERO_CHILD` / `CREATURE_YOUNG_HERO` in
`game.bin`. It is **not** the first `0049F180` name bind
(that is `"CREATURE_HERO"`). It is **not** the first Hero
Thing (`006AC910` is 4299). Host live instance is the
`FirstSceneWorld` / `WorldGeometry.IsPrimaryStart` clone
only. Native live lookup is later `00DBDE40` after a
**proven** `Q_NewOakValeIntro` activate (`00DBDF08`
`"CREATURE_HERO_CHILD"`). That fiber is leftover vs Pump.

Type1 **`0x80`** is MainScene drain of queue **slot 14**
after sky, filled only when `[inst+104]+8==1`. On 4300 that
dword is **UNREAD as 1**. There is **no** first-Present 4300
DIP to put it on. Do not invent one.

Leftover **#4** stays **open**: Lookout Present (this id)
and Oakvale intro view (fixture 4300) are **two ledgers**.

| Claim | Class |
|---|---|
| First Present PALSKIN Graphic is **4299** | **PROVEN** |
| First Present PALSKIN Graphic is **4300** | **DISPROVEN** |
| Kid 4300 is Pump / `SubmitCurrentWorld` | **DISPROVEN** |
| Kid 4300 is `FirstSceneWorld` fixture | **PROVEN** (host). **LEFTOVER** as first Present |
| `0049F180` miss is `"CREATURE_HERO"` not CHILD | **PROVEN** |
| Type1 `0x80` = slot 14 when `[inst+104]+8==1` | **PROVEN** formula |
| Type1 `0x80` as a 4300 first-Present DIP | **UNREAD** / **skip**. No such submit |
| `CHeroCentreDef` in `SI_HERO_CHILD` cluster | **DISPROVEN** |
| Collapse leftover #4 | **DISPROVEN** |

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Does first Present PALSKIN draw 4299? | **Yes.** Lookout `CREATURE_HERO` `MESH_HERO`. Submit set **`[4299]`**. | **PROVEN** |
| First-seen hero def? | `PLAYER_HERO` miss → **`CREATURE_HERO`**. Adult at `GuildArrivalHSP`. | **PROVEN** |
| When does 4300 bind? | Def Graphic on CHILD / YOUNG_HERO in `game.bin`. Host instance: `FirstSceneWorld` clone. Native Thing: later `00DBDE40` after proven intro activate. **Not** first Present. | **PROVEN** split |
| Is 4300 Pump? | **No.** Pump `PresentWorld` `expand=false` on Lookout already has adult `CREATURE_HERO`. `IsPrimaryStart` clone **does not run**. | **DISPROVEN** |
| Type1 `0x80` meaning? | Drain of queue **slot 14** after sky. Filled only when `[inst+104]+8==1`. Not Flag1 `0x200`. Not Duration. Not C3D type 5. | **PROVEN** |
| Type1 `0x80` on unread 4300? | **No 4300 DIP on first Present.** On the 4300 *file*, slot 14 stays empty unless that dword is 1 (**UNREAD as 1**). Host skip **MATCH**es type0 + Flag1 hair. | **UNREAD** as submit. Host skip **MATCH** |
| `CHeroCentreDef` as kid PALSKIN? | **No.** Pair 41 registrar. Not `SI_HERO_CHILD` `10537`…`10543`. Not a Graphic. | **DISPROVEN** |

---

## Timeline (no-save New Game)

```
0042F2A2  Leave frontend
  009BE420 + 009BEEB0 Present            // black; no PALSKIN
0042F491  Init Game
  004EE23F  Init Thing Components
    004F3338  CHeroCentreDef             // Note-only class; not mesh
  00416005  Init Definition Manager      // game.bin Graphic fields
  00416953  Loading world FinalAlbion.wld
    0049F180 Init Characters  (push 0)
      00449D90 PLAYER_HERO miss
      00449E0D "CREATURE_HERO"           // NOT CHILD, NOT 4300
      00489D40 holy-site miss → ret 0    // no 006AC910
004189C2  dummy pumps                    // HeroSpawned=false
later 00501450 LookoutPoint              // leftover #4 Present ledger
  006C2170 Loading objects
  HOLY_SITE_PLAYER_START GuildArrivalHSP
    006AC910 CThingPlayerCreature::Create
      Graphic 4299 MESH_HERO             // FIRST Hero Thing
then first 00A243B0(id=4299) miss
  00A26D40 type 5                        // FIRST PALSKIN payload
first 3D Present
  0x4 / 0x40 landscape
  0x20 static C3D (props; not Hero)
  0x80 / 0x100 PALSKIN dest 4299         // FIRST PALSKIN submit set
```

`CREATURE_HERO_CHILD` / 4300 / `00DBDE40` / `FirstSceneWorld`
are **not** on this list. **PROVEN.**

---

## 1. First Present PALSKIN draws 4299 — PROVEN

No-save first real region is WLD index **1** `LookoutPoint`
(`00501450` → `00500540(1,0,0)`). Marker is
`HOLY_SITE_PLAYER_START` **`GuildArrivalHSP`**. Lookout TNG
has **no** `PlayerCreature` / `CREATURE_HERO` / CHILD.

`0049F180` (`proofs/0049F180-first-children`):

```
009AD410("PLAYER_HERO")
0044BA90 fail                  // PLAYER has no Graphic
00449E0D push "CREATURE_HERO"  // not CREATURE_HERO_CHILD
0048A070 → 00489D40
  00488B20 holy-site miss
  [0x13B8647]==0 → ret 0       // no 006AC910
```

Live `game.bin` (`entries.tsv` / `GameBinFormatTests`):

| Def | Type | Graphic | SubDefs |
|---|---|---|---|
| `PLAYER_HERO` | `PLAYER` | **none** | 0 |
| `CREATURE_HERO` | `CREATURE` | **4299** | 32 |
| `CREATURE_HERO_TRAINING` | `CREATURE` | **4299** | 32 |
| `CREATURE_HERO_CHILD` | `CREATURE` | **4300** | 33 |
| `CREATURE_YOUNG_HERO` | `CREATURE` | **4300** | 26 |

`FindMeshId("CREATURE_HERO")==4299` **PROVEN**. First-seen
name bind is that row, not CHILD.

Later `006AC910` at HSP writes Graphic **4299**. First
`00A243B0` miss on that id is the first type-5 payload.
Same Present as first Lookout 3D frame, after land + static
`0x20` (`c3d-first-submit`): PALSKIN family `00BCE740` →
`00B84720` → `00BD7110` / `00BD71B0`.

Dump after three pumps
(`2026-08-18-first-scene-things.dump.txt`):

```
pump3 lookout region=LookoutPoint index=1 hero=True mesh=4299
palskin=[4299] heroPalskin=True
HERO def=CREATURE_HERO script=Hero mesh=4299 pos=52.688,69.597,36.982
unique palskinMeshes=1
```

`EngineLifecycleTests.Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`:

- `HeroMeshId == 4299`
- `SubmittedPalskinMeshIds` contains **4299**
- `SubmittedWorld.Region == LookoutPoint`
- `Expanded == false`

Live file **4299** `MESH_HERO` type 5: **77** bones, **19**
prims, prim0 stride **36** / flags **22**. Kid **4300** is
76 / 4 / stride 28. **DISPROVEN** as this Graphic.

`006AC910` does **not** parse type 5 and does **not** DIP.
Host extra `Meshes.Get(HeroMeshId)` is **LEFTOVER** dead:
PresentWorld already has the instance (`seen.Add` fails).
`SubmittedHeroPalskin` is `BoneCount>0` **membership**, not
`00BD71B0`. Keep as membership.

---

## 2. When 4300 binds — not first Present

Three different “4300 binds”. Only the first is on the
no-save walk, and it is a **file field**, not a Thing.

### Def Graphic (Init Definition Manager)

`CREATURE_HERO_CHILD` id **1472** Graphic **4300**.
`CREATURE_YOUNG_HERO` same mesh. That is `game.bin` parse.
It does not spawn a Thing and does not DIP.

### First `0049F180` — DISPROVEN as CHILD

Miss immediate is `"CREATURE_HERO"`. Kid is a different
def. First-seen `00489D40` returns 0. **PROVEN.**

### Host fixture — `FirstSceneWorld`, not Pump

```
FirstSceneWorld.Build
  Region = StartOakValeWest          // leftover #4 intro ledger
  WorldGeometry.Build expand=true
    IsPrimaryStart && no existing hero
      CloneAs(NOVStartHSP, CREATURE_HERO_CHILD)
      PlayerMeshId = 4300
```

`WorldGeometryTests.New_game_oakvale_loads_contains_and_sees_maps`
locks `PlayerMeshId==4300`. Callers of `FirstSceneWorld.Build`
are **tests only** (`WorldPipelineTests` /
`ScriptRuntimeParityTests`). Zero callers in `Fable.Client`
or `EngineLifecycle.Pump`. Type comment: **not** Pump;
no-save Present is LookoutPoint. **Do not collapse leftover
#4.**

Pump `PresentWorld` uses `expandGeometry: false` on
LookoutPoint and already has adult `CREATURE_HERO` in
`_regionThings`. `existingHero` is set → kid clone
**does not run**.

### Native live Thing — leftover intro fiber

`00DBDE40` (only `E8` is `00DAC295` inside `00DABAC0`,
`Q_NewOakValeIntro` slot 2):

```
map-wait "StartOakVale"
00CB7940 abort?
READ [this+80] AttackOver
00DBDF08  push "CREATURE_HERO_CHILD"
          004AA840 construct
WatchBarrels / WatchForGotGold / ManageQuestCoreMarkers
Q_NewOakValeIntro_PreAttack
12 s wait, HerosOldHouse
```

No-save **does not** activate `Q_NewOakValeIntro`
(`No_save_does_not_activate_Q_NewOakValeIntro`). Gameflow
**yields** on the name. Do **not** wire `00DBDE40` into
`PumpGameUpdate`. Do **not** call that first Present.

`CHeroCentreDef` does **not** change this split (next
section).

---

## 3. `CHeroCentreDef` is not `SI_HERO_CHILD`

`004EE23F` pair 41 (`proofs/004F3338-hero-centre`):

```
004F3310  push "CHeroCentreDef"
004F3320  push 0x4D86F0
004F3338  call 0044C6B0
004F333F  call 009B0AC0
```

Factory size **37**, vtbl `0123BE54`. Host Notes + flag
only. **Not** a live object. **Not** a Graphic. **Not** a
Lookout TNG kind (those are OBJECT / MARKER / THING /
AICreature / Holy Site).

`game.bin` instances: **2**. NULLDEF **40** and id **9456**.
Both raw **3**, subdefs **0**. Id **9456** sits in a
`CAppearanceDef` / `CShopDef` / `CMultiStaticMeshDef`
cluster (`9447`…`9463`), **not** the hero-creature cluster.

Adjacent compiled-def cluster at `SI_HERO_CHILD`
(`10537`…`10543`):

| Id | Type | ASCII |
|---|---|---|
| 10537 | `CAppearanceDef` | *(clip names)* |
| 10538 | `CPhysicsDef` | |
| 10539 | `CCreatureDef` | |
| 10540 | `CHeroDef` | |
| 10541 | `CEntitySoundDef` | **`SI_HERO_CHILD`** |
| 10542 | `CSkeletalMorphDef` | |
| 10543 | `CHeroMorphDef` | |

**No** `CHeroCentreDef`. **DISPROVEN** in that cluster.
`CREATURE_HERO_CHILD` has **33** `SubDefs`; the 33-row
type list is **UNREAD** (`Fable.Dump bin` prints eight).
Do **not** invent this class as kid PALSKIN identity, as
first-Present child types, or as type1 `0x80`.

---

## 4. Type1 `0x80` — slot 14, unread as 4300 submit

Queue inside `00BD71B0` (`edi` = instance):

```
00BD77FE  mov eax, [edi+104]
          test eax, eax
          je  00BD7958                 ; null → no queue
00BD780D  mov eax, [eax+8]             ; [inst+104]+8
          sub eax, 0
          je  00BD789C                 ; type 0 → slot 8
          dec eax
          jne 00BD7958                 ; not 0/1 → skip
; type 1:
          00BCE740 helper
          00B84720(10)                 ; slot 10 → bit 0x100
          00BCE740 helper
          00B84720(14)                 ; slot 14 → bit 0x80 after sky
```

Type 0 Flag1 extra (`[mat+41]` at `00BD78D9`) → slot **9** /
**`0x200`**. Type1 **never** reads Flag1.

Drain `00B33010`:

```
00B33083  cmp eax, 0x80
          je  00B3311A                 ; push 14 / 00B849F0
00B33183  cmp eax, 0x100
          je  00B331BD                 ; slots 8 then 10
00B3318A  cmp eax, 0x200
          … 00B331AA push 9            ; Flag1 extra
```

Registration (`ScenePasses.Registration`, 34 layers):

```
0x20 static → 0x100 PALSKIN 8+10 → 0x2000 sky → … → 0x80 slot 14 → 0x200 slot 9
```

`WorldShading`: `PalskinQueueSlotType1A=10`, `Type1B=14`,
type0=8, Flag1 extra=9. `InstanceDraw.PalskinPassBit80`:
“Not first-seen 4300 (`[inst+104]+8` unread as 1).”

Not:

| Tempting alias | Why not |
|---|---|
| Kid 4300 first Present layer | **No 4300 DIP** on this Present |
| Flag1 hair `0x200` | Type0 extra slot **9**. Type1 drops it |
| `Duration=1` / XSEQ | Integer queue dword after `sub 0` / `dec` |
| C3D bank type **5** | 4299 and 4300 are both type 5 |
| Helper `+28` type index 4 | Hair MapFlags; bind does not read it |
| `CHeroCentreDef` | Registrar pair, not a queue dword |

Kid **4300** file (`Kid_c3d_*`): four prims, hair Flag1=1,
body Flag1=0. That shape is **type0 + Flag1 extra**. If 4300
were type1, Flag1 would **not** add slot 9 and hair would
land on `0x80` instead of `0x200`. First-seen MATCH cannot
be both (`palskin-type1-0x80-kid`).

Live `[inst+104]+8` as **1** is **UNREAD** (no dword dump;
writer of `[inst+104]` **UNREAD**). First-seen 4300 submit
of `0x80` is still **skip**:

```
DrawnPasses(Palskin, 0) = [0x100]
DrawnPasses(Palskin, 1) = [0x100, 0x200]   // never 0x80
Kid_4300_flag1_hair_drains_0x200_after_sky  DoesNotContain 0x80
WorldPipelineTests FirstSceneWorld          DoesNotContain 0x80
```

Registration still **walks** bit `0x80` (index 25). Empty
drain of slot 14 is not a kid DIP. Native first Present
PALSKIN **family** includes bits `0x80` / `0x100` on Graphic
**4299** (`hero-palskin-first-submit`). Filling slot 14
still needs type==1; that live dword on 4299 is **UNREAD**
as well. Host Pump `DrawnPasses` emits only `0x100`
(**DIVERGE** vs native drain visit of `0x80` on 4299, **not**
a 4300 layer).

Do **not** invent 4300 triangles on `0x80` to close that
DIVERGE.

---

## 5. Two sites — leftover #4 stays open

| Ledger | Native pairing | First no-save Present? |
|---|---|---|
| LookoutPoint WLD index 1 | `GuildArrivalHSP` / `006AC910` / **4299** / camera `006B3FF0` FOV **70** | **yes** |
| Oakvale intro view | `StartOakValeWest` / `CAM_OVIF_SHOT2` / kid **4300** / `Q_NewOakValeIntro` / `00DBDE40` FOV **72** | **no** |

`docs/status/README.md` leftover #4: do not collapse those
ledgers. `leftover-4-collapse-audit`: **CLEAN** as a live
Present bug; leave #4 **open as ledger pairing**. This proof
does **not** close #4 by proving 4299.

Pump vs fixture PALSKIN:

| Site | Graphic | Bones | Host layers | Native family |
|---|---|---|---|---|
| Pump no-save | **4299** `MESH_HERO` | 77 | `0x100` only | `0x80` + `0x100` drain (**DIVERGE** host). **Not 4300** |
| FirstSceneWorld | **4300** `MESH_YOUNGHERO_02` | 76 | `0x100` + Flag1 `0x200`; **no `0x80`** | type0 skip of slot 14 |

---

## Host vs native

| Host | Native first-seen | Class |
|---|---|---|
| `PumpGameUpdate` PALSKIN **4299** | Lookout adult `00BD71B0` | **MATCH** Graphic. **DISPROVEN** as 4300 |
| Extra `Meshes.Get(HeroMeshId)` | create does not DIP | **LEFTOVER** (dead) |
| `FirstSceneWorld.PlayerMeshId=4300` | Oakvale intro leftover | **PROVEN** fixture. **DISPROVEN** as Pump |
| `DrawnPasses` no `0x80` on 4300 | type0 skip of slot 14 | **MATCH** skip. Type dword **UNREAD** as 1 |
| `DrawnPasses` only `0x100` on Pump 4299 | drain also visits `0x80` | **DIVERGE** vs 4299. Not a 4300 layer |
| `CHeroCentreDef` Note-only | registrar pair 41 | **MATCH** Notes. **DISPROVEN** as mesh |
| `SI_HERO_CHILD` cluster without this class | `10537`…`10543` | **MATCH** |
| Leftover #4 still two ledgers | Lookout Present vs Oakvale view | **MATCH** open |

---

## Do not

- Invent kid **4300** on first Present / Pump /
  `SubmittedPalskinMeshIds`.
- Invent `00489D40` / `006AC910` Graphic **4300**.
- Call `FirstSceneWorld.Build` the no-save Present.
- Collapse leftover **#4** (Lookout Present vs Oakvale
  intro). Proving 4299 does **not** close #4.
- Wire `00DBDE40` / `Q_NewOakValeIntro` into
  `PumpGameUpdate`.
- Invent a 4300 DIP on type1 `0x80` (hair, mouth, second
  body, clothing 4126, `Duration=1`).
- Treat Flag1 `0x200` as type1 `0x80`.
- Fold type1 into `DrawnPasses` without `[inst+104]+8==1`.
- Pair `CHeroCentreDef` with `SI_HERO_CHILD` or with this
  PALSKIN id.
- Submit Graphic **4126** as this Present.

Next leftover on **4300** is still the **writer** of
`[inst+104]+8` and Oakvale spawn, not a Pump
CreateCharacter. On Pump the leftover is 4299 per-prim +
`c38` (which primitive is first `00BD71B0`), not this id.
Leftover **#4** stays the Lookout-vs-intro **ledger**.
