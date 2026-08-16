using System.Numerics;
using Fable.Core;
using Fable.Formats.Tng;
using Fable.Formats.Wld;
using Fable.Game;
using Fable.Render;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

var install = GameInstall.TryLocate();
if (install is null)
{
    Console.Error.WriteLine("Fable TLC not found. Set FABLE_PATH.");
    return 2;
}

using var levels = new LevelLibrary(install);
var region = args.FirstOrDefault(arg => !arg.StartsWith('-')) ?? RegionTravel.StartingRegion(levels.World);
ThingFile things = null!;
GizmoScene scene = null!;
WorldGeometry world = null!;
WorldMap? map = null;
IReadOnlyList<RegionExit> exits = [];
Vector3 startPosition = new(64f, -40f, 95f);
Vector3 startLook = new(64f, 64f, 36f);
using var textures = new TextureLibrary(install);
EnterRegion(region, arrivedFromExit: null);

var camera = new FlyCamera { Position = startPosition };
camera.LookAt(startLook);

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
var gWasDown = false;

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
    Console.WriteLine($"camera {camera.Position} -> {startLook}  meshVerts={mesh.Vertices.Length} textures={mesh.Draws.Length}");
    Console.WriteLine("WASD walk  Q/E up-down  Shift sprint  RMB look  Home reset  G gizmos  F1 dump  Esc quit");
    Console.WriteLine($"start {region} at {startPosition.X:0.0},{startPosition.Y:0.0},{startPosition.Z:0.0}  exits={exits.Count}");
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
        camera.Position = startPosition;
        camera.LookAt(startLook);
    }

    var f1Down = keyboard.IsKeyPressed(Key.F1);
    if (f1Down && !f1WasDown)
        DumpThings();
    f1WasDown = f1Down;

    var gDown = keyboard.IsKeyPressed(Key.G);
    if (gDown && !gWasDown && renderer is not null)
        renderer.ShowGizmos = !renderer.ShowGizmos;
    gWasDown = gDown;

    var move = Vector3.Zero;
    if (keyboard.IsKeyPressed(Key.W)) move.Y += 1;
    if (keyboard.IsKeyPressed(Key.S)) move.Y -= 1;
    if (keyboard.IsKeyPressed(Key.D)) move.X += 1;
    if (keyboard.IsKeyPressed(Key.A)) move.X -= 1;
    if (keyboard.IsKeyPressed(Key.E) || keyboard.IsKeyPressed(Key.Space)) move.Z += 1;
    if (keyboard.IsKeyPressed(Key.Q) || keyboard.IsKeyPressed(Key.ControlLeft)) move.Z -= 1;
    if (move.LengthSquared() > 0)
        camera.Move(Vector3.Normalize(move), (float)dt, keyboard.IsKeyPressed(Key.ShiftLeft));

    TryWalk();

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

void EnterRegion(string next, RegionExit? arrivedFromExit)
{
    region = next;
    things = levels.LoadThings(region);
    scene = GizmoScene.FromMarkers(region, things.Things
        .Where(t => t.PositionX is not null)
        .Select(t => new SceneMarker(
            new Vector3(t.PositionX!.Value, t.PositionY!.Value, t.PositionZ!.Value),
            t.DefinitionType ?? t.Kind)));
    Console.WriteLine($"Building {region}...");
    world = WorldGeometry.Build(install, region, things.Things);
    map = levels.World.FindMap(region);
    exits = RegionTravel.ActiveExits(things.Things);
    Console.WriteLine($"Instanced {world.MeshInstances} meshes ({world.Triangles.Count} tris), missing {world.MissingMeshes}");

    ThingInstance? spawn = null;
    if (arrivedFromExit is { } hit)
        spawn = RegionTravel.FindEntrance(things.Things, hit.Link);
    spawn ??= RegionTravel.FindPlayerStart(things.Things);
    if (spawn is not null)
    {
        startPosition = RegionTravel.PositionOf(spawn);
        startLook = startPosition + RegionTravel.ForwardOf(spawn) * 8f;
    }
    else
    {
        startPosition = new Vector3(64f, -40f, 95f);
        startLook = new Vector3(64f, 64f, 36f);
    }
}

void TryWalk()
{
    if (renderer is null)
        return;
    var hit = RegionTravel.HitExit(exits, camera.Position);
    if (hit is not { } crossed)
        return;
    var dest = levels.World.Maps.FirstOrDefault(item => item.MapUid == crossed.Link.MapUid);
    if (dest is null)
        return;

    Console.WriteLine($"walk {region} -> {dest.ScriptName}  radius={crossed.Radius}");
    EnterRegion(dest.ScriptName, crossed);
    camera.Position = startPosition;
    camera.LookAt(startLook);
    renderer.SetLines(CollectionsMarshalAsSpan(scene.Lines));
    var mesh = MeshBatches.Build(world.Triangles);
    renderer.SetTextures(LoadGpuTextures(mesh, textures));
    renderer.SetMesh(mesh.Vertices, mesh.Draws);
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
