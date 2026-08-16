using System.Text;
using Fable.Core;
using Fable.Formats.Defs;

namespace Fable.Game;

/// <summary>
/// Compiled <c>script.bin</c> command lists. Same GameBin
/// container as <c>game.bin</c>. <c>S_QNOVI</c> is not an
/// entry; <c>CCutsceneDef</c> instances are.
/// </summary>
public sealed class ScriptBank
{
    public IReadOnlyList<ScriptDef> Entries { get; }

    private readonly Dictionary<string, ScriptDef> _byName;

    private ScriptBank(IReadOnlyList<ScriptDef> entries)
    {
        Entries = entries;
        _byName = entries
            .GroupBy(entry => entry.InstanceName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
    }

    public static ScriptBank Load(GameInstall install)
    {
        var namesPath = install.FindCompiledDef("names.bin")
            ?? throw new FileNotFoundException("names.bin");
        var scriptPath = install.FindCompiledDef("script.bin")
            ?? throw new FileNotFoundException("script.bin");
        return Load(GameBin.Load(scriptPath, NamesBin.Load(namesPath)));
    }

    public static ScriptBank Load(GameBin bin)
    {
        var entries = new List<ScriptDef>(bin.Entries.Count);
        foreach (var entry in bin.Entries)
        {
            if (entry.InstanceName is null)
                continue;
            entries.Add(new ScriptDef(
                entry.Index,
                entry.TypeName ?? "",
                entry.InstanceName,
                ExtractCommands(entry.Raw)));
        }

        return new ScriptBank(entries);
    }

    public ScriptDef? Find(string instanceName) =>
        _byName.TryGetValue(instanceName, out var hit) ? hit : null;

    /// <summary>
    /// Printable runs of length ≥ 4. Matches the CCutsceneDef
    /// line list <c>00CBFB7D</c> walks from def+60.
    /// </summary>
    public static IReadOnlyList<string> ExtractCommands(byte[] raw)
    {
        var list = new List<string>();
        var i = 0;
        while (i < raw.Length)
        {
            if (raw[i] is < 32 or > 126)
            {
                i++;
                continue;
            }

            var start = i;
            while (i < raw.Length && raw[i] is >= 32 and <= 126)
                i++;
            if (i - start >= 4)
                list.Add(Encoding.ASCII.GetString(raw, start, i - start));
        }

        return list;
    }
}

public sealed class ScriptDef
{
    public int Index { get; }
    public string TypeName { get; }
    public string InstanceName { get; }
    public IReadOnlyList<string> Commands { get; }

    public ScriptDef(int index, string typeName, string instanceName, IReadOnlyList<string> commands)
    {
        Index = index;
        TypeName = typeName;
        InstanceName = instanceName;
        Commands = commands;
    }
}
