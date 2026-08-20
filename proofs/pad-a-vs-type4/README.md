# Pad A is type 19, not type 4 (LMB)

Investigation only. No production `src/` edits.

Authority: `Fable.exe` `00AB5420` / pad device ctor `00AB7850` /
poll `00AB6E40` / button ctor `00A04090` / analog ctors
`00A04030` / `00A04060` / classify `0042E3EE`;
listings `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00a80000.txt`,
`listing-00a00000.txt`, `listing-00a40000.txt`,
`listing-00400000.txt`, `listing-009c0000.txt`;
RTTI `CInputTypeXboxPadButtonEvent` /
`CInputTypeXboxPadLeftStickEvent` /
`CInputTypeXboxPadRightStickEvent` /
`CInputTypeMouseButtonEvent`;
`proofs/type4-dinput-raw/README.md`,
`proofs/type13-vs-type4/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Do not guess DIKs. Pad buttons are not keyboard keys.

---

## Verdict

**Xbox / pad A does not produce type 4.** Type 4 is LMB
down only (`00AB5420` raw 1 → `00A03C80` `[+40]=4`,
device 3). Every digital pad button down is type **19**
(`00A04090` `[+40]=0x13`, device **1**). Type **17**
(`00A04030` `[+40]=0x11`) is the left-stick analog
record, not A.

First-seen Press Start still posts `0xE5` only on
action **26**. `0042E3EE` `push 26` is the type-4
site (`0042E4A4`). Type 19 never pushes 26.

| Claim | Status |
| --- | --- |
| `00AB5420` → `00A03C80` is type 4 / LMB | **PROVEN** |
| `00AB5420` is the pad button translator | **DISPROVEN** — mouse only |
| Pad device ctor is `00AB7850` vtbl `0129EA7C` | **PROVEN** |
| Pad poll is `00AB6E40` (`GetDeviceData` `vtbl+40`) | **PROVEN** |
| Pad button **down** ctor is `00A04090` type **19** device 1 | **PROVEN** |
| Pad button **up** ctor is `00A04110` type **21** device 1 | **PROVEN** |
| Pad A / any `rgbButtons[*]` down is type 4 | **DISPROVEN** |
| Type 17 is pad A | **DISPROVEN** — left stick `00A04030` |
| Type 18 is right stick `00A04060` | **PROVEN** |
| `0042E3EE` type 19 → `push 26` | **DISPROVEN** |
| First-seen Press Start via pad A | **DISPROVEN** on this walk |
| Button id at `[record+8]` is a DIK | **DISPROVEN** — `00A03B90` byte, immediates 1–12 |

---

## 1. `00AB5420` is not the pad path

`00AB5420` (`listing-00a80000.txt`) is the mouse
translator already recovered in `type4-dinput-raw`.

```
00AB54D3  mov eax, [esi+8]
00AB54D6  lea ecx, [eax-1]
00AB54D9  cmp ecx, 23
00AB54E2  movzx edx, [ecx+0xAB56EC]
00AB54E9  jmp [0xAB56C4+edx*4]
00AB54F0  … call 00A03C80          ; raw 1 only
```

`00A03C80` writes `[+32]=3`, `[+40]=4`. Sole `.text`
caller is `00AB5500`. Raw 1 is mouse
`DIMOFS_BUTTON0` down (`00AB4910` `dwOfs=12`).

Pad poll `00AB6E40` never calls `00A03C80` /
`00AB5420`. Device 3 (mouse) and device 1 (pad) are
different ctors.

---

## 2. Pad device ctor `00AB7850`

`00A60050` (`listing-00a40000.txt`) builds the three
devices from a flags byte triple:

| Flag | Alloc | Ctor | Object |
| --- | ---: | --- | --- |
| `[ebp+1]` | `0xD44` | **`00AB7850`** | pad, vtbl `0129EA7C` |
| `[ebp+2]` | `0x3614` | `00AB64E0` | sibling device |
| `[ebp+0]` | `0x4860` | `00AB5D00` | mouse-sized sibling |

`00AB7850`:

```
call 009E43B0                      ; base
mov [esi], 0x129EA7C
mov [esi+3392], ebx                ; owner
mov [esi+3372], 0xAAAA
mov [esi+3380], 0xAAAA
mov [esi+3368], 0x5556
mov [esi+3376], 0x5556
call 00AB6DE0                      ; acquire
```

`00AB6DE0` pushes callback `00AB6B10`,
`SetDataFormat` `0x12AE20C`, then
`IDirectInputDevice8::Acquire`. Stored device is
`[this+3384]`. Dtor `00AB7950` / copy-dtor
`00AB6A90` write the same vtbl.

RTTI names on the events this object enqueues:
`CInputTypeXboxPadButtonEvent` /
`LeftStickEvent` / `RightStickEvent`.

---

## 3. Poll `00AB6E40` — buttons vs analog

`00AB6E40` (`GetDeviceData` `vtbl+40`,
`DIERR_INPUTLOST` `0x8007001E` → `Acquire` `vtbl+28`).
20-byte `DIDEVICEOBJECTDATA`. `dwOfs` in `[esp+24]`,
`dwData` in `[esp+28]`, timestamp in `[esp+32]`.

```
cmp eax, 59                        ; dwOfs 0..59
movzx eax, [eax+0xAB780C]
jmp [0xAB77C4+eax*4]
```

`59` is `DIJOYSTATE` `rgbButtons[11]`
(`rgbButtons[0]` = ofs **48**). Not a `DIK_*` range
(those live on the keyboard device, type 1).

Jump table `0xAB77C4` (16 dwords; listing treats them
as code). First dword is `6B 6F AB 00` = `00AB6F6B`.

| i | Target | Role |
| ---: | --- | --- |
| 0 | `00AB6F6B` | axis → `[this+3352]` |
| 1 | `00AB6F88` | axis → `[this+3356]` |
| 2 | `00AB6FA5` | axis → `[this+3360]` |
| 3 | `00AB6FC2` | axis → `[this+3364]` |
| 4 | `00AB6FDF` | digital, `push 1` |
| 5 | `00AB7070` | digital, `push 2` |
| 6 | `00AB7103` | digital, `push 3` |
| 7 | `00AB7196` | digital, `push 4` |
| 8 | `00AB7229` | digital, `push 5` |
| 9 | `00AB72BC` | digital, `push 6` |
| 10 | `00AB7346` | digital, `push 7` |
| 11 | `00AB73D9` | digital, `push 8` |
| 12 | `00AB745D` | digital, `push 9` |
| 13 | `00AB74F0` | digital, `push 10` |
| 14 | `00AB7583` | digital, `push 11` |
| 15 | `00AB7616` | digital, `push 12` |

Each digital case:

```
mov al, [esp+28]                   ; dwData
test bl, al                        ; bl = 0x80
je  …up
…                                  ; pair = +3352/+3356
push 0
push <id>                          ; 1..12, not a DIK
lea ecx, [esp+72]
call 00A04090                      ; type 19
jmp 00AB76A1
…up:
push 0
push <id>
call 00A04110                      ; type 21
```

Then `009E41E0` copies the 52-byte record into a
64-slot ring at `this+4`. `009E4470` on type 19
also builds a held type-**20** twin (`00A040D0`)
on list `this+3340`. Type 21 unlinks the matching
type-20 by `00A03B90` id.

After the buffer drains, **every** poll emits:

```
00A04030    ; type 17, pair +3352/+3356  (left stick)
00A04060    ; type 18, pair +3360/+3364  (right stick)
```

That is `CInputTypeXboxPadLeftStickEvent` /
`RightStickEvent`. Not a face button.

### Button ctor `00A04090` (`listing-00a00000.txt`)

```
mov al, [esp+4]
mov [ecx+8], al                    ; button id 1..12
mov [ecx+40], 0x13                 ; type 19
mov [ecx+32], 0x1                  ; device 1
; [ecx+0]/[+4] from pair ptr (current stick)
; [ecx+12] = the `push 0`
ret 20
```

Siblings:

| Ctor | `[+40]` | `[+32]` | RTTI |
| --- | ---: | ---: | --- |
| `00A03C80` | **4** | 3 | `CInputTypeMouseButtonEvent` |
| `00A04030` | **17** (`0x11`) | 1 | left stick |
| `00A04060` | 18 (`0x12`) | 1 | right stick |
| **`00A04090`** | **19** (`0x13`) | **1** | **`CInputTypeXboxPadButtonEvent`** |
| `00A040D0` | 20 (`0x14`) | 1 | held twin |
| `00A04110` | 21 (`0x15`) | 1 | pad button up |

`00A03C80` is never in this table.

### First face button (Xbox A)

`rgbButtons[0]` is `dwOfs` **48**. Index table
`0xAB780C+48` = `00AB783C`. Listing data there is
`05 06 07 08 09 0A 0B 0C 0D 0E 0F …` (same
reconstruction style as `0xAB56EC` in
`type4-dinput-raw`).

Byte **05** → jt[5] `00AB7070` → `push 2` →
`00A04090` id **2**, type **19**.

That id is a pad-button ordinal at `[record+8]`.
It is **not** `DIK_A` (30). Keyboard A remains
type 1 / `00A03B70` compare 30 / action 33.

Which face glyph the OEM maps to `rgbButtons[0]`
is the DINPUT Xbox layout (A is the first digital
button on that format). The game never writes a
`"A"` / `DIK_*` immediate on this path.

---

## 4. `0042E3EE` type 17 vs type 19 vs type 4

`00A03B40` then (`listing-00400000.txt`):

```
cmp eax, 17
jg  0042E67E                       ; types > 17
je  0042E608                       ; type 17 analog
…
sub eax, 3
je  0042E4A4                       ; type 4 → push 26
```

### Type 4 (`0042E4A4`)

`push 26` → `0055CB10(26)`. Press Start type-10
posts `widget+352` (`0xE5`). No `[+8]` / DIK
compare.

### Type 17 (`0042E608`)

```
cmp [edi+40], 0
je  skip
call 00A043A0                      ; stick pair
; threshold vs 0x1230B98 / 0x1230C8C
or [ebp-4], 2 / 1 / 8 / 4          ; NESW bits
```

No `0055CB10`. Same movement bits as type-1
arrow/WASD-slot compares. **Not** accept.

### Type 19 (`0042E67E` `sub eax, 19` → `0042E72E`)

```
; player-slot filter 0049BB90 / 0049BB50 / 0049BB40
; vs 00A03B60 ([record+36])
cmp [edi+40], 0
je  skip
call 00A03B90                      ; [record+8] id
```

`00A03B90` is `movzx eax, [ecx+8]`. Switch on that
id (not `00A03B70` / `[+0]`):

| `[+8]` | Flag | Later `0055CB10` |
| ---: | --- | ---: |
| 1 | `or [ebp-2], 1` → `0x10000` | **23** |
| 2 | `or [ebp-2], 2` → `0x20000` | **22** |
| 4 | `or [ebp-3], 1` | 4 |
| 5 | `or [ebp-3], 2` | 5 |
| 11 | `or [ebp-3], 4` | 4 |
| 12 | `or [ebp-3], 8` | 5 |
| 13–16 | `or [edi+252], 16/32/64/128` | 8–11 |

Ids 3, 6–10 are no-ops in this switch. None of
these sites `push 26`.

So pad A (`rgbButtons[0]` → id **2** on the
reconstructed table) is type 19 → bit `0x20000` →
action **22**, not action 26.

Type 21 (`0042E6DA`) only **clears** `edi+252`
bits for ids 13–16. No accept.

---

## 5. First-seen Press Start on pad

Frontend first-seen accept is already **PROVEN** as:

```
0042E3EE type 4 → push 26
  → 0055CB10(26)
  → type-10 0054E280 case 0
  → 0059A238(&widget+352) = 0xE5
```

Pad A never enters that site:

- different event type (19, not 4)
- different device (1, not 3)
- different ctor (`00A04090`, not `00A03C80`)
- classify writes a mask bit / action 22, not 26

Held analog type 17 also does not post `0xE5`.

Do not map pad A / Start / Enter onto type 4.
Do not invent a DIK for pad A.

---

## 6. Host leftover

`FrontendInputMap.ActionFromEvent` returns 26 only
for type 4 and `null` for type 17. Type 19 is
absent (**LEFTOVER** vs dump). Live `Program.cs`
still does not queue type 4; pad is unread on the
host. This proof does not change `src/`.
