using System.Numerics;
using Fable.Formats;
using Fable.Game;
using Fable.Render;

/// <summary>
/// Silk / Vulkan Present adapter for
/// <c>009BEEB0</c>. The engine already
/// chose camera, world, and AVI.
/// </summary>
public sealed class SilkEngineHost : IEngineHost
{
    private readonly Action? _quit;
    private EngineFrame _frame;
    private WorldGeometry? _uploadedWorld;

    public SilkEngineHost(
        VulkanLineRenderer? renderer = null,
        TextureLibrary? textures = null,
        int width = EngineLifecycle.DisplayDefaultWidth,
        int height = EngineLifecycle.DisplayDefaultHeight,
        string? title = null,
        Action? quit = null)
    {
        Renderer = renderer;
        Textures = textures;
        Width = width;
        Height = height;
        Title = title ?? EngineLifecycle.WindowTitleDefault;
        _quit = quit;
    }

    public VulkanLineRenderer? Renderer { get; set; }
    public TextureLibrary? Textures { get; set; }

    public int Width { get; set; }
    public int Height { get; set; }
    public string Title { get; set; }

    public EngineFrame LastFrame => _frame;

    /// <summary>
    /// <c>009BEEB0</c> Present. Does not
    /// expand, enter a region, or start
    /// New Game.
    /// </summary>
    public void Present(EngineFrame frame)
    {
        _frame = frame;
        var renderer = Renderer;
        if (renderer is null)
            return;

        if (frame is { AviPlaying: true, AviRgba: not null })
        {
            var dest = RegionTravel.PlayAviLetterbox(
                frame.AviWidth, frame.AviHeight, Width, Height);
            renderer.SetVideoFrame(
                frame.AviWidth, frame.AviHeight, frame.AviRgba,
                new Vector4(dest.X0, dest.Y0, dest.X1, dest.Y1),
                frame.AviSerial);
            VulkanLineRenderer.NoteReceived(frame.AviSerial);
            renderer.SetPlayAviPump(true);
        }
        else
        {
            renderer.ClearVideoFrame();
            renderer.SetPlayAviPump(false);
        }

        renderer.FadeOverlayAlpha = frame.FadeAlpha;
        renderer.FadeOverlayRgb = (frame.FadeR, frame.FadeG, frame.FadeB);

        if (frame.Vertices is { Length: > 0 } verts)
        {
            var draws = frame.Draws ?? [];
            if (frame.Textures is { Length: > 0 } engineTex)
                renderer.SetTextures(engineTex);
            else if (Textures is { } bank)
            {
                var dummy = new TexturedMesh { Vertices = verts, Draws = draws };
                renderer.SetTextures(LoadGpuTextures(dummy, bank));
            }

            renderer.SetMesh(verts, draws);
            _uploadedWorld = frame.World;
            return;
        }

        if (frame.World is { Expanded: true, Triangles.Count: > 0 } world)
        {
            if (ReferenceEquals(_uploadedWorld, world))
                return;

            var mesh = MeshBatches.Build(world.Triangles);
            if (Textures is { } textures)
                renderer.SetTextures(LoadGpuTextures(mesh, textures));
            renderer.SetMesh(mesh.Vertices, mesh.Draws);
            _uploadedWorld = world;
            return;
        }

        if (_uploadedWorld is null)
            return;

        renderer.SetMesh([], []);
        _uploadedWorld = null;
    }

    /// <summary>
    /// Draw the last Present using the
    /// engine camera from that frame.
    /// </summary>
    public void Draw(float aspect)
    {
        if (Renderer is null)
            return;

        var cam = _frame.Camera;
        if (cam is null)
        {
            Renderer.Draw(default);
            return;
        }

        var fogPlane = WorldShading.LinearFogPlane(cam.Position, cam.Forward);
        Renderer.Draw(
            cam.ViewProjection(aspect),
            cam.Position,
            fogPlane,
            cam.SkyViewProjection(aspect),
            cam.HostLandscapeViewProjection(aspect));
    }

    public void Quit() => _quit?.Invoke();

    private static IReadOnlyList<GpuTexture> LoadGpuTextures(TexturedMesh mesh, TextureLibrary textures)
    {
        var ids = mesh.Draws.SelectMany(draw => new[] { draw.TextureId, draw.TextureId1 });
        var files = textures.LoadMany(ids);
        var list = files
            .Select(file => new GpuTexture(file.Id, file.Width, file.Height, file.Rgba))
            .ToList();
        list.Add(GpuTexture.White());
        return list;
    }
}
