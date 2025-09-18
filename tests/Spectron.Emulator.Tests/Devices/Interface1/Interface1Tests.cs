using System.Reflection;
using OldBit.Spectron.Emulation.Devices.Interface1;
using OldBit.Spectron.Emulation.Devices.Interface1.Microdrives;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulator.Tests.Devices.Interface1;

public class Interface1Tests : IDisposable
{
    private const Word ControlPort = 0xEEEF;
    private const Word DataPort = 0xEEE7;

    private readonly TestMicrodriveProvider _microdriveProvider = new();
    private readonly Interface1Device _interface1;

    public Interface1Tests()
    {
        var rom = RomReader.ReadRom(RomType.Interface1V2);

        var memory = new Memory48K(rom);
        var z80 = new Z80(memory);

        _interface1 = new Interface1Device(z80, memory, _microdriveProvider);
        _interface1.Enable();
    }

    [Theory]
    [InlineData(new byte[] { 0xEE, 0xEE, 0xEF, 0xED, 0xEF, 0xED, 0xEF, 0xED, 0xEF, 0xED, 0xEF, 0xED, 0xEF, 0xED, 0xEF, 0xED, 0xEE, 0xEC, 0xEE }, MicrodriveId.Drive1)]
    [InlineData(new byte[] { 0xEE, 0xEE, 0xEF, 0xED, 0xEF, 0xED, 0xEE, 0xEC, 0xEF, 0xED, 0xEF, 0xED, 0xEF, 0xED, 0xEF, 0xED, 0xEF, 0xED, 0xEE }, MicrodriveId.Drive6)]
    [InlineData(new byte[] { 0xEE, 0xEE, 0xEE, 0xEC, 0xEF, 0xED, 0xEF, 0xED, 0xEF, 0xED, 0xEF, 0xED, 0xEF, 0xED, 0xEF, 0xED, 0xEF, 0xED, 0xEE }, MicrodriveId.Drive8)]
    public void Microdrive_ShouldHaveMotorOn(byte[] values, MicrodriveId expectedSelectedDriveId)
    {
        foreach (var value in values)
        {
            _interface1.WritePort(ControlPort, value);
        }

        _microdriveProvider.Microdrives
            .Single(kv => kv.Key == expectedSelectedDriveId).Value
            .ShouldSatisfyAllConditions(microdrive => microdrive.IsMotorOn.ShouldBeTrue());

        _microdriveProvider.Microdrives
            .Where(kv => kv.Key != expectedSelectedDriveId)
            .Select(kv => kv.Value)
            .ShouldAllBe(microdrive => !microdrive.IsMotorOn);
    }

    [Fact]
    public void Microdrive_ShouldReturnSyncAndGapFlags()
    {
        InsertDemoCartridge();
        ActivateDrive7();

        Repeat.Run(10, () =>
        {
            // GAP / SYNC are not present
            Repeat.Run(15, () =>
            {
                var value = _interface1.ReadPort(ControlPort);

                value.ShouldBe((byte)0xFF);
            });

            // GAP / SYNC are present
            Repeat.Run(15, () =>
            {
                var value = _interface1.ReadPort(ControlPort);

                value.ShouldBe((byte)0xF9);
            });
        });
    }

    [Fact]
    public void Microdrive_ShouldDetectPreamble()
    {
        InsertNewCartridge();
        ActivateDrive7();

        Repeat.Run(10, () => _interface1.WritePort(DataPort, 0x00));
        Repeat.Run(2, () => _interface1.WritePort(DataPort, 0xFF));
    }

    private void ActivateDrive7()
    {
        byte[] values = [0xEE, 0xEE, 0xEF, 0xED, 0xEE, 0xEC, 0xEF, 0xED, 0xEF, 0xED, 0xEF, 0xED, 0xEF, 0xED, 0xEF, 0xED, 0xEF, 0xED, 0xEE];

        foreach (var value in values)
        {
            _interface1.WritePort(ControlPort, value);
        }
    }

    private void InsertDemoCartridge()
    {
        var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var testFilePath =  Path.Combine(binFolder, "TestData", "Demo Cartridge.mdr");

        var microdrive = _microdriveProvider.Microdrives.First(kv => kv.Key == MicrodriveId.Drive7).Value;
        microdrive.InsertCartridge(testFilePath);
    }

    private void InsertNewCartridge()
    {
        var microdrive = _microdriveProvider.Microdrives.First(kv => kv.Key == MicrodriveId.Drive7).Value;
        microdrive.NewCartridge();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        _interface1.Dispose();
    }
}