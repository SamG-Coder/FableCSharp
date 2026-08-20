#r "C:\FableCSharp\src\Fable.Formats\bin\Debug\net10.0\Fable.Formats.dll"
#r "C:\FableCSharp\src\Fable.Core\bin\Debug\net10.0\Fable.Core.dll"
using Fable.Core;
using Fable.Formats.Defs;

const uint Plus545Crc = 0x9E47F106u;
const uint Plus544Crc = 0xCA2D971Du;
const uint Plus522Crc = 0xE59C9B55u;
const uint Plus548Crc = 0xF26C87EAu;

var install = GameInstall.TryLocate() ?? throw new Exception("no install");
var names = NamesBin.Load(install.FindCompiledDef("names.bin")!);
var bin = GameBin.Load(install.FindCompiledDef("frontend.bin")!, names);
Console.WriteLine($"frontend={install.FindCompiledDef("frontend.bin")}");

foreach (var name in new[]
{
    "UI_FRONTEND_BUTTON_NEW_GAME",
    "UI_ACCEPT_NEW_PROFILE",
    "UI_FRONTEND_BUTTON_INVISIBLE",
    "UI_FRONTEND_LIST_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE",
    "UI_FRONTEND_MAIN_MENU_NO_LIVEAWARE_NO_CONTINUE",
})
{
    var entry = bin.FindEntry(name);
    if (entry is null)
    {
        Console.WriteLine($"{name} MISSING");
        continue;
    }

    var parsed = FrontendUiDef.TryParse(entry);
    var plus545 = FrontendUiDef.ReadPersistU8(entry.Raw, Plus545Crc);
    var plus544 = FrontendUiDef.ReadPersistU8(entry.Raw, Plus544Crc);
    var plus522 = FrontendUiDef.ReadPersistU8(entry.Raw, Plus522Crc);
    var plus548 = FrontendUiDef.ReadPersistI32(entry.Raw, Plus548Crc);
    var hits = 0;
    var first = -1;
    var firstByte = -1;
    for (var i = 0; i + 5 <= entry.Raw.Length; i++)
    {
        if (BitConverter.ToUInt32(entry.Raw, i) != Plus545Crc)
            continue;
        hits++;
        if (first < 0)
        {
            first = i;
            firstByte = entry.Raw[i + 4];
        }
    }

    Console.WriteLine(
        $"{name} type={parsed?.Type} raw={entry.Raw.Length} " +
        $"+544={plus544} +522={plus522} +545={plus545} +548={plus548} " +
        $"hits={hits} @{first} byte={firstByte} " +
        $"msg={parsed?.MessageId} +224={parsed?.Plus224}");

    if (first >= 0)
    {
        var start = Math.Max(0, first - 16);
        var end = Math.Min(entry.Raw.Length, first + 13);
        Console.Write("  hex ");
        for (var i = start; i < end; i++)
            Console.Write($"{entry.Raw[i]:X2}");
        Console.WriteLine();
    }
}
