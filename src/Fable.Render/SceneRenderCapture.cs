using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Fable.Render;

/// <summary>
/// Stable scene-slice artifact. Large immutable payloads are binary; the
/// manifest is deliberately line-oriented and grep-friendly for parity work.
/// </summary>
public static class SceneRenderCapture
{
    public const int FormatVersion = 1;

    public static void Write(string directory, SceneRenderPacket packet)
    {
        Directory.CreateDirectory(directory);
        WriteStructs(Path.Combine(directory, "landscape.vertices.bin"), packet.LandscapeVertices);
        WriteStructs(Path.Combine(directory, "landscape.draws.bin"), packet.LandscapeDraws);
        WriteStructs(Path.Combine(directory, "landscape.indices.bin"), packet.LandscapeIndices);
        WriteStructs(Path.Combine(directory, "objects.vertices.bin"), packet.ObjectVertices);
        WriteStructs(Path.Combine(directory, "objects.draws.bin"), packet.ObjectDraws);

        var textureDir = Path.Combine(directory, "textures");
        Directory.CreateDirectory(textureDir);
        foreach (var texture in packet.Textures.OrderBy(t => t.Id))
            File.WriteAllBytes(Path.Combine(textureDir, $"{texture.Id}.rgba"), texture.Rgba);

        using var writer = new StreamWriter(
            Path.Combine(directory, "scene-render-grep.txt"), false, new UTF8Encoding(false));
        writer.WriteLine($"CAPTURE\tversion={FormatVersion}\tname={Atom(packet.SceneName)}\thash={packet.ContentHash()}\tviewport={packet.ViewportWidth}x{packet.ViewportHeight}");
        writer.WriteLine($"CAMERA\tposition={V(packet.CameraPosition)}\tfog_plane={V(packet.FogPlane)}");
        Matrix(writer, "view_projection_dx9", packet.ViewProjection);
        Matrix(writer, "landscape_view_projection_dx9", packet.LandscapeViewProjection);
        Matrix(writer, "sky_view_projection_dx9", packet.SkyViewProjection);
        Blob(writer, directory, "landscape.vertices.bin", packet.LandscapeVertices.Length, (int)MeshVertex.Stride);
        Blob(writer, directory, "landscape.draws.bin", packet.LandscapeDraws.Length, Marshal.SizeOf<MeshDraw>());
        Blob(writer, directory, "landscape.indices.bin", packet.LandscapeIndices.Length, sizeof(ushort));
        Blob(writer, directory, "objects.vertices.bin", packet.ObjectVertices.Length, (int)MeshVertex.Stride);
        Blob(writer, directory, "objects.draws.bin", packet.ObjectDraws.Length, Marshal.SizeOf<MeshDraw>());
        foreach (var group in packet.AllDraws.GroupBy(d => d.PassBit).OrderBy(g => Fable.Formats.Scene.ScenePasses.Rank(g.Key)))
            writer.WriteLine($"PASS\tbit=0x{group.Key:X8}\tdraws={group.Count()}\tvertices={group.Sum(d => d.VertexCount)}\tindices={group.Sum(d => d.IndexCount)}\tshader_modes={string.Join(',', group.Select(d => d.ShaderMode).Distinct())}");
        foreach (var texture in packet.Textures.OrderBy(t => t.Id))
            writer.WriteLine($"TEXTURE\tid={texture.Id}\twidth={texture.Width}\theight={texture.Height}\tbytes={texture.Rgba.Length}\tsha256={Sha(texture.Rgba)}\tfile=textures/{texture.Id}.rgba");
    }

    private static void WriteStructs<T>(string path, ReadOnlySpan<T> data) where T : struct =>
        File.WriteAllBytes(path, MemoryMarshal.AsBytes(data));

    private static void Blob(TextWriter writer, string root, string name, int count, int stride)
    {
        var bytes = File.ReadAllBytes(Path.Combine(root, name));
        writer.WriteLine($"BLOB\tname={name}\tcount={count}\tstride={stride}\tbytes={bytes.Length}\tsha256={Sha(bytes)}");
    }

    private static void Matrix(TextWriter writer, string name, Matrix4x4 m) =>
        writer.WriteLine($"MATRIX\tname={name}\tvalue={m.M11:R},{m.M12:R},{m.M13:R},{m.M14:R},{m.M21:R},{m.M22:R},{m.M23:R},{m.M24:R},{m.M31:R},{m.M32:R},{m.M33:R},{m.M34:R},{m.M41:R},{m.M42:R},{m.M43:R},{m.M44:R}");

    private static string Sha(ReadOnlySpan<byte> bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static string V(Vector3 v) => $"{v.X:R},{v.Y:R},{v.Z:R}";
    private static string V(Vector4 v) => $"{v.X:R},{v.Y:R},{v.Z:R},{v.W:R}";
    private static string Atom(string value) => value.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
}
