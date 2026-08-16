using System.Text;

namespace Fable.Formats.Text;

/// <summary>
/// UTF-16 LE payload used by lang/English/text.big entries.
/// </summary>
public static class TextPayload
{
    public static string ReadUtf16(ReadOnlySpan<byte> payload)
    {
        if (payload.Length < 2)
            return "";
        var chars = payload.Length / 2;
        var text = Encoding.Unicode.GetString(payload[..(chars * 2)]);
        var nul = text.IndexOf('\0');
        return nul >= 0 ? text[..nul] : text;
    }
}
