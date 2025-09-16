using OldBit.Spectron.Emulation.Devices.Interface1.Microdrives;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices.Interface1;

/// <summary>
/// Interface 1 emulation for Microdrives only. No serial port on network emulation.
/// <remarks>
/// https://sinclair.wiki.zxnet.co.uk/wiki/ZX_Interface_1
/// https://worldofspectrum.org/faq/reference/48kreference.htm#PortE7
/// https://worldofspectrum.org/faq/reference/formats.htm
/// </remarks>
/// </summary>
public sealed class Interface1Device(
    Z80 cpu,
    IEmulatorMemory emulatorMemory,
    IMicrodriveProvider microdriveProvider) : IDevice, IDisposable
{
    public readonly ShadowRom ShadowRom = new(emulatorMemory, Interface1RomVersion.V2);

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
            ControlMicrodrive(value);
        }

        if (IsDataPort(address))
        {
            WriteData(value);
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
            value = GetData();
        }

        return value;
    }

    private byte? GetControlValue()
    {
        var microdrive = microdriveProvider.GetActiveDrive();

        byte result = 0xFF;

        if (microdrive is not { IsMotorOn: true, IsCartridgeInserted: true } ||
            !microdrive.Cartridge!.Blocks[microdrive.CurrentBlock].IsPreambleValid)
        {
            return result;
        }

        if (microdrive.IsGapSync())
        {
            result = 0xF9; // GAP and SYNC low
        }

        if (microdrive.IsCartridgeWriteProtected)
        {
            result &= 0xFE; // WR-PROT: A "low" indicates a cartridge is write protected.
        }

        microdrive.SynchronizeBlock();

        return result;
    }

    private byte? GetData()
    {
        var microdrive = microdriveProvider.GetActiveDrive();

        return microdrive is { IsMotorOn: true, IsCartridgeInserted: true } ?
            microdrive.Read() :
            (byte?)0xFF;
    }

    private void ControlMicrodrive(byte value)
    {
        if (IsFallingClockEdge(value, _previousControlValue))
        {
            ShiftDrivesLeftOnClock(value);
        }

        _previousControlValue = value;

        var microdrive = microdriveProvider.GetActiveDrive();
        microdrive?.SynchronizeBlock();
    }

    private void WriteData(byte value)
    {
        var microdrive = microdriveProvider.GetActiveDrive();

        if (microdrive is not { IsMotorOn: true, IsCartridgeInserted: true })
        {
            return;
        }

        microdrive.Write(value);
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

    private static bool IsControlPort(Word address) => (address & 0x18) == 0x08;
    private static bool IsDataPort(Word address) => (address & 0x18) == 0x00;

    public void Dispose()
    {
        UnsubscribeCpuEvents();
        Reset();
    }
}