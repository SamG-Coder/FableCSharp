# Intro leftover: Speak UI `vtbl+1472` (`0x5C0`)

Investigation only. No production `src` edits.

Do **not** start New Game dialogue at Leave frontend
(`0042F2A2` / `FrontendMessages`). That path has **no**
`.Speak` / `.InteractiveSpeak` / `.DialogSpeak`. See
`proofs/dialogue-first/README.md`.

Do **not** invent spoken lines. Store `TEXT_*` ids only.
`ScriptRuntime.LookupText` may fill `ResolvedBody` from
`text.big`; missing body stays empty.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Sources: `src/Fable.Game/ScriptInterpreter.cs`,
`ScriptRuntime.cs`, `RegionTravel.cs`, `FrontendMessages.cs`,
`ScriptCommandMap.cs`, `Scripting/EntityDispatcher.cs`,
`Scripting/GlobalDispatcher.cs`, `Scripting/ExecutionContext.cs`,
`proofs/dialogue-first`, `proofs/cutscene-first`,
`proofs/audit-scriptinterpreter`,
`docs/status/README.md` leftover “Dialogue UI”,
`docs/runtime/COMMAND_MAP.md` / `COMMAND_MAP.generated.md`,
`WorldSceneTests` / `ScriptRuntimeArchitectureTests`,
ExeIndex `text-map/listing-00cc0000.txt`,
`listing-00880000.txt` (`008907D0`),
`script-runtime/speak-token-00cc25fd`,
`speak-apply-00cc27ea`, `dialog-wait-vtbl1472-008907d0`,
`dialog-wait-body-006e5660`, `dialog-begin-vtbl1456-008906c0`,
`dialog-line-vtbl1464-00890750`,
`script-bank/0481-cs-oakvale-intro-father.md`,
`strings.tsv` `0x012C2498` `.Speak`.

---

## Verdict

| Item | Answer | Class |
|---|---|---|
| Speak opcode in `00CBFB7D` | **`00CC25FD`** `push ".Speak"` (`0x012C2498`). Match `00BFEAF8`. Apply **`00CC27EA`**. | **PROVEN** |
| Speak uses `vtbl+1472`? | **No.** Thing `vtbl+52` then leftover poll thing `vtbl+104` at `00CC2909`. | **DISPROVEN** |
| `vtbl+1472` (`0x5C0`) consumer | Context `[0x143E8F8]` `call [eax+1472]` → **`008907D0`** `jmp 006E5660`. Four sites in this runner. | **PROVEN** sites; **PARTIAL** UI |
| After Leave | No `00CBFB7D`. Frontend is gone (`FrontendMessages.LeaveFrontendSite`). No spoken UI. | **PROVEN** empty |
| Intro leftover | `CS_OAKVALE_INTRO_FATHER` def+60 keys `TEXT_QST_048_FATHER_INTRO_*`. Host records session + one yield. | **LEFTOVER** |
| Host gap | Dialogue **UI / dismiss / voice**. Not parse. Not the `TEXT_*` id. | **PARTIAL** / **UNREAD** |

**Opcode site = `00CC25FD`** (Speak) inside runner `00CBFB7D`.

**Host gap = Present of the line.** `EntityDispatcher` queues
`Dialogue.Speak` / `LookupText(id)` and `YieldOnce`
(`"Speak vtbl+52 leftover vtbl+104"`). No widget, no
`008907D0` poll body, no invented subtitle.

---

## 1. After Leave vs intro

```
0042F2A2 Leave frontend          // FrontendMessages; not Speak
0042F491 Init Game
  004CDB10 Init Subtitled        // register only
  004CD670 Init Conversation     // names only
  006E6150 conversation empty
  006E3EC0 speech-gain empty
004189C2 first type-1
  00CE7670 00CBE87F(10)
    → TEXT_QST_LOG_STORY_10      // journal, not Speak
  Q_NewOakValeIntro inactive → yield
```

`S_QNOVI` / `00DB86B0` / `00CBFB7D` /
`Father.Speak TEXT_QST_048_FATHER_INTRO_10` are **not** on
this list. **PROVEN** (`dialogue-first`, `cutscene-first`).

Later leftover:

```
NOVI_LiveFather → 00DB86B0
  → 00CBFB7D("CS_OAKVALE_INTRO_FATHER")
```

First executed leftover line is `PlayMusic MUSIC_SET_NULL`,
not Speak. First Speak in vector 0 is after `GamePause 1.6`.
**PROVEN** (`0481-cs-oakvale-intro-father.md`,
`WorldSceneTests`).

---

## 2. Opcode site (`00CBFB7D` → `00CC25FD`)

Runner walks persist CStrings (`00CC0205` … `00BFEAF8`).
Token:

| VA | String |
|---|---|
| `0x012C2498` | `.Speak` |
| `0x00CC25FD` | `push ".Speak"` |

`listing-00cc0000.txt`:

```
00CC25FD  push ".Speak"
00CC2602  lea ecx, [ebp-424]
00CC2608  call 0099EBF0
…
00CC262E  call 00BFEAF8
00CC264A  cmp [ebp+127], 0x00
00CC264E  je 00CC2939            // miss → .DataSpeak
00CC2654  test ebx, ebx
00CC2656  je 00CC7081            // empty actor
00CC266C  lea ecx, [ebp+44]
00CC266F  call 00403A00          // text
00CC2676  je 00CC7081
00CC267C  lea ecx, [ebp+44]
00CC267F  call 00CBEE5E          // strcmp "null"
00CC2686  jne 00CC7081
00CC27EA  … call [esi+52]        // apply 00CC27EA
00CC2821  jmp 00CC2909
00CC2909  call [eax+104]         // leftover poll
00CC2910  test al, al
00CC2912  jne 00CC28C4           // yield vtbl+28
```

Args: listener `[ebp+40]`, text `[ebp+44]`, optional
`00CBEDBA` hold → context `vtbl+1484(1)`, optional mode
`random=1` / `norepeat=2` / `sequence=3` at `[ebp+20]`.
First-seen intro line is target+text only (`mode=0`).
**PROVEN.**

Apply `00CC27EA`: persist `00CD3187` or thing lookup, then
thing **`vtbl+52(text, mode, 0, 1)`**. Father vtbl
`0x0127293C` +52 is **`004CD1B0`** `mov al,1; ret`.
Poll **`vtbl+104`** is **`00661A40`** `ret 4` (leaves `al`).
First leftover poll is busy → one `vtbl+28` then continue.
`FirstSeenSpeakYieldsOnce=true`. **PROVEN** yield.
Apply body as UI: **UNREAD** (stub).

`.Speak` does **not** `call [eax+1472]`. **DISPROVEN.**

---

## 3. Intro keys (no invented dialogue)

`CCutsceneDef` index 481, `this+60`. Dump
`script-bank/0481-cs-oakvale-intro-father.md`.

| Line | Verb | Key |
|---|---|---|
| after `GamePause 1.6` | `Father.Speak Father,…` | `TEXT_QST_048_FATHER_INTRO_10` |
| | `Father.InteractiveSpeak Hero,…,FALSE,…` | `_20` prompt, `_30` extra |
| after tired | `Father.DialogSpeak HERO,…` | `_60` |
| after `WaitActiveDialog` | `Father.Speak Father,…` | `_70` `_80` `_90` |
| | `Father.DialogadSpeak Father,…` | `_100` |

`WaitActiveDialog` sits after `GamePause 0.8` (post-Create /
`VILL1.WalkTo`). First-seen InteractiveSpeak third arg is
**FALSE** (`FirstSeenInteractiveSpeakArgIsTrue=false`).
**PROVEN.**

Do not paste `text.big` sentences here. Host may
`LookupText` those ids; tests lock the **id**, not a
made-up line.

---

## 4. `vtbl+1472` consumer (`0x5C0`)

Context object `[0x143E8F8]`. Slot **368**. Nearby
`CGameScriptInterface` dialog slots:

| Off | VA | Fn | Role |
|---|---|---|---|
| 1456 | `00CC2F5B` / `00CC32CA` | `008906C0` | begin handle → `006E61A0` |
| 1460 | | `00890710` | bind |
| 1464 | | `00890750` | line → `006E5950` |
| 1468 | `00CC015F` | `008907C0` | close; `jmp 006E5990` |
| **1472** | **`call [eax+1472]`** | **`008907D0`** | **wait; `jmp 006E5660`** |
| 1488 | fade | `008907E0` | not Speak |

`listing-00880000.txt`:

```
008907D0  mov eax, [ecx+4]
008907D3  mov ecx, [eax+124]
008907D6  jmp 006E5660
```

`006E5660`: `006E69E0` then `cmp edx,ecx` / `setne al` /
`ret 4`. Busy = handle still in the conversation list.
**Not** a draw. **PROVEN** as poll. UI widgets **UNREAD**.

Four `call [eax+1472]` sites in `listing-00cc0000.txt`
(the `lea …1472` hits are stack offsets, not this slot):

### 4a. Skip copy `00CC0148` — not first-seen

```
00CC012E  call 00CBEB7E
00CC0133  test al, al
00CC0135  je 00CC0196
00CC0137  cmp [ebp-21], 0x00
00CC013B  jne 00CC0196
00CC0148  call [eax+1472]         // push [ebp-44] handle
00CC014E  test al, al
00CC0150  je 00CC0165
00CC015F  call [eax+1468]         // (1, handle)
00CC016D  call [eax+28]
00CC017C  copy def+72
```

`FirstSeenCutsceneSkipFires=false`. **DISPROVEN** first-seen.

### 4b. InteractiveSpeak TRUE `00CC3158`

Token `00CC2EAA`. Apply `00CC2F50` `vtbl+1456(1,1)` stores
handle `[ebp-44]`, then `1460` / `1464`. Arg2 `00CBEDBA`:

- **FALSE** (intro): one `vtbl+28`, `jmp 00CC707C`.
  **Does not** poll `1472`. **PROVEN.**
- **TRUE**: loop `00CC3158 call [eax+1472]` until `al==0`.
  Host `WaitOperation` `"InteractiveSpeak TRUE vtbl+1472"`.
  Intro does not take this arm.

### 4c. DialogSpeak leftover `00CC32A5`

Token `00CC3165`. Apply `00CC31BC`. If `[ebp-44]!=0`
(handle from a prior InteractiveSpeak/DialogSpeak):

```
00CC32A5  call [eax+1472]
00CC32AB  test al, al
00CC32AD  jne 00CC32B2
00CC32AF  mov [ebp-44], edi        // idle → drop
```

Then `vtbl+1456/1460/1464`, one `vtbl+28`, `jmp 00CC707C`.
Intro DialogSpeak `_60` runs **after** InteractiveSpeak
FALSE, so `[ebp-44]` may still hold that handle. Poll is
**PARTIAL** (one leftover). Line body `006E5950` **UNREAD**
as UI. `COMMAND_MAP`: “`1472` unread UI”.

### 4d. WaitActiveDialog `00CC661D`

Token `00CC656B`. If `[ebp-44]==0` → `00CC7081` (Speak
never writes the handle). Else:

```
00CC6612  push [ebp-44]
00CC661D  call [eax+1472]
00CC6623  test al, al
00CC6625  jne 00CC65C6            // leftover vtbl+28
00CC6627  jmp 00CC7081
```

Intro **does** run `WaitActiveDialog` after the
InteractiveSpeak/DialogSpeak session. Host:
`WaitActiveDialog leftover vtbl+1472`, one yield,
`WaitActiveDialogCount++`. Dismiss **UNREAD**.
`FirstSeenWaitActiveDialogYieldsOnce=true`. **PROVEN**
opcode; **PARTIAL** runtime.

---

## 5. Host vs native

| Native | Host | Class |
|---|---|---|
| `00CC25FD` parse + skip empty/`null` | `ScriptLine` / `ParseSpeak` | **PROVEN** |
| thing `vtbl+52` `004CD1B0` | `Dialogue.Speak` record + `LookupText` | **PARTIAL** (no stub call as UI) |
| leftover `vtbl+104` one yield | `YieldOnce` | **PROVEN** first-seen |
| `00CC2EAA` FALSE one `vtbl+28` | `YieldOnce` `"InteractiveSpeak FALSE vtbl+28"` | **PROVEN** |
| `00CC3165` `1456/1460/1464` + leftover `1472` | session `HasHandle=true`; no `008907D0` | **PARTIAL** |
| `00CC656B` poll `1472` | one yield if session active | **PARTIAL** |
| `008907D0` / `006E5660` `setne` | none | **UNREAD** as UI |
| `006E61A0` / `006E5950` insert/line | none | **UNREAD** |
| Draw / subtitle / voice | none (`ResolvedBody` is storage only) | **DISPROVEN** present |
| `FrontendMessages` after Leave | no Speak | **PROVEN** |
| Invented spoken sentence | — | **DISPROVEN** |

`docs/status/README.md`: Dialogue UI leftover is
“one yield; no invented UI”. Keep that.

---

## Classifications (short)

1. **Speak opcode site — `00CC25FD` in `00CBFB7D`. PROVEN.**
   Apply `00CC27EA` thing `vtbl+52`; leftover `vtbl+104`.
2. **`vtbl+1472` is not Speak apply. PROVEN.** Consumers:
   skip `00CC0148`, InteractiveSpeak TRUE `00CC3158`,
   DialogSpeak leftover `00CC32A5`, WaitActiveDialog
   `00CC661D`. Fn `008907D0` → `006E5660`.
3. **After Leave — no Speak UI. PROVEN empty.**
4. **Intro leftover — `TEXT_QST_048_FATHER_INTRO_*` ids.
   PROVEN keys. PARTIAL UI.**
5. **Host gap — Present / dismiss / voice. UNREAD.**
   Do not invent the father line.

Next unread on this leftover: `006E5950` / `006E61A0`
bodies as widgets, not another opcode in `00CBFB7D`.
