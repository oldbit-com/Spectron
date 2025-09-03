using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices.Interface1.Microdrive;

public sealed class MicrodriveManager : IMicrodriveProvider
{
    private Interface1Device? _interface1Device;

    public Dictionary<MicrodriveId, Microdrive> Microdrives { get; } = new();

    public Microdrive this[MicrodriveId drive] => Microdrives[drive];

    public delegate void MicrodriveStateChangedEvent(EventArgs e);
    public event MicrodriveStateChangedEvent? StateChanged;

    public MicrodriveManager()
    {
        foreach (var drive in Enum.GetValues<MicrodriveId>())
        {
            var microdrive = new Microdrive();

            microdrive.StateChanged += _ => { OnStateChanged(); };

            Microdrives[drive] = microdrive;
        }
    }

    private void OnStateChanged()
    {
        StateChanged?.Invoke(EventArgs.Empty);
    }

    public Interface1Device CreateDevice(Z80 cpu, IEmulatorMemory emulatorMemory)
    {
        _interface1Device?.Dispose();

        _interface1Device = new Interface1Device(cpu, emulatorMemory, this);

        return _interface1Device;
    }

    public void NewCartridge(MicrodriveId drive)
    {
        Microdrives[drive].NewCartridge();

        OnStateChanged();
    }

    public void InsertCartridge(MicrodriveId drive, string filePath)
    {
        Microdrives[drive].InsertCartridge(filePath);

        OnStateChanged();
    }

    public void EjectCartridge(MicrodriveId drive)
    {
        Microdrives[drive].EjectCartridge();

        OnStateChanged();
    }
}