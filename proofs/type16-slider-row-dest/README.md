# Type-16 / table-row dest writer (New Profile sliders)

Investigation only. No production `src/` edits.

Question: on New Profile, who writes the type-16
`CTextSlider` dest so
`UI_OPTIONS_CONTROL_METHOD_TEXT_SLIDER` and
`UI_OPTIONS_CAMERA_UP_DOWN_TEXT_SLIDER` do **not**
share `y`? Host dump has both at dest `448,245`
(children `ARROWS` / `NORMAL` both `544,245`).
Parents
`UI_OPTIONS_TEXT_SLIDER_WHOLE_CONTROL_METHOD` and
`UI_OPTIONS_TEXT_SLIDER_WHOLE_CAMERA_UP_DOWN` both
layout to dest `-96,240`. Camera sensitivity
correctly lands at `y=352`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Authority: `Fable.exe`
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00500000.txt`
(`005339B0` / `0052C730` / `0052FFD0` / `00530260` /
`00531EC0`);
`listing-00540000.txt` (`005517E0` / `00550C60` /
`00550DC0` / `00551340` / `005514F0` / `005518E0` /
`00551BC0` / `00551EA0` / `00549F60` / `00549B20` /
`00549230` / `0054D660` / `0054EF00`);
`listing-00400000.txt` (`0041AFA0` / `0041D21B`);
`out/00-index/rtti.txt` (`CTable@NUISystem` /
`CTextSlider@NUISystem`);
`export/frontend/new-profile-dests.txt`;
`src/Fable.Formats/Defs/FrontendWidgetType.cs`;
`src/Fable.Game/FrontendLayout.cs`;
`implementer/frontend/02-layout.md`;
`implementer/frontend/14-container.md`;
`proofs/type6-plus204-writer`;
`proofs/type16-18-present-child`;
`proofs/frontend-screens-vs-native`.

Do not invent dest width. Type-6 leftover `+204` as
dest width is **DISPROVEN** (`0054EF00` dest is a
**POINT**). Do not edit `src/`.

---

## Verdict

**Type-16 dest origin is `005339B0` → `0052FFD0`,
not type-6 leftover and not type-2 dest width.**

Type 16 `vtbl+172` is `00549230`: `call 0052C730`
then extras. `0052C730` is `call 005339B0`.
`005339B0` copies layout-state 0 `+8/+12`
(persist `PositionX`/`PositionY`) into widget
`+52/+56`. `0052FFD0` / the `0053017B` tail writes
dest `+248/+252` from `+52/+56` (remap /
Absolute / parent `+256`). First-seen type-16
leftover `+204` stays 0 → `0041AFA0` dest is a
point at that origin.

Type 2 `CTable` ctor is `005517E0` (vtbl
`0124A224`). **Table-cell dest is not the ctor
and is not `vtbl+8`.** Cell Position is written
into the clone’s layout record `+8/+12` by
`00551EA0` (and the axis helpers `005518E0` /
`00551BC0`), then the clone’s `vtbl+172` runs
the same `005339B0` path.

Host `LayoutFrontendWidgets` only runs the
`005339B0` persist-math analog. It never runs
`00551EA0`. Both WHOLE rows therefore keep
persist `PositionY` (inferred **0**) and both
type-16 points stay `y=245`. Sensitivity
`y=352` is persist `PositionY` **70** on a
different WHOLE (`-96,352`). That split is
**MATCH** persist, not a table-row write.

| Claim | Class |
| --- | --- |
| Type 16 ctor `00549F60` vtbl `01248A8C` | **PROVEN** |
| Type 16 `vtbl+8` is `00530260` (child walk) | **PROVEN** (`type16-18-present-child`) |
| Type 16 `vtbl+172` is `00549230` → `0052C730` → `005339B0` | **PROVEN** |
| Type-16 dest origin writer is `005339B0` / `0052FFD0` from persist pos + parent dest | **PROVEN** |
| Type-6 leftover `+204` is dest width / `0054EF00` writes dest W | **DISPROVEN** |
| Host New Profile type-16 dest is a point `448,245` | **PROVEN** (`new-profile-dests.txt`) |
| Both WHOLE parents dest `-96,240`; sensitivity WHOLE `-96,352` | **PROVEN** dump |
| Type 2 ctor `005517E0` vtbl `0124A224` size `0x170` | **PROVEN** |
| Type 2 leftover `+204/+208` = persist W/H (`def+92/+88`) in `00551340` | **PROVEN** |
| Type 2 `vtbl+172` body is `00550C60` (`0052C730` then `vtbl+568`) | **PROVEN** body; slot **PARTIAL** (no rdata dump this pass) |
| Type 2 `vtbl+8` body is `00550DC0` (`ret 20`, writes table `+248`, then `00530260`) | **PROVEN** body; slot **PARTIAL** |
| Table-cell Position writer is `00551EA0` layout `+8/+12` (cursor += cell `vtbl+92`) | **PROVEN** stores; `vtbl+568` identity **PARTIAL** |
| `005518E0` places along X; `00551BC0` along Y; both `ret 28` | **PROVEN** |
| Host runs `00551EA0` / `005518E0` / `00551BC0` | **DISPROVEN** |
| Persist `PositionY` raw on the two WHOLE / type-16 defs | **UNREAD** (no `frontend.bin` sequential dump this pass) |
| Inferred persist Y: WHOLE sliders `0`, sensitivity `70` (640-space, parent list `64,240`, scale `1.6`) | **PARTIAL** |
| Persist Absolute / `+300` bit 6 on those defs | **UNREAD** |
| New Profile type-16 widgets are type-2 cell clones | **DISPROVEN** as the dumped names — they are persist children of type-5 WHOLE |
| Type 2 LEFT/RIGHT dest Y is the row stacker for the two sliders | **DISPROVEN** — those tables are columns at the same Y as the WHOLE |
| Type 12 `0054D660` writes child dest Y after `0052C730` | **DISPROVEN** — it `0052C730`s first, then may spawn extras from `def+308/+304` |

**Answer:** the type-16 dest dword pair is written by
`005339B0` / `0052FFD0`. The writer that can give
**different row Y** to table clones is `00551EA0`
(layout `+12`). Host never calls it. The two New
Profile sliders share `y` because both persist
WHOLE rows feed the same `PositionY` into
`005339B0`. Do not invent a dest width to split
them.

---

## 1. Symptom (host dest dump)

`export/frontend/new-profile-dests.txt`:

```
UI_NEW_PROFILE_MENU                              t=12  dest=64,240,64,240
UI_OPTIONS_TEXT_SLIDER_WHOLE_CONTROL_METHOD      t=5   dest=-96,240,-96,240
  UI_OPTIONS_CONTROL_METHOD_TEXT_SLIDER          t=16  dest=448,245,448,245
  UI_OPTIONS_TEXT_CONTROL_ARROWS                 t=6   dest=544,245,544,245
  UI_OPTIONS_TEXT_CONTROL_WASD                   t=6   dest=544,245,544,245
UI_OPTIONS_TEXT_SLIDER_WHOLE_CAMERA_UP_DOWN      t=5   dest=-96,240,-96,240
  UI_OPTIONS_CAMERA_UP_DOWN_TEXT_SLIDER          t=16  dest=448,245,448,245
  UI_TEXT_NORMAL                                 t=6   dest=544,245,544,245
  UI_TEXT_INVERTED                               t=6   dest=544,245,544,245
UI_OPTIONS_SLIDER_WHOLE_CAMERA_SENSITIVITY       t=5   dest=-96,352,-96,352
  UI_SLIDER_CAMERA_SENSITIVITY                   t=15  dest=448,360,474,386
```

Type-16 dest is a **point**. Type-6 children inherit
that Y. CPU blit stacks `"Arrows"` + `"Normal"`
because both points are `544,245`. That is dest,
not Vulkan (`frontend-screens-vs-native`).

`+204=0` on every type-6 / type-16 row in that
dump. Type-2 columns **do** carry leftover width
(`LEFT` `-96,240,192,240` → W `288`; title table
`0,0,1024,0` → W `1024`). That is type-2 persist
Width × dest scale, **not** a type-16 dest width.

---

## 2. Type-16 dest path (**PROVEN**)

Ctor `00549F60` (`listing-00540000`):

```
00549F68  call 0052CC50
00549F6F  mov [esi], 0x1248A8C
… zeros +364…+412 …
00549FC7  call 00549B20          ; +348=0 (selected child)
```

`00549230` (`vtbl+172`):

```
00549239  call 0052C730          ; → 005339B0
; optional +400 / +396 attach + their vtbl+172
; then +176[+348] vtbl+192(3)
```

`0052C730` (`listing-00500000`): `call 005339B0`
then first-seen `+324/+328/+332=0`.

`005339B0`:

```
lea ebx, [esi+36]                ; layout map
0042D5B1(key=0)
mov [esi+52], [eax+8]            ; persist PositionX
mov [esi+56], [eax+12]           ; persist PositionY
… scale +92/+96 from +16 …
if +280==0: +272/+276=1; +76/+80/+256/+260=0
walk +176: if child vtbl+208==0:
  vtbl+204(this); child vtbl+172
```

Dest origin (`0052FFD0` falls through to
`0053017B` when no lerp):

```
+84/+88 = +52/+56
if remapOrigin (vtbl+468): 0052E580(+52/+56) → +248
else: +248 = +52/+56
```

Parent combine is `00531EC0` / `0052FFD0` when
`!Absolute` (`vtbl+408` = `+300` bit 6 from persist
`AbsoluteCrc` / def `+191`):

```
+248 = persistPos * +272 + parent +256
```

`0041AFA0` size: `+360` else leftover `+204`, then
`* +264`. Type-16 GraphicIndex 0 and no
`00551340` → leftover 0 → dest is a **point** at
`+248`. Same shape as type-6 `0054EF00`
(`type6-plus204-writer`). **Do not invent dest
width.**

Collapsed first-seen (flag 0, scale 1, viewport
`1024×768` with per-widget remap bits on — the
New Profile numbers match `pos * 1024/640`):

```
list origin     = persist(40,150) → 64,240     ; or persist(64,240) if no remap
WHOLE origin    = persist(-100,0) * 1.6 + 64,240 → -96,240
type-16 origin  = persist(340,3)  * 1.6 + -96,240 → 448,245
sensitivity     = persist(-100,70)* 1.6 + 64,240 → -96,352
```

The `1.6` / persist integers are **PARTIAL**
(back-solved from dest + `0052E580`). The
**relative** fact is **PROVEN**: both WHOLE
`PositionY` values that `005339B0` reads are the
same; sensitivity’s is `+70` authored units.

---

## 3. Type 2: ctor vs `vtbl+8` vs cell dest

### 3.1 Ctor `005517E0` — not dest

```
005517E9  call 0052CC50
005517EE  mov [esi], 0x124A224
00551806  mov [esi+348], alloc(28)   ; cell-template tree
0055184B  call 00551340
```

RTTI `CTable@NUISystem` `0x0137C2EC`.

`00551340`:

```
vtbl+432 → persist def
[esi+204] = [def+92]              ; persist Width
[esi+208] = [def+88]              ; persist Height
walk [def+100] list:
  0041D21B(cell name) → vtbl+172
```

This is leftover size for **the table**, plus
first construct of named cells. It does **not**
write widget `+248`.

### 3.2 `vtbl+172` `00550C60` — layout then fill

```
00550C81  call 0052C730              ; persist dest on table + current +176
00550C8A  call [eax+568]             ; refill / place cells
```

`0052C730` has already run `005339B0` on persist
children. Cell **row/column** dest is the `+568`
callee.

### 3.3 `vtbl+8` `00550DC0` — draw, table dest only

`ret 20` (same 5 dwords as `00530260`). Writes
**this** table’s dest from persist `+52/+56` plus
parent `vtbl+100` / `vtbl+472`:

```
00550DF3  fld [esi+56] / [esi+52]
          fadd parent.vtbl+100
          → +84/+88
00550E36  mov [esi+248], ecx         ; dest origin
00550E44  mov [esi+252], edx
… dest scale +264 from parent vtbl+476 …
00551329  call 00530260              ; draw +176
```

This is **not** the type-16 dest writer. It does
not add a row pitch to sibling WHOLE groups.

### 3.4 Cell Position writer `00551EA0`

Called after `005514F0` wipes `+176` and from
table `vtbl+568` (`00551701` / `00550C8A`).
0-arg thiscall. Uses leftover `+204/+208` as the
table W/H budget (column/row counts =
budget / cell `vtbl+92` size). Then for each
`+348` template:

```
0041D21B → clone
clone.vtbl+172
layout0 = 0042D5B1(key=0)
[layout+8]  = cursor.x                 ; PositionX
[layout+12] = cursor.y                 ; PositionY
append clone to +176
cursor.x += cell.vtbl+92.x
cursor.y += cell.vtbl+92.y
```

`005518E0` (`ret 28`): same write of
`[layout+8/+12]`, loop `ebp+28` times, accumulate
**X** (`fadd [eax]` after template `vtbl+92`).
`00551BC0`: twin, accumulate **Y**
(`fadd [eax+4]`), writes scale into layout `+20`.

Those stores are the **table-row dest**. Next
`005339B0` on the clone copies `+8/+12` →
`+52/+56` → `+248`.

`.text` has **no** `E8 005518E0` / `E8 00551BC0`
(`e8.tsv`). Call is vtbl / `00551EA0` only.

---

## 4. Why the two sliders share `y`

Dumped tree (persist `Children`, host factory):

```
UI_NEW_PROFILE_MENU  type 12
  UI_NEW_PROFILE_BUTTON                 type 11   y=240
  UI_OPTIONS_TEXT_SLIDER_WHOLE_*METHOD  type 5    y=240
    UI_OPTIONS_TEXT_SLIDER_BUTTON_TABLES type 5
      UI_BUTTON_OPTIONS_LEFT            type 2    dest W=288 (persist W)
      UI_BUTTON_OPTIONS_RIGHT           type 2    dest W=192
      UI_OPTIONS_*_TEXT_SLIDER          type 16   y=245
  UI_OPTIONS_TEXT_SLIDER_WHOLE_*UP_DOWN type 5    y=240   ← same
  UI_OPTIONS_SLIDER_WHOLE_*SENSITIVITY  type 5    y=352
```

The type-2 widgets are **columns inside one
row**, not a parent that stacks the two WHOLEs.
Type 12 `0054D660` starts with `0052C730` (persist
layout already applied to every persist child)
and does not rewrite those children’s `+52/+56`.

Host `FrontendLayout.Compute` is that persist
path only. `00551EA0` is **LEFTOVER** on the
host. Duplicate persist names
(`UI_BUTTON_OPTIONS_LEFT`,
`UI_OPTIONS_TEXT_SLIDER_BUTTON_TABLES`) also
collide in `dests[name]`, but the WHOLE /
type-16 names are unique — the shared `y` is
not that map.

So first-seen native `005339B0` on the **persist
instances** also produces the same two points
unless a later table fill **replaces** `+176`
with clones that carry a new layout `+12`.
`005514F0` does wipe `+176` then `vtbl+568` —
**on the type-2 object**, not on the type-5
WHOLE or type-12 list.

| Object | Native dest writer | Host |
| --- | --- | --- |
| Type 16 persist instance | `005339B0` / `0052FFD0` from persist Y | **MATCH** → both `245` |
| Type-2 cell clone | `00551EA0` layout `+12` then `005339B0` | **LEFTOVER** (never constructed) |
| Type-6 `ARROWS`/`NORMAL` | parent type-16 origin + persist (point) | **MATCH** stack |
| Type-2 leftover W | persist Width → `+204` → dest X1 | **MATCH** `288` / `192` / `1024` |

Splitting the two type-16 points requires either
different persist `PositionY` on the WHOLE rows
(not what the dest math shows) or running
`00551EA0` on a table whose cells **are** those
rows. The dumped New Profile tree is the first
case. Do not invent a type-16 dest width to
fake a second row.

---

## 5. Type-6 leftover `+204` as dest width

Already **DISPROVEN** (`proofs/type6-plus204-writer`).
`0054EF00` **reads** `[esi+204]` for centre/right
pen only. Dest is `0041AFA0` from `+360` else
`+204`. First-seen type-6 / type-16 `+204` is 0
→ dest point. `00551340` leftover store is type
**2**, not type 16.

---

## 6. UNREAD / next

- rdata dwords at `0124A224+8` / `+172` / `+568`
  / `+572` / `+576` (slot numbers **PARTIAL**).
- Sequential `frontend.bin` walk of
  `PositionX`/`PositionY`/`Absolute`/`Width` on
  the two WHOLE defs, both type-16 defs, and
  `UI_BUTTON_OPTIONS_LEFT`.
- Whether any New Profile type-2 `def+100` cell
  names the WHOLE / type-16 templates (would
  make `00551EA0` the row writer on a **clone**,
  while persist instances stay at Y=0).
- Type 12 `def+308/+304` extras in `0054D660`
  (not dest Y on existing persist children).

Do not apply a host `+70` row guess without the
persist dump or an `00551EA0` walk of that
table’s `+348` tree.
