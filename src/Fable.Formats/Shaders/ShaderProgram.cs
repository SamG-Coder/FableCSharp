namespace Fable.Formats.Shaders;

/// <summary>
/// One program from data\shaders\pc\shaders.big. Payload is a u32 byte
/// length then Direct3D 8/9 tokens (vs_1_1 / ps_1_1 / ps_1_4).
/// </summary>
public sealed class ShaderProgram
{
    public const uint VertexVersionTag = 0xFFFE;
    public const uint PixelVersionTag = 0xFFFF;
    public const uint TexOpcode = 0x42;
    public const uint MulOpcode = 0x05;

    public required string Name { get; init; }
    public required string Bank { get; init; }
    public required uint EntryType { get; init; }
    public required int DeclaredSize { get; init; }
    public required uint VersionToken { get; init; }
    public required byte[] Tokens { get; init; }

    public bool IsPixel => VersionTag == PixelVersionTag;
    public bool IsVertex => VersionTag == VertexVersionTag;
    public uint VersionTag => VersionToken >> 16;
    public int Major => (int)((VersionToken >> 8) & 0xFF);
    public int Minor => (int)(VersionToken & 0xFF);
    public string Profile => $"{(IsPixel ? "ps" : "vs")}_{Major}_{Minor}";

    public int TexCount
    {
        get
        {
            var n = 0;
            for (var i = 0; i + 4 <= Tokens.Length; i += 4)
            {
                if (BitConverter.ToUInt32(Tokens, i) == TexOpcode)
                    n++;
            }

            return n;
        }
    }

    /// <summary>
    /// ps_1.1 dest shift 1 is <c>_x2</c>: <c>sat(2 * src0 * src1)</c>.
    /// Landscape FG and the object fog shaders use this, not a lerp.
    /// </summary>
    public bool HasMulX2
    {
        get
        {
            for (var i = 0; i + 8 <= Tokens.Length; i += 4)
            {
                if (BitConverter.ToUInt32(Tokens, i) != MulOpcode)
                    continue;
                var dest = BitConverter.ToUInt32(Tokens, i + 4);
                if (((dest >> 24) & 0xF) == 1)
                    return true;
            }

            return false;
        }
    }

    /// <summary>
    /// D3D register numbers of type CONST (2) in dest/src tokens.
    /// vs_1_1 / ps_1_1 use the D3D8-style type field at bits 28–30.
    /// </summary>
    public IReadOnlyList<int> ConstRegisters
    {
        get
        {
            var set = new SortedSet<int>();
            for (var i = 4; i + 4 <= Tokens.Length; i += 4)
            {
                var token = BitConverter.ToUInt32(Tokens, i);
                if (token is 0x0000FFFF or 0x0000FFFE)
                    continue;
                var type = (int)((token >> 28) & 7);
                if (type != 2)
                    continue;
                var reg = (int)(token & 0x7FF);
                if (reg is >= 0 and < 96)
                    set.Add(reg);
            }

            return set.ToList();
        }
    }

    public static ShaderProgram Parse(string name, string bank, uint entryType, byte[] data)
    {
        if (data.Length < 8)
            throw new InvalidDataException($"Shader '{name}' is too small ({data.Length}).");

        var declared = BitConverter.ToInt32(data, 0);
        var version = BitConverter.ToUInt32(data, 4);
        var tag = version >> 16;
        if (tag is not (VertexVersionTag or PixelVersionTag))
            throw new InvalidDataException($"Shader '{name}' is not a D3D vs/ps token stream (0x{version:X8}).");

        return new ShaderProgram
        {
            Name = name,
            Bank = bank,
            EntryType = entryType,
            DeclaredSize = declared,
            VersionToken = version,
            Tokens = data[4..],
        };
    }
}
