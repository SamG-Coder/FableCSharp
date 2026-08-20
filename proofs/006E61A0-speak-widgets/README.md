# Speak widgets: `006E61A0` vs `004CD1B0` on `CS_OAKVALE_INTRO_FATHER`

Investigation only. No production `src` edits.

Do **not** start New Game dialogue at Leave frontend
(`0042F2A2` / `FrontendMessages`). That path has **no**
`.Speak`. See `proofs/dialogue-first/README.md`.

Do **not** invent spoken lines. Store `TEXT_*` ids only.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE**.

Sources: `proofs/speak-vtbl1472`,
`proofs/dialogue-first`,
`src/Fable.Formats/Defs/FrontendWidgetType.cs`,
`implementer/frontend/01-widget-construction.md`,
`assembly/compiled-defs/frontend/0015-UI_DIALOG_TABLE.md`
… `0018-UI_DIALOG.md`,
`assembly/compiled-defs/names.tsv` `PC_SUBTITLE`,
ExeIndex `text-map/listing-006c0000.txt` (`006E61A0`–
`006E62B0`, `006E5A00`, `006E60F0`, `006E6150`),
`listing-00700000.txt` (**no** `006E61A0`),
`listing-004c0000.txt` (`004CD1B0`),
`listing-00880000.txt` (`008906C0`),
`listing-00cc0000.txt` Speak / InteractiveSpeak /
DialogSpeak / WaitActiveDialog,
`00-index/vtbl.tsv` `0x0127293C` slots 13 / 26,
`script-bank/0481-cs-oakvale-intro-father.md`,
`script-runtime/dialog-begin-vtbl1456-008906c0`,
`speak-apply-00cc27ea`.

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| Which frontend widgets **call** Speak on childhood Oakvale intro `CS_OAKVALE_INTRO_FATHER`? | **None.** Types `0..43` (`0041D21B`) never enter `.Speak` `00CC25FD`. | **PROVEN** empty |
| Does Speak construct a widget? | **No.** Father thing `vtbl+52` is stub `004CD1B0` `mov al,1; ret`. | **PROVEN** |
| Is `006E61A0` Speak apply? | **No.** It is conversation-manager **insert**, from context `vtbl+1456` `008906C0`. Speak uses thing `vtbl+52`. | **DISPROVEN** as Speak |
| Does `006E61A0` construct widgets? | **No.** Map insert + CString copy. No `0041D21B` / `0041DB1D` / `009AD410` / type ctors. | **PROVEN** none |
| Is `vtbl+1472` Speak? | **No.** Wait / skip / TRUE InteractiveSpeak. Intro leftover poll is `WaitActiveDialog` `00CC661D` → `008907D0`. | **DISPROVEN** as Speak |
| First-seen no-save after Leave: Speak? | **No.** Leave / first type-1 never run `00CBFB7D`. Intro Speak is later leftover. | **PROVEN** empty on Leave; **LEFTOVER** intro |
| Invented father line / subtitle sentence? | **No.** Ids only (`TEXT_QST_048_FATHER_INTRO_*`). | **DISPROVEN** |
| Widget Present of those ids? | Not in `006E61A0` / `004CD1B0`. Later tick `006E5A00` has `004CDAE0` / `00492E4B` / `SND_` `00A01920` — **not** a `CUIDef` factory. | **UNREAD** as Present |

**Answer:** no frontend widget calls Speak on
`CS_OAKVALE_INTRO_FATHER`. Speak is the actor opcode.
`006E61A0` widgets stay **none** (insert only).

---

## 1. First-seen no-save: Speak not Leave

```
0042F2A2 Leave frontend          // FrontendMessages; not Speak
0042F491 Init Game
  004CDB10 Init Subtitled        // register only
  004CD670 Init Conversation     // names only
  006E6150 conversation empty    // [node+8]=self
004189C2 first type-1
  00CE7670 00CBE87F(10)
    → TEXT_QST_LOG_STORY_10      // journal, not Speak
  Q_NewOakValeIntro inactive → yield
```

`S_QNOVI` / `00DB86B0` / `00CBFB7D` /
`Father.Speak TEXT_QST_048_FATHER_INTRO_10` are
**not** on this list. **PROVEN** (`dialogue-first`).

Later leftover:

```
NOVI_LiveFather → 00DB86B0
  → 00CBFB7D("CS_OAKVALE_INTRO_FATHER")
```

First executed leftover line is `PlayMusic MUSIC_SET_NULL`,
not Speak. First Speak in vector 0 is after `GamePause 1.6`.
**PROVEN** (`0481-cs-oakvale-intro-father.md`).

---

## 2. Speak apply is thing `vtbl+52` stub `004CD1B0`

`listing-00cc0000.txt` / `speak-apply-00cc27ea`:

```
00CC25FD  push ".Speak"
…
00CC27EA  … call [esi+52]        // apply
00CC2821  jmp 00CC2909
00CC2909  call [eax+104]         // leftover poll
```

Father vtbl `0x0127293C` (`vtbl.tsv`):

| Slot | Off | Fn | Body |
|---|---|---|---|
| 13 | `+52` | **`004CD1B0`** | `mov al, 0x01; ret` |
| 26 | `+104` | **`00661A40`** | `ret 4` (leaves `al`) |

`listing-004c0000.txt`:

```
004CD1B0  mov al, 0x01
004CD1B2  ret
```

No widget factory. No `006E61A0`. No `call [eax+1472]`.
First leftover poll is busy → one `vtbl+28` then continue.
`FirstSeenSpeakYieldsOnce=true`. Apply as UI: **UNREAD**
because the slot is a stub.

`.Speak` does **not** `call [eax+1472]`. **DISPROVEN.**

---

## 3. `vtbl+1472` is WaitActiveDialog, not Speak

Context `[0x143E8F8]` slot 368. `008907D0` `jmp 006E5660`
(`setne al` = handle still in the conversation list).

Four `call [eax+1472]` sites (`speak-vtbl1472`):

| Site | Opcode | First-seen intro |
|---|---|---|
| `00CC0148` | skip copy | **DISPROVEN** (`FirstSeenCutsceneSkipFires=false`) |
| `00CC3158` | InteractiveSpeak **TRUE** | **DISPROVEN** (intro arg is FALSE) |
| `00CC32A5` | DialogSpeak leftover poll | **PARTIAL** one leftover |
| **`00CC661D`** | **`WaitActiveDialog` `00CC656B`** | **PROVEN** opcode; dismiss **UNREAD** |

Speak never writes `[ebp-44]`. WaitActiveDialog with
`[ebp-44]==0` would `jmp 00CC7081`. Intro **does** hold
a handle from prior InteractiveSpeak / DialogSpeak, so
the leftover poll runs. Still **not** Speak apply.

---

## 4. `006E61A0` is insert, not widgets

`listing-00700000.txt` has **no** `006E61A0`. The whole
fn is `listing-006c0000.txt` `006E61A0`–`006E62B0`
(`ret 12`). Sole caller: `008906F0` inside
`008906C0` (context `vtbl+1456`).

```
008906C0  … call [eax+300]          // alive
          mov edi, [[this+4]+124]   // Script Conversation Manager
          call [thing+44]           // id
          call 006E61A0             // ecx = manager
```

`006E61A0`:

```
006E61A0  sub esp, 0x94
          inc [esi+12]              // handle = old [+12]
          … stack record, [+32]=-1
006E6207  call 006E5790             // participant; vtbl 0x1238C6C
006E6229  call 0062ED40             // CString copy
006E6237  call 006E6A30             // vector copy (00BFEA0E)
006E6275  call 006E6B60             // record copy
006E6286  call 006E6F70             // std::map insert by handle
006E628F  call 004AE0E0 ×3          // dtor temps
006E62B0  ret 12                    // eax = handle
```

Callees: `006E5790`, `0062ED40`, `006E6A30`, `006E6B60`,
`006E6F70`, `004AE0E0`. Nested: `00A01B10` / `00A01B90`
/ `00A01C10`, `004AD520`, `00BFEA0E`, `006E6EA0`.

**Not** present: `0041D21B`, `0041DB1D`, `009AD410`,
`0054F5C0` (type 6), `0052CC50` (type 5), `0054E3D0`
(type 10), `0041B800` (type 0).

`ecx` is `world+124` conversation manager (`006E6150`
ctor: sentinel node, `[+12]=1`). Insert returns the
handle InteractiveSpeak / DialogSpeak store at
`[ebp-44]`. That is a list node, not a `CUIDef`.

---

## 5. Intro leftover: who calls what (ids only)

`CCutsceneDef` 481 `this+60`
(`0481-cs-oakvale-intro-father.md`):

| After | Verb | Key | Native | Widget |
|---|---|---|---|---|
| `GamePause 1.6` | `Father.Speak Father,…` | `TEXT_QST_048_FATHER_INTRO_10` | thing `+52` `004CD1B0` | **none** |
| | `Father.InteractiveSpeak Hero,…,FALSE,…` | `_20` / `_30` | `1456` → **`006E61A0`**, then `1460`/`1464`; FALSE one `vtbl+28` | insert only |
| tired | `Father.DialogSpeak HERO,…` | `_60` | same `1456`/`1460`/`1464`; leftover `1472` | insert only |
| `GamePause 0.8` | `WaitActiveDialog` | — | `1472` `008907D0` | **not Speak** |
| | `Father.Speak Father,…` | `_70` `_80` `_90` | `+52` stub | **none** |
| | `Father.DialogadSpeak Father,…` | `_100` | `+52` stub `004CD1B0`; no yield | **none** |

Do not paste `text.big` sentences. Host may
`LookupText` those ids; tests lock the **id**.

InteractiveSpeak first-seen third arg is **FALSE**
(`FirstSeenInteractiveSpeakArgIsTrue=false`): begin
**does** call `006E61A0`, then one yield, **no**
`1472` wait loop.

---

## 6. Frontend widget types vs this path

`FrontendWidgetType` / `0041D21B` `cmp eax, 43`
`jmp [0x41D7F8+type*4]`. Factory `0041DB1D`.

frontend.bin names that *look* like dialogue:

| Name | Persist type | Role in table |
|---|---|---|
| `UI_DIALOG` | **4** Base | modal chrome |
| `UI_DIALOG_TEXT` | **6** Text | modal chrome |
| `UI_DIALOG_BUTTON` | **5** Group | modal chrome |
| `UI_DIALOG_TABLE` | **2** Table | modal chrome |
| `UI_DIALOG_OK` / `YESNO` / … | same family | frontend.bin, not Speak |

Those trees are Leave / frontend.bin. **DISPROVEN**
as callers of `00CC25FD` / `006E61A0`.

game.bin `PC_SUBTITLE` (`names.tsv` `0x524C3BB4`)
xrefs are HUD (`0055FEC0` / `00563920` / …), **not**
`006E61A0` / `004CD1B0` / `00CC27EA`. Binding that
name to intro Speak is **UNREAD** and not licensed
from this insert.

---

## 7. Later tick is not `006E61A0` widgets

`006E60F0` walks the map `006E61A0` filled and
`call 006E5A00` per node. That tick can:

- `004CDAE0` table pick, then `"SND_"` + `00A01920`
- `00492E4B` with `[0x13B86A0]+90444` font
- `007F5890` handle bind, `00753840` flag 0x43

Still **no** `0041D21B`. Voice / overlay Present on
that tick is **UNREAD** as UI. It is **not** Speak
`vtbl+52`, and it is **not** a frontend widget ctor.

First-seen after Leave: `006E60F0` walks nothing
(`[node+8]=self`). **PROVEN** empty (`dialogue-first`).

---

## 8. Host vs native

| Native | Host | Class |
|---|---|---|
| `00CC25FD` + thing `+52` `004CD1B0` | record id + `YieldOnce` | **PARTIAL** (stub, no UI) |
| leftover `+104` `00661A40` | one yield | **PROVEN** first-seen |
| `008906C0` → `006E61A0` insert | session handle for InteractiveSpeak / DialogSpeak | **PARTIAL** (no map object) |
| `vtbl+1472` WaitActiveDialog | one yield if session | **PARTIAL** |
| frontend type 0..43 call Speak | none | **PROVEN** empty |
| `006E61A0` widget ctor | none | **PROVEN** empty |
| Leave / first type-1 Speak | none | **PROVEN** empty |
| Invented spoken sentence | — | **DISPROVEN** |

---

## Classifications (short)

1. **Widgets that call Speak on `CS_OAKVALE_INTRO_FATHER` — none. PROVEN.**
   Frontend types `0..43` are not on the opcode. Speak is
   Father `vtbl+52`.
2. **Speak apply — `004CD1B0` stub. PROVEN.** No widget.
3. **`006E61A0` is not Speak and constructs no widgets. PROVEN.**
   Insert only, from `vtbl+1456` `008906C0`.
4. **`vtbl+1472` is WaitActiveDialog, not Speak. PROVEN.**
5. **First-seen no-save — Speak is leftover, not Leave. PROVEN.**
6. **Present / voice / `PC_SUBTITLE` bind — UNREAD.**
   Next unread is `006E5A00` overlay/`SND_`, not another
   opcode and not a `0041D21B` factory. Do not invent
   the father line.
