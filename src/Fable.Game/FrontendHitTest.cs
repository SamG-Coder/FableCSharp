using Fable.Formats.Defs;

namespace Fable.Game;

/// <summary>
/// Type 11/38 <c>vtbl+568</c>
/// <c>0055B8F0</c>: AABB from dest
/// origin (<c>vtbl+488</c>) plus size
/// (<c>vtbl+492</c>). Point dests use
/// the union of presented descendants
/// (mouse-area children). Empty space
/// hits nothing.
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
    /// Submit dest if it has area, else
    /// the union of presented descendants
    /// that do. Same rectangle is used
    /// for draw leftovers and hit.
    /// </summary>
    public static (float X0, float Y0, float X1, float Y1) HitRect(
        IReadOnlyList<FrontendWidget> tree, int index)
    {
        if ((uint)index >= (uint)tree.Count)
            return (0f, 0f, 0f, 0f);
        var widget = tree[index];
        if (widget.DestX1 > widget.DestX0 && widget.DestY1 > widget.DestY0)
            return DestRect(widget);

        var have = false;
        var x0 = 0f;
        var y0 = 0f;
        var x1 = 0f;
        var y1 = 0f;
        UnionDescendants(tree, index, ref have, ref x0, ref y0, ref x1, ref y1);
        return have ? (x0, y0, x1, y1) : DestRect(widget);
    }

    public static bool Contains(IReadOnlyList<FrontendWidget> tree, int index, float x, float y)
    {
        var rect = HitRect(tree, index);
        return Contains(rect.X0, rect.Y0, rect.X1, rect.Y1, x, y);
    }

    /// <summary>
    /// Reverse-walk presented widgets and
    /// return the interactive target under
    /// <paramref name="x"/>,<paramref name="y"/>.
    /// Clicking empty space returns null.
    /// </summary>
    public static int? HitIndex(IReadOnlyList<FrontendWidget> tree, float x, float y)
    {
        ArgumentNullException.ThrowIfNull(tree);
        int? best = null;
        var bestArea = float.MaxValue;
        for (var i = tree.Count - 1; i >= 0; i--)
        {
            if (!FrontendWidgetFactory.IsPresented(tree, i))
                continue;
            if (!tree[i].Enabled)
                continue;
            if (!Contains(tree, i, x, y))
                continue;
            var target = InteractiveAt(tree, i);
            if (target is not int hit)
                continue;
            var rect = HitRect(tree, hit);
            var area = MathF.Max(1f, rect.X1 - rect.X0) * MathF.Max(1f, rect.Y1 - rect.Y0);
            if (area > bestArea)
                continue;
            best = hit;
            bestArea = area;
        }

        return best;
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

        var row = NearestRow(tree, index);
        if ((uint)row >= (uint)tree.Count)
            return null;
        var rowParent = tree[row].ParentIndex;
        if (rowParent < 0 || tree[rowParent].Type != FrontendWidgetType.List)
            return null;
        return InteractiveInSubtree(tree, row);
    }

    public static bool IsLeftHalf(
        IReadOnlyList<FrontendWidget> tree, int index, float x)
    {
        var rect = HitRect(tree, index);
        if (rect.X1 <= rect.X0)
            return x < rect.X0;
        return x < (rect.X0 + rect.X1) * 0.5f;
    }

    private static int NearestRow(IReadOnlyList<FrontendWidget> tree, int index)
    {
        var i = index;
        while ((uint)i < (uint)tree.Count)
        {
            var parent = tree[i].ParentIndex;
            if (parent < 0)
                break;
            if (tree[parent].Type == FrontendWidgetType.List)
                return i;
            i = parent;
        }

        return index;
    }

    private static int? InteractiveInSubtree(IReadOnlyList<FrontendWidget> tree, int root)
    {
        if ((uint)root >= (uint)tree.Count)
            return null;
        if (IsInteractive(tree[root].Type))
            return root;
        var kids = FrontendWidgetFactory.ChildrenOf(tree, root);
        foreach (var kid in kids)
        {
            var found = InteractiveInSubtree(tree, kid);
            if (found is int hit)
                return hit;
        }

        return null;
    }

    private static void UnionDescendants(
        IReadOnlyList<FrontendWidget> tree,
        int parent,
        ref bool have,
        ref float x0,
        ref float y0,
        ref float x1,
        ref float y1)
    {
        var kids = FrontendWidgetFactory.ChildrenOf(tree, parent);
        foreach (var kid in kids)
        {
            if (!FrontendWidgetFactory.IsPresented(tree, kid))
                continue;
            var child = tree[kid];
            if (child.DestX1 > child.DestX0 && child.DestY1 > child.DestY0)
            {
                if (!have)
                {
                    x0 = child.DestX0;
                    y0 = child.DestY0;
                    x1 = child.DestX1;
                    y1 = child.DestY1;
                    have = true;
                }
                else
                {
                    if (child.DestX0 < x0)
                        x0 = child.DestX0;
                    if (child.DestY0 < y0)
                        y0 = child.DestY0;
                    if (child.DestX1 > x1)
                        x1 = child.DestX1;
                    if (child.DestY1 > y1)
                        y1 = child.DestY1;
                }
            }

            UnionDescendants(tree, kid, ref have, ref x0, ref y0, ref x1, ref y1);
        }
    }
}
