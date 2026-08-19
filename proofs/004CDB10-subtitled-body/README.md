# `004CDB10` Init Subtitled Message after Init Fonts

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave this
walk is `FinalAlbion.wld` → `"Init Game"` →
`00418DCA` → vtbl+4 `004184BD`. Do **not** treat
later `004CDC40` / `004CEA60` / `Speak` as this
site.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: `004CDB10` Init Subtitled Message after
Init Fonts. First-seen callees? Host Note-only
leftover? Real work?

Authority: Fable.exe dump
`listing-00400000.txt` (`004184BD` `00418605`–
`00418692`, `0041A060`–`0041A090`);
`listing-004c0000.txt` (`004CDB10`–`004CDB68`,
`004CD670`, `004CDB70`, `004CDC40`, `004CDF91`);
`listing-00a00000.txt` (`00A39010`–`00A39187`,
`00A38E50`–`00A38FAB`, `00A38500`–`00A3854B`,
`00A01A37`–`00A01A58`);
`listing-00980000.txt` (`0099B6B0` `ret 4`,
`0099B940` wchar walk, `0099BF30` `ret 4`);
`listing-01200000.txt` (`0121A630` / `01228DD0`);
`e8.tsv` dests `004CDB10` / `00A39010` /
`00A38500`;
`functions.tsv` `004184BD` callee list;
`xrefs.tsv` `"Init Subtitled Message"`;
`strings.tsv` (ASCII; wchar prefixes **absent**);
`docs/runtime/FORWARD_TREE.md` §6;
`src/Fable.Game/EngineLifecycle.cs`
(`InitGameStages` / `EnterGame`);
siblings `proofs/004168DC-after-graphics`,
`proofs/004168DC-init-fonts`,
`proofs/dialogue-first`,
`proofs/audio-initgame-first`,
`proofs/initgame-after-leave-order`.

---

## Verdict

**Yes: on `004184BD`, `004CDB10` is the next named
child after Init Fonts `004168DC`.** Parent logs
`"Init Subtitled Message"` then `call 004CDB10`
with **no** `ecx=game`. Sole `E8` of `004CDB10`
is `00418637`.

Body is **not** the log trio. It builds a UTF-16
path (`0041A080` prefix `0x122F3D0` + `0099BF30`
leaf `0x1239E74`) and `00A39010` **loads that
file’s `enum` symbols** into the already-live
singleton `[0x13B8A54]`. That is **real work**.
No spoken line is queued.

Host already has the **name** in `InitGameStages`
(after `"Init Fonts"`). `EnterGame` only
`Note(apply)`. No path, no `00A39010`, no
`[0x13B8A54]` fill. **LEFTOVER** Note-only.

| Claim | Class |
|---|---|
| `00418637` `call 004CDB10` immediately after `00418607` `004168DC` (log in between) | **PROVEN** |
| `ecx=game` thiscall like fonts | **DISPROVEN** |
| Other `E8` of `004CDB10` | **DISPROVEN** — only `00418637` |
| Host `InitGameStages` notes the name | **PROVEN** **MATCH** |
| Host `EnterGame` runs the body | **DISPROVEN** — **LEFTOVER** Note-only |
| Body is log-only (`0041863D` class) | **DISPROVEN** |
| Work is `0041A080`+`0099BF30` path then `00A39010` on `[0x13B8A54]` | **PROVEN** |
| `00A39010` first-seen on this walk is `004CDB46` (before sound `00A01A4F`) | **PROVEN** |
| Decoded path strings (`0x122F3D0` / `0x1239E74`) | **UNREAD** (wchar; ASCII `strings.tsv` skips) |
| Queues a spoken / subtitled line | **DISPROVEN** (dialogue-first) |
| Nested under Init Fonts / Init Graphics | **DISPROVEN** — sibling |

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| After Init Fonts `004168DC` on `004184BD`? | **Yes.** `00418607` then log `"Init Subtitled Message"` then `00418637`. Next named sibling is log `"Adding Console Variables"` `0041863D` then `"Init Conversation Attitude"` `004CD670`. | **PROVEN** |
| First-seen callees? | Direct `E8`: `0099EBF0`, `0041A080`, `0099BF30`, `00A39010`, `0099B510`×2, `0099EAE0`. Work dest is `00A39010`. | **PROVEN** |
| Host Note-only leftover? | **Yes.** Name present; body omitted. Same leftover class as `004CD670` today. Walk-first hole stays earlier unnamed `0044C6B6`. | **PROVEN** |
| Real work? | **Yes.** Path join + symbol-file load into `[0x13B8A54]`. Not log-only. File hit vs miss first-seen is **UNREAD**. | **PROVEN** work; I/O **UNREAD** |

**Answer:** first leftover **on this named site**
is the body (`00A39010`). Adding Note-only would
**MATCH** what host already does. Adding real
work closes this site; next named leftover is
`004CD670`. Do not start Oakvale.

---

## 1. Site: sibling after Init Fonts

`listing-00400000.txt`:

```
00418605  mov ecx, esi
00418607  call 004168DC          ; Init Fonts (name inside)
0041860C  push edi
0041860D  push "Init Subtitled Message"
00418612  lea ecx, [ebp-8]
00418615  call 0099EBF0
0041861A  fld [0x122DEE0]
…
0041862A  call 009E9F40
00418632  call 0099EAE0
00418637  call 004CDB10          ; THIS SITE — no ecx=esi
0041863C  push edi
0041863D  push "Adding Console Variables"
…
00418662  call 0099EAE0          ; log only — no apply E8
00418692  call 004CD670
```

`esi` is game (`004184D1` `[0x13B86A0]=esi`).
`004184BD` does **not** pass it into `004CDB10`.

`e8.tsv` dest `004CDB10`: **only** `00418637`.

`functions.tsv` `004184BD` callee list:
`…,00416C8A,004168DC,0099EBF0,…004CDB10,0099EBF0,…004CD670…`.

`FORWARD_TREE` §6 wrongly nests `004168DC` under
Init Graphics. `004CDB10` is a **sibling** of
both. Nest **DISPROVEN** (`004168DC-after-graphics`).

---

## 2. First-seen callees (`e8.tsv` in-range)

`004CDB10`–`004CDB68` (`listing-004c0000.txt`):

```
004CDB10  sub esp, 12
004CDB13  push -1
004CDB15  push 0x122D70E
004CDB1A  lea ecx, [esp+8]
004CDB1E  call 0099EBF0            ; empty CString
004CDB23  lea eax, [esp]
004CDB26  push eax                 ; 00A39010 arg1 (ret 8)
004CDB27  push 0x1239E74           ; UTF-16 leaf
004CDB2C  lea ecx, [esp+16]
004CDB30  call 0041A080            ; intern prefix 0x122F3D0
004CDB35  mov edx, eax
004CDB37  lea ecx, [esp+12]
004CDB3B  call 0099BF30            ; concat leaf (ret 4)
004CDB40  push eax
004CDB41  mov ecx, 0x13B8A54
004CDB46  call 00A39010            ; load (ret 8)
004CDB4B  lea ecx, [esp+4]
004CDB4F  call 0099B510
004CDB54  lea ecx, [esp+8]
004CDB58  call 0099B510
004CDB5D  lea ecx, [esp]
004CDB60  call 0099EAE0
004CDB65  add esp, 12
004CDB68  ret
```

| # | Site | Dest | Role | Keep? |
|--:|---|---|---|---|
| 1 | `004CDB1E` | `0099EBF0` | CString `0x122D70E` (`-1`) | plumbing |
| 2 | `004CDB30` | `0041A080` | prefix `0x122F3D0` via `0099B6B0` | **path** |
| 3 | `004CDB3B` | `0099BF30` | append `0x1239E74` (`0099B940` `eax*2`) | **path** |
| 4 | `004CDB46` | `00A39010` | load into `[0x13B8A54]` | **work** |
| 5–6 | `004CDB4F` / `58` | `0099B510` | drop temps | cleanup |
| 7 | `004CDB60` | `0099EAE0` | drop temp | cleanup |

`0041A080` is the same stub shape as
`0041A060` (`push 0x122F3B4` / `"Data\Levels\"`):

```
0041A080  push esi
0041A081  push 0x122F3D0
0041A086  mov esi, ecx
0041A088  call 0099B6B0            ; ret 4
0041A08D  mov eax, esi
0041A08F  pop esi
0041A090  ret
```

`0x122F3B4` + UTF-16 `"Data\Levels\"` (14 wchar)
= `0x122F3D0`. Next prefix is therefore the
**next** wchar dir. Bytes **UNREAD** (same skip
as `0041A0A0` `0x0122F3E8` in
`anim-event-first`). Leaf `0x1239E74` sits
between ASCII `"STANDARD_TALK_GENERIC"`
(`0x01239E5C`) and `"UNKNOWN"` (`0x01239E98`);
`0099B940` walks it as wchar. Text **UNREAD**.
Do not invent a filename.

`00A39010` `e8.tsv`: `004CDB46`, `00A01A4F`.
The second is sound `"Sound Bank: Init Symbols"`
**after** `"Init Sound"` `00417A58`. First-seen
is **this** site. **PROVEN**.

---

## 3. `00A39010` is the load (real work)

`ecx = 0x13B8A54`. `ret 8` (path CString +
scratch). `listing-00a00000.txt`:

| Step | VA | Role |
|---|---|---|
| lock | `00A39900` on `this+4` | |
| clear list | `[this+20]` via `004CF810` if `[this+24]!=0` | |
| push path | `0099B7D0` `ecx=0x13D27E8` | file-stack |
| alloc + zero | `00BFEA0E` / `rep stos` | buffer |
| token rewrite | `00A60410` ×3 (`0x129B208`/`204`/`200`/`1FC`/`1F8`) | **PARTIAL** tokens |
| parse | `00A38E50(this, buf, arg1)` | **work** |
| pop path | `0099B800` `0x122D70C` | |
| free | `00BFEA14` | |

`00A38E50` walks `"enum"` via `009B9BA0` /
`00A38670`. Nearby ASCII
`"DefinitionManager : CreateSymbolsFromPathList"`
(`0x0129B20C`) names the **generic** loader,
not a spoken line. Hit vs miss of the TLC file
is **UNREAD**. Empty table on miss is enough
for first-seen (no line). **PROVEN** attempt;
payload **UNREAD**.

Static ctor already ran (`0121A630`):

```
0121A630  mov ecx, 0x13B8A54
0121A635  call 00A38500            ; vtbl 0x129CF84; list at +20
0121A63A  push 0x1228DD0
0121A63F  call 004012BC            ; atexit → 01228DD0 004D1D50
```

`e8.tsv` dest `00A38500`: `0121A635`,
`00A01A0C` (sound, later). So Init Game does
**not** construct the object; it **fills** it.
`dialogue-first` “construct” is the fill.
**DISPROVEN** as a heap ctor on this site.

Later lookup (`004CDF91` … `00A38420` on the
same singleton) is **not** on the Init Game
first-seen walk. **DISPROVEN** as queued text.

`004CDB70` / `004CEA60` are later subtitle
records / `"SND_"` helpers. Zero `E8` from
`004CDB10`. **PROVEN** skip
(`audio-initgame-first` §3).

---

## 4. Host Note-only leftover

`InitGameStages` (13; fonts already inserted):

```
Init Graphics                  00416C8A   ; Note + OpenTextureBank
Init Fonts                     004168DC   ; Note + GameFontFace store
Init Subtitled Message         004CDB10   ; Note only  ← this site
Init Conversation Attitude     004CD670   ; Note only
```

`EnterGame` loop: `Note(apply, name, "InitGame",
name)` for this row. No `00A39010`, no path,
no `[0x13B8A54]`. Contrast Init Fonts, which
notes `009E2C80` / `00419463` and stores
`ENG_ARIAL_18`.

### If we keep Note-only

Named notes **MATCH**. Leftover **on this
site** is the work row (path + load). Same
class as `004CD670`. Walk-first omitted child
stays `0044C6B6` (`initgame-after-leave-order`
row 5).

### If we add real work

Note + `00A39010` into `[0x13B8A54]` **MATCH**
this site. First leftover **after** this insert
on the named list is `004CD670` (still
Note-only; body is `STANDARD_TALK_*` binds,
not this proof). Walk-first hole still
`0044C6B6`.

---

## 5. What this does **not** say

- Decoded `Data\…` + leaf. **UNREAD**.
- `00A60410` token bytes. **PARTIAL**.
- TLC file present first-seen. **UNREAD**.
- `004CDC40` / `004CEA60` / `Speak` fire here.
  **DISPROVEN**.
- New Game is `00DBDE40`. **DISPROVEN**.

---

## Classification (VAs)

| VA | Role | Class |
|---|---|---|
| `004184BD` | vtbl+4 parent | **PROVEN** |
| `004168DC` | Init Fonts, previous sibling | **PROVEN** |
| `00418637` | sole caller | **PROVEN** |
| `004CDB10` | this fn | **PROVEN** on walk; host **LEFTOVER** Note-only |
| `0041A080` | prefix intern `0x122F3D0` | **PROVEN** callee; text **UNREAD** |
| `0099BF30` | leaf concat `0x1239E74` | **PROVEN** callee; text **UNREAD** |
| `00A39010` | symbol load | **PROVEN** first-seen work |
| `00A38E50` | `"enum"` parse | **PROVEN** nested; file **UNREAD** |
| `00A38500` / `0121A630` | static ctor of `[0x13B8A54]` | **PROVEN** pre-Init Game |
| `0041863D` | log-only neighbor | **PROVEN** not this fn |
| `004CD670` | next named leftover | **PROVEN** site; body **UNREAD** here |
| `0044C6B6` | earlier unnamed omit | **PROVEN** first walk hole |
| `004CDB70` / `004CEA60` | later subtitle / `SND_` | **DISPROVEN** first-seen |
| Oakvale / `00DBDE40` | — | **DISPROVEN** |
