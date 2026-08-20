# `0043314A` `setne` dest is `CUIDef+545` (0/1); type 11 vs 38 consumers differ

Investigation only. No production `src/` edits.

Question: `0043314A` writes `CUIDef+545`. Called from
`00632233` / `00631C60`. Confirm dest **`+545`**, payload
**0/1 only**, and that **type 11 vs 38** consumers differ.

Authority: `Fable.exe` dump identity
`42D7DBDF-0106C000-16666624`
(`tools/Fable.ExeIndex/out/01-sections/text-map/INDEX.md`);
`listing-00600000.txt` (`00631C60` / `00632233` / copy
`00631C38` / size `006314B0`);
`listing-00400000.txt` (`0043314A` / `00403EB0` /
`00404500`; also
`implementer/frontend/fn-0043314A-exact.txt`);
`listing-00540000.txt` (`0054DBC0` / `0054DC30` /
`0054E0B0` / `00558B90` / `0055AD60` / `0055AEB0`).

Do not re-prove CRC `0x9E47F106` file hex on INVISIBLE /
NEW_GAME / ACCEPT (`proofs/cuidef-plus545`,
`proofs/accept-newgame-plus545`). Do not invent a Lionhead
name for that CRC.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN**.

---

## Verdict

| Claim | Status |
| --- | --- |
| `00631C60` is CUIDef persist; `esi` is `CUIDef*` | **PROVEN** |
| `00632233` `lea edx,[esi+545]` / `push edx` / `call 0043314A` | **PROVEN** |
| Dest of that call is **`CUIDef+545`** | **PROVEN** |
| `+545` is a byte (copy `00631C38`; size `0x228`) | **PROVEN** |
| File load (mode 2) `0043314A` → `00403EB0` `setne` / `mov [edx],cl` | **PROVEN** |
| Loaded dest is **0 or 1 only** | **PROVEN** |
| File payload is one `u8` after skipped CRC (`00404500` +4) | **PROVEN** |
| Raw file byte other than 0/1 survives as dest | **DISPROVEN** (nonzero → 1) |
| Type 11 apply `0054DBC0` tests `[def+545]` and skips `0055AD60` when 0 | **PROVEN** |
| Type 11 activate `0054DC30` tests `[def+545]` | **PROVEN** |
| Type 38 apply body `0055AD60` reads `+545` | **DISPROVEN** |
| Type 38 enable `0055AEB0` reads `+545` | **DISPROVEN** |
| Type 11 vs 38 consumers of `+545` differ | **PROVEN** |
| Type 38 inner vtbl `0124B024+4` dword is `0055AD60` | **PARTIAL** (ctor installs inner; `.rdata` slot **UNREAD**) |
| `setne` lives inside `0043314A` itself | **DISPROVEN** (it is `00403EB0`) |

**Answer:** dest is **`CUIDef+545`**. Persist load stores
**0 or 1**. Type 11 gates on the byte; type 38 apply/enable
do not.

---

## 1. Caller `00631C60` / `00632233`

CUIDef persist starts at `00631C60` (`listing-00600000.txt`):

```
00631C60  push esi
          mov  esi, ecx              ; CUIDef*
          push edi
          mov  edi, [esp+12]         ; persist stream
          …
```

Size getter `006314B0` `mov eax, 0x228`. Copy ctor ends:

```
00631C38  mov dl, [edi+545]
00631C3E  mov [esi+545], dl
00631C44  mov eax, [edi+548]
```

`+545` is one byte; `+546/+547` padding before `+548`.

Tail of persist (`listing-00600000.txt`):

```
00632217  lea eax, [esi+544]
          push eax
          mov  ecx, edi
          call 0043314A
00632225  lea ecx, [esi+522]
          push ecx
          mov  ecx, edi
          call 0043314A
00632233  lea edx, [esi+545]
00632239  push edx                   ; dest*
0063223A  mov  ecx, edi              ; stream this
0063223C  call 0043314A
00632241  add esi, 0x224             ; CUIDef+548
          call 006326C0
```

Dest of the `00632233` call is **`[esi+545]`**. That is
the whole claim for dest.

---

## 2. `0043314A` file load → `00403EB0` `setne`

`listing-00400000.txt` / `fn-0043314A-exact.txt`:

```
0043314A  push ebp
          mov  ebp, esp
          push ecx
          push esi
          push 0x122D70E
          mov  esi, ecx              ; stream
          call 00404500              ; mode 2: skip 4-byte field CRC
          mov  eax, [esi+24]
          dec  eax
          dec  eax
          mov  [ebp-4], 0x00
          je   0043317A              ; mode == 2 (file load)
          dec  eax
          jne  00433188              ; mode != 3: no dest write
          mov  eax, [ebp+8]
          movzx eax, [eax]           ; mode 3: serialize dest byte
          …
0043317A  push [ebp+8]               ; dest* = CUIDef+545
          lea  ecx, [ebp-4]
          push [esi+36]
          call 00403EB0
0043318A  ret  4
```

`00404500` mode 2 (`[esi+24]==2`) advances the stream
cursor by 4 (`add esi, 4` / `add eax, -4`). Field CRC is
skipped, not matched.

`00403EB0` (`stdcall` dest last):

```
00403EB0  mov ecx, [esp+4]           ; stream
          …
          mov dl, [eax]              ; one file byte
          inc eax
          …
          test al, al
          mov edx, [esp+16]          ; dest*
          pop esi
          setne ecx
          mov [edx], cl              ; 0 or 1
          pop edi
          ret 8
```

Fail path `00403F19` `setne` / `mov [edx],cl` is the same
boolean store.

Loaded dest is **only 0 or 1**. A file `u8` of 2 still
becomes **1**. Mode 3 copies the dest byte out (serialize);
it does not load. Persist `00631C60` is mode 2.

`setne` is **not** an insn of `0043314A`. The helper’s
load arm is that `00403EB0` store.

---

## 3. Type 11 consumers test `+545`

Type 11 ctor `0054E0B0` (`FrontendWidgetType` table slot
11): `call 0055B460` then

```
0054E0BF  mov [esi],   0x1249554     ; outer
0054E0C5  mov [esi+4], 0x1249530     ; inner
```

Inner apply `0054DBC0` (`ecx` = `widget+4`):

```
0054DBEC  mov eax, [eax]             ; CUIDef* via vtbl+432
0054DBEE  mov bl, [eax+545]
          …
0054DC10  test bl, bl
          pop ebx
          je  0054DC21               ; no 0055AD60
          push [action]
          mov  ecx, esi
          call 0055AD60
```

Activate `0054DC30` (outer `ecx`):

```
0054DC4C  mov bl, [edx+545]
0054DC68  test bl, bl
          je  0054DCB2               ; no vtbl+192(3), no local 26-map
          push 3
          call [edx+192]
          add  esi, 4
          push 26 / 31 / 28 / 27 / 32 / 29
          call [inner.vtbl+12]
```

Deactivate `0054DCC0` uses the same `[edx+545]` then
`inner.vtbl+16` erase. `0054DB50` also `mov bl,[edx+545]`.

Zero `+545` **drops type-11 `0055AD60`**. Broadcast can
still enter `0054DBC0`.

---

## 4. Type 38 consumers do not

Type 38 factory ctor `00558B90`:

```
00558B98  call 0055B460              ; type-34 body (same as type 11)
00558B9D  mov [esi],   0x124B04C     ; outer
00558BA3  mov [esi+4], 0x124B024     ; inner
```

No type-11 wrapper. Apply used by that family is
**`0055AD60`**:

```
0055AD60  push esi / edi
          mov  edi, [esp+12]         ; action
          lea  eax, [edi-26]
          cmp  eax, 6
          mov  esi, ecx              ; inner
          ja   0055AE79
          jmp  [0x55AE88+eax*4]
0055AD7B  mov al, [esi+348]          ; widget+352, not +545
          test al, al
          je   0055AE3D
```

No `[…+545]` in `0055AD60`. Enable `0055AEB0`:

```
0055AEB0  call 0055BAE0
          add  esi, 4
          push 26 / 31 / 27 / 32
          call [inner.vtbl+12]
```

No `+545`. Type 38 action 26 still runs `0055AD60` when
the widget is a listener; selected `+352` is a **different**
gate.

`.rdata` dword at `0124B024+4` was not printed this pass
(`listing-01200000.txt` is `.text` tail). That pointer is
**PARTIAL**. The **bodies** type 38 uses (`0055AD60` /
`0055AEB0`) vs type 11 (`0054DBC0` / `0054DC30`) is
**PROVEN** from `.text`.

---

## Do not invent

- Lionhead name `Enabled` / `Visible` / `Clickable` for
  dest `+545`.
- NEW_GAME / ACCEPT file byte 0 or 1 (other proofs:
  **UNREAD**).
- Type 38 apply requiring `+545`.
- `+545==1` as “action 26 posts” (still `+352` / `+224` /
  `+372`).
- `setne` as an insn of `0043314A`.
