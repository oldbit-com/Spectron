using OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

namespace OldBit.Spectron.Emulation.Devices.Beta128;

internal static class TrdImage
{
    internal static void Read(string filePath)
    {
        var data = File.ReadAllBytes(filePath).AsSpan();

        var disk = new FloppyDisk(80, 2);

        for (var i = 0; i < data.Length; i += FloppyDisk.BytesPerSector)
        {
            var cylinderNo = i >> 13;
            var sideNo = (i >> 12) & 1;
            var sectorNo = ((i >> 8) & 0x0F) + 1;

            var sectorData = data.Slice(i, FloppyDisk.BytesPerSector);
            disk.WriteSector(cylinderNo, sideNo, sectorNo, sectorData);
        }
    }
}