# `004B3CE0` factory 0: 52-byte stub vs host fiber

Investigation only. No production `src/` edits.

Question: `004B3CE0` factory 0 builds a 52-byte stub
(`004B4063`). Host `CreateFiber` / `Scheduler.Create` /
`EventPosts` leftover? First-seen
`ChapterAndSceneManager` / `NPCDeath`?

Do **not** invent a factory / `Main` / fiber for those
two QST names. They have **no** PE string and **no**
`00CD52D0` row.

Do **not** invent a `Started=false` skip of the stub
or of `[manager+56]`. That flag is not this branch.

Do **not** start at `S_QNOVI` / `00DBDE40` /
`Q_NewOakValeIntro`. Those names are not on the no-save
`world+172` walk.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Authority: Fable.exe `listing-00480000.txt` `004B3CE0` /
`004B3EE7` / `004B4063` / `004B3FEC` / `004B4042` /
`004B4260` / `004B4325` / `004B4386`;
`listing-00c80000.txt` `00CB7900`;
`proofs/factory0-construct`;
also `factory0-enqueue`, `004B3CE0-factory0`,
`fiber-first`, `00CB8220-first-pump`;
`EngineLifecycle.ActivateNamedQuest` /
`ScriptRuntime.ActivateQuest`;
`EngineLifecycleTests.Init_quests_004B4260_*` /
`Type1_00CB8220_*`.

`factory0-construct` closed host `Started=false` as
leftover **comment**. This note is the stub vs the
three leftover **mutations** (`CreateFiber` /
`Scheduler.Create` / `EventPosts++`).

---

## Verdict table

| Question | Answer | Class |
|---|---|---|
| Factory 0 object? | `004B4063` `00BFEA1A(52)` named slot. Id `[manager+132]++`. Name `+48`. `[+4]=[+8]=0`. Linked on `[manager+56]`. | **PROVEN** |
| Stub vs skip? | **Stub.** Skip is only `004AF610` already-in-`+56` (`jne 004B417A`). First-seen those names are not on `+56`. | **PROVEN** |
| Native fiber? | **No.** `00CB7900` is only `004B3FEC` on the factory arm. Stub has no run object, no `004B0310`, no `00A44740`. | **PROVEN** |
| Native `00687540`? | **No.** `004B4042` is after `00CB7900` on the factory arm. Stub never reaches it. | **PROVEN** |
| First-seen those two names? | **Yes.** No-save `world+172` rows 2 and 5 (`AddQuest` TRUE). Both pass `004B00C0`, miss `00CB5AD0`, enqueue factory **0**, then this stub. | **PROVEN** |
| `[0x1375454]==1` first-seen change? | **No.** Factory 0 `je 004B4063` at `004B3EE7` is **before** `004B3EED`. Byte unread. | **PROVEN** |
| Host `CreateFiber` leftover? | **Yes, mutation.** Always before `Find`. Native stub has no `00CB7900`. | **LEFTOVER** / **DIVERGE** |
| Host `Scheduler.Create` leftover? | **Yes, mutation.** Always. Native stub has no watcher. | **LEFTOVER** / **DIVERGE** |
| Host `EventPosts++` leftover? | **Yes, mutation.** Always after activate. Native first-seen posts **8**, not **10**. | **LEFTOVER** / **DIVERGE** |
| `Started=false` skip construct? | **No.** Do not invent. `false` **MATCH**es no `StartFactory` / `[+8]==0`. It is not a skip of `004B4063`. | **DISPROVEN** |

**52-byte stub. Not skip. Not a fiber.** Host fiber /
scheduler / event-post on those two names are leftover
mutations. First-seen New Game **does** construct the
two stubs.

---

## Timeline (no-save, first `004B4260`)

```
0049F24E  call 004B4260([world+172])     // nine QST TRUE
  loop name:
    004B00C0                             // both names pass
    00CB5AD0 [manager+120]
      hit  → 004BB720 [rec+4]=factory
      miss → 004B4325 [rec+4]=0 004BB720
  004B4386  call 004B3CE0                // once
    second loop 004B3E82:
      Q_SunnyvaleMaster         factory → 004B0310 + 00CB7900 + 00687540
      ChapterAndSceneManager    [+4]==0 → 004B4063 stub
      PersonalScriptMain        factory
      PersonalScript_GlobalThings factory
      NPCDeath                  [+4]==0 → 004B4063 stub
      HeroBoasts … Global_WatchForHeroDeath  factory
```

Later `user.ini` `Gameflow` is a second `004B4260` and
the 8th `00687540`. Not these two names.

---

## 1. Native factory 0 is the 52-byte stub

`listing-00480000.txt` second loop:

```
004B3E8C  call 004AF610          // already on +56?
004B3E93  jne 004B417A           // SKIP — no alloc
…
004B3EE4  cmp [edi+4], ebx       // ebx=0
004B3EE7  je 004B4063            // FACTORY 0 → stub
004B3EED  mov al, [0x1375454]    // unread when +4==0
004B3EF2  test al, al
004B3EF4  je 004B4063
```

`004B4063` (`factory0-construct` / listing):

```
id = [ebp+132]++
esi = 00BFEA1A(52)
[esi+0]  = id                  // not a vtbl
[esi+4]  = 0                   // no factory
[esi+8]  = 0                   // no run / no fiber this
[esi+12 … +24] = 0
[esi+36] = 1
[esi+37] = [edi+9]             // 1 on Init Quests
[esi+40] = 0
[esi+44] = 0
0099EC30(esi+48, edi)          // name only
wrapper 00BFEA1A(12) → 16-byte node on [ebp+56]
004B9D50 / 004B9D00 / 004B9C10
; no 004B0310
; no 00CB7900
; no 00687540
```

`00CB8690` on the stub arm needs `[obj+8]!=0`. Stub
stores 0. **PROVEN** no fiber helper.

Skip of the 52-byte alloc is **only** the already-active
name test. First-seen `+56` does not hold these names.
**PROVEN** stub, not skip (`factory0-construct`).

---

## 2. Not a fiber

Factory arm only (`004B3F4B` `push 52` → `004B0310`):

```
004B3FEC  call 00CB7900
…
004B403E  push 50
004B4040  push 55
004B4042  call 00687540
```

`00CB7900` (`listing-00c80000.txt`):

```
call [vtbl+12]
jmp  [vtbl+4]                  // Main → 00CDD450 → 00A44740
```

No site on `004B4063`. First *script* fiber on this
`004B4260` remains `Q_SunnyvaleMaster` `00CDD380` /
`00CDD450` (`fiber-first`). Factory 0 is not that.
**PROVEN**.

---

## 3. First-seen `ChapterAndSceneManager` / `NPCDeath`

| # | `world+172` | `00CB5AD0` | `004B3CE0` |
|--:|---|---|---|
| 1 | `Q_SunnyvaleMaster` | factory | live + fiber + post |
| 2 | `ChapterAndSceneManager` | **0** | **stub** |
| 3 | `PersonalScriptMain` | factory | live + fiber + post |
| 4 | `PersonalScript_GlobalThings` | factory | live + fiber + post |
| 5 | `NPCDeath` | **0** | **stub** |
| 6–9 | `HeroBoasts` … `Global_WatchForHeroDeath` | factory | live + fiber + post |

Writer is QST `AddQuest` TRUE (`004A0D90` → `+172`),
not WLD `START_INITIAL_QUESTS`. Both names are TRUE
in `FinalAlbion.qst`. Both are already in `QM+44`
(`004B2850` on every `AddQuest`) so `004B00C0` returns
1. Miss is lookup only. **PROVEN**.

No PE string. No `QuestFactoryTable` / `00CD52D0` row.
Do **not** invent a second registrar. Absence is
**PROVEN**. A later fill is **UNREAD**.

`Q_NewOakValeIntro` is FALSE + `AddTestQuest`. Not
this walk. **DISPROVEN** as first-seen factory 0.

---

## 4. Host leftover mutations

`ScriptRuntime.ActivateQuest` (every `ActivateNamedQuest`
name, including the two stubs):

```
CreateFiber(name, persist);           // ScriptRuntime._fibers
quest = new QuestInstance(…);
state = Scheduler.Create(name, persist);
quest.AttachFiber(state);
if (QuestFactoryTable.Find(name) is { } bind)
    quest.StartFactory(…);            // only factory hits
```

`ActivateNamedQuest` then always:

```
EventPosts++;
Note(00687540 kind=55 delay=50);
```

`Find` misses both names. `StartFactory` is not called.
`Started` stays false. That **MATCH**es “no `004B0310` /
no `00CB7900`.” It does **not** MATCH “no `004B3CE0`.”

The leftover work is the three always-on stores:

| Host | Native factory 0 | Class |
|---|---|---|
| `CreateFiber` | no `00CB7900` / `00A44740` | **LEFTOVER** / **DIVERGE** |
| `Scheduler.Create` | no watcher / `00CB7E50` | **LEFTOVER** / **DIVERGE** |
| `EventPosts++` | no `00687540` | **LEFTOVER** / **DIVERGE** |

Host lock: `Fibers.Count==10`, `EventPosts==10`
(9 `world+172` + Gameflow). Native first-seen:
**8** fibers / **8** posts (7 `+172` factories +
Gameflow). The extra two are these stubs.
**PROVEN** count split (`00CB8220-first-pump`).

`Started=false` does **not** gate those three paths.
Do **not** implement a skip from the bool. Do **not**
treat the bool as `[obj+36]` (stub stores **1**).
That skip is **DISPROVEN** (`factory0-construct`).

---

## What this is not

| Claim | Class |
|---|---|
| Factory 0 is skipped (no 52-byte alloc) | **DISPROVEN** |
| Factory 0 starts `00CB7900` / `00A44740` | **DISPROVEN** |
| Factory 0 posts `00687540` | **DISPROVEN** |
| Factory 0 unread because `[0x1375454]` | **DISPROVEN** (unread; stub anyway) |
| Those two names fail `004B00C0` | **DISPROVEN** |
| Those two names are not first-seen | **DISPROVEN** (rows 2 and 5) |
| Host `CreateFiber` / `Scheduler.Create` MATCH stub | **DISPROVEN** |
| Host `EventPosts==10` MATCH native | **DISPROVEN** (native 8) |
| `Started=false` skip stub / skip `+56` | **DISPROVEN** — do not invent |
| Second registrar fills the two names | **UNREAD** — do not invent |
| `Global_WatchForHeroDeath` is factory 0 | **DISPROVEN** (`00EE90A0`) |

---

## Classifications (short)

1. **`004B3CE0` factory 0 constructs a 52-byte named
   slot on `[manager+56]` — PROVEN.** `004B4063`.
   Not skip. Not a fiber.
2. **First-seen `ChapterAndSceneManager` / `NPCDeath`
   take that stub — PROVEN.** On `world+172`.
3. **Host `CreateFiber` / `Scheduler.Create` /
   `EventPosts++` on those names — LEFTOVER
   mutations / DIVERGE — PROVEN.**
4. **`Started=false` as skip of this construct —
   DISPROVEN.** Do not invent it.
