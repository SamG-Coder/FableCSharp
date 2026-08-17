using System.Text;

namespace Fable.Game.Scripting;

/// <summary>
/// CCutsceneDef invocation environment. Substitution
/// runs before handlers so <c>$ARG1</c> / <c>$ANIM</c>
/// never reach world apply.
/// </summary>
public sealed class ScriptArguments
{
    public static readonly string[] NamedSlots =
        ["ARG1", "ARG2", "ARG3", "ARG4", "ANIM", "LOOP", "LINE", "CAMERA"];

    private readonly Dictionary<string, string> _values =
        new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, string> Values => _values;

    public void Set(string name, string value) =>
        _values[Normalize(name)] = value;

    public bool TryGet(string name, out string value) =>
        _values.TryGetValue(Normalize(name), out value!);

    public ScriptLine Substitute(ScriptLine line, out string? unresolved)
    {
        unresolved = null;
        var target = SubstituteToken(line.Target, ref unresolved);
        var args = new string[line.Args.Count];
        for (var i = 0; i < line.Args.Count; i++)
            args[i] = SubstituteToken(line.Args[i], ref unresolved) ?? "";
        return new ScriptLine(line.Raw, line.Verb, target, args);
    }

    public string? SubstituteToken(string? text, ref string? unresolved)
    {
        if (string.IsNullOrEmpty(text) || text.IndexOf('$') < 0)
            return text;
        var sb = new StringBuilder(text.Length);
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] != '$')
            {
                sb.Append(text[i]);
                continue;
            }

            var start = i + 1;
            var end = start;
            while (end < text.Length && (char.IsAsciiLetterOrDigit(text[end]) || text[end] == '_'))
                end++;
            var name = text[start..end];
            if (name.Length == 0)
            {
                sb.Append('$');
                continue;
            }

            if (_values.TryGetValue(name, out var value))
                sb.Append(value);
            else
                unresolved ??= name;
            i = end - 1;
        }

        return sb.ToString();
    }

    private static string Normalize(string name) =>
        name.StartsWith('$') ? name[1..] : name;
}
