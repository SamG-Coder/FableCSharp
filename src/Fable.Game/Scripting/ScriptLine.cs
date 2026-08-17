using System.Globalization;

namespace Fable.Game.Scripting;

/// <summary>
/// One script.bin command line. Preserves raw text;
/// does not deduplicate. Quotes, case-insensitive
/// TRUE/FALSE/NULL/FOREVER, and $tokens stay intact
/// until <see cref="ScriptArguments.Substitute"/>.
/// </summary>
public readonly struct ScriptLine
{
    public string Raw { get; }
    public string Verb { get; }
    public string? Target { get; }
    public CommandFamily Family { get; }
    public IReadOnlyList<string> Args { get; }

    public ScriptLine(string raw, string verb, string? target, IReadOnlyList<string> args)
    {
        Raw = raw;
        Verb = verb;
        Target = target;
        Family = target is { Length: > 0 } ? CommandFamily.Entity : CommandFamily.Global;
        Args = args;
    }

    public static ScriptLine Parse(string raw)
    {
        var trimmed = raw.Trim();
        var space = IndexOfUnquoted(trimmed, ' ');
        var head = space < 0 ? trimmed : trimmed[..space];
        var rest = space < 0 ? "" : trimmed[(space + 1)..];
        var dot = head.LastIndexOf('.');
        string verb;
        string? target;
        if (dot > 0)
        {
            target = head[..dot];
            verb = head[(dot + 1)..];
        }
        else
        {
            target = null;
            verb = head;
        }

        return new ScriptLine(raw, verb, target, SplitArgs(rest));
    }

    public string Arg(int index) =>
        index >= 0 && index < Args.Count ? Args[index] : "";

    public static bool IsTrue(string? text) =>
        text is not null && text.Equals("true", StringComparison.OrdinalIgnoreCase);

    public static bool IsFalse(string? text) =>
        text is not null && text.Equals("false", StringComparison.OrdinalIgnoreCase);

    public static bool IsNull(string? text) =>
        text is not null && text.Equals("null", StringComparison.OrdinalIgnoreCase);

    public static bool IsForever(string? text) =>
        text is not null && text.Equals("forever", StringComparison.OrdinalIgnoreCase);

    public static bool TryFloat(string? text, out float value) =>
        float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);

    public static bool TryInt(string? text, out int value) =>
        int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

    public static string[] SplitArgs(string arguments)
    {
        if (arguments.Length == 0)
            return [];
        var list = new List<string>();
        var start = 0;
        var quote = '\0';
        for (var i = 0; i < arguments.Length; i++)
        {
            var ch = arguments[i];
            if (quote != '\0')
            {
                if (ch == quote)
                    quote = '\0';
                continue;
            }

            if (ch is '\'' or '"')
            {
                quote = ch;
                continue;
            }

            if (ch == ',')
            {
                list.Add(Unquote(arguments.AsSpan(start, i - start)));
                start = i + 1;
            }
        }

        list.Add(Unquote(arguments.AsSpan(start)));
        return list.ToArray();
    }

    public static string Unquote(ReadOnlySpan<char> span)
    {
        var text = span.Trim().ToString();
        if (text.Length >= 2 &&
            ((text[0] == '\'' && text[^1] == '\'') ||
             (text[0] == '"' && text[^1] == '"')))
            return text[1..^1];
        return text;
    }

    private static int IndexOfUnquoted(string text, char needle)
    {
        var quote = '\0';
        for (var i = 0; i < text.Length; i++)
        {
            var ch = text[i];
            if (quote != '\0')
            {
                if (ch == quote)
                    quote = '\0';
                continue;
            }

            if (ch is '\'' or '"')
            {
                quote = ch;
                continue;
            }

            if (ch == needle)
                return i;
        }

        return -1;
    }
}

public enum CommandFamily
{
    Global,
    Entity,
}
