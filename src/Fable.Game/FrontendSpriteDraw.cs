namespace Fable.Game;

/// <summary>
/// Type <c>0x22</c> sprite submit record.
/// Packer <c>0041BEB0</c> (untextured) /
/// <c>0041BF60</c> (widget+380 set).
/// Instance apply <c>00BAD8A0</c>. Handler
/// <c>00BAE2D0</c>. Enqueue
/// <c>009DB700</c> is 60 bytes at display
/// <c>+16020</c>; that call is not inside
/// the packer or <c>00BAD8A0</c>.
/// </summary>
public readonly record struct FrontendSpriteDraw
{
    public const uint CursorDrawFn = 0x0041A980;
    public const uint CursorInputFn = 0x0041E5F2;
    public const uint CursorSubmitFn = 0x009DD210;
    public const uint PackerFn = 0x0041BEB0;
    public const uint TexturedPackerFn = 0x0041BF60;
    public const uint InstanceSubmitFn = 0x00BAD8A0;
    public const uint HandlerSubmitFn = 0x00BAE2D0;
    public const uint HandlerCtorFn = 0x00BAD040;
    public const uint LayerCtorFn = 0x00B4AC10;
    public const uint EnqueueFn = 0x009DB700;
    public const uint EngineVtbl = 0x012A0F3C;
    public const uint HandlerVtbl = 0x012A5664;
    public const uint InstanceVtbl = 0x012A54BC;
    public const int Type = 0x22;
    public const int TypeAlt = 0x23;
    public const int RecordBytes = 0xC0;
    public const int EnqueueBytes = 60;
    public const int SubmitDestOffset = 0x15C;
    public const int WidgetTextureOffset = 380;
    public const int WidgetSprite2DFlagOffset = 372;
    public const int DefaultBlend = 2;
    public const int DefaultSprite2DFlag = 2;
    public const string Shader = "VSHADER_2D_SPRITE";

    /// <summary>
    /// Type-32 <c>0041A980</c> preserves the widget's computed
    /// size, then submits it at the live input position read after
    /// <c>0041E5F2</c>.
    /// </summary>
    public static (float X0, float Y0, float X1, float Y1) CursorDest(
        float widgetX0, float widgetY0, float widgetX1, float widgetY1,
        float pointerX, float pointerY) =>
        (pointerX, pointerY,
            pointerX + widgetX1 - widgetX0,
            pointerY + widgetY1 - widgetY0);

    public const int TypeOffset = 0;
    public const int ArgOffset = 4;
    public const int DestOffset = 12;
    public const int Field28Offset = 28;
    public const int FontOrIndexOffset = 32;
    public const int Field36Offset = 36;
    public const int Field40Offset = 40;
    public const int Field44Offset = 44;
    public const int BlendOffset = 48;
    public const int ColourBOffset = 52;
    public const int ColourGOffset = 53;
    public const int ColourROffset = 54;
    public const int ColourAOffset = 55;
    public const int SizeFromFrameOffset = 56;
    public const int Field60Offset = 60;
    public const int TextureOffset = 64;
    public const int U0Offset = 68;
    public const int V0Offset = 72;
    public const int U1Offset = 76;
    public const int V1Offset = 80;
    public const int WrittenBytes = 84;

    public int RecordType { get; init; }
    public int Arg { get; init; }
    public float DestX0 { get; init; }
    public float DestY0 { get; init; }
    public float DestX1 { get; init; }
    public float DestY1 { get; init; }
    public int FontOrIndex { get; init; }
    public int TextureId { get; init; }
    public float U0 { get; init; }
    public float V0 { get; init; }
    public float U1 { get; init; }
    public float V1 { get; init; }
    public byte ColourB { get; init; }
    public byte ColourG { get; init; }
    public byte ColourR { get; init; }
    public byte ColourA { get; init; }
    public int Blend { get; init; }
    public int Sprite2DFlag { get; init; }
    public byte SizeFromFrame { get; init; }

    /// <summary>
    /// <c>0041BF60</c> when widget+380 is
    /// set. Texture dword at <c>+64</c>.
    /// <c>+32/+36</c> and <c>+56</c> are 0
    /// so <c>00BAD8A0</c> does not grow dest
    /// from info <c>+6/+8</c>.
    /// </summary>
    public static FrontendSpriteDraw PackTextured(
        float destX0, float destY0, float destX1, float destY1,
        int textureId,
        float u0, float v0, float u1, float v1,
        byte colourB, byte colourG, byte colourR, byte colourA,
        int blend = DefaultBlend) =>
        new()
        {
            RecordType = Type,
            DestX0 = destX0,
            DestY0 = destY0,
            DestX1 = destX1,
            DestY1 = destY1,
            TextureId = textureId,
            U0 = u0,
            V0 = v0,
            U1 = u1,
            V1 = v1,
            ColourB = colourB,
            ColourG = colourG,
            ColourR = colourR,
            ColourA = colourA,
            Blend = blend,
            Sprite2DFlag = DefaultSprite2DFlag,
            SizeFromFrame = 0,
        };

    /// <summary>
    /// <c>0041BEB0</c> when widget+380 is 0.
    /// Texture dword at <c>+64</c> stays 0;
    /// <c>+32</c> is the font/index arg.
    /// </summary>
    public static FrontendSpriteDraw PackUntextured(
        float destX0, float destY0, float destX1, float destY1,
        int fontOrIndex,
        float u0, float v0, float u1, float v1,
        byte colourB, byte colourG, byte colourR, byte colourA,
        int blend = DefaultBlend) =>
        new()
        {
            RecordType = Type,
            DestX0 = destX0,
            DestY0 = destY0,
            DestX1 = destX1,
            DestY1 = destY1,
            FontOrIndex = fontOrIndex,
            TextureId = 0,
            U0 = u0,
            V0 = v0,
            U1 = u1,
            V1 = v1,
            ColourB = colourB,
            ColourG = colourG,
            ColourR = colourR,
            ColourA = colourA,
            Blend = blend,
            Sprite2DFlag = DefaultSprite2DFlag,
            SizeFromFrame = 0,
        };

    public byte[] ToRecord()
    {
        var rec = new byte[RecordBytes];
        Write(rec);
        return rec;
    }

    public void Write(Span<byte> rec)
    {
        if (rec.Length < WrittenBytes)
            throw new ArgumentException("type 0x22 packer writes 84 bytes of a 0xC0 record.", nameof(rec));
        BitConverter.TryWriteBytes(rec.Slice(TypeOffset), RecordType);
        BitConverter.TryWriteBytes(rec.Slice(ArgOffset), Arg);
        rec[8] = 0;
        rec[9] = 0;
        rec[10] = 0;
        BitConverter.TryWriteBytes(rec.Slice(DestOffset), DestX0);
        BitConverter.TryWriteBytes(rec.Slice(DestOffset + 4), DestY0);
        BitConverter.TryWriteBytes(rec.Slice(DestOffset + 8), DestX1);
        BitConverter.TryWriteBytes(rec.Slice(DestOffset + 12), DestY1);
        BitConverter.TryWriteBytes(rec.Slice(FontOrIndexOffset), FontOrIndex);
        BitConverter.TryWriteBytes(rec.Slice(BlendOffset), Blend);
        BitConverter.TryWriteBytes(rec.Slice(Field60Offset), Sprite2DFlag);
        rec[ColourBOffset] = ColourB;
        rec[ColourGOffset] = ColourG;
        rec[ColourROffset] = ColourR;
        rec[ColourAOffset] = ColourA;
        rec[SizeFromFrameOffset] = SizeFromFrame;
        BitConverter.TryWriteBytes(rec.Slice(TextureOffset), TextureId);
        BitConverter.TryWriteBytes(rec.Slice(U0Offset), U0);
        BitConverter.TryWriteBytes(rec.Slice(V0Offset), V0);
        BitConverter.TryWriteBytes(rec.Slice(U1Offset), U1);
        BitConverter.TryWriteBytes(rec.Slice(V1Offset), V1);
    }

    public static FrontendSpriteDraw Read(ReadOnlySpan<byte> rec)
    {
        if (rec.Length < WrittenBytes)
            throw new ArgumentException("type 0x22 record is 84 written bytes.", nameof(rec));
        return new FrontendSpriteDraw
        {
            RecordType = BitConverter.ToInt32(rec.Slice(TypeOffset)),
            Arg = BitConverter.ToInt32(rec.Slice(ArgOffset)),
            DestX0 = BitConverter.ToSingle(rec.Slice(DestOffset)),
            DestY0 = BitConverter.ToSingle(rec.Slice(DestOffset + 4)),
            DestX1 = BitConverter.ToSingle(rec.Slice(DestOffset + 8)),
            DestY1 = BitConverter.ToSingle(rec.Slice(DestOffset + 12)),
            FontOrIndex = BitConverter.ToInt32(rec.Slice(FontOrIndexOffset)),
            Blend = BitConverter.ToInt32(rec.Slice(BlendOffset)),
            Sprite2DFlag = BitConverter.ToInt32(rec.Slice(Field60Offset)),
            ColourB = rec[ColourBOffset],
            ColourG = rec[ColourGOffset],
            ColourR = rec[ColourROffset],
            ColourA = rec[ColourAOffset],
            SizeFromFrame = rec[SizeFromFrameOffset],
            TextureId = BitConverter.ToInt32(rec.Slice(TextureOffset)),
            U0 = BitConverter.ToSingle(rec.Slice(U0Offset)),
            V0 = BitConverter.ToSingle(rec.Slice(V0Offset)),
            U1 = BitConverter.ToSingle(rec.Slice(U1Offset)),
            V1 = BitConverter.ToSingle(rec.Slice(V1Offset)),
        };
    }
}
