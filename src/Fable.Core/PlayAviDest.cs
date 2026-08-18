namespace Fable.Core;

/// <summary>
/// Native <c>00628B79</c> PlayAVI dest rectangle.
/// <c>009BEDC0</c> supplies the current viewport
/// W/H. Scale the frame to viewport width, then
/// center leftover height with <c>0x0122F59C</c>
/// (0.5). No pillarbox / fit-to-height branch.
/// </summary>
public static class PlayAviDest
{
    public const float Half = 0.5f;

    /// <summary>
    /// Dest in viewport pixels. X0 is 0, X1 is
    /// screen W. destH is integer
    /// <c>videoH * screenW / videoW</c> (imul/idiv).
    /// Y0/Y1 center the leftover; they go negative
    /// when destH exceeds the viewport (crop).
    /// </summary>
    public static (float X0, float Y0, float X1, float Y1) Pixels(
        int videoWidth, int videoHeight, int screenWidth, int screenHeight)
    {
        var vw = Math.Max(1, videoWidth);
        var vh = Math.Max(1, videoHeight);
        var sw = Math.Max(1, screenWidth);
        var sh = Math.Max(1, screenHeight);
        var destH = (float)((long)vh * sw / vw);
        var y0 = (sh - destH) * Half;
        return (0f, y0, sw, sh - y0);
    }

    /// <summary>Dest in 0–1 framebuffer UV.</summary>
    public static (float X0, float Y0, float X1, float Y1) Uv(
        int videoWidth, int videoHeight, int screenWidth, int screenHeight)
    {
        var sw = (float)Math.Max(1, screenWidth);
        var sh = (float)Math.Max(1, screenHeight);
        var px = Pixels(videoWidth, videoHeight, screenWidth, screenHeight);
        return (px.X0 / sw, px.Y0 / sh, px.X1 / sw, px.Y1 / sh);
    }
}
