using Fable.Core;
using Fable.Formats.Defs;

namespace Fable.Game;

/// <summary>
/// Retail dialogue resolver used by <c>006E5A00</c>:
/// <c>"SND_" + text key</c> -> Lionhead CRC table in
/// <c>*dialoguesnds.bin</c> -> one-based RIFF index in the authored LUT bank.
/// The LUT is indexed with a bounded streaming scan and is never loaded whole.
/// </summary>
public sealed class DialogueAudioBank(GameInstall install)
{
    private readonly Dictionary<string, DialogueBankIndex> _banks =
        new(StringComparer.OrdinalIgnoreCase);

    public byte[]? Resolve(string textId, string authoredBank)
    {
        if (textId.Length == 0 || authoredBank.Length == 0)
            return null;
        var stem = Path.GetFileNameWithoutExtension(authoredBank);
        if (stem.Length == 0)
            return null;
        if (!_banks.TryGetValue(stem, out var bank))
        {
            bank = DialogueBankIndex.TryOpen(install, stem);
            if (bank is null)
                return null;
            _banks[stem] = bank;
        }
        return bank.Resolve(FableCrc.Hash("SND_" + textId));
    }

    private sealed class DialogueBankIndex
    {
        private readonly string _lutPath;
        private readonly Dictionary<uint, uint> _samples;
        private readonly List<long> _riffOffsets = [];
        private long _scanOffset;
        private bool _scanComplete;

        private DialogueBankIndex(string lutPath, Dictionary<uint, uint> samples)
        {
            _lutPath = lutPath;
            _samples = samples;
        }

        public static DialogueBankIndex? TryOpen(GameInstall install, string stem)
        {
            var lower = stem.ToLowerInvariant();
            var mapName = lower.EndsWith('2')
                ? lower[..^1] + "snds2.bin"
                : lower + "snds.bin";
            var mapPath = Path.Combine(install.DataRoot, "Defs", mapName);
            var lutPath = Path.Combine(
                install.DataRoot, "lang", "English", stem + ".lut");
            if (!File.Exists(mapPath) || !File.Exists(lutPath))
                return null;
            var bytes = File.ReadAllBytes(mapPath);
            if (bytes.Length < 4)
                return null;
            var count = BitConverter.ToUInt32(bytes, 0);
            if (4L + count * 8L > bytes.Length)
                return null;
            var samples = new Dictionary<uint, uint>((int)count);
            for (var i = 0; i < count; i++)
            {
                var at = 4 + i * 8;
                samples[BitConverter.ToUInt32(bytes, at)] =
                    BitConverter.ToUInt32(bytes, at + 4);
            }
            return new DialogueBankIndex(lutPath, samples);
        }

        public byte[]? Resolve(uint crc)
        {
            if (!_samples.TryGetValue(crc, out var oneBased) || oneBased == 0)
                return null;
            EnsureIndexed(oneBased);
            if (oneBased > _riffOffsets.Count)
                return null;
            using var stream = File.OpenRead(_lutPath);
            stream.Position = _riffOffsets[checked((int)oneBased - 1)];
            Span<byte> header = stackalloc byte[8];
            stream.ReadExactly(header);
            var size = BitConverter.ToUInt32(header[4..]);
            if (size < 4 || size > int.MaxValue - 8)
                return null;
            var result = new byte[(int)size + 8];
            header.CopyTo(result);
            stream.ReadExactly(result.AsSpan(8));
            return result;
        }

        /// <summary>
        /// LUT banks contain a Lionhead header and per-sample metadata around
        /// their RIFF files. Scan forward to the next RIFF, skip its declared
        /// body, and stop as soon as the requested one-based sample is found.
        /// The next lookup resumes from the previous file offset, so the first
        /// spoken line no longer scans the complete (often hundreds of MB) bank.
        /// </summary>
        private void EnsureIndexed(uint oneBased)
        {
            if (_scanComplete || oneBased <= _riffOffsets.Count)
                return;
            using var stream = File.OpenRead(_lutPath);
            stream.Position = _scanOffset;
            Span<byte> header = stackalloc byte[8];
            while (_riffOffsets.Count < oneBased)
            {
                var offset = FindNextRiff(stream);
                if (offset < 0)
                {
                    _scanComplete = true;
                    break;
                }
                stream.Position = offset;
                stream.ReadExactly(header);
                var bodyBytes = BitConverter.ToUInt32(header[4..]);
                var next = offset + 8L + bodyBytes + (bodyBytes & 1u);
                if (bodyBytes < 4 || next > stream.Length)
                {
                    _scanComplete = true;
                    break;
                }
                _riffOffsets.Add(offset);
                stream.Position = next;
            }
            _scanOffset = stream.Position;
            if (_scanOffset >= stream.Length)
                _scanComplete = true;
        }

        private static long FindNextRiff(Stream stream)
        {
            Span<byte> buffer = stackalloc byte[16 * 1024];
            uint window = 0;
            var windowBytes = 0;
            while (true)
            {
                var start = stream.Position;
                var read = stream.Read(buffer);
                if (read == 0)
                    return -1;
                for (var i = 0; i < read; i++)
                {
                    window = (window << 8) | buffer[i];
                    if (windowBytes < 4)
                        windowBytes++;
                    if (windowBytes == 4 && window == 0x52494646)
                        return start + i - 3;
                }
            }
        }
    }
}
