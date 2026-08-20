# Every `CActivateQuestDef` instance vs `Q_NewOakValeIntro` / intern `0x012C5D14`

Investigation only. No production `src/` edits.

Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** treat `XXXSectionStart Q_NewOakValeIntro` as a
`CActivateQuestDef` payload. Do **not** treat names.bin
**offset** as PE CString intern `0x012C5D14`.
Sibling `proofs/cactivatequestdef-payloads` hex **UNREAD**
is **STALE**.

Question: which `CActivateQuestDef` `game.bin` / TNG
instances have payload `Q_NewOakValeIntro` or intern
`0x012C5D14`? Re-check the six 16-byte rows against
`FableCrc` of that name.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **UNKNOWN**.

Authority: TLC install `Fable.exe` +
`data\CompiledDefs\game.bin` / `names.bin` /
`script.bin` / `FinalAlbion.wad`;
`Fable.Dump bin CActivateQuestDef`;
`GameBinFormatTests.CActivateQuestDef_payloads_are_16_bytes_and_do_not_intern_Q_NewOakValeIntro`;
`src/Fable.Formats/Defs/FableCrc.cs` /
`GameBin.cs` / `NamesBin.cs` / `Tng/ThingFile.cs`;
`assembly/compiled-defs/game/entries.tsv` ids
**61 / 9241 / 9248 / 12277 / 12857 / 12874**;
`assembly/compiled-defs/names.tsv`;
`listing-00840000.txt` `00843F50` / `00843FC0`;
`listing-00780000.txt` `007B5740`;
siblings `proofs/cactivatequestdef-payloads`,
`proofs/012C5D14-fablecrc-imm`,
`proofs/oakvale-activate-unread-audit`,
`proofs/007EF200-first-plus120`,
`proofs/lookout-tng-walk`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| How many `game.bin` `CActivateQuestDef` rows? | **Six.** Ids **61 / 9241 / 9248 / 12277 / 12857 / 12874**. All **raw 16**, subdefs **0**. | **PROVEN** |
| Does any 16-byte body contain intern `0x012C5D14`? | **No.** Every 4-byte window ≠ that dword. | **PROVEN** |
| Does any row’s `+7` names.bin offset resolve to `Q_NewOakValeIntro`? | **No.** NULLDEF `-1`; two `Global_OpenChest`; then `Global_GiveHeroItemsFromRewardChest` / `Global_TeleportToHeroGuild` / `Global_ToggleTimeDisplay`. | **PROVEN** |
| Is `FableCrc("Q_NewOakValeIntro")` in those 16 bytes? | **No.** Live hash is **`0x8D19C362`**. Not a window. Field CRCs are `0x1FB35A1B` / `0xBF394B78`. | **PROVEN** |
| `names.bin` row for the quest? | **None.** `names.Find` null. Type rows are `NULLDEF_CActivateQuestDef` `0xD75076C4` / `CActivateQuestDef` `0xFA5557F6`. | **PROVEN** |
| `StartOakVale.tng`? | **Absent** from TLC WAD (796 entries). Region is WLD-only; maps are West / East / fillers / seas. | **PROVEN** |
| `LookoutPoint.tng` Oakvale / `CActivateQuestDef`? | **0** of either string. 288 things, 8 sections, none Oakvale. | **PROVEN** |
| `StartOakValeWest.tng`? | **`XXXSectionStart Q_NewOakValeIntro`** only (line 20100). 24 markers/cameras. **0** `CActivateQuestDef` / `ActivateQuest` / `StartCTCExpression`. | **PROVEN** section; **DISPROVEN** as def payload |
| Any WAD `*.tng` `CActivateQuestDef` instance? | **0** of 398 files. | **PROVEN** |
| Invent activate from these rows? | **No.** | **DISPROVEN** |

---

## Verdict

**Zero `CActivateQuestDef` instances carry
`Q_NewOakValeIntro` or intern `0x012C5D14`.**

The six `game.bin` bodies are 16-byte persist records.
`+7` is a **names.bin offset** (or `-1`), not a PE
CString. Inflated hex is now **read**. Sibling
`cactivatequestdef-payloads` “intern dword UNREAD” is
**closed**.

`FableCrc("Q_NewOakValeIntro")` from `FableCrc.cs`
(poly `0xEDB88320`, init 0, ASCII, no NUL) is
**`0x8D19C362`**. That value is **not** in the six
rows, **not** in `names.tsv`, and is **not** the
field CRC. Sibling `012C5D14-fablecrc-imm`
`0x02C878A8` is **DISPROVEN** against the same hasher
that **MATCH**es `names.tsv` `UI` / `ENGINE` /
`CActivateQuestDef`.

TNG never stores this class. Lookout (first no-save
file) has no Oakvale token. There is **no**
`StartOakVale.tng`. West’s section bucket is
`ThingInstance.Section`, not `def+40`.

Do **not** add `ActivateQuest("Q_NewOakValeIntro")`.

---

## 1. Six `game.bin` rows — hex is read

`Fable.Dump bin CActivateQuestDef` on TLC
`data\CompiledDefs\game.bin` (14761 entries). Layout
**3-byte** GameBin header + field CRC + u32 + field
CRC + bool = **16**:

| Id | Hex | `+3` CRC | `+7` u32 | `names.Get(+7)` | `+15` |
|---:|---|---|---|---|---:|
| 61 | `0000001B5AB31FFFFFFFFF784B39BF01` | `0x1FB35A1B` | `0xFFFFFFFF` | none | 1 |
| 9241 | `0100011B5AB31F8D1F0500784B39BF00` | same | `0x00051F8D` | `Global_OpenChest` | 0 |
| 9248 | same as 9241 | same | `0x00051F8D` | `Global_OpenChest` | 0 |
| 12277 | `0100011B5AB31FEFA10500784B39BF00` | same | `0x0005A1EF` | `Global_GiveHeroItemsFromRewardChest` | 0 |
| 12857 | `0100011B5AB31F51A60500784B39BF00` | same | `0x0005A651` | `Global_TeleportToHeroGuild` | 0 |
| 12874 | `0100011B5AB31FD0A60500784B39BF00` | same | `0x0005A6D0` | `Global_ToggleTimeDisplay` | 0 |

`names.tsv` offsets **MATCH** those `+7` dwords:

| Offset | CRC | Name |
|---|---|---|
| `0x00051F8D` | `0x7B20749F` | `Global_OpenChest` |
| `0x0005A1EF` | `0x02BD84B4` | `Global_GiveHeroItemsFromRewardChest` |
| `0x0005A651` | `0x6C4BBB33` | `Global_TeleportToHeroGuild` |
| `0x0005A6D0` | `0xBE4EC8F8` | `Global_ToggleTimeDisplay` |

Every overlapping 4-byte window of every row:

- **≠** PE intern `0x012C5D14` (`14 5D 2C 01`)
- **≠** `FableCrc("Q_NewOakValeIntro")` `0x8D19C362`
- **≠** stale sibling `0x02C878A8`

Id 61 is `NULLDEF_CActivateQuestDef`. The other five
are unnamed type-only rows (`GuessInstanceName` falls
back to `CActivateQuestDef`). ASCII `ExtractAscii` in
`entries.tsv` is empty — the name lives as an
**offset**, not a 4+ char run.

`script.bin`: **0** `CActivateQuestDef` types; **0**
intern / name-CRC dwords across 611 inflated `Raw`.
`frontend.bin` dump: **0** type hits.

---

## 2. `FableCrc` of the quest name vs field CRCs

Live `FableCrc.Hash` (same routine as `names.bin`):

| Seed | Hash | Check |
|---|---|---|
| `Q_NewOakValeIntro` | **`0x8D19C362`** | not in `names.tsv` (string absent) |
| `Q_NewOakValeIntro` + NUL | `0xA33119B7` | not in the 16 bytes |
| lower-case | `0x707EB959` | not in the 16 bytes |
| `CActivateQuestDef` | `0xFA5557F6` | **MATCH** `names.tsv` |
| `UI` | `0xC8CC5025` | **MATCH** |
| `ENGINE` | `0xA9927CA8` | **MATCH** |
| `QuestName` | `0x06A69EC7` | **≠** field `0x1FB35A1B` |
| `AlwaysActive` | `0x3D6AECFC` | **≠** field `0xBF394B78` |

File field `0x1FB35A1B` is the persist id shared with
`EXPRESSION+120` (`00456A5A-expression-plus120`).
Lionhead name of that CRC stays **UNREAD**. It is
**not** `FableCrc("Q_NewOakValeIntro")` and **not**
`FableCrc("QuestName")`.

`+11` `0xBF394B78` is the bool field (runtime
`CActivateQuestDef+44`). **Not** the quest CRC.

Sibling `012C5D14-fablecrc-imm` claimed
`FableCrc("Q_NewOakValeIntro")==0x02C878A8`. That
constant is **not** what `FableCrc.cs` returns. UI /
ENGINE checks in that sibling **do** MATCH; the quest
hash there is **DISPROVEN**. Neither wrong nor live
hash is a `.text` imm needed here: activation of this
name is a **CString intern**, and these 16 bytes never
hold it.

Runtime persist `007B5740` still copies **`[def+40]`**
as a CString after compile. File bytes at `+7` are the
**offset**. Compile leftover `009B08C0` would resolve
`Global_OpenChest` etc., **not** Oakvale.

---

## 3. Parent OBJECT defs are still not Oakvale

`game.bin` subdef `NameCrc=0xFA5557F6`
(`FableCrc("CActivateQuestDef")`) points at the six
ids:

| Parent | Subdef id | Payload name |
|---|---:|---|
| `OBJECT_CHEST_OPENABLE_TPL` / `OBJECT_CHEST_OPENABLE` / `OBJECT_NW_CHEST_OPENABLE` / `OBJECT_SILVERKEY_CHEST_*` | 9241 | `Global_OpenChest` |
| `OBJECT_SARCOPHAGUS_*` / `OBJECT_NW_SARCOPHAGUS_01` | 9248 | `Global_OpenChest` |
| `OBJECT_CHEST_REWARD_ON_DEATH` | 12277 | `Global_GiveHeroItemsFromRewardChest` |
| `OBJECT_GUILD_SEAL_1` | 12857 | `Global_TeleportToHeroGuild` |
| `OBJECT_POCKET_WATCH` | 12874 | `Global_ToggleTimeDisplay` |

NULLDEF **61** has **no** parent subdef. Use-item path
`007B57C0` / `007EF600` → `00843F50([def+40])` therefore
activates those **Global_*** / expression-style names.
**DISPROVEN** as `Q_NewOakValeIntro`.

`00843F50` six `E8`s still push
`Expression_Follow` / `Wait` / `Fish` / `Dig` or copy
`[def+40]`. **None** `push 0x012C5D14`.

---

## 4. TNG — no `CActivateQuestDef` instance

TLC `FinalAlbion.wad` **398** `*.tng`. ASCII search:

| Needle | Hits |
|---|---|
| `CActivateQuestDef` | **0** |
| `ActivateQuest` | **0** |
| `StartCTCExpression` | **0** |
| `CTCCarriedActionUseActivateQuest` | **0** |
| `Q_NewOakValeIntro` | **West 2 / East 1** (section tokens only) |

`StartCTCCarried` appears in `WitchwoodCavern.tng` and
`DemonDoor_Guild.tng` (**not** Oakvale, **not** this
class).

### `StartOakVale.tng`

**No such file.** `wad.Find("StartOakVale.tng")` null.
WLD region `StartOakVale` Contains
`StartOakValeWest` / `StartOakValeEast` and Sees
fillers / seas. Filler / sea / MemorialGarden TNG are
**12-byte** `Version 2;\r\n` stubs.

### `LookoutPoint.tng` (first no-save open)

197526 bytes, **288** things, sections:

```
Gameflow            2     ; M_Maze, M_LadyGameflow
NULL              252
Q_FireHeart         3
Q_GuildTraining    10
Q_WaspBoss          3
V_BeggarAndChild   14
V_SickChild_Activate 3
V_StatueMaster      1
```

**0** `Q_NewOakValeIntro`. **0** `CActivateQuestDef`.
CTC blocks are physics / editor / camera / village —
not expression / activate-quest. **PROVEN** this file
cannot be an Oakvale `CActivateQuestDef` instance.

### `StartOakValeWest.tng`

```
20100  XXXSectionStart Q_NewOakValeIntro;
21067  XXXSectionStart Q_NewOakValeIntro_PreAttack;
```

Section `Q_NewOakValeIntro`: **24** things. First is
`MARKER_BASIC` `MK_OVI_ID_HERO`. Keys: Player / UID /
DefinitionType / ScriptName / ScriptData / persist /
`CTCPhysicsStandard.*` / Health. **No** quest-name
property. Host stores the token as
`ThingInstance.Section` (`ThingFile.cs`).

That is a **section bucket** after the map exists,
not `CActivateQuestDef+40`, not `004B4A10`. Chicken-egg
vs first Lookout Present (`007EF200-first-plus120`).

### `StartOakValeEast.tng`

One hit: `XXXSectionStart Q_NewOakValeIntro_PreAttack`
(substring of the PreAttack name). **No** exact
`Q_NewOakValeIntro` section. **0** `CActivateQuestDef`.

---

## What this is not

| Claim | Class |
|---|---|
| Six 16-byte hex UNREAD | **STALE** (`cactivatequestdef-payloads`); hex **PROVEN** this pass |
| Any row intern dword `0x012C5D14` | **DISPROVEN** |
| Any row names offset → `Q_NewOakValeIntro` | **DISPROVEN** |
| `FableCrc("Q_NewOakValeIntro")` is field `0x1FB35A1B` | **DISPROVEN** |
| That hash is `0x02C878A8` | **DISPROVEN** (live `0x8D19C362`) |
| `names.bin` stores the quest | **DISPROVEN** |
| `StartOakVale.tng` exists | **DISPROVEN** |
| Lookout TNG presents Oakvale / this class | **DISPROVEN** |
| West `XXXSectionStart` is `CActivateQuestDef` | **DISPROVEN** |
| Chest / guild-seal / watch use-item is Oakvale | **DISPROVEN** (`Global_*`) |
| Host `ActivateQuest("Q_NewOakValeIntro")` from these rows | **DISPROVEN** |

---

## Remaining UNKNOWN

1. Lionhead name of persist CRC `0x1FB35A1B` (shared
   with `EXPRESSION+120`). Not required to reject
   Oakvale.
2. First live Thing after a **later** region whose
   copied CString equals intern `0x012C5D14` —
   **not** a `CActivateQuestDef` file instance.

Until (2) dumps a live name, the no-save presenter of
`Q_NewOakValeIntro` to `00CB5AD0` stays **nobody**.
These six defs are **not** that presenter.

---

## Sources (absolute)

- `C:\Program Files (x86)\Steam\steamapps\common\Fable The Lost Chapters\data\CompiledDefs\game.bin`
- `C:\Program Files (x86)\Steam\steamapps\common\Fable The Lost Chapters\data\CompiledDefs\names.bin`
- `C:\Program Files (x86)\Steam\steamapps\common\Fable The Lost Chapters\data\CompiledDefs\script.bin`
- `C:\Program Files (x86)\Steam\steamapps\common\Fable The Lost Chapters\data\Levels\FinalAlbion.wad`
- `C:\FableCSharp\assembly\compiled-defs\game\entries.tsv`
- `C:\FableCSharp\assembly\compiled-defs\names.tsv`
- `C:\FableCSharp\src\Fable.Formats\Defs\FableCrc.cs`
- `C:\FableCSharp\tests\Fable.Formats.Tests\GameBinFormatTests.cs`
- `C:\FableCSharp\proofs\cactivatequestdef-payloads\README.md`
- `C:\FableCSharp\proofs\012C5D14-fablecrc-imm\README.md`
- `C:\FableCSharp\proofs\oakvale-activate-unread-audit\README.md`
