namespace OldBit.Spectron.Emulation.Devices.DivMmc;

/// <summary>
/// Represents a SD card disk image file. It should be a raw image of a disk,
/// with MBR (Master Boot Record) at the beginning and 512-byte sectors, formatted
/// as FAT16 or FAT32. E.g. compatible with esxDOS.
/// </summary>
public sealed class DiskImage : IDisposable
{
    private const int SectorSize = 512;

    private readonly BinaryReader _reader;
    private readonly BinaryWriter _writer;
    private readonly string _fileName;
    private readonly Dictionary<int, byte[]> _writeSectors = new();

    private uint _firstSectorOffset;

    internal uint TotalSectors { get; private set; }

    internal uint DiskSizeInBytes => TotalSectors * SectorSize;

    internal bool IsWriteEnabled { get; set; }

    internal DiskImage(string fileName)
    {
        _fileName = fileName;

        var file = File.Open(_fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);

        _reader = new BinaryReader(file);
        _writer = new BinaryWriter(file);

        ParseMbr();
    }

    public static bool Validate(string fileName, out string? errorMessage)
    {
        try
        {
            using var diskImage = new DiskImage(fileName);
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            return false;
        }

        errorMessage = null;
        return true;
    }

    internal byte[] ReadSector(int sector)
    {
        if (_writeSectors.TryGetValue(sector, out var data))
        {
            return data;
        }

        var position = GetSectorPosition(sector);
        _reader.BaseStream.Seek(position, SeekOrigin.Begin);

        data = _reader.ReadBytes(SectorSize);

        return data;
    }

    internal void WriteSector(int sector, byte[] data)
    {
        if (IsWriteEnabled)
        {
            foreach (var (cachedSector, cachedData) in _writeSectors)
            {
                if (cachedSector == sector)
                {
                    continue;
                }

                WriteSectorPrivate(cachedSector, cachedData);
            }

            WriteSectorPrivate(sector, data);
        }
        else
        {
            _writeSectors[sector] = data;
        }
    }

    private void WriteSectorPrivate(int sector, byte[] data)
    {
        var position = GetSectorPosition(sector);

        _writer.BaseStream.Seek(position, SeekOrigin.Begin);
        _writer.Write(data);
    }

    private void ParseMbr()
    {
        using var file = File.Open(_fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
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

    private long GetSectorPosition(int sector) => _firstSectorOffset + SectorSize * sector;

    public void Dispose()
    {
        _reader.Dispose();
        _writer.Dispose();
    }
}