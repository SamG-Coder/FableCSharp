# Type-2 table row dest (`005517E0` / `00550DC0` / `00551EA0`)

Investigation only. No production `src/` edits.

Question: New Profile two type-16 sliders share dest
`448,245` because parents
`UI_OPTIONS_TEXT_SLIDER_WHOLE_CONTROL_METHOD` and
`UI_OPTIONS_TEXT_SLIDER_WHOLE_CAMERA_UP_DOWN` both
layout to `-96,240`. Camera sensitivity correctly
lands at `y=352`. Who writes type-2 table **row dest**
so those rows get different Y?

Authority: dump only.
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
(`005517E0` / `00551860` / `00550C60` / `00550DC0` /
`00551340` / `005514F0` / `005518E0` / `00551BC0` /
`00551EA0`);
`listing-00500000.txt` (`005339B0` / `0052C730` /
`0052FFD0` / `00531EC0` / `00530260`);
`listing-00400000.txt` (`0041D432` factory type 2);
`e8.tsv` (`005517E0` ← `0041D432`; `00551340` ←
`0055184B` / `005518CB`; **no** `E8 00550DC0` /
`E8 005518E0` / `E8 00551BC0` / `E8 00551EA0`);
`functions.tsv`;
`out/00-index/rtti.txt` (`CTable@NUISystem`
`0x0137C2EC`);
`export/frontend/new-profile-dests.txt`;
`implementer/frontend/02-layout.md`;
`implementer/frontend/14-container.md`;
`proofs/type6-plus204-writer`;
`proofs/type16-slider-row-dest`.

Do not invent dest width. Type-6 leftover `+204` as
dest width is **DISPROVEN**. Do not run ExeIndex
`fn` / `vtbl`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

---

## Verdict

**Row dest is not the type-2 ctor and is not
`vtbl+8`.** Persist instances (the dumped WHOLE /
type-16 names) get dest from `005339B0` child
inherit → `0052FFD0` `+248/+252` using layout
`+8/+12` (persist `PositionX`/`PositionY`). Both
WHOLE rows feed the **same** `PositionY` (inferred
**0**). Sensitivity is a different persist Y
(inferred **70**).

The writer that *can* give clones a different row
Y is **`00551EA0`** (axis helpers `005518E0` X /
`00551BC0` Y): it stores cursor into the clone’s
layout record `+8/+12`, then the clone’s
`vtbl+172` re-enters `005339B0`. Host
`LayoutFrontendWidgets` never calls that path.
Type-2 `LEFT`/`RIGHT` on New Profile are **columns
at the same Y as their WHOLE**, not a parent that
stacks the two sliders.

| Claim | Class |
| --- | --- |
| Type 2 ctor `005517E0` vtbl `0124A224` size `0x170` | **PROVEN** |
| Factory `0041D21B` type 2 is `push 0x170` / `call 005517E0` | **PROVEN** `0041D41C` |
| RTTI `CTable@NUISystem` `0x0137C2EC` | **PROVEN** |
| Ctor writes dest `+248/+252` | **DISPROVEN** |
| Ctor leftover `+204/+208` = persist W/H (`def+92/+88`) via `00551340` | **PROVEN** |
| Type 2 `vtbl+8` body is `00550DC0` (`ret 20`, then `00530260`) | **PROVEN** body; rdata dword **PARTIAL** |
| `00550DC0` writes **this** table `+248/+252`, not sibling WHOLE Y | **PROVEN** |
| Type 2 `vtbl+172` body is `00550C60` (`0052C730` then `vtbl+568`) | **PROVEN** body; slot **PARTIAL** |
| `005339B0` copies layout0 `+8/+12` → widget `+52/+56` and recurses child `vtbl+172` | **PROVEN** |
| Persist dest origin is `0052FFD0` / `00531EC0` from `+52/+56` + parent | **PROVEN** |
| Cell / row Position writer is `00551EA0` layout `+8/+12` | **PROVEN** stores |
| `005518E0` accumulates X; `00551BC0` accumulates Y; both `ret 28` | **PROVEN** |
| `.text` `E8 00550DC0` / `E8 005518E0` / `E8 00551BC0` / `E8 00551EA0` | **DISPROVEN** (empty `e8.tsv`) |
| Host runs `00551EA0` | **DISPROVEN** |
| Both WHOLE dest `-96,240`; sensitivity WHOLE `-96,352` | **PROVEN** dump |
| Sequential `frontend.bin` `PositionY` on those WHOLE defs | **UNREAD** this pass |
| Inferred persist Y: WHOLE sliders `0`, sensitivity `70` (640-space × `1.6` + list `64,240`) | **PARTIAL** |
| Type-2 `LEFT`/`RIGHT` dest Y is the row stacker for the two sliders | **DISPROVEN** |
| Type-16 widgets are type-2 cell clones | **DISPROVEN** as dumped names (persist children of type-5 WHOLE) |
| Type-6 leftover `+204` is dest width | **DISPROVEN** |

**Answer:** type-2 **row dest** is `00551EA0`
writing clone layout `+12`, then `005339B0` /
`0052FFD0`. The overlapping sliders are **not**
those clones. Their dest Y is persist WHOLE
`PositionY` through the same inherit. Do not
invent a leftover width to split them.

---

## 1. Symptom

`export/frontend/new-profile-dests.txt`:

```
UI_NEW_PROFILE_MENU                              t=12  dest=64,240
UI_NEW_PROFILE_BUTTON                            t=11  dest=64,240
UI_OPTIONS_TEXT_SLIDER_WHOLE_CONTROL_METHOD      t=5   dest=-96,240
  UI_OPTIONS_TEXT_SLIDER_BUTTON_TABLES           t=5   dest=-96,240
    UI_BUTTON_OPTIONS_LEFT                       t=2   dest=-96,240,192,240
    UI_BUTTON_OPTIONS_RIGHT                      t=2   dest=-96,240,96,240
    UI_OPTIONS_CONTROL_METHOD_TEXT_SLIDER        t=16  dest=448,245
UI_OPTIONS_TEXT_SLIDER_WHOLE_CAMERA_UP_DOWN      t=5   dest=-96,240
    UI_OPTIONS_CAMERA_UP_DOWN_TEXT_SLIDER        t=16  dest=448,245
UI_OPTIONS_SLIDER_WHOLE_CAMERA_SENSITIVITY       t=5   dest=-96,352
```

Type-16 dest is a **point**. Type-2 dest X1 ≠ X0
is persist Width × dest scale (288 / 192 after
`1.6`), written into leftover `+204` by
`00551340` — **not** a type-16 / type-6 dest
width.

---

## 2. Type 2 ctor `005517E0` — not dest

Factory (`listing-00400000`):

```
0041D41C  push 0x170
0041D421  call 00BFEA1A
0041D42F  push edi            ; CUIDef*
0041D432  call 005517E0
```

`e8.tsv` has **one** `.text` site for
`005517E0`: `0041D432`. Copy ctor `00551860`
is the twin (`0052CCA0` then same tail).

`005517E0` (`listing-00540000`):

```
005517E9  call 0052CC50              ; type 5/base → 005331A0 persist Children
005517EE  mov [esi], 0x124A224       ; vtbl
005517F4  mov [esi+4], 0x124A1FC
005517FB  mov [esi+24], 0x124A1F4
00551806  push 28
          call 00BFEA0E
          mov [esi+348], eax         ; cell-template tree
00551845  mov [esi+360], ebx
0055184B  call 00551340
```

No `mov` / `fstp` of `+248/+252`.

`00551340` (only `E8` from `0055184B` /
`005518CB`):

```
call [eax+432]                   ; CUIDef*
mov [esi+204], [def+92]          ; persist Width
mov [esi+208], [def+88]          ; persist Height
alloc +360 from def+124
alloc +364 from def+136
walk [def+100] list:
  0041E5F2 / 0041D21B            ; named cell
  vtbl+172
```

That leftover store is **type 2 only**. It is
why `UI_TABLE_TITLE_WHOLE` dest W is `1024` and
`LEFT` dest W is `288`. It is **not**
`0041AC20` GraphicIndex leftover and is **not**
type-6 dest width.

---

## 3. `005339B0` child dest inherit

Type-2 layout `00550C60` starts with
`call 0052C730`. `0052C730`
(`listing-00500000`):

```
0052C733  call 005339B0
          +324/+328/+332=0 …
```

`005339B0`:

```
lea ebx, [esi+36]                ; layout-state map
0042D5B1(key=0)
mov [esi+52], [eax+8]            ; persist PositionX
mov [esi+56], [eax+12]           ; persist PositionY
… scale +92/+96 from layout +16 …
if +280==0:
  +76/+80/+256/+260=0
  +272/+276=1.0
walk +176:
  if child vtbl+208==0:
    vtbl+204(this)
    child vtbl+172               ; inherit recurse
```

Dest origin is later `0052FFD0` /
`00531EC0`:

```
+248 = persistPos * inheritScale + parent +256
```

(`02-layout.md`. Remap via `0052E580` when
`+302` bit 7.) First-seen type-16 leftover
`+204` is 0 → `0041AFA0` dest is a **point**
at that origin.

So persist WHOLE / type-16 dest Y is whatever
`PositionY` `005339B0` copied. No type-2
row pitch in this walk.

---

## 4. Type 2 `vtbl+8` `00550DC0` — table dest only

No `.text` `E8`. Signature is `ret 20` (same
five dwords as `00530260`). Unique large
method on the `0124A224` page. Slot dword is
**PARTIAL** (`.rdata` `0124A224+8` is past
`listing-01200000.txt`).

```
00550DD3  call [eax+400]             ; borrowed-visible
00550DF3  fld [esi+56] / [esi+52]
          fadd parent.vtbl+100
          → +84/+88
00550E36  mov [esi+248], ecx         ; this table dest X
00550E44  mov [esi+252], edx         ; this table dest Y
… dest scale +264 from parent vtbl+476 …
walk +176: colour / vtbl+480 combine
00551329  call 00530260              ; draw children
00551334  ret 20
```

This rewrites **the table’s** dest at draw. It
adds persist `+52/+56` to the parent, not a
running row cursor onto sibling type-5 WHOLEs.

`e8.tsv` `00530260` in this listing: this
site is the type-2 draw tail (`type38-on-off-first`).

---

## 5. Type 2 `vtbl+172` `00550C60` then `vtbl+568`

```
00550C60  cmp [esi+360], 0
          je ret
          cmp [esi+364], 0
          je ret
00550C81  call 0052C730              ; persist dest on table + current +176
00550C8A  call [eax+568]             ; refill / place cells
```

`005514F0` (size / refill helper) rebuilds
`+360/+364`, walks `def+100` into `+348`,
**wipes `+176`**, then:

```
00551701  call [edx+568]
```

`00551EA0` is the 0-arg thiscall on that
page that clones `+348` templates. No
`.text` `E8` — vtbl / `00551701` only.
Identity of `+568` as `00551EA0` is
**PARTIAL** (no rdata dword); the store
pattern is **PROVEN** in `00551EA0`.

---

## 6. Who writes row dest: `00551EA0` / `005518E0` / `00551BC0`

`00551EA0` (`listing-00540000`):

```
call [eax+432]                   ; def
if [def+96] bit 0:
  cell.vtbl+92.x → count
  fdivr [ebx+204]                ; leftover W budget
if [def+96] bit 1:
  cell.vtbl+92.y → count
  fdivr [ebx+208]                ; leftover H budget
walk +348 templates:
  0041D21B clone
  clone.vtbl+172
  0042D5B1(key=0)
  mov [eax+8],  cursor.x         ; layout PositionX
  mov [eax+12], cursor.y         ; layout PositionY
  append clone to +176
  cursor.x += cell.vtbl+92.x
  cursor.y += cell.vtbl+92.y
```

That `[layout+12]` store **is** the row dest.
Next `005339B0` on the clone copies it to
`+56` → `+252`.

`005518E0` (`ret 28`): same layout write;
loop `ebp+28` times; `fadd [eax]` after
template `vtbl+92` (**X**). `00551BC0`: twin;
`fadd [eax+4]` (**Y**); also writes scale
into layout `+20`.

Those three functions are the only recovered
writers of **different** type-2 cell Y.

---

## 7. Persist `PositionY` on the WHOLE widgets

No sequential `frontend.bin` dump of
`UI_OPTIONS_TEXT_SLIDER_WHOLE_CONTROL_METHOD`,
`UI_OPTIONS_TEXT_SLIDER_WHOLE_CAMERA_UP_DOWN`,
or `UI_OPTIONS_SLIDER_WHOLE_CAMERA_SENSITIVITY`
this pass (**UNREAD** raw CRC).

Host dest is `FrontendLayout.Compute` =
`005339B0` persist math only. New Profile
numbers match authored × `1024/640` plus the
type-12 list origin `64,240`:

```
WHOLE dest Y     = persistY * 1.6 + 240
-96,240          → persistY = 0      (both slider WHOLEs)
-96,352          → persistY = 70     (sensitivity WHOLE)
type-16 448,245  → persist (340, 3) * 1.6 + (-96, 240)
```

The **relative** fact is **PROVEN** from the
dump + inherit formula: both slider WHOLE
`PositionY` values that `005339B0` reads are
the same; sensitivity’s is `+70` authored
units. A later `frontend.bin` walk should
lock the raw floats / `Absolute` /
`ScaleOrigin` bits; do not invent a host
`+70` row guess on the type-16 widgets.

Type-2 `LEFT`/`RIGHT` sit at the **same** dest
Y as their WHOLE (`240` or `352`). They are
column tables inside one row
(`UI_OPTIONS_TEXT_SLIDER_BUTTON_TABLES`), not
the parent of the two WHOLEs.

---

## 8. Type-6 leftover width — **DISPROVEN**

`proofs/type6-plus204-writer` /
`type6-plus204-writers`: type-6 ctor
`0054F5C0` never stores `+204`. Draw
`0054EF00` **reads** `[esi+204]` for
centre/right pen only. First-seen New
Profile type-6 `g=0` `+204=0` dest is a
point. `00551340` leftover is type **2**.
Do not feed dest W back as type-6 `+204`
to fake a second slider row.

---

## 9. Classification

| Object | Native dest writer | Host |
| --- | --- | --- |
| Type-5 WHOLE persist instance | `005339B0` / `0052FFD0` from persist Y | **MATCH** → both `240` |
| Type-16 persist instance | same, parent dest + persist (3) | **MATCH** → both `245` |
| Type-2 table persist instance | same + leftover W from `00551340` | **MATCH** columns |
| Type-2 cell clone | `00551EA0` layout `+12` then `005339B0` | **LEFTOVER** (never constructed) |
| Type-6 `ARROWS` / `NORMAL` | parent type-16 origin (point) | **MATCH** stack |

Splitting the two type-16 points requires
different persist `PositionY` on the WHOLE
rows (not what dest math shows) or running
`00551EA0` on a table whose cells **are**
those rows. The dumped New Profile tree is
the first case.

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00540000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00500000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\functions.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\00-index\rtti.txt`
- `C:\FableCSharp\export\frontend\new-profile-dests.txt`
- `C:\FableCSharp\proofs\type6-plus204-writer\README.md`
- `C:\FableCSharp\proofs\type16-slider-row-dest\README.md`
