# Lookout brain / marker first-seen vs host

Investigation only. Production `src/` and `tests/` were not
edited.

Do **not** start at Oakvale / `00DBDE40` / `StartOakVale` /
`StartOakValeWest` / `CREATURE_HERO_CHILD` /
`Hero.WaitTask` / `BRAIN_GOOD_VILLAGER_BASE`.
Do **not** treat Lookout as New Game start.
Oakvale / Lookout as New Game is **DISPROVEN**.

Sibling `proofs/lookout-brain-name` is the **brain
identity** on first `CAIBrain`. Sibling
`proofs/lookout-marker-graphic` is **Graphic** on the
first two `NewThing`s. Sibling `proofs/lookout-tng-walk`
is the **file walk** of the first opened TNG. This note
is **first leftover construct vs host**, and whether that
leftover sits on the **New Game path** or a **later
region**.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **MATCH** / **LEFTOVER**.

Question: Lookout brain / marker first-seen vs host. Is
the first leftover on the New Game path or a later
region?

Authority: existing proofs (`lookout-brain-name`,
`lookout-marker-graphic`, `lookout-tng-walk`,
`005223F0-plus128-gate`, `0049E200-thing-count`,
`00416392-after-initgame`, `004FDBC0-open`,
`host-tng-construct-early`, `host-00501450-timing`,
`first-region-after-leave`, `dummy-pumps-before-region`,
`npc-first-create`, `creature-ai-first`); dump
`00523540` / `005223F0` / `004FDBC0` / `00521AE0` /
`0051FD80` / `008338D0`; TLC WLD / WAD `LookoutPoint.tng`
(world / TNG read only). Host notes only:
`EngineLifecycle.LoadGlobalThingsFile` /
`LoadRegionMapThings` / `LoadSingleThing` /
`LoadFromFirstRealRegion`.

---

## Verdict

**First leftover is on the New Game path, not a later
region.**

Dump-static first leftover is `CThingManager+128 = 1`
from ctor `00523540` (Init Thing Manager, **before**
Loading world). Nothing on the recovered New Game tree
rewrites it. First `005223F0` during `004FDBC0` therefore
**takes** `00521AE0` / `0051FD80`. First file is still
`LookoutPoint.tng`. First leftover **marker** is
`MARKER_BASIC` `M_Maze`. First leftover **brain** is
later in that same walk: `CAIBrain` `0088C160` on
`Q_FireHeart` `FH_Villager` `CREATURE_BS_VILLAGER_MALE`.

That walk is **Init Game / `00416953`**. Dummy pumps and
`00501450` have not run. Later-region ContainsMap
(`00500540(1,0,0)` Lookout → Bridge then Lookout reopen)
is **not** this leftover.

Host `LoadGlobalThingsFile` **parses** those two (and the
rest of the 288) and **does not** `LoadSingleThing`. That
skip is **LEFTOVER** vs dump-static `+128==1`, not
**MATCH**. Host first-seen of `M_Maze` / `FH_Villager`
is later `LoadRegionMapThings` after an explicit
`LoadFromFirstRealRegion` (**later region**). That is
the **when** leftover, not a New Game start.

Live RAM at the first `005223F0` is still **UNREAD**.
Do not keep “parse-only MATCH skip” as the working
model (`005223F0-plus128-gate`, `0049E200-thing-count`).

Lookout as New Game start stays **DISPROVEN**. First
Present / dummy `004189C2` still has `CurrentRegion=null`
and does not `E8` `00501450`. Oakvale `NewRegion 4` /
`00DBDE40` is later leftover, not this site.

| Claim | Status |
|---|---|
| First leftover writer is ctor `00523540` `[manager+128]=1` | **PROVEN** dump-static |
| That write is Init World, **before** `00507C30` / `004FDBC0` | **PROVEN** |
| First `005223F0` on New Game path **takes** construct | **PROVEN** dump-static; live **UNREAD** |
| First leftover **file** is `LookoutPoint.tng` (`004FBF60(1)`) | **PROVEN** |
| First leftover **marker** is `MARKER_BASIC` `M_Maze` | **PROVEN** file order |
| Second leftover marker is `M_LadyGameflow` (no Graphic) | **PROVEN** |
| First leftover **brain** is `0088C160` on `FH_Villager` `CREATURE_BS_VILLAGER_MALE` | **PROVEN** site; occupancy **PARTIAL** |
| First leftover is later region `00501450` / ContainsMap | **DISPROVEN** vs dump-static leftover |
| Later region first `0051FD80` file is `BowerstoneBridge` `TRACK_NODE_BASIC` | **PROVEN** (different site) |
| Host `LoadGlobalThingsFile` constructs `M_Maze` / brain | **DISPROVEN** |
| Host skip vs dump-static `+128==1` | **LEFTOVER** |
| Host file I/O first `LookoutPoint.tng` | **MATCH** |
| Host later `LoadRegionMapThings` order Bridge → Lookout | **MATCH** later-region body |
| Host first-seen of these Things on dummy New Game `Pump` | **DISPROVEN** |
| Host has a `CAIBrain` fibre | **DISPROVEN** (note only) |
| Graphic on `M_Maze` / `M_LadyGameflow` | **DISPROVEN** |
| Oakvale / Lookout as New Game start | **DISPROVEN** |
| `RegionTravel` “Lookout is not new-game” (implies Oakvale is) | **LEFTOVER** vs Leave |
| Concrete `BRAIN_*` instance / first `CAIStateGroup_*` | **UNREAD** (`lookout-brain-name`) |

---

## Direct answers

| Question | Answer |
|---|---|
| First leftover on New Game path or later region? | **New Game path** (`004FDBC0` leftover `+128==1`) |
| First leftover marker vs host? | Native `M_Maze` during `004FDBC0`. Host parse only. **LEFTOVER** |
| First leftover brain vs host? | Native `CAIBrain` on `FH_Villager` on that same walk. Host no fibre. **LEFTOVER** / missing |
| Is Lookout New Game start? | **No.** **DISPROVEN.** |
| Is Oakvale New Game start? | **No.** **DISPROVEN.** |
| When does host first-seen these Things? | Later region `LoadFromFirstRealRegion` / ContainsMap — **if** that helper runs |

---

## Timeline (no-save; no Oakvale; no Lookout start)

```
0042F2A2  Leave frontend                         // no Things
0042F491  Init Game
  Init World 004A6E30
    Init Thing Manager 0049EBF0
      00523540  [manager+128]=1                 ← leftover writer
  00416953  Loading world  FinalAlbion.wld
    00507C30
      NewMap 1  LookoutPoint                    // name only
      NewRegion 1  ContainsMap Bridge/Lookout/Guild
      NewRegion 4  StartOakVale                 // later leftover
      004FDBC0
        004FBF60(1) LookoutPoint.tng            // FIRST OPEN  MATCH
        005223F0  leftover 1 → 00521AE0         // FIRST LEFTOVER
          0051FD80  MARKER_BASIC M_Maze         ← leftover marker
          0051FD80  MARKER_BASIC M_LadyGameflow
          … NULL …
          0051FD80  CREATURE_BS_VILLAGER_MALE FH_Villager
            00833A70 / 008338D0 / 0088C160      ← leftover brain
        … later prox maps (PicnicArea …) …
    00416392  0051E530([world+80])              // walk ≠0 dump-static
004189C2  dummy pumps  index 0                  // not a region
  CurrentRegion=null
  no 00501450                                   PROVEN skip
later 00501450  E8 caller UNREAD                // later region
  00500540(1,0,0) Lookout job
    ContainsMap[0] BowerstoneBridge             // first later CThing
    ContainsMap[1] LookoutPoint reopen          // not first leftover
```

`00DBDE40` / `Q_NewOakValeIntro` / `WaitTask` are **not**
on this list.

---

## 1. New Game path leftover (not later region)

`005223F0-plus128-gate` / `0049E200-thing-count`:

```
005235CD  mov [esi+128], 0x1     // ctor; only first-seen writer
…
004FDBC0  004FBF60(1) LookoutPoint.tng
005223F7  mov eax, [esi+128]
005223FF  cmp eax, 1
          taken → 00521AE0 + 0051E2F0
```

`004FE030` would also force `1`, but AllowDataGeneration
`[0x1375459]` is BSS 0 — **skipped**. No other manager
`+128` store on this tree.

`00416392` after Init Game is a **census**, not the gate.
Dump-static insert onto `[manager+24]` makes that walk
**non-zero**. The older “count 0 so parse-only” working
model is **DISPROVEN** against the writer.

So the first leftover **CThing** is whatever
`00521AE0` builds from the first opened file — **during
Loading world**, still New Game Init Game. Later region
`00501450` is a **second** construct site (UNREAD caller;
dummy pumps never reach it).

---

## 2. Leftover marker (`lookout-tng-walk` / `lookout-marker-graphic`)

First two `NewThing`s in `LookoutPoint.tng`:

```
#1  Marker  MARKER_BASIC  M_Maze           no Graphic
#2  Marker  MARKER_BASIC  M_LadyGameflow   no Graphic
```

TNG `Graphic` key **absent**. `FindMeshId("MARKER_BASIC")`
**null**. `FirstSeenInstancesAsC3d` **false**. Editor
`CAppearanceDef` 4511/4512 is **not** a world Graphic.

If leftover `+128==1` is live, these two are the first
two `0051FD80`s after Leave. They are **not** the first
later-region `0051FD80` (that is Bridge `TRACK_NODE_BASIC`
`GuardTrack`).

Host `LoadGlobalThingsFile` stores them on `GlobalThings`
only. Dump `src=ContainsTng` is the **later** apply, same
UIDs, still `mesh=-`.

---

## 3. Leftover brain (`lookout-brain-name` / `creature-ai-first`)

Same leftover walk, later in the same file (after Gameflow
+ NULL; `Q_FireHeart`):

| Field | Value |
|---|---|
| Def | `CREATURE_BS_VILLAGER_MALE` |
| Script | `FH_Villager` / `"0"` |
| UID | `18446741874686300555` |
| Factory | `005272E0` → `00831F80` vtbl `0127293C` |
| Attach | `vtbl+32` `00833A70` → `008338D0` → `0088C160` |

Retail name is **not** `BRAIN_STAND_AROUND_LIKE_A_MORON`
(`[0x13B86EA]` BSS 0) and **not** `BRAIN_NULL` (compare
only). Key is CREATURE `+232` through `0079BD80`. Instance
string **UNREAD**. First `CAIStateGroup_*` **UNREAD**.

`LookoutPointBeggar`, Hero `006AC910`, and Oakvale
`WaitTask` / `BRAIN_GOOD_VILLAGER_BASE` are **not** this
leftover.

`0051E2F0` after the global load walks the **job
vector**, not `[manager+24]`. It does not empty the list
and is **not** the later-region object pass.

---

## 4. Host vs native

| Sense | Native dump-static | Host | Class |
|---|---|---|---|
| First `.tng` I/O | `004FBF60(1)` Lookout | `TryLoadThings("LookoutPoint")` | **MATCH** |
| `M_Maze` / `M_LadyGameflow` parse | same file order | `ThingFile.Parse` 288 | **MATCH** |
| Graphic on those two | none | none | **MATCH** |
| `005223F0` construct on New Game path | leftover `1` → taken | no `LoadSingleThing` | **LEFTOVER** |
| `CAIBrain` on first villager | `0088C160` at `+424` | note `"Initial Activate vtbl+32"` only | **LEFTOVER** / missing |
| Dummy New Game `Pump` first-seen | leftover Things already exist | 0 Things; `HeroSpawned=false` | **LEFTOVER** |
| Later `00501450` first file | Bridge `TRACK_NODE_BASIC` | `LoadRegionMapThings("BowerstoneBridge")` | **MATCH** body |
| Later Lookout `M_Maze` / `FH_Villager` | ContainsMap[1] reopen | same foreach | **MATCH** later region |
| Pair `EnqueueAfterDummy` to dummy `Pump` | 0 `E8` `00501450` | helper exists, unused | **LEFTOVER** glue |
| `EnsureLevels` WAD + `_RT.stb` in `004FDBC0` arm | `.tng` only | yes, before first `.tng` | **LEFTOVER** I/O (`leave-tng-first-diverge`) |

Do not report host later-region ContainsTng as the
**first leftover**. Do not “fix” host by constructing
inside `LoadGlobalThingsFile` as if Lookout were New
Game start.

`lookout-marker-graphic` / `host-tng-construct-early`
“parse-only **MATCH** skip” is **LEFTOVER** wording
against `005223F0` (gate was UNREAD; writer is now
known). Live gate remains **UNREAD**.

---

## 5. Why Lookout is not New Game start

| Myth | Class |
|---|---|
| New Game starts in Oakvale / `00DBDE40` | **DISPROVEN** |
| New Game starts at Lookout `GuildArrivalHSP` / first Present | **DISPROVEN** (dummy region null; `00501450` not on that tree) |
| `Maps[0]` Lookout is “adult overworld, not new-game” (so Oakvale is) | **LEFTOVER** (`region-travel-first`, `wld-map0-dummy`) |
| First leftover construct **is** later Lookout region apply | **DISPROVEN** vs dump-static `004FDBC0` |
| First leftover construct **means** the player starts there | **DISPROVEN** (global thing-manager list; no current region) |

New Game path after Leave is: WLD parse + leftover
global construct + dummy pumps. The **region job** that
names Lookout is later and unread as a live `E8`.
Constructing Lookout markers / brains during `004FDBC0`
is leftover **gate** behaviour, not a start pose.

---

## Not these

| Candidate | Why not first leftover |
|---|---|
| Bridge `TRACK_NODE_BASIC` `GuardTrack` | first **later-region** `0051FD80`, not leftover `004FDBC0` |
| `GuildArrivalHSP` / Hero 4299 | later Lookout holy site / `006AC910` |
| `OBJECT_SILVER_KEY` 7934 | first Graphic **def** in this file; not marker/brain |
| Picnic / Greatwood prox TNG | later `004FDBC0` slots |
| `StartOakVale*.tng` | `NewRegion 4` |
| `BRAIN_STAND_AROUND_*` / `BRAIN_NULL` / `BRAIN_GOOD_VILLAGER_BASE` | `lookout-brain-name` |
| Host `FirstSceneWorld` / `RegionTravel.NewGameRegion` | Oakvale leftover contract |

---

## UNREAD / PARTIAL

- Live `[manager+128]` bytes at the first `005223F7`.
- Exact `0051E530` sum after a taken global walk
  (`vtbl+92` identity).
- Whether leftover `0051FD80` here uses the same mode
  dword as ContainsMap `push 3` (enter test is still
  `==1`).
- Live `00666310` / `+232` occupancy on the leftover
  villager (`lookout-brain-name`).
- `BRAIN_*` instance string / first `CAIStateGroup_*`.
- Who first `E8`s `00501450` (already UNREAD).

---

## Do not

- Treat Lookout as New Game start.
- Start at Oakvale / `00DBDE40`.
- Collapse leftover `004FDBC0` construct with later
  ContainsMap construct.
- Keep “parse-only MATCH” as the host/native model for
  this VA.
- Bind a Graphic to `M_Maze`.
- Invent `BRAIN_GOOD_VILLAGER_BASE` / Wander /
  `MinionWander` as the leftover brain.
- Call host later-region `src=ContainsTng` the first
  leftover.

---

## Classifications (short)

1. **First leftover is New Game path `004FDBC0`, not
   later region `00501450`.** Writer is ctor
   `[manager+128]=1`. **PROVEN** dump-static; live
   **UNREAD**.
2. **First leftover marker is Lookout `M_Maze` (then
   `M_LadyGameflow`). No Graphic.** **PROVEN.** Host
   parse **MATCH**; host construct skip **LEFTOVER**.
3. **First leftover brain is `CAIBrain` `0088C160` on
   Lookout `FH_Villager` `CREATURE_BS_VILLAGER_MALE`.**
   **PROVEN** site. Host missing fibre **LEFTOVER**.
   Name / first group **UNREAD**.
4. **Later region first CThing stays Bridge
   `TRACK_NODE_BASIC`.** Host ContainsMap order
   **MATCH** that later body. **DISPROVEN** as the
   first leftover.
5. **Oakvale / Lookout as New Game start is
   DISPROVEN.** Dummy Present has no current region.
   `00501450` is not on the dummy tree.
