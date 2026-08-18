# First creature / Hero *create* after Leave

Investigation only. Production `src/` was not edited.

Do **not** start at Oakvale / `00DBDE40` / `CREATURE_HERO_CHILD` /
`NOVI_LiveFather` / script `Create CREATURE_OAKVALE_*`.
Those are later `Q_NewOakValeIntro`, not Leave / Init Game /
first no-save 3D Present.

Do **not** treat `0049F180` Init Characters as a Thing spawn.
It is a *name/factory bind* plus a *failed* `00489D40`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Question: after Leave Frontend, what is the first *creature*
Thing that is actually *created*, and when is Hero
`006AC910`? Is that create `0049F180`?

Sources: listings `0049F180` / `00449D90` / `0048A070` /
`00489D40` / `004A2C80` / `006AC910` / `0051FD80`;
`docs/runtime/FORWARD_TREE.md` §§7–10;
`src/Fable.Game/EngineLifecycle.cs` (`LoadWorld`,
`InitCharactersAndQuests`, `SpawnHeroFromPlayerStart`,
`SpawnHero`);
siblings `proofs/hero-stats-first`, `npc-first-create`,
`tng-spawn`, `tng-first-def`, `player-bind-world`,
`script-setnewstart`, `audit-lifecycle-newgame`,
`audit-creature-leftovers`;
`EngineLifecycleTests.LoadWorld_00416953_no_save_is_004A1840_then_0049F180`,
`Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`.

---

## Verdict

**`0049F180` does not create a creature or Hero.**

It runs during no-save `00416953` after `004A1840`, with
`ecx=world` and `push 0` (`00416BCA`). It logs
`"Init Characters"`, looks up a player Thing
(`00449970` / `00487DC0`), and on miss calls **`00449D90`**
(`009AD410("PLAYER_HERO")` miss → `"CREATURE_HERO"` →
`0048A070` → `00489D40`). First-seen holy-site miss +
`[0x13B8647]==0` → **`ret 0`**, no `006AC910`.

Then `"Init GUI"` `0043A380` and `"Init Quests"` `004B4260`.
Those construct quest / watcher objects, **not**
`CThingAICreature` / `CThingPlayerCreature`.

| Question | Answer | Class |
|---|---|---|
| Frontend creates a creature / Hero? | **No.** 2D UI only | **DISPROVEN** |
| `0049F180` creates a Thing? | **No.** bind + failed `00489D40` | **PROVEN** |
| First *name* bind of Hero def? | `0049F1D7` → `00449D90` | **PROVEN** |
| First *creature Thing* create? | Lookout TNG `0051FD80` **`CREATURE_BS_VILLAGER_MALE` / `FH_Villager` / `"0"`** | **PROVEN** (region walk) |
| First *Hero Thing* create? | later **`006AC910`** at `GuildArrivalHSP` | **PROVEN** |
| First `0051FD80` at all? | Bridge `TRACK_NODE_BASIC` | **PROVEN** (not a creature) |
| `004AE9D0` / Create Players is Hero? | **No.** slots / tick bind | **DISPROVEN** |
| First create is `00DBDE40` / kid / script `Create` | **No.** | **DISPROVEN** |

---

## Timeline (no-save New Game)

```
0042F2A2  Leave frontend
0042F491  Init Game → 004184BD
  Init Thing Components 004EE23F     // CTCHeroMorph names; no Thing
  Init Definition Manager 00416005(1) // game.bin table; no Thing
  Create Players 004166A8            // 5×0x22C slots; not a creature
  Init World 004A6E30
    00522A20  register PlayerCreature / AICreature factories
  00416953  Loading world FinalAlbion.wld
    004A1840
      00507C30  WLD / 0050959F .gtng miss
      004FDBC0  proximity .tng parse           // no 0051FD80
      [world+258]==0 → 004A2C80 0049F180(1)    // PARTIAL take
    [0x13B8648]==0
    00416BCA  0049F180(ecx=world, 0)           // PROVEN site
      "Init Characters"
        00449970 / 00487DC0  miss
        0049F1D7  00449D90
          009AD410 "PLAYER_HERO" → 0044BA90 miss
          00449E0D "CREATURE_HERO"
          0048A070 → 00489D40
            00488B20 holy site miss
            [0x13B8647]==0 → ret 0             // no 006AC910
      "Init GUI" 0043A380 PLAYER_GUI_PC
      "Init Quests" 004B4260([world+172])
        00CB5AD0 quest factories               // not AICreature
    00416BCF  Activate Initial Quests 004B4A10
    004BBC00  ret 4
  0041891D  004AE9D0 tick slots                // not Hero
004189C2  dummy pumps  WorldMap+156=0
  HeroSpawned=false; CurrentRegion=null
later 00501450 → 00500540(1,0,0) LookoutPoint
  006C2170 Loading objects
    ContainsMap[0] BowerstoneBridge
      0051FD80 TRACK_NODE_BASIC                // first Thing, not creature
    ContainsMap[1] LookoutPoint
      Q_FireHeart
        0051FD80 CREATURE_BS_VILLAGER_MALE FH_Villager "0"  ← first creature
        0051FD80 CREATURE_BS_VILLAGER_FEMALE FH_Villager "2"
        0051FD80 CREATURE_BS_VILLAGER_MALE FH_Villager "1"
    ContainsMap[2] GuildExterior
  006AC910 CREATURE_HERO ScriptName=Hero       // first Hero
           pose GuildArrivalHSP
```

`Q_NewOakValeIntro` / `00DBDE40` / kid `4300` are **not**
on this list. **PROVEN**.

---

## 1. `0049F180` is Init Characters, not create

Listing `0049F180`–`0049F25E` (`listing-00480000.txt`):

```
0049F180  sub esp, 48
0049F18D  push "Init Characters"
0049F1B3  mov ecx, [esi+12]          // world+12 player manager
0049F1B6  call 00449970
0049F1BD  call 00487DC0              // player Thing
0049F1C4  je   0049F1CF              // miss → bind
0049F1C6  test [eax+145], 1
0049F1CD  je   0049F1DC              // live Thing, bit0 clear → skip
0049F1D7  call 00449D90
0049F1EA  push "Init GUI"
0049F214  call 0043A380              // [0x13B8790] PLAYER_GUI_PC
0049F21B  push "Init Quests"
0049F247  lea edx, [esi+172]
0049F24E  call 004B4260
0049F259  call 004B2890
```

`00449D90` is the **only** `E8` of that bind (`0049F1D7`).
`0049F180` has **no** `E8 006AC910` and **no** `E8 0051FD80`.

No-save Load World: no player Thing → **always** `00449D90`.
**PROVEN**.

### Callers of `0049F180`

| Site | Arg | Class |
|---|---|---|
| `00416BCA` after `004A1840` when `[0x13B8648]==0` | `push 0` | **PROVEN** first-seen |
| `004A2C80` inside `004A1840` when `[world+258]==0` | `push 1` | **PROVEN** insn; first-seen take **PARTIAL** (`+258` ctor 0, writer **UNREAD**) |

`004A2BEE` reads `[ebp+258]`; `jne 004A2CC3` skips the
`004A2C80` tail. Even if that tail runs, it is the same
function: still no Thing if `00489D40` early-outs.

Host `InitCharactersAndQuests` Notes `0049F180` then
`00449970 / 00487DC0` only — it does **not** Note
`00449D90` here. That gap is **LEFTOVER** vs the listing
(sibling `hero-stats-first`).

---

## 2. `00449D90` / `00489D40` bind, then miss

`00449D90` (`listing-00440000.txt`):

```
0099EBF0 "PLAYER_HERO"
009AD410([esi+8])
0044BA90(def)                 // 009AD9E0 appearance; fail if no Graphic
je 00449E0B
00449E0D  push "CREATURE_HERO"
004498C0
00449E2D  call 0048A070       // both hit and miss
```

TLC `PLAYER_HERO` is type `PLAYER`, **no** Graphic → miss.
`00449E0D` is the fallback immediate, not a function.
Host `InitHeroDefFn = 00449D90` is the right entry.

`0048A070` `CPlayer::InitCharacterAs`: empty `[esi+52]` →
`0048A0AF call 00489D40`. **PROVEN** first-seen.

`00489D40` (`listing-00480000.txt`):

```
00488B20  find holy site
test al
mov al, [0x13B8647]
jne create-body               // 00489D86 …
cmp al, 0
je  ret 0                     // 00489D7B — no 006AC910
…
00489FC1  call 006AC910       // only on the taken create body
```

`006AC910` `E8` sites: **`00489FC1`** (this fn) and
`0089F660` (later leftover). First-seen
`[0x13B866C]` is empty (WLD path is `game+90576`;
`userst.ini` `NOVStartHSP` is a *name store before
frontend*, not a live Lookout Thing). Miss +
`[0x13B8647]==0` → no create. **PROVEN**.

So the first post-Leave *Hero def name* is here.
The first Hero *Thing* is **not**.

---

## 3. First creature Thing is TNG, not `0049F180`

After dummy pumps, `00501450` / `006C2170` Loading objects
walks WLD ContainsMap order.

First `0051FD80` is `BowerstoneBridge.tng`
**`TRACK_NODE_BASIC`**. Not a creature
(`proofs/tng-first-def`).

First `AICreature` is Lookout `Q_FireHeart`:

| Field | Value |
|---|---|
| Def | `CREATURE_BS_VILLAGER_MALE` |
| Script | `FH_Villager` |
| ScriptData | `"0"` |
| UID low | `300555` |
| Factory | `005272E0` → ctor `00831F80` (`CThingAICreature` vtbl `0127293C`, size `0x1D8`) |
| Mesh | **5149** |
| Pose | navigator (124.357, 81.083, 30.100) |

That is **`0051FD80` → `00A371C0` Allocate Class**, not
`0052B880` / `006AC910`. Not quest `00CB5AD0`. Not script
`Create` `00CCC246`. Not `LookoutPointBeggar` (7th Lookout
`AICreature`). **PROVEN** (`proofs/npc-first-create`).

`004FDBC0` global proximity parse is **DISPROVEN** as this
create (no `0051FD80`; host does not `LoadSingleThing`).
Whether native `[manager+128]==1` ever constructs during
that walk stays **PARTIAL**; first-seen `00416392==0` after
Init Game argues the countable list is still empty.

---

## 4. First Hero Thing is `006AC910` after the maps

Lookout TNG has **no** `PlayerCreature` / `CREATURE_HERO`.
Host `SpawnHeroFromPlayerStart` therefore runs **after**
the three ContainsMap loads.

```
HOLY_SITE_PLAYER_START GuildArrivalHSP
  (52.688, 69.597, 36.982)
006AC910  CThingPlayerCreature::Create
  DefinitionType = CREATURE_HERO   // PLAYER_HERO Graphic miss
  ScriptName     = Hero
  factory        = 0052B880 (size 0x208)
  mesh           = 4299
```

Native create body is `00489D40` once a holy site is
found (or `[0x13B8647]!=0`). The *successful* first-seen
create is this region-load retry, not the Load World
`0049F180`. Which later `E8 00489D40` / `0048A070` first
returns 1 is **UNREAD** (`004A2C80` retry vs `0066FF20`
vs region holy-site list). Host folds the create into
`LoadFromFirstRealRegion` after TNG. **MATCH** order vs
native “after maps”; **DIVERGE** if it Notes `0049F180`
at that site.

Kid `CREATURE_HERO_CHILD` / mesh **4300** /
`00DBDE40` are **DISPROVEN** on this tree.

---

## 5. Not these

| Candidate | Why not first creature / Hero create |
|---|---|
| Frontend `009AD410` | UI Type=10 only |
| `004EE23F` `CTCHeroMorph` / `CHeroDef` | type register; no instance |
| `00416005` `game.bin` | table load |
| `004166A8` Create Players | 5 player *objects*, not Things |
| `004AE9D0` | tick slots on `game+80568` |
| `0049F180` / `00449D90` / first `00489D40` | bind; holy-site miss |
| `004B4260` / `00CB5AD0` | quest / watcher objects |
| `004FDBC0` | parse only |
| Bridge `TRACK_NODE_BASIC` | first Thing, not a creature |
| `CREATURE_BEGGAR_01` | later Lookout section |
| Script `Create` `00CCC246` | no `00CBFB7D` after Leave |
| `00DBDE40` / Oakvale kid | leftover intro |

---

## Host leftovers

| Host | Native | Class |
|---|---|---|
| Second `Note(InitCharactersFn)` in `SpawnHeroFromPlayerStart` | `0049F180` is not called from `006AC910` | **LEFTOVER** |
| Test recover-`0051FD80` / recover-`00662880` “Hero via `0049F180` → `006AC910`” | those VAs are two different times | **LEFTOVER** wording |
| `InitCharactersAndQuests` Notes `00449970` / `00487DC0` only | listing also `0049F1D7` `00449D90` | **LEFTOVER** gap |
| `ResolveHeroDefinition` Notes `00449D90` as LevelLoader | that VA already ran in `0049F180` | **LEFTOVER** site |
| `FirstSceneWorld` / `CREATURE_HERO_CHILD` inject | unused by `Fable.Client` | **LEFTOVER** |

`EnterGame` / first `004189C2` do **not** call
`LoadFromFirstRealRegion`. Tests that assert
`HeroSpawned` call it after Leave. **PROVEN**.

---

## Open

- First-seen take of `004A2C80` (`[world+258]==0`) **PARTIAL**.
- First later `00489D40` that hits `006AC910` **UNREAD**.
- Live `0051FD80` miss (`004C9B80`) on first `AICreature` **UNREAD**.
- First-Present submitted creature C3D is still Hero **4299**
  only (villager `Submitted=false` on navigator pose).
  Draw is a sibling (`npc-first-create`, `hero-appearance-first`).
