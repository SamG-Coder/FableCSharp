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
/// Missing AABB (<c>[patch+4]==0</c>) submits every cell. Fill is
/// <c>00BF6F80</c> (only <c>E8</c> from <c>00BDC280</c>): first-seen
/// start is <c>0,0</c>, size is map +92/+94, origin map +96/+98,
/// <c>min.z=max.z=0</c>. Getter <c>00BDBFC0</c> is <c>&this+168</c>
/// only; <c>00BE9C80</c> ctor-zeros the same slots.
/// <c>00B54310</c> uploads inverse rows to <c>c2/c3/c4</c>; first-seen
/// fog colour <c>c18</c> is record <c>(0,0,0,1)</c>, start 1000, end 2000.
/// First-seen <c>c5–c8</c> WVP is <c>00988A50</c>: world
/// wrapper+496 * view +560 * proj +624 → +752, then
/// <c>SetVSConstantF(c5, count=4)</c>. Proj is <c>009883F0</c> /
/// camera+372: XY identity, <c>M33/M34</c> from helper
/// near 0.1 / far 4000 / minZ 0.1 / maxZ 0.99. View XY already
/// holds the letterbox cots from <c>00B30B50</c>.
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
    public const uint AabbFill = 0x00BF6F80;
    public const uint AabbFillCaller = 0x00BDC280;
    public const uint AabbFillSetup = 0x00BDC180;
    public const uint TessellatorCtor = 0x00BF6E20;
    public const int MapSizeXOffset = 92;
    public const int MapSizeYOffset = 94;
    public const int MapOriginXOffset = 96;
    public const int MapOriginYOffset = 98;
    public const float AabbZ = 0f;
    public const int FirstSeenAabbStartX = 0;
    public const int FirstSeenAabbStartY = 0;
    public const int LandscapeBit40 = 0x40;
    public const float CompareZero = 0f;
    public const float NormalizeDivisor = 1f;
    public const float FovHalfScale = 0.5f;
    public const float LetterboxFourByThree = 0.75f;
    public const uint CameraUpdate = 0x00B314E0;
    public const uint SplineUpdate = 0x00B31160;
    public const uint CameraCtor = 0x00B31700;
    public const uint SplineEnable = 0x00B2FC10;
    public const uint FovFlagGetter = 0x00A0BE80;
    public const uint FovHGetter = 0x00A0BE90;
    public const uint FovVGetter = 0x00A0BEA0;
    public const int HelperFlagsOffset = 40;
    public const int HelperFovHOffset = 44;
    public const int HelperFovVOffset = 48;
    public const int TwoFovFlagBit = 2;
    public const int SplineFlagOffset = 536;
    public const float FovTurnsToDegrees = 360f;
    public const float Inv360 = 1f / 360f;
    public const float TwoPi = 6.283185307179586f;
    public const float FirstSeenFovTurns = 0.2f;
    public const bool FirstSeenTwoFovFlag = false;
    public const bool FirstSeenSplineEnabled = false;
    /// <summary>
    /// <c>00B314E0</c> with <c>+536==0</c> copies helper
    /// <c>+0/+12/+24</c> as pos/look/up then <c>00B30B50</c>.
    /// No hero-follow offset. <c>00B2FBF0</c> only stores the
    /// helper pointer at camera <c>+12</c>. <c>00B2FC10</c> has
    /// zero <c>E8</c> callers. SHOT2 TNG
    /// <c>HeroIsSubject=FALSE</c>.
    /// </summary>
    public const bool FirstSeenUsesThirdPersonView = false;
    public const uint BindSource = 0x00B23B50;
    public const uint StoreSource = 0x00B2FBF0;
    /// <summary>
    /// <c>00B314E0</c> has two <c>E8</c> callers: bind
    /// <c>00B23B6C</c> and pre-pass <c>00B2799D</c>. Neither
    /// names <c>CAM_OVIF_SHOT2</c> — that string is TNG only.
    /// </summary>
    public const int CameraUpdateCallerCount = 2;
    public const uint PrePassUpdate = 0x00B277A0;
    public const int HelperPosOffset = 0;
    public const int HelperLookOffset = 12;
    public const int HelperUpOffset = 24;
    public const int SourcePointerOffset = 12;
    public static readonly Vector3 FirstSeenCameraUp = new(0f, 0f, 1f);
    public const uint ViewThirdPersonRtti = 0x0137F67C;
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
    /// <summary>
    /// <c>00B54310</c> <c>add edi, 0xE4</c> then
    /// <c>00989B00(2, [+228], [+240], [+252], [+264])</c>.
    /// Column stride 12 gathers inverse row 0 as <c>c2</c>.
    /// Only caller is mesh <c>00B555A0</c>. First-seen fog
    /// <c>c2</c> is <c>00B47630</c> linear view-Z, flushed to
    /// wrapper+880. Per-cell <c>00989A60(3)</c> still overwrites
    /// <c>c3</c> with the UV table; fog flush restores <c>c2</c>.
    /// </summary>
    public const uint CameraConstantUpload = 0x00B54310;
    public const uint CameraConstantUploadCaller = 0x00B555A0;
    public const uint SetVsConstantF4 = 0x00989B00;
    public const uint FogCompute = 0x00B47630;
    public const uint FogComputeCaller = 0x00B25877;
    public const uint FogColorSetter = 0x009886C0;
    public const uint FogColorFlush = 0x009897C0;
    public const uint FogPlaneSetter = 0x00988600;
    public const uint LayoutBasic = 0x00BDBB70;
    public const uint LightingRecordAlloc = 0x00B4A4C0;
    public const int InverseRow0Register = 2;
    public const int InverseRow1Register = 3;
    public const int InverseRow2Register = 4;
    public const int InverseColumnStrideBytes = 12;
    public const int LayoutFogRegisterOffset = 56;
    public const int LayoutFogCountOffset = 60;
    public const int LayoutFogRegister = 18;
    public const int LayoutFogCount = 1;
    public const int FogRecordColorOffset = 64;
    public const int FogRecordStartOffset = 80;
    public const int FogRecordEndOffset = 84;
    public const int FogRecordStrideBytes = 112;
    public const int WrapperFogColorOffset = 444;
    public const int WrapperFogPlaneOffset = 880;
    public const int FogDirtyBit = 0x20000;
    public const float FogRecordStart = 1000f;
    public const float FogRecordEnd = 2000f;
    public static readonly Vector4 FogRecordColor = new(0f, 0f, 0f, 1f);
    /// <summary>
    /// Mesh-path <c>00B54310</c> writes inverse row 0 to <c>c2</c>.
    /// First-seen New Game never calls it. Fog <c>c2</c> is
    /// <see cref="Fable.Formats.WorldShading.LinearFogPlane"/>.
    /// </summary>
    public const bool FirstSeenUploadsInverseRow0AsC2 = false;
    /// <summary>
    /// <c>00B54310</c> <c>push ebx</c> with <c>ebx=4</c> writes
    /// inverse row 2 to <c>c4</c>. That site is only reached from
    /// mesh draw <c>00B555A0</c>, not first-seen landscape.
    /// </summary>
    public const bool FirstSeenUploadsInverseRow2AsC4OnLandscape = false;
    /// <summary>
    /// <c>00988A50</c> multiplies wrapper world+496 * view+560 *
    /// proj+624 into +752, then <c>SetVSConstantF</c>
    /// register <c>[inner+120]=5</c> count 4.
    /// </summary>
    public const uint WvpFlush = 0x00988A50;
    public const uint ProjBuilder = 0x009883F0;
    public const uint ViewCopy = 0x00988350;
    public const uint WorldCopy = 0x009881F0;
    public const uint WorldIdentity = 0x00988290;
    public const uint ProjCopy = 0x00988540;
    public const int WrapperWorldOffset = 496;
    public const int WrapperViewOffset = 560;
    public const int WrapperProjOffset = 624;
    public const int WrapperWvpOffset = 752;
    public const int CameraProjOffset = 372;
    public const int CameraZTermM34Offset = 220;
    public const int CameraZTermM33Offset = 224;
    public const int LayoutWvpRegisterOffset = 120;
    public const int LayoutWvpRegister = 5;
    public const int LayoutWvpCount = 4;
    /// <summary>
    /// <c>00B314E0</c> stack <c>0x3DCCCCCD</c> / <c>0x457A0000</c>
    /// then helper +88/+92. <c>0x01399D44</c> is minZ 0.1;
    /// <c>0x3F7D70A4</c> is maxZ 0.99.
    /// </summary>
    public const float FirstSeenNear = 0.1f;
    public const float FirstSeenFar = 4000f;
    public const float FirstSeenMinZ = 0.1f;
    public const float FirstSeenMaxZ = 0.99f;
    public const uint HelperMinZConst = 0x01399D44;
    /// <summary>
    /// Bind <c>00B23B50</c> <c>push 1</c> / <c>00B314E0</c> so
    /// <c>00B30B50</c> arg2≠0 and <c>00B2FC50</c> runs:
    /// <c>00988350(camera+128)</c> view, <c>00988540(camera+372)</c>
    /// proj, <c>009881F0</c> identity world. Pre-pass
    /// <c>00B27994 push 0</c> skips it. Sky
    /// <c>00B66A01</c> calls <c>00B2FC50</c> on
    /// <c>[0x1436EA0]</c> again before landscape.
    /// </summary>
    public const uint ViewSourceOffset = 128;
    /// <summary>
    /// <c>00B30B50</c> copies helper+16 (12 floats) to
    /// camera+128, writes translation <c>-R*pos</c> at
    /// +164/+168/+172, then <c>rep movsd</c> that 3x4 to
    /// <c>+276</c> before scaling +128 rows 0/1 by
    /// cotH/cotV. Fog <c>00B47630</c> reads +276 (unscaled).
    /// WVP <c>00988350</c> reads the cot-scaled +128.
    /// </summary>
    public const int ViewUnscaledCopyOffset = 276;
    public const bool FirstSeenFogUsesUnscaledView = true;
    public const bool FirstSeenView128IsCotScaled = true;
    public const uint ViewBuilder = 0x00B30B50;
    public const uint BindCameraUpdate = 0x00B23B50;
    public const int BindCameraUpdateArg = 1;
    public const int PrePassCameraUpdateArg = 0;
    public const uint ExtractOtherWritesView = 0x00B2FC50;
    public const uint ExtractOtherSkyCaller = 0x00B66A07;
    public const uint CameraObject = 0x1436EA0;
    /// <summary>
    /// Per-cell <c>00BF46A2</c> builds a 3x4 at <c>esp+144</c>:
    /// diagonal 1, then <c>[0x1436EA0]+84/+88/+92</c> into
    /// +36/+40/+44, and <c>009881F0</c> copies it to wrapper+496.
    /// After the +20 source copy those slots are helper pos.
    /// </summary>
    public const int CameraWorldXOffset = 84;
    public const int CameraWorldYOffset = 88;
    public const int CameraWorldZOffset = 92;
    public const int PerCellWorldStack = 144;
    public const uint PerCellWorldFill = 0x00BF46A2;
    public const bool FirstSeenLandscapeWorldIsCameraTranslation = true;
    public const bool FirstSeenBindWritesViewFromCamera128 = true;
    /// <summary>
    /// VS <c>dp4 oPos, pos, c5–c8</c> with <c>00988A50</c>
    /// <c>SetVSConstantF(+752)</c>. Wrapper product is world+496
    /// then view+560 then proj+624. Numerics/GPU consume that as
    /// row-vector <c>p * W * V * P</c>.
    /// </summary>
    public const bool FirstSeenWvpIsWorldViewProj = true;
    /// <summary>
    /// First-seen FG / static / PALSKIN / inner-sky VS only
    /// <c>dp4 oPos</c> against <c>c5–c8</c>. Those four
    /// registers are the <c>00988A50</c> product
    /// (<c>SetVSConstantF</c> at wrapper+752). No first-seen
    /// VS reads a separate world / view / proj bank
    /// (registers 9–16 are unused on those programs).
    /// Combined <see cref="ComposeWvp"/> is that product.
    /// </summary>
    public const bool FirstSeenVsReadsSeparateWvp = false;
    public const int WvpStartRegister = 5;
    public const int WvpRegisterCount = 4;
    public const int WvpUploadOffset = 752;
    /// <summary>
    /// <c>009883F0</c> writes <c>M11=M22=1</c>, Z from helper
    /// near/far/minZ/maxZ, <c>M43=1</c>, <c>M44=0</c>. Cot lives
    /// on camera+128, not in proj.
    /// </summary>
    public const bool FirstSeenProjXyIsIdentity = true;
    public const float VulkanNdcYSign = -1f;

    public readonly record struct Plane(Vector3 Normal, float D);

    /// <summary>
    /// <c>00B314E0</c>: <c>00A0BE90</c> <c>[helper+44]</c> times
    /// <c>360 * 1/360 * 2π</c> (<c>0x1238020</c> / <c>0x1238E00</c> /
    /// <c>0x128F608</c>). TNG spline FOV <c>0.2</c> is turns.
    /// </summary>
    public static float TurnsToRadians(float turns) =>
        turns * FovTurnsToDegrees * Inv360 * TwoPi;

    /// <summary>
    /// <c>00B30B50</c> +84 set: <c>1/tan(fov*0.5)</c> from source+76 / +80.
    /// </summary>
    public static float CotHalfAngle(float radians) =>
        1f / MathF.Tan(radians * FovHalfScale);

    /// <summary>
    /// <c>009883F0</c> / <c>00B3106C</c>:
    /// <c>Q = ((minZ-maxZ)*near*far)/(far-near)</c> is <c>M34</c>
    /// (+220 / wrapper+668). <c>M33 = minZ - Q/near</c> (+224 /
    /// wrapper+664). <c>FDIVR</c> then <c>FSUBR minZ</c>.
    /// </summary>
    public static void ViewportZTerms(
        float near, float far, float minZ, float maxZ,
        out float m33, out float m34)
    {
        m34 = ((minZ - maxZ) * near * far) / (far - near);
        m33 = minZ - m34 / near;
    }

    /// <summary>
    /// <c>00988350</c>: 3x4 at camera+128 (column stride 12) into
    /// wrapper+560 as rows <c>(c0,c1,c2,c3)</c> and bottom
    /// <c>(0,0,0,1)</c>.
    /// </summary>
    public static Matrix4x4 View4x4From3x4(
        Vector3 row0, Vector3 row1, Vector3 row2, Vector3 translation)
    {
        return new Matrix4x4(
            row0.X, row0.Y, row0.Z, translation.X,
            row1.X, row1.Y, row1.Z, translation.Y,
            row2.X, row2.Y, row2.Z, translation.Z,
            0f, 0f, 0f, 1f);
    }

    /// <summary>
    /// <c>00BF46A2</c> / <c>009881F0</c>: identity 3x4 plus
    /// camera +84/+88/+92 in the last column.
    /// </summary>
    public static void LandscapeWorld3x4(
        Vector3 cameraPos,
        out Vector3 col0, out Vector3 col1, out Vector3 col2, out Vector3 col3)
    {
        col0 = Vector3.UnitX;
        col1 = Vector3.UnitY;
        col2 = Vector3.UnitZ;
        col3 = cameraPos;
    }

    /// <summary>
    /// Numerics / GPU form of <see cref="LandscapeWorld3x4"/>:
    /// row-vector <c>p * T(cam)</c> = <c>p + cam</c>.
    /// </summary>
    public static Matrix4x4 LandscapeWorld(Vector3 cameraPos) =>
        Matrix4x4.CreateTranslation(cameraPos);

    /// <summary>
    /// Bind / static / PALSKIN world is identity
    /// (<c>009881F0</c> from <c>00B2FC50</c>).
    /// </summary>
    public static Matrix4x4 IdentityWorld() => Matrix4x4.Identity;

    /// <summary>
    /// <c>00B314E0</c> copies helper +0/+12/+24, normalizes look
    /// and up with <c>00A14440</c>, then
    /// <c>right = normalize(up × look)</c>. Look is not
    /// re-orthogonalized against up. Packed at source+16 as
    /// column-stride-12 <c>(right, look, up)</c> with translation
    /// 0; <c>00B30B50</c> overwrites translation as
    /// <c>-dot(axis, pos)</c>. Not <c>CreateLookAt</c>
    /// (<c>right, up, -forward</c>).
    /// </summary>
    public const int HelperBasisOffset = 16;
    public const uint HelperNormalize = 0x00A14440;
    public const bool FirstSeenViewUsesCreateLookAt = false;
    public const bool FirstSeenViewReorthogonalizesLook = false;

    public static void HelperViewAxes(
        Vector3 look, Vector3 up, out Vector3 right, out Vector3 lookN, out Vector3 upN)
    {
        lookN = Vector3.Normalize(look);
        upN = Vector3.Normalize(up);
        right = Vector3.Cross(upN, lookN);
        if (right.LengthSquared() < 1e-12f)
            right = Vector3.UnitX;
        else
            right = Vector3.Normalize(right);
    }

    /// <summary>
    /// Camera+128 after <c>00B30B50</c>: helper
    /// <c>(right, look, up)</c> 3x4, then rows 0/1 scaled by
    /// letterbox cots. Numerics columns are those three axes so
    /// <c>p * view</c> matches VS <c>dp4</c> against wrapper rows.
    /// </summary>
    public static Matrix4x4 CotScaledView(
        Vector3 position, Vector3 look, Vector3 up, float cotH, float cotV)
    {
        HelperViewAxes(look, up, out var right, out var lookN, out var upN);
        var view = new Matrix4x4(
            right.X, lookN.X, upN.X, 0f,
            right.Y, lookN.Y, upN.Y, 0f,
            right.Z, lookN.Z, upN.Z, 0f,
            -Vector3.Dot(right, position),
            -Vector3.Dot(lookN, position),
            -Vector3.Dot(upN, position),
            1f);
        view.M11 *= cotH; view.M21 *= cotH; view.M31 *= cotH; view.M41 *= cotH;
        view.M12 *= cotV; view.M22 *= cotV; view.M32 *= cotV; view.M42 *= cotV;
        return view;
    }

    /// <summary>
    /// <c>009883F0</c> XY identity plus viewport Z, with
    /// <see cref="VulkanNdcYSign"/> on Y so the Vulkan NDC flip
    /// stays out of the cot-scaled view.
    /// </summary>
    public static Matrix4x4 FirstSeenProjection(float m33, float m34, float ySign)
    {
        return new Matrix4x4(
            1f, 0f, 0f, 0f,
            0f, ySign, 0f, 0f,
            0f, 0f, -m33, -1f,
            0f, 0f, m34, 0f);
    }

    /// <summary>
    /// <c>00988A50</c> / VS: <c>p * world * view * proj</c>.
    /// </summary>
    public static Matrix4x4 ComposeWvp(Matrix4x4 world, Matrix4x4 view, Matrix4x4 proj) =>
        world * view * proj;

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
    /// <c>00B54310</c> / <c>00B30B50</c> store: inverse columns of 3
    /// at +228 stride 12. Row <c>i</c> is
    /// <c>(i0[i], i1[i], i2[i], it[i])</c> and is uploaded to
    /// <c>c[2+i]</c>.
    /// </summary>
    public static void CotScaledInverse(
        Vector3 position, Vector3 look, Vector3 up, float cotH, float cotV,
        out Vector4 row0, out Vector4 row1, out Vector4 row2)
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
        row0 = new Vector4(i0.X, i1.X, i2.X, it.X);
        row1 = new Vector4(i0.Y, i1.Y, i2.Y, it.Y);
        row2 = new Vector4(i0.Z, i1.Z, i2.Z, it.Z);
    }

    /// <summary>
    /// <c>00B54310</c> <c>push 2</c> / <c>00989B00</c>: camera
    /// <c>+228/+240/+252/+264</c>.
    /// </summary>
    public static Vector4 InverseRow0(
        Vector3 position, Vector3 look, Vector3 up, float cotH, float cotV)
    {
        CotScaledInverse(position, look, up, cotH, cotV, out var row0, out _, out _);
        return row0;
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
        CotScaledInverse(position, look, up, cotH, cotV, out var row0, out var row1, out var row2);
        var i0 = new Vector3(row0.X, row1.X, row2.X);
        var i1 = new Vector3(row0.Y, row1.Y, row2.Y);
        var i2 = new Vector3(row0.Z, row1.Z, row2.Z);
        var it = new Vector3(row0.W, row1.W, row2.W);

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

    /// <summary>
    /// <c>00BF6F80</c>: <c>min=(ox+sx0, oy+sy0, 0)</c>,
    /// <c>max=(ox+sx0+w, oy+sy0+h, 0)</c>. First-seen
    /// <c>00BDC27A</c> pushes start <c>0,0</c>.
    /// </summary>
    public static void PatchAabb(
        float originX, float originY, float sizeX, float sizeY,
        out Vector3 min, out Vector3 max)
    {
        min = new Vector3(originX + FirstSeenAabbStartX, originY + FirstSeenAabbStartY, AabbZ);
        max = new Vector3(min.X + sizeX, min.Y + sizeY, AabbZ);
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
