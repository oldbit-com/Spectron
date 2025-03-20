using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Devices.Storage;
using OldBit.Spectron.Emulation.Rom;
using Shouldly;

namespace OldBit.Spectron.Emulator.Tests.Devices.Storage;

public class DivMmcMemoryTests
{
    private readonly Memory48K _emulatorMemory;
    private readonly DivMmcMemory _divMmcMemory;

    public DivMmcMemoryTests()
    {
        _emulatorMemory = new Memory48K(RomReader.ReadRom(RomType.Original48));
        var eeprom = Enumerable.Repeat((byte)0xEE, 0x2000).ToArray();

        _divMmcMemory = new DivMmcMemory(_emulatorMemory, eeprom);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(12)]
    [InlineData(13)]
    [InlineData(14)]
    [InlineData(15)]
    public void DivMmcMemory_ShouldPageBank0EepromAndBank1Ram(byte bank)
    {
        _divMmcMemory.PageMemory(bank);

        var value = _emulatorMemory.Read(0x0000);
        value.ShouldBe((byte)0xEE);

        value = _emulatorMemory.Read(0x1FFF);
        value.ShouldBe((byte)0xEE);
    }

    [Fact]
    public void EepromMemory_ShouldBeReadOnly()
    {
        _divMmcMemory.PageMemory(0);

        for (Word address = 0; address < 0x2000; address++)
        {
            _emulatorMemory.Write(address, 0x00);

            var value = _emulatorMemory.Read(address);
            value.ShouldBe((byte)0xEE);
        }

        _divMmcMemory.Memory[..0x2000].ShouldAllBe(x => x == 0xEE);
    }

    [Fact]
    public void Bank1_ShouldBeWritable()
    {
        // Fill all memory banks with a value
        for (byte bank = 0; bank < 16; bank++)
        {
            _divMmcMemory.PageMemory(bank);

            for (Word address = 0x2000; address < 0x4000; address++)
            {
                _emulatorMemory.Write(address, (byte)(0xF0 | bank));
            }
        }

        // Read all memory banks and check the value
        for (byte bank = 0; bank < 16; bank++)
        {
            _divMmcMemory.PageMemory(bank);

            for (Word address = 0x2000; address < 0x4000; address++)
            {
                var value = _emulatorMemory.Read(address);

                value.ShouldBe((byte)(0xF0 | bank));
            }
        }
    }

    [Fact]
    public void WhenMapRam_Bank3ShouldBeReadonlyInBothBanks()
    {
        // Fill bank 3 with 0x33 value
        _divMmcMemory.PageMemory(0b0000_0011);

        for (Word address = 0x2000; address < 0x4000; address++)
        {
            _emulatorMemory.Write(address, 0x33);
        }

        // Map bank 3 to bank 0
        _divMmcMemory.PageMemory(0b0100_0011);

        for (Word address = 0; address < 0x4000; address++)
        {
            _emulatorMemory.Write(address, 0);

            var value = _emulatorMemory.Read(address);

            value.ShouldBe((byte)0x33);
        }
    }
}