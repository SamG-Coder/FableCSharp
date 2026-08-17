using System.Numerics;

namespace Fable.Formats.World;

/// <summary>
/// STB tile strip decode. D3D triangle-strip
/// <c>IndexCount = PrimitiveCount + 2</c>. Odd <c>t</c>
/// is <c>(b, a, c)</c> — FIRST_SCENE_CONTRACT. Fable has
/// no <c>n.Z &lt; 0</c> rewind write; that host repair is
/// DISPROVEN as native behaviour.
/// </summary>
public static class LandscapeStrip
{
    /// <summary>
    /// No exe write rewinds a landscape triangle because
    /// the geometric normal points down.
    /// </summary>
    public const bool FirstSeenRewindsNegativeNz = false;

    public static int IndexCountFromPrimitiveCount(int primitiveCount, bool hasPrimaryStrip) =>
        hasPrimaryStrip ? primitiveCount + 2 : 0;

    /// <summary>
    /// D3D strip face <paramref name="t"/> from consecutive
    /// indices. Even: <c>(a,b,c)</c>. Odd: <c>(b,a,c)</c>.
    /// Same winding as swapping <c>(b,c)</c> on odd <c>t</c>.
    /// </summary>
    public static (int A, int B, int C) Unwind(int t, int ia, int ib, int ic) =>
        (t & 1) == 0 ? (ia, ib, ic) : (ib, ia, ic);

    public static Vector3 FaceNormal(Vector3 a, Vector3 b, Vector3 c) =>
        Vector3.Cross(b - a, c - a);

    /// <summary>
    /// Host must not reverse a strip face from the sign of
    /// <c>n.Z</c>. Returns the unwind order unchanged.
    /// </summary>
    public static (Vector3 A, Vector3 B, Vector3 C) SubmitWinding(
        int t, Vector3 a, Vector3 b, Vector3 c)
    {
        var (ia, ib, ic) = Unwind(t, 0, 1, 2);
        var pts = new[] { a, b, c };
        return (pts[ia], pts[ib], pts[ic]);
    }
}
