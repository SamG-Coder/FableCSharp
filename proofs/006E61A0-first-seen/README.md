# First-seen `006E61A0`: widgets, first call, Leave vs Speak

Investigation only. No production `src` edits.

Do **not** start New Game dialogue at Leave frontend
(`0042F2A2` / `FrontendMessages`). That path has **no**
`.Speak`. See `proofs/dialogue-first/README.md`.

Do **not** invent spoken lines. Store `TEXT_*` ids only.

Do **not** treat context `vtbl+1472` as Speak. That slot
is `WaitActiveDialog` poll `008907D0` → `006E5660`.
Speak apply is thing `vtbl+52` stub `004CD1B0`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Sources: `proofs/006E61A0-speak-widgets`,
`proofs/speak-vtbl1472` leftover Dialogue UI,
`proofs/dialogue-first`, `proofs/00DAAC00-sqnovi-no-save`,
`src/Fable.Game/FrontendMessages.cs`
(`LeaveFrontendSite = 0x0042F2A2`; no Speak),
`src/Fable.Formats/Defs/FrontendWidgetType.cs`,
`RegionTravel.cs` Speak / InteractiveSpeak /
WaitActiveDialog constants,
ExeIndex `listing-006c0000.txt` (`006E61A0`–`006E62B0`,
`006E6150`, `006E60F0`, `006E5790`, `006E6F70`,
`006E5660`),
`listing-00700000.txt` (**no** `006E61A0`),
`listing-004c0000.txt` (`004CD1B0`),
`listing-00640000.txt` (`00661A40`),
`listing-00880000.txt` (`008906C0` / `008907D0`),
`listing-00cc0000.txt` Speak / InteractiveSpeak /
DialogSpeak / WaitActiveDialog,
`listing-00400000.txt` (`0042F2A2` Leave),
`listing-00480000.txt` Init Script Conversation Manager,
`00-index/vtbl.tsv` `0x0127293C` slots 13 / 26,
`script-runtime/dialog-begin-vtbl1456-008906c0`,
`speak-apply-00cc27ea`,
`dialog-wait-vtbl1472-008907d0`,
`script-bank/0481-cs-oakvale-intro-father.md`,
`docs/status/README.md` leftover “Dialogue UI”.

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| What widgets does `006E61A0` construct? | **None.** Map insert + CString / vector copy. No `0041D21B` / `0041DB1D` / `009AD410` / type ctors. | **PROVEN** empty |
| When is `006E61A0` first called? | Sole `.text` `E8`: `008906F0` inside context `vtbl+1456` `008906C0`. First leftover enter is intro `InteractiveSpeak` `00CC2F5B` (FALSE). Speak never calls it. | **PROVEN** site; **LEFTOVER** enter |
| Does Leave call Speak? | **No.** `FrontendMessages.LeaveFrontendSite` `0042F2A2` is fade 500 ms then Init Game. No `00CC25FD` / `004CD1B0` / `006E61A0`. | **DISPROVEN** |
| First-seen no-save: `006E61A0` / Speak? | **Neither.** Ctor `006E6150` `[node+8]=self`; tick `006E60F0` walks nothing. `Q_NewOakValeIntro` not constructed. | **PROVEN** empty |
| Is `006E61A0` Speak apply? | **No.** Speak is thing `vtbl+52` `004CD1B0` `mov al,1; ret`. | **DISPROVEN** as Speak |
| Is `vtbl+1472` Speak? | **No.** `WaitActiveDialog` `00CC661D` → `008907D0` `jmp 006E5660`. Also skip / TRUE InteractiveSpeak / DialogSpeak leftover poll. | **DISPROVEN** as Speak |
| Invented father line? | **No.** Ids only (`TEXT_QST_048_FATHER_INTRO_*`). | **DISPROVEN** |

**Answer:** `006E61A0` widgets stay **none**. First-seen
no-save after Leave never enters it and never runs Speak.
First leftover call is InteractiveSpeak begin, not Speak
and not Leave.

---

## 1. First-seen no-save after Leave (empty)

`FrontendMessages.LeaveFrontendSite`:

```
0042F2A2  push "Leave frontend"
0042F2D3  push 0x1F4
0042F2D8  call [eax+72]              // 500 ms fade; keep voice
…
0042F491  Init Game 004184BD
```

`listing-00400000.txt` around `0042F2A2` has **no**
`00CC25FD`, **no** `006E61A0`, **no** `004CD1B0`.
`FrontendMessages.cs` has **no** Speak string. Leave
enum `Screen.LeaveFrontend` is a menu-root after the
frontend is gone.

Init World (`004A6E30`):

```
004A7411  push "Init Script Conversation Manager"
004A7439  push 20
004A743B  call 00BFEA1A
004A7449  call 006E6150              // world+124
```

`006E6150`:

```
006E6153  push 64
006E6155  mov [esi], 0
006E615B  call 00BFEA0E              // sentinel node
006E6177  mov [eax+8], eax           // [node+8]=self
006E617C  mov [eax+12], eax
006E6182  mov [esi+12], 1            // next handle
```

Type-1 `004A5A40` then `006E60F0`:

```
006E60F4  mov eax, [edi]
006E60F6  mov esi, [eax+8]
006E60F9  cmp esi, eax
006E60FB  je 006E6140                // empty → ret
…
006E6137  call 006E5A00              // per filled node; not reached
```

Host `TickConversations` notes `006E6150 [+8]=self` /
`006E60F0 empty`. `ConversationWalked=0`. **PROVEN.**

No-save also does **not** construct `S_QNOVI`
(`proofs/00DAAC00-sqnovi-no-save`). Gameflow
`00893610("Q_NewOakValeIntro")` miss → yield.
`00CBFB7D` / `CS_OAKVALE_INTRO_FATHER` are **not**
on this list. **PROVEN.**

First Gameflow text is journal `00CBE87F(10)` →
`TEXT_QST_LOG_STORY_10`, not Speak.

---

## 2. `006E61A0` is insert, not widgets

`listing-00700000.txt` starts at `00700002`. The whole
fn is `listing-006c0000.txt` `006E61A0`–`006E62B0`
(`ret 12`). Sole `.text` caller: `008906F0` inside
`008906C0` (context `vtbl+1456`).

```
008906C0  mov esi, [esp+8]           // actor
          call [eax+300]             // alive
          je 008906FA                // eax=0
          mov edi, [[this+4]+124]    // Script Conversation Manager
          call [thing+44]            // id
          call 006E61A0              // ecx = manager
          ret 12                     // eax = handle
```

`006E61A0` (`ecx` = manager):

```
006E61A0  sub esp, 0x94
006E61B8  mov edi, [esi+12]          // handle = old [+12]
006E61BB  lea eax, [edi+1]
006E61BE  mov [esi+12], eax
          … stack record, [+32]=-1
006E6207  call 006E5790              // participant; vtbl 0x1238C6C
006E6229  call 0062ED40              // CString copy
006E6237  call 006E6A30              // vector copy (00BFEA0E)
006E6275  call 006E6B60              // record copy
006E6286  call 006E6F70              // std::map insert by handle
006E628F  call 004AE0E0 ×3           // dtor temps
006E62A6  mov eax, edi               // return handle
006E62B0  ret 12
```

`006E6F70` walks `[node+16]` keys, `setl` / child `+8`/`+12`,
then `006E6EA0` node alloc. `006E6A30` is a 16-byte vector
copy (`sar esi, 4`). `006E5790` looks up / inserts a
participant (`00A01B10` / `00A01B90` / `004AD520`).

**Not** present in `006E61A0` or those callees:
`0041D21B`, `0041DB1D`, `009AD410`, `0054F5C0` (type 6),
`0052CC50` (type 5), `0054E3D0` (type 10), `0041B800`
(type 0). Frontend types `0..43` (`FrontendWidgetType`)
never enter this fn.

Returned `eax` is the handle InteractiveSpeak /
DialogSpeak store at `[ebp-44]`. That is a list node,
not a `CUIDef`.

`UI_DIALOG` / `UI_DIALOG_TEXT` / `UI_DIALOG_BUTTON` /
`UI_DIALOG_TABLE` are Leave / frontend.bin chrome.
**DISPROVEN** as callers of `006E61A0` / `00CC25FD`.

---

## 3. Speak is thing `vtbl+52` stub `004CD1B0`

`listing-00cc0000.txt` / `speak-apply-00cc27ea`:

```
00CC25FD  push ".Speak"
…
00CC27EA  push [ebp-564]
          push 1
          push 0
          push [ebp+20]              // mode
00CC2813  call [esi+52]              // apply
00CC2821  jmp 00CC2909
00CC2909  call [eax+104]             // leftover poll
```

Father vtbl `0x0127293C` (`vtbl.tsv`):

| Slot | Off | Fn | Body |
|---|---|---|---|
| 13 | `+52` | **`004CD1B0`** | `mov al, 0x01; ret` |
| 26 | `+104` | **`00661A40`** | `ret 4` (leaves `al`) |

```
004CD1B0  mov al, 0x01
004CD1B2  ret
```

No widget factory. No `006E61A0`. No `call [eax+1472]`.
First leftover poll is busy → one `vtbl+28` then continue.
`RegionTravel.SpeakUsesVtbl1472=false`.
`FirstSeenSpeakYieldsOnce=true`. Apply as UI: **UNREAD**
because the slot is a stub.

`.Speak` does **not** `call [eax+1472]`. **DISPROVEN.**

---

## 4. `vtbl+1472` is WaitActiveDialog, not Speak

Context `[0x143E8F8]` slot 368 (`0x5C0`). Nearby
dialog slots:

| Off | Fn | Role |
|---|---|---|
| 1456 | `008906C0` | begin → **`006E61A0`** |
| 1460 | `00890710` | bind → `006E5800` |
| 1464 | `00890750` | line → `006E5950` |
| **1472** | **`008907D0`** | **wait; `jmp 006E5660`** |

```
008907D0  mov eax, [ecx+4]
008907D3  mov ecx, [eax+124]
008907D6  jmp 006E5660
```

`006E5660`: `006E69E0` then `cmp edx,ecx` / `setne al` /
`ret 4`. Busy = handle still in the conversation list.
**Not** a draw. **Not** Speak apply.

Four `call [eax+1472]` sites in the runner:

| Site | Opcode | First-seen no-save / intro leftover |
|---|---|---|
| `00CC0148` | skip copy | **DISPROVEN** (`FirstSeenCutsceneSkipFires=false`) |
| `00CC3158` | InteractiveSpeak **TRUE** | **DISPROVEN** (intro arg is FALSE) |
| `00CC32A5` | DialogSpeak leftover poll | **PARTIAL** one leftover |
| **`00CC661D`** | **`WaitActiveDialog` `00CC656B`** | leftover after `GamePause 0.8`; **not Speak** |

```
00CC656B  push "WaitActiveDialog"
00CC65BB  cmp [ebp-44], edi
00CC65BE  je 00CC7081                // no handle → continue
00CC6612  push [ebp-44]
00CC661D  call [eax+1472]
00CC6625  jne 00CC65C6              // leftover vtbl+28
```

Speak never writes `[ebp-44]`. WaitActiveDialog with
a prior InteractiveSpeak / DialogSpeak handle polls
`1472`. Still **not** Speak, and still **not**
`006E61A0`.

`docs/status/README.md` leftover “Dialogue UI
(`Speak` / `InteractiveSpeak` / `DialogSpeak`)” is
this apply/runtime gap: one yield; no invented UI.
`006E61A0` was UNREAD there as widgets; this note
closes it as **insert, no widgets**.

---

## 5. When first called (leftover intro, ids only)

Not Leave. Not first type-1. First leftover path that
can enter `008906C0`:

`CCutsceneDef` 481 `this+60`
(`0481-cs-oakvale-intro-father.md`), after
`NOVI_LiveFather` → `00DB86B0` →
`00CBFB7D("CS_OAKVALE_INTRO_FATHER")`. First executed
line is `PlayMusic MUSIC_SET_NULL`, not Speak.

| After | Verb | Key | Native | `006E61A0` |
|---|---|---|---|---|
| `GamePause 1.6` | `Father.Speak Father,…` | `TEXT_QST_048_FATHER_INTRO_10` | thing `+52` `004CD1B0` | **no** |
| `GamePause 1.0` | `Father.InteractiveSpeak Hero,…,FALSE,…` | `_20` / `_30` | **`1456` → `006E61A0`**, then `1460`/`1464`; FALSE one `vtbl+28`; **no** `1472` | **first enter** |
| tired / `GamePause 2.0` | `Father.DialogSpeak HERO,…` | `_60` | leftover `1472` then `1456` again | second enter |
| `GamePause 0.8` | `WaitActiveDialog` | — | `1472` `008907D0` | **no** |
| | `Father.Speak Father,…` | `_70` `_80` `_90` | `+52` stub | **no** |
| | `Father.DialogadSpeak Father,…` | `_100` | `+52` stub; no yield | **no** |

Do not paste `text.big` sentences. Host may
`LookupText` those ids; tests lock the **id**.

InteractiveSpeak first-seen third arg is **FALSE**
(`FirstSeenInteractiveSpeakArgIsTrue=false`):

```
00CC2F50  push 1
          push 1
          lea edx, [ebp-1364]
00CC2F5B  call [eax+1456]            // 008906C0 → 006E61A0
00CC2F73  mov [ebp-44], esi
…
00CC3100  jmp 00CC707C               // FALSE: one vtbl+28, no 1472
00CC3158  call [eax+1472]            // TRUE arm; intro does not take it
```

So the **first** `006E61A0` on leftover intro is
InteractiveSpeak begin, **after** Speak already ran
the `004CD1B0` stub. Speak does not call insert.

---

## 6. Host vs native

| Native | Host | Class |
|---|---|---|
| Leave `0042F2A2` | `FrontendMessages.LeaveFrontendSite`; no Speak | **PROVEN** |
| `006E6150` `[+8]=self` | `TickConversations` empty | **PROVEN** first-seen |
| `006E60F0` empty walk | `ConversationWalked=0` | **PROVEN** |
| `006E61A0` insert | none (no map object) | **UNREAD** as host object; **PROVEN** no widgets |
| `00CC25FD` + `vtbl+52` `004CD1B0` | record id + `YieldOnce` | **PARTIAL** (stub, no UI) |
| leftover `+104` `00661A40` | one yield | **PROVEN** leftover intro |
| `008906C0` → `006E61A0` | session handle for InteractiveSpeak / DialogSpeak | **PARTIAL** |
| `vtbl+1472` WaitActiveDialog | one yield if session | **PARTIAL**; **DISPROVEN** as Speak |
| Invented spoken sentence | — | **DISPROVEN** |

---

## Classifications (short)

1. **`006E61A0` widgets — none. PROVEN.**
   Conversation-manager map insert from `vtbl+1456`
   `008906C0`. No `CUIDef` factory.
2. **First call — leftover InteractiveSpeak `00CC2F5B`,
   not Leave, not Speak, not first-seen no-save. PROVEN.**
3. **Leave does not call Speak. PROVEN.**
   `FrontendMessages` `0042F2A2` is fade + Init Game.
4. **First-seen no-save — `006E61A0` never entered;
   Speak never runs. PROVEN empty.**
5. **Speak apply — `004CD1B0` stub. PROVEN.**
   Not `006E61A0`. Not `vtbl+1472`.
6. **`vtbl+1472` is WaitActiveDialog, not Speak. PROVEN.**
7. **Present / voice / `PC_SUBTITLE` bind — UNREAD.**
   Later tick `006E5A00` is overlay/`SND_`, not a
   `0041D21B` factory and not this insert. Do not
   invent the father line.
