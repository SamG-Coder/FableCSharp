using System.Numerics;
using Fable.Formats;
using Fable.Render;

namespace Fable.Game;

/// <summary>Freezes an engine frame at the backend comparison boundary.</summary>
public static class SceneRenderPacketFactory
{
    public static SceneRenderPacket Capture(EngineLifecycle life, string? name = null)
    {
        var frame = life.BuildFrame();
        if (frame.Vertices is not { Length: > 0 } landscape ||
            frame.ObjectVertices is not { Length: > 0 } objects ||
            frame.Textures is not { Length: > 0 } textures)
            throw new InvalidOperationException("The engine has not submitted a complete 3D scene.");

        var camera = frame.Camera;
        var aspect = EngineLifecycle.DisplayDefaultWidth /
                     (float)EngineLifecycle.DisplayDefaultHeight;
        return new SceneRenderPacket
        {
            SceneName = name ?? life.CurrentRegion?.RegionName ?? "scene",
            LandscapeVertices = landscape,
            LandscapeDraws = frame.Draws ?? [],
            LandscapeIndices = frame.Indices ?? [],
            ObjectVertices = objects,
            ObjectDraws = frame.ObjectDraws ?? [],
            Textures = textures,
            ViewProjection = camera.ViewProjection(aspect),
            LandscapeViewProjection = camera.HostLandscapeViewProjection(aspect),
            SkyViewProjection = camera.SkyViewProjection(aspect),
            CameraPosition = camera.Position,
            FogPlane = WorldShading.LinearFogPlane(camera.Position, camera.Forward),
            ViewportWidth = life.ViewportWidth,
            ViewportHeight = life.ViewportHeight,
        };
    }
}
