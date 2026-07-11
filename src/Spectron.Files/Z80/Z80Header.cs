using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Z80.Types;

namespace OldBit.Spectron.Files.Z80;

/// <summary>
/// Represents the .z80 file header. Depending on the version, it may contain additional data.
/// </summary>
public sealed class Z80Header
{
    private byte[] _data;

    /// <summary>
    /// Gets or sets the raw header data.
    /// </summary>
    internal byte[] Data
    {
        get => _data;
        set => _data = value;
    }

    /// <summary>
    /// Gets or sets the A register.
    /// </summary>
    public byte A
    {
        get => Data[0];
        set => Data[0] = value;
    }

    /// <summary>
    /// Gets or sets the F register.
    /// </summary>
    public byte F
    {
        get => Data[1];
        set => Data[1] = value;
    }

    /// <summary>
    /// Gets or sets the BC register.
    /// </summary>
    public Word BC
    {
        get => Data.GetWord(2);
        set => Data.SetWord(2, value);
    }

    /// <summary>
    /// Gets or sets the HL register.
    /// </summary>
    public Word HL
    {
        get => Data.GetWord(4);
        set => Data.SetWord(4, value);
    }

    /// <summary>
    /// Gets or sets the PC register.
    /// </summary>
    public Word PC
    {
        get => Data.GetWord(Version == 1 ? 6 : 32);
        set => Data.SetWord(Version == 1 ? 6 : 32, value);
    }

    /// <summary>
    /// Gets or sets the SP register.
    /// </summary>
    public Word SP
    {
        get => Data.GetWord(8);
        set => Data.SetWord(8, value);
    }

    /// <summary>
    /// Gets or sets the I register.
    /// </summary>
    public byte I
    {
        get => Data[10];
        set => Data[10] = value;
    }

    /// <summary>
    /// Gets or sets the R register. Bit 7 is applied from the Flags1 byte.
    /// </summary>
    public byte R
    {
        get => (byte)(Data[11] & 0x7F | (Data[12] & 0x01) << 7);
        set
        {
            Data[11] = value;
            Flags1.Bit7R = (byte)(value >> 7);
        }
    }

    /// <summary>
    /// Gets or sets miscellaneous flags 1.
    /// </summary>
    public Flags1 Flags1 => field ??= new Flags1(Data);

    /// <summary>
    /// Gets or sets the DE register.
    /// </summary>
    public Word DE
    {
        get => Data.GetWord(13);
        set => Data.SetWord(13, value);
    }

    /// <summary>
    /// Gets or sets the BC' register.
    /// </summary>
    public Word BCPrime
    {
        get => Data.GetWord(15);
        set => Data.SetWord(15, value);
    }

    /// <summary>
    /// Gets or sets the DE' register.
    /// </summary>
    public Word DEPrime
    {
        get => Data.GetWord(17);
        set => Data.SetWord(17, value);
    }

    /// <summary>
    /// Gets or sets the HL' register.
    /// </summary>
    public Word HLPrime
    {
        get => Data.GetWord(19);
        set => Data.SetWord(19, value);
    }

    /// <summary>
    /// Gets or sets the A' register.
    /// </summary>
    public byte APrime
    {
        get => Data[21];
        set => Data[21] = value;
    }

    /// <summary>
    /// Gets or sets the F' register.
    /// </summary>
    public byte FPrime
    {
        get => Data[22];
        set => Data[22] = value;
    }

    /// <summary>
    /// Gets or sets the IY register.
    /// </summary>
    public Word IY
    {
        get => Data.GetWord(23);
        set => Data.SetWord(23, value);
    }

    /// <summary>
    /// Gets or sets the IX register.
    /// </summary>
    public Word IX
    {
        get => Data.GetWord(25);
        set => Data.SetWord(25, value);
    }

    /// <summary>
    /// Gets or sets the Interrupt flip-flop: 0=DI, otherwise EI.
    /// </summary>
    public byte IFF1
    {
        get => Data[27];
        set => Data[27] = value;
    }

    /// <summary>
    /// Gets or sets the IFF2 value.
    /// </summary>
    public byte IFF2
    {
        get => Data[28];
        set => Data[28] = value;
    }

    /// <summary>
    /// Gets or sets miscellaneous flags 2.
    /// </summary>
    public Flags2 Flags2 => field ??= new Flags2(Data);

    /// <summary>
    /// Gets the version of the Z80 header.
    /// </summary>
    public int Version
    {
        get
        {
            if (Data[6] != 0 || Data[7] != 0)
            {
                return 1;
            }

            return _data[30] == 23 ? 2 : 3;
        }
    }

    /// <summary>
    /// Gets the length of the additional header data.
    /// </summary>
    public int ExtraHeaderLength => _data.Length switch
    {
        30 => 0,
        _ => Data.GetWord(30)
    };

    /// <summary>
    /// Gets or sets the byte at the specified index in the header data.
    /// </summary>
    /// <param name="index">The zero-based index of the byte to get or set.</param>
    /// <returns>The byte at the specified index.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown when the index is outside the bounds of the header data array.</exception>
    public byte this[int index]
    {
        get => Data[index];
        set => Data[index] = value;
    }

    /// <summary>
    /// Gets or sets the hardware mode (applies to version 2 and 3).
    /// </summary>
    public HardwareMode HardwareMode
    {
        get => (Version > 1 ? Data[34] : -1, Version) switch
        {
            (0, _) => HardwareMode.Spectrum48,
            (1, _) => HardwareMode.Spectrum48 | HardwareMode.Interface1,
            (2, _) => HardwareMode.SamRam,
            (3, 2) => HardwareMode.Spectrum128,
            (3, 3) => HardwareMode.Spectrum48 | HardwareMode.Mgt,
            (4, 2) => HardwareMode.Spectrum128 | HardwareMode.Interface1,
            (4, 3) => HardwareMode.Spectrum128,
            (5, 3) => HardwareMode.Spectrum128 | HardwareMode.Interface1,
            (6, 3) => HardwareMode.Spectrum128 | HardwareMode.Mgt,
            (7, _) => HardwareMode.SpectrumPlus3,
            (9, _) => HardwareMode.Pentagon128,
            (10, _) => HardwareMode.Scorpion256,
            (11, _) => HardwareMode.DidaktikKompakt,
            (12, _) => HardwareMode.SpectrumPlus2,
            (13, _) => HardwareMode.SpectrumPlus2A,
            (14, _) => HardwareMode.TC2048,
            (15, _) => HardwareMode.TC2068,
            (128, _) => HardwareMode.TS2068,
            _ => HardwareMode.None
        };
        set
        {
            if (Version == 1)
            {
                return;
            }

            Data[34] = (Version, value) switch
            {
                (_, HardwareMode.Spectrum48) => 0,
                (_, HardwareMode.Spectrum48 | HardwareMode.Interface1) => 1,
                (_, HardwareMode.SamRam) => 2,
                (2, HardwareMode.Spectrum128) => 3,
                (3, HardwareMode.Spectrum48 | HardwareMode.Mgt) => 3,
                (2, HardwareMode.Spectrum128 | HardwareMode.Interface1) => 4,
                (_, HardwareMode.Spectrum128) => 4,
                (_, HardwareMode.Spectrum128 | HardwareMode.Interface1) => 5,
                (_, HardwareMode.Spectrum128 | HardwareMode.Mgt) => 6,
                (_, HardwareMode.SpectrumPlus3) => 7,
                (_, HardwareMode.Pentagon128) => 9,
                (_, HardwareMode.Scorpion256) => 10,
                (_, HardwareMode.DidaktikKompakt) => 11,
                (_, HardwareMode.SpectrumPlus2) => 12,
                (_, HardwareMode.SpectrumPlus2A) => 13,
                (_, HardwareMode.TC2048) => 14,
                (_, HardwareMode.TC2068) => 15,
                (_, HardwareMode.TS2068) => 128,
                _ => 0
            };
        }
    }

    /// <summary>
    /// Gets sets the last byte written to the 0x7FFD port (.
    /// </summary>
    public byte Port7FFD
    {
        get => (byte)(Version > 1 ? Data[35] : 0);
        set
        {
            if (Version > 1)
            {
                Data[35] = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the interface 1 ROM page status.
    /// </summary>
    public bool IsInterface1RomPage
    {
        get => Version > 1 && Data[36] == 0xFF;
        set
        {
            if (Version > 1)
            {
                Data[36] = (byte)(value ? 0xFF : 0);
            }
        }
    }

    /// <summary>
    /// Gets sets the last byte written to the 0x7FFD port.
    /// </summary>
    public byte PortFFFD
    {
        get => (byte)(Version > 1 ? Data[38] : 0);
        set
        {
            if (Version > 1)
            {
                Data[38] = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the AY sound chip registers. These registers control the behavior
    /// of the AY-3-8910 sound chip in the Z80 snapshot format. The data consists of
    /// 16 bytes representing the state of the AY registers.
    /// </summary>
    public byte[] AyRegisters
    {
        get => Version > 1 ? Data[39..55] : [];
        set => Array.Copy(value, 0, Data, 39, 16);
    }

    /// <summary>
    /// Gets or sets miscellaneous flags 3.
    /// </summary>
    public Flags3? Flags3
    {
        get
        {
            if (Version == 1)
            {
                return null;
            }

            return field ??= new Flags3(Data);
        }
    }

    /// <summary>
    /// Gets or sets the low T state counter.
    /// </summary>
    public Word LowTStateCounter
    {
        get => Version < 3 ? (Word)0 : Data.GetWord(55);
        set
        {
            if (Version >= 3)
            {
                Data.SetWord(55, value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the high T state counter.
    /// </summary>
    public byte HighTStateCounter
    {
        get => Version < 3 ? (byte)0 : Data[57];
        set
        {
            if (Version >= 3)
            {
                Data[57] = value;
            }
        }
    }

    /// <summary>
    /// Creates a new instance of the Z80Header header.
    /// </summary>
    public Z80Header()
    {
        const int headerSize = 84;

        _data = new byte[headerSize + 2];

        Data.SetWord(30, headerSize - 30);
        Flags1?.IsDataCompressed = true;
    }

    /// <summary>
    /// Creates a new instance of the Z80Header header.
    /// </summary>
    /// <param name="reader">The ByteStreamReader used to initialize the Z80Header properties.</param>
    internal Z80Header(ByteStreamReader reader)
    {
        _data = reader.ReadBytes(30);
        if (_data[6] != 0 || _data[7] != 0)
        {
            // V1, no additional header
            return;
        }

        // V2 or greater, read additional header
        var extraHeaderLength = reader.ReadWord();
        var extraHeader = reader.ReadBytes(extraHeaderLength);

        // Resize and copy the additional header data
        Array.Resize(ref _data, 30 + extraHeaderLength + 2);

        // Write the extra header length
        _data[30] = (byte)(extraHeaderLength & 0xFF);
        _data[31] = (byte)(extraHeaderLength >> 8);

        // Copy the extra header data
        Array.Copy(extraHeader, 0, _data, 32, extraHeaderLength);
    }
}