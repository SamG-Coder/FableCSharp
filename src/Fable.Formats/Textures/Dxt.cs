namespace Fable.Formats.Textures;

internal static class Dxt
{
    public static byte[] Decode(ReadOnlySpan<byte> blocks, int width, int height, bool dxt5)
    {
        var rgba = new byte[width * height * 4];
        var blockW = Math.Max(1, (width + 3) / 4);
        var blockH = Math.Max(1, (height + 3) / 4);
        var blockSize = dxt5 ? 16 : 8;
        var offset = 0;

        for (var by = 0; by < blockH; by++)
        for (var bx = 0; bx < blockW; bx++)
        {
            if (offset + blockSize > blocks.Length)
                return rgba;

            var alpha = dxt5
                ? DecodeAlpha(blocks.Slice(offset, 8))
                : null;
            var colorOff = dxt5 ? offset + 8 : offset;
            var colors = DecodeColors(blocks.Slice(colorOff, 8));
            var lookup = BitConverter.ToUInt32(blocks.Slice(colorOff + 4, 4));

            for (var py = 0; py < 4; py++)
            for (var px = 0; px < 4; px++)
            {
                var x = bx * 4 + px;
                var y = by * 4 + py;
                if (x >= width || y >= height)
                    continue;
                var idx = (int)((lookup >> (2 * (py * 4 + px))) & 3);
                var o = (y * width + x) * 4;
                rgba[o] = colors[idx, 0];
                rgba[o + 1] = colors[idx, 1];
                rgba[o + 2] = colors[idx, 2];
                rgba[o + 3] = alpha is null ? (byte)255 : alpha[py * 4 + px];
            }

            offset += blockSize;
        }

        return rgba;
    }

    public static int MipChainSize(int width, int height, int blockBytes)
    {
        var total = 0;
        var w = width;
        var h = height;
        while (true)
        {
            var bw = Math.Max(1, (w + 3) / 4);
            var bh = Math.Max(1, (h + 3) / 4);
            total += bw * bh * blockBytes;
            if (w == 1 && h == 1)
                break;
            w = Math.Max(1, w / 2);
            h = Math.Max(1, h / 2);
        }

        return total;
    }

    private static byte[,] DecodeColors(ReadOnlySpan<byte> block)
    {
        var c0 = BitConverter.ToUInt16(block);
        var c1 = BitConverter.ToUInt16(block.Slice(2));
        var colors = new byte[4, 3];
        Unpack565(c0, out colors[0, 0], out colors[0, 1], out colors[0, 2]);
        Unpack565(c1, out colors[1, 0], out colors[1, 1], out colors[1, 2]);
        if (c0 > c1)
        {
            for (var i = 0; i < 3; i++)
            {
                colors[2, i] = (byte)((2 * colors[0, i] + colors[1, i]) / 3);
                colors[3, i] = (byte)((colors[0, i] + 2 * colors[1, i]) / 3);
            }
        }
        else
        {
            for (var i = 0; i < 3; i++)
                colors[2, i] = (byte)((colors[0, i] + colors[1, i]) / 2);
        }

        return colors;
    }

    private static byte[] DecodeAlpha(ReadOnlySpan<byte> block)
    {
        var a0 = block[0];
        var a1 = block[1];
        var table = new byte[8];
        table[0] = a0;
        table[1] = a1;
        if (a0 > a1)
        {
            for (var i = 1; i <= 6; i++)
                table[i + 1] = (byte)(((7 - i) * a0 + i * a1) / 7);
        }
        else
        {
            for (var i = 1; i <= 4; i++)
                table[i + 1] = (byte)(((5 - i) * a0 + i * a1) / 5);
            table[6] = 0;
            table[7] = 255;
        }

        ulong bits = 0;
        for (var i = 0; i < 6; i++)
            bits |= (ulong)block[2 + i] << (8 * i);

        var alpha = new byte[16];
        for (var i = 0; i < 16; i++)
            alpha[i] = table[(bits >> (3 * i)) & 7];
        return alpha;
    }

    private static void Unpack565(ushort value, out byte r, out byte g, out byte b)
    {
        r = (byte)(((value >> 11) & 31) * 255 / 31);
        g = (byte)(((value >> 5) & 63) * 255 / 63);
        b = (byte)((value & 31) * 255 / 31);
    }
}
