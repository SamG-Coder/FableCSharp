# First reader of `world+196` after Leave `AddTestQuest`

Investigation only. No production `src/` edits.

Do **not** treat `004A113B` / `AddTestQuest` as `004B4260`.
Do **not** start at `00DBDE40` / `S_QNOVI`. `Q_NewOakValeIntro`
and `NOVStartHSP` live in the 28-byte card. They are **not**
consumed from this vector on no-save New Game.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Authority: dump `listing-00480000.txt` `004A113B` / `004A16EA` /
`004A08D0` / `004A68D2` / `004A6BC7` / `0049F247` / `004B4260` /
`004AF190`; `listing-00600000.txt` `0061A8A0` / `0061AB30` /
`0061B590`; `listing-00680000.txt` `00686A70` / `00686A80`;
`listing-00cc0000.txt` `00CE791D`; `e8.tsv`;
`00-index/xrefs.tsv` (`AddTestQuest` `0x01238E98`);
`proofs/addtestquest-token/README.md`;
`proofs/world-plus184-first-use/README.md`;
`proofs/oakvale-later-activate/README.md`;
`proofs/qst-clear-004A08D0/README.md`.

Do not re-prove `AddQuest` TRUE → `+172` / all → `+184`, or
first `004B4260` = `0049F24E` `lea edx, [esi+172]`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| First **play-path** reader of filled `world+196` after Leave `AddTestQuest`? | **Nobody.** Store, then no walk until leftover UI or world dtor. | **PROVEN** |
| Is that reader `004B4260`? | **No.** First site is `0049F247` `lea edx, [esi+172]`. Stride **4**, not 28. | **DISPROVEN** |
| Does `Q_NewOakValeIntro` / `NOVStartHSP` get **used from** `+196`? | **No** on no-save. Name / HSP sit in the record. Gameflow wait is a **PE** string. `NOVStartHSP` is **QST-only** (not in exe strings). | **PROVEN** stored; **DISPROVEN** as first use |
| First dump **consumer** that copies the vector? | Leftover quest-selection `0061A8A0` (`00686A80` + `add eax, 0xC4`). | **PROVEN** leftover |
| Confirm that uses `+0` name / `+4` HSP? | `0061AB30`, gated `[this+343]`. Oakvale card nonempty → `004B4C50`, not first `004B4A10`. | **PROVEN** leftover; **DISPROVEN** as New Game |

---

## Verdict

`004A113B` is the `AddTestQuest` token arm inside `004A0D90`.
It parses seven arguments and `push_back`s a **28-byte** record
onto `CWorld+196/+200/+204`. `ebp` is the world (`004A0D9E`).

After Leave, flag-1 `004A08D0` **clears** the (still empty)
triple, then FinalAlbion fills it. GlobalQuests flag 0 has
**zero** `AddTestQuest`. Init Quests walks **`+172`**.

**First reader of the filled cards is not on the no-save play
path.** `004B4260` is not that reader. `Q_NewOakValeIntro` and
`NOVStartHSP` are **not** pulled from `+196` to start Oakvale
or to spawn the hero.

The only later walk is leftover `PC_QUESTS_SELECTION_MENU`
(`006224C0` / `0061A6A0` family). `0061A8A0` copies `world+196`.
If `[this+352]==0` it keeps rows where `004AF610(name)` is
already true — Oakvale is **false** on no-save, so the row is
dropped and `NOVStartHSP` is never handed to `004A0940`.
Confirm `0061AB30` is `[this+343]` only.

---

## Timeline (no-save New Game)

```
0042F2A2  Leave frontend
004A67D0  CWorld ctor
  004A68D2  [world+196/+200/+204]=0
004A1840  Load Quests
  004A193C  004A0D90(world, FinalAlbion.qst, 1)
    004A08D0  read +196 to erase (empty)     // before fill
    token AddQuest      → +184; TRUE → +172
    token AddTestQuest  → 004A0EA0 / 004A1127 → 004A113B
      004A16EA  lea esi, [ebp+196]           // WRITE 28 B
  004A199C  004A0D90(..., GlobalQuests.qst, 0)
    no AddTestQuest; no second 004A08D0
0049F247  lea edx, [esi+172]
0049F24E  004B4260                           // NOT +196
user.ini  ActivateQuest("Gameflow")
00CE7670  00893610 "Q_NewOakValeIntro"       // PE, not +196
          00896A30 OBJECT_QUEST_CARD_…       // PE, not +196
0061A8A0  (no E8 on this walk)               // leftover
004A6BC7  dtor free only                     // teardown
```

---

## 1. Dump `004A113B` (store, not a reader)

`xrefs.tsv` `"AddTestQuest"` `0x01238E98`:

| Site | Fn | Role |
|---|---|---|
| `004A0E93` | `004A0D90` | intern-empty `rep cmpsb` → `je 004A113B` |
| `004A1128` | `004A0D90` | interned `004115A0` → `je 004A113B` else `004A17B5` |

`listing-00480000.txt` arity (punct `0x122E028` `(` /
`0x122DF24` `"` / `0x122E024` `,` / `0x122E020` `)`):

```
( "name" , "hsp" , <int 009BA540 → esi> , "title" , "ini" , "end" , "card" )
```

Scratch pack then grow:

```
004A16A3  mov [esp+252], esi          // group dword
004A16E4  mov ecx, [ebp+200]
004A16EA  lea esi, [ebp+196]
004A16F0  cmp ecx, [esi+8]            // vs capacity +204
          je 004A170C → 004ADB50      // imul 28
004A1701  call 004A89D0               // in-place copy
004A1706  add [esi+4], 28             // end += 28
```

Those `[ebp+200]` / `[esi+8]` loads are the **writer** checking
room. They are not a consumer of a prior card.

Oakvale shipped row (`addtestquest-token`; QST-only HSP / title /
`.end`):

| Off | Field | Value |
|---:|---|---|
| +0 | name | `Q_NewOakValeIntro` |
| +4 | holy site | `NOVStartHSP` |
| +8 | group | `2` |
| +12 | title | `Q Oak Vale Introduction` |
| +16 | ini | `""` |
| +20 | `.end` | `OakValeIntro.end` |
| +24 | card | `OBJECT_QUEST_CARD_OAKVALE_INTRO` |

First `+196` row is **`Gameflow` / `NOVStartHSP` / `2`**, not
Oakvale. `Q_NewOakValeIntro` is also `AddQuest(..., FALSE)` →
`+184` only, **not** `+172`. **PROVEN**.

No `004B2850` / `004B4260` / `004B4A10` on this arm. **PROVEN**.

---

## 2. Xrefs to `CWorld+196` (vector triple)

`lea` / `[reg+196]` that sit on `+196/+200/+204`:

| VA | Fn | Op | After Leave fill? |
|---|---|---|---|
| `004A68D2` | `004A67D0` ctor | `mov [esi+196], ebx` (+200/+204) | zero **before** parse |
| `004A090D` | `004A08D0` | `mov eax, [esi+196]` + `004AA580` / `004ABD90` | flag 1 **before** `004A113B` (empty) |
| `004A16EA` | `004A0D90` | `lea esi, [ebp+196]` | **write** each card |
| `004A6BC7` / `004A6BD2` | `004A6AB0` dtor | load begin, `004ABD90`, `00BFEA14` | teardown only |
| `0061A8B0` | `0061A8A0` | `00686A80` then `add eax, 0xC4` | leftover copy |

`004AF190` / `004AF1A0` / `004AF1B0` / `004AF1D0` are raw
get/set of `+196/+200/+204` next to the `+184` family.
**0** `e8.tsv` sites. Same **UNREAD** as `004AF130` (`world-plus184-first-use`).

### Not this vector

| Site | Why excluded |
|---|---|
| `004A1679` / `004A179D` `lea … [esp+196]` | token scratch, not `CWorld` |
| `00619FF0` `lea eax, [ecx+196]; ret` | quest-selection **widget** (sibling `+333` / `+343`) |
| `00491738` `lea eax, [esi+196]` zero triple | other ctor (also zeros `+172/+184`) |
| `00506863` `lea edi, [esi+196]` | World Map object (`00A01B10`, not 28-byte cards) |
| `0048F413` stride **20** | different object (`addtestquest-token`) |
| `00631F77` `lea eax, [esi+196]` | CUIDef persist Action (`action-crc-plus196`) |
| `006D0D92` `add eax, 0xC4` | `[esi+12]` +196, then `0066BED0` — not `00686A80` world |
| stack / `esp+196` | not the world triple |

No `lea edx, [esi+196]` activate shape. No
`004B4260([world+196])`. **PROVEN**.

---

## 3. Not `004B4260`

```
0049F247  lea edx, [esi+172]
0049F24D  push edx
0049F24E  call 004B4260
```

`004B4260` takes a **CString\*** range (`[ebp+0]` / `[ebp+4]`,
`sar eax, 2`). `+196` records are **28** bytes
(`0x92492493` / `sar 4` on the leftover copy). Passing `+196`
as that argument would be a type error. First no-save names
are `Q_SunnyvaleMaster` … (`world-plus172-activate`).

`Q_NewOakValeIntro` is FALSE. It is **not** on that walk.
**DISPROVEN**.

---

## 4. Are `Q_NewOakValeIntro` / `NOVStartHSP` used from `+196`?

| Later mention | From `+196`? | Use |
|---|---|---|
| `00CD6E27` bind `Q_NewOakValeIntro` / `S_QNOVI` | **no** (PE, **before** parse) | register only |
| `004A113B` store | write | not a use |
| `0049F24E` `004B4260` | **no** | TRUE `+172` |
| `user.ini` `ActivateQuest("Gameflow")` | **no** | ini literal |
| first type-1 `00CE791D` `push "Q_NewOakValeIntro"` | **no** (`xrefs.tsv` `0x012C5D14`) | `00893610` miss → yield |
| `00896A30` `OBJECT_QUEST_CARD_OAKVALE_INTRO` | **no** (PE `0x012C5CF4`) | card find; `004AF610` still false |
| `userst.ini` `SetStartingHolySite("NOVStartHSP")` | **no** | `[0x13B866C]` before frontend |
| leftover `0061A8A0` / `0061AB30` | **yes** if menu opens | **LEFTOVER** |
| `004A6BC7` dtor | free | not gameplay |

`NOVStartHSP` is **not** in `strings.tsv`. The only native
path that can load that exact QST field is `record+4` after a
`+196` copy. First-region spawn is **LookoutPoint** /
**GuildArrivalHSP**, not this HSP (`first-region-after-leave`).

Gameflow’s Oakvale **wait** uses the PE name. That is **not**
a `+196` read and **not** an activate. **PROVEN**.

---

## 5. Leftover first consumer (`0061A8A0`)

```
0061A8A0  mov edi, ecx
          call 00686A80                 // [0x13B8A1C]+36 = world
0061A8B0  add eax, 0xC4                 // world+196
          call 00624A30                 // copy 28-byte vector
          006257C0 sort
0061A8CC  cmp [edi+352], 0
          jne skip-filter
          walk copy; 004AF610(name); keep if true
```

`00686A70` is the **same two instructions** as `00686A80`
(world getter; `ecx` unused). Confirm leaves the HSP pointer
on the stack for `004A0940`.

`e8.tsv` `0061A8A0` sites are all inside `0061A6A0`
(`0061A9FA` / `0061AAB0` / `0061AB63` / `0061AC92` /
`0061AF99`). **0** `E8` of `0061A6A0` / `006224C0` — vtbl /
factory only. Strings on `006224C0`:
`PC_QUESTS_SELECTION_MENU` / `PC_TITLE_QUEST_SELECTION`.

`0061AB30` (`E8` only `0061B59D`, `[esi+343]!=0`):

```
0061A8A0 copy
index [+344] * 28
+16 ini  nonempty → Data\Levels\Ini\ + 009EC890
004B43D0 / 004B39B0 (record / +0 name)
+4 HSP → 00686A70 ; 004A0940 teleport
+24 card nonempty → 004B4C50          // Oakvale
         empty    → 004B4A10(1,1, rec) // Gameflow card
```

Oakvale’s card is nonempty, so leftover confirm is
**`004B4C50`**, not the no-save `004B4A10` skip at `00416BCF`.
`0061B530` / `0061B560` cycle the list under the same `+343`
byte. **PROVEN** leftover; **DISPROVEN** as Leave / Init Game /
first type-1.

If `[+352]==0` on no-save, `004AF610("Q_NewOakValeIntro")` is
false (Gameflow just yielded on that miss). The Oakvale row
is copied for the test and **dropped**. `NOVStartHSP` is not
passed to `004A0940`. **PROVEN** as filter; **UNREAD** as a
live menu open on this walk (no `E8`).

---

## 6. Host vs native

| Host | Native after Leave | Class |
|---|---|---|
| `QuestFile.Parse` `AddQuest` only | `004A113B` also fills `+196` (112 rows) | **MISMATCH** |
| `AddTestQuestStoreFn` Note | 28-byte store, not activate | **PROVEN** note |
| `LoadQuestsFn` comment “into world+184” | `AddTestQuest` is `+196` | **STALE** comment |
| `RegionTravel.NewGameStartScript = NOVStartHSP` | field +4; first region is GuildArrival | **LEFTOVER** as New Game spawn |
| Invent `ActivateQuest("Q_NewOakValeIntro")` from this store | no play-path reader | **DISPROVEN** |

---

## Classifications (short)

1. **First play-path reader of filled `world+196` after Leave
   `AddTestQuest` — none. PROVEN.** Writer / empty clear / dtor
   only. Do not invent a New Game walk.
2. **`004B4260` is not that reader — DISPROVEN.** `+172`,
   4-byte names.
3. **`Q_NewOakValeIntro` / `NOVStartHSP` used from `+196` —
   DISPROVEN on no-save.** Stored. First later Oakvale *mention*
   is Gameflow’s PE wait. HSP is QST-only plus leftover `+4`.
4. **First dump consumer — leftover `0061A8A0`. PROVEN.**
   Confirm `0061AB30` / `+343`. Oakvale card → `004B4C50`.
   Not first `004B4260`.
