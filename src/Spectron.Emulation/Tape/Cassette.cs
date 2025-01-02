using OldBit.Spectron.Emulation.Storage;
using OldBit.Spectron.Files.Extensions;
using OldBit.Spectron.Files.Tap;
using OldBit.Spectron.Files.Tzx;
using OldBit.Spectron.Files.Tzx.Blocks;

namespace OldBit.Spectron.Emulation.Tape;

public class BlockSelectedEventArgs(int position) : EventArgs
{
    public int Position { get; } = position;
}

/// <summary>
/// Represents a virtual cassette that can contain multiple data files.
/// </summary>
public sealed class Cassette
{
    private byte[]? _contentBytes = null;

    public int Position { get; set; }
    public TzxFile Content { get; private set; } = new();
    public bool IsEmpty => Content.Blocks.Count == 0;
    internal byte[] ContentBytes => GetContentBytes();

    public delegate void BlockSelectedEvent(BlockSelectedEventArgs e);
    public event BlockSelectedEvent? BlockSelected;

    public event EventHandler? EndOfTape;

    public void Load(Stream stream, FileType fileType)
    {
        Position = 0;

        switch (fileType)
        {
            case FileType.Tap:
                var tapFile = TapFile.Load(stream);
                Load(tapFile.ToTzx());
                break;

            case FileType.Tzx:
                var tzxFile = TzxFile.Load(stream);
                Load(tzxFile);
                break;
        }

        _contentBytes = null;
    }

    public void Load(string filePath)
    {
        Position = 0;
        var fileType = FileTypeHelper.GetFileType(filePath);

        using var stream = File.OpenRead(filePath);
        Load(stream, fileType);
    }

    internal void Load(TzxFile tzxFile, int blockIndex = 0)
    {
        Position = blockIndex;
        Content = tzxFile;
        _contentBytes = null;
    }

    public TapData? GetNextTapData()
    {
        while (Position < Content.Blocks.Count)
        {
            var block = Content.Blocks[Position];

            BlockSelected?.Invoke(new BlockSelectedEventArgs(Position));

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
            EndOfTape?.Invoke(this, EventArgs.Empty);
            return null;
        }

        var block = Content.Blocks[Position];

        BlockSelected?.Invoke(new BlockSelectedEventArgs(Position));

        Position += 1;

        return block;
    }

    private byte[] GetContentBytes()
    {
        if (_contentBytes != null)
        {
            return _contentBytes;
        }

        using var stream = new MemoryStream();
        Content.Save(stream);

        _contentBytes = stream.ToArray();

        return _contentBytes;
    }
}