namespace OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

internal sealed class Track(int totalSectors)
{
    internal const int DataLength = 6400; // = 5314;

    private readonly byte[] _data = new byte[DataLength];

    internal byte[] Data => _data;

    internal Sector[] Sectors { get; } = new Sector[totalSectors];

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
}