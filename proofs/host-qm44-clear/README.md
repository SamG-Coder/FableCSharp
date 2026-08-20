# Host `LoadQuestDefs` clears `+172`/`+184` but not `QM+44`

Investigation only. No production `src/` edits.

Question: Host `LoadQuestDefs` `.Clear()`s `_worldPlus172` /
`_worldPlus184` and does **not** `.Clear()` `_questManagerPlus44`.
Does that **MATCH** `004A08D0`? Is the skip a **double New Game
leftover**?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Authority: `src/Fable.Game/EngineLifecycle.cs` (`LoadWorld` /
`LoadWorldMap` / `LoadQuestDefs` / `StoreAddQuestNames` /
`EnterGame`); `proofs/qst-clear-004A08D0`;
`proofs/quest-manager-plus44`; `proofs/qm44-gate-find`;
`proofs/qst-first-load`; `proofs/004A1840-second-site`;
`proofs/init-world-004A6E30`;
ExeIndex `listing-00480000.txt` `004A08D0` / `004A0D90` /
`004A1840` / `004A6550` / `004A6697` / `004A9A10` /
`004B2850` / `004B4590`;
`e8.tsv` dest `004B4590` (one site `004A6697`);
`EngineLifecycleTests.Init_quests_004B4260_activates_wld_initial_list`.

Do **not** start at `Q_NewOakValeIntro` / `S_QNOVI` /
`00DBDE40`. That name is FALSE + `AddTestQuest`, not this
clear.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Host `LoadQuestDefs` clears `+172` / `+184`? | **Yes.** `_worldPlus172.Clear()` / `_worldPlus184.Clear()` then flag-1 `Note(QstClearFn, … "004A08D0 … +184/+172/+196")`. | **PROVEN** |
| Host `LoadQuestDefs` clears `QM+44`? | **No.** `_questManagerPlus44` is not touched here. `StoreAddQuestNames` `Add`s after the world clears. | **PROVEN** skip |
| Match `004A08D0`? | **Yes.** Native flag-1 erase is `world+184` / `+172` / `+196` only. **No** `[0x13B89FC]+44` walk, no `004B2850` reverse, no manager store. | **MATCH** (`qst-clear-004A08D0`) |
| Double New Game leftover? | **No** on `EnterGame` / `LoadWorld`. `QM+44` is emptied by **`004B4590`** (host: `LoadWorld` `.Clear()`), **before** `004A1840`. A second parse does not need `004A08D0` to wipe `+44`. | **DISPROVEN** as leftover |
| Put `_questManagerPlus44.Clear()` inside `LoadQuestDefs`? | **No.** That would impersonate the ctor, not `004A08D0`. | **DIVERGE** if added |

---

## Verdict

**`LoadQuestDefs` skipping `QM+44` MATCHES `004A08D0`.
It is not a double New Game leftover.**

Three lists, two owners:

| Slot | Owner | Flag-1 `004A08D0` | Host `LoadQuestDefs` | Host `LoadWorld` |
|---|---|---|---|---|
| `world+184` | `CWorld` | `0043336A` `clear` | `_worldPlus184.Clear()` | — |
| `world+172` | `CWorld` | `0043336A` `clear` | `_worldPlus172.Clear()` | — |
| `world+196` | `CWorld` | `004AA580` / `004ABD90` | **Note only** (no list) | — |
| `QM+44` | `[0x13B89FC]` | **not written** | **not cleared** | `_questManagerPlus44.Clear()` as `004B4590` |

`+184` / `+172` / `+196` live on the world. `QM+44` lives on
the QuestManager singleton. `AddQuest` copies the same name
into all three catalogs **after** the flag-1 erase
(`004A1101` `004B2850`). Reloading QST with flag 1 can
desync `+184` from `+44` **on purpose** if the manager is
not reconstructed. New Game reconstructs the manager first.

---

## Timeline (no-save New Game)

```
0041735A  Init World
  004A67D0  CWorld ctor
    004A68AE  [world+172/+184/+196] triples = 0

00416953  Loading world
  [world].vtbl+28  004A6550                 // first insn
    alloc 0xB4
    004A6697  call 004B4590                 // QM ctor
      004B45BF  [QM+44/+48/+52]=0
    004A66A1  ecx=0x13B89FC
    004A66A6  call 004A9A10                 // install / release old
  [+90588] empty
  00416ABA  call 004A1840
    004A0D90(FinalAlbion.qst, 1)
      004A08D0  clear world +184 / +172 / +196   // NOT QM+44
      AddQuest → +184; TRUE → +172; 004B2850 +44
    004A0D90(GlobalQuests.qst, 0)
      skip 004A08D0; append all three

0049F24E  004B4260([world+172])
  004B00C0  find in QM+44
```

Host pairing:

```
EnterGame
  … Init World notes 004A67D0 / 004A6E30 …
  LoadWorld
    _questManagerPlus44.Clear()             // 004B4590
    Note(004B2850, "004B4590 [0x13B89FC]+44=0")
    LoadWorldMap
      LoadQuestDefs
        _worldPlus172.Clear()               // 004A08D0
        _worldPlus184.Clear()               // 004A08D0
        // _questManagerPlus44 stays empty
        Note(004A08D0, "clear +184/+172/+196")
        StoreAddQuestNames(FinalAlbion)     // +184 / TRUE +172 / +44
        StoreAddQuestNames(GlobalQuests)    // append, no clear
```

**PROVEN** order: manager zero **then** world-vector
`clear` **then** `AddQuest` push. Same as native.

---

## 1. `004A08D0` does not walk `QM+44` — **PROVEN**

`004A0D90` `test al, al` / `je 004A0DA7`: flag 1 calls
`004A08D0` (`ecx` = `CWorld`). Flag 0 skips.

`004A08D0`…`004A093F` `ret` (`qst-clear-004A08D0`):

```
004A08D4  [world+184]  0043336A(begin,end)   // CString×4
004A08EE  [world+172]  0043336A(begin,end)
004A0907  [world+196]  empty 004AA580 + 004ABD90; [+200]=begin
```

Writes: two vector `end` dwords plus `[world+200]`.
No `00BFEA14` of buffers. No `world+208`. No
`[0x13B89FC]`. No `004B2850`. **PROVEN** absence.

`004A1840`: FinalAlbion `push 1` (`004A1931`);
GlobalQuests `push 0` (`004A1991`). **PROVEN.**

`AddQuest` after the clear (`004A10F6`):

```
004A1080  lea esi, [ebp+184]      // always
004A10B2  test bl, bl             // TRUE
004A10C4  lea esi, [ebp+172]      // TRUE only
004A10F6  mov ecx, [0x13B89FC]
004A1101  call 004B2850           // QM+44 always
```

Only `E8` of `004B2850` in the listings: `004A1101`.
`004A08D0` cannot empty `+44`. **PROVEN.**

---

## 2. Host `LoadQuestDefs` is the flag-1 / flag-0 pair

```
private void LoadQuestDefs()
{
    _worldPlus172.Clear();
    _worldPlus184.Clear();
    Quests = null;
    if (Install…QuestPath)
    {
        Note(QstClearFn, … "004A08D0 flag 1 clear +184/+172/+196");
        Quests = QuestFile.Load(Install.QuestPath);
        StoreAddQuestNames(Quests);
    }
    if (Install…GlobalQuestPath)
    {
        var global = QuestFile.Load(Install.GlobalQuestPath);
        StoreAddQuestNames(global);          // append
        Quests = Quests.Append(global);
    }
}

private void StoreAddQuestNames(QuestFile file)
{
    foreach (var quest in file.Quests)
    {
        _worldPlus184.Add(quest.Name);
        if (quest.Persistent)
            _worldPlus172.Add(quest.Name);
        _questManagerPlus44.Add(quest.Name);
    }
}
```

| Native | Host | Class |
|---|---|---|
| `004A08D0` erase `+184` / `+172` | `.Clear()` those two lists | **MATCH** |
| `004A08D0` erase `+196` | Note string only; no `_worldPlus196` | **PARTIAL** (same gap as `qst-first-load`) |
| `004A08D0` skip `QM+44` | no `.Clear()` of `_questManagerPlus44` | **MATCH** |
| Flag 0 GlobalQuests append | second `StoreAddQuestNames` without a second world clear | **MATCH** |
| `004B2850` every `AddQuest` | `_questManagerPlus44.Add` | **MATCH** |

`Init_quests_004B4260_activates_wld_initial_list` locks
`WorldPlus184 == QuestManagerPlus44` after one
`EnterGame`. That equality is first-seen copies, not an
alias (`quest-manager-plus44`).

Comment on `QuestManagerPushFn`: “Not `004A08D0`-cleared.”
**MATCH** the listing.

---

## 3. Who *does* empty `QM+44` — `004B4590`, not `004A08D0`

Only `E8` of `004B4590`: `004A6697` inside `004A6550`
(world vtbl+28). `00416953` calls that vtbl **before**
`"Loading world"` / `004A1840` (`init-world-004A6E30`).

```
004A667C  push 0xB4
004A6681  call 00BFEA1A
004A6695  mov ecx, eax
004A6697  call 004B4590           // [+44/+48/+52]=0
004A66A1  mov ecx, 0x13B89FC
004A66A6  call 004A9A10           // SmartPtr assign
```

`004A9A10`: if `[esi+4]` live, `dec` ref; last ref calls
`[ref+4]` (`0x004A8220`) then `00BFE9BC`. Then
`[esi]=new`. A second `00416953` **replaces** the
singleton. Old `+44` dies with the old object.
**PROVEN** replace; not an append onto the previous
catalog.

Ctor zeros (`004B45BF`–`004B45C5`):

```
[esi+44]=0
[esi+48]=0
[esi+52]=0
```

Host `LoadWorld` first line:

```
_questManagerPlus44.Clear();
Note(QuestManagerPushFn, … "004B4590 [0x13B89FC]+44=0");
```

Then `LoadWorldMap` → `LoadQuestDefs`. **MATCH** the
ctor, **DISPROVEN** as a job of `004A08D0`.

`LoadWorldMap` has **one** caller: `LoadWorld`.
`LoadQuestDefs` has **one** caller: `LoadWorldMap`.
No production path parses QST twice without the
`004B4590` clear.

---

## 4. Double New Game leftover — **DISPROVEN**

### Native second no-save `00416953`

New `CWorld` (`004A67D0` zeros `+172`/`+184`/`+196`).
New QuestManager (`004B4590` + `004A9A10`). Flag-1
`004A08D0` is an empty-range no-op. `AddQuest` fills
three **empty** lists. **PROVEN** first-seen shape
repeats; no leftover `+44`.

Save `004A2A01` is **not** a second New Game QST
(`004A1840-second-site`). It still sits under
`00416953` after the same `004A6550` ctor, then
HEADER `QUESTS` overlay. No-save never takes it.

In-game `004A2F10` when `world+248 != 0` can re-enter
`004A1840` **without** `004A6550`. Then native
`004A08D0` would wipe world vectors and **append**
onto a live `QM+44`. That is native, not a host
`LoadQuestDefs` bug. No-save ctor leaves `+248==0`.
That take is **UNREAD** here and **DISPROVEN** as
New Game.

### Host second `EnterGame`

```
if (Stage is not (LeaveFrontend or Frontend))
    return;
```

After the first New Game `Stage=Game`. A second
`EnterGame` is a no-op (`_loadprobe` “2nd process-warm”
included). No second `LoadQuestDefs`. **PROVEN** skip.
Host never resets `Stage` to `Frontend` (only
`Shutdown`). A same-instance “click New Game again”
path is **UNREAD** / unimplemented, not a `+44`
append.

### Host second `LoadWorld` (tests / reuse)

`.Clear()` of `_questManagerPlus44` runs **before**
`LoadQuestDefs`. Counts match first-seen. **MATCH**
`004B4590` then `004A08D0`.

### Hypothetical leftover (not this call graph)

`LoadQuestDefs()` twice with no `LoadWorld`:
`+172`/`+184` reset, `+44` doubles. **LEFTOVER**
only if that call existed. It does not.

Clearing `+44` inside `LoadQuestDefs` would hide
that hypothetical and **DIVERGE** from `004A08D0`.
Do not “fix” the skip.

---

## 5. What this is not

| Claim | Class |
|---|---|
| Host forgot `QM+44` in the flag-1 clear | **DISPROVEN** (`004A08D0` also skips it) |
| `QM+44` *is* `world+184` | **DISPROVEN** (third list; `quest-manager-plus44`) |
| `004B4260` walks `QM+44` | **DISPROVEN** (walks `world+172`; `+44` is the gate) |
| First-seen TRUE names miss `004B00C0` because `+44` was not cleared | **DISPROVEN** (`qm44-gate-find`: `+172` ⊂ `+44`) |
| Second `004A1840` site is a second New Game parse | **DISPROVEN** (`004A1840-second-site`) |
| Host `+196` list is cleared here | **PARTIAL** (no host vector; Note only) |

---

## Classifications (short)

1. **Host `LoadQuestDefs` clears `+172`/`+184`, not `QM+44`.
   PROVEN.** Lists and `StoreAddQuestNames` `Add`.

2. **That skip MATCHES `004A08D0`. PROVEN.** World
   triples only. Manager `+44` is a different object.

3. **Not a double New Game leftover. PROVEN** on
   `LoadWorld` / `00416953`. `004B4590` + `004A9A10`
   empty / replace `+44` before QST. Host
   `LoadWorld.Clear()` is that ctor, not the flag-1
   erase.

4. **Moving the `+44` clear into `LoadQuestDefs` —
   DIVERGE.** Would invent an `004A08D0` store that
   the listing does not have.
