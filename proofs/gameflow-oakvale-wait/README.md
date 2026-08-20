# First `00CE7670` after Leave — `00893610("Q_NewOakValeIntro")` wait

Investigation only. No production `src/` edits.

Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** collapse this fn onto first `00CE75B0` attach-`Main`.
Do **not** treat host `GameflowWaitQuest` as construct or as
the first `00CE75B0` body.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Sources: `listing-00cc0000.txt` `00CE75B0` / `00CE7640` /
`00CE7670` / `00CE77D7` / `00CDD440` / `00CD6E27`;
`listing-00880000.txt` `00893610`;
`listing-00c80000.txt` `00CB7950` / `00CB7C40` / `00CB8220`;
`listing-00480000.txt` `004A5A40` / `004B4490`;
`listing-00600000.txt` `00629270`;
`listing-006c0000.txt` `006E7410`;
`listing-00a40000.txt` `00A44880` / `00A446A0` / `00A44660`;
`proofs/gameflow-main-first-tick/README.md`;
`docs/runtime/FORWARD_TREE.md` §§6–11;
`docs/PARITY.md` first-seen `004B4490` / later type-1 /
who-activates;
`EngineLifecycleTests`
(`Gameflow_00CE75B0_is_Main_watcher_not_S_GF`,
`Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`,
`Type1_resume_00CB8220_is_00A44880_then_00893610_yield`,
`No_save_does_not_activate_Q_NewOakValeIntro`).

---

## Verdict

**First `00CE7670` after Leave is the first type-1
`00CB8220` walk. It only waits. It does not construct
`Q_NewOakValeIntro`.**

`user.ini` `ActivateQuest("Gameflow")` already ran
`00CE75B0` (attach `"Main"` / `00CDD450` / `00CB7E50`).
That body has no `"Q_NewOakValeIntro"`, no `00893610`,
no yield. `GameflowYieldQuest==null` after construct.
**PROVEN** (`proofs/gameflow-main-first-tick`).

Later, first type-1 `004A5A40` → `004B4490([0x13B89FC])`
→ `00CB8220` → `00CB7C40` → `00CB7950` Main (`+41=0`)
→ `vtbl+4` `00A44880` → `00A446A0` `vtbl+16` `00CE7640`
→ `00CDD440` `jmp [vtbl+8]` → **`00CE7670`**. State 0
(`00CE77D7`, `[SharedRun+4]=0`) calls
`[esi+64].vtbl+100` `00893610("Q_NewOakValeIntro")`.
Miss is `al=0`. Invert + `je` skip-advance: miss falls
into `[esi+64].vtbl+28` `006E7410` → `[0x13D2838]`
`vtbl+8` `00A44840` → `009D8650`. **PROVEN** wait.

The same first `00CE7670` attaches `CoreQuestReminder`
/ `CheckBarrowFieldsGuards`. Those are Gameflow
watchers, not Oakvale. Three `"Q_NewOakValeIntro"`
pushes in this fn are card-bind + is-active + wait
loop. Zero `00CB5AD0` / `004B4A10`. **DISPROVEN** as
construct.

Host `EngineLifecycle.GameflowWaitQuest =
"Q_NewOakValeIntro"` is **LEFTOVER** as a description
of first `00CE75B0`. At **this** site it is only a
**note** of the `00893610` argument.
`TickGameflowMain` sets `GameflowYieldQuest` to that
name. `ActivatedQuests` / `Runtime.Quests` still have
no row. Feeding the constant to `ActivateNamedQuest`
is invented.

| Question | Answer | Class |
|---|---|---|
| First `00CE7670` after Leave? | first type-1 `00CB8220`, not construct | **PROVEN** |
| First `00CE75B0` after Leave? | `user.ini` Gameflow attach `"Main"` | **PROVEN** earlier |
| What starts the type-1 walk? | `0049DFB0` flag `00629270` → `004A5A40` → `004B4490` | **PROVEN** |
| First `004189C2` dummy reach `00CE7670`? | no; `0041674A=0` skips type-1 | **DISPROVEN** |
| `00893610("Q_NewOakValeIntro")` first-seen? | `0` (lookup miss) | **PROVEN** |
| Then what? | invert → yield `006E7410` / `009D8650` | **PROVEN** |
| Does `00CE7670` construct the quest? | no; wait + attach Core/Barrow | **DISPROVEN** |
| Host `GameflowWaitQuest` as first Main body? | wrong layer | **LEFTOVER** |
| Host `GameflowWaitQuest` at this site? | note of `00893610` arg, not an activate | **PROVEN** note |
| Invent `ActivateQuest("Q_NewOakValeIntro")`? | not on this walk | **DISPROVEN** |

---

## Timeline (no-save New Game)

```
0042F2A2 Leave frontend
0042F491 Init Game → 004184BD
  004A1840
    00CD6E27 00CB5C90 bind Q_NewOakValeIntro / S_QNOVI / 00DBEF70
                                                      // BIND ONLY
  004B4260([world+172])                 // six WLD; not Oakvale
  user.ini 009EC890
    ActivateQuest("Gameflow")
      00CB7900 vtbl+12 00CE6CF0 then jmp vtbl+4
      00CE75B0                          // FIRST 00CE75B0, not 00CE7670
        sub esp, 8
        00CDD450 "Main" / 00CB7E50
        ret                             // GameflowYieldQuest == null
004189C2 first pumps
  first dummy: 0041674A=0 → skip 0041726D / 004A5A40
  later 004AEBA0=1, 0041674A=1
    004AEAA0 009F16F0 type 1
    0041726D → 0049DFB0
      first walk skip type 1
      flag walk 00629270 → 004A5A40     // STARTS the type-1 walk
        [0x13B89FC] 004B4490
          [esi+56] constructed factories
          [node+8]+8 → 00CB8220         // FIRST 00CB8220
            00CB7C40 walk [this+4]
              head = Q_SunnyvaleMaster 00CDD360
              …
              Gameflow Main
                00CB7950 +41=0 → [eax+4] 00A44880
                00A446A0 first [vtbl+16] 00CE7640
                [watcher+52] 00CDD440 jmp [factory.vtbl+8]
                00CE7670                    // FIRST 00CE7670
                  attach CoreQuestReminder 00CEF3B0
                  attach CheckBarrowFieldsGuards 00CEF550
                  [esi+68]+4 = 0 → 00CE77D7
                  00896A30 OBJECT_QUEST_CARD_OAKVALE_INTRO miss
                  00893610 "Q_NewOakValeIntro" → 0
                  invert bl=1; je skip not taken
                  [esi+64].vtbl+28 006E7410
                    [0x13D2838].vtbl+8 00A44840 → 009D8650
                  no 004B4A10 / 00CB5AD0
            00CB8170 [+8]=0 empty
  later type-1
    00A44880 / 00A44660 009D87F0
    00893610 still 0 → same yield
    no re-attach Core/Barrow
```

`00DABAC0` / `00DBDE40` / `S_QNOVI` / `00CBFB7D` are
**not** on this list. **PROVEN**.

Construct vs first wait is locked by
`Gameflow_00CE75B0_*` (`GameflowYieldQuest==null`,
watchers=`Main` only) then `Type1_00CB8220_*`
(set after the first type-1 `Pump`).

---

## 1. Later fn `00CE7670`, not first `00CE75B0`

`listing-00cc0000.txt` pads `00CE75B0` (`ret` at
`00CE763A`) from `00CE7640` / `00CE7670`.

| VA | First x86 | First-seen role |
|---|---|---|
| `00CE75B0` | `sub esp, 8` | construct attach `"Main"` |
| `00CE7640` | `push esi` | watcher run: `call [esi+52]` |
| `00CE7670` | `sub esp, 0x824` | factory `vtbl+8` tick |

`00CE75B0` (`listing-00cc0000`):

```
00CE75B0  sub esp, 8
00CE75D0  push "Main"
00CE75EC  call 00CDD450
00CE75F1  mov [esi], 0x12C44B4
00CE75F7  mov [esi+52], 0xCDD440
00CE75FE  mov [esi+56], edi
00CE7619  call 00CB7E50
00CE763A  ret
```

No `"Q_NewOakValeIntro"`. No `00893610`. No `006E7410`.
Oakvale pushes start at `00CE791D` **inside** `00CE7670`.
**PROVEN**.

`00CE7640`:

```
00CE7640  push esi
00CE7641  mov esi, ecx
00CE7643  mov ecx, [esi+56]
00CE7646  call [esi+52]
00CE7649  mov [esi+5], 0x01
00CE764E  ret
```

`[esi+52]` was written by `00CE75B0` to `00CDD440`:

```
00CDD440  mov eax, [ecx]
00CDD442  jmp [eax+8]
```

That is Gameflow factory `vtbl+8` → `00CE7670`.
Construct never calls it. **PROVEN**.

ExeIndex title `q-newoakvaleintro-script-00ce7670` names
the **wait** site. It is not a construct / activate of
that quest.

---

## 2. What starts the type-1 walk that reaches it

First `004189C2` after Leave is dummy
(`0041674A=0` → skip `004AEAA0` / `0041726D`).
That pump does **not** enter `004A5A40` / `00CB8220`.
**DISPROVEN** as first `00CE7670`.

Type-1 starts when `004AEBA0` returns 1 and
`0041674A` is 1:

```
0041726D → 0049DFB0
  table [0x13B9288]; first walk skip type 1
  flag walk 00629270
    00629270  call 004A5A40
    00629275  ret 4
```

`004A5A40` (`listing-00480000`) first-seen
`[world+260]==0` (or `==9`) takes:

```
004A5D82  mov ecx, [0x13B89FC]
004A5D88  call 004B4490
```

`004B4490` walks QuestManager `[esi+56]` (constructed
factory objects; tail-insert: six WLD then Gameflow).
Nonempty `[node+8]+8` → `00CB8220`:

```
004B4517  mov eax, [esi+56]
004B4522  mov eax, [edi+8]
004B4525  cmp [eax+8], ebx        // ebx=0
004B4528  je 004B4549
004B453B  mov ecx, [eax+8]
004B453E  call 00CB8220
```

`00CB8220` (`listing-00c80000`):

```
00CB8220  push esi
00CB8223  call 00CB7C40
00CB822B  jmp 00CB8170
```

`00CB7C40` walks `[this+4]` and `00CB7950`s each
`[node+8]`. Head is `Q_SunnyvaleMaster` `00CDD360`
(first fiber yield on the walk, not Gameflow).
Gameflow Main is later on the **same** list.

`00CB7950` first-seen: `+40=0`, `00F35A00=1`,
`+41=0` → `call [eax+4]` (`00A44880`):

```
00CB7976  mov al, [esi+41]
00CB797B  je 00CB7997
00CB7997  call [eax+4]
```

`00A44880` first-seen `[0x13D2838]==0` enqueues and
`00A44660`. Fiber entry `00A446A0` first pass
`call [eax+16]` (`00CE7640`) then loops `vtbl+8`.
That is the walk that first enters `00CE7670`.
**PROVEN**.

Later type-1 takes the same `+41=0` → `00A44880`
path, then `00A44660` / `009D87F0` resume. Host
parked-skip-`00A44880` is **DISPROVEN**.

---

## 3. `00CE7670` first-seen body — attach, then wait

`listing-00cc0000.txt` `00CE7670`:

```
00CE7670  sub esp, 0x824
00CE7694  push "CoreQuestReminder"
00CE76B2  call 00CDD450
00CE76BD  mov [edi+52], 0xCEF3B0
00CE76DF  call 00CB7E50
00CE7718  push "CheckBarrowFieldsGuards"
00CE7735  call 00CDD450
00CE7740  mov [edi+52], 0xCEF550
00CE7762  call 00CB7E50
00CE7785  mov ecx, [esi+68]
00CE7788  mov eax, [ecx+4]
          … jump table …
00CE77D7  mov [ecx+4], ebp          // state 0
```

State 0 (`00CE77D7`) then tattoo GiveNamedObject
miss, `00CBE87F(10)` `TEXT_QST_LOG_STORY_10`,
quest-card bind, is-active:

```
00CE791D  push "Q_NewOakValeIntro"
00CE7930  push "OBJECT_QUEST_CARD_OAKVALE_INTRO"
00CE7957  call [edx+1180]           // 00896A30; 004B0C80 miss
00CE7977  push "Q_NewOakValeIntro"
00CE7995  call [edx+100]            // 00893610
00CE7998  mov bl, al
00CE799A  neg bl
00CE799C  sbb bl, bl
00CE79A5  inc bl                    // active→0; miss→1
00CE79AC  test bl, bl
00CE79AE  je 00CE7A02               // skip wait if already active
00CE79B0  mov ecx, [esi+64]
00CE79B5  call [edx+28]             // 006E7410 yield
00CE79BA  call 00CB7940
00CE79C9  push "Q_NewOakValeIntro"
00CE79E7  call [eax+100]            // 00893610 again
00CE7A00  jne 00CE79B0              // still miss → yield again
00CE7A02  … fresco hooks only if the wait completed …
```

Invert: `00893610` `al=0` → `bl=1` → `je 00CE7A02`
not taken → yield. `al=1` → `bl=0` → skip to
`00CE7A02` (fresco names). First-seen is the miss.
**PROVEN**.

`006E7410` (`listing-006c0000`):

```
006E7410  mov ecx, [0x13D2838]
006E7416  mov al, [ecx+5]
006E741B  jne 006E7451
006E741F  call [eax+8]              // 00A44840 → 009D8650
```

First yield does **not** take the later
`0049D870` WorldFrame compare (`+5` already set
by the wait). **PROVEN** (PARITY).

Same first walk, insert-at-tail after Main yield:
`00CEF3B0` `[+72]=0` yield; `00CEF550` trader
`Q_TraderConflict*` miss yield. Later type-1 does
not re-run tattoo/card or re-attach those two.
**PROVEN**.

---

## 4. `00893610` is lookup, not construct

`listing-00880000.txt`:

```
00893610  sub esp, 20
00893617  call 006E7510
00893621  call 006E7530
00893645  mov [esp+12], 0x33
00893651  call 008ABED0
00893656  test eax, eax
0089365A  je 0089367D
00893666  call 004AF3C0
00893675  mov al, 0x01
0089367A  ret 4
0089367D  xor al, al
00893682  ret 4
```

Miss (`008ABED0` 0) returns `al=0`. Hit returns 1
after `004AF3C0`. No `004B4A10`. No `00CB5AD0`.
No factory alloc. **PROVEN** is-active.

`listing-00cc0000.txt` `"Q_NewOakValeIntro"` sites:

| VA | Role |
|---|---|
| `00CD6E27` / `00CD6E86` | `00CB5C90` bind `S_QNOVI` / `00DBEF70` (Init World, **not** this tick) |
| `00CE791D` | card name next to `OBJECT_QUEST_CARD_OAKVALE_INTRO` |
| `00CE7977` | first `vtbl+100` is-active |
| `00CE79C9` | wait-loop is-active |

No other push in this listing. `00CE7670` never
hands the string to `00CB5AD0`. **PROVEN** wait
only.

Who later constructs the quest on no-save is
**UNREAD**. Not Leave / not `004B4260` / not
`user.ini` / not `00CE7670`. Do not invent
`ActivateQuest` to “unblock” Gameflow.

---

## 5. Host `GameflowWaitQuest` leftover vs this site

`EngineLifecycle.GameflowWaitQuest =
"Q_NewOakValeIntro"`.

| Host use | Native | Class |
|---|---|---|
| Name of first `00CE75B0` body | attach `"Main"`; no such string | **LEFTOVER** |
| `GameflowYieldQuest` after `EnterGame` | `null` | **PROVEN** unused at construct |
| `TickGameflowMain` `00893610` note | first type-1 `00CE7670` | **PROVEN** note of **this** site |
| `ResumeGameflowWait` later type-1 | same miss → yield | **PROVEN** note |
| `ActivatedQuests` / `Runtime.Quests` row | none | **PROVEN** absent |
| `ActivateNamedQuest(GameflowWaitQuest)` | not on no-save | **DISPROVEN** / invented |

So: leftover **versus first `00CE75B0`**. Valid
**only** as the `00893610` argument at `00CE7670`.
`GameflowYieldQuest` is a host sticky of that wait,
not a constructed quest.

---

## Classifications (short)

1. **First `00CE7670` after Leave — PROVEN first
   type-1 `00CB8220`.** Started by `0049DFB0`
   flag `00629270` → `004A5A40` → `004B4490`.
   Not first `004189C2`. Not `00CE75B0`.
2. **State 0 wait — PROVEN
   `00893610("Q_NewOakValeIntro")==0` →
   `006E7410` / `009D8650`.** Same miss on resume.
3. **Construct / activate Oakvale at this site —
   DISPROVEN.** Three name pushes are card +
   is-active + wait loop. Bind is earlier
   `00CD6E27`. Activator remains **UNREAD**.
4. **Host `GameflowWaitQuest` — LEFTOVER as first
   Main body. PROVEN note of this wait only.**
   Do not invent `ActivateQuest("Q_NewOakValeIntro")`.
