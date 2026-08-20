# `GuildArrivalHSP` parse vs construct

Investigation only. Production `src/` was not edited.

Do **not** start at Oakvale / `00DBDE40` / `StartOakVale` /
`StartOakValeWest` / `NOVStartHSP` / `CREATURE_HERO_CHILD`.
No-save New Game is message **15** → Leave `0042F2A2` →
`FinalAlbion.wld` → LookoutPoint.

Do **not** collapse `004FDBC0` (parse) with `0051FD80`
(construct) or `0052AC90` (holy-site factory). Do **not**
treat Hero `006AC910` as the HSP construct.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER**.

Question: when is Lookout `HOLY_SITE_PLAYER_START`
**`GuildArrivalHSP`** constructed vs only parsed?
`004FDBC0` parse vs `0051FD80` construct. What is the first
`0052AC90` holy-site factory after Leave?

Authority: dump `0052AC90` / `0051FD80` / `004FDBC0`;
`proofs/tng-first-after-leave`, `proofs/first-0051FD80-file`,
`proofs/hero-4299-create`; siblings `tng-spawn`, `tng-after-leave`,
`tng-first-def`, `creature-after-leave`, `script-setnewstart`;
listings `listing-004c0000.txt` / `listing-00500000.txt` /
`listing-00a00000.txt`;
`docs/status/investigations/2026-08-18-first-scene-things.md`
(+ dump);
Anniversary loose `LookoutPoint.tng` / `BowerstoneBridge.tng`
(UID/pos **PARTIAL** vs TLC WAD; def/script/kind **PROVEN**);
`EngineLifecycle.LoadGlobalThingsFile` / `LoadSingleThing` /
`SpawnHeroFromPlayerStart`;
`EngineLifecycleTests.Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`.

---

## Verdict

**Parsed in `004FDBC0`. Constructed later by `0051FD80`.**

`GuildArrivalHSP` is a Lookout TNG `NewThing Holy Site`. First
open after Leave is `LookoutPoint.tng` inside `004FDBC0` (still
in `00507C30` / Loading world). That walk **parses** the block.
It does **not** call `0052AC90` on the host / first-seen empty
countable list.

CThing construct is `00501450` → `006C2170` Loading objects
**ContainsMap[1]** `LookoutPoint.tng` → `0051FD80` →
`00A371C0` → **`0052AC90`**. That is after dummy pumps and
after 88 Bridge constructs.

The **first** `0052AC90` factory after Leave is **not**
`GuildArrivalHSP`. It is **`BowerstoneBridgeHSP`** on
ContainsMap[0]. `006AC910` constructs **Hero**, not the marker.

| Claim | Status |
|---|---|
| First **parse** of `GuildArrivalHSP` is `004FDBC0` / `LookoutPoint.tng` | **PROVEN** |
| That parse is **before** Set Static Map / dummy / `00501450` | **PROVEN** |
| `004FDBC0` **constructs** the HSP (`0051FD80` / `0052AC90`) | **DISPROVEN** host. Live `[manager+128]` **UNREAD** |
| `GuildArrivalHSP` **construct** is Lookout `0051FD80` → `0052AC90` | **PROVEN** |
| First `0052AC90` after Leave is `GuildArrivalHSP` | **DISPROVEN** (`BowerstoneBridgeHSP` first) |
| First `0052AC90` after Leave is the Create Players registrar | **DISPROVEN** (stores `0x52AC90`; no call) |
| `006AC910` / `0049F180` constructs the HSP | **DISPROVEN** (Hero / bind) |
| First holy-site parse is Oakvale / `NOVStartHSP` | **DISPROVEN** |
| HSP is a first-Present C3D | **DISPROVEN** (`FirstSeenInstancesAsC3d` false) |

---

## Path (no-save New Game)

```
0042F2A2  Leave frontend                         // no TNG
0042F491  Init Game → 004184BD
  Create Players 004166A8
    00522A20  registrar  "Holy Site" / "HOLY_SITE"
              [esp+28] = 0x52AC90  kind 7        // TABLE only
00416953  Loading world  FinalAlbion.wld
  004A1840
    00507C30  Load .wld file                     // PARSE tables
      NewMap 1  LookoutPoint                     // Maps[0]
      NewRegion 1  ContainsMap Bridge, Lookout, Guild
      NewRegion 4  StartOakVale                  // later; not this tree
      0050959F  stem+.gtng  TLC miss             PROVEN skip
      00509859  Load global things
        [0x13B8609]==0 → 004FDBC0                ← FIRST OPEN
          ebx=1 skip slot 0
          first hit Maps[0] LookoutPoint.tng
            NewThing Marker MARKER_BASIC M_Maze
            … 33 more NewThings …
            NewThing Holy Site                   ← FIRST PARSE of HSP
              DefinitionType HOLY_SITE_PLAYER_START
              ScriptName     GuildArrivalHSP
            then LookoutPointHSP, later MAIN_START_POSITION
          005223F0  [manager+128]==1?            UNREAD live
            host: GlobalThings concat, no 0051FD80 / no 0052AC90
          next prox slot BowerstoneBridge.tng    // also parse only
            BowerstoneBridgeHSP
    004A1BD3  Set Static Map                     // AFTER parse
    0049F180  Init Characters
      00489D40 → 00488B20 holy-site miss         // no 006AC910
004189C2  dummy pumps  0 things
later 00501450 → 00500540(1,0,0) LookoutPoint
  006C2170  Loading objects  ContainsMap
    [0] BowerstoneBridge.tng  88
      0051FD80  TRACK_NODE_BASIC GuardTrack      // first Thing
      … ×8 track, objects, building …
      0051FD80  HOLY_SITE_PLAYER_START
                BowerstoneBridgeHSP              ← FIRST 0052AC90
    [1] LookoutPoint.tng  288                    ← Lookout CONSTRUCT
      first: MARKER_BASIC M_Maze
      later: 0051FD80 GuildArrivalHSP            ← THIS HSP CONSTRUCT
              00A371C0 → 0052AC90
      then LookoutPointHSP, later MAIN_START_POSITION
    [2] GuildExterior.tng  88                    // no Holy Site
  006AC910  CREATURE_HERO ScriptName=Hero 4299
            pose copied from GuildArrivalHSP     // USE, not construct
```

`Q_NewOakValeIntro` / `00DBDE40` never open or construct this
HSP. **PROVEN**.

---

## 1. Dump `004FDBC0` — parse, no factory

`listing-004c0000.txt`:

```
004FDBC0  sub esp, 8
          map count = ([+36]−[+32]) / 72     // 0x38E38E39
          ebx = 1, edi = 0x48                // skip unused slot 0
004FDC00  push "Loading global things"
          [slot+36] && [slot+40]             // filled + prox
            call 004FBF60(ebx)
          inc ebx / add edi, 72
          jb 004FDC00
004FDCA8  ret
```

`004FBF60` (`listing-004c0000.txt`):

```
004FBF76  lea ecx, [eax+edx*8+24]            // slot name
004FBF85  call 004FAFF0                      // append ".tng"
          [worldmap+168] → 00A39D80 else 0099AD80
004FC01E  call [edx+12]
004FC023  call 005223F0
```

`005223F0` (`listing-00500000.txt`):

```
005223F7  mov eax, [esi+128]
005223FF  cmp eax, 1
00522407  jne 00522502                       // drop stream
0052249F  call 00521AE0                      // only if +128==1
005224AB  call 0051E2F0
```

`004FDBC0` has **no** `E8 0051FD80` and **no** `E8 0052AC90`.
Open is `004FAFF0`. Construct is gated.

First prox file is `LookoutPoint.tng` (`tng-first-after-leave`).
Dump global walk: Lookout 288 then Bridge 88 then Guild
complex / woods / exterior. First `NewThing Holy Site` in
that first file is **`GuildArrivalHSP`**.

Host `LoadGlobalThingsFile` concatenates into `GlobalThings`
and does **not** `LoadSingleThing`. **MATCH** vs an untaken
`+128` gate. First-seen `00416392==0` after Init Game is
**PARTIAL** evidence the countable list stayed empty.

If live `+128==1`, native would construct Lookout during
`00416953` (first HSP would then be `GuildArrivalHSP` **before**
dummy). That take stays **UNREAD**.

---

## 2. Dump `0051FD80` — construct one `NewThing`

`listing-00500000.txt`:

```
0051FDA6  push "Load Single Thing 1"
          esi+24 in {2,3} → def stream 004C81F0
          else tokenizer 009BA330
0051FE7F  push "EndThing"
0051FECC  push "EndThing;"
0051FEF1  push "Load Single Thing 2"
0051FF1B  push "NULL"                        // default DefinitionType
          00528760  def lookup
          [world+258] && "PlayerCreature" → 00449970 / 00487DC0
          else:
00520114  "Load Single Thing: Allocate Class"   00A371C0
00520159  "Load Single Thing: Construct Thing"
            [+24] in {2,3} → vtbl+64 else vtbl+16
005201BA  "Load Single Thing: Initial Activate"
            PlayerCreature → vtbl+36/+40
            else vtbl+32  (or 004C9B80 miss)
00520246  "Load Single Thing 3"
```

`00A371C0` (`listing-00a00000.txt`):

```
00A371C0  call 00A37060                      // type index
          ecx = [eax*20 + table + 12]        // factory dword
          test ecx / je miss
          call ecx                           // 0052AC90 for HOLY_SITE
```

Region caller is `00520D00` @ `00520F9A` after
`"NewThing"` / `"Loading entities from script"`.
File open is `00522720` / `00521AE0`, **not** `0051FD80`.

Lookout TNG has **no** `PlayerCreature`. HSP takes the
allocate / construct / `vtbl+32` path, not the player bind.

---

## 3. Dump `0052AC90` — `CThingHolySite` factory

Registrar (`00522A20` during Create Players, after Leave,
**before** any `.tng`):

```
00522E69  push "Holy Site"
00522E79  push "HOLY_SITE"
00522E90  mov [esp+24], 0x07                 // kind 7
00522E95  mov [esp+28], 0x52AC90             // factory
          00A36F90  insert type row
```

TypeName getter `0051DC30`: `push "HOLY_SITE"`; `ret`.
RTTI `CThingHolySite` `0x0137B854`.

Factory body (`listing-00500000.txt`):

```
0052AC90  push esi
          xor dl, dl
          mov ecx, 0xD8                      // size 216
          call 004C7380                      // alloc
          test esi / je 0052ACDB
          push 7
          call 005296B0                      // base, kind 7
          lea edi, [esi+192]
          mov [esi], 0x1244F7C
          call 00A01B10
          mov [edi], 0x1238C6C
          mov [esi], 0x12452DC               // CThingHolySite vtbl
          mov [esi+208], 0
          mov eax, esi
          ret
```

This is **not** `0052B880` / `006AC910` (Hero size `0x208`).
It is **not** `005272E0` (AICreature).

Other `E8 0052AC90`: `0083BBFB` (later leftover helper).
**DISPROVEN** as first-seen after Leave.

---

## 4. First `0052AC90` after Leave

Create Players only **stores** the pointer. First **call** on
the proven region walk is the first `HOLY_SITE_PLAYER_START`
`0051FD80` after dummy.

ContainsMap file order is WLD bytes:
`BowerstoneBridge` → `LookoutPoint` → `GuildExterior`.

Anniversary `BowerstoneBridge.tng` (88; first eight
`TRACK_NODE_BASIC` `GuardTrack`, then objects / one building):

```
NewThing Holy Site                    // NewThing #47 in file
  DefinitionType "HOLY_SITE_PLAYER_START"
  ScriptName     BowerstoneBridgeHSP
  UID            18446741874686297328
  pos            (78.928, 81.643, 20.000)
```

That is the first `0052AC90`. **PROVEN** order vs dump
census (Bridge Holy Site ×1). TLC WAD UID/pos **PARTIAL**.

`GuildExterior.tng` has **no** `NewThing Holy Site`.

So after Leave:

| # | Factory call | File | When |
|---|---|---|---|
| 1 | `BowerstoneBridgeHSP` | Bridge ContainsMap[0] | first `0052AC90` |
| 2 | **`GuildArrivalHSP`** | Lookout ContainsMap[1] | this note |
| 3 | `LookoutPointHSP` | same file, later | not start |
| 4 | `MAIN_START_POSITION` | same file, later | not start |

---

## 5. `GuildArrivalHSP` — parse vs construct

Lookout file order (Anniversary; Gameflow then NULL). First
two `NewThing`s are `MARKER_BASIC` `M_Maze` /
`M_LadyGameflow`. First holy site is the 35th `NewThing`:

```
NewThing Holy Site
  Player         4
  UID            18446741874686297902
  DefinitionType "HOLY_SITE_PLAYER_START"
  ScriptName     GuildArrivalHSP
  pos            (52.688, 69.597, 36.982)
  fwd            (0.999994, 0, 0)            // +X
  up             (0, 0, 0.999994)            // +Z
```

Then `LookoutPointHSP` `(102.781, 74.156, 37.494)`, then much
later `MAIN_START_POSITION` `(102.887, 74.127, 37.488)`.
Dump exist-set matches those three. **PROVEN**.

| Event | Site | `0052AC90`? |
|---|---|---|
| First text parse | `004FDBC0` Lookout open | **no** (host) |
| Re-parse on apply | `00521AE0` ContainsMap[1] | n/a (token walk) |
| Allocate / vtbl | `0051FD80` → `00A371C0` | **yes** |
| Hero pose copy | `00489D40` / `006AC910` | **no** (Hero factory `0052B880`) |

Host `LoadSingleThing` Notes `"0052AC90 HOLY_SITE "` +
`ScriptName` when `DefinitionType==HOLY_SITE_PLAYER_START`.
`SpawnHeroFromPlayerStart` prefers script `GuildArrivalHSP`,
then any positioned holy site. `FirstSceneMapName=LookoutPoint`.

`FirstSeenInstancesAsC3d("HOLY_SITE", "HOLY_SITE_PLAYER_START")`
is false. Marker exists; not submitted.

---

## 6. Use after construct (`006AC910`, not HSP)

Lookout TNG has **no** `PlayerCreature` / `CREATURE_HERO`.
Load World `0049F180` → `00489D40` → `00488B20` **misses**
(empty holy-site list; `NOVStartHSP` is not here).
**PROVEN** (`hero-4299-create`).

After the three ContainsMap files:

```
00488B20  finds GuildArrivalHSP
00489FC1  call 006AC910
  size 0x208 / 0052AB20 / 006A9DD0
  CREATURE_HERO ScriptName=Hero  mesh 4299
  pose (52.688, 69.597, 36.982)  +X / +Z
```

Which later `E8 00489D40` first hits `00489FC1` is **UNREAD**.
Host folds create into `LoadFromFirstRealRegion` after TNG.
**MATCH** order (“after maps”). Noting `0049F180` at that
site is **LEFTOVER**.

---

## Not these

| Candidate | Why not this HSP parse / construct / first factory |
|---|---|
| `NOVStartHSP` / `00DBDE40` | `userst` store before frontend; Oakvale leftover |
| `MAIN_START_POSITION` / `LookoutPointHSP` | other Lookout HSPs; not no-save pose |
| `BowerstoneBridgeHSP` | first **factory**; different map / pose |
| `GuildTrainingHSP` | `HeroGuildComplexInside`; later tween |
| `FinalAlbion.gtng` / `.gtg` | miss / BSS 0 |
| Create Players `00522A20` | type table only |
| Dummy index 0 | 0 things |
| Bridge `TRACK_NODE_BASIC` | first `0051FD80`, not holy site |
| `CREATURE_BS_VILLAGER_MALE` | first creature; later Lookout section |
| Hero `006AC910` | uses the HSP; factory `0052B880` |

---

## Host vs native

| Site | Host | Native |
|---|---|---|
| First parse | `LoadGlobalThingsFile` Lookout | `004FDBC0` ebx=1 → `.tng` |
| Global construct | no `LoadSingleThing` | `005223F0` gated **UNREAD** |
| First `0052AC90` | Bridge `BowerstoneBridgeHSP` | same if `+128` off |
| This HSP construct | Lookout `LoadSingleThing` note | `0051FD80` → `0052AC90` |
| Hero | `SpawnHeroFromPlayerStart` after maps | `006AC910` after apply |

---

## UNREAD / PARTIAL

- Live `[manager+128]` on first `005223F0` (would construct
  Lookout HSPs during `00416953`, still **not** Oakvale).
- `00501450` E8 caller (body recovered).
- Live factory miss `004C9B80` on first holy site.
- TLC WAD vs Anniversary UID/pos for the four HSPs above
  (def/script/kind/Lookout dump pose **PROVEN**).
- First later `00489D40` that finds `GuildArrivalHSP`.

---

## Do not

- Report `GuildArrivalHSP` as constructed inside `004FDBC0`.
- Report the first `0052AC90` as `GuildArrivalHSP` (Bridge
  is first on the region walk).
- Move HSP construct to `006AC910` / `0049F180`.
- Bind `NOVStartHSP` / `MAIN_START_POSITION` /
  `StartOakVale` / kid `4300` as this marker.
- Draw the holy site as a first-Present C3D.
- Collapse first **open** (Lookout parse) with first
  **construct** (Bridge Things).
