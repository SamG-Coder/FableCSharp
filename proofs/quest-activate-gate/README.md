# `004B00C0` gate before `00CB5AD0` — first-seen New Game

Investigation only. No production `src/` edits.

Do **not** start at `00DBDE40` / `S_QNOVI` / `Q_NewOakValeIntro`.
That name is `AddQuest(..., FALSE)` + `AddTestQuest`. It is not
on the no-save `world+172` walk and is **not** this gate.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: `listing-00480000.txt` `004A0D90` / `004A1101` /
`004B00C0` / `004B2850` / `004B3CE0` / `004B3EED` / `004B4260` /
`004B4A10` / `004B8FF0` / `004AF610`;
`listing-00880000.txt` `00892F56`;
`listing-00980000.txt` `0099E5A0`;
`listing-00400000.txt` `00411570` / `004115A0`;
`tools/Fable.ExeIndex/out/00-index/sections.txt`;
`docs/runtime/FORWARD_TREE.md` §§7–11;
`docs/PARITY.md` Init Game suffix;
`EngineLifecycle` (`QuestFactoryGateVa` / `ActivateNamedQuest`);
`EngineLifecycleTests`
(`Init_quests_004B4260_activates_wld_initial_list`,
`Activate_quests_00CB5AD0_starts_factory_scripts`);
`proofs/qst-first-load`, `fiber-first`, `ini-activate-quest`,
`audit-startnewgame`.

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| First-seen `004B00C0` return on no-save New Game? | **`al = 1`**. First site is `0049F24E` → `004B4260([world+172])`. First name `Q_SunnyvaleMaster` is already in `[manager+44]`. | **PROVEN** |
| What does the gate test? | After optional `'%'` (37) strip: allow `"NULL"` / then **membership** in QuestManager `+44/+48` `CString` vector (`004B8FF0`). `al=1` → `00CB5AD0`. `al=0` → skip lookup. | **PROVEN** |
| Can it skip a `world+172` name? | **Not on this walk.** `+172` is `AddQuest` TRUE. Every `AddQuest` (TRUE and FALSE) `004B2850`s the same name onto `+44` first. Skip needs a name that is **not** in `+44`. | **PROVEN** no-skip for QST TRUE; **PROVEN** skip exists for non-members |
| `[0x1375454]` relation? | **None.** That byte is **not** read by `004B00C0`. One `.text` imm: `004B3EED` inside `004B3CE0` **after** the activate loop. `.data` first-seen `1` → full factory construct. | **PROVEN** distinct gate |

---

## Verdict

`004B4260` calls `004B00C0` **per name** before `[manager+120]`
`00CB5AD0`. The predicate is **“is this QST `AddQuest` name
(or `"NULL"`)?”**, not “already running” and not `[0x1375454]`.

No-save first walk: `004A0D90` has already pushed every
`AddQuest` name to `[0x13B89FC]+44`. `world+172` is the TRUE
slice of that same parse. First `004B00C0` therefore returns
**1** and does **not** skip `Q_SunnyvaleMaster` or any other
`+172` slot written on this path.

`[0x1375454]` is the later `004B3CE0` construct switch
(factory body vs 52-byte stub). BSS-0 stub is **DISPROVEN**.

---

## Timeline (no-save New Game)

```
0042F2A2  Leave frontend
004A1840  Load Quests
  004A0D90 FinalAlbion.qst flag 1
    AddQuest name → world+184
    TRUE         → world+172
    always       → 004B2850 [0x13B89FC]+44     // GATE TABLE
    AddTestQuest → world+196 only              // not +44
  004A0D90 GlobalQuests.qst flag 0             // append
0049F180  Init Characters / Init GUI
0049F21B  "Init Quests"
0049F23D  ecx = [0x13B89FC]
0049F247  lea edx, [esi+172]
0049F24E  call 004B4260                         // FIRST ACTIVATE
  loop name:
    "QuestManager: Activate Quest"
    004B42D7  call 004B00C0                    // THIS GATE
      al=1 → 00CB5AD0 / 004BB720
      al=0 → 004B4363 skip lookup
  004B4386  call 004B3CE0                      // always
    004AF610 already-active                    // different
    [edi+4]==0  → stub 004B4063
    [0x1375454]==0 → stub 004B4063            // NOT 004B00C0
    else factory + 00CB7900
00416BCF  +90584 empty skip 004B4A10
user.ini  ActivateQuest("Gameflow")
  004B4A10 → 004B4260 → 004B00C0 again
    Gameflow is AddQuest FALSE → still in +44 → al=1
```

`00DBDE40` is **not** on this list. **PROVEN**.

---

## 1. `004B4260` calls the gate, then lookup

`listing-00480000.txt` `004B4260`:

```
004B4260  sub esp, 44
          ebp = name-vector (arg)
          edi = ecx (QuestManager [0x13B89FC])
          eax = (end-begin)>>2
          jbe 004B437F                    // empty list
004B42A2  push "QuestManager: Activate Quest"
004B42D4  push esi                        // &name
004B42D5  mov ecx, edi
004B42D7  call 004B00C0
004B42DC  test al, al
004B42DE  je 004B4363                     // SKIP 00CB5AD0
004B42E4  mov ecx, [edi+120]
004B42E8  call 00CB5AD0
          hit  → 004BB720 factory
          miss → 004BB720 factory=0
004B4361  mov bl, 1
004B4363  next index; jb loop
004B4386  call 004B3CE0                   // once, after the loop
004B43BE  mov al, bl                      // any name passed
004B43C4  ret 12
```

Only `E8` of `004B00C0`: `004B42D7`. Script thunk
`00892F56` is `mov ecx,[0x13B89FC]; jmp 004B00C0`.
**PROVEN**.

First no-save site: `0049F247` `lea edx,[esi+172]` /
`0049F24E` `call 004B4260`. Same fn later from `004B4A5A`
(single-name wrapper / `user.ini` Gameflow).

---

## 2. What `004B00C0` tests

`listing-00480000.txt` `004B00C0` (`ecx` = QuestManager,
arg = `CString*`):

```
004B00C8  push 37                         // '%'
004B00CC  call 0099E5A0                   // strchr index or -1
004B00D1  cmp eax, -1
004B00D4  jle 004B00E7
          0099EC70 Mid(0, '%')            // prefix
          jmp 004B00F1
004B00E7  0099EC30 copy whole name
004B00F1  ecx = dest CString object
          test ecx, ecx
          jne 004B013C                    // non-empty object
          rep cmpsb 0x122D70E vs "NULL"   // empty intern
          je  → al=1                      // allow
          jmp lookup
004B013C  push "NULL"
          call 004115A0                   // [ecx] char* vs "NULL"
          test al, al
          je 004B0110                     // not NULL → lookup
          al=1                            // allow
004B0110  ecx = [this+44]  edx = [this+48]
          call 004B8FF0                   // CString* linear search
          setne al                        // found ≠ end
          ret 4
```

`0099E5A0` is `CString::Find(char)` (`00BFEDA0` / `-1` miss).
**PROVEN**.

`004B8FF0` is an unrolled 4-byte-stride scan of `[begin,end)`
(`sar ebx,4` = groups of four pointers, remainder `sar eax,2`).
Hit: pointer equality, else length at `+4`, else `00411570`.
Miss returns `end`. **PROVEN**.

| Result | Meaning | `004B4260` |
|---|---|---|
| `al=1` `"NULL"` / empty-as-NULL | no name filter | `00CB5AD0` |
| `al=1` found in `+44` | QST `AddQuest` name | `00CB5AD0` |
| `al=0` not in `+44` | unknown / `AddTestQuest`-only / never parsed | skip |

This is **not** `004AF610` (already constructed: walk
`[manager+56]` by name). `004AF610` runs later inside
`004B3CE0`. **PROVEN** distinct.

---

## 3. Who fills `+44` (why first-seen is 1)

Only `E8` of `004B2850`: `004A1101` inside `004A0D90`
`AddQuest` **after** the TRUE/`+172` branch.

`004B2850` is `vector<CString>::push_back` at `this+44/+48/+52`
(`add [esi+4],4` or `00433530` grow).

```
AddQuest("Name", TRUE|FALSE):
  copy → world+184
  TRUE → copy → world+172
  004B2850(Name) → manager+44          // always
AddTestQuest(...):
  world+196 only                       // no 004B2850
```

No-save: FinalAlbion flag 1 then GlobalQuests flag 0
(`qst-first-load`). First `004B4260` name is
`Q_SunnyvaleMaster` (TRUE #1). That pointer text is in `+44`.
`Find('%')` is `-1`. Not `"NULL"`. `004B8FF0` hits.
**`al=1`.** **PROVEN**.

Same for the other eight TRUE names and for later `Gameflow`
(FALSE, still `004B2850`’d). `004B4260` itself returns `bl=1`.

`0x122D70E` is the empty intern (sibling `004AFA7F` / `+90584`
compare). It is **not** `"NULL"`. Empty objects fall through to
the map. No empty `+172` slot after `004A08D0` + TRUE push.
**PROVEN**.

---

## 4. Can it skip a `world+172` name?

| Name class | In `+172`? | In `+44`? | `004B00C0` | `00CB5AD0` |
|---|---|---|---|---|
| QST TRUE (`Q_SunnyvaleMaster`, … `Global_WatchForHeroDeath`) | yes | yes | **1** | runs |
| QST FALSE (`Gameflow`, `Q_NewOakValeIntro`, …) | no | yes | **1** if someone `004B4260`s it | runs (Oakvale bind exists; activate is later / **UNREAD**) |
| `AddTestQuest` only | no | **no** | **0** | skipped |
| never `AddQuest` | no | **no** | **0** | skipped |
| `"NULL"` / empty intern | — | — | **1** | miss + factory 0 |

A `+172` slot written by this `004A0D90` **cannot** miss `+44`.
**PROVEN**.

Abstract skip still exists: `004B4260` on a name that was never
`004B2850`’d. `AddTestQuest` is that class. WLD
`START_INITIAL_QUESTS` is **not** the `+172` writer
(`qst-first-load` / `wld-parse`). If a caller passed a
non-`AddQuest` string, the gate would skip it.

`'%'` Mid: lookup uses the prefix only. No shipped `AddQuest`
name in-repo contains `%`. A hypothetical `+172` value
`"Foo%Bar"` would search `"Foo"` and could miss. **UNREAD**
on TLC QST bytes (no `%` in host `QuestFile` table). Treat as
non-issue for first-seen names.

`ChapterAndSceneManager` / `NPCDeath`: gate **passes** (TRUE →
`+44`). `00CB5AD0` **misses**. `004BB720` still runs with
factory 0. That is **not** a `004B00C0` skip.

---

## 5. `[0x1375454]` — later, different

| | `004B00C0` | `[0x1375454]` |
|---|---|---|
| Site | `004B42D7` per name | `004B3EED` inside `004B3CE0` |
| When | **before** `00CB5AD0` | **after** the enqueue loop |
| Test | `+44` membership / `"NULL"` | `.data` byte |
| Fail | skip lookup | `je 004B4063` 52-byte stub (same as factory `==0`) |
| First-seen | `al=1` | byte `1` → full factory |

`sections.txt`: VA `0x1375454` → RVA `0xF75454` in `.data`
(`rva=0xF74000`). One `.text` immediate: `004B3EED`
`mov al,[0x1375454]` / `test al,al`. Dword in-file
`0x01010101` so the byte is **1**. No `.text` imm writer.
BSS-0 “construct never runs” is **DISPROVEN**.

`004B3CE0` order per queued record:

```
004B3D2A  004AF610          // already active → skip
          [edi+4]==0        // no factory → stub 004B4063
004B3EED  [0x1375454]==0    // → stub 004B4063
          else 004AFA10 / factory vtbl / 00CB7900
```

A `004B00C0` miss never reaches this table. A factory-0 enqueue
hits `004B4063` **before** the byte. First-seen Sunnyvale has
factory `00CDD550` and byte 1 → construct. **PROVEN**.

Host `QuestFactoryGateVa = 0x01375454` / `FirstSeen = 1` notes
the construct flag. `ActivateNamedQuest` does **not** call
`004B00C0`. First-seen WLD six + `Gameflow` are all `AddQuest`
names, so the missing predicate does not change that walk.
Activating a non-`AddQuest` name from the host would
**DIVERGE** (native skip).

---

## Classifications (short)

1. **First-seen `004B00C0` — PROVEN `al=1`.**
   `0049F24E` / `Q_SunnyvaleMaster` is in `[manager+44]`.

2. **Gate test — PROVEN.**
   `'%'` strip, `"NULL"` allow, else `004B8FF0` on
   `AddQuest` names at `+44`. Not already-active, not
   `[0x1375454]`.

3. **Skip a `world+172` name — DISPROVEN on no-save.**
   `+172` ⊂ `+44`. Skip is real for names never `004B2850`’d
   (`AddTestQuest`, unknown).

4. **`[0x1375454]` — PROVEN unrelated to `004B00C0`.**
   `.data` 1. `004B3CE0` construct vs stub after enqueue.
