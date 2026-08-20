# Named stage `"Init Sound"` `00417A58` first-seen body

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI` / `MUSIC_SET_*`.
After Leave this walk is `FinalAlbion.wld`
(`0042F44D`), then `"Init Game"` `0042F491` →
`00418DCA` → `[vtbl+4]` `004184BD`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: Init Game named stage `"Init Sound"`
`00417A58`. Host only Notes the name. What is the
first-seen body / first-seen callees? Relation to
later `00A01A4F` Init Symbols? Any bank open?

Authority: `Fable.exe` via ExeIndex
`listing-00400000.txt` (`004184BD` `0041883A` /
`00418886` / `00417A58`–`00418288` / `00415550` /
`004196B2` / `0041A080` / `0041A2D0` / `0041A390`),
`listing-00980000.txt` (`009919C0` / `00991840` /
`00991C10` / `009ADA40`),
`listing-00a00000.txt` (`00A01950`–`00A01A9B` /
`00A01A4F` / `00A39010` / `00A38500`),
`listing-00a40000.txt` (`00A42B80`),
`e8.tsv` dests `00417A58` / `009919C0` /
`00A01950` / `00A39010` / `00991C10` / `00991840`,
`functions.tsv` `004184BD` / `00417A58`,
`xrefs.tsv` / `strings.tsv` (`"Init Sound"`
`0x0122F078`);
host `src/Fable.Game/EngineLifecycle.cs`
`InitGameStages` / `EnterGame` (read only).

Siblings: `proofs/audio-initgame-first`,
`proofs/leave-first-sound`,
`proofs/004CDB10-00A39010`,
`proofs/004166A8-create-players-work`,
`proofs/initgame-after-leave-order`.

---

## Verdict

**First-seen child of `00417A58` is register, not
play, not a mesh-style bank Open.** After the
`[0x13B8394]==0` skip (not taken after Leave),
first `E8` is the `"Lut register"` log trio.
First *work* `E8` is `00415550` (`*_SOUND_SETUP`
name) then `0044C6B0` / `004196B2` (`009ADA40`
def lookup). First *audio* `E8` is `009919C0`
(`"Registering Localised Sound Bank"`).

`00A01A4F` is **not** a sibling Init Game stage.
It is `call 00A39010` inside `00A01950`
(`"Sound Bank: Init Symbols"`), reached only from
`009919C0` `00991A44`. First-seen `00A39010` on
this walk is still Init Subtitled `004CDB46`.
`00A01A4F` is the **second** `.text` `E8` of
that helper — later, nested under this stage.

Host `EnterGame` **Notes** `"Init Sound"` /
`0x00417A58` and does **no** body. Name
**MATCH**. Work **LEFTOVER** Note-only.

| Claim | Class |
|---|---|
| Native `004184BD`: Create Players `004166A8` then `"Init Sound"` `00417A58` then Load Particles `004174F1` | **PROVEN** |
| Only `.text` `E8` of `00417A58` is `00418886` | **PROVEN** |
| Gate `[0x13B8394]==0` → `je 00418286` (no register) | **PROVEN** |
| After Leave `[0x13B8394]` is live → body runs | **PROVEN** (`leave-first-sound`) |
| First `E8` `"Lut register"` `0099EBF0` | **PROVEN** |
| First work `00415550` → `0044C6B0` → `004196B2` | **PROVEN** |
| First audio register `009919C0` (only two `E8`, both here) | **PROVEN** |
| `00991840` is map lookup (`audio+48`), not Open | **PROVEN** |
| `00991C10` only `E8` is `0041816A` (atmos register) | **PROVEN** |
| Tail `00991840(1)` → `[game+16]` | **PROVEN** |
| `00A01A4F` is a top-level Init Game stage | **DISPROVEN** — site inside `00A01950` |
| `00A01950` only `E8` is `00991A44` (`009919C0`) | **PROVEN** |
| `00A39010` first-seen is `004CDB46`; second is `00A01A4F` | **PROVEN** |
| This stage **opens** a sound bank (graphic-bank analog) | **DISPROVEN** — strings / callees are Register |
| `00A39010` / `00A42B80` may read a **symbol / payload** file | **PARTIAL** (nested; path hit **UNREAD**) |
| Host named-stage `Note("Init Sound")` | **MATCH** name |
| Host runs `00417A58` / `009919C0` / `00A01950` | **DISPROVEN** — **LEFTOVER** Note-only |
| `00417A58` plays `SND_*` / `vtbl+68` / `00A01920` | **DISPROVEN** (`audio-initgame-first`) |
| Oakvale / `00DBDE40` | **DISPROVEN** here |

---

## 1. Named site after Create Players

`listing-00400000.txt` `004184BD`:

```
00418808  push "Create Players"
00418834  call 004166A8
0041883A  push "Init Sound"          ; log
…
0041885A  push "Init Sound"
…
00418884  mov ecx, esi               ; game
00418886  call 00417A58
0041888B  cmp [0x13B8648], bl
00418891  jne 004188E5
00418894  push "Load Particles"
```

`e8.tsv` dest `00417A58`: **only** `00418886`.
`functions.tsv` `004184BD` callees:
`004166A8` then `00417A58` then `004174F1`.

Host `InitGameStages` twelfth name is
`("Init Sound", 0x00417A58)` after
`"Create Players"`. `EnterGame` only
`Note(apply, name, …)`. No `if (name ==
"Init Sound")` body. **MATCH** name.
**LEFTOVER** work.

---

## 2. `00417A58` body (first-seen)

`listing-00400000.txt` `00417A58`–`00418288`
(next fn `00418289`). `functions.tsv` size
`658` does **not** match the listing span —
**PARTIAL** index.

```
00417A58  push ebp
00417A5B  sub esp, 124
00417A61  mov [ecx+16], ebx          ; [game+16]=0
00417A64  cmp [0x13B8394], ebx
00417A6A  mov [ebp-124], ecx
00417A6D  je  00418286               ; skip all
00417A77  push "Lut register"
00417A7F  call 0099EBF0              ; FIRST E8
00417A8A  call 009D8240
00417A92  call 0099EAE0
00417AA1  call 00415550              ; FIRST WORK
00417AA7  call 0044C6B0
00417AAE  call 004196B2              ; 009ADA40
          edi = [ebp-24]
          count = ([edi+64]-[edi+60])/20
          jbe 00417CB8               ; empty table
```

Then three **register** loops, then a lookup
store:

| Loop / tail | Log | First audio dest |
|---|---|---|
| Localised | `"Init Localised Sound Bank Entries"` / `"Registering Localised Sound Bank"` | `00417C67 call 009919C0` |
| Main | `"MAIN_SOUND_SETUP"` / `"Init Sound Bank Entries"` / `"Registering Sound Bank"` | `00417F86 call 009919C0`; then maybe `00991840` + `[vtbl+12]` |
| Atmos | `"Registering Atmos Sound Bank"` | `0041816A call 00991C10`; then `00991840` + `[vtbl+12]` |
| Tail | — | `00418259 call 00991840(1)` → `[game+16]` |

`e8.tsv` dest `009919C0`: **only** `00417C67` /
`00417F86`. Dest `00991C10`: **only**
`0041816A`. First-seen register is the
**localised** `009919C0`. **PROVEN.**

No `call [eax+68]`. No `00A01920`. No
`"Opening … Sound Bank"` (`strings.tsv` has
`"Opening Main Graphic Bank"` only). **PROVEN**
register, not play, not graphic-bank Open.

---

## 3. First-seen callees (direct)

Order from `functions.tsv` / listing. Plumbing
`0099EBF0` / `009D8240` / `0099EAE0` /
`009E9F40` / `0099B510` omitted after first
mention.

| # | Dest | Role | Class |
|---:|---|---|---|
| 1 | `0099EBF0` | `"Lut register"` | **PROVEN** first `E8` |
| 2 | `00415550` | locale → `"ENGLISH_SOUND_SETUP"` default / `FRENCH_*`… | **PROVEN** first work |
| 3 | `0044C6B0` | `[0x13B879C]` player-manager getter | **PROVEN** |
| 4 | `004196B2` | `009ADA40` named-def lookup | **PROVEN** |
| 5 | `00415D90` / `00415DD0` | copy 20-byte entry strings | **PROVEN** |
| 6 | `0041A2D0` / `0041A080` | wchar prefix join (`0x122DE78` / `0x122F3D0`) | **PROVEN** join; prefix **UNREAD** (wchar) |
| 7 | `009919C0` | `"Register Sound Bank 1/2"` | **PROVEN** first audio |
| 8 | `0041A390` / `0041A3B0` | prefix `0x122F4EC` | **UNREAD** wchar |
| 9 | `00991840` | find registered id | **PROVEN** lookup |
| 10 | `[edi+12]` | bank vtbl+12 | **PARTIAL** payload |
| 11 | `00991C10` | `"Register Atmos Sound Bank"` | **PROVEN** |
| 12 | `00991840(1)` | `[game+16]` | **PROVEN** |

`00415550` (`00415070` language cache
`[0x13B8684]`): `eax-4>14` →
`"ENGLISH_SOUND_SETUP"`. Cached id / jump-table
slot for TLC **PARTIAL** (`00851770-default-seed`
probes `TEXT_ENGLISH_MAIN` → `9`).

`004196B2` miss leaves `[ebp-24]==0` then
`[edi+64]` would fault. After `"Init Definition
Manager"`, first-seen is a **hit**. Table
contents (how many 20-byte rows, paths)
**UNREAD** (defs not in this dump).

---

## 4. `00A01A4F` Init Symbols — later nested `00A39010`

`00A01A4F` is **not** a function start:

```
00A01950  …                          ; Sound Bank: Init Symbols
00A019D3  push "Sound Bank: Init Symbols"
00A019FC  push 36
00A01A03  call 00BFEA1A
00A01A0C  call 00A38500              ; heap symbol table
00A01A1B  call 00A01AA0              ; store at bank+4
00A01A20  mov al, [0x13BC9F0]
00A01A27  je  00A01A37
          call 00A38C20              ; packed path
00A01A4F  call 00A39010              ; file enum fill
```

`e8.tsv` dest `00A01950`: **only** `00991A44`.
`009919C0`:

```
009919CB  push "Register Sound Bank 1"
          call [eax+4]               ; audio vtbl+4 (name)
00991A02  push "Register Sound Bank 2"
          0099B150(path)
00991A3F  je  00991A49               ; empty → skip symbols
00991A44  call 00A01950
          …
          00BFEA1A(0x180) / 00A42B80
          [edx+4](path, 2)
          insert audio+48 (36-byte slot)
```

`e8.tsv` dest `00A39010`: `004CDB46` then
`00A01A4F` only.

| Site | `this` | When |
|---|---|---|
| `004CDB46` | BSS `[0x13B8A54]` | `"Init Subtitled Message"` — **first-seen** |
| `00A01A4F` | heap `00A38500` at bank+4 | inside first nonempty `009919C0` |

So `"Init Symbols"` is **the same fill helper**,
later, on a **per-bank** object. It is not a
second Init Game name. Host has no analog.

`[0x13BC9F0]` is copied from `[0x13B8619]` at
`"Setup Basic install files"` (`004025EE`).
If set, `00A38C20` is taken and `00A01A4F` is
**not** first-seen. Flag first-seen **UNREAD**.
Empty bank path also skips `00A01950`. Fire of
`00A01A4F` on the first localised row is
**PARTIAL**.

`00A39010` still: lock, clear `+20`,
`0099B7D0` file-stack `0x13D27E8`,
`00A38E50` `"enum"`. Symbol-file **fill**,
not a voice start (`004CDB10-00A39010`).

---

## 5. Any bank open?

**No** Open analog of `"Opening Main Graphic
Bank"` / mesh `OpenFn`.

| What | Open? | Class |
|---|---|---|
| Stage strings | `"Registering … Sound Bank"` / `"Register Sound Bank 1/2"` | **PROVEN** register |
| `00991840` | `audio+48` map find by id; ret ptr or 0 | **DISPROVEN** as Open |
| `009919C0` / `00991C10` | vtbl+4 name + insert `+48` | **PROVEN** register |
| `00A01950` / `00A01A4F` | `00A39010` enum symbols | **PROVEN** fill; file-stack **PARTIAL** |
| `00A42B80` then `[vtbl+4](path, 2)` | 0x180 ctor `0129D3D4` | **UNREAD** (could read payload; not named Open) |
| `[game+16] = 00991840(1)` | store looked-up bank 1 | **PROVEN** not Open |

Do **not** invent a host `OpenSoundBank()` from
this name. Adding work is `009919C0` /
`00991C10` register + optional nested
`00A01950`.

---

## 6. Host leftover vs Note-only

`EnterGame` (read only):

```
foreach InitGameStages
  Note(apply, name, "InitGame", name)
  "Init Subtitled Message" → extra 00A39010 notes
  "Create Players"         → CreatePlayers()
  "Init Sound"             → (nothing else)
```

| Host | Native | Class |
|---|---|---|
| `Note(00417A58 "Init Sound")` | `00418886` | **MATCH** name |
| no `[0x13B8394]` gate | `je 00418286` | **LEFTOVER** |
| no `00415550` / `004196B2` | locale + `MAIN_SOUND_SETUP` | **LEFTOVER** |
| no `009919C0` / `00991C10` | register | **LEFTOVER** |
| no `00A01950` / `00A01A4F` | nested symbols | **LEFTOVER** |
| no `[game+16]` | `00991840(1)` | **LEFTOVER** |
| no `SND_*` / `00A01920` | none here | **MATCH** skip |

Adding another Note-only line would still be
**LEFTOVER**. Real leftover **is** the register.

---

## 7. What this does **not** say

- First `SND_*` / `MUSIC_SET_*` / `vtbl+68`.
  **DISPROVEN** first-seen (`audio-initgame-first`).
- `Init Atmos` `006B1960` `vtbl+144`. Later
  `[game].vtbl+32` `00416953`, not this stage.
- `00A01A4F` first-seen `00A39010`. **DISPROVEN**
  — `004CDB46` is first.
- Bank file names / wchar prefixes `0x122F3D0` /
  `0x122F4EC`. **UNREAD**.
- `functions.tsv` callee list replaces the
  listing. **PARTIAL** — use the listing.

---

## Classification (VAs)

| VA | Role | Class |
|---|---|---|
| `00418886` | only caller of `00417A58` | **PROVEN** |
| `00417A58` | Init Sound body | **PROVEN**; host **LEFTOVER** Note-only |
| `00415550` | first-seen work | **PROVEN** |
| `0044C6B0` / `004196B2` | def lookup | **PROVEN** |
| `009919C0` | first-seen register | **PROVEN** |
| `00991840` | lookup | **PROVEN**; **DISPROVEN** Open |
| `00991C10` | atmos register | **PROVEN** |
| `00A01950` | `"Sound Bank: Init Symbols"` | **PROVEN** nested |
| `00A01A4F` | second `00A39010` | **PROVEN** later; fire **PARTIAL** |
| `004CDB46` | first `00A39010` | **PROVEN** earlier stage |
| `00A42B80` | 0x180 after register | **UNREAD** |
| `00DBDE40` | Oakvale | **DISPROVEN** here |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00980000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00a00000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00a40000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\functions.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\strings.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\xrefs.tsv`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\proofs\audio-initgame-first\README.md`
- `C:\FableCSharp\proofs\leave-first-sound\README.md`
- `C:\FableCSharp\proofs\004CDB10-00A39010\README.md`
