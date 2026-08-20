# Who reads `[0x13B866C]` (`SetStartingHolySite`)

Investigation only. No production `src/` / `tests/` edits.

Do **not** start at Oakvale / `00DBDE40` / `StartOakVale` /
`CREATURE_HERO_CHILD` / `NOVStartHSP` spawn.
`userst.ini` `SetStartingHolySite("NOVStartHSP")` stores
this CString **before** frontend. It is **not** a quest
activate and **not** the no-save Lookout pose.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: who reads BSS `[0x13B866C]`? Search
`disp.tsv` / `abs.tsv` / listings for `013B866C`.
Relation to `NOVStartHSP` /
`HOLY_SITE_PLAYER_START` / `00DBDE40`.

Authority: `assembly/exe/01-sections/text-map/abs.tsv`
(`0x013B866C`); `disp.tsv` (no hit);
`listing-00400000.txt` (`00413800`–`0041387F`,
`00413D55`–`00413D8F`, `00416953` / `00416A38`–
`00416A5F`); `listing-00480000.txt` (`00488B20`–
`00488D0B`, `00489D40`, `0048BC70`, `0048A0AF`);
`listing-00980000.txt` (`0099AF10`, `0099B220`,
`0099B2C0`, `0099B510`, `0099B6B0`, `0099B7D0`);
`listing-01200000.txt` (`01219FE0`–`01219FFA`,
`01228980`); `listing-00d80000.txt` (`00DAC295`,
`00DBDE40`); `e8.tsv` dests `00488B20` /
`00489D40` / `00DBDE40`; `00-index/strings.tsv`
(`SetStartingHolySite` `0x0122E8A0`; no
`NOVStartHSP`); siblings `proofs/script-setnewstart`,
`proofs/ini-activate-quest`,
`proofs/hero-00489D40-retry`,
`proofs/hero-stats-first`.

---

## Verdict

**Two gameplay readers, both inside `00488B20`.**
Not `0` / **UNREAD**.

`disp.tsv` has **zero** `013B866C` rows (the slot is
an absolute BSS VA, not `[reg±disp]`). `abs.tsv` has
**six** sites. Four are stores / CRT. Two copy the
CString out for a holy-site **ScriptName** walk.

| Site | Fn | Callee | Kind | Class |
|---|---|---|---|---|
| `01219FE5` | CRT `.text` stub | `0099B6B0` | **store** default wchar `0x0122E8C0` (`MAIN_START_POSITION`) | **PROVEN** ctor |
| `00413861` | `00413840` `SetStartingHolySite` | `0099B7D0` | **store** ini arg | **PROVEN** |
| `00416A55` | `00416953` Load world | `0099B7D0` | **store** from `game+90580` if length `>0` | **PROVEN** insn; first-seen take **PARTIAL** (empty skip) |
| `00488B68` | **`00488B20`** | `0099B2C0` | **read** wchar copy → `0048CC60` | **PROVEN** |
| `00488BFB` | **`00488B20`** | `0099AF10` | **read** wchar* for miss warning | **PROVEN** (miss path only) |
| `01228980` | CRT atexit stub | `jmp 0099B510` | CString **dtor** | **PROVEN** teardown; **DISPROVEN** as gameplay |

`abs.tsv` `fn=` on the `00488B*` rows is `0x00488AB0`
(heuristic miss: `00488AAC ret` / `int3` then
`00488AB0`, then `00488B1C ret` / `int3` then
**`00488B20`**). Live reader is `00488B20`. **PROVEN**.

`00DBDE40` has **no** `013B866C` operand. **DISPROVEN**
as a reader. Only `.text` `E8` is `00DAC295`
(`Q_NewOakValeIntro` slot 2). **PROVEN** leftover.

`NOVStartHSP` is **not** in `strings.tsv`. It arrives
only as the `userst.ini` argument stored by
`00413840`. Lookout `HOLY_SITE_PLAYER_START` names
are `GuildArrivalHSP` / `LookoutPointHSP` /
`MAIN_START_POSITION`. None is `NOVStartHSP`.
First `00488B20` after Leave therefore **misses**.
**PROVEN**.

---

## Direct answers

| Item | Value |
|---|---|
| `disp.tsv` `013B866C` | **0** rows |
| `abs.tsv` `013B866C` | **6** rows (2 gameplay reads) |
| Gameplay readers | **`00488B20`** @ `00488B68`, `00488BFB` |
| First reader after Leave | `0049F180` → `00489D40` @ `00489D65` → `00488B20` |
| Spawn / quest? | **no** |
| `00DBDE40` reads it? | **no** |

**Answer:** not UNREAD. One function reads the slot:
holy-site finder `00488B20`.

---

## Timeline (no-save New Game)

```
CRT  01219FE0  0099B6B0 [0x13B866C] ← L"MAIN_START_POSITION"   // ctor
00402510 Parse Command Line
  00413C50 register SetStartingHolySite → [cmd+20]=00413840
  [0x1375444]!=0 → 00414C66 009EC890 userst.ini     // BEFORE frontend
    SetStartingHolySite("NOVStartHSP")
      00413840  0099B7D0 [0x13B866C] ← "NOVStartHSP"  // not a quest
0042EC7C retail / frontend 2D
0042F2A2 Leave frontend                              // not 00DBDE40
  00416953 Load world
    [game+90576] FinalAlbion.wld → [0x13B8668]       // SetLevel slot
    [game+90580] empty → skip 00416A55               // no rewrite of +866C
    004A1840 …
    0049F180 Init Characters                         // FIRST read
      00449D90 PLAYER_HERO miss → CREATURE_HERO
      0048A0AF  00489D40
        00489D65  00488B20
          [CPlayer+244]==0
          00488B68  0099B2C0 [0x13B866C]             // READ
          0048CC60 pred 0048BC70  Thing+116 == name
          miss → 00488BFB  0099AF10 [0x13B866C]      // READ (warning)
                  "*** WARNING : failed to find a holy site with ScriptName %S"
          al=0, [0x13B8647]==0 → ret 0               // no 006AC910
later 00501450 LookoutPoint
  HOLY_SITE_PLAYER_START GuildArrivalHSP             // live pose; not NOVStartHSP
```

`00DBDE40` / `Q_NewOakValeIntro` / `AddTestQuest` field
`+4 = NOVStartHSP` are **not** on this list. **PROVEN**.

---

## 1. Scan — `disp.tsv` / `abs.tsv` / listings

Grep `013B866C` / `13B866C`:

| File | Hits |
|---|---|
| `disp.tsv` | **0** |
| `abs.tsv` | **6** (table above) |
| `listing-00400000.txt` | `00413861`, `00416A55` |
| `listing-00480000.txt` | `00488B68`, `00488BFB` |
| `listing-01200000.txt` | `01219FE5`, `01228980` |
| other `listing-*.txt` | **0** |
| `00-index/xrefs.tsv` | **0** (BSS not indexed) |
| `e8.tsv` dest `00DBDE40` | `00DAC295` only; no `013B866C` |

`disp.tsv` columns are `[reg±disp]` with small
signed offsets. A global CString is always
`mov ecx, 0x13B866C` → `abs.tsv`. **PROVEN**
absence in `disp.tsv`.

---

## 2. Stores (not readers)

### 2.1 CRT ctor — default `MAIN_START_POSITION`

```
01219FE0  push 0x122E8C0          // UTF-16 MAIN_START_POSITION
01219FE5  mov  ecx, 0x13B866C
01219FEA  call 0099B6B0           // CString ctor; empty src → leave [esi]=0
01219FEF  push 0x1228980          // atexit 01228980
01219FF4  call 004012BC
```

`0099B6B0` zeros `[ecx]`, then intern if
`[src] != 0`. Sibling `SetLevel` ctor
`01219FC0` uses `0099AED0` on `[0x13B8668]`.
`01228980` is `mov ecx, 0x13B866C; jmp 0099B510`
(ref-count dtor). Process lifetime only.
**PROVEN** store / teardown.

`EngineLifecycle.WorldPathAltGlobalVa = 0x013B866C`
comments this slot as a WLD path. Native ctor
string is a holy-site ScriptName. Treating `+866C`
as `updatedscenic.wld` fallback is **LEFTOVER**.

### 2.2 `00413840` — `SetStartingHolySite`

Parse Command Line `00413C50`:

```
00413D55  push "SetStartingHolySite"     // 0x0122E8A0
00413D8F  mov  [esi+20], 0x413840
```

Handler is the sibling of `SetLevel` `00413800`
(`[0x13B8668]`):

```
00413840  mov eax, [ecx]
          … intern arg …
00413861  mov ecx, 0x13B866C
00413866  call 0099B7D0           // assign CString
```

`0099B7D0`: `ecx = dest`, stack arg = source
CString; `mov [esi], [edi]` + refcount.
**PROVEN** store.

TLC `userst.ini` line
`SetStartingHolySite("NOVStartHSP")` runs at
`00414C66` when `[0x1375444]!=0` (PE 1),
**before** frontend / message 15 / Leave.
Zero `ActivateQuest` in that file.
**DISPROVEN** as spawn / quest
(`proofs/ini-activate-quest`).

No `.text` `E8` of `00413840` (table bind only).
**PROVEN**.

### 2.3 `00416A55` — Load world copy from `game+90580`

```
00416A39  mov ecx, 0x13B8668
00416A3E  call 0099B7D0           // copy game+90576 → SetLevel slot
00416A43  lea edi, [esi+90580]
00416A4B  call 0099B220           // length of +90580
00416A52  jle 00416AB3            // empty → skip
00416A55  mov ecx, 0x13B866C
00416A5A  call 0099B7D0           // copy +90580 → holy-site slot
```

This **writes** `+866C`. It does **not** use
`+866C` as the WLD filename (`00416A61` reads
`[0x13B8668]` / default `0x122EE14`
`updatedscenic.wld`). Host name
`GameWorldPathAltOffset` **DIVERGE**s vs this
assign. First-seen no-save `+90580` empty →
branch not taken. **PARTIAL** take;
**PROVEN** as store when taken.

---

## 3. Readers — `00488B20` only

Only `E8` of `00488B20` is `00489D65` inside
`00489D40`. Only `E8` of `00489D40` is
`0048A0AF` inside `0048A070`. After Leave that
is Load World `0049F180` → `00449E2D`.

```
00488B20  sub esp, 36
          mov esi, ecx                    // CPlayer
          … [player+32]→[+4]+140 …
00488B50  call 0048D5C0                   // collect holy-site Things
00488B55  mov al, [esi+244]
00488B5D  jne 00488C53                    // skip name; nearest vs +232
00488B68  mov ecx, 0x13B866C
00488B6D  call 0099B2C0                   // READ: wchar copy
          mov [esp+32], 0x48BC70          // pred: Thing+116 == name
00488BC0  call 0048CC60
          cmp edi, [esp+44]
          je  00488BF2                    // miss
          … store Thing, jmp 00488CDA al=1
00488BFB  mov ecx, 0x13B866C
00488C00  call 0099AF10                   // READ: wchar*
00488C0A  push "*** WARNING : failed to find a holy site with Sc"
          call 0099F1F0
          zero [esi+232/+236/+240]
          … al=0
```

`0099B2C0` loads `[ecx]`, length
`([buf+4]-[buf])>>1`, heap copy. `0099AF10`
returns `[ [ecx] ]` or empty `0x129A8E0`.
Both **read** the CString at `0x13B866C`.
**PROVEN**.

`0048BC70`: `00BFEBA8([thing+116], search)`.
Exact ScriptName match. **PROVEN**.

`[CPlayer+244]!=0` jumps to `00488C53` and
**does not** load `0x13B866C`. First-seen
`+244` is 0 (`00487470` writer not on this
walk). **PROVEN** that first-seen does take
the two reads.

`004A5DB9` `E8 00488AB0` is the **previous**
function (`[esi+534]` / `004887C0`), not the
finder. **DISPROVEN** as a `+866C` site.

---

## 4. `NOVStartHSP` / `HOLY_SITE_PLAYER_START`

| Name | Where | First-seen `00488B20`? |
|---|---|---|
| CRT default `MAIN_START_POSITION` | ctor `01219FE0` | overwritten if `userst` ran |
| `NOVStartHSP` | `userst.ini` → `00413840` | **miss** (not a live Thing) |
| `GuildArrivalHSP` | Lookout TNG `HOLY_SITE_PLAYER_START` | not the search string |
| `LookoutPointHSP` | same file | no |
| `MAIN_START_POSITION` | same file | would hit **if** `+866C` still the ctor default **and** Things exist |
| `StartOakValeHSP` | later Oakvale TNG | leftover |

First `00488B20` is during Load World, **before**
`00501450` / `006C2170` ContainsMap. Candidate
list from `0048D5C0` is empty → miss even if the
stored name were `GuildArrivalHSP` or
`MAIN_START_POSITION`. **PROVEN**.

After maps, Lookout has three
`HOLY_SITE_PLAYER_START`. Native name walk still
searches `[0x13B866C]` (`NOVStartHSP` if `userst`
ran) unless `+244!=0`. Host
`SpawnHeroFromPlayerStart` prefers
`GuildArrivalHSP` (52.688, 69.597, 36.982) and
skips the name. **DIVERGE** vs a raw
`00488B20("NOVStartHSP")` if `+244` stays 0.
Which rewrite / `+244` / `[0x13B8647]` first
succeeds is **UNREAD**
(`proofs/hero-00489D40-retry`).

`RegionTravel.FindPlayerStart` ranks
`NOVStartHSP` then `StartOakValeHSP` then
`MAIN_START_POSITION`. **LEFTOVER** vs live
`GuildArrivalHSP`.

QST `AddTestQuest("Q_NewOakValeIntro",
"NOVStartHSP", …)` stores the same string at
`world+196` record `+4`. That vector is **not**
`[0x13B866C]`. **DISPROVEN** as this reader
(`proofs/addtestquest-token`).

---

## 5. `00DBDE40` is unrelated

```
e8.tsv: 00DAC295 → 00DBDE40     // only site
00DBDE40  push "StartOakVale"
          … 00CB7940 hero-exists …
          push "CREATURE_HERO_CHILD"
```

No `mov ecx, 0x13B866C`. No `SetStartingHolySite`.
Parent `00DABAC0` is `Q_NewOakValeIntro` VM slot 2.
Leave / `004184BD` / `00416953` / `0049F180` /
`00501450` do not `E8` it. **PROVEN** leftover
(`docs/runtime/FORWARD_TREE.md` §12).

---

## 6. Host vs native

| Host | Native | Class |
|---|---|---|
| `WorldPathAltGlobalVa = 0x013B866C` as WLD fallback | holy-site CString; WLD file is `+90576` / `[0x13B8668]` | **LEFTOVER** name |
| `ApplyUserstIni` `SetStartingHolySite` | `00414C66` → `00413840` before frontend | **PROVEN** |
| `SpawnHeroFromPlayerStart` `GuildArrivalHSP` | later create pose; **not** the `00488B20` search string after `userst` | **MATCH** pose / **DIVERGE** selector |
| `FindPlayerStart` `NOVStartHSP` | Oakvale intro ranking | **LEFTOVER** |
| Fold `0049F180` into `LoadFromFirstRealRegion` | first read is Load World, miss | **LEFTOVER** site |

---

## Classifications (short)

1. **Gameplay readers — PROVEN: `00488B20` ×2.**
   `00488B68` `0099B2C0` (search) and `00488BFB`
   `0099AF10` (miss warning). `disp.tsv` empty.
2. **Stores — PROVEN: ctor, `00413840`, `00416A55`.**
   `userst` `NOVStartHSP` is the command-line store.
3. **Spawn / quest from this slot — DISPROVEN.**
   First take misses; no `006AC910`; no
   `ActivateQuest`.
4. **`00DBDE40` — DISPROVEN as a reader.**
   No operand; leftover Oakvale intro.
5. **`HOLY_SITE_PLAYER_START` live pose —
   `GuildArrivalHSP`, not `NOVStartHSP`.**
   **PROVEN** file. Name-walk hit after maps
   **UNREAD** / host **DIVERGE**.
)
