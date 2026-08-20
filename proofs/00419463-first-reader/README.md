# First reader of `game+90444` / `+90448` after Init Fonts

Investigation only. No production `src/` / `tests/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave this
walk is `FinalAlbion.wld` → `"Init Game"` →
`00418DCA` → vtbl+4 `004184BD` → `"Init Fonts"`
`004168DC`. Do **not** invent `fonts.big` open.
Do **not** treat frontend type-6 `ENG_ARIAL_16` /
persist `ENG_ARIAL_24` as this slot.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Question: after Init Fonts `004168DC` stores
`{T*, ctrl*}` at `game+90444` / `+90448` via
`00419463` → `004190E2`, who first **READS** those
dwords on the `004184BD` walk or first `004189C2`
pump? Host `GameFontFace` is still the lookup name
string (leftover shape). What would a first-seen
read expect (`T*` vs name)?

Authority: Fable.exe dump xrefs to `game+90444`
(listing `+90444`; no `+90448` mnemonic);
`00419463` / `004190E2` / `0041947A`;
`listing-00400000.txt` (`004168DC`, `004184BD`
`00418607`–`004189C1`, `004189C2`–`00418B77`,
`0040B790` / `0040B8E0`, `0040EF10`, `0040D2A0` /
`0040BC80` / `0040A7F0`, `004162B5`, `00417747`,
`00435F70` jmp `00435530`, `00435A71` / `00435C76`,
`00434DE0`, `00436550`, `00417418` / `004350D0`,
`00418E5A` / `00418BC1` / `0041771E`);
`listing-00480000.txt` (`004A386A` in `004A3740`);
`listing-00640000.txt` (`006444A3`, `00640040`);
`listing-006c0000.txt` (`006C9FA0`);
`e8.tsv` dests `00419463` / `004190E2` / `0041947A`
/ `004168DC` / `0040B790` / `00435F70` / `004350D0`
/ `004057A0`;
`functions.tsv` `004184BD` / `004189C2` / `00417418`
/ `004162B5` / `00417747`;
host `src/Fable.Game/EngineLifecycle.cs`
(`GameFontFace` / Init Fonts arm);
siblings `proofs/00419463-pair-layout`,
`proofs/00419463-004190E2`,
`proofs/004168DC-init-fonts`,
`proofs/13B8A54-first-reader`.

---

## Verdict

**No first reader on the asked walk.** After
`0041693B call 00419463` the rest of `004184BD`
never loads `+90444` / `+90448`. First
`004189C2` takes dummy `004FC180` then
`0040D2A0` / `0040BC80` / `0040A7F0` and inner
`[game+52]==0` → `009F8BA0` / `004162B5`, not
`00417747` / `00435F70`.

`+90448` (`ctrl*`) has **zero** listing
mnemonics. The only later native read of that
half is `004057A0` on `lea ecx,[esi+90444]`
(dtor `00418BC1` / leave `0041771E`). Not this
walk.

First leftover *site* (lowest VA load after the
store) is `0040B8E0` inside `0040B790`:
`edx = [ecx+90444]` then `00493049` (`[0x13B8790]+24`).
`e8.tsv` dest `0040B790`: `0040BB81`, `006C9FB1`.
Neither is on `004184BD` or first `004189C2`.

First leftover *Present* consumer is `00435A71`
inside `00435530` (`00435F70` jmp). Gated:
WorldFrame `<=1` skips the camera/Present body;
first-seen `[0x13B8688]=0` and `004AEA70=0`
skip `00435F70`.

Every leftover load is the **face `T*`**, not
the name, not `ctrl*`. `00435C76` does
`edi=[game+90444]; call [edi.vtbl+8]`. A C-string
at that dword is **DISPROVEN**.

Host `GameFontFace = "ENG_ARIAL_18"` is still
the lookup key. **LEFTOVER** shape. Name
**MATCH**.

| Claim | Class |
|---|---|
| Store is `{T*, ctrl*}` via `00419463`→`004190E2` | **PROVEN** (siblings) |
| `004184BD` after `00418607` loads `+90444`/`+90448` | **DISPROVEN** — **PROVEN** empty |
| First `004189C2` head / `004162B5` loads the slot | **DISPROVEN** — **PROVEN** empty |
| `+90448` mnemonic in listings | **DISPROVEN** (none) |
| First leftover site `0040B8E0` | **PROVEN** site; **DISPROVEN** this walk |
| First leftover Present `00435A71` / `00435F70` | **PROVEN** site; **DISPROVEN** first pump |
| First-seen read expects the name string | **DISPROVEN** |
| First-seen read expects face `T*` | **PROVEN** (leftover sites) |
| Host `GameFontFace` string is the native pair | **DISPROVEN** — **LEFTOVER** |
| Host name `"ENG_ARIAL_18"` | **MATCH** |
| `fonts.big` open on this read | **UNREAD** (not claimed) |
| Oakvale / `00DBDE40` here | **DISPROVEN** |

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| First reader VA on `004184BD` after the store? | **none** | **PROVEN** empty |
| First reader VA on first `004189C2`? | **none** | **PROVEN** empty |
| First leftover reader VA | `0040B8E0` (`0040B790`); Present `00435A71` (`00435F70`) | **PROVEN** sites |
| Offsets read | `+90444` only (`T*`). `+90448` unread until dtor | **PROVEN** |
| Expect `T*` vs name? | **`T*` face.** `00435C76` calls `[T*.vtbl+8]`. | **PROVEN** |
| Host leftover? | `GameFontFace` still the name string | **PROVEN** leftover |

Overall: **PROVEN** empty on the asked walk.
Leftover readers **PROVEN**. Host **LEFTOVER**.

---

## 1. Store, then closed `+90444` set

`004168DC` (`00418607` on `004184BD`):

```
0041692F  call 009E2C80          ; "ENG_ARIAL_18" → temp pair
00416935  lea ecx, [esi+90444]
0041693B  call 00419463          ; copy {src[0], src[4]}
00416940  lea ecx, [ebp-12]
00416943  call 004057A0          ; drop temp, not the game slot
```

`00419463` → `004190E2` writes:

```
game+90444  T*      face
game+90448  ctrl*   {refs, dtor* 00419028, T*}
```

`e8.tsv` dest `00419463`: `0041693B` (this dest),
`00499BCA` (`0x13B8998` / `ENG_ARIAL_16`),
`009BD831` (`0x13CA7F8` overlay). Dest
`004190E2`: only `0041946F`. Dest `0041947A`
(new ctrl wrap): `00A5F1F4`, `00A63524` (MAIN
lazy `00A634F0`). **DISPROVEN** as readers of
the game slot.

Listing mnemonic `+90444` in `004xxxxx`
(complete):

| VA | Role | This walk? |
|---|---|---|
| `00416935` | store `lea` | writer |
| `00418E5A` | ctor zero | before Fonts |
| `0040EF10` | `return [ecx+90444]` | no `.text` `E8`/`jmp` |
| `00434DE3` | `return [[ecx+8]+90444]` | no `.text` `E8` |
| `00436550` | same as `0040EF10` | no `.text` `E8` |
| `0040B8E0` | HUD `0040B790` | leftover |
| `00435A71` / `AD9` / `BAA` / `C76` | Present `00435530` | leftover; later pump |
| `0041771E` / `00418BC1` | leave / dtor `004057A0` | not New Game |
| `004A386A` | `004A3740` world switch | type-1 / later |

No `+90448` in any `listing-*.txt`. Other
banks (`006444A3`, `0059DB69`, `0062A47B`,
`007Exxxx`, …) are HUD / GUI / later draw.
**PROVEN** closed set for this question.

`xrefs.tsv` has no row for offset `90444`
(it is not a VA). Dump xrefs **are** the
listing loads above.

---

## 2. `004184BD` after `00418607` is empty

`004184BD` body after Init Fonts (`esi` = game):

```
00418607  call 004168DC
00418637  call 004CDB10          ; Init Subtitled Message
00418692  call 004CD670          ; Init Conversation Attitude
004186E4  call 0041732A          ; Init Player Manager
00418729  call 004473A0          ; Player Interface
00418746  call 0049E740          ; optional
00418784  call 0041735A          ; Init World
004187E2  call 00417418          ; Init Display Engine
00418834  call 004166A8          ; Create Players
00418886  call 00417A58          ; Init Sound
004188E0  call 004174F1          ; Load Particles
004188E9  call [eax+32]          ; 00416953 Loading world
00418901  call 0049BA70
0041890E  call 00416392
0041891D  call 004AE9D0
0041895C / 00418981  009EC890    ; user.ini / Gameflow
```

None of those VAs appear in the `+90444` table.
`functions.tsv` `00417418`: `00434E10`,
`0041940C` (`game+40` = display), `004350D0`.
`00434E10` copies blob `+0` (game) to
`display+8` and does **not** load `+90444`.
`004350D0` logs, `[eax+4]`, alloc 8,
`00640040` (zeros two dwords). Next sibling
`00435190` has no `+90444` before Present
`00435530`.

`004166A8` first-seen: `0044C6B0` / `0044A530`
/ `004AE940`. `listing-00440000.txt` has **zero**
`+90444`.

**PROVEN** empty on Init Game after the store.

---

## 3. First `004189C2` is empty

`004189C2` first-seen (flags default 0;
`[0x13B8628]==0`; dummy record):

```
004AE9C0 / 009E1BC0 / 00416231
004FB150 / 004FC180              ; index 0 dummy
0040D2A0                         ; alloc 0x140, 0040CEC0 +51=1
0040BC80                         ; 00407370 + jmp 0040A7F0
[game+40]+44 vtbl+220 00B239A0
009F2660 / 009F26B0
jmp 00418B48 → [game+8]==0 → 00418AB1
0098E1B0 / 009A6460 → 1
[game+52]==0 → 009F8BA0 / 004162B5
00416202 / 00415E85 skip / 009AC9E0
```

`0040A7F0` is PARTIAL as a *body*, but the
closed `+90444` list has no load in
`0040A7F0`–`0040B74F`. It does **not**
`E8` `0040B790`. First pump uses `0040BC80`,
not `0040BB30` / `0040B790`.

`functions.tsv` `004162B5`: `009A4EC0`,
`009A57B0`, `009E1BC0`, `009E9FB0`,
`00434BA0`. `00434BA0` writes display fade
fields. No `+90444`.

`00417747` **does** `E8` `00435F70`
(`004178F0`). First pump `[game+52]==0`
skips `00417747`. `00435F70` other `E8`s:
`004165B4` (interpolation `0041649C`),
`00417237` (`00417001` WorldFrame `>1`),
`00406D1E` / `00406D34` (`00406CC0`; dests
`004212CE`, `004214B6`, `005E5F54`). None
are first dummy pump.

`004A386A` lives in `004A3740` (`[world+260]`
switch, `TEXT_GUI_STAY`). Not first
`004189C2`.

**PROVEN** empty on first pump.

---

## 4. Leftover readers expect `T*`

`0040B8E0` (`0040B790`; `ecx` from
`[0x13B86A0]` game):

```
0040B8E0  mov edx, [ecx+90444]
          mov ecx, [0x13B8790]
          push edx
          add ecx, 24
          call 00493049
```

`edx` is the font object pointer into the GUI
draw helper. **Not** a name compare.

`00435A71` (`ecx = [display+8]` = game):

```
00435A71  mov eax, [ecx+90444]
          push eax
          … 0099B6B0 / 009DD8F0
```

`00435C76`:

```
00435C76  mov edi, [ecx+90444]
00435C7C  mov edx, [edi]
00435C7E  mov ecx, edi
00435C80  call [edx+8]           ; face vtbl+8
```

A leftover `"ENG_ARIAL_18"` C-string at
`+90444` would be **DISPROVEN** here (`[char*]`
is not a vtbl). Getters `0040EF10` /
`00434DE0` / `00436550` return that same
`T*` only. **PROVEN** unused as `E8`.

`006444A3`: `[esi+16]+90444` → `00438010`
(HUD). Later draw. Same `T*` shape.

---

## 5. Host leftover

`EngineLifecycle.EnterGame` Init Fonts arm:

```
Note(009E2C80, "ENG_ARIAL_18 [0x13B838C]");
Note(00419463, "[game+90444]");
GameFontFace = "ENG_ARIAL_18";
```

`GameFontFace` is `string?`. Tests assert the
**name** after `ActivateNewGame`. No host
`+90448`. No `0x2034` face. No first-reader
note on `004184BD` / first `004189C2`.

Adding `0040B790` / `00435F70` / `004A386A`
as New Game or first-pump work would
**DIVERGE**. Inventing `fonts.big` open on
this read would **DIVERGE**.

---

## Classification (VAs)

| VA | Role | Class |
|---|---|---|
| `0041693B` `00419463` | store pair | **PROVEN** writer |
| `004190E2` | assign `{T*, ctrl*}` | **PROVEN** |
| `0041947A` | wrap new ctrl (bank) | **DISPROVEN** as this slot reader |
| Init Game / first `004189C2` reader | none | **PROVEN** empty |
| `game+90448` on this walk | unread | **PROVEN** empty |
| `0040EF10` / `00434DE0` / `00436550` | return `T*` | **PROVEN** body; **DISPROVEN** `E8` |
| `0040B8E0` | first leftover load | **PROVEN** site; **DISPROVEN** this walk |
| `00435A71` / `00435C76` | Present; `T*` / vtbl+8 | **PROVEN** leftover; **DISPROVEN** first pump |
| `004A386A` | later world GUI | **DISPROVEN** this walk |
| `GameFontFace` string | host leftover vs `T*` | **PROVEN** leftover |
| `fonts.big` / Oakvale | — | **UNREAD** / **DISPROVEN** |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00480000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00640000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-006c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\functions.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\xrefs.tsv`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\proofs\00419463-pair-layout\README.md`
- `C:\FableCSharp\proofs\00419463-004190E2\README.md`
- `C:\FableCSharp\proofs\004168DC-init-fonts\README.md`
- `C:\FableCSharp\proofs\13B8A54-first-reader\README.md`
