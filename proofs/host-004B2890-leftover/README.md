# Host leftover at first `004B2890` after `004B4260`

Investigation only. No production `src/` edits.

Question: host Notes `004B2890` after `004B4260`. Native
empty `QM+112` sentinel. Any host leftover **side effect**?
Next proven `004B2890` body to implement (none if empty)?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Authority: `proofs/004B2890-first` (body / sentinel /
`+56` skip); `proofs/004B2890-empty-first` (first site
`0049F259`); `EngineLifecycle.InitCharactersAndQuests`;
ExeIndex `listing-00480000.txt` `004B2890` / `004B465D` /
`0049F24E` / `0049F259` / `0049EAC0`;
`EngineLifecycleTests.Init_quests_004B4260_activates_wld_initial_list`.

Do **not** start at `S_QNOVI` / `00DBDE40` /
`Q_NewOakValeIntro`. Those names are not this call.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Host Notes `004B2890` after `004B4260`? | **Yes.** `InitCharactersAndQuests` walks `world+172` via `ActivateNamedQuest` (the `004B4260` arm), then `Note(QuestManagerActivate, …, "004B2890")`. | **PROVEN** |
| Native empty sentinel? | **Yes.** Ctor `004B465D` dummy `[head]==head`. Persist `004B07B7` is not on no-save. `004B2890` `je 004B2989`. | **PROVEN** (`004B2890-first`) |
| Host leftover **side effect**? | **No.** `Note` only `Trace.Add`. No `EventPosts++`, no `Runtime.ActivateQuest`, no `QM+112`/`+108`/`+56` walk, no event 41/73. | **PROVEN** skip; **DISPROVEN** leftover mutation |
| Next proven `004B2890` body to implement? | **None.** First-seen body is empty. Implementing the skipped `+112` / hero / `+56` arms here is leftover theater. Load-game first `004B2890` is **UNREAD**. | **PROVEN** none |

---

## Verdict

**No leftover side effect. Next body: none.**

Native first-seen is `0049F259` immediately after
`0049F24E` `004B4260([world+172])`. Same `ecx=[0x13B89FC]`.
`QM+112` is still the ctor circular dummy, so the boast
restore loop is not entered. The tail looks up a player
Thing (`00449970` / `00487DC0`) and misses, so the
`QM+56` walk is not entered. Return. Stack only.

Host first-seen is the same pair: activate names, then a
trace line. The activate loop is **`004B4260` /
`004B3CE0`**, not this function. The `004B2890` line is
a skip of a **PROVEN** no-op.

Do **not** grow work at this Note. Do **not** treat
`004B2890` as a second activate.

---

## 1. Host site (`InitCharactersAndQuests`)

```
Note(InitCharactersFn, … "0049F180 …");
Note(PlayerCreatureBindFn, … "00449970 / 00487DC0");
Note(InitGuiFn, … "0043A380 …");
PlayerGuiReady = true;

Note(InitQuestsFn, … "004B4260 [world+172] …");
Runtime = ScriptRuntime.Detached();
// optional script.bin load
foreach (var name in _worldPlus172)
    ActivateNamedQuest(name, "Init Quests");   // 004B4260

Note(QuestManagerActivate, "Init Quests", "Quest", "004B2890");
Note(ActivateInitialQuestsSite, … "00416BCF +90584 empty … skip 004B4A10");
Note(ActivateInitialQuestsFn, … "004B4A10 not Q_NewOakValeIntro");
Note(QuestCardFindFn, …);
QuestsInitDone = true;
```

`QuestManagerActivate = 0x004B2890`. Only `src/` use
besides the constant and the `006874B0` comment.

`Note` (`EngineLifecycle` ~7494):

```
Trace.Add(va, stage, subsystem, action);
```

`ForwardLifecycleTrace.Add` appends a
`ForwardLifecycleEvent`. No other field. **PROVEN**
no mutation.

`Init_quests_004B4260_activates_wld_initial_list`
asserts the `004B4260` activate list / `QuestsInitDone`.
It does **not** assert `QuestManagerActivate` in the
trace. The Note is still the listing pair.

---

## 2. Native empty sentinel (`004B2890-first`)

Ctor `004B4590`:

```
004B465D  mov [esi+112], ebx        // 0
          push 40
          call 00BFEA0E
          mov [eax], eax
          mov [eax+4], eax
          mov [esi+112], eax        // next=prev=self
```

`004B4260` / `004B3CE0` write `QM+56` / `+156`, not
`+112`. Persist load `004B07B7` (`004B05C0` not mode
1/3) is the `+112` filler. Not on no-save Init Quests.

Head (`listing-00480000.txt`):

```
004B2890  sub esp, 28
          mov esi, ecx
          mov eax, [esi+112]
          mov edi, [eax]
          cmp edi, eax
          je  004B2989              // THIS PATH
004B2989  ecx = [[0x13B86A0]+28]
          00449970 / 00487DC0
          je  004B2AC1              // no Thing
004B2AC1  pop / add esp, 28 / ret
```

Taken stores: `[esp+20]` = sentinel, `[esp+28]` = 0.
No `00BFEA0E`, `004B73A0`, `004B1960`, `00687540(41)`,
`00687540(73)`, no `QM+108` insert.

`QM+56` already has the nine `004B3CE0` nodes. The
`+56` **consumer** is still skipped (hero miss). That
is **not** an empty list and **not** work for this
call.

---

## 3. What is **not** a `004B2890` leftover

These run **before** the Note. They belong to
`004B4260` / `004B3CE0` (or the later Activate
Initial Quests sibling). Collapsing them into
`004B2890` is **DISPROVEN**.

| Host action | When | Native owner | Class vs `004B2890` |
|---|---|---|---|
| `ActivateNamedQuest` / `00CB5AD0` / `004B3CE0` | foreach `world+172` **before** the Note | `004B4260` | **DISPROVEN** as this body |
| `Runtime.ActivateQuest` / fiber seed | inside that loop | factory / `00CB7900` (factory 0: stub, no fiber) | **DISPROVEN** as this body |
| `EventPosts++` / `00687540` kind **55** | inside that loop | `004B3CE0` on `[world+96]` | **DISPROVEN** as this body (41/73 never posted here) |
| `QuestsInitDone = true` | after later Notes | host completion of Init Quests | not a QM write |
| Activate Initial Quests Notes | after this Note | `00416BCF` sibling, not `004B2890` | **DISPROVEN** as this body |
| Later `PumpQuests` `004B4490` | first type-1 | different function | **DISPROVEN** |

Factory-0 fiber / extra `00687540(55)` on stub names
is leftover vs **`004B3CE0`**, already scoped in
`factory0-enqueue` / `004B3CE0-factory0`. Not a
`004B2890` side effect.

`EventManagerPumpFn` xml still says first-seen
`004B2890` walks empty `[quest+112]`. That sentence
is **MATCH**. It does not mention the hero miss /
unread `+56`. That gap is **comment leftover**, not
a runtime store.

---

## 4. What would be leftover (do not implement)

| Host action at this Note | Class |
|---|---|
| `Note` then continue | **MATCH** skip of a no-op |
| Walk `QM+112` / insert `QM+108` / post event 41 | **LEFTOVER** (sentinel) |
| Walk `QM+56` / `004B8E40` / `004B1960` / event 73 | **LEFTOVER** (hero miss) |
| Second `ActivateNamedQuest` pass | **DISPROVEN** (activate was `004B4260`) |
| Drop the Note | **DIVERGE** vs `0049F24E` then `0049F259` |
| Implement `004B4490` / `00CB8220` here | **DISPROVEN** (later pump) |
| Implement `0049EAC0` `jmp 004B2890` here | **DISPROVEN** as first-seen (later / vtbl sibling) |

`004B4A10` (ini `ActivateQuest`) does **not** call
`004B2890` (`004B2890-first`).

Load-game: `004B05C0` / `004B07B7` would fill `+112`,
then a later `004B2890` could copy into `+108`. That
first load-game take is **UNREAD**. It is not a
next **proven** no-save body.

---

## 5. Next proven body

**None.**

First-seen no-save `004B2890` is empty. The next
*work* on this walk is Activate Initial Quests
(`+90584` empty → `004B4A10` skip / user.ini
Gameflow later), then `004BBC00`, then pumps.
Those are other functions.

Do not schedule `+112` restore, boast UI, or
`+56` replay until a listing proves a taken
`004B28A4` fall-through or a taken `004B29A6`
hero hit. That site is not `0049F259`.

---

## Classifications (short)

1. **Host Notes `004B2890` after the `004B4260`
   name loop. PROVEN.** `InitCharactersAndQuests`.
2. **Native `QM+112` empty sentinel. PROVEN.**
   Authority `004B2890-first`.
3. **No host leftover side effect. PROVEN.**
   `Note` is `Trace.Add` only.
4. **Next proven `004B2890` body: none. PROVEN.**
   Implementing the skipped arms here is leftover.
5. **Treat `004B2890` as activate. DISPROVEN.**
