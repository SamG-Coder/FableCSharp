using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Fable.Core;

/// <summary>
/// Side-by-side PlayAVI wall-clock log. Same site
/// names as Fable.exe <c>006286F0</c> /
/// <c>00A3B730</c> / <c>00CA4AA0</c>. Observation
/// only — does not pace Receive or Present.
/// </summary>
public static class PlayAviTimeline
{
    public const int WaitObject0 = 0;
    public const int WaitAbandoned = 0x00000080;
    public const int WaitIoCompletion = 0x000000C0;
    public const int WaitTimeout = 0x00000102;
    public const int WaitFailed = -1;

    public const uint SiteCopy = 0x00A3B730;
    public const uint SiteGetSampleTimes = 0x00CA49F0;
    public const uint SiteSchedule = 0x00CA4AA0;
    public const uint SiteAdviseTime = 0x00CA4B07;
    public const uint SiteDoRender = 0x00A3BCF0;
    public const uint SiteExecuteRender = 0x00CA4B20;
    public const uint SiteSetEvent = 0x00A3B8EB;
    public const uint SiteWaitEnter = 0x00628A9E;
    public const uint SiteWaitLeave = 0x00628AAC;
    public const uint SiteBeginScene = 0x009BEF20;
    public const uint SiteBlit = 0x009DC870;
    public const uint SiteEndScene = 0x009BEF50;
    public const uint SitePresentEnter = 0x009BEEB0;
    public const uint SitePresentLeave = 0x009BEF10;
    public const uint SiteOpen = 0x00A3B9D0;
    public const uint SiteClock = 0x00A3BCD0;

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private static readonly object Gate = new();
    private static readonly List<PlayAviTlEvent> Buffer = [];
    private static long _startQpc;
    private static bool _enabled = true;
    private static string _side = "csharp";
    private static string _path = "";
    private static string _clock = "";
    private static string _presentParams = "";

    public static bool Enabled
    {
        get { lock (Gate) return _enabled; }
        set
        {
            lock (Gate)
            {
                _enabled = value;
                if (value && _startQpc == 0)
                    _startQpc = Stopwatch.GetTimestamp();
            }
        }
    }

    public static void Reset(string side = "csharp")
    {
        lock (Gate)
        {
            Buffer.Clear();
            _startQpc = Stopwatch.GetTimestamp();
            _enabled = true;
            _side = side;
            _path = "";
            _clock = "";
            _presentParams = "";
        }
    }

    public static void NotePath(string path)
    {
        lock (Gate) _path = path;
        Note("open", SiteOpen, extra: path);
    }

    public static void NoteClock(string clock)
    {
        lock (Gate) _clock = clock;
        Note("clock", SiteClock, extra: clock);
    }

    public static void NotePresentParams(string text)
    {
        lock (Gate) _presentParams = text;
        Note("d3dpp", 0, extra: text);
    }

    public static void Note(
        string kind,
        uint site,
        int serial = 0,
        long sampleStartHns = 0,
        long sampleEndHns = 0,
        int waitResult = int.MinValue,
        int threadId = 0,
        string? extra = null)
    {
        if (!_enabled)
            return;
        var qpc = Stopwatch.GetTimestamp();
        long start;
        lock (Gate)
        {
            if (!_enabled)
                return;
            if (Buffer.Count >= 250_000)
                return;
            start = _startQpc == 0 ? qpc : _startQpc;
            if (_startQpc == 0)
                _startQpc = qpc;
            Buffer.Add(new PlayAviTlEvent
            {
                Qpc = qpc,
                WallMs = (qpc - start) * 1000.0 / Stopwatch.Frequency,
                Kind = kind,
                Site = site,
                Serial = serial,
                SampleStartHns = sampleStartHns,
                SampleEndHns = sampleEndHns,
                WaitResult = waitResult,
                ThreadId = threadId == 0 ? Environment.CurrentManagedThreadId : threadId,
                Extra = extra,
            });
        }
    }

    public static IReadOnlyList<PlayAviTlEvent> Snapshot()
    {
        lock (Gate)
            return Buffer.ToArray();
    }

    public static string DefaultOutDir()
    {
        var env = Environment.GetEnvironmentVariable("FABLE_PLAYAVI_TIMELINE");
        if (!string.IsNullOrWhiteSpace(env))
            return env;
        var here = Path.GetDirectoryName(typeof(PlayAviTimeline).Assembly.Location) ?? ".";
        return Path.GetFullPath(Path.Combine(
            here, "..", "..", "..", "..", "tools", "Fable.ExeIndex",
            "out", "01-sections", "playavi-timeline"));
    }

    public static string Write(string? dir = null, string? name = null)
    {
        dir ??= DefaultOutDir();
        Directory.CreateDirectory(dir);
        name ??= _side;
        var events = Snapshot();
        string path;
        string clock;
        string present;
        string side;
        lock (Gate)
        {
            path = _path;
            clock = _clock;
            present = _presentParams;
            side = _side;
        }

        var jsonl = Path.Combine(dir, $"{name}.jsonl");
        using (var w = new StreamWriter(jsonl, false, Encoding.UTF8))
        {
            foreach (var e in events)
                w.WriteLine(JsonSerializer.Serialize(e, JsonOptions));
        }

        var summary = Summarize(events, side, path, clock, present);
        var md = Path.Combine(dir, $"{name}.md");
        File.WriteAllText(md, summary);
        return md;
    }

    public static string WriteComparison(string dir, IReadOnlyList<PlayAviTlEvent> exe, IReadOnlyList<PlayAviTlEvent> csharp)
    {
        Directory.CreateDirectory(dir);
        var sb = new StringBuilder();
        sb.AppendLine("# PlayAVI timeline comparison");
        sb.AppendLine();
        sb.AppendLine("Wall-clock events. Not an inferred reconstruction.");
        sb.AppendLine();
        sb.AppendLine("## Metrics");
        sb.AppendLine();
        sb.AppendLine("| metric | Fable.exe | FableCSharp |");
        sb.AppendLine("|---|---|---|");
        var em = Measure(exe);
        var cm = Measure(csharp);
        foreach (var row in MetricRows(em, cm))
            sb.AppendLine($"| {row.Name} | {row.Left} | {row.Right} |");

        sb.AppendLine();
        sb.AppendLine("## Sample of aligned events");
        sb.AppendLine();
        sb.AppendLine("First 40 copy/wait/present rows after the first copy.");
        sb.AppendLine();
        sb.AppendLine("### Fable.exe");
        sb.AppendLine();
        AppendSample(sb, exe);
        sb.AppendLine();
        sb.AppendLine("### FableCSharp");
        sb.AppendLine();
        AppendSample(sb, csharp);
        sb.AppendLine();
        sb.AppendLine("## Where they diverge");
        sb.AppendLine();
        sb.AppendLine(DescribeDiverge(em, cm));

        var path = Path.Combine(dir, "compare.md");
        File.WriteAllText(path, sb.ToString());
        return path;
    }

    public static PlayAviTlMetrics Measure(IReadOnlyList<PlayAviTlEvent> events)
    {
        var copies = events.Where(e => e.Kind == "copy").ToList();
        var presents = events.Where(e => e.Kind == "present-leave" || e.Kind == "present").ToList();
        if (presents.Count == 0)
            presents = events.Where(e => e.Kind == "present-enter").ToList();
        var waits = events.Where(e => e.Kind == "wait-leave").ToList();
        var gettimes = events.Where(e => e.Kind == "gettime").ToList();
        var advise = events.Count(e => e.Kind == "advise");
        var dorender = events.Count(e => e.Kind == "dorender");

        var sampleIntervals = Deltas(copies);
        var presentIntervals = Deltas(presents);
        var latencies = new List<double>();
        var presentsPerSample = new List<int>();
        var skipped = 0;
        var lastSerial = 0;
        var presentOf = new Dictionary<int, int>();
        foreach (var p in presents)
        {
            if (p.Serial == 0)
                continue;
            presentOf[p.Serial] = presentOf.GetValueOrDefault(p.Serial) + 1;
        }

        for (var i = 0; i < copies.Count; i++)
        {
            var c = copies[i];
            if (lastSerial != 0 && c.Serial > lastSerial + 1)
                skipped += c.Serial - lastSerial - 1;
            lastSerial = c.Serial;
            var next = presents.FirstOrDefault(p => p.Serial == c.Serial && p.WallMs >= c.WallMs - 0.05);
            if (next is not null)
                latencies.Add(next.WallMs - c.WallMs);
            if (presentOf.TryGetValue(c.Serial, out var n))
                presentsPerSample.Add(n);
        }

        var phase = new List<double>();
        foreach (var g in gettimes)
        {
            if (g.SampleStartHns <= 0)
                continue;
            var mediaMs = g.SampleStartHns / 10_000.0;
            phase.Add(mediaMs - g.WallMs);
        }

        return new PlayAviTlMetrics
        {
            Events = events.Count,
            Copies = copies.Count,
            Presents = presents.Count,
            Waits = waits.Count,
            GetTimes = gettimes.Count,
            AdviseTimeHits = advise,
            DoRenderHits = dorender,
            WaitSignaled = waits.Count(w => w.WaitResult == WaitObject0),
            WaitTimeout = waits.Count(w => w.WaitResult == WaitTimeout),
            WaitApc = waits.Count(w => w.WaitResult == WaitIoCompletion),
            WaitOther = waits.Count(w =>
                w.WaitResult != WaitObject0 &&
                w.WaitResult != WaitTimeout &&
                w.WaitResult != WaitIoCompletion &&
                w.WaitResult != int.MinValue),
            SampleInterval = Dist(sampleIntervals),
            PresentInterval = Dist(presentIntervals),
            SampleToPresentMs = Dist(latencies),
            PresentsPerSample = Dist(presentsPerSample.Select(n => (double)n).ToList()),
            SkippedSamples = skipped,
            RepeatedPresents = presentOf.Count(kv => kv.Value > 1),
            MaxPresentsPerSample = presentOf.Count == 0 ? 0 : presentOf.Values.Max(),
            PhaseErrorMs = Dist(phase),
            PhaseDriftMs = phase.Count >= 2 ? phase[^1] - phase[0] : 0,
            FirstCopyMs = copies.Count == 0 ? 0 : copies[0].WallMs,
            LastCopyMs = copies.Count == 0 ? 0 : copies[^1].WallMs,
            SpanMs = events.Count == 0 ? 0 : events[^1].WallMs - events[0].WallMs,
        };
    }

    public static string Summarize(
        IReadOnlyList<PlayAviTlEvent> events,
        string side,
        string path,
        string clock,
        string presentParams)
    {
        var m = Measure(events);
        var sb = new StringBuilder();
        sb.AppendLine($"# PlayAVI timeline ({side})");
        sb.AppendLine();
        sb.AppendLine($"path `{path}`");
        sb.AppendLine();
        sb.AppendLine($"clock `{clock}`");
        sb.AppendLine();
        sb.AppendLine($"D3DPRESENT_PARAMETERS `{presentParams}`");
        sb.AppendLine();
        sb.AppendLine("| metric | value |");
        sb.AppendLine("|---|---|");
        sb.AppendLine($"| events | {m.Events} |");
        sb.AppendLine($"| copies | {m.Copies} |");
        sb.AppendLine($"| presents | {m.Presents} |");
        sb.AppendLine($"| waits | {m.Waits} |");
        sb.AppendLine($"| gettime | {m.GetTimes} |");
        sb.AppendLine($"| AdviseTime hits | {m.AdviseTimeHits} |");
        sb.AppendLine($"| DoRenderSample hits | {m.DoRenderHits} |");
        sb.AppendLine($"| WaitEx signaled / timeout / APC / other | {m.WaitSignaled} / {m.WaitTimeout} / {m.WaitApc} / {m.WaitOther} |");
        sb.AppendLine($"| sample interval ms | {Fmt(m.SampleInterval)} |");
        sb.AppendLine($"| Present interval ms | {Fmt(m.PresentInterval)} |");
        sb.AppendLine($"| sample→Present ms | {Fmt(m.SampleToPresentMs)} |");
        sb.AppendLine($"| Presents per sample | {Fmt(m.PresentsPerSample)} |");
        sb.AppendLine($"| skipped samples | {m.SkippedSamples} |");
        sb.AppendLine($"| samples presented more than once | {m.RepeatedPresents} |");
        sb.AppendLine($"| max Presents per sample | {m.MaxPresentsPerSample} |");
        sb.AppendLine($"| phase error (media−wall) ms | {Fmt(m.PhaseErrorMs)} |");
        sb.AppendLine($"| phase drift ms (last−first) | {m.PhaseDriftMs:F2} |");
        sb.AppendLine($"| copy span ms | {m.LastCopyMs - m.FirstCopyMs:F1} |");
        sb.AppendLine($"| log span ms | {m.SpanMs:F1} |");
        sb.AppendLine();
        sb.AppendLine("## Events");
        sb.AppendLine();
        sb.AppendLine("| wall_ms | kind | site | serial | start_hns | end_hns | wait | tid | extra |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|---|");
        var n = 0;
        foreach (var e in events)
        {
            if (n >= 2500)
            {
                sb.AppendLine($"| … | {events.Count - n} more | | | | | | | |");
                break;
            }

            sb.AppendLine(
                $"| {e.WallMs:F3} | {e.Kind} | `{e.Site:X8}` | {e.Serial} | {e.SampleStartHns} | {e.SampleEndHns} | {WaitName(e.WaitResult)} | {e.ThreadId} | {e.Extra} |");
            n++;
        }

        return sb.ToString();
    }

    public static string WaitName(int result) => result switch
    {
        int.MinValue => "",
        WaitObject0 => "signaled",
        WaitTimeout => "timeout",
        WaitIoCompletion => "apc",
        WaitAbandoned => "abandoned",
        WaitFailed => "failed",
        _ => result.ToString(CultureInfo.InvariantCulture),
    };

    public static IReadOnlyList<PlayAviTlEvent> LoadJsonl(string path)
    {
        if (!File.Exists(path))
            return [];
        var list = new List<PlayAviTlEvent>();
        foreach (var line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            var ev = JsonSerializer.Deserialize<PlayAviTlEvent>(line, JsonOptions);
            if (ev is not null)
                list.Add(ev);
        }

        return list;
    }

    private static void AppendSample(StringBuilder sb, IReadOnlyList<PlayAviTlEvent> events)
    {
        sb.AppendLine("```");
        var shown = 0;
        var started = false;
        foreach (var e in events)
        {
            if (!started)
            {
                if (e.Kind != "copy")
                    continue;
                started = true;
            }

            if (e.Kind is not ("copy" or "gettime" or "setevent" or "wait-enter" or "wait-leave"
                or "beginscene" or "blit" or "endscene" or "present-enter" or "present-leave"
                or "present" or "advise" or "dorender"))
                continue;
            sb.AppendLine(
                $"{e.WallMs,10:F3}  {e.Kind,-14} serial={e.Serial} start={e.SampleStartHns} wait={WaitName(e.WaitResult)} {e.Extra}");
            shown++;
            if (shown >= 40)
                break;
        }

        if (shown == 0)
            sb.AppendLine("(no copy/wait/present events)");
        sb.AppendLine("```");
    }

    private static string DescribeDiverge(PlayAviTlMetrics exe, PlayAviTlMetrics csharp)
    {
        var sb = new StringBuilder();
        if (exe.Copies == 0)
            sb.AppendLine("Fable.exe log has no `copy` events — live attach did not see `00A3B730`.");
        if (csharp.Copies == 0)
            sb.AppendLine("FableCSharp log has no `copy` events.");
        if (exe.Copies == 0 || csharp.Copies == 0)
            return sb.ToString();

        void Cmp(string name, double a, double b, double slack)
        {
            if (Math.Abs(a - b) > slack)
                sb.AppendLine($"- {name}: exe {a:F2} vs csharp {b:F2}");
        }

        Cmp("sample interval median ms", exe.SampleInterval.Median, csharp.SampleInterval.Median, 2);
        Cmp("Present interval median ms", exe.PresentInterval.Median, csharp.PresentInterval.Median, 2);
        Cmp("sample→Present median ms", exe.SampleToPresentMs.Median, csharp.SampleToPresentMs.Median, 4);
        Cmp("Presents per sample mean", exe.PresentsPerSample.Mean, csharp.PresentsPerSample.Mean, 0.15);
        Cmp("phase drift ms", exe.PhaseDriftMs, csharp.PhaseDriftMs, 30);
        if (exe.AdviseTimeHits != csharp.AdviseTimeHits)
            sb.AppendLine($"- AdviseTime hits: exe {exe.AdviseTimeHits} vs csharp {csharp.AdviseTimeHits}");
        if (exe.WaitTimeout == 0 && csharp.WaitTimeout > exe.Waits / 10)
            sb.AppendLine("- FableCSharp WaitEx times out often; Fable.exe almost never does.");
        if (exe.WaitTimeout > 0 && csharp.WaitTimeout == 0)
            sb.AppendLine("- Fable.exe WaitEx times out; FableCSharp does not.");
        if (sb.Length == 0)
            sb.AppendLine("Medians are within slack. Inspect the raw jsonl for the wave.");
        return sb.ToString();
    }

    private static IEnumerable<(string Name, string Left, string Right)> MetricRows(
        PlayAviTlMetrics a, PlayAviTlMetrics b)
    {
        yield return ("copies", a.Copies.ToString(), b.Copies.ToString());
        yield return ("presents", a.Presents.ToString(), b.Presents.ToString());
        yield return ("WaitEx signaled/timeout/APC",
            $"{a.WaitSignaled}/{a.WaitTimeout}/{a.WaitApc}",
            $"{b.WaitSignaled}/{b.WaitTimeout}/{b.WaitApc}");
        yield return ("sample interval ms", Fmt(a.SampleInterval), Fmt(b.SampleInterval));
        yield return ("Present interval ms", Fmt(a.PresentInterval), Fmt(b.PresentInterval));
        yield return ("sample→Present ms", Fmt(a.SampleToPresentMs), Fmt(b.SampleToPresentMs));
        yield return ("Presents per sample", Fmt(a.PresentsPerSample), Fmt(b.PresentsPerSample));
        yield return ("skipped samples", a.SkippedSamples.ToString(), b.SkippedSamples.ToString());
        yield return ("repeated Presents", a.RepeatedPresents.ToString(), b.RepeatedPresents.ToString());
        yield return ("AdviseTime hits", a.AdviseTimeHits.ToString(), b.AdviseTimeHits.ToString());
        yield return ("phase drift ms", a.PhaseDriftMs.ToString("F2"), b.PhaseDriftMs.ToString("F2"));
        yield return ("copy span ms", (a.LastCopyMs - a.FirstCopyMs).ToString("F1"), (b.LastCopyMs - b.FirstCopyMs).ToString("F1"));
    }

    private static List<double> Deltas(List<PlayAviTlEvent> events)
    {
        var d = new List<double>(Math.Max(0, events.Count - 1));
        for (var i = 1; i < events.Count; i++)
            d.Add(events[i].WallMs - events[i - 1].WallMs);
        return d;
    }

    private static PlayAviTlDist Dist(IReadOnlyList<double> values)
    {
        if (values.Count == 0)
            return new PlayAviTlDist();
        var sorted = values.OrderBy(v => v).ToArray();
        return new PlayAviTlDist
        {
            N = sorted.Length,
            Min = sorted[0],
            P10 = Percentile(sorted, 0.10),
            Median = Percentile(sorted, 0.50),
            P90 = Percentile(sorted, 0.90),
            Max = sorted[^1],
            Mean = sorted.Average(),
        };
    }

    private static double Percentile(double[] sorted, double p)
    {
        if (sorted.Length == 1)
            return sorted[0];
        var i = (sorted.Length - 1) * p;
        var lo = (int)Math.Floor(i);
        var hi = (int)Math.Ceiling(i);
        if (lo == hi)
            return sorted[lo];
        return sorted[lo] + (sorted[hi] - sorted[lo]) * (i - lo);
    }

    private static string Fmt(PlayAviTlDist d)
    {
        if (d.N == 0)
            return "—";
        return $"n={d.N} min={d.Min:F2} p10={d.P10:F2} med={d.Median:F2} p90={d.P90:F2} max={d.Max:F2} mean={d.Mean:F2}";
    }
}

public sealed class PlayAviTlEvent
{
    public long Qpc { get; init; }
    public double WallMs { get; init; }
    public string Kind { get; init; } = "";
    public uint Site { get; init; }
    public int Serial { get; init; }
    public long SampleStartHns { get; init; }
    public long SampleEndHns { get; init; }
    public int WaitResult { get; init; } = int.MinValue;
    public int ThreadId { get; init; }
    public string? Extra { get; init; }
}

public sealed class PlayAviTlDist
{
    public int N { get; init; }
    public double Min { get; init; }
    public double P10 { get; init; }
    public double Median { get; init; }
    public double P90 { get; init; }
    public double Max { get; init; }
    public double Mean { get; init; }
}

public sealed class PlayAviTlMetrics
{
    public int Events { get; init; }
    public int Copies { get; init; }
    public int Presents { get; init; }
    public int Waits { get; init; }
    public int GetTimes { get; init; }
    public int AdviseTimeHits { get; init; }
    public int DoRenderHits { get; init; }
    public int WaitSignaled { get; init; }
    public int WaitTimeout { get; init; }
    public int WaitApc { get; init; }
    public int WaitOther { get; init; }
    public PlayAviTlDist SampleInterval { get; init; } = new();
    public PlayAviTlDist PresentInterval { get; init; } = new();
    public PlayAviTlDist SampleToPresentMs { get; init; } = new();
    public PlayAviTlDist PresentsPerSample { get; init; } = new();
    public int SkippedSamples { get; init; }
    public int RepeatedPresents { get; init; }
    public int MaxPresentsPerSample { get; init; }
    public PlayAviTlDist PhaseErrorMs { get; init; } = new();
    public double PhaseDriftMs { get; init; }
    public double FirstCopyMs { get; init; }
    public double LastCopyMs { get; init; }
    public double SpanMs { get; init; }
}
