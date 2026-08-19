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
        Add(widgets, parsed, rootName, parent: null, sprites, lookupText, names);
        if (parsed is null)
            return widgets;
        AttachChildren(widgets, defs, parsed, rootName, sprites, lookupText, names);
        ApplyFirstSeenState(widgets);
        return widgets;
    }

    /// <summary>
    /// <c>0052C730</c> writes
    /// <c>+324/+328/+332=0</c>. Type 18
    /// keeps persist child 0. Type 5/10/12
    /// keep every +176 child. Visibility
    /// and enabled inherit from the parent.
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
                Clip = false,
                ActiveChild = FrontendWidgetType.FirstSeenState,
                State = FrontendWidgetType.FirstSeenState,
            };
        }

        for (var i = 0; i < widgets.Count; i++)
        {
            if (!FrontendWidgetType.SelectsChild(widgets[i].Type))
                continue;
            var kids = ChildrenOf(widgets, widgets[i].Name);
            var active = FrontendWidgetType.FirstSeenState;
            widgets[i] = widgets[i] with { ActiveChild = active };
            for (var k = 0; k < kids.Count; k++)
            {
                if (k == active)
                    continue;
                var child = kids[k];
                widgets[child] = widgets[child] with { Visible = false };
            }
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
                Visible = widgets[i].Visible && inherit.Visible && !inherit.Clip,
                Enabled = widgets[i].Enabled && inherit.Enabled,
                Clip = widgets[i].Clip || inherit.Clip,
            };
        }
    }

    public static List<int> ChildrenOf(IReadOnlyList<FrontendWidget> widgets, string? parent)
    {
        var kids = new List<int>();
        for (var i = 0; i < widgets.Count; i++)
        {
            if (string.Equals(widgets[i].ParentName, parent, StringComparison.Ordinal))
                kids.Add(i);
        }

        return kids;
    }

    private static void AttachChildren(
        List<FrontendWidget> widgets,
        GameBin defs,
        FrontendUiDef parent,
        string parentName,
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
            Add(widgets, child, child.InstanceName, parentName, sprites, lookupText, names);
            AttachChildren(
                widgets, defs, child, child.InstanceName, sprites, lookupText, names);
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
        FrontendSpriteBank? sprites,
        Func<string, string?>? lookupText,
        NamesBin? names)
    {
        var text = def?.TextTag;
        string? body = null;
        if (!string.IsNullOrEmpty(text) && lookupText is not null)
            body = lookupText(text);
        var graphicId = def?.GraphicBankId ?? 0;
        var texture = sprites?.NameForWidget(name, graphicId);
        var font = def?.Font ?? 0;
        widgets.Add(new FrontendWidget(
            name,
            def?.Type ?? 0,
            0, 0, 0, 0,
            text,
            body,
            parent,
            texture,
            graphicId,
            def?.Width ?? 0,
            def?.Height ?? 0,
            def?.PositionX ?? 0,
            def?.PositionY ?? 0,
            PersistScaleX: def?.ZoomX ?? 1f,
            PersistScaleY: def?.ZoomY ?? 1f,
            Center: def?.Center ?? false,
            Absolute: def?.Absolute ?? false,
            ScaleOriginToViewport: def?.ScaleOriginToViewport ?? false,
            ScaleSizeToViewport: def?.ScaleSizeToViewport ?? false,
            Visible: true,
            Enabled: true,
            Clip: false,
            ActiveChild: FrontendWidgetType.FirstSeenState,
            Font: font,
            FontFace: ResolveFontFace(font, names),
            MessageId: def?.MessageId ?? 0,
            Plus224: def?.Plus224 ?? 0,
            Colour: FrontendFrameDump.PackPersistColour(
                def?.ColourR ?? 0f,
                def?.ColourG ?? 0f,
                def?.ColourB ?? 0f,
                def?.ColourA ?? 0f)));
    }
}
