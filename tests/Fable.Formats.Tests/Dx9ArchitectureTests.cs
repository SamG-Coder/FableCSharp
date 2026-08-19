using Fable.Client;
using Fable.Dx9;
using Fable.Game;

namespace Fable.Formats.Tests;

public sealed class Dx9ArchitectureTests
{
    [Fact]
    public void Neutral_dx9_assembly_has_no_silk_or_vulkan()
    {
        var names = typeof(IDirect3DDevice9).Assembly
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? "")
            .ToArray();
        Assert.DoesNotContain(names, n => n.Contains("Silk", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(names, n => n.Contains("Vulkan", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain("Fable.Render", names);
        Assert.DoesNotContain("Fable.Game", names);
    }

    [Fact]
    public void Attaching_dx9_device_does_not_discard_frontend_batch()
    {
        var rec = new RecordingDx9Device();
        var host = new FakeEngineHost();
        var life = new EngineLifecycle { Device = rec };
        life.AttachHost(host);
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.Equal(Dx9SubmitMode.Shadow, life.FrontendSubmitMode);
        Assert.False(life.Dx9OwnsFrontendPresent);
        Assert.True(life.Pump());
        Assert.False(life.Dx9OwnsFrontendPresent);
        Assert.NotNull(life.FrontendBatch);
        Assert.True(host.PresentCalls > 0);
        var frame = life.BuildFrame();
        Assert.NotNull(frame.FrontendBatch);
        Assert.Null(frame.Vertices);
    }

    [Fact]
    public void Frontend_stays_shadow_until_sprite_and_glyph_capabilities()
    {
        var rec = new RecordingDx9Device();
        var life = new EngineLifecycle { Device = rec };
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        Assert.False(life.SubmitCapabilities.CanRenderFrontendSprites);
        Assert.False(life.SubmitCapabilities.CanRenderFrontendGlyphs);
        Assert.Equal(Dx9SubmitMode.Shadow, life.FrontendSubmitMode);
        Assert.False(life.Dx9OwnsFrontendPresent);
        life.SubmitCapabilities = new Dx9SubmitCapabilities
        {
            CanRenderFrontendSprites = true,
            CanRenderFrontendGlyphs = true,
        };
        Assert.Equal(Dx9SubmitMode.NativeSemantic, life.FrontendSubmitMode);
        Assert.True(life.Dx9OwnsFrontendPresent);
    }

    [Fact]
    public void Native_semantic_without_capabilities_is_not_device_attached()
    {
        var life = new EngineLifecycle();
        life.Bootstrap(null);
        Assert.Equal(Dx9SubmitMode.Compatibility, life.FrontendSubmitMode);
        Assert.False(life.Dx9OwnsFrontendPresent);
    }

    [Fact]
    public void Native_semantic_frontend_skips_host_present()
    {
        var rec = new RecordingDx9Device();
        var host = new FakeEngineHost();
        var life = new EngineLifecycle
        {
            Device = rec,
            SubmitCapabilities = new Dx9SubmitCapabilities
            {
                CanRenderFrontendSprites = true,
                CanRenderFrontendGlyphs = true,
            },
        };
        life.AttachHost(host);
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        var presents = host.PresentCalls;
        Assert.True(life.Pump());
        Assert.True(life.Dx9OwnsFrontendPresent);
        Assert.Null(life.FrontendBatch);
        Assert.Equal(presents, host.PresentCalls);
    }

    [Fact]
    public void Silk_native_input_type_does_not_post_gameplay()
    {
        var type = typeof(SilkNativeInput);
        Assert.Null(type.GetMethod("RequestNewGame"));
        Assert.Null(type.GetMethod("DispatchFrontendMessage"));
        Assert.NotNull(type.GetMethod("QueueKeys"));
        Assert.NotNull(type.GetMethod("QueuePointer"));
    }
}
