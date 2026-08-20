# Type 6 / action 28 is LMB up, not accept

Investigation only. No production `src/` edits.

Authority: `Fable.exe` `00A03D60` / `00AB4910` / `00AB4BB0` /
`00AB5420` / `0042E3EE` / `0055CB10` / `0054E280` /
`0054DBC0` / `0055AD60` / `0055B9D0`;
listings `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00a00000.txt`,
`listing-00a80000.txt`, `listing-00400000.txt`, `listing-00540000.txt`;
`proofs/type4-dinput-raw/README.md`,
`proofs/action26-subscribers/README.md`,
`proofs/type13-vs-type4/README.md`.

Status: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Do not invent a DIK for type 6. Return (DIK 28) is type 1 / action 33.

---

## Verdict

**Type 6 is LMB up. `0042E3EE` maps it to action 28. First-seen
Press Start / New Profile / Main Menu: no stored-id post.**

Action 28 is **not** release of accept. On type 10 it is
**nothing**. On type 11/38 it is **cancel / unarm** of an already
armed press (`vtbl+588`, `[+364]=0`) and still does not post
`0xE5` / `0x126` / 15.

| Claim | Status |
| --- | --- |
| `00A03D60` writes `[+40]=6`, `[+32]=3` (mouse device) | **PROVEN** |
| Raw 4 is LMB **up** (`DIMOFS_BUTTON0` / primary `009A4FC0` release) | **PROVEN** |
| `00AB5420` raw 4 → `00AB5590` → `00A03D60` | **PROVEN** (`0xAB56EC[3]=3`) |
| `0042E3EE` type 6 → `0055CB10(28)` | **PROVEN** `0042E498` `push 28` |
| Type-10 `0054E280` action 28 posts `+352` / `0xE5` | **DISPROVEN** — case 3 is `0054E319` |
| Type-11/38 `0055AD60` action 28 posts persist `0x126` / 15 | **DISPROVEN** — case 2 is unarm |
| Action 28 is release-of-accept (`vtbl+524([+372])`) | **DISPROVEN** — that is action **27** |
| First-seen type 6 posts a frontend message | **DISPROVEN** |
| Type 6 is a DIK / Return | **DISPROVEN** |
| `vtbl+588` exact VA | **UNREAD** (no rdata listing) |
| Pad button-up also builds type 6 | **UNREAD** |

`FrontendInputMap.ActionFromEvent(6)=28` already matches.
`MessageFromWidgets` / `MessageFromAction` stay null on 28.

---

## 1. `00A03D60` is type 6, device 3 (**PROVEN**)

```
00A03D60  mov eax, [esp+4]
00A03D64  fld qword [esp+8]
00A03D68  mov [ecx+32], 0x3        ; same device as type 4
00A03D6F  mov [ecx+40], 0x6        ; CInputType
00A03D76  mov edx, [eax]
00A03D78  mov [ecx+24], edx        ; origin pair
00A03D7B  mov eax, [eax+4]
00A03D7E  fstp [ecx+48]
00A03D81  fld qword [esp+16]       ; second double (unlike type 4)
00A03D85  mov [ecx+28], eax
00A03D88  fstp [ecx+44]
00A03D8B  ret 20
```

`00A03B40` / `00A03B50` are getters for `+40` / `+32`. Sole `.text`
E8 to `00A03D60` is `00AB55A8` inside translator `00AB5420`.

Sibling `00A03C80` is type 4 / device 3 / one double (`ret 12`).
RTTI family: `CInputTypeMouseButtonEvent`.

---

## 2. Raw 4 is LMB up → type 6 (**PROVEN**)

`00AB5420` second switch (`listing-00a80000.txt`):

```
00AB54D3  mov eax, [esi+8]          ; raw kind
00AB54D6  lea ecx, [eax-1]
00AB54D9  cmp ecx, 23
00AB54E2  movzx edx, [ecx+0xAB56EC]
00AB54E9  jmp [0xAB56C4+edx*4]
00AB54F0  call 00A03C80             ; jt[0] type 4
…
00AB5590  call 00A03D60             ; type 6
```

Jump table `0xAB56C4` and index `0xAB56EC` are recovered in
`type4-dinput-raw` (listing decodes the dwords as `push`/`stosd`):

| `[esi+8]` | idx | Dest | `+40` |
| ---: | ---: | --- | ---: |
| 1 | 0 | `00A03C80` | **4** |
| 4 | 3 | `00A03D60` | **6** |

Who writes raw 4:

`00AB4910` `GetDeviceData`, `dwOfs==12` (`DIMOFS_BUTTON0`):

```
00AB4A72  mov al, [ebx+4]           ; dwData
          and al, 0x80
          neg al
          sbb eax, eax              ; -1 down / 0 up
          and eax, -3
          add eax, 4                ; down=1, up=4
          mov [edi+8], eax
```

Win32 path `00AB4BB0` primary `009A4FC0` edge:

```
dec al; neg al; sbb eax, eax; and eax, 3; inc eax
; press=1, release=4
```

Same raw 4. No DIK. Keyboard type 1 never enters this translator.

---

## 3. `0042E3EE` type 6 → action 28 (**PROVEN**)

`00A03B40` then (`listing-00400000.txt`):

```
0042E479  dec eax                   ; type-1
0042E47A  je 0042E4B0               ; type 1 → last-key + action 33
0042E47C  sub eax, 3                ; type-4
0042E47F  je 0042E4A4               ; type 4 → push 26
0042E481  dec eax
0042E482  dec eax                   ; type-6
0042E483  je 0042E498
0042E498  call 0041E5F2
0042E49D  push 28
0042E49F  jmp 0042E5AB              ; call [input.vtbl+0]
```

`0042E5AB` is `0055CB10` (`FrontendInputMap.ActionApply`). Same
walk as action 26: focused `[+8]` exclusive, else broadcast `+12`
else `+4`. Accept is listener `vtbl+8`; apply is `vtbl+4`.

Type 5 is not dispatched on this arm. Type 10 **event** (RMB down)
is `0042E557` `push 27` — a different physical event from type 6.

---

## 4. Type 10 `0054E280` — action 28 is nothing (**PROVEN**)

`ecx` is widget+4. `lea eax,[ebx-26]`; `cmp eax,8`; index
`00 01 03 03 03 03 03 02 02` at `0x54E33C`; jmp `[0x54E32C+al*4]`.

Table dwords (listing `db`/`loop`/`jecxz` at `0054E32C`):

| i | VA | Actions (via index) | Body |
| ---: | --- | --- | --- |
| 0 | `0054E2FA` | **26** | if `[inner+348]` post `&widget+352` → UI `vtbl+32` |
| 1 | `0054E2B8` | **27** | `00597BF2(0)` |
| 2 | `0054E2C8` | 33–34 | last-key==1 → `00597BF2(1)` |
| 3 | `0054E319` | **28–32** | fall through |

```
0054E319  cmp ebx, 25
          je  skip stamp
          [edi+344] = [edi+44]      ; debounce only
```

Action 28 is index byte `03` → `0054E319`. No `0059A238`. No
`0xE5`. After the switch, 28 ≠ 25 so it only stamps `+344`.

Press Start type-10 `+352` is the attach packet (`00598EE6` /
`0054E4F0`). New Profile / Main Menu type-10 persist `+224` is 0.
None of that is consulted on action 28.

---

## 5. Type 11 `0054DBC0` → `0055AD60` (**PROVEN** gate)

```
0054DBC0  ; debounce +44 vs +400 / +392
          call [outer.vtbl+432]     ; def*
          mov bl, [def+545]
          test bl, bl
          je  0054DC21              ; drop
          push action
          call 0055AD60
```

If parent `+545` is clear, action 28 never reaches the switch.
First-seen `+545` is **UNREAD**. Enable `0054DC30` (parent `+545`)
inserts local map ids **26, 31, 28, 27, 32, 29**. Disable
`0054DCC0` erases the same via `vtbl+16`.

---

## 6. Type 38/11 `0055AD60` jump (actions 26–32) (**PROVEN**)

```
0055AD60  edi = action
          lea eax, [edi-26]
          cmp eax, 6
          ja  0055AE79              ; 0055B9D0 only
          jmp [0x55AE88+eax*4]
```

Table dwords start `7B AD 55 00` = `0055AD7B`. Cases:

| Action | Site | Effect |
| ---: | --- | --- |
| **26** | `0055AD7B` | if `[inner+348]==0` skip. Else `vtbl+584`, `[+364]=1`, `0055B9D0` |
| **27** | `0055ADB2` | if armed `[+364]` and debounce: `vtbl+524([+372])` — **post persist** |
| **28** | `0055ADDE` | if armed: `vtbl+588`, `[+364]=0`. Else timestamp. Then `0055B9D0` |
| 29 | `0055AE01` | hover: `vtbl+592`, `[+384]=1` |
| 30 | `0055AE20` | if `[+384]`: `vtbl+524([+388])` |
| 31 | `0055AE53` | unhover: `vtbl+596`, `[+384]=0` |
| 32 | `0055AE70` | timestamp + `0055B9D0` |

Action 28 body:

```
0055ADDE  mov al, [esi+364]
          test al, al
          je  0055AE70              ; unarmed → stamp only
          lea ecx, [esi-4]
          call [outer.vtbl+588]
          mov [esi+364], 0
          jmp 0055AE70
0055AE70  [esi+396] = [esi+44]
0055AE79  push edi
          call 0055B9D0
```

`0055B9D0` is `cmp arg,25; je vtbl+580; ret 4`. Action 28 does
**not** take that arm. No `E8 0059A238` / `E8 00595582` in
`0055AD60`.

So:

- **Release of accept** = action **27** (`vtbl+524` / persist list
  `+372`: `0x126` / 15). Physical producer of 27 is event type 10
  (RMB down), not type 6.
- **Cancel / unarm** = action **28** (`vtbl+588`). No UI message.
- Type 38 enable `0055AEB0` inserts **26, 31, 27, 32** — **not 28**.
  `0055AF60` (0-arg click shape used as `vtbl+584`) posts then
  `inner.vtbl+12(28)`. Type 38 therefore only locally maps 28
  **after** a successful action-26 click. Exact `vtbl+584` /
  `vtbl+588` VAs stay **UNREAD** (rdata).

Ctor `0055B460` zeros `+364…+392`. First-seen `[+364]=0` → action
28 takes the `je 0055AE70` path even if apply runs.

---

## 7. First-seen screens

`0055CB10` first-seen is broadcast (`[+8]=0` after `0042BE7B`) of
type-33+ inners (`0055BA20`). Type 10 ctor does **not** register.

| Screen | Proven action-28 object | Armed? | Effect of type 6 |
| --- | --- | --- | --- |
| Press Start | type-11 `UI_FRONTEND_BUTTON_INVISIBLE` (persist `0xE5`) | `[+364]=0` | no post; unarm skipped |
| New Profile | type-38 `UI_ACCEPT_NEW_PROFILE` (persist `0x126`) | `[+364]=0`; 28 not in `0055AEB0` set | no apply, or unarm skipped |
| Main Menu | type-11 `UI_FRONTEND_BUTTON_NEW_GAME` (persist 15) + sibling type-11s | `[+364]=0` | no post; unarm skipped |

Type-10 roots: apply is **UNREAD** as a `0055CB10` node first-seen.
If invoked, action 28 is still `0054E319` (nothing). Press Start
`0xE5` remains action **26** / type 4 (`0054E2FA`). New Profile
`0x126` and Main Menu 15 remain action **26** arm + later
`vtbl+524` (26 click body / action 27), not 28.

A later LMB up **after** a type-4 that armed `[+364]` still does
not post. It only calls `vtbl+588` and clears the flag.

---

## 8. C# leftover

| Site | Native | Host |
| --- | --- | --- |
| type 6 → 28 | `0042E49D` | `ActionType6=28` **MATCH** |
| 28 → message | none on 10/11/38 | `MessageFromWidgets` only action 26 **MATCH** as no-op |
| LMB up queued | `00A03D60` | host does not queue type 6 (`host-input-type4`) **MATCH** skip |
| Return | type 1 / 33 | type 1 / DIK 28 — **not** type 6 |

Do **not** map LMB up → action 26. Do **not** treat action 28 as
`0xE5` / `0x126` / 15. Do **not** invent a DIK for type 6.

---

## Sources

- `listing-00a00000.txt` (`00A03D60`)
- `listing-00a80000.txt` (`00AB4910`, `00AB4BB0`, `00AB5420`, `00AB5590`)
- `listing-00400000.txt` (`0042E3EE` / `0042E498`)
- `listing-00540000.txt` (`0054E280`, `0054DBC0`, `0054DC30`, `0055AD60`, `0055AE88`, `0055AEB0`, `0055AF60`, `0055B9D0`, `0055CB10`)
- `tools/Fable.ExeIndex/out/01-sections/text-map/e8.tsv` (`00AB55A8` → `00A03D60`)
- `src/Fable.Game/FrontendInputMap.cs`
- `proofs/type4-dinput-raw/README.md` (raw 4 → type 6)
- `proofs/action26-subscribers/README.md` (arm 26 vs post 27)
