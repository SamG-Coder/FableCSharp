using System.Numerics;

namespace Fable.Formats;

/// <summary>
/// First-seen VS programs (<c>VSHADER_STATIC_DIRLIGHT_FOG</c>,
/// <c>VSHADER_LANDSCAPE_FOREGROUND</c>, <c>VSHADER_PALSKIN_DIRLIGHT_FOG</c>)
/// read CONST slots including <c>c20</c> and <c>c35</c>. The float4s written
/// there are still UNREAD — do not invent a sun direction or N·L scale.
/// Fog is VS <c>oFog</c> from <c>c2</c>/<c>c18</c>; far 7000 is SKY_DEF.
/// </summary>
public static class WorldShading
{
    public static readonly Vector3 FogColor = new(0.52f, 0.58f, 0.68f);
    public const float FogStart = 0f;
    public const float FogEnd = 7000f;
}
