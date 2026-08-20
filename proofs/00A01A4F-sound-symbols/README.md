# Second `00A39010` (`00A01A4F`) is not `[0x13B8A54]` / `misc_def_types.h`

Investigation only. No production `src/` or `tests/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `MUSIC_SET_*` / `SND_*`.
After Leave this walk is `FinalAlbion.wld` →
`"Init Game"` → `00418DCA` → vtbl+4 `004184BD`.
`00A01A4F` is **not** a named Init Game stage.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: `e8.tsv` dest `00A39010` has two sites:
`004CDB46` (Init Subtitled, first-seen) and
`00A01A4F` (`"Sound Bank: Init Symbols"` after
Init Sound `00417A58`). What path/file does the
second `00A39010` load? Same `[0x13B8A54]` or a
different `this`? Same `misc_def_types.h` or
another leaf?

Authority: Fable.exe dump
`listing-00a00000.txt` (`00A01920` lookup,
`00A01950`–`00A01A9B`, `00A01A0C` `00A38500`,
`00A01A4F` `00A39010`, `00A38C20`, `00A38500`);
`listing-00400000.txt` (`004184BD` `00418637`
`004CDB10`, `00418886` `00417A58`, `00415550`,
`0041A060`/`080`/`0A0`, `004025EE`, `00414370`);
`listing-00980000.txt` (`009919C0` `00991A44`);
`listing-004c0000.txt` (`004CDB10`–`004CDB68`);
`e8.tsv` dests `00A39010` / `00A38500` /
`00A01950` / `009919C0` / `00417A58`;
`strings.tsv` / `xrefs.tsv`
(`"Sound Bank: Init Symbols"` `0x0129C93C`,
`"UseCompiledSoundSymbols"` `0x0122E7CC` →
`00414371`);
wchar prefixes `0x122F3B4` `Data\Levels\`,
`0x122F3D0` `Data\Defs\`, `0x122F3E8`
`Data\Misc\`; leaf `0x1239E74`
`misc_def_types.h` (first site only);
TLC `userst.ini` `UseCompiledSoundSymbols TRUE;`;
siblings `proofs/004CDB10-00A39010`,
`proofs/004CDB10-host-register`,
`proofs/004CDB10-subtitled-body`,
`proofs/audio-initgame-first`,
`proofs/00417A58-init-sound-body`,
`proofs/ini-activate-quest`.

---

## Verdict

| Question | Answer | Class |
|---|---|---|
| Second `.text` `E8` of `00A39010` is `00A01A4F`? | **Yes.** Only two sites: `004CDB46` then `00A01A4F`. | **PROVEN** |
| Same `this` as `[0x13B8A54]`? | **No.** `ecx = [bank+4]` after heap `00A38500(36)`. | **DISPROVEN** same singleton |
| Same leaf `misc_def_types.h` (`0x1239E74`)? | **No.** That immediate is only `004CDB27`. | **DISPROVEN** same leaf |
| Hardcoded exe path at `00A01A4F`? | **None.** `push edi` is the `009919C0` arg4 CString. | **PROVEN** no exe leaf |
| Prefix if the call ran? | `0041A080` `0x122F3D0` `Data\Defs\` + SOUND_SETUP `[record+8]` (`00415D90`). Not `Data\Levels\` / not `Data\Misc\`. | **PROVEN** prefix; leaf **UNREAD** |
| Exact file name? | Not in the exe. Do **not** invent `atmos_types.h` / `gamesnds.bin`. | **UNREAD** (game.bin) |
| Order vs `004CDB46`? | **Later.** Subtitled is after Init Fonts. This site is nested under `"Init Sound"` after Create Players. | **PROVEN** |
| First-seen TLC fires `00A01A4F`? | **No.** `[0x13BC9F0]` is `UseCompiledSoundSymbols` (TRUE from `userst.ini`) → `00A38C20`, skip `00A39010`. | **DISPROVEN** first-seen fire |

**Answer:** the second `00A39010` would fill a
**new heap** symbol map (`00A38500` at
`[CSoundBankLH+4]`), **not** `[0x13B8A54]`,
from `Data\Defs\` + a **per-bank** names.bin
leaf — **not** `misc_def_types.h`. On first-seen
TLC that `E8` is **not taken**.

---

## Direct answers

| | |
|---|---|
| Path / file | **No single exe path.** Arg0 is `edi` = `Data\Defs\` + SOUND_SETUP / `*_SOUND_SETUP` record[+8]. Exact leaf **UNREAD**. First-seen does **not** call this site. |
| `this` | **Different.** Heap `00A38500` stored at `[esi]` after `add esi, 4` (`bank+4`). **DISPROVEN** `[0x13B8A54]`. |
| Leaf | **Another leaf** (per bank). **DISPROVEN** `0x1239E74` `misc_def_types.h`. |
| vs `004CDB46` | **Second** site, **later** on `004184BD`. First-seen fill remains Init Subtitled. |

Do not start Oakvale. Do not play a `SND_*`.
Do not reuse the subtitled singleton for sound
banks.

---

## 1. Two `E8` sites, later nested

`e8.tsv` dest `00A39010`:

| Site | Owner | `this` | When on `004184BD` |
|---|---|---|---|
| `004CDB46` | `004CDB10` `"Init Subtitled Message"` | `mov ecx, 0x13B8A54` | after Init Fonts `00418607` |
| `00A01A4F` | `00A01950` `"Sound Bank: Init Symbols"` | `mov ecx, [esi]` heap | inside Init Sound `00417A58` |

`e8.tsv` dest `00A01950`: **only** `00991A44`.
Dest `009919C0`: **only** `00417C67` (localised)
and `00417F86` (main). Dest `00417A58`: **only**
`00418886` (after Create Players `004166A8`).

```
00418607  call 004168DC          ; Init Fonts
00418637  call 004CDB10          ; FIRST 00A39010 (004CDB46)
…
00418834  call 004166A8          ; Create Players
00418886  call 00417A58          ; Init Sound
  00417C67 / 00417F86  call 009919C0
    00991A44  call 00A01950      ; may reach 00A01A4F
```

`00A01920` (same listing) is the **31-byte
lookup**, not this fill. Zero `E8` of
`00A01920` in `00417A58`. **PROVEN** skip.

---

## 2. `00A01A4F` — different `this`, no exe leaf

`listing-00a00000.txt` `00A01950` (`ret 4`):

```
00A01950  sub esp, 20
          mov esi, ecx           ; CSoundBankLH
          mov edi, [esp+32]      ; arg0 = 009919C0 arg4 path
…
00A019D3  push "Sound Bank: Init Symbols"
00A019FC  push 36
00A019FE  call 00BFEA1A
00A01A0A  mov ecx, eax
00A01A0C  call 00A38500          ; SAME ctor as 0121A635
00A01A15  add esi, 4             ; bank+4
00A01A18  push eax
00A01A1B  call 00A01AA0          ; store heap ptr
00A01A20  mov al, [0x13BC9F0]
00A01A25  test al, al
00A01A27  je  00A01A37
00A01A30  call 00A38C20          ; compiled arm
00A01A35  jmp 00A01A72           ; SKIP 00A39010
00A01A37  push -1
00A01A39  push 0x122D70E
00A01A42  call 0099EBF0          ; empty scratch
00A01A47  mov ecx, [esi]         ; heap map, NOT 0x13B8A54
00A01A4D  push edx               ; scratch (arg1)
00A01A4E  push edi               ; original path (arg0)
00A01A4F  call 00A39010          ; ret 8
```

`e8.tsv` dest `00A38500`: `0121A635` (BSS
`[0x13B8A54]`, static ctor) and `00A01A0C`
(this heap). Init Sound **constructs a second
map**. Same vtbl `0x129CF84`, list at `+20`.
**PROVEN** different `this`.

`00A39010` still: lock `this+4`, clear `+20`,
file-stack `0099B7D0` `0x13D27E8`,
`00A38E50` `"enum"`. Same helper, other object.

No `push 0x1239E74`. No `call 0041A080` inside
`00A01950`. The path is **already joined** by
the Init Sound loops.

---

## 3. Path builder is `Data\Defs\` + record[+8]

`00417A58` both `009919C0` sites join the
symbol path the same way (main shown):

```
00417EF9  lea ecx, [edi+8]
00417EFC  call 00415D90          ; names.bin id → CString
00417F05  call 0041A080          ; intern 0x122F3D0
          0099BE70 / 00999110    ; → [ebp-52]
…
00417F7B  push lea [ebp-52]      ; 009919C0 arg4
00417F86  call 009919C0
```

`0041A060` `push 0x122F3B4` = `Data\Levels\`
(14 wchar) → next slot `0x122F3D0`.
`0041A080` `push 0x122F3D0` = `Data\Defs\`
(sibling `004CDB10-host-register`, recovered
from exe). `0041A0A0` `push 0x122F3E8` =
`Data\Misc\` (anim-event lists). **This site
uses `0041A080` only.**

`00415D90` is `009D49B0` on `0x13CA828`
(name table), **not** an exe wchar. Leaf is
the SOUND_SETUP 20-byte row field at `+8`.

`009919C0`:

```
00991A2D  mov ebp, [esp+104]     ; arg4 = defs path
00991A31  push 0x122D70C
00991A38  call 0099B150          ; empty?
00991A3F  je  00991A49           ; skip 00A01950
00991A41  push ebp
00991A42  mov ecx, edi           ; bank from audio vtbl+4
00991A44  call 00A01950
```

Localised loop (`00417C67`) is the **first**
`009919C0`. Its arg4 is the same
`0041A080` join on `[esi+8]` (`00417BE6`).
Locale name is `00415550` (`ENGLISH_SOUND_SETUP`
default when `00415070` id-4 `> 14`). Then
`MAIN_SOUND_SETUP` (`00417CC2`). Atmos
`00991C10` has **no** `00A01950`.

Prefix **PROVEN** `Data\Defs\`. Leaf string
**UNREAD** (compiled `SOUND_SETUP` in
`game.bin` / `names.bin`; ASCII `strings.tsv`
has no `.h` leaf here). TLC `data\Defs\`
inventory (`misc_def_types.h`,
`atmos_types.h`, `gamesnds.bin`, …) is
**not** an assignment. Do not invent.

`0x012649D4` is pushed **before** the gate
and concatenated into a **different**
CString (`0099BE70` / `0099BF30`). That is
the compiled-list suffix used by `00A38C20`
(`anim-event-first` same immediate). It is
**not** `00A39010` arg0 (`push edi` is the
un-suffixed path). Suffix text **UNREAD**
(wchar; sits just after ASCII
`"BEGIN_ANIMATION_EVENTS"` `0x012649BC`).

---

## 4. First-seen TLC does not call `00A01A4F`

`[0x13BC9F0]` is the live compiled-sound flag:

```
00414370  push "UseCompiledSoundSymbols"
004143A0  mov [esi+12], 0x13B8619   ; ini bool slot
…
004025E9  mov al, [0x13B8619]
004025EE  mov [0x13BC9F0], al       ; "Setup Basic install files"
```

`xrefs.tsv` `0x0122E7CC` → `00414371` inside
`00413C50` Parse Command Line. TLC
`userst.ini` line 36:
`UseCompiledSoundSymbols TRUE;`
applied at `00414C66` **before** frontend
(`ini-activate-quest`). BSS default 0 is
overwritten. `user.ini` has **no** override.

So first-seen `[0x13BC9F0] != 0` →
`00A38C20`, **`je 00A01A37` not taken**.
`00A39010` at `00A01A4F` is **DISPROVEN**
as a first-seen load. The `.text` site still
exists for the FALSE arm.

`00A38C20` is a **packed** reader
(`0099AD80` / `00A39AE0`), not
`00A38E50` `"enum"`. Its file is the
suffixed sibling of the same `Data\Defs\`
path. Exact compiled name **UNREAD**.
Do not call that `misc_def_types.h`.

Empty arg4 also skips `00A01950` entirely
(`0099B150`). Whether every first-seen row
has a nonempty `+8` is **UNREAD**. Either
way `00A01A4F` is not the first-seen fill.

---

## 5. vs first site / host

| | `004CDB46` | `00A01A4F` |
|---|---|---|
| Stage | `"Init Subtitled Message"` | nested `"Init Sound"` |
| `this` | BSS `[0x13B8A54]` | heap `[bank+4]` |
| Path | `0x122F3D0` + **`0x1239E74`** `misc_def_types.h` | `0x122F3D0` + **record[+8]** |
| First-seen fire | **PROVEN** | **DISPROVEN** (compiled gate) |
| Host | `EnterGame` now notes + register | Note-only `"Init Sound"` |

Host leftover on Init Sound remains the
register (`009919C0` / `00991C10`), not a
second fill of `[0x13B8A54]`. Adding
`00A39010` into the subtitled singleton
from this site would be **wrong `this`**
and **wrong leaf**.

---

## Classification (VAs)

| VA | Role | Class |
|---|---|---|
| `004CDB46` | first `00A39010`; `[0x13B8A54]`; `misc_def_types.h` | **PROVEN** earlier |
| `00A01A4F` | second `00A39010` site | **PROVEN** site; fire **DISPROVEN** first-seen |
| `00A01950` | `"Sound Bank: Init Symbols"` | **PROVEN** owner |
| `00991A44` | only caller of `00A01950` | **PROVEN** |
| `00A01A0C` `00A38500` | per-bank heap map | **PROVEN** different `this` |
| `[0x13B8A54]` as this site's `ecx` | — | **DISPROVEN** |
| `0x1239E74` / `misc_def_types.h` as this leaf | — | **DISPROVEN** |
| `0041A080` `0x122F3D0` `Data\Defs\` | prefix of arg4 | **PROVEN** |
| `0041A060` `Data\Levels\` / `0041A0A0` `Data\Misc\` | this load | **DISPROVEN** |
| SOUND_SETUP `[+8]` leaf text | — | **UNREAD** |
| `[0x13BC9F0]` first-seen TRUE | `userst.ini` → `004025EE` | **PROVEN** |
| `00A38C20` first-seen arm | compiled symbols | **PROVEN** taken; file **UNREAD** |
| `00A01920` / `SND_*` / Oakvale | — | **DISPROVEN** here |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00a00000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-004c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00980000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\strings.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\xrefs.tsv`
- TLC `userst.ini` / `data\Defs\` (inventory only)
- `C:\FableCSharp\proofs\004CDB10-00A39010\README.md`
- `C:\FableCSharp\proofs\004CDB10-host-register\README.md`
- `C:\FableCSharp\proofs\00417A58-init-sound-body\README.md`
- `C:\FableCSharp\proofs\audio-initgame-first\README.md`
