using System.Text.RegularExpressions;

namespace Fable.Formats.Qst;

public sealed class QuestFile
{
    public required IReadOnlyList<QuestEntry> Quests { get; init; }
    public required IReadOnlyList<TestQuestEntry> TestQuests { get; init; }

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

        var testRegex = new Regex(
            @"AddTestQuest\(\s*""(?<name>[^""]*)""\s*,\s*""(?<start>[^""]*)""\s*,\s*(?<kind>-?\d+)\s*,\s*""(?<description>[^""]*)""\s*,\s*""(?<ini>[^""]*)""\s*,\s*""(?<end>[^""]*)""\s*,\s*""(?<card>[^""]*)""\s*\)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        var testQuests = new List<TestQuestEntry>();
        foreach (Match match in testRegex.Matches(text))
        {
            testQuests.Add(new TestQuestEntry
            {
                Name = match.Groups["name"].Value,
                StartHolySite = match.Groups["start"].Value,
                Kind = int.Parse(match.Groups["kind"].Value),
                Description = match.Groups["description"].Value,
                IniFile = match.Groups["ini"].Value,
                EndScript = match.Groups["end"].Value,
                QuestCard = match.Groups["card"].Value,
            });
        }

        return new QuestFile { Quests = quests, TestQuests = testQuests };
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
        var testQuests = new List<TestQuestEntry>(TestQuests.Count + other.TestQuests.Count);
        testQuests.AddRange(TestQuests);
        testQuests.AddRange(other.TestQuests);
        return new QuestFile { Quests = quests, TestQuests = testQuests };
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

public sealed class TestQuestEntry
{
    public required string Name { get; init; }
    public required string StartHolySite { get; init; }
    public required int Kind { get; init; }
    public required string Description { get; init; }
    public required string IniFile { get; init; }
    public required string EndScript { get; init; }
    public required string QuestCard { get; init; }
}

public sealed class QuestEntry
{
    public required string Name { get; init; }
    public required bool Persistent { get; init; }
}
