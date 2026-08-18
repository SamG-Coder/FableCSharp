# Type-10 ctor never calls `0055B040`

Investigation only. No production `src/` edits.

Authority: `Fable.exe` listings
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
(`0041D21B` type switch `0x41D7F8`),
`listing-00500000.txt` (`0052CC50` / `005334A0` / `005331A0`),
`listing-00540000.txt` (`0054E3D0` / `0054E410` / `0054E4F0` /
`0055B040` / `0055B460`),
`listing-00580000.txt` (`00598EE6` / `0059A6BE`),
`listing-00600000.txt` (`00631C60` `lea [esi+224]` / `+228`);
inflated `frontend.bin` `implementer/frontend/persist-scan.txt`
(`UI_FRONTEND_PRESS_START_MENU` `#620`);
`proofs/press-start-action-e5/README.md`;
`proofs/press-start-e5-attach/README.md`;
`proofs/type10-plus352/README.md`;
`proofs/messageid-plus228/README.md`;
`implementer/frontend/01-widget-construction.md`.

Do not re-prove type 4 → action 26, Return ≠ `0xE5`,
`0059A238` consume (`0xE5` → `00599D5C`), or type-10 +352
layout (`type10-plus352`).

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER** / **MATCH**.

---

## Verdict

**Type-10 ctor does not copy persist `+224` / `+228` onto the widget.**
Factory type 10 is `0041D4FC` alloc `0x16C` → **`0054E3D0`**, not
`0054C050` / `0054C1D0` / `0054C3A0` (those are types 15 / 14 / 12).

`0054E3D0` is 14 instructions: `0052CC50` (type 5), three vtbl
stores, **zero `+352/+356/+360`**, `ret 4`. **No** `call 0055B040`.
Copy-ctor `0054E410` is the same shape via `0052CCA0`.

`0055B040` has **two** `.text` callers: `0055B4B5` and `0055B515`
(type **34** ctor / sister). Type 11/38 reach it only through
`0055B460`. Type 10 never does.

Press Start `0xE5` is **only** attach `00598EE6` `mov [eax],0xE5`
then slot `0x14` `vtbl+284` **`0054E4F0`** → **widget+352**
(packet*). That is the sole `.text` `mov […], 0xE5`. Persist
`+224` / `+228` on the type-10 def are **0** and would not land
on the widget even if they were nonzero.

| Claim | Status |
| --- | --- |
| Type 10 factory ctor is `0054E3D0` (size `0x16C`) | **PROVEN** `0041D512` |
| `0054C050` / `0054C1D0` / `0054C3A0` are type 10 | **DISPROVEN** (15 / 14 / 12) |
| `0054E3D0` / `0054E410` call `0055B040` | **DISPROVEN** |
| `0055B040` callers are type-34 `0055B460` / `0055B4C0` only | **PROVEN** listing |
| Type 10 inherits `0052CC50` → `005334A0` → `005331A0` | **PROVEN** |
| `005331A0` copies persist **`+212`** (`vtbl+520`), not `+224`/`+228` | **PROVEN** |
| File→def `00631C60` still stores CUIDef `+224` then `+228` for every UI def | **PROVEN** (def only) |
| Type-10 **widget** `+224`/`+228` are persist message copies | **DISPROVEN** (`005334A0` list sentinel / zero) |
| PRESS_START file `0x230364D6` (`+224`) and `0x53C644E4` (`+228`) payloads | **PROVEN** both **0** |
| Same type-10 `Action` `0xF1A22807` | **PROVEN** 0 (`@1335`) |
| Type-10 persist already holds `0xE5` | **DISPROVEN** |
| Press Start `0xE5` is `00598EE6` → `0054E4F0` → widget+352 | **PROVEN** |
| Another `.text` `mov […], 0xE5` | **DISPROVEN** (only `00598EE6`) |
| NEW_PROFILE / MAIN_MENU type-10 persist `+224`/`+228` copied onto those roots | **DISPROVEN** (same ctor; persist 0; no attach write) |

---

## 1. Factory type 10 is `0054E3D0`, not `0054C??`

`0041D21B` `Type=[def+60]`, `jmp [0x41D7F8+type*4]`
(`01-widget-construction.md`):

```
0041D4FC  push 0x16C
0041D501  call 00BFEA1A
0041D510  mov ecx, eax
0041D512  call 0054E3D0          ; type 10
0041D51C  push 0x1B4
0041D532  call 0054E0B0          ; type 11
0041D53C  push 0x1FC
0041D552  call 0054C3A0          ; type 12
0041D57C  push 0x190
0041D592  call 0054C1D0          ; type 14
0041D59C  push 0x1EC
0041D5B2  call 0054C050          ; type 15
```

`0054E4F0` is **not** a ctor. It is type-10 widget `vtbl+284`
(`012497E4+284`), the attach packet store.

---

## 2. Type-10 ctor body (entire function)

`listing-00540000.txt`:

```
0054E3D0  mov eax, [esp+4]       ; def
0054E3D4  push esi
0054E3D5  push eax
0054E3D6  mov esi, ecx
0054E3D8  call 0052CC50          ; type 5 group
0054E3DD  xor eax, eax
0054E3DF  mov [esi], 0x12497E4   ; widget vtbl
0054E3E5  mov [esi+4], 0x12497BC ; inner
0054E3EC  mov [esi+24], 0x12497B4
0054E3F3  mov [esi+352], eax     ; packet* = 0
0054E3F9  mov [esi+356], eax
0054E3FF  mov [esi+360], eax
0054E405  mov eax, esi
0054E407  pop esi
0054E408  ret 4
```

Copy-ctor `0054E410` calls `0052CCA0` then the same three zeros.
Dtor `0054E450` clears `+352/+356` and jumps `0052CCF0`. None of
these three call `0055B040`.

Type-5 `0052CC50` is equally short: `005334A0`, vtbl `01245DE4`,
alloc list at `+316`. No persist-message copy.

---

## 3. Who actually calls `0055B040`

```
0055B040  sub esp, 16
          mov ebx, ecx
          call [vtbl+432]         ; def*
          [ebx+396] = [def+388]
          ecx = [def+224]
          test ecx, ecx
          je 0055B15A             ; skip if 0
          0042BE50 / 0042AA29
          [heap] = [def+224]
          call [this.vtbl+284]
          then [def+228] → vtbl+320
               [def+232] → vtbl+288
               [def+236] → vtbl+292
```

`.text` `call 0055B040`:

| Site | Role |
| --- | --- |
| `0055B4B5` in `0055B460` | Type **34** ctor (`0124BD2C`) after `0055BA20` |
| `0055B515` in `0055B4C0` | Sister / copy-ctor |

Type **11** `0054E0B0` and type **38** `00558B90` `call 0055B460`
then overwrite vtbl. That is the persist copy for `0x126` / 15
(`type38-msg126`, `type11-msg15`). Type 10 is not on that path.

---

## 4. What type 10 *does* copy from the def

`0052CC50` → type-4 `005334A0` ends with `005331A0` (child walk).
`005331A0` (`listing-00500000.txt`):

```
005331B6  call [eax+432]         ; def*
005331C0  mov eax, [ecx+212]     ; persist +212
          test / packet / 0042AA29
00533208  call [edx+520]         ; not vtbl+284
          then Type u8, flags, styles, Children
```

So the inherited ctor copies **`def+212`**, styles, and children.
It does **not** read `def+224` or `def+228`.

Type-4 object slots `widget+224` / `+228` exist and are **not**
those persist dwords:

```
0053361D  mov [esi+224], ebx     ; 0
          alloc 16-byte list node
0053362D  mov [esi+224], eax     ; sentinel*
00533633  mov [esi+228], ebx     ; 0
```

A later `00532D90` walk of `[esi+224]` is that list, not a
message id. Do not fold widget `+224` into CUIDef `+224`.

File→def still happens for every CUIDef, type-agnostic:

```
00631FBD  lea edx, [esi+224]
00631FC6  call 00632500
00631FCB  lea eax, [esi+228]
00631FD4  call 00632500
```

CRC map (`messageid-plus228`, tests): `+224` = `0x230364D6`,
`+228` = `0x53C644E4` (host `MessageIdCrc`). Older notes that
put `0x53C644E4` at `+224` are **STALE**. Those def dwords stay
on the type-10 **def**. Nothing in the type-10 ctor copies them
onto the widget.

---

## 5. Press Start `0xE5` is attach only

`persist-scan.txt` `#620` `UI_FRONTEND_PRESS_START_MENU` Type=10
hex (CRC then i32):

```
0728A2F1 00000000     ; Action 0xF1A22807 = 0   (@1335)
D6640323 00000000     ; +224 0x230364D6 = 0
E444C653 00000000     ; +228 0x53C644E4 = 0
```

`0055B040` would `je` skip even on type 11/38. Type 10 never
reaches it.

The only `.text` store of immediate `0xE5` is `00598A1C` after
the other named slots:

```
00598EC3  push 16
00598EC5  call 00BFEA1A
00598ED1  call 0042BE50
00598EDE  call 0042AA29
00598EE3  mov eax, [ebp-56]
00598EE6  mov [eax], 0xE5        ; packet[0] = 0xE5
00598EF2  mov [ebp+108], 0x14
00598EF9  call 0059B5D7          ; slot 0x14
00598F06  call [eax+284]         ; type-10 0054E4F0
```

```
0054E4F0  mov ebx, [eax]         ; packet*
          mov edi, [eax+4]
0054E530  mov [esi+352], ebx
0054E536  mov [esi+356], edi
```

Ctor left `+352 = 0`. Without this attach, action 26
(`0054E2FA` `test eax,eax`) is a no-op (`type10-plus352`).

NEW_PROFILE / MAIN_MENU type-10 roots: same ctor, persist
`+224/+228 = 0`, **not** written by `00598EE6`
(`press-start-e5-attach`, `action26-subscribers`). Their
`+352` stays 0.

Host `AttachFrontendTree` (`PRESS_START && MessageId==0` →
`0xE5`) remains the C# analog of `00598EE6`, collapsed onto
`FrontendWidget.MessageId`. It is **not** filling a missing
type-10 persist copy. Keep it (`press-start-action-e5`).

---

## Do not invent

- Type-10 ctor `0054C3A0` / `0054C050`.
- Type-10 `call 0055B040`.
- Persist `0xE5` on `UI_FRONTEND_PRESS_START_MENU` `+224` or `+228`.
- Widget `+224`/`+228` as the type-10 message slots (those are
  type-4 list fields; the message is packet* at **+352**).
- A second `.text` `mov […], 0xE5`.
- Dropping the PRESS_START attach analog because INVISIBLE
  type-11 persist is 229 (`press-start-action-e5`).
