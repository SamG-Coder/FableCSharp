using System.Numerics;

namespace Fable.Render;

public readonly record struct SceneMarker(Vector3 Position, string Classification);

public sealed class GizmoScene
{
    public required string Region { get; init; }
    public required IReadOnlyList<LineVertex> Lines { get; init; }
    public required Vector3 Centroid { get; init; }
    public required int ThingCount { get; init; }

    public static GizmoScene FromMarkers(string region, IEnumerable<SceneMarker> markers)
    {
        var placed = markers.ToList();

        var centroid = placed.Count == 0
            ? Vector3.Zero
            : placed.Aggregate(Vector3.Zero, (sum, item) => sum + item.Position) / placed.Count;

        var minZ = placed.Count == 0 ? 0f : placed.Min(item => item.Position.Z);
        var lines = new List<LineVertex>(placed.Count * 8 + 200);

        AddGrid(lines, centroid, minZ - 0.15f);
        foreach (var item in placed)
            AddGizmo(lines, item.Position, ColorFor(item.Classification));

        return new GizmoScene
        {
            Region = region,
            Lines = lines,
            Centroid = centroid,
            ThingCount = placed.Count,
        };
    }

    private static void AddGrid(List<LineVertex> lines, Vector3 center, float z)
    {
        const int half = 40;
        const float step = 2f;
        var color = new Vector3(0.18f, 0.22f, 0.20f);
        var axisX = new Vector3(0.35f, 0.12f, 0.12f);
        var axisY = new Vector3(0.12f, 0.35f, 0.12f);
        var origin = new Vector3(center.X, center.Y, z);
        var extent = half * step;

        for (var i = -half; i <= half; i++)
        {
            var t = i * step;
            var c = i == 0 ? axisX : color;
            AddLine(lines, origin + new Vector3(-extent, t, 0), origin + new Vector3(extent, t, 0), c);
            c = i == 0 ? axisY : color;
            AddLine(lines, origin + new Vector3(t, -extent, 0), origin + new Vector3(t, extent, 0), c);
        }
    }

    private static void AddGizmo(List<LineVertex> lines, Vector3 p, Vector3 body)
    {
        const float arm = 0.55f;
        AddLine(lines, p, p + new Vector3(arm, 0, 0), new Vector3(0.95f, 0.25f, 0.22f));
        AddLine(lines, p, p + new Vector3(0, arm, 0), new Vector3(0.30f, 0.85f, 0.30f));
        AddLine(lines, p, p + new Vector3(0, 0, arm), new Vector3(0.30f, 0.55f, 1.00f));
        AddLine(lines, p + new Vector3(0, 0, 0.04f), p + new Vector3(0, 0, 0.28f), body);
    }

    private static void AddLine(List<LineVertex> lines, Vector3 a, Vector3 b, Vector3 color)
    {
        lines.Add(new LineVertex(a, color));
        lines.Add(new LineVertex(b, color));
    }

    private static Vector3 ColorFor(string classification)
    {
        var kind = classification.ToUpperInvariant();
        if (kind.Contains("CREATURE") || kind.Contains("HERO"))
            return new Vector3(0.95f, 0.35f, 0.85f);
        if (kind.Contains("BUILDING"))
            return new Vector3(0.95f, 0.62f, 0.22f);
        if (kind.Contains("OBJECT") || kind.Contains("SILVER") || kind.Contains("CHEST"))
            return new Vector3(0.95f, 0.88f, 0.25f);
        if (kind.Contains("MARKER") || kind.Contains("CAMERA"))
            return new Vector3(0.35f, 0.85f, 0.95f);
        return new Vector3(0.85f, 0.85f, 0.85f);
    }
}
