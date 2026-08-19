using System.Text;
using Fable.Core;
using Fable.Formats.Banks;
using Fable.Formats.Defs;
using Fable.Formats.Levels;
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
    case "anim":
        DumpAnim(install, rest.FirstOrDefault());
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
    case "bins":
        DumpCompiledBins(install, rest.FirstOrDefault());
        break;
    case "scene":
        DumpScene(install, rest.FirstOrDefault() ?? "LookoutPoint");
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
          anim [id|name]       hex-dump a type-6 animation entry
          lev [region]         inspect a compiled .lev
          tex [id|name]        decode a textures.big image
          bin [name]           compiled game.bin def / mesh id
          bins [out-dir]       dump every CompiledDefs .bin (frontend/script/game/names)
          scene [region]       tile/object coverage vs AABB neighbours

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
        Console.WriteLine($"name={mesh.Name} tris={mesh.Triangles.Count} declared={mesh.DeclaredTriangles} strip={mesh.StripFaces} list={mesh.ListFaces} noblock={mesh.NoBlockFaces} deg={mesh.DegenerateSkipped} bounds {mesh.BoundsMin} .. {mesh.BoundsMax}");
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

static void DumpAnim(GameInstall install, string? query)
{
    var graphics = Path.Combine(install.DataRoot, "graphics", "graphics.big");
    using var big = BigArchive.Open(graphics);
    var bank = big.SubBanks.First(b => b.Name.Contains("MESH", StringComparison.OrdinalIgnoreCase));
    var entries = big.ReadEntries(bank);
    BankEntry? entry = null;
    if (uint.TryParse(query, out var id))
        entry = entries.FirstOrDefault(e => e.Id == id);
    else if (!string.IsNullOrWhiteSpace(query))
        entry = entries.FirstOrDefault(e =>
            e.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
    else
        entry = entries.FirstOrDefault(e => e.Type == 6);
    if (entry is null)
    {
        Console.WriteLine("No type-6 entry.");
        return;
    }

    var data = big.Read(entry);
    Console.WriteLine($"#{entry.Id} type={entry.Type} size={entry.Size} {entry.Name}");
    var n = Math.Min(data.Length, 384);
    for (var i = 0; i < n; i += 16)
    {
        var slice = data.AsSpan(i, Math.Min(16, n - i));
        var hex = Convert.ToHexString(slice);
        var ascii = new string(slice.ToArray().Select(b => b is >= 32 and <= 126 ? (char)b : '.').ToArray());
        Console.WriteLine($"{i:X4}  {hex,-32}  {ascii}");
    }

    for (var i = 0; i + 4 <= data.Length; i++)
    {
        if (data[i] is < 0x41 or > 0x5A) continue;
        var ok = true;
        for (var k = 1; k < 4; k++)
            if (data[i + k] is < 0x41 or > 0x5A) { ok = false; break; }
        if (ok)
            Console.WriteLine($"FOURCC @{i:X4} {System.Text.Encoding.ASCII.GetString(data, i, 4)}");
    }
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

    var lev = LevFile.Parse(bytes);
    Console.WriteLine($"grid {lev.GridWidth}x{lev.GridHeight} materials={lev.Materials.Count} payload={lev.PayloadOffset}");
    var cells = LevCellGrid.TryParse(lev);
    if (cells is null)
    {
        Console.WriteLine("No 21-byte cell table.");
        return;
    }

    var hist = new Dictionary<byte, int>();
    for (var y = 0; y < cells.Height; y++)
    for (var x = 0; x < cells.Width; x++)
    {
        var slot = cells.Cells[x, y].Material0;
        hist[slot] = hist.GetValueOrDefault(slot) + 1;
    }
    Console.WriteLine($"cells {cells.Width}x{cells.Height} const60={cells.Cells[0, 0].Constant60}");
    foreach (var kv in hist.OrderByDescending(item => item.Value).Take(12))
    {
        var mat = lev.Materials.FirstOrDefault(m => m.Slot == kv.Key);
        var texHeader = Path.Combine(install.DataRoot, "Defs", "RetailHeaders", "pc", "textures.h");
        var mapped = File.Exists(texHeader)
            ? LandscapeTextures.Resolve(mat.Name ?? "", HeaderEnums.Load(texHeader))
            : 0;
        Console.WriteLine($"  slot {kv.Key,3} x{kv.Value,-6} {mat.Name} slotId={mat.Id} tex={mapped}");
    }
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

static void DumpScene(GameInstall install, string region)
{
    using var levels = new LevelLibrary(install);
    var things = levels.LoadThings(region);
    var world = WorldGeometry.Build(install, region, things.Things);
    Console.WriteLine($"region {region} maps={string.Join(",", world.Regions)}");
    Console.WriteLine($"instances={world.MeshInstances} missingMesh={world.MissingMeshes} tris={world.Triangles.Count}");
    Console.WriteLine($"primaryThings={things.Things.Count()}");

    foreach (var name in world.Regions)
    {
        var height = levels.LoadHeightField(name);
        if (height is null)
        {
            Console.WriteLine($"  {name}  NO STB");
            continue;
        }

        var compiled = levels.LoadCompiledLev(name);
        var cells = compiled is null ? null : LevCellGrid.TryParse(compiled);
        var tris = cells is null || compiled is null
            ? 0
            : height.ToTileTriangles(cells, compiled.Materials).Count;
        var tiles = height.Tiles.Tiles;
        var full = tiles.Count(t => t.Vertices.Count == 289);
        var adaptive = tiles.Count(t => t.Vertices.Count != 289);
        var noIx = tiles.Count(t => t.Vertices.Count != 289 && t.Indices.Count < 3);
        var extraObjs = tiles.Sum(t => t.Extras.Count);
        var extraVerts = tiles.Sum(t => t.Extras.Sum(e => e.Vertices.Count));
        var failedGrid = 0;
        foreach (var tile in tiles.Where(t => t.Vertices.Count == 289))
        {
            var keys = new HashSet<(int, int)>();
            foreach (var v in tile.Vertices)
                keys.Add(((int)MathF.Round(v.WorldX - height.OriginX), (int)MathF.Round(v.WorldY - height.OriginY)));
            if (keys.Count < 280)
                failedGrid++;
        }

        var tng = levels.TryLoadThings(name);
        Console.WriteLine(
            $"  {name} tiles={tiles.Count} full289={full} failGrid={failedGrid} adaptive={adaptive} adaptiveNoIx={noIx} extras={extraObjs}/{extraVerts}v landTris={tris} tng={(tng is null ? -1 : tng.Things.Count())} origin={height.OriginX},{height.OriginY} cells={height.CellsX}x{height.CellsY}");
    }

    var namesPath = install.FindCompiledDef("names.bin");
    var binPath = install.FindCompiledDef("game.bin");
    if (namesPath is null || binPath is null)
        return;
    var defs = GameBin.Load(binPath, NamesBin.Load(namesPath));
    var misses = new Dictionary<string, int>(StringComparer.Ordinal);
    foreach (var name in world.Regions)
    {
        var tng = levels.TryLoadThings(name);
        if (tng is null)
            continue;
        foreach (var thing in tng.Things)
        {
            if (thing.DefinitionType is null || thing.PositionX is null)
                continue;
            if (defs.FindMeshId(thing.DefinitionType) is not null)
                continue;
            misses[thing.DefinitionType] = misses.GetValueOrDefault(thing.DefinitionType) + 1;
        }
    }

    var lookout = levels.LoadHeightField(region);
    if (lookout is not null)
    {
        Console.WriteLine("sample adaptive index prefixes:");
        foreach (var tile in lookout.Tiles.Tiles.Where(t => t.Vertices.Count is > 3 and < 289 && t.Indices.Count >= 12).Take(4))
        {
            var head = string.Join(",", tile.Indices.Take(24));
            var mid = tile.Indices.Count > 600 ? string.Join(",", tile.Indices.Skip(560).Take(16)) : "-";
            var tail = string.Join(",", tile.Indices.TakeLast(12));
            Console.WriteLine($"  v={tile.Vertices.Count} ix={tile.Indices.Count} head={head}");
            Console.WriteLine($"    mid560={mid} tail={tail}");
            var deg = 0;
            var real = 0;
            for (var i = 0; i + 2 < tile.Indices.Count; i++)
            {
                var a = tile.Indices[i];
                var b = tile.Indices[i + 1];
                var c = tile.Indices[i + 2];
                if (a == b || b == c || a == c)
                    deg++;
                else
                    real++;
            }

            Console.WriteLine($"    stripDeg={deg} stripReal={real} fullQuadWouldBe=512");
        }
    }

    Console.WriteLine($"missing Graphic types={misses.Count}");
    foreach (var pair in misses.OrderByDescending(p => p.Value).Take(20))
        Console.WriteLine($"  {pair.Value,4} {pair.Key}");
}

static void DumpCompiledBins(GameInstall install, string? outOverride)
{
    var repo = FindRepoRoot();
    var dest = string.IsNullOrWhiteSpace(outOverride)
        ? Path.Combine(repo ?? Directory.GetCurrentDirectory(), "assembly", "compiled-defs")
        : Path.GetFullPath(outOverride);
    Directory.CreateDirectory(dest);

    var namesPath = install.FindCompiledDef("names.bin");
    if (namesPath is null)
    {
        Console.Error.WriteLine("names.bin not found.");
        return;
    }

    var names = NamesBin.Load(namesPath);
    DumpNamesBin(dest, namesPath, names);

    foreach (var fileName in new[] { "frontend.bin", "script.bin", "game.bin" })
    {
        var path = install.FindCompiledDef(fileName);
        if (path is null)
        {
            Console.Error.WriteLine($"{fileName} missing");
            continue;
        }

        var bin = GameBin.Load(path, names);
        var family = Path.GetFileNameWithoutExtension(fileName);
        DumpGameBinFamily(dest, family, path, bin, parseFrontend: fileName == "frontend.bin", parseScript: fileName == "script.bin");
    }

    DumpLooseBins(dest, install);
    WriteCompiledDefsIndex(dest, install, names);
    Console.WriteLine($"bins  {dest}");
}

static string? FindRepoRoot()
{
    foreach (var start in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
    {
        for (var dir = new DirectoryInfo(start); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "FableCSharp.slnx")))
                return dir.FullName;
        }
    }

    return null;
}

static void DumpNamesBin(string dest, string path, NamesBin names)
{
    var sb = new StringBuilder();
    sb.AppendLine("offset\tcrc\tname");
    foreach (var entry in names.Entries)
        sb.Append("0x").Append(entry.Offset.ToString("X8")).Append('\t')
            .Append("0x").Append(entry.Hash.ToString("X8")).Append('\t')
            .Append(entry.Name.Replace('\t', ' ')).AppendLine();
    File.WriteAllText(Path.Combine(dest, "names.tsv"), sb.ToString());
    Console.WriteLine($"bins  names.bin entries={names.Entries.Count} from {path}");
}

static void DumpGameBinFamily(
    string dest,
    string family,
    string path,
    GameBin bin,
    bool parseFrontend,
    bool parseScript)
{
    var dir = Path.Combine(dest, family);
    Directory.CreateDirectory(dir);
    var tsv = new StringBuilder();
    tsv.AppendLine("index\ttype\tinstance\tsource\tmesh\traw\tsubdefs\tstrings");
    var index = new StringBuilder();
    index.AppendLine($"# {family}.bin");
    index.AppendLine();
    index.AppendLine($"file `{path}` · entries **{bin.Entries.Count}** · chunks **{bin.Chunks.Count}** · useNames **{bin.UseNamesBin}** · plat `0x{bin.PlatformIndicator:X8}`.");
    index.AppendLine();
    index.AppendLine("| type | count |");
    index.AppendLine("|---|---|");
    foreach (var group in bin.Entries.GroupBy(e => e.TypeName ?? "?").OrderByDescending(g => g.Count()))
        index.AppendLine($"| `{group.Key}` | {group.Count()} |");
    index.AppendLine();
    index.AppendLine("[entries.tsv](entries.tsv)");
    index.AppendLine();

    var writeParts = parseFrontend || parseScript;
    if (writeParts)
        index.AppendLine("| # | instance | type | raw | file |");
    if (writeParts)
        index.AppendLine("|---|---|---|---|---|");

    foreach (var entry in bin.Entries)
    {
        var type = entry.TypeName ?? "";
        var inst = entry.InstanceName ?? "";
        var strings = ExtractAscii(entry.Raw);
        tsv.Append(entry.Index).Append('\t')
            .Append(type.Replace('\t', ' ')).Append('\t')
            .Append(inst.Replace('\t', ' ')).Append('\t')
            .Append((entry.SourceName ?? "").Replace('\t', ' ')).Append('\t')
            .Append(entry.MeshId?.ToString() ?? "").Append('\t')
            .Append(entry.Raw.Length).Append('\t')
            .Append(entry.SubDefs.Count).Append('\t')
            .Append(string.Join('|', strings.Take(12)).Replace('\t', ' ')).AppendLine();

        if (!writeParts)
            continue;

        var slug = Slug($"{entry.Index:D4}-{inst}");
        var body = new StringBuilder();
        body.AppendLine($"# {inst}");
        body.AppendLine();
        body.AppendLine($"type `{type}` · index **{entry.Index}** · raw **{entry.Raw.Length}** · source `{entry.SourceName}` · mesh `{entry.MeshId}`.");
        body.AppendLine();
        if (entry.SubDefs.Count > 0)
        {
            body.AppendLine("subdefs:");
            foreach (var sub in entry.SubDefs)
            {
                var child = (uint)sub.DefIndex < (uint)bin.Entries.Count ? bin.Entries[sub.DefIndex] : null;
                body.AppendLine($"- crc `0x{sub.NameCrc:X8}` idx **{sub.DefIndex}** → `{child?.TypeName}` `{child?.InstanceName}`");
            }

            body.AppendLine();
        }

        if (parseFrontend && FrontendUiDef.TryParse(entry) is { } ui)
        {
            body.AppendLine("## UI persist");
            body.AppendLine();
            body.AppendLine($"- type **{ui.Type}** layer **{ui.Layer}**");
            body.AppendLine($"- size {ui.Width}×{ui.Height} pos ({ui.PositionX},{ui.PositionY}) angle {ui.Angle}");
            body.AppendLine($"- zoom ({ui.ZoomX},{ui.ZoomY}) centre **{ui.Center}** absolute **{ui.Absolute}**");
            body.AppendLine($"- graphic `{ui.GraphicId}` bank `{ui.GraphicBankId}` sprites **{ui.Sprites}** states **{ui.States}**");
            body.AppendLine($"- font `{ui.Font}` text `{ui.TextTag}` message **{ui.MessageId}**");
            body.AppendLine($"- colour ({ui.ColourR},{ui.ColourG},{ui.ColourB},{ui.ColourA}) haveA **{ui.HaveColourA}**");
            body.AppendLine($"- plus96 **{ui.Plus96}** plus224 **{ui.Plus224}** plus322 **{ui.Plus322}** plus326 **{ui.Plus326}**");
            body.AppendLine($"- plus392 **{ui.Plus392}** plus504 **{ui.Plus504}** plus508 **{ui.Plus508}**");
            body.AppendLine($"- scale size **{ui.ScaleSizeToViewport}**/{ui.ScaleSizeByte} origin **{ui.ScaleOriginToViewport}**/{ui.ScaleOriginByte}");
            body.AppendLine($"- partial **{ui.Partial}** unreadOffset **{ui.UnreadOffset}**");
            if (ui.ChildIndices.Count > 0)
                body.AppendLine("- children: " + string.Join(", ", ui.ChildIndices.Select(i => $"`{i}`")));
            if (ui.SpriteDefIndices.Count > 0)
                body.AppendLine("- sprite defs: " + string.Join(", ", ui.SpriteDefIndices.Zip(ui.SpriteKeys, (d, k) => $"`{k}:{d}`")));
            if (ui.UnreadCrcs.Count > 0)
                body.AppendLine("- unread crcs: " + string.Join(", ", ui.UnreadCrcs.Select(c => $"`0x{c:X8}`")));
            body.AppendLine();
        }

        if (parseScript)
        {
            var script = ScriptBank.FromEntry(entry);
            if (script.CommandsLayoutProven)
            {
                body.AppendLine("## cutscene vectors");
                body.AppendLine();
                for (var v = 0; v < script.Vectors.Count; v++)
                {
                    body.AppendLine($"### vector {v} ({script.Vectors[v].Count})");
                    body.AppendLine();
                    foreach (var line in script.Vectors[v])
                        body.AppendLine($"- `{line}`");
                    body.AppendLine();
                }
            }
        }

        if (strings.Count > 0)
        {
            body.AppendLine("## strings");
            body.AppendLine();
            foreach (var s in strings)
                body.AppendLine($"- `{s}`");
            body.AppendLine();
        }

        body.AppendLine("## raw");
        body.AppendLine();
        body.AppendLine("```");
        body.Append(HexDump(entry.Raw));
        body.AppendLine("```");
        File.WriteAllText(Path.Combine(dir, slug + ".md"), body.ToString());
        index.AppendLine($"| {entry.Index} | `{inst}` | `{type}` | {entry.Raw.Length} | [{slug}.md]({slug}.md) |");
    }

    File.WriteAllText(Path.Combine(dir, "entries.tsv"), tsv.ToString());
    File.WriteAllText(Path.Combine(dir, "INDEX.md"), index.ToString());
    Console.WriteLine($"bins  {family}.bin entries={bin.Entries.Count} parts={(writeParts ? bin.Entries.Count : 0)}");
}

static void DumpLooseBins(string dest, GameInstall install)
{
    var other = Path.Combine(dest, "other");
    Directory.CreateDirectory(other);
    var roots = new[]
    {
        Path.Combine(install.DataRoot, "CompiledDefs"),
        Path.Combine(install.DataRoot, "Defs"),
        Path.Combine(install.DataRoot, "Misc"),
    };
    var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach (var known in new[] { "frontend.bin", "script.bin", "game.bin", "names.bin" })
        seen.Add(known);

    var list = new StringBuilder();
    list.AppendLine("# other .bin");
    list.AppendLine();
    foreach (var root in roots)
    {
        if (!Directory.Exists(root))
            continue;
        foreach (var file in Directory.EnumerateFiles(root, "*.bin"))
        {
            var name = Path.GetFileName(file);
            if (!seen.Add(name))
                continue;
            var bytes = File.ReadAllBytes(file);
            var slug = Slug(Path.GetFileNameWithoutExtension(name));
            var body = new StringBuilder();
            body.AppendLine($"# {name}");
            body.AppendLine();
            body.AppendLine($"path `{file}` · bytes **{bytes.Length}**.");
            body.AppendLine();
            body.AppendLine("```");
            body.Append(HexDump(bytes, max: 4096));
            body.AppendLine("```");
            File.WriteAllText(Path.Combine(other, slug + ".md"), body.ToString());
            list.AppendLine($"- [{name}]({slug}.md) **{bytes.Length}** `{file}`");
            Console.WriteLine($"bins  other {name} {bytes.Length}");
        }
    }

    File.WriteAllText(Path.Combine(other, "INDEX.md"), list.ToString());
}

static void WriteCompiledDefsIndex(string dest, GameInstall install, NamesBin names)
{
    var sb = new StringBuilder();
    sb.AppendLine("# compiled-defs");
    sb.AppendLine();
    sb.AppendLine($"install `{install.Root}` · names **{names.Entries.Count}**.");
    sb.AppendLine();
    sb.AppendLine("- [names.tsv](names.tsv)");
    sb.AppendLine("- [frontend/INDEX.md](frontend/INDEX.md)");
    sb.AppendLine("- [script/INDEX.md](script/INDEX.md)");
    sb.AppendLine("- [game/INDEX.md](game/INDEX.md)");
    sb.AppendLine("- [other/INDEX.md](other/INDEX.md)");
    File.WriteAllText(Path.Combine(dest, "INDEX.md"), sb.ToString());
}

static List<string> ExtractAscii(byte[] raw)
{
    var list = new List<string>();
    var i = 0;
    while (i < raw.Length)
    {
        if (raw[i] is < 32 or > 126)
        {
            i++;
            continue;
        }

        var start = i;
        while (i < raw.Length && raw[i] is >= 32 and <= 126)
            i++;
        if (i - start >= 4)
            list.Add(System.Text.Encoding.ASCII.GetString(raw, start, i - start));
    }

    return list;
}

static string HexDump(byte[] raw, int max = int.MaxValue)
{
    var n = Math.Min(raw.Length, max);
    var sb = new StringBuilder((n / 16 + 1) * 72);
    for (var i = 0; i < n; i += 16)
    {
        sb.Append($"{i:X4}  ");
        for (var j = 0; j < 16 && i + j < n; j++)
            sb.Append($"{raw[i + j]:X2} ");
        sb.AppendLine();
    }

    if (n < raw.Length)
        sb.AppendLine($"… {raw.Length - n} more bytes");
    return sb.ToString();
}

static string Slug(string name)
{
    var chars = name.ToCharArray();
    for (var i = 0; i < chars.Length; i++)
    {
        if (chars[i] is < '0' or > 'z' || (chars[i] > '9' && chars[i] < 'A') || (chars[i] > 'Z' && chars[i] < 'a'))
            chars[i] = chars[i] is '-' or '_' or '.' ? chars[i] : '-';
    }

    var s = new string(chars).Trim('-');
    return s.Length == 0 ? "unnamed" : s.Length > 80 ? s[..80] : s;
}
