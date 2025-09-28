namespace OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

internal sealed class Track
{
    private readonly byte[] _data = new byte[6400]; // TODO: Update length

    internal byte[] Data => _data;

    internal Sector[] Sectors { get; }

    public Track(int numberOfSectors)
    {
        Sectors = new Sector[numberOfSectors];
    }

    internal void Write(int position, byte value, bool isMarker = false)
    {
        _data[position] = value;

        if (isMarker)
        {

        }
    }
}