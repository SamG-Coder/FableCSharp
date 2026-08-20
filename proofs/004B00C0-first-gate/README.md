# First `004B00C0` after `004B4260` — name, `QM+44`, skip vs take

Investigation only. No production `src/` edits.

Question: `004B00C0` gates `ActivateNamedQuest` on `QM+44`.
First-seen after `004B4260`: which name is first, is it in
`QM+44`, and does the gate skip or take? How does host
`EngineLifecycle.ActivateNamedQuest` compare (do not edit it)?

Do **not** start at `S_QNOVI` / `00DBDE40` / `Q_NewOakValeIntro`.
That name is `AddQuest(..., FALSE)` plus `AddTestQuest`. It is
not on the no-save `world+172` walk.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Authority: Fable.exe dump
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00480000.txt`
(`004B00C0` / `004B2850` / `004B4260` / `004B42D7` / `004A1101` /
`004A193C` / `0049F24E` / `004B4A5A`)
and `listing-00400000.txt` (`00411570` / `004115A0`);
`listing-00880000.txt` (`00892F56`);
TLC `data\Levels\FinalAlbion.qst` /
`data\Levels\GlobalQuests.qst`.
Host text is read-only
(`src/Fable.Game/EngineLifecycle.cs`
`ActivateNamedQuest` / `StoreAddQuestNames` /
`InitCharactersAndQuests`).

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| First name after first `004B4260`? | **`Q_SunnyvaleMaster`.** `0049F24E` walks `world+172`. First TRUE `AddQuest` in `FinalAlbion.qst` is that string. | **PROVEN** |
| Is it in `QM+44`? | **Yes.** Same `AddQuest` always `004B2850`s the stack `CString` onto `[0x13B89FC]+44` after the TRUE `+172` push. | **PROVEN** |
| Gate skip or take? | **Take.** `004B42D7` `004B00C0` → `al=1` → fall through to `00CB5AD0`. Skip is `je 004B4363`. | **PROVEN** take |
| Host `ActivateNamedQuest`? | **MATCH take** for this first name. Same `+44` membership, same string, same `00CB5AD0` path. Extras (case, `'%'`, empty, per-name `004B3CE0`) are unused here. | **MATCH** first gate; **PARTIAL** extras |

---

## Verdict

On no-save New Game the first `004B4260` is Init Quests
`0049F24E([world+172])`. The first slot is
**`Q_SunnyvaleMaster`**. That name is already in **`QM+44`**.
`004B00C0` **takes** (`al=1`). It does **not** skip
`00CB5AD0`.

Host `ActivateNamedQuest` **takes** the same first name for
the same reason: `StoreAddQuestNames` copied every
`AddQuest` into `_questManagerPlus44`, and
`InitCharactersAndQuests` walks `_worldPlus172`.

`Q_NewOakValeIntro` is **not** this name. **DISPROVEN.**

---

## Timeline (no-save New Game)

```
004A1840  Load Quests
  004A1931  push 1
  004A193C  004A0D90(FinalAlbion.qst, 1)
    AddQuest("Q_SunnyvaleMaster", TRUE)     // file line 2
      +184 always
      +172 TRUE
      004A1101  004B2850 → QM+44            // always
    … more AddQuest …
  004A1991  push 0
  004A199C  004A0D90(GlobalQuests.qst, 0)    // append

0049F21B  "Init Quests"
0049F23D  ecx = [0x13B89FC]
0049F247  lea edx, [esi+172]
0049F24E  call 004B4260                     // FIRST ACTIVATE
  first slot:
    "QuestManager: Activate Quest"
    004B42D7  call 004B00C0                 // THIS GATE
      al=1 → 00CB5AD0 / 004BB720            // TAKE
      al=0 → 004B4363                       // skip (not this name)
  … remaining +172 names …
  004B4386  call 004B3CE0                   // once, after the loop
```

`S_QNOVI` / Oakvale is **not** on this list. **PROVEN.**

---

## 1. First name is `Q_SunnyvaleMaster` — **PROVEN**

`0049F247` `lea edx, [esi+172]` / `0049F24E` `call 004B4260`.
`esi` is the world. The walk is `world+172`, not `QM+44`,
not WLD `START_INITIAL_QUESTS`.

`004B4260` (`listing-00480000.txt`):

```
004B4265  mov ebp, [esp+56]        // arg0 = name vector
004B4269  mov eax, [ebp+4]         // end
004B4270  mov ecx, [ebp+0]         // begin
          sar eax, 2               // count
          jbe 004B437F
004B42CA  mov ecx, [ebp+0]
004B42D1  lea esi, [ecx+edx*4]     // &name[i], i from 0
004B42D4  push esi
004B42D5  mov ecx, edi             // QuestManager
004B42D7  call 004B00C0
```

First iteration uses index 0. That pointer is the first
`CString` on `world+172`.

`world+172` is filled only by `AddQuest` **TRUE**
(`004A10B2` `test bl,bl` / `004A10C4` `lea esi,[ebp+172]`).
`004A1840` parses `FinalAlbion.qst` **first** (flag 1) then
`GlobalQuests.qst` (flag 0). **PROVEN.**

TLC `FinalAlbion.qst` head:

```
AddQuest("Q_SunnyvaleMaster", 			TRUE);
AddQuest("ChapterAndSceneManager", 		TRUE);
AddQuest("PersonalScriptMain", 			TRUE);
AddQuest("PersonalScript_GlobalThings",		TRUE);
AddQuest("NPCDeath", 				TRUE);
AddQuest("HeroBoasts",				TRUE);
AddQuest("Gameflow", 				FALSE);
```

Later TRUE in the same file: `V_HeroDolls`, `CS_PlayCutscene`.
`Q_NewOakValeIntro` is **FALSE** (line 129).

`GlobalQuests.qst` head (parsed **after**):

```
AddQuest("Global_WatchForHeroDeath", TRUE);
```

So `world+172[0]` is **`Q_SunnyvaleMaster`**. Not
`Global_WatchForHeroDeath`. Not `Gameflow`. Not Oakvale.

No `'%'` in that string. **PROVEN** from the QST bytes.

---

## 2. That name is in `QM+44` — **PROVEN**

Same `AddQuest` after the TRUE push:

```
004A10F6  mov ecx, [0x13B89FC]
004A10FC  lea eax, [esp+20]        // same name CString
004A1101  call 004B2850
```

`004B2850` is `vector<CString>::push_back` at `this+44`:

```
004B2850  mov eax, [ecx+52]
          lea esi, [ecx+44]
          mov ecx, [esi+4]
          … 0099EC30 copy or 00433530 grow …
          add [esi+4], 4
```

Only `E8` of `004B2850` in the listing: `004A1101`.
TRUE and FALSE both push. `AddTestQuest` does **not**.

Therefore `+172` ⊂ `+44` after this parse. First TRUE is
in `+44`. **PROVEN.**

---

## 3. Gate **takes** — **PROVEN**

`004B00C0` (`ecx` = QuestManager, arg = `CString*`):

```
004B00C8  push 37                  // '%'
004B00CC  call 0099E5A0            // Find; -1 miss
          jle  → 0099EC30 whole name
          else → 0099EC70 Mid(0, '%')
empty intern 0x122D70E vs "NULL"  → al=1
else 004115A0 "NULL"              → al=1
else:
  ecx=[this+44]  edx=[this+48]
  call 004B8FF0                   // CString find
  setne al                        // found → 1
```

`004B8FF0` is a stride-4 scan. Hit = pointer equal, else
length `[+4]`, else `00411570` (byte `cmp al,bl`,
**case-sensitive**). Miss returns `end`.

`004B4260` after the call:

```
004B42DC  test al, al
004B42DE  je  004B4363             // SKIP 00CB5AD0 and 004BB720
004B42E4  mov ecx, [edi+120]
004B42E8  call 00CB5AD0            // TAKE
```

First-seen `Q_SunnyvaleMaster`:

| Step | Result |
|---|---|
| `Find('%')` | `-1` (no `%` in the QST name) |
| empty / `"NULL"` | no |
| `004B8FF0` on `QM+44` | **hit** (same-call copy) |
| `al` | **1** |
| `je 004B4363` | **not taken** |

**Take.** Skip exists for names never `004B2850`’d
(`AddTestQuest`, unknown). That is **not** this first name.
**PROVEN** take; **DISPROVEN** skip for
`Q_SunnyvaleMaster`.

Only `E8` of `004B00C0`: `004B42D7`. Thunk `00892F56` is
`mov ecx,[0x13B89FC]; jmp 004B00C0` (script, not this
first walk).

---

## 4. Host `ActivateNamedQuest` — first gate **MATCH**

Read-only. Not edited.

`InitCharactersAndQuests` walks `_worldPlus172` then
calls `ActivateNamedQuest(name, "Init Quests")`.
`StoreAddQuestNames` (the `004A0D90` stand-in):

```
_worldPlus184.Add(quest.Name);
if (quest.Persistent) _worldPlus172.Add(quest.Name);
_questManagerPlus44.Add(quest.Name);
```

`ActivateNamedQuest`:

```
if (name.Length == 0) return;
var inTable = name.Equals("NULL", OrdinalIgnoreCase) ||
    _questManagerPlus44.Exists(n =>
        n.Equals(name, OrdinalIgnoreCase));
Note(004B00C0, … hit or "miss skip 00CB5AD0");
if (!inTable) return;
… 00CB5AD0 / 004BB720 / 004B3CE0 …
```

| Native first name | Host first name | Class |
|---|---|---|
| `Q_SunnyvaleMaster` | `_worldPlus172[0]` same string | **MATCH** |
| already in `QM+44` | already in `_questManagerPlus44` | **MATCH** |
| `004B00C0` **take** | `inTable` true → `00CB5AD0` | **MATCH** |
| no `'%'` / not `"NULL"` / not empty | those host extras unused | **MATCH** this walk |

Later extras (not the first-gate decision):

| Extra | Native | Host | Class for *this* first name |
|---|---|---|---|
| Case | `00411570` byte | `OrdinalIgnoreCase` | unused (exact) — **UNREAD** as a shipped hazard |
| `'%'` Mid | yes | no | no `%` in first name — **UNREAD** as a QST hazard |
| empty | intern vs `"NULL"` then find | silent `return` | empty is not `+172[0]` |
| `004B3CE0` | once after the loop | Note + construct **per** name | after the take; not the gate |
| factory-0 names | still **take** `004B00C0` | still **take** | not the first name |

Host `ActivateNamedQuest` is also the `user.ini`
`ActivateQuest("Gameflow")` arm (`004B4A10` → temp
one-name `004B4260`). That is **later**. First-seen
first gate is Init Quests, not Gameflow.

Do **not** treat WLD’s six `START_INITIAL_QUESTS` as
the walk. Native first name happens to be in that six,
but the list is QST TRUE `+172` (nine names). Host
walks `_worldPlus172`, not `World.InitialQuests`.

---

## What this is not

| Claim | Class |
|---|---|
| First name is `Q_NewOakValeIntro` / `S_QNOVI` | **DISPROVEN** (FALSE + test card) |
| First name is `Global_WatchForHeroDeath` | **DISPROVEN** (second file) |
| First name is `Gameflow` | **DISPROVEN** (FALSE; `user.ini` later) |
| Gate **skips** `Q_SunnyvaleMaster` | **DISPROVEN** |
| `004B4260` walks `QM+44` | **DISPROVEN** (walks `+172`; `+44` is the find) |
| Host first gate skips this name | **DISPROVEN** |

---

## Classifications (short)

1. **First name after first `004B4260` — `Q_SunnyvaleMaster`.
   PROVEN.** QST file order + `world+172` + index 0.

2. **In `QM+44` — PROVEN.** Same `AddQuest` `004B2850`.

3. **Gate takes — PROVEN.** `al=1` → `00CB5AD0`. Skip is
   not this name.

4. **Host `ActivateNamedQuest` — MATCH take** on that
   first name and `+44` membership. Do not edit it.
)
