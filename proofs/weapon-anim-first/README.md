# First weapon / item animation after Leave Frontend

Investigation only. No production `src` edits.

Do **not** start at Oakvale / `CS_WAKING_UP_LOOP` / `3420` /
`PlayAnimation` `00CC15DA` / `SetHeroWeapon OBJECT_SWORD_*`.
Those are later leftover `Q_NewOakValeIntro` or campaign
scripts, not Leave / Init Game / first no-save Present.

Do **not** treat hero materials / `WEAPON_FOCUS_*` bone
*names* on `MESH_HERO` **4299** as a equipped weapon clip.
That is C3D skeleton split, not `CTCWeapon` / `00CCFDA9`.

XSEQ object construction is a sibling:
`proofs/xseq-first/README.md`. PALSKIN dest / hero bind:
`docs/status/investigations/E-player-palskin.md`.
This note is **weapon attach**, **item C3D**, and
**XSEQ vs PALSKIN**.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: ExeIndex `rtti.txt` / `strings.tsv` / `xrefs.tsv`;
listings `004EE23F` / `004F1C92` / `004D3CAE` / `005DA6A0` /
`005DA6E0` / `0042B1A6` / `00858E00` / `006AC910` /
`006A9DD0` / `0077BA40` / `0077B680` / `00AA4710` /
`00BD2F91` / `00AA0090`;
`proofs/xseq-first/README.md`, `proofs/c3d-first-submit/README.md`,
`proofs/entity-task-queue/README.md`, `proofs/script-global-cmds/README.md`,
`proofs/morph-first/README.md`, `proofs/bone-config-first/README.md`;
`docs/status/investigations/2026-08-18-first-scene-things.md`,
`docs/status/investigations/2026-08-18-palskin.md`;
`WorldShading.FirstSeenPlaysAnim`,
`RegionTravel.FirstSeenPlayAnimationAppliesPose`,
`XSeqFormatTests`, `EngineLifecycleTests`.

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| Weapon / item *clip* during frontend? | **No.** 2D UI only. | **DISPROVEN** |
| First weapon/item *name* after Leave? | Init Thing Components `004EE23F`: `CTCWeapon` then `CWeaponDef` then `CCarryingDef`. | **PROVEN** |
| First XSEQ *payload* on a weapon/item? | **None** on no-save New Game / first pumps. Empty helper `00AA4710` is not a clip. | **DISPROVEN** as this site |
| First PALSKIN dest that includes weapon bones? | Hero **4299** bind locals (`00AA0090` / `00BD2F91`). Bones 73–76 `WEAPON_FOCUS_*` / `WEAPON_SCABBARD_*`. Dest ≈ identity. | **PROVEN** dest. **DISPROVEN** as equipped mesh |
| First Lookout *item* C3D (e.g. `OBJECT_SILVER_KEY` **7934**)? | Static Graphic `0077BA40` / layer `0x20`. Not PALSKIN. Not type-6. | **PROVEN** static |
| `SetHeroWeapon` / `PutInHeroHands` / `HoldInHand` after Leave? | Runner `00CBFB7D` is not on the tree. `World.HeroWeapon` stays empty. | **DISPROVEN** |
| `CTCWeapon` instance on `006AC910`? | Create adds `CTCPhysicsControlled` only. | **DISPROVEN** |
| XSEQ **or** PALSKIN for first-seen pose? | **PALSKIN dest from C3D bind locals.** Not an XSEQ sample. | **PROVEN** |

**Answer:** after Leave there is **no** first weapon/item
*animation clip*. The first pose work that *touches weapon
bones* is **PALSKIN dest** on the unarmed Lookout hero
(bind locals, no type-6). Lookout pickups / props are
**static C3D**, not PALSKIN and not XSEQ.

A later leftover clip (`PlayAnimation` / `3420`) would be
**XSEQ first-key → PALSKIN dest**. That product is **not**
first-seen New Game (`FirstSeenPlaysAnim=false`,
`FirstSeenPlayAnimationAppliesPose=false`).

---

## Timeline (no-save New Game)

```
0042EC7C retail
  2D UI / PlayAVI                       // no CTCWeapon, no 3DAF/XSEQ
0042F2A2 Leave frontend
  009BE420 + 009BEEB0 Present
0042F491 Init Game → 00418DCA → 004184BD
  "Init Thing Components" 004EE23F      // FIRST weapon/item names
    004F1C2C  CTCWeapon intern
              factory 004D3C91 size 40 → 005DA6A0
    004F1C92  CWeaponDef → 0042DAE0 / 009B0AC0
    004F1D48  CCarryingDef
    004D4419  CTCGraphicAppearanceAnimatedMesh intern
  "Init Definition Manager" 00416005(1)
    game.bin CREATURE_HERO sub-def 10526 CWeaponDef
             (SWORD / weapon_pos_a / weapon_pos_b)
  … Init Graphics / Create Players …    // slots; not a weapon Thing
  Init World 004A6E30
    0049E620 Init Mesh Bank             // first empty 3DAF/XSEQ helper
      00AA4710 → 00A999B0 tags 3DAF+XSEQ
    006FAA90 Init Animation Event Managers   // not a clip
  00416953 FinalAlbion.wld
004189C2 pumps
later 00501450(1) LookoutPoint
  006C2170 Loading objects
    Graphic 0077BA40  static props (silver key 7934, walls, …)
    AICreature constructed; no XYZ → not first-scene 0x20
  0051FD80 / 006AC910 hero CREATURE_HERO 4299
    006A9DD0  00662880 + 004C9D60("CTCPhysicsControlled")
    no 004C9D60("CTCWeapon")
    no 005B37F7 DEFAULT / no 00CCFDA9
  first 3D Present
    0x20  static C3D (items/props)
    0x100 PALSKIN dest hero 4299 bind locals (weapon bones idle)
    FirstSeenPlaysAnim=false
    SubmittedPalskinMeshIds = [4299]
```

`SetHeroWeapon` / `3420` / `CS_WAKING_UP_LOOP` are
**not** on this list. **PROVEN**.

---

## 1. Frontend / Leave play a weapon or item clip?

| Claim | Class | Evidence |
|---|---|---|
| Frontend Present is 2D UI | **PROVEN** | FORWARD_TREE §4; `0042DF9E` type `0x22` |
| Leave teardown opens type-6 / `CTCWeapon` | **DISPROVEN** | `0042F2A2` is fade / clear+Present / `FinalAlbion.wld` |
| UI type 6 is XSEQ | **DISPROVEN** | glyphs `0054EF00` (`xseq-first`) |
| `SetHeroWeapon` on Leave | **DISPROVEN** | `00CBFB7D` not on tree (`script-global-cmds`) |

**Answer:** no.

---

## 2. First names — Init Thing Components

`004184BD` → `004EE23F` (after Leave, before Init World):

| Site | What | Class |
|---|---|---|
| `004D3CAE` / `004F1C2C` `CTCWeapon` | intern; factory `004D3C91` alloc **40** → ctor `005DA6A0` vtbl `012554FC` | **PROVEN** name + ctor body |
| `004F1C92` `CWeaponDef` | `0099EBF0` + `0042DAE0` / `009B0AC0` | **PROVEN** register |
| `004F1D48` `CCarryingDef` | immediately after `CWeaponDef` | **PROVEN** register |
| `004D4419` / `0077B680` `CTCGraphicAppearanceAnimatedMesh` | name intern only | **PROVEN** name. **DISPROVEN** as first Lookout Graphic |
| `004D3E33` `CTCWeaponTrail` | intern | **PROVEN** name. First-seen instance **UNREAD** |

`005DA6A0` (ctor): parent `00686800`, zero `+12/+16/+24/+28/+32/+36`,
`+20=0xFFFFFFFF`. **No** mesh, **no** `00A26D40`, **no** PALSKIN
insert.

`005DA6E0` (later init, **UNREAD** first callee): `[thing+140]` →
`006869D0` → `0042AF3C` → **`0042B1A6("CWeaponDef")`**. That is
def lookup + appearance attach, not a clip.

`00858E00` builds socket names (`"weapon_pos_"` + `a`/`b`/`c`/`d`
/ `left_hand` / `right_hand`). Callers sit under `00859A70` /
`0076EB00`. **PROVEN** body. **DISPROVEN** as Leave / first Present.

No file I/O. **PROVEN** as first *name* use. Not animation.

---

## 3. XSEQ — empty helper, not a weapon clip

From `proofs/xseq-first/README.md`:

| Site | When | Class |
|---|---|---|
| `00AA4710` → `00A999B0` | Init Mesh Bank `0049E620` | **PROVEN** first `3DAF`/`XSEQ` *object*. Empty. No BIG read |
| `00A26D40` type-6 payload | later vtbl+48 | **DISPROVEN** as first-seen (`FirstSeenPlaysAnim=false`) |
| `00A4C5E0` / `00A4CDD0` unpack | persist inner | **DISPROVEN** on Leave / Init World |
| Wake `3420` | Oakvale leftover | **DISPROVEN** as this site |

Host `MeshBank.GetAnim` is the later type-6 slot. Nothing on
`EngineLifecycle` New Game calls it.

`C3DAnimationInfo` / `C3DAnimation2` / `C3DAnimationManager`
exist as RTTI (`0x013972B8` / `0x01397830` / `0x01398A8C`).
**Zero** `xrefs.tsv` hits. First-seen **UNREAD**. Do not invent
an in-C3D item clip on Lookout props.

**Answer:** first XSEQ after Leave is the empty mesh-bank
helper. It is **not** a weapon/item animation.

---

## 4. PALSKIN dest — hero 4299, including unused weapon bones

Live `MESH_HERO` (graphics.big **4299**, type 5, **anim=1**):

| Field | Value |
|---|---|
| Bones | **77** |
| Weapon bones | `WEAPON_FOCUS_02` **73**, `WEAPON_FOCUS_01` **74**, `WEAPON_SCABBARD_01` **75**, `WEAPON_SCABBARD_02` **76** |
| First-seen dest | hierarchy(48-byte locals) × IBM (`00A9E1E0` + `00BD2F91`) ≈ identity |
| Clip | none (`FirstSeenPlaysAnim=false`) |
| Layer | `0x100` (slots 8+10), not static `0x20` |

`00AA0090` from packer `00BD2E35` uses mixer `bank+960`
(`00AA0F60`). Input is **C3D bind locals**, not a type-6 sample.

`SubmittedPalskinMeshIds` on first Present is **`[4299]`** only.
No sword / key / clothing PALSKIN id.

`CWeaponDef` idx **10526** on `CREATURE_HERO` stores ASCII
`SWORD`, `weapon_pos_a`, `weapon_pos_b`. Mesh/socket apply
**UNREAD**. Helpers/dummies on 4299 are decompressed then
**dropped** by `MeshFile.Parse` — sockets cannot be recovered
from the host mesh.

**Answer:** first pose that *includes* weapon bone slots is
**PALSKIN dest**. No weapon mesh is parented. Dest is bind
pose, not XSEQ.

---

## 5. Lookout items / props — static C3D, not PALSKIN

`006C2170` Graphic apply is **`0077BA40`** (single-mesh +
`004C0050` / `009AD410`). That is **not**
`CTCGraphicAppearanceAnimatedMesh` (`0077B680` is intern).

First-scene submitted props are layer **`0x20`** static-lit
(`00BB2540`). Census includes `OBJECT_SILVER_KEY` mesh
**7934** (1), walls, rocks, lamps, fencegates. Parse fail = 0.
PALSKIN id list does **not** contain 7934.

Lookout `AICreature` (beggar / bully / villagers / traders)
exist after `0051FD80` but TNG has **no** `CTCPhysicsStandard`
XYZ → host `ResolveSubmit` skips them. Native first-seen
draw of those PALSKIN bodies is **UNREAD**. Do not invent
their idle XSEQ as first Present.

ContainsMap doors (Bridge `5080`, Guild `6078`) exist off-primary
and are **not** first-scene `0x20`.

**Answer:** first *item* C3D after Leave is static Graphic,
not an animated mesh and not XSEQ.

---

## 6. Hero create does not arm or play

`006AC910` → `006A9DD0`:

```
00662880            parent construct
0042B0A2            [thing+112]
004C9D60("CTCPhysicsControlled")
004C9CA0            activate
```

No `004C9D60("CTCWeapon")`. No `005B37F7` DEFAULT. No
`00CCFDA9` `SetHeroWeapon`. No `005DA6E0`.

`FirstSeenAppearancePlaysDefault=false`.
`FirstSeenCallsPlayAnimationDispatcher=false`.

Script verbs that *would* attach later:

| Verb | Native | First-seen after Leave |
|---|---|---|
| `SetHeroWeapon` | `00CCFDA9` vtbl+488 | **DISPROVEN** (`00CBFB7D` off tree) |
| `PutInHeroHands` | vtbl+572/568 | **DISPROVEN** |
| `RemoveHeroWeapons` | vtbl+552/560 | **DISPROVEN** |
| `HoldInHand` | entity | **DISPROVEN** |
| `PlayAnimation` | vtbl+72 `004C7470` | **DISPROVEN** |
| `PlayLoopingAnim` | vtbl+80 | **DISPROVEN** |

Host `World.HeroWeapon` stays `""`. Binding that name to a
C3D is **UNREAD** even when the verb later runs.

---

## 7. C# vs native

| Site | What | Class |
|---|---|---|
| `MeshBank.GetAnim` / `XSeqFile.Parse` | on-demand type-6 | **EQUIVALENT** later slot. **Not** first-seen |
| `PaletteForPose` / `FirstSeenPalettes` | dest = bind locals when clip null | **MATCH** first Present |
| `World.HeroWeapon` / `SetHeroWeapon` | name store only | **LEFTOVER** vs Leave |
| `SubmitCurrentWorld` PALSKIN ids | `[4299]` | **MATCH** set. Submit path is CPU flatten leftover vs `00BD3070` |
| `0077BA40` static items | host Graphic resolve | **MATCH** ids (silver key 7934, …) |
| `CTCWeapon` / `00858E00` sockets | unused | **LEFTOVER** |

---

## Classifications (short)

1. **Frontend / Leave weapon or item clip — DISPROVEN.**
2. **First names after Leave — `004EE23F` `CTCWeapon` /
   `CWeaponDef` / `CCarryingDef`. PROVEN.** Register only.
3. **First XSEQ — empty `00AA4710` helper. DISPROVEN as a
   weapon/item clip.** `3420` is leftover.
4. **First pose involving weapon bones — PALSKIN dest on
   hero 4299 bind locals. PROVEN.** No attached weapon mesh.
5. **First Lookout item C3D — static `0077BA40` / `0x20`.
   PROVEN. DISPROVEN as PALSKIN or XSEQ.**
6. **XSEQ or PALSKIN? PALSKIN dest (no clip). PROVEN.**

Do not treat `SetHeroWeapon`, `GetAnim(3420)`, or
`PlayAnimation` apply as the first weapon/item animation
after Leave.
