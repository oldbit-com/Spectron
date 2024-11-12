using System.IO.Compression;

namespace OldBit.Spectron.Emulation.Storage;

public record ArchiveEntry(string Name, FileType FileType);

public sealed class CompressedFile(string filePath) : IDisposable
{
    private readonly ZipArchive _zip = ZipFile.OpenRead(filePath);

    public List<ArchiveEntry> GetFiles()
    {
        var entries = new List<ArchiveEntry>();

        foreach (var entry in _zip.Entries)
        {
            var fileType = FileTypeHelper.GetFileType(entry.FullName);

            if (fileType.IsSnapshot() || fileType.IsTape())
            {
                entries.Add(new ArchiveEntry(entry.FullName, fileType));
            }
        }

        return entries;
    }

    public Stream? GetFile(string fullName) =>
        _zip.Entries.FirstOrDefault(e => e.FullName == fullName)?.Open();

    public void Dispose() => _zip.Dispose();
}