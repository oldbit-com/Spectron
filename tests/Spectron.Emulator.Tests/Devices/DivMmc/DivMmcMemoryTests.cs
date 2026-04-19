using OldBit.Spectron.Emulation.Devices.DivMmc;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Extensions;
using OldBit.Spectron.Emulation.Rom;

namespace OldBit.Spectron.Emulator.Tests.Devices.DivMmc;

public class DivMmcMemoryTests
{
    private const byte CONMEM = 0x80;
    private const byte MAPRAM = 0x40;

    private readonly Memory48K _memory48K;
    private readonly byte[] _rom48 = RomReader.ReadRom(RomType.Original48);
    private readonly byte[] _romDivMmc = RomReader.ReadRom(RomType.DivMmc);
    private readonly DivMmcMemory _divMmcMemory;

    public DivMmcMemoryTests()
    {
        _memory48K = new Memory48K(_rom48);
        _divMmcMemory = new DivMmcMemory(_memory48K, _romDivMmc);
    }

    [Fact]
    public void Rom_ShouldNotBePaged()
    {
        var rom = _memory48K.ReadRange(0, 16384);

        rom.ShouldBeEquivalentTo(_rom48);
    }

    [Fact]
    public void Rom_ShouldBePaged()
    {
        _divMmcMemory.Paging(PagingMode.On);

        // First 8k is ROM
        var rom1 = _memory48K.ReadRange(0, 8192);
        // Second 8k is RAM block
        var rom2 = _memory48K.ReadRange(8192, 8192);

        rom1.ShouldBeEquivalentTo(_romDivMmc);
        rom2.ShouldBeEquivalentTo(new byte[8192]);
    }

    [Fact]
    public void Rom_ShouldBeUnPaged()
    {
        _divMmcMemory.Paging(PagingMode.On);
        _divMmcMemory.Paging(PagingMode.Off);

        var rom = _memory48K.ReadRange(0, 16384);

        rom.ShouldBeEquivalentTo(_rom48);
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
    public void RamBlocks_ShouldBeMapped(int ramBlock)
    {
        var fillValue = (byte)(ramBlock + 0x20);
        Array.Fill(_divMmcMemory.Banks[ramBlock], fillValue);

        _divMmcMemory.PagingControl((byte)(CONMEM | ramBlock));

        _memory48K.ReadRange(0x2000, 0x2000).ShouldAllBe(x => x == fillValue);
    }

    [Fact]
    public void RamBlocks_ShouldBeWritable()
    {
        // Write to RAM blocks
        for (var ramBlock = 0; ramBlock < 8; ramBlock++)
        {
            _divMmcMemory.PagingControl((byte)(CONMEM | ramBlock));

            for (Word address = 0x2000; address < 0x4000; address++)
            {
                _memory48K.Write(address, (byte)(ramBlock * 0x100));
            }
        }

        // Disable paging and we should read standard ROM
        _divMmcMemory.Paging(PagingMode.Off);
        var rom = _memory48K.ReadBytes(0, 16384).ToArray();
        rom.ShouldBeEquivalentTo(_rom48);

        // Verify RAM blocks still contain the data
        for (var ramBlock = 0; ramBlock < 8; ramBlock++)
        {
            _divMmcMemory.PagingControl((byte)(CONMEM | ramBlock));

            var bytes = _memory48K.ReadRange(0x2000, 0x2000);
            bytes.ShouldAllBe(x => x == (byte)(ramBlock * 0x100));

            _memory48K.ReadRange(0, 8192).ShouldBe(_romDivMmc);
        }
    }

    [Fact]
    public void Paging_ShouldMapBank3At0x0000()
    {
        // Map bank 3 as RAM and enable paging
        _divMmcMemory.PagingControl(0x03);
        _divMmcMemory.Paging(PagingMode.On);

        // Write to RAM block 3
        _memory48K.Write(0x2000, 0xAA);

        // Map bank 3 as ROM
        _divMmcMemory.PagingControl(MAPRAM);

        _memory48K.Read(0x0000).ShouldBe(0xAA);
    }

    [Fact]
    public void Paging_ShouldMapWritableBankAt0x2000()
    {
        // Map bank 7 as RAM and enable paging
        _divMmcMemory.PagingControl(0x07);
        _divMmcMemory.Paging(PagingMode.On);

        // Write to RAM block 7
        _memory48K.Write(0x2000, 0xDD);

        // Verify the value is written to RAM block 7
        _memory48K.Read(0x2000).ShouldBe(0xDD);
    }

    [Fact]
    public void Paging_ShouldMapRomAt0x0000()
    {
        _divMmcMemory.PagingControl(0x06);
        _divMmcMemory.Paging(PagingMode.On);

        _memory48K.ReadRange(0, 8192).ShouldBe(_romDivMmc);
    }

    [Fact]
    public void MapRam_ShouldMakeBlock3ReadOnly()
    {
        _divMmcMemory.Banks[0][0] = 0x55;
        _divMmcMemory.Banks[0][1] = 0xCC;

        // Map bank 3 as RAM and enable paging
        _divMmcMemory.Paging(PagingMode.On);
        _divMmcMemory.PagingControl(0x03);

        // Write to RAM block 3
        _memory48K.Write(0x2000, 0xDD);
        _memory48K.Write(0x2001, 0xAA);

        // Verify the value is written to RAM block 3
        _divMmcMemory.Banks[3][0].ShouldBe(0xDD);
        _divMmcMemory.Banks[3][1].ShouldBe(0xAA);
        _memory48K.Read(0x2000).ShouldBe(0xDD);
        _memory48K.Read(0x2001).ShouldBe(0xAA);

        // Map bank 3 as ROM and RAM and make it read-only
        _divMmcMemory.PagingControl(MAPRAM + 0x03);

        // Verify the value is written to RAM block 3
        _divMmcMemory.Banks[3][0].ShouldBe(0xDD);
        _divMmcMemory.Banks[3][1].ShouldBe(0xAA);
        _memory48K.Read(0x0000).ShouldBe(0xDD);
        _memory48K.Read(0x0001).ShouldBe(0xAA);
        _memory48K.Read(0x2000).ShouldBe(0xDD);
        _memory48K.Read(0x2001).ShouldBe(0xAA);

        // It should not be possible to write
        _memory48K.Write(0x0000, 0x00);
        _memory48K.Write(0x0001, 0x00);
        _memory48K.Write(0x2000, 0x00);
        _memory48K.Write(0x2001, 0x00);

        // Verify the values have not changed
        _divMmcMemory.Banks[3][0].ShouldBe(0xDD);
        _divMmcMemory.Banks[3][1].ShouldBe(0xAA);
        _memory48K.Read(0x0000).ShouldBe(0xDD);
        _memory48K.Read(0x0001).ShouldBe(0xAA);
        _memory48K.Read(0x2000).ShouldBe(0xDD);
        _memory48K.Read(0x2001).ShouldBe(0xAA);

        // And MAPRAM should be preserved and bank 0 selected as RAM
        _divMmcMemory.PagingControl(0x00);
        _memory48K.Read(0x0000).ShouldBe(0xDD);
        _memory48K.Read(0x0001).ShouldBe(0xAA);
        _memory48K.Read(0x2000).ShouldBe(0x55);
        _memory48K.Read(0x2001).ShouldBe(0xCC);
    }

    [Fact]
    public void WhenEepromWriteEnabled_ShouldNotPage()
    {
        _divMmcMemory.IsEepromWriteEnabled = true;

        _divMmcMemory.Paging(PagingMode.On);

        _memory48K.ReadRange(0, 16384).ShouldBeEquivalentTo(_rom48);
    }

    [Fact]
    public void WhenEepromWriteEnabled_Bank0_ShouldBeWritable()
    {
        _divMmcMemory.IsEepromWriteEnabled = true;

        _divMmcMemory.PagingControl(CONMEM);

        _divMmcMemory.Write(0x0000, 0x01);
        _divMmcMemory.Write(0x0001, 0x02);

        _divMmcMemory.Read(0x0000).ShouldBe(0x01);
        _divMmcMemory.Read(0x0001).ShouldBe(0x02);
    }
}