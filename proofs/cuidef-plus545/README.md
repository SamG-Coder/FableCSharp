# Type 11 `0054DBC0` requires `[CUIDef+545]` before `0055AD60`

Investigation only. No production `src/` edits.

Authority: dump `Fable.exe` `0054DBC0` / `0054DC30` /
`0055AD60` / `0055AEB0` in
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`;
`00631C60` tail / copy `00631C38` / size `006314B0` in
`listing-00600000.txt`; `0043314A` / `00403EB0` in
`listing-00400000.txt` (`implementer/frontend/fn-0043314A-exact.txt`);
inflated `frontend.bin`
`implementer/frontend/persist-scan.txt` `#625`;
`export/frontend/persist-tail.txt`;
`proofs/invisible-button-e5/README.md` (`+545` first-seen was
**UNREAD**; “parent `+545`” **STALE**);
`proofs/action26-subscribers/README.md`;
`proofs/type11-plus352-select/README.md`;
`proofs/persist-flag-names/README.md`;
`src/Fable.Formats/Defs/FrontendUiDef.cs`.

Do not re-prove type 4 → `push 26`, Return ≠ `0xE5` /
`0x126` / 15, type-10 attach `+352`, or
`0x53C644E4` → `+228`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**STALE** / **LEFTOVER**.

---

## Verdict

**`[CUIDef+545]` is this widget’s persist `u8`, not the
parent list.** Type 11 inner apply `0054DBC0` loads it via
outer `vtbl+432` (same get-def as `0055B040`) and
**skips `0055AD60` when the byte is 0.** Action 26 is still
*delivered* to `0054DBC0`; the 26–32 table never runs.

First-seen **Press Start** type-11
`UI_FRONTEND_BUTTON_INVISIBLE` stores **1**. Zero would drop
26; that blob does not. Remaining click gates are debounce
(`[inner+44]` vs `+400` / `+392`) and selected `widget+352`
(`type11-plus352-select`).

Type 38 `UI_ACCEPT_NEW_PROFILE` apply **is** `0055AD60`.
It does **not** read `+545`. Type 38 enable `0055AEB0`
does not either. `+545==0` does **not** stop Accept action
26.

Persist CRC is **`0x9E47F106`**. Writer `00632233`
`0043314A`. Lionhead name **UNREAD**. Not `Enabled` /
`Visible` / `Clip` (`persist-tail` labels that CRC `?`
while those English hashes are in `KnownCrcNames`).

| Claim | Status |
| --- | --- |
| `0054DBC0` `ecx` is inner = `widget+4` | **PROVEN** |
| `vtbl+432` returns **this** widget’s `CUIDef*` | **PROVEN** |
| `mov bl,[eax+545]` / `test bl` / `je 0054DC21` skips `0055AD60` | **PROVEN** |
| That field is the **parent** list def | **DISPROVEN** (`action26-subscribers` / `action27-release` **STALE**) |
| `+545` is persist `u8` (`00631C60` `0043314A`) | **PROVEN** |
| File CRC is **`0x9E47F106`** | **PROVEN** (writer order + `#625` / PRESS_START hex) |
| `00403EB0` `setne` → dest is 0 or 1 | **PROVEN** |
| `CUIDef` size `0x228`; copy ctor copies `+545` | **PROVEN** |
| Runtime writer of CUIDef `+545` besides persist / copy | **UNREAD** (no CUIDef store in this listing) |
| First-seen `UI_FRONTEND_BUTTON_INVISIBLE` `+545` | **1** |
| First-seen `UI_FRONTEND_BUTTON_NEW_GAME` `+545` | **UNREAD** (CRC locked; blob not in `persist-scan` hex) |
| First-seen `UI_ACCEPT_NEW_PROFILE` `+545` | **UNREAD** (same) |
| Type 38 apply tests `+545` | **DISPROVEN** |
| If type-11 `+545==0`, `0055AD60` action 26 never runs | **PROVEN** |
| If type-11 `+545==0`, `0054DBC0` is not entered | **DISPROVEN** (broadcast still calls apply) |
| C# parses `0x9E47F106` | **DISPROVEN** (unread) |

**Answer:** gate is **this** def `+545`. INVISIBLE file
**1**. NEW_GAME / ACCEPT file bytes **UNREAD** here. Zero
drops type-11 `0055AD60` only.

---

## 1. Dump `0054DBC0` (type 11 inner apply)

`esi = ecx` = inner. Debounce, then get-def, then `+545`,
then `0055AD60`:

```
0054DBC0  push ecx
          push esi
          mov  esi, ecx              ; inner = widget+4
          fld  [esi+44]
          fsub [esi+400]
          fcomp [esi+392]
          fnstsw ax
          test ah, 0x05
          jnp  0054DC21              ; drop (no 0055AD60)
          mov  eax, [esi-4]          ; outer vtbl
          lea  ecx, [esi-4]
          lea  edx, [esp+8]
          push edx
          call [eax+432]             ; out CUIDef**
          mov  eax, [eax]
          mov  bl, [eax+545]         ; this def, not parent
          … COM-ptr release …
0054DC10  test bl, bl
          pop  ebx
          je   0054DC21              ; drop
          mov  eax, [esp+12]         ; action
          push eax
          mov  ecx, esi              ; inner
          call 0055AD60
0054DC21  pop  esi
          pop  ecx
          ret  4
```

`invisible-button-e5` already dumped this shape. “Parent
`+545`” is **STALE**: `lea ecx,[esi-4]` is the type-11
outer. `0055B040` uses the same `vtbl+432`.

If `+545==0`, **`0055AD60` is not called.** Actions 26–32
on that table (arm / `vtbl+584` / `+364=1`) do not run.
`0055CB10` still invoked inner `vtbl+4`.

Debounce `jnp 0054DC21` is a **separate** skip. First-seen
INVISIBLE `+545==1` does not by itself prove 26 enters
`0055AD60` (`+352` still 0 from ctor until `0055BF10`).

---

## 2. Activate `0054DC30` uses the same byte

Type 11 activate (`ecx` = outer):

```
0054DC30  mov  esi, ecx
          call [eax+432]
          mov  bl, [edx+545]
          test bl, bl
          je   0054DCB2              ; no vtbl+192(3), no local map
          push 3
          call [edx+192]
          add  esi, 4
          push 26 / 31 / 28 / 27 / 32 / 29
          call [inner.vtbl+12]       ; 0052DA20 local map, not 0055CB10
```

Ctor already registered inner on the input list
(`0055BA20`). Activate’s 26-set is extra. Zero `+545`
skips that map **and** apply’s `0055AD60`.

Type 8/12 activate `0053D540` is the same
`mov bl,[edx+545]` after `vtbl+432` (`listing-00500000.txt`).
Still **this** def.

Type 38 enable `0055AEB0` (`00558B90` family):

```
0055AEB0  call 0055BAE0
          add  esi, 4
          push 26 / 31 / 27 / 32
          call [inner.vtbl+12]
```

**No** `+545`. Accept action 26 is not gated here.

---

## 3. Type 38 apply is `0055AD60` (no `+545`)

```
0055AD60  mov  edi, [esp+12]         ; action
          lea  eax, [edi-26]
          cmp  eax, 6
          mov  esi, ecx              ; inner
          ja   0055AE79
          jmp  [0x55AE88+eax*4]
0055AD7B  mov  al, [esi+348]         ; widget+352
          test al, al
          je   0055AE3D              ; no vtbl+584
          …
```

No `[…+545]` in this function. ACCEPT first-seen apply
does not consult CUIDef `+545`. Selected `+352` still
applies (`type11-plus352-select`).

---

## 4. Persist writer `00631C60` tail

CUIDef size getter `006314B0` `mov eax,0x228`. Copy ends:

```
00631C2C  mov cl, [edi+544]
          mov [esi+544], cl
00631C38  mov dl, [edi+545]
00631C3E  mov [esi+545], dl
00631C44  mov eax, [edi+548]
          mov [esi+548], eax
```

`+545` is a byte. `+546/+547` padding. Persist:

```
00632217  lea eax, [esi+544]
          call 0043314A
00632225  lea ecx, [esi+522]
          call 0043314A
00632233  lea edx, [esi+545]
          call 0043314A
00632241  add esi, 0x224             ; dest = CUIDef+548
          call 006326C0              ; CRC skip + i32
```

`0043314A` file mode 2: `00404500` skips the 4-byte field
CRC, `00403EB0` reads one byte, `setne` stores 0/1. CRC is
**not** a `.text` immediate.

C# does not walk this slot (`FrontendUiDef.TryParse` stops
short). `ReadPersistU8(0x9E47F106)` would match the helper.

---

## 5. File CRC lock `0x9E47F106`

Writer order after ScaleSize / ScaleOrigin
(`0xC50CA371` / `0xB466D948`) on every dumped Press Start
blob (`persist-scan` hex tails):

| File CRC | Writer dest | Helper | INVISIBLE |
| --- | ---: | --- | ---: |
| `0xC50CA371` | `+520` | u8 | 0 |
| `0xB466D948` | `+521` | u8 | 0 |
| `0x180E20C5` | `+516` | i32 | 3 |
| `0xC08267F2` | `+524` | i32 | 3 |
| `0x50D249C6` | `+528` | i32 | 3 |
| `0xD63A4547` | `+532` | vec n | 0 |
| `0x298F8140` | `+332` | i32 | 0 |
| `0x5E88B1D6` | `+336` | i32 | 0 |
| `0xE565615D` | `+340` | i32 | 640 |
| `0x926251CB` | `+344` | i32 | 480 |
| `0xCA2D971D` | `+544` | u8 | 0 |
| `0xE59C9B55` | `+522` | u8 | 1 |
| **`0x9E47F106`** | **`+545`** | **u8** | **1** |
| `0xF26C87EA` | `+548` | i32 | 2 |

`#625` hex end:

```
1D972DCA 00     ; +544 = 0
559B9CE5 01     ; +522 = 1
06F1479E 01     ; +545 = 1
EA876CF2 02000000
```

Same four CRCs on PRESS_START type 10, LIST type 12, TITLE,
LEGAL, TEXT, MOUSE, FOREST tiles: **`+545 = 0`**
(`06F1479E00`). `export/frontend/persist-tail.txt`
`UI_PRESS_START_TEXT` `@1067 0x9E47F106 ? u8=0`.

Lionhead name **UNREAD**. `persist-tail` `KnownCrcNames`
includes `Enabled` / `Visible` / `Clip` / `Absolute` /
`Centre` / `ScaleSize` / `Action` and still prints `?` for
`0x9E47F106` → those English hashes are **DISPROVEN**.

---

## 6. First-seen INVISIBLE / NEW_GAME / ACCEPT

| Widget | Type | Apply | File `0x9E47F106` |
| --- | ---: | --- | ---: |
| `UI_FRONTEND_BUTTON_INVISIBLE` | 11 | `0054DBC0` | **1** (`#625`) |
| `UI_FRONTEND_BUTTON_NEW_GAME` | 11 | `0054DBC0` | **UNREAD** |
| `UI_ACCEPT_NEW_PROFILE` | 38 | `0055AD60` | **UNREAD** |

NEW_GAME / ACCEPT inflated hex is **not** in
`persist-scan.txt` (only the Press Start tree). Tests lock
`MessageId` / `Plus224` on those two, not this u8. Same
`00631C60` runs for every `UI` def, so the CRC **is** in
those blobs; the **value** is not recovered this pass.

Recipe (do not apply here):
`FrontendUiDef.ReadPersistU8(entry.Raw, 0x9E47F106)`.

INVISIBLE `+545==1` means first-seen Press Start type 11
**does not** drop 26 at this gate. `invisible-button-e5`
“first-seen INVISIBLE `+545` **UNREAD**” is **STALE**.
Zero on TITLE / LIST / type-10 root is expected: those
applies are not `0054DBC0`.

---

## 7. If 0, does action 26 never run?

**Type 11:** `0055AD60` never runs. Local activate map
never inserts 26. The widget can still be a `0055CB10`
listener. Click post `0055AF60` / `widget+372` does not
happen through this apply.

**Type 38:** **no.** Accept 26 is `0055AD60` with
`+352` / debounce-equivalent only.

**INVISIBLE first-seen:** file is **1**, so this gate is
**open**. Native 26 still needs `+352≠0` to take
`vtbl+584`. Ctor left `+352=0` (`0055BA20`). That is a
different proof.

C# leftover: host action 26 posts factory `MessageId`
without reading `+545`. Native type 11 would drop if the
byte were 0.

---

## Do not invent

- Parent-list `+545` as the type-11 gate.
- Lionhead name `Enabled` / `Visible` / `Clickable` for
  `0x9E47F106`.
- NEW_GAME / ACCEPT file bytes without a dump.
- Type 38 apply requiring `+545`.
- `+545==1` as “action 26 posts” (still `+352` /
  `+224` / `+372`).
- C# `ReadPersistU8` as already wired for this CRC.
