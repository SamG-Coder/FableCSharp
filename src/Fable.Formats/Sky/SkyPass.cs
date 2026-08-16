namespace Fable.Formats.Sky;

/// <summary>
/// First-seen sky else-path <c>00B662F0</c> when layer bit is not
/// <c>0x400000</c>. <c>00B30B50</c> is called on
/// <c>[0x1436EA0]</c> with source near <c>0x42C80000=100</c>,
/// far <c>0x461C4000=10000</c>, minZ from <c>0x0139A704=0.99</c>,
/// maxZ 1. Then <c>00B66190</c> draws outer <c>00B63C00</c> and
/// inner <c>00B640E0</c>. A second <c>00B30B50</c> restores the
/// world camera. <c>VSHADER_INNER_SKY</c> is
/// <c>dp4 oPos, v0, c5–c8</c> (no <c>c4</c>).
/// </summary>
public static class SkyPass
{
    public const uint Draw = 0x00B662F0;
    public const uint DrawElse = 0x00B66416;
    public const uint MeshDraw = 0x00B66190;
    public const uint OuterDraw = 0x00B63C00;
    public const uint InnerDraw = 0x00B640E0;
    public const uint SkyCameraSetup = 0x00B30B50;
    public const uint LayerBit400000 = 0x400000;
    public const uint FirstSeenLayerBit = 0x2000;
    public const bool FirstSeenUses400000 = false;
    public const float FirstSeenNear = 100f;
    public const float FirstSeenFar = 10000f;
    public const float FirstSeenMinZ = 0.99f;
    public const float FirstSeenMaxZ = 1f;
    public const uint NearImm = 0x42C80000;
    public const uint FarImm = 0x461C4000;
    public const uint MinZConst = 0x0139A704;
    public const int WvpStartRegister = 5;
    public const int WvpCount = 4;
}
