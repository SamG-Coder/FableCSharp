# Type-11 `UI_FRONTEND_BUTTON_NEW_GAME` posts persist 15 on action 26

Authority: `Fable.exe` listing `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
(+ `listing-00500000.txt` for type 8/12 subscribe); inflated `frontend.bin`
`UI_FRONTEND_BUTTON_NEW_GAME`; `0059A238` consumer already recovered.

Related (do not re-prove): type 4 → action 26 (`0042E3EE`); type-10
`0054E280` posts widget+352 (`0xE5` on Press Start); msg 15 →
`0059A2DA` / `[retail+41]=1` / Leave. See
`proofs/who-posts-0x126-and-15/README.md`,
`proofs/type4-input-lifecycle/README.md`,
`implementer/frontend/05-input.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

---

## Verdict

| Claim | Status |
| --- | --- |
| Type 11 ctor `0054E0B0` | **PROVEN** |
| Ctor → type 34 `0055B460` → persist copy `0055B040` | **PROVEN** |
| `0055B040` reads `[def+224]`, stores via vtbl+284 | **PROVEN** |
| File CRC `0x53C644E4` i32 **15** on `UI_FRONTEND_BUTTON_NEW_GAME` | **PROVEN** (parser + tests). Offset 1145 in that entry raw. |
| CRC name | **UNREAD** (not `Message` / `MessageId`) |
| Inner `0054DBC0` forwards action to `0055AD60` | **PROVEN** |
| `0055AD60` case 0 is action 26 | **PROVEN** (`lea eax,[edi-26]`, table `0x55AE88` dword0 `0055AD7B`) |
| Action 26 click → outer vtbl+584 → `0055AF60` → vtbl+524(`[widget+372]`) | **PROVEN** call sites; vtbl slot IDs **PARTIAL** (no rdata listing) |
| Type 10 Main Menu posts 15 | **DISPROVEN** (`0054E280` posts `+352` only if nonzero; no attach-15) |
| Who receives action 26 on Main Menu | **PROVEN** subscribers; exclusive first-match **DISPROVEN** |

---

## 1. Construct (PROVEN)

`0041D21B` type 11 → alloc `0x1B4` → `0054E0B0`.

```
0054E0B0  mov eax, [esp+4]          ; def
0054E0B8  call 0055B460             ; type 34 ctor
0054E0BF  mov [esi],     0x1249554  ; outer vtbl
0054E0C5  mov [esi+4],   0x1249530  ; inner vtbl
0054E0CC  mov [esi+24],  0x1249528
0054E114  call 0054DF50             ; extra persist (def+196), not +224
```

`0055B460` (type 34, also used by type 38 `00558B90`):

```
0055B468  call 0055BA20             ; type 33
0055B471  mov [esi], 0x124BD2C      ; type 34 vtbl (live during persist copy)
…
0055B4B5  call 0055B040
```

Type 11 then **overrides** the vtbl. Persist copy therefore uses type-34
vtbl+284, not the later type-11 table.

---

## 2. Persist copy `0055B040` `[def+224]` → vtbl+284 (PROVEN)

```
0055B052  call [eax+432]            ; get CUIDef
0055B05C  mov edx, [eax+388]
0055B062  mov [ebx+396], edx
0055B068  mov ecx, [eax+224]        ; persist message i32
0055B06E  test ecx, ecx
0055B075  je  0055B15A              ; 0 → no store
…
0055B0A2  mov ecx, [eax+224]
0055B0AC  mov [edx], ecx            ; object dword0 = def+224
0055B12E  call [edx+284]            ; store
```

Same function then copies `[def+228]` via vtbl+320, `[def+232]` via
vtbl+288, then vtbl+292. First-seen New Game only needs +224.

Type-34 store `0055B520` (vtbl+284 while `0055B040` runs) appends that
object onto a list at **widget+372**. Destructor `0055B760` frees
`[esi+372]` first.

Type-10 analog remains `0054E4F0`: vtbl+284 writes the raw i32 at
widget+352. Different layout, same persist helper.

---

## 3. File: CRC `0x53C644E4` = 15 (PROVEN)

`FrontendUiDef.MessageIdCrc = 0x53C644E4`. Sequential skip +
`ReadPersistI32`. Name **UNREAD**
(`FrontendUiDefTests` ≠ `FableCrc("Message")` / `"MessageId"`).

| Def | Type | Value after CRC |
| --- | --- | --- |
| `UI_ACCEPT_NEW_PROFILE` | 38 | `0x126` |
| `UI_FRONTEND_BUTTON_NEW_GAME` | 11 | **15** |

Tests: `FrontendUiDefTests.Persist_00631C60_plus189_plus190_are_u8_and_font_is_names_offset`
(MessageId asserts), factory copies `def.MessageId` onto the widget.

`UI_FRONTEND_BUTTON_NEW_GAME` raw: CRC at file off **1145** (`0x479`),
payload i32 15. A second 15 after `0xF1A22807` is **not** `[def+224]`
(`0055B040` only reads the runtime def dword).

`0059A238` msg 15 → `0059A2DA` is already proven. No `.text`
`mov […], 15` writer — the file is the source.

---

## 4. Action 26 on type 11 (PROVEN)

Subscribe (enable) `0054DC30` / unsub `0054DD0E`:

```
push 3
call [edx+192]          ; SelectState
add esi, 4              ; inner
push 26
call [eax+12]           ; 0055CB10 subscribe
; also 31, 28, 27, 32, 29
```

Type 34 `0055AEB0` subscribes 26 / 31 / 27 / 32 the same way.

Handler (inner vtbl+4) `0054DBC0`:

```
; hold-off compare on +44 / +400 / +392
call [eax+432]
mov bl, [eax+545]       ; def flag
test bl, bl
je  0054DC21
push [action]
call 0055AD60
```

`0055AD60`:

```
mov edi, [esp+12]       ; action
lea eax, [edi-26]
cmp eax, 6
ja  default → 0055B9D0
jmp [0x55AE88+eax*4]
```

Table dword0 at `0055AE88` is `7B AD 55 00` = **`0055AD7B`** (action 26):

```
0055AD7B  mov al, [esi+348]     ; inner+348 = widget+352
          test al, al
          je  0055AE3D          ; no click
          lea ecx, [esi-4]
          call [eax+584]        ; click
          mov [esi+364], 1
          call 0055B9D0
```

`0055B9D0` is **not** the poster (`cmp arg,25` → vtbl+580 only).

Click body `0055AF60` (0-arg, matches vtbl+584):

```
call [edx+432]
push [def+524]
call [edx+192]          ; state from def
push [esi+372]          ; persist list from §2
call [eax+524]          ; post
push 28
call [edx+12]           ; re-subscribe 28
```

vtbl+524 is the widget “post this stored id” slot (type 8/12 also
call it with `[+428]` / `[+432]`). Exact callee VA **UNREAD** (rdata
vtbl `0x1249554+584` / `+524` not listed). Consumer is UI vtbl+32
`0059A238` (same as type-10 `0054E315`).

`[widget+352]` as the action-26 click gate is set to 1 by `0055C0DE`
when the widget takes selection. First-seen list index is 0
(type 8 ctor `0053B662` zeros `+348`).

---

## 5. Who receives action 26 on Main Menu?

Tree (`01-widget-construction.md`, factory walk `005331A0`):

```
UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE   type 10
└── first persist child: UI_FRONTEND_LIST_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE
        type 12  ctor 0054C3A0  (0053B63E then vtbl 0x1249224 / inner 0x12491FC)
    └── first child: UI_FRONTEND_BUTTON_NEW_GAME   type 11
```

`0055CB10` (`FrontendInputMap.ActionApply`):

- If `[input+8]` set: predicate vtbl+8, then handle vtbl+4, **return**.
- Else walk `[input+12]` list: **every** node with predicate true gets
  vtbl+4. No exclusive first-match.

Subscribed to 26 on this screen:

| Widget | Subscribe | Handler | Action 26 effect |
| --- | --- | --- | --- |
| Type 10 menu | type-10 inner (Press Start proven) | `0054E280` | Post `+352` **if nonzero**. Main Menu attach is `0059899A`, **not** `00598EE6` `0xE5`. First-seen `+352=0` → no-op. |
| Type 12 list | type 8 `0053D5C1` `push 26` | type 8 `0053D200` (type 12 inner override **PARTIAL**) | Forwards to `0055AD60` if def+545. List `+348` is **selected index**, not the click flag. Also posts list `+428/+432` on some `+324` states (`00536A0B`) — those are **not** 15. |
| Type 11 first child | `0054DC7E` `push 26` | `0054DBC0` → `0055AD60` | Click vtbl+584 posts persist **15** when selected (`+352≠0`). |

**Answer:** action 26 is delivered to **every subscribed inner** whose
vtbl+8 is true. On first-seen Main Menu that includes the **type-12
list** and its **type-11 first child**. The persist-15 **poster** is
the type-11 child, not the type-10 root and not the list’s own
`+428/+432`.

Physical device that produces type 4 remains **UNREAD**. Return
(DIK 28) is type 1 / action 33 — **DISPROVEN** as a 15 poster.

---

## C#

`FrontendUiDef.MessageId` / factory `MessageId`.
`FrontendInputMap.MessageFromWidgets` returns the first visible
type 10/11/38 stored id on action 26. Press Start type 10 is forced
to `0xE5` when persist is 0 (`EngineLifecycle.AttachFrontendTree`).
`MessageFromAction(screen)` is unused (always null).

No production change in this proof.
