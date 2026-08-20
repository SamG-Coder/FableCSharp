using Fable.Formats.Defs;

namespace Fable.Game;

/// <summary>
/// <c>0041DB1D</c> factory → <c>009AD410</c>
/// lookup → <c>0041D21B</c> type switch, then
/// <c>005331A0</c> child walk of persist
/// <c>Children</c> indices. Same walk for
/// Press Start, New Profile, and Main Menu.
/// </summary>
public static class FrontendWidgetFactory
{
    public const uint ConstructFn = FrontendWidgetType.ConstructFn;
    public const uint FactoryFn = FrontendWidgetType.FactoryFn;
    public const uint ChildAttachFn = FrontendWidgetType.ChildAttachFn;
    public const string FrontEndBankHeader = "front_end_bank.h";

    public static List<FrontendWidget> Build(
        GameBin defs,
        string rootName,
        FrontendSpriteBank? sprites = null,
        Func<string, string?>? lookupText = null,
        NamesBin? names = null)
    {
        var widgets = new List<FrontendWidget>();
        var root = defs.FindEntry(rootName);
        var parsed = root is null ? null : FrontendUiDef.TryParse(root);
        Add(widgets, parsed, rootName, parent: null, parentIndex: -1, sprites, lookupText, names);
        if (parsed is null)
            return widgets;
        AttachChildren(widgets, defs, parsed, rootName, 0, sprites, lookupText, names);
        ApplyFirstSeenState(widgets);
        return widgets;
    }

    /// <summary>
    /// <c>0052C730</c> zeros
    /// <c>+324/+328/+332</c>.
    /// <c>005339B0</c> packs style 0
    /// into <c>+132</c> and
    /// <c>+144..+147=0xFF</c>, so
    /// present <c>+151</c> is style-0 A.
    /// Type 16 <c>00549230</c>
    /// <c>SelectState(3)</c> on child
    /// <c>+348</c> only: <c>+332=3</c>
    /// and <c>+328=3</c> because
    /// <c>vtbl+176</c> <c>0041C5C0</c>
    /// is “style key exists”. Colour
    /// stays style 0. Unselected
    /// siblings stay <c>+328=0</c>.
    /// </summary>
    public static void ApplyFirstSeenState(List<FrontendWidget> widgets)
    {
        for (var i = 0; i < widgets.Count; i++)
        {
            var widget = widgets[i];
            widgets[i] = widget with
            {
                Visible = true,
                Enabled = true,
                ActiveChild = FrontendWidgetType.FirstSeenState,
                State = FrontendWidgetType.FirstSeenState,
                StyleIndex = FrontendWidgetType.FirstSeenState,
                Colour = ColourAtStyle(widget, FrontendWidgetType.FirstSeenState),
            };
        }

        for (var i = 0; i < widgets.Count; i++)
        {
            if (widgets[i].Type != FrontendWidgetType.TextSlider)
                continue;
            var kids = ChildrenOf(widgets, i);
            widgets[i] = widgets[i] with
            {
                ActiveChild = FrontendWidgetType.FirstSeenState,
            };
            if (kids.Count == 0)
                continue;
            var child = widgets[kids[0]];
            var select = FrontendWidgetType.TextSliderFirstSeenSelect;
            widgets[kids[0]] = child with
            {
                State = select,
                StyleIndex = select,
                // 00549230 selects state 3 during construction.  The native
                // style tick then resolves that state's colour before the
                // first stable menu frame; keep the reconstructed stable
                // frame at the same point rather than leaving style-0 alpha.
                Colour = ColourAtStyle(child, select),
            };
        }

        var byName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < widgets.Count; i++)
            byName.TryAdd(widgets[i].Name, i);
        for (var i = 0; i < widgets.Count; i++)
        {
            if (widgets[i].ParentName is not { } parentName ||
                !byName.TryGetValue(parentName, out var parent))
                continue;
            var inherit = widgets[parent];
            widgets[i] = widgets[i] with
            {
                Visible = widgets[i].Visible && inherit.Visible,
                Enabled = widgets[i].Enabled && inherit.Enabled,
            };
        }
    }

    /// <summary>
    /// <c>00530260</c> walks every
    /// <c>+176</c> child. Skip is persist
    /// clip <c>+392</c> / inherited parent
    /// clip, not a sibling index hide.
    /// </summary>
    public static bool IsPresented(IReadOnlyList<FrontendWidget> tree, int index)
    {
        if ((uint)index >= (uint)tree.Count)
            return false;
        var widget = tree[index];
        if (!widget.Visible)
            return false;
        if (widget.ParentIndex < 0 && widget.ParentName is null)
            return true;
        var parent = widget.ParentIndex;
        if (parent < 0)
        {
            for (var i = 0; i < tree.Count; i++)
            {
                if (string.Equals(tree[i].Name, widget.ParentName, StringComparison.Ordinal))
                {
                    parent = i;
                    break;
                }
            }
        }

        if (parent < 0)
            return true;
        return IsPresented(tree, parent);
    }

    public static List<int> ChildrenOf(IReadOnlyList<FrontendWidget> widgets, string? parent)
    {
        var kids = new List<int>();
        var parentIndex = -1;
        if (parent is not null)
        {
            for (var i = 0; i < widgets.Count; i++)
            {
                if (string.Equals(widgets[i].Name, parent, StringComparison.Ordinal))
                {
                    parentIndex = i;
                    break;
                }
            }
        }

        if (parentIndex >= 0)
            return ChildrenOf(widgets, parentIndex);
        for (var i = 0; i < widgets.Count; i++)
        {
            if (string.Equals(widgets[i].ParentName, parent, StringComparison.Ordinal))
                kids.Add(i);
        }

        return kids;
    }

    public static List<int> ChildrenOf(IReadOnlyList<FrontendWidget> widgets, int parentIndex)
    {
        var kids = new List<int>();
        for (var i = 0; i < widgets.Count; i++)
        {
            if (widgets[i].ParentIndex == parentIndex)
                kids.Add(i);
        }

        return kids;
    }

    /// <summary>Allocation-free equivalent of the native <c>+176</c> child walk.</summary>
    public static int ChildCount(IReadOnlyList<FrontendWidget> widgets, int parentIndex)
    {
        var count = 0;
        for (var i = 0; i < widgets.Count; i++)
            if (widgets[i].ParentIndex == parentIndex)
                count++;
        return count;
    }

    public static int ChildAt(IReadOnlyList<FrontendWidget> widgets, int parentIndex, int ordinal)
    {
        if (ordinal < 0)
            return -1;
        for (var i = 0; i < widgets.Count; i++)
        {
            if (widgets[i].ParentIndex != parentIndex)
                continue;
            if (ordinal-- == 0)
                return i;
        }
        return -1;
    }

    public static int FirstChild(IReadOnlyList<FrontendWidget> widgets, int parentIndex) =>
        ChildAt(widgets, parentIndex, 0);

    private static void AttachChildren(
        List<FrontendWidget> widgets,
        GameBin defs,
        FrontendUiDef parent,
        string parentName,
        int parentIndex,
        FrontendSpriteBank? sprites,
        Func<string, string?>? lookupText,
        NamesBin? names)
    {
        foreach (var index in parent.ChildIndices)
        {
            if ((uint)index >= (uint)defs.Entries.Count)
                continue;
            var child = FrontendUiDef.TryParse(defs.Entries[index]);
            if (child is null)
                continue;
            var childIndex = widgets.Count;
            Add(widgets, child, child.InstanceName, parentName, parentIndex, sprites, lookupText, names);
            AttachChildren(
                widgets, defs, child, child.InstanceName, childIndex, sprites, lookupText, names);
        }

        AttachSpriteCells(widgets, defs, parent, parentName, parentIndex, sprites, lookupText, names);
    }

    /// <summary>
    /// Type-2 <c>00551340</c> walks persist
    /// Sprites <c>(key, defIndex)</c> and
    /// <c>0041D21B</c>s each cell.
    /// </summary>
    private static void AttachSpriteCells(
        List<FrontendWidget> widgets,
        GameBin defs,
        FrontendUiDef parent,
        string parentName,
        int parentIndex,
        FrontendSpriteBank? sprites,
        Func<string, string?>? lookupText,
        NamesBin? names)
    {
        if (parent.Type != FrontendWidgetType.TableType || parent.SpriteDefIndices.Count == 0)
            return;
        foreach (var index in parent.SpriteDefIndices)
        {
            if ((uint)index >= (uint)defs.Entries.Count)
                continue;
            var child = FrontendUiDef.TryParse(defs.Entries[index]);
            if (child is null || child.InstanceName == parentName)
                continue;
            Add(widgets, child, child.InstanceName, parentName, parentIndex, sprites, lookupText, names);
        }
    }

    /// <summary>
    /// Type-6 ctor <c>0054ED90</c> pushes
    /// persist Font i32 into
    /// <c>009D49B0</c> names-blob offset
    /// then <c>009E2C80</c> face lookup.
    /// </summary>
    public static string? ResolveFontFace(int font, NamesBin? names)
    {
        if (font <= 0 || names is null)
            return null;
        return names.Get((uint)font);
    }

    private static void Add(
        List<FrontendWidget> widgets,
        FrontendUiDef? def,
        string name,
        string? parent,
        int parentIndex,
        FrontendSpriteBank? sprites,
        Func<string, string?>? lookupText,
        NamesBin? names)
    {
        var text = def?.TextValue;
        string? body = null;
        if (!string.IsNullOrEmpty(text) && lookupText is not null)
            body = lookupText(text);
        var graphicId = def?.GraphicBankId ?? 0;
        var texture = sprites?.NameForWidget(name, graphicId);
        var font = def?.Font ?? 0;
        var styleColours = PackStyleColours(def);
        widgets.Add(new FrontendWidget(
            name,
            def?.Type ?? 0,
            0, 0, 0, 0,
            text,
            body,
            parent,
            ParentIndex: parentIndex,
            texture,
            graphicId,
            def?.Width ?? 0,
            def?.Height ?? 0,
            def?.PositionX ?? 0,
            def?.PositionY ?? 0,
            PersistScaleX: def?.ZoomX ?? 1f,
            PersistScaleY: def?.ZoomY ?? 1f,
            PositionIsCenter: def?.PositionIsCenter ?? false,
            Independant: def?.Independant ?? false,
            UseRelativePosition: def?.UseRelativePosition ?? false,
            UseRelativeZoom: def?.UseRelativeZoom ?? false,
            Visible: true,
            Enabled: true,
            DrawFromViewport: def?.DrawFromViewport ?? 0,
            ActiveChild: FrontendWidgetType.FirstSeenState,
            Font: font,
            FontFace: ResolveFontFace(font, names),
            ActionOnLeftUnclicked: def?.ActionOnLeftUnclicked ?? 0,
            ActionOnLeftClicked: def?.ActionOnLeftClicked ?? 0,
            HoveredState: def?.HoveredState ?? 0,
            LeftClickedState: def?.LeftClickedState ?? 0,
            RightClickedState: def?.RightClickedState ?? 0,
            InputDelay: def?.InputDelay ?? 0f,
            EditBoxCharLimit: def?.EditBoxCharLimit ?? 0,
            DisallowSpaceAsFirstChar: def?.DisallowSpaceAsFirstChar ?? false,
            Colour: ColourAtStyle(styleColours, FrontendWidgetType.FirstSeenState,
                def),
            Layer: def?.Layer ?? 0,
            PositionOffsetY: def?.PositionOffsetY ?? 0f,
            PositionOffsetX: def?.PositionOffsetX ?? 0f,
            ExpansionType: def?.ExpansionType ?? 0,
            StyleIndex: FrontendWidgetType.FirstSeenState,
            Flag302: PackFlag302(def),
            Alignement: def?.Alignement ?? 0,
            StyleColours: styleColours,
            StyleDurations: def?.StyleDurations,
            SwappingStates: def?.SwappingStates,
            SwappingTimes: def?.SwappingTimes,
            SwapCurrentState: def?.SwappingStates.FirstOrDefault() ?? int.MinValue));
    }

    /// <summary>
    /// <c>005331A0</c> DrawFromViewport bit 0 from
    /// <c>+392</c>, PositionIsCenter bit 1 from
    /// <c>+188</c>, remap bits 6/7 from
    /// <c>+520/+521</c>. Type-6
    /// <c>0054ED90</c> align from
    /// <c>+508</c>.
    /// </summary>
    public static byte PackFlag302(FrontendUiDef? def)
    {
        if (def is null)
            return 0;
        byte flag = 0;
        if (def.DrawFromViewport != 0)
            flag |= 1;
        if (def.PositionIsCenter)
            flag |= 2;
        if (def.Alignement == 1)
            flag |= FrontendTextDraw.Flag302CentreBit;
        else if (def.Alignement == 2)
            flag |= FrontendTextDraw.Flag302RightBit;
        if (def.UseRelativeZoom)
            flag |= 0x40;
        if (def.UseRelativePosition)
            flag |= 0x80;
        return flag;
    }

    public static uint ColourAtStyle(FrontendWidget widget, int style) =>
        ColourAtStyle(widget.StyleColours, style, def: null, fallback: widget.Colour);

    /// <summary>
    /// Native <c>0052FE3C..0052FFA2</c> writes the final draw colour at
    /// <c>+148</c> by multiplying the widget colour by its inherited parent
    /// colour.  <c>vtbl+404</c> (persist absolute, <c>+300</c> bit 6) breaks
    /// that inheritance.  This is what makes the zero-alpha blending groups
    /// under the forest/sunbeam swaps suppress their otherwise opaque tiles.
    /// </summary>
    public static uint EffectiveColour(IReadOnlyList<FrontendWidget> widgets, int index)
    {
        if ((uint)index >= (uint)widgets.Count)
            return 0;

        var colour = widgets[index].Colour;
        var current = index;
        while (!widgets[current].Independant)
        {
            var parent = widgets[current].ParentIndex;
            if ((uint)parent >= (uint)widgets.Count)
                break;
            // The resident slot root is the outer draw boundary. Its menu
            // fade is handled by the slot/state path; descendants inherit
            // colours from containers below it.
            if (widgets[parent].ParentIndex < 0)
                break;
            colour = MultiplyArgb(colour, widgets[parent].Colour);
            current = parent;
        }

        return colour;
    }

    private static uint MultiplyArgb(uint child, uint parent)
    {
        static uint Channel(uint a, uint b) => (a * b + 127u) / 255u;

        var a = Channel(child >> 24, parent >> 24);
        var r = Channel((child >> 16) & 0xFFu, (parent >> 16) & 0xFFu);
        var g = Channel((child >> 8) & 0xFFu, (parent >> 8) & 0xFFu);
        var b = Channel(child & 0xFFu, parent & 0xFFu);
        return (a << 24) | (r << 16) | (g << 8) | b;
    }

    public static uint ColourAtStyle(
        IReadOnlyList<uint>? colours, int style, FrontendUiDef? def, uint fallback = 0)
    {
        if (colours is { Count: > 0 } && (uint)style < (uint)colours.Count)
            return colours[style];
        if (def is not null)
        {
            return FrontendFrameDump.PackPersistColour(
                def.ColourR, def.ColourG, def.ColourB, def.ColourA, def.HaveColourA);
        }

        return fallback;
    }

    public static List<uint> PackStyleColours(FrontendUiDef? def)
    {
        var packed = new List<uint>();
        if (def is null)
            return packed;
        var n = Math.Min(
            Math.Min(def.StyleColourR.Count, def.StyleColourG.Count),
            Math.Min(def.StyleColourB.Count, def.StyleColourA.Count));
        for (var i = 0; i < n; i++)
        {
            packed.Add(FrontendFrameDump.PackPersistColour(
                def.StyleColourR[i],
                def.StyleColourG[i],
                def.StyleColourB[i],
                def.StyleColourA[i],
                haveColourA: true));
        }

        return packed;
    }
}
