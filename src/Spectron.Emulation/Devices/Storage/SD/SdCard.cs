namespace OldBit.Spectron.Emulation.Devices.Storage.SD;

internal sealed class SdCard : IDisposable
{
    private readonly DiskImage _diskImage;

    internal uint DiskSizeInBytes => _diskImage.DiskSizeInBytes;

    internal uint TotalSectors => _diskImage.TotalSectors;

    internal SdCard(string filename) => _diskImage = new DiskImage(filename);

    internal byte[] ReadSector(int sector) => _diskImage.ReadSector(sector);

    public void Dispose() => _diskImage.Dispose();
}