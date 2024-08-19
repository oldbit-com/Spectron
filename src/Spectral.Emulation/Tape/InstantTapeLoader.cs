using OldBit.Z80Cpu;
using OldBit.Z80Cpu.Registers;

namespace OldBit.Spectral.Emulation.Tape;

/// <summary>
/// Allows for instant loading of the tape files by bypassing the tape player
/// and loading the data directly to memory.
/// </summary>
internal sealed class InstantTapeLoader
{
    private readonly Z80 _z80;
    private readonly IMemory _memory;
    private readonly TapePlayer _tapePlayer;

    internal InstantTapeLoader(Z80 z80, IMemory memory, TapePlayer tapePlayer)
    {
        _z80 = z80;
        _memory = memory;
        _tapePlayer = tapePlayer;
    }

    internal void LoadBytes()
    {
        if (_tapePlayer.TzxFile == null)
        {
            return;
        }

        var tapeBlock = _tapePlayer.NextBlock();
        if (tapeBlock == null)
        {
            return;
        }

        // Check if running Load (CF=1) or Verify (CF = 0)
        if ((_z80.Registers.F & Flags.C) == 0)
        {
            return;
        }

        var checksum = tapeBlock.Flag;
        var startAddress = _z80.Registers.IX;
        var blockLength = _z80.Registers.DE;

        // Load data directly to memory
        if (_z80.Registers.A == tapeBlock.Flag)
        {
            for (var i = 0; i < blockLength; i++)
            {
                _memory.Write((Word)(startAddress + i), tapeBlock.BlockData[i]);
                checksum ^= tapeBlock.BlockData[i];
            }

            checksum ^= tapeBlock.Checksum;
        }

        // Set registers as if the data was loaded and return to the caller
        _z80.Registers.DE = 0;
        _z80.Registers.IX = (Word)(startAddress + blockLength);
        _z80.Registers.A = checksum;
        _z80.Registers.PC = 0x05E0;
    }
}
