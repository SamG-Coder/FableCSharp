# First Hero face / morph after Leave Frontend

Investigation only. No production `src` edits.

Do **not** start at Oakvale / `EXPRESSION_FLIRT` /
`GiveHeroExpression` / `hero_young_set.bncfg` /
`CREATURE_HERO_CHILD` / `00DBDE40`. Those are later leftover
`Q_NewOakValeIntro` or Guild tween, not Leave / Init Game /
first no-save Present.

Do **not** collapse these into one “face morph”:

| Kind | What it actually is |
|---|---|
| `CHeroMorphDef` / `CTCHeroMorph` | Body-stat persist (Strength / Will / Skill / Morality / Fatness / Teenager). Not a mouth clip. |
| 4299 materials `face` / `mouth` / `eye shadow` | C3D primitive groups on `MESH_HERO`. |
| Hair modifier **4275 / 4276 / 4277** | `CAppearanceModifierDef` blend siblings. Not Graphic **4126**. |
| `CSkeletalMorphDef` / `CTCSkeletalMorph` | Kid **4300** / `_DEAD_CREATURE`. Not Lookout adult. |
| `GiveHeroExpression` / `EXPRESSION_*` | Full-body social emotes. See `proofs/morph-first`. |

Siblings: `proofs/morph-first` (expression play),
`proofs/hero-stats-first` (stats / inventory / `PLAYER_HERO`),
`proofs/bone-config-first` (`.bncfg`),
`proofs/palskin-open` (first 4299 payload),
`docs/status/investigations/E-player-palskin.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: ExeIndex `rtti.txt` / `strings.tsv` / `xrefs.tsv`;
listings `004184BD` / `004EE23F` / `0071D020` / `0071C360` /
`0057E2F1` / `006AC910` / `006A9DD0` / `00662880` / `008388D0`;
`EngineLifecycleTests.Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`;
`WorldShading.FirstSeenPlaysAnim`.

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| Hero face / morph *play* during frontend? | **No.** 2D UI only. | **DISPROVEN** |
| First morph *name* after Leave? | Init Thing Components `004EE23F`: **`CTCHeroMorph`** then **`CHeroMorphDef`**, then `CTCSimpleAppearanceMorph`. | **PROVEN** |
| First `CHeroMorphDef` persist keys? | `0071D020` on `00416005(1)` `game.bin` load: Strength / Will / Skill / `0x0124FD5C` / Morality / Fatness / `0x01265CD0` / Teenager, then tattoo list. | **PROVEN** names. Apply **UNREAD** |
| First *visible* Hero face after Leave? | Lookout `006AC910` `CREATURE_HERO` Graphic **4299**. Prim **17** `face`, **16** `mouth`, **18** `eye shadow`. Bind pose. | **PROVEN** file + spawn. First PALSKIN DIP **PARTIAL** |
| Face *blendshape* / viseme / `C3DSkeletalMorph` after Leave? | **None** on no-save / first pumps. `FirstSeenPlaysAnim=false`. Dialogue managers empty. | **DISPROVEN** |
| Hair morph 4275–4277 / `HeroHair` after Leave? | `START_INITIAL_QUESTS` does not run `HeroHair`. Submit set is **`[4299]` only**. | **DISPROVEN** first-seen. Ids **PROVEN** leftover |
| Face tattoo card after Leave? | `0071C360` looks up `OBJECT_TATTOO_CARD_FACE_CUSTOM_01`. New-game `NumberOfTattoos` path is persist I/O, not a worn card. | **LEFTOVER** vs first Present |
| C# for this? | Name bags only. No CTC, no slider apply, no per-prim face bind. | **DIVERGE** / **LEFTOVER** |

**Answer:** after Leave the first Hero “face” is the **4299
material split** on bind-pose PALSKIN. The first “morph” is
**register + persist**, not a live slider or blendshape.

---

## Timeline (no-save New Game)

```
0042EC7C retail
  2D UI / PlayAVI                         // no CTC*, no 4299
0042F2A2 Leave frontend
  009BE420 + 009BEEB0 Present
0042F491 Init Game → 00418DCA → 004184BD
  "Init Thing Components" 004EE23F        // FIRST morph names
    004EE294  CTCHeroMorph → 004D2EF0 / 004D28BB
    004EE304  CHeroMorphDef → 0044C6B0 + 009B0AC0
    004EE35A  CTCSimpleAppearanceMorph
              CTCRandomAppearanceMorph
    … later CTCCreatureExpression / CSkeletalMorphDef …
  "Init Definition Manager" 00416005(1)
    CHeroMorphDef persist 0071D020        // Strength…Teenager + tattoos
  … Init Graphics / World / Create Players …
  Init World 004A6E30
    MBANK_ALLMESHES directory             // no C3D yet
    tail 006C37D0 .bncfg preload          // bone-config-first; not face
  00416953 FinalAlbion.wld
    0049F180 → 00449D90 PLAYER_HERO miss
               → CREATURE_HERO            // def bind; often no Thing
004189C2 first pumps                      // no region; no face DIP
later 00501450 LookoutPoint
  006C2170 ContainsMap TNG                // no PlayerCreature
  006AC910 CREATURE_HERO Graphic 4299
    006A9DD0 → 00662880 → 008388D0 → 006A5950
    004C9D60("CTCPhysicsControlled") only // not CTCHeroMorph by name
    0042AF3C [thing+112] appearance Note
  first 00A243B0 miss 4299 → 00A26D40     // FIRST hero C3D (includes face)
  dest identity; FirstSeenPlaysAnim=false
  submit palskin ids = [4299]             // no 4275
```

`EXPRESSION_FLIRT` / `HeroHair OBJECT_HERO_HAIR_YOUNG_01` /
`CREATURE_HERO_TRAINING` Teenager / kid 4300 are **not** on
this list. **PROVEN.**

---

## 1. Do not collapse the names

| Kind | Native | Role after Leave |
|---|---|---|
| Body-stat def | `CHeroMorphDef` `0x0137B12C` idx **10535** (raw 4217) | persist sliders on `CREATURE_HERO` |
| Body-stat CTC | `CTCHeroMorph` | factory intern `004EE294`. Create **does not** `004C9D60` it |
| Appearance morph | `CTCSimpleAppearanceMorph` / `CTCRandomAppearanceMorph` | registered next. First-seen apply **UNREAD** |
| Vertex/skel morph | `CSkeletalMorphDef` / `CTCSkeletalMorph` | kid **4300** / dead attach `00835C80` |
| Engine prim | `C3DSkeletalMorph` / `CEngineInternalPrimitiveMorphedAnimatedMesh` | RTTI only. First-seen **UNREAD**. Not on Leave listing |
| Face C3D groups | 4299 prims 16–18 | materials, not XSEQ |
| Hair morph set | modifier **11656** → **4275 / 4276 / 4277** | leftover `HeroHair` |
| Face tattoo object | `OBJECT_TATTOO_CARD_FACE_CUSTOM_01` | `0071C360` lookup. Not worn at New Game |
| Player persist sibling | `0057E2F1` Morality / `0x0124FD5C` / SunTan / Fatness | save/profile fields. Not Init Game apply |

`0057E2F1` proves `0x0124FD5C` is **not** `SunTan`
(`0057E333` is the next push). The immediate sits between
`MoralityChangingEnabled` (`0x0124FD60`) and `SunTan`
(`0x0124FD54`). Four bytes at `0x0124FD5C` is a **3-char**
name the string scanner skipped. Slot order matches **Age**.
**PARTIAL**. `0x01265CD0` is four bytes before `Skill`
(`0x01265CD4`). **UNREAD**.

---

## 2. First after Leave: register, not a face

`004184BD` `0041855B` → `E8 004EE23F` is the first
morph/face-adjacent ASCII after Leave. Order:

1. `CTCHeroMorph` + `CHeroMorphDef` — first morph pair
2. `CTCSimpleAppearanceMorph` / `CTCRandomAppearanceMorph`
3. (much later) `CSkeletalMorphDef` `004F40D1`

`0071D020` (vtbl on `CHeroMorphDef`, no `E8`):

```
+64  "Strength"        00410620
+72  "Will"
+76  "Skill"
+80  0x0124FD5C        // PARTIAL Age (see §1)
+84  "Morality"
+88  "Fatness"
+92  0x01265CD0        // UNREAD
+96  "Teenager"        004045C0  bool
then NumberOfTattoos / "Tattoo" list
```

`0071D102` is the Teenager **push**, not a Guild apply.
`0071C360` is a tattoo-card resolver that **starts** with
`OBJECT_TATTOO_CARD_FACE_CUSTOM_01` then legs/arms/back/chest.
That is persist I/O, not a face mesh attach.

Apply of those floats onto PALSKIN / `.bncfg` / face verts is
**UNREAD** (E-player-palskin leftover). Adult Lookout does
**not** use `CSkeletalMorphDef`. Teenager is
`CREATURE_HERO_TRAINING` leftover.

---

## 3. First visible Hero face: 4299 materials

No-save Hero is `006AC910` after `00501450` Lookout
`GuildArrivalHSP`. `PLAYER_HERO` has no Graphic; fallback
`CREATURE_HERO` → **4299** `MESH_HERO`.

Live parse (`2026-08-18-palskin.md` §4):

| Prim | Material | Stride | Flags | GroupBones | Notes |
|---|---|---|---|---|---|
| 16 | `mouth` | **28** | **20** | 6 | bones 59,62,60,57,58,61 |
| 17 | `face` | 36 | 22 | 16 | 11,15,57 (`Bip01 Head`),13,35,58… |
| 18 | `eye shadow` | **28** | **20** | 1 | bone 57 |

`face`: diff **1250**, bump **1233**, MapFlags=3 → type index
**11** / family **`0xB`**.
`mouth`: MapFlags=1, type index **4**.
`eye shadow`: Flag1=1 MapFlags=1, hair-like type **4**.

These are **the same file** as torso/hands. First payload is
`00A243B0(4299)` → `00A26D40` type 5 (`proofs/palskin-open`).
First dest is identity. **PROVEN** as file + first hero C3D.
Which prim is first `00BD71B0` DIP is **PARTIAL**.

Static C3D `SetTexture` on layer `0x20` happens **before**
PALSKIN `0x100` / `0x80` (`proofs/c3d-material-first`).
So the first *device* material after Leave is a Lookout
prop, not the Hero face. The first *Hero face* bind is the
later PALSKIN drain.

`006AC910` / `006A9DD0` do **not** play DEFAULT
(`005B37F7` clothing GUI only). **PROVEN.**

---

## 4. Create does not name-attach `CTCHeroMorph`

```
006AC910 Create
  004C7380 size 0x208
  0052AB20
  006A9DD0 ConstructFromParams
    00662880 → 008388D0 → 006A5950     // generic creature
    0042B0A2 [thing+112]
    004C9D60("CTCPhysicsControlled")   // only named add
```

`00722360` is a **name setter** (`push "CTCHeroMorph"` /
`0099EBF0` / `ret 4`), same family as `0071B7C0`
`CHeroMorphDef`. Not an apply.

Whether `006A5950` / `0042AF3C` walks sub-def **10535**
onto a live `CTCHeroMorph` is **UNREAD**. Host
`InsertThing` keeps Graphic 4299 and **drops** every
creature sub-def. **PROVEN** host gap.

---

## 5. Hair morph siblings are leftover

`OBJECT_HERO_HAIR_YOUNG_01` Graphic is **4126**
`MESH_HERO_FOLDED_HAT_BANDITCAMP` (static type-1). Submitting
that as hair is a masquerade.

Real attach is `CAppearanceModifierDef` **11656**: count 3,
24-byte records → meshes **4275 / 4276 / 4277** with blend
`lo`/`hi`. 4275 is `MESH_HERO_HAIR_YOUNG_01` type-5 PALSKIN
(15 bones, stride 28 / flags 20, mat `young hair`).

`HeroHair` `00CC9182` is a global verb. Leave /
`004B4260` START_INITIAL_QUESTS / first pumps do **not**
run it. First-scene submit palskin set is **`[4299]` only**.
**DISPROVEN** as first-seen. **PROVEN** as later ids.

Omit attachments until dummy/helper bytes are kept. Do
**not** draw 4126.

---

## 6. Face clip vs expression vs slider

| Claim | Class |
|---|---|
| 4299 has materials `face` / `mouth` / `eye shadow` | **PROVEN** |
| Those materials are a blendshape / viseme after Leave | **DISPROVEN**. Bind pose. Type-6 XSEQ not opened (`xseq-first`) |
| `C3DSkeletalMorph` first-seen on Leave | **UNREAD** name. **DISPROVEN** as Leave listing |
| `CTCLook` is face anim | **DISPROVEN**. Look-at, registered next to expression CTC |
| Speech mouth clip after Leave | **DISPROVEN** (`dialogue-first` empty) |
| `GiveHeroExpression` after Leave | **DISPROVEN** (`morph-first`) |
| Adult Lookout uses `CSkeletalMorphDef` | **DISPROVEN** |
| First Present pose is bind locals | **PROVEN** `FirstSeenPlaysAnim=false` |

Fable “expressions” are full-body emotes. `CHeroMorphDef`
sliders reshape the **body** C3D (and maybe head scale).
They are not a separate face mesh.

---

## 7. C# exists?

| Native | C# | Class |
|---|---|---|
| `CHeroMorphDef` persist / apply | comments only. No type, no `0071D020` | **UNREAD** apply. **LEFTOVER** names |
| `CTCHeroMorph` | none | **LEFTOVER** |
| 4299 face/mouth/eye-shadow prims | `MeshFile` groups parse **MATCH**. Submit flattens soup | **DIVERGE** |
| Hair 4275–4277 | `World.HeroHairs` name bag. `FindMeshId` would hit **4126** | **DIVERGE** if submitted |
| `HeroHair` `00CC9182` | `GlobalDispatcher` → `ApplyHeroHair` | **EQUIVALENT** bag. Not on New Game Leave |
| `GiveHeroExpression` | name bag. No `vtbl+900` | **DIVERGE** (`morph-first` §6) |
| Face tattoo card | none | **LEFTOVER** |
| `EngineLifecycle` after Leave | Graphic 4299 only; no morph Note | **PROVEN** absence |

---

## Classifications (short)

1. **Frontend / Leave face play — DISPROVEN.**
2. **First morph *name* after Leave — `004EE23F` `CTCHeroMorph` / `CHeroMorphDef`. PROVEN.**
3. **First persist — `0071D020` on game.bin. PROVEN.** Apply **UNREAD**. `0x0124FD5C` **PARTIAL** Age. `0x01265CD0` **UNREAD**.
4. **First visible Hero face — 4299 prims `face` / `mouth` / `eye shadow` after `006AC910`. PROVEN** as file + first hero C3D. DIP order **PARTIAL**.
5. **Face blendshape / viseme / hair morph 4275 after Leave — DISPROVEN.** Bind pose. Submit `[4299]` only.
6. **C# — Graphic 4299 soup. No CTC, no slider apply, no face prim bind. DIVERGE / LEFTOVER.**
