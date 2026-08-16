using Fable.Formats.Defs;

namespace Fable.Formats.Levels;

/// <summary>
/// Maps WAD .lev GROUND_/PATH_ material names onto textures.h LANDSCAPE_* ids.
/// The u32 sitting at the end of a material slot is not a textures.big id.
/// </summary>
public static class LandscapeTextures
{
    public const int DefaultId = 414;
    public const int WaterId = 442;

    /// <summary>
    /// <c>00B6D6E0</c> compares the first bank u32 to 8 and returns 0
    /// otherwise. Type 8 copies the next two u32s to <c>[this]</c>/
    /// <c>[this+4]</c> and advances the stream by 8 — no further payload
    /// in that function. StartOakVale sea is 7363; there is no
    /// water-prefix STB bank.
    /// </summary>
    public const uint RequiredWaterBankType = 8;
    public const int WaterType8CopiedDwords = 2;

    /// <summary>
    /// StartOakVale <c>__ENGINE_SEA_*</c> first u32. Not compared
    /// anywhere in the water renderer (the only <c>0x1CC3</c> imm is
    /// in a KnotholeGlade/Arena string fn). <c>00B6D6E0</c> accepts
    /// type 8 only.
    /// </summary>
    public const uint SeaBankFirstU32 = 7363;

    /// <summary>
    /// Water ctor <c>00B73760</c> zeros <c>+508</c>..<c>+624</c>
    /// (ebx=0). Draw <c>00B783F0</c> treats begin==end on those
    /// vectors plus flags <c>+630</c>/<c>+645</c> as empty and
    /// <c>je 00B7A865</c>. <c>00B6D4D0</c> stores the sea name only.
    /// Missing water intern returns at <c>00B420E4</c>. First-seen
    /// never pushes a type-8 record, so the draw is empty.
    /// </summary>
    public const int WaterDrawVectorFirst = 508;
    public const int WaterDrawVectorLast = 624;
    public const bool FirstSeenWaterDrawIsEmpty = true;

    public static bool IsLoadableWaterBank(ReadOnlySpan<byte> bank) =>
        bank.Length >= 4 && BitConverter.ToUInt32(bank) == RequiredWaterBankType;

    /// <summary>
    /// Exe table <c>0x0139C5D8</c> uploaded via <c>00989A60</c> as VS
    /// float4s: <c>0.125</c> / <c>-0.125</c>. Tile verts have no UV;
    /// <c>VSHADER_LANDSCAPE_FOREGROUND</c> does <c>mad oT0</c> from world XY.
    /// Cell lookup still uses <c>&gt;&gt;4</c> (16 m). UV scale is 1/8, not 1/16.
    /// </summary>
    public const float UvScale = 0.125f;

    public static bool IsUsable(string materialName) =>
        materialName.Length > 0 &&
        !materialName.StartsWith("INVALID", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Exe <c>OpenStaticMaps</c> <c>00B4282B</c> hands <c>SEA_*</c> to
    /// <c>CEngineWaterRenderer</c> via <c>__ENGINE_SEA_STATIC_MAP_BANK_FILE__</c>.
    /// <c>LoadWaterData</c> <c>00B41FA0</c> does the same for
    /// <c>__ENGINE_WATER_STATIC_MAP_BANK_FILE__</c>. Those are not landscape FG.
    /// </summary>
    public static bool IsWaterOrSeaPass(string materialName) =>
        materialName.StartsWith("WATER_", StringComparison.OrdinalIgnoreCase) ||
        materialName.StartsWith("SEA_", StringComparison.OrdinalIgnoreCase) ||
        materialName.Contains("LAKE", StringComparison.OrdinalIgnoreCase);

    public static int? TryResolve(string materialName, HeaderEnums? textures)
    {
        if (!IsUsable(materialName))
            return null;
        // LoadWaterData 00B41FA0 rejects unless the bank u32 is 8.
        // StartOakVale sea is 7363. Draw 00B783F0 returns when the
        // renderer vectors are empty. Not landscape FG.
        if (IsWaterOrSeaPass(materialName))
            return null;
        return textures is null ? DefaultId : Resolve(materialName, textures);
    }

    public static int WaterTexture(HeaderEnums? textures) =>
        textures is not null && textures.ByName.TryGetValue("LANDSCAPE_WATER", out var id)
            ? id
            : WaterId;

    public static int Resolve(string materialName, HeaderEnums textures)
    {
        foreach (var key in Candidates(materialName))
        {
            if (textures.ByName.TryGetValue(key, out var id) && !key.Contains("PROC_", StringComparison.Ordinal))
                return id;
        }

        var tokens = Tokens(materialName);
        var best = DefaultId;
        var bestScore = 0;
        foreach (var (name, id) in textures.ByName)
        {
            if (!name.StartsWith("LANDSCAPE_", StringComparison.Ordinal) ||
                name.Contains("PROC_", StringComparison.Ordinal) ||
                name.Contains("DIST_", StringComparison.Ordinal))
                continue;
            var score = 0;
            foreach (var token in Tokens(name))
            {
                if (tokens.Contains(token))
                    score += 2;
                else if (tokens.Any(t => name.Contains(t, StringComparison.Ordinal)))
                    score += 1;
            }
            if (score > bestScore)
            {
                bestScore = score;
                best = id;
            }
        }

        return bestScore > 0 ? best : DefaultId;
    }

    public static IEnumerable<string> Candidates(string materialName)
    {
        var rest = materialName;
        if (rest.StartsWith("GROUND_", StringComparison.Ordinal))
            rest = rest["GROUND_".Length..];
        if (rest.EndsWith("_ET", StringComparison.Ordinal))
            rest = rest[..^3];

        yield return "LANDSCAPE_" + materialName;
        yield return "LANDSCAPE_" + rest;
        yield return "LANDSCAPE_" + rest + "_01";
        yield return "LANDSCAPE_" + rest + "_PLAIN";
        if (rest.Contains("COBBLE", StringComparison.Ordinal))
            yield return "LANDSCAPE_COBBLES_IRREGULAR_01";
        if (rest.Contains("FOREST", StringComparison.Ordinal) || rest.Contains("LEAF", StringComparison.Ordinal))
            yield return "LANDSCAPE_FORESTFLOOR";
        if (rest == "GRASS")
            yield return "LANDSCAPE_GRASS_PLAIN";
        if (rest.Contains("POPPY", StringComparison.Ordinal))
            yield return "LANDSCAPE_PROC_POPPY";
        if (rest.Contains("DANDELION", StringComparison.Ordinal))
            yield return "LANDSCAPE_PROC_DANDELIONS";
        if (materialName.StartsWith("WATER_", StringComparison.Ordinal) ||
            rest.Contains("WATER", StringComparison.Ordinal) ||
            rest.Contains("LAKE", StringComparison.Ordinal))
            yield return "LANDSCAPE_WATER";
    }

    private static HashSet<string> Tokens(string name)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var part in name.Split('_', StringSplitOptions.RemoveEmptyEntries))
        {
            if (part is "GROUND" or "LANDSCAPE" or "PROC" or "DIST" or "01" or "02" or "ET" or "THE")
                continue;
            set.Add(part);
        }
        return set;
    }
}
