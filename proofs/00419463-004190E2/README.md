# `00419463` → `004190E2` `{ptr, ctrl*}` at `game+90444` / `+90448`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave this
walk is `FinalAlbion.wld` → `"Init Game"` →
`00418DCA` → vtbl+4 `004184BD` → `"Init Fonts"`
`004168DC`. Do **not** treat frontend type-6
`ENG_ARIAL_16` / persist `ENG_ARIAL_24` as this
slot.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Question: `00419463` → `004190E2` `{ptr, ctrl*}`
at `game+90444` / `+90448`. Exact dwords after
Init Fonts? Host `GameFontFace` string leftover
vs pair?

Authority: Fable.exe dump
`listing-00400000.txt` (`004168DC`–`00416952`,
`00419463`, `004190E2`, `0041947A`, `004057A0`,
`00418E5A`);
`listing-009c0000.txt` (`009E2C80` / `009E2BA0` /
`009E2C10`);
`listing-00a40000.txt` (`00A634F0`);
`e8.tsv` dests `00419463` / `004190E2`;
`xrefs.tsv` `"Init Fonts"`;
host `src/Fable.Game/EngineLifecycle.cs`
(`GameFontFace` / Init Fonts arm);
`src/Fable.Formats/Fonts/FontFile.cs`
(`GameFace` / `MainFaceCtorFn`);
siblings `proofs/00419463-pair-layout`,
`proofs/004168DC-init-fonts`.

---

## Verdict

After Init Fonts the native slot is an 8-byte
shared pair, not a C-string:

```
game+90444  T*      face object   ; pair+0  (src[0])
game+90448  ctrl*   12-byte block ; pair+4  (src[4])
```

`00419463` (`ret 4`) copies that pair into
`ecx`. `004190E2` (`ret 8`) is the assign:
same `ctrl*` → no-op; else `004057A0` drop old,
store `{T*, ctrl*}`, `inc [ctrl]` if non-null.

`ctrl` layout from `0041947A` / `004057A0`:
`{u32 refs, dtor*, T*}`. Dtor site is
`00419028`. **Not** `char*` at either dword.

Source pair is `009E2C80("ENG_ARIAL_18")` on
`[0x13B838C]`. The name is the **lookup key**.
`T*` is the MAIN face (`00A634F0` lazy:
alloc `0x2034` → `00AB8E10` → wrap
`0041947A`). Live pointer values after this
call are **UNREAD** (no heap dump). Roles are
**PROVEN**.

Host `EnterGame` notes `009E2C80` / `00419463`
then `GameFontFace = "ENG_ARIAL_18"`. That is
`string?`. No `+90448`. **LEFTOVER** shape.
Name **MATCH**.

| Claim | Class |
|---|---|
| `0041693B` `ecx = game+90444` into `00419463` | **PROVEN** |
| `00419463` → `004190E2` copies `{src[0], src[4]}` | **PROVEN** |
| After Init Fonts: `+90444 = T*`, `+90448 = ctrl*` | **PROVEN** |
| Either dword is the C-string `"ENG_ARIAL_18"` | **DISPROVEN** |
| `ctrl` = `{refs, dtor* 00419028, T*}`, size 12 | **PROVEN** |
| Exact heap VAs of `T*` / `ctrl*` after the store | **UNREAD** |
| Host `GameFontFace` string **is** the native pair | **DISPROVEN** — **LEFTOVER** |
| Host name `"ENG_ARIAL_18"` | **MATCH** |
| Ctor zeros both dwords before this writer | **PROVEN** `00418E5A` |

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Exact dwords after Init Fonts? | `{face*, ctrl*}` at `+90444` / `+90448`. | **PROVEN** roles; values **UNREAD** |
| Host `GameFontFace` leftover vs pair? | **Yes.** Host stores the lookup name. Native stores the face pair. | **PROVEN** leftover |

---

## 1. Store path (`004168DC` → `00419463` → `004190E2`)

`listing-00400000.txt` Init Fonts work (log trio
omitted here):

```
00416912  push -1
00416914  push "ENG_ARIAL_18"
0041691C  call 0099EBF0
00416921  mov ecx, [0x13B838C]
0041692A  push &ebp-4
0041692E  push &ebp-12          ; dest pair
0041692F  call 009E2C80
00416934  push eax              ; src = returned pair*
00416935  lea ecx, [esi+90444]
0041693B  call 00419463
00416940  lea ecx, [ebp-12]
00416943  call 004057A0         ; drop temp pair
```

`esi` is game. `e8.tsv` dest `00419463`:
`0041693B`, `00499BCA`, `009BD831`. This walk’s
dest is only `game+90444`.

`00419463`:

```
00419463  mov eax, [esp+4]      ; src pair*
00419467  push esi
00419468  push [eax+4]          ; ctrl*
0041946B  mov esi, ecx          ; dest
0041946D  push [eax]            ; T*
0041946F  call 004190E2
00419474  mov eax, esi
00419476  pop esi
00419477  ret 4
```

`e8.tsv` `0041946F` → `004190E2`. **PROVEN.**

`004190E2`:

```
004190E2  push esi / push edi
004190E4  mov edi, [esp+16]     ; ctrl*
004190E8  mov esi, ecx
004190EA  cmp [esi+4], edi
004190ED  je  00419103          ; same ctrl → skip
004190EF  call 004057A0
004190F4  test edi, edi
004190F6  mov eax, [esp+12]     ; T*
004190FA  mov [esi], eax        ; dest+0 = T*
004190FC  mov [esi+4], edi      ; dest+4 = ctrl*
004190FF  je  00419103
00419101  inc [edi]             ; refs++
00419103  pop edi / pop esi
00419105  ret 8
```

Ctor `00418DCA` `00418E5A` (`ebx = 0`):

```
lea eax, [esi+90444]
mov [eax], ebx                  ; +90444 = 0
mov [eax+4], ebx                ; +90448 = 0
```

First-seen writer on this walk therefore always
takes the assign arm (`dest+4` was 0). After
return:

```
[game+90444] = temp[0]   ; face*
[game+90448] = temp[4]   ; ctrl*
```

Temp `ebp-12` is then `004057A0`’d, so the
game slot holds one extra ref vs the lookup
temp. Neighbor pair `+90436` / `+90440` is a
**different** `{ptr, ctrl*}`. **DISPROVEN** as
this slot.

---

## 2. What the two dwords are

`009E2BA0` (MAIN attach; STREAMING twin
`009E2C10` same pair write):

```
mov [esi], ecx                  ; dest+0 = T*
mov [esi+4], eax                ; dest+4 = ctrl*
inc [eax]                       ; refs++
```

`00A634F0` lazy MAIN miss:

```
push 0x2034
call 00BFEA1A
call 00AB8E10                   ; FontFile.MainFaceCtorFn
call 0041947A                   ; wrap into bank pair
copy pair to out; inc ctrl
```

`0041947A` (new ctrl):

```
call 004057A0
mov [esi], eax                  ; T*
push 12
call 00BFEA1A
mov [eax], 0x1                  ; refs
mov [eax+4], 0x419028           ; dtor 00419028
mov [eax+8], ecx                ; T*
mov [esi+4], eax                ; ctrl*
```

Release `004057A0` reads the same 12-byte
block (`dec [ctrl]`; on 0, `call [ctrl+4]`
with `ecx = [ctrl+8]`, free ctrl, zero both
dest dwords).

So after Init Fonts:

| Offset | Width | Value |
|---|---|---|
| `game+90444` | dword | face `T*` (`00AB8E10` object, size `0x2034` on lazy miss) |
| `game+90448` | dword | `ctrl*` |

**DISPROVEN:** `+90444` is `"ENG_ARIAL_18"` /
`0099EBF0` CString. That string lives in the
lookup arg and is dropped at `0041694B`.

Which `009E2C80` arm (MAIN hit vs MAIN insert
vs STREAMING) is first-seen New Game remains
**PARTIAL** (sibling `004168DC-init-fonts`).
Both attach helpers write the same pair shape.

---

## 3. Host `GameFontFace` leftover

`EngineLifecycle.EnterGame` Init Fonts arm:

```
Note(009E2C80, "ENG_ARIAL_18 [0x13B838C]");
Note(00419463, "[game+90444]");
GameFontFace = GameFontFaceName;   // "ENG_ARIAL_18"
```

`GameFontFace` is `string?`. Constants
`GameFontStoreFn = 0x00419463`,
`GameFontOffset = 90444` **MATCH** the VAs.
No host field for `+90448`. No `0x2034` face
object. Tests assert the **name** after
`ActivateNewGame`.

That is the lookup key, not `{T*, ctrl*}`.
**DISPROVEN** as the native pair.
**LEFTOVER** field shape. Name **MATCH**.

Frontend `FontBank` (`ENG_ARIAL_16` / `24`) is
a different path. **DISPROVEN** as this slot.

---

## 4. What this does **not** say

- Host omit of the named `"Init Fonts"` stage.
  **DISPROVEN** as current leftover —
  `InitGameStages` already has the row.
  Leftover is the stored word, not the stage.
- Getters `0040EF10` / `00434DE0` as first-seen
  after this store. Sibling
  `00419463-pair-layout`: bodies return `T*`
  only; no `.text` `E8`. Not asked here.
- Exact `T*` / `ctrl*` addresses in a live
  process. **UNREAD**.

---

## Classification (VAs)

| VA | Role | Class |
|---|---|---|
| `004168DC` | Init Fonts; lookup then store | **PROVEN** |
| `009E2C80` | name → pair out | **PROVEN** |
| `00419463` | copy `{T*, ctrl*}` into `ecx` | **PROVEN** |
| `004190E2` | assign + `inc [ctrl]` | **PROVEN** |
| `004057A0` | release pair | **PROVEN** |
| `0041947A` | wrap `T*` in 12-byte ctrl | **PROVEN** |
| `00AB8E10` | MAIN face ctor `0x2034` | **PROVEN** on lazy miss |
| `GameFontFace` string | host leftover vs pair | **PROVEN** leftover |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-009c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00a40000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\xrefs.tsv`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\src\Fable.Formats\Fonts\FontFile.cs`
- `C:\FableCSharp\proofs\00419463-pair-layout\README.md`
- `C:\FableCSharp\proofs\004168DC-init-fonts\README.md`
