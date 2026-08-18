using Fable.Game;

namespace Fable.Formats.Tests;

public sealed class EngineFrameTests
{
    [Fact]
    public void EngineFrame_constructs_for_host_present()
    {
        var camera = new ScriptedCamera();
        var frame = new EngineFrame(
            camera,
            World: null,
            AviWidth: 640,
            AviHeight: 480,
            AviRgba: null,
            AviSerial: 3,
            AviPlaying: false,
            FadeAlpha: 16,
            FadeR: 1,
            FadeG: 2,
            FadeB: 3);

        Assert.Same(camera, frame.Camera);
        Assert.Null(frame.World);
        Assert.Equal(640, frame.AviWidth);
        Assert.Equal(480, frame.AviHeight);
        Assert.Null(frame.AviRgba);
        Assert.Equal(3, frame.AviSerial);
        Assert.False(frame.AviPlaying);
        Assert.Equal(16, frame.FadeAlpha);
        Assert.Equal(1, frame.FadeR);
        Assert.Equal(2, frame.FadeG);
        Assert.Equal(3, frame.FadeB);
        Assert.Null(frame.Textures);
    }

    [Fact]
    public void Unexpanded_world_is_not_a_geometry_submit()
    {
        var world = new WorldGeometry
        {
            Region = "TEST",
            Regions = ["TEST"],
            Triangles = [],
            MeshInstances = 0,
            MissingMeshes = 0,
            Expanded = false,
        };
        var frame = new EngineFrame(
            new ScriptedCamera(),
            world,
            0, 0, null, 0, false, 0, 0, 0, 0);

        Assert.False(frame.World!.Expanded);
        Assert.Empty(frame.World.Triangles);
    }

    [Fact]
    public void BuildFrame_reuses_texture_array()
    {
        var life = new EngineLifecycle();
        var first = life.BuildFrame().Textures;
        var second = life.BuildFrame().Textures;
        Assert.Same(first, second);
    }
}
