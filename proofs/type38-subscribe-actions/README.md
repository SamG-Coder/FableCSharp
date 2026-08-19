# Type 38 `00558B90` does not subscribe 26/28/27; enable is type-34 `0055AEB0`

Investigation only. No production `src/` edits.

Authority: dump `Fable.exe` `00558B90` / `0055AEB0` /
`0055AEF0` / `0055AF30` / `0055AF60` / `0055B460` /
`0055BA20` / `0055BAE0` / `0054DC30` / `0055A5B0` /
`00557860` / `00557880` in
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`;
factory `0041D2BC` / `0041D35C` in `listing-00400000.txt`;
inner insert `0052DA20` in `listing-00500000.txt`;
`FrontendWidgetType` type 34 ctor `0055B460`, type 38
`00558B90` / vtbl `0124B04C`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN**.

Do not re-prove type 4 → `push 26`, type 6 → 28, type 10
(RMB) → 27, persist `0x126` / 15, or the `0055CB10` walk.

---

## Verdict

**No.** Type-38 ctor `00558B90` does **not** local-map
26 / 28 / 27. Enable `0055AEB0` maps **26, 31, 27, 32** —
**not 28**. That body is the **same** type-34-family enable
type 35/39 wrap; type 38 has no private copy.

| Claim | Status |
| --- | --- |
| Factory type 38 is `0041D35C` alloc `0x194` → `00558B90` | **PROVEN** |
| Factory type 34 is `0041D2BC` alloc `0x194` → `0055B460` | **PROVEN** |
| `00558B90` is `call 0055B460` then three vtbl stores, `ret 4` | **PROVEN** |
| `00558B90` `push 26` / `28` / `27` | **DISPROVEN** |
| Inherited ctor `0055B460` / `0055BA20` local-maps 26/28/27 | **DISPROVEN** |
| `0055BA20` does register inner on input (`vtbl+8`) | **PROVEN** (listener list, not action ids) |
| Enable body is `0055AEB0`: inner `vtbl+12(26, 31, 27, 32)` | **PROVEN** |
| `0055AEB0` also maps **28** | **DISPROVEN** |
| `0055AEB0` is a type-38-only function | **DISPROVEN** |
| Type 34 ctor `0055B460` `E8 0055AEB0` | **DISPROVEN** |
| Type 35 `0055A5B0` / type 39 `00557860`/`880` wrap `0055AEB0` | **PROVEN** |
| Type 11 activate `0054DC30` maps 26, 31, **28**, 27, 32, 29 | **PROVEN** (gated on `[def+545]`) |
| 28 on type 38 is inserted later by click `0055AF60` | **PROVEN** site; first-seen hit **UNREAD** (gate `+352`) |
| Inner `vtbl+12` body is local-map insert `0052DA20` | **PROVEN** shape; rdata dword **PARTIAL** |
| `.rdata` `0124BD2C` / `0124B04C` enable slot == `0055AEB0` | **PARTIAL** (no vtbl dump) |

**Subscribe 26/28/27 on construct?** No.

**Same `0055AEB0` as type 34?** Yes — shared type-34 enable,
not a type-38 rewrite. Slot dwords still **PARTIAL**.

---

## Answer

Two different “subscribe”s:

1. **Input list** (`0055CB10` node). Type 38 inherits
   `0055BA20` via `0055B460`. That is `input.vtbl+8(inner)`
   at **ctor**. The widget can see action 26 as a listener
   from birth. It is **not** an action-id map.
2. **Local action map** (inner `vtbl+12` = `0052DA20`).
   Type 38 ctor never inserts 26 / 28 / 27. Enable
   `0055AEB0` inserts **26 / 31 / 27 / 32**. 28 is
   **absent** until click `0055AF60` `vtbl+12(28)`.

Type 11 is the 26/28/27 (plus 31/32/29) pattern, and only
on activate if `[def+545]`. Do not copy that set onto
Accept.

---

## 1. Dump `00558B90` (type 38 ctor)

`listing-00400000.txt` factory:

```
0041D2BC  push 0x194
          call 00BFEA1A
          call 0055B460              ; type 34
0041D35C  push 0x194
          call 00BFEA1A
          call 00558B90              ; type 38
```

Same alloc size. Type 38 is the next slot after type 36,
not an alias of type 34.

`listing-00540000.txt`:

```
00558B90  mov eax, [esp+4]           ; def
00558B94  push esi
00558B95  push eax
00558B96  mov esi, ecx
00558B98  call 0055B460              ; type 34 ctor
00558B9D  mov [esi],     0x124B04C   ; outer
00558BA3  mov [esi+4],   0x124B024   ; inner
00558BAA  mov [esi+24],  0x124B01C
00558BB1  mov eax, esi
00558BB3  pop esi
00558BB4  ret 4
```

Fourteen instructions. **No** `push 26` / `28` / `27`.
**No** `E8 0055AEB0`. Copy ctor `00558BC0` is the same
shape with `0055B4C0`. Dtor thunk `00558BF0` /
`00558D30` only restore those three vtbls and
`jmp 0055B760`.

The only other type-38 bodies in this pad are select
`00558C70` (may `inner.vtbl+12(25)`), walk `00558C10`,
and action-30 thunk `00558D90`. None map 26/28/27.

---

## 2. Dump type 34 ctor `0055B460` (what type 38 actually runs)

```
0055B460  mov eax, [esp+4]
0055B468  call 0055BA20              ; type 33
0055B471  mov [esi],     0x124BD2C   ; type 34 outer (live during persist)
0055B477  mov [esi+4],   0x124BD08
0055B47E  mov [esi+24],  0x124BD00
          xor eax, eax
          [esi+364] = 0
          [esi+368] = 0
          [esi+372] … [esi+392] = 0
0055B4B5  call 0055B040              ; persist copy
          ret 4
```

`0055BA20`:

```
0055BA29  call 0052CC50              ; type 5 + children
          [esi]    = 0124BFB4        ; type 33, overwritten later
          [esi+4]  = 0124BF90
          [esi+24] = 0124BF88
          +348/+352/+356/+360 = 0
          call 0041E5F2
          input.vtbl+8(inner)        ; 0055CB10 list
          ret 4
```

Ctor chain: type 38 → 34 → 33 → 5 → children, then
**register inner**, persist-copy under **type-34** vtbl,
then type 38 overwrites to `0124B04C`. Local-map insert
of 26/28/27 is not in this chain.

---

## 3. Dump `0055AEB0` (enable) — 26/31/27/32, not 28

0-arg (`ret`). `ecx` = **outer** widget.

```
0055AEB0  push esi
0055AEB1  mov esi, ecx
0055AEB3  call 0055BAE0              ; copy +332 → +348; vtbl+192([def+516]); post +356
0055AEB8  mov eax, [esi+4]
0055AEBB  add esi, 4                 ; inner
0055AEBE  push 26
          call [eax+12]
0055AEC7  push 31
          call [edx+12]
0055AED0  push 27
          call [eax+12]
0055AED9  push 32
          call [edx+12]
0055AEE1  ret
```

Disable pair `0055AEF0` erases the **same four** via
inner `vtbl+16`. Still no 28.

`0055BAE0` is visual / SelectState / `vtbl+524([+356])`.
It does **not** `push` an action id.

Inner `vtbl+12` insert (`listing-00500000.txt`):

```
0052DA20  ; ecx = inner, arg = action
          lea esi, [edi+4]           ; map at inner+4
          call 0052DF20              ; find
          call 0052E230              ; insert if missing
          cmp arg, 25
          jne  ret 4                 ; 26/27/31/32: insert only
          ; 25 also call inner.vtbl+4
```

So `0055AEB0` **arms the local map** for LMB-down (26),
RMB-down (27), and 31/32. It does **not** arm LMB-up (28).

---

## 4. Same function as type 34?

**Yes, as a shared body.** Evidence:

- `0055AEB0` sits in the type-34 cluster immediately after
  shared apply `0055AD60` (used by type 11 inner
  `0054DBC0` and type 38). Type 38’s own pad at
  `00558B90` has **no** enable clone.
- Type 34 ctor does **not** call it. Enable is a later
  0-arg vtbl method, same as type 11 `0054DC30`.
- `.text` `E8 0055AEB0` (listing): only

  | Site | Owner | Extra after the call |
  | --- | --- | --- |
  | `0055A5B0` | type **35** (ctor `0055A9C0` → `0055B460`) | `jmp [input+184].vtbl+596` |
  | `00557860` | type **39** family | `vtbl+192(3)` |
  | `00557880` | same | `vtbl+192(4)` |

  Those are **wrappers** of the type-34 enable, not a
  second implementation. Type 38 ctor is the same
  inheritance shape as type 35 (`call 0055B460` then
  overwrite vtbl) **without** wrapping `0055AEB0`.

- Type 11 is the **other** set: `0054DC30` if
  `[def+545]` → `vtbl+192(3)` then
  `vtbl+12(26, 31, 28, 27, 32, 29)`. That is **not**
  `0055AEB0`.

Exact rdata:

| Table | Role | Enable dword |
| --- | --- | --- |
| `0124BD2C` | type 34 outer (`0055B471`) | **UNREAD** (want `0055AEB0`) |
| `0124B04C` | type 38 outer (`00558B9D`) | **UNREAD** (want `0055AEB0`) |
| `0124BA94` | type 35 (`0055A9CD`) | **UNREAD** (want `0055A5B0`) |

Dump: `Fable.ExeIndex vtbl 0x0124BD2C 160`,
`vtbl 0x0124B04C 160`. Until those dwords print, identity
is **body + wrappers**, not the slot index.

---

## 5. Where 28 actually appears on type 38

Click `0055AF60` (action 26, outer `vtbl+584` family):

```
0055AFAC  push [esi+372]
          call [this.vtbl+524]       ; post +224 list
0055AFC3  push 28
          call [inner.vtbl+12]       ; insert 28
          ret
```

Deactivate `0055AF30` erases 28 and 29 (`vtbl+16`).
`0055ACF0` also `vtbl+16(28)` then posts `+380`.

First-seen Accept: ctor `[+364]=0` and `[widget+352]=0`.
Action 26 skips `0055AF60` until selection
(`type11-plus352-select`). So first-seen type 38 has
**no** local 28 from enable **and** none from click
until that gate is 1.

Type 11 may already have 28 from `0054DC30`. Type 38
does not.

---

## 6. Contrast table

| Who | When | Inner `vtbl+12` ids |
| --- | --- | --- |
| Type 38 ctor `00558B90` | construct | **none** |
| Type 34 ctor `0055B460` | construct | **none** |
| Type 38/34 enable `0055AEB0` | vtbl, later | **26, 31, 27, 32** |
| Type 38/34 disable `0055AEF0` | vtbl | same erase (`+16`) |
| Type 11 activate `0054DC30` | if `[def+545]` | 26, 31, **28**, 27, 32, **29** |
| Type 38 click `0055AF60` | after selected 26 | **28** |
| Type 39 `00557B6C` | other pad | 33, 26, 27, 35, 38… (not this widget) |

Hypothesized ctor set **26/28/27** matches **neither**
type 38 enable **nor** type 11 activate (type 11 also
has 31/32/29; enable omits 28/29 and adds 31/32).

---

## Sources

- `tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
  (`0041D2BC`, `0041D35C`)
- `listing-00540000.txt` (`00558B90`, `0055B460`,
  `0055BA20`, `0055AEB0`, `0055AEF0`, `0055AF30`,
  `0055AF60`, `0055BAE0`, `0054DC30`, `0055A5B0`,
  `0055A9C0`, `00557860`)
- `listing-00500000.txt` (`0052DA20`)
- `src/Fable.Formats/Defs/FrontendWidgetType.cs`
- `proofs/action26-subscribers/README.md`
- `proofs/action28-after-26/README.md`
- `proofs/type11-msg15/README.md`
- `proofs/type38-msg126/README.md`
