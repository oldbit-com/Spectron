namespace OldBit.Spectron.Emulation.Devices.Interface1.Microdrives;

public interface IMicrodriveProvider
{
    Dictionary<MicrodriveId, Microdrive> Microdrives { get; }

    Microdrive? ActiveDrive { get; }
}