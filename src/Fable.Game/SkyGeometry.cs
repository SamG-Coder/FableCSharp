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
        const int rings = 12;
        const int segments = 32;
        const float radius = 1800f;
        var origin = new Vector3(64f, 64f, 0f);
        Vector3 Point(int ring, int seg)
        {
            var t = ring / (float)rings;
            var elev = (0.5f - t) * MathF.PI;
            var az = seg / (float)segments * MathF.PI * 2f;
            return origin + new Vector3(
                MathF.Cos(elev) * MathF.Cos(az),
                MathF.Cos(elev) * MathF.Sin(az),
                MathF.Sin(elev)) * radius;
        }

        Vector2 Uv(int ring, int seg) =>
            new(seg / (float)segments, ring / (float)rings);

        for (var r = 0; r < rings; r++)
        for (var s = 0; s < segments; s++)
        {
            var s1 = (s + 1) % segments;
            var a = Point(r, s);
            var b = Point(r, s1);
            var c = Point(r + 1, s);
            var d = Point(r + 1, s1);
            Textured(triangles, a, b, d, Uv(r, s), Uv(r, s1), Uv(r + 1, s1), textureId);
            Textured(triangles, a, d, c, Uv(r, s), Uv(r + 1, s1), Uv(r + 1, s), textureId);
        }
    }

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
