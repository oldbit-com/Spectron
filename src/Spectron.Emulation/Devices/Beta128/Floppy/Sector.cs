namespace OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

internal sealed class Sector
{
    private readonly ArraySegment<byte> _id;
    private readonly ArraySegment<byte> _data; // 0xFB | DATA | CRC

    internal Sector(Track track, int idPosition, int dataPosition, int bytesPerSector)
    {
        _id = new ArraySegment<byte>(track.Data, idPosition, 5);
       _data = new ArraySegment<byte>(track.Data, dataPosition - 1, 1 + bytesPerSector + 2);
    }

    internal byte this[int index]
    {
        get => _data[index + 1];
        set => _data[index + 1] = value;
    }

    internal void UpdateCrc()
    {
        var crc = Crc.Calculate(_data[..^2]);

        _data[^2] = (byte)(crc >> 8);
        _data[^1] = (byte)crc;
    }
}