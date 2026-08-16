using Fable.Formats.Tng;

namespace Fable.Formats.Wld;

/// <summary>
/// Packed TNG field CTCDRegionExit.EntranceConnectedToUID.
/// High bits are the destination WLD MapUID; low 32 bits match the
/// destination REGION_ENTRANCE_POINT.UID (which itself is 0xFFFFFE00_00000000 | slot).
/// </summary>
public readonly record struct RegionLink(int MapUid, uint EntranceSlot)
{
    public const ulong EntranceUidPrefix = 0xFFFFFE0000000000UL;

    public static RegionLink Unpack(ulong packed) =>
        new((int)(packed >> 40), (uint)packed);

    public ulong Pack() => ((ulong)(uint)MapUid << 40) | EntranceSlot;

    public static uint SlotOfThing(ulong thingUid) => (uint)thingUid;

    public static ulong EntranceThingUid(uint slot) => EntranceUidPrefix | slot;

    public ThingInstance? FindEntrance(IEnumerable<ThingInstance> destThings)
    {
        foreach (var thing in destThings)
        {
            if (thing.DefinitionType == "REGION_ENTRANCE_POINT" &&
                (uint)(thing.Uid ?? 0) == EntranceSlot)
                return thing;
        }

        return null;
    }
}
