# Leftover #48: native dest AABB for New Profile

Investigation only. Production `src/` and `tests/` were
not edited. Do **not** invent dest fill. Do **not**
plant type-16/37 hit size. Do **not** re-enable
`Key.N` / `ActivateNewGame`.

Question: recover native dest AABB for New Profile
from listings if the listing allows. Native
`TryChromeHit` / `0055B8F0` must not invent
type-16/37 hit. `PlaceTableCell` `n==3` leftover
fill stays host.

Authority: dump only.
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00400000.txt`
(`0041A850` / `0041AC20` / `0041AFA0` / `0041B173` /
`0041BEB0`);
`listing-00500000.txt`
(`0052C730` / `0052ECC0` / `0052EEC0` / `0052F5C0` /
`0052FFD0` / `00531090` / `005339B0`);
`listing-00540000.txt`
(`0054D660` / `00551340` / `00551EA0` / `0055B8F0` /
`0055BF10` / `005491A0` / `00540CF0`);
`e8.tsv` (**no** `.text` `E8 0055B8F0`; **no**
`.text` `E8 00551EA0`);
`src/Fable.Formats/Defs/FrontendUiDef.cs`
(persist `PositionX`/`PositionY` / Width / Height /
`ScaleSizeCrc` / `ScaleOriginCrc`);
`src/Fable.Game/FrontendLayout.cs`
(`Type12DestIsPointWhenSizeZero` /
`PlaceTableCellCount3IsNative=false` /
`TryChromeHitIsNativeHit=false` /
`NativeHitWalksRightmostType2=false` /
`NativeDestTupleUnread=true`);
`src/Fable.Game/FrontendHitTest.cs`;
`src/Fable.Game/EngineLifecycle.cs`
(`PlaceTableCell` / `TryChromeHit` /
`AssignHitRects`);
`export/frontend/new-profile-dests.txt`;
`proofs/leftover-48-dest`;
`proofs/leftover-48-native-hit`;
`proofs/leftover-48-chrome-hit`;
`proofs/leftover-14-dest-aabb`;
`proofs/0041AC20-dest-formula`;
`docs/status/README.md` leftover #48.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Do not re-prove GraphicIndex leftover, type-6
`+204` as dest width, or LMB type 4/6 posters.
Do **not** invent dest 4-tuples.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| Native dest AABB 4-tuple stored on a New Profile widget? | **None.** `0041AFA0` dest is stack `[esp+36..48]` then type-`0x22` rec `+12..+24`. Hover `0055B8F0` writes `al` only | **DISPROVEN** as a widget store |
| Native dest **formula** for New Profile? | Same `005339B0` / `0052FFD0` / `0041AFA0`: persist W/H else leftover `+204/+208`, then `* destScale` from dest origin. Persist 0 and leftover 0 → **POINT** | **PROVEN** |
| Persist `Position` / remap bits on New Profile defs? | **Yes, file recover this pass.** `UI_NEW_PROFILE_MENU` `(40,150)` `o=0 s=0`. Screen `s=1`. Sibling leftover-48-dest “UNREAD persist Position” is **stale** | **PROVEN** file |
| Type-12 dest AABB analog from that persist? | Parent dest scale `1.6` (`s=1` on type-10). `(40,150)×1.6` → POINT `(64,240,64,240)` | **MATCH** formula analog vs host dump; **UNREAD** as native 4-tuple dump |
| Native `0055B8F0` invent type-16/37 hit? | **No.** Type 11/38 `vtbl+568` only. Type 16 `+568` is `005491A0`. Type 37 `+568` is `00540CF0`. No sibling type-2 dest copy | **DISPROVEN** |
| Host `TryChromeHit` type-16/37 hit? | Dest origin + rightmost type-2 dest size. `TryChromeHitIsNativeHit=false` | **LEFTOVER** invented |
| `PlaceTableCell` `n==3` leftover fill? | Host. Listing `00551EA0` has no `cmp …, 3`. `PlaceTableCellCount3IsNative=false` | **LEFTOVER** |
| Native dest AABB numbers dump (`[esp+36..48]` / rec `+12..+24`)? | **None.** Same unread as leftover #36 dest-lock | **UNREAD** |
| Close leftover #48? | **No.** Cell fill / chrome hit / row-pack dest stay host. Do not edit `src/` | **LEFTOVER** open |
| Re-enable `Key.N`? | **No.** Native poster is LMB type 4/6 (`NativeKeyNPostsNewGame=false`) | **DISPROVEN** |

---

## Verdict

**Native dest AABB for New Profile is the
`0041AFA0` formula, not a widget dest field
and not `TryChromeHit`. Persist 0 and leftover
0 → POINT. Native `0055B8F0` does not invent
type-16/37 hit. `PlaceTableCell` `n==3` leftover
fill stays host. Leave leftover #48 open.**

Listing **allows** the dest **formula** and the
persist inputs. It does **not** allow a native
4-tuple dump or a type-16/37 hit size.

`TryChromeHit` is leftover #48 hit stand-in
(`leftover-48-chrome-hit`). Native `0055B8F0`
is type **11/38** hover: dest **origin**
(`vtbl+488`) plus dest **scale** (`vtbl+492`)
times `+176` child extra (`vtbl+96`). Extra
empty → empty hit. Sibling type-2 tables
**not** on `+176` never enter.
`NativeHitWalksRightmostType2=false`.
Type-16/37 dest stays a **point**. Do not
copy type-2 dest size onto that point and
call it native dest AABB.

`PlaceTableCell` `count==3` left/right leftover
W + middle fill is **not** `00551EA0`. Native
walks `+348` templates and writes clone layout
`+8/+12` from a cursor. No `cmp count, 3`.
Type-2 dest **height** 32 on the host dump is
that leftover, not persist Height (0).

**Answer:** recover persist + dest formula.
Type-12 dest analog is POINT `64,240,64,240`.
Type-16/37 dest analog is a POINT at persist
pos × inherit; host `608,293` is row-pack
leftover, not native dest AABB. Do not invent
dest. Do not invent type-16/37 hit. Do not
edit `src/`.

| Claim | Class |
| --- | --- |
| Type-12 persist `Position=(40,150)` `+326=30` `+322=0` `s=0 o=0` | **PROVEN** file |
| Type-10 screen persist `(0,0)` `s=1` `o=0` → dest scale `1.6` | **PROVEN** file + `0052F5C0` |
| Type-12 dest formula analog POINT `(64,240,64,240)` | **MATCH** host dump; **UNREAD** as native dump |
| Type-16/37 persist W/H 0 leftover 0 → dest POINT | **PROVEN** formula |
| Type-16 host dest `608,293` is native dest AABB | **DISPROVEN** — needs unread `+326` dest copy; persist WHOLE `X=-100` |
| Native dest AABB 4-tuple on the widget | **DISPROVEN** (`SubmitDestStoresOnWidget=false`) |
| Native first-seen dest 4-tuple dump | **UNREAD** (`NativeDestTupleUnread=true`) |
| `0055B8F0` is type 11/38 `vtbl+568` | **PROVEN** rdata |
| Type 16 `vtbl+568` is `0055B8F0` | **DISPROVEN** (`005491A0`) |
| Type 37 `vtbl+568` is `0055B8F0` | **DISPROVEN** (`00540CF0`) |
| `0055B8F0` walks rightmost type-2 dest size | **DISPROVEN** |
| `TryChromeHit` is native `0055B8F0` | **DISPROVEN** (`TryChromeHitIsNativeHit=false`) |
| `00551EA0` `count==3` leftover fill | **DISPROVEN** (no such compare) |
| `PlaceTableCell` `n==3` is native dest | **DISPROVEN** (`PlaceTableCellCount3IsNative=false`) |
| Type-2 leftover `+204/+208` = persist W/H (`00551340`) | **PROVEN** |
| Type-6 leftover `+204` is dest width | **DISPROVEN** |
| `Key.N` / `ActivateNewGame` native New Game | **DISPROVEN** (`NativeKeyNPostsNewGame=false`) |

---

## 1. Persist recover (file, this pass)

`FrontendUiDef.TryParse` of `frontend.bin`
(`PositionXCrc` `0x1EDB8A31` / `PositionYCrc`
`0x69DCBAA7` / `ScaleSizeCrc` `0xC50CA371` /
`ScaleOriginCrc` `0xB466D948`). Flags `C/A/o/s`
= Centre / Absolute / remap origin / remap size.

| Def | Type | Persist XY | Persist WH | o/s | Notes |
| --- | ---: | --- | --- | --- | --- |
| `UI_FRONTEND_NEW_PROFILE_SCREEN` | 10 | `0,0` | `0,0` | 0/1 | dest scale `1.6` |
| `UI_TEXT_NEW_PROFILE_MENU_TITLE` | 6 | `65,44` | `0,0` | 1/0 | leftover 0 → POINT |
| `UI_TABLE_TITLE_WHOLE` | 2 | `0,35` | `640,0` | 0/0 | leftover W=640 H=0 |
| `UI_NEW_PROFILE_MENU` | 12 | `40,150` | `0,0` | 0/0 | `+326=30` `+322=0` |
| `UI_NEW_PROFILE_BUTTON` | 11 | `0,0` | `0,0` | 0/0 | kids tables / text / type 37 |
| `UI_BUTTON_OPTIONS_LEFT` | 2 | `0,0` | `180,0` | 0/0 | leftover `(180,0)` |
| `UI_BUTTON_OPTIONS_RIGHT` | 2 | `331,-3` | `120,0` | 0/0 | leftover `(120,0)` |
| `UI_BUTTON_OPTIONS_RIGHT_EDITBOX` | 2 | `320,-3` | `220,0` | 0/0 | leftover `(220,0)` |
| `UI_NEW_PROFILE_EDIT_BOX` | 37 | `330,0` | `0,0` | 0/0 | POINT dest |
| `UI_OPTIONS_TEXT_SLIDER_WHOLE_CONTROL_METHOD` | 5 | `-100,0` | `0,0` | 0/0 | same Y as next WHOLE |
| `UI_OPTIONS_CONTROL_METHOD_TEXT_SLIDER` | 16 | `340,3` | `0,0` | 0/0 | POINT dest |
| `UI_OPTIONS_TEXT_SLIDER_WHOLE_CAMERA_UP_DOWN` | 5 | `-100,0` | `0,0` | 0/0 | same persist Y as control-method |
| `UI_OPTIONS_CAMERA_UP_DOWN_TEXT_SLIDER` | 16 | `340,3` | `0,0` | 0/0 | POINT dest |
| `UI_OPTIONS_SLIDER_WHOLE_CAMERA_SENSITIVITY` | 5 | `-100,70` | `0,0` | 0/0 | persist Y **70** |
| `UI_SLIDER_CAMERA_SENSITIVITY` | 15 | `0,8` | `0,0` | 0/0 | g=386 leftover area |
| `UI_ACCEPT_NEW_PROFILE` | 38 | `362,420` | `0,0` | 0/0 | POINT dest |
| `UI_CANCEL` | 38 | `20,420` | `0,0` | 0/0 | POINT dest |
| `UI_HELPER_BUTTON_MOUSE_AREA` | 0 | `0,0` | `250,30` | 0/0 | dest **has area** |
| `UI_OPTIONS_TEXT_CONTROL_ARROWS` | 6 | `60,0` | `0,0` | 0/0 | POINT; leftover 0 |
| `UI_OPTIONS_TEXT_CONTROL_WASD` | 6 | `60,0` | `0,0` | 0/0 | POINT; not a hit |

Type-12 children in persist order: type-11
button, WHOLE control-method, WHOLE camera
up/down, WHOLE sensitivity.

leftover-48-dest back-solve
`(40,150)×1.6` **or** `(64,240)` no remap:
the file is **`(40,150)` no remap on the
list**; remap size is on the **screen**.

---

## 2. Native dest AABB formula (listing)

Dest origin `0052FFD0` / dest scale
`0052F5C0` / submit dest `0041AFA0`
(`02-layout.md` / `FrontendLayout.Compute`):

```
if remapOrigin: pos = persistPos / 640×vpW , /480×vpH
else            pos = persistPos
if !absolute:   destOrigin = pos * inheritScale + parentDestOrigin

w = (persistW != 0) ? persistW : leftover204
h = (persistH != 0) ? persistH : leftover208
w *= destScaleX
h *= destScaleY
dest = center ? (ox±w/2, oy±h/2) : (ox, oy, ox+w, oy+h)
fistp/fild snap onto [esp+36..48]
```

First-seen screen `s=1`, persist scale 1,
viewport 1024×768 → dest scale **1.6**.
Global scale 1 (`[0x13B86A0]==0`).

`0041AFA0` does **not** `fstp` dest onto the
widget (`leftover-14-dest-aabb`).
`NativeDestTupleUnread=true`. Formula analog
below is **not** a native dump.

Collapsed when persist W/H 0 and leftover 0:

```
dest = (ox, oy, ox, oy)     ; POINT
```

`Type12DestIsPointWhenSizeZero=true`. Do not
grow that point from type-2 children.
`ExpandControlDests` was deleted (`68eb1c5`).
`TryMouseAreaDest` dest-copy was removed
(`e3208eb`). Do not restore dest invent.

---

## 3. Formula analog vs host dump

Parent inherit **without** host
`ListChildAuthoredPos` (native `0054D660`
does **not** rewrite child dest;
`leftover-48-dest`).

| Widget | Formula analog dest | Host dump | Class |
| --- | --- | --- | --- |
| Screen t=10 | POINT `0,0,0,0` scale 1.6 | `0,0,0,0` | **MATCH** analog |
| Title t=6 | remap `(65,44)` → `(104,70.4)` × inherit 1.6 → POINT `166,113` | `166,113,166,113` | **MATCH** analog |
| List t=12 | `(40,150)×1.6` POINT `64,240,64,240` | `64,240,64,240` | **MATCH** analog |
| Button t=11 | persist `0,0` under list → POINT `64,240` | `64,240,64,240` | **MATCH** analog |
| Edit t=37 | `64+330×1.6` POINT `592,240` | `592,240,592,240` | **MATCH** analog |
| LEFT t=2 | origin `64,240` W=`180×1.6=288` H=**0** → `64,240,352,240` | `64,240,352,272` | W **MATCH**; H **LEFTOVER** fill 32 |
| RIGHT_EDITBOX t=2 | `64+320×1.6=576`, `240-3×1.6=235.2` W=`220×1.6=352` H=0 | `576,235,928,267` | W **MATCH**; H **LEFTOVER** |
| WHOLE control-method t=5 | `64+(-100)×1.6=-96`, Y=`240` POINT | `64,288,64,288` | **LEFTOVER** pack (discard X=-100, Y=`index×30`) |
| Slider t=16 | parent `-96,240` + `(340,3)×1.6` POINT `448,245` | `608,293,608,293` | **LEFTOVER** pack; dest still a **point** |
| WHOLE sensitivity t=5 | persist Y **70** → `-96,352` POINT | `64,384,64,384` | persist Y **MATCH** split; pack **LEFTOVER** |
| Accept t=38 | `(362,420)×1.6` POINT `579,672` | `579,672,579,672` | **MATCH** analog |
| Cancel t=38 | `(20,420)×1.6` POINT `32,672` | `32,672,32,672` | **MATCH** analog |
| Mouse area t=0 | persist `250×30` ×1.6 = `400×48` from Accept → `579,672,979,720` | `579,672,979,720` | **MATCH** analog (**has area**) |
| WASD t=6 | persist `60,0` leftover 0 POINT | `704,293,704,293` | POINT **MATCH** class; host XY is pack leftover. **Not** a hit |

Host dump is `LayoutFrontendWidgets` analog
(`export/frontend/new-profile-dests.txt`).
It is **not** `[esp+36..48]`. Leave leftover
**#36** dest-lock open for native numbers.

Row-pack dest (`ListChildAuthoredPos`
`index × +326`, persist X=-100 discarded)
stays leftover #48 family. Tests lock four
distinct `DestY0`, **not** native dest
numbers.

---

## 4. Native `TryChromeHit` must not invent type-16/37 hit

`listing-00540000.txt` `0055B8F0`:

```
0055B8F0  sub esp, 32
          call [eax+488]          ; 0052EEC0 origin → [esp+12]
          call [edx+492]          ; 0052ECC0 scale → [esp+20]
          call [edx+96]           ; 00531090 extra → [esp+28]
          left  = fistp(origin.x + extra[0])
          top   = fistp(origin.y + extra[1])
          right = fistp(origin.x + scale.x * extra[2])
          bot   = fistp(origin.y + scale.y * extra[3])
          hit iff left <= x < right && top <= y < bot
          al = 1 / 0
          ret 4
```

No `+176` walk in **this** body (extra getter
does it). No type compare. No type-2 dest
load. `.text` `E8 0055B8F0` empty.

`vtbl+492` is persist **scale** `+100/+104`,
**not** `0041AFA0` dest W/H
(`leftover-48-native-hit`).
`FrontendHitTest` comments that still say
“dest origin plus dest size” are **STALE**
vs this listing. Investigation only — do
not edit `src/`.

| Type | `vtbl+568` | AABB? |
| ---: | --- | --- |
| 11 / 38 | `0055B8F0` | yes, origin + scale × `+176` extra |
| 16 | `005491A0` | **no** — 0-arg `+404` copy |
| 37 | `00540CF0` | **no** — string get |
| 2 | `00551EA0` | **no** — clone cursor |

Host `AssignHitRects`: dest AABB if dest has
area, else `TryChromeHit` for type **16/37
only**:

```
hit = (destX0, destY0, destX0 + rightmostType2W, destY0 + rightmostType2H)
```

That invents type-16 hit `608,293,800,325`
from RIGHT dest `192×32`. Native type 16
never owns `0055B8F0`. Type-16 parent is
type 5, which also has no `0055B8F0`.
Type-16 `+176` kids are type-6 leftover 0
→ extra `0,0,0,0` even if someone called
`0055B8F0` on the slider.

Type-37 native row hit is **parent** type 11
`0055B8F0` extra, **not** a dest write onto
the edit box. Extra numbers on New Profile
type 11/38: **UNREAD**. Do not copy that
AABB onto type-37 dest. Do not treat it as
type-16.

`TryChromeHitIsNativeHit=false`.
`NativeHitWalksRightmostType2=false`.
Native TryChromeHit **must not** invent
type-16/37 hit. Host helper stays leftover
#48 (`leftover-48-chrome-hit`: do not delete
it here; `(700,300)` is a host lock on the
invented rect). Dest-AABB-only (`6e76ac5`)
does not recover `0055B8F0`.

WASD type-6 dest is a **point**. Frontend
LMB is not WASD (`audit-playerinterface`).
Do not invent a WASD dest / DIK heuristic.

---

## 5. `PlaceTableCell` n==3 leftover fill still host

`listing-00540000.txt` `00551EA0`:

```
call [eax+432]                   ; def
if [def+96] bit 0:
  firstTemplate.vtbl+92.x → count
  fdivr [ebx+204]                ; leftover W / count
if [def+96] bit 1:
  leftover H / count
walk +348 templates:
  0041D21B clone
  clone.vtbl+172
  layout key 0 +8/+12 = cursor
  cursor += cell.vtbl+92
```

No `cmp …, 3` in this function. Nearest
`cmp eax, 3` in the same listing file are
other bodies (`00556427` / `00556AA1` / …).
`.text` `E8 00551EA0` empty. Host never
constructs those clones (`type2-row-dest`).

Host `PlaceTableCell` `count==3`:

```
leftW  = first sibling leftover W
rightW = second sibling leftover W
midW   = leftoverW - leftW - rightW
index 0: origin, leftW
index 1: origin + leftoverW - rightW, rightW
index 2: origin + leftW, midW
```

Cap/fill numbers from sibling leftover W
are **invented**. Persist-size skip
(`persistW==0 && persistH==0` before the
helper) is extra host heuristic, same
family.

Type-2 dest **width** analog is persist W ×
dest scale (`180×1.6=288`). Type-2 dest
**height** persist 0 / leftover H 0.
Host dest H 32 is n==3 fill /
`ExpandTableDests` from sprite cells.
`PlaceTableCellCount3IsNative=false`.

---

## 6. Host leftovers (do not plant)

```
AssignHitRects
  Hit* = dest AABB
  if dest has no area:
    TryChromeHit(type 16/37)     ; invented hit size

LayoutFrontendWidgets
  ListChildAuthoredPos            ; unread as dest
  PlaceTableCell n==3             ; invented cell dest
  persistW==0 && persistH==0 skip ; extra gate
```

`FrontendHitTest.HitRect` prefers assigned
`Hit*` when it has area. That assignment is
the leftover feeding `(700,300)` /
nonempty type-16/37 chrome hit.

Accept LMB `0x126` does **not** need
`TryChromeHit`: type-38 dest is a point;
`HitIndex` walks type-0 mouse-area dest
`579,672,979,720` then `InteractiveAt`.
First-seen no-save `0xE5` / `0x126` / 15
never enter the helper
(`leftover-48-chrome-hit`).

Do not restore `Key.N`.
`FrontendInputMap.NativeKeyNPostsNewGame=false`.
Native poster is LMB type 4/6.

---

## Gap

```
Evidence                 Original                         Host                            Gap
0041AFA0 dest            stack [esp+36..48] then          stored widget Dest*             widget dest AABB has
                         rec +12..+24; persist else       analog of the formula           no listing writer
                         leftover; 0 → POINT
persist Position         frontend.bin (40,150) etc.       PersistX/Y on widgets           MATCH file; dest
                                                                                          numbers still analog
type-12 dest             POINT (40,150)×1.6               64,240,64,240                   analog MATCH
type-16/37 dest          POINT; pack dest UNREAD          packed 608,293 + TryChromeHit   leftover #48
type-2 dest H            persist 0                        n==3 fill 32                    leftover #48
0055B8F0                 type 11/38 origin+scale×extra    TryChromeHit type 16/37         invented hit
00551EA0                 clone cursor; no count==3        PlaceTableCell n==3             leftover #48
Key.N                    DISPROVEN                        still off                       do not re-enable
```

| Object | Native dest AABB | Host | Gap |
| --- | --- | --- | --- |
| Type-12 list | formula POINT `64,240` | MATCH analog | dump **UNREAD** |
| Type-16/37 | formula POINT; pack **UNREAD** | packed point + invented hit | **LEFTOVER** |
| Type-2 chrome | leftover persist W × scale; H 0 | n==3 H 32 | W **PARTIAL**; H **LEFTOVER** |
| Type-38 Accept | formula POINT `579,672` | MATCH analog; Hit=point | extra **UNREAD** |
| Type-0 mouse | persist `250×30` ×1.6 | MATCH analog area | **MATCH** analog |
| Type-16 hit | **UNREAD** (not `0055B8F0`) | `TryChromeHit` | **LEFTOVER** invented |
| Type-37 hit | parent type 11 `0055B8F0` row | `TryChromeHit` on the edit box | **LEFTOVER** (wrong object) |

**Next site (do not apply here):**

1. Keep dest a point when persist/leftover size 0. Do not grow type-16/37 dest from type-2 chrome. Do not restore `TryMouseAreaDest`.
2. Native `0055B8F0` is type-11/38 extra AABB. Do **not** invent type-16/37 hit. Leave `TryChromeHit` classified leftover.
3. `PlaceTableCell` `n==3` leftover fill stays host. Native cell dest is `00551EA0` clone cursor — unread per-key stack map.
4. Do **not** re-enable `Key.N`. Do not plant dest numbers as a native dump.

Leave leftover #48 open. Leave leftover #36 dest-lock open.

---

## UNREAD sites

- Native first-seen dest 4-tuple
  (`[esp+36..48]` / rec `+12..+24` /
  widget `+248`) on New Profile Present.
- Whether `00539B58` packed key-5 pos is
  copied onto child layout key 0
  (`005385AE` / `005392FC` / `0053AD07` /
  `0053B91E`).
- Exact `00531090` extra numbers on New
  Profile type 11/38.
- Type-16 LMB AABB writer (not `0055B8F0`,
  not `00549440` actions 4–21).
- `00551EA0` per-key cursor for sprites
  `(0,1,4)` after the integer leftover
  divide.

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00400000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00500000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00540000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\export\frontend\new-profile-dests.txt`
- `C:\FableCSharp\proofs\leftover-48-dest\README.md`
- `C:\FableCSharp\proofs\leftover-48-native-hit\README.md`
- `C:\FableCSharp\proofs\leftover-48-chrome-hit\README.md`
- `C:\FableCSharp\proofs\leftover-14-dest-aabb\README.md`
- `C:\FableCSharp\proofs\0041AC20-dest-formula\README.md`
- `C:\FableCSharp\src\Fable.Game\FrontendLayout.cs`
- `C:\FableCSharp\src\Fable.Game\FrontendHitTest.cs`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\src\Fable.Formats\Defs\FrontendUiDef.cs`
- `C:\FableCSharp\docs\status\README.md`
- `C:\FableCSharp\implementer\frontend\02-layout.md`
