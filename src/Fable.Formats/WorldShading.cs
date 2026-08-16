using System.Numerics;

namespace Fable.Formats;

/// <summary>
/// Directional term fed as VS c19 / landscape c42. Exact register values are
/// unread (CEngineVSConstantLayoutLights). Fog is oFog from c2/c18 in the
/// VS — not a CPU start/end pair — so we do not invent distances here.
/// </summary>
public static class WorldShading
{
    public static readonly Vector3 SunDirection = Vector3.Normalize(new Vector3(0.35f, 0.25f, 0.90f));
    public static readonly Vector3 FogColor = new(0.52f, 0.58f, 0.68f);
    public const float FogStart = 0f;
    public const float FogEnd = 7000f;
}
