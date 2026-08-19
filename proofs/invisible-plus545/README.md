# `UI_FRONTEND_BUTTON_INVISIBLE` `CUIDef+545` persist CRC

Investigation only. No production `src/` edits.

Authority: inflated `frontend.bin`
`implementer/frontend/persist-scan.txt` `#625`;
dump `Fable.exe` `0054DBC0` / `0054DC30` /
`0055AD60` in
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`;
writer `00631C60` `00632233` / copy `00631C38` in
`listing-00600000.txt`;
`proofs/cuidef-plus545/README.md` (CRC lock + type-11 gate);
`proofs/invisible-button-e5/README.md` (`+545` first-seen was
**UNREAD**; “parent `+545`” **STALE**);
`proofs/persist-flag-names/README.md`;
`proofs/type11-plus352-select/README.md`;
`src/Fable.Formats/Defs/FrontendUiDef.cs`.

Do not re-prove type 4 → `push 26`, Return ≠ `0xE5`,
type-10 attach `+352`, or `0x53C644E4` → `+228`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**STALE** / **LEFTOVER**.

---

## Verdict

**If `[CUIDef+545]==0`, type-11 inner `0054DBC0` never
`call 0055AD60`.** Action 26 is still delivered to apply
(`0055CB10` → inner `vtbl+4`). Debounce may also skip. The
26–32 table does not run.

That byte is **this** widget’s persist `u8`, not the parent
list. File CRC is **`0x9E47F106`**. Writer
`00632233` `0043314A`. Lionhead name **UNREAD**.

First-seen Press Start child
`UI_FRONTEND_BUTTON_INVISIBLE` `#625` stores **1**
(`06F1479E 01`). Parent list `#624` stores **0**. Zero on
the list would drop a type-12 forward, **not** this
type-11 apply. Treating INVISIBLE as `+545==0` because
the list is 0 is **STALE**.

This gate is **open**. Remaining first-seen skips are
debounce (`[inner+44]` vs `+400` / `+392`) and selected
`widget+352` (ctor 0). Open `+545` does **not** make
action 26 post `0xE5` (`invisible-button-e5`: click posts
empty `+372`).

| Claim | Status |
| --- | --- |
| `0054DBC0` `ecx` is inner = `widget+4` | **PROVEN** |
| `mov bl,[eax+545]` is **this** def (`vtbl+432`) | **PROVEN** |
| `test bl` / `je 0054DC21` skips `0055AD60` | **PROVEN** |
| Gate is parent list `+545` | **DISPROVEN** |
| Persist helper is `0043314A` u8 (`00632233`) | **PROVEN** |
| File CRC is **`0x9E47F106`** | **PROVEN** (`cuidef-plus545` + `#625` hex) |
| `#625` `0x9E47F106` = **1** | **PROVEN** |
| Parent `#624` `0x9E47F106` = **0** | **PROVEN** |
| PRESS_START type 10 / TEXT / LEGAL / MOUSE = **0** | **PROVEN** |
| First-seen INVISIBLE `+545` **UNREAD** | **STALE** (`invisible-button-e5`) |
| `+545==0` → `0054DBC0` is not entered | **DISPROVEN** (broadcast still calls apply) |
| `+545==1` → first-seen 26 posts `0xE5` | **DISPROVEN** (`+352` / `+224` / `+372`) |
| C# parses `0x9E47F106` | **DISPROVEN** (unread) |
| Lionhead name `Enabled` / `Visible` / `Clickable` | **DISPROVEN** (`persist-tail` still `?`) |

**Answer:** persist CRC **`0x9E47F106`**, dest **`+545`**,
INVISIBLE file **1**. Zero would skip `0055AD60`; this blob
does not.

---

## 1. Dump `0054DBC0` — zero never reaches `0055AD60`

```
0054DBC0  push ecx
          push esi
          mov  esi, ecx              ; inner = widget+4
          fld  [esi+44]
          fsub [esi+400]
          fcomp [esi+392]
          fnstsw ax
          test ah, 0x05
          jnp  0054DC21              ; debounce drop
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
          mov  eax, [esp+12]         ; action
          push eax
          mov  ecx, esi
          call 0055AD60
0054DC21  pop esi / pop ecx / ret 4
```

`lea ecx,[esi-4]` is the type-11 outer. Same get-def as
`0055B040`. `action26-subscribers` “parent `+545`” is
**STALE**.

Activate `0054DC30` uses the same byte: `je 0054DCB2`
skips `vtbl+192(3)` and the local 26/31/28/27/32/29 map.
Ctor already registered inner (`0055BA20`). Zero `+545`
does **not** unregister.

---

## 2. Persist CRC lock `0x9E47F106`

`00631C60` last three `0043314A` stores, then `+548`:

```
00632217  lea eax, [esi+544]     ; 0xCA2D971D
00632225  lea ecx, [esi+522]     ; 0xE59C9B55
00632233  lea edx, [esi+545]     ; 0x9E47F106
          call 0043314A
00632241  add esi, 0x224
          call 006326C0          ; 0xF26C87EA
```

Copy ctor `00631C38` `mov dl,[edi+545]` / `mov [esi+545],dl`.
CUIDef size `0x228`. File mode 2: skip CRC (`00404500`),
one byte, `00403EB0` `setne` → dest 0 or 1.

Writer order after ScaleSize / ScaleOrigin on `#625`
(`persist-scan` hex tail):

| File CRC | Dest | `#625` |
| --- | ---: | ---: |
| `0xC50CA371` | `+520` | 0 |
| `0xB466D948` | `+521` | 0 |
| `0x180E20C5` | `+516` | 3 |
| `0xC08267F2` | `+524` | 3 |
| `0x50D249C6` | `+528` | 3 |
| `0xD63A4547` | `+532` | n=0 |
| `0x298F8140` | `+332` | 0 |
| `0x5E88B1D6` | `+336` | 0 |
| `0xE565615D` | `+340` | 640 |
| `0x926251CB` | `+344` | 480 |
| `0xCA2D971D` | `+544` | 0 |
| `0xE59C9B55` | `+522` | 1 |
| **`0x9E47F106`** | **`+545`** | **1** |
| `0xF26C87EA` | `+548` | 2 |

```
1D972DCA 00     ; +544 = 0
559B9CE5 01     ; +522 = 1
06F1479E 01     ; +545 = 1   UI_FRONTEND_BUTTON_INVISIBLE
EA876CF2 02000000
```

Parent `#624` `UI_FRONTEND_LIST_PRESS_START_MENU` same
four CRCs, last u8 **`06F1479E 00`**. TEXT / SWAP / LEGAL /
MOUSE / PRESS_START root match that 0
(`export/frontend/persist-tail.txt` TEXT `@1067`
`0x9E47F106 ? u8=0`). `persist-tail` never dumps `#625`;
the inflated hex above is the lock.

---

## 3. First-seen Press Start — this gate is open

```
0042E3EE  type 4 → 0055CB10(26)
  type-11 INVISIBLE  0054DBC0
    debounce?        → drop
    +545==0          → drop   (file is 1 → not this)
    +545==1          → 0055AD60
      +352==0        → no 0055AF60   (ctor 0055BA20)
      else           → 0055AF60(+372==0)  no 0xE5
  type-12 list       +545==0; apply is 0053D200, not 0054DBC0
```

`invisible-button-e5` §4 sketched `+545==0 → drop` as a
**possible** path. File **1** closes that path on this
widget. The leftover C# DFS of INVISIBLE `MessageId`
`0xE5` is still leftover vs attach type-10 `+352`.

---

## Do not invent

- Parent-list `+545` as the type-11 apply gate.
- Lionhead name for `0x9E47F106`.
- `#625` `+545==0`.
- `+545==1` as “action 26 posts” (still `+352` / `+224` /
  `+372`).
- C# `ReadPersistU8` already wired for this CRC.
