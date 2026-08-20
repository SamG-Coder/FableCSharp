# Host leftover at `01375454` in `ActivateNamedQuest`

Investigation only. No production `src/` edits.

Question: host Notes `01375454` as a **gate** in
`ActivateNamedQuest`. Native `004B00C0` is QM+44 find;
`01375454` is `004B3CE0` construct. Host leftover
**comment**?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Authority: `EngineLifecycle.ActivateNamedQuest`;
`proofs/quest-activate-gate` (`004B00C0` ≠ `[0x1375454]`);
`proofs/004B3CE0-factory0` / `factory0-enqueue` (byte after
factory-0); `proofs/qm44-gate-find`;
ExeIndex `listing-00480000.txt` `004B00C0` / `004B42D7` /
`004B3CE0` / `004B3EED` / `004B4386`;
`tools/Fable.ExeIndex/out/00-index/sections.txt`;
`EngineLifecycleTests.Init_quests_004B4260_activates_wld_initial_list`.

Do **not** start at `S_QNOVI` / `00DBDE40` /
`Q_NewOakValeIntro`. Those names are not this walk.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Host Notes `01375454` in `ActivateNamedQuest`? | **Yes.** After a passing `004B00C0` find, before `00CB5AD0` / `004BB720` / `004B3CE0` Notes. Same line on `InitGame` Gameflow. | **PROVEN** |
| Native activate gate? | **`004B00C0`.** `'%'` strip, `"NULL"` allow, else `004B8FF0` on `QM+44`. `al=0` skips `00CB5AD0`. | **PROVEN** (`quest-activate-gate`) |
| Native `[0x1375454]`? | **`004B3CE0` construct switch** at `004B3EED`, **after** the enqueue loop, only if `[rec+4]!=0`. `.data` first-seen `1`. | **PROVEN** |
| Host leftover **comment**? | **Yes.** Constant name `QuestFactoryGateVa` + per-name Note make the byte look like the activate gate. It is not. | **PROVEN** leftover comment |
| Host leftover **side effect**? | **No.** `Note` is `Trace.Add` only. `QuestFactoryGateFirstSeen` is never branched. | **PROVEN** skip; **DISPROVEN** leftover mutation |

---

## Verdict

**Leftover comment / trace. No leftover side effect.**

Native first-seen `004B4260` (`0049F24E` `[world+172]`, later
`004B4A10` Gameflow) gates **per name** with **`004B00C0`**
(`QM+44` membership). Host already implements that find
(`QuestActivateGateFn`). A miss returns before lookup.

`[0x1375454]` is a later `.data` byte inside **one**
`004B3CE0` walk of the queued 12-byte records. First-seen
value `1` means construct, not stub, for a **non-zero**
factory. Factory 0 (`ChapterAndSceneManager` / `NPCDeath`)
never loads the byte (`004B3CE0-factory0`).

Host still emits `Note(QuestFactoryGateVa, …,
"01375454=1 .data")` on **every** name that passed
`004B00C0`, including factory 0, and again from
`PumpQuests` (`004B4490` never reads the VA). That is
leftover theater next to the real gate. The XML body on
the constant is already the construct cmp, not a skip
predicate.

Do **not** treat `01375454` as the `ActivateNamedQuest`
gate. Do **not** implement a skip from this Note.

---

## 1. Host site (`ActivateNamedQuest`)

`EngineLifecycle` ~6555:

```
if (name.Length == 0)
    return;
var inTable = name == "NULL" || _questManagerPlus44.Exists(name);
Note(QuestActivateGateFn, …,                 // 004B00C0
    inTable ? "004B00C0 [QM+44] {name}"
            : "004B00C0 miss skip 00CB5AD0 {name}");
if (!inTable)
    return;                                  // THIS is the gate

Note(ActivateQuestFn, … "00CB5AD0 " + name);
Note(QuestFactoryCollectFn, … "004BB720");
Note(QuestFactoryGateVa, …                   // 01375454
    "01375454=1 .data");                     // LEFTOVER TRACE
var factory = QuestFactoryTable.Find(name);
if (factory is { } bind)
    Note(QuestFactoryStartFn, … "004B3CE0 construct");
Runtime.ActivateQuest(…);
```

Callers: `InitCharactersAndQuests` foreach `_worldPlus172`
(phase `"Init Quests"`), then `user.ini` `ActivateQuest`
(phase `"InitGame"`). Same Note on both.

`QuestFactoryGateVa = 0x01375454`.
`QuestFactoryGateFirstSeen = 1` (const). No `if`
on that value anywhere in `src/`.

`Note` (`EngineLifecycle` ~7527):

```
Trace.Add(va, stage, subsystem, action);
```

`ForwardLifecycleTrace.Add` appends a
`ForwardLifecycleEvent`. No other field. **PROVEN**
no mutation.

`Init_quests_004B4260_activates_wld_initial_list` asserts
`QuestActivateGateFn == 0x004B00C0` and a `004B00C0`
trace for `Q_SunnyvaleMaster`. It does **not** require
`QuestFactoryGateVa` in the trace. Pump tests only
assert the constant / first-seen `1`.

---

## 2. Native activate gate is `004B00C0`

`listing-00480000.txt` `004B4260`:

```
004B42D4  push esi                 // &name
004B42D5  mov ecx, edi             // QuestManager
004B42D7  call 004B00C0            // only E8
004B42DC  test al, al
004B42DE  je 004B4363              // SKIP 00CB5AD0
004B42E8  call 00CB5AD0
          hit  → 004BB720 factory
          miss → 004BB720 [rec+4]=0
…
004B4386  call 004B3CE0            // once, after the loop
```

`004B00C0` (`ecx` = manager, arg = `CString*`):

```
004B00C8  push 37                  // '%'
          call 0099E5A0            // Find
          Mid prefix or copy whole
          "NULL" / empty intern → al=1
004B0110  ecx=[this+44] edx=[this+48]
          call 004B8FF0            // linear find
          setne al
          ret 4
```

No load of `0x1375454`. **PROVEN** (`quest-activate-gate`
§5). Host `inTable` / early return **MATCH** this
predicate for first-seen `AddQuest` names.

---

## 3. Native `01375454` is `004B3CE0` construct

One `.text` immediate in the dump:
`004B3EED` `mov al, [0x1375454]` / `test al,al` /
`je 004B4063`.

```
004B3E8C  call 004AF610            // already on +56?
004B3EE4  cmp [edi+4], ebx         // ebx=0
004B3EE7  je 004B4063              // factory 0 — unread byte
004B3EED  mov al, [0x1375454]
004B3EF2  test al, al
004B3EF4  je 004B4063              // stub only if factory != 0
          else 004AFA10 / factory / 00CB7900
```

`sections.txt`: VA `0x1375454` → RVA `0xF75454` in
`.data` (`rva=0xF74000`). In-file dword `0x01010101`
so the byte is **1**. No `.text` writer. BSS-0 stub
is **DISPROVEN**.

| | `004B00C0` | `[0x1375454]` |
|---|---|---|
| Site | `004B42D7` **per name** | `004B3EED` inside `004B3CE0` |
| When | **before** `00CB5AD0` | **after** the enqueue loop |
| Test | `QM+44` / `"NULL"` | `.data` byte |
| Fail | skip lookup | 52-byte stub (`004B4063`) |
| First-seen | `al=1` | byte `1` → construct |
| Factory 0 | still `al=1` | **unread** |

`004B4490` (`PumpQuests`) has **no** load of the VA.

---

## 4. What is leftover (comment / trace)

| Host text | Native owner | Class |
|---|---|---|
| Name `QuestFactoryGateVa` | construct cmp, not activate gate | **LEFTOVER** naming (`004B00C0` is `QuestActivateGateFn`) |
| `Note(0x01375454)` in `ActivateNamedQuest` | not a `004B4260` insn | **LEFTOVER** comment / trace |
| Same Note on factory-0 names | byte unread (`je 004B4063` first) | **LEFTOVER** |
| `PumpQuests` `Note(01375454)` + `"004B3CE0 construct already at 004B4260"` | `004B4490` does not read / construct | **LEFTOVER** (value MATCH, site wrong) |
| XML on the constant (`004B3CE0` `cmp`, `.data` 1) | `004B3EED` | **MATCH** body; name still leftover |
| `quest-activate-gate` “`ActivateNamedQuest` does **not** call `004B00C0`” | host now finds `QM+44` | **STALE** (that gap is closed) |

`FORWARD_TREE` nests `[0x1375454]=1` under
`004B3CE0` construct after `004B4260` / Gameflow —
**MATCH**. Nesting construct under first `004B4490`
is the same leftover as the pump Note.

---

## 5. What is **not** leftover

| Host action | Native owner | Class |
|---|---|---|
| `004B00C0` `QM+44` find / miss skip | `004B42D7` | **MATCH** |
| `00CB5AD0` / `004BB720` Notes after pass | same loop | **MATCH** when |
| `004B3CE0 construct` Note only if `QuestFactoryTable.Find` hits | factory arm, not factory 0 | **MATCH** first-seen split |
| `QuestFactoryGateFirstSeen == 1` | `.data` 1 | **MATCH** value |
| `Runtime.ActivateQuest` / `EventPosts++` | factory / `00687540` inside `004B3CE0` | not this byte (see `004B3CE0-factory0`) |

Collapsing `004B00C0` into `[0x1375454]` is
**DISPROVEN**. The host already keeps them as two
constants; only the second Note is misplaced.

---

## 6. What would be leftover (do not implement)

| Host action at this Note | Class |
|---|---|
| `Note` then continue (current) | leftover **comment**; no store |
| Use `01375454` to skip `00CB5AD0` | **DISPROVEN** (wrong gate) |
| Use `01375454==0` to stub factory 0 | **DISPROVEN** (unread) |
| Drop the `004B00C0` find | **DIVERGE** vs `004B42DE` |
| Drop the `01375454` Note | comment cleanup; **not** a runtime **DIVERGE** |
| Branch construct on a writable host copy of the byte | first-seen is PE `.data` 1; no writer |
| Read the byte again in `PumpQuests` | **LEFTOVER** vs `004B4490` |

Load-game / a later writer of `[0x1375454]` is
**UNREAD**. First-seen no-save does not need one.

---

## Classifications (short)

1. **Host Notes `01375454` in `ActivateNamedQuest`.
   PROVEN.** After a passing `004B00C0`.
2. **Native activate gate is `004B00C0` `QM+44` find.
   PROVEN.** Authority `quest-activate-gate`.
3. **Native `01375454` is `004B3CE0` construct.
   PROVEN.** `004B3EED`; factory 0 unread.
4. **Leftover comment / trace. PROVEN.** Name
   `QuestFactoryGateVa` + per-name / pump Notes.
5. **No leftover side effect. PROVEN.** `Note` is
   `Trace.Add` only; first-seen const never branches.
6. **Treat `01375454` as the activate gate.
   DISPROVEN.**
