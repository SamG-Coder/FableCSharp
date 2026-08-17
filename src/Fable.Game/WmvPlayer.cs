using System.Runtime.InteropServices;

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

    public static string? LastError { get; private set; }

    private readonly object _gate = new();
    private readonly AutoResetEvent _frameEvent = new(false);
    private Thread? _thread;
    private volatile bool _stop;
    private IGraphBuilder? _graph;
    private IMediaControl? _control;
    private IMediaEvent? _events;
    private IMediaPosition? _position;
    private TextureRenderer? _renderer;
    private long _elapsedHns;

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
            player.Dispose();
            return null;
        }

        return player;
    }

    /// <summary>
    /// One <c>006286F0</c> present tick:
    /// <c>WaitForSingleObject([player+124], 33)</c>
    /// then blit the latest GetPointer frame.
    /// </summary>
    public bool TryAdvance(float dt)
    {
        if (Ended)
            return false;
        if (dt > 0f)
            _elapsedHns += (long)(dt * 10_000_000d);
        _frameEvent.WaitOne(RegionTravel.PlayAviPresentMs);
        return Rgba is not null;
    }

    public void Dispose()
    {
        _stop = true;
        _frameEvent.Set();
        if (_thread is { IsAlive: true } &&
            !_thread.Join(TimeSpan.FromSeconds(2)))
            LastError ??= "sta-join";
        _thread = null;
        _frameEvent.Dispose();
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
        if (hr < 0)
            return $"AddFilter {hr:X8}";

        hr = _graph.RenderFile(path, null);
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

        if (hr < 0)
            return $"Run {hr:X8}";

        if (!_renderer.IsConnected)
            return $"renderer-not-connected enumPins={EnumPinsCalls} next={PinNextCalls} dir={QueryDirectionCalls} conn={ConnectedToCalls} memqi={MemInputQiCalls} qa={QueryAcceptCalls} rc={ReceiveConnectionCalls} vis=" +
                   GraphPinVisible() + " " + GraphSummary();

        var deadline = Environment.TickCount64 + 5000;
        while (Environment.TickCount64 < deadline && !_stop)
        {
            DrainEvents();
            if (_frameEvent.WaitOne(RegionTravel.PlayAviPresentMs) && Rgba is not null)
                return null;
            if (Rgba is not null)
                return null;
        }

        return Rgba is null ? "first-sample-timeout" : null;
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

    private int ConnectSourceToRenderer(IBaseFilter source)
    {
        if (_graph is null || _renderer is null)
            return unchecked((int)0x80004005);
        if (source.EnumPins(out var en) < 0 || en is null)
            return unchecked((int)0x80004005);
        var hr = unchecked((int)0x80040217); // VFW_E_CANNOT_CONNECT
        while (NextPin(en) is { } pin)
        {
            if (pin.QueryDirection(out var dir) < 0 || dir != PinDirection.Output)
                continue;
            hr = _graph.Connect(pin, _renderer.Pin);
            if (hr >= 0)
                return 0;
            hr = _graph.Render(pin);
            if (hr >= 0 && _renderer.IsConnected)
                return 0;
        }

        return hr;
    }

    private void StealWindowedInput()
    {
        if (_graph is null || _renderer is null)
            return;
        if (_graph.EnumFilters(out var filters) < 0 || filters is null)
            return;
        var batch = new IBaseFilter[8];
        var tried = 0;
        var lastHr = 0;
        while (filters.Next(1, batch, IntPtr.Zero) == 0)
        {
            var filter = batch[0];
            if (filter is null || ReferenceEquals(filter, _renderer))
                continue;
            if (filter.EnumPins(out var pins) < 0 || pins is null)
                continue;
            pins.Reset();
            while (NextPin(pins) is { } pin)
            {
                if (pin.QueryDirection(out var dir) < 0 || dir != PinDirection.Output)
                    continue;
                if (pin.ConnectedTo(out var dest) >= 0 && dest != IntPtr.Zero)
                {
                    var destPin = (IPin)Marshal.GetObjectForIUnknown(dest);
                    Marshal.Release(dest);
                    _graph.Disconnect(pin);
                    _graph.Disconnect(destPin);
                }

                tried++;
                lastHr = _graph.Connect(pin, _renderer.Pin);
                if (lastHr >= 0 && _renderer.IsConnected)
                    return;
                lastHr = _graph.Render(pin);
                if (lastHr >= 0 && _renderer.IsConnected)
                    return;
            }
        }

        LastError = $"steal-tried={tried} hr={lastHr:X8}";
    }

    private void HideVideoWindow()
    {
        if (_graph is not IVideoWindow window)
            return;
        try
        {
            window.put_AutoShow(0);
            window.put_Visible(0);
            window.put_WindowState(0);
        }
        catch
        {
            // Custom renderer has no IVideoWindow.
        }
    }

    /// <summary>
    /// RenderFile may still insert VMR / Video
    /// Renderer. Exe present is the game
    /// backbuffer only — drop windowed filters.
    /// </summary>
    private void RemoveWindowedRenderers()
    {
        if (_graph is null)
            return;
        if (_graph.EnumFilters(out var en) < 0 || en is null)
            return;
        var batch = new IBaseFilter[16];
        var drop = new List<IBaseFilter>();
        while (en.Next(1, batch, IntPtr.Zero) == 0)
        {
            var filter = batch[0];
            if (filter is null || ReferenceEquals(filter, _renderer))
                continue;
            if (filter.QueryFilterInfo(out var info) < 0)
                continue;
            if (info.Graph != IntPtr.Zero)
                Marshal.Release(info.Graph);
            var name = info.Name ?? "";
            if (name.Contains("Video Renderer", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("Video Mixing", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("EVR", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("VMR", StringComparison.OrdinalIgnoreCase))
                drop.Add(filter);
        }

        foreach (var filter in drop)
            _graph.RemoveFilter(filter);
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
        IntPtr src, int srcStride, byte[] dest, int width, int height, int bpp, bool flip)
    {
        fixed (byte* d0 = dest)
        {
            for (var y = 0; y < height; y++)
            {
                var srcY = flip ? height - 1 - y : y;
                var s = (byte*)src + srcY * srcStride;
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
                Rgba = new byte[rgba.Length];
            rgba.AsSpan().CopyTo(Rgba);
            FrameSerial++;
        }

        // 00A3B730 SetEvent([player+124])
        _frameEvent.Set();
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
    }

    private static class Ds
    {
        public static readonly Guid FilterGraph = new("e436ebb3-524f-11ce-9f53-0020af0ba770");
        public static readonly Guid Video = new("73646976-0000-0010-8000-00aa00389b71");
        public static readonly Guid Rgb24 = new("e436eb7d-524f-11ce-9f53-0020af0ba770");
        public static readonly Guid Rgb32 = new("e436eb7e-524f-11ce-9f53-0020af0ba770");
        public static readonly Guid Yuy2 = new("32595559-0000-0010-8000-00aa00389b71");
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
    [Guid("56a868b4-0ad4-11ce-b03a-0020af0ba770")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IVideoWindow
    {
        void GetTypeInfoCount(out int count);
        void GetTypeInfo(int itinfo, int lcid, out IntPtr info);
        void GetIDsOfNames(ref Guid iid, IntPtr names, int count, int lcid, IntPtr dispIds);
        void Invoke(int dispId, ref Guid iid, int lcid, short flags, IntPtr dispParams, IntPtr result, IntPtr excep, IntPtr argErr);
        [PreserveSig] int put_Caption([MarshalAs(UnmanagedType.BStr)] string caption);
        [PreserveSig] int get_Caption([MarshalAs(UnmanagedType.BStr)] out string caption);
        [PreserveSig] int put_WindowStyle(int style);
        [PreserveSig] int get_WindowStyle(out int style);
        [PreserveSig] int put_WindowStyleEx(int style);
        [PreserveSig] int get_WindowStyleEx(out int style);
        [PreserveSig] int put_AutoShow(int autoShow);
        [PreserveSig] int get_AutoShow(out int autoShow);
        [PreserveSig] int put_WindowState(int state);
        [PreserveSig] int get_WindowState(out int state);
        [PreserveSig] int put_BackgroundPalette(int background);
        [PreserveSig] int get_BackgroundPalette(out int background);
        [PreserveSig] int put_Visible(int visible);
        [PreserveSig] int get_Visible(out int visible);
    }

    [ComImport]
    [Guid("56a868b5-0ad4-11ce-b03a-0020af0ba770")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IBasicVideo
    {
        void GetTypeInfoCount(out int count);
        void GetTypeInfo(int itinfo, int lcid, out IntPtr info);
        void GetIDsOfNames(ref Guid iid, IntPtr names, int count, int lcid, IntPtr dispIds);
        void Invoke(int dispId, ref Guid iid, int lcid, short flags, IntPtr dispParams, IntPtr result, IntPtr excep, IntPtr argErr);
        [PreserveSig] int get_AvgTimePerFrame(out double time);
        [PreserveSig] int get_BitRate(out int rate);
        [PreserveSig] int get_BitErrorRate(out int rate);
        [PreserveSig] int get_VideoWidth(out int width);
        [PreserveSig] int get_VideoHeight(out int height);
        [PreserveSig] int put_SourceLeft(int left);
        [PreserveSig] int get_SourceLeft(out int left);
        [PreserveSig] int put_SourceWidth(int width);
        [PreserveSig] int get_SourceWidth(out int width);
        [PreserveSig] int put_SourceTop(int top);
        [PreserveSig] int get_SourceTop(out int top);
        [PreserveSig] int put_SourceHeight(int height);
        [PreserveSig] int get_SourceHeight(out int height);
        [PreserveSig] int put_DestinationLeft(int left);
        [PreserveSig] int get_DestinationLeft(out int left);
        [PreserveSig] int put_DestinationWidth(int width);
        [PreserveSig] int get_DestinationWidth(out int width);
        [PreserveSig] int put_DestinationTop(int top);
        [PreserveSig] int get_DestinationTop(out int top);
        [PreserveSig] int put_DestinationHeight(int height);
        [PreserveSig] int get_DestinationHeight(out int height);
        [PreserveSig] int SetSourcePosition(int left, int top, int width, int height);
        [PreserveSig] int GetSourcePosition(out int left, out int top, out int width, out int height);
        [PreserveSig] int SetDefaultSourcePosition();
        [PreserveSig] int SetDestinationPosition(int left, int top, int width, int height);
        [PreserveSig] int GetDestinationPosition(out int left, out int top, out int width, out int height);
        [PreserveSig] int SetDefaultDestinationPosition();
        [PreserveSig] int GetVideoSize(out int width, out int height);
        [PreserveSig] int GetVideoPaletteEntries(int start, int count, out int retrieved, IntPtr palette);
        [PreserveSig] int GetCurrentImage(ref int size, IntPtr dib);
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
        [PreserveSig] int QueryPinInfo(out PinInfo info);
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
    [Guid("56a8689c-0ad4-11ce-b03a-0020af0ba770")]
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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct FilterInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string Name;
        public IntPtr Graph;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct PinInfo
    {
        // 00CA7680 writes IBaseFilter* (AddRef'd).
        // Interface fields here do not marshal a
        // native IBaseFilter* that RenderFile can QI.
        public IntPtr Filter;
        public PinDirection Direction;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string Name;
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
    private sealed class TextureRenderer : IBaseFilter, IPin, IMemInputPin, IAMFilterMiscFlags, ICustomQueryInterface
    {
        private readonly RendererPin _pin;
        private int _state;
        private IntPtr _graph;
        private string _name = RegionTravel.PlayAviFilterName;

        // CBaseRenderer::GetMiscFlags. 64-bit
        // RenderFile only QueryAccepts an in-graph
        // pin when this bit is set (dir=32 conn=6
        // qa=0 without it).
        public int GetMiscFlags() => 1;

        public TextureRenderer(Action<int, int, byte[]> onSample) =>
            _pin = new RendererPin(this, onSample);

        public IPin Pin => this;
        public bool IsConnected => _pin.IsConnected;

        public int GetClassID(out Guid clsid)
        {
            clsid = new Guid("a3b51000-0000-0000-0000-000000a3b510");
            return 0;
        }

        public int Stop()
        {
            _state = 0;
            return 0;
        }

        public int Pause()
        {
            _state = 1;
            return 0;
        }

        public int Run(long start)
        {
            _ = start;
            _state = 2;
            return 0;
        }

        public int GetState(int timeout, out int state)
        {
            _ = timeout;
            state = _state;
            return 0;
        }

        public int SetSyncSource(IntPtr clock)
        {
            _ = clock;
            return 0;
        }

        public int GetSyncSource(out IntPtr clock)
        {
            clock = IntPtr.Zero;
            return 0;
        }

        public int EnumPins(out IEnumPins enumerator)
        {
            EnumPinsCalls++;
            // Same COM object as IPin (exe pin at +12).
            enumerator = new PinEnum(this);
            return 0;
        }

        public int FindPin(string id, out IPin? pin)
        {
            // 00CA4910 lstrcmpiW vs "In" (0x129D184),
            // then returns this+12.
            pin = string.Equals(id, RegionTravel.PlayAviPinName, StringComparison.OrdinalIgnoreCase)
                ? this
                : null;
            return pin is null ? unchecked((int)0x80040216) : 0;
        }

        public int Connect(IPin receive, IntPtr type) => _pin.Connect(receive, type);
        public int ReceiveConnection(IPin connector, IntPtr type) =>
            _pin.ReceiveConnection(connector, type);
        public int Disconnect() => _pin.Disconnect();
        public int ConnectedTo(out IntPtr pin) => _pin.ConnectedTo(out pin);
        public int ConnectionMediaType(out AMMediaType type) => _pin.ConnectionMediaType(out type);
        public int QueryPinInfo(out PinInfo info) => _pin.QueryPinInfo(out info);
        public int QueryDirection(out PinDirection direction) => _pin.QueryDirection(out direction);
        public int QueryId(out string id) => _pin.QueryId(out id);
        public int QueryAccept(IntPtr type) => _pin.QueryAccept(type);
        public int EnumMediaTypes(out IEnumMediaTypes enumerator) =>
            _pin.EnumMediaTypes(out enumerator);
        public int QueryInternalConnections(IntPtr pins, ref int count) =>
            _pin.QueryInternalConnections(pins, ref count);
        public int EndOfStream() => _pin.EndOfStream();
        public int BeginFlush() => _pin.BeginFlush();
        public int EndFlush() => _pin.EndFlush();
        public int NewSegment(long start, long stop, double rate) =>
            _pin.NewSegment(start, stop, rate);
        public int GetAllocator(out IntPtr allocator) => _pin.GetAllocator(out allocator);
        public int NotifyAllocator(IntPtr allocator, bool readOnly) =>
            _pin.NotifyAllocator(allocator, readOnly);
        public int GetAllocatorRequirements(out AllocatorProperties props) =>
            _pin.GetAllocatorRequirements(out props);
        public int Receive(IMediaSample sample) => _pin.Receive(sample);
        public int ReceiveMultiple(IMediaSample[] samples, int count, out int processed) =>
            _pin.ReceiveMultiple(samples, count, out processed);
        public int ReceiveCanBlock() => _pin.ReceiveCanBlock();

        // Exe pin QI 00A3BD00 / IMemInputPin 00A3BD60
        // this-adjust onto the same object. RenderFile
        // QIs IMemInputPin after QueryDirection.
        public CustomQueryInterfaceResult GetInterface(ref Guid iid, out IntPtr ppv)
        {
            if (iid == typeof(IMemInputPin).GUID)
                MemInputQiCalls++;
            ppv = IntPtr.Zero;
            return CustomQueryInterfaceResult.NotHandled;
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
    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    private sealed class RendererPin : IPin, IMemInputPin
    {
        private readonly TextureRenderer _filter;
        private readonly Action<int, int, byte[]> _onSample;
        private IPin? _connected;
        private AMMediaType _type;
        private int _width;
        private int _height;
        private int _bitCount;
        private int _stride;
        private bool _topDown;
        private Guid _subType;

        public bool IsConnected => _connected is not null;

        public RendererPin(TextureRenderer filter, Action<int, int, byte[]> onSample)
        {
            _filter = filter;
            _onSample = onSample;
        }

        public int Connect(IPin receive, IntPtr type)
        {
            _ = (receive, type);
            return unchecked((int)0x80004001);
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

        public int QueryPinInfo(out PinInfo info)
        {
            info = new PinInfo
            {
                Filter = Marshal.GetComInterfaceForObject(_filter, typeof(IBaseFilter)),
                Direction = PinDirection.Input,
                Name = RegionTravel.PlayAviPinName,
            };
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
            id = "In";
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
            allocator = IntPtr.Zero;
            return unchecked((int)0x8004020A);
        }

        public int NotifyAllocator(IntPtr allocator, bool readOnly)
        {
            _ = (allocator, readOnly);
            return 0;
        }

        public int GetAllocatorRequirements(out AllocatorProperties props)
        {
            props = default;
            return unchecked((int)0x80004001);
        }

        public int Receive(IMediaSample sample)
        {
            if (sample.GetPointer(out var data) < 0 || data == IntPtr.Zero)
                return 0;
            var length = sample.GetActualDataLength();
            if (length <= 0)
                length = sample.GetSize();
            if (_width <= 0 || _height <= 0 || length <= 0)
                return 0;
            var rgba = CopySample(data, length);
            if (rgba is not null)
                _onSample(_width, _height, rgba);
            return 0;
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
            _subType = Marshal.PtrToStructure<Guid>(type + 16);
            _width = Marshal.ReadInt32(format, 52);
            var rawHeight = Marshal.ReadInt32(format, 56);
            _height = Math.Abs(rawHeight);
            _topDown = rawHeight < 0;
            _bitCount = _subType == Ds.Rgb32 ? 32 : 24;
            if (formatSize >= 70)
            {
                var bits = Marshal.ReadInt16(format, 62);
                if (bits is 24 or 32)
                    _bitCount = bits;
            }

            // 00A3B5F0: stride = ((width+1)*3) & ~3 for RGB24.
            _stride = _subType == Ds.Yuy2
                ? ((_width * 2 + 3) & ~3)
                : _bitCount == 32
                    ? _width * 4
                    : ((_width + 1) * 3) & ~3;
        }

        private byte[]? CopySample(IntPtr data, int length)
        {
            var pixels = _width * _height;
            if (pixels <= 0)
                return null;
            var rgba = new byte[pixels * 4];
            if (_subType == Ds.Yuy2)
                return CopyYuy2(data, length, rgba);
            var bpp = _bitCount == 32 ? 4 : 3;
            var stride = _stride > 0 ? _stride : ((_width * bpp + 3) & ~3);
            if (length < stride * _height && length >= pixels * bpp)
                stride = _width * bpp;
            // 00A3B730 copies GetPointer row 0 into
            // LockRect row 0 — no V flip.
            BgrToRgba(data, stride, rgba, _width, _height, bpp, flip: false);
            return rgba;
        }

        private unsafe byte[] CopyYuy2(IntPtr data, int length, byte[] rgba)
        {
            var stride = _stride > 0 ? _stride : ((_width * 2 + 3) & ~3);
            if (length < stride * _height)
                stride = _width * 2;
            fixed (byte* d0 = rgba)
            {
                for (var y = 0; y < _height; y++)
                {
                    var s = (byte*)data + y * stride;
                    var d = d0 + y * _width * 4;
                    for (var x = 0; x + 1 < _width; x += 2)
                    {
                        YuvToRgba(s[0], s[1], s[3], d);
                        YuvToRgba(s[2], s[1], s[3], d + 4);
                        s += 4;
                        d += 8;
                    }
                }
            }

            return rgba;
        }

        private static unsafe void YuvToRgba(int y, int u, int v, byte* dest)
        {
            var c = y - 16;
            var d = u - 128;
            var e = v - 128;
            dest[0] = ClampByte((298 * c + 409 * e + 128) >> 8);
            dest[1] = ClampByte((298 * c - 100 * d - 208 * e + 128) >> 8);
            dest[2] = ClampByte((298 * c + 516 * d + 128) >> 8);
            dest[3] = 255;
        }

        private static byte ClampByte(int value) =>
            (byte)Math.Clamp(value, 0, 255);
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
            // 00CA84F0 GetMediaType is
            // E_UNEXPECTED. EnumMediaTypes Next
            // therefore yields no types.
            _ = (count, types);
            _index++;
            if (fetched != IntPtr.Zero)
                Marshal.WriteInt32(fetched, 0);
            return 1;
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
}
