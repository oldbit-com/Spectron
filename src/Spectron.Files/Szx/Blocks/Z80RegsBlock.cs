using OldBit.Spectron.Files.Extensions;
using OldBit.Spectron.Files.IO;

namespace OldBit.Spectron.Files.Szx.Blocks;

/// <summary>
/// This block ('Z','8','0','R') contains the Z80 registers and other internal state values.
/// It does not contain any specific model registers.
/// </summary>
public sealed class Z80RegsBlock
{
    /// <summary>
    /// The last instruction executed was an EI instruction or an invalid $DD or $FD prefix.
    /// <remarks>ZXSTZF_EILAST</remarks>
    /// </summary>
    public const byte FlagsEiLast = 1;

    /// <summary>
    /// The last instruction executed was a HALT instruction. The CPU is currently executing NOPs and
    /// will continue to do so until the next interrupt occurs.
    /// This flag is mutually exclusive with ZXSTZF_EILAST.
    /// <remarks>ZXSTZF_HALTED</remarks>
    /// </summary>
    public const byte FlagsHalted = 2;

    /// <summary>
    /// Gets or sets he contents of the AF register.
    /// </summary>
    public Word AF { get; set; }

    /// <summary>
    /// Gets or sets the contents of the BC register.
    /// </summary>
    public Word BC { get; set; }

    /// <summary>
    /// Gets or sets the contents of the DE register.
    /// </summary>
    public Word DE { get; set; }

    /// <summary>
    /// Gets or sets the contents of the HL register.
    /// </summary>
    public Word HL { get; set; }

    /// <summary>
    /// Gets or sets the  contents of the AF' register.
    /// </summary>
    public Word AF1 { get; set; }

    /// <summary>
    /// Gets or sets the  contents of the BC' register.
    /// </summary>
    public Word BC1 { get; set; }

    /// <summary>
    /// Gets or sets the  contents of the DE' register.
    /// </summary>
    public Word DE1 { get; set; }

    /// <summary>
    /// Gets or sets the  contents of the HL' register.
    /// </summary>
    public Word HL1 { get; set; }

    /// <summary>
    /// Gets or sets the contents of the IX register.
    /// </summary>
    public Word IX { get; set; }

    /// <summary>
    /// Gets or sets the contents of the IY register.
    /// </summary>
    public Word IY { get; set; }

    /// <summary>
    /// Gets or sets the contents of the stack pointer, SP.
    /// </summary>
    public Word SP { get; set; }

    /// <summary>
    /// Gets or sets the current value of the program counter, PC.
    /// </summary>
    public Word PC { get; set; }

    /// <summary>
    /// Gets or sets the contents of the I register.
    /// </summary>
    public byte I { get; set; }

    /// <summary>
    /// Gets or sets the contents of the R register.
    /// </summary>
    public byte R { get; set; }

    /// <summary>
    /// Gets or sets the value of Interrupt Flip Flop 1 (IFF1). This is guaranteed to be either 0 or 1.
    /// </summary>
    public byte IFF1 { get; set; }

    /// <summary>
    /// Gets or sets the value of Interrupt Flip Flop 2 (IFF2). This is guaranteed to be either 0 or 1.
    /// </summary>
    public byte IFF2 { get; set; }

    /// <summary>
    /// Gets or sets the current Z80 interrupt mode. This can be 0, 1 or 2.
    /// </summary>
    public byte IM { get; set; }

    /// <summary>
    /// Gets or sets the t-states value at the time the snapshot was made.
    /// This counts up from zero to the maximum number of t-states per frame for the
    /// specific Spectrum model.
    /// </summary>
    public DWord CyclesStart { get; set; }

    /// <summary>
    /// Gets or sets The number of t-states left on restart when an interrupt can occur.
    /// This is used to support interrupt re-triggering properly.
    /// <remarks>
    /// On the Spectrum, the ULA holds the INTREQ line of the Z80 low for up to 48 t-states
    /// (depending on the model). If interrupts are enabled during this time, the Z80 will accept
    /// the request and invoke the appropriate interrupt service routine.The AMX mouse also asserts
    /// the INTREQ line when it needs attention. It is therefore possible for this member to be
    /// non-zero even if the t-state counter suggests we are not at the beginning of a frame.
    /// </remarks>
    /// </summary>
    public byte HoldIntReqCycles { get; set; }

    /// <summary>
    /// Gets or sets the various flags indicating the internal state of the CPU. This can be a combination of:
    /// ZXSTZF_EILAST or ZXSTZF_HALTED.
    /// </summary>
    public byte Flags { get; set; }

    /// <summary>
    /// Internal Z80 register used to generate bits 5 and 3 of the F register after executing a BIT x,(HL) instruction.
    /// Set to 0 (zero) if not supported.
    /// </summary>
    public Word MemPtr { get; set; }

    private static int Size => 37;

    internal void Write(Stream writer)
    {
        var header = new BlockHeader(BlockIds.Z80Regs, Size);
        header.Write(writer);

        writer.WriteWord(AF);
        writer.WriteWord(BC);
        writer.WriteWord(DE);
        writer.WriteWord(HL);
        writer.WriteWord(AF1);
        writer.WriteWord(BC1);
        writer.WriteWord(DE1);
        writer.WriteWord(HL1);
        writer.WriteWord(IX);
        writer.WriteWord(IY);
        writer.WriteWord(SP);
        writer.WriteWord(PC);
        writer.WriteByte(I);
        writer.WriteByte(R);
        writer.WriteByte(IFF1);
        writer.WriteByte(IFF2);
        writer.WriteByte(IM);
        writer.WriteDWord(CyclesStart);
        writer.WriteByte(HoldIntReqCycles);
        writer.WriteByte(Flags);
        writer.WriteWord(MemPtr);
    }

    internal static Z80RegsBlock Read(ByteStreamReader reader, int size)
    {
        if (size != Size)
        {
            throw new InvalidDataException("Invalid Z80Regs block size.");
        }

        var registers = new Z80RegsBlock
        {
            AF = reader.ReadWord(),
            BC = reader.ReadWord(),
            DE = reader.ReadWord(),
            HL = reader.ReadWord(),
            AF1 = reader.ReadWord(),
            BC1 = reader.ReadWord(),
            DE1 = reader.ReadWord(),
            HL1 = reader.ReadWord(),
            IX = reader.ReadWord(),
            IY = reader.ReadWord(),
            SP = reader.ReadWord(),
            PC = reader.ReadWord(),
            I = reader.ReadByte(),
            R = reader.ReadByte(),
            IFF1 = reader.ReadByte(),
            IFF2 = reader.ReadByte(),
            IM = reader.ReadByte(),
            CyclesStart = reader.ReadDWord(),
            HoldIntReqCycles = reader.ReadByte(),
            Flags = reader.ReadByte(),
            MemPtr = reader.ReadWord()
        };

        return registers;
    }
}