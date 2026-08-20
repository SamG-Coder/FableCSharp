# First-seen `004B2890` vs host: still leftover?

Investigation only. No production `src/` edits.

Question: first-seen `004B2890` vs host. Still leftover?
What is the next proven leftover slice?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Authority: `proofs/004B2890-first` (body / `QM+112`
sentinel / `+56` skip); `proofs/004B2890-empty-first`
(first site `0049F259`, no write); `proofs/host-004B2890-leftover`
(Note-only, no mutation); `proofs/0049F180-first-children`
(tail after `0049F259`); `proofs/factory0-stub-vs-fiber`
(factory-0 leftover is **`004B3CE0`**, not this VA);
`EngineLifecycle.InitCharactersAndQuests` /
`ActivateNamedQuest`; `QuestFactoryTable`;
ExeIndex `listing-00480000.txt` `004B2890` / `0049F24E` /
`0049F259`–`0049F2CB`; `listing-00400000.txt` `00416BCA` /
`00416BCF` / `00416C11` / `00416C2C`;
`EngineLifecycleTests.Init_quests_004B4260_activates_wld_initial_list`.

Do **not** start at `S_QNOVI` / `00DBDE40` /
`Q_NewOakValeIntro`. Those names are not this call.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| First-seen native? | **`0049F259` `call 004B2890`.** Same `ecx=[0x13B89FC]` immediately after `0049F24E` `004B4260([world+172])`. | **PROVEN** |
| First-seen body? | `je 004B2989` empty `QM+112`, then hero `00449970` / `00487DC0` miss `je 004B2AC1`. No write. | **PROVEN** |
| Host at this VA? | `Note(QuestManagerActivate, …, "004B2890")` then continue. `Note` is `Trace.Add` only. | **PROVEN** |
| Still leftover **side effect**? | **No.** Host does not walk `+112` / `+56`, does not `EventPosts++`, does not `ActivateQuest` here. | **MATCH** skip; **DISPROVEN** leftover mutation |
| Still leftover **comment**? | **Yes, one.** `EventManagerPumpFn` XML still says first-seen walks empty `[quest+112]` and omits the hero miss / unread filled `+56`. | **LEFTOVER** comment |
| Next proven `004B2890` body? | **None.** Implementing the skipped arms here is leftover theater. Load-game first take is **UNREAD**. | **PROVEN** none |
| Next proven leftover **slice**? | **`0049F180` tail after `0049F259`:** `009F1760` / `00449700` / `0041649C` / `[world+140]=[0x13B89BC]`. Host omits the stores. | **PROVEN** leftover gap |

---

## Verdict

**Not leftover work. Next leftover slice is the `0049F180` tail.**

`host-004B2890-leftover` still holds. First-seen native
is a stack-only no-op. Host first-seen is the listing
pair `004B4260` then a trace line. That is a **MATCH**
skip of a **PROVEN** empty body.

Do **not** grow `+112` restore, boast UI, or a `+56`
replay at this Note. Do **not** treat `004B2890` as a
second activate.

The next leftover on this walk is **not** another
`004B2890` arm. After `0049F259` the same function
packs a type-`9` record and writes `world+140`. Host
`InitCharactersAndQuests` never models that tail
(`0049F180-first-children` §5). That is the next
proven leftover **slice**.

Factory-0 `CreateFiber` / extra `00687540(55)` is
still leftover, but it belongs to **`004B3CE0`**
inside the just-finished `004B4260`. Collapsing it
into this VA is **DISPROVEN**.

---

## 1. First-seen vs host (unchanged)

Native (`listing-00480000.txt`):

```
0049F24E  call 004B4260          // world+172 activate
0049F253  mov ecx, [0x13B89FC]
0049F259  call 004B2890          // THIS VA
0049F25E  xor ecx, ecx
0049F260  call 009D8250          // ret stub
```

Taken body (`004B2890-empty-first`):

```
004B2890  [esi+112] next==head → je 004B2989
004B2989  [[0x13B86A0]+28] 00449970 / 00487DC0
004B29A6  je 004B2AC1            // no Thing
004B2AC1  pop / add esp, 28 / ret
```

Host (`InitCharactersAndQuests` after the name loop):

```
foreach (var name in _worldPlus172)
    ActivateNamedQuest(name, "Init Quests");   // 004B4260

Note(QuestManagerActivate, "Init Quests", "Quest", "004B2890");
```

`QuestManagerActivate = 0x004B2890`. `Note` (~7818):

```
Trace.Add(va, stage, subsystem, action);
```

`ForwardLifecycleTrace.Add` appends a
`ForwardLifecycleEvent`. No `EventPosts++`, no
`Runtime.ActivateQuest`, no QM walk. **PROVEN**
no mutation at this line.

`Init_quests_004B4260_activates_wld_initial_list`
locks the `004B4260` list / `QuestsInitDone`. It
does **not** assert `QuestManagerActivate` in the
trace. The Note is still the listing pair.

---

## 2. Still leftover?

### Side effect — no

| Host at this Note | Native first-seen | Class |
|---|---|---|
| `Note` then continue | call, two miss jumps, ret | **MATCH** skip |
| Walk `QM+112` / insert `QM+108` / event 41 | sentinel; loop not entered | would be **LEFTOVER** |
| Walk `QM+56` / `004B8E40` / event 73 | hero miss; list unread | would be **LEFTOVER** |
| Second `ActivateNamedQuest` pass | activate was `004B4260` | **DISPROVEN** |
| Drop the Note | listing pair `0049F24E` then `0049F259` | **DIVERGE** |

`QM+56` already has the nine `004B3CE0` nodes
(`004B2890-first`). First-seen still does **not**
load `[esi+56]`. That is **not** an empty list and
**not** work for this call.

### Comment — yes, one line

`EventManagerPumpFn` (`006874B0`) XML:

> first-seen `004B2890` walks empty `[quest+112]`
> (ctor sentinel; `004B4260` uses `+156` / a local
> vector).

The empty `+112` sentence is **MATCH**. It still
does not say `+56` was just filled and then skipped
for no Thing. That gap is **LEFTOVER** comment, not
a runtime store (`004B2890-first` §5).

### Mutations that are **not** this leftover

These run **before** the Note. Owner is `004B4260`
/ `004B3CE0`.

| Host | Native owner | Class vs `004B2890` |
|---|---|---|
| `ActivateNamedQuest` / `00CB5AD0` | `004B4260` | **DISPROVEN** as this body |
| `Runtime.ActivateQuest` / `CreateFiber` on factory 0 | leftover vs **`004B4063` stub** | **LEFTOVER** / **DIVERGE** vs `004B3CE0`, not this VA |
| `EventPosts++` kind 55 on stub names | factory arm only (`004B4042`) | same |
| `Note(01375454)` on every name | `004B3CE0` construct byte | leftover comment vs `004B3CE0` (`host-gate-va-leftover`) |

Do **not** schedule those three stub mutations as
the next `004B2890` slice.

---

## 3. Next proven leftover slice

**After `0049F259`, still inside `0049F180`:**

```
0049F25E  xor ecx, ecx
0049F260  call 009D8250            // ret — not leftover
0049F270  [esp+16]=9, [esp+20]=0xFF
0049F291  call 009F1760            // pack type-9 record
0049F296  mov ecx, [esi+12]
0049F299  call 00449700            // [manager+28]
0049F2A3  mov ecx, [0x13B86A0]
0049F2AD  call 0041649C            // game + record
0049F2B2  mov edx, [0x13B89BC]
0049F2BC  mov [esi+140], edx       // WorldFrame, 0 here
0049F2CB  ret 4
```

Host has **no** `009F1760` / `0041649C` / `world+140`
store (`0049F180-first-children` §4–§5).
`009D8250` is a one-insn `ret`. Not leftover work.

`0041649C` first child `0049D8C0` tests
`[0x13B9288 + type*64]`. Type **9** first-seen
nonzero is **UNREAD**. Hit would `004AE9A0` on
`game+80568`; miss still runs `0049E1D0` /
`00434A30`. That unread is **inside** this leftover
slice, not a reason to reopen `004B2890`.

`[world+140]=[0x13B89BC]` is WorldFrame **0** at
this instant (unique inc is later `004A5E10`).
Host omit of a 0-store is still a leftover **gap**
vs the listing, not a first-seen visible diverge.

### What is **not** the next slice

| Candidate | Why not |
|---|---|
| `+112` / `+56` arms of `004B2890` | first-seen not taken; leftover theater |
| `0049EAC0` `jmp 004B2890` | later / vtbl sibling, not first-seen |
| `004B4A10` at `00416C11` | sibling **after** `0049F180` returns; `+90584` empty vs `0x122D70E` → `je 00416C16` skip. Host Note of the skip is **MATCH**; Note of the VA as if it ran is leftover **parent** |
| `004B0D30` / `00896A30` card find | later; needs `004AF610` already active. Host Note inside this method is leftover **parent** |
| `004BBC00` at `00416C2C` | next sibling after the empty activate; `ret 4`, not this leftover |
| Load-game `004B07B7` then `004B2890` | **UNREAD**; not no-save |
| Oakvale / `00DBDE40` | not this walk |

`QuestsInitDone = true` after those Notes is a host
completion flag, not a QM write.

---

## 4. Timeline (no-save New Game)

```
0049F24E  004B4260([world+172])     // 004B3CE0 fills QM+56
0049F259  004B2890                  // THIS NOTE — MATCH skip
0049F260  009D8250 ret
0049F291  009F1760 / 0041649C       // NEXT leftover slice
0049F2BC  [world+140] = WorldFrame  // 0; host omit
0049F2CB  ret 4
00416BCF  Activate Initial Quests   // +90584 empty skip
00416C2C  004BBC00                  // ret 4
```

---

## Classifications (short)

1. **First-seen `004B2890` vs host is a Note-only
   skip. MATCH. PROVEN.** Same site, same empty
   body. Authority `004B2890-empty-first` /
   `host-004B2890-leftover`.

2. **No leftover side effect at this Note.
   DISPROVEN leftover mutation. PROVEN.**

3. **One leftover comment remains. PROVEN.**
   `EventManagerPumpFn` omits the hero miss /
   unread `+56`.

4. **Next proven `004B2890` body: none. PROVEN.**
   Load-game first take **UNREAD**.

5. **Next proven leftover slice: `0049F180` tail
   `009F1760` / `00449700` / `0041649C` /
   `world+140`. PROVEN** gap
   (`0049F180-first-children`). Type-9 take
   **UNREAD**.

6. **Factory-0 fiber leftover is not this VA.
   DISPROVEN** as a `004B2890` slice
   (`factory0-stub-vs-fiber`).
