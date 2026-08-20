# `004B3CE0` factory 0 construct: 52-byte stub vs host `Started=false`

Investigation only. No production `src/` edits.

Question: after first `004B4260` enqueues
`ChapterAndSceneManager` / `NPCDeath` with factory **0**,
what does `004B3CE0` construct? 52-byte stub? Host
`QuestInstance.Started=false` leftover?

Do **not** invent a factory / `Main` / fiber for those
two QST names. They have **no** PE string and **no**
`00CD52D0` row.

Do **not** start at `S_QNOVI` / `00DBDE40` /
`Q_NewOakValeIntro`. Those names are not on the no-save
`world+172` walk.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Authority: `proofs/004B3CE0-factory0`,
`proofs/factory0-enqueue`;
also `proofs/factory0-type1-tick`,
`proofs/host-gate-va-leftover`,
`proofs/qm44-gate-find`;
ExeIndex `listing-00480000.txt` `004B3CE0` /
`004B3EED` / `004B4063` / `004B0310` / `004B4260` /
`004B4386` / `004AF610`;
`EngineLifecycle.ActivateNamedQuest` /
`ScriptRuntime.ActivateQuest` /
`QuestInstance.StartFactory`;
`EngineLifecycleTests.Init_quests_004B4260_activates_wld_initial_list`.

`factory0-enqueue` closed the **enqueue** (`00CB5AD0`
miss still `004BB720`s factory 0). `004B3CE0-factory0`
closed the **native construct** arm. This note is the
host `Started=false` leftover next to that stub.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| 52-byte stub? | **Yes.** `004B4063` `00BFEA1A(52)`, name at `+48`, id from `[manager+132]++`, `[+4]=[+8]=0`, linked on `[manager+56]`. | **PROVEN** |
| Stub vs skip? | **Stub.** Skip is only `004AF610` already-in-`+56`. First-seen those names are not on `+56`. | **PROVEN** |
| Fiber / `00CB7900` / `00687540`? | **No.** Those are factory-arm only. | **PROVEN** |
| Does `[0x1375454]==1` change it? | **No.** Factory 0 `je 004B4063` at `004B3EE7` is **before** `004B3EED`. | **PROVEN** |
| Host `Started=false` leftover **comment**? | **Yes.** `StartFactory` XML / tests treat `Started` as “`004B3CE0` construct.” Native still constructs the stub. `Started` is not `[obj+36]` (stub stores **1**). | **PROVEN** leftover comment |
| Host `Started=false` leftover **side effect**? | **No.** The bool is host-only; `false` means `Find` missed so `StartFactory` never ran. Native analog is `[+4]=[+8]=0`, not skip. | **PROVEN** skip; **MATCH** factory-arm miss |
| Host leftover **mutations** on those names? | **Yes, but they are DIVERGE, not a `Started` store.** `CreateFiber` / `Scheduler.Create`, always `EventPosts++` / `00687540` Note, type-1 else-arm `00CB7950`. | **DIVERGE** |

---

## Verdict

**52-byte stub. Not skip. Not a fiber.**

`ChapterAndSceneManager` and `NPCDeath` pass `004B00C0`,
miss `00CB5AD0`, enqueue factory **0**, then `004B3CE0`
builds named slots on `[manager+56]`. First-seen
`[0x1375454]` is **1** and **unread** for factory 0.

**`Started=false` is leftover comment / naming, not a
leftover skip of the stub.**

Host `QuestInstance.Started` is set only by
`StartFactory` (factory hit) or `StartChildCutscene`.
Both QST-only names miss `QuestFactoryTable.Find`, so
the flag stays false. That **MATCH**es “no `004B0310` /
no `00CB7900`.” It does **not** MATCH “`004B3CE0`
skipped.” Native still allocates.

Do **not** implement a skip from `Started=false`.
Do **not** invent a factory so the flag becomes true.
Do **not** treat the flag as `[obj+36]`.

---

## Timeline (no-save, first `004B4260`)

```
0049F21B  "Init Quests"
0049F24E  call 004B4260([world+172])
  loop name:
    004B00C0                    // +44; both names pass
    00CB5AD0 [manager+120]
      hit  → 004BB720 factory
      miss → 004BB720 [rec+4]=0
  004B4386  call 004B3CE0       // once, after the loop
    second loop 004B3E82:
      Q_SunnyvaleMaster         factory → 004B0310 + 00CB7900
      ChapterAndSceneManager    [+4]==0 → 004B4063 stub
      PersonalScriptMain        factory
      PersonalScript_GlobalThings factory
      NPCDeath                  [+4]==0 → 004B4063 stub
      HeroBoasts … Global_WatchForHeroDeath  factory
```

Host `InitCharactersAndQuests` now walks `_worldPlus172`
(nine TRUE names) then later `Gameflow`.
`Init_quests_004B4260_*` locks `ActivatedQuests.Take(9)
== WorldPlus172` and `Started==false` on rows 2 and 5.
**PROVEN** names on the walk.

`factory0-enqueue` “host never `00CB5AD0`s the two
QST-only names” is **STALE**. The leftover is no longer
omission; it is the stub vs `Started` / fiber split.

---

## 1. Native construct is the 52-byte stub

`004B3CE0` second loop (`listing-00480000.txt`):

```
004B3E8C  call 004AF610          // already on +56?
004B3E93  jne 004B417A           // SKIP — no alloc
…
004B3EE4  cmp [edi+4], ebx       // ebx=0
004B3EE7  je 004B4063            // FACTORY 0 → stub
004B3EED  mov al, [0x1375454]
004B3EF2  test al, al
004B3EF4  je 004B4063            // only if factory != 0
```

Stub at `004B4063` (`004B3CE0-factory0` / `factory0-enqueue`):

```
id = [ebp+132]++
esi = 00BFEA1A(52)
[esi+0]  = id                  // not a vtbl
[esi+4]  = 0                   // no factory record
[esi+8]  = 0                   // no run object
[esi+12 … +24] = 0
[esi+36] = 1                   // same as live 004B0310
[esi+37] = [edi+9]             // 1 on Init Quests
[esi+40] = 0
[esi+44] = 0
0099EC30(esi+48, edi)          // name CString only
wrapper 00BFEA1A(12): {1, 004BAEF0, esi}
16-byte node → [ebp+56]        // same list as live
004B9D50 / 004B9D00 / 004B9C10
; no 004B0310
; no 00CB7900
; no 00687540
```

Live `004B0310` is the same 52-byte size with
`[+4]=factory` / `[+8]=run` then `00CB7900` /
`00687540(55,50)`. Stub fill is the difference, not
size.

Later type-1 `004B4490` visits the node and
`je 004B4549` when `[quest+8]==0`
(`factory0-type1-tick`). That is the **next** walk,
not this construct.

---

## 2. Host `Started=false` site

`QuestInstance.Started` (`ScriptScheduler.cs`): default
false. Writers: `StartFactory` and `StartChildCutscene`
only.

`ScriptRuntime.ActivateQuest`:

```
CreateFiber(name, persist);
quest = new QuestInstance(++_questId, name, persist);
state = Scheduler.Create(name, persist);
quest.AttachFiber(state);
_quests.Add(quest);

factory = QuestFactoryTable.Find(name);
if (factory is { } bind)
    quest.StartFactory(bind.Factory, bind.Run, bind.Init, bind.ScriptName);
```

`QuestFactoryTable` has **no** row for
`ChapterAndSceneManager` / `NPCDeath`. `Find` misses.
`StartFactory` is not called. `Started` stays false.
`Init_quests_004B4260_*` asserts that.

`StartFactory` XML still says
“`004B3CE0` factory construct + run.vtbl+8.”
Native `004B3CE0` also runs the stub arm. Naming the
flag after the whole walk is leftover.

`ActivateNamedQuest` emits `Note(QuestFactoryStartFn,
… "004B3CE0 construct")` **only** on a `Find` hit.
That split **MATCH**es factory vs stub for the Note.
The `Started` bool is the same split. The leftover is
reading `false` as “no `004B3CE0` / no `[manager+56]`
slot.”

`[+36]=1` on the stub. Mapping `Started` to that byte
would require **true**. **DISPROVEN**.

`HasStarted("S_HB")` / `"S_PSM"` / `"S_GF"` is a
different leftover (`CCutsceneDef` interpreter). Those
names are not factory 0. Do not collapse.

---

## 3. What is leftover (comment / flag)

| Host text | Native owner | Class |
|---|---|---|
| `StartFactory` XML “`004B3CE0` factory construct” | factory arm inside `004B3CE0`, not the stub | **LEFTOVER** naming |
| `Assert.False(…Started)` as “not constructed” | `004B4063` still `00BFEA1A(52)` + `[manager+56]` | **LEFTOVER** comment |
| `Started=false` as `[0x1375454]` skip | factory 0 unread (`004B3EE7` first) | **DISPROVEN** / leftover gate |
| `Note(01375454)` on those names | byte unread | **LEFTOVER** (`host-gate-va-leftover`) |
| `PumpQuests` “`004B3CE0` construct already at `004B4260`” | `004B4490` does not construct | **LEFTOVER** (site wrong) |

---

## 4. What is **not** leftover (`Started` itself)

| Host action | Native owner | Class |
|---|---|---|
| `Find` miss → no `StartFactory` | `[rec+4]==0` → no `004B0310` / `00CB7900` | **MATCH** |
| `Started==false` after activate | `[+4]=[+8]=0` | **MATCH** as no-run, **not** as skip |
| Quest object still exists | 52-byte named slot | **MATCH** existence |
| `004B00C0` pass for both names | `QM+44` from `AddQuest` | **MATCH** |
| `00CB5AD0` / `004BB720` Notes | miss still enqueues | **MATCH** when |

Dropping the two names from `_worldPlus172` would be
**DIVERGE** vs native (they are TRUE). Setting
`Started=true` without a factory would be leftover
theater vs `[+8]==0`.

---

## 5. Leftover **mutations** (not the bool)

These are real host work native factory 0 does not do.
They are **DIVERGE**, not a `Started` store.

| Host action | Native factory 0 | Class |
|---|---|---|
| `CreateFiber` + `Scheduler.Create` | no `00CB7900` / `00A44740` | **DIVERGE** (`Fibers.Count==10`) |
| `EventPosts++` / `00687540` Note | no `00687540` on stub | **DIVERGE** (`EventPosts==10` vs 8 first-seen posts: 7 `world+172` factories + Gameflow) |
| Type-1 `TickNamedQuestMain` else-arm `00CB7950` | `[quest+8]==0` skips `00CB8220` | **DIVERGE** (`factory0-type1-tick`) |

`Started=false` does **not** gate those paths. The flag
is unused as a pump skip. That is why it is leftover
**comment**, while the fiber / event / tick remain
separate **DIVERGE**s.

---

## What this is not

| Claim | Class |
|---|---|
| Factory 0 is skipped (no 52-byte alloc) | **DISPROVEN** |
| Factory 0 is skipped because `[0x1375454]` | **DISPROVEN** (unread) |
| Byte 1 makes factory 0 take `004B0310` | **DISPROVEN** |
| Factory 0 starts `00CB7900` / `00A44740` | **DISPROVEN** |
| Those two names fail `004B00C0` | **DISPROVEN** |
| Host `Started=false` means no `[manager+56]` slot | **DISPROVEN** / leftover |
| Host `Started` is `[obj+36]` | **DISPROVEN** (stub stores 1) |
| `Started=false` leftover **mutation** | **DISPROVEN** (bool unused as skip) |
| `Global_WatchForHeroDeath` is factory 0 | **DISPROVEN** (`00EE90A0`) |
| Second registrar fills those two names | **UNREAD** — do not invent |

---

## Classifications (short)

1. **`004B3CE0` factory 0 constructs a 52-byte named
   slot on `[manager+56]` — PROVEN.** `004B4063`.
   Not skip. Authority `004B3CE0-factory0` /
   `factory0-enqueue`.
2. **Not a fiber — PROVEN.** No `00CB7900`. Host fiber
   on those names is **DIVERGE**.
3. **Host `Started=false` leftover comment — PROVEN.**
   Flag names the factory arm, not the stub alloc.
4. **No leftover `Started` side effect — PROVEN.**
   `false` **MATCH**es no `StartFactory`; it is not a
   skip predicate.
5. **Treat `Started=false` as skip construct /
   skip `[manager+56]` — DISPROVEN.**
