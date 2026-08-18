using System.Text;
using Fable.Core;
using Fable.Formats.Defs;

if (args.Contains("--transform"))
{
    TransformDump.Run();
    return;
}

var install = GameInstall.TryLocate() ?? throw new InvalidOperationException("no install");
var names = NamesBin.Load(install.FindCompiledDef("names.bin")!);
var bin = GameBin.Load(install.FindCompiledDef("frontend.bin")!, names);

if (args.Contains("--520"))
{
    Dump520(bin, names);
    return;
}

var want = args.Where(a => a is not "--transform" and not "--520").ToArray();
if (want.Length == 0)
    want = new[]
    {
        "UI_FRONTEND_PRESS_START_MENU",
        "UI_TITLE",
        "UI_TITLE_01",
        "UI_TITLE_02",
        "UI_BLENDING_BACKGROUNDS_FORREST",
        "UI_PRESS_START_TEXT",
        "UI_FRONTEND_NEW_PROFILE_SCREEN",
        "UI_NEW_PROFILE_EDIT_BOX",
        "UI_ACCEPT_NEW_PROFILE",
        "UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE",
        "UI_TEXT_NEW_GAME",
        "UI_MOUSE_POINTER",
        "UI_FRONTEND_BG_FORREST_1_1",
    };

var crcToName = new Dictionary<uint, string>();
foreach (var named in names.Entries)
{
    if (!crcToName.ContainsKey(named.Hash))
        crcToName[named.Hash] = named.Name;
}

foreach (var extra in new[]
{
    "Type", "Children", "Width", "Height", "PositionX", "PositionY",
    "Text", "TextTag", "Graphic", "Texture", "Sprite", "Visible",
    "Anchor", "ScaleX", "ScaleY", "Colour", "Color", "Font", "Message",
})
{
    var crc = FableCrc.Hash(extra);
    crcToName.TryAdd(crc, extra);
}

var known = crcToName.Select(kv => (Name: kv.Value, Crc: kv.Key)).ToArray();

var candidates = new[]
{
    "Type", "Children", "Width", "Height", "PositionX", "PositionY",
    "Text", "TextTag", "String", "StringId", "Localisation", "Localization",
    "Graphic", "Graphics", "Texture", "Sprite", "Material", "Bitmap",
    "Visible", "Visibility", "Enabled", "Hidden", "Active",
    "Anchor", "Align", "Alignment", "HAlign", "VAlign",
    "Scale", "ScaleX", "ScaleY", "Centre", "Center",
    "Colour", "Color", "ColourTop", "ColourBottom", "Alpha",
    "Font", "FontName", "Face", "FontId",
    "Message", "MessageId", "Id", "Name", "Parent",
    "Layer", "Z", "ZOrder", "Priority",
    "OffsetX", "OffsetY", "Left", "Top", "Right", "Bottom",
    "U0", "V0", "U1", "V1", "FlipU", "FlipV",
    "Blend", "BlendMode", "Additive",
    "Sound", "ClickSound", "HoverSound",
    "Next", "Prev", "Target", "Screen",
    "ToolTip", "Tooltip", "Help",
    "MinWidth", "MinHeight", "MaxWidth", "MaxHeight",
    "AutoSize", "Wrap", "Clip",
    "ButtonId", "ControlId", "WidgetId",
    "Background", "Foreground", "Highlight",
    "Normal", "Hover", "Pressed", "Disabled",
    "Tile", "Tiled", "Repeat",
    "Rotation", "Angle",
};

Console.WriteLine($"frontend.bin entries={bin.Entries.Count}");
Console.WriteLine("names.bin UI/graphic-ish:");
foreach (var named in names.Search("UI_"))
{
    if (named.Name.Contains("SPRITE", StringComparison.OrdinalIgnoreCase) ||
        named.Name.Contains("GRAPHIC", StringComparison.OrdinalIgnoreCase) ||
        named.Name.Contains("TEXTURE", StringComparison.OrdinalIgnoreCase) ||
        named.Name.Contains("PICTURE", StringComparison.OrdinalIgnoreCase) ||
        named.Name.Contains("IMAGE", StringComparison.OrdinalIgnoreCase) ||
        named.Name.Contains("BITMAP", StringComparison.OrdinalIgnoreCase) ||
        named.Name.Contains("MATERIAL", StringComparison.OrdinalIgnoreCase))
        Console.WriteLine($"  name {named.Name} crc=0x{named.Hash:X8}");
}

foreach (var field in new[]
{
    "Picture", "Image", "Bitmap", "MaterialName", "GraphicName",
    "TextureName", "SpriteName", "Resource", "ResourceName",
    "BackgroundGraphic", "ForegroundGraphic", "NormalGraphic",
    "HighlightGraphic", "SelectedGraphic", "DisabledGraphic",
    "LeftGraphic", "RightGraphic", "MidGraphic",
    "U", "V", "UV", "TexU", "TexV",
    "DrawWidth", "DrawHeight", "SizeX", "SizeY",
    "X", "Y", "PosX", "PosY",
    "AbsoluteX", "AbsoluteY", "RelX", "RelY",
    "CentreX", "CentreY", "CenterX", "CenterY",
    "Justification", "Justify", "HJustification", "VJustification",
    "TextID", "StringID", "LocText", "TextString",
    "OnClick", "OnSelect", "Event", "Action",
    "Unknown", "Flags", "Flag", "State",
    "Child", "ChildList", "Widgets",
    "ScaleWidth", "ScaleHeight",
})
{
    var crc = FableCrc.Hash(field);
    crcToName.TryAdd(crc, field);
}

Console.WriteLine("crc candidates:");
foreach (var name in candidates)
{
    var crc = FableCrc.Hash(name);
    var hits = 0;
    foreach (var e in bin.Entries)
    {
        if (e.TypeName != "UI")
            continue;
        var raw = e.Raw;
        for (var i = 0; i + 4 <= raw.Length; i++)
        {
            if (BitConverter.ToUInt32(raw, i) == crc)
                hits++;
        }
    }

    if (hits > 0)
        Console.WriteLine($"  {name} crc=0x{crc:X8} hits={hits}");
}

foreach (var name in want)
{
    var entry = bin.FindEntry(name);
    Console.WriteLine();
    Console.WriteLine($"===== {name} =====");
    if (entry is null)
    {
        Console.WriteLine("MISSING");
        continue;
    }

    Console.WriteLine($"index={entry.Index} type={entry.TypeName} inst={entry.InstanceName} src={entry.SourceName} raw={entry.Raw.Length} body={entry.BodyOffset}");
    DumpRaw(entry.Raw, crcToName);
    Console.WriteLine("sequential:");
    DumpSequential(entry.Raw, crcToName, entry.BodyOffset);
}

static void DumpSequential(byte[] raw, Dictionary<uint, string> crcToName, int start)
{
    var i = start;
    var steps = 0;
    while (i + 4 <= raw.Length && steps < 80)
    {
        var crc = BitConverter.ToUInt32(raw, i);
        if (!crcToName.TryGetValue(crc, out var label))
        {
            i++;
            continue;
        }

        var payload = i + 4;
        Console.Write($"  seq @{i:D4} {label} 0x{crc:X8}");
        if (payload + 4 <= raw.Length)
        {
            var i32 = BitConverter.ToInt32(raw, payload);
            var f32 = BitConverter.ToSingle(raw, payload);
            Console.Write($" i32={i32} f32={f32}");
        }

        Console.WriteLine();
        if (label == "Children" && payload + 4 <= raw.Length)
        {
            var n = BitConverter.ToInt32(raw, payload);
            i = payload + 4 + Math.Max(0, n) * 4;
        }
        else if (label is "Text" or "TextTag" or "String" or "Font" or "Texture" or "Sprite" or "Graphic")
        {
            var t = payload;
            if (t + 1 < raw.Length && raw[t] != 0 && raw[t + 1] == 0)
            {
                while (t + 1 < raw.Length)
                {
                    var ch = BitConverter.ToUInt16(raw, t);
                    t += 2;
                    if (ch == 0)
                        break;
                }

                i = t;
            }
            else
                i = payload + 4;
        }
        else
            i = payload + 4;
        steps++;
    }
}

static void DumpRaw(byte[] raw, Dictionary<uint, string> crcToName)
{
    Console.WriteLine($"hex[0..{Math.Min(64, raw.Length)}]={Convert.ToHexString(raw.AsSpan(0, Math.Min(64, raw.Length)))}");
    for (var i = 0; i + 4 <= raw.Length; i++)
    {
        var crc = BitConverter.ToUInt32(raw, i);
        if (!crcToName.TryGetValue(crc, out var label))
            continue;

        var payload = i + 4;
        Console.Write($"  @{i:D4} {label} 0x{crc:X8}");
        if (label == "Type" && payload + 4 <= raw.Length)
            Console.Write($" i32={BitConverter.ToInt32(raw, payload)}");
        else if (label is "Width" or "Height" or "PositionX" or "PositionY" && payload + 4 <= raw.Length)
            Console.Write($" f32={BitConverter.ToSingle(raw, payload)}");
        else if (label == "Children" && payload + 4 <= raw.Length)
        {
            var n = BitConverter.ToInt32(raw, payload);
            Console.Write($" n={n}");
            var p = payload + 4;
            for (var c = 0; c < n && p + 4 <= raw.Length; c++, p += 4)
                Console.Write($" [{BitConverter.ToInt32(raw, p)}]");
        }
        else if (label == "Text" && payload + 2 <= raw.Length)
        {
            var t = payload;
            var sb = new StringBuilder();
            while (t + 1 < raw.Length)
            {
                var ch = BitConverter.ToUInt16(raw, t);
                t += 2;
                if (ch == 0)
                    break;
                sb.Append((char)ch);
            }

            Console.Write($" utf16={sb}");
        }

        Console.WriteLine();
    }
}

static void Dump520(GameBin bin, NamesBin names)
{
    var crcToName = new Dictionary<uint, string>();
    foreach (var named in names.Entries)
        crcToName.TryAdd(named.Hash, named.Name);
    foreach (var extra in new[]
    {
        "Type", "Children", "Width", "Height", "PositionX", "PositionY",
        "Text", "TextTag", "Font", "GraphicIndex", "Sprites", "States",
        "ColourR", "ColourG", "ColourB", "ColourA", "Centre", "Center",
        "ScaleX", "ScaleY", "Scale", "Absolute", "ScaleToScreen",
        "ScaleToResolution", "ScaleWithResolution", "UseResolutionScale",
        "ResolutionScale", "AutoScale", "ScalePosition", "ScaleSize",
        "ScaleOrigin", "ScalePos", "FitToScreen", "RelativeToScreen",
        "ScreenRelative", "PixelScale", "IsScaled", "Scaled", "Stretch",
        "StretchToFit", "MaintainAspect", "ScaleWidth", "ScaleHeight",
        "Centred", "Centered", "IsCentred", "IsCentered",
        "ScaleXToScreen", "ScaleYToScreen", "Rescale", "RescaleX",
        "RescaleY", "UseScreenSize", "UseDisplaySize", "AdjustForResolution",
        "LockTo640", "LockTo480", "Normalise", "Normalize",
        "ScaleFrom640", "ScaleFrom480", "Widescreen",
    })
        crcToName.TryAdd(FableCrc.Hash(extra), extra);

    Console.WriteLine("names.bin scale/remap-ish:");
    foreach (var named in names.Entries)
    {
        var n = named.Name;
        if (n.Contains("SCALE", StringComparison.OrdinalIgnoreCase) ||
            n.Contains("REMAP", StringComparison.OrdinalIgnoreCase) ||
            n.Contains("RESOL", StringComparison.OrdinalIgnoreCase) ||
            n.Contains("STRETCH", StringComparison.OrdinalIgnoreCase) ||
            n.Contains("VIEWPORT", StringComparison.OrdinalIgnoreCase) ||
            n.Contains("640", StringComparison.Ordinal) ||
            n.Contains("SCREEN", StringComparison.OrdinalIgnoreCase) &&
                (n.Contains("UI", StringComparison.OrdinalIgnoreCase) ||
                 n.Contains("SCALE", StringComparison.OrdinalIgnoreCase)))
            Console.WriteLine($"  {n} crc=0x{named.Hash:X8}");
    }

    var widgets = new[]
    {
        "UI_FRONTEND_PRESS_START_MENU",
        "UI_TITLE",
        "UI_TITLE_01",
        "UI_TITLE_02",
        "UI_BLENDING_BACKGROUNDS_FORREST",
        "UI_FRONTEND_BG_FORREST_1_1",
        "UI_PRESS_START_TEXT",
        "UI_MOUSE_POINTER",
        "UI_PRESS_START_SWAP",
        "UI_FRONTEND_LIST_PRESS_START_MENU",
        "UI_LEGAL_TEXT",
    };

    foreach (var name in widgets)
    {
        var entry = bin.FindEntry(name);
        Console.WriteLine();
        Console.WriteLine($"===== {name} =====");
        if (entry is null)
        {
            Console.WriteLine("MISSING");
            continue;
        }

        Console.WriteLine($"raw={entry.Raw.Length} body={entry.BodyOffset}");
        var parsed = FrontendUiDef.TryParse(entry);
        if (parsed is not null)
            Console.WriteLine($"parse type={parsed.Type} pos={parsed.PositionX},{parsed.PositionY} w={parsed.Width} h={parsed.Height} unread=0x{(parsed.UnreadCrcs.Count > 0 ? parsed.UnreadCrcs[0] : 0):X8} @{parsed.UnreadOffset} partial={parsed.Partial}");
        WalkCuiDef(entry.Raw, entry.BodyOffset, crcToName);
    }
}

static void WalkCuiDef(byte[] raw, int start, Dictionary<uint, string> crcToName)
{
    var c = start;
    if (c + 6 <= raw.Length &&
        BitConverter.ToUInt16(raw, c) == 0 &&
        BitConverter.ToUInt32(raw, c + 2) == FrontendUiDef.TypeCrc)
        c += 2;

    void Fail(string why)
    {
        Console.WriteLine($"  FAIL {why} @{c} remain={raw.Length - c}");
        DumpHex(raw, c, 64);
    }

    bool Need(int n) => c + n <= raw.Length;

    uint PeekCrc() => Need(4) ? BitConverter.ToUInt32(raw, c) : 0;

    string Lab(uint crc) =>
        crcToName.TryGetValue(crc, out var n) ? n : $"crc=0x{crc:X8}";

    int ReadI32(string field)
    {
        if (!Need(8)) { Fail(field + " i32"); return 0; }
        var crc = BitConverter.ToUInt32(raw, c);
        var v = BitConverter.ToInt32(raw, c + 4);
        Console.WriteLine($"  {field} {Lab(crc)} i32={v} @{c}");
        c += 8;
        return v;
    }

    float ReadF32(string field)
    {
        if (!Need(8)) { Fail(field + " f32"); return 0; }
        var crc = BitConverter.ToUInt32(raw, c);
        var v = BitConverter.ToSingle(raw, c + 4);
        Console.WriteLine($"  {field} {Lab(crc)} f32={v} @{c}");
        c += 8;
        return v;
    }

    byte ReadU8(string field)
    {
        if (!Need(5)) { Fail(field + " u8"); return 0; }
        var crc = BitConverter.ToUInt32(raw, c);
        var v = raw[c + 4];
        Console.WriteLine($"  {field} {Lab(crc)} u8={v} @{c}");
        c += 5;
        return v;
    }

    string ReadStr(string field)
    {
        if (!Need(4)) { Fail(field + " str"); return ""; }
        var crc = BitConverter.ToUInt32(raw, c);
        c += 4;
        var t = c;
        var sb = new StringBuilder();
        while (t + 1 < raw.Length)
        {
            var ch = BitConverter.ToUInt16(raw, t);
            t += 2;
            if (ch == 0) break;
            sb.Append((char)ch);
        }
        Console.WriteLine($"  {field} {Lab(crc)} utf16={sb} @{c - 4}");
        c = t;
        return sb.ToString();
    }

    int ReadVecI32(string field)
    {
        var n = ReadI32(field + ".n");
        if (n is < 0 or > 256) { Fail(field + " bad n"); return 0; }
        for (var i = 0; i < n && Need(4); i++, c += 4)
            Console.WriteLine($"    [{i}]={BitConverter.ToInt32(raw, c)}");
        return n;
    }

    int ReadVecF32(string field)
    {
        var n = ReadI32(field + ".n");
        if (n is < 0 or > 256) { Fail(field + " bad n"); return 0; }
        for (var i = 0; i < n && Need(4); i++, c += 4)
            Console.WriteLine($"    [{i}]={BitConverter.ToSingle(raw, c)}");
        return n;
    }

    void ReadMapII(string field)
    {
        var n = ReadI32(field + ".n");
        if (n is < 0 or > 256) { Fail(field + " bad n"); return; }
        for (var i = 0; i < n && Need(8); i++, c += 8)
            Console.WriteLine($"    [{i}] {BitConverter.ToInt32(raw, c)} {BitConverter.ToInt32(raw, c + 4)}");
    }

    void ReadMapIStr(string field)
    {
        var n = ReadI32(field + ".n");
        if (n is < 0 or > 256) { Fail(field + " bad n"); return; }
        for (var i = 0; i < n; i++)
        {
            if (!Need(4)) { Fail(field + " key"); return; }
            var key = BitConverter.ToInt32(raw, c);
            c += 4;
            var t = c;
            var sb = new StringBuilder();
            while (t + 1 < raw.Length)
            {
                var ch = BitConverter.ToUInt16(raw, t);
                t += 2;
                if (ch == 0) break;
                sb.Append((char)ch);
            }
            Console.WriteLine($"    [{i}] key={key} {sb}");
            c = t;
        }
    }

    bool WalkStyle(int index)
    {
        Console.WriteLine($"  -- style[{index}] @{c} {Lab(PeekCrc())}");
        ReadI32($"style[{index}].GraphicIndex");
        ReadF32($"style[{index}].PositionX");
        ReadF32($"style[{index}].PositionY");
        ReadF32($"style[{index}].E78E");
        ReadF32($"style[{index}].9089");
        ReadF32($"style[{index}].ColourR");
        ReadF32($"style[{index}].ColourG");
        ReadF32($"style[{index}].ColourB");
        ReadF32($"style[{index}].ColourA");
        ReadF32($"style[{index}].F97D");
        ReadI32($"style[{index}].A5F8");
        ReadU8($"style[{index}].+120");
        ReadI32($"style[{index}].+64");
        ReadVecI32($"style[{index}].+108");
        return true;
    }

    ReadI32("+60 Type");
    ReadVecI32("+112 Children");
    ReadI32("+76");
    ReadStr("+84");
    ReadI32("+80 Font");
    ReadF32("+88 Height");
    ReadF32("+92 Width");
    ReadI32("+96");
    ReadI32("+100 Sprites");
    ReadVecI32("+124");
    ReadVecI32("+136");
    var states = ReadI32("+64 States");
    if (states is < 0 or > 16)
    {
        Fail("states");
        return;
    }

    for (var i = 0; i < states; i++)
    {
        if (!WalkStyle(i))
        {
            Console.WriteLine($"  style walk stopped, remain={raw.Length - c}");
            DumpHex(raw, c, 96);
            return;
        }
    }

    ReadU8("+189");
    ReadU8("+190");
    ReadU8("+191 Absolute");
    ReadI32("+160");
    ReadVecI32("+148");
    ReadF32("+164");
    ReadF32("+168");
    ReadF32("+172");
    ReadF32("+176");
    ReadI32("+180");
    ReadF32("+184");
    var centre = ReadU8("+188 Centre");
    ReadF32("+192");
    ReadU8("+320");
    ReadU8("+321");
    ReadF32("+322");
    ReadF32("+326");
    ReadI32("+330");
    ReadF32("+260");
    ReadF32("+264");
    ReadF32("+268");
    ReadF32("+272");
    ReadF32("+276");
    ReadF32("+280");
    ReadF32("+284");
    ReadF32("+288");
    ReadF32("+292");
    ReadF32("+296");
    ReadI32("+300");
    ReadI32("+304");
    ReadI32("+308");
    ReadI32("+312");
    ReadI32("+316");
    ReadU8("+331");
    ReadU8("+348");
    ReadF32("+364");
    ReadF32("+368");
    ReadF32("+372");
    ReadF32("+376");
    ReadF32("+380");
    ReadF32("+384");
    ReadF32("+356");
    ReadF32("+360");
    ReadI32("+396");
    ReadI32("+400");
    for (var i = 0; i < 16; i++)
        ReadI32($"+colourish[{i}]");
    ReadF32("+388");
    ReadU8("+392");
    ReadI32("+404");
    ReadI32("+408");
    ReadI32("+412");
    ReadI32("+420");
    ReadI32("+416");
    ReadMapIStr("+424");
    ReadMapII("+436");
    ReadVecI32("+448");
    ReadU8("+460");
    ReadU8("+461");
    ReadI32("+464");
    ReadU8("+468");
    ReadStr("+472");
    ReadU8("+469");
    ReadU8("+476");
    ReadVecI32("+480");
    ReadVecF32("+492");
    var b504 = ReadU8("+504");
    ReadI32("+508");
    var b512 = ReadU8("+512");
    var b520 = ReadU8("+520 remapSize");
    var b521 = ReadU8("+521 remapOrigin");
    Console.WriteLine($"  RESULT centre={centre} +504={b504} +512={b512} +520={b520} +521={b521} next=@{c} {Lab(PeekCrc())}");
}

static void DumpHex(byte[] raw, int off, int n)
{
    if (off < 0 || off >= raw.Length) return;
    n = Math.Min(n, raw.Length - off);
    Console.WriteLine($"  hex@{off}={Convert.ToHexString(raw.AsSpan(off, n))}");
}
