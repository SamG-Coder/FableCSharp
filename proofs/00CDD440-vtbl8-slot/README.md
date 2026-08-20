# When is the yielded-quest vtbl slot set to `00DABAC0` on no-save?

Investigation only. No production `src/` edits.

Do **not** invent a `call [vtbl+8]` that
resumes a yielded `00DABAC0`. Do **not**
invent `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** treat the 24-byte walk
`00CB6EA0` as the slot-2 dispatcher.
Do **not** collapse Gameflow
`00CDD440` `jmp [vtbl+8]` onto
`S_QNOVI` `00DABAC0`.

Question: Main watcher thunk `00CDD440`
`jmp [vtbl+8]` is how slot 2
`00DABAC0` is first entered — **after**
construct. On **no-save**, when (if ever)
is that yielded-quest vtbl slot set to
`00DABAC0`?

Status words: **PROVEN** / **PARTIAL** /
**UNREAD** / **DISPROVEN** / **LEFTOVER** /
**MATCH**.

Authority:

- `listing-00cc0000.txt` `00CDD430` /
  `00CDD440` / `00CDD450` / `00CD6E27` /
  `00CE75B0` / `00CE7640` / `00CE7670`
- `listing-00c80000.txt` `00CB5C70` /
  `00CB5C90` / `00CB5D80` / `00CB6CE0` /
  `00CB6EA0` / `00CB70E0` / `00CB7210` /
  `00CB7780` / `00CB8110` / `00CBD4C0`
- `listing-00d80000.txt` `00DAAC00` /
  `00DAACE0` / `00DAAD70` / `00DABAC0` /
  `00DBEF70`
- ExeIndex `calls-s-qnovi-run-00dabac0`
  (0 `E8`), `calls-script-walk-00cb6ea0`
  (2 hits, both in `00CB70E0`),
  `calldisp-vtbl8-slot2-scripts` (1 hit
  `00CB89C8`, not resume),
  `native-sqnovi.md`
- `assembly/exe/00-index/vtbl.tsv`
  `0x012D7A28` / `0x012C1648` /
  `0x012C3FA4` / `0x012C44B4`
- PARITY 0b leftovers; “Who activates
  `Q_NewOakValeIntro`”
- Siblings `proofs/sqnovi-yield-resume`,
  `proofs/intro-fiber-attackover`,
  `proofs/host-yield-first-tick`,
  `proofs/gameflow-main-first-tick`,
  `proofs/00DBDE40-host-gap`
- Host `EngineLifecycle.SqnoviMainWatcherThunk`
  / `SqnoviReentersRunAfterYield=false`;
  `No_save_does_not_activate_Q_NewOakValeIntro`

---

## Verdict

**Never on no-save.** The dword
`00DABAC0` lives in rdata vtbl
`012D7A28` slot 2. A live object only
indexes that slot after
`00DAAC16 mov [esi], 0x12D7A28`,
which is factory `00DBEF70` after
`00CB5AD0("Q_NewOakValeIntro")`.
That construct is **not** on the
no-save walk.

The no-save yielded quest is
**Gameflow**. First type-1
`00CDD440` `jmp [eax+8]` dest is
**`00CE7670`**, not `00DABAC0`.

The 24-byte list walk `00CB6EA0`
copies records and `E8 00CB6CE0`.
It does **not** `call [vtbl+8]` and
does **not** write `00DABAC0`. First
`00CB5D80` skips `00CB7780`
(`[esi+17]==0`), so `00CB6EA0` does
**not** run on no-save Init Scripts.

The exact `call [vtbl+8]` site that
**resumes** a yielded `00DABAC0` stays
**UNREAD** (PARITY 0b). Do not invent
a fiber resume as that site.

| Question | Answer | Class |
|---|---|---|
| `00CDD440` body? | `mov eax,[ecx]; jmp [eax+8]` | **PROVEN** |
| No-save first `00CDD440` dest? | Gameflow `vtbl+8` = `00CE7670` | **PROVEN** |
| Does that dest become `00DABAC0` on no-save? | **No** | **DISPROVEN** |
| rdata slot 2 of `012D7A28`? | `00DABAC0` (PE-constant) | **PROVEN** rdata; **not** a no-save store |
| Live `[this]=012D7A28`? | `00DAAC16` inside `00DAAC00` | **PROVEN** later ctor |
| Who `E8`s `00DAAC00`? | only `00DBEF8B` in factory `00DBEF70` | **PROVEN** |
| Does no-save run `00DBEF70` / `00DAAC00`? | **No.** Activator unread; name not in `world+172` / Gameflow | **DISPROVEN** first-seen |
| Bind `00CD6E27` writes `00DABAC0`? | **No.** `00CB5C90` stores factory `00DBEF70` | **DISPROVEN** as slot-2 write |
| `00CB6EA0` `call [vtbl+8]`? | **No.** copy + `E8 00CB6CE0` | **DISPROVEN** |
| `00CB6EA0` on first `00CB5D80`? | skipped (`[esi+17]==0` → no `00CB7780`) | **DISPROVEN** first-seen |
| First enter of `00DABAC0`? | later `00DAAD76` → `00CDD440` `jmp [S_QNOVI.vtbl+8]` | **PROVEN** leftover vs no-save |
| `.text` `E8` of `00DABAC0`? | **0** | **PROVEN** |
| `call [vtbl+8]` that **resumes** a yielded `00DABAC0`? | — | **UNREAD** |

---

## Timeline (no-save New Game)

```
00416953 Loading world
  004A6550 Init Scripts
    00CB5C70 list; [+17]=0
    00CB5D80 Registering Scripts
      00CD52D0 fill
        00CD6E27 00CB5C90 bind
          Q_NewOakValeIntro / S_QNOVI / factory 00DBEF70
          00CB7210 store 24-byte record
          // NOT 00DABAC0
      [esi+17]==0
        skip 00CB7780                    // 00CB6EA0 not entered
004B4260 WLD initial
  Q_SunnyvaleMaster … CS_PlayCutscene    // not Oakvale
user.ini ActivateQuest("Gameflow")
  00CEF950 factory vtbl 012C3FA4
    [2] = 00CE7670                       // Gameflow slot 2
  00CB7900 jmp [vtbl+4]
  00CE75B0 attach "Main"
    00CDD450; vtbl 012C44B4
    +52 = 00CDD440
    +56 = Gameflow
    ret                                  // no 00CDD440 yet
004189C2 first pumps
  dummy inner: 0041674A=0 skip 00CB8220
  first type-1 00CB8220
    head Sunnyvale 00CDD440 → 00CDD360
    tail Gameflow
      00CB7950 +41=0 → 00A44880
      00A446A0 [watcher+16] 00CE7640
        call [esi+52]                    // 00CDD440
        00CDD440 jmp [Gameflow.vtbl+8]
                 00CE7670                // NOT 00DABAC0
          00893610 "Q_NewOakValeIntro" → 0
          006E7410 yield
```

`00DAAC00` / `012D7A28` / `00DABAC0` /
`00DBDE40` are **not** on this list.
**PROVEN**.

After a **proven** later activate
(activator still **UNREAD** on no-save):

```
00DBEF70 alloc 0x10C
  00DAAC00
    00CB8110 vtbl 012C1648               // slot 2 = 00CBD4C0 ret
    00DAAC16 mov [esi], 0x12D7A28        // LIVE slot 2 = 00DABAC0
00DAACE0 Main
  +52 = 00CDD440
  +56 = S_QNOVI
00DAAD70 [watcher+16]
  call [esi+52]                          // FIRST 00CDD440 of this object
  00CDD440 jmp [S_QNOVI.vtbl+8]
           00DABAC0                      // FIRST enter; 0 E8
```

That install is leftover vs no-save
first Present. **PROVEN** as ctor;
**DISPROVEN** as this walk.

---

## 1. `00CDD440` is a factory `jmp [vtbl+8]`

`listing-00cc0000.txt` int3-bounded
`00CDD430`–`00CDD444`:

```
00CDD430  push esi
00CDD431  mov esi, ecx
00CDD433  mov ecx, [esi+56]           // factory
00CDD436  call [esi+52]               // callback
00CDD439  mov [esi+5], 1
00CDD43E  ret

00CDD440  mov eax, [ecx]
00CDD442  jmp [eax+8]                 // factory vtbl+8
```

`ecx` is the **factory**, not the
watcher. Watcher `+52` holds this
thunk; watcher `+56` is the factory
`this`. **PROVEN**.

No-save Gameflow (`00CE75B0`):

```
00CE75F1  mov [esi], 0x12C44B4
00CE75F7  mov [esi+52], 0xCDD440
00CE75FE  mov [esi+56], edi           // Gameflow
```

Watcher vtbl `012C44B4` slot 2 is
`00A44840` (fiber wait). Factory
vtbl `012C3FA4` slot 2 is
**`00CE7670`**. First type-1
`00CE7640` `call [esi+52]` therefore
enters `00CE7670`. **PROVEN**.
**DISPROVEN** as `00DABAC0`.

Later S_QNOVI (`00DAACE0`):

```
00DAAD21  mov [esi], 0x12D7A3C
00DAAD27  mov [esi+52], 0xCDD440
00DAAD2E  mov [esi+56], edi           // S_QNOVI
```

Clone `00DAAD70` (watcher `+16`):

```
00DAAD70  mov ecx, [esi+56]
00DAAD76  call [esi+52]               // 00CDD440
00DAAD79  mov [esi+5], 1
```

Same thunk, different `+56`. Dest
is then `012D7A28[+8] = 00DABAC0`.
**PROVEN** leftover vs no-save.

---

## 2. Slot `00DABAC0` is rdata, then a ctor overwrite

`vtbl.tsv`:

| vtbl | slot 2 (`+8`) | When live |
|---|---|---|
| `012C1648` | `00CBD4C0` (`ret`) | base `00CB8110` |
| `012D7A28` | **`00DABAC0`** | `00DAAC16` |
| `012C3FA4` | `00CE7670` | Gameflow factory (no-save) |
| `012C44B4` | `00A44840` | Gameflow **watcher** (not `00CDD440` `ecx`) |

`00DAAC00` (`listing-00d80000.txt`):

```
00DAAC00  push esi
00DAAC03  call 00CB8110               // [esi]=012C1648
00DAAC10  mov [esi+64], eax
00DAAC16  mov [esi], 0x12D7A28        // slot 2 becomes 00DABAC0
```

No `.text` `mov imm, 00DABAC0` into
the object. The ctor writes the
**vtbl pointer**. Slot 2 is then
rdata `[012D7A28+8]`. **PROVEN**.

Factory `00DBEF70`:

```
00DBEF72  push 0x10C
00DBEF7B  call 00BFEA1A
00DBEF8B  call 00DAAC00               // only E8
```

`calls-s-qnovi-run-00dabac0`: **0**
`.text` `E8` of `00DABAC0`. First
enter is this thunk after construct.
**PROVEN**.

---

## 3. `00CB6EA0` is not the vtbl write and not the invoke

`listing-00c80000.txt` int3-bounded
`00CB6EA0`–`00CB6F0D`:

```
00CB6EA0  …
00CB6EAF  lea ebx, [ecx+24]           // RECORD = 24
00CB6EC0:
  sub esp, 24                         // copy
  0099EC30  CString +0
  [esi+4] = [edi-8]
  [esi+8] = [edi-4]                   // record +8 dword
  0099EC30  CString +12
  [esi+16] / [esi+20]
  call 00CB6CE0                       // per-item
  add ebx, 24
00CB6F0D  ret 4
```

`00CB6CE0` is name-compare
`00429950` then memmove
`00CB62F0` / `00CB6420`. It writes
**record** `+4/+8/+16/+20`, not an
object vtbl. Bind `00CD6E55`
`mov [esp+32], 0xDBEF70` — factory,
not `00DABAC0`. **DISPROVEN** as
the slot-2 store.

Callers (`calls-script-walk-00cb6ea0`):
`00CB710A` / `00CB7128`, both in
`00CB70E0`. That invoke has one
`E8` from `00CB77C8` inside
`00CB7780`. **PROVEN**.

First `00CB5D80` after
`00CD52D0`:

```
00CB5E12  call 00CD52D0               // includes 00CD6E27 bind
00CB5E17  mov al, [esi+17]
00CB5E1A  test al, al
00CB5E1C  je  00CB5E33                // skip start
00CB5E2A  call 00CB7780
```

`00CB5C70` ctor `[eax+17]=0`.
**PROVEN** first-seen skip of
`00CB7780` / `00CB6EA0`.

`calldisp-vtbl8-slot2-scripts` in
`00CB7000`–`00CB9000`: **1** hit
`00CB89C8`. Not `00CB6EA0`. Not a
resume of `00DABAC0`. **PROVEN**
absence in the walk.

---

## 4. Yielded no-save quest is Gameflow, not `S_QNOVI`

Host
`No_save_does_not_activate_Q_NewOakValeIntro`
/ `Type1_00CB8220_Gameflow_state0_*`:
`ActivatedQuests` omit the name;
`GameflowYieldQuest =
Q_NewOakValeIntro` is the
**`00893610` miss** wait inside
`00CE7670`. Sibling
`proofs/host-yield-first-tick`:
that write is first type-1, not
construct `00CE75B0`. **PROVEN**.

`00CE7670` does **not**
`004B4A10` / `00CB5AD0` the name
and does **not** `00DBEF70`.
**DISPROVEN** as the ctor that
installs `012D7A28`.

Who later activates
`Q_NewOakValeIntro` on no-save is
still **UNREAD** (PARITY “Who
activates”;
`proofs/00DBDE40-host-gap`
blocked-on-activator). Bind
`00CD6E27` is not construct.

---

## 5. What stays UNREAD

PARITY 0b: “the exact `call
[vtbl+8]` site that resumes a
yielded `00DABAC0` is still
UNREAD.”

This pass does **not** fill that
row. Do **not** invent:

- a second `00CDD440` /
  `[S_QNOVI.vtbl+8]` after yield
  (`SqnoviReentersRunAfterYield=false`;
  sibling `sqnovi-yield-resume`)
- `00CB6EA0` as that resume
- `00A446A0` looping
  `[S_QNOVI.vtbl+8]` on the watcher
  `this` (watcher `+8` is
  `00A44840`; persist `00DAADA0`
  is a different object)

`00DABAC0` first enter is
**`00CDD440`**. After it **returns**,
`00DAAD79` sets watcher `+5=1`.
While it is **on the stack** and
has yielded, any `call [vtbl+8]`
that continues that stack is
**UNREAD** as a concrete site.
Sibling notes `006E741F` dest
`00A44840` as the fiber slot
**during** that park — that dest
is **not** `00DABAC0` and is **not**
claimed here as the PARITY resume
row.

---

## Host

| Host | Native | Class |
|---|---|---|
| `SqnoviMainWatcherThunk = 00CDD440` | thunk | **MATCH** VA |
| `SqnoviReentersRunAfterYield=false` | no second `[S_QNOVI.vtbl+8]` | **MATCH** |
| `FiberCallsPersistThenRun=false` | watcher `+16` ≠ `00DAADA0` | **MATCH** |
| `IntroQuestRun = 00DABAC0` | rdata slot 2 | **MATCH** constant |
| `IntroQuestVtbl = 012D7A28` | ctor write | **MATCH** constant |
| `IntroQuestFactory = 00DBEF70` | bind / ctor | **MATCH** constant; **not executed** no-save |
| `NewGameScript.ListWalk = 00CB6EA0` | 24-byte walk | **MATCH** VA; **DISPROVEN** first-seen |
| `GameflowTickFn = 00CE7670` | no-save `00CDD440` dest | **MATCH** |
| `TickGameflowMain` sets `GameflowYieldQuest` | wait, not construct | **MATCH** (`host-yield-first-tick`) |
| `ActivateNamedQuest("Q_NewOakValeIntro")` | would run `00DAAC16` | **DISPROVEN** no-save; do not invent |

Pump traces on no-save must not
contain `Va==00DABAC0` or
`Va==00DAAC00`. **PROVEN** absence
in existing type-1 tests.

---

## Gap

| Item | Class |
|---|---|
| No-save write of live `[quest]=012D7A28` | **DISPROVEN** (never) |
| Bind / walk as that write | **DISPROVEN** |
| No-save `00CDD440` dest `00DABAC0` | **DISPROVEN** |
| Activator of `Q_NewOakValeIntro` | **UNREAD** |
| `call [vtbl+8]` that **resumes** yielded `00DABAC0` | **UNREAD** (PARITY 0b) |

Do **not**:

- `ActivateQuest("Q_NewOakValeIntro")`
  so that `00CDD440` hits
  `00DABAC0` on first type-1.
- Treat `00CB6EA0` as installing or
  calling slot 2.
- Invent a fiber `call [vtbl+8]`
  resume of `00DABAC0`.

---

## Sources (absolute)

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00cc0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00c80000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00d80000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\script-runtime\calls-s-qnovi-run-00dabac0-00dabac0.md`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\script-runtime\calls-script-walk-00cb6ea0-00cb6ea0.md`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\script-runtime\calldisp-vtbl8-slot2-scripts-00000008.md`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\script-bank\native-sqnovi.md`
- `C:\FableCSharp\assembly\exe\00-index\vtbl.tsv`
- `C:\FableCSharp\docs\PARITY.md`
- `C:\FableCSharp\proofs\sqnovi-yield-resume\README.md`
- `C:\FableCSharp\proofs\intro-fiber-attackover\README.md`
- `C:\FableCSharp\proofs\host-yield-first-tick\README.md`
- `C:\FableCSharp\proofs\gameflow-main-first-tick\README.md`
- `C:\FableCSharp\proofs\00DBDE40-host-gap\README.md`
