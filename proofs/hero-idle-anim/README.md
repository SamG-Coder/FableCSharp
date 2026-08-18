# First Hero idle / stand XSEQ after Leave Frontend

Investigation only. No production `src` edits.

Sibling of `proofs/xseq-first/README.md` (first **object**: empty
`00AA4710` / `00A999B0` helper on Init Mesh Bank) and
`proofs/xseq-walk-first/README.md` (first **cyclic** sample of any
creature). This note is the first **Hero** (`CREATURE_HERO` /
`ScriptName=Hero` / mesh **4299**) idle or stand clip after
Leave.

Do **not** start at Oakvale / `CS_WAKING_UP_LOOP` / `3420` /
`ST_IDLE_SUBTLE` / `PlayAnimation` `00CC15DA`. That path is later
`Q_NewOakValeIntro` / `CS_OAKVALE_INTRO_FATHER`, not Leave /
Init World / first no-save 3D Present.

Do **not** treat `EngineLifecycle.PlayerManagerIdleFn`
(`009AC9E0`, pump `ret 4`) as a clip.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER**.

Sources: `proofs/xseq-first/README.md`;
`proofs/xseq-walk-first/README.md`;
`proofs/anim-blend-first/README.md`;
`proofs/palskin-open/README.md`;
`proofs/player-bind-world/README.md`;
`docs/runtime/FORWARD_TREE.md` §§7, 14–15;
`docs/status/investigations/E-player-palskin.md`;
`docs/status/investigations/2026-08-18-palskin.md`;
`WorldShading.cs` / `RegionTravel.cs` / `MeshBank.cs` /
`XSeqFile.cs` / `AnimationRuntime`;
`XSeqFormatTests` / `EngineLifecycleTests` /
`ScriptRuntimeArchitectureTests` / `WorldSceneTests`;
listings `006AC910` / `006A9DD0` / `00662880` / `004C9CA0` /
`005B37F7` / `0070B4D0` / `0070D580` / `00662A00` / `0073E320` /
`00982210` / `00E5E5A0` / `00A26D40`.

---

## Verdict

**There is no Hero idle / stand XSEQ after Leave on the no-save
spine.** Hero does not exist at Leave. Create
`006AC910` / `006A9DD0` / `004C9CA0` never request a clip.
First Present dest for mesh **4299** is C3D bind locals
(`FirstSeenPlaysAnim=false`,
`FirstSeenPlayAnimationAppliesPose=false`,
`FirstSeenAppearancePlaysDefault=false`).

A later named clip is **UNREAD** as identity. Do not invent
`STAND` / `HERO_IDLE` / `ST_IDLE` / `STANDARD_IDLE` /
`DEFAULT` / `3420` as that first Hero cycle.

Native type-6 *reader* VAs are **PROVEN**. Host `XSeqFile.Parse`
/ `MeshBank.GetAnim` is the stand-in. Lifecycle never calls it.

---

## Timeline (no-save New Game)

```
0042EC7C retail
  PlayAVI / frontend 2D          // UI type 6 = glyphs 0054EF00
                                 // no Hero Thing, no MBANK
0042F2A2 Leave frontend
0042F491 Init Game → 00418DCA → 004184BD
  Create Players 004166A8        // slots; NOT Hero
  Init World 004A6E30
    0049E620 Init Mesh Bank
      00AA4710 → 00A999B0        // empty 3DAF+XSEQ helper  (xseq-first)
      009D56C0 directory         // type 6 indexed, ParsedCount=0
      00AA0F60 mixer bank+960    // 0 channels               (anim-blend-first)
  00416953 Load world FinalAlbion.wld
  004AE9D0 PlayerBindAfterWorld  // tick slots; NOT Hero     (player-bind-world)
004189C2 first pumps
  FirstSeenPlaysAnim=false
  no 00A26D40 type-6 payload
  no 004C7470 / 0070D580 / 0070B4D0 / 005B37F7
  no Hero
later 00501450 Lookout / 006AC910 hero 4299
  006A9DD0 ConstructFromParams
    00662880 parent / 004CA010 Graphic
    0042B0A2 appearance attach
    004C9D60("CTCPhysicsControlled")   // not CTCAnimationComplex
  004C9CA0 activate                    // vtbl+32/+36/+40; no play
  create has no PlayAnimation / STAND / HERO_IDLE / CTCIdle / 005B37F7
  first PALSKIN dest = bind locals (00AA0090, 0 channels)
```

`3420` `CS_OAKVALE_DREAM_INTRO_YOUNG_HERO_WAKING_UP_LOOP` and
`Hero.PlayAnimation ST_IDLE_SUBTLE` are **not** on this list.
**PROVEN** leftover fixtures.

---

## 1. Hero does not exist at Leave

| Claim | Class | Evidence |
|---|---|---|
| Frontend / Leave constructs `CREATURE_HERO` | **DISPROVEN** | `0042F2A2` is fade / teardown. No `006AC910`. `HeroSpawned` later |
| `004AE9D0` is Hero spawn | **DISPROVEN** | tick-slot sync on `game+80568` (`player-bind-world`) |
| `009AC9E0` PlayerManager idle is a clip | **DISPROVEN** | `ret 4` pump note. Not `0070D580` |
| First Hero Thing is Lookout `006AC910` | **PROVEN** | `GuildArrivalHSP` / `CREATURE_HERO` / `ScriptName=Hero` / mesh **4299**. Lookout TNG has no `PlayerCreature` |

**Answer:** no Hero idle is possible until after `00501450`.
Leave cannot open a Hero XSEQ.

---

## 2. Create / activate do not start idle

`006AC910` (`80` insns): alloc `0x208` → `0052AB20` →
`006A9DD0` → `004C9CA0`. Direct calls are construct / string
temps / `006A06E0` pose pack / activate. **0** `E8` to
`004C7470` / `0070C050` / `0070D580` / `005B37F7` /
`0070B4D0` / `00A26D40`.

`006A9DD0` ConstructFromParams:

```
00662880 parent          // 008388D0 + 004C7990 / 00513160
0042B0A2 [esi+112]       // appearance attach
004C9D60("CTCPhysicsControlled")
```

Not `CTCAnimationComplex`. Not `CTCIdleScheduler`.

`004C9CA0` activate: `[vtbl+32]` then `+36` / `+40`, or flags
then `[vtbl+48]` + `004C8C00` / `005202B0` / `0051E000`.
**No** clip name.

`CTCAnimationComplex` factory `0070B3F0` + post-attach
`0070B600` is `mov al,1; ret`. **PROVEN** stub. **DISPROVEN**
as a play site.

`0070D580` `E8` sites: **38**. None are `006AC910` /
`006A9DD0` / `004C9CA0` / `00662880` / Leave / first pumps.

| Claim | Class |
|---|---|
| Create plays `STAND` / `HERO_IDLE` / `ST_IDLE` | **DISPROVEN** |
| Create plays `DEFAULT` via `005B37F7` | **DISPROVEN** (`E8` only `005B4E7F` / `005B8758` → clothing GUI `005B6881` / `PC_UI_FRAME` `005B8743`) |
| `0070B4D0` (`CTCAnimationComplex` vtbl+16) plays `DEFAULT` mode 6 | **PROVEN** body (`push "DEFAULT"` → `005DC340` → `0070C050(6)` → `0070D580`). **DISPROVEN** as create / first Present |
| `CTCIdleScheduler` first tick starts a clip | **UNREAD** ctor/tick. Name register `004D5EA8` / `004D2EF0` **PROVEN**. **DISPROVEN** as XSEQ open on this spine (0 create call) |

**Answer:** first Lookout Hero frame is bind pose. Do not call
`PlayAppearanceDefault` from `SpawnHero`.

---

## 3. Named idle / stand tokens — leftover, not this spine

Exe strings exist. They are **not** Leave / create / first
Present callers.

| Token | VA / site | On Leave / `006AC910`? |
|---|---|---|
| `STAND` `0x012674DC` | **0** rows in `xrefs.tsv`. Listing still `push "STAND"` at `0073AB50` (component-miss fallback) and ctor family `0073E320` / `0073E410` / `0073E510` / `0073EACA` (`STAND` + `NOT_CARRYING` + `STAND_FRONT`/`LEFT`/…) vtbls `012674AC` / `012674F4` | **DISPROVEN** as this spine. **PROVEN** as later action-state defaults |
| `STAND_FRONT` / `LEFT` / `RIGHT` / `HAPPY` / `BORED` | same `0073E3xx` family | leftover |
| `HERO_IDLE` `0x012E7074` | `00E5EB0C` / `00E5EB84` inside `00E5E5A0` | **PROVEN** as a later script-table fill (caller uses `00CB7940` hero-exists). **DISPROVEN** as Leave / create |
| `STANDARD_IDLE` `0x01299594` | `00982219` ctor `00982210` → `006924B0` / `00693B30` | **DISPROVEN** as this spine |
| `ST_IDLE` | `007ECFD9` and later AI/combat | **DISPROVEN** as this spine |
| `ST_IDLE_SUBTLE` | `008621A0` / `008D6780` / `00E9E420`; host intro list `Hero.PlayAnimation ST_IDLE_SUBTLE` | **DISPROVEN** as Leave. Oakvale leftover |
| `DEFAULT` | miss path `00662AAB`; `0070B518`; `005B3A26` | **PROVEN** fallback *once lookup/play runs*. Lookup / play do **not** run at create |
| `CTCIdle` | fnmap **0** fns | **DISPROVEN** as a play site |
| Wake `3420` | `XSeqFormatTests` fixture | **DISPROVEN** as Hero Lookout clip (kid 4300 / dream) |

Appearance table:

```
00662A00  ecx = appearance
  [ebx+308] → 0073A6E0 / vtbl+16
  0042B0A2([ebx+112])
  lea edi, [esi+52]             ; 20-byte name table
  005DC2E0 contains(name)
    hit  → 005DC340 walk
    miss → push "DEFAULT" → 005DC340
```

`CAppearanceDef` idx **10533** on `CREATURE_HERO` is type
**PROVEN**; raw body **UNREAD**. Combat names at +3697 are not
the 20-byte runtime table. First clip id/name after Leave stays
**UNREAD**. Do not pair Lookout Hero to `STAND` / `HERO_IDLE` /
`DEFAULT`.

---

## 4. Type-6 payload vs first-key (format, not runtime)

`MeshBank.Open` indexes type 6 at Init Mesh Bank. `ParsedCount=0`.
`00A26D40` `cmp ebx,6` / `00A26ED3` is **PROVEN**. First no-save
pump / create / first Present **DISPROVEN** as that branch.
First C3D parse is type 5 Graphic **4299**, not type 6
(`palskin-open`).

`00A4C5E0` / `00A4CDD0` callers are inner persist, not
`004A6E30` / `006AC910`.

Host `GetAnim` / `FindAnim` is the later slot. `EngineLifecycle`
never calls them (**MATCH** skip). `FindAnim("STAND")` /
`FindAnim("HERO_IDLE")` as a first-seen id is **UNREAD** and
must not be invented.

A Hero idle *cycle* would need: type-6 payload **and**
`0070D580` **and** mixer channels / `00AA0090` time lerp.
None of those three run on this spine. First `00AA0090` is
PALSKIN dest pack with **0** channels → bind locals
(`anim-blend-first`).

`TrySample` / `PaletteForPose` discard `time` and take the
first stored key. Even a cyclic `HERO_IDLE` forced through
host submit would pose frame 0 only.

---

## 5. C# vs native

| Site | What | Class |
|---|---|---|
| `SpawnHero` / `006AC910` insert `CREATURE_HERO` 4299 | Thing | **MATCH** identity. No appearance/clips |
| `InsertThing` first Graphic only | drops `CAppearanceDef` 10533 | **PARTIAL** vs native attach. **MATCH** first-seen pose (bind) |
| `AnimationRuntime.Clips` empty on engine path | no `GetAnim` | **MATCH** skip |
| `PlayAppearanceDefault` | `005B37F7` / `0070B4D0` stand-in | **EQUIVALENT** later. **DISPROVEN** if called from create |
| `PlayLoopingAnim` host `ClipKey=WALK` | vtbl+80 | **PARTIAL** vs native; **DISPROVEN** as first-seen |
| `WakeLoopId=3420` | Oakvale dream | **LEFTOVER** vs Leave / adult 4299 |
| `PaletteForPose(..., sequence)` first-key | 48-byte locals | **PROVEN** format. Unused on first Present |
| `PlayerManagerIdleFn` | pump | **DISPROVEN** as XSEQ |

---

## Classifications (short)

1. **Frontend / Leave Hero idle XSEQ — DISPROVEN.** No Hero.
   UI type 6 is glyphs.
2. **First XSEQ *object* after Leave — empty helper. PROVEN**
   in `xseq-first`. Not a Hero clip.
3. **First Hero Thing after Leave — Lookout `006AC910` mesh
   4299. PROVEN.** Create / activate do not play.
4. **First Hero idle / stand *name* after Leave — UNREAD.**
   `STAND` / `HERO_IDLE` / `ST_IDLE` / `STANDARD_IDLE` /
   `DEFAULT` are leftover later setters. `3420` / `ST_IDLE_SUBTLE`
   **DISPROVEN** as this site.
5. **First Hero PALSKIN dest after Leave — bind locals.
   PROVEN.** A cycle cannot appear until `0070D580` + mixer
   channels + `00AA0090` time.
6. **Host `GetAnim` / `PlayAppearanceDefault` during New Game —
   LEFTOVER if called; MATCH if left on-demand.**

Do not treat `XSeqFile.Parse(3420)`, `STAND`, `HERO_IDLE`,
`PlayAnimation` apply, or `PlayerManagerIdleFn` as the first
Hero idle after Leave.
