using System.Text;

namespace Fable.Formats.Tests;

/// <summary>
/// Writes decode dumps under repo <c>export/</c>
/// (gitignored). Later 3D frames go here as PNG.
/// </summary>
internal static class ExportDir
{
    public static string Root
    {
        get
        {
            var dir = AppContext.BaseDirectory;
            while (!string.IsNullOrEmpty(dir))
            {
                if (File.Exists(Path.Combine(dir, "FableCSharp.slnx")))
                    return Path.Combine(dir, "export");
                dir = Path.GetDirectoryName(dir);
            }

            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "export"));
        }
    }

    public static string PathFor(params string[] parts)
    {
        var path = Path.Combine(new[] { Root }.Concat(parts).ToArray());
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        return path;
    }

    public static void WriteGrayBmp(string path, int width, int height, ReadOnlySpan<byte> gray)
    {
        var row = (width + 3) & ~3;
        var pixels = new byte[row * height];
        for (var y = 0; y < height; y++)
            gray.Slice(y * width, width).CopyTo(pixels.AsSpan((height - 1 - y) * row, width));
        using var stream = System.IO.File.Create(path);
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write((byte)'B');
        writer.Write((byte)'M');
        writer.Write(54 + 1024 + pixels.Length);
        writer.Write(0);
        writer.Write(54 + 1024);
        writer.Write(40);
        writer.Write(width);
        writer.Write(height);
        writer.Write((ushort)1);
        writer.Write((ushort)8);
        writer.Write(0);
        writer.Write(pixels.Length);
        writer.Write(0);
        writer.Write(0);
        writer.Write(256);
        writer.Write(256);
        for (var i = 0; i < 256; i++)
        {
            writer.Write((byte)i);
            writer.Write((byte)i);
            writer.Write((byte)i);
            writer.Write((byte)0);
        }

        writer.Write(pixels);
    }

    public static void WriteRgbaBmp(string path, int width, int height, ReadOnlySpan<byte> rgba)
    {
        var row = width * 4;
        var pixels = new byte[row * height];
        for (var y = 0; y < height; y++)
        {
            var srcY = height - 1 - y;
            for (var x = 0; x < width; x++)
            {
                var s = (srcY * width + x) * 4;
                var d = y * row + x * 4;
                pixels[d] = rgba[s + 2];
                pixels[d + 1] = rgba[s + 1];
                pixels[d + 2] = rgba[s];
                pixels[d + 3] = rgba[s + 3];
            }
        }

        using var stream = System.IO.File.Create(path);
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write((byte)'B');
        writer.Write((byte)'M');
        writer.Write(54 + pixels.Length);
        writer.Write(0);
        writer.Write(54);
        writer.Write(40);
        writer.Write(width);
        writer.Write(height);
        writer.Write((ushort)1);
        writer.Write((ushort)32);
        writer.Write(0);
        writer.Write(pixels.Length);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);
        writer.Write(pixels);
    }
}