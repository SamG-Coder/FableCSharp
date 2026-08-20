# `00DBDE40` after a proven activate: map-wait / AttackOver / 12s

Investigation only. No production `src/` or `tests/` edits.
Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** wire `00DBDE40` into `EngineLifecycle.Pump`.
Do **not** collapse leftover **#4** (Lookout first Present vs
Oakvale intro view).

Question: inside `00DBDE40`, after a **proven**
`00CB5AD0("Q_NewOakValeIntro")`, what is the order of
**map-wait**, **AttackOver**, and the **12 s wait** — and
what can the host implement from that order **without**
inventing the activate?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: `listing-00d80000.txt` (`00DAAC00` /
`00DAADA0` / `00DABAC0` / `00DAC295` / `00DBDE40`–
`00DBE2D3`; dump window is `00D80000`, not a separate
`listing-00db0000`);
`listing-00cc0000.txt` `00CDD450`;
`listing-00c80000.txt` `00CB7940`;
ExeIndex `calls-startoakvale-00dbde40` (1 hit:
`00DAC295`); `vtbl-s-qnovi-vtbl-012d7a28`;
`s-qnovi-slot4-00daada0`; `native-sqnovi.md`;
`00DBB2A7-attackover-store`; sibling
`00DBDE40-host-gap`;
`ScriptRuntime.ActivateQuest` /
`BindSqnoviFactory`;
`ScriptRuntimeParityTests.ActivateQuest_Oakvale_binds_S_QNOVI_without_region_or_raid`;
`EngineLifecycleTests.No_save_does_not_activate_Q_NewOakValeIntro`;
`docs/PARITY.md` “Who activates `Q_NewOakValeIntro`” /
“First-scene runtime”.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Only `E8` of `00DBDE40`? | **`00DAC295`** inside `00DABAC0` (`Q_NewOakValeIntro` slot 2). Zero `E8` of `00DABAC0`. VM calls `[vtbl+8]`. | **PROVEN** |
| Does activate run `00DBDE40`? | **No.** Activate constructs `00DBEF70` / `00DAAC00`. Fiber `00A446A0` then `vtbl+16` persist **then** `vtbl+8` `00DABAC0`, which map-waits inside `00DBDE40`. | **PROVEN** |
| Wait order inside `00DBDE40`? | **Map-wait first** (`vtbl+48 "StartOakVale"`). Then **AttackOver READ** `[this+80]`. Then kid + three watchers + PreAttack. Then **12 s** `vtbl+2592(1,&+76)` / `vtbl+2584(12.0)`. Then `HerosOldHouse`. Then **AttackOver SPIN**. | **PROVEN** |
| Is AttackOver persist bind inside `00DBDE40`? | **No.** Bind is `00DAADA0` `004045C0("AttackOver", this+80)` = vtbl **+16**. Store `+80=1` is **`00DBB2A7`**, after Theresa CS + raid AVI. | **PROVEN** |
| Are `WatchBarrels` / `WatchForGotGold` / `ManageQuestCoreMarkers` the activate? | **No.** They are **after** activate, **after** map-wait, **after** AttackOver early-out, **before** the 12 s wait. | **PROVEN** |
| Does host `ActivateQuest` load StartOakVale? | **No.** Binds factory / run / persist / `NOVI_*` names. Empty interpreters. AttackOver stays **false**. | **MATCH** bind |
| Does that bind run the wait order? | **No.** `StartFactory` stores VAs. `Update` resumes cutscenes, not slot 2. `TickNamedQuestMain` else-arm Notes `00CB7950` + `009D8650` only. | **PARTIAL** (data **MATCH**, fiber **not run**) |
| May the host invent `00CB5AD0("Q_NewOakValeIntro")` to reach this? | **No.** Activator is not on the no-save walk. | **DISPROVEN** as New Game first Present |

---

## Verdict

**After a proven activate, `00DBDE40` does not start
the 12 s wait, does not write AttackOver, and does
not load the region at the bind site.**

Order is locked by the listing:

```
map-wait "StartOakVale"     // 00DBDE49 / loop 00DBDE81
00CB7940 abort → ret
READ [this+80] AttackOver   // 00DBDED9; true → skip to PostAttack
CREATURE_HERO_CHILD
WatchBarrels     00DBE890   // after activate; after map-wait
WatchForGotGold  00DBE2E0
ManageQuestCoreMarkers 00DBE4E0
Q_NewOakValeIntro_PreAttack
vtbl+2592(1,&+76)
vtbl+2584(12.0f)            // 00DBE13E; blocking 12 s
HerosOldHouse
SPIN [this+80]              // 00DBE200; no mov here
PostAttack 00DBE3C0         // not first-seen
```

Host `ScriptRuntime.ActivateQuest("Q_NewOakValeIntro")`
already **MATCH**es the **bind without region**. Once a
**proven** activator exists, the host may implement this
**fiber continuation** — still without inventing the
activate, without writing `+80=1`, and without jumping
here from no-save `Pump`.

---

## Evidence → Original → Host → Gap

| Evidence | Original | Host | Gap |
|---|---|---|---|
| `calls-startoakvale` 1 hit `00DAC295` | Slot 2 `00DABAC0` is the only caller | `RegionTravel.IntroQuestRunCallsSetup` | **MATCH** VA. **PARTIAL**: host never `E8`s it |
| vtbl `012D7A28` `[2]=00DABAC0` `[4]=00DAADA0` | Fiber `00A446A0`: `vtbl+16` persist, loop `vtbl+8` run | `QuestFactoryTable` row + `StartFactory(00DBEF70, 00DABAC0, 00DAACE0, S_QNOVI)` | **MATCH** record. Fiber body not scheduled |
| `00DAADA0` `004045C0("AttackOver", this+80)` default 0 | Persist **bind** before `00DABAC0` | `BindSqnoviFactory` → `Persist.InstallRecovered()`; first-seen **false** | **MATCH** bind. Not a store |
| `00DBDED9` `mov al,[esi+80]` / `jne 00DBE21E` | After map-wait: if AttackOver already true, skip kid/12 s | `FirstSeenPlus80WrittenInStartOakVale=false`; `ApplyPersist` is save/load | **MATCH** “no write here”. Host has no READ-after-map-wait |
| `00DBB2A7` `mov [ecx+80],1` | Later S_QNOVI after raid AVI | `PersistTable.AttackOverStore` | **MATCH** writer VA. **DISPROVEN** as 12 s / map-wait writer |
| `00DBDE49` `"StartOakVale"` `call [eax+48]` | Region-ready query. `neg/sbb/inc` → loop while **not** ready, yield `vtbl+28` | `ActivateQuest` comment: “does not load StartOakVale”. Test: no region / no raid | **MATCH** “bind without region”. Map-wait itself **not implemented** |
| `00DBE128` `vtbl+2592(1,&+76)` then `push 0x41400000` `vtbl+2584` | 12 s **after** map-wait, AttackOver READ, kid, three watchers, PreAttack | `PreAttackDuration=12f` / `ScriptWaitVtbl=2584` constants | **MATCH** constants. **PARTIAL**: no blocking wait |
| `00DBDF5D` / `00DBDFDE` / `00DBE056` + `00CDD450` | Watchers **after** activate and **after** map-wait | `WatchBarrelsCallback` etc. constants; `ScriptFactoryTable` `NOVI_Barrel` is TNG, not this | **MATCH** VAs. **DISPROVEN** as activate. Host does not construct them from bind |
| `00CB7940` `[this+44]` then `[eax+5]` | True → `ret` from `00DBDE40` (abort). Not the map-wait predicate | Dump name “hero-exists”; host unused on this path | **PROVEN** control. Semantic of `+44` **PARTIAL** |
| `ActivateQuest_Oakvale_binds_S_QNOVI_without_region_or_raid` | Native construct does not NewRegion | Factory / Run / ScriptName set; `NamedScripts` `NOVI_*`; `Interpreters` empty; AttackOver false | **MATCH** bind-without-region |
| `No_save_does_not_activate_Q_NewOakValeIntro` | `00CD6E27` is `00CB5C90` bind only | `ActivatedQuests` omit the name; Gameflow **waits** | **PROVEN** no-save. Activator **UNREAD**. Do not invent |
| `TickNamedQuestMain` else-arm | Native `00A44880` → `00A446A0` → `00DABAC0` → `00DBDE40` | Note `00CB7950` + `009D8650` yield | **PARTIAL** (generic fiber Note, not this body) |
| leftover **#4** / `LoadFromFirstRealRegion` | First no-save region is Lookout index **1**. Map-wait wants **StartOakVale** (WLD region 4) | `FirstSceneWorld` / `StartNewGame` fixture soup | **LEFTOVER**. Do not collapse into this fiber |

---

## 1. Proven activate is **not** this function

`00DBDE40` has **one** `E8`: `00DAC295` in `00DABAC0`.
`00DABAC0` has **zero** `E8` callers. Slot map:

```
012D7A28
  [0] 00DBEFA0  dtor
  [1] 00DAACE0  Main watcher 00CDD450 / 00CDD440
  [2] 00DABAC0  run  → E8 00DBDE40
  [3] 00DAADD0  reset; mov [esi+80], bl   // clear, not store 1
  [4] 00DAADA0  persist AttackOver
```

Fiber `00A446A0`: `call [vtbl+16]` (`00DAADA0`) **once**,
then loop `call [vtbl+8]` (`00DABAC0`) until object `+5`.

`00DAADA0` (listing):

```
00DAADA0  push ecx
00DAADA6  add ecx, 80
00DAADAE  push "AttackOver"
00DAADB3  mov [esp+15], 0
00DAADB8  call 004045C0
00DAADBE  ret 4
```

That is the persist **bind**. Default byte 0. Not a wait.
Not inside `00DBDE40`.

`00DABAC0` **before** `00DAC295`:

1. `00CB8230` name table `NOVI_LiveFather` … `OVI_DeadFather`
   (factories at record `+16`). **Before** map-wait.
2. `00CB8930` flush; `vtbl+256`.
3. **READ** `[esi+80]`. If true: `00CB7940`; on abort
   **skip** `00DBDE40`; else name `Q__OakValeIntro_PostAttack`.
4. Objective `TEXT_QUEST_OAKVALE_INTRO_OBJECTIVE_01`.
5. Watcher `StartBarrelTimer` callback `00DB4F70`.
6. `mov ecx, esi; call 00DBDE40`.

Kid watchers are **not** here. They are the next function.

Activator `00CB5AD0` / `004B4A10` remains **UNREAD** on
no-save. Host must not call `ActivateNamedQuest` for this
name from Leave / `Pump` / `user.ini`. `user.ini` is
`ActivateQuest("Gameflow")` only.

---

## 2. Map-wait (first wait)

```
00DBDE49  push "StartOakVale"
00DBDE69  call [eax+48]          ; [esi+64] context
00DBDE6C  mov bl, al
00DBDE6E  neg bl / sbb bl, bl / inc bl   ; bl = !ready
00DBDE7F  je 00DBDECA            ; already ready → skip loop
00DBDE81  call [eax+28]          ; yield
00DBDE8B  call 00CB7940
00DBDE92  jne 00DBE2CE           ; abort → ret
00DBDE9A  push "StartOakVale"
00DBDEB2  call [edx+48]
00DBDEC8  jne 00DBDE81           ; still not ready
```

`00CB7940` is **not** the wait condition. True → **return**.
Dump name is hero-exists (`[this+44]` then `[eax+5]`).
Map-wait is **only** `vtbl+48("StartOakVale")`.

Host bind **MATCH**es “without region”:
`BindSqnoviFactory` registers names and persist; it does
not `NewRegion` / `LoadFromFirstRealRegion` /
`E8 00DBDE40`. That is correct at activate time. Native
also does not load the map inside `00CB5AD0`. The fiber
**yields** until the region is current.

Implementable **after** a proven activate: a slot-2 fiber
that **yields** while current region ≠ `StartOakVale`.
Do **not** satisfy this by loading Oakvale from
`ActivateQuest` or from no-save first Present (Lookout).

---

## 3. AttackOver READ (not a wait, not a write)

Immediately after map-wait + abort check:

```
00DBDED9  mov al, [esi+80]
00DBDEDF  jne 00DBE21E           ; already true → PostAttack
```

First-seen: byte is 0 from `00DAADA0` / ctor zeros /
slot 3 clear. Path falls through.

`00DAADD0` writes `[esi+80]=bl` with `bl=0` (reset).
No `mov [esi+80], 1` in `00DBDE00–00DBF000`
(`FirstSeenPlus80WrittenInStartOakVale=false`).
Writer is `00DBB2A7` after `CS_OAKVALE_INTRO_THERESA`
and PlayAVI `1_raid_on_oak_vale_comp.xmv`.

So AttackOver in this function is:

| Site | Role |
|---|---|
| `00DAADA0` (before `00DBDE40`) | bind name ↔ `+80` |
| `00DAC158` in `00DABAC0` | READ; true may skip the call |
| `00DBDED9` | READ; true skips 12 s / kid |
| `00DBE1F3` / `00DBE217` | SPIN READ after 12 s + house |
| `00DBB2A7` | WRITE 1 (later; not this fn) |

Host **MATCH**: `PersistBool("AttackOver")==false` after
`ActivateQuest`. Gap: no post-map-wait READ and no spin.

`PersistStore.cs` header still says “Writer UNREAD”.
That comment is **LEFTOVER** versus
`PersistTable.AttackOverWriterKnown` / `00DBB2A7`.

---

## 4. Kid + three watchers (after activate, after map-wait)

Only on AttackOver **false**:

```
00DBDEFB  push 0x40000000        ; 2.0f
00DBDF00  call [edx+1488]        ; fade (same vtbl as FadeOut)
00DBDF08  "CREATURE_HERO_CHILD"
00DBDF24  call [eax+280] / [edi+376]
00DBDF3D  call 004AA840          ; CString dtor, not spawn
```

Then three 60-byte `00CDD450` objects (ctor itself
`push 0x3DCCCCCD; push 64; push 1` = 0.1f / 64 / 1),
vtbl `0x012D7A3C`, parent `esi`:

| Name | `[+52]` callback | Attach |
|---|---|---|
| `WatchBarrels` | `00DBE890` | `00CB7E50` |
| `WatchForGotGold` | `00DBE2E0` | `00CB7E50` |
| `ManageQuestCoreMarkers` | `00DBE4E0` | `00CB7E50` |

`00CB7E50` + empty `0x122D70E`. These are **after**
activate. `NOVI_Barrel` TNG factory `00DB7D00` is a
different start (`ScriptFactoryTable`); do not conflate.
`ManageQuestCoreMarkers` names later `NOVI_*` — do not
follow off first-seen StartOakVale.

Then `vtbl+2792(46)`, intern
`Q_NewOakValeIntro_PreAttack`, `vtbl+1104` (activate
quest by name on context), yield `vtbl+28`, `00CB7940`.

---

## 5. 12 s wait (second wait)

```
00DBE128  lea eax, [esi+76]
00DBE12B  push eax
00DBE12C  push 1
00DBE12E  call [edx+2592]
00DBE139  push 0x41400000        ; 12.0f
00DBE13E  call [edx+2584]
```

Blocking wait. **After** map-wait. **After** AttackOver
early-out. **After** kid + watchers + PreAttack.
**Before** `HerosOldHouse` (`00DBE15E`) and the
`+80` spin (`00DBE200` yield + `00CB7940` until
`[esi+80]`).

`vtbl+2584` also hits `00DBE425` (PostAttack) — later.
First-seen wait is `00DBE13E`.

Host already stores `PreAttackDuration = 12f` /
`ScriptWaitVtbl = 2584`. It does not block 12 s on
activate, and must not fake `+80=1` when the wait
returns (`ScriptRuntime.Update` / `ApplyPersist` do
not invent that writer).

---

## 6. Host: what is already MATCH vs what may be added

### MATCH today (keep; this is the “bind without region”)

`ScriptRuntime.ActivateQuest` when
`QuestFactoryTable.Find("Q_NewOakValeIntro")` hits:

- `StartFactory(00DBEF70, 00DABAC0, 00DAACE0, "S_QNOVI")`
- `BindSqnoviFactory`: `Persist.InstallRecovered()` +
  `RegisterNamedScript` for recovered `NOVI_*`
- AttackOver **false**
- **no** `StartOakVale` load
- **no** `E8 00DBDE40`
- **no** cutscene interpreters (Father / Theresa / DeadDad)
- **no** raid AVI

Test:
`ActivateQuest_Oakvale_binds_S_QNOVI_without_region_or_raid`.

No-save `Pump` still omits the name
(`No_save_does_not_activate_Q_NewOakValeIntro`). Keep
that. Gameflow `00893610` **waits**; it is not this fiber.

### PARTIAL — implementable **once activate is proven**, without inventing it

Ordered. Stop if the activator is still UNREAD.

1. **Do not call activate.** Wait for a dump `E8`/`vtbl`
   of `004B4A10` / `00CB5AD0("Q_NewOakValeIntro")` that
   is **not** no-save first Present.
2. **Fiber slot 2**, not `ActivateQuest` itself:
   persist bind (already MATCH) then `00DABAC0` name
   table (already MATCH as `RegisterNamedScript`) then
   **map-wait**.
3. **Map-wait**: yield `vtbl+28` until context
   `vtbl+48("StartOakVale")` is ready. Abort if
   `00CB7940`. Do **not** load the region inside
   `ActivateQuest`.
4. **READ** AttackOver. If true, skip to PostAttack
   (`00DBE3C0`) — not first-seen.
5. Kid lookup `CREATURE_HERO_CHILD`. Three `00CDD450`
   watchers (`00DBE890` / `00DBE2E0` / `00DBE4E0`).
6. `Q_NewOakValeIntro_PreAttack` then **12 s**
   `vtbl+2592` / `vtbl+2584(12.0f)`.
7. `HerosOldHouse`, then **spin** on `+80`. Writer
   stays `00DBB2A7`.
8. Gameflow peer `00893610` should then see the live
   active name — separate from this fiber
   (`ResumeGameflowWait` still Notes `"… 0"`).

`ScriptRuntime.Update` already resumes **cutscene**
interpreters after TNG construct (`NOVI_LiveFather` →
`00DB86B0` → `00CBFB7D`). That is **after** names + map
+ thing construct. It is **not** a substitute for
map-wait / 12 s / `+80` spin.

### Do not implement

- `ActivateQuest("Q_NewOakValeIntro")` from `Pump` /
  `RequestNewGame` / Leave / `user.ini`.
- `E8 00DBDE40` from `ActivateQuest` / `BindSqnoviFactory`.
- Region load as a side effect of the bind.
- `mov [this+80],1` inside a host `00DBDE40` analog.
- Collapsing leftover **#4** / `FIRST_SCENE_*` /
  `FirstSceneWorld` into this fiber.
- Treating `StartNewGame` / `InstallRecoveredBindings`
  as live New Game construct.
- Following `ManageQuestCoreMarkers` / PostAttack
  `00DBE3C0` / maze `00DBEB20` off first-seen.

---

## Timeline (after a *proven* `00CB5AD0`, still no invented activate)

```
00CB5AD0("Q_NewOakValeIntro")          // ACTIVATOR UNREAD on no-save
004BB720 / 004B3CE0
00DBEF70 alloc 0x10C
00DAAC00 vtbl 012D7A28  [+64]=ctx
00CB7900 vtbl+12 then vtbl+4
slot1 00DAACE0 Main 00CDD450 / 00CDD440
00A447D0 fiber
00A446A0
  vtbl+16 00DAADA0 AttackOver bind 0     // BEFORE 00DBDE40
  vtbl+8  00DABAC0
    00CB8230 NOVI_* names                // BEFORE map-wait
    StartBarrelTimer 00DB4F70
    E8 00DBDE40                          // only caller
      map-wait StartOakVale              // WAIT 1
      READ +80                           // skip body if 1
      CREATURE_HERO_CHILD
      WatchBarrels / GotGold / Markers   // AFTER activate
      PreAttack
      vtbl+2584(12.0)                    // WAIT 2
      HerosOldHouse
      SPIN +80                           // WAIT 3; write is 00DBB2A7
      00DBE3C0 PostAttack                // not first-seen
```

No-save New Game never enters this timeline. **PROVEN**.
Keep it that way until the activator is a dump site.

---

## Do not

- Invent `ActivateQuest("Q_NewOakValeIntro")`.
- Call `00DBDE40` from `Pump` / first `004189C2`.
- Load `StartOakVale` from the Oakvale bind.
- Write AttackOver true from map-wait or from the 12 s return.
- Collapse leftover **#4**.
- Treat the three kid watchers as the activate.
