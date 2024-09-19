using OldBit.Spectron.Emulation.Storage;
using OldBit.ZX.Files.Extensions;
using OldBit.ZX.Files.Tap;
using OldBit.ZX.Files.Tzx;
using OldBit.ZX.Files.Tzx.Blocks;

namespace OldBit.Spectron.Emulation.Tape;

public class TapePositionChangedEventArgs(int position) : EventArgs
{
    public int Position { get; } = position;
}

/// <summary>
/// Represents a virtual cassette that can contain multiple data files.
/// </summary>
public sealed class Cassette
{
    public int Position { get; set; }
    public TzxFile Content { get; private set; } = new();

    public delegate void TapePositionChangedEvent(TapePositionChangedEventArgs e);
    public event TapePositionChangedEvent? TapeBlockSelected;

    public void Load(string filePath)
    {
        Position = 0;
        var fileType = FileTypeHelper.GetFileType(filePath);

        switch (fileType)
        {
            case FileType.Tap:
                var tapFile = TapFile.Load(filePath);
                Load(tapFile.ToTzx());
                break;

            case FileType.Tzx:
                var tzxFile = TzxFile.Load(filePath);
                Load(tzxFile);
                break;
        }
    }

    internal void Load(TzxFile tzxFile, int blockIndex = 0)
    {
        Position = blockIndex;
        Content = tzxFile;
    }

    public TapData? GetNextTapData()
    {
        while (Position < Content.Blocks.Count)
        {
            var block = Content.Blocks[Position];
            Position += 1;

            if (block is not StandardSpeedDataBlock standardSpeedDataBlock)
            {
                continue;
            }

            if (TapData.TryParse(standardSpeedDataBlock.Data, out var tapData))
            {
                return tapData;
            }
        }

        return null;
    }

    public IBlock? GetNextBlock()
    {
        if (Position >= Content.Blocks.Count)
        {
            return null;
        }

        TapeBlockSelected?.Invoke(new TapePositionChangedEventArgs(Position));

        var block = Content.Blocks[Position];
        Position += 1;

        return block;
    }
}