using Fable.Core;
using Fable.Formats.Defs;

internal static class TransformDump
{
    public static void Run()
    {
        var install = GameInstall.TryLocate() ?? throw new InvalidOperationException("no install");
        var names = NamesBin.Load(install.FindCompiledDef("names.bin")!);
        var bin = GameBin.Load(install.FindCompiledDef("frontend.bin")!, names);

        Console.WriteLine("CUIDef fields (original Lionhead names)");
        foreach (var field in FrontendUiFieldCatalog.Fields)
            Console.WriteLine($"{field.Name}\t0x{field.Crc:X8}\t{field.SerializedAs}\tretail+{field.RetailOffset}\tdonor+{field.DonorOffset}");

        Console.WriteLine("CUIStateDef fields (original Lionhead names)");
        foreach (var field in FrontendUiFieldCatalog.StateFields)
            Console.WriteLine($"{field.Name}\t0x{field.Crc:X8}\t{field.SerializedAs}\tretail+{field.RetailOffset}\tdonor+{field.DonorOffset}");

        var count = 0;
        foreach (var entry in bin.Entries.Where(e => e.TypeName == "UI"))
        {
            if (!FrontendUiSchema.TryConsume(entry, out var end, out var error) || end != entry.Raw.Length)
                throw new InvalidDataException($"{entry.InstanceName}: {error ?? $"ended at {end}/{entry.Raw.Length}"}");
            count++;
        }

        Console.WriteLine($"Validated {count} UI entries through exact EOF.");
    }
}
