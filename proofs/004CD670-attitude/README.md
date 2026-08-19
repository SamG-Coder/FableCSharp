# `004CD670` Init Conversation Attitude after `004CDB10`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave this
walk is `FinalAlbion.wld` → `"Init Game"` →
`00418DCA` → vtbl+4 `004184BD`. Do **not** treat
later getters `004CD9A0` / `004CD9B0` /
`004CD9C0` or script `Speak` as this site.
Do **not** re-prove `004CDB10` / `00A39010`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: After Init Game `004184BD` logs
`"Init Subtitled Message"` and calls `004CDB10`,
the next named apply is `00418692` `call 004CD670`
`"Init Conversation Attitude"`. First-seen body?
First-seen callees? Does it bind `STANDARD_TALK_*`
strings? Host `EngineLifecycle.EnterGame` only
Notes the name — leftover vs real work?

Authority: Fable.exe dump
`listing-00400000.txt` (`004184BD` `00418637`–
`00418692`);
`listing-004c0000.txt` (`004CD670`–`004CD999`,
`004CD9A0` / `004CD9B0` / `004CD9C0`);
`listing-00980000.txt` (`0099EFE0`–`0099F02E`);
`listing-01200000.txt` (`0121A460` / `0121A4F0` /
`0121A580`, dtors `01228D00` / `01228D40` /
`01228D80`);
`e8.tsv` dest `004CD670` / sites `004CD688`–
`004CD98D`;
`functions.tsv` `004184BD` callee list (no own
row for `004CD670`);
`xrefs.tsv` `"Init Conversation Attitude"` /
`STANDARD_TALK_*` / `CONVERSATION_*`;
`strings.tsv` `STANDARD_TALK_*` / `CONVERSATION_*`
(ASCII; empty `0x122D70E` and 4-char `"NULL"`
**absent**);
`src/Fable.Game/EngineLifecycle.cs`
(`InitGameStages` / `EnterGame`);
siblings `proofs/004CDB10-00A39010`,
`proofs/004CDB10-subtitled-body`,
`proofs/dialogue-first`.

---

## Verdict

**Yes: on `004184BD`, `004CD670` is the next named
apply after `004CDB10`.** Parent logs
`"Adding Console Variables"` (`0041863D`, log
only) then `"Init Conversation Attitude"` then
`00418692 call 004CD670` with **no** `ecx=game`.
Sole `E8` of `004CD670` is `00418692`.

Body is **not** the log trio. If `[0x13B8A28]==0`
it writes ASCII names into three CRT-allocated
CString arrays at `[0x13B8A2C]` (18 slots),
`[0x13B8A38]` (12), `[0x13B8A44]` (12) via
`0099EFE0`, then sets `[0x13B8A28]=1`. That is
**real work**. No spoken line is queued.

It **does** bind every `STANDARD_TALK_*` string
in `strings.tsv`. It also binds `CONVERSATION_*`
and one listing `"NULL"`. Slot order below is
the listing push order, **not** a named enum
table.

Host already has the **name** in `InitGameStages`
(after `"Init Subtitled Message"`). `EnterGame`
only `Note(0x0041863D, "Adding Console Variables")`
then `Note(apply)`. No `0099EFE0`, no
`STANDARD_TALK_*`, no `[0x13B8A2C]` fill.
**LEFTOVER** Note-only.

| Claim | Class |
|---|---|
| `00418692` `call 004CD670` after `00418637` `004CDB10` (console-var log in between) | **PROVEN** |
| `ecx=game` thiscall like fonts | **DISPROVEN** |
| Other `E8` of `004CD670` | **DISPROVEN** — only `00418692` |
| Host `InitGameStages` notes the name | **PROVEN** **MATCH** |
| Host `EnterGame` runs the body | **DISPROVEN** — **LEFTOVER** Note-only |
| Body is log-only (`0041863D` class) | **DISPROVEN** |
| Work is `0099EFE0` fills of `[0x13B8A2C]` / `[0x13B8A38]` / `[0x13B8A44]` then `[0x13B8A28]=1` | **PROVEN** |
| Binds `STANDARD_TALK_*` | **PROVEN** |
| Queues a spoken / subtitled line | **DISPROVEN** (`dialogue-first`) |
| Nested under `004CDB10` | **DISPROVEN** — sibling |
| `functions.tsv` own row for `004CD670` | **DISPROVEN** — no frame prologue |

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Next named apply after `004CDB10`? | **Yes.** `00418637` then log `"Adding Console Variables"` `0041863D` (no apply `E8`) then log `"Init Conversation Attitude"` then `00418692`. Next named sibling is `"Init Player Manager"` `0041732A`. | **PROVEN** |
| First-seen body? | Gate `[0x13B8A28]`; if clear, `0099EFE0` ×42 into three BSS vectors; set flag. | **PROVEN** |
| First-seen callees? | Direct `E8`: **only** `0099EFE0` (42 sites). Nested first-seen: `0099E9B0` (live slot), `00BFEA1A`(17), `009A0590`. | **PROVEN** dest; nested **PROVEN** on `0099EFE0` |
| Bind `STANDARD_TALK_*`? | **Yes.** All eight `strings.tsv` names are pushed. Also `CONVERSATION_*` and listing `"NULL"`. | **PROVEN** |
| Host Note-only leftover? | **Yes.** Name present; body omitted. Contrast `"Init Subtitled Message"` which now Notes `00A39010`. | **PROVEN** |
| Real work? | **Yes.** CString assign into CRT arrays. Not log-only. First-seen *use* of the arrays is **UNREAD** on this walk. | **PROVEN** work; consumers **UNREAD** |

**Answer:** first leftover **on this named site**
is the body (`0099EFE0` binds). Adding Note-only
would **MATCH** what host already does. Adding
real work closes this site; next named leftover
is `"Init Player Manager"` `0041732A` (host
already `EnsurePlayerManagerSingleton` earlier —
do not assume that MATCH here). Do not start
Oakvale. Do not invent slot meanings.

---

## 1. Site: sibling after `004CDB10`

`listing-00400000.txt`:

```
00418637  call 004CDB10          ; Init Subtitled Message — prior named apply
0041863C  push edi
0041863D  push "Adding Console Variables"
00418642  lea ecx, [ebp-8]
00418645  call 0099EBF0
…
0041865A  call 009E9F40
00418662  call 0099EAE0          ; log only — no apply E8
00418667  push edi
00418668  push "Init Conversation Attitude"
0041866D  lea ecx, [ebp-8]
00418670  call 0099EBF0
…
00418685  call 009E9F40
0041868D  call 0099EAE0
00418692  call 004CD670          ; THIS SITE — no ecx=esi
00418697  push edi
00418698  push "Init Player Manager"
…
004186E2  mov ecx, esi
004186E4  call 0041732A
```

`esi` is game. `004184BD` does **not** pass it
into `004CD670`.

`e8.tsv` dest `004CD670`: **only** `00418692`.

`xrefs.tsv`: `0x0122F0E0` → `00418669`
`"Init Conversation Attitude"`.
`"Adding Console Variables"` is `0x0122F0FC` →
`0041863E`.

`functions.tsv` `004184BD` callee list:
`…,004CDB10,0099EBF0,009E9F40,0099EAE0,0099EBF0,009E9F40,0099EAE0,004CD670,0099EBF0,009D8240,…`.
`004CD670` itself has **no** `functions.tsv` row
(starts `mov al,[0x13B8A28]`, not a frame
prologue). **PROVEN**.

---

## 2. First-seen body

`listing-004c0000.txt` `004CD670`–`004CD999`
(int3 pad after `ret`):

```
004CD670  mov al, [0x13B8A28]
004CD675  test al, al
004CD677  jne 004CD999          ; already filled → ret
004CD67D  mov ecx, [0x13B8A2C]
004CD683  push "STANDARD_TALK_GENERIC"
004CD688  call 0099EFE0
… 17 more [0x13B8A2C]+n slots …
004CD7D0  mov ecx, [0x13B8A38]
004CD7D6  push "NULL"
004CD7DB  call 0099EFE0
… 11 more [0x13B8A38]+n slots …
004CD8B1  mov ecx, [0x13B8A44]
004CD8B7  push 0x122D70E
004CD8BC  call 0099EFE0
… 11 more [0x13B8A44]+n slots …
004CD992  mov [0x13B8A28], 0x01
004CD999  ret
```

`[0x13B8A28]` is written **only** at `004CD992`
in the dump (grep of `out/`). BSS starts 0, so
first Init Game takes the fill arm. **PROVEN**.

No heap ctor, no file I/O, no `Speak`, no
`004CDC40`. **DISPROVEN** as a spoken line.

---

## 3. First-seen callees

`e8.tsv` sites in `004CD670` (`004CD688`–
`004CD98D`): **42** rows, dest **only**
`0099EFE0`.

`0099EFE0` (`listing-00980000.txt`):

```
0099EFE0  push ebx
0099EFE1  mov ebx, ecx           ; CString* slot
0099EFE3  cmp [ebx], 0
0099EFE9  call 0099E9B0          ; if live, release
0099EFEE  mov edi, [esp+12]      ; pushed ASCII
0099EFF2  test edi, edi
0099EFF6  cmp [edi], 0x00
0099EFFB  push 17
0099EFFE  call 00BFEA1A          ; alloc 0x11
0099F00C  push edi
0099F00F  call 009A0590          ; copy bytes
0099F014  mov [esi+13], 0x1
0099F01B  mov [ebx], esi
          ret 4
```

CRT `0121A460` / `0121A4F0` / `0121A580` already
filled the slots with empty CStrings
(`0099EC30` from `0x122D70E`), so first-seen
`0099EFE0` takes the live-slot arm (`0099E9B0`)
then replaces. **PROVEN** shape.
`009A0590` byte-copy internals **UNREAD** beyond
that store.

Neighbors `004CD9A0` / `004CD9B0` / `004CD9C0`
are `lea eax,[base+ecx*4]; ret` indexers. Zero
`E8` from `004CD670`. `e8.tsv` dest `004CD9A0`:
**none**. Dest `004CD9B0`: `00842419`. Dest
`004CD9C0`: `008D924A` / `008D92A1` /
`008D92E1` / `008D9335` / `008D9397`. Those
sites are **not** this Init Game apply.
**DISPROVEN** first-seen.

---

## 4. `STANDARD_TALK_*` and listing order

`strings.tsv` (all eight; **no** other
`STANDARD_TALK_*`):

| VA | String |
|---|---|
| `0x01239E5C` | `STANDARD_TALK_GENERIC` |
| `0x01239E48` | `STANDARD_TALK_LOVE` |
| `0x01239E34` | `STANDARD_TALK_FEAR` |
| `0x01239E20` | `STANDARD_TALK_ANGER` |
| `0x01239E0C` | `STANDARD_TALK_GLEE` |
| `0x01239DF4` | `STANDARD_TALK_SADNESS` |
| `0x01239DDC` | `STANDARD_TALK_RIDICULE` |
| `0x01239DC4` | `STANDARD_TALK_FRIENDLY` |

`xrefs.tsv` `fn=0x004CD670` hits those plus
`CONVERSATION_*` (`0x01239D28`–
`0x01239DB0`). **PROVEN**.

Observed first-seen pushes (listing order;
offsets are `add ecx, n` after `mov ecx,[base]`).
**Not** a named bind table.

`[0x13B8A2C]` — CRT `0121A460` `push 72` /
`edi=0x12` (18 slots):

| + | String |
|---|---|
| 0 | `STANDARD_TALK_GENERIC` |
| 4 | `STANDARD_TALK_GENERIC` |
| 8 | `STANDARD_TALK_LOVE` |
| 12 | `STANDARD_TALK_GENERIC` |
| 16 | `STANDARD_TALK_GENERIC` |
| 20 | `STANDARD_TALK_FEAR` |
| 24 | `STANDARD_TALK_ANGER` |
| 28 | `STANDARD_TALK_ANGER` |
| 32 | `STANDARD_TALK_ANGER` |
| 36 | `STANDARD_TALK_ANGER` |
| 40 | `STANDARD_TALK_LOVE` |
| 44 | `STANDARD_TALK_GENERIC` |
| 48 | `STANDARD_TALK_ANGER` |
| 52 | `STANDARD_TALK_FEAR` |
| 56 | `STANDARD_TALK_ANGER` |
| 60 | `STANDARD_TALK_ANGER` |
| 64 | `STANDARD_TALK_LOVE` |
| 68 | `STANDARD_TALK_GENERIC` |

`[0x13B8A38]` — CRT `0121A4F0` `push 48` /
`edi=0xC` (12 slots):

| + | String |
|---|---|
| 0 | `"NULL"` (listing; VA **UNREAD** in `strings.tsv`) |
| 4 | `STANDARD_TALK_GENERIC` |
| 8 | `STANDARD_TALK_LOVE` |
| 12 | `STANDARD_TALK_ANGER` |
| 16 | `STANDARD_TALK_FEAR` |
| 20 | `STANDARD_TALK_GLEE` |
| 24 | `STANDARD_TALK_SADNESS` |
| 28 | `STANDARD_TALK_RIDICULE` |
| 32 | `STANDARD_TALK_FRIENDLY` |
| 36 | `STANDARD_TALK_RIDICULE` |
| 40 | `STANDARD_TALK_GENERIC` |
| 44 | `STANDARD_TALK_GENERIC` |

`[0x13B8A44]` — CRT `0121A580` `push 48` /
`edi=0xC` (12 slots):

| + | String |
|---|---|
| 0 | `0x122D70E` empty (between `BTRUE` `0x0122D704` and `TEXT_GUI_WINLOGO_…` `0x0122D710`; ASCII `strings.tsv` skips) |
| 4 | `CONVERSATION_HAPPY` |
| 8 | `CONVERSATION_HAPPY` |
| 12 | `CONVERSATION_ANGRY` |
| 16 | `CONVERSATION_FEAR` |
| 20 | `CONVERSATION_HAPPY` |
| 24 | `CONVERSATION_SAD` |
| 28 | `CONVERSATION_BELLIGERENT` |
| 32 | `CONVERSATION_HAPPY` |
| 36 | `CONVERSATION_BELLIGERENT` |
| 40 | `CONVERSATION_CAGED` |
| 44 | `CONVERSATION_HOLDING_SWORD` |

`"NULL"` VA **UNREAD**: listing decodes the
immediate; `strings.tsv` has `NULL%` /
`NULLDEF_` / `NULL File Object` but no 4-char
`NULL`. Do not invent `0x01239438`.

Do **not** name the three arrays or map slot
index → attitude enum. That would be invention.

---

## 5. Host leftover: Note-only, not bind

`InitGameStages` (`EngineLifecycle.cs`):

```
Init Subtitled Message         004CDB10   ; Note + 00A39010 path (sibling)
Init Conversation Attitude     004CD670   ; Note only  ← this site
Init Player Manager            0041732A
```

`EnterGame` for this name:

```
if (name == "Init Conversation Attitude")
    Note(0x0041863D, "InitGame", "InitGame", "Adding Console Variables");
Note(apply, name, "InitGame", name);
```

No `0099EFE0`. No `STANDARD_TALK_*`. No
`[0x13B8A28]` / `[0x13B8A2C]`. **PROVEN**
Note-only. Name **MATCH**. Leftover **is**
the bind.

Sibling proofs that still say `"Init Subtitled
Message"` is Note-only are **stale** vs current
`EnterGame` (`SubtitledSymbolsRegistered`).
This site is still Note-only. **MATCH** that
narrow claim.

### If we keep Note-only

Named notes **MATCH**. Leftover **on this site**
is the `0099EFE0` fill.

### If we add the bind

Note + `0099EFE0` into the three BSS vectors
**MATCH** this site. Do **not** play a line
from `"Init Conversation Attitude"`. Do **not**
ship a guessed enum for the 18/12/12 slots.

---

## Classification (VAs)

| VA | Role | Class |
|---|---|---|
| `00418637` | prior named apply `004CDB10` | **PROVEN** site (body not re-proved) |
| `0041863D` | `"Adding Console Variables"` log only | **PROVEN** |
| `00418692` | sole caller of `004CD670` | **PROVEN** |
| `004CD670` | named apply | **PROVEN** on walk |
| `0099EFE0` | sole work dest; CString assign | **PROVEN** |
| `[0x13B8A28]` | once-flag | **PROVEN** |
| `[0x13B8A2C]` / `[0x13B8A38]` / `[0x13B8A44]` | CRT vectors | **PROVEN** |
| `0121A460` / `0121A4F0` / `0121A580` | pre-Init empty fill | **PROVEN** |
| `0x01239E5C`…`0x01239DC4` | `STANDARD_TALK_*` | **PROVEN** |
| `0x01239D28`…`0x01239DB0` | `CONVERSATION_*` | **PROVEN** |
| `0x122D70E` | empty first `CONVERSATION` slot | **PARTIAL** (empty; not in `strings.tsv`) |
| listing `"NULL"` | first `[0x13B8A38]` slot | **PARTIAL** (listing); VA **UNREAD** |
| Spoken line from this site | — | **DISPROVEN** |
| Host `EnterGame` body | Note only | **LEFTOVER** vs bind |
| Host named stage | present after subtitled | **MATCH** |
| `0041732A` | next named apply | **PROVEN** site; body **UNREAD** here |
| Oakvale / `00DBDE40` | — | **DISPROVEN** |

---

## Remaining UNREAD

- Exact VA of listing `"NULL"` (`strings.tsv`
  has no 4-char `NULL`).
- `009A0590` / `0099E9B0` internals beyond
  CString replace.
- First-seen *reader* of the three vectors on
  the Leave / type-1 walk (`004CD9B0` /
  `004CD9C0` callers are later / other).
- Semantic names for slot indices (do not
  invent).
- Whether a second `004CD670` can run with
  `[0x13B8A28]==1` on this walk (flag write
  is this fn only; re-entry **UNREAD**).

Walk-first omitted child stays earlier unnamed
`0044C6B6`. This note stops at the bind.
