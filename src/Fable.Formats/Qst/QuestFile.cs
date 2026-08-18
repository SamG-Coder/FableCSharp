using System.Text.RegularExpressions;

namespace Fable.Formats.Qst;

public sealed class QuestFile
{
    public required IReadOnlyList<QuestEntry> Quests { get; init; }

    public static QuestFile Load(string path) => Parse(File.ReadAllText(path));

    public static QuestFile Parse(string text)
    {
        var regex = new Regex(
            @"AddQuest\(\s*""(?<name>[^""]+)""\s*,\s*(?<persistent>TRUE|FALSE)\s*\)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        var quests = new List<QuestEntry>();
        foreach (Match match in regex.Matches(text))
        {
            quests.Add(new QuestEntry
            {
                Name = match.Groups["name"].Value,
                Persistent = match.Groups["persistent"].Value.Equals("TRUE", StringComparison.OrdinalIgnoreCase),
            });
        }

        return new QuestFile { Quests = quests };
    }

    /// <summary>
    /// <c>004A0D90</c> flag 0 append:
    /// concatenate <paramref name="other"/>
    /// after this file's <c>AddQuest</c>
    /// rows. Does not clear.
    /// </summary>
    public QuestFile Append(QuestFile other)
    {
        ArgumentNullException.ThrowIfNull(other);
        var quests = new List<QuestEntry>(Quests.Count + other.Quests.Count);
        quests.AddRange(Quests);
        quests.AddRange(other.Quests);
        return new QuestFile { Quests = quests };
    }

    public IEnumerable<string> PersistentNames()
    {
        foreach (var quest in Quests)
        {
            if (quest.Persistent)
                yield return quest.Name;
        }
    }
}

public sealed class QuestEntry
{
    public required string Name { get; init; }
    public required bool Persistent { get; init; }
}
