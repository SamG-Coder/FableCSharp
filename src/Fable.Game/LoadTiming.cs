using System.Diagnostics;
using System.Text;

namespace Fable.Game;

/// <summary>
/// Compact New Game / submit clocks. Native
/// open is names + directories + headers;
/// draw later reads handles.
/// </summary>
public sealed class LoadTiming
{
    private readonly List<Row> _rows = [];

    public IReadOnlyList<Row> Rows => _rows;

    public double TotalMs => _rows.Sum(r => r.Ms);

    public T Measure<T>(string name, Func<T> action, Func<T, string>? extra = null)
    {
        var sw = Stopwatch.StartNew();
        var result = action();
        sw.Stop();
        _rows.Add(new Row(name, sw.Elapsed.TotalMilliseconds, extra?.Invoke(result) ?? ""));
        return result;
    }

    public void Add(string name, double ms, string extra = "") =>
        _rows.Add(new Row(name, ms, extra));

    public string Format()
    {
        var sb = new StringBuilder();
        foreach (var row in _rows)
        {
            sb.Append(row.Name.PadRight(16));
            sb.Append(row.Ms.ToString("0").PadLeft(8));
            sb.Append(" ms");
            if (row.Extra.Length > 0)
            {
                sb.Append("  ");
                sb.Append(row.Extra);
            }

            sb.AppendLine();
        }

        sb.Append("----------------    --------");
        sb.AppendLine();
        sb.Append("First World".PadRight(16));
        sb.Append(TotalMs.ToString("0").PadLeft(8));
        sb.Append(" ms");
        return sb.ToString();
    }

    public readonly record struct Row(string Name, double Ms, string Extra);
}