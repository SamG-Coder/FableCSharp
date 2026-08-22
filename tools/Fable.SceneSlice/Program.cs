using Fable.Core;
using Fable.Game;
using Fable.Render;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

var backendName = Option(args, "--backend") ?? "vulkan";
var output = Path.GetFullPath(Option(args, "--out") ??
    Path.Combine("tools", "Fable.SceneSlice", "out", "start-oakvale"));
var install = GameInstall.TryLocate()
              ?? throw new InvalidOperationException("Fable TLC not found. Set FABLE_PATH.");

Console.WriteLine("SLICE\tphase=build\tregion=StartOakVale\tsource=Gameflow/Q_NewOakValeIntro/S_QNOVI");
var packet = BuildStartOakvale(install);
SceneRenderCapture.Write(output, packet);
Console.WriteLine($"SLICE\tphase=capture\thash={packet.ContentHash()}\tpath={output}");

var contractPath = Path.Combine(output, "dx9-render-grep.txt");
using (var dx9 = new Dx9SceneContractBackend(contractPath))
{
    dx9.Load(packet);
    dx9.Render();
}
Console.WriteLine($"SLICE\tphase=dx9-contract\tpath={contractPath}");

var vulkanContractPath = Path.Combine(output, "vulkan-render-grep.txt");
using (var vulkanContract = new VulkanSceneContractBackend(vulkanContractPath))
{
    vulkanContract.Load(packet);
    vulkanContract.Render();
}
Console.WriteLine($"SLICE\tphase=vulkan-contract\tpath={vulkanContractPath}");

if (backendName.Equals("capture", StringComparison.OrdinalIgnoreCase) ||
    backendName.Equals("dx9-contract", StringComparison.OrdinalIgnoreCase))
    return;
if (!backendName.Equals("vulkan", StringComparison.OrdinalIgnoreCase))
    throw new ArgumentException("--backend must be vulkan, dx9-contract, or capture");

var options = WindowOptions.DefaultVulkan with
{
    Title = $"Fable scene slice [{packet.ContentHash()[..12]}] Vulkan",
    Size = new Vector2D<int>(packet.ViewportWidth, packet.ViewportHeight),
    WindowBorder = WindowBorder.Fixed,
    VSync = true,
};
using var window = Window.Create(options);
VulkanLineRenderer? renderer = null;
VulkanSceneRenderBackend? backend = null;
IInputContext? input = null;
window.Load += () =>
{
    renderer = new VulkanLineRenderer(window);
    backend = new VulkanSceneRenderBackend(renderer);
    backend.Load(packet);
    input = window.CreateInput();
    if (input.Keyboards.Count > 0)
        input.Keyboards[0].KeyDown += (_, key, _) =>
        {
            if (key == Key.Escape)
                window.Close();
        };
    Console.WriteLine("SLICE\tphase=render\tbackend=vulkan\tescape=close");
};
window.Render += _ => backend?.Render();
window.Closing += () =>
{
    input?.Dispose();
    backend?.Dispose();
    renderer?.Dispose();
};
window.Run();

static SceneRenderPacket BuildStartOakvale(GameInstall install)
{
    using var life = new EngineLifecycle();
    life.Bootstrap(install);
    life.Trace.Enabled = false;
    while (life.Stage == EngineStage.StartupVideos)
        life.FinishStartupVideo();
    life.RequestNewGame();
    var loaded = false;
    for (var frame = 0; frame < 16 && !loaded; frame++)
    {
        if (!life.Pump(1f / 60f))
            throw new InvalidOperationException("New Game lifecycle stopped before Gameflow.");
        loaded = life.EnsureFirstPlayableRegionLoaded();
    }
    if (!loaded)
        throw new InvalidOperationException(
            $"S_QNOVI did not select StartOakVale (stage={life.Stage}, gameflow={life.GameflowState}, quests={string.Join(',', life.ActivatedQuests)}).");
    var reachedIntroShot = false;
    for (var frame = 0; frame < 2400 && !reachedIntroShot; frame++)
    {
        if (!life.Pump(0.1f))
            throw new InvalidOperationException("New Game lifecycle stopped during the Oakvale intro.");

        // PlayAVI is a native blocking command. The comparison slice models the
        // player's skip input so that the real cutscene interpreter can reach
        // UseCamera; it never writes the camera or instruction pointer itself.
        if (life.Runtime?.AviPlaying == true)
            life.Runtime.SkipAvi();

        reachedIntroShot =
            life.Runtime?.ExecutedVerb(RegionTravel.IntroCutscene, "UseCamera") == true &&
            string.Equals(life.Camera.ActiveName, RegionTravel.IntroFirstSeenCamera,
                StringComparison.OrdinalIgnoreCase);
    }
    if (!reachedIntroShot)
    {
        var intro = life.Runtime?.FindInterpreter(RegionTravel.IntroCutscene);
        throw new InvalidOperationException(
            $"Cutscene did not reach its authored intro shot " +
            $"(camera={life.Camera.ActiveName ?? "NULL"}, ip={intro?.InstructionPointer}, " +
            $"command={(intro is not null && intro.InstructionPointer < intro.Commands.Count ? intro.Commands[intro.InstructionPointer] : "NULL")}).");
    }
    if (!life.WorldSubmitted)
        throw new InvalidOperationException("StartOakVale scene did not submit.");
    if (!string.Equals(life.CurrentRegion?.RegionName, "StartOakVale", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException($"Unexpected region {life.CurrentRegion?.RegionName ?? "NULL"}.");
    return SceneRenderPacketFactory.Capture(life, "StartOakVale/NOVI_LiveFather/CS_OAKVALE_INTRO_FATHER");
}

static string? Option(string[] args, string name)
{
    for (var i = 0; i < args.Length; i++)
    {
        if (args[i].StartsWith(name + "=", StringComparison.OrdinalIgnoreCase))
            return args[i][(name.Length + 1)..];
        if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            return args[i + 1];
    }
    return null;
}
