namespace Fable.Formats.Upk;

/// <summary>
/// Unreal package header. Anniversary cooked files are version 860 / licensee 26985.
/// TLC does not use this format.
/// </summary>
public sealed class UpkHeader
{
    public const uint UnrealMagic = 0x9E2A83C1;

    public required uint Magic { get; init; }
    public required ushort Version { get; init; }
    public required ushort Licensee { get; init; }
    public required long FileSize { get; init; }
    public required string Path { get; init; }

    public bool IsUnrealPackage => Magic == UnrealMagic;

    public static UpkHeader Load(string path)
    {
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);
        if (stream.Length < 8)
            throw new InvalidDataException($"File too small to be a UPK: {path}");

        return new UpkHeader
        {
            Magic = reader.ReadUInt32(),
            Version = reader.ReadUInt16(),
            Licensee = reader.ReadUInt16(),
            FileSize = stream.Length,
            Path = path,
        };
    }
}
