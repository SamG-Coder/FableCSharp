using System.Diagnostics;
using System.Runtime.InteropServices;
using Fable.Core;

namespace Fable.Game;

/// <summary>
/// Sample vtbl used by the dumped PlayAVI path:
/// GetTime <c>+20</c>, GetPointer <c>+12</c>,
/// AddRef <c>+4</c>, Release <c>+8</c>.
/// </summary>
public interface IPlayAviSample
{
    int GetTime(out long start, out long end);
    int GetPointer(out IntPtr data);
    int GetActualDataLength();
    int GetSize();
    void AddRef();
    void Release();
}

/// <summary>
/// C# generated from Fable.exe
/// <c>playavi-timeline</c> v6. Method names are
/// VAs. Control flow matches the listings.
/// <c>00A3BCD0</c> never stores a clock, so
/// <c>00CA49F0</c> returns draw-now and
/// <c>00CA4AA0</c> only SetEvent(+84). The
/// decoder queue is the CMemAllocator from
/// <c>00CA89F0</c> plus the one sample at +108.
/// Present wait is <c>00628A9E</c> WaitEx 33.
/// </summary>
public sealed class PlayAviFromExe : IDisposable
{
    public const int StateStopped = 0;
    public const int StatePaused = 1;
    public const int StateRunning = 2;
    public const int WaitForRenderTimeoutMs = 10_000;
    public const int PresentMs = 33;
    public const int D3dFmtA8R8G8B8 = 21;
    public const int D3dFmt555 = 25;
    public const int VfwESampleRejected = unchecked((int)0x8004022B);
    public const int VfwEStateChanged = unchecked((int)0x80040223);
    public const int VfwEStartTimeAfterEnd = unchecked((int)0x80040228);
    public const int VfwEInvalidMediaType = unchecked((int)0x80040200);
    public const int VfwETypeNotAccepted = unchecked((int)0x8004022A);
    public const int EUnexpected = unchecked((int)0x8000FFFF);
    public const int EPointer = unchecked((int)0x80004003);
    public const int ENotImpl = unchecked((int)0x80004001);
    public const int EFail = unchecked((int)0x80004005);
    public const int EarlyDrawHns = 0x13880;
    public const int Rgb24StrideAlign = 4;

    // +20
    public int State { get; private set; }
    // +24 — ctor-zero; 00A3BCD0 does not write it.
    public IntPtr Clock { get; private set; }
    // +100
    public int Streaming { get; private set; }
    // +360
    public int D3dFormat { get; private set; } = D3dFmtA8R8G8B8;
    // +364 / +368 / +372
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Stride { get; private set; }
    public int BitCount { get; private set; } = 24;
    public bool TopDown { get; private set; }
    public byte[]? Rgba { get; private set; }
    public int FrameSerial { get; private set; }
    public int RecvSerial { get; private set; }
    public int LastCopyHr { get; private set; }
    public bool SampleHeld => _sample is not null;
    public int AdviseCookie => _adviseCookie;
    public int InReceive => _inReceive;
    public int Repaint => _repaint;

    private readonly object _interfaceLock = new();
    private readonly object _rendererLock = new();
    private readonly AutoResetEvent _renderEvent = new(false);
    private readonly ManualResetEvent _threadSignal = new(false);
    private readonly AutoResetEvent _evComplete = new(false);
    private readonly AutoResetEvent _playerEvent = new(false);
    private readonly object _textureGate = new();
    private IPlayAviSample? _sample;
    private IntPtr _allocator;
    private int _adviseCookie;
    private int _inReceive;
    private int _repaint;
    private bool _disposed;
    private byte[]? _scratch;

    /// <summary><c>00A3BCD0</c> <c>xor eax,eax; ret</c>.</summary>
    public int SetSyncSource_00A3BCD0(IntPtr clock)
    {
        _ = clock;
        return 0;
    }

    /// <summary><c>00A3BCF0</c> <c>ret</c>.</summary>
    public int DoRenderSample_00A3BCF0(IPlayAviSample? sample)
    {
        _ = sample;
        return 0;
    }

    /// <summary><c>00A3BCE0</c> <c>ret 4</c>.</summary>
    public int Ready_00A3BCE0(IPlayAviSample? sample)
    {
        _ = sample;
        return 0;
    }

    /// <summary>
    /// <c>00A3B590</c>: FORMAT_VideoInfo at +44,
    /// MEDIATYPE_Video at +0, RGB24 at +16.
    /// Failed check is <c>E_FAIL</c> /
    /// <c>E_INVALIDARG</c>; pin QueryAccept maps
    /// any negative to S_FALSE.
    /// </summary>
    public static int AcceptType_00A3B590(Guid major, Guid subtype, Guid formatType)
    {
        if (formatType != RegionTravel.PlayAviFormatVideoInfo)
            return unchecked((int)0x80070057);
        if (major != RegionTravel.PlayAviMediaTypeVideo ||
            subtype != RegionTravel.PlayAviRgb24)
            return EFail;
        return 0;
    }

    /// <summary>
    /// <c>00A3B5F0</c> reads VIH biWidth / abs(biHeight)
    /// and RGB24 stride <c>((w+1)*3)&amp;~3</c>.
    /// Texture format must be 21 or 25.
    /// </summary>
    public int CheckMediaType_00A3B5F0(int width, int height, int bitCount, bool topDown)
    {
        Width = width;
        Height = Math.Abs(height);
        TopDown = topDown || height < 0;
        BitCount = bitCount is 24 or 32 ? bitCount : 24;
        Stride = BitCount == 32
            ? Width * 4
            : ((Width + 1) * 3) & ~3;
        if (D3dFormat is not D3dFmtA8R8G8B8 and not D3dFmt555)
            return VfwETypeNotAccepted;
        return 0;
    }

    /// <summary>
    /// Filter stop: <c>00CA4E30</c> then
    /// <c>00CA6580</c> SetEvent(+88) so
    /// WaitForRenderTime returns
    /// <c>VFW_E_STATE_CHANGED</c>.
    /// </summary>
    public int Stop()
    {
        lock (_interfaceLock)
        {
            State = StateStopped;
            StopStreaming_00CA4E30();
            _threadSignal.Set();
            ClearPendingSample_00CA4BF0();
        }

        return 0;
    }

    /// <summary>
    /// <c>00CA4E30</c> StopStreaming: if +100==1
    /// clear it and call vtbl+68.
    /// </summary>
    public int StopStreaming_00CA4E30()
    {
        lock (_rendererLock)
        {
            if (Streaming == 1)
                Streaming = 0;
        }

        return 0;
    }

    /// <summary>State +20 = 1.</summary>
    public int Pause()
    {
        lock (_interfaceLock)
        {
            State = StatePaused;
            _threadSignal.Reset();
        }

        return 0;
    }

    /// <summary>State +20 = 2 then <c>00CA4D80</c>.</summary>
    public int Run()
    {
        lock (_interfaceLock)
        {
            State = StateRunning;
            _threadSignal.Reset();
        }

        StartStreaming_00CA4D80();
        return 0;
    }

    /// <summary>
    /// <c>00CA4D80</c>: if +100 already 1, leave.
    /// Else +100=1 and ScheduleSample the held
    /// sample so WaitForRenderTime can return.
    /// </summary>
    public int StartStreaming_00CA4D80()
    {
        lock (_rendererLock)
        {
            if (Streaming == 1)
                return 1;
            Streaming = 1;
            if (_sample is null)
                return 0;
            if (ScheduleSample_00CA4AA0(_sample) == 0)
                _renderEvent.Set();
            return 1;
        }
    }

    /// <summary>
    /// <c>00CA49F0</c>. GetTime fail or no clock
    /// → S_OK (draw now). Stop &lt; start →
    /// <c>0x80040228</c>. Clock present →
    /// <c>00CA5850</c>.
    /// </summary>
    public int GetSampleTimes_00CA49F0(
        IPlayAviSample sample, out long start, out long end)
    {
        var hr = sample.GetTime(out start, out end);
        if (hr < 0)
            return 0;
        if (end < start)
            return VfwEStartTimeAfterEnd;
        if (Clock == IntPtr.Zero)
            return 0;
        return ShouldDrawSampleNow_00CA5850(ref start, ref end);
    }

    /// <summary>
    /// <c>00CA5850</c>. Subtracts 8 ms
    /// (<c>0x13880</c>) from start and stop.
    /// Only reached when +24 is non-zero.
    /// Returns S_FALSE (schedule) here because
    /// Fable never stores a clock.
    /// </summary>
    public int ShouldDrawSampleNow_00CA5850(ref long start, ref long end)
    {
        if (start >= EarlyDrawHns)
        {
            start -= EarlyDrawHns;
            end -= EarlyDrawHns;
        }

        return 1;
    }

    /// <summary>
    /// <c>00CA4AA0</c>. GetSampleTimes &lt; 0 fail;
    /// ==0 SetEvent(+84) return 1; &gt;0 would
    /// AdviseTime <c>[clock.vtbl+16]</c> at
    /// <c>00CA4B07</c> — dead without a clock.
    /// </summary>
    public int ScheduleSample_00CA4AA0(IPlayAviSample? sample)
    {
        if (sample is null)
            return 0;
        var hr = GetSampleTimes_00CA49F0(sample, out _, out _);
        if (hr < 0)
            return 0;
        if (hr == 0)
        {
            _renderEvent.Set();
            return 1;
        }

        if (Clock == IntPtr.Zero)
            return 0;
        return 0;
    }

    /// <summary><c>00CA5CF0</c> wrapper around <c>00CA4AA0</c>.</summary>
    public int ScheduleWrapper_00CA5CF0(IPlayAviSample? sample)
    {
        if (ScheduleSample_00CA4AA0(sample) == 0)
            return 0;
        return 1;
    }

    /// <summary>
    /// <c>00CA65B0</c>. WaitForMultipleObjects on
    /// +88 then +84, 10 s, retry on
    /// <c>WAIT_TIMEOUT</c> (0x102). First handle
    /// is state-changed.
    /// </summary>
    public int WaitForRenderTime_00CA65B0()
    {
        WaitHandle[] handles = [_threadSignal, _renderEvent];
        int which;
        do
        {
            if (_disposed)
                return VfwEStateChanged;
            which = WaitHandle.WaitAny(handles, WaitForRenderTimeoutMs);
        } while (which == WaitHandle.WaitTimeout);

        if (which == 0)
            return VfwEStateChanged;
        _adviseCookie = 0;
        return 0;
    }

    /// <summary>
    /// <c>00CA6C40</c>. One sample at +108.
    /// Already-held → SetEvent(+92) and
    /// <c>0x8000FFFF</c>. If +100==1, ScheduleSample
    /// must succeed or <c>VFW_E_SAMPLE_REJECTED</c>.
    /// </summary>
    public int PrepareReceive_00CA6C40(IPlayAviSample sample)
    {
        lock (_interfaceLock)
        {
            _inReceive = 1;
            // 00CA6CF9 lock +148 for +108 / +100 /
            // ScheduleSample. Same lock as 00CA4D80.
            lock (_rendererLock)
            {
                if (_sample is not null)
                {
                    _inReceive = 0;
                    _evComplete.Set();
                    return EUnexpected;
                }

                if (Streaming == 1 && ScheduleWrapper_00CA5CF0(sample) == 0)
                {
                    _inReceive = 0;
                    return VfwESampleRejected;
                }

                _sample = sample;
                sample.AddRef();
                if (Streaming == 0)
                    _repaint = 1;
                return 0;
            }
        }
    }

    /// <summary>
    /// <c>00CA6E10</c> (filter vtbl+156). Pin
    /// <c>00CA7210</c> calls this. Paused path
    /// Ready + SetEvent(+92), then always
    /// WaitForRenderTime, then ExecuteRender.
    /// </summary>
    public int Receive_00CA6E10(IPlayAviSample sample)
    {
        var hr = PrepareReceive_00CA6C40(sample);
        if (hr < 0)
            return hr == VfwESampleRejected ? 0 : hr;

        if (State == StatePaused)
        {
            DoRenderSample_00A3BCF0(sample);
            lock (_interfaceLock)
            {
                if (State == StateStopped)
                {
                    _inReceive = 0;
                    return 0;
                }

                lock (_rendererLock)
                    Ready_00A3BCE0(sample);
            }

            _evComplete.Set();
        }

        hr = WaitForRenderTime_00CA65B0();
        if (hr < 0)
        {
            _inReceive = 0;
            return 0;
        }

        DoRenderSample_00A3BCF0(_sample);
        lock (_interfaceLock)
        {
            if (State == StateStopped)
            {
                _inReceive = 0;
                return 0;
            }

            lock (_rendererLock)
            {
                if (_sample is not null)
                    ExecuteRender_00CA4B20(_sample);
                ClearPendingSample_00CA4BF0();
            }
        }

        _inReceive = 0;
        return 0;
    }

    /// <summary>
    /// <c>00CA7210</c>. <c>[pin+64]</c> filter
    /// vtbl+156. Success returns that HRESULT.
    /// </summary>
    public int PinReceive_00CA7210(IPlayAviSample sample) =>
        Receive_00CA6E10(sample);

    /// <summary>
    /// <c>00CA4B20</c>: OnRenderStart, vtbl+172
    /// copy, OnRenderEnd.
    /// </summary>
    public int ExecuteRender_00CA4B20(IPlayAviSample sample)
    {
        // 00CA4B20: sample null or +100==0 → 1.
        // +100 is streaming, not +20 state.
        if (sample is null || Streaming == 0)
            return 1;
        DoRenderSample_00A3BCF0(sample);
        LastCopyHr = Copy_00A3B730(sample);
        return 0;
    }

    /// <summary>
    /// <c>00A3B730</c> GetPointer then format 21
    /// RGB+0xFF into the one texture. Ends at
    /// <c>00A3B8EB</c> SetEvent(player+124).
    /// </summary>
    public int Copy_00A3B730(IPlayAviSample sample)
    {
        var gp = sample.GetPointer(out var data);
        if (gp < 0 || data == IntPtr.Zero)
            return 0;
        var length = sample.GetActualDataLength();
        if (length <= 0)
            length = sample.GetSize();
        if (Width <= 0 || Height <= 0 || length <= 0)
            return 0;

        sample.GetTime(out var start, out var end);
        PlayAviTimeline.Note("gettime", PlayAviTimeline.SiteGetSampleTimes, RecvSerial + 1, start, end);

        var pixels = Width * Height;
        var need = pixels * 4;
        if (_scratch is null || _scratch.Length != need)
            _scratch = new byte[need];
        var bpp = BitCount == 32 ? 4 : 3;
        var stride = Stride > 0 ? Stride : ((Width * bpp + 3) & ~3);
        if (length < stride * Height && length >= pixels * bpp)
            stride = Width * bpp;

        // Format 21: byte0..2 + 0xFF. Consume is
        // RGBA for the Vulkan blit (1-t.y).
        CopyRgb24ToRgba(data, stride, _scratch, Width, Height, bpp);

        lock (_textureGate)
        {
            if (Rgba is null || Rgba.Length != need)
                Rgba = new byte[need];
            _scratch.AsSpan().CopyTo(Rgba);
            RecvSerial++;
            FrameSerial = RecvSerial;
        }

        PlayAviTimeline.Note("copy", PlayAviTimeline.SiteCopy, RecvSerial, start, end);
        SetEvent_00A3B8EB();
        return 0;
    }

    /// <summary><c>00A3B8EB</c> SetEvent([player+124]).</summary>
    public void SetEvent_00A3B8EB()
    {
        _playerEvent.Set();
        PlayAviTimeline.Note("setevent", PlayAviTimeline.SiteSetEvent, FrameSerial);
    }

    /// <summary>
    /// <c>00628A9E</c>
    /// <c>WaitForSingleObjectEx([player+124], 33, TRUE)</c>.
    /// Timeout still presents the last texture.
    /// </summary>
    public bool WaitEx_00628A9E()
    {
        var wait0 = Stopwatch.GetTimestamp();
        PlayAviTimeline.Note("wait-enter", PlayAviTimeline.SiteWaitEnter, FrameSerial);
        var signaled = _playerEvent.WaitOne(PresentMs);
        WmvPlayer.PresentWaitTicks += Stopwatch.GetTimestamp() - wait0;
        PlayAviTimeline.Note(
            "wait-leave",
            PlayAviTimeline.SiteWaitLeave,
            FrameSerial,
            waitResult: signaled ? PlayAviTimeline.WaitObject0 : PlayAviTimeline.WaitTimeout);
        return Rgba is not null;
    }

    /// <summary><c>00CA4BF0</c> Release +108.</summary>
    public void ClearPendingSample_00CA4BF0()
    {
        lock (_rendererLock)
        {
            _sample?.Release();
            _sample = null;
        }
    }

    /// <summary>
    /// <c>00CA89F0</c> CoCreate
    /// <c>CLSID_MemoryAllocator</c> +
    /// <c>IID_IMemAllocator</c> when [pin+4] is 0.
    /// </summary>
    public int GetAllocator_00CA89F0(out IntPtr allocator)
    {
        if (_allocator == IntPtr.Zero)
        {
            var clsid = RegionTravel.PlayAviMemoryAllocatorClsid;
            var iid = RegionTravel.PlayAviIMemAllocatorIid;
            var hr = CoCreateInstance(
                ref clsid, IntPtr.Zero, 1, ref iid, out _allocator);
            if (hr < 0 || _allocator == IntPtr.Zero)
            {
                allocator = IntPtr.Zero;
                _allocator = IntPtr.Zero;
                return hr < 0 ? hr : EFail;
            }
        }

        allocator = _allocator;
        Marshal.AddRef(_allocator);
        return 0;
    }

    /// <summary><c>00CA8AC0</c> store + AddRef, Release old.</summary>
    public int NotifyAllocator_00CA8AC0(IntPtr allocator, bool readOnly)
    {
        _ = readOnly;
        if (allocator == IntPtr.Zero)
            return EPointer;
        if (_allocator != IntPtr.Zero)
            Marshal.Release(_allocator);
        _allocator = allocator;
        Marshal.AddRef(_allocator);
        return 0;
    }

    /// <summary><c>00CA8EC0</c> <c>mov eax, 0x80004001; ret 8</c>.</summary>
    public int GetAllocatorRequirements_00CA8EC0(out int count, out int size, out int align, out int prefix)
    {
        count = 0;
        size = 0;
        align = 0;
        prefix = 0;
        return ENotImpl;
    }

    /// <summary>
    /// <c>00CA8D50</c> on a one-input renderer:
    /// no output pin → S_FALSE.
    /// </summary>
    public int ReceiveCanBlock_00CA8D50() => 0;

    /// <summary><c>00CA8CF0</c> loops pin Receive.</summary>
    public int ReceiveMultiple_00CA8CF0(IPlayAviSample[] samples, int count, out int processed)
    {
        processed = 0;
        for (var i = 0; i < count; i++)
        {
            var hr = PinReceive_00CA7210(samples[i]);
            if (hr != 0)
                return hr;
            processed++;
        }

        return 0;
    }

    public void SnapshotTexture(out int width, out int height, out byte[]? rgba, out int serial)
    {
        lock (_textureGate)
        {
            width = Width;
            height = Height;
            rgba = Rgba;
            serial = FrameSerial;
        }
    }

    public void Dispose()
    {
        _disposed = true;
        Stop();
        _renderEvent.Dispose();
        _threadSignal.Dispose();
        _evComplete.Dispose();
        _playerEvent.Dispose();
        if (_allocator != IntPtr.Zero)
        {
            Marshal.Release(_allocator);
            _allocator = IntPtr.Zero;
        }
    }

    private static unsafe void CopyRgb24ToRgba(
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

    [DllImport("ole32.dll")]
    private static extern int CoCreateInstance(
        ref Guid clsid, IntPtr outer, int ctx, ref Guid iid, out IntPtr ppv);
}
