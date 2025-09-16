using OldBit.Spectron.Emulation.Devices.Interface1.Microdrives.Events;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices.Interface1.Microdrives;

public sealed class MicrodriveManager : IMicrodriveProvider
{
    private Interface1Device? _interface1Device;
    private Microdrive? _activeMicrodrive;

    public Dictionary<MicrodriveId, Microdrive> Microdrives { get; } = new();

    public Microdrive this[MicrodriveId drive] => Microdrives[drive];

    public event CartridgeChangedEvent? CartridgeChanged;
    public event MicrodriveMotorStateChangedEvent? MotorStateChanged;

    public MicrodriveManager()
    {
        foreach (var drive in Enum.GetValues<MicrodriveId>())
        {
            var microdrive = new Microdrive(drive);

            microdrive.MotorStateChanged += e =>
            {
                _activeMicrodrive = Microdrives.GetValueOrDefault(e.DriveId);

                MotorStateChanged?.Invoke(e);
            };

            microdrive.CartridgeChanged += e => CartridgeChanged?.Invoke(e);

            Microdrives[drive] = microdrive;
        }
    }

    public Microdrive? GetActiveDrive() => _activeMicrodrive;

    public Interface1Device CreateDevice(Z80 cpu, IEmulatorMemory emulatorMemory)
    {
        _interface1Device?.Dispose();

        _interface1Device = new Interface1Device(cpu, emulatorMemory, this);

        return _interface1Device;
    }
}