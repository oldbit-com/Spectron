using System;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Models;

namespace OldBit.Spectron.Helpers;

internal record struct Border(int Top, int Left, int Right, int Bottom);

/// <summary>
/// Converts and writes the frame buffer to a WriteableBitmap which can be displayed by Avalonia.
/// </summary>
internal sealed class FrameBufferConverter : IDisposable
{
    private static readonly Border BorderNone = new(Top: 0, Left: 0, Right: 0, Bottom: 0);
    private static readonly Border BorderSmall = new(Top: 15, Left: 15, Right: 15, Bottom: 15);
    private static readonly Border BorderMedium = new(Top: 25, Left: 25, Right: 25, Bottom: 25);
    private static readonly Border BorderLarge = new(Top: 40, Left: 40, Right: 40, Bottom: 40);
    private static readonly Border BorderFull = new(Top: 64, Left: 48, Right: 48, Bottom: 56);

    private Border _border = BorderFull;

    private readonly int _zoomX;
    private readonly int _zoomY;

    internal WriteableBitmap Bitmap { get; private set; }

    internal FrameBufferConverter(int zoomX, int zoomY)
    {
        _zoomX = zoomX;
        _zoomY = zoomY;

        Bitmap = CreateBitmap();
    }
    internal void UpdateBitmap(FrameBuffer frameBuffer)
    {
        var startFrameBufferRow = BorderFull.Top - _border.Top;
        var endFrameBufferRow = FrameBuffer.Height - (BorderFull.Bottom - _border.Bottom) - 1;
        var startFrameBufferCol = BorderFull.Left - _border.Left;
        var endFrameBufferCol = FrameBuffer.Width - (BorderFull.Right - _border.Right) - 1;

        using var lockedBitmap = Bitmap.Lock();
        var targetAddress = lockedBitmap.Address;

        for (var frameBufferRow = startFrameBufferRow; frameBufferRow <= endFrameBufferRow; frameBufferRow++)
        {
            var rowOffset = frameBufferRow * FrameBuffer.Width;

            for (var frameBufferCol = startFrameBufferCol; frameBufferCol <= endFrameBufferCol; frameBufferCol++)
            {
                var pixelIndex = rowOffset + frameBufferCol;
                var color = frameBuffer.Pixels[pixelIndex].Abgr;

                // Duplicate pixels horizontally
                for (var x = 0; x < _zoomX; x++)
                {
                    unsafe
                    {
                        *(int*)targetAddress = color;
                    }

                    targetAddress += 4;
                }
            }

            // Duplicate previous line vertically based on zoom factor
            var previousLine = targetAddress - lockedBitmap.RowBytes;
            for (var y = 0; y < _zoomY - 1; y++)
            {
                unsafe
                {
                    Buffer.MemoryCopy(previousLine.ToPointer(), targetAddress.ToPointer(), lockedBitmap.RowBytes, lockedBitmap.RowBytes);
                }

                targetAddress += lockedBitmap.RowBytes;
            }
        }
    }

    internal void SetBorderSize(BorderSize borderSize)
    {
        _border = borderSize switch
        {
            BorderSize.None => BorderNone,
            BorderSize.Small => BorderSmall,
            BorderSize.Medium => BorderMedium,
            BorderSize.Large => BorderLarge,
            _ => BorderFull,
        };

        Bitmap = CreateBitmap();
    }

    private WriteableBitmap CreateBitmap()
    {
        var height = FrameBuffer.Height - (BorderFull.Top - _border.Top) - (BorderFull.Bottom - _border.Bottom);
        var width = FrameBuffer.Width - (BorderFull.Left - _border.Left) - (BorderFull.Right - _border.Right);

        return new WriteableBitmap(
            new PixelSize(
                width * _zoomX,
                height * _zoomY),
            new Vector(96, 96),
            PixelFormats.Rgba8888);
    }

    public void Dispose()
    {
        Bitmap.Dispose();
    }
}