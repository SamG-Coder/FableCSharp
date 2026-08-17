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
sb.AppendLine($"| IMediaPosition QI | {t.MediaPositionQi} |");
sb.AppendLine($"| IMediaSeeking QI | {t.MediaSeekingQi} |");
sb.AppendLine($"| IOverlay QI | {t.OverlayQi} |");
sb.AppendLine($"| Filter QI | `{t.FilterQi}` |");
sb.AppendLine($"| Pin QI | `{t.PinQi}` |");
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

if (args.Any(a => a is "--pace"))
{
    if (player is null)
        return 1;
    var deadline = Environment.TickCount64 + 180_000;
    while (!player.Ended && Environment.TickCount64 < deadline)
        player.TryAdvance(0.016f);
    var paceDir = Path.Combine(
        Path.GetDirectoryName(typeof(Program).Assembly.Location) ?? ".",
        "..", "..", "..", "..", "Fable.ExeIndex", "out", "01-sections", "playavi-pace");
    if (outDir is not null)
        paceDir = outDir;
    Directory.CreateDirectory(paceDir);
    var md = new StringBuilder();
    md.AppendLine("# PlayAVI pace samples");
    md.AppendLine();
    md.AppendLine("`00628A9E` is `WaitForSingleObjectEx([player+124], 33, TRUE)`:");
    md.AppendLine("wait until SetEvent or 33 ms. Not a fixed sleep.");
    md.AppendLine("`00A3B8EB` `SetEvent` after GetPointer copy on the");
    md.AppendLine("DirectShow Receive thread. `006286F0` presents the");
    md.AppendLine("latest completed sample. No pacing change in this run.");
    md.AppendLine();
    md.AppendLine($"ended `{player.Ended}` serial `{player.FrameSerial}`");
    md.AppendLine();
    md.AppendLine("| n | recv | gp | serial | sampleStart_hns | wall_ms | recv_ms | copy_ms | wait_ms | heap | gen0 | gen1 | gen2 | ws | priv | scratch | rgba | fqi_chars | pqi_chars |");
    md.AppendLine("|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|");
    foreach (var s in WmvPlayer.PaceSamples)
    {
        md.AppendLine(
            $"| {s.Receive} | {s.Receive} | {s.GetPointer} | {s.FrameSerial} | {s.SampleStartHns} | {s.WallMs:F1} | {s.ReceiveMs:F3} | {s.CopyMs:F3} | {s.PresentWaitMs:F1} | {s.HeapBytes} | {s.Gen0} | {s.Gen1} | {s.Gen2} | {s.WorkingSet} | {s.PrivateBytes} | {s.ScratchAllocs} | {s.RgbaAllocs} | {s.FilterQiChars} | {s.PinQiChars} |");
    }

    File.WriteAllText(Path.Combine(paceDir, "samples.md"), md.ToString());
    Console.Write(md.ToString());
}

return player is { SamplesFromGetPointer: true, FrameSerial: > 0 } ? 0 : 1;
