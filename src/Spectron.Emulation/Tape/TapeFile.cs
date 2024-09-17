using OldBit.Spectron.Emulation.Storage;
using OldBit.ZX.Files.Extensions;
using OldBit.ZX.Files.Tap;
using OldBit.ZX.Files.Tzx;
using OldBit.ZX.Files.Tzx.Blocks;

namespace OldBit.Spectron.Emulation.Tape;

public sealed class TapeFile : ITapeBlockDataProvider
{
    private int _blockIndex;

    public TzxFile CurrentFile { get; private set; } = new();

    public void Load(string filePath)
    {
        _blockIndex = 0;
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
        while (_blockIndex < CurrentFile.Blocks.Count)
        {
            var block = CurrentFile.Blocks[_blockIndex];
            _blockIndex += 1;

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
        if (_blockIndex >= CurrentFile.Blocks.Count)
        {
            return null;
        }

        var block = CurrentFile.Blocks[_blockIndex];
        _blockIndex += 1;

        return block;
    }

    public void SaveTape(string filePath)
    {
    }
}