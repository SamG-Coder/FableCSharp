#r "C:\FableCSharp\src\Fable.Formats\bin\Debug\net10.0\Fable.Formats.dll"
#r "C:\FableCSharp\src\Fable.Core\bin\Debug\net10.0\Fable.Core.dll"
using Fable.Core;
using Fable.Formats.Defs;

var install = GameInstall.TryLocate() ?? throw new Exception("no install");
var names = NamesBin.Load(install.FindCompiledDef("names.bin")!);
var bin = GameBin.Load(install.FindCompiledDef("frontend.bin")!, names);
foreach (var name in new[] { "UI_ACCEPT_NEW_PROFILE", "UI_FRONTEND_BUTTON_NEW_GAME", "UI_FRONTEND_BUTTON_INVISIBLE" })
{
    var entry = bin.FindEntry(name)!;
    var parsed = FrontendUiDef.TryParse(entry)!;
    var plus224 = FrontendUiDef.ReadPersistI32(entry.Raw, FrontendUiDef.Plus224Crc);
    var plus228 = FrontendUiDef.ReadPersistI32(entry.Raw, FrontendUiDef.MessageIdCrc);
    Console.WriteLine($"{name} type={parsed.Type} raw={entry.Raw.Length} Plus224={plus224} (0x{plus224:X}) MessageId={plus228} (0x{plus228:X}) parsed.Plus224={parsed.Plus224} parsed.MessageId={parsed.MessageId}");
}
