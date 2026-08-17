using System.Text;
using Fable.Core;
using Fable.Game;

var install = GameInstall.TryLocate();
if (install is null)
{
    Console.Error.WriteLine("Fable install not found.");
    return 2;
}

var relative = RegionTravel.PlayAviPrefix + RegionTravel.IntroPlayAvi;
var file = RegionTravel.ResolvePlayAviFile(install, relative);
if (file is null || !File.Exists(file))
{
    Console.Error.WriteLine("dream_sequence_comp.wmv not found.");
    return 2;
}

using var player = WmvPlayer.TryOpen(file);
var t = WmvPlayer.CaptureTrace();
if (player is not null)
{
    t = new PlayAviGraphTrace
    {
        ProcessArch = t.ProcessArch,
        IntPtrSize = t.IntPtrSize,
        AddFilterHr = t.AddFilterHr,
        RenderFileHr = t.RenderFileHr,
        RunHr = t.RunHr,
        EnumPins = t.EnumPins,
        Next = t.Next,
        QueryDirection = t.QueryDirection,
        ConnectedTo = t.ConnectedTo,
        QueryPinInfo = t.QueryPinInfo,
        QueryId = t.QueryId,
        EnumMediaTypes = t.EnumMediaTypes,
        MediaTypeNext = t.MediaTypeNext,
        QueryAccept = t.QueryAccept,
        ReceiveConnection = t.ReceiveConnection,
        MemInputQi = t.MemInputQi,
        Receive = t.Receive,
        GetPointer = t.GetPointer,
        MiscFlags = t.MiscFlags,
        Graph = t.Graph,
        PinVisible = t.PinVisible,
        Connected = true,
        SamplesFromGetPointer = player.SamplesFromGetPointer,
        Frames = player.FrameSerial,
        Width = player.Width,
        Height = player.Height,
        Error = t.Error,
    };
}

var sb = new StringBuilder();
sb.AppendLine($"# PlayAVI quartz {t.ProcessArch}");
sb.AppendLine();
sb.AppendLine($"Process `{t.ProcessArch}` · `IntPtr.Size={t.IntPtrSize}` · `{file}`");
sb.AppendLine();
sb.AppendLine("| Step | Value |");
sb.AppendLine("|---|---|");
sb.AppendLine($"| AddFilter HR | `0x{t.AddFilterHr:X8}` ({t.AddFilterHr}) |");
sb.AppendLine($"| RenderFile HR | `0x{t.RenderFileHr:X8}` ({t.RenderFileHr}) |");
sb.AppendLine($"| Run HR | `0x{t.RunHr:X8}` ({t.RunHr}) |");
sb.AppendLine($"| EnumPins | {t.EnumPins} |");
sb.AppendLine($"| IEnumPins.Next | {t.Next} |");
sb.AppendLine($"| QueryDirection | {t.QueryDirection} |");
sb.AppendLine($"| ConnectedTo | {t.ConnectedTo} |");
sb.AppendLine($"| QueryPinInfo | {t.QueryPinInfo} |");
sb.AppendLine($"| QueryId | {t.QueryId} |");
sb.AppendLine($"| EnumMediaTypes | {t.EnumMediaTypes} |");
sb.AppendLine($"| IEnumMediaTypes.Next | {t.MediaTypeNext} |");
sb.AppendLine($"| QueryAccept | {t.QueryAccept} |");
sb.AppendLine($"| ReceiveConnection | {t.ReceiveConnection} |");
sb.AppendLine($"| IMemInputPin QI | {t.MemInputQi} |");
sb.AppendLine($"| IMemInputPin.Receive | {t.Receive} |");
sb.AppendLine($"| IMediaSample.GetPointer | {t.GetPointer} |");
sb.AppendLine($"| IAMFilterMiscFlags | {t.MiscFlags} |");
sb.AppendLine($"| Pin visible | `{t.PinVisible}` |");
sb.AppendLine($"| Renderer connected | {t.Connected} |");
sb.AppendLine($"| SamplesFromGetPointer | {t.SamplesFromGetPointer} |");
sb.AppendLine($"| Frames | {t.Frames} |");
sb.AppendLine($"| Size | {t.Width}x{t.Height} |");
sb.AppendLine($"| Graph | `{t.Graph}` |");
sb.AppendLine($"| Error | `{t.Error ?? ""}` |");
sb.AppendLine();
sb.AppendLine("AddFilter + RenderFile only. No Connect, no VMR steal.");

Console.Write(sb.ToString());

var outDir = args.FirstOrDefault(a => !a.StartsWith('-'));
if (outDir is not null)
{
    Directory.CreateDirectory(outDir);
    var name = t.IntPtrSize == 4 ? "x86.md" : "x64.md";
    File.WriteAllText(Path.Combine(outDir, name), sb.ToString());
}

return player is { SamplesFromGetPointer: true, FrameSerial: > 0 } ? 0 : 1;
