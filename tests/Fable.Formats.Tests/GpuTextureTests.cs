using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Fable.Core;
using Fable.Game;
using Fable.Render;
using Silk.NET.Shaderc;

namespace Fable.Formats.Tests;

/// <summary>
/// GPU texturing: UVs ride on the vertex, RGBA is uploaded as a sampled image.
/// CPU vertex colour is no longer the display path.
/// </summary>
public sealed class GpuTextureTests
{
    [Fact]
    public void Mesh_vertex_carries_uv_not_colour()
    {
        Assert.Equal(60u, MeshVertex.Stride);
        Assert.Equal(24u, MeshVertex.UvOffset);
        Assert.Equal(32u, MeshVertex.ColorOffset);
        Assert.Equal(48u, MeshVertex.ExtraOffset);
        Assert.Equal(60, Unsafe.SizeOf<MeshVertex>());
        Assert.Equal(24, (int)Marshal.OffsetOf<MeshVertex>(nameof(MeshVertex.Uv)));
        Assert.Equal(32, (int)Marshal.OffsetOf<MeshVertex>(nameof(MeshVertex.Color)));
        Assert.Equal(48, (int)Marshal.OffsetOf<MeshVertex>(nameof(MeshVertex.Extra)));
    }

    [Fact]
    public void Mesh_shaders_compile_with_a_combined_image_sampler()
    {
        var vert = GlslCompiler.Compile(LineShaders.MeshVertex, ShaderKind.VertexShader, "mesh.vert");
        var frag = GlslCompiler.Compile(LineShaders.MeshFragment, ShaderKind.FragmentShader, "mesh.frag");
        Assert.True(vert.Length > 16);
        Assert.True(frag.Length > 16);
        Assert.Contains("albedo0", LineShaders.MeshFragment, StringComparison.Ordinal);
        Assert.Contains("albedo1", LineShaders.MeshFragment, StringComparison.Ordinal);
        Assert.Contains("inColor", LineShaders.MeshVertex, StringComparison.Ordinal);
        Assert.Contains("inExtra", LineShaders.MeshVertex, StringComparison.Ordinal);
        Assert.Contains("fragExtra.yz", LineShaders.MeshFragment, StringComparison.Ordinal);
        Assert.Contains("fragUv", LineShaders.MeshFragment, StringComparison.Ordinal);
        Assert.Contains("mode < 1.5 ? fragExtra.yz", LineShaders.MeshFragment, StringComparison.Ordinal);
        Assert.Contains("* 2.0", LineShaders.MeshFragment, StringComparison.Ordinal);
        Assert.DoesNotContain("mix(t0.rgb, t1.rgb, t1.a)", LineShaders.MeshFragment, StringComparison.Ordinal);
    }

    [Fact]
    public void Lookout_batches_by_texture_and_keeps_uvs()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        var things = levels.LoadThings("LookoutPoint");
        var world = WorldGeometry.Build(install, "LookoutPoint", things.Things);
        var mesh = MeshBatches.Build(world.Triangles);

        Assert.Equal(world.Triangles.Count * 3, mesh.Vertices.Length);
        Assert.True(mesh.Draws.Length >= 4, $"draws={mesh.Draws.Length}");
        Assert.Contains(mesh.Draws, draw => draw.TextureId is 4133 or 414);
        Assert.True(mesh.Draws.Sum(draw => draw.VertexCount) >= mesh.Vertices.Length);

        var sand = world.Triangles.First(tri => tri.TextureId == 4133);
        Assert.Contains(mesh.Vertices, v => v.Uv == sand.UvA);
        Assert.Contains(mesh.Vertices, v => v.Uv.X != 0 || v.Uv.Y != 0);
    }

    [Fact]
    public void Lookout_unique_textures_decode_to_rgba()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        using var textures = new TextureLibrary(install);
        var things = levels.LoadThings("LookoutPoint");
        var world = WorldGeometry.Build(install, "LookoutPoint", things.Things);
        var mesh = MeshBatches.Build(world.Triangles);
        var files = textures.LoadMany(mesh.Draws.SelectMany(draw => new[] { draw.TextureId, draw.TextureId1 }));

        Assert.True(files.Count >= 4, $"decoded={files.Count} draws={mesh.Draws.Length}");
        Assert.Contains(files, file => file.Id is 4133 or 414);
        Assert.Null(textures.TryLoad(int.MinValue));
        Assert.All(mesh.Draws.Where(draw => draw.TextureId > 0), draw => textures.TryLoad(draw.TextureId));
        Assert.All(files, file =>
        {
            Assert.True(file.Width >= 1);
            Assert.True(file.Height >= 1);
            Assert.Equal(file.Width * file.Height * 4, file.Rgba.Length);
        });
    }

    [Fact]
    public void Cpu_vertex_colour_sample_is_not_the_gpu_path()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var textures = new TextureLibrary(install);
        var a = textures.Sample(414, new System.Numerics.Vector2(0.1f, 0.1f));
        var b = textures.Sample(414, new System.Numerics.Vector2(0.8f, 0.8f));
        Assert.True((a - b).LengthSquared() > 1e-6f, "grass should vary; GPU sampler now does this per pixel");
        Assert.DoesNotContain("fragColor * (0.22", LineShaders.MeshFragment, StringComparison.Ordinal);
    }

    [Fact]
    public void Texture_decode_is_cached()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var textures = new TextureLibrary(install);
        var first = textures.TryLoad(414);
        var decoded = textures.DecodedCount;
        var second = textures.TryLoad(414);
        Assert.Same(first, second);
        Assert.Equal(decoded, textures.DecodedCount);
        textures.LoadMany([414, 414, 4133]);
        Assert.Equal(decoded + 1, textures.DecodedCount);
    }
}
