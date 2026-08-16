using System.Text;

namespace Fable.Formats.Defs;

/// <summary>
/// Lionhead CRC used by names.bin and game.bin field ids.
/// Same polynomial as EgoCore / fable-defs (0xEDB88320, init 0).
/// </summary>
public static class FableCrc
{
    private static readonly uint[] Table = BuildTable();

    public static uint Hash(string text) => Hash(Encoding.ASCII.GetBytes(text));

    public static uint Hash(ReadOnlySpan<byte> data)
    {
        uint crc = 0;
        foreach (var value in data)
            crc = Table[(crc ^ value) & 0xFF] ^ (crc >> 8);
        return crc;
    }

    private static uint[] BuildTable()
    {
        var table = new uint[256];
        for (uint i = 0; i < 256; i++)
        {
            var c = i;
            for (var j = 0; j < 8; j++)
                c = (c & 1) != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1;
            table[i] = c;
        }

        return table;
    }
}
