using System.Numerics;
using Fable.Client;
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
life.BootstrapUntilGraphics(install);
var options = WindowOptions.DefaultVulkan with
{
    Title = life.WindowTitle,
    Size = new Vector2D<int>(life.BackBufferWidth, life.BackBufferHeight),
    WindowState = SilkHostWindow.DefaultState,
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
var enterWasDown = false;
var lmbWasDown = false;
var debugFly = false;
var debugCam = new FlyCamera();

window.Load += () =>
{
    if (window.VkSurface is null)
        throw new NotSupportedException("This window backend cannot create a Vulkan surface.");

    host.Renderer = new VulkanLineRenderer(window);
    // Shadow: records Clear/Present.
    // OwnsSwapchainPresent stays false
    // until frontend sprites and glyphs
    // are NativeSemantic.
    life.Device = new VulkanDx9Device { Renderer = host.Renderer };
    life.CompleteRetailLoop();
    // After Device: skip AVI still issues
    // 0042EFF7 Clear+Present.
    if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("FABLE_SKIP_STARTUP_AVI")))
    {
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Console.WriteLine("FABLE_SKIP_STARTUP_AVI skipped PlayAVI to " + life.Stage);
    }

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

    var altDown = keyboard.IsKeyPressed(Key.AltLeft) ||
                  keyboard.IsKeyPressed(Key.AltRight);
    var enterDown = keyboard.IsKeyPressed(Key.Enter);
    if (SilkHostWindow.AltEnterPressed(altDown, enterDown, enterWasDown))
        window.WindowState = SilkHostWindow.ToggleFullscreen(window.WindowState);
    enterWasDown = enterDown;

    if (!debugFly)
        SilkNativeInput.QueueKeys(life, keyboard, skipEnter: altDown);

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
        var pos = mouse.Position;
        var moved = pos.X != lastMouse.X || pos.Y != lastMouse.Y;
        lastMouse = new Vector2(pos.X, pos.Y);
        // 0055BF10 reads input+184 vtbl+64
        // as dest pixels. 009BEF80 viewport
        // is the created window size.
        if (!debugFly)
            SilkNativeInput.QueuePointer(
                life,
                new Vector2D<float>(pos.X, pos.Y),
                moved,
                lmbDown,
                lmbWasDown,
                window.Size.X,
                window.Size.Y);
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
    // NativeSemantic Device.Present already
    // consumed the swapchain. Shadow and
    // Compatibility still need host.Draw.
    else if (life.Dx9OwnsFrontendPresent)
        return;
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
