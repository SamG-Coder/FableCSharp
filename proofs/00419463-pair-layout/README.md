# `00419463` pair at `game+90444` / `+90448`

Investigation only. No production `src/` edits.

Do **not** start at Oakvale / `00DBDE40` /
`Q_NewOakValeIntro` / `S_QNOVI`. After Leave this
walk is `FinalAlbion.wld` → `"Init Game"` →
`00418DCA` → vtbl+4 `004184BD` → `"Init Fonts"`
`004168DC`. Do **not** treat frontend type-6
`ENG_ARIAL_16` / persist `ENG_ARIAL_24` as this
slot.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **DIVERGE** / **MATCH**.

Question: `00419463` stores `{ptr, ctrl*}` at
`game+90444` / `+90448`. Exact fields? Host string
name leftover vs native face object? Getters
`0040EF10` / `00434DE0` first-seen after Init Fonts?

Authority: Fable.exe dump
`listing-00400000.txt` (`004168DC`–`00416952`,
`00419463`, `004190E2`, `0041947A`, `004057A0`,
`0041901D` / `00419028`, `00418E5A`, `00418BC1`,
`0041771E`, `0040EF10`, `00434DE0`, `00434DF0`,
`00434E10`, `00417418`, `0040B8E0`, `00435A71`,
`00436550`);
`listing-009c0000.txt` (`009E2C80` / `009E2BA0` /
`009E2C10`);
`listing-00a40000.txt` (`00A634F0` / `00A635E0`);
`listing-00480000.txt` (`00499BCA`);
`listing-00980000.txt` (`009BD831`);
`e8.tsv` dests `00419463` / `004190E2` / `004168DC`
/ `00434E10` / `004350D0` / `0040EF10` / `00434DE0`;
`functions.tsv` `004168DC` / `00417418`;
host `src/Fable.Game/EngineLifecycle.cs`
(`GameFontFace` / `Init Fonts` arm);
`src/Fable.Formats/Fonts/FontFile.cs`
(`GameFace` / `MainFaceCtorFn`);
siblings `proofs/004168DC-init-fonts`,
`proofs/004168DC-after-graphics`,
`proofs/0054E4F0-store-shape`.

---

## Verdict

**Exact store:** `ecx = game+90444`. `00419463`
is a generic pair copy (`ret 4`). It pushes
`src[4]` then `src[0]` into `004190E2`:

```
game+90444  face*     ; pair+0, T*
game+90448  ctrl*     ; pair+4, 12-byte block
```

`ctrl` is `{u32 refs, dtor*, T*}`. Same shape as
`0054E4F0` `{packet*, ctrl*}`. **Not** a
`char*` / `CString` at either dword.

`009E2C80("ENG_ARIAL_18")` returns that pair.
Lazy MAIN face is `00A634F0` → alloc `0x2034` →
`00AB8E10` then `0041947A`. Native slot is the
**face object**, keyed by the name.

Host `EnterGame` notes `004168DC` / `009E2C80` /
`00419463` then `GameFontFace = "ENG_ARIAL_18"`.
That is the **lookup key**, not `T*` / `ctrl*`.
**LEFTOVER** field shape. Name **MATCH**.

Getters `0040EF10` / `00434DE0` return **only**
`[game+90444]` (`face*`). They have **zero**
`.text` `E8` in `e8.tsv`. They are **not**
first-seen after Init Fonts on `004184BD`. Next
named sibling is `"Init Subtitled Message"`.
`00417418` does not `E8` `00434DE0`. First-seen
vtbl taker of those thunks is **UNREAD**.

| Claim | Class |
|---|---|
| `00419463` dest `ecx` is `game+90444` from `00416935` | **PROVEN** |
| `+90444` = `src[0]` ptr, `+90448` = `src[4]` ctrl* | **PROVEN** |
| `004190E2` same-ctrl skip; else `004057A0` then `inc [ctrl]` | **PROVEN** |
| `ctrl` size 12: `[0]` refs, `[4]` dtor, `[8]` `T*` | **PROVEN** (`0041947A` / `004057A0`) |
| Slot is the C-string `"ENG_ARIAL_18"` | **DISPROVEN** |
| Slot is native face `T*` from `00AB8E10` (`0x2034`) | **PROVEN** (MAIN lazy path) |
| Host `GameFontFace` string is native pair | **DISPROVEN** — **LEFTOVER** shape; name **MATCH** |
| `0040EF10` / `00434DE0` return `face*` only | **PROVEN** |
| Those getters first-seen `E8` after Init Fonts | **DISPROVEN** — no `E8` dest at all |
| `00434DE0` first-seen inside `00417418` / `00434E10` | **DISPROVEN** |
| First-seen vtbl `call [eax+n]` of those thunks | **UNREAD** |
| First writer of the slot on this walk | **PROVEN** `00419463` (ctor zeros earlier) |

---

## Direct answers

| Question | Answer | Class |
|---|---|---|
| Exact fields? | `{T* face, ctrl*}` at `+90444` / `+90448`. `ctrl` = `{refs, dtor*, T*}`. | **PROVEN** |
| Host string leftover vs native face? | **Yes.** Host stores the name. Native stores the `00AB8E10` face pair. | **PROVEN** leftover |
| Getters first-seen after Init Fonts? | **No** as `E8` / named-stage children. Bodies exist; consumers are later inline HUD / display reads. | **DISPROVEN** as first-seen; bodies **PROVEN** |

---

## 1. `00419463` / `004190E2`

`listing-00400000.txt`:

```
00419463  mov eax, [esp+4]     ; src pair*
00419467  push esi
00419468  push [eax+4]         ; ctrl*
0041946B  mov esi, ecx         ; dest
0041946D  push [eax]           ; T*
0041946F  call 004190E2
00419474  mov eax, esi
00419476  pop esi
00419477  ret 4
```

```
004190E2  push esi / push edi
004190E4  mov edi, [esp+16]    ; ctrl*
004190E8  mov esi, ecx         ; dest
004190EA  cmp [esi+4], edi
004190ED  je  00419103         ; same ctrl → no-op
004190EF  call 004057A0        ; drop old pair
004190F4  test edi, edi
004190F6  mov eax, [esp+12]    ; T*
004190FA  mov [esi], eax       ; dest+0
004190FC  mov [esi+4], edi     ; dest+4
004190FF  je  00419103
00419101  inc [edi]            ; refs++
00419103  pop edi / pop esi
00419105  ret 8
```

Release `004057A0` (same listing):

```
004057A0  mov eax, [esi+4]
          test eax, eax / je zero
          dec [eax]
          cmp [eax], 0 / jne zero
          mov ecx, [eax+8]     ; T*
          call [eax+4]         ; dtor
          push [esi+4]
          call 00BFE9BC        ; free ctrl
          zero: [esi]=0; [esi+4]=0
          ret
```

Create-from-ptr twin `0041947A` (used by `00A634F0`):

```
0041947A  call 004057A0
          mov [esi], eax       ; T*
          je  null-ctrl
          push 12
          call 00BFEA1A
          mov [eax], 0x1
          mov [eax+4], 0x419028   ; 00419028
          mov [eax+8], ecx        ; T*
          mov [esi+4], eax
```

`00419028` / `0041901D`: `push 1` / `call [ecx.vtbl+0]`
if `ecx != 0`.

Ctor `00418DCA` `00418E5A`:

```
lea eax, [esi+90444]
mov [eax], ebx          ; +90444 = 0
mov [eax+4], ebx        ; +90448 = 0
```

Dtor `00418BC1` / leave `0041771E` `004057A0` that
pair. Neighbor pair `+90436` / `+90440` is a
**different** `{ptr, ctrl*}` (`00418E4F`, getter
`00434DF0`). Not this slot.

`e8.tsv` dest `00419463` (only three):

| Site | Dest `ecx` | Name |
|---|---|---|
| `0041693B` | `game+90444` | `"ENG_ARIAL_18"` Init Fonts |
| `00499BCA` | `0x13B8998` | `"ENG_ARIAL_16"` progress UI |
| `009BD831` | `0x13CA7F8` | `"ENG_ARIAL_18"` overlay |

Helper is generic. This walk’s dest is only
`game+90444`. **PROVEN.**

---

## 2. Native value is the face object

`004168DC`:

```
push "ENG_ARIAL_18"
call 0099EBF0                 ; name CString (temp)
mov ecx, [0x13B838C]          ; font manager
lea eax, [ebp-4] / push
lea eax, [ebp-12] / push      ; dest pair
call 009E2C80
push eax
lea ecx, [esi+90444]
call 00419463
lea ecx, [ebp-12]
call 004057A0                 ; drop temp pair
```

`009E2C80` returns the dest pair*. MAIN hit /
miss both `009E2BA0`; STREAMING-only `009E2C10`.
Which arm first-seen New Game is **PARTIAL**
(sibling `004168DC-init-fonts`).

`009E2BA0` writes `[dest]=T*`, `[dest+4]=ctrl*`,
`inc [ctrl]`. `T*` comes from `00A635E0` then
`00A634F0`:

```
00A634F0  if [this+4] == 0:
            push 0x2034
            call 00BFEA1A
            call 00AB8E10        ; FontFile.MainFaceCtorFn
            call 0041947A        ; pair into bank slot
          copy pair to out, inc ctrl
```

So `game+90444` is that `00AB8E10` object (or a
prior MAIN insert of the same face), **not** the
`0099EBF0` string. The string is only the lookup
key. **PROVEN.**

---

## 3. Host leftover

`EngineLifecycle.EnterGame` Init Fonts arm:

```
Note(009E2C80, "ENG_ARIAL_18 [0x13B838C]");
Note(00419463, "[game+90444]");
GameFontFace = "ENG_ARIAL_18";
```

`GameFontFace` is `string?`. No `+90448` ctrl.
No `0x2034` face. Notes **MATCH** the VAs and
the name. Stored word **DISPROVEN** as native
`T*`. **LEFTOVER** shape (same class as
`0054E4F0-store-shape` host `MessageId` vs
packet*).

Frontend `FontBank` (`ENG_ARIAL_16` / `24`) is
still a different path. **DISPROVEN** as this
slot.

---

## 4. Getters `0040EF10` / `00434DE0`

```
0040EF10  mov eax, [ecx+90444]
          ret
00434DE0  mov eax, [ecx+8]
          mov eax, [eax+90444]
          ret
```

`0040EF10` sits with other CGame dword getters
(`+36` world, `+28`, `+40` display). `ecx` is
game. Returns **face***, not the pair, not
ctrl*, not the name.

`00434DE0`: display `this`. Ctor `00434E10`
`mov [esi+8], [blob+0]` — blob `+0` is game
(`00417418` packs it). Same `face*` via
`display+8`. Twin `00434DF0` reads `+90436`.

`e8.tsv`: **no** dest `0040EF10`, `00434DE0`,
or `00436550` (duplicate `mov eax,[ecx+90444];
ret`). Not `jmp` immediates in the listings
either. **PROVEN** unused as `.text` `E8`.

`004168DC` callees: log trio, `009E2C80`,
`00419463`, `004057A0`. No getters.

`004184BD` after `00418607` is `"Init Subtitled
Message"` `004CDB10`, not these thunks.

`functions.tsv` `00417418` callees:
`00434E10`, `0041940C` (`game+40` = display),
log trio, `004350D0`. **No** `00434DE0`.
Ctor does not load `+90444`.

Later **inline** readers (not the asked
thunks): display `00435A71` `[ecx+90444]`
after `[esi+8]`; HUD `0040B8E0`
`[ecx+90444]` → `[0x13B8790]+24`
`00493049`; `006444A3` `[esi+16]+90444` →
`00438010`. First-seen among those on a live
New Game frame is **UNREAD** here. None of
them run inside `004168DC`. Sibling proofs
already: “getters (not this walk).”

**DISPROVEN** that `0040EF10` / `00434DE0` are
first-seen after Init Fonts. **PROVEN** they
return the ptr half. Vtbl slot of each thunk
**UNREAD** (rdata not decoded as VAs).

---

## 5. What this does **not** say

- Host omit of `"Init Fonts"` as a named stage.
  **DISPROVEN** as current leftover — `InitGameStages`
  already has the row. Leftover is the pair shape.
- `009BD460` / `0x13CA7F8` is `game+90444`.
  **DISPROVEN**.
- `+90436` is the font face. **DISPROVEN**.
- Getters return `{ptr, ctrl*}`. **DISPROVEN** —
  `eax` is `T*` only.
- `004168DC` opens `fonts.big`. **UNREAD** here.
- MAIN vs STREAMING first-seen arm. **PARTIAL**
  (sibling).

---

## Classification (VAs)

| VA | Role | Class |
|---|---|---|
| `00419463` | copy `{T*, ctrl*}` into `ecx` | **PROVEN** |
| `004190E2` | assign + `inc [ctrl]` | **PROVEN** |
| `004057A0` | release pair | **PROVEN** |
| `0041947A` | wrap `T*` in new 12-byte ctrl | **PROVEN** |
| `00AB8E10` | MAIN face ctor `0x2034` | **PROVEN** on lazy miss |
| `0040EF10` | `return [game+90444]` | **PROVEN** body; **DISPROVEN** first-seen `E8` |
| `00434DE0` | `return [display+8]+90444` | **PROVEN** body; **DISPROVEN** on `00417418` |
| `00436550` | same body as `0040EF10` | **PROVEN** duplicate; no `E8` |
| `GameFontFace` string | host leftover vs `T*` | **PROVEN** leftover |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-009c0000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00a40000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00480000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00980000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\functions.tsv`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\src\Fable.Formats\Fonts\FontFile.cs`
- `C:\FableCSharp\proofs\004168DC-init-fonts\README.md`
- `C:\FableCSharp\proofs\0054E4F0-store-shape\README.md`
