using OldBit.Spectron.Emulation.Storage;
using OldBit.ZX.Files.Extensions;
using OldBit.ZX.Files.Tap;
using OldBit.ZX.Files.Tzx;
using OldBit.ZX.Files.Tzx.Blocks;

namespace OldBit.Spectron.Emulation.Tape;

public class TapeBlockSelectedEventArgs(int blockIndex) : EventArgs
{
    public int BlockIndex { get; } = blockIndex;
}

/// <summary>
/// Represents a virtual tape, e.g. a file that contains data that can be loaded into the ZX Spectrum.
/// </summary>
public sealed class TapeFile
{
    public int BlockIndex { get; set; }
    public TzxFile FileImage { get; private set; } = new();

    public delegate void TapeBlockSelectedEvent(TapeBlockSelectedEventArgs e);
    public event TapeBlockSelectedEvent? TapeBlockSelected;

    public void Load(string filePath)
    {
        BlockIndex = 0;
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

    internal void Load(TzxFile tzxFile)
    {
        BlockIndex = 0;
        FileImage = tzxFile;
    }

    public TapData? GetNextTapData()
    {
        while (BlockIndex < FileImage.Blocks.Count)
        {
            var block = FileImage.Blocks[BlockIndex];
            BlockIndex += 1;

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
        if (BlockIndex >= FileImage.Blocks.Count)
        {
            return null;
        }

        TapeBlockSelected?.Invoke(new TapeBlockSelectedEventArgs(BlockIndex));

        var block = FileImage.Blocks[BlockIndex];
        BlockIndex += 1;

        return block;
    }

    public void SaveTape(string filePath)
    {
    }
}