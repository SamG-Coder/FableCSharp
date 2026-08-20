# Leftover #48: New Profile dest / hit (type-12)

Investigation only. No production `src/` edits.
`PlaceTableCell` n==3 leftover fill and
`TryChromeHit` type-16/37 hit size stay host
stand-in.

Question: recover dest of type-12
`UI_NEW_PROFILE_MENU` from listings
`00551340` / `00551EA0` / `005339B0`.

Authority: dump only.
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
(`00551340` / `005514F0` / `005517E0` / `00551EA0` /
`0054C3A0` / `0054D660` / `0055B8F0`);
`listing-00500000.txt` (`005339B0` / `0052C730` /
`0053822B` / `00539B58` / `0052FFD0`);
`listing-00400000.txt` (`0041AFA0` / `0041D21B`);
`e8.tsv` (`00551340` ← `0055184B` / `005518CB`;
`005339B0` ← `0052C733`; **no** `.text` `E8 00551EA0`);
`export/frontend/new-profile-dests.txt`;
`src/Fable.Game/FrontendLayout.cs`;
`src/Fable.Game/FrontendWidgetFactory.cs`;
`src/Fable.Game/FrontendHitTest.cs`;
`src/Fable.Game/EngineLifecycle.cs`
(`PlaceTableCell` / `TryChromeHit`);
`implementer/frontend/02-layout.md`;
`proofs/type2-row-dest`;
`proofs/type16-slider-row-dest`;
`proofs/list-type12-focus`;
`proofs/type12-highlight-plus348`.

Do not invent dest width. Type-6 leftover `+204`
as dest width is **DISPROVEN**. Do not run
ExeIndex `fn` / `vtbl`.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

---

## Verdict

**Type-12 dest is `005339B0` inherit then
`0052FFD0` / `0041AFA0`. It is a POINT.
`00551340` and `00551EA0` are not type-12
dest writers.**

Type 12 `vtbl+172` is `0054D660`: `call 0052C730`
then extras (`+348=0`, optional `def+308` clone).
`0052C730` is `call 005339B0`. `005339B0` copies
layout-state 0 `+8/+12` (persist `PositionX` /
`PositionY`) into widget `+52/+56` and, when
`+280==0`, `+272/+276=1` and
`+76/+80/+256/+260=0`. Dest origin is later
`0052FFD0`. Size is `0041AFA0`: persist
`+360/+364` else leftover `+204/+208`. Type 12
is not `00551340` (type-2 leftover only). First-seen
GraphicIndex 0 → leftover 0 → dest is a **point**.

Dump **MATCH**: `UI_NEW_PROFILE_MENU t=12 dest=64,240,64,240`.

Host leftovers that are **not** this dest:

| Host | Listing | Class |
| --- | --- | --- |
| `PlaceTableCell` `count==3` left/right leftover W + middle fill | `00551EA0` | **LEFTOVER** invented. Native walks `+348` templates, writes clone layout `+8/+12` from a cursor, `cursor += vtbl+92`. No `cmp count, 3`. |
| `TryChromeHit` type-16/37 hit = dest origin + rightmost type-2 dest size | `0055B8F0` | **LEFTOVER** invented. Native AABB is `vtbl+488` origin + `vtbl+492` size. Point dest → empty / point hit. |
| `ListChildAuthoredPos` later rows `first + index * +326` as dest Y | `005339B0` / `0054D660` / `00539B58` | **UNREAD** as dest. `0054D660` does **not** rewrite child dest. `00539B58` is list `+380` layout **key 5**, not child layout key 0. |
| persist-size skip (`persistW==0 && persistH==0`) before `PlaceTableCell` | `00551EA0` | **LEFTOVER** host gate on the same helper. |

**Answer:** type-12 dest formula is listing-locked
below. Leftover #48 cell fill / chrome hit /
row-pack dest stay **UNREAD** / invented. Do not
edit `src/`.

| Claim | Class |
| --- | --- |
| Type 12 ctor `0054C3A0` → type 8 `0053B63E` vtbl `01249224` | **PROVEN** |
| Type 12 `vtbl+172` body is `0054D660` (`0052C730` then `+348=0`) | **PROVEN** body; rdata dword **PARTIAL** |
| `0054D660` writes child dest Y / `+326` stride onto persist kids | **DISPROVEN** |
| `005339B0` copies layout0 `+8/+12` → `+52/+56`; recurse child `vtbl+172` | **PROVEN** |
| Dest origin is `0052FFD0` / parent `00531EC0` from `+52/+56` | **PROVEN** |
| Dest size is `0041AFA0` persist `+360` else leftover `+204` | **PROVEN** |
| Type 12 leftover `+204` from `00551340` | **DISPROVEN** (`00551340` is type-2 ctor tail) |
| Host dump type-12 dest is a point `64,240,64,240` | **PROVEN** |
| Raw persist `Position` / remap bits on `UI_NEW_PROFILE_MENU` | **UNREAD** (back-solved **PARTIAL**: `(40,150)` × `1.6` **or** `(64,240)` no remap) |
| Type-8 ctor `0053822B` copies `def+322/+326` → widget `+392/+396` | **PROVEN** |
| Persist `+326=30` / `+322=0` on this list | **PROVEN** file (`Plus326Crc` `0xD7495328`) |
| `00539B58` `fadd [esi+396]` is child dest writer | **UNREAD** (writes list `+380` key 5) |
| Type 2 leftover `+204/+208` = persist W/H in `00551340` | **PROVEN** |
| Cell Position writer is `00551EA0` clone layout `+8/+12` | **PROVEN** stores |
| `.text` `E8 00551EA0` | **DISPROVEN** (empty `e8.tsv`) |
| Host runs `00551EA0` | **DISPROVEN** |
| `00551EA0` `count==3` leftover fill | **DISPROVEN** (no such compare) |
| `TryChromeHit` is `0055B8F0` size | **DISPROVEN** |
| Type-16/37 dest is a point | **PROVEN** dump |
| Type-6 leftover `+204` is dest width | **DISPROVEN** |

---

## 1. Type-12 dest formula (listing-locked)

Type 12 layout (`listing-00540000`):

```
0054D660  sub esp, 32
          call 0052C730          ; 005339B0 dest inherit
          … walk +356 → +380 …
          mov [esi+348], 0
          vtbl+432 → def
          optional 0041D21B(def+308) append +176
```

`0052C730` (`listing-00500000`):

```
0052C733  call 005339B0
          +324/+328/+332=0
```

`005339B0`:

```
lea ebx, [esi+36]                ; layout-state map
0042D5B1(key=0)
mov [esi+52], [eax+8]            ; persist PositionX
mov [esi+56], [eax+12]           ; persist PositionY
… +92/+96 from layout +16 …
if +280==0:
  +76/+80/+256/+260=0
  +272/+276=1.0
+144..+147=0xFF
walk +176:
  if child vtbl+208==0:
    vtbl+204(this)
    child vtbl+172
```

Dest origin (`02-layout.md` / `0052FFD0`):

```
ox = remapOrigin ? persistPosX / 640 * vpW : persistPosX
oy = remapOrigin ? persistPosY / 480 * vpH : persistPosY
if !absolute:
  ox = ox * inheritScaleX + parentDestX
  oy = oy * inheritScaleY + parentDestY
```

Dest size (`0041AFA0`):

```
w = (persistW != 0) ? persistW : leftover204
h = (persistH != 0) ? persistH : leftover208
w *= destScaleX
h *= destScaleY
dest = center ? (ox±w/2, oy±h/2) : (ox, oy, ox+w, oy+h)
fistp/fild snap
```

Collapsed first-seen (flags 0, persist scale 1,
`+280==0`, leftover 0, persist W/H 0):

```
UI_NEW_PROFILE_MENU dest = (ox, oy, ox, oy)
```

That is a **point**. Do not grow it from type-2
children. `ExpandTableDests` / `TryChromeHit`
are not this widget.

---

## 2. `00551340` — type-2 leftover, not type-12 dest

Only `.text` `E8` sites: `0055184B` / `005518CB`
(type-2 ctor `005517E0` / copy ctor `00551860`).

```
call [eax+432]                   ; CUIDef*
mov [esi+204], [def+92]          ; persist Width
mov [esi+208], [def+88]          ; persist Height
alloc +360 from def+124
alloc +364 from def+136
walk [def+100]:
  0041E5F2 / 0041D21B            ; named cell
  vtbl+172
```

This leftover store is **type 2 only**. It is why
`UI_BUTTON_OPTIONS_LEFT` leftover is persist
`(180, 0)` and dest W is that × dest scale
(`288` after `1.6`). It is **not** type-12 dest
width and **not** type-16/37 dest width.

---

## 3. `00551EA0` — clone cursor, not n==3 fill

No `.text` `E8`. Identity as type-2 `vtbl+568` is
**PARTIAL**. Store pattern **PROVEN**.

```
call [eax+432]                   ; def
if [def+96] bit 0:
  firstTemplate.vtbl+92.x → i32 count
  fdivr [ebx+204]                ; leftover W / count  (ftol)
  divide +364 dword column by count
if [def+96] bit 1:
  firstTemplate.vtbl+92.y → count
  fdivr [ebx+208]
walk +348 templates (tree keys 0, then 1, 2, 3, 4, …):
  0041D21B clone
  clone.vtbl+172
  0042D5B1(key=0)
  mov [eax+8],  cursor.x         ; layout PositionX
  mov [eax+12], cursor.y         ; layout PositionY
  append clone to +176
  cursor += cell.vtbl+92
```

Later keys adjust the cursor (`fsubr` leftover
budget, `fsub` cell size). That is **not**
host `PlaceTableCell`:

```
count==3:
  leftW  = first sibling leftover W
  rightW = second sibling leftover W
  midW   = leftoverW - leftW - rightW
  index 0: origin, leftW
  index 1: origin + leftoverW - rightW, rightW
  index 2: origin + leftW, midW
```

No `cmp …, 3` in `00551EA0`. Cap/fill numbers
from sibling leftover W are **invented**. Exact
native cursor per sprite key `0,1,4` after the
integer leftover divide is **UNREAD** (stack
locals of a 1946-insn frame). Host also skips
the helper unless `persistW==0 && persistH==0`
— extra heuristic, not in this listing.

Host never constructs these clones
(`type2-row-dest`). Persist sprite kids are
attached by name (`AttachSpriteCells` /
`00551340` `def+100` walk) and dest’d by
`005339B0` + leftover graphic / table leftover.

---

## 4. `+326` row pack is not `005339B0` dest

Type-8 ctor `0053822B`:

```
[esi+392] = [def+322]            ; Plus322 (0 on this list)
[esi+396] = [def+326]            ; Plus326 (30)
[esi+400] = -[def+322]
[esi+404] = [def+326]
```

`00539B58` (inside `005392FC` pack):

```
0042D5B1 on list+380, key ebx=5
fld [eax+12]
fadd [esi+396]                   ; previous packed Y + spacing
fld [eax+8]
fadd [esi+392]                   ; previous packed X + spacing
```

That record is the list’s `+380` array, layout
**key 5**. `005339B0` dest pos is layout **key 0**.
Whether key 5 is later copied onto persist
children’s key 0 / `+52/+56` is **UNREAD**
(`005385AE` / `005392FC` / `0053AD07` /
type-8 `0053B91E`). Type-12 `0054D660` does
not do that copy.

Host `ListChildAuthoredPos` overwrites authored
pos for dest compute (`index * 30`). Tests lock
that helper and “four distinct DestY0”, **not**
native dest numbers. Leave that as leftover
#48 family.

---

## 5. Hit — `0055B8F0` does not invent type-16 size

```
0055B8F0
  vtbl+488 → origin
  vtbl+492 → size
  vtbl+96  → extra
  left  = origin.x + extra.x
  top   = origin.y + extra.y
  right = origin.x + size.x * extra.?
  bot   = origin.y + size.y * extra.?
  contains inclusive/exclusive on those ints
```

Type-16/37 `0041AFA0` dest is a **point**
(leftover 0). Native hit of a point dest has
no area. Host `TryChromeHit` copies the
rightmost type-2 table dest size onto the
type-16/37 hit rect and claims `0055B8F0`.
That **regresses** dest-AABB-only
(`6e76ac5`) and is **DISPROVEN** as this
body.

`FrontendHitTest.HitRect` already prefers a
pre-assigned `Hit*` when it has area. That
assignment is the leftover.

---

## 6. Classification

| Object | Native dest writer | Host |
| --- | --- | --- |
| Type-12 `UI_NEW_PROFILE_MENU` | `005339B0` / `0052FFD0` / `0041AFA0` point | **MATCH** dump `64,240,64,240` |
| Type-12 persist children dest Y | persist pos through same inherit; `+326` pack **UNREAD** as dest | **LEFTOVER** `ListChildAuthoredPos` |
| Type-2 table persist instance | same + leftover W from `00551340` | **MATCH** leftover store; dest W **PARTIAL** scale |
| Type-2 cell clone | `00551EA0` layout `+8/+12` then `005339B0` | **LEFTOVER** (never constructed); n==3 fill invented |
| Type-16 / type-37 | `005339B0` / `0052FFD0` point | **MATCH** point dest; **LEFTOVER** chrome hit size |

---

## UNREAD sites

- Raw `frontend.bin` `PositionX` / `PositionY` /
  `Absolute` / `+302` bit 7 on
  `UI_NEW_PROFILE_MENU` and its four persist
  children (dest `64,240` back-solve only).
- Whether `00539B58` packed key-5 pos is
  copied onto child layout key 0
  (`005385AE` / `005392FC` / `0053AD07` /
  `0053B91E`).
- `00551EA0` stack map: leftover-budget
  integer divide → per-key cursor for sprites
  `(0,1,4)`. Not host n==3.
- `.rdata` dword `01249224+172` =
  `0054D660`; `0124A224+568` = `00551EA0`.
- `0055B8F0` `vtbl+488/+492/+96` bodies
  (claimed dest origin / dest size).
- Type-16/37 dest size writer: none recovered;
  leftover stays 0.

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00540000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00500000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\functions.tsv`
- `C:\FableCSharp\export\frontend\new-profile-dests.txt`
- `C:\FableCSharp\proofs\type2-row-dest\README.md`
- `C:\FableCSharp\proofs\type16-slider-row-dest\README.md`
- `C:\FableCSharp\proofs\list-type12-focus\README.md`
- `C:\FableCSharp\proofs\type12-highlight-plus348\README.md`
- `C:\FableCSharp\implementer\frontend\02-layout.md`
