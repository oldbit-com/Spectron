using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Szx.Blocks;

namespace OldBit.Spectron.Files.Szx;

/// <summary>
/// Represents a .szx file.
/// </summary>
public sealed class SzxFile
{
    /// <summary>
    /// Gets the SZX file header.
    /// </summary>
    public SzxHeader Header { get; private set; } = new();

    /// <summary>
    /// Gets or sets the program that created this zx-state file.
    /// </summary>
    public CreatorBlock? Creator { get; set; }

    /// <summary>
    /// Gets the Z80 registers and other internal state values.
    /// </summary>
    public Z80RegsBlock Z80Registers { get; private set; } = new();

    /// <summary>
    /// Gets the Spectrum's ULA state specifying the current border colour, memory paging status etc.
    /// </summary>
    public SpecRegsBlock SpecRegs { get; private set; } = new();

    /// <summary>
    /// Gets or sets the status of the ZX Printer.
    /// </summary>
    public ZxPrinterBlock? ZxPrinter { get; set; }

    /// <summary>
    /// Gets or sets the state of the Spectrum keyboard and any keyboard joystick emulation.
    /// </summary>
    public KeyboardBlock? Keyboard { get; set; }

    /// <summary>
    /// Gets or sets joystick setup for both players.
    /// </summary>
    public JoystickBlock? Joystick { get; set; }

    /// <summary>
    /// Gets or sets the custom ROM that has been installed for the current Spectrum model.
    /// </summary>
    public CustomRomBlock? CustomRom { get; set; }

    /// <summary>
    /// Gets a list of RAM pages.
    /// </summary>
    public List<RamPageBlock> RamPages { get; } = [];

    /// <summary>
    /// Gets or sets the Timex Sinclair screen mode and memory paging status.
    /// </summary>
    public TimexSinclairBlock? TimexSinclair { get; set; }

    /// <summary>
    /// Gets or sets the 64 colour replacement ULA state.
    /// </summary>
    public PaletteBlock? Palette { get; set; }

    /// <summary>
    /// Gets or sets the AY sound chip state.
    /// </summary>
    public AyBlock? Ay { get; set; }

    /// <summary>
    /// Gets or sets the virtual cassette recorder state and its contents.
    /// </summary>
    public TapeBlock? Tape { get; set; }

    /// <summary>
    /// Loads a SZX file from the given stream.
    /// </summary>
    /// <param name="stream">The stream containing the SZX data.</param>
    /// <returns>The loaded SzxFile object.</returns>
    public static SzxFile Load(Stream stream)
    {
        var reader = new ByteStreamReader(stream);
        var header = SzxHeader.Read(reader);

        if (header.Magic != BlockIds.Magic)
        {
            throw new InvalidDataException("Not a valid SZX file. Invalid magic number.");
        }

        var state = new SzxFile { Header = header };

        while (BlockHeader.Read(reader) is { } blockHeader)
        {
            switch (blockHeader.BlockId)
            {
                case BlockIds.Creator:
                    state.Creator = CreatorBlock.Read(reader, blockHeader.Size);
                    break;

                case BlockIds.Z80Regs:
                    state.Z80Registers = Z80RegsBlock.Read(reader, blockHeader.Size);
                    break;

                case BlockIds.SpecRegs:
                    state.SpecRegs = SpecRegsBlock.Read(reader, blockHeader.Size);
                    break;

                case BlockIds.ZxPrinter:
                    state.ZxPrinter = ZxPrinterBlock.Read(reader, blockHeader.Size);
                    break;

                case BlockIds.Keyboard:
                    state.Keyboard = KeyboardBlock.Read(reader, blockHeader.Size);
                    break;

                case BlockIds.Joystick:
                    state.Joystick = JoystickBlock.Read(reader, blockHeader.Size);
                    break;

                case BlockIds.CustomRom:
                    state.CustomRom = CustomRomBlock.Read(reader, blockHeader.Size);
                    break;

                case BlockIds.RamPage:
                    state.RamPages.Add(RamPageBlock.Read(reader, blockHeader.Size));
                    break;

                case BlockIds.TimexSinclair:
                    state.TimexSinclair = TimexSinclairBlock.Read(reader, blockHeader.Size);
                    break;

                case BlockIds.Ay:
                    state.Ay = AyBlock.Read(reader, blockHeader.Size);
                    break;

                case BlockIds.Palette:
                    state.Palette = PaletteBlock.Read(reader, blockHeader.Size);
                    break;

                case BlockIds.Tape:
                    state.Tape = TapeBlock.Read(reader, blockHeader.Size);
                    break;

                default:
                    // Ignore this block, not needed for now.
                    reader.ReadBytes(blockHeader.Size);
                    break;
            }
        }

        return state;
    }

    /// <summary>
    /// Loads a SZX file from the given file.
    /// </summary>
    /// <param name="fileName">The file containing the SZX data.</param>
    /// <returns>The loaded SzxFile object.</returns>
    public static SzxFile Load(string fileName)
    {
        using var stream = File.OpenRead(fileName);

        return Load(stream);
    }

    /// <summary>
    /// Saves the SZX file to a stream.
    /// </summary>
    /// <param name="stream">The stream to save the SZX data to.</param>
    public void Save(Stream stream)
    {
        Header.Write(stream);
        Creator?.Write(stream);
        Z80Registers.Write(stream);
        SpecRegs.Write(stream);
        CustomRom?.Write(stream);
        RamPages.ForEach(x => x.Write(stream));
        Ay?.Write(stream);
        Keyboard?.Write(stream);
        Joystick?.Write(stream);
        ZxPrinter?.Write(stream);
        TimexSinclair?.Write(stream);
        Palette?.Write(stream);
        Tape?.Write(stream);
    }

    /// <summary>
    /// Saves the SZX data to a file.
    /// </summary>
    /// <param name="fileName">The name of the file to save the SZX data to.</param>
    public void Save(string fileName)
    {
        using var stream = File.Create(fileName);

        Save(stream);
    }
}