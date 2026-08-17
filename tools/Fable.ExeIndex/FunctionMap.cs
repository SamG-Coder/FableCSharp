using System.Text;

namespace Fable.ExeIndex;

/// <summary>
/// Walks every function reachable from New Game / StartOakVale seeds.
/// Does not seed Lookout or later campaign maps.
/// </summary>
internal static class FunctionMap
{
    public const int MaxDepth = 8;
    public const int MaxFunctions = 16000;
    public const int MaxInsns = 2500;

    /// <summary>
    /// Code windows used by New Game / StartOakVale first scene only.
    /// Menu-wide frontend and later-town ranges are omitted.
    /// </summary>
    public static readonly (uint Lo, uint Hi, string Name)[] NewGameRanges =
    [
        (0x00595B00, 0x00595D00, "UI-new-game"),
        (0x004B5000, 0x004B5200, "start-new-quest"),
        (0x00489D00, 0x0048A200, "CreateCharacter"),
        (0x004FD000, 0x004FE000, "WLD-region"),
        (0x006A5900, 0x006AD200, "PlayerCreature"),
        (0x004C7000, 0x004CD200, "Thing-type-activate"),
        (0x00522000, 0x00523100, "Thing-type-registrar"),
        (0x00529600, 0x0052AE00, "CThingBuilding"),
        (0x0072DF00, 0x0072E500, "CREATEBUILDING"),
        (0x007E12F0, 0x007E1B00, "CMultiStaticMeshDef"),
        (0x006E0880, 0x006E0960, "particle-emitter-create"),
        (0x004E1500, 0x004E3200, "CMultiStaticMeshDef-factory"),
        (0x004EB800, 0x004EB960, "CMultiStatic-entry"),
        (0x004EDE00, 0x004EE200, "CMultiStatic-vector-persist"),
        (0x00433140, 0x00433190, "persist-u8"),
        (0x004735D0, 0x00473680, "persist-tail"),
        (0x0077BA40, 0x0077BC80, "single-mesh-skip-global"),
        (0x006BF600, 0x006C1D00, "CTCBuyableHouse"),
        (0x0082E0E0, 0x0082E130, "inside-building"),
        (0x00886400, 0x00888600, "theme-slot-star"),
        (0x00CDD400, 0x00CDD500, "WatchBarrels"),
        (0x00DABA00, 0x00DAC2B0, "intro-parent-NOVI"),
        (0x00DBE890, 0x00DBEB20, "WatchBarrels-callback"),
        (0x00DBEF70, 0x00DBF200, "S-QNOVI"),
        (0x00DAAC00, 0x00DABAC0, "S-QNOVI-ctor"),
        (0x00DAACE0, 0x00DAAE00, "S-QNOVI-vtbl1"),
        (0x00B25900, 0x00B25B00, "render-frame"),
        (0x00CD0800, 0x00CD0A80, "fade-opcodes"),
        (0x00CB5A00, 0x00CB5D00, "quest-script-bind"),
        (0x00CB5D80, 0x00CB5E80, "registering-scripts"),
        (0x00A44700, 0x00A44A00, "microthread"),
        (0x00CB7E00, 0x00CB8280, "quest-watcher-register"),
        (0x00CBF280, 0x00CC1A00, "script-camera-anim"),
        (0x00CC4B00, 0x00CC5300, "script-fade"),
        (0x00903570, 0x00903680, "CActionPlayAnimation"),
        (0x004C7470, 0x004C74B0, "thing-play-anim-vtbl72"),
        (0x0070B3C0, 0x0070B800, "CTCAnimationComplex"),
        (0x0070C050, 0x0070C0A0, "anim-play-request"),
        (0x0070D580, 0x0070D780, "anim-play-inner"),
        (0x005B37F0, 0x005B3B00, "appearance-DEFAULT-play"),
        (0x004CE500, 0x004CE800, "CAM-SHOT-parser"),
        (0x0089FA00, 0x0089FC00, "MARKER-LIGHT"),
        (0x00988000, 0x0098C000, "VS-wrapper"),
        (0x0098D400, 0x0098D800, "inner-vs-ctor"),
        (0x0099AE00, 0x0099B000, "cstring-ctor"),
        (0x00430800, 0x00431200, "environment-persist"),
        (0x00B23800, 0x00B24000, "water-name-setter"),
        (0x00B71F00, 0x00B72200, "water-prepare"),
        (0x00BF4400, 0x00BF4580, "water-enqueue"),
        (0x00B2FC00, 0x00B31D00, "frustum-extract"),
        (0x00A04600, 0x00A04C00, "d3d-fog-slots"),
        (0x00A5B800, 0x00A5B880, "sse-detect"),
        (0x00B32000, 0x00B34000, "MainScene-prims"),
        (0x00B84000, 0x00B84C00, "prim-queue-drain"),
        (0x00B91000, 0x00B91400, "palskin-drain-unwrap"),
        (0x00B41000, 0x00B4B000, "maps-lighting"),
        (0x00B54000, 0x00B56800, "camera-constant-upload"),
        (0x00A89400, 0x00A89600, "c3d-mesh-serialize"),
        (0x00A9E000, 0x00AA0B00, "bone-hierarchy"),
        (0x00ABF600, 0x00ABF800, "c3d-material-serialize"),
        (0x00B26000, 0x00B27000, "engine-video-options"),
        (0x01224800, 0x01224900, "sky-uv-divisor-crt"),
        (0x00B61B00, 0x00B63200, "sky-dome"),
        (0x00B63200, 0x00B66000, "sky-stars"),
        (0x0099E400, 0x0099F000, "nstring"),
        (0x00B66000, 0x00B7F000, "landscape-water"),
        (0x00B89C00, 0x00BDB000, "static-palskin"),
        (0x00BE9100, 0x00BE9500, "sea-mesh-builder"),
        (0x00BDB000, 0x00BDC800, "LayoutLights"),
        (0x00BF4000, 0x00BF6000, "per-cell"),
        (0x00BF6E00, 0x00BF7200, "patch-aabb-fill"),
        (0x00BDA000, 0x00BDB000, "landscape-vb"),
        (0x00BFDE00, 0x00BFF000, "tile-expand"),
        (0x00A63100, 0x00A63200, "create-vb-wrapper"),
        (0x00DBDE00, 0x00DBF000, "StartOakVale"),
        (0x00DB8500, 0x00DB9800, "oakvale-intro-father"),
        (0x00CBFB70, 0x00CC0000, "cutscene-runner-head"),
        (0x00CC9F00, 0x00CCA220, "UseCamera-activate"),
        (0x00CBFDC0, 0x00CBFE50, "cutscene-FadeOut-0.5"),
        (0x00CCA260, 0x00CCA400, "PlayAVI"),
        (0x0088F890, 0x0088F8D0, "PlayAVI-vtbl1476"),
        (0x00A3B120, 0x00A3B1C0, "PlayAVI-run"),
        (0x00A3B500, 0x00A3B720, "PlayAVI-renderer"),
        (0x00A3B9D0, 0x00A3BC20, "PlayAVI-open"),
        (0x0089B780, 0x0089BB30, "Teleport-apply"),
        (0x004AA980, 0x004AA9A0, "Teleport-marker-pos"),
        (0x00CC9E50, 0x00CC9F20, "NoLoadUseCamera"),
        (0x00CBF7F0, 0x00CBF980, "PlayMusic-helper"),
        (0x00CC8E90, 0x00CC8F40, "PlayMusic-interpreter"),
        (0x00CC4670, 0x00CC4810, "Teleport-token"),
        (0x00CC3B30, 0x00CC3CF0, "LookToThing-token"),
        (0x00CC7070, 0x00CC7130, "actor-command-join"),
        (0x00CBEE00, 0x00CBEE60, "IsFalse-arg"),
        (0x00DAC2B0, 0x00DAC360, "NOVI-LiveFather-factory"),
        (0x00CB8230, 0x00CB8B40, "NOVI-name-bind"),
        (0x004C7CF0, 0x004C7D50, "thing-script-activate"),
        (0x004C97B0, 0x004C9830, "thing-construct-bind"),
        (0x004AFA60, 0x004AFBB0, "thing-script-match"),
    ];

    public static readonly (string Name, uint Va)[] NewGameSeeds =
    [
        ("UI TEXT NEW GAME", 0x00595B24),
        ("UI FRONTEND MAIN MENU", 0x0059899A),
        ("START NEW QUEST", 0x004B5080),
        ("StartOakVale", 0x00DBDE40),
        ("HerosOldHouse tail", 0x00DBE0C6),
        ("hero-exists", 0x00CB7940),
        ("CThingPlayerCreature Create", 0x006AC910),
        ("CPlayer CreateCharacter", 0x00489D40),
        ("ConstructFromParams", 0x006A9DD0),
        ("Thing construct", 0x006A5950),
        ("THING TYPE BUILDING name", 0x004C75B0),
        ("Thing type registrar", 0x00522A20),
        ("CThingBuilding factory", 0x0052AC10),
        ("CThingBuilding base ctor", 0x005296B0),
        ("CThing parent ctor", 0x004C9030),
        ("CThingBuilding vtbl3 params", 0x006A5AF0),
        ("CREATEBUILDING script", 0x0072E290),
        ("CREATEBUILDING body", 0x0072DF50),
        ("CMultiStaticMeshDef name", 0x007E12F0),
        ("CMultiStaticMeshDef lookup", 0x007E1400),
        ("CMultiStaticMeshDef ctor", 0x007E14C0),
        ("CMultiStaticMeshDef apply", 0x007E15C0),
        ("CMultiStaticMeshDef factory", 0x004E31FA),
        ("CMultiStaticMeshDef persist ctor", 0x004E1516),
        ("CMultiStatic persist this+40", 0x004EDE1B),
        ("CMultiStatic vector persist", 0x004EDE2B),
        ("CMultiStatic entry persist", 0x004EB8C3),
        ("skip-global other apply", 0x0077BA40),
        ("Default float 004BC180", 0x004BC180),
        ("LayoutRepeatedMesh ctor", 0x00BDB080),
        ("CTC multi-static name", 0x007E1A80),
        ("PALSKIN dest x87 00BD2F91", 0x00BD2F91),
        ("PALSKIN hierarchy 00AA0090", 0x00AA0090),
        ("SSE detect CPUID 00A5B850", 0x00A5B850),
        ("CTCBuyableHouse ctor", 0x006BF8A0),
        ("CTCBuyableHouse construct", 0x006C14D0),
        ("CTCBuyableHouse ready", 0x006BFB90),
        ("CTCBuyableHouse window swap", 0x006C0F00),
        ("Inside-building predicate", 0x0082E0E0),
        ("CBuyableHouseDef lookup", 0x006C1B00),
        ("Theme slot ctor zeros +424", 0x008864A0),
        ("Theme slot copy dest+424", 0x008865C0),
        ("WatchBarrels ctor", 0x00CDD450),
        ("WatchBarrels callback", 0x00DBE890),
        ("intro parent NOVI", 0x00DABAC0),
        ("S_QNOVI entry", 0x00DBEF70),
        ("S_QNOVI ctor", 0x00DAAC00),
        ("S_QNOVI vtbl", 0x012D7A28),
        ("S_QNOVI vtbl1", 0x00DAACE0),
        ("script camera hooks", 0x00CBF29F),
        ("CS_OAKVALE_INTRO_FATHER start", 0x00DB86B0),
        ("cutscene runner", 0x00CBFB7D),
        ("cutscene FadeOut 0.5", 0x00CBFDD0),
        ("PlayAVI", 0x00CCA26D),
        ("PlayAVI apply", 0x00CCA2BD),
        ("PlayAVI vtbl+1476", 0x0088F890),
        ("PlayAVI singleton", 0x0040D2A0),
        ("PlayAVI player", 0x006286F0),
        ("PlayAVI open", 0x00A3B9D0),
        ("PlayAVI rewrite", 0x0099C1E0),
        ("PlayAVI ctor", 0x00A3BC70),
        ("PlayAVI renderer ctor", 0x00A3B510),
        ("PlayAVI CheckMediaType", 0x00A3B5F0),
        ("PlayAVI Run", 0x00A3B130),
        ("PlayAVI DoRenderSample", 0x00A3BCF0),
        ("PlayAVI copy sample", 0x00A3B740),
        ("PlayAVI LockRect", 0x009FA450),
        ("PlayAVI event pump", 0x00A3B000),
        ("PlayAVI blit", 0x009DC870),
        ("PlayAVI flush", 0x009D9C80),
        ("PlayAVI FilterGraph CLSID", 0x012AB174),
        ("PlayAVI IGraphBuilder IID", 0x012A9934),
        ("command continue join", 0x00CD17F8),
        ("MuteSounds token", 0x00CC7258),
        ("NoLoadUseCamera", 0x00CC9E6A),
        ("PlayMusic helper", 0x00CBF7FE),
        ("intro-father dtor", 0x00DB8680),
        ("NOVI_LiveFather factory", 0x00DAC2C0),
        ("NOVI name register", 0x00CB8230),
        ("construct name bind", 0x00CB8960),
        ("thing construct bind", 0x004C97B0),
        ("thing script activate", 0x004C7CF0),
        ("Registering Scripts", 0x00CB5D80),
        ("CActionPlayAnimation", 0x00903570),
        ("DialogSpeak token", 0x00CC3165),
        ("dialog begin vtbl+1456", 0x008906C0),
        ("dialog wait vtbl+1472", 0x008907D0),
        ("dialog wait body", 0x006E5660),
        ("WaitTask token", 0x00CC0783),
        ("WaitTask hero poll", 0x006A9550),
        ("SneakTo token", 0x00CC0CB5),
        ("SneakTo thing vtbl+20 stub", 0x004C72B0),
        ("SneakTo wait poll", 0x00CC0F1A),
        ("PlayCombatAnim token", 0x00CC15E3),
        ("PlayCombatAnim Father vtbl+76", 0x00834760),
        ("CActionPlayCombatAnimation", 0x009035F0),
        ("Create token", 0x00CCC246),
        ("Create vtbl+364", 0x008A9100),
        ("WalkTo token", 0x00CC083D),
        ("WaitActiveDialog token", 0x00CC656B),
        ("Remove token", 0x00CD0116),
        ("Remove vtbl+432", 0x008910D0),
        ("Remove inner", 0x004C9B80),
        ("DialogadSpeak token", 0x00CC3354),
        ("DialogadSpeak table", 0x00CD3187),
        ("DialogadSpeak miss join", 0x00CC2C6B),
        ("LookInDirection token", 0x00CC3F73),
        ("LookInDirection apply vtbl+1896", 0x0089BDF0),
        ("cutscene skip predicate", 0x00CBEB7E),
        ("cutscene vector1 copy", 0x00CC017C),
        ("cutscene skip vtbl+168", 0x00894440),
        ("cutscene skip vtbl+176", 0x00893B00),
        ("PlayAnimation thing vtbl+72", 0x004C7470),
        ("CTCAnimationComplex factory", 0x0070B3F0),
        ("CTCAnimationComplex +68 stub", 0x00686920),
        ("CTCAnimationComplex inner play", 0x0070D580),
        ("appearance DEFAULT play", 0x005B37F7),
        ("render frame", 0x00B25950),
        ("StayFadedOut", 0x00CD087E),
        ("FadeOut opcode", 0x00CD096F),
        ("FadeIn vtbl+1496", 0x0088E4C0),
        ("fade overlay draw", 0x006496BC),
        ("fade overlay alpha", 0x004348D0),
        ("fade overlay tick", 0x00434870),
        ("fade overlay record", 0x0041BEB0),
        ("Teleport token", 0x00CC4678),
        ("Teleport apply vtbl+1892", 0x0089B780),
        ("Teleport marker pos", 0x004AA980),
        ("Teleport marker yaw", 0x004AAA40),
        ("Teleport heading apply", 0x0089BDF0),
        ("cutscene actor bind", 0x00CD3D2E),
        ("cutscene actor slot", 0x008ABD10),
        ("LookToThing token", 0x00CC3B3F),
        ("IsFalse arg", 0x00CBEE0C),
        ("actor command join", 0x00CC707C),
        ("DoScriptFrame token", 0x00CC7085),
        ("DoScriptFrame wait", 0x00CC70D5),
        ("CString atoi", 0x0099E7F0),
        ("DoCameraPreloading token", 0x00CC86D0),
        ("IsTrue arg", 0x00CBEDBA),
        ("quest script bind", 0x00CB5C90),
        ("bind camera source", 0x00B23B50),
        ("Component add by name", 0x004C9D60),
        ("NONE primitive pass", 0x00B89C30),
        ("NONE-draw layer", 0x00BBE090),
        ("Primitive layer switch", 0x00BBC130),
        ("Static primitive submit", 0x00BBC460),
        ("LayoutLights ctor", 0x00BDB400),
        ("lighting ctor", 0x00B482A0),
        ("TOD blend", 0x00B46C80),
        ("c35 flush", 0x0098A760),
        ("c35 setter", 0x0098B2C0),
        ("PALSKIN register upload", 0x009896D0),
        ("SetVSConstantF wrapper", 0x00989A60),
        ("OpenStaticMaps", 0x00B42750),
        ("LoadWaterData", 0x00B41FA0),
        ("landscape draw", 0x00B6B0B0),
        ("patch frustum AABB", 0x00BDC2D0),
        ("patch AABB fill", 0x00BF6F80),
        ("patch AABB setup", 0x00BDC180),
        ("tessellator ctor", 0x00BF6E20),
        ("frustum extract", 0x00B2FD60),
        ("camera setup FOV inverse", 0x00B30B50),
        ("camera constant upload c2", 0x00B54310),
        ("c4 inverse row2 00B545D5", 0x00B545D5),
        ("mesh draw 00B555A0", 0x00B555A0),
        ("FOGENABLE setter", 0x00B46890),
        ("landscape setup FOGENABLE", 0x00B67480),
        ("MainScene FOGENABLE bits", 0x00B32AD0),
        ("SetVSConstantF 4float", 0x00989B00),
        ("fog compute 00B47630", 0x00B47630),
        ("fog colour setter", 0x009886C0),
        ("fog colour flush c18", 0x009897C0),
        ("fog plane setter +880", 0x00988600),
        ("LayoutBasic fog +56", 0x00BDBB70),
        ("LayoutBasic flush c0 c1", 0x00989BF0),
        ("PALSKIN default draw", 0x00BD549D),
        ("lighting record alloc", 0x00B4A4C0),
        ("camera update helper FOV", 0x00B314E0),
        ("extract other writes view", 0x00B2FC50),
        ("sky draw 00B662F0", 0x00B662F0),
        ("sky dome setup 00B620A0", 0x00B620A0),
        ("sky dome fill 00B61DD0", 0x00B61DD0),
        ("sky float-to-int 00BFEA70", 0x00BFEA70),
        ("sky ctor this+16 00B627E2", 0x00B627E2),
        ("ENGINE_VIDEO_OPTIONS lookup", 0x00B2640F),
        ("ENVIRONMENT lookup 00B26828", 0x00B26828),
        ("ENVIRONMENT CString ctor 0099AED0", 0x0099AED0),
        ("ENVIRONMENT string persist 004310A7", 0x004310A7),
        ("sky UV divisor CRT 01224830", 0x01224830),
        ("sky star draw 00B65A20", 0x00B65A20),
        ("sky weather draw 00B64FA0", 0x00B64FA0),
        ("sky weather all-zero ret", 0x00B659A5),
        ("map manager +408 setter", 0x00B42ED0),
        ("sky gather texture ids 00B63800", 0x00B63800),
        ("sky inner PS bind 00B62BA8", 0x00B62BA8),
        ("sky inner SIMPLE bind 00B62BB6", 0x00B62BB6),
        ("sky inner FULL bind 00B62C2D", 0x00B62C2D),
        ("ENVIRONMENT NString persist", 0x00431143),
        ("NString ctor zeros 0099E4B0", 0x0099E4B0),
        ("sky cap fill 00B61B30", 0x00B61B30),
        ("sky skirt fill 00B61CD0", 0x00B61CD0),
        ("sky mesh draw 00B66190", 0x00B66190),
        ("sky inner 00B640E0", 0x00B640E0),
        ("sky outer 00B63C00", 0x00B63C00),
        ("per-cell world fill", 0x00BF46A2),
        ("view copy 3x4", 0x00988350),
        ("world copy 3x4", 0x009881F0),
        ("proj copy 4x4", 0x00988540),
        ("WVP flush 00988A50", 0x00988A50),
        ("proj builder 009883F0", 0x009883F0),
        ("view copy 00988350", 0x00988350),
        ("world copy 009881F0", 0x009881F0),
        ("world identity 00988290", 0x00988290),
        ("proj copy 00988540", 0x00988540),
        ("bind camera source", 0x00B23B50),
        ("store camera helper +12", 0x00B2FBF0),
        ("camera spline update", 0x00B31160),
        ("camera ctor", 0x00B31700),
        ("spline enable +536", 0x00B2FC10),
        ("FOV flag getter", 0x00A0BE80),
        ("frustum extract other", 0x00B2FC50),
        ("camera copy 00B4AF50", 0x00B4AF50),
        ("per-cell submit", 0x00BF4570),
        ("per-cell c1 flip", 0x00BF5175),
        ("tile expand 00BFE050", 0x00BFE050),
        ("per-cell edi=2 00BF4EB7", 0x00BF4EB7),
        ("SetVSConstantF1 00989A60", 0x00989A60),
        ("inner VS ctor 0098D4A0", 0x0098D4A0),
        ("LayoutRepeatedMesh ctor", 0x00BDB080),
        ("unpack tile normal 00BFDEC0", 0x00BFDEC0),
        ("create landscape VB 00BDA3D0", 0x00BDA3D0),
        ("CreateVertexBuffer wrapper", 0x00A63150),
        ("water draw", 0x00B783F0),
        ("water prepare vtbl+4", 0x00B71FB0),
        ("water type-4 enqueue", 0x00BF44B3),
        ("water type-8 ingest", 0x00B6DAF0),
        ("sea bind 00B6DC40", 0x00B6DC40),
        ("water rebuild vtbl1", 0x00B71FB0),
        ("water +636 setter", 0x00B23F00),
        ("sea mesh copy", 0x00B6D420),
        ("sea mesh builder", 0x00BE91E0),
        ("C3D material serialize", 0x00ABF6B0),
        ("PALSKIN helper ctor", 0x00BCE740),
        ("prim queue drain", 0x00B849F0),
        ("PALSKIN drain vtbl20", 0x00BD7110),
        ("PALSKIN drain vtbl24", 0x00B91340),
        ("MainScene plus616 draw", 0x00B33010),
        ("static VS bind", 0x00B8B660),
        ("static-lit draw FVF", 0x00BB2540),
        ("static-lit caller", 0x00BB30A0),
        ("static compact ctor", 0x00B8B630),
        ("SetVertexShader wrapper", 0x00988020),
        ("Attach PS record", 0x00988140),
        ("PS const wrapper ctor", 0x0098ACF0),
        ("PSCONST slot assign", 0x0098DB20),
        ("PARTICLE_EMITTER_NORMAL create", 0x006E0880),
        ("PALSKIN VS bind", 0x00BD01B8),
        ("PALSKIN bone pack", 0x00BD2D90),
        ("PALSKIN bind switch", 0x00BD3070),
        ("PALSKIN draw entry", 0x00BD71B0),
        ("MARKER LIGHT", 0x0089FAA8),
        ("CAM intro writer", 0x004FD040),
    ];

    public sealed class Node
    {
        public required uint Va { get; init; }
        public required int Depth { get; init; }
        public required string Seed { get; init; }
        public required int Insns { get; init; }
        public required IReadOnlyList<uint> Calls { get; init; }
        public required IReadOnlyList<string> Strings { get; init; }
    }

    public static bool InNewGameRange(uint va)
    {
        foreach (var (lo, hi, _) in NewGameRanges)
        {
            if (va >= lo && va < hi)
                return true;
        }

        return false;
    }

    public static List<Node> WalkNewGame(PeImage pe)
    {
        var queue = new Queue<(uint Va, int Depth, string Seed)>();
        foreach (var (name, va) in NewGameSeeds)
        {
            var file = pe.FileOffset(va);
            if (file < 0)
                continue;
            var start = pe.Va(X86.FindPrologue(pe, file));
            queue.Enqueue((start, 0, name));
        }

        foreach (var start in ScanRangeStarts(pe))
            queue.Enqueue((start, 0, "range"));

        var seen = new HashSet<uint>();
        var nodes = new List<Node>();
        while (queue.Count > 0 && nodes.Count < MaxFunctions)
        {
            var (va, depth, seed) = queue.Dequeue();
            if (!seen.Add(va))
                continue;
            var file = pe.FileOffset(va);
            if (file < 0 || !pe.InCode(file))
                continue;

            var steps = X86.WalkFunction(pe, file, MaxInsns);
            var calls = new List<uint>();
            var strings = new List<string>();
            foreach (var step in steps)
            {
                if (step.DirectCall is { } dest && pe.FileOffset(dest) >= 0)
                    calls.Add(dest);
                CollectQuoted(step.Text, strings);
            }

            nodes.Add(new Node
            {
                Va = va,
                Depth = depth,
                Seed = seed,
                Insns = steps.Count,
                Calls = calls,
                Strings = strings,
            });

            if (depth >= MaxDepth)
                continue;
            foreach (var dest in calls)
            {
                var destFile = pe.FileOffset(dest);
                if (destFile < 0 || !pe.InCode(destFile))
                    continue;
                var start = pe.Va(X86.FindPrologue(pe, destFile));
                if (seen.Contains(start))
                    continue;
                if (!InNewGameRange(start))
                    continue;
                queue.Enqueue((start, depth + 1, seed));
            }
        }

        nodes.Sort((a, b) => a.Va.CompareTo(b.Va));
        return nodes;
    }

    public static string ToMarkdown(IReadOnlyList<Node> nodes)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# New Game function map");
        sb.AppendLine();
        sb.AppendLine("Every function that starts in New Game / `StartOakVale` code ranges.");
        sb.AppendLine("Callees outside those ranges (CRT, later towns) are listed on the caller only.");
        sb.AppendLine();
        sb.AppendLine($"functions **{nodes.Count}** · depth ≤ {MaxDepth} · ranges {NewGameRanges.Length} · [INDEX](INDEX.md)");
        sb.AppendLine();
        sb.AppendLine("## Ranges");
        sb.AppendLine();
        foreach (var (lo, hi, name) in NewGameRanges)
        {
            var n = nodes.Count(x => x.Va >= lo && x.Va < hi);
            sb.AppendLine($"- `{name}` `0x{lo:X8}`–`0x{hi:X8}` · **{n}** fns");
        }

        sb.AppendLine();
        sb.AppendLine("## Seeds");
        sb.AppendLine();
        foreach (var (name, va) in NewGameSeeds)
            sb.AppendLine($"- `{name}` `0x{va:X8}`");
        sb.AppendLine();
        sb.AppendLine("## Hits");
        sb.AppendLine();
        WriteHits(sb, nodes, "StartOakVale");
        WriteHits(sb, nodes, "CREATURE_HERO_CHILD");
        WriteHits(sb, nodes, "HerosOldHouse");
        WriteHits(sb, nodes, "PALSKIN");
        WriteHits(sb, nodes, "CAM_OVIF");
        WriteHits(sb, nodes, "WatchBarrels");
        WriteHits(sb, nodes, "007E15C0");
        WriteHits(sb, nodes, "CBuyableHouse");
        WriteHits(sb, nodes, "0082E0E0");
        WriteHits(sb, nodes, "008864A0");
        WriteHits(sb, nodes, "CThingPlayerCreature");
        WriteHits(sb, nodes, "CTCAnimation");
        WriteHits(sb, nodes, "CTCIdle");
        WriteHits(sb, nodes, "PlayAnimation");
        WriteHits(sb, nodes, "STAND");
        WriteHits(sb, nodes, "01396FB8");
        WriteHits(sb, nodes, "00B783F0");
        WriteHits(sb, nodes, "00BDC2D0");
        WriteHits(sb, nodes, "00B2FD60");
        WriteHits(sb, nodes, "00B30B50");
        WriteHits(sb, nodes, "00B314E0");
        WriteHits(sb, nodes, "00B31160");
        WriteHits(sb, nodes, "00B2FC50");
        WriteHits(sb, nodes, "00BF6F80");
        WriteHits(sb, nodes, "00BDC180");
        WriteHits(sb, nodes, "00B41FA0");
        WriteHits(sb, nodes, "00B7A865");
        WriteHits(sb, nodes, "00BBC130");
        WriteHits(sb, nodes, "009896D0");
        WriteHits(sb, nodes, "2LIGHTS");
        WriteHits(sb, nodes, "2POINTLIGHTS");
        WriteHits(sb, nodes, "VSHADER_STATIC_DIRLIGHT_FOG");
        WriteHits(sb, nodes, "VSHADER_LANDSCAPE_FOREGROUND");
        WriteHits(sb, nodes, "MARKER_LIGHT");
        WriteHits(sb, nodes, "ENGINE_WATER");
        WriteHits(sb, nodes, "00BE91E0");
        WriteHits(sb, nodes, "00ABF6B0");
        WriteHits(sb, nodes, "00BCE740");
        WriteHits(sb, nodes, "00B849F0");
        WriteHits(sb, nodes, "00BD7110");
        WriteHits(sb, nodes, "00B91340");
        WriteHits(sb, nodes, "00B6CBD0");
        WriteHits(sb, nodes, "00B68DA0");
        WriteHits(sb, nodes, "00BB5040");
        sb.AppendLine();
        sb.AppendLine("## Functions");
        sb.AppendLine();
        sb.AppendLine("| va | depth | seed | insns | strings | calls |");
        sb.AppendLine("|---|---|---|---|---|---|");
        var shown = 0;
        foreach (var n in nodes)
        {
            if (shown++ >= 600)
            {
                sb.AppendLine($"| … | | | | | {nodes.Count - 600} more |");
                break;
            }

            var str = n.Strings.Count == 0 ? "" : "`" + Trunc(string.Join("; ", n.Strings.Take(3)), 48) + "`";
            var call = n.Calls.Count == 0 ? "" : string.Join(" ", n.Calls.Take(4).Select(c => $"`{c:X8}`"));
            if (n.Calls.Count > 4)
                call += $" +{n.Calls.Count - 4}";
            sb.AppendLine($"| `0x{n.Va:X8}` | {n.Depth} | {n.Seed} | {n.Insns} | {str} | {call} |");
        }

        return sb.ToString();
    }

    public static string ToTsv(IReadOnlyList<Node> nodes)
    {
        var sb = new StringBuilder();
        sb.AppendLine("va\tdepth\tseed\tinsns\tstrings\tcalls");
        foreach (var n in nodes)
        {
            var str = string.Join("|", n.Strings).Replace('\t', ' ');
            var call = string.Join(",", n.Calls.Select(c => c.ToString("X8")));
            sb.AppendLine($"0x{n.Va:X8}\t{n.Depth}\t{n.Seed}\t{n.Insns}\t{str}\t{call}");
        }

        return sb.ToString();
    }

    public static IReadOnlyList<uint> ScanRangeStarts(PeImage pe)
    {
        var starts = new HashSet<uint>();
        var data = pe.Data;
        foreach (var (lo, hi, _) in NewGameRanges)
        {
            var a = pe.FileOffset(lo);
            var b = pe.FileOffset(hi - 1);
            if (a < 0 || b < 0)
                continue;
            for (var i = a; i < b; i++)
            {
                if (!pe.InCode(i))
                    continue;
                if (X86.IsFramePrologue(data, i))
                {
                    var va = pe.Va(i);
                    if (InNewGameRange(va))
                        starts.Add(va);
                    continue;
                }

                // Two INT3s — skip lone 0xCC immediates inside instructions.
                if (i > a + 1 && data[i - 2] == 0xCC && data[i - 1] == 0xCC && data[i] != 0xCC)
                {
                    var start = pe.Va(X86.FindPrologue(pe, i));
                    if (InNewGameRange(start))
                        starts.Add(start);
                }
            }
        }

        return starts.OrderBy(v => v).ToList();
    }

    private static void WriteHits(StringBuilder sb, IReadOnlyList<Node> nodes, string key)
    {
        var hits = nodes.Where(n =>
            n.Strings.Any(s => s.Contains(key, StringComparison.OrdinalIgnoreCase)) ||
            n.Seed.Contains(key, StringComparison.OrdinalIgnoreCase) ||
            n.Calls.Any(c => c.ToString("X8").Contains(key, StringComparison.OrdinalIgnoreCase))).ToList();
        sb.AppendLine($"- **{key}**: {hits.Count} fns" + (hits.Count == 0
            ? ""
            : " — " + string.Join(", ", hits.Take(12).Select(h => $"`0x{h.Va:X8}`"))));
    }

    private static void CollectQuoted(string text, List<string> strings)
    {
        var i = text.IndexOf('"');
        if (i < 0)
            return;
        var j = text.IndexOf('"', i + 1);
        if (j <= i + 1)
            return;
        var s = text[(i + 1)..j];
        if (s.Length >= 4 && !strings.Contains(s, StringComparer.Ordinal))
            strings.Add(s);
    }

    private static string Trunc(string s, int n) => s.Length <= n ? s : s[..n] + "…";
}
