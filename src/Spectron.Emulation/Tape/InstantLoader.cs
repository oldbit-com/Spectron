using OldBit.Z80Cpu;
using OldBit.Z80Cpu.Registers;

namespace OldBit.Spectron.Emulation.Tape;

/// <summary>
/// Allows fast data loading by skipping standard
/// ROM routines and loading the data directly to the memory.
/// </summary>
internal sealed class InstantLoader
{
    private readonly Z80 _z80;
    private readonly IMemory _memory;

    internal InstantLoader(Z80 z80, IMemory memory)
    {
        _z80 = z80;
        _memory = memory;
    }

    internal void LoadBytes(ITapDataProvider tapeDataProvider)
    {
        var tap = tapeDataProvider.GetNextTapData();
        if (tap == null)
        {
            return;
        }

        // Check if running Load (CF=1) or Verify (CF = 0)
        if ((_z80.Registers.F & Flags.C) == 0)
        {
            return;
        }

        var checksum = tap.Flag;
        var startAddress = _z80.Registers.IX;
        var blockLength = _z80.Registers.DE;

        // Load data directly to memory
        if (_z80.Registers.A == tap.Flag)
        {
            for (var i = 0; i < blockLength; i++)
            {
                _memory.Write((Word)(startAddress + i), tap.BlockData[i]);
                checksum ^= tap.BlockData[i];
            }

            checksum ^= tap.Checksum;
        }

        // Set registers as if the data was loaded and return to the caller
        _z80.Registers.DE = 0;
        _z80.Registers.IX = (Word)(startAddress + blockLength);
        _z80.Registers.A = checksum;
        _z80.Registers.PC = 0x05E0;
    }
}
