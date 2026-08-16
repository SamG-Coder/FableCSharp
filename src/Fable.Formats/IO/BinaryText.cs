using System.Text;

namespace Fable.Formats.IO;

internal static class BinaryText
{
    public static string ReadCString(BinaryReader reader)
    {
        var buffer = new StringBuilder();
        while (true)
        {
            var value = reader.ReadByte();
            if (value == 0)
                break;
            buffer.Append((char)value);
        }

        return buffer.ToString();
    }

    public static string ReadLengthPrefixed(BinaryReader reader)
    {
        var length = reader.ReadUInt32();
        if (length == 0)
            return string.Empty;
        if (length > 1_000_000)
            throw new InvalidDataException($"Implausible string length {length}.");

        var bytes = reader.ReadBytes((int)length);
        var end = bytes.Length;
        while (end > 0 && bytes[end - 1] == 0)
            end--;

        return Encoding.ASCII.GetString(bytes, 0, end);
    }
}
