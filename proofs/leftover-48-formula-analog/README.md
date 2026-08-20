# Leftover #48: New Profile dest formula analog vs tests

Investigation only. Production `src/` and `tests/` were
not edited. Do **not** plant dest. Do **not** invent
type-16/37 hit. Do **not** re-enable `Key.N` /
`ActivateNewGame`.

Question: can any of the leftover-48-native-aabb
POINT tuples be locked as **host analog tests**
WITHOUT claiming a native dest dump? If tests
already lock them, report **MATCH**. If locking
would invent dest numbers, leave open.

Tuples in question:

| Object | Formula analog |
| --- | --- |
| Type-12 list | POINT `(64,240,64,240)` from persist `(40,150)` × dest scale `1.6` |
| Type-38 Accept | POINT `(579,672)` from persist `(362,420)` × `1.6` |
| Type-0 mouse-area | `579,672,979,720` from persist `250×30` × `1.6` at Accept origin |

Native dest AABB dump still **UNREAD**.
`TryChromeHitIsNativeHit=false`.

Authority: dump only.
`proofs/leftover-48-native-aabb`;
`proofs/leftover-48-dest`;
`proofs/leftover-48-native-hit`;
`proofs/leftover-48-chrome-hit`;
`export/frontend/new-profile-dests.txt`;
`src/Fable.Game/FrontendLayout.cs`;
`tests/Fable.Formats.Tests/FrontendLayoutTests.cs`;
`tests/Fable.Formats.Tests/FrontendInputTests.cs`;
`tests/Fable.Formats.Tests/FrontendFrameDumpTests.cs`;
`tests/Fable.Formats.Tests/EngineLifecycleTests.cs`;
`tests/Fable.Formats.Tests/FrontendUiDefTests.cs`;
`docs/status/README.md` leftover #48.

Status words: **PROVEN** / **PARTIAL** / **UNREAD** /
**DISPROVEN** / **LEFTOVER** / **MATCH**.

Do not re-prove `0055B8F0` vs `TryChromeHit`,
`PlaceTableCell` `n==3`, or GraphicIndex leftover.

---

## Direct answers

| Question | Answer | Class |
| --- | --- | --- |
| Native dest AABB 4-tuple dump? | **Still none.** `NativeDestTupleUnread=true` | **UNREAD** |
| Persist `(40,150)` + screen remap-size 1 → dest scale `1.6`? | File recover (`leftover-48-native-aabb`). Tests do **not** assert list `PositionX/Y` | **PROVEN** file; **UNLOCKED** in tests |
| Live widget dest POINT `(64,240,64,240)` locked? | **No.** Synthetic `ComputeSubmitDest` at origin `64,240` scale **1** only | **DISPROVEN** as a live dest lock |
| Live Accept POINT `(579,672)` locked? | **No.** Tests lock Hit=dest / `TryDestPoint` / hover / `0x126`, not numbers | **UNLOCKED** |
| Live mouse-area `579,672,979,720` locked? | **No.** `TryDestPoint` uses area dest without asserting the 4-tuple | **UNLOCKED** |
| Can we add those 4-tuple asserts as host analog without claiming native dump? | **No.** Status leftover #48 already: tests lock pack / tiling / chrome hit / `(700,300)` / hover tautology, **not dest numbers**. New dest equals would plant dest | **LEFTOVER** open |
| `TryChromeHitIsNativeHit=false` already locked? | **Yes** (`Submit_uses_leftover_when_persist_size_is_0` / `EngineLifecycleTests`) | **MATCH** |
| Close leftover #48? | **No.** Do not edit `src/` / `tests/` | **LEFTOVER** open |

---

## Verdict

**Leave the three dest 4-tuples open. Do not add
tests that assert them. Existing tests MATCH the
formula *class* (persist 0 + leftover 0 → POINT;
`TryChromeHitIsNativeHit=false`), not the New
Profile dest numbers.**

Native dump of `[esp+36..48]` / rec `+12..+24`
is still **UNREAD** (`leftover-14-dest-aabb` /
`NativeDestTupleUnread`). Host dump
`export/frontend/new-profile-dests.txt` is
`LayoutFrontendWidgets` analog. Formula analog
MATCH vs that dump is **proof**, not a test lock.

Locking live dest `(64,240,64,240)` /
`(579,672)` / `579,672,979,720` would plant
dest numbers the same way Press Start
`(512,384,512,384)` is leftover **#36** analog.
Status leftover #48 already forbids dest-number
locks. Prefer no `src`.

**Answer:** none of those POINT tuples are
already locked as live dest. The one existing
`(64,240,64,240)` assert is a submit-dest
tautology at scale **1**, not persist
`(40,150)×1.6`. Leave dest tuples open. Leave
#48 open.

| Claim | Class |
| --- | --- |
| `ComputeSubmitDest(0,0,0,0, 64,240, 1,1)` → POINT `(64,240,64,240)` | **MATCH** formula identity; scale **1**, not 1.6 |
| `Type12DestIsPointWhenSizeZero=true` | **MATCH** |
| `TryChromeHitIsNativeHit=false` | **MATCH** |
| `NativeDestTupleUnread=true` / `SubmitDestStoresOnWidget=false` | **MATCH** |
| Live `UI_NEW_PROFILE_MENU` dest `(64,240,64,240)` | **UNLOCKED** |
| Persist list `(40,150)` / Accept `(362,420)` / mouse `250×30` | **PROVEN** file; **UNLOCKED** in tests |
| Live Accept dest `(579,672)` | **UNLOCKED** |
| Live mouse-area dest `579,672,979,720` | **UNLOCKED** |
| Type-16/37 dest is a **point** (`X0==X1`) | **MATCH** (no numbers) |
| LEFT `DestY0==240` | **LEFTOVER** locator, not list dest lock |
| New dest-tuple tests as host analog | **DISPROVEN** — would plant dest |
| Native dest dump | **UNREAD** |
| `Key.N` native New Game | **DISPROVEN** |

---

## 1. What tests already lock

### Formula class (no New Profile dest numbers)

`FrontendLayoutTests.Submit_uses_leftover_when_persist_size_is_0`:

```
ComputeSubmitDest(0, 0, 0, 0, 64, 240, 1, 1)
  == (64, 240, 64, 240)
Type12DestIsPointWhenSizeZero == true
TryChromeHitIsNativeHit == false
PlaceTableCellCount3IsNative == false
NativeHitWalksRightmostType2 == false
```

That is dest size 0 at an **already written**
origin, dest scale **1**. It is **not**
`Compute(persist 40,150, parent scale 1.6)`.
Same file `Dest_writers_are_0041AFA0_stack_not_0041AC20`
repeats the 64,240 point and locks
`NativeDestTupleUnread=true`.

`Children_inherit_parent_dest_scale` uses a
synthetic parent scale **2**, not 1.6.

`Press_Start_root_remapSize_scales_child_origin_to_viewport`
locks Press Start `s=1` → dest scale
`1024/640=1.6` and type-6 POINT `512,384`.
That is leftover **#36** dest analog, not New
Profile. `Frontend_PRESS_START_is_type_10…`
locks live `FrontendScaleX == 1024/640`.

### New Profile live widgets (no dest 4-tuples)

`New_Profile_type12_rows_use_persist_plus326_not_equal_Y`:

| Assert | Dest numbers? |
| --- | --- |
| list type 12, `Plus326=30`, `Plus322=0`, 4 kids | persist spacing only |
| `ListChildAuthoredPos` pack formula | host leftover pack |
| four distinct `DestY0` | **not** the values |
| slider / edit `DestX0==DestX1` | POINT **class** |
| LEFT leftover `(180,0)` | persist W/H |
| LEFT `DestY0==240` **finder** | locates a row; does not assert list dest |
| cell tiling `capL/stretch/capR` | relative, not dest tuples |

`New_Profile_apply_cancel_hit_rects_are_disjoint` /
`New_Profile_hover_0055BF10_swaps_type38_on_off`:
`TryDestPoint` then Hit=dest / hover /
`(700,300)` chrome hit. **No** `579` / `672` /
`979` / `720`.

`FrontendUiDefTests` Accept: type 38, MessageId
`0x126`. **No** persist `(362,420)`.

`Frontend_dumps_press_start_new_profile_main_menu_after_avi_skip`:
title dest is a point (`X0==X1`), leftover 204
= 0. Writes `new-profile-dests.txt`. **No**
assert of those dump numbers.

`FrontendFrameDumpTests`: names present, four
row Ys, Hit=dest when dest has area. Empty
click `(12,12)` null.

Status leftover #48: “Tests lock pack formula /
tiling / nonempty chrome hit / click `(700,300)`
/ hover tautology, **not dest numbers**.”

---

## 2. Formula analog (proof, not a test)

`leftover-48-native-aabb` persist +
`0041AFA0` / `0052FFD0` / `0052F5C0`:

```
screen s=1, persist scale 1, 1024×768
  destScale = 1024/640 = 1.6

list persist (40,150) o=0 leftover 0
  dest = (64, 240, 64, 240)     POINT

Accept persist (362,420) leftover 0
  dest = (579.2 snap 579, 672, 579, 672)  POINT

mouse persist (0,0) WH 250×30 under Accept
  dest = (579, 672, 979, 720)   HAS AREA
```

Host dump **MATCH** those analogs. Native
4-tuple dump **UNREAD**. Do not copy dump
numbers into tests and call them native dest.

List persist `(40,150)` is **not** in
`FrontendUiDefTests` / `FrontendLayoutTests`
(only `Plus326` / `Plus322`). Adding persist
Position asserts would be file recover, not
dest plant — still **not** this leftover’s
POINT tuples, and not required here. Prefer
no `src`.

---

## 3. Why new dest-tuple tests would plant dest

A new test:

```
list.Dest == (64,240,64,240)
accept.Dest == (579,672,579,672)
mouse.Dest == (579,672,979,720)
```

would lock **host** `LayoutFrontendWidgets`
output. That store has no listing dest AABB
writer (`SubmitDestStoresOnWidget=false`).
Sibling Press Start `512,384,512,384` is the
same leftover **#36** pattern. leftover-48
status already says do not dest-lock.

`(64,240)` as `ComputeSubmitDest` origin is
already locked at scale 1. Extending it to
`Compute(40,150)` × parent 1.6 **is** the
formula analog, but asserting the **result
tuple** as a New Profile dest still plants
the same numbers the dump invented for host
layout. Leave it in the proof.

Do **not** lock type-16/37 dest `608,293`
or invented chrome hit `608,293,800,325`.
Those are leftover pack / `TryChromeHit`
(`TryChromeHitIsNativeHit=false`).

Do **not** re-enable `Key.N`.

---

## Gap

```
Evidence                    Tests today                         Gap
formula POINT size 0        ComputeSubmitDest 64,240 scale 1    MATCH class; not persist×1.6
persist (40,150) / 362,420  UNLOCKED                            file recover in proof only
live list dest 64,240       UNLOCKED (LEFT Y=240 finder only)   do not plant
live Accept dest 579,672    Hit=dest / TryDestPoint / 0x126     do not plant
live mouse 579,672,979,720  TryDestPoint area, no tuple         do not plant
TryChromeHit native         false                               MATCH
native dest dump            NativeDestTupleUnread               UNREAD
```

| Object | Formula analog | Tests | Action |
| --- | --- | --- | --- |
| List POINT `64,240,64,240` | persist `(40,150)×1.6` | tautology at scale 1; live dest **UNLOCKED** | **leave open** |
| Accept POINT `579,672` | persist `(362,420)×1.6` | **UNLOCKED** | **leave open** |
| Mouse `579,672,979,720` | persist `250×30` ×1.6 | **UNLOCKED** | **leave open** |
| Dest POINT class | persist/leftover 0 | slider/edit/title `X0==X1` | **MATCH** class |
| `TryChromeHitIsNativeHit` | false | **MATCH** | keep false |

**Next site (do not apply here):** keep dest a
point when size 0. Do not add dest-number
asserts. Do not invent type-16/37 hit. Do
not `Key.N`. Native dest dump still needs
`[esp+36..48]` / rec `+12..+24`, not a host
table. Leave leftover #48 open. Leave leftover
#36 dest-lock open.

---

## UNREAD sites

- Native first-seen dest 4-tuple
  (`[esp+36..48]` / rec `+12..+24` /
  widget `+248`) on New Profile Present.
- Whether `+326` pack copies onto child
  dest (host `ListChildAuthoredPos` leftover).
- Exact `00531090` extra numbers on type 11/38.

---

## Sources

- `C:\FableCSharp\proofs\leftover-48-native-aabb\README.md`
- `C:\FableCSharp\proofs\leftover-48-dest\README.md`
- `C:\FableCSharp\proofs\leftover-48-native-hit\README.md`
- `C:\FableCSharp\proofs\leftover-48-chrome-hit\README.md`
- `C:\FableCSharp\export\frontend\new-profile-dests.txt`
- `C:\FableCSharp\src\Fable.Game\FrontendLayout.cs`
- `C:\FableCSharp\tests\Fable.Formats.Tests\FrontendLayoutTests.cs`
- `C:\FableCSharp\tests\Fable.Formats.Tests\FrontendInputTests.cs`
- `C:\FableCSharp\tests\Fable.Formats.Tests\FrontendFrameDumpTests.cs`
- `C:\FableCSharp\tests\Fable.Formats.Tests\EngineLifecycleTests.cs`
- `C:\FableCSharp\tests\Fable.Formats.Tests\FrontendUiDefTests.cs`
- `C:\FableCSharp\docs\status\README.md`
