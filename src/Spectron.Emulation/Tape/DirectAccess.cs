using OldBit.Spectron.Emulation.Extensions;
using OldBit.Z80Cpu;
using OldBit.Z80Cpu.Registers;
using OldBit.ZX.Files.Tap;
using OldBit.ZX.Files.Tzx.Blocks;

namespace OldBit.Spectron.Emulation.Tape;

/// <summary>
/// Allows fast loading and saving data by skipping standard ROM routines.
/// </summary>
internal sealed class DirectAccess
{
    private readonly Z80 _cpu;
    private readonly IMemory _memory;

    internal DirectAccess(Z80 cpu, IMemory memory)
    {
        _cpu = cpu;
        _memory = memory;
    }

    internal void LoadBytes(Cassette cassette)
    {
        var tap = cassette.GetNextTapData();
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
                if (i >= tap.Data.Count)
                {
                    // More bytes requested than the block contains
                    break;
                }

                _memory.Write((Word)(startAddress + i), tap.Data[i]);
                checksum ^= tap.Data[i];
            }

            checksum ^= tap.Checksum;
        }

        // Set registers as if the data was loaded and return to the caller
        _cpu.Registers.DE = 0;
        _cpu.Registers.IX = (Word)(startAddress + blockLength);
        _cpu.Registers.A = checksum;
        _cpu.Registers.PC = 0x05E0;
    }

    internal void SaveBytes(Cassette cassette)
    {
        var blockType = _cpu.Registers.A;
        var length = _cpu.Registers.DE;
        var startAddress = _cpu.Registers.IX;

        var data = _memory.ReadBytes(startAddress, length);
        var tapData = new TapData(blockType, data);

        cassette.Content.Blocks.Add(new StandardSpeedDataBlock(tapData));

        _cpu.Registers.PC = 0x053A;
    }
}
