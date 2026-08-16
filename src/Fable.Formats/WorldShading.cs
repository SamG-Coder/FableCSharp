using System.Numerics;

namespace Fable.Formats;

/// <summary>
/// CEngineLightingManager / VSHADER_STATIC_DIRLIGHT_FOG / PSCONST_MAX_FOG_ALPHA.
/// Sun direction is the same Z-up vector the client already used. Fog start/end
/// are not named floats in the exe; they match the visible Albion haze.
/// </summary>
public static class WorldShading
{
    public static readonly Vector3 SunDirection = Vector3.Normalize(new Vector3(0.35f, 0.25f, 0.90f));
    public static readonly Vector3 FogColor = new(0.52f, 0.58f, 0.68f);
    public const float FogStart = 70f;
    public const float FogEnd = 320f;
}
