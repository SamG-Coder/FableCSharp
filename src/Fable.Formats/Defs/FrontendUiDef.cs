using System.Text;

namespace Fable.Formats.Defs;

/// <summary>
/// frontend.bin <c>UI</c> persist. Sequential CRC+typed
/// fields after the 3-byte GameBin header and a u16 0 pad.
/// Persist helpers <c>00431102</c> / <c>00431061</c> /
/// <c>0043314A</c> skip the field CRC then read the value.
/// Field CRCs are Lionhead names hashed with
/// <see cref="FableCrc"/>. Height is
/// <c>0x4323419A</c>, not the mistyped <c>0x4341A19A</c>.
/// </summary>
public sealed class FrontendUiDef
{
    public const uint TypeCrc = 0x0DA8270B;
    public const uint ChildrenCrc = 0x3DC30C85;
    public const uint WidthCrc = 0x8BF99D36;
    public const uint HeightCrc = 0x4323419A;
    public const uint PositionXCrc = 0x1EDB8A31;
    public const uint PositionYCrc = 0x69DCBAA7;
    /// <summary>
    /// UTF-16 localised id. Name is not
    /// <c>TextTag</c> (<c>FableCrc("TextTag")</c>
    /// is <c>0x66D9E7F9</c>). String UNREAD.
    /// </summary>
    public const uint TextTagCrc = 0xE215EF13;
    public const uint FontCrc = 0x51E278F0;
    public const uint LayerCrc = 0xE338F903;
    public const uint AngleCrc = 0x07629D10;
    public const uint Unknown0961Crc = 0x0961B216;
    public const uint Unknown38BBCrc = 0x38BB7ED4;
    public const uint SpritesCrc = 0x5E5D8A25;
    public const uint Unknown6B10Crc = 0x6B1015E4;
    public const uint UnknownF81FCrc = 0xF81F10A8;
    public const uint StatesCrc = 0x87ACD3D8;
    /// <summary>
    /// <c>FableCrc("ZoomX")</c>. First style
    /// scale, default 1. <c>0052F440</c>
    /// copies it to layout <c>+16</c> →
    /// widget <c>+92</c>.
    /// </summary>
    public const uint ZoomXCrc = 0xE78E700E;
    /// <summary>
    /// <c>FableCrc("ZoomY")</c>. First style
    /// scale, default 1. Layout <c>+20</c> →
    /// widget <c>+96</c>.
    /// </summary>
    public const uint ZoomYCrc = 0x90894098;
    public const uint UnknownE78ECrc = ZoomXCrc;
    public const uint Unknown9089Crc = ZoomYCrc;
    public const uint ColourRCrc = 0x79902E65;
    public const uint ColourGCrc = 0x144DCA8E;
    public const uint ColourBCrc = 0x64273E01;
    public const uint ColourACrc = 0xFD2E6FBB;
    public const uint UnknownF97DCrc = 0xF97D3844;
    public const uint UnknownA5F8Crc = 0xA5F8D969;
    /// <summary>
    /// CUIStateDef persist <c>00625630</c>
    /// <c>+120</c> u8. Not a nested object.
    /// </summary>
    public const uint UnreadNestedCrc = 0x56A59976;
    /// <summary>
    /// Style <c>+64</c> after
    /// <see cref="UnreadNestedCrc"/>.
    /// </summary>
    public const uint StylePlus64Crc = 0xF8D265DA;
    /// <summary>
    /// Style <c>+108</c> i32 vector after
    /// <see cref="StylePlus64Crc"/>.
    /// </summary>
    public const uint StylePlus108Crc = 0x2085F2AB;
    /// <summary>
    /// <c>005331A0</c> def <c>+188</c> →
    /// widget <c>+302</c> bit 1. Persist u8.
    /// Name UNREAD.
    /// </summary>
    public const uint CentreCrc = 0x64D3430E;
    /// <summary>
    /// <c>005331A0</c> def <c>+191</c> →
    /// widget <c>+300</c> bit 6. Persist u8.
    /// Name UNREAD.
    /// </summary>
    public const uint AbsoluteCrc = 0x38BBD87F;
    /// <summary>
    /// <c>005331A0</c> def <c>+520</c> →
    /// widget <c>+302</c> bit 6 remap size.
    /// Persist u8 <c>0043314A</c>. Name UNREAD.
    /// </summary>
    public const uint ScaleSizeCrc = 0xC50CA371;
    /// <summary>
    /// <c>005331A0</c> def <c>+521</c> →
    /// widget <c>+302</c> bit 7 remap origin.
    /// Persist u8 <c>0043314A</c>. Name UNREAD.
    /// </summary>
    public const uint ScaleOriginCrc = 0xB466D948;
    /// <summary>
    /// <c>FableCrc("GraphicIndex")</c>. Persist i32 is
    /// <c>GBANK_FRONT_END_PC</c> <c>BankEntry.Id</c>.
    /// </summary>
    public const uint GraphicIndexCrc = 0x38E36902;
    public const int HeaderBytes = 3;
    public const int StyleRecordBytes = 124;
    public const int StyleGraphicOffset = 60;
    public const uint PersistDwordFn = 0x00431102;
    public const uint PersistFloatFn = 0x00431061;
    public const uint PersistU8Fn = 0x0043314A;
    public const uint PersistStringFn = 0x004310A7;

    public required string InstanceName { get; init; }
    public int Type { get; init; }
    public IReadOnlyList<int> ChildIndices { get; init; } = [];
    public float Width { get; init; }
    public float Height { get; init; }
    public float PositionX { get; init; }
    public float PositionY { get; init; }
    public string? TextTag { get; init; }
    public int Font { get; init; }
    public int Layer { get; init; }
    public float Angle { get; init; }
    public int GraphicId { get; init; }
    public int GraphicBankId { get; init; }
    public int Sprites { get; init; }
    public int States { get; init; }
    public float ColourR { get; init; }
    public float ColourG { get; init; }
    public float ColourB { get; init; }
    public float ColourA { get; init; }
    public float ZoomX { get; init; } = 1f;
    public float ZoomY { get; init; } = 1f;
    /// <summary>
    /// <c>005331A0</c> <c>+302</c> bit 1 from
    /// def <c>+188</c> / <see cref="CentreCrc"/>.
    /// </summary>
    public bool Center { get; init; }
    /// <summary>
    /// <c>005331A0</c> <c>+300</c> bit 6 from
    /// def <c>+191</c> / <see cref="AbsoluteCrc"/>.
    /// </summary>
    public bool Absolute { get; init; }
    /// <summary>
    /// <c>005331A0</c> <c>+302</c> bit 7 from
    /// def <c>+521</c> / <see cref="ScaleOriginCrc"/>.
    /// </summary>
    public bool ScaleOriginToViewport { get; init; }
    /// <summary>
    /// <c>005331A0</c> <c>+302</c> bit 6 from
    /// def <c>+520</c> / <see cref="ScaleSizeCrc"/>.
    /// </summary>
    public bool ScaleSizeToViewport { get; init; }
    /// <summary>
    /// Raw persist u8 at def <c>+520</c>.
    /// </summary>
    public byte ScaleSizeByte { get; init; }
    /// <summary>
    /// Raw persist u8 at def <c>+521</c>.
    /// </summary>
    public byte ScaleOriginByte { get; init; }
    public IReadOnlyList<uint> UnreadCrcs { get; init; } = [];
    public int UnreadOffset { get; init; }
    public bool Partial { get; init; }

    public static FrontendUiDef? TryParse(GameBinEntry entry)
    {
        if (entry.TypeName != "UI" || entry.Raw.Length < 8)
            return null;
        var raw = entry.Raw;
        var cursor = entry.BodyOffset > 0 ? entry.BodyOffset : HeaderBytes;
        if (cursor + 6 <= raw.Length &&
            BitConverter.ToUInt16(raw, cursor) == 0 &&
            BitConverter.ToUInt32(raw, cursor + 2) == TypeCrc)
            cursor += 2;

        var type = 0;
        var children = new List<int>();
        var width = 0f;
        var height = 0f;
        var px = 0f;
        var py = 0f;
        var havePx = false;
        var havePy = false;
        string? text = null;
        var font = 0;
        var layer = 0;
        var angle = 0f;
        var graphic = 0;
        var haveGraphic = false;
        var sprites = 0;
        var states = 0;
        var colourR = 0f;
        var colourG = 0f;
        var colourB = 0f;
        var colourA = 0f;
        var zoomX = 1f;
        var zoomY = 1f;
        var haveZoomX = false;
        var haveZoomY = false;
        var unread = new List<uint>();
        var unreadOffset = raw.Length;
        var partial = false;

        while (cursor + 4 <= raw.Length)
        {
            var crc = BitConverter.ToUInt32(raw, cursor);
            var payload = cursor + 4;
            if (crc == TypeCrc && payload + 4 <= raw.Length)
            {
                type = BitConverter.ToInt32(raw, payload);
                cursor = payload + 4;
                continue;
            }

            if (crc == ChildrenCrc && payload + 4 <= raw.Length)
            {
                var n = BitConverter.ToInt32(raw, payload);
                children.Clear();
                cursor = payload + 4;
                if (n is >= 0 and <= 256)
                {
                    for (var i = 0; i < n && cursor + 4 <= raw.Length; i++, cursor += 4)
                        children.Add(BitConverter.ToInt32(raw, cursor));
                }
                else
                {
                    unread.Add(crc);
                    partial = true;
                    unreadOffset = payload - 4;
                    break;
                }

                continue;
            }

            if (crc == TextTagCrc)
            {
                var t = payload;
                text = ReadUtf16(raw, ref t);
                cursor = t;
                continue;
            }

            if (crc == FontCrc && payload + 4 <= raw.Length)
            {
                font = BitConverter.ToInt32(raw, payload);
                cursor = payload + 4;
                continue;
            }

            if (crc == HeightCrc && payload + 4 <= raw.Length)
            {
                var value = BitConverter.ToSingle(raw, payload);
                if (float.IsFinite(value))
                    height = value;
                cursor = payload + 4;
                continue;
            }

            if (crc == WidthCrc && payload + 4 <= raw.Length)
            {
                var value = BitConverter.ToSingle(raw, payload);
                if (float.IsFinite(value))
                    width = value;
                cursor = payload + 4;
                continue;
            }

            if (crc == PositionXCrc && payload + 4 <= raw.Length)
            {
                var value = BitConverter.ToSingle(raw, payload);
                if (float.IsFinite(value) && !havePx)
                {
                    px = value;
                    havePx = true;
                }

                cursor = payload + 4;
                continue;
            }

            if (crc == PositionYCrc && payload + 4 <= raw.Length)
            {
                var value = BitConverter.ToSingle(raw, payload);
                if (float.IsFinite(value) && !havePy)
                {
                    py = value;
                    havePy = true;
                }

                cursor = payload + 4;
                continue;
            }

            if (crc == LayerCrc && payload + 4 <= raw.Length)
            {
                layer = BitConverter.ToInt32(raw, payload);
                cursor = payload + 4;
                continue;
            }

            if (crc == AngleCrc && payload + 4 <= raw.Length)
            {
                var value = BitConverter.ToSingle(raw, payload);
                if (float.IsFinite(value))
                    angle = value;
                cursor = payload + 4;
                continue;
            }

            if (crc == GraphicIndexCrc && payload + 4 <= raw.Length)
            {
                var id = BitConverter.ToInt32(raw, payload);
                if (!haveGraphic)
                {
                    graphic = id;
                    haveGraphic = true;
                }

                cursor = payload + 4;
                continue;
            }

            if (crc == Unknown0961Crc && payload + 4 <= raw.Length)
            {
                cursor = payload + 4;
                continue;
            }

            if (crc == Unknown38BBCrc && payload + 4 <= raw.Length)
            {
                cursor = payload + 4;
                continue;
            }

            if (crc == SpritesCrc && payload + 4 <= raw.Length)
            {
                sprites = BitConverter.ToInt32(raw, payload);
                cursor = payload + 4;
                if (sprites != 0)
                {
                    unread.Add(crc);
                    partial = true;
                    unreadOffset = cursor;
                    break;
                }

                continue;
            }

            if (crc == Unknown6B10Crc && payload + 4 <= raw.Length)
            {
                cursor = payload + 4;
                continue;
            }

            if (crc == UnknownF81FCrc && payload + 4 <= raw.Length)
            {
                cursor = payload + 4;
                continue;
            }

            if (crc == StatesCrc && payload + 4 <= raw.Length)
            {
                states = BitConverter.ToInt32(raw, payload);
                cursor = payload + 4;
                continue;
            }

            if (crc == ZoomXCrc && payload + 4 <= raw.Length)
            {
                var value = BitConverter.ToSingle(raw, payload);
                if (float.IsFinite(value) && !haveZoomX)
                {
                    zoomX = value;
                    haveZoomX = true;
                }

                cursor = payload + 4;
                continue;
            }

            if (crc == ZoomYCrc && payload + 4 <= raw.Length)
            {
                var value = BitConverter.ToSingle(raw, payload);
                if (float.IsFinite(value) && !haveZoomY)
                {
                    zoomY = value;
                    haveZoomY = true;
                }

                cursor = payload + 4;
                continue;
            }

            if (crc == ColourRCrc && payload + 4 <= raw.Length)
            {
                var value = BitConverter.ToSingle(raw, payload);
                if (float.IsFinite(value))
                    colourR = value;
                cursor = payload + 4;
                continue;
            }

            if (crc == ColourGCrc && payload + 4 <= raw.Length)
            {
                var value = BitConverter.ToSingle(raw, payload);
                if (float.IsFinite(value))
                    colourG = value;
                cursor = payload + 4;
                continue;
            }

            if (crc == ColourBCrc && payload + 4 <= raw.Length)
            {
                var value = BitConverter.ToSingle(raw, payload);
                if (float.IsFinite(value))
                    colourB = value;
                cursor = payload + 4;
                continue;
            }

            if (crc == ColourACrc && payload + 4 <= raw.Length)
            {
                var value = BitConverter.ToSingle(raw, payload);
                if (float.IsFinite(value))
                    colourA = value;
                cursor = payload + 4;
                continue;
            }

            if (crc == UnknownF97DCrc && payload + 4 <= raw.Length)
            {
                cursor = payload + 4;
                continue;
            }

            if (crc == UnknownA5F8Crc && payload + 4 <= raw.Length)
            {
                cursor = payload + 4;
                continue;
            }

            if (crc == UnreadNestedCrc && payload < raw.Length)
            {
                cursor = payload + 1;
                continue;
            }

            if (crc == StylePlus64Crc && payload + 4 <= raw.Length)
            {
                cursor = payload + 4;
                continue;
            }

            if (crc == StylePlus108Crc && payload + 4 <= raw.Length)
            {
                var n = BitConverter.ToInt32(raw, payload);
                cursor = payload + 4;
                if (n is >= 0 and <= 256)
                    cursor += n * 4;
                continue;
            }

            if (crc is CentreCrc or AbsoluteCrc or ScaleSizeCrc or ScaleOriginCrc
                && payload < raw.Length)
            {
                cursor = payload + 1;
                continue;
            }

            unread.Add(crc);
            unreadOffset = cursor;
            partial = true;
            break;
        }

        var centreByte = ReadPersistU8(raw, CentreCrc);
        var absoluteByte = ReadPersistU8(raw, AbsoluteCrc);
        var scaleSizeByte = ReadPersistU8(raw, ScaleSizeCrc);
        var scaleOriginByte = ReadPersistU8(raw, ScaleOriginCrc);

        return new FrontendUiDef
        {
            InstanceName = entry.InstanceName ?? entry.SourceName ?? "UI",
            Type = type,
            ChildIndices = children,
            Width = width,
            Height = height,
            PositionX = px,
            PositionY = py,
            TextTag = text,
            Font = font,
            Layer = layer,
            Angle = angle,
            GraphicId = graphic,
            GraphicBankId = graphic,
            Sprites = sprites,
            States = states,
            ColourR = colourR,
            ColourG = colourG,
            ColourB = colourB,
            ColourA = colourA,
            ZoomX = zoomX,
            ZoomY = zoomY,
            Center = centreByte != 0,
            Absolute = absoluteByte != 0,
            ScaleSizeToViewport = scaleSizeByte != 0,
            ScaleOriginToViewport = scaleOriginByte != 0,
            ScaleSizeByte = scaleSizeByte,
            ScaleOriginByte = scaleOriginByte,
            UnreadCrcs = unread,
            UnreadOffset = unreadOffset,
            Partial = partial,
        };
    }

    /// <summary>
    /// <c>0043314A</c> file form: CRC then one
    /// byte. <c>00403EB0</c> <c>setne</c> so any
    /// nonzero is 1. Each Press Start flag CRC
    /// occurs once per widget.
    /// </summary>
    public static byte ReadPersistU8(byte[] raw, uint crc)
    {
        for (var i = 0; i + 5 <= raw.Length; i++)
        {
            if (BitConverter.ToUInt32(raw, i) == crc)
                return raw[i + 4];
        }

        return 0;
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
        var text = Encoding.Unicode.GetString(raw, start, bytes);
        var nul = text.IndexOf('\0');
        return nul >= 0 ? text[..nul] : text;
    }
}
