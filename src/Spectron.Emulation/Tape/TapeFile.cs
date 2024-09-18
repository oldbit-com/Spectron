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

public sealed class TapeFile : ITapeBlockDataProvider
{
    public int BlockIndex { get; set; }
    public TzxFile CurrentFile { get; private set; } = new();

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
                CurrentFile = tapFile.ToTzx();
                break;

            case FileType.Tzx:
                CurrentFile = TzxFile.Load(filePath);
                break;
        }
    }

    public TapData? GetNextTapData()
    {
        while (BlockIndex < CurrentFile.Blocks.Count)
        {
            var block = CurrentFile.Blocks[BlockIndex];
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
        if (BlockIndex >= CurrentFile.Blocks.Count)
        {
            return null;
        }

        TapeBlockSelected?.Invoke(new TapeBlockSelectedEventArgs(BlockIndex));

        var block = CurrentFile.Blocks[BlockIndex];
        BlockIndex += 1;

        return block;
    }

    public void SaveTape(string filePath)
    {
    }
}