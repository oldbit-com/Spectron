namespace OldBit.Spectron.Emulation.Devices.Storage.SD;

/// <summary>
/// Wrapper for SD card disk image.
/// </summary>
internal sealed class SdCard : IDisposable
{
    private readonly DiskImage _diskImage;

    internal uint DiskSizeInBytes => _diskImage.DiskSizeInBytes;

    internal uint TotalSectors => _diskImage.TotalSectors;

    internal SdCard(string filename) => _diskImage = new DiskImage(filename);

    internal byte[] ReadSector(int sector) =>  _diskImage.ReadSector(sector);

    internal void WriteSector(int sector, byte[] data) => _diskImage.WriteSector(sector, data);

    public void Dispose() => _diskImage.Dispose();
}