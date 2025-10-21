using System.Text;
using OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

namespace OldBit.Spectron.Emulation.Devices.Beta128.Image;

internal sealed class SclDiskImage : IDiskImage
{
    private const int DirEntrySize = 16;
    private const int FileCount = 0x08;
    private const int FileSectors = 0x0D;
    private const int StartingSector = 0x0E;
    private const int LogicalTrack = 0x0F;

    private string? _fileName;

    public FloppyDisk Read(string filePath)
    {
        _fileName = Path.GetFileName(filePath);

        var data = File.ReadAllBytes(filePath).AsSpan();

        return Read(data);
    }

    public FloppyDisk Read(ReadOnlySpan<byte> data)
    {
        var signature = Encoding.ASCII.GetString(data[..8]);

        if (!string.Equals(signature, "SINCLAIR", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Invalid SCL file signature.");
        }

        var numberOfFiles = data[FileCount];

        if (numberOfFiles is > 128 or 0)
        {
            throw new ArgumentException("Too many files in SCL image.");
        }

        var disk = new FloppyDisk(80, 2);
        var totalFileSectors = disk.TotalFreeSectors;

        var fileDataStart = 9 + 14 * numberOfFiles;

        for (var i = 0; i < numberOfFiles; i++)
        {
            var fileHeader = GetFileHeader(data, i);

            var numberOfSectors = fileHeader[FileSectors];
            totalFileSectors -= numberOfSectors;

            var fileDataEnd = fileDataStart + numberOfSectors * FloppyDisk.BytesPerSector;
            var fileData = data[fileDataStart..fileDataEnd];
            fileDataStart = fileDataEnd;

            AddEntry(disk, fileHeader, fileData);
        }

        disk.TotalFreeSectors = totalFileSectors;
        disk.Label = _fileName ?? "SCL";

        disk.SystemSector.UpdateCrc();

        return disk;
    }

    private static void AddEntry(FloppyDisk disk, ReadOnlySpan<byte> header, ReadOnlySpan<byte> data)
    {
        var dirPos = disk.TotalFiles * DirEntrySize;
        var dirSector = disk.GetSector(0, 0, 1 + dirPos / FloppyDisk.BytesPerSector);
        dirPos &= 0xFF;

        // Write directory info
        dirSector.Write(dirPos, header);
        dirSector[dirPos + StartingSector] = disk.SystemSector[0xE1];
        dirSector[dirPos + LogicalTrack] = disk.SystemSector[0xE2];

        var firstFreeSector = disk.FirstFreeSector;
        var nextFreeSector = firstFreeSector + header[FileSectors];

        // Update disk info
        disk.FirstFreeSector = nextFreeSector;
        disk.TotalFiles += 1;
        disk.TotalFreeSectors -= header[FileSectors];

        dirSector.UpdateCrc();

        // Write data
        for (var i = 0; i < header[FileSectors]; i++)
        {
            var currentSector = firstFreeSector + i;

            var cylinderNo = currentSector >> 5;
            var sideNo = (currentSector >> 4) & 1;
            var sectorNo = 1 + (currentSector & 0x0F);

            var dataStart = i * FloppyDisk.BytesPerSector;
            var dataEnd = (i + 1) * FloppyDisk.BytesPerSector;

            disk.WriteSector(cylinderNo, sideNo, sectorNo, data[dataStart..dataEnd]);
        }
    }

    private static ReadOnlySpan<byte> GetFileHeader(ReadOnlySpan<byte> data, int index)
    {
        var fileHeaderStart = 9 + 14 * index;
        var fileHeaderEnd = fileHeaderStart + 14;

        return data[fileHeaderStart..fileHeaderEnd];
    }

    public void Write(FloppyDisk disk, Stream stream)
    {
        throw new NotImplementedException();
    }
}