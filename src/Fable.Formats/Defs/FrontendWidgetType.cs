namespace Fable.Formats.Defs;

/// <summary>
/// <c>0041D21B</c> <c>cmp eax, 43</c> then
/// <c>jmp [0x41D7F8+type*4]</c>. Size is the
/// <c>push imm</c> before <c>00BFEA1A</c>.
/// Ctor is the <c>call</c> in that case.
/// Vtbl is the first <c>mov [esi], imm</c>
/// after the base ctor. Type 29 is the
/// default arm (no alloc).
/// </summary>
public static class FrontendWidgetType
{
    public const uint ConstructFn = 0x0041D21B;
    public const uint FactoryFn = 0x0041DB1D;
    public const uint ResolveFn = 0x0042AEDA;
    public const uint JumpTableVa = 0x0041D7F8;
    public const uint DefLookupFn = 0x009AD410;
    public const uint ChildAttachFn = 0x005331A0;
    /// <summary>
    /// Type 5/10/18 <c>vtbl+8</c>. Walks
    /// <c>+176</c> then <c>+188</c>.
    /// </summary>
    public const uint ContainerDrawFn = 0x00530260;
    /// <summary>
    /// <c>vtbl+192</c>. Writes
    /// <c>+332</c> and forwards
    /// <c>vtbl+188</c> to own
    /// <c>+176</c> children.
    /// </summary>
    public const uint SelectStateFn = 0x0052CF40;
    /// <summary>
    /// <c>0052C730</c> after
    /// <c>005339B0</c>:
    /// <c>+324/+328/+332=0</c>.
    /// </summary>
    public const int FirstSeenState = 0;
    public const int ChildListOffset = 176;
    public const int ChildListEndOffset = 180;
    public const int StateOffset = 332;
    public const int TypeOffset = 60;
    public const int MaxType = 43;

    public const int Button = 0;
    public const int TableType = 2;
    public const int Base = 4;
    public const int Group = 5;
    public const int Text = 6;
    public const int Menu = 10;
    public const int List = 12;
    public const int TextSlider = 16;
    public const int Swap = 18;
    public const int Unused = 29;
    public const int Mouse = 32;
    public const int EditBox = 37;
    public const int AcceptButton = 38;

    public const uint ButtonCtor = 0x0041B800;
    public const uint TableCtor = 0x005517E0;
    public const uint BaseCtor = 0x005334A0;
    public const uint GroupCtor = 0x0052CC50;
    public const uint TextCtor = 0x0054F5C0;
    public const uint MenuCtor = 0x0054E3D0;
    public const uint ListCtor = 0x0054C3A0;
    public const uint TextSliderCtor = 0x00549F60;
    public const uint SwapCtor = 0x00547600;
    public const uint MouseCtor = 0x0055C650;
    public const uint EditBoxCtor = 0x005407B0;
    public const uint AcceptCtor = 0x00558B90;

    public const uint ButtonVtbl = 0x0122F5D4;
    public const uint BaseVtbl = 0x0124608C;
    public const uint GroupVtbl = 0x01245DE4;
    public const uint TextVtbl = 0x01249CCC;
    public const uint MenuVtbl = 0x012497E4;
    public const uint TextSliderVtbl = 0x01248A8C;
    public const uint SwapVtbl = 0x012485AC;
    public const uint MouseVtbl = 0x0124C22C;
    public const uint EditBoxVtbl = 0x01246B8C;
    public const uint AcceptVtbl = 0x0124B04C;

    public const int ButtonSize = 0x184;
    public const int GroupSize = 0x15C;
    public const int TextSize = 0x18C;
    public const int MenuSize = 0x16C;
    public const int MouseSize = 0x184;
    public const int EditBoxSize = 0x18C;
    public const int AcceptSize = 0x194;
    public const int BaseSize = 0x134;

    public static FrontendWidgetTypeInfo Info(int type)
    {
        if ((uint)type > MaxType)
            return default;
        return Table[type];
    }

    public static uint Ctor(int type) => Info(type).Ctor;

    public static uint Vtbl(int type) => Info(type).Vtbl;

    public static int Size(int type) => Info(type).Size;

    public static bool IsContainer(int type) =>
        type is Group or Menu or List or Swap or TableType;

    /// <summary>
    /// <c>vtbl+400</c> <c>0052F180</c>:
    /// <c>[+300] >> 7</c>. <c>00530260</c>
    /// skips when parent!=this and this
    /// bit is 0. Persist <c>def+504</c>.
    /// </summary>
    public const uint BorrowedVisibleFn = 0x0052F180;
    public const int Plus300Offset = 300;
    public const int Plus300BorrowedBit = 7;
    /// <summary>
    /// <c>vtbl+420</c> <c>0052F1D0</c>:
    /// <c>[+302] &amp; 1</c>. Persist
    /// <c>def+392</c> at <c>00533288</c>.
    /// CRC UNREAD.
    /// </summary>
    public const uint ClipBitFn = 0x0052F1D0;
    public const int Plus302Offset = 302;
    public const int Plus302ClipBit = 0;
    /// <summary>
    /// Type 16 selected child is
    /// <c>+348</c> (<c>00549B20</c>),
    /// not <c>+332</c>.
    /// </summary>
    public const int TextSliderIndexOffset = 348;
    /// <summary>
    /// <c>vtbl+188</c> <c>0041C5A0</c>:
    /// store duration at <c>+320</c>
    /// then <c>vtbl+192</c>.
    /// </summary>
    public const uint ForwardSelectFn = 0x0041C5A0;
    public const int DurationOffset = 320;
    /// <summary>
    /// Type-0 <c>0041AFA0</c> tests
    /// <c>+151</c>. Alpha 0 with
    /// <c>+368==1</c> returns before
    /// <c>0041BEB0</c>. Type 6
    /// <c>0054EF00</c> same alpha
    /// vs <c>+394</c>.
    /// </summary>
    public const uint LeafPresentFn = 0x0041AFA0;
    public const int PackedAlphaOffset = 151;
    public const uint Type6PresentFn = 0x0054EF00;

    /// <summary>
    /// Packed <c>+151</c> is the high
    /// byte of persist colour.
    /// </summary>
    public static bool LeafDipSkipped(uint colour) =>
        (colour >> 24) == 0;

    /// <summary>
    /// <c>vtbl+8 == 00530260</c> on
    /// type 5 / 10 / 12 / 16 / 18.
    /// </summary>
    public static bool DrawsChildList(int type) =>
        type is Group or Menu or List or TextSlider or Swap;

    /// <summary>
    /// Type 18 <c>CSwappingStateComponent</c>
    /// (<c>00547600</c> vtbl
    /// <c>012485AC</c>) and type 16
    /// <c>CTextSlider</c>
    /// (<c>00549F60</c> vtbl
    /// <c>01248A8C</c>). First-seen
    /// keeps persist child 0
    /// (type 18 style <c>+332</c>;
    /// type 16 index
    /// <see cref="TextSliderIndexOffset"/>).
    /// Other persist children stay
    /// in the tree. <c>00530260</c>
    /// walks every <c>+176</c> child
    /// and skips via <c>vtbl+400</c>
    /// / <c>vtbl+420</c>.
    /// </summary>
    public static bool SelectsChild(int type) =>
        type is Swap or TextSlider;

    public static bool TryConstruct(int type) =>
        type != Unused && Info(type).Ctor != 0;

    public static readonly FrontendWidgetTypeInfo[] Table =
    [
        new(0, 0x0041B800, 0x184, 0x0122F5D4, "Button"),
        new(1, 0x005545D0, 0x19C, 0, null),
        new(2, 0x005517E0, 0x170, 0, "Table"),
        new(3, 0x00550190, 0x1A4, 0, null),
        new(4, 0x005334A0, 0x134, 0x0124608C, "Base"),
        new(5, 0x0052CC50, 0x15C, 0x01245DE4, "Group"),
        new(6, 0x0054F5C0, 0x18C, 0x01249CCC, "Text"),
        new(7, 0x0053DFE0, 0x1BC, 0, null),
        new(8, 0x0053B63E, 0x1FC, 0, null),
        new(9, 0x0054EA00, 0x174, 0, null),
        new(10, 0x0054E3D0, 0x16C, 0x012497E4, "Menu"),
        new(11, 0x0054E0B0, 0x1B4, 0, null),
        new(12, 0x0054C3A0, 0x1FC, 0, "List"),
        new(13, 0x0053F120, 0x19C, 0, null),
        new(14, 0x0054C1D0, 0x190, 0, null),
        new(15, 0x0054C050, 0x1EC, 0, null),
        new(16, 0x00549F60, 0x1A0, 0x01248A8C, "TextSlider"),
        new(17, 0x005482D0, 0x198, 0, null),
        new(18, 0x00547600, 0x170, 0x012485AC, "Swap"),
        new(19, 0x00546F40, 0x15C, 0, null),
        new(20, 0x00546D30, 0x16C, 0, null),
        new(21, 0x00546B00, 0x184, 0, null),
        new(22, 0x005460C0, 0x15C, 0, null),
        new(23, 0x00545720, 0x17C, 0, null),
        new(24, 0x00544B70, 0x1A8, 0, null),
        new(25, 0x0041CADC, 0x164, 0, null),
        new(26, 0x0041CB70, 0x160, 0, null),
        new(27, 0x00544010, 0x164, 0, null),
        new(28, 0x0041CBE4, 0x160, 0, null),
        new(29, 0, 0, 0, null),
        new(30, 0x00542330, 0x1B4, 0, null),
        new(31, 0x005415F0, 0x180, 0, null),
        new(32, 0x0055C650, 0x184, 0x0124C22C, "Mouse"),
        new(33, 0x0055BA20, 0x16C, 0, null),
        new(34, 0x0055B460, 0x194, 0, null),
        new(35, 0x0055A9C0, 0x1AC, 0, null),
        new(36, 0x00558EC0, 0x170, 0, null),
        new(37, 0x005407B0, 0x18C, 0x01246B8C, "EditBox"),
        new(38, 0x00558B90, 0x194, 0x0124B04C, "AcceptButton"),
        new(39, 0x00558540, 0x1C0, 0, null),
        new(40, 0x00556350, 0x190, 0, null),
        new(41, 0x00559830, 0x1DC, 0, null),
        new(42, 0x00559360, 0x1A0, 0, null),
        new(43, 0x00555180, 0x17C, 0, null),
    ];
}

public readonly record struct FrontendWidgetTypeInfo(
    int Type,
    uint Ctor,
    int Size,
    uint Vtbl,
    string? Role);
