namespace OldBit.Spectron.Emulation.Devices.Storage;

internal sealed class DiskImage : IDisposable
{
    private const int SectorSize = 512;

    private readonly BinaryReader _reader;
    private readonly string _filename;
    private readonly Dictionary<int, byte[]> _writeSectors = new();

    private uint _firstSectorOffset;

    internal uint TotalSectors { get; private set; }

    internal uint DiskSizeInBytes => TotalSectors * SectorSize;

    internal DiskImage(string filename)
    {
        _filename = filename;

        var file = File.Open(_filename, FileMode.Open, FileAccess.Read, FileShare.Read);
        _reader = new BinaryReader(file);

        ParseMbr();
    }

    internal byte[] ReadSector(int sector)
    {
        if (_writeSectors.TryGetValue(sector, out var data))
        {
            return data;
        }

        _reader.BaseStream.Seek(_firstSectorOffset + SectorSize * sector, SeekOrigin.Begin);

        data = _reader.ReadBytes(SectorSize);

        return data;
    }

    internal void WriteSector(int sector, byte[] data)
    {
        _writeSectors[sector] = data;
    }

    private void ParseMbr()
    {
        using var file = File.Open(_filename, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new BinaryReader(file);

        _reader.BaseStream.Seek(0, SeekOrigin.Begin);
        var mbr = _reader.ReadBytes(SectorSize);

        if (mbr[0x1FE] != 0x55 || mbr[0x1FF] != 0xAA)
        {
            throw new InvalidDataException("Invalid disk image file. MBR not found.");
        }

        var startLba = (uint)(mbr[0x1C6] | (mbr[0x1C7] << 8) | (mbr[0x1C8] << 16) | (mbr[0x1C9] << 24));
        TotalSectors = (uint)(mbr[0x1CA] | (mbr[0x1CB] << 8) | (mbr[0x1CC] << 16) | (mbr[0x1CD] << 24));

        _firstSectorOffset = startLba * SectorSize;
    }

    public void Dispose() => _reader.Dispose();
}