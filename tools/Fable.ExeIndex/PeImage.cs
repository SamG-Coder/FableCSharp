namespace Fable.ExeIndex;

/// <summary>Minimal PE32 reader for Fable.exe (I386, 32-bit).</summary>
internal sealed class PeImage
{
    public required byte[] Data { get; init; }
    public required uint ImageBase { get; init; }
    public required uint TimeDateStamp { get; init; }
    public required uint SizeOfImage { get; init; }
    public required IReadOnlyList<PeSection> Sections { get; init; }
    public required IReadOnlyList<string> Imports { get; init; }

    /// <summary>Identity for dump-cache: stamp + image size + file length.</summary>
    public string Identity => $"{TimeDateStamp:X8}-{SizeOfImage:X8}-{Data.Length}";

    public static PeImage Load(string path)
    {
        var data = File.ReadAllBytes(path);
        if (data.Length < 64 || data[0] != (byte)'M' || data[1] != (byte)'Z')
            throw new InvalidDataException("Not a PE.");
        var pe = BitConverter.ToInt32(data, 0x3C);
        if (pe + 24 > data.Length || BitConverter.ToUInt32(data, pe) != 0x00004550)
            throw new InvalidDataException("Missing PE signature.");
        var coff = pe + 4;
        var machine = BitConverter.ToUInt16(data, coff);
        var sectionCount = BitConverter.ToUInt16(data, coff + 2);
        var optSize = BitConverter.ToUInt16(data, coff + 16);
        var opt = coff + 20;
        if (BitConverter.ToUInt16(data, opt) != 0x10B)
            throw new InvalidDataException($"Need PE32 (got 0x{BitConverter.ToUInt16(data, opt):X}).");
        var imageBase = BitConverter.ToUInt32(data, opt + 28);
        var timeDateStamp = BitConverter.ToUInt32(data, coff + 4);
        var sizeOfImage = BitConverter.ToUInt32(data, opt + 56);
        var importRva = BitConverter.ToUInt32(data, opt + 104);
        var sectionOff = opt + optSize;
        var sections = new List<PeSection>(sectionCount);
        for (var i = 0; i < sectionCount; i++)
        {
            var o = sectionOff + i * 40;
            var name = System.Text.Encoding.ASCII.GetString(data, o, 8).TrimEnd('\0');
            sections.Add(new PeSection(
                name,
                BitConverter.ToUInt32(data, o + 12),
                BitConverter.ToUInt32(data, o + 8),
                BitConverter.ToUInt32(data, o + 20),
                BitConverter.ToUInt32(data, o + 16),
                BitConverter.ToUInt32(data, o + 36)));
        }

        return new PeImage
        {
            Data = data,
            ImageBase = imageBase,
            TimeDateStamp = timeDateStamp,
            SizeOfImage = sizeOfImage,
            Sections = sections,
            Imports = ReadImports(data, sections, importRva),
        };
    }

    public uint Va(int fileOffset)
    {
        foreach (var s in Sections)
        {
            if (fileOffset >= s.FileOffset && fileOffset < s.FileOffset + s.FileSize)
                return ImageBase + s.Rva + (uint)(fileOffset - s.FileOffset);
        }

        return ImageBase + (uint)fileOffset;
    }

    public int FileOffset(uint va)
    {
        var rva = va - ImageBase;
        foreach (var s in Sections)
        {
            if (rva >= s.Rva && rva < s.Rva + Math.Max(s.FileSize, s.VirtualSize))
                return (int)(s.FileOffset + (rva - s.Rva));
        }

        return -1;
    }

    public bool InCode(int fileOffset)
    {
        foreach (var s in Sections)
        {
            if (fileOffset >= s.FileOffset && fileOffset < s.FileOffset + s.FileSize)
                return (s.Characteristics & 0x20000000) != 0 || s.Name is ".text" or "CODE";
        }

        return false;
    }

    private static List<string> ReadImports(byte[] data, List<PeSection> sections, uint importRva)
    {
        var names = new List<string>();
        if (importRva == 0)
            return names;
        var off = RvaToFile(sections, importRva);
        if (off < 0)
            return names;
        for (var i = 0; i < 512; i++)
        {
            var nameRva = BitConverter.ToUInt32(data, off + i * 20 + 12);
            if (nameRva == 0)
                break;
            var nameOff = RvaToFile(sections, nameRva);
            if (nameOff < 0)
                continue;
            names.Add(ReadCString(data, nameOff));
        }

        return names;
    }

    private static int RvaToFile(List<PeSection> sections, uint rva)
    {
        foreach (var s in sections)
        {
            if (rva >= s.Rva && rva < s.Rva + Math.Max(s.FileSize, 1u))
                return (int)(s.FileOffset + (rva - s.Rva));
        }

        return -1;
    }

    private static string ReadCString(byte[] data, int off)
    {
        var end = off;
        while (end < data.Length && data[end] != 0)
            end++;
        return System.Text.Encoding.ASCII.GetString(data, off, end - off);
    }

    public List<uint> FindBytes(ReadOnlySpan<byte> needle, int max = 32)
    {
        var hits = new List<uint>();
        var n = needle.Length;
        if (n == 0)
            return hits;
        var data = Data;
        var end = data.Length - n;
        for (var i = 0; i <= end && hits.Count < max; i++)
        {
            var ok = true;
            for (var j = 0; j < n; j++)
            {
                if (data[i + j] != needle[j])
                {
                    ok = false;
                    break;
                }
            }

            if (!ok)
                continue;
            hits.Add(Va(i));
        }

        return hits;
    }

    public IReadOnlyList<(string Name, uint Va)> Exports()
    {
        var list = new List<(string, uint)>();
        if (Data.Length < 64)
            return list;
        var pe = BitConverter.ToInt32(Data, 0x3C);
        var coff = pe + 4;
        var opt = coff + 20;
        if (BitConverter.ToUInt16(Data, opt) != 0x10B)
            return list;
        var exportRva = BitConverter.ToUInt32(Data, opt + 96);
        if (exportRva == 0)
            return list;
        var dir = FileOffset(ImageBase + exportRva);
        if (dir < 0 || dir + 40 > Data.Length)
            return list;
        var count = BitConverter.ToInt32(Data, dir + 24);
        var namesRva = BitConverter.ToUInt32(Data, dir + 32);
        var ordsRva = BitConverter.ToUInt32(Data, dir + 36);
        var fnsRva = BitConverter.ToUInt32(Data, dir + 28);
        var names = FileOffset(ImageBase + namesRva);
        var ords = FileOffset(ImageBase + ordsRva);
        var fns = FileOffset(ImageBase + fnsRva);
        if (names < 0 || ords < 0 || fns < 0)
            return list;
        for (var i = 0; i < count && i < 256; i++)
        {
            var nameRva = BitConverter.ToUInt32(Data, names + i * 4);
            var nameOff = FileOffset(ImageBase + nameRva);
            if (nameOff < 0)
                continue;
            var ord = BitConverter.ToUInt16(Data, ords + i * 2);
            var fnRva = BitConverter.ToUInt32(Data, fns + ord * 4);
            list.Add((ReadCString(Data, nameOff), ImageBase + fnRva));
        }

        return list;
    }
}

internal readonly record struct PeSection(
    string Name,
    uint Rva,
    uint VirtualSize,
    uint FileOffset,
    uint FileSize,
    uint Characteristics);
