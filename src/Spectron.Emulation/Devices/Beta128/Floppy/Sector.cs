namespace OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

internal sealed class Sector
{
    private const int CylinderByte = 1;
    private const int SideByte = 2;
    private const int SectorByte = 3;

    private readonly ArraySegment<byte> _id;   // 0xFE | C | H | R | N | CRC
    private readonly ArraySegment<byte> _data; // 0xFB | ...DATA... | CRC

    internal int IdPosition => _id.Offset + 1;
    internal byte CylinderNo => _id[CylinderByte];
    internal byte SectorNo => _id[SectorByte];
    internal byte SideNo => _id[SideByte];

    private Word IdCrc => (Word)(_id[5] << 8 | _id[6]);

    internal Sector(Track track, int idPosition, int dataPosition, int bytesPerSector)
    {
        _id = new ArraySegment<byte>(track.Data, idPosition - 1, 7);
        _data = new ArraySegment<byte>(track.Data, dataPosition - 1, 1 + bytesPerSector + 2);
    }

    internal byte this[int index]
    {
        get
        {
            ValidateDataIndex(index);
            return _data[index + 1];
        }
        set
        {
            ValidateDataIndex(index);
            _data[index + 1] = value;
        }
    }

    internal void UpdateCrc()
    {
        var crc = Crc.Calculate(_data[..^2]);

        _data[^2] = (byte)(crc >> 8);
        _data[^1] = (byte)crc;
    }

    internal bool VerifyIdCrc() => CalculateIdCrc() == IdCrc;

    private Word CalculateIdCrc() => Crc.Calculate(_id[..^2]);

    private static void ValidateDataIndex(int index)
    {
        if (index is < 0 or > 255)
        {
            throw new ArgumentException("Data index must be between 0 and 255");
        }
    }
}