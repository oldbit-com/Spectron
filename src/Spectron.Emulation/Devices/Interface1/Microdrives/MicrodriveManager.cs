using OldBit.Spectron.Emulation.Devices.Interface1.Microdrives.Events;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices.Interface1.Microdrives;

public sealed class MicrodriveManager : IMicrodriveProvider
{
    private Interface1Device? _interface1Device;

    public Dictionary<MicrodriveId, Microdrive> Microdrives { get; } = new();

    public Microdrive this[MicrodriveId drive] => Microdrives[drive];

    public event CartridgeChangedEvent? CartridgeChanged;

    public MicrodriveManager()
    {
        foreach (var drive in Enum.GetValues<MicrodriveId>())
        {
            var microdrive = new Microdrive(drive);
            Microdrives[drive] = microdrive;

            microdrive.MotorStateChanged += _ =>
                ActiveDrive = Microdrives.Values.FirstOrDefault(x => x.IsMotorOn);

            microdrive.CartridgeChanged += e => CartridgeChanged?.Invoke(e);
        }
    }

    public Microdrive? ActiveDrive { get; private set; }

    public Interface1Device CreateDevice(Z80 cpu, IEmulatorMemory emulatorMemory)
    {
        _interface1Device?.Dispose();

        _interface1Device = new Interface1Device(cpu, emulatorMemory, this);

        return _interface1Device;
    }
}