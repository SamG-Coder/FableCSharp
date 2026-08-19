# Issue #36 vs HEAD — frontend dest / DIP / type-6

Investigation only. Production `src/` and `tests/` were not edited.

Issue (as filed): frontend layout invents dest; host claims
`009DB700` / `009DA9F0` DIP; native first-seen dest is
`0,0,0,0`; host fills leftover `+204` from sprite/font
measure; type-6 packed as `0x22` vs native `0x27`.

Authority: current HEAD of `C:\FableCSharp`.
`src/Fable.Game/EngineLifecycle.cs`
(`LayoutFrontendWidgets`, `QueueFrontend2dRecord`,
`FlushFrontendDisplay`, `CollectFrontendRecords`),
`src/Fable.Game/FrontendLayout.cs`,
`src/Fable.Game/FrontendDx9Submit.cs`,
`src/Fable.Game/FrontendTextDraw.cs`,
`tests/Fable.Formats.Tests/FrontendLayoutTests.cs`,
`FrontendDx9SubmitTests.cs`, `EngineLifecycleTests.cs`,
`docs/status/README.md` (leave #36 open).

Statuses: **PROVEN** / **PARTIAL** / **UNREAD** / **DISPROVEN** /
**STILL OPEN** / **FIXED** / **LEFTOVER**.

Do not invent dest writers. Do not treat font measure as
`0041AFA0` leftover.

---

## Verdict vs HEAD

**PARTIAL. Leave #36 open.**

The invented dest math and the `009DB700` widget-enqueue
claim are gone. First-seen dest `0,0,0,0` and type-6 GPU
records as `0x27` match native. The filed leftovers that
still stand are:

| Filed claim | vs HEAD | Leftover |
|---|---|---|
| Layout invents dest | **FIXED** live | Isolated calculator tests still feed `16×16` leftover for type-6. Dead helper `FrontendWidgetDest` still uses PlayAVI `0.5`. |
| Host claims `009DB700` as widget dest | **FIXED** | Notes now say `00BAE2D0` / `no 009DB700`. `EnqueuesDisplayQueue=false`. |
| Host claims `009DA9F0` DIP | **STILL OPEN** | Install + nonempty dest sets `FrontendEnqueueRan` → Notes `009DA9F0(...) DIP vtbl+332` and `Frontend2dDipIssued=true`. Native `009DA9F0` drains `+16020` only; first-seen skip `009DB6E6`. Type-`0x22` draw is `00BAE2D0`. |
| Native first-seen dest `0,0,0,0` | **FIXED** / **MATCH** | Root Width=0. Type-6 dest is a point at remapped origin. |
| Host fills `+204` from sprite/font measure | **PARTIAL** | Live leftover only when `GraphicId != 0` (sprite frame). Type-6 `GraphicId=0` → dest point. Collect still treats dest width as `+204`. Type-6 `+204` writer UNREAD. |
| Type-6 packed `0x22` vs native `0x27` | **PARTIAL** | Batch / `FrontendTextDraw.Type6RecordType` is `0x27`. `QueueFrontend2dRecord` still Notes `0041BEB0` type `0x22` for every leaf, including type-6. |

Ledger already says leave #36 open (`docs/status/README.md`
rows for dest inherit / `00B324A0` vtbl+20 / `0041AC20`).

---

## 1. Dest layout — live path is recovered, not invented

`FrontendLayout` is the isolated `005339B0` / `0052F5C0` /
`0052FFD0` / `0041AFA0` calculator. No screen-specific
numbers.

```216:252:src/Fable.Game/FrontendLayout.cs
    public static (float X0, float Y0, float X1, float Y1) ComputeSubmitDest(
        int persistWidth,
        int persistHeight,
        float leftoverW,
        float leftoverH,
        float originX,
        float originY,
        float destScaleX,
        float destScaleY,
        bool center)
    {
        var width = persistWidth != 0 ? persistWidth : leftoverW;
        var height = persistHeight != 0 ? persistHeight : leftoverH;
        width *= destScaleX;
        height *= destScaleY;
        // ...
        return (Snap(x0), Snap(y0), Snap(x1), Snap(y1));
    }
```

`LayoutFrontendWidgets` fills leftover **only** when
`GraphicIndex` (`GraphicId`) is nonzero, from bank frame
w/h. That is `0041AC20` `+376 != 0` → vtbl+84/+88. It
does **not** measure fonts into dest.

```7658:7688:src/Fable.Game/EngineLifecycle.cs
            var leftoverW = 0f;
            var leftoverH = 0f;
            // 0041AC20 leftover +204/+208 only when
            // +376 = first style GraphicIndex != 0,
            // via bank vtbl+84/+88 (frame w/h).
            if (widget.GraphicId != 0 &&
                widget.TextureName is { } leftoverName &&
                _frontendSprites?.TryLoad(leftoverName) is { } leftoverTex)
            {
                leftoverW = leftoverTex.FrameWidth > 0 ? leftoverTex.FrameWidth : leftoverTex.Width;
                leftoverH = leftoverTex.FrameHeight > 0 ? leftoverTex.FrameHeight : leftoverTex.Height;
            }
            // ...
            var dest = FrontendLayout.Compute(layout, parentDest, viewport);
```

First-seen PRESS_START dest table is locked
(`FrontendLayoutTests.Press_Start_first_seen_dest_table_matches_0041AFA0`):

- root / forrest group: `0,0,0,0`
- forest tiles: `410` lattice
- `UI_TITLE`: `112,48`–`112,48` (point)
- `UI_PRESS_START_TEXT`: `512,384`–`512,384` (`GraphicId=0`)
- `UI_MOUSE_POINTER`: `0,0,32,32` (GraphicIndex leftover)

Lifecycle after install pump matches the type-6 point:

```1602:1606:tests/Fable.Formats.Tests/EngineLifecycleTests.cs
        var drawn = life.FrontendWidgets.First(w => w.Name == "UI_PRESS_START_TEXT");
        Assert.Equal(512f, drawn.DestX0);
        Assert.Equal(384f, drawn.DestY0);
        Assert.Equal(512f, drawn.DestX1);
        Assert.Equal(384f, drawn.DestY1);
```

No-install first pump: dest stays ctor `0,0,0,0`.
`Frontend_0041AC20_dest_is_0041AFA0_scale_not_PlayAVI`
asserts that.

**DISPROVEN as current live leftover:** parent.Dest + Position;
texture-or-font size for every widget; always-remap 640→1024.
Those were the old host guesses (`implementer/frontend/02-layout.md`
“What current C# is wrong” is **STALE** against this HEAD).

### Dest leftover that still stands

Calculator tests still invent type-6 leftover `16×16`:

```56:69:tests/Fable.Formats.Tests/FrontendLayoutTests.cs
        var text = new FrontendWidgetLayout(
            PositionX: 320f,
            PositionY: 240f,
            LeftoverW: 16f,
            LeftoverH: 16f);
        var dest = FrontendLayout.Compute(text, root, FirstSeen);
        Assert.True(dest.X1 > dest.X0, $"text dest {dest.X0},{dest.Y0},{dest.X1},{dest.Y1}");
        // ...
        Assert.Equal(336f, dest.X1);
        Assert.Equal(256f, dest.Y1);
```

Same invented `16×16` in
`Press_Start_persist_positions_are_640_480_pixels` and
`Press_Start_root_remapSize_scales_child_origin_to_viewport`.
The first-seen table test does **not** feed leftover on
`UI_PRESS_START_TEXT` (`GraphicId==0`). Ledger:
“Calculator tests still feed 16×16. leave #36 open.”

Dead helper `FrontendWidgetDest` is unused by live layout
and still centres with PlayAVI half:

```3418:3437:src/Fable.Game/EngineLifecycle.cs
    public static (float X0, float Y0, float X1, float Y1) FrontendWidgetDest(
        int sizeW, int sizeH,
        float leftoverW, float leftoverH,
        float originX, float originY,
        float scaleX, float scaleY,
        bool center)
    {
        var w = sizeW == 0 ? leftoverW : sizeW;
        var h = sizeH == 0 ? leftoverH : sizeH;
        w *= scaleX;
        h *= scaleY;
        var x0 = originX;
        var y0 = originY;
        if (center)
        {
            x0 -= w * RegionTravel.PlayAviLetterboxHalf;
            y0 -= h * RegionTravel.PlayAviLetterboxHalf;
        }

        return (x0, y0, x0 + w, y0 + h);
    }
```

Do not retarget this helper by inventing dest writes.

---

## 2. `009DB700` vs `00BAE2D0` vs `009DA9F0`

Two queues. Widget sprites are **not** the display
`+16020` enqueue.

| Path | Insert | Drain | First-seen frontend |
|---|---|---|---|
| Type `0x22` sprite | `0041BEB0` → dest vtbl+92 `00B23BC0` → `00B324A0` | factory vtbl+20 `00BAE2D0` → `00A0AEA0` DIPUP | dest `0,0,0,0`; `00BAD8A0` early-out; **no** `009DB700` |
| Display `+16020` | `009DB700` (`009DBFF0` / `009DD8F0` only) | `009DA9F0` vtbl+332 | empty → `009DB6E6` |
| Type-6 glyphs | `0054EF00` → `00543910` type `0x27` | `00AB7C20` → `00A0ABE0` vtbl+324 | dest is a point; glyphs from pen |

Host records this correctly on the isolated types:

```92:133:src/Fable.Game/FrontendDx9Submit.cs
    /// First-seen dest after ctor is 0,0,0,0
    /// (<c>0041AFA0</c> leftover <c>+204</c>
    /// never written, Width=0).
    /// ...
    /// <c>009DB700</c> is not a callee.
    public static FrontendDx9SpriteRecord FirstSeenEmptyDest() =>
        new()
        {
            // DestX0..DestY1 = 0
            EnqueuesDisplayQueue = false,
            CallsDraw = false,
            // ...
        };

    /// Nonempty dest: <c>00BAD8A0</c>
    /// ... Direct <c>E8 009DB700</c> is
    /// still absent. Draw is factory
    /// vtbl+20 <c>00BAE2D0</c>
```

`QueueFrontend2dRecord` notes match that:

```3712:3736:src/Fable.Game/EngineLifecycle.cs
        if (FrontendFrameCount == 0 && destW <= 0 && destH <= 0)
        {
            Note(FrontendSpriteSubmitFn, "Frontend", "UI",
                "00BAE2D0 VSHADER_2D_SPRITE 00987FE0 no 009DB700");
            // ...
        }
        else if (destW <= 0 && destH <= 0)
        {
            Note(FrontendSpriteInstanceSubmitFn, "Frontend", "UI",
                "00BAD8A0 [rec+32]=0 [rec+64]=0 00BADB36 ret 8 no 009DB700");
        }
        else
        {
            FrontendEnqueueRan = true;
            Note(FrontendSpriteInstanceSubmitFn, "Frontend", "UI",
                $"00BAD8A0 dest {destX0},{destY0},{destX1},{destY1} 00BAE2D0 no 009DB700");
        }
```

`009DB700` as the widget dest writer is **DISPROVEN** and
**FIXED** in notes. Tests:
`Nonempty_dest_draws_via_00BAE2D0_not_009DB700`,
`First_seen_dest_zero_does_not_enqueue_or_dip`.

### `009DA9F0` DIP leftover (the open half)

`FlushFrontendDisplay` ties DIP to **any nonempty dest**
(`FrontendEnqueueRan`), not to `+16020` count:

```3784:3793:src/Fable.Game/EngineLifecycle.cs
        var shouldDip = FrontendEnqueueRan || DisplayFlushShouldDip(0, 0);
        Note(DisplayFlushLayersFn, "Frontend", "D3D9",
            shouldDip
                ? $"009DA9F0({DisplayFlushLayersArg}) [+{DisplayQueueBeginOffset}] DIP vtbl+{DrawIndexedPrimitiveVtbl}"
                : $"009DA9F0({DisplayFlushLayersArg}) [+{DisplayQueueBeginOffset}] empty");
        // ...
        Frontend2dDipIssued = shouldDip;
```

`DisplayFlushShouldDip(0, 0)` is always false. So
`Frontend2dDipIssued` is just “some widget dest was
nonempty.” Native `009DA9F0` only DIPs when
`[+16020, +16024)` is nonempty. Type-`0x22` never
calls `009DB700`, so that vector stays empty on the
frontend sprite path. First-seen is the skip
(`Frontend_0041AC20_dest…` `Frontend2dDipIssued=false`).

Install PRESS_START **locks the invented claim**:

```1547:1561:tests/Fable.Formats.Tests/EngineLifecycleTests.cs
        Assert.True(life.FrontendEnqueueRan);
        Assert.True(life.Frontend2dDipIssued);
        // ...
        Assert.Contains(life.Trace.Events, e =>
            e.Va == EngineLifecycle.DisplayFlushLayersFn &&
            e.Action.Contains("DIP vtbl+", StringComparison.Ordinal));
```

That is the host leftover named in the ledger:
“Host still Notes `009DA9F0` DIP. leave #36 open.”

---

## 3. Leftover `+204` — sprite gate MATCH, type-6 analog leftover

Native `0041AFA0`:

```
w = (+360 != 0) ? (float)+360 : +204
h = (+364 != 0) ? (float)+364 : +208
```

`0041AC20` writes `+204/+208` only when `+376 != 0`
(first style GraphicIndex). First-seen type-0/type-6
PRESS_START: `+376=0`, Width=0, dest is a point.
Font measure is **not** this field.

Live dest matches that. Collect then uses dest width
as the type-6 `+204` stand-in (first-seen MATCH because
the dest is a point → 0):

```7814:7817:src/Fable.Game/EngineLifecycle.cs
                    var leftoverW = MathF.Max(0f, widget.DestX1 - widget.DestX0);
                    var (penX, penY) = FrontendTextDraw.Type6Pen(
                        widget.DestX0, widget.DestY0, leftoverW, 1f,
                        FrontendTextDraw.AlignLeft);
```

Native `0054EF00` does `fmul [esi+204]` for centre/right
only. Writer of type-6 `+204` is **UNREAD** (not
`00AB7B00` width 301; not font measure as dest). Scale
is hard `1f` vs dest `+264` (root remap `1.6`). Left
align ignores leftover×scale. Centre/right would
diverge if `def+508 != 0`.

**Do not invent a dest / `+204` writer.**

---

## 4. Type-6 record type — batch `0x27`, notes still `0x22`

Native packer `00543910` writes type `0x27` size 64.
Not `0041BEB0` type `0x22`.

```23:24:src/Fable.Game/FrontendTextDraw.cs
    public const int Type6RecordType = 0x27;
    public const int Type6RecordSize = 64;
```

`CollectFrontendRecords` emits that type per glyph.
`FrontendDx9Submit.GlyphRecordType` / tests
`Type6_glyph_is_type_27_size_64_stride_28` and
`FrontendTextDraw_emits_one_00AB7C20_quad_per_glyph`
lock `0x27` and `Assert.NotEqual(0x22, …)`.

`QueueFrontend2dRecord` still packs every leaf as
`Frontend2dRecordType` (`0x22`, sibling of fade overlay):

```3682:3705:src/Fable.Game/EngineLifecycle.cs
    /// so <c>0041BEB0</c> at <c>0041B47C</c>
    /// (type <c>0x22</c>, dest 0xC0), then
    /// ...
        Note(packer, "Frontend", "UI",
            sibling
                ? $"0041BF60 type 0x{Frontend2dRecordType:X} [+380]"
                : $"0041BEB0 type 0x{Frontend2dRecordType:X} +{FrontendWidgetBlendOffset}={FrontendWidgetBlend}");
```

Type-6 children go through `DrawContainerWalk` →
`QueueFrontend2dRecord(widget)` with dest
`512,384,512,384` (zero size → no `FrontendEnqueueRan`
from the text widget itself). Forest / title sprites
with nonempty dest set `FrontendEnqueueRan` and the
`0x22` last-type. `Frontend2dLastType` stays `0x22`
even on the type-6 draw path.

`Type6Glyph().FaceName` is still `"ENG_ARIAL_16"`
(`0054F4B0` helper). Persist Font `26051` is
`ENG_ARIAL_24`. That face leftover is related but
not the dest/`0x22` claim.

---

## 5. Classification

| Item | Status vs HEAD |
|---|---|
| Invented dest numbers in live `LayoutFrontendWidgets` | **FIXED** |
| Native first-seen dest `0,0,0,0` | **FIXED** / MATCH |
| `009DB700` as widget dest enqueue | **FIXED** (DISPROVEN) |
| `009DA9F0` DIP when dest nonempty | **STILL OPEN** |
| `+204` from font measure into dest | **FIXED** live |
| `+204` from sprite when `GraphicIndex==0` | **FIXED** live |
| Calculator `16×16` type-6 leftover | **STILL OPEN** |
| Type-6 GPU type `0x27` | **FIXED** |
| Type-6 Note / `Frontend2dLastType` `0x22` | **STILL OPEN** |
| Type-6 `+204` writer | **UNREAD** |

**Overall: PARTIAL.**

---

## Proposed next step

Do **not** invent dest writers.

1. Split `FrontendEnqueueRan` from `009DA9F0` DIP.
   Nonempty dest → `00BAD8A0` / `00BAE2D0`.
   `Frontend2dDipIssued` only if `+16020` count ≠ 0
   (`DisplayFlushShouldDip`). First-seen stays skip.
   Change `Frontend_present_runs_on_install_after_videos`
   so it no longer requires `DIP vtbl+` on an empty
   display queue.
2. Stop feeding `LeftoverW/H: 16` into type-6
   calculator cases. Type-6 dest is a point at the
   remapped origin. Keep GraphicIndex leftover on
   sprites only.
3. Type-6 leftover204 = widget `+204` analog (0
   first-seen), not dest W. Pass dest scale `+264`
   into `Type6Pen`. Do not invent `00AB7B00` width.
4. Note type-6 via `00543910` / `0x27`, not
   `0041BEB0` / `0x22`. Leave `0041BEB0` on type-0
   sprites.

Until (1) and the 16×16 tests change, leave #36 open.
