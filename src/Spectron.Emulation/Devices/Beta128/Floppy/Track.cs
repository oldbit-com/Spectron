namespace OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

internal sealed class Track(int totalSectors)
{
    internal const int DataLength = 6098;

    private readonly Sector[] _sectors = new Sector[totalSectors];

    internal byte[] Data { get; } = new byte[DataLength];

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

    internal void Write(int position, byte value) => Data[position] = value;

    private static void ValidateSectorIndex(int sectorNo)
    {
        if (sectorNo is < 1 or > 16)
        {
            throw new ArgumentException("Sector index must be between 1 and 16");
        }
    }
}