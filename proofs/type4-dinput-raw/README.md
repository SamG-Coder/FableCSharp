# Type 4 is DINPUT mouse button 0 down

Investigation only. No production `src/` edits.

Authority: `Fable.exe` `00AB4910` / `00AB5420` / `00A03C80` /
`00AB5710`; listings `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00a80000.txt`
and `listing-00a00000.txt`;
`proofs/type4-input-lifecycle/README.md`;
`src/Fable.Game/FrontendInputMap.cs`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

---

## Verdict

**Type 4 is left-mouse down, not a DIK.**

| Claim | Status |
| --- | --- |
| `00AB5420` `[esi+8]` that calls `00A03C80` is **1** | **PROVEN** |
| `00A03C80` writes `[record+40]=4` | **PROVEN** |
| Raw 1 is `GetDeviceData` `dwOfs=12` `DIMOFS_BUTTON0` and `dwData & 0x80` | **PROVEN** |
| Type 4 is Return / `DIK_RETURN` (28) | **DISPROVEN** |
| `0xAB28CC` is the `00AB5420` index table | **DISPROVEN** — that VA is `fstp [edx+44]` displacement `0x2C` |

`0042E3EE` type 4 → action 26 is unchanged. The missing physical
device is **mouse button 0 press**.

---

## 1. `00AB5420` `[esi+8]` → `00A03C80`

Arg0 `esi` is the 16-byte translator record (`fld qword [esi]`,
`[esi+8]` enum, `[esi+12]` payload).

```
00AB54D3  mov eax, [esi+8]
00AB54D6  lea ecx, [eax-1]          ; ecx = raw-1
00AB54D9  cmp ecx, 23
00AB54DC  ja 00AB5669               ; default, no ctor
00AB54E2  movzx edx, [ecx+0xAB56EC] ; 11228908
00AB54E9  jmp [0xAB56C4+edx*4]
00AB54F0  … call 00A03C80           ; jt[0]
```

`00A03C80`:

```
mov [ecx+32], 3
mov [ecx+40], 4
```

### Jump table `0xAB56C4` (10 dwords)

Linear listing decodes these as `push`/`stosd`; bytes are
little-endian VAs.

| i | dword @ | Target | Ctor / note |
| ---: | --- | --- | --- |
| 0 | `00AB56C4` | `00AB54F0` | `00A03C80` **`+40=4`** |
| 1 | `00AB56C8` | `00AB553E` | `00A03E40` `+40=0xA` |
| 2 | `00AB56CC` | `00AB5517` | `00A03D90` `+40=7` |
| 3 | `00AB56D0` | `00AB5590` | `00A03D60` `+40=6` |
| 4 | `00AB56D4` | `00AB55EE` | `00A03EC0` `+40=0xC` |
| 5 | `00AB56D8` | `00AB55BF` | `00A03E10` `+40=9` |
| 6 | `00AB56DC` | `00AB5650` | `00A04000` |
| 7 | `00AB56E0` | `00AB5565` | `00A03EF0` `+40 = arg+22` |
| 8 | `00AB56E4` | `00AB561D` | `00A03F70` `+40 = arg+32` |
| 9 | `00AB56E8` | `00AB5669` | default |

### Index `0xAB56EC` (24 bytes, `raw-1` in 0..23)

```
00 01 02 03 04 05 09 09 09 06 09 09
09 09 07 07 07 07 07 08 08 08 08 08
```

Reconstructed from the same listing (`add`/`or`/`db 0x07` at
`00AB56EB`–`00AB5703`). Only **raw 1** has index 0.

| `[esi+8]` | idx | Dest |
| ---: | ---: | --- |
| **1** | 0 | **`00A03C80` type 4** |
| 2 | 1 | type `0xA` |
| 3 | 2 | type 7 |
| 4 | 3 | type 6 |
| 5 | 4 | type `0xC` |
| 6 | 5 | type 9 |
| 7–9, 11–14 | 9 | default |
| 10 | 6 | `00A04000` |
| 15–19 | 7 | `00A03EF0` |
| 20–24 | 8 | `00A03F70` |

### Requested `0xAB28CC`

Not this switch. `00AB28CA` is `fstp [edx+44]` (`DD 5A 2C`);
byte `00AB28CC = 0x2C`. The `00AB5420` index immediate is
`0xAB56EC`.

---

## 2. Who writes `[esi+8] = 1`

`00AB5710` acquire + `SetDataFormat` (`vtbl+44`)
`0x12ADDFC` then `0x12ADBF4`, `SetProperty` buffer size
`0x100`, then `00AB4910`.

`00AB4910` is `IDirectInputDevice8::GetDeviceData`
(`vtbl+40`) into `this+13376`, 20 × 20-byte
`DIDEVICEOBJECTDATA`. `DIERR_INPUTLOST` (`0x8007001E`) →
`Acquire` (`vtbl+28`) and retry.

```
ebx → { dwOfs+0, dwData+4, dwTimeStamp+8, … }
cmp [ebx], 19
movzx edx, [eax+0xAB4B90]    ; 11226000
jmp [0xAB4B60+edx*4]
```

`cmp 19` is `DIMOUSESTATE2` (`rgbButtons[8]`, last ofs 19),
not a keyboard `DIK_*` range.

### Jump table `0xAB4B60` (12 dwords)

| i | Target | Meaning |
| ---: | --- | --- |
| 0 | `00AB4A0A` | `dwOfs` 0 → raw **7** (`DIMOFS_X`) |
| 1 | `00AB4A33` | `dwOfs` 4 → raw **8** (`DIMOFS_Y`) |
| 2 | `00AB4A5C` | `dwOfs` 8 → raw **0xA** (`DIMOFS_Z`) |
| 3 | `00AB4A72` | `dwOfs` 12 → raw **1 / 4** (`BUTTON0`) |
| 4 | `00AB4A8D` | `dwOfs` 13 → raw **2 / 5** (`BUTTON1`) |
| 5 | `00AB4AA2` | `dwOfs` 14 → raw **3 / 6** (`BUTTON2`) |
| 6 | `00AB4ABB` | `dwOfs` 15 → raw **15 / 20** (`BUTTON3`) |
| 7 | `00AB4AD3` | `dwOfs` 16 → raw **16 / 21** |
| 8 | `00AB4AE5` | `dwOfs` 17 → raw **17 / 22** |
| 9 | `00AB4AFE` | `dwOfs` 18 → raw **18 / 23** |
| 10 | `00AB4B16` | `dwOfs` 19 → raw **19 / 24** |
| 11 | `00AB4B4E` | unused ofs → fail |

### Index `0xAB4B90` (20 bytes, `dwOfs` 0..19)

```
00 0B 0B 0B 01 0B 0B 0B 02 0B 0B 0B 03 04 05 06 07 08 09 0A
```

`0x0B` is default. Sparse 4-byte axes, then one byte per button.

### Button 0 down

```
00AB4A72  mov al, [ebx+4]    ; dwData
          and al, 0x80
          neg al
          sbb eax, eax       ; -1 if down, 0 if up
          and eax, -3
          add eax, 4         ; down → 1, up → 4
          mov [edi+8], eax
```

`dwTimeStamp` is stored as the `qword` at `[edi+0]`
(`fild [ebx+8]` × `0x1266300`).

So the only DINPUT pair that reaches `00A03C80`:

| Field | Value | Name |
| --- | ---: | --- |
| `dwOfs` | `12` | `DIMOFS_BUTTON0` |
| `dwData` | bit 7 set (`0x80`) | button down |

Button 0 **up** (`dwData & 0x80 == 0`) writes raw **4**, which
is `00A03D60` type **6**, not type 4.

---

## 3. Sibling raw → event type (mouse)

| DINPUT | `dwOfs` | `dwData&0x80` | raw `[+8]` | `+40` |
| --- | ---: | --- | ---: | ---: |
| LMB down | 12 | 1 | **1** | **4** |
| RMB down | 13 | 1 | 2 | `0xA` |
| MMB down | 14 | 1 | 3 | 7 |
| LMB up | 12 | 0 | 4 | 6 |
| RMB up | 13 | 0 | 5 | `0xC` |
| MMB up | 14 | 0 | 6 | 9 |
| X / Y / Z | 0 / 4 / 8 | n/a | 7 / 8 / `0xA` | default in this switch |

Keyboard `DIK_*` never enter `00AB4910` (`dwOfs>19` is default).
Return remains type 1 / action 33.

---

## 4. C# leftover

`FrontendInputMap` already names `Type4RecordCtor=00A03C80` and
`Type4TranslateFn=00AB5420`, and still says the physical device
is UNREAD. Host Return → type 1 is correct; it must not synthesize
type 4. Type 4 is **LMB down**. No `src/` change in this proof.
