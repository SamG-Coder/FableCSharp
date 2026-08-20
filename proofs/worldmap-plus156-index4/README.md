# Who first stays `WorldMap+156=4` (StartOakVale) without persist `00487C20`

Investigation only. No production `src/` edits.

Do **not** invent persist `PlayerRegionName` on New Game.
Do **not** invent `PlayerRegionNameWrittenOnNewGame` (already
`false`). Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** collapse leftover **#4**.

Sibling `proofs/startoakvale-current-writer`: `WorldMap+156`
writers are ctor `005066E0` → `0`; unload `004FEEC0` → `0`;
apply `004FC8A0(index)` sole `E8` `006C2671`. This note asks
who first **stays** on `+156=4` (`StartOakVale`) **without**
persist `00487C20`.

Childhood `00DBDE40` waits on `StartOakVale` already current.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: ExeIndex `listing-004c0000.txt` (`004FC8A0` /
`004FEEC0` / `004FB150` / `004FC210`; there is **no**
`listing-004f0000.txt` — `004FC8A0` lives in the `004C0000`
chunk), `listing-006c0000.txt` (`006C2671` / `006C2170` /
`006C2710` / `006C27A0` / `006C20A0`), `listing-00500000.txt`
(`005066E0` / `00500540` / `00501450` / `00502500` /
`005025B0` / `005063E0` / `005064C0`),
`listing-00480000.txt` (`00487C20` / `00487F10` / `00488AB0`
/ `004A3740` / `004A1AA3` / `004A5BFB`),
`listing-00d80000.txt` (`00DBDE40`),
`listing-00880000.txt` (`0089B99E`),
`e8.tsv` dest `004FC8A0` / `00500540` / `006C27A0` /
`006C2170` / `00487C20` / `00502500` / `005063E0`,
`calls.tsv`, `vtbl.tsv` (`0x0125D9B0+4` `006C27D0`,
`0x01244AEC+88` `005064C0`);
`00-index/strings.tsv` / `xrefs-by-string.tsv`
(`StartOakVale` `0x012D9D1C`, `PlayerRegionName`
`0x01231C98`);
siblings `proofs/startoakvale-current-writer`,
`startoakvale-index4-loader`, `00501450-e8-callers`,
`player-region-name-writer`, `00DBDE40-host-gap`,
`oakvale-without-leftover4`;
`EngineLifecycle.CurrentRegionIndex` /
`SetRegionAsLoaded` / `PlayerRegionNameWrittenOnNewGame`
(read only).

---

## Verdict

**Nobody** on first-seen no-save.

`006C2671` is **not** first-seen no-save. It is the sole
`E8` of `004FC8A0`, and `004FC8A0` is the sole recovered
writer of a **nonzero** `+156`. First-seen never reaches
it. Dummy ctor / first pumps leave `+156=0`.

A write of `+156=4` **stays** only if that apply’s
`job+28` is `4` and no later `004FEEC0` / `004FC8A0`
overwrites it. The only recovered stay of index `4` is
persist `00487C20` → `00500540(index,0,1)` — **excluded**.

Without persist, the recovered `00500540(4)` is
`00501450` loop `i=4`: it **does not stay** (`i` continues
to `141`). Parent `E8` of `00501450` is **0**. Later
stay-current machines (`00502500` dest-4, `005063E0`
map→region 4) are first-seen skip / flag-gated / not
childhood. `00DBDE40` **waits**; it does not write `+156`.

| Claim | Class |
|---|---|
| `WorldMap+156` writers | **PROVEN** three: ctor `0`, `004FEEC0` `0`, `004FC8A0` job index |
| Every `E8` of `004FC8A0` | **PROVEN** one: `006C2671` inside apply `006C2170` |
| `006C2671` first-seen no-save? | **DISPROVEN** |
| First-seen `+156` | **PROVEN** `0` (ctor `0050682F`) |
| `job+28<=0` skips `004FC8A0` | **PROVEN** `006C25E8` `jle 006C267F` |
| Nonzero `job+28` only from `00500540` | **PROVEN** — other `006C27A0` sites push `ebx=0` |
| `00501450` `i=4` writes `+156=4` | **PROVEN** numeric; **DISPROVEN** stay; **DISPROVEN** first-seen |
| `E8` of `00501450` | **PROVEN** `0` |
| Persist `00487C20` stays on named index (e.g. 4) | **PROVEN** continue; **DISPROVEN** no-save; **excluded** by the question |
| `00502500` dest-4 would stay | **PROVEN** body if `[0x13756F6]` and dest region `4`; first-seen `004A3740` skip **PROVEN** |
| `005063E0` region-4 would stay | **PROVEN** body if flag; `00487C91` is persist-adjacent (**excluded**); first-seen hero miss / flag **PARTIAL** |
| `005064C0` villages calls `00500540` | **DISPROVEN** — no `E8`; `00506455` is `005063E0` |
| `00DBDE40` writes `+156` / `E8` `004FC8A0` | **DISPROVEN** — wait on name already current |
| Invent `PlayerRegionName` / `PlayerRegionNameWrittenOnNewGame` | **DISPROVEN** (method) |
| Who first **stays** on `+156=4` without persist | **UNREAD** (no recovered no-save stay) |

**Answer:** nobody first-seen. Nobody recovered stays on
`4` without persist `00487C20`. `006C2671` is the only
apply site that *can* write `4`, and it is not first-seen
no-save. Childhood `00DBDE40` still waits.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Every `E8` of `004FC8A0`? | One: `006C2671` (`e8.tsv` / `calls.tsv` / listing) | **PROVEN** |
| Is `006C2671` first-seen no-save? | **No.** Empty loader queue; `job+28` never 4 | **DISPROVEN** |
| Who first **stays** `+156=4` without `00487C20`? | No recovered stay. Transient `00501450 i=4` only | **UNREAD** stay; **DISPROVEN** first-seen |
| Does `00DBDE40` set current to StartOakVale? | **No.** `vtbl+48` wait on `"StartOakVale"` | **DISPROVEN** as a writer |

---

## 1. The slot and the three writers (PROVEN)

`listing-004c0000.txt` — no `listing-004f0000.txt`:

```
004FB150  mov eax, [ecx+156]
          ret
```

`004FC8A0` (`SetRegionAsLoaded: Initialise MiniMap`):

```
004FC8A0  mov eax, [esp+4]
          mov esi, ecx
          …
004FC8B2  mov [esi+156], eax          ; write current INDEX
          …
          imul ecx, ecx, 88
          add ecx, [esi+44]
          push ecx
          mov ecx, [0x13B8790]
          call 00437CE0
          call 0082BA00               ; MiniMap only
          ret 8
```

`004FEEC0` tail:

```
004FF03F  mov [esi+156], 0
```

Ctor `005066E0` (`listing-00500000.txt`):

```
0050682F  mov [esi+156], ebx          ; ebx=0 dummy
```

WorldMap object (`004FB150` / ctor `005066E0`):

| Site | Parent | Value |
|---|---|---|
| `0050682F` | `005066E0` Init World Map | `0` |
| `004FF03F` | `004FEEC0` unload | `0` |
| `004FC8B2` | `004FC8A0` after apply | job native index |

Other `.text` `[reg+156]` stores are **other** objects.
They are not this WorldMap dword.

Index **4** is WLD `NewRegion 4` `RegionName "StartOakVale"`,
not `StartOakValeWest` / `HerosOldHouse`.

---

## 2. Every `E8` of `004FC8A0` is `006C2671` (PROVEN)

`e8.tsv` dest `0x004FC8A0`: **one** row.

```
0x006C2671    0x004FC8A0
```

`calls.tsv`: same site, containing fn `0x006C1BE0`
(recovery blob; the call sits in apply `006C2170`).

Listing `call 004FC8A0`: **one**, `listing-006c0000.txt`
`006C2671`. Zero in `listing-00d80000.txt` (`00DBDE40`).
Zero `FF` of `004FC8A0`.

`006C2170` tail (`listing-006c0000.txt`):

```
006C25E8  mov eax, [esi+28]           ; job+28
          test eax, eax
          jle 006C267F                ; <=0: skip 004FC8A0
          …
006C2649  call [edx+88]               ; WorldMap vtbl+88 = 005064C0
006C2662  mov ecx, [esi+28]
          push name
          push ecx                    ; index
          mov ecx, WorldMap
006C2671  call 004FC8A0               ; +156 = job+28
006C267F  …
```

`vtbl.tsv` `0x01244AEC` slot 22 (`+88`) = `005064C0`
(`World Map: Post Region Load Villages`). That body has
**no** `E8` `00500540`. Earlier notes that hung
`00506455` on `005064C0` are **wrong**: `00506455` is
inside `005063E0`.

`006C27A0` stores the third arg into `job+28`:

```
006C27C0  mov [esi+28], edx
```

`e8.tsv` dest `006C27A0` — **four** sites:

| Site | Parent | Third arg (`job+28`) |
|---|---|---|
| `00500D7A` | `00500540` | native index (`[esp+172]` = arg0) |
| `005010AE` | `00500EB0` | `push ebx` (`0`) |
| `00501319` | `00501150` | `push ebx` (`0`) |
| `00501C0E` | `UpdateNavMaps` | `push ebx` (`0`) |

Only `00500540` can make `job+28=4`. The other three
leave `job+28=0` and `006C25E8` **skips** `006C2671`.

Null `+36` in `00500540` still continues (`je 005009BE`
→ `jmp 00500887` → same `006C27A0` with arg0).

---

## 3. `006C2671` is not first-seen no-save (DISPROVEN)

Apply chain:

```
006C27A0  job+28 = index
006C2120  enqueue
sync: 006C20A0 (queue nonempty?)
      vtbl+4 006C27D0 → 006C2710
006C2752  call 006C2170
006C2671  call 004FC8A0
```

`vtbl.tsv` `0x0125D9B0` slot 4 = `006C27D0`
(`jmp 006C2710` `Level loader update`). Sole `E8` of
`006C2170`: `006C2752`.

`006C2710` empty-queue skip:

```
006C2718  cmp [eax], eax
006C271D  je  006C2797                ; no 006C2170
```

`006C20A0` is a **query**, not a pump (`setne` if
`[head]!=head`).

First-seen no-save:

```
005066E0  +156=0
004189C2  dummy pumps
  no 00500540 / no 006C27A0(index=4)
  loader queue empty
004A1AA3  006C20A0 → al=0 → skip vtbl+4
  006C2710 if reached: je skip apply
  006C2671 not executed
```

`E8` of `00501450`: **0**. `E8` of `00487C20` is
`00487F10` inside `00487EF0`; empty no-save skips
`00449E60`. Type-1 `[world+260]=0` skips `004A3740`
(`004A5BEF` `je 004A5C21`).

First-seen current stays ctor **0**. `006C2671` is a
later apply site, not first-seen.

---

## 4. `00500540` callers — who can **stay** on 4

`e8.tsv` dest `0x00500540`: **six** sites.

| Site | Parent | `00500540` args | Stay on 4 without persist? |
|---|---|---|---|
| `00487C55` | `00487C20` persist name | `(index,0,1)` async | **Yes** if name is `StartOakVale` — **excluded** |
| `005014EC` | `00501450` loop | `(i,0,0)` sync | `i=4` transient; then `i=5..141` |
| `00501935` | `00501450` restore | `(saved,0,1)` no pump | first-seen `saved=0`; current stays **141** |
| `0050255D` | `00502500` map switch | `(destRegion, arg, 1)` | **if** dest region is 4 and `[0x13756F6]` |
| `005025F8` | `005025B0` reload current | `(+156, 0, 1)` | only if already 4 — not first |
| `00506455` | `005063E0` map→region | `(regionOfMap, arg, 1)` | **if** that region is 4 and `[0x13756F6]` |

### 4a. Persist `00487C20` — stay, excluded (PROVEN)

```
00487C34  call [world.vtbl+48]        ; name CString on PLAYER blob
00487C39  call 004FC210              ; FindRegionByName from index 1
          je  00487CD7               ; empty / miss
00487C55  call 00500540(index, 0, 1)
00487C91  call 005063E0              ; after the load
```

`004FC210` walks 88-byte rows from index **1**.
`"StartOakVale"` is row 4. Async enqueue; later
`006C2671` writes `+156=4` and **stays** (no sweep).

`E8` of `00487C20`: one, `00487F10` inside `00487EF0`.
Parent `00449E60` pushes `"PlayerRegionName"`. Empty
no-save skips. `PlayerRegionNameWrittenOnNewGame` is
already `false`. Do **not** invent a New Game write.

### 4b. `00501450` `i=4` — first numeric 4, does not stay (PROVEN body)

```
00501495  mov esi, [edi+156]         ; saved
005014A3  call 004FEEC0              ; +156=0
          i = 1
005014EC  call 00500540(i, 0, 0)     ; sync → 006C2671 +156=i
          inc i
          jb  005014E3               ; 1 .. count-1
00501935  call 00500540(saved, 0, 1) ; no pump
```

Count 142. `i=4` **is** StartOakVale ContainsMap, then
`i=5`…`i=141`. Last `004FC8A0` leaves `+156=141`
`Filler_NorthernWastes_02`. Restore `(0,0,1)` does not
pump. Not stay-current. Not first-seen. Inbound `E8` **0**.

### 4c. `00502500` — later stay, first-seen skip (PROVEN skip)

```
00502512  mov al, [0x13756F6]
          je  00502564               ; flag 0: no 00500540
00502534  call 004FC190              ; dest map → region
0050253E  call 004FC190
          je  0050257F               ; same region
0050254E  call 004FEEC0(old, 1)
0050255D  call 00500540(new, arg, 1) ; stay on new
```

`E8` of `00502500`: `004A4CB9` (`004A3740`) and
`0089B99E` (`00892D80`).

`004A5BEF`: `[world+260]==0` → skip `004A3740`.
First-seen type-1 is 0.

`0089B918` `jmp 0089BAE9` after `0049EAF0` — same-region
intro Teleport **jumps over** `0089B99E`. Cross-region
Teleport would take it.

`abs.tsv` `0x013756F6`: **three** reads
(`00502512` / `00502850` / `005063E0`), **zero** stores.
Writer of the byte **UNREAD**. BSS first-seen 0 would
skip both load arms.

First dest map whose `004FC190` region is 4: **UNREAD**.
Not first-seen. Not a recovered childhood stay.

### 4d. `005063E0` — flag-gated map load (PROVEN body; not first stay)

```
005063E0  mov al, [0x13756F6]
          je  0050646F               ; skip 00500540
          …
0050640A  call 004FC190              ; map → region
00506455  call 00500540(region, arg, 1)
```

`E8` of `005063E0`:

| Site | Parent | No-save first-seen |
|---|---|---|
| `00487C91` | `00487C20` | persist — **excluded** |
| `00488B16` | `00488AB0` player tick | needs hero `+44`; flag |
| `00489F47` | later place / dest map | not first-seen |

`00488AB0` from `004A5DB9`. First-seen player-thing miss
skips `005063E0`. Even with a hero on Lookout, dest
region would be **1**, not 4.

`005025B0` unloads then reloads `[edi+156]`. Stay of
whatever is already current. Not first.

---

## 5. Childhood `00DBDE40` waits (PROVEN)

`.text` `"StartOakVale"` `0x012D9D1C`: only `00DBDE4A`
and `00DBDE9B`, both `fn=00DBDE40`.

```
00DBDE49  push "StartOakVale"
          intern 0099EBF0
          mov ecx, [esi+64]          ; script context
          call [eax+48]              ; name current / ready?
          ; neg/sbb/inc → wait while false
00DBDE7F  je  00DBDECA               ; already true → skip wait
00DBDE81  call [eax+28]              ; yield
          …
          intern "StartOakVale" again
          call [edx+48]
          jne 00DBDE81
00DBDECA  … then CREATURE_HERO_CHILD
```

Zero `call 00500540` / `call 00487C20` / `call 004FC8A0`
in `listing-00d80000.txt` for this fn. Sole `E8` of
`00DBDE40`: `00DAC295` in `00DABAC0` (`S_QNOVI` slot 2).

The fiber **requires** `+156=4` already. It is not a
writer. Activator `00CB5AD0("Q_NewOakValeIntro")` is
**UNREAD** on no-save. Bind is not construct. Gameflow
yields on the inactive name.

---

## Childhood path without persist (recovered order)

```
no-save Leave / first pumps
  005066E0  +156=0
  004189C2  dummy
  no 00500540 / no 006C2671
  00DBDE40  not reached               // wait, not load
  00487C20  not reached               // persist empty
later (E8 UNREAD; not first-seen)
  00501450  00500540(1,0,0)           // leftover #4 first real
            00500540(4,0,0)           // transient +156=4
            +156=141                  // does not stay
  00502500 dest-4                     // stay IF flag + dest 4
  005063E0 map region 4               // stay IF flag
006C2671 004FC8A0(4) stay without persist
  // UNREAD caller that both runs and is last apply
```

---

## Do not

- Write `PlayerRegionName = "StartOakVale"` on New Game.
- Invent `PlayerRegionNameWrittenOnNewGame` (already
  `false`).
- Treat `006C2671` as first-seen no-save.
- Treat `00501450 i=4` as stay-current or first Present.
- Attribute `00506455` to villages `005064C0`.
- Call `00DBDE40` / `LoadFromFirstRealRegion` from Pump.
- Collapse leftover **#4** (Lookout first real vs Oakvale
  intro view).
- Store `StartOakValeWest` / `HerosOldHouse` as `+156`.

---

## Locking tests (not edited)

- `Install_banks_and_startup_videos_exist` — first Pump
  dummy index 0; WLD index 4 is `StartOakVale`; after
  explicit `LoadFromFirstRealRegion`, current is 141
- `Game_pump_is_004189C2_not_00DBDE40` —
  `GetCurrentRegionIndexFn=004FB150`, offset 156
- `Second_pump_00501450_is_004FEEC0_then_00500540_1_0_0`
  — first `(1,0,0)`, last 141, no `00DBDE40`
- `Persist_PlayerRegionName_is_00487C20_not_new_game` —
  continue stand-in index 4;
  `PlayerRegionNameWrittenOnNewGame==false`;
  `StartOakValeSetupLoadsRegion==false`
- `No_save_does_not_activate_Q_NewOakValeIntro`
