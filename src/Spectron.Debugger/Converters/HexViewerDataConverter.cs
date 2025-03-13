using System.Globalization;
using Avalonia.Data.Converters;
using OldBit.Spectron.Debugger.Controls;
using OldBit.Spectron.Debugger.Extensions;

namespace OldBit.Spectron.Debugger.Converters;

public class HexViewerDataConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        var hexRows = new List<HexViewerDataRow>();

        if (values.Count != 2 || values.Any(x => x == null) || values[0] is not byte[] byteArray || values[1] is not int bytesPerRow)
        {
            return hexRows;
        }

        var chunks = byteArray.ToChunks(bytesPerRow);
        hexRows.AddRange(
            chunks.Select((chunk, i) => new HexViewerDataRow(
                Address: FormatAddress(bytesPerRow * i, byteArray.Length),
                Cells: chunk.Select((byteValue, index) => new HexViewerCell
                {
                    ColumnIndex = index,
                    RowIndex = i,
                    Value = byteValue
                }).ToArray())));

        return hexRows;
    }

    private static string FormatAddress(int number, int size) => size switch
    {
        <= 0xFFFF => $"{number:X4}",
        < 0xFFFFFF => $"{number:X6}",
        _ => $"{number:X8}"
    };
}