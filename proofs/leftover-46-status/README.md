# Leftover #46 vs HEAD — type-16/18 exclusive-walk

Investigation only. Production `src/` and `tests/` were not edited
by this proof.

Filed leftover (`docs/status/README.md` row “Type-16/18 host
exclusive-walk SelectsChild”): host exclusive-walks
`kids[ActiveChild]` for `SelectsChild`. Native `00530260` walks
every `+176` child. Skip is `vtbl+400` / `vtbl+420`. Present skip
VAs are constants + proof only and are never read by
`DrawContainerWalk` or `IsPresented`.

Authority: current host
`src/Fable.Game/FrontendWidgetFactory.cs`
(`IsPresented` / `ApplyFirstSeenState` / `ApplySelectState`),
`src/Fable.Game/EngineLifecycle.cs` (`DrawContainerWalk`),
`src/Fable.Formats/Defs/FrontendWidgetType.cs`
(`SelectsChild` / `BorrowedVisibleFn` / `ClipBitFn`),
`proofs/type16-18-present-child/README.md`,
`docs/status/README.md` leftover #46,
`tests/Fable.Formats.Tests/FrontendUiDefTests.cs`
(`Factory_builds_press_start_then_main_menu_from_the_same_walk`).

Status words: **PROVEN** / **MATCH** / **STALE** / **UNREAD** /
**DISPROVEN**.

Do not invent skip wiring of `0052F180` / `0052F1D0` into
`DrawContainerWalk`. Do not re-prove native `00530260` walk
(already **DISPROVEN** as exclusive-index in
`proofs/type16-18-present-child`). Do not add type 38 to
`SelectsChild`.

---

## Verdict

**Leftover #46 (exclusive-walk) is STALE.** Host already walks
every child. Native and host draw walks **MATCH** on “every
`+176` child, not `kids[ActiveChild]`”.

The remaining real gap is **not** an exclusive-index walk. Skip
VAs `0052F180` / `0052F1D0` stay named constants and are **UNREAD**
in the host walk. Do not treat that unread gap as leftover #46
still standing, and do not invent host calls of those VAs from
`DrawContainerWalk`.

| Claim | vs HEAD | Class |
| --- | --- | --- |
| Native `00530260` exclusive-walks `ActiveChild` | unchanged | **DISPROVEN** (`proofs/type16-18-present-child`) |
| Host `DrawContainerWalk` exclusive-walks `kids[ActiveChild]` for `SelectsChild` | gone | **STALE** |
| Host `IsPresented` exclusive-walks `kids[ActiveChild]` | gone | **STALE** |
| Host `ApplyFirstSeenState` `Visible=false` on `k != 0` for `SelectsChild` | gone | **STALE** (`type16-18-present-child` host paragraph is **STALE**) |
| Host walks every persist child | `DrawContainerWalk` `foreach` / `IsPresented` parent chain | **MATCH** native `00530260` every `+176` |
| First-seen presented child is persist index 0 | `ActiveChild=0`; type-16 `SelectState(3)` on `kids[0]` only | **MATCH** |
| First-seen siblings stay in the tree and stay `Visible` | tests lock `FORREST_2` / `WASD` / `INVERTED` `Visible` | **MATCH** |
| `BorrowedVisibleFn` `0052F180` / `ClipBitFn` `0052F1D0` read by `DrawContainerWalk` / `IsPresented` | constants + tests only | **UNREAD** |
| Host must call `0052F180` / `0052F1D0` from `DrawContainerWalk` | not shown; host skip is `Visible` / `Clip` | do **not** invent |

---

## 1. Filed leftover

`docs/status/README.md` leftover #46:

> Native `00530260` walks every +176 child. Present skip from
> listing is constants + proof only: `BorrowedVisibleFn`
> `0052F180` / `ClipBitFn` `0052F1D0` / `ForwardSelectFn`
> `0041C5A0` / `TextSliderIndexOffset` 348. Those VAs are never
> read by `DrawContainerWalk` or `IsPresented`. Host still
> exclusive-walks `kids[ActiveChild]` for SelectsChild.

Same exclusive-walk sentence is repeated in the freeze blurb,
the “Frontend `00595222` widget DIP body” row, and the
`71ae66e` / `405b1e8` ledger rows. Those sentences describe
the host at those commits, not HEAD.

Native exclusive-walk was already **DISPROVEN**. The leftover
was the **host** `kids[ActiveChild]` stand-in.

---

## 2. Current host walk — no exclusive-index

### `DrawContainerWalk` — `EngineLifecycle.cs:4231`

```4231:4246:src/Fable.Game/EngineLifecycle.cs
    private void DrawContainerWalk(
        IReadOnlyList<FrontendWidget> tree, int index, ref int drawn)
    {
        if ((uint)index >= (uint)tree.Count)
            return;
        var widget = tree[index];
        if (!widget.Visible || widget.Clip)
            return;
        drawn++;
        if (FrontendWidgetType.DrawsChildList(widget.Type))
        {
            var kids = FrontendWidgetFactory.ChildrenOf(tree, index);
            foreach (var child in kids)
                DrawContainerWalk(tree, child, ref drawn);
            return;
        }
```

No `SelectsChild`. No `kids[widget.ActiveChild]`. Recurse is
every `ChildrenOf` index. Skip is host `Visible` / `Clip`, not
an exclusive sibling pick.

### `IsPresented` — `FrontendWidgetFactory.cs:112`

```112:137:src/Fable.Game/FrontendWidgetFactory.cs
    public static bool IsPresented(IReadOnlyList<FrontendWidget> tree, int index)
    {
        if ((uint)index >= (uint)tree.Count)
            return false;
        var widget = tree[index];
        if (!widget.Visible || widget.Clip)
            return false;
        if (widget.ParentIndex < 0 && widget.ParentName is null)
            return true;
        ...
        return IsPresented(tree, parent);
    }
```

Parent-chain `Visible` / `Clip` only. No `ActiveChild`. Present
collect (`CollectFrontendRecords` `EngineLifecycle.cs:9123` /
`9156`) uses this helper, not `kids[ActiveChild]`.

### `ApplyFirstSeenState` — `FrontendWidgetFactory.cs:53`

Every widget is written `Visible = true`, `Enabled = true`,
`ActiveChild = FirstSeenState` (0). Type 16 then sets
`kids[0]` style to `TextSliderFirstSeenSelect` (3) and leaves
siblings `Visible`. There is **no** `k != 0` hide.

`proofs/type16-18-present-child/README.md` still says host
`SelectsChild` + `ApplyFirstSeenState` `Visible=false` on
`k != 0`. That host paragraph is **STALE** vs this function.

### `ApplySelectState` — `FrontendWidgetFactory.cs:146`

`SelectsChild` (type 16 / 18) writes `ActiveChild = state` then
loops **every** `kids[k]` for style. Type 16 style is
`k == ActiveChild ? 3 : 0`. That is a style pick, not a walk
that drops siblings.

`SelectsChild` itself (`FrontendWidgetType.cs:227`) is still
type 16 / 18 only. It is **not** a draw gate.

---

## 3. Tests lock siblings visible

`FrontendUiDefTests.Factory_builds_press_start_then_main_menu_from_the_same_walk`:

- Type-18 `UI_SWAPPING_FORREST` `ActiveChild == 0`.
- `BLENDING_BG_FORREST_1` **and** `BLENDING_BG_FORREST_2`
  `Visible`.
- Type-16 `UI_OPTIONS_CONTROL_METHOD_TEXT_SLIDER`
  `ActiveChild == 0`.
- `UI_OPTIONS_TEXT_CONTROL_ARROWS` **and**
  `UI_OPTIONS_TEXT_CONTROL_WASD` `Visible`.
- `UI_TEXT_NORMAL` **and** `UI_TEXT_INVERTED` `Visible`.
- Inactive type-16 kids keep style 0; child 0 keeps style 3.

That is persist-child-0 **MATCH**, not exclusive-hide.

---

## 4. Remaining real gap — skip VAs unread

Native `00530260` skip is `vtbl+400` (`0052F180`, `[+300]>>7`,
persist `def+504`) and `vtbl+420` (`0052F1D0`, `[+302]&1`,
persist `def+392`). Bodies are **PROVEN** in
`proofs/vtbl400-first-seen-hide` / `proofs/listing-present-skip`.

Host:

| Native | Host HEAD |
| --- | --- |
| `vtbl+400` `0052F180` | `FrontendWidgetType.BorrowedVisibleFn` constant; tests equal the VA. `Plus504` is parsed on the def. Widget `Visible` is factory `true`. **UNREAD** in `DrawContainerWalk` / `IsPresented`. |
| `vtbl+420` `0052F1D0` | `ClipBitFn` constant; tests equal the VA. Widget `Clip` is `def.Plus392 != 0` at `FrontendWidgetFactory.Add` (`FrontendWidgetFactory.cs:338`). Field stand-in, not a call of `0052F1D0`. |
| `0041C5A0` / `+348` | named / type-16 style only. Not a present skip. |

Listing of native `00530260` calls `[edx+400]` / `[edx+420]`
then `[edx+8]`. That does **not** show the host C# walk must
call `0052F180` / `0052F1D0`. Host skip is `Visible` / `Clip`.
Do not invent those calls on this leftover.

---

## 5. Nearby hide that is **not** leftover #46

`ApplyTextSliderHit` (`EngineLifecycle.cs:8674`) on a type-16
click writes `Visible = k == next` on each child
(`EngineLifecycle.cs:8686`). That is an exclusive **hide after
input**, not the filed first-seen / draw exclusive-**walk**.
`DrawContainerWalk` still visits every child; hidden kids then
return on `!Visible`. Do not fold that click hide into leftover
#46.

---

## Do not

- Leave leftover #46 open as “host still exclusive-walks
  `kids[ActiveChild]`”. That walk is gone
  (`DrawContainerWalk` `EngineLifecycle.cs:4243`,
  `IsPresented` `FrontendWidgetFactory.cs:112`).
- Invent `DrawContainerWalk` calls of `0052F180` / `0052F1D0`.
- Treat unread skip VAs as recovered because they exist as
  constants.
- Re-file native `00530260` exclusive-walk (already
  **DISPROVEN**).
- Add type 38 to `SelectsChild`.
