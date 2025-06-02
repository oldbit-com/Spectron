using OldBit.Spectron.Emulation.Extensions;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Z80Cpu;
using OldBit.Z80Cpu.Registers;
using OldBit.Spectron.Files.Tap;
using OldBit.Spectron.Files.Tzx.Blocks;

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

    internal void FastLoad(Cassette cassette)
    {
        var tap = cassette.GetNextTapData();
        if (tap == null)
        {
            return;
        }

        var verify = (_cpu.Registers.Prime.F & Flags.C) == 0;
        var checksum = tap.Flag;
        var address = _cpu.Registers.IX;
        var length = _cpu.Registers.DE;

        if (_cpu.Registers.Prime.A != tap.Flag)
        {
            _cpu.Registers.F |= ~Flags.C;
            return;
        }

        for (var i = 0; i < length; i++)
        {
            _cpu.Registers.DE -= 1;
            _cpu.Registers.IX += 1;

            var hasMoreBytes = i < tap.Data.Count;
            if (!hasMoreBytes)
            {
                break;
            }

            _cpu.Registers.L = tap.Data[i];
            checksum ^= _cpu.Registers.L;

            if (verify)
            {
                var data = _memory.Read((Word)(address + i));
                if (data == _cpu.Registers.L)
                {
                    continue;
                }

                _cpu.Registers.F |= ~Flags.C;

                return;
            }

            _memory.Write((Word)(address + i), _cpu.Registers.L);
        }

        checksum ^= tap.Checksum;

        _cpu.Registers.H = checksum;
        _cpu.Registers.A = checksum;
        _cpu.Registers.PC = 0x05E0;
    }

    internal void FastSave(Cassette cassette, TapeSpeed speed)
    {
        var blockType = _cpu.Registers.A;
        var length = _cpu.Registers.DE;
        var startAddress = _cpu.Registers.IX;

        var data = _memory.ReadBytes(startAddress, length);
        var tapData = new TapData(blockType, data);

        cassette.Content.Blocks.Add(new StandardSpeedDataBlock(tapData));

        if (speed == TapeSpeed.Instant)
        {
            _cpu.Registers.PC = RomRoutines.SA_DELAY - 2;
        }
    }
}
