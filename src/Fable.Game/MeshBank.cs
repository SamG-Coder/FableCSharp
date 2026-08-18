using Fable.Core;
using Fable.Formats.Anims;
using Fable.Formats.Banks;
using Fable.Formats.Meshes;

namespace Fable.Game;

/// <summary>
/// <c>0049E620</c> "Opening Mesh Bank"
/// <c>MBANK_ALLMESHES</c> via <c>00A09F20</c>.
/// Miss constructs <c>00A27030</c> size
/// <c>0x460</c> then <c>[bank].vtbl+4</c>
/// opens the named BIG directory.
/// <c>004BBFD0</c> stores the handle at
/// <c>[0x13B8A04]</c>.
/// <c>009AD410</c> is a later id lookup —
/// it does not parse C3Ds.
/// </summary>
public sealed class MeshBank : IDisposable
{
    public const uint OpenFn = 0x0049E620;
    public const uint LookupFn = 0x00A09F20;
    public const uint ObjectCtor = 0x00A27030;
    public const uint ObjectVtbl = 0x0129CE94;
    /// <summary>
    /// <c>00A09F20</c> miss: <c>[bank].vtbl+4</c>
    /// is <c>009D56C0</c> ("Open Bank File
    /// Async") then <c>009A7F80</c> on
    /// <c>[0x13CA79C]</c>.
    /// </summary>
    public const uint OpenVtbl4 = 0x009D56C0;
    public const uint OpenBankFileAsync = 0x009A7F80;
    public const uint OpenBankTableVa = 0x013CA79C;
    public const int ObjectSize = 0x460;
    public const uint SetGlobalFn = 0x004BBFD0;
    public const uint GlobalVa = 0x013B8A04;
    public const uint DefLookupFn = 0x009AD410;
    public const string BankName = "MBANK_ALLMESHES";

    public bool Opened { get; private set; }
    public int EntryCount { get; private set; }
    /// <summary>
    /// C3Ds actually parsed by <see cref="Get"/>.
    /// Directory open does not increment this.
    /// </summary>
    public int ParsedCount => _parsed.Count;

    private BigArchive? _big;
    private Dictionary<uint, BankEntry>? _byId;
    private readonly Dictionary<uint, MeshFile?> _parsed = [];
    private readonly Dictionary<uint, XSeqFile?> _anims = [];

    public void Open(GameInstall install)
    {
        if (Opened)
            return;
        var path = Path.Combine(install.DataRoot, "graphics", "graphics.big");
        if (!File.Exists(path))
            return;
        _big = BigArchive.Open(path);
        var bank = _big.SubBanks.First(item =>
            item.Name.Contains("MESH", StringComparison.OrdinalIgnoreCase));
        var entries = _big.ReadEntries(bank);
        _byId = entries
            .Where(entry => entry.Type is not 3)
            .GroupBy(entry => entry.Id)
            .ToDictionary(group => group.Key, group => group.First());
        EntryCount = _byId.Count;
        Opened = true;
    }

    public bool TryGetEntry(uint id, out BankEntry entry)
    {
        if (_byId is not null && _byId.TryGetValue(id, out var hit))
        {
            entry = hit;
            return true;
        }

        entry = default!;
        return false;
    }

    /// <summary>
    /// On-demand <c>009AD410</c> then parse.
    /// Not a load-time walk of every id.
    /// </summary>
    public MeshFile? Get(uint id)
    {
        if (_parsed.TryGetValue(id, out var hit))
            return hit;
        if (_big is null || _byId is null || !_byId.TryGetValue(id, out var entry))
        {
            _parsed[id] = null;
            return null;
        }

        var mesh = MeshFile.TryParse(_big.Read(entry), (int)entry.Type);
        _parsed[id] = mesh;
        return mesh;
    }

    /// <summary>
    /// Type-6 <c>3DAF</c>/<c>XSEQ</c> via
    /// <c>00A999B0</c> / <c>00AA4680</c>.
    /// Same bank as C3D; not parsed at open.
    /// </summary>
    public XSeqFile? GetAnim(uint id)
    {
        if (_anims.TryGetValue(id, out var hit))
            return hit;
        if (_big is null || _byId is null || !_byId.TryGetValue(id, out var entry))
        {
            _anims[id] = null;
            return null;
        }

        var clip = XSeqFile.TryParse(_big.Read(entry), entry.Name);
        _anims[id] = clip;
        return clip;
    }

    public XSeqFile? FindAnim(string name)
    {
        if (_big is null || _byId is null)
            return null;
        foreach (var (id, entry) in _byId)
        {
            if (entry.Type != 6)
                continue;
            if (entry.Name.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                entry.Name.EndsWith(name, StringComparison.OrdinalIgnoreCase))
                return GetAnim(id);
        }

        return null;
    }

    public void Dispose()
    {
        _big?.Dispose();
        _big = null;
        _byId = null;
        _parsed.Clear();
        _anims.Clear();
        Opened = false;
        EntryCount = 0;
    }
}
