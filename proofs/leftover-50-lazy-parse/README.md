# Leftover #50: lazy parse / current+adj vs native `004FDBC0` width

Investigation only. Production `src/` and `tests/` were
not edited. Tests still lock `GlobalThingMapsLoaded == 1`
(`New_Game_004FDBC0_opens_LookoutPoint_only`). That host
count is **not** a recovered native bound.

Do **not** parse every `LoadedOnPlayerProximity` `.tng`
(host OOM). Census 151 / ~21746 is already locked in
`proofs/004FDBC0-vs-host`. This note uses dump
`004FDBC0` / `004FBF60` / `005223F0`, WLD `NewMap`
tokens, and host `LoadGlobalThingsFile` / later
`006C2170` / `00B41E50` only.

Do **not** fold leftover **#4** (Lookout first *rendered*
scene vs Oakvale intro *view*) into this leftover. #50
is the **global TNG pump** (`004FDBC0` / host
`LoadGlobalThingsFile` first-prox `break`).

Do **not** invent persist `PlayerRegionName` to pick a
“current” map at this VA. First-seen no-save region
index is dummy **0**. Who writes `PlayerRegionName` on
New Game stays **UNREAD**.

Do **not** start at Oakvale / `00DBDE40` / `StartOakVale`.
No-save New Game is Leave `0042F2A2` → `FinalAlbion.wld`
→ Loading world `004A1840` → `00507C30` → `00509859`
→ `004FDBC0`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: is there a recovered native skip besides the
host OOM `break`? Smallest MATCH host change that would
open **more than 1** map without OOM (lazy parse / only
current+adj)? If that would invent a skip, leave open.

Authority: `proofs/leftover-50-native-ebx` (loop bound
`ebx=1..count-1`), `listing-004c0000.txt` (`004FDBC0` /
`004FBF60`), `listing-00500000.txt` (`00509859` /
`005223F0` / `006C2170`), TLC WLD tokens,
`EngineLifecycle.LoadGlobalThingsFile` /
`LoadRegionMapThings` / `WorldGeometry.StaticMapsAround`
(read only).
Siblings: `proofs/leftover-50-004FDBC0`,
`proofs/leftover-50-tng-ebx`,
`proofs/leftover-50-tng-oom`,
`proofs/004FDBC0-open`, `proofs/004FDBC0-vs-host`,
`proofs/004FDBC0-host-leftover`,
`proofs/wld-map-index-0`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Recovered native skip **besides** host OOM `break`? | **No map skip at this VA.** Recovered filters are dummy slot 0, unfilled `[+36]`, non-prox `[+40]`. Host already MATCHES those. Extra `break` after first prox is **not** recovered. | **PROVEN** (`leftover-50-native-ebx`) |
| Native loads only Lookout / current / adj? | **No.** `ebx=1..count-1`, every filled+prox. TLC **`1..398` / 151 opens**. | **DISPROVEN** as native skip |
| Lazy parse of all 151 at this VA? | Native **always** `CreateFileW` / WAD open (`004FBF60`) **now**, during Loading world. Deferring parse invents a **timing** skip. Host has **no** later `GlobalThings` consumer, so demand-parse **never fires** → same as skip. | **DISPROVEN** as MATCH |
| Only current+adj at this VA? | **Invented skip.** `004FDBC0` does not read `WorldMap+156`, ContainsMap, SeesMap, or BWD. Current is dummy **0**. ContainsMap TNG is later `006C2170` (3 files). `00B41E50` is neighbour **STB**, not this pump. | **DISPROVEN** as `004FDBC0` |
| Smallest MATCH host change to open >1 without OOM? | **None recovered.** Any subset (Lookout+Picnic, ContainsMap 3, StaticMapsAround, “current” via `PlayerRegionName`) invents a skip. Full `ThingFile.Parse` of 151 OOMs. Tests must keep `GlobalThingMapsLoaded == 1` until a recovered bound exists. | **LEFTOVER** — leave **#50** open |

**Native skip besides host OOM: none at this VA.**
**Lazy parse / current+adj: invent skip. Do not ship.**
Leave leftover **#50** open.

---

## Verdict

**Open first file is MATCH. Pump width is DIVERGE.
No recovered narrower bound. Do not implement a subset.**

`leftover-50-native-ebx` already locked: native
`004FDBC0` starts `ebx=1`, visits **`1..count-1`**
(TLC **`1..398`**), and `004FBF60`s every filled +
`LoadedOnPlayerProximity` slot. There is **no** `break`
after Lookout. Host `LoadGlobalThingsFile` `break`s
after the first prox because `ThingFile.Parse` of 151
files / ~21746 things OOMs the New Game pump. Tests
lock count **1** + Lookout Note, leftover-break Note,
no Bowerstone Note.

That host cut is leftover **#50**, not a recovered
native filter. “Lazy parse” and “only current+adj”
were checked as the smallest changes that might open
**more than 1** file without OOM. Both **invent a skip**
that the dump does not take at this VA. Leave #50 open.
Do not fold into #4. Do not invent `PlayerRegionName`.

---

## Recovered native filters (not the OOM `break`)

`004FDBC0` (`listing-004c0000.txt`):

```
004FDBDE  mov ebx, 1
          count = ([esi+36]−[esi+32])/72
          jbe 004FDCA3                  ; count<=1: dummy only
004FDBF2  mov edi, 0x48
loop:
004FDC60  test [begin+edi+36]           ; filled
          je 004FDC79
004FDC6A  test [begin+edi+40]           ; LoadedOnPlayerProximity
          je 004FDC79
004FDC71  push ebx
004FDC74  call 004FBF60                 ; ALWAYS open this slot
004FDC90  inc ebx
004FDC93  add edi, 72
004FDC96  cmp ebx, eax                  ; recount
004FDC9C  jb 004FDC00                   ; ebx < count
```

| Filter | Native | Host today | Class |
|---|---|---|---|
| Dummy slot 0 | never pushed (`ebx` starts 1) | C# `Maps` has no dummy | **MATCH** |
| Unfilled `[+36]==0` | skip `004FBF60` | WLD `EndMap` rows only | **MATCH** |
| Non-prox `[+40]==0` | skip `004FBF60` | `if (!LoadedOnPlayerProximity) continue` | **MATCH** |
| First prox only | **no such test** | **`break` / parse 1** | **DIVERGE** leftover **#50** |
| Current region / adj | **no read** of `+156` / ContainsMap / BWD | not applied here | **no recovered skip** |
| Oakvale / intro | **opens** `ebx=203` | not opened | **DIVERGE** width; **not** #4 Present |

`004FBF60` **always** builds `script+".tng"` and opens
(`0099AD80` / `00A39D80`) before `005223F0`. The
`[manager+128]==1` gate is **construct** (`00521AE0` /
`0051FD80`), leftover `004FDBC0-host-leftover`, live
`+128` **UNREAD**. Do **not** collapse that gate into
#50. Open happens even if construct drops.

Native *use* of the opened set is **UNREAD**. It is
**DISPROVEN** as first-Present C3Ds (`006C2170`
ContainsMap + hero). UNREAD use is **not** a recovered
permission to skip the open.

---

## Candidate: only current+adj — invents skip

At `004FDBC0` time, Loading world has **not** applied a
real region. Dummy `WorldMap+156=0`. There is **no**
current map for “current+adj” without inventing a write
(`PlayerRegionName` / `00501450`). Do **not** invent
that write.

Even if one **assumed** Lookout (leftover **#4** first
*rendered* region, a **different** ledger):

| Walk | Maps that get `.tng` | Native `004FDBC0`? |
|---|---|---|
| Host today | **1** Lookout | first file **MATCH**; width **DIVERGE** |
| ContainsMap `006C2170` | **3** Bridge / Lookout / Guild | later construct VA, **not** this pump |
| `StaticMapsAround` `00B42750` / `00B41E50` | **14** headers; TNG **not** the neighbour walk | STB, **not** `004FBF60` |
| Lookout AABB + Picnic / Greatwood / Fisherman TNG | small subset; Picnic TNG is **not** `006C2170` | still skips `HeroGuildComplex` (1110), `StartOakValeWest` (`ebx=203`), ~140 others |
| Native this VA | **151** filled+prox in `1..398` | **PROVEN** |

`StartOakValeWest` is WLD `NewMap 203` / prox **TRUE**.
Native **does** `004FBF60(203)` inside `004FDBC0`,
before first Present. Restricting to Lookout adj
**skips** that open. That is an invented skip, and it
is **not** leftover #4 (Present still is not Oakvale).

`HeroGuildComplex` is prox TRUE, **not** Lookout
ContainsMap, 1110 things in the census dump. Native
opens it on this pump. Current+adj from Lookout would
skip it.

Shipping current+adj as `LoadGlobalThingsFile` would
also fold #50 into #4’s region set. **Do not.**

---

## Candidate: lazy parse — invents skip

Two senses. Neither is MATCH at this VA.

### 1. Record names, `ThingFile.Parse` on first use

Host `GlobalThings` has **no** later consumer in
`EngineLifecycle` besides this write. Native *use*
after `004FDBC0` is **UNREAD**. Demand-parse therefore
**never runs** for the extra 150 files. Same as the
OOM `break`. **DISPROVEN** as MATCH.

Wiring demand-parse to `LoadRegionMapThings` /
`006C2170` is the **ContainsMap** walk (3 files), not
`004FDBC0`. That invents a map skip of the other 148.

### 2. Open all 151 streams now, defer token parse

Native `004FBF60` **does** open now. Token walk is
gated (`005223F0`). Host `TryLoadThings` **is**
`ThingFile.Parse` (full managed tree). A host
open-and-drop of 151 WAD entries without concat would
approximate dump-static **skip** of `00521AE0`
(`+128!=1`), which is **UNREAD** live and **DISPROVEN**
as MATCH vs dump-static leftover `1`. It would still
need a recovered bound to set
`GlobalThingMapsLoaded` to 151. Tests lock **1**.

Sequential parse-and-discard of 151 (no `GLOBAL`
concat) might not OOM, but:

- native does not discard the open;
- store/construct is a **different** leftover;
- counting 151 would break `GlobalThingMapsLoaded==1`;
- keeping count 1 while walking 151 is still a host
  lie, not a recovered bound.

Do not ship that as a NewMap-1 lock or as “lazy MATCH.”

---

## Why host OOMs / why 1 file stays

Host `ThingFile.Parse` materializes every `NewThing`
into `ThingInstance`s and concatenates into one
`GLOBAL` section. Census ~21746. Native CRT stream +
gated construct does not keep that managed list.
Parsing every prox file on the New Game pump **OOMs**.
That is the leftover reason **PROVEN** as host
engineering, **not** as native width.

`New_Game_004FDBC0_opens_LookoutPoint_only` locks:

- `GlobalThingMapsLoaded == 1`
- PerMap Note contains `LookoutPoint`
- MapFile Note contains `004FDC00 leftover host break`
- PerMap Note does **not** contain `Bowerstone`

Until a recovered native bound exists (it does not:
bound is **151 opens / `ebx=1..398`**, which OOMs
host Parse), those tests **must** stay at count **1**.
Noting extra maps on `LoadGlobalThingsPerMap` would
fail the Bowerstone assert without recovering native.

---

## Smallest MATCH change?

| Change | Opens >1? | OOM? | Invent skip? | MATCH `004FDBC0`? |
|---|---|---|---|---|
| Parse all 151 into `GLOBAL` | yes | **yes** | no | width MATCH; **not shippable** |
| Keep `break` (today) | no | no | **yes** (host) | first name MATCH; width **DIVERGE** |
| Lookout + Picnic | yes | likely no | **yes** (149) | **DISPROVEN** |
| ContainsMap 3 | yes | no | **yes**; wrong VA (`006C2170`) | **DISPROVEN** |
| StaticMapsAround TNG subset | yes | no | **yes**; STB walk | **DISPROVEN** |
| Current+adj via `PlayerRegionName` | yes | no | **yes** + invents persist write | **DISPROVEN** |
| Lazy names / parse on use | no (no consumer) | no | **yes** | **DISPROVEN** |
| Open-and-drop 151, parse 1 | Notes maybe | no | timing/store skip; count still 1 | **PARTIAL** I/O; **not** recovered bound |

**No smallest MATCH host change remains** that opens
more than one map without OOM without inventing a skip.
Leave leftover **#50** open. Prefer this proof over
`src/` edits.

---

## Not leftover #4 / not `PlayerRegionName`

| Claim | Class |
|---|---|
| First *open* is LookoutPoint | **PROVEN** (this VA) |
| Native bound `ebx=1..count-1` / 151 prox | **PROVEN** (`leftover-50-native-ebx`) |
| Host `break` is OOM leftover #50 | **PROVEN** |
| Host `break` recovers native skip | **DISPROVEN** |
| Lazy parse recovers native skip | **DISPROVEN** |
| Current+adj recovers native skip | **DISPROVEN** |
| First *rendered* region is LookoutPoint | leftover **#4** |
| Oakvale intro view | leftover **#4** (`FIRST_SCENE_*`) |
| Persist `PlayerRegionName` on New Game | **UNREAD** — do not invent |
| `005223F0` construct skip | **different leftover** |

---

## Do not

- Fold #50 into #4.
- Invent `PlayerRegionName` / `00501450` so “current”
  exists at `004FDBC0`.
- Ship lazy parse or current+adj as MATCH.
- Parse every proximity TNG (OOM). Census is locked.
- Treat `GlobalThingMapsLoaded == 1` as a recovered
  NewMap-1 lock. Keep that test until a recovered bound
  exists (none does without OOM).
- Collapse first **open** (151 prox, Lookout first)
  with first **CThing** (Bridge after `006C2170`) or
  first Present (dummy / leftover #4).
- Collapse #50 (`break`) into `004FDBC0-host-leftover`
  (`+128` construct skip).
- Bind Oakvale / `00DBDE40` / kid `CREATURE_HERO_CHILD`
  onto this pump.
