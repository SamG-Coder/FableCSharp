# `0041863D` Adding Console Variables is log-only

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave this
walk is `FinalAlbion.wld` → `"Init Game"` →
`00418DCA` → vtbl+4 `004184BD`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Question: `004184BD` at `0041863D` pushes
`"Adding Console Variables"` then `0099EAE0`.
Host Notes `0x0041863D` before Init Conversation
Attitude. Is that site truly log-only, or is
there a nearby store / bind of console variables
that the host omits?

Authority: Fable.exe dump
`listing-00400000.txt` (`004184BD` `0041863C`–
`00418692`, `004184F1`–`00418502`, `00419D90`);
`listing-004c0000.txt` (`004CD670`);
`listing-00480000.txt` (`004A7101`–`004A715A`);
`listing-009c0000.txt` (`009ED190`);
`e8.tsv` sites `00418645`–`00418692`;
`functions.tsv` `004184BD`;
`strings.tsv` `0x0122F0FC`;
`xrefs.tsv` `"Adding Console Variables"` /
`"Adding Console Commands"` /
`"Init Global Console"`;
`docs/runtime/FORWARD_TREE.md` §6;
`src/Fable.Game/EngineLifecycle.cs`
(`InitGameStages` / `EnterGame`);
siblings `proofs/004CDB10-subtitled-body`,
`proofs/004168DC-after-graphics`,
`proofs/0044C6B6-first-omit`,
`proofs/initgame-after-leave-order`,
`proofs/00419D90-hoist`.

---

## Verdict

**Yes: `0041863D` is truly log-only.** After
`"Init Subtitled Message"` `004CDB10` the parent
pushes `"Adding Console Variables"` and runs the
standard log trio (`0099EBF0` / `009E9F40` /
`0099EAE0`). There is **no** apply `E8`, **no**
`lea ecx, [esi+N]` store, **no** console-var
bind. The next named sibling is immediately
`"Init Conversation Attitude"` then
`call 004CD670`.

Host `EnterGame` Notes `0x0041863D` immediately
before the Conversation Attitude apply. That
**MATCH**es the native hole. There is **no**
omitted bind at this site.

The only real-work `E8` in `0041863C`–`00418692`
is `00418692` → `004CD670`, which is the **next**
named stage (attitude strings into
`[0x13B8A2C]`), not a hidden console-var store.

Earlier `00414C90` / `009ED190` (BindKey /
RunScript) and later `"Init Global Console"`
`00419D90` are **other** sites. Do not fold them
into `0041863D`.

| Claim | Class |
|---|---|
| `0041863D` push `"Adding Console Variables"` on `004184BD` | **PROVEN** |
| Apply `E8` after that log trio | **DISPROVEN** — next `E8` after `0099EAE0` is the Conversation Attitude log |
| Nearby store / bind of `CConsoleVariable` that host omits | **DISPROVEN** |
| Host Note `0x0041863D` before `004CD670` | **MATCH** |
| Host leftover at this site | **DISPROVEN** — Note-only is the native work |
| `00418692` is a console-var bind | **DISPROVEN** — `004CD670` attitude |
| `"Adding Console Commands"` / `00419D90` is this site | **DISPROVEN** — `004A6E30` later |
| `009ED190` is this site | **DISPROVEN** — earlier `00418502` |
| Oakvale / `00DBDE40` here | **DISPROVEN** |

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Truly log-only? | **Yes.** Log trio then the next named log. | **PROVEN** |
| Nearby store / bind host omits? | **No.** Nothing to omit. | **DISPROVEN** |
| Host Note before Init Conversation Attitude? | **Yes.** **MATCH** native order. | **MATCH** |
| Any `E8` in `0041863C`–`00418692` real work? | Only `00418692` → `004CD670` (next stage). The six log `E8`s are not. | **PROVEN** |

**Answer:** adding more than the existing Note
would invent work the parent never calls. First
named leftover after this hole stays
`004CD670` (Note-only of real work). Do not
start Oakvale.

---

## 1. Site on `004184BD`

`listing-00400000.txt`:

```
00418637  call 004CDB10          ; Init Subtitled Message (work)
0041863C  push edi               ; edi = -1 (00418557)
0041863D  push "Adding Console Variables"   ; 0x0122F0FC
00418642  lea ecx, [ebp-8]
00418645  call 0099EBF0          ; log name
0041864A  fld [0x122DEE0]
00418650  push ebx               ; ebx = 0
00418651  push ecx
00418652  xor dl, dl
00418654  fstp [esp]
00418657  lea ecx, [ebp-8]
0041865A  call 009E9F40          ; progress
0041865F  lea ecx, [ebp-8]
00418662  call 0099EAE0          ; drop log
00418667  push edi
00418668  push "Init Conversation Attitude"
0041866D  lea ecx, [ebp-8]
00418670  call 0099EBF0
00418675  fld [0x122DEE0]
0041867B  push ebx
0041867C  push ecx
0041867D  xor dl, dl
0041867F  fstp [esp]
00418682  lea ecx, [ebp-8]
00418685  call 009E9F40
0041868A  lea ecx, [ebp-8]
0041868D  call 0099EAE0
00418692  call 004CD670          ; next named apply
```

`esi` is game (`004184D1` `[0x13B86A0]=esi`).
This block never uses `esi`. Contrast Init
Fonts (`mov ecx, esi` / `004168DC`) and Init
Graphics. **DISPROVEN** that this is a
thiscall into the game object.

Same shape as every other named log on this
walk. Named stages that do work insert an
apply `E8` **after** `0099EAE0` (Subtitled
`00418637`, Attitude `00418692`, Player
Manager `004186E4`). This name does not.

`xrefs.tsv`: string `0x0122F0FC` is **only**
`0x0041863E` (`fn=0x004184BD`). One push.
**PROVEN** unique site.

`FORWARD_TREE` §6 already lists this child as
`(log only)`. **MATCH.**

---

## 2. Every `E8` in `0041863C`–`00418692`

`e8.tsv`:

| # | Site | Dest | Role | Real work? |
|--:|---|---|---|---|
| 1 | `00418645` | `0099EBF0` | CString `"Adding Console Variables"` | **no** — log |
| 2 | `0041865A` | `009E9F40` | progress bar / clock | **no** — log |
| 3 | `00418662` | `0099EAE0` | drop that CString | **no** — log |
| 4 | `00418670` | `0099EBF0` | CString `"Init Conversation Attitude"` | **no** — next-stage log |
| 5 | `00418685` | `009E9F40` | progress | **no** — log |
| 6 | `0041868D` | `0099EAE0` | drop that CString | **no** — log |
| 7 | `00418692` | `004CD670` | Init Conversation Attitude | **yes** — **next** sibling |

Seven rows. No eighth dest. No `FF 15` /
`call [reg]` / `jmp` in the listing for this
span. No `mov [mem]` / `lea ecx, [esi+disp]`
store.

`functions.tsv` `004184BD` callee run:

```
…004CDB10,0099EBF0,009E9F40,0099EAE0,0099EBF0,009E9F40,0099EAE0,004CD670…
```

Named strings on that row include
`Adding Console Variables` between
`Init Subtitled Message` and
`Init Conversation Attitude`. **PROVEN.**

`004CD670` (`listing-004c0000.txt`) starts:

```
004CD670  mov al, [0x13B8A28]
004CD677  jne 004CD999
004CD67D  mov ecx, [0x13B8A2C]
004CD683  push "STANDARD_TALK_GENERIC"
004CD688  call 0099EFE0
```

Attitude table fill. **DISPROVEN** as a
console-variable bind. Body leftover vs host
Note-only is the **004CD670** sibling
(`004CDB10-subtitled-body`), not this file.

---

## 3. Host Note **MATCH**

`InitGameStages` has no `"Adding Console
Variables"` row (correct: no apply VA).
`EnterGame`:

```
foreach InitGameStages:
  if name == "Init Conversation Attitude"
      Note(0x0041863D, …, "Adding Console Variables")
  Note(apply, name, …)          ; 004CD670
```

That is the native order: log this name, then
Conversation Attitude. **MATCH.** Not a
**LEFTOVER** omit (nothing omitted) and not a
hoist.

Sibling `0044C6B6-first-omit` already used this
site as the contrast class: log-only, Note-only
**MATCH**.

---

## 4. Other console strings are other sites

| String | xref | Parent | After log? |
|---|---|---|---|
| `"Adding Console Variables"` | `0041863E` | `004184BD` | **no** apply |
| `"Init Global Console"` | `004A7104` | `004A6E30` | `004A712B` `00419D90` |
| `"Adding Console Commands"` | `004A7133` | `004A6E30` | **no** apply (same log-only class, later walk) |

`00419D90` registers `"ActivateQuest"`
(`[cmd+20]=00419CE0`, `009EC5E0` into
`[0x13CAA40]`). Host already Notes it on the
Init World arm — **hoist leftover** of that
later grandchild (`00419D90-hoist`), **not**
a missing bind at `0041863D`. **DISPROVEN**
as this site.

Earlier on the **same** `004184BD` walk,
before any named stage:

```
004184F1  push [esi+90436]
004184F7  push 41
004184F9  push 96
004184FB  call 00414C90
00418502  call 009ED190          ; BindString / BindKey / RunScript
```

Host Notes `009ED190` **before**
`InitGameStages`. That bind is **not**
`0041863D`. **DISPROVEN** as omitted work
of this log.

`CConsoleVariable` RTTI `0x013754EC` has no
xref into `0041863C`–`00418692`.

---

## 5. What this does **not** say

- `009ED190` is empty. **DISPROVEN** — it
  binds console commands earlier.
- `"Adding Console Commands"` (`004A7132`) is
  this site. **DISPROVEN**.
- Host should invent a bind at `0041863D` to
  close a leftover. **DISPROVEN**.
- `functions.tsv` size `378` is a byte length.
  **DISPROVEN** (sibling proof).
- New Game is `00DBDE40`. **DISPROVEN**.

---

## Classification (VAs)

| VA | Role | Class |
|---|---|---|
| `004184BD` | vtbl+4 parent | **PROVEN** |
| `0041863D` | push name | **PROVEN** log-only; host **MATCH** |
| `00418645` | `0099EBF0` | **PROVEN** log |
| `0041865A` | `009E9F40` | **PROVEN** log |
| `00418662` | `0099EAE0` | **PROVEN** log |
| `00418670` / `85` / `8D` | next-stage log trio | **PROVEN** not this name |
| `00418692` | `004CD670` | **PROVEN** next sibling work |
| `004CDB10` | previous sibling | **PROVEN** not this fn |
| `009ED190` | earlier BindKey | **DISPROVEN** as this site |
| `00419D90` / `004A712B` | later Init Global Console | **DISPROVEN** as this site |
| `00DBDE40` | later quest body | **DISPROVEN** here |
