using OldBit.Spectron.Emulation.Storage;
using OldBit.ZX.Files.Extensions;
using OldBit.ZX.Files.Tap;
using OldBit.ZX.Files.Tzx;
using OldBit.ZX.Files.Tzx.Blocks;

namespace OldBit.Spectron.Emulation.Tape;

public sealed class TapeFile : ITapDataProvider, ITapeBlockDataProvider
{
    private int _currentBlockIndex;
    private TzxFile _file  = new();

    public void Load(string filePath)
    {
        _currentBlockIndex = 0;
        var fileType = FileTypeHelper.GetFileType(filePath);

        switch (fileType)
        {
            case FileType.Tap:
                var tapFile = TapFile.Load(filePath);
                _file = tapFile.ToTzx();
                break;

            case FileType.Tzx:
                _file = TzxFile.Load(filePath);
                break;
        }
    }

    public TapData? GetNextTapData()
    {
        while (_currentBlockIndex < _file.Blocks.Count)
        {
            var block = _file.Blocks[_currentBlockIndex];
            _currentBlockIndex += 1;

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
        if (_currentBlockIndex >= _file.Blocks.Count)
        {
            return null;
        }

        var block = _file.Blocks[_currentBlockIndex];
        _currentBlockIndex += 1;

        return block;
    }

    public void SaveTape(string filePath)
    {
    }
}