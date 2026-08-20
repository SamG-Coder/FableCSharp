# Remaining `006E61A0`: widget types constructed, first-seen no-save

Investigation only. No production `src` edits.

Do **not** invent Speak on Leave frontend
(`0042F2A2` / `FrontendMessages`). That path has **no**
`.Speak` / `004CD1B0` / `006E61A0`. See
`proofs/dialogue-first` and
`proofs/006E61A0-first-seen`.

Do **not** treat context `vtbl+1472` as Speak. That
slot is `WaitActiveDialog` poll `008907D0` →
`006E5660`. Speak apply is thing `vtbl+52` stub
`004CD1B0`. Intro leftover `CS_OAKVALE_INTRO_FATHER`
uses `.Speak`; insert is later InteractiveSpeak.

Do **not** invent spoken lines. Store `TEXT_*` ids
only.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Sources: `proofs/006E61A0-speak-widgets` (widgets
UNREAD as host Present), `proofs/006E61A0-first-seen`,
`src/Fable.Formats/Defs/FrontendWidgetType.cs`
(`ConstructFn` `0041D21B`, `FactoryFn` `0041DB1D`,
`MaxType` 43, `Table[0..43]`),
`src/Fable.Game/ScriptRuntime.cs` Speak
(`00CC25FD` → thing `vtbl+52` `004CD1B0`;
“Do not invent dialogue UI”),
`RegionTravel.cs` (`SpeakApplyStub`,
`SpeakUsesVtbl1472=false`,
`FirstSeenInteractiveSpeakArgIsTrue=false`),
`FrontendMessages.cs` (`LeaveFrontendSite`;
no Speak string),
`Scripting/ExecutionContext.cs` `Dialogue.Speak`
records id + session; no widget ctor,
ExeIndex `listing-006c0000.txt` (`006E61A0`–
`006E62B0`, `006E6150`, `006E60F0`, `006E5790`,
`006E5660`),
`listing-00700000.txt` (**no** `006E61A0`),
`listing-004c0000.txt` (`004CD1B0`),
`listing-00880000.txt` (`008906C0` / `008907D0`),
`listing-00cc0000.txt` Speak / InteractiveSpeak /
WaitActiveDialog,
`listing-00400000.txt` (`0042F2A2` Leave),
`00-index/vtbl.tsv` `0x0127293C` slot 13 /
`+52` = `004CD1B0`,
`script-bank/0481-cs-oakvale-intro-father.md`.

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| Widget types `006E61A0` constructs? | **None.** Empty set. Types `0..43` never enter this fn. No `0041D21B` / `0041DB1D` / `009AD410` / type ctors. | **PROVEN** empty |
| Named types (Button / Text / Group / Menu / Table / …)? | **None of them.** `FrontendWidgetType.Table` ctor VAs are absent from `006E61A0` and its callees. | **PROVEN** empty |
| Is `006E61A0` Speak apply? | **No.** Speak is thing `vtbl+52` `004CD1B0` `mov al,1; ret`. | **DISPROVEN** as Speak |
| Is `vtbl+1472` Speak? | **No.** `WaitActiveDialog` `00CC661D` → `008907D0` `jmp 006E5660`. | **DISPROVEN** as Speak |
| First-seen no-save: widgets / Speak / insert? | **None.** Leave never Speak. Ctor `006E6150` `[node+8]=self`; tick `006E60F0` empty. `Q_NewOakValeIntro` not constructed. | **PROVEN** empty |
| When does intro fiber first Speak? | Leftover `CS_OAKVALE_INTRO_FATHER` after `GamePause 1.6`: `Father.Speak` → `004CD1B0`. Still **no** widgets. | **LEFTOVER** |
| When is `006E61A0` first entered? | Same leftover fiber, later `InteractiveSpeak` `00CC2F5B` (FALSE) via `vtbl+1456`. Not Leave. Not Speak. | **LEFTOVER** |
| Invent Speak on Leave? | **No.** `0042F2A2` fade 500 ms then Init Game. | **DISPROVEN** |

**Answer:** remaining widget recovery is the empty
set. First-seen no-save never constructs a Speak
widget and never enters `006E61A0`. Expect insert
only on leftover intro fiber, after Speak already
ran the `004CD1B0` stub.

---

## 1. Widget types constructed — none

`listing-00700000.txt` starts at `00700002` and
has **no** `006E61A0`. Whole fn is
`listing-006c0000.txt` `006E61A0`–`006E62B0`
(`ret 12`). Sole `.text` `E8`: `008906F0` inside
context `vtbl+1456` `008906C0`.

```
006E61A0  sub esp, 0x94
006E61B8  mov edi, [esi+12]          // handle = old [+12]
006E61BB  lea eax, [edi+1]
006E61BE  mov [esi+12], eax
          … stack record, [+32]=-1
006E6207  call 006E5790              // participant; vtbl 0x1238C6C
006E6229  call 0062ED40              // CString copy
006E6237  call 006E6A30              // vector copy
006E6275  call 006E6B60              // record copy
006E6286  call 006E6F70              // std::map insert by handle
006E628F  call 004AE0E0 ×3           // dtor temps
006E62A6  mov eax, edi               // return handle
006E62B0  ret 12
```

Callees: `006E5790`, `0062ED40`, `006E6A30`,
`006E6B60`, `006E6F70`, `004AE0E0`. Nested:
`00A01B10` / `00A01B90` / `00A01C10`,
`004AD520`, `00BFEA0E`, `006E6EA0`.

`006E5790` looks up / inserts a participant
(`mov [esp+16], 0x1238C6C`). That is not a
`CUIDef`. Returned `eax` is the handle
InteractiveSpeak / DialogSpeak store at
`[ebp-44]`. List node, not a widget.

`listing-006c0000.txt` has **no**
`call 0041D21B` and **no** `call 0041DB1D`.
`009AD410` hits in that file are other fns
(`006E125D` before this insert, `006EA101`
after). **Not** in `006E61A0`–`006E62B0`.

Recovered construct set vs
`FrontendWidgetType.Table` (`cmp eax, 43`
`jmp [0x41D7F8+type*4]`):

| Type | Role | Ctor | In `006E61A0`? |
|---|---|---|---|
| 0 | Button | `0041B800` | **no** |
| 2 | Table | `005517E0` | **no** |
| 4 | Base | `005334A0` | **no** |
| 5 | Group | `0052CC50` | **no** |
| 6 | Text | `0054F5C0` | **no** |
| 10 | Menu | `0054E3D0` | **no** |
| 12 | List | `0054C3A0` | **no** |
| 16 | TextSlider | `00549F60` | **no** |
| 18 | Swap | `00547600` | **no** |
| 29 | Unused | (none) | **no** |
| 32 | Mouse | `0055C650` | **no** |
| 37 | EditBox | `005407B0` | **no** |
| 38 | AcceptButton | `00558B90` | **no** |
| 1,3,7–9,11,13–15,17,19–28,30,31,33–36,39–43 | other `Table[]` | their ctors | **no** |

`UI_DIALOG` (persist type **4** Base),
`UI_DIALOG_TEXT` (**6** Text),
`UI_DIALOG_BUTTON` (**5** Group),
`UI_DIALOG_TABLE` (**2** Table) are Leave /
frontend.bin chrome. **DISPROVEN** as products
of this insert and **DISPROVEN** as callers of
`00CC25FD` / `006E61A0`.

---

## 2. First-seen no-save — empty until intro fiber

`FrontendMessages.LeaveFrontendSite` `0042F2A2`:

```
0042F2A2  push "Leave frontend"
0042F2D3  push 0x1F4
0042F2D8  call [eax+72]              // 500 ms fade
…
0042F491  Init Game
```

No `00CC25FD`. No `004CD1B0`. No `006E61A0`.
`FrontendMessages.cs` has **no** Speak string.
Do **not** invent Speak on Leave.

Init World constructs the conversation manager
empty:

```
006E6153  push 64
006E615B  call 00BFEA0E              // sentinel
006E6177  mov [eax+8], eax           // [node+8]=self
006E6182  mov [esi+12], 1            // next handle
```

Type-1 tick `006E60F0`:

```
006E60F4  mov eax, [edi]
006E60F6  mov esi, [eax+8]
006E60F9  cmp esi, eax
006E60FB  je 006E6140                // empty → ret
…
006E6137  call 006E5A00              // not reached
```

Host `TickConversations` notes
`006E6150 [+8]=self` / `006E60F0 empty`.
`ConversationWalked=0`. **PROVEN.**

No-save also does **not** construct
`S_QNOVI` (`proofs/00DAAC00-sqnovi-no-save`).
Gameflow `00893610("Q_NewOakValeIntro")` miss
→ yield. `00CBFB7D` /
`CS_OAKVALE_INTRO_FATHER` are **not** on this
list. First Gameflow text is journal
`00CBE87F(10)` → `TEXT_QST_LOG_STORY_10`,
not Speak.

So first-seen no-save: **zero** widget types
from this insert, **zero** Speak apply, **zero**
`006E61A0` enters. Expect **not** until intro
fiber.

---

## 3. Intro fiber uses Speak — still no widgets

Leftover, after `NOVI_LiveFather` → `00DB86B0`
→ `00CBFB7D("CS_OAKVALE_INTRO_FATHER")`.
First executed line is `PlayMusic MUSIC_SET_NULL`,
not Speak (`0481-cs-oakvale-intro-father.md`).

`ScriptRuntime` Speak: thing `vtbl+52` then poll
`vtbl+104`. Father `0x0127293C` slot 13 /
`+52` is `004CD1B0`. Host records the id and
yields once. Comment: “Do not invent dialogue
UI.”

```
00CC25FD  push ".Speak"
…
00CC27EA  push [ebp-564]
          push 1
          push 0
          push [ebp+20]
00CC2813  call [esi+52]              // 004CD1B0
00CC2821  jmp 00CC2909
00CC2909  call [eax+104]             // 00661A40 ret 4
```

```
004CD1B0  mov al, 0x01
004CD1B2  ret
```

No widget factory. No `006E61A0`. No
`call [eax+1472]`. First leftover Speak is
after `GamePause 1.6`:
`Father.Speak Father,'TEXT_QST_048_FATHER_INTRO_10'`.
Ids only.

`vtbl+1472` is **not** that apply. Four runner
sites; intro leftover poll is
`WaitActiveDialog`:

```
00CC656B  push "WaitActiveDialog"
00CC65BB  cmp [ebp-44], edi
00CC65BE  je 00CC7081                // no handle → continue
00CC661D  call [eax+1472]            // 008907D0
```

```
008907D0  mov eax, [ecx+4]
008907D3  mov ecx, [eax+124]
008907D6  jmp 006E5660               // setne al = handle in list
```

Speak never writes `[ebp-44]`. WaitActiveDialog
with a prior InteractiveSpeak handle polls
`1472`. Still **not** Speak, still **not** a
widget ctor.

First `006E61A0` on leftover intro is
InteractiveSpeak begin (third arg **FALSE**),
**after** Speak already ran the stub:

```
00CC2F50  push 1
          push 1
          lea edx, [ebp-1364]
00CC2F5B  call [eax+1456]            // 008906C0 → 006E61A0
00CC2F73  mov [ebp-44], esi
…
00CC3100  jmp 00CC707C               // FALSE: one vtbl+28, no 1472
```

Insert still constructs **no** widget types.

| After | Verb | Key | Native | Widgets |
|---|---|---|---|---|
| `GamePause 1.6` | `Father.Speak` | `TEXT_QST_048_FATHER_INTRO_10` | `+52` `004CD1B0` | **none** |
| `GamePause 1.0` | `Father.InteractiveSpeak … FALSE` | `_20` / `_30` | `1456` → **`006E61A0`** | **none** (insert) |
| tired | `Father.DialogSpeak` | `_60` | same `1456` | **none** (insert) |
| `GamePause 0.8` | `WaitActiveDialog` | — | `1472` `008907D0` | **not Speak** |
| | `Father.Speak` | `_70` `_80` `_90` | `+52` stub | **none** |
| | `Father.DialogadSpeak` | `_100` | `+52` stub | **none** |

---

## 4. Host vs native (remaining)

| Native | Host | Class |
|---|---|---|
| `006E61A0` widget ctor | none; no type `0..43` | **PROVEN** empty |
| Leave `0042F2A2` Speak | `FrontendMessages`; no Speak | **PROVEN** / **DISPROVEN** invent |
| `006E6150` / `006E60F0` empty | `TickConversations` `Walked=0` | **PROVEN** first-seen |
| `00CC25FD` + `004CD1B0` | record id + `YieldOnce` | **PARTIAL** (stub, no UI) |
| `008906C0` → `006E61A0` | session handle | **PARTIAL** (no map object) |
| `vtbl+1472` WaitActiveDialog | one yield if session | **PARTIAL**; **DISPROVEN** as Speak |
| Invented spoken sentence | — | **DISPROVEN** |

---

## Classifications (short)

1. **Widget types constructed by `006E61A0` — none. PROVEN.**
   Remaining recovery is the empty set. Types
   `0..43` / `0041D21B` never run here.
2. **First-seen no-save — not until intro fiber. PROVEN empty.**
   Leave does not Speak. Map is sentinel-only.
   `006E61A0` is never entered.
3. **Intro leftover Speak is `004CD1B0`, not insert. PROVEN.**
   `CS_OAKVALE_INTRO_FATHER` uses `.Speak` after
   `GamePause 1.6`. Still no widgets.
4. **First `006E61A0` is leftover InteractiveSpeak. PROVEN.**
   After Speak stub. FALSE arg; no `1472`.
5. **`vtbl+1472` is WaitActiveDialog, not Speak. PROVEN.**
6. **Present / voice / `PC_SUBTITLE` bind — UNREAD.**
   Later tick `006E5A00` is overlay/`SND_`, not a
   `0041D21B` factory and not this insert. Do not
   invent the father line. Do not invent Speak on
   Leave.
