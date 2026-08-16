using System.Numerics;
using Fable.Core;
using Fable.Game;
using Fable.Render;

namespace Fable.Formats.Tests;

public sealed class GizmoSceneTests
{
    [Fact]
    public void Lookout_point_builds_gizmo_lines()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        using var levels = new LevelLibrary(install);
        var things = levels.LoadThings("LookoutPoint");
        var scene = GizmoScene.FromMarkers("LookoutPoint", things.Things
            .Where(t => t.PositionX is not null)
            .Select(t => new SceneMarker(
                new Vector3(t.PositionX!.Value, t.PositionY!.Value, t.PositionZ!.Value),
                t.DefinitionType ?? t.Kind)));
        Assert.True(scene.ThingCount > 10);
        Assert.True(scene.Lines.Count > scene.ThingCount * 4);
        Assert.True(scene.Centroid.Length() > 1f);
    }
}
