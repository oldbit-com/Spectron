using OldBit.Spectron.Emulation.Devices.Interface1;
using OldBit.Spectron.Emulation.Devices.Interface1.Microdrive;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulator.Tests.Devices.Interface1;

public class Interface1Tests : IDisposable
{
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
            _interface1.WritePort(0xEEEF, value);
        }

        _microdriveProvider.Microdrives
            .Single(kv => kv.Key == expectedSelectedDriveId).Value
            .ShouldSatisfyAllConditions(microdrive => microdrive.IsMotorOn.ShouldBeTrue());

        _microdriveProvider.Microdrives
            .Where(kv => kv.Key != expectedSelectedDriveId)
            .Select(kv => kv.Value)
            .ShouldAllBe(microdrive => !microdrive.IsMotorOn);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _interface1.Dispose();
    }
}