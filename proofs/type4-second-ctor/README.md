# `00A0BF07` `[ecx+40]=4` is not the type-4 input ctor

Investigation only. Production `src/` was not edited.

Question: `listing-00a00000.txt` also has `00A0BF07 mov [ecx+40], 4`.
Who calls it? Same device as `00A03C80` or different?

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Sources: `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00a00000.txt`,
`listing-00a80000.txt`, `listing-00400000.txt`, `listing-00480000.txt`,
`e8.tsv`; `newgame-trace/camera-update-helper-fov-00b314e0-00b314e0.md`;
`proofs/type4-input-lifecycle/README.md`;
`implementer/frontend/05-input.md`.

---

## Verdict

**Different object. Not the type-4 input device.**

| Site | Writes | Also writes | Object | Sole `.text` E8 |
|---|---|---|---|---|
| `00A03C80` | `[ecx+40]=4` | `[ecx+32]=3`, `+24/+28` pair, `+44/+48` same double | input record (`00A03B40` reads type) | `00AB5500` inside `00AB5420` |
| `00A0BF00` (`00A0BF07`) | `[ecx+40]=4` | `[ecx+52]=arg0` only | FOV blob (`00A0BE80` / `00A0BE90`) | **none** |

`.text` has exactly two `mov [ecx+40], 0x4` sites (`listing-00a00000.txt`).
They do not share a class, vtbl, or caller.

Physical type-4 **input** producer is no longer fully **UNREAD**:
only `00A03C80` fills `[record+40]=4`, and only `C*DX` vtbl
`0129EA14` poll `00AB58E0` → `00AB5420` calls it. Keyboard type 1
(`00A03BF0`) is a different ctor. Which control id in the
`00AB56C4` switch hits `00A03C80` is still **UNREAD**.

---

## 1. `00A03C80` — input record type 4 (**PROVEN**)

```
00A03C80  mov eax, [esp+4]
00A03C84  fld qword [esp+8]
00A03C88  mov [ecx+32], 0x3
00A03C8F  mov [ecx+40], 0x4
00A03C96  mov edx, [eax]          ; +24/+28 from ptr
00A03C9B  mov eax, [eax+4]
00A03C9E  fst [ecx+48]
00A03CA1  fstp [ecx+44]           ; same double both slots
00A03CA4  mov [ecx+28], eax
00A03CA7  ret 12
```

Getter `00A03B40` is `mov eax, [ecx+40]; ret`. `0042E3EE` at
`0042E456` reads that and the `dec` / `sub eax, 3` chain hits
`0042E4A4` `push 26` when type==4 (`listing-00400000.txt`). That
is the Press Start action-26 path already **PROVEN** in
`type4-input-lifecycle`.

Family on the same page (`00A03BF0` type 1, `00A03C20` type 2,
`00A03C50` type 3, `00A03D10` type 5, `00A03D60` type 6) all
also write `[ecx+32]` + `[ecx+40]`. `00A0BF00` does not touch
`+32`.

### Who calls `00A03C80`

`e8.tsv`: one site, `00AB5500` → `00A03C80`.

`00AB5420` (`listing-00a80000.txt`) is thiscall `(sample, dest_record)`.
`[sample+8]` is a control id. Second switch (`id-1`, 0..23) case
at `00AB54F0` does:

```
fld qword [esi]                 ; sample value
mov ecx, [ebp+12]               ; dest record
lea eax, [esp+32]               ; pair = this+13332 / +13336
call 00A03C80
```

`+13332/+13336` are the current 2D origin (later `00AB5C09`
stores half client W/H there). Type 4 therefore carries that
pair plus the sample double. Which id the `0xAB56C4` table maps
to `00AB54F0` is **UNREAD** (table bytes, not recovered names).

### Device around `00AB5420`

| VA | Role |
|---|---|
| `00AB5D00` | ctor; `mov [esi], 0x129EA14`; `+13344` = device ptr |
| `00AB5320` | also plants `0129EA14` |
| `00AB58E0` | poll. No E8 callers → vtbl slot |
| `00AB59B7` | only E8 to `00AB5420` |
| `00AB5940` / `00AB594E` | `00AB4910` if `[+13372]==1` else `00AB4BB0` |
| `00AB4BB0` | `GetCursorPos` / `GetWindowRect` / `GetClientRect` (`[0x14403A8]`, `[0x1440304]`, `[0x1440338]`) |
| acquire block `00AB5814`… | IDirectInputDevice `SetCooperativeLevel` (`vtbl+52`, `0x10`), `SetDataFormat` (`+44`, `0x12ADDFC` then `0x12ADBF4`), `Acquire` (`+28`) |

RTTI names in range: `CMouseDX` / `CJoystickDX` / `CKeyboardDX`
(`rtti.txt` `0x01399038`…`0x01399070`). Object reaches `+18524`
and walks 24 control ids — joystick-sized — but the sample path
used after a successful acquire is the Win32 cursor helper.
Class name **PARTIAL**. It is **DISPROVEN** that this is the
keyboard type-1 ctor or the FOV blob.

`00AB58E0` also sums analog axes (`esp+20/24/28`) and can drive
`SetCursorPos`. Type 4 is one mapped control on that poll, not
a second device.

---

## 2. `00A0BF00` / `00A0BF07` — FOV flag 4 (**PROVEN** different)

```
00A0BF00  mov eax, [esp+4]
00A0BF04  mov [ecx+52], eax
00A0BF07  mov [ecx+40], 0x4
00A0BF0E  ret 4
```

Same file, ~48 KB later. Neighbours (`listing-00a00000.txt` /
`newgame-trace`):

| VA | Body | Trace name |
|---|---|---|
| `00A0BE80` | `mov eax, [ecx+40]; ret` | FOV flag getter |
| `00A0BE90` | `fld [ecx+44]` | FOV H |
| `00A0BEA0` | `fld [ecx+48]` | FOV V |
| `00A0BEB0` | `fld [ecx+44]` | lerp source |
| `00A0BEC0` | `fld [ecx+52]` | type-4 payload getter |
| `00A0BED0` | `+44/+48` floats; `+40=1` or `3` if `fcomp [0x122DEDC]` | FOV set (used) |
| `00A0BE30` | copy two xyz, `00A14440` normalize | look/up |

Camera update `00B314E0` (`esi+12` = this blob):

```
call 00A0BE90                   ; H
call 00A0BE80
test al, 0x02                   ; +40 is FLAGS, not CInputType
je   skip_V
call 00A0BEA0                   ; V only if bit 1
```

So `+40==4` here is bit 2 of a FOV mode, not event type 4.
`00A0BED0` (flags 1 or 3) is the live setter: E8 from
`0049EEDF`, `00697917` (lerp two FOVs), `0084DF83`, `0087BB85`.

### Who calls `00A0BF00`

| Probe | Result |
|---|---|
| `e8.tsv` dest `00A0BF00` / `00A0BEC0` | **zero** |
| `.text` `call` / `jmp 00A0BF00` | **zero** |
| `.text` immediate `0xA0BF00` | **zero** |

No rdata listing, so a vtbl slot is **UNREAD**. Live camera /
script paths use `00A0BED0`, not this setter. Unused-or-rdata
only; either way it does not produce `0042E3EE` type 4.

---

## 3. Same device? **DISPROVEN**

| | `00A03C80` | `00A0BF00` |
|---|---|---|
| `[+32]` | 3 | never written |
| `[+40]` meaning | `CInputType` enum, consumed by `0042E3EE` | FOV flags, `test al, 2` in `00B314E0` |
| Extra payload | `+24/+28` + double `+44/+48` | dword `+52` |
| `this` | dest record from `00AB58E0` poll | camera `+12` FOV blob |
| E8 caller | `00AB5420` | none |

`00B59B80` `mov [esi+40], 0x4` is a third object (`vtbl 012A2588`)
and is not `ecx+40`. Ignore for this question.

---

## Host

Do not treat `00A0BF00` as a second Press Start device.
Do not invent a DIK for type 4 from this VA.

`FrontendInputMap` type 4 → action 26 stays the `00A03C80` /
`0042E3EE` contract. Host may later queue type 4 from the
`0129EA14` poll once the `00AB56C4` id is recovered; that id
is still **UNREAD**.
