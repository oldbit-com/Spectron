using System.Text;

namespace OldBit.Spectron.Debugger.Controls.Hex;

internal record RowTextLayout(int Position, int Width);

internal sealed class RowTextBuilder
{
    private readonly IAsciiFormatter _asciiFormatter;
    private readonly int _groupSize;
    private readonly int _bytesPerRow;
    private readonly RowTextLayout[] _layout;

    internal double CharWidth { get; }
    internal bool IsOffsetVisible { get; }

    public RowTextBuilder(
        IAsciiFormatter asciiFormatter,
        bool isOffsetVisible,
        int groupSize,
        int bytesPerRow,
        double charWidth)
    {
        IsOffsetVisible = isOffsetVisible;
        _asciiFormatter = asciiFormatter;
        _groupSize = groupSize;
        _bytesPerRow = bytesPerRow;
        CharWidth = charWidth;

        _layout = CreateLayout().ToArray();
    }

    internal string Build(ReadOnlySpan<byte> data, int offset)
    {
        var hex = new StringBuilder();
        var ascii = new StringBuilder();

        var address = IsOffsetVisible ? $"{offset:X4}: " : string.Empty;
        hex.Append(address);

        for (var i = 0; i < _bytesPerRow; i++)
        {
            if (i > 0)
            {
                hex.Append(' ');
            }

            if (IsGroupSpacer(i))
            {
                hex.Append(' ');
            }

            hex.Append(i >= data.Length ? "  " : data[i].ToString("X2"));
            ascii.Append(i >= data.Length ? ' ' : _asciiFormatter.Format(data[i]));
        }

        hex.Append("  ");
        hex.Append(ascii);

        return hex.ToString();
    }

    internal string BuildHeader()
    {
        var hex = new StringBuilder();

        var address = IsOffsetVisible ? "      " : string.Empty;
        hex.Append(address);

        for (var i = 0; i < _bytesPerRow; i++)
        {
            if (i > 0)
            {
                hex.Append(' ');
            }

            if (IsGroupSpacer(i))
            {
                hex.Append(' ');
            }

            hex.Append(i.ToString("X2"));
        }

        return hex.ToString();
    }

    internal int? GetIndexFromPosition(double x)
    {
        var item = _layout
            .Select((value, index) => new { Item = value, Index = index })
            .FirstOrDefault(p =>
                x > p.Item.Position * CharWidth &&
                x < (p.Item.Position + p.Item.Width) * CharWidth);

        return item?.Index % _bytesPerRow;
    }

    internal RowTextLayout GetLayout(int index) => _layout[index];

    internal static int CalculateTotalLength(bool isOffsetVisible, int groupSize, int bytesPerRow)
    {
        // Offset  "F000: "
        var length = isOffsetVisible ? 6 : 0;

        // Hex bytes
        length += bytesPerRow * 3;

        // Group extra gap
        if (groupSize > 1)
        {
            length += bytesPerRow / groupSize - 1;
        }

        // Before ASCII gap
        length += 2;

        // ASCII "ABCDEFGHIJKLMNOP"
        length += bytesPerRow;

        return length;
    }

    private IEnumerable<RowTextLayout> CreateLayout()
    {
        var position = IsOffsetVisible ? 6 : 0;

        for (var i = 0; i < _bytesPerRow; i++)
        {
            if (IsGroupSpacer(i))
            {
                position += 1;
            }

            yield return new RowTextLayout (position, 2);
            position += 3;
        }

        position += 1;

        for (var i = 0; i < _bytesPerRow; i++)
        {
            yield return new RowTextLayout (position++, 1);
        }
    }

    private bool IsGroupSpacer(int index) => _groupSize > 0 & index > 0 && index % _groupSize == 0;
}