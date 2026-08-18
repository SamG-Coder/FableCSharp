# Factory 0 enqueue: `004BB720` miss / `004B3CE0` stub

Investigation only. No production `src/` edits.

Do **not** invent a second registrar for `ChapterAndSceneManager`
or `NPCDeath`. They have **no** PE string and **no** `00CD52D0`
row. Another registrar is **UNREAD**.

Do **not** start at `S_QNOVI` / `00DBDE40` / `Q_NewOakValeIntro`.
Those names are not on the no-save `world+172` walk.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **DIVERGE**.

Sources: ExeIndex `listing-00480000.txt` (`004B3CE0` / `004B4260` /
`004BB720` / `004B00C0` / `004B0310` / `004AF610` / `004AFA10` /
`004B2850` / `004A10C4` / `0049F24E`);
`listing-00c80000.txt` (`00CB5AD0` / `00CB7900`);
`00-index/strings.tsv` (no `ChapterAndSceneManager` / `NPCDeath`);
`proofs/qst-first-load`, `proofs/script-factory-tables`,
`proofs/fiber-first`;
`EngineLifecycleTests.Init_quests_004B4260_activates_wld_initial_list`.

`qst-first-load` left “does factory 0 allocate a fiber?” as
**UNREAD**. This note closes that from the listing.

---

## Verdict

On `00CB5AD0` miss, `004B4260` still `004BB720`s a 12-byte
record with factory **0**. `004B3CE0` **does** allocate a
52-byte quest *slot* and links it at `[manager+56]`.

It does **not** call a factory, does **not** call `00CB7900`,
does **not** call `00A44740` / `00CDD450`, does **not** call
`00687540`. Factory 0 is **not** a fiber.

| Question | Answer | Class |
|---|---|---|
| Miss still enqueues? | Yes. `je 004B4325` writes `[record+4]=0` then `004BB720` | **PROVEN** |
| What is the 12-byte record? | `CString` name + factory dword + `u8` arg1 + `u8` arg2 | **PROVEN** |
| Does `004B3CE0` alloc a quest object for factory 0? | Yes. `00BFEA1A(52)` stub, name at `+48`, id from `[manager+132]` | **PROVEN** |
| Does it alloc / start a fiber? | No. `00CB7900` is only on `[record+4]!=0` | **PROVEN** |
| Second registrar for those two QST names? | Do not invent. PE / `00CD52D0` absence only | **UNREAD** |

---

## Timeline (no-save, first `004B4260`)

```
004A0D90 AddQuest
  TRUE → world+172          // 004A10C4
  every AddQuest → 004B2850 manager+44
0049F180 Init Quests
  push 1                    // 004B4260 arg2 → record+9
  push 0                    // 004B4260 arg1 → record+8
  lea edx, [esi+172]
  call 004B4260
    for each name:
      "QuestManager: Activate Quest"
      004B00C0              // must be in manager+44
      00CB5AD0 [manager+120]
      004BB720              // hit: factory record; miss: 0
    004B3CE0(queue)
      stride 12 (0x2AAAAAAB)
      [rec+4]==0 → 004B4063 stub 52-byte, no 00CB7900
      [rec+4]!=0 && [0x1375454] → factory / 004AFA10 / 00CB7900
```

`world+172` writer is QST `AddQuest` TRUE, not WLD
`START_INITIAL_QUESTS`. **PROVEN** (`qst-first-load` / `wld-parse`).

Nine TRUE names. Two have **no** factory:

| # | `world+172` | `00CB5AD0` | `004BB720` | `004B3CE0` |
|--:|---|---|---|---|
| 1 | `Q_SunnyvaleMaster` | `00CDD550` | factory | construct + `00CB7900` |
| 2 | `ChapterAndSceneManager` | **0** | factory 0 | **stub 52-byte, no fiber** |
| 3 | `PersonalScriptMain` | `00CDE2F0` | factory | construct + `00CB7900` |
| 4 | `PersonalScript_GlobalThings` | `00CE19A0` | factory | construct + `00CB7900` |
| 5 | `NPCDeath` | **0** | factory 0 | **stub 52-byte, no fiber** |
| 6 | `HeroBoasts` | `00CE6C40` | factory | construct + `00CB7900` |
| 7 | `V_HeroDolls` | `00E98640` | factory | construct + `00CB7900` |
| 8 | `CS_PlayCutscene` | `00F01760` | factory | construct + `00CB7900` |
| 9 | `Global_WatchForHeroDeath` | `00EE90A0` | factory | construct + `00CB7900` |

Rows 2 and 5: no PE string (`strings.tsv` miss), no `00CD52D0`
bind. **PROVEN** absence. Do not invent a second table.

`004B00C0` is not a factory check. `AddQuest` already
`004B2850`’d the name into `[manager+44]`. Find via `004B8FF0`
returns 1 → enqueue. Those two names **do** pass. **PROVEN**.

Host `InitCharactersAndQuests` walks WLD six + ini `Gameflow`
and never `00CB5AD0`s the two QST-only names. **DIVERGE**.

---

## 1. `004B4260` miss still calls `004BB720`

`listing-00480000.txt` `004B42E4`–`004B4386`:

```
ecx = [this+120]
push name
call 00CB5AD0
test edi, edi
je 004B4325                 // MISS
  copy name → record at esp+36
  [record+4] = edi          // factory record
  [record+8] = arg1
  [record+9] = arg2
  call 004BB720
jmp consume
004B4325:                   // MISS
  copy name → record at esp+48
  [record+4] = 0            // factory 0
  [record+8] = arg1
  [record+9] = arg2
  call 004BB720
…
lea edx, [esp+24]           // queue vector
call 004B3CE0               // once, after the loop
```

`00CB5AD0` (`listing-00c80000.txt`):

```
00CB65D0 search [this+4]
hit  → lea eax, [edi+4]     // record after the name
miss → xor eax, eax
ret 4
```

No alloc. Miss is **0**, not a fallback factory. **PROVEN**.

`004B4260` `ret 12`. First no-save site `0049F243`–`0049F24E`:

```
push 1
push 0
lea edx, [esi+172]
push edx
call 004B4260
```

So this walk: arg1 = **0**, arg2 = **1**.

---

## 2. The 12-byte enqueue record

`004BB720` (`listing-00480000.txt`):

```
esi = [vec+4]               // end
if esi != [vec+8] && esi:
  0099EC30(esi, src)        // CString copy to +0
  [esi+4] = [src+4]         // dword
  [esi+8] = [src+8]         // byte
  [esi+9] = [src+9]         // byte
  [vec+4] += 12
else
  004BB2E0 grow, same 12
ret 4
```

`004B3CE0` counts with `0x2AAAAAAB` (`(end-begin)/12`) and
`add …, 12` on both loops. After `004B3CE0`, `004B4260`
destroys the queue with `add esi, 12` / `0099EAE0` / `00BFEA14`.

| Off | Type | Init Quests (`0049F24E`) | Who writes |
|---:|---|---|---|
| `+0` | `CString` (4) | quest name | `0099EC30` from `world+172[i]` |
| `+4` | `void*` | factory record, or **0** | `00CB5AD0` eax / immediate 0 |
| `+8` | `u8` | **0** | `004B4260` arg1 |
| `+9` | `u8` | **1** | `004B4260` arg2 |

`+10`/`+11` unused padding. **PROVEN**.

`004B3CE0` reads `+0` (name), `+4` (factory), `+9` (first-loop
side list; stub stores it at `[obj+37]`). `+8` is copied by
`004BB720` and not read in `004B3CE0`. Later readers of `+8`
are **UNREAD**.

`00892E80` / `004B4A10` (ini `ActivateQuest`) push `1,1` into
`004B4A10`, which rebuilds a one-name vector and calls
`004B4260`. That is **not** the first `world+172` walk.

`004BB780` is a different 12-byte push (three `CString`s).
`004B4260` does not call it.

---

## 3. `004B3CE0` factory 0 = stub object, not a fiber

Two loops over the same queue.

### First loop (`004B3D20`)

`004AF610` (`[manager+56]` already has this name?) → skip.
`004B9370` on `[manager+156]`. If `[rec+9]!=0` (this walk: 1),
push the name into a temp pointer vector and `004AC380`
`[manager+156]`. Then optional `0x13CAA68` / `009F0570` work.

`009F0570` returning 0 only skips the rest of **this**
iteration. It does not skip the second loop. Purpose of
`0x13CAA68` is **UNREAD**.

### Second loop (`004B3E82`) — the split

```
004AF610                    // already in +56? skip
004B97D0 [manager+12]
cmp [rec+4], 0
je 004B4063                 // FACTORY 0 — before [0x1375454]
cmp [0x1375454], 0
je 004B4063                 // BSS-0 stub; PE .data is 1
; factory != 0:
  [eax+16]==0 → 004AFA10 reuse SharedRun
  else call [factory+4]     // construct
  call [factory+0]          // run
  00BFEA1A(52) + 004B0310   // live quest object
  link [manager+56]
  004BB270 +156
  00CB7900                  // vtbl+12 then jmp vtbl+4 Main
  00687540(55,50)
```

Factory 0 never reaches `[0x1375454]`. The `.data`=1 gate is
only for a **non-zero** factory.

### Stub at `004B4063`

```
id = [manager+132]++
obj = 00BFEA1A(52)
[obj+0]  = id
[obj+4 … +24] = 0           // no factory / run / wrappers
[obj+36] = 1
[obj+37] = [rec+9]           // 1 on this walk
[obj+40] = 0
[obj+44] = 0
0099EC30(obj+48, rec)        // name only
wrapper 00BFEA1A(12): {1, 004BAEF0, obj}
16-byte node → [manager+56]  // same list as live quests
004B9D50 / 004B9D00 / 004B9C10   // erase +156 range
; no 00CB7900
; no 00687540
; no 004B0310
dec wrapper; list still holds ref (inc then dec → 1)
```

`004B0310` is the factory-hit 52-byte ctor (stores factory /
run / name). Same size, different fill. Stub is a **slot**
with a name and an id, not a running quest.

`004BAEF0` is the wrapper dtor (`004B9B90` + `00BFE9BC`).
It is **not** called on the success path while `[manager+56]`
holds the ref.

`00CB7900`:

```
call [vtbl+12]
jmp [vtbl+4]                // Main → 00CDD450 → 00A44740
```

No site on the stub arm. First *script* fiber remains
`Q_SunnyvaleMaster` `00CDD380` / `00CDD450` (`fiber-first`).
Factory 0 is not that. **PROVEN**.

---

## 4. What this is not

| Claim | Class |
|---|---|
| Miss drops the name (no `004BB720`) | **DISPROVEN** |
| Factory 0 starts `00CB7900` / `00A44740` | **DISPROVEN** |
| Factory 0 is skipped because `[0x1375454]` | **DISPROVEN** (checked only after `+4!=0`) |
| `00CB5AD0` miss invents a factory | **DISPROVEN** (eax=0) |
| `ChapterAndSceneManager` / `NPCDeath` live in `00CD52D0` | **DISPROVEN** (no PE string, no bind) |
| A second registrar fills them | **UNREAD** — do not invent |
| Those two names fail `004B00C0` | **DISPROVEN** (`004B2850` already inserted) |
| Host activates them on New Game | **DIVERGE** (WLD six + `Gameflow` only) |
| `Global_WatchForHeroDeath` is factory 0 | **DISPROVEN** (`00EE90A0` exists; host omit is a different DIVERGE) |
| `004BB780` is this enqueue | **DISPROVEN** |

---

## Classifications (short)

1. **`004B4260` miss → `004BB720` factory 0 — PROVEN.**
2. **12-byte record = name + factory + arg1 + arg2 — PROVEN.**
   Init Quests: factory 0 / 0 / 1 for the two QST-only names.
3. **`004B3CE0` factory 0 allocates a 52-byte named slot on
   `[manager+56]` — PROVEN.** Not a fiber. No `00CB7900`.
4. **Second registrar for `ChapterAndSceneManager` / `NPCDeath`
   — UNREAD.** Absence from PE / `00CD52D0` is **PROVEN**.
   Do not invent a table to “fix” the miss.
