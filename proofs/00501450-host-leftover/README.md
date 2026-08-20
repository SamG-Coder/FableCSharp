# `00501450` vs `00449D90` — host leftover still present?

Investigation only. No production `src/` or `tests/` edits.

Do **not** start at Oakvale / `00DBDE40` / `CREATURE_HERO_CHILD`
/ Graphic **4300**. First real region on this walk is
**LookoutPoint** (native index 1). Dummy pumps never open it.

Do **not** treat `00501450` as Init Characters.
Do **not** invent a Thing at `0049F180` to fill a Note gap.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: `00501450` vs `00449D90`. Host leftover still
present? First leftover on the shared lifecycle?

Authority: existing proofs
`proofs/00501450-no-00449D90`,
`proofs/host-00501450-timing`,
`proofs/00449D90-player-hero-miss`,
`proofs/0049F180-first-children`,
`proofs/0048A0AF-first-miss`,
`proofs/dummy-pumps-before-region`,
`proofs/first-region-after-leave`;
Fable.exe dump
`listing-00500000.txt` (`00501450`…`005014A3`),
`listing-00480000.txt` (`0049F1D7`),
`listing-00440000.txt` (`00449D90`…`00449E0D`),
`e8.tsv` dests `00449D90` / `00501450`,
`functions.tsv` `0x00501450`;
`EngineLifecycle` `LoadFromFirstRealRegion` /
`EnqueueAfterDummy` / `InitCharactersAndQuests` /
`ApplyLoadJob` / `SpawnHeroFromPlayerStart` /
`ResolveHeroDefinition` / `Pump` / `PumpGame`
(read only).

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| Same function? | **No.** Region enqueue vs `PLAYER_HERO` bind | **PROVEN** |
| `00501450` `E8` `00449D90` on miss? | **No.** `ebx==0` → `je 00501495` → `004FEEC0` | **PROVEN** |
| Any `.text` `E8` of `00449D90` from `00501450`? | **0.** Sole dest is `0049F1D7` | **PROVEN** |
| First `00449D90` after Leave? | `00416BCA` `0049F180(0)` miss | **PROVEN** |
| Host leftover **still** present? | **Yes.** Two sites, live as of this read | **LEFTOVER** |
| First leftover of this pair on the **shared** lifecycle? | **Yes:** `InitCharactersAndQuests` omits `00449D90` at Load World. Not the later `00501450` fold | **PROVEN** |
| `00501450` leftover on that shared walk? | **No.** Shared `Pump` / `PumpGame` never call it | **DISPROVEN** as this leftover; **MATCH** skip |
| Walk-first leftover on all of Init Game? | **No.** Earlier unnamed `0044C6B6` | **DISPROVEN** as walk-first |

**Still leftover.** Same two holes as
`00501450-no-00449D90` / `00449D90-player-hero-miss`.
Nothing in `EngineLifecycle` closed them.

Do **not** re-hook `00501450` onto `Pump`.
Do **not** Note `00449D90` under `LoadFromFirstRealRegion`.
Do **not** create a Hero at `0049F180`.

---

## Contrast (dump; unchanged)

Init Characters **does** call `00449D90` on miss.
`00501450` does **not**. Same two getters; different miss.

```
0049F180  Init Characters                    // first-seen after Leave
  0049F1B6  call 00449970
  0049F1BD  call 00487DC0
  test eax, eax
  je 0049F1CF
  0049F1D7  call 00449D90                    // only e8.tsv dest

00501450  region enqueue                     // later; caller UNREAD
  0050146B  call 00449970
  00501472  call 00487DC0
  cmp ebx, ebp
  je 00501495                                // miss: no 00449D90
  test [ebx+145], 1
  jne 00501495
  call 004C8CF0(1)                           // live Thing only
  00501495  call 004FEEC0(current, 0)
```

`e8.tsv`: dest `0x00449D90` = **one** site `0x0049F1D7`.
Dest `0x00501450` = **none**. `functions.tsv` callee list
for `0x00501450` starts `00449970,00487DC0,004C8CF0,…`
and has **no** `00449D90`.

`00449D90` itself is `"PLAYER_HERO"` → `009AD410` →
`0044BA90` fail → `00449E0D` `"CREATURE_HERO"` →
`0048A070`. First-seen `00489D40` **ret 0**. Not a
create. Not `CREATURE_HERO_CHILD`.

---

## Shared lifecycle (Leave → dummy pumps)

Both host and native walk this without an explicit
`LoadFromFirstRealRegion`:

```
0042F2A2  Leave frontend
0042F491  Init Game → 004184BD
  00416953  Loading world
    00416ABA  004A1840
    [0x13B8648]==0
    00416BCA  0049F180(0)                    // FIRST 00449D90
      00449970 / 00487DC0 miss → 00449D90
      00489D40 holy miss → ret 0
  user.ini Gameflow                          // 0 E8 00501450
004189C2  dummy pumps                        // 0 E8 00501450
```

| Site on shared walk | Native `00449D90`? | Native `00501450`? | Host | Class |
|---|---|---|---|---|
| Leave / `EnterGame` | no | no | skip | **MATCH** |
| Init Game suffix / Gameflow | no | no | skip | **MATCH** |
| `InitCharactersAndQuests` / `00416BCA` | **yes** (`0049F1D7`) | no | Notes `0049F180` + pair only | **LEFTOVER** gap |
| Dummy `Pump` / `PumpGame` | no | no | no `EnqueueAfterDummy` | **MATCH** skip |
| Dummy type-1 / first Present | no | no | skip | **MATCH** skip |

`Pump` / `PumpGame` still never call
`LoadFromFirstRealRegion` / `EnqueueAfterDummy`.
`FirstRealRegionLoadDone` stays false through dummy
pumps. **PROVEN** (`host-00501450-timing`).
`SilkEngineHost` / `Program` / `FirstSceneWorld` still
do not call either method.

So on the **shared** lifecycle the `00501450` leftover
is **not present as a call**. The first leftover of
**this pair** is the Init Characters Note gap.

That gap is **not** the first leftover on the whole
`004184BD` walk (`0044C6B6` is earlier). Scoped to
`00501450` vs `00449D90`, it is first.

---

## Host leftover still present (two sites)

Read of `EngineLifecycle.cs` (no edits):

### 1. Shared-lifecycle gap — first leftover of this pair

`InitCharactersAndQuests` (from `LoadWorld` when
`SkipParticlesFirstSeen==0` — **MATCH** site):

```
Note(InitCharactersFn, … "0049F180 push 0 ecx=world");
Note(PlayerCreatureBindFn, … "00449970 / 00487DC0");
Note(InitGuiFn, … "0043A380 …");
… 004B4260 / 004B2890 …
```

No `Note(InitHeroDefFn)`. No `009AD410 PLAYER_HERO`.
No `00449E0D`. No `0048A070`. No `00489D40`.

Native no-save **always** `0049F1D7`. **LEFTOVER**
gap. Method still does **not** `new ThingInstance` —
that no-create is **MATCH** (`00489D40` `ret 0`).

### 2. Stand-in fold — leftover **when**, not shared

`LoadFromFirstRealRegion` (`00501450`) still notes
the pair and **does not** note `00449D90`. **MATCH**
body.

After each sync `00500540` → `006C2170`,
`ApplyLoadJob` still:

```
if (!HeroSpawned)
    SpawnHeroFromPlayerStart(_regionThings);
```

`SpawnHeroFromPlayerStart`:

```
Note(InitCharactersFn, … "0049F180 Init Characters");
Note(InitHeroDefFn, … "00449D90 PLAYER_HERO then CREATURE_HERO");
Note(CreateCharacterFn, … "00489D40 " + HSP);
SpawnHero → ResolveHeroDefinition → InsertThing
```

`ResolveHeroDefinition` again Notes `009AD410
PLAYER_HERO` and `00449E0D` / `0048A070` as
LevelLoader. TLC always takes the fallback. Identity
**MATCH**; time **LEFTOVER**.

Native `00501450` miss never enters that stack.
`006C2170` / `0051FD80` have **0** `E8` of
`0049F180` / `00449D90` / `00489D40`. First Hero
Thing is a later unread take of `0048A0AF`
(`hero-00489D40-retry`), **not** a `00501450` `E8`.

Create at `GuildArrivalHSP` / adult **4299** after
Lookout ContainsMap is **MATCH** work. The leftover
is folding Init Characters VAs onto that work.

`EnqueueAfterDummy` is still leftover **glue**:
unused by live `Pump`, exists to fire `00501450`
as if dummy pumps were the trigger
(`host-00501450-timing`). Persist arm is
`00487C20`, not no-save.

---

## First leftover — scoped

| Scope | First leftover | Class |
|---|---|---|
| Shared Leave → dummy `Pump` | `InitCharactersAndQuests` omits `00449D90` | **LEFTOVER** gap **PROVEN** |
| Same walk, `00501450` fold | not reached | **DISPROVEN** as first |
| Explicit `LoadFromFirstRealRegion` after dummy | `SpawnHeroFromPlayerStart` Notes `0049F180` / `00449D90` after first `006C2170` | **LEFTOVER** site **PROVEN** |
| That method’s own pair note | no `00449D90` | **MATCH** |
| Whole `004184BD` named/unnamed walk | still `0044C6B6` | **DISPROVEN** as this pair |
| Fill Init Characters gap with a create | would **DIVERGE** (`00489D40` `ret 0`) | **DISPROVEN** |

`Load_single_thing_0051FD80_spawns_hero_at_LookoutPoint`
still `Assert.Contains(… InitHeroDefFn)` after
explicit `LoadFromFirstRealRegion`. That locks the
**stand-in** leftover Note, not a native
`00501450` `E8`.

---

## Not these

| Candidate | Class |
|---|---|
| `00501450` is first-seen `00449D90` | **DISPROVEN** |
| `00501450` miss creates Hero | **DISPROVEN** |
| Live second `Pump` → `EnqueueAfterDummy` | **DISPROVEN** leftover site (API still exists) |
| Host `LoadFromFirstRealRegion` Notes `00449D90` | **DISPROVEN** — body **MATCH** |
| Leftover closed since prior proofs | **DISPROVEN** — both sites still in source |
| `00DBDE40` / kid 4300 as this leftover | **DISPROVEN** |
| Walk-first leftover of Init Game | **DISPROVEN** (`0044C6B6`) |

---

## Classifications (short)

1. **`00501450` vs `00449D90`: different fns; same pair; miss does not `E8` the bind. PROVEN.**
2. **Host leftover still present. LEFTOVER.** Gap at `InitCharactersAndQuests`. Fold at `SpawnHeroFromPlayerStart` after `006C2170`.
3. **First leftover of this pair on the shared lifecycle: the Init Characters Note gap. PROVEN.**
4. **`00501450` leftover is not on the shared dummy walk. DISPROVEN as first. MATCH skip of the enqueue.**
5. **Stand-in body MATCH; stand-in Hero Notes LEFTOVER. DIVERGE when (`UNREAD` native `E8`). Not Oakvale.**

---

## Open

| Item | Class |
|---|---|
| Who transfers control to `00501450` | **UNREAD** (0 `E8`/`E9`/imm/vtbl) |
| Which non-`E8` feeder later hits `00489FC1` | **UNREAD** (`hero-retry-site`) |
| Drop unused `EnqueueAfterDummy` | leftover API; not this pair |
