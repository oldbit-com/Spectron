using System.Diagnostics.CodeAnalysis;
using OldBit.Spectral.Emulation.Snapshot;
using OldBit.ZXTape.Extensions;
using OldBit.ZXTape.Tap;
using OldBit.ZXTape.Tzx;

namespace OldBit.Spectral.Emulation.Tape;

public sealed class FileLoader
{
    private readonly Emulator _emulator;

    internal FileLoader(Emulator emulator)
    {
        _emulator = emulator;
    }

    public static Emulator LoadSnapshot(string fileName, FileType fileType)
    {
        switch (fileType)
        {
            case FileType.Sna:
                return SnaFileLoader.Load(fileName);

            case FileType.Szx:
                var szxFileLoader = new SzxFileLoader();
                throw new NotImplementedException();

            case FileType.Z80:
                // var z80FileLoader = new Z80FileLoader();
                // z80FileLoader.Load(fileName);
                throw new NotImplementedException();

            default:
                throw new NotSupportedException($"The file extension '{Path.GetExtension(fileName)}' is not supported.");
        }
    }

    public void LoadFile(string fileName)
    {
        var fileType = FileTypeHelper.GetFileType(fileName);

        switch (fileType)
        {
            case FileType.Tap:
                LoadTap(fileName);
                break;

            case FileType.Tzx:
                LoadTzx(fileName);
                break;

            default:
                throw new NotSupportedException($"The file extension '{Path.GetExtension(fileName)}' is not supported.");
        }
    }

    public static bool TryLoadTape(string fileName, [NotNullWhen(true)] out TzxFile? tzxFile)
    {
        tzxFile = null;

        if (IsTapFile(fileName))
        {
            var tapFile = TapFile.Load(fileName);
            tzxFile = tapFile.ToTzx();
        }
        else if (IsTzxFile(fileName))
        {
            tzxFile = TzxFile.Load(fileName);
        }

        return tzxFile != null;
    }

    private void LoadTzx(string fileName)
    {
        var tzxFile = TzxFile.Load(fileName);

        //_tapePlayer.LoadTape(tzxFile);
    }

    private void LoadTap(string fileName)
    {
        var tapFile = TapFile.Load(fileName);

        //_tapePlayer.LoadTape(tapFile.ToTzx());
    }

    private static bool IsTapFile(string fileName) => Path.GetExtension(fileName).Equals(".tap", StringComparison.InvariantCultureIgnoreCase);
    private static bool IsTzxFile(string fileName) => Path.GetExtension(fileName).Equals(".tzx", StringComparison.InvariantCultureIgnoreCase);
}