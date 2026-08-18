# First `00CE75B0` / `00CB7C40` body after `ActivateQuest("Gameflow")`

Investigation only. No production `src/` edits.

Do **not** invent `ActivateQuest("Q_NewOakValeIntro")`.
Do **not** treat host `GameflowWaitQuest` as the first
`00CE75B0` body. That name is a later `00CE7670`
`00893610` argument, not construct and not an activate.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Sources: `listing-00cc0000.txt` `00CE75B0` / `00CDD450` /
`00CE6CF0` / `00CE7670` / `00CEF950` / `00CDD360` /
`00CDD440` / `00CEF016`;
`listing-00c80000.txt` `00CB7C40` / `00CB7900` /
`00CB7950` / `00CB7E50` / `00CB8220` / `00CB8170`;
`listing-00480000.txt` `004B4A10` / `004B4260` /
`004B3FEC`;
`listing-006c0000.txt` `006E7410`;
`proofs/ini-activate-quest/README.md`,
`proofs/script-gameflow/README.md`,
`proofs/fiber-yield-first/README.md`,
`proofs/script-opcode-after-leave/README.md`;
`docs/runtime/FORWARD_TREE.md` §§6–11;
`docs/PARITY.md` Init Game / type-1 / who-activates;
`EngineLifecycleTests`
(`Gameflow_00CE75B0_is_Main_watcher_not_S_GF`,
`Type1_00CB8220_Gameflow_state0_yields_on_Q_NewOakValeIntro`,
`No_save_does_not_activate_Q_NewOakValeIntro`).

---

## Verdict

**The first `00CE75B0` body is attach-`Main` only.
It does not wait, yield, run an opcode, or start
`Q_NewOakValeIntro`.**

`user.ini` `ActivateQuest("Gameflow")` is
`00419CE0` → `00892E80` → `004B4A10` → `004B4260`
→ `00CB5AD0` → `004B3CE0` → `00CB7900`.
`00CB7900` is `vtbl+12` (`00CE6CF0` seed) then
`jmp [vtbl+4]` (`00CE75B0`). First x86 of
`00CE75B0` is `sub esp, 8`. Body: alloc 60,
CString `"Main"`, `00CDD450` (0.1f / 64 / 1),
vtbl `0x12C44B4`, `+52=00CDD440`, `+56=Gameflow`,
`00CB7E50` attach. Return. **PROVEN**.

**`00CB7C40` is not on that construct path.**
One `.text` caller: `00CB8223` inside type-1
`00CB8220`. First x86 is `push ebx`. It walks
`[this+4]` and `00CB7950`s each `[node+8]`.
Tail-insert: first node is WLD
`Q_SunnyvaleMaster` (`00CDD360`), not Gameflow
Main. It does **not** call `00CE75B0`. **PROVEN**.

The Oakvale *name* appears later, on the first
type-1 tick of the already-attached Main fiber:
`00CB7950` `+41=0` → `vtbl+4` `00A44880` →
`00A446A0` `vtbl+16` `00CE7640` → `00CDD440`
`jmp [vtbl+8]` `00CE7670`. State 0
`00893610("Q_NewOakValeIntro")` is 0 →
`[SharedRun+28]` `006E7410` yield. That is
**not** the first `00CE75B0` / `00CB7C40` body.
It does **not** `004B4A10` that name. **PROVEN**
wait; **DISPROVEN** as start.

Host `EngineLifecycle.GameflowWaitQuest =
"Q_NewOakValeIntro"` is **LEFTOVER** as a
description of first `00CE75B0` / first
`00CB7C40`. After construct,
`GameflowYieldQuest==null` and watchers=`Main`
only (`Gameflow_00CE75B0_*`). The constant is
only a **note** of later `00CE7670`.

| Question | Answer | Class |
|---|---|---|
| First `00CE75B0` body? | attach `"Main"` via `00CDD450` / `00CB7E50` | **PROVEN** |
| First x86 of `00CE75B0`? | `sub esp, 8` | **PROVEN** |
| First `00CB7C40` body? | type-1 walk; `00CB7950([node+8])` | **PROVEN** |
| First x86 of `00CB7C40`? | `push ebx` | **PROVEN** |
| Does `00CB7C40` run at activate? | no; only `E8` is `00CB8223` | **DISPROVEN** |
| Does either wait on `Q_NewOakValeIntro`? | no | **DISPROVEN** |
| Does either yield (`006E7410` / `009D8650`)? | no | **DISPROVEN** |
| First script opcode (`00CBFB7D`)? | none | **DISPROVEN** |
| Who first uses the Oakvale *string*? | later type-1 `00CE7670` `00893610` | **PROVEN** wait |
| Does that tick activate Oakvale? | no | **DISPROVEN** |
| Host `GameflowWaitQuest` as first Main body? | wrong layer | **LEFTOVER** |

---

## Timeline (no-save New Game)

```
0042F2A2 Leave frontend
0042F491 Init Game → 004184BD
  004B4260([world+172])                 // six WLD; not Gameflow
  user.ini 009EC890
    ActivateQuest("Gameflow")
      00419CE0 → 00892E80
      004B4A10  sub esp, 12
        004B4A5A 004B4260
          00CB5AD0 "Gameflow"
          004B3CE0 construct
            00CEF950 factory 100 vtbl 012C3FA4
            flag 0 → 004AFA10 reuse 00CDBD20
            004B3FEC 00CB7900
              vtbl+12 00CE6CF0                  // seed OV_INTRO…
              jmp vtbl+4 00CE75B0               // FIRST 00CE75B0
                sub esp, 8
                00BFEA1A(60)
                0099EBF0 "Main"
                00CDD450  push 0.1f / 64 / 1
                          00A44740 fiber
                          +40=+41=0
                vtbl 0x12C44B4
                +52 = 00CDD440                  // jmp [vtbl+8]
                +56 = Gameflow
                00CB7E50 attach (0x122D70E empty)
                ret                             // no yield
              GameflowYieldQuest == null
              watchers == { Main }
              HasStarted("S_GF") == false
004189C2 first pumps
  first type-1 004A5A40 → 004B4490 → 00CB8220
    00CB8223 call 00CB7C40                    // FIRST 00CB7C40
      push ebx
      walk [ebx+4]; 00CB7950([esi+8])
      head = Q_SunnyvaleMaster 00CDD360       // first yield
      …
      tail = Gameflow Main
        00CB7950 +41=0 → vtbl+4 00A44880
        00A446A0 vtbl+16 00CE7640
        00CDD440 jmp [Gameflow.vtbl+8]
        00CE7670                              // NOT 00CE75B0
          attach CoreQuestReminder / Barrow
          state 0 00CE77D7
          00893610 "Q_NewOakValeIntro" → 0
          [run+28] 006E7410 → yield
          no 004B4A10 / 00CB5AD0
    jmp 00CB8170  [+8]=0 empty
```

`00DABAC0` / `00DBDE40` / `S_QNOVI` /
`00CBFB7D` are **not** on this list. **PROVEN**.

Construct vs first type-1 is locked by
`Gameflow_00CE75B0_*` then `Type1_00CB8220_*`.

---

## 1. `00CE75B0` — construct Main, not a tick

`listing-00cc0000.txt` `00CE75B0` (ends `00CE763A ret`
before `00CE7640`):

```
00CE75B0  sub esp, 8
00CE75B6  xor ebx, ebx
00CE75B8  push 60
00CE75BA  mov edi, ecx
00CE75C0  call 00BFEA1A
00CE75CA  test esi, esi
00CE75CE  push -1
00CE75D0  push "Main"
00CE75D9  call 0099EBF0
00CE75DE  push 0
00CE75E7  mov ebx, 0x1
00CE75EC  call 00CDD450
00CE75F1  mov [esi], 0x12C44B4
00CE75F7  mov [esi+52], 0xCDD440
00CE75FE  mov [esi+56], edi
00CE7605  push -1
00CE7607  push 0x122D70E          // empty CString
00CE7619  call 00CB7E50
00CE763A  ret
```

No `"Q_NewOakValeIntro"`. No `00893610`. No
`006E7410`. No `00CBFB7D`. No `004B4A10`.
**PROVEN** (`listing-00cc0000`; the Oakvale
pushes start at `00CE791D` inside `00CE7670`).

`00CDD450` (`listing-00cc0000`):

```
00CDD450  push esi
00CDD451  push 0x3DCCCCCD        // 0.1f
00CDD456  push 64
00CDD458  push 1
00CDD45C  call 00A44740
00CE…     vtbl 0x12C2F9C; +40=+41=0
00CDD49D  ret 8
```

`00CE75B0` then overwrites that vtbl to
`0x12C44B4`. Same attach shape as Sunnyvale
`00CDD380` (`"Main"` / `00CDD450` /
`+52=00CDD440`). Gameflow’s vtbl after overwrite
is `0x12C44B4`, not Sunnyvale `0x12C2F78`.
**PROVEN** pattern; **DISPROVEN** as Oakvale.

`00CDD440` is two insns: `mov eax, [ecx];
jmp [eax+8]`. It is a **later** tick thunk
(factory `vtbl+8` → `00CE7670`). Construct
does not call it. **PROVEN**.

`00CB7E50` allocates a 16-byte list node
`[+8]=watcher` and inserts at tail of the
quest’s watcher list. First opcode
`mov eax, [esp+8]`. Not a wait. **PROVEN**.

`00CE6CF0` runs **before** `00CE75B0` from
the same `00CB7900` (`call [eax+12]` then
`jmp [edx+4]`). It zeros `[+68]+4` and
`[+72]`, then inserts `OV_INTRO` …
`SNOWSPIRE_ARRIVAL` via vtbl+2868. Script
state names, not `ActivateQuest`. **PROVEN**.

`00CE75B0` has **0** `E8` sites. Live call
is `004B3FEC call 00CB7900` → `jmp [vtbl+4]`.
**PROVEN**.

---

## 2. `00CB7C40` — first type-1 walk, not construct

`listing-00c80000.txt` `00CB7C40`:

```
00CB7C40  push ebx
00CB7C41  mov ebx, ecx
00CB7C43  mov eax, [ebx+4]
00CB7C47  mov esi, [eax]
00CB7C49  cmp esi, eax
00CB7C4B  je 00CB7CA8            // empty → ret
00CB7C51  mov eax, [esi+8]
00CB7C54  push eax
00CB7C57  call 00CB7950
00CB7C5C  test al, al
00CB7C5E  je 00CB7C9F            // keep node
          … unlink / free …
00CB7CAA  ret
```

`00CB8220`:

```
00CB8220  push esi
00CB8223  call 00CB7C40
00CB822B  jmp 00CB8170
```

Grep of `listing-*.txt`: **one** `call 00CB7C40`
(`00CB8223`). Not `004B4A10` / `004B4260` /
`00CE75B0` / `00CB7900`. **PROVEN**.

First-seen list is tail-insert: six WLD
factory objects, then Gameflow Main (attached
at construct), then Core / Barrow (attached
*during* first `00CE7670`). Host
`QuestPumpWalked==9`. First `00CB7950` is
Sunnyvale `00CDD360` (`vtbl+28` / `00CB7940`
loop) — first fiber yield on the walk, not
Gameflow. See `proofs/fiber-yield-first`.
**PROVEN** order.

`00CB7950` first-seen: `+40=0`, `00F35A00=1`,
`+41=0` → `call [eax+4]` (`00A44880`).
Does not take `vtbl+24`. Does not re-enter
`00CE75B0`. **PROVEN**.

---

## 3. Wait / yield / opcode — not this pair

| Claim | Site | Class |
|---|---|---|
| Wait on `Q_NewOakValeIntro` | `00CE7670` `00CE791D` / `00CE7977` `push` then `[edx+100]` (`00893610`) | **PROVEN** later; **DISPROVEN** at `00CE75B0` / `00CB7C40` |
| Yield | `00CE79B5` `[edx+28]` → `006E7410` (`[0x13D2838]` `vtbl+8`) then `00CB7940`; epilogue jump `00CEF016` | **PROVEN** later |
| Activate Oakvale | `00CE79AE je 00CE7A02` skip-advance when *active*; miss does **not** `004B4A10` | **DISPROVEN** as start |
| Script opcode | runner `00CBFB7D` | **DISPROVEN** (no `S_GF` interpreter; `HasStarted("S_GF")==false`) |
| First *named* wait on type-1 | Gameflow row of the **same** `00CB7C40` walk, after Sunnyvale | **PROVEN** order; **not** first node |

`00CE7670` first x86 is `sub esp, 0x824`.
Its first *work* is attach
`CoreQuestReminder` / `CheckBarrowFieldsGuards`
(`00CDD450` / `00CB7E50`), then state switch
at `[esi+68]+4`. State 0 is `00CE77D7`.
Collapsing that fn onto `00CE75B0` is
**DISPROVEN**. ExeIndex title
`q-newoakvaleintro-script-00ce7670` is the
wait site, not a construct / activate of
that quest.

---

## 4. Host `GameflowWaitQuest`

`EngineLifecycle.GameflowWaitQuest =
"Q_NewOakValeIntro"`.

| Host use | Native | Class |
|---|---|---|
| `SeedGameflowStates` / `00CE75B0` notes | attach `Main` only; `GameflowYieldQuest==null` | **PROVEN** notes; constant unused here |
| `TickGameflowMain` sets `GameflowYieldQuest` | first type-1 `00CE7670` `00893610` | **PROVEN** note |
| `ActivatedQuests` / `Runtime.Quests` row | none | **PROVEN** absent |
| Treat constant as first `00CE75B0` body | construct has no such string | **LEFTOVER** |
| `ActivateNamedQuest(GameflowWaitQuest)` | not on no-save | **DISPROVEN** / invented |

Who later constructs `Q_NewOakValeIntro` on
no-save is **UNREAD**. Do not invent it to
“unblock” Gameflow.

---

## Classifications (short)

1. **First `00CE75B0` after `user.ini`
   Gameflow — PROVEN attach `"Main"`.**
   First opcode `sub esp, 8`. `00CDD450` /
   `00CB7E50`. No wait, no yield, no opcode.
2. **First `00CB7C40` — PROVEN later type-1
   `00CB8223`.** First opcode `push ebx`.
   Walks constructed watchers. Head is
   Sunnyvale, not Gameflow. Does not call
   `00CE75B0`.
3. **Wait on `Q_NewOakValeIntro` as that
   first body — DISPROVEN.** The wait is
   `00CE7670` state 0 on the same type-1
   walk, after attach. Miss → yield. No
   activate.
4. **Host `GameflowWaitQuest` as first Main
   / first `00CB7C40` — LEFTOVER.** Valid
   only as a later `00893610` note.
5. **Invent `ActivateQuest("Q_NewOakValeIntro")`
   — DISPROVEN** on this walk. Activator
   remains **UNREAD**.
