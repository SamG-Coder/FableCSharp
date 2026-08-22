using System.Text;
using System.Buffers.Binary;

namespace Fable.Formats.Text;

/// <summary>
/// UTF-16 LE payload used by lang/English/text.big entries.
/// </summary>
public static class TextPayload
{
    public static string ReadUtf16(ReadOnlySpan<byte> payload)
    {
        if (payload.Length < 2)
            return "";
        var chars = payload.Length / 2;
        var text = Encoding.Unicode.GetString(payload[..(chars * 2)]);
        var nul = text.IndexOf('\0');
        return nul >= 0 ? text[..nul] : text;
    }

    /// <summary>
    /// Reads the complete retail text record. Spoken entries append three
    /// length-prefixed ASCII strings (audio bank, speaker and text key), then
    /// a counted list of authored tags such as <c>ANIM:...</c>. Group entries
    /// are a count followed by text-entry ids and contain no UTF-16 body.
    /// </summary>
    public static TextRecord ReadRecord(ReadOnlySpan<byte> payload)
    {
        if (payload.Length >= 4)
        {
            var count = BinaryPrimitives.ReadUInt32LittleEndian(payload);
            if (count <= 4096 && payload.Length == 4L + count * 4L)
            {
                var children = new uint[(int)count];
                for (var i = 0; i < children.Length; i++)
                    children[i] = BinaryPrimitives.ReadUInt32LittleEndian(
                        payload.Slice(4 + i * 4, 4));
                return new TextRecord("", "", "", "", [], children);
            }
        }

        var terminator = -1;
        for (var i = 0; i + 1 < payload.Length; i += 2)
        {
            if (payload[i] == 0 && payload[i + 1] == 0)
            {
                terminator = i;
                break;
            }
        }
        if (terminator < 0)
            return new TextRecord(ReadUtf16(payload), "", "", "", [], []);

        var body = Encoding.Unicode.GetString(payload[..terminator]);
        var offset = terminator + 2;
        var bank = ReadAscii(payload, ref offset);
        var speaker = ReadAscii(payload, ref offset);
        var key = ReadAscii(payload, ref offset);
        if (offset + 4 > payload.Length)
            return new TextRecord(body, bank, speaker, key, [], []);
        var tagCount = BinaryPrimitives.ReadUInt32LittleEndian(payload[offset..]);
        offset += 4;
        var tags = new List<string>((int)Math.Min(tagCount, 128));
        for (var i = 0u; i < tagCount && offset < payload.Length; i++)
            tags.Add(ReadAscii(payload, ref offset));
        return new TextRecord(body, bank, speaker, key, tags, []);
    }

    private static string ReadAscii(ReadOnlySpan<byte> payload, ref int offset)
    {
        if (offset + 4 > payload.Length)
        {
            offset = payload.Length;
            return "";
        }
        var length = BinaryPrimitives.ReadUInt32LittleEndian(payload[offset..]);
        offset += 4;
        if (length > int.MaxValue || offset + (long)length > payload.Length)
        {
            offset = payload.Length;
            return "";
        }
        var value = Encoding.ASCII.GetString(payload.Slice(offset, (int)length));
        offset += (int)length;
        return value;
    }
}

public sealed record TextRecord(
    string Body,
    string AudioBank,
    string Speaker,
    string TextKey,
    IReadOnlyList<string> Tags,
    IReadOnlyList<uint> Children)
{
    public bool IsGroup => Children.Count > 0;
    public string? Animation => Tags.FirstOrDefault(tag =>
        tag.StartsWith("ANIM:", StringComparison.OrdinalIgnoreCase))?.Substring(5);
}
