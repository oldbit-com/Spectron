using OldBit.Z80Cpu;

namespace OldBit.Spectron.Debugger.Stepping;

/// <summary>
/// Decides where "step over" should stop for the instruction at a given address.
/// </summary>
public static class StepOver
{
    /// <summary>
    /// Gets the address of the instruction after the one at <paramref name="pc"/>, when stepping over it should
    /// run until execution reaches that address: calls and restarts (until they return), conditional jumps,
    /// DJNZ and repeating block instructions (until the loop ends) and HALT (until the interrupt returns).
    /// </summary>
    /// <returns>The address to run to, or null when a single step is enough.</returns>
    public static Word? GetReturnAddress(IMemory memory, Word pc)
    {
        var opCode = memory.Read(pc);

        var length = opCode switch
        {
            0xCD => 3,                                          // CALL nn
            _ when (opCode & 0b1100_0111) == 0b1100_0100 => 3,  // CALL cc,nn
            _ when (opCode & 0b1100_0111) == 0b1100_0010 => 3,  // JP cc,nn
            _ when (opCode & 0b1100_0111) == 0b1100_0111 => 1,  // RST p
            0x10 => 2,                                          // DJNZ e
            0x20 or 0x28 or 0x30 or 0x38 => 2,                  // JR cc,e
            0x76 => 1,                                          // HALT
            0xED when IsRepeatingBlockInstruction(memory.Read((Word)(pc + 1))) => 2,
            _ => 0
        };

        return length == 0 ? null : (Word)(pc + length);
    }

    // LDIR, CPIR, INIR, OTIR, LDDR, CPDR, INDR, OTDR
    private static bool IsRepeatingBlockInstruction(byte opCode) => opCode is >= 0xB0 and <= 0xB3 or >= 0xB8 and <= 0xBB;
}
