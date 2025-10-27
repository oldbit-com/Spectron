using OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

namespace OldBit.Spectron.Emulation.Devices.Beta128.Image;

internal sealed class TrdDiskImage : IDiskImage
{
    public FloppyDisk Read(string filePath)
    {
        var data = File.ReadAllBytes(filePath).AsSpan();

        return Read(data);
    }

    public FloppyDisk Read(ReadOnlySpan<byte> data)
    {
        var disk = new FloppyDisk(80, 2);

        for (var i = 0; i < data.Length; i += FloppyDisk.BytesPerSector)
        {
            var cylinderNo = i >> 13;
            var sideNo = (i >> 12) & 1;
            var sectorNo = ((i >> 8) & 0x0F) + 1;

            var sectorData = data.Slice(i, FloppyDisk.BytesPerSector);
            disk.WriteSector(cylinderNo, sideNo, sectorNo, sectorData);
        }

        return disk;
    }

    public void Write(FloppyDisk disk, Stream stream)
    {
        for (var cylinderNo = 0; cylinderNo < disk.TotalCylinders; cylinderNo++)
        {
            for (var sideNo = 0; sideNo < disk.TotalSides; sideNo++)
            {
                var track = disk.GetTrack(cylinderNo, sideNo);

                for (var sectorNo = 1; sectorNo <= FloppyDisk.TotalSectors; sectorNo++)
                {
                    var sectorData = track[sectorNo].GetData().ToArray();

                    stream.Write(sectorData, 0, sectorData.Length);
                }
            }
        }

        stream.Flush();
    }
}