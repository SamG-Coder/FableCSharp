# `UI_FRONTEND_BUTTON_NEW_GAME` `CUIDef+545` persist CRC

Investigation only. No production `src/` edits.

Question: type-11 `UI_FRONTEND_BUTTON_NEW_GAME` persist
`CUIDef+545` CRC `0x9E47F106` u8 — **0 or 1**? Does ACCEPT
type 38 store the **same** field?

Authority: inflated `frontend.bin`
`C:\Program Files (x86)\Steam\steamapps\common\Fable The Lost Chapters\data\CompiledDefs\frontend.bin`;
writer `00631C60` / `00632233` / copy `00631C38` / size
`006314B0` in
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00600000.txt`;
`0043314A` in `implementer/frontend/fn-0043314A-exact.txt`;
type-11 apply `0054DBC0` / activate `0054DC30` / type-38
apply `0055AD60` / enable `0055AEB0` in
`listing-00540000.txt`;
`implementer/frontend/persist-scan.txt` `#625`;
`export/frontend/persist-tail.txt`;
`proofs/invisible-plus545/README.md`;
`proofs/cuidef-plus545/README.md`;
`src/Fable.Formats/Defs/FrontendUiDef.cs`;
`proofs/newgame-plus545/Dump.csx`.

Do not re-prove type 4 → `push 26`, `0x53C644E4` → `+228`
15 / `0x126`, type-10 attach `+352`, or INVISIBLE
`#625` `0x9E47F106` = **1**.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**STALE** / **LEFTOVER**.

---

## Verdict

**Same persist field on both widgets. Type 11 tests it.
Type 38 apply does not.**

`00632233` `lea edx,[esi+545]` / `0043314A` is CUIDef-wide.
Every `UI` blob, including `UI_FRONTEND_BUTTON_NEW_GAME` and
`UI_ACCEPT_NEW_PROFILE`, stores CRC **`0x9E47F106`** then one
u8 at dest **`+545`**. `00403EB0` `setne` → dest is 0 or 1.
Copy ctor `00631C38` copies that byte. Size is `0x228`.

Type **11** inner apply `0054DBC0` loads **this** def
`[eax+545]` after `vtbl+432` and **`je 0054DC21` skips
`0055AD60` when the byte is 0.** Activate `0054DC30` uses
the same byte. That is the New Game gate.

Type **38** apply **is** `0055AD60`. That body has **no**
`[…+545]`. Enable `0055AEB0` maps 26/31/27/32 with **no**
`+545` test. Accept action 26 is **not** gated here.
`+545==0` on ACCEPT does **not** stop `0055AD60`.

File payloads for those two names are **not** in
`persist-scan.txt` (Press Start tree only). Tests lock
`MessageId` / `Plus224` on them, not this u8. C#
`TryParse` does not walk `0x9E47F106`.
`ReadPersistU8(raw, 0x9E47F106)` is the lock; this pass
did not execute it against the install blob.

`cuidef-plus545` “NEW_GAME / ACCEPT file bytes **UNREAD**”
is still **UNREAD** for the **value**. The **field** (CRC,
dest, type-38 apply does not test it) is **PROVEN**.

| Claim | Status |
| --- | --- |
| File CRC `0x9E47F106` dest `CUIDef+545` u8 | **PROVEN** (`00632233`) |
| Helper `0043314A` / `00403EB0` → 0 or 1 | **PROVEN** |
| Writer is CUIDef-wide (`00631C60`), not type-11-only | **PROVEN** |
| NEW_GAME / ACCEPT therefore **have** this field | **PROVEN** |
| Type 11 `0054DBC0` / `0054DC30` test **this** `+545` | **PROVEN** |
| Gate is the parent list `+545` | **DISPROVEN** (`invisible-plus545`) |
| Type 38 apply `0055AD60` tests `+545` | **DISPROVEN** |
| Type 38 enable `0055AEB0` tests `+545` | **DISPROVEN** |
| INVISIBLE `#625` file u8 | **1** (prior proof) |
| PRESS_START / LIST / TEXT / MOUSE file u8 | **0** (prior proof) |
| NEW_GAME file `0x9E47F106` u8 | **UNREAD** (CRC locked; blob not dumped) |
| ACCEPT file `0x9E47F106` u8 | **UNREAD** (same) |
| C# parses `0x9E47F106` | **DISPROVEN** (unread) |
| Lionhead name `Enabled` / `Visible` / `Clickable` | **DISPROVEN** (`persist-tail` `?`) |

**Answer:** dest **`+545`**, CRC **`0x9E47F106`**, u8 0/1.
NEW_GAME type 11 **does** gate `0055AD60` on that byte.
ACCEPT type 38 **has the same persist field** and **does
not** consult it on apply. File 0 vs 1 on those two blobs
is **UNREAD** here (`Dump.csx`).

---

## 1. Writer `00631C60` / `00632233`

CUIDef size `006314B0` `mov eax,0x228`. Copy tail:

```
00631C38  mov dl, [edi+545]
00631C3E  mov [esi+545], dl
00631C44  mov eax, [edi+548]
          mov [esi+548], eax
```

Persist tail (`listing-00600000.txt`):

```
00632217  lea eax, [esi+544]     ; 0xCA2D971D
          call 0043314A
00632225  lea ecx, [esi+522]     ; 0xE59C9B55
          call 0043314A
00632233  lea edx, [esi+545]     ; 0x9E47F106
          push edx
          mov  ecx, edi
          call 0043314A
00632241  add esi, 0x224         ; dest = +548
          call 006326C0          ; 0xF26C87EA
```

`0043314A` file mode 2: `00404500` skips the 4-byte CRC,
`00403EB0` reads one byte, `setne` stores 0/1. CRC is
**not** a `.text` immediate. File form is **CRC + u8**.

This function is the CUIDef persist vtbl, not a type-11
special. Type 38 ctor `00558B90` still constructs a
CUIDef. ACCEPT **must** carry the same CRC.

`FrontendUiDefTests` already parse both entries
(type 11 / 15 and type 38 / `0x126`). Those tests never
scan `0x9E47F106`.

---

## 2. Type 11 New Game — zero never reaches `0055AD60`

`0054DBC0` `ecx` = inner = `widget+4`
(`listing-00540000.txt`):

```
0054DBC0  push ecx / push esi
          mov  esi, ecx
          fld  [esi+44]
          fsub [esi+400]
          fcomp [esi+392]
          fnstsw ax
          test ah, 0x05
          jnp  0054DC21              ; debounce
          mov  eax, [esi-4]
          lea  ecx, [esi-4]
          lea  edx, [esp+8]
          push edx
          call [eax+432]             ; this CUIDef*
          mov  eax, [eax]
          mov  bl, [eax+545]
          … COM-ptr release …
0054DC10  test bl, bl
          pop  ebx
          je   0054DC21              ; no 0055AD60
          push [esp+12]              ; action
          mov  ecx, esi
          call 0055AD60
0054DC21  pop esi / pop ecx / ret 4
```

`lea ecx,[esi-4]` is the type-11 **outer**. Same get-def
as `0055B040`. `action26-subscribers` “parent `+545`” is
**STALE**.

Activate `0054DC30` (`ecx` = outer):

```
0054DC4C  mov bl, [edx+545]
          test bl, bl
          je   0054DCB2              ; no vtbl+192(3), no local map
          push 3
          call [edx+192]
          add  esi, 4
          push 26 / 31 / 28 / 27 / 32 / 29
          call [inner.vtbl+12]
```

If NEW_GAME file `+545==0`, click still **enters**
`0054DBC0` (`0055CB10` → inner `vtbl+4`) and **never**
calls `0055AD60`. Local activate map of 26/28 is also
skipped. Ctor `0055BA20` already registered inner; zero
does **not** unregister.

That is why the file byte matters on New Game. It is
**not** proven 1 by “native New Game works” without the
blob. Do not invent it as INVISIBLE’s 1.

---

## 3. Type 38 ACCEPT — same field, no apply test

`0055AD60` (`ecx` = inner):

```
0055AD60  push esi / edi
          mov  edi, [esp+12]         ; action
          lea  eax, [edi-26]
          cmp  eax, 6
          mov  esi, ecx
          ja   0055AE79
          jmp  [0x55AE88+eax*4]
0055AD7B  mov  al, [esi+348]         ; widget+352
          test al, al
          je   0055AE3D              ; no vtbl+584
```

No `[…+545]` in this function. Selected `+352` still
applies (`type11-plus352-select`).

Enable `0055AEB0`:

```
0055AEB0  mov  esi, ecx
          call 0055BAE0
          add  esi, 4
          push 26 / 31 / 27 / 32
          call [inner.vtbl+12]
          ret
```

**No** `+545`. Accept action 26 is not gated on this
byte. Type 38 **does** persist the same CRC (writer
above). “Same field” = **file slot**. “Same gate” =
**DISPROVEN**.

---

## 4. File CRC lock (INVISIBLE / PRESS_START only)

Writer order after ScaleSize / ScaleOrigin on `#625`
(`invisible-plus545`):

| File CRC | Dest | Helper | `#625` INVISIBLE |
| --- | ---: | --- | ---: |
| `0xCA2D971D` | `+544` | u8 | 0 |
| `0xE59C9B55` | `+522` | u8 | 1 |
| **`0x9E47F106`** | **`+545`** | **u8** | **1** |
| `0xF26C87EA` | `+548` | i32 | 2 |

```
1D972DCA 00     ; +544 = 0
559B9CE5 01     ; +522 = 1
06F1479E 01     ; +545 = 1
EA876CF2 02000000
```

`export/frontend/persist-tail.txt` TEXT `@1067` /
MOUSE `@1035` `0x9E47F106 ? u8=0`. Parent list `#624`
is `06F1479E 00`.

`persist-scan.txt` names `UI_FRONTEND_BUTTON_NEW_GAME`
and `UI_ACCEPT_NEW_PROFILE` (CRC list only). Hex dumps
stop at the Press Start tree. `type11-msg15` locks
NEW_GAME `0x53C644E4` at raw offset **1145**; that is
`+228`, not `+545`.

| Widget | Type | Apply | File `0x9E47F106` |
| --- | ---: | --- | ---: |
| `UI_FRONTEND_BUTTON_INVISIBLE` | 11 | `0054DBC0` | **1** |
| `UI_FRONTEND_BUTTON_NEW_GAME` | 11 | `0054DBC0` | **UNREAD** |
| `UI_ACCEPT_NEW_PROFILE` | 38 | `0055AD60` | **UNREAD** |
| PRESS_START / LIST / TEXT / MOUSE | 10/12/6/32 | not `0054DBC0` | **0** |

Recipe (do not apply in `src/`):

```
FrontendUiDef.ReadPersistU8(entry.Raw, 0x9E47F106)
```

`proofs/newgame-plus545/Dump.csx` is that scan plus the
three neighbour CRCs (`+544` / `+522` / `+548`) so a
false first-hit cannot hide.

---

## 5. If 0, does action 26 never run?

**Type 11 NEW_GAME:** `0055AD60` never runs. Activate
does not insert 26/28. Broadcast still calls apply.
Click post `0055AF60` / `+380` `0055ACF0` do not happen
through this apply.

**Type 38 ACCEPT:** **no.** Apply is `0055AD60` with
`+352` / debounce-equivalent only.

Do **not** treat INVISIBLE `+545==1` as NEW_GAME’s
byte. Do **not** treat “host posts factory `MessageId`”
as a native `+545` read (C# leftover).

---

## Do not invent

- NEW_GAME / ACCEPT file u8 as 0 or 1 without
  `ReadPersistU8` on the inflated entry.
- Type 38 apply requiring `+545`.
- Parent-list `+545` as the type-11 gate.
- Lionhead name for `0x9E47F106`.
- C# `ReadPersistU8` as already wired for this CRC.
- `+545==1` as “action 26 posts 15” (still `+352` /
  `+224` / `+372` / `+380`).
