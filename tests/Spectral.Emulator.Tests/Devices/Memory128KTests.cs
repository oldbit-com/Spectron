using FluentAssertions;
using OldBit.Spectral.Emulation.Devices;
using OldBit.ZXSpectrum.Emulator.Tests.Fixtures;

namespace OldBit.ZXSpectrum.Emulator.Tests.Devices;

public class Memory128KTests
{
    private readonly byte[] _rom48;
    private readonly byte[] _rom128;

    public Memory128KTests()
    {
        var random = new Random(15001900);

        _rom48 = Enumerable.Repeat(0, 16384).Select(_ => (byte)random.Next(0, 256)).ToArray();
        _rom128 = Enumerable.Repeat(0, 16384).Select(_ => (byte)random.Next(0, 256)).ToArray();
    }

    [Fact]
    public void Rom_ShouldBePopulatedAndDefaultTo128K()
    {
        var memory = new Memory128K(_rom48, _rom128);

        memory.ReadRom().Should().BeEquivalentTo(_rom128);
    }

    [Fact]
    public void Rom_ShouldBeAbleToSwitchBetweenRoms()
    {
        var memory = new Memory128K(_rom48, _rom128);

        memory.SetPagingMode(0b00010000);
        memory.ReadRom().Should().BeEquivalentTo(_rom48);

        memory.SetPagingMode(0b00000000);
        memory.ReadRom().Should().BeEquivalentTo(_rom128);
    }

    [Fact]
    public void Rom_ShouldBeAbleToSwitchScreen()
    {
        var memory = new Memory128K(_rom48, _rom128);
        // Fill standard screen with 0x20
        memory.Fill(0x4000, 0x4000, 0x20);

        // Activate block 7 and fill it with 0x30
        memory.SetPagingMode(7);
        memory.Fill(0xC000, 0x4000, 0x30);

        // Memory should still read as 0x20, also screen bank
        memory.ReadRange(0x4000, 0x4000).Should().AllBeEquivalentTo(0x20);
        memory.ReadScreen().Should().AllBeEquivalentTo(0x20);

        // Select shadow screen
        memory.SetPagingMode(0b00001000);

        // Memory should still read as 0x20, but now screen should be 0x30 (shadow)
        memory.ReadRange(0x4000, 0x4000).Should().AllBeEquivalentTo(0x20);
        memory.ReadScreen().Should().AllBeEquivalentTo(0x30);
    }

    [Fact]
    public void Rom_ShouldBeAbleToWriteToBank2()
    {
        var memory = new Memory128K(_rom48, _rom128);
        memory.SetPagingMode(2);

        memory.Fill(0xC000, 0x4000, 0x56);

        // Bank 2 to is always at 0x8000 and now at 0xC000 based on the paging mode
        memory.ReadRange(0x8000, 0x4000).Should().AllBeEquivalentTo(0x56);
        memory.ReadRange(0xC000, 0x4000).Should().AllBeEquivalentTo(0x56);
    }

    [Fact]
    public void Rom_ShouldBeAbleToWriteToBank5()
    {
        var memory = new Memory128K(_rom48, _rom128);
        memory.SetPagingMode(5);

        memory.Fill(0xC000, 0x4000, 0xAF);

        // Bank 5 to is always at 0x4000 and now at 0xC000 based on the paging mode
        memory.ReadRange(0x4000, 0x4000).Should().AllBeEquivalentTo(0xAF);
        memory.ReadRange(0xC000, 0x4000).Should().AllBeEquivalentTo(0xAF);
    }
}