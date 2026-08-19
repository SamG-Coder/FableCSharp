namespace Fable.Game;

/// <summary>
/// Native dest from <c>005339B0</c> / <c>0052F5C0</c> /
/// <c>0052FFD0</c> / <c>0041AFA0</c>. Inputs are persist
/// fields, parent dest, and viewport. Output is the
/// submit rect those functions leave for
/// <c>0041AFA0</c>. No screen-specific numbers.
/// </summary>
public static class FrontendLayout
{
    public const uint ScaleInitFn = 0x0052C730;
    public const uint ScaleWriteFn = 0x005339B0;
    public const uint DestLayoutFn = 0x00531EC0;
    public const uint DestScaleFn = 0x0052F5C0;
    public const uint DestOriginFn = 0x0052FFD0;
    public const uint SubmitDestFn = 0x0041AFA0;
    /// <summary>
    /// <c>0041AC20</c> leftover
    /// <c>+204/+208</c> from bank
    /// vtbl+84/+88 when
    /// <c>+376</c> GraphicIndex != 0.
    /// </summary>
    public const uint LeftoverFn = 0x0041AC20;
    public const int GraphicIndexOffset = 376;
    public const int BankFrameWVtbl = 84;
    public const int BankFrameHVtbl = 88;
    public const uint ResolutionScaleFn = 0x0052E580;
    public const uint CenterFn = 0x0052F1E0;
    public const uint ViewportFlagWriterFn = 0x004299A8;
    public const uint UiSingletonCtorFn = 0x0041E3F6;
    public const uint GlobalScaleFn = 0x0041CF47;

    public const uint AuthoredWidthVa = 0x01375CD4;
    public const uint AuthoredHeightVa = 0x01375CD8;
    public const uint ViewportFlagVa = 0x013B8768;
    public const uint ViewportWidthVa = 0x013B876C;
    public const uint ViewportHeightVa = 0x013B8770;
    public const uint GameSingletonVa = 0x013B86A0;
    public const uint HalfVa = 0x0122F59C;
    public const uint GlobalWidthFloorVa = 0x01230010;
    public const uint GlobalHeightFloorVa = 0x0123000C;

    public const float AuthoredWidth = 640f;
    public const float AuthoredHeight = 480f;
    public const float Half = 0.5f;
    public const float GlobalWidthFloor = 1024f;
    public const float GlobalHeightFloor = 768f;

    public const int LayoutMapOffset = 36;
    public const int PosXOffset = 52;
    public const int PosYOffset = 56;
    public const int ParentLocalXOffset = 76;
    public const int ParentLocalYOffset = 80;
    public const int PersistScaleXOffset = 92;
    public const int PersistScaleYOffset = 96;
    public const int ParentInnerScaleXOffset = 116;
    public const int ParentInnerScaleYOffset = 120;
    public const int InnerScaleXOffset = 124;
    public const int InnerScaleYOffset = 128;
    public const int DestWOffset = 204;
    public const int DestHOffset = 208;
    public const int ParentPtrOffset = 200;
    public const int OriginXOffset = 248;
    public const int OriginYOffset = 252;
    public const int ParentDestXOffset = 256;
    public const int ParentDestYOffset = 260;
    public const int DestScaleXOffset = 264;
    public const int DestScaleYOffset = 268;
    public const int InheritScaleXOffset = 272;
    public const int InheritScaleYOffset = 276;
    public const int InheritScaleFlagOffset = 280;
    public const int Flag300Offset = 300;
    public const int Flag302Offset = 302;
    public const int SizeWOffset = 360;
    public const int SizeHOffset = 364;

    public const int CenterBit = 0x02;
    public const int ScaleSizeBit = 0x40;
    public const int ScaleOriginBit = 0x80;
    public const int AbsoluteBit300 = 0x40;

    /// <summary>
    /// <c>0041E3F6</c> <c>mov cl,1</c> then
    /// <c>004299A8</c> writes <c>[0x13B8768]=1</c>
    /// and copies the display into
    /// <c>0x13B876C/70</c>. <c>[0x13B86A0]</c>
    /// is still 0 on the frontend.
    /// </summary>
    public static FrontendViewport FirstSeenFrontend(float displayWidth, float displayHeight) =>
        new(true, displayWidth, displayHeight, GamePresent: false);

    /// <summary>
    /// <c>005339B0</c>: when <c>+280==0</c>
    /// write <c>+272/+276=1.0</c>. Else leave
    /// the incoming inherit scale.
    /// </summary>
    public static (float ScaleX, float ScaleY) InitInheritedScale(
        int flag280, float currentX = 0f, float currentY = 0f) =>
        flag280 == 0 ? (1f, 1f) : (currentX, currentY);

    /// <summary>
    /// <c>0052E580</c>. Identity unless
    /// <c>[0x13B8768]</c> is set.
    /// </summary>
    public static (float X, float Y) ApplyResolutionScale(
        float x, float y, FrontendViewport viewport)
    {
        if (!viewport.ResolutionScaleEnabled)
            return (x, y);
        return (
            x / AuthoredWidth * viewport.Width,
            y / AuthoredHeight * viewport.Height);
    }

    /// <summary>
    /// <c>0041CF47</c>. Frontend first-seen
    /// <c>[0x13B86A0]==0</c> returns 1,1.
    /// </summary>
    public static (float ScaleX, float ScaleY) GlobalUiScale(FrontendViewport viewport)
    {
        if (!viewport.GamePresent)
            return (1f, 1f);
        var width = viewport.ResolutionScaleEnabled ? viewport.Width : AuthoredWidth;
        var height = viewport.ResolutionScaleEnabled ? viewport.Height : AuthoredHeight;
        if (width < GlobalWidthFloor || height < GlobalHeightFloor)
            return (width / GlobalWidthFloor, height / GlobalHeightFloor);
        return (1f, 1f);
    }

    /// <summary>
    /// <c>0052F5C0</c> dest scale at
    /// <c>+264/+268</c>.
    /// </summary>
    public static (float ScaleX, float ScaleY) ComputeDestScale(
        FrontendWidgetLayout widget,
        float inheritScaleX,
        float inheritScaleY,
        FrontendViewport viewport)
    {
        float destX;
        float destY;
        if (!widget.Absolute)
        {
            if (widget.ScaleSizeToViewport)
            {
                var scaled = ApplyResolutionScale(
                    widget.PersistScaleX, widget.PersistScaleY, viewport);
                destX = scaled.X * inheritScaleX;
                destY = scaled.Y * inheritScaleY;
            }
            else
            {
                destX = inheritScaleX * widget.PersistScaleX;
                destY = inheritScaleY * widget.PersistScaleY;
            }
        }
        else if (widget.ScaleSizeToViewport)
        {
            var scaled = ApplyResolutionScale(
                widget.PersistScaleX, widget.PersistScaleY, viewport);
            destX = scaled.X;
            destY = scaled.Y;
        }
        else
        {
            destX = widget.PersistScaleX;
            destY = widget.PersistScaleY;
        }

        if (!widget.HasParent || widget.Absolute)
        {
            var global = GlobalUiScale(viewport);
            destX *= global.ScaleX;
            destY *= global.ScaleY;
        }

        return (destX, destY);
    }

    /// <summary>
    /// <c>0052FFD0</c> dest origin at
    /// <c>+248/+252</c>.
    /// </summary>
    public static (float X, float Y) ComputeDestOrigin(
        FrontendWidgetLayout widget,
        float inheritScaleX,
        float inheritScaleY,
        float parentDestX,
        float parentDestY,
        FrontendViewport viewport)
    {
        float x;
        float y;
        if (widget.ScaleOriginToViewport)
        {
            var scaled = ApplyResolutionScale(
                widget.PositionX, widget.PositionY, viewport);
            x = scaled.X;
            y = scaled.Y;
        }
        else
        {
            x = widget.PositionX;
            y = widget.PositionY;
        }

        if (!widget.Absolute)
        {
            x = x * inheritScaleX + parentDestX;
            y = y * inheritScaleY + parentDestY;
        }

        return (x, y);
    }

    /// <summary>
    /// <c>0041AC20</c>:
    /// <c>cmp [esi+376], ebx</c> /
    /// <c>jbe</c> skip. Nonzero
    /// GraphicIndex stores bank
    /// vtbl+84/+88 (frame w/h) into
    /// leftover <c>+204/+208</c>.
    /// Not persist Width/Height.
    /// Not font measure.
    /// </summary>
    public static (float W, float H) LeftoverFromGraphic(
        int graphicIndex, float frameWidth, float frameHeight)
    {
        if (graphicIndex == 0)
            return (0f, 0f);
        return (frameWidth, frameHeight);
    }

    /// <summary>
    /// <c>0041AFA0</c> submit dest. Size is
    /// <c>+360/+364</c> when nonzero else
    /// leftover <c>+204/+208</c>, then
    /// <c>* +264/+268</c> from
    /// <c>+248/+252</c>. Center is
    /// <c>vtbl+424</c> <c>0052F1E0</c>
    /// (<c>+302</c> bit 1).
    /// </summary>
    public static (float X0, float Y0, float X1, float Y1) ComputeSubmitDest(
        int persistWidth,
        int persistHeight,
        float leftoverW,
        float leftoverH,
        float originX,
        float originY,
        float destScaleX,
        float destScaleY,
        bool center)
    {
        var width = persistWidth != 0 ? persistWidth : leftoverW;
        var height = persistHeight != 0 ? persistHeight : leftoverH;
        width *= destScaleX;
        height *= destScaleY;
        float x0;
        float y0;
        float x1;
        float y1;
        if (center)
        {
            var halfW = width * Half;
            var halfH = height * Half;
            x0 = originX - halfW;
            y0 = originY - halfH;
            x1 = originX + halfW;
            y1 = originY + halfH;
        }
        else
        {
            x0 = originX;
            y0 = originY;
            x1 = originX + width;
            y1 = originY + height;
        }

        return (Snap(x0), Snap(y0), Snap(x1), Snap(y1));
    }

    /// <summary>
    /// One widget: <c>005339B0</c> inherit
    /// init, parent apply from
    /// <c>00531EC0</c>, then
    /// <c>0052F5C0</c> / <c>0052FFD0</c> /
    /// <c>0041AFA0</c>.
    /// </summary>
    public static FrontendDest Compute(
        FrontendWidgetLayout widget,
        FrontendDest? parent,
        FrontendViewport viewport)
    {
        float inheritX;
        float inheritY;
        float parentDestX;
        float parentDestY;
        var child = widget;
        if (parent is { } p)
        {
            inheritX = p.ScaleX;
            inheritY = p.ScaleY;
            parentDestX = p.OriginX;
            parentDestY = p.OriginY;
            child = widget with { HasParent = true };
        }
        else
        {
            (inheritX, inheritY) = InitInheritedScale(widget.InheritScaleFlag);
            parentDestX = 0f;
            parentDestY = 0f;
        }

        var scale = ComputeDestScale(child, inheritX, inheritY, viewport);
        var origin = ComputeDestOrigin(
            child, inheritX, inheritY, parentDestX, parentDestY, viewport);
        var dest = ComputeSubmitDest(
            child.PersistWidth, child.PersistHeight,
            child.LeftoverW, child.LeftoverH,
            origin.X, origin.Y,
            scale.ScaleX, scale.ScaleY,
            child.Center);
        return new FrontendDest(
            origin.X, origin.Y,
            scale.ScaleX, scale.ScaleY,
            dest.X0, dest.Y0, dest.X1, dest.Y1);
    }

    private static float Snap(float value) =>
        (float)(int)MathF.Round(value);

    /// <summary>
    /// Type-2 <c>00551340</c> leftover
    /// <c>+204/+208</c> is persist W/H,
    /// not GraphicIndex.
    /// </summary>
    public static (float W, float H) Type2Leftover(float persistWidth, float persistHeight) =>
        (persistWidth, persistHeight);

    /// <summary>
    /// Type-12 persist <c>+326</c> row
    /// stride. When nonzero, first-seen
    /// child authored Y is
    /// <c>index * spacing</c>, not the
    /// persist PositionY (those are 0
    /// on the first New Profile rows).
    /// </summary>
    public static float ListChildAuthoredY(int index, float persistY, float spacing)
    {
        if (index < 0 || spacing == 0f)
            return persistY;
        return index * spacing;
    }

    /// <summary>
    /// <c>00551EA0</c> when def+96 bit 0:
    /// place clones along X from the
    /// parent origin. First and last
    /// keep leftover width; the middle
    /// cell (n==3) fills the leftover
    /// budget. Height is the cell
    /// leftover H.
    /// </summary>
    public static (float X0, float Y0, float X1, float Y1) PlaceTableCell(
        int index,
        int count,
        float originX,
        float originY,
        float leftoverW,
        float leftoverH,
        float cellW,
        float cellH,
        int plus96)
    {
        var height = leftoverH > 0f ? leftoverH : cellH;
        if (height <= 0f)
            height = cellH;
        if ((plus96 & 1) == 0 || count <= 0)
        {
            var w = cellW > 0f ? cellW : leftoverW;
            return (Snap(originX), Snap(originY), Snap(originX + w), Snap(originY + height));
        }

        if (count == 3 && leftoverW > 0f)
        {
            var leftW = cellW > 0f ? cellW : leftoverW / 3f;
            var rightW = leftW;
            var midW = leftoverW - leftW - rightW;
            if (midW < 0f)
                midW = 0f;
            float x0;
            float width;
            if (index == 0)
            {
                x0 = originX;
                width = leftW;
            }
            else if (index == 1)
            {
                x0 = originX + leftW;
                width = midW;
            }
            else
            {
                x0 = originX + leftW + midW;
                width = rightW;
            }

            return (Snap(x0), Snap(originY), Snap(x0 + width), Snap(originY + height));
        }

        var step = leftoverW > 0f ? leftoverW / count : cellW;
        var px = originX + index * step;
        return (Snap(px), Snap(originY), Snap(px + step), Snap(originY + height));
    }
}

public readonly record struct FrontendViewport(
    bool ResolutionScaleEnabled,
    float Width,
    float Height,
    bool GamePresent);

public readonly record struct FrontendWidgetLayout(
    float PositionX,
    float PositionY,
    float PersistScaleX = 1f,
    float PersistScaleY = 1f,
    int PersistWidth = 0,
    int PersistHeight = 0,
    float LeftoverW = 0f,
    float LeftoverH = 0f,
    bool Center = false,
    bool Absolute = false,
    bool ScaleOriginToViewport = false,
    bool ScaleSizeToViewport = false,
    bool HasParent = false,
    int InheritScaleFlag = 0);

public readonly record struct FrontendDest(
    float OriginX,
    float OriginY,
    float ScaleX,
    float ScaleY,
    float X0,
    float Y0,
    float X1,
    float Y1);
