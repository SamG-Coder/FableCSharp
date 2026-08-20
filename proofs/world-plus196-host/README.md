# `world+196` first read vs host

Investigation only. No production `src/` / `tests/` edits.

Do **not** start Oakvale / `00DBDE40` / `S_QNOVI`.
`Q_NewOakValeIntro` and `NOVStartHSP` sit in the 28-byte
`AddTestQuest` card. They are **not** consumed from
`CWorld+196` on no-save New Game.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH** /
**MISMATCH**.

Authority: `proofs/world-plus196-first-read` (dump first
reader); `proofs/addtestquest-token`;
`proofs/oakvale-later-activate`;
`proofs/host-qm44-clear`;
`proofs/script-setnewstart`;
`proofs/region-travel-first`;
`src/Fable.Game/EngineLifecycle.cs`
(`LoadQuestDefs` / `StoreAddQuestNames` /
`InitCharactersAndQuests` / `SpawnHeroFromPlayerStart`);
`src/Fable.Formats/Qst/QuestFile.cs`;
`src/Fable.Game/RegionTravel.cs`;
`src/Fable.Game/FirstSceneWorld.cs`;
`EngineLifecycleTests.No_save_does_not_activate_Q_NewOakValeIntro`.
Dump cites stay on the first-read proof. Do not re-prove
`004A113B` store or `0049F247` `lea edx, [esi+172]`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Native first **play-path** reader of filled `world+196`? | **Nobody.** Writer / empty clear / dtor only. | **PROVEN** (`world-plus196-first-read`) |
| Native first dump **consumer**? | Leftover `0061A8A0` (`00686A80` + `add eax, 0xC4`). | **PROVEN leftover** |
| Host first play-path reader of `+196`? | **Nobody.** No `_worldPlus196`. Init Quests walks `_worldPlus172`. | **MATCH** |
| Host first leftover that **copies the vector**? | **None.** No `0061A8A0` / `PC_QUESTS_SELECTION_MENU`. | **MATCH** skip |
| First leftover **string** that looks like a `+196` use? | `RegionTravel.NewGameStartScript = "NOVStartHSP"` (`record+4`). `FindPlayerStart` / `FirstSceneWorld` only. Live spawn is `GuildArrivalHSP`. | **LEFTOVER** (not a vector read) |

---

## Verdict

**MATCH on first read. First leftover is native `0061A8A0`.**

No-save New Game does not walk filled `world+196` on either
side. Native `004B4260` is `+172`. Host `InitCharactersAndQuests`
is the same list. Host must **not** invent
`ActivateQuest("Q_NewOakValeIntro")` from this store, and
must **not** implement `0061A8A0` as Leave work.

The store itself is still a **gap**: `QuestFile.Parse` is
`AddQuest` only, so the 112 FinalAlbion cards never land in
a host list. That is **MISMATCH** vs the native fill,
**MATCH** vs the first-read (nothing later reads them).
The `AddTestQuestStoreFn` line is a Note, not a consumer.

First leftover that *would* read the filled cards is
`0061A8A0`. Host has no such walk. The first leftover that
*mentions* a card field is hardcoded `NOVStartHSP` on
`FindPlayerStart`. `EngineLifecycle` does not call it.

---

## Timeline (no-save New Game)

```
native                              host EngineLifecycle
------                              --------------------
004A68D2  +196 triple = 0           no list
004A08D0  clear empty +196          Note only (no .Clear of +196)
004A16EA  push_back 28 B × 112      QuestFile.Parse drops AddTestQuest
0049F24E  004B4260([world+172])     foreach _worldPlus172 ActivateNamedQuest
0061A8A0  no E8                     no 0061A8A0
004A6BC7  dtor free                 —
Spawn: GuildArrivalHSP              SpawnHeroFromPlayerStart(GuildArrivalHSP)
FindPlayerStart(NOVStartHSP)        FirstSceneWorld / WorldGeometry only
```

---

## 1. Native first read (existing proof)

`world-plus196-first-read`:

- Fill is `004A113B` / `004A16EA` `lea esi, [ebp+196]`.
- Empty read before fill: `004A08D0` `004A090D`.
- First activate is **not** this vector
  (`0049F247` `+172`, stride 4).
- Gameflow wait is a **PE** `"Q_NewOakValeIntro"`, not a
  row walk. `NOVStartHSP` is QST-only.
- First copy of the filled range is leftover
  `0061A8A0`. Confirm `0061AB30` needs `[this+343]`.
  Oakvale card nonempty → `004B4C50`, not first
  `004B4A10`. If `[this+352]==0`,
  `004AF610("Q_NewOakValeIntro")` is false and the row
  drops. **0** `E8` of `0061A6A0` / `006224C0` on this
  walk.

Do not re-list the xrefs. **PROVEN** there.

---

## 2. Host store / clear (not a reader)

`LoadQuestDefs` (`EngineLifecycle` ~6645):

```
Note(AddTestQuestStoreFn, … "004A113B AddTestQuest [world+196] store not 004B4A10");
_worldPlus172.Clear();
_worldPlus184.Clear();
Quests = null;
if FinalAlbion.qst:
    Note(QstClearFn, … "004A08D0 flag 1 clear +184/+172/+196");
    Quests = QuestFile.Load(…);
    StoreAddQuestNames(Quests);
if GlobalQuests.qst:
    StoreAddQuestNames(global);   // append, no second world clear
```

`StoreAddQuestNames` walks `file.Quests` only:

```
_worldPlus184.Add(name);
if Persistent: _worldPlus172.Add(name);
_questManagerPlus44.Add(name);
```

There is **no** `_worldPlus196` / `WorldPlus196`.
`WorldAddTestQuestOffset = 196` is used in Note strings.

`QuestFile.Parse` regex:

```
AddQuest("name", TRUE|FALSE)
```

No `AddTestQuest`. 112 native rows (first
`Gameflow` / `NOVStartHSP` / `2`) are dropped.
**MISMATCH** vs `004A113B`. **PARTIAL** vs the
`004A08D0` `+196` erase (Note names the offset; no
buffer). Same gap as `host-qm44-clear` / `qst-first-load`.

`LoadQuestsFn` comment still says
“AddQuest / AddTestQuest into world+184”.
`AddTestQuest` is `+196`. **STALE** comment. Not a reader.

`No_save_does_not_activate_Q_NewOakValeIntro` locks the
Note, `WorldPlus172` miss, `ActivatedQuests` miss. It
does **not** assert a card list. **PROVEN** skip.

---

## 3. Host first read of filled `+196` — none. MATCH

`InitCharactersAndQuests`:

```
var names = _worldPlus172;
Note(InitQuestsFn, … "004B4260 [world+172] …");
foreach (var name in names)
    ActivateNamedQuest(name, "Init Quests");
Note(QuestManagerActivate, … "004B2890");
Note(ActivateInitialQuestsSite, … "skip 004B4A10");
```

Same pairing as native `0049F24E` / `0049F259` /
`00416BCF`. `Q_NewOakValeIntro` is FALSE → not in
`+172` → not activated. Gameflow wait is the PE name
(`GameflowYieldQuest`), not `record+0`.

`SpawnHeroFromPlayerStart` prefers
`EngineLifecycle.GuildArrivalHsp`, then any positioned
`HOLY_SITE_PLAYER_START`. It does **not** call
`FindPlayerStart`. **MATCH** first-region spawn
(`first-region-after-leave` / `script-setnewstart`).

No host site copies a 28-byte card, reads `record+4`,
or calls `004A0940` / `004B4C50` from this vector.

| Host | Native after Leave | Class |
|---|---|---|
| Init Quests = `_worldPlus172` | `004B4260([world+172])` | **MATCH** |
| No `WorldPlus196` walk | no play-path reader | **MATCH** |
| Note `004A113B` store not activate | 28-byte store only | **MATCH** note |
| Skip `004B4A10` | `+90584` empty | **MATCH** |
| No `ActivateQuest(Q_NewOakValeIntro)` | no play-path reader | **MATCH**; **DIVERGE** if added |
| No `0061A8A0` | leftover UI, 0 `E8` | **MATCH** skip |
| `QuestFile` drops 112 cards | fill `+196` | **MISMATCH** store; **MATCH** first-read |

---

## 4. First leftover

Native first leftover **consumer of the vector** is
`0061A8A0` (`PC_QUESTS_SELECTION_MENU` /
`006224C0` / `0061A6A0`). Host does not implement that
menu. Adding it on Leave / first type-1 is leftover
theater.

Host first leftover that **looks** like `+196` is the
hardcoded HSP, not a list walk:

```
RegionTravel.NewGameStartScript = "NOVStartHSP";  // record+4
FindPlayerStart → NOVStartHSP, StartOakValeHSP, MAIN_START_POSITION, …
FirstSceneWorld.Kid fallback → FindPlayerStart
WorldGeometry.Build → FindPlayerStart
```

`EngineLifecycle` does not reference `FindPlayerStart`.
Live New Game spawn is `GuildArrivalHSP`.
`FindPlayerStart(LookoutPoint)` would pick
`MAIN_START_POSITION` and miss Guild Arrival
(`script-setnewstart`). **LEFTOVER** façade.
**DISPROVEN** as the first `+196` reader.

`RegionTravel` type header still claims kid start is
`StartOakVale` / `NOVStartHSP` from
`AddTestQuest("Q_NewOakValeIntro","NOVStartHSP")`.
That documents the QST field. It is **not** Leave.

Do **not** grow `_worldPlus196` only so
`FindPlayerStart` can “read” it. That would impersonate
`0061A8A0` / Oakvale intro, not first-read.

---

## Classifications (short)

1. **First play-path reader of filled `world+196` vs host —
   MATCH. Nobody.** Native first-read proof. Host has no
   list and walks `+172`.
2. **First leftover — native `0061A8A0`. PROVEN.** Host
   skip MATCH. Do not implement on no-save.
3. **Host `NOVStartHSP` / `FindPlayerStart` — LEFTOVER
   string, not a `+196` read. DISPROVEN as Leave spawn.**
   Live path is `GuildArrivalHSP`.
4. **`QuestFile` dropping `AddTestQuest` — MISMATCH store,
   MATCH first-read.** Note-only `004A113B` / `004A08D0 +196`.
   Inventing activate from this store is **DIVERGE**.
