# `004168DC` after Init Graphics `00416C8A`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave this
walk is `FinalAlbion.wld` → `"Init Game"` →
`00418DCA` → vtbl+4 `004184BD`. Do **not** treat
frontend type-6 `ENG_ARIAL_16` / persist
`ENG_ARIAL_24` as this site.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: `004168DC` Init Fonts on `004184BD`:
after Init Graphics `00416C8A`? Host omits it.
First leftover if we add a Note-only vs real work?

Authority: Fable.exe dump
`listing-00400000.txt` (`004168DC`–`00416952`,
`00416C8A`–`00417000`, `004184BD` `004185D5`–
`00418637`, `004175DA`–`004175E4`, `0041799B`–
`00417A55`, `00418E5A`, `00419463`, `0040EF10`,
`00434DE0`);
`e8.tsv` dests `004168DC` / `00416C8A`;
`functions.tsv` `004168DC` / `00416C8A` /
`004184BD`;
`xrefs.tsv` `Init Fonts` / `ENG_ARIAL_18`;
`docs/runtime/FORWARD_TREE.md` §6;
`src/Fable.Game/EngineLifecycle.cs`
(`InitGameStages` / `EnterGame` / `OpenTextureBank`);
siblings `proofs/004168DC-init-fonts`,
`proofs/initgame-after-leave-order`.

---

## Verdict

**Yes: on `004184BD`, `004168DC` is the next
`E8` after `"Init Graphics"` `00416C8A`.** Same
`esi` (game). It is a **sibling**, not a child.
`00416C8A` callees have no `004168DC`. Other
`00416C8A` sites (`004179D2`, `00417A3B`) do
**not** call fonts.

Host `InitGameStages` has the twelve names
`004184BD` itself logs. `"Init Fonts"` is logged
**inside** `004168DC`, so the table skips it.
`EnterGame` after the Init Graphics note only
`OpenTextureBank()`. **LEFTOVER** omit.

**Note-only vs work.** Adding
`("Init Fonts", 0x004168DC)` as another
`InitGameStages` row would **MATCH** the other
named-stage **notes** (first omitted named child
closes). The leftover then is the **real work**:
`009E2C80` (`ENG_ARIAL_18` on `[0x13B838C]`)
then `00419463` into `game+90444`. That work is
**not** log-only (contrast `"Adding Console
Variables"` `0041863D`). Adding the store closes
this site; the walk’s **first** omitted child is
still earlier unnamed `0044C6B6`.

| Claim | Class |
|---|---|
| `00418607` `ecx=game` `call 004168DC` immediately after `00418600` `00416C8A` | **PROVEN** |
| Nested under `00416C8A` / `FORWARD_TREE` §6 nest | **DISPROVEN** |
| After every `00416C8A` | **DISPROVEN** — `004179D2` / `00417A3B` skip it |
| Host `InitGameStages` / `EnterGame` runs `004168DC` | **DISPROVEN** — **LEFTOVER** omit |
| First omitted **named** `004184BD` child | **PROVEN** — `"Init Fonts"` |
| First omitted `004184BD` child on the walk | **DISPROVEN** — `0044C6B6` is earlier |
| Note-only closes the named-list hole | **PROVEN** (notes **MATCH** 13 names) |
| Note-only leftover is `009E2C80` + `00419463` `game+90444` | **PROVEN** |
| Note-only **MATCH** native body | **DISPROVEN** — not log-only |
| Frontend `FontBank` (`ENG_ARIAL_16`/`24`) is this site | **DISPROVEN** |

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| After Init Graphics `00416C8A` on `004184BD`? | **Yes.** `00418600` then `00418607`. Before `"Init Subtitled Message"` `004CDB10`. | **PROVEN** |
| Host omits it? | **Yes.** Twelve `InitGameStages`; no `004168DC` note; `OpenTextureBank` only. | **PROVEN** leftover |
| First leftover if we add **Note-only**? | The **work**: face lookup + `game+90444`. Named notes then **MATCH**. Walk-first hole stays `0044C6B6`. | **PROVEN** |
| First leftover if we add **real work**? | This site **MATCH**. Next named leftover is `004CDB10` (already Note-only). Walk-first hole still `0044C6B6`. | **PROVEN** site; next named **PARTIAL** (body **UNREAD** here) |

---

## 1. Site: sibling after `00416C8A`

`listing-00400000.txt`:

```
004185DE  push edi
004185DF  push "Init Graphics"
…
004185FE  mov ecx, esi
00418600  call 00416C8A
00418605  mov ecx, esi
00418607  call 004168DC          ; THIS SITE — no parent string
0041860C  push edi
0041860D  push "Init Subtitled Message"
…
00418637  call 004CDB10
```

`esi` is game (`004184D1` `[0x13B86A0]=esi`).
`004184BD` does **not** push `"Init Fonts"`.
`xrefs.tsv`: string at `004168E8` inside
`004168DC` only.

`functions.tsv` `004184BD` callee list:
`…,00416C8A,004168DC,0099EBF0,…004CDB10…`.
`00416C8A` dests:
`0099EBF0,009E9F40,…004BBFC0` — **no**
`004168DC`.

`e8.tsv` dest `004168DC`:

| Site | Parent | After `00416C8A`? |
|---|---|---|
| `00418607` | `004184BD` | **yes** first-seen New Game |
| `004175DD` | thunk `004175DA` | **no** |

`e8.tsv` dest `00416C8A`:

| Site | Next `E8` | `004168DC`? |
|---|---|---|
| `00418600` | `00418607` | **yes** |
| `004179D2` | `00434DC0` (display) | **no** |
| `00417A3B` | `00434DD0` | **no** |

`004175DA` is `add ecx, -4` / `call 004168DC` /
`mov al, 1` / `ret` after `00417568` `ret`.
**DISPROVEN** on this walk. Thunk taker
**UNREAD**.

---

## 2. Work inside `004168DC` (not the log)

`functions.tsv` size 38:

```
004168DC  push ebp / sub esp, 12 / mov esi, ecx
004168E7  push "Init Fonts"
004168EF  call 0099EBF0
00416905  call 009E9F40
0041690D  call 0099EAE0            ; log trio
00416914  push "ENG_ARIAL_18"
0041691C  call 0099EBF0
00416921  mov ecx, [0x13B838C]     ; engine+132
0041692F  call 009E2C80            ; face lookup
00416935  lea ecx, [esi+90444]
0041693B  call 00419463            ; store pair
00416943  call 004057A0
0041694B  call 0099EAE0
00416952  ret
```

| # | VA | Role | Keep? |
|--:|---|---|---|
| 1–3 | `0099EBF0` / `009E9F40` / `0099EAE0` | `"Init Fonts"` | log |
| 4 | `0099EBF0` | `"ENG_ARIAL_18"` | name |
| 5 | `009E2C80` | lookup | **work** |
| 6 | `00419463` | `game+90444` | **work** |
| 7–8 | `004057A0` / `0099EAE0` | drop temps | cleanup |

`00418DCA` `00418E5A` zeros `+90444/+90448`.
`00419463` is `push [eax+4]` / `push [eax]` /
`004190E2`. Getters (not this walk):
`0040EF10` `[ecx+90444]`; `00434DE0`
`[[this+8]+90444]`; HUD `0040B8E0` reads the
slot then `[0x13B8790]+24`. `009E2C80` MAIN vs
STREAMING arm first-seen is **PARTIAL**
(sibling `004168DC-init-fonts` §2).

Neighbor `"Adding Console Variables"`
`0041863D` is the log trio only — **no** apply
`E8`. Host already Notes `0041863D` before
Conversation Attitude. **DISPROVEN** that
`004168DC` is that kind of stage.

---

## 3. Host omit and the two add-paths

`InitGameStages` (12; test asserts length 12):

```
Init Thing Components          004EE23F   ; Note only
Init Definition Manager        00416005   ; Note only
Init Graphics                  00416C8A   ; Note + OpenTextureBank
                                           ← native 004168DC here
Init Subtitled Message         004CDB10   ; Note only
…
```

`EnterGame` after the Init Graphics note:
`OpenTextureBank()` only. No `004168DC`, no
`ENG_ARIAL_18`, no `game+90444`.
`AttachFrontendTree` `FontBank` is type-6 UI.

### If we add Note-only

Insert `("Init Fonts", 0x004168DC)` between
Graphics and Subtitled. Named-stage **notes**
**MATCH** `004184BD` sibling order. Leftover
**on this site** becomes the work row (lookup +
store). That is the same leftover class as
`004CDB10` / `004CD670` today: name present,
body not. Walk-first omitted child stays
`0044C6B6` (`initgame-after-leave-order` row 5).

### If we add real work

Note + bind `ENG_ARIAL_18` into the game slot
**MATCH** this site. First leftover **after**
this insert on the named list is then
`004CDB10` (already Note-only; body **UNREAD**
here). First leftover on the whole `004184BD`
walk is still `0044C6B6`, not fonts.

---

## 4. What this does **not** say

- `004168DC` opens `fonts.big`. **UNREAD** —
  this fn only names a face.
- `FORWARD_TREE` §6 nest under Init Graphics
  is the dump. **DISPROVEN**.
- Host `OpenTextureBank` is `004168DC`.
  **DISPROVEN**.
- New Game is `00DBDE40`. **DISPROVEN**.

---

## Classification (VAs)

| VA | Role | Class |
|---|---|---|
| `004184BD` | vtbl+4 parent | **PROVEN** |
| `00416C8A` | Init Graphics, previous sibling | **PROVEN** |
| `004168DC` | Init Fonts, next sibling | **PROVEN** on walk; host **LEFTOVER** |
| `0041863D` | log-only neighbor | **PROVEN** not this fn |
| `009E2C80` | face lookup | **PROVEN** callee; arm **PARTIAL** |
| `00419463` | store `game+90444` | **PROVEN** |
| `0044C6B6` | earlier unnamed omit | **PROVEN** first walk hole |
| `004175DA` | this-4 thunk | **DISPROVEN** on this walk |
| `004179D2` / `00417A3B` | other `00416C8A` | **DISPROVEN** as fonts predecessors |
| `00DBDE40` | later quest body | **DISPROVEN** here |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\functions.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\xrefs.tsv`
- `C:\FableCSharp\docs\runtime\FORWARD_TREE.md`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\proofs\004168DC-init-fonts\README.md`
- `C:\FableCSharp\proofs\initgame-after-leave-order\README.md`
