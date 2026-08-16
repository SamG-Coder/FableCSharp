namespace Fable.Formats.Scene;

/// <summary>
/// One CRenderManager layer from Fable.exe <c>00B26A75</c>–<c>00B276A8</c>.
/// The frame (<c>00B25950</c>) walks <c>+348…+352</c> in this order.
/// </summary>
public readonly record struct ScenePass(uint Bit, SceneSubmit Submit);

public enum SceneSubmit
{
    None,
    Unread,
    LandscapeBit4,
    LandscapeBit40,
    SkyElse,
    Sky400000,
    Water,
    Shadows,
    Primitives,
}

public static class ScenePasses
{
    /// <summary>
    /// Registration order. Landscape <c>vtbl+16</c> only draws bits 4 and
    /// <c>0x40</c>. Sky else-path is <c>0x2000</c>. <c>0x2000000</c> is a
    /// no-op. Static meshes are submitted once on <c>0x20</c> (first
    /// MainScene+616 bit after landscape FG); the other +616 bits stay unread.
    /// </summary>
    public static readonly ScenePass[] Registration =
    [
        new(0x00000001, SceneSubmit.None),
        new(0x00000002, SceneSubmit.Shadows),
        new(0x00000004, SceneSubmit.LandscapeBit4),
        new(0x00000008, SceneSubmit.Unread),
        new(0x00000010, SceneSubmit.Unread),
        new(0x00000040, SceneSubmit.LandscapeBit40),
        new(0x00000020, SceneSubmit.Primitives),
        new(0x00000100, SceneSubmit.Unread),
        new(0x00000400, SceneSubmit.Unread),
        new(0x00001000, SceneSubmit.Unread),
        new(0x00002000, SceneSubmit.SkyElse),
        new(0x00004000, SceneSubmit.Unread),
        new(0x00008000, SceneSubmit.Unread),
        new(0x00020000, SceneSubmit.Water),
        new(0x00100000, SceneSubmit.Unread),
        new(0x08000000, SceneSubmit.Unread),
        new(0x10000000, SceneSubmit.Unread),
        new(0x00010000, SceneSubmit.Unread),
        new(0x00040000, SceneSubmit.Unread),
        new(0x00000800, SceneSubmit.Unread),
        new(0x00080000, SceneSubmit.Unread),
        new(0x00200000, SceneSubmit.Unread),
        new(0x00400000, SceneSubmit.Sky400000),
        new(0x00800000, SceneSubmit.Unread),
        new(0x02000000, SceneSubmit.None),
        new(0x00000080, SceneSubmit.Unread),
        new(0x00000200, SceneSubmit.Unread),
        new(0x04000000, SceneSubmit.Unread),
        new(0x01000000, SceneSubmit.Unread),
        new(0x08000000, SceneSubmit.Unread),
        new(0x10000000, SceneSubmit.Unread),
        new(0x20000000, SceneSubmit.Unread),
        new(0x40000000, SceneSubmit.Unread),
        new(0x80000000, SceneSubmit.Unread),
    ];

    public static int Rank(uint bit)
    {
        for (var i = 0; i < Registration.Length; i++)
        {
            if (Registration[i].Bit == bit)
                return i;
        }

        return int.MaxValue;
    }

    public static bool Draws(SceneSubmit submit) =>
        submit is SceneSubmit.LandscapeBit4 or SceneSubmit.LandscapeBit40
            or SceneSubmit.SkyElse or SceneSubmit.Primitives;

    public static float ShaderMode(SceneSubmit submit) => submit switch
    {
        SceneSubmit.LandscapeBit4 => 0f,
        SceneSubmit.LandscapeBit40 => 1f,
        SceneSubmit.SkyElse => 2f,
        SceneSubmit.Primitives => 3f,
        _ => 1f,
    };

    public static IReadOnlyList<ScenePass> DrawnPasses(Meshes.SceneLayer layer)
    {
        var submit = layer switch
        {
            Meshes.SceneLayer.Landscape => (SceneSubmit[]) [SceneSubmit.LandscapeBit4, SceneSubmit.LandscapeBit40],
            Meshes.SceneLayer.Sky => [SceneSubmit.SkyElse],
            _ => [SceneSubmit.Primitives],
        };
        return Registration.Where(p => submit.Contains(p.Submit)).ToArray();
    }
}
