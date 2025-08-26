using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices.Interface1;

public record CartridgeState
{
    public bool IsInserted { get; internal set; }
    public string? FilePath { get; internal set; }
}

public sealed class MicrodriveManager
{
    private Interface1Device? _interface1Device;

    public readonly Dictionary<MicrodriveId, CartridgeState> MicrodriveStates = new();

    public MicrodriveManager()
    {
        foreach (var driveId in Enum.GetValues<MicrodriveId>())
        {
            MicrodriveStates[driveId] = new CartridgeState();
        }
    }

    public Interface1Device CreateDevice(Z80 cpu, IEmulatorMemory emulatorMemory)
    {
        _interface1Device = new Interface1Device(cpu, emulatorMemory);

        return _interface1Device;
    }

    public CartridgeState InsertCartridge(MicrodriveId driveId, string filePath)
    {
        _interface1Device?.Microdrives[driveId].InsertCartridge(filePath);

        MicrodriveStates[driveId].IsInserted = true;
        MicrodriveStates[driveId].FilePath = filePath;

        return MicrodriveStates[driveId];
    }

    public void EjectCartridge(MicrodriveId driveId)
    {
        MicrodriveStates[driveId].IsInserted = false;
        MicrodriveStates[driveId].FilePath = null;
    }
}