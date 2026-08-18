using Fable.Core;
using Fable.Formats.Banks;
using Fable.Formats.Defs;
using Fable.Formats.Textures;
using Fable.Game;

internal static class TransformDump
{
    public static void Run()
    {
        var install = GameInstall.TryLocate() ?? throw new InvalidOperationException("no install");
        var names = NamesBin.Load(install.FindCompiledDef("names.bin")!);
        var bin = GameBin.Load(install.FindCompiledDef("frontend.bin")!, names);
        using var sprites = new FrontendSpriteBank(install);

        var candidates = new[]
        {
            "Centre", "Center", "Centred", "Centered", "CentreX", "CentreY",
            "CenterX", "CenterY", "UseCentre", "UseCenter", "IsCentre", "IsCenter",
            "Absolute", "AbsolutePosition", "AbsolutePos", "AbsoluteX", "AbsoluteY",
            "ScaleToScreen", "ScaleToViewport", "ScalePosition", "ScaleSize",
            "ScaleOrigin", "ScaleOriginToViewport", "ScaleSizeToViewport",
            "ScaleX", "ScaleY", "ScaleWidth", "ScaleHeight",
            "ScalePositionX", "ScalePositionY", "ScalePos", "ScalePosX", "ScalePosY",
            "Relative", "RelativePosition", "ScreenSpace", "UseScreen",
            "FitToScreen", "FitToViewport", "Stretch", "StretchToScreen",
            "ResolutionScale", "ResScale", "ScaleWithResolution",
            "KeepSize", "KeepPosition", "NoScale", "DontScale",
            "PixelScale", "PixelPerfect", "FixedSize", "FixedPosition",
            "OriginScale", "SizeScale", "ScaleOn", "AutoScale",
            "Align", "Alignment", "HAlign", "VAlign", "Justification",
            "Justify", "HJustification", "VJustification",
            "Left", "Right", "Top", "Bottom", "Middle",
            "Flags", "Flag", "State", "Mode",
            "Visible", "Visibility", "Enabled", "Hidden", "Active",
            "Clip", "Wrap", "AutoSize", "Tile", "Tiled",
            "Layer", "Angle", "Rotation",
            "UseGraphicSize", "UseTextureSize", "SizeFromGraphic",
            "GraphicSize", "TextureSize", "SpriteSize",
            "HasGraphic", "HasTexture",
            "IgnoreParent", "Detach", "WorldSpace",
            "Percent", "Normalised", "Normalized", "UnitCoords",
            "ViewportRelative", "ScreenRelative",
            "ScaleToDisplay", "DisplayScale",
            "XScaleToScreen", "YScaleToScreen",
            "ScaleXToScreen", "ScaleYToScreen",
            "PositionScale", "DimensionScale",
            "bCentre", "bCenter", "bAbsolute", "bScale",
            "m_bCentre", "m_bCenter", "m_bAbsolute",
            "m_Centre", "m_Center", "m_Absolute",
            "m_bScaleToScreen", "m_bScalePosition", "m_bScaleSize",
            "m_bScaleX", "m_bScaleY",
            "CentreAlign", "CenterAlign", "CentredAlign",
            "IsAbsolute", "IsRelative", "IsScaled",
            "DoScale", "DoCentre", "DoCenter",
            "ScaleTo640", "From640", "AuthoredScale",
            "Widescreen", "Letterbox",
            "OffsetX", "OffsetY",
            "DrawAtCentre", "DrawAtCenter",
            "OriginAtCentre", "OriginAtCenter",
            "Hotspot", "Pivot", "Anchor", "AnchorX", "AnchorY",
            "Unknown0961", "Unknown38BB",
        };

        Console.WriteLine("=== candidate CRC hits in frontend.bin UI ===");
        foreach (var name in candidates)
        {
            var crc = FableCrc.Hash(name);
            var hits = CountCrc(bin, crc);
            if (hits > 0)
                Console.WriteLine($"  {name} crc=0x{crc:X8} hits={hits}");
        }

        Console.WriteLine();
        Console.WriteLine("=== names.bin field-ish ===");
        foreach (var named in names.Entries)
        {
            if (named.Name.Length > 32)
                continue;
            if (named.Name.Contains("Centre", StringComparison.OrdinalIgnoreCase) ||
                named.Name.Contains("Center", StringComparison.OrdinalIgnoreCase) ||
                named.Name.Contains("Absolute", StringComparison.OrdinalIgnoreCase) ||
                named.Name.Contains("ScaleTo", StringComparison.OrdinalIgnoreCase) ||
                named.Name.Contains("ScalePos", StringComparison.OrdinalIgnoreCase) ||
                named.Name.Contains("ScaleSize", StringComparison.OrdinalIgnoreCase) ||
                named.Name.Equals("ScaleX", StringComparison.OrdinalIgnoreCase) ||
                named.Name.Equals("ScaleY", StringComparison.OrdinalIgnoreCase))
                Console.WriteLine($"  {named.Name} crc=0x{named.Hash:X8}");
        }

        var known = BuildKnown();
        var rootName = "UI_FRONTEND_PRESS_START_MENU";
        var widgets = FrontendWidgetFactory.Build(bin, rootName, sprites);
        Console.WriteLine();
        Console.WriteLine("=== unread CRC name brute ===");
        var unread = new Dictionary<uint, string>
        {
            [0x0961B216u] = "u0961",
            [0x38BB7ED4u] = "u38BB",
            [0x6B1015E4u] = "u6B10",
            [0xF81F10A8u] = "uF81F",
            [0xE78E700Eu] = "uE78E",
            [0x90894098u] = "u9089",
            [0xF97D3844u] = "uF97D",
            [0xA5F8D969u] = "uA5F8",
            [0x56A59976u] = "NESTED",
            [0xE215EF13u] = "Text",
        };
        var brute = new List<string>();
        foreach (var stem in new[]
                 {
                     "Centre", "Center", "Centred", "Centered", "Absolute", "Relative",
                     "Scale", "ScaleX", "ScaleY", "ScaleToScreen", "ScaleToViewport",
                     "ScalePosition", "ScaleSize", "ScaleOrigin", "ScaleWidth", "ScaleHeight",
                     "UseScreen", "FitToScreen", "Stretch", "Resolution", "Viewport",
                     "Align", "Alignment", "HAlign", "VAlign", "Visible", "Enabled",
                     "Clip", "Wrap", "AutoSize", "Layer", "Angle", "Rotation",
                     "XScale", "YScale", "SizeX", "SizeY", "Zoom", "ZoomX", "ZoomY",
                     "OriginX", "OriginY", "Hotspot", "Pivot", "Anchor",
                     "TextID", "StringID", "TextTag", "String", "Message",
                     "GraphicIndex", "Graphic", "Sprite", "Texture",
                     "AlwaysScale", "DoScale", "NoScale", "PixelScale",
                     "Normalised", "Normalized", "Percent",
                     "IgnoreParent", "World", "Screen",
                     "DrawCentre", "DrawCenter", "OriginCentre", "OriginCenter",
                     "KeepAspect", "Letterbox", "Widescreen",
                     "PosScale", "DimScale", "SizeScale",
                     "bCentre", "bCenter", "bAbsolute", "bScaleToScreen",
                     "m_bCentre", "m_bCenter", "m_bAbsolute",
                     "m_bScaleX", "m_bScaleY", "m_bScalePosition", "m_bScaleSize",
                     "ScalePos", "ScalePosX", "ScalePosY",
                     "From640", "ToScreen", "ScreenScale", "DisplayScale",
                     "Use640", "Base640", "Ref640",
                     "IsCentre", "IsCenter", "IsAbsolute",
                     "CentreAligned", "CenterAligned",
                     "ScaleWithScreen", "ScaleWithResolution",
                     "AdjustToScreen", "AdjustPosition", "AdjustSize",
                     "RelX", "RelY", "AbsX", "AbsY",
                     "UnitX", "UnitY", "NormX", "NormY",
                     "Fade", "FadeTime", "Blend", "BlendMode",
                     "Priority", "ZOrder", "Depth",
                     "MinWidth", "MinHeight", "MaxWidth", "MaxHeight",
                     "Padding", "Margin",
                     "Sound", "ClickSound",
                     "OnClick", "OnSelect",
                     "Id", "Name", "Parent",
                     "StateCount", "StyleCount", "NumStates",
                     "DefaultState", "CurrentState",
                     "Alpha", "Colour", "Color",
                     "TextAlign", "TextAlignment", "HTextAlign",
                     "Multiline", "WordWrap",
                     "Shadow", "Outline",
                     "Input", "Focus", "Tab",
                     "Tooltip", "Help",
                     "Group", "Category",
                     "Locked", "Modal",
                     "Inherit", "InheritScale", "InheritPos",
                     "Local", "Global",
                     "PixelOffset", "PixelPos",
                     "Virtual", "Logical",
                     "Canvas", "CanvasX", "CanvasY",
                     "SafeArea",
                     "Flip", "FlipX", "FlipY",
                     "Mirror", "MirrorX", "MirrorY",
                     "UVScale", "UScale", "VScale",
                     "Scroll", "ScrollX", "ScrollY",
                     "Offset", "Bias",
                     "Weight", "Order",
                     "Kind", "Class", "Role",
                     "Flags", "Bits", "Mask",
                     "On", "Off",
                     "Yes", "No",
                     "True", "False",
                     "EnableScale", "DisableScale",
                     "ScaleOn", "ScaleOff",
                     "CentreOn", "CenterOn",
                     "AbsoluteOn",
                     "PositionIsAbsolute", "SizeIsAbsolute",
                     "PositionIsRelative", "SizeIsRelative",
                     "PositionIsScreen", "SizeIsScreen",
                     "ScalePositionToScreen", "ScaleSizeToScreen",
                     "ScalePositionToViewport", "ScaleSizeToViewport",
                     "ApplyResolution", "ApplyScreenScale",
                     "ResolutionIndependent",
                     "FixedPixelSize", "FixedPixelPos",
                     "UseAuthoredSize", "UseAuthoredPos",
                     "Authored", "Design", "DesignSize",
                     "TargetWidth", "TargetHeight",
                     "RefWidth", "RefHeight",
                     "BaseWidth", "BaseHeight",
                     "ScreenX", "ScreenY",
                     "ViewX", "ViewY",
                     "NdcX", "NdcY",
                 })
        {
            brute.Add(stem);
            brute.Add(stem + "X");
            brute.Add(stem + "Y");
            brute.Add("b" + stem);
            brute.Add("m_" + stem);
            brute.Add("m_b" + stem);
        }

        foreach (var name in brute.Distinct())
        {
            var crc = FableCrc.Hash(name);
            if (unread.TryGetValue(crc, out var slot))
                Console.WriteLine($"  HIT {slot} = {name} 0x{crc:X8}");
        }

        Console.WriteLine();
        Console.WriteLine($"factory widgets={widgets.Count}");

        var viewport = FrontendLayout.FirstSeenFrontend(1024f, 768f);
        Console.WriteLine();
        Console.WriteLine("=== persist walk + native dest ===");
        Console.WriteLine("name | type | persist XYWH | leftover | graphic | flags C/A/o/s | dest");

        var dests = new Dictionary<string, FrontendDest>(StringComparer.OrdinalIgnoreCase);
        foreach (var widget in widgets)
        {
            var entry = bin.FindEntry(widget.Name);
            var parsed = entry is null ? null : FrontendUiDef.TryParse(entry);
            var seq = entry is null ? [] : WalkSequential(entry.Raw, entry.BodyOffset, known);
            DumpUnread(widget.Name, entry, parsed, seq, known);

            var leftover = Leftover(parsed, sprites);
            var flags = FlagsFromParsed(parsed, seq);
            FrontendDest? parentDest = null;
            if (widget.ParentName is { } pn && dests.TryGetValue(pn, out var p))
                parentDest = p;
            var layout = new FrontendWidgetLayout(
                widget.PersistX,
                widget.PersistY,
                PersistWidth: widget.PersistWidth > 0 ? (int)widget.PersistWidth : 0,
                PersistHeight: widget.PersistHeight > 0 ? (int)widget.PersistHeight : 0,
                LeftoverW: leftover.W,
                LeftoverH: leftover.H,
                Center: flags.Center,
                Absolute: flags.Absolute,
                ScaleOriginToViewport: flags.RemapOrigin,
                ScaleSizeToViewport: flags.RemapSize);
            var dest = FrontendLayout.Compute(layout, parentDest, viewport);
            dests[widget.Name] = dest;

            var currentLeftover = CurrentLeftover(widget, sprites);
            var current = FrontendLayout.Compute(
                new FrontendWidgetLayout(
                    widget.PersistX, widget.PersistY,
                    PersistWidth: widget.PersistWidth > 0 ? (int)widget.PersistWidth : 0,
                    PersistHeight: widget.PersistHeight > 0 ? (int)widget.PersistHeight : 0,
                    LeftoverW: currentLeftover.W,
                    LeftoverH: currentLeftover.H,
                    Center: false),
                parentDest is { } pd
                    ? new FrontendDest(pd.OriginX, pd.OriginY, pd.ScaleX, pd.ScaleY, pd.X0, pd.Y0, pd.X1, pd.Y1)
                    : null,
                viewport);

            var diverge = dest.X0 != current.X0 || dest.Y0 != current.Y0 ||
                          dest.X1 != current.X1 || dest.Y1 != current.Y1;
            Console.WriteLine(
                $"{widget.Name} t={widget.Type} xy={widget.PersistX},{widget.PersistY} " +
                $"wh={widget.PersistWidth}x{widget.PersistHeight} leftover={leftover.W}x{leftover.H} " +
                $"g={widget.GraphicId} flags={Bool(flags.Center)}{Bool(flags.Absolute)}" +
                $"{Bool(flags.RemapOrigin)}{Bool(flags.RemapSize)} " +
                $"native={dest.X0},{dest.Y0},{dest.X1},{dest.Y1} " +
                $"csharp={current.X0},{current.Y0},{current.X1},{current.Y1} " +
                $"{(diverge ? "DIVERGE" : "match")}");
        }
    }

    private static string Bool(bool value) => value ? "1" : "0";

    private static int CountCrc(GameBin bin, uint crc)
    {
        var hits = 0;
        foreach (var entry in bin.Entries)
        {
            if (entry.TypeName != "UI")
                continue;
            var raw = entry.Raw;
            for (var i = 0; i + 4 <= raw.Length; i++)
            {
                if (BitConverter.ToUInt32(raw, i) == crc)
                    hits++;
            }
        }

        return hits;
    }

    private static Dictionary<uint, string> BuildKnown()
    {
        var map = new Dictionary<uint, string>
        {
            [FrontendUiDef.TypeCrc] = "Type",
            [FrontendUiDef.ChildrenCrc] = "Children",
            [FrontendUiDef.WidthCrc] = "Width",
            [FrontendUiDef.HeightCrc] = "Height",
            [FrontendUiDef.PositionXCrc] = "PositionX",
            [FrontendUiDef.PositionYCrc] = "PositionY",
            [FrontendUiDef.TextTagCrc] = "Text",
            [FrontendUiDef.FontCrc] = "Font",
            [FrontendUiDef.LayerCrc] = "Layer",
            [FrontendUiDef.AngleCrc] = "Angle",
            [FrontendUiDef.Unknown0961Crc] = "u0961",
            [FrontendUiDef.Unknown38BBCrc] = "u38BB",
            [FrontendUiDef.SpritesCrc] = "Sprites",
            [FrontendUiDef.Unknown6B10Crc] = "u6B10",
            [FrontendUiDef.UnknownF81FCrc] = "uF81F",
            [FrontendUiDef.StatesCrc] = "States",
            [FrontendUiDef.UnknownE78ECrc] = "uE78E",
            [FrontendUiDef.Unknown9089Crc] = "u9089",
            [FrontendUiDef.ColourRCrc] = "ColourR",
            [FrontendUiDef.ColourGCrc] = "ColourG",
            [FrontendUiDef.ColourBCrc] = "ColourB",
            [FrontendUiDef.ColourACrc] = "ColourA",
            [FrontendUiDef.UnknownF97DCrc] = "uF97D",
            [FrontendUiDef.UnknownA5F8Crc] = "uA5F8",
            [FrontendUiDef.UnreadNestedCrc] = "NESTED",
            [FrontendUiDef.GraphicIndexCrc] = "GraphicIndex",
        };
        foreach (var name in new[]
                 {
                     "Centre", "Center", "Absolute", "ScaleToScreen", "ScalePosition",
                     "ScaleSize", "ScaleX", "ScaleY", "ScaleOrigin", "Visible",
                 })
            map.TryAdd(FableCrc.Hash(name), name);
        return map;
    }

    private static List<(int Off, uint Crc, string Name, string Value)> WalkSequential(
        byte[] raw, int start, Dictionary<uint, string> known)
    {
        var list = new List<(int, uint, string, string)>();
        var cursor = start;
        if (cursor + 6 <= raw.Length &&
            BitConverter.ToUInt16(raw, cursor) == 0 &&
            BitConverter.ToUInt32(raw, cursor + 2) == FrontendUiDef.TypeCrc)
            cursor += 2;

        var steps = 0;
        while (cursor + 4 <= raw.Length && steps < 120)
        {
            var crc = BitConverter.ToUInt32(raw, cursor);
            known.TryGetValue(crc, out var name);
            name ??= $"0x{crc:X8}";
            var payload = cursor + 4;
            if (crc == FrontendUiDef.ChildrenCrc && payload + 4 <= raw.Length)
            {
                var n = BitConverter.ToInt32(raw, payload);
                list.Add((cursor, crc, name, $"n={n}"));
                cursor = payload + 4 + Math.Max(0, n) * 4;
                steps++;
                continue;
            }

            if (crc == FrontendUiDef.TextTagCrc)
            {
                var t = payload;
                var text = ReadUtf16(raw, ref t);
                list.Add((cursor, crc, name, text ?? ""));
                cursor = t;
                steps++;
                continue;
            }

            if (payload + 4 <= raw.Length)
            {
                var i32 = BitConverter.ToInt32(raw, payload);
                var f32 = BitConverter.ToSingle(raw, payload);
                var u8 = raw[payload];
                list.Add((cursor, crc, name, $"i32={i32} f32={f32} u8={u8}"));
                cursor = payload + 4;
            }
            else
                break;

            steps++;
            if (crc == FrontendUiDef.UnreadNestedCrc)
                break;
        }

        return list;
    }

    private static void DumpUnread(
        string name,
        GameBinEntry? entry,
        FrontendUiDef? parsed,
        List<(int Off, uint Crc, string Name, string Value)> seq,
        Dictionary<uint, string> known)
    {
        if (entry is null)
            return;
        var interesting = name is
            "UI_FRONTEND_PRESS_START_MENU" or "UI_TITLE" or "UI_TITLE_01" or
            "UI_TITLE_02" or "UI_PRESS_START_TEXT" or "UI_FRONTEND_BG_FORREST_1_1" or
            "UI_MOUSE_POINTER" or "UI_BLENDING_BACKGROUNDS_FORREST";
        if (!interesting)
            return;

        Console.WriteLine();
        Console.WriteLine($"----- {name} raw={entry.Raw.Length} unreadOff={parsed?.UnreadOffset} -----");
        foreach (var field in seq)
            Console.WriteLine($"  @{field.Off:D4} {field.Name} {field.Value}");
        if (parsed is { Partial: true })
        {
            var raw = entry.Raw;
            var tail = parsed.UnreadOffset;
            Console.WriteLine($"  tail hex @{tail} {Convert.ToHexString(raw.AsSpan(tail, Math.Min(64, raw.Length - tail)))}");
            var layerOff = -1;
            for (var i = tail; i + 4 <= raw.Length; i++)
            {
                var crc = BitConverter.ToUInt32(raw, i);
                if (crc == FrontendUiDef.LayerCrc && layerOff < 0)
                    layerOff = i;
                if (!known.TryGetValue(crc, out var label))
                    continue;
                Console.WriteLine($"  tail-crc @{i:D4} {label} 0x{crc:X8}");
            }

            if (layerOff >= 0)
            {
                Console.WriteLine($"  after-layer @{layerOff} len={raw.Length - layerOff}");
                DumpAfterLayer(raw, layerOff, known);
            }
        }
    }

    private static (float W, float H) Leftover(FrontendUiDef? parsed, FrontendSpriteBank sprites)
    {
        if (parsed is null || parsed.GraphicBankId == 0)
            return (0f, 0f);
        var texName = sprites.TryNameForId(parsed.GraphicBankId);
        if (texName is null)
            return (0f, 0f);
        var tex = sprites.TryLoad(texName);
        if (tex is null)
            return (0f, 0f);
        return (tex.Width, tex.Height);
    }

    private static (float W, float H) CurrentLeftover(FrontendWidget widget, FrontendSpriteBank sprites)
    {
        var leftoverW = 0f;
        var leftoverH = 0f;
        if (widget.TextureName is { } name)
        {
            var tex = sprites.TryLoad(name);
            if (tex is not null)
            {
                leftoverW = tex.Width;
                leftoverH = tex.Height;
            }
        }

        return (leftoverW, leftoverH);
    }

    private static (bool Center, bool Absolute, bool RemapOrigin, bool RemapSize) FlagsFromParsed(
        FrontendUiDef? parsed,
        List<(int Off, uint Crc, string Name, string Value)> seq)
    {
        _ = parsed;
        _ = seq;
        return (false, false, false, false);
    }

    private static void DumpAfterLayer(byte[] raw, int layerOff, Dictionary<uint, string> known)
    {
        var cursor = layerOff;
        var steps = 0;
        while (cursor + 4 <= raw.Length && steps < 80)
        {
            var crc = BitConverter.ToUInt32(raw, cursor);
            known.TryGetValue(crc, out var name);
            name ??= $"0x{crc:X8}";
            var payload = cursor + 4;
            if (payload + 4 > raw.Length)
            {
                Console.WriteLine($"    @{cursor:D4} {name} short");
                break;
            }

            var i32 = BitConverter.ToInt32(raw, payload);
            var f32 = BitConverter.ToSingle(raw, payload);
            var u8 = raw[payload];
            Console.WriteLine($"    @{cursor:D4} {name} i32={i32} f32={f32} u8={u8} hex={Convert.ToHexString(raw.AsSpan(payload, Math.Min(8, raw.Length - payload)))}");
            cursor = payload + 4;
            steps++;
        }
    }

    private static string? ReadUtf16(byte[] raw, ref int cursor)
    {
        var start = cursor;
        while (cursor + 1 < raw.Length)
        {
            var ch = BitConverter.ToUInt16(raw, cursor);
            cursor += 2;
            if (ch == 0)
                break;
        }

        var bytes = cursor - start;
        if (bytes < 2)
            return null;
        var text = System.Text.Encoding.Unicode.GetString(raw, start, bytes);
        var nul = text.IndexOf('\0');
        return nul >= 0 ? text[..nul] : text;
    }
}
