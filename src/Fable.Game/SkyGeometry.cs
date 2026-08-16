using System.Numerics;
using Fable.Core;
using Fable.Formats.Meshes;
using Fable.Formats.Sky;

namespace Fable.Game;

/// <summary>
/// CEngineSkyRenderer. Dome uses GRAPHIC_ATMOSPHERIC_SKY_MIDDAY (time-of-day
/// set in textures.big). First-seen <c>00B65A20</c> does not emit
/// <c>stars.dat</c> billboards (44-byte texture-id records + already-bound
/// sky stream). Lens-flare sprites stay unread.
/// </summary>
public static class SkyGeometry
{
    public const int UnlitTextureId = -1;

    public static IReadOnlyList<MeshTriangle> Build(GameInstall install)
    {
        _ = install;
        var triangles = new List<MeshTriangle>(8_000);
        AddDome(triangles, SkyDef.MiddaySkyTextureId);
        return triangles;
    }

    private static void AddDome(List<MeshTriangle> triangles, int textureId)
    {
        var rings = SkyPass.DomeRings;
        var segs = SkyPass.DomeSegments;
        Vector2 Uv(int ring, int seg) =>
            SkyPass.DomeUv(ring, seg, SkyPass.FirstSeenThis16, SkyPass.FirstSeenThis20, SkyPass.FirstSeenInvUvDivisor);

        for (var r = 0; r < rings - 1; r++)
        for (var s = 0; s < segs; s++)
        {
            var s1 = (s + 1) % segs;
            var a = SkyPass.EllipsoidPoint(r, s);
            var b = SkyPass.EllipsoidPoint(r, s1);
            var c = SkyPass.EllipsoidPoint(r + 1, s);
            var d = SkyPass.EllipsoidPoint(r + 1, s1);
            Textured(triangles, a, b, d, Uv(r, s), Uv(r, s1), Uv(r + 1, s1), textureId);
            Textured(triangles, a, d, c, Uv(r, s), Uv(r + 1, s1), Uv(r + 1, s), textureId);
        }

        for (var s = 0; s < segs; s++)
        {
            var az = s * SkyPass.TwoPiOver35;
            var s1 = (s + 1) % segs;
            var az1 = s1 == 0 ? 0f : s1 * SkyPass.TwoPiOver35;
            var lo = Ring(SkyPass.HorizRadius, az, SkyPass.SkirtZ);
            var hi = Ring(SkyPass.HorizRadius, az, SkyPass.PoleZ);
            var lo1 = Ring(SkyPass.HorizRadius, az1, SkyPass.SkirtZ);
            var hi1 = Ring(SkyPass.HorizRadius, az1, SkyPass.PoleZ);
            Textured(triangles, lo, lo1, hi1, default, default, default, textureId);
            Textured(triangles, lo, hi1, hi, default, default, default, textureId);
            var pole = new Vector3(0f, 0f, SkyPass.PoleZ);
            Textured(triangles, pole, hi, hi1, default, default, default, textureId);
            var nadir = new Vector3(0f, 0f, SkyPass.NadirZ);
            Textured(triangles, nadir, lo1, lo, default, default, default, textureId);
        }
    }

    private static Vector3 Ring(float radius, float az, float z) =>
        new(radius * MathF.Sin(az), radius * MathF.Cos(az), z);

    private static void Textured(
        List<MeshTriangle> triangles,
        Vector3 a, Vector3 b, Vector3 c,
        Vector2 ua, Vector2 ub, Vector2 uc,
        int textureId)
    {
        triangles.Add(new MeshTriangle(
            a, b, c, Vector3.Zero,
            ua, ub, uc,
            textureId, Vector3.One, Vector3.One, Vector3.One, textureId,
            default, default, default, SceneLayer.Sky));
    }
}
