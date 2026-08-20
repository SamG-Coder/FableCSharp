using Fable.Core;
using NAudio.Vorbis;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Fable.Client;

/// <summary>
/// Plays the retail frontend music and the CS_GUI cues embedded as RIFF
/// segments in Sound/Frontend.lug. Assets are decoded once; hover never reads
/// from disk or rebuilds an audio device.
/// </summary>
public sealed class FrontendAudioPlayer : IDisposable
{
    private readonly string _musicPath;
    private readonly Dictionary<string, CachedSound> _cues;
    private WaveOutEvent? _musicOutput;
    private VorbisWaveReader? _musicReader;
    private LoopWaveStream? _musicLoop;
    private readonly WaveOutEvent? _cueOutput;
    private readonly MixingSampleProvider? _cueMixer;
    private int _serial = -1;

    public FrontendAudioPlayer(GameInstall install)
    {
        ArgumentNullException.ThrowIfNull(install);
        _musicPath = Path.Combine(install.SoundDirectory, "Intro.ogg");
        _cues = LoadFrontendCues(Path.Combine(install.SoundDirectory, "Frontend.lug"));
        try
        {
            _cueMixer = new MixingSampleProvider(
                WaveFormat.CreateIeeeFloatWaveFormat(44100, 2))
            {
                ReadFully = true,
            };
            _cueOutput = new WaveOutEvent();
            _cueOutput.Init(_cueMixer);
            _cueOutput.Play();
        }
        catch
        {
            _cueOutput?.Dispose();
            _cueOutput = null;
            _cueMixer = null;
        }
    }

    public void Sync(bool frontendActive, string cue, int serial)
    {
        if (frontendActive)
            EnsureMusic();
        else
            StopMusic();

        if (serial == _serial)
            return;
        _serial = serial;
        if (_cues.TryGetValue(cue, out var wave))
            _cueMixer?.AddMixerInput(new CachedSoundSampleProvider(wave));
    }

    private void EnsureMusic()
    {
        if (_musicOutput is not null || !File.Exists(_musicPath))
            return;
        try
        {
            _musicReader = new VorbisWaveReader(_musicPath);
            _musicLoop = new LoopWaveStream(_musicReader);
            _musicOutput = new WaveOutEvent();
            _musicOutput.Init(_musicLoop);
            _musicOutput.Play();
        }
        catch
        {
            StopMusic();
        }
    }

    private void StopMusic()
    {
        _musicOutput?.Dispose();
        _musicOutput = null;
        _musicLoop?.Dispose();
        _musicLoop = null;
        _musicReader?.Dispose();
        _musicReader = null;
    }

    private static Dictionary<string, CachedSound> LoadFrontendCues(string path)
    {
        var result = new Dictionary<string, CachedSound>(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(path))
            return result;
        var bank = File.ReadAllBytes(path);
        var waves = new List<byte[]>();
        for (var offset = 0; offset <= bank.Length - 12; offset++)
        {
            if (bank[offset] != (byte)'R' || bank[offset + 1] != (byte)'I' ||
                bank[offset + 2] != (byte)'F' || bank[offset + 3] != (byte)'F')
                continue;
            var payload = BitConverter.ToUInt32(bank, offset + 4);
            var length64 = 8L + payload;
            if (length64 < 12 || length64 > int.MaxValue || offset + length64 > bank.Length)
                continue;
            var length = (int)length64;
            waves.Add(bank.AsSpan(offset, length).ToArray());
            offset += length - 1;
        }

        // Frontend.met maps CS_GUI_1 and CS_GUI_2 to the GUI_01 and GUI_02
        // samples, which are RIFF segments 2 and 3 in Frontend.lug.
        if (waves.Count > 1)
            result["CS_GUI_1"] = CachedSound.Decode(waves[1]);
        if (waves.Count > 2)
            result["CS_GUI_2"] = CachedSound.Decode(waves[2]);
        return result;
    }

    public void Dispose()
    {
        _cueOutput?.Dispose();
        StopMusic();
    }

    private sealed class LoopWaveStream(WaveStream source) : WaveStream
    {
        public override WaveFormat WaveFormat => source.WaveFormat;
        public override long Length => source.Length;
        public override long Position
        {
            get => source.Position;
            set => source.Position = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var total = 0;
            while (total < count)
            {
                var read = source.Read(buffer, offset + total, count - total);
                if (read == 0)
                {
                    source.Position = 0;
                    continue;
                }
                total += read;
            }
            return total;
        }

        protected override void Dispose(bool disposing)
        {
            // The owner disposes the underlying Vorbis reader.
            base.Dispose(disposing);
        }
    }

    private sealed record CachedSound(float[] Samples)
    {
        public static CachedSound Decode(byte[] wave)
        {
            using var stream = new MemoryStream(wave, writable: false);
            using var reader = new WaveFileReader(stream);
            ISampleProvider provider = reader.ToSampleProvider();
            if (provider.WaveFormat.Channels == 1)
                provider = new MonoToStereoSampleProvider(provider);
            if (provider.WaveFormat.SampleRate != 44100)
                provider = new WdlResamplingSampleProvider(provider, 44100);
            var samples = new List<float>();
            var chunk = new float[4096];
            int read;
            while ((read = provider.Read(chunk, 0, chunk.Length)) > 0)
                samples.AddRange(chunk.AsSpan(0, read).ToArray());
            return new CachedSound(samples.ToArray());
        }
    }

    private sealed class CachedSoundSampleProvider(CachedSound sound) : ISampleProvider
    {
        private int _position;
        public WaveFormat WaveFormat { get; } =
            WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

        public int Read(float[] buffer, int offset, int count)
        {
            var available = sound.Samples.Length - _position;
            var copy = Math.Min(available, count);
            if (copy <= 0)
                return 0;
            Array.Copy(sound.Samples, _position, buffer, offset, copy);
            _position += copy;
            return copy;
        }
    }
}
