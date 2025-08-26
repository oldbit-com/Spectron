using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices.Interface1;

public sealed class Interface1Device : IDevice
{
    public readonly ShadowRom ShadowRom;

    private const int COMMS_DATA = 0x01;
    private const int COMMS_CLK = 0x02;

    private readonly Microdrive[] _microdrives = new Microdrive[8];
    private readonly Z80 _cpu;

    private byte _previousControlValue = 0xFF;

    internal Dictionary<MicrodriveId, Microdrive> Microdrives = new();

    public Interface1Device(Z80 cpu, IEmulatorMemory emulatorMemory)
    {
        _cpu = cpu;
        ShadowRom = new ShadowRom(emulatorMemory, RomVersion.V2);

        foreach (var microdrive in Enum.GetValues<MicrodriveId>())
        {
            Microdrives.Add(microdrive, new Microdrive());
        }

        for (var i = 0; i < _microdrives.Length; i++)
        {
            _microdrives[i] = new Microdrive();
        }
    }

    internal bool IsEnabled { get; private set; }

    public void Enable()
    {
        IsEnabled = true;

        _cpu.BeforeFetch += BeforeFetch;
        _cpu.AfterFetch += AfterFetch;
    }

    public void Disable()
    {
        IsEnabled = false;

        ShadowRom.UnPage();

        _cpu.BeforeFetch -= BeforeFetch;
        _cpu.AfterFetch -= AfterFetch;
    }

    public void Reset() => ShadowRom.UnPage();

    public void WritePort(Word address, byte value)
    {
        if (!IsEnabled)
        {
            return;
        }

        if (IsControlPort(address))
        {
            Console.WriteLine($"Writing port {address:X4}: {value:B8}");

            if (IsFallingClockEdge(value, _previousControlValue))
            {
                ShiftDrivesLeftOnClock(value);
            }

            _previousControlValue = value;

            // microdrives_restart();
            // libspectrum_microdrive_set_cartridge_len( microdrive,
            // data_length / LIBSPECTRUM_MICRODRIVE_BLOCK_LEN );  543
        }
    }

    // Preamble: 10 zeros and 2 FF bytes (the preamble)
    // 15 bte header


    public byte? ReadPort(Word address)
    {
        if (!IsEnabled)
        {
            return null;
        }

        if (IsControlPort(address))
        {
            var microdrive = GetActiveMicrodrive();

            if (microdrive == null)
            {
                return 0; // TODO: 0, null, or FF?
            }

            if (microdrive is { IsMotorOn: true, IsCartridgeInserted: true })
            {
                var result = 0xFF;

                if (microdrive.GapCounter > 0)
                {
                    // 15 times GAP and SYNC high
                    microdrive.GapCounter -= 1;
                }
                else
                {
                    result &= 0xF9;     // 15 times GAP and SYNC low

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

                // Write protect b0

                return (byte)result;
            }
        }

        if (IsMicrodrivePort(address))
        {
            var activeMicrodrive = GetActiveMicrodrive();

            if (activeMicrodrive is { IsMotorOn: true, IsCartridgeInserted: true })
            {
                // Logic
                //
            }

            return 0xFF;
        }

        return null;
    }

    private void ShiftDrivesLeftOnClock(byte value)
    {
        for (var driveNumber = 7; driveNumber > 0; driveNumber--)
        {
            _microdrives[driveNumber].IsMotorOn = _microdrives[driveNumber - 1].IsMotorOn;
        }

        _microdrives[0].IsMotorOn = (value & COMMS_DATA) == 0;
    }

    private static bool IsFallingClockEdge(byte value, byte previousValue) =>
        (previousValue & COMMS_CLK) != 0 && (value & COMMS_CLK) == 0;

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

    private Microdrive? GetActiveMicrodrive() => _microdrives.FirstOrDefault(microdrive => microdrive.IsMotorOn);

    private static bool IsControlPort(Word address) => (address & 0x18) == 0x08;
    private static bool IsMicrodrivePort(Word address) => (address & 0x18) == 0x00;
    private static bool IsNetworkPort(Word address) => (address & 0x18) == 0x10;
}