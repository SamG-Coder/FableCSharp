# Factory 0 vs first type-1 `00CB7C40` / `00CB7950`

Investigation only. No production `src/` edits.

Do **not** invent a `00CB7950` body for
`ChapterAndSceneManager` or `NPCDeath`. They have **no**
factory, **no** `[quest+8]` run object, **no** watcher list.

Do **not** start at `S_QNOVI` / `00DBDE40` /
`Q_NewOakValeIntro`. Those names are not on the no-save
`world+172` walk.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Sources: ExeIndex `listing-00480000.txt` (`004B3CE0` /
`004B4063` / `004B0310` / `004B4260` / `004B4490` /
`004A5D88`);
`listing-00c80000.txt` (`00CB8220` / `00CB7C40` /
`00CB8170` / `00CB7950` / `00CB7900` / `00CB7E50`);
`proofs/factory0-enqueue`, `proofs/fiber-yield-first`,
`proofs/gameflow-main-first-tick`;
`EngineLifecycle.PumpQuestList` /
`TickNamedQuestMain`;
`EngineLifecycleTests.Init_quests_004B4260_activates_wld_initial_list`,
`Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`.

`factory0-enqueue` closed construct: factory 0 is a 52-byte
named slot on `[manager+56]`, not a fiber. This note is
the first type-1 walk of that list.

---

## Verdict

**No. The first type-1 walk does not `00CB7950` those
names. Only constructed factory objects (non-zero
`[quest+8]`) enter `00CB8220` → `00CB7C40`.**

`004B4260` still enqueues `ChapterAndSceneManager` /
`NPCDeath` with factory **0**. `004B3CE0` `004B4063`
links a 52-byte stub at `[manager+56]` with
`[obj+4]=[obj+8]=0`. **PROVEN** (`factory0-enqueue`).

First type-1 is `004A5A40` → `004A5D82` `[0x13B89FC]`
`004B4490`. That walk **visits** every `[esi+56]` node,
including the two stubs, then **skips** `00CB8220` when
`[quest+8]==0`. **PROVEN**.

`00CB7C40` does **not** walk names. `this` is the run
object at `[quest+8]`. It walks `[this+4]` and
`00CB7950`s each `[node+8]` watcher. Factory 0 never
builds that object, never `00CB7900`s, never
`00CB7E50`s. The first `00CB7C40` is
`Q_SunnyvaleMaster`’s run list (head = `Main`
`00CDD360`). **PROVEN**.

Host `PumpQuestList` ticks every `_activatedQuests`
name except the string `"Gameflow"`. After no-save
Init Quests that is all nine `WorldPlus172` names,
including the two factory-0 stubs, then Gameflow
Main / Core / Barrow (`QuestPumpWalked==12`).
**DIVERGE**.

| Question | Answer | Class |
|---|---|---|
| Are the two names on `[manager+56]` before first type-1? | Yes. Stub slots from `004B3CE0` | **PROVEN** |
| Does `004B4490` visit those nodes? | Yes. Tail-insert order, then `cmp [eax+8], 0` | **PROVEN** |
| Does it call `00CB8220` / `00CB7C40` on them? | No. `[quest+8]==0` → `je 004B4549` | **PROVEN** |
| Does first `00CB7C40` walk names? | No. Watcher list on one run object | **PROVEN** |
| First `00CB7950` target? | Sunnyvale `Main` (`00CDD360`), not either stub | **PROVEN** |
| Can `00CB7950` run on the 52-byte stub? | No site. `this` is the run object; arg is a watcher | **DISPROVEN** |
| Host ticks those two names? | Yes. `TickNamedQuestMain` else-arm notes `00CB7950` | **DIVERGE** |

---

## Timeline (no-save, first type-1)

```
004B4260([world+172])                  // already done
  004B3CE0
    factory != 0 → 52-byte live, [+8]=run, 00CB7900
    factory  0  → 004B4063 stub, [+8]=0, no 00CB7900
user.ini ActivateQuest("Gameflow")     // factory; [+8]=run
004189C2 first pumps
  004A5A40 type-1
    004A5D82 mov ecx, [0x13B89FC]
    004A5D88 call 004B4490
      walk [esi+56]:
        Q_SunnyvaleMaster              [+8]=run → 00CB8220
          00CB7C40                     // FIRST 00CB7C40
            walk [run+4]; 00CB7950([node+8])
            head = Main 00CDD360       // FIRST 00CB7950
          jmp 00CB8170
        ChapterAndSceneManager         [+8]=0  SKIP
        PersonalScriptMain             [+8]=run → 00CB8220
        PersonalScript_GlobalThings    [+8]=run → 00CB8220
        NPCDeath                       [+8]=0  SKIP
        HeroBoasts                     [+8]=run → 00CB8220
        V_HeroDolls                    [+8]=run → 00CB8220
        CS_PlayCutscene                [+8]=run → 00CB8220
        Global_WatchForHeroDeath       [+8]=run → 00CB8220
        Gameflow                       [+8]=run → 00CB8220
          00CB7C40 Main then Core / Barrow (insert-at-tail)
```

`00CB8220` count on this pump: **8** (seven `world+172`
factories + Gameflow). Factory-0 `00CB7950` count: **0**.

---

## 1. `004B3CE0` factory 0 leaves `[quest+8]=0`

Live ctor `004B0310` (`listing-00480000.txt`):

```
[esi+0]  = id
[esi+4]  = factory record          // [esp+12]
[esi+8]  = run object              // factory [+0] result
[esi+12] = wrapper of +8
[esi+36] = 1
[esi+37] = [rec+9]
[esi+48] = name
```

Then `00CB7900` on the **run** object (`vtbl+12` then
`jmp [vtbl+4]`), which `00CB7E50`s `Main` onto
`[run+4]`. **PROVEN**.

Stub `004B4063` (`xor ebx, ebx` at `004B3EA9` still
holds on the `004B97D0` miss):

```
obj = 00BFEA1A(52)
[obj+0]  = id
[obj+4]  = 0
[obj+8]  = 0                       // no run object
[obj+12 … +24] = 0
[obj+36] = 1
[obj+37] = [rec+9]                 // 1 on Init Quests
[obj+40] = 0
[obj+44] = 0
[obj+48] = name
16-byte node → [manager+56]        // same list as live
; no 00CB7900
; no 00CB7E50
; no 00687540
```

`[obj+0]` is an **id**, not a vtbl. It is not a
`00CB7950` `this` or arg. **PROVEN**.

---

## 2. `004B4490` gates `00CB8220` on `[quest+8]`

`listing-00480000.txt` `004B4517`–`004B454E`:

```
mov eax, [esi+56]
mov edi, [eax]
xor ebx, ebx
cmp edi, eax
je 004B4550                        // empty → no 00CB8220
004B4522:
  mov eax, [edi+8]                 // 52-byte quest slot
  cmp [eax+8], ebx
  je 004B4549                      // [quest+8]==0 → next node
  mov ecx, [eax+8]                 // this = run object
  call 00CB8220
004B4549:
  mov edi, [edi]
  cmp edi, [esi+56]
  jne 004B4522
```

The skip is **not** a name compare. It is a null run
pointer. Factory 0 always takes it. A constructed
factory with a live run never does. **PROVEN**.

`00CB8220` (`listing-00c80000.txt`):

```
00CB8220  push esi
00CB8221  mov esi, ecx             // run object
00CB8223  call 00CB7C40
00CB8228  mov ecx, esi
00CB822B  jmp 00CB8170             // [run+8] timed list
```

One `.text` `E8` of `00CB7C40`: `00CB8223`. Not
`004B4260` / `004B3CE0` / `00CB7900`. **PROVEN**.

---

## 3. `00CB7C40` / `00CB7950` tick watchers, not `[esi+56]` names

`00CB7C40`:

```
00CB7C40  push ebx
00CB7C41  mov ebx, ecx             // run object
00CB7C43  mov eax, [ebx+4]
00CB7C47  mov esi, [eax]
00CB7C49  cmp esi, eax
00CB7C4B  je 00CB7CA8              // empty → ret
00CB7C51  mov eax, [esi+8]         // watcher / fiber
00CB7C54  push eax
00CB7C55  mov ecx, ebx
00CB7C57  call 00CB7950
          al==0 → keep; al!=0 → unlink
```

`00CB7950`:

```
esi = arg                          // watcher
edi = ecx                          // run object
[edi+44] = esi
[esi+40]!=0 → return 0 (keep)
00F35A00
[+41]==0 → call [vtbl+4]           // first-seen 00A44880
           or Sunnyvale-class 00CDD360
[+41]!=0 → vtbl+24; clear +41
```

`this` / arg are the **run + watcher** pair from
`00CDD450` / `00CB7E50`. First-seen `+41=0`.
Factory-0 stubs are not on `[run+4]`. **PROVEN**.

`00CB8170` is the sibling walk of `[run+8]`. First-seen
Gameflow `[+8]=0` is empty (`gameflow-main-first-tick`).
Factory 0 never reaches it. **PROVEN** skip;
**DISPROVEN** as a second path onto those names.

---

## 4. What the first `00CB7C40` actually ticks

`[esi+56]` is tail-insert. First **non-skip** node after
Init Quests + `user.ini` Gameflow:

| `[esi+56]` # | Name | `[quest+8]` | This pump |
|--:|---|---|---|
| 1 | `Q_SunnyvaleMaster` | run | `00CB8220` / first `00CB7C40` |
| 2 | `ChapterAndSceneManager` | **0** | **skip** |
| 3 | `PersonalScriptMain` | run | later `00CB8220` |
| 4 | `PersonalScript_GlobalThings` | run | later |
| 5 | `NPCDeath` | **0** | **skip** |
| 6 | `HeroBoasts` | run | later |
| 7 | `V_HeroDolls` | run | later |
| 8 | `CS_PlayCutscene` | run | later |
| 9 | `Global_WatchForHeroDeath` | run | later (`00EE90A0`, not factory 0) |
| 10 | `Gameflow` | run | later; Main then Core / Barrow |

First `00CB7950` is row 1 `Main` (`00CDD360`
`vtbl+28`). See `fiber-yield-first`. Rows 2 and 5 are
**not** in any `00CB7C40` list. **PROVEN**.

`Global_WatchForHeroDeath` is a constructed factory.
Omitting it on the host is a different **DIVERGE**
(`factory0-enqueue`). Do not fold it into factory 0.

---

## 5. Host `PumpQuestList`

`C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`

```
foreach (var name in _activatedQuests)
{
    if (name == "Gameflow")
        continue;
    TickNamedQuestMain(name);
}
// then Gameflow Main / Core / Barrow
```

`_activatedQuests` after `EnterGame` is the nine
`WorldPlus172` names plus `"Gameflow"`
(`Init_quests_004B4260_*`). The skip is **only** the
Gameflow **string**, so `TickNamedQuestMain` still
runs for `ChapterAndSceneManager` and `NPCDeath`.
Those hit the else-arm:

```
00CB7950 ChapterAndSceneManager Main
009D8650 ChapterAndSceneManager
```

Native never issues those two `00CB7950`s.
`Type1_00CB8220_*` asserts `QuestPumpWalked==12`
(9 names + 3 Gameflow watchers). Native
`00CB8220` count is 8; factory-0 name ticks are 0.
**DIVERGE**.

Host `ScriptRuntime.ActivateQuest` also
`CreateFiber` / `Scheduler.Create` for factory 0.
Native does not (`factory0-enqueue`). That is the
construct DIVERGE; this note is the **tick** one.

---

## What this is not

| Claim | Class |
|---|---|
| Factory 0 is dropped from `[esi+56]` before type-1 | **DISPROVEN** (linked at construct) |
| First `00CB7C40` walks `[esi+56]` names | **DISPROVEN** (`[run+4]` watchers) |
| First `00CB7950` is `ChapterAndSceneManager` | **DISPROVEN** (Sunnyvale `Main`) |
| `00CB7950` on the 52-byte stub (`[+0]=id`) | **DISPROVEN** (no site) |
| `00CB8170` ticks the stubs | **DISPROVEN** (same `00CB8220` skip) |
| `NPCDeath` / `ChapterAndSceneManager` have a factory `Main` | **DISPROVEN** (no `00CB7900`) |
| Host skip-Gameflow matches the native `[+8]==0` gate | **DIVERGE** (host still ticks the two stubs) |
| `Global_WatchForHeroDeath` is factory 0 | **DISPROVEN** |

---

## Classifications (short)

1. **`004B4490` visits factory-0 slots, then skips
   `00CB8220` because `[quest+8]==0` — PROVEN.**
2. **First `00CB7C40` / `00CB7950` tick constructed
   factory watchers only — PROVEN.** First pair is
   Sunnyvale `Main`. The two QST-only names are not
   in that walk.
3. **Host ticks all `WorldPlus172` names except the
   Gameflow string — DIVERGE.** Native gate is the
   run pointer, not the name.
