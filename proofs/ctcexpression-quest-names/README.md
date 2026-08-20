# `CExpressionDef+120` is not a TNG quest-name property

Investigation only. No production `src/` edits.

Question: does any TNG / `game.bin` `CExpressionDef` store
`Q_NewOakValeIntro` as the CString `007EF200` copies from
`[CExpressionDef+120]` into `004B4A10`? Compare
`LookoutPoint.tng` vs `StartOakValeWest.tng`. Host TNG parser
properties.

Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** collapse leftover **#4** (Lookout Present vs
Oakvale intro view). Do **not** fold leftover **#50**
(first-proximity TNG pump) into #4.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER**.

Authority: Anniversary loose
`WellingtonGame\FableData\Build\Data\Levels\FinalAlbion\LookoutPoint.tng`
and `StartOakValeWest.tng` (UID/pos MATCH TLC WAD parse;
`proofs/lookout-tng-walk`); TLC
`data\Levels\FinalAlbion.qst` / `.wld`; TLC
`CompiledDefs\game.bin` via `assembly/compiled-defs/game/entries.tsv`
and `names.tsv`; `src/Fable.Formats/Tng/ThingFile.cs`;
`listing-007c0000.txt` `007EF200` / `007EF3A1`;
siblings `proofs/007EEF60-activate`,
`proofs/q-novi-activator-callers`,
`proofs/tng-spawn`, `proofs/004FDBC0-open`.

---

## Verdict

**No TNG property and no compiled `CExpressionDef` /
`EXPRESSION` instance name is `Q_NewOakValeIntro`.**

The only TNG occurrence of that string is
`XXXSectionStart Q_NewOakValeIntro` on
**`StartOakValeWest.tng`**. Host `ThingFile` stores that as
`ThingInstance.Section`, not as a CTC field. Lookout has
**zero** hits.

That is chicken-egg vs activate: Oakvale West things that
carry the section name exist only after that map’s TNG is
open. Leftover **#50** first-proximity parse is
**`LookoutPoint.tng`**, which does **not** contain the name.
Even a full native `004FDBC0` prox walk that later opens
Oakvale West is parse/store during Loading world
(`CurrentRegion` still unset) and is **not** first Present.

`007EF200` still only activates when nested `[def+120]` is
non-empty vs intern `0x122D70E`. No recovered TNG/def here
fills that slot with `Q_NewOakValeIntro`.

| Question | Answer | Class |
|---|---|---|
| Does `LookoutPoint.tng` contain `Q_NewOakValeIntro`? | **No** | **PROVEN** |
| Does `StartOakValeWest.tng` contain it as a CTC / `QuestName` property? | **No.** Only `XXXSectionStart Q_NewOakValeIntro` | **PROVEN** |
| Other TNG files? | `StartOakValeEast.tng` has `XXXSectionStart Q_NewOakValeIntro_PreAttack` only. No other `.tng` hit | **PROVEN** |
| Host parser key for that string? | `ThingInstance.Section` from `XXXSectionStart` | **PROVEN** |
| `StartCTCExpression` / `ExpressionDef` / `QuestName` TNG keys? | **None** in Anniversary `FinalAlbion\*.tng` | **PROVEN** absent |
| `game.bin` `CExpressionDef` instance named Oakvale? | **No** `CExpressionDef` type in `names.tsv`. `EXPRESSION` rows are `EXPRESSION_FOLLOW`… social names, size 187 | **PROVEN** |
| `CActivateQuestDef` payload = Oakvale? | Six unnamed size-16 rows; field names **UNREAD** | **PARTIAL** / **UNREAD** payload |
| `007EF200` hardcoded Oakvale intern `0x012C5D14`? | **No.** Name is `[esi+120]` | **DISPROVEN** (sibling) |
| Leftover #50 Lookout TNG is a no-save Oakvale activate? | **No.** Lookout file has no that name | **DISPROVEN** |
| Wire `ActivateQuest(Q_NewOakValeIntro)` from this? | **No** | **DISPROVEN** |

---

## 1. `007EF200` name slot (reminder)

`listing-007c0000.txt`:

```
007EF303  mov esi, [ebp+12]          ; nested CExpressionDef*
007EF30E  mov eax, [esi+116]
          test eax, eax
          je  007EF36B               ; +116 set → camera 0041649C
007EF36B  lea ebx, [esi+120]
          push 0x122D70E             ; empty intern
          call 005FA740
          je  skip
          call 00415DD0              ; copy +120
          mov al, [esi+124]
          push eax / push 0 / push &copy
007EF3A1  call 004B4A10
```

**PROVEN** path (sibling `007EEF60-activate`). The concrete
string at `+120` on Lookout / Oakvale things was **UNREAD**
there. This note fills that from TNG / compiled defs.

CTC persist of the nested name is `"ExpressionDef"` at
`007EF0F9` / `007EF152` (`004109A0`). That is a **def
lookup name**, not a quest string.

---

## 2. Host TNG parser properties

`src/Fable.Formats/Tng/ThingFile.cs`:

| Token | Stored as |
|---|---|
| `XXXSectionStart <name>` | `ThingInstance.Section` |
| `NewThing <Kind>` | `Kind` |
| `DefinitionType` / `ScriptName` / `UID` / `Player` | named fields |
| `StartCTC*` … `EndCTC*` | `Properties["CTC….Key"]` (`Block.Key`) |
| other `Key Value` | `Properties[Key]` |

Native `00520D91` also matches `XXXSectionStart`
(`proofs/tng-spawn`). Section name is a **grouping /
quest-visibility bucket**, not `CExpressionDef+120`.

There is **no** host mapping from a TNG key onto
`CExpressionDef+120`. `QuestName` persist in the exe is
`0070278A` on a **different** object (`+24`, with
`Finished` / `Replayable` / `FeatIndex` / `ObjectiveName`)
and that token **never** appears in TNG.

---

## 3. `LookoutPoint.tng`

Anniversary loose; TLC WAD parse MATCH (`lookout-tng-walk`).

Sections:

```
XXXSectionStart Gameflow;          // M_Maze, M_LadyGameflow
XXXSectionStart NULL;
XXXSectionStart Q_FireHeart;
XXXSectionStart Q_GuildTraining;
XXXSectionStart Q_WaspBoss;
XXXSectionStart V_BeggarAndChild;
XXXSectionStart V_SickChild_Activate;
XXXSectionStart V_StatueMaster;
```

Grep `Q_NewOakValeIntro`: **0**. Grep `StartCTCExpression`
/ `CTCExpression` / `ExpressionDef` / `QuestName`: **0**.

CTC blocks present: `CTCPhysicsStandard`, `CTCEditor`,
camera, village, targeted, inventory, readable, light,
info-display, region enter/exit, fishing, container.
**Not** `CTCExpression`.

Leftover **#50** host `LoadGlobalThingsFile` `break`s on
the first `LoadedOnPlayerProximity` map and Notes
`004FBF60 LookoutPoint.tng`. That file cannot supply
`[CExpressionDef+120] = Q_NewOakValeIntro`. **PROVEN.**

Even if that parse constructed Things (`005223F0` gate
**UNREAD** live), there is still no Oakvale string on them.

---

## 4. `StartOakValeWest.tng`

WLD `NewMap 203` / `LoadedOnPlayerProximity TRUE`.
ContainsMap of region `StartOakVale` (index 4).

Sections:

```
XXXSectionStart NULL;                         // line 3
XXXSectionStart Q_NewOakValeIntro;            // line 20100
XXXSectionStart Q_NewOakValeIntro_PreAttack;  // line 21067
XXXSectionStart Q__OakValeIntro_PostAttack;   // line 22812
```

The **only** `Q_NewOakValeIntro` token (exact) is the
section start at line 20100. Properties on the things
inside are `DefinitionType`, `ScriptName`, `ScriptData`,
`ThingGamePersistent`, physics, editor, camera. **No**
quest-name field.

First thing in that section:

```
NewThing Marker;
DefinitionType "MARKER_BASIC";
ScriptName MK_OVI_ID_HERO;
StartCTCPhysicsStandard; … EndCTCPhysicsStandard;
StartCTCEditor; EndCTCEditor;
```

ScriptNames in `Q_NewOakValeIntro` (24 things):
`MK_OVI_ID_HERO`, `MK_OIF_LADYEND`, `MK_OIF_B2END`,
`CAM_OIF_SHOT10`, `CAM_OIF_SHOT8`,
`AffairWomanRunOffPoint`, `CAM_OVID_SHOT8`,
`MK_OVID_MAZE3`, `MK_OVID_BANDIT2`, `MK_OVID_DAD`,
`MK_OVID_CRYING`, `M_TriggerOutro`, `MK_OVI_DADTRIGGER`,
`MK_OVID_MAZE1`, `CAM_OVID_SHOT12`, `CAM_OVI_ID_MASTER`,
`CAM_OVI_ID_DAD`, `CAM_OVI_ID_DADMID`, `CAM_OVI_ID_WIDE`,
`CAM_OVI_ID_HERODAD`, `MK_OVIF_HERO2`, `MK_OVIF_HERO5`,
`MK_OVIF_HERO4`, `MK_OVIF_HERO3`.

`CAM_OVIF_SHOT2` / `HerosOldHouse` / `NOVStartHSP` live in
the **NULL** section (lines 2405 / 1160 / 17779), **before**
the Oakvale intro section. Intro *view* gizmos are not the
section named `Q_NewOakValeIntro`.

`StartOakValeEast.tng` line 2256:
`XXXSectionStart Q_NewOakValeIntro_PreAttack` only.

---

## 5. `game.bin` / names.bin

`assembly/compiled-defs/names.tsv`: **no** `CExpressionDef`,
**no** `Q_NewOakValeIntro`. Hits: `CExpressionSubDef` (39),
`CTCExpression` (type name), `CActivateQuestDef`.

`game/INDEX.md` / `entries.tsv`:

| Type | Count | Notes |
|---|---:|---|
| `EXPRESSION` | 39 | `EXPRESSION_FOLLOW`, `EXPRESSION_WAIT`, `EXPRESSION_FLIRT`, … size **187**, 0 extra named fields in the dump |
| `CExpressionSubDef` | 39 | unnamed rows, size 11 |
| `CActivateQuestDef` | 6 | size **16**, 0 fields; `NULLDEF` + five unnamed |

RTTI `0x01376DCC` `CExpressionDef` exists on the exe.
Ctor `004DB050` writes vtbl `0x012401F4`, zeros `+12`.
Slot 22 intern is `"CTCExpression"` (`004D4B75`). Compiled
bank type name for the 187-byte rows is **`EXPRESSION`**,
not `CExpressionDef`. None of those instance names is a
`Q_*` quest.

`CActivateQuestDef` → later `00843FC0` → `004B4A10([this+168])`
is a **different** activator. Payload strings **UNREAD**.
Same ctor also queues `"Expression_Follow"`. **DISPROVEN**
as a hardcoded Oakvale literal (sibling
`q-novi-activator-callers`).

---

## 6. QST (not TNG / not `+120`)

TLC `FinalAlbion.qst`:

```
AddQuest("Q_NewOakValeIntro", FALSE);
AddQuest("Q_NewOakValeIntro_PreAttack", FALSE);
AddTestQuest("Q_NewOakValeIntro", "NOVStartHSP", …);
```

**PROVEN** catalog / `world+184` / test card `+196`.
`AddQuest FALSE` does **not** `004B4A10`.
`EngineLifecycleTests.No_save_does_not_activate_Q_NewOakValeIntro`.

---

## 7. Timing / leftover #50 vs #4

```
00507C30 Load .wld
  004FDBC0  prox .tng parse          // CurrentRegion unset
    host leftover #50: first prox only = LookoutPoint.tng
    native: ebx=1… all prox, including StartOakValeWest map 203
  Set Static Map                     // after 00507C30
later 00501450 ContainsMap construct // no-save: Lookout region
first Present: Lookout + 006B3FF0    // leftover #4 keep open
```

Native would **parse** Oakvale West TNG during global things
(prox TRUE). That still:

- does not construct first Present (`004FDBC0` open ≠
  `0051FD80`; sibling `004FDBC0-open`);
- does not run `007EF200` on live `[thing+145]` before a
  region exists;
- does not put `Q_NewOakValeIntro` into `CExpressionDef+120`
  (section token only).

If someone later ticks `CTCExpression` on an Oakvale West
thing **after** that map is the current region, that is
**after** the quest world is already Oakvale — chicken-egg
vs “who first `004B4A10`s this name on no-save Lookout.”

Host must **not** treat leftover #50 Lookout TNG load as
Oakvale activate. Must **not** collapse #4.

---

## Timeline (this hunt)

```
TNG LookoutPoint          sections Gameflow / NULL / Q_FireHeart / …
                          Q_NewOakValeIntro ABSENT
TNG StartOakValeWest      XXXSectionStart Q_NewOakValeIntro
                          → ThingInstance.Section only
game.bin EXPRESSION       social names, not Q_*
CActivateQuestDef         16-byte unnamed; payload UNREAD
007EF200                  [CExpressionDef+120] vs empty intern
                          no TNG/def here equals Q_NewOakValeIntro
```

---

## Host

No `ActivateQuest("Q_NewOakValeIntro")`. No change to
`ThingFile` / leftover #4 / leftover #50.
**MATCH** skip.

---

## Remaining **UNREAD**

1. Live bytes at `CExpressionDef+120` on a spawned Thing
   that actually has component `0x8F` (none in these TNG
   `StartCTC*` blocks; attach may come from creature def
   sub-objects — not dumped here).
2. `CActivateQuestDef` 16-byte intern / `+44` bool.
3. Persist field **name** for `CExpressionDef+120` (not
   TNG `QuestName`; not `"ExpressionDef"` nested lookup).

Until a live `+120` equals `Q_NewOakValeIntro`, the no-save
activator stays **UNKNOWN** and must not be invented.
