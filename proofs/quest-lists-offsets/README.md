# Quest list offsets: `world+172` / `+184` / `QM+44` / activated

Investigation only. No production `src/` edits.

Question: map native offsets `world+172`, `world+184`,
quest-manager `+44`, and the activated list from listings
`004A1840` / `004B2890` / `004B3CE0` / `004B4260`. What
does “constructed but not activated” mean at the byte
level? Is `00DABAC0` reachable without `00CB5AD0`? Do
constructed quests tick?

Do **not** start `S_QNOVI` / `00DBDE40` / `Q_NewOakValeIntro`
as no-save New Game. That name is `AddQuest(..., FALSE)`
plus `AddTestQuest`. It is on `world+184` and `QM+44`.
It is **not** on `world+172` and **not** on the activated
list.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Authority: ExeIndex `listing-00480000.txt` (`004A1840` /
`004A0D90` / `004A08D0` / `004A1080` / `004A10C4` /
`004A1101` / `004A113B` / `0049F24E` / `004B2850` /
`004B2890` / `004B3CE0` / `004B3F17` / `004B4063` /
`004B4260` / `004B42E8` / `004B4386` / `004B4490` /
`004B4590`); `listing-00c80000.txt` `00CB5AD0`;
`listing-00d80000.txt` `00DABAC0` / `00DBEF70`;
`e8.tsv` (`00CB5AD0` unique site `004B42E8`;
`00DABAC0` / `00DBEF70` **0** `E8`); TLC
`FinalAlbion.qst` / `GlobalQuests.qst`;
`proofs/quest-manager-plus44`, `qst-clear-004A08D0`,
`004B00C0-first-gate`, `004B3CE0-factory0`,
`004B2890-first`, `factory0-type1-tick`,
`oakvale-later-activate`, `world-plus172-activate`;
host `EngineLifecycle` `_worldPlus172` /
`_worldPlus184` / `_questManagerPlus44` /
`_activatedQuests`;
`EngineLifecycleTests.Init_quests_004B4260_activates_wld_initial_list`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| `world+172`? | `CWorld` CString vector (`+172` begin / `+176` end / `+180` cap). **`AddQuest` TRUE only.** First no-save walk of `004B4260`. Host `WorldPlus172`. Head is `Q_SunnyvaleMaster`. Nine names after both QST files. | **PROVEN** |
| `world+184`? | Same world, next triple (`+184/+188/+192`). **Every `AddQuest` name** (TRUE and FALSE). Catalog only. Host `WorldPlus184`. Contains `Q_NewOakValeIntro` and `Gameflow`. | **PROVEN** |
| `QM+44`? | Third CString vector on `[0x13B89FC]` (`+44/+48/+52`). **Every `AddQuest`**, via `004B2850`. Gate `004B00C0` finds here. Not the `004B4260` walk. Host `QuestManagerPlus44`. | **PROVEN** |
| Activated list? | Native: circular 16-byte nodes at **`QM+56`**, each `[node+8]` → 52-byte slot. Filled by `004B3CE0` after `004B4260`. Host `_activatedQuests` is the name log of that walk plus later `ActivateQuest("Gameflow")`. | **PROVEN** |
| `004B2890` is the activated list? | **No.** It walks `QM+112` (empty persist boasts) then maybe `QM+56` (needs a player Thing). First-seen `je`s both. | **DISPROVEN** as the catalog / activate writer |
| “Constructed but not activated” for Oakvale? | **Misnomer.** `Q_NewOakValeIntro` is a **CString** on `+184` and `+44` (and a 28-byte `+196` test card). **No** 52-byte slot. **No** `00DAAC00`. Bind `00CB5C90` is a factory row, not an instance. | **PROVEN** store; **DISPROVEN** construct |
| Factory-0 on `+172`? | **Activated then stub-constructed.** `00CB5AD0` miss → `[rec+4]=0` → `004B4063` 52-byte slot with `[+8]=0` on `QM+56`. | **PROVEN** |
| Does a constructed slot tick? | **Only if `[slot+8] ≠ 0`.** `004B4490` `cmp [eax+8], 0` / `je 004B4549` skips stubs. Live factories `00CB8220`. Catalog-only names are not even visited. | **PROVEN** |
| `00DABAC0` without `00CB5AD0`? | **No dump-proven path.** Unique `E8` of `00CB5AD0` is `004B42E8`. `00DABAC0` has **0** `E8`. It is `S_QNOVI` `vtbl+8`, called at `004B3F20` `call [edx+8]` only after a **hit** factory `[eax+4]` (`00DBEF70`). Miss / absent name never reaches that pair. | **PROVEN** not reachable |

---

## Offset table

| Host field | Native slot | Object | Elem | Writer | First no-save consumer | First-seen Oakvale? |
|---|---|---|---|---|---|---|
| `WorldPlus172` | `CWorld+172/+176/+180` | world | CString ×4 | `004A10C4` if `AddQuest` TRUE | `0049F247` `lea edx,[esi+172]` → `004B4260` | **No** (FALSE) |
| `WorldPlus184` | `CWorld+184/+188/+192` | world | CString ×4 | `004A1080` every `AddQuest` | catalog; **not** `004B4260` | **Yes** (name only) |
| `QuestManagerPlus44` | `[0x13B89FC]+44/+48/+52` | QuestManager | CString ×4 | `004A1101` `004B2850` every `AddQuest` | `004B00C0` find (gate) | **Yes** (name only) |
| `ActivatedQuests` | **`QM+56`** circular 16-byte nodes → 52-byte slots | QuestManager | node 16 / slot 52 | `004B3CE0` after `004B4260` / `00CB5AD0` | type-1 `004B4490` / `004AF610` already-active | **No** |
| (no host list) | `CWorld+196/+200/+204` | world | struct ×28 | `AddTestQuest` `004A113B` | debug `0061AB30` / `004B4A10`, **not** New Game | card only |
| (not activated) | `QM+112` circular 40-byte | QuestManager | boast persist | ctor sentinel; load `004B07B7` | `004B2890` first walk | empty |

Do **not** collapse `+184` with `+44`. Same strings after one
`004A1840` parse; different objects. `004A08D0` clears
`+184/+172/+196` and **does not** touch `QM+44`.
`WorldPlus184 == QuestManagerPlus44` is a **host** equality
after one New Game parse (`Init_quests_004B4260_*`), not an
identity of pointers.

WLD `START_INITIAL_QUESTS` / `World.InitialQuests` (six names,
`Q_SunnyvaleMaster` first) is a **file table**. `00507C30` has
no that case. Init Quests walks QST TRUE `+172` (nine).
**DISPROVEN** as the writer of any row above.

---

## Timeline (no-save New Game)

```
004A1840  Load Quests                    // 00416ABA
  004A1931  push 1
  004A193C  004A0D90(FinalAlbion.qst, 1)
    004A08D0  clear +184 / +172 / +196
    AddQuest:
      004A1080  lea esi, [ebp+184]       // ALWAYS
      004A10B2  test bl, bl              // TRUE?
      004A10C4  lea esi, [ebp+172]       // TRUE only
      004A1101  004B2850 → QM+44         // ALWAYS
    AddTestQuest → +196 only             // no 004B2850
  004A1991  push 0
  004A199C  004A0D90(GlobalQuests.qst, 0) // append

0049F180  Init Characters / GUI / Quests
  0049F247  lea edx, [esi+172]
  0049F24E  004B4260                     // WALK +172
    each name:
      004B00C0  find in QM+44            // miss → skip
      004B42E8  00CB5AD0 [QM+120]        // UNIQUE E8
        hit  → 004BB720 factory
        miss → 004BB720 [rec+4]=0
    004B4386  004B3CE0                   // once
      factory ≠ 0 && [0x1375454]==1:
        call [eax+4]                     // e.g. 00DBEF70
        call [edx+8]                     // e.g. 00DABAC0
        52-byte live, [+8]=run, 00CB7900
      factory == 0:
        004B4063 52-byte stub, [+8]=0
      link 16-byte node on QM+56
  0049F259  004B2890                     // NOT a list fill
    [QM+112].next==head → skip
    no player Thing → skip +56

user.ini ActivateQuest("Gameflow")
  004B4A10 → 004B4260(one name) → 00CB5AD0
  // Gameflow is on +184 and +44, not +172
```

---

## 1. Four listings — who writes which list

### `004A1840` — fill catalogs, do not activate

`listing-00480000.txt` `"Load Quests"` then:

```
004A1931  push 1
004A193C  call 004A0D90          ; FinalAlbion.qst, flag 1 → 004A08D0
004A1991  push 0
004A199C  call 004A0D90          ; GlobalQuests.qst, append
```

`AddQuest` (`004A1080` / `004A10C4` / `004A1101`) writes
**names**. No `004B4260`. No `00CB5AD0`. No `004B3CE0`.
**PROVEN.**

Ctor zeros (`004A68AE`):

```
[world+172]=[world+176]=[world+180]=0
[world+184]=[world+188]=[world+192]=0
[world+196]=[world+200]=[world+204]=0
```

Push is `0099EC30` into `*end` then `add [esi+4], 4`.
Element is a 4-byte Lionhead CString pointer. **PROVEN**
(`qst-clear-004A08D0`).

### `004B4260` — walk `world+172`, gate `QM+44`, lookup `00CB5AD0`

```
004B4265  mov ebp, [esp+56]      ; arg0 = vector
004B4269  mov eax, [ebp+4]       ; end
004B4270  mov ecx, [ebp+0]       ; begin
          sar eax, 2             ; count
004B42D1  lea esi, [ecx+edx*4]
004B42D7  call 004B00C0          ; QM+44 find
004B42DE  je  004B4363           ; skip
004B42E4  mov ecx, [edi+120]
004B42E8  call 00CB5AD0          ; UNIQUE E8
          …
004B4386  call 004B3CE0          ; once, after the loop
          ret 12
```

Init Quests site is `0049F247` `lea edx,[esi+172]`. The
function **never** loads `[edi+44]` as the walk.
**PROVEN.**

### `004B3CE0` — construct slots onto `QM+56`

Arg is the 12-byte queue `004BB720` built in `004B4260`.
Second loop:

```
004B3EE4  cmp [edi+4], ebx       ; factory from 00CB5AD0
004B3EE7  je  004B4063           ; 0 → stub
004B3EED  mov al, [0x1375454]    ; first-seen .data 1
004B3EF4  je  004B4063
004B3F0B  mov eax, [edi+4]
004B3F17  call [eax+4]           ; factory ctor
004B3F1C  mov edx, [esi]
004B3F20  call [edx+8]           ; run / vtbl+8
          push 52 / 00BFEA1A
          [slot+8] = run
          00CB7900
          16-byte node → [QM+56]
```

Stub `004B4063`: same 52 / same `+56` link /
`[slot+4]=[slot+8]=0` / name at `+48` / **no**
`00CB7900`. **PROVEN** (`004B3CE0-factory0`).

### `004B2890` — not a catalog, not an activate

```
004B2890  mov eax, [esi+112]
          mov edi, [eax]
          cmp edi, eax
          je  004B2989           ; empty +112 first-seen
          … boast restore …
004B2989  00449970 / 00487DC0    ; player Thing
          je  004B2AC1           ; first-seen miss
          eax = [esi+56]         ; would walk activated slots
```

Does **not** read `QM+44` or `world+172`. Does **not**
`00CB5AD0`. First-seen is a no-op after the just-finished
`004B3CE0`. **PROVEN** (`004B2890-first`).

---

## 2. Host vs native after `EnterGame`

`Init_quests_004B4260_activates_wld_initial_list`:

| List | Contents | Oakvale |
|---|---|---|
| `WorldPlus172` | 9 TRUE names, `[0]=Q_SunnyvaleMaster` | absent |
| `WorldPlus184` | every `AddQuest` (TRUE+FALSE) | **present** |
| `QuestManagerPlus44` | same names as `+184` (one parse) | **present** |
| `ActivatedQuests` | `WorldPlus172` then `"Gameflow"` (count 10) | **absent** |

That is the host sentence: **`WorldPlus172` is
`Q_SunnyvaleMaster` (first); `WorldPlus184` contains
`Q_NewOakValeIntro`; `QuestManagerPlus44` contains it;
`ActivatedQuests` does not.** **PROVEN** as host lock;
**MATCH** native catalogs; activated host list **MATCH**es
`QM+56` **names** (9 + Gameflow) and **DIVERGE**s on
factory-0 fibers (`Started==false` on the two stubs, but
host still `CreateFiber`s them).

---

## 3. “Constructed but not activated” at the byte level

Three different byte states. Do not use one phrase for all.

### A. Catalogued, not constructed, not activated

`Q_NewOakValeIntro`, `Gameflow` (until `user.ini`), every
other `AddQuest` FALSE.

| Byte | Value |
|---|---|
| `world+184` | one CString (4-byte blob pointer from `0099EC30`) |
| `QM+44` | one CString copy (`004B2850`) |
| `world+172` | **absent** |
| `world+196` | Oakvale also has a 28-byte `AddTestQuest` card |
| `[QM+120]` map | bind row from `00CB5C90` (`00CD6E27`: name / `S_QNOVI` / `[rec]=00DBEF70`). **No alloc** |
| `QM+56` | **no node** |
| 52-byte slot | **none** |
| `00DAAC00` / `00DABAC0` | **not called** |

`0099EC30` constructing a **string** is not constructing a
quest. **PROVEN.**

### B. Activated, then stub-constructed (factory 0)

`ChapterAndSceneManager`, `NPCDeath`. On `world+172`.
`004B00C0` **takes**. `00CB5AD0` **miss** (`eax=0`).
`004BB720` stores `[rec+4]=0`. `004B3CE0` `je 004B4063`:

```
obj = 00BFEA1A(52)
[obj+0]  = id          ; not a vtbl
[obj+4]  = 0           ; no factory record
[obj+8]  = 0           ; no run object
[obj+36] = 1
[obj+37] = 1           ; Init Quests arg
[obj+48] = name
16-byte node → QM+56
```

They **are** activated (gate + enqueue + `+56` membership).
They are constructed as **stubs**. They are **not** live
scripts. **PROVEN.**

### C. Activated and live-constructed

`Q_SunnyvaleMaster` and the other TRUE factory hits, plus
later `Gameflow`.

```
[slot+0]  = id
[slot+4]  = factory record     ; 00CB5AD0 hit
[slot+8]  = run object         ; call [eax+4]
[slot+48] = name
00CB7900(run) → Main watcher
```

`004AF610` “already active” is a **name walk of `QM+56`**
(`[node+8]+48`). Catalog lists do not count. **PROVEN.**

---

## 4. Do constructed quests tick?

Type-1 `004A5A40` → `004A5D88` `004B4490`:

```
004B4522:
  mov eax, [edi+8]               ; 52-byte slot
  cmp [eax+8], ebx
  je  004B4549                   ; [slot+8]==0 → NEXT
  mov ecx, [eax+8]               ; this = run
  call 00CB8220
```

| Kind | On `QM+56`? | `[slot+8]` | `00CB8220`? | Tick? |
|---|---|---|---|---|
| Live factory | yes | run | yes | **yes** (watchers / `00CB7950`) |
| Factory-0 stub | yes | **0** | **no** (`je 004B4549`) | **no** |
| Catalog-only (Oakvale) | **no** | — | not visited | **no** |

First type-1: **10** `+56` visits, **8** `00CB8220`
(skip the two stubs). **PROVEN** (`factory0-type1-tick`,
`00CB8220-first-pump`).

Host `PumpQuestList` notes `00CB7950` on every
`_activatedQuests` name except the string `"Gameflow"`,
including the two stubs. **DIVERGE.**

Constructed **live** quests tick. Constructed **stubs** do
not. Stored names do not.

---

## 5. `00DABAC0` is not reachable without `00CB5AD0`

`e8.tsv`:

| dest | sites |
|---|---|
| `00CB5AD0` | **1**: `004B42E8` in `004B4260` |
| `00DBEF70` | **0** |
| `00DABAC0` | **0** |
| `00DAAC00` | **0** (called from `00DBEF70` body) |

`00DABAC0` is `S_QNOVI` `vtbl+8` (`0x012D7A28+8`). The only
dump-proven first call is `004B3F20` `call [edx+8]` after
`004B3F17` `call [eax+4]` (`00DBEF70` → `00DAAC00`).
That block is behind `cmp [edi+4], 0` / `je 004B4063`.
`[edi+4]` is the `00CB5AD0` result stored by `004B4260`.

Without a **hit**:

- name never in the `004B4260` arg (`Q_NewOakValeIntro` not
  on `+172`) → no queue record → no `004B3F17` / `004B3F20`
- name in the arg but `00CB5AD0` miss → `[rec+4]=0` → stub,
  no `[eax+4]`, no `[edx+8]`

Bind `00CD6E27` / `00CB5C90` writes the factory **pointer**
into the script-def map. It does not call it. **PROVEN.**

`00CB7780` VM-list start is skipped first-seen
(`[esi+17]==0`, `script-bank-open`). It is not a back door
onto `00DABAC0` on this walk. **PROVEN** skip.

Later fiber `00A446A0` can resume `00DABAC0` only after an
`S_QNOVI` instance already exists — i.e. after the same
`00CB5AD0` hit + `004B3CE0` pair. Gameflow `00CE7670` only
**waits** on the name (`vtbl+100`). Debug `0061AB30` can
later `004B4A10` the `+196` card, which is still
`004B4260` → `00CB5AD0`. **PROVEN** that every construct
path still goes through that unique `E8`.

**Verdict: `00DABAC0` is not reachable without a prior
`00CB5AD0` hit for `Q_NewOakValeIntro`.** No-save New Game
never does that hit. **PROVEN.**

---

## What this is not

| Claim | Class |
|---|---|
| `QM+44` *is* `world+184` | **DISPROVEN** (third buffer; `004A08D0` skips `+44`) |
| `004B4260` walks `QM+44` or `+184` | **DISPROVEN** (walks arg; Init Quests = `+172`) |
| `004B2890` activates / constructs | **DISPROVEN** |
| Being on `+184` / `+44` constructs `S_QNOVI` | **DISPROVEN** |
| `Q_NewOakValeIntro` is on first `004B4260` | **DISPROVEN** |
| Factory-0 names are skipped (no 52-byte alloc) | **DISPROVEN** |
| Factory-0 stubs `00CB8220` / `00CB7950` | **DISPROVEN** |
| `00DABAC0` `E8` from Leave / `004A1840` / bind | **DISPROVEN** |
| `00CB5AD0` from user.ini directly | **DISPROVEN** (ini is `004B4A10` → `004B4260`) |
| WLD `InitialQuests` is `world+172` | **DISPROVEN** |
| Host ticks stubs | **DIVERGE** (native skip) |

---

## Classifications (short)

1. **`world+172` = TRUE `AddQuest` CStrings. `world+184` =
   every `AddQuest`. `QM+44` = every `AddQuest` on the
   manager. Activated = `QM+56` 52-byte slots. PROVEN.**

2. **`004A1840` fills catalogs. `004B4260` walks `+172`.
   `004B3CE0` constructs `+56`. `004B2890` does not fill
   those lists. PROVEN.**

3. **“Constructed but not activated” for Oakvale is a
   CString store, not a quest object. PROVEN.** Factory-0
   is the opposite: activated, stub-constructed, **no
   tick**.

4. **Constructed live slots tick. Stubs do not. Catalog
   names do not. PROVEN.**

5. **`00DABAC0` without `00CB5AD0` — DISPROVEN.** Unique
   lookup `E8` is `004B42E8`; run is `call [edx+8]` after
   that hit.
