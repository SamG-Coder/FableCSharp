# Type 38 `UI_ACCEPT_NEW_PROFILE` persist `0x126` on action 26

Investigation only. No production `src/` edits.

Authority: `Fable.exe` `0041D21B` / `00558B90` / `0055B460` /
`0055B040` / `0055AD60` / `0059A238` / `00851920`;
`00631C60` / `00632500`; inflated `frontend.bin`;
`FrontendUiDef.MessageIdCrc`;
`FrontendInputTests` / `FrontendUiDefTests`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Do not re-prove Return→`0xE5`/`0x126`/15 (**DISPROVEN**).
Do not treat type-10 attach `0xE5` (`00598EE6` / `+352`) as this
poster.

---

## Verdict

| Claim | Status |
| --- | --- |
| Type 38 factory is `0041D21B` → `00558B90` | **PROVEN** |
| `00558B90` → `0055B460` → `0055B040` | **PROVEN** |
| `UI_ACCEPT_NEW_PROFILE` type 38 file i32 after CRC `0x53C644E4` is `0x126` | **PROVEN** |
| That CRC sits at inflated raw `+939` | **PARTIAL** (scan-proven; fixed offset not asserted) |
| CUIDef persist writes def `+224` as the next load i32 (`00632500`) | **PROVEN** |
| `0055B040` copies `[def+224]` through vtbl+284 when nonzero | **PROVEN** |
| Inner `0055AD60` `lea eax,[action-26]` case 0 is action 26 | **PROVEN** |
| Action 26 case 0 itself `push 0x126` / `call 0059A238` | **DISPROVEN** (calls outer vtbl+584) |
| `0059A238` msg `0x126` → `00851920` `[ui+96+5]=1` | **PROVEN** |
| `.text` `mov […], 0x126` writer | **DISPROVEN** |
| Return (DIK 28) posts `0x126` | **DISPROVEN** |

---

## 1. Construct

`0041D21B` `Type=[def+60]`, `jmp [0x41D7F8+type*4]`. Type 38 arm
(`listing-00400000.txt`):

```
0041D35C  push 0x194
0041D361  call 00BFEA1A
0041D372  call 00558B90
```

`00558B90` (`listing-00540000.txt`):

```
00558B90  mov eax, [esp+4]          ; def
00558B98  call 0055B460             ; type-34 body
00558B9D  mov [esi], 0x124B04C      ; type-38 outer vtbl
00558BA3  mov [esi+4], 0x124B024    ; inner
00558BAA  mov [esi+24], 0x124B01C
```

`0055B460`:

```
0055B468  call 0055BA20             ; 0052CC50 + inner subscribe
          ; [esi]=0124BD2C, +364..+392 = 0
0055B4B5  call 0055B040
```

`0055BA20` zeros outer `+348/+352/+356/+360` and
`0041E5F2` vtbl+8 on the inner at `+4`. `0055AEB0` (same family)
`push 26` / `31` / `27` / `32` into inner vtbl+12 — this widget
**subscribes** action 26.

Type 11 (`0054E0B0`) also calls `0055B460` then overwrites vtbl to
`01249554`. Its inner action `0054DBC0` forwards to `0055AD60`.

---

## 2. Persist `0x53C644E4` → def `+224` → `0x126`

CUIDef persist `00631C60` (`listing-00600000.txt`):

```
00631FBD  lea edx, [esi+224]
00631FC6  call 00632500
```

`00632500` is the same load-i32 helper family as `00431102`:
`00404500(0x122D70E)` then mode 2 `00632550` `mov edi,[eax]; add eax,4`
into the destination. On load the 4-byte field CRC is **skipped**,
not matched. Sequential persist order places that i32 at def `+224`.

`0055B040` (ecx = constructed widget):

```
call [vtbl+432]                     ; def*
mov ecx, [eax+224]
test ecx, ecx
je 0055B15A
  box ecx (0042BE50 / 0042AA29)
  cmp [boxed], 65
  jne 0055B125                      ; 0x126 != 65
  call [vtbl+284]                   ; store boxed id
```

Siblings: `[def+228]` → vtbl+320, `[+232]` → +288, `[+236]` → +292.
`0x126` takes the `+224` / vtbl+284 arm.

File (install `frontend.bin`, test
`Persist_00631C60_plus189_plus190_are_u8_and_font_is_names_offset`):

| Entry | Type | CRC `0x53C644E4` i32 |
| --- | --- | --- |
| `UI_ACCEPT_NEW_PROFILE` | 38 | `0x126` |
| `UI_FRONTEND_BUTTON_NEW_GAME` | 11 | 15 |

Name of `0x53C644E4` is **UNREAD** (`FableCrc("Message")` /
`("MessageId")` do not match). C# `ReadPersistI32` finds the first
CRC hit; it does **not** lock byte `939`. Assignment offset `939`
is **PARTIAL** until a raw-index assert exists.

No `.text` immediate `0x126` store. The id lives in the def blob.

---

## 3. `0055AD60` jump (action 26)

ecx is the **inner** object (`widget+4`). `lea ecx,[esi-4]` is the
outer this-adjust.

```
0055AD66  lea eax, [edi-26]
0055AD69  cmp eax, 6
0055AD6E  ja 0055AE79                 ; 0055B9D0
0055AD74  jmp [0x55AE88+eax*4]
0055AD7B  ; case 0 = action 26
          mov al, [esi+348]           ; outer+352
          je 0055AE3D
          call [outer.vtbl+584]
          [esi+364]=1
          call 0055B9D0
```

Jump table starts `7B AD 55 00` = `0055AD7B`. Action 26 is case 0.

Case 0 does **not** `push` the boxed persist id and does **not**
call UI vtbl+32 (`0059A238`). That is type-10 `0054E2FA`
(`push &widget+352`).

Other cases in the same switch `push [esi+372]` / `[esi+388]` and
`call [outer.vtbl+524]` (`0055AE30`). Type-38 thunk `00558D90`
posts `[esi+360]` on **action 30**, not 26.

So: action 26 is the subscribed click on this widget; the persist
id is already on the object from `0055B040`; the UI message hop is
vtbl+584 / later +524, **not** a `.text` `0x126` immediate.

---

## 4. Consumer `00851920`

`0059A238` (`listing-00580000.txt`):

```
cmp ecx, 0x127
…
sub ecx, 0xE5          ; 0xE5 → 00599D5C
…
dec ecx                ; 0x125
dec ecx
jne 0059A7FF
mov esi, [esi+96]
call 00851920          ; msg == 0x126
```

`00851920`: if `[this+5]==0` and trim length `>0`,
`[this+5]=1` `[this+4]=0`. Next `00599E3F` → `0059697A` /
`004067C0` writable → `MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE`.

Relation: persist `0x126` is the **id** `0059A238` already knew
how to consume. Type 38 is the **store** of that id. Action 26 is
the **user event** that fires the widget that holds it. Host
`FrontendInputMap.MessageFromWidgets` posts the stored id on
action 26 (type 10/11/38). That is equivalent to “click the
visible stored-id widget”, not a clone of case 0’s vtbl+584 body.

---

## 5. C# vs leftover

| Site | Native | Host |
| --- | --- | --- |
| `FrontendUiDef.MessageId` | CRC scan `0x53C644E4` | **MATCH** value on accept / New Game |
| Factory `MessageId` | ctor `0055B040` | copied from def |
| Type 4 → action 26 | `0042E3EE` `push 26` | **MATCH** |
| Action 26 → `0x126` | type-38 `0055AD60` + persist store | first visible type 10/11/38 `MessageId` |
| `MessageFromAction(screen)` | unused for this id | always null |
| Return | action 33 | null |

Physical device that produces type 4 remains **UNREAD**.

---

## Sources

- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
  (`0041D35C`)
- `listing-00540000.txt` (`00558B90`, `0055B460`, `0055B040`,
  `0055AD60`, `0054DBC0`, `0054E280`)
- `listing-00580000.txt` (`0059A6AB`)
- `listing-00840000.txt` (`00851920`)
- `listing-00600000.txt` (`00631C60` / `00632500`)
- `tests/Fable.Formats.Tests/FrontendUiDefTests.cs`
- `tests/Fable.Formats.Tests/FrontendInputTests.cs`
- `proofs/who-posts-0x126-and-15/README.md` (shorter sibling)
- `proofs/type4-input-lifecycle/README.md` (Press Start `0xE5` only)
