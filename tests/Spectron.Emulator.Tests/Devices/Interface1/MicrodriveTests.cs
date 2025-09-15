using System.Reflection;

namespace OldBit.Spectron.Emulator.Tests.Devices.Interface1;

public class MicrodriveTests
{
    private readonly Emulation.Devices.Interface1.Microdrive.Microdrive _microdrive = new();
    private readonly string _testFilePath;

    public MicrodriveTests()
    {
        var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        _testFilePath = Path.Combine(binFolder, "TestData", "Demo Cartridge.mdr");
    }

    [Fact]
    public void NewCartridge_ShouldCreateEmptyCartridge()
    {
        _microdrive.NewCartridge();

        _microdrive.Cartridge.ShouldNotBeNull();
        _microdrive.IsCartridgeInserted.ShouldBeTrue();
        _microdrive.IsCartridgeWriteProtected.ShouldBeFalse();

        _microdrive.Cartridge.Blocks.Count.ShouldBe(508);
        _microdrive.Cartridge.FilePath.ShouldBeNull();
        _microdrive.Cartridge.Blocks.ShouldAllBe(x => !x.IsPreambleValid);
    }

    [Fact]
    public void InsertCartridge_ShouldLoadCartridge()
    {
        _microdrive.InsertCartridge(_testFilePath);

        _microdrive.Cartridge.ShouldNotBeNull();
        _microdrive.IsCartridgeInserted.ShouldBeTrue();
        _microdrive.IsCartridgeWriteProtected.ShouldBeFalse();

        _microdrive.Cartridge.Blocks.Count.ShouldBe(508);
        _microdrive.Cartridge.FilePath.ShouldBe(_testFilePath);
        _microdrive.Cartridge.Blocks.ShouldAllBe(x => x.IsPreambleValid);
    }

    [Fact]
    public void EjectCartridge_ShouldRemoveCartridge()
    {
        _microdrive.NewCartridge();
        _microdrive.EjectCartridge();

        _microdrive.Cartridge.ShouldBeNull();
        _microdrive.IsCartridgeInserted.ShouldBeFalse();
    }

    [Fact]
    public void Read_ShouldReadCartridgeData()
    {
        var allData = File.ReadAllBytes(_testFilePath);
        _microdrive.InsertCartridge(_testFilePath);

        var data = new List<byte>();

        for (var i = 0; i < 15; i++)
        {
            var value = _microdrive.Read();

            value.ShouldNotBeNull();

            data.Add(value.Value);
        }

        data.ShouldBe(allData.Take(15));
    }

    [Fact]
    public void ReadOutsideRange_ShouldReturnLastResult()
    {
        _microdrive.InsertCartridge(_testFilePath);

        for (var i = 0; i < 15; i++)
        {
            _microdrive.Read();
        }

        for (var i = 0; i < 5; i++)
        {
            var value = _microdrive.Read();

            value.ShouldNotBeNull();
            value.Value.ShouldBe(0x3A);
        }
    }
}