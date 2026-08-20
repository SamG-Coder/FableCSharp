# Leftover #48: `TryChromeHit` (native chrome vs host)

Investigation only. No production `src/` edits.
Do **not** delete `TryChromeHit`. Do **not** invent
a WASD / hit heuristic. Dest of type-12/16/37
stays a point (`leftover-48-dest`).

Question: what is `TryChromeHit`, when does
first-seen no-save need it, dest analog? Status.

Authority: dump only.
`src/Fable.Game/EngineLifecycle.cs`
(`AssignHitRects` / `TryChromeHit` /
`ApplyTextSliderHit`);
`src/Fable.Game/FrontendHitTest.cs`
(`HitRect` / `HitIndex` / `TryDestPoint` /
`InteractiveAt`);
`src/Fable.Game/FrontendLayout.cs`
(`TryChromeHitIsNativeHit=false` /
`NativeHitWalksRightmostType2=false` /
`PlaceTableCellCount3IsNative=false` /
`Type12DestIsPointWhenSizeZero=true`);
`export/frontend/new-profile-dests.txt`;
`export/frontend/main-menu-dests.txt`;
`export/frontend/press-start-dests.txt`;
`proofs/leftover-48-dest`;
`proofs/leftover-48-native-hit`;
`proofs/leftover-14-present-dest`;
`proofs/audit-playerinterface`;
`tests/Fable.Formats.Tests/FrontendLayoutTests.cs`
(`New_Profile_apply_cancel_hit_rects_are_disjoint`);
`tests/Fable.Formats.Tests/FrontendInputTests.cs`
(`New_Profile_per_control_LMB_uses_dest_not_empty_space`);
`tools/Fable.ExeIndex/out/01-sections/text-map/listing-00540000.txt`
(`0055B8F0` / `0055BF10` / `00551340` /
`00551EA0` / `005491A0` / `00540CF0`).

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

---

## Verdict

**`TryChromeHit` is host leftover size-invent for
type-16/37 hit when dest is a point.** It is
**not** native chrome and **not** `0055B8F0`.

Native chrome is the type-2 table dest from
`00551340` leftover `+204/+208` = persist W/H
(`leftover-48-dest`). That dest already has
area. Native hit for type **11/38** is
`vtbl+568` `0055B8F0`: dest **origin**
(`vtbl+488`) plus dest **scale** (`vtbl+492`)
times `+176` child extra (`vtbl+96`). Native
does **not** walk siblings or copy the
rightmost type-2 dest size
(`NativeHitWalksRightmostType2=false`).
Type-16 `+568` is `005491A0` (0-arg `+404`
copy). Type-37 `+568` is `00540CF0` (string
get). Neither is a point-in-rect.

Host `AssignHitRects`: dest AABB if dest has
area, else `TryChromeHit` for type 16/37
**only**. Helper walks up to the type-12 list,
finds the rightmost type-2 dest under that
row, and writes

```
hit = (destX0, destY0, destX0 + type2W, destY0 + type2H)
```

onto the type-16/37 `Hit*`. Dest stays a
point. `TryChromeHitIsNativeHit=false`.

**First-seen no-save does not need it** to
post Press Start `0xE5` / Accept `0x126` /
New Game 15. Those clicks never enter the
helper.

**Dest analog:** leftover #48 dest recovered
that type-12/16/37 dest is a **point** when
persist W/H and leftover `+204` are 0. Dest
leftover sibling is `PlaceTableCell` `n==3`
(invents type-2 **cell dest**). Hit leftover
sibling is `TryChromeHit` (invents type-16/37
**hit** size from that chrome dest).
`TryMouseAreaDest` dest-copy was removed in
`e3208eb` — dest stays a point; do not
restore dest invent. `6e76ac5` dest-AABB-only
regresses `(700,300)`.

**Do not invent WASD / hit heuristic.**
`UI_OPTIONS_TEXT_CONTROL_WASD` is a type-6
**point** dest, first-seen `StyleIndex` 0,
Visible. Frontend input is not WASD
(`audit-playerinterface`). `(700,300)` is a
host lock on invented type-16 hit overlapping
type-2 `UI_BUTTON_OPTIONS_RIGHT` dest, not a
recovered WASD dest or DIK walk.
`ApplyTextSliderHit` left-half / cycle
`ActiveChild` is host.

| Claim | Class |
| --- | --- |
| `TryChromeHit` body is type 16/37 only, dest origin + rightmost type-2 dest size | **PROVEN** host |
| `TryChromeHit` is native `0055B8F0` | **DISPROVEN** (`TryChromeHitIsNativeHit=false`) |
| Native `0055B8F0` walks rightmost type-2 dest size | **DISPROVEN** (`NativeHitWalksRightmostType2=false`) |
| Native type-16 `vtbl+568` is `0055B8F0` | **DISPROVEN** (`005491A0`) |
| Native type-37 `vtbl+568` is `0055B8F0` | **DISPROVEN** (`00540CF0`) |
| Native chrome dest is type-2 leftover `00551340` persist W/H | **PROVEN** (`leftover-48-dest`) |
| Type-16/37 dest is a point | **PROVEN** dump |
| Type-12 dest is a point when persist/leftover size 0 | **PROVEN** (`Type12DestIsPointWhenSizeZero`) |
| First-seen no-save `0xE5` / `0x126` / 15 needs `TryChromeHit` | **DISPROVEN** |
| New Profile Accept needs `TryChromeHit` | **DISPROVEN** (type 38 skipped; child dest walk) |
| Type-16 `(700,300)` needs `TryChromeHit` today | **PROVEN** host test |
| Type-37 nonempty chrome hit needs `TryChromeHit` today | **PROVEN** host test |
| `PlaceTableCell` `n==3` is native dest | **DISPROVEN** (`PlaceTableCellCount3IsNative=false`) |
| `TryMouseAreaDest` dest-copy is live | **DISPROVEN** (removed `e3208eb`) |
| Dest-AABB-only (`6e76ac5`) recovers native hit | **DISPROVEN** (regresses type-16/37) |
| WASD type-6 dest is the type-16 hit | **DISPROVEN** (point dest `704,293`) |
| Frontend LMB / Type4 is WASD / Hero walk | **DISPROVEN** (`audit-playerinterface`) |
| `ApplyTextSliderHit` left-half cycle is native | **UNREAD** (host) |
| Type-16 native LMB AABB writer | **UNREAD** (`leftover-48-native-hit`) |

**Answer:** `TryChromeHit` is leftover #48 hit
stand-in. First-seen no-save New Game does
**not** need it. Dest analog is
`PlaceTableCell` `n==3` / killed
`TryMouseAreaDest`. Leave the helper. Leave
#48 open.

---

## 1. What `TryChromeHit` is

`EngineLifecycle.AssignHitRects` after dest
layout:

```
for each widget:
  Hit* = dest AABB
  if dest has no area:
    TryChromeHit(type 16/37 only)
```

`FrontendHitTest.HitRect` prefers pre-assigned
`Hit*` when it has area, else dest. That
assignment is the leftover feeding
`HitIndex` / `Hovered` (`leftover-14`).

`TryChromeHit` (`EngineLifecycle.cs`):

```
if type != TextSlider && type != EditBox: return
walk ParentIndex until type-12 List (or root)
scan type-2 TableType whose ancestor is that row
keep the rightmost dest with W>0 && H>0
x0,y0 = this dest origin
x1,y1 = origin + that type-2 dest W/H
```

Comment claims leftover #48 and “do not treat
this size as `0055B8F0` recovered.” Constants
lock the same:

| Flag | Value |
| --- | --- |
| `FrontendLayout.NativeHitFn` | `0x0055B8F0` |
| `FrontendHitTest.HitTestFn` | `0x0055B8F0` (claimed; body is not this helper) |
| `TryChromeHitIsNativeHit` | `false` |
| `NativeHitWalksRightmostType2` | `false` |
| `PlaceTableCellCount3IsNative` | `false` |

Native `0055B8F0` (`leftover-48-native-hit`):

```
left  = destOrigin.x + extra[0]
top   = destOrigin.y + extra[1]
right = destOrigin.x + destScale.x * extra[2]
bot   = destOrigin.y + destScale.y * extra[3]
hit iff left <= x < right && top <= y < bot
```

Extra is this widget’s `+176` children. Empty
`+176` → extra `0,0,0,0` → empty hit. Sibling
type-2 tables **not** on `+176` never enter.

---

## 2. Native chrome vs host hit

Dump (`new-profile-dests.txt`):

```
UI_NEW_PROFILE_BUTTON                 t=11 dest=64,240,64,240
  UI_BUTTON_OPTIONS_LEFT              t=2  dest=64,240,352,272   +204=180
  UI_BUTTON_OPTIONS_RIGHT_EDITBOX     t=2  dest=576,235,928,267  +204=220
  UI_NEW_PROFILE_EDIT_BOX             t=37 dest=592,240,592,240  +204=0
UI_OPTIONS_TEXT_SLIDER_WHOLE_CONTROL_METHOD t=5 dest=64,288,64,288
  UI_BUTTON_OPTIONS_LEFT              t=2  dest=64,288,352,320   +204=180
  UI_BUTTON_OPTIONS_RIGHT             t=2  dest=594,283,786,315  +204=120
  UI_OPTIONS_CONTROL_METHOD_TEXT_SLIDER t=16 dest=608,293,608,293 +204=0
    UI_OPTIONS_TEXT_CONTROL_ARROWS    t=6  dest=704,293,704,293  +204=0
    UI_OPTIONS_TEXT_CONTROL_WASD      t=6  dest=704,293,704,293  +204=0
UI_ACCEPT_NEW_PROFILE                 t=38 dest=579,672,579,672
  UI_HELPER_BUTTON_MOUSE_AREA         t=0  dest=579,672,979,720  +204=64
UI_SLIDER_CAMERA_SENSITIVITY          t=15 dest=608,392,634,418  +204=16
```

| Object | Native chrome / hit | Host |
| --- | --- | --- |
| Type-2 LEFT/RIGHT dest | leftover persist W × dest scale (`00551340`) | **MATCH** leftover store; dest W **PARTIAL** |
| Type-16 dest | `0041AFA0` point | **MATCH** point `608,293,608,293` |
| Type-16 hit | **UNREAD** (not `0055B8F0`; `+176` kids are type-6 leftover 0) | **LEFTOVER** `TryChromeHit` → `608,293,800,325` from RIGHT dest `192×32` |
| Type-37 dest | point | **MATCH** `592,240,592,240` |
| Type-37 hit | parent type 11 `0055B8F0` **row** AABB; self slot is string get | **LEFTOVER** same helper on the edit box |
| Type-38 dest | point | **MATCH** |
| Type-38 hit | `0055BF10` → `0055B8F0` extra includes type-0 leftover / persist W | Hit=`dest` point; `HitIndex` at child dest then `InteractiveAt` — **PARTIAL** effect **MATCH**; formula **LEFTOVER** |
| Type-15 knob dest | leftover 16 → dest has area | **MATCH** area; `AssignHitRects` copies dest; no `TryChromeHit` |
| Type-6 WASD dest | point leftover 0 | **MATCH** point. **Not** a hit rect |

`(700,300)` sits in native chrome dest
`UI_BUTTON_OPTIONS_RIGHT` `594,283,786,315`
**and** in invented type-16 hit
`608,293,800,325`. Type 2 is not
`IsInteractive`. Without the invent,
`HitIndex` reverse-walks the type-2 dest,
`InteractiveAt` never reaches type 16 (sibling,
not ancestor) → **null**. That is why the host
test needs the helper.

Do **not** treat the overlapping type-2 dest as
native type-16 AABB. Native type 16 never
owns `0055B8F0`. Type-16 parent is type 5,
which also has no `0055B8F0`.

---

## 3. When first-seen no-save needs it

First-seen no-save frontend (`playable-path-now`
steps 8–13):

| Step | Native poster | Host click | `TryChromeHit`? |
| --- | --- | --- | --- |
| Press Start `0xE5` | type-10 `vtbl+284` packet; Type4/Type6 | Type4/Type6 without dest, or type-10 attach | **no** — type 10 not in the helper |
| New Profile Accept `0x126` | type 38 `0055BF10` then action 26/28 | `ClickNamed` → `TryDestPoint` type-0 mouse-area dest `579,672,979,720` → `InteractiveAt` type 38 | **no** — type 38 skipped; Hit=dest point |
| Main Menu New Game 15 | type 11 `+228` | `ClickNamed` → `TryDestPoint` `UI_BUTTON_MOUSE_AREA` dest `96,309,496,357` → type 11 | **no** — type 11 dest is a point; Hit=dest; child dest walk |
| Type-16 control-method LMB | **UNREAD** | `(700,300)` / nonempty chrome hit | **yes, host tests only** |
| Type-37 edit-box chrome | parent type 11 `0055B8F0` | nonempty `HitRect` | **yes, host tests only** |
| Type-15 sensitivity knob | dest has area | `ClickIndex` dest midpoint | **no** — dest AABB |

`ClickNamed` / `TryDestPoint` walks the first
presented descendant dest that has **area**.
Accept and New Game already have a type-0
mouse-area dest. That path does **not** read
type-16/37 `Hit*`.

Empty space `(12,12)` and left-column
`(96,304)` stay null — tests lock that
without the helper inventing those.

Deleting `TryChromeHit` breaks
`New_Profile_apply_cancel_hit_rects_are_disjoint`
(nonempty type-16/37 hit) and
`New_Profile_per_control_LMB_uses_dest_not_empty_space`
(`(700,300)` flips `ActiveChild`). It does
**not** break `0xE5` / `0x126` / 15.

Typing a custom name is not a recovered
keyboard poster (`EditBoxTypesFromDik=false`).
Not this leftover.

---

## 4. Dest analog

Leftover #48 dest (`leftover-48-dest`):

```
0041AFA0:
  w = persistW != 0 ? persistW : leftover204
  h = persistH != 0 ? persistH : leftover208
  persist 0 && leftover 0 → dest POINT
```

Type-12 `UI_NEW_PROFILE_MENU` dest
`64,240,64,240` **MATCH**. Type-16/37 same.
Do **not** grow dest from type-2 children.
`ExpandTableDests` / `TryChromeHit` are not
this widget.

| Analog | Writes | Native | Host leftover |
| --- | --- | --- | --- |
| dest size | dest `X0,Y0,X1,Y1` | `0041AFA0` persist else leftover; 0 → point | `PlaceTableCell` `count==3` invents type-2 **cell dest** from leftover W (`PlaceTableCellCount3IsNative=false`) |
| dest copy onto type 38 | dest of Accept | dest stays a point | `TryMouseAreaDest` **removed** `e3208eb` |
| persist-size skip | who runs `PlaceTableCell` | no such gate in `00551EA0` | `persistW==0 && persistH==0` before the helper — extra host heuristic, same family |
| hit size | `Hit*` | type 11/38 `0055B8F0` origin + scale × `+176`; type 16/37 **UNREAD** / not AABB | `TryChromeHit` dest origin + rightmost type-2 dest size |

`PlaceTableCell` invents **dest** of type-2
sprite cells (caps/stretch). `TryChromeHit`
invents **hit** of type-16/37 without changing
dest. Same leftover family, different store.
Do not collapse them. Do not restore
`TryMouseAreaDest`. Do not replace
`TryChromeHit` with dest-AABB-only
(`6e76ac5`).

---

## 5. Do not invent WASD / hit heuristic

`UI_OPTIONS_TEXT_CONTROL_WASD` first-seen:

```
t=6 dest=704,293,704,293 +204=0
Visible=true
StyleIndex=FirstSeenState (0)
Colour == ARROWS
LeafDipSkipped
```

`ARROWS` is `TextSliderFirstSeenSelect` (3).
Both type-6 dests are the **same point**.
Neither has area. `HitIndex` never returns
WASD. Tests lock StyleIndex + colour, **not**
a WASD dest tuple.

`(700,300)` → type-16 slider because
`TryChromeHit` copied RIGHT table dest size
onto the type-16 origin. That is **not**:

- a recovered WASD click dest
- a DIK_W/A/S/D frontend bind
- `0042E3EE` player walk (`audit-playerinterface`:
  frontend Type4 is not WASD)
- native type-16 `vtbl+568`

`ApplyTextSliderHit` uses `IsLeftHalf` on the
**invented** `HitRect` then cycles
`ActiveChild`. Native type-16 inner
`00549440` switches actions 4–21, not 25/26.
Who maps LMB onto type 16 is **UNREAD**
(`leftover-48-native-hit`). Until that site
is recovered, leave the helper. Do not add a
new WASD / chrome / midpoint heuristic.

Host F2 `FlyCamera` WASD is debug leftover,
not this frontend.

---

## Gap

| Object | Native | Host | Gap |
| --- | --- | --- | --- |
| Type-2 chrome dest | `00551340` leftover persist W | leftover store + `PlaceTableCell` n==3 cells | dest **PARTIAL**; n==3 **LEFTOVER** |
| Type-16 dest | point | point | **MATCH** |
| Type-16 hit | **UNREAD** | `TryChromeHit` | **LEFTOVER** invented |
| Type-37 dest | point | point | **MATCH** |
| Type-37 hit | parent type 11 `0055B8F0` | `TryChromeHit` on the edit box | **LEFTOVER** (wrong object) |
| Type-38 Accept | `0055B8F0` child extra | child dest walk, Hit=point | path **MATCH**; formula **LEFTOVER** |
| First-seen `0xE5`/`0x126`/15 | type-10 packet / type 38 / type 11 `+228` | Type4/Type6 + `ClickNamed` | **does not use `TryChromeHit`** |
| `(700,300)` | **UNREAD** as native type-16 | `HitIndex` → slider | keep helper |
| WASD type-6 | point dest, unselected | same | **MATCH** dest; **DISPROVEN** as hit |

**Next site (do not delete `TryChromeHit`):**

1. Keep dest a point. Do not restore
   `TryMouseAreaDest`. Do not grow type-16/37
   dest from type-2 chrome.
2. Type-11 `UI_NEW_PROFILE_BUTTON` `0055B8F0`
   child-union is the **row** hit. Do not
   write that rect onto type-37 dest. Do not
   treat it as type-16.
3. Type-16 mouse: who calls inner `00549440`
   / `vtbl+560` `00548760` (action 25). Not
   25/26 in the switch. Until that maps LMB
   to the slider, leave `TryChromeHit`.
4. Do **not** replace `TryChromeHit` with
   dest-AABB-only (`6e76ac5`). Do **not**
   invent a WASD dest / DIK / left-half
   heuristic as native.

---

## UNREAD sites

- Type-16 LMB → `+348` writer (not
  `0055B8F0`, not `00549440` actions 4–21).
- Exact `00531090` extra numbers on New
  Profile type 11/38.
- Whether type-11 row hit then focuses
  type 37 (`00540120` actions 33/34).
- Native `ApplyTextSliderHit` analog
  (`IsLeftHalf` / `ActiveChild` cycle).

---

## Sources

- `C:\FableCSharp\src\Fable.Game\EngineLifecycle.cs`
- `C:\FableCSharp\src\Fable.Game\FrontendHitTest.cs`
- `C:\FableCSharp\src\Fable.Game\FrontendLayout.cs`
- `C:\FableCSharp\export\frontend\new-profile-dests.txt`
- `C:\FableCSharp\export\frontend\main-menu-dests.txt`
- `C:\FableCSharp\export\frontend\press-start-dests.txt`
- `C:\FableCSharp\proofs\leftover-48-dest\README.md`
- `C:\FableCSharp\proofs\leftover-48-native-hit\README.md`
- `C:\FableCSharp\proofs\leftover-14-present-dest\README.md`
- `C:\FableCSharp\proofs\audit-playerinterface\README.md`
- `C:\FableCSharp\tools\Fable.ExeIndex\out\01-sections\text-map\listing-00540000.txt`
- `C:\FableCSharp\tests\Fable.Formats.Tests\FrontendLayoutTests.cs`
- `C:\FableCSharp\tests\Fable.Formats.Tests\FrontendInputTests.cs`
