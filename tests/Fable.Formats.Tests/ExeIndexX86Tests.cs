using Fable.Core;
using Fable.ExeIndex;

namespace Fable.Formats.Tests;

public sealed class ExeIndexX86Tests
{
    [Fact]
    public void Frame_prologue_is_push_ebp()
    {
        Assert.True(X86.IsFramePrologue([0x55, 0x8B, 0xEC], 0));
        Assert.True(X86.IsFramePrologue([0x55, 0x8D, 0x6C, 0x24, 0x80], 0));
        Assert.False(X86.IsFramePrologue([0x56, 0x8B, 0xF1], 0));
    }

    [Fact]
    public void Thiscall_is_a_start_only_after_int3()
    {
        Assert.True(X86.IsThiscallPrologue([0x56, 0x8B, 0xF1], 0));
        Assert.True(X86.IsThiscallPrologue([0x53, 0x56, 0x8B, 0xF1], 0));
        Assert.True(X86.IsThiscallPrologue([0x53, 0x8B, 0xD9], 0));
        Assert.False(X86.IsFunctionStart([0x56, 0x8B, 0xF1], 0));
        Assert.True(X86.IsFunctionStart([0xCC, 0x56, 0x8B, 0xF1], 1));
        Assert.False(X86.IsFunctionStart([0x90, 0x56, 0x8B, 0xF1], 1));
    }

    [Fact]
    public void Transfer_00430900_is_a_function_start()
    {
        var pe = LoadFable();
        var file = pe.FileOffset(0x00430900);
        Assert.True(file > 0);
        Assert.True(X86.IsFunctionStart(pe.Data, file));
        Assert.Equal(0x56, pe.Data[file]);
        Assert.Equal(0x8B, pe.Data[file + 1]);
        Assert.Equal(0xF1, pe.Data[file + 2]);
        Assert.Equal(0xCC, pe.Data[file - 1]);
    }

    [Fact]
    public void Mid_lea_00430C80_snaps_to_instruction_start()
    {
        var pe = LoadFable();
        var mid = pe.FileOffset(0x00430C80);
        var start = X86.FindInsnStart(pe, mid);
        Assert.Equal(0x00430C7Fu, pe.Va(start));
        var steps = X86.Walk(pe, start, 3, stopOnRet: false);
        Assert.StartsWith("lea ecx, [esi+", steps[0].Text);
        Assert.DoesNotContain("db 0x8E", steps[0].Text);
    }

    [Fact]
    public void Walk_from_00430345_stops_at_thiscall_not_980_insns()
    {
        var pe = LoadFable();
        var file = pe.FileOffset(0x00430345);
        var steps = X86.WalkFunction(pe, file, 2500);
        Assert.True(steps.Count < 200, $"merged walk insns={steps.Count}");
        Assert.True(steps.All(s => s.Va < 0x00430900u),
            $"walk leaked into Transfer last=0x{steps[^1].Va:X8}");
    }

    [Fact]
    public void Fn_from_00430C80_without_exact_is_Transfer()
    {
        var pe = LoadFable();
        var file = pe.FileOffset(0x00430C80);
        var start = X86.FindPrologue(pe, file);
        Assert.Equal(0x00430900u, pe.Va(start));
    }

    [Fact]
    public void WalkAllCode_includes_Transfer_thiscall()
    {
        var pe = LoadFable();
        var nodes = FunctionMap.WalkAllCode(pe);
        Assert.Contains(nodes, n => n.Va == 0x00430900u);
        var helper = nodes.Single(n => n.Va == 0x00430345u);
        Assert.True(helper.Insns < 200, $"00430345 still merged insns={helper.Insns}");
    }

    [Fact]
    public void Opcode_82_is_alu_imm8_alias_not_db()
    {
        // add al, 1  encoded as 82 C0 01 (alias of 80 C0 01)
        var pe = TinyPe([0x82, 0xC0, 0x01, 0xC3]);
        var steps = X86.Walk(pe, 0, 4, stopOnRet: true);
        Assert.StartsWith("add", steps[0].Text);
        Assert.DoesNotContain("db", steps[0].Text);
    }

    private static PeImage LoadFable()
    {
        var install = GameInstall.TryLocate();
        Assert.NotNull(install);
        var exe = Path.Combine(install.Root, "Fable.exe");
        Assert.True(File.Exists(exe), exe);
        return PeImage.Load(exe);
    }

    private static PeImage TinyPe(byte[] code) =>
        new()
        {
            Data = code,
            ImageBase = 0x00400000,
            TimeDateStamp = 0,
            SizeOfImage = (uint)code.Length,
            Sections =
            [
                new PeSection(".text", 0, (uint)code.Length, 0, (uint)code.Length, 0x60000020),
            ],
            Imports = [],
        };
}
