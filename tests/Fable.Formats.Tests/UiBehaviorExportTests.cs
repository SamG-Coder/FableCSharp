using System.Text;
using Fable.ExeIndex;
using Fable.Formats.Defs;

namespace Fable.Formats.Tests;

public sealed class UiBehaviorExportTests
{
    [Fact]
    public void Grep_atom_keeps_each_fact_on_one_line()
    {
        Assert.Equal(@"a\\b\tc\nd", UiBehaviorExport.Atom("a\\b\tc\nd"));
    }

    [Fact]
    public void Field_scanner_emits_named_scalar_vector_and_utf16_values()
    {
        using var bytes = new MemoryStream();
        using var writer = new BinaryWriter(bytes, Encoding.Unicode, leaveOpen: true);
        writer.Write(FrontendUiDef.TypeCrc);
        writer.Write(16);
        writer.Write(FrontendUiDef.ChildrenCrc);
        writer.Write(2);
        writer.Write(208);
        writer.Write(209);
        writer.Write(FrontendUiDef.TextValueCrc);
        writer.Write("WASD".ToCharArray());
        writer.Write((ushort)0);
        writer.Flush();

        var facts = UiBehaviorExport.ScanFieldValuesForTest(bytes.ToArray(), 0);

        Assert.Contains(("Type", "16"), facts);
        Assert.Contains(("Children", "count=2;items=[208,209]"), facts);
        Assert.Contains(("TextValue", "WASD"), facts);
    }
}
