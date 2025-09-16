using OldBit.Spectron.Emulation.Devices.Interface1.Microdrives;

namespace OldBit.Spectron.Emulator.Tests.Fixtures;

public class TestMicrodriveProvider : IMicrodriveProvider
{
    public Dictionary<MicrodriveId, Microdrive> Microdrives { get; } = new();

    public TestMicrodriveProvider()
    {
        foreach (var drive in Enum.GetValues<MicrodriveId>())
        {
            Microdrives[drive] = new Microdrive(drive);
        }
    }

    public Microdrive? GetActiveDrive() => 
        Microdrives.Values.FirstOrDefault(drive => drive.IsMotorOn);
}