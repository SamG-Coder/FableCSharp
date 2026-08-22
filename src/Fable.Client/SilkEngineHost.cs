using System.Numerics;
using Fable.Formats;
using Fable.Game;
using Fable.Render;
using Fable.Render.Parity.Dx9Vulkan;

/// <summary>
/// Silk / Vulkan Present adapter for
/// <c>009BEEB0</c>. The engine already
/// chose camera, world, and AVI.
/// </summary>
public sealed class SilkEngineHost : IEngineHost
{
    private readonly Action? _quit;
    private EngineFrame _frame;
    private int _aviWidth;
    private int _aviHeight;
    private WorldGeometry? _uploadedWorld;
    private MeshVertex[]? _uploadedVertices;
    private MeshVertex[]? _uploadedObjects;
    private GpuTexture[]? _uploadedTextures;

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
    public int MeshUploads { get; private set; }
    public int TextureUploads { get; private set; }

    /// <summary>
    /// <c>009BEEB0</c> Present. Does not
    /// expand, enter a region, or start
    /// New Game.
    /// </summary>
    public void Present(EngineFrame frame)
    {
        _frame = frame;
        var renderer = Renderer;

        if (renderer is not null)
        {
            if (frame is { AviPlaying: true, AviRgba: not null })
            {
                if (frame.AviWidth != _aviWidth || frame.AviHeight != _aviHeight)
                    renderer.ClearVideoFrame();
                _aviWidth = frame.AviWidth;
                _aviHeight = frame.AviHeight;
                // 009BEDC0: dest uses the presented
                // framebuffer, not a stale 1024×768
                // host size or the world camera.
                var fbW = renderer.FramebufferWidth > 0
                    ? renderer.FramebufferWidth
                    : Width;
                var fbH = renderer.FramebufferHeight > 0
                    ? renderer.FramebufferHeight
                    : Height;
                var dest = RegionTravel.PlayAviLetterbox(
                    frame.AviWidth, frame.AviHeight, fbW, fbH);
                renderer.VideoClearColor =
                    Dx9VulkanColor.FromD3dArgb(frame.AviClearArgb);
                renderer.SetVideoFrame(
                    frame.AviWidth, frame.AviHeight, frame.AviRgba,
                    new Vector4(dest.X0, dest.Y0, dest.X1, dest.Y1),
                    frame.AviSerial);
                VulkanLineRenderer.NoteReceived(frame.AviSerial);
                renderer.SetPlayAviPump(true);
                renderer.SetFrontendBatch(null);
            }
            else if (frame.FrontendBatch is { IsEmpty: false } batch)
            {
                _aviWidth = 0;
                _aviHeight = 0;
                renderer.ClearVideoFrame();
                renderer.SetPlayAviPump(false);
                renderer.VideoClearColor =
                    Dx9VulkanColor.FromD3dArgb(0xFF000000);
                renderer.SetFrontendBatch(
                    batch,
                    frame.World is not null &&
                    (frame.Vertices is { Length: > 0 } ||
                     frame.ObjectVertices is { Length: > 0 }));
            }
            else
            {
                _aviWidth = 0;
                _aviHeight = 0;
                renderer.ClearVideoFrame();
                renderer.SetPlayAviPump(false);
                renderer.SetFrontendBatch(null);
            }

            renderer.FadeOverlayAlpha = frame.FadeAlpha;
            renderer.FadeOverlayRgb = (frame.FadeR, frame.FadeG, frame.FadeB);
        }

        if ((frame.Vertices is { Length: > 0 }) ||
            frame.ObjectVertices is { Length: > 0 })
        {
            var verts = frame.Vertices ?? [];
            var draws = frame.Draws ?? [];
            var objects = frame.ObjectVertices ?? [];
            var objectDraws = frame.ObjectDraws ?? [];
            var sameMesh = ReferenceEquals(_uploadedVertices, frame.Vertices) &&
                           ReferenceEquals(_uploadedObjects, frame.ObjectVertices);
            var sameTex = ReferenceEquals(_uploadedTextures, frame.Textures);
            if (sameMesh && (sameTex || frame.Textures is null || frame.Textures.Length == 0))
                return;

            if (frame.Textures is { Length: > 0 } engineTex && !sameTex)
            {
                renderer?.SetTextures(engineTex);
                _uploadedTextures = engineTex;
                TextureUploads++;
            }
            else if (Textures is { } bank && !sameMesh && frame.Textures is null)
            {
                var dummy = new TexturedMesh { Vertices = verts, Draws = draws };
                renderer?.SetTextures(LoadGpuTextures(dummy, bank));
                _uploadedTextures = null;
                TextureUploads++;
            }

            if (!sameMesh)
            {
                renderer?.SetMesh(verts, draws, frame.Indices ?? []);
                renderer?.SetObjects(objects, objectDraws);
                _uploadedVertices = frame.Vertices;
                _uploadedObjects = frame.ObjectVertices;
                MeshUploads++;
            }

            _uploadedWorld = frame.World;
            return;
        }

        if (frame.World is { Expanded: true, Triangles.Count: > 0 } world)
        {
            if (ReferenceEquals(_uploadedWorld, world))
                return;

            var mesh = MeshBatches.Build(world.Triangles);
            if (Textures is { } textures)
                renderer?.SetTextures(LoadGpuTextures(mesh, textures));
            renderer?.SetMesh(mesh.Vertices, mesh.Draws);
            _uploadedWorld = world;
            MeshUploads++;
            return;
        }

        if (_uploadedWorld is null && _uploadedVertices is null)
            return;

        renderer?.SetMesh([], []);
        renderer?.SetObjects([], []);
        _uploadedWorld = null;
        _uploadedVertices = null;
        _uploadedObjects = null;
        _uploadedTextures = null;
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
        // 006286F0 does not compose a 3D WVP.
        if (cam is null || _frame.AviPlaying)
        {
            Renderer.Draw(default);
            return;
        }

        // 00B30B50 letterbox uses camera +176/+180
        // = 1024×768. Window resize is not the
        // first-seen viewport.
        var nativeAspect = EngineLifecycle.DisplayDefaultWidth
            / (float)EngineLifecycle.DisplayDefaultHeight;
        _ = aspect;
        var fogPlane = WorldShading.LinearFogPlane(cam.Position, cam.Forward);
        Renderer.Draw(
            cam.ViewProjection(nativeAspect),
            cam.Position,
            fogPlane,
            cam.SkyViewProjection(nativeAspect),
            cam.HostLandscapeViewProjection(nativeAspect));
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
