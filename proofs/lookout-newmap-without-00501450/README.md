# Lookout NewMap 1 without `00501450`

Investigation only. Production `src/` was not edited.

Do **not** invent persist `PlayerRegionName` on New Game
(`PlayerRegionNameWrittenOnNewGame` is already `false`).
Do **not** wire dummy `Pump` / `PumpGame` to `00501450`
/ `LoadFromFirstRealRegion`. Do **not** collapse leftover
**#4** (Lookout first *rendered* scene vs Oakvale intro
*view*).

Question: fog first Present is **LookoutPoint NewMap 1**
(**PROVEN**). `00501450` has **0** inbound `E8`. Who then
sets **NewMap / current region** to Lookout **without**
`00501450`?

Find `006C2120` / `00500540` / `004FC8A0` callers on
no-save.

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Authority: dump `e8.tsv` / `calls-by-dest.tsv`;
`listing-00500000.txt` (`00500540` / `00501450` /
`00501990` / `00507F0A` / `0050682F`);
`listing-004c0000.txt` (`004FC8A0`);
`listing-006c0000.txt` (`006C2120` / `006C2170` /
`006C2671` / `006C2710`);
`listing-00400000.txt` dummy `004189C2`.
Siblings: `proofs/fog-first-present/`,
`proofs/00501450-e8-callers/`,
`proofs/00501450-inbound-final/`,
`proofs/first-region-after-leave/`,
`proofs/dummy-pumps-before-region/`,
`proofs/current-region-no-save/`,
`proofs/wld-first-region/`.
`EngineLifecycle.Pump` / `LoadFromFirstRealRegion` /
`EnqueueAfterDummy` (read only).

---

## Verdict

**NewMap 1 is WLD parse, not a current-region write.
Nobody recovered sets `WorldMap+156` to Lookout
without `00501450`.**

Fog first Present **map identity** is LookoutPoint
because `00507C30` already stored WLD `NewMap 1` /
`NewRegion 1` during Loading world — **before**
`0049F180`, **without** `00501450`. That is the
table. Dummy `+156` stays **0**.

The only recovered **nonzero** current writer is
`004FC8A0` (`mov [esi+156], eax`) at sole `E8`
`006C2671` (tail of apply `006C2170`). That apply
needs a nonempty loader list from `006C2120`, and
nonzero `job+28` from `00500540(index,…)`. On
no-save the only recovered parent that would first
open index **1** is unread `00501450` body
`005014EC`. Other `00500540` parents skip or need
a persist name — do **not** invent
`PlayerRegionName`.

| Question | Answer | Class |
|---|---|---|
| Fog first Present map = Lookout **NewMap 1**? | **Yes.** File `NewMap 1` / `NewRegion 1` | **PROVEN** (`fog-first-present`, `wld-first-region`) |
| Who writes **NewMap 1** without `00501450`? | `00507C30` token `"NewMap"` at `00507F0A` during `004A1840` | **PROVEN** parse. **Not** `+156` |
| Who writes **current** `+156=1` without `00501450`? | **Nobody recovered** on no-save | **DISPROVEN** as a first-seen path |
| `00501450` inbound `E8`? | **0** | **PROVEN** absence |
| Dummy `Pump` → `00501450`? | **No.** Do not wire | **DISPROVEN**; host **MATCH** skip |
| Invent `PlayerRegionName`? | **No.** Empty no-save; persist is `00487C20` | **DISPROVEN** |
| Collapse leftover **#4**? | **No.** Lookout rendered ≠ Oakvale intro view | **LEFTOVER** — leave open |

---

## Recovered order (no-save New Game)

```
0042F2A2  Leave
0042F491  Init Game → 00416953 FinalAlbion.wld
  004A1840
    00507C30  parse
      NewMap 1     LookoutPoint.lev / LevelScriptName "LookoutPoint"
      NewRegion 1  RegionName "LookoutPoint"     ; native index 1
      NewRegion 4  StartOakVale                  ; later leftover #4 view
    004A1AA3  006C20A0  EMPTY                    ; no 006C27A0 / 006C2120
    00B428E0  FinalAlbion.stb MISS               ; +44 empty
    0049F180  Init Characters                    ; 0 of 00501450 / 00500540
  user.ini Gameflow                              ; 0
004189C2  dummy pumps
  004FB150 / 004FC180 index 0                    ; +156 stays ctor 0
  type-1 004A5DF3 006B3FF0                       ; still dummy; 0 of these dests
00435F70  first game Present                     ; dest empty; not fog-lit
; 00501450 still 0 inbound
; 00500540 / 006C2120 / 004FC8A0 still 0
later  (E8 UNREAD; host LoadFromFirstRealRegion stand-in)
  00501450  00500540(1,0,0) Lookout              ; body only
    006C27A0 / 006C2120 / 006C20A0 → 006C2170
    006C2671  004FC8A0(1)  MiniMap; +156=1
```

Fog-lit Present needs nonempty landscape `+44` after
a later STB hit. That apply is the same unread
`00500540` hole. Dummy Present is **not** that
Present (`fog-first-present`). Do not close the hole
by calling `00501450` from `Pump`.

---

## 1. NewMap 1 ≠ current region

`NewMap` is a WLD token. Current is `WorldMap+156`.

### Table (no `00501450`)

`00507C30` (`listing-00500000.txt`):

```
00507F0A  mov esi, "NewMap"
00507F18  je  0050834F
```

First authored block in TLC `FinalAlbion.wld`:

```
NewMap 1;
  LevelName "FinalAlbion\LookoutPoint.lev";
  LevelScriptName "LookoutPoint";
NewRegion 1; RegionName "LookoutPoint";
```

Native table index **1** (dummy slot 0 is
`005066E0`). `StartOakVale` is `NewRegion 4`.
**PROVEN** file bytes (`wld-first-region`).

Parse is **not** `006C2170`. **PROVEN**
(`first-region-after-leave`).

### Current (`+156`)

| Writer | Store | `E8` sites | No-save first-seen |
|---|---|---|---|
| ctor `005066E0` | `0050682F mov [esi+156], ebx` → **0** | construct | **PROVEN** dummy |
| unload `004FEEC0` | `004FF03F` → **0** | `005014A3` / `00501839` / `0050254E` / `005025EC` / `00506442` / `004FF569` | none on dummy tree |
| `004FC8A0` SetRegionAsLoaded | `004FC8B2 mov [esi+156], eax` | **one:** `006C2671` | **0** |

Fog first Present “Lookout **NewMap 1**” names the
**authored map**. It does **not** prove `+156=1` on
dummy Present. Dummy `00435F70` is dest-empty
(**PROVEN** skip of fog DIP).

---

## 2. Callers on no-save (`e8.tsv` / `calls-by-dest`)

### `00500540` — six `E8`s

| Site | Real parent | Args | No-save recovered? |
|---|---|---|---|
| `00487C55` | persist `00487C20` | `(index,0,1)` after name lookup | **No.** Needs nonempty `PlayerRegionName`. Empty no-save. **DISPROVEN**. Do **not** invent the key |
| `005014EC` | **`00501450`** | `(i,0,0)` loop; first `i=1` Lookout | Body **PROVEN**; inbound **0** / **UNREAD** |
| `00501935` | **`00501450`** | `(saved,0,1)` restore, no pump | same |
| `0050255D` | **`00502500`** (swallow tag `00501450`) | after `004FEEC0`; `(ebx, arg, 1)` | Parent has `E8` (`004A4CB9`). First-seen `[world+260]=0` skips `004A3740`. **DISPROVEN** as no-save first |
| `005025F8` | **`005025B0`** | `(saved,0,1)` | Dest `005025B0` **0** — same unread class |
| `00506455` | **`00502E90`** | after `004FEEC0` | Later travel (`0065C7B4` / `008A1CAD`). **DISPROVEN** as dummy |

None of these run on dummy / type-1 / first
`00435F70`. **PROVEN** skip (`dummy-pumps-before-region`,
`00501450-e8-callers`).

### `006C2120` — four `E8`s (list-insert `[loader+20]`)

```
006C2120  mov esi, ecx          ; loader
          call 006C20B0
          mov esi, [esi+20]
          push 16 / call 00BFEA0E
          ; link node onto [loader+20]
          ret 4
```

| Site | Real parent | No-save recovered? |
|---|---|---|
| `00500D8A` | **`00500540`** (after `006C27A0`) | only if `00500540` ran |
| `005010BE` | **`00500540`** | same |
| `00501329` | **`00500540`** | same |
| `00501C48` | **`00501990` UpdateNavMaps** | Init Quests `+144` empty → **no-op**. `00501C07 push ebx` so `job+28=0` — would **skip** `004FC8A0` even if it ran (`006C25E8 jle 006C267F`) |

**0** of these call `00501450`. **0** on dummy tree.
WLD `004A1AA3 006C20A0` is **empty** (no
`006C27A0` / `006C2120`). **PROVEN.**

### `004FC8A0` — one `E8`

```
004FC8A0  mov eax, [esp+4]
004FC8B2  mov [esi+156], eax     ; current = arg
          "SetRegionAsLoaded: Initialise MiniMap"
          call 00437CE0 / 0082BA00
          ret 8
```

| Site | Parent | No-save recovered? |
|---|---|---|
| `006C2671` | **`006C2170`** apply tail | only if loader list nonempty **and** `job+28>0` |

`006C2170` itself: dest **one** `E8` `006C2752`
inside `006C2710` (“Level loader update”), plus
`006C2700 jmp 006C2170` (same family). Reached
from `006C20A0` while `[loader+20]` is nonempty.

`006C20A0` `E8`s: `004A1AA3` (WLD empty), three
inside `00500540`, `00501C86` (UpdateNavMaps),
`00502948` / `00502962` (other apply, not dummy).

So: **NewMap parse does not call `004FC8A0`.**
MiniMap current write is apply-only.

---

## 3. Without `00501450`, current stays dummy 0

```
dummy / type-1 / first Present
  +156 = 0
  00500540 = 0
  006C2120 = 0
  004FC8A0 = 0
```

**PROVEN.** Lookout current (`+156=1`) is
`004FC8A0(1)` after `00500540(1,0,0)`. That
loader’s only recovered **no-save first open**
parent is `00501450`. Inbound still **0**.

Do **not** treat WLD `NewMap 1` as that write.
Do **not** treat UpdateNavMaps `006C2120` as
Lookout current (job index 0). Do **not** treat
persist `00487C55` as New Game.

Host tests that need Lookout TNG / hero /
fog-lit Present call `LoadFromFirstRealRegion()`
**explicitly** after dummy
(`Second_pump_00501450_is_004FEEC0_then_00500540_1_0_0`).
That API **is** `00501450`. Leftover **when**,
not a recovered `Pump` site. `PumpCallsLoadFromFirstRealRegion
= false`. **MATCH** skip.

`EnqueueAfterDummy` with a **set**
`PlayerRegionName` is persist `00487C20`, not
no-save. **DISPROVEN** as this walk.

---

## Leftover #4 (do not collapse)

| Ledger | Pairing | This page |
|---|---|---|
| LookoutPoint WLD index **1** / NewMap **1** | first fog-lit / first authored region; `GuildArrivalHSP` | **keep** |
| Oakvale intro *view* | `StartOakVale` / West / `HerosOldHouse` / `CAM_OVIF_SHOT2` / `Q_NewOakValeIntro` | **later**; **DISPROVEN** as first Present |

Dummy empty `00435F70` is **not** Oakvale and is
**not** a reason to `E8` `00501450` from `Pump`.
#4 is Lookout geometry vs intro view, not dummy vs
Lookout.

---

## Classifications (short)

1. **NewMap 1 Lookout without `00501450` — `00507C30`
   parse. PROVEN.** Fog first Present map identity
   is that table row. Not `WorldMap+156`.
2. **Current Lookout without `00501450` — DISPROVEN
   on recovered no-save.** Only nonzero writer is
   `004FC8A0` ← `006C2671` ← `006C2170` ←
   `006C2120` ← `00500540(1,…)`. First no-save
   parent of that apply is unread `00501450`.
3. **No-save callers of `00500540` / `006C2120` /
   `004FC8A0` — PROVEN skip** on dummy / type-1 /
   first Present. Persist needs `PlayerRegionName`
   — **do not invent**. Travel / `00502500` skip.
4. **Do not wire dummy `Pump` to `00501450`. Do not
   collapse leftover #4.** Host explicit
   `LoadFromFirstRealRegion` is leftover **when**;
   live `Pump` skip **MATCH**.
