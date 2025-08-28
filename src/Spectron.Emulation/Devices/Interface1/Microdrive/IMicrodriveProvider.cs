namespace OldBit.Spectron.Emulation.Devices.Interface1.Microdrive;

public interface IMicrodriveProvider
{
    Dictionary<MicrodriveId, Microdrive> Microdrives { get; }
}