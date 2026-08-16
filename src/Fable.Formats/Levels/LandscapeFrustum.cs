using System.Numerics;

namespace Fable.Formats.Levels;

/// <summary>
/// Landscape patch submit <c>00BDC2D0</c> (only <c>E8</c> from
/// <c>00B6B1A5</c> when draw arg+4 is <c>0x40</c>). Four planes at
/// camera <c>[0x1436EA0]+448</c>, stride 16. Extract <c>00B2FD60</c>
/// writes them via <c>00A42140</c> at +448/+464/+480/+496 after
/// normalize <c>00A14440</c> (divide by length, <c>0x122DED8=1</c>).
/// AABB on the patch object: +168 is the n&gt;0 corner (max), +180
/// the n≤0 corner (min). <c>0x122DEDC=0</c>. Reject when
/// <c>n·p &gt; d</c>. Missing AABB (<c>[patch+4]==0</c>) submits every
/// cell. Plane values themselves stay in the camera object; this
/// type only encodes the compare.
/// </summary>
public static class LandscapeFrustum
{
    public const int PlaneCount = 4;
    public const int PlaneStrideBytes = 16;
    public const int PlaneBaseOffset = 448;
    public const int AabbMaxOffset = 168;
    public const int AabbMinOffset = 180;
    public const uint Extract = 0x00B2FD60;
    public const uint Normalize = 0x00A14440;
    public const uint StorePlane = 0x00A42140;
    public const uint PatchSubmit = 0x00BDC2D0;
    public const uint PatchSubmitCaller = 0x00B6B1A5;
    public const int LandscapeBit40 = 0x40;
    public const float CompareZero = 0f;
    public const bool FirstSeenUsesFourPlaneAabb = true;

    public readonly record struct Plane(Vector3 Normal, float D);

    /// <summary>
    /// <c>00BDC2D0</c>: for each axis, <c>n[i] &gt; 0</c> picks max
    /// else min. <c>fcomp d</c> / <c>test ah, 0x41</c> / <c>je</c>
    /// reject is <c>n·p &gt; d</c>.
    /// </summary>
    public static bool AabbIsOutside(Vector3 min, Vector3 max, ReadOnlySpan<Plane> planes)
    {
        foreach (var plane in planes)
        {
            var p = new Vector3(
                plane.Normal.X > CompareZero ? max.X : min.X,
                plane.Normal.Y > CompareZero ? max.Y : min.Y,
                plane.Normal.Z > CompareZero ? max.Z : min.Z);
            if (Vector3.Dot(plane.Normal, p) > plane.D)
                return true;
        }

        return false;
    }
}
