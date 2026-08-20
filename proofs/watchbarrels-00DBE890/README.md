# `00DBE890` WatchBarrels callback

Investigation only. No production `src/` edits.

Do **not** invent smash physics, animation events, or a
health check inside this callback. `00DBE890` **polls**
quest `+116`. It does **not** walk barrel things after
the initial name collect.

Do **not** treat `00DBE4E0` (`ManageQuestCoreMarkers`) as
this body. ExeIndex `fn=` / `watchbarrels-00dbe890` walk
can start at the wrong prologue. Listing prologue is
`00DBE890`.

Do **not** spawn a beetle on first-seen. Host
`FirstSeenWatchBarrelsSpawnsBeetle = false`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER**.

Authority: `listing-00d80000.txt` `00DBDE40` /
`00DBE890`–`00DBEB16` / `00DB7D00` / `00DB7DB0` /
`00DAEA70` / `00DAAD70` / `00DAADA0` / `00DAADD0`;
`listing-00cc0000.txt` `00CDD450` / `00CDEE00`;
`assembly/exe/00-index/vtbl.tsv` `0x012D7A28` /
`0x012D94EC`; `src/Fable.Game/RegionTravel.cs`
`WatchBarrels*`; `ScriptFactoryTable.BarrelFactory`;
`proofs/novi-factory-starts`; `proofs/00DBDE40-host-gap`.

---

## Verdict

`WatchBarrels` is a 60-byte fiber attached in
`00DBDE40` after `CREATURE_HERO_CHILD`. Callback
`00DBE890` runs with **quest** `this` (not the
watcher). It waits until context `vtbl+300("NOVI_Barrel")`
returns a positive count, then yield-loops on
`[quest+116]`. Each rising edge increments a **local**
smash counter (`edi`). Thresholds fire deed / gold /
beetle. The byte at `+116` is written **1** by
`NOVI_Barrel` script `00DB7DB0` (`vtbl+20`). Who
**calls** that slot is **UNREAD**. Not physics, not
anim, not this callback.

| Claim | Class |
|---|---|
| Attach site is `00DBDE40` `00DBDF4B`–`00DBDFA9` | **PROVEN** |
| Name `"WatchBarrels"`, ctor `00CDD450`, vtbl `0x012D7A3C`, callback `[+52]=00DBE890`, owner `[+56]=quest` | **PROVEN** |
| Ctor args `0.1f` / 64 / 1 (`RegionTravel.WatchBarrelsInterval/Capacity/Arg2`) | **PROVEN** |
| Fiber `00A446A0` `[watcher vtbl+16]` = `00DAAD70` → `ecx=[+56]`, `call [+52]` | **PROVEN** |
| Callback `this` is `S_QNOVI` (`[esi+64]` context) | **PROVEN** |
| Collect is context `vtbl+300` name `"NOVI_Barrel"` into 12-byte records | **PROVEN** |
| Smash count is `edi` in this frame, not a persist dword | **PROVEN** |
| Smash edge is `[quest+116] != 0` then clear | **PROVEN** |
| Writer of `+116=1` is `00DB7DB0` | **PROVEN** |
| `00DBE890` detects smash via physics | **DISPROVEN** |
| `00DBE890` detects smash via anim event | **DISPROVEN** |
| `00CDEE00` event-1 thunk writes `+116` | **DISPROVEN** (dtor `vtbl+0(1)`) |
| `00DB7E10` is smash detect | **DISPROVEN** (radius 2.0 + break-barrels text) |
| `00DBE4E0` is this callback | **DISPROVEN** |
| First smash → `00DAEA70(0)` | **PROVEN** |
| Smash `== N-1` → `OBJECT_GOLD_1` at `vtbl+288("NOVI_Barrel")` then `vtbl+2340` | **PROVEN** |
| Smash `> N-4` → `CREATURE_OAKVALE_STAG_BEETLE` as `NOVI_CreatedBeetle` | **PROVEN** as later branch |
| First-seen reaches beetle spawn | **DISPROVEN** (`FirstSeenWatchBarrelsSpawnsBeetle`) |
| Fixed barrel count in this fn | **UNREAD** (`N = (end-begin)/12`) |
| Caller of `00DB7DB0` / barrel `vtbl+20` | **UNREAD** |

---

## 1. Relation to `00DBDE40` watchers

`00DBDE40` after `CREATURE_HERO_CHILD` (`00DBDF06`)
allocates three 60-byte objects (`push 60` /
`00BFEA1A`) and `00CDD450` each:

| Order | Name | `[+52]` callback | Host const |
|---|---|---|---|
| 1 | `"WatchBarrels"` | `00DBE890` | `WatchBarrelsCallback` |
| 2 | `"WatchForGotGold"` | `00DBE2E0` | `WatchForGotGoldCallback` |
| 3 | `"ManageQuestCoreMarkers"` | `00DBE4E0` | `ManageQuestCoreMarkersCallback` |

Shared plant (`00DBDF81` / `00DBDFFD` / `00DBE075`):

```
[edi]    = 0x012D7A3C     ; RegionTravel.WatchBarrelsVtbl
[+52]    = callback
[+56]    = esi            ; quest
00CB7E50 attach
```

`00CDD450` (`listing-00cc0000`): `push 0x3DCCCCCD`
(`0.1f`) / `push 64` / `push 1` / `00A44740`. Interval
is the **fiber** period, not a timer inside `00DBE890`.

`00A446A0` calls watcher `vtbl+16`. Quest table
`0x012D7A28` slot 9 at `0x012D7A4C` is `00DAAD70`
(watcher vtbl starts at stored `0x012D7A3C`, so
`+16` = `00DAAD70`):

```
00DAAD70  mov ecx, [esi+56]     ; quest
          call [esi+52]         ; 00DBE890
          mov [esi+5], 1
```

Then `00DBDE40` continues: `Q_NewOakValeIntro_PreAttack`,
`vtbl+2584(12.0)`, `HerosOldHouse`, spin `[this+80]`.
`WatchBarrels` is **parallel**, not that wait.

`StartBarrelTimer` (`00DAC22B`, callback `00DB4F70`) is
**before** `00DBDE40`. Not this fn.

`WatchForGotGold` `00DBE2E0` waits context `vtbl+508`
`> 2`, then `TEXT_QUEST_OAKVALE_INTRO_OBJECTIVE_03`.
Gold spawn in `00DBE890` (`OBJECT_GOLD_1`) is a later
input to that watcher. **PARTIAL** coupling (no shared
store in this listing).

`ManageQuestCoreMarkers` `00DBE4E0` looks up
`NOVI_LiveFather` / `NOVI_BookTrader` / `NOVI_Theresa` /
`HUD_ORB_QUEST_CORE`. Later intro. Do not follow off
first-seen (`RegionTravel` comment).

---

## 2. Callback body (`00DBE890`)

`esi = ecx` (quest). Context `[esi+64]`.

### Wait until barrels exist

```
0099EBF0 "NOVI_Barrel"
context vtbl+300(name, &vector)     ; 00DBE8CF
setle bl  if eax <= 0
0099EAE0
if bl:
  loop: vtbl+28 yield, 00CB7940, retry vtbl+300
```

`00CB7940` is hero-exists (`[this+44]` then `[hero+5]`).
True → free the 12-byte vector and `ret`. **PROVEN**
abort, not spawn.

`vtbl+300` vs `vtbl+288`: this wait uses `+300` and a
vector. Gold branch uses `+288` (single). `eax` is a
signed count (`test` / `setle`). Record stride **12**
(`0x2AAAAAAB` signed `/12` at `00DBE94E`).

### Poll loop

```
N   = (vec_end - vec_begin) / 12     ; ebx
[esi+116] = 0
edi = 0
loop 00DBE973:
  if [esi+80]  → 008AC970 vector dtor, ret     ; AttackOver byte
  if [esi+116] == 0 → yield vtbl+28, 00CB7940, loop
  inc edi
  [esi+116] = 0
  if edi == 1     → 00DAEA70(0), yield
  if edi == N-1   → gold
  if edi >  N-4   → beetle
  yield, 00CB7940, loop
```

`[esi+80]` is the same offset persist binds as
`AttackOver` (`00DAADA0` `004045C0("AttackOver", this+80)`).
Here it is a **byte** test. Store of `1` is later
(`00DBB2A7`), not this fn.

`edi` is the smashed **count**. Not written back to the
quest. **PROVEN** local.

### First smash — `00DAEA70`

```
00DAEA70  inc [esi+88]
          fld [0x143E90C+3428]; fchs
          context vtbl+624
          if [+88]==1 && [+84]==0:
            TEXT_QST_048_SCRMSG_DID_FIRST_BAD_DEED
            vtbl+460, wait vtbl+160, TEXT_QST_LOG_BASICS_MAP
            [esi+252] = 1
          else if [esi+252]==0:
            TEXT_QST_048_SCRMSG_DID_BAD_DEED
```

Arg from WatchBarrels is `push 0`, so the `+252` flag is
on the quest. `[+88]` is a deed counter (also used by
Guard wander). Morality float at `vtbl+624` is
**PARTIAL** (sign-flip only; table slot unread).

### Smash `N-1` — gold

`vtbl+288("NOVI_Barrel")` then `vtbl+2340("OBJECT_GOLD_1")`.
Then `004AA840` string dtor. **PROVEN** names + vtbl
indices. Spawn body **UNREAD**.

### Smash `> N-4` — beetle

`vtbl+364(…, "NOVI_CreatedBeetle", [esi+118],
"CREATURE_OAKVALE_STAG_BEETLE", 0)` then
`vtbl+1064(&[esp+60], 2.0f, 1)`. `[esi+118]` is the
12-byte copy from the smash writer. **PROVEN** strings.
Create body **UNREAD**. Stag, not wasp
(`novi-factory-starts`). First-seen does not smash, so
does not spawn.

Yield is context `vtbl+28` (`RegionTravel.ScriptYieldVtbl`).

---

## 3. `+116` smash writer

### Reader / clearer in `00DBE890`

```
00DBE960  mov [esi+116], 0
00DBE97E  mov al, [esi+116]
00DBE98D  mov [esi+116], 0     ; after inc edi
```

### Writer of `1` on this quest

`NOVI_Barrel` factory `00DB7D00` (`ScriptFactoryTable.BarrelFactory`,
`00DABAC0` `00DAC045`). 28-byte object, vtbl `0x012D94F0`,
`[+20]=quest`, `[+8]` thing ref (`004ABE90`). Release
record: `[0]=1`, `[4]=00CDEE00`, `[8]=object`.

`00CDEE00`: `push 1; call [vtbl+0]` — dtor. **DISPROVEN**
as `+116` writer.

Barrel object vtbl (stored `0x012D94F0`; `vtbl.tsv`
`0x012D94EC`):

| disp | VA | Role |
|---|---|---|
| +0 | `00DB7DF0` | dtor |
| +4 | `00DB7E10` | start: `00CBE2FF` r=`2.0` then `TEXT_QST_048_INSTRUCTION_BREAK_BARRELS[_PC]` |
| +8 | `00CDEBB0` | `ret` |
| +12 | `00DB7DA0` | `return [this+20]` (quest) |
| +16 | `00CDEBC0` | `ret 4` |
| +20 | `00DB7DB0` | smash notify |
| +24 | `00CDEBE0` | `ret` |

`00DB7DB0` (**PROVEN** `+116=1` writer):

```
ecx = [esi+20]              ; quest
al  = 1
[ecx+116] = al
[ecx+117] = al
eax = [esi+8].vtbl+24       ; thing-ref
copy 12 bytes to [quest+118]
```

No `.text` `E8` to `00DB7DB0` (`functions.tsv` callees
empty except inner `[eax+24]`). Invocation is
`call [vtbl+20]` from an **UNREAD** site. Do **not**
name that site physics, collision, anim-event, or
script opcode.

Sibling scripts leave slot `+20` as `00CDEBD0` (`ret`).
Barrel **overrides** it.

`00DAADD0` reset zeros `+117`, not `+116`. Persist
`00DAADA0` binds only `"AttackOver"` at `+80`.

Other `mov [reg+116], 1` hits (`00DC0D31` BanditCamp,
`00F25779` ArenaCell, `00E6C807` chapel) are **other**
quests. **DISPROVEN** as this writer.

---

## 4. Host constants (lock, do not grow)

From `RegionTravel.cs` / tests, already matching listing:

| Const | Value | Listing |
|---|---|---|
| `WatchBarrelsCtor` | `00CDD450` | `00DBDF7C` |
| `WatchBarrelsCallback` | `00DBE890` | `[edi+52]` |
| `WatchBarrelsVtbl` | `0x012D7A3C` | `[edi]` |
| `WatchBarrelsIntervalBits` | `0x3DCCCCCD` | ctor `push` |
| `WatchBarrelsCapacity` | 64 | ctor `push 64` |
| `WatchBarrelsArg2` | 1 | ctor `push 1` |
| `WatchBarrelsThing` | `"NOVI_Barrel"` | `00DBE89A` |
| `FirstSeenWatchBarrelsSpawnsBeetle` | false | beetle is `edi > N-4` |

Do **not** add a smash-physics helper from this note.

---

## Classifications (short)

1. **`00DBE890` is the WatchBarrels poller — PROVEN.**
   Name collect `vtbl+300`, yield `vtbl+28`, edge on
   `+116`, local `edi` count.
2. **Attach is `00DBDE40` watcher 1 of 3 — PROVEN.**
   Same ctor as gold / markers. Callback via `00DAAD70`.
3. **Smash detect is not in this fn — DISPROVEN**
   physics / anim / `00DB7E10` / `00CDEE00`.
4. **`+116=1` writer is `00DB7DB0` — PROVEN.**
   Caller of barrel `vtbl+20` **UNREAD**.
5. **Deed / gold / beetle are count thresholds — PROVEN**
   as branches. First-seen does not take them.

---

## Next UNREAD

**Who calls `NOVI_Barrel` `vtbl+20` (`00DB7DB0`).**

That call is the smash detector. Scan `call [reg+20]`
sites that pass this 28-byte script object (or a
generic thing-script dispatcher that uses slot 5).
Until that `E8`/`FF` site is listed, do **not** write
physics, anim-event, or breakable-object health.

Follow-ons, not this gap: context `vtbl+300` /
`vtbl+2340` / `vtbl+364` bodies; runtime `N` from TNG
`ScriptName=NOVI_Barrel` (dump `ScriptName` is `NULL`
on `OBJECT_BARREL_BREAKABLE`).
