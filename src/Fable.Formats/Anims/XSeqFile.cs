using System.Numerics;
using System.Text;
using Fable.Formats.IO;
using Fable.Formats.Meshes;

namespace Fable.Formats.Anims;

/// <summary>
/// graphics.big type-6 clip. Persist ctor <c>00A999B0</c> tags
/// <c>3DAF</c>; nested <c>XSEQ</c> is <c>00AA4680</c> vtbl
/// <c>0129E194</c> ("Compressed Animation Sequence").
/// Bank payload is <c>u32</c> uncompressed size then raw LZO
/// (not framed C3D LZO) unless the first dword is
/// <c>&gt;&gt;&gt;&gt;</c>.
/// </summary>
public sealed class XSeqFile
{
    public const uint UncompressedMarker = 0x3E3E3E3E;
    public const uint FourCc3Daf = 0x46414433;
    public const uint FourCcAnrt = 0x54524E41;
    public const uint FourCcAobj = 0x4A424F41;
    public const uint FourCcXseq = 0x51455358;
    public const uint FourCcXalo = 0x4F4C4158;
    public const uint FourCcHlpr = 0x52504C48;
    public const uint FourCcMvec = 0x4345564D;
    public const uint FourCcFoot = 0x544F4F46;
    public const uint FourCcPupp = 0x50505550;
    public const uint FourCcAmsk = 0x4B534D41;
    public const uint FourCcTmev = 0x56454D54;
    public const uint Ctor3Daf = 0x00A999B0;
    public const uint CtorXseq = 0x00AA4680;
    public const uint XseqVtbl = 0x0129E194;
    public const uint UnpackFn = 0x00A4C5E0;
    public const uint PersistLoadFn = 0x00A4CDD0;
    public const uint CompressFn = 0x00A4EFC0;
    public const uint LocalCopyFn = 0x00AAF1E0;
    public const uint HierarchyFn = 0x00AA0090;
    public const int ClipRecordBytes = 44;
    public const int BoneLocalBytes = 48;
    public const int WakeLoopId = 3420;
    public const string WakeLoopName = "CS_OAKVALE_DREAM_INTRO_YOUNG_HERO_WAKING_UP_LOOP";

    public required string Name { get; init; }
    public required int UncompressedSize { get; init; }
    public required bool WasCompressed { get; init; }
    public required float Duration { get; init; }
    public required bool Cyclic { get; init; }
    public required IReadOnlyList<XSeqChunk> Chunks { get; init; }
    public required IReadOnlyList<XSeqTrack> Tracks { get; init; }
    public IReadOnlyList<string> BoneNames { get; init; } = [];

    public static XSeqFile? TryParse(byte[] data, string name = "")
    {
        try
        {
            return Parse(data, name);
        }
        catch
        {
            return null;
        }
    }

    public static XSeqFile Parse(byte[] data, string name = "")
    {
        if (data.Length < 8)
            throw new InvalidDataException("type-6 clip shorter than header.");

        var size0 = BitConverter.ToUInt32(data, 0);
        byte[] payload;
        var compressed = false;
        if (size0 == UncompressedMarker)
        {
            payload = data.AsSpan(4).ToArray();
        }
        else if (size0 == FourCc3Daf || size0 == FourCcAnrt)
        {
            payload = data;
        }
        else if (size0 > 8 && size0 < 16_000_000)
        {
            var raw = new byte[size0];
            var produced = Lzo.DecompressRaw(data.AsSpan(4), raw);
            if (produced <= 0)
            {
                var cursor = 4;
                raw = Lzo.DecompressFramed(data, ref cursor, (int)size0, out produced);
            }

            if (produced <= 0)
                throw new InvalidDataException($"type-6 LZO produced 0 for {name} size0={size0}.");
            payload = produced == raw.Length ? raw : raw.AsSpan(0, produced).ToArray();
            compressed = true;
        }
        else
        {
            payload = data;
        }

        var body = payload;
        var o = 0;
        if (body.Length >= 4 && BitConverter.ToUInt32(body, 0) == UncompressedMarker)
            o = 4;
        if (o + 4 <= body.Length && BitConverter.ToUInt32(body, o) == FourCc3Daf)
            o += 4;
        if (o + 4 <= body.Length)
        {
            var ver = BitConverter.ToUInt32(body, o);
            if (ver is > 0 and < 1000)
                o += 4;
        }

        if (o < body.Length && body[o] is >= 0x20 and <= 0x7E)
            ReadCString(body, ref o);
        Align4(ref o);

        var chunks = new List<XSeqChunk>();
        WalkChunks(body, o, body.Length, chunks, 0);

        var cyclic = false;
        var duration = 0f;
        foreach (var chunk in chunks)
        {
            if (chunk.FourCc != FourCcAnrt || chunk.Payload.Length < 5)
                continue;
            cyclic = chunk.Payload[0] != 0;
            duration = BitConverter.ToSingle(chunk.Payload, 1);
            break;
        }

        var tracks = new List<XSeqTrack>();
        foreach (var chunk in chunks)
        {
            if (chunk.FourCc != FourCcXseq)
                continue;
            if (TryReadTrack(chunk.Payload, out var track))
                tracks.Add(track);
        }

        if (tracks.Count == 0)
        {
            foreach (var chunk in chunks)
            {
                if (chunk.FourCc is FourCcHlpr or FourCcMvec or FourCcFoot or FourCcPupp)
                    continue;
                if (TryReadTrack(chunk.Payload, out var track) && track.Name.Length > 0)
                    tracks.Add(track);
            }
        }

        return new XSeqFile
        {
            Name = name,
            UncompressedSize = payload.Length,
            WasCompressed = compressed,
            Duration = duration > 0f ? duration : 1f,
            Cyclic = cyclic,
            Chunks = chunks,
            Tracks = tracks,
            BoneNames = tracks.Select(t => t.Name).Where(n => n.Length > 0).ToArray(),
        };
    }

    public bool TrySample(string bone, float time, out Quaternion rotation, out Vector3 translation)
    {
        _ = time;
        foreach (var track in Tracks)
        {
            if (!track.Name.Equals(bone, StringComparison.OrdinalIgnoreCase))
                continue;
            rotation = track.FirstRotation;
            translation = track.FirstTranslation;
            return track.HasRotation || track.HasTranslation;
        }

        rotation = Quaternion.Identity;
        translation = Vector3.Zero;
        return false;
    }

    public MeshBone[] ApplyFirstLocals(IReadOnlyList<MeshBone> bones)
    {
        var replaced = new MeshBone[bones.Count];
        for (var i = 0; i < bones.Count; i++)
        {
            var bone = bones[i];
            XSeqTrack? track = null;
            foreach (var item in Tracks)
            {
                if (item.Name.Equals(bone.Name, StringComparison.OrdinalIgnoreCase))
                {
                    track = item;
                    break;
                }
            }

            if (track is null)
            {
                replaced[i] = bone;
                continue;
            }

            replaced[i] = bone with
            {
                LocalRotation = track.Value.HasRotation
                    ? track.Value.FirstRotation
                    : bone.LocalRotation,
                LocalTranslation = track.Value.HasTranslation
                    ? track.Value.FirstTranslation
                    : bone.LocalTranslation,
            };
        }

        return replaced;
    }

    private static void WalkChunks(
        byte[] data, int start, int end, List<XSeqChunk> chunks, int depth)
    {
        var cursor = start;
        while (cursor + 8 <= end && depth < 8)
        {
            if (!LooksLikeFourCc(data, cursor))
            {
                cursor++;
                continue;
            }

            var four = BitConverter.ToUInt32(data, cursor);
            var size = (int)BitConverter.ToUInt32(data, cursor + 4);
            if (size < 0 || cursor + 8 + size > end)
                break;
            var payload = data.AsSpan(cursor + 8, size).ToArray();
            chunks.Add(new XSeqChunk(four, size, cursor, payload));
            if (four is FourCcAnrt or FourCcAobj or FourCcHlpr or FourCcMvec)
                WalkChunks(payload, 0, payload.Length, chunks, depth + 1);
            cursor += 8 + size;
        }
    }

    private static bool TryReadTrack(byte[] payload, out XSeqTrack track)
    {
        track = default;
        if (payload.Length < 12)
            return false;
        var cursor = 0;
        var boneIndex = ReadI32(payload, ref cursor);
        var parent = ReadI32(payload, ref cursor);
        var name = "";
        if (cursor < payload.Length && payload[cursor] is >= 0x20 and <= 0x7E)
            name = ReadCString(payload, ref cursor);
        else
        {
            cursor = 0;
            name = ReadCString(payload, ref cursor);
            boneIndex = 0;
            parent = -1;
        }

        if (name.Length == 0 || name.Length > 64)
            return false;

        var preFps = cursor < payload.Length ? payload[cursor++] : (byte)0;
        var fps = cursor + 4 <= payload.Length ? ReadF32(payload, ref cursor) : 0f;
        var frames = cursor + 4 <= payload.Length ? ReadU32(payload, ref cursor) : 0;
        if (cursor + 4 <= payload.Length)
            cursor += 4;
        var posFactor = cursor + 4 <= payload.Length ? ReadF32(payload, ref cursor) : 1f;
        var scaleFactor = cursor + 4 <= payload.Length ? ReadF32(payload, ref cursor) : 1f;
        if (posFactor is 0f or float.NaN)
            posFactor = 1f;

        var rot = Quaternion.Identity;
        var pos = Vector3.Zero;
        var hasRot = false;
        var hasPos = false;
        var rotCount = 0;
        var posCount = 0;
        if (cursor + 2 <= payload.Length)
        {
            rotCount = BitConverter.ToUInt16(payload, cursor);
            cursor += 2;
            if (rotCount > 0 && rotCount < 4096 && cursor + 16 <= payload.Length)
            {
                rot = new Quaternion(
                    BitConverter.ToSingle(payload, cursor),
                    BitConverter.ToSingle(payload, cursor + 4),
                    BitConverter.ToSingle(payload, cursor + 8),
                    BitConverter.ToSingle(payload, cursor + 12));
                hasRot = rot.LengthSquared() > 0.01f;
                if (hasRot)
                    rot = Quaternion.Normalize(rot);
                cursor += 16 * Math.Min(rotCount, (payload.Length - cursor) / 16);
            }
            else if (rotCount >= 4096)
            {
                rotCount = 0;
            }
        }

        if (cursor + 2 <= payload.Length)
        {
            var palRot = BitConverter.ToUInt16(payload, cursor);
            cursor += 2;
            var width = rotCount > 255 ? 2 : 1;
            var skip = palRot * width;
            if (palRot < 8192 && cursor + skip <= payload.Length)
                cursor += skip;
        }

        if (cursor + 2 <= payload.Length)
        {
            posCount = BitConverter.ToUInt16(payload, cursor);
            cursor += 2;
            if (posCount > 0 && posCount < 4096 && cursor + 6 <= payload.Length)
            {
                pos = new Vector3(
                    BitConverter.ToInt16(payload, cursor) * posFactor,
                    BitConverter.ToInt16(payload, cursor + 2) * posFactor,
                    BitConverter.ToInt16(payload, cursor + 4) * posFactor);
                hasPos = true;
            }
        }

        if (!hasRot && !hasPos && name.Length == 0)
            return false;

        track = new XSeqTrack(
            name, boneIndex, parent, fps, frames, preFps,
            posFactor, scaleFactor, rot, pos, hasRot, hasPos, rotCount, posCount);
        return name.Length > 0;
    }

    private static bool LooksLikeFourCc(byte[] data, int offset)
    {
        for (var i = 0; i < 4; i++)
        {
            var b = data[offset + i];
            if (b is < (byte)'A' or > (byte)'Z')
                return false;
        }

        return true;
    }

    private static void Align4(ref int cursor) =>
        cursor = (cursor + 3) & ~3;

    private static string ReadCString(byte[] data, ref int cursor)
    {
        var start = cursor;
        while (cursor < data.Length && data[cursor] != 0)
            cursor++;
        var text = start < cursor
            ? Encoding.ASCII.GetString(data, start, cursor - start)
            : "";
        if (cursor < data.Length)
            cursor++;
        return text;
    }

    private static int ReadI32(byte[] data, ref int cursor)
    {
        var value = BitConverter.ToInt32(data, cursor);
        cursor += 4;
        return value;
    }

    private static uint ReadU32(byte[] data, ref int cursor)
    {
        var value = BitConverter.ToUInt32(data, cursor);
        cursor += 4;
        return value;
    }

    private static float ReadF32(byte[] data, ref int cursor)
    {
        var value = BitConverter.ToSingle(data, cursor);
        cursor += 4;
        return value;
    }
}

public readonly record struct XSeqChunk(uint FourCc, int Size, int Offset, byte[] Payload)
{
    public string Tag => Encoding.ASCII.GetString(BitConverter.GetBytes(FourCc));
}

public readonly record struct XSeqTrack(
    string Name,
    int BoneIndex,
    int Parent,
    float SamplesPerSecond,
    uint FrameCount,
    byte PreFps,
    float PositionFactor,
    float ScalingFactor,
    Quaternion FirstRotation,
    Vector3 FirstTranslation,
    bool HasRotation,
    bool HasTranslation,
    int RotationCount,
    int PositionCount);
