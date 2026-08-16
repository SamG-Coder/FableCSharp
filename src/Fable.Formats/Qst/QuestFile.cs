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
}

public sealed class QuestEntry
{
    public required string Name { get; init; }
    public required bool Persistent { get; init; }
}
