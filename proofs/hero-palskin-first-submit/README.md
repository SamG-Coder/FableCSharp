# First PALSKIN submit of Hero 4299 after Leave

Investigation only. Production `src/` was not edited.

Do **not** start at Oakvale / `CAM_OVIF_SHOT2` / father /
`CREATURE_HERO_CHILD` / Graphic **4300** / `00DBDE40`.
Those are leftover `Q_NewOakValeIntro`, not Leave / Init Game /
first no-save 3D Present.

Do **not** collapse `006AC910` create, `MeshBank.Get`, or
`SubmittedHeroPalskin` into a PALSKIN DIP.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Question: after Leave Frontend, what is the first PALSKIN
**submit** of Hero Graphic **4299**? Is that Host `MeshBank.Get`
/ extra-submit leftover, or `006AC910`?

Authority: `proofs/hero-4299-create`, `proofs/palskin-after-leave`;
siblings `palskin-open`, `hero-appearance-first`, `c3d-first-submit`,
`meshbank-after-leave`, `bone-after-leave`;
`docs/status/investigations/E-player-palskin.md`,
`2026-08-18-palskin.md`, `2026-08-18-first-scene-things.md`,
`2026-08-18-resource-manager.md`;
listings / dumps `006AC910`, `00A243B0`, `00A26D40`,
`00BCE740`, `00B84720`, `00BD7110`, `00BD3070`, `00BD71B0`,
`00BD2D90`, `00BCFB00`;
`EngineLifecycle.SubmitCurrentWorld` / `SpawnHero` / `InsertThing`
(read only); `MeshBank.Get`; `MeshBatches.BuildMeshes`;
`EngineLifecycleTests.Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`.

---

## Verdict

**First PALSKIN *submit* after Leave is Lookout adult Graphic
4299 `MESH_HERO`, on bits `0x80` / `0x100` through
`00BD71B0`.** Not frontend. Not `006AC910`. Not Host
`MeshBank` leftover extra-submit.

`006AC910` is the first Hero **Thing**. It does not parse type 5
and does not DIP. Host extra-submit of `HeroMeshId` exists because
“`006AC910` is a Thing, not a TNG Graphic”. PresentWorld already
has that instance. The extra `Meshes.Get` **does not fire**.
**LEFTOVER** vs native create.

| Layer | First after Leave | Class |
|---|---|---|
| Frontend / Leave Present | no PALSKIN DIP | **DISPROVEN** |
| `MBANK_ALLMESHES` directory | Init World `0049E620` | **PROVEN** open. **DISPROVEN** as submit |
| `006AC910` Create | GuildArrivalHSP Thing, Graphic field **4299** | **PROVEN** identity. **DISPROVEN** as DIP |
| Host `InsertThing` / `009AD410` Note | Graphic id only; no `Meshes.Get` | **PROVEN** id. Note pairing **LEFTOVER** vs `009AD9E0` |
| Type-5 payload | first `00A243B0` miss on **4299** | **PROVEN** id. Caller **PARTIAL** |
| Host `PresentWorld` `expand=false` | `TryGetEntry` handle; `ParsedCount` unchanged | **MATCH** `009AD410`. **DISPROVEN** as parse |
| Host extra `seen.Add(HeroMeshId)` | dead: 4299 already in primary instances | **LEFTOVER** |
| Host `SubmitCurrentWorld` `Meshes.Get(4299)` | first live parse | **MATCH** timing vs `00A243B0`. **DISPROVEN** as `006AC910` / `00BD71B0` |
| Native PALSKIN DIP | helper `00BCE740` → queue `00B84720` → drain `00BD7110` / `00BD71B0` | **PROVEN** family. First primitive **PARTIAL** |
| Submit set | **`[4299]` only** | **PROVEN** |
| Host `SubmittedHeroPalskin` | `BoneCount>0` membership | **DISPROVEN** as draw |
| Kid 4300 / father / 4126 | Oakvale / hat trap | **DISPROVEN** |

---

## Timeline (no-save New Game)

```
0042F2A2  Leave frontend
  009BE420 + 009BEEB0 Present            // black; no 4299
0042F491  Init Game → 004184BD
  Init World 004A6E30
    0049E620 MBANK_ALLMESHES directory   // ParsedCount=0
  00416953  Loading world FinalAlbion.wld
    0049F180 Init Characters
      00449D90 PLAYER_HERO miss → CREATURE_HERO
      00489D40 holy-site miss → no 006AC910
004189C2  dummy pumps                    // HeroSpawned=false; no submit
later 00501450 LookoutPoint
  006C2170 Loading objects
    Graphic HANDLE 009AD410              // static props; no type 5
  HOLY_SITE_PLAYER_START GuildArrivalHSP
    006AC910 CThingPlayerCreature::Create  size 0x208
      004C7380 / 0052AB20 / 006A9DD0
      004CA010 Graphic field 4299
      004C9CA0 activate
      NO 00A243B0 / NO 00BD71B0
then first 00A243B0(id=4299) miss
  vtbl+48 00A26D40 type 5                // FIRST payload (open, not DIP)
next 00B27D90 with records
  0x4 / 0x40 landscape
  0x20 static C3D 00BB2540               // props; not Hero
  0x80 / 0x100 PALSKIN
    00BCE740 helper
    00B84720 slots 8 / 10 / 14
    00B33010 → 00B849F0
      vtbl+20 00BD7110
        helper+32==0 → 00BD3070
          first-seen 00BD549D
            00BD2D90 pack dest ≈ I
            00BCFB00 c38
            00BD71B0 [this+8] enable     // FIRST Hero PALSKIN submit
```

`CREATURE_HERO_CHILD` / 4300 / `00DBDE40` are **not** on this
list. **PROVEN**.

---

## 1. `006AC910` is create, not submit

`006AC910` (`cthingplayercreature-create-006ac910.md`):

```
006AC910  alloc 0x208 (004C7380)
          0052AB20
          006A9DD0 ConstructFromParams
006AC9D4  004C9CA0 activate
006ACA13  ret 8
```

Zero `E8` to `00A243B0` / `00A26D40` / `00BCE740` / `00B84720` /
`00BD71B0`. Graphic **4299** is the def field after
`004CA010`. That is identity for the later submit. It is **not**
the submit.

Host `SpawnHero` → `InsertThing`:

- Notes `006AC910` / `006A9DD0` / `004CA010` / `0042AF3C`
- `ResolveSubmit` → first Graphic **4299** (`FindMeshIds`)
- Notes `009AD410 mesh=4299` (**LEFTOVER** wording: native id
  attach is `009AD9E0`; `009AD410` is the earlier *name* HANDLE)
- **does not** `Meshes.Get`
- appends `Hero` to `_regionThings` and
  `_thingsByMap[LookoutPoint]`

So the Thing is in PresentWorld before any PALSKIN parse.

---

## 2. Host MeshBank leftover vs that create

Three Host `MeshBank` sites get confused with `006AC910`.

### Directory (`0049E620`) — not this submit

Init World after Leave. `Opened=true`, `ParsedCount=0`.
**PROVEN** (`meshbank-after-leave`). **DISPROVEN** as PALSKIN.

### Extra-submit in `SubmitCurrentWorld` — leftover

```
// 006AC910 spawn is a Thing, not a TNG
// Graphic. Submit it as PALSKIN even if
// PresentWorld missed the instance.
if (HeroMeshId != 0 && Hero has XYZ && seen.Add(HeroMeshId))
    Meshes.Get(HeroMeshId) + ObjectTransform(Hero)
```

PresentWorld (`expandGeometry: false`) already walks Lookout
Things, including the appended Hero. `AddInstances` uses
`TryGetEntry` (handle) and records `WorldMeshInstance` mesh
**4299**. The C3D loop then `Meshes.Get(inst.MeshId)` and
`seen.Add(4299)` **before** the fallback. `seen.Add` fails.
Fallback **does not run**.

Dump: `Hero fallback | not used`
(`2026-08-18-first-scene-things.md` §13). **PROVEN** dead.
**LEFTOVER** comment pairing create with submit.

Native never “extra-submits” 4299 because TNG missed it.
`006AC910` inserts the Thing; later drain draws the Graphic.

### `WorldGeometry.Build` `meshes ??= new MeshBank()` — leftover helper

Production `PresentWorld` / `SubmitCurrentWorld` pass
`EngineLifecycle.Meshes`. A second bank would DIVERGE from one
`00A27030` / `[0x13B8A04]`. Kid clone on `IsPrimaryStart` is
**DISPROVEN** for Lookout (`existingHero` is adult
`CREATURE_HERO`). `FirstSceneWorld.TracePalskin` father is
Oakvale **LEFTOVER**.

`MeshBank.Get` comment “On-demand `009AD410` then parse” is
**LEFTOVER** pairing. `009AD410` is handle. Get-or-load is
`00A243B0` vtbl+52.

---

## 3. First payload vs first submit

Do not collapse open and DIP (`palskin-after-leave`).

| Step | Native | Host |
|---|---|---|
| Handle | `009AD410` on apply / create | `TryGetEntry` in PresentWorld |
| Get-or-load | `00A243B0` miss → `00A26D40` type 5 | `Meshes.Get` in `SubmitCurrentWorld` |
| File | `00A89450` / `00A8FD40` 77 bones / 19 prims | `MeshFile.TryParse` same id |
| Dest pack | `00BD2D90` → `00A9E1E0` × IBM ≈ I | file `Triangles` already dest |
| Submit | `00BCE740` / `00B84720` / `00BD71B0` | `BuildMeshes` soup + `World` |

Host first parse of 4299 is among the **45** primary Lookout
ids (193 instances). Timing vs first `00A243B0` miss is
**MATCH**. Flatten after is **DIVERGE**. Exact native miss
caller (construct vs first `00BD71B0`) remains **PARTIAL**.

Live file (Graphic **4299** `MESH_HERO`, type 5):

| Field | Value | Class |
|---|---|---|
| Bones | **77** | **PROVEN** |
| Skin faces | 2117 | **PROVEN** |
| Prims | **19** | **PROVEN** |
| Prim0 | stride **36**, flags **22**, group **9** | **PROVEN** |
| First dest | bind identity (`FirstSeenPlaysAnim=false`) | **PROVEN** |
| Submit set | **`[4299]`** | **PROVEN** |

Kid **4300** is 76 bones / 4 prims / stride 28. **DISPROVEN**.

---

## 4. Native first PALSKIN submit (after maps + create)

Same Present as first Lookout 3D frame, **after** land and
static props (`c3d-first-submit`):

| Bit | Family | Hero 4299? |
|---|---|---|
| `0x4` / `0x40` | landscape `00BF4570` | **no** |
| `0x20` | static `00BB2540` | **DISPROVEN** |
| `0x80` | PALSKIN type1 slot **14** | **yes** family |
| `0x100` | PALSKIN slots **8+10** | **yes** family |
| `0x200` | Flag1 slot **9** | later / **PARTIAL** |
| `0x2000` | sky | **no** |

Queue (`00BD780D`):

```
type==1 → 00BCE740 then 00B84720(10) and 00B84720(14)
type==0 → 00BCE740 then 00B84720(8)  [Flag1 → slot 9]
```

Drain:

```
00B33010 bit 0x80 / 0x100
  00B849F0
    vtbl+20 00BD7110
      [helper+32]==0 → 00BD3070     // first-seen
        00BD549D default (not type4 00BD3C04)
          00BD2D90 [this+288]==0 pack
          00BCFB00 SetVSConstantF c38
00BD71B0  test [ecx+8]; je ret 12   // enable
          DIP family VSHADER_PALSKIN_DIRLIGHT_FOG
```

First primitive in slot-walk order is **PARTIAL**. Family and
Graphic id are **PROVEN**. Dest ≈ I. No `005B37F7` DEFAULT on
create (`hero-4299-create`).

Lookout AICreature type-5 Things exist. Host `ResolveSubmit`
bails on null XYZ. They are **not** the first PALSKIN set.
**PROVEN** exist / **DISPROVEN** as this submit.

---

## 5. Host submit leftover (not `00BD71B0`)

`PumpGameUpdate` after `006C2170` / `HeroSpawned`:

```
if HeroSpawned && !WorldSubmitted
  SubmitCurrentWorld
    PresentWorld expand=false          // handles
    foreach primary Instances
      Meshes.Get + props.Add           // 193 Lookout C3Ds incl. 4299
    extra HeroMeshId                   // LEFTOVER; does not fire
    MeshBatches.BuildMeshes(props)
      BoneCount>0 → SceneLayer.Palskin
      file-local verts + draw.World
    Concat(objects, sky) then Concat(land, that)
```

`DrawnPasses(Palskin)` emits **only** bit `0x100`. Native also
fills slot 14 on `0x80`. Host **DIVERGE** layer set.

`SubmittedHeroPalskin=true` means 4299 is in
`SubmittedPalskinMeshIds` because `BoneCount>0`. Tests lock
that membership (`HeroMeshId==4299`, palskin contains 4299).
That is **not** `00BD71B0`. Keep as **membership**.

`BuildMeshes` no longer `TrianglesForPose()` +
`Vector3.Transform` into verts (file dest already
`00A9E1E0` × IBM; W is instance). Still one soup, not 19
PALSKIN records + `c38`. **DISPROVEN** as character draw (E).

One-shot `WorldSubmitted` vs every `00B27D90`: **TEMPORARY
BRIDGE**.

---

## Host vs native

| Host | Native first-seen | Class |
|---|---|---|
| `PumpFrontendFrame` no `Meshes.Get` | no type 5 / no `00BD71B0` | **MATCH** |
| `OpenMeshBank` directory after Leave | `0049E620` | **MATCH** |
| `SpawnHero` / `HeroMeshId=4299` at HSP | `006AC910` Graphic field | **MATCH** identity |
| Extra-submit “Thing not TNG Graphic” | create does not submit | **LEFTOVER** (dead) |
| `InsertThing` Note `009AD410 mesh=` | `009AD9E0` id attach | **LEFTOVER** wording |
| `PresentWorld` handles only | `009AD410` | **MATCH** |
| `Meshes.Get` at submit | `00A243B0` / `00A26D40` | **MATCH** timing. **DISPROVEN** as create |
| `SubmittedPalskinMeshIds == [4299]` | one PALSKIN Graphic | **MATCH** set |
| `SubmittedHeroPalskin` | `00BD71B0` | **DISPROVEN** as draw |
| `BuildMeshes` + Concat | helper / slots / `c38` | **DIVERGE** |
| `DrawnPasses` only `0x100` | `0x80` + `0x100` | **DIVERGE** |
| `WorldGeometry` auto `new MeshBank` | one world bank | **LEFTOVER** if called without `Meshes` |
| `TracePalskin` father / kid 4300 | Lookout adult | **LEFTOVER** |
| `FirstSeenPalskinStrideBytes=28` | adult prim0 **36** | **LEFTOVER** name |

---

## Classification table

| Claim | Status |
|---|---|
| Frontend / Leave submits Hero PALSKIN | **DISPROVEN** |
| `006AC910` is first Hero **Thing** / Graphic **4299** | **PROVEN** |
| `006AC910` parses type 5 or DIPs PALSKIN | **DISPROVEN** |
| Host extra `Meshes.Get(HeroMeshId)` is that create | **DISPROVEN**. Path **LEFTOVER** / unused |
| First type-5 payload is 4299 after create | **PROVEN**. Caller **PARTIAL** |
| First PALSKIN **submit** is `00BD71B0` family on `[4299]` | **PROVEN** family / set. First prim **PARTIAL** |
| First static `0x20` DIP is Hero | **DISPROVEN** |
| Host flatten / `SubmittedHeroPalskin` is that DIP | **DISPROVEN** |
| Kid 4300 / father / 4126 is this submit | **DISPROVEN** |
| `MeshBank.Get` == `009AD410` | **DISPROVEN** (handle vs `00A243B0`) |

---

## Do not

- Treat `006AC910` as PALSKIN parse or DIP.
- Keep the extra-submit as the reason 4299 draws; PresentWorld
  already has the instance.
- Call `MeshBank.Get` from `InsertThing` / PresentWorld handles.
- Open a second `MeshBank` beside `EngineLifecycle.Meshes`.
- Treat `SubmittedHeroPalskin` as `00BD71B0`.
- Collapse 4299 into static `0x20` / `MeshBatches` soup and call
  that PALSKIN submit.
- Submit Graphic **4126** or kid **4300** as this Present.
- `Meshes.Get(4299|4300)` on frontend frames.

Next recoverable slice is still per-prim PALSKIN records +
`c38` for **4299** (which primitive is first `00BD71B0`), not
another Graphic id and not a MeshBank extra-submit.
