using System.Numerics;
using Fable.Core;
using Fable.Formats.Defs;
using Fable.Formats.Meshes;
using Fable.Formats.Sky;

namespace Fable.Game;

/// <summary>
/// CEngineSkyRenderer. Dome uses GRAPHIC_ATMOSPHERIC_SKY_MIDDAY (time-of-day
/// set in textures.big). Stars use SKY_DEF.StarTexture + stars.dat.
/// Lens-flare sprites stay unread (CEngineSkyRenderer flare list).
/// </summary>
public static class SkyGeometry
{
    public const int UnlitTextureId = -1;

    public static IReadOnlyList<MeshTriangle> Build(GameInstall install)
    {
        var triangles = new List<MeshTriangle>(8_000);
        SkyDef? def = null;
        var namesPath = install.FindCompiledDef("names.bin");
        var binPath = install.FindCompiledDef("game.bin");
        if (namesPath is not null && binPath is not null)
            def = SkyDef.TryLoadFromGameBin(GameBin.Load(binPath, NamesBin.Load(namesPath)));

        AddDome(triangles, SkyDef.MiddaySkyTextureId);
        if (File.Exists(install.StarsPath))
            AddStars(triangles, StarField.Load(install.StarsPath), def?.StarTextureId ?? SkyDef.StarTextureIdDefault);
        return triangles;
    }

    private static void AddDome(List<MeshTriangle> triangles, int textureId)
    {
        var rings = SkyPass.DomeRings;
        var segs = SkyPass.DomeSegments;
        Vector2 Uv(int ring, int seg) =>
            new(seg / (float)segs, ring / (float)(rings - 1));

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

    private static void AddStars(List<MeshTriangle> triangles, StarField field, int textureId)
    {
        var origin = new Vector3(64f, 64f, 0f);
        foreach (var star in field.Stars)
        {
            if (star.Position.LengthSquared() < 1f)
                continue;
            var dir = Vector3.Normalize(star.Position);
            if (dir.Z < -0.05f)
                continue;
            var pos = origin + dir * 1700f;
            var right = Vector3.Normalize(Vector3.Cross(dir, MathF.Abs(dir.Z) > 0.9f ? Vector3.UnitY : Vector3.UnitZ));
            var up = Vector3.Normalize(Vector3.Cross(right, dir));
            var size = Math.Clamp(0.8f + star.Size * 0.02f, 0.8f, 4f);
            var a = pos + (-right - up) * size;
            var b = pos + (right - up) * size;
            var c = pos + (right + up) * size;
            var d = pos + (-right + up) * size;
            Textured(triangles, a, b, c, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), textureId);
            Textured(triangles, a, c, d, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 1), textureId);
        }
    }

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
