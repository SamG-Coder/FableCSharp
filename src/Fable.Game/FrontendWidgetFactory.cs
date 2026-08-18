using Fable.Formats.Banks;
using Fable.Formats.Defs;
using Fable.Formats.Text;

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
        Func<string, string?>? lookupText = null)
    {
        var widgets = new List<FrontendWidget>();
        var root = defs.FindEntry(rootName);
        var parsed = root is null ? null : FrontendUiDef.TryParse(root);
        Add(widgets, parsed, rootName, parent: null, sprites, lookupText);
        if (parsed is null)
            return widgets;
        AttachChildren(widgets, defs, parsed, rootName, sprites, lookupText);
        return widgets;
    }

    private static void AttachChildren(
        List<FrontendWidget> widgets,
        GameBin defs,
        FrontendUiDef parent,
        string parentName,
        FrontendSpriteBank? sprites,
        Func<string, string?>? lookupText)
    {
        foreach (var index in parent.ChildIndices)
        {
            if ((uint)index >= (uint)defs.Entries.Count)
                continue;
            var child = FrontendUiDef.TryParse(defs.Entries[index]);
            if (child is null)
                continue;
            Add(widgets, child, child.InstanceName, parentName, sprites, lookupText);
            AttachChildren(widgets, defs, child, child.InstanceName, sprites, lookupText);
        }
    }

    private static void Add(
        List<FrontendWidget> widgets,
        FrontendUiDef? def,
        string name,
        string? parent,
        FrontendSpriteBank? sprites,
        Func<string, string?>? lookupText)
    {
        var text = def?.TextTag;
        string? body = null;
        if (!string.IsNullOrEmpty(text) && lookupText is not null)
            body = lookupText(text);
        var graphicId = def?.GraphicBankId ?? 0;
        var texture = sprites?.NameForWidget(name, graphicId);
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
            def?.PositionY ?? 0));
    }
}
