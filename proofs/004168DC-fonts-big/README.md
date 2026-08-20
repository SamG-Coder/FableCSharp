# `004168DC` does **not** open `fonts.big`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave this
walk is `FinalAlbion.wld` → `"Init Game"` →
`00418DCA` → vtbl+4 `004184BD` → `"Init Fonts"`
`004168DC`. Do **not** treat frontend type-6
`ENG_ARIAL_16` / persist `ENG_ARIAL_24` as this
site.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Question: Init Fonts `004168DC` stores
`ENG_ARIAL_18` via `009E2C80` / `00419463`.
Does this fn (or a first-seen callee) also
open `fonts.big`? If yes, via which VA and
path string? Host leftover vs Note+name-only
store?

Authority: Fable.exe dump
`listing-00400000.txt` (`004168DC`–`00416952`,
`004184BD` `00418600`–`00418607`, `00415350` /
`00415440` / `00415530`, `00403230`,
`00419463`, `0042DDB3`);
`listing-009c0000.txt` (`009E2C80` / `009E2BA0` /
`009E2C10` / `009E51C0` / `009CC240`);
`listing-00a40000.txt` (`00A634F0`);
`listing-00a80000.txt` (`00AB8E10`);
`e8.tsv` dests `004168DC` / `009E2C80` /
`00419463`;
`functions.tsv` `004168DC`;
`xrefs.tsv` `"Init Fonts"` / `"ENG_ARIAL_18"` /
`"FONT_ENGLISH_MAIN"`;
`strings.tsv` (ASCII; wchar paths **absent**);
host `src/Fable.Game/EngineLifecycle.cs`
(`Init Fonts` arm / `GameFontFace`);
`src/Fable.Game/FontBank.cs`;
siblings `proofs/004168DC-init-fonts`,
`proofs/00419463-pair-layout`.

---

## Verdict

**DISPROVEN open.** `004168DC` and every
first-seen callee only log, intern the face
**name**, look it up, and copy a `{T*, ctrl*}`
pair. No `fonts.big` path, no `0099AD80`
file open, no bank `vtbl+4` bind.

ASCII `strings.tsv` has **no** `fonts.big`.
`xrefs.tsv` for this fn is only `"Init Fonts"`
and `"ENG_ARIAL_18"`. Listings never emit
`fonts.big`. Do **not** invent a path string
or an open VA on this site.

Host leftover is **Note + name-only store**,
not a `fonts.big` open. `EnterGame` Notes
`009E2C80` / `00419463` and sets
`GameFontFace = "ENG_ARIAL_18"`. Native slot
is the face pair (`00419463-pair-layout`).
Host `FontBank` `BigArchive.Open(fonts.big)`
is `AttachFrontendTree` — **DISPROVEN** as
this child.

| Claim | Class |
|---|---|
| `004168DC` body opens `fonts.big` | **DISPROVEN** |
| First-seen callee opens `fonts.big` | **DISPROVEN** |
| Listing / ASCII / `xrefs` path string | **DISPROVEN** — absent |
| Work is `009E2C80` + `00419463` `game+90444` | **PROVEN** |
| Host Note + `GameFontFace` name | **PROVEN** leftover shape; name **MATCH** |
| Host `FontBank` ctor is this site | **DISPROVEN** |
| Exact native `fonts.big` open VA + wchar | **UNREAD** (not this listing) |

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Does `004168DC` open `fonts.big`? | **No.** | **DISPROVEN** |
| First-seen callee? | **No.** `009E2C80` is map lookup; `00419463` is pair copy. | **DISPROVEN** |
| VA + path string? | **None on this site.** Do not invent one. | **DISPROVEN** open |
| Host leftover vs Note+name-only? | Host already Notes + stores the **name**. Leftover is the native `{T*, ctrl*}` pair, not a missing `fonts.big` open. | **PROVEN** leftover |

---

## 1. `004168DC` listing — no file

`functions.tsv` size 38, dests:

`0099EBF0,009E9F40,0099EAE0,0099EBF0,009E2C80,00419463,004057A0,0099EAE0`

`listing-00400000.txt`:

```
004168DC  push ebp / sub esp, 12 / mov esi, ecx
004168E7  push "Init Fonts"
004168EF  call 0099EBF0
00416905  call 009E9F40
0041690D  call 0099EAE0            ; log trio
00416914  push "ENG_ARIAL_18"
0041691C  call 0099EBF0
00416921  mov ecx, [0x13B838C]     ; engine+132
0041692F  call 009E2C80            ; lookup
00416935  lea ecx, [esi+90444]
0041693B  call 00419463            ; store pair
00416943  call 004057A0
0041694B  call 0099EAE0
00416952  ret
```

| # | VA | Role | File? |
|--:|---|---|---|
| 1–3 | `0099EBF0` / `009E9F40` / `0099EAE0` | `"Init Fonts"` log | no |
| 4 | `0099EBF0` | `"ENG_ARIAL_18"` | name only |
| 5 | `009E2C80` | lookup on `[0x13B838C]` | **no** |
| 6 | `00419463` | `game+90444` | **no** |
| 7–8 | `004057A0` / `0099EAE0` | drop temps | no |

No `0099AD80`, no `009F83D0`, no `0041A180`
wchar intern, no `push 0x12xxxx` path, no
`CreateFile`. **PROVEN** name-only + pair
store.

`xrefs.tsv` fn `004168DC`:

| VA | Site | String |
|---|---|---|
| `0x0122EDF0` | `004168E8` | `Init Fonts` |
| `0x0122EDE0` | `00416915` | `ENG_ARIAL_18` |

`strings.tsv`: no `fonts.big`, no
`lang\English\fonts`. Neighbor ASCII is
`"Opening Text Bank"` / `"Init Text"` on
**sibling** `00416832`, not this fn.

---

## 2. First-seen callee `009E2C80` — no file

`listing-009c0000.txt`:

```
ecx = font manager
009E51C0(this+20, name)          ; MAIN contains?
  yes → 009E5120(this+20) + 009E2BA0
  no  → 009E51C0(this+28, name)  ; STREAMING
          hit  → 009E5120(this+28) + 009E2C10
          miss → still 009E2CBF MAIN + 009E2BA0
```

`009E51C0` is a map probe (`009E6910` /
`009E6530`, `setne al`). **No** path.

`009E2BA0`:

```
00A635E0(this+4, entry) → 00A634F0
copy {T*, ctrl*} into dest; inc/dec refs
```

`00A634F0` lazy MAIN face: if `[slot+4]==0`,
alloc `0x2034`, `00AB8E10`, wrap `0041947A`.
`00AB8E10` reads via `009CC240` / `009CC2A0`
from an **already-open** bank stream
(`[bank+124].vtbl+20`). That is entry
payload, not `fonts.big` archive open.

`00419463` (`listing-00400000.txt`):
`push [src+4]` / `push [src]` / `004190E2`.
**No** I/O.

First-seen arm MAIN hit vs insert remains
**PARTIAL** (sibling). Neither arm is a
`.big` open.

---

## 3. Bank is already live before this sibling

`0042DDB3` (frontend graphic-bank setup,
**before** Leave / Init Game):

```
0042DDC4  mov ecx, [0x13B838C]
0042DDCB  call 009E2C80
```

Same manager, same lookup. Font maps exist
before `"Init Fonts"`. `004022F0` only
aliases `[engine+132]` → `[0x13B838C]`.

`FONT_ENGLISH_MAIN` (`0x0122E988`) xrefs
**only** `004153F3` inside `00415350`
(language → bank **name** CString).
`e8.tsv` dest `00415350`: **empty**.
`00415440` at `00403230` (`"Setup library"`)
writes `STREAMING_FONT_ENGLISH_PC` into the
engine config blob. Names only. **DISPROVEN**
as `004168DC` children.

Exact first `File.Open` / `0099AD80` of
`lang\English\fonts.big` is **UNREAD** here.
Do not pin it to this fn.

---

## 4. Host leftover is Note + name, not the file

`InitGameStages` already has
`("Init Fonts", 0x004168DC)`. `EnterGame`:

```
Note(009E2C80, "ENG_ARIAL_18 [0x13B838C]");
Note(00419463, "[game+90444]");
GameFontFace = "ENG_ARIAL_18";
```

No `BigArchive.Open`. `GameFontFace` is
`string?`. Native `+90444/+90448` is
`{face*, ctrl*}`. **LEFTOVER** shape; name
**MATCH**.

`FontBank` ctor (`lang/English/fonts.big`,
`FONT_ENGLISH_MAIN`) runs from
`AttachFrontendTree` for type-6 UI.
**DISPROVEN** as `004168DC`.

Adding a host `fonts.big` open on this
stage would **DIVERGE** the listing.

---

## 5. What this does **not** say

- `009E2C80` MAIN vs STREAMING first-seen
  arm. **PARTIAL** (sibling).
- `00AB8E10` first-seen on this New Game.
  **PARTIAL** (only if MAIN slot empty).
- Bootstrap `009A6610` / `CFontBank` ctor
  is the `fonts.big` open VA. **UNREAD**.
- `00416832` `"Opening Text Bank"` is
  fonts. **DISPROVEN** (`TEXT_*_MAIN`).
- New Game is `00DBDE40`. **DISPROVEN**.

---

## Classification (VAs)

| VA | Role | Class |
|---|---|---|
| `004168DC` | Init Fonts | **PROVEN** name+store; **DISPROVEN** `fonts.big` open |
| `009E2C80` | face lookup | **PROVEN** callee; **DISPROVEN** open |
| `00419463` | pair into `game+90444` | **PROVEN**; **DISPROVEN** open |
| `009E51C0` / `009E2BA0` | map / attach | **PROVEN** children of lookup; **DISPROVEN** open |
| `00AB8E10` / `009CC240` | read face bytes | **PROVEN** on lazy miss; **DISPROVEN** archive open |
| `00415350` | `FONT_ENGLISH_MAIN` name | **DISPROVEN** as this callee (no `E8`) |
| `00403230` | `STREAMING_FONT_*` name | **DISPROVEN** as this child |
| `0042DDB3` | earlier `009E2C80` | **PROVEN** manager already live |
| `GameFontFace` | host name | **PROVEN** leftover vs `T*` |
| `FontBank` ctor | host `fonts.big` | **DISPROVEN** as this site |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-009c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00a40000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00a80000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\functions.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\xrefs.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\strings.tsv`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\src\Fable.Game\FontBank.cs`
- `C:\FableCSharp\proofs\004168DC-init-fonts\README.md`
- `C:\FableCSharp\proofs\00419463-pair-layout\README.md`
