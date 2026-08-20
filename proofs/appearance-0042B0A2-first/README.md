# First `CAppearanceDef` `0042B0A2` after Leave (Hero 4299 create)

Investigation only. Production `src/` was not edited.

Do **not** start at Oakvale / `00DBDE40` / `CREATURE_HERO_CHILD` /
Graphic **4300** / `CSkeletalMorphDef`. That is later leftover
`Q_NewOakValeIntro`, not Leave / Init Game / first no-save
Lookout Hero.

Do **not** collapse this site into PALSKIN open, C3D bones, or
`005B37F7` `DEFAULT` play. Those are different VAs.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Question: on Hero **4299** create (`006A9DD0` / parent
`004CA010`), what does first `0042B0A2` attach? Morph /
palskin / bone at this site? `DEFAULT` `005B37F7` is not on
create — when?

Authority: dumps `0042B0A2` (`text-map/listing-00400000.txt`),
`004CA010` (`listing-004c0000.txt`), `006A9DD0`
(`newgame-trace/constructfromparams-006a9dd0-006a9dd0.md`,
`listing-00680000.txt`); `005B37F7` /
`calls-appearance-default-play-005b37f7`;
siblings `proofs/hero-4299-create`, `hero-appearance-first`,
`morph-first`; also `hero-idle-anim`, `palskin-after-leave`,
`bone-after-leave`; `WorldShading.FirstSeenAppearancePlaysDefault`.

---

## Verdict

**At this site `0042B0A2` is a named `CAppearanceDef` HANDLE
lookup from `[thing+112]`. On `006A9DD0` the dest is a stack
slot that is released before return. It does not persist on
the Thing. It does not attach morph, PALSKIN, or bone. It
does not play `DEFAULT`.**

Graphic **4299** is already bound by parent `004CA010` →
`0042AF3C` / `009AD9E0` into `[thing+140]` / `[thing+112]`.
That is the compiled def + Graphic id, not `CAppearanceDef`.

| Layer | What this site does | Class |
|---|---|---|
| Frontend / Leave | no `0042B0A2` | **DISPROVEN** |
| `004CA010` parent bind | `[thing+140]` def id; `0042AF3C(&thing+112)` | **PROVEN**. **DISPROVEN** as `CAppearanceDef` |
| `006A9E9F` `0042B0A2` | `vtbl+56("CAppearanceDef")` → `009ADA10` → stack HANDLE | **PROVEN** |
| Persist appearance on Thing | dest released at `006A9ECD` | **DISPROVEN** |
| Morph / `CHeroMorphDef` / `006AC430` | other type / later play | **DISPROVEN** at this site |
| PALSKIN type 5 | later `00A243B0(4299)` | **DISPROVEN** at this site |
| C3D bones | later `00A89450` / `00A894ED` | **DISPROVEN** at this site |
| `005B37F7` `DEFAULT` | clothing wrapper `005B4E7C` / `PC_UI_FRAME` `005B8743` only | **PROVEN** when. **DISPROVEN** on create |
| Kid 4300 | Oakvale leftover | **DISPROVEN** |

`CAppearanceDef` idx **10533** on `CREATURE_HERO` is type
**PROVEN**. +52 clip table (`00662A00`) is **UNREAD** as a
first-seen walk: create never calls `00662A00` / `005DC340`.

---

## Timeline (no-save New Game)

```
0042F2A2  Leave frontend
  009BE420 + 009BEEB0 Present            // no 0042B0A2
0042F491  Init Game
  004EE23F Init Thing Components         // CTCHeroMorph names (morph-first)
  00416005(1) game.bin                   // Graphic 4299 + sub-def 10533 live here
  0049F180 Init Characters
    00449D90 PLAYER_HERO miss → CREATURE_HERO
    00489D40 holy-site miss → no 006AC910
004189C2  dummy pumps
later 00501450 LookoutPoint
  006C2170 Loading objects
    0077BA40 static Graphic apply        // props; not Hero
  GuildArrivalHSP (52.688, 69.597, 36.982)
    006AC910 CThingPlayerCreature::Create
      004C7380 size 0x208
      0052AB20
      006A9DD0 ConstructFromParams
        00662880 → 008388D0 (arg0>0) → 006A5950
          004CA010
            [thing+140] = def id
            0042AF3C(manager+32, id, &thing+112) → 009AD9E0
            CTCEditor? + CTCMapwho
        006A9E9F  0042B0A2([esi+112], &stack)   // THIS SITE
          push "CAppearanceDef"
          [vtbl+56] → 009ADA10
          store HANDLE in stack dest
        004C9D60("CTCPhysicsControlled")
        006A9ECD  release stack HANDLE           // not kept
      004C9CA0 activate
    Graphic 4299 MESH_HERO
then 00A243B0(4299) miss → 00A26D40 type 5   // PALSKIN
  00A894ED 77 bones                          // bone
no 005B37F7 / 0070C050 / 0070D580
```

`CREATURE_HERO_CHILD` / 4300 / `00DBDE40` are **not** on this
list. **PROVEN.**

TNG props can call `0042B0A2` at `0077BEE8` during
`0077BA40` **before** Hero create. That is a **prop**
appearance walk, not this Hero site. Scope of this note is
`006A9DD0` / `004CA010`.

---

## 1. Dump — `0042B0A2` is typed HANDLE get

`listing-00400000.txt` (`0042B0A2`–`0042B141`, `ret 4`):

```
0042B0A2  push ebp
0042B0AC  push "CAppearanceDef"
0042B0B4  call 0099EBF0
0042B0B9  mov eax, [esi]          ; ecx = compiled def
0042B0C1  call [eax+56]           ; vtbl+56(name)
0042B0C9  call 0099EAE0
0042B0CE  test edi, edi
0042B0D0  je 0042B13C             ; al=0
0042B0D2  push [edi]              ; sub-def id
0042B0D4  mov ecx, [esi+28]
0042B0DB  call 009ADA10           ; id → object
          store into [ebp+8] dest ; refcount swap
0042B138  mov al, 1
0042B141  ret 4
```

Same shape as sibling `0042AF9E` (`"CObjectAugmentationsDef"`).
**0** mesh id. **0** `00A26D40`. **0** `00A89450`. **0**
`"CHeroMorphDef"` / `"CSkeletalMorphDef"`. **0** `"DEFAULT"` /
`0070C050` / `0070D580`.

**Answer:** get `CAppearanceDef` object into caller dest.
Nothing else.

---

## 2. Dump — Hero create dest is a **stack** HANDLE

`constructfromparams-006a9dd0-006a9dd0.md` / `listing-00680000.txt`:

```
006A9E37  call 00662880                 ; parent (004CA010)
006A9E47  test bl, bl
006A9E49  je 006A9EE7                   ; fail → no appearance
006A9E7A  mov eax, [esi+224]
006A9E80  fld [eax+80]
006A9E83  lea ecx, [esp+24]
006A9E87  push ecx                      ; dest = &local
006A9E88  fst [esi+180]                 ; scale copy, not appearance
006A9E8E  mov ecx, [esi+112]            ; compiled def
006A9E91  fstp [esi+176]
006A9E97  mov [esp+28], 0
006A9E9F  call 0042B0A2                 ; THIS SITE
006A9EA6  push "CTCPhysicsControlled"
006A9EBF  call 004C9D60                 ; physics only
006A9ECD  mov ecx, [esp+24]
006A9ED1  test ecx, ecx
006A9ED3  je 006A9EDF
006A9ED5  dec [ecx+4]                   ; RELEASE dest
006A9EE0  mov al, 1
006A9EE4  ret 16
```

`006AC910` (`cthingplayercreature-create-006ac910.md`): alloc
`0x208` → `0052AB20` → `006A9DD0` → pose pack `006A06E0` →
`004C9CA0`. **0** `E8` to `0042B0A2` / `005B37F7` /
`00662A00` / `006AC430`.

The float stores at `+176` / `+180` are scale from
`[thing+224]+80` (parent already wrote the same pair in
`006A5950`). **DISPROVEN** as morph / bone.

**Answer:** first Hero `0042B0A2` after Leave on this path
**proves the type exists** on `[thing+112]` (idx **10533**).
It does **not** keep the object. Host `InsertThing` skipping
the walk is **MATCH** first-seen (discarded anyway) and
**PARTIAL** vs a later `00662A00` user.

---

## 3. Dump — `004CA010` binds Graphic / compiled def, not appearance

`listing-004c0000.txt` (`004CA010`–`004CA1C2`, `ret 16`).
Reached as `006A9DD0` → `00662880` (`ret 28`) → `008388D0`
(`arg0>0`) → `006A5950` → `004CA010`.

```
004CA03E  mov [esi+140], ecx            ; def id
004CA045  mov [esi+144], dl
004CA04B  mov [esi+142], eax
004CA052  call 0049D870
004CA060  mov [esi+104], eax
004CA099  mov eax, [esi+140]
004CA0AC  jbe 004CA0C4
004CA0B4  mov ecx, [ecx+32]             ; [0x13B8A1C]+32
004CA0B7  lea edx, [esi+112]
004CA0BF  call 0042AF3C                 ; id → [thing+112]
004CA0C6  call 004C7FF0
004CA0D6  call 0051E580
004CA0DB  mov [esi+108], eax
          optional clamp +144
004CA103  call [edx+60]                 ; if [thing+112]
          copy [def+104]/[+108] names via 009D49B0
          into [thing+116]/[+120]
004CA172  push "CTCEditor"              ; editor only
004CA199  push "CTCMapwho"
004CA1B0  call 004C9D60
```

`0042AF3C` (`listing-00400000.txt`): `arg<=0` fail; else
`009AD9E0` + refcount store at dest. Generic **id→object**.
Not `"CAppearanceDef"`. Not a mesh load.

Live `game.bin`: `CREATURE_HERO` Graphic **4299**
`MESH_HERO`. `PLAYER_HERO` has no Graphic. Kid **4300** is
`CREATURE_HERO_CHILD`. **PROVEN**
(`GameBinFormatTests.FindMeshId`).

**Answer:** `004CA010` attaches the **compiled creature def**
(and therefore Graphic **4299**). PALSKIN payload is still
later.

---

## 4. Not morph / palskin / bone at this site

| Candidate | Why not `006A9E9F` |
|---|---|
| `CHeroMorphDef` 10535 | different sub-def. Persist `0071D020` on game.bin load. Apply **UNREAD**. Create does not `vtbl+56("CHeroMorphDef")` |
| `CTCHeroMorph` | registered `004EE23F` (`morph-first`). Not added here |
| `CSkeletalMorphDef` / `CTCSkeletalMorph` | **kid 4300** name intern. Adult 4299 **DISPROVEN** |
| Expression play `006AC430` | **0** `E8` on `006AC910` / `006A9DD0` |
| Type-5 PALSKIN `00A26D40` | first miss `00A243B0(4299)` **after** create (`palskin-after-leave`) |
| 77-bone C3D `00A894ED` | inside that type-5 parse (`bone-after-leave`) |
| `.bncfg` `006C37D0` | Init World preload; XYZ scales, not this HANDLE |
| Clothing Graphic **4126** | static trap. Create does not apply modifiers (`hero-appearance-first`) |

`face` / `mouth` on 4299 are C3D materials, not a morph clip
(`morph-first`). **DISPROVEN** as this attach.

---

## 5. `005B37F7` DEFAULT — not create; when?

`calls-appearance-default-play-005b37f7-005b37f7.md`: **2**
`E8` hits — `005B4E7F`, `005B8758`. None in `006AC910` /
`006A9DD0` / `00662880` / `004CA010` / Leave / first pumps.

`005B4E7C` (listing): `call 005B37F7` then `[esi+348]=1` →
`005B4795` / `005BD4EA` / `vtbl+88`. No `E8` of `005B4E7C`
in text-map (vtbl / clothing wrapper). Sibling clothing
string helper is `005B6881`
(`TEXT_GUI_MENU_CLOTHING_TOTAL_ARMOUR`) — **not** the `E8`.
`WorldShading.AppearanceDefaultClothingCaller=005B6881` is
that nearby identity.

`005B8743` `PC_UI_FRAME`: `005B8758 call 005B37F7` then
`[esi+348]=1`. Caller `0041FBDD` after `"CLOTHING_LIST"`
when `[player+702]!=0` (`listing-00400000.txt`).

Body (`appearance-default-play-005b37f7` +
`listing-00580000.txt` tail):

```
005B3801  [esi+104]+212 == 0 → clear [esi+344]; ret
          else 00835A20 → [esi+344]
005B3992  004C9D60("CTCAnimationComplex") on that object
005B39FB  edi = [obj+112]
005B3A0F  call 0042B0A2                  ; SAME getter
005B3A26  push "DEFAULT"
005B3A40  lea ecx, [ebx+52]              ; appearance+52
005B3A4A  call 005DC340
005B3A7D  push 6
005B3A82  call 0070C050
005B3A92  call 0070B460
005B3A99  call 0070D580                  ; inner play
```

`0070B4D0` (`CTCAnimationComplex` vtbl+16) is a **second**
`DEFAULT` play (`0042B0A2` → `+52` → `0070C050(6)` →
`0070D580`). **PROVEN** body. **DISPROVEN** as create /
first Present (`hero-idle-anim`).

`FirstSeenAppearancePlaysDefault=false`.
`FirstSeenPlaysAnim=false`. First Lookout frame is bind
locals. **PROVEN.**

**When `005B37F7` runs:** clothing GUI / `PC_UI_FRAME`
open (`CLOTHING_LIST`), not Hero create, not Leave, not
first pumps.

---

## Host vs native

| Host | Native first-seen | Class |
|---|---|---|
| `InsertThing` Notes `004CA010` / `0042AF3C [thing+112]` | listing **PROVEN** | **MATCH** Graphic bind |
| no `0042B0A2` on insert | stack dest released | **MATCH** persist. **PARTIAL** vs later `00662A00` |
| `HeroMeshId=4299` | Graphic field | **MATCH** |
| no `PlayAppearanceDefault` from `SpawnHero` | no `005B37F7` | **MATCH** |
| `AnimationRuntime.Clips` empty | +52 never walked | **MATCH** first-seen |
| `FirstSceneWorld` kid 4300 | Lookout adult | **LEFTOVER** |

---

## Classification table

| Claim | Status |
|---|---|
| First Hero `0042B0A2` after Leave is `006A9E9F` on `006A9DD0` | **PROVEN** (this path). Earlier prop `0077BEE8` **PARTIAL** as global first |
| That call attaches `CAppearanceDef` (idx 10533) as a live Thing field | **DISPROVEN** (stack + release) |
| That call attaches morph | **DISPROVEN** |
| That call opens PALSKIN / bones | **DISPROVEN** |
| `004CA010` is the Graphic / compiled-def bind | **PROVEN** |
| Create plays `DEFAULT` via `005B37F7` | **DISPROVEN** |
| `005B37F7` is clothing / `PC_UI_FRAME` | **PROVEN** (`E8` `005B4E7F` / `005B8758`) |
| First Present pose is bind locals | **PROVEN** |
| Kid 4300 / `CSkeletalMorphDef` is this site | **DISPROVEN** |

---

## Do not

- Call `005B37F7` / `PlayAppearanceDefault` from `006AC910`
  / `SpawnHero`.
- Treat `0042B0A2` as PALSKIN open or bone pack.
- Treat `0042AF3C` `[thing+112]` as `CAppearanceDef`.
- Invent a kept appearance pointer on the first-seen Hero
  Thing.
- Use `CREATURE_HERO_CHILD` / 4300 / Oakvale morph as this
  attach.
- Collapse +52 clip names / combat strings at raw +3697
  into a first-seen `DEFAULT` / `STAND` play.

Next recoverable slice is still `CAppearanceDef` +52
(`00662A00`) **when a later user first walks it** — not a
second Graphic, and not this create site.
