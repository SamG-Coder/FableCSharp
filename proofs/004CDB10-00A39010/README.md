# `004CDB10` first work is `00A39010` at `[0x13B8A54]`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI` / `Father.Speak`.
After Leave this walk is `FinalAlbion.wld` →
`"Init Game"` → `00418DCA` → vtbl+4 `004184BD`.
Do **not** treat later `004CDC40` / `004CEA60` /
`00A01920` / `Speak` as this site.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: `004CDB10` Init Subtitled Message first
work is `00A39010` register at `[0x13B8A54]`.
Confirm no spoken line. Host leftover Note-only
vs register?

Authority: Fable.exe dump
`listing-00400000.txt` (`004184BD` `00418605`–
`00418692`, `0041A060`–`0041A090`);
`listing-004c0000.txt` (`004CDB10`–`004CDB68`,
`004CDB70`, `004CDF91`, `004CE513` `004CDC40`,
`004CEADC` `00A01920`);
`listing-00a00000.txt` (`00A39010`–`00A39187`,
`00A38E50`, `00A01A4F`, `"enum"` / `"Unexpected
EOF in enum"`);
`listing-00980000.txt` (`0099BF30` → `0099B940`
`cmp [edx+eax*2]`);
`listing-01200000.txt` (`0121A630` `00A38500`);
`e8.tsv` dests `004CDB10` / `00A39010` /
`00A38500` / `00A38E50`;
`functions.tsv` `004184BD` callee list;
`xrefs.tsv` `"Init Subtitled Message"`
`0x0122F118` → `0041860E`;
`strings.tsv` (ASCII; wchar prefixes **absent**);
`docs/runtime/FORWARD_TREE.md` §6;
`src/Fable.Game/EngineLifecycle.cs`
(`InitGameStages` / `EnterGame`);
siblings `proofs/004CDB10-subtitled-body`,
`proofs/dialogue-first`,
`proofs/audio-initgame-first`,
`proofs/004168DC-after-graphics`.

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| First work of `004CDB10` is `00A39010` on `[0x13B8A54]`? | **Yes.** After empty CString + path join, sole work `E8` is `004CDB46 call 00A39010` with `ecx=0x13B8A54`. | **PROVEN** |
| That call is a **register / fill**, not a heap ctor? | **Yes.** Static ctor `0121A630` already `00A38500` on the BSS singleton. Init Game fills `enum` symbols. | **PROVEN** |
| Spoken / subtitled **line** queued here? | **No.** No `Speak`, no `004CDC40`, no `004CEA60`, no `00A01920`, no `vtbl+52` / `vtbl+1096`. | **DISPROVEN** as a spoken line |
| Host leftover: Note-only vs register? | Host **Note-only**. Name **MATCH**. No `00A39010`, no `[0x13B8A54]` fill. Leftover **is** the register. | **PROVEN** leftover Note-only |

**Answer:** first work is the **register**. Host
already Notes the name. Adding Note-only would
**MATCH** today. Adding real work is `00A39010`
into `[0x13B8A54]`. Do not play a line from
`"Init Subtitled Message"`.

---

## 1. Site and first work `E8`

`listing-00400000.txt`:

```
00418607  call 004168DC          ; Init Fonts
0041860D  push "Init Subtitled Message"
…
00418637  call 004CDB10          ; no ecx=esi (not game thiscall)
0041863D  push "Adding Console Variables"   ; log only
00418692  call 004CD670
```

`e8.tsv` dest `004CDB10`: **only** `00418637`.

`listing-004c0000.txt` `004CDB10`–`004CDB68`:

```
004CDB1E  call 0099EBF0          ; CString 0x122D70E (-1)
004CDB27  push 0x1239E74         ; wchar leaf (UNREAD)
004CDB30  call 0041A080          ; prefix intern 0x122F3D0
004CDB3B  call 0099BF30          ; concat leaf (0099B940 eax*2)
004CDB41  mov ecx, 0x13B8A54
004CDB46  call 00A39010          ; FIRST WORK
004CDB4F  call 0099B510          ; drop
004CDB58  call 0099B510
004CDB60  call 0099EAE0
004CDB68  ret
```

Plumbing (`0099EBF0` / `0099B510` / `0099EAE0`)
is the same CString trio as every named stage.
Path join (`0041A080` + `0099BF30`) only exists
to feed `00A39010`. **PROVEN** first work dest.

`0041A080` is the same stub as `0041A060`
(`push 0x122F3B4` / sibling of UTF-16
`"Data\Levels\"`). Prefix bytes at `0x122F3D0`
and leaf at `0x1239E74` are **UNREAD** (ASCII
`strings.tsv` skips wchar). Do not invent a
filename.

---

## 2. `00A39010` is register, not speech

`ecx = 0x13B8A54`. `ret 8`. `e8.tsv` dests:
`004CDB46`, `00A01A4F`. Second site is later
`"Sound Bank: Init Symbols"` after `"Init Sound"`
`00417A58`. First-seen is **this** call.

`listing-00a00000.txt`:

| Step | VA | Role |
|---|---|---|
| lock | `00A39900` on `this+4` | |
| clear list | `[this+20]` via `004CF810` if `[this+24]!=0` | empty then refill |
| file-stack | `0099B7D0` `ecx=0x13D27E8` | |
| parse | `00A38E50(this, buf, arg1)` | `"enum"` via `009B9BA0` |
| pop | `0099B800` `0x122D70C` | |

`00A38E50` walks `"enum"` and errors
`"Unexpected EOF in enum"`. Nearby ASCII
`"DefinitionManager : CreateSymbolsFromPathList"`
(`0x0129B20C`) names the generic loader.
**PROVEN** symbol-file fill. TLC hit vs miss
**UNREAD**. Empty table on miss still queues
**no** line.

Static ctor already ran:

```
0121A630  mov ecx, 0x13B8A54
0121A635  call 00A38500          ; vtbl 0x129CF84; list at +20
```

`e8.tsv` dest `00A38500`: `0121A635`, later
sound `00A01A0C`. Init Game does **not**
construct the object. **DISPROVEN** as a heap
ctor on this site. `dialogue-first` “construct”
is this **fill**.

---

## 3. No spoken line

| Candidate | On `004CDB10`? | Class |
|---|---|---|
| Script `Speak` `00CC25FD` / `vtbl+52` | no | **DISPROVEN** |
| Guild `vtbl+1096` / `TEXT_QST_*` | no | **DISPROVEN** |
| `004CDC40` subtitle record | later; `E8` from `004CE513` / `004CE826` | **DISPROVEN** first-seen |
| `004CEA60` / `00A01920` `"SND_"` | later; `004CEADC` | **DISPROVEN** first-seen |
| `004CDB70` `"UNKNOWN"` ctor | neighbor; zero `E8` from `004CDB10` | **DISPROVEN** |
| Later `[0x13B8A54]` `00A38420` (`004CDF91`…) | lookup, not Init Game | **DISPROVEN** as queued text |

`dialogue-first`: first-seen after Leave is
**no** spoken line. This site does not change
that. **PROVEN** register. **DISPROVEN** speech.

---

## 4. Host leftover: Note-only, not register

`InitGameStages`:

```
Init Fonts                     004168DC   ; Note + GameFontFace
Init Subtitled Message         004CDB10   ; Note only  ← this site
Init Conversation Attitude     004CD670   ; Note only
```

`EnterGame`: `Note(apply, name, "InitGame", name)`
only. No `00A39010`, no path, no `[0x13B8A54]`.
Contrast Init Fonts (`009E2C80` / `00419463`
store). Test
`Init_Fonts_004168DC_stores_ENG_ARIAL_18_at_game_plus90444`
only asserts the **name** sits after fonts.

### If we keep Note-only

Named notes **MATCH**. Leftover **on this site**
is the register (`00A39010`). Same class as
`004CD670`. Walk-first omitted child stays
`0044C6B6`.

### If we add the register

Note + `00A39010` into `[0x13B8A54]` **MATCH**
this site. Next named leftover is `004CD670`.
Walk-first hole still `0044C6B6`. Do **not**
queue a spoken line as that “real work.”

---

## Classification (VAs)

| VA | Role | Class |
|---|---|---|
| `00418637` | sole caller | **PROVEN** |
| `004CDB10` | named apply | **PROVEN** on walk |
| `00A39010` | first work; register | **PROVEN** |
| `[0x13B8A54]` | BSS singleton `this` | **PROVEN** |
| `00A38500` / `0121A630` | pre-Init ctor | **PROVEN** |
| `00A38E50` | `"enum"` parse | **PROVEN** nested; file **UNREAD** |
| `0x122F3D0` / `0x1239E74` | path wchar | **UNREAD** |
| Spoken line from this site | — | **DISPROVEN** |
| Host `EnterGame` body | Note only | **LEFTOVER** vs register |
| Host named stage | present after fonts | **MATCH** |
| `004CD670` | next named leftover | **PROVEN** site |
| Oakvale / `00DBDE40` | — | **DISPROVEN** |
