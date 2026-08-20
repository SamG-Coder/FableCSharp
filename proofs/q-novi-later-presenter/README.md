# Later presenter of `Q_NewOakValeIntro` after a region is current

Investigation only. No production `src/` edits.

Do **not** invent `ActivateQuest("Q_NewOakValeIntro")` on
no-save. Do **not** collapse leftover **#4** (Lookout first
Present vs Oakvale intro view). Do **not** treat bind
`00CD6E6D` / Gameflow wait / Give `00DBE295` as construct.

KNOWN (not re-proved as new): nobody on no-save presents
the intern to `00CB5AD0`; bind `S_QNOVI` is Init Scripts
`00CD6E6D`; Gameflow waits forever for type-`0x33`; later
Give is `00DBE295` after AttackOver / PostAttack / Maze.

Question: which later Thing / TNG / `CActivateQuestDef` /
script opcode **would** present intern `0x012C5D14` after
a region is current? Does `[retail+8]=1` (`00430340`, New
Game) change Init Quests `world+172`? Who loads
`StartOakVale` without `PlayerRegionName`?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: Anniversary / TLC TNG parse
(`LookoutPoint.tng` / `StartOakValeWest.tng` /
`StartOakValeEast.tng`); `assembly/compiled-defs/game`
`CActivateQuestDef` rows 61 / 9241 / 9248 / 12277 /
12857 / 12874; `assembly/compiled-defs/script`
`entries.tsv` / `CS_OAKVALE_*` / `CRegionScriptDef`;
`listing-00400000.txt` `00430340` / `00416BC8` /
`0042F491`; `listing-00480000.txt` `0049F180`;
`listing-00d80000.txt` `00DBDE40`;
`GameBinFormatTests.CActivateQuestDef_payloads_*`;
`EngineLifecycle.ChildhoodTngQueuesActivateQuest=false`;
siblings `proofs/q-novi-construct-no-save-audit`,
`proofs/ctcexpression-quest-names`,
`proofs/007EF200-first-plus120`,
`proofs/00501450-e8-callers`,
`proofs/00DBDE40-after-activate`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Later TNG presenter after a region is current? | **None recovered.** Lookout 0 hits. Oakvale West/East = `XXXSectionStart` buckets, not CTC / `QuestName` / `004B4A10`. | **DISPROVEN** as TNG |
| `CActivateQuestDef` instance intern Oakvale? | **No.** Six 16-byte rows: NULLDEF / `Global_OpenChest` ×2 / `Global_GiveHeroItemsFromRewardChest` / `Global_TeleportToHeroGuild` / `Global_ToggleTimeDisplay`. | **DISPROVEN** |
| `S_QNOVI` / script.bin opcode? | **No.** `S_QNOVI` is native C++ (factory `00DBEF70`). `script.bin` 0 intern / 0 `ActivateQuest`. `CS_OAKVALE_*` cutscenes run **after** construct. No `CRegionScriptDef` for Oakvale. | **DISPROVEN** |
| `[retail+8]=1` (`00430340`) changes `world+172`? | **No.** `00430340` is `mov [ecx+8],1; ret` on frontend vtbl `0x01230CA0+16`. Init Quests `0049F24E` always `lea edx,[esi+172]`. Gate is `[0x13B8648]==0`, not retail+8. | **DISPROVEN** |
| Who loads `StartOakVale` without `PlayerRegionName`? | **Nobody as stay-current.** `00501450` `i=4` opens then `004FEEC0` unloads. Persist `00487C20` needs nonempty name. `00DBDE40` **waits** `vtbl+48`, does not enqueue. | **PROVEN** omit |
| Invent `ActivateQuest` so leftover #4 can be Oakvale? | **No.** First Present stays Lookout. | **DISPROVEN** |

---

## Verdict

**PROVEN no recovered later presenter.** The files that
exist after a region is current still do not supply intern
`0x012C5D14` to `004B4A10` / `004B4260` / `00CB5AD0`.

`StartOakValeWest.tng` only stores the name as
`XXXSectionStart` → `ThingInstance.Section` (visibility
bucket **on** the quest). That is a **consumer**, not a
presenter. Those things spawn only if the quest is already
active — leftover **#4** chicken-egg vs Lookout first
Present.

`[retail+8]=1` is the frontend New Game store. It does
**not** rewrite QST TRUE / `world+172`. No-save Init
Quests is still `00416BCA` `push 0` → `0049F180` →
`004B4260([world+172])` omitting Oakvale.

Nothing on this walk makes `StartOakVale` (WLD region
index **4**) the current region without persist
`PlayerRegionName`. `00DBDE40` yields on `vtbl+48` **after**
`S_QNOVI` construct. Give `00DBE295` is later still.

Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.

---

## Status table

| Claim | Class | Evidence |
|---|---|---|
| `LookoutPoint.tng` intern / `StartCTCExpression` | **DISPROVEN** | 0 `Q_NewOakValeIntro`; 0 `StartCTCExpression` / `QuestName` |
| `StartOakValeWest.tng` CTC / `+120` / `+168` / `0x6C+40` | **DISPROVEN** | only `XXXSectionStart` line 20100; first thing `MK_OVI_ID_HERO` physics+editor |
| `StartOakValeEast.tng` exact name | **DISPROVEN** | `XXXSectionStart Q_NewOakValeIntro_PreAttack` only |
| Other `FinalAlbion\*.tng` | **DISPROVEN** | sibling grep 0 extra hits |
| Childhood TNG queues activate | **DISPROVEN** | `ChildhoodTngQueuesActivateQuest=false` |
| `CActivateQuestDef` intern dword | **DISPROVEN** | test hex; `names.Get` Global_* |
| `script.bin` intern / `ActivateQuest` opcode | **DISPROVEN** | 0 hits; `CS_OAKVALE_*` 0 `ActivateQuest` |
| `CRegionScriptDef` Oakvale | **DISPROVEN** | rows are Lookout / Greatwood / Darkwood / …; no `REGION_OAK*` script |
| `S_QNOVI` presents itself | **DISPROVEN** | native; Give `00DBE295` after construct |
| `00430340` body | **PROVEN** | `C6 41 08 01` / `C3`; vtbl `0x01230CA0` slot 4 |
| `0049F180` reads retail+8 | **DISPROVEN** | `lea edx,[esi+172]`; flags `push 1` / `push 0` only |
| `00416BCA` gate | **PROVEN** | `[0x13B8648]==0`; editor skip `00416BC6` |
| `00501450` stay-current Oakvale | **DISPROVEN** | first `i=1` Lookout; `i=4` transient |
| Persist `PlayerRegionName` no-save | **DISPROVEN** | empty; `00487C20` not taken |
| `00DBDE40` loads the region | **DISPROVEN** | `call [eax+48]` wait; yield `vtbl+28` |
| Host invents no-save activate | **DISPROVEN** | Notes skip; test `No_save_does_not_activate_*` |

---

## 1. TNG after a region is current

### LookoutPoint (leftover #4 / #50 — first current)

First no-save TNG open `004FDBC0` / first Present region
is **LookoutPoint** (WLD NewRegion 1). Sections:

```
XXXSectionStart Gameflow;
XXXSectionStart NULL;
XXXSectionStart Q_FireHeart;
XXXSectionStart Q_GuildTraining;
XXXSectionStart Q_WaspBoss;
…
```

Grep `Q_NewOakValeIntro`: **0**.
Grep `StartCTCExpression` / `CTCExpression` /
`ExpressionDef` / `QuestName` /
`CTCActionUseActivateQuest` /
`CTCCarriedActionUseActivateQuest`: **0**.

**DISPROVEN** as presenter. Live `[thing+145]` on
Lookout still has no Oakvale CString to copy.

### StartOakValeWest (WLD map 203, region index 4)

`LoadedOnPlayerProximity TRUE`. ContainsMap of
`StartOakVale`. **Not** first Present.

```
XXXSectionStart NULL;                         // CAM_OVIF_SHOT2 / HerosOldHouse / NOVStartHSP
XXXSectionStart Q_NewOakValeIntro;            // line 20100 — ONLY exact token
XXXSectionStart Q_NewOakValeIntro_PreAttack;
XXXSectionStart Q__OakValeIntro_PostAttack;
```

Host `ThingFile` stores `XXXSectionStart` as
`ThingInstance.Section`. Native match `00520D91` is
the same grouping token.

First thing in `Q_NewOakValeIntro`:

```
NewThing Marker;
DefinitionType "MARKER_BASIC";
ScriptName MK_OVI_ID_HERO;
StartCTCPhysicsStandard; … EndCTCPhysicsStandard;
StartCTCEditor; EndCTCEditor;
```

24 ScriptNames in that section: markers / cameras
(`MK_OVI_*` / `CAM_OVI_*`). **No** quest-name field.
**No** `CActivateQuestDef`. **No** `CTCExpression`.

Section buckets in this engine are **visibility on an
already-active quest**. They do not `004B4A10`.
**PROVEN** consumer; **DISPROVEN** presenter.

`StartOakValeEast.tng`: `XXXSectionStart
Q_NewOakValeIntro_PreAttack` only.

### Chicken-egg vs leftover #4

To parse/tick Oakvale-West things as **current**,
region 4 must be current. No-save current is Lookout.
Those section things also want the quest **already
active**. They cannot be the first presenter.

Do **not** collapse intro-view gizmos in **NULL**
(`CAM_OVIF_SHOT2`) into an activate.

---

## 2. `CActivateQuestDef` payloads (closed)

`GameBinFormatTests.CActivateQuestDef_payloads_*`:

| Id | Intern at raw+7 | Flag |
|---:|---|---|
| 61 | `-1` NULLDEF | 1 |
| 9241 | `Global_OpenChest` | 0 |
| 9248 | `Global_OpenChest` | 0 |
| 12277 | `Global_GiveHeroItemsFromRewardChest` | 0 |
| 12857 | `Global_TeleportToHeroGuild` | 0 |
| 12874 | `Global_ToggleTimeDisplay` | 0 |

Every 4-byte window ≠ `0x012C5D14`. Runtime path
`007B5680` → `00843F50` → `00843FC0` →
`004B4A10([this+168])` still copies **def+40**, which
is those Global_* names (chest / guild teleport /
time display), not Oakvale. Neighbours in `entries.tsv`
are chest / carryable / inventory clusters.

**DISPROVEN** as later Oakvale intern.

---

## 3. `S_QNOVI` / script.bin / region scripts

`S_QNOVI` is **not** a `script.bin` `CScriptDef`.
Registrar `00CD6E14` / factory `00DBEF70` / ctor
`00DAAC00` / run `00DABAC0` are native.

`assembly/compiled-defs/script`:

| Hunt | Hits |
|---|---|
| `Q_NewOakValeIntro` | **0** |
| opcode `ActivateQuest` | **0** |
| `CRegionScriptDef` Oakvale | **0** (Lookout / Greatwood / Darkwood / Witchwood / Bandit / Hobbe only) |
| `CS_OAKVALE_INTRO_*` `ActivateQuest` | **0** |

`CS_OAKVALE_INTRO_FATHER` (index 481) is
`Hero.Teleport MK_OVI_ID_HERO` / `UseCamera
CAM_OVIF_SHOT2` / father speak. That runs from
already-ticking `S_QNOVI` **after** `00DBDE40`
map-wait. **DISPROVEN** as presenter.

`00DBE295` Give of the quest name is
`call [vtbl+1152]` **after** AttackOver
`00DBB2A7` + PostAttack + Maze. Circular:
unblocks Gameflow only if childhood already ran.

---

## 4. `[retail+8]=1` does not change Init Quests

`00430340` (`listing-00400000`, 5 bytes):

```
00430340  mov [ecx+8], 0x01
00430344  ret
```

`vtbl.tsv` `0x01230CA0` slot 4 = `0x00430340`.
Msg 15 New Game: `[retail+8]=1` then `[retail+41]=1`.
**0** `E8` of `00430340` (vtbl only).

Init Quests (`listing-00480000`):

```
0049F21B  push "Init Quests"
0049F23D  mov ecx, [0x13B89FC]
          push 1
          push 0
0049F247  lea edx, [esi+172]
0049F24E  call 004B4260
```

**No** `cmp [retail+8]`. **No** branch on New Game vs
Load. The TRUE vector is QST `AddQuest(..., TRUE)`
only. Oakvale is FALSE → not on this list.

Caller (`listing-00400000`):

```
00416ABF  cmp [0x13B8648], 0
          je  00416BC8               ; no-save
          … editor …
00416BC6  jmp 00416C31               ; SKIP 0049F180
00416BC8  push 0
00416BCA  call 0049F180
```

Gate is **editor** `[0x13B8648]`, not retail+8.
Second `E8` `004A2C80` `push 1` is save-only.

`0042F491` Init Game ends `0042F508 ret 4`. Next
`0042F50E cmp [esi+8],0` is a **different** function.
Game-object `00418B48 cmp [esi+8], bl` is the inner
pump / `WM_DESTROY` `[game+8]=1`, **not**
`00430340`.

**DISPROVEN:** New Game `[retail+8]=1` does not
put `Q_NewOakValeIntro` on `world+172`.

---

## 5. Who loads `StartOakVale` without `PlayerRegionName`?

| Candidate | Stay-current Oakvale? |
|---|---|
| `00501450` first `00500540(1,0,0)` | **LookoutPoint** |
| `00501450` loop `i=4` | `00500540(4,0,0)` then `004FEEC0(4,0)` **unload**. Transient. Not first. Not stay. |
| Persist `00487C20` | `00500540(index,0,1)` after name lookup. Empty no-save. **Not taken.** |
| `00DBDE40` | `push "StartOakVale"` / `call [eax+48]`. Miss → yield `vtbl+28`. **Wait, not load.** Needs `S_QNOVI` already constructed. |
| `004FDBC0` first prox TNG | **LookoutPoint.tng** (leftover #50) |

**PROVEN omit.** Leftover #4 first *rendered* scene is
Lookout. Intro *view* assets (`StartOakValeWest` /
`HerosOldHouse` / `CAM_OVIF_SHOT2`) stay on region 4.

Circular vs presenter:

```
need construct  →  00CB5AD0 hit  →  00DBEF70 / 00DAAC00
need region 4   →  00DBDE40 vtbl+48 wait  →  needs construct
TNG section     →  needs quest already active
Give 00DBE295   →  needs S_QNOVI already ticking
Gameflow 0x33   →  waits Give
```

No-save never enters this cycle. First Present is
Lookout; Gameflow yields forever.

---

## Timeline (no-save New Game)

```
0059A2DA  msg 15
00430340  [retail+8]=1                  // NOT world+172
0042F2A2  Leave frontend
0042F491  Init Game
00CD6E6D  00CB5C90 bind S_QNOVI         // NOT construct
004A0D90  AddQuest FALSE                // +184 / QM+44
00416BCA  0049F180 push 0               // [0x13B8648]==0
0049F24E  004B4260([world+172])         // Oakvale absent
user.ini  004B4A10("Gameflow")
004FDBC0  LookoutPoint.tng              // 0 Oakvale token
00501450  00500540(1,0,0) Lookout       // i=4 transient only
type-1    00CE7670 wait 0x33 FOREVER
00DAAC00 / 00DABAC0 / 00DBDE40          // NOT ENTERED
StartOakValeWest.tng                    // not current
```

---

## What this is not

| Claim | Class |
|---|---|
| Lookout TNG later `007EF200` intern | **DISPROVEN** (0 string) |
| Oakvale TNG `XXXSectionStart` = activate | **DISPROVEN** (section bucket) |
| `CActivateQuestDef` chest rows = Oakvale | **DISPROVEN** |
| `CS_OAKVALE_INTRO_FATHER` `ActivateQuest` | **DISPROVEN** (0 opcode) |
| `CRegionScriptDef` StartOakVale | **DISPROVEN** (no row) |
| `[retail+8]=1` adds TRUE Oakvale | **DISPROVEN** |
| `00501450` current = region 4 | **DISPROVEN** |
| `00DBDE40` enqueues StartOakVale | **DISPROVEN** (wait) |
| Host `ActivateQuest` on no-save | **DISPROVEN** |

---

## Remaining UNKNOWN

A live Thing **after some later region** whose copied
`CTCExpression+120` / action `+168` / `0x6C+40`
**equals** intern `0x012C5D14`. No recovered TNG,
def, or opcode fills that slot. Debug picker
`0061AB30` / save `004B5080` stay leftover / off
no-save.

Until that live CString dumps, the later presenter
stays **nobody recovered**. Do not invent it.

---

## Host

`ChildhoodTngQueuesActivateQuest=false`.
`ActivateNamedQuest` walks `world+172` only.
`No_save_does_not_activate_Q_NewOakValeIntro`.
`RetailPlus8StoreFn=0x00430340` Notes the frontend
store; Pump must **not** treat it as Oakvale
autostart. **MATCH.**

Do **not** add `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** load `StartOakVale` as first current.
Do **not** fold leftover #4 into this presenter.

---

## Sources (absolute)

- `C:\FableCSharp\assembly\compiled-defs\game\entries.tsv`
- `C:\FableCSharp\assembly\compiled-defs\script\entries.tsv`
- `C:\FableCSharp\assembly\compiled-defs\script\0481-CS_OAKVALE_INTRO_FATHER.md`
- `C:\FableCSharp\assembly\compiled-defs\script\0598-REGION_LOOKOUT_POINT.md`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00480000.txt`
- `C:\FableCSharp\assembly\exe\01-sections\text-map\listing-00d80000.txt`
- `C:\FableCSharp\assembly\exe\00-index\vtbl.tsv`
- `C:\FableCSharp\tests\Fable.Formats.Tests\GameBinFormatTests.cs`
- `C:\FableCSharp\proofs\ctcexpression-quest-names\README.md`
- `C:\FableCSharp\proofs\007EF200-first-plus120\README.md`
- `C:\FableCSharp\proofs\00501450-e8-callers\README.md`
- `C:\FableCSharp\proofs\00DBDE40-after-activate\README.md`
- `C:\FableCSharp\proofs\q-novi-construct-no-save-audit\README.md`
