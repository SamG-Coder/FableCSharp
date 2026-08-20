# Remaining readers of `[0x13B866C]` after `00488B20`

Investigation only. No production `src/` / `tests/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`CREATURE_HERO_CHILD` / `NOVStartHSP` spawn.
`userst.ini` `SetStartingHolySite("NOVStartHSP")`
stores this CString **before** frontend. Live
Lookout pose is **`GuildArrivalHSP`**, not
`NOVStartHSP`. Host already pins
`StartingHolySiteFindFn=00488B20`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Question: remaining unread readers of BSS
`[0x13B866C]` after `00488B20`. Childhood vs
Lookout pose. Host gap.

Authority: sibling
`proofs/13B866C-holy-site-readers`;
`proofs/13B8A54-first-reader` (closed-set
method); `assembly/exe/01-sections/text-map/abs.tsv`
(`0x013B866C`, 6 rows);
`disp.tsv` (0 rows);
`listing-00400000.txt` (`00413840`,
`00413D55`–`00413D8F`, `00416A39`–
`00416A66`);
`listing-00480000.txt` (`00488B20`–
`00488D0B`, `00489D40`, `0048A070`,
`0048BC70`);
`listing-01200000.txt` (`01219FE0`,
`01228980`);
`listing-00d80000.txt` (`00DAC295`,
`00DBDE40`);
`e8.tsv` dest `00488B20` /
`00489D40` / `00DBDE40`;
`00-index/strings.tsv` (`SetStartingHolySite`
`0x0122E8A0`; no `NOVStartHSP`);
host `EngineLifecycle.StartingHolySiteFindFn`
/ `SpawnHeroFromPlayerStart` /
`RegionTravel.FindPlayerStart`;
siblings `proofs/hero-00489D40-retry`,
`proofs/0048A0AF-first-miss`,
`proofs/script-setnewstart`.

---

## Verdict

**No remaining unread gameplay readers.**

`abs.tsv` is a closed set of **six**
`0x013B866C` immediates. Two are the
already-proven reads inside **`00488B20`**.
The other four are stores / CRT. After
that function there is **no** leftover
`.text` load of the slot.

| Question | Answer | Class |
|---|---|---|
| Remaining gameplay readers after `00488B20`? | **none** | **PROVEN** empty |
| Remaining listing sites with VA `> 00488B20`? | `01219FE5` ctor, `01228980` atexit | **PROVEN** sites; **DISPROVEN** as gameplay reads |
| Is `00488BFB` a remaining unread reader? | **No.** Same fn, miss-warning arm of `00488B20` | **MATCH** sibling |
| Later re-entry after Lookout maps? | Same two VAs (`00488B68` / `00488BFB`) unless `[CPlayer+244]!=0` skips both | **PROVEN** listing; live feeder **UNREAD** |
| Childhood pose from this slot? | **No.** `NOVStartHSP` is the stored *name*. `00DBDE40` / `CREATURE_HERO_CHILD` do not read it | **DISPROVEN** |
| Live Lookout pose? | `HOLY_SITE_PLAYER_START` **`GuildArrivalHSP`** `(52.688, 69.597, 36.982)` | **PROVEN** |
| Host `StartingHolySiteFindFn=00488B20`? | constant + test **MATCH**. Spawn selector does not Note / emulate the name walk | **MATCH** VA. **DIVERGE** selector |

**Answer:** remaining unread readers of
`[0x13B866C]` after `00488B20` = **empty**.
Childhood is leftover Oakvale. Lookout pose
is `GuildArrivalHSP`. Host gap is the
selector, not a missing reader.

---

## Direct answers

| Item | Value |
|---|---|
| `disp.tsv` `013B866C` | **0** rows |
| `abs.tsv` `013B866C` | **6** (closed) |
| Gameplay readers | **`00488B20`** @ `00488B68`, `00488BFB` only |
| Remaining unread readers after that fn | **none** |
| Remaining VA-after sites | `01219FE5` store, `01228980` dtor |
| `e8.tsv` dest `00488B20` | **`00489D65` only** |
| Childhood reader? | **no** |
| Live pose | **`GuildArrivalHSP`**, not `NOVStartHSP` |
| Host find-fn constant | **`0x00488B20`** **MATCH** |

Overall: **PROVEN** empty remaining set.

---

## 1. Closed listing — remaining after `00488B20`

Grep `013B866C` / opcode `B9 6C 86 3B 01`
(`mov ecx, 0x13B866C`) across `text-map`:

| File | Hits |
|---|---|
| `disp.tsv` | **0** |
| `abs.tsv` | **6** |
| `listing-00400000.txt` | `00413861`, `00416A55` |
| `listing-00480000.txt` | `00488B68`, `00488BFB` |
| `listing-01200000.txt` | `01219FE5`, `01228980` |
| other `listing-*.txt` | **0** |
| `00-index/xrefs.tsv` | **0** (BSS not indexed) |

`abs.tsv` rows (site / imm / `fn=` heuristic):

| Site | `fn=` column | Kind | After `00488B20`? |
|---|---|---|---|
| `00413861` | `0x00413590` | **store** `0099B7D0` | no |
| `00416A55` | `0x00416953` | **store** `0099B7D0` | no |
| `00488B68` | `0x00488AB0` | **read** `0099B2C0` | **inside** finder |
| `00488BFB` | `0x00488AB0` | **read** `0099AF10` | **inside** finder |
| `01219FE5` | `0x00F3FAA0` | **store** `0099B6B0` | VA yes; process ctor |
| `01228980` | `0x0121F86F` | **dtor** `jmp 0099B510` | VA yes; atexit |

`fn=` on the `00488B*` rows is the previous
function (`00488AAC ret` / `int3` then
`00488AB0`, then `00488B1C ret` / `int3`
then **`00488B20`**). Live reader is
`00488B20`. **PROVEN** (sibling).

No `lea` / `push` / other-reg encoding of
`0x13B866C`. `00494BA9` pushes
**`0x13B8668`** (`SetLevel` slot), not
`+4`. **DISPROVEN** as a hidden `+866C`
reader.

---

## 2. The two reads are not remaining unread

Both loads live in `00488B20` (`ret 4`).
Only `.text` `E8` of that fn is
`00489D65` inside `00489D40`. Only `E8`
of `00489D40` is `0048A0AF`.

```
00488B20  sub esp, 36
          mov esi, ecx                    // CPlayer
00488B50  call 0048D5C0                   // collect holy-site Things
00488B55  mov al, [esi+244]
00488B5D  jne 00488C53                    // skip name; nearest vs +232
00488B68  mov ecx, 0x13B866C
00488B6D  call 0099B2C0                   // READ: wchar copy
          mov [esp+32], 0x48BC70          // pred Thing+116 == name
00488BC0  call 0048CC60
          je  00488BF2                    // miss
          … al=1
00488BFB  mov ecx, 0x13B866C
00488C00  call 0099AF10                   // READ: wchar* for warning
00488C0A  push "*** WARNING : failed to find a holy site with ScriptName %S"
          call 0099F1F0
          zero [esi+232/+236/+240]
          … al=0
00488C53  … nearest-site walk; no 0x13B866C …
00488CEE  ret 4                           // hit
00488D0B  ret 4                           // miss
```

`0048BC70`: `00BFEBA8([thing+116], search)`.
Exact ScriptName match. **PROVEN**.

`[CPlayer+244]!=0` jumps to `00488C53` and
**does not** load `0x13B866C`. That skip is
not a remaining reader; it is the finder
**not** reading the slot. **PROVEN**.

A later take of `0048A0AF` after
`006C2170` ContainsMap still hits the
**same** two VAs, or skips them via `+244`.
That is re-entry of `00488B20`, not a new
reader. Outer non-`E8` feeder of that take
is **UNREAD** (`proofs/0048A0AF-first-miss`)
and is **not** an `[0x13B866C]` operand.

---

## 3. Remaining VA-after sites (not gameplay reads)

### 3.1 CRT ctor — `01219FE5`

```
01219FE0  push 0x122E8C0          // UTF-16 MAIN_START_POSITION
01219FE5  mov  ecx, 0x13B866C
01219FEA  call 0099B6B0           // CString ctor
01219FEF  push 0x1228980          // atexit 01228980
01219FF4  call 004012BC
```

Process start, **before** Parse Command
Line / `userst` / Leave. **PROVEN** store.
**DISPROVEN** as a remaining reader after
`00488B20`.

### 3.2 CRT dtor — `01228980`

```
01228980  mov ecx, 0x13B866C
01228985  jmp 0099B510            // CString ref-count dtor
```

Sibling stubs: `01228970` → `[0x13B8668]`,
`01228990` → `[0x13B8670]`. Process
teardown only. **PROVEN** leftover.
**DISPROVEN** as New Game / Lookout.

### 3.3 Stores before the finder (not remaining)

`00413840` (`SetStartingHolySite`,
`[esi+20]=0x413840` at `00413D8F`) and
`00416A55` (`game+90580` if length `>0`)
**write** the CString. First-seen no-save
`+90580` is empty → `jle 00416AB3`.
**PROVEN** stores. **DISPROVEN** as
readers. Rewrite of `+866C` after Leave
is still **UNREAD** (writer, not reader).

---

## 4. Childhood vs Lookout pose

| Name / path | Role | Reads `[0x13B866C]`? | First-seen pose? |
|---|---|---|---|
| CRT `MAIN_START_POSITION` | ctor default | store | overwritten if `userst` ran |
| `NOVStartHSP` | `userst` → `00413840` | store only | **miss** at first `00488B20` (not a live Thing) |
| `00DBDE40` / `CREATURE_HERO_CHILD` / 4300 | Oakvale intro leftover | **no** operand | **DISPROVEN** as this slot |
| QST `AddTestQuest(..., "NOVStartHSP")` | `world+196` record `+4` | **no** | leftover intro token |
| Lookout `GuildArrivalHSP` | `HOLY_SITE_PLAYER_START` | **not** the search string after `userst` | **yes** `(52.688, 69.597, 36.982)` |
| `LookoutPointHSP` / `MAIN_START_POSITION` | same TNG | no | markers only |
| `RegionTravel.FindPlayerStart` ranks `NOVStartHSP` first | host leftover picker | n/a | **LEFTOVER** vs live HSP |

`00DBDE40` (`listing-00d80000.txt`):

```
00DBDE40  push "StartOakVale"
          … 00CB7940 hero-exists …
00DBDF08  push "CREATURE_HERO_CHILD"
```

Only `.text` `E8` is `00DAC295`
(`Q_NewOakValeIntro` slot 2). **No**
`mov ecx, 0x13B866C`. **DISPROVEN** as a
remaining reader. Childhood pose is a
**different** fiber.

First `00488B20` is Load World
`0049F180` → `0048A0AF` → `00489D65`,
**before** `00501450` / `006C2170`.
Candidate list empty → miss even if the
stored name were `GuildArrivalHSP`.
After maps, Lookout has three
`HOLY_SITE_PLAYER_START`. Native name
walk still searches `[0x13B866C]`
(`NOVStartHSP` if `userst` ran) unless
`+244!=0`. Live create pose the host and
first-scene dump consume is
**`GuildArrivalHSP`**, adult
`CREATURE_HERO` / mesh **4299**.
**PROVEN** pose. Name-walk hit
**PARTIAL** (`NOVStartHSP` still in the
slot unless `+244` / rewrite /
`[0x13B8647]`). Those writers are
**UNREAD** and are **not** remaining
readers of `+866C`.

---

## 5. Host gap — Evidence → Original → Host → Gap

| Evidence | Original | Host | Gap |
|---|---|---|---|
| `abs.tsv` / listings: only `00488B20` reads the slot | finder `00488B20`; reads `00488B68` / `00488BFB` | `StartingHolySiteFindFn=0x00488B20`, `StartingHolySiteReadName=0x00488B68` | **MATCH** constants. No remaining reader to pin |
| `e8.tsv` dest `00488B20` = `00489D65` only | `00489D40` CreateCharacter | `CreateCharacterFn=0x00489D40`; spawn Notes that VA | **MATCH** caller. Finder itself is **not** Noted |
| First take misses; live TNG is `GuildArrivalHSP` | `StartingHolySiteIsNovStartOnNoSave=false` | same bool; spawn copies GuildArrival XYZ | **MATCH** pose / flag |
| `00488B20` matches `[0x13B866C]` / `CPlayer+244` | ScriptName walk, not a hardcoded HSP | `SpawnHeroFromPlayerStart` `FirstOrDefault(GuildArrivalHsp)` | **DIVERGE** selector. Host never Notes `StartingHolySiteFindFn` |
| `00416A39` copies `game+90576` → `[0x13B8668]`; `+866C` is holy-site | WLD file is `WorldPathGlobalVa` | `WorldPathAltGlobalVa=0x013B866C` still named as WLD alt | **LEFTOVER** name |
| `00DBDE40` childhood `CREATURE_HERO_CHILD` | leftover intro; no `+866C` | `RegionTravel.FindPlayerStart` ranks `NOVStartHSP` first | **LEFTOVER** picker. **DISPROVEN** as this reader |
| `userst` `SetStartingHolySite` | `00413840` before frontend | `SetStartingHolySiteFn=0x00413840` | **MATCH** store. **DISPROVEN** as Lookout pose |

`SpawnHeroFromPlayerStart` Notes
`0049F180` / `00449D90` / `00489D40` after
ContainsMap. Those VAs already ran (and
missed) in Load World. **LEFTOVER** site.
**MATCH** order / def / HSP vs the later
create. Folding a second `00488B20`
name-walk of `NOVStartHSP` into that host
site would **DIVERGE** unless `+244` /
rewrite is proven.

Inventing a remaining `[0x13B866C]`
reader (GetStartingHolySite, `00DBDE40`,
`FindPlayerStart`, `world+196`) would
**DIVERGE**. The closed listing forbids it.

---

## Classifications (short)

1. **Remaining unread gameplay readers after
   `00488B20` — PROVEN empty.** Closed
   `abs.tsv` / listing set. `00488B68` /
   `00488BFB` are the finder, not leftovers.
2. **VA-after sites — PROVEN ctor / atexit.
   DISPROVEN as New Game reads.**
3. **Childhood pose from this slot —
   DISPROVEN.** `NOVStartHSP` is the stored
   name. `00DBDE40` / kid **4300** do not
   load `0x13B866C`.
4. **Lookout pose — PROVEN
   `GuildArrivalHSP`**, not `NOVStartHSP`.
   Native name-walk hit after maps
   **PARTIAL** (`+244` / rewrite **UNREAD**).
5. **Host `StartingHolySiteFindFn=00488B20`
   — MATCH** constant. Selector still
   **DIVERGE**s (hardcoded `GuildArrivalHSP`).
   `WorldPathAltGlobalVa` / `FindPlayerStart`
   **LEFTOVER**.

## Do not

- Invent a seventh `0x13B866C` immediate.
- Treat `01228980` / `00416A55` / `00413840`
  as remaining readers.
- Spawn `CREATURE_HERO_CHILD` / 4300 from
  this slot.
- Rank `NOVStartHSP` as the no-save Lookout
  pose.
- Note `00488B20` as a child of `006C2170`.
- Collapse leftover #4 (Lookout Present vs
  Oakvale intro view) into this gap.

Open (not remaining *readers*): first-seen
writer of `CPlayer+244` / `[0x13B8647]` /
rewrite of `[0x13B866C]` after Leave; the
non-`E8` feeder that re-enters `0048A0AF`
after ContainsMap.
