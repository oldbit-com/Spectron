using static OldBit.Spectron.Emulation.Devices.Audio.AY.Registers;

namespace OldBit.Spectron.Emulation.Devices.Audio.AY;

internal sealed class AyDevice : IDevice
{
    private const int MaxAmplitude = 10900;

    private static readonly double[] SignalLevels = [
        0.0000, 0.0137, 0.0205, 0.0291, 0.0423, 0.0618, 0.0847, 0.1369,
        0.1691, 0.2647, 0.3527, 0.4499, 0.5704, 0.6873, 0.8482, 1.0000];

    private readonly int[] _volumeLevels = SignalLevels.Select(x => (int)(x * MaxAmplitude)).ToArray();
    private readonly byte[] _registers = new byte[16];

    internal Channel ChannelA { get; } = new();
    internal Channel ChannelB { get; } = new();
    internal Channel ChannelC { get; } = new();
    internal Envelope Envelope { get; }= new();
    internal Noise Noise { get; } = new();

    internal bool IsEnabled { get; set; }
    internal Action OnUpdateAudio { get; set; } = () => { };

    private int _selectedRegister;

    public void WritePort(Word address, byte value)
    {
        if (!IsEnabled)
        {
            return;
        }

        if (IsRegisterPort(address))
        {
            _selectedRegister = value & 0x0F;
        }
        else if (IsDataPort(address))
        {
            OnUpdateAudio();

            SetRegister(_selectedRegister, value);

            switch (_selectedRegister)
            {
                case FineTuneA:
                case CoarseTuneA:
                    ChannelA.SetPeriod(_registers[FineTuneA], _registers[CoarseTuneA]);
                    break;

                case FineTuneB:
                case CoarseTuneB:
                    ChannelB.SetPeriod(_registers[FineTuneB], _registers[CoarseTuneB]);
                    break;

                case FineTuneC:
                case CoarseTuneC:
                    ChannelC.SetPeriod(_registers[FineTuneC], _registers[CoarseTuneC]);
                    break;

                case NoisePeriod:
                    Noise.SetPeriod(value);
                    break;

                case Mixer:
                    ChannelA.ToneDisabled = (value & 0x01) != 0;
                    ChannelB.ToneDisabled = (value & 0x02) != 0;
                    ChannelC.ToneDisabled = (value & 0x04) != 0;

                    ChannelA.NoiseDisabled = (value & 0x08) != 0;
                    ChannelB.NoiseDisabled = (value & 0x10) != 0;
                    ChannelC.NoiseDisabled = (value & 0x20) != 0;
                    break;

                case AmplitudeA:
                    (ChannelA.AmplitudeMode, ChannelA.Amplitude) = GetAmplitude(value);
                    break;

                case AmplitudeB:
                    (ChannelB.AmplitudeMode, ChannelB.Amplitude) = GetAmplitude(value);
                    break;

                case AmplitudeC:
                    (ChannelC.AmplitudeMode, ChannelC.Amplitude) = GetAmplitude(value);
                    break;

                case FineTuneEnvelope:
                case CoarseTuneEnvelope:
                    Envelope.SetPeriod(_registers[FineTuneEnvelope], _registers[CoarseTuneEnvelope]);
                    break;

                case EnvelopeShape:
                    Envelope.SetShape(value);

                    if (ChannelA.AmplitudeMode == AmplitudeMode.VariableLevel)
                    {
                        ChannelA.Amplitude = _volumeLevels[Envelope.Amplitude];
                    }

                    if (ChannelB.AmplitudeMode == AmplitudeMode.VariableLevel)
                    {
                        ChannelB.Amplitude = _volumeLevels[Envelope.Amplitude];
                    }

                    if (ChannelC.AmplitudeMode == AmplitudeMode.VariableLevel)
                    {
                        ChannelC.Amplitude = _volumeLevels[Envelope.Amplitude];
                    }

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

        if (IsDataPort(address))
        {
            return _registers[_selectedRegister];
        }

        return null;
    }

    internal void SetAmplitudeUsingEnvelope(Channel channel) =>
        channel.Amplitude = _volumeLevels[Envelope.Amplitude];

    internal void Reset()
    {
        _selectedRegister = 0;

        Array.Fill(_registers, (byte)0);

        ChannelA.Reset();
        ChannelB.Reset();
        ChannelC.Reset();
        Noise.Reset();
        Envelope.Reset();
    }

    private (AmplitudeMode Mode, int Amplitude) GetAmplitude(int value)
    {
        var mode = (AmplitudeMode)((value >> 4) & 0x01);
        var amplitude = mode switch
        {
            AmplitudeMode.FixedLevel => _volumeLevels[value & 0x0F],
            _ => _volumeLevels[Envelope.Amplitude],
        };

        return (mode, amplitude);
    }

    private void SetRegister(int register, byte value) => _registers[register] = value;

    // Register port 0xFFFD is decoded as: A15=1,A14=1 & A1=0
    private static bool IsRegisterPort(Word address) => (address & 0xC002) == 0xC000;

    // Data port 0xBFFD is decoded as: A15=1 & A1=0
    private static bool IsDataPort(Word address) => (address & 0x8002) == 0x8000;
}