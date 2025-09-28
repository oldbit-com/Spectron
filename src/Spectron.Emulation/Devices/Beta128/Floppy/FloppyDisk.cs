using OldBit.Spectron.Emulation.Devices.Beta128.Drive;

namespace OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

/// <summary>
/// Represents floppy disk formatted for TR-DOS. 16 sectors per track, 256 bytes per sector.
/// Sectors are interleaved. Compatible with 40 or 80 cylinders, 1 or 2 sides.
/// </summary>
internal sealed class FloppyDisk
{
    private const int TotalSectors = 16;
    private const int BytesPerSector = 256;

    private readonly byte[] _sectorInterleave = [1, 9, 2, 10, 3, 11, 4, 12, 5, 13, 6, 14, 7, 15, 8, 16];

    private readonly int _totalCylinders;
    private readonly int _totalSides;

    private readonly Track[][] _tracks;

    internal FloppyDisk(int totalCylinders, int totalSides)
    {
        _totalCylinders = totalCylinders;
        _totalSides = totalSides;

        _tracks = new Track[_totalCylinders][];
    }

    // Use standard IBM track layout for MFM disks
    internal void Initialize()
    {
        var size = (byte)Math.Log2(BytesPerSector / 128.0);

        for (byte cylinderNo = 0; cylinderNo < _totalCylinders; cylinderNo += 1)
        {
            _tracks[cylinderNo] = new Track[_totalSides];

            for (byte sideNo = 0; sideNo < _totalSides; sideNo += 1)
            {
                var position = 0;

                var track = new Track(TotalSectors);
                _tracks[cylinderNo][sideNo] = track;

                Write(ref position, track, 0x4E, 80);           // GAP 4A
                Write(ref position, track, 0x00, 12);           // SYNC 1
                Write(ref position, track, 0xC2, 3, true);      // IAM
                Write(ref position, track, 0xFC);               // IAM
                Write(ref position, track, 0x4E, 50);           // GAP 1

                for (byte sectorNo = 0; sectorNo < TotalSectors; sectorNo += 1)
                {
                    var interleaveSectorNo = _sectorInterleave[sectorNo];

                    Write(ref position, track, 0x00, 12);       // SYNC
                    Write(ref position, track, 0xA1, 3, true);  // IDAM
                    Write(ref position, track, 0xFE);           // IDAM

                    var idPosition = position;

                    Write(ref position, track, cylinderNo);            // C
                    Write(ref position, track, sideNo);                // H
                    Write(ref position, track, interleaveSectorNo);    // R
                    Write(ref position, track, size);                  // N

                    var crc = Crc.Calculate(track.Data.AsSpan().Slice(position - 5, 5));

                    Write(ref position, track, (byte)(crc >> 8));       // CRC
                    Write(ref position, track, (byte)crc);              // CRC
                    Write(ref position, track, 0x4E, 22);               // GAP 2
                    Write(ref position, track, 0x00, 12);               // SYNC
                    Write(ref position, track, 0xA1, 3, true);          // DAM
                    Write(ref position, track, 0xFB);                   // DAM

                    track.Sectors[interleaveSectorNo - 1] = new Sector(track, idPosition, position, BytesPerSector);
                    crc = Crc.Calculate(track.Data.AsSpan().Slice(position - 1, BytesPerSector + 1));
                    position += BytesPerSector;

                    Write(ref position, track, (byte)(crc >> 8));       // CRC
                    Write(ref position, track, (byte)crc);              // CRC
                }

                // TODO: This should be part of sector???
                Write(ref position, track, 0x4E, 80);               // GAP 3
            }
        }

        AddTrDosDiskInfo();
    }

    private void AddTrDosDiskInfo()
    {
        var totalFreeSectors = _totalCylinders * _totalSides * TotalSectors - 16;
        var sector = GetSector(0, 0, 9);

        sector[0xE1] = 0;
        sector[0xE2] = 1;       // E1:E2 First free sector address sec:track
        sector[0xE3] = DiskType.GetDiskType(_totalCylinders, _totalSides);
        sector[0xE5] = (byte)(totalFreeSectors & 0xFF);
        sector[0xE6] = (byte)(totalFreeSectors >> 8);
        sector[0xE7] = 0x10;    // TR-DOS id

        sector.UpdateCrc();
    }

    private Sector GetSector(int track, int side, int sector) =>
        _tracks[track][side].Sectors[sector - 1];

    private static void Write(ref int position, Track track, byte value, int count = 1, bool isMarker = false)
    {
        for (var i = 0; i < count; i++)
        {
            track.Write(position++, value, isMarker);
        }
    }
}