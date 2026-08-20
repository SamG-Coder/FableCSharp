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
    /// UTF-16 literal or localised-text symbol.
    /// </summary>
    public const uint TextValueCrc = 0xE215EF13;
    public const uint FontCrc = 0x51E278F0;
    public const uint LayerCrc = 0xE338F903;
    public const uint AngleCrc = 0x07629D10;
    public const uint MeshIndexCrc = 0x0961B216;
    public const uint ExpansionTypeCrc = 0x38BB7ED4;
    public const uint SpritesCrc = 0x5E5D8A25;
    /// <summary>
    /// CUIDef persist <c>+326</c> <c>00431061</c>
    /// (<c>00631DE1</c>).
    /// Type-12 New Profile list stores 30.
    /// </summary>
    public const uint PositionOffsetYCrc = 0xD7495328;
    /// <summary>
    /// CUIDef persist <c>+322</c> <c>00431061</c>
    /// (<c>00631DD3</c>). Type-8 ctor
    /// <c>0053822B</c> copies it to
    /// widget <c>+392</c>. New Profile
    /// list stores 0.
    /// </summary>
    public const uint PositionOffsetXCrc = 0xA04E63BE;
    /// <summary>
    /// CUIDef persist <c>+96</c> i32
    /// (<c>00631CCD</c> / <c>00632340</c>).
    /// Bit 0 places type-2 cells on X
    /// (<c>00551EA0</c>).
    /// </summary>
    public const uint HorizontalSeparationsCrc = 0x6B1015E4;
    public const uint VerticalSeparationsCrc = 0xF81F10A8;
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
    public const uint ColourRCrc = 0x79902E65;
    public const uint ColourGCrc = 0x144DCA8E;
    public const uint ColourBCrc = 0x64273E01;
    public const uint ColourACrc = 0xFD2E6FBB;
    /// <summary>
    /// Style transition duration at def style <c>+104</c>. Copied by
    /// <c>0052F440</c> to runtime style <c>+28</c> and selected by
    /// <c>0052CF40</c> when it is non-negative.
    /// </summary>
    public const uint StyleDurationCrc = 0xF97D3844;
    public const uint StateChangeTypeCrc = 0xA5F8D969;
    /// <summary>
    /// Style <c>+120</c> u8. CUIDef persist
    /// <c>00631C60</c> after the style
    /// vector. Not a nested object.
    /// </summary>
    public const uint LinearChangeCrc = 0x56A59976;
    /// <summary>
    /// CUIDef persist <c>00631C60</c>
    /// <c>+189</c> u8 <c>0043314A</c>.
    /// </summary>
    public const uint TextLineBreakCrc = 0xBDACBABA;
    /// <summary>
    /// CUIDef persist <c>00631C60</c>
    /// <c>+190</c> u8 <c>0043314A</c>.
    /// </summary>
    public const uint ScaleTextCrc = 0xAC637D43;
    /// <summary>
    /// CUIDef persist writer
    /// <c>00631C60</c>.
    /// </summary>
    public const uint PersistFn = 0x00631C60;
    /// <summary>
    /// CUIDef persist <c>+160</c>
    /// <c>00632420</c>. Sequential stop
    /// before the flag tail. File CRC
    /// from inflated UI.
    /// </summary>
    public const uint MeshTypeCrc = 0x424AD096;
    /// <summary>
    /// CUIDef persist <c>+164</c> f32
    /// after <see cref="MeshTypeCrc"/>.
    /// </summary>
    public const uint TextWindowTLXCrc = 0xECB91A6A;
    /// <summary>
    /// CUIDef persist <c>+192</c> f32
    /// after PositionIsCenter.
    /// </summary>
    public const uint ScrollingSpeedCrc = 0x9B9B2628;
    /// <summary>
    /// CUIDef persist <c>+392</c> u8
    /// <c>0043314A</c> at <c>00632065</c>.
    /// <c>00533288</c> <c>or [+302],1</c>
    /// → <c>vtbl+420</c> <c>0052F1D0</c>.
    /// </summary>
    public const uint DrawFromViewportCrc = 0x8A69D67E;
    /// <summary>
    /// CUIDef persist <c>+476</c> u8
    /// <c>00632137</c>.
    /// </summary>
    public const uint LayerIndependantCrc = 0xD5B65965;
    /// <summary>
    /// CUIDef persist <c>+504</c> u8
    /// <c>00632161</c>. <c>0053324C</c>
    /// <c>or [+300],0x80</c> →
    /// <c>vtbl+400</c> <c>0052F180</c>.
    /// </summary>
    public const uint BastardChildCrc = 0x2CB06C8E;
    /// <summary>
    /// CUIDef persist <c>+508</c> i32
    /// <c>006325E0</c>. Type-6
    /// <c>0054ED90</c> 0/1/2 →
    /// <c>+302</c> <c>0x08/0x10/0x20</c>.
    /// </summary>
    public const uint AlignementCrc = 0x02F094DB;
    /// <summary>
    /// CUIDef persist <c>+512</c> u8
    /// <c>0063217D</c>.
    /// </summary>
    public const uint RandomSwapCrc = 0x7084E2DD;
    /// <summary>
    /// Type-18 state keys persisted at def <c>+480</c> and copied by
    /// <c>00547500</c> into the widget <c>+348</c> key/duration list.
    /// </summary>
    public const uint SwappingStatesCrc = 0xDB6D4753;
    /// <summary>
    /// Type-18 dwell seconds persisted at def <c>+492</c>. Press Start's
    /// forest, sunbeam, and prompt swaps author zero for every state.
    /// </summary>
    public const uint SwappingTimesCrc = 0x68CAB92E;
    /// <summary>
    /// Style <c>+64</c> after
    /// <see cref="LinearChangeCrc"/>.
    /// </summary>
    public const uint StateChangeFlagCrc = 0xF8D265DA;
    /// <summary>
    /// Style <c>+108</c> i32 vector after
    /// <see cref="StateChangeFlagCrc"/>.
    /// </summary>
    public const uint ChildrenNotAffectedCrc = 0x2085F2AB;
    /// <summary>
    /// <c>005331A0</c> def <c>+188</c> →
    /// widget <c>+302</c> bit 1. Persist u8.
    /// </summary>
    public const uint PositionIsCenterCrc = 0x64D3430E;
    /// <summary>
    /// <c>005331A0</c> def <c>+191</c> →
    /// widget <c>+300</c> bit 6. Persist u8.
    /// </summary>
    public const uint IndependantCrc = 0x38BBD87F;
    /// <summary>
    /// <c>005331A0</c> def <c>+520</c> →
    /// widget <c>+302</c> bit 6 remap size.
    /// Persist u8 <c>0043314A</c>.
    /// </summary>
    public const uint UseRelativeZoomCrc = 0xC50CA371;
    /// <summary>
    /// <c>005331A0</c> def <c>+521</c> →
    /// widget <c>+302</c> bit 7 remap origin.
    /// Persist u8 <c>0043314A</c>.
    /// </summary>
    public const uint UseRelativePositionCrc = 0xB466D948;
    /// <summary>
    /// <c>FableCrc("GraphicIndex")</c>. Persist i32 is
    /// <c>GBANK_FRONT_END_PC</c> <c>BankEntry.Id</c>.
    /// </summary>
    public const uint GraphicIndexCrc = 0x38E36902;
    /// <summary>
    /// <c>00631C60</c> <c>00632500</c>
    /// dest <c>+224</c>. First
    /// <c>0055B040</c> copy (vtbl+284).
    /// File CRC <c>0x230364D6</c>.
    /// Not <see cref="ActionOnLeftUnclickedCrc"/>.
    /// </summary>
    public const uint ActionOnLeftClickedCrc = 0x230364D6;
    public const int ActionOnLeftClickedRetailOffset = 224;
    /// <summary>
    /// Persist i32 copied by
    /// <c>0055B040</c> from def
    /// <c>+228</c> then vtbl+320.
    /// Type 38 <c>UI_ACCEPT_NEW_PROFILE</c>
    /// stores <c>0x126</c>; type 11
    /// <c>UI_FRONTEND_BUTTON_NEW_GAME</c>
    /// stores 15.
    /// <c>+224</c> is
    /// <see cref="ActionOnLeftClickedCrc"/>.
    /// </summary>
    public const uint ActionOnLeftUnclickedCrc = 0x53C644E4;
    public const int ActionOnLeftUnclickedRetailOffset = 228;
    /// <summary>
    /// <c>00631C60</c> tail i32 helper
    /// for <c>+196</c>…<c>+256</c>.
    /// Not <see cref="PersistDwordFn"/>.
    /// </summary>
    public const uint PersistTailDwordFn = 0x00632500;
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
    public string? TextValue { get; init; }
    public int Font { get; init; }
    public int Layer { get; init; }
    public float Angle { get; init; }
    public int GraphicId { get; init; }
    public int GraphicBankId { get; init; }
    public int Sprites { get; init; }
    /// <summary>
    /// <c>006327A0</c> / <c>006327E0</c>:
    /// count then <c>n</c> <c>(key, defIndex)</c>
    /// pairs. <c>00551340</c> constructs
    /// <c>defIndex</c> via <c>0041E5F2</c>.
    /// </summary>
    public IReadOnlyList<int> SpriteDefIndices { get; init; } = [];
    /// <summary>
    /// Persist Sprites pair key.
    /// New Profile LEFT is
    /// <c>(0,L) (1,R) (4,M)</c>:
    /// 0/1 caps, 4 stretch.
    /// </summary>
    public IReadOnlyList<int> SpriteKeys { get; init; } = [];
    /// <summary>
    /// Persist <see cref="ExpansionTypeCrc"/> /
    /// def <c>+96</c>. Bit 0 = place
    /// type-2 cells along X.
    /// </summary>
    public int ExpansionType { get; init; }
    /// <summary>
    /// Persist <see cref="PositionOffsetYCrc"/> /
    /// def <c>+326</c>.
    /// </summary>
    public float PositionOffsetY { get; init; }
    /// <summary>
    /// Persist <see cref="PositionOffsetXCrc"/> /
    /// def <c>+322</c>. Type-8/12 item
    /// X spacing at widget <c>+392</c>.
    /// </summary>
    public float PositionOffsetX { get; init; }
    public int States { get; init; }
    public float ColourR { get; init; }
    public float ColourG { get; init; }
    public float ColourB { get; init; }
    public float ColourA { get; init; }
    /// <summary>
    /// Persist <see cref="ColourACrc"/> was
    /// present. An absent colour stays at the ctor
    /// <c>005339B0</c> <c>+144..+147=0xFF</c>.
    /// Explicit <c>ColourA=0</c> is
    /// <c>0041AFA0</c> <c>+151</c> skip.
    /// </summary>
    public bool HaveColourA { get; init; }
    /// <summary>
    /// One ColourRGBA per persist
    /// <see cref="States"/> record.
    /// <c>0052C7E0</c> indexes with
    /// widget <c>+328</c>.
    /// </summary>
    public IReadOnlyList<float> StyleColourR { get; init; } = [];
    public IReadOnlyList<float> StyleColourG { get; init; } = [];
    public IReadOnlyList<float> StyleColourB { get; init; } = [];
    public IReadOnlyList<float> StyleColourA { get; init; } = [];
    public IReadOnlyList<float> StyleDurations { get; init; } = [];
    /// <summary>
    /// Persist style <c>+64</c>
    /// <see cref="StateChangeFlagCrc"/>. Map
    /// dword0. Tick <c>0052C7E0</c>
    /// <c>0x10/0x20/0x40</c>.
    /// </summary>
    public IReadOnlyList<int> StyleFlags { get; init; } = [];
    public IReadOnlyList<int> SwappingStates { get; init; } = [];
    public IReadOnlyList<float> SwappingTimes { get; init; } = [];
    /// <summary>
    /// Persist <see cref="DrawFromViewportCrc"/> /
    /// def <c>+392</c>. Nonzero → widget
    /// <c>+302</c> bit 0.
    /// </summary>
    public byte DrawFromViewport { get; init; }
    /// <summary>
    /// Persist <see cref="BastardChildCrc"/> /
    /// def <c>+504</c>. Nonzero → widget
    /// <c>+300</c> bit 7.
    /// </summary>
    public byte BastardChild { get; init; }
    /// <summary>
    /// Persist <see cref="AlignementCrc"/> /
    /// def <c>+508</c>. Type-6 align.
    /// </summary>
    public int Alignement { get; init; }
    public float ZoomX { get; init; } = 1f;
    public float ZoomY { get; init; } = 1f;
    /// <summary>
    /// <c>005331A0</c> <c>+302</c> bit 1 from
    /// def <c>+188</c> / <see cref="PositionIsCenterCrc"/>.
    /// </summary>
    public bool PositionIsCenter { get; init; }
    /// <summary>
    /// <c>005331A0</c> <c>+300</c> bit 6 from
    /// def <c>+191</c> / <see cref="IndependantCrc"/>.
    /// </summary>
    public bool Independant { get; init; }
    /// <summary>
    /// <c>005331A0</c> <c>+302</c> bit 7 from
    /// def <c>+521</c> / <see cref="UseRelativePositionCrc"/>.
    /// </summary>
    public bool UseRelativePosition { get; init; }
    /// <summary>
    /// <c>005331A0</c> <c>+302</c> bit 6 from
    /// def <c>+520</c> / <see cref="UseRelativeZoomCrc"/>.
    /// </summary>
    public bool UseRelativeZoom { get; init; }
    /// <summary>
    /// Raw persist u8 at def <c>+520</c>.
    /// </summary>
    public byte UseRelativeZoomByte { get; init; }
    /// <summary>
    /// Raw persist u8 at def <c>+521</c>.
    /// </summary>
    public byte UseRelativePositionByte { get; init; }
    /// <summary>
    /// Persist <see cref="ActionOnLeftUnclickedCrc"/>
    /// → def <c>+228</c>.
    /// </summary>
    public int ActionOnLeftUnclicked { get; init; }
    /// <summary>
    /// Persist <see cref="ActionOnLeftClickedCrc"/>
    /// → def <c>+224</c>.
    /// </summary>
    public int ActionOnLeftClicked { get; init; }
    /// <summary>
    /// True when <see cref="FrontendUiSchema"/> validated every serialized
    /// field boundary and consumed the inflated entry through exact EOF.
    /// </summary>
    public bool SchemaComplete { get; init; }
    public string? SchemaError { get; init; }

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
        var spriteDefs = new List<int>();
        var spriteKeys = new List<int>();
        var expansionType = 0;
        var positionOffsetY = 0f;
        var positionOffsetX = 0f;
        var states = 0;
        var colourR = 0f;
        var colourG = 0f;
        var colourB = 0f;
        var colourA = 0f;
        var haveColourA = false;
        var styleColourR = new List<float>();
        var styleColourG = new List<float>();
        var styleColourB = new List<float>();
        var styleColourA = new List<float>();
        var styleDurations = new List<float>();
        var styleFlags = new List<int>();
        var drawFromViewport = (byte)0;
        var bastardChild = (byte)0;
        var alignement = 0;
        var zoomX = 1f;
        var zoomY = 1f;
        var haveZoomX = false;
        var haveZoomY = false;

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
                    break;
                }

                continue;
            }

            if (crc == TextValueCrc)
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
                if (!haveGraphic || (graphic == 0 && id != 0))
                {
                    graphic = id;
                    haveGraphic = true;
                }

                cursor = payload + 4;
                continue;
            }

            if (crc == MeshIndexCrc && payload + 4 <= raw.Length)
            {
                cursor = payload + 4;
                continue;
            }

            if (crc == ExpansionTypeCrc && payload + 4 <= raw.Length)
            {
                expansionType = BitConverter.ToInt32(raw, payload);
                cursor = payload + 4;
                continue;
            }

            if (crc == SpritesCrc && payload + 4 <= raw.Length)
            {
                sprites = BitConverter.ToInt32(raw, payload);
                cursor = payload + 4;
                spriteDefs.Clear();
                spriteKeys.Clear();
                if (sprites is > 0 and <= 64)
                {
                    for (var i = 0; i < sprites && cursor + 8 <= raw.Length; i++)
                    {
                        spriteKeys.Add(BitConverter.ToInt32(raw, cursor));
                        cursor += 4;
                        spriteDefs.Add(BitConverter.ToInt32(raw, cursor));
                        cursor += 4;
                    }

                    for (var a = 0; a < spriteKeys.Count; a++)
                    {
                        var best = a;
                        for (var b = a + 1; b < spriteKeys.Count; b++)
                        {
                            if (spriteKeys[b] < spriteKeys[best])
                                best = b;
                        }

                        if (best == a)
                            continue;
                        (spriteKeys[a], spriteKeys[best]) = (spriteKeys[best], spriteKeys[a]);
                        (spriteDefs[a], spriteDefs[best]) = (spriteDefs[best], spriteDefs[a]);
                    }
                }
                else if (sprites != 0)
                {
                    break;
                }

                continue;
            }

            if (crc == PositionOffsetYCrc && payload + 4 <= raw.Length)
            {
                var value = BitConverter.ToSingle(raw, payload);
                if (float.IsFinite(value))
                    positionOffsetY = value;
                cursor = payload + 4;
                continue;
            }

            if (crc == HorizontalSeparationsCrc && payload + 4 <= raw.Length)
            {
                cursor = payload + 4;
                continue;
            }

            if (crc == VerticalSeparationsCrc && payload + 4 <= raw.Length)
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
                haveColourA = true;
                if (float.IsFinite(value))
                    colourA = value;
                styleColourR.Add(colourR);
                styleColourG.Add(colourG);
                styleColourB.Add(colourB);
                styleColourA.Add(colourA);
                cursor = payload + 4;
                continue;
            }

            if (crc == StyleDurationCrc && payload + 4 <= raw.Length)
            {
                var value = BitConverter.ToSingle(raw, payload);
                styleDurations.Add(float.IsFinite(value) ? value : -1f);
                cursor = payload + 4;
                continue;
            }

            if (crc == StateChangeTypeCrc && payload + 4 <= raw.Length)
            {
                cursor = payload + 4;
                continue;
            }

            if (crc == LinearChangeCrc && payload < raw.Length)
            {
                cursor = payload + 1;
                continue;
            }

            if (crc is TextLineBreakCrc or ScaleTextCrc && payload < raw.Length)
            {
                cursor = payload + 1;
                continue;
            }

            if (crc is ActionOnLeftClickedCrc or ActionOnLeftUnclickedCrc && payload + 4 <= raw.Length)
            {
                cursor = payload + 4;
                continue;
            }

            if (crc == StateChangeFlagCrc && payload + 4 <= raw.Length)
            {
                styleFlags.Add(BitConverter.ToInt32(raw, payload));
                cursor = payload + 4;
                continue;
            }

            if (crc == ChildrenNotAffectedCrc && payload + 4 <= raw.Length)
            {
                var n = BitConverter.ToInt32(raw, payload);
                cursor = payload + 4;
                if (n is >= 0 and <= 256)
                    cursor += n * 4;
                continue;
            }

            if (crc is PositionIsCenterCrc or IndependantCrc or UseRelativeZoomCrc or UseRelativePositionCrc
                && payload < raw.Length)
            {
                cursor = payload + 1;
                continue;
            }

            if (crc is MeshTypeCrc or TextWindowTLXCrc or ScrollingSpeedCrc or AlignementCrc
                && payload + 4 <= raw.Length)
            {
                if (crc == AlignementCrc)
                    alignement = BitConverter.ToInt32(raw, payload);
                cursor = payload + 4;
                continue;
            }

            if (crc is DrawFromViewportCrc or LayerIndependantCrc or BastardChildCrc or RandomSwapCrc
                && payload < raw.Length)
            {
                if (crc == DrawFromViewportCrc)
                    drawFromViewport = raw[payload];
                else if (crc == BastardChildCrc)
                    bastardChild = raw[payload];
                cursor = payload + 1;
                continue;
            }

            break;
        }

        var centreByte = ReadPersistU8(raw, PositionIsCenterCrc);
        var independentByte = ReadPersistU8(raw, IndependantCrc);
        var relativeZoomByte = ReadPersistU8(raw, UseRelativeZoomCrc);
        var relativePositionByte = ReadPersistU8(raw, UseRelativePositionCrc);
        var actionOnLeftUnclicked = ReadPersistI32(raw, ActionOnLeftUnclickedCrc);
        var actionOnLeftClicked = ReadPersistI32(raw, ActionOnLeftClickedCrc);
        var swappingStates = ReadPersistI32Vector(raw, SwappingStatesCrc);
        var swappingTimes = ReadPersistF32Vector(raw, SwappingTimesCrc);
        var scanned326 = ReadPersistF32(raw, PositionOffsetYCrc);
        if (float.IsFinite(scanned326))
            positionOffsetY = scanned326;
        var scanned322 = ReadPersistF32(raw, PositionOffsetXCrc);
        if (float.IsFinite(scanned322))
            positionOffsetX = scanned322;
        drawFromViewport = ReadPersistU8(raw, DrawFromViewportCrc);
        bastardChild = ReadPersistU8(raw, BastardChildCrc);
        alignement = ReadPersistI32(raw, AlignementCrc);
        if (graphic == 0)
        {
            for (var i = 0; i + 8 <= raw.Length; i++)
            {
                if (BitConverter.ToUInt32(raw, i) != GraphicIndexCrc)
                    continue;
                var id = BitConverter.ToInt32(raw, i + 4);
                if (id != 0)
                {
                    graphic = id;
                    break;
                }
            }
        }

        var schemaComplete = FrontendUiSchema.TryConsume(entry, out var schemaEnd, out var schemaError);
        _ = schemaEnd;

        return new FrontendUiDef
        {
            InstanceName = entry.InstanceName ?? entry.SourceName ?? "UI",
            Type = type,
            ChildIndices = children,
            Width = width,
            Height = height,
            PositionX = px,
            PositionY = py,
            TextValue = text,
            Font = font,
            Layer = layer,
            Angle = angle,
            GraphicId = graphic,
            GraphicBankId = graphic,
            Sprites = sprites,
            SpriteDefIndices = spriteDefs,
            SpriteKeys = spriteKeys,
            ExpansionType = expansionType,
            PositionOffsetY = positionOffsetY,
            PositionOffsetX = positionOffsetX,
            States = states,
            ColourR = colourR,
            ColourG = colourG,
            ColourB = colourB,
            ColourA = colourA,
            HaveColourA = haveColourA,
            StyleColourR = styleColourR,
            StyleColourG = styleColourG,
            StyleColourB = styleColourB,
            StyleColourA = styleColourA,
            StyleDurations = styleDurations,
            StyleFlags = styleFlags,
            SwappingStates = swappingStates,
            SwappingTimes = swappingTimes,
            DrawFromViewport = drawFromViewport,
            BastardChild = bastardChild,
            Alignement = alignement,
            ZoomX = zoomX,
            ZoomY = zoomY,
            PositionIsCenter = centreByte != 0,
            Independant = independentByte != 0,
            UseRelativeZoom = relativeZoomByte != 0,
            UseRelativePosition = relativePositionByte != 0,
            UseRelativeZoomByte = relativeZoomByte,
            UseRelativePositionByte = relativePositionByte,
            ActionOnLeftUnclicked = actionOnLeftUnclicked,
            ActionOnLeftClicked = actionOnLeftClicked,
            SchemaComplete = schemaComplete,
            SchemaError = schemaError,
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

    public static IReadOnlyList<int> ReadPersistI32Vector(byte[] raw, uint crc)
    {
        var offset = FindPersistField(raw, crc);
        if (offset < 0)
            return [];
        var count = BitConverter.ToInt32(raw, offset + 4);
        if (count is < 0 or > 256 || offset + 8 + count * 4 > raw.Length)
            return [];
        var values = new int[count];
        for (var i = 0; i < count; i++)
            values[i] = BitConverter.ToInt32(raw, offset + 8 + i * 4);
        return values;
    }

    public static IReadOnlyList<float> ReadPersistF32Vector(byte[] raw, uint crc)
    {
        var offset = FindPersistField(raw, crc);
        if (offset < 0)
            return [];
        var count = BitConverter.ToInt32(raw, offset + 4);
        if (count is < 0 or > 256 || offset + 8 + count * 4 > raw.Length)
            return [];
        var values = new float[count];
        for (var i = 0; i < count; i++)
        {
            var value = BitConverter.ToSingle(raw, offset + 8 + i * 4);
            values[i] = float.IsFinite(value) ? value : 0f;
        }
        return values;
    }

    private static int FindPersistField(byte[] raw, uint crc)
    {
        for (var i = 0; i + 8 <= raw.Length; i++)
            if (BitConverter.ToUInt32(raw, i) == crc)
                return i;
        return -1;
    }

    /// <summary>
    /// File form: CRC then i32.
    /// Tail slots <c>+224</c>/<c>+228</c>
    /// use <see cref="PersistTailDwordFn"/>.
    /// </summary>
    public static int ReadPersistI32(byte[] raw, uint crc)
    {
        for (var i = 0; i + 8 <= raw.Length; i++)
        {
            if (BitConverter.ToUInt32(raw, i) == crc)
                return BitConverter.ToInt32(raw, i + 4);
        }

        return 0;
    }

    /// <summary>
    /// File form: CRC then f32.
    /// </summary>
    public static float ReadPersistF32(byte[] raw, uint crc)
    {
        for (var i = 0; i + 8 <= raw.Length; i++)
        {
            if (BitConverter.ToUInt32(raw, i) == crc)
                return BitConverter.ToSingle(raw, i + 4);
        }

        return 0f;
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
