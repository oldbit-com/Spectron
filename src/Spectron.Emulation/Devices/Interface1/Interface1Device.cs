using OldBit.Spectron.Emulation.Devices.Interface1.Microdrive;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices.Interface1;

public sealed class Interface1Device(
    Z80 cpu,
    IEmulatorMemory emulatorMemory,
    IMicrodriveProvider microdriveProvider) : IDevice, IDisposable
{
    public readonly ShadowRom ShadowRom = new(emulatorMemory, RomVersion.V2);

    private const int CommsData = 0x01;
    private const int CommsClk = 0x02;

    private byte _previousControlValue = 0xFF;

    public bool IsEnabled { get; private set; }

    public void Enable()
    {
        IsEnabled = true;

        Reset();
        SubscribeCpuEvents();
    }

    public void Disable()
    {
        IsEnabled = false;

        Reset();
        UnsubscribeCpuEvents();
    }

    public void Reset()
    {
        ShadowRom.UnPage();

        _previousControlValue =  0xFF;

        foreach (var drive in microdriveProvider.Microdrives.Values)
        {
            drive.Reset();
        }
    }

    public void WritePort(Word address, byte value)
    {
        if (!IsEnabled)
        {
            return;
        }

        if (IsControlPort(address))
        {
            if (IsFallingClockEdge(value, _previousControlValue))
            {
                ShiftDrivesLeftOnClock(value);
            }

            _previousControlValue = value;

            var microdrive = GetActiveMicrodrive();
            microdrive?.Synchronize();
        }

        if (IsDataPort(address))
        {
            // Write to microdrive
        }
    }

    public byte? ReadPort(Word address)
    {
        if (!IsEnabled)
        {
            return null;
        }

        byte? value = null;

        if (IsControlPort(address))
        {
            value = GetControlValue();
        }

        if (IsDataPort(address))
        {
            value = GetDataValue();
        }

        return value;
    }

    private byte? GetControlValue()
    {
        var microdrive = GetActiveMicrodrive();

        if (microdrive is not { IsMotorOn: true, IsCartridgeInserted: true })
        {
            return null;
        }

        var result = 0xFF;

        // TODO: Rework, we only need 15 GAP and SYNC signals
        // Synchronization
        if (microdrive.GapCounter > 0)
        {
            // 15 times GAP and SYNC high
            microdrive.GapCounter -= 1;
        }
        else
        {
            result &= 0xF9; // 15 times GAP and SYNC low

            if (microdrive.SyncCounter > 0)
            {
                microdrive.SyncCounter -= 1;
            }
            else
            {
                microdrive.GapCounter = 15;
                microdrive.SyncCounter = 15;
            }
        }

        if (microdrive.IsWriteProtected)
        {
            result &= 0xFE; // WR-PROT: A "low" indicates a cartridge is write protected.
        }

        microdrive.Synchronize();

        return (byte)result;
    }

    private byte? GetDataValue()
    {
        var microdrive = GetActiveMicrodrive();

        return microdrive is { IsMotorOn: true, IsCartridgeInserted: true } ?
            microdrive.Read() :
            (byte?)0xFF;
    }

    private void SubscribeCpuEvents()
    {
        cpu.BeforeFetch += BeforeFetch;
        cpu.AfterFetch += AfterFetch;
    }

    private void UnsubscribeCpuEvents()
    {
        cpu.BeforeFetch -= BeforeFetch;
        cpu.AfterFetch -= AfterFetch;
    }

    private void ShiftDrivesLeftOnClock(byte value)
    {
        for (var driveNumber = 8; driveNumber > 1; driveNumber--)
        {
            microdriveProvider.Microdrives[(MicrodriveId)driveNumber].IsMotorOn =
                microdriveProvider.Microdrives[(MicrodriveId)(driveNumber - 1)].IsMotorOn;
        }

        microdriveProvider.Microdrives[MicrodriveId.Drive1].IsMotorOn = (value & CommsData) == 0;
    }

    private static bool IsFallingClockEdge(byte value, byte previousValue) =>
        (previousValue & CommsClk) != 0 && (value & CommsClk) == 0;

    private void BeforeFetch(Word pc)
    {
        // Paging happens before the opcode
        if (pc is 0x0008 or 0x1708)
        {
            ShadowRom.Page();
        }
    }

    private void AfterFetch(Word pc)
    {
        // UnPaging happens after the opcode
        if (pc == 0x0700 + 1)
        {
            ShadowRom.UnPage();
        }
    }

    private Microdrive.Microdrive? GetActiveMicrodrive() =>
        microdriveProvider.Microdrives.Values.FirstOrDefault(microdrive => microdrive.IsMotorOn);

    private static bool IsControlPort(Word address) => (address & 0x18) == 0x08;
    private static bool IsDataPort(Word address) => (address & 0x18) == 0x00;

    public void Dispose()
    {
        UnsubscribeCpuEvents();
        Reset();
    }
}