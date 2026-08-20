# Who constructs `Q_NewOakValeIntro` / `S_QNOVI` on no-save so `00CDD440` can `jmp 00DABAC0`?

Investigation only. No production `src/` edits.

Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** collapse catalog (`world+184` / `QM+44`) with
construct (`004B3CE0` kind `0x37`). Do **not** collapse
Gameflow wait (kind `0x33` Give, `00893570`) with
construct. Do **not** treat bind `00CD6E27` /
`00CB5C90` as ctor. Do **not** collapse Gameflow
`00CDD440` `jmp [vtbl+8]` onto `S_QNOVI` `00DABAC0`.

Question: remaining unread E8 of `004B4A10` /
`004B4260` / `004B2850` / `004AF610` / `00CB5C90`
— who, on no-save, presents intern `0x012C5D14`
so `00CB5AD0` hits, factory `00DBEF70` runs,
`00DAAC00` writes vtbl `0x012D7A28`, and Main
`00CDD440` can `jmp [eax+8]` into `00DABAC0`?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: ExeIndex `e8.tsv` / `calls-by-dest.tsv` /
`ff.tsv` dests below; `xrefs.tsv` /
`xrefs-by-string.tsv` intern `0x012C5D14`;
`listing-00480000.txt` (`004B4A10` / `004B4260` /
`004B2850` / `004AF610` / `004B3CE0` / `004B2890` /
`004A1101` / `0049EA40` / `0049EAC0` / `0049F24E`);
`listing-00c80000.txt` (`00CB5AD0` / `00CB5C90` /
`00CB7210`); `listing-00cc0000.txt` (`00CD52D0` /
`00CD6E27` / `00CD6E6D` / `00CDD440` / `00CE7670`);
`listing-00d80000.txt` (`00DAAC00` / `00DBEF70` /
`00DABAC0`); siblings
`proofs/q-novi-activator-callers`,
`proofs/00CB5AD0-remaining-presenters`,
`proofs/012C5D14-fablecrc-imm`,
`proofs/cactivatequestdef-payloads`,
`proofs/sqnovi-first-construct`,
`proofs/00DAAC00-sqnovi-no-save`,
`proofs/oakvale-activate-unread-audit`.

Do **not** re-prove as new: Gameflow `00CE7670`
state 0 waits `vtbl+100` `00893570` for type-`0x33`
Give on `[world+96]`, not construct `0x37`; unique
`00CB5AD0` `E8` is `004B42E8` in `004B4260`; Init
Quests walks `world+172` TRUE only; `Q_NewOakValeIntro`
is FALSE catalog `world+184` / `QM+44`;
`CActivateQuestDef` 16-byte payloads never intern
`0x012C5D14`; `script.bin` 0 intern hits; `0049EAC0`
0 inbound; `0061AB30` leftover not New Game;
`005E7B77` leftover; `user.ini` Gameflow not Oakvale;
`00DBE295` Give AFTER AttackOver/PostAttack/Maze
needs `S_QNOVI` already constructed.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Who constructs `S_QNOVI` on no-save so `00CDD440` can `jmp 00DABAC0`? | **Nobody.** | **PROVEN** omit |
| `004B4A10` body intern? | **None.** Arg0 is a caller `CString*`. No `push 0x012C5D14`. | **PROVEN** |
| Remaining unread E8 of the five dests that could present this name on no-save? | **None.** Full census below. | **PROVEN** |
| `xrefs.tsv` `0x012C5D14` extra site into `004B4A10` / `004B4260` / `00CB5AD0`? | **No.** Five sites: bind + Gameflow card/wait. | **PROVEN** |
| `00CB5C90` constructs? | **No.** Map insert `00CB7210`. All 161 E8 live in Registering Scripts. | **DISPROVEN** |
| `004B2850` constructs? | **No.** `vector<CString>::push_back` at `QM+44`. Unique E8 `004A1101` (AddQuest). Catalog only. | **DISPROVEN** |
| `004AF610` constructs? | **No.** `QM+56` name-is-present, `al` 0/1. Twelve E8, all query. | **DISPROVEN** |
| First no-save `00CDD440` dest? | Gameflow `vtbl+8` = **`00CE7670`**, not `00DABAC0`. | **PROVEN** |
| Invent `ActivateQuest("Q_NewOakValeIntro")`? | **No.** | **DISPROVEN** |

---

## Verdict

**PROVEN nobody on no-save.**

The construct chain is closed and unique:

```
presenter 004B4A10 / 004B4260
  → 004B00C0 (QM+44 membership; this name is already in)
  → 00CB5AD0  UNIQUE E8 004B42E8
  → 004B3CE0  UNIQUE E8 004B4386   // factory 00DBEF70
  → 00DBEF70  0 E8                 // call [record+4]
  → 00DAAC00  UNIQUE E8 00DBEF8B   // mov [esi], 0x12D7A28
  → Main 00DAACE0  +52 = 00CDD440, +56 = S_QNOVI
  → type-1 00DAAD76 call [esi+52]
  → 00CDD440  jmp [eax+8]          // 012D7A28+8 = 00DABAC0
```

On no-save New Game the name never enters the
12-byte queue. `00DAAC00` does not run. No live
object holds vtbl `0x012D7A28`. `00CDD440` therefore
cannot `jmp 00DABAC0`. First no-save `00CDD440` is
Gameflow’s `00CE7670` wait.

Every previously unread dest in the assigned search
set is now classified. There is **no remaining
unread no-save presenter**. Later construct after a
region exists stays **UNKNOWN** and must not be
filled with `ActivateQuest("Q_NewOakValeIntro")`.

---

## Status table

| Claim | Class | Evidence |
|---|---|---|
| Eight `E8` of `004B4A10` | **PROVEN** | `e8.tsv` / `calls-by-dest.tsv` |
| Six `E8` of `004B4260` | **PROVEN** | same |
| One `E8` of `004B2850` = `004A1101` | **PROVEN** | same |
| Twelve `E8` of `004AF610` | **PROVEN** | same |
| 161 `E8` of `00CB5C90`, all grouping `00CD4FD0` | **PROVEN** | same; real fn `00CD52D0` |
| Unique `00CB5AD0` `E8` = `004B42E8` | **PROVEN** (KNOWN) | `e8.tsv` 1 row |
| `00DBEF70` / `00DABAC0` / `00CDD440` `E8` | **PROVEN** 0 | `e8.tsv` 0; `calls-by-dest` no dest row for factory/run/thunk |
| Unique `00DAAC00` `E8` = `00DBEF8B` | **PROVEN** | grouping `00DB8680`; real factory `00DBEF70` |
| Unique `004B3CE0` `E8` = `004B4386` in `004B4260` | **PROVEN** | grouping `004B2510` |
| `ff.tsv` dest of those VAs | **PROVEN** 0 as abs dest | `ff.tsv` has `mem`/`disp`, not these VAs |
| `004B4A10` intern | **PROVEN** none | listing: `[esp+36]` = arg0 `CString*` |
| Five intern pushes `68 14 5D 2C 01` | **PROVEN** | bind `00CD6E27`/`00CD6E86`; wait `00CE791D`/`00CE7977`/`00CE79C9` |
| `00CB5C90` Oakvale site `00CD6E6D` is bind | **PROVEN** | `[esp+32]=0xDBEF70`; then `00CB7210` |
| `004B2850` = `QM+44` catalog push | **PROVEN** | `lea esi, [ecx+44]`; `00433530` |
| `004AF610` = `QM+56` is-present | **PROVEN** | `mov ebx, [ecx+56]`; `al` 0/1; `ret 4` |
| No-save `004B4260` list includes Oakvale | **DISPROVEN** (KNOWN) | `world+172` TRUE only |
| `0049EAC0` inbound | **DISPROVEN** (KNOWN) | 0 `E8` |
| `0061AB30` New Game | **DISPROVEN** leftover (KNOWN) | `[this+343]` picker, `world+196` |
| `005E7B77` first Oakvale Give | **DISPROVEN** leftover (KNOWN) | mode-1 UI after `004B1D30` |
| `user.ini` Oakvale | **DISPROVEN** (KNOWN) | `"Gameflow"` |
| `00DBE295` constructs | **DISPROVEN** (KNOWN) | Give after childhood already ran |
| Host invents `ActivateQuest` | **DISPROVEN** | Notes skip; test `No_save_does_not_activate_*` |

---

## 1. `004B4A10` body — what intern does it take?

`listing-00480000.txt`, int3 `004B4A09`–`004B4A0F`,
`ret 12` at `004B4A96`:

```
004B4A10  sub esp, 12
          push ebp / esi / edi
          push 1
          push 1
          mov esi, ecx                  ; QuestManager [0x13B89FC]
          mov ecx, [esp+36]             ; arg0 CString*
          lea eax, [esp+36]             ; &end = &arg0 + 0 (1-name range)
          push eax / push ecx / push 0
          lea ecx, [esp+32]
          xor-clear 12-byte vector
          call 00433530                 ; 12-byte name list
          push [list+4] / push [list]
          lea ecx, [esp+20]
          push ecx
          mov ecx, esi
004B4A5A  call 004B4260
          … dtor list …
          ret 12
```

**PROVEN:** no PE intern. The name is **whatever
CString\*** the caller passed. Flags from the two
`push 1` + the two leftover stack flags are
`(1,1)` into `00433530`; `004B4A10` itself then
forwards that 1-name vector. None of the eight
callers push `0x012C5D14`.

---

## 2. Complete `E8` census (assigned dests)

`e8.tsv` dest column; `calls-by-dest.tsv` grouping
parent in the third column. Real fn is int3-bounded
(same grouping lie as `007EEF60` vs `007EF200`).

### `004B4A10` — 8 sites, 0 Oakvale intern

| `E8` | Grouping | Real fn | Name | No-save Oakvale? |
|---|---|---|---|---|
| `00416C11` | `00416953` | `00416BCF` arm | `[game+90584]` vs empty `0x122D70E` | **skip** (empty) |
| `004B4B5F` | `004B49E0` | `004B4AA0` | thing `0x6C` `+40` | needs live thing |
| `004B4D45` | `004B49E0` | sibling of `004B4AA0` | copy of same `+40`, flags `(1,1)` | same |
| `0061AC28` | `0061A6A0` | **`0061AB30`** leftover picker | `world+196` record | **DISPROVEN** New Game |
| `007EF3A1` | `007EEF60` | **`007EF200`** `CTCExpression` | nested `[esi+120]` copy | needs `0x8F`; persist ≠ intern |
| `0084407E` | `008421C0` | **`00843FC0`** action | `[this+168]` | needs thing / def `+40` |
| `00892E8F` | `00892D80` | **`00892E80`** vtbl+1104 | script/ini CString `(1,1)` | `"Gameflow"` |
| `00892ECF` | `00892D80` | **`00892EC0`** vtbl+1112 | same CString `(1,0)` | not Oakvale |

**PROVEN** none of these is a no-save presenter of
intern `0x012C5D14`.

### `004B4260` — 6 sites, 0 no-save Oakvale list

| `E8` | Grouping | Real fn | List | No-save Oakvale? |
|---|---|---|---|---|
| `0049EAD1` | `0049EA40` | **`0049EAC0`** | `ecx+0xAC` stub | **0 inbound `E8`** |
| `0049F24E` | `0049EF50` | **`0049F180`** Init Quests | `world+172` TRUE | name **absent** |
| `004B4A5A` | `004B49E0` | **`004B4A10`** | 1-name wrapper | eight callers, none intern |
| `004B5B84` | `004B5080` | save `END_ACTIVE_QUESTS` | streamed names | not no-save |
| `00892EAF` | `00892D80` | **`00892EA0`** slot 277 | caller CString | **0 `E8`**, **0 `ff` disp 1108** |
| `00892EEF` | `00892D80` | **`00892EE0`** slot 279 | caller CString | two `ff` disp 1116, both **after** Oakvale wait, fresco / `Q_GuildTraining` / `V_*` |

**PROVEN** omit. Gameflow `00CE7AE7` / `00CE7FEC`
never return on this walk.

### `004B2850` — 1 site, catalog not construct

```
004B2850  mov eax, [ecx+52]
          lea esi, [ecx+44]             ; QM+44
          … push_back arg CString …
          ret 4
```

Unique `E8`: `004A1101` in AddQuest `004A0D90`
(grouping `004A0850`). After always
`world+184` and TRUE-only `world+172`, **every**
AddQuest name is `004B2850`’d onto `QM+44`.
`Q_NewOakValeIntro` **is** in that catalog.

`004B4260` does **not** walk `QM+44`. It calls
`004B00C0` as a **membership find** on `+44`.
Init Quests never presents this name, so the
gate is never asked. `004B2890` walks `QM+112`,
empty skip `je 004B2989` on no-save.

**DISPROVEN** as construct. **PROVEN** as why a
later presenter would **take** `004B00C0`.

### `004AF610` — 12 sites, query not construct

```
004AF610  mov ebx, [ecx+56]             ; QM+56 live list
          … compare arg CString to [node+8]+48 …
hit:      mov al, 1 / ret 4
miss:     xor al, al / ret 4
```

| `E8` | Grouping | Real fn | Name | Role |
|---|---|---|---|---|
| `0049EA9E` | `0049EA40` | predicate `0049EA40` | `[esi+88]` | `test al`; **not** fall into `0049EAC0` |
| `004AFC79` | `004AFC60` | `004AFC60` | walk `[this+64]` names | returns `al`; used by the predicate |
| `004B3B18` | `004B2510` | `004B3CE0` | queued name | already-on-`+56` skip |
| `004B3D2A` | `004B2510` | `004B3CE0` | same | skip |
| `004B3E8C` | `004B2510` | `004B3CE0` | same | skip before factory |
| `004B44C8` | `004B4450` | `004B4450` | `[edi+8]` | query |
| `0061A91D` | `0061A6A0` | leftover picker | record name | leftover UI |
| `0061B10A` | `0061A6A0` | leftover picker | record name | leftover UI |
| `006C7086` | `006C6010` | expression UI | `"Expression_Steal"` `0x0125EADC` | **not** Oakvale |
| `006C70BA` | `006C6010` | expression UI | `"Expression_Picklock"` `0x0125EAC8` | **not** Oakvale |
| `008440A0` | `00844090` | after `00843FC0` | `[this+168]` | is-active after action |
| `00896A62` | `00892D80` | HUD `008969A0` arm | caller `esi` | leftover card |

**DISPROVEN** as construct. `004B3CE0` uses this
to **skip** already-built slots; it cannot be the
first writer of vtbl `0x012D7A28`.

### `00CB5C90` — 161 sites, bind not construct

All 161 `calls-by-dest` rows: dest `0x00CB5C90`,
fn **`0x00CD4FD0`** (over-merge). Real Registering
Scripts is **`00CD52D0`** (`sub esp, 24`). First
site `00CD5358`, last `00CDB31A` — still inside
that fn.

```
00CB5C90  copy 24-byte record
          … optional [ebx+20] push_back …
00CB5D0B  lea edi, [ebx+4]              ; map
          lea ebx, [esi+8]
          … copy name / factory / persist byte …
00CB5D47  call 00CB7210                 ; INSERT
          mov [edi+13], 1
          ret 8
```

Oakvale bind:

```
00CD6E14  push "S_QNOVI"
00CD6E27  push "Q_NewOakValeIntro"
00CD6E51  mov [esp+48], bl              ; persist 0
00CD6E55  mov [esp+32], 0xDBEF70
00CD6E6D  call 00CB5C90
```

**DISPROVEN** as ctor. Factory dword is stored,
not called. `00CB5AD0` later returns
`lea eax, [edi+4]` or 0.

---

## 3. Intern `0x012C5D14` xrefs (closed)

`xrefs.tsv` / `xrefs-by-string.tsv`:

| Site | Parent | Role |
|---|---|---|
| `00CD6E28` | `00CD5170` blob / bind | `00CB5C90` |
| `00CD6E87` | same | cleanup / second bind arm |
| `00CE791E` | `00CE7670` | `vtbl+1180` card |
| `00CE7978` | `00CE7670` | `vtbl+100` Give-wait |
| `00CE79CA` | `00CE7670` | same wait loop |

**Zero** into `004B4A10` / `004B4260` / `00CB5AD0` /
`004B2850` / `004AF610`. Listing bytes `14 5D 2C 01`
are those five `push` only. FableCrc of the name
(`0x02C878A8`) is not a `.text` imm
(`proofs/012C5D14-fablecrc-imm`).

---

## 4. No-save timeline — still no `00DAAC00`

```
00CD52D0  Registering Scripts
  00CD6E6D  00CB5C90 bind Q_NewOakValeIntro / S_QNOVI / 00DBEF70
            persist bl=0                     // NOT 00CB5AD0
004A0D90  FinalAlbion.qst
  AddQuest FALSE → world+184 + 004B2850 QM+44
  TRUE only → world+172                      // skip
  AddTestQuest → world+196 only              // 0061AB30 leftover
0049F24E  004B4260([world+172])
  004B42E8  00CB5AD0 per TRUE name            // Oakvale absent
  004B4386  004B3CE0                         // Sunnyvale … not 00DBEF70
0049F259  004B2890 empty QM+112 skip
00416C11  +90584 empty skip 004B4A10
user.ini  00892E80 004B4A10("Gameflow")
first type-1 00CB8220
  Sunnyvale 00CDD440 → 00CDD360
  Gameflow  00CDD440 → 00CE7670              // NOT 00DABAC0
    vtbl+1180 card
    vtbl+100 00893570 kind 0x33 miss → yield FOREVER
    00CE7AE7 / 00CE7FEC not reached
00DAAC00 / 00DABAC0 / 00DBDE40               // NOT ENTERED
00DBE295 Give                                // needs S_QNOVI already live
005E7B77 CTCQuestCompletionUI                // leftover, not first Give
```

---

## 5. What must be true for `S_QNOVI` to exist later

Catalog, bind, and factory are **already** in
place on no-save. The missing step is **one
presenter**:

1. Some later `004B4A10` / `004B4260` must put
   intern `0x012C5D14` (or a CString equal to it)
   on the 12-byte queue.
2. `004B00C0` **takes** — name is already in
   `QM+44` from AddQuest.
3. Unique `00CB5AD0` **hits** — bind row exists,
   `[record+0] = 00DBEF70`, persist `[+16] = 0`.
4. Unique `004B3CE0` persist-0 arm runs factory
   `call [eax+4]` / `004AFA10` (not the persist-1
   `call [edx+8]` that first-seen Sunnyvale uses).
5. `00DBEF70` `00BFEA1A(0x10C)` then unique
   `00DAAC00`. Ctor `00DAAC16`
   `mov [esi], 0x12D7A28` — **this** is the first
   write of live slot 2 = `00DABAC0`.
6. Main `00DAACE0` attaches watcher
   `+52 = 00CDD440`, `+56 = S_QNOVI`.
7. Type-1 clone `00DAAD70` `call [esi+52]` →
   `00CDD440` `jmp [eax+8]` = **first** `00DABAC0`.

Until (1) happens, (5)–(7) are unreachable.
`00DBE295` Give of this name is **after**
AttackOver inside already-ticking `S_QNOVI`
and cannot be (1). Gameflow wait is kind `0x33`,
construct posts `0x37`.

Later candidates for (1), all **off** no-save
Type-1 (`CurrentRegion=null`):

| Path | Copied CString | Equal intern on no-save? |
|---|---|---|
| `007EF200` `+120` | nested expression | **DISPROVEN** persist; live Thing **UNKNOWN** |
| `00843FC0` `+168` | def `+40` / ctor arg | six `CActivateQuestDef` rows **DISPROVEN**; live **UNKNOWN** |
| `004B4AA0` `0x6C+40` | component record | live **UNKNOWN** |
| debug `0061AB30` `world+196` | test card | **DISPROVEN** New Game |
| save `004B5B84` | streamed | **DISPROVEN** no-save |

Do **not** close (1) with
`ActivateQuest("Q_NewOakValeIntro")`.

---

## What this is not

| Claim | Class |
|---|---|
| Remaining unread `004B4A10` E8 on no-save | **DISPROVEN** (all eight classified) |
| Remaining unread `004B4260` E8 on no-save | **DISPROVEN** (all six classified) |
| `004B2850` / `QM+44` is construct | **DISPROVEN** |
| `004AF610` is construct | **DISPROVEN** |
| `00CB5C90` outside `00CD52D0` | **DISPROVEN** (161/161) |
| `0049EA40` presents to `00CB5AD0` | **DISPROVEN** (predicate; `test al`) |
| `006C7086` Oakvale | **DISPROVEN** (`Expression_Steal`) |
| `00CE7670` constructs `0x37` | **DISPROVEN** (KNOWN wait `0x33`) |
| Bind writes `00DABAC0` onto a live object | **DISPROVEN** |
| Host missing a recovered no-save caller | **DISPROVEN** |

---

## Host

`EngineLifecycle` Notes `00416BCF` skip and
`"004B4A10 not Q_NewOakValeIntro"`.
`ActivateNamedQuest` walks `world+172` only.
`No_save_does_not_activate_Q_NewOakValeIntro`.
Pump traces must not contain `Va==00DAAC00` or
`Va==00DABAC0` on no-save first Present.
**MATCH.**

Do **not** add `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** add a recovered-caller Note that pretends
`0061AB30` / `007EF200` / `00843FC0` / `00892EA0`
ran on this walk.

---

## Remaining UNKNOWN

Only **after** a region exists: first live Thing
whose copied `CTCExpression+120` / action `+168` /
`0x6C+40` CString **equals** intern `0x012C5D14`.
That is not no-save Type-1. The assigned E8 set
does not contain that presenter.

---

## Sources (absolute)

- `C:\FableCSharp\assembly\exe\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\calls-by-dest.tsv`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\ff.tsv`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00480000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00600000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-006c0000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00840000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00880000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00c80000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00cc0000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00d80000.txt`
- `C:\FableCSharp\assembly\exe\00-index\xrefs.tsv`
- `C:\FableCSharp\assembly\exe\00-index\xrefs-by-string.tsv`
- `C:\FableCSharp\assembly\exe\00-index\strings.tsv`
- `C:\FableCSharp\proofs\q-novi-activator-callers\README.md`
- `C:\FableCSharp\proofs\00CB5AD0-remaining-presenters\README.md`
- `C:\FableCSharp\proofs\012C5D14-fablecrc-imm\README.md`
- `C:\FableCSharp\proofs\cactivatequestdef-payloads\README.md`
- `C:\FableCSharp\proofs\sqnovi-first-construct\README.md`
- `C:\FableCSharp\proofs\00DAAC00-sqnovi-no-save\README.md`
- `C:\FableCSharp\proofs\oakvale-activate-unread-audit\README.md`
