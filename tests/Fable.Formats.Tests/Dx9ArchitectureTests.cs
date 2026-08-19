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
    public void Migrated_frontend_does_not_pass_vulkan_batch()
    {
        var rec = new RecordingDx9Device();
        var host = new FakeEngineHost();
        var life = new EngineLifecycle { Device = rec };
        life.AttachHost(host);
        life.Bootstrap(null);
        while (life.Stage == EngineStage.StartupVideos)
            life.FinishStartupVideo();
        life.Pump();
        Assert.Null(life.FrontendBatch);
        Assert.Equal(0, host.PresentCalls);
        var frame = life.BuildFrame();
        Assert.Null(frame.FrontendBatch);
        Assert.Null(frame.Vertices);
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
