# Leftover #14 — native key/mouse that posts `0xE5` / `0x126` / 15

Investigation only. No production `src/` edits.
Do **not** re-enable `Key.N` / `ActivateNewGame`.

Question: which native key or mouse posts Press Start `0xE5`,
New Profile `0x126`, and New Game message 15?

Authority: `Fable.exe` listings
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
(`0041E6D3` / `0042E3EE`),
`listing-00540000.txt` (`0054E2FA` / `0054E4F0` / `0055AF60` /
`0055ACF0` / `00540320`),
`listing-00580000.txt` (`0059A238` / `00598EE6` / `00595582`),
`listing-00a00000.txt` (`00A03C80` / `00A03D60`),
`listing-00a80000.txt` (`00AB5420` / `00AB4910`);
`proofs/type4-dinput-raw/README.md`,
`proofs/type4-type6-ring/README.md`,
`proofs/type6-action28/README.md`,
`proofs/action28-plus228/README.md`,
`proofs/input-vtbl56-vs-ui32/README.md`,
`proofs/0041E6D3-frontend-gate/README.md`,
`proofs/0054E4F0-store-shape/README.md`,
`proofs/0059A238-first-consumes/README.md`,
`proofs/who-posts-0x126/README.md`,
`proofs/who-posts-15/README.md`,
`proofs/list-type12-focus/README.md`,
`src/Fable.Client/Program.cs`,
`src/Fable.Client/SilkNativeInput.cs`,
`src/Fable.Game/FrontendInputMap.cs`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**STALE** / **LEFTOVER**.

Do not re-prove Leave `0042F2A2` / `FinalAlbion.wld`, persist CRC
`0x53C644E4` values, or dest AABB (#48).

---

## Verdict

**Native key: UNREAD.** Return / Enter / `N` do not post
`0xE5` / `0x126` / 15. `0041E6D3` is not that poster.

**Native mouse: LMB.** Device 3, `DIMOFS_BUTTON0`.

| Device | Event | Record | Action | Posted id (first-seen) |
| --- | --- | ---: | ---: | --- |
| LMB down | `dwOfs=12` + `dwData&0x80` → raw **1** | type **4** `00A03C80` | **26** | Press Start **`0xE5`** (type-10 `+352`) |
| LMB up | same ofs, data clear → raw **4** | type **6** `00A03D60` | **28** | New Profile **`0x126`** / New Game **15** (type-11/38 `+228` list, after arm) |

`0059A238` (UI `012521A8+32`) **consumes** those ids.
`0041E6D3` (input `01230134+56`) is a **second fan-in** that
forwards a live boxed pair to the same `vtbl+32` when
`[0x13B86A0]==0`. Type-4 Press Start never enters it
(`input-vtbl56-vs-ui32`).

`docs/PARITY.md` / `FORWARD_TREE.md` “native key UNREAD
(`0041E6D3` is the consumer)” is **STALE** on the consumer
and on the **mouse**. It remains **MATCH** as unread **key**.

Host leftover #14: client still has no `Key.N` /
`ActivateNewGame`. Enter is type 1 (AVI skip). Clicks exist
(`ClickNamed` / host hit midpoint) but dest is invented
(leave #48). Host Return stays quarantined as accept. Leave
#14 open for dest / Present Notes, not for a missing LMB
translator.

| Claim | Class |
| --- | --- |
| Type 4 is LMB down, device 3 | **PROVEN** `00AB4910` / `00A03C80` |
| Type 6 is LMB up, same device | **PROVEN** `00A03D60` |
| Type 4 → `0055CB10(26)`; type 6 → `0055CB10(28)` | **PROVEN** `0042E3EE` |
| Press Start user post of `0xE5` is type-10 `0054E2FA` → `0059A238` | **PROVEN** |
| Attach store of `0xE5` is `00598EE6` then `0054E4F0` at widget+352 (packet*) | **PROVEN** |
| `0x126` / 15 have no `.text` `mov […], imm` | **PROVEN** |
| `0x126` is persist on type-38 `UI_ACCEPT_NEW_PROFILE` | **PROVEN** |
| 15 is persist on type-11 `UI_FRONTEND_BUTTON_NEW_GAME` | **PROVEN** |
| Type 4 arms type 11/38; type 6 posts `+228` if armed | **PROVEN** site (`action28-plus228`) |
| Return (DIK 28) posts `0xE5` / `0x126` / 15 | **DISPROVEN** (type 1 / action 33) |
| `Key.N` posts New Game 15 | **DISPROVEN** (no native site; host cheat removed) |
| Type-12 `005403D2` / `005405C9` first-seen posts those ids | **DISPROVEN** (empty circular lists on Press Start) |
| `0041E6D3` is the Press Start `0xE5` consumer | **DISPROVEN** — that is `0054E2FA` → `0059A238` |
| Physical **key** that posts `0xE5` / `0x126` / 15 | **UNREAD** |
| Pad button that also builds type 4 / 6 | **UNREAD** |
| Live LMB down+up in one `GetDeviceData` | **UNREAD** (`type4-type6-ring`) |

**Answer:** native poster is **LMB**, not a key. Native **key**
stays **UNREAD**.

---

## 1. Consumer `0059A238` (not `0041E6D3`)

`listing-00580000.txt`:

```
0059A238  push ebp
0059A281  mov eax, [ebp+8]      ; pair*
0059A284  mov eax, [eax]        ; boxed*
0059A286  mov ecx, [eax]        ; id
…
0059A2C5  je 0059A2DA           ; 15 → [esi+28].vtbl+16; [esi+41]=1
…
0059A6BE  sub ecx, 0xE5
0059A6C4  je 0059A77F           ; 0xE5 → 00599D5C
…
0059A6DE  dec ecx
0059A6DF  jne 0059A7FF
0059A6E5  mov esi, [esi+96]     ; 0x126
0059A6F2  call 00851920
```

UI vtbl `012521A8+32` = `012521C8` = `0059A238`
(`FrontendMessages.UiMessageFn`). Host
`DispatchFrontendMessage` is this switch.

`0041E6D3` (`listing-00400000.txt`) is input `vtbl+56`:

```
0041E6D3  push ebp
0041E6E6  mov edi, [ebp+124]    ; pair*
0041E6EE  mov al, [eax+12]
0041E6F5  je 00426DFC           ; dead packet: no UI
0041E6FB  mov esi, [0x13B86A0]
0041E703  jne 0041E718          ; game live: skip UI hop
0041E705  call 00595582
0041E70F  call [edx+32]         ; 0059A238
0041E718  … id switch (cmp eax, 0xD8) …
```

First-seen frontend `[0x13B86A0]==0`, so **if** this function
is entered with a live packet it forwards to `0059A238`. Type 4
does not enter it (`0042E3EE` is `call [edx]` = `0055CB10`,
never +56).

---

## 2. `0054E4F0` stores the Press Start pair; `0054E2FA` posts it

Attach (`listing-00580000.txt`):

```
00598EE6  mov [eax], 0xE5       ; packet[0]
          … slot 0x14 …
00598F06  call [eax+284]        ; type-10 0054E4F0
```

Store (`listing-00540000.txt`):

```
0054E4F0  mov eax, [esp+4]      ; wrapper {packet*, ctrl*}
0054E4F5  mov ebx, [eax]
0054E4F9  mov edi, [eax+4]
0054E4FC  mov esi, ecx          ; widget
0054E530  mov [esi+352], ebx    ; packet*
0054E536  mov [esi+356], edi    ; ctrl*
```

**Not** `mov [widget+352], 0xE5`. Action 26 posts `&+352`:

```
0054E2FA  mov eax, [edi+348]    ; inner this: widget+352
0054E303  lea esi, [edi+348]
0054E309  je 0054E318
0054E30B  call 00595582
0054E315  call [edx+32]         ; 0059A238(&widget+352)
```

That is the recovered Press Start **user** post of attach
`0xE5`. Type-10 Main Menu / New Profile roots do **not** store
`0x126` or 15 at +352.

---

## 3. Type 4 / type 6 ring — mouse, not a key

Ctor (`listing-00a00000.txt`):

```
00A03C80  mov [ecx+32], 0x3     ; device 3
          mov [ecx+40], 0x4     ; type 4
          ret 12

00A03D60  mov [ecx+32], 0x3
          mov [ecx+40], 0x6     ; type 6
          ret 20
```

`00AB5420`: raw 1 → `00A03C80`; raw 4 → `00A03D60`.
`00AB4910` `GetDeviceData` `dwOfs==12` (`DIMOFS_BUTTON0`):
down `dwData&0x80` → raw 1; up → raw 4. Keyboard type 1 never
enters this translator (`type4-dinput-raw`).

Same 52-byte store, 256×52 mouse array, mux harvest. Not a
wrapping ring. `009F4ED0` copies slot 0 only; `009F4F10`
walks the rest and skips type 0 only. One poll can dequeue
**4 then 6** (`type4-type6-ring`). Live down+up in one
`GetDeviceData` stays **UNREAD**.

`0042E3EE` (`listing-00400000.txt`):

```
0042E456  call 00A03B40         ; [rec+40]
0042E47C  sub eax, 3
0042E47F  je 0042E4A4           ; type 4 → push 26
0042E483  je 0042E498           ; type 6 → push 28
0042E4A4  call 0041E5F2
0042E4A9  push 26
0042E4AB  jmp 0042E5AB          ; call [edx] = 0055CB10
0042E498  call 0041E5F2
0042E49D  push 28
```

Type 1 (any key, including DIK 28 Return) is `push 33` into
the same `vtbl+0`. Not 26, not 28.

---

## 4. Who posts each id after the mouse event

```
0042E3EE
  type 4 → 0055CB10(26)
    type-10 0054E280 → 0054E2FA → 0059A238(0xE5)     ; Press Start
    type-11/38 0055AD60 → vtbl+584 0055AF60
      arm [+364]=1; post +372 / [def+224] (first-seen empty)
  type 6 → 0055CB10(28)
    type-10 case 3: no UI message
    type-11/38 armed → vtbl+588 / 0055ACF0
      post +380 / [def+228] → 0x126 or 15
```

| Screen | Widget | Persist | User post |
| --- | --- | ---: | --- |
| Press Start | type-10 attach packet | `00598EE6` `0xE5` | type **4** / 26 / `0054E2FA` |
| New Profile | type-38 `UI_ACCEPT_NEW_PROFILE` | `0x53C644E4` → **`0x126`** | type **4** arm + type **6** / 28 / `+228` |
| Main Menu | type-11 `UI_FRONTEND_BUTTON_NEW_GAME` | same CRC → **15** | type **4** arm + type **6** / 28 / `+228` |

`0059A238` dests first-seen: `0xE5` → `00599D5C`; `0x126` →
`00851920`; 15 → `0059A2DA` / `[retail+41]=1`
(`0059A238-first-consumes`). No other dest on that walk.

---

## 5. Keyboard paths that do **not** post

| Event | Native | Result |
| --- | --- | --- |
| Return DIK 28 | type 1 / action 33 | last-key / `00597BF2`; **DISPROVEN** as `0xE5`/`0x126`/15 |
| Type-12 list `cmp edi, 1` / `28` | `005403D2` / `005405C9` → `0041E6D3` | Press Start circular `+352`/`+348` empty first-seen |
| Host Enter | `SilkNativeInput.QueueKeys` type 1 / PlayAVI skip | **MATCH** type 1; **DISPROVEN** as New Game |
| Host `Key.N` | **absent** in `Fable.Client` | do not restore |

`FrontendInputMap.DikPosterUnread = false` means type 4 is
**not** a DIK, not that a keyboard poster was found.
`MessageFromAction(ActionFromKey, *)` is null.

---

## 6. Host leftover (do not “fix” with N/Enter)

```
src/Fable.Client/SilkNativeInput.cs
  Enter → TypeKey (skip)
  LMB edge → Type4
  LMB-up → Type6
  no Key.N, no ActivateNewGame

EngineLifecycle.MaybeActivateNewGameFromInput
  action 26 → ArmType34Widgets + MessageFromWidgets (type-10 +352)
  action 28 → MessageFromPlus228List + Unarm
  DispatchFrontendMessage(msg)            ; 0059A238 analog
```

Clicks exist. Dest/hit for New Profile chrome is still invented
(#48). Host Return remains quarantined as accept. Present
`0042DF9E` still Note-only. Leave #14 open on those leftovers,
not on an unread LMB device.

---

## Classification (VAs)

| VA | Role | Class |
| --- | --- | --- |
| `00A03C80` | type 4 ctor, device 3 | **PROVEN** LMB down |
| `00A03D60` | type 6 ctor, device 3 | **PROVEN** LMB up |
| `00AB5420` / `00AB4910` | raw 1/4 translator | **PROVEN** |
| `0042E3EE` | type 4→26, type 6→28, type 1→33 | **PROVEN** |
| `0055CB10` | apply `vtbl+0` | **PROVEN** |
| `0054E4F0` | type-10 store packet* at +352 | **PROVEN** |
| `0054E2FA` | type-10 action 26 → `0059A238` | **PROVEN** `0xE5` |
| `00598EE6` | only `.text` `mov […], 0xE5` | **PROVEN** attach |
| `0055AF60` | type-11/38 action 26 arm / `+372` | **PROVEN** |
| `0055ACF0` | type-11/38 action 28 / `+380` | **PROVEN** site |
| `0059A238` | UI vtbl+32 consumer | **PROVEN** |
| `0041E6D3` | input vtbl+56 fan-in | **PROVEN** hop; **DISPROVEN** type-4 poster/consumer |
| DIK 28 / `Key.N` | keyboard New Game | **DISPROVEN** |
| Some other DIK | posts `0xE5`/`0x126`/15 | **UNREAD** |

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00540000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00580000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00a00000.txt`
- `C:\FableCSharp\proofs\type4-dinput-raw\README.md`
- `C:\FableCSharp\proofs\type4-type6-ring\README.md`
- `C:\FableCSharp\proofs\input-vtbl56-vs-ui32\README.md`
- `C:\FableCSharp\proofs\0041E6D3-frontend-gate\README.md`
- `C:\FableCSharp\proofs\action28-plus228\README.md`
- `C:\FableCSharp\src\Fable.Client\SilkNativeInput.cs`
