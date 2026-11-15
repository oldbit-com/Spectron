using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using OldBit.Spectron.Emulation.Screen;

namespace OldBit.Spectron.Controls.Scr;

internal static class ScrConverter
{
    private static readonly int[] Palette =
    [
        unchecked((int)0xFF000000), unchecked((int)0xFF0000D7), unchecked((int)0xFFD70000), unchecked((int)0xFFD700D7),
        unchecked((int)0xFF00D700), unchecked((int)0xFF00D7D7), unchecked((int)0xFFD7D700), unchecked((int)0xFFD7D7D7),
        unchecked((int)0xFF000000), unchecked((int)0xFF0000FF), unchecked((int)0xFFFF0000), unchecked((int)0xFFFF00FF),
        unchecked((int)0xFF00FF00), unchecked((int)0xFF00FFFF), unchecked((int)0xFFFFFF00), unchecked((int)0xFFFFFFFF),
    ];

    internal static WriteableBitmap Decode(ReadOnlySpan<byte> scr, bool flashInverted)
    {
        var bitmap = new WriteableBitmap(
            new PixelSize(256, 192),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Opaque);

        using var lockedBitmap = bitmap.Lock();
        var row = new int[256];

        for (var y = 0; y < ScreenSize.ContentHeight; y++)
        {
            var pixelBase = ((y & 0xC0) << 5) | ((y & 0x07) << 8) | ((y & 0x38) << 2);
            var attrBase = 0x1800 + (y >> 3) * 32;

            for (var col = 0; col < 32; col++)
            {
                var pixels = scr[pixelBase + col];
                var attr = scr[attrBase + col];

                var bright = (attr & 0x40) >> 3;
                var ink = Palette[(attr & 0x07) | bright];
                var paper = Palette[((attr >> 3) & 0x07) | bright];
                var flash = (attr & 0x80) != 0;

                for (var bit = 0; bit < 8; bit++)
                {
                    row[col * 8 + bit] = ((pixels & (0x80 >> bit)) != 0) ^ (flash & flashInverted) ? ink : paper;
                }
            }

            Marshal.Copy(row, 0, lockedBitmap.Address + y * lockedBitmap.RowBytes, 256);
        }

        return bitmap;
    }
}