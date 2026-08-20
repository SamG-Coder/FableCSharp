# Persist `0x230364D6` (`+224`) is **0** on Accept / New Game; action 26 posts nothing

Investigation only. No production `src/` edits.

Authority: inflated `frontend.bin` via
`FrontendUiDef.ReadPersistI32` /
`FrontendUiDefTests.Persist_00631C60_plus189_plus190_are_u8_and_font_is_names_offset`;
`Fable.exe` `listing-00540000.txt` (`0055B040` / `0055B460` /
`0055AF60` / `0055AD60` / `00558DE0` / `0055ACF0`);
`proofs/messageid-plus228/README.md`;
`proofs/0055B9D0-post-dword/README.md`;
`proofs/crc-230364D6/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**STALE** / **LEFTOVER**.

Do not re-prove CRC map `+224` = `0x230364D6`, `+228` =
`0x53C644E4`. Do not invent Lionhead names.

---

## Verdict

| Widget | Type | `0x230364D6` (`+224`) | `0x53C644E4` (`+228`) |
| --- | ---: | ---: | ---: |
| `UI_ACCEPT_NEW_PROFILE` | 38 | **0** | **`0x126`** |
| `UI_FRONTEND_BUTTON_NEW_GAME` | 11 | **0** | **15** |
| `UI_FRONTEND_BUTTON_INVISIBLE` | 11 | **0** | **`0xE5`** |

`0055B040` `test [def+224]; je 0055B15A` therefore **skips
vtbl+284** on both lifecycle buttons. Type-34 ctor already
zeroed `widget+372`. Action 26 `0055AF60` still
`push [this+372]` → `vtbl+524`. `00558DE0` `test edi,edi;
je` empty: **no** `0041E6D3`, **no** `0059A238`.

**Action 26 posts no UI message** on Accept / New Game.
It does **not** post `0x126` or 15.

Those integers live on **`+228`**. `0055B040` still copies a
nonzero `+228` through vtbl+320 into list `+380`. Action 26
never pushes `+380`. `0055ACF0` does.

`0055B9D0-post-dword` “action 26 posts `0x126` / 15” is
**STALE** for these two defs (it posts the `+224` list, which
is empty). Host `MessageFromWidgets` posting factory
`MessageId` (`+228`) is **LEFTOVER** vs native action 26.

---

## 1. File payloads (`ReadPersistI32`)

`00631C60` writes adjacent tail i32s (`00632500`, CRC skip +
4-byte payload):

```
00631FBD  lea edx, [esi+224]     ; CRC 0x230364D6
00631FCB  lea eax, [esi+228]     ; CRC 0x53C644E4
```

`FrontendUiDef.Plus224Crc` / `MessageIdCrc` **MATCH** that
pair (`messageid-plus228`). Tests scan the installed blob:

```
Assert.Equal(0, accept.Plus224);     // ReadPersistI32(0x230364D6)
Assert.Equal(0x126, accept.MessageId); // ReadPersistI32(0x53C644E4)
Assert.Equal(0, newGame.Plus224);
Assert.Equal(15, newGame.MessageId);
HasAdjacentPersistI32(Plus224Crc, MessageIdCrc)  // both entries
```

Hex form (same as INVISIBLE / PRESS_START in
`persist-scan.txt`):

```
D6640323 00000000   ; 0x230364D6 + i32 0
E444C653 26010000   ; 0x53C644E4 + i32 0x126   ACCEPT
E444C653 0F000000   ; 0x53C644E4 + i32 15      NEW_GAME
```

`0x230364D6` holding `0x126` / 15 is **DISPROVEN**.
`0x53C644E4` at dest `+224` is **DISPROVEN**.
Field strings **UNREAD** (`FableCrc("Message")` /
`"MessageId"` / `"Action"` are not this pair; `Action` is
`+196` `0xF1A22807`, also 15 on NEW_GAME).

---

## 2. If `+224` is 0, `0055B040` skips vtbl+284

Type 11/38 ctor: `0054E0B0` / `00558B90` → `0055B460`:

```
0055B46D  xor eax, eax
0055B491  mov [esi+372], eax      ; list head = 0
0055B49D  mov [esi+380], eax
0055B4B5  call 0055B040
```

```
0055B068  mov ecx, [eax+224]
0055B06E  test ecx, ecx
0055B075  je  0055B15A            ; skip box + vtbl+284
0055B15A  mov edx, [esp+16]
0055B15E  mov eax, [edx+228]
0055B164  test eax, eax
0055B166  je  0055B24B            ; skip vtbl+320
          … box [def+228] …
0055B21F  call [edx+320]
```

Zero `+224` does **not** skip `+228`. ACCEPT / NEW_GAME
still box `0x126` / 15 onto the **second** list (`+380`,
store `0055B5B0` if that slot is vtbl+320 — rdata
**PARTIAL**).

`+372` stays the ctor 0. `0055B520` (append to `+372`)
never runs.

---

## 3. What action 26 posts

`0055AD60` case 0 (`0055AD7B`), `ecx` = inner:

```
test [esi+348]                ; widget+352 u8
je   skip click
call [outer.vtbl+584]         ; 0055AF60
[esi+364] = 1
call 0055B9D0                 ; action==25 only; nop for 26
```

`0055AF60` (outer):

```
call [vtbl+192]([def+524])    ; select-state (not a message)
mov  ecx, [esi+372]           ; 0
call [this.vtbl+524]          ; 00558DE0
push 28
call [inner.vtbl+12]          ; local map
```

`00558DE0`:

```
test edi, edi
je   00558E09                 ; ret 4 — no 0041E6D3
```

| Path | Posts `0x126` / 15? |
| --- | --- |
| Action 26 `vtbl+584` / `0055AF60` / `[+372]` | **no** (null list) |
| Action 26 tail `0055B9D0` | **no** |
| `vtbl+320` / `[+380]` / `+228` via `0055AF60` | **DISPROVEN** |
| `0055ACF0` `push [esi+380]` / `vtbl+524` | **yes**, if that fn runs; **not** action 26 |
| Type-11 `+196` Action vector `+408` | **DISPROVEN** (`action-crc-plus196`) |
| Host `MessageFromWidgets` | **yes** (`MessageId` / `+228`) — **LEFTOVER** |

`0055ACF0` callers in this listing: `00557AF4` (key
redefiner after subscribe 35) and `jmp`s at `0055A726` /
`0055A73B`. First-seen Accept / New Game apply does **not**
take those sites. Native first-seen click therefore
**does not** deliver `0x126` / 15 through action 26.

Action 26 still applies click state (`vtbl+192` /
`[inner+364]=1`). The missing piece is the UI dword, not
the arm.

---

## 4. C# leftover (do not apply here)

Keep `Plus224Crc = 0x230364D6`, `MessageIdCrc = 0x53C644E4`
at def `+228`. Tests already lock `Plus224==0` on these
two plus INVISIBLE.

`FrontendInputMap.MessageFromWidgets` / factory
`MessageId` still feed the lifecycle (`0x126` →
`00851920`, 15 → Leave). Native type 11/38 action 26
does not. Do not switch the host to post `+224` (it is 0).
Do not treat `0055B9D0` as a chooser.

---

## What this pass did not do

- Did not dump `0124BD2C+284` / `+320` / type-11/38 `+584`.
- Did not walk first-seen apply to prove `0055ACF0` never
  fires on Accept / New Game (callers recovered; attach
  **UNREAD**).
- Did not recover the Lionhead string for either CRC.
