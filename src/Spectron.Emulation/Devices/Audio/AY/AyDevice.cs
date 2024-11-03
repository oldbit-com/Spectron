using static OldBit.Spectron.Emulation.Devices.Audio.AY.Registers;

namespace OldBit.Spectron.Emulation.Devices.Audio.AY;

internal sealed class AyDevice : IDevice
{
    private const int MaxAmplitude = 10900;

    private static readonly double[] SignalLevels = [
        0.0000, 0.0137, 0.0205, 0.0291, 0.0423, 0.0618, 0.0847, 0.1369,
        0.1691, 0.2647, 0.3527, 0.4499, 0.5704, 0.6873, 0.8482, 1.0000];

    private readonly int[] _volumeLevels = SignalLevels.Select(x => (int)(x * MaxAmplitude)).ToArray();

    internal byte CurrentRegister { get; private set; }
    internal byte[] Registers { get; } = new byte[16];

    internal Channel ChannelA { get; } = new();
    internal Channel ChannelB { get; } = new();
    internal Channel ChannelC { get; } = new();
    internal Envelope Envelope { get; }= new();
    internal Noise Noise { get; } = new();

    internal bool IsEnabled { get; set; }
    internal Action OnUpdateAudio { get; set; } = () => { };

    public void WritePort(Word address, byte value)
    {
        if (!IsEnabled)
        {
            return;
        }

        if (IsRegisterPort(address))
        {
            CurrentRegister = (byte)(value & 0x0F);
        }
        else if (IsDataPort(address))
        {
            SetRegisterValue(CurrentRegister, value);
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
            return Registers[CurrentRegister];
        }

        return null;
    }

    internal void SetAmplitudeUsingEnvelope(Channel channel) =>
        channel.Amplitude = _volumeLevels[Envelope.Amplitude];

    internal void Reset()
    {
        CurrentRegister = 0;

        Array.Fill(Registers, (byte)0);

        ChannelA.Reset();
        ChannelB.Reset();
        ChannelC.Reset();
        Noise.Reset();
        Envelope.Reset();
    }

    internal void LoadRegisters(byte currentRegister, byte[] registers)
    {
        CurrentRegister = currentRegister;

        for (var i = 0; i < registers.Length; i++)
        {
            SetRegisterValue(i, registers[i]);
        }
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

    private void SetRegisterValue(int register, byte value)
    {
        OnUpdateAudio();
        Registers[register] = value;

        switch (register)
        {
            case FineTuneA:
            case CoarseTuneA:
                ChannelA.SetPeriod(Registers[FineTuneA], Registers[CoarseTuneA]);
                break;

            case FineTuneB:
            case CoarseTuneB:
                ChannelB.SetPeriod(Registers[FineTuneB], Registers[CoarseTuneB]);
                break;

            case FineTuneC:
            case CoarseTuneC:
                ChannelC.SetPeriod(Registers[FineTuneC], Registers[CoarseTuneC]);
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
                Envelope.SetPeriod(Registers[FineTuneEnvelope], Registers[CoarseTuneEnvelope]);
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

    // Register port 0xFFFD is decoded as: A15=1,A14=1 & A1=0
    private static bool IsRegisterPort(Word address) => (address & 0xC002) == 0xC000;

    // Data port 0xBFFD is decoded as: A15=1 & A1=0
    private static bool IsDataPort(Word address) => (address & 0x8002) == 0x8000;
}