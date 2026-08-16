using System.Numerics;
using Fable.Core;
using Fable.Game;
using Fable.Render;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

var region = args.FirstOrDefault(arg => !arg.StartsWith('-')) ?? "LookoutPoint";
var install = GameInstall.TryLocate();
if (install is null)
{
    Console.Error.WriteLine("Fable TLC not found. Set FABLE_PATH.");
    return 2;
}

using var levels = new LevelLibrary(install);
var things = levels.LoadThings(region);
var scene = GizmoScene.FromMarkers(region, things.Things
    .Where(t => t.PositionX is not null)
    .Select(t => new SceneMarker(
        new Vector3(t.PositionX!.Value, t.PositionY!.Value, t.PositionZ!.Value),
        t.DefinitionType ?? t.Kind)));
Console.WriteLine("Building world meshes...");
var world = WorldGeometry.Build(install, region, things.Things);
Console.WriteLine($"Instanced {world.MeshInstances} meshes ({world.Triangles.Count} tris), missing {world.MissingMeshes}");
using var textures = new TextureLibrary(install);
var map = levels.World.FindMap(region);

var lookTarget = new Vector3(64f, 64f, 36f);
var camera = new FlyCamera { Position = new Vector3(64f, -40f, 95f) };
camera.LookAt(lookTarget);

var options = WindowOptions.DefaultVulkan with
{
    Title = Title(),
    Size = new Vector2D<int>(1600, 900),
    VSync = true,
};

using var window = Window.Create(options);
VulkanLineRenderer? renderer = null;
IInputContext? input = null;
IMouse? mouse = null;
Vector2 lastMouse = Vector2.Zero;
var looking = false;
var f1WasDown = false;

window.Load += () =>
{
    if (window.VkSurface is null)
        throw new NotSupportedException("This window backend cannot create a Vulkan surface.");

    renderer = new VulkanLineRenderer(window);
    renderer.SetLines(CollectionsMarshalAsSpan(scene.Lines));
    var mesh = MeshBatches.Build(world.Triangles);
    renderer.SetTextures(LoadGpuTextures(mesh, textures));
    renderer.SetMesh(mesh.Vertices, mesh.Draws);
    input = window.CreateInput();
    mouse = input.Mice.Count > 0 ? input.Mice[0] : null;
    if (mouse is not null)
        mouse.MouseMove += (_, point) => OnMouseMove(new Vector2(point.X, point.Y));

    Console.WriteLine($"{install.Edition}: {install.Root}");
    Console.WriteLine($"{region}: {scene.ThingCount} things, {scene.Lines.Count} line verts");
    Console.WriteLine($"camera {camera.Position} -> {lookTarget}  meshVerts={mesh.Vertices.Length} textures={mesh.Draws.Length}");
    Console.WriteLine("WASD move  Q/E up-down  Shift sprint  RMB look  Home reset  F1 dump  Esc quit");
};

window.Update += dt =>
{
    if (input is null)
        return;

    var keyboard = input.Keyboards.Count > 0 ? input.Keyboards[0] : null;
    if (keyboard is null)
        return;

    if (keyboard.IsKeyPressed(Key.Escape))
    {
        window.Close();
        return;
    }

    if (keyboard.IsKeyPressed(Key.Home))
    {
        camera.Position = new Vector3(64f, -40f, 95f);
        camera.LookAt(lookTarget);
    }

    var f1Down = keyboard.IsKeyPressed(Key.F1);
    if (f1Down && !f1WasDown)
        DumpThings();
    f1WasDown = f1Down;

    var move = Vector3.Zero;
    if (keyboard.IsKeyPressed(Key.W)) move.Y += 1;
    if (keyboard.IsKeyPressed(Key.S)) move.Y -= 1;
    if (keyboard.IsKeyPressed(Key.D)) move.X += 1;
    if (keyboard.IsKeyPressed(Key.A)) move.X -= 1;
    if (keyboard.IsKeyPressed(Key.E) || keyboard.IsKeyPressed(Key.Space)) move.Z += 1;
    if (keyboard.IsKeyPressed(Key.Q) || keyboard.IsKeyPressed(Key.ControlLeft)) move.Z -= 1;
    if (move.LengthSquared() > 0)
        camera.Move(Vector3.Normalize(move), (float)dt, keyboard.IsKeyPressed(Key.ShiftLeft));

    looking = mouse is not null && mouse.IsButtonPressed(MouseButton.Right);
    if (mouse is not null)
        mouse.Cursor.CursorMode = looking ? CursorMode.Disabled : CursorMode.Normal;

    window.Title = Title();
};

window.Render += _ =>
{
    if (renderer is null || window.FramebufferSize.X == 0)
        return;

    var aspect = window.FramebufferSize.X / (float)window.FramebufferSize.Y;
    renderer.Draw(camera.ViewProjection(aspect), camera.Position);
};

window.Closing += () =>
{
    renderer?.Dispose();
    renderer = null;
    input?.Dispose();
};

window.Run();
return 0;

string Title()
{
    var mapLabel = map is null ? region : $"{map.ScriptName}  ({map.MapX},{map.MapY})";
    return $"FableCSharp — {mapLabel} — {world.MeshInstances} meshes / {scene.ThingCount} things — cam {camera.Position.X:0.0}, {camera.Position.Y:0.0}, {camera.Position.Z:0.0}";
}

void OnMouseMove(Vector2 point)
{
    if (looking)
        camera.Look(point.X - lastMouse.X, point.Y - lastMouse.Y);
    lastMouse = point;
}

void DumpThings()
{
    Console.WriteLine($"--- {region} {scene.ThingCount} things ---");
    foreach (var thing in things.Things.Take(40))
    {
        Console.WriteLine(
            $"{thing.Kind,-12} {(thing.DefinitionType ?? "-"),-36} {thing.PositionX:0.0},{thing.PositionY:0.0},{thing.PositionZ:0.0}");
    }
}

static ReadOnlySpan<LineVertex> CollectionsMarshalAsSpan(IReadOnlyList<LineVertex> lines) =>
    lines is List<LineVertex> list ? System.Runtime.InteropServices.CollectionsMarshal.AsSpan(list) : lines.ToArray();

static IReadOnlyList<GpuTexture> LoadGpuTextures(TexturedMesh mesh, TextureLibrary textures)
{
    var ids = mesh.Draws.SelectMany(draw => new[] { draw.TextureId, draw.TextureId1 });
    var files = textures.LoadMany(ids);
    var list = files
        .Select(file => new GpuTexture(file.Id, file.Width, file.Height, file.Rgba))
        .ToList();
    list.Add(GpuTexture.White());
    return list;
}
