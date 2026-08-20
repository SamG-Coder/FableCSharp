# Type-10 `vtbl+192` `0052CF40` `SelectState(6)` on old current

Investigation only. No production `src/` edits.

Question: Type-10 `vtbl+192` `0052CF40` `SelectState(6)` on
old current in `00596763`. Does arg 6 hide (`+302` bit 0 /
`Visible`) so `00595222` / `00530260` skip Press Start
children after New Profile? What do args 5 and 3 do?

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00500000.txt`
(`0052CF40`–`0052D362` / `00530260` / `0052F1D0` /
`005331A0` `00533288` / `0052C730`);
`listing-00580000.txt` (`00596763` `005967C9` / `0059A119` /
`00595222` / `005952CF`);
`listing-00540000.txt` (`0054E3D0` / `0054CBF0` `0054CD8E`
`0054CF94` / `00547C90`);
`proofs/00596763-switch/README.md`;
`proofs/00599E3F-walk-slots/README.md`;
`proofs/00595222-first-node/README.md`;
`implementer/frontend/14-container.md`.

Do not re-prove persist Type=10 on PRESS_START / NEW_PROFILE,
`00596763` as `[ui+32]` push_back / `+152`/`+156`, or
`[ui+84]` keeping slot `0x14`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN**.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| `00596763` `vtbl+192(6)` on old current? | **Yes.** `edi` = `[ui+32].back()`, `push 6`, `call [eax+192]`. First-seen old back is slot `0x14` Press Start (type 10). | **PROVEN** `005967C3` |
| Is that `0052CF40`? | Shared select body. Type-10 ctor cluster (`0054E3D0`–`0054E4F0`) has **no** `+192` override. Type 12 / 18 **do** replace the slot then `E8`/`call` `0052CF40`. `.rdata` dword `012497E4+192` is past `listing-01200000`. | **PROVEN** body; rdata dword **UNREAD** |
| Arg 6 writes `+302` bit 0 / host `Visible`? | **No.** `0052CF40` never touches `+302`. Whole `.text` `or […+302], 0x01` is only `00533288` (persist `def+392` at attach). | **DISPROVEN** |
| `00595222` / `00530260` skip Press Start children after New Profile because of that? | **No.** Draw walk still calls slot-`0x14` `vtbl+8`. `00530260` skips a child only if `vtbl+420` (`0052F1D0` = `[+302] & 1`). Arg 6 does not set that bit. `00530260` does not read `+332`. | **DISPROVEN** |
| Arg 5? | Same `0052CF40` body: `+332=5`, style list `+316` cleared, `vtbl+540(5)`, child `vtbl+188(5, duration)`. On this path `0059A119` applies it to **new** current `[ui+156]` (New Profile), not Press Start. Type-8 child skip is **not** taken (`+332` is 5, not 1/3/4). | **PROVEN** stores / site; extra `vtbl+560/+564` arm **PARTIAL** |
| Arg 3? | Same body with `+332=3`. **Does** skip type-8 children in the `vtbl+188` walk (`cmp +332, 3`). Not used by `00596763` / `0059A119` on the type-10 roots. Type 12 / 8 / 11 treat 3 as selected/activate around this call. | **PROVEN** skip test; this switch path **DISPROVEN** as the arg-3 site |

---

## Verdict

**Arg 6 does not hide via `+302` bit 0 / `Visible`, and
`00595222` / `00530260` do not drop Press Start children
for that reason.**

`00596763` does call old current `vtbl+192(6)` (`0052CF40`
on type 10). That writes `+332=6` and forwards
`vtbl+188` to own `+176` children. Draw skip in
`00530260` is `vtbl+420` = persist clip bit, not the
style key. Slot `0x14` stays in `[ui+84]` and still
gets `vtbl+8`.

Arg 5 is the **new**-current select on the same tick
(`0059A119`). Arg 3 is a different style key that also
gates type-8 child forward; it is not the New Profile
switch argument.

| Claim | Status |
| --- | --- |
| `005967C9` is old current `vtbl+192(6)` | **PROVEN** |
| First-seen old current is type-10 Press Start | **PROVEN** (`00596763-switch`) |
| `0052CF40` `mov [this+332], arg` | **PROVEN** |
| `0052CF40` `or`/`mov` `+302` | **DISPROVEN** |
| Host `Visible=false` is this native write | **DISPROVEN** |
| `00530260` skip is `vtbl+420` (`[+302]&1`), twice | **PROVEN** |
| `00530260` / `00595222` test `+332` | **DISPROVEN** |
| After switch, `00595222` still walks slot `0x14` | **PROVEN** (`00599E3F-walk-slots`) |
| Arg 6 type-8 child skip in `0052CF40` | **DISPROVEN** (only 1/3/4) |
| Arg 3 type-8 child skip | **PROVEN** |
| `0059A119` is new current `vtbl+192(5)` | **PROVEN** |
| Switch jump-table `vtbl+560` vs `+564` per arg | **PARTIAL** (byte groups 0/6, 1/5, 2/3/4; pointer dwords garbled in listing) |
| Type-10 `.rdata` `012497E4+192 == 0052CF40` | **UNREAD** this listing set |

---

## 1. Call site: old current `SelectState(6)`

`00596763`–`0059686A` `ret 8`. `edi` = `[ui+32].back()`
(`0059B039`). First-seen that widget is slot `0x14`
Press Start.

```
005967C3  mov eax, [edi]
005967C5  push 6
005967C7  mov ecx, edi
005967C9  call [eax+192]
005967CF  call 0041E5F2
          … input vtbl+20 unregister [edi+4] …
00596812  mov [esi+156], [ebp+8]   ; new
00596818  mov [esi+152], edi       ; old
```

Same tick, after `[ui+84]` `vtbl+4` walk
(`0059A0C4`), if `[ui+152] ≠ 0`:

```
0059A0EF  mov ecx, [esi+152]       ; old
          call [vtbl+196]
          … or vtbl+56; [eax+3] gate …
0059A119  mov ecx, [esi+156]       ; new
0059A121  push 5
0059A123  call [eax+192]
          … input vtbl+8 on new inner …
0059A155  mov [esi+152], 0
0059A15B  mov [esi+156], 0
```

Arg 6 = leaving Press Start. Arg 5 = entering New
Profile. Neither is arg 3.

Type-10 ctor `0054E3D0` writes vtbl `012497E4` then
zeros `+352/+356/+360`. Nearby methods: copy, dtor,
`vtbl+172` `0054E4B0`, `vtbl+284` `0054E4F0`. No
`0052CF40` thunk in that cluster. Type 12
`0054CBF0` and type 18 `00547C90` wrap `0052CF40`.

---

## 2. `0052CF40` body — no `+302`

`0052CF40`–`0052D362` `ret 4`. `ebp` = arg.
Early-out if `[this+332] == arg`.

```
0052CF49  cmp [esi+332], ebp
          je  0052D35E
0052CF58  mov [esi+332], ebp
          xor eax, eax
          mov [esi+312], eax
          mov [esi+308], eax
          ; free +316 list, relink sentinel
0052CF93  cmp ebp, 6
          ja  0052CFDD              ; still vtbl+540
          movzx edx, [ebp+0x52D374]
          jmp [0x52D368+edx*4]
0052CFC7  call [vtbl+564]
          jmp 0052CFDD
0052CFD3  call [vtbl+560]
0052CFDD  push ebp
          call [vtbl+540]           ; style record*
          push ebp
          call [vtbl+176]           ; bool
          jne animated
          ; duration +336 ← +320; enqueue +316 node 1
          ; walk +176: parent==this, type≠8 or +332∉{1,3,4}
          ;   child vtbl+188(+332, +336)
```

Animated arm (`vtbl+176` true): `[+328]=arg`, duration
from style `+28` or `+320`, then `0052D740` keys 1/2/3
into `+316`, then the **same** child `vtbl+188` walk
(type-8 skip still 1/3/4 only).

No `esi+302` / `ebx+302` in this range.
`listing-00500000.txt` `+302` writers:

| VA | Op | Role |
| --- | --- | --- |
| `00533288` | `or [ebx+302], 0x01` | persist `def+392` in `005331A0` |
| `00533298` | `or …, 0x02` | centre `def+188` |
| `005336D2` / `00533953` | `mov …, 0` | ctor |
| `0053824F` / `5F` / `6F` | `or 0x08/10/20` | type-6 align |

Whole text-map `or […+302], 0x01`: **only** `00533288`.
`0052CF40` cannot set clip bit 0. Host `Visible` is not
a native field on this path.

---

## 3. Draw skip is not `+332`

`00595222`: `[ui+84]` in-order, `[node+20].vtbl+8`,
no `+302` / `+332` test. After `00596763` the map still
holds slot `0x14`.

Type 10 `vtbl+8` `00530260`:

```
parent = child.vtbl+208
if parent != this && !child.vtbl+400: skip   ; +300 bit 7
if child.vtbl+420: skip                      ; twice
else child.vtbl+8(...)
```

`vtbl+420` `0052F1D0`:

```
0052F1D0  mov al, [ecx+302]
          and eax, 1
          ret
```

Own children (`parent==this`) only test that bit.
`+332` is not loaded. Arg 6 therefore does not make
`00530260` skip Press Start’s `+176` kids.

Type-8 skip inside `0052CF40` (not draw):

```
vtbl+260 == 8 && (+332 == 1 || 3 || 4) → skip vtbl+188
```

Arg 6 does not match. Press Start children are types
5 / 18 / 12 / 6 / 32, not 8.

---

## 4. Args 5 and 3 in `0052CF40`

All args `0…6` share the `+332` store, `+316` clear,
`+324/+344` rotate, `vtbl+540(arg)`, and child
`vtbl+188`. `cmp ebp, 6` / `ja` still reaches
`0052CFDD` for arg `>6`.

Listing-as-data at `0x52D374` (byte table, 7 entries)
groups:

| Arg | Compact index | Used on this New Profile path? |
| ---: | ---: | --- |
| 0, **6** | 0 | **6** on old current |
| 1, **5** | 1 | **5** on new current `0059A119`; also `005952CF` on `[ui+32].back()` |
| 2, **3**, 4 | 2 | **3** not this switch |

Jump pointers at `0x52D368` decode as code in the
listing (`mov edi, 0xCFD30052`), so which group calls
`vtbl+564` (`0052CFC7`) vs `vtbl+560` (`0052CFD3`) vs
neither (`0052CFDD`) is **PARTIAL**. Those slots are
style helpers (`vtbl+540` already looks up the style
record for dest/colour). They are not `+302`.

Arg **5** at `0059A119` is gated: `[ui+152]` non-null,
then old `vtbl+196` true **or** `vtbl+56` with
`[eax+3]==0`. Then new `vtbl+192(5)`, register new
inner (`vtbl+8`), `vtbl+24`, clear `+152/+156`.

Arg **3**: `+332=3` **does** skip type-8 children in
the forward walk. Type 12 `0054CBF0` case 3 registers
list inners and `0052CF40(3)`. Type 8 `0053D3B0` case
3 is selected. Type 11 `0054DC30` is `push 3; call
[vtbl+192]` then inner `+12(26…)`. None of those are
`00596763`.

Type 12 case 5 (`0054CD8E`): `+368` style walk
`vtbl+192(1)` then `0052CF40(5)`. Case 6
(`0054CF94`): unregister, then `0052CF40(0 or 6)`
from `[+324]`. Still no `+302`.

---

## Do not invent

- Arg 6 writing `Visible` or `+302` bit 0.
- `00595222` filtering to `[ui+32]` / `[ui+156]`.
- `00530260` skipping on `+332==6`.
- Lionhead names for args 0–6 (unlabelled style keys).
- `.rdata` `012497E4+192` dword (not in
  `listing-01200000.txt`).
- `vtbl+560/+564` bodies as hide.

**Proposed (do not apply here):** keep slot `0x14`
drawn after `00596763`. Do not map `SelectState(6)` to
`Visible=false`.
