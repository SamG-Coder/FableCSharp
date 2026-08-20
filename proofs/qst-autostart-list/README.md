# QST autostart vs catalog: no-save New Game

Investigation only. No production `src/` edits.

Do **not** treat `Q_NewOakValeIntro` as auto-started.
Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** treat `CActivateQuestDef` compiled-def rows as
the New Game start list.
Do **not** start at `S_QNOVI` / `00DBDE40`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Sources: ExeIndex `listing-00480000.txt` `0049D770` /
`004A08D0` / `004A0D90` / `004A113B` / `004A1840` /
`0049F180` / `004B2850` / `004B2890` / `004B3CE0` /
`004B4260` / `004B4A10`; `listing-004c0000.txt`
`004F5B7D`; `listing-00780000.txt` `007B5590` /
`007B5680`;
`src/Fable.Formats/Qst/QuestFile.cs`;
`src/Fable.Game/EngineLifecycle.cs`
(`LoadQuestDefs` / `StoreAddQuestNames` /
`InitCharactersAndQuests`);
`assembly/compiled-defs/game/entries.tsv`;
`TlcInstallTests.Quest_table_includes_opening`;
`EngineLifecycleTests.Init_quests_004B4260_activates_wld_initial_list`;
`EngineLifecycleTests.No_save_does_not_activate_Q_NewOakValeIntro`;
`proofs/addtestquest-token`;
`proofs/qst-first-load`;
`proofs/quest-manager-plus44`;
`proofs/quest-activate-gate`;
`proofs/q-novi-activator-callers`;
`proofs/oakvale-later-activate`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| How do QST names become *catalogued*? | `004A1840` → `0049D770` path → `004A0D90` `AddQuest` → **always** `world+184` and `004B2850` `QM+44`. Flag 1 on `FinalAlbion.qst` (`004A08D0` clear); flag 0 append `GlobalQuests.qst`. | **PROVEN** |
| How do QST names become *activated* on no-save? | `0049F24E` `004B4260([world+172])`. `+172` is `AddQuest` **TRUE** only. Per name: `004B00C0` (must be in `QM+44`) → unique `00CB5AD0` `E8` at `004B42E8` → `004BB720` → once `004B3CE0` instance construct on `QM+56`. Then `004B2890` (empty `+112`, **no** activate). Later `user.ini` `ActivateQuest("Gameflow")` via `004B4A10`. | **PROVEN** |
| Is `Q_NewOakValeIntro` on the auto-start list? | **No.** `AddQuest(..., FALSE)` → **not** `world+172`. Host: `WorldPlus184` / `QuestManagerPlus44` contain it; `ActivatedQuests` / `WorldPlus172` do not. | **PROVEN** constructed-only |
| Does `AddTestQuest` activate? | **No.** `004A113B` → `world+196` 28-byte record only. No `004B2850` / `004B4260` / `004B4A10`. | **DISPROVEN** as activate |
| Do `CActivateQuestDef` 16-byte rows auto-start? | **No.** Six unnamed size-16 / 0-field compiled-def rows. Runtime is later thing action `007B5680` → `00843FC0` → `004B4A10`. Not on no-save Leave. | **DISPROVEN** |
| Does `004B2890` activate QST names? | **No.** First-seen `0049F259` after `004B4260`. Empty `QM+112` sentinel → skip. No `004B4A10`. | **DISPROVEN** |

---

## Verdict

**Catalog construction and activation are two different
stores and two different VAs. PROVEN.**

`Q_NewOakValeIntro` is **constructed-only** on no-save New
Game: it is an `AddQuest` **FALSE** name, so it lands in
`CWorld+184` (`WorldPlus184`) and `QuestManager+44`
(`QuestManagerPlus44`). It is **not** copied to `CWorld+172`,
so first `004B4260` never walks it, never `00CB5AD0`s it,
never `004B3CE0`s it. Host `ActivatedQuests` matches that
walk plus later `Gameflow`. **PROVEN**
(`Init_quests_004B4260_*` / `No_save_does_not_activate_*`).

There is **no** QST-side auto-start of FALSE names, of
`AddTestQuest` cards, of `CActivateQuestDef` defs, or of
per-region `.qst` files other than the WLD-stem file plus
the intern `GlobalQuests.qst`.

---

## Three name vectors (do not collapse)

| Slot | Object | Writer | Who is on it | First no-save consumer |
|---|---|---|---|---|
| `world+184/+188/+192` | `CWorld` | `004A0D90` `AddQuest` always (`004A1080`) | every `AddQuest` TRUE **and** FALSE | catalog only; **not** `004B4260` |
| `world+172/+176/+180` | `CWorld` | `AddQuest` **TRUE** (`004A10C4`) | eight FinalAlbion + `Global_WatchForHeroDeath` | `0049F247` `lea edx,[esi+172]` → `004B4260` **auto-start** |
| `QM+44/+48/+52` | `[0x13B89FC]` | `004A1101` `004B2850` always | same set as `+184` | `004B00C0` **membership gate**, not the walk |
| `world+196/+200/+204` | `CWorld` | `AddTestQuest` `004A113B` | 28-byte cards (112 in FinalAlbion; 0 in Global) | leftover picker `0061A8A0`; **not** no-save activate |

`WorldPlus184 == QuestManagerPlus44` after one
`004A08D0` + both files. They are still two buffers
(different object; flag-1 clear does not wipe `+44`).
**PROVEN** (`quest-manager-plus44`).

`ActivatedQuests` is **not** `+184`. It is the names
that passed `004B4260` (TRUE list) plus later
`004B4A10("Gameflow")`.

---

## Timeline (no-save New Game)

```
004A67D0  CWorld ctor
  +172 / +184 / +196  zeroed

004B4590  QuestManager ctor  [0x13B89FC]
  +44 vector empty
  +56 / +112 circular sentinels

00416ABA  004A1840 "Load Quests"
  0049D770  Data\Levels\ + WLD stem + .qst
            → Data\Levels\FinalAlbion.qst
  004A0D90(world, path, 1)
    004A08D0  clear +184 / +172 / +196
    AddQuest     → +184; TRUE → +172; 004B2850 QM+44
    AddTestQuest → 004A113B → +196 only
  intern 0x01238F38 Data\Levels\GlobalQuests.qst
  004A0D90(world, path, 0)          // append, no clear
    same tokens; no AddTestQuest in this file

[0x13B8648]==0
  0049F180  Init Characters / Init GUI
  0049F21B  "Init Quests"
  0049F23D  ecx = [0x13B89FC]
  0049F247  lea edx, [esi+172]
  0049F24E  call 004B4260            // ACTIVATE TRUE names
    per name:
      "QuestManager: Activate Quest"
      004B00C0  find in QM+44
      00CB5AD0  unique E8 004B42E8
      004BB720  enqueue factory or 0
    004B4386  call 004B3CE0          // INSTANCE construct QM+56
  0049F259  call 004B2890            // NOT activate; empty +112
  00416BCF  +90584 empty skip 004B4A10
user.ini  ActivateQuest("Gameflow")
  00419CE0 → 00892E80 → 004B4A10 → 004B4260 one-name
```

`Q_NewOakValeIntro` is on `+184` / `+44` / `+196` after
parse. It is **absent** from the `0049F24E` walk.
**PROVEN**.

---

## 1. Path join `0049D770` is not a parser

`listing-00480000.txt` `0049D770`:

```
0049D770  sub esp, 16
          mov esi, ecx               ; dest CString
          mov edi, edx               ; WLD path
          00997620  stem
          push 0x1238C40             ; "Data\Levels\" + ".qst"
          0041A060 / 0099BE70 / 0099BF30
          ret
```

Site: `004A190D` inside `004A1840` `"Load Quests"`.
Then `00999230` exists → `004A193C` `004A0D90(..., 1)`.
Hard intern `0x01238F38` `Data\Levels\GlobalQuests.qst`
→ `004A199C` `004A0D90(..., 0)`.

Host `GameInstall.QuestPath` / `GlobalQuestPath` and
`EngineLifecycle.DeriveQuestFileName` match.
**PROVEN.**

Per-region `.qst` other than the WLD stem is **not**
opened on this walk. Global is a **fixed intern**, not
derived. **PROVEN.**

---

## 2. Token walk `004A0D90` (catalog)

`listing-00480000.txt` `004A0D90`:

```
004A0D90  mov al, [esp+8]            ; flag
          test al, al
          je  004A0DA7
          call 004A08D0              ; flag 1: clear +184/+172/+196
          tokenizer 009BA4F0 / 009B9C60 / 009BA330
004A0E7D  "AddQuest"     → 004A0EBF
004A0E92  "AddTestQuest" → 004A113B
          else jmp 004A17B5          ; next token
```

`AddQuest` after name + `00BFEBA8("TRUE")` → `bl`:

```
004A1080  lea esi, [ebp+184]         ; ALWAYS
004A10B2  test bl, bl
          je  004A10F6               ; FALSE skips +172
004A10C4  lea esi, [ebp+172]         ; TRUE only
004A10F6  mov ecx, [0x13B89FC]
          call 004B2850              ; ALWAYS → QM+44
```

`QuestFile.Parse` is a regex over
`AddQuest("name", TRUE|FALSE)` only. That is enough to
fill host `WorldPlus184` / `WorldPlus172` /
`QuestManagerPlus44` (`StoreAddQuestNames`). It **drops**
`AddTestQuest`. Native still stores those 28-byte rows.
**PARTIAL** as a `004A0D90` stand-in; **PROVEN** as the
TRUE/FALSE name table used by auto-start.

### `TlcInstallTests.Quest_table_includes_opening`

`QuestFile.Load(QuestPath)` contains `Q_NewOakValeIntro`
(no persist assert on the name-only `Contains`).
`PersistentNames()` is the eight FinalAlbion TRUE rows:

1. `Q_SunnyvaleMaster`
2. `ChapterAndSceneManager`
3. `PersonalScriptMain`
4. `PersonalScript_GlobalThings`
5. `NPCDeath`
6. `HeroBoasts`
7. `V_HeroDolls`
8. `CS_PlayCutscene`

`Append(GlobalQuests.qst)` adds TRUE
`Global_WatchForHeroDeath`. Merged
`Q_NewOakValeIntro` is **`!Persistent`**. **PROVEN**
that the format table knows the name and that it is
**not** the autostart slice.

---

## 3. Auto-start walk `004B4260`

`listing-00480000.txt` `004B4260` (`this` = QuestManager,
arg0 = name vector):

```
004B4260  ebp = arg0
          count = ([ebp+4]-[ebp]) >> 2
          jbe 004B437F
004B42A2  push "QuestManager: Activate Quest"
004B42D7  call 004B00C0              ; QM+44 find
          test al, al
          je  004B4363               ; skip lookup
004B42E8  call 00CB5AD0              ; UNIQUE E8
          hit  → 004BB720 factory
          miss → 004BB720 [rec+4]=0
004B4386  call 004B3CE0              ; once after the loop
          ret 12
```

First no-save site is **`0049F24E`** with
`lea edx, [esi+172]`. **Not** `+184`. **Not** `+196`.
**Not** `QM+44` as the walk.

`world+172` after both files (host `WorldPlus172`):

1. `Q_SunnyvaleMaster`
2. `ChapterAndSceneManager`
3. `PersonalScriptMain`
4. `PersonalScript_GlobalThings`
5. `NPCDeath`
6. `HeroBoasts`
7. `V_HeroDolls`
8. `CS_PlayCutscene`
9. `Global_WatchForHeroDeath`

`Init_quests_004B4260_activates_wld_initial_list`:

- `ActivatedQuests.Take(9) == WorldPlus172`
- `ActivatedQuests[9] == "Gameflow"` (ini, not QST TRUE)
- `DoesNotContain("Q_NewOakValeIntro", WorldPlus172)`
- `DoesNotContain("Q_NewOakValeIntro", ActivatedQuests)`
- `Contains(WorldPlus184, "Q_NewOakValeIntro")`
- `WorldPlus184 == QuestManagerPlus44`
- `Contains(QuestManagerPlus44, "Q_NewOakValeIntro")`
- `Contains(QuestManagerPlus44, "Gameflow")`

WLD `START_INITIAL_QUESTS` is a **subset** of that TRUE
list and is **not** the writer (`00507C30` has no case).
**PROVEN** (`qst-first-load` / current host walk of
`_worldPlus172`).

`Gameflow` is `AddQuest(..., FALSE)` so it is in `+184` /
`+44` only. `004B00C0` still returns 1 when `user.ini`
later `004B4A10`s it. **PROVEN** membership vs autostart.

---

## 4. Instance construct `004B3CE0` is not the catalog

`004B4260` tail `004B4386` `call 004B3CE0` on the
12-byte enqueue vector (`imul 0x2AAAAAAB` = `/12`).

Second loop (`004B3E82`):

```
004AF610  already on QM+56?  jne skip
[rec+4]==0  → 004B4063  52-byte stub (factory 0)
else        → 004B0310 + 00CB7900 fiber, then
              00BFEA0E(16) node on [QM+56]
```

Those **16-byte circular nodes** are **active-quest
slots**, filled only for names on this walk. They are
**not** `CActivateQuestDef` compiled-def rows.

`Q_NewOakValeIntro` is **not** in the enqueue, so
`004B3CE0` does **not** allocate a slot for it.
Bind `00CD6E27` `00CB5C90` `S_QNOVI` / `00DBEF70` is
factory **register**, not this construct.
**PROVEN** (`oakvale-later-activate`).

---

## 5. `004B2890` is the sibling, not an activator

`listing-00480000.txt` `004B2890`:

```
004B2890  eax = [this+112]
          cmp [eax], eax
          je  004B2989               ; empty sentinel first-seen
          … persist boast restore …
004B2989  00449970 / 00487DC0        ; player Thing
          je  004B2AC1               ; no Thing → skip +56
```

Site `0049F259` immediately after `004B4260`.
No `004B4A10`. No walk of `+172` / `+184` / `+44`.
**DISPROVEN** as QST autostart (`004B2890-empty-first`).

---

## 6. `AddTestQuest` token (`004A113B`)

`proofs/addtestquest-token`: seven args, 28-byte
`push_back` at `world+196`. Oakvale shipped row:

```
AddTestQuest("Q_NewOakValeIntro", "NOVStartHSP", 2,
  "Q Oak Vale Introduction", "", "OakValeIntro.end",
  "OBJECT_QUEST_CARD_OAKVALE_INTRO");
```

No `004B2850`. `004B00C0` would skip a name that is
**only** an `AddTestQuest` (never `AddQuest`). This name
**is** also `AddQuest FALSE`, so `+44` membership exists
— activation still needs a later `004B4260` / `004B4A10`
of that string. No-save does not supply one.
**PROVEN** store; **DISPROVEN** as first activate.

Consumer of `+196` is leftover `PC_QUESTS_SELECTION_MENU`
`0061A8A0` / `0061AB30` (`004B4A10` / `004B4C50`).
**LEFTOVER** vs no-save.

---

## 7. `CActivateQuestDef` empty 16-byte rows — DISPROVEN

`assembly/compiled-defs/game/entries.tsv`: **six** rows,
type `CActivateQuestDef`, **size 16**, extra-field count
**0**, names empty / `NULLDEF_CActivateQuestDef`:

| Id | Name |
|---:|---|
| 61 | `NULLDEF_CActivateQuestDef` |
| 9241 | unnamed |
| 9248 | unnamed |
| 12277 | unnamed |
| 12857 | unnamed |
| 12874 | unnamed |

Registrar `004F5B7D` (`004EE23F` remaining pairs, class
62). String xrefs: `007B5594` type-name, `007B5680`
lookup. Runtime path (`q-novi-activator-callers`):

```
007B5680  lookup "CActivateQuestDef"
          → 00843F50 CCreatureAction_ActivateQuest
          → 00843FC0
          → 004B4A10([this+168])
```

`[this+168]` is a **runtime CString**, not a QST TRUE
name and not intern `0x012C5D14` `Q_NewOakValeIntro`.
Same ctor also queues `"Expression_Follow"`. **DISPROVEN**
as the no-save autostart list. **PROVEN** as a later
thing-use activator class. Field payload inside those
16-byte defs is **UNREAD** (0 named fields in the dump).

Do **not** confuse these compiled-def rows with
`004B3CE0`'s 16-byte `QM+56` nodes.

---

## 8. `FinalAlbion.qst` vs `GlobalQuests.qst`

| File | How opened | Flag | `AddQuest` | TRUE | `AddTestQuest` |
|---|---|---|---:|---:|---:|
| `Data\Levels\FinalAlbion.qst` | `0049D770` from WLD stem | **1** (clear) | 187 | **8** | **112** |
| `Data\Levels\GlobalQuests.qst` | intern `0x01238F38` | **0** (append) | 13–14 | **1** (`Global_WatchForHeroDeath`) | **0** |

FALSE names in Global (`Global_TeleportToHeroGuild`,
`Expression_*`, …) join `+184` / `+44` the same way
`Q_NewOakValeIntro` does: catalogued, **not** auto-started.

Host `LoadQuestDefs` now `StoreAddQuestNames`s both files
into `_worldPlus184` / `_worldPlus172` / `_questManagerPlus44`
and `Quests.Append(global)`. Older notes that Global is
Note-only are **STALE** vs current `EngineLifecycle.cs`.

---

## 9. `Q_NewOakValeIntro`: constructed-only

| Store / site | Has the name? | Role |
|---|---|---|
| `FinalAlbion.qst` `AddQuest(..., FALSE)` | yes | catalog |
| `world+184` / `WorldPlus184` | **yes** | catalog construct |
| `QM+44` / `QuestManagerPlus44` | **yes** | gate membership |
| `world+172` / `WorldPlus172` | **no** | auto-start list |
| `ActivatedQuests` / `Runtime.Quests` | **no** | activate + `004B3CE0` |
| `world+196` `AddTestQuest` | yes | debug card / HSP |
| `00CD6E27` bind `S_QNOVI` / `00DBEF70` | yes | factory register |
| `00CE7670` Gameflow | wait `00893610` / `004AF610` | **not** activate |
| `CActivateQuestDef` rows | no named payload | **DISPROVEN** autostart |
| `004B2890` | n/a | **DISPROVEN** activator |

`No_save_does_not_activate_Q_NewOakValeIntro` also Notes:
`00CD6E27` bind-only; `004A113B` store not `004B4A10`;
`004B5080` 0 external `E8`; `00416BCF` skip `004B4A10`.
**PROVEN.**

---

## C# vs native

| Host | Native | Class |
|---|---|---|
| `QuestFile.Parse` `AddQuest` only | `004A0D90` also `AddTestQuest` | **PARTIAL** |
| `StoreAddQuestNames` → `+184` / TRUE `+172` / `+44` | same three stores | **PROVEN** |
| `InitCharactersAndQuests` walks `_worldPlus172` | `004B4260([world+172])` | **PROVEN** |
| `ActivatedQuests` = nine TRUE + `Gameflow` | same | **PROVEN** |
| `WorldPlus184` has `Q_NewOakValeIntro`; `ActivatedQuests` does not | catalog vs walk | **PROVEN** |
| `ActivateQuest(Q_NewOakValeIntro)` | none on no-save | **DISPROVEN** if invented |

---

## Classifications (short)

1. **Catalog construct — PROVEN:** `004A0D90` `AddQuest` →
   `world+184` + `004B2850` `QM+44`. Includes FALSE
   (`Q_NewOakValeIntro`, `Gameflow`).
2. **Auto-start list — PROVEN:** `world+172` = TRUE slice
   of **both** QST files. Walked by `0049F24E` `004B4260`.
3. **Activate / instance construct — PROVEN:** `004B00C0` →
   `00CB5AD0` `004B42E8` → `004B3CE0` `QM+56`. Only names
   on that walk (plus later `004B4A10("Gameflow")`).
4. **`Q_NewOakValeIntro` auto-start — DISPROVEN.**
   Constructed-only (`WorldPlus184` / `QuestManagerPlus44`).
5. **`AddTestQuest` / `004A113B` / `0049D770` / `004B2890` /
   `CActivateQuestDef` 16-byte rows as New Game start —
   DISPROVEN.**
