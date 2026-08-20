# `00DAAC00` / `00DABAC0` on no-save first Present

Investigation only. No production `src/` edits.

Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** treat `QuestFactoryTable.Find` as construct.
Do **not** treat Gameflow’s `00893610` wait as
`00DAAC00`. Do **not** re-enter `00DABAC0` from
resume `00A44660` / `009D87F0`.

Question: on no-save first Present, is `S_QNOVI`
constructed? When is `00DAAC00` / `00DABAC0`
first entered?

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority:

- `listing-00d80000.txt` (`00DAAC00` / `00DAACE0` /
  `00DAAD70` / `00DABAC0` / `00DAC295` / `00DBDE40` /
  `00DBEF70`)
- `listing-00cc0000.txt` (`00CD52D0` / `00CD6E27` /
  `00CDD440` / `00CE7670`)
- `listing-00c80000.txt` (`00CB5AD0` / `00CB5C90` /
  `00CB7900`)
- `listing-00480000.txt` (`004B4260` / `004B3CE0`)
- `listing-00a40000.txt` (`00A44660` / `00A446A0` /
  `00A44690` / `00A44840` / `00A44880`)
- `listing-006c0000.txt` (`006E7410`)
- ExeIndex `calls-s-qnovi-factory-00dbef70` (0 `E8`),
  `calls-s-qnovi-run-00dabac0` (0 `E8`),
  `calls-s-qnovi-ctor-00daac00` (1: `00DBEF8B`),
  `e8.tsv` dest `00DAAC00` / `00CB5AD0`,
  `vtbl.tsv` `0x012D7A28`
- Host `EngineLifecycle.OakvaleFactoryFn=00DBEF70`,
  `OakvaleBindSite=00CD6E27`,
  `SqnoviMainWatcherThunk=00CDD440`,
  `SqnoviReentersRunAfterYield=false`
- `QuestFactoryTable.Find("Q_NewOakValeIntro")`
  ScriptName `S_QNOVI` factory `00DBEF70` run
  `00DABAC0` persist **false**
- Tests `No_save_does_not_activate_Q_NewOakValeIntro`,
  `Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`
- Siblings `proofs/sqnovi-first-construct`,
  `proofs/sqnovi-yield-resume`,
  `proofs/gameflow-oakvale-wait`,
  `proofs/intro-fiber-attackover`,
  `proofs/oakvale-later-activate`,
  `proofs/00DBDE40-host-gap`

---

## Verdict

**No-save first Present does not construct `S_QNOVI`.**
`00DAAC00` and `00DABAC0` are **not** entered.

`00CD6E27` is factory **bind** (`00CB5C90`
`Q_NewOakValeIntro` / `S_QNOVI` / `00DBEF70`).
`QuestFactoryTable.Find` returns that row. Init
Quests `004B4260([world+172])` never names Oakvale.
Type-1 Gameflow `00CE7670` **waits**
`00893610("Q_NewOakValeIntro")` miss. First
rendered region is Lookout.

Ctor `00DAAC00` has **one** `.text` `E8`:
`00DBEF8B` inside factory `00DBEF70` (0 `E8`).
Run `00DABAC0` has **zero** `.text` `E8`. First
enter is `00DAAD76` → `00CDD440`
`jmp [S_QNOVI.vtbl+8]`. Resume is `00A44880` →
`00A44660` → `009D87F0([fiber+16])`. That is
**not** a second `[S_QNOVI.vtbl+8]`.

Who later `00CB5AD0`s this name on no-save is
**UNREAD**. Do not invent `ActivateQuest`.

| Question | Answer | Class |
|---|---|---|
| Constructed on no-save first Present? | **No.** Not in `world+172` / `ActivatedQuests` / `Runtime.Quests`. | **DISPROVEN** |
| `Find("Q_NewOakValeIntro")`? | Bind row: ScriptName `S_QNOVI`, factory `00DBEF70`, run `00DABAC0`. Not an instance. | **PROVEN** bind |
| Bind `00CD6E27` = ctor? | **No.** `00CB5C90` map insert. Persist `bl=0`. | **DISPROVEN** |
| First `00DAAC00`? | `00DBEF8B` after a later `004B3CE0` that `00CB5AD0`s this name. | **PROVEN** site; **UNREAD** walk |
| First `00DABAC0`? | `00CDD440` `jmp [vtbl+8]`. 0 `E8`. | **PROVEN** after construct |
| `004B3CE0` persist-1 `call [edx+8]`? | Sunnyvale-style; Oakvale `[record+16]=bl=0` takes `004AFA10`. | **DISPROVEN** as this object |
| Resume re-enters `00DABAC0` via `[vtbl+8]`? | **No.** `00A44660` / `009D87F0`. | **DISPROVEN** |
| Gameflow first Present? | `00893610` miss → yield. No `00CB5AD0`. | **DISPROVEN** as construct |
| Invent `ActivateQuest("Q_NewOakValeIntro")`? | Not on this walk. | **DISPROVEN** |
| Later no-save constructor? | Not Init Quests / bind / ini / Gameflow / `007EF200` intern. | **UNREAD** |

---

## Timeline (no-save New Game, first Present)

```
0042F2A2 Leave frontend
0042F491 Init Game
  00CD52D0 Registering Scripts
    xor ebx, ebx
    Q_SunnyvaleMaster [esp+48]=1          // persist 1
    …
    Gameflow / S_GF [esp+48]=bl           // 0
    …
    00CD6E14 push "S_QNOVI"
    00CD6E20 00CB5AC0                     // CString dtor stub
    00CD6E27 push "Q_NewOakValeIntro"
    00CD6E4D mov [esp+44], edi
    00CD6E51 mov [esp+48], bl             // 0
    00CD6E55 mov [esp+32], 0xDBEF70
    00CD6E5D mov [esp+36], ebp            // 00CDBD20
    00CD6E6D 00CB5C90                     // BIND ONLY
  004A0D90 FinalAlbion.qst
    AddQuest("Q_NewOakValeIntro", FALSE)  // world+184 / QM+44
                                          // not world+172
  0049F24E 004B4260([world+172])
    004B42E8 00CB5AD0 per TRUE name
    Q_NewOakValeIntro not in that vector
    004B3CE0 first script fiber = Sunnyvale
                                          // not 00DBEF70
  user.ini ActivateQuest("Gameflow")
    00CE75B0 attach "Main"                // no Oakvale string
004189C2 pumps / first Present
  type-1 00CB8220
    Gameflow 00CE7670 state 0
      00893610 "Q_NewOakValeIntro" → 0
      006E7410 / 009D8650 yield
  LookoutPoint region (leftover #4)
                                          // not StartOakVale
```

`00DAAC00` / `00DABAC0` / `00DBDE40` / `NOVI_*` /
`CS_OAKVALE_INTRO_FATHER` are **not** on this list.
**PROVEN** (`No_save_does_not_activate_*`,
`Type1_00CB8220_*`).

---

## 1. Bind is not construct

`listing-00cc0000.txt`. `00CD52D0` `xor ebx, ebx`
at `00CD52E9`. Only Sunnyvale writes
`[esp+48], 0x01`. Oakvale uses `bl`.

```
00CD6E12  push -1
00CD6E14  push "S_QNOVI"
00CD6E20  call 00CB5AC0                   // 0099EAE0 dtor, not construct
00CD6E27  push "Q_NewOakValeIntro"
00CD6E4D  mov [esp+44], edi
00CD6E51  mov [esp+48], bl                // 0
00CD6E55  mov [esp+32], 0xDBEF70
00CD6E5D  mov [esp+36], ebp               // 00CDBD20
00CD6E6D  call 00CB5C90
```

`00CB5C90` copies the 24-byte record into
`[manager+4]` (`00CB7210`). `00CB5AD0` later
looks that map up and returns `lea eax, [edi+4]`
or 0. It does **not** `00BFEA1A(0x10C)`.
**PROVEN**.

`.text` pushes of `"Q_NewOakValeIntro"`:

| VA | Role |
|---|---|
| `00CD6E27` / `00CD6E86` | bind / cleanup |
| `00CE791D` | Gameflow card |
| `00CE7977` / `00CE79C9` | is-active wait |

No push into `00892E80` / `004B4A10`. **PROVEN**.

Host:

| Constant | Value | Class |
|---|---|---|
| `OakvaleBindSite` | `00CD6E27` | **MATCH** |
| `OakvaleFactoryFn` | `00DBEF70` | **MATCH** |
| `Find(...).ScriptName` | `S_QNOVI` | **MATCH** |
| `Find(...).Factory` | `00DBEF70` | **MATCH** |
| `Find(...).Run` | `00DABAC0` | **MATCH** |
| `Find(...).PersistentBind` | `false` | **MATCH** `bl` |
| `Runtime.Quests` row | none | **MATCH** absent |

---

## 2. Ctor `00DAAC00` — only from factory `00DBEF70`

`listing-00d80000.txt`. Int3-bounded
`00DBEF70`–`00DBEF97`:

```
00DBEF70  push esi / push edi
00DBEF72  push 0x10C
00DBEF7B  call 00BFEA1A
00DBEF85  je  00DBEF93                    // eax=0
00DBEF87  push esi / push edi
00DBEF89  mov ecx, eax
00DBEF8B  call 00DAAC00                   // ONLY E8
00DBEF92  ret
```

Ctor:

```
00DAAC00  push esi
00DAAC03  call 00CB8110                   // base
00DAAC10  mov [esi+64], eax               // ctx
00DAAC13  mov [esi+68], ecx
00DAAC16  mov [esi], 0x12D7A28
          xor-clear +156 … +248
00DAACB6  call [edx+348]                  // two ctx ids
00DAACD9  ret 8
```

`e8.tsv` dest `00DAAC00`: **`00DBEF8B` only**.
`00DBEF70` / `00DABAC0`: **0** `E8`. Factory is
`call [record+4]` (persist-1) or an indirect
`call eax` after `004AFA10` (persist-0), both
inside `004B3CE0` **after** `00CB5AD0` hit.

Oakvale persist `bl=0` skips

```
004B3F0E  mov cl, [eax+16]
004B3F15  je  004B3F30                    // 004AFA10
004B3F17  call [eax+4]
004B3F1C  mov edx, [esi]
004B3F20  call [edx+8]                    // not this object
```

That `call [edx+8]` is the Sunnyvale persist-1
run (`00CDBA10`). It is **not** first-seen
`00DABAC0`. **DISPROVEN** as Oakvale’s construct
arm.

`00CB7900` (after the object exists):

```
00CB7905  call [eax+12]                   // 00DAADD0 reset
00CB790D  jmp  [edx+4]                    // 00DAACE0 Main
```

Main attaches the watcher. It does **not** enter
slot 2. **PROVEN**.

`vtbl.tsv` `0x012D7A28`:

| Off | Dest | Role |
|---:|---|---|
| +0 | `00DBEFA0` | dtor |
| +4 | `00DAACE0` | Main `00CDD450` / `+52=00CDD440` |
| **+8** | **`00DABAC0`** | slot 2 run |
| +12 | `00DAADD0` | reset; clears `+80` |
| +16 | `00DAADA0` | persist bind `AttackOver` |
| +24 | `00A44880` | update |
| +28 | `00A44840` | wait helper |
| +32 | `00A447D0` | fiber recreate (0 `E8`) |
| +36 | `00DAAD70` | watcher `+16` thunk |

On no-save first Present none of this object
exists, so none of these run. **PROVEN**.

---

## 3. First `00DABAC0` is `00CDD440` `jmp [vtbl+8]`

After a **proven** construct (not this Present):

```
00DAACE0  slot 1 Main
  00CDD450 "Main"
  vtbl 012D7A3C
  +52 = 00CDD440
  +56 = S_QNOVI
  00CB7E50 attach
type-1 00CB7950 +41=0
  [watcher.vtbl+4] 00A44880
    00A446A0
      [watcher.vtbl+16] 00DAAD70          // NOT persist 00DAADA0
        mov ecx, [esi+56]                 // S_QNOVI
        call [esi+52]                     // 00CDD440
          00CDD440  mov eax, [ecx]
                    jmp [eax+8]           // FIRST 00DABAC0
```

`listing-00cc0000.txt` int3-bounded:

```
00CDD440  mov eax, [ecx]
00CDD442  jmp [eax+8]
```

`00DAAD70` (`listing-00d80000`):

```
00DAAD70  push esi
00DAAD73  mov ecx, [esi+56]
00DAAD76  call [esi+52]
00DAAD79  mov [esi+5], 1
00DAAD7E  ret
```

`+5=1` is stored **after** `00DABAC0` **returns**.
Until then `00A446A0` may loop watcher `[vtbl+8]`
=`00A44840` (park), not `00DABAC0`. **PROVEN**.

Slot 2 body (`00DABAC0`): register `NOVI_LiveFather`
`00DAC2C0`, `NOVI_Theresa` `00DAC420`, … then

```
00DAC293  mov ecx, esi
00DAC295  call 00DBDE40                   // only E8 of setup
00DAC2A1  ret
```

`00DBDE40` map-waits `"StartOakVale"` via
`[this+64].vtbl+28` / `00CB7940`. That is
**after** construct, **not** first Present.

---

## 4. Resume does not re-enter `00DABAC0`

Yield inside `00DBDE40` (`listing-00d80000`):

```
00DBDE81  mov ecx, [esi+64]
00DBDE84  mov eax, [ecx]
00DBDE86  call [eax+28]                   // 006E7410
```

`006E7410` (`listing-006c0000`):

```
006E7410  mov ecx, [0x13D2838]
006E7416  mov al, [ecx+5]
006E741B  jne 006E7451
006E741F  call [eax+8]                    // watcher 00A44840
```

`00A44840` → `00A44690` → `009D8650` park.
`[eax+8]` here is the **fiber** vtbl, dest
`00A44840`, **not** `00DABAC0`. **PROVEN**.

Later type-1 `00A44880` (`listing-00a40000`):

```
00A448D5  … enqueue …
00A44913  call 009E1BC0
00A44918  fstp [this+8]
00A44921  call 00A44660                   // RESUME
```

```
00A4466A  mov [0x13D2838], ecx
00A44672  mov ecx, [ecx+16]
00A44675  call 009D87F0                   // continue after 00A44690
00A4467A  mov [0x13D2838], 0
```

`009D87F0` continues **inside** `00A44840` /
`006E7410` / `00DBDE40`. It does not
`jmp [S_QNOVI.vtbl+8]`. Host
`SqnoviReentersRunAfterYield=false`. **PROVEN**.

Gameflow on this same first Present uses the
**same** pump (`00A44880` / `00A44660`) for
`00CE7670`’s wait. That still does not construct
`S_QNOVI`. **PROVEN**.

---

## 5. What stays UNREAD

| Item | Class |
|---|---|
| First no-save `004B4260` / `004B4A10` list that includes `Q_NewOakValeIntro` | **UNREAD** |
| Debug test-quest UI `0061AB30` (`world+196`) | **PROVEN** consumer; **DISPROVEN** as New Game |
| `007EF200` / `CExpressionDef+120` intern `0x012C5D14` | **DISPROVEN** first-seen (`ExpressionPlus120IsOakvaleIntern=false`) |
| Give type-`0x33` vs construct type-`0x37` | do not collapse (`proofs/gameflow-type33-give`) |
| Persist-0 `004B3F47 call [record+0]` exact dest for this row | **PARTIAL** (factory still `00DBEF70`; 0 `E8`) |

Do **not** close the gap with
`ActivateQuest("Q_NewOakValeIntro")` from Leave,
Pump, or `TickGameflowMain`.

---

## Host

| Host | Native | Class |
|---|---|---|
| `OakvaleFactoryFn` / `IntroQuestFactory` `00DBEF70` | factory | **MATCH** |
| `IntroQuestCtor` `00DAAC00` | ctor | **MATCH** VA; **not executed** |
| `IntroQuestRun` `00DABAC0` | slot 2 | **MATCH** VA; **not executed** |
| `SqnoviMainWatcherThunk` `00CDD440` | first enter | **MATCH** |
| `SqnoviReentersRunAfterYield=false` | resume `00A44660` | **MATCH** |
| `GameflowWaitQuest` | `00893610` arg | **PROVEN** note; **DISPROVEN** as activate |
| `ActivatedQuests` / `Runtime.Quests` | empty for this name | **MATCH** |
| `StartNewGame` / `ActivateQuest` as New Game | invents the object | **DISPROVEN** as first Present |

---

## Sources (absolute)

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00d80000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00cc0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00c80000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00480000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00a40000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-006c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\script-runtime\calls-s-qnovi-factory-00dbef70-00dbef70.md`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\script-runtime\calls-s-qnovi-run-00dabac0-00dabac0.md`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\newgame-trace\calls-s-qnovi-ctor-00daac00-00daac00.md`
- `C:\FableCSharp\assembly\exe\00-index\vtbl.tsv`
- `C:\FableCSharp\proofs\sqnovi-first-construct\README.md`
- `C:\FableCSharp\proofs\sqnovi-yield-resume\README.md`
- `C:\FableCSharp\proofs\gameflow-oakvale-wait\README.md`
- `C:\FableCSharp\proofs\intro-fiber-attackover\README.md`
- `C:\FableCSharp\proofs\oakvale-later-activate\README.md`
- `C:\FableCSharp\proofs\00DBDE40-host-gap\README.md`
