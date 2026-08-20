# Leftover #48 remaining — native Accept hit, no type-16/37 invent

Investigation only. No production `src/` or `tests/`
edits. Do **not** invent dest AABB. Do **not**
invent type-16/37 hit. Do **not** restore
`TryMouseAreaDest`. Do **not** delete
`TryChromeHit`. Do **not** re-enable `Key.N` /
`ActivateNewGame`.

Question: recover native hit for New Profile
**Accept** without inventing type-16/37. What
still keeps leftover #48 open after
`0055BF10` hover / Accept `0x126` **MATCH**?

Authority: dump only.
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
(`0055B8F0` / `0055BF10` / `0055ACB0` /
`0055B890` / `00551EA0` / `005491A0` /
`00540CF0`);
`listing-00500000.txt` (`00531090` /
`0052EEC0` / `0052ECC0`);
`e8.tsv` (**no** `.text` `E8 0055B8F0`;
**no** `.text` `E8 0055BF10`; **no**
`.text` `E8 00551EA0`);
ExeIndex `vtbl` `0124B04C` / `01249554` /
`01248A8C` / `01246B8C`;
`export/frontend/new-profile-dests.txt`;
`src/Fable.Game/FrontendHitTest.cs`;
`src/Fable.Game/FrontendLayout.cs`
(`NativeHitFn` / `TryChromeHitIsNativeHit=false` /
`NativeHitWalksRightmostType2=false` /
`PlaceTableCellCount3IsNative=false` /
`Type12DestIsPointWhenSizeZero=true` /
`NativeDestTupleUnread=true`);
`src/Fable.Game/EngineLifecycle.cs`
(`AssignHitRects` / `TryChromeHit` /
`TickType11Type38Hover` / persist-size skip
before `PlaceTableCell`);
`proofs/leftover-48-chrome-hit`;
`proofs/leftover-48-dest`;
`proofs/leftover-48-native-hit`;
`proofs/leftover-48-native-aabb`;
`proofs/leftover-14-hover-before-click`;
`docs/status/README.md` leftover #48
(`681620d` `0055BF10` hover / Accept `0x126`).

Status words: **PROVEN** / **PARTIAL** /
**UNREAD** / **DISPROVEN** / **LEFTOVER** /
**MATCH**.

Do not re-prove persist CRC `0x53C644E4` →
`0x126`, LMB type 4/6 posters, or dest
**formula**. Native dest 4-tuple stays
**UNREAD**.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| Native Accept hit without type-16/37 invent? | Type-38 `vtbl+580` `0055BF10` → `vtbl+568` `0055B8F0`: dest **origin** + dest **scale** × `+176` extra. Extra includes type-0 `UI_HELPER_BUTTON_MOUSE_AREA` persist `250×30`. Dest of Accept stays a **point** | **PROVEN** listing; extra numbers **UNREAD** |
| Invent dest AABB onto Accept? | **No.** `0041AFA0` persist 0 leftover 0 → POINT. `TryMouseAreaDest` dest-copy **removed** `e3208eb` | **DISPROVEN** as dest write |
| Invent type-16/37 hit for Accept? | **No.** Type 38 is **not** in `TryChromeHit`. Type 16 `+568` is `005491A0`. Type 37 `+568` is `00540CF0` | **DISPROVEN** |
| Accept `0x126` already MATCH? | **Yes.** `681620d` / `New_Profile_hover_0055BF10_swaps_type38_on_off` / `Type4_drives_lifecycle_0xE5_then_0x126_then_15`. Host `ClickNamed` → `TryDestPoint` child dest → `InteractiveAt` type 38 | **MATCH** path |
| `TryChromeHit` still leftover? | Type-16/37 dest origin + rightmost type-2 dest size. `TryChromeHitIsNativeHit=false` | **LEFTOVER** invented |
| `TryMouseAreaDest` dest-copy live? | **No.** Removed `e3208eb`. Dest stays a point | **DISPROVEN** |
| Persist-size skip native? | Host `persistW==0 && persistH==0` before `PlaceTableCell`. No such gate in `00551EA0` | **LEFTOVER** |
| `PlaceTableCell` `n==3` native dest? | **No.** `PlaceTableCellCount3IsNative=false` | **LEFTOVER** |
| Close leftover #48? | **No.** Chrome hit / cell fill / persist skip / dest dump stay host. Do not edit `src/` | **LEFTOVER** open |

---

## Verdict

**Native New Profile Accept hit is type-38
`0055BF10` → `0055B8F0` child extra. It does
not invent type-16/37 hit and does not write
a dest AABB. Accept `0x126` is already
MATCH. Leftover #48 remaining is host
stand-in: `TryChromeHit` still invents
type-16/37 hit; persist-size skip still
host; `PlaceTableCell` `n==3` leftover fill
still host. Dest stays a point.**

`0055BF10` hover / Accept `0x126` is locked
(`681620d`). That lock is **not** dest-lock
and **not** `TryChromeHit`. Host Accept
never enters the helper: type 38 is skipped;
`Hit*` equals dest **point**; `HitIndex` at
`TryDestPoint` walks type-0 mouse-area dest
`579,672,979,720` then `InteractiveAt`.

Native hover AABB is computed, not stored as
dest. Extra empty → empty hit even when dest
has area. Accept’s type-0 child has persist
W/H, so extra is nonempty and the **point**
dest still hits. Do **not** copy that AABB
onto Accept dest (`TryMouseAreaDest` was
that dest-copy; it is gone). Do **not** copy
type-2 chrome dest size onto type-16/37 and
call it native Accept hit.

**Answer:** recover Accept via type-38 extra
AABB. Leave dest a point. Leave
`TryChromeHit` classified leftover. Leave
persist-size skip host. Leave #48 open.

| Claim | Class |
| --- | --- |
| Type 38 `vtbl+580` is `0055BF10` | **PROVEN** rdata |
| Type 38 `vtbl+568` is `0055B8F0` | **PROVEN** rdata |
| Type 16 `vtbl+568` is `0055B8F0` | **DISPROVEN** (`005491A0`) |
| Type 37 `vtbl+568` is `0055B8F0` | **DISPROVEN** (`00540CF0`) |
| `0055B8F0` AABB = origin + scale × `+176` extra | **PROVEN** |
| `0055B8F0` walks rightmost type-2 dest size | **DISPROVEN** |
| `TryChromeHit` is native `0055B8F0` | **DISPROVEN** (`TryChromeHitIsNativeHit=false`) |
| New Profile Accept needs `TryChromeHit` | **DISPROVEN** |
| Type-38 dest is a point | **PROVEN** dump / formula |
| Type-0 mouse-area dest has area | **PROVEN** dump analog persist `250×30` × `1.6` |
| `TryMouseAreaDest` dest-copy is live | **DISPROVEN** (removed `e3208eb`) |
| Dest-AABB-only (`6e76ac5`) recovers native Accept hit | **DISPROVEN** (Accept already hits via child dest; type-16/37 regress) |
| Persist-size skip is `00551EA0` | **DISPROVEN** |
| `PlaceTableCell` `n==3` is native dest | **DISPROVEN** |
| Accept LMB `0x126` | **MATCH** (`681620d`) |
| Native dest 4-tuple dump | **UNREAD** (`NativeDestTupleUnread=true`) |
| Exact `00531090` extra numbers on Accept | **UNREAD** |

---

## 1. Native Accept hit (no type-16/37, no dest write)

`listing-00540000.txt`. Type 38 vtbl
`0124B04C`: `+4` tick `0055ACB0`, `+580`
hover `0055BF10`, `+568` hit `0055B8F0`.
`.text` `E8` of both hover and hit is empty.
Dispatch is vtbl only.

`0055ACB0` jmp `0055B890` → `vtbl+580`
when persist-target has area **or** point
dest `+48` vs dt (`leftover-14-hover-before-click`).
Accept persist W/H 0 is a **point dest**;
take of `+48` is **PARTIAL**. Do not treat
persist width as dest AABB.

`0055BF10`:

```
call 0041E5F2
test [input+164]; jne leave
push 25
call [inner.vtbl+8]            ; 0052D900 contains(25)
je  leave
; if [input+184] type-32: vtbl+64 / +92 → [esp+32]
test [esi+352]; jne already
call [vtbl+568]                ; 0055B8F0 AABB of [esp+32]
je  fail
… peer walk 0x13B8AD4 …
0055C0DE  mov [esi+352], 1
```

`0055B8F0`:

```
call [eax+488]                 ; origin  0052EEC0
call [edx+492]                 ; scale   0052ECC0
call [edx+96]                  ; extra   00531090 +176 kids
left  = fistp(origin.x + extra[0])
top   = fistp(origin.y + extra[1])
right = fistp(origin.x + scale.x * extra[2])
bot   = fistp(origin.y + scale.y * extra[3])
hit iff left <= x < right && top <= y < bot
al = 1 / 0
ret 4
```

No type compare. No sibling type-2 dest
copy. Empty `+176` → extra `0,0,0,0` →
miss. `NativeHitWalksRightmostType2=false`.

Dump (`new-profile-dests.txt`):

```
UI_ACCEPT_NEW_PROFILE            t=38 dest=579,672,579,672  +204=0  msg=294
  UI_HELPER_BUTTON_MOUSE_AREA    t=0  dest=579,672,979,720  +204=64
  UI_SPRITE_ACCEPT_ON            t=0  dest=579,646,989,749  +204=256
  UI_SPRITE_ACCEPT_OFF           t=0  dest=579,646,989,749  +204=256
```

Persist (`leftover-48-native-aabb`): Accept
`(362,420)` W/H `0,0` → formula POINT
`(362,420)×1.6` = `579,672`. Mouse-area
persist `250×30` at Accept origin → dest
`400×48` = `579,672,979,720` **has area**.
That child is on type-38 `+176`, so extra
is nonempty. Hover AABB is that extra,
**not** a dest field and **not** type-16/37
`Hit*`.

Type 16/37 never own `0055B8F0`. They are
not this Accept site.

Host `TickType11Type38Hover` Notes
`0055ACB0 vtbl+580 0055BF10` then
`Hovered = Contains \|\| HitIndex == i`.
`Contains` on Accept dest **point** is
false. `HitIndex` at the mouse-area dest
returns type 38 via `InteractiveAt`. Effect
**MATCH** on Accept LMB; formula
**LEFTOVER** (dest AABB / `Hovered`, not
`+352` extra AABB). `#14` still owns that
apply gap. This leftover owns the **size
invent** that is **not** on Accept.

`FrontendHitTest` comments that still say
`0055B8F0` is dest origin + dest **size**
are **STALE** vs listing (origin + **scale**
× extra). Investigation only — do not edit
`src/`.

---

## 2. Do not invent dest AABB

`0041AFA0`: persist W/H else leftover
`+204/+208`. Persist 0 and leftover 0 →
**POINT**. Type-38 GraphicIndex 0 → leftover
0. Dest analog `579,672,579,672` **MATCH**
host dump. Native 4-tuple dump
(`[esp+36..48]` / rec `+12..+24`) **UNREAD**.
Do not plant those numbers as dest-lock
(`leftover-48-formula-analog`).

`TryMouseAreaDest` copied type-0 dest onto
type-38 dest. Removed in `e3208eb`. Host
Accept `Hit*` equals dest point
(`New_Profile_apply_cancel_hit_rects_are_disjoint`
locks `Hit==Dest` on apply/cancel). Do
**not** restore dest-copy.

`ExpandControlDests` (type-16/37 dest grow)
deleted in `68eb1c5`. Dest of type-12/16/37
stays a point when persist/leftover size 0
(`Type12DestIsPointWhenSizeZero=true`).

Dest-AABB-only hit (`6e76ac5`) is the
honest dest rule: point dests miss.
Accept still works because the **child**
dest has area. Restoring dest invent on
Accept would plant dest AABB. Do not.

---

## 3. Do not invent type-16/37 hit

Host `AssignHitRects`:

```
Hit* = dest AABB
if dest has no area:
  TryChromeHit(type 16/37 only)
```

`TryChromeHit` walks up to the type-12
list, finds the rightmost type-2 dest
under that row, writes

```
hit = (destX0, destY0, destX0 + type2W, destY0 + type2H)
```

Type 38 is **not** in the helper. Accept
does not use it.

Type-16 host hit `608,293,800,325` from
RIGHT dest `192×32` is leftover
(`leftover-48-chrome-hit`). Type-37 host
hit is the same helper on the **wrong
object** (native row hit is parent type 11
`0055B8F0`). `(700,300)` is a host lock on
that invented type-16 rect. Tests
`New_Profile_apply_cancel_hit_rects_are_disjoint`
/ `New_Profile_per_control_LMB_uses_dest_not_empty_space`
need the helper today. Leave it. Do not
call it native Accept hit. Do not replace
it with dest-AABB-only (regresses
`(700,300)`).

`TryChromeHitIsNativeHit=false`.

---

## 4. Persist-size skip still host

`LayoutFrontendWidgets`:

```
spriteClone = persistW == 0 && persistH == 0
if parent is TableType && spriteClone:
  PlaceTableCell(n==3 leftover fill)
```

Listing `00551EA0` has no persist-size
gate and no `cmp count, 3`. Native walks
`+348` templates and writes clone layout
`+8/+12` from a cursor. `.text` `E8
00551EA0` empty. Host never constructs
those clones (`type2-row-dest`).

The skip is extra host heuristic on the
same helper — leftover **#48 family**, not
a new leftover (`docs/status` leftover
#48). Mouse-area persist `250×30` happens
to skip `PlaceTableCell` and fall through
to generic dest / inherit-scale
`005339B0`. That **effect** MATCH analog
for the mouse-area dest; the **gate** is
still invented.

`PlaceTableCell` `n==3` left/right leftover
W + middle fill stays host
(`PlaceTableCellCount3IsNative=false`).
Type-2 dest **height** 32 on the dump is
that fill / `ExpandTableDests`, not persist
Height (0). Not Accept hit.

---

## 5. Host vs native on Accept click

First-seen no-save (`playable-path-now`
step 9):

| Step | Native | Host | `TryChromeHit`? |
| --- | --- | --- | --- |
| Hover Accept | `00599E3F` `vtbl+4` `0055ACB0` → `0055BF10` → `0055B8F0` extra | `TickType11Type38Hover` `Contains \|\| HitIndex`; `Hovered` stand-in for `+352` | **no** |
| LMB-down action 26 | `0055CB10(26)` reads `+352` | `ArmType34Widgets` requires `Hovered` | **no** |
| LMB-up action 28 | posts widget `+228` = persist `0x126` | Type6 `MessageFromPlus228List` | **no** |
| Type-16 `(700,300)` | **UNREAD** | invented `Hit*` | **yes, host tests only** |

`ClickNamed("UI_ACCEPT_NEW_PROFILE")`
uses `TryDestPoint` first presented
descendant dest **with area** — the type-0
mouse area — then TypeMouse / Type4 /
Type6. Empty space `(12,12)` stays null.
That path **MATCH** `0x126` without dest
invent and without type-16/37 invent.

---

## Gap

```
Evidence              Original                         Host                            Gap
0055BF10 / 0055B8F0   type 11/38 extra AABB; al        Contains||HitIndex; Hit=dest    path MATCH on Accept;
                      no dest store                    point; Hovered for +352         formula leftover #14
type-38 dest          POINT persist 0 leftover 0       POINT 579,672                   analog MATCH; dump UNREAD
type-0 mouse dest     persist 250×30 × scale           579,672,979,720                 analog MATCH
TryMouseAreaDest      never a dest writer              removed e3208eb                 do not restore
TryChromeHit          not 0055B8F0; type 16/37 UNREAD  dest origin + rightmost type-2  leftover #48 invented
persist-size skip     not in 00551EA0                  persistW==0 && persistH==0      leftover #48 host gate
PlaceTableCell n==3   00551EA0 clone cursor            leftover W caps + mid fill      leftover #48 dest
```

| Object | Native | Host | Gap |
| --- | --- | --- | --- |
| Type-38 Accept hit | `0055B8F0` origin + scale × `+176` | child dest walk; Hit=point | path **MATCH**; extra **UNREAD** |
| Type-38 dest | POINT | POINT | **MATCH** analog; dump **UNREAD** |
| Type-16 hit | **UNREAD** | `TryChromeHit` | **LEFTOVER** invented |
| Type-37 hit | parent type 11 `0055B8F0` row | `TryChromeHit` on the edit box | **LEFTOVER** (wrong object) |
| Persist-size skip | none in `00551EA0` | host gate | **LEFTOVER** |
| `PlaceTableCell` `n==3` | **DISPROVEN** | leftover fill | **LEFTOVER** dest |
| Accept `0x126` | type 38 `+228` after hover | `ClickNamed` | **MATCH** |

**Next site (do not apply here):**

1. Keep dest a point when persist/leftover
   size 0. Do **not** restore
   `TryMouseAreaDest`. Do **not** grow
   Accept dest from the mouse-area dest.
2. Native Accept hit is type-38 extra
   AABB. Do **not** invent type-16/37 hit
   to recover it. Leave `TryChromeHit`
   classified leftover (`(700,300)` host
   lock).
3. Persist-size skip / `PlaceTableCell`
   `n==3` stay host. Native cell dest is
   `00551EA0` clone cursor — unread
   per-key stack map.
4. Do **not** plant dest 4-tuples as a
   native dump. Do **not** re-enable
   `Key.N`.

Leave leftover #48 open.

---

## UNREAD sites

- Exact `00531090` extra numbers on New
  Profile type 38 (persist W vs leftover
  64 on `UI_HELPER_BUTTON_MOUSE_AREA`;
  authored vs dest-space unit on extra[0]
  vs extra[2] × scale).
- Native dest 4-tuple
  (`[esp+36..48]` / rec `+12..+24`).
- Type-16 LMB AABB writer (not
  `0055B8F0`, not `00549440` actions
  4–21).
- `00551EA0` per-key cursor for sprites
  `(0,1,4)`.
- Whether type-11 row hit then focuses
  type 37 (`00540120` actions 33/34).

---

## Sources

- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00540000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00500000.txt`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\e8.tsv`
- `C:\FableCSharp\export\frontend\new-profile-dests.txt`
- `C:\FableCSharp\proofs\leftover-48-chrome-hit\README.md`
- `C:\FableCSharp\proofs\leftover-48-dest\README.md`
- `C:\FableCSharp\proofs\leftover-48-native-hit\README.md`
- `C:\FableCSharp\proofs\leftover-48-native-aabb\README.md`
- `C:\FableCSharp\proofs\leftover-14-hover-before-click\README.md`
- `C:\FableCSharp\src\Fable.Game\FrontendHitTest.cs`
- `C:\FableCSharp\src\Fable.Game\FrontendLayout.cs`
- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\tests\Fable.Formats.Tests\FrontendLayoutTests.cs`
- `C:\FableCSharp\tests\Fable.Formats.Tests\FrontendInputTests.cs`
- `C:\FableCSharp\docs\status\README.md`
