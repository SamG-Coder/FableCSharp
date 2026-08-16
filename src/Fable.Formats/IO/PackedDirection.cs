using System.Numerics;

namespace Fable.Formats.IO;

/// <summary>
/// Signed 11-11-10 unit vector. Same bit layout as C3D packed positions,
/// used for landscape tile normals after the XYZ in each 15-byte vert.
/// </summary>
public static class PackedDirection
{
    public static Vector3 Unpack(uint packed)
    {
        var ix = (int)(packed & 0x7FF);
        if ((ix & 0x400) != 0)
            ix |= unchecked((int)0xFFFFF800);
        var iy = (int)((packed >> 11) & 0x7FF);
        if ((iy & 0x400) != 0)
            iy |= unchecked((int)0xFFFFF800);
        var iz = (int)(packed >> 22);
        if ((iz & 0x200) != 0)
            iz |= unchecked((int)0xFFFFFC00);
        var n = new Vector3(ix / 1023f, iy / 1023f, iz / 511f);
        var len = n.Length();
        return len > 1e-6f ? n / len : Vector3.UnitZ;
    }

    public static Vector3 ColorRgb(byte r, byte g, byte b) =>
        new(r / 255f, g / 255f, b / 255f);
}
