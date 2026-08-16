using Fable.Core;
using Fable.Formats;
using Fable.Formats.Meshes;
using Fable.Formats.Scene;
using Fable.Game;
using Fable.Render;

namespace Fable.Formats.Tests;

public sealed class ScenePassTests
{
    [Fact]
    public void Registration_is_34_layers_and_walks_landscape_before_sky()
    {
        Assert.Equal(34, ScenePasses.Registration.Length);
        Assert.True(ScenePasses.Rank(0x4) < ScenePasses.Rank(0x40));
        Assert.True(ScenePasses.Rank(0x40) < ScenePasses.Rank(0x20));
        Assert.True(ScenePasses.Rank(0x20) < ScenePasses.Rank(0x2000));
        Assert.True(ScenePasses.Rank(0x2000) < ScenePasses.Rank(0x20000));
        Assert.Equal(SceneSubmit.LandscapeBit4, ScenePasses.Registration[2].Submit);
        Assert.Equal(SceneSubmit.LandscapeBit40, ScenePasses.Registration[5].Submit);
        Assert.Equal(SceneSubmit.Primitives, ScenePasses.Registration[6].Submit);
        Assert.Equal(SceneSubmit.SkyElse, ScenePasses.Registration[10].Submit);
        Assert.Equal(SceneSubmit.None, ScenePasses.Registration.First(p => p.Bit == 0x02000000).Submit);
    }

    [Fact]
    public void Lookout_draws_follow_exe_layer_bits()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        var things = levels.LoadThings("LookoutPoint");
        var world = WorldGeometry.Build(install, "LookoutPoint", things.Things);
        var mesh = MeshBatches.Build(world.Triangles);
        Assert.Contains(mesh.Draws, d => d.PassBit == 0x4);
        Assert.Contains(mesh.Draws, d => d.PassBit == 0x40);
        Assert.Contains(mesh.Draws, d => d.PassBit == 0x20);
        Assert.Contains(mesh.Draws, d => d.PassBit == 0x2000);
        var ranks = mesh.Draws.Select(d => ScenePasses.Rank(d.PassBit)).ToList();
        Assert.Equal(ranks.OrderBy(r => r), ranks);
        var firstLand = mesh.Draws.First(d => d.PassBit == 0x4);
        var fg = mesh.Draws.First(d => d.PassBit == 0x40);
        var sky = mesh.Draws.First(d => d.PassBit == 0x2000);
        Assert.Equal(0f, firstLand.ShaderMode);
        Assert.Equal(1f, fg.ShaderMode);
        Assert.Equal(2f, sky.ShaderMode);
    }

    [Fact]
    public void First_seen_cull_is_d3d_ccw()
    {
        Assert.Equal(22, D3dDeviceState.CullMode);
        Assert.Equal(3, D3dDeviceState.CullCcw);
        Assert.Equal(1, D3dDeviceState.CullNone);
        Assert.Equal(2, D3dDeviceState.CullCw);
        Assert.False(WorldShading.FirstSeenAppliesCullNoneFromFlag1);
        Assert.False(WorldShading.FirstSeenPlaysAnim);
        Assert.Equal(20, D3dDeviceState.PrimitiveTypeNoneDraw);
        Assert.True(D3dDeviceState.PrimitiveTypeUsesNoneDraw(20));
        Assert.False(D3dDeviceState.PrimitiveTypeUsesNoneDraw(7));
        Assert.False(WorldShading.FirstSeenFlag1WritesLayerType20);
        Assert.Equal(19, D3dDeviceState.SrcBlend);
        Assert.Equal(20, D3dDeviceState.DestBlend);
        Assert.Equal(27, D3dDeviceState.AlphaBlendEnable);
        Assert.Equal(5, D3dDeviceState.FirstSeenPalskinSrcBlend);
        Assert.Equal(6, D3dDeviceState.FirstSeenPalskinDestBlend);
        Assert.True(WorldShading.FirstSeenPalskinSrcAlphaBlend);
        Assert.False(WorldShading.FirstSeenFlag1SelectsAlphaBlend);
        Assert.True(WorldShading.FirstSeenPalskinReadsFlag1);
        Assert.False(WorldShading.FirstSeenStaticLitReadsFlag1);
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        Assert.Equal("StartOakValeWest", RegionTravel.StartingRegion(levels.World));
    }
}
