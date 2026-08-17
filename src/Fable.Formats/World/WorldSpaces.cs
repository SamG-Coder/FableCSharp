using System.Numerics;
using Fable.Formats.Levels;

namespace Fable.Formats.World;

/// <summary>
/// Named first-scene coordinate spaces. No conversion is implicit:
/// every step is a function with units and an evidence status.
/// Locked findings stay locked — see
/// <c>docs/render/WORLD_SPACE_CONTRACT.md</c>.
/// </summary>
public static class WorldSpaces
{
    public const float C3dCentimetresToMetres = 0.01f;
    public const float RegionExtentMetres = 128f;

    /// <summary>
    /// STB file XY are WLD/global ushorts. TNG and the SHOT2 helper
    /// are region-local metres. The conversion that places both in
    /// one space is <c>STB − (MapX, MapY)</c>. Native camera is the
    /// TNG helper pos (local). Native landscape VB is then
    /// camera-relative in that same local space.
    /// </summary>
    public static Vector3 StbFileToRegionLocal(float worldX, float worldY, float z, float mapX, float mapY) =>
        new(worldX - mapX, worldY - mapY, z);

    public static Vector3 StbFileToRegionLocal(LevTileVertex vertex, float mapX, float mapY) =>
        StbFileToRegionLocal(vertex.WorldX, vertex.WorldY, vertex.Z, mapX, mapY);

    public static Vector2 WldToRegionLocal(float worldX, float worldY, float mapX, float mapY) =>
        new(worldX - mapX, worldY - mapY);

    public static Vector2 RegionLocalToWld(float localX, float localY, float mapX, float mapY) =>
        new(localX + mapX, localY + mapY);

    /// <summary>
    /// Neighbour maps are placed in the primary region's local frame
    /// by the WLD MapX/MapY delta. That is the same subtract used
    /// for STB verts, applied to the neighbour origin.
    /// </summary>
    public static Vector2 NeighbourRegionOffset(float neighbourMapX, float neighbourMapY, float primaryMapX, float primaryMapY) =>
        new(neighbourMapX - primaryMapX, neighbourMapY - primaryMapY);

    /// <summary>
    /// Fable landscape expand is camera-relative in region-local
    /// metres: <c>p_camrel = p_local − cam</c>. File verts stay
    /// WLD; the host applies <see cref="StbFileToRegionLocal"/>
    /// first so this subtraction matches native.
    /// </summary>
    public static Vector3 RegionLocalToCameraRelative(Vector3 regionLocal, Vector3 cameraPos) =>
        regionLocal - cameraPos;

    /// <summary>
    /// <c>00BF46A2</c> <c>T(cam)</c> on a camera-relative VB:
    /// <c>p_camrel + cam = p_local</c>.
    /// </summary>
    public static Vector3 CameraRelativeToRegionLocal(Vector3 cameraRelative, Vector3 cameraPos) =>
        cameraRelative + cameraPos;

    public static Vector3 C3dLocalToMetres(Vector3 centimetres) =>
        centimetres * C3dCentimetresToMetres;

    /// <summary>
    /// Native landscape clip: file → region-local → cam-relative →
    /// <c>T(cam)</c> → V → P.
    /// </summary>
    public static Vector4 NativeLandscapeClip(
        Vector3 stbFileXyz, float mapX, float mapY, Vector3 cameraPos,
        Matrix4x4 view, Matrix4x4 proj)
    {
        var local = StbFileToRegionLocal(stbFileXyz.X, stbFileXyz.Y, stbFileXyz.Z, mapX, mapY);
        var camRel = RegionLocalToCameraRelative(local, cameraPos);
        return Clip(camRel, LandscapeFrustum.LandscapeWorld(cameraPos), view, proj);
    }

    /// <summary>
    /// Host landscape clip: file → region-local (world) → identity W → V → P.
    /// </summary>
    public static Vector4 HostLandscapeClip(
        Vector3 stbFileXyz, float mapX, float mapY,
        Matrix4x4 view, Matrix4x4 proj)
    {
        var local = StbFileToRegionLocal(stbFileXyz.X, stbFileXyz.Y, stbFileXyz.Z, mapX, mapY);
        return Clip(local, LandscapeFrustum.HostWorldSpaceLandscapeWorld(), view, proj);
    }

    public static Vector4 Clip(Vector3 position, Matrix4x4 world, Matrix4x4 view, Matrix4x4 proj) =>
        Vector4.Transform(new Vector4(position, 1f), LandscapeFrustum.ComposeWvp(world, view, proj));

    public static Vector4 ToNdc(Vector4 clip)
    {
        if (MathF.Abs(clip.W) < 1e-8f)
            return clip;
        return new Vector4(clip.X / clip.W, clip.Y / clip.W, clip.Z / clip.W, clip.W);
    }

    public static bool NearlyEqual(Vector4 a, Vector4 b, float eps = 1e-3f) =>
        MathF.Abs(a.X - b.X) <= eps &&
        MathF.Abs(a.Y - b.Y) <= eps &&
        MathF.Abs(a.Z - b.Z) <= eps &&
        MathF.Abs(a.W - b.W) <= eps;

    public static float DistanceXy(Vector3 a, Vector3 b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    public static IReadOnlyList<WorldSpaceDef> Catalog() =>
    [
        new("graphics.big C3D local", "centimetres", "right",
            "X right, Y forward, Z up", "mesh origin", "relative to mesh",
            "× 0.01 → C3D metres", "MeshFile bounds; WorldGeometry.MeshToWorld",
            WorldSpaceStatus.Proven),
        new("C3D units / centimetres", "centimetres", "right",
            "same as C3D local", "mesh origin", "relative",
            "× 0.01 → TNG metres", "FIRST_SCENE_CONTRACT GEOMETRY",
            WorldSpaceStatus.Proven),
        new("TNG object local transform", "metres", "right",
            "RHSetForward / RHSetUp, right = forward × up",
            "CTCPhysicsStandard position", "object-local then world translation",
            "ObjectTransform → region-local world",
            "WorldGeometry.ObjectTransform; CreateWorld DISPROVEN",
            WorldSpaceStatus.Proven),
        new("region-local coordinates", "metres", "right",
            "X east, Y north, Z up", "WLD MapX/MapY of the current map",
            "absolute within the region (Lookout/Oakvale 0..128 typical)",
            "neighbour offset = ΔMapX/ΔMapY; camera is already here",
            "TNG XY 0–128; SHOT2 helper pos; WorldGeometryTests",
            WorldSpaceStatus.Proven),
        new("WLD/global map coordinates", "metres", "right",
            "same axes as region-local", "WLD world origin",
            "absolute overworld",
            "local = WLD − (MapX, MapY)",
            "StartOakValeWest MapX=3456 MapY=736; Lookout 3232/3488",
            WorldSpaceStatus.Proven),
        new("STB file coordinates", "metres (ushort XY, float Z)", "right",
            "X/Y WLD, Z up metres", "WLD origin",
            "absolute WLD",
            "StbFileToRegionLocal = XY − (MapX, MapY)",
            "LevTileVertex.WorldX/Y; Lookout 3232+; Oakvale 3456+",
            WorldSpaceStatus.Proven),
        new("expanded Fable landscape VB coordinates", "metres", "right",
            "same as region-local after convert", "camera position",
            "camera-relative (native GPU VB)",
            "p_camrel = p_local − cam",
            "00BFE050 expand; FirstSeenLandscapeDeviceVbIsCameraRelative",
            WorldSpaceStatus.Proven),
        new("camera-relative landscape coordinates", "metres", "right",
            "same axes", "camera", "relative",
            "T(cam) 00BF46A2 → region-local world",
            "LandscapeFrustum.LandscapeWorld; HostTcamOnWorldSpaceLandscapeIsDisproven",
            WorldSpaceStatus.Proven),
        new("camera/world coordinates", "metres", "right",
            "Z-up; look on view Z; right = up × look",
            "SHOT2 helper +0/+12/+24", "absolute region-local",
            "CotScaledView → view space",
            "00B314E0 / 00B30B50; FirstSeenViewUsesCreateLookAt=false",
            WorldSpaceStatus.Proven),
        new("static-object world coordinates", "metres", "right",
            "same as region-local", "region origin", "absolute region-local",
            "identity W → view",
            "009881F0 identity world for static/PALSKIN",
            WorldSpaceStatus.Proven),
        new("skinned-character coordinates", "metres after 0.01 + palette", "right",
            "C3D local then palette dest[group[a0/3]]",
            "bind-pose dest ≈ identity first-seen", "object then world",
            "SkinPosition → ObjectTransform → region-local",
            "file byte = VS register offset, not mesh bone id",
            WorldSpaceStatus.Proven),
        new("view space", "metres, cot-scaled XY", "right",
            "X right, Y camera-up, Z look", "camera", "relative",
            "009883F0 P: clip.xy=view.xy, clip.w=view.z",
            "CotScaledView; FirstSeenViewLookIsZ; FirstSeenProjWIsViewZ",
            WorldSpaceStatus.Proven),
        new("clip space", "homogeneous", "DX9 Y-up clip",
            "clip.xy from view; clip.z = m33*view.z+Q; clip.w=view.z",
            "clip origin", "homogeneous",
            "Dx9VulkanProjection.ToVulkanWvp Y flip → Vulkan NDC",
            "009883F0; Y flip is NOT Fable P (DISPROVEN bake)",
            WorldSpaceStatus.Proven),
        new("Vulkan NDC", "NDC −1..1, Y down", "Vulkan",
            "X right, Y down, Z 0..1 after viewport",
            "NDC origin", "clip / clip.w after Y flip",
            "framebuffer",
            "Dx9VulkanProjection.NdcYSign=-1; EQUIVALENT translation",
            WorldSpaceStatus.Equivalent),
    ];
}

/// <summary>
/// Status of one recovered conversion. Matches the parity matrix
/// vocabulary: PROVEN / EQUIVALENT / UNREAD / DISPROVEN / TEMPORARY.
/// </summary>
public enum WorldSpaceStatus
{
    Proven,
    Equivalent,
    Unread,
    Disproven,
    Temporary,
}

public readonly record struct WorldSpaceDef(
    string Name,
    string Units,
    string Handedness,
    string Axes,
    string Origin,
    string AbsoluteOrRelative,
    string NextConversion,
    string Evidence,
    WorldSpaceStatus Status);
