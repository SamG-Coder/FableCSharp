using System.Numerics;

namespace Fable.Formats.Levels;

/// <summary>
/// Landscape patch submit <c>00BDC2D0</c> (only <c>E8</c> from
/// <c>00B6B1A5</c> when draw arg+4 is <c>0x40</c>). Four planes at
/// camera <c>[0x1436EA0]+448</c>, stride 16. Setup <c>00B30B50</c>
/// copies the source camera, writes cot half-FOV at +212/+216, inverts
/// the cot-scaled view 3x4 at +228, then <c>00B2FD60</c> (arg1≠0)
/// unprojects eye + four NDC corners and stores via <c>00A42140</c>
/// at +448/+464/+480/+496 after normalize <c>00A14440</c> (divide by
/// length, <c>0x122DED8=1</c>). AABB on the patch object: +168 is the
/// n&gt;0 corner (min), +180 the n≤0 corner (max) — the n-vertex for a
/// fully-outside test. <c>0x122DEDC=0</c>. Reject when <c>n·p &gt; d</c>.
/// Missing AABB (<c>[patch+4]==0</c>) submits every cell.
/// </summary>
public static class LandscapeFrustum
{
    public const int PlaneCount = 4;
    public const int PlaneStrideBytes = 16;
    public const int PlaneBaseOffset = 448;
    public const int AabbMinOffset = 168;
    public const int AabbMaxOffset = 180;
    public const uint Extract = 0x00B2FD60;
    public const uint Normalize = 0x00A14440;
    public const uint StorePlane = 0x00A42140;
    public const uint CameraSetup = 0x00B30B50;
    public const uint ExtractOther = 0x00B2FC50;
    public const uint CameraCopy = 0x00B4AF50;
    public const uint PatchSubmit = 0x00BDC2D0;
    public const uint PatchSubmitCaller = 0x00B6B1A5;
    public const int LandscapeBit40 = 0x40;
    public const float CompareZero = 0f;
    public const float NormalizeDivisor = 1f;
    public const float FovHalfScale = 0.5f;
    public const float LetterboxFourByThree = 0.75f;
    public const int ViewMatrixOffset = 128;
    public const int InverseOffset = 228;
    public const int ViewportWidthOffset = 176;
    public const int ViewportHeightOffset = 180;
    public const int CotHOffset = 212;
    public const int CotVOffset = 216;
    public const int TwoFovFlagOffset = 84;
    public const int FovHOffset = 76;
    public const int FovVOffset = 80;
    public const int SourceReadyOffset = 104;
    public const int CameraPosOffset = 64;
    public const bool FirstSeenUsesFourPlaneAabb = true;

    public readonly record struct Plane(Vector3 Normal, float D);

    /// <summary>
    /// <c>00B30B50</c> +84 set: <c>1/tan(fov*0.5)</c> from source+76 / +80.
    /// </summary>
    public static float CotHalfAngle(float radians) =>
        1f / MathF.Tan(radians * FovHalfScale);

    /// <summary>
    /// <c>00B30B50</c> +84 clear: <c>0.75 - h/w</c> letterbox, then
    /// <c>cotH = 1/tan(scaled*0.5)</c> and <c>cotV = cotH * (w/h)</c>.
    /// A 4:3 viewport leaves the horizontal FOV unchanged.
    /// </summary>
    public static void LetterboxCots(float fovRadians, float width, float height, out float cotH, out float cotV)
    {
        var aspect = width / height;
        var scaled = ((LetterboxFourByThree - height / width) * FovHalfScale + 1f) * fovRadians;
        cotH = CotHalfAngle(scaled);
        cotV = cotH * aspect;
    }

    /// <summary>
    /// <c>00B2FD60</c>: unproject eye <c>(0,0,0)</c> and the four NDC
    /// corners <c>(±1, ±1, 1)</c> through the inverse of the cot-scaled
    /// world-to-view 3x4, then <c>n = (Pnext-Eye) × (Pprev-Eye)</c>,
    /// normalize, <c>d = n·Eye</c>. View +Z is look; +Y is camera up;
    /// right is <c>look × up</c>. Screen-top is NDC +Y because extract
    /// uses <c>(centerY - screenY) / halfH</c>.
    /// </summary>
    public static Plane[] ExtractSidePlanes(
        Vector3 position, Vector3 look, Vector3 up, float cotH, float cotV)
    {
        var forward = Vector3.Normalize(look);
        var right = Vector3.Normalize(Vector3.Cross(forward, up));
        if (right.LengthSquared() < 1e-12f)
            right = Vector3.UnitX;
        up = Vector3.Cross(right, forward);

        var t = new Vector3(
            -Vector3.Dot(right, position),
            -Vector3.Dot(up, position),
            -Vector3.Dot(forward, position));
        Invert3x4(
            right * cotH,
            up * cotV,
            forward,
            new Vector3(t.X * cotH, t.Y * cotV, t.Z),
            out var i0, out var i1, out var i2, out var it);

        var eye = Unproject(i0, i1, i2, it, Vector3.Zero);
        var pLt = Unproject(i0, i1, i2, it, new Vector3(-1f, 1f, 1f));
        var pRt = Unproject(i0, i1, i2, it, new Vector3(1f, 1f, 1f));
        var pRb = Unproject(i0, i1, i2, it, new Vector3(1f, -1f, 1f));
        var pLb = Unproject(i0, i1, i2, it, new Vector3(-1f, -1f, 1f));
        return
        [
            MakePlane(pRt - eye, pLt - eye, eye),
            MakePlane(pRb - eye, pRt - eye, eye),
            MakePlane(pLb - eye, pRb - eye, eye),
            MakePlane(pLt - eye, pLb - eye, eye),
        ];
    }

    /// <summary>
    /// <c>00BDC2D0</c>: for each axis, <c>n[i] &gt; 0</c> picks min
    /// else max (n-vertex). <c>fcomp d</c> / <c>test ah, 0x41</c> /
    /// <c>je</c> reject is <c>n·p &gt; d</c> (fully outside).
    /// </summary>
    public static bool AabbIsOutside(Vector3 min, Vector3 max, ReadOnlySpan<Plane> planes)
    {
        foreach (var plane in planes)
        {
            var p = new Vector3(
                plane.Normal.X > CompareZero ? min.X : max.X,
                plane.Normal.Y > CompareZero ? min.Y : max.Y,
                plane.Normal.Z > CompareZero ? min.Z : max.Z);
            if (Vector3.Dot(plane.Normal, p) > plane.D)
                return true;
        }

        return false;
    }

    public static bool AabbIsOutside(IEnumerable<(Vector3 A, Vector3 B, Vector3 C)> triangles, ReadOnlySpan<Plane> planes)
    {
        var min = new Vector3(float.PositiveInfinity);
        var max = new Vector3(float.NegativeInfinity);
        var any = false;
        foreach (var (a, b, c) in triangles)
        {
            any = true;
            min = Vector3.Min(min, Vector3.Min(a, Vector3.Min(b, c)));
            max = Vector3.Max(max, Vector3.Max(a, Vector3.Max(b, c)));
        }

        return any && AabbIsOutside(min, max, planes);
    }

    private static Vector3 Unproject(Vector3 i0, Vector3 i1, Vector3 i2, Vector3 it, Vector3 dir) =>
        dir.X * i0 + dir.Y * i1 + dir.Z * i2 + it;

    private static Plane MakePlane(Vector3 next, Vector3 prev, Vector3 eye)
    {
        var n = Vector3.Cross(next, prev);
        var length = n.Length();
        if (length > 1e-8f)
            n /= length;
        return new Plane(n, Vector3.Dot(n, eye));
    }

    private static void Invert3x4(
        Vector3 r0, Vector3 r1, Vector3 r2, Vector3 t,
        out Vector3 i0, out Vector3 i1, out Vector3 i2, out Vector3 it)
    {
        var det = Vector3.Dot(r0, Vector3.Cross(r1, r2));
        var invDet = NormalizeDivisor / det;
        i0 = Vector3.Cross(r1, r2) * invDet;
        i1 = Vector3.Cross(r2, r0) * invDet;
        i2 = Vector3.Cross(r0, r1) * invDet;
        it = -(i0 * t.X + i1 * t.Y + i2 * t.Z);
    }
}
