using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices.Audio.AY;

/// <summary>
///
/// \_________    00xx
/// /|________    01xx
/// \|\|\|\|\|    1000
/// \_________    1001
/// \|‾‾‾‾‾‾‾‾    1011
/// /|/|/|/|/|    1100
/// /‾‾‾‾‾‾‾‾     1101
/// /\/\/\/\/\    1110
/// /|________    1111
///
/// </summary>

internal class AY8910(Clock clock) : IDevice
{
    private const Word RegisterPort = 0xFFFD;
    private const Word DataPort = 0xBFFD;

    private int _selectedRegister;
    private readonly int[] _registers = new int[16];

    internal bool IsEnabled { get; set; }

    public void WritePort(Word address, byte value)
    {
        if (!IsEnabled)
        {
            return;
        }

        if (IsSelectRegisterPort(address))
        {
            _selectedRegister = value & 0x0F;
        }
        else if (IsDataPort(address))
        {
            Update(clock.FrameTicks);

            switch (_selectedRegister)
            {
                case Register.FineTuneA:
                    SetRegister(_selectedRegister, value);
                    break;

                case Register.CoarseTuneA:
                    SetRegister(_selectedRegister, (byte)(value & 0x0F));
                    break;

                case Register.FineTuneB:
                    SetRegister(_selectedRegister, value);
                    break;

                case Register.CoarseTuneB:
                    SetRegister(_selectedRegister, (byte)(value & 0x0F));
                    break;

                case Register.FineTuneC:
                    SetRegister(_selectedRegister, value);
                    break;

                case Register.CoarseTuneC:
                    SetRegister(_selectedRegister, (byte)(value & 0x0F));
                    break;

                case Register.NoisePeriod:
                    SetRegister(_selectedRegister, (byte)(value & 0x1F));
                    break;

                case Register.Mixer:
                    SetRegister(_selectedRegister, value);
                    break;

                case Register.AmplitudeA:
                    SetRegister(_selectedRegister, value);
                    break;

                case Register.AmplitudeB:
                    SetRegister(_selectedRegister, value);
                    break;

                case Register.AmplitudeC:
                    SetRegister(_selectedRegister, value);
                    break;
            }
        }
    }

    public byte? ReadPort(Word address)
    {
        if (!IsEnabled)
        {
            return null;
        }

        if (IsSelectRegisterPort(address))
        {

        }
        if (address != RegisterPort)
        {
            return null;
        }
        return null;
    }

    internal void EndFrame(int frameTicks)
    {

    }

    internal void Update(int frameTicks)
    {
        //
    }

    private void SetRegister(int register, byte value) => _registers[register] = value;

    // Register port 0xFFFD is decoded as: A15=1,A14=1 & A1=0
    private static bool IsSelectRegisterPort(Word address) => (address & 0xC002) == 0xC000;

    // Data port 0xBFFD is decoded as: A15=1 & A1=0
    private static bool IsDataPort(Word address) => (address & 0x8002) == 0x8000;
}