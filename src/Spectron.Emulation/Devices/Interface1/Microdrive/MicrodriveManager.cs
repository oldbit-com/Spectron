using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices.Interface1.Microdrive;

public class MicrodriveStateChangedEventArgs : EventArgs
{
    public MicrodriveId MicrodriveId { get; init; }
}

public sealed class MicrodriveManager : IMicrodriveProvider
{
    private Interface1Device? _interface1Device;
    private Microdrive? _activeMicrodrive;

    public Dictionary<MicrodriveId, Microdrive> Microdrives { get; } = new();

    public Microdrive this[MicrodriveId drive] => Microdrives[drive];

    public delegate void MicrodriveStateChangedEvent(MicrodriveStateChangedEventArgs e);
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

    public Microdrive? GetActiveDrive() => _activeMicrodrive;

    private void OnStateChanged()
    {
        var activeMicrodriveId = Microdrives.FirstOrDefault(microdrive => microdrive.Value.IsMotorOn).Key;

        _activeMicrodrive = Microdrives.GetValueOrDefault(activeMicrodriveId);

        StateChanged?.Invoke(new MicrodriveStateChangedEventArgs
        {
            MicrodriveId = activeMicrodriveId
        });
    }

    private void OnStateChanged(MicrodriveId drive)
    {
        StateChanged?.Invoke(new MicrodriveStateChangedEventArgs
        {
            MicrodriveId = drive
        });
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

        OnStateChanged(drive);
    }

    public void InsertCartridge(MicrodriveId drive, string filePath)
    {
        Microdrives[drive].InsertCartridge(filePath);

        OnStateChanged(drive);
    }

    public void EjectCartridge(MicrodriveId drive)
    {
        Microdrives[drive].EjectCartridge();

        OnStateChanged(drive);
    }
}