namespace OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

internal sealed class Track(int totalSectors)
{
    internal const int DataLength = 6400; // = 5314;

    private readonly byte[] _data = new byte[DataLength];
    private readonly Sector[] _sectors = new Sector[totalSectors];

    internal byte[] Data => _data;

    internal Sector this[int sectorNo]
    {
        get
        {
            ValidateSectorIndex(sectorNo);
            return _sectors[sectorNo - 1];
        }
        set
        {
            ValidateSectorIndex(sectorNo);
            _sectors[sectorNo - 1] = value;
        }
    }

    internal void Write(int position, byte value, bool isMarker = false)
    {
        _data[position] = value;

        // if (isMarker)
        // {
        //     _id[position / 8] |= (byte)(1 << (position & 7));
        // }
        // else
        // {
        //     _id[position / 8] &= (byte)~(1 << (position & 7));
        // }
    }

    private static void ValidateSectorIndex(int index)
    {
        if (index is < 1 or > 16)
        {
            throw new ArgumentException("Sector index must be between 1 and 16");
        }
    }
}