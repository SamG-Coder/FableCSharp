using Fable.Core;
using Fable.Formats.Banks;
using Fable.Formats.Defs;
using Fable.Formats.Meshes;
using Fable.Formats.Qst;
using Fable.Formats.Textures;
using Fable.Formats.Upk;
using Fable.Formats.Wld;
using Fable.Game;

if (args.Length == 0)
{
    PrintUsage();
    return 1;
}

var pathOverride = Environment.GetEnvironmentVariable(GameInstall.PathEnvironmentVariable);
var commandIndex = 0;
if (Directory.Exists(args[0]) || File.Exists(Path.Combine(args[0], "Fable.exe")))
{
    pathOverride = args[0];
    commandIndex = 1;
}

if (commandIndex >= args.Length)
{
    PrintUsage();
    return 1;
}

var command = args[commandIndex].ToLowerInvariant();
var rest = args.Skip(commandIndex + 1).ToArray();
var install = GameInstall.TryLocate(pathOverride);
if (install is null)
{
    Console.Error.WriteLine("Fable install not found. Set FABLE_PATH or pass the install directory.");
    return 2;
}

switch (command)
{
    case "info":
        DumpInfo(install);
        break;
    case "wld":
        DumpWorld(install);
        break;
    case "wad":
        DumpWad(install, rest.FirstOrDefault());
        break;
    case "tng":
        if (rest.Length == 0)
        {
            Console.Error.WriteLine("Usage: Fable.Dump tng <LookoutPoint>");
            return 1;
        }
        DumpTng(install, rest[0]);
        break;
    case "qst":
        DumpQuests(install);
        break;
    case "names":
        DumpNames(install, rest.FirstOrDefault());
        break;
    case "big":
        DumpBig(install, rest.FirstOrDefault());
        break;
    case "upk":
        DumpUpk(install, rest.FirstOrDefault());
        break;
    case "mesh":
        DumpMesh(install, rest.FirstOrDefault());
        break;
    case "lev":
        DumpLev(install, rest.FirstOrDefault() ?? "LookoutPoint");
        break;
    case "tex":
        DumpTex(install, rest.FirstOrDefault());
        break;
    case "bin":
        DumpGameBin(install, rest.FirstOrDefault());
        break;
    default:
        PrintUsage();
        return 1;
}

return 0;

static void PrintUsage()
{
    Console.WriteLine("""
        Fable.Dump — inspect Fable TLC / Anniversary game files

        Usage:
          Fable.Dump [install-path] <command>

        Commands:
          info                 install + data roots
          wld                  FinalAlbion.wld region graph
          wad [filter]         FinalAlbion.wad entries
          tng <region>         things in a region (LookoutPoint)
          qst                  quest table
          names [filter]       compiled definition names
          big [path-or-name]   BIGB / BBB bank header
          upk [path-or-name]   Unreal package header (Anniversary only)
          mesh [id|name]       parse a graphics.big mesh
          lev [region]         inspect a compiled .lev
          tex [id|name]        decode a textures.big image
          bin [name]           compiled game.bin def / mesh id

        FABLE_PATH overrides the default Steam TLC install.
        """);
}

static void DumpInfo(GameInstall install)
{
    Console.WriteLine($"Edition:     {install.Edition}");
    Console.WriteLine($"Root:        {install.Root}");
    Console.WriteLine($"Data:        {install.DataRoot}");
    Console.WriteLine($"World:       {install.WorldPath}  exists={File.Exists(install.WorldPath)}");
    Console.WriteLine($"WAD:         {install.WadPath}  exists={File.Exists(install.WadPath)}");
    Console.WriteLine($"Loose TNG:   {install.LooseLevelsDirectory}  exists={Directory.Exists(install.LooseLevelsDirectory)}");
    Console.WriteLine($"names.bin:   {install.FindCompiledDef("names.bin") ?? "(missing)"}");
    Console.WriteLine($"CookedPC:    {install.CookedPcDirectory ?? "(none)"}");
}

static void DumpWorld(GameInstall install)
{
    var world = WorldFile.Load(install.WorldPath);
    Console.WriteLine($"Initial quests: {string.Join(", ", world.InitialQuests)}");
    Console.WriteLine($"Maps: {world.Maps.Count}  MapUIDCount={world.MapUidCount}");
    Console.WriteLine($"{"#",4} {"X",6} {"Y",6} {"UID",10} {"Script",-28} Level");
    foreach (var map in world.Maps)
    {
        Console.WriteLine(
            $"{map.Index,4} {map.MapX,6} {map.MapY,6} {map.MapUid,10} {map.ScriptName,-28} {map.LevelName}");
    }
}

static void DumpWad(GameInstall install, string? filter)
{
    if (!File.Exists(install.WadPath))
    {
        Console.WriteLine("No FinalAlbion.wad (Anniversary often ships loose TNG files instead).");
        if (Directory.Exists(install.LooseLevelsDirectory))
        {
            var files = Directory.GetFiles(install.LooseLevelsDirectory, "*.tng");
            Console.WriteLine($"Loose TNG count: {files.Length}");
        }
        return;
    }

    using var wad = BbbArchive.Open(install.WadPath);
    var entries = wad.Entries.AsEnumerable();
    if (!string.IsNullOrWhiteSpace(filter))
        entries = entries.Where(e => e.Name.Contains(filter, StringComparison.OrdinalIgnoreCase));

    var list = entries.ToList();
    Console.WriteLine($"WAD version={wad.Version} align={wad.Alignment} entries={wad.Entries.Count} shown={list.Count}");
    foreach (var entry in list)
        Console.WriteLine($"{entry.Size,10}  type={entry.Type,-4}  {entry.Name}");
}

static void DumpTng(GameInstall install, string region)
{
    using var levels = new LevelLibrary(install);
    var file = levels.LoadThings(region);
    var things = file.Things.ToList();
    Console.WriteLine($"Region {region}: version={file.Version} sections={file.Sections.Count} things={things.Count}");
    foreach (var thing in things)
    {
        var pos = thing.PositionX is null
            ? ""
            : $" ({thing.PositionX:0.###}, {thing.PositionY:0.###}, {thing.PositionZ:0.###})";
        Console.WriteLine(
            $"{thing.Kind,-16} {(thing.DefinitionType ?? "-"),-40} {(thing.ScriptName ?? "-"),-24}{pos}");
    }
}

static void DumpQuests(GameInstall install)
{
    var quests = QuestFile.Load(install.QuestPath);
    Console.WriteLine($"Quests: {quests.Quests.Count}");
    foreach (var quest in quests.Quests)
        Console.WriteLine($"{(quest.Persistent ? "P" : " ")}  {quest.Name}");
}

static void DumpNames(GameInstall install, string? filter)
{
    var path = install.FindCompiledDef("names.bin");
    if (path is null)
    {
        Console.Error.WriteLine("names.bin not found.");
        return;
    }

    var names = NamesBin.Load(path);
    var entries = string.IsNullOrWhiteSpace(filter)
        ? names.Entries
        : names.Search(filter).ToList();
    Console.WriteLine($"names.bin declared={names.DeclaredCount} parsed={names.Entries.Count} shown={entries.Count}");
    foreach (var entry in entries)
        Console.WriteLine($"{entry.Hash:X8}  {entry.Name}");
}

static void DumpBig(GameInstall install, string? name)
{
    var path = ResolveBankPath(install, name);
    if (path is null)
    {
        Console.WriteLine("Available banks:");
        foreach (var bank in install.FindBigBanks())
            Console.WriteLine($"  {bank}");
        return;
    }

    using var stream = File.OpenRead(path);
    var magicBytes = new byte[4];
    if (stream.Read(magicBytes, 0, 4) != 4)
        throw new InvalidDataException(path);
    stream.Seek(0, SeekOrigin.Begin);
    var magic = BitConverter.ToUInt32(magicBytes, 0);

    if (magic == BbbArchive.Magic)
    {
        using var bbb = BbbArchive.Open(stream, ownsStream: false);
        Console.WriteLine($"{path}");
        Console.WriteLine($"BBBB version={bbb.Version} entries={bbb.Entries.Count} footer=0x{bbb.FooterOffset:X}");
        foreach (var entry in bbb.Entries.Take(40))
            Console.WriteLine($"{entry.Size,10}  {entry.Name}");
        if (bbb.Entries.Count > 40)
            Console.WriteLine($"... {bbb.Entries.Count - 40} more");
    }
    else if (magic == BigArchive.Magic)
    {
        using var big = BigArchive.Open(stream, ownsStream: false);
        Console.WriteLine($"{path}");
        Console.WriteLine($"BIGB version={big.Version} subbanks={big.SubBanks.Count} footer=0x{big.FooterOffset:X}");
        foreach (var bank in big.SubBanks)
            Console.WriteLine($"{bank.EntryCount,6} entries  size={bank.Size,10}  {bank.Name}");
    }
    else
    {
        Console.Error.WriteLine($"Unknown bank magic 0x{magic:X8} in {path}");
    }
}

static void DumpUpk(GameInstall install, string? name)
{
    if (install.CookedPcDirectory is null)
    {
        Console.WriteLine("No CookedPC directory. UPK files exist only in Fable Anniversary.");
        return;
    }

    string? path = null;
    if (!string.IsNullOrWhiteSpace(name) && File.Exists(name))
        path = name;
    else if (!string.IsNullOrWhiteSpace(name))
    {
        path = Directory.EnumerateFiles(install.CookedPcDirectory, name, SearchOption.AllDirectories)
            .FirstOrDefault();
        if (path is null && !name.EndsWith(".umap", StringComparison.OrdinalIgnoreCase))
        {
            path = Directory.EnumerateFiles(install.CookedPcDirectory, name + ".umap", SearchOption.AllDirectories)
                .FirstOrDefault();
        }
    }
    else
    {
        path = Directory.EnumerateFiles(Path.Combine(install.CookedPcDirectory, "Maps", "Regions"), "*.umap")
            .FirstOrDefault();
    }

    if (path is null)
    {
        Console.Error.WriteLine("No UPK/UMAP found.");
        return;
    }

    var header = UpkHeader.Load(path);
    Console.WriteLine(path);
    Console.WriteLine(
        $"magic=0x{header.Magic:X8} unreal={header.IsUnrealPackage} version={header.Version} licensee={header.Licensee} size={header.FileSize}");
}

static void DumpMesh(GameInstall install, string? query)
{
    var graphics = Path.Combine(install.DataRoot, "graphics", "graphics.big");
    var headerPath = Path.Combine(install.DataRoot, "Defs", "RetailHeaders", "meshdata.h");
    using var big = BigArchive.Open(graphics);
    var bank = big.SubBanks.First(b => b.Name.Contains("MESH", StringComparison.OrdinalIgnoreCase));
    var entries = big.ReadEntries(bank);
    HeaderEnums? enums = File.Exists(headerPath) ? HeaderEnums.Load(headerPath) : null;

    BankEntry? entry = null;
    if (uint.TryParse(query, out var id))
        entry = entries.FirstOrDefault(e => e.Id == id);
    else if (!string.IsNullOrWhiteSpace(query))
    {
        var mapped = enums?.FindMeshId(query);
        entry = entries.FirstOrDefault(e =>
            e.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            (mapped is not null && e.Id == (uint)mapped.Value));
    }
    else
        entry = entries.FirstOrDefault(e => e.Type is 1 or 2);

    if (entry is null)
    {
        Console.WriteLine($"No mesh matched '{query}'. First 20 static meshes:");
        foreach (var item in entries.Where(e => e.Type is 1 or 2).Take(20))
            Console.WriteLine($"{item.Id,6} type={item.Type} {item.Name}");
        return;
    }

    var bytes = big.Read(entry);
    Console.WriteLine($"#{entry.Id} type={entry.Type} size={entry.Size} {entry.Name}");
    Console.WriteLine("head " + Convert.ToHexString(bytes.AsSpan(0, Math.Min(80, bytes.Length))));
    WalkMeshHeader(bytes);
    try
    {
        var mesh = MeshFile.Parse(bytes, (int)entry.Type);
        Console.WriteLine($"name={mesh.Name} tris={mesh.Triangles.Count} bounds {mesh.BoundsMin} .. {mesh.BoundsMax}");
        foreach (var mat in mesh.Materials)
            Console.WriteLine($"  mat '{mat.Name}' diffuse={mat.DiffuseMapId} bump={mat.BumpMapId}");
        if (mesh.Triangles.Count > 0)
        {
            var uvMin = mesh.Triangles.Min(tri => Math.Min(tri.UvA.X, tri.UvA.Y));
            var uvMax = mesh.Triangles.Max(tri => Math.Max(tri.UvA.X, tri.UvA.Y));
            Console.WriteLine($"  uv range [{uvMin:0.###},{uvMax:0.###}] textured={mesh.Triangles.Count(tri => tri.TextureId > 0)}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Parse failed: " + ex.Message);
    }
}

static void WalkMeshHeader(byte[] data)
{
    var c = 0;
    string Str()
    {
        var s = c;
        while (c < data.Length && data[c] != 0) c++;
        var t = System.Text.Encoding.ASCII.GetString(data, s, c - s);
        if (c < data.Length) c++;
        return t;
    }
    int I32() { var v = BitConverter.ToInt32(data, c); c += 4; return v; }
    ushort U16() { var v = BitConverter.ToUInt16(data, c); c += 2; return v; }

    Console.WriteLine($"name={Str()} cursor={c}");
    Console.WriteLine($"anim={data[c++]}");
    c += 12 + 4 + 12 + 12;
    var helpers = U16();
    var dummies = U16();
    var names = U16();
    var vols = U16();
    var gens = U16();
    Console.WriteLine($"helpers={helpers} dummies={dummies} names={names} vols={vols} gens={gens} cursor={c}");
    if (helpers + dummies + names + vols != 0)
    {
        Console.WriteLine("header has compressed blocks; skip detailed walk");
        return;
    }

    for (var i = 0; i < gens; i++)
    {
        c += 48 + 4;
        var genName = Str();
        var bank = I32();
        var local = data[c++];
        Console.WriteLine($"  gen[{i}] '{genName}' bank={bank} local={local} cursor={c}");
    }

    Console.WriteLine($"mats={I32()} prims={I32()} bones={I32()} boneNames={I32()} cloth={data[c++]} static={U16()} anim={U16()} cursor={c}/{data.Length}");
}

static void DumpLev(GameInstall install, string region)
{
    using var wad = BbbArchive.Open(install.WadPath);
    var entry = wad.Find(region.EndsWith(".lev", StringComparison.OrdinalIgnoreCase) ? region : region + ".lev");
    if (entry is null)
    {
        Console.Error.WriteLine($"No LEV for {region}");
        return;
    }

    var bytes = wad.Read(entry);
    Console.WriteLine($"{entry.Name} size={bytes.Length}");
    Console.WriteLine("hex " + Convert.ToHexString(bytes.AsSpan(0, Math.Min(64, bytes.Length))));
    var ascii = new string(bytes.Take(200).Select(b => b is >= 32 and <= 126 ? (char)b : '.').ToArray());
    Console.WriteLine(ascii);

    var asMesh = MeshFile.TryParse(bytes);
    Console.WriteLine(asMesh is null
        ? "Not a C3D mesh."
        : $"Parsed as mesh '{asMesh.Name}' tris={asMesh.Triangles.Count} bounds {asMesh.BoundsMin} .. {asMesh.BoundsMax}");
}

static void DumpTex(GameInstall install, string? query)
{
    var path = Path.Combine(install.DataRoot, "graphics", "pc", "textures.big");
    using var big = BigArchive.Open(path);
    var bank = big.SubBanks.First(item => item.Name == "GBANK_MAIN_PC");
    var entries = big.ReadEntries(bank);
    BankEntry? entry = null;
    if (uint.TryParse(query, out var id))
        entry = entries.FirstOrDefault(e => e.Id == id);
    else if (!string.IsNullOrWhiteSpace(query))
        entry = entries.FirstOrDefault(e => e.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
    entry ??= entries.FirstOrDefault(e => e.Name == "LANDSCAPE_GRASS_PLAIN") ?? entries.First();

    var header = TextureFile.ReadHeader(entry.Info.ToArray());
    var texture = TextureFile.Parse(entry.Id, entry.Name, entry.Type, entry.Info, big.Read(entry));
    Console.WriteLine($"#{entry.Id} type={entry.Type} packed={entry.Size} {entry.Name}");
    Console.WriteLine($"header {header.Width}x{header.Height} frame={header.FrameWidth}x{header.FrameHeight} fmt={header.FormatCode}");
    Console.WriteLine($"decoded {texture.Compression} {texture.Width}x{texture.Height} rgba={texture.Rgba.Length}");
}

static void DumpGameBin(GameInstall install, string? query)
{
    var namesPath = install.FindCompiledDef("names.bin");
    var binPath = install.FindCompiledDef("game.bin");
    if (namesPath is null || binPath is null)
    {
        Console.Error.WriteLine("names.bin / game.bin not found.");
        return;
    }

    var names = NamesBin.Load(namesPath);
    var bin = GameBin.Load(binPath, names);
    Console.WriteLine(
        $"game.bin entries={bin.Entries.Count} chunks={bin.Chunks.Count} useNames={bin.UseNamesBin} file=0x{bin.FileIndicator:X8} plat=0x{bin.PlatformIndicator:X8}");

    if (string.IsNullOrWhiteSpace(query))
    {
        var types = bin.Entries
            .GroupBy(entry => entry.TypeName ?? "?")
            .OrderByDescending(group => group.Count())
            .Take(20);
        foreach (var group in types)
            Console.WriteLine($"  {group.Count(),5}  {group.Key}");
        var withMesh = bin.Entries.Count(entry => entry.MeshId is > 0);
        Console.WriteLine($"entries with mesh id: {withMesh}");
        return;
    }

    var matches = bin.Entries.Where(entry =>
            (entry.InstanceName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (entry.TypeName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (entry.SourceName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
        .Take(30)
        .ToList();
    Console.WriteLine($"matches={matches.Count}");
    foreach (var entry in matches)
    {
        Console.WriteLine(
            $"#{entry.Index} type={entry.TypeName} inst={entry.InstanceName} src={entry.SourceName} mesh={entry.MeshId} subs={entry.SubDefs.Count} raw={entry.Raw.Length}");
        foreach (var sub in entry.SubDefs.Take(8))
        {
            var child = sub.DefIndex >= 0 && sub.DefIndex < bin.Entries.Count ? bin.Entries[sub.DefIndex] : null;
            Console.WriteLine(
                $"    sub crc={sub.NameCrc:X8} idx={sub.DefIndex} -> {child?.TypeName} {child?.InstanceName} mesh={child?.MeshId}");
        }
    }

    var mapped = bin.FindMeshId(query);
    Console.WriteLine($"FindMeshId({query}) = {mapped}");
}

static string? ResolveBankPath(GameInstall install, string? name)
{
    if (string.IsNullOrWhiteSpace(name))
        return install.FindBigBanks().FirstOrDefault();
    if (File.Exists(name))
        return name;

    var relative = Path.Combine(install.DataRoot, name);
    if (File.Exists(relative))
        return relative;

    return install.FindBigBanks()
        .FirstOrDefault(path => Path.GetFileName(path).Equals(name, StringComparison.OrdinalIgnoreCase));
}
