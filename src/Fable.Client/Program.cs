using System.Numerics;
using Fable.Core;
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

using var life = new EngineLifecycle();
life.Bootstrap(install);

var options = WindowOptions.DefaultVulkan with
{
    Title = life.WindowTitle,
    Size = new Vector2D<int>(life.BackBufferWidth, life.BackBufferHeight),
    WindowState = life.DeviceWindowed
        ? WindowState.Normal
        : WindowState.Fullscreen,
    WindowBorder = WindowBorder.Fixed,
    VSync = true,
};

using var window = Window.Create(options);
var host = new SilkEngineHost(
    width: life.BackBufferWidth,
    height: life.BackBufferHeight,
    title: life.WindowTitle,
    quit: () => window.Close());
life.AttachHost(host);

IInputContext? input = null;
IMouse? mouse = null;
Vector2 lastMouse = Vector2.Zero;
var looking = false;
var f2WasDown = false;
var lmbWasDown = false;
var debugFly = false;
var debugCam = new FlyCamera();

window.Load += () =>
{
    if (window.VkSurface is null)
        throw new NotSupportedException("This window backend cannot create a Vulkan surface.");

    host.Renderer = new VulkanLineRenderer(window);
    input = window.CreateInput();
    mouse = input.Mice.Count > 0 ? input.Mice[0] : null;
    if (mouse is not null)
        mouse.MouseMove += (_, point) =>
        {
            var now = new Vector2(point.X, point.Y);
            if (looking)
                debugCam.Look(now.X - lastMouse.X, now.Y - lastMouse.Y);
            lastMouse = now;
        };

    Console.WriteLine($"{install.Edition}: {install.Root}");
    Console.WriteLine($"lifecycle {life.Stage} pe=0x{EngineLifecycle.PeEntry:X8}");
};

window.Update += dt =>
{
    if (input is null)
        return;
    var keyboard = input.Keyboards.Count > 0 ? input.Keyboards[0] : null;
    if (keyboard is null)
        return;

    if (keyboard.IsKeyPressed(Key.Escape))
        life.QueueInput(EngineInput.TypeKey, RegionTravel.PlayAviSkipEscape);
    if (keyboard.IsKeyPressed(Key.Space))
        life.QueueInput(EngineInput.TypeKey, RegionTravel.PlayAviSkipSpace);
    if (keyboard.IsKeyPressed(Key.Enter))
        life.QueueInput(EngineInput.TypeKey, RegionTravel.PlayAviSkipReturn);
    if (keyboard.IsKeyPressed(Key.F4))
        life.QueueInput(EngineInput.TypeKey, RegionTravel.PlayAviSkipF4);
    if (keyboard.IsKeyPressed(Key.A))
        life.QueueInput(EngineInput.TypeKey, EngineInput.KeyDikA);
    if (keyboard.IsKeyPressed(Key.B))
        life.QueueInput(EngineInput.TypeKey, EngineInput.KeyDikB);

    var f2Down = keyboard.IsKeyPressed(Key.F2);
    if (f2Down && !f2WasDown)
    {
        debugFly = !debugFly;
        var cam = host.LastFrame.Camera;
        debugCam.Position = cam.Position;
        debugCam.FovDegrees = cam.FovDegrees;
        debugCam.LookAt(cam.LookAt);
    }

    f2WasDown = f2Down;
    if (mouse is not null)
    {
        var lmbDown = mouse.IsButtonPressed(MouseButton.Left);
        if (lmbDown && !lmbWasDown)
            life.QueueInput(EngineInput.Type4, 0);
        if (!lmbDown && lmbWasDown)
            life.QueueInput(EngineInput.Type6, 0);
        lmbWasDown = lmbDown;
    }

    looking = debugFly && mouse is not null && mouse.IsButtonPressed(MouseButton.Right);
    if (mouse is not null)
        mouse.Cursor.CursorMode = looking ? CursorMode.Disabled : CursorMode.Normal;
    if (debugFly)
    {
        var move = Vector3.Zero;
        if (keyboard.IsKeyPressed(Key.W)) move.Y += 1;
        if (keyboard.IsKeyPressed(Key.S)) move.Y -= 1;
        if (keyboard.IsKeyPressed(Key.D)) move.X += 1;
        if (keyboard.IsKeyPressed(Key.A)) move.X -= 1;
        if (keyboard.IsKeyPressed(Key.E)) move.Z += 1;
        if (keyboard.IsKeyPressed(Key.Q)) move.Z -= 1;
        if (move.LengthSquared() > 0)
            debugCam.Move(Vector3.Normalize(move), (float)dt, keyboard.IsKeyPressed(Key.ShiftLeft));
    }

    host.Width = window.FramebufferSize.X;
    host.Height = window.FramebufferSize.Y;
    if (!life.Pump((float)dt) || life.Stage == EngineStage.Shutdown)
        window.Close();
    window.Title = life.WindowTitle;
};

window.Render += _ =>
{
    if (window.FramebufferSize.X == 0 || host.Renderer is null)
        return;
    var aspect = window.FramebufferSize.X / (float)window.FramebufferSize.Y;
    if (debugFly)
    {
        var fog = Fable.Formats.WorldShading.LinearFogPlane(debugCam.Position, debugCam.Forward);
        host.Renderer.Draw(
            debugCam.ViewProjection(aspect), debugCam.Position, fog,
            debugCam.SkyViewProjection(aspect),
            debugCam.HostLandscapeViewProjection(aspect));
    }
    else
        host.Draw(aspect);
};

window.Closing += () =>
{
    life.Dispose();
    host.Renderer?.Dispose();
};

window.Run();
return 0;
