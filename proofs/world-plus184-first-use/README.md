# `CWorld+184` — all `AddQuest` names; first FALSE use after Leave

Investigation only. No production `src/` edits.

Do **not** start at `00DBDE40` / `S_QNOVI` / `Q_NewOakValeIntro`
as an activate. That name is `AddQuest(..., FALSE)` in
`FinalAlbion.qst`. Leave never constructs it.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: ExeIndex `listing-00480000.txt` (`004A08D0` / `004A0D90`
`004A1080` / `004A1840` / `004A67D0` / `004A6AB0` / `0049F180`
`004B00C0` / `004B2850` / `004B4260` / `004B8FF0`),
`listing-00880000.txt` (`00893610`),
`e8.tsv` (`004A0D90` / `004A08D0` / `004B2850` / `004B4260`
`004B4A10`), `00-index/xrefs.tsv` (`AddQuest` / `AddTestQuest`);
`out/01-sections/newgame-trace/addtestquest-004a0d90.md`;
`out/01-sections/script-bank/quests-qst.md`;
`proofs/qst-first-load/README.md`, `qst-first-quest`,
`ini-activate-quest`, `script-gameflow`, `wld-parse`;
`docs/runtime/FORWARD_TREE.md` §10; `docs/PARITY.md`
(Load world / who-activates).

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Does `world+184` hold **all** `AddQuest` names (TRUE and FALSE)? | **Yes.** `004A1080` `push_back`s the name **before** the persist test. TRUE also goes to `+172`. FALSE does not. | **PROVEN** |
| Who **walks** `+184` after Leave? | **Nobody on the no-save play path.** Sites are ctor zero, flag-1 clear, parse push, dtor free. First activate is `0049F247` `lea edx, [esi+172]` → `004B4260`. | **PROVEN** |
| Is `Gameflow` in `+184`? | **Yes** (`AddQuest("Gameflow", FALSE)` — first FALSE row in `FinalAlbion.qst`). It is **not** in `+172`. `user.ini` starts it by name, not by walking `+184`. | **PROVEN** |
| `Q_NewOakValeIntro` is FALSE — used **from** `+184`? | **No.** Stored in `+184` (and copied to `manager+44`). No later reader iterates `+184`. First later *mention* is Gameflow `00893610` (PE string → active-list miss). | **PROVEN** not from `+184` |
| First *use* of a FALSE name after Leave, **not** `user.ini` `Gameflow`? | First type-1 `00CE7670` `00893610("Q_NewOakValeIntro")` yield. That is **not** a `+184` walk. | **PROVEN** wait; **DISPROVEN** as `+184` use |

---

## Verdict

`CWorld+184/+188/+192` is the QST **catalogue** vector
(begin / end / cap): every `AddQuest("name", TRUE|FALSE)`
name, file order, both `.qst` files.

It is **not** the activate list. That is `+172` (TRUE only).
`AddTestQuest` is `+196` only.

After Leave fill, **no** function walks `+184` to pick a
FALSE name. `004B4260` is handed `+172`. `004B00C0` **searches**
the parallel copy at **quest-manager `+44`** (filled by
`004B2850` from the same stack `CString`, not by reading
`[world+184]`). `user.ini` `ActivateQuest("Gameflow")` is a
literal name, not an index into `+184`.

So: **FALSE names live in `+184`. Nothing after Leave consumes
them from `+184`.** Host must not invent
`ActivateQuest("Q_NewOakValeIntro")` because that string sits
in the catalogue.

---

## Timeline (no-save New Game)

```
0042F2A2 Leave frontend
004A67D0 CWorld ctor
  [+172/+176/+180]=0
  [+184/+188/+192]=0                         // 004A68AE / 004A68C0
00416953 Load world
  00416ABA 004A1840
    004A193C 004A0D90(world, FinalAlbion.qst, 1)
      004A0DA2 004A08D0                      // clear +184 / +172 / +196
      token AddQuest:
        004A1080 lea esi, [ebp+184]          // ALWAYS
        00433530 / add [esi+4], 4
        test bl  (00BFEBA8 "TRUE")
          jne → 004A10C4 lea esi, [ebp+172]  // TRUE only
        004A1101 004B2850                    // manager+44, stack name
      token AddTestQuest:
        004A113B → +196 only                 // no +184, no 004B2850
    004A199C 004A0D90(world, GlobalQuests.qst, 0)
      no 004A08D0; same tokens; append
    Startup WAD / 00507C30 / Set Static Map
  [0x13B8648]==0
    0049F180
      0049F247 lea edx, [esi+172]            // NOT +184
      0049F24E 004B4260
        004B00C0 → 004B8FF0(manager+44)      // search, TRUE names
        00CB5AD0 / 004B3CE0
user.ini ActivateQuest("Gameflow")           // not a +184 walk
  00419CE0 → 00892E80 → 004B4A10 → 004B4260("Gameflow")
first type-1 00CE7670
  00893610 "Q_NewOakValeIntro" → 0           // PE name, not +184
  006E7410 yield
```

`00DBDE40` / `S_QNOVI` are **not** on this list. **PROVEN**.

---

## 1. Writer `004A0D90` (`listing-00480000.txt`)

`ecx` = `CWorld` (`ebp`). Flag `[esp+8]`:

| Flag | Site | Effect |
|---|---|---|
| 1 | `004A193C` `FinalAlbion.qst` | `004A0DA2` `004A08D0` then parse |
| 0 | `004A199C` `GlobalQuests.qst` | append; skip clear |

`e8.tsv`: **two** `004A0D90` sites, both inside `004A1840`.
Zero `E8` on Leave `0042F2A2`. **PROVEN**.

`004A08D0` (`e8.tsv`: **one** site `004A0DA2`):

```
004A08D4  mov eax, [esi+188]
004A08DA  mov edx, [esi+184]
004A08E0  lea ecx, [esi+184]
          call 0043336A                 // destroy [begin,end)
then +172 via 0043336A
then +196 via 004AA580
```

`AddQuest` name store — **not** gated on TRUE:

```
004A1015  push "TRUE"
004A104F  call 00BFEBA8
004A1059  neg ebx / sbb bl, bl / inc bl   // bl=1 iff TRUE
004A1072  mov ecx, [ebp+188]
004A1078  mov eax, [ebp+192]
004A1080  lea esi, [ebp+184]
          // room? 0099EC30 copy : 00433530 grow
004A10B2  test bl, bl
004A10B4  je 004A10F6                     // FALSE skips +172
004A10C4  lea esi, [ebp+172]
004A10F6  mov ecx, [0x13B89FC]
004A10FC  lea eax, [esp+20]
004A1101  call 004B2850                   // both TRUE and FALSE
```

`xrefs.tsv`: `"AddQuest"` `0x01238EA8` at `004A0E7E` /
`004A0EAC` (`fn=0x004A0D90`). `"AddTestQuest"` `0x01238E98`
at `004A0E93` / `004A1128`. **PROVEN**.

`AddTestQuest` `004A113B` never executes `lea … [ebp+184]`.
**PROVEN** not a `+184` writer.

---

## 2. Xrefs to `[world+184]` (CWorld vector only)

`listing-00480000.txt` `lea` / `[reg+184]` that sit on the
`+184/+188/+192` triple (ctor also zeros the three dwords):

| VA | Fn | Op | Role after Leave |
|---|---|---|---|
| `004A68C0` / `+188` / `+192` | `004A67D0` ctor | `mov [esi+184], ebx` | zero |
| `004A08DA` / `004A08E0` | `004A08D0` | load / `lea ecx, [esi+184]` | clear (flag 1 only) |
| `004A1072` / `004A1080` | `004A0D90` | `[ebp+188]` / `lea esi, [ebp+184]` | **write** every `AddQuest` |
| `004A6BE5` / `004A6BEB` / `004A6C03` | `004A6AB0` dtor | walk `[184,188)` `0099EAE0` then `00BFEA14` | teardown only |

No `lea edx, [esi+184]` (the `+172` activate shape is
`0049F247` `lea edx, [esi+172]`). No `004B4260([world+184])`.

`0049EAC0` is `add ecx, 0xAC` (`+172`) then `004B4260`.
**0** `e8.tsv` callers. **Not** `+184` (`0xB8`). **PROVEN**
as a `+172` thunk; **UNREAD** as a no-save site.

### Not CWorld `+184`

| Site | Why excluded |
|---|---|
| `lea ecx, [esp+184]` (many) | stack |
| `004AF130` / `004AF140` get/set `[ecx+184]` | 0 `E8`; sibling get/set `+48` / `+188` / `+192` as raw dwords; **UNREAD** object, not the parse vector walk |
| `004B2422` `inc [eax+184]` | other object (flag/counter) |
| `0049172A` `lea eax, [esi+184]` zero triple | other ctor (also zeros `+172/+196`); not `004A67D0` |

---

## 3. Parallel copy is `manager+44`, not a `+184` walk

`004B2850` (`e8.tsv`: **one** site `004A1101`):

```
004B2850  mov eax, [ecx+52]
004B2854  lea esi, [ecx+44]
          // same 4-byte CString push as +184
```

Argument is `lea eax, [esp+20]` — the token `CString`, **not**
`[ebp+184]`. **PROVEN** sibling store.

`004B4260` first-seen (`0049F24E`) walks the **passed**
vector (`[ebp+0]` / `[ebp+4]`). That pointer is `world+172`.

Per name it calls `004B00C0` → `004B8FF0([manager+44],
[manager+48], name)`: 4-way unrolled `CString` find
(`sar ebx, 4` = 16-byte stride). Hit → `00CB5AD0`.

That **searches** the catalogue for a name already chosen
from `+172` (or later from `004B4A10`). It does **not**
iterate FALSE leftovers. Manager-dtor `004B4930` walks
`[+44,+48)` at teardown only.

---

## 4. What is **in** `+184` after both files

`004A08D0` then file order.

**`FinalAlbion.qst`** (`quests-qst.md`): every `AddQuest`
line. Eight **TRUE** (also `+172`):

1. `Q_SunnyvaleMaster`
2. `ChapterAndSceneManager`
3. `PersonalScriptMain`
4. `PersonalScript_GlobalThings`
5. `NPCDeath`
6. `HeroBoasts`
7. `V_HeroDolls`
8. `CS_PlayCutscene`

First **FALSE** (still `+184`, not `+172`): **`Gameflow`**,
then `GameflowAssistance`, …, **`Q_NewOakValeIntro`**,
`Q_NewOakValeIntro_PreAttack`, …

**`GlobalQuests.qst`** (flag 0 append): all its `AddQuest`
rows. One TRUE (`Global_WatchForHeroDeath` → `+172` too).
No `Gameflow` / no `Q_NewOakValeIntro` in that file.

`Gameflow` **is** in `+184`. **PROVEN**.

---

## 5. First FALSE *use* after Leave (not ini Gameflow)

| Event | Touches FALSE name? | From `+184`? |
|---|---|---|
| `00CD52D0` bind `Gameflow` / `Q_NewOakValeIntro` | PE strings, **before** `004A0D90` | **no** |
| `004A1080` push | write | write, not use |
| `004B2850` | copy of same stack name | **no** (not a `+184` load) |
| `0049F24E` `004B4260([+172])` | TRUE only | **no** |
| `004B00C0` find | looks up the TRUE name in `manager+44` | **no** |
| `user.ini` `ActivateQuest("Gameflow")` | **yes** — excluded by question | **no** (ini literal → `004B4A10`) |
| first type-1 `00893610("Q_NewOakValeIntro")` | **yes** — first remaining FALSE *mention* | **no** (`004AF3C0` active list; `al=0`) |
| `004A6AB0` dtor | frees every catalogue `CString` | teardown walk |

`00893610` (`listing-00880000.txt`): script slot `0x33` →
`008ABED0` → `[eax+60]` → `004AF3C0` on `[0x13B89FC]`.
**0** loads of `[world+184]`. Miss → Gameflow `006E7410`
yield. Does **not** `004B4A10`. **PROVEN**
(`script-gameflow` / `Type1_00CB8220_*`).

So `Q_NewOakValeIntro` is **in** `+184` and is **not used
from** `+184`. First play-path mention is the wait, not an
activate.

---

## 6. Host vs native

| Host | Native after Leave | Class |
|---|---|---|
| `QuestFile` all `AddQuest` names+bool | `+184` catalogue | **PROVEN** names; host has no `+184` vector |
| `life.Quests` = FinalAlbion only | `+184` = both files | **DIVERGE** |
| `004B4260` via `World.InitialQuests` (WLD six) | `+172` nine TRUE | **DIVERGE** (wrong writer; see `qst-first-load`) |
| `ActivateNamedQuest("Gameflow")` from `user.ini` | `00419CE0` / not `+184` | **PROVEN** |
| Invent activate from `+184` FALSE / Oakvale | no walker | **DISPROVEN** |
| Comment “AddQuest/AddTestQuest → world+184” | `AddQuest` only; `AddTestQuest` is `+196` | **PARTIAL** (`EngineLifecycle` note) |

---

## Classifications (short)

1. **`world+184` = every `AddQuest` name (TRUE and FALSE) —
   PROVEN.** Writer `004A1080` is before `test bl`. TRUE
   duplicates into `+172`. `Gameflow` and `Q_NewOakValeIntro`
   are in `+184`.
2. **Who walks `+184` after Leave — PROVEN: nobody on the
   play path.** Ctor / `004A08D0` / parse push / dtor only.
   First use of the tables is `004B4260([world+172])`.
3. **`Gameflow` in `+184` — PROVEN.** FALSE. Started from
   `user.ini`, not from this vector.
4. **`Q_NewOakValeIntro` used from `+184` — DISPROVEN.**
   Catalogue only. First later mention is `00893610` miss.
5. **First FALSE *use* excluding ini Gameflow — PROVEN as
   Gameflow’s Oakvale wait, not as a `+184` walk.** Do not
   invent `ActivateQuest(Q_NewOakValeIntro)` because the
   name is in the catalogue.
