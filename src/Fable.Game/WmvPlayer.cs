using System.Runtime.InteropServices;

namespace Fable.Game;

/// <summary>
/// <c>00A3B9D0</c> DirectShow / WMV path when the
/// rewritten name ends <c>.wmv</c> / <c>.asf</c>.
/// Media Foundation SourceReader is that same ASF
/// graph on modern Windows. First sample is the
/// first presented frame; EOF ends the blocking
/// apply the way <c>006286F0</c> returns.
/// </summary>
public sealed class WmvPlayer : IDisposable
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public byte[]? Rgba { get; private set; }
    public bool Ended { get; private set; }

    private IMFSourceReader? _reader;
    private long _elapsedHns;
    private bool _started;
    private string? _aviPath;

    public static string? LastError { get; private set; }

    public static WmvPlayer? TryOpen(string path)
    {
        LastError = null;
        if (!File.Exists(path) || !RegionTravel.FileHasAsfMagic(path))
        {
            LastError = "missing-or-not-asf";
            return null;
        }

        var start = Mf.MFStartup(Mf.Version, Mf.StartupLite);
        if (start < 0)
        {
            LastError = $"MFStartup {start:X8}";
            return null;
        }

        var started = true;
        try
        {
            var url = path.StartsWith("file:", StringComparison.OrdinalIgnoreCase)
                ? path
                : "file:///" + path.Replace('\\', '/');
            var hr = Mf.MFCreateSourceReaderFromURL(url, IntPtr.Zero, out var reader);
            if (hr < 0 || reader is null)
            {
                LastError = $"SourceReader {hr:X8}";
                Mf.MFShutdown();
                return null;
            }

            var player = new WmvPlayer { _reader = reader, _started = true, _aviPath = path };
            if (!player.ConfigureRgb32() || !player.ReadUntil(0))
            {
                LastError ??= "configure-or-first-sample";
                player.Dispose();
                return null;
            }

            return player;
        }
        catch (Exception ex)
        {
            LastError = ex.GetType().Name + ": " + ex.Message;
            if (started)
                Mf.MFShutdown();
            return null;
        }
    }

    public bool TryAdvance(float dt)
    {
        if (Ended || _reader is null)
            return false;
        if (dt > 0f)
            _elapsedHns += (long)(dt * 10_000_000d);
        return ReadUntil(_elapsedHns);
    }

    private bool ConfigureRgb32()
    {
        if (_reader is null)
            return false;
        _reader.SetStreamSelection(Mf.AnyStream, false);
        _reader.SetStreamSelection(Mf.FirstVideoStream, true);
        TryReadFrameSizeFromType();
        if (Width <= 0 || Height <= 0)
            TryReadAsfBitmapSize();

        var hr = Mf.MFCreateMediaType(out var type);
        if (hr < 0 || type is null)
        {
            LastError = $"MFCreateMediaType {hr:X8}";
            return false;
        }

        type.SetGUID(Mf.MajorType, Mf.Video);
        type.SetGUID(Mf.Subtype, Mf.Rgb32);
        if (Width > 0 && Height > 0)
            type.SetUINT64(Mf.FrameSize, ((ulong)(uint)Width << 32) | (uint)Height);
        try
        {
            _reader.SetCurrentMediaType(Mf.FirstVideoStream, IntPtr.Zero, type);
        }
        catch (Exception ex)
        {
            LastError = $"SetCurrentMediaType {Width}x{Height}: {ex.Message}";
            Marshal.ReleaseComObject(type);
            return false;
        }

        Marshal.ReleaseComObject(type);
        if (Width <= 0 || Height <= 0)
            TryReadFrameSizeFromType();
        return true;
    }

    private void TryReadFrameSizeFromType()
    {
        if (_reader is null)
            return;
        try
        {
            _reader.GetNativeMediaType(Mf.FirstVideoStream, 0, out var native);
            if (native is null)
                return;
            try
            {
                if (TryPackedSize(native, out var w, out var h))
                {
                    Width = w;
                    Height = h;
                }
            }
            finally
            {
                Marshal.ReleaseComObject(native);
            }
        }
        catch
        {
            // Frame size may live in the ASF BITMAPINFOHEADER instead.
        }
    }

    private static bool TryPackedSize(IMFMediaType type, out int width, out int height)
    {
        width = 0;
        height = 0;
        try
        {
            type.GetUINT64(Mf.FrameSize, out var packed);
            width = (int)(packed >> 32);
            height = (int)(packed & 0xFFFFFFFF);
            return width > 0 && height > 0;
        }
        catch
        {
            return false;
        }
    }

    private void InferSizeFromLength(int length)
    {
        if (length < 16 * 16 * 4 || length % 4 != 0)
            return;
        var pixels = length / 4;
        foreach (var (w, h) in (ReadOnlySpan<(int, int)>)[(640, 480), (720, 480), (854, 480), (1280, 720), (320, 240)])
        {
            if (w * h != pixels)
                continue;
            Width = w;
            Height = h;
            return;
        }

        var height = (int)Math.Round(Math.Sqrt(pixels * 3.0 / 4.0));
        if (height > 0 && pixels % height == 0)
        {
            Width = pixels / height;
            Height = height;
        }
    }

    private void TryReadAsfBitmapSize()
    {
        if (_aviPath is null)
            return;
        var header = new byte[Math.Min(1 << 18, (int)new FileInfo(_aviPath).Length)];
        using (var stream = File.OpenRead(_aviPath))
            _ = stream.Read(header);
        var bytes = header;
        for (var i = 0; i + 12 <= bytes.Length; i++)
        {
            if (BitConverter.ToInt32(bytes, i) != 40)
                continue;
            var width = BitConverter.ToInt32(bytes, i + 4);
            var height = Math.Abs(BitConverter.ToInt32(bytes, i + 8));
            if (width is >= 16 and <= 4096 && height is >= 16 and <= 4096)
            {
                Width = width;
                Height = height;
                return;
            }
        }
    }

    private bool ReadUntil(long targetHns)
    {
        if (_reader is null)
            return false;
        while (true)
        {
            var hr = _reader.ReadSample(
                Mf.FirstVideoStream, 0,
                out _, out var flags, out var time, out var samplePtr);
            if (hr < 0)
            {
                LastError = $"ReadSample {hr:X8}";
                return Rgba is not null;
            }

            if ((flags & Mf.EndOfStream) != 0)
            {
                Ended = true;
                if (samplePtr != IntPtr.Zero)
                    Marshal.Release(samplePtr);
                return Rgba is not null;
            }

            if (samplePtr == IntPtr.Zero)
            {
                LastError = $"ReadSample empty hr={hr:X8} flags={flags} time={time}";
                return Rgba is not null;
            }

            try
            {
                CopySample(samplePtr);
            }
            catch (Exception ex)
            {
                LastError = $"CopySample hr={hr:X8} flags={flags} ptr={samplePtr.ToInt64():X} {ex.Message}";
                Marshal.Release(samplePtr);
                return false;
            }

            Marshal.Release(samplePtr);
            if (Rgba is null)
                return false;
            if (time >= targetHns)
                return true;
        }
    }

    private void CopySample(IntPtr samplePtr)
    {
        var sampleIid = typeof(IMFSample).GUID;
        var bufferIid = typeof(IMFMediaBuffer).GUID;
        IMFMediaBuffer? buffer;
        if (Marshal.QueryInterface(samplePtr, ref sampleIid, out var sampleUnk) >= 0)
        {
            var sample = (IMFSample)Marshal.GetUniqueObjectForIUnknown(sampleUnk);
            Marshal.Release(sampleUnk);
            sample.ConvertToContiguousBuffer(out buffer);
        }
        else if (Marshal.QueryInterface(samplePtr, ref bufferIid, out var bufferUnk) >= 0)
        {
            buffer = (IMFMediaBuffer)Marshal.GetUniqueObjectForIUnknown(bufferUnk);
            Marshal.Release(bufferUnk);
        }
        else
        {
            LastError = $"sample-qi ptr={samplePtr.ToInt64():X}";
            return;
        }
        if (buffer is null)
            return;
        buffer.Lock(out var data, out _, out var length);
        try
        {
            if (Width <= 0 || Height <= 0)
                InferSizeFromLength(length);
            var pixels = Width * Height;
            if (pixels <= 0 || length < pixels * 4)
                return;
            var bgra = new byte[pixels * 4];
            Marshal.Copy(data, bgra, 0, bgra.Length);
            var rgba = Rgba is { Length: var n } && n == pixels * 4
                ? Rgba
                : new byte[pixels * 4];
            for (var i = 0; i < pixels; i++)
            {
                var o = i * 4;
                rgba[o] = bgra[o + 2];
                rgba[o + 1] = bgra[o + 1];
                rgba[o + 2] = bgra[o];
                rgba[o + 3] = 255;
            }

            Rgba = rgba;
        }
        finally
        {
            buffer.Unlock();
            Marshal.ReleaseComObject(buffer);
        }
    }

    public void Dispose()
    {
        if (_reader is not null)
        {
            Marshal.ReleaseComObject(_reader);
            _reader = null;
        }

        if (_started)
        {
            Mf.MFShutdown();
            _started = false;
        }
    }

    private static class Mf
    {
        public const int Version = 0x00020070;
        public const int StartupLite = 1;
        public const int FirstVideoStream = unchecked((int)0xFFFFFFFC);
        public const int AnyStream = unchecked((int)0xFFFFFFFE);
        public const int EndOfStream = 0x2;
        public static readonly Guid MajorType = new("48eba18e-f8c9-4687-bf11-0a74c9f96a8f");
        public static readonly Guid Subtype = new("f7e34c9a-42e8-4714-b74b-cb29d72c35e5");
        public static readonly Guid Video = new("73646976-0000-0010-8000-00AA00389B71");
        public static readonly Guid Rgb32 = new("00000016-0000-0010-8000-00AA00389B71");
        public static readonly Guid FrameSize = new("18231bfc-49ed-4d60-9e6c-d69c6afd7d1e");

        [DllImport("mfplat.dll")]
        public static extern int MFStartup(int version, int flags);

        [DllImport("mfplat.dll")]
        public static extern int MFShutdown();

        [DllImport("mfplat.dll")]
        public static extern int MFCreateMediaType(out IMFMediaType type);

        [DllImport("mfreadwrite.dll", CharSet = CharSet.Unicode)]
        public static extern int MFCreateSourceReaderFromURL(
            string url, IntPtr attributes, out IMFSourceReader reader);
    }

    [ComImport]
    [Guid("2cd2d921-c447-44a7-a13c-4adabfc247e3")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMFAttributes
    {
        void GetItem(in Guid key, IntPtr value);
        void GetItemType(in Guid key, out int type);
        void CompareItem(in Guid key, IntPtr value, out int result);
        void Compare(IMFAttributes other, int match, out int result);
        void GetUINT32(in Guid key, out int value);
        void GetUINT64(in Guid key, out ulong value);
        void GetDouble(in Guid key, out double value);
        void GetGUID(in Guid key, out Guid value);
        void GetStringLength(in Guid key, out int length);
        void GetString(in Guid key, IntPtr value, int size, out int length);
        void GetAllocatedString(in Guid key, out IntPtr value, out int length);
        void GetBlobSize(in Guid key, out int size);
        void GetBlob(in Guid key, IntPtr buf, int size, out int written);
        void GetAllocatedBlob(in Guid key, out IntPtr buf, out int size);
        void GetUnknown(in Guid key, in Guid iid, out IntPtr unk);
        void SetItem(in Guid key, IntPtr value);
        void DeleteItem(in Guid key);
        void DeleteAllItems();
        void SetUINT32(in Guid key, int value);
        void SetUINT64(in Guid key, ulong value);
        void SetDouble(in Guid key, double value);
        void SetGUID(in Guid key, in Guid value);
        void SetString(in Guid key, [MarshalAs(UnmanagedType.LPWStr)] string value);
        void SetBlob(in Guid key, IntPtr buf, int size);
        void SetUnknown(in Guid key, [MarshalAs(UnmanagedType.IUnknown)] object unk);
        void LockStore();
        void UnlockStore();
        void GetCount(out int count);
        void GetItemByIndex(int index, out Guid key, IntPtr value);
        void CopyAllItems(IMFAttributes dest);
    }

    [ComImport]
    [Guid("44ae0fa8-ea31-4109-8d2e-4cae4997c555")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMFMediaType
    {
        void GetItem(in Guid key, IntPtr value);
        void GetItemType(in Guid key, out int type);
        void CompareItem(in Guid key, IntPtr value, out int result);
        void Compare(IntPtr other, int match, out int result);
        void GetUINT32(in Guid key, out int value);
        void GetUINT64(in Guid key, out ulong value);
        void GetDouble(in Guid key, out double value);
        void GetGUID(in Guid key, out Guid value);
        void GetStringLength(in Guid key, out int length);
        void GetString(in Guid key, IntPtr value, int size, out int length);
        void GetAllocatedString(in Guid key, out IntPtr value, out int length);
        void GetBlobSize(in Guid key, out int size);
        void GetBlob(in Guid key, IntPtr buf, int size, out int written);
        void GetAllocatedBlob(in Guid key, out IntPtr buf, out int size);
        void GetUnknown(in Guid key, in Guid iid, out IntPtr unk);
        void SetItem(in Guid key, IntPtr value);
        void DeleteItem(in Guid key);
        void DeleteAllItems();
        void SetUINT32(in Guid key, int value);
        void SetUINT64(in Guid key, ulong value);
        void SetDouble(in Guid key, double value);
        void SetGUID(in Guid key, in Guid value);
        void SetString(in Guid key, [MarshalAs(UnmanagedType.LPWStr)] string value);
        void SetBlob(in Guid key, IntPtr buf, int size);
        void SetUnknown(in Guid key, [MarshalAs(UnmanagedType.IUnknown)] object unk);
        void LockStore();
        void UnlockStore();
        void GetCount(out int count);
        void GetItemByIndex(int index, out Guid key, IntPtr value);
        void CopyAllItems(IntPtr dest);
        void GetMajorType(out Guid type);
        void IsCompressedFormat(out int compressed);
        void IsEqual(IntPtr other, out int flags);
        void GetRepresentation(in Guid guid, out IntPtr pv);
        void FreeRepresentation(in Guid guid, IntPtr pv);
    }

    [ComImport]
    [Guid("70ae66f2-c809-4e4f-8915-bdcb406b7993")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMFSourceReader
    {
        void GetStreamSelection(int index, out int selected);
        void SetStreamSelection(int index, [MarshalAs(UnmanagedType.Bool)] bool selected);
        void GetNativeMediaType(int stream, int typeIndex, out IMFMediaType native);
        void GetCurrentMediaType(int stream, out IMFMediaType current);
        void SetCurrentMediaType(int stream, IntPtr reserved, IMFMediaType type);
        void SetCurrentPosition(ref Guid format, IntPtr position);
        [PreserveSig]
        int ReadSample(
            int stream, int flags,
            out int actual, out int sampleFlags, out long timestamp,
            out IntPtr sample);
        void Flush(int stream);
        void GetServiceForStream(int stream, in Guid service, in Guid iid, out IntPtr unk);
        void GetPresentationAttribute(int stream, in Guid guid, IntPtr value);
    }

    [ComImport]
    [Guid("c40a00f2-b397-4bdf-ab59-09d1738ec2da")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMFSample
    {
        void GetItem(in Guid key, IntPtr value);
        void GetItemType(in Guid key, out int type);
        void CompareItem(in Guid key, IntPtr value, out int result);
        void Compare(IntPtr other, int match, out int result);
        void GetUINT32(in Guid key, out int value);
        void GetUINT64(in Guid key, out ulong value);
        void GetDouble(in Guid key, out double value);
        void GetGUID(in Guid key, out Guid value);
        void GetStringLength(in Guid key, out int length);
        void GetString(in Guid key, IntPtr value, int size, out int length);
        void GetAllocatedString(in Guid key, out IntPtr value, out int length);
        void GetBlobSize(in Guid key, out int size);
        void GetBlob(in Guid key, IntPtr buf, int size, out int written);
        void GetAllocatedBlob(in Guid key, out IntPtr buf, out int size);
        void GetUnknown(in Guid key, in Guid iid, out IntPtr unk);
        void SetItem(in Guid key, IntPtr value);
        void DeleteItem(in Guid key);
        void DeleteAllItems();
        void SetUINT32(in Guid key, int value);
        void SetUINT64(in Guid key, ulong value);
        void SetDouble(in Guid key, double value);
        void SetGUID(in Guid key, in Guid value);
        void SetString(in Guid key, [MarshalAs(UnmanagedType.LPWStr)] string value);
        void SetBlob(in Guid key, IntPtr buf, int size);
        void SetUnknown(in Guid key, [MarshalAs(UnmanagedType.IUnknown)] object unk);
        void LockStore();
        void UnlockStore();
        void GetCount(out int count);
        void GetItemByIndex(int index, out Guid key, IntPtr value);
        void CopyAllItems(IntPtr dest);
        void GetSampleFlags(out int flags);
        void SetSampleFlags(int flags);
        void GetSampleTime(out long time);
        void SetSampleTime(long time);
        void GetSampleDuration(out long duration);
        void SetSampleDuration(long duration);
        void GetBufferCount(out int count);
        void GetBufferByIndex(int index, out IMFMediaBuffer buffer);
        void ConvertToContiguousBuffer(out IMFMediaBuffer buffer);
        void AddBuffer(IMFMediaBuffer buffer);
        void RemoveBufferByIndex(int index);
        void RemoveAllBuffers();
        void GetTotalLength(out int length);
        void CopyToBuffer(IMFMediaBuffer buffer);
    }

    [ComImport]
    [Guid("045fa593-8799-42b8-bc8d-8968c6453507")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMFMediaBuffer
    {
        void Lock(out IntPtr data, out int max, out int current);
        void Unlock();
        void GetCurrentLength(out int length);
        void SetCurrentLength(int length);
        void GetMaxLength(out int length);
    }
}
