using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using OldBit.Spectron.Emulation.Screen;

namespace OldBit.Spectron.Screen;

/// <summary>
/// Converts and writes the frame buffer to a WriteableBitmap which Avalonia can display.
/// </summary>
internal sealed class FrameBufferConverter : IDisposable
{
    private Border _border = BorderSizes.Full;

    private int _startFrameBufferRow;
    private int _endFrameBufferRow;
    private int _startFrameBufferCol;
    private int _endFrameBufferCol;

    private readonly FrameBuffer _frameBuffer;

    internal WriteableBitmap ScreenBitmap { get; private set; }
    public bool IsHiRes => _frameBuffer.IsHiRes;

    internal FrameBufferConverter(FrameBuffer frameBuffer, BorderSize borderSize)
    {
        _frameBuffer = frameBuffer;
        SetBorderSize(borderSize);
    }

    internal void UpdateBitmap()
    {
        using var lockedBitmap = ScreenBitmap.Lock();

        var colCount = _endFrameBufferCol - _startFrameBufferCol + 1;
        var rowBytes = colCount * 4;

        unsafe
        {
            fixed (Color* pixelsBase = _frameBuffer.Pixels)
            {
                var destination = (byte*)lockedBitmap.Address;

                for (var row = _startFrameBufferRow; row <= _endFrameBufferRow; row++)
                {
                    var source = (byte*)(pixelsBase + row * _frameBuffer.Width + _startFrameBufferCol);

                    Buffer.MemoryCopy(source, destination, rowBytes, rowBytes);

                    destination += rowBytes;
                }
            }
        }
    }

    [MemberNotNull(nameof(ScreenBitmap))]
    internal void SetBorderSize(BorderSize borderSize)
    {
        _border = borderSize switch
        {
            BorderSize.None => BorderSizes.None,
            BorderSize.Small => BorderSizes.Small,
            BorderSize.Medium => BorderSizes.Medium,
            BorderSize.Large => BorderSizes.Large,
            _ => BorderSizes.Full,
        };

        // In hi-res mode the frame buffer border is doubled, so the skip amount scales accordingly.
        var borderMultiplier = _frameBuffer.Width / (ScreenSize.BorderLeft + ScreenSize.ContentWidth + ScreenSize.BorderRight);

        _startFrameBufferRow = BorderSizes.Max.Top - _border.Top;
        _endFrameBufferRow = _frameBuffer.Height - (BorderSizes.Max.Bottom - _border.Bottom) - 1;
        _startFrameBufferCol = (BorderSizes.Max.Left - _border.Left) * borderMultiplier;
        _endFrameBufferCol = _frameBuffer.Width - (BorderSizes.Max.Right - _border.Right) * borderMultiplier - 1;

        ScreenBitmap = CreateBitmap();
    }

    private WriteableBitmap CreateBitmap()
    {
        var height = _endFrameBufferRow - _startFrameBufferRow + 1;
        var width = _endFrameBufferCol - _startFrameBufferCol + 1;

        return new WriteableBitmap(
            new PixelSize(
                width,
                height),
            new Vector(96, 96),
            PixelFormats.Rgba8888);
    }

    public void Dispose() => ScreenBitmap.Dispose();
}