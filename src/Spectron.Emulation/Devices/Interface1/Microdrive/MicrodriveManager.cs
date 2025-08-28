using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices.Interface1.Microdrive;

public sealed class MicrodriveManager : IMicrodriveProvider
{
    private Interface1Device? _interface1Device;

    public Dictionary<MicrodriveId, Microdrive> Microdrives { get; } = new();

    public Microdrive this[MicrodriveId driveId] => Microdrives[driveId];

    public MicrodriveManager()
    {
        foreach (var driveId in Enum.GetValues<MicrodriveId>())
        {
            Microdrives[driveId] = new Microdrive();
        }
    }

    public Interface1Device CreateDevice(Z80 cpu, IEmulatorMemory emulatorMemory)
    {
        _interface1Device?.Dispose();

        _interface1Device = new Interface1Device(cpu, emulatorMemory, this);

        return _interface1Device;
    }

    public void NewCartridge(MicrodriveId driveId) =>
        Microdrives[driveId].NewCartridge();

    public void InsertCartridge(MicrodriveId driveId, string filePath) =>
        Microdrives[driveId].InsertCartridge(filePath);

    public void EjectCartridge(MicrodriveId driveId) =>
        Microdrives[driveId].EjectCartridge();
}