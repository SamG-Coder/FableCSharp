# First morph / expression / face anim after Leave Frontend

Investigation only. No production `src` edits.

Do **not** start at Oakvale / `EXPRESSION_FLIRT` /
`GiveHeroExpression` / `hero_young_set.bncfg` / `00DBDE40`.
Those are later leftover `Q_NewOakValeIntro` or Guild tween,
not Leave / Init Game / first no-save Present.

Do **not** treat mesh materials named `face` / `mouth` on
`MESH_HERO` **4299** as a facial clip. That is C3D material
split, not `CTCExpression` / `006AC430`.

`.bncfg` bone-scale I/O is a sibling: see
`proofs/bone-config-first/README.md`. This note is
**expression play**, **CHeroMorph persist**, and **face**.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: ExeIndex `rtti.txt` / `strings.tsv` / `xrefs.tsv`;
listings `004184BD` / `004EE23F` / `00416005` / `0071D020` /
`006AC430` / `0057BE71` / `00CC6132` / `00905420` / `00846710` /
`00422B99`;
`proofs/bone-config-first/README.md`, `proofs/xseq-first/README.md`,
`proofs/entity-task-queue/README.md`, `proofs/dialogue-first/README.md`;
`docs/status/investigations/E-player-palskin.md`;
`ScriptCommandMap` / `ExecutionContext.GiveHeroExpression`;
`WorldShading.FirstSeenPlaysAnim`.

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| Expression / face *clip* during frontend? | **No.** 2D UI only. | **DISPROVEN** |
| First morph/expression *name* after Leave? | Init Thing Components `004EE23F`: `CTCHeroMorph` / `CHeroMorphDef` first, then appearance morphs, `CTCCreatureExpression` / `CTCLook`, `CSkeletalMorphDef`, `CExpressionSubDef`. | **PROVEN** |
| First `CHeroMorphDef` persist keys? | `0071D020`: Strength / Will / Skill / Morality / Fatness / Teenager. Invoked from game.bin load (`00416005(1)`), not a live apply. | **PROVEN** names. Apply **UNREAD** |
| First *play* of an expression after Leave? | **None** on no-save New Game / first pumps. `006AC430` is not on `006AC910`. `00CBFB7D` / `GiveHeroExpression` / `CActionPerformExpression` are not on the Leave tree. | **PROVEN** skip |
| First leftover *named* play helper? | `0057BE71` table: first string `EXPRESSION_SNEER` if `[this+112]>=1`. Callers `0057C43C` / `0057CB22` / `006D8516` — opinion / unlock, not Init Game. | **PROVEN** body. First-seen **DISPROVEN** |
| Face anim (blendshape / mouth clip)? | Adult 4299 has materials `face` / `mouth`. `CSkeletalMorphDef` is kid **4300** only. `FirstSeenPlaysAnim=false`. | **DISPROVEN** as first Present |
| C# runtime for this? | **Partial leftover.** Name store only. | see §6 |

**Answer:** first-seen after Leave is **register + persist**,
not a played expression or face clip. First *Thing* expression
apply (`006AC430`) is later / unread on this spine.

---

## Timeline (no-save New Game)

```
0042EC7C retail
  2D UI / PlayAVI                  // no CTC*, no 006AC430
0042F2A2 Leave frontend
  009BE420 + 009BEEB0 Present
0042F491 Init Game → 00418DCA → 004184BD
  00418536  if [0x13B879C]==0
              alloc 0xE0 → 0044C6C2 / 0044C71F
  "Init Thing Components" 004EE23F     // FIRST morph/expression names
    004EE294  CTCHeroMorph  → 004D2EF0 / 004D28BB
    004EE304  CHeroMorphDef → 0044C6B0 + 009B0AC0
    004EE35A  CTCSimpleAppearanceMorph
              CTCRandomAppearanceMorph
    004EF260  CTCCreatureExpression → 004D2AA4
    004EF2DC  CTCLook
    004F40D1  CSkeletalMorphDef → 009B0AC0
    004D481C  CTCSkeletalMorph  (name intern)
    004F49EE  CExpressionSubDef → 009B0AC0
    004D4B75  CTCExpression     (name intern)
  "Init Definition Manager" 00416005(1)
    0044C6B0 getter; [edx+8]; 009ACB10
    CHeroMorphDef persist 0071D020     // Strength…Teenager
    CExpressionSubDef / ExpressionDef fields
  … Init Graphics / World / Create Players …
  Init World 004A6E30
    Init Animation Event Managers 006FAA90   // not expression
    tail 006C37D0 .bncfg preload             // bone-config-first
  00416953 FinalAlbion.wld
004189C2 first pumps
  type-1 00CB8220 yield
  FirstSeenPlaysAnim=false
  no 00CBFB7D / 00CC6132 / 006AC430 / 0057BE71
later 0051FD80 / 006AC910 CREATURE_HERO
  no 005B37F7 DEFAULT; no 006AC430
```

`EXPRESSION_FLIRT` / `GiveHeroExpression` / Oakvale father Speak
are **not** on this list. **PROVEN.**

---

## 1. What the dump names are (do not collapse)

RTTI / strings (`out/00-index`):

| Kind | Native name | Role |
|---|---|---|
| Body-stat morph def | `CHeroMorphDef` `0x0137B12C` | persist Strength/Will/Skill/Morality/Fatness + Teenager |
| Body-stat component | `CTCHeroMorph` | Thing component |
| Vertex/skel morph def | `CSkeletalMorphDef` | **kid 4300**, not adult 4299 |
| Vertex/skel component | `CTCSkeletalMorph` | apply `00786700`; `_DEAD_CREATURE` attach |
| Appearance morph | `CTCSimpleAppearanceMorph` / `CTCRandomAppearanceMorph` | persist; `ExpressionDef` field at `007EF0F9` |
| Social expression def | `CExpressionDef` RTTI only (no `push "CExpressionDef"`) | `CExpressionSubDef` is the interned name |
| Social expression CTC | `CTCCreatureExpression` / `CTCExpression` | component; attach `00846710` → `004C9D60` |
| Scripted action | `CActionPerformExpression` `00905420` | **name setter**, same family as `CActionPlayAnimation` |
| Creature AI action | `CCreatureAction_PerformExpression` / `Extended` / `LearnExpression` | later AI. Bodies **UNREAD** |
| Look (not face clip) | `CTCLook` / `CLookDef` | registered after `CTCCreatureExpression` |
| UI list | `EXPRESSIONS_LIST` `00422B99` | Player Interface inventory, not Init Game |
| Mesh materials | `face` / `mouth` on 4299 | C3D groups, not XSEQ |

`0044C72B` is a long `009B0AC0` bank registrar that also pushes
`"EXPRESSION"` next to `CAMERA_MANAGER_SET` / `OPINION_DEED_EFFECTS`.
**0 E8** to `0044C72B`. Init Game uses `0044C6C2`/`0044C71F` for the
`0xE0` singleton at `[0x13B879C]`, then `004EE23F` registers types.
Whether `0044C72B` is a vtbl on that object is **UNREAD**.

---

## 2. First after Leave: register, not play

`004184BD` at `0041855B` logs `"Init Thing Components"` then
`E8 004EE23F`. That is the first morph/expression ASCII after Leave.

Order inside `004EE23F` (listing-004c0000):

1. `CTCHeroMorph` + `CHeroMorphDef` — **first** morph pair
2. `CTCSimpleAppearanceMorph` / `CTCRandomAppearanceMorph`
3. `CTCCreatureExpression` + `CTCLook`
4. `CSkeletalMorphDef` (`004F40D1`)
5. `CExpressionSubDef` (`004F49EE`)

`00416005(1)` immediately after (`004185D5` push 1). Persist
`0071D020` (no E8; vtbl on `CHeroMorphDef`):

```
push "Strength"   00410620   this+64
push "Will"                    +72
push "Skill"                   +76
push 0x124FD5C                 +80   // name UNREAD here
push "Morality"                +84
push "Fatness"                 +88
push 0x1265CD0                 +92   // name UNREAD here
push "Teenager"   004045C0     +96   // bool persist
```

`0071D102` xref is the `Teenager` push, not a separate apply.
Apply of those floats onto PALSKIN / bncfg is **UNREAD**
(E-player-palskin leftover). Adult Lookout does **not** use
`CSkeletalMorphDef`.

---

## 3. Play path (exists, not first-seen)

### 3a. `006AC430` — named `EXPRESSION` on a Thing

```
006AC390  already-has? → ret 1
else
  004C7990 / 009AD410  count
  push "EXPRESSION"
  004C7990 / 009ACCE0 / 005B3440
  005F81BE
  00703210  (generic def-by-name; many callers)
  optional 005D8D50 → [obj] vtbl+320
```

E8 callers of `006AC430`:

| Site | Notes |
|---|---|
| `0057BE71` ×19 | unlock table, first name **`EXPRESSION_SNEER`** |
| `00847A3B` | persist slot `0x8F` → `004C9D60` family |

`006AC910` Create has **no** `006AC430`. **PROVEN.**

`0057BE71` `[esi+112]` is a level/count:

| `+112` | Always-on name | Extra if float `0057AAD9` > 0 |
|---|---|---|
| ≥1 | `EXPRESSION_SNEER` | `EXPRESSION_TAP` |
| ≥2 | `EXPRESSION_VICTORY_PUMP` | `EXPRESSION_KISS_MY_ASS` |
| ≥3 | `EXPRESSION_EVIL_LAUGH` | `EXPRESSION_FLAMENCO` |
| … | Heroic / rude / dance / steal / picklock | |

This is **learn / grant / opinion**, not Leave.

### 3b. Script `GiveHeroExpression` `00CC6132`

Token `0x012C2158`. Apply `00CC6185`:

- empty name → `00CC7081`
- `007ADB30` → `007ACC90` lookup; miss (`eax == sentinel`) skip
- parse flag `00CBEDBA`, param `0099E7F0`
- `[0x143E8F8].vtbl+900(name, esi, flag)`
- `jmp 00CC2C6B`

Not on Leave / `004184BD` / type-1 `00CB8220`. Same as other
global verbs (`proofs/script-global-cmds`). **DISPROVEN** first-seen.

### 3c. `CActionPerformExpression` `00905420`

`push "CActionPerformExpression"` / `0099EBF0` / `ret 4`.
Name setter, **not** enqueue. Same pattern as `00903570`
`CActionPlayAnimation` (`proofs/entity-task-queue`).

### 3d. UI `EXPRESSIONS_LIST` `00422B99`

Player Interface list (`vtbl+12` / `+332` / `+280`), sibling of
`MAGIC_LIST` / `QUESTS_LIST`. Not Init Game. First-seen **UNREAD**
(opens with the expression menu).

---

## 4. Face anim vs expression vs morph

| Claim | Class |
|---|---|
| 4299 materials include `face` and `mouth` | **PROVEN** (E-player-palskin §6) |
| Those materials are a face XSEQ / blendshape play after Leave | **DISPROVEN**. First C3D is type 1/2/4/5; type-6 not opened (`xseq-first`) |
| `C3DSkeletalMorph` / `CEngineInternalPrimitiveMorphedAnimatedMesh` RTTI | **PROVEN** names. First-seen **UNREAD**. Not on Leave listing |
| `CTCLook` is face animation | **DISPROVEN** pairing. It is look-at, registered next to expression CTC |
| `CActionTurnToFacePosition` is face anim | **DISPROVEN**. Turn-to-point |
| Speech viseme / mouth clip after Leave | **DISPROVEN**. Dialogue managers construct empty (`dialogue-first`) |
| First Present pose is bind locals | **PROVEN** `FirstSeenPlaysAnim=false` |

Fable “expressions” in script (`EXPRESSION_FLIRT`, `EXPRESSION_SNEER`,
…) are **full-body social emotes**, not facial blendshapes.

---

## 5. Later / leftover (do not promote)

| Path | Class |
|---|---|
| `GiveHeroExpression EXPRESSION_FLIRT` in script bank | **LEFTOVER** vs Leave |
| `Expression_*` quests in global QST | **PROVEN** names exist (`WorldSceneTests`). Not activated at `004B4260` |
| `Q_NewOakValeIntro` / child mesh 4300 / `CSkeletalMorphDef` | **LEFTOVER** |
| Guild `CREATURE_HERO_TRAINING` + Teenager + `hero_teen_set.bncfg` | **LEFTOVER** vs first Lookout adult |
| `005B37F7` DEFAULT play | clothing GUI only; Create skip **PROVEN** |

---

## 6. C# exists?

| Native | C# | Class |
|---|---|---|
| `CHeroMorphDef` persist / apply | comments in E-player-palskin / PARITY only. No type, no `0071D020` | **UNREAD** apply. **LEFTOVER** names |
| `CTCHeroMorph` / `CTCCreatureExpression` / `CTCExpression` | none | **LEFTOVER** |
| `006AC430` / `0057BE71` / `005D8D50` | none | **LEFTOVER** |
| `GiveHeroExpression` `00CC6185` | `GlobalDispatcher` + `World.GiveHeroExpression` stores `{Name,Flag,Param}` | **EQUIVALENT** as bag. **DIVERGE**: no `007ADB30` miss-skip, no `vtbl+900` |
| `CActionPerformExpression` | not in `EntityTaskQueue` | **LEFTOVER** |
| `.bncfg` | `BoneConfig.cs` parser; unused by lifecycle | **LEFTOVER** (`bone-config-first` §4) |
| Face/mouth materials | `MeshFile` triangle groups | **MATCH** parse. No face clip bind |
| `EngineLifecycle` after Leave | no expression/morph Note | **PROVEN** absence |

`ScriptRuntimeArchitectureTests.GiveHeroExpression_*` pins the
store and the recover comment (`007ADB30` UNREAD → Runtime
**PARTIAL**). That test does not run on New Game Leave.

---

## Classifications (short)

1. **Frontend / Leave expression play — DISPROVEN.**
2. **First morph/expression *name* after Leave — `004EE23F` `CTCHeroMorph` / `CHeroMorphDef`. PROVEN.**
3. **First persist keys — `0071D020` on game.bin load. PROVEN.** Apply **UNREAD**.
4. **First expression *play* — not on Leave / first pump / `006AC910`. PROVEN skip.** Leftover helper `006AC430`; first table name `EXPRESSION_SNEER`.
5. **Face clip after Leave — DISPROVEN.** Bind pose. `face`/`mouth` are materials.
6. **C# — `GiveHeroExpression` name bag only. No CTC, no `006AC430`, no face anim. DIVERGE / LEFTOVER.**
