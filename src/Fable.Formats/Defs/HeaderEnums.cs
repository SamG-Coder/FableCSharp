using System.Text.RegularExpressions;

namespace Fable.Formats.Defs;

public sealed class HeaderEnums
{
    public IReadOnlyDictionary<string, int> ByName { get; }
    public IReadOnlyDictionary<int, string> ById { get; }

    public HeaderEnums(IReadOnlyDictionary<string, int> byName, IReadOnlyDictionary<int, string> byId)
    {
        ByName = byName;
        ById = byId;
    }

    public static HeaderEnums Load(string path) => Parse(File.ReadAllText(path));

    public static HeaderEnums Parse(string text)
    {
        var byName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var byId = new Dictionary<int, string>();
        var regex = new Regex(@"^\s*([A-Z0-9_\[\]]+)\s*=\s*(-?\d+)\s*,?", RegexOptions.Multiline);
        foreach (Match match in regex.Matches(text))
        {
            var name = match.Groups[1].Value;
            var id = int.Parse(match.Groups[2].Value);
            byName[name] = id;
            byId.TryAdd(id, name);
        }

        return new HeaderEnums(byName, byId);
    }

    public int? FindMeshId(string definitionType)
    {
        foreach (var key in MeshNameCandidates(definitionType))
        {
            if (ByName.TryGetValue(key, out var id) &&
                !key.Contains("[PHYSICS]", StringComparison.OrdinalIgnoreCase))
                return id;
        }

        return null;
    }

    public static IEnumerable<string> MeshNameCandidates(string definitionType)
    {
        yield return definitionType;
        yield return "MESH_" + definitionType;
        foreach (var prefix in new[] { "OBJECT_", "CREATURE_", "BUILDING_" })
        {
            if (!definitionType.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                continue;
            var stem = definitionType[prefix.Length..];
            if (stem.Length == 0)
                continue;
            yield return stem;
            yield return "MESH_" + stem;
        }
    }
}
