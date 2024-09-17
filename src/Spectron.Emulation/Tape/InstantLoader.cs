using OldBit.Z80Cpu;
using OldBit.Z80Cpu.Registers;

namespace OldBit.Spectron.Emulation.Tape;

/// <summary>
/// Allows fast data loading by skipping standard
/// ROM routines and loading the data directly to the memory.
/// </summary>
internal sealed class InstantLoader
{
    private readonly Z80 _cpu;
    private readonly IMemory _memory;

    internal InstantLoader(Z80 cpu, IMemory memory)
    {
        _cpu = cpu;
        _memory = memory;
    }

    internal void LoadBytes(TapeFile tapeFile)
    {
        var tap = tapeFile.GetNextTapData();
        if (tap == null)
        {
            return;
        }

        // Check if running Load (CF=1) or Verify (CF = 0)
        if ((_cpu.Registers.F & Flags.C) == 0)
        {
            return;
        }

        var checksum = tap.Flag;
        var startAddress = _cpu.Registers.IX;
        var blockLength = _cpu.Registers.DE;

        // Load data directly to memory
        if (_cpu.Registers.A == tap.Flag)
        {
            for (var i = 0; i < blockLength; i++)
            {
                if (i >= tap.BlockData.Count)
                {
                    // More bytes requested than the block contains
                    break;
                }

                _memory.Write((Word)(startAddress + i), tap.BlockData[i]);
                checksum ^= tap.BlockData[i];
            }

            checksum ^= tap.Checksum;
        }

        // Set registers as if the data was loaded and return to the caller
        _cpu.Registers.DE = 0;
        _cpu.Registers.IX = (Word)(startAddress + blockLength);
        _cpu.Registers.A = checksum;
        _cpu.Registers.PC = 0x05E0;
    }
}
