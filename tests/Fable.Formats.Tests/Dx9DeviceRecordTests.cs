using Fable.Client;
using Fable.Dx9;
using Fable.Game;
using Fable.Render;

namespace Fable.Formats.Tests;

public sealed class FakeEngineHost : IEngineHost
{
    public int Width { get; set; } = 1024;
    public int Height { get; set; } = 768;
    public string Title { get; set; } = "";
    public int PresentCalls { get; private set; }
    public EngineFrame LastFrame { get; private set; }

    public void Present(EngineFrame frame)
    {
        PresentCalls++;
        LastFrame = frame;
    }

    public void Quit()
    {
    }
}

/// <summary>
/// First-seen <c>0042EFF7</c> /
/// <c>0042DF9E</c> against a recording
/// device. Drives shipped Bootstrap/Pump.
/// </summary>
public sealed class Dx9DeviceRecordTests
{
    [Fact]
    public void First_frontend_present_is_clear_begin_end_present()
    {
        var rec = new RecordingDx9Device();
        var host = new FakeEngineHost();
        var life = new EngineLifecycle { Device = rec };
        life.AttachHost(host);
        life.Bootstrap(null);
        Assert.Empty(rec.Calls);
        Assert.Equal(0, host.PresentCalls);

        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.Equal(EngineStage.Frontend, life.Stage);

        Assert.Equal(["Clear", "Present"], rec.Names.ToList());
        var afterAvi = Assert.IsType<Dx9ClearCall>(rec.Calls[0]);
        Assert.Equal(Dx9Clear.WhenArgZero, afterAvi.Flags);
        Assert.Equal(FrontendDx9Submit.FrontendFrame().ClearColorArgb, afterAvi.ColorArgb);
        Assert.Equal(1f, afterAvi.Z);
        Assert.Equal(0, afterAvi.Stencil);
        Assert.Equal(0, host.PresentCalls);
        Assert.DoesNotContain(rec.Calls, c => c is Dx9DrawIndexedPrimitiveCall);
        Assert.Null(life.FrontendBatch);

        Assert.True(life.Pump());
        Assert.Equal(
            ["Clear", "Present", "Clear", "BeginScene", "EndScene", "Present"],
            rec.Names.ToList());
        var frameClear = Assert.IsType<Dx9ClearCall>(rec.Calls[2]);
        Assert.Equal(Dx9Clear.WhenArgZero, frameClear.Flags);
        Assert.Equal(0xFF000000u, frameClear.ColorArgb);
        var vp = rec.Calls.OfType<Dx9SetViewportCall>().ToList();
        Assert.Empty(vp);
        Assert.False(life.Frontend2dDipIssued);
        Assert.Equal(2, rec.PresentCount);
        Assert.Equal(0, host.PresentCalls);
        Assert.Null(life.FrontendBatch);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
    }

    [Fact]
    public void Device_exists_before_first_clear()
    {
        var rec = new RecordingDx9Device();
        var life = new EngineLifecycle();
        life.BootstrapUntilGraphics(null);
        Assert.Null(life.Device);
        Assert.True(life.BackBufferWidth >= 32);
        life.Device = rec;
        life.CompleteRetailLoop();
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.Same(rec, life.Device);
        Assert.NotEmpty(rec.Calls);
        Assert.IsType<Dx9ClearCall>(rec.Calls[0]);
    }

    [Fact]
    public void Lock_returns_writable_storage_of_requested_size()
    {
        var rec = new RecordingDx9Device();
        var vb = rec.CreateVertexBuffer(8, 0, 0, 0);
        rec.LockVertexBuffer(vb, 0, 8, out var mem, 0);
        Assert.Equal(8, mem.Length);
        mem.Span.Fill(0x3C);
        rec.UnlockVertexBuffer(vb);
        rec.LockVertexBuffer(vb, 2, 4, out var slice, 0);
        Assert.Equal(4, slice.Length);
        Assert.Equal(0x3C, slice.Span[0]);
        slice.Span[0] = 0x7F;
        rec.UnlockVertexBuffer(vb);
        rec.LockVertexBuffer(vb, 0, 0, out var whole, 0);
        Assert.Equal(8, whole.Length);
        Assert.Equal(0x7F, whole.Span[2]);
    }

    [Fact]
    public void Shader_constants_preserve_values()
    {
        var rec = new RecordingDx9Device();
        rec.SetVertexShaderConstantF(5, [1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f]);
        var call = Assert.IsType<Dx9SetVertexShaderConstantFCall>(rec.Calls[0]);
        Assert.Equal(5, call.StartRegister);
        Assert.Equal([1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f], call.Data);
    }

    [Fact]
    public void Draw_indexed_keeps_every_argument()
    {
        var rec = new RecordingDx9Device();
        rec.DrawIndexedPrimitive(Dx9PrimitiveType.TriangleStrip, 1, 2, 4, 3, 2);
        var call = Assert.IsType<Dx9DrawIndexedPrimitiveCall>(rec.Calls[0]);
        Assert.Equal(Dx9PrimitiveType.TriangleStrip, call.Type);
        Assert.Equal(1, call.BaseVertexIndex);
        Assert.Equal(2, call.MinVertexIndex);
        Assert.Equal(4, call.NumVertices);
        Assert.Equal(3, call.StartIndex);
        Assert.Equal(2, call.PrimitiveCount);
    }

    [Fact]
    public void Viewport_keeps_xyz_minmax()
    {
        var rec = new RecordingDx9Device();
        rec.SetViewport(new Dx9Viewport(0, 0, 1024, 768, 0f, 1f));
        var call = Assert.IsType<Dx9SetViewportCall>(rec.Calls[0]);
        Assert.Equal(0, call.Viewport.X);
        Assert.Equal(0, call.Viewport.Y);
        Assert.Equal(1024, call.Viewport.Width);
        Assert.Equal(768, call.Viewport.Height);
        Assert.Equal(0f, call.Viewport.MinZ);
        Assert.Equal(1f, call.Viewport.MaxZ);
    }

    [Fact]
    public void Vulkan_dx9_device_clear_and_present_are_functional()
    {
        var device = new VulkanDx9Device();
        device.Clear(Dx9Clear.WhenArgZero, 0xFF000000u, 1f, 0);
        Assert.Equal(Dx9Clear.WhenArgZero, device.LastClearFlags);
        Assert.Equal(0xFF000000u, device.LastClearArgb);
        Assert.Equal(1f, device.LastClearZ);
        device.BeginScene();
        Assert.True(device.InScene);
        device.EndScene();
        Assert.False(device.InScene);
        device.Present();
        Assert.Equal(1, device.PresentCount);
        Assert.Throws<NotSupportedException>(
            () => device.DrawIndexedPrimitive(Dx9PrimitiveType.TriangleList, 0, 0, 0, 0, 0));
        Assert.Throws<NotSupportedException>(() => device.CreateVertexBuffer(4, 0, 0, 0));
    }

    [Fact]
    public void Host_os_window_starts_windowed_alt_enter_toggles()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        Assert.False(life.DeviceWindowed);
        Assert.Equal(Silk.NET.Windowing.WindowState.Normal, SilkHostWindow.DefaultState);
        Assert.False(SilkHostWindow.AltEnterPressed(false, true, false));
        Assert.True(SilkHostWindow.AltEnterPressed(true, true, false));
        Assert.False(SilkHostWindow.AltEnterPressed(true, true, true));
        Assert.Equal(
            Silk.NET.Windowing.WindowState.Fullscreen,
            SilkHostWindow.ToggleFullscreen(Silk.NET.Windowing.WindowState.Normal));
        Assert.Equal(
            Silk.NET.Windowing.WindowState.Normal,
            SilkHostWindow.ToggleFullscreen(Silk.NET.Windowing.WindowState.Fullscreen));
    }

    [Fact]
    public void Skip_avi_after_device_still_issues_clear_present()
    {
        var rec = new RecordingDx9Device();
        var life = new EngineLifecycle();
        life.BootstrapUntilGraphics(null);
        life.Device = rec;
        life.CompleteRetailLoop();
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.Equal(["Clear", "Present"], rec.Names.ToList());
    }
}
