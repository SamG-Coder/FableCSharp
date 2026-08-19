# Who `0055CB10`-subscribes type-10 Press Start?

Investigation only. No production `src/` edits.

Authority: dump `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
(`0054E3D0` / `0054E0B0` / `0055B460` / `0055BA20` / `0055C650` /
`0055CB10` / `0055CF00`),
`listing-00580000.txt` (`00598A1C`…`00599D15` / `00598EE6` /
`0059A144`),
`listing-00500000.txt` (`0052CC50`),
`listing-00400000.txt` (`0041D512` / `0042EA62`),
`e8.tsv` / `functions.tsv`;
`implementer/frontend/persist-scan.txt` `#620`;
`proofs/type10-subscribe-first/README.md`;
`proofs/action26-subscribers/README.md`;
`proofs/00598A1C-only-e5/README.md`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Do not re-prove type 4 → `push 26`, Return ≠ `0xE5`,
`0059A238` consume, type-10 `+352` packet layout, or the
`0055CB10` broadcast-vs-focused walk shape.

---

## Verdict

**Nobody** puts the Press Start type-10 inner on the
`0055CB10` list. Ctor `0054E3D0` has no register.
`00598A1C` after factory does **not** add one either.

`0055CB10` only walks nodes already on `input+12` / `+4`.
The nodes that exist first-seen are children who register
**themselves** inside their own ctors (DFS post-order during
`005331A0` under `0054E3D0` → `0052CC50`):

1. **`UI_FRONTEND_BUTTON_INVISIBLE`** type 11 — `0054E0B0` →
   `0055B460` → `0055BA20` `input.vtbl+8(widget+4)`
2. **`UI_MOUSE_POINTER`** type 32 — `0055C650` same
   `vtbl+8` (local map **25**)

`00598A1C` after `0041DB1D` only `mov [slot], widget*` and,
later, writes packet `0xE5` via slot `0x14` `vtbl+284`
(`0054E4F0` → `+352`). That is attach, not subscribe.

| Claim | Status |
| --- | --- |
| Type-10 factory is `0041D512` → `0054E3D0` (size `0x16C`) | **PROVEN** |
| `0054E3D0` / copy `0054E410` call `0041E5F2` + `vtbl+8` | **DISPROVEN** (14 insns) |
| Inherited `0052CC50` registers the type-10 inner | **DISPROVEN** |
| `00598A1C` after factory registers type-10 | **DISPROVEN** |
| `0041E5F2` inside `00598A1C` is the subscribe shape | **DISPROVEN** (factory getter) |
| `00598A1C` callee list includes `0055BA20` / `0055C650` / `0055CF00` | **DISPROVEN** (`functions.tsv`) |
| `00598A1C`…`00599D15` has `call [edx+8]` / `call [eax+8]` | **DISPROVEN** (`listing-00580000`) |
| `00598EE6` is subscribe | **DISPROVEN** (packet `0xE5` + `vtbl+284`) |
| `0055CB10` inserts a node | **DISPROVEN** (walk only) |
| First-seen Press Start `0055CB10` nodes | **INVISIBLE** then **MOUSE** |
| Later tick `0059A144` registers type-10 Press Start | **DISPROVEN** (`[ui+156]+4`, not slot `0x14`) |
| A later (non-first-seen) writer of **this** type-10 inner | **UNREAD** |

---

## Answer

Ctor has no register. **`00598A1C` after factory still has no
register.** Type-10 Press Start is not a `0055CB10` subscriber.
Children subscribe themselves. Action 26 therefore never reaches
inner `0054E280` first-seen.

---

## 1. Dump `0054E3D0` — ctor has no register

Factory `0041D21B` type 10 (`listing-00400000.txt`):

```
0041D4FC  push 0x16C
0041D501  call 00BFEA1A
0041D510  mov ecx, eax
0041D512  call 0054E3D0
```

Sole `.text` `E8 0054E3D0` is that site (`e8.tsv` `0041D512`).

Entire function (`listing-00540000.txt`):

```
0054E3D0  mov eax, [esp+4]       ; def
0054E3D4  push esi
0054E3D5  push eax
0054E3D6  mov esi, ecx
0054E3D8  call 0052CC50          ; type 5 → 005334A0 → 005331A0 children
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

No `0041E5F2`. No `push widget+4`. No `call [edx+8]`. Copy
`0054E410` is the same shape via `0052CCA0`.

`0052CC50` (18 insns) only sets type-5 vtbls and allocs the
`+316` list. Type-10 then **overrides** both vtbls:

```
0052CC58  call 005334A0
0052CC5D  mov [esi], 0x1245DE4
0052CC63  mov [esi+4], 0x1245DBC
0052CC71  push 12
0052CC7D  call 00BFEA0E
0052CC87  mov [esi+316], eax
0052CC93  ret 4
```

Contrast the live register (`0055BA20`, type 33 / 11 / 34 / 38):

```
0055BA29  call 0052CC50
0055BA30  lea edi, [esi+4]
0055BA5E  call 0041E5F2
0055BA63  mov edx, [eax]
0055BA65  push edi
0055BA68  call [edx+8]           ; input.vtbl+8(inner)
```

Type-10 never does that pair.

---

## 2. Dump `00598A1C` — after factory is slot store, not subscribe

First-seen populate (`0042EA62` after `005958F5`, arg 0):

```
00598BA2  push "UI_FRONTEND_PRESS_START_MENU"
00598BB7  mov [ebp+108], 0x14
00598BBE  call 0059B5D7          ; slot 0x14 cell
00598BCB  call 0041E5F2          ; factory context (name already pushed)
00598BD0  mov ecx, eax
00598BD2  call 0041DB1D          ; → 0041D21B → 0054E3D0
00598BD7  mov ecx, [ebp+108]
00598BDA  mov [ecx], eax         ; *slot = widget*
00598BDC  lea ecx, [ebp+112]
00598BDF  call 0099EAE0          ; free name
00598BE4  push edi
00598BE5  push "UI_FRONTEND_PROFILES_MENU"
```

Immediately the next menu. No `lea …,[widget+4]`. No
`input.vtbl+8`.

`0041E5F2` here is **not** the subscribe helper. Subscribe
is `0041E5F2` → `push inner` → `call [edx+8]`. Factory is
`push 0` / `push &name` → `0041E5F2` → `0041DB1D`. Every
`0041E5F2` in `00598A1C` is the latter (`functions.tsv`:
paired with `0041DB1D`, never with a register primitive).

`functions.tsv` `0x00598A1C` (1667 bytes, `00598A1C`…
`00599D15` `ret 4`) callees include `0059B5D7` / `0041E5F2` /
`0041DB1D` / `00BFEA1A` / `0042BE50` / `0042AA29`. **No**
`0055BA20`, **no** `0055C650`, **no** `0055CF00`, **no**
`005407B0`.

`listing-00580000.txt` `call [edx+8]` / `call [eax+8]` in
this listing: last before the fn is `00595247`; first after
is `0059A144`. **Zero** inside `00598A1C`.

Later in the **same** function, after other slot factories:

```
00598EC3  push 16
00598EC5  call 00BFEA1A
00598ED1  call 0042BE50
00598EDE  call 0042AA29
00598EE6  mov [eax], 0xE5        ; packet[0]
00598EF2  mov [ebp+108], 0x14
00598EF9  call 0059B5D7
00598F06  call [eax+284]         ; type-10 0054E4F0 → +352
```

That stores the attach packet. It does not insert
`012497BC` onto the input list.

---

## 3. Dump `0055CB10` — walk only; no insert

```
0055CB10  mov esi, ecx              ; input
          mov eax, [esi+8]
          test eax, eax
          je   0055CB3F              ; first-seen: +8 is 0

; focused: accept then apply one listener*, ret 4

0055CB3F  mov ecx, [esi+12]
          cmp [ecx], ecx
          je   0055CB90              ; empty → walk +4
          call 0055CF50              ; snapshot
0055CB64  mov ecx, [esi+8]          ; listener* = [node+8]
          call [eax+8]               ; accept
          test al, al
          je   skip
          call [edx+4]               ; apply  ← only path to 0054E280
          mov esi, [esi]             ; next; no return
```

No alloc. No `0055CF00`. If the type-10 inner is not already
a `node+8`, `012497BC+4` (`0054E280`) is never called.

Live insert is `input.vtbl+8(inner)` at the ctor sites in §4.
Append primitive `0055CF00` has **no** `.text` `E8`
(`e8.tsv` empty). Identity of `01230134+8` as that append
stays **PARTIAL**; the Press Start type-10 still never calls
it.

---

## 4. Who *does* subscribe on this screen

`005331A0` factories `[def+112]..[def+116]` during the
**parent** ctor. Register order is DFS post-order of types
11 / 32 / 33 / 34 / 37 / 38.

`persist-scan.txt` `#620` `UI_FRONTEND_PRESS_START_MENU`
Type=10, `Children=6`:

| # | Name | Type | Ctor | `vtbl+8`? |
| --- | --- | ---: | --- | --- |
| 1 | `UI_BLENDING_BACKGROUNDS_FORREST` | 5 | `0052CC50` | no |
| 2 | `UI_TITLE` | 5 | `0052CC50` | no |
| 3 | `UI_PRESS_START_SWAP` | 18 | `00547600` → `0052CC50` | no |
|  | → `UI_PRESS_START_TEXT` | 6 | `0054F5C0` | no |
| 4 | `UI_FRONTEND_LIST_PRESS_START_MENU` | 12 | `0054C3A0` | no |
|  | → **`UI_FRONTEND_BUTTON_INVISIBLE`** | **11** | `0054E0B0` → `0055B460` → **`0055BA20`** | **yes** |
| 5 | `UI_LEGAL_TEXT` | 6 | `0054F5C0` | no |
| 6 | **`UI_MOUSE_POINTER`** | **32** | **`0055C650`** | **yes** (map 25) |

Type 11:

```
0054E0B8  call 0055B460          ; → 0055BA20 register, then override vtbl
```

Type 32:

```
0055C659  call 0041B800
0055C65E  lea edi, [esi+4]
0055C674  call 0041E5F2
0055C67E  call [edx+8]
0055C681  push 25
0055C685  call 0052DA20
```

Those two `vtbl+8` calls run **inside** `00598BD2`
`0041DB1D`, before `00598BDA` stores the root. The root
returns from `0054E3D0` without a third call.

`0059A144` (`0041E5F2` + `push edi` + `call [edx+8]`) is
**after** `00598A1C` (`00599E3F` tick). `edi` is
`[ui+156]+4`, not slot `0x14` Press Start. Do not treat it
as a late type-10 subscribe.

---

## Do not invent

- `00598A1C` `0041E5F2` as `input.vtbl+8` of the type-10.
- `00598EE6` / `vtbl+284` as a `0055CB10` insert.
- Type 12 / 18 / 5 / 6 as first-seen list nodes.
- First-seen `0054E280` because accept `0054E190` is
  `mov al,1`.
- `0055CB10` itself adding the missing node.
- Dropping the Press Start attach analog because INVISIBLE
  persist is `0xE5`.
