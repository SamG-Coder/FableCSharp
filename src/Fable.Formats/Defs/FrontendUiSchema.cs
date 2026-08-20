namespace Fable.Formats.Defs;

/// <summary>
/// Exact serialized <c>CUIDef</c> field walk recovered from native persist
/// function <c>00631C60</c> and its helpers. This class deliberately validates
/// the CRC at every field boundary: reaching EOF means the complete inflated
/// UI payload was classified structurally, rather than located by scanning.
/// </summary>
public static class FrontendUiSchema
{
    public const uint ActionCrc = 0xF1A22807;
    public const uint ActionOnBackCrc = 0x8B645C94;
    public const uint ActionOnSelectedCrc = 0x0E79EEFC;
    public const uint ActionOnUnselectedCrc = 0x12A56842;
    public const uint ActionOnDestructionCrc = 0xCB9ADD65;
    public const uint ActionOnLeftHeldCrc = 0xECEC0A1E;
    public const uint ActionOnRightClickedCrc = 0x15F8091D;
    public const uint ActionOnDroppedCrc = 0x04158DDD;
    public const uint ActionOnDroppedNowhereCrc = 0x79633DD6;
    public const uint PreActionCrc = 0xB40B9CDE;
    public const uint ActionOnDraggedUpCrc = 0xAC7A9A1B;
    public const uint ActionOnDraggedDownCrc = 0x8C476A63;
    public const uint ActionOnLeftClickedAboveCrc = 0xFD94AA5E;
    public const uint ActionOnLeftClickedUnderCrc = 0xCC4B0F10;

    public static bool TryConsume(GameBinEntry entry, out int end, out string? error)
    {
        end = entry.BodyOffset > 0 ? entry.BodyOffset : FrontendUiDef.HeaderBytes;
        error = null;
        if (entry.TypeName != "UI")
        {
            error = "entry is not UI";
            return false;
        }

        var reader = new Reader(entry.Raw, end);
        if (reader.Remaining >= 6 && reader.PeekU16() == 0 &&
            reader.PeekU32(2) == FrontendUiDef.TypeCrc)
            reader.Skip(2);

        reader.I32(FrontendUiDef.TypeCrc, "Type");
        reader.I32Vector(FrontendUiDef.ChildrenCrc, "Children");
        reader.I32(FrontendUiDef.MeshIndexCrc, "MeshIndex");
        reader.Utf16(FrontendUiDef.TextValueCrc, "TextValue");
        reader.I32(FrontendUiDef.FontCrc, "Font");
        reader.F32(FrontendUiDef.HeightCrc, "Height");
        reader.F32(FrontendUiDef.WidthCrc, "Width");
        reader.I32(FrontendUiDef.ExpansionTypeCrc, "ExpansionType");
        reader.I32PairMap(FrontendUiDef.SpritesCrc, "Sprites");
        reader.I32Vector(FrontendUiDef.HorizontalSeparationsCrc, "HorizontalSeparations");
        reader.I32Vector(FrontendUiDef.VerticalSeparationsCrc, "VerticalSeparations");
        var states = reader.I32(FrontendUiDef.StatesCrc, "States");
        if (states is < 0 or > 256)
            reader.Fail($"States count {states} is invalid");
        for (var i = 0; i < states && reader.Ok; i++)
            ConsumeStyle(ref reader, i);

        reader.U8(FrontendUiDef.TextLineBreakCrc, "TextLineBreak");
        reader.U8(FrontendUiDef.ScaleTextCrc, "ScaleText");
        reader.U8(FrontendUiDef.IndependantCrc, "Independant");
        reader.I32(FrontendUiDef.MeshTypeCrc, "MeshType");
        reader.I32Vector(0xDF05CD7A, "NonScrollingChildren");
        reader.F32(FrontendUiDef.TextWindowTLXCrc, "TextWindowTLX");
        reader.F32(0x9BBE2AFC, "TextWindowTLY");
        reader.F32(0x2053FA77, "TextWindowBRX");
        reader.F32(0x5754CAE1, "TextWindowBRY");
        reader.I32(FrontendUiDef.LayerCrc, "Layer");
        reader.F32(FrontendUiDef.AngleCrc, "Angle");
        reader.U8(FrontendUiDef.PositionIsCenterCrc, "PositionIsCenter");
        reader.F32(FrontendUiDef.ScrollingSpeedCrc, "ScrollingSpeed");
        reader.U8(0x1AFFE50C, "Wrapping");
        reader.U8(0xA1696AA7, "Inverted");
        reader.F32(FrontendUiDef.PositionOffsetXCrc, "PositionOffsetX");
        reader.F32(FrontendUiDef.PositionOffsetYCrc, "PositionOffsetY");
        // Stored in a byte at CUIDef+330, but enum transfer 00473593
        // serializes its value as a tagged 32-bit integer.
        reader.I32(0xED15731E, "AlphaOffset");
        reader.F32(0x20F84C6B, "UpX");
        reader.F32(0x57FF7CFD, "UpY");
        reader.F32(0xCEF62D47, "UpZ");
        reader.F32(0x2FDA3C4E, "ForwardX");
        reader.F32(0x58DD0CD8, "ForwardY");
        reader.F32(0xC1D45D62, "ForwardZ");
        reader.F32(0x8399BEC5, "RotationAxisX");
        reader.F32(0xF49E8E53, "RotationAxisY");
        reader.F32(0x6D97DFE9, "RotationAxisZ");
        reader.F32(0x66E062FC, "RotationSpeed");
        reader.I32(0x9246E6F6, "AnimationIndex");
        reader.I32(0x4E5056B4, "DownArrow");
        reader.I32(0xA51BFDF6, "UpArrow");
        reader.I32(0x02BAFBA8, "UpLimit");
        reader.I32(0xE9F150EA, "DownLimit");
        reader.U8(0xD57236EC, "Scrolling");
        reader.U8(0x6448E488, "ComputeOffsetsOnActivate");
        reader.F32(0xB0B6EFA0, "MinX");
        reader.F32(0xC7B1DF36, "MinY");
        reader.F32(0xA23D0BCF, "MaxX");
        reader.F32(0xD53A3B59, "MaxY");
        reader.F32(0x9F7D2B2B, "StepX");
        reader.F32(0xE87A1BBD, "StepY");
        reader.F32(0xFCF7229C, "DimensionsX");
        reader.F32(0x8BF0120A, "DimensionsY");
        reader.I32(0x78132691, "SliderLeft");
        reader.I32(0xA2ABA4E0, "SliderRight");
        reader.I32(ActionCrc, "Action");
        reader.I32(ActionOnBackCrc, "ActionOnBack");
        reader.I32(ActionOnSelectedCrc, "ActionOnSelected");
        reader.I32(ActionOnUnselectedCrc, "ActionOnUnselected");
        reader.I32(ActionOnDestructionCrc, "ActionOnDestruction");
        reader.I32(FrontendUiDef.ActionOnLeftClickedCrc, "ActionOnLeftClicked");
        reader.I32(FrontendUiDef.ActionOnLeftUnclickedCrc, "ActionOnLeftUnclicked");
        reader.I32(ActionOnLeftHeldCrc, "ActionOnLeftHeld");
        reader.I32(ActionOnRightClickedCrc, "ActionOnRightClicked");
        reader.I32(ActionOnDroppedCrc, "ActionOnDropped");
        reader.I32(ActionOnDroppedNowhereCrc, "ActionOnDroppedNowhere");
        reader.I32(PreActionCrc, "PreAction");
        reader.I32(ActionOnDraggedUpCrc, "ActionOnDraggedUp");
        reader.I32(ActionOnDraggedDownCrc, "ActionOnDraggedDown");
        reader.I32(ActionOnLeftClickedAboveCrc, "ActionOnLeftClickedAbove");
        reader.I32(ActionOnLeftClickedUnderCrc, "ActionOnLeftClickedUnder");
        reader.F32(FrontendUiDef.InputDelayCrc, "InputDelay");
        reader.U8(FrontendUiDef.DrawFromViewportCrc, "DrawFromViewport");
        reader.I32(0x108E2C36, "TextBankIndex");
        reader.I32(0x47FFE4E5, "ActionText");
        reader.I32(0x500AEA31, "KeyText");
        reader.I32(0x57FB628C, "Redefiner");
        reader.I32(0xE8F26483, "UndefinedWarning");
        reader.I32CStringMap(0x346B4F53, "ActionMap");
        reader.I32PairMap(0x705B3FE2, "ActionMapAliases");
        reader.I32Vector(0xF23749B2, "ActionOrder");
        reader.U8(FrontendUiDef.EditBoxParentIsButtonCrc, "EditBoxParentIsButton");
        reader.U8(FrontendUiDef.PasswordBoxCrc, "PasswordBox");
        reader.I32(FrontendUiDef.EditBoxCharLimitCrc, "EditBoxCharLimit");
        reader.U8(FrontendUiDef.EditBoxUsesImeCrc, "EditBoxUsesIME");
        reader.Utf16(0x2DA92517, "MovieFilename");
        reader.U8(FrontendUiDef.DisallowSpaceAsFirstCharCrc, "DisallowSpaceAsFirstChar");
        reader.U8(FrontendUiDef.LayerIndependantCrc, "LayerIndependant");
        reader.I32Vector(FrontendUiDef.SwappingStatesCrc, "SwappingStates");
        reader.F32Vector(FrontendUiDef.SwappingTimesCrc, "SwappingTimes");
        reader.U8(FrontendUiDef.BastardChildCrc, "BastardChild");
        reader.I32(FrontendUiDef.AlignementCrc, "Alignement");
        reader.U8(FrontendUiDef.RandomSwapCrc, "RandomSwap");
        reader.U8(FrontendUiDef.UseRelativeZoomCrc, "UseRelativeZoom");
        reader.U8(FrontendUiDef.UseRelativePositionCrc, "UseRelativePosition");
        reader.I32(FrontendUiDef.HoveredStateCrc, "HoveredState");
        reader.I32(FrontendUiDef.LeftClickedStateCrc, "LeftClickedState");
        reader.I32(FrontendUiDef.RightClickedStateCrc, "RightClickedState");
        reader.I32Vector(0xD63A4547, "ShapeChildren");
        reader.I32(0x298F8140, "ViewAreaTLX");
        reader.I32(0x5E88B1D6, "ViewAreaTLY");
        reader.I32(0xE565615D, "ViewAreaBRX");
        reader.I32(0x926251CB, "ViewAreaBRY");
        reader.U8(0xCA2D971D, "UseViewArea");
        reader.U8(0xE59C9B55, "PartOfListTree");
        reader.U8(0x9E47F106, "PCStyle");
        reader.I32(0xF26C87EA, "Sprite2DFlag");

        end = reader.Cursor;
        if (!reader.Ok)
        {
            error = reader.Error;
            return false;
        }

        if (reader.Remaining != 0)
        {
            error = $"{reader.Remaining} unconsumed bytes at {reader.Cursor}";
            return false;
        }

        return true;
    }

    private static void ConsumeStyle(ref Reader reader, int index)
    {
        var prefix = $"Style[{index}]";
        reader.I32(FrontendUiDef.GraphicIndexCrc, prefix + ".GraphicIndex");
        reader.F32(FrontendUiDef.PositionXCrc, prefix + ".PositionX");
        reader.F32(FrontendUiDef.PositionYCrc, prefix + ".PositionY");
        reader.F32(FrontendUiDef.ZoomXCrc, prefix + ".ZoomX");
        reader.F32(FrontendUiDef.ZoomYCrc, prefix + ".ZoomY");
        reader.F32(FrontendUiDef.ColourRCrc, prefix + ".ColourR");
        reader.F32(FrontendUiDef.ColourGCrc, prefix + ".ColourG");
        reader.F32(FrontendUiDef.ColourBCrc, prefix + ".ColourB");
        reader.F32(FrontendUiDef.ColourACrc, prefix + ".ColourA");
        reader.F32(FrontendUiDef.StyleDurationCrc, prefix + ".UpdateTime");
        reader.I32(FrontendUiDef.StateChangeTypeCrc, prefix + ".StateChangeType");
        reader.U8(FrontendUiDef.LinearChangeCrc, prefix + ".LinearChange");
        reader.I32(FrontendUiDef.StateChangeFlagCrc, prefix + ".StateChangeFlag");
        reader.I32Vector(FrontendUiDef.ChildrenNotAffectedCrc, prefix + ".ChildrenNotAffected");
    }

    private ref struct Reader
    {
        private readonly ReadOnlySpan<byte> _raw;
        public int Cursor;
        public string? Error;
        public bool Ok => Error is null;
        public int Remaining => _raw.Length - Cursor;

        public Reader(ReadOnlySpan<byte> raw, int cursor)
        {
            _raw = raw;
            Cursor = cursor;
            Error = null;
        }

        public ushort PeekU16() => Remaining >= 2 ? BitConverter.ToUInt16(_raw[Cursor..]) : (ushort)0;
        public uint PeekU32(int relative = 0) => Remaining >= relative + 4
            ? BitConverter.ToUInt32(_raw[(Cursor + relative)..]) : 0;
        public void Skip(int bytes) => Cursor += bytes;
        public void Fail(string error) => Error ??= $"{error} at {Cursor}";

        public int I32(uint crc, string name)
        {
            if (!Field(crc, 4, name))
                return 0;
            var value = BitConverter.ToInt32(_raw[Cursor..]);
            Cursor += 4;
            return value;
        }

        public void F32(uint crc, string name)
        {
            if (Field(crc, 4, name))
                Cursor += 4;
        }

        public void U8(uint crc, string name)
        {
            if (Field(crc, 1, name))
                Cursor++;
        }

        public void Utf16(uint crc, string name)
        {
            if (!Field(crc, 2, name))
                return;
            while (Remaining >= 2)
            {
                var ch = BitConverter.ToUInt16(_raw[Cursor..]);
                Cursor += 2;
                if (ch == 0)
                    return;
            }
            Fail($"unterminated {name}");
        }

        public void I32Vector(uint crc, string name) => Vector(crc, name, 4);
        public void F32Vector(uint crc, string name) => Vector(crc, name, 4);
        public void I32PairMap(uint crc, string name) => Vector(crc, name, 8);

        public void I32CStringMap(uint crc, string name)
        {
            var count = I32(crc, name + ".Count");
            if (!Count(count, name))
                return;
            for (var i = 0; i < count && Ok; i++)
            {
                if (Remaining < 4)
                {
                    Fail($"truncated {name}[{i}] key");
                    return;
                }
                Cursor += 4;
                while (Remaining >= 1)
                {
                    var ch = _raw[Cursor++];
                    if (ch == 0)
                        break;
                }
            }
        }

        private void Vector(uint crc, string name, int stride)
        {
            var count = I32(crc, name + ".Count");
            if (!Count(count, name))
                return;
            var bytes = (long)count * stride;
            if (bytes > Remaining)
            {
                Fail($"truncated {name}: need {bytes}, have {Remaining}");
                return;
            }
            Cursor += (int)bytes;
        }

        private bool Count(int count, string name)
        {
            if (count is >= 0 and <= 4096)
                return true;
            Fail($"invalid {name} count {count}");
            return false;
        }

        private bool Field(uint expected, int payloadBytes, string name)
        {
            if (!Ok)
                return false;
            if (Remaining < 4 + payloadBytes)
            {
                Fail($"truncated {name}");
                return false;
            }
            var actual = PeekU32();
            if (actual != expected)
            {
                Fail($"{name} expected CRC 0x{expected:X8}, got 0x{actual:X8}");
                return false;
            }
            Cursor += 4;
            return true;
        }
    }
}
