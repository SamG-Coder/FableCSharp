using System.Numerics;
using Fable.Core;
using Fable.Formats.Levels;
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

var life = new EngineLifecycle();
life.Bootstrap(install);
LevelLibrary? levels = null;
TextureLibrary? textures = null;
var region = args.FirstOrDefault(arg => !arg.StartsWith('-')) ?? "";
ThingFile? things = null;
GizmoScene scene = GizmoScene.FromMarkers("FRONT_END", []);
WorldGeometry? world = null;
WorldMap? map = null;
IReadOnlyList<RegionExit> exits = [];
Vector3 startPosition = new(64f, -40f, 95f);
Vector3 startLook = new(64f, 64f, 36f);
var startFov = RegionTravel.IntroCameraFovDegrees;
NewGameScript? intro = null;
WmvPlayer? startupAvi = null;
VulkanLineRenderer? renderer = null;
var gameCam = new ScriptedCamera();
OpenStartupVideo();

var debugCam = new FlyCamera { Position = gameCam.Position, FovDegrees = gameCam.FovDegrees };
debugCam.LookAt(gameCam.LookAt);
var debugFly = false;

var options = WindowOptions.DefaultVulkan with
{
    Title = life.WindowTitle,
    Size = new Vector2D<int>(life.BackBufferWidth, life.BackBufferHeight),
    VSync = true,
};

using var window = Window.Create(options);
IInputContext? input = null;
IMouse? mouse = null;
Vector2 lastMouse = Vector2.Zero;
var looking = false;
var f1WasDown = false;
var f2WasDown = false;
var gWasDown = false;
var nWasDown = false;
var aWasDown = false;
var bWasDown = false;
var wasAvi = false;

window.Load += () =>
{
    if (window.VkSurface is null)
        throw new NotSupportedException("This window backend cannot create a Vulkan surface.");

    renderer = new VulkanLineRenderer(window);
    BindWorldToRenderer();
    input = window.CreateInput();
    mouse = input.Mice.Count > 0 ? input.Mice[0] : null;
    if (mouse is not null)
        mouse.MouseMove += (_, point) => OnMouseMove(new Vector2(point.X, point.Y));

    Console.WriteLine($"{install.Edition}: {install.Root}");
    Console.WriteLine($"lifecycle {life.Stage} mode {life.Mode} pe=0x{EngineLifecycle.PeEntry:X8}");
    Console.WriteLine(
        $"window {life.WindowTitle} {life.BackBufferWidth}x{life.BackBufferHeight} " +
        $"input 0x{EngineLifecycle.FrontendInputFn:X} [0x{EngineLifecycle.InputDeviceVa:X}]");
    Console.WriteLine($"banks {string.Join(", ", EngineLifecycle.RetailBanks.Select(b => b.Pc))}");
    if (life.CurrentStartupVideo is { } first)
        Console.WriteLine($"startup 0x{EngineLifecycle.PlayAviPlayer:X} {first.RelativePath} {first.Width}x{first.Height}");
    Console.WriteLine("Esc skip video / quit  Enter New Game after frontend  F2 debug fly");
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
        if (startupAvi is not null)
            SkipStartupVideo();
        else if (intro?.Runtime.AviPlaying == true)
            intro.Runtime.SkipAvi();
        else
            window.Close();
        return;
    }

    if (life.Stage == EngineStage.StartupVideos)
    {
        if (keyboard.IsKeyPressed(Key.Space) ||
            keyboard.IsKeyPressed(Key.Enter) ||
            keyboard.IsKeyPressed(Key.F4))
            SkipStartupVideo();
        PumpStartupVideo((float)dt);
        PresentAvi(renderer, startupAvi, window.FramebufferSize.X, window.FramebufferSize.Y);
        life.Pump();
        window.Title = Title();
        return;
    }

    if (life.Stage == EngineStage.Frontend)
    {
        // 0059A238 msg 15. Not WASD.
        var nDown = keyboard.IsKeyPressed(Key.N) || keyboard.IsKeyPressed(Key.Enter);
        if (nDown && !nWasDown)
        {
            life.ActivateNewGame();
            life.Pump();
            Console.WriteLine(
                $"Leave frontend {life.WorldFileName} → Init Game 0x{EngineLifecycle.GameModeCtor:X}");
        }

        nWasDown = nDown;
        var aDown = keyboard.IsKeyPressed(Key.A);
        if (aDown && !aWasDown)
            life.QueueInput(EngineInput.TypeKey, EngineInput.KeyDikA);
        aWasDown = aDown;
        var bDown = keyboard.IsKeyPressed(Key.B);
        if (bDown && !bWasDown)
            life.QueueInput(EngineInput.TypeKey, EngineInput.KeyDikB);
        bWasDown = bDown;
        UnloadStartupAvi();
        // 0042DF9E BeginScene/UI/EndScene/009BEEB0.
        // window.Render Draw is that Present.
        life.Pump();
        window.Title = Title();
        return;
    }

    if (life.Stage == EngineStage.Game)
    {
        var aDown = keyboard.IsKeyPressed(Key.A);
        if (aDown && !aWasDown)
            life.QueueInput(EngineInput.TypeKey, EngineInput.KeyDikA);
        aWasDown = aDown;
        var bDown = keyboard.IsKeyPressed(Key.B);
        if (bDown && !bWasDown)
            life.QueueInput(EngineInput.TypeKey, EngineInput.KeyDikB);
        bWasDown = bDown;
        var first = !life.GamePumpFirstDone;
        var before = life.CurrentRegion;
        life.Pump();
        if (first && life.GamePumpFirstDone)
            Console.WriteLine(
                $"Game pump 0x{EngineLifecycle.GamePump:X} vtbl+52 0x{EngineLifecycle.WorldGetMapFn:X} " +
                $"[{life.CurrentRegionIndex}] dummy record+36 null (not 00DBDE40)");
        if (before is null && life.CurrentRegion is { } loaded)
        {
            Console.WriteLine(
                $"00501450/00487C20 [{life.CurrentRegionIndex}] {loaded.RegionName} " +
                $"SetRegionAsLoaded 0x{EngineLifecycle.SetRegionAsLoadedFn:X}");
            BindLifecycleFirstRegion();
        }
    }

    var f2Down = keyboard.IsKeyPressed(Key.F2);
    if (f2Down && !f2WasDown)
    {
        debugFly = !debugFly;
        if (debugFly)
            CopyGameToDebug();
    }
    f2WasDown = f2Down;

    if (debugFly && keyboard.IsKeyPressed(Key.Home))
        CopyGameToDebug();

    var f1Down = keyboard.IsKeyPressed(Key.F1);
    if (f1Down && !f1WasDown)
        DumpThings();
    f1WasDown = f1Down;

    var gDown = keyboard.IsKeyPressed(Key.G);
    if (gDown && !gWasDown && renderer is not null)
        renderer.ShowGizmos = !renderer.ShowGizmos;
    gWasDown = gDown;

    // WASD is F2 host fly only. Native input is 0042E3EE.
    if (debugFly)
    {
        var move = Vector3.Zero;
        if (keyboard.IsKeyPressed(Key.W)) move.Y += 1;
        if (keyboard.IsKeyPressed(Key.S)) move.Y -= 1;
        if (keyboard.IsKeyPressed(Key.D)) move.X += 1;
        if (keyboard.IsKeyPressed(Key.A)) move.X -= 1;
        if (keyboard.IsKeyPressed(Key.E) || keyboard.IsKeyPressed(Key.Space)) move.Z += 1;
        if (keyboard.IsKeyPressed(Key.Q) || keyboard.IsKeyPressed(Key.ControlLeft)) move.Z -= 1;
        if (move.LengthSquared() > 0)
            debugCam.Move(Vector3.Normalize(move), (float)dt, keyboard.IsKeyPressed(Key.ShiftLeft));
    }

    var avi = intro?.Runtime.AviPlaying == true || startupAvi is not null;
    if (avi && !wasAvi)
    {
        PlayAviTimeline.Reset("csharp");
        var rt = intro?.Runtime;
        Console.WriteLine(
            $"PlayAVI {rt?.AviRelativePath ?? life.CurrentStartupVideo?.RelativePath} " +
            $"{rt?.AviWidth ?? startupAvi?.Width}x{rt?.AviHeight ?? startupAvi?.Height} " +
            $"frames={rt?.AviFrameSerial ?? startupAvi?.FrameSerial} err={WmvPlayer.LastError ?? "ok"}");
    }
    if (!avi && wasAvi)
        PlayAviTimeline.Write(name: "csharp");
    wasAvi = avi;
    renderer?.SetPlayAviPump(avi);
    intro?.Update((float)dt);
    if (renderer is not null)
    {
        renderer.FadeOverlayAlpha = intro?.Runtime.OverlayAlphaByte ?? 0;
        var rgb = intro?.Runtime.FadeColor ?? default;
        renderer.FadeOverlayRgb = (rgb.R, rgb.G, rgb.B);
        var runtime = intro?.Runtime;
        if (runtime is { AviPlaying: true, AviRgba: not null } &&
            runtime.AviWidth > 0 && runtime.AviHeight > 0)
        {
            var dest = RegionTravel.PlayAviLetterbox(
                runtime.AviWidth, runtime.AviHeight,
                window.FramebufferSize.X, window.FramebufferSize.Y);
            renderer.SetVideoFrame(
                runtime.AviWidth, runtime.AviHeight, runtime.AviRgba,
                new Vector4(dest.X0, dest.Y0, dest.X1, dest.Y1),
                runtime.AviFrameSerial);
            VulkanLineRenderer.NoteReceived(runtime.AviFrameSerial);
        }
        else
            renderer.ClearVideoFrame();
    }

    TryWalk();

    looking = debugFly && mouse is not null && mouse.IsButtonPressed(MouseButton.Right);
    if (mouse is not null)
        mouse.Cursor.CursorMode = looking ? CursorMode.Disabled : CursorMode.Normal;

    window.Title = Title();
};

window.Render += _ =>
{
    if (renderer is null || window.FramebufferSize.X == 0)
        return;

    var aspect = window.FramebufferSize.X / (float)window.FramebufferSize.Y;
    if (debugFly)
    {
        var fogPlane = Fable.Formats.WorldShading.LinearFogPlane(
            debugCam.Position, debugCam.Forward);
        renderer.Draw(
            debugCam.ViewProjection(aspect), debugCam.Position, fogPlane,
            debugCam.SkyViewProjection(aspect),
            debugCam.HostLandscapeViewProjection(aspect));
    }
    else
    {
        var cam = ActiveCamera();
        var fogPlane = Fable.Formats.WorldShading.LinearFogPlane(
            cam.Position, cam.Forward);
        renderer.Draw(
            cam.ViewProjection(aspect), cam.Position, fogPlane,
            cam.SkyViewProjection(aspect),
            cam.HostLandscapeViewProjection(aspect));
    }
};

window.Closing += () =>
{
    if (wasAvi || PlayAviTimeline.Snapshot().Count > 0)
        PlayAviTimeline.Write(name: "csharp");
    UnloadStartupAvi();
    textures?.Dispose();
    textures = null;
    if (levels is not null && !ReferenceEquals(levels, life.Levels))
        levels.Dispose();
    levels = null;
    renderer?.Dispose();
    renderer = null;
    input?.Dispose();
};

window.Run();
return 0;

string Title() => life.WindowTitle;

void OnMouseMove(Vector2 point)
{
    if (looking)
        debugCam.Look(point.X - lastMouse.X, point.Y - lastMouse.Y);
    lastMouse = point;
}

void CopyGameToDebug()
{
    var cam = ActiveCamera();
    debugCam.Position = cam.Position;
    debugCam.FovDegrees = cam.FovDegrees;
    debugCam.LookAt(cam.LookAt);
}

ScriptedCamera ActiveCamera() =>
    life.Stage == EngineStage.Game && life.HeroSpawned ? life.Camera : gameCam;

LevelLibrary DrawLevels()
{
    if (life.Levels is not null)
        return life.Levels;
    levels ??= new LevelLibrary(install);
    return levels;
}

void BindLifecycleFirstRegion()
{
    if (life.Hero is null)
        return;
    UnloadStartupAvi();
    var opened = life.PresentWorld();
    if (opened is null)
        return;
    var presented = life.ExpandPresentedWorld(opened);
    if (presented is null)
        return;
    var mapName = life.FirstSceneMapName ?? presented.Region;
    var mapThings = life.ThingsForMap(mapName).ToList();
    if (mapThings.Count == 0)
        mapThings = life.RegionThings.ToList();
    if (mapThings.Count == 0)
        return;

    region = mapName;
    things = new ThingFile
    {
        Version = 2,
        Sections = [new ThingSection { Name = mapName, Things = mapThings }],
    };
    scene = GizmoScene.FromMarkers(mapName, mapThings
        .Where(t => t.PositionX is not null)
        .Select(t => new SceneMarker(
            new Vector3(t.PositionX!.Value, t.PositionY!.Value, t.PositionZ!.Value),
            t.DefinitionType ?? t.Kind)));
    gameCam.Bind(
        life.Hero.ScriptName ?? EngineLifecycle.HeroScriptName,
        life.Camera.Position, life.Camera.LookAt, life.Camera.Up,
        life.Camera.FovDegrees);
    world = presented;
    map = DrawLevels().World.FindMap(mapName);
    exits = RegionTravel.ActiveExits(mapThings);
    BindWorldToRenderer();
    Console.WriteLine(
        $"first scene {mapName} hero " +
        $"{life.Hero.PositionX:0.0},{life.Hero.PositionY:0.0},{life.Hero.PositionZ:0.0} " +
        $"eye {life.Camera.Position.X:0.0},{life.Camera.Position.Y:0.0},{life.Camera.Position.Z:0.0} " +
        $"fov={life.Camera.FovDegrees:0} " +
        $"things={mapThings.Count} inst={opened.MeshInstances} tris={world.Triangles.Count} " +
        $"parsed={life.Meshes.ParsedCount} opened={life.OpenedStaticMaps.Count} not 00DBDE40");
}

void EnterRegion(string next, RegionExit? arrivedFromExit)
{
    region = next;
    var lib = DrawLevels();
    things = lib.LoadThings(region);
    scene = GizmoScene.FromMarkers(region, things.Things
        .Where(t => t.PositionX is not null)
        .Select(t => new SceneMarker(
            new Vector3(t.PositionX!.Value, t.PositionY!.Value, t.PositionZ!.Value),
            t.DefinitionType ?? t.Kind)));
    LandscapeFrustum.Plane[]? planes = null;
    ThingInstance? spawn = null;
    if (arrivedFromExit is { } hit)
        spawn = RegionTravel.FindEntrance(things.Things, hit.Link);
    spawn ??= RegionTravel.FindPlayerStart(things.Things);
    if (life.Stage == EngineStage.Game && life.HeroSpawned)
    {
        planes = null;
        intro = null;
    }
    else if (spawn is not null &&
        gameCam.UseCamera(things.Things, RegionTravel.IntroFirstSeenCamera))
    {
        startPosition = gameCam.Position;
        startLook = gameCam.LookAt;
        startFov = gameCam.FovDegrees;
        LandscapeFrustum.LetterboxCots(
            float.DegreesToRadians(gameCam.FovDegrees), 4f, 3f, out var cotH, out var cotV);
        planes = LandscapeFrustum.ExtractSidePlanes(
            gameCam.Position, gameCam.Forward, gameCam.Up, cotH, cotV);
    }

    Console.WriteLine($"Building {region}...");
    world = WorldGeometry.Build(
        install, region, things.Things,
        adjacentStaticMaps: false,
        landscapePlanes: planes,
        levels: lib,
        meshes: life.Meshes.Opened ? life.Meshes : null);
    map = lib.World.FindMap(region);
    exits = RegionTravel.ActiveExits(things.Things);
    Console.WriteLine($"Instanced {world.MeshInstances} meshes ({world.Triangles.Count} tris), missing {world.MissingMeshes}");

    if (life.Stage == EngineStage.Game && life.HeroSpawned)
        return;
    if (planes is null && spawn is not null)
    {
        var feet = RegionTravel.PositionOf(spawn);
        var eye = world.PlayerHeight * 0.5f;
        startPosition = feet + Vector3.UnitZ * eye;
        startLook = feet + RegionTravel.ForwardOf(spawn) * 8f + Vector3.UnitZ * eye;
        gameCam.Bind(spawn.ScriptName ?? "spawn", startPosition, startLook, Vector3.UnitZ, startFov);
    }
}

void TryWalk()
{
    if (renderer is null || world is null || things is null)
        return;
    var hit = RegionTravel.HitExit(exits, ActiveCamera().Position);
    if (hit is not { } crossed)
        return;
    var dest = DrawLevels().World.Maps.FirstOrDefault(item => item.MapUid == crossed.Link.MapUid);
    if (dest is null)
        return;

    Console.WriteLine($"walk {region} -> {dest.ScriptName}  radius={crossed.Radius}");
    EnterRegion(dest.ScriptName, crossed);
    CopyGameToDebug();
    BindWorldToRenderer();
}

void UnloadStartupAvi()
{
    if (startupAvi is null)
    {
        renderer?.ClearVideoFrame();
        renderer?.SetPlayAviPump(false);
        return;
    }

    startupAvi.Dispose();
    if (!startupAvi.GraphReleased)
    {
        Console.WriteLine("PlayAVI 00A3B380 blocked; skip remaining slots");
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        startupAvi = null;
        renderer?.ClearVideoFrame();
        renderer?.SetPlayAviPump(false);
        return;
    }

    startupAvi = null;
    renderer?.ClearVideoFrame();
    renderer?.SetPlayAviPump(false);
}

void OpenStartupVideo()
{
    UnloadStartupAvi();
    if (life.Stage != EngineStage.StartupVideos)
        return;
    if (life.CurrentStartupVideo is not { } video)
        return;
    var file = RegionTravel.ResolvePlayAviFile(install, video.RelativePath);
    if (file is null)
    {
        Console.WriteLine($"startup miss {video.RelativePath}");
        life.FinishStartupVideo();
        OpenStartupVideo();
        return;
    }

    startupAvi = WmvPlayer.TryOpen(file);
    Console.WriteLine(
        $"PlayAVI {video.RelativePath} {video.Width}x{video.Height} " +
        $"player=0x{EngineLifecycle.PlayAviPlayer:X} err={WmvPlayer.LastError ?? "ok"}");
    if (startupAvi is null)
    {
        life.FinishStartupVideo();
        OpenStartupVideo();
    }
}

void SkipStartupVideo()
{
    UnloadStartupAvi();
    life.FinishStartupVideo();
    OpenStartupVideo();
}

void PumpStartupVideo(float dt)
{
    if (startupAvi is null)
        return;
    startupAvi.TryAdvance(dt);
    if (startupAvi.Ended)
        SkipStartupVideo();
}

void BindWorldToRenderer()
{
    if (renderer is null)
        return;
    renderer.SetLines(CollectionsMarshalAsSpan(scene.Lines));
    if (world is null)
        return;
    textures ??= new TextureLibrary(install);
    var mesh = MeshBatches.Build(world.Triangles);
    renderer.SetTextures(LoadGpuTextures(mesh, textures));
    renderer.SetMesh(mesh.Vertices, mesh.Draws);
}

static void PresentAvi(VulkanLineRenderer? renderer, WmvPlayer? player, int fbW, int fbH)
{
    if (renderer is null)
        return;
    if (player is { Rgba: not null, Width: > 0, Height: > 0 })
    {
        var dest = RegionTravel.PlayAviLetterbox(player.Width, player.Height, fbW, fbH);
        renderer.SetVideoFrame(
            player.Width, player.Height, player.Rgba,
            new Vector4(dest.X0, dest.Y0, dest.X1, dest.Y1),
            player.FrameSerial);
        VulkanLineRenderer.NoteReceived(player.FrameSerial);
        renderer.SetPlayAviPump(true);
        return;
    }

    renderer.ClearVideoFrame();
    renderer.SetPlayAviPump(false);
}

void DumpThings()
{
    if (things is null)
    {
        Console.WriteLine($"--- {life.Stage} no things ---");
        return;
    }

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
