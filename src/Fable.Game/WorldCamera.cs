using System.Numerics;

namespace Fable.Game;

/// <summary>
/// World object at <c>+24</c>. Alloc
/// <c>0x1970</c>, ctor <c>006B4900</c>,
/// vtbl <c>0125D53C</c>. <c>0049E080</c>
/// calls <c>006B42F0(this, t)</c>.
/// </summary>
public sealed class WorldCamera
{
    public const uint Ctor = 0x006B4900;
    public const uint Vtbl = 0x0125D53C;
    public const uint SeedFn = 0x006B3FF0;
    /// <summary>
    /// <c>006B3FF0</c> after the
    /// <c>008889C0</c> loop:
    /// <c>006B2CA0</c> then
    /// <c>006B3030</c> /
    /// <c>006B3B80</c>.
    /// </summary>
    public const uint PoseFn = 0x006B2CA0;
    public const uint PoseFollowFn = 0x006B3030;
    public const uint PoseTickFn = 0x006B3B80;
    public const uint NormalizeFn = 0x00A14440;
    public const uint FollowSlotFn = 0x008889C0;
    public const uint ZeroFloatVa = 0x0122DEDC;
    public const uint OneFloatVa = 0x0122DED8;
    public const int SkipFlagOffset = 61;
    public const int YawOffset = 3084;
    public const int PitchOffset = 424;
    public const int BlendOffset = 412;
    public const int OutDirOffset = 3120;
    public const uint BlendFn = 0x006B42F0;
    public const uint SlotCtor = 0x008864A0;
    public const int ObjectSize = 0x1970;
    public const int WorldOffset = 24;
    public const int SlotAOffset = 3084;
    public const int SlotBOffset = 6188;
    public const int SlotStride = SlotBOffset - SlotAOffset;
    public const int OutParamOffset = 6292;
    public const int Out0Offset = 6296;
    public const int Out1Offset = 6312;
    public const int Out2Offset = 6328;
    public const int Out3Offset = 6340;
    public const int Out4Offset = 6352;
    public const float DefaultWeight = 0.2f;
    public const float DefaultAxisX = 1f;

    public uint VtblValue { get; private set; } = Vtbl;
    public bool Seeded { get; private set; }
    /// <summary>
    /// <c>[+61]</c>. Ctor
    /// <c>006B4900</c> writes 0 so
    /// <c>006B2CA0</c> runs.
    /// </summary>
    public bool PoseSkipFlag { get; private set; }
    public bool PoseComputed { get; private set; }
    public bool FollowSpringRan { get; private set; }
    public const uint FollowRngFn = 0x004978A0;
    public const uint YawRotateFn = 0x00A14260;
    public const float FollowWeightMin = 0.04f;
    public const float FollowWeightMax = 0.2f;
    public const uint FollowWeightMinBits = 0x3D23D70A;
    public const uint FollowWeightMaxBits = 0x3E4CCCCD;
    public CameraSlot SlotA { get; private set; } = CameraSlot.CtorDefault();
    public CameraSlot SlotB { get; private set; } = CameraSlot.Zero();
    public CameraSlot Output { get; private set; } = CameraSlot.Zero();
    public float LastBlend { get; private set; }

    /// <summary>
    /// <c>006B4900</c>: six <c>008864A0</c>
    /// slots at +84 / +3188, then
    /// <c>+3092/+3108=(1,0,0)</c>,
    /// <c>+3088/+3104=0.2</c>, <c>+68=0</c>.
    /// </summary>
    public void Construct()
    {
        VtblValue = Vtbl;
        Seeded = false;
        PoseSkipFlag = false;
        PoseComputed = false;
        FollowSpringRan = false;
        SlotA = CameraSlot.CtorDefault();
        SlotB = CameraSlot.Zero();
        Output = CameraSlot.Zero();
        LastBlend = 0f;
    }

    /// <summary>
    /// <c>006B3FF0</c> when <c>+68==0</c>.
    /// Follow-slot fill <c>008889C0</c>
    /// is a list helper; first-seen
    /// <c>008864A0</c> zeros
    /// <c>+412/+424…+444</c> and
    /// <c>+3084=0</c>. Then
    /// <c>006B2CA0</c>. Does not invent
    /// a 1.6 m eye.
    /// </summary>
    public void SeedHero()
    {
        ComputePose();
        SlotB = SlotA;
        Seeded = true;
    }

    /// <summary>
    /// <c>006B2CA0</c>. First-seen ctor
    /// angles are 0 and
    /// <c>[0x122DEDC]=0</c> so the two
    /// normalised dirs are
    /// <c>(1,0,0)</c>; blend
    /// <c>+412=0</c> writes
    /// <c>V4=(-1,0,0)</c>.
    /// <c>00A14440</c> is in-place
    /// normalize. <c>006B3030</c> /
    /// <c>006B3B80</c> stay UNREAD.
    /// </summary>
    public void ComputePose()
    {
        if (PoseSkipFlag)
            return;
        var first = Normalize(new Vector3(1f, 0f, 0f));
        var second = Normalize(new Vector3(1f, 0f, 0f));
        var k = 0f;
        var blended = Normalize(-first * (1f - k) - second * k);
        SlotA = SlotA with { V2 = first, V3 = second, V4 = blended };
        PoseComputed = true;
    }

    /// <summary>
    /// <c>006B3030</c>. Gate
    /// <c>[+3168]==0</c>. <c>004978A0</c>
    /// is an LCG (not a named float);
    /// first-seen seed is UNREAD so the
    /// yaw rotate is not applied.
    /// Weight0 already 0.2 stays inside
    /// <c>[0.04, 0.2]</c>. V0/V1 stay
    /// ctor <c>(1,0,0)</c>.
    /// </summary>
    public void ApplyFollowSpring()
    {
        FollowSpringRan = true;
        var w = SlotA.Weight0;
        if (w < FollowWeightMin)
            w = FollowWeightMin;
        else if (w > FollowWeightMax)
            w = FollowWeightMax;
        SlotA = SlotA with { Weight0 = w };
    }

    private static Vector3 Normalize(Vector3 v)
    {
        var len = v.Length();
        return len > 1e-8f ? v / len : Vector3.Zero;
    }

    /// <summary>
    /// <c>006B42F0</c>. Clamps t to
    /// <c>[0,1]</c>. First call with
    /// <c>+68==0</c> runs <c>006B3FF0</c>
    /// then lerps B→A.
    /// </summary>
    public CameraSlot Blend(float t)
    {
        if (t < 0f)
            t = 0f;
        else if (t > 1f)
            t = 1f;
        if (!Seeded)
        {
            Seeded = true;
        }

        LastBlend = t;
        Output = CameraSlot.Lerp(SlotB, SlotA, t);
        return Output;
    }

    /// <summary>
    /// <c>006B3FF0</c> when <c>+68==0</c>:
    /// both slots take the current subject
    /// so a later <c>006B42F0</c> lerp at
    /// t=0 stays on the hero.
    /// </summary>
    public void SeedAt(Vector3 position, Vector3 lookAt, Vector3 up)
    {
        var slot = SlotA with
        {
            V0 = position,
            V1 = lookAt,
            V2 = up.LengthSquared() > 1e-8f ? Vector3.Normalize(up) : Vector3.UnitZ,
        };
        SlotA = slot;
        SlotB = slot;
        Seeded = true;
    }

    /// <summary>
    /// Push the current target into B and
    /// write a new A. Used when a named
    /// camera bind arrives.
    /// </summary>
    public void WriteTarget(Vector3 position, Vector3 lookAt, Vector3 up)
    {
        SlotB = SlotA;
        SlotA = SlotA with
        {
            V0 = position,
            V1 = lookAt,
            V2 = up.LengthSquared() > 1e-8f ? Vector3.Normalize(up) : Vector3.UnitZ,
        };
    }
}

public readonly record struct CameraSlot(
    float Param,
    float Weight0,
    Vector3 V0,
    float Weight1,
    Vector3 V1,
    Vector3 V2,
    Vector3 V3,
    Vector3 V4)
{
    public static CameraSlot Zero() => default;

    public static CameraSlot CtorDefault() => new(
        0f,
        WorldCamera.DefaultWeight,
        new Vector3(WorldCamera.DefaultAxisX, 0f, 0f),
        WorldCamera.DefaultWeight,
        new Vector3(WorldCamera.DefaultAxisX, 0f, 0f),
        Vector3.Zero,
        Vector3.Zero,
        Vector3.Zero);

    public static CameraSlot Lerp(CameraSlot from, CameraSlot to, float t)
    {
        var s = 1f - t;
        return new CameraSlot(
            from.Param * s + to.Param * t,
            from.Weight0 * s + to.Weight0 * t,
            from.V0 * s + to.V0 * t,
            from.Weight1 * s + to.Weight1 * t,
            from.V1 * s + to.V1 * t,
            from.V2 * s + to.V2 * t,
            from.V3 * s + to.V3 * t,
            from.V4 * s + to.V4 * t);
    }
}

/// <summary>
/// <c>006FD8C0</c> object at world+44,
/// size <c>0xC8</c>, vtbl <c>01264A8C</c>.
/// </summary>
public sealed class GameCamera
{
    public const uint Ctor = 0x006FD8C0;
    public const uint Vtbl = 0x01264A8C;
    public const uint ScaleVa = 0x0125AADC;
    public const float Scale = 1.5f;
    public const int ObjectSize = 0xC8;
    public const int WorldOffset = 44;
    public const float Plus144 = 0.2f;
    public const float Plus148 = 1.25f;
    public const float Plus152 = 10f;
    public const float Plus192 = 0.1f;

    public uint VtblValue { get; private set; }
    public int Plus176 { get; private set; }
    public bool Constructed { get; private set; }

    public void Construct()
    {
        VtblValue = Vtbl;
        Plus176 = EngineLifecycle.FistpTowardZero(
            EngineLifecycle.CameraCatchupMin * Scale);
        Constructed = true;
    }
}

/// <summary>
/// <c>0069AE80</c> object at world+48,
/// size <c>0x160</c>, vtbl <c>0125C754</c>.
/// Copied to world+52.
/// </summary>
public sealed class GameCameraManager
{
    public const uint Ctor = 0x0069AE80;
    public const uint Vtbl = 0x0125C754;
    public const int ObjectSize = 0x160;
    public const int WorldOffset = 48;
    public const int WorldCopyOffset = 52;
    public const string NullMode = "CAMERA_MODE_MODE_NULL";
    public const string SetDefault = "CAMERA_MANAGER_SET_DEFAULT";

    public uint VtblValue { get; private set; }
    public bool Constructed { get; private set; }

    public void Construct()
    {
        VtblValue = Vtbl;
        Constructed = true;
    }
}
