using System.Runtime.InteropServices;
using Fable.Client;
using Fable.Core;
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
        Assert.DoesNotContain(rec.Calls, c => c is Dx9DrawIndexedPrimitiveUpCall);
        Assert.Equal(Dx9SubmitMode.Shadow, life.FrontendSubmitMode);
        Assert.False(life.Dx9OwnsFrontendPresent);

        var presents = host.PresentCalls;
        Assert.True(life.Pump());
        Assert.Equal(
            ["Clear", "Present", "Clear", "BeginScene", "SetViewport", "EndScene", "Present"],
            rec.Names.ToList());
        var frameClear = Assert.IsType<Dx9ClearCall>(rec.Calls[2]);
        Assert.Equal(Dx9Clear.WhenArgZero, frameClear.Flags);
        Assert.Equal(0xFF000000u, frameClear.ColorArgb);
        var vp = rec.Calls.OfType<Dx9SetViewportCall>().ToList();
        Assert.Single(vp);
        Assert.Equal(life.BackBufferWidth, vp[0].Viewport.Width);
        Assert.Equal(life.BackBufferHeight, vp[0].Viewport.Height);
        Assert.False(life.Frontend2dDipIssued);
        Assert.DoesNotContain(rec.Calls, c => c is Dx9DrawIndexedPrimitiveCall);
        Assert.DoesNotContain(rec.Calls, c => c is Dx9DrawIndexedPrimitiveUpCall { PrimitiveCount: <= 0 });
        Assert.Equal(2, rec.PresentCount);
        Assert.True(host.PresentCalls > presents);
        Assert.False(life.Dx9OwnsFrontendPresent);
        Assert.NotNull(life.FrontendBatch);
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
        Assert.False(device.OwnsSwapchainPresent);
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
        device.DrawIndexedPrimitiveUP(
            Dx9PrimitiveType.TriangleList, 0, 4, 2,
            FrontendDx9Submit.PackQuadIndexBytes(),
            FrontendDx9Submit.Index16Format,
            new byte[4 * FrontendDx9Submit.SpriteUpVertexStride],
            FrontendDx9Submit.SpriteUpVertexStride);
        device.DrawPrimitiveUP(
            Dx9PrimitiveType.TriangleList, 2,
            new byte[FrontendDx9Submit.GlyphUpVertsPerQuad * FrontendDx9Submit.GlyphUpVertexStride],
            FrontendDx9Submit.GlyphUpVertexStride);
        device.BeginScene();
        var rec = new FrontendDx9DrawRecord(
            0, 0, 128, 64, 0, 0, 1, 1, 0xFFFFFFFFu, 0, 2);
        device.DrawIndexedPrimitiveUP(
            Dx9PrimitiveType.TriangleList, 0, 4, 2,
            FrontendDx9Submit.PackQuadIndexBytes(),
            FrontendDx9Submit.Index16Format,
            FrontendDx9Submit.PackSpriteUpVertices(rec),
            FrontendDx9Submit.SpriteUpVertexStride);
        device.EndScene();
        device.Present();
        Assert.False(device.LastBatch.IsEmpty);
        Assert.Equal(4, device.LastBatch.Vertices.Length);
        Assert.Equal(6, device.LastBatch.Indices.Length);
    }

    [Fact]
    public void Empty_dest_does_not_issue_dipup()
    {
        var rec = new RecordingDx9Device();
        FrontendDx9Submit.IssueRecoveredDraws(rec,
        [
            new FrontendDx9DrawRecord(0, 0, 0, 0, 0, 0, 1, 1, 0xFFFFFFFFu, 0, 2),
        ]);
        Assert.Empty(rec.Calls);
        Assert.DoesNotContain(rec.Calls, c => c is Dx9DrawIndexedPrimitiveCall);
    }

    [Fact]
    public void Dipup_copies_index_and_vertex_bytes()
    {
        var rec = new RecordingDx9Device();
        var verts = new byte[] { 1, 2, 3, 4 };
        var indices = new byte[] { 0, 0, 1, 0, 2, 0 };
        rec.DrawIndexedPrimitiveUP(
            Dx9PrimitiveType.TriangleList, 0, 4, 2,
            indices, FrontendDx9Submit.Index16Format, verts, 32);
        var call = Assert.IsType<Dx9DrawIndexedPrimitiveUpCall>(rec.Calls[0]);
        Assert.Equal(Dx9PrimitiveType.TriangleList, call.Type);
        Assert.Equal(0, call.MinVertexIndex);
        Assert.Equal(4, call.NumVertices);
        Assert.Equal(2, call.PrimitiveCount);
        Assert.Equal(FrontendDx9Submit.Index16Format, call.IndexFormat);
        Assert.Equal(32, call.VertexStride);
        Assert.Equal(indices, call.IndexData);
        Assert.Equal(verts, call.VertexData);
        indices[0] = 9;
        verts[0] = 9;
        Assert.Equal(0, call.IndexData[0]);
        Assert.Equal(1, call.VertexData[0]);
    }

    [Fact]
    public void Device_does_not_own_game_host_present()
    {
        var rec = new RecordingDx9Device();
        var host = new FakeEngineHost();
        var life = new EngineLifecycle { Device = rec };
        life.AttachHost(host);
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Frontend, life.Stage);
        Assert.False(life.Dx9OwnsFrontendPresent);
        life.RequestNewGame();
        Assert.Equal(EngineStage.LeaveFrontend, life.Stage);
        Assert.False(life.Dx9OwnsFrontendPresent);
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.False(life.Dx9OwnsFrontendPresent);
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

    [Fact]
    public void Device_shadow_frontend_keeps_press_start_new_profile_main_menu()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var rec = new RecordingDx9Device();
        var host = new FakeEngineHost();
        using var life = new EngineLifecycle { Device = rec };
        life.AttachHost(host);
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.True(life.Pump());
        Assert.Equal(EngineLifecycle.FrontendPressStartMenu, life.FrontendMenuRoot);
        Assert.False(life.Dx9OwnsFrontendPresent);
        Assert.NotNull(life.FrontendBatch);
        Assert.False(life.FrontendBatch.Value.IsEmpty);
        Assert.True(host.PresentCalls > 0);

        life.QueueInput(EngineInput.Type4, 0);
        life.QueueInput(EngineInput.Type6, 0);
        Assert.True(life.Pump());
        Assert.Equal(EngineLifecycle.FrontendNewProfileMenu, life.FrontendMenuRoot);
        Assert.NotNull(life.FrontendBatch);
        Assert.False(life.FrontendBatch.Value.IsEmpty);

        ClickNamed(life, "UI_ACCEPT_NEW_PROFILE");
        Assert.Equal(EngineLifecycle.FrontendMainMenuNoContinue, life.FrontendMenuRoot);
        Assert.Contains(life.FrontendWidgets, w =>
            w.Name == "UI_FRONTEND_BUTTON_NEW_GAME" &&
            w.MessageId == FrontendMessages.NewGame);
        Assert.NotNull(life.FrontendBatch);
        Assert.False(life.FrontendBatch.Value.IsEmpty);
        Assert.False(life.Dx9OwnsFrontendPresent);
        AssertNonemptyShadowDraws(rec);
    }

    [Fact]
    public void Native_semantic_frontend_present_builds_device_batch()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var device = new VulkanDx9Device { OwnsSwapchainPresent = false };
        var host = new FakeEngineHost();
        using var life = new EngineLifecycle
        {
            Device = device,
            SubmitCapabilities = new Dx9SubmitCapabilities
            {
                CanRenderFrontendSprites = true,
                CanRenderFrontendGlyphs = true,
            },
        };
        life.AttachHost(host);
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        var presents = host.PresentCalls;
        Assert.True(life.Pump());
        Assert.True(life.Dx9OwnsFrontendPresent);
        Assert.True(EngineLifecycle.FrontendPresentBodyIsLive);
        Assert.True(EngineLifecycle.DisplayFlushQueueIsNoteOnly);
        Assert.Null(life.FrontendBatch);
        Assert.Equal(presents, host.PresentCalls);
        Assert.False(device.LastBatch.IsEmpty);
        Assert.True(device.LastBatch.Vertices.Length >= 4);
        Assert.Contains(device.LastBatch.Draws, d => d.IndexCount == 6);
        Assert.Contains(device.LastBatch.Draws, d => d.IndexCount == 0 && d.VertexCount == 6);
        Assert.Equal(EngineLifecycle.FrontendPressStartMenu, life.FrontendMenuRoot);
    }

    [Fact]
    public void Shadow_frontend_pump_records_nonempty_sprite_and_glyph_draws()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var rec = new RecordingDx9Device();
        var host = new FakeEngineHost();
        using var life = new EngineLifecycle { Device = rec };
        life.AttachHost(host);
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        var presents = host.PresentCalls;
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Frontend, life.Stage);
        Assert.Equal(Dx9SubmitMode.Shadow, life.FrontendSubmitMode);
        Assert.False(life.Dx9OwnsFrontendPresent);
        Assert.False(life.SubmitCapabilities.CanRenderFrontendSprites);
        Assert.False(life.SubmitCapabilities.CanRenderFrontendGlyphs);
        Assert.NotNull(life.FrontendBatch);
        Assert.False(life.FrontendBatch.Value.IsEmpty);
        Assert.True(host.PresentCalls > presents);
        Assert.False(life.Frontend2dDipIssued);
        AssertNonemptyShadowDraws(rec);
    }

    [Fact]
    public void Device_shadow_does_not_drop_first_world_submit()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var rec = new RecordingDx9Device();
        var host = new FakeEngineHost();
        using var life = new EngineLifecycle { Device = rec };
        life.AttachHost(host);
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.True(life.Pump());
        Assert.NotNull(life.FrontendBatch);
        Assert.False(life.FrontendBatch.Value.IsEmpty);
        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.Pump());
        life.LoadFromFirstRealRegion();
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.False(life.Dx9OwnsFrontendPresent);
        Assert.True(life.WorldSubmitted);
        Assert.True(life.SubmittedLandscapeCells > 0);
        Assert.NotNull(life.SubmittedLandscape);
        Assert.NotNull(life.SubmittedObjects);
        Assert.True(life.SubmittedHeroPalskin);
        var frame = life.BuildFrame();
        Assert.Same(frame.Vertices, life.SubmittedLandscape!.Vertices);
        Assert.Same(frame.ObjectVertices, life.SubmittedObjects!.Vertices);
        Assert.True(host.PresentCalls > 0);
        Assert.Equal("LookoutPoint", life.FirstSceneMapName);
        Assert.Contains(life.RegionThings, t =>
            t.DefinitionType == RegionTravel.PlayerStartType &&
            t.ScriptName == EngineLifecycle.GuildArrivalHsp);
        Assert.True(life.HeroSpawned);
        Assert.Contains(life.SubmittedMesh!.Draws, d => d.PassBit == 0x2000);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        Assert.DoesNotContain(life.ActivatedQuests, q => q == RegionTravel.IntroQuest);
        Assert.DoesNotContain(life.ActivatedQuests, q => q == RegionTravel.IntroScriptName);
    }

    [Fact]
    public void No_save_guild_arrival_slice_native_semantic_frontend_then_3d()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var device = new VulkanDx9Device { OwnsSwapchainPresent = true };
        var host = new FakeEngineHost();
        using var life = new EngineLifecycle
        {
            Device = device,
            SubmitCapabilities = new Dx9SubmitCapabilities
            {
                CanRenderFrontendSprites = true,
                CanRenderFrontendGlyphs = true,
            },
        };
        life.AttachHost(host);
        life.Bootstrap(install);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.True(life.Pump());
        Assert.True(life.Dx9OwnsFrontendPresent);
        Assert.Null(life.FrontendBatch);
        Assert.False(device.LastBatch.IsEmpty);
        Assert.Contains(device.LastBatch.Draws, d => d.IndexCount == 6);
        Assert.Contains(device.LastBatch.Draws, d => d.IndexCount == 0 && d.VertexCount == 6);
        Assert.Equal(EngineLifecycle.FrontendPressStartMenu, life.FrontendMenuRoot);

        life.ActivateNewGame();
        Assert.True(life.Pump());
        Assert.True(life.Pump());
        Assert.Equal(EngineStage.Game, life.Stage);
        Assert.False(life.Dx9OwnsFrontendPresent);
        Assert.False(device.OwnsSwapchainPresent);
        Assert.True(host.PresentCalls > 0);
        life.LoadFromFirstRealRegion();
        Assert.True(life.Pump());
        Assert.False(life.Dx9OwnsFrontendPresent);
        Assert.Equal("LookoutPoint", life.FirstSceneMapName);
        Assert.Contains(life.RegionThings, t =>
            t.DefinitionType == RegionTravel.PlayerStartType &&
            t.ScriptName == EngineLifecycle.GuildArrivalHsp);
        Assert.True(life.HeroSpawned);
        Assert.True(life.WorldSubmitted);
        Assert.True(life.SubmittedLandscapeCells > 0);
        Assert.NotNull(life.SubmittedLandscape);
        Assert.NotNull(life.SubmittedObjects);
        Assert.True(life.SubmittedHeroPalskin);
        Assert.Contains(life.SubmittedMesh!.Draws, d => d.PassBit == 0x2000);
        Assert.NotNull(life.BuildFrame().Vertices);
        Assert.NotNull(life.BuildFrame().ObjectVertices);
        Assert.DoesNotContain(life.Trace.Events, e => e.Va == RegionTravel.StartOakValeSetup);
        Assert.DoesNotContain(life.ActivatedQuests, q => q == RegionTravel.IntroQuest);
        Assert.DoesNotContain("StartOakVale", life.FirstSceneMapName);
    }

    private static void AssertNonemptyShadowDraws(RecordingDx9Device rec)
    {
        var names = rec.Names.ToList();
        var begin = names.LastIndexOf("BeginScene");
        var end = names.LastIndexOf("EndScene");
        Assert.True(begin >= 0 && end > begin);
        var between = rec.Calls.Skip(begin + 1).Take(end - begin - 1).ToList();
        Assert.Contains(between, c => c is Dx9DrawIndexedPrimitiveUpCall);
        Assert.DoesNotContain(between, c => c is Dx9DrawIndexedPrimitiveCall);
        Assert.DoesNotContain(rec.Calls, c =>
            c is Dx9DrawIndexedPrimitiveCall dip && dip.PrimitiveCount <= 0);
        Assert.DoesNotContain(rec.Calls, c =>
            c is Dx9DrawIndexedPrimitiveUpCall up && up.PrimitiveCount <= 0);

        var sprite = between.OfType<Dx9DrawIndexedPrimitiveUpCall>().First();
        Assert.Equal(Dx9PrimitiveType.TriangleList, sprite.Type);
        Assert.Equal(FrontendDx9Submit.DipUpMinVertexIndex, sprite.MinVertexIndex);
        Assert.Equal(FrontendDx9Submit.SpriteUpNumVertices, sprite.NumVertices);
        Assert.Equal(FrontendDx9Submit.SpriteUpPrimitiveCount, sprite.PrimitiveCount);
        Assert.Equal(FrontendDx9Submit.Index16Format, sprite.IndexFormat);
        Assert.Equal(FrontendDx9Submit.SpriteUpVertexStride, sprite.VertexStride);
        Assert.Equal(
            FrontendDx9Submit.QuadIndices,
            MemoryMarshal.Cast<byte, ushort>(sprite.IndexData).ToArray());
        Assert.Equal(
            FrontendDx9Submit.SpriteUpNumVertices * FrontendDx9Submit.SpriteUpVertexStride,
            sprite.VertexData.Length);

        var glyph = between.OfType<Dx9DrawPrimitiveUpCall>().First();
        Assert.Equal(Dx9PrimitiveType.TriangleList, glyph.Type);
        Assert.Equal(FrontendDx9Submit.GlyphUpPrimitiveCount, glyph.PrimitiveCount);
        Assert.Equal(FrontendDx9Submit.GlyphUpVertexStride, glyph.VertexStride);
        Assert.Equal(
            FrontendDx9Submit.GlyphUpVertsPerQuad * FrontendDx9Submit.GlyphUpVertexStride,
            glyph.VertexData.Length);
    }

    private static void ClickNamed(EngineLifecycle life, string name)
    {
        var index = -1;
        for (var i = 0; i < life.FrontendWidgets.Count; i++)
        {
            if (life.FrontendWidgets[i].Name == name)
            {
                index = i;
                break;
            }
        }

        Assert.True(index >= 0, name);
        Assert.True(
            FrontendHitTest.TryDestPoint(life.FrontendWidgets, index, out var x, out var y),
            name + " dest empty");
        life.SetFrontendPointer(x, y);
        life.QueueInput(EngineInput.TypeMouse, 0);
        Assert.True(life.Pump());
        life.QueueInput(EngineInput.Type4, 0);
        life.QueueInput(EngineInput.Type6, 0);
        Assert.True(life.Pump());
    }
}
