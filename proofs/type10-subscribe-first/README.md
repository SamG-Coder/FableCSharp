# Press Start type-10 is not a first-seen `0055CB10` subscriber

Investigation only. No production `src/` edits.

Authority: dump `Fable.exe` `0054E3D0` / `0054E280` / `0054E190` /
`0055CB10` / `0055CF00` / `0055CE90` / `0055CF50` / `0055BA20` /
`0055C650` / `005407B0` in
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`;
`0041D21B` / `0041D512` in `listing-00400000.txt`;
`0052CC50` / `005334A0` / `005331A0` / `0053B63E` in
`listing-00500000.txt` + `implementer/frontend/fn-0052CC50-exact.txt`
/ `fn-005334A0-exact.txt` / `fn-005331A0-exact.txt`;
`e8.tsv` (`0054E3D0`, `0055BA20`, `0055C650`, `005407B0`;
**no** `0054E280` / **no** `0055CF00`);
`implementer/frontend/persist-scan.txt` `#620`;
`proofs/action26-subscribers/README.md`;
`proofs/type10-plus352/README.md`;
`proofs/type10-no-0055B040/README.md`;
`proofs/invisible-button-e5/README.md`;
`FrontendUiDefTests.Press_Start_is_type_10_with_text_child`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**LEFTOVER**.

Do not re-prove type 4 → `push 26`, Return ≠ `0xE5` / `0x126` / 15,
`0059A238` consume (`0xE5` → `00599D5C`), type-10 `+352` packet
layout, or the `0055CB10` walk shape (broadcast when `[input+8]==0`).

---

## Verdict

**No.** First-seen Press Start type-10
(`UI_FRONTEND_PRESS_START_MENU`) is **not** a `0055CB10` node.
Ctor `0054E3D0` never calls `0041E5F2` + input `vtbl+8`. **Nobody
adds it later on this path.** Action 26 therefore **never** reaches
inner `0054E280` first-seen.

Who *does* land on the list during that ctor is the descendants
that register themselves (DFS post-order, inside `005331A0`):

1. **`UI_FRONTEND_BUTTON_INVISIBLE`** type 11 — `0055BA20`
2. **`UI_MOUSE_POINTER`** type 32 — `0055C650` (local map **25**)

| Claim | Status |
| --- | --- |
| Type-10 factory ctor is `0041D512` → `0054E3D0` (size `0x16C`) | **PROVEN** |
| `0054E3D0` / copy `0054E410` call input `vtbl+8` | **DISPROVEN** (14 insns; no `0041E5F2`) |
| Inherited `0052CC50` / `005334A0` register the type-10 inner | **DISPROVEN** |
| Type 12 / 18 / 6 / 5 ctors on this tree register | **DISPROVEN** |
| Live insert is `input.vtbl+8(inner)`; unregister `vtbl+20` | **PROVEN** sites |
| Append primitive `0055CF00` / copy `0055CE90` insert **before** sentinel | **PROVEN** shape |
| `.text` `E8 0055CF00` | **DISPROVEN** (`e8.tsv` empty) |
| `.text` `E8 0054E280` | **DISPROVEN** (vtbl only) |
| First-seen `0055CB10` is the **broadcast** arm (`[input+8]==0`) | **PROVEN** (`action26-subscribers`) |
| Broadcast only calls `listener.vtbl+4` for nodes already on `+12`/`+4` | **PROVEN** |
| Type-10 inner `012497BC+4` = `0054E280`; accept `+8` = `0054E190` (`al=1`) | **PROVEN** (`type10-plus352`) |
| First-seen action 26 reaches `0054E280` | **DISPROVEN** |
| Who adds the type-10 inner first-seen | **nobody** |
| First Press Start subscriber | **`UI_FRONTEND_BUTTON_INVISIBLE`** |
| Second Press Start subscriber | **`UI_MOUSE_POINTER`** |
| Type-11 action 26 posts attach `0xE5` | **DISPROVEN** (`invisible-button-e5`) |
| Input `01230134+8` body **is** `0055CF00` | **PARTIAL** (no `.rdata` dword) |
| A later (non-first-seen) writer of type-10 onto the list | **UNREAD** (no ctor/attach/activate site recovered) |

`invisible-button-e5` “type-10 is a `0055CB10` node first-seen =
**UNREAD**” and `05-input.md` “type-10 subscribe-set **PARTIAL**”
are closed for **first-seen Press Start**: the set does **not**
include type 10.

---

## Answer

Ctor has no register. **No other first-seen site adds the type-10
inner.** Children add **themselves**. Without a list node,
`0055CB10(26)` never calls `012497BC+4`, so action 26 never
enters `0054E280` / `0054E2FA`. Attach `00598EE6` → `0054E4F0`
still writes packet* `0xE5` at widget+352; that store is idle
until something later puts this inner on the list.

---

## 1. Dump `0054E3D0` (entire function)

Factory `0041D21B` type 10:

```
0041D4FC  push 0x16C
0041D501  call 00BFEA1A
0041D510  mov ecx, eax
0041D512  call 0054E3D0
```

Sole `.text` `E8 0054E3D0` is that site (`e8.tsv`).

```
0054E3D0  mov eax, [esp+4]       ; def
0054E3D4  push esi
0054E3D5  push eax
0054E3D6  mov esi, ecx
0054E3D8  call 0052CC50          ; type 5 → type 4 → 005331A0 children
0054E3DD  xor eax, eax
0054E3DF  mov [esi], 0x12497E4   ; widget vtbl
0054E3E5  mov [esi+4], 0x12497BC ; inner (0054E280 this)
0054E3EC  mov [esi+24], 0x12497B4
0054E3F3  mov [esi+352], eax     ; packet* = 0
0054E3F9  mov [esi+356], eax
0054E3FF  mov [esi+360], eax
0054E405  mov eax, esi
0054E407  pop esi
0054E408  ret 4
```

No `0041E5F2`. No `call [edx+8]`. Copy-ctor `0054E410` is the same
shape via `0052CCA0`. Dtor `0054E450` clears `+352/+356` and
`jmp 0052CCF0`. Attach store `0054E4F0` writes the packet* only
(`type10-plus352`).

`0052CC50` (18 insns): `005334A0`, vtbl `01245DE4` / inner
`01245DBC`, alloc list at `+316`. **No** input register.
`005334A0` ends in `005331A0` (child walk). Type-10 then
**overrides** both vtbls, so the type-5 inner is not live.

---

## 2. Dump `0055CB10` insert / walk (why no node ⇒ no `0054E280`)

`0042E3EE` type 4 `push 26` → `0042E5AB` `call [edx]` =
input `vtbl+0` = `0055CB10` (`action26-subscribers`).

```
0055CB10  mov esi, ecx              ; input
          mov eax, [esi+8]
          test eax, eax
          je   0055CB3F              ; first-seen: +8 is 0

; focused: one listener, then ret 4  (not first-seen)

0055CB3F  mov ecx, [esi+12]         ; preferred sentinel
          cmp [ecx], ecx
          je   0055CB90              ; empty → walk +4
          call 0055CF50              ; snapshot
0055CB64  mov ecx, [esi+8]          ; listener*
          call [eax+8]               ; accept
          test al, al
          je   skip
          call [edx+4]               ; apply  ← only path to 0054E280
          mov esi, [esi]             ; next; no return
```

`0055CF50` builds a new 12-byte sentinel and `0055CE90` copies
the live circular list (`node+8` preserved). Copy insert is
**before dest sentinel** (`0055CEB9`–`0055CEC3`).

Append primitive (same cluster; **no** `E8` site):

```
0055CF00  esi = [ecx]               ; dest sentinel
          alloc 12
          [node+8] = *arg           ; listener
          insert before sentinel    ; push_back
```

Register sites recovered first-seen:

```
0055BA20  (type 33; 11/34/38 go through here)
  call 0052CC50                     ; children first
  lea edi, [esi+4]
  call 0041E5F2
  push edi
  call [edx+8]                      ; input.vtbl+8(inner)

0055C650  (type 32)
  call 0041B800
  lea edi, [esi+4]
  call 0041E5F2
  push edi
  call [edx+8]
  0052DA20(25)

005407B0  (type 37 — not on Press Start)
  same vtbl+8(inner); 0052DA20(33/34)
```

`e8.tsv`: `0041D2B2`/`0055B468`/`00558EC8` → `0055BA20`;
`0041D292` → `0055C650`; `0041D79C` → `005407B0`.

Inner `vtbl+12(26)` is local map `0052DA20`, **not** this list
(`action26-subscribers`). Type-11 activate `0054DC30` `push 26`
does not create a `0055CB10` node.

---

## 3. `0054E280` is vtbl-only

```
0054E280  mov edi, ecx              ; inner = widget+4
          lea eax, [ebx-26]
          cmp eax, 8
          jmp [0x54E32C+eax*4]
0054E2FA  mov eax, [edi+348]        ; widget+352
          test eax, eax
          je  skip
          push esi                  ; &widget+352
          call [edx+32]             ; 0059A238
```

`e8.tsv` has **zero** `0054E280` rows. The only recovered caller
shape is `0055CB10` `call [listener.vtbl+4]`. Accept slot
`0054E190` is `mov al,1; ret` — always true **if** this inner
were a node. It is not first-seen, so the always-true accept is
dead.

`0041E5F2` inside `0054E280` at `0054E2C8` is a **different**
action arm (`vtbl+52`), not register.

Later type-10 `0041E5F2` uses (`0054E64A` dest scale, `0054E78B`
draw) read input fields / `vtbl+84` / `+88` / `+144`. They do
**not** `push widget+4` / `call [edx+8]`. `0054E8F6` /
`0054E953` `call [edx+8]` are **child** vtbls (5-arg), not input
register.

---

## 4. First-seen Press Start tree — who actually registers

`005331A0` walks `[def+112]..[def+116]` and `0041D21B` each child
during the **parent** ctor. Combined with §2, register order is
DFS **post-order** of types 11 / 32 / 33 / 34 / 37 / 38.

`persist-scan.txt` `#620` `UI_FRONTEND_PRESS_START_MENU` Type=10,
`Children=6`:

| # | Name | Type | Ctor | `vtbl+8`? |
| --- | --- | ---: | --- | --- |
| 1 | `UI_BLENDING_BACKGROUNDS_FORREST` | 5 | `0052CC50` | no |
| 2 | `UI_TITLE` | 5 | `0052CC50` | no |
| 3 | `UI_PRESS_START_SWAP` | 18 | `00547600` → `0052CC50` + zeros | no |
|  | → `UI_PRESS_START_TEXT` | 6 | `0054F5C0` → `0052CC50` | no |
| 4 | `UI_FRONTEND_LIST_PRESS_START_MENU` | 12 | `0054C3A0` → `0053B63E` (10 insns) | no |
|  | → **`UI_FRONTEND_BUTTON_INVISIBLE`** | **11** | `0054E0B0` → `0055B460` → **`0055BA20`** | **yes** |
| 5 | `UI_LEGAL_TEXT` | 6 | `0054F5C0` | no |
| 6 | **`UI_MOUSE_POINTER`** | **32** | **`0055C650`** | **yes** (map 25) |

No type 38 / 37 / 33 on this screen. Type-10 root returns **after**
those children have already registered. The root itself never
calls `vtbl+8`.

Type-8/12 helper `0053B321` can `0041E5F2` + `vtbl+8` on a
**list child** inner during later highlight work. That is not the
type-10 widget, and it is not the first-seen ctor path.

---

## 5. C# leftover

`FrontendInputMap.MessageFromWidgets` still prefers the first
visible type-10 with `MessageId ≠ 0` (host attach patch `0xE5`).
Native first-seen:

- does **not** deliver 26 to that type-10;
- delivers 26 to INVISIBLE (`0054DBC0`) and then the mouse
  (map 25);
- INVISIBLE action 26 does **not** post `0xE5`
  (`invisible-button-e5`).

Install `Type4_drives_lifecycle_0xE5_…` **MATCH**es the **id**
via the attach analog, not via `0054E280`. Keep
`AttachFrontendTree`. Do not treat type-10 as a recovered
`0055CB10` listener.

---

## Do not invent

- Type-10 ctor / copy / attach / dest-scale as a register site.
- Inner `vtbl+12(26)` / `0052DA20` as the `0055CB10` subscribe.
- Type 12 / 18 / 5 / 6 as first-seen list nodes.
- First-seen `0054E280` because accept is `mov al,1`.
- Dropping the PRESS_START attach analog because INVISIBLE
  persist is `0xE5`.
- Input `+8` focused listener as first-seen (ctor zeros it).
- A Lionhead name for CRC `0x53C644E4`.
