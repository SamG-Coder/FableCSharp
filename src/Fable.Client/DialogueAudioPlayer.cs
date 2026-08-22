using Fable.Core;
using Fable.Game;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Fable.Client;

/// <summary>Plays authored dialogue LUT entries without materialising a bank.</summary>
public sealed class DialogueAudioPlayer : IDisposable
{
    private readonly DialogueAudioBank _bank;
    private readonly MixingSampleProvider? _mixer;
    private readonly WaveOutEvent? _output;
    private int _serial = -1;

    public DialogueAudioPlayer(GameInstall install)
    {
        _bank = new DialogueAudioBank(install);
        try
        {
            _mixer = new MixingSampleProvider(
                WaveFormat.CreateIeeeFloatWaveFormat(44100, 2))
            {
                ReadFully = true,
            };
            _output = new WaveOutEvent();
            _output.Init(_mixer);
            _output.Play();
        }
        catch
        {
            _output?.Dispose();
            _output = null;
            _mixer = null;
        }
    }

    public void Sync(Fable.Game.Scripting.DialogueRuntime? dialogue)
    {
        if (dialogue is null || dialogue.Serial == _serial)
            return;
        _serial = dialogue.Serial;
        var session = dialogue.Session;
        if (session is null || session.Text.Length == 0 || session.AudioBank.Length == 0)
            return;
        var riff = _bank.Resolve(session.Text, session.AudioBank);
        if (riff is null || !XboxIma.TryDecode(riff, out var sound))
            return;
        _mixer?.AddMixerInput(new DialogueSampleProvider(sound));
    }

    public void Dispose() => _output?.Dispose();

    private sealed record CachedDialogue(float[] Samples);

    private sealed class DialogueSampleProvider(CachedDialogue sound) : ISampleProvider
    {
        private int _position;
        public WaveFormat WaveFormat { get; } =
            WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

        public int Read(float[] buffer, int offset, int count)
        {
            var copy = Math.Min(count, sound.Samples.Length - _position);
            if (copy <= 0)
                return 0;
            Array.Copy(sound.Samples, _position, buffer, offset, copy);
            _position += copy;
            return copy;
        }
    }

    private static class XboxIma
    {
        private static readonly int[] IndexDelta = [-1, -1, -1, -1, 2, 4, 6, 8];
        private static readonly int[] Step =
        [
            7, 8, 9, 10, 11, 12, 13, 14, 16, 17, 19, 21, 23, 25, 28, 31,
            34, 37, 41, 45, 50, 55, 60, 66, 73, 80, 88, 97, 107, 118, 130,
            143, 157, 173, 190, 209, 230, 253, 279, 307, 337, 371, 408, 449,
            494, 544, 598, 658, 724, 796, 876, 963, 1060, 1166, 1282, 1411,
            1552, 1707, 1878, 2066, 2272, 2499, 2749, 3024, 3327, 3660, 4026,
            4428, 4871, 5358, 5894, 6484, 7132, 7845, 8630, 9493, 10442,
            11487, 12635, 13899, 15289, 16818, 18500, 20350, 22385, 24623,
            27086, 29794, 32767,
        ];

        public static bool TryDecode(byte[] riff, out CachedDialogue sound)
        {
            sound = new CachedDialogue([]);
            if (riff.Length < 44 || !riff.AsSpan(0, 4).SequenceEqual("RIFF"u8))
                return false;
            var channels = 0;
            var rate = 0;
            var blockAlign = 0;
            ReadOnlySpan<byte> data = [];
            for (var at = 12; at + 8 <= riff.Length;)
            {
                var size = checked((int)BitConverter.ToUInt32(riff, at + 4));
                var body = at + 8;
                if (size < 0 || body + (long)size > riff.Length)
                    return false;
                var id = riff.AsSpan(at, 4);
                if (id.SequenceEqual("fmt "u8) && size >= 16)
                {
                    if (BitConverter.ToUInt16(riff, body) != 0x69)
                        return false;
                    channels = BitConverter.ToUInt16(riff, body + 2);
                    rate = BitConverter.ToInt32(riff, body + 4);
                    blockAlign = BitConverter.ToUInt16(riff, body + 12);
                }
                else if (id.SequenceEqual("data"u8))
                    data = riff.AsSpan(body, size);
                at = body + size + (size & 1);
            }
            if (channels is < 1 or > 2 || rate <= 0 || blockAlign <= channels * 4 || data.IsEmpty)
                return false;

            var pcm = new List<short>((data.Length / blockAlign) * 64 * channels);
            for (var block = 0; block + blockAlign <= data.Length; block += blockAlign)
                DecodeBlock(data.Slice(block, blockAlign), channels, pcm);
            if (pcm.Count == 0)
                return false;

            var sourceFrames = pcm.Count / channels;
            var targetFrames = Math.Max(1, (int)Math.Round(sourceFrames * (44100d / rate)));
            var stereo = new float[targetFrames * 2];
            for (var frame = 0; frame < targetFrames; frame++)
            {
                var source = Math.Min(sourceFrames - 1, (int)(frame * (rate / 44100d)));
                var left = pcm[source * channels] / 32768f;
                var right = channels == 1 ? left : pcm[source * channels + 1] / 32768f;
                stereo[frame * 2] = left;
                stereo[frame * 2 + 1] = right;
            }
            sound = new CachedDialogue(stereo);
            return true;
        }

        private static void DecodeBlock(ReadOnlySpan<byte> block, int channels, List<short> output)
        {
            var predictor = new int[2];
            var index = new int[2];
            for (var channel = 0; channel < channels; channel++)
            {
                var at = channel * 4;
                predictor[channel] = BitConverter.ToInt16(block.Slice(at, 2));
                index[channel] = Math.Clamp((int)block[at + 2], 0, 88);
            }
            var samples = new short[64 * channels];
            var written = new int[channels];
            var cursor = channels * 4;
            while (cursor + channels * 4 <= block.Length)
            {
                for (var channel = 0; channel < channels; channel++)
                {
                    for (var i = 0; i < 4; i++)
                    {
                        var value = block[cursor + channel * 4 + i];
                        Put(channel, value & 0xF);
                        Put(channel, value >> 4);
                    }
                }
                cursor += channels * 4;
            }
            var frames = written.Min();
            for (var frame = 0; frame < frames; frame++)
                for (var channel = 0; channel < channels; channel++)
                    output.Add(samples[frame * channels + channel]);

            void Put(int channel, int nibble)
            {
                if (written[channel] >= 64)
                    return;
                var step = Step[index[channel]];
                var delta = step >> 3;
                if ((nibble & 1) != 0) delta += step >> 2;
                if ((nibble & 2) != 0) delta += step >> 1;
                if ((nibble & 4) != 0) delta += step;
                predictor[channel] = Math.Clamp(
                    predictor[channel] + ((nibble & 8) != 0 ? -delta : delta),
                    short.MinValue, short.MaxValue);
                index[channel] = Math.Clamp(index[channel] + IndexDelta[nibble & 7], 0, 88);
                samples[written[channel] * channels + channel] = (short)predictor[channel];
                written[channel]++;
            }
        }
    }
}
