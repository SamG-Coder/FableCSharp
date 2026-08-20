# `004B3CE0` after `004B4260`: factory-0 construct

Investigation only. No production `src/` edits.

Question: after first `004B4260` enqueues
`ChapterAndSceneManager` / `NPCDeath` with factory **0**,
what does `004B3CE0` construct? 52-byte stub vs skip?
`[0x1375454]==1` first-seen. Fiber?

Do **not** invent a factory / `Main` / fiber for those
two QST names. They have **no** PE string and **no**
`00CD52D0` row.

Do **not** start at `S_QNOVI` / `00DBDE40` /
`Q_NewOakValeIntro`. Those names are not on the no-save
`world+172` walk.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Authority: ExeIndex `listing-00480000.txt` `004B3CE0` /
`004B3EED` / `004B4063` / `004B0310` / `004B4260` /
`004B4386` / `004AF610` / `0049F24E`;
`proofs/factory0-enqueue`, `proofs/quest-activate-gate`;
also `factory0-type1-tick`, `fiber-first`,
`qm44-gate-find`;
`EngineLifecycleTests.Init_quests_004B4260_activates_wld_initial_list`.

`factory0-enqueue` closed the **enqueue** (miss still
`004BB720`s factory 0). This note is the **construct**
arm of the same walk.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| What object for factory 0? | `00BFEA1A(52)` named **slot** at `004B4063`. Id from `[manager+132]++`. Name at `+48`. `[+4]=[+8]=0`. Linked on `[manager+56]`. | **PROVEN** |
| Stub vs skip? | **Stub.** Skip is only `004AF610` already-in-`+56` (`jne 004B417A`). First-seen those names are not on `+56`. | **PROVEN** |
| Does `[0x1375454]==1` change it? | **No.** Factory 0 `je 004B4063` at `004B3EE7` is **before** `004B3EED` `mov al,[0x1375454]`. Byte 1 is first-seen and only gates a **non-zero** factory. | **PROVEN** |
| Fiber? | **No.** `00CB7900` is only on the factory arm (`004B3FEC`). Stub has no run object, no `004B0310`, no `00687540`. | **PROVEN** |

---

## Verdict

**52-byte stub. Not skip. Not a fiber.**

`004B4260` walks `world+172`, `004B00C0`s, `00CB5AD0`s,
`004BB720`s, **then once** `004B4386` `call 004B3CE0`.
`ChapterAndSceneManager` and `NPCDeath` are already in
that queue with `[rec+4]==0`.

`004B3CE0` second loop:

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

First-seen `[0x1375454]` is **1** (`.data` dword
`0x01010101`; one `.text` imm = this cmp; no `.text`
writer). BSS-0 “never construct” is **DISPROVEN** for
real factories. It is **irrelevant** for factory 0:
those two names never reach the load.

First *script* fiber on this `004B4260` remains
`Q_SunnyvaleMaster` `00CB7900` → `00CDD380` /
`00CDD450` (`fiber-first`). Factory 0 is not that.

---

## Timeline (no-save, first `004B4260`)

```
0049F21B  "Init Quests"
0049F23D  ecx = [0x13B89FC]
0049F243  push 1                 // arg2 → rec+9
0049F245  push 0                 // arg1 → rec+8
0049F247  lea edx, [esi+172]
0049F24E  call 004B4260
  loop name:
    "QuestManager: Activate Quest"
    004B00C0                    // +44; both names pass
    00CB5AD0 [manager+120]
      hit  → 004BB720 factory
      miss → 004BB720 [rec+4]=0
  004B4386  call 004B3CE0       // once, after the loop
    first loop 004B3D20         // +156 / 0x13CAA68; no 52
    second loop 004B3E82:
      Q_SunnyvaleMaster         factory → 004B0310 + 00CB7900
      ChapterAndSceneManager    [+4]==0 → 004B4063 stub
      PersonalScriptMain        factory
      PersonalScript_GlobalThings factory
      NPCDeath                  [+4]==0 → 004B4063 stub
      HeroBoasts … Global_WatchForHeroDeath  factory
```

`004B00C0` is not this decision (`quest-activate-gate`).
Both QST-only names are `AddQuest` TRUE → `QM+44` →
`al=1` → enqueue. **PROVEN**.

---

## 1. `004B3CE0` is after the enqueue loop

`listing-00480000.txt` `004B4260` tail:

```
004B42E4  00CB5AD0
004B42F1  je 004B4325            // miss
          [rec+4] = edi          // factory
          004BB720
          jmp consume
004B4325:
          [rec+4] = 0
          004BB720
…
004B437F  lea edx, [esp+24]      // queue vector
004B4386  call 004B3CE0
          destroy queue stride 12
          ret 12
```

`004B3CE0` `ret 4`. Arg is that queue. Count is
`(end-begin)*0x2AAAAAAB` = `/12`. Both loops
`add …, 12`. **PROVEN**.

---

## 2. Skip is `004AF610`, not factory 0

`004AF610` walks `[this+56]` by name at `[node+8]+48`.
Miss: `al=0`. Hit: `al=1`.

Second loop (`004B3E82`):

```
push edi
mov ecx, ebp                   // QuestManager
call 004AF610
test al, al
jne 004B417A                   // next rec; no 00BFEA1A(52)
```

First-seen `+56` does not yet hold
`ChapterAndSceneManager` / `NPCDeath`. `al=0`.
Construct continues. **PROVEN**.

First loop (`004B3D20`) can also `004AF610`-skip the
`+156` / `0x13CAA68` work. That loop never allocates
the 52-byte object. `009F0570==0` only ends **that**
iteration. Second loop still runs. **PROVEN**.

`004B97D0` on `[manager+12]` between the already-active
test and the factory cmp does not skip construct.
`00CB8690` on the stub arm needs `[obj+8]!=0`; stub
stores 0 there. **PROVEN** no fiber helper.

---

## 3. Factory 0 never reads `[0x1375454]`

`004B3EA9` `xor ebx, ebx` then:

```
004B3EE4  cmp [edi+4], ebx
004B3EE7  je 004B4063
004B3EED  mov al, [0x1375454]    // only .text imm
004B3EF2  test al, al
004B3EF4  je 004B4063
```

| `[rec+4]` | `[0x1375454]` | Arm |
|---|---|---|
| 0 | unread | `004B4063` stub |
| ≠0 | 0 | `004B4063` stub (not first-seen) |
| ≠0 | 1 (first-seen) | factory + `004B0310` + `00CB7900` |

VA `0x1375454` → RVA `0xF75454` in `.data`
(`rva=0xF74000`). First-seen byte **1**.
`quest-activate-gate`. Factory 0 is the first row.

---

## 4. Stub object at `004B4063`

```
id = [ebp+132]++
esi = 00BFEA1A(52)
[esi+0]  = id
[esi+4]  = 0                   // no factory record
[esi+8]  = 0                   // no run object
[esi+12 … +24] = 0
[esi+36] = 1
[esi+37] = [edi+9]             // 1 on Init Quests
[esi+40] = 0
[esi+44] = 0
0099EC30(esi+48, edi)          // name CString only
wrapper 00BFEA1A(12): {1, 004BAEF0, esi}
16-byte node → [ebp+56]        // same list as live
004B9D50 / 004B9D00 / 004B9C10 // erase +156 range
; no 004B0310
; no 00CB7900
; no 00687540
dec wrapper; list keeps ref
```

`[+0]` is an **id**, not a vtbl. Same 52-byte size
as the live ctor, different fill.

Live `004B0310` (`004B3F4B` `push 52` then this call):

```
[+0]=id  [+4]=factory  [+8]=run
[+12]=wrapper of +8
[+36]=1  [+37]=[rec+9]
[+48]=name
then 004BB270 +156
     00CB7900(run)             // 004B3FEC
     00687540(55, 50)
```

`[+8]` is how later `004B4490` decides `00CB8220`
(`factory0-type1-tick`). Stub stays 0 → type-1
**visits** the node and **skips** the pump. That is
the next walk, not this construct.

---

## 5. Not a fiber

`00CB7900` (factory arm only):

```
call [vtbl+12]
jmp  [vtbl+4]                  // Main → 00CDD450 → 00A44740
```

No site on `004B4063`. No `00A44740` / `00A447D0` /
`00CB7E50`. First quest watcher fiber is still
Sunnyvale `Main`, queued **before** these two names
and constructed on the factory arm. **PROVEN**.

Host `ScriptRuntime.ActivateQuest` still
`CreateFiber` / `Scheduler.Create` for every
`world+172` name, including the two stubs
(`Init_quests_004B4260_*` `Fibers.Count==10`,
`Assert.False(…Started)` only). **DIVERGE**.

---

## What this is not

| Claim | Class |
|---|---|
| Factory 0 is skipped (no 52-byte alloc) | **DISPROVEN** |
| Factory 0 is skipped because `[0x1375454]` | **DISPROVEN** (unread) |
| `[0x1375454]==0` first-seen | **DISPROVEN** (`.data` 1) |
| Byte 1 makes factory 0 take `004B0310` | **DISPROVEN** |
| Factory 0 starts `00CB7900` / `00A44740` | **DISPROVEN** |
| Those two names fail `004B00C0` so never reach `004B3CE0` | **DISPROVEN** |
| `004AF610` already-active on first-seen | **DISPROVEN** |
| Stub `[+0]` is a vtbl / fiber `this` | **DISPROVEN** |
| `Global_WatchForHeroDeath` is factory 0 | **DISPROVEN** (`00EE90A0`) |
| Second registrar fills those two names | **UNREAD** — do not invent |

---

## Classifications (short)

1. **`004B3CE0` factory 0 constructs a 52-byte named
   slot on `[manager+56]` — PROVEN.** `004B4063`.
   Not skip.
2. **`[0x1375454]==1` first-seen does not apply —
   PROVEN.** Factory 0 branches before `004B3EED`.
3. **Not a fiber — PROVEN.** No `00CB7900`. Host
   fiber on those names is **DIVERGE**.
