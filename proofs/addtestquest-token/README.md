# `004A113B` AddTestQuest token walk — `world+196` record

Investigation only. No production `src/` edits.

Do **not** treat `AddTestQuest` as the first `004B4260` activate.
Do **not** start at `00DBDE40` / `S_QNOVI`. `Q_NewOakValeIntro`
is `AddQuest(..., FALSE)` plus one `AddTestQuest` card. The
store is `world+196` only.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: ExeIndex `listing-00480000.txt` `004A08D0` / `004A0D90` /
`004A113B` / `004A16EA` / `004A68D2` / `004A6BC7` / `004A89D0` /
`004ABD90` / `004ADB50`; `listing-00600000.txt` `0061A6A0` /
`0061A8A0` / `0061AB30` / `00624A30`; `listing-00680000.txt`
`00686A80`; `listing-00980000.txt` `009BA540`;
`out/00-index/strings.tsv` (`AddTestQuest` `0x01238E98`,
`AddQuest` `0x01238EA8`, `OBJECT_QUEST_CARD_OAKVALE_INTRO`
`0x012C5CF4`); TLC `data\Levels\FinalAlbion.qst` /
`GlobalQuests.qst`; `proofs/qst-first-load/README.md`;
`proofs/script-gameflow/README.md`;
`EngineLifecycleTests.No_save_does_not_activate_Q_NewOakValeIntro`.

---

## Verdict

`004A0D90` is a token walk. `AddTestQuest` matches at
`004A0EA0` / `004A1127` and lands at **`004A113B`**. That
path parses seven arguments and `push_back`s a **28-byte**
record onto the `CWorld` vector at **`+196` / `+200` / `+204`**.

It does **not** call `004B2850`, `004B4260`, or `004B4A10`.
It is **not** the first no-save activate. First `004B4260`
walks `world+172` (`AddQuest` TRUE), starting at
`Q_SunnyvaleMaster`. **PROVEN**.

For `Q_NewOakValeIntro` / `NOVStartHSP` the stored record is:

| Off | Type | QST arg | Value |
|---:|---|---|---|
| +0 | `CString` (4) | 1 name | `Q_NewOakValeIntro` |
| +4 | `CString` | 2 holy site | `NOVStartHSP` |
| +8 | `dword` | 3 group | `2` |
| +12 | `CString` | 4 title | `Q Oak Vale Introduction` |
| +16 | `CString` | 5 ini | `""` |
| +20 | `CString` | 6 `.end` | `OakValeIntro.end` |
| +24 | `CString` | 7 card | `OBJECT_QUEST_CARD_OAKVALE_INTRO` |

On automatic no-save New Game, **nobody later reads
`world+196`**. The only later consumer is the leftover
quest-selection UI (`0061A8A0`, `PC_QUESTS_SELECTION_MENU`).
Gameflow's `00896A30` uses the card **string**, not this
vector.

---

## Timeline (no-save New Game)

```
004A67D0  CWorld ctor
  004A68D2  [world+196/+200/+204]=0          // empty triple
004A1840  Load Quests
  004A193C  004A0D90(world, FinalAlbion.qst, 1)
    004A08D0  clear +184 / +172 / +196
    009BA4F0  tokens
      AddQuest      → +184; TRUE → +172; 004B2850
      AddTestQuest  → 004A113B → +196 only
  004A199C  004A0D90(world, GlobalQuests.qst, 0)
    no AddTestQuest
0049F24E  004B4260([world+172])              // NOT +196
00416BCF  +90584 empty → skip 004B4A10
user.ini  ActivateQuest("Gameflow")
00CE7670  00893610 Q_NewOakValeIntro = 0
          00896A30 OBJECT_QUEST_CARD_OAKVALE_INTRO miss
                                            // hardcoded name, not +196
```

---

## 1. Token walk (`004A0D90` → `004A113B`)

`004A0D90` flag `[esp+8]`: `1` → `004A08D0` first; `0` append.

Tokenizer `009BA4F0` / `009B9C60` / `009BA330`. Empty intern
`0x122D70E` uses `rep cmpsb`; else `004115A0`.

Dump strings (`strings.tsv`):

| VA | Text |
|---|---|
| `0x01238EA8` | `AddQuest` |
| `0x01238E98` | `AddTestQuest` |
| `0x012C5CF4` | `OBJECT_QUEST_CARD_OAKVALE_INTRO` |

`NOVStartHSP`, `OakValeIntro.end`, and
`Q Oak Vale Introduction` are **QST-only**. They are **not**
in the exe string dump.

Punctuation immediates (same as `AddQuest`): `0x122E028` `(`,
`0x122DF24` `"`, `0x122E024` `,`, `0x122E020` `)`.

`AddTestQuest` arity at `004A113B`:

```
( "name" , "hsp" , <int 009BA540> , "title" , "ini" , "end" , "card" )
```

`009BA540` is "parse integer" (`"Error parsing integer"` on
miss). Result stays in `esi` and is stored as a raw dword.

Compare `AddQuest` at `004A0EBF`: name + `TRUE`/`FALSE` only,
then `004B2850` onto the quest-manager list. **No** such
push on the test path.

Unknown tokens `jmp 004A17B5` (next token). **PROVEN**.

---

## 2. Record layout (`004A16E4` / `004A89D0` / `004ABD90`)

Vector at `CWorld`:

| Off | Role |
|---:|---|
| +196 | begin |
| +200 | end |
| +204 | capacity |

Grow helper `004ADB50` (`imul 28`). In-place store
`add [esi+4], 28` (`004A1706`). Copy `004A89D0`. Range
dtor `004ABD90` tears down `+24,+20,+16,+12,+4,+0` and
**skips +8** — the dword.

Shipped `FinalAlbion.qst` line 274:

```
AddTestQuest("Q_NewOakValeIntro", "NOVStartHSP", 2,
  "Q Oak Vale Introduction", "", "OakValeIntro.end",
  "OBJECT_QUEST_CARD_OAKVALE_INTRO");
```

Parse dests are stack-stable into the 28-byte scratch at
`[esp+240]`, then `004A89D0` / `004ADB50` onto `lea esi,
[ebp+196]`.

| Off | Field | Oakvale row |
|---:|---|---|
| +0 | quest name | `Q_NewOakValeIntro` |
| +4 | holy-site player start | `NOVStartHSP` |
| +8 | menu group dword | `2` |
| +12 | selection title | `Q Oak Vale Introduction` |
| +16 | ini stem (`Data\Levels\Ini\` + this) | empty |
| +20 | `.end` script | `OakValeIntro.end` |
| +24 | quest-card object name | `OBJECT_QUEST_CARD_OAKVALE_INTRO` |

Group dword in the shipped file is **0 / 1 / 2** (most
playable rows are `2`; debug / `DummyQuestForHeroLevels`
`HL*.ini` use `0`/`1`). Exact enum name is **PARTIAL**.
The quest-selection UI builds **six** tab strings
(`0061A736` `esi=0..5`, `[0x13B8C68]=6`). Mapping of
0/1/2 onto those tabs is **UNREAD**.

**112** `AddTestQuest` rows in `FinalAlbion.qst`. First
row is `Gameflow` / `NOVStartHSP` / `2` /
`1. Gameflow (Play Fable)` / `Gameflow.end` / empty card
— **not** Oakvale. `GlobalQuests.qst` has **zero**.
**PROVEN**.

`Q_NewOakValeIntro` is also `AddQuest(..., FALSE)` at
line 129, so it is **not** in `world+172`. **PROVEN**.

---

## 3. Who writes `world+196`

| Site | When | What |
|---|---|---|
| `004A68D2` | `CWorld` ctor / Init World | zero the triple |
| `004A08D0` | `004A0D90` flag 1 | erase via `004AA580` / `004ABD90` |
| `004A16EA` | each `AddTestQuest` | `push_back` 28 B |
| `004A6BC7` | `CWorld` dtor | `004ABD90` then `00BFEA14` |

No other writer of this 28-byte vector. Nearby
`[reg+196]` with stride **20** (`0048F40A`) is a
**different** object. **PROVEN**.

---

## 4. Who reads `world+196` on no-save New Game

Automatic Leave / Init Game / first type-1: **no reader**.

`0049F24E` `004B4260` is `lea edx, [esi+172]`. `00416BCF`
compares `+90584` to empty intern and skips `004B4A10`.
`00CE7670` calls `00893610("Q_NewOakValeIntro")` and
`00896A30("OBJECT_QUEST_CARD_OAKVALE_INTRO")` — both
**hardcoded names**, not a walk of `+196`. Card miss
because `004AF610` is not yet true. **PROVEN**
(`script-gameflow`).

The **only** later reader of this vector is leftover UI:

`0061A8A0`:

```
00686A80          ; [0x13B8A1C]+36 → world
add eax, 0xC4     ; world+196
00624A30          ; copy 28-byte vector
[this+352]==0 → keep rows where 004AF610(name) is true
006257C0          ; sort
```

Callers are all inside the `0061A6A0` family
(`0061A9D0` / `0061AA80` / `0061AB30` / `0061AC60` /
draw). Input / confirm sites gate on `[this+343]`.
There is **no** `E8 0061A6A0` / `E8 0061A8A0` on the
Leave / `004A1840` / `0049F180` / `0043A380` / type-1
walk. Strings on the sibling ctor `006224C0`:
`PC_QUESTS_SELECTION_MENU` / `PC_TITLE_QUEST_SELECTION`.
**LEFTOVER** vs no-save New Game.

Confirm `0061AB30` (only when `+343`):

| Record field | Use |
|---|---|
| +16 ini | if not empty intern: `Data\Levels\Ini\` + stem → `009EC890` |
| +0 name | `004B43D0` / `004B39B0` |
| +4 HSP | `00686A70` → `004A0940` teleport |
| +24 card | non-empty → `004B4C50`; else `004B4A10(1,1, record)` |

That `004B4A10` is the **debug picker**, not first
`004B4260`. Opening this menu is how a human would
force-start `Q_NewOakValeIntro` from the test card.
It is **not** on no-save New Game. **PROVEN** as leftover
reader; **DISPROVEN** as first activate.

World dtor `004A6BC7` reads `+196` only to free. Not a
gameplay consumer.

---

## 5. C# vs native

| Host | Native | Class |
|---|---|---|
| `QuestFile.Parse` regex `AddQuest` only | `004A0D90` also walks `AddTestQuest` | **MISMATCH** (112 rows dropped) |
| `AddTestQuestStoreFn` Note only | 28-byte `+196` record | **PARTIAL** |
| `RegionTravel.NewGameStartScript = NOVStartHSP` | field +4 of this record | **PROVEN** string; **LEFTOVER** as New Game spawn |
| `ActivateQuest(Q_NewOakValeIntro)` | not from this store | **DIVERGE** if invented |

---

## Classifications (short)

1. **`004A113B` store layout — PROVEN.** Seven fields,
   28 bytes, vector `world+196/+200/+204`. Oakvale row
   is name / `NOVStartHSP` / `2` / title / empty ini /
   `OakValeIntro.end` / `OBJECT_QUEST_CARD_OAKVALE_INTRO`.
2. **Not first `004B4260` — DISPROVEN.** No
   `004B2850` / `004B4260` / `004B4A10` on this token.
   First activate list is `+172` TRUE names.
3. **No-save later read of `world+196` — none.**
   Gameflow card check is a hardcoded string.
4. **Quest-selection UI `0061A8A0` — LEFTOVER** reader /
   debug activate. Not on Leave / Init Game / first type-1.
