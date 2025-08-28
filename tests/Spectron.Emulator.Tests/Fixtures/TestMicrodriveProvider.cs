using OldBit.Spectron.Emulation.Devices.Interface1.Microdrive;

namespace OldBit.Spectron.Emulator.Tests.Fixtures;

public class TestMicrodriveProvider : IMicrodriveProvider
{
    public Dictionary<MicrodriveId, Microdrive> Microdrives { get; } = new();

    public TestMicrodriveProvider()
    {
        foreach (var driveId in Enum.GetValues<MicrodriveId>())
        {
            Microdrives[driveId] = new Microdrive();
        }
    }
}