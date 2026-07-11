using OldBit.Spectron.Files.Extensions;
using OldBit.Spectron.Files.IO;

namespace OldBit.Spectron.Files.Szx;

/// <summary>
/// The zx-state header appears right at the start of a zx-state (.szx) file.
/// </summary>
public sealed class SzxHeader
{
    /// <summary>
    /// ZX Spectrum 16K.
    /// </summary>
    public const byte MachineId16K = 0;

    /// <summary>
    /// ZX Spectrum 48K.
    /// </summary>
    public const byte MachineId48K = 1;

    /// <summary>
    /// ZX Spectrum 128K.
    /// </summary>
    public const byte MachineId128K = 2;

    /// <summary>
    /// ZX Spectrum +2.
    /// </summary>
    public const byte MachineIdPlus2 = 3;

    /// <summary>
    /// ZX Spectrum +2A.
    /// </summary>
    public const byte MachineIdPlus2A = 4;

    /// <summary>
    /// ZX Spectrum +3.
    /// </summary>
    public const byte MachineIdPlus3 = 5;

    /// <summary>
    /// ZX Spectrum +3E.
    /// </summary>
    public const byte MachineIdPlus3E = 6;

    /// <summary>
    /// Pentagon 128.
    /// </summary>
    public const byte MachineIdPentagon128 = 7;

    /// <summary>
    /// Timex TC2048.
    /// </summary>
    public const byte MachineIdTc2048 = 8;

    /// <summary>
    /// Timex TC2068.
    /// </summary>
    public const byte MachineIdTc2068 = 9;

    /// <summary>
    /// Scorpion.
    /// </summary>
    public const byte MachineIdScorpion = 10;

    /// <summary>
    /// ZX Spectrum SE.
    /// </summary>
    public const byte MachineIdSe = 11;

    /// <summary>
    /// Timex TS2068.
    /// </summary>
    public const byte MachineIdTs2068 = 12;

    /// <summary>
    /// Pentagon 512K.
    /// </summary>
    public const byte MachineIdPentagon512 = 13;

    /// <summary>
    /// Pentagon 1024K.
    /// </summary>
    public const byte MachineIdPentagon1024 = 14;

    /// <summary>
    /// ZX Spectrum 48K NTSC.
    /// </summary>
    public const byte MachineIdNtsc48K = 15;

    /// <summary>
    /// ZX Spectrum 128 KE.
    /// </summary>
    public const byte MachineId128Ke = 16;

    /// <summary>
    /// Alternate timings.
    /// </summary>
    public const byte FlagsAlternateTimings = 1;

    /// <summary>
    /// Byte sequence of 'Z', 'X', 'S', 'T' to identify the file as a zx-state file.
    /// </summary>
    public DWord Magic { get; private set; } = BlockIds.Magic;

    /// <summary>
    /// Major version number of the file format. Currently, 1.
    /// </summary>
    public byte MajorVersion { get; set; } = 1;

    /// <summary>
    /// Minor version number of the file format. Currently, 4.
    /// </summary>
    public byte MinorVersion { get; set; } = 4;

    /// <summary>
    /// The model of ZX Spectrum (or clone) to switch to when loading the file.
    /// </summary>
    public byte MachineId { get; set; } = MachineId48K;

    /// <summary>
    /// Various flags.
    /// </summary>
    public byte Flags { get; set; }

    internal static SzxHeader Read(ByteStreamReader reader) => new()
    {
        Magic = reader.ReadDWord(),
        MajorVersion = reader.ReadByte(),
        MinorVersion = reader.ReadByte(),
        MachineId = reader.ReadByte(),
        Flags = reader.ReadByte(),
    };

    internal void Write(Stream writer)
    {
        writer.WriteDWord(Magic);
        writer.WriteByte(MajorVersion);
        writer.WriteByte(MinorVersion);
        writer.WriteByte(MachineId);
        writer.WriteByte(Flags);
    }
}