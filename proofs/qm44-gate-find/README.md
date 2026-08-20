# `004B00C0` find on `QM+44` — does every `world+172` TRUE name pass?

Investigation only. No production `src/` edits.

Question: `004B2850` pushes every `AddQuest` name onto
QuestManager `+44`. `004B00C0` finds in that list to gate
`004B4260` before `00CB5AD0`. First-seen: does every
`world+172` TRUE name hit `+44` so the gate passes? Can a
TRUE name fail the find? Relation to `ChapterAndSceneManager`
/ `NPCDeath` misses?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Authority: ExeIndex `listing-00480000.txt` `004B00C0` /
`004B2850` / `004B4260` (also `004A10C4` / `004A1101` /
`0049F24E` / `004B8FF0` / `004A08D0`);
`listing-00400000.txt` `00411570`;
`listing-00880000.txt` `00892F56`;
`listing-00980000.txt` `0099E5A0` / `0099EC70`;
`listing-00c80000.txt` `00CB5AD0`;
`proofs/quest-manager-plus44`;
also `qst-first-load`, `factory0-enqueue`,
`quest-activate-gate`, `qst-clear-004A08D0`;
`TlcInstallTests.Quest_table_includes_opening`;
`EngineLifecycleTests.Init_quests_004B4260_activates_wld_initial_list`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Every first-seen `world+172` TRUE name in `QM+44`? | **Yes.** Same `AddQuest` call writes TRUE → `+172`, then **always** `004B2850` the same stack `CString` onto `+44`. `+172` ⊂ `+44`. | **PROVEN** |
| Gate pass (`al=1`) for those nine names? | **Yes.** `004B42D7` `004B00C0` then `test al,al` / `je 004B4363`. First-seen find hits. All nine reach `00CB5AD0`. | **PROVEN** |
| Can a TRUE name fail the find on this walk? | **No.** Same-call copy, content compare (`004B8FF0` / `00411570`). No `'%'` in the nine names. Empty/`"NULL"` short-circuit is not this set. | **PROVEN** no-fail first-seen |
| Abstract find-fail still exist? | **Yes**, for a name `004B4260` walks that was never `004B2850`’d (`AddTestQuest`, unknown). Not a `+172` TRUE from this parse. | **PROVEN** skip path; **DISPROVEN** as first-seen TRUE |
| `ChapterAndSceneManager` / `NPCDeath` miss? | **`00CB5AD0` factory 0**, not a `004B00C0` miss. Gate **passes**. `004BB720` still runs. | **PROVEN** |

---

## Verdict

On no-save New Game, **every `world+172` TRUE name is already
in `QM+44` before the first `004B4260`.** The gate is a
membership find, not a factory check. It **passes** for all
nine QST TRUE names, including the two with **no**
`00CD52D0` row.

Those two names (`ChapterAndSceneManager`, `NPCDeath`) still
call `00CB5AD0` and miss (`eax=0`). That miss is **after**
the gate. Do not treat “no factory / not in WLD six” as
“failed `004B00C0`.”

A TRUE name **cannot** fail this find on the first-seen parse:
`004A10C4` (`+172`) and `004A1101` (`004B2850` → `+44`) share
`[esp+20]`. `004A08D0` does not wipe `+44`. The only `E8` of
`004B2850` is that `AddQuest` site.

---

## Timeline (no-save New Game)

```
004B4590  QuestManager ctor
  004B45BF  [QM+44/+48/+52]=0

004A1840  Load Quests
  004A0D90(FinalAlbion.qst, 1)
    004A08D0  clear world +184 / +172 / +196   // not QM+44
    AddQuest:
      +184 always
      test bl; TRUE → 004A10C4 +172
      004A10F6  ecx=[0x13B89FC]
      004A1101  call 004B2850                  // QM+44 always
    AddTestQuest → +196 only                   // no 004B2850
  004A0D90(GlobalQuests.qst, 0)                // append, same tokens

0049F23D  ecx=[0x13B89FC]
0049F247  lea edx, [esi+172]
0049F24E  call 004B4260
  for each +172 name:
    004B42D7  call 004B00C0                    // find QM+44
      al=0 → 004B4363  SKIP 00CB5AD0 and 004BB720
      al=1 → [edi+120] 00CB5AD0
               hit  → 004BB720 factory
               miss → 004BB720 factory 0
  004B4386  call 004B3CE0
```

`Q_NewOakValeIntro` is FALSE + `AddTestQuest`. Not in `+172`.
Not this question. **PROVEN.**

---

## 1. Same `AddQuest` writes `+172` then `+44`

`listing-00480000.txt` `004A10B2`–`004A1101`:

```
004A10B2  test bl, bl                 // 00BFEBA8("TRUE")
004A10B4  je  004A10F6                // FALSE skips +172
004A10C4  lea esi, [ebp+172]
          … 0099EC30 / 00433530 …     // TRUE only
004A10F6  mov ecx, [0x13B89FC]
          lea eax, [esp+20]           // same name CString
          push eax
004A1101  call 004B2850               // always, TRUE or FALSE
```

`004B2850` (`listing-00480000.txt`):

```
004B2850  mov eax, [ecx+52]           // cap
          lea esi, [ecx+44]           // begin
          mov ecx, [esi+4]            // end
          cmp ecx, eax
          je  004B2874                // 00433530 grow
          0099EC30 into *end
          add [esi+4], 4
          ret 4
```

Only `E8` of `004B2850` in the listings: `004A1101`. **PROVEN**
(`quest-manager-plus44`).

So after both `.qst` files (`qst-first-load`):

| Slot | Contents |
|---|---|
| `world+172` | nine TRUE names (8 FinalAlbion + `Global_WatchForHeroDeath`) |
| `QM+44` | every `AddQuest` (TRUE **and** FALSE) |

`+172` is a **subset** of `+44`, not an alias. Different
object (`CWorld` vs `[0x13B89FC]`). `004A08D0` erases the
world triples only (`qst-clear-004A08D0`). First-seen ctor
already zeroed `+44`; parse then fills it. **PROVEN.**

Nine TRUE names (`TlcInstallTests` / `quests-qst.md`):

1. `Q_SunnyvaleMaster`
2. `ChapterAndSceneManager`
3. `PersonalScriptMain`
4. `PersonalScript_GlobalThings`
5. `NPCDeath`
6. `HeroBoasts`
7. `V_HeroDolls`
8. `CS_PlayCutscene`
9. `Global_WatchForHeroDeath`

---

## 2. `004B00C0` is find-in-`+44`, then `004B4260` gates lookup

`004B4260` (`listing-00480000.txt`):

```
004B42D4  push esi                    // &world+172[i]
004B42D5  mov ecx, edi                // QuestManager
004B42D7  call 004B00C0
004B42DC  test al, al
004B42DE  je  004B4363                // skip 00CB5AD0 AND 004BB720
004B42E4  mov ecx, [edi+120]
004B42E8  call 00CB5AD0
          …
004B4361  mov bl, 1
004B4363  next index
```

Only `E8` of `004B00C0`: `004B42D7`. Thunk `00892F56` is
`mov ecx,[0x13B89FC]; jmp 004B00C0` (script, not this walk).
**PROVEN.**

`004B00C0`:

```
push 37                               // '%'
call 0099E5A0                         // CString::Find; -1 miss
jle  → 0099EC30 whole name
else → 0099EC70 Mid(0, '%')
empty intern 0x122D70E vs "NULL"  → al=1
else 004115A0 "NULL"              → al=1
else:
  ecx=[this+44]  edx=[this+48]
  call 004B8FF0                       // unrolled CString find
  cmp eax, end
  setne al                            // found → 1
```

`004B8FF0`: stride-4 scan. Hit = pointer equal, else length
`[+4]`, else `00411570` (byte, case-sensitive). Miss returns
`end`. First-seen names are **copies**, so the pointer-equal
fast path may miss and the length/`00411570` path still hits.
**PROVEN.**

`'%'` strip searches a **prefix** against **full** catalog
strings. A hypothetical `Foo%Bar` in both lists would look
up `Foo` and could miss. **UNREAD** as a shipped QST hazard:
none of the nine TRUE names (and no `%` in
`quests-qst.md`) contain `%`. **PROVEN** absence for this
walk.

---

## 3. First-seen TRUE cannot fail the find

| Failure mode | First-seen `+172` TRUE? |
|---|---|
| Never `004B2850`’d | **DISPROVEN.** Same call, after `+172`. |
| `004A08D0` dropped `+44` | **DISPROVEN.** Clears world vectors only. |
| Other `+172` writer (WLD `START_INITIAL_QUESTS`) | **DISPROVEN** as this writer (`wld-parse` / `world-plus172-activate`). |
| Empty / `"NULL"` allow without find | Not these nine strings. They still **pass** (`al=1`) via find. |
| `'%'` prefix desync | No `%` in the nine names. |
| Case / length mismatch vs `0099EC30` copy | Same bytes into both vectors. |
| Manager null | `004B4590` before `004A0D90`. `[0x13B89FC]` live. |

`AddTestQuest` names (`Q_NewOakValeIntro` card, …) are the
real `al=0` class: `+196` only, **no** `004B2850`. They are
**not** in `+172`. A later `004B4260` of such a name would
skip `00CB5AD0`. **PROVEN** skip path; **DISPROVEN** as a
TRUE-from-this-parse case.

`Gameflow` is FALSE → in `+44`, not `+172`. `user.ini`
`004B4A10` still finds it. **PROVEN** membership; not this
walk’s arg.

---

## 4. `ChapterAndSceneManager` / `NPCDeath` are gate **hits**

They are TRUE #2 and #5. They **are** `004B2850`’d. Gate
`al=1`. `004B4260` **does** call `00CB5AD0`.

No PE string (`strings.tsv`). No `00CD52D0` bind
(`script-factory-tables`). `00CB5AD0` returns **0**.
`004B4260` still `004BB720`s factory 0. `004B3CE0` allocates
a 52-byte named slot, **no** `00CB7900` fiber
(`factory0-enqueue`).

| Kind | `004B00C0` | `00CB5AD0` | `004BB720` |
|---|---|---|---|
| TRUE with factory (`Q_SunnyvaleMaster`, … `Global_WatchForHeroDeath`) | 1 | record | factory |
| TRUE without factory (`ChapterAndSceneManager`, `NPCDeath`) | **1** | **0** | factory 0 |
| Never in `+44` | **0** | **not called** | **not called** |

Do **not** invent a second registrar to “fix” the two names.
Absence from PE / `00CD52D0` is **PROVEN**. Another table is
**UNREAD**.

They are also **absent** from WLD `START_INITIAL_QUESTS`
(`World.InitialQuests` is six names). That is a **file-list**
difference, not a gate miss. Native `004B4260` walks QST
TRUE `+172`, so it **does** see them. Host
`InitCharactersAndQuests` now walks `_worldPlus172` (nine +
later `Gameflow`). Test locks `ActivatedQuests.Take(9) ==
WorldPlus172` and `Started==false` on those two. **PROVEN**
names on the walk; **DIVERGE** if host still grew a fiber
for factory 0 (`factory0-enqueue`: native stub, no fiber).

---

## What this is not

| Claim | Class |
|---|---|
| `004B4260` walks `QM+44` | **DISPROVEN** (walks the arg; first-seen `world+172`) |
| `QM+44` *is* `world+172` | **DISPROVEN** (third list; TRUE ⊂ every `AddQuest`) |
| Gate is factory / `[0x1375454]` / already-active | **DISPROVEN** (`quest-activate-gate`) |
| The two QST-only names fail `004B00C0` | **DISPROVEN** |
| Those two skip `00CB5AD0` | **DISPROVEN** (lookup runs; eax=0) |
| `Global_WatchForHeroDeath` is a gate miss | **DISPROVEN** (`00EE90A0` exists; TRUE #9) |
| Host must skip the two names because WLD six omits them | **DISPROVEN** as native (`+172` has them) |

---

## Classifications (short)

1. **Every first-seen `world+172` TRUE name is in `QM+44`.
   PROVEN.** Same `AddQuest` `CString` after the TRUE
   `push_back`. `004B2850` is the only writer.

2. **Gate passes for all nine. PROVEN.** `004B00C0` find
   hits → `00CB5AD0`. A TRUE name does **not** fail this
   find on the no-save parse.

3. **Find-fail is real only for non-`AddQuest` names.
   PROVEN** skip at `je 004B4363`. **DISPROVEN** for QST
   TRUE from `004A0D90`.

4. **`ChapterAndSceneManager` / `NPCDeath` — factory miss,
   not gate miss. PROVEN.** `al=1`, `00CB5AD0` eax=0,
   `004BB720` factory 0. Do not invent a registrar.
)
