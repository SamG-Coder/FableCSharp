using System.Text;
using Fable.Core;
using Fable.Formats.Defs;

namespace Fable.Game;

/// <summary>
/// Compiled <c>script.bin</c>. <c>CCutsceneDef</c> persist
/// <c>00F2A1D0</c> reads eight CString vectors via
/// <c>004331F9</c> / <c>00433273</c>. First vector is
/// runtime <c>+60</c>, the list <c>00CBFB7D</c> copies
/// with <c>00432EE9</c>.
/// </summary>
public sealed class ScriptBank
{
    public const int CutscenePreambleBytes = 5;
    public const int CutsceneVectorCount = 8;
    public const int CommandVectorIndex = 0;
    public const int CommandRuntimeOffset = 60;
    public const uint PersistFn = 0x00F2A1D0;
    public const uint VectorPersist = 0x004331F9;
    public const uint VectorRead = 0x00433273;
    public const uint VectorCopy = 0x00432EE9;
    public const uint CtorFn = 0x00F29D00;
    public const uint CtorVtbl = 0x012FB6E0;
    public const int RuntimeBytes = 0x9C;
    public const string CutsceneType = "CCutsceneDef";

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
            entries.Add(FromEntry(entry));
        }

        return new ScriptBank(entries);
    }

    public ScriptDef? Find(string instanceName) =>
        _byName.TryGetValue(instanceName, out var hit) ? hit : null;

    public static ScriptDef FromEntry(GameBinEntry entry)
    {
        var type = entry.TypeName ?? "";
        IReadOnlyList<IReadOnlyList<string>> vectors = [];
        var proven = false;
        if (type.Equals(CutsceneType, StringComparison.Ordinal) &&
            TryReadCutsceneVectors(entry.Raw, out var parsed))
        {
            vectors = parsed;
            proven = true;
        }

        return new ScriptDef(
            entry.Index,
            type,
            entry.InstanceName ?? "",
            proven && vectors.Count > CommandVectorIndex ? vectors[CommandVectorIndex] : [],
            vectors,
            proven,
            entry.Raw);
    }

    /// <summary>
    /// <c>00F2A1D0</c>: eight <c>004331F9</c> reads after the
    /// 5-byte GameBin prefix. Each vector is a skipped u32
    /// (<c>00404500</c> mode 2) then count then NUL strings
    /// (<c>00433273</c> / stream vtbl+24).
    /// </summary>
    public static bool TryReadCutsceneVectors(
        byte[] raw, out IReadOnlyList<IReadOnlyList<string>> vectors)
    {
        vectors = [];
        if (raw.Length < CutscenePreambleBytes + 8)
            return false;
        var cursor = CutscenePreambleBytes;
        var list = new List<IReadOnlyList<string>>(CutsceneVectorCount);
        for (var n = 0; n < CutsceneVectorCount; n++)
        {
            if (!TryReadStringVector(raw, ref cursor, out var lines))
                return false;
            list.Add(lines);
        }

        vectors = list;
        return true;
    }

    private static bool TryReadStringVector(byte[] raw, ref int cursor, out List<string> lines)
    {
        lines = [];
        if (cursor + 8 > raw.Length)
            return false;
        cursor += 4;
        var count = BitConverter.ToInt32(raw, cursor);
        cursor += 4;
        if (count is < 0 or > 10_000)
            return false;
        for (var i = 0; i < count; i++)
        {
            if (cursor > raw.Length)
                return false;
            var start = cursor;
            while (cursor < raw.Length && raw[cursor] != 0)
                cursor++;
            lines.Add(Encoding.ASCII.GetString(raw, start, cursor - start));
            if (cursor < raw.Length)
                cursor++;
        }

        return true;
    }

    /// <summary>
    /// Discovery-only printable scrape. Do not execute these
    /// as commands; use <see cref="ScriptDef.Commands"/>.
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
    public IReadOnlyList<IReadOnlyList<string>> Vectors { get; }
    public bool CommandsLayoutProven { get; }
    public byte[] Raw { get; }

    public ScriptDef(
        int index,
        string typeName,
        string instanceName,
        IReadOnlyList<string> commands,
        IReadOnlyList<IReadOnlyList<string>> vectors,
        bool commandsLayoutProven,
        byte[] raw)
    {
        Index = index;
        TypeName = typeName;
        InstanceName = instanceName;
        Commands = commands;
        Vectors = vectors;
        CommandsLayoutProven = commandsLayoutProven;
        Raw = raw;
    }
}
