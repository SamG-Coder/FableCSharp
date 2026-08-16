using Fable.Formats.Defs;

namespace Fable.Formats.Sky;

/// <summary>
/// game.bin <c>SKY_DEF</c> / <c>CSkyDef</c>. CEngineSkyRenderer reads this.
/// Field ids are FableCrc names: SunTexture, StarTexture, then flare
/// records of Texture + Radius + Position + Colour.
/// </summary>
public sealed class SkyDef
{
    public const int MiddaySkyTextureId = 391;
    public const int SunTextureIdDefault = 384;
    public const int StarTextureIdDefault = 401;

    public required int SunTextureId { get; init; }
    public required int StarTextureId { get; init; }
    public required IReadOnlyList<SkyFlare> Flares { get; init; }
    public float MaxRadius => Flares.Count == 0 ? 2000f : Flares.Max(f => f.Radius);

    public static SkyDef Parse(byte[] raw)
    {
        var sun = (int)(FindU32(raw, "SunTexture") ?? (uint)SunTextureIdDefault);
        var star = (int)(FindU32(raw, "StarTexture") ?? (uint)StarTextureIdDefault);
        var flares = new List<SkyFlare>();
        var texCrc = FableCrc.Hash("Texture");
        var radCrc = FableCrc.Hash("Radius");
        var posCrc = FableCrc.Hash("Position");
        var colCrc = FableCrc.Hash("Colour");
        for (var i = 0; i + 8 <= raw.Length; i++)
        {
            if (BitConverter.ToUInt32(raw, i) != texCrc)
                continue;
            var tex = (int)BitConverter.ToUInt32(raw, i + 4);
            if (tex is < 390 or > 410)
                continue;
            var radius = FindNearbyF32(raw, i, radCrc, 24) ?? 0;
            var pos = FindNearbyF32(raw, i, posCrc, 24) ?? 0;
            var colour = FindNearbyU32(raw, i, colCrc, 24) ?? 0;
            if (radius > 0)
                flares.Add(new SkyFlare(tex, radius, pos, colour));
        }

        return new SkyDef
        {
            SunTextureId = sun,
            StarTextureId = star,
            Flares = flares,
        };
    }

    public static SkyDef? TryLoadFromGameBin(GameBin bin)
    {
        var entry = bin.Entries.FirstOrDefault(e =>
            e.InstanceName == "SKY_DEF" && e.Raw.Length > 32);
        return entry is null ? null : Parse(entry.Raw);
    }

    private static uint? FindU32(byte[] raw, string name)
    {
        var crc = FableCrc.Hash(name);
        for (var i = 0; i + 8 <= raw.Length; i++)
        {
            if (BitConverter.ToUInt32(raw, i) == crc)
                return BitConverter.ToUInt32(raw, i + 4);
        }

        return null;
    }

    private static float? FindNearbyF32(byte[] raw, int around, uint crc, int window)
    {
        var lo = Math.Max(0, around - window);
        var hi = Math.Min(raw.Length - 8, around + window);
        for (var i = lo; i <= hi; i++)
        {
            if (BitConverter.ToUInt32(raw, i) == crc)
                return BitConverter.ToSingle(raw, i + 4);
        }

        return null;
    }

    private static uint? FindNearbyU32(byte[] raw, int around, uint crc, int window)
    {
        var lo = Math.Max(0, around - window);
        var hi = Math.Min(raw.Length - 8, around + window);
        for (var i = lo; i <= hi; i++)
        {
            if (BitConverter.ToUInt32(raw, i) == crc)
                return BitConverter.ToUInt32(raw, i + 4);
        }

        return null;
    }
}

public readonly record struct SkyFlare(int TextureId, float Radius, float Position, uint Colour);
