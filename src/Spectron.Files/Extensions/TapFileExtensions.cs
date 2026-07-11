using OldBit.Spectron.Files.Serialization;
using OldBit.Spectron.Files.Tap;
using OldBit.Spectron.Files.Tzx;
using OldBit.Spectron.Files.Tzx.Blocks;

namespace OldBit.Spectron.Files.Extensions;

/// <summary>
/// Extensions for converting between TapFile and TzxFile.
/// </summary>
public static class TapFileExtensions
{
    /// <summary>
    /// Converts a TapFile to a TzxFile by serializing each block in the TapFile
    /// and adding it as a StandardSpeedDataBlock to the TzxFile.
    /// </summary>
    /// <param name="tapFile">The TapFile to convert.</param>
    /// <returns>A TzxFile containing the serialized blocks from the TapFile.</returns>
    public static TzxFile ToTzx(this TapFile tapFile)
    {
        var tzx = new TzxFile();

        foreach (var block in tapFile.Blocks)
        {
            tzx.Blocks.Add(new StandardSpeedDataBlock
            {
                Data = FileDataSerializer.Serialize(block).ToList(),
                PauseDuration = 1000,
            });
        }

        return tzx;
    }

    /// <summary>
    /// Converts a TzxFile to a TapFile, only including standard speed data blocks.
    /// Any other block types will be ignored since they cannot be represented in a TapFile.
    /// </summary>
    /// <param name="tzxFile">The TzxFile to convert.</param>
    /// <returns>A TapFile containing the standard speed data blocks.</returns>
    public static TapFile ToTap(this TzxFile tzxFile)
    {
        var tap = new TapFile();

        foreach (var block in tzxFile.Blocks.Where(b => b is StandardSpeedDataBlock).Cast<StandardSpeedDataBlock>())
        {
            if (TapData.TryParse(block.Data, out var tapData))
            {
                tap.Blocks.Add(tapData);
            }
        }

        return tap;
    }
}