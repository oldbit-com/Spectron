using System.Text;

namespace OldBit.Spectron.Emulation.Devices.Beta128.Disks;

internal sealed class SclToTrdFormatConverter
{
    internal static byte[] Convert(byte[] data)
    {
        var signature = Encoding.ASCII.GetString(data[..7]);

        if (signature != "SINCLAIR")
        {
            //
        }

        var fileCount = data[8];

        return data;
    }
}