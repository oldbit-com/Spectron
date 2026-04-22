using OldBit.Spectron.Emulation.Screen;
using SkiaSharp;

namespace OldBit.Spectron.Integration.Tests.Fixtures;

public sealed class ScreenConverter : IDisposable
{
    private readonly int _startRow;
    private readonly int _endRow;
    private readonly int _startCol;
    private readonly int _endCol;
    private readonly FrameBuffer _frameBuffer;

    private SKBitmap ScreenBitmap { get; }

    internal ScreenConverter(FrameBuffer frameBuffer)
    {
        _frameBuffer = frameBuffer;

        _startRow = 1;
        _endRow = _frameBuffer.Height - 2;
        _startCol = 0;
        _endCol = _frameBuffer.Width - 1;

        var width = _endCol - _startCol + 1;
        var height = _endRow - _startRow + 1;

        ScreenBitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque);
    }

    internal void UpdateBitmap()
    {
        var bitmapRow = 0;

        for (var row = _startRow; row <= _endRow; row++)
        {
            var rowOffset = row * _frameBuffer.Width;
            var bitmapCol = 0;

            for (var col = _startCol; col <= _endCol; col++)
            {
                ref var c = ref _frameBuffer.Pixels[rowOffset + col];
                ScreenBitmap.SetPixel(bitmapCol, bitmapRow, new SKColor(c.Red, c.Green, c.Blue, c.Alpha));

                bitmapCol++;
            }

            bitmapRow++;
        }
    }

    internal void SaveBitmap(Stream stream)
    {
        using var image = SKImage.FromBitmap(ScreenBitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        data.SaveTo(stream);
    }

    internal void SaveBitmap(string filePath)
    {
        using var image = SKImage.FromBitmap(ScreenBitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        using var stream = File.OpenWrite(filePath);
        data.SaveTo(stream);
    }

    public void Dispose() => ScreenBitmap.Dispose();
}