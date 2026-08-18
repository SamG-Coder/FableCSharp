# Native TNG / ThingFile load order after Leave

Investigation only. No production `src` edits.
Do **not** start at Oakvale / `00DBDE40` / `StartOakVale`.
No-save New Game is message **15** → Leave `0042F2A2` → LookoutPoint.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Sources: listings `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`,
`listing-00480000.txt`, `listing-004c0000.txt`, `listing-00500000.txt`,
`listing-006c0000.txt`; `docs/runtime/FORWARD_TREE.md` §§6–10, 15;
`docs/status/investigations/2026-08-18-first-scene-things.md`;
`src/Fable.Formats/Tng/ThingFile.cs`;
`EngineLifecycle.LoadGlobalThingsFile` / `LoadRegionMapThings`;
`LevelLibrary.TryLoadThings`;
`EngineLifecycleTests.InitGame_004184BD_after_00416953_reserves_then_user_ini`,
`Loading_objects_00521AE0_loads_LookoutPoint_tng`,
`Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`.

---

## Verdict (read this first)

After Leave there are **two TNG walks**, not one.

| Walk | When | Maps | Native | Host |
|---|---|---|---|---|
| Global | Inside `00507C30` Load `.wld`, still in `00416953` | every filled slot with `LoadedOnPlayerProximity` (**151**) | `00509859` → `004FDBC0` → `004FBF60` / `004FAFF0` `.tng` | `LoadGlobalThingsFile` |
| Region | Later `00501450` → `006C2170` Lookout | **ContainsMap only** (Bridge, Lookout, Guild) | `00522720` then `00521AE0` | `LoadRegionMapThings` |

`00416392` / `0049E200` / `004AE9D0` run **after** the global walk and
**before** the region walk. They do **not** open TNG files. They snapshot
a count into player bind slots.

First **constructed** CThing from a `NewThing` block is **not** the hero.
Lookout TNG has no `PlayerCreature`. Hero `006AC910` is appended after the
ContainsMap loop at `GuildArrivalHSP`.

First region-file `NewThing` is the first block in
`BowerstoneBridge.tng` (WLD ContainsMap order). Identity of that first
block is **UNREAD** here (need WAD first `NewThing`). Lookout’s file
starts in section `Gameflow` (2 things) then `NULL` (252).

---

## Timeline (no-save New Game)

```
0042F2A2 Leave frontend
0042F491 Init Game → 00418DCA → 004184BD
  Create Players 004166A8          // slots 0–4; NOT TNG; NOT hero_swap_*.tng
  vtbl+32 00416953 Loading world
    004A1840
      00507C30 Load .wld file
        NewMap N → slot N (72-byte). Slot 0 unused.
        EndMap: [slot+36]=1, [slot+40]=LoadedOnPlayerProximity
        0050959F Load GTNG stem+.gtng
          00999230 miss → skip 00521AE0          // TLC: no FinalAlbion.gtng
        00509859 Load global things
          [0x13B8609]==0 (BSS) → 004FDBC0        // not 004FE2A0 .gtg
            for i=1 .. mapCount-1                // skip slot 0
              if [slot+36] && [slot+40]:
                004FBF60 → 004FAFF0 (0x12442C4 ".tng")
                005223F0
                  if [manager+128]==1:
                    00521AE0 → 00520D00 → 0051FD80   // PARTIAL (gate UNREAD)
                    0051E2F0
        00509982 Load region graph
      Set Static Map 00B428E0
      [0x13B8648]==0 → 0049F180 Init Characters  // PLAYER_HERO miss; no hero yet
      Activate Initial Quests 004B4A10
    004BBC00 ret 4
  [0x13B8648]==0 suffix:
    0049BA70(game+90488, 60, 0)
    00416392 → 0049E200 → 0051E530(world+80)+[0x13B89BC]
    004AE9D0(game+80568) +9836/+9840/+9844
    default_user.ini miss / user.ini
004189C2 pumps
  dummy index 0: no 006C2170 objects
  later 00501450 → 00500540(1,0,0) LookoutPoint
    006C2170 Loading topology then Loading objects
      ContainsMap order: BowerstoneBridge, LookoutPoint, GuildExterior
      per map: 00522720 then 00521AE0 (push 3)
        00520D00 NewThing loop → 0051FD80 Load Single Thing
      0051E2F0 Activate After Loading
      no PlayerCreature → 006AC910 CREATURE_HERO at GuildArrivalHSP
```

`Q_NewOakValeIntro` / `00DBDE40` / kid `CREATURE_HERO_CHILD` are
**DISPROVEN** on this tree.

---

## 1. `ThingFile.cs` vs native parser  **PROVEN** tokens / **PARTIAL** apply

Native `00521AE0` / `00520D00` / `0051FD80` walk the same ASCII:

| Token | Native | `ThingFile.Parse` |
|---|---|---|
| `Version` | `00521BC4` then `00520D86` needs ≥2 for sections | MATCH |
| `XXXSectionStart` | `00520D91` | MATCH |
| `NewThing` | `00520F13` / `00521000` | MATCH (`Kind`) |
| `EndThing` / `EndThing;` | `0051FE7F` / `0051FECC` | MATCH (strips `;`) |
| `Start*` / `End*` blocks | physics etc. | MATCH (`Block.key`) |
| `DefinitionType` `ScriptName` `UID` `Player` `CTCPhysicsStandard.Position*` | `0051FD80` / def lookup | MATCH fields |

Host `ThingFile` is a **line parse**. It does not `Allocate Class` /
`004CA010`. Construction is `EngineLifecycle.LoadSingleThing` only on
the **region** walk.

`.gtg` exists on disk (`FinalAlbion.gtg`). BSS `[0x13B8609]=0` so
no-save does **not** take `004FE2A0`. **PROVEN**. Host
`SingleGlobalThingsFile` default false. **MATCH**.

`.gtng` missing skip **PROVEN**. Host `LoadGtngFile` notes miss, `Gtng`
null. **MATCH**.

---

## 2. Global TNG  `004FDBC0`  **PROVEN** walk / **PARTIAL** construct

`00509859`: `[0x13B8609]==0` → `004FDBC0`, else `004FE2A0`.

`004FDBC0` (listing `004FDBC0`):

- Map stride **72** (`0x38E38E39` = 1/72). Same as `EndMap` `ebx+ebx*8` × 8.
- `ebx` starts **1**, `edi=0x48`: **skips slot 0**.
- Slot 0 is unused because `NewMap N` writes index **N** (`EndMap`
  `lea edi,[ebx+1]` resize). WLD `NewMap 1` is LookoutPoint.
  C# `World.Maps[0]` is that map (`Index==1`). **MATCH**.
- Gate: `[slot+36] && [slot+40]`.
  - `+36=1` on successful `EndMap` (filled slot).
  - `+40` = parse flag `[esp+19]` = `LoadedOnPlayerProximity`
    (`0050842B` / `0050843B`). Default `[esp+19]=1` at `00507E76`
    (**TRUE** if token omitted). C# `bool` default is **false**.
    TLC maps write the token. **MATCH** on this WLD; omit-token would
    **DIVERGE**.
- Per hit: `004FBF60` → `004FAFF0` (`.tng` `0x12442C4`) → stream
  (`00A39D80` if `[worldmap+168]`, else `0099AD80`) → vtbl+12 →
  `005223F0`.

`005223F0`: `cmp [esi+128], 1`; only then `00521AE0` + `0051E2F0`.
If `+128!=1`, the stream is opened and dropped. **UNREAD** first-seen
`+128`. Status leftover “`00521AE0` is not the global apply” is
**DISPROVEN** as a call-graph claim (`0052249F call 00521AE0`) and
**UNREAD** as a live New Game taken branch.

Host `LoadGlobalThingsFile`:

- Iterates `World.Maps` with `LoadedOnPlayerProximity` (no dummy 0).
  **MATCH** set (151 / ~21 746 things).
- `LevelLibrary.TryLoadThings` loose then WAD. **MATCH** TLC (no loose
  `Levels\FinalAlbion\LookoutPoint.tng`).
- Concatenates into one `ThingFile` named `GLOBAL`. Does **not** call
  `LoadSingleThing` / `InsertThing`. Does **not** put them in
  `RegionThings`. **DIVERGE** vs a taken `00521AE0` branch; **MATCH**
  vs an untaken `+128` gate plus first-seen `00416392==0`.

Native *use* of that 21k set (PersonalScript walk, UID table, later
region cache) stays **UNREAD** except: they are **DISPROVEN** as the
first-Present C3D set. First Present exist set is ContainsMap TNG +
hero (465).

---

## 3. Region TNG  `006C2170`  **PROVEN**

`006C2170` two passes over 28-byte job records (`0x92492493`):

1. `Loading topology` → vtbl+24 `004FF080` / `00638310`.
2. `Loading objects` if `[record+20]!=0`:
   - `00522720` (cached blobs / `00520D00` / `00521240`)
   - **`push 3` then `00521AE0`** (`006C2368`)

`00521AE0` arg 3 remaps through `[manager+128]` (`00521B06`).

ContainsMap order is WLD order: `BowerstoneBridge`, `LookoutPoint`,
`GuildExterior`. SeesMap / BWD-touch maps are **not** TNG-loaded
(`00B42750` headers only). **PROVEN**.

Host `ApplyLoadJob` → `foreach ContainsMaps LoadRegionMapThings`:
`00522720` note, `00521AE0` note, `ThingFile` parse, `LoadSingleThing`
per instance, `0051E5A0`. **MATCH** order and counts (88+288+88).

`LevelLibrary` caches per region key; global walk and region walk can
parse the same three files twice. Native can too (`004FDBC0` then
`006C2170`). Host extra WAD open is hygiene, not a missing native step.

---

## 4. `00416392` WorldThingCount  **PROVEN**

```
00416392:
  if [ecx+90394] != 0
     && [0x13B8388]+56 == 0
     && [0x13B8388]+57 == 0
    return 1
  ecx = [ecx+36]          // world
  jmp 0049E200
```

First-seen `+90394==0` → always the jump. **PROVEN**.
(Host `FinishInitGameAfterWorld` notes that.)

Callers after Leave:

| Site | Role |
|---|---|
| `0041890E` | Init Game suffix after `00416953` |
| `00416F67` | `00416E78` prefix every time vtbl+24 runs |
| `00416602` / `004172F5` | other game helpers |

Not a loader.

---

## 5. `0049E200`  **PROVEN**

```
0049E200:
  ecx = [ecx+80]          // world+80 thing manager
  eax = 0051E530(ecx)
  return eax + [0x13B89BC]
```

`0051E530`: walk `[manager+24]` list. If `!([thing+145] & 1)`, add
`vtbl+92()`. Empty list → 0.

`[0x13B89BC]` is WorldFrame. Unique `inc` is `004A5E10`. At Init Game
suffix it is still **0**. Host comment “`00416392` first-seen 0”
**PROVEN** as the sum (empty countable list + frame 0).

If global `0051FD80` had filled `world+80` with unflagged things,
this would not be 0. So either global construct did not run, or those
Things are flagged / on another list. **PARTIAL**.

---

## 6. `004AE9D0` player bind  **PROVEN**

```
00418906  push [esi+90428]      // arg3
0041890E  call 00416392
00418913  push eax              // arg2 = count
00418914  push [esi+72]         // arg1
00418917  lea ecx, [esi+80568]
0041891D  call 004AE9D0
```

```
004AE9D0:
  if ![ecx+9826]: ret 12
  [ecx+9836] = arg1   // [game+72]
  [ecx+9840] = arg2   // 00416392
  [ecx+9844] = arg3   // [game+90428]
```

Create Players sets `+9826=1`, so the writes run.

Host `FinishInitGameAfterWorld`:

- `PlayerBindSlot0 = GamePlus72` **MATCH** `+9836`
- `PlayerBindSlot1 = WorldFrame` **MATCH** only because first-seen
  `00416392==0==WorldFrame`. Later pumps: native `+9840` is
  **count+frame**, host `AdvanceGameTicks` still stores `WorldFrame`.
  **DIVERGE** if `0051E530!=0`.
- `PlayerBindSlot2 = 0` **MATCH** first-seen `[game+90428]`.

Not a Thing spawn.

---

## 7. First Thing created  **PROVEN** class / **UNREAD** first UID

Not these:

| Candidate | Why not |
|---|---|
| Create Players `0044A530` | 5× `0x22C` player slots + `00522A20` type registrar |
| `hero_swap_*.tng` `0044A3B0` | **DISPROVEN** (no-save) |
| GTNG `00521AE0` at `00509810` | **DISPROVEN** (file miss jumps to `00509857`) |
| Hero `CREATURE_HERO` | After all three ContainsMap files; no `PlayerCreature` in Lookout TNG |
| Oakvale kid 4300 | **DISPROVEN** |

**First TNG `0051FD80` on the region walk** (the path host actually
constructs): first `NewThing` in **`BowerstoneBridge.tng`**, then the
rest of 88, then Lookout 288, then Guild 88, then hero.

**If** global `005223F0` takes `+128==1`, first `0051FD80` is earlier:
first `NewThing` in **`LookoutPoint.tng`** (NewMap 1, first prox slot)
during `00416953`, before `00416392`. Lookout file section order is
`Gameflow` (2) then `NULL` (252). First two would be Gameflow-section
Things. That branch vs first-seen count 0 is **UNREAD**.

Hero after region TNG:

```
HOLY_SITE_PLAYER_START GuildArrivalHSP
  (52.688, 69.597, 36.982)
0049F180 / 00449D90 PLAYER_HERO miss
CREATURE_HERO / 0048A070 / 006AC910
mesh 4299
```

Host appends `Hero` to `RegionThings` and Lookout’s map list.
**PROVEN**. `LookoutPointHSP` / `MAIN_START_POSITION` exist as markers
and are **not** the no-save spawn.

---

## 8. Host vs native ledger

| Claim | Status |
|---|---|
| Two TNG walks after Leave: WLD global then region ContainsMap | **PROVEN** |
| Global = all `LoadedOnPlayerProximity`; region = ContainsMap | **PROVEN** |
| TLC no-save uses per-map `.tng`, not `.gtg` / `.gtng` | **PROVEN** |
| `ThingFile` tokens match `00521AE0` / `00520D00` | **PROVEN** |
| Host global walk constructs CThings | **DISPROVEN** (parse-only concat) |
| Native global walk always constructs | **UNREAD** (`[manager+128]==1`) |
| Host region walk constructs + inserts | **PROVEN** |
| `00416392`/`0049E200`/`004AE9D0` load TNG | **DISPROVEN** |
| Those three run after global, before region | **PROVEN** |
| First-seen `00416392` is 0 | **PROVEN** |
| First Thing is hero / Oakvale kid | **DISPROVEN** |
| First region `NewThing` file is `BowerstoneBridge.tng` | **PROVEN** |
| First `NewThing` def/UID | **UNREAD** |
| SeesMap TNG at New Game | **DISPROVEN** |

---

## Do not

- Treat `LoadGlobalThingsFile` as `0051FD80`.
- Treat `00416392` as WorldThing *create*.
- Load Picnic / Greatwood / filler TNG on first Lookout job.
- Spawn `CREATURE_HERO_CHILD` or bind `StartOakVale` on this path.
- Skip `BowerstoneBridge` because the camera is on Lookout.
