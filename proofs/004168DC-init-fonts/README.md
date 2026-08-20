# `004168DC` Init Fonts on the `004184BD` walk

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave this
walk is `FinalAlbion.wld` → `"Init Game"` →
`00418DCA` → vtbl+4 `004184BD`. Do **not** treat
frontend type-6 `ENG_ARIAL_16` / persist
`ENG_ARIAL_24` as this site.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: `004168DC` Init Fonts on the `004184BD`
walk. Host omits it. First-seen callees and leftover?

Authority: Fable.exe dump
`listing-00400000.txt` (`004168DC`–`00416952`,
`004184BD` `004185D5`–`00418637`, `00416C8A`,
`004175DA`–`004175E4`, `00418DCA` `00418E5A`,
`00418B7A` `00418BC1`, `004175E5` `0041771E`,
`004190E2`, `00419463`, `004057A0`, `004022F0`,
`0040EF10`, `00434DE0`);
`listing-009c0000.txt` (`009E2C80` / `009E2BA0` /
`009E2C10` / `009E51C0` / `009E5120`);
`listing-00980000.txt` (`009BD460` `009BD7FF`);
`listing-00480000.txt` (`00499B98` `ENG_ARIAL_16`);
`listing-00a00000.txt` (`00A3B9D0` `00A3BA16`);
`e8.tsv` dest `004168DC`;
`functions.tsv` `004168DC` / `004184BD` / `00416C8A`;
`xrefs.tsv` `Init Fonts` / `ENG_ARIAL_18`;
`docs/runtime/FORWARD_TREE.md` §6;
`src/Fable.Game/EngineLifecycle.cs`
(`InitGameStages` / `EnterGame` / `OpenTextureBank` /
`AttachFrontendTree`);
`src/Fable.Formats/Fonts/FontFile.cs`
(`InitFontsFn` / `GameFace`);
siblings `proofs/initgame-after-leave-order`,
`proofs/0042F491-init-game-callees`,
`proofs/glyph-uv-gaps`.

---

## Verdict

**`004168DC` is a first-class `004184BD` sibling
immediately after `"Init Graphics"` `00416C8A`,
not a child of it.** `ecx` is the game object.
Work is: log `"Init Fonts"`, look up
`"ENG_ARIAL_18"` on `[0x13B838C]` (`engine+132`)
via `009E2C80`, store the shared pair into
`game+90444` via `00419463`.

Host `InitGameStages` has the twelve named stages
**except** `"Init Fonts"`. `EnterGame` after
`"Init Graphics"` only `OpenTextureBank()`.
`FontBank` opens later on the **frontend** tree
(`ENG_ARIAL_16` / persist faces). That omit is
the leftover: no `game+90444` writer.

| Claim | Class |
|---|---|
| `004184BD` site `00418607` `ecx=game` `call 004168DC` | **PROVEN** |
| Sibling of `00416C8A`, not nested under it | **PROVEN** |
| `FORWARD_TREE` §6 nests `004168DC` under Init Graphics | **DISPROVEN** |
| `e8.tsv` callers of `004168DC` | only `00418607` and thunk `004175DD` | **PROVEN** |
| Thunk `004175DA` on this walk | **DISPROVEN** |
| Host `InitGameStages` / `EnterGame` runs `004168DC` | **DISPROVEN** — **LEFTOVER** omit |
| First-seen dest is `game+90444` (`ENG_ARIAL_18`) | **PROVEN** |
| Frontend `FontBank` is this site | **DISPROVEN** |
| `009E2C80` first-seen arm (MAIN hit vs insert) | **PARTIAL** |

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| On the `004184BD` walk? | **Yes.** After `00416C8A`, before `"Init Subtitled Message"` `004CDB10`. | **PROVEN** |
| First-seen callees of `004168DC`? | §2. Non-log: `009E2C80` → `00419463` → `004057A0`. | **PROVEN** |
| Host leftover? | Whole fn omitted. No `game+90444` store. Later HUD / display getters read that slot. | **PROVEN** leftover |
| Nested under Init Graphics? | **No.** Same `esi` (game), two `E8`s. | **DISPROVEN** |
| Oakvale / `00DBDE40` here? | **No.** | **DISPROVEN** |

---

## 1. Site on `004184BD`

`listing-00400000.txt`:

```
004185D5  push 1
004185D7  mov ecx, esi
004185D9  call 00416005          ; Init Definition Manager
004185DE  push edi
004185DF  push "Init Graphics"
…
004185FE  mov ecx, esi
00418600  call 00416C8A
00418605  mov ecx, esi
00418607  call 004168DC          ; THIS SITE
0041860C  push edi
0041860D  push "Init Subtitled Message"
…
00418637  call 004CDB10
```

`esi` is game (`004184D1` `[0x13B86A0]=esi`).
`00416C8A` callees (`functions.tsv`) have no
`004168DC`. `e8.tsv` dest `004168DC`:

| Site | Parent | On `004184BD`? |
|---|---|---|
| `00418607` | `004184BD` | **yes** first-seen New Game |
| `004175DD` | thunk `004175DA` | **no** |

`004175DA` is `add ecx, -4` / `call 004168DC` /
`mov al, 1` / `ret`, immediately after
`00417568` `ret`. `functions.tsv` lists
`004168DC` under `00417568` because the scanner
walked past that `ret`. No other listing /
`e8` site of `004175DA`. **Not** this walk.
**PROVEN** boundary leftover; thunk taker
**UNREAD**.

---

## 2. First-seen callees (`004168DC`)

`004168DC`–`00416952` (`functions.tsv` size 38):

```
004168DC  push ebp / sub esp, 12 / mov esi, ecx
004168E3  push -1
004168E7  push "Init Fonts"
004168EF  call 0099EBF0
004168F4  fld [0x122DEE0]
00416905  call 009E9F40
0041690D  call 0099EAE0            ; log trio
00416912  push -1
00416914  push "ENG_ARIAL_18"
0041691C  call 0099EBF0
00416921  mov ecx, [0x13B838C]     ; engine+132
0041692A  push &ebp-4 / push &ebp-12
0041692F  call 009E2C80            ; face lookup
00416934  push eax
00416935  lea ecx, [esi+90444]
0041693B  call 00419463            ; store pair
00416943  call 004057A0            ; drop ebp-12
0041694B  call 0099EAE0            ; drop name
00416952  ret
```

Listing order (**PROVEN**):

| # | VA | Role | Keep? |
|--:|---|---|---|
| 1 | `0099EBF0` | `"Init Fonts"` | log |
| 2 | `009E9F40` | progress | log |
| 3 | `0099EAE0` | drop log | log |
| 4 | `0099EBF0` | `"ENG_ARIAL_18"` | name |
| 5 | `009E2C80` | lookup on `[0x13B838C]` | **work** |
| 6 | `00419463` | `game+90444` assign | **work** |
| 7 | `004057A0` | release lookup temp | cleanup |
| 8 | `0099EAE0` | drop name | cleanup |

`004022F0`: `[0x13B838C] = [009A4EC0()+132]`.
Same object `009BD814` / `00499BAD` use as
`[engine+132]`. Font manager. **PROVEN.**

`009E2C80` (`listing-009c0000.txt`):

```
ecx = font manager
009E51C0(this+20, name)          ; MAIN contains?
  yes → 009E5120(this+20) + 009E2BA0   ; attach
  no  → 009E51C0(this+28, name)        ; STREAMING
          yes → 009E5120(this+28) + 009E2C10
```

`FontFile.MainBank` is `FONT_ENGLISH_MAIN`;
`GameFace` is `ENG_ARIAL_18`. Which arm first-seen
New Game takes is **PARTIAL**: `009BD460`
(`009BD801` same face → `00419463` into
`0x13CA7F8`) is only reached from `00A3B9D0`
(`e8` `00547D30` / `006288BB`, WMV/overlay),
not from `004184BD`. If that overlay already
inserted the face, `009E51C0(+20)` hits and
`009E2BA0` is a ref-copy. Else this site is
the first insert. No live `[this+20]` dump here.

`00419463`:

```
push [eax+4] / push [eax]
call 004190E2     ; {ptr, ctrl}; inc ctrl if new
```

Dest `ecx = game+90444`. Ctor `00418DCA`
zeros `+90444/+90448` (`00418E5A`). Dtor
`00418B7A` / leave `004175E5` `004057A0` that
slot. No other `lea ecx, [esi+90444]` store
on the Init Game walk. **PROVEN** first writer
of the face.

Getters (not this walk): `0040EF10` returns
`[game+90444]`; `00434DE0` returns
`[[this+8]+90444]`; HUD `006444A3` and many
`0064xxxx` / `0040B8E0` (`[0x13B8790]+24`
draw) read it. Host never fills that slot.

---

## 3. Host omit (**LEFTOVER**)

`EngineLifecycle.InitGameStages` (12 names):

```
Init Thing Components
Init Definition Manager
Init Graphics                 ; → OpenTextureBank only
Init Subtitled Message        ; ← native has Init Fonts here
Init Conversation Attitude
…
Load Particles
```

`EnterGame` notes those twelve (plus
`"Adding Console Variables"` `0041863D`).
After `"Init Graphics"` it only
`OpenTextureBank()` (`00416C8A` /
`GBANK_MAIN_PC`). No `004168DC` note, no
`ENG_ARIAL_18`, no `game+90444`.

`FontBank` constructs in
`AttachFrontendTree` for type-6 UI
(`ENG_ARIAL_16` / persist `ENG_ARIAL_24`).
That is the **frontend** leftover path, not
this game-object slot. **DISPROVEN** as a
stand-in for `004168DC`.

Named-stage **notes** otherwise **MATCH**
`004184BD` string order
(`initgame-after-leave-order`). The first
**omitted named child** on that list is
`"Init Fonts"`. (`0044C6B6` is an earlier
unnamed hole; not this file.)

---

## 4. What this does **not** say

- `004168DC` opens `fonts.big` from disk.
  **UNREAD** here. Bank open is earlier
  bootstrap; this fn only names a face.
- `009BD460` / `0x13CA7F8` is the game slot.
  **DISPROVEN** — that is a different dest.
- `00499B9A` `"ENG_ARIAL_16"` → `0x13B8998`
  is this site. **DISPROVEN** (progress UI).
- `functions.tsv` `004184BD` size `378` is
  the body length. **DISPROVEN** as a
  byte size (sibling proof).
- New Game is `00DBDE40`. **DISPROVEN**.

---

## Classification (VAs)

| VA | Role | Class |
|---|---|---|
| `004184BD` | vtbl+4 parent | **PROVEN** |
| `00416C8A` | Init Graphics, previous sibling | **PROVEN** |
| `004168DC` | Init Fonts | **PROVEN** on walk; host **LEFTOVER** |
| `009E2C80` | face lookup | **PROVEN** callee; arm **PARTIAL** |
| `00419463` | store `game+90444` | **PROVEN** |
| `004057A0` | release temp / dtor slot | **PROVEN** |
| `004175DA` | this-4 thunk | **DISPROVEN** on this walk |
| `009BD460` | other `ENG_ARIAL_18` | **DISPROVEN** as this child |
| `00DBDE40` | later quest body | **DISPROVEN** here |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00480000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00980000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-009c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00a00000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\functions.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\xrefs.tsv`
- `C:\FableCSharp\docs\runtime\FORWARD_TREE.md`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\src\Fable.Formats\Fonts\FontFile.cs`
- `C:\FableCSharp\proofs\initgame-after-leave-order\README.md`
