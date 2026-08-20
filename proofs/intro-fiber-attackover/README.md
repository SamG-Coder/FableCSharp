# Intro fiber leftovers after Father-only cutscene start

Investigation only. No production `src/` or `tests/` edits.

Do **not** start `CS_OAKVALE_INTRO_THERESA` /
`CS_OAKVALE_INTRO_THERESA_MEET` / `CS_DEAD_DAD` at
construct. Do **not** write `[quest+80]=1` from
`StartNewGame` / `ApplyPersist`. Do **not** treat
persist bind `00DAADA0` as the store. Do **not**
collapse the 12 s `vtbl+2584` wait into
`WaitActiveDialog`.

Question: after Father-only cutscene start
(`NOVI_LiveFather` → `00DB86B0` →
`00CBFB7D("CS_OAKVALE_INTRO_FATHER")`), what is still
leftover on the intro fiber?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority:

- Listings `listing-00a40000.txt` (`00A44690` /
  `00A446A0`), `listing-00d80000.txt` (`00DAADA0` /
  `00DABAC0` / `00DAC295` / `00DBDE40` / `00DBE128` /
  `00DBB2A7` / `00DB86B0` / `00DB97A0` / `00DB8300`),
  `listing-00cc0000.txt` (`00CC656B`)
- ExeIndex `microthread-fiber-entry-00a446a0`,
  `attackover-persist-004045c0`,
  `calldisp-vtbl2584-wait-intro`,
  `calls-s-qnovi-run-00dabac0` (0 `E8`),
  `calls-microthread-fiber-00a446a0` (0 `E8`),
  `native-sqnovi.md`, `0481-cs-oakvale-intro-father.md`
- `assembly/exe/00-index/vtbl.tsv` `0x012D7A28`
- Host `ScriptRuntime.ApplyPersist` /
  `ConstructStartsCutscene` / `WaitActiveDialog`
- Siblings `proofs/00DBB2A7-attackover-store`,
  `proofs/sqnovi-yield-resume`,
  `proofs/00DBDE40-host-gap`,
  `proofs/speak-vtbl1472`
- Tests `First_seen_interpreters_are_only_father_cutscene`,
  `ActivateQuest_Oakvale_binds_S_QNOVI_without_region_or_raid`,
  `WaitActiveDialog_leftover_polls_interactive_handle`

---

## Verdict

Father-only construct starts `CS_OAKVALE_INTRO_FATHER`.
That does **not** finish `S_QNOVI`. After that start
the quest fiber is still in `00DABAC0` → `00DBDE40`:
12 s `vtbl+2584(12.0)`, `HerosOldHouse`, spin
`[this+80]` (`AttackOver`, still 0). Inside the
Father runner the leftover is `WaitActiveDialog`
(`00CC656B` poll `vtbl+1472`). Theresa / DeadFather
must not start.

| Question | Answer | Class |
|---|---|---|
| Fiber entry? | `00A446A0` `[this+16]` then loop `[this+8]` until `+5`; park `00A44690` → `009D8650` | **PROVEN** |
| Does `00A446A0` call persist `00DAADA0`? | **No.** Fiber `this` is the watcher; `+16` is `00DAAD70` | **DISPROVEN** |
| Persist bind? | S_QNOVI `vtbl+16` = `00DAADA0` `004045C0("AttackOver", this+80)` | **PROVEN** bind |
| Is `00DAADA0` the `+80=1` write? | **No.** Bind only; first-seen stays 0 | **DISPROVEN** |
| Slot 2 run? | `00DABAC0` (0 `.text` `E8`); first enter via `00CDD440` `jmp [S_QNOVI.vtbl+8]` | **PROVEN** |
| After name table? | `00DAC295` `E8 00DBDE40` (only caller) | **PROVEN** |
| 12 s wait? | `00DBE128` `vtbl+2592(1,&+76)` then `vtbl+2584(0x41400000)` | **PROVEN** |
| `+80` spin in `00DBDE40`? | `00DBE1F3`–`00DBE21C` `vtbl+28` until `[esi+80]!=0`; no `mov` here | **PROVEN** wait; store **DISPROVEN** here |
| Who writes `+80=1`? | `00DBB2A7` after Theresa CS + raid AVI | **PROVEN** later; **LEFTOVER** vs first-seen |
| Father-only CS at construct? | `00DB86B0` `00CBFB7D("CS_OAKVALE_INTRO_FATHER")` | **PROVEN** |
| Theresa at construct? | `00DB97A0` first named `M_TriggerOutro`; first `00CBFB7D` is MEET later | **DISPROVEN** |
| DeadFather at construct? | `00DB8300` later `007E73F0("CS_DEAD_DAD")`, not `00CBFB7D` | **DISPROVEN** |
| Host Father-only start? | `ConstructStartsCutscene` true only on `NOVI_LiveFather` | **MATCH** |
| Host `WaitActiveDialog`? | one leftover yield; dismiss **UNREAD** | **MATCH** leftover; **PARTIAL** UI |
| Host 12 s / `+80` spin? | none; persist stays false | **LEFTOVER** vs native fiber; first-seen false **MATCH** |

---

## Evidence

Dumps. Absolute paths under
`C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\`.

### `00A446A0` fiber / `00A44690` yield

`text-map/listing-00a40000.txt`. 0 `E8` of
`00A446A0` (`calls-microthread-fiber-00a446a0`).
Create stores the VA as the fiber body
(`00A44773 mov ecx, 0xA446A0` / `009D8710`).

```
00A44690  call 009D8650                 // park
00A446A0  …
00A446CE  movzx eax, [fiber+5]
00A446D7  jne  00A44719                 // done
00A446EF  call [eax+16]                 // setup once
00A44714  call [edx+8]                  // until +5
00A44717  jmp  00A446CE
00A44726  call 009D8650                 // idle park
```

Watcher `this` (Main `00CDD450`, vtbl `012D7A3C`):
`+16 = 00DAAD70`, `+8 = 00A44840`. S_QNOVI
`0x012D7A28` slot 4 (`+16`) is persist
`00DAADA0`; slot 2 (`+8`) is run `00DABAC0`.
Different `this`. Sibling
`proofs/sqnovi-yield-resume`. **PROVEN**.

### `00DAADA0` persist bind (not store)

`listing-00d80000.txt` int3-bounded
`00DAADA0`–`00DAADBE`:

```
00DAADA0  push ecx
00DAADA1  lea  eax, [esp+3]
00DAADA6  add  ecx, 80
00DAADA9  push ecx
00DAADAE  push "AttackOver"
00DAADB3  mov  [esp+15], 0              // seed 0
00DAADB8  call 004045C0
00DAADBE  ret  4
```

Reset `00DAADD0` clears `[esi+80]`.
Helper `004045C0` copies/binds the byte; it does
not `mov 1`. **PROVEN** bind.

### `00DABAC0` run then `00DBDE40`

`listing-00d80000.txt`. 0 `.text` `E8`
(`calls-s-qnovi-run-00dabac0`). First name:

```
00DABAE3  push "NOVI_LiveFather"
00DABB04  call 0099EC30
00DABB0C  mov  [edi+16], 0xDAC2C0
00DABB20  call 00CB8230
```

Then `NOVI_Theresa` `00DAC420`, `NOVI_Guard`,
villagers, `OVI_DeadFather` `00DB81B0`, … Names
before map-wait. Tail:

```
00DAC293  mov  ecx, esi
00DAC295  call 00DBDE40                 // only E8
00DAC2A1  ret
```

### 12 s `vtbl+2584` + `+80` spin

`calldisp-vtbl2584-wait-intro`: two hits in
`00DBDE00`–`00DBF000` (`00DBE13E` first-seen,
`00DBE425` post-attack — do not follow).

```
00DBE0C6  push "Q_NewOakValeIntro_PreAttack"
00DBE0E0  call [eax+1104]               // start quest
00DBE128  lea  eax, [esi+76]
00DBE12C  push 1
00DBE12E  call [edx+2592]               // flag ptr
00DBE139  push 0x41400000               // 12.0f
00DBE13E  call [edx+2584]               // blocking wait
00DBE15E  push "HerosOldHouse"
…
00DBE1F3  mov  al, [esi+80]
00DBE1F8  jne  00DBE21E
00DBE200  call [eax+28]                 // yield
00DBE20A  call 00CB7940                 // hero-exists
00DBE217  mov  al, [esi+80]
00DBE21C  je   00DBE200                 // spin AttackOver
00DBE22D  call 00DBE3C0                 // PostAttack — later
```

No `mov [esi+80]` in this range.
`FirstSeenPlus80WrittenInStartOakVale=false`.
**PROVEN**.

### Store is later Theresa + raid AVI

`listing-00d80000.txt` / sibling
`00DBB2A7-attackover-store`:

```
00DBB21A  push "CS_OAKVALE_INTRO_THERESA"
00DBB238  call 00CBFB7D
00DBB248  push "Data\Video\1_raid_on_oak_vale_comp.xmv"
00DBB260  call [edx+1476]
00DBB2A7  mov  [ecx+80], 1              // AttackOver
```

Not first-seen. Not the 12 s wait. **PROVEN** later.

### Father-only construct

`00DB86B0` (not dtor `00DB8680`) looks up Hero /
Father then `00CBFB7D("CS_OAKVALE_INTRO_FATHER")`.
TNG `CREATURE_HERO_FATHER` / `NOVI_LiveFather`.
`ConstructStartsCutscene=true` only on that row.

Theresa `00DB97A0` first named work:

```
00DB9812  push "M_TriggerOutro"
```

First `00CBFB7D` is MEET `00DB9B28`; raid THERESA
is `00DBB238`. DeadFather `00DB8300`:

```
00DB8478  push "CS_DEAD_DAD"
00DB84A5  call 007E73F0                 // not 00CBFB7D
```

**DISPROVEN** as construct cutscenes.

### Father leftover `WaitActiveDialog`

`0481-cs-oakvale-intro-father.md` after
`VILL1.WalkTo` / `GamePause 0.8`:

```
WaitActiveDialog
UseCamera CAM_OVIF_SHOT3
```

Token `00CC656B`. No session → `00CC7081`. Else:

```
00CC6612  mov  ecx, [0x143E8F8]
00CC661D  call [eax+1472]               // 008907D0
00CC6623  test al, al
00CC6625  jne  00CC65C6                 // leftover vtbl+28
00CC6627  jmp  00CC7081
```

First-seen has an InteractiveSpeak / DialogSpeak
handle so one leftover. Dismiss body **UNREAD**.
Not the 12 s wait (different `this`, different vtbl).

---

## Original

After a proven `Q_NewOakValeIntro` construct (not
no-save Leave):

```
00DAAC00  ctor  size 0x10C  vtbl 012D7A28
  +8  00DABAC0   slot 2 run
  +16 00DAADA0   persist AttackOver @ +80 = 0
  +24 00A44880   pump
00DAACE0  Main watcher 00CDD450 / callback 00CDD440
00A447D0  fiber 00A446A0
  [watcher+16] 00DAAD70
    call [esi+52] = 00CDD440
      jmp [S_QNOVI.vtbl+8] = 00DABAC0     // FIRST run
00DABAC0
  00CB8230 NOVI_LiveFather 00DAC2C0
           NOVI_Theresa    00DAC420
           … OVI_DeadFather 00DB81B0
  00DAC295 E8 00DBDE40                    // map-wait
TNG construct (names already registered)
  NOVI_LiveFather 004C97B0 → 00DAC2C0
    fiber 00DB8630 [+52].vtbl+4 = 00DB86B0
      00CBFB7D("CS_OAKVALE_INTRO_FATHER")  // FATHER-ONLY START
  NOVI_Theresa 00DB97A0  M_TriggerOutro    // no CS
  OVI_DeadFather 00DB8300                  // no 00CBFB7D
00DBDE40  (still on 00DABAC0 stack)
  map-ready + 00CB7940
  CREATURE_HERO_CHILD
  00CDD450 ×3
  Q_NewOakValeIntro_PreAttack
  vtbl+2592(1,&+76)  vtbl+2584(12.0)       // LEFTOVER after CS start
  HerosOldHouse
  spin [this+80] until AttackOver          // still 0
00CBFB7D  Father def+60
  … InteractiveSpeak / DialogSpeak …
  GamePause 0.8
  WaitActiveDialog  00CC656B leftover 1472 // LEFTOVER in runner
  UseCamera CAM_OVIF_SHOT3
later  00DBB2A7  [quest+80]=1              // after Theresa + raid
```

Yield while `00DABAC0` is parked is context
`vtbl+28` = `006E7410` → fiber `vtbl+8` =
`00A44840` → `00A44690`. Resume is `00A44880` →
`00A44660` → `009D87F0`. That pump is **not** a
re-enter of `00DABAC0` via `[S_QNOVI.vtbl+8]`.

The 12 s wait and the `+80` spin are **on the
quest fiber**, concurrent with / after Father CS.
`WaitActiveDialog` is **inside** Father CS. Three
leftovers, two objects.

---

## Host

`ScriptRuntime.StartNewGame` →
`InstallRecoveredBindings` + `ActivateThings`.
`BindSqnoviFactory` / `ActivateQuest` register the
`NOVI_*` table and persist `AttackOver=false`.
`StartNamedScript` starts a cutscene only when
`ConstructStartsCutscene`.

| Host | Native | Class |
|---|---|---|
| `PersistTable.AttackOverWrite = 00DAADA0` | persist bind | **MATCH** |
| `ApplyPersist("AttackOver", …)` | save/load of `+80` | **MATCH** API; first-seen value **false MATCH** |
| `AttackOverStore = 00DBB2A7` | later `mov 1` | **MATCH** VA; **LEFTOVER** vs this start |
| `ScriptFiberTable` `S_QNOVI` + `AttackOver` | persist association | **MATCH** name; fiber `this` is **not** `00DAADA0` |
| `IntroQuestRun = 00DABAC0` | slot 2 | **MATCH** constant |
| `StartOakValeSetup = 00DBDE40` | map-wait body | **MATCH** constant; **not executed** |
| `PreAttackDuration = 12f` / vtbl 2584 | `00DBE139` | **MATCH** data; **LEFTOVER** runtime |
| `FirstSeenPlus80WrittenInStartOakVale=false` | no `mov` in wait | **MATCH** |
| `ConstructStartsCutscene` Father only | `00DB86B0` | **MATCH** |
| Theresa / DeadFather `false` | `M_TriggerOutro` / `007E73F0` | **MATCH** skip |
| `WaitActiveDialog` YieldOnce + `Count++` | leftover `00CC661D` | **MATCH** leftover; dismiss **UNREAD** |
| `Update` resumes Father interpreter | `00A44880` analog for CS | **PARTIAL**; no `00DBDE40` |

`First_seen_interpreters_are_only_father_cutscene`
locks one interpreter, Theresa/DeadFather not
started, `WaitActiveDialogCount==0` at construct.
Pump to `GamePause 0.8` then one leftover
`WaitActiveDialog`; still Father-only.

`ActivateQuest("Q_NewOakValeIntro")` binds names
and persist **without** `E8 00DBDE40`, region, or
raid. Interpreters empty until TNG construct.

`PersistStore` comment “Writer UNREAD” is stale vs
`AttackOverWriterKnown=true`. The **store VA** is
known; the **first-seen wait** still does not write.

---

## Gap

Leftovers **after** Father-only start. Do not close
them by starting Theresa, writing `+80=1`, or
calling `00DBDE40` from Pump.

| Leftover | Native | Host | Class |
|---|---|---|---|
| 12 s wait | `00DBE13E` `vtbl+2584(12.0)` after PreAttack | constants only; `Update` does not wait 12 s | **LEFTOVER** |
| `+80` spin | `00DBE200` yield until AttackOver | persist false; no spin | first-seen **MATCH**; wait **LEFTOVER** |
| `+80=1` | `00DBB2A7` after Theresa + raid AVI | must not write here | **DISPROVEN** as this leftover |
| `WaitActiveDialog` dismiss | `008907D0` / `006E5660` until `al==0` | one yield; no UI | **PARTIAL** / **UNREAD** |
| `00DBDE40` map-wait / hero-child / three watchers | after names, before 12 s | `StartNewGame` skips | **LEFTOVER** (see `00DBDE40-host-gap`) |
| Fiber `00A446A0` → persist `00DAADA0` | PARITY shorthand | `ScriptFiberTable` pairs the names | **DISPROVEN** same-`this`; persist is S_QNOVI `+16` |
| Theresa / DeadFather CS | later starts | host skips at construct | **MATCH** omit |

Do **not**:

- `StartCutscene("CS_OAKVALE_INTRO_THERESA")` or
  `"CS_DEAD_DAD"` from `ActivateThing`.
- `ApplyPersist("AttackOver", true)` to leave the
  12 s / `+80` wait.
- Treat host `WaitActiveDialog` as the 12 s
  `vtbl+2584` wait.
- Re-enter `00DABAC0` from `[S_QNOVI.vtbl+8]` after
  a yield (`proofs/sqnovi-yield-resume`).
- Invent `00DBDE40` on no-save Leave first Present
  (`proofs/00DBDE40-host-gap`).

---

## Sources (absolute)

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00a40000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00d80000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00cc0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\script-runtime\microthread-fiber-entry-00a446a0-00a446a0.md`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\script-runtime\calldisp-vtbl2584-wait-intro-00000a18.md`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\script-bank\native-sqnovi.md`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\script-bank\0481-cs-oakvale-intro-father.md`
- `C:\FableCSharp\assembly\exe\00-index\vtbl.tsv`
- `C:\FableCSharp\proofs\00DBB2A7-attackover-store\README.md`
- `C:\FableCSharp\proofs\sqnovi-yield-resume\README.md`
- `C:\FableCSharp\proofs\00DBDE40-host-gap\README.md`
