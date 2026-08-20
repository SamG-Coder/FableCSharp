# First type-1 `00CB8220` / `00CB7C40` after Leave

Investigation only. No production `src/` edits.

Do **not** treat the first walk as Gameflow.
Do **not** collapse `004B4490` `[QM+56]` into one
`00CB7C40`. Do **not** invent
`ActivateQuest("Q_NewOakValeIntro")`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Sources: `listing-00c80000.txt` `00CB8220` /
`00CB7C40` / `00CB7950` / `00CB8170` / `00CB7E50` /
`00CB8110` / `00CB7900`;
`listing-00480000.txt` `004B4490` / `004B3CE0` /
`004B0310` / `004B4260`;
`listing-00cc0000.txt` `00CDD360` / `00CDD380` /
`00CDD430` / `00CDD440` / `00CDD450` / `00CDD550` /
`00CDE380` / `00CDDCB0` / `00CE1A30` / `00CE1AF0` /
`00CE75B0` / `00CE7640` / `00CE7670`;
`listing-00ec0000.txt` `00EE90A0` / `00EE9120`;
`listing-00e80000.txt` `00E98640` Main;
`listing-00f00000.txt` `00F01760` / `00F017F0` /
`00F35A00`;
`listing-006c0000.txt` `006E7410`;
`proofs/gameflow-main-first-tick/README.md`,
`proofs/fiber-yield-first/README.md`,
`proofs/world-plus172-activate/README.md`,
`proofs/ini-activate-quest/README.md`,
`proofs/factory0-enqueue/README.md`;
`docs/runtime/FORWARD_TREE.md` §§6–11;
`EngineLifecycleTests.Type1_00CB8220_*`.

---

## Verdict

**First `00CB8220` / `00CB7C40` after Leave is
`Q_SunnyvaleMaster`, not Gameflow.**

`00CB8220` is not a global watcher walk. It is
`00CB7C40` then `jmp 00CB8170` on **one** factory
object. The only `.text` caller is `004B453E`
inside type-1 `004B4490`. That pump walks
`[QM+56]` (constructed slots, tail-insert) and
calls `00CB8220([slot+8])` when `[slot+8] != 0`.
`[slot+8]` is the factory (`004B0310`).

`00CB7C40` walks **`[this+4]`** — the factory’s
circular watcher list, not `[QM+56]`. Head is
Sunnyvale `"Main"` (`00CDD380` /
`00CB7E50`). First `00CB7950` target is that
watcher. First body is `00CDD430` → `00CDD440`
→ factory `vtbl+8` **`00CDD360`**. **PROVEN**.

`user.ini` `ActivateQuest("Gameflow")` already
ran. Gameflow is the **tail** of `[QM+56]`, a
**later** `00CB8220` on Gameflow’s own
`[this+4]`. It is **not** on Sunnyvale’s
`[this+4]`. **PROVEN**.

| Question | Answer | Class |
|---|---|---|
| First `00CB8220` after Leave? | first type-1 `004B4490`, first `[QM+56]` slot with `[+8]!=0` = Sunnyvale factory | **PROVEN** |
| First x86 of `00CB8220`? | `push esi` | **PROVEN** |
| First `00CB7C40`? | `00CB8223`; `this` = Sunnyvale factory | **PROVEN** |
| First x86 of `00CB7C40`? | `push ebx` | **PROVEN** |
| List walked by `00CB7C40`? | `[this+4]` circular; `[node+8]` = watcher | **PROVEN** |
| Head of that list? | Sunnyvale `"Main"` → `00CDD360` | **PROVEN** |
| Is Gameflow that head? | no | **DISPROVEN** |
| When vs `user.ini` Gameflow attach? | attach **before** first type-1; first walk is **after** | **PROVEN** |
| First `00CB7950` target? | Sunnyvale Main watcher (`+41=0` → `vtbl+4` `00A44880`) | **PROVEN** |
| `00CB8170` on that first call? | `[this+8]` vector empty | **PROVEN** |
| One `00CB7C40` walks every quest? | no; one per factory | **DISPROVEN** |

---

## Timeline (no-save New Game)

```
0042F2A2 Leave frontend
0042F491 Init Game → 004184BD
  004A1840 Load Quests
    world+172 = QST AddQuest TRUE (9 names)
  0049F24E 004B4260([world+172])
    00CB5AD0 / 004BB720 / 004B3CE0
    [QM+56] tail-insert (factory 0 stubs too)
      1 Q_SunnyvaleMaster     +8=00CDD550 obj
      2 ChapterAndSceneManager +8=0          // skip later
      3 PersonalScriptMain
      4 PersonalScript_GlobalThings
      5 NPCDeath               +8=0
      6 HeroBoasts
      7 V_HeroDolls
      8 CS_PlayCutscene
      9 Global_WatchForHeroDeath
    each factory:
      00CB8110  [+4]=empty sentinel; [+8]=0
      00CB7900  vtbl+12 then jmp vtbl+4 Main
      00CDD380 / …  00CDD450("Main") 00CB7E50
  00418969 user.ini 009EC890
    ActivateQuest("Gameflow")                // AFTER +172
      00419CE0 → 00892E80 → 004B4A10
      004B4260 → 00CB5AD0 → 004B3CE0
      00CE6CF0 seed; 00CE75B0 Main 00CB7E50
      [QM+56] tail = Gameflow                // still construct
      GameflowYieldQuest == null
004189C2 first pumps
  first type-1 004A5A40 → 004B4490 [0x13B89FC]
    walk [esi+56]
      [slot+8]==0 → skip 00CB8220
      [slot+8]!=0 → ecx=[slot+8]; call 00CB8220
        FIRST: Sunnyvale factory
          00CB8220  push esi
          00CB8223  call 00CB7C40            // FIRST 00CB7C40
            [ebx+4] head = Main watcher
            00CB7950(Main)                   // FIRST 00CB7950
              +40=0 00F35A00=1 +41=0
              vtbl+4 00A44880
              00A446A0 vtbl+16 00CDD430
              00CDD440 jmp [factory.vtbl+8]
              00CDD360                       // not Gameflow
                [factory+64].vtbl+28 yield
                00CB7940 [factory+44]+5 == 0
                park 009D8650; node kept
          jmp 00CB8170  [+8]=0 empty
      … later slots …
      last: Gameflow factory 00CB8220
        [this+4] head = Gameflow Main
        00CB7950 → 00CE7640 → 00CE7670
        attach Core / Barrow at tail
        state 0 00893610 Q_NewOakValeIntro → 0
```

`00DABAC0` / `00DBDE40` / `00CBFB7D` /
`Q_NewOakValeIntro` construct are **not** on
this list. **PROVEN**.

Construct vs first walk is locked by
`Gameflow_00CE75B0_*` (`GameflowYieldQuest==null`
after `EnterGame`) then `Type1_00CB8220_*`
(first type-1 `Pump(0.1f)`). **PROVEN**.

---

## 1. Dump: `00CB8220` / `00CB7C40` / `00CB7950`

`listing-00c80000.txt`.

### `00CB8220` — two-insn trampoline

```
00CB8220  push esi
00CB8221  mov esi, ecx
00CB8223  call 00CB7C40
00CB8228  mov ecx, esi
00CB822A  pop esi
00CB822B  jmp 00CB8170
```

Next prologue is `00CB8230` (ExeIndex title
“NOVI name register”). That is a **different**
function. **PROVEN**.

Grep of `listing-*.txt`: **one** `call 00CB8220`
(`004B453E`). **One** `call 00CB7C40`
(`00CB8223`). Not `004B4A10` / `004B4260` /
`00CE75B0` / `00CB7900`. **PROVEN**.

### `00CB7C40` — walk `[this+4]`

```
00CB7C40  push ebx
00CB7C41  mov ebx, ecx
00CB7C43  mov eax, [ebx+4]        // sentinel
00CB7C47  mov esi, [eax]          // first node
00CB7C49  cmp esi, eax
00CB7C4B  je 00CB7CA8             // empty → ret
00CB7C51  mov eax, [esi+8]        // watcher
00CB7C54  push eax
00CB7C55  mov ecx, ebx
00CB7C57  call 00CB7950
00CB7C5C  test al, al
00CB7C5E  je 00CB7C9F             // al=0 keep
          … unlink / 00BFEA14 …
00CB7C9F  mov esi, [esi]
00CB7CA1  cmp esi, [ebx+4]
00CB7CA4  jne 00CB7C51
00CB7CAA  ret
```

Node: `[+0]` next, `[+4]` prev, `[+8]` watcher.
Same 16-byte shape `00CB7E50` inserts.

`00CB7E50` tail-inserts before the sentinel
(`[eax]=sentinel`, `[sentinel+4]=node`). First
insert is head (`[sentinel]`). **PROVEN**.

### `00CB7950` — dispatch one watcher

```
00CB7950  esi = arg; edi = ecx          // factory
          [edi+44] = esi
          if [esi+40] != 0 → al=0 keep
          00F35A00(esi)                 // [esi+44]==0 → al=1
          if al==0: [+5]=1; vtbl+20
          else if [esi+41] != 0:
            vtbl+24; 00A4B200; vtbl+12; [+41]=0; bl=1
          else:
            call [vtbl+4]               // FIRST-SEEN 00A44880
            00F35A00
          if [esi+4]==0 && bl==0 → al=1 remove
          else if [esi+5]==0 → al=0 keep
          else 00A4B200; al=1 remove
```

`00F35A00`: `[ecx+44]==0` → `al=1`. Watcher
ctor `00CDD450` zeros `+40` / `+41` / `+44`.
First-seen takes `vtbl+4`. **PROVEN**.

First-seen yield parks inside `00A446A0`
(`[+4]=1`, `[+5]` still 0) → `00CB79EA`
`al=0` → `00CB7C40` **keeps** the node.
**PROVEN**.

`00CB8170` walks a **vector** at `[this+8]`
(16-byte stride). `00CB8110` zeros `+8/+12/+16`.
First-seen empty. **PROVEN**.

---

## 2. Two lists — do not collapse

| Walk | Object | Slot | First no-save head |
|---|---|---|---|
| `004B4490` | QuestManager `[0x13B89FC]` | `[+56]` circular slots | `Q_SunnyvaleMaster` 52-byte slot |
| `00CB7C40` | **that slot’s factory** | `[factory+4]` watchers | Sunnyvale `"Main"` |
| `00CB8170` | same factory | `[factory+8]` vector | empty |

`004B4490` (`listing-00480000.txt`):

```
esi = QM
eax = [esi+56]
edi = [eax]
… lea ebx, [edi+8] …          // earlier +60 drain
eax = [esi+56]
edi = [eax]
xor ebx, ebx
cmp edi, eax → empty skip
004B4522:
  eax = [edi+8]               // 52-byte slot
  cmp [eax+8], ebx
  je  004B4549                // factory 0 / no factory
  ecx = [eax+8]               // FACTORY
  call 00CB8220
  edi = [edi]
  cmp edi, [esi+56]
  jne 004B4522
```

`004B0310` stores the factory instance at
`[slot+8]`. Factory-0 stub (`factory0-enqueue`)
writes `[slot+8]=0`. Those nodes stay on
`[QM+56]` and **never** enter `00CB8220`.
**PROVEN**.

`00CB8110` (factory base, `00CDD550` /
`00F01760` / `00EE90A0`):

```
[+4] = 16-byte sentinel  [p]=p, [p+4]=p
[+8] = [+12] = [+16] = 0
[+44] = 0
```

`00CDD550` then `[+64]=run` (`00CDBD20`),
`[+68]=ebx`, vtbl `012C2F64`. Size 72.
**PROVEN**.

`gameflow-main-first-tick` “one `00CB7C40`,
head Sunnyvale, tail Gameflow” names the
**`[QM+56]`** order, not the first
`[this+4]`. First `[this+4]` has **one**
node. **PROVEN** correction.

---

## 3. When relative to `user.ini` Gameflow

`ini-activate-quest`: after Leave, **one** ini
quest — `user.ini` `ActivateQuest("Gameflow")`
via `00419CE0` at `00418969`. That is
**after** `0049F24E` `004B4260([world+172])`
and **before** `004189C2` first pumps.

Gameflow is `AddQuest(..., FALSE)` — not in
`world+172`. Second `004B4260` tail-inserts it
on `[QM+56]`. `00CE75B0` attaches `"Main"` to
**Gameflow** `[+4]`, not Sunnyvale `[+4]`.
**PROVEN** (`gameflow-main-first-tick`).

So at first type-1:

- Gameflow **exists** on `[QM+56]` already.
- First `00CB8220` still hits Sunnyvale.
- Gameflow’s `00CB8220` is the **last** factory
  on that same `004B4490`.
- First `00CE7670` (Oakvale **name** wait) is
  inside that last call, not the first.

`00CB7C40` at activate / `00CE75B0` —
**DISPROVEN** (no `E8`).

---

## 4. Exact first-seen lists

`world+172` (`world-plus172-activate` /
`qst-first-load`):

| `[QM+56]` # | Name | `[slot+8]` | First `00CB8220`? | First `[factory+4]` |
|--:|---|---|---|---|
| 1 | `Q_SunnyvaleMaster` | `00CDD550` 72 | **yes — first** | `"Main"` `00CDD380` |
| 2 | `ChapterAndSceneManager` | 0 | no | — |
| 3 | `PersonalScriptMain` | `00CDE2F0` | later | `"Main"` `00CDE380` |
| 4 | `PersonalScript_GlobalThings` | `00CE19A0` | later | `"Main"` (same shape) |
| 5 | `NPCDeath` | 0 | no | — |
| 6 | `HeroBoasts` | `00CE6C40` | later | `"Main"` `00CE1A30` |
| 7 | `V_HeroDolls` | `00E98640` | later | `"Main"` `00E9871C` |
| 8 | `CS_PlayCutscene` | `00F01760` | later | `"Main"` `00F017F0` |
| 9 | `Global_WatchForHeroDeath` | `00EE90A0` | later | `"Main"` `00EE9120` |
| 10 | `Gameflow` (ini, not `+172`) | `00CEF950` | **last** | `"Main"` `00CE75B0` |

Every live Main is `00CDD450("Main")` /
`+52=00CDD440` / `+56=factory` / `00CB7E50`.
**PROVEN** (Sunnyvale, Personal, HeroDolls,
PlayCutscene, Global, Gameflow listings).

### First `00CB7950` targets (first type-1)

Order is `004B4490` × per-factory `00CB7C40`.
`+41=0` every first-seen.

| # | `00CB7950` arg | Reaches | Yield |
|--:|---|---|---|
| 1 | Sunnyvale Main | `00CDD430` → **`00CDD360`** | `[+64].vtbl+28` / `00CB7940` |
| 2 | PersonalScriptMain Main | `00CDD440` → **`00CDDCB0`** | `vtbl+72` empty thing list |
| 3 | PersonalScript_GlobalThings Main | same `00CDDCB0` | same |
| 4 | HeroBoasts Main | **`00CE1AF0`** | empty → `00CE1C24` |
| 5 | V_HeroDolls Main | `00CDD440` → factory `vtbl+8` | **UNREAD** body |
| 6 | CS_PlayCutscene Main | empty factory | no `CCutsceneDef` |
| 7 | `Global_WatchForHeroDeath` Main | `00EE91B0` → factory `vtbl+8` `00EE91E0` | **PARTIAL** |
| 8 | Gameflow Main | `00CE7640` → **`00CE7670`** | `00893610` miss |
| 9 | `CoreQuestReminder` | **`00CEF3B0`** | `[+72]=0` |
| 10 | `CheckBarrowFieldsGuards` | **`00CEF550`** | trader miss |

Rows 9–10 are attached **during** row 8
(`00CE7670` `00CDD450` / `00CB7E50` tail).
Same Gameflow `00CB7C40`, not Sunnyvale’s.
**PROVEN** (`gameflow-main-first-tick`).

**First `00CB7950` after Leave is row 1.**
**PROVEN**.

Native `00CB8220` count on this tick: **8**
(seven `+172` factories + Gameflow). Host
`QuestPumpWalked==12` still notes factory-0
names as `00CB7950` and counts Gameflow’s
three watchers. **DIVERGE** count; first
name still Sunnyvale.

---

## 5. `00CDD360` — first body, now dumped

`listing-00cc0000.txt` (was **UNREAD** in
`fiber-yield-first`):

```
00CDD360  push esi
00CDD361  mov esi, ecx                 // factory
00CDD363  mov ecx, [esi+64]            // SharedRun
00CDD366  mov eax, [ecx]
00CDD368  call [eax+28]
00CDD36B  mov ecx, esi
00CDD36D  call 00CB7940                // [factory+44]+5
00CDD372  test al, al
00CDD374  je 00CDD363
00CDD376  pop esi
00CDD377  ret
```

`00CDD550` writes `[factory+64]=run`.
Sunnyvale allocates `00CDBD20`. **PROVEN**.

`00CB7950` already stored
`[factory+44]=Main` before `vtbl+4`.
`00CB7940` is **not** “hero exists” here —
it is `[current watcher+5]`. First-seen `+5=0`
→ loop. **PROVEN**.

How `00CDD360` is reached (not a
`00CB7950` vtbl slot):

```
00A446A0  first pass [watcher.vtbl+16]
00CDD430  ecx=[watcher+56]=factory
          call [watcher+52]            // 00CDD440
00CDD440  mov eax, [ecx]; jmp [eax+8]  // factory vtbl+8
          then [watcher+5]=1           // after return
```

Same shape as Gameflow `00CE7640` →
`00CE7670`. Collapsing `00CDD360` onto
`00CE75B0` / `00CE7670` is **DISPROVEN**.

`[SharedRun.vtbl+28]` concrete VA is
**UNREAD** (no `012C2748` rdata in the
listings). Host / FORWARD_TREE name
`006E7410`. That helper is
`[0x13D2838].vtbl+8` then optional
`0049D870`. **PARTIAL** slot; **PROVEN**
that the first Sunnyvale work is this
`vtbl+28` loop, not Gameflow state 0.

Wait **predicate** (which flag / thing
`00CDD360` wants) is still **UNREAD**.
Do not invent one.

---

## 6. Host

`EngineLifecycle.PumpQuestList` notes one
`00CB8220 00CB7C40 then 00CB8170` then
`TickNamedQuestMain` in activate order
(skipping the Gameflow name) and then
Gameflow Main / Core / Barrow.

| Host | Native | Class |
|---|---|---|
| First note `00CDD360` | first `00CB8220` | **PROVEN** |
| `WorldPlus172` + ini Gameflow | `[QM+56]` order | **PROVEN** names |
| One trampoline note for the whole pump | one `00CB8220` per factory | **DIVERGE** shape |
| `QuestPumpWalked==12` | 8 `00CB8220` / 10 `00CB7950` | **DIVERGE** |
| factory-0 still `00CB7950` note | `[slot+8]==0` skip | **DIVERGE** |
| `GameflowWaitQuest` as first walk head | later last-factory `00CE7670` | **LEFTOVER** |
| `Runtime.Update` / `Scheduler.Pump` | unused on Leave `Pump()` | **LEFTOVER** |

Comment at `QuestManagerPumpFn` still says
`00CB8220` body **UNREAD**. This dump
closes that. No `src/` edit here.

---

## Classifications (short)

1. **First `00CB8220` / `00CB7C40` after
   Leave — PROVEN Sunnyvale factory, first
   type-1 `004B4490`.** Head of `[this+4]`
   is WLD / QST `Q_SunnyvaleMaster` Main
   (`00CDD360`), not Gameflow.
2. **List walked by `00CB7C40` — PROVEN
   `[this+4]` watchers.** Outer constructed
   order is `[QM+56]`. Factory 0 skips.
3. **Vs `user.ini` Gameflow — PROVEN
   attach already done; Gameflow is last
   `00CB8220` on the same pump.** First
   `00CE75B0` is construct, not this walk.
4. **First `00CB7950` — PROVEN Sunnyvale
   Main, `+41=0` → `00A44880` →
   `00CDD430` / `00CDD360`.** Later
   targets are the other factory Mains,
   then Gameflow Main + Core + Barrow.
5. **One global `00CB7C40` of every
   watcher — DISPROVEN.**
6. **`00CDD360` wait predicate /
   SharedRun `vtbl+28` VA — UNREAD /
   PARTIAL.** Dump exists; slot not in
   rdata here.
