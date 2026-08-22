using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Fable.Formats;
using Fable.Formats.Scene;
using Fable.Render.Parity.Dx9Vulkan;

namespace Fable.Render;

/// <summary>
/// Immutable, backend-neutral input for one 3D scene. This is the comparison
/// boundary: asset parsing, culling and batching happen before it; Vulkan,
/// native DX9 and diagnostic backends must consume the same packet.
/// </summary>
public sealed record SceneRenderPacket
{
    public required string SceneName { get; init; }
    public required MeshVertex[] LandscapeVertices { get; init; }
    public required MeshDraw[] LandscapeDraws { get; init; }
    public required ushort[] LandscapeIndices { get; init; }
    public required MeshVertex[] ObjectVertices { get; init; }
    public required MeshDraw[] ObjectDraws { get; init; }
    public required GpuTexture[] Textures { get; init; }
    public required Matrix4x4 ViewProjection { get; init; }
    public required Matrix4x4 LandscapeViewProjection { get; init; }
    public required Matrix4x4 SkyViewProjection { get; init; }
    public required Vector3 CameraPosition { get; init; }
    public required Vector4 FogPlane { get; init; }
    public int ViewportWidth { get; init; } = 1024;
    public int ViewportHeight { get; init; } = 768;

    public IEnumerable<MeshDraw> AllDraws => LandscapeDraws.Concat(ObjectDraws);

    public string ContentHash()
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        Append(hash, Encoding.UTF8.GetBytes(SceneName));
        AppendValue(hash, ViewProjection);
        AppendValue(hash, LandscapeViewProjection);
        AppendValue(hash, SkyViewProjection);
        AppendValue(hash, CameraPosition);
        AppendValue(hash, FogPlane);
        Span<byte> viewport = stackalloc byte[8];
        BitConverter.TryWriteBytes(viewport, ViewportWidth);
        BitConverter.TryWriteBytes(viewport[4..], ViewportHeight);
        Append(hash, viewport);
        Append(hash, MemoryMarshal.AsBytes(LandscapeVertices.AsSpan()));
        Append(hash, MemoryMarshal.AsBytes(LandscapeDraws.AsSpan()));
        Append(hash, MemoryMarshal.AsBytes(LandscapeIndices.AsSpan()));
        Append(hash, MemoryMarshal.AsBytes(ObjectVertices.AsSpan()));
        Append(hash, MemoryMarshal.AsBytes(ObjectDraws.AsSpan()));
        Span<byte> header = stackalloc byte[12];
        foreach (var texture in Textures.OrderBy(t => t.Id))
        {
            BitConverter.TryWriteBytes(header, texture.Id);
            BitConverter.TryWriteBytes(header[4..], texture.Width);
            BitConverter.TryWriteBytes(header[8..], texture.Height);
            Append(hash, header);
            Append(hash, texture.Rgba);
        }
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static void Append(IncrementalHash hash, ReadOnlySpan<byte> bytes) =>
        hash.AppendData(bytes);

    private static void AppendValue<T>(IncrementalHash hash, T value) where T : struct
    {
        ReadOnlySpan<T> values = MemoryMarshal.CreateReadOnlySpan(ref value, 1);
        Append(hash, MemoryMarshal.AsBytes(values));
    }
}

/// <summary>Common visual-test surface for Vulkan, DX9 and trace backends.</summary>
public interface ISceneRenderBackend : IDisposable
{
    string Name { get; }
    void Load(SceneRenderPacket packet);
    void Render();
}

/// <summary>Vulkan implementation of the common scene-slice boundary.</summary>
public sealed class VulkanSceneRenderBackend(VulkanLineRenderer renderer) : ISceneRenderBackend
{
    private SceneRenderPacket? _packet;

    public string Name => "vulkan";

    public void Load(SceneRenderPacket packet)
    {
        _packet = packet;
        renderer.SetTextures(packet.Textures);
        renderer.SetMesh(packet.LandscapeVertices, packet.LandscapeDraws, packet.LandscapeIndices);
        renderer.SetObjects(packet.ObjectVertices, packet.ObjectDraws);
    }

    public void Render()
    {
        if (_packet is not { } packet)
            throw new InvalidOperationException("No scene packet loaded.");
        renderer.Draw(
            packet.ViewProjection,
            packet.CameraPosition,
            packet.FogPlane,
            packet.SkyViewProjection,
            packet.LandscapeViewProjection);
    }

    public void Dispose()
    {
        // The window host owns the Vulkan renderer lifetime.
    }
}

/// <summary>
/// Grep-first reference backend. It records the complete backend-neutral draw
/// contract without translating it to Vulkan. A native DX9 implementation can
/// implement <see cref="ISceneRenderBackend"/> beside this and consume the
/// exact same packet.
/// </summary>
public sealed class Dx9SceneContractBackend(string outputPath) : ISceneRenderBackend
{
    private SceneRenderPacket? _packet;

    public string Name => "dx9-contract";

    public void Load(SceneRenderPacket packet) => _packet = packet;

    public void Render()
    {
        if (_packet is not { } packet)
            throw new InvalidOperationException("No scene packet loaded.");
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);
        using var writer = new StreamWriter(outputPath, false, new UTF8Encoding(false));
        writer.WriteLine($"SCENE\tname={Atom(packet.SceneName)}\tbackend=dx9-contract\thash={packet.ContentHash()}\tviewport={packet.ViewportWidth}x{packet.ViewportHeight}");
        WriteMatrix(writer, "view_projection_dx9", packet.ViewProjection);
        WriteMatrix(writer, "landscape_view_projection_dx9", packet.LandscapeViewProjection);
        WriteMatrix(writer, "sky_view_projection_dx9", packet.SkyViewProjection);
        writer.WriteLine($"CAMERA\tposition={Vec(packet.CameraPosition)}\tfog_plane={Vec(packet.FogPlane)}");
        writer.WriteLine($"STATE\tapi=dx9\tname=D3DRS_CULLMODE\tvalue={D3dDeviceState.CullCcw}\tmeaning=D3DCULL_CCW\tstatus=recovered");
        writer.WriteLine($"STATE\tapi=dx9\tname=D3DRS_ZFUNC\tvalue={D3dDeviceState.FirstSeenZFunc}\tmeaning=D3DCMP_LESSEQUAL\tstatus=recovered");
        writer.WriteLine($"STATE\tapi=dx9\tname=D3DRS_ZENABLE\tvalue={D3dDeviceState.FirstSeenZEnable}\tmeaning=TRUE\tstatus=default_unread_write");
        writer.WriteLine($"STATE\tapi=dx9\tname=D3DRS_ZWRITEENABLE\tvalue={D3dDeviceState.FirstSeenZWriteEnable}\tmeaning=TRUE\tstatus=default_unread_write");
        writer.WriteLine($"STATE\tapi=dx9\tname=D3DRS_FOGENABLE\tvalue={D3dDeviceState.FirstSeenFogEnable}\tmeaning=TRUE\tstatus=recovered");
        writer.WriteLine($"STATE\tapi=dx9\tname=D3DRS_FOGCOLOR\tvalue=0x{D3dDeviceState.FirstSeenFogColorArgb:X8}\tmeaning=ARGB_BLACK\tstatus=recovered");
        writer.WriteLine("STATE\tapi=dx9\tname=SAMPLER0_MAG_MIN_MIP\tvalue=POINT,POINT,NONE\tmeaning=D3D9_DEFAULT\tstatus=first_scene_explicit_write_unread");
        writer.WriteLine("STATE\tapi=dx9\tname=SAMPLER0_ADDRESS_UVW\tvalue=WRAP,WRAP,WRAP\tmeaning=D3D9_DEFAULT\tstatus=first_scene_explicit_write_unread");
        writer.WriteLine($"CONSTANT\tapi=dx9\tstage=vs\tregister=c2\tvalue={Vec(packet.FogPlane)}\tmeaning=linear_fog_plane\tstatus=recovered");
        writer.WriteLine($"CONSTANT\tapi=dx9\tstage=vs\tregister=c3\tvalue={Vec(WorldShading.FirstSeenC3)}\tmeaning=lighting_table_leftover\tstatus=first_seen_only");
        writer.WriteLine($"CONSTANT\tapi=dx9\tstage=vs\tregister=c19\tvalue={Vec(WorldShading.DirLightDirection)}\tmeaning=directional_light_direction\tstatus=first_seen_only");
        writer.WriteLine($"CONSTANT\tapi=dx9\tstage=vs\tregister=c20\tvalue={Vec(WorldShading.DirLightColor)}\tmeaning=directional_light_colour\tstatus=first_seen_only");
        writer.WriteLine($"CONSTANT\tapi=dx9\tstage=vs\tregister=c35\tvalue={Vec(WorldShading.LitColor)}\tmeaning=light_scene_add\tstatus=first_seen_only");
        writer.WriteLine($"STREAM\tkind=landscape\tvertices={packet.LandscapeVertices.Length}\tindices={packet.LandscapeIndices.Length}\tdraws={packet.LandscapeDraws.Length}\tstride={MeshVertex.Stride}");
        writer.WriteLine($"STREAM\tkind=objects\tvertices={packet.ObjectVertices.Length}\tindices=0\tdraws={packet.ObjectDraws.Length}\tstride={MeshVertex.Stride}");
        var ordinal = 0;
        foreach (var draw in packet.LandscapeDraws)
            WriteDraw(writer, ordinal++, "landscape", draw);
        foreach (var draw in packet.ObjectDraws)
            WriteDraw(writer, ordinal++, "objects", draw);
        foreach (var texture in packet.Textures.OrderBy(t => t.Id))
            writer.WriteLine($"TEXTURE\tid={texture.Id}\twidth={texture.Width}\theight={texture.Height}\tbytes={texture.Rgba.Length}\tformat=A8R8G8B8_semantic\tsha256={Convert.ToHexString(SHA256.HashData(texture.Rgba)).ToLowerInvariant()}");
    }

    public void Dispose()
    {
    }

    private static void WriteDraw(TextWriter writer, int ordinal, string stream, MeshDraw draw) =>
        writer.WriteLine($"DRAW\tordinal={ordinal}\tstream={stream}\tpass=0x{draw.PassBit:X8}\ttexture0={draw.TextureId}\ttexture1={draw.TextureId1}\tfirst_vertex={draw.FirstVertex}\tvertex_count={draw.VertexCount}\tfirst_index={draw.FirstIndex}\tindex_count={draw.IndexCount}\tindexed={draw.Indexed}\tshader_mode={draw.ShaderMode:R}\talpha_blend={draw.SrcAlphaBlend}\tworld={Matrix(draw.WorldOrIdentity)}");

    private static void WriteMatrix(TextWriter writer, string name, Matrix4x4 matrix) =>
        writer.WriteLine($"MATRIX\tname={name}\tvalue={Matrix(matrix)}");

    private static string Matrix(Matrix4x4 m) => string.Join(',',
        m.M11, m.M12, m.M13, m.M14, m.M21, m.M22, m.M23, m.M24,
        m.M31, m.M32, m.M33, m.M34, m.M41, m.M42, m.M43, m.M44);

    private static string Vec(Vector3 v) => $"{v.X:R},{v.Y:R},{v.Z:R}";
    private static string Vec(Vector4 v) => $"{v.X:R},{v.Y:R},{v.Z:R},{v.W:R}";
    private static string Atom(string value) => value.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
}

/// <summary>
/// Textual description of the Vulkan translation actually used by the scene
/// renderer. Kept separate from the native DX9 contract so unknown native
/// writes cannot silently become claimed parity.
/// </summary>
public sealed class VulkanSceneContractBackend(string outputPath) : ISceneRenderBackend
{
    private SceneRenderPacket? _packet;

    public string Name => "vulkan-contract";
    public void Load(SceneRenderPacket packet) => _packet = packet;

    public void Render()
    {
        if (_packet is not { } packet)
            throw new InvalidOperationException("No scene packet loaded.");
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);
        using var writer = new StreamWriter(outputPath, false, new UTF8Encoding(false));
        writer.WriteLine($"SCENE\tname={Atom(packet.SceneName)}\tbackend=vulkan-contract\thash={packet.ContentHash()}\tviewport={packet.ViewportWidth}x{packet.ViewportHeight}");
        writer.WriteLine("IMAGE\tformat=VK_FORMAT_R8G8B8A8_UNORM\tmips=1\tcolour_space=linear_numeric");
        writer.WriteLine($"STATE\tapi=vulkan\tname=frontFace\tvalue={Dx9VulkanRasterState.FirstSeenFrontFace}\tdx9=D3DCULL_CCW\tstatus=mapped");
        writer.WriteLine($"STATE\tapi=vulkan\tname=cullMode\tvalue={Dx9VulkanRasterState.FirstSeenCullMode}\tdx9=D3DCULL_CCW\tstatus=mapped");
        writer.WriteLine($"STATE\tapi=vulkan\tname=depthCompareOp\tvalue={Dx9VulkanDepth.FirstSeenCompareOp}\tdx9=D3DCMP_LESSEQUAL\tstatus=mapped");
        writer.WriteLine($"STATE\tapi=vulkan\tname=depthTestEnable\tvalue={Dx9VulkanDepth.FirstSeenDepthTest}\tdx9=D3DRS_ZENABLE\tstatus=temporary_native_write_unread");
        writer.WriteLine($"STATE\tapi=vulkan\tname=depthWriteEnable\tvalue={Dx9VulkanDepth.FirstSeenDepthWrite}\tdx9=D3DRS_ZWRITEENABLE\tstatus=temporary_native_write_unread");
        writer.WriteLine($"SAMPLER\tmag={Dx9VulkanSamplerState.MagFilter}\tmin={Dx9VulkanSamplerState.MinFilter}\tmip={Dx9VulkanSamplerState.MipMode}\tu={Dx9VulkanSamplerState.AddressU}\tv={Dx9VulkanSamplerState.AddressV}\tw={Dx9VulkanSamplerState.AddressW}\tmax_lod={Dx9VulkanSamplerState.MaxLod:R}\tstatus=temporary_native_write_unread");
        writer.WriteLine($"PUSH\tname=fog_plane\tvalue={Vec(packet.FogPlane)}\tdx9=vs_c2\tstatus=mapped");
        writer.WriteLine($"SHADER_LITERAL\tname=c3\tvalue={Vec(WorldShading.FirstSeenC3)}\tdx9=vs_c3\tstatus=first_seen_only");
        writer.WriteLine($"PUSH\tname=light_direction\tvalue={Vec(WorldShading.DirLightDirection)}\tdx9=vs_c19\tstatus=first_seen_only");
        writer.WriteLine($"PUSH\tname=light_colour\tvalue={Vec(WorldShading.DirLightColor)}\tdx9=vs_c20\tstatus=first_seen_only");
        writer.WriteLine($"PUSH\tname=light_scene_add\tvalue={Vec(WorldShading.LitColor)}\tdx9=vs_c35\tstatus=first_seen_only");
        writer.WriteLine("GAP\tname=dynamic_environment_constants\tdx9=per_frame_constant_table\tvulkan=first_seen_literals\tstatus=not_implemented");
        writer.WriteLine("GAP\tname=native_shader_identity\tdx9=shader_bank_program_per_draw\tvulkan=shader_mode_branch\tstatus=partially_classified");

        var ordinal = 0;
        foreach (var draw in packet.LandscapeDraws)
            WriteDraw(writer, ordinal++, "landscape", draw);
        foreach (var draw in packet.ObjectDraws)
            WriteDraw(writer, ordinal++, "objects", draw);
    }

    public void Dispose() { }

    private static void WriteDraw(TextWriter writer, int ordinal, string stream, MeshDraw draw)
    {
        var pipeline = draw.SrcAlphaBlend ? "mesh-alpha" : "mesh-opaque";
        var src = draw.SrcAlphaBlend ? Dx9VulkanBlendState.FirstSeenPalskinSrc.ToString() : "One";
        var dst = draw.SrcAlphaBlend ? Dx9VulkanBlendState.FirstSeenPalskinDst.ToString() : "Zero";
        writer.WriteLine($"DRAW\tordinal={ordinal}\tstream={stream}\tpass=0x{draw.PassBit:X8}\tpipeline={pipeline}\tshader_mode={draw.ShaderMode:R}\ttexture0={draw.TextureId}\ttexture1={draw.TextureId1}\tindexed={draw.Indexed}\tfirst_vertex={draw.FirstVertex}\tvertex_count={draw.VertexCount}\tfirst_index={draw.FirstIndex}\tindex_count={draw.IndexCount}\tblend_src={src}\tblend_dst={dst}\tblend_op=Add");
    }

    private static string Vec(Vector4 v) => $"{v.X:R},{v.Y:R},{v.Z:R},{v.W:R}";
    private static string Atom(string value) => value.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
}
