using System.Numerics;
using Fable.Core;
using Fable.Formats;
using Fable.Formats.Meshes;
using Fable.Formats.Sky;

namespace Fable.Game;

/// <summary>
/// Fable.exe CEngine sky: inner/outer dome plus stars.dat on a sphere.
/// We have no sky mesh in the banks, so the dome is a hemisphere whose
/// horizon matches the fog colour and whose stars come from stars.dat.
/// </summary>
public static class SkyGeometry
{
    public const int UnlitTextureId = -1;
    public const float Radius = 180f;

    public static IReadOnlyList<MeshTriangle> Build(GameInstall install)
    {
        var triangles = new List<MeshTriangle>(3_200);
        AddDome(triangles);
        if (File.Exists(install.StarsPath))
            AddStars(triangles, StarField.Load(install.StarsPath));
        return triangles;
    }

    private static void AddDome(List<MeshTriangle> triangles)
    {
        const int rings = 8;
        const int segments = 24;
        var zenith = new Vector3(0.28f, 0.42f, 0.68f);
        var horizon = WorldShading.FogColor;
        Vector3 Point(int ring, int seg)
        {
            var t = ring / (float)rings;
            var elev = (1f - t) * MathF.PI * 0.5f;
            var az = seg / (float)segments * MathF.PI * 2f;
            return new Vector3(
                MathF.Cos(elev) * MathF.Cos(az),
                MathF.Cos(elev) * MathF.Sin(az),
                MathF.Sin(elev)) * Radius;
        }

        Vector3 Color(int ring)
        {
            var t = ring / (float)rings;
            return Vector3.Lerp(zenith, horizon, t * t);
        }

        for (var r = 0; r < rings; r++)
        for (var s = 0; s < segments; s++)
        {
            var s1 = (s + 1) % segments;
            var a = Point(r, s);
            var b = Point(r, s1);
            var c = Point(r + 1, s);
            var d = Point(r + 1, s1);
            var ca = Color(r);
            var cb = Color(r);
            var cc = Color(r + 1);
            var cd = Color(r + 1);
            Unlit(triangles, a, b, d, ca, cb, cd);
            Unlit(triangles, a, d, c, ca, cd, cc);
        }
    }

    private static void AddStars(List<MeshTriangle> triangles, StarField field)
    {
        foreach (var star in field.Stars)
        {
            if (star.Position.LengthSquared() < 1f)
                continue;
            var dir = Vector3.Normalize(star.Position);
            if (dir.Z < -0.05f)
                continue;
            var pos = dir * (Radius - 2f);
            var right = Vector3.Normalize(Vector3.Cross(dir, MathF.Abs(dir.Z) > 0.9f ? Vector3.UnitY : Vector3.UnitZ));
            var up = Vector3.Normalize(Vector3.Cross(right, dir));
            var size = Math.Clamp(0.12f + star.Size * 0.004f, 0.12f, 0.7f);
            var bright = Math.Clamp(0.35f + star.Brightness * 0.16f, 0.35f, 1f);
            var color = new Vector3(bright, bright, bright * 0.95f);
            var a = pos + (-right - up) * size;
            var b = pos + (right - up) * size;
            var c = pos + (right + up) * size;
            var d = pos + (-right + up) * size;
            Unlit(triangles, a, b, c, color, color, color);
            Unlit(triangles, a, c, d, color, color, color);
        }
    }

    private static void Unlit(
        List<MeshTriangle> triangles,
        Vector3 a, Vector3 b, Vector3 c,
        Vector3 ca, Vector3 cb, Vector3 cc)
    {
        triangles.Add(new MeshTriangle(
            a, b, c, Vector3.Zero,
            default, default, default,
            UnlitTextureId, ca, cb, cc, UnlitTextureId));
    }
}
