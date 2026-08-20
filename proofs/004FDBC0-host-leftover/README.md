# `004FDBC0` open vs host — still leftover?

Investigation only. Production `src/` was not edited.

Do **not** start at Oakvale / `00DBDE40` / `StartOakVale`.
No-save New Game is Leave `0042F2A2` → `FinalAlbion.wld` →
Loading world `004A1840` → `00507C30`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: `004FDBC0` **open** vs host `LoadGlobalThingsFile`.
Still leftover? Next proven slice?

Authority: `proofs/004FDBC0-open`, `proofs/004FDBC0-vs-host`
(open / first file / host parse); `proofs/005223F0-plus128-gate`
(gate writer); `proofs/0049E200-thing-count` (`+24` insert);
dump `listing-004c0000.txt` (`004FDBC0` / `004FBF60` /
`004FAFF0`), `listing-00500000.txt` (`00507C30` / `005223F0`
/ `00521AE0` / `00520D00` / `0051E2F0`). Host notes only:
`EngineLifecycle.LoadWorldMap` / `LoadGlobalThingsFile` /
`LoadRegionMapThings` / `LoadSingleThing` / `EnsureLevels`.

Siblings that still say “parse-only MATCH skip” are
**superseded** on construct: `host-tng-construct-early`,
`00416392-after-initgame` (count→gate), `first-0051FD80-file`
(“Lookout constructed in `00507C30` **DISPROVEN**”),
`thing-manager-activate` (no `E8 0051E2F0` on `004FDBC0`),
status “`00521AE0` is per-map TNG, not this apply”.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Open vs host? | **Same site, same first file.** Host `TryLoadThings("LookoutPoint")` **MATCH**es `004FBF60(1)` → `LookoutPoint.tng` inside `00507C30`. 151 prox maps. | **MATCH** (**PROVEN**) |
| Still leftover? | **Yes — construct, not open.** Host parse+store only. Native first-seen `[manager+128]` is ctor **`1`**, so `005223F0` **takes** `00521AE0` / `00520D00` / `0051FD80`. | leftover **PROVEN** dump-static; live RAM **UNREAD** |
| Host constructs here? | **No.** No `LoadSingleThing` / `InsertThing` / `_regionThings`. | **DISPROVEN** |
| vs-host “MATCH skip”? | **No.** Gate writer is known. Skip vs first-seen `1` is **LEFTOVER**, not MATCH. | **DISPROVEN** as MATCH |
| Next proven slice? | **`005223F0` taken-arm stack into `00521AE0` (`ret 36`).** Mode + dest vector. **Not** first-Present insert of 21k. | next **PROVEN**; dest **UNREAD** |

---

## Verdict

**Open is MATCH. Construct skip is still leftover.**

`004FDBC0-open` / `004FDBC0-vs-host` stay the authority for
**file I/O**: first name, filter, walk order, census, timing.
They do **not** stay the authority for **construct**.
`005223F0-plus128-gate` + `0049E200-thing-count` closed the
gate: ctor `00523540` writes `[CThingManager+128]=1` during
Init World; `AllowDataGeneration` skips `004FE030`; nothing
on the first-seen path rewrites the dword before
`004FDBC0`. Host `LoadGlobalThingsFile` is unchanged
(`EngineLifecycle` still concatenates `ThingInstance`s into
`GlobalThings` and never `LoadSingleThing`).

That skip is **LEFTOVER** vs dump-static first-seen `1`.
Live bytes at `005223F7` are still **UNREAD**. Do not
keep “parse-only MATCH” as the working model.

Do **not** treat leftover as “stuff 21746 Things into
`RegionThings` / first Present.” First Present is still
ContainsMap + hero (`006C2170` after `00501450`).

---

## Classification

| Claim | Class |
|---|---|
| Host site is `LoadGlobalThingsFile` (`004FDBC0` arm, `[0x13B8609]=0`) | **PROVEN** |
| First open is `LookoutPoint.tng` (native index 1 / host `Maps[0]`) | **PROVEN** / **MATCH** |
| When: inside `00507C30`, before Set Static Map | **PROVEN** / **MATCH** |
| Filter / order / census 151 / ~21746 | **PROVEN** / **MATCH** |
| Host **opens** the prox set | **PROVEN** (`ThingFile.Parse`) |
| Host **constructs** (`LoadSingleThing` / `InsertThing`) | **DISPROVEN** |
| Native **always** opens (`004FAFF0` / `0099AD80`) | **PROVEN** |
| Native first-seen **enters** `005223F0` taken arm | **PROVEN** dump-static (`+128` leftover `1`) |
| Native live `+128` at first `005223F7` | **UNREAD** |
| Host skip vs that first-seen `1` | **LEFTOVER** |
| `004FDBC0-vs-host` “construct MATCH vs off” | **DISPROVEN** / **LEFTOVER** |
| `00521AE0` is **not** reachable from this open | **DISPROVEN** (`004FC023` → `0052249F`) |
| `004FDBC0` itself `E8 0051E2F0` | **DISPROVEN** (indirect via `005223F0`) |
| First **open** is first **CThing** / first Present | **DISPROVEN** |
| `GlobalThings` leak into host `RegionThings` | **DISPROVEN** |
| `EnsureLevels` WAD + `FinalAlbion_RT.stb` inside this call | **DIVERGE** (I/O leftover; **not** construct) |
| Token walk if `+128!=1` | host always parse; native open+drop | **PARTIAL** (counterfactual) |
| Stream: host WAD `Read` vs native `CreateFileW` | **PARTIAL** |
| Host `GLOBAL` concat vs native store | **PARTIAL** (*use* after parse) |

---

## Open — MATCH (not leftover)

From `004FDBC0-open` / `004FDBC0-vs-host`:

```
00509859  "Load global things"
          [0x13B8609]==0
00509948  call 004FDBC0          ; ecx=CWorldMap, no stack
004FDBDE  ebx = 1                ; skip dummy 0
          if [slot+36] && [slot+40]:
004FDC74    call 004FBF60        ; push ebx
004FAFF0    append 0x12442C4 ".tng"
004FBFED    0099AD80 CreateFileW ; first-seen [map+168]==0
004FC023    call 005223F0        ; shared_ptr + map index
```

Host (`LoadWorldMap` → `LoadGlobalThingsFile`):

```
EnsureLevels();
foreach (var map in World.Maps)            // Maps[0] = NewMap 1
{
    if (!map.LoadedOnPlayerProximity) continue;
    var tng = _levels?.TryLoadThings(map.ScriptName);
    loaded.AddRange(tng.Things);
}
GlobalThings = new ThingFile { Sections = [GLOBAL] };
```

No `LoadSingleThing`. TLC has no loose `LookoutPoint.tng`;
bytes are `FinalAlbion.wad`. First increment
`"LookoutPoint"`, **288** tokens, first `NewThing`
`MARKER_BASIC` `M_Maze`.

| Sense | Host | Native | Class |
|---|---|---|---|
| File I/O | `TryLoadThings("LookoutPoint")` first | `004FBF60(1)` first | **MATCH** |
| Switch | `SingleGlobalThingsFile==false` | `[0x13B8609]==0` | **MATCH** |
| Walk | `Maps[0]…` (no dummy 0) | `ebx=1…` | **MATCH** |
| Count | 151 / ~21746 | same census | **MATCH** |

Open leftover: **none.** Do not re-walk first-file or
start at `Maps[1]` / Picnic / Bridge / Oakvale for this
open.

---

## Construct — still LEFTOVER

`005223F0` (`listing-00500000`):

```
005223F7  mov eax, [esi+128]
005223FF  cmp eax, 1
00522407  jne 00522502            ; drop shared_ptr, ret 12
          …
0052249F  call 00521AE0           ; ret 36
005224AB  call 0051E2F0           ; local vector
0052251F  ret 12
```

Caller is `004FBF60` after the open (`004FC023`).
`ecx` is `[world+80]` (`0049E1B0`).

First-seen writer (`005223F0-plus128-gate`):

```
004A6E30  "Init Thing Manager"
0049EBF0  → 00523540
005235CD  mov [esi+128], 0x1
```

`004FE030` would rewrite inside `00507C30` and is
**skipped** (`[0x1375459]==0`). `00507C30` /
`004FDBC0` therefore **read leftover `1`**. Taken arm
is dump-static first-seen.

Taken arm does **not** stop at tokenize:

```
00521AE0  "Thing Manager: Load From File"
00521C45  call 00520D00           ; NewThing walk
00520F9A  call 0051FD80           ; Load Single Thing
004C9030  → 00A371F0              ; splice [manager+24]
          [thing+145] = 0x04      ; 0051E530 counts it
```

`0049E200-thing-count`: empty `[manager+24]` at
`0041890E` is **DISPROVEN** dump-static. One successful
NewThing is enough. Exact `0051E530` sum **UNREAD**.

Host does none of that on this VA. Same leftover as
`PlayerBindSlot1 = WorldFrame` (drops the walk).

| Sense | Native first-seen | Host | Class |
|---|---|---|---|
| Open | `LookoutPoint.tng` | same | **MATCH** |
| `005223F0` | leftover `1` → taken | no `LoadSingleThing` | **LEFTOVER** |
| Token depth | `00521AE0` taken | always `ThingFile.Parse` | **PARTIAL** (host parses even if gate were 0) |
| `+24` at `00416392` | dump-static **not** empty | host walk never runs | leftover **PROVEN**; live **UNREAD** |

Older “Lookout is **not** constructed in `00507C30`”
(`first-0051FD80-file`) is **LEFTOVER** against the
writer. It remains **PROVEN** that the **later**
ContainsMap first file after dummy pumps is
`BowerstoneBridge.tng`. Do not collapse the two
`0051FD80` sites.

---

## Not leftover (do not grow work here)

| Item | Why |
|---|---|
| First-file / 151 census | already **MATCH** |
| `.gtng` miss / `.gtg` skip | **PROVEN**; not this arm |
| `EnsureLevels` WAD + `_RT.stb` | **DIVERGE** I/O (`lev-first-after-leave`, `stb-first-open`). Real extra during this host call, **not** the construct leftover. |
| Insert 21k into first Present | **DISPROVEN** as first-scene C3Ds. ContainsMap + hero stay the Present set. |
| Call this VA `0051FD80` / first CThing | first **open** ≠ first **region apply** |
| Flip `SingleGlobalThingsFile` | first-seen 0; `.gtg` is **not** no-save |
| Invent `+128=0` so skip MATCH | dump does not take that store |

`GlobalThings` has **no** later consumer in `src/` besides
this write. Host *use* after parse is empty. Native *use*
is the taken `005223F0` arm (list `+24` + local activate
vector), **not** a host `GLOBAL` section.

---

## Next proven slice

**`005223F0` taken arm → `00521AE0` stack.**

`00521AE0` is `ret 36` (nine dwords). `006C2170` is the
`push 3` site (`006C2336`). `005223F0` is **not** that
site. Mode `3` is the only value remapped through
`[manager+128]` (`00521B06`). The enter test is still
`cmp [esi+128], 1`.

What is **UNREAD** on the taken arm (and therefore the
next dump walk):

1. The nine stack dwords `005223F0` leaves for
   `00521AE0` (visible `push` is only the shared_ptr +
   one dword at `0052248C`; the rest is frame /
   `vtbl+104` residue).
2. Mode actually seen at `[esp+84]` after the
   `sub esp, 48` prologue — **not** 3 unless proven.
3. Dest of `00520D00` / `0051FD80` from **this** call:
   manager `+24` (0049E200 dump-static) vs the local
   20-byte vector (`00BFEA0E(20)` + `"indow"`) that
   `0051E2F0` walks then `005224B0` frees.
4. Whether every prox `NewThing` is constructed, or a
   subset / UID / PersonalScript table.

Until (1)–(3) are classified, do **not** implement
host `LoadSingleThing` inside `LoadGlobalThingsFile`.
That would guess dest and would flood first Present if
wired to `_regionThings`.

**Not** the next slice:

- Re-prove Lookout as first **open**.
- `00416392` / `PlayerBindSlot1` (census **after**;
  leftover already named).
- `006C2170` ContainsMap (later, **PROVEN**).
- Oakvale / `00DBDE40` / kid `CREATURE_HERO_CHILD`.

---

## Path (no-save)

```
0041735A  Init World
  00523540  [manager+128]=1          ← FIRST WRITE
00416953  Loading world
  00507C30
    0050959F  .gtng miss
    00509859  Load global things
      004FDBC0                         ← THIS
        LookoutPoint.tng               open  MATCH
          005223F0  leftover 1 → taken   host skip LEFTOVER
            00521AE0 / 00520D00          next slice
        … 150 more prox maps …
    00509982  region graph
  Set Static Map                     AFTER
0041890E  00416392  0051E530(+24)+0
004189C2  dummy pumps
00501450 → 006C2170  ContainsMap     later construct
  BowerstoneBridge.tng               first post-dummy 0051FD80
  LookoutPoint.tng                   reopen
```

---

## UNREAD / PARTIAL

- Live `[manager+128]` at `005223F7`.
- `005223F0` → `00521AE0` mode / dest (next slice).
- Exact `0051E530` sum after 151 files.
- Byte identity of `CreateFileW` vs WAD
  `Find("LookoutPoint.tng")`.
- Any TLC prox miss that would desync 151.

---

## Do not

- Keep `004FDBC0-vs-host` “MATCH skip” as current.
- Report leftover as **open** (Lookout / 151).
- “Fix” host by `LoadSingleThing` here before the
  taken-arm stack is proven.
- Insert 21k `GlobalThings` into the first scene.
- Invent a `+128=0` store to restore the skip model.
- Call `00521AE0` unreachable from `004FDBC0`.
- Start this walk at Oakvale / `00DBDE40`.
- Collapse first **open** (Lookout) with first
  **region** `0051FD80` (Bridge).
