using System.IO.Compression;
using OldBit.Spectron.Emulation.Extensions;

namespace OldBit.Spectron.Emulation.Files;

public record ArchiveEntry(string Name, FileType FileType);

public sealed class ZipArchiveReader(string filePath) : IDisposable
{
    private readonly ZipArchive _zip = ZipFile.OpenRead(filePath);

    public List<ArchiveEntry> GetSupportedFiles()
    {
        var entries = new List<ArchiveEntry>();

        foreach (var entry in _zip.Entries)
        {
            var fileType = FileTypeResolver.FromPath(entry.FullName);

            if (fileType.IsSnapshot() || fileType.IsTape() || fileType.IsMicrodrive() || fileType == FileType.Rzx)
            {
                entries.Add(new ArchiveEntry(entry.FullName, fileType));
            }
        }

        return entries;
    }

    public bool ContainsTapeFile() => _zip.Entries.Any(file => FileTypeResolver.FromPath(file.FullName).IsTape());

    public Stream? GetFile(string fullName) =>
        _zip.Entries.FirstOrDefault(e => e.FullName == fullName)?.Open();

    public void Dispose() => _zip.Dispose();
}