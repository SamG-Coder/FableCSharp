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
    public void Fable_iat_has_d3d9()
    {
        var pe = LoadFable();
        Assert.Contains(pe.Iat.Values, v => v.Contains("d3d9.dll", StringComparison.OrdinalIgnoreCase)
            || v.Contains("Direct3D", StringComparison.OrdinalIgnoreCase));
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

    [Fact]
    public void Cpuid_is_two_bytes_not_modrm()
    {
        var pe = TinyPe([0x0F, 0xA2, 0xC3]);
        var steps = X86.Walk(pe, 0, 4, stopOnRet: true);
        Assert.Equal("cpuid", steps[0].Text);
        Assert.Equal("ret", steps[1].Text);
        Assert.Equal(0x00400002u, steps[1].Va);
    }

    [Fact]
    public void Rdtsc_is_two_bytes_not_modrm()
    {
        var pe = TinyPe([0x0F, 0x31, 0xC3]);
        var steps = X86.Walk(pe, 0, 4, stopOnRet: true);
        Assert.Equal("rdtsc", steps[0].Text);
        Assert.Equal("ret", steps[1].Text);
        Assert.Equal(0x00400002u, steps[1].Va);
    }

    [Fact]
    public void Bswap_eax_is_two_bytes()
    {
        var pe = TinyPe([0x0F, 0xC8, 0xC3]);
        var steps = X86.Walk(pe, 0, 4, stopOnRet: true);
        Assert.Equal("bswap eax", steps[0].Text);
        Assert.Equal("ret", steps[1].Text);
    }

    [Fact]
    public void Push_fs_pop_gs_are_two_bytes()
    {
        var pe = TinyPe([0x0F, 0xA0, 0x0F, 0xA9, 0xC3]);
        var steps = X86.Walk(pe, 0, 6, stopOnRet: true);
        Assert.Equal("push fs", steps[0].Text);
        Assert.Equal("pop gs", steps[1].Text);
        Assert.Equal("ret", steps[2].Text);
    }

    [Fact]
    public void Sse_detect_00A5B850_is_cpuid_then_test_edx()
    {
        var pe = LoadFable();
        var file = pe.FileOffset(0x00A5B850);
        var steps = X86.WalkFunction(pe, file, 32);
        Assert.Equal("push ebx", steps[0].Text);
        Assert.StartsWith("mov eax,", steps[1].Text);
        Assert.Equal("cpuid", steps[2].Text);
        Assert.StartsWith("test edx,", steps[3].Text);
        Assert.DoesNotContain(steps, s => s.Text.Contains("0F_A2", StringComparison.Ordinal));
        Assert.Contains(steps, s => s.Text == "pop ebx");
        Assert.Contains(steps, s => s.Text == "ret");
        Assert.True(steps.Count < 16, $"sse-detect insns={steps.Count}");
        Assert.Equal(0x00A5B868u, steps[^1].Va);
    }

    [Fact]
    public void F6_slash1_test_alias_consumes_imm8()
    {
        var pe = TinyPe([0xF6, 0xC8, 0x01, 0xC3]);
        var steps = X86.Walk(pe, 0, 4, stopOnRet: true);
        Assert.StartsWith("test", steps[0].Text);
        Assert.Equal("ret", steps[1].Text);
        Assert.Equal(0x00400003u, steps[1].Va);
    }

    [Fact]
    public void Movaps_uses_xmm_not_gpr()
    {
        var pe = TinyPe([0x0F, 0x28, 0x02, 0xC3]);
        var steps = X86.Walk(pe, 0, 4, stopOnRet: true);
        Assert.Equal("movaps xmm0, [edx]", steps[0].Text);
        Assert.Equal("0F 28 02", steps[0].Bytes);
        Assert.Equal("ret", steps[1].Text);
    }

    [Fact]
    public void Grep_facts_disp_ff_rel()
    {
        Assert.True(GrepFacts.TryDisp("mov eax, [edi+348]", out var d) && d == 348);
        Assert.True(GrepFacts.TryFf("call [edx+52]", out var kind, out var mem));
        Assert.Equal("call", kind);
        Assert.Equal("[edx+52]", mem);
        Assert.True(GrepFacts.TryRelTarget("je 00401BD9", out var dest));
        Assert.Equal(0x00401BD9u, dest);
        Assert.False(GrepFacts.TryRelTarget("jmp [0x401BDC+eax*4]", out _));
        Assert.Contains(0x013D2880u, GrepFacts.AbsValues("mov [0x13D2880], 0x01"));
    }

    [Fact]
    public void Shufps_xmm4_imm0()
    {
        var pe = TinyPe([0x0F, 0xC6, 0xE4, 0x00, 0xC3]);
        var steps = X86.Walk(pe, 0, 4, stopOnRet: true);
        Assert.Equal("shufps xmm4, xmm4, 0", steps[0].Text);
    }

    [Fact]
    public void Matrix_0098789F_is_movaps_xmm0()
    {
        var pe = LoadFable();
        var file = pe.FileOffset(0x0098789F);
        var steps = X86.Walk(pe, file, 8, stopOnRet: false);
        Assert.Equal("movaps xmm0, [edx]", steps[0].Text);
        Assert.Equal("movaps xmm1, [edx+16]", steps[1].Text);
        Assert.Contains("xmm", steps[0].Text, StringComparison.Ordinal);
        Assert.DoesNotContain("movaps eax", steps[0].Text, StringComparison.Ordinal);
    }

    [Fact]
    public void Switch_table_dumps_as_dd_not_salc()
    {
        var pe = TinyPe(
        [
            0xFF, 0x24, 0x85, 0x10, 0x00, 0x40, 0x00,
            0xC3,
            0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC,
            0x07, 0x00, 0x40, 0x00,
            0x07, 0x00, 0x40, 0x00,
            0xCC,
        ]);
        var steps = X86.Walk(pe, 0, 16, stopOnRet: false);
        Assert.Equal("jmp [0x400010+eax*4]", steps[0].Text);
        Assert.Contains(steps, s => s.Va == 0x00400010u && s.Text == "dd 0x00400007");
        Assert.DoesNotContain(steps, s => s.Va == 0x00400010u && s.Text.StartsWith("salc", StringComparison.Ordinal));
    }

    [Fact]
    public void Fable_00401BDC_is_switch_dd()
    {
        var pe = LoadFable();
        var file = pe.FileOffset(0x00401BDC);
        var steps = X86.Walk(pe, file, 24, stopOnRet: false);
        Assert.StartsWith("dd 0x", steps[0].Text);
        Assert.Equal("dd 0x00401BD6", steps[0].Text);
        Assert.DoesNotContain("salc", steps[0].Text, StringComparison.Ordinal);
        var idx = steps.First(s => s.Va == 0x00401BE4u);
        Assert.Equal("db 0x00", idx.Text);
    }

    [Fact]
    public void Fable_0054E32C_is_switch_dd_then_next_fn()
    {
        var pe = LoadFable();
        var file = pe.FileOffset(0x0054E2B1);
        var steps = X86.Walk(pe, file, 80, stopOnRet: false);
        Assert.Equal("jmp [0x54E32C+eax*4]", steps[0].Text);
        var table = steps.First(s => s.Va == 0x0054E32Cu);
        Assert.StartsWith("dd 0x", table.Text);
        Assert.DoesNotContain(steps, s => s.Va == 0x0054E32Cu && s.Text == "cli");
        Assert.Contains(steps, s => s.Va == 0x0054E350u && s.Text == "push ebx");
        var idx = steps.First(s => s.Va == 0x0054E33Cu);
        Assert.StartsWith("db 0x", idx.Text);
        Assert.DoesNotContain(steps, s => s.Va == 0x0054E33Cu && s.Text.StartsWith("add", StringComparison.Ordinal));
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
            EntryPoint = 0x00400000,
            Sections =
            [
                new PeSection(".text", 0, (uint)code.Length, 0, (uint)code.Length, 0x60000020),
            ],
            Imports = [],
        };
}
