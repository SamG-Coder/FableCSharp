# Every `WorldMap+156` writer besides `00501450` — who would set index **4**

Investigation only. No production `src/` or `tests/` edits.

Do **not** invent persist `PlayerRegionName` on New Game
(`PlayerRegionNameWrittenOnNewGame` is already `false`).
Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** wire `LoadFromFirstRealRegion` / dummy Pump.
Do **not** collapse leftover **#4**.

Playable-path-gap-graph leftover: dump caller that **stays**
current region **4** `StartOakVale` is **UNREAD**.
`00DBDE40` only **waits** context `vtbl+48`.
`StartOakValeSetupLoadsRegion=false`.

Question: every writer of `WorldMap+156` / NewMap current
**besides** `00501450` (`e8.tsv` dest **0**). Who would
set index **4**?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: ExeIndex
`listing-004c0000.txt` (`004FB150` / `004FC8A0` /
`004FC190` / `004FC210` / `004FEEC0` / `004FF03F`),
`listing-00500000.txt` (`00500540` / `00501450` /
`00502500` / `005025B0` / `005063E0` / `005064C0` /
`005066E0` / `0050682F`),
`listing-00480000.txt` (`00487C20` / `00487C55` /
`00487C91` / `00488B16` / `00489F47` / `004A4CB9` /
`004A5BEF` / `004A5BFB`),
`listing-006c0000.txt` (`006C2170` / `006C2671` /
`006C27A0`),
`listing-00880000.txt` (`0089B780` / `0089B918` /
`0089B99E`),
`listing-00d80000.txt` (`00DBDE40` / `00DAC295`),
`e8.tsv` dest `004FC8A0` / `00500540` / `00502500` /
`005025B0` / `005063E0` / `00501450` / `00487C20` /
`006C27A0` / `004FEEC0`,
`abs.tsv` `0x013756F6`,
`00-index/strings.tsv` / `xrefs.tsv` `StartOakVale`
`0x012D9D1C`;
`EngineLifecycle.GetCurrentRegionIndexFn` /
`SetRegionAsLoadedFn` / `LoadRegionAtMapFn` /
`PlayerRegionNameWrittenOnNewGame` /
`PumpCallsLoadFromFirstRealRegion` (read only);
`RegionTravel.StartOakValeSetupLoadsRegion` /
`FirstSeenTeleportChangesRegion`;
siblings `worldmap-plus156-index4`,
`startoakvale-index4-loader`,
`oakvale-without-leftover4`,
`playable-path-gap-graph`,
`00501450-e8-callers`,
`leftover-4-collapse-audit`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Every recovered `WorldMap+156` **store**? | Three: ctor `0050682F` **0**; unload `004FF03F` **0**; apply `004FC8B2` **job index**. | **PROVEN** |
| Every `E8` of that apply `004FC8A0`? | **One:** `006C2671` inside `006C2170`. `job+28<=0` skips. | **PROVEN** |
| Every `E8` of `00500540` (the only path that can make `job+28=4`)? | **Six:** `00487C55`, `005014EC`, `00501935`, `0050255D`, `005025F8`, `00506455`. | **PROVEN** |
| `E8` / `E9` / imm / vtbl of `00501450`? | **0.** Body can write 4 **transiently**. Not Pump. | **PROVEN** absence; **UNREAD** live entry |
| Who besides `00501450` would set **4** and **stay**? | Persist `00487C20` (`00487C55`) — **continue, excluded**. Later `0050255D` / `00506455` **if** dest region is 4 **and** `[0x13756F6]`. | persist **DISPROVEN** no-save; dest-4 **UNREAD** |
| Does `00DBDE40` write `+156`? | **No.** `vtbl+48("StartOakVale")` wait. `StartOakValeSetupLoadsRegion=false`. | **DISPROVEN** as writer |
| First no-save Present `+156`? | Ctor **0** / dummy. Pump never calls `00501450`. | **PROVEN** |
| Leftover **#4** first *real* if unread `00501450` ran? | Lookout index **1**, then sweep to **141**. `i=4` does **not** stay. | **PROVEN** body |

**Answer:** nobody recovered **stays** on `WorldMap+156=4`
without persist `00487C20`. Besides `00501450` (0 inbound,
transient `i=4` only), the only other `00500540` sites that
*can* pass **4** are persist (do **not** invent) and the
flag-gated later switch `00502500` / `005063E0` whose dest
region-4 caller is **UNREAD**. Do not fill that with
`PlayerRegionName`. Do not wire dummy Pump.

---

## Verdict

`WorldMap+156` is an **integer index** (`004FB150`
`mov eax,[ecx+156]; ret`). Index **4** is WLD
`NewRegion 4` `RegionName "StartOakVale"`, not
`StartOakValeWest` and not `HerosOldHouse`.

The sole recovered **nonzero** writer is `004FC8A0`
(`004FC8B2 mov [esi+156], eax`). Sole `E8`:
`006C2671`. `eax` is load-job `+28`. Only
`00500540` stores a native index there; the other
three `006C27A0` sites `push ebx` (**0**) and
`006C25E8 jle` **skips** the apply.

`00501450` is **out of this question’s stay-current
set**: inbound encodings **0**; if it ran, `i=4` is
one step of `1..141` and current **ends at 141**.

Childhood `00DBDE40` **requires** index 4 already
current. It does not load. Activator of `S_QNOVI` is
a different UNREAD (`00CB5AD0` intern `0x012C5D14`).

---

## 1. The three `WorldMap+156` stores (PROVEN)

`listing-004c0000.txt` / `listing-00500000.txt`:

```
004FB150  mov eax, [ecx+156]
          ret

0050682F  mov [esi+156], ebx          ; ebx=0; ctor 005066E0
004FF03F  mov [esi+156], 0            ; unload 004FEEC0
004FC8B2  mov [esi+156], eax          ; apply 004FC8A0 arg0
```

`e8.tsv` dest `0x004FC8A0`:

```
0x006C2671    0x004FC8A0
```

One listing `call 004FC8A0`. Zero in `00DBDE40`.
Zero `FF` of `004FC8A0`.

`006C2170` tail:

```
006C25E8  mov eax, [esi+28]           ; job+28
          test eax, eax
          jle 006C267F                ; skip 004FC8A0
          …
006C2649  call [edx+88]               ; 005064C0 villages BEFORE write
006C2662  mov ecx, [esi+28]
          push ecx                    ; index
006C2671  call 004FC8A0               ; +156 = job+28
```

Other `.text` `mov [reg+156]` are **other objects**
(widgets, fade, camera, stack `[esp+156]`). They are
not this WorldMap dword. Byte stores (`mov [esi+156], al`)
are not the index.

`004FC8A0` then `00437CE0` / `0082BA00` MiniMap only.
Not `00B428E0`. Villages `005064C0` is vtbl+88 **before**
the write, and has **no** `E8` `00500540`.

**Host.** `CurrentRegionIndex` / offset 156 /
`GetCurrentRegionIndexFn=004FB150`. First Pump after
Leave: index **0**, `CurrentRegion=null`. MATCH ctor.

**Gap.** MiniMap / villages bodies UNREAD. Not index 4.

---

## 2. How `job+28` becomes a native index (PROVEN)

`006C27A0` `mov [esi+28], edx`. `e8.tsv` dest
`0x006C27A0` — **four**:

| Site | Parent | Third arg (`job+28`) | Can be 4? |
|---|---|---|---|
| `00500D7A` | `00500540` | arg0 native index | **yes** |
| `005010AE` | `00500EB0` | `push ebx` **0** | no — apply skip |
| `00501319` | `00501150` | `push ebx` **0** | no |
| `00501C0E` | UpdateNavMaps | `push ebx` **0** | no |

**Only `00500540` can make `+156=4`.** Census that dest.

---

## 3. Every `E8` of `00500540` — six, none are Pump (PROVEN)

`e8.tsv` dest `0x00500540`:

| Site | Parent | Args | Sets 4? | Stays on 4 without persist? |
|---|---|---|---|---|
| `00487C55` | `00487C20` persist name | `(index,0,1)` async | **if** `004FC210("StartOakVale")` | **Yes** — **excluded** (continue) |
| `005014EC` | `00501450` loop | `(i,0,0)` sync | `i=4` one step | **No** — `i` continues to 141 |
| `00501935` | `00501450` restore | `(saved,0,1)` no pump | first-seen `saved=0` | **No** — current stays **141** |
| `0050255D` | `00502500` map switch | `(destRegion, arg, 1)` | **if** `004FC190` dest is 4 and `[0x13756F6]` | **if** those — **UNREAD** dest |
| `005025F8` | `005025B0` reload current | `(+156, 0, 1)` | only if **already** 4 | not first; **0** `E8` of `005025B0` |
| `00506455` | `005063E0` map→region | `(regionOfMap, arg, 1)` | **if** that region is 4 and flag | **if** those — **UNREAD** dest |

`00501908 push 4` inside `00501450` is **not** index 4.
It is `0099AD80` (`push 2` / `push 4`) for
`RegionGraph.txt` (`0x0124467C`). **DISPROVEN** as
`+156=4`.

---

## 4. Besides `00501450` — persist (excluded)

```
00487C20  lea eax, [edi+8]            ; CString on PLAYER blob
          call [world.vtbl+48]
00487C39  call 004FC210              ; FindRegionByName from 1
          je  00487CD7               ; empty / miss
00487C55  call 00500540(index, 0, 1)
00487C91  call 005063E0
```

`004FC210` walks 88-byte rows from index **1**.
`"StartOakVale"` is row **4**. Later `006C2671` writes
`+156=4` and **stays** (no sweep).

`e8.tsv` dest `0x00487C20`: **one**, `00487F10` inside
`00487EF0`. `e8.tsv` dest `0x00487F10`: **0**. Parent
`00449E60` `push "PlayerRegionName"`. Only `E8` of
`00449E60`: `004A2B05` after `push "PLAYER"` in
FableSav. No-save `[game+90588]` empty skips
`004A3200`.

Save of the key is `00449F90` (`0049FB5C` inside
`0049F4C0`). `.text` xrefs of `"PlayerRegionName"`
`0x01231C98`: **two** (load / save). HEADER
`CurrentRegionName` is a different key.

`PlayerRegionNameWrittenOnNewGame=false`. Tests that
**assign** `PlayerRegionName="StartOakVale"` then
`EnqueueAfterDummy` prove the **continue** arm
(`CurrentRegionIndex==4`). That assignment is **not**
a recovered New Game write. Do **not** invent it.
Do **not** wire it onto dummy Pump.

---

## 5. Besides `00501450` — later switch that *could* stay on 4

### 5a. `00502500` LoadRegionAtMap (PROVEN body; first-seen skip)

```
00502500  call [eax+64]               ; map handle
00502512  mov al, [0x13756F6]
          je  00502564               ; flag 0: NO 00500540
00502534  call 004FC190              ; dest map → region
0050253E  call 004FC190              ; other → region
          je  0050257F               ; same region: no load
0050254E  call 004FEEC0(old, 1)
0050255D  call 00500540(new, arg, 1) ; stay on new
```

`e8.tsv` dest `0x00502500` — **two**:

| Site | Real parent | First-seen |
|---|---|---|
| `004A4CB9` | `004A3740` WorldUpdate | `[esi+260]==0` → `004A5BF7 je 004A5C21` **skips** `004A5BFB` |
| `0089B99E` | `0089B780` Teleport `vtbl+1892` | same-region intro **jumps over** (`0089B918 jmp 0089BAE9` after `0049EAF0`) |

`functions.tsv` parent `00892D80` for `0089B99E` is
**over-merge**. Int3-bounded apply is `0089B780`.
`FirstSeenTeleportChangesRegion=false`. Cross-region
Teleport would take `0089B99E`; intro
`Hero.Teleport MK_OVI_ID_HERO` is **same-region**
and does **not**.

`004A4CB9` is after `004A4CD5 mov [ebp+260], 6`.
First-seen type-1 `[world+260]=0` never enters
`004A3740`. Who later writes `+260` and feeds a
StartOakVale **map** into `00502500`: **UNREAD**.

Flag `[0x13756F6]`: `abs.tsv` **three** reads
(`00502512` / `00502850` / `005063E0`), **zero**
stores. BSS first-seen 0 would skip both load arms.
Writer **UNREAD**.

### 5b. `005025B0` reload current (PROVEN unused as `E8`)

```
005025BD  mov ebp, [edi+156]
          …
005025EC  call 004FEEC0(current, 0)
005025F8  call 00500540(current, 0, 1)
```

Stays on **whatever is already current**. Cannot
**first** write 4. `e8.tsv` dest `0x005025B0`: **0**.

### 5c. `005063E0` map→region (PROVEN body; grouping lie)

```
005063E0  mov al, [0x13756F6]
          je  0050646F               ; skip 00500540
          …
0050640A  call 004FC190              ; map → region
00506455  call 00500540(ebx, arg, 1)
```

`005064C0` starts **after** this fn (`005064C0 sub esp,60`).
`00506455` is **not** villages. Older notes that hung
that site on `005064C0` are **DISPROVEN**.

`e8.tsv` dest `0x005063E0` — **three**:

| Site | Parent | No-save first-seen |
|---|---|---|
| `00487C91` | `00487C20` persist | **excluded** |
| `00488B16` | `00488AB0` player tick | first-seen `00A01B50` hero miss; dest on Lookout would be **1** |
| `00489F47` | later place (`[edx+48]` then load) | not first-seen; dest map **UNREAD** as Oakvale |

`00488AB0` is reached from `004A5A40` player walk.
First-seen miss skips `005063E0`. Even with a Lookout
hero, `004FC190` of that map is index **1**.

---

## 6. `00501450` itself — 0 inbound; 4 is transient (PROVEN body)

```
00501495  mov esi, [edi+156]         ; saved
005014A3  call 004FEEC0              ; +156=0
          i = 1
005014EC  call 00500540(i, 0, 0)     ; +156=i
          inc i … count-1            ; 142 → i=1..141
00501935  call 00500540(saved, 0, 1) ; no pump
```

`i=1` LookoutPoint — leftover **#4** first *real*.
`i=4` StartOakVale ContainsMap — **then overwritten**.
Last `004FC8A0` leaves `+156=141`
`Filler_NorthernWastes_02`. Restore first-seen
`saved=0` does not pump.

`e8.tsv` dest `0x00501450`: **0**. Host
`PumpCallsLoadFromFirstRealRegion=false`.
`LoadFromFirstRealRegionNamedInbound=0`. Dummy Pump
must **not** call it. Tests that call the API
explicitly are not first Present.

---

## 7. Childhood wait is not a writer (PROVEN)

`.text` `"StartOakVale"` `0x012D9D1C`: **two** sites,
both inside `00DBDE40`:

```
00DBDE49  push "StartOakVale"
          intern 0099EBF0
          mov ecx, [esi+64]
00DBDE69  call [eax+48]              ; vtbl+48 ready query
          neg / sbb / inc            ; wait while false
00DBDE7F  je  00DBDECA
00DBDE81  call [eax+28]              ; yield
          call 00CB7940              ; abort → ret
00DBDE9B  push "StartOakVale"
00DBDEB2  call [edx+48]
          jne 00DBDE81
00DBDECA  CREATURE_HERO_CHILD …
00DBE161  push "HerosOldHouse"       ; lookup vtbl+288, not +156
```

Zero `E8` `00500540` / `00487C20` / `004FC8A0` /
`00502500` in this fn. Sole `E8` of `00DBDE40`:
`00DAC295` in `00DABAC0` (S_QNOVI slot 2).
`StartOakValeSetupLoadsRegion=false`.

Context `vtbl+48` **body** (does it read `+156` vs a
name table): **UNREAD**. Treat as a ready predicate.
Do **not** satisfy it by inventing `PlayerRegionName`
or by jumping `00DBDE40` from Pump.

`.text` `"StartOakValeWest"`: **0**. `"HerosOldHouse"`:
one, the lookup above. Neither is a slot value.

---

## Dual leftover **#4** (do not collapse)

| Ledger | `+156` | First no-save Present? |
|---|---|---|
| Pump first Present | ctor **0**, `CurrentRegion=null`, empty `009DA9F0` skip, no `00501450` | **yes** |
| First *real* if unread `00501450` ran | loop starts **1** LookoutPoint; ends **141**; `i=4` transient | **not** Pump; **not** stay-4 |
| Childhood intro *view* | needs **stay** `+156=4` then `00DBDE40` wait hits | **no** |

Do **not** fold leftover **#50** (first-proximity TNG OOM)
into #4.

---

## Who would set index 4 — recovered order without persist

```
no-save Leave 0042F2A2
  005066E0  +156=0
  004189C2  dummy pumps
  no 00500540 / no 006C2671
  004A5BF7  [world+260]==0 skip 004A3740 / 00502500
  00487C20  empty skip
  00DBDE40  not constructed; would WAIT anyway
later UNREAD
  00501450  inbound 0                   // if recovered: transient 4, stay 141
  00502500 dest map whose 004FC190 == 4 // flag [0x13756F6]; callers skip first-seen
  005063E0 dest map whose 004FC190 == 4 // same flag; 00488B16 Lookout=1
006C2671 004FC8A0(4)                    // only if last 00500540 was (4, …, 1)
00DBDE40 vtbl+48("StartOakVale")        // then, and only then, wait returns
```

Stay-current without persist = last `00500540` is
`(4, *, 1)` with no later overwrite. That last caller
is **UNREAD**.

---

## Do not

- Write `PlayerRegionName = "StartOakVale"` on New Game.
- Treat `PlayerRegionNameWrittenOnNewGame` as anything
  but `false`.
- Call `LoadFromFirstRealRegion` / `00501450` /
  `00DBDE40` from dummy Pump.
- Treat `00501450 i=4` as childhood current.
- Treat `00501908 push 4` as index 4.
- Attribute `00506455` to villages `005064C0`.
- Attribute `0089B99E` to `00892D80`.
- Collapse leftover **#4**.
- Store `StartOakValeWest` / `HerosOldHouse` as `+156`.
- Invent `ActivateQuest` so the wait “has somewhere
  to land.”

---

## Locking tests (not edited)

- `Install_banks_and_startup_videos_exist` — first Pump
  dummy index 0; WLD index 4 is `StartOakVale`; after
  explicit `LoadFromFirstRealRegion`, current is **141**
- `Game_pump_is_004189C2_not_00DBDE40` —
  `GetCurrentRegionIndexFn=004FB150`, offset 156
- Dummy / second-pump tests —
  `PumpCallsLoadFromFirstRealRegion==false`
- `Second_pump_00501450_is_004FEEC0_then_00500540_1_0_0`
  — first `(1,0,0)`, last 141, no `00DBDE40`
- `Persist_PlayerRegionName_is_00487C20_not_new_game` —
  continue stand-in index 4;
  `PlayerRegionNameWrittenOnNewGame==false`;
  `StartOakValeSetupLoadsRegion==false`
- `No_save_does_not_activate_Q_NewOakValeIntro`
- `WorldSceneTests` `FirstSeenTeleportChangesRegion==false`
