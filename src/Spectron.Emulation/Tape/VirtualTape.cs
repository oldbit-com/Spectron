using OldBit.Spectron.Emulation.Storage;
using OldBit.ZX.Files.Extensions;
using OldBit.ZX.Files.Tap;
using OldBit.ZX.Files.Tzx;
using OldBit.ZX.Files.Tzx.Blocks;

namespace OldBit.Spectron.Emulation.Tape;

public class TapeBlockSelectedEventArgs(int blockNumber) : EventArgs
{
    public int BlockNumber { get; } = blockNumber;
}

/// <summary>
/// Represents a virtual tape, e.g. a file that contains data that can be loaded into the ZX Spectrum.
/// </summary>
public sealed class VirtualTape
{
    public int BlockNumber { get; set; }
    public TzxFile CurrentFile { get; private set; } = new();

    public delegate void TapeBlockSelectedEvent(TapeBlockSelectedEventArgs e);
    public event TapeBlockSelectedEvent? TapeBlockSelected;

    public void Load(string filePath)
    {
        BlockNumber = 0;
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
        BlockNumber = blockIndex;
        CurrentFile = tzxFile;
    }

    public TapData? GetNextTapData()
    {
        while (BlockNumber < CurrentFile.Blocks.Count)
        {
            var block = CurrentFile.Blocks[BlockNumber];
            BlockNumber += 1;

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
        if (BlockNumber >= CurrentFile.Blocks.Count)
        {
            return null;
        }

        TapeBlockSelected?.Invoke(new TapeBlockSelectedEventArgs(BlockNumber));

        var block = CurrentFile.Blocks[BlockNumber];
        BlockNumber += 1;

        return block;
    }
}