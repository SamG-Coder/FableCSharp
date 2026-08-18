# First navigation mesh / AI pathing (after Leave)

Investigation only. Production `src/` was not edited.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** / **LEFTOVER**.

Question: when does native first **construct** a navigator, first **load** a
nav mesh / quadtree, and first **path** after Leave? Must that be after
world / region load, not during frontend?

Sources: listings `004A6E30` / `00A15670` / `00501990` / `006C2170` /
`004FF080` / `00500230` / `0050AF10`;
`docs/runtime/FORWARD_TREE.md` §§7–9;
`EngineLifecycle.InitWorldInitStages` / `JobNavPassFn`;
RTTI `CNavigatorManager` / `CNavigatorAStar` / `CNavQuadTree`;
`proofs/newgame-script/`, `proofs/camera-after-leave/`.

---

## Verdict

**First navigator exists after Leave + Init World. Not frontend.**

**First Lookout apply loads region topology (`004FF080` / `008224E0`)
and skips the `CNavQuadTree` commit (`00500230` / `0050AF10`, job `+12=0`).**

**First live A\* / WalkTo / FollowNavRoute on this no-save spine is
not first-seen.** Oakvale sneak/walk is leftover.

| Object | First native site | Frontend `0042DF9E` |
|---|---|---|
| `CNavigatorManager` | `00A15670` ← `004A6FFB` Init World | no ctor |
| A\* / flyer register | `[world+72].vtbl+4` `"Navigator A Star"` / `"Navigator flyer"` | no |
| Topology grid | `006C2170` pass 1 → WorldMap `vtbl+24` (`004FF080`) → `008224E0` | no |
| `CNavQuadTree` insert | `00A156D0` / `00A157C0` | no |
| First-seen quadtree commit | `0050AF10` | **skipped** (`+12=0`) |
| Script gait (`WalkTo` / `FollowNavRoute`) | later VM / Oakvale | **DISPROVEN** as Leave |

---

## Recovered order (no-save New Game)

```
0042EC7C retail pump
  0042DF9E  frontend frame          // 2D UI only
  no 00A15670 / 00501990 / 004FF080 / 00A157C0
retail+41
0042F2A2 Leave
  FinalAlbion.wld
  00418DCA Init Game
    0041735A Init World
      004A6E30 Init World Init
        00A15670  "Init Navigation Manager"
          alloc 48, vtbl 0x129CA84 → [world+72]
          00A373B0(5); +32=0x320; +36/+44 empty circular lists
          arg 4 + [game+8]+0x1613C
        [nav].vtbl+4  "Navigator A Star"   flag [0x129CBA4]
        [nav].vtbl+4  "Navigator flyer"    flag [0x129CB44]
        006B97E0 flyer object → [world+84]
    00416953 Load world
    004B4260 START_INITIAL_QUESTS
      004B3CE0 construct
        004B2510 → 00500EB0 / 00501990
          WorldMap+144 still empty → UpdateNavMaps no-op
004189C2 dummy pump
  index 0; no 00501450
later 00501450 Lookout
  006C27A0 / 006C2120 / 006C2710
  006C2170 apply
    pass 1 [rec+4]: "Loading topology"
      WorldMap vtbl+24  004FF080
        alloc 0x1D40 → 008224E0 (vtbl 0x1272494 / 0x127252C)
        00824580 from AABB; push index onto WorldMap+144
      00638310
      vtbl+28  004FF440  "Post load topology"
    pass 2 [rec+20]: "Loading objects"  00522720 / 006C2170
    [rec+12]==0 → skip 00500230 / 0050AF10
      // would have been 00A155D0 / 00A157C0 / 00A156D0 / 00A16050
    Activate Topology 004FCBB0 / 004FCFE0
    004FC8A0 MiniMap
```

Frontend never `E8`s `00A15670`. Only caller is `004A6FFB`. **PROVEN.**

---

## 1. Frontend vs Leave

| Claim | Class | Evidence |
|---|---|---|
| Frontend Present is 2D (`0042DF9E`) | **PROVEN** | `proofs/camera-after-leave/`; FORWARD_TREE §4 |
| `00A15670` during frontend | **DISPROVEN** | one `E8`: `004A6FFB` |
| `00501990` / `004FF080` / `00A157C0` in `0042DF9E` | **DISPROVEN** | listing `0042DF9E`; e8.tsv |
| World + nav manager exist before Leave | **DISPROVEN** | `004A67D0` / `004A6E30` only after Init Game |
| Host `EngineLifecycle` field `new()` of cameras during Bootstrap | **LEFTOVER** | not a navigator; see camera proof |

**Answer:** no nav mesh and no AI pathing during frontend.

---

## 2. First navigator after Leave

`004A6E30` (world vtbl+36), after Environment / WorldCamera:

```
004A6F82  "Init Navigation Manager"
004A6FCE  push 48
004A6FD0  00BFEA1A
004A6FE4  [esi+8]+0x1613C
004A6FEB  push 4
004A6FFB  00A15670
004A7009  [world+72] = eax
004A7016  "Navigator A Star"
004A7050  [nav].vtbl+4
004A7067  "Navigator flyer"
004A70A1  [nav].vtbl+4
004A70B6  alloc 16 → 006B97E0 → [world+84]
```

`00A15670` (**PROVEN** body):

| Field | Value |
|---|---|
| vtbl | `0x129CA84` (`CNavigatorManager`) |
| size | 48 |
| `00A373B0` | ctor arg **5** (table `0x129CF70`) |
| `+32` | `0x320` |
| `+36` / `+44` | circular sentinels (map lists) |
| `+40` | `[arg].vtbl+8` |

Host `InitWorldInitStages` names `"Init Navigation Manager"` `0x00A15670`.
**EQUIVALENT** name/order. Host does **not** allocate the 48-byte object
or register A\*/flyer. **PARTIAL** vs native construct.

RTTI (names only, not first-seen call):

| Type | VA |
|---|---|
| `CNavigatorManager` | `0x013970D4` |
| `CNavigatorAStar` | `0x01397224` |
| `CNavigatorFlyer` | `0x01397118` |
| `CNavQuadTree` | `0x013985B8` |
| `CNavNavigableLeafNode` / `CNavBlockedQuadTreeNode` / `CNavSwitchableLeafNode` | `0x0139856C` / `44` / `90` |
| `CTCCreatureNavigation` | `0x0137FE28` |
| `CCreatureNavigationDef` | `0x01379D20` |
| `CTCPreCalculatedNavigationRoute` | `0x0137AEEC` |

`[nav].vtbl+4` bodies **UNREAD** (slot proven; first-seen only the two
string-tagged calls).

---

## 3. `nav.data` is not the first load

`00A15890` (`e8` from `00A160E8` self and `00A1D70A` inside `00A1C010`):

```
00A158EA  push "…\FablePathVisualiser\nav.data"
00A158EE  00BFEDFA
miss →
00A15905  push "nav.data"
```

Debug / visualiser dump, then `"%f, %f, %f, %f, %f, %f, %f, %f, %d"`.
**DISPROVEN** as Leave / Init World / first Lookout apply.

Live New Game `E8` of `00A15890` **UNREAD** (not on the FORWARD_TREE
spine). Do not invent a `nav.data` open on New Game.

---

## 4. First topology after Leave (Lookout apply)

Reached after dummy pump. Not `00DBDE40`. FORWARD_TREE §9.

`006C2170` pass 1 if `[rec+4] != 0`:

```
006C220A  WorldMap.vtbl+24(index, blob)   // host: 004FF080
006C2217  00638310(index)
006C2250  WorldMap.vtbl+28(index)         // host: 004FF440
```

`004FF080` (**PROVEN** body):

```
alloc 0x1D40
0049D810 → [game+8]+0x1613C     // same cookie as 00A15670
008224E0(index, map-row, cookie, …)
push region index onto WorldMap+144/+148
0063DFD0
```

`008224E0` vtbl `0x1272494` then `0x127252C`; AABB → `fmul [0x123078C]`;
`00824580`. That is a **grid object**, not `CNavQuadTree` (`00A7A760`
vtbl `0x129DE64`).

Whether `008224E0` *is* the navigation mesh vs village / region
topology **UNREAD**. String is `"Loading topology"`. It **does**
feed `UpdateNavMaps` via `+144`. **PROVEN** as first post-Leave
topology object. **PARTIAL** as “the navmesh.”

Host `ApplyLoadJob` notes `004FF080 vtbl+24` per ContainsMap.
**MATCH** pass order. Host does not construct `008224E0`. **DIVERGE**
payload.

---

## 5. First `CNavQuadTree` — skipped first-seen

`006C2170` then:

```
006C23C6  [rec+12]
006C23CB  je skip
006C23DD  00500230(worldmap, job, blob, index)
…
006C2429  [rec+12]
006C2432  je skip
006C243D  0050AF10(worldmap, index)
```

First-seen `00500540(1,0,0)`: third arg 0 does not fill `+12`.
Host and FORWARD_TREE: **PROVEN skip.**

If `+12` were set:

| Site | Nav manager | Role |
|---|---|---|
| `005002FC` / `0050030A` | `00A14E80` / `00A155D0` | already-have? then **remove** |
| `00500331` / `005003C3` | `00A157C0` | **insert** map + `00A7A760` quadtree + `00A7AF60` |
| `0050AFCF` | `00A156D0` | **insert** from AABB + `00A762F0` + `00A7A8D0` |
| `0050B007` | `00A16050` | post-insert |

`00A14E80`: walk `[manager+36]` for `node+16 == region`.
`00A155D0`: unlink + `00A79290` + walk `[manager+24]` `vtbl+72`.
`00A7A760`: `CNavQuadTree`-shaped object, size `0x88`, vtbl `0x129DE64`.

**Answer:** first-seen Lookout does **not** insert a quadtree.
First later insert is **UNREAD** (needs a job with `+12!=0`, or
`UpdateNavMaps` rebuild that fills it).

---

## 6. `CWorldMap::UpdateNavMaps` `00501990`

```
walk [this+144, +148)
  log "CWorldMap::UpdateNavMaps"
  006C27A0  build job
  "SetAsLoading" → 006C2120([this+188])
  "Add" → while 006C20A0: "WAIT_FOR_LOAD" ; loader.vtbl+4
```

This **re-enqueues** `CLevelLoader`, it does not `E8` `00A157C0`.

Callers (`e8.tsv`):

| Site | Parent | First-seen `+144` |
|---|---|---|
| `004B2652` | `004B2510` ← `004B3E56` `004B3CE0` | empty at Init Quests (**PROVEN**) |
| `004B3C2D` | larger quest / map helper (ends `004B3CD2`) | **UNREAD** as first-seen |

`004B2510` also `E8`s `00500EB0` (same `+144` walk; name helper /
`00A39D80`). Empty list → `je 00501029`.

After `004FF080` fills `+144`, the **next** `004B3CE0` /
`004B2510` would enqueue. Type-1 `004B4490` can re-enter
`004B3CE0`. Whether that runs before first 3D Present
**UNREAD**. Do not treat Init-Quest `UpdateNavMaps` as a load.

---

## 7. First AI pathing (query / gait)

| Claim | Class |
|---|---|
| A\* *object* registered at Init World | **PROVEN** (string + `vtbl+4`) |
| `00A1C010` / `00A1E5C0` A\* family | **PROVEN** as code; `E8` only from itself |
| First live A\* query after Leave | **UNREAD** |
| `FollowNavRoute` `00CC42FA` → actor `vtbl+24` gait 0/1/2 | **PROVEN** opcode; **LEFTOVER** vs Leave (needs script) |
| `WalkTo` / `SneakTo` `vtbl+20` `004C72B0` stub | **PROVEN** dest+gait; **DISPROVEN** mesh move |
| `S_QNOVI` / `Hero.SneakTo` / `VILL1.WalkTo` | **LEFTOVER** (`proofs/newgame-script/`) |
| `CTCCreatureNavigation` on first Lookout hero | **UNREAD** (factory `006CD540` exists; not on Leave tree) |
| Frontend pathing | **DISPROVEN** |

**Answer:** first-seen after Leave is **register A\***, not **run A\***.
Script locomotion is later / leftover. `vtbl+20` remains a stub.

---

## Host vs native

| Host | Native | Class |
|---|---|---|
| Note `"Init Navigation Manager"` | `00A15670` 48-byte object | **PARTIAL** (name only) |
| No `CNavQuadTree` | first-seen skip `0050AF10` | **MATCH** skip |
| `ApplyLoadJob` topology notes | `006C2170` pass 1 | **MATCH** order |
| No `008224E0` grid | first topology object | **DIVERGE** payload |
| `WalkTo` writes `World.Positions` | dest `006A9960`; mesh `004C72B0` stub | **MATCH** dest / **MATCH** no mesh |
| `FollowNavRoute` gait table | `00BFEBA8` run/sneak | **MATCH** script layer |
| Invented New Game `S_QNOVI` walk | Leave never starts it | **DISPROVEN** / leftover |
| `nav.data` open | visualiser | **DISPROVEN** as first-seen |

---

## Classification table

| Claim | Status |
|---|---|
| First navigator is `00A15670` at Init World after Leave | **PROVEN** |
| That is not frontend | **PROVEN** |
| A\* / flyer registered immediately after the manager | **PROVEN** |
| First Lookout apply loads topology `004FF080` / `008224E0` | **PROVEN** |
| `008224E0` **is** `CNavQuadTree` | **DISPROVEN** (different vtbl / ctor) |
| `008224E0` **is** the walkable navmesh | **UNREAD** |
| First-seen `CNavQuadTree` insert | **DISPROVEN** (`+12=0` skip) |
| `nav.data` is the New Game mesh | **DISPROVEN** |
| `UpdateNavMaps` at Init Quests loads a mesh | **DISPROVEN** (empty `+144`) |
| First live A\* / `WalkTo` after Leave | **UNREAD** / leftover if Oakvale |
| Host first-frame pathing on Lookout | **DISPROVEN** as native first-seen |

Dumps: listing `004A6E30` / `00A15670` / `00A15890` / `004FF080` /
`008224E0` / `00501990` / `006C2170` / `00500230` / `0050AF10`;
`e8.tsv` callers; `rtti.txt` navigator family;
`EngineLifecycleTests.SetRegionAsLoaded_004FC8A0_is_minimap_after_005064C0`
(`00500230`/`0050AF10` skip).
