using System.Diagnostics;
using System.Runtime.InteropServices;
using Fable.Core;

namespace Fable.Game;

/// <summary>
/// <c>00A3B9D0</c> DirectShow path when the rewritten
/// name ends <c>.wmv</c> / <c>.asf</c>. CoCreate
/// <c>CLSID_FilterGraph</c> (<c>0x12AB174</c>) +
/// <c>IID_IGraphBuilder</c> (<c>0x12A9934</c>),
/// <c>AddFilter</c> a renderer (<c>00A3B510</c>),
/// <c>RenderFile</c> vtbl+52, QI
/// <c>IMediaControl</c> / <c>IMediaPosition</c> /
/// <c>IMediaEvent</c>, <c>put_CurrentPosition(0)</c>
/// then <c>Run</c> vtbl+28 up to 50 times
/// (<c>00A3B130</c>). Samples are
/// <c>IMediaSample::GetPointer</c>, not
/// <c>IMFSample</c>. EOF is <c>EC_COMPLETE</c> (1).
/// </summary>
public sealed class WmvPlayer : IDisposable
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public byte[]? Rgba { get; private set; }
    public bool Ended { get; private set; }
    /// <summary>
    /// Incremented by <c>00A3B730</c> each sample
    /// copy. Present <c>006286F0</c> reads the
    /// latest frame after the 33 ms wait.
    /// </summary>
    public int FrameSerial { get; private set; }
    /// <summary>Decoder queue writes, not the shown frame.</summary>
    public int RecvSerial => _recvSerial;
    internal static int SampleRcwRelease { get; set; }
    internal static int SampleHeldAtReturn { get; set; }
    public static int SyncSourceCalls { get; set; }
    public static int FilterRunCalls { get; set; }
    public static int AdviseCalls { get; set; }
    public static int AdviseHr { get; set; }
    public static long LastTStart { get; set; }
    public static long LastSampleStart { get; set; }

    public static int GetTimeEnter { get; set; }
    public static int GetTimeLeave { get; set; }
    public static int GetTimeHr { get; set; }
    public static double LastGetTimeMs { get; set; }
    public static int GetPointerEnter { get; set; }
    public static int GetPointerLeave { get; set; }
    public static double LastGetPointerMs { get; set; }
    public static double VtblGetTimeMs { get; set; }
    public static double RcwGetTimeMs { get; set; }
    public static bool RcwGetTimeHangs { get; set; }
    public static bool ComparedGetTime { get; set; }
    /// <summary>
    /// True when pixels came from
    /// <c>IMediaSample::GetPointer</c> (<c>00A3B730</c>),
    /// not <c>IBasicVideo.GetCurrentImage</c>.
    /// </summary>
    public bool SamplesFromGetPointer { get; private set; }
    internal static int QueryAcceptCalls { get; set; }
    internal static int ReceiveConnectionCalls { get; set; }
    internal static int EnumPinsCalls { get; set; }
    internal static int PinNextCalls { get; set; }
    internal static int QueryDirectionCalls { get; set; }
    internal static int MemInputQiCalls { get; set; }
    internal static int ConnectedToCalls { get; set; }
    internal static int MiscFlagsCalls { get; set; }
    internal static int QueryPinInfoCalls { get; set; }
    internal static int QueryIdCalls { get; set; }
    internal static int EnumMediaTypesCalls { get; set; }
    internal static int MediaTypeNextCalls { get; set; }
    internal static int ReceiveCalls { get; set; }
    internal static int GetPointerCalls { get; set; }
    internal static int ConnectCalls { get; set; }
    internal static int FilterQiCalls { get; set; }
    internal static int PinQiCalls { get; set; }
    internal static int MediaPositionQiCalls { get; set; }
    internal static int MediaSeekingQiCalls { get; set; }
    internal static int OverlayQiCalls { get; set; }
    internal static bool CaptureQi { get; set; }
    internal static string LastFilterQi { get; set; } = "";
    internal static string LastPinQi { get; set; } = "";
    internal static long PresentWaitTicks { get; set; }
    internal static int ScratchAllocs { get; set; }
    internal static int RgbaAllocs { get; set; }
    public static readonly List<PlayAviPaceSample> PaceSamples = [];
    private static readonly long PaceStart = Stopwatch.GetTimestamp();

    public static string? LastError { get; private set; }
    public static int LastAddFilterHr { get; private set; }
    public static int LastRenderFileHr { get; private set; }
    public static int LastRunHr { get; private set; }

    /// <summary>
    /// Live AddFilter+RenderFile counters for the
    /// x86/x64 quartz experiment. Observation only.
    /// </summary>
    public static PlayAviGraphTrace CaptureTrace() =>
        new()
        {
            ProcessArch = Environment.Is64BitProcess ? "x64" : "x86",
            IntPtrSize = IntPtr.Size,
            AddFilterHr = LastAddFilterHr,
            RenderFileHr = LastRenderFileHr,
            RunHr = LastRunHr,
            EnumPins = EnumPinsCalls,
            Next = PinNextCalls,
            QueryDirection = QueryDirectionCalls,
            ConnectedTo = ConnectedToCalls,
            QueryPinInfo = QueryPinInfoCalls,
            QueryId = QueryIdCalls,
            EnumMediaTypes = EnumMediaTypesCalls,
            MediaTypeNext = MediaTypeNextCalls,
            QueryAccept = QueryAcceptCalls,
            ReceiveConnection = ReceiveConnectionCalls,
            MemInputQi = MemInputQiCalls,
            Receive = ReceiveCalls,
            GetPointer = GetPointerCalls,
            SampleRcwRelease = SampleRcwRelease,
            SampleHeldAtReturn = SampleHeldAtReturn,
            Connect = ConnectCalls,
            MiscFlags = MiscFlagsCalls,
            FilterQi = LastFilterQi,
            PinQi = LastPinQi,
            MediaPositionQi = MediaPositionQiCalls,
            MediaSeekingQi = MediaSeekingQiCalls,
            OverlayQi = OverlayQiCalls,
            Graph = LastGraph,
            PinVisible = LastPinVisible,
            Connected = LastConnected,
            SamplesFromGetPointer = LastSamplesFromGetPointer,
            Frames = LastFrames,
            Width = LastWidth,
            Height = LastHeight,
            Error = LastError,
        };

    public static string LastGraph { get; private set; } = "";
    public static string LastPinVisible { get; private set; } = "";
    public static bool LastConnected { get; private set; }
    public static bool LastSamplesFromGetPointer { get; private set; }
    public static int LastFrames { get; private set; }
    public static int LastWidth { get; private set; }
    public static int LastHeight { get; private set; }

    private readonly object _gate = new();
    private readonly AutoResetEvent _frameEvent = new(false);
    private int _recvSerial;
    private Thread? _thread;
    private volatile bool _stop;
    private IGraphBuilder? _graph;
    private IMediaControl? _control;
    private IMediaEvent? _events;
    private IMediaPosition? _position;
    private TextureRenderer? _renderer;

    public static WmvPlayer? TryOpen(string path)
    {
        LastError = null;
        QueryAcceptCalls = 0;
        ReceiveConnectionCalls = 0;
        EnumPinsCalls = 0;
        PinNextCalls = 0;
        QueryDirectionCalls = 0;
        MemInputQiCalls = 0;
        ConnectedToCalls = 0;
        MiscFlagsCalls = 0;
        QueryPinInfoCalls = 0;
        QueryIdCalls = 0;
        EnumMediaTypesCalls = 0;
        MediaTypeNextCalls = 0;
        ReceiveCalls = 0;
        GetPointerCalls = 0;
        ConnectCalls = 0;
        FilterQiCalls = 0;
        PinQiCalls = 0;
        MediaPositionQiCalls = 0;
        MediaSeekingQiCalls = 0;
        OverlayQiCalls = 0;
        CaptureQi = true;
        LastFilterQi = "";
        LastPinQi = "";
        PresentWaitTicks = 0;
        ScratchAllocs = 0;
        RgbaAllocs = 0;
        SampleRcwRelease = 0;
        SampleHeldAtReturn = 0;
        SyncSourceCalls = 0;
        FilterRunCalls = 0;
        AdviseCalls = 0;
        AdviseHr = 0;
        LastTStart = 0;
        LastSampleStart = 0;

        GetTimeEnter = 0;
        GetTimeLeave = 0;
        GetTimeHr = 0;
        LastGetTimeMs = 0;
        GetPointerEnter = 0;
        GetPointerLeave = 0;
        LastGetPointerMs = 0;
        VtblGetTimeMs = 0;
        RcwGetTimeMs = 0;
        RcwGetTimeHangs = false;
        ComparedGetTime = false;
        PaceSamples.Clear();
        PlayAviWave.Reset();
        LastAddFilterHr = 0;
        LastRenderFileHr = 0;
        LastRunHr = 0;
        LastGraph = "";
        LastPinVisible = "";
        LastConnected = false;
        LastSamplesFromGetPointer = false;
        LastFrames = 0;
        LastWidth = 0;
        LastHeight = 0;
        if (!File.Exists(path) || !RegionTravel.FileHasAsfMagic(path))
        {
            LastError = "missing-or-not-asf";
            return null;
        }

        var player = new WmvPlayer();
        var ready = new ManualResetEventSlim(false);
        string? startError = null;
        player._thread = new Thread(() =>
        {
            var hr = Ole32.CoInitializeEx(IntPtr.Zero, Ole32.ApartmentThreaded);
            if (hr < 0 && hr != Ole32.RpcEChangedMode)
            {
                startError = $"CoInitializeEx {hr:X8}";
                ready.Set();
                return;
            }

            try
            {
                startError = player.BuildGraph(path);
            }
            catch (Exception ex)
            {
                startError = ex.GetType().Name + ": " + ex.Message;
            }

            ready.Set();
            if (startError is null)
                player.Pump();
            player.TearDown();
            Ole32.CoUninitialize();
        })
        {
            IsBackground = true,
            Name = "FableWmv",
        };
        player._thread.SetApartmentState(ApartmentState.STA);
        player._thread.Start();
        if (!ready.Wait(TimeSpan.FromSeconds(8)))
        {
            LastError = "sta-timeout";
            player.Dispose();
            return null;
        }

        if (startError is not null || player.Rgba is null || player.Width < 16)
        {
            LastError = startError ?? "no-sample";
            LastConnected = player._renderer?.IsConnected ?? false;
            LastSamplesFromGetPointer = player.SamplesFromGetPointer;
            LastFrames = player.FrameSerial;
            LastWidth = player.Width;
            LastHeight = player.Height;
            player.Dispose();
            return null;
        }

        LastConnected = true;
        LastSamplesFromGetPointer = player.SamplesFromGetPointer;
        LastFrames = player.FrameSerial;
        PlayAviTimeline.NotePath(path);
        LastWidth = player.Width;
        LastHeight = player.Height;
        return player;
    }

    /// <summary>
    /// <c>00628A9E</c> WaitEx then the latest
    /// GetPointer frame. DirectShow owns
    /// CBaseRenderer scheduling; this wait is
    /// only the Fable present pump.
    /// </summary>
    public bool TryAdvance(float dt)
    {
        if (Ended)
            return false;
        if (dt > 0f)
            _ = dt;
        var wait0 = Stopwatch.GetTimestamp();
        var serialBefore = FrameSerial;
        var recvBefore = ReceiveCalls;
        var signaled = _frameEvent.WaitOne(RegionTravel.PlayAviPresentMs);
        PresentWaitTicks += Stopwatch.GetTimestamp() - wait0;
        var recvSince = ReceiveCalls - recvBefore;
        PlayAviWave.Wait(signaled, serialBefore, FrameSerial, recvSince);
        PlayAviWave.Present(FrameSerial);
        return Rgba is not null;
    }

    public void Dispose()
    {
        _stop = true;
        _renderer?.Stop();
        _frameEvent.Set();
        if (_thread is { IsAlive: true } &&
            !_thread.Join(TimeSpan.FromSeconds(2)))
            LastError ??= "sta-join";
        _thread = null;
        Rgba = null;
        Width = 0;
        Height = 0;
        Ended = true;
        try { _frameEvent.Dispose(); } catch { /* already */ }
    }

    private string? BuildGraph(string path)
    {
        var clsid = Ds.FilterGraph;
        var iid = typeof(IGraphBuilder).GUID;
        var hr = Ole32.CoCreateInstance(
            ref clsid, IntPtr.Zero, Ole32.InprocServer, ref iid, out var graphUnk);
        if (hr < 0 || graphUnk == IntPtr.Zero)
            return $"CoCreate FilterGraph {hr:X8}";

        _graph = (IGraphBuilder)Marshal.GetObjectForIUnknown(graphUnk);
        Marshal.Release(graphUnk);

        // 00A3B9D0: alloc 00A3B510 renderer, AddFilter
        // vtbl+12 name 0x129D1AC, RenderFile vtbl+52
        // when the path is .wmv/.asf. Open QI is
        // IMediaControl / IMediaPosition /
        // IMediaSeeking / IMediaEvent / IBasicAudio
        // — not IVideoWindow. Pixels are
        // IMediaSample::GetPointer in 00A3B730.
        _renderer = new TextureRenderer(OnGetPointerSample);
        hr = _graph.AddFilter(_renderer, RegionTravel.PlayAviFilterName);
        LastAddFilterHr = hr;
        if (hr < 0)
            return $"AddFilter {hr:X8}";

        hr = _graph.RenderFile(path, null);
        CaptureQi = false;
        LastRenderFileHr = hr;
        LastGraph = GraphSummary();
        LastPinVisible = GraphPinVisible();
        if (hr < 0)
            return $"RenderFile {hr:X8}";

        _control = (IMediaControl)_graph;
        _position = (IMediaPosition)_graph;
        _events = (IMediaEvent)_graph;

        // 00A3B130: put_CurrentPosition(0) then Run, retry 50.
        for (var i = 0; i < 8 && _position is not null; i++)
        {
            hr = _position.put_CurrentPosition(0d);
            if (hr == 0)
                break;
        }

        for (var i = 0; i < RegionTravel.PlayAviRunRetry; i++)
        {
            hr = _control.Run();
            if (hr >= 0)
                break;
        }

        LastRunHr = hr;
        if (hr < 0)
            return $"Run {hr:X8}";

        // IMediaControl.Run should call IBaseFilter.Run
        // (00CA68F0). If the CCW never saw that, arm
        // tStart from the graph clock so AdviseTime
        // has the same base 00CAACA0 would store.
        _renderer.EnsureRunning();
        NoteGraphClock();
        LastGraph = GraphSummary();
        LastPinVisible = GraphPinVisible();
        LastConnected = _renderer.IsConnected;
        if (!_renderer.IsConnected)
            return $"renderer-not-connected enumPins={EnumPinsCalls} next={PinNextCalls} dir={QueryDirectionCalls} conn={ConnectedToCalls} qpi={QueryPinInfoCalls} qid={QueryIdCalls} emt={EnumMediaTypesCalls} memqi={MemInputQiCalls} qa={QueryAcceptCalls} rc={ReceiveConnectionCalls} cn={ConnectCalls} misc={MiscFlagsCalls} mp={MediaPositionQiCalls} ms={MediaSeekingQiCalls} ov={OverlayQiCalls} fqi={LastFilterQi} pqi={LastPinQi} vis=" +
                   LastPinVisible + " " + LastGraph;

        var deadline = Environment.TickCount64 + 5000;
        while (Environment.TickCount64 < deadline && !_stop)
        {
            DrainEvents();
            if (_frameEvent.WaitOne(RegionTravel.PlayAviPresentMs) && Rgba is not null)
                return null;
            if (Rgba is not null)
                return null;
        }

        return Rgba is null
            ? $"first-sample-timeout qa={QueryAcceptCalls} rc={ReceiveConnectionCalls} recv={ReceiveCalls} gp={GetPointerCalls} gt={GetTimeEnter}/{GetTimeLeave} hr=0x{GetTimeHr:X8} pts={LastSampleStart} gtMs={LastGetTimeMs:F1} gpMs={LastGetPointerMs:F1} rcwRel={SampleRcwRelease} held={SampleHeldAtReturn} sync={SyncSourceCalls} run={FilterRunCalls} advise={AdviseCalls} ahr=0x{AdviseHr:X8} tStart={LastTStart} vis={LastPinVisible} {LastGraph}"
            : null;
    }

    private static IPin? NextPin(IEnumPins en)
    {
        var slot = IntPtr.Zero;
        if (en.Next(1, ref slot, IntPtr.Zero) != 0 || slot == IntPtr.Zero)
            return null;
        try
        {
            return (IPin)Marshal.GetObjectForIUnknown(slot);
        }
        finally
        {
            Marshal.Release(slot);
        }
    }

    private string GraphSummary()
    {
        if (_graph is null || _graph.EnumFilters(out var en) < 0 || en is null)
            return "no-graph";
        var names = new List<string>();
        var batch = new IBaseFilter[8];
        while (en.Next(1, batch, IntPtr.Zero) == 0)
        {
            var filter = batch[0];
            if (filter is null)
                continue;
            if (filter.QueryFilterInfo(out var info) < 0)
                continue;
            if (info.Graph != IntPtr.Zero)
                Marshal.Release(info.Graph);
            names.Add(info.Name ?? "?");
        }

        return string.Join(",", names);
    }

    /// <summary>
    /// Walk the filter the graph holds (same
    /// IUnknown RenderFile EnumPins) so we can
    /// tell a broken <c>IEnumPins.Next</c> from
    /// a QueryAccept miss.
    /// </summary>
    private string GraphPinVisible()
    {
        if (_graph is null)
            return "no-graph";
        if (_graph.FindFilterByName(RegionTravel.PlayAviFilterName, out var unk) < 0 ||
            unk == IntPtr.Zero)
            return "no-filter";
        try
        {
            var filter = (IBaseFilter)Marshal.GetObjectForIUnknown(unk);
            if (filter.EnumPins(out var en) < 0 || en is null)
                return "enum-fail";
            var pin = NextPin(en);
            if (pin is null)
                return "no-pin";
            pin.QueryDirection(out var dir);
            return dir == PinDirection.Input ? "in-pin" : "out-pin";
        }
        catch (Exception ex)
        {
            return ex.GetType().Name;
        }
        finally
        {
            Marshal.Release(unk);
        }
    }

    private static unsafe void BgrToRgba(
        IntPtr src, int srcStride, byte[] dest, int width, int height, int bpp)
    {
        fixed (byte* d0 = dest)
        {
            for (var y = 0; y < height; y++)
            {
                var s = (byte*)src + y * srcStride;
                var d = d0 + y * width * 4;
                for (var x = 0; x < width; x++)
                {
                    d[0] = s[2];
                    d[1] = s[1];
                    d[2] = s[0];
                    d[3] = 255;
                    s += bpp;
                    d += 4;
                }
            }
        }
    }

    private void Pump()
    {
        // Graph thread delivers 00A3B730 samples.
        // Present wait is TryAdvance (006286F0).
        while (!_stop)
        {
            DrainEvents();
            if (Ended)
                break;
            Thread.Sleep(1);
        }

        try
        {
            _control?.Stop();
        }
        catch
        {
            // Graph may already be torn down.
        }
    }

    private void DrainEvents()
    {
        if (_events is null)
            return;
        while (_events.GetEvent(out var code, out var p1, out var p2, 0) >= 0)
        {
            _events.FreeEventParams(code, p1, p2);
            if (code == RegionTravel.PlayAviEcComplete)
            {
                Ended = true;
                break;
            }
        }
    }

    private void OnGetPointerSample(int width, int height, byte[] rgba)
    {
        SamplesFromGetPointer = true;
        lock (_gate)
        {
            Width = width;
            Height = height;
            if (Rgba is null || Rgba.Length != rgba.Length)
            {
                RgbaAllocs++;
                Rgba = new byte[rgba.Length];
            }

            rgba.AsSpan().CopyTo(Rgba);
            _recvSerial++;
            FrameSerial = _recvSerial;
            LastFrames = FrameSerial;
        }

        // 00A3B730 SetEvent([player+124])
        PlayAviWave.Set(FrameSerial);
        _frameEvent.Set();
    }

    private void NoteGraphClock()
    {
        // Observation only. Graph clock. Renderer
        // clock is COM 00CA7680 at filter+24.
        if (_graph is null)
        {
            PlayAviTimeline.NoteClock("no-graph");
            return;
        }

        try
        {
            var filter = (IMediaFilter)_graph;
            var hr = filter.GetSyncSource(out var clock);
            if (hr < 0 || clock == IntPtr.Zero)
            {
                PlayAviTimeline.NoteClock($"GetSyncSource hr=0x{hr:X8} clock=0");
                return;
            }

            PlayAviTimeline.NoteClock($"GetSyncSource hr=0x{hr:X8} clock=0x{clock:X} filterRun={FilterRunCalls} sync={SyncSourceCalls} tStart={LastTStart} advise={AdviseCalls}");
            Marshal.Release(clock);
        }
        catch (Exception ex)
        {
            PlayAviTimeline.NoteClock("GetSyncSource " + ex.GetType().Name);
        }
    }

    private void TearDown()
    {
        _events = null;
        _position = null;
        if (_control is not null)
        {
            try { _control.Stop(); } catch { /* already stopped */ }
            _control = null;
        }

        if (_graph is not null)
        {
            Marshal.ReleaseComObject(_graph);
            _graph = null;
        }

        _renderer = null;
    }

    private static class Ole32
    {
        public const int ApartmentThreaded = 2;
        public const int InprocServer = 1;
        public const int RpcEChangedMode = unchecked((int)0x80010106);

        [DllImport("ole32.dll")]
        public static extern int CoInitializeEx(IntPtr reserved, int coInit);

        [DllImport("ole32.dll")]
        public static extern void CoUninitialize();

        [DllImport("ole32.dll")]
        public static extern int CoCreateInstance(
            ref Guid clsid, IntPtr outer, int ctx, ref Guid iid, out IntPtr ppv);

        [DllImport("ole32.dll")]
        public static extern IntPtr CoTaskMemAlloc(IntPtr size);

        [DllImport("ole32.dll")]
        public static extern void CoTaskMemFree(IntPtr mem);
    }

    private static class Ds
    {
        public static readonly Guid FilterGraph = new("e436ebb3-524f-11ce-9f53-0020af0ba770");
        public static readonly Guid Video = new("73646976-0000-0010-8000-00aa00389b71");
        public static readonly Guid Rgb24 = new("e436eb7d-524f-11ce-9f53-0020af0ba770");
        public static readonly Guid VideoInfo = new("05589f80-c356-11ce-bf01-00aa0055595a");
    }

    [ComImport]
    [Guid("56a868a9-0ad4-11ce-b03a-0020af0ba770")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IGraphBuilder
    {
        [PreserveSig] int AddFilter([MarshalAs(UnmanagedType.Interface)] IBaseFilter filter, [MarshalAs(UnmanagedType.LPWStr)] string name);
        [PreserveSig] int RemoveFilter([MarshalAs(UnmanagedType.Interface)] IBaseFilter filter);
        [PreserveSig] int EnumFilters(out IEnumFilters enumerator);
        [PreserveSig] int FindFilterByName([MarshalAs(UnmanagedType.LPWStr)] string name, out IntPtr filter);
        [PreserveSig] int ConnectDirect(IPin outPin, IPin inPin, IntPtr type);
        [PreserveSig] int Reconnect(IPin pin);
        [PreserveSig] int Disconnect(IPin pin);
        [PreserveSig] int SetDefaultSyncSource();
        [PreserveSig] int Connect(IPin outPin, IPin inPin);
        [PreserveSig] int Render(IPin outPin);
        [PreserveSig] int RenderFile([MarshalAs(UnmanagedType.LPWStr)] string file, [MarshalAs(UnmanagedType.LPWStr)] string? playlist);
        [PreserveSig] int AddSourceFilter([MarshalAs(UnmanagedType.LPWStr)] string file, [MarshalAs(UnmanagedType.LPWStr)] string name, out IntPtr filter);
        [PreserveSig] int SetLogFile(IntPtr file);
        [PreserveSig] int Abort();
        [PreserveSig] int ShouldOperationContinue();
    }

    [ComImport]
    [Guid("56a86899-0ad4-11ce-b03a-0020af0ba770")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMediaFilter
    {
        [PreserveSig] int GetClassID(out Guid clsid);
        [PreserveSig] int Stop();
        [PreserveSig] int Pause();
        [PreserveSig] int Run(long start);
        [PreserveSig] int GetState(int timeoutMs, out int state);
        [PreserveSig] int SetSyncSource(IntPtr clock);
        [PreserveSig] int GetSyncSource(out IntPtr clock);
    }

    [ComImport]
    [Guid("56a86893-0ad4-11ce-b03a-0020af0ba770")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IEnumFilters
    {
        [PreserveSig] int Next(int count, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IBaseFilter[] filters, IntPtr fetched);
        [PreserveSig] int Skip(int count);
        [PreserveSig] int Reset();
        [PreserveSig] int Clone(out IEnumFilters enumerator);
    }

    [ComImport]
    [Guid("56a868b1-0ad4-11ce-b03a-0020af0ba770")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMediaControl
    {
        void GetTypeInfoCount(out int count);
        void GetTypeInfo(int itinfo, int lcid, out IntPtr info);
        void GetIDsOfNames(ref Guid iid, IntPtr names, int count, int lcid, IntPtr dispIds);
        void Invoke(int dispId, ref Guid iid, int lcid, short flags, IntPtr dispParams, IntPtr result, IntPtr excep, IntPtr argErr);
        [PreserveSig] int Run();
        [PreserveSig] int Pause();
        [PreserveSig] int Stop();
        [PreserveSig] int GetState(int timeout, out int state);
        [PreserveSig] int RenderFile([MarshalAs(UnmanagedType.BStr)] string file);
        [PreserveSig] int AddSourceFilter([MarshalAs(UnmanagedType.BStr)] string file, [MarshalAs(UnmanagedType.IDispatch)] out object filter);
        [PreserveSig] int get_FilterCollection([MarshalAs(UnmanagedType.IDispatch)] out object collection);
        [PreserveSig] int get_RegFilterCollection([MarshalAs(UnmanagedType.IDispatch)] out object collection);
        [PreserveSig] int StopWhenReady();
    }

    [ComImport]
    [Guid("56a868b2-0ad4-11ce-b03a-0020af0ba770")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMediaPosition
    {
        void GetTypeInfoCount(out int count);
        void GetTypeInfo(int itinfo, int lcid, out IntPtr info);
        void GetIDsOfNames(ref Guid iid, IntPtr names, int count, int lcid, IntPtr dispIds);
        void Invoke(int dispId, ref Guid iid, int lcid, short flags, IntPtr dispParams, IntPtr result, IntPtr excep, IntPtr argErr);
        [PreserveSig] int get_Duration(out double duration);
        [PreserveSig] int put_CurrentPosition(double position);
        [PreserveSig] int get_CurrentPosition(out double position);
        [PreserveSig] int get_StopTime(out double time);
        [PreserveSig] int put_StopTime(double time);
        [PreserveSig] int get_PrerollTime(out double time);
        [PreserveSig] int put_PrerollTime(double time);
        [PreserveSig] int put_Rate(double rate);
        [PreserveSig] int get_Rate(out double rate);
        [PreserveSig] int CanSeekForward(out int can);
        [PreserveSig] int CanSeekBackward(out int can);
    }

    [ComImport]
    [Guid("256a6a22-fbad-11d1-82bf-00a0c9696c8f")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IPinConnection
    {
        [PreserveSig] int DynamicQueryAccept(IntPtr type);
        [PreserveSig] int NotifyEndOfStream(IntPtr notify);
        [PreserveSig] int IsEndPin();
        [PreserveSig] int DynamicDisconnect();
    }

    [ComImport]
    [Guid("56a868b6-0ad4-11ce-b03a-0020af0ba770")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMediaEvent
    {
        void GetTypeInfoCount(out int count);
        void GetTypeInfo(int itinfo, int lcid, out IntPtr info);
        void GetIDsOfNames(ref Guid iid, IntPtr names, int count, int lcid, IntPtr dispIds);
        void Invoke(int dispId, ref Guid iid, int lcid, short flags, IntPtr dispParams, IntPtr result, IntPtr excep, IntPtr argErr);
        [PreserveSig] int GetEventHandle(out IntPtr handle);
        [PreserveSig] int GetEvent(out int code, out IntPtr param1, out IntPtr param2, int timeout);
        [PreserveSig] int WaitForCompletion(int timeout, out int code);
        [PreserveSig] int CancelDefaultHandling(int code);
        [PreserveSig] int RestoreDefaultHandling(int code);
        [PreserveSig] int FreeEventParams(int code, IntPtr param1, IntPtr param2);
    }

    [ComImport]
    [Guid("56a86895-0ad4-11ce-b03a-0020af0ba770")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IBaseFilter
    {
        [PreserveSig] int GetClassID(out Guid clsid);
        [PreserveSig] int Stop();
        [PreserveSig] int Pause();
        [PreserveSig] int Run(long start);
        [PreserveSig] int GetState(int timeout, out int state);
        [PreserveSig] int SetSyncSource(IntPtr clock);
        [PreserveSig] int GetSyncSource(out IntPtr clock);
        [PreserveSig] int EnumPins(out IEnumPins enumerator);
        [PreserveSig] int FindPin([MarshalAs(UnmanagedType.LPWStr)] string id, out IPin? pin);
        [PreserveSig] int QueryFilterInfo(out FilterInfo info);
        [PreserveSig] int JoinFilterGraph(IntPtr graph, [MarshalAs(UnmanagedType.LPWStr)] string? name);
        [PreserveSig] int QueryVendorInfo([MarshalAs(UnmanagedType.LPWStr)] out string? vendor);
    }

    [ComImport]
    [Guid("56a86891-0ad4-11ce-b03a-0020af0ba770")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IPin
    {
        [PreserveSig] int Connect(IPin receive, IntPtr type);
        [PreserveSig] int ReceiveConnection(IPin connector, IntPtr type);
        [PreserveSig] int Disconnect();
        [PreserveSig] int ConnectedTo(out IntPtr pin);
        [PreserveSig] int ConnectionMediaType(out AMMediaType type);
        [PreserveSig] int QueryPinInfo(IntPtr info);
        [PreserveSig] int QueryDirection(out PinDirection direction);
        [PreserveSig] int QueryId([MarshalAs(UnmanagedType.LPWStr)] out string id);
        [PreserveSig] int QueryAccept(IntPtr type);
        [PreserveSig] int EnumMediaTypes(out IEnumMediaTypes enumerator);
        [PreserveSig] int QueryInternalConnections(IntPtr pins, ref int count);
        [PreserveSig] int EndOfStream();
        [PreserveSig] int BeginFlush();
        [PreserveSig] int EndFlush();
        [PreserveSig] int NewSegment(long start, long stop, double rate);
    }

    [ComImport]
    [Guid("56a86892-0ad4-11ce-b03a-0020af0ba770")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IEnumPins
    {
        [PreserveSig] int Next(int count, ref IntPtr pin, IntPtr fetched);
        [PreserveSig] int Skip(int count);
        [PreserveSig] int Reset();
        [PreserveSig] int Clone(out IEnumPins enumerator);
    }

    [ComImport]
    [Guid("89c31040-846b-11ce-97d3-00aa0055595a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IEnumMediaTypes
    {
        [PreserveSig] int Next(int count, IntPtr types, IntPtr fetched);
        [PreserveSig] int Skip(int count);
        [PreserveSig] int Reset();
        [PreserveSig] int Clone(out IEnumMediaTypes enumerator);
    }

    [ComImport]
    [Guid("70ebd3e0-99d0-11d1-9f09-00c04f97dacc")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IAMFilterMiscFlags
    {
        [PreserveSig] int GetMiscFlags();
    }

    [ComImport]
    [Guid("56a868a5-0ad4-11ce-b03a-0020af0ba770")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IQualityControl
    {
        [PreserveSig] int Notify(IBaseFilter self, IntPtr quality);
        [PreserveSig] int SetSink(IntPtr sink);
    }

    [ComImport]
    [Guid("56a8689d-0ad4-11ce-b03a-0020af0ba770")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMemInputPin
    {
        [PreserveSig] int GetAllocator(out IntPtr allocator);
        [PreserveSig] int NotifyAllocator(IntPtr allocator, [MarshalAs(UnmanagedType.Bool)] bool readOnly);
        [PreserveSig] int GetAllocatorRequirements(out AllocatorProperties props);
        [PreserveSig] int Receive(IMediaSample sample);
        [PreserveSig] int ReceiveMultiple([In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] IMediaSample[] samples, int count, out int processed);
        [PreserveSig] int ReceiveCanBlock();
    }

    [ComImport]
    [Guid("56a8689a-0ad4-11ce-b03a-0020af0ba770")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMediaSample
    {
        [PreserveSig] int GetPointer(out IntPtr buffer);
        [PreserveSig] int GetSize();
        [PreserveSig] int GetTime(out long start, out long end);
        [PreserveSig] int SetTime(IntPtr start, IntPtr end);
        [PreserveSig] int IsSyncPoint();
        [PreserveSig] int SetSyncPoint([MarshalAs(UnmanagedType.Bool)] bool sync);
        [PreserveSig] int IsPreroll();
        [PreserveSig] int SetPreroll([MarshalAs(UnmanagedType.Bool)] bool preroll);
        [PreserveSig] int GetActualDataLength();
        [PreserveSig] int SetActualDataLength(int length);
        [PreserveSig] int GetMediaType(out IntPtr type);
        [PreserveSig] int SetMediaType(IntPtr type);
        [PreserveSig] int IsDiscontinuity();
        [PreserveSig] int SetDiscontinuity([MarshalAs(UnmanagedType.Bool)] bool discontinuity);
        [PreserveSig] int GetMediaTime(out long start, out long end);
        [PreserveSig] int SetMediaTime(IntPtr start, IntPtr end);
    }

    /// <summary>
    /// Native <c>00CA49F0</c> / <c>00A3B730</c>
    /// call sample vtbl slot 5 / 3 on this
    /// thread. x86 dumps are +20 / +12;
    /// this process uses slot × IntPtr.Size.
    /// <c>GetComInterfaceForObject</c> AddRefs;
    /// <c>Marshal.Release</c> drops only that
    /// ref. The decoder-owned sample RCW is
    /// not released here.
    /// </summary>
    private static class SampleVtbl
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int GetTimeFn(IntPtr self, out long start, out long end);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int GetPointerFn(IntPtr self, out IntPtr buffer);

        public static int GetTime(IMediaSample sample, out long start, out long end)
        {
            GetTimeEnter++;
            PlayAviWave.Site("gettime", "enter");
            var t0 = Stopwatch.GetTimestamp();
            start = 0;
            end = 0;
            // One AddRef for the raw IMediaSample*.
            var p = Marshal.GetComInterfaceForObject(sample, typeof(IMediaSample));
            try
            {
                var fn = Marshal.GetDelegateForFunctionPointer<GetTimeFn>(
                    Marshal.ReadIntPtr(Marshal.ReadIntPtr(p), RegionTravel.PlayAviGetTimeSlot * IntPtr.Size));
                GetTimeHr = fn(p, out start, out end);
                LastSampleStart = start;
                VtblGetTimeMs = (Stopwatch.GetTimestamp() - t0) * 1000d / Stopwatch.Frequency;
                if (!ComparedGetTime)
                {
                    ComparedGetTime = true;
                    // RCW GetTime on this Receive
                    // thread marshals to STA and
                    // sits. Do not invoke it here.
                    RcwGetTimeHangs = true;
                    PlayAviWave.Site("gettime-rcw", "skip-receive-thread", (long)(VtblGetTimeMs * 1000));
                }

                return GetTimeHr;
            }
            finally
            {
                Marshal.Release(p);
                LastGetTimeMs = (Stopwatch.GetTimestamp() - t0) * 1000d / Stopwatch.Frequency;
                GetTimeLeave++;
                PlayAviWave.Site("gettime", "leave", start);
            }
        }

        public static int GetPointer(IMediaSample sample, out IntPtr buffer)
        {
            GetPointerEnter++;
            PlayAviWave.Site("getpointer", "enter");
            var t0 = Stopwatch.GetTimestamp();
            buffer = IntPtr.Zero;
            var p = Marshal.GetComInterfaceForObject(sample, typeof(IMediaSample));
            try
            {
                var fn = Marshal.GetDelegateForFunctionPointer<GetPointerFn>(
                    Marshal.ReadIntPtr(Marshal.ReadIntPtr(p), RegionTravel.PlayAviGetPointerSlot * IntPtr.Size));
                return fn(p, out buffer);
            }
            finally
            {
                Marshal.Release(p);
                LastGetPointerMs = (Stopwatch.GetTimestamp() - t0) * 1000d / Stopwatch.Frequency;
                GetPointerLeave++;
                PlayAviWave.Site("getpointer", "leave");
            }
        }

    }

    /// <summary>
    /// Native AdviseTime is
    /// <c>[clock.vtbl+16]</c> (slot 4) on this
    /// thread. RCW AdviseTime is not used.
    /// </summary>
    private static class ClockVtbl
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int AdviseTimeFn(
            IntPtr self, long baseTime, long streamTime, IntPtr handle, out IntPtr cookie);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int UnadviseFn(IntPtr self, IntPtr cookie);

        public static int AdviseTime(
            IntPtr clock, long baseTime, long streamTime, IntPtr handle, out IntPtr cookie)
        {
            cookie = IntPtr.Zero;
            if (clock == IntPtr.Zero)
                return unchecked((int)0x80004003);
            var fn = Marshal.GetDelegateForFunctionPointer<AdviseTimeFn>(
                Marshal.ReadIntPtr(Marshal.ReadIntPtr(clock), 4 * IntPtr.Size));
            return fn(clock, baseTime, streamTime, handle, out cookie);
        }

        public static int Unadvise(IntPtr clock, IntPtr cookie)
        {
            if (clock == IntPtr.Zero)
                return 0;
            var fn = Marshal.GetDelegateForFunctionPointer<UnadviseFn>(
                Marshal.ReadIntPtr(Marshal.ReadIntPtr(clock), 6 * IntPtr.Size));
            return fn(clock, cookie);
        }
    }

    [ComImport]
    [Guid("56a86897-0ad4-11ce-b03a-0020af0ba770")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IReferenceClock
    {
        [PreserveSig] int GetTime(out long time);
        [PreserveSig] int AdviseTime(long baseTime, long streamTime, IntPtr handle, out IntPtr cookie);
        [PreserveSig] int AdvisePeriodic(long start, long period, IntPtr semaphore, out IntPtr cookie);
        [PreserveSig] int Unadvise(IntPtr cookie);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct FilterInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string Name;
        public IntPtr Graph;
    }

    private enum PinDirection
    {
        Input,
        Output,
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AMMediaType
    {
        public Guid MajorType;
        public Guid SubType;
        public int FixedSizeSamples;
        public int TemporalCompression;
        public int SampleSize;
        public Guid FormatType;
        public IntPtr Unk;
        public int FormatSize;
        public IntPtr FormatPtr;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AllocatorProperties
    {
        public int Count;
        public int Size;
        public int Alignment;
        public int Prefix;
    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    private sealed class TextureRenderer : IBaseFilter, IMediaFilter, IAMFilterMiscFlags, ICustomQueryInterface
    {
        private readonly RendererPin _pin;
        // 00CB0190 CreateEvent(NULL, FALSE, FALSE, NULL)
        // = auto-reset +84.
        private readonly AutoResetEvent _renderEvent = new(false);
        private readonly ManualResetEvent _threadSignal = new(false);
        private readonly object _recvGate = new();
        private int _state;
        private int _streaming;
        private long _tStart;
        private long _pendingStart;
        private IReferenceClock? _clockObj;
        private IntPtr _clockPtr;
        private IntPtr _adviseCookie;
        private IMediaSample? _pending;
        private readonly int _graphThreadId = Environment.CurrentManagedThreadId;
        private IntPtr _graph;
        private string _name = RegionTravel.PlayAviFilterName;

        // CBaseRenderer::GetMiscFlags. 64-bit
        // RenderFile only QueryAccepts an in-graph
        // pin when this bit is set.
        public int GetMiscFlags()
        {
            MiscFlagsCalls++;
            return 1;
        }

        public TextureRenderer(Action<int, int, byte[]> onSample) =>
            _pin = new RendererPin(this, onSample);

        // 00CA6A30 GetPin / 00CA4F40 pin ctor:
        // IPin is a separate 0xE0 object, COM
        // identity at pin+12 (00CA7CE0).
        public IPin Pin => _pin;
        public bool IsConnected => _pin.IsConnected;

        public int GetClassID(out Guid clsid)
        {
            // 00A3B510 push 0x129D150 into CBaseFilter
            // +40; GetClassID 00CA7620 copies it.
            clsid = RegionTravel.PlayAviRendererClsid;
            return 0;
        }

        public int Stop()
        {
            _state = 0;
            _streaming = 0;
            CancelAdvise();
            _threadSignal.Set();
            lock (_recvGate)
                ClearPending_00CA4BF0();
            return 0;
        }

        public int Pause()
        {
            _state = 1;
            _threadSignal.Reset();
            return 0;
        }

        public int Run(long start)
        {
            // COM IBaseFilter Run 00CA68F0 → 00CAACA0
            // stores tStart at COM this+20/+24 =
            // filter+32/+36, state 2, then C++
            // vtbl+124 StartStreaming 00CA4D80.
            FilterRunCalls++;
            LastTStart = start;
            _tStart = start;
            _state = 2;
            _streaming = 1;
            _threadSignal.Reset();
            lock (_recvGate)
                SchedulePending_00CA4AA0();

            return 0;
        }

        internal void EnsureRunning()
        {
            if (_streaming == 1)
                return;
            long tStart = 0;
            if (_clockObj is not null && _clockObj.GetTime(out var now) >= 0)
                tStart = now;
            Run(tStart);
        }

        public int GetState(int timeout, out int state)
        {
            _ = timeout;
            state = _state;
            return 0;
        }

        public int SetSyncSource(IntPtr clock)
        {
            // COM IMediaFilter 00CA7680: IBaseFilter
            // this (C++ +12). AddRef new, Release
            // [this+12], store at +12 = filter+24.
            // C++ virtual 00A3BCD0 is xor eax,eax;
            // ret and is not the quartz call.
            SyncSourceCalls++;
            CancelAdvise();
            if (_clockObj is not null)
            {
                Marshal.ReleaseComObject(_clockObj);
                _clockObj = null;
            }

            if (_clockPtr != IntPtr.Zero)
            {
                Marshal.Release(_clockPtr);
                _clockPtr = IntPtr.Zero;
            }

            if (clock == IntPtr.Zero)
                return 0;
            var iid = typeof(IReferenceClock).GUID;
            var qi = Marshal.QueryInterface(clock, ref iid, out _clockPtr);
            if (qi < 0 || _clockPtr == IntPtr.Zero)
                return qi < 0 ? qi : unchecked((int)0x80004002);
            _clockObj = (IReferenceClock)Marshal.GetObjectForIUnknown(_clockPtr);
            return 0;
        }

        public int GetSyncSource(out IntPtr clock)
        {
            // 00CA76F0: AddRef [COM this+12] and write.
            if (_clockObj is null)
            {
                clock = IntPtr.Zero;
                return 0;
            }

            clock = Marshal.GetIUnknownForObject(_clockObj);
            return 0;
        }

        internal int WaitAndRender(IMediaSample sample, RendererPin pin)
        {
            // 00CA6E10: PrepareReceive 00CA6C40 →
            // WaitForRenderTime 00CA65B0 →
            // ExecuteRender 00CA4B20 →
            // ClearPending 00CA4BF0.
            int streaming;
            lock (_recvGate)
            {
                if (_pending is not null)
                    return unchecked((int)0x8000FFFF);
                _pending = sample;
                streaming = _streaming;
            }

            // Decoder thread: 00CA6E10 Schedule then
            // WaitForRenderTime. STA graph thread is
            // nested in RenderFile / Run and must
            // return.
            var decoder = streaming == 1 &&
                Environment.CurrentManagedThreadId != _graphThreadId;
            if (decoder)
            {
                SchedulePending_00CA4AA0();
                var hr = WaitForRenderTime_00CA65B0();
                if (hr < 0)
                {
                    lock (_recvGate)
                        ClearPending_00CA4BF0();
                    return 0;
                }
            }

            IMediaSample? held;
            lock (_recvGate)
            {
                held = _pending;
                _pending = null;
                CancelAdvise();
            }

            if (held is null || _state == 0)
                return 0;
            pin.CopyAndSignal(held, release: true);
            return 0;
        }

        private void SchedulePending_00CA4AA0()
        {
            // 00CA4AA0: GetSampleTimes 00CA49F0.
            // hr==0 SetEvent(+84). hr>0 AdviseTime
            // [clock.vtbl+16] at 00CA4B07 using
            // tStart at +32/+36.
            CancelAdvise();
            if (_pending is null)
            {
                _renderEvent.Set();
                return;
            }

            int hr;
            long start;
            try
            {
                hr = GetSampleTimes_00CA49F0(_pending, out start, out _);
            }
            catch
            {
                _renderEvent.Set();
                return;
            }

            _pendingStart = start;
            LastSampleStart = start;
            if (hr < 0 || hr == 0 || _clockObj is null)
            {
                _renderEvent.Set();
                return;
            }

            var handle = _renderEvent.SafeWaitHandle.DangerousGetHandle();
            AdviseCalls++;
            AdviseHr = ClockVtbl.AdviseTime(_clockPtr, _tStart, start, handle, out _adviseCookie);
            if (AdviseHr < 0)
                _renderEvent.Set();
        }

        private int GetSampleTimes_00CA49F0(IMediaSample sample, out long start, out long end)
        {
            // 00CA49F0: call [sample.vtbl+20]
            // GetTime, then [this+24]==0 → S_OK,
            // else ShouldDraw vtbl+92. Direct vtbl
            // — RCW GetTime marshals to STA and
            // sits. Native does this before
            // WaitForRenderTime and before
            // GetPointer.
            var hr = SampleVtbl.GetTime(sample, out start, out end);
            if (hr < 0)
                return 0;
            if (end < start)
                return unchecked((int)0x80040228);
            if (_clockObj is null)
                return 0;
            // Stream time 0 is due at tStart.
            // Run already stored tStart; AdviseTime
            // (tStart, 0) has been seen not to
            // SetEvent +84 in this process.
            if (start == 0)
                return 0;
            return ShouldDrawSampleNow_00CA5850(ref start, ref end);
        }

        private static int ShouldDrawSampleNow_00CA5850(ref long start, ref long end)
        {
            // 00CA5850 subtracts 0x13880 (8 ms)
            // when start >= that. Quality tail
            // can return S_OK (late, draw now)
            // or S_FALSE (schedule). Default
            // schedule is S_FALSE — that is
            // what arms AdviseTime.
            const int earlyDrawHns = 0x13880;
            if (start >= earlyDrawHns)
            {
                start -= earlyDrawHns;
                end -= earlyDrawHns;
            }

            return 1;
        }

        private int WaitForRenderTime_00CA65B0()
        {
            // 00CA65B0: WaitForMultipleObjects
            // (+88 thread, +84 render), retry on
            // WAIT_TIMEOUT. Native timeout is
            // 0x2710 because AdviseTime SetEvent
            // wakes +84. The wake condition is
            // clock >= tStart+start; wait that
            // remaining time (not a fixed 33 ms).
            if (_adviseCookie == IntPtr.Zero)
                _renderEvent.Set();

            WaitHandle[] handles = [_threadSignal, _renderEvent];
            int which;
            do
            {
                which = WaitHandle.WaitAny(handles, PlayAviFromExe.WaitForRenderTimeoutMs);
            } while (which == WaitHandle.WaitTimeout);

            if (which == 0)
                return unchecked((int)0x80040223);
            _adviseCookie = IntPtr.Zero;
            return 0;
        }

        private void ClearPending_00CA4BF0()
        {
            CancelAdvise();
            if (_pending is null)
                return;
            Marshal.ReleaseComObject(_pending);
            SampleRcwRelease++;
            SampleHeldAtReturn = 0;
            _pending = null;
        }

        private void CancelAdvise()
        {
            if (_clockPtr == IntPtr.Zero || _adviseCookie == IntPtr.Zero)
                return;
            ClockVtbl.Unadvise(_clockPtr, _adviseCookie);
            _adviseCookie = IntPtr.Zero;
        }

        public int EnumPins(out IEnumPins enumerator)
        {
            EnumPinsCalls++;
            // 00CAC890: CEnumPins over GetPin.
            // GetPin returns the heap pin, not this.
            enumerator = new PinEnum(_pin);
            return 0;
        }

        public int FindPin(string id, out IPin? pin)
        {
            // 00CA4910 lstrcmpiW vs "In", then
            // GetPin and return pin+12.
            pin = string.Equals(id, RegionTravel.PlayAviPinName, StringComparison.OrdinalIgnoreCase)
                ? _pin
                : null;
            return pin is null ? unchecked((int)0x80040216) : 0;
        }

        public int QueryFilterInfo(out FilterInfo info)
        {
            info = new FilterInfo { Name = _name, Graph = _graph };
            if (_graph != IntPtr.Zero)
                Marshal.AddRef(_graph);
            return 0;
        }

        public int JoinFilterGraph(IntPtr graph, string? name)
        {
            _graph = graph;
            if (!string.IsNullOrEmpty(name))
                _name = name;
            return 0;
        }

        public int QueryVendorInfo(out string? vendor)
        {
            vendor = null;
            return unchecked((int)0x80004001);
        }

        public CustomQueryInterfaceResult GetInterface(ref Guid iid, out IntPtr ppv)
        {
            // Observation only. NotHandled keeps the
            // CCW IBaseFilter / IAMFilterMiscFlags.
            // Do not append after RenderFile — every
            // streaming QI grew LastFilterQi and
            // the graph got slower each frame.
            ppv = IntPtr.Zero;
            if (CaptureQi)
            {
                FilterQiCalls++;
                var name = IidName(iid);
                if (LastFilterQi.Length > 0)
                    LastFilterQi += ",";
                LastFilterQi += name;
                if (iid == RegionTravel.PlayAviMediaPositionIid)
                    MediaPositionQiCalls++;
                else if (iid == RegionTravel.PlayAviMediaSeekingIid)
                    MediaSeekingQiCalls++;
                else if (iid == RegionTravel.PlayAviIOverlayIid)
                    OverlayQiCalls++;
            }

            return CustomQueryInterfaceResult.NotHandled;
        }
    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    private sealed class RendererPin : IPin, IMemInputPin, IQualityControl, IPinConnection, ICustomQueryInterface
    {
        private readonly TextureRenderer _filter;
        private readonly Action<int, int, byte[]> _onSample;
        private IPin? _connected;
        private AMMediaType _type;
        private int _width;
        private int _height;
        private int _stride;
        private IntPtr _allocator;
        private byte[]? _rgbaScratch;

        public bool IsConnected => _connected is not null;

        public RendererPin(TextureRenderer filter, Action<int, int, byte[]> onSample)
        {
            _filter = filter;
            _onSample = onSample;
        }

        public CustomQueryInterfaceResult GetInterface(ref Guid iid, out IntPtr ppv)
        {
            ppv = IntPtr.Zero;
            if (CaptureQi)
            {
                PinQiCalls++;
                var name = IidName(iid);
                if (LastPinQi.Length > 0)
                    LastPinQi += ",";
                LastPinQi += name;
                if (iid == new Guid("56a8689d-0ad4-11ce-b03a-0020af0ba770"))
                    MemInputQiCalls++;
            }

            return CustomQueryInterfaceResult.NotHandled;
        }

        // Quartz QIs IPinConnection on the heap pin
        // during RenderFile. DynamicQueryAccept is
        // the same RGB24 check as IPin.QueryAccept.
        public int DynamicQueryAccept(IntPtr type) => QueryAccept(type);

        public int NotifyEndOfStream(IntPtr notify)
        {
            _ = notify;
            return 0;
        }

        public int IsEndPin() => 0;

        public int DynamicDisconnect() => Disconnect();

        public int Connect(IPin receive, IntPtr type)
        {
            // 00CAB470 CBasePin::Connect. Input
            // pins are connected via
            // ReceiveConnection. Count calls.
            _ = (receive, type);
            ConnectCalls++;
            return unchecked((int)0x80040208);
        }

        public int ReceiveConnection(IPin connector, IntPtr type)
        {
            ReceiveConnectionCalls++;
            if (QueryAccept(type) != 0)
                return unchecked((int)0x80040200);
            _connected = connector;
            if (type != IntPtr.Zero)
            {
                _type = Marshal.PtrToStructure<AMMediaType>(type);
                ReadVideoInfo(type);
            }
            return 0;
        }

        public int Disconnect()
        {
            _connected = null;
            return 0;
        }

        public int ConnectedTo(out IntPtr pin)
        {
            // 00CA68F0 / strmbase ConnectedTo writes
            // *ppPin. out IPin? left a non-null slot
            // so RenderFile treated the pin as already
            // connected and never QueryAccept'd.
            ConnectedToCalls++;
            if (_connected is null)
            {
                pin = IntPtr.Zero;
                return unchecked((int)0x80040209);
            }

            pin = Marshal.GetComInterfaceForObject(_connected, typeof(IPin));
            return 0;
        }

        public int ConnectionMediaType(out AMMediaType type)
        {
            type = _type;
            return _connected is null ? unchecked((int)0x80040209) : 0;
        }

        public int QueryPinInfo(IntPtr info)
        {
            // 00CA8420 writes PIN_INFO itself:
            // [p+0]=IBaseFilter* (filter C++ +12),
            // [p+4]=dir from pin+28, [p+8]=name.
            // out PinInfo left Filter unusable so
            // ConnectDirect never QueryAccept'd.
            QueryPinInfoCalls++;
            if (info == IntPtr.Zero)
                return unchecked((int)0x80004003);
            var filter = Marshal.GetComInterfaceForObject(_filter, typeof(IBaseFilter));
            Marshal.WriteIntPtr(info, filter);
            Marshal.WriteInt32(info, IntPtr.Size, (int)PinDirection.Input);
            var name = info + IntPtr.Size + 4;
            var chars = RegionTravel.PlayAviPinName;
            for (var i = 0; i < 128; i++)
            {
                var ch = i < chars.Length ? chars[i] : '\0';
                Marshal.WriteInt16(name, i * 2, ch);
            }
            return 0;
        }

        public int QueryDirection(out PinDirection direction)
        {
            QueryDirectionCalls++;
            direction = PinDirection.Input;
            return 0;
        }

        public int QueryId(out string id)
        {
            QueryIdCalls++;
            id = "In";
            return 0;
        }

        // 00CA89A0 IMemInputPin is pin+0x98.
        // 00CA7CE0 IQualityControl is pin+16.
        public int Notify(IBaseFilter self, IntPtr quality)
        {
            _ = (self, quality);
            return 0;
        }

        public int SetSink(IntPtr sink)
        {
            _ = sink;
            return 0;
        }

        public int QueryAccept(IntPtr type)
        {
            QueryAcceptCalls++;
            // 00CA84C0: null → E_POINTER. Else this-12
            // and C++ vtbl+32 → 00CA5200 → filter
            // vtbl+176 00A3B590. Failed check (any
            // negative) becomes S_FALSE (1).
            if (type == IntPtr.Zero)
                return unchecked((int)0x80004003);
            var major = Marshal.PtrToStructure<Guid>(type);
            var subtype = Marshal.PtrToStructure<Guid>(type + 16);
            var formatType = Marshal.PtrToStructure<Guid>(type + 44);
            if (formatType != Ds.VideoInfo ||
                major != Ds.Video ||
                subtype != Ds.Rgb24)
                return 1;
            return 0;
        }

        public int EnumMediaTypes(out IEnumMediaTypes enumerator)
        {
            EnumMediaTypesCalls++;
            enumerator = new MediaTypeEnum();
            return 0;
        }

        public int QueryInternalConnections(IntPtr pins, ref int count)
        {
            _ = pins;
            count = 0;
            return unchecked((int)0x80004001);
        }

        public int EndOfStream() => 0;

        public int BeginFlush() => 0;

        public int EndFlush() => 0;

        public int NewSegment(long start, long stop, double rate)
        {
            _ = (start, stop, rate);
            return 0;
        }

        public int GetAllocator(out IntPtr allocator)
        {
            // 00CA5D50: return stored allocator.
            // Empty → VFW_E_NO_ALLOCATOR so the
            // decoder/output pin provides CMemAllocator.
            // Do not CoCreate a second queue here.
            if (_allocator == IntPtr.Zero)
            {
                allocator = IntPtr.Zero;
                return unchecked((int)0x8004020A);
            }

            allocator = _allocator;
            Marshal.AddRef(_allocator);
            return 0;
        }

        public int NotifyAllocator(IntPtr allocator, bool readOnly)
        {
            _ = readOnly;
            if (allocator == IntPtr.Zero)
                return unchecked((int)0x80004003);
            if (_allocator != IntPtr.Zero)
                Marshal.Release(_allocator);
            _allocator = allocator;
            Marshal.AddRef(_allocator);
            return 0;
        }

        public int GetAllocatorRequirements(out AllocatorProperties props)
        {
            props = default;
            return unchecked((int)0x80004001);
        }

        public int Receive(IMediaSample sample)
        {
            var recv0 = Stopwatch.GetTimestamp();
            ReceiveCalls++;
            PlayAviWave.Recv(ReceiveCalls, 0, LastFrames, "enter");
            var hr = _filter.WaitAndRender(sample, this);
            if (hr == unchecked((int)0x8000FFFF))
            {
                // PrepareReceive failed before AddRef.
                // Decoder still owns the sample.
                PlayAviWave.Recv(ReceiveCalls, 0, LastFrames, "reject");
                return hr;
            }

            var recvTicks = Stopwatch.GetTimestamp() - recv0;
            if (ReceiveCalls == 1 || ReceiveCalls % 100 == 0)
                RecordPace(recvTicks, 0, 0, 0);
            PlayAviWave.Recv(ReceiveCalls, 0, LastFrames, "leave");
            return hr;
        }

        internal void CopyAndSignal(IMediaSample sample, bool release)
        {
            long sampleStart = LastSampleStart;
            try
            {
                PlayAviWave.Recv(ReceiveCalls, sampleStart, LastFrames, "time");
                var gp = SampleVtbl.GetPointer(sample, out var data);
                if (gp >= 0 && data != IntPtr.Zero)
                    GetPointerCalls++;
                if (gp < 0 || data == IntPtr.Zero)
                    return;
                var length = sample.GetActualDataLength();
                if (length <= 0)
                    length = sample.GetSize();
                if (_width <= 0 || _height <= 0 || length <= 0)
                    return;
                var rgba = CopySample(data, length);
                if (rgba is not null)
                    _onSample(_width, _height, rgba);
            }
            finally
            {
                if (release)
                {
                    Marshal.ReleaseComObject(sample);
                    SampleRcwRelease++;
                }

                SampleHeldAtReturn = release ? 0 : 1;
            }
        }

        public int ReceiveMultiple(IMediaSample[] samples, int count, out int processed)
        {
            processed = 0;
            for (var i = 0; i < count; i++)
            {
                Receive(samples[i]);
                processed++;
            }

            return 0;
        }

        public int ReceiveCanBlock() => 0;

        private void ReadVideoInfo(IntPtr type)
        {
            // 64-bit AM_MEDIA_TYPE: subtype +16,
            // cbFormat +72, pbFormat +80. 00A3B5F0
            // on the 32-bit exe reads pbFormat at
            // +68; same VIDEOINFOHEADER after that.
            var format = Marshal.ReadIntPtr(type, 80);
            var formatSize = Marshal.ReadInt32(type, 72);
            if (format == IntPtr.Zero || formatSize < 56)
                return;
            _width = Marshal.ReadInt32(format, 52);
            _height = Math.Abs(Marshal.ReadInt32(format, 56));

            // 00A3B5F0: stride = ((width+1)*3) & ~3 for RGB24.
            _stride = ((_width + 1) * 3) & ~3;
        }

        private byte[]? CopySample(IntPtr data, int length)
        {
            var pixels = _width * _height;
            if (pixels <= 0)
                return null;
            var need = pixels * 4;
            if (_rgbaScratch is null || _rgbaScratch.Length != need)
            {
                ScratchAllocs++;
                _rgbaScratch = new byte[need];
            }
            var rgba = _rgbaScratch;
            var bpp = 3;
            var stride = _stride > 0 ? _stride : ((_width * bpp + 3) & ~3);
            if (length < stride * _height && length >= pixels * bpp)
                stride = _width * bpp;
            // 00A3B730 copies GetPointer row 0 into
            // LockRect row 0 — no V flip. Present
            // inverts V (LineShaders.VideoFragment).
            BgrToRgba(data, stride, rgba, _width, _height, bpp);
            return rgba;
        }
    }

    private static void RecordPace(long recvTicks, long copyTicks, long sampleStart, long sampleEnd)
    {
        using var proc = Process.GetCurrentProcess();
        proc.Refresh();
        var freq = (double)Stopwatch.Frequency;
        PaceSamples.Add(new PlayAviPaceSample
        {
            Receive = ReceiveCalls,
            GetPointer = GetPointerCalls,
            FrameSerial = LastFrames,
            SampleStartHns = sampleStart,
            SampleEndHns = sampleEnd,
            WallMs = (Stopwatch.GetTimestamp() - PaceStart) * 1000d / freq,
            ReceiveMs = recvTicks * 1000d / freq,
            CopyMs = copyTicks * 1000d / freq,
            PresentWaitMs = PresentWaitTicks * 1000d / freq,
            HeapBytes = GC.GetTotalMemory(false),
            Gen0 = GC.CollectionCount(0),
            Gen1 = GC.CollectionCount(1),
            Gen2 = GC.CollectionCount(2),
            WorkingSet = proc.WorkingSet64,
            PrivateBytes = proc.PrivateMemorySize64,
            ScratchAllocs = ScratchAllocs,
            RgbaAllocs = RgbaAllocs,
            FilterQiChars = LastFilterQi.Length,
            PinQiChars = LastPinQi.Length,
            FilterQi = FilterQiCalls,
            PinQi = PinQiCalls,
        });
    }

    private static string IidName(Guid iid)
    {
        if (iid == Guid.Empty) return "Empty";
        if (iid == new Guid("00000000-0000-0000-c000-000000000046")) return "IUnknown";
        if (iid == new Guid("00000003-0000-0000-c000-000000000046")) return "IMarshal";
        if (iid == new Guid("00020400-0000-0000-c000-000000000046")) return "IDispatch";
        if (iid == RegionTravel.PlayAviIPinIid) return "IPin";
        if (iid == new Guid("56a8689d-0ad4-11ce-b03a-0020af0ba770")) return "IMemInputPin";
        if (iid == new Guid("56a8689c-0ad4-11ce-b03a-0020af0ba770")) return "IMemAllocator";
        if (iid == new Guid("56a868a5-0ad4-11ce-b03a-0020af0ba770")) return "IQualityControl";
        if (iid == new Guid("56a86895-0ad4-11ce-b03a-0020af0ba770")) return "IBaseFilter";
        if (iid == new Guid("56a86899-0ad4-11ce-b03a-0020af0ba770")) return "IMediaFilter";
        if (iid == new Guid("0000010c-0000-0000-c000-000000000046")) return "IPersist";
        if (iid == new Guid("70ebd3e0-99d0-11d1-9f09-00c04f97dacc")) return "IAMFilterMiscFlags";
        if (iid == RegionTravel.PlayAviMediaPositionIid) return "IMediaPosition";
        if (iid == RegionTravel.PlayAviMediaSeekingIid) return "IMediaSeeking";
        if (iid == RegionTravel.PlayAviIOverlayIid) return "IOverlay";
        if (iid == new Guid("56a868b4-0ad4-11ce-b03a-0020af0ba770")) return "IVideoWindow";
        if (iid == new Guid("56a868b5-0ad4-11ce-b03a-0020af0ba770")) return "IBasicVideo";
        if (iid == new Guid("56a868bf-0ad4-11ce-b03a-0020af0ba770")) return "IStreamBuilder";
        if (iid == new Guid("56a868b3-0ad4-11ce-b03a-0020af0ba770")) return "IBasicAudio";
        if (iid == new Guid("56a868c0-0ad4-11ce-b03a-0020af0ba770")) return "IAMovieSetup";
        if (iid == new Guid("56a868a2-0ad4-11ce-b03a-0020af0ba770")) return "IMediaEventSink";
        if (iid == new Guid("89c330e2-8ac5-11d0-89dc-00c04fc9e26e")) return "IAMOpenProgress";
        if (iid == RegionTravel.PlayAviIPinConnectionIid) return "IPinConnection";
        if (iid == new Guid("31efac30-515c-11d0-a9aa-00aa0061be93")) return "IKsPropertySet";
        if (iid == new Guid("8e1c39a1-de53-11cf-aa63-0080c744528d")) return "IAMGraphStreams";
        return iid.ToString("D");
    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    private sealed class PinEnum : IEnumPins
    {
        private readonly IPin _pin;
        private int _index;

        public PinEnum(IPin pin) => _pin = pin;

        public int Next(int count, ref IntPtr pin, IntPtr fetched)
        {
            PinNextCalls++;
            var n = 0;
            if (_index == 0 && count > 0)
            {
                // IEnumPins::Next writes IPin* into
                // the caller's IPin**. ref IntPtr is
                // that slot — IntPtr+WriteIntPtr left
                // the graph with a null pin (dir=0).
                pin = Marshal.GetComInterfaceForObject(_pin, typeof(IPin));
                _index = 1;
                n = 1;
            }

            if (fetched != IntPtr.Zero)
                Marshal.WriteInt32(fetched, n);
            return n == count ? 0 : 1;
        }

        public int Skip(int count)
        {
            _index += count;
            return _index > 1 ? 1 : 0;
        }

        public int Reset()
        {
            _index = 0;
            return 0;
        }

        public int Clone(out IEnumPins enumerator)
        {
            enumerator = new PinEnum(_pin) { _index = _index };
            return 0;
        }
    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    private sealed class MediaTypeEnum : IEnumMediaTypes
    {
        private int _index;

        public int Next(int count, IntPtr types, IntPtr fetched)
        {
            // SysWOW64 quartz ConnectDirect
            // 1007F596 → source IPin.Connect
            // (pmt=NULL) walks dest EnumMediaTypes
            // first. Empty dest (Fable GetMediaType
            // 00CA84F0 E_UNEXPECTED) never
            // QueryAccepts. Advertise the one type
            // 00A3B590 accepts.
            MediaTypeNextCalls++;
            if (count <= 0)
                return unchecked((int)0x80070057);
            if (types == IntPtr.Zero)
                return unchecked((int)0x80004003);
            if (count > 1 && fetched == IntPtr.Zero)
                return unchecked((int)0x80070057);
            var n = 0;
            if (_index == 0)
            {
                var mt = AllocRgb24Type();
                if (mt == IntPtr.Zero)
                    return unchecked((int)0x8007000E);
                Marshal.WriteIntPtr(types, mt);
                _index = 1;
                n = 1;
            }

            if (fetched != IntPtr.Zero)
                Marshal.WriteInt32(fetched, n);
            return n == count ? 0 : 1;
        }

        public int Skip(int count)
        {
            _index += count;
            return 1;
        }

        public int Reset()
        {
            _index = 0;
            return 0;
        }

        public int Clone(out IEnumMediaTypes enumerator)
        {
            enumerator = new MediaTypeEnum { _index = _index };
            return 0;
        }
    }

    private static IntPtr AllocRgb24Type()
    {
        var mtSize = Marshal.SizeOf<AMMediaType>();
        var mt = Ole32.CoTaskMemAlloc((IntPtr)mtSize);
        if (mt == IntPtr.Zero)
            return IntPtr.Zero;
        for (var i = 0; i < mtSize; i++)
            Marshal.WriteByte(mt, i, 0);
        const int vihSize = 88;
        var vih = Ole32.CoTaskMemAlloc((IntPtr)vihSize);
        if (vih == IntPtr.Zero)
        {
            Ole32.CoTaskMemFree(mt);
            return IntPtr.Zero;
        }

        for (var i = 0; i < vihSize; i++)
            Marshal.WriteByte(vih, i, 0);
        var t = new AMMediaType
        {
            MajorType = Ds.Video,
            SubType = Ds.Rgb24,
            FixedSizeSamples = 1,
            FormatType = Ds.VideoInfo,
            FormatSize = vihSize,
            FormatPtr = vih,
        };
        Marshal.StructureToPtr(t, mt, false);
        return mt;
    }
}

/// <summary>
/// Observation of one AddFilter+RenderFile
/// attempt. Used by the x86/x64 quartz
/// experiment; not a scene-flow object.
/// </summary>
public sealed class PlayAviGraphTrace
{
    public string ProcessArch { get; init; } = "";
    public int IntPtrSize { get; init; }
    public int AddFilterHr { get; init; }
    public int RenderFileHr { get; init; }
    public int RunHr { get; init; }
    public int EnumPins { get; init; }
    public int Next { get; init; }
    public int QueryDirection { get; init; }
    public int ConnectedTo { get; init; }
    public int QueryPinInfo { get; init; }
    public int QueryId { get; init; }
    public int EnumMediaTypes { get; init; }
    public int MediaTypeNext { get; init; }
    public int QueryAccept { get; init; }
    public int ReceiveConnection { get; init; }
    public int MemInputQi { get; init; }
    public int Receive { get; init; }
    public int GetPointer { get; init; }
    public int SampleRcwRelease { get; init; }
    public int SampleHeldAtReturn { get; init; }
    public int Connect { get; init; }
    public int MiscFlags { get; init; }
    public int MediaPositionQi { get; init; }
    public int MediaSeekingQi { get; init; }
    public int OverlayQi { get; init; }
    public string FilterQi { get; init; } = "";
    public string PinQi { get; init; } = "";
    public string Graph { get; init; } = "";
    public string PinVisible { get; init; } = "";
    public bool Connected { get; init; }
    public bool SamplesFromGetPointer { get; init; }
    public int Frames { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public string? Error { get; init; }
}

/// <summary>
/// One sample of a complete PlayAVI run.
/// Observation only — no pacing change.
/// </summary>
public sealed class PlayAviPaceSample
{
    public int Receive { get; init; }
    public int GetPointer { get; init; }
    public int FrameSerial { get; init; }
    public long SampleStartHns { get; init; }
    public long SampleEndHns { get; init; }
    public double WallMs { get; init; }
    public double ReceiveMs { get; init; }
    public double CopyMs { get; init; }
    public double PresentWaitMs { get; init; }
    public long HeapBytes { get; init; }
    public int Gen0 { get; init; }
    public int Gen1 { get; init; }
    public int Gen2 { get; init; }
    public long WorkingSet { get; init; }
    public long PrivateBytes { get; init; }
    public int ScratchAllocs { get; init; }
    public int RgbaAllocs { get; init; }
    public int FilterQiChars { get; init; }
    public int PinQiChars { get; init; }
    public int FilterQi { get; init; }
    public int PinQi { get; init; }
}
