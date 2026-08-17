namespace Fable.Formats.IO;

/// <summary>Fable-framed LZO1X (uint16 size, 0xFFFF + uint32 for large blocks, last 3 bytes raw).</summary>
internal static class Lzo
{
    public static byte[] DecompressFramed(ReadOnlySpan<byte> data, ref int cursor, int expectedSize) =>
        DecompressFramed(data, ref cursor, expectedSize, out _);

    public static byte[] DecompressFramed(
        ReadOnlySpan<byte> data, ref int cursor, int expectedSize, out int produced)
    {
        produced = 0;
        if (expectedSize <= 0)
            return [];

        var result = new byte[expectedSize];
        var written = 0;
        var target = expectedSize > 3 ? expectedSize - 3 : 0;

        while (written < target && cursor + 2 <= data.Length)
        {
            var packed = BitConverter.ToUInt16(data.Slice(cursor, 2));
            cursor += 2;
            var compressedSize = (int)packed;
            if (packed == 0xFFFF)
            {
                if (cursor + 4 > data.Length)
                    break;
                compressedSize = (int)BitConverter.ToUInt32(data.Slice(cursor, 4));
                cursor += 4;
            }

            if (compressedSize == 0)
            {
                var remaining = target - written;
                var copy = Math.Min(remaining, data.Length - cursor);
                data.Slice(cursor, copy).CopyTo(result.AsSpan(written));
                cursor += copy;
                written += copy;
            }
            else
            {
                if (cursor + compressedSize > data.Length)
                    break;
                var rawProduced = DecompressRaw(data.Slice(cursor, compressedSize), result.AsSpan(written));
                cursor += compressedSize;
                written += rawProduced;
                if (rawProduced == 0)
                    break;
            }
        }

        if (cursor + 3 <= data.Length && expectedSize >= 3 && written >= target)
        {
            data.Slice(cursor, 3).CopyTo(result.AsSpan(expectedSize - 3));
            cursor += 3;
            written = expectedSize;
        }

        produced = written;
        return result;
    }

    public static int DecompressRaw(ReadOnlySpan<byte> input, Span<byte> output)
    {
        if (input.IsEmpty || output.IsEmpty)
            return 0;

        var ip = 0;
        var op = 0;
        var ipEnd = input.Length;
        var opEnd = output.Length;
        uint t = 0;
        int mPos = 0;
        uint mLen = 0;
        var state = input[0] > 17 ? 1 : 2;

        while (true)
        {
            switch (state)
            {
                case 1:
                    t = input[ip++] - 17u;
                    if (t < 4)
                    {
                        state = 6;
                        break;
                    }
                    if (t > opEnd - op || t > ipEnd - ip)
                        return 0;
                    while (t-- > 0)
                        output[op++] = input[ip++];
                    state = 3;
                    break;

                case 2:
                    if (ip >= ipEnd)
                        return op;
                    t = input[ip++];
                    if (t >= 16)
                    {
                        state = 4;
                        break;
                    }
                    if (t == 0)
                    {
                        while (ip < ipEnd && input[ip] == 0)
                        {
                            t += 255;
                            ip++;
                        }
                        if (ip >= ipEnd)
                            return 0;
                        t += input[ip++] + 15u;
                    }
                    if (4 > opEnd - op || 4 > ipEnd - ip)
                        return 0;
                    Copy4(output, op, input, ip);
                    op += 4;
                    ip += 4;
                    if (t > 0)
                    {
                        t--;
                        if (t > opEnd - op || t > ipEnd - ip)
                            return 0;
                        while (t >= 4)
                        {
                            Copy4(output, op, input, ip);
                            op += 4;
                            ip += 4;
                            t -= 4;
                        }
                        while (t-- > 0)
                            output[op++] = input[ip++];
                    }
                    state = 3;
                    break;

                case 3:
                    if (ip >= ipEnd)
                        return op;
                    t = input[ip++];
                    if (t >= 16)
                    {
                        state = 4;
                        break;
                    }
                    if (ip >= ipEnd)
                        return 0;
                    {
                        var offset = 2049 + (int)(t >> 2) + 4 * input[ip++];
                        if (offset > op || 3 > opEnd - op)
                            return 0;
                        var m = op - offset;
                        output[op++] = output[m];
                        output[op++] = output[m + 1];
                        output[op++] = output[m + 2];
                    }
                    state = 5;
                    break;

                case 4:
                    if (ip >= ipEnd)
                        return op;
                    if (t >= 64)
                    {
                        if (ip >= ipEnd)
                            return 0;
                        var offset = 1 + (int)((t >> 2) & 7) + 8 * input[ip++];
                        if (offset > op)
                            return 0;
                        mPos = op - offset;
                        mLen = (t >> 5) - 1;
                        state = 8;
                        break;
                    }
                    if (t >= 32)
                    {
                        mLen = t & 31;
                        if (mLen == 0)
                        {
                            while (ip < ipEnd && input[ip] == 0)
                            {
                                mLen += 255;
                                ip++;
                            }
                            if (ip >= ipEnd)
                                return 0;
                            mLen += input[ip++] + 31u;
                        }
                        if (ip + 2 > ipEnd)
                            return 0;
                        var dist = BitConverter.ToUInt16(input.Slice(ip, 2));
                        ip += 2;
                        var off32 = 1 + (dist >> 2);
                        if (off32 > op)
                            return 0;
                        mPos = op - off32;
                        state = 7;
                        break;
                    }
                    if (t >= 16)
                    {
                        var baseOff = 0x800 * (int)(t & 8);
                        mLen = t & 7;
                        if (mLen == 0)
                        {
                            while (ip < ipEnd && input[ip] == 0)
                            {
                                mLen += 255;
                                ip++;
                            }
                            if (ip >= ipEnd)
                                return 0;
                            mLen += input[ip++] + 7u;
                        }
                        if (ip + 2 > ipEnd)
                            return 0;
                        var dist = BitConverter.ToUInt16(input.Slice(ip, 2));
                        ip += 2;
                        var totalOff = baseOff + (dist >> 2);
                        if (totalOff == 0)
                            return op;
                        totalOff += 0x4000;
                        if (totalOff > op)
                            return 0;
                        mPos = op - totalOff;
                        state = 7;
                        break;
                    }
                    if (ip >= ipEnd)
                        return 0;
                    {
                        var offset = 1 + (int)(t >> 2) + 4 * input[ip++];
                        if (offset > op || 2 > opEnd - op)
                            return 0;
                        mPos = op - offset;
                        output[op++] = output[mPos];
                        output[op++] = output[mPos + 1];
                    }
                    state = 5;
                    break;

                case 5:
                    if (ip >= ipEnd)
                        return op;
                    t = (uint)(input[ip - 2] & 3);
                    state = t == 0 ? 2 : 6;
                    break;

                case 6:
                    if (t > opEnd - op || t > ipEnd - ip)
                        return 0;
                    while (t-- > 0)
                        output[op++] = input[ip++];
                    if (ip >= ipEnd)
                        return op;
                    t = input[ip++];
                    state = 4;
                    break;

                case 7:
                    if (mLen + 2 > opEnd - op)
                        return 0;
                    if (mLen >= 6 && op - mPos >= 4)
                    {
                        Copy4(output, op, output, mPos);
                        op += 4;
                        mPos += 4;
                        var remaining = (int)mLen - 2;
                        while (remaining >= 4)
                        {
                            Copy4(output, op, output, mPos);
                            remaining -= 4;
                            op += 4;
                            mPos += 4;
                        }
                        while (remaining-- > 0)
                            output[op++] = output[mPos++];
                        state = 5;
                        break;
                    }
                    state = 8;
                    break;

                case 8:
                    if (mLen + 2 > opEnd - op)
                        return 0;
                    output[op++] = output[mPos++];
                    output[op++] = output[mPos++];
                    while (mLen-- > 0)
                        output[op++] = output[mPos++];
                    state = 5;
                    break;
            }
        }
    }

    private static void Copy4(Span<byte> dest, int destIndex, ReadOnlySpan<byte> src, int srcIndex)
    {
        dest[destIndex] = src[srcIndex];
        dest[destIndex + 1] = src[srcIndex + 1];
        dest[destIndex + 2] = src[srcIndex + 2];
        dest[destIndex + 3] = src[srcIndex + 3];
    }
}
