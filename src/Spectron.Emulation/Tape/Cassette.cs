using OldBit.Spectron.Emulation.Files;
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
    private byte[]? _contentBytes;
    private int _marker;

    public int Position { get; private set; }

    public TzxFile Content { get; private set; } = new();

    public bool IsEmpty => Content.Blocks.Count == 0;

    internal byte[] ContentBytes => GetContentBytes();

    public delegate void BlockSelectedEvent(BlockSelectedEventArgs e);
    public event BlockSelectedEvent? BlockSelected;
    public event EventHandler? EndOfTape;

    internal void SetContent(Stream stream, FileType fileType)
    {
        Position = 0;

        switch (fileType)
        {
            case FileType.Tap:
                var tapFile = TapFile.Load(stream);
                SetContent(tapFile.ToTzx());

                break;

            case FileType.Tzx:
                var tzxFile = TzxFile.Load(stream);
                SetContent(tzxFile);

                break;
        }

        _contentBytes = null;
    }

    internal void SetContent(TzxFile tzxFile, int currentPosition = 0)
    {
        Position = currentPosition;
        Content = tzxFile;
        _contentBytes = null;
    }

    internal TapData? GetNextTapData()
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

    internal IBlock? GetNextBlock()
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

    internal void Rewind()
    {
        Position = 0;
        BlockSelected?.Invoke(new BlockSelectedEventArgs(Position));
    }

    internal void SetMarker() => _marker = Position;

    internal void GotoMarker() => Position = _marker;

    public void SetPosition(int position)
    {
        Position = position;
        BlockSelected?.Invoke(new BlockSelectedEventArgs(Position));
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