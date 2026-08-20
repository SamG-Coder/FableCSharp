# Leftover unread in `004B00C0` after first-gate — activate anything?

Investigation only. No production `src/` or `tests/` edits.

Question: remaining unread in `004B00C0` on first-seen
no-save. Does it activate anything? Host gap.

Do **not** re-prove `proofs/004B00C0-first-gate`. That
file closed the first name after first `004B4260`:
`Q_SunnyvaleMaster` is in `QM+44`, the gate **takes**,
host `ActivateNamedQuest` **MATCH** take. This note is
the leftover.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: `proofs/004B00C0-first-gate` (closed first
take); `proofs/quest-activate-gate`;
`proofs/qm44-gate-find` (nine TRUE already in `+44`);
`proofs/qst-first-load`; `proofs/ini-activate-quest`;
`proofs/00DBDE40-host-gap` (do not invent Oakvale
activate);
ExeIndex `listing-00480000.txt` (`004B00C0` /
`004B2850` / `004B4260` / `004B42D7` / `004B4A10` /
`004A10B2` / `004A1101` / `0049F24E`);
`listing-00400000.txt` `00411570` / `004115A0`;
`listing-00880000.txt` `00892F50`;
`e8.tsv` (one `E8` of `004B00C0`: `004B42D7`);
`out/01-sections/script-bank/quests-qst.md`;
host read-only `EngineLifecycle.ActivateNamedQuest` /
`StoreAddQuestNames` / `InitCharactersAndQuests`;
`EngineLifecycleTests.Init_quests_004B4260_activates_wld_initial_list`
/ `No_save_does_not_activate_Q_NewOakValeIntro`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Remaining unread **body** of `004B00C0` on this walk? | `'%'` Mid, empty intern vs `"NULL"`, `"NULL"` allow, skip `je 004B4363`, thunk `00892F56`. None fire. | **UNREAD** as shipped hazard; **PROVEN** unused first-seen |
| Remaining eight `world+172` TRUE names unread? | **No.** Same loop as the closed first take. `004B00C0` **takes**; `00CB5AD0` runs. Closed by `qm44-gate-find`. | **MATCH** take; **not leftover unread** |
| Does leftover unread **activate** extra names? | **No.** Unread arms do not run. FALSE catalog is never the `004B00C0` arg on Init Quests. | **PROVEN** no extra activate |
| `Q_NewOakValeIntro`? | **FALSE** `AddQuest`. Catalog `world+184` / `QM+44`. Not `world+172`. Init Quests never calls `004B00C0` with this name. | **PROVEN** catalog; **DISPROVEN** activate |
| Host gap? | Walk `_worldPlus172` only: **MATCH** omit Oakvale, **MATCH** remaining TRUE. Leftover extras (`OrdinalIgnoreCase`, no `'%'` Mid, empty silent `return`) unused. `inTable` **would** take Oakvale if someone called `ActivateNamedQuest`. | **MATCH** omit; **LEFTOVER** extras; **DISPROVEN** invent activate |

---

## Verdict

**Remaining unread in `004B00C0` does not activate
anything extra on first-seen no-save.**

Init Quests `0049F24E` walks **`world+172` TRUE only**.
`004B00C0` is a **membership find** on `QM+44`, not the
walk list. Being in `+44` does not start a quest.

`Q_NewOakValeIntro` is **FALSE** catalog (`world+184` /
`QM+44`). It is **not** an Init Quests arg. The unread
body (`'%'`, `"NULL"`, empty intern, skip, script thunk)
never runs on this walk.

Host already walks `_worldPlus172` then later
`user.ini` `Gameflow`. **MATCH** omit of Oakvale.
Do **not** close this leftover by calling
`ActivateNamedQuest("Q_NewOakValeIntro")`. The gate
**would** take (name already in `+44`). That is the
host gap, not a missing first-seen activate.

---

## Evidence → Original → Host → Gap

### 1. Closed first-gate (do not re-prove)

| | Original | Host | Class |
|---|---|---|---|
| First `004B4260` | `0049F247` `lea edx,[esi+172]` / `0049F24E` | `InitCharactersAndQuests` foreach `_worldPlus172` | **MATCH** |
| First name | `Q_SunnyvaleMaster` | `_worldPlus172[0]` | **MATCH** (`004B00C0-first-gate`) |
| Gate | `004B42D7` `004B00C0` → `al=1` | `inTable` true → `00CB5AD0` | **MATCH** take |

Everything below is leftover of that first name.

---

### 2. Init Quests walk is `world+172` TRUE only — **PROVEN**

Original (`listing-00480000.txt`):

```
004A1072  lea esi, [ebp+184]     // every AddQuest
004A10B2  test bl, bl            // 00BFEBA8("TRUE")
004A10B4  je  004A10F6           // FALSE skips +172
004A10C4  lea esi, [ebp+172]     // TRUE only
004A10F6  mov ecx, [0x13B89FC]
004A1101  call 004B2850          // QM+44 always
004A113B  AddTestQuest → +196    // no 004B2850

0049F21B  "Init Quests"
0049F247  lea edx, [esi+172]
0049F24E  call 004B4260          // THIS WALK
  004B42D7  call 004B00C0        // per +172 slot
  004B42DE  je  004B4363         // skip 00CB5AD0
  004B42E8  call 00CB5AD0        // TAKE
```

`quests-qst.md` / `qst-first-load`:

| Slot | Filled by | First-seen contents |
|---|---|---|
| `world+172` | `AddQuest` **TRUE** | nine names (8 FinalAlbion + `Global_WatchForHeroDeath`) |
| `world+184` | every `AddQuest` | 187 + 14 names |
| `QM+44` | `004B2850` every `AddQuest` | same catalog as `+184` |
| `world+196` | `AddTestQuest` | 112 cards; **not** this walk |

Nine TRUE (already closed as gate **takes** by
`qm44-gate-find`; leftover of first-gate’s first-name
scope, **not** unread):

1. `Q_SunnyvaleMaster` *(closed first-gate)*
2. `ChapterAndSceneManager`
3. `PersonalScriptMain`
4. `PersonalScript_GlobalThings`
5. `NPCDeath`
6. `HeroBoasts`
7. `V_HeroDolls`
8. `CS_PlayCutscene`
9. `Global_WatchForHeroDeath`

No `'%'` in `quests-qst.md`. None is `"NULL"` or empty.

Host:

```
StoreAddQuestNames:
  _worldPlus184.Add(name);                 // catalog
  if (Persistent) _worldPlus172.Add(name); // TRUE
  _questManagerPlus44.Add(name);           // always

InitCharactersAndQuests:
  foreach name in _worldPlus172            // not +184
    ActivateNamedQuest(name, "Init Quests");
```

| | Original | Host | Gap |
|---|---|---|---|
| Walk | `+172` nine TRUE | `_worldPlus172` nine | **MATCH** |
| Catalog | `+184` / `QM+44` | `_worldPlus184` == `_questManagerPlus44` | **MATCH** |
| Remaining eight TRUE | `004B00C0` take → `00CB5AD0` | `inTable` → `ActivateNamedQuest` | **MATCH** take |
| WLD `START_INITIAL_QUESTS` (six) | **not** this walk | host does **not** walk `World.InitialQuests` | **MATCH** omit |

`qst-first-load` “host still walks `World.InitialQuests`”
is **STALE**. Test locks `ActivatedQuests.Take(9) ==
WorldPlus172`.

---

### 3. Unread `004B00C0` body — **does not activate** — **PROVEN**

Original (`004B00C0`):

```
004B00C8  push 37                 // '%'
004B00CC  call 0099E5A0           // Find; -1 miss
004B00D4  jle  004B00E7           // THIS WALK: always (no %)
          0099EC70 Mid(0, '%')    // UNREAD
004B00E7  0099EC30 copy whole
004B00F5  test ecx, ecx
004B00F7  jne  004B013C           // THIS WALK: non-empty object
          intern 0x122D70E vs "NULL" → al=1   // UNREAD
004B013C  004115A0 "NULL" → al=1  // UNREAD (not these names)
004B0110  004B8FF0 on [this+44, this+48)
          setne al                // THIS WALK: 1
004B42DE  je 004B4363             // UNREAD skip
```

`00411570` (find compare) is byte `cmp al,bl`,
**case-sensitive**. First-seen names are same-call
copies, exact. Case-mismatch fail is **UNREAD**.

Only `.text` `E8` of `004B00C0`: `004B42D7`
(`e8.tsv`). Thunk `00892F50` `mov ecx,[0x13B89FC]` /
`00892F56 jmp 004B00C0` is a script wrapper, **not**
Init Quests. **PROVEN** unused on this walk.

| Unread arm | First-seen `+172` arg | Activates? |
|---|---|---|
| `'%'` Mid `0099EC70` | no `%` in nine names | **no** |
| empty intern vs `"NULL"` | not empty | **no** |
| `"NULL"` allow | not `"NULL"` | **no** |
| skip `je 004B4363` | `al=1` for all nine | **no** (skip is the non-activate path) |
| `00892F56` thunk | not this site | **no** |
| case-mismatch miss | exact copies | **no** |

**Unread ≠ activate.** Those arms are leftover of the
predicate, not extra `00CB5AD0` names.

Host extras vs those arms:

| Extra | Original | Host | First-seen | Gap |
|---|---|---|---|---|
| Case | `00411570` byte | `OrdinalIgnoreCase` | unused (exact) | **LEFTOVER** unused |
| `'%'` Mid | yes | **no** | no `%` | **LEFTOVER** unused; **UNREAD** as QST hazard |
| empty | intern / `"NULL"` then find | silent `return` | empty not in `+172` | **LEFTOVER** unused |
| `"NULL"` allow | `al=1` without find | `Equals("NULL", IgnoreCase)` | not this walk | **LEFTOVER** unused |

Do **not** implement `'%'` / case / empty to “finish”
`004B00C0`. First-seen does not exercise them.

---

### 4. `Q_NewOakValeIntro` is FALSE catalog — **not activated** — **PROVEN**

Evidence (`quests-qst.md` line 126 / `qst-first-load`):

```
AddQuest("Q_NewOakValeIntro", FALSE);          // +184 + QM+44
AddTestQuest("Q_NewOakValeIntro", "NOVStartHSP", …);  // +196 only
```

`004A10B4 je 004A10F6` skips `+172`. `004B2850` still
pushes the name onto `QM+44`. Init Quests never passes
that pointer to `004B00C0`.

`AddTestQuest` is **not** in `+44`. A later `004B4260`
of a test-only name would **skip**. Oakvale is **both**
FALSE `AddQuest` **and** a test card. Catalog membership
comes from `AddQuest`, not the card.

| | Original | Host | Class |
|---|---|---|---|
| In `world+184` / `QM+44` | yes | `_worldPlus184` / `_questManagerPlus44` | **MATCH** catalog |
| In `world+172` | **no** | `_worldPlus172` omits | **MATCH** omit |
| `004B00C0` arg on Init Quests | **never** | `ActivateNamedQuest` not called | **MATCH** omit |
| `ActivatedQuests` | not this name | test `DoesNotContain` | **MATCH** |
| `00CB5AD0` / `00DBDE40` | not from this gate | no `Va==00DBDE40` | **MATCH** omit |

If someone **did** call `004B4260` / `ActivateNamedQuest`
with this string, `004B00C0` / `inTable` would **take**
(`+44` hit). Native first-seen does not. Host first-seen
does not. Inventing that call is **DISPROVEN** as this
walk (`00DBDE40-host-gap` / PARITY “Who activates”).

Activator site remains **UNREAD**. Do not invent it here.

---

### 5. FALSE catalog unread as `004B00C0` args — **PROVEN**

Every FALSE `AddQuest` sits in `+184` / `+44` the same
way: `Gameflow`, `GameflowAssistance`, Oakvale, 179 more
FinalAlbion FALSE, 13 GlobalQuests FALSE.

Init Quests does **not** walk them. `004B00C0` is not
called with those names on this site.

Later (not leftover unread of Init Quests, leftover of
first-gate “later”):

```
user.ini  ActivateQuest("Gameflow")
00419CE0 → 00892E80 → 004B4A10 → 004B4260 (one-name)
  004B00C0  Gameflow ∈ +44 → al=1 → 00CB5AD0
```

That is a **second** `004B4260`, after `+90584` empty
skips the WLD `004B4A10`. It activates **Gameflow only**.
Not Oakvale. Host `ActivateNamedQuest("Gameflow",
"InitGame")` **MATCH**. Closed by `ini-activate-quest`.

| FALSE name | Init Quests `004B00C0` | Later first-seen | Activates? |
|---|---|---|---|
| `Q_NewOakValeIntro` | never | never | **no** |
| `Gameflow` | never | `004B4A10` take | **yes**, later site |
| other FALSE | never | not this walk | **no** |

Membership leftover does **not** mean “activate the
catalog.”

---

### 6. Host gap (read-only)

| Host action | Original owner | Class |
|---|---|---|
| Walk `_worldPlus172` then gate | `0049F24E` / `004B42D7` | **MATCH** |
| Remaining eight TRUE take | same loop | **MATCH** |
| Oakvale in `+184`/`+44`, not `ActivatedQuests` | FALSE catalog | **MATCH** omit |
| Later `Gameflow` via `user.ini` | `004B4A10` | **MATCH** |
| `OrdinalIgnoreCase` / no `'%'` / empty `return` | unread native arms | **LEFTOVER** unused |
| `Note(01375454)` per name | `004B3CE0` construct, not this gate | leftover **comment** (`host-gate-va-leftover`); not `004B00C0` |
| `004B3CE0` Note + `ActivateQuest` **per** name | native construct **once** after the loop | leftover of **`004B3CE0`**, not this gate |
| Call `ActivateNamedQuest("Q_NewOakValeIntro")` | no first-seen `E8` | **DISPROVEN**; `inTable` would take — **do not** |
| Walk `_worldPlus184` / `World.InitialQuests` | walk is `+172` | **DIVERGE** if added |

Smallest leftover on **this** VA is unused extras plus
the would-take-if-called hazard. There is **no** missing
first-seen activate to implement.

Construct / factory-0 stub / fiber after a **passing**
gate is **`004B3CE0` / `00CB5AD0` leftover**, already
named elsewhere. Collapsing it into unread `004B00C0`
is **DISPROVEN**.

---

## What this is not

| Claim | Class |
|---|---|
| Re-open first-gate skip of `Q_SunnyvaleMaster` | **DISPROVEN** (closed take) |
| Remaining TRUE names fail `004B00C0` | **DISPROVEN** (`qm44-gate-find`) |
| `QM+44` catalog **is** the Init Quests walk | **DISPROVEN** (walk is `+172`) |
| `Q_NewOakValeIntro` activated because it is in `+44` | **DISPROVEN** |
| Unread `'%'` / `"NULL"` / empty / thunk starts a quest | **DISPROVEN** |
| Host must call `ActivateNamedQuest` for Oakvale to finish `004B00C0` | **DISPROVEN** |
| Host first-gate extras (`IgnoreCase`, no Mid) fire on this walk | **DISPROVEN** unused |

---

## Classifications (short)

1. **Remaining unread `004B00C0` body on first-seen
   no-save — unused. PROVEN.** `'%'` Mid, `"NULL"`,
   empty intern, skip, `00892F56`. They do **not**
   activate anything.

2. **Remaining eight TRUE names — not unread.
   MATCH take.** Same `+172` loop as the closed first
   name. Authority `qm44-gate-find`. Host walks
   `_worldPlus172`.

3. **`Q_NewOakValeIntro` is FALSE catalog `world+184`
   / `QM+44`. PROVEN.** Init Quests never passes it
   to `004B00C0`. **DISPROVEN** activate.

4. **Leftover unread does not activate extra names.
   PROVEN.** Membership is the gate table, not the
   walk.

5. **Host gap — MATCH omit Oakvale; LEFTOVER unused
   extras; DISPROVEN invent `ActivateNamedQuest`.
   `inTable` would take if called.** Do not edit
   `src/` / `tests/` to close that.
)
