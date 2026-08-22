using System.Numerics;
using Fable.Render;

namespace Fable.Formats.Tests;

public sealed class SceneRenderBackendTests
{
    [Fact]
    public void Content_hash_includes_camera_and_render_payload()
    {
        var packet = Packet();
        Assert.Equal(packet.ContentHash(), packet.ContentHash());
        Assert.NotEqual(packet.ContentHash(), (packet with
        {
            CameraPosition = packet.CameraPosition + Vector3.UnitX,
        }).ContentHash());
        Assert.NotEqual(packet.ContentHash(), (packet with
        {
            Textures = [new GpuTexture(7, 1, 1, [1, 2, 4, 255])],
        }).ContentHash());
    }

    [Fact]
    public void Capture_and_dx9_contract_are_line_oriented_and_grep_friendly()
    {
        var root = Path.Combine(Path.GetTempPath(), "fable-scene-slice-" + Guid.NewGuid().ToString("N"));
        try
        {
            var packet = Packet();
            SceneRenderCapture.Write(root, packet);
            var manifest = File.ReadAllText(Path.Combine(root, "scene-render-grep.txt"));
            Assert.Contains("CAPTURE\tversion=1", manifest);
            Assert.Contains("CAMERA\tposition=1,2,3", manifest);
            Assert.Contains("PASS\tbit=0x00000040", manifest);

            var contract = Path.Combine(root, "dx9-render-grep.txt");
            using (var backend = new Dx9SceneContractBackend(contract))
            {
                backend.Load(packet);
                backend.Render();
            }

            var dx9 = File.ReadAllText(contract);
            Assert.Contains("SCENE\tname=test\tbackend=dx9-contract", dx9);
            Assert.Contains("DRAW\tordinal=0\tstream=landscape\tpass=0x00000040", dx9);
            Assert.Contains("TEXTURE\tid=7\twidth=1\theight=1", dx9);

            var vulkanContract = Path.Combine(root, "vulkan-render-grep.txt");
            using (var backend = new VulkanSceneContractBackend(vulkanContract))
            {
                backend.Load(packet);
                backend.Render();
            }

            var vulkan = File.ReadAllText(vulkanContract);
            Assert.Contains("backend=vulkan-contract", vulkan);
            Assert.Contains("dx9=D3DCMP_LESSEQUAL\tstatus=mapped", vulkan);
            Assert.Contains("GAP\tname=dynamic_environment_constants", vulkan);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }

    private static SceneRenderPacket Packet() => new()
    {
        SceneName = "test",
        LandscapeVertices =
        [
            new MeshVertex(Vector3.Zero, Vector3.UnitZ, Vector2.Zero, Vector4.One),
            new MeshVertex(Vector3.UnitX, Vector3.UnitZ, Vector2.UnitX, Vector4.One),
            new MeshVertex(Vector3.UnitY, Vector3.UnitZ, Vector2.UnitY, Vector4.One),
        ],
        LandscapeDraws = [new MeshDraw(7, 0, 3, PassBit: 0x40, FirstIndex: 0, IndexCount: 3)],
        LandscapeIndices = [0, 1, 2],
        ObjectVertices = [],
        ObjectDraws = [],
        Textures = [new GpuTexture(7, 1, 1, [1, 2, 3, 255])],
        ViewProjection = Matrix4x4.Identity,
        LandscapeViewProjection = Matrix4x4.Identity,
        SkyViewProjection = Matrix4x4.Identity,
        CameraPosition = new Vector3(1, 2, 3),
        FogPlane = new Vector4(0, 0, 1, -3),
        ViewportWidth = 320,
        ViewportHeight = 240,
    };
}
