using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Devices.Storage;
using OldBit.Spectron.Emulation.Rom;
using Shouldly;

namespace OldBit.Spectron.Emulator.Tests.Devices.Storage;

public class DivMmcMemoryTests
{
    private const byte DefaultEepromValue = 0xEE;
    private const byte CONMEM = 0x80;
    private const byte MAPRAM = 0x40;

    private readonly Memory48K _emulatorMemory;
    private readonly DivMmcMemory _divMmcMemory;

    public DivMmcMemoryTests()
    {
        _emulatorMemory = new Memory48K(RomReader.ReadRom(RomType.Original48));
        var eeprom = Enumerable.Repeat(DefaultEepromValue, 0x2000).ToArray();

        _divMmcMemory = new DivMmcMemory(_emulatorMemory, eeprom);
    }

    [Fact]
    public void DivMmcMemory_ShouldPageBank0EepromAndBank1Ram()
    {
        // Fill all memory banks with a value
        for (byte bank = 0; bank < 16; bank++)
        {
            _divMmcMemory.PagingControl((byte)(CONMEM | bank));

            for (Word address = 0x2000; address < 0x4000; address++)
            {
                _emulatorMemory.Write(address, (byte)(0xF0 | bank));
            }
        }

        // Read all memory banks and check the value
        for (byte bank = 0; bank < 16; bank++)
        {
            _divMmcMemory.PagingControl((byte)(CONMEM | bank));

            _divMmcMemory.Memory[..0x2000].ShouldAllBe(x => x == DefaultEepromValue);
            _divMmcMemory.Memory[0x2000..0x4000].ShouldAllBe(x => x == (byte)(0xF0 | bank));
        }
    }

    [Fact]
    public void EepromMemory_ShouldBeReadOnly()
    {
        _divMmcMemory.PagingControl(CONMEM);

        for (Word address = 0; address < 0x2000; address++)
        {
            _emulatorMemory.Write(address, 0x00);

            var value = _emulatorMemory.Read(address);
            value.ShouldBe(DefaultEepromValue);
        }

        _divMmcMemory.Memory[..0x2000].ShouldAllBe(x => x == DefaultEepromValue);
    }

    [Fact]
    public void WhenWriteEnabled_EepromMemory_ShouldBeWritable()
    {
        _divMmcMemory.IsWriteEnabled = true;
        _divMmcMemory.PagingControl(CONMEM);

        for (Word address = 0; address < 0x2000; address++)
        {
            var value = _emulatorMemory.Read(address);
            value.ShouldBe(DefaultEepromValue);

            _emulatorMemory.Write(address, 0x00);

            value = _emulatorMemory.Read(address);
            value.ShouldBe((byte)0x00);
        }
    }

    [Fact]
    public void Bank1_ShouldBeWritable()
    {
        // Fill all memory banks with a value
        for (byte bank = 0; bank < 16; bank++)
        {
            _divMmcMemory.PagingControl((byte)(CONMEM | bank));

            for (Word address = 0x2000; address < 0x4000; address++)
            {
                _emulatorMemory.Write(address, (byte)(0xF0 | bank));
            }
        }

        // Read all memory banks and check the value
        for (byte bank = 0; bank < 16; bank++)
        {
            _divMmcMemory.PagingControl((byte)(CONMEM | bank));

            for (Word address = 0x2000; address < 0x4000; address++)
            {
                var value = _emulatorMemory.Read(address);

                value.ShouldBe((byte)(0xF0 | bank));
            }
        }
    }

    [Fact]
    public void WhenMapRamIsRequested_Bank3ShouldBeReadonlyInBothBanks()
    {
        // Immediately page bank 3 and fill it with 0x33 value
        _divMmcMemory.PagingControl(CONMEM | 3);

        for (Word address = 0x2000; address < 0x4000; address++)
        {
            _emulatorMemory.Write(address, 0x33);
        }

        // Map bank 3 as RON and also as  bank 1
        _divMmcMemory.PagingControl(MAPRAM | 3);

        for (Word address = 0; address < 0x4000; address++)
        {
            _emulatorMemory.Write(address, 0);

            var value = _emulatorMemory.Read(address);

            value.ShouldBe((byte)0x33);
        }
    }

    [Fact]
    public void WhenMapRamIsEnabled_ItCannotBeDisabled()
    {
        // Immediately page bank 3 and fill it with 0x33 value
        _divMmcMemory.PagingControl(CONMEM | 3);

        for (Word address = 0x2000; address < 0x4000; address++)
        {
            _emulatorMemory.Write(address, 0x33);
        }

        // Map bank 3 as RON
        _divMmcMemory.PagingControl(MAPRAM);

        // Map bank 0
        _divMmcMemory.PagingControl(CONMEM | 0);

        // Bank 3 is still mapped as ROM
        for (Word address = 0; address < 0x2000; address++)
        {
            var value = _emulatorMemory.Read(address);

            value.ShouldBe((byte)0x33);
        }
    }
}