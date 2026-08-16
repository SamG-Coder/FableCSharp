using System.Numerics;

namespace Fable.Formats.Sky;

/// <summary>
/// data\Misc\stars.dat — u32 count, then 24-byte records: xyz, unused 0,
/// size, brightness. Positions sit on a ~6500-unit sphere.
/// </summary>
public sealed class StarField
{
    public required IReadOnlyList<Star> Stars { get; init; }

    public static StarField Load(string path) => Parse(File.ReadAllBytes(path));

    public static StarField Parse(byte[] data)
    {
        if (data.Length < 4)
            throw new InvalidDataException("stars.dat too small.");
        var count = BitConverter.ToInt32(data, 0);
        if (count < 0 || 4 + count * 24 > data.Length)
            throw new InvalidDataException($"stars.dat count {count} overruns file.");

        var stars = new Star[count];
        for (var i = 0; i < count; i++)
        {
            var o = 4 + i * 24;
            stars[i] = new Star(
                new Vector3(
                    BitConverter.ToSingle(data, o),
                    BitConverter.ToSingle(data, o + 4),
                    BitConverter.ToSingle(data, o + 8)),
                BitConverter.ToSingle(data, o + 16),
                BitConverter.ToSingle(data, o + 20));
        }

        return new StarField { Stars = stars };
    }
}

public readonly record struct Star(Vector3 Position, float Size, float Brightness);
