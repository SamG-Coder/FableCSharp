using Fable.Formats.Defs;

namespace Fable.Game;

/// <summary>
/// Type 11/38 <c>vtbl+568</c>
/// <c>0055B8F0</c>: AABB from dest
/// origin (<c>vtbl+488</c>) plus size
/// (<c>vtbl+492</c>). Draw dest and
/// hit dest are the same rectangle.
/// Point dests do not invent a
/// leftover union. Empty space hits
/// nothing; click a presented child
/// dest and walk to the interactive
/// ancestor.
/// </summary>
public static class FrontendHitTest
{
    public const uint HitTestFn = 0x0055B8F0;
    public const uint HoverSelectFn = 0x0055BF10;
    public const int HitVtbl = 568;

    public static bool Contains(
        float x0, float y0, float x1, float y1, float x, float y) =>
        x1 > x0 && y1 > y0 && x >= x0 && x < x1 && y >= y0 && y < y1;

    public static (float X0, float Y0, float X1, float Y1) DestRect(FrontendWidget widget) =>
        (widget.DestX0, widget.DestY0, widget.DestX1, widget.DestY1);

    /// <summary>
    /// <c>0055B8F0</c> uses dest origin
    /// (<c>vtbl+488</c>) plus dest size
    /// (<c>vtbl+492</c>). Draw dest stays
    /// a point when leftover is 0; hit
    /// size is leftover #48 sibling
    /// type-2 packing, not a child dest.
    /// </summary>
    public static (float X0, float Y0, float X1, float Y1) HitRect(
        IReadOnlyList<FrontendWidget> tree, int index)
    {
        if ((uint)index >= (uint)tree.Count)
            return (0f, 0f, 0f, 0f);
        var widget = tree[index];
        if (widget.HitX1 > widget.HitX0 && widget.HitY1 > widget.HitY0)
            return (widget.HitX0, widget.HitY0, widget.HitX1, widget.HitY1);
        return DestRect(widget);
    }

    /// <summary>
    /// Centre of this dest if it has
    /// area, else the first presented
    /// descendant dest that does.
    /// </summary>
    public static bool TryDestPoint(
        IReadOnlyList<FrontendWidget> tree, int index, out float x, out float y)
    {
        x = 0f;
        y = 0f;
        if ((uint)index >= (uint)tree.Count)
            return false;
        var widget = tree[index];
        if (widget.DestX1 > widget.DestX0 && widget.DestY1 > widget.DestY0)
        {
            x = (widget.DestX0 + widget.DestX1) * 0.5f;
            y = (widget.DestY0 + widget.DestY1) * 0.5f;
            return true;
        }

        foreach (var kid in FrontendWidgetFactory.ChildrenOf(tree, index))
        {
            if (!FrontendWidgetFactory.IsPresented(tree, kid))
                continue;
            if (TryDestPoint(tree, kid, out x, out y))
                return true;
        }

        return false;
    }

    public static bool Contains(IReadOnlyList<FrontendWidget> tree, int index, float x, float y)
    {
        var rect = HitRect(tree, index);
        return Contains(rect.X0, rect.Y0, rect.X1, rect.Y1, x, y);
    }

    /// <summary>
    /// Reverse-walk presented widgets and
    /// return the first interactive target
    /// whose dest contains
    /// <paramref name="x"/>,<paramref name="y"/>.
    /// Last-drawn dest wins. Clicking
    /// empty space returns null.
    /// </summary>
    public static int? HitIndex(IReadOnlyList<FrontendWidget> tree, float x, float y)
    {
        ArgumentNullException.ThrowIfNull(tree);
        for (var i = tree.Count - 1; i >= 0; i--)
        {
            if (!FrontendWidgetFactory.IsPresented(tree, i))
                continue;
            if (!tree[i].Enabled)
                continue;
            if (!Contains(tree, i, x, y))
                continue;
            if (InteractiveAt(tree, i) is int hit)
                return hit;
        }

        return null;
    }

    public static bool IsInteractive(int type) =>
        type is FrontendInputMap.TypeButton
            or FrontendInputMap.TypeAccept
            or FrontendWidgetType.TextSlider
            or 15
            or FrontendWidgetType.EditBox;

    public static int? InteractiveAt(IReadOnlyList<FrontendWidget> tree, int index)
    {
        if ((uint)index >= (uint)tree.Count)
            return null;
        for (var i = index; i >= 0;)
        {
            var widget = tree[i];
            if (IsInteractive(widget.Type))
                return i;
            if (widget.ParentIndex < 0 && widget.ParentName is null)
                break;
            var parent = widget.ParentIndex;
            if (parent < 0)
            {
                for (var p = 0; p < tree.Count; p++)
                {
                    if (string.Equals(tree[p].Name, widget.ParentName, StringComparison.Ordinal))
                    {
                        parent = p;
                        break;
                    }
                }
            }

            if (parent < 0)
                break;
            i = parent;
        }

        return null;
    }

    public static bool IsLeftHalf(
        IReadOnlyList<FrontendWidget> tree, int index, float x)
    {
        var rect = HitRect(tree, index);
        if (rect.X1 <= rect.X0)
            return x < rect.X0;
        return x < (rect.X0 + rect.X1) * 0.5f;
    }
}
