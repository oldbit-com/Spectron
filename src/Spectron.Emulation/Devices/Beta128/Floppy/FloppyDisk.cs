using System.Text;
using OldBit.Spectron.Emulation.Devices.Beta128.Controller;

namespace OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

/// <summary>
/// Represents a floppy disk formatted for TR-DOS. 16 sectors per track, 256 bytes per sector.
/// Sectors are interleaved. Compatible with 40 or 80 cylinders, 1 or 2 sides.
/// The supported format is IBM34 MFM.
/// </summary>
internal sealed class FloppyDisk
{
    internal const int TotalSectors = 16;
    internal const int BytesPerSector = 256;

    private readonly byte[] _sectorInterleave = [1, 9, 2, 10, 3, 11, 4, 12, 5, 13, 6, 14, 7, 15, 8, 16];
    private readonly Track[][] _tracks;
    private readonly int _maxLogicalSector;

    internal Sector SystemSector { get; private set; } = null!;
    internal int TotalCylinders { get; }
    internal int TotalSides { get; }

    internal int TotalFreeSectors
    {
        get => (SystemSector[0xE5] | (SystemSector[0xE6] << 8));
        set
        {
            SystemSector[0xE5] = (byte)value;
            SystemSector[0xE6] = (byte)(value >> 8);
        }
    }

    internal int FirstFreeSector
    {
        get => SystemSector[0xE1] | (SystemSector[0xE2] << 4);
        set
        {
            SystemSector[0xE1] = (byte)(value & 0x0F);
            SystemSector[0xE2] = (byte)(value >> 4);
        }
    }

    internal byte TotalFiles
    {
        get => SystemSector[0xE4];
        set => SystemSector[0xE4] = value;
    }

    internal byte DiskType
    {
        get => SystemSector[0xE3];
        private set => SystemSector[0xE3] = value;
    }

    internal string Label
    {
        get => Encoding.ASCII.GetString(SystemSector.GetData(0xF5, 8));
        set => SystemSector.Write(0xF5, Encoding.ASCII.GetBytes(value.PadRight(8)[..8]));
    }

    internal FloppyDisk(int totalCylinders, int totalSides)
    {
        TotalCylinders = totalCylinders;
        TotalSides = totalSides;

        _tracks = new Track[TotalCylinders][];
        _maxLogicalSector = (TotalCylinders - 1) << 5 | (TotalSides - 1) << 4 | (TotalSectors - 1);

        Initialize();
    }

    internal Track GetTrack(int cylinderNo, int sideNo) => _tracks[cylinderNo][sideNo];

    internal Sector GetSector(int cylinderNo, int sideNo, int sectorNo) => _tracks[cylinderNo][sideNo][sectorNo];

    internal Sector GetLogicalSector(int logicalSector)
    {
        if (logicalSector < 0 || logicalSector > _maxLogicalSector)
        {
            throw new ArgumentOutOfRangeException(nameof(logicalSector));
        }

        var cylinderNo = logicalSector >> 5;
        var sideNo = (logicalSector >> 4) & 1;
        var sectorNo = 1 + (logicalSector & 0x0F);

        return _tracks[cylinderNo][sideNo][sectorNo];
    }

    private void Initialize()
    {
        var size = (byte)Math.Log2(BytesPerSector / 128.0);

        for (byte cylinderNo = 0; cylinderNo < TotalCylinders; cylinderNo++)
        {
            _tracks[cylinderNo] = new Track[TotalSides];

            for (byte sideNo = 0; sideNo < TotalSides; sideNo++)
            {
                var position = 0;

                var track = new Track(TotalSectors);
                _tracks[cylinderNo][sideNo] = track;

                Write(ref position, track, 0x4E, 80);           // GAP 4A
                Write(ref position, track, 0x00, 12);           // SYNC 1
                Write(ref position, track, 0xC2, 3);            // IAM
                Write(ref position, track, 0xFC);               // IAM
                Write(ref position, track, 0x4E, 50);           // GAP 1

                for (byte sectorNo = 0; sectorNo < TotalSectors; sectorNo++)
                {
                    var interleaveSectorNo = _sectorInterleave[sectorNo];

                    Write(ref position, track, 0x00, 12);       // SYNC
                    Write(ref position, track, 0xA1, 3);        // IDAM
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
                    Write(ref position, track, 0xA1, 3);                // DAM
                    Write(ref position, track, DataAddressMark.Normal); // DAM

                    track[interleaveSectorNo] = new Sector(track, idPosition, position, BytesPerSector);
                    crc = Crc.Calculate(track.Data.AsSpan().Slice(position - 1, BytesPerSector + 1));
                    position += BytesPerSector;

                    Write(ref position, track, (byte)(crc >> 8));       // CRC
                    Write(ref position, track, (byte)crc);              // CRC

                    Write(ref position, track, 0x4E, 54);               // GAP 3
                }
            }
        }

        AddTrDosDiskInfo();
    }

    private void AddTrDosDiskInfo()
    {
        SystemSector = _tracks[0][0][9];
        var totalFreeSectors = TotalCylinders * TotalSides * TotalSectors - 16;

        FirstFreeSector = 0x10;       // H1T0S0
        DiskType = DiskGeometry.GetIdentifier(TotalCylinders, TotalSides);
        TotalFreeSectors = totalFreeSectors;
        SystemSector[0xE7] = 0x10;    // TR-DOS id
        SystemSector.Write(0xE9, "         "u8);        // unused 9 x 0x20
        SystemSector.Write(0xF5, "        "u8);         // disk label (8)

        SystemSector.UpdateCrc();
    }

    internal void WriteSector(int trackNo, int sideNo, int sectorNo, ReadOnlySpan<byte> data)
    {
        var sector = _tracks[trackNo][sideNo][sectorNo];

        for (var i = 0; i < data.Length; i++)
        {
            sector[i] = data[i];
        }

        sector.UpdateCrc();
    }

    private static void Write(ref int position, Track track, byte value, int count = 1)
    {
        for (var i = 0; i < count; i++)
        {
            track.Write(position++, value);
        }
    }
}