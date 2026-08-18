# `0051E2F0` Thing Manager: Activate Things (after world load)

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` / `CREATURE_HERO`.
Those are later than the first `0051E2F0` walk.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Sources: `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00500000.txt`
(`0051E2F0`, `00521AE0`, `00520D00`, `0051FD80`, `0050959F`);
`listing-006c0000.txt` (`006C2170` @ `006C2470`);
`listing-004c0000.txt` (`+148` phase, ctor `004C9030`);
`docs/runtime/FORWARD_TREE.md` §§7–9;
`docs/status/investigations/2026-08-18-first-scene-things.md`;
Anniversary loose `FinalAlbion/BowerstoneBridge.tng`.

---

## Verdict

**First no-save `0051E2F0` after world load is `006C2470` inside
`006C2170` for LookoutPoint (`00500540(1,0,0)`).**

It is **not** GTNG, **not** `004FDBC0`, **not** dummy index 0,
**not** `0051E5A0` Activate After Loading, **not** the hero.

The argument is the job vector filled by ContainsMap object load
(`00522720` then `00521AE0` / `00520D00` / `0051FD80`).
WLD ContainsMap order is **BowerstoneBridge**, LookoutPoint,
GuildExterior. First successful NewThing on that first map is
**`TRACK_NODE_BASIC` / `GuardTrack`**.

| Claim | Status |
|---|---|
| `0051E2F0` = `"Activate Things"` three-pass walk | **PROVEN** |
| First TLC call after WLD is `006C2470`, not GTNG | **PROVEN** |
| Job is first real region LookoutPoint | **PROVEN** |
| Vector order = first ContainsMap TNG order | **PROVEN** (`00520D00` push) |
| First NewThing = `TRACK_NODE_BASIC` `GuardTrack` | **PROVEN** vs Anniversary TNG; TLC WAD byte-id **PARTIAL** |
| Hero / `GuildArrivalHSP` is this first slot | **DISPROVEN** (spawned after apply) |
| `0051E5A0` is this function | **DISPROVEN** |
| Host `004FCBB0`-before-objects | **DISPROVEN** (already) |

---

## When (after world load)

```
004A1840 / 00507C30  Load .wld
  0050959F  Load GTNG  stem+.gtng
    TLC miss → 00999230 false → jmp 00509857
    00509810 00521AE0 / 00509827 0051E2F0   SKIPPED  PROVEN
  00509859  Load global things
    [0x13B8609]=0 → 004FDBC0 parse only
    no E8 0051E2F0                              PROVEN
dummy 004189C2 index 0
  no 00501450 / no 006C2170                     PROVEN
enqueue (E8 UNREAD) → 00501450
  004FEEC0(0,0)
  00500540(1,0,0)  LookoutPoint                 PROVEN
    006C27A0 / 006C2120 / 006C2710
    006C2170
      Loading topology     (all ContainsMap)
      Loading objects      00522720 then 00521AE0
      00500230 / 0050AF10  +12=0 skip
      006C2470  call 0051E2F0                   ← first
        ecx = [job+12]+24  CThingManager
        arg = lea [esp+32]  ptr vector
      004FD020 Post Load Initialise
      004FCBB0 / 004FCFE0
      005064C0 / 004FC8A0
```

`this` at `006C246D` is `[job+12]+24` (Thing Manager).
The vector at `[esp+32]` is zeroed at `006C22CE` then appended
per map. After objects + skipped nav, `006C2468` pushes that
same vector.

Later `00501450` iterations (`i=2…141`) each apply again.
Those are **not** first.

Other `E8 0051E2F0` sites (`00521CF0+00521E2C`, `005224AB`)
are not the `006C2170` path.

---

## Body (`0051E2F0` ret 4)

Walks `[arg+0, arg+4)` as `CThing*` words.

| pass | string | gate `[thing+148] & 7` | call |
|---|---|---|---|
| 1 | `"Activate Things"` | `== 0` | vtbl+32 |
| 2 | `"Activate Things: OnCreate"` | `== 1` | vtbl+36 |
| 3 | `"Activate Things: Initial Activate"` | `== 2` | vtbl+40 |
| end | `"Activate Things End"` | — | — |

Skip in every pass if `[thing+16]==2` **and** `[[manager+36]+257]!=0`
(player / editor). Pass 3 also **defers** `[thing+16]==2` to after
the loop (`[ebp+145]&1==0` then last vtbl+40).

Ctor `004C9030` clears the low 3 bits (`and +148, 0xF8`).
`004C7160` / `004C9560` / `004C9690` / `004C97B0` bump them.
`0051FD80` already called vtbl+32 (or player +36/+40) per thing.
Whether pass 1 is a no-op is **PARTIAL** (building `0082D6F0`
does **not** bump `+148`; generic `004C9120` tail does not either).

`0051E5A0` is a **different** walk: `[manager+24]` list,
`004C8CF0` / `004AFA60`. Host notes it per-map after
`0051FD80`. Not this apply.

---

## First activated thing

`00520D00` on `"NewThing"`: `0051FD80`; if eax≠0, store into
the job vector (`[vec+4] += 4`). File order.

Lookout ContainsMap[0] = **BowerstoneBridge** (88 NewThings).
`00522720` may prepend manager+160 cache; first-visit after
`004FDBC0` parse-only is **PARTIAL** empty (no second
`00521AE0` apply from global things — already **DISPROVEN**).

Anniversary `BowerstoneBridge.tng` (version 2, 88 things; TLC
census matches 8× `TRACK_NODE_BASIC`):

```
NewThing TrackNode
  DefinitionType "TRACK_NODE_BASIC"
  ScriptName     GuardTrack
  UID            18446741874686299399
  pos            (76.686, 30.849, 17.517)
```

That is vector[0] if `0051FD80` succeeds (TrackNode factory
exists; fail would `004C9B80` and skip the push — **UNREAD**
as a live miss).

**Not** `CREATURE_HERO` / `GuildArrivalHSP`. Hero is created
after the ContainsMap apply (`0049F180` / `006AC910`), after
this `0051E2F0`.

First **vtbl** inside this first `0051E2F0`: pass 1 vtbl+32
on that `GuardTrack` if `+148&7==0`, else pass 2 vtbl+36.
Which pass actually fires is **PARTIAL**.

---

## Host

`EngineLifecycle.ApplyLoadJob` notes `0051E2F0` once after
the objects loop — site match. It does not walk the vector
or call vtbl+32/+36/+40.

`ActivateAfterLoading` (`0051E5A0`) is a leftover pairing
name vs this apply.

---

## Leftover / UNREAD

- Live `0051FD80` miss on first TrackNode.
- TLC WAD vs Anniversary TNG byte identity.
- `00522720` first-visit cache occupancy (`[0x13B86A0].vtbl+36`).
- Exact `+148` after `0051FD80` for `CThingTrackNode`.
- `00501450` E8 caller (already UNREAD in FORWARD_TREE).
